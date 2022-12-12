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

        public string LogicalName { get; private set; } = string.Empty;

        public string PrimaryIdField { get; private set; } = string.Empty;

        public bool? ChangeTrackingEnabled { get; private set; }

        public bool Enabled { get; private set; }

        public string? DataToken { get; private set; }

        public string? TableSchema { get; private set; }

    }
}
