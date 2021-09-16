Imports System.Runtime.CompilerServices
Imports BluePrism.Server.Domain.Models.DataFilters
Imports BluePrism.Server.Domain.Models.Pagination

Public Module ScheduleParametersFilterCollector
    <Extension>
    Public Function GetSqlWhereClauses(ByVal scheduleParameters As ScheduleParameters, dbCommand As IDbCommand) _
        As IReadOnlyCollection(Of SqlClause)
        Dim whereClauses = new List(Of SqlClause)
        whereClauses.AddRange(scheduleParameters.Name.GetSqlWhereClauses(dbCommand, "s.name"))
        whereClauses.AddRange(scheduleParameters.RetirementStatus.GetSqlWhereClauses(dbCommand, "retired"))

        If TypeOf scheduleParameters.PagingToken Is Func.Some(Of SchedulePagingToken) Then
            Dim pagingToken = DirectCast(scheduleParameters.PagingToken, Func.Some(Of SchedulePagingToken)).Value

            Dim pagingTokenFilter = New PagingTokenDataFilter(Of SchedulePagingToken, String) With
                    {
                    .PagingToken = pagingToken
                    }

            whereClauses.Add(pagingTokenFilter.GetSqlWhereClauses(dbCommand, SchedulePagingToken.IdColumnName))
        End If
        Return whereClauses
    End Function
End Module
