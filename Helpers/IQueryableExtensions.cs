namespace Library.API.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Library.API.Services;
    using System.Linq.Dynamic.Core;

    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy, IDictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (mappingDictionary == null)
            {
                throw new ArgumentNullException(nameof(mappingDictionary));
            }

            if (string.IsNullOrWhiteSpace(orderBy))
            {
                return source;
            }

            var orderByAfterSplit = orderBy.Split(',');

            var sortingFields = new StringBuilder();
            var orderByAfterSplitList = orderByAfterSplit.ToList();
            foreach (var orderByClause in orderByAfterSplitList)
            {
                var trimmedOrderByClause = orderByClause.Trim();
                var orderDescending = trimmedOrderByClause.EndsWith(" desc", StringComparison.Ordinal);

                var indexOfFirstSpace = trimmedOrderByClause.IndexOf(' ', StringComparison.InvariantCulture);
                var propertyName = indexOfFirstSpace == -1
                    ? trimmedOrderByClause
                    : trimmedOrderByClause.Remove(indexOfFirstSpace);

                if (!mappingDictionary.ContainsKey(propertyName))
                {
                    throw new ArgumentNullException($"Key mapping for {propertyName} is missing");
                }

                var propertyMappingValue = mappingDictionary[propertyName];

                if (propertyMappingValue == null)
                {
                    throw new ArgumentException(nameof(propertyMappingValue));
                }

                var destinationProperties = propertyMappingValue.DestinationProperties.ToList();
                foreach (var destinationProperty in destinationProperties)
                {
                    if (propertyMappingValue.Revert)
                    {
                        orderDescending = !orderDescending;
                    }

                    var sortOrder = orderDescending ? " descending" : " ascending";
                    sortingFields.Append(
                        destinationProperties.IndexOf(destinationProperty) < destinationProperties.Count - 1
                            ? $"{destinationProperty}{sortOrder}, "
                            : $"{destinationProperty}{sortOrder}");
                }

                if (orderByAfterSplitList.IndexOf(orderByClause) < orderByAfterSplitList.Count - 1)
                {
                    sortingFields.Append(", ");
                }
            }

            source = source.OrderBy(sortingFields.ToString());

            return source;
        }
    }
}
