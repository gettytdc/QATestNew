Imports BluePrism.AutomateProcessCore

Namespace Commands

    ''' <summary>
    ''' The 'vars' command
    ''' </summary>
    Public Class VarsCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "vars"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.VarsCommand_GetsAllTheCurrentSessionVariablesAndTheirValuesForTheGivenSessionIDExampleRespo & vbCrLf
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired() As CommandAuthenticationMode
            Get
                Return CommandAuthenticationMode.Authed
            End Get
        End Property

        Protected Overrides Function Exec(command As String) As String

            If command.Length < 6 Then Return "NO SESSION SPECIFIED" & vbCrLf
            Dim sessId As Guid = GetNearestSessionID(command.Substring(5))
            If sessId = Nothing Then Return "INVALID ID"

            'If the listener is a controller of a pool, we need to check if the
            'session is being handled on another resource, and if so get them
            'from there instead...
            If Listener.IsController Then
                Dim sessionResourceId As Guid = gSv.GetSessionResourceID(sessId)
                If sessionResourceId <> Listener.ResourceId Then
                    Dim errorCode As String = Nothing
                    Dim resMachine = Listener.ResourceConnections.GetResource(sessionResourceId)
                    Dim reply As String = clsTalker.PoolControllerProxyCommand(resMachine,
                                                                               Client.AuthenticatedUser,
                                                                               Listener.PoolId,
                                                                               command,
                                                                               "VARIABLES:",
                                                                               10000,
                                                                               Nothing,
                                                                               True,
                                                                               errorCode)
                    Return If(reply, $"COMMUNICATION FAILED : {errorCode}")
                End If
            End If

            Dim nr = Listener.FindRunner(sessId)
            If nr Is Nothing Then Return "MISSING RUNNER"

            ' If no session it probably hasn't started yet, or has finished.
            Dim session As clsSession = nr.Session
            If session Is Nothing Then Return "SESSION UNAVAILABLE"

            Dim vars _
                    As Dictionary(Of String, clsProcessValue) = session.GetAllVars()

            With New StringBuilder()
                .Append("VARIABLES")
                For Each var As String In vars.Keys
                    .Append(New clsSessionVariable(var, vars(var)).ToString())
                    .Append("//END//")
                Next
                Return .ToString()
            End With

        End Function

    End Class
End Namespace
