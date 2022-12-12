using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.Data.Export.Schema
{
    public class TableDefinition
    {

        [JsonInclude]
        public string Name { get; init; }
        [JsonInclude]
        public string PrimaryIdAttribute { get; init; }
        [JsonInclude]
        public IEnumerable<ColumnDefinition> Columns { get; init; }

        public TableDefinition()
        {
            // this constructtor is required by Syste.Text.Json for deserialization
            Name = String.Empty;
            PrimaryIdAttribute = String.Empty;
            Columns = Enumerable.Empty<ColumnDefinition>();
        }

        internal TableDefinition(string name, string primaryIdAttribute, List<ColumnDefinition> columns)
        {
            Name = name;
            PrimaryIdAttribute = primaryIdAttribute;
            Columns = columns ?? Enumerable.Empty<ColumnDefinition>();
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
