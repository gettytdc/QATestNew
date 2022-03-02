Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.ClientServerResources.Core.Enums

Namespace Commands

    ''' <summary>
    ''' The 'outputs' command
    ''' </summary>
    Public Class OutputsCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "outputs"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.OutputsCommand_UseOutputsSessionidOrOutputsLastToGetTheOutputParametersFromAProcessOrActionTha
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

            Dim splitten As String() = Split(command)
            Dim sessId As Guid
            If splitten(1).ToLower() = "last" _
                Then sessId = Client.LastSessionCreated _
                Else sessId = GetNearestSessionID(splitten(1))
            If sessId = Nothing Then Return "INVALID ID"

            SyncLock Listener.Runners.Lock
                Dim cr As RunnerRecord = Listener.FindRunner(sessId)
                If cr Is Nothing Then Return "SESSION NOT FOUND"

                Select Case cr.Status
                    Case RunnerStatus.COMPLETED
                        'A Business Object goes back to idle status.
                        If cr.mAction IsNot Nothing Then
                            cr.Status = RunnerStatus.IDLE
                            cr.mReviewStatus = True
                        End If

                        ' Return the outputs, making sure there are no line breaks in
                        ' the XML...
                        Dim sOutputsXML As String = cr.mOutputs.ArgumentsToXML(True)
                        sOutputsXML = sOutputsXML.Replace("\r", "")
                        sOutputsXML = sOutputsXML.Replace("\n", "")
                        Return "OUTPUTS:" & sOutputsXML

                    Case RunnerStatus.FAILED

                        'A Business Object goes back to idle status.
                        If cr.mAction IsNot Nothing Then
                            cr.Status = RunnerStatus.IDLE
                            cr.mReviewStatus = True
                        End If
                        Return "FAILED:" & cr.mFailReason

                    Case Else
                        'If the session hasn't completed, respond with the status.
                        Return "SESSION " & cr.Status.ToString()
                End Select

            End SyncLock

        End Function

    End Class
End Namespace
