using System.Linq;

namespace BluePrism.Server.Domain.Models.Pagination
{
    public static class OrderBySqlGenerator
    {
        public static string GetOrderByClause(string orderDirection, params string[] columnNames) =>
            string.Join(", ", columnNames.Distinct().Select(x => $"{x} {orderDirection}"));
    }
}
