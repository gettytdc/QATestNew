Imports System.Timers

Namespace DataMonitor

    ''' <summary>
    ''' DataMonitor class which polls its store regularly in a background thread,
    ''' according to a timer.
    ''' </summary>
    Public Class TimerDataMonitor : Inherits SimpleDataMonitor

        ' The timer which indicates when the store should be polled
        Private WithEvents mTimer As Timer

        ''' <summary>
        ''' Creates a new data monitor using a timer which polls the given store for
        ''' changes to monitored data.
        ''' </summary>
        ''' <param name="dataStore">The store which provides the version data for the
        ''' monitored data.</param>
        Public Sub New(dataStore As IMonitoredDataStore)
            MyBase.New(dataStore)
            mTimer = New Timer() With {
                .Interval = 30 * 1000,
                .AutoReset = False,
                .Enabled = False
            }
        End Sub

        ''' <summary>
        ''' The interval at which the timer should trigger, causing the store's
        ''' monitored data to be polled for changes.
        ''' Note that, to avoid re-entrance, the timer is disabled while the data is
        ''' being polled, meaning that this interval will not be followed precisely
        ''' due to the extra time used to perform the actual polling.
        ''' </summary>
        Public Property Interval As TimeSpan
            Get
                Return TimeSpan.FromMilliseconds(mTimer.Interval)
            End Get
            Set(value As TimeSpan)
                mTimer.Interval = value.TotalMilliseconds
            End Set
        End Property

        ''' <summary>
        ''' Sets whether the timer in this data monitor is enabled or disabled.
        ''' Enabling it will cause the timer to start ticking and thus, the monitor
        ''' to start polling for data changes.
        ''' </summary>
        Public Property Enabled As Boolean
            Get
                Return mTimer.Enabled
            End Get
            Set(value As Boolean)
                mTimer.Enabled = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the object used to marshal event-handler calls that are
        ''' issued when an interval has elapsed. Effectively, this object is used
        ''' to ensure that events are raised on a specific thread. Typically, this
        ''' object might be a Windows Forms control, ensuring that events are raised
        ''' on the UI thread.
        ''' Leaving this at null means that the events will be raised on whichever
        ''' background thread the polling is occurring on. Any work resulting from
        ''' these events which require a particular thread will have to be invoked
        ''' by the listening code.
        ''' </summary>
        Public Property SynchronizingObject As ISynchronizeInvoke
            Get
                Return mTimer.SynchronizingObject
            End Get
            Set(value As ISynchronizeInvoke)
                mTimer.SynchronizingObject = value
            End Set
        End Property

        ''' <summary>
        ''' Handles the timer ticking, indicating that the data store should be
        ''' polled for changes to the monitored data.
        ''' </summary>
        Private Sub HandleElapsed(sender As Object, e As EventArgs) _
         Handles mTimer.Elapsed
            Poll()
            ' The timer is not auto-reset (to prevent re-entrance) so make sure we
            ' cue it up again for the next occurrence.
            mTimer.Enabled = True
        End Sub

        ''' <summary>
        ''' Disposes of this timer data monitor, ensuring that the timer that it
        ''' holds is also disposed of.
        ''' </summary>
        ''' <param name="explicit">True to indicate this is the result of a Dispose()
        ''' call; False to indicate that this is the result of object finalization.
        ''' </param>
        Protected Overrides Sub Dispose(explicit As Boolean)
            If explicit AndAlso mTimer IsNot Nothing Then mTimer.Dispose()
            MyBase.Dispose(explicit)
        End Sub

    End Class

End Namespace