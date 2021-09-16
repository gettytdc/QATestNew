using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Windows.Forms;
using AutomateControls.Localisation;
using BluePrism.Server.Domain.Models;

namespace AutomateControls
{
    /// <summary>
    /// Extension methods for ListControls
    /// </summary>
    public static class ListControlExtensions
    {
        /// <summary>
        /// Gets the SelectedValue property cast to the specified type, returning the
        /// default value for the type if SelectedValue is null. Note that the
        /// ValueMember property of the control should be set to "Tag" for
        /// SelectedValue to return the value of the selected item's Tag property.
        /// </summary>
        /// <typeparam name="T">The type to cast the SelectedValue to</typeparam>
        /// <param name="control">The ComboBoxItem instance</param>
        /// <returns>The SelectedValue cast to the specified type or the default 
        /// value for the type if SelectedValue is null</returns>
        public static T GetSelectedValueOrDefault<T>(this ListControl control)
        {
            return control.GetSelectedValueOrDefault(default(T));
        }

        /// <summary>
        /// Gets the ComboBox SelectedValue property cast to the specified type,
        /// returning a default value if SelectedValue is null. Note that the
        /// ValueMember property of the control should be set to "Tag" for
        /// SelectedValue to return the value of the selected item's Tag property.
        /// </summary>
        /// <typeparam name="T">The type to cast the SelectedValue to</typeparam>
        /// <param name="control">The ComboBoxItem instance</param>
        /// <param name="default">The default value to return</param>
        /// <returns>The SelectedValue cast to the specified type or the default 
        /// value if SelectedValue is null</returns>
        public static T GetSelectedValueOrDefault<T>(this ListControl control, T @default)
        {
            object value = control.SelectedValue;
            if (value == null)
            {
                return @default;
            }
            if (!typeof(T).IsInstanceOfType(value))
            {
                throw new InvalidTypeException("Expected the SelectedValue to be of type {0} but was {1}", 
                    typeof(T), value.GetType());
            }
            return (T)value;
        }

        /// <summary>
        /// Sets the DataSource property of a ListControl to a sequence of ComboBoxItems 
        /// created from the specified values with the value as the Tag property and a 
        /// localised text value as the Text property. The text is obtained from a 
        /// string resource whose name is formatted from the template and value of each 
        /// tag (using <see cref="string.Format(string, object)"/>). Note that the
        /// DisplayMember of the ListControl should be set to Text to display the text
        /// value from the ComboBox item. The ValueMember should be set to Tag to ensure
        /// that the SelectedValue property returns the underlying value.
        /// </summary>
        /// <param name="control">The ListControl to update</param>
        /// <param name="resources">The ResourceManager containing the string resource to
        /// use for each text property</param>
        /// <param name="template">The resource name template</param>
        /// <param name="tags">The tags for which items will be created</param>
        /// <param name="getName">The function used to get the name for each tag that will 
        /// be inserted into the template (optional). If not specified, then the name will
        /// be obtained from each value using <see cref="Object.ToString()"/>.</param>
        public static void BindToLocalisedItems<T>(this ListControl control, 
            ResourceManager resources, 
            string template, 
            IEnumerable<T> tags, 
            Func<T,string> getName = null)
        {
            var items = ComboBoxLocalisationHelper.CreateItems(resources, template, tags, getName);
            control.DataSource = items.ToList();
        }

        /// <summary>
        /// Sets the datasource of a ListControl to a sequence of ComboBoxItems with 
        /// localised text values and tags that are based on the values from an 
        /// enumeration. The text is obtained from a string resource whose name is 
        /// formatted from the template and value of the tag (using 
        /// <see cref="string.Format(string, object)"/>). The item order will be 
        /// based on the values of the enumeration constants. Note that the
        /// DisplayMember of the ListControl should be set to Text to display the
        /// text value from the ComboBox item. The ValueMember should be set to Tag
        /// to ensure that the SelectedValue property returns the underlying value.
        /// </summary>
        /// <param name="control">The ListControl to update</param>
        /// <param name="resources">The ResourceManager containing the string resource to
        /// use for the text property</param>
        /// <param name="template">The resource name template</param>
        public static void BindToLocalisedEnumItems<T>(this ListControl control, 
            ResourceManager resources, string template)
        {
            var items = ComboBoxLocalisationHelper.CreateEnumItems<T>(resources, template);
            control.DataSource = items.ToList();
        }
    }
}
