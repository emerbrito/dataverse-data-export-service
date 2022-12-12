using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Schema
{
    public class TableDefinition
    {

        [JsonInclude]
        public string Name { get; init; }
        [JsonInclude]
        public string PrimaryIdAttribute { get; init; }
        [JsonInclude]
        public bool ChangeTrackingEnabled { get; internal set; }
        [JsonInclude]
        public ColumnDefinitionCollection Columns { get; init; }

        public TableDefinition()
        {
            // this constructtor is required by Syste.Text.Json for deserialization
            Name = String.Empty;
            PrimaryIdAttribute = String.Empty;
            Columns = new ColumnDefinitionCollection();
        }

        internal TableDefinition(string name, string primaryIdAttribute, bool changeTrackingEnabled, List<ColumnDefinition> columns)
        {
            Name = name;
            PrimaryIdAttribute = primaryIdAttribute;
            ChangeTrackingEnabled= changeTrackingEnabled;
            Columns = columns != null ? new ColumnDefinitionCollection(columns) : new ColumnDefinitionCollection();
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public static TableDefinition FromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));
            return JsonSerializer.Deserialize<TableDefinition>(json)!;
        }

    }
}
