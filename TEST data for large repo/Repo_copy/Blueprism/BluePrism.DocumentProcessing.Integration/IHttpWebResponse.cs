namespace BluePrism.DocumentProcessing.Integration
{
    using System;
    using System.IO;
    using System.Net;

    public interface IHttpWebResponse : IDisposable
    {
        HttpStatusCode StatusCode { get; }
        string StatusDescription { get; }

        Stream GetResponseStream();
    }
}