using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Core
{
    public class ApplicationOptions
    {
        public const string SectionName = "Values";

        public string SqlDbConnectionString { get; set; } = string.Empty;
        public string DataverseInstanceUrl { get; set; } = string.Empty;
        public string AppId { get; set; } = string.Empty;
        public string AppSecret { get; set; } = string.Empty;
        public int SqlCommandTimeoutSeconds { get; set; }
    }
}
