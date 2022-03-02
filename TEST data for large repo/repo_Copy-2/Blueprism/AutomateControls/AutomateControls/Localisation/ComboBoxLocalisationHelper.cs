using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using BluePrism.Core.Utility;

namespace AutomateControls.Localisation
{
    /// <summary>
    /// Contains helper functions for localising ComboBox controls
    /// </summary>
    public static class ComboBoxLocalisationHelper
    {
        /// <summary>
        /// Creates a ComboBoxItem from the specified value with the value as the Tag 
        /// property and a localised text value as the Text property. The text is 
        /// obtained from a string resource whose name is formatted from the template 
        /// and name of the value (using <see cref="string.Format(string, object)"/>).
        /// </summary>
        /// <param name="resources">The ResourceManager containing the string resource to
        /// use for the text property</param>
        /// <param name="template">The resource name template</param>
        /// <param name="value">The value used to create the ComboBoxItem</param>
        /// <param name="valueName">The name of the value used with the template to create
        /// the full resource name. If not supplied, the string value will be obtained
        /// from the value using the <see cref="Object.ToString"/> method</param>
        /// <returns>A ComboBoxItem for the specified value</returns>
        public static ComboBoxItem CreateItem(ResourceManager resources, string template, 
            object value, string valueName = null)
        {
            if (resources == null) throw new ArgumentNullException(nameof(resources));
            if (template == null) throw new ArgumentNullException(nameof(template));
            if (value == null) throw new ArgumentNullException(nameof(value));

            valueName = valueName ?? value.ToString();
            string text = resources.EnsureString(template, valueName);
            return new ComboBoxItem(text, value);
        }

        /// <summary>
        /// Creates a sequence of ComboBoxItems from the specified values with the value as
        /// the Tag property and a localised text value as the Text property. The text is 
        /// obtained from a string resource whose name is formatted from the template and 
        /// each value (using <see cref="string.Format(string, object)"/>).
        /// </summary>
        /// <param name="resources">The ResourceManager containing the string resource to
        /// use for the text property</param>
        /// <param name="template">The resource name template</param>
        /// <param name="values">The values for which items will be created</param>
        /// <param name="getName">The function used to get the name for each tag that will 
        /// be inserted into the template (optional). If not specified, then the name will
        /// be obtained from each value using <see cref="Object.ToString()"/>.</param>
        /// <returns>A sequence of ComboBoxItems based on the specified tags </returns>
        public static IEnumerable<ComboBoxItem> CreateItems<T>(ResourceManager resources, 
            string template, IEnumerable<T> values, Func<T,string> getName = null)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            return values.Select(value =>
            {
                string valueName = getName?.Invoke(value);
                return CreateItem(resources, template, value, valueName);
            });
        }

        /// <summary>
        /// Creates a sequence of ComboBoxItems with localised text values and tags that 
        /// are based on the values from an enumeration. The text is obtained from a string 
        /// resource whose name is formatted from the template and value of each enum item 
        /// (using <see cref="string.Format(string, object)"/>). The items will be 
        /// sorted based on the binary values of the enumeration constants.
        /// </summary>
        /// <param name="resources">The ResourceManager containing the string resource to
        /// use for the text property</param>
        /// <param name="template">The resource name template</param>
        /// <returns>A sequence of ComboBoxItems based on the enumeration values</returns>
        public static IEnumerable<ComboBoxItem> CreateEnumItems<T>(ResourceManager resources, 
            string template)
        {
            var enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("The type specified is not an enumeration", nameof(T));
            }
            var values = Enum.GetValues(enumType).Cast<T>().ToArray();
            return CreateItems(resources, template, values);
        }
    }
}