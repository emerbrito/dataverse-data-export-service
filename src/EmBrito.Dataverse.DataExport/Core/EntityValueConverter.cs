using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Core
{
    public static class EntityValueConverter
    {

        public static object? ToPrimitiveValue(object attributeValue)
        {

            if (attributeValue is null)
            {
                return null;
            }

            if (attributeValue is string 
                || attributeValue is DateTime
                || attributeValue is Guid
                || IsNumber(attributeValue))
            {
                return attributeValue;
            }

            if (attributeValue is bool)
            {
                return (bool)attributeValue ? 1 : 0;
            }

            if (attributeValue.GetType().IsEnum)
            {
                return (int)attributeValue;
            }

            if (attributeValue is EntityReference)
            {
                return ((EntityReference)attributeValue).Id;
            }

            if (attributeValue is OptionSetValue)
            {
                return ((OptionSetValue)attributeValue).Value;
            }

            if (attributeValue is Money)
            {
                return ((Money)attributeValue).Value;
            }

            if (attributeValue is OptionSetValueCollection)
            {
                return String.Join(",", ((OptionSetValueCollection)attributeValue)
                    .Select(x => x.Value.ToString()));
            }

            return null;

        }

        public static bool IsNumber(object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }

    }
}
