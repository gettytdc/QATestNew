Imports System.Runtime.CompilerServices
Imports BluePrism.Server.Domain.Models.DataFilters

Imports BluePrism.Server.Domain.Models.Pagination

Public Module WorkQueueItemParametersFilterCollector
    <Extension()>
    Public Function GetSqlWhereClauses(ByVal workQueueItemParameters As WorkQueueItemParameters, dbCommand As IDbCommand) As IReadOnlyCollection(Of SqlClause)

        Dim whereClauses = GetPaginationSqlWhereClauses(workQueueItemParameters, dbCommand)
        Dim whereClauseForFilters = GetFilteringSqlWhereClauses(workQueueItemParameters, dbCommand)

        Return whereClauseForFilters.Concat(whereClauses).ToList()

    End Function

    Private Function GetPaginationSqlWhereClauses(ByVal workQueueItemParameters As WorkQueueItemParameters, dbCommand As IDbCommand) As IReadOnlyCollection(Of SqlClause)
        Dim whereClauses = New List(Of SqlClause)

        Dim sortData = ColumnSortProvider(Of WorkQueueItemSortByProperty).ColumnNameAndDirection(workQueueItemParameters.SortBy)

        If TypeOf workQueueItemParameters.PagingToken Is Func.Some(Of WorkQueueItemPagingToken) Then
            Dim pagingToken = DirectCast(workQueueItemParameters.PagingToken, Func.Some(Of WorkQueueItemPagingToken)).Value

            Dim pagingTokenFilter = New PagingTokenDataFilter(Of WorkQueueItemPagingToken, Long) With
                    {
                    .PagingToken = pagingToken
                    }

            whereClauses.Add(pagingTokenFilter.GetSqlWhereClauses(dbCommand, sortData.ColumnName, sortData.SortDirection, WorkQueueItemPagingToken.IdColumnName))

        End If

        Return whereClauses
    End Function

    Private Function GetFilteringSqlWhereClauses(ByVal workQueueItemParameters As WorkQueueItemParameters, dbCommand As IDbCommand) As IReadOnlyCollection(Of SqlClause)
        Dim whereClauses = New List(Of SqlClause)

        whereClauses.AddRange(workQueueItemParameters.KeyValue.GetSqlWhereClauses(dbCommand, "keyvalue"))
        whereClauses.AddRange(workQueueItemParameters.Status.GetSqlWhereClauses(dbCommand, "status"))
        whereClauses.AddRange(workQueueItemParameters.ExceptionReason.GetSqlWhereClauses(dbCommand, "exceptionreason"))
        whereClauses.AddRange(workQueueItemParameters.WorkTime.GetSqlWhereClauses(dbCommand, "worktime"))
        whereClauses.AddRange(workQueueItemParameters.Attempt.GetSqlWhereClauses(dbCommand, "attempt"))
        whereClauses.AddRange(workQueueItemParameters.Priority.GetSqlWhereClauses(dbCommand, "priority"))
        whereClauses.AddRange(workQueueItemParameters.LastUpdated.GetSqlWhereClauses(dbCommand, "lastupdated"))
        whereClauses.AddRange(workQueueItemParameters.LoadedDate.GetSqlWhereClauses(dbCommand, "loaded"))
        whereClauses.AddRange(workQueueItemParameters.LockedDate.GetSqlWhereClauses(dbCommand, "locktime"))
        whereClauses.AddRange(workQueueItemParameters.DeferredDate.GetSqlWhereClauses(dbCommand, "deferred"))
        whereClauses.AddRange(workQueueItemParameters.CompletedDate.GetSqlWhereClauses(dbCommand, "completed"))
        whereClauses.AddRange(workQueueItemParameters.ExceptionedDate.GetSqlWhereClauses(dbCommand, "exception"))

        Return whereClauses

    End Function

End Module
