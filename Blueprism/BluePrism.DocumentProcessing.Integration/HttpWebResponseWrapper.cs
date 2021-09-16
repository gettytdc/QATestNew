namespace BluePrism.DocumentProcessing.Integration
{
    using System;
    using System.IO;
    using System.Net;

    public class HttpWebResponseWrapper : IHttpWebResponse
    {
        private readonly HttpWebResponse _httpWebResponse;

        public HttpStatusCode StatusCode => _httpWebResponse.StatusCode;
        public string StatusDescription => _httpWebResponse.StatusDescription;

        public HttpWebResponseWrapper(HttpWebResponse httpWebResponse)
        {
            _httpWebResponse = httpWebResponse;
        }

        public Stream GetResponseStream() => _httpWebResponse.GetResponseStream();


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                    _httpWebResponse?.Dispose();

            _disposed = true;
        }

        ~HttpWebResponseWrapper()
        {
            Dispose(false);
        }

    }
}