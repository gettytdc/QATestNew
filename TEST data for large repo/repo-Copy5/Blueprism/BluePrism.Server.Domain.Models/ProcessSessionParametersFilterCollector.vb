Imports System.Runtime.CompilerServices
Imports BluePrism.Server.Domain.Models.DataFilters
Imports BluePrism.Server.Domain.Models.Pagination

Public Module ProcessSessionParametersFilterCollector
    <Extension()>
    Public Function GetSqlWhereClauses(ByVal processSessionParameters As ProcessSessionParameters, dbCommand As IDbCommand) As IReadOnlyCollection(Of SqlClause)

        Dim whereClauses = GetPaginationSqlWhereClauses(processSessionParameters, dbCommand)
        Dim whereClauseForFilters = GetFilteringSqlWhereClauses(processSessionParameters, dbCommand)

        Return whereClauseForFilters.Concat(whereClauses).ToList()

    End Function

    Private Function GetPaginationSqlWhereClauses(ByVal processSessionParameters As ProcessSessionParameters, dbCommand As IDbCommand) As IReadOnlyCollection(Of SqlClause)
        Dim whereClauses = New List(Of SqlClause)

        Dim sortData = ColumnSortProvider(Of ProcessSessionSortByProperty).ColumnNameAndDirection(processSessionParameters.SortBy)

        If TypeOf processSessionParameters.PagingToken Is Func.Some(Of SessionsPagingToken) Then
            Dim pagingToken = DirectCast(processSessionParameters.PagingToken, Func.Some(Of SessionsPagingToken)).Value

            Dim pagingTokenFilter = New PagingTokenDataFilter(Of SessionsPagingToken, Long) With
                    {
                    .PagingToken = pagingToken
                    }

            whereClauses.Add(pagingTokenFilter.GetSqlWhereClauses(dbCommand, sortData.ColumnName, sortData.SortDirection, SessionsPagingToken.IdColumnName))

        End If

        Return whereClauses
    End Function

    Private Function GetFilteringSqlWhereClauses(ByVal processSessionParameters As ProcessSessionParameters, dbCommand As IDbCommand) As IReadOnlyCollection(Of SqlClause)
        Dim whereClauses = New List(Of SqlClause)

        whereClauses.AddRange(processSessionParameters.SessionNumber.GetSqlWhereClauses(dbCommand, "sessionnumber"))
        whereClauses.AddRange(processSessionParameters.ProcessName.GetSqlWhereClauses(dbCommand, "processname"))
        whereClauses.AddRange(processSessionParameters.Status.GetSqlWhereClauses(dbCommand, "statusid"))
        whereClauses.AddRange(processSessionParameters.User.GetSqlWhereClauses(dbCommand, "starterusername"))
        whereClauses.AddRange(processSessionParameters.LatestStage.GetSqlWhereClauses(dbCommand, "laststage"))
        whereClauses.AddRange(processSessionParameters.ResourceName.GetSqlWhereClauses(dbCommand, "starterresourcename"))
        whereClauses.AddRange(processSessionParameters.StageStarted.GetSqlWhereClauses(dbCommand, "dateadd(SECOND, -lastupdatedtimezoneoffset, lastupdated)"))
        whereClauses.AddRange(processSessionParameters.StartTime.GetSqlWhereClauses(dbCommand, "dateadd(SECOND, -starttimezoneoffset, startdatetime)"))
        whereClauses.AddRange(processSessionParameters.EndTime.GetSqlWhereClauses(dbCommand, "dateadd(SECOND, -endtimezoneoffset, enddatetime)"))

        Return whereClauses
    End Function

End Module
