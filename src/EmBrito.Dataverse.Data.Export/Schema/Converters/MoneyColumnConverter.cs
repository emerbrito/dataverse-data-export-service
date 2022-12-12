using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.Data.Export.Schema.Converters
{
    internal class MoneyColumnConverter : AttributeToColumnConverterBase
    {
        public override AttributeTypeDisplayName[] AttributeTypeFilter()
        {
            return new AttributeTypeDisplayName[]
            {
                AttributeTypeDisplayName.MoneyType
            };
        }

        protected override ColumnDefinition? Convert(AttributeMetadata attributeMetadata, EntityMetadata entityMetadata)
        {
            return ColumnDefinitionFactory.CreateMoney(
                attributeMetadata.LogicalName,
                ((MoneyAttributeMetadata)attributeMetadata).Precision ?? 0);
        }
    }
}
