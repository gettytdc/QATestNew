Imports System.Runtime.CompilerServices
Imports BluePrism.Server.Domain.Models.DataFilters
Imports BluePrism.Server.Domain.Models.Pagination

Public Module WorkQueueParametersFilterCollector

    <Extension()>
    Public Function GetSqlWhereClauses(ByVal workQueueParameters As WorkQueueParameters, dbCommand As IDbCommand) As IReadOnlyCollection(Of SqlClause)

        Dim whereClauses = GetPaginationSqlWhereClauses(workQueueParameters, dbCommand)
        Dim whereClauseForFilters = GetFilteringSqlWhereClauses(workQueueParameters, dbCommand)

        Return whereClauseForFilters.Concat(whereClauses).ToList()

    End Function

    Private Function GetPaginationSqlWhereClauses(ByVal workQueueParameters As WorkQueueParameters, dbCommand As IDbCommand) As IReadOnlyCollection(Of SqlClause)
        Dim whereClauses = New List(Of SqlClause)

        Dim sortData = ColumnSortProvider(Of WorkQueueSortByProperty).ColumnNameAndDirection(workQueueParameters.SortBy)

        If TypeOf workQueueParameters.PagingToken Is Func.Some(Of WorkQueuePagingToken) Then
            Dim pagingToken = DirectCast(workQueueParameters.PagingToken, Func.Some(Of WorkQueuePagingToken)).Value

            Dim pagingTokenFilter = New PagingTokenDataFilter(Of WorkQueuePagingToken, Integer) With
                    {
                    .PagingToken = pagingToken
                    }

            whereClauses.Add(pagingTokenFilter.GetSqlWhereClauses(dbCommand, sortData.ColumnName, sortData.SortDirection, WorkQueuePagingToken.IdColumnName))

        End If

        Return whereClauses
    End Function


    Public Function GetFilteringSqlWhereClauses(ByVal workQueueParameters As WorkQueueParameters, dbCommand As IDbCommand) As IReadOnlyCollection(Of SqlClause)
        Dim whereClauses = New List(Of SqlClause)

        whereClauses.AddRange(workQueueParameters.NameFilter.GetSqlWhereClauses(dbCommand, "name"))
        whereClauses.AddRange(workQueueParameters.QueueStatusFilter.GetSqlWhereClauses(dbCommand, "running"))
        whereClauses.AddRange(workQueueParameters.KeyFieldFilter.GetSqlWhereClauses(dbCommand, "keyfield"))
        whereClauses.AddRange(workQueueParameters.MaxAttemptsFilter.GetSqlWhereClauses(dbCommand, "maxattempts"))
        whereClauses.AddRange(workQueueParameters.PendingItemsCountFilter.GetSqlWhereClauses(dbCommand, "pending"))
        whereClauses.AddRange(workQueueParameters.CompletedItemsCountFilter.GetSqlWhereClauses(dbCommand, "completed"))
        whereClauses.AddRange(workQueueParameters.LockedItemsCountFilter.GetSqlWhereClauses(dbCommand, "locked"))
        whereClauses.AddRange(workQueueParameters.ExceptionedItemsCountFilter.GetSqlWhereClauses(dbCommand, "exceptioned"))
        whereClauses.AddRange(workQueueParameters.TotalItemCountFilter.GetSqlWhereClauses(dbCommand, "total"))
        whereClauses.AddRange(workQueueParameters.TotalCaseDurationFilter.GetSqlWhereClauses(dbCommand, "totalworktime"))
        whereClauses.AddRange(workQueueParameters.AverageItemWorkTimeFilter.GetSqlWhereClauses(dbCommand, "averageWorktime"))

        Return whereClauses
    End Function
End Module
