using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Tests
{
    public class VirtualAttributeMetadata : AttributeMetadata
    {

        public VirtualAttributeMetadata(string schemaName): base(AttributeTypeCode.Virtual, schemaName)
        {
            this.LogicalName = schemaName;            
        }

        public VirtualAttributeMetadata(string schemaName, string attributeOf) : this(schemaName)
        {
            SetPrivateProperty<string>("AttributeOf", attributeOf);
        }

        public void SetPrivateProperty<T>(string propertyName, T newValue)
        {
            PropertyHelper.SetPrivateProperty<VirtualAttributeMetadata, T>(this, propertyName, newValue);
        }

    }
}
