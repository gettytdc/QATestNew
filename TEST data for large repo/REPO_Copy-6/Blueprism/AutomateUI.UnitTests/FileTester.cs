using System;
using System.IO;
using System.Text;
using BluePrism.BPCoreLib;
using NUnit.Framework;

namespace AutomateUI.UnitTests
{

    /// <summary>
    /// Test class for session variable parsing
    /// </summary>
    [TestFixture()]
    public partial class FileTester
    {

        /// <summary>
        /// Class to expose the ExtractBestFitDir() method to enable it to be tested
        /// </summary>
        private partial class DummyFileChooser : ctlFileChooser
        {
            public void TestDirectoryEstimates(string input, string expectedOutput)
            {
                var output = ExtractBestFitDir(input);
                if ((output ?? "") != (expectedOutput ?? ""))
                {
                    Assert.Fail("ExtractBestFitDir(\"{0}\"): Expected: \"{1}\"; Got: \"{2}\"", input, expectedOutput, output);

                }
            }
        }

        /// <summary>
        /// Tests the estimating of the directory when a partial path is given in
        /// a file dialog.
        /// </summary>
        [Test()]
        public void TestDirectoryEstimates()
        {
            using (var chooser = new DummyFileChooser())
            {
                chooser.TestDirectoryEstimates("", null);
                chooser.TestDirectoryEstimates(" ", null);
                chooser.TestDirectoryEstimates(null, null);
                chooser.TestDirectoryEstimates("g32f23", null);
                chooser.TestDirectoryEstimates(@"C:\", @"C:\");
                chooser.TestDirectoryEstimates(@"C:\<Ooh, wassup?>", @"C:\");
                chooser.TestDirectoryEstimates(@"\\\\/\///\/", null);
                chooser.TestDirectoryEstimates(":", null);

                // Create a little temp file to test that it correctly gets the dir from an existing file
                var tempFile = new FileInfo(BPUtil.GetRandomFilePath());
                try
                {
                    using (var @out = new StreamWriter(tempFile.Create(), Encoding.UTF8))
                    {
                        @out.WriteLine("Just a little test");
                        @out.Flush();
                    }

                    chooser.TestDirectoryEstimates(tempFile.FullName, tempFile.DirectoryName);
                }
                finally
                {
                    tempFile.Delete();
                }

                var tempDirectory = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine);
                chooser.TestDirectoryEstimates(Path.Combine(tempDirectory, @"This\doesn't\actually\exist"), tempDirectory);

                chooser.TestDirectoryEstimates(@"C:\WINDOWS\system32\drivers\etc\hosts", @"C:\WINDOWS\system32\drivers\etc");

            }
        }
    }
}
