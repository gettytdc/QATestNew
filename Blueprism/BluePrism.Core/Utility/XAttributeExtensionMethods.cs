using BluePrism.Core.Properties;
using System;
using System.Xml.Linq;
using System.ComponentModel;
using BluePrism.BPCoreLib;
using BluePrism.Server.Domain.Models;

namespace BluePrism.Core.Utility
{
    /// <summary>
    /// Contains Extension methods for working with <see cref="XAttribute"/> instances
    /// </summary>
    public static class XAttributeExtensionMethods
    {
        /// <summary>
        /// Gets the value of attribute value and returns it as type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to convert the attribute value to</typeparam>
        /// <param name="this">
        /// The <see cref="XAttribute"/> instance to convert the value of
        /// </param>
        /// <exception cref="InvalidValueException">
        /// Throws exception if attribute value cannot be converted to 
        /// <typeparamref name="T"/>
        /// </exception>
        /// <returns>
        /// Returns the attribute value as type <typeparamref name="T"/>
        /// </returns>
        public static T Value<T>(this XAttribute @this)
        {
            try
            {
                var x = (T)TypeDescriptor.GetConverter(typeof(T)).
                    ConvertFromString(@this.Value);
                return x;
            }                                  
            catch 
            {
                throw new InvalidValueException(
                    Resource.XAttributeExtensionMethods_TheValue0ForTheXMLAttribute1CannotBeConvertedTo2, 
                    @this.Value, @this.Name, typeof(T).ToString());
            }
        }


        /// <summary>
        /// Gets the value of attribute value and returns it as a <see cref="Uri"/>
        /// instance. Use this instead of <see cref="Value{T}(XAttribute)"/> if you
        /// want to throw an exception when the attribute value is not a valid Uri
        /// </summary>
        /// <param name="this">
        /// The <see cref="XAttribute"/> instance whose value you create a 
        /// <see cref="Uri"/> from
        /// </param>
        /// <exception cref="InvalidValueException">
        /// Throws exception if a <see cref="Uri"/> cannot be created from the 
        /// attribute value 
        /// </exception>
        /// <returns>
        /// Returns the attribute value as an instance of <see cref="Uri"/>
        /// </returns>
        public static Uri ValueAsUri(this XAttribute @this)
        {
            try
            {
                return new Uri(@this.Value);
            }
            catch 
            {
                throw new InvalidValueException(
                    Resource.XAttributeExtensionMethods_TheValue0ForTheXMLAttribute1IsNotAValidURI,
                    @this.Value, @this.Name);
            }
        }

    }
}
