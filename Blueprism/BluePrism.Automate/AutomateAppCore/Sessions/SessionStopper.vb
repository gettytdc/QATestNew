Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.ClientServerResources.Core.Events

Namespace Sessions

    Public Class SessionStopper
        Inherits SessionActionBase

        Private ReadOnly mRefresh As Action

        Public Sub New(server As IServer, resourceConnectionManager As IResourceConnectionManager, refresh As Action)
            MyBase.New(server, resourceConnectionManager)
            mRefresh = refresh
        End Sub

        Public Overrides Sub OnSessionStop(sender As Object, e As SessionStopEventArgs)
            MyBase.OnSessionStop(sender, e)
            mLog.Trace($"Super - Received session stopped event from robot {e?.SessionId}")
        End Sub

        Public Sub StopProcesses(sessions As IEnumerable(Of clsProcessSession))
            StartAction()
            Dim sessionData As New List(Of StopSessionData)

            For Each processSession In sessions
                'Get the ID of the resource we want to talk to
                Dim resourceId As Guid = GetTargetResourceID(processSession)
                sessionData.Add(New StopSessionData(resourceId, processSession.SessionID, 0))
            Next

            For Each sessionBatch In sessionData.Batch(MaxItemsInSessionActionBatch)
                Try
                    mResourceConnectionManager.SendStopSession(sessionBatch)
                    PauseForResponse()
                    mRefresh()
                Catch ex As Exception
                    Throw New Exception(String.Format(My.Resources.FailedToSendStopInstructionToResourcePC0, ex.Message))
                End Try
            Next
            mRefresh()
        End Sub
    End Class
End Namespace
