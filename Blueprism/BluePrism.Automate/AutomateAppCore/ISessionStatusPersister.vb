''' <summary>
''' Saves changes to session status as it runs
''' </summary>
Public Interface ISessionStatusPersister
    Sub SetPendingSessionRunning(sessionStarted As DateTimeOffset)
    Sub SetSessionTerminated(sessionExceptionDetail As SessionExceptionDetail)
    Sub SetSessionStopped()
    Sub SetSessionCompleted()
End Interface
