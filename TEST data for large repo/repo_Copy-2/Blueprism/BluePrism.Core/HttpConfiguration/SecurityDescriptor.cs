using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BluePrism.Core.HttpConfiguration
{
    /// <summary>
    /// Contains permissions assigned to one or more users or groups within a URL
    /// Reservation
    /// </summary>
    /// <remarks>
    /// See the following for background on format and values that apply to URL ACL:
    /// https://msdn.microsoft.com/en-us/library/windows/desktop/aa364653(v=vs.85).aspx
    /// </remarks>
    public class SecurityDescriptor
    {
        /// <summary>
        /// Creates an instance of SecurityDescriptor from an SSDL string
        /// </summary>
        /// <param name="input">The SSDL string to parse</param>
        public static SecurityDescriptor Parse(string input)
        {
            var result = SsdlParser.ParseInternal(input);
            if (result == null)
            {
                throw new FormatException("Invalid SSDL string");
            }
            return result;
        }

        /// <summary>
        /// Determines whether a string contains a valid security descriptor definition 
        /// language (SSDL) value
        /// </summary>
        /// <param name="input">The value to test</param>
        /// <param name="descriptor">The SecurityDescriptor initialised from the input 
        /// supplied</param>
        /// <returns>A value indicating whether the input was parsed successfully
        /// </returns>
        public static bool TryParse(string input, out SecurityDescriptor descriptor)
        {
            descriptor = SsdlParser.ParseInternal(input);
            return descriptor != null;
        }

        /// <summary>
        /// Creates a new SecurityDescriptor
        /// </summary>
        /// <param name="entries">The Access Control Entries belonging to the 
        /// descriptor</param>
        public SecurityDescriptor(IEnumerable<AccessControlEntry> entries)
        {
            Entries = entries.ToList().AsReadOnly();
            SsdlString = "D:" + string.Concat(Entries.Select(x => x.AceString));
        }

        /// <summary>
        /// The SSDL string for this SecurityDescriptor
        /// </summary>
        public string SsdlString { get; private set; }

        /// <summary>
        /// The individual permissions assigned for the URL
        /// </summary>
        public IList<AccessControlEntry> Entries { get; private set; }

        public override string ToString()
        {
            return string.Format("SsdlString: {0}", SsdlString);
        }

        #region Parser

        /// <summary>
        /// Inner class used to parse SSDL strings
        /// </summary>
        private class SsdlParser
        {
            private static readonly string AcePattern 
                = @"\(A;;(?<rights>G[AXW]);;;(?<sid>[a-zA-Z-0-9-]+)\)";
            private static readonly string SsdlPattern 
                = string.Format(@"D:(?<entry>{0})+", AcePattern);

            /// <summary>
            /// Creates an instance of SecurityDescriptor from an SSDL string
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static SecurityDescriptor ParseInternal(string input)
            {
                var result = Regex.Match(input, SsdlPattern);
                if (!result.Success)
                {
                    return null;
                }
                var entries = ParseEntries(result);
                return new SecurityDescriptor(entries);
            }

            private static IEnumerable<AccessControlEntry> ParseEntries(Match result)
            {
                // Matches indexed left to right and are flat in structure so
                // we can iterate through captures
                int entryCount = result.Groups["entry"].Captures.Count;
                for (int index = 0; index < entryCount; index++)
                {
                    string rights = result.Groups["rights"].Captures[index].Value;
                    bool allowListen = rights == "GA" || rights == "GX";
                    bool allowDelegate = rights == "GA" || rights == "GW";
                    string sid = result.Groups["sid"].Captures[index].Value;
                    yield return new AccessControlEntry(sid, allowListen, allowDelegate);
                }
            }
        }

        #endregion
    }
}