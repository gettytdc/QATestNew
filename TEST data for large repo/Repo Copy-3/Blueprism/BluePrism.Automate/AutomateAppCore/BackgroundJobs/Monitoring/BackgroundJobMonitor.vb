Imports BluePrism.Core.Utility

Namespace BackgroundJobs.Monitoring

    ''' <summary>
    ''' Monitors progress of background jobs by getting updates from server, raising events
    ''' as the job progresses. 
    ''' </summary>
    Public Class BackgroundJobMonitor : Implements IDisposable

        ''' <summary>
        ''' Clock used to get time
        ''' </summary>
        Private ReadOnly mClock As ISystemClock

        ''' <summary>
        ''' Background job identifier
        ''' </summary>
        Private ReadOnly mJob As BackgroundJob

        ''' <summary>
        ''' The server instance used to query for job information
        ''' </summary>
        Private ReadOnly mServer As IServer

        ''' <summary>
        ''' Object that tells us when to check server for updates
        ''' </summary>
        Private ReadOnly mUpdateTrigger As IUpdateTrigger

        ''' <summary>
        ''' The maximum duration that we will wait after the last update of
        ''' a running job before treating the job as having timed out
        ''' </summary>
        Private ReadOnly mTimeout As TimeSpan

        ''' <summary>
        ''' Records the date of the last job data received from the server
        ''' </summary>
        Private mLastDateFromServer As DateTime = DateTime.MinValue

        ''' <summary>
        ''' The date on which we last received an update from the server
        ''' about a running job
        ''' </summary>
        Private mLastRunningUpdateDate As DateTime = DateTime.MinValue

        ''' <summary>
        ''' Used for
        ''' </summary>
        Private ReadOnly mUpdateLock As New Object()

        ''' <summary>
        ''' Indicates when job has completed or timed out and we are no longer
        ''' handling further updates
        ''' </summary>
        Private mDone As Boolean = False

        ''' <summary>
        ''' Creates a new BackgroundJobMonitor
        ''' </summary>
        ''' <param name="job">The background job to monitor</param>
        ''' <param name="server">The IServer used to check for job updates</param>
        ''' <param name="updateTrigger">An IUpdateTrigger that tells us when to check server for updates</param>
        ''' <param name="timeout">The maximum duration that we will wait for new updates before timing out</param>
        Public Sub New(job As BackgroundJob, server As IServer, updateTrigger As IUpdateTrigger, timeout As TimeSpan, clock As ISystemClock)
            If server Is Nothing Then Throw New ArgumentNullException(NameOf(server))
            mJob = job
            mServer = server
            mUpdateTrigger = updateTrigger
            mTimeout = timeout
            mClock = clock
        End Sub

        ''' <summary>
        ''' Event fired when job is running and updated data is available about progress
        ''' of the job
        ''' </summary>
        Public Event Running(sender As Object, args As BackgroundJobRunningEventArgs)

        ''' <summary>
        ''' Event fired when monitoring of the job has finished (with success or failure)
        ''' or due to an error or timeout that occured during monitoring
        ''' </summary>
        Public Event Done(sender As Object, args As BackgroundJobDoneEventArgs)

        ''' <summary>
        ''' Begins monitoring for updates.
        ''' </summary>
        Public Sub Start()
            AddHandler mUpdateTrigger.Update, AddressOf HandleUpdate
        End Sub

        Private Sub HandleUpdate(sender As Object, e As EventArgs)
            ' We don't have control over whether our trigger is firing events from multiple threads.
            ' A double-checked lock is used to prevent multiple updates from executing concurrently.
            ' This avoids further updates being made after a job has completed. 
            If Not mDone Then
                SyncLock mUpdateLock
                    If Not mDone Then
                        CheckForUpdate()
                    End If
                End SyncLock
            End If
        End Sub

        Private Sub CheckForUpdate()
            Dim jobData As BackgroundJobData
            Try
                jobData = mServer.GetBackgroundJob(mJob.Id, True)
            Catch exception As Exception
                RaiseDoneAndStopMonitoring(JobMonitoringStatus.MonitoringError, BackgroundJobData.Unknown, exception)
                Return
            End Try

            Select jobData.Status
                Case BackgroundJobStatus.Unknown
                    RaiseDoneAndStopMonitoring(JobMonitoringStatus.Missing, jobData)

                Case BackgroundJobStatus.Running
                    Dim currentDate = mClock.UtcNow.UtcDateTime
                    If jobData.Date > mLastDateFromServer Then
                        ' New update
                        mLastDateFromServer = jobData.Date
                        mLastRunningUpdateDate = currentDate
                        RaiseEvent Running(Me, New BackgroundJobRunningEventArgs(jobData))
                    ElseIf currentDate - mLastRunningUpdateDate > mTimeout Then
                        ' No new update within timeout interval
                        RaiseDoneAndStopMonitoring(JobMonitoringStatus.Timeout, jobData)
                    End If

                Case BackgroundJobStatus.Success
                    RaiseDoneAndStopMonitoring(JobMonitoringStatus.Success, jobData)

                Case BackgroundJobStatus.Failure
                    RaiseDoneAndStopMonitoring(JobMonitoringStatus.Failure, jobData)
            End Select
        End Sub

        ''' <summary>
        ''' Raises the Done event with the given data and stops monitoring
        ''' </summary>
        ''' <param name="status">The status of the job</param>
        ''' <param name="data">Data about the job</param>
        ''' <param name="exception">Exception that occured on client during monitoring</param>
        Private Sub RaiseDoneAndStopMonitoring(status As JobMonitoringStatus, data As BackgroundJobData, Optional exception As Exception = Nothing)
            mDone = True
            Dim result = New BackgroundJobResult(status, data, exception)
            RaiseEvent Done(Me, New BackgroundJobDoneEventArgs(result))
            RemoveHandler mUpdateTrigger.Update, AddressOf HandleUpdate
        End Sub

        ''' <summary>
        ''' Frees resources used by this BackgroundJobMonitor
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            RemoveHandler mUpdateTrigger.Update, AddressOf HandleUpdate
        End Sub

    End Class
End NameSpace