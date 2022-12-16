using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Models
{
    internal class EntityChanges
    {
        public IEnumerable<Entity> NewOrUpdated { get; private set; }
        public IEnumerable<EntityReference> Removed { get; private set; }
        public string Token { get; private set; }

        public EntityChanges(string token, IEnumerable<Entity> newOrUpdate, IEnumerable<EntityReference> removed)
        {
            this.Token = token;
            this.NewOrUpdated = newOrUpdate;
            this.Removed = removed;
        }
    }
}
