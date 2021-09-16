#if UNITTESTS

using System.IO;
using BluePrism.Core.Xml;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests
{
    [TestFixture]
    public class ReadableXmlDocumentTests
    {
        /// <summary>
        /// Tests that creating a readable XmlDocument with all forms which should
        /// create an empty document do so (and, more importantly, don't throw an
        /// unexpected exception)
        /// </summary>
        [Test]
        public void TestEmptyConstruction()
        {
            ReadableXmlDocument r;

            r = new ReadableXmlDocument();
            Assert.That(r.DocumentElement, Is.Null);

            r = new ReadableXmlDocument("");
            Assert.That(r.DocumentElement, Is.Null);

            r = new ReadableXmlDocument((Stream)null);
            Assert.That(r.DocumentElement, Is.Null);

            r = new ReadableXmlDocument(new MemoryStream());
            Assert.That(r.DocumentElement, Is.Null);

            r = new ReadableXmlDocument((StreamReader) null);
            Assert.That(r.DocumentElement, Is.Null);

            r = new ReadableXmlDocument(new StreamReader(new MemoryStream()));
            Assert.That(r.DocumentElement, Is.Null);
        }

        /// <summary>
        /// Basic test to ensure that an Xml document is correctly parsed from a
        /// string.
        /// </summary>
        [Test]
        public void TestLoadsXml()
        {
            var r = new ReadableXmlDocument("<test attr=\"value\">Hello</test>");
            Assert.That(r.DocumentElement.Name, Is.EqualTo("test"));
            Assert.That(r.DocumentElement.GetAttribute("attr"), Is.EqualTo("value"));
            Assert.That(r.DocumentElement.InnerText, Is.EqualTo("Hello"));
        }

        /// <summary>
        /// Basic test to ensure that an Xml document is correctly parsed from a
        /// stream.
        /// </summary>
        [Test]
        public void TestLoadsStream()
        {
            var ms = new MemoryStream();
            var w = new StreamWriter(ms);
            w.Write("<test attr=\"value\">Hello</test>");
            w.Flush();
            ms.Position = 0;

            var r = new ReadableXmlDocument(ms);
            Assert.That(r.DocumentElement.Name, Is.EqualTo("test"));
            Assert.That(r.DocumentElement.GetAttribute("attr"), Is.EqualTo("value"));
            Assert.That(r.DocumentElement.InnerText, Is.EqualTo("Hello"));
        }

        [Test]
        public void LoadXml_FromStreamReader_ReadsCorrectly()
        {
            var stream = new MemoryStream();
            var streamWriter = new StreamWriter(stream);
            streamWriter.Write("<test attr=\"value\">Hello</test>");
            streamWriter.Flush();
            stream.Position = 0;

            var streamReader = new StreamReader(stream);

            var document = new ReadableXmlDocument(streamReader);
            Assert.That(document.DocumentElement.Name, Is.EqualTo("test"));
            Assert.That(document.DocumentElement.GetAttribute("attr"), Is.EqualTo("value"));
            Assert.That(document.DocumentElement.InnerText, Is.EqualTo("Hello"));
        }
    }
}

#endif