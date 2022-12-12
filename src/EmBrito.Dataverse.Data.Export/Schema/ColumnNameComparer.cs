using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.Data.Export.Schema
{
    internal class ColumnNameComparer : IEqualityComparer<ColumnDefinition>
    {
        public bool Equals(ColumnDefinition? x, ColumnDefinition? y)
        {
            return x is null && y is null || x != null && y != null && x.Name == y.Name;
        }

        public int GetHashCode([DisallowNull] ColumnDefinition obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
