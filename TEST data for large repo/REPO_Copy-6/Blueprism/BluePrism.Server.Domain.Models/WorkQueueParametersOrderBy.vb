Imports System.Runtime.CompilerServices
Imports BluePrism.Server.Domain.Models.Pagination

Public Module WorkQueueParametersOrderBy
    <Extension()>
    Public Function GetSqlOrderByClauses(ByVal workQueueParameters As WorkQueueParameters) As String
        Dim sortData = ColumnSortProvider(Of WorkQueueSortByProperty).ColumnNameAndDirection(workQueueParameters.SortBy)
        Return OrderBySqlGenerator.GetOrderByClause(sortData.SortDirection, sortData.ColumnName, WorkQueuePagingToken.IdColumnName)
    End Function
End Module