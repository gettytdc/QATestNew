namespace BluePrism.DocumentProcessing.Integration
{
    using System.Collections.Generic;
    using Api.Models;

    public interface IBatchTypeApi
    {
        IReadOnlyCollection<BatchTypeModel> GetBatchTypes(string token);
    }
}