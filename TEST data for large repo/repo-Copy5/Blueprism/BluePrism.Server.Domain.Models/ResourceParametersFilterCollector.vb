Imports System.Runtime.CompilerServices
Imports BluePrism.Server.Domain.Models.DataFilters
Imports BluePrism.Server.Domain.Models.Pagination

Public Module ResourceParametersFilterCollector
    <Extension>
    Public Function GetSqlWhereClauses(resourceParameters As ResourceParameters, dbCommand As IDbCommand) _
        As IReadOnlyCollection(Of SqlClause)
        Dim whereClauses = GetSqlPaginationWhereClauses(resourceParameters, dbCommand)
        Dim whereClauseForFilters = GetSqlFilteringWhereClauses(resourceParameters, dbCommand)

        Return whereClauseForFilters.Concat(whereClauses).ToList()
    End Function

    <Extension>
    Private Function GetSqlPaginationWhereClauses(resourceParameters As ResourceParameters, dbCommand As IDbCommand) _
        As IReadOnlyCollection(Of SqlClause)
        Dim whereClauses = New List(Of SqlClause)

        If TypeOf resourceParameters.PagingToken Is Func.Some(Of ResourcePagingToken) Then
            Dim pagingToken = DirectCast(resourceParameters.PagingToken, Func.Some(Of ResourcePagingToken)).Value

            Dim pagingTokenFilter = New PagingTokenDataFilter(Of ResourcePagingToken, String) With
                    {
                    .PagingToken = pagingToken
                    }

            Dim sortData = ColumnSortProvider(Of ResourceSortBy).ColumnNameAndDirection(resourceParameters.SortBy)
            whereClauses.Add(pagingTokenFilter.GetSqlWhereClauses(dbCommand, sortData.ColumnName, sortData.SortDirection, ResourcePagingToken.IdColumnName))
        End If
        Return whereClauses
    End Function

    <Extension>
    Public Function GetSqlOrderByClause(resourceParameters As ResourceParameters) As String
        Dim sortData = ColumnSortProvider(Of ResourceSortBy).ColumnNameAndDirection(resourceParameters.SortBy)
        Return OrderBySqlGenerator.GetOrderByClause(sortData.SortDirection, sortData.ColumnName, ResourcePagingToken.IdColumnName)
    End Function

    <Extension>
    Public Function GetSqlFilteringWhereClauses(resourceParameters As ResourceParameters, dbCommand As IDbCommand) _
        As IReadOnlyCollection(Of SqlClause)
        Dim whereClauses = New List(Of SqlClause)

        whereClauses.AddRange(resourceParameters.Name.GetSqlWhereClauses(dbCommand, "r.name"))
        whereClauses.AddRange(resourceParameters.GroupName.GetSqlWhereClauses(dbCommand, "g.name"))
        whereClauses.AddRange(resourceParameters.PoolName.GetSqlWhereClauses(dbCommand, "poolRes.name"))
        whereClauses.AddRange(resourceParameters.ActiveSessionCount.GetSqlWhereClauses(dbCommand, "r.actionsrunning"))
        whereClauses.AddRange(resourceParameters.PendingSessionCount.GetSqlWhereClauses(dbCommand, "(r.processesrunning - r.actionsrunning)"))
        whereClauses.AddRange(resourceParameters.DisplayStatus.GetSqlWhereClauses(dbCommand, "s.displayStatus"))

        Return whereClauses
    End Function

End Module
