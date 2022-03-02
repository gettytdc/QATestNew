
Namespace BackgroundJobs.Monitoring
    ''' <summary>
    ''' Event arguments for BackgroundJobMonitor Done event
    ''' </summary>
    Public Class BackgroundJobDoneEventArgs : Inherits EventArgs

        Private ReadOnly mResult As BackgroundJobResult

        ''' <summary>
        ''' Creates a new instance of BackgroundJobDoneEventArgs
        ''' </summary>
        ''' <param name="result">Result containing the outcome of the job</param>
        Public Sub New(result As BackgroundJobResult)
            mResult = result
        End Sub

        ''' <summary>
        ''' Result containing the outcome of the job
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Result as BackgroundJobResult
            Get
                return mResult
            End Get
        End Property
    
    End Class
End NameSpace