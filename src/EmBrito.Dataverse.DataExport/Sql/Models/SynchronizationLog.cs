using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Sql.Models
{
    [Table("_SynchronizationLogs")]
    public class SynchronizationLog
    {
        public int Id { get; set; }

        [Required]
        public Guid SynchronizationJobId { get; set; }

        [Required]
        public int SynchronizedTableId { get; set; }

        public int NewOrUpdated { get; set; }

        public int Removed { get; set; }

        public DateTime StartedOn { get; set; }

        public DateTime CompletedOn { get; set; }

        [MaxLength(400)]
        public string? RequestDeltaToken { get; set; }

        [MaxLength(400)]
        public string? ResponseDeltaToken { get; set; }

        public bool Errors { get; set; }

        public SynchronizationJob? SynchronizationJob { get; set; }

        public SynchronizedTable? SynchronizedTable { get; set; }
    }
}
