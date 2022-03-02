using System.Data;

namespace BluePrism.DatabaseInstaller
{
    public interface ISqlConnectionFactory
    {
        IDbConnection Create(string connectionString);
    }
}