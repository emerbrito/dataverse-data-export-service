using EmBrito.Dataverse.Data.Export.Models;
using EmBrito.Dataverse.DataExport.Schema;
using EmBrito.Dataverse.DataExport.Scripts;
using EmBrito.Dataverse.DataExport.Sql.Models;
using EmBrito.Dataverse.DataExport.Sql.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Core
{
    public class DataStoreService
    {

        DataContext dataContext;
        ILogger logger;

        public DataStoreService(DataContext dataContext, ILogger logger)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));            
        }

        public async Task LogActivity(string description, string tableName, Guid jobId, bool isError)
        {
            var table = await GetTableByName(tableName);
            var job = await GetJobById(jobId);

            if(job != null && table != null) 
            {
                await dataContext.ActivityLogs.AddAsync(BuildActivityLog(description, table, job, isError));
                await dataContext.SaveChangesAsync();
            }

        }

        public async Task<Guid> StartJob()
        {
            var job = new SynchronizationJob
            { 
                StartedOn = DateUtility.LocalNow
            };

            await dataContext.SynchronizationJobs.AddAsync(job);
            await dataContext.SaveChangesAsync();
            return job.Id;
        }

        public async Task CompleteJob(Guid jobId)
        {
            var job = await GetJobById(jobId);

            job.CompletedOn = DateUtility.LocalNow;
            await dataContext.SaveChangesAsync();
        }

        public async Task<TableSetting> SetChangeTrackingEnabled(string tableName)
        {
            var table = await GetTableByName(tableName);

            if(table is null)
            {
                throw new KeyNotFoundException($"Unable to find table {tableName}");
            }

            if(string.IsNullOrWhiteSpace(table.TableSchema))
            {
                throw new KeyNotFoundException($"Cannot mark change tracking as enable. Table schema is empty.");
            }

            var schema = TableDefinition.FromJson(table.TableSchema);
            
            schema.ChangeTrackingEnabled = true;
            table.TableSchema = schema.ToJson();
            await dataContext.SaveChangesAsync();

            var updatedTableSetting = dataContext
                .SynchronizedTables
                .Where(x => x.LogicalName == tableName)
                .Select(SynchronizedTableToTableSetting)
                .FirstOrDefault();

            return updatedTableSetting;
        }

        public async Task ExecuteSchemaUpdateScript(SqlScript script, Guid jobId)
        {

            var startedOn = DateUtility.LocalNow;
            SynchronizedTable? table = null;
            SynchronizationJob? job = null;

            try
            {

                var strategy = dataContext.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () => 
                {

                    table = await GetTableByName(script.TableName, throwIfNotFound: true);
                    job = await GetJobById(jobId);

                    using (var trans = await dataContext.Database.BeginTransactionAsync())
                    {

                        await dataContext.Database.ExecuteSqlRawAsync(script.Script);
                        script.Description.ToList().ForEach(async s =>
                        {
                            await dataContext.ActivityLogs.AddAsync(BuildActivityLog(s, table!, job, false));
                        });

                        switch (script.Type)
                        {
                            case SqlScriptBuilder.ScriptTypeAlterTable:
                            case SqlScriptBuilder.ScriptTypeCreateTable:
                                table!.TableSchema = script.TableDefinition.ToJson();
                                break;
                            case SqlScriptBuilder.ScriptTypeDropTable:
                                table!.DataToken = null;
                                table.Enabled = false;                                
                                table.TableSchema = null;
                                break;
                        }

                        await dataContext.SaveChangesAsync();
                        await trans.CommitAsync();
                    }

                });

            }
            catch (Exception ex)
            {
                //TODO: notify error? perhaps log as a custom event on application insights.
                logger.LogError(ex, $"Error executing raw sql script. {ex.Message}");                
                try
                {
                    if (table != null && job != null)
                    {
                        var description = script.Description.Count() == 1
                            ? script.Description.First()
                            : $"Script type {script.Type}. Table name: {script.TableDefinition.Name}.";
                        var message = $"Error executing raw sql. {description} {ex.Message}";
                        await dataContext.ActivityLogs.AddAsync(BuildActivityLog(message, table!, job, isError: true));
                        await dataContext.SaveChangesAsync();
                    }
                }
                catch (Exception)
                {
                }

            }

        }

        public TableSettingCollection GetEnabledTables()
        {
            var tables = dataContext
                .SynchronizedTables
                .Where(x => x.Enabled == true)
                .Select(SynchronizedTableToTableSetting)
                .ToList();

            var collection = new TableSettingCollection();
            tables.ForEach(x => collection.Add(x));

            return collection;
        }

        ActivityLog BuildActivityLog(string description, SynchronizedTable table, SynchronizationJob job, bool isError)
        {
            return new ActivityLog
            {
                CreatedOn = DateUtility.LocalNow,
                Description = description,
                Error = isError,
                SynchronizationJob = job,
                SynchronizedTable = table
            };
        }

        async Task<SynchronizationJob> GetJobById(Guid jobId)
        {
            var job = await dataContext
                .SynchronizationJobs
                .Where(x => x.Id == jobId)
                .FirstOrDefaultAsync();

            if (job is null)
            {
                throw new KeyNotFoundException($"Job with ID {jobId} could not be found.");
            }

            return job;
        }

        async Task<SynchronizedTable?> GetTableByName(string tableName, bool throwIfNotFound = false)
        {
            var table = await dataContext
                .SynchronizedTables
                .Where(x => x.LogicalName == tableName)
                .FirstOrDefaultAsync();

            if (table is null && throwIfNotFound)
            {
                throw new KeyNotFoundException($"Table {tableName} could not be found.");
            }

            return table;
        }

        TableSetting SynchronizedTableToTableSetting(SynchronizedTable table)
        {
            return new TableSetting
            {
                DataToken = table.DataToken,
                Enabled = table.Enabled,
                Id = table.Id,
                LogicalName = table.LogicalName,
                TableSchema = table.TableSchema
            };
        }

    }
}
