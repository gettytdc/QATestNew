Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.BackgroundJobs

' Server functionality for background jobs

Public Partial Class clsServer

    Private ReadOnly Property JobDataStore As New BackgroundJobDataStore

    ''' <summary>
    ''' Gets id for a new background job
    ''' </summary>
    Private Shared Function GetNewBackgroundJobId() As Guid
        Return Guid.NewGuid
    End Function

    ''' <summary>
    ''' Gets data containing information about status of a background job
    ''' </summary>
    ''' <param name="id">Background job identifier</param>
    <SecuredMethod(True)>
    Public Function GetBackgroundJob(id As Guid, clearWhenComplete As Boolean) _
        As BackgroundJobData Implements IServer.GetBackgroundJob
        CheckPermissions()
        Return JobDataStore.GetBackgroundJob(id, clearWhenComplete)
    End Function

    ''' <summary>
    ''' Clears data held for a background job
    ''' </summary>
    ''' <param name="id">Background job identifier</param>
    <SecuredMethod(True)>
    Public Sub ClearBackgroundJob(id As Guid) Implements IServer.ClearBackgroundJob
        CheckPermissions()
        JobDataStore.RemoveBackgroundJob(id)
    End Sub

    ''' <summary>
    ''' Updates the data held for a background job - a convenience method used by long-running 
    ''' server operations
    ''' </summary>
    ''' <param name="id">Background job identifier</param>
    ''' <param name="notifier">The notifier used to signal that update has been made</param>
    ''' <param name="percentComplete">A value between 0-100 indicating progress of the job</param>
    ''' <param name="status">Job status</param>
    ''' <param name="description">Optional description of the current state of the job</param>
    ''' <param name="exception">Exception supplied when job has failed</param>
    Private Sub UpdateBackgroundJob(id As Guid,
                                    notifier As BackgroundJobNotifier,
                                    percentComplete As Integer,
                                    status As BackgroundJobStatus,
                                    Optional description As String = Nothing,
                                    Optional exception As Exception = Nothing,
                                    Optional resultData As Object = Nothing)
        Dim [error] As BackgroundJobError = _
            If(exception IsNot Nothing, New BackgroundJobError(exception), Nothing)
        Dim data As New BackgroundJobData(status, percentComplete, description,
            DateTime.UtcNow, [error], resultData)
        JobDataStore.UpdateJob(id, data)
        Try
            ' Firing the Updated event will fail if running via a server connection 
            ' using remoting with the callback port disabled (error message "This 
            ' remoting proxy has no channel sink". The error handling here replicates
            ' the earlier callback implementation (clsProgressMonitor and 
            ' inner clsServer.MonitorWrapper class) that suppressed these exceptions 
            ' in the same way.
            notifier.Notify
        Catch ex As Exception
            Debug.WriteLine(ex.ToString())
        End Try
    End Sub

    ''' <summary>
    ''' Performs general cleanup of background job data, ensuring that data is eventually removed 
    ''' for any jobs that have stalled and stopped updating their data or where clients have stopped
    ''' polling for successful / failed jobs. 
    ''' </summary>
    ''' <remarks>
    ''' As only a small amount of data is held in memory for each job, it is sufficient for this 
    ''' to be initiated from the server methods that creates background jobs. A scheduled cleanup
    ''' mechanism could be considered if background jobs were to be extended to other server methods.
    ''' </remarks>
    Private Sub CleanUpExpiredBackgroundJobs()
        Dim expiredJobInterval = TimeSpan.FromMinutes(30)
        JobDataStore.RemoveExpiredJobs(DateTime.UtcNow.Subtract(expiredJobInterval))
    End Sub
End Class