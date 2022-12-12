using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.Data.Export.Schema.Converters
{
    internal class StringColumnConverter : AttributeToColumnConverterBase
    {
        public override AttributeTypeDisplayName[] AttributeTypeFilter()
        {
            return new AttributeTypeDisplayName[]
            {
                AttributeTypeDisplayName.StringType
            };
        }

        protected override ColumnDefinition? Convert(AttributeMetadata attributeMetadata, EntityMetadata entityMetadata)
        {

            var stringMetadata = (StringAttributeMetadata)attributeMetadata;

            return ColumnDefinitionFactory.CreateString(
                attributeMetadata.LogicalName,
                ((StringAttributeMetadata)attributeMetadata).MaxLength!.Value);
        }
    }
}
