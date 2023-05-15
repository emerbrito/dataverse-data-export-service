using EmBrito.Dataverse.DataExport.Schema.Converters;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Schema
{

    public interface IDefinitionBuilderConfig
    {
        TableDefinition BuildDefinitions();
        IDefinitionBuilderConfig ExcludeAttributes(Predicate<AttributeMetadata> predicate);
        void SetNamePrefix(string prefix);
        public void SetNameSufix(string sufix);
    }

    public class TableDefinitionBuilder : IDefinitionBuilderConfig
    {

        private string namePrefix = string.Empty;
        private string nameSufix = string.Empty;
        private EntityMetadata _entityMetadata;
        private List<AttributeMetadata> _attributes;
        static Dictionary<AttributeTypeDisplayName, AttributeConverter> converters = new();

        static TableDefinitionBuilder()
        {
            RegisterConverters();
        }

        private TableDefinitionBuilder(EntityMetadata entityMetadata)
        {
            _entityMetadata = entityMetadata;
            _attributes = new List<AttributeMetadata>();
            entityMetadata.Attributes.ToList().ForEach(a => _attributes.Add(a));
        }

        public static IDefinitionBuilderConfig WithMetadata(EntityMetadata metadata)
        {
            _ = metadata ?? throw new ArgumentNullException(nameof(metadata));
            var builder = new TableDefinitionBuilder(metadata);
            return builder;
        }

        public TableDefinition BuildDefinitions()
        {
            var columns = new List<ColumnDefinition>();

            // all all attributes with an available converter
            foreach (var attribute in _attributes)
            {
                if (attribute.AttributeTypeName != null)
                {
                    if (converters.TryGetValue(attribute.AttributeTypeName, out AttributeConverter? converter))
                    {
                        if (converter!.ToColumnDefinitin(attribute, _entityMetadata, out ColumnDefinition? column))
                        {
                            columns.Add(column!);
                        }
                    }
                }
            }
            
            var tableDef = new TableDefinition(
                $"{(namePrefix ?? "")}{_entityMetadata.LogicalName}{(nameSufix ?? "")}", 
                _entityMetadata.LogicalName, 
                _entityMetadata.PrimaryIdAttribute, 
                _entityMetadata.ChangeTrackingEnabled ?? false, 
                columns);

            return tableDef;
        }

        public IDefinitionBuilderConfig ExcludeAttributes(Predicate<AttributeMetadata> predicate)
        {
            _attributes.RemoveAll(predicate);
            return this;
        }

        public void SetNamePrefix(string prefix)
        {
            if(string.IsNullOrEmpty(prefix)) throw new ArgumentNullException(prefix);
            namePrefix = prefix;
        }

        public void SetNameSufix(string sufix)
        {
            if (string.IsNullOrEmpty(sufix)) throw new ArgumentNullException(sufix);
            nameSufix = sufix;
        }

        static void RegisterConverters()
        {
            IEnumerable<Type> converterTypes = typeof(TableDefinitionBuilder).Assembly
                .GetTypes()
                .Where(x => x.BaseType != null
                    && x.BaseType.IsAbstract
                    && x.BaseType == typeof(AttributeConverter))
                .ToList();

            foreach (var type in converterTypes)
            {
                var instance = (AttributeConverter)Activator.CreateInstance(type)!;

                foreach (var attributeType in instance.AttributeTypeFilter())
                {
                    if (converters.ContainsKey(attributeType))
                    {
                        throw new InvalidOperationException($"An attempt to register column converter for type {attributeType} failed. Type already registered. Converter: {instance.GetType().Name}.");
                    }
                    else
                    {
                        converters.Add(attributeType, instance);
                    }
                }

            }

        }
    }
}
