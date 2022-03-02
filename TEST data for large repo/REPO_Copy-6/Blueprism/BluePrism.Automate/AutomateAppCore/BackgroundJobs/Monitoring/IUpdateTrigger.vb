Namespace BackgroundJobs.Monitoring
    ''' <summary>
    ''' Triggers when BackgroundJobMonitor should check for updates
    ''' </summary>
    Public Interface IUpdateTrigger
    
        ''' <summary>
        ''' The event that fires when an update should be made
        ''' </summary>
        Event Update(sender As Object, e As EventArgs)

    End Interface
End NameSpace