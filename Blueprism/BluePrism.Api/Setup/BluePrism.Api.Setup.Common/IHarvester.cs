namespace BluePrism.Api.Setup.Common
{
    using System.Collections.Generic;
    using WixSharp;

    public interface IHarvester
    {
        ManagedProject GetProject();        
        IHarvester AddInnerEntity(WixEntity entity);
        IHarvester AddInnerEntity(WixEntity entity, params Feature[] features);
        IHarvester AddOuterEntity(WixEntity entity);
        IHarvester AddOuterEntity(IList<Property> properties);
        IHarvester AddOuterEntity(WixEntity entity, params Feature[] features);
        IHarvester AddDirectory(string path, string name);
        IHarvester AddDirectory(string path, string name, params Feature[] features);
        IHarvester AddContentFromPackage(string packageName, string directoryName, params Feature[] features);
        IHarvester AddContentFromPackage(string packageName, string directoryName);
        IHarvester AddContentFromPackage(string packageName);
        IHarvester AddFilesFromPackageDirectory(string packageName, string sourceDirectory);
        IHarvester AddContentFromPackagesToSingleDirectory(IEnumerable<string> packageNames, string directoryName,
            params Feature[] features);
        IHarvester AddFilesFromDirectory(string directory);  
    }
}
