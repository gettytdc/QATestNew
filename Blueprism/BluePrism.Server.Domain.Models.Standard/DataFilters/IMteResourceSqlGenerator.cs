using System.Collections.Generic;
using System.Data.SqlClient;

namespace BluePrism.Server.Domain.Models.DataFilters
{
    public interface IMteResourceSqlGenerator
    {
        string ReplaceTokenAndAddParameters(
            SqlCommand sqlCommand,
            IReadOnlyCollection<int> userRoles = null,
            IReadOnlyCollection<int> resources = null);
    }
}
