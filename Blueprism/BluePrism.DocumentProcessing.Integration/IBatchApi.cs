namespace BluePrism.DocumentProcessing.Integration
{
    using Api.Models;

    public interface IBatchApi
    {
        string CreateBatch(string token, string batchType, string name, string description, string returnQueueSuffix);
        void AddDocumentToBatch(string token, string batchId, string fileExtension, string fileData);
        void SubmitBatch(string token, string batchId);
        BatchModel GetBatch(string token, BatchIdentifier batchId);
    }
}