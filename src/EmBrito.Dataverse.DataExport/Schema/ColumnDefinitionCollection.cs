using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Schema
{
    public class ColumnDefinitionCollection : KeyedCollection<string, ColumnDefinition>
    {

        public ColumnDefinitionCollection()
        {
        }

        public ColumnDefinitionCollection(IEnumerable<ColumnDefinition> items)
        {
            AddRange(items);
        }

        public void AddAndSort(ColumnDefinition item)
        {
            Add(item);
            Sort();
        }

        public void Sort()
        {
            List<ColumnDefinition>? items = base.Items as List<ColumnDefinition>;
            items?.Sort(CompareByName);
        }

        private void AddRange(IEnumerable<ColumnDefinition> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        private static int CompareByName(ColumnDefinition a, ColumnDefinition b)
        {
            return String.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
        }

        protected override string GetKeyForItem(ColumnDefinition item)
        {
            return item.Name;
        }
    }
}
