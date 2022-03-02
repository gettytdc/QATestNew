using System.Resources;
using BluePrism.BPCoreLib;
using BluePrism.Server.Domain.Models;

namespace BluePrism.Core.Utility
{
    /// <summary>
    /// Extension methods for <see cref="ResourceManager"/>
    /// </summary>
    public static class ResourceManagerExtensions
    {
        /// <summary>
        /// Gets a string value from a <see cref="ResourceManager"/> based on a 
        /// templated resource name. The resource name is created from the 
        /// template and values using the <see cref="string.Format(System.String, object[])" /> 
        /// method.
        /// </summary>
        /// <param name="resources">The ResourceManager containing the resource</param>
        /// <param name="template">The template used</param>
        /// <param name="values">The values inserted into the template</param>
        /// <returns>The value of the string resource or null if a resource is
        /// not found with the resulting name</returns>
        public static string GetString(this ResourceManager resources, string template, params object[] values)
        {
            return GetStringInternal(resources, template, values).Item2;
        }

        public static (string, string) GetStringInternal(ResourceManager resources, string template, object[] values)
        {
            string name = string.Format(template, values);
            string value = resources.GetString(name);
            return (name, value);
        }

        /// <summary>
        /// Gets a string value from a <see cref="ResourceManager"/> based on a 
        /// templated resource name. The resource name is created from the 
        /// template and values using the <see cref="string.Format(System.String, object[])" /> 
        /// method.
        /// </summary>
        /// <param name="resources">The ResourceManager containing the resource</param>
        /// <param name="template">The template used</param>
        /// <param name="values">The values inserted into the template</param>
        /// <returns>The value of the string resource</returns>
        /// <exception cref="MissingResourceException">Thrown if the resource does
        /// not exist based on the name</exception>
        public static string EnsureString(this ResourceManager resources, string template, params object[] values)
        {
            (string name, string value) = GetStringInternal(resources, template, values);
            if (value == null)
            {
                throw new MissingResourceException(@"Could not find resource ""{0}""", name);
            }
            return value;
        }
    }
}
