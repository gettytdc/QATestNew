namespace BluePrism.DocumentProcessing.Integration
{
    using Api.Models;
    using Core.Configuration;
    using Utilities.Functional;
    using Newtonsoft.Json;

    public class BatchApi : ApiBase, IBatchApi
    {
        public BatchApi(IApiCommunicator apiCommunicator, IAppSettings appSettings)
            : base(apiCommunicator, appSettings)
        {
        }

        public string CreateBatch(string token, string batchType, string name, string description, string returnQueueSuffix) =>
            new CreateBatchModel
                {
                    BatchTypeIdentifier = BatchTypeIdentifier.Parse(batchType),
                    Name = name,
                    Description = description,
                    ReturnQueueSuffix = returnQueueSuffix
                }
                .Map<object, string>(JsonConvert.SerializeObject)
                .Map(SendRequestWithBodyGetResponse(token, "Batch", "POST"));

        public void AddDocumentToBatch(string token, string batchId, string fileExtension, string fileData) =>
            new AddDocumentToBatchModel
                {
                    FileExtension = fileExtension,
                    FileData = fileData
                }
                .Map<object, string>(JsonConvert.SerializeObject)
                .Tee(SendRequestWithBody(token, $"Batch/{batchId}", "POST"));

        public void SubmitBatch(string token, string batchId) =>
            SendRequest(token, $"Batch/{batchId}/Submit", "POST");

        public BatchModel GetBatch(string token, BatchIdentifier batchId) =>
            SendRequestGetResponse(token, $"Batch/{batchId}", "GET")
                .Map(JsonConvert.DeserializeObject<BatchModel>);
    }
}