using EmBrito.Dataverse.DataExport.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Scripts
{
    public class SqlScriptBuilder
    {

        public const string ScriptTypeCreateTable = "Create";
        public const string ScriptTypeAlterTable = "Alter";
        public const string ScriptTypeDropTable = "Drop";

        public SqlScript CreateTable(TableDefinition tableDefinition, string? alternativeName = null, bool stagingTable = false)
        {
            var builder = new StringBuilder();

            string tableName = alternativeName ?? tableDefinition.Name;

            // open create table statment
            builder.AppendLine("SET ANSI_NULLS ON");
            builder.AppendLine("SET QUOTED_IDENTIFIER ON");
            builder.AppendLine($"create table [dbo].[{tableName}](");

            // append columns

            foreach (var col in tableDefinition.Columns.ToList())
            {
                builder.Append($" {ColumnToSqlDefinition(col, tableDefinition)}");
                builder.AppendLine(",");
            }

            if(!stagingTable)
            {
                // append primary 
                builder.AppendLine($"constraint [PK_{tableName}] primary key clustered");
                builder.AppendLine("  (");
                builder.Append($"    [{tableDefinition.PrimaryIdAttribute}] ASC");
                builder.AppendLine();
                builder.AppendLine("  ) with (pad_index = off, statistics_norecompute = off, ignore_dup_key = off, allow_row_locks = on, allow_page_locks = on)");
            }

            // close create statment
            builder.AppendLine(")");

            if(!stagingTable)
            {
                // append additional indexes
                foreach (var col in tableDefinition.Columns.Where(c => c.Indexed).ToList())
                {
                    builder.AppendLine($" create nonclustered index IX_{tableName}_{col.Name} on {tableName} (");
                    builder.AppendLine($"   [{col.Name}] ASC");
                    builder.AppendLine($" )");
                }
            }

            var desc = $"SQL script: Create table. Table name: {tableName}.";
            return new SqlScript(ScriptTypeCreateTable, builder.ToString(), tableDefinition, desc);
        }

        public SqlScript DropTable(TableDefinition tableDefinition)
        {
            var desc = $"SQL script: Drop table. Table name: {tableDefinition.Name}.";
            return new SqlScript(ScriptTypeDropTable, $"drop table {tableDefinition.Name}", tableDefinition, desc);
        }

        public SqlScript AlterTable(TableSchemaChanges changes)
        {

            var builder = new StringBuilder();
            var description = new List<string>();

            foreach (var col in changes.NewColumns)
            {
                builder.AppendLine($"alter table [dbo].[{changes.TableDefinition.Name}] add {ColumnToSqlDefinition(col, changes.TableDefinition)};");
                description.Add($"SQL script: Alter table. New column. Table name: {changes.TableDefinition.Name}. Column: {col.Name}.");
            }

            foreach (var col in changes.DeletedColumns)
            {
                builder.AppendLine($"alter table [dbo].[{changes.TableDefinition.Name}] drop column [{col.Name}];");
                description.Add($"SQL script: Alter table. Column dropped. Table name: {changes.TableDefinition.Name}. Column: {col.Name}.");
            }

            foreach (var col in changes.UpdatedColumns)
            {
                builder.AppendLine($"alter table [dbo].[{changes.TableDefinition.Name}] alter column {ColumnToSqlDefinition(col, changes.TableDefinition)};");
                description.Add($"SQL script: Alter table. Column updated. Table name: {changes.TableDefinition.Name}. Column: {col.Name}.");
            }

            return new SqlScript(ScriptTypeAlterTable, builder.ToString(), changes.TableDefinition, description);
        }

        public string ColumnToSqlDefinition(ColumnDefinition column, TableDefinition tableDefinition)
        {
            string nullableColumn = tableDefinition.PrimaryIdAttribute.Equals(column.Name) ? String.Empty : "NULL";
            string sqlDefinition;

            switch (column.TypeName)
            {
                case "bigint":
                    sqlDefinition = $"[{column.Name}] [bigint] {nullableColumn}";
                    break;
                case "bit":
                    sqlDefinition = $"[{column.Name}] [bit] {nullableColumn}";
                    break;
                case "datetime":
                    sqlDefinition = $"[{column.Name}] [datetime] {nullableColumn}";
                    break;
                case "datetime2":
                    sqlDefinition = $"[{column.Name}] [datetime2]({column.Scale}) {nullableColumn}";
                    break;
                case "decimal":
                    sqlDefinition = $"[{column.Name}] [decimal](38, {column.Scale}) {nullableColumn}";
                    break;
                case "float":
                    sqlDefinition = $"[{column.Name}] [float] {nullableColumn}";
                    break;
                case "int":
                    sqlDefinition = $"[{column.Name}] [int] {nullableColumn}";
                    break;
                case "money":
                    sqlDefinition = $"[{column.Name}] [money] {nullableColumn}";
                    break;
                case "nvarchar":
                    sqlDefinition = $"[{column.Name}] [nvarchar]({(column.MaxLength == -1 ? "MAX" : column.MaxLength / 2)}) {nullableColumn}";
                    break;
                case "uniqueidentifier":
                    sqlDefinition = $"[{column.Name}] [uniqueidentifier] {nullableColumn}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("ColumnTypeName", $"Unexpected column type name: {column.TypeName}");
            }

            return sqlDefinition;
        }
    }
}
