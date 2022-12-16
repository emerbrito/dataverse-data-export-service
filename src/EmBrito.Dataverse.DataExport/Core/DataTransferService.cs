using EmBrito.Dataverse.Data.Export.Models;
using EmBrito.Dataverse.DataExport.Schema;
using EmBrito.Dataverse.DataExport.Scripts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmBrito.Dataverse.DataExport.Models;
using Polly;
using Polly.Contrib.WaitAndRetry;
using EmBrito.Dataverse.DataExport.Sql.Sql;
using EmBrito.Dataverse.DataExport.Sql.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EmBrito.Dataverse.DataExport.Core
{
    public class DataTransferService
    {

        #region Private declaration

        IOrganizationServiceAsync2 client;
        ILogger log;
        DataContext dbContext;
        DataStoreService storeService;

        #endregion

        #region Constructors

        public DataTransferService(IOrganizationServiceAsync2 client, DataContext dbContext, DataStoreService storeService, ILogger logger)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.log = logger ?? throw new ArgumentNullException(nameof(logger)); ;
            this.storeService = storeService ?? throw new ArgumentNullException(nameof(storeService)); ;
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext)); ;
        }

        #endregion

        #region Public Methods

        public async Task<bool> SyncTable(string logicalName, Guid jobId, CancellationToken cancelToken)
        {
            log.LogInformation($"Synchronization started for table {logicalName}.");
            DateTime startDate = DateUtility.LocalNow;

            int newOrUpdatedCount = 0;
            int removedCount = 0;
            bool success = false;
            string responseToken = string.Empty;
            IDbContextTransaction transaction = null;
            TableSetting tableSetting;
            DataTable newOrUpdated;
            DataTable removed;            

            try
            {

                tableSetting = await storeService.GetTableSettingsByName(logicalName);
                var tableDefinition = TableDefinition.FromJson(tableSetting!.TableSchema);

                if (!cancelToken.IsCancellationRequested)
                {

                    using (SqlConnection conn = new(dbContext.Database.GetConnectionString()))
                    {
                        conn.Open();
                        log.LogTrace($"Building empty data tables.");
                        newOrUpdated = BuildEmptyDataTable(tableDefinition.Name, conn);
                        removed = BuildEmtptyDeletedRecordsTable();

                        log.LogTrace($"Dropping temp tables if already exist.");
                        DropStagingTablesByTableName(tableDefinition.Name, conn);
                        CreateStagingTables(tableDefinition, conn);

                        log.LogInformation($"Populating staging tables for {logicalName}.");
                        foreach (var changes in RetrieveAllChanges(logicalName, tableSetting.DataToken, cancelToken))
                        {
                            if (changes != null)
                            {
                                responseToken = changes.Token;
                                newOrUpdatedCount += changes.NewOrUpdated.Count();
                                removedCount += changes.Removed.Count();
                                FillDataTable(newOrUpdated, changes.NewOrUpdated, tableDefinition);
                                FillDeleteDataTable(removed, changes.Removed.Select(e =>
                                {
                                    var entity = new Entity(e.LogicalName, e.Id);
                                    entity.Attributes.Add(tableDefinition.PrimaryIdAttribute, e.Id);
                                    return entity;
                                }));

                                BulkAddToTempTables(tableDefinition, newOrUpdated, removed, conn, cancelToken);
                            }
                        }

                        log.LogInformation($"Done transferring data to temp table.");

                        if (!cancelToken.IsCancellationRequested)
                        {

                            var strategy = dbContext.Database.CreateExecutionStrategy();
                            await strategy.ExecuteAsync(async () => 
                            {

                                using (var trans = await dbContext.Database.BeginTransactionAsync())
                                {

                                    log.LogInformation($"Bulk upsert and remove from temp tables stated.");
                                    if (BulkUpsert(tableDefinition, cancelToken))
                                    {

                                        log.LogInformation($"Bulk upsert complete. Writing logs.");

                                        if (!cancelToken.IsCancellationRequested)
                                        {

                                            await storeService.LogActivity($"Synchronization complete for table {tableSetting.LogicalName}", tableSetting.LogicalName, jobId, isError: false);

                                            var tableLog = new SynchronizationLog
                                            {
                                                CompletedOn = DateUtility.LocalNow,
                                                NewOrUpdated = newOrUpdatedCount,
                                                Removed = removedCount,
                                                RequestDeltaToken = tableSetting.DataToken,
                                                ResponseDeltaToken = responseToken,
                                                StartedOn = startDate,
                                                SynchronizedTableId = tableSetting.Id,
                                                SynchronizationJobId = jobId
                                            };

                                            dbContext.SynchronizationLogs.Add(tableLog);
                                            await storeService.SetDataToken(tableSetting.LogicalName, responseToken);
                                            dbContext.SaveChanges();

                                            log.LogInformation($"Commiting transaction");
                                            trans.Commit();
                                            success = true;
                                            log.LogInformation($"Data synchronization complete. Table: {tableSetting.LogicalName}. Job id: {jobId}");
                                        }
                                    }

                                }

                            });

                        }

                    }

                }

                success = true;

            }
            catch (Exception ex)
            {

                if (transaction != null) transaction.Rollback();
                var errorMessage = CombineExceptionsMessage(logicalName, jobId, ex);
                //TODO: Maybe add telemtry indicating sync error.
                log.LogError(ex, errorMessage);

                try
                {
                    await storeService.LogActivity(errorMessage, logicalName, jobId, isError: true);

                }
                catch (Exception)
                {
                }

            }

            return success;
        }

        #endregion

        #region Internal Implementation

        DataTable BuildEmptyDataTable(string tableName, SqlConnection conn)
        {
            DataTable dtResult = new DataTable(tableName);

            using (SqlCommand command = conn.CreateCommand())
            {
                command.CommandText = String.Format("SELECT TOP 1 * FROM {0}", tableName);
                command.CommandType = CommandType.Text;

                SqlDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly);
                dtResult.Load(reader);
            }

            return dtResult;
        }

        DataTable BuildEmtptyDeletedRecordsTable()
        {
            var Table = new DataTable("DeletedRecords");
            Table.Columns.Add(new DataColumn
            {
                AllowDBNull = true,
                DataType = typeof(Guid),
                ColumnName = "Id"
            });

            return Table;
        }

        void BulkAddToTempTables(TableDefinition tableDefinition, DataTable newOrUpdated, DataTable removed, SqlConnection conn, CancellationToken cancelToken)
        {

            var tempDeleteTable = $"##Tmp{tableDefinition.Name}_Del";

            using (SqlCommand command = new("", conn))
            {

                if (cancelToken.IsCancellationRequested) return;

                //Bulk insert into temp table
                if (newOrUpdated.Rows.Count > 0)
                {
                    ExecuteBulkCopy(newOrUpdated, StagingTableName(tableDefinition.Name), conn);
                }

                if (cancelToken.IsCancellationRequested) return;

                //Bulk insert into temp delete table
                if (removed.Rows.Count > 0)
                {
                    ExecuteBulkCopy(removed, StagingTableNameRemovedRecords(tableDefinition.Name), conn);
                }

            }

        }

        bool BulkUpsert(TableDefinition tableDefinition, CancellationToken cancelToken)
        {

            var tempTable = StagingTableName(tableDefinition.Name);
            var tempDeleteTable = StagingTableNameRemovedRecords(tableDefinition.Name);
            var primaryKeys = tableDefinition
                .Columns
                .Where(c => c.Name == tableDefinition.PrimaryIdAttribute)
                .ToList();

            if (cancelToken.IsCancellationRequested) return false;

            // delete records
            dbContext.Database.ExecuteSqlRaw($"DELETE {tableDefinition.Name} FROM {tableDefinition.Name} INNER JOIN [{tempDeleteTable}] t ON {tableDefinition.Name}.{tableDefinition.PrimaryIdAttribute} = t.Id;");

            //next query support
            var updateSetColumns = BuildColumnEqualsColumn(tableDefinition.Columns.ToList(), tableDefinition.Name, "t", ", ");
            var joinColumns = BuildColumnEqualsColumn(primaryKeys, tableDefinition.Name, "t", " and ");

            if (cancelToken.IsCancellationRequested) return false;

            //Updating existing records in destination table
            dbContext.Database.ExecuteSqlRaw($"UPDATE {tableDefinition.Name} SET {updateSetColumns} FROM {tableDefinition.Name} INNER JOIN [{tempTable}] t ON {joinColumns};");

            //next query support
            var namelistTarget = string.Join(", ", tableDefinition.Columns.Select(c => c.Name).ToArray());
            var namelistSource = string.Join(", ", tableDefinition.Columns.Select(c => $"t.{c.Name}").ToArray());
            joinColumns = BuildColumnEqualsColumn(primaryKeys, "t", tableDefinition.Name, " and ");

            if (cancelToken.IsCancellationRequested) return false;

            //inserting new records in destination table
            dbContext.Database.ExecuteSqlRaw($"INSERT INTO {tableDefinition.Name} ({namelistTarget}) SELECT {namelistSource} FROM [{tempTable}] t LEFT JOIN {tableDefinition.Name} ON {joinColumns} WHERE {tableDefinition.Name}.{tableDefinition.PrimaryIdAttribute} IS NULL;");

            //dropping temp tables
            dbContext.Database.ExecuteSqlRaw($"DROP TABLE [{tempTable}];");
            dbContext.Database.ExecuteSqlRaw($"DROP TABLE [{tempDeleteTable}];");

            return true;
        }

        string CombineExceptionsMessage(string tableName, Guid jobId, Exception ex)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Error synchronizing Table: {tableName}. Job ID: {jobId}");
            builder.AppendLine($"Exception: {ex.GetType().FullName}. Message: {ex.Message}");

            var inner = ex.InnerException;

            while (inner != null)
            {
                builder.AppendLine($"Inner Exception: {inner.GetType().FullName}. Message: {inner.Message}");
                inner = inner.InnerException;
            }

            return builder.ToString();
        }

        void CreateStagingTables(TableDefinition tableDefinition, SqlConnection conn)
        {
            var builder = new SqlScriptBuilder();

            using (SqlCommand command = new("", conn))
            {

                //Creating temp tables
                command.CommandText = builder.CreateTable(tableDefinition, alternativeName: StagingTableName(tableDefinition.Name), stagingTable: true).Script;
                command.ExecuteNonQuery();

                command.CommandText = $"CREATE TABLE {StagingTableNameRemovedRecords(tableDefinition.Name)} ([Id] [uniqueidentifier] NULL);";
                command.ExecuteNonQuery();

            }
        }

        void DropStagingTablesByTableName(string tableName, SqlConnection conn)
        {

            using (SqlCommand command = new("", conn))
            {

                //drop temp table if exist
                command.CommandText = $"DROP TABLE IF EXISTS [{StagingTableName(tableName)}] ";
                command.ExecuteNonQuery();
                command.CommandText = $"DROP TABLE IF EXISTS [{StagingTableNameRemovedRecords(tableName)}]";
                command.ExecuteNonQuery();

            }

        }

        void ExecuteBulkCopy(DataTable dataTable, string tableName, SqlConnection conn)
        {

            using (SqlBulkCopy bulkcopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.TableLock, null))
            {
                //TODO: configurable bulk copy time out
                bulkcopy.BulkCopyTimeout = 3600;
                bulkcopy.DestinationTableName = tableName;
                bulkcopy.BatchSize = dataTable.Rows.Count;

                foreach (DataColumn column in dataTable.Columns)
                {
                    bulkcopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName));
                }

                bulkcopy.WriteToServer(dataTable);
                bulkcopy.Close();
            }
        }

        void FillDataTable(DataTable dataTable, IEnumerable<Entity> entities, TableDefinition tableDefinition)
        {
            dataTable.Clear();

            foreach (var entity in entities)
            {
                var row = dataTable.NewRow();

                foreach (DataColumn col in dataTable.Columns)
                {

                    if (row[col.ColumnName] != null && row[col.ColumnName] != DBNull.Value)
                    {
                        continue;
                    }

                    object value = null;

                    if (entity.Contains(col.ColumnName))
                    {
                        value = entity[col.ColumnName];
                    }

                    row[col.ColumnName] = EntityValueConverter.ToPrimitiveValue(value) ?? DBNull.Value;

                    // If attribute is of type entity reference, look for corresponding attribute with format: [attribute_name]idtype
                    // this will apply to relationships where the id could be from different entities such as customer (account and contact)
                    // or owner (user or team).
                    // the standard is alwais ending in "idtype", regardless of whether the dolumn name ends with "id".
                    // eg: mycusomerid = mycustomeridtype, onwer = owneridtype
                    var entityTypeNameCol = ToEntityNameTypeColumnName(col.ColumnName);
                    if (dataTable.Columns.Contains(entityTypeNameCol) && value != null && value != DBNull.Value && value is EntityReference)
                    {
                        row[entityTypeNameCol] = ((EntityReference)value).LogicalName;
                    }

                }

                if (dataTable.Columns.Contains("Id") && (!string.IsNullOrWhiteSpace(tableDefinition.PrimaryIdAttribute) && entity.Contains(tableDefinition.PrimaryIdAttribute)))
                {
                    row.SetField<Guid>("Id", entity.Id);
                }

                dataTable.Rows.Add(row);
            }
        }

        void FillDeleteDataTable(DataTable dataTable, IEnumerable<Entity> entities)
        {
            dataTable.Clear();

            foreach (var entity in entities)
            {
                if (!dataTable.Columns.Contains("Id") || entity.Id == Guid.Empty)
                {
                    continue;
                }

                var row = dataTable.NewRow();
                row["Id"] = entity.Id;
                dataTable.Rows.Add(row);
            }
        }

        IEnumerable<EntityChanges> RetrieveAllChanges(string logicalName, string? requestToken, CancellationToken cancelToken)
        {
            log.LogInformation($"Retrieving all changes for entity {logicalName}");
            string responseToken = string.Empty;
            var upserts = new List<Entity>();
            var removed = new List<EntityReference>();
            //TODO: Make page size configurable
            var req = new RetrieveEntityChangesRequest
            {
                Columns = new ColumnSet(true),
                DataVersion = requestToken,
                EntityName = logicalName,
                PageInfo = new PagingInfo
                {
                    Count = 2000,
                    PageNumber = 1,
                    ReturnTotalRecordCount = false
                }
            };
            var pageCount = 0;
            //TODO: Make linear back off initial delay and retry count configurable
            var delay = Backoff.LinearBackoff(TimeSpan.FromMilliseconds(10000), retryCount: 6);

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetry(delay);

            var moreRecords = true;

            while (moreRecords)
            {

                pageCount++;
                RetrieveEntityChangesResponse resp = null;
                upserts = new List<Entity>();
                removed = new List<EntityReference>();

                retryPolicy.Execute(() => resp = (RetrieveEntityChangesResponse)client.Execute(req));

                upserts.AddRange(resp.EntityChanges.Changes.Select(x => (x as NewOrUpdatedItem)?.NewOrUpdatedEntity).Where(e => e != null).ToList());
                removed.AddRange(resp.EntityChanges.Changes.Select(x => (x as RemovedOrDeletedItem)?.RemovedItem).Where(e => e != null).ToList());

                req.PageInfo.PageNumber++;
                req.PageInfo.PagingCookie = resp.EntityChanges.PagingCookie;
                responseToken = resp.EntityChanges.DataToken;

                if (cancelToken.IsCancellationRequested || !resp.EntityChanges.MoreRecords)
                {
                    moreRecords = false;
                }

                yield return new EntityChanges(responseToken, upserts, removed);
            }

            log.LogInformation($"Retrieve entity {logicalName} complete - {pageCount} pages. {upserts.Count} new or updated record(s). {removed.Count} removed record(s).");
        }

        public string BuildColumnEqualsColumn(List<ColumnDefinition> columns, string leftPrefix, string rightPrefix, string separator)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < columns.Count; i++)
            {
                var col = columns[i];
                builder.Append($"{leftPrefix}.{col.Name} = {rightPrefix}.{col.Name}");

                if (i < columns.Count - 1)
                {
                    builder.Append(separator);
                }
            }

            return builder.ToString();
        }

        string StagingTableName(string tableName)
        {
            return $"##Tmp{tableName}";
        }

        string StagingTableNameRemovedRecords(string tableName)
        {
            return $"##Tmp{tableName}_Del";
        }

        string ToEntityNameTypeColumnName(string logicalName)
        {
            string name;

            if (logicalName.EndsWith("id", StringComparison.InvariantCultureIgnoreCase))
            {
                name = $"{(logicalName.Length == 2 ? logicalName : logicalName[..^2])}idtype";
            }
            else
            {
                name = $"{logicalName}idtype";
            }

            return name;
        }

        #endregion

    }
}
