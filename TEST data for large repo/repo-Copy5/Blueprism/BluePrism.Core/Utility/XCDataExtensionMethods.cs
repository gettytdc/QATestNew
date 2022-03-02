using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace BluePrism.Core.Utility
{
    /// <summary>
    /// Contains Extension methods for working with <see cref="XCData"/> instances
    /// </summary>
    public static class XCDataExtensionMethods
    {
        /// <summary>
        /// Returns an <see cref="IEnumerable{XCData}"/> that breaks the <see cref="XCData"/>
        /// object into <see cref="XCData"/> chunks, so that no single chunk contains
        /// the illegal sequence of chars: ]]>. If the value does not contain this
        /// sequence of chars, then the original <see cref="XCData"/> object is 
        /// returned as an IEnumerable.
        /// </summary>
        /// <param name="this">The <see cref="XCData"/> instance to break up into valid chunks</param>
        /// <returns>
        /// Returns an <see cref="IEnumerable{XCData}"/> containing valid <see cref="XCData"/>
        /// instances
        /// </returns>
        /// <remarks>In XML, CDATA sections are not allowed to contain the following
        /// sequence of chars, ]]>. You can create an XCData object containing these
        /// chars, but when you try and create an XML document containing this
        /// XCData object the document will be invalid. There is no escaping of the chars
        /// but instead you need to break the CDATA section into multiple sections.
        /// For example: 
        /// <![CDATA[Certain tokens like ]]> can be difficult and <invalid>]]> 
        /// should be written as:
        /// <![CDATA[Certain tokens like]]]]><![CDATA[> can be difficult and <invalid>]]> 
        /// </remarks>        
        public static IEnumerable<XCData> ToEscapedEnumerable(this XCData @this)
        {
            var cData = @this.Value;

            if (!cData.Contains("]]>"))
                yield return @this;               
            else
            {
                var i = 0;
                while (i > -1)
                {
                    i = cData.IndexOf("]]>");
                    yield return new XCData(cData.Substring(0, i + 2));                                                                   
                    cData = cData.Substring(i + 2, cData.Length - (i + 2));
                    
                };

                yield return new XCData(cData);

            }

        }

        /// <summary>
        /// Returns the concatenated values of an <see cref="IEnumerable{XCData}"/>
        /// </summary>
        /// <param name="this">
        /// The <see cref="IEnumerable{XCData}"/> to concatenate the values of
        /// </param>
        /// <returns>
        /// Returns the concatenated values of an <see cref="IEnumerable{XCData}"/>
        /// </returns>
        /// <remarks>
        /// This can be used as a helper method, to get the concatenated values of
        /// <see cref="XCData"/> instances escaped using 
        /// <see cref="XCDataExtensionMethods.ToEscapedEnumerable(XCData)"/>
        /// </remarks>
        public static string GetConcatenatedValue(this IEnumerable<XCData> @this)
        {
            return string.Join("", @this.Select(x => x.Value));
        }

    }
}
