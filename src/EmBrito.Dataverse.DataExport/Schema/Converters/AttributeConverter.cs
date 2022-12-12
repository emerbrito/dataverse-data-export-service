using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Schema.Converters
{
    internal abstract class AttributeConverter
    {
        
        protected abstract ColumnDefinition? Convert(AttributeMetadata attributeMetadata, EntityMetadata entityMetadata);

        public abstract AttributeTypeDisplayName[] AttributeTypeFilter();

        public bool ToColumnDefinitin(AttributeMetadata attributeMetadata, EntityMetadata entityMetadata, out ColumnDefinition? column)
        {

            _ = attributeMetadata ?? throw new ArgumentNullException(nameof(attributeMetadata));
            _ = entityMetadata ?? throw new ArgumentNullException(nameof(entityMetadata));

            column = null;

            if(attributeMetadata.AttributeTypeName is null || !AttributeTypeFilter().Contains(attributeMetadata.AttributeTypeName))
            {
                return false;
            }

            if(!entityMetadata.Attributes.Any(a => a.LogicalName == attributeMetadata.LogicalName))
            {
                throw new InvalidOperationException($"Attribute {attributeMetadata.LogicalName} was passed as an argument for entity {entityMetadata.LogicalName} but could not be found in the entity metadata attributes collection.");
            }

            column = Convert(attributeMetadata, entityMetadata);
            return column != null;
        }
    }
}
