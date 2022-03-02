using System.Xml;

namespace BluePrism.Core.Xml
{
    /// <summary>
    /// A collection of extension methods dealing with XML handling.
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Add an attribute to an attribute collection, initialised to a particular
        /// value.
        /// </summary>
        /// <param name="this">The attribute collection to which the attribute
        /// should be added.</param>
        /// <param name="doc">The XML document which should be used to create the new
        /// attribute.</param>
        /// <param name="name">The name of the attribute to add</param>
        /// <param name="value">The value to set in the attribute.</param>
        /// <returns>The newly created and initialised attribute after it has been
        /// added to the specified attribute collection.</returns>
        public static XmlAttribute Add(
            this XmlAttributeCollection @this,
            XmlDocument doc, string name, object value)
        {
            XmlAttribute attr = doc.CreateAttribute(name);
            attr.Value = (value?.ToString() ?? "");
            @this.Append(attr);
            return attr;
        }
    }
}
