Namespace BackgroundJobs.Monitoring
    ''' <summary>
    ''' The outcome of waiting for completion of a background job
    ''' </summary>
    Public Enum JobMonitoringStatus
        ''' <summary>
        ''' No information about the job was available from the server
        ''' </summary>
        Missing
        ''' <summary>
        ''' The job completed successfully
        ''' </summary>
        Success
        ''' <summary>
        ''' The job failed 
        ''' </summary>
        Failure
        ''' <summary>
        ''' The job still appears to be running but no updated data about progress
        ''' has been received within the specified timeout interval
        ''' </summary>
        Timeout
        ''' <summary>
        ''' An error occured while checking for updates
        ''' </summary>
        MonitoringError
    End Enum
End NameSpace