Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.ClientServerResources.Core.Events

Namespace Sessions

    Public Class SessionCreator
        Inherits SessionActionBase

        Private ReadOnly mRefresh As Action
        Private ReadOnly mUserMessage As Action(Of String)

        Public Sub New(resourceConnectionManager As IResourceConnectionManager, server As IServer, refresh As Action, message As Action(Of String))
            MyBase.New(server, resourceConnectionManager)
            mUserMessage = message
            mRefresh = refresh
        End Sub

        Public Overrides Sub OnSessionCreate(sender As Object, e As SessionCreateEventArgs)
            MyBase.OnSessionCreate(sender, e)
            mLog.Trace($"Super - Received session created event from robot, released wait {e?.SessionId}")
        End Sub

        Public Sub CreateSessions(processes As IEnumerable(Of IGroupMember), resources As ICollection(Of IGroupMember))
            StartAction()

            Dim sessionData = ValidateAndFormat(processes, resources)
            Try
                BatchAndCreateSessions(sessionData)
            Catch ex As Exception                
                mUserMessage(String.Format(My.Resources.ErrorFailedToCreateSession0, ex.Message))
            End Try
        End Sub

        Private Function ValidateAndFormat(processes As IEnumerable(Of IGroupMember), resources As ICollection(Of IGroupMember)) As IEnumerable(Of CreateSessionData)
            Dim sessionData As New List(Of CreateSessionData)
            Dim errorMessage = String.Empty
            Dim attributesCache = mServer.GetProcessAttributesBulk(processes.Select(Function(x) x.IdAsGuid).ToList())

            For Each treeItem As IGroupMember In processes
                Dim process = TryCast(treeItem, ProcessGroupMember)
                If process Is Nothing Then Continue For

                Dim processAttributes = attributesCache(process.IdAsGuid)
                If Not processAttributes.HasFlag(ProcessAttributes.Published) Then
                    Throw New ApplicationException(My.Resources.ProcessNotPublished)
                End If

                For Each resourceTreeItem As IGroupMember In resources
                    Dim resource = TryCast(resourceTreeItem, ResourceGroupMember)
                    If resource Is Nothing Then Continue For

                    Dim resourceMachine = mResourceConnectionManager.GetResource(resource.IdAsGuid)
                    If Not resourceMachine.CheckResourcePCStatus(resource.Name, errorMessage) Then
                        Throw New ApplicationException(errorMessage)
                    End If
                    sessionData.Add(New CreateSessionData(resource.IdAsGuid, process.IdAsGuid, Nothing, Nothing, Nothing))
                Next
            Next

            Return sessionData
        End Function

        Private Function BatchAndCreateSessions(sessisonData As IEnumerable(Of CreateSessionData)) As Boolean
            Dim resourceBatches = sessisonData.Batch(MaxItemsInSessionActionBatch)

            For Each resourceBatch In resourceBatches
                mResourceConnectionManager.SendCreateSession(resourceBatch)
                PauseForResponse()
                mRefresh()
            Next
            mRefresh()
            ' TODO if any of this throws an error, the whole thing will just exit as it's running in a task.run. We should be handling these errors in the ui layer.
        End Function
    End Class
End Namespace
