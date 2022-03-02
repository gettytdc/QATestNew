using System;
using System.IO;
using System.Text;

namespace SkillPackageUtility.Utilities
{
    internal static class FileHelper
    {
        internal static string GetTextFromFile(string filepath)
        {
            try
            {
                return File.ReadAllText(filepath);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to read from file.", e);
            }
        }

        internal static void SaveTextToFile(string text, string filepath)
        {
            try
            {
                File.WriteAllText(filepath, text, Encoding.Default);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to save file to specified destination", e);
            }
        }

        internal static void CheckFileExtension(string fileType, string filePath, string validExtension)
        {
            if (!filePath.EndsWith(validExtension))
                throw new Exception($"Invalid extension for {fileType} filepath: Only {validExtension} files are allowed.");
        }
    }
}