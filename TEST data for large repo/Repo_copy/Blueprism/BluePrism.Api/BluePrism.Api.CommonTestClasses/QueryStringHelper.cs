namespace BluePrism.Api.CommonTestClasses
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using Extensions;

    public static class QueryStringHelper
    {
        public static string GenerateQueryStringFromParameters<TParameterObject>(TParameterObject parameters)
            where TParameterObject : class
        {
            var propertyNamesAndValues = GetPropertyNamesAndValues(parameters, p => p.GetType().GetProperties()).ToList();

            return string.Join("&", propertyNamesAndValues.Select(x => $"{x.ParameterName}={x.ParameterValue}"));
        }

        private static IEnumerable<(string ParameterName, object ParameterValue)> GetPropertyNamesAndValues(object parameterObject, Func<object, PropertyInfo[]> getProperties)
        {
            var parameterObjectStack = new Stack<(string, object)>();
            parameterObjectStack.Push((string.Empty, parameterObject));

            while (parameterObjectStack.Count != 0)
            {
                var (parentPropertyName, current) = parameterObjectStack.Pop();

                foreach (var propertyInfo in getProperties(current))
                {
                    var propertyValue = propertyInfo.GetValue(current, null);

                    if (propertyValue == null)
                        continue;

                    propertyValue = ConvertSpecialValueTypes(propertyValue);

                    // e.g. processName.strtw
                    var dotNotationPropertyName = GetDotNotationPropertyName(parentPropertyName, propertyInfo.Name);

                    if (CanTraverse(propertyInfo.PropertyType))
                        parameterObjectStack.Push((dotNotationPropertyName, propertyValue));
                    else
                        yield return (dotNotationPropertyName, propertyValue);
                }
            }
        }

        private static bool CanTraverse(Type propertyType) =>
            propertyType.IsClass
            && propertyType != typeof(string)
            && !TypeDescriptor.GetConverter(propertyType).CanConvertFrom(typeof(string));

        private static string GetDotNotationPropertyName(string parentPropertyName, string currentPropertyName) =>
            string.IsNullOrEmpty(parentPropertyName)
                ? currentPropertyName.ToLowerCaseFirstCharacter()
                : $"{parentPropertyName.ToLowerCaseFirstCharacter()}.{currentPropertyName.ToLowerCaseFirstCharacter()}";

        private static object ConvertSpecialValueTypes(object value)
        {
            switch (value)
            {
                case DateTimeOffset dateTime:
                    return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

                default:
                    return value;
            }
        }
    }
}
