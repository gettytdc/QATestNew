

Namespace BackgroundJobs.Monitoring

    ''' <summary>
    ''' Event arguments for BackgroundJobMonitor Running event
    ''' </summary>
    Public Class BackgroundJobRunningEventArgs : Inherits EventArgs

        Private ReadOnly mData As BackgroundJobData

        ''' <summary>
        ''' Creates a new BackgroundJobRunningEventArgs
        ''' </summary>
        ''' <param name="data">The job data from the server for the running job</param>
        Public Sub New(data As BackgroundJobData)
            mData = data
        End Sub

        ''' <summary>
        ''' The job data from the server for the running job
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Data as BackgroundJobData
            Get
                return mData
            End Get
        End Property
    End Class
End NameSpace