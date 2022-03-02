using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BluePrism.Server.Domain.Models.DataFilters
{
    public class MteResourceSqlGenerator : IMteResourceSqlGenerator
    {
        public const string MteToken = "{mteCriteria}";

        private const string ResourcesParameterName = "resourcePermissions";
        private const string UserRolesParameterName = "userRoles";
        private const string TableVariableTypeName = "dbo.IntIdTableType";

        private readonly string _query;

        public MteResourceSqlGenerator(string query)
        {
            if (!query?.Contains(MteToken) ?? true)
            {
                throw new ArgumentException($"Argument '{nameof(query)}' must contain '{MteToken}'");
            }

            _query = query;
        }

        public string ReplaceTokenAndAddParameters(
            SqlCommand sqlCommand,
            IReadOnlyCollection<int> userRoles = null,
            IReadOnlyCollection<int> resources = null)
        {
            if (sqlCommand == null)
            {
                throw new ArgumentNullException(nameof(sqlCommand));
            }

            var resourcesList = resources ?? Array.Empty<int>();
            var userRolesList = userRoles ?? Array.Empty<int>();

            if (resourcesList.Count == 0)
            {
                return _query.Replace(MteToken, string.Empty);
            }

            AddParameters(sqlCommand, userRolesList, resourcesList);

            return _query.Replace(MteToken, $"inner join ufn_GetResourcesWithPermissionOnRole(@{ResourcesParameterName}, @{UserRolesParameterName}) p on p.id = r.resourceid");
        }

        private void AddParameters(
            SqlCommand sqlCommand,
            IReadOnlyCollection<int> userRoles,
            IReadOnlyCollection<int> resources)
        {
            AddIntIdTableParameter(sqlCommand, ResourcesParameterName, resources);
            AddIntIdTableParameter(sqlCommand, UserRolesParameterName, userRoles);
        }

        private static void AddIntIdTableParameter(SqlCommand sqlCommand, string parameterName, IEnumerable<int> values)
        {
            var table = CreateIdTable(values);

            var parameter = sqlCommand.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.TypeName = TableVariableTypeName;
            parameter.Value = table;

            sqlCommand.Parameters.Add(parameter);
        }

        private static DataTable CreateIdTable(IEnumerable<int> values)
        {
            const string idColumnName = "id";

            var table = new DataTable();
            table.Columns.Add(idColumnName, typeof(int));
            foreach (var value in values)
            {
                var row = table.NewRow();
                row[idColumnName] = value;
                table.Rows.Add(row);
            }

            return table;
        }
    }
}
