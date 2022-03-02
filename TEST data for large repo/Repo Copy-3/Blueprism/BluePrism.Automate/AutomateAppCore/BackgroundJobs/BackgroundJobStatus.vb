Namespace BackgroundJobs

    ''' <summary>
    ''' Represents the status of a background job running on the server
    ''' </summary>
    Public Enum BackgroundJobStatus
        ''' <summary>
        ''' Job is not recognised or status is not known
        ''' </summary>
        Unknown=0
        ''' <summary>
        ''' The job is currently in progress
        ''' </summary>
        Running=1
        ''' <summary>
        ''' The job has failed
        ''' </summary>
        Failure=2
        ''' <summary>
        ''' The job has finished running successfully
        ''' </summary>
        Success=3
    End Enum
End NameSpace