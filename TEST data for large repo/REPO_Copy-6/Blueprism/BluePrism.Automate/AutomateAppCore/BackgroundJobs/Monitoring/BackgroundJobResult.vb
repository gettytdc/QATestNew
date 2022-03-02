

Namespace BackgroundJobs.Monitoring

    ''' <summary>
    ''' Contains the outcome of BackgroundJobMonitor waiting for completion of a background job.
    ''' This combines the outcome of the job on the server together with any other problems that 
    ''' might have interrupted monitoring of the job.
    ''' </summary>
    Public Class BackgroundJobResult
    
        Private ReadOnly mStatus As JobMonitoringStatus
        Private ReadOnly mData As BackgroundJobData
        Private ReadOnly mException As Exception

        ''' <summary>
        ''' Creates a new BackgroundJobResult
        ''' </summary>
        ''' <param name="status">The status representing the outcome of the job</param>
        ''' <param name="data">Data about the job returned from the server</param>
        ''' <param name="exception">Exception that occured during monitoring</param>
        Public Sub New(status As JobMonitoringStatus, data As BackgroundJobData, 
                       Optional exception As Exception = Nothing)
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
            mStatus = status
            mData = data
            mException = exception
        End Sub

        ''' <summary>
        ''' The status representing the outcome of monitoring for the completion of the job
        ''' </summary>
        Public ReadOnly Property Status as JobMonitoringStatus
            Get
                return mStatus
            End Get
        End Property

        ''' <summary>
        ''' Data about the job returned from the server
        ''' </summary>
        Public ReadOnly Property Data as BackgroundJobData
            Get
                return mData
            End Get
        End Property

        ''' <summary>
        ''' Exception thrown while checking for updates - used with a Status of MonitoringError.
        ''' Errors that occur while processing the job on the server will be held within the
        ''' BackgroundJobData instance returned by the Data property.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Exception as Exception
            Get
                return mException
            End Get
        End Property
    End Class
End NameSpace