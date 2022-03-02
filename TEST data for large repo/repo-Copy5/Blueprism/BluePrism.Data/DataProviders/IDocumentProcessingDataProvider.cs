namespace BluePrism.Data.DataProviders
{
    using System;
    using System.Collections.Generic;

    using DataModels;

    public interface IDocumentProcessingDataProvider
    {
        void AddBatchOutputQueueOverride(IDatabaseConnection connection, Guid batchId, Guid workQueueId);
        Guid? GetBatchOutputQueueOverrideForBatch(IDatabaseConnection connection, Guid batchId);
        IReadOnlyCollection<DocumentTypeQueueMapping> GetDocumentTypeQueues(IDatabaseConnection connection);
        Guid GetDefaultDocumentTypeQueueId(IDatabaseConnection connection);
        void SetDocumentTypeQueue(IDatabaseConnection connection, Guid documentTypeId, Guid queueId);
    }
}