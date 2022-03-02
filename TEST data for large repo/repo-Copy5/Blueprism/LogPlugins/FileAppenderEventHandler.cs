using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using BluePrism.Core.Plugins.ConfigElements;
using Newtonsoft.Json;

namespace BluePrism.Core.Plugins
{
    /// <summary>
    /// Handles events and logs them to a hard-coded file location.
    /// </summary>
    /// <remarks>
    /// This class is currently not being used, and is for demonstration purposes.
    /// </remarks>
    [Export(typeof(IEventHandler))]
    public class FileAppenderEventHandler : BaseEventHandler
    {
        private const string DefaultFileName = @"file-appender.log";

        public FileAppenderEventHandler()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DefaultFileName);
            Config.Add(
                new FileConfigElement() {
                    Name = "Output File",
                    DefaultValue = new FileInfo(path)
                }
            );
        }

        public override string Name { get; set; }

        /// <summary>
        /// Writes the event to the log file.
        /// </summary>
        public override void HandleEvent(IDictionary<string, object> data)
        {
            var fileElem = Config.
                OfType<FileConfigElement>().
                First(c => c.Name == "Output File");

            var file = fileElem.Value;
            if (file == null) throw new ArgumentNullException(
                "No output file specified for file appender: " + Name);

            using (var writer = File.AppendText(file.FullName))
            {
                new JsonSerializer().Serialize(writer, data);
                writer.WriteLine();
            }
        }

    }
}
