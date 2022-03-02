Imports BluePrism.Core.Utility

Namespace BackgroundJobs.Monitoring

    ''' <summary>
    ''' Triggers when BackgroundJobMonitor should check for updates using a simple
    ''' polling interval together with updates triggered via notifier where callbacks
    ''' are supported.
    ''' </summary>
    Public Class UpdateTrigger : Implements IUpdateTrigger, IDisposable

        Private ReadOnly mNotifier As BackgroundJobNotifier
        Private ReadOnly mTimer As SystemTimer

        ''' <summary>
        ''' Creates a new UpdateTrigger
        ''' </summary>
        ''' <param name="interval">Interval at which to poll for updates</param>
        ''' <param name="notifier">Signals when updates are available for the job
        ''' (when running in-process or server callbacks are supported)</param>
        Sub New(interval As TimeSpan, notifier As BackgroundJobNotifier)
            mNotifier = notifier
            mTimer = New SystemTimer(interval)
            AddHandler mTimer.Elapsed, AddressOf Timer_Elapsed
            AddHandler mNotifier.Updated, AddressOf Notifier_Updated
        End Sub

        ''' <summary>
        ''' The event that fires when an update should be made
        ''' </summary>
        Public Event Update(sender As Object, e As EventArgs) Implements IUpdateTrigger.Update

        ''' <summary>
        ''' Starts triggering updates on the interval specified
        ''' </summary>
        Public Sub Start
            mTimer.Start
        End Sub

        Private Sub Timer_Elapsed(sender As Object, e As EventArgs)
            RaiseEvent Update(Me, EventArgs.Empty)
        End Sub

        Private Sub Notifier_Updated(sender As Object, e As EventArgs)
            RaiseEvent Update(Me, EventArgs.Empty)
        End Sub

        ''' <summary>
        ''' Frees resources used by this UpdateTrigger
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            RemoveHandler mTimer.Elapsed, AddressOf Timer_Elapsed
            RemoveHandler mNotifier.Updated, AddressOf Notifier_Updated
            mTimer.Dispose()
        End Sub
    End Class
End NameSpace