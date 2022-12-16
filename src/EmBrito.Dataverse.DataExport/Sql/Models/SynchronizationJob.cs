using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Sql.Models
{
    [Table("_SynchronizationJobs")]
    public class SynchronizationJob
    {

        public Guid Id { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }

        public List<SynchronizationLog> Logs { get; set; } = new List<SynchronizationLog>();

    }
}
