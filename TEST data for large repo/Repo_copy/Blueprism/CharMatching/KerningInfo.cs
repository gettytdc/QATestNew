using System;
using System.Collections.Generic;
using System.Xml;
using BluePrism.BPCoreLib.Collections;

namespace BluePrism.CharMatching
{

    /// <summary>
    /// Data structure allowing a less explicit representation of the kerning
    /// values for all possible pairs of characters than a direct lookup table.
    /// </summary>
    /// <remarks>The idea is to hope that most characters will conform to a 
    /// 'default value' according to some rule or another.</remarks>
    internal class ComputedKerningInfo
    {

        internal Dictionary<string, int> mKernValuesSpace = null;


        /// <summary>
        /// Internal storage for all kerning pairs.
        /// </summary>
        private Dictionary<string, int> mKernValues;

        /// <summary>
        /// Creates a new kerning info object based on the given values.
        /// </summary>
        /// <param name="KernValues"></param>
        public ComputedKerningInfo(Dictionary<string, int> KernValues)
        {
            mKernValues = KernValues;
        }

        /// <summary>
        /// Creates a new, empty kerning info object
        /// </summary>
        public ComputedKerningInfo() : this(new Dictionary<string, int>()) { }

        /// <summary>
        /// The default kerning value to be reported, where no explicit value
        /// is known.
        /// </summary>
        public int DefaultKernValue
        {
            get { return mDefaultKernValue; }
            set { mDefaultKernValue = value; }
        }

        private int mDefaultKernValue = 0;

        /// <summary>
        /// Gets the kerning value that is used for the two characters of interest.
        /// </summary>
        /// <param name="c1">The first (ie left hand) character of interest.</param>
        /// <param name="c2">The second (ie left hand) character of interest.</param>
        /// <returns>Gets the offset in pixels, between the right-hand edge of the
        /// first character, and the left-hand edge of the second. This may in fact
        /// be negative.</returns>
        public int GetKernValue(CharData c1, CharData c2)
        {
            return GetKernValue(c1.Value, c2.Value);
        }

        /// <summary>
        /// Gets the kerning value that is used for the two strings.
        /// </summary>
        /// <param name="c1">The first (ie left hand) string of interest.</param>
        /// <param name="c2">The second (ie right hand) string of interest.</param>
        /// <returns>Gets the offset in pixels, between the right-hand edge of the
        /// first string , and the left-hand edge of the second. This may in fact
        /// be negative.</returns>
        public int GetKernValue(string str1, string str2)
        {
            if (str1 == "" || str2 == "")
                return mDefaultKernValue;
            // Get the value between the last char of str1 and the first char of str2
            return GetKernValue(str1[str1.Length - 1], str2[0]);
        }

        /// <summary>
        /// Gets the kerning value that is used for the two characters of interest.
        /// </summary>
        /// <param name="c1">The first (ie left hand) character of interest.</param>
        /// <param name="c2">The second (ie left hand) character of interest.</param>
        /// <returns>Gets the offset in pixels, between the right-hand edge of the
        /// first character, and the left-hand edge of the second. This may in fact
        /// be negative.</returns>
        public int GetKernValue(char c1, char c2)
        {
            int val;
            if (mKernValues.TryGetValue(string.Concat(c1, c2), out val))
                return val;
            return mDefaultKernValue;
        }

        /// <summary>
        /// Generates xml for this kerning info.
        /// </summary>
        /// <param name="node">The element under which kerning information
        /// should be added.</param>
        public void ToXML(XmlElement node)
        {
            XmlDocument doc = node.OwnerDocument;
            //Default kern width
            XmlElement row = doc.CreateElement("defaultkernwidth");
            row.SetAttribute("kernvalue", this.mDefaultKernValue.ToString());
            node.AppendChild(row);

            //space width
            row = doc.CreateElement("spacewidth");
            row.SetAttribute("value", mSpaceWidth.ToString());
            node.AppendChild(row);

            //Compute a dictionary of kerning intervals, together with a
            //list of pairs of characters possessing that interval. Eg - 
            // 1 - ab, ac, xz, pf
            // 2 - ft, ws, iu, df,
            // 3 - etc
            Dictionary<int, List<string>> charPairs = new Dictionary<int, List<string>>();
            foreach (KeyValuePair<string, int> entry in mKernValues)
            {
                if (!charPairs.ContainsKey(entry.Value))
                {
                    charPairs.Add(entry.Value, new List<string>());
                }
                charPairs[entry.Value].Add(entry.Key);
            }

            foreach (int kern in charPairs.Keys)
            {
                row = doc.CreateElement("kernvalue");
                row.SetAttribute("value", kern.ToString());
                row.SetAttribute("letterpairs", 
                    CollectionUtil.Join(charPairs[kern], ""));
                node.AppendChild(row);
            }

            if (this.mKernValuesSpace != null) {
                charPairs = new Dictionary<int, List<string>>();
                foreach (KeyValuePair<string, int> entry in this.mKernValuesSpace)
                {
                    if (!charPairs.ContainsKey(entry.Value))
                    {
                        charPairs.Add(entry.Value, new List<string>());
                    }
                    charPairs[entry.Value].Add(entry.Key);
                }
                foreach (int kern in charPairs.Keys)
                {
                    row = doc.CreateElement("kernvalueSpace");
                    row.SetAttribute("value", kern.ToString());
                    row.SetAttribute("letterpairs", 
                        CollectionUtil.Join(charPairs[kern], ""));
                    node.AppendChild(row);
                }
            }
        }

        /// <summary>
        /// The width of the space character, in pixels, for this font. If zero, the
        /// font does not define any spacing information, and no spaces will be
        /// included in any output read using it.
        /// </summary>
        public int SpaceWidth
        {
            get { return mSpaceWidth; }
            set { mSpaceWidth = value; }
        }

        private int mSpaceWidth;

        private IDictionary<char, int> _minKernCache = new Dictionary<char, int>();

        public int GetMinKerningValueAfter(string str)
        {
            if (str == "") return 0;
            return GetMinKerningValueAfter(str[str.Length - 1]);
        }

        /// <summary>
        /// Gets the minimum kerning value for any character following the supplied
        /// character.
        /// </summary>
        /// <param name="c">The character of interest.</param>
        /// <returns>Returns the minimum kerning value encountered after this
        /// letter, or else Integer.MaxValue if no suitable information exists.
        /// </returns>
        public int GetMinKerningValueAfter(char c)
        {
            int val;
            if (!_minKernCache.TryGetValue(c, out val))
            {
                val = int.MaxValue;
                foreach (KeyValuePair<string, int> kvp in mKernValues)
                {
                    if (kvp.Key[0] == c)
                    {
                        val = Math.Min(val, kvp.Value);
                    }
                }
                _minKernCache[c] = val;
            }
            return val;
        }
    }
}
