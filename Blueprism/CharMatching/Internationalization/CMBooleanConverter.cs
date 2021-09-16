using BluePrism.CharMatching.Properties;
using System;
using System.ComponentModel;

namespace BluePrism.CharMatching.Internationalization
{
    class CMBooleanConverter : BooleanConverter

    {



        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)

        {
            string localizedTrue = Resources.True;
            string localizedFalse = Resources.False;

            if (value != null && value is string)
            {
                if (localizedTrue.Equals((string)value)) return true;
                if (localizedFalse.Equals((string)value)) return false;
            }

            return base.ConvertFrom(context, culture, value);

        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)

        {

            if (destinationType == typeof(string) && value != null && value is bool)
            {

                if ((bool)value) return Resources.True;
                return Resources.False;

            }

            return base.ConvertTo(context, culture, value, destinationType);

        }

    }
}
