using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.Data.Export.Models
{
    public class TableSetting
    {

        public int Id { get; set; }

        public string LogicalName { get; internal set; } = string.Empty;

        public bool Enabled { get; internal set; }

        public string? DataToken { get; internal set; }

        public string? TableSchema { get; internal set; }

    }
}
