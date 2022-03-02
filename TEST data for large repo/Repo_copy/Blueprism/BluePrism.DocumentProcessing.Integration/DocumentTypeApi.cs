namespace BluePrism.DocumentProcessing.Integration
{
    using System.Collections.Generic;
    using Core.Configuration;
    using Utilities.Functional;
    using Domain;
    using Newtonsoft.Json;

    public class DocumentTypeApi : ApiBase, IDocumentTypeApi
    {
        public DocumentTypeApi(IApiCommunicator apiCommunicator, IAppSettings appSettings)
            : base(apiCommunicator, appSettings)
        {
        }

        public IEnumerable<DocumentType> GetDocumentTypes(string token) =>
            SendRequestGetResponse(token, "DocumentType", "GET")
                .Map(JsonConvert.DeserializeObject<DocumentType[]>);
    }
}