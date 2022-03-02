Imports System.Runtime.CompilerServices
Imports BluePrism.Server.Domain.Models.Pagination

Public Module ProcessSessionParametersOrderBy
    <Extension()>
    Public Function GetSqlOrderByClauses(ByVal processSessionParameters As ProcessSessionParameters) As String
        Dim sortData = ColumnSortProvider(Of ProcessSessionSortByProperty).ColumnNameAndDirection(processSessionParameters.SortBy)

        If processSessionParameters.SortBy = ProcessSessionSortByProperty.SessionNumberAsc OrElse processSessionParameters.SortBy = ProcessSessionSortByProperty.SessionNumberDesc Then
            Return OrderBySqlGenerator.GetOrderByClause(sortData.SortDirection, sortData.ColumnName)
        End If
        Return OrderBySqlGenerator.GetOrderByClause(sortData.SortDirection, sortData.ColumnName, SessionsPagingToken.IdColumnName)
    End Function
End Module
