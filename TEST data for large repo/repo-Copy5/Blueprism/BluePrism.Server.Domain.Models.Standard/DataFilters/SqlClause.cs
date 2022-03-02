using System.Data;

namespace BluePrism.Server.Domain.Models.DataFilters
{
    public class SqlClause
    {
        public string SqlText { get; set; }
        public IDbDataParameter[] Parameters { get; set; }
    }
}
