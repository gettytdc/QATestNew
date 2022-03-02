Public Class ServerSessionStatusPersister
    Implements ISessionStatusPersister

    Private ReadOnly mSessionId As Guid

    Public Sub New(sessionId As Guid)
        mSessionId = sessionId
    End Sub

    Public Sub SetPendingSessionRunning(sessionStarted As DateTimeOffset) Implements ISessionStatusPersister.SetPendingSessionRunning
        gSv.SetPendingSessionRunning(mSessionId, sessionStarted)
    End Sub

    Public Sub SetSessionTerminated(sessionExceptionDetail As SessionExceptionDetail) Implements ISessionStatusPersister.SetSessionTerminated
        gSv.SetSessionTerminated(mSessionId, DateTimeOffset.Now, sessionExceptionDetail)
    End Sub

    Public Sub SetSessionStopped() Implements ISessionStatusPersister.SetSessionStopped
        gSv.SetSessionStopped(mSessionId, DateTimeOffset.Now)
    End Sub

    Public Sub SetSessionCompleted() Implements ISessionStatusPersister.SetSessionCompleted
        gSv.SetSessionCompleted(mSessionId, DateTimeOffset.Now)
    End Sub

End Class
