Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.ClientServerResources.Core.Events

Namespace Sessions
    Public Class SessionDeleter
        Inherits SessionActionBase

        Private ReadOnly mRefresh As Action

        Public Sub New(server As IServer, resourceConnectionManager As IResourceConnectionManager, refresh As Action)
            MyBase.New(server, resourceConnectionManager)
            mRefresh = refresh
        End Sub

        Public Overrides Sub OnSessionDelete(sender As Object, e As SessionDeleteEventArgs)
            MyBase.OnSessionDelete(sender, e)
            mLog.Trace($"Received session deleted event from robot {e?.SessionId}")
        End Sub

        Public Function DeleteSessions(sessions As IEnumerable(Of clsProcessSession)) As Boolean
            StartAction()
            For Each deleteBatch In sessions.Batch(MaxItemsInSessionActionBatch)
                Dim batch As New List(Of DeleteSessionData)
                For Each session As clsProcessSession In deleteBatch
                    Dim resourceId = GetTargetResourceID(session)
                    batch.Add(New DeleteSessionData(resourceId, session.ProcessID, session.SessionID, Nothing, 0))
                Next

                Try
                    mResourceConnectionManager.SendDeleteSession(batch)
                    PauseForResponse()
                    mRefresh()
                Catch
                    batch.ForEach(Sub(session)
                                      Try
                                          ' Anything goes wrong, try and delete the sessions by brute force
                                          ' copied from original code, not sure how this would trigger
                                          Dim token = mServer.RegisterAuthorisationToken(session.ProcessId)
                                          mServer.DeleteSessionAs(token.ToString, session.SessionId)
                                      Catch
                                          ' Don't worry, it may have already been deleted.
                                      End Try
                                  End Sub)
                End Try
            Next
            mRefresh()
        End Function
    End Class
End Namespace
