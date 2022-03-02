using System;
using System.Drawing;
using Newtonsoft.Json;
using SkillPackager.Model;

namespace SkillPackageUtility.Model
{
    internal class SkillConfigurationFile
    {
        public Guid ProductId { get; set; }
        public Guid SelectedWebApi { get; set; }
        public int CategoryId { get; set; }
        public string VersionNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconFilePath { get; set; } = string.Empty;
        public string BluePrismVersionCreated { get; set; } = string.Empty;
        public string BluePrismVersionTested { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;

        internal string GetTemplateFile() => JsonConvert.SerializeObject(this, Formatting.Indented);

        internal SkillConfiguration CreateSkillConfiguration()
        {
            var icon = LoadIcon(IconFilePath);
            return new SkillConfiguration(
                SelectedWebApi, 
                CategoryId, 
                VersionNumber,
                Name, 
                Description, 
                icon, 
                BluePrismVersionCreated,
                BluePrismVersionTested, 
                Provider, 
                ProductId);
        }

        private static byte[] LoadIcon(string filepath)
        {
            using (var image = Image.FromFile(filepath))
                return new ImageConverter().ConvertTo(image, typeof(byte[])) as byte[];
        }
    }
}