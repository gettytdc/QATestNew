Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Common.Security

Namespace Classes
    Public Class UserLogin : Implements IUserLogin

        Public ReadOnly Property LoggedIn As Boolean Implements IUserLogin.LoggedIn
            Get
                Return User.LoggedIn
            End Get
        End Property

        Public Sub Logout() Implements IUserLogin.Logout
            User.Logout()
        End Sub

        Public Function Login(machine As String, locale As String, Optional server As IServer = Nothing) As LoginResult Implements IUserLogin.Login
            Return User.Login(machine, locale, server)
        End Function

        Public Function Login(machine As String, username As String, password As SafeString, locale As String, Optional server As IServer = Nothing) As LoginResult Implements IUserLogin.Login
            Return User.Login(machine, username, password, locale, server)
        End Function

        Public Function LoginWithMappedActiveDirectoryUser(machine As String, locale As String, Optional server As IServer = Nothing) As LoginResult Implements IUserLogin.LoginWithMappedActiveDirectoryUser
            Return User.LoginWithMappedActiveDirectoryUser(machine, locale, server)
        End Function

        Public Function LoginAsAnonResource(machine As String, Optional server As IServer = Nothing) As LoginResult Implements IUserLogin.LoginAsAnonResource
            Return User.LoginAsAnonResource(machine, server)
        End Function

    End Class
End NameSpace