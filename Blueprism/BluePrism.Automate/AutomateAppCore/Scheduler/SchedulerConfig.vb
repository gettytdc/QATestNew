
''' <summary>
''' Static class to encapsulate the scheduler config and keep the defaults
''' to be used in a single place.
''' </summary>
Public Class SchedulerConfig

#Region " Constants "

    ''' <summary>
    ''' The maximum number of seconds to check back to mop up any missed
    ''' schedules - equivalent to 1 hour
    ''' </summary>
    Public Const MaximumCheckSeconds As Integer = 60 * 60

    ''' <summary>
    ''' The maximum number of retries of an offline resource
    ''' </summary>
    Public Const MaximumRetryTimes As Integer = 100

    ''' <summary>
    ''' The maximum period (in seconds) to wait between retries - equivalent to
    ''' 5 minutes.
    ''' </summary>
    Public Const MaximumRetryPeriod As Integer = 5 * 60

#End Region

    ''' <summary>
    ''' No instantiation - effectively a 'static class' in C# parlance
    ''' </summary>
    Private Sub New()
    End Sub

    ''' <summary>
    ''' Whether the scheduler is active on the current connection or not.
    ''' </summary>
    Public Shared ReadOnly Property Active() As Boolean
        Get
            Return gSv.GetPref(PreferenceNames.Scheduler.Active, True)
        End Get
    End Property

    ''' <summary>
    ''' The number of seconds to check back when the scheduler is started
    ''' </summary>
    Public Shared ReadOnly Property CheckSeconds() As Integer
        Get
            Dim secs As Integer = gSv.GetPref(PreferenceNames.Scheduler.CheckSeconds, -1)
            If secs < 0 OrElse secs > MaximumCheckSeconds Then Return 15 * 60 Else Return secs
        End Get
    End Property

    ''' <summary>
    ''' The number of times to retry an offline resource
    ''' </summary>
    Public Shared ReadOnly Property RetryTimes() As Integer
        Get
            Dim times As Integer = gSv.GetPref(PreferenceNames.Scheduler.RetryTimes, -1)
            If times < 0 OrElse times > MaximumRetryTimes Then Return 10 Else Return times
        End Get
    End Property

    ''' <summary>
    ''' The number of seconds between each retry of an offline resource.
    ''' </summary>
    Public Shared ReadOnly Property RetryPeriod() As Integer
        Get
            Dim period As Integer = gSv.GetPref(PreferenceNames.Scheduler.RetryPeriod, -1)
            If period < 1 OrElse period >= MaximumRetryPeriod Then Return 5 Else Return period
        End Get
    End Property

End Class
