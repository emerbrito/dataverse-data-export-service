using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Schema
{
    public class SchemaComparer
    {

        readonly TableDefinitionCollection local;
        readonly TableDefinitionCollection remote;

        public bool HasChanges { get => 
                CreatedTables.Any() || 
                DeletedTables.Any() || 
                SchemaChanges.Any();  }

        public IEnumerable<TableDefinition> CreatedTables { get; set; }
        public IEnumerable<TableDefinition> DeletedTables { get; set; }
        public IEnumerable<TableSchemaChanges> SchemaChanges { get; set; }

        public SchemaComparer(TableDefinitionCollection localDefinitions, TableDefinitionCollection remoteDefinitions)
        {
            local = localDefinitions;
            remote = remoteDefinitions;
            CreatedTables = Enumerable.Empty<TableDefinition>();
            DeletedTables = Enumerable.Empty<TableDefinition>();
            SchemaChanges = Enumerable.Empty<TableSchemaChanges>();
            LoadChanges();
        }

        void LoadChanges()
        {
            CreatedTables = remote.Except(local, new TableNameComparer());
            DeletedTables = local.Except(remote, new TableNameComparer());
            LoadTableSchemaChanges();
        }

        void LoadTableSchemaChanges()
        {
            var changes = new List<TableSchemaChanges>();
            var intersectingNames = local
                .Intersect(remote, new TableNameComparer()).Select(t => t.Name)
                .ToArray();

            var existingTablesLocal = local
                .Where(t => intersectingNames.Contains(t.Name))
                .ToList();

            foreach (var localTable in existingTablesLocal)
            {
                var remoteTable = remote.First(t => t.Name == localTable.Name);
                var tableChanges = GetSchemaChanges(localTable, remoteTable);

                if(tableChanges.HasChanges)
                {
                    changes.Add(tableChanges);
                }
            }

            SchemaChanges = changes;
        }

        TableSchemaChanges GetSchemaChanges(TableDefinition localTable, TableDefinition remoteTable)
        {
            var newColumns = remoteTable
                .Columns
                .Except(localTable.Columns, new ColumnNameComparer())
                .ToList();

            var deleted = localTable
                .Columns
                .Except(remoteTable.Columns, new ColumnNameComparer())
                .ToList();

            var modified = GetModifiedColumns(localTable, remoteTable);
            var changes = new TableSchemaChanges(remoteTable);

            changes.AddColumns(newColumns);
            changes.UpdateColumns(modified);
            changes.DeleteColumns(deleted);

            return changes;
        }

        IEnumerable<ColumnDefinition> GetModifiedColumns(TableDefinition localTable, TableDefinition remoteTable)
        {

            var n = new HashSet<string>();
            var commonColumnNames = localTable
                .Columns
                .Select(c => c.Name)
                .Intersect(remoteTable.Columns.Select(c => c.Name).ToList())
                .ToHashSet();

            var modified = remoteTable
                .Columns
                .Where(c => commonColumnNames.Contains(c.Name))
                .Except(
                    localTable.Columns.Where(c => commonColumnNames.Contains(c.Name)),
                    new ColumnComparer())
                .ToList();

            return modified;
        }

    }
}
