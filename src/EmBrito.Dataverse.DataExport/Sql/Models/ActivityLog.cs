using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Sql.Models
{

    [Table("_ActivityLogs")]
    public class ActivityLog
    {

        public int Id { get; set; }

        [Required]
        public Guid SynchronizationJobId { get; set; }

        [Required]
        public int SynchronizedTableId { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Description { get; set; } = string.Empty;

        public bool Error { get; set; } = false;

        public SynchronizationJob? SynchronizationJob { get; set; }

        public SynchronizedTable? SynchronizedTable { get; set; }

    }
}
