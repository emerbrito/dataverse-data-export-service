using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Schema
{
    public class TableDefinitionCollection : KeyedCollection<string, TableDefinition>
    {
        public TableDefinitionCollection()
        {
        }

        public TableDefinitionCollection(TableDefinition item)
        {
            Add(item);
        }

        public TableDefinitionCollection(IEnumerable<TableDefinition> items)
        {
            AddRange(items);
        }


        public void AddRange(IEnumerable<TableDefinition> items)
        {
            foreach (var item in items)
            {
                this.Add(item);
            }
        }
        

        protected override string GetKeyForItem(TableDefinition item)
        {
            return item.Name;
        }
    }
}
