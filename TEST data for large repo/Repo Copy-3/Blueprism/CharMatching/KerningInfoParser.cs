using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;
using BluePrism.Server.Domain.Models;

namespace BluePrism.CharMatching
{
    internal abstract class KerningInfoParser
    {
        /// <summary>
        /// The regex which detects kern info in its old format within XML.
        /// </summary>
        private static Regex __legacyKernPattern =
            new Regex("default(?:before|after)kern");

        /// <summary>
        /// Generates a kerning info object from its xml representation.
        /// </summary>
        /// <param name="InfoElement">The xml element under which kerning info is
        /// found.</param>
        /// <param name="ParentFont">>The parent font which will own
        /// the resulting ComputedKerningInfo
        /// object.</param>
        internal abstract ComputedKerningInfo FromXML(
            XmlElement InfoElement, FontData ParentFont);

        /// <summary>
        /// Gets the kerning info parser suitable for the given XML.
        /// If the XML contains 'defaultbeforekern' or 'defaultafterkern' then a
        /// legacy parser is returned, otherwise a default parser is returned.
        /// </summary>
        /// <param name="xml">The XML for which a kern info parser is required
        /// </param>
        /// <returns>A KerningInfoParser suitable for handling the given XML.
        /// </returns>
        internal static KerningInfoParser GetParser(string xml)
        {
            if (__legacyKernPattern.Matches(xml).Count > 0)
                return new LegacyKerningInfoParser();
            return new DefaultKerningInfoParser();
        }
    }

    /// <summary>
    /// Parser for legacy fontinfo xml format. This was a silly arrangement of
    /// "default" values coming after char X, with exceptions printed afterwards.
    /// The resulting format was more verbose than just writing out all pairs.
    /// </summary>
    /// <remarks></remarks>
    internal class LegacyKerningInfoParser : KerningInfoParser
    {
        /// <summary>
        /// The value to be used as the kerning value for two characters where
        /// no result is given by mKernDefaultsAfter, mKernDefaultsBefore or mKernExceptions.
        /// </summary>
        private int mDefaultKernValue;
        /// <summary>
        /// An explicit lookup table of kerning values for pairs of characters whose
        /// kerning value does not conform to the 'before' or 'after' rule, and is not
        /// equal to the default kerning value.
        /// </summary>
        private Dictionary<string, int> mKernExceptions = new Dictionary<string, int>();

        /// <summary>
        /// A lookup of DEFAULT kerning values for characters which appear
        /// before a particular letter. Keyed by letter; the value is the default
        /// kerning value for characters appearing immediately before this letter.
        /// </summary>
        /// <remarks>For example, if the key "a" returns 3, then that means that
        /// the default value of a letter ("b" say) appearing before "a" (ie as "ba")
        /// is 3. Exceptions to this rule are listed in mKernExceptions.</remarks>
        private Dictionary<char, int> mKernDefaultsBefore = new Dictionary<char, int>();

        /// <summary>
        /// A lookup of DEFAULT kerning values for characters which appear
        /// after a particular letter. Keyed by letter; the value is the default
        /// kerning value for characters appearing immediately after this letter.
        /// </summary>
        /// <remarks>For example, if the key "a" returns 3, then that means that
        /// the default value of a letter ("b" say) appearing after "a" (ie as "ab")
        /// is 3. Exceptions to this rule are listed in mKernExceptions.</remarks>
        private Dictionary<char, int> mKernDefaultsAfter = new Dictionary<char, int>();

        /// <summary>
        /// Gets the kerning value that is used for the two characters of interest.
        /// </summary>
        /// <param name="c1">The first (ie left hand) character of interest.</param>
        /// <param name="c2">The second (ie right hand) character of interest.</param>
        /// <returns>Gets the offset in pixels, between the right-hand edge of the
        /// first character, and the left-hand edge of the second. This may in fact
        /// be negative.</returns>
        public int GetKernValue(CharData c1, CharData c2)
        {
            return GetKernValue(c1.Value, c2.Value);
        }

        /// <summary>
        /// Gets the kerning value that is used between two strings
        /// </summary>
        /// <param name="str1">The first (ie left hand) string of interest.</param>
        /// <param name="str2">The second (ie right hand) string of interest.</param>
        /// <returns>Gets the offset in pixels, between the right-hand edge of the
        /// first string, and the left-hand edge of the second. This may in fact be
        /// negative.</returns>
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
        /// <returns>Gets the offset in pixels, between the right-hand edge of the first
        /// character, and the left-hand edge of the second. This may in fact be
        /// negative.</returns>
        public int GetKernValue(char c1, char c2)
        {
            string str = string.Concat(c1, c2);
            int val;
            if (mKernExceptions.TryGetValue(str, out val))
                return val;

            if (mKernDefaultsAfter.TryGetValue(c1, out val))
                return val;

            if (mKernDefaultsBefore.TryGetValue(c2, out val))
                return val;

            return mDefaultKernValue;
        }

        /// <summary>
        /// Generates a kerning info object from its xml representation.
        /// </summary>
        /// <param name="InfoElement">The xml element under which kerning info is found.</param>
        internal override ComputedKerningInfo FromXML(XmlElement InfoElement, FontData ParentFont)
        {
            int SpaceWidth = 0;
            foreach (XmlElement infochild in InfoElement.ChildNodes)
            {
                switch (infochild.Name)
                {
                    case "defaultkernwidth":
                        mDefaultKernValue = ElementParser.ParseElement<int>(
                            infochild.Attributes.GetNamedItem("kernvalue").Value,
                            string.Format(Resources.InvalidKernvalueFor0, infochild.Name), int.TryParse);
                        break;
                    case "spacewidth":
                        SpaceWidth = ElementParser.ParseElement<int>(infochild.Attributes.GetNamedItem("value").Value,
                            string.Format(Resources.InvalidValueFor0, infochild.Name), int.TryParse);
                        break;
                    case "defaultbeforekern":
                    {
                        mKernDefaultsBefore.Add(ElementParser.ParseElement<char>(infochild.Attributes.GetNamedItem("letter").Value,
                            string.Format(Resources.InvalidLetterFor0, infochild.Name), char.TryParse), ElementParser.ParseElement<int>(
                            infochild.Attributes.GetNamedItem("kernvalue").Value,
                            string.Format(Resources.InvalidKernvalueFor0, infochild.Name), int.TryParse));
                    }
                    break;
                    case "defaultafterkern":
                    {
                        mKernDefaultsAfter.Add(ElementParser.ParseElement<char>(infochild.Attributes.GetNamedItem("letter").Value,
                            string.Format(Resources.InvalidLetterFor0, infochild.Name), char.TryParse), ElementParser.ParseElement<int>(
                            infochild.Attributes.GetNamedItem("kernvalue").Value,
                            string.Format(Resources.InvalidKernvalueFor0, infochild.Name), int.TryParse));
                    }
                    break;
                    case "kernexception":
                    {
                        mKernExceptions.Add(infochild.Attributes.GetNamedItem("letterpair").Value,
                                                            ElementParser.ParseElement<int>(infochild.Attributes.GetNamedItem("kernvalue").Value,
                                                    string.Format(Resources.InvalidKernvalueFor0, infochild.Name),
                                                        int.TryParse));
                    }
                    break;
                }
            }
            Dictionary<string, int> D = new Dictionary<string, int>();
            foreach (CharData Ch1 in ParentFont.Characters)
            {
                foreach (CharData Ch2 in ParentFont.Characters)
                {
                    D.Add(Ch1 + Ch2, GetKernValue(Ch1, Ch2));
                }
            }
            ComputedKerningInfo Retval = new ComputedKerningInfo(D);
            Retval.SpaceWidth = SpaceWidth;
            return Retval;
        }

    }

    /// <summary>
    /// Parser for current fontinfo xml format. This is simpler and more
    /// compact than the above legacy format.
    /// </summary>
    internal class DefaultKerningInfoParser : KerningInfoParser
    {
        /// <summary>
        /// Generates a kerning info object from its xml representation.
        /// </summary>
        /// <param name="infoEl">The xml element under which kerning info is found.
        /// </param>
        /// <param name="font">>The parent font which will own the resulting
        /// ComputedKerningInfo object.</param>
        internal override ComputedKerningInfo FromXML(XmlElement infoEl, FontData font)
        {
            int defaultKern = 0;
            Dictionary<string, int> kernValues = new Dictionary<string, int>();
            int width = 0;

            foreach (XmlElement el in infoEl.ChildNodes) {
                string valueStr;
                switch (el.Name) {
                    case "defaultkernwidth":
                        valueStr = el.GetAttribute("kernvalue");
                        if (!int.TryParse(valueStr, out defaultKern)) {
                            throw new InvalidValueException(
                                Resources.InvalidKernvalueFor01, el.Name, valueStr);
                        }
                        break;

                    case "kernvalue":{
                        int val = 0;
                        valueStr = el.GetAttribute("value");
                        if (!int.TryParse(valueStr, out val)) {
                            throw new InvalidValueException(
                                Resources.InvalidValueAttributeFor01, el.Name, valueStr);
                        }
                        string pairs = el.GetAttribute("letterpairs");
                        if (pairs.Length % 2 != 0) {
                            throw new InvalidFormatException(
                                Resources.InvalidFileFormatExpectedEvenNumberOfLetterPairs);
                        }
                        for (int i = 0; i < pairs.Length - 1; i += 2)
                        {
                            kernValues[pairs.Substring(i, 2)] = val;
                        }
                        break;
                        }

                    case "spacewidth":
                        valueStr = el.GetAttribute("value");
                        if (!int.TryParse(valueStr, out width)) {
                            throw new InvalidValueException(
                                Resources.InvalidValueFor01, el.Name, valueStr);
                        }
                        break;
                }
            }
            ComputedKerningInfo kernInfo = new ComputedKerningInfo(kernValues);
            kernInfo.DefaultKernValue = defaultKern;
            kernInfo.SpaceWidth = width;
            return kernInfo;
        }
    }
}
