using BluePrism.DocumentProcessing.Integration.Properties;

namespace BluePrism.DocumentProcessing.Integration
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Text;
    using BPCoreLib;
    using Server.Domain.Models;
    using Utilities.Functional;

    public class ApiCommunicator : IApiCommunicator
    {
        private readonly Func<HttpWebResponse, IHttpWebResponse> _webResponseFactory;

        public ApiCommunicator(Func<HttpWebResponse, IHttpWebResponse> webResponseFactory)
        {
            _webResponseFactory = webResponseFactory;
        }

        public IHttpWebResponse SendHttpRequest(string uri, string method, string token) =>
            SendHttpRequestWithBody(uri, method, token, null);

        public IHttpWebResponse SendHttpRequestWithBody(string uri, string method, string token, string body)
        {
            var request = WebRequest.Create(uri);
            request.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {token}");
            request.Method = method;

            if (body != null)
            {
                var bodyData = body.Map(Encoding.UTF8.GetBytes);
                request.ContentType = "application/json";
                request.ContentLength = bodyData.Length;
                request.GetRequestStream().Use(s => s.Write(bodyData, 0, bodyData.Length));
            }
            else
            {
                request.ContentLength = 0;
            }

            return request.Map(GetResponseWithoutExceptions)?.Map(_webResponseFactory);
        }

        public void ThrowErrorOnInvalidResponse(IHttpWebResponse response)
        {
            if (response == null)
                throw new UnavailableException(Resources.CommunicationWithAPIFailed);

            if (response.StatusCode == HttpStatusCode.Forbidden)
                throw new PermissionException();

            if (!ValidResponseCodes.Contains(response.StatusCode))
            {
                throw new InvalidResponseException(
                    response.StatusCode,
                    String.Format(Resources.ErrorCode01ReturnedFromDocumentProcessingServer, response.StatusCode, response.StatusDescription));
            }
        }

        private HttpWebResponse GetResponseWithoutExceptions(WebRequest request)
        {
            try
            {
                return request.GetResponse() as HttpWebResponse;
            }
            catch (WebException exception)
            {
                return exception.Response as HttpWebResponse;
            }
        }

        private static readonly HttpStatusCode[] ValidResponseCodes =
            {HttpStatusCode.OK, HttpStatusCode.Created};
    }
}
