using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.Data.Export.Schema
{
    public class ColumnDefinition
    {

        [JsonInclude]
        public string Name { get; internal set; } = string.Empty;
        [JsonInclude]
        public string TypeName { get; internal set; } = string.Empty;
        [JsonInclude]
        public int MaxLength { get; internal set; }
        [JsonInclude]
        public int Precision { get; internal set; }
        [JsonInclude]
        public int Scale { get; internal set; }

        public ColumnDefinition Clone(string cloneLogicalName)
        {
            return new ColumnDefinition
            { 
                MaxLength = MaxLength, 
                Name = cloneLogicalName, 
                Precision = Precision, 
                Scale = Scale, 
                TypeName = TypeName
            };
        }

    }
}
