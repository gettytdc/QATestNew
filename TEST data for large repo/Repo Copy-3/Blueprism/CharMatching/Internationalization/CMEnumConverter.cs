using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using BluePrism.CharMatching.Properties;

namespace BluePrism.CharMatching.Internationalization
{
    public class CMEnumConverter : EnumConverter
    {
        Dictionary<string, object> reverseConversionDictionary;

        public CMEnumConverter(Type type)
            : base(type)
        {
            reverseConversionDictionary = new Dictionary<string, object>();
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string && reverseConversionDictionary.ContainsKey(value as string))
            {
                
                    return reverseConversionDictionary[value as string];
                
            }
                return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value != null && destinationType == typeof(string))
            {

                var resxKey = Regex.Replace(value.ToString(), @"\b(\w)+\b",
                    x => x.Value[0].ToString().ToUpper() + x.Value.Substring(1));
                resxKey = "CMEnum_" + Regex.Replace(resxKey, @"[^a-zA-Z0-9]*", "");
                var localized = Resources.ResourceManager.GetString($"{resxKey}");

                if (localized == null) localized = value.ToString();

                // In order to convert the String back to the enum value, store it in a dictionary.
                // As the string needs to be generated first before converted back, the dictionary will be filled.
                // Faster than reverse lookup in resx files.
                reverseConversionDictionary[localized] = value;
                return localized;
            }
                return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
