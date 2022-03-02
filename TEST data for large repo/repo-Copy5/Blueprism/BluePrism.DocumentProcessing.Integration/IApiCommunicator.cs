namespace BluePrism.DocumentProcessing.Integration
{
    public interface IApiCommunicator
    {
        IHttpWebResponse SendHttpRequest(string uri, string method, string token);
        IHttpWebResponse SendHttpRequestWithBody(string uri, string method, string token, string body);
        void ThrowErrorOnInvalidResponse(IHttpWebResponse response);
    }
}