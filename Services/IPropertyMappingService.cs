namespace Library.API.Services
{
    using System.Collections.Generic;

    public interface IPropertyMappingService
    {
        IDictionary<string, PropertyMappingValue> GetPropertyMappingValue<TSource, TDestination>();
        bool ValidMappingExistsFor<TSource, TDestination>(string fields);
    }
}