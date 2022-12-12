using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Sql.Models
{
    public class SynchronizedTable
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string LogicalName { get; set; } = string.Empty;

        public bool Enabled { get; set; }

        [MaxLength(150)]
        public string? DataToken { get; set; }

        public string? TableSchema { get; set; }

        public List<SynchronizationLog> Logs { get; set; } = new List<SynchronizationLog>();
    }
}
