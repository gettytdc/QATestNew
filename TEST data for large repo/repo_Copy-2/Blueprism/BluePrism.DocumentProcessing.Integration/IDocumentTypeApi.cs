namespace BluePrism.DocumentProcessing.Integration
{
    using System.Collections.Generic;
    using Domain;

    public interface IDocumentTypeApi
    {
        IEnumerable<DocumentType> GetDocumentTypes(string token);
    }
}