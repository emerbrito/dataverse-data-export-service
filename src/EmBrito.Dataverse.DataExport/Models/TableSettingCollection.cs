using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.Data.Export.Models
{
    public class TableSettingCollection : KeyedCollection<string, TableSetting>
    {
        protected override string GetKeyForItem(TableSetting item)
        {
            return item.LogicalName;
        }

        public string[] LogicalNames()
        {
            return this
                .Where(t => !string.IsNullOrWhiteSpace(t.LogicalName))
                .Select(t => t.LogicalName)
                .OrderBy(l => l)
                .ToArray();
        }
    }
}
