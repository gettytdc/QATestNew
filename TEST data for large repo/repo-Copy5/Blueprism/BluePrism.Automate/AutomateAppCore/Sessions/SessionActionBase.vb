Imports System.Threading
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.ClientServerResources.Core.Events
Imports BluePrism.Server.Domain.Models
Imports NLog

Namespace Sessions

    Public MustInherit Class SessionActionBase
        Implements IDisposable

        Protected ReadOnly mServer As IServer
        Protected WithEvents mResourceConnectionManager As IResourceConnectionManager
        Protected Const MaxItemsInSessionActionBatch As Integer = 50
        Private ReadOnly mBulkSessionActionTimeout As Integer = 5000
        Private ReadOnly mSessionActionRateLimiter As New AutoResetEvent(True)
        Protected ReadOnly mLog As Logger = LogManager.GetCurrentClassLogger()
        Private mPoolCache As IDictionary(Of String, ResourcePoolInfo) = Nothing
        Private mIsDisposed As Boolean

        Public Sub New(server As IServer, resourceConnectionManager As IResourceConnectionManager)
            If (resourceConnectionManager Is Nothing) Then Throw New ArgumentException(My.Resources.ParameterCannotBeNull, NameOf(resourceConnectionManager))
            If (server Is Nothing) Then Throw New ArgumentException(My.Resources.ParameterCannotBeNull, NameOf(server))

            mServer = server
            mResourceConnectionManager = resourceConnectionManager
            mBulkSessionActionTimeout = mServer.GetPref(PreferenceNames.Session.SessionActionSendSignalMilliseconds, 5000)

            AddHandler mResourceConnectionManager.SessionCreate, AddressOf OnSessionCreate
            AddHandler mResourceConnectionManager.SessionDelete, AddressOf OnSessionDelete
            AddHandler mResourceConnectionManager.SessionStart, AddressOf OnSessionStart
            AddHandler mResourceConnectionManager.SessionStop, AddressOf OnSessionStop

        End Sub

        Protected Sub StartAction()
            mPoolCache = GetPoolLookupCache()
        End Sub

        Public Overridable Sub OnSessionDelete(sender As Object, e As SessionDeleteEventArgs)
            'mLog.Trace($"Released Wait {e?.SessionId}")
            mSessionActionRateLimiter.Set()
        End Sub

        Public Overridable Sub OnSessionCreate(sender As Object, e As SessionCreateEventArgs)
            'mLog.Trace($"Released Wait {e?.SessionId}")
            mSessionActionRateLimiter.Set()
        End Sub
        Public Overridable Sub OnSessionStart(sender As Object, e As SessionStartEventArgs)
            'mLog.Trace($"Released Wait {e?.SessionId}")
            mSessionActionRateLimiter.Set()
        End Sub
        Public Overridable Sub OnSessionStop(sender As Object, e As SessionStopEventArgs)
            'mLog.Trace($"Released Wait {e?.SessionId}")
            mSessionActionRateLimiter.Set()
        End Sub

        Protected Sub PauseForResponse()
            mLog.Trace("Sent action waiting for response")
            mSessionActionRateLimiter.Reset()
            mSessionActionRateLimiter.WaitOne(mBulkSessionActionTimeout)
            mLog.Trace("Released on to next action")
        End Sub

        Private Function GetPoolLookupCache() As IDictionary(Of String, ResourcePoolInfo)
            Dim resourcePoolInfoRaw = mServer.GetResourcesPoolInfo()
            Return resourcePoolInfoRaw.ToDictionary(Function(a) a.ResourceName, Function(b) b)
        End Function

        Protected Function GetTargetResourceID(session As clsProcessSession) As Guid
            Dim info = mPoolCache(session.ResourceName)
            If Not info.PoolId.Equals(Guid.Empty) Then
                If info.PoolControllerId.Equals(Guid.Empty) Then
                    Throw New ResourceUnavailableException(My.Resources.clsProcessSession_TheControllerOfThatResourcePoolIsCurrentlyOffline)
                End If
                Return info.PoolId
            End If
            Return info.ResourceId
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not mIsDisposed AndAlso disposing Then
                RemoveHandler mResourceConnectionManager.SessionCreate, AddressOf OnSessionCreate
                RemoveHandler mResourceConnectionManager.SessionDelete, AddressOf OnSessionDelete
                RemoveHandler mResourceConnectionManager.SessionStart, AddressOf OnSessionStart
                RemoveHandler mResourceConnectionManager.SessionStop, AddressOf OnSessionStop
            End If
            mIsDisposed = True
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
        End Sub
    End Class

End Namespace
