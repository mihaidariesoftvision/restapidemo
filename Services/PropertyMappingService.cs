namespace Library.API.Services
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class PropertyMappingService : IPropertyMappingService
    {
        private readonly IDictionary<string, PropertyMappingValue> _authorPropertyMapping = new Dictionary<string, PropertyMappingValue>(StringComparer.CurrentCultureIgnoreCase)
        {
            { "Id", new PropertyMappingValue(new List<string> { "Id" }) },
            { "Genre", new PropertyMappingValue(new List<string> { "Genre" })},
            { "Age", new PropertyMappingValue(new List<string> { "DateOfBirth" }, true) },
            { "Name", new PropertyMappingValue(new List<string> { "FirstName", "LastName" }) }
        };

        private readonly IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            _propertyMappings.Add(new PropertyMapping(_authorPropertyMapping));
        }

        public IDictionary<string, PropertyMappingValue> GetPropertyMappingValue<TSource, TDestination>()
        {
            var matchingMapping = _propertyMappings.OfType<PropertyMapping>();

            var mappings = matchingMapping.ToList();
            if (mappings.Count == 1)
            {
                return mappings.First().MappingDictionary;
            }

            throw new Exception($"Could not find property matching instance for <{typeof(TSource)}");
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMappingValue<TSource, TDestination>();

            if (string.IsNullOrEmpty(fields))
            {
                return true;
            }

            return (from field in fields.Split(',')
                    select field.Trim()
                    into trimmedField
                    let indexOfFirstSpace = trimmedField.IndexOf(' ', StringComparison.InvariantCulture)
                    select indexOfFirstSpace == -1
                        ? trimmedField
                        : trimmedField.Remove(indexOfFirstSpace))
                .All(propertyName => propertyMapping.ContainsKey(propertyName));
        }
    }
}