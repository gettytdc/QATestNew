using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using SkillPackager.Enum;
using SkillPackager.Cryptography;
using System.Drawing;

namespace SkillPackager.Model
{
    public class SkillConfiguration
    {
        public Guid SelectedWebApi;
        public Guid ProductId;
        public int CategoryId;
        public string VersionNumber;
        public string Name;
        public string Description;
        public byte[] Icon;
        public string BluePrismVersionCreated;
        public string BluePrismVersionTested;
        public string Provider;

        private const int MAX_IMAGE_HEIGHT = 50;
        private const int MAX_IMAGE_WIDTH = 300;

        private bool IsNew() => ProductId.Equals(Guid.Empty);

        private SkillConfiguration() { }

        public SkillConfiguration(Guid selectedWebApi, int categoryId, string versionNumber,
                                  string name, string description, byte[] icon,
                                  string bluePrismVersionCreated, string bluePrismVersionTested, string provider,
                                  Guid productId)
        {
            SelectedWebApi = selectedWebApi;
            CategoryId = categoryId;
            VersionNumber = versionNumber;
            Name = name;
            Description = description;
            Icon = icon;
            BluePrismVersionCreated = bluePrismVersionCreated;
            BluePrismVersionTested = bluePrismVersionTested;
            Provider = provider;
            ProductId = productId;

            ValidateConfiguration();

            if (IsNew())
                ProductId = Guid.NewGuid();
        }

        private void ValidateConfiguration()
        {
            if (SelectedWebApi.Equals(Guid.Empty))
                throw new Exception(SkillPackagerResources.Error_InvalidWebApiId);
            if ((SkillCategories) CategoryId == SkillCategories.Unknown)
                throw new Exception(SkillPackagerResources.Error_InvalidSkillCategory);
            if (string.IsNullOrEmpty(VersionNumber))
                throw new Exception(SkillPackagerResources.Error_InvalidSkillVersion);
            if (string.IsNullOrEmpty(Name))
                throw new Exception(SkillPackagerResources.Error_InvalidSkillName);
            ValidateFieldLength(nameof(Name), Name);
            if (string.IsNullOrEmpty(Description))
                throw new Exception(SkillPackagerResources.Error_InvalidSkillDescription);
            if (string.IsNullOrEmpty(BluePrismVersionCreated))
                throw new Exception(SkillPackagerResources.Error_InvalidVersionCreated);
            ValidateFieldLength(nameof(BluePrismVersionCreated), BluePrismVersionCreated);
            if (string.IsNullOrEmpty(BluePrismVersionTested))
                throw new Exception(SkillPackagerResources.Error_InvalidVersionTested);
            ValidateFieldLength(nameof(BluePrismVersionTested), BluePrismVersionTested);
            if (IsNew() && string.IsNullOrEmpty(Provider))
                throw new Exception(SkillPackagerResources.Error_ProviderMissingOnCreation);
            ValidateFieldLength(nameof(Provider), Provider);

            ValidateIcon();
        }

        private void ValidateIcon()
        {
            if (Icon == null)
                throw new Exception(SkillPackagerResources.Error_InvalidIcon);

            using (var byteStream = new MemoryStream(Icon))
            using (var image = Image.FromStream(byteStream))
            {
                if (image.Height > MAX_IMAGE_HEIGHT)
                    throw new Exception(string.Format(SkillPackagerResources.Error_ImageHeightGreaterThanMaximum, MAX_IMAGE_HEIGHT));
                if (image.Width > MAX_IMAGE_WIDTH)
                    throw new Exception(string.Format(SkillPackagerResources.Error_ImageWidthGreaterThanMaximum, MAX_IMAGE_WIDTH));
            }
        }

        private void ValidateFieldLength(string fieldName, string value)
        {
            if (value.Length > 255)
                throw new Exception(string.Format(SkillPackagerResources.Error_MaxFieldLengthExceeded, fieldName));
        }

        internal string ToXml()
        {
            var doc = new XmlDocument();
            var rootElement = doc.CreateElement("skill");
            rootElement.SetAttribute("id", ProductId.ToString());
            rootElement.SetAttribute("name", Name);
            rootElement.SetAttribute("xmlns", "http://www.blueprism.co.uk/product/skill");
            rootElement.InnerText = GetEncryptedSkillInformation();
            doc.AppendChild(rootElement);

            return doc.OuterXml;
        }

        private XmlElement CreateElement(XmlDocument doc, string name, string value)
        {
            var element = doc.CreateElement(name);
            var content = doc.CreateTextNode(value);
            element.AppendChild(content);
            return element;
        }

        private string GetEncryptedSkillInformation()
        {
            var doc = new XmlDocument();
            var infoElement = doc.CreateElement("skillinfo");
            doc.AppendChild(infoElement);

            infoElement.AppendChild(CreateElement(doc, "selectedwebapi", SelectedWebApi.ToString()));
            infoElement.AppendChild(CreateElement(doc, "categoryid", $"{CategoryId}"));
            infoElement.AppendChild(CreateElement(doc, "versionnumber", VersionNumber));
            infoElement.AppendChild(CreateElement(doc, "description", Description));
            infoElement.AppendChild(CreateElement(doc, "icon", Convert.ToBase64String(Icon)));
            infoElement.AppendChild(CreateElement(doc, "blueprismversioncreated", BluePrismVersionCreated));
            infoElement.AppendChild(CreateElement(doc, "blueprismversiontested", BluePrismVersionTested));
            infoElement.AppendChild(CreateElement(doc, "provider", Provider));

            return Encryptor.Encrypt(doc.OuterXml);
        }
    }
}