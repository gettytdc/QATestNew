namespace BluePrism.Core.Plugins
{
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;

    using ConfigElements;

    using Utility;
    using Utilities.Functional;

    /// <summary>
    /// Provides methods for loading configuration settings
    /// from an XML file.
    /// </summary>
    /// <seealso cref="BluePrism.Core.Plugins.BaseConfiguration" />
    public class FileBackedConfiguration : BaseConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileBackedConfiguration"/> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public FileBackedConfiguration(string filePath)
        {
            LoadConfig(filePath);
        }

        /// <summary>
        /// Loads the configuration from the given file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        private void LoadConfig(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            try
            {
                Add(new FlagElement
                {
                    Name = "Enabled",
                    Value = true
                });

                var document = XDocument.Load(filePath);
                if (document.Root?.Name != "config")
                    return;

                document.Root
                    .Elements()
                    .Select(ParseNode)
                    .ForEach(Add)
                    .Evaluate();
            }
            catch
            {
                //Failure to load the config should just leave the default values
            }
        }

        /// <summary>
        /// Parses the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The configuration element parsed from the node</returns>
        private static IConfigElement ParseNode(XElement node)
        {
            IConfigElement result = null;

            switch (node.Name.LocalName)
            {
                case "flag":
                    result = ParseFlag(node);
                    break;

                case "text":
                    result = ParseText(node);
                    break;

                case "endpoint":
                    result = ParseEndpoint(node);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Parses a boolean value from the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The boolean value parsed from the node.</returns>
        private static IConfigElement ParseFlag(XElement node) =>
            node.Attribute("name")
                ?.Map(x => new FlagElement { Name = x.Value, Value = bool.Parse(node.Value) });

        /// <summary>
        /// Parses a text value from the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The text value parsed from the node.</returns>
        private static IConfigElement ParseText(XElement node) =>
            node.Attribute("name")
                ?.Map(x => new TextElement { Name = x.Value, Value = node.Value.ToSecureString() });

        /// <summary>
        /// Parses DNS details from the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The parsed DNS details.</returns>
        private static DnsEndPoint ParseDnsDetails(XContainer node)
        {
            var host = node.Element("host")?.Value;
            var port =
                node.Element("port")
                    .Value
                    .Map(int.Parse);
            return new DnsEndPoint(host, port);
        }

        /// <summary>
        /// Parses an endpoint from the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The parsed endpoint.</returns>
        private static IConfigElement ParseEndpoint(XElement node) =>
            node.Attribute("name")
            ?.Value
            .Map(x =>
                new HostConfigElement
                {
                    Name = x,
                    Value = ParseDnsDetails(node)
                });

    }
}
