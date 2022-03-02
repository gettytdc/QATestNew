Imports System.Runtime.CompilerServices
Imports BluePrism.Server.Domain.Models.Pagination

Public Module WorkQueueItemParametersOrderBy
    <Extension()>
    Public Function GetSqlOrderByClauses(ByVal workQueueItemParameters As WorkQueueItemParameters) As String
        Dim sortData = ColumnSortProvider(Of WorkQueueItemSortByProperty).ColumnNameAndDirection(workQueueItemParameters.SortBy)
        Return OrderBySqlGenerator.GetOrderByClause(sortData.SortDirection, sortData.ColumnName, WorkQueueItemPagingToken.IdColumnName)
    End Function
End Module
