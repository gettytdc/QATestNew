Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Common.Security
Imports BluePrism.Server.Domain.Models

Namespace Commands

    Public Class TokenCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer,
                       memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name As String = "getauthtoken"
        Public Overrides ReadOnly Property Help As String
            Get
                Return My.Resources.TokenCommand_GetAnAuthorisationTokenUsingTheProvidedCredentialsUseGetauthtokenProcessidUseri
            End Get
        End Property             
        
        Public Overrides ReadOnly Property CommandAuthenticationRequired As CommandAuthenticationMode = CommandAuthenticationMode.Any
                
        Protected Overrides Function Exec(command As String) As String
            Try
                Const authenticationFailed = "AUTHENTICATION FAILED"
                Const invalidParameters = "INVALID PARAMETERS"
                Const upnNotSupported = "UPN NOT SUPPORTED FOR SSO DATABASE"

                Dim commandLength = Name.Length                                
                Dim parameters = command.Substring(commandLength + 1).Split(Nothing)

                If parameters.Length < 3 Then Return invalidParameters 

                Dim processId As Guid
                Dim userName As String
                Dim password As SafeString
                Dim authMode As AuthMode

                Dim isUpnCommand = parameters(0).Equals("upn", StringComparison.OrdinalIgnoreCase)                

                If isUpnCommand Then
                    If parameters.Length < 4 Then Return invalidParameters

                    Dim isSsoDatabase = Server.DatabaseType() = DatabaseType.SingleSignOn
                    If isSsoDatabase Then Return upnNotSupported

                    userName = parameters(1)
                    password = New SafeString(parameters(2))
                    processId = Guid.Parse(parameters(3))
                    authMode = AuthMode.MappedActiveDirectory
                Else
                    Dim userId = Guid.Parse(parameters(1))

                    processId = Guid.Parse(parameters(0))
                    userName = Server.GetUserName(userId)
                    password = New SafeString(parameters(2))
                    authMode = AuthMode.Unspecified                    
                End If

                Dim token = Server.RegisterAuthorisationToken(username, New SafeString(password), processId, authMode)
                If token Is Nothing Then Return authenticationFailed

                Return token.ToString()

            Catch ex As Exception
                Dim reply As String = ex.Message
                reply &= vbCrLf
                Return reply
            End Try



        End Function

    End Class
End Namespace
