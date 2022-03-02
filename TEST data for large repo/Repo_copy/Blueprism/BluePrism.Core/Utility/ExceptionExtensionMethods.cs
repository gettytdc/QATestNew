using System;
using System.Net;

namespace BluePrism.Core.Utility
{
    /// <summary>
    /// Contains extension methods for exceptions
    /// </summary>
    public static class ExceptionExtensionMethods
    {

        /// <summary>
        /// Returns True, if the exception is a <see cref="WebException"/> whose
        /// response has a 401 status code.
        /// </summary>
        /// <param name="this">The exception</param>
        /// <returns>Returns True, if the exception is a <see cref="WebException"/> whose
        /// response has a 401 status code.</returns>
        public static bool Is401WebException(this Exception @this)
            => ((@this as WebException)
                    ?.Response as HttpWebResponse)
                    ?.StatusCode == HttpStatusCode.Unauthorized;
    }
}
