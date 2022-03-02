using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using BluePrism.Core.Xml;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// Class to fully encapsulate a Blue Prism font - ie. a named set of characters
    /// and related information.
    /// </summary>
    public class BPFont
    {
        // The name of the font on the database, null if it is not saved
        private string _dbName;

        // The name of the font
        private string _name;

        // The version of the font
        private string _ver;

        // The data describing the font
        private FontData _data;

        /// <summary>
        /// Creates a new font from the given XML document
        /// </summary>
        /// <param name="doc">The document from which to draw the font</param>
        private BPFont(XmlDocument doc)
            : this("xml-input-font", null, new FontData())
        {
            FromXml(doc);
        }

        /// <summary>
        /// Creates a new empty, unversioned font with the given name
        /// </summary>
        /// <param name="name">The name to give the font</param>
        public BPFont(string name)
            : this(name, null, new FontData()) { }

        /// <summary>
        /// Creates a new BP Font based on the given name and encoded data.
        /// </summary>
        /// <param name="name">The name of the new font</param>
        /// <param name="data">The data representing the characters and their
        /// relationships from the font</param>
        public BPFont(string name, string data)
            : this(name, null, new FontData(data)) { }

        /// <summary>
        /// Creates a new BP Font based on the given name and data.
        /// </summary>
        /// <param name="name">The name of the new font</param>
        /// <param name="data">The data representing the characters and their
        /// relationships from the font</param>
        public BPFont(string name, FontData data)
            : this(name, null, data) { }

        /// <summary>
        /// Creates a new BP Font based on the given properties
        /// </summary>
        /// <param name="name">The name of the new font</param>
        /// <param name="ver">The version of the font</param>
        /// <param name="fontDataXml">The data representing the characters and their
        /// relationships from the font, encoded in XML format</param>
        public BPFont(string name, string ver, string fontDataXml)
            : this(name, ver, new FontData(fontDataXml)) { }

        /// <summary>
        /// Creates a new BP Font based on the given name, version and data.
        /// </summary>
        /// <param name="name">The name of the new font</param>
        /// <param name="ver">The version of the font</param>
        /// <param name="data">The data representing the characters and their
        /// relationships from the font</param>
        public BPFont(string name, string ver, FontData data)
            : this(null, name, ver, data) { }

        /// <summary>
        /// Creates a new BP Font based on the given name, version and data.
        /// </summary>
        /// <param name="dbName">The name of this font on the database</param>
        /// <param name="name">The name of the new font</param>
        /// <param name="ver">The version of the font</param>
        /// <param name="fontDataXml">The data representing the characters and their
        /// relationships from the font, encoded in XML format</param>
        public BPFont(string dbName, string name, string ver, string fontDataXml)
            : this(dbName, name, ver, new FontData(fontDataXml)) { }

        /// <summary>
        /// Creates a new BP Font based on the given name, version and data.
        /// </summary>
        /// <param name="name">The name of the new font</param>
        /// <param name="ver">The version of the font</param>
        /// <param name="data">The data representing the characters and their
        /// relationships from the font</param>
        public BPFont(string dbName, string name, string ver, FontData data)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException(
                "name", Resources.NameCannotBeEmpty);

            if (data == null) throw new ArgumentException(
                "data", Resources.DataMustBeProvidedForAFont);

            _dbName = dbName;
            _name = name;
            _ver = (ver ?? "1.0");
            _data = data;
        }

        /// <summary>
        /// Creates a new font object from the given file
        /// </summary>
        /// <param name="inputFile">The fileinfo object representing the file from
        /// which to draw the font</param>
        /// <exception cref="IOException">If there was a problem with the given
        /// file / path or any IO errors occurred while attempting to load the file.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">If filename specified a
        /// file that is read-only -or- this operation is not supported on the
        /// current platform.-or- filename specified a directory -or- the caller
        /// does not have the required permission. </exception>
        /// <exception cref="XmlException">If the given file was not a valid XML
        /// file.</exception>
        /// <exception cref="InvalidFontXmlException">If the file represented a valid
        /// XML file, but not a font file.</exception>
        public BPFont(FileInfo inputFile)
        {
            ImportData(inputFile.FullName);
        }

        /// <summary>
        /// The name of the font
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// The name of this font on the database - may be different to
        /// <see cref="Name"/> if it has been renamed since being loaded.
        /// </summary>
        public string DbName
        {
            get { return _dbName; }
            set { _dbName = value; }
        }

        /// <summary>
        /// The version of the font - "Unversioned" if no version is set
        /// </summary>
        public string Version
        {
            get { return (_ver ?? "Unversioned"); }
            set { _ver = (value == "Unversioned" ? null : value); }
        }

        /// <summary>
        /// The data which describes the characters and their relationships
        /// </summary>
        public FontData Data
        {
            get { return _data; }
        }

        /// <summary>
        /// The characters defined in the font
        /// </summary>
        public ICollection<CharData> CharacterData
        {
            get { return _data.Characters; }
        }

        /// <summary>
        /// Gets whether this font is empty - ie. has no characters set within it
        /// </summary>
        public bool IsEmpty
        {
            get { return (_data.Characters.Count == 0); }
        }

        /// <summary>
        /// Deep clones this font object.
        /// </summary>
        /// <returns>A deep clone of this font object.</returns>
        public BPFont Clone()
        {
            // Rather a naive implementation of a clone - a round trip to XML
            BPFont f = new BPFont(ToXml());
            f.DbName = DbName;
            return f;
        }

        /// <summary>
        /// Merges the given characters with this font
        /// </summary>
        /// <param name="data">The char data objects to merge with this font</param>
        public void Merge(ICollection<CharData> data)
        {
            foreach (CharData cd in data)
            {
                _data.AddCharacter(cd, true);
            }
        }

        /// <summary>
        /// Removes the given CharData object from this font
        /// </summary>
        /// <param name="cd">The data to remove</param>
        public void Remove(CharData cd)
        {
            _data.RemoveCharacter(cd);
        }

        /// <summary>
        /// Removes all of the given CharData instances from this font
        /// </summary>
        /// <param name="data">The data to remove from this font</param>
        public void RemoveAll(ICollection<CharData> data)
        {
            foreach (CharData cd in data)
                Remove(cd);
        }

        /// <summary>
        /// Exports this font to the given file
        /// </summary>
        /// <param name="filename">The name of the file to which this font should be
        /// exported.</param>
        public void ExportData(string filename)
        {
            ToXml().Save(filename);
        }

        /// <summary>
        /// Imports the data from the given filename into this font object.
        /// </summary>
        /// <param name="filename"></param>
        /// <exception cref="ArgumentException">If filename is a zero-length string,
        /// contains only white space, or contains one or more invalid characters as
        /// defined by <see cref="Path.InvalidPathChars"/></exception>
        /// <exception cref="ArgumentNullException">If filename is null</exception>
        /// <exception cref="IOException">If there was a problem with the given
        /// file / path or any IO errors occurred while attempting to load the file.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">If this operation is not
        /// supported on the current platform -or- filename specified a directory
        /// -or- the caller does not have the required permission. </exception>
        /// <exception cref="XmlException">If the given file was not a valid XML
        /// file.</exception>
        /// <exception cref="InvalidFontXmlException">If the file represented a valid
        /// XML file, but not a font file.</exception>
        public void ImportData(string filename)
        {
            // Load the text into an XML document
            XmlDocument doc = new ReadableXmlDocument();
            doc.Load(filename);
            FromXml(filename, doc);
        }

        /// <summary>
        /// Writes this font into an XML document object.
        /// </summary>
        /// <returns>The XML document to which this font has been written</returns>
        private XmlDocument ToXml()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("bpfont");
            root.SetAttribute("name", _name);
            root.SetAttribute("version", _ver);
            doc.AppendChild(root);

            XmlElement dataEl = doc.CreateElement("fontcharacters");
            root.AppendChild(dataEl);

            _data.ToXml(dataEl);
            return doc;
        }

        /// <summary>
        /// Initialises the data in this font to that found in the given XML
        /// document.
        /// </summary>
        /// <param name="doc">The document from which this font's data should be
        /// drawn</param>
        /// <exception cref="InvalidFontXmlException">If the file represented a valid
        /// XML file, but not a font file.</exception>
        private void FromXml(XmlDocument doc)
        {
            FromXml(null, doc);
        }

        /// <summary>
        /// Initialises the data in this font to that found in the given XML
        /// document.
        /// </summary>
        /// <param name="filename">The filename which provided this XML, if
        /// appropriate. Null if it is not from a file</param>
        /// <param name="doc">The document from which this font's data should be
        /// drawn</param>
        /// <exception cref="InvalidFontXmlException">If the file represented a valid
        /// XML file, but not a font file.</exception>
        private void FromXml(string filename, XmlDocument doc)
        {
            // Check the root element
            // 'bpfont' indicates that it contains the name, ver & data;
            // 'fontcharacters' indicates that it contains ver & data only.
            // Anything else means it's not a font.
            XmlElement root = doc.DocumentElement;
            XmlElement dataEl;
            if (root.Name == "bpfont")
            {
                _name = root.Attributes["name"].Value;
                _ver = root.Attributes["version"].Value;
                dataEl = root["fontcharacters"];
            }
            else if (root.Name == "fontcharacters")
            {
                // Intuit the font name from the filename
                _name = Path.GetFileNameWithoutExtension(filename);
                if (_name.StartsWith("font_"))
                    _name = _name.Substring(5);
                // Clear the version for now - we'll use the one from FontData.
                _ver = null;
                dataEl = root;
            }
            else
            {
                throw new InvalidFontXmlException(
                    Resources.TheGivenXMLFileCouldNotBeParsedIntoAFontRootElementFound0, root.Name);
            }

            FontData fd = new FontData();
            fd.FromXml(dataEl);
            // Only update the version if we're importing a legacy font file,
            // not if we're importing one recorded as a BPFont file
            if (root == dataEl)
                Version = fd.Version;
            _data = fd;

        }
    }
}
