namespace BluePrism.DocumentProcessing.Integration
{
    using System.Collections.Generic;
    using Api.Models;
    using Api.Models.JsonConverter;
    using Core.Configuration;
    using Utilities.Functional;
    using Newtonsoft.Json;

    public class BatchTypeApi : ApiBase, IBatchTypeApi
    {
        public BatchTypeApi(IApiCommunicator apiCommunicator, IAppSettings appSettings) 
            : base(apiCommunicator, appSettings)
        {
        }

        public IReadOnlyCollection<BatchTypeModel> GetBatchTypes(string token) =>
            SendRequestGetResponse(token, "BatchType", "GET")
                .Map(x => JsonConvert.DeserializeObject<BatchTypeModel[]>(x, new MaybeJsonConverter<ClassificationModelIdentifier>()));

    }
}