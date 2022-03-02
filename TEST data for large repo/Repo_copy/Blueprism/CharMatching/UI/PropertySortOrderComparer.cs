using BluePrism.CharMatching.Properties;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Used to compare the Sort Order attribute of two Property Descriptor objects
    /// </summary>
    internal class PropertySortOrderComparer : IComparer
    {
        /// <summary>
        /// Compare two property descriptors, using the Sort Order attribute value
        /// against the property. If there is no Sort Order attribute against the
        /// property, then it uses 0 as the sort order value for that property.
        /// </summary>
        /// <param name="x">The first PropertyDescriptor to compare</param>
        /// <param name="y">The second PropertyDescriptor to compare</param>
        /// <returns>Returns Greater than zero if the Sort Order of x is greater than the Sort Order of y.
        /// Returns Less than zero if the Sort Order of y is greater than the Sort Order of x.
        /// Returns Zero if the Sort Order of x the same as the Sort Order of y.</returns>
        public int Compare(object x, object y)
        {
            PropertyDescriptor propertyX = x as PropertyDescriptor;
            PropertyDescriptor propertyY = y as PropertyDescriptor;

            if (propertyX == null) throw new ArgumentException(Resources.XMustBeAPropertyDescriptor, nameof(x));
            if (propertyY == null) throw new ArgumentException(Resources.YMustBeAPropertyDescriptor, nameof(y));

            SortOrderAttribute xAttr = propertyX.Attributes.OfType<SortOrderAttribute>().FirstOrDefault();
            var xOrder = (xAttr != null) ? xAttr.SortOrder : 0;

            SortOrderAttribute yAttr = propertyY.Attributes.OfType<SortOrderAttribute>().FirstOrDefault();
            var yOrder = (yAttr != null) ? yAttr.SortOrder : 0;

            return xOrder.CompareTo(yOrder);

        }

    }

}
