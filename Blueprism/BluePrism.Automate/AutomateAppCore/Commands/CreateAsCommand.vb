Imports BluePrism.BPCoreLib

Namespace Commands



    Public Class CreateAsCommand
        Inherits CreateCommand

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name As String
            Get
                Return "createas"
            End Get
        End Property

        Public Overrides ReadOnly Property Help As String
            Get
                Return My.Resources.CreateAsCommand_UsesTokenAuthenticationToEitherCreateAPendingSessionWithGivenProcessIDCreateasT
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired As CommandAuthenticationMode
            Get
                Return CommandAuthenticationMode.Authed
            End Get
        End Property

        Protected Overrides Function Exec(command As String) As String
            Dim output As String
            Try

                'Reset the last session created, so there isn't one if this fails.
                Client.LastSessionCreated = Guid.Empty
                'A user must be set before you can create a session...
                If Not Client.UserSet Then
                    output = "USER NOT SET" & vbCrLf
                    Return output
                End If
                'create {token} {processid} | create {token} {processid} {queue} {sessionid} | create name {token} {name}
                Dim token As clsAuthToken
                'Get the process id, and make sure it is valid...
                Dim procId As Guid
                Dim procName As String
                Dim queueIdent = 0
                Dim sessionId As Guid

                If command.StartsWith("createas name ") Then
                    token = New clsAuthToken(command.Mid(15, clsAuthToken.TokenLength))
                    procName = command.Substring(14 + clsAuthToken.TokenLength)
                    procId = gSv.GetProcessIDByName(procName, True)
                    If procId.Equals(Guid.Empty) Then
                        output = "NO SUCH PROCESS" & vbCrLf
                        Return output
                    End If
                Else
                    token = New clsAuthToken(command.Mid(10, clsAuthToken.TokenLength))
                    Dim ids() As String = command.Mid(11 + clsAuthToken.TokenLength).Split(" "c)

                    If ids.Count = 0 Then Return "INVALID ID"

                    procId = GetNearestProcID(ids(0))
                    If procId = Nothing Then Return "INVALID ID"


                    If ids.Count = 2 Then
                        If Not Integer.TryParse(ids(1), queueIdent) Then
                            Return "INVALID QUEUE IDENTITY"
                        End If
                    ElseIf ids.Count > 2 Then
                        If Not Integer.TryParse(ids(1), queueIdent) Then
                            Return "INVALID QUEUE IDENTITY"
                        End If

                        If Not Guid.TryParse(ids(2), sessionId) Then
                            Return "INVALID SESSION IDENTITY"
                        End If
                    End If

                End If

                'Ensure we have a valid session guid
                If sessionId = Guid.Empty Then
                    sessionId = Guid.NewGuid
                End If

                Return CreateSession(token, procId, queueIdent, sessionId)

            Catch ex As Exception
                output = ex.Message
                output &= vbCrLf
                Return output
            End Try

        End Function
    End Class

End Namespace
