namespace Library.API.Helpers
{
    using System;
    using System.Dynamic;
    using System.Collections.Generic;
    using System.Reflection;

    public static class ObjectExtensions
    {
        public static ExpandoObject ShapeData<TSource>(this TSource source, string fields)
        {
            void AddToObject(PropertyInfo propertyInfo, ExpandoObject expandoObject)
            {
                var propertyValue = propertyInfo.GetValue(source);
                ((IDictionary<string, object>) expandoObject).Add(propertyInfo.Name, propertyValue);
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var dataShapedObject = new ExpandoObject();

            if (string.IsNullOrWhiteSpace(fields))
            {
                var propertyInfos = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var propertyInfo in propertyInfos)
                {
                    AddToObject(propertyInfo, dataShapedObject);
                }
            }
            else
            {
                var fieldsAfterSplit = fields.Split(',');

                foreach (var field in fieldsAfterSplit)
                {
                    var propertyName = field.Trim();
                    var propertyInfo = typeof(TSource).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (propertyInfo == null)
                    {
                        throw new Exception($"Property {propertyName} wasn't found on {typeof(TSource)}");
                    }

                    AddToObject(propertyInfo, dataShapedObject);
                }
            }

            return dataShapedObject;
        }
    }
}