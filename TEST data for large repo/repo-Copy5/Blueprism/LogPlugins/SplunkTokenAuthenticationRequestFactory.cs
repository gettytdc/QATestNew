namespace BluePrism.Core.Plugins
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Security;
    using Utility;

    /// <summary>
    /// Provides methods for instantiating requests with token-based authentication
    /// </summary>
    /// <seealso cref="BluePrism.Core.Plugins.IRequestFactory" />
    public class SplunkTokenAuthenticationRequestFactory : IRequestFactory
    {
        private readonly string _token;

        /// <summary>
        /// Initializes a new instance of the <see cref="SplunkTokenAuthenticationRequestFactory"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public SplunkTokenAuthenticationRequestFactory(IConfiguration configuration)
        {
            _token = configuration.GetConfig("Token", new SecureString()).MakeInsecure();
        }

        /// <summary>
        /// Determines if the given configuration is suitable for configuring
        /// this object.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns><c>true</c> if the configuration is suitable; otherwise, <c>false</c>.</returns>
        public static bool ConfigurationIsSuitable(IConfiguration configuration) =>
            configuration.Elements.Any(x => x.Name == "Token");

        /// <summary>
        /// Gets a request for the given URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>
        /// A <see cref="HttpWebRequest" /> object.
        /// </returns>
        public HttpWebRequest GetRequestForUri(Uri uri)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);

            request.ContentType = "application/json";
            request.Method = "POST";
            request.Headers.Add(
                HttpRequestHeader.Authorization,
                $"Splunk {_token}");

            return request;
        }
    }
}
