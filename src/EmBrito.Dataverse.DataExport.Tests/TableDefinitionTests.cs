using EmBrito.Dataverse.DataExport.Schema;
using Microsoft.Xrm.Sdk.Metadata;
using System.Reflection;

namespace EmBrito.Dataverse.DataExport.Tests
{
    public class TableDefinitionTests
    {

        [Fact]
        public void BuildFromMetadata()
        {

            var metadata = MetadataHelper.GetEntityMetadata();
            var virtualAttributesCount = metadata.Attributes.OfType<VirtualAttributeMetadata>().Count();

            var definitions = TableDefinitionBuilder
                .WithMetadata(metadata)
                .ExcludeAttributes(a => a.AttributeTypeName == AttributeTypeDisplayName.VirtualType)
                .BuildDefinitions();            

            Assert.NotNull(definitions);
            Assert.Equal(metadata.LogicalName, definitions.Name);
            Assert.Equal(metadata.PrimaryIdAttribute, definitions.PrimaryIdAttribute);
            Assert.Equal(metadata.Attributes.Count() - virtualAttributesCount, definitions.Columns.Count());

            Assert.NotNull(definitions.Columns.FirstOrDefault(c => c.Name == "new_bigintattribute" && c.TypeName == ColumnDefinitionFactory.BigIntDataType));
            Assert.NotNull(definitions.Columns.FirstOrDefault(c => c.Name == "new_booleanattribute" && c.TypeName == ColumnDefinitionFactory.BooleanDataType));
            Assert.NotNull(definitions.Columns.FirstOrDefault(c => c.Name == "new_datatimeattribute" && c.TypeName == ColumnDefinitionFactory.DateTimeDataType));
            Assert.NotNull(definitions.Columns.FirstOrDefault(c => c.Name == "new_decimalattribute" && c.TypeName == ColumnDefinitionFactory.DecimalDataType));
            Assert.NotNull(definitions.Columns.FirstOrDefault(c => c.Name == "new_doubleattribute" && c.TypeName == ColumnDefinitionFactory.DoubleDataType));
            Assert.NotNull(definitions.Columns.FirstOrDefault(c => c.Name == "new_entitynameattribute" && c.TypeName == ColumnDefinitionFactory.StringDataType));
            Assert.NotNull(definitions.Columns.FirstOrDefault(c => c.Name == "new_integerattribute" && c.TypeName == ColumnDefinitionFactory.IntegerDataType));
            Assert.NotNull(definitions.Columns.FirstOrDefault(c => c.Name == "new_lookupattribute" && c.TypeName == ColumnDefinitionFactory.UniqueIdentifierDataType));
            Assert.NotNull(definitions.Columns.FirstOrDefault(c => c.Name == "new_memoattribute" && c.TypeName == ColumnDefinitionFactory.StringDataType));
            Assert.NotNull(definitions.Columns.FirstOrDefault(c => c.Name == "new_moneyattribute" && c.TypeName == ColumnDefinitionFactory.MoneyDataType));
            Assert.NotNull(definitions.Columns.FirstOrDefault(c => c.Name == "new_multiselectpicklistattribute" && c.TypeName == ColumnDefinitionFactory.StringDataType));
            Assert.NotNull(definitions.Columns.FirstOrDefault(c => c.Name == "new_picklistattribute" && c.TypeName == ColumnDefinitionFactory.IntegerDataType));
            Assert.NotNull(definitions.Columns.FirstOrDefault(c => c.Name == "new_stringattribute" && c.TypeName == ColumnDefinitionFactory.StringDataType));
            Assert.NotNull(definitions.Columns.FirstOrDefault(c => c.Name == "new_uniqueidentifierattribute" && c.TypeName == ColumnDefinitionFactory.UniqueIdentifierDataType));

        }

        [Fact]
        public void SerializeTableDefintion()
        {

            var metadata = MetadataHelper.GetEntityMetadata();
            var virtualAttributesCount = metadata.Attributes.OfType<VirtualAttributeMetadata>().Count();

            var definitions = TableDefinitionBuilder
                .WithMetadata(metadata)
                .ExcludeAttributes(a => a.AttributeTypeName == AttributeTypeDisplayName.VirtualType)
                .BuildDefinitions();

            string actualJson = definitions.ToJson();
            string expectedJson = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "SerializedTableDefinition.json"));

            actualJson = actualJson.Replace("\n", "").Replace("\r", "").Replace(" ", "");
            expectedJson = expectedJson.Replace("\n", "").Replace("\r", "").Replace(" ", "");

            Assert.Equal(expectedJson, actualJson);

        }

        [Fact]
        public void DeserializeTableDefintion()
        {

            var metadata = MetadataHelper.GetEntityMetadata();
            var virtualAttributesCount = metadata.Attributes.OfType<VirtualAttributeMetadata>().Count();

            var expectedDefinitions = TableDefinitionBuilder
                .WithMetadata(metadata)
                .ExcludeAttributes(a => a.AttributeTypeName == AttributeTypeDisplayName.VirtualType)
                .BuildDefinitions();

            string json = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "SerializedTableDefinition.json"));
            var actualDefinitions = TableDefinition.FromJson(json);
            var comparer = new SchemaComparer(
                new TableDefinitionCollection(expectedDefinitions),
                new TableDefinitionCollection(actualDefinitions));

            Assert.Equal(expectedDefinitions.Name, actualDefinitions.Name);
            Assert.Equal(expectedDefinitions.Columns.Count(), actualDefinitions.Columns.Count());
            Assert.False(comparer.HasChanges);

        }


        [Fact]
        public void ComparerWithNoLocalDefinitions()
        {

            var metadata = MetadataHelper.GetEntityMetadata();
            var virtualAttributesCount = metadata.Attributes.OfType<VirtualAttributeMetadata>().Count();

            var remoteDefinitions = TableDefinitionBuilder
                .WithMetadata(metadata)
                .ExcludeAttributes(a => a.AttributeTypeName == AttributeTypeDisplayName.VirtualType)
                .BuildDefinitions();

            var comparer = new SchemaComparer(
                new TableDefinitionCollection(),
                new TableDefinitionCollection(remoteDefinitions));

            Assert.True(comparer.HasChanges);
            Assert.True(comparer.CreatedTables.Count() == 1);
            Assert.True(comparer.DeletedTables.Count() == 0);
            Assert.True(comparer.SchemaChanges.Count() == 0);

        }

        [Fact]
        public void ComparerAddTable()
        {

            var metadata = MetadataHelper.GetEntityMetadata();
            var remoteDefinitions = new TableDefinitionCollection();
            var localDefinitions = new TableDefinitionCollection();
            var table2 = $"{metadata.LogicalName}2";

            // add the same table to local and remote so they can match

            remoteDefinitions.Add(TableDefinitionBuilder
                .WithMetadata(metadata)
                .ExcludeAttributes(a => a.AttributeTypeName == AttributeTypeDisplayName.VirtualType)
                .BuildDefinitions());

            localDefinitions.Add(TableDefinitionBuilder
                .WithMetadata(metadata)
                .ExcludeAttributes(a => a.AttributeTypeName == AttributeTypeDisplayName.VirtualType)
                .BuildDefinitions());

            // change name and add again to remote expecting it will be seen as a new table
            metadata.LogicalName = table2;
            metadata.SchemaName = table2;

            remoteDefinitions.Add(TableDefinitionBuilder
                .WithMetadata(metadata)
                .ExcludeAttributes(a => a.AttributeTypeName == AttributeTypeDisplayName.VirtualType)
                .BuildDefinitions());

            var comparer = new SchemaComparer(new TableDefinitionCollection(localDefinitions), remoteDefinitions);

            Assert.True(comparer.HasChanges);
            Assert.True(comparer.CreatedTables.Count() == 1);
            Assert.True(comparer.DeletedTables.Count() == 0);
            Assert.True(comparer.SchemaChanges.Count() == 0);
            Assert.Equal(table2, comparer.CreatedTables.First().Name);

        }


        [Fact]
        public void ComparerDeleteTable()
        {

            var metadata = MetadataHelper.GetEntityMetadata();
            var remoteDefinitions = new TableDefinitionCollection();
            var localDefinitions = new TableDefinitionCollection();
            var table2 = $"{metadata.LogicalName}2";

            // add the same table to local and remote so they can match

            remoteDefinitions.Add(TableDefinitionBuilder
                .WithMetadata(metadata)
                .ExcludeAttributes(a => a.AttributeTypeName == AttributeTypeDisplayName.VirtualType)
                .BuildDefinitions());

            localDefinitions.Add(TableDefinitionBuilder
                .WithMetadata(metadata)
                .ExcludeAttributes(a => a.AttributeTypeName == AttributeTypeDisplayName.VirtualType)
                .BuildDefinitions());

            // change name and add again to loca so it can be considered a table no longer in the dataverse
            metadata.LogicalName = table2;
            metadata.SchemaName = table2;

            localDefinitions.Add(TableDefinitionBuilder
                .WithMetadata(metadata)
                .ExcludeAttributes(a => a.AttributeTypeName == AttributeTypeDisplayName.VirtualType)
                .BuildDefinitions());

            var comparer = new SchemaComparer(localDefinitions, remoteDefinitions);

            Assert.True(comparer.HasChanges);
            Assert.True(comparer.CreatedTables.Count() == 0);
            Assert.True(comparer.DeletedTables.Count() == 1);
            Assert.True(comparer.SchemaChanges.Count() == 0);
            Assert.Equal(table2, comparer.DeletedTables.First().Name);

        }

        [Fact]
        public void ComparerAlterTable()
        {

            var originalMetadata = MetadataHelper.GetEntityMetadata();
            var remoteDefinitions = new TableDefinitionCollection();
            var localDefinitions = new TableDefinitionCollection();

            // add table to local definitions 
            localDefinitions.Add(TableDefinitionBuilder
                .WithMetadata(originalMetadata)
                .ExcludeAttributes(a => a.AttributeTypeName == AttributeTypeDisplayName.VirtualType)
                .BuildDefinitions());

            // manipulate the attributes collection before re-creating metadata to simulate changes
            var attributes = MetadataHelper.GetAttributeMetadata();
            var attString = (StringAttributeMetadata)attributes.First(a => a.LogicalName == "new_stringattribute");
            var attLookup = attributes.First(a => a.LogicalName == "new_lookupattribute"); ;

            // remove lookup attribute
            attributes.Remove(attLookup);
            // update string attribute
            attString.MaxLength = 250;
            // add new attribute
            attributes.Add(new IntegerAttributeMetadata("newattribute")
            {
                LogicalName = "newattribute"
            });

            var modifiedMetadata = MetadataHelper.GetEntityMetadata(attributes);
            remoteDefinitions.Add(TableDefinitionBuilder
                .WithMetadata(modifiedMetadata)
                .ExcludeAttributes(a => a.AttributeTypeName == AttributeTypeDisplayName.VirtualType)
                .BuildDefinitions());

            var comparer = new SchemaComparer(localDefinitions, remoteDefinitions);
            var tableChanges = comparer.SchemaChanges.FirstOrDefault();

            Assert.True(comparer.HasChanges);
            Assert.True(comparer.CreatedTables.Count() == 0);
            Assert.True(comparer.DeletedTables.Count() == 0);
            Assert.True(comparer.SchemaChanges.Count() == 1);
            Assert.NotNull(tableChanges);
            Assert.Equal(modifiedMetadata.LogicalName, tableChanges!.LogicalName);
            Assert.True(tableChanges!.HasChanges);
            Assert.True(tableChanges!.DeletedColumns.Count() == 1);
            Assert.True(tableChanges!.NewColumns.Count() == 1);
            Assert.True(tableChanges!.UpdatedColumns.Count() == 1);
            Assert.Equal(attString.LogicalName, tableChanges!.UpdatedColumns.First().Name);
            Assert.Equal(attString.MaxLength * 2, tableChanges!.UpdatedColumns.First().MaxLength);
            Assert.Equal(attLookup.LogicalName, tableChanges!.DeletedColumns.First().Name);
            Assert.Equal("newattribute", tableChanges!.NewColumns.First().Name);
            
        }

    }
}