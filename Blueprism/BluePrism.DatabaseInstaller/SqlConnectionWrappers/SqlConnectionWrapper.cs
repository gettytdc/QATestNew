using System.Data.SqlClient;

namespace BluePrism.DatabaseInstaller
{
    public class SqlConnectionWrapper : ISqlConnectionWrapper
    {
        public void ClearAllPools()
        {
            SqlConnection.ClearAllPools();
        }
    }
}
