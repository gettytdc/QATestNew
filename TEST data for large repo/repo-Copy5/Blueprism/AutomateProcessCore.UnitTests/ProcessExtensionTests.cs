using BluePrism.AutomateProcessCore;
using NUnit.Framework;
using System.IO;
using System.Text;
using System;

namespace AutomateProcessCore.UnitTests
{

    [TestFixture]
    public class ProcessExtensionTests
    {
        private const string InvalidXmlFragment = "This is not xml";
        private const string ObjectXmlFragment = "<process name=\"processName\" type=\"object\"></process>";
        private const string ProcessXmlFragment = "<process name=\"processName\" ></process>";

        [Test,
         TestCase("txt", InvalidXmlFragment, ExpectedResult = clsProcess.IsValidForType.InvalidFileType),
         TestCase(clsProcess.ObjectFileExtension , InvalidXmlFragment,ExpectedResult = clsProcess.IsValidForType.InvalidXML),
         TestCase(clsProcess.ObjectFileExtension, ObjectXmlFragment,ExpectedResult = clsProcess.IsValidForType.Valid),
         TestCase(clsProcess.ObjectFileExtension, ProcessXmlFragment,ExpectedResult = clsProcess.IsValidForType.InValid),
         TestCase(clsProcess.ProcessFileExtension , ProcessXmlFragment,ExpectedResult = clsProcess.IsValidForType.Valid),
         TestCase(clsProcess.ProcessFileExtension, ObjectXmlFragment,ExpectedResult = clsProcess.IsValidForType.InValid)]
        public clsProcess.IsValidForType ClsProcessTestValidExtensionForType(string fileExtension, string xml)
        {
            var fileName = $"{Path.GetTempPath()}{Guid.NewGuid()}.{fileExtension}";
            try
            {
                using (var fs = File.Create(fileName))
                {
                    var info = new UTF8Encoding(true).GetBytes(xml);
                    fs.Write(info, 0, info.Length);
                    var data = new byte[] { 0x0 };
                    fs.Write(data, 0, data.Length);
                }

                return clsProcess.CheckValidExtensionForType(fileName);
            }
            finally
            {
                File.Delete(fileName);
            }
        }

    }
}
