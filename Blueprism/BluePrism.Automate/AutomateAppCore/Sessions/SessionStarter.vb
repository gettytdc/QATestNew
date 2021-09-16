Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.ClientServerResources.Core.Events
Imports BluePrism.Core.Extensions
Imports BluePrism.Server.Domain.Models

Namespace Sessions
    Public Delegate Sub SessionStartingEventHandler(sender As Object, e As SessionStartingEventArgs)

    Public Class SessionStarter
        Inherits SessionActionBase

        Private ReadOnly mRefresh As Action
        Private ReadOnly mUserMessage As Action(Of String)

        Public Event SessionStarting As SessionStartingEventHandler

        Public Sub New(resourceConnectionManager As IResourceConnectionManager, server As IServer, refresh As Action, message As Action(Of String))
            MyBase.New(server, resourceConnectionManager)
            mUserMessage = message
            mRefresh = refresh
        End Sub

        Public Overrides Sub OnSessionStart(sender As Object, e As SessionStartEventArgs)
            MyBase.OnSessionStart(sender, e)
            mLog.Trace($"Super - Received session started event from robot {e?.SessionId}")
        End Sub

        Public Sub StartSession(sessions As IEnumerable(Of clsProcessSession))
            StartAction()
            For Each sessionBatch As IEnumerable(Of clsProcessSession) In sessions.Batch(MaxItemsInSessionActionBatch)
                If Not sessionBatch.All(Function(x) ValidateStartSession(x)) Then
                    Exit Sub
                End If

                RunProcess(sessionBatch)
                PauseForResponse()
                mRefresh()
            Next
            mRefresh()
        End Sub

        Private Function ValidateStartSession(session As clsProcessSession) As Boolean
            Dim resourceId As Guid = GetTargetResourceID(session)

            'Check resource status before proceeding...
            Dim resourceMachine = mResourceConnectionManager.GetResource(resourceId)
            If resourceMachine Is Nothing Then
                mUserMessage(String.Format(My.Resources.NotConnectedTo0CanTStartProcess, session.ResourceName))
                Return False
            End If
            Dim errorMessage As String = String.Empty
            If Not resourceMachine.CheckResourcePCStatus(session.ResourceName, errorMessage) Then
                Throw New ResourceUnavailableException(errorMessage)
                Return False
            End If

            session.ResourceID = resourceId

            Return True
        End Function

        Private Sub RunProcess(sessions As IEnumerable(Of clsProcessSession))
            Dim sessionData As New List(Of StartSessionData)

            For Each session In sessions
                Dim inputParametersXML As String = Nothing
                If session.HasArguments Then inputParametersXML = session.ArgumentsXml

                'Lets make sure the xml we send over the network stream is all on one line
                inputParametersXML = Replace(inputParametersXML, vbCr, String.Empty)
                inputParametersXML = Replace(inputParametersXML, vbLf, String.Empty)

                sessionData.Add(New StartSessionData(session.ResourceID, session.ProcessID, session.SessionID, inputParametersXML, Nothing, Nothing, 0))
            Next
            Try
                mResourceConnectionManager.SendStartSession(sessionData)
                mRefresh()
                RaiseEvent SessionStarting(Me, New SessionStartingEventArgs(sessions))
            Catch ex As Exception
                mUserMessage(String.Format(My.Resources.FailedToSendStartInstructionToResourcePC0, ex.Message))
                Exit Sub
            Finally
                mRefresh()
            End Try
        End Sub
    End Class
End Namespace
