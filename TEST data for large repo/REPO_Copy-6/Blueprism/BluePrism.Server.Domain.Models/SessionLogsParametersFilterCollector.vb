Imports System.Runtime.CompilerServices
Imports BluePrism.Server.Domain.Models.DataFilters
Imports BluePrism.Server.Domain.Models.Pagination

Public Module SessionLogsParametersFilterCollector
    <Extension()>
    Public Function GetSqlWhereClauses(ByVal sessionLogsParameters As SessionLogsParameters, dbCommand As IDbCommand) As IReadOnlyCollection(Of SqlClause)
        Return GetPaginationSqlWhereClauses(sessionLogsParameters, dbCommand)
    End Function

    Private Function GetPaginationSqlWhereClauses(ByVal sessionLogsParameters As SessionLogsParameters, dbCommand As IDbCommand) As IReadOnlyCollection(Of SqlClause)
        Dim whereClauses = New List(Of SqlClause)


        If TypeOf sessionLogsParameters.PagingToken Is Func.Some(Of SessionLogsPagingToken) Then
            Dim pagingToken = DirectCast(sessionLogsParameters.PagingToken, Func.Some(Of SessionLogsPagingToken)).Value

            Dim pagingTokenFilter = New PagingTokenDataFilter(Of SessionLogsPagingToken, Long) With
                    {
                    .PagingToken = pagingToken
                    }

            whereClauses.Add(pagingTokenFilter.GetSqlWhereClauses(dbCommand, SessionLogsPagingToken.IdColumnName))

        End If

        Return whereClauses
    End Function
End Module
