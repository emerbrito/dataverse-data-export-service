using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Schema.Converters
{
    internal class DateTimeColumnConverter : AttributeConverter
    {

        public override AttributeTypeDisplayName[] AttributeTypeFilter()
        {
            return new AttributeTypeDisplayName[]
            {
                AttributeTypeDisplayName.DateTimeType
            };
        }

        protected override ColumnDefinition? Convert(AttributeMetadata attributeMetadata, EntityMetadata entityMetadata)
        {            
            return ColumnDefinitionFactory.CreateDateTime(attributeMetadata.LogicalName);
        }
    }
}
