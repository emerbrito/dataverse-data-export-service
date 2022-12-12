using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.Data.Export.Schema
{
    internal class TableNameComparer : IEqualityComparer<TableDefinition>
    {
        public bool Equals(TableDefinition? x, TableDefinition? y)
        {
            return x is null && y is null || x != null && y != null && x.Name == y.Name;
        }

        public int GetHashCode([DisallowNull] TableDefinition obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
