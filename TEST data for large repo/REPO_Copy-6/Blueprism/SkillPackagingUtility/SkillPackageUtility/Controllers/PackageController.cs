using System;
using System.Drawing;
using Newtonsoft.Json;
using SkillPackager.Model;
using SkillPackageUtility.Model;
using SkillPackageUtility.Utilities;

namespace SkillPackageUtility.Controllers
{
    internal class PackageController
    {
        internal void PackageSkill(string releasePath, string configurationPath, string destinationPath)
        {
            ValidateFileExtensions(releasePath, configurationPath, destinationPath);

            var skill = CreateSkill(releasePath, configurationPath);
            SaveSkillToFile(skill, destinationPath);
        }

        private static void ValidateFileExtensions(string releasePath, string configurationPath, string destinationPath)
        {
            FileHelper.CheckFileExtension("Release", releasePath, ".bprelease");
            FileHelper.CheckFileExtension("Configuration", configurationPath, ".json");
            FileHelper.CheckFileExtension("Skill", destinationPath, ".bpskill");
        }

        private static Skill CreateSkill(string releasePath, string configurationPath)
        {
            var releaseXml = FileHelper.GetTextFromFile(releasePath);
            var configurationJson = FileHelper.GetTextFromFile(configurationPath);
            var skillConfiguration = CreateConfiguration(configurationJson);

            return new Skill(releaseXml, skillConfiguration);
        }

        private static void SaveSkillToFile(Skill skill, string destinationPath)
        {
            var fileContent = skill.Export();
            FileHelper.SaveTextToFile(fileContent, destinationPath);
        }

        private static SkillConfiguration CreateConfiguration(string configurationJson)
        {
            try
            {
                var file =  JsonConvert.DeserializeObject<SkillConfigurationFile>(configurationJson);
                return file.CreateSkillConfiguration();
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to load skill configuration: {e.Message}");
            }
        }

        internal void GenerateConfigurationTemplate(string filepath)
        {
            ValidateConfigTemplateFilePath(filepath);

            var file = new SkillConfigurationFile();
            var template = file.GetTemplateFile();
            FileHelper.SaveTextToFile(template, filepath);
        }

        private static void ValidateConfigTemplateFilePath(string filepath)
        {
            if (filepath == null)
                throw new Exception("You must provide a valid destination for the configuration template.");
            FileHelper.CheckFileExtension("Configuration template", filepath, ".json");
        }
    }
}