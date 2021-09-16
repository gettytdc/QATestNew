Imports System.DirectoryServices.AccountManagement

Public NotInheritable Class GroupScopeStaticConverter
    Public Shared Function LocaliseGroupScope(value As GroupScope?) As String
        Select value
            Case GroupScope.Universal  
                Return My.Resources.SecurityGroup_Universal
            Case GroupScope.Global
                Return My.Resources.SecurityGroup_Global
            Case GroupScope.Local
                Return My.Resources.SecurityGroup_Local
            Case Else
                Throw New ArgumentException($"{NameOf(value)} not recognised as Universal, Global or Local")
        End Select
    End Function
End Class
