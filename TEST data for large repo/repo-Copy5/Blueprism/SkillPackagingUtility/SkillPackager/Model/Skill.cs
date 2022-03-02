using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using SkillPackager.Cryptography;

namespace SkillPackager.Model
{
    public class Skill
    {
        private readonly string _releaseXmlContents;
        private readonly SkillConfiguration _configuration;

        public Skill(string releaseXml, SkillConfiguration configuration)
        {
            _releaseXmlContents = releaseXml;
            _configuration = configuration;

            ValidateWebApi();
        }

        private void ValidateWebApi()
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(_releaseXmlContents)))
            using (var streamReader = new StreamReader(stream))
            using (var reader = new XmlTextReader(streamReader))
            {
                var xml = XDocument.Load(reader);
                var selectedWebApiId = _configuration.SelectedWebApi.ToString();

                if (!IsSelectedWebApiInRelease(xml, selectedWebApiId))
                    throw new Exception(SkillPackagerResources.Error_WebApiNotInRelease);
            }
        }

        private static bool IsSelectedWebApiInRelease(XContainer document, string selectedWebApiId)
        {
            var webapis = document.Descendants().Where(d => d.Name.LocalName == "webapiservice");
            return webapis.Any(a => HasSingleIdAttributeOf(a, selectedWebApiId));
        }

        private static bool HasSingleIdAttributeOf(XElement element, string selectedWebApiId)
        {
            var idAttributes = element.Attributes("id").ToList();

            if (idAttributes.Count() != 1)
                throw new Exception("Web API must have one attribute for Id.");

            return idAttributes.Single().Value.Equals(selectedWebApiId, StringComparison.OrdinalIgnoreCase);
        }

        public string Export()
        {
            try
            {
                return Encryptor.Encrypt(ToXml());
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(SkillPackagerResources.Error_ExportingSkill_Template, e.Message));
            }
        }

        private string ToXml()
        {
            var doc = new XmlDocument();
            doc.LoadXml(_releaseXmlContents);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("bpr", "http://www.blueprism.co.uk/product/release");
            var contentsNode = doc.SelectNodes("bpr:release/bpr:contents", nsmgr)[0];

            var skillDoc = new XmlDocument();
            skillDoc.LoadXml(_configuration.ToXml());

            XmlNode skill = doc.ImportNode(skillDoc.DocumentElement, true);
            contentsNode.InsertAfter(skill, contentsNode.LastChild);
            ((XmlElement)contentsNode).SetAttribute("count", $"{contentsNode.ChildNodes.Count}");

            return doc.OuterXml;
        }
    }
}
