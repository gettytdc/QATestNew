using BluePrism.AutomateProcessCore;

namespace BluePrism.DocumentProcessing.Integration
{
    public interface IDocumentFormDataCollectionBuilder
    {
        clsCollection CreateCollection(string formDataAsString);
    }
}