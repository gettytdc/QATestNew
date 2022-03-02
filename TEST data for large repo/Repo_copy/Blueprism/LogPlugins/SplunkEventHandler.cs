namespace BluePrism.Core.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Security;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using Utility;
    using Utilities.Functional;

    /// <summary>
    /// Handles Splunk events and logs them via a configurable HTTP event collector
    /// connector
    /// </summary>
    public class SplunkEventHandler : BaseEventHandler
    {
        /// <summary>
        /// The Unix epoch 1970-1-1
        /// </summary>
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1);

        /// <summary>
        /// The Uri for the Splunk handler
        /// </summary>
        private readonly Uri _uri;

        /// <summary>
        /// The source of the Splunk event
        /// </summary>
        private readonly string _source;

        /// <summary>
        /// Whether the plug-in is enabled.
        /// </summary>
        private readonly bool _enabled;

        private readonly IRequestFactory _requestFactory;

        /// <summary>
        /// Constructs as new splunk event handler
        /// </summary>
        public SplunkEventHandler(
            Func<IConfiguration, IRequestFactory> requestFactoryFactory)
            : base(new FileBackedConfiguration(ConfigurationFilePath))
        {
            _requestFactory = requestFactoryFactory(Config);

            _enabled = Config.GetConfig("Enabled", false);

            var host = 
                Config.GetConfig(
                    "HTTP Server",
                    new DnsEndPoint("localhost", 8088));

            var protocol = 
                Config.GetConfig("Secure", false) 
                ? "https" 
                : "http";

            var path =
                Config.GetConfig("Path", new SecureString())
                .MakeInsecure()
                .TrimStart('/');

            _source = Config.GetConfig("Source", "BluePrism".ToSecureString()).MakeInsecure();

            _uri = new Uri($"{protocol}://{host.Host}:{host.Port}/{path}");
        }

        /// <summary>
        /// Gets the configuration file path.
        /// </summary>
        public static string ConfigurationFilePath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "splunk_config.xml");

        /// <summary>
        /// Handles the logging event.
        /// </summary>
        /// <param name="data">The data to log</param>
        public override void HandleEvent(IDictionary<string, object> data)
        {
            if (!_enabled)
                return;

            var eventTime = (long)(DateTime.UtcNow - Epoch).TotalSeconds;
            HandleEventAsync(eventTime, data).Continue();
        }

        /// <summary>
        /// Handles the logging event asynchronously.
        /// </summary>
        /// <param name="eventTime">The time the event occurred (seconds since Unix epoch)</param>
        /// <param name="data">The data in the event.</param>
        /// <returns>The task to run asynchronously</returns>
        private async Task HandleEventAsync(long eventTime, IDictionary<string, object> data)
        {
            if (_requestFactory == null)
                return;

            try
            {
                var request = _requestFactory.GetRequestForUri(_uri);
                await SendRequest(request, eventTime, data);
                var result = await GetResult(request);
                Debug.Print("Received response: " + result);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
            }
        }

        private async Task SendRequest(WebRequest request, long eventTime, IDictionary<string, object> data)
        {
            using (var requestStream = await request.GetRequestStreamAsync())
            using (var writer = new StreamWriter(requestStream))
            {
                new JsonSerializer().Serialize(writer, new
                {
                    time = eventTime,
                    host = Environment.MachineName,
                    source = _source,
                    @event = data
                });
            }
        }

        private static async Task<string> GetResult(WebRequest request)
        {
            using (var response = await request.GetResponseAsync() as HttpWebResponse)
            using (var reader = response?.GetResponseStream().Map(x => new StreamReader(x)))
            {
                return await (reader?.ReadToEndAsync() ?? Task.FromResult(string.Empty));
            }
        }
    }
}
