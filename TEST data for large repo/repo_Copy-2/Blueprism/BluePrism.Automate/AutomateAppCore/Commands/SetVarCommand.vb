Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore

Namespace Commands

    ''' <summary>
    ''' The 'setvar' command
    ''' </summary>
    Public Class SetVarCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "setvar"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return String.Format(
                    My.Resources.SetVarCommand_SetvarSessionidVarnameTypeValueDescriptionSetsTheValueOfASessionVariableWithinA)
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
            Dim suffix As String = command.Substring(7)
            Dim index As Integer = suffix.IndexOf(" "c)
            If index = -1 Then Return "MISSING PARAMETER"
            Dim variablesText As String = suffix.Substring(index + 1)
            Dim session As String = suffix.Substring(0, index)

            Dim svs As New List(Of clsSessionVariable)
            For Each varText In variablesText.Split(New Char() {","c})
                svs.Add(clsSessionVariable.Parse(varText & " """""))
            Next

            Dim sessId As Guid = GetNearestSessionID(session)
                If sessId = Nothing Then Return "INVALID ID"

                'If the listener is a controller of a pool, we need to check if the
                'session is being handled on another resource, and if so do it
                'there instead...
                If Listener.IsController Then
                    Dim sessionResourceId As Guid = gSv.GetSessionResourceID(sessId)
                    If sessionResourceId <> Listener.ResourceId Then
                        Dim errorCode As String = String.Empty
                        Dim resMachine = Listener.ResourceConnections.GetResource(sessionResourceId)
                        Dim reply As String = clsTalker.PoolControllerProxyCommand(resMachine,
                                                                                   Client.AuthenticatedUser,
                                                                                   Listener.PoolId,
                                                                                   command,
                                                                                   "SET",
                                                                                   5000,
                                                                                   Nothing,
                                                                                   False,
                                                                                   errorCode)
                        Return If(reply, errorCode)
                    End If
                End If

                Dim nr = Listener.FindRunner(sessId)
                If nr Is Nothing Then
                    Listener.RaiseError(
                        "Attempt to set session variable, but runner is missing. " &
                        "Controller:{0} Command:{1}", Listener.IsController(), command)
                    Return "MISSING RUNNER"
                End If
                Dim sess As clsSession = nr.Session
            ' If null, it probably hasn't started yet!
            If sess Is Nothing Then Return "SESSION UNAVAILABLE"

            For Each sv In svs
                Dim message = GetLoggableMessage(Client, sv, sess)

                nr.Log.SetEnvironmentVariable(New LogInfo(), message)

                sess.SetVar(sv.Name, sv.Value)
            Next
            Return "SET"

        End Function

        ''' <summary>
        ''' Creates a log message for adding to the session audit log when the
        ''' session variable has been changed in control room.  Handling the absence
        ''' of the previous session variable value.
        ''' </summary>
        ''' <param name="client">The current ListenerClient.</param>
        ''' <param name="sv">The session variable containing the new value to set
        ''' into the session. </param>
        ''' <param name="sess">The current session. </param>
        ''' <returns>A log message. </returns>
        Private Function GetLoggableMessage(client As IListenerClient,
                                            sv As clsSessionVariable,
                                            sess As clsSession) As String

            Dim sb = New StringBuilder()
            sb.AppendFormat("Session variable '{0}' changed", sv.Name)

            Dim pvOld = sess.GetVar(sv.Name)
            If pvOld IsNot Nothing Then _
                sb.AppendFormat(" from '{0}'", pvOld.LoggableValue)

            sb.AppendFormat(" to '{0}' by '{1}'", sv.Value.LoggableValue, client.UserName)

            sb.AppendFormat(" on {0}", client.RemoteHostIdentity)

            Return sb.ToString()

        End Function

    End Class
End Namespace
