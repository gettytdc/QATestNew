namespace BluePrism.BrowserAutomation.Behaviors
{
    using System;
    using WebMessages;

    public class ResponseHandler<TResponse> : IResponseHandler
    {
        public Action<IWebMessage> OnResponseReceived { get; }
        public Type ExpectedResponseType { get; }

        public ResponseHandler(Action<IWebMessage> onResponseReceived)
        {
            OnResponseReceived = onResponseReceived;
            ExpectedResponseType = typeof(TResponse);
        }
    }
}
