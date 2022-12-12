using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Tests
{
    internal static class MetadataHelper
    {

        public static EntityMetadata GetEntityMetadata()
        {
            return GetEntityMetadata(GetAttributeMetadata());
        }

        public static EntityMetadata GetEntityMetadata(IEnumerable<AttributeMetadata> attributeMetadata)
        {
            var metadata = new EntityMetadata
            {
                LogicalName = "new_testentity",
                SchemaName = "new_testentity"                
            };

            PropertyHelper.SetPrivateProperty<EntityMetadata, string>(metadata, "PrimaryIdAttribute", "new_uniqueidentifierattribute");
            PropertyHelper.SetPrivateProperty<EntityMetadata, string>(metadata, "PrimaryNameAttribute", "new_stringattribute");
            PropertyHelper.SetPrivateProperty<EntityMetadata, AttributeMetadata[]>(
                metadata,
                "Attributes",
                attributeMetadata.ToArray());

            return metadata;
        }

        public static List<AttributeMetadata> GetAttributeMetadata()
        {
            var list = new List<AttributeMetadata>()
            {
                new BigIntAttributeMetadata("new_bigintattribute")
                {
                    LogicalName = "new_bigintattribute"
                },
                new BooleanAttributeMetadata("new_booleanattribute")
                {
                    LogicalName = "new_booleanattribute"
                },
                new DateTimeAttributeMetadata(DateTimeFormat.DateAndTime, "new_datatimeattribute")
                {
                    LogicalName = "new_datatimeattribute"
                },
                new DecimalAttributeMetadata("new_decimalattribute") 
                {
                    LogicalName = "new_decimalattribute",
                    Precision = 4
                },
                new DoubleAttributeMetadata("new_doubleattribute")
                {
                    LogicalName = "new_doubleattribute",
                    Precision = 6
                },
                new EntityNameAttributeMetadata("new_entitynameattribute")
                {
                    LogicalName = "new_entitynameattribute"
                },
                new IntegerAttributeMetadata("new_integerattribute")
                {
                    LogicalName = "new_integerattribute"
                },
                new LookupAttributeMetadata(LookupFormat.None)
                {
                    LogicalName = "new_lookupattribute",
                    SchemaName = "new_lookupattribute"
                },
                new MemoAttributeMetadata("new_memoattribute")
                {
                    LogicalName = "new_memoattribute"
                },
                new MoneyAttributeMetadata("new_moneyattribute")
                {
                    LogicalName = "new_moneyattribute",
                    Precision = 3
                },
                new MultiSelectPicklistAttributeMetadata("new_multiselectpicklistattribute")
                {
                    LogicalName = "new_multiselectpicklistattribute"
                },
                new PicklistAttributeMetadata("new_picklistattribute")
                {
                    LogicalName = "new_picklistattribute"
                },
                new StringAttributeMetadata("new_stringattribute")
                {
                    LogicalName = "new_stringattribute",
                    MaxLength = 150
                },
                new UniqueIdentifierAttributeMetadata("new_uniqueidentifierattribute")
                {
                    LogicalName = "new_uniqueidentifierattribute"
                },

                new VirtualAttributeMetadata("new_virtualattribute"),
                new VirtualAttributeMetadata("new_virtualofoptionsetattribute", "new_picklistattribute"),
                new VirtualAttributeMetadata("new_virtualoflookupattribute", "new_lookupattribute"),
            };

            return list;
        }

    }
}
