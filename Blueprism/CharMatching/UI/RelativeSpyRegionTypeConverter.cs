using BluePrism.CharMatching.Properties;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// A custom converter used by the SpyRegion.RelativeParent property to enable
    /// selection of a region from the container in the PropertyGrid control
    /// </summary>
    public class RelativeSpyRegionTypeConverter : TypeConverter
    {
        public static string EmptyLabel = Resources.NoRegionSelected;

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
            var spyRegion = context.Instance as SpyRegion;
            string name = value as string;
            if (spyRegion != null && name != null)
            {
                if (name == EmptyLabel)
                {
                    return null;
                }
                return spyRegion.Container.SpyRegions.FirstOrDefault(x => x.Name == name);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var region = value as SpyRegion;
                    if (region != null && destinationType == typeof(string))
            {
                return region.Name;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var instance = (SpyRegion) context.Instance;
            var otherRegions = instance.Container.SpyRegions.Where(x => x != instance);
            var otherRegionNames = otherRegions.Select(x => x.Name).ToList();
            // remove any regions which would cause a circular path
            foreach (SpyRegion reg in otherRegions)
            {
                if (!IsValid(context, reg))
                {
                    otherRegionNames.Remove(reg.Name);
                }
            }
            
            otherRegionNames.Insert(0, EmptyLabel);
            var collection = new StandardValuesCollection(otherRegionNames);
            return collection;
        }

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            // ensure that circular paths between the start and end regions are invalid
            SpyRegion startReg = (SpyRegion)context.Instance;
            SpyRegion endReg = (SpyRegion)value;

            SpyRegion currentReg = endReg;

            do
            {
                if (currentReg.Name == startReg.Name)
                {
                    return false;
                }
                else
                {
                    currentReg = currentReg.RelativeParent;
                }

            } while (currentReg != null);

            return true;
        }

    }
}