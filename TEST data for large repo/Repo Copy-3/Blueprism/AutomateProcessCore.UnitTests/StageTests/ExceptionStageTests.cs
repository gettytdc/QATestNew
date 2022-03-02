#if UNITTESTS
using System.Xml;
using System.Xml.Linq;
using BluePrism.AutomateProcessCore.Stages;
using BluePrism.BPCoreLib;
using NUnit.Framework;

namespace AutomateProcessCore.UnitTests.StageTests
{
    [TestFixture]
    public class ExceptionStageTests
    {
        [TestCase("10.5", "en-GB", "10.5", "10.5")]
        [TestCase("10,5", "de-DE", "10,5", "10,5")]
        [TestCase("RndDn(0.1,2)", "en-GB", "RndDn(0.1,2)", "RndDn(0.1,2)")]
        [TestCase("RndDn(0,1;2)", "de-DE", "RndDn(0,1;2)", "RndDn(0,1;2)")]
        [TestCase("Replace(&quot;greenday&quot;, &quot;green&quot;, &quot;blue&quot;)", "en-GB", "Replace(\"greenday\", \"green\", \"blue\")", "Replace(\"greenday\", \"green\", \"blue\")")]
        [TestCase("Replace(&quot;greenday&quot;, &quot;green&quot;, &quot;blue&quot;)", "de-DE", "Replace(\"greenday\", \"green\", \"blue\")", "Replace(\"greenday\", \"green\", \"blue\")")]
        [TestCase("Replace(&quot;greenday&quot;; &quot;green&quot;; &quot;blue&quot;)", "de-DE", "Replace(\"greenday\"; \"green\"; \"blue\")", "Replace(\"greenday\"; \"green\"; \"blue\")")]
        public void ExceptionStageTestFromToXml(string expression, string lang, string expected, string exceptiondetail)
        {
            using (new CultureBlock(lang))
            {
                TestFromToXmlImpl(expression, expected, exceptiondetail);
            }
        }

        public void TestFromToXmlImpl(string expression, string expected, string exceptionDetail)
        {
            var stageXML = XElement.Parse($"<stage stageid=\"37230c78-99b3-48d5-bdf5-db72323714b2\" name=\"Exception1\" type=\"Exception\">\r\n                           <display x=\"0\" y=\"0\"/>\r\n                           <exception type=\"\" detail=\"{expression}\"/>\r\n                       </stage>").ToXmlElement();
            var stage = new clsExceptionStage(null);
            stage.FromXML(stageXML);
            var doc = new XmlDocument();
            var resultXML = doc.CreateElement("stage");
            stage.ToXml(doc, resultXML, false);
            Assert.That(stageXML.OuterXml, Is.EqualTo(resultXML.OuterXml), "FromToXml failed for expression: " + expression);
            Assert.AreEqual(stage.ExceptionLocalized, "no", "ExceptionLocalized failed");
            Assert.AreEqual(stage.ExceptionDetail, expected, "ExceptionDetail mismatch");
            stage.ExceptionDetailForLocalizationSetting = exceptionDetail;
            Assert.AreEqual(stage.ExceptionDetail, expected, "ExceptionDetail mismatch");
            var str = stage.ExceptionDetailForLocalizationSetting;
            Assert.AreEqual(str, exceptionDetail, "ExceptionDetail mismatch");
        }

        [TestCase("10.5", "en-GB", "10.5", "10.5")]
        [TestCase("10.5", "de-DE", "10.5", "10,5")]
        [TestCase("RndDn(0.1,2)", "en-GB", "RndDn(0.1,2)", "RndDn(0.1,2)")]
        [TestCase("RndDn(0.1,2)", "de-DE", "RndDn(0.1,2)", "RndDn(0,1;2)")]
        [TestCase("Replace(&quot;greenday&quot;, &quot;green&quot;, &quot;blue&quot;)", "en-GB", "Replace(\"greenday\", \"green\", \"blue\")", "Replace(\"greenday\", \"green\", \"blue\")")]
        [TestCase("Replace(&quot;greenday&quot;, &quot;green&quot;, &quot;blue&quot;)", "de-DE", "Replace(\"greenday\", \"green\", \"blue\")", "Replace(\"greenday\"; \"green\"; \"blue\")")]
        public void ExceptionStageTestFromToXmlLocalized(string expression, string lang, string expected, string exceptiondetail)
        {
            using (new CultureBlock(lang))
            {
                TestFromToXmlImplLocalized(expression, expected, exceptiondetail);
            }
        }

        public void TestFromToXmlImplLocalized(string expression, string expected, string exceptionDetail)
        {
            var stageXML = XElement.Parse($"<stage stageid=\"37230c78-99b3-48d5-bdf5-db72323714b2\" name=\"Exception1\" type=\"Exception\"><display x=\"0\" y=\"0\"/><exception localized=\"yes\" type=\"\" detail=\"{expression}\"/></stage>").ToXmlElement();
            var stage = new clsExceptionStage(null);
            stage.FromXML(stageXML);
            var doc = new XmlDocument();
            var resultXML = doc.CreateElement("stage");
            stage.ToXml(doc, resultXML, false);
            Assert.That(stageXML.OuterXml, Is.EqualTo(resultXML.OuterXml), "FromToXml failed for expression: " + expression);
            Assert.AreEqual(stage.ExceptionLocalized, "yes", "ExceptionLocalized failed");
            Assert.AreEqual(stage.ExceptionDetail, expected, "ExceptionDetail mismatch");
            stage.ExceptionDetailForLocalizationSetting = exceptionDetail;
            Assert.AreEqual(stage.ExceptionDetail, expected, "ExceptionDetail mismatch");
            var str = stage.ExceptionDetailForLocalizationSetting;
            Assert.AreEqual(str, exceptionDetail, "ExceptionDetail mismatch");
        }

        [TestCase("Replace(&quot;greenday&quot;; &quot;green&quot;;&quot;blue&quot;)", "de-DE", "Replace(\"greenday\"; \"green\";\"blue\")", "Replace(\"greenday\", \"green\",\"blue\")")]
        public void ExceptionStageTestFromToXmlLocalizedInvalid(string expression, string lang, string expected, string exceptiondetail)
        {
            using (new CultureBlock(lang))
            {
                TestFromToXmlImplLocalizedInvalid(expression, expected, exceptiondetail);
            }
        }

        public void TestFromToXmlImplLocalizedInvalid(string expression, string expected, string exceptionDetail)
        {
            var stageXML = XElement.Parse($"<stage stageid=\"37230c78-99b3-48d5-bdf5-db72323714b2\" name=\"Exception1\" type=\"Exception\">\r\n                           <display x=\"0\" y=\"0\"/>\r\n                           <exception localized=\"yes\" type=\"\" detail=\"{expression}\"/>\r\n                       </stage>").ToXmlElement();
            var stage = new clsExceptionStage(null);
            stage.FromXML(stageXML);
            var doc = new XmlDocument();
            var resultXML = doc.CreateElement("stage");
            stage.ToXml(doc, resultXML, false);
            Assert.That(stageXML.OuterXml, Is.EqualTo(resultXML.OuterXml), "FromToXml failed for expression: " + expression);
            Assert.AreEqual(stage.ExceptionLocalized, "yes", "ExceptionLocalized failed");
            Assert.AreEqual(stage.ExceptionDetail, expected, "ExceptionDetail mismatch");
            stage.ExceptionDetailForLocalizationSetting = exceptionDetail;
            Assert.AreEqual(stage.ExceptionDetail, exceptionDetail, "ExceptionDetail mismatch");
            var str = stage.ExceptionDetailForLocalizationSetting;
            Assert.AreEqual(str, expected, "ExceptionDetail mismatch");
        }
    }
}
#endif
