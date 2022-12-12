using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Tests
{
    internal static class PropertyHelper
    {
        public static void SetPrivateProperty<TObject, TValue>(TObject targetObject, string propertyName, TValue newValue)
        {
            _ = targetObject ?? throw new ArgumentNullException(nameof(targetObject));
            _ = propertyName ?? throw new ArgumentNullException(nameof(propertyName));

            PropertyInfo? propertyInfo = targetObject.GetType().GetProperty(propertyName);
            if (propertyInfo == null) return;
            propertyInfo.SetValue(targetObject, newValue);
        }

    }
}
