namespace BluePrism.DocumentProcessing.Integration
{
    using Core.Configuration;

    public class DocumentApi : ApiBase, IDocumentApi
    {
        public DocumentApi(IApiCommunicator apiCommunicator, IAppSettings appSettings)
            : base(apiCommunicator, appSettings)
        {

        }

        public string GetDocumentData(string token, string documentId) =>
            SendRequestGetResponse(token, $"Document/{documentId}/data", "GET");
    }
}