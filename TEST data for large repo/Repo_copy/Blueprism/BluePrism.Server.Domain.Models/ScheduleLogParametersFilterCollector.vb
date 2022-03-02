Imports System.Runtime.CompilerServices
Imports BluePrism.Server.Domain.Models.DataFilters
Imports BluePrism.Server.Domain.Models.Pagination

Public Module ScheduleLogParametersFilterCollector
    <Extension()>
    Public Function GetSqlWhereClauses(ByVal scheduleLogParameters As ScheduleLogParameters, dbCommand As IDbCommand) As IReadOnlyCollection(Of SqlClause)

        Dim whereClauses = New List(Of SqlClause)

        whereClauses.AddRange(scheduleLogParameters.ScheduleId.GetSqlWhereClauses(dbCommand, "scheduleId"))
        whereClauses.AddRange(scheduleLogParameters.StartTime.GetSqlWhereClauses(dbCommand, "startTime"))
        whereClauses.AddRange(scheduleLogParameters.EndTime.GetSqlWhereClauses(dbCommand, "endTime"))
        whereClauses.AddRange(scheduleLogParameters.ScheduleLogStatus.GetSqlWhereClauses(dbCommand, "status"))

        If TypeOf ScheduleLogParameters.PagingToken Is Func.Some(Of ScheduleLogsPagingToken) Then
            Dim pagingToken = DirectCast(ScheduleLogParameters.PagingToken, Func.Some(Of ScheduleLogsPagingToken)).Value

            Dim pagingTokenFilter = New PagingTokenDataFilter(Of ScheduleLogsPagingToken, Integer) With
                    {
                    .PagingToken = pagingToken
                    }

            whereClauses.Add(pagingTokenFilter.GetSqlWhereClauses(dbCommand, "startTime", "desc", ScheduleLogsPagingToken.IdColumnName))
        End If

        Return whereClauses

    End Function

End Module
