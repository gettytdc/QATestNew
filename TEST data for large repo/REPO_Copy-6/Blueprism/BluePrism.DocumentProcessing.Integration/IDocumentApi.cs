namespace BluePrism.DocumentProcessing.Integration
{
    public interface IDocumentApi
    {
        string GetDocumentData(string token, string documentId);
    }
}