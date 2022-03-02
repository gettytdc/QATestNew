using System.IO;
using System.Xml;

namespace BluePrism.Core.Xml
{
    /// <summary>
    /// Minor variation on an XmlDocument intended for reading.
    /// This ensures that the external entity resolver is disabled for the document,
    /// meaning that we are not susceptible to attacks which can spoof external
    /// entities within XML being parsed by this application.
    /// </summary>
    public class ReadableXmlDocument : XmlDocument
    {
        /// <summary>
        /// Creates a new, empty, readable XmlDocument with no external resolver.
        /// </summary>
        public ReadableXmlDocument()
        {
            XmlResolver = null;
        }

        /// <summary>
        /// Creates a new readable XmlDocument with no external resolver.
        /// </summary>
        /// <param name="xml">The XML to read into this document; a null value will
        /// create an empty Readable XmlDocument.</param>
        public ReadableXmlDocument(string xml) : this()
        {
            if (!string.IsNullOrEmpty(xml))
                LoadXml(xml);
        }

        /// <summary>
        /// Creates a new readable XmlDocument with no external resolver.
        /// </summary>
        /// <param name="file">The file to read the XML from</param>
        public ReadableXmlDocument(FileInfo file) : this()
        {
            Load(file.FullName);
        }

        /// <summary>
        /// Creates a new readable XmlDocument with no external resolver.
        /// </summary>
        /// <param name="stream">The stream from which to read the XML into the new
        /// document. A null or empty stream will create an empty Readable
        /// XmlDocument. </param>
        public ReadableXmlDocument(Stream stream) : this()
        {
            if (stream != null && (!stream.CanSeek || stream.Length > 0))
                Load(stream);
        }

        /// <summary>
        /// Creates a new readable XmlDocument with no external resolver.
        /// </summary>
        /// <param name="reader">The stream reader from which to read the XML into the new
        /// document.</param>
        public ReadableXmlDocument(StreamReader reader) : this()
        {
            if (reader != null && 
                    (!reader.BaseStream.CanSeek || reader.BaseStream.Length > 0))
                Load(reader);
        }

    }
}
