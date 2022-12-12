using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Schema.Converters
{
    internal class UniqueIdentifierColumnConverter : AttributeConverter
    {
        public override AttributeTypeDisplayName[] AttributeTypeFilter()
        {
            return new AttributeTypeDisplayName[]
            {
                AttributeTypeDisplayName.CustomerType,
                AttributeTypeDisplayName.LookupType,
                AttributeTypeDisplayName.OwnerType,
                AttributeTypeDisplayName.UniqueidentifierType
            };
        }

        protected override ColumnDefinition? Convert(AttributeMetadata attributeMetadata, EntityMetadata entityMetadata)
        {
            return ColumnDefinitionFactory.CreateUniqueIdentifier(attributeMetadata.LogicalName);
        }
    }
}
