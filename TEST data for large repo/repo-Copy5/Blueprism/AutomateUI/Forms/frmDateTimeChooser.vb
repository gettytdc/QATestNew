Imports System.Globalization

Imports BluePrism.AutomateAppCore
''' Project  : Automate
''' Class    : frmDateTimeChooser
''' 
''' <summary>
''' A form for choosing a date and time.
''' </summary>
Public Class frmDateTimeChooser

    ''' <summary>
    ''' Enumeration of the validation rules for the date/time
    ''' </summary>
    Public Enum ValidationRule
        AllowAll
        BeforeToday
        BeforeNow
        AfterNow
        AfterToday
    End Enum

    ''' <summary>
    ''' The default date/time to return if Cancel is pressed.
    ''' By default this is 'Nothing', ie. the DateTime 'zero' value.
    ''' </summary>
    Private mDefaultDateTime As DateTime

    ''' <summary>
    ''' The validation rule in use within this chooser
    ''' </summary>
    Private mValRule As ValidationRule

    ' Whether to use the third party culture aware calendar as windows form version cannot have locale set
    Private mUseCultureCal As Boolean

    ''' <summary>
    ''' Creates a new date/time chooser with the default date and time 
    ''' (ie. the current date and time), which allows all dates/times
    ''' </summary>
    Public Sub New()
        Me.New(Nothing, ValidationRule.AllowAll)
    End Sub

    ''' <summary>
    ''' Creates a new date/time chooser with the given date/time set,
    ''' which allows all dates to be selected.
    ''' </summary>
    ''' <param name="selectedTime">The date and time to display initially
    ''' on the form.</param>
    Public Sub New(ByVal selectedTime As DateTime)
        Me.New(selectedTime, ValidationRule.AllowAll)
    End Sub

    ''' <summary>
    ''' Creates a new date/time chooser initalised with the specified
    ''' date/time, and using the given validation rule.
    ''' </summary>
    ''' <param name="selectedTime">The time to set initially into the
    ''' form.</param>
    ''' <param name="rule">The rule defining what dates and times should
    ''' be allowed. An error message will be displayed if a date/time is
    ''' chosen which does not meet the specified rule.</param>
    Public Sub New( _
     ByVal selectedTime As DateTime, _
     ByVal rule As ValidationRule)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' If we are using a different locale to the system then use the 3rd party calendar
        mUseCultureCal = Options.Instance.UseCultureCalendar

        ' Reposition the control if we are using culture aware
        If (mUseCultureCal) Then
            cultureDateChooser.Location = dateChooser.Location
            cultureDateChooser.Width = dateChooser.Width
            cultureDateChooser.Culture = New CultureInfo(CultureInfo.CurrentCulture.Name)
            cultureDateChooser.Visible = True
            dateChooser.Visible = False
        Else
            cultureDateChooser.Visible = False
            dateChooser.Visible = True
        End If

        DefaultDateTime = selectedTime
        ' Add any initialization after the InitializeComponent() call.
        If selectedTime = Nothing Then selectedTime = Now()

        ChosenDateTime = selectedTime

        SetValidation(rule)

    End Sub

    ''' <summary>
    ''' The currently set date/time.
    ''' Note that if the chosen date/time is invalid or does not pass the
    ''' currently validation rule, this will return the default date/time
    ''' </summary>
    Public Property ChosenDateTime() As DateTime
        Get
            Return timeChooser.ApplyTime(If(mUseCultureCal, cultureDateChooser.Value, dateChooser.Value))
        End Get
        Set(ByVal value As DateTime)
            dateChooser.Value = value
            cultureDateChooser.Value = value
            timeChooser.SetTime(value)
        End Set
    End Property

    ''' <summary>
    ''' The default date/time to use if the date/time entered was invalid
    ''' or did not pass the validation rule present in this form.
    ''' By default, this will be the 'zero' value of DateTime
    ''' </summary>
    Public Property DefaultDateTime() As DateTime
        Get
            Return mDefaultDateTime
        End Get
        Set(ByVal value As DateTime)
            mDefaultDateTime = value
            timeChooser.DefaultTime = value.TimeOfDay
        End Set
    End Property

    ''' <summary>
    ''' Sets the validation rule that this chooser should use to validate
    ''' the date/time chosen by the user.
    ''' </summary>
    ''' <param name="rule">The rule which should apply to the values entered
    ''' on this form. </param>
    Public Sub SetValidation(ByVal rule As ValidationRule)
        Select Case rule
            Case ValidationRule.AfterToday
                dateChooser.MinDate = DateTime.Today().AddDays(1)
            Case ValidationRule.AfterNow
                dateChooser.MinDate = DateTime.Today()
            Case ValidationRule.BeforeToday
                dateChooser.MaxDate = DateTime.Today().AddDays(-1)
            Case ValidationRule.BeforeNow
                dateChooser.MaxDate = DateTime.Today()
            Case Else
                dateChooser.MinDate = Date.MinValue
                dateChooser.MaxDate = Date.MaxValue
        End Select
        cultureDateChooser.MinDate = dateChooser.MinDate
        cultureDateChooser.MaxDate = dateChooser.MaxDate
        mValRule = rule
    End Sub

    ''' <summary>
    ''' Handles the closing of the form. If the user hasn't clicked OK, ensure that
    ''' the chosen date/time is set to the default date/time.
    ''' </summary>
    Private Sub frmDateTimeChooser_Closing( _
     ByVal sender As Object, ByVal e As FormClosingEventArgs) Handles MyBase.FormClosing

        ' If OK pressed (or Enter pressed), check that the entered value passes
        ' the validation rule. We don't need to worry about AfterToday or BeforeToday
        ' since the dates aren't selectable on the calendar control, but BeforeNow
        ' and AfterNow can be broken by the combination of calendar and time control
        ' Obviously we don't need to check anything if AllowAll is set.
        If Me.DialogResult = DialogResult.OK Then
            Select Case mValRule
                Case ValidationRule.AfterNow
                    Dim dt As DateTime = Me.ChosenDateTime
                    If dt < DateTime.Now() Then
                        MessageBox.Show(My.Resources.TheChosenDateTimeMustBeInTheFuture, My.Resources.xError,
                         MessageBoxButtons.OK, MessageBoxIcon.Error)
                        e.Cancel = True
                    End If
                Case ValidationRule.BeforeNow
                    Dim dt As DateTime = Me.ChosenDateTime
                    If dt > DateTime.Now() Then
                        MessageBox.Show(My.Resources.TheChosenDateTimeMustBeInThePast, My.Resources.xError,
                         MessageBoxButtons.OK, MessageBoxIcon.Error)
                        e.Cancel = True
                    End If
            End Select
        Else ' Otherwise, it's Cancel / Alt-F4 / Escape - set date to default
            ChosenDateTime = DefaultDateTime
        End If

    End Sub

End Class
