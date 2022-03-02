namespace BluePrism.DatabaseInstaller.Data
{
    public class SqlDatabaseConnectionSetting : ISqlDatabaseConnectionSetting
    {
        public SqlDatabaseConnectionSetting(string databaseName, string databaseFilePath,
            string connectionString, string masterConnectionString,  bool isComplete)
        {
            DatabaseName = databaseName;
            DatabaseFilePath = databaseFilePath;
            ConnectionString = connectionString;
            MasterConnectionString = masterConnectionString;
            IsComplete = isComplete;
        }
        public string DatabaseName { get; }
        public string DatabaseFilePath { get; }
        public string ConnectionString { get; }
        public string MasterConnectionString { get; }
        public bool IsComplete { get; }
    }
}
