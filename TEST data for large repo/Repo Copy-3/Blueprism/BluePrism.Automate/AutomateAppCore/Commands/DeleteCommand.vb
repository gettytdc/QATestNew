Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib

Namespace Commands

    ''' <summary>
    ''' The 'delete' command
    ''' </summary>
    Public Class DeleteCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "delete"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.DeleteCommand_DeleteAPendingSessionDeleteSessionidUseDeleteasInsteadWhenTheSessionManagementE
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
            Try

                Dim gID As Guid = GetNearestSessionID(command.Mid(8))
                If gID = Nothing Then Return "INVALID ID"

                Return DeleteSession(Nothing, gID)
            Catch ex As Exception
                Return ex.Message & vbCrLf
            End Try

        End Function

        Protected Function DeleteSession(token As clsAuthToken, gId As Guid) As String
            Try

                ' Find the runner record with the given ID and remove it from the
                ' runner record list
                Dim rr = Listener.FindRunner(gId)
                If rr Is Nothing Then
                    'If we didn't find the runner record, then either there's an error (presumably
                    'on the caller's part!) or maybe we are a pool controller and the session to
                    'be deleted is not here, but on one of our pool members...
                    If Listener.IsController() Then
                        Dim resid As Guid = gSv.GetSessionResourceID(gId)
                        If resid = Guid.Empty Then Return "MISSING SESSION"
                        Dim res = Listener.ResourceConnections.GetResource(resid)
                        If res IsNot Nothing Then

                            Dim errcode As String = Nothing
                            Dim deleteCmd As String
                            If token IsNot Nothing Then
                                deleteCmd = $"deleteas {token.ToString} {gId}"
                            Else
                                deleteCmd = $"delete {gId}"
                            End If
                            Dim reply As String = clsTalker.PoolControllerProxyCommand(res, Client.AuthenticatedUser, Listener.PoolId, deleteCmd, "SESSION DELETED", 10000, Nothing, False, errcode)
                            If reply Is Nothing Then
                                Return errcode
                            End If
                            Return reply
                        End If
                    End If

                    Return "MISSING RUNNER"

                End If

                Try
                    If gSv.GetControllingUserPermissionSetting() Then
                        gSv.DeleteSessionAs(token?.ToString, rr.SessionID)
                    Else
                        gSv.DeleteSession(rr.SessionID)
                    End If
                    Listener.Runners.Remove(gId, rr)
                Catch ex As Exception
                    Return "ERROR:Could not update database following deletion - " & ex.Message
                End Try

                Listener.AddNotification("DELETED " & gId.ToString())
                Listener.NotifyStatus()
                Listener.RaiseInfo("Session deleted - ID: {0}", gId)
                Return "SESSION DELETED : " & gId.ToString()

            Finally
                Listener.Availability = Listener.Runners.Availability
            End Try

        End Function

    End Class
End Namespace
