namespace BluePrism.Api.Models
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http.Controllers;
    using System.Web.Http.ModelBinding;
    using Exceptions;
    using Func;

    public class CommaDelimitedCollectionModelBinder : IModelBinder
    {
        private delegate Result<object> ResultValueConverter(string value);

        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            if (!IsCommaDelimitedCollectionType(bindingContext.ModelType))
                return false;

            var genericArgumentType = bindingContext.ModelType.GenericTypeArguments[0];
            var collectionConstructor = GetCommaDelimitedCollectionConstructorOrThrow(genericArgumentType, typeof(object[]));
            var failedModelBindCollectionConstructor = GetCommaDelimitedCollectionConstructorOrThrow(genericArgumentType, typeof(string));
            var valueConverterFunc = GetValueConverterForType(genericArgumentType);
            var rawValues = GetRawStringValues(bindingContext);

            var values = rawValues
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => valueConverterFunc(x))
                .ToArray();

            bindingContext.Model = values.OfType<Failure>().Any()
                ? failedModelBindCollectionConstructor.Invoke(new object[] {string.Join(",", rawValues)})
                : collectionConstructor.Invoke(new object[] {values.OfType<Success<object>>().Select(x => x.Value)});

            return true;
        }

        private static ConstructorInfo GetCommaDelimitedCollectionConstructorOrThrow(Type genericArgumentType, Type constructorType) =>
            typeof(CommaDelimitedCollection<>)
                .MakeGenericType(genericArgumentType)
                .GetConstructor(new[] {constructorType})
            ?? throw new CommaDelimitedCollectionConstructorMissingException();

        private static bool IsCommaDelimitedCollectionType(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(CommaDelimitedCollection<>);

        private static string[] GetRawStringValues(ModelBindingContext bindingContext)
        {
            var rawValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).RawValue;

            if (rawValue == null)
                return Array.Empty<string>();

            if (rawValue is IEnumerable enumerable && rawValue.GetType() != typeof(string))
                return enumerable.Cast<string>().ToArray();

            return ((string)rawValue).Split(',');
        }

        private static ResultValueConverter GetValueConverterForType(Type type)
        {
            if (type.IsEnum)
                return v => TryParseEnum(type, v);

            if (typeof(IConvertible).IsAssignableFrom(type))
                return v => TryConvertType(type, v);

            throw new ArgumentException($"Unable to use CommaDelimitedCollection with type '{type.FullName}'");
        }

        private static Result<object> TryParseEnum(Type enumType, string value)
        {
            try
            {
                return ResultHelper.Succeed(Enum.Parse(enumType, value, true));
            }
            catch (ArgumentException)
            {
                return ResultHelper<object>.Fail<EnumMemberNotFoundError>();
            }
        }

        private static Result<object> TryConvertType(Type targetType, string value)
        {
            try
            {
                return ResultHelper.Succeed(Convert.ChangeType(value, targetType));
            }
            catch (FormatException)
            {
                return ResultHelper<object>.Fail<InvalidTypeFormatError>();
            }
        }

        public class EnumMemberNotFoundError : ResultError { }
        public class InvalidTypeFormatError : ResultError { }
    }
}
