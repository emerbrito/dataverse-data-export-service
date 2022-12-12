using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Schema
{
    internal class ColumnComparer : IEqualityComparer<ColumnDefinition>
    {
        public bool Equals(ColumnDefinition? x, ColumnDefinition? y)
        {
            return x is null && y is null || 
                x != null && y != null && 
                (
                    x.MaxLength == y.MaxLength && 
                    x.Name == y.Name && 
                    x.Precision == y.Precision && 
                    x.Scale == y.Scale &&                    
                    x.TypeName == y.TypeName
                );
        }

        public int GetHashCode([DisallowNull] ColumnDefinition obj)
        {
            return (obj.MaxLength + obj.Precision + obj.Scale).GetHashCode()
                + obj.Name.GetHashCode()
                + obj.TypeName.GetHashCode();
        }
    }
}
