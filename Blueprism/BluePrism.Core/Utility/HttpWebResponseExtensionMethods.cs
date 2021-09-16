using BluePrism.Core.Properties;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BluePrism.BPCoreLib;

namespace BluePrism.Core.Utility
{
    using BluePrism.Server.Domain.Models;
    using Utilities.Functional;

    /// <summary>
    /// Extension methods for the <see cref="HttpWebResponse"/> class and other 
    /// supporting extension methods
    /// </summary>
    public static class HttpWebResponseExtensionMethods
    {
        /// <summary>
        /// A collection of encodings that may be prefixed with a byte order mark (BOM).
        /// A byte order mark can be used to determine the encoding that was used to 
        /// encode a text stream.
        /// </summary>
        private static IEnumerable<Encoding> EncodingsWithByteOrderMarks = 
            new Encoding[] { Encoding.UTF8, Encoding.UTF32, Encoding.Unicode, Encoding.BigEndianUnicode };

      
        /// <summary>
        /// Get the body of the response as string, decoded using the correct encoding
        /// type
        /// </summary>
        /// <param name="this">The Http Web Response to get the body response of</param>
        /// <returns>The response body as a string</returns>
        public static string GetResponseBodyAsString(this HttpWebResponse @this)
        {
            byte[] response;

            using (var memoryStream = new MemoryStream())
            using(var responseStream = @this.GetResponseStream())
            {
                responseStream.CopyTo(memoryStream);
                response = memoryStream.ToArray();
            }
                        
            (Encoding type, int byteOrderMarkLength) responseEncoding;
            var charSet = @this.CharacterSet;

            responseEncoding = 
                string.IsNullOrEmpty(charSet) ? response.GetEncodingFromByteOrderMarkPrefix() 
                                                      : response.GetEncodingFromName(charSet);
                
            // Decode the response bytes using the correct encoding, excluding the
            // byte order mark at the start of the response
           return  responseEncoding.type.GetString(response, 
                                                   responseEncoding.byteOrderMarkLength, 
                                                   response.Length - responseEncoding.byteOrderMarkLength);

        }

        /// <summary>
        /// Get the encoding details of a byte array, using the name of its encoding
        /// type
        /// </summary>
        /// <param name="this">The bytes to get the encoding details of</param>
        /// <param name="encodingName">The name of the byte array's encoding type</param>
        /// <returns>A tuple containing the encoding type of the byte array and the
        /// length of any byte order mark prefix at the start of the response</returns>
        public static (Encoding encoding, int byteOrderMarkLength) 
            GetEncodingFromName(this byte[] @this, string encodingName)
        {
            Encoding encoding;
            try
            {
                encoding = Encoding.GetEncoding(encodingName);
                return (encoding, @this.ByteOrderMarkPrefixLength(encoding.GetPreamble()));
            }
            catch
            {
                throw new InvalidValueException(String.Format(Resource.HttpWebResponseExtensionMethods_InvalidEncodingType0, encodingName));
            }                
            
        }

        /// <summary>
        /// Get the encoding details of a byte array by checking for any byte order 
        /// mark that prefixes the array. If a byte order mark isn't found, assume 
        /// it is encoded using UTF-8.
        /// </summary>
        /// <param name="this">The bytes to get the encoding details of</param>
        /// <returns>A tuple containing the encoding type of the byte array and the
        /// length of any byte order mark prefix at the start of the response</returns>
        public static (Encoding encoding, int byteOrderMarkLength) 
            GetEncodingFromByteOrderMarkPrefix(this byte[] @this)
        {
            return EncodingsWithByteOrderMarks
                        .Select((x) => new { encoding = x, byteOrderMark = x.GetPreamble() })
                        .Where((x) => @this.StartsWithByteOrderMark(x.byteOrderMark))
                        .Select((x) => (x.encoding, x.byteOrderMark.Length))
                        .FirstOrDefault()
                        .Map((x) => !x.Equals(default(ValueTuple<Encoding, int>))
                                    ? x
                                    : (Encoding.UTF8, 0));
        }


        /// <summary>
        /// Checks whether byte array is prefixed with the specified byte order mark
        /// </summary>
        /// <param name="this">The byte array to check for the byte order mark prefix
        /// </param>
        /// <param name="byteOrderMark">The byte order mark</param>
        /// <returns>
        /// True, if the byte array is prefixed with the specified byte order mark
        /// </returns>
        public static bool StartsWithByteOrderMark(this byte[] @this, byte[] byteOrderMark)
        {
            return byteOrderMark?.SequenceEqual(@this.Take(byteOrderMark.Length)) ?? false;
        }

        /// <summary>
        /// If the byte array is prefixed with the byte order mark then return the 
        /// length of the byte order mark. Otherwise, return 0.
        /// </summary>
        /// <param name="byteArray">The bytes array to check for the byte order mark
        /// prefix</param>
        /// <param name="byteOrderMark">The byte order mark</param>
        /// <returns>The length of the byte order mark that prefixes the specified 
        /// byte array</returns>
        public static int ByteOrderMarkPrefixLength(this byte[] @this, byte[] byteOrderMark)
        {
            return @this.StartsWithByteOrderMark(byteOrderMark) ? byteOrderMark.Length : 0;
        }       



    }
}
