namespace BluePrism.Core.Plugins
{
    using System;
    using System.Net;

    /// <summary>
    /// Factory for web requests
    /// </summary>
    /// <remarks>
    /// Any classes implementing this interface will automatically be used to 
    /// create web requests if they return true from a static method called
    /// ConfigurationIsSuitable. See <see cref="SplunkBasicHttpAuthenticationRequestFactory"/>
    /// for an example.
    /// </remarks>
    public interface IRequestFactory
    {
        /// <summary>
        /// Gets a request for the given URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>A <see cref="HttpWebRequest"/> object.</returns>
        HttpWebRequest GetRequestForUri(Uri uri);
    }
}