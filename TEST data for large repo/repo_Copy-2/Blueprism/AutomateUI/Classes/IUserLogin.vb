Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Common.Security

Namespace Classes

    Public Interface IUserLogin
        ReadOnly Property LoggedIn As Boolean
        Function Login(machine As String, locale As String, Optional server As IServer = Nothing) As LoginResult
        Function LoginAsAnonResource(machine As String, Optional server As IServer = Nothing) As LoginResult
        Function LoginWithMappedActiveDirectoryUser(machine As String, locale As String, Optional server As IServer = Nothing) As LoginResult
        Function Login(machine As String, username As String, password As SafeString, locale As String, Optional server As IServer = Nothing) As LoginResult
        Sub Logout()
    End Interface
End NameSpace