﻿using EmBrito.Dataverse.Data.Export.Models;
using EmBrito.Dataverse.DataExport.Core;
using EmBrito.Dataverse.DataExport.Schema;
using EmBrito.Dataverse.DataExport.Scripts;
using EmBrito.Dataverse.DataExport.Sql.Sql;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace EmBrito.Dataverse.DataExport
{
    public class DataExport
    {

        private readonly ILogger log;
        private readonly ApplicationOptions appOptions;
        private readonly ServiceClient client;

        public DataExport(ServiceClient client, IOptions<ApplicationOptions> applicationOptions, ILogger<DataExport> logger)
        {            
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.appOptions = applicationOptions.Value ?? throw new ArgumentNullException(nameof(appOptions));
            this.log = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Start(CancellationToken token)
        {

            if(!client.IsReady)
            {
                client.Connect();
            }            

            // Added through dependency injection
            var dbContext = new DataContext(appOptions.StoreConnectionString, appOptions.SqlCommandTimeoutSeconds);
            var settings = new DataStoreService(dbContext, log);
            var metadataService = new DataverseMetadataService(client);

            // Create database or tables if database does not exist or is empty.
            await dbContext.Database.EnsureCreatedAsync();

            // Create attribute filters
            List<Predicate<AttributeMetadata>> attributeFilters = new List<Predicate<AttributeMetadata>>();
            Predicate<AttributeMetadata> virtualAttributeFilter = (attribute) =>
                attribute.AttributeTypeName == AttributeTypeDisplayName.VirtualType;

            attributeFilters.Add(virtualAttributeFilter);

            // load the current table settings.
            var tableSettings = settings.GetEnabledTables();
            var jobId = Guid.Empty;

            // start job
            jobId = await settings.StartJob();

            // process each table in parallel

            var blockOptions = new ExecutionDataflowBlockOptions
            {
                EnsureOrdered = false,
                MaxDegreeOfParallelism = 1, // client.RecommendedDegreesOfParallelism
                CancellationToken = token
            };
            var parallelBlock = new ActionBlock<TableSetting>(async tableSetting =>
            {

                var dataContext = new DataContext(appOptions.StoreConnectionString, appOptions.SqlCommandTimeoutSeconds);
                var storeService = new DataStoreService(dataContext, log);
                (bool error, bool deleted) processSchemaResult = new();

                try
                {
                    processSchemaResult = await ProcessTableSchema(tableSetting, jobId, tableSettings, storeService, metadataService, attributeFilters);
                }
                catch (Exception ex)
                {
                    var msg = $"Unexpected error handling table schema. Table name: {tableSetting.LogicalName}. {ex.Message}";
                    log.LogError(ex, msg);

                    try
                    {
                        await settings.LogActivity(msg, tableSetting.LogicalName, jobId, isError: true);
                    }
                    catch (Exception)
                    {
                    }
                }

                if (processSchemaResult.error || processSchemaResult.deleted) return;

                try
                {
                    var dataService = new DataTransferService(client, dataContext, storeService, appOptions, log);
                    await dataService.SyncTable(tableSetting.LogicalName, jobId, token);
                }
                catch (Exception ex)
                {
                    var msg = $"Unexpected error handling table schema. Table name: {tableSetting.LogicalName}. {ex.Message}";
                    log.LogError(ex, msg);

                    try
                    {
                        await settings.LogActivity(msg, tableSetting.LogicalName, jobId, isError: true);
                    }
                    catch (Exception)
                    {
                    }
                }

            }, blockOptions);

            foreach (var tableSetting in tableSettings)
            {
                parallelBlock.Post(tableSetting);
            }

            parallelBlock.Complete();
            await parallelBlock.Completion;
            await settings.CompleteJob(jobId);
        }

        async Task<(bool error, bool deleted)> ProcessTableSchema(TableSetting tableSetting, Guid jobId, TableSettingCollection tableSettings, DataStoreService storeService, DataverseMetadataService metadataService, IEnumerable<Predicate<AttributeMetadata>> attributeFilters)
        {

            log.LogInformation($"Processing table {tableSetting.LogicalName}.");

            var metadata = await metadataService.GetMetadata(tableSetting.LogicalName);
            TableDefinition? remoteDefinition = null;
            var error = false;

            if (metadata is null)
            {
                log.LogInformation($"Metadata not found for remote table {tableSetting.LogicalName}.");
            }
            else
            {
                log.LogTrace($"Metadata found. Instantiating table definition builder for {tableSetting.LogicalName}.");
                var builder = TableDefinitionBuilder.WithMetadata(metadata);

                if (!string.IsNullOrWhiteSpace(appOptions.TableNamePrefix)) builder.SetNamePrefix(appOptions.TableNamePrefix);
                if (!string.IsNullOrWhiteSpace(appOptions.TableNameSufix)) builder.SetNameSufix(appOptions.TableNameSufix);

                if (attributeFilters != null)
                {
                    attributeFilters.ToList().ForEach(f => builder.ExcludeAttributes(f));
                }

                remoteDefinition = builder.BuildDefinitions();

                if(appOptions.EnableIdColumn)
                {
                    remoteDefinition.Columns.AddAndSort(ColumnDefinitionFactory.CreateUniqueIdentifier("Id", indexed: true));
                }                

                log.LogTrace($"Table definition built for {tableSetting.LogicalName}. Total columns: {remoteDefinition.Columns.Count}.");
            }

            var localDefinition = tableSettings
                .Where(s => !string.IsNullOrWhiteSpace(s.TableSchema) && s.LogicalName == tableSetting.LogicalName)
                .Select(s => TableDefinition.FromJson(s.TableSchema!))
                .ToList();

            var localDefinitions = new TableDefinitionCollection(localDefinition);
            var dataverseDefinitions = new TableDefinitionCollection();

            if (remoteDefinition != null)
            {
                dataverseDefinitions.Add(remoteDefinition);
            }

            log.LogTrace($"Determining changes for table {tableSetting.LogicalName}");
            var changes = new SchemaComparer(localDefinitions, dataverseDefinitions);

            // apply schema changes
            if (changes.HasChanges)
            {
                log.LogInformation($"Table {tableSetting.LogicalName} has changed.");
                var failedTables = await ApplySchemaChanges(changes, jobId, tableSetting, storeService);
                error = failedTables.Count > 0;
            }
            else
            {
                log.LogInformation($"No Changes found for table {tableSetting.LogicalName}.");
                await storeService.LogActivity($"No schema changes. Table name: {tableSetting.LogicalName}", tableSetting.LogicalName, jobId, isError: false);
            }

            // check and enable change tracking
            if (metadata != null)
            {
                try
                {
                    await EnsureChangeTrackEnabled(metadata, metadataService, localDefinitions, storeService);
                }
                catch (Exception ex)
                {
                    error = true;
                    var msg = $"Unexpected error updating change tracking for table {tableSetting.LogicalName}.";
                    log.LogError(ex, msg);

                    try
                    {
                        await storeService.LogActivity(msg, tableSetting.LogicalName, jobId, isError: true);
                    }
                    catch (Exception)
                    {
                    }
                }

            }

            return (error: error, deleted: changes.DeletedTables.Any());
        }

        async Task<List<string>> ApplySchemaChanges(SchemaComparer changes, Guid jobId, TableSetting tableSetting, DataStoreService storeService)
        {
            var builder = new SqlScriptBuilder();
            var failedTables = new List<string>();

            foreach (var table in changes.CreatedTables)
            {
                try
                {
                    var script = builder.CreateTable(table);
                    await storeService.ExecuteSchemaUpdateScript(script, jobId);
                }
                catch (Exception ex)
                {
                    failedTables.Add(table.Name);
                    var msg = $"Unable to generate create table script for table {table.Name}";
                    log.LogError(ex, msg);
                    await LogScriptGenerationError(msg, table.Name, jobId, storeService);
                }
            }

            foreach (var table in changes.DeletedTables)
            {

                try
                {
                    var script = builder.DropTable(table);
                    await storeService.ExecuteSchemaUpdateScript(script, jobId);
                }
                catch (Exception ex)
                {
                    failedTables.Add(table.Name);
                    var msg = $"Unable to generate drop table script for table {table.Name}";
                    log.LogError(ex, msg);
                    await LogScriptGenerationError(msg, table.Name, jobId, storeService);
                }

            }

            foreach (var change in changes.SchemaChanges)
            {
                try
                {
                    var script = builder.AlterTable(change);
                    await storeService.ExecuteSchemaUpdateScript(script, jobId);
                }
                catch (Exception ex)
                {
                    failedTables.Add(change.LogicalName);
                    var msg = $"Unable to generate alter table script for table {change.LogicalName}";
                    log.LogError(ex, msg);
                    await LogScriptGenerationError(msg, change.LogicalName, jobId, storeService);
                }
            }

            return failedTables;
        }

        async Task LogScriptGenerationError(string message, string tableName, Guid jobId, DataStoreService storeService)
        {
            await storeService.LogActivity(message, tableName, jobId, isError: true);
        }

        async Task EnsureChangeTrackEnabled(EntityMetadata metadata, DataverseMetadataService metadataService, TableDefinitionCollection localDefinitions, DataStoreService storeService)
        {
            log.LogInformation($"Checking change tracking status for table {metadata.LogicalName}.");
            var tableName = metadata.LogicalName;

            if (!metadata.ChangeTrackingEnabled.HasValue || !metadata.ChangeTrackingEnabled.Value)
            {
                log.LogInformation($"Enabling change tracking on table {metadata.LogicalName}.");
                await metadataService.EnableChangeTracking(metadata);
            }
            else
            {
                log.LogInformation($"Change tracking already enabled for table : {metadata.LogicalName}");
            }

            if (metadata.ChangeTrackingEnabled.HasValue && metadata.ChangeTrackingEnabled.Value)
            {
                if (localDefinitions.Contains(tableName) && !localDefinitions[tableName].ChangeTrackingEnabled)
                {
                    log.LogInformation($"Updating local settings for table {metadata.LogicalName} with new change tracking status. Enabled: {metadata.ChangeTrackingEnabled.Value}.");
                    await storeService.SetChangeTrackingEnabled(tableName);
                }
            }
        }

    }
}
