using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.Data.Export.Schema
{
    public class TableSchemaChanges
    {

        TableDefinition originalDefinition;
        List<ColumnDefinition> newColumns;
        List<ColumnDefinition> updatedColumns;
        List<ColumnDefinition> deletedColumns;

        public string LogicalName { get; init; }

        public IEnumerable<ColumnDefinition> NewColumns { get => newColumns; }
        public IEnumerable<ColumnDefinition> UpdatedColumns { get => updatedColumns; }
        public IEnumerable<ColumnDefinition> DeletedColumns { get => deletedColumns; }

        public TableSchemaChanges(TableDefinition tableDefinition)
        {
            originalDefinition = tableDefinition;
            LogicalName = tableDefinition.Name;
            newColumns = new();
            deletedColumns = new();
            updatedColumns = new();
        }

        public bool HasChanges { get => newColumns.Count > 0 || updatedColumns.Count > 0 || deletedColumns.Count > 0;  }

        public void AddColumns(IEnumerable<ColumnDefinition> columns)
        {
            newColumns.AddRange(columns);
        }

        public void UpdateColumns(IEnumerable<ColumnDefinition> columns)
        {
            updatedColumns.AddRange(columns);
        }

        public void DeleteColumns(IEnumerable<ColumnDefinition> columns)
        {
            deletedColumns.AddRange(columns);
        }
    }
}
