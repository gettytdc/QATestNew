using System;
using System.ComponentModel;

namespace BluePrism.Core.Utility
{
    public static class AttributeExtensionMethods
    {
        // Returns the value of the Description attribute on the enum if it exists.
        public static string ToDescription(this Enum value)
        {
            var descriptionAttribute = (DescriptionAttribute[])(value.GetType().GetField(value.ToString())).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return descriptionAttribute.Length > 0 ? descriptionAttribute[0].Description : value.ToString();
        }
    }
}
