using System.Data;
using System.Data.SqlClient;

namespace BluePrism.DatabaseInstaller
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        public IDbConnection Create(string connectionString)
            => new SqlConnection(connectionString);
    }
}
