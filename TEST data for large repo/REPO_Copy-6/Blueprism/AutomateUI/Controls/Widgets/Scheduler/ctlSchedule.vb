Imports System.Globalization
Imports AutomateControls
Imports BluePrism.Scheduling
Imports BluePrism.Scheduling.Triggers
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports IntervalType = BluePrism.Scheduling.IntervalType

''' Project  : Automate
''' Class    : ctlSchedule
''' <summary>
''' Allows users to view an modify schedules.
''' </summary>
Public Class ctlSchedule
    Inherits UserControl
    Implements IPermission
    Implements IHelp
    Implements IScheduleModifier

#Region "AutomateUI.Interfaces implementations"
    Public ReadOnly Property RequiredPermissions As ICollection(Of Permission) _
        Implements IPermission.RequiredPermissions
        Get
            Return Permission.None
        End Get
    End Property

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "helpSchedulesUI.htm"
    End Function
#End Region

#Region "IScheduleModifier implementation"
    ''' <summary>
    ''' Event fired when the schedule data has changed.
    ''' </summary>
    ''' <param name="sender">
    ''' The schedule whose data has changed as a result of a change on this class.
    ''' </param>
    Public Event ScheduleDataChange(sender As SessionRunnerSchedule) Implements IScheduleModifier.ScheduleDataChange

    ''' <summary>
    ''' Raises a change event for the schedule data, if this control has a
    ''' schedule assigned to it, and is not currently being populated from it.
    ''' </summary>
    Private Sub RaiseScheduleChangeEvent()
        If mSchedule IsNot Nothing And Not mPopulating Then
            RaiseEvent ScheduleDataChange(mSchedule)
        End If
    End Sub

    ''' <summary>
    ''' Handles the schedule data change event from child controls.
    ''' This just bubbles the event up to any listeners on this control
    ''' </summary>
    ''' <param name="sched">
    ''' The schedule whose data has changed.
    ''' </param>
    Private Sub ScheduleDataChanged(sched As SessionRunnerSchedule)
        RaiseScheduleChangeEvent()
    End Sub
#End Region

#Region "Member Variables"
    ''' <summary>
    ''' This is the minimum date that is allowed in some systems so we treat any
    ''' date before this value as invalid
    ''' </summary>
    Private Shared ReadOnly MinDateTime As Date = New DateTime(1753, 1, 1)

    ''' <summary>
    ''' This is the maximum date that is allowed in the start date.
    ''' </summary>
    Private Shared ReadOnly MaxDateTime As Date = New DateTime(9999, 1, 1)

    ''' <summary>
    ''' Holds a reference to the schedule handled by this control
    ''' </summary>
    Private mSchedule As SessionRunnerSchedule

    ''' <summary>
    ''' Flag to indicate that the control is currently populating from a new
    ''' schedule object - this just inhibits some of the UI event handling.
    ''' </summary>
    Private mPopulating As Boolean

    ''' <summary>
    ''' Collection of repetition detail controls mapped against the corresponding type 
    ''' </summary>
    Private ReadOnly mIntervalControlCache As IDictionary(Of IntervalType, IIntervalControl) =
        New Dictionary(Of IntervalType, IIntervalControl)

    ''' <summary>
    ''' Flag to indicate if this control is read-only or not
    ''' </summary>
    Private mReadOnly As Boolean

    ''' <summary>
    ''' Whether to use the Culture Aware Picker
    ''' </summary>
    Private ReadOnly mUseCultureAwarePicker As Boolean
#End Region

#Region "Constructors"
    ''' <summary>
    ''' Constructs a new ctlSchedule
    ''' </summary>
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        mUseCultureAwarePicker = Options.Instance.UseCultureCalendar

        mPopulating = False

        ' Add any initialization after the InitializeComponent() call.
        radioRunOnce.Tag = IntervalType.Once
        ' The radio button 'Run Hourly' is used for an interval type of minute too,
        ' but the assignment to the tag is just to identify which type of control
        ' to use to configure the specified interval. Since the same control
        ' governs hourly and minutely, this works as expected.
        radioRunHourly.Tag = IntervalType.Hour
        radioRunDaily.Tag = IntervalType.Day
        radioRunWeekly.Tag = IntervalType.Week
        radioRunMonthly.Tag = IntervalType.Month
        radioRunYearly.Tag = IntervalType.Year

        StartDate = Now()

        ' Make sure that there's always an interval control on the repeat-schedule panel
        mRepeatingSchedulePanel.Controls.Add(GenerateIntervalControl(IntervalType.Once).Control)

        ' If we need to use the culture aware picker then replace the standard control with it
        If mUseCultureAwarePicker Then
            cultureDpStartDate.Location = dpStartDate.Location
            cultureDpStartDate.Width = dpStartDate.Width
            cultureDpEndDate.Location = dpEndDate.Location
            cultureDpEndDate.Width = dpEndDate.Width
            cultureDpStartDate.Culture = New CultureInfo(CultureInfo.CurrentCulture.Name)
            cultureDpEndDate.Culture = New CultureInfo(CultureInfo.CurrentCulture.Name)
        End If

        UpdateCultureAwareVisibility()

        [ReadOnly] = Not User.Current.HasPermission("Edit Schedule")
    End Sub
#End Region

#Region "Properties"
    ''' <summary>
    ''' Provides access to the name of the schedule
    ''' </summary>
    Public Property ScheduleName As String
        Get
            Return txtName.Text
        End Get
        Set
            txtName.Text = Value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the schedule held by this control.
    ''' </summary>
    Friend Property Schedule As SessionRunnerSchedule
        Get
            Return mSchedule
        End Get
        Set
            Populate(Value)
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the Description of the schedule
    ''' </summary>
    Public Property Description As String
        Get
            Return txtDescription.Text
        End Get
        Set
            txtDescription.Text = Value
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the StartDateTime of the schedule
    ''' </summary>
    Public Property StartDateTime As Date
        Get
            Return StartDate.Add(StartTime)
        End Get
        Set
            StartDate = Value
            StartTime = Value.TimeOfDay
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the EndDateTime of the schedule
    ''' </summary>
    Public Property EndDateTime As Date
        Get
            Dim dt As Date = EndDate
            If dt = Date.MaxValue Then Return dt Else Return dt.Add(EndTime)
        End Get
        Set
            EndDate = Value
            EndTime = Value.TimeOfDay
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the StartDate of the Schedule
    ''' </summary>
    Public Property StartDate As Date
        Get
            Return If(mUseCultureAwarePicker, cultureDpStartDate.Value.Date, dpStartDate.Value.Date)
        End Get
        Set
            ' We have to make sure that the value is >= 1/1/1753 since MS, blessed be their
            ' wisdom, have hardcoded it as a minimum date all the way through it.
            ' So... if it's prior to 1753, assume invalid and go for today.
            If Value < MinDateTime OrElse Value > MaxDateTime Then
                Dim dt As Date = Today
                dpStartDate.Value = dt.Date
                cultureDpStartDate.Value = dt.Date
            Else
                dpStartDate.Value = Value.Date
                cultureDpStartDate.Value = Value.Date
            End If
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the StartTime of the Schedule
    ''' </summary>
    Public Property StartTime As TimeSpan
        Get
            Return dpStartTime.Value.TimeOfDay
        End Get
        Set
            dpStartTime.Text = Value.ToString()
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the EndDate of the Schedule
    ''' </summary>
    Public Property EndDate As Date
        Get
            ' If it never ends, say so
            If radioRunOnce.Checked OrElse radioEndsNever.Checked Then Return Date.MaxValue
            Return If(mUseCultureAwarePicker, cultureDpEndDate.Value.Date, dpEndDate.Value.Date)
        End Get
        Set
            ' We have to make sure that the value is >= 1/1/1753 since MS, blessed be their
            ' wisdom, have hardcoded it as a minimum date all the way through it.
            ' So... if it's prior to 1753, assume invalid and go for today.
            If Value < MinDateTime Then
                Dim dt As Date = Today
                dpEndDate.Value = dt.Date
                cultureDpEndDate.Value = dt.Date
            Else
                dpEndDate.Value = Value.Date
                cultureDpEndDate.Value = Value.Date
            End If
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the EndTime of the Schedule
    ''' </summary>
    Public Property EndTime As TimeSpan
        Get
            Return dpEndTime.Value.TimeOfDay
        End Get
        Set
            dpEndTime.Text = Value.ToString()
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the read-only state of this control and all its child controls.
    ''' Note that this will currently just enable / disable the interval panel,
    ''' rather than having it set its fields to be read-only. This will need to
    ''' be fixed for when we do implement a read-only mode across the scheduler UI.
    ''' </summary>
    Public Property [ReadOnly] As Boolean
        Get
            Return mReadOnly
        End Get

        Set
            mReadOnly = Value

            txtName.ReadOnly = Value
            txtDescription.ReadOnly = Value

            radioRunDaily.AutoCheck = Not Value
            radioRunHourly.AutoCheck = Not Value
            radioRunMonthly.AutoCheck = Not Value
            radioRunOnce.AutoCheck = Not Value
            radioRunWeekly.AutoCheck = Not Value
            radioRunYearly.AutoCheck = Not Value

            radioEndsNever.AutoCheck = Not Value
            radioEndsOn.AutoCheck = Not Value

            clsUserInterfaceUtils.ShowReadOnlyControl(Value, mInitialTaskCombo, txtReadOnlyInitialTask)
            clsUserInterfaceUtils.ShowReadOnlyControl(Value, dpEndDate, txtEndDate)
            clsUserInterfaceUtils.ShowReadOnlyControl(Value, dpStartDate, txtStartDate)
            clsUserInterfaceUtils.ShowReadOnlyControl(Value, dpStartTime, txtStartTime)
            clsUserInterfaceUtils.ShowReadOnlyControl(Value, cultureDpStartDate, txtStartDate)
            clsUserInterfaceUtils.ShowReadOnlyControl(Value, cultureDpEndDate, txtEndDate)
            clsUserInterfaceUtils.ShowReadOnlyControl(Value, dpEndTime, txtEndTime)

            GetCurrentIntervalControl().ReadOnly = Value

            UpdateCultureAwareVisibility()

        End Set
    End Property

    ''' <summary>
    ''' The radio buttons which represent the trigger intervals on this control
    ''' </summary>
    Private ReadOnly Property IntervalRadios As IEnumerable(Of RadioButton)
        Get
            Return radioRunOnce.Parent.Controls.OfType(Of RadioButton)().ToList()
        End Get
    End Property

    ''' <summary>
    ''' Sets the timing enabled state on this control. Only really disabled if there
    ''' is no interval specified and, as such, the timing cannot be set.
    ''' </summary>
    Private WriteOnly Property TimingEnabled As Boolean
        Set
            For Each ctl As Control In pnlScheduleTiming.Controls
                If ctl IsNot mRunRadioTable AndAlso ctl IsNot cmbTimeZone Then ctl.Enabled = Value
            Next

            mRepeatingSchedulePanel.Enabled = Value

            ' Hide any repeating schedule control - easier than removing it if it's
            ' there which might have unintended consequences
            If mRepeatingSchedulePanel.Controls.Count > 0 Then
                mRepeatingSchedulePanel.Controls(0).Visible = Value
            End If
        End Set
    End Property
#End Region

#Region "Member Methods"
    Private Sub UpdateCultureAwareVisibility()
        dpStartDate.Visible = Not mUseCultureAwarePicker
        dpEndDate.Visible = Not mUseCultureAwarePicker
        cultureDpStartDate.Visible = mUseCultureAwarePicker
        cultureDpEndDate.Visible = mUseCultureAwarePicker
    End Sub

    ''' <summary>
    ''' Gets the current interval control held in this schedule control.
    ''' </summary>
    ''' <returns>
    ''' The interval control currently held in the repeating data of this control, or null if none is set.
    ''' </returns>
    Private Function GetCurrentIntervalControl() As IIntervalControl
        If mRepeatingSchedulePanel.HasChildren() Then
            Return TryCast(mRepeatingSchedulePanel.Controls(0), IIntervalControl)
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' Gets the interval control corresponding to the given interval type
    ''' </summary>
    ''' <param name="type">The type of interval for which the control is required</param>
    ''' <returns>The corresponding control to the given interval type</returns>
    ''' <exception cref="ApplicationException">If the given type is unrecognised</exception>
    Private Function GenerateIntervalControl(type As IntervalType) As IIntervalControl
        If mIntervalControlCache.ContainsKey(type) Then
            Return mIntervalControlCache(type)
        End If

        Dim intervalControl As IIntervalControl
        Select Case type
            Case IntervalType.Once
                intervalControl = New ctlRunOnce()
            Case IntervalType.Hour, IntervalType.Minute
                intervalControl = New ctlRunHourly()
            Case IntervalType.Day
                intervalControl = New ctlRunDaily()
            Case IntervalType.Week
                intervalControl = New ctlRunWeekly()
            Case IntervalType.Month
                intervalControl = New ctlRunMonthly()
            Case IntervalType.Year
                intervalControl = New ctlRunYearly()
            Case Else
                Throw New InvalidOperationException(String.Format(
                    My.Resources.ctlSchedule_CannotGenerateControlForIntervalType0, type))
        End Select

        intervalControl.Control.Dock = DockStyle.Fill
        mIntervalControlCache(type) = intervalControl
        Return intervalControl
    End Function

    ''' <summary>
    ''' Handles any radio button being changed - this will decide which interval
    ''' control is displayed on mRepeatingSchedulePanel
    ''' </summary>
    Private Sub HandleRadioChange(sender As Object, e As EventArgs) _
        Handles radioRunOnce.CheckedChanged, radioRunHourly.CheckedChanged,
                radioRunDaily.CheckedChanged, radioRunWeekly.CheckedChanged,
                radioRunMonthly.CheckedChanged, radioRunYearly.CheckedChanged

        If mPopulating Then Return ' Don't be messing if we're still populating with the schedule

        Dim radio = DirectCast(sender, RadioButton)
        If radio.Tag Is Nothing Then Return ' Not yet initialised... ignore the event.
        If radio.Checked Then
            TimingEnabled = True
            Dim t As New TriggerMetaData With {
                .Interval = DirectCast(radio.Tag, IntervalType),
                .IsUserTrigger = True
            }
            PopulateIntervalControl(mSchedule, t)
            panEnds.Enabled = t.Interval <> IntervalType.Once
        End If

        If mPopulating Then Return

        RaiseScheduleChangeEvent()
    End Sub

    ''' <summary>
    ''' Tests if any of the trigger interval radio buttons are checked. This is
    ''' disabled by default or if a schedule without a trigger is being displayed
    ''' </summary>
    ''' <returns>
    ''' True if any of the radio buttons determining the trigger interval are set as checked; False otherwise
    ''' </returns>
    Private Function IsAnyTriggerIntervalRadioChecked() As Boolean
        Return radioRunOnce.Parent.Controls.OfType(Of RadioButton)().Any(Function(rb) rb.Checked)
    End Function

    ''' <summary>
    ''' Populates the scedule control with the given schedule
    ''' </summary>
    ''' <param name="schedule">The schedule with which to populate the control.</param>
    Private Sub Populate(schedule As SessionRunnerSchedule)
        SuspendLayout()

        Dim bReadonly As Boolean = [ReadOnly]
        [ReadOnly] = False
        mSchedule = schedule
        mPopulating = True

        GetTimeZoneData()

        Try
            Dim trig As ITrigger = schedule.Triggers.UserTrigger

            If trig Is Nothing Then
                ' Set some sensible defaults and then disable the controls
                ' until a radio button is checked

                StartDate = Today
                StartTime = Now.TimeOfDay
                radioEndsNever.Checked = True
                For Each rb As RadioButton In IntervalRadios
                    rb.Checked = False
                Next
                TimingEnabled = False
            Else
                TimingEnabled = True
                StartDate = trig.Start
                StartTime = trig.Start.TimeOfDay

                If trig.End = Date.MaxValue Then
                    radioEndsNever.Checked = True
                Else
                    EndDate = trig.End
                    EndTime = trig.End.TimeOfDay
                End If
            End If

            ScheduleName = schedule.Name
            Description = schedule.Description

            mInitialTaskCombo.Items.Clear()
            mInitialTaskCombo.Items.Add(My.Resources.ctlSchedule_None)
            For Each task As ScheduledTask In schedule
                mInitialTaskCombo.Items.Add(task)
            Next
            If mSchedule.InitialTask IsNot Nothing Then
                mInitialTaskCombo.Text = mSchedule.InitialTask.ToString
            End If

            If trig IsNot Nothing Then
                Dim meta As TriggerMetaData = trig.PrimaryMetaData
                If meta IsNot Nothing Then
                    PopulateTriggerMetaDatControls(meta)
                End If
            End If

            [ReadOnly] = bReadonly

            AddHandler mSchedule.Events.TaskAdded, AddressOf TasksAdded
            AddHandler mSchedule.Events.TaskRemoved, AddressOf TasksRemoved
        Finally
            mPopulating = False
            ResumeLayout()
        End Try
    End Sub

    Private Sub PopulateTriggerMetaDatControls(meta As TriggerMetaData)
        Select Case meta.Interval
            Case IntervalType.Once
                radioRunOnce.Checked = True
            Case IntervalType.Hour, IntervalType.Minute
                radioRunHourly.Checked = True
            Case IntervalType.Day
                radioRunDaily.Checked = True
            Case IntervalType.Week
                radioRunWeekly.Checked = True
            Case IntervalType.Month
                radioRunMonthly.Checked = True
            Case IntervalType.Year
                radioRunYearly.Checked = True
            Case Else
                Throw New InvalidOperationException(String.Format(
                    My.Resources.ctlSchedule_CannotSetRadioForIntervalType0, meta.Interval))
        End Select

        Dim hasTimeZone = meta.TimeZoneId IsNot Nothing
        Dim timeZonesEnabled = hasTimeZone OrElse Schedule.Id = 0
        cmbTimeZone.SelectedItem = If(hasTimeZone, meta.TimeZone, TimeZoneInfo.Local)
        useTimeZoneCheckbox.Checked = timeZonesEnabled
        UpdateDaylightSavings(timeZonesEnabled)

        If meta.UtcOffset IsNot Nothing Then
            PopulateDaylightSavings(False)
        End If

        panEnds.Enabled = meta.Interval <> IntervalType.Once
        PopulateIntervalControl(Schedule, meta)
    End Sub

    ''' <summary>
    ''' Handles removal of tasks from the schedule
    ''' </summary>
    ''' <param name="args"></param>
    ''' <param name="task"></param>
    Private Sub TasksAdded(args As ScheduleEventArgs, task As ScheduledTask)
        mInitialTaskCombo.Items.Add(task)
        If mSchedule.InitialTask IsNot Nothing Then
            mInitialTaskCombo.Text = mSchedule.InitialTask.ToString
        End If
    End Sub

    ''' <summary>
    ''' Handles removal of tasks from the schedule
    ''' </summary>
    ''' <param name="args"></param>
    ''' <param name="task"></param>
    Private Sub TasksRemoved(args As ScheduleEventArgs, task As ScheduledTask)
        mInitialTaskCombo.Items.Remove(task)
        If mSchedule.InitialTask IsNot Nothing Then
            mInitialTaskCombo.Text = mSchedule.InitialTask.ToString
        End If
    End Sub

    ''' <summary>
    ''' Populates the interval control
    ''' </summary>
    ''' <param name="schedule">A reference to the schedule</param>
    ''' <param name="triggerData">The data used to populate the control</param>
    Private Sub PopulateIntervalControl(schedule As SessionRunnerSchedule, triggerData As TriggerMetaData)
        ' Get the current one.. if there's one there.
        Dim intervalControl As IIntervalControl = GetCurrentIntervalControl()

        ' If it doesn't match the interval we want, remove it
        If intervalControl IsNot Nothing AndAlso
            Not intervalControl.SupportsInterval(triggerData.Interval) Then
            mRepeatingSchedulePanel.Controls.RemoveAt(0)
            RemoveHandler intervalControl.ScheduleDataChange, AddressOf ScheduleDataChanged
            intervalControl = Nothing
        End If

        ' If we don't have an interval control at this point, create one
        ' and add it to the repeating schedule panel.
        If intervalControl Is Nothing Then
            intervalControl = GenerateIntervalControl(triggerData.Interval)
            mRepeatingSchedulePanel.Controls.Add(intervalControl.Control)
        End If

        ' Populate it and we're good to go.
        intervalControl.SetStartDate(If(mUseCultureAwarePicker, cultureDpStartDate.Value, dpStartDate.Value))
        intervalControl.Populate(triggerData, schedule)

        AddHandler intervalControl.ScheduleDataChange, AddressOf ScheduleDataChanged
    End Sub

    ''' <summary>
    ''' Commits the changes made to the interval control to the trigger of the schedule.
    ''' </summary>
    ''' <returns>
    ''' True if the changes were committed successfully,
    ''' False if a validation error of some kind occurred and the changes couldn't be committed.
    ''' </returns>
    Public Function CommitIntervalChanges() As Boolean
        Dim factory As New TriggerFactory()
        Dim errors As New List(Of clsValidationError)
        Dim triggerData As TriggerMetaData = Nothing

        ' Do initial validation (of the schedule data) - only get the trigger data if that initial validation is passed
        If StartDateTime >= EndDateTime Then
            errors.Add(New clsValidationError(If(mUseCultureAwarePicker, CType(cultureDpStartDate, Control), CType(dpStartDate, Control)),
            My.Resources.ctlSchedule_TheScheduleMustStartBeforeItExpiresItCurrentlyStartsAt0GAndEndsAt1G, StartDateTime, EndDateTime))
        Else
            triggerData = GetCurrentIntervalControl().GetData(errors)
        End If

        If triggerData Is Nothing Then
            If errors.Count = 0 Then
                MessageBox.Show(
                 My.Resources.ctlSchedule_AnUnspecifiedErrorOccurredWhileSavingTheData, My.Resources.ctlSchedule_Error,
                 MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Else
                MessageBox.Show(errors(0).Message, My.Resources.ctlSchedule_ValidationError,
                 MessageBoxButtons.OK, MessageBoxIcon.Error)
                clsUserInterfaceUtils.SetFocus(errors(0).Control)
            End If
            Return False
        End If

        triggerData.Start = StartDateTime

        If radioEndsNever.Checked Then
            triggerData.End = Date.MaxValue
        Else
            triggerData.End = EndDateTime
        End If

        Dim info = CType(cmbTimeZone.SelectedItem, TimeZoneInfo)
        triggerData.TimeZoneId = If(useTimeZoneCheckbox.Checked, info.Id, Nothing)
        triggerData.UtcOffset = If(useTimeZoneCheckbox.Checked AndAlso Not chkUseDaylightSavings.Checked AndAlso
                                   info.SupportsDaylightSavingTime, info.BaseUtcOffset, CType(Nothing, TimeSpan?))
        triggerData.IsUserTrigger = True
        triggerData.Mode = TriggerMode.Fire

        mSchedule.Triggers.UserTrigger = If(IsAnyTriggerIntervalRadioChecked(), factory.CreateTrigger(triggerData), Nothing)

        Return True
    End Function
#End Region

#Region "UI Event Handlers"

    ''' <summary>
    ''' Checks the repeatuntil checkbox is the end date is changed
    ''' </summary>
    Private Sub dpEndDate_ValueChanged(sender As Object, e As EventArgs) Handles dpEndDate.ValueChanged, cultureDpEndDate.ValueChanged
        radioEndsOn.Checked = True
        If mPopulating Then Return
        RaiseScheduleChangeEvent()
    End Sub

    ''' <summary>
    ''' Sets the interval controls start date when it has changed.
    ''' </summary>
    Private Sub dpStartDate_ValueChanged(sender As Object, e As EventArgs) Handles dpStartDate.ValueChanged, cultureDpStartDate.ValueChanged
        If mRepeatingSchedulePanel.HasChildren() Then
            Dim currentControl As Control = mRepeatingSchedulePanel.Controls(0)
            Dim f = TryCast(currentControl, IIntervalControl)
            If f IsNot Nothing Then
                f.SetStartDate(If(mUseCultureAwarePicker, cultureDpStartDate.Value, dpStartDate.Value))
            End If
        End If

        If mPopulating Then Return
        RaiseScheduleChangeEvent()
    End Sub

    ''' <summary>
    ''' Handles the start time being changed in the GUI.
    ''' This just ensures that any interested parties are aware of a change of the
    ''' schedule's data.
    ''' </summary>
    Private Sub StartTimeChanged(sender As Object, e As EventArgs) Handles dpStartTime.ValueChanged
        If mPopulating Then Return
        RaiseScheduleChangeEvent()
    End Sub

    ''' <summary>
    ''' Saves the schedule when the button is pressed.
    ''' </summary>
    Private Sub buttonSave_Click(sender As Object, e As EventArgs)
        Dim manager = GetAncestor(Of ctlControlRoom)()
        If manager IsNot Nothing Then manager.SaveSchedule(mSchedule)
    End Sub

    ''' <summary>
    ''' Updates the schedule name when the textbox is edited.
    ''' </summary>
    Private Sub NameChanged(sender As Object, e As EventArgs) _
     Handles txtName.TextChanged
        mSchedule.Name = txtName.Text
        RaiseScheduleChangeEvent()
    End Sub

    ''' <summary>
    ''' Updates the schedule description when the textbox is edited.
    ''' </summary>
    Private Sub DescriptionChanged(sender As Object, e As EventArgs) _
     Handles txtDescription.TextChanged
        mSchedule.Description = txtDescription.Text
        RaiseScheduleChangeEvent()
    End Sub

    ''' <summary>
    ''' Updates the schedule's initial task when the combo is changed.
    ''' </summary>
    Private Sub InitialTaskChanged(sender As Object, e As EventArgs) _
     Handles mInitialTaskCombo.SelectedIndexChanged
        mSchedule.InitialTask = TryCast(mInitialTaskCombo.SelectedItem, ScheduledTask)
        RaiseScheduleChangeEvent()
    End Sub

    ''' <summary>
    ''' Handles the end time being changed - set the 'Ends On' radio button so the
    ''' user doesn't have to.
    ''' </summary>
    Private Sub dpEndTime_ValueChanged(sender As Object, e As EventArgs) Handles dpEndTime.ValueChanged
        radioEndsOn.Checked = True
        If mPopulating Then Return
        RaiseScheduleChangeEvent()
    End Sub

    ''' <summary>
    ''' Handle the 'End Date' radio buttons changing state. This just ensures that
    ''' the 'schedule changed event' is fired.
    ''' </summary>
    Private Sub RadioEndsChanged(sender As Object, e As EventArgs) Handles radioEndsNever.CheckedChanged, radioEndsOn.CheckedChanged
        Dim enableEndInputs As Boolean = radioEndsOn.Checked
        dpEndDate.Enabled = enableEndInputs
        cultureDpEndDate.Enabled = enableEndInputs
        dpEndTime.Enabled = enableEndInputs
        txtEndDate.Enabled = enableEndInputs
        txtEndTime.Enabled = enableEndInputs
        If mPopulating Then Return
        RaiseScheduleChangeEvent()
    End Sub


    Private Sub GetTimeZoneData()
        cmbTimeZone.DataSource = TimeZoneInfo.GetSystemTimeZones()
        cmbTimeZone.SelectedItem = TimeZoneInfo.Local
    End Sub

    Private Sub TimezoneChanged(sender As Object, e As EventArgs) Handles cmbTimeZone.SelectedValueChanged
        UpdateDaylightSavings(useTimeZoneCheckbox.Checked)
        StartTimeChanged(sender, e)
    End Sub

    Private Sub useTimeZoneCheckbox_CheckedChanged(sender As Object, e As EventArgs) Handles useTimeZoneCheckbox.CheckedChanged
        cmbTimeZone.Enabled = useTimeZoneCheckbox.Checked
        TimezoneWarningLabel.Visible = Not useTimeZoneCheckbox.Checked
        UpdateDaylightSavings(useTimeZoneCheckbox.Checked)
        RaiseScheduleChangeEvent()
    End Sub

    Private mUpdatingDaylightSavings As Boolean = False

    Private Sub PopulateDaylightSavings(checked As Boolean)
        mUpdatingDaylightSavings = True
        chkUseDaylightSavings.Checked = checked
        mUpdatingDaylightSavings = False
    End Sub

    Private Sub UpdateDaylightSavings(timeZoneEnabled As Boolean)
        Dim enabled = False
        If timeZoneEnabled Then
            Dim timezone = TryCast(cmbTimeZone.SelectedItem, TimeZoneInfo)
            enabled = timezone IsNot Nothing AndAlso timezone.SupportsDaylightSavingTime
        End If
        chkUseDaylightSavings.Enabled = enabled
        PopulateDaylightSavings(enabled)
    End Sub

    Private Sub chkUseDaylightSavings_CheckedChanged(sender As Object, e As EventArgs) Handles chkUseDaylightSavings.CheckedChanged
        If mUpdatingDaylightSavings Then Return
        RaiseScheduleChangeEvent()
    End Sub
#End Region

End Class
