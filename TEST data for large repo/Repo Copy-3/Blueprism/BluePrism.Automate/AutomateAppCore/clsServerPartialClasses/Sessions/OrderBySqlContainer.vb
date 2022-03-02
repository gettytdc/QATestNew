Namespace clsServerPartialClasses.Sessions
    Public Class OrderBySqlContainer
        Public ReadOnly Property SelectSql As String
        Public ReadOnly Property OrderBySql As String

        Public Sub New(selectSql As String, orderBySql As String)
            Me.SelectSql = selectSql
            Me.OrderBySql = orderBySql
        End Sub

    End Class
End Namespace
