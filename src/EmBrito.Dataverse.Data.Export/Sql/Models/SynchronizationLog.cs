using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.Data.Export.Models
{
    internal class SynchronizationLog
    {
        public int Id { get; set; }

        [Required]
        public Guid SynchronizationJobId { get; set; }

        [Required]
        public int SynchronizedTableId { get; set; }

        public int NewOrUpdated { get; set; }

        public int Removed { get; set; }

        [Required]
        [MaxLength(150)]
        public string SchemaQueryType { get; set; } = string.Empty;

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
