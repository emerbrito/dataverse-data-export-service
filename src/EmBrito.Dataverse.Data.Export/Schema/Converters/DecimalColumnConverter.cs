using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.Data.Export.Schema.Converters
{
    internal class DecimalColumnConverter : AttributeToColumnConverterBase
    {

        public override AttributeTypeDisplayName[] AttributeTypeFilter()
        {
            return new AttributeTypeDisplayName[]
            {
                AttributeTypeDisplayName.DecimalType
            };
        }

        protected override ColumnDefinition? Convert(AttributeMetadata attributeMetadata, EntityMetadata entityMetadata)
        {
            return ColumnDefinitionFactory.CreateDecimal(
                attributeMetadata.LogicalName, 
                ((DecimalAttributeMetadata)attributeMetadata).Precision!.Value);
        }
    }
}
