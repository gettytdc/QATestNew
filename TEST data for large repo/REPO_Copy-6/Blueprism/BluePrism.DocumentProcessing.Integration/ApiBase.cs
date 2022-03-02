namespace BluePrism.DocumentProcessing.Integration
{
    using System;
    using Core.Configuration;
    using Core.Utility;
    using Utilities.Functional;

    public abstract class ApiBase
    {
        private readonly IApiCommunicator _apiCommunicator;
        private readonly string _apiEndpoint;

        protected ApiBase(IApiCommunicator apiCommunicator, IAppSettings appSettings)
        {
            _apiCommunicator = apiCommunicator;
            _apiEndpoint = appSettings["DocumentProcessing.ApiEndpoint"];
        }

        protected string SendRequestGetResponse(string token, string apiPath, string method) =>
            _apiCommunicator.SendHttpRequest($"{_apiEndpoint}{apiPath}", method, token)
                .Use(r => r
                    .Tee(_apiCommunicator.ThrowErrorOnInvalidResponse)
                    .GetResponseStream()
                    ?.ReadEntireStream());

        protected Func<string, string> SendRequestWithBodyGetResponse(string token, string apiPath, string method) => requestBody =>
            SendRequestWithBodyGetResponse(token, apiPath, method, requestBody);

        protected string SendRequestWithBodyGetResponse(string token, string apiPath, string method, string requestBody) =>
            _apiCommunicator.SendHttpRequestWithBody($"{_apiEndpoint}{apiPath}", method, token, requestBody)
                ?.Use(r => r
                    .Tee(_apiCommunicator.ThrowErrorOnInvalidResponse)
                    .GetResponseStream()
                    ?.ReadEntireStream());

        protected void SendRequest(string token, string apiPath, string method) =>
            _apiCommunicator.SendHttpRequest($"{_apiEndpoint}{apiPath}", method, token)
                ?.Use(_apiCommunicator.ThrowErrorOnInvalidResponse);

        protected Action<string> SendRequestWithBody(string token, string apiPath, string method) => requestBody =>
            SendRequestWithBody(token, apiPath, method, requestBody);

        protected void SendRequestWithBody(string token, string apiPath, string method, string requestBody) =>
            _apiCommunicator.SendHttpRequestWithBody($"{_apiEndpoint}{apiPath}", method, token, requestBody)
                ?.Use(_apiCommunicator.ThrowErrorOnInvalidResponse);
    }
}