namespace BluePrism.Core.Plugins
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Security;
    using System.Text;
    using Utilities.Functional;
    using Utility;

    /// <inheritdoc />
    /// <summary>
    /// Provides methods for instantiating requests with basic HTTP authentication
    /// </summary>
    /// <seealso cref="T:BluePrism.Core.Plugins.IRequestFactory" />
    public class SplunkBasicHttpAuthenticationRequestFactory : IRequestFactory
    {
        private string AuthorizationHeader =>
            $"{_username}:{_password.MakeInsecure()}"
                .Map(Encoding.UTF8.GetBytes)
                .Map(Convert.ToBase64String)
                .Map(x => $"Basic {x}");

        private readonly string _username;

        private readonly SecureString _password;

        /// <summary>
        /// Initializes a new instance of the <see cref="SplunkTokenAuthenticationRequestFactory"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public SplunkBasicHttpAuthenticationRequestFactory(IConfiguration configuration)
        {
            _username = configuration.GetConfig("Username", new SecureString()).MakeInsecure();
            _password = configuration.GetConfig("Password", new SecureString());
        }

        /// <summary>
        /// Determines if the given configuration is suitable for configuring
        /// this object.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns><c>true</c> if the configuration is suitable; otherwise, <c>false</c>.</returns>
        public static bool ConfigurationIsSuitable(IConfiguration configuration) =>
            configuration.Elements.Any(x => x.Name == "Username")
            && configuration.Elements.Any(x => x.Name == "Password");

        /// <summary>
        /// Gets a request for the given URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>
        /// A <see cref="HttpWebRequest" /> object.
        /// </returns>
        /// <inheritdoc />
        public HttpWebRequest GetRequestForUri(Uri uri)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);

            request.ContentType = "application/json";
            request.Method = "POST";
            request.Headers.Add(
                HttpRequestHeader.Authorization,
                AuthorizationHeader);
            return request;
        }
    }
}
