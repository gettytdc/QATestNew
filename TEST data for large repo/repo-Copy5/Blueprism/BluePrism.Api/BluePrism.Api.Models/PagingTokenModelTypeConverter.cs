namespace BluePrism.Api.Models
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public class PagingTokenModelTypeConverter<T> : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
            sourceType == typeof(PagingTokenModel<T>) ||
            sourceType == typeof(string) ||
            base.CanConvertFrom(context, sourceType);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string s)
            {
                return (PagingTokenModel<T>)s;
            }

            if(value == null)
            {
                return null;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
            destinationType == typeof(string)
                ? (value as PagingTokenModel<T>)?.ToString()
                : base.ConvertTo(context, culture, value, destinationType);
    }

    public class PagingTokenModelTypeDescriptor : CustomTypeDescriptor
    {
        private readonly Type _objectType;

        public PagingTokenModelTypeDescriptor(Type objectType) => _objectType = objectType;

        public override TypeConverter GetConverter()
        {
            var genericArg = _objectType.GenericTypeArguments[0];
            var converterType = typeof(PagingTokenModelTypeConverter<>).MakeGenericType(genericArg);
            return (TypeConverter)Activator.CreateInstance(converterType);
        }
    }

    public class PagingTokenModelTypeDescriptionProvider : TypeDescriptionProvider
    {
        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance) => new PagingTokenModelTypeDescriptor(objectType);
    }
}
