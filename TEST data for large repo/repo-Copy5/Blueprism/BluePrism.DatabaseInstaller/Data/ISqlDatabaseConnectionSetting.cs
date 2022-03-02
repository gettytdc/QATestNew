
namespace BluePrism.DatabaseInstaller
{
    public interface ISqlDatabaseConnectionSetting
    {
        string DatabaseName { get; }
        string ConnectionString { get; }
        string DatabaseFilePath { get; }
        string MasterConnectionString { get; }
        bool IsComplete { get; }
    }
}