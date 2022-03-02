namespace BluePrism.BrowserAutomation.Behaviors
{
    using System;
    using WebMessages;

    /// <summary>
    /// Provides methods for handling responses
    /// </summary>
    public interface IResponseHandler
    {
        /// <summary>
        /// The response type handled by this handler
        /// </summary>
        Type ExpectedResponseType { get; }

        /// <summary>
        /// Called when a response message is received
        /// </summary>
        Action<IWebMessage> OnResponseReceived { get; }
    }
}
