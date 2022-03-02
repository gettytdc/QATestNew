Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib

Namespace Commands

    ''' <summary>
    ''' The 'startp' command
    ''' </summary>
    Public Class StartPCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "startp"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.StartPCommand_SetStartupParametersToBeUsedForTheNextSessionCreatedOnThisConnectionTheParamete
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired() As CommandAuthenticationMode
            Get
                Return CommandAuthenticationMode.Authed
            End Get
        End Property

        ''' <summary>
        ''' Validates that the given user has permission to perform this action on the give resource.
        ''' </summary>
        ''' <param name="authUser">the requesting user</param>
        ''' <param name="resourceId">the identity of the resource</param>
        ''' <returns></returns>
        Public Overrides Function CheckPermissions(authUser As IUser, resourceId As Guid) As (String, Boolean)
            If CheckUserHasPermissionOnResource(authUser, resourceId, Permission.Resources.ControlResource) Then
                Return ("", True)
            Else
                Return ("USER DOES NOT HAVE CONTROL RESOURCE PERMISSION FOR THIS RESOURCE", False)
            End If
        End Function

        Protected Overrides Function Exec(command As String) As String

            Dim xml As String = command.Mid(8)
            Client.StartupParameters = clsArgumentList.XMLToArguments(xml, False)
            Client.StartupParametersXml = xml
            Return "PARAMETERS SET"
        End Function

    End Class
End Namespace
