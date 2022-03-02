using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using BluePrism.CharMatching.Properties;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// A custom converter used by the SpyRegion.RegionPosition property to restrict
    /// the available values that can be selected depending on othe proeprties
    /// 
    /// </summary>
    public class RegionPositionTypeConverter : TypeConverter
    {
        Dictionary<string, object> reverseConversionDictionary;

        public RegionPositionTypeConverter()
            : base()
        {
            reverseConversionDictionary = new Dictionary<string, object>();
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string && reverseConversionDictionary.ContainsKey(value as string))
            {

                return reverseConversionDictionary[value as string];

            }

            //if not in dictionary, fallback to original and see if it is the enum itself.
            string name = value as string;
            RegionPosition position;
            if (name != null && Enum.TryParse(name, out position) == true) return position;

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value != null && destinationType == typeof(string))
            {
                var resxKey = Regex.Replace(value.ToString(), @"\b(\w)+\b",
                    x => x.Value[0].ToString().ToUpper() + x.Value.Substring(1));
                resxKey = "RegionPosition_" + Regex.Replace(resxKey, @"[^a-zA-Z0-9]*", "");
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

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var instance = (SpyRegion)context.Instance;
            
            var props = Enum.GetValues(typeof(RegionPosition))
                .OfType<RegionPosition>()
                .Where(a => (instance.LocationMethod == RegionLocationMethod.Coordinates) ? a != RegionPosition.Anywhere : true)
                .ToList();

            return new StandardValuesCollection(props);
                    
        }

        
    }
}