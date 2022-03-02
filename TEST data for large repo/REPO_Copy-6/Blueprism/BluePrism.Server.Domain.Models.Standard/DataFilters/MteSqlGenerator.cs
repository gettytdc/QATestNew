using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BluePrism.Server.Domain.Models.DataFilters
{
    public class MteSqlGenerator : IMteSqlGenerator
    {
        public const string MteToken = "{mteCriteria}";

        private const string TempTableName = "#TempMte";
        private const string ResourcesParameterName = "resourcePermissions";
        private const string ProcessParameterName = "processPermissions";
        private const string UserRolesParameterName = "userRoles";
        private const string TableVariableTypeName = "dbo.IntIdTableType";

        private readonly string _query;
        private readonly string _filterTableAlias;
        private readonly bool _hasOtherCriteria;

        public MteSqlGenerator(string query, string filterTableAlias, bool hasOtherCriteria = true)
        {
            if (!query?.Contains(MteToken) ?? true)
            {
                throw new ArgumentException($"Argument '{nameof(query)}' must contain '{MteToken}'");
            }

            if (string.IsNullOrWhiteSpace(filterTableAlias))
            {
                throw new ArgumentException($"Argument '{nameof(filterTableAlias)}' must have a value");
            }

            _query = query;
            _filterTableAlias = filterTableAlias;
            _hasOtherCriteria = hasOtherCriteria;
        }

        public string BuildQueryString(
            SqlCommand sqlCommand,
            IReadOnlyCollection<int> userRoles = null,
            IReadOnlyCollection<int> resources = null,
            IReadOnlyCollection<int> processes = null)
        {
            if (sqlCommand == null)
            {
                throw new ArgumentNullException(nameof(sqlCommand));
            }

            var resourcesList = resources ?? Array.Empty<int>();
            var processesList = processes ?? Array.Empty<int>();
            userRoles = userRoles ?? Array.Empty<int>();

            if (resourcesList.Count == 0 && processesList.Count == 0)
            {
                return ReplaceToken(string.Empty);
            }

            var stringBuilder = new StringBuilder();
            BuildCreateTempTable(stringBuilder);
            BuildPopulateTempTable(stringBuilder, resourcesList, processesList);

            BuildWhereCriteria(stringBuilder, resourcesList, processesList);
            BuildDropTempTable(stringBuilder);

            AddParameters(sqlCommand, userRoles, resourcesList, processesList);

            return stringBuilder.ToString();
        }

        private static void BuildCreateTempTable(StringBuilder stringBuilder) => stringBuilder
            .Append("create table ")
            .Append(TempTableName)
            .AppendLine("(Id uniqueidentifier, FieldType char(1));")
            .AppendLine();

        private void BuildPopulateTempTable(StringBuilder stringBuilder, IReadOnlyCollection<int> resources, IReadOnlyCollection<int> processes)
        {
            var sources = new List<string>();
            if (processes.Count > 0)
            {
                sources.Add($"select id, 'P' from ufn_GetProcessesWithPermissionOnRole(@{ProcessParameterName}, @{UserRolesParameterName})");
            }

            if (resources.Count > 0)
            {
                sources.Add($"select id, 'R' from ufn_GetResourcesWithPermissionOnRole(@{ResourcesParameterName}, @{UserRolesParameterName})");
            }

            var unionSql = string.Join(Environment.NewLine + " union all " + Environment.NewLine, sources) + ";";
            stringBuilder
                .Append("insert into ")
                .AppendLine(TempTableName)
                .AppendLine(unionSql)
                .AppendLine();
        }

        private static void BuildDropTempTable(StringBuilder stringBuilder) => stringBuilder
            .AppendLine()
            .AppendLine()
            .Append("drop table ")
            .Append(TempTableName)
            .AppendLine(";");

        private string ReplaceToken(string whereCriteria)
        {
            if (string.IsNullOrWhiteSpace(whereCriteria))
            {
                return _query.Replace(MteToken, " ");
            }

            var clauseStart = _hasOtherCriteria ? " and " : " where ";
            return _query.Replace(MteToken, clauseStart + whereCriteria + " ");
        }

        private void BuildWhereCriteria(StringBuilder stringBuilder, IReadOnlyCollection<int> resources, IReadOnlyCollection<int> processes)
        {
            var sources = new List<string>();
            if (processes.Count > 0)
            {
                sources.Add($"exists(select 1 from {TempTableName} as pp where pp.Id = {_filterTableAlias}.processid and pp.FieldType = 'P')");
            }

            if (resources.Count > 0)
            {
                sources.Add($"exists(select 1 from {TempTableName} as rp where rp.Id = {_filterTableAlias}.runningresourceid and rp.FieldType = 'R')");
            }

            var whereSql = string.Join(Environment.NewLine + " and ", sources);
            var query = ReplaceToken(whereSql);
            stringBuilder.Append(query);
        }

        private void AddParameters(
            SqlCommand sqlCommand,
            IReadOnlyCollection<int> userRoles,
            IReadOnlyCollection<int> resources,
            IReadOnlyCollection<int> processes)
        {
            if (resources.Count > 0)
            {
                AddIntIdTableParameter(sqlCommand, ResourcesParameterName, resources);
            }

            if (processes.Count > 0)
            {
                AddIntIdTableParameter(sqlCommand, ProcessParameterName, processes);
            }

            if (processes.Count > 0 || resources.Count > 0)
            {
                AddIntIdTableParameter(sqlCommand, UserRolesParameterName, userRoles);
            }
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
