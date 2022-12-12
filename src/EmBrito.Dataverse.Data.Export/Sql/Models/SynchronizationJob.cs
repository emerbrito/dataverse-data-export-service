using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.Data.Export.Models
{
    internal class SynchronizationJob
    {

        public Guid Id { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public int NewOrUpdated { get; set; }
        public int Removed { get; set; }
        public int SchemaChanges { get; set; }
        public bool? Timeout { get; set; }

        public List<SynchronizationLog> Logs { get; set; } = new List<SynchronizationLog>();

    }
}
