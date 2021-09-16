#if UNITTESTS
using System.Xml;
using System.Xml.Linq;
using BluePrism.AutomateProcessCore.Stages;
using BluePrism.BPCoreLib;
using NUnit.Framework;

namespace AutomateProcessCore.UnitTests.StageTests
{
    internal static class XElementExtensions
    {
        public static XmlElement ToXmlElement(this XElement el)
        {
            var doc = new XmlDocument();
            doc.Load(el.CreateReader());
            return doc.DocumentElement;
        }
    }

    [TestFixture]
    public class WriteStageTests
    {
        [TestCase("Replace(&quot;a-b-c&quot;, &quot;-&quot;, &quot;.&quot;)")]
        [TestCase("10.5")]
        [TestCase("RndDn(0.1,2)")]
        public void TestFromToXml(string expression)
        {
            using (new CultureBlock("da-DK"))
            {
                TestFromToXmlImpl(expression);
            }

            using (new CultureBlock("en-GB"))
            {
                TestFromToXmlImpl(expression);
            }
        }

        public void TestFromToXmlImpl(string expression)
        {
            var stageXML = XElement.Parse("<stage stageid=\"37230c78-99b3-48d5-bdf5-db72323714b2\" name=\"Write Data\" type=\"Write\">\r\n   <display x=\"0\" y=\"0\"/>\r\n   <step expr=\"&lt;%= expression %&gt;\">\r\n\t   <element id=\"a313ba8c-88d6-4a0f-a810-aed4edf55b17\"/>\r\n   </step>\r\n</stage>").ToXmlElement();
            var stage = new clsWriteStage(null);
            stage.FromXML(stageXML);
            var doc = new XmlDocument();
            var resultXML = doc.CreateElement("stage");
            stage.ToXml(doc, resultXML, false);
            Assert.That(stageXML.OuterXml, Is.EqualTo(resultXML.OuterXml), "FromToXml failed for expression: " + expression);
        }
    }
}
#endif
