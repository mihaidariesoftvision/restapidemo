namespace Library.API.Services
{
    using System.Collections.Generic;

    public class PropertyMapping : IPropertyMapping
    {
        public IDictionary<string, PropertyMappingValue> MappingDictionary { get; }

        public PropertyMapping(IDictionary<string, PropertyMappingValue> mappingDictionary)
        {
            MappingDictionary = mappingDictionary;
        }
    }
}
