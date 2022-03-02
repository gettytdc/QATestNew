using System.Collections.Generic;
using System.Data.SqlClient;

namespace BluePrism.Server.Domain.Models.DataFilters
{
    public interface IMteSqlGenerator
    {
        string BuildQueryString(
            SqlCommand sqlCommand,
            IReadOnlyCollection<int> userRoles = null,
            IReadOnlyCollection<int> resources = null,
            IReadOnlyCollection<int> processes = null);
    }
}
