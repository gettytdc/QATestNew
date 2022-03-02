Imports AutomateControls.Diary

''' Project  : Automate
''' Class    : ctlSchedulerOverview
''' 
''' <summary>
''' Provides an overview of all the currently configure schedules
''' </summary>
Public Class ctlSchedulerOverview

    ''' <summary>
    ''' Creates a new scheduler overview control
    ''' </summary>
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    ''' <summary>
    ''' Gets the diary view for this overview control.
    ''' </summary>
    Friend ReadOnly Property DiaryView() As WeekView
        Get
            Return mWeekView
        End Get
    End Property

End Class
