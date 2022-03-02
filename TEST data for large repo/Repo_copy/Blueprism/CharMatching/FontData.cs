using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml;
using System.Text.RegularExpressions;
using BluePrism.BPCoreLib.Collections;
using BluePrism.BPCoreLib;
using BluePrism.Core.Xml;
using BluePrism.Server.Domain.Models;
using System.Linq;

namespace BluePrism.CharMatching
{
    public class FontData
    {
        public ISet<int> Underline { get; set; } = new HashSet<int>();
        public ISet<int> Strikeout { get; set; } = new HashSet<int>();
        public void SetKernValuesSpace(Dictionary<string, int> Values)
        {
            this.mKernInfo.mKernValuesSpace = Values;
        }


        #region - Class Scope Declarations -

        // The pattern to search for which indicates that the legacy kerning
        // format is used in the XML file being parsed.
        private static readonly Regex LegacyKernPattern =
            new Regex(".*default(?:before|after)kern");

        #endregion

        #region - Member Variables -

        // The kerning info for this font.
        private ComputedKerningInfo mKernInfo =
            new ComputedKerningInfo(new Dictionary<string, int>());

        // The characters representing this font
        private ICollection<CharData> _chars;

        // The version of this font. Universally pointless
        private string _version;

        // The number of blank lines to allow before assuming a new line of text
        private int _lineBreakThreshold = 1;

        // Variables for caching character properties in a font
        private int _cachedWhiteSpaceBelow = -1;
        private int _cachedWhiteSpaceAbove = -1;
        private int _cachedHeight;
        private ICollection<ICollection<CharData>> _cachedConflicts;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Construct a new empty font.
        /// </summary>
        public FontData() : this(null) { }

        /// <summary>
        /// Construct a new font from the given XML document.
        /// </summary>
        /// <param name="xml">The XML representation of the font, in the format
        /// created by GetXML().</param>
        public FontData(string xml)
        {
            _chars = new clsAutoSortedList<CharData>();
            _version = "unversioned";

            if (xml != null)
            {
                XmlDocument doc = new ReadableXmlDocument(xml);

                XmlElement root = doc.DocumentElement;
                if (root.Name != "fontcharacters") throw new BluePrismException(
                    Resources.NotAValidFontDefinitionFoundRootNode0, root.Name);

                FromXml(root);
            }
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The characters defined in this font. Never add to this list, always use
        /// AddCharacter, as the font must be properly sorted.
        /// </summary>
        public ICollection<CharData> Characters
        {
            get { return _chars; }
        }

        /// <summary>
        /// The version of this font data.
        /// </summary>
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }

        /// <summary>
        /// The width of the space character, in pixels, for this font. If zero, the
        /// font does not define any spacing information, and no spaces will be included
        /// in any output read using it.
        /// </summary>
        public int SpaceWidth
        {
            get { return this.mKernInfo.SpaceWidth; }
            set { this.mKernInfo.SpaceWidth = value; }
        }

        /// <summary>
        /// The number of blank rows of pixels that should be observed before deciding
        /// that any remaining content resides on a new line.
        /// </summary>
        /// <returns>Returns a value of 1 or more, indicating the required number of rows
        /// of blank pixels</returns>
        /// <remarks>This is usually 1, but in some fonts the "_" character (for example)
        /// can stoop very low such that there is a blank row between it and the bottom
        /// of any other character. In such cases, the LineBreakThreshold would be more
        /// than one.</remarks>
        public int LineBreakThreshold
        {
            get { return this._lineBreakThreshold; }
        }

        /// <summary>
        /// Gets the maximum white space above any character in the font.
        /// </summary>
        public int MaxWhiteSpaceAbove
        {
            get
            {
                if (_cachedWhiteSpaceAbove == -1)
                {
                    int ws = -1;
                    foreach (CharData c in _chars)
                    {
                        ws = Math.Max(ws, c.Mask.CountEmptyLines(Direction.Top));
                    }
                    _cachedWhiteSpaceAbove = ws;
                }
                return _cachedWhiteSpaceAbove;
            }
        }

        /// <summary>
        /// Gets the minimum amount of whitespace found above characters in this font
        /// </summary>
        public int MinWhiteSpaceAbove
        {
            get
            {
                int ws = int.MaxValue;
                foreach (CharData cd in _chars)
                    ws = Math.Min(ws, cd.Mask.CountEmptyLines(Direction.Top));
                return ws;
            }
        }

        /// <summary>
        /// Gets the maximum white space below any character in the font.
        /// </summary>
        public int MaxWhiteSpaceBelow
        {
            get
            {
                if (_cachedWhiteSpaceBelow == -1)
                {
                    int ws = -1;
                    foreach (CharData c in this._chars)
                    {
                        ws = Math.Max(ws, c.Mask.CountEmptyLines(Direction.Bottom));
                    }
                    _cachedWhiteSpaceBelow = ws;
                }
                return _cachedWhiteSpaceBelow;
            }
        }

        /// <summary>
        /// Gets the minimum amount of whitespace found below characters in this font
        /// </summary>
        public int MinWhiteSpaceBelow
        {
            get
            {
                int ws = int.MaxValue;
                foreach (CharData cd in _chars)
                    ws = Math.Min(ws, cd.Mask.CountEmptyLines(Direction.Bottom));
                return ws;
            }
        }

        /// <summary>
        /// Gets the (maximum) height of the font.
        /// </summary>
        public int Height
        {
            get
            {
                if (_cachedHeight == 0)
                {
                    int maxHeight = 0;
                    foreach (CharData ch in this._chars)
                    {
                        maxHeight = Math.Max(maxHeight, ch.Height);
                    }
                    _cachedHeight = maxHeight;
                }
                return _cachedHeight;
            }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Gets the minimum kerning value for any character following the supplied
        /// character.
        /// </summary>
        /// <param name="str">The string of interest.</param>
        public int GetMinKerningValueAfter(string str)
        {
            return mKernInfo.GetMinKerningValueAfter(str);
        }

        /// <summary>
        /// Loads this font data from the given XML element.
        /// </summary>
        /// <param name="node">The XML node which represents the parent node for
        /// this font data and from which the data should be parsed.</param>
        internal void FromXml(XmlElement node)
        {
            _chars.Clear();
            _version = "unversioned";

            foreach (XmlElement el in node.ChildNodes)
            {
                switch (el.Name)
                {
                    case "character":
                        string str = (el.GetAttribute("char") ?? CharData.NullValue);
                        AddCharacter(
                            new CharData(str, el.GetAttribute("encoded"),
                             el.GetAttribute("statemask")), false
                        );
                        break;

                    case "fontinfo":
                        mKernInfo =
                            KerningInfoParser.GetParser(el.OuterXml).FromXML(el, this);
                        break;

                    case "version":
                        _version = el.FirstChild.Value;
                        break;
                }
            }
        }

        /// <summary>
        /// Appends an XML representation of this font data to the given XML element.
        /// </summary>
        /// <param name="node">The XmlElement to which this font data should be
        /// appended.</param>
        internal void ToXml(XmlElement node)
        {
            XmlDocument doc = node.OwnerDocument;

            XmlElement v = doc.CreateElement("version");
            v.AppendChild(doc.CreateTextNode(_version));
            node.AppendChild(v);

            foreach (CharData fc in _chars)
            {
                XmlElement row = doc.CreateElement("character");
                row.SetAttribute("char", fc.CharacterString);
                row.SetAttribute("encoded", fc.EncodedMask);
                row.SetAttribute("statemask", fc.EncodedStateMask);
                node.AppendChild(row);
            }

            //This stuff has to go last, because the xml SetXML function
            //above assumes that the fontcharacters is always
            //the root element
            XmlElement infoEl = doc.CreateElement("fontinfo");

            if (this.Underline.Count > 0) {
                XmlElement e = doc.CreateElement("underline");
                e.SetAttribute("y_min", this.Underline.Min().ToString());
                e.SetAttribute("y_max", this.Underline.Max().ToString());
                infoEl.AppendChild(e);
            }
            if (this.Strikeout.Count > 0) {
                XmlElement e = doc.CreateElement("strikeout");
                e.SetAttribute("y_min", this.Strikeout.Min().ToString());
                e.SetAttribute("y_max", this.Strikeout.Max().ToString());
                infoEl.AppendChild(e);
            }

            node.AppendChild(infoEl);

            //kerning stuff
            mKernInfo.ToXML(infoEl);
        }

        /// <summary>
        /// Get a formatted XML representation of the font data.
        /// </summary>
        /// <returns>The XML font data, formatted for readability.</returns>
        public string GetXML()
        {
            return GetXML(false);
        }

        /// <summary>
        /// Get an XML representation of the font data.
        /// </summary>
        /// <param name="compact">If True, the XML will be in compact (no line breaks
        /// or indentation) format - otherwise it will be 'pretty'.</param>
        /// <returns>The XML font data.</returns>
        public string GetXML(bool compact)
        {
            XmlDocument doc = new XmlDocument();

            XmlElement element = doc.CreateElement("fontcharacters");
            doc.AppendChild(element);
            ToXml(element);

            if (compact)
                return doc.OuterXml;

            // Return output all nicely formatted if so instructed
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            doc.WriteContentTo(XmlWriter.Create(sb, settings));
            return sb.ToString();

        }

        /// <summary>
        /// Gets a list of all matching character groups.
        /// </summary>
        /// <returns>Returns a list of matching character groups, or an empty list if
        /// none are found.
        /// 
        /// A matching character group consists of two or more characters, which are
        /// all identical in appearance.</returns>
        public ICollection<ICollection<CharData>> GetConflictingCharacterGroups()
        {
            if (_cachedConflicts != null)
                return _cachedConflicts;

            List<CharData> allChars = new List<CharData>(_chars);
            List<ICollection<CharData>> conflicts = new List<ICollection<CharData>>();

            for (int i = allChars.Count - 1; i >= 1; i--)
            {
                CharData refChar = allChars[i];
                List<CharData> matches = new List<CharData>();
                matches.Add(refChar);

                for (int j = i - 1; j >= 0; j--)
                {
                    CharData testChar = allChars[j];
                    if (testChar.Mask == refChar.Mask)
                        matches.Add(testChar);
                }

                if (matches.Count > 1)
                    conflicts.Add(matches);
            }

            _cachedConflicts = conflicts;
            return conflicts;
        }

        /// <summary>
        /// Gets a list of characters which share the same visual representation in this font
        /// as the supplied character. This includes the supplied character itself.
        /// </summary>
        /// <param name="c">The character of interest, which must be from this font.</param>
        /// <returns>Returns a list of characters which share the same visual representation in this font
        /// as the supplied character. This includes the supplied character itself,
        /// (though the object reference may not be the same).</returns>
        public ICollection<CharData> GetCharacterAlternatives(CharData c)
        {
            if (c == null)
                throw new ArgumentNullException(nameof(c));

            foreach (ICollection<CharData> gp in GetConflictingCharacterGroups())
            {
                foreach (CharData testchar in gp)
                {
                    if (c.IsEquivalentChar(testchar))
                        return gp;
                }
            }

            return GetSingleton.ICollection(c);
        }

        /// <summary>
        /// Add a character to the font.
        /// </summary>
        /// <param name="fc">The CharData to add.</param>
        /// <param name="replaceExisting">If True, and there is already a character
        /// in the font with the same character code, it will be replaced. Otherwise
        /// the new character will be added as an alternate variant.</param>
        public void AddCharacter(CharData fc, bool replaceExisting)
        {
            //Remove an existing character with the same mapping, if present.
            if (replaceExisting)
            {
                foreach (CharData c in _chars)
                {
                    if (fc.IsEquivalentChar(c))
                    {
                        _chars.Remove(c);
                        break;
                    }
                }
            }

            //Add the new character and sort the font.
            _chars.Add(fc);
        }

        /// <summary>
        /// Removes a character from the font
        /// </summary>
        /// <param name="cd">The CharData object representing the character to remove
        /// from this font</param>
        public void RemoveCharacter(CharData cd)
        {
            _chars.Remove(cd);
        }

        /// <summary>
        /// Determines which character, if any, exists at the specified position on
        /// the supplied canvas.
        /// </summary>
        /// <param name="Offset">The offset from which to start analysing the canvas.
        /// </param>
        /// <param name="Canvas">The canvas on which to test for a character.</param>
        /// <param name="iCharWidth">Carries back the width of the character
        /// identified at the specified position, if any, or zero if none identified.
        /// </param>
        /// <returns>Returns the strnig value found at the specified position, or
        /// <see cref="CharData.NullValue"/>, if none is found.</returns>
        public string GetTextValueAtPosition(
            Point offset, CharCanvas canv, out int charWidth)
        {
            bool nonStrict; // not used in any meaningful way

            CharData cc = null;
            foreach (CharData chr in _chars)
            {
                cc = chr;
                if (cc.IsAtPosition(offset, canv, out nonStrict, true, true))
                {
                    charWidth = cc.Width;
                    return cc.Value;
                }
            }
            charWidth = 0;
            return CharData.NullValue;
        }

        /// <summary>
        /// Determines which character, if any, exists at the specified position on the
        /// supplied canvas.
        /// </summary>
        /// <returns>Returns the character found at the specified position, or nothing,
        /// if none found.</returns>
        public CharData GetCharAtPosition(Point offset, CharCanvas canvas,
            out bool nonStrict, bool strictLeft, bool strictRight)
        {
            nonStrict = false;
            foreach (CharData cc in _chars)
            {
                if (cc.IsAtPosition(offset, canvas, out nonStrict, strictLeft, strictRight))
                    return cc;
            }

            return null;
        }

        /// <summary>
        /// Gets the kerning value that is used for the two characters of interest.
        /// </summary>
        /// <param name="c1">The first (ie left hand) character of interest.
        /// </param>
        /// <param name="c2">The second (ie left hand) character of interest.</param>
        /// <returns>Gets the offset in pixels, between the right-hand edge of the
        /// first character, and the left-hand edge of the second. This may in fact
        /// be negative.</returns>
        public int GetKernValue(CharData c1, CharData c2)
        {
            return mKernInfo.GetKernValue(c1, c2);
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
            return mKernInfo.GetKernValue(c1, c2);
        }

        /// <summary>
        /// Records the kerning values used for pairs of characters. This is
        /// the offset in pixels between the right hand edge of the first character,
        /// and the left hand edge of the second character (which may in fact be
        /// negative - it is acceptable for some characters to overlap).
        /// </summary>
        /// <param name="Values">A lookup table of kerning values, keyed as two-character
        /// strings, "ab".</param>
        public void SetKernValues(Dictionary<string, int> Values)
        {
            int oldSpaceWidth = 0;
            if (mKernInfo != null)
                oldSpaceWidth = mKernInfo.SpaceWidth;
            this.mKernInfo = new ComputedKerningInfo(Values);
            this.mKernInfo.SpaceWidth = oldSpaceWidth;
        }

        #endregion
    }
}
