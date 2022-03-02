
Imports BluePrism.Scheduling
Imports BluePrism.AutomateAppCore
Imports BluePrism.Scheduling.Calendar

' It's just too darn long...
Imports UIUtil = AutomateUI.clsUserInterfaceUtils
Imports LocaleTools

''' Project  : Automate
''' Class    : ctlRunMonthly
''' <summary>
''' Allows the user to configure monthly running
''' </summary>
Public Class ctlRunMonthly
    Inherits UserControl
    Implements IIntervalControl

#Region "MissingDateAction Structure"

    ''' <summary>
    ''' Simple structure to use in the missing date combo box which maps the
    ''' 'NthOfMonth' value onto more context-specific strings.
    ''' </summary>
    Private Structure MissingDateAction

        ' The nth of month value represented by this action
        Private policy As NonExistentDatePolicy

        ''' <summary>
        ''' Creates a new missing date action around the given value.
        ''' </summary>
        ''' <param name="val">The NthOfMonth value to represent</param>
        Public Sub New(ByVal val As NonExistentDatePolicy)
            policy = val
        End Sub

        ''' <summary>
        ''' The value held in this structure
        ''' </summary>
        Public ReadOnly Property Value() As NonExistentDatePolicy
            Get
                Return policy
            End Get
        End Property

        ''' <summary>
        ''' A string representation of this missing date action.
        ''' </summary>
        ''' <returns>A string detailing this value.</returns>
        Public Overrides Function ToString() As String

            If policy = NonExistentDatePolicy.FirstSupportedDayInNextMonth Then
                Return My.Resources.TheFirstDayOfTheNextMonth
            ElseIf policy = NonExistentDatePolicy.LastSupportedDayInMonth Then
                Return My.Resources.TheLastDayOfTheMonth
            Else
                Return My.Resources.NotInThatMonth
            End If

        End Function
    End Structure

#End Region

#Region "Constructors"

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        txtMissingDateAction.Visible = False

        ' Set up the combo boxes to hold some useful values rather than
        ' just a load of strings.
        comboNthDayOfWeek.Items.Clear()
        Dim NthDayOfWeekItems As New List(Of EnumToString)(
            {New EnumToString(NthOfMonthExtensions.GetLocalizedFriendlyName(NthOfMonth.First), NthOfMonth.First),
            New EnumToString(NthOfMonthExtensions.GetLocalizedFriendlyName(NthOfMonth.Second), NthOfMonth.Second),
            New EnumToString(NthOfMonthExtensions.GetLocalizedFriendlyName(NthOfMonth.Third), NthOfMonth.Third),
            New EnumToString(NthOfMonthExtensions.GetLocalizedFriendlyName(NthOfMonth.Fourth), NthOfMonth.Fourth),
            New EnumToString(NthOfMonthExtensions.GetLocalizedFriendlyName(NthOfMonth.Fifth), NthOfMonth.Fifth),
            New EnumToString(NthOfMonthExtensions.GetLocalizedFriendlyName(NthOfMonth.Last), NthOfMonth.Last)})
        comboNthDayOfWeek.DisplayMember = "ItemText"
        comboNthDayOfWeek.ValueMember = "ItemValue"
        comboNthDayOfWeek.DataSource = NthDayOfWeekItems

        comboNthWorkingDay.Items.Clear()
        comboNthWorkingDay.DisplayMember = "ItemText"
        comboNthWorkingDay.ValueMember = "ItemValue"
        Dim NthWorkingDayItems As New List(Of EnumToString)(
            {New EnumToString(NthOfMonthExtensions.GetLocalizedFriendlyName(NthOfMonth.First), NthOfMonth.First),
            New EnumToString(NthOfMonthExtensions.GetLocalizedFriendlyName(NthOfMonth.Last), NthOfMonth.Last)})
        comboNthWorkingDay.DataSource = NthWorkingDayItems

        comboDayOfWeek.Items.Clear()
        comboDayOfWeek.DisplayMember = "ItemText"
        comboDayOfWeek.ValueMember = "ItemValue"
        Dim DayOfWeekItems As New List(Of EnumToString)
        For Each day As DayOfWeek In DaySet.FullWeek
            'comboDayOfWeek.Items.Add(System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.DayNames(day))
            DayOfWeekItems.Add(New EnumToString(System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.DayNames(day), day))
        Next
        comboDayOfWeek.DataSource = DayOfWeekItems

        comboMissingDateAction.Items.Clear()
        comboMissingDateAction.Items.AddRange(New Object() {
         New MissingDateAction(NonExistentDatePolicy.Skip),
         New MissingDateAction(NonExistentDatePolicy.FirstSupportedDayInNextMonth),
         New MissingDateAction(NonExistentDatePolicy.LastSupportedDayInMonth)})

    End Sub

#End Region

#Region "IScheduleModifier implementation"

    ''' <summary>
    ''' Event fired when the schedule data has changed.
    ''' </summary>
    ''' <param name="sender">The schedule whose data has changed as a result of a
    '''  change on this class.</param>
    Public Event ScheduleDataChange(ByVal sender As SessionRunnerSchedule) _
     Implements IScheduleModifier.ScheduleDataChange

    ''' <summary>
    ''' Raises a change event for the schedule data, if this control is currently
    ''' assigned to a ctlSchedule control (which has a schedule assigned to it).
    ''' </summary>
    Private Sub RaiseScheduleChangeEvent()
        Dim ctl As ctlSchedule = clsUserInterfaceUtils.GetAncestor(Of ctlSchedule)(Me)
        If ctl IsNot Nothing Then
            RaiseEvent ScheduleDataChange(ctl.Schedule)
        End If
    End Sub

#End Region

#Region "IIntervalControl Implementation"

    ''' <summary>
    ''' The actual control object that this interval control uses to display
    ''' itself. That's me, that is.
    ''' </summary>
    Public ReadOnly Property Control() As Control Implements IIntervalControl.Control
        Get
            Return Me
        End Get
    End Property

    ''' <summary>
    ''' Checks if this control supports the given interval or not.
    ''' </summary>
    ''' <param name="interval">The interval to check if the control supports it
    ''' or not.</param>
    ''' <returns>True if the given interval type is <see cref="IntervalType.Month"/>;
    ''' False otherwise.</returns>
    Public Function SupportsInterval(ByVal interval As IntervalType) As Boolean _
     Implements IIntervalControl.SupportsInterval
        Return (interval = IntervalType.Month)
    End Function

    ''' <summary>
    ''' Gets the TriggetMetaData generated by the fields on the control
    ''' </summary>
    Public Function GetData(ByVal err As ICollection(Of clsValidationError)) As TriggerMetaData _
     Implements IIntervalControl.GetData

        Dim period As Integer = 0
        If Not Integer.TryParse(updnPeriod.Text, period) OrElse period <= 0 Then
            err.Add(New clsValidationError(updnPeriod,
             My.Resources.ctlRunMonthly_ThePeriodEnteredWasInvalid0, updnPeriod.Text))
        End If

        Dim d As New TriggerMetaData()
        d.Interval = IntervalType.Month
        d.Period = period

        If radioNthOfMonth.Checked Then

            d.MissingDatePolicy =
             DirectCast(UIUtil.GetSelectedItem(comboMissingDateAction), MissingDateAction).Value

        ElseIf radioNthWeekdayOfMonth.Checked Then

            Dim obj As Object = UIUtil.GetSelectedItem(comboDayOfWeek)
            If obj IsNot Nothing Then
                d.Days = New DaySet(CType(comboDayOfWeek.SelectedValue, DayOfWeek))
            End If
            obj = UIUtil.GetSelectedItem(comboNthDayOfWeek)
            If obj IsNot Nothing Then
                d.Nth = CType(comboNthDayOfWeek.SelectedValue, NthOfMonth)
            End If

            Dim day As DayOfWeek
            Dim dayIsSet As Boolean = d.Days.HasAtLeastOneDay(day)

            If Not dayIsSet AndAlso d.Nth = NthOfMonth.None Then
                err.Add(New clsValidationError(comboNthDayOfWeek,
                 My.Resources.ctlRunMonthly_YouMustSelectWhichInstanceOfWhichDayOfTheWeekTheScheduleShouldRunOn))
                Return Nothing

            ElseIf d.Nth = NthOfMonth.None Then
                err.Add(New clsValidationError(comboNthDayOfWeek,
                 My.Resources.ctlRunMonthly_YouMustSelectWhich0OfTheMonthTheScheduleShouldRunOn, day))
                Return Nothing

            ElseIf Not dayIsSet Then
                err.Add(New clsValidationError(comboDayOfWeek,
                 My.Resources.ctlRunMonthly_YouMustSelectWhichDayOfTheWeekToRunOn))
                Return Nothing

            End If


        ElseIf radioNthWorkingDay.Checked Then

            Dim cal As ScheduleCalendar = TryCast(UIUtil.GetSelectedItem(comboCalendar), ScheduleCalendar)
            If cal Is Nothing Then
                err.Add(New clsValidationError(comboCalendar,
                 My.Resources.ctlRunMonthly_YouMustSelectACalendarToRunOn))
            Else
                d.CalendarId = cal.Id
            End If

            Dim obj As Object = UIUtil.GetSelectedItem(comboNthWorkingDay)
            If obj IsNot Nothing Then
                d.Nth = CType(comboNthWorkingDay.SelectedValue, NthOfMonth)
            Else
                err.Add(New clsValidationError(comboNthWorkingDay,
                 My.Resources.YouMustSelectWhetherToRunOnTheFirstOrLastWorkingDayOfTheMonth))
            End If

            If err.Count > 0 Then Return Nothing

        End If

        Return d

    End Function

    ''' <summary>
    ''' Populates the control from the given TriggerMetaData
    ''' </summary>
    ''' <param name="data">The TriggerMetaData to populate the control with</param>
    ''' <param name="sched">The schedule also needed to populate some controls</param>
    Public Sub Populate(ByVal data As TriggerMetaData, ByVal sched As SessionRunnerSchedule) _
     Implements IIntervalControl.Populate

        Dim store As IScheduleStore = sched.Owner.Store

        ' Reset all our combos
        comboMissingDateAction.SelectedIndex = 0
        comboNthDayOfWeek.SelectedIndex = 0
        comboNthWorkingDay.SelectedIndex = 0
        comboDayOfWeek.SelectedIndex = 0

        comboCalendar.Items.Clear()
        For Each cal As ScheduleCalendar In store.GetAllCalendars()
            comboCalendar.Items.Add(cal)
        Next

        updnPeriod.Value = data.Period

        ' If days is not empty, then it's the 'nth weekday of month'
        If Not data.Days.IsEmpty Then

            radioNthWeekdayOfMonth.Checked = True
            ' Get the first day and set that into the combo
            If data.Days.Any() Then
                comboDayOfWeek.SelectedValue = data.Days.First()
            End If
            comboNthDayOfWeek.SelectedValue = data.Nth

        ElseIf data.CalendarId <> 0 Then

            radioNthWorkingDay.Checked = True
            comboCalendar.SelectedItem = store.GetCalendar(data.CalendarId)
            comboNthWorkingDay.SelectedValue = data.Nth

        Else

            radioNthOfMonth.Checked = True
            comboMissingDateAction.SelectedItem = New MissingDateAction(data.MissingDatePolicy)

        End If

    End Sub

    ''' <summary>
    ''' Sets the start date needed by some controls
    ''' </summary>
    ''' <param name="d">The Date</param>
    Public Sub SetStartDate(ByVal d As Date) Implements IIntervalControl.SetStartDate
        lblNthOfMonth.Text = LTools.OrdinalWithNumberDayOfMonth(d.Day, Options.Instance.CurrentLocale)

        ' we need to ensure that we know how to deal when the date doesn't exist
        ' in a given month... this can happen for any date of 29+
        panMissingAction.Visible = (d.Day > 28)

    End Sub

    ''' <summary>
    ''' Gets or sets the readonly state of this control
    ''' </summary>
    Public Property [ReadOnly]() As Boolean Implements IIntervalControl.ReadOnly
        Get
            Return mReadonly
        End Get
        Set(ByVal value As Boolean)
            mReadonly = value
            updnPeriod.ReadOnly = value
            updnPeriod.Increment = CInt(IIf(value, 0, 1))

            radioNthOfMonth.AutoCheck = Not value
            radioNthWeekdayOfMonth.AutoCheck = Not value
            radioNthWorkingDay.AutoCheck = Not value

            UIUtil.ShowReadOnlyControl(value, comboNthWorkingDay, txtNthWorkingDay)
            UIUtil.ShowReadOnlyControl(value, comboNthDayOfWeek, txtNthDayOfWeek)
            UIUtil.ShowReadOnlyControl(value,
             comboMissingDateAction, txtMissingDateAction)
            UIUtil.ShowReadOnlyControl(value, comboDayOfWeek, txtDayOfWeek)
            UIUtil.ShowReadOnlyControl(value, comboCalendar, txtCalendar)
        End Set
    End Property
    Private mReadonly As Boolean
#End Region

#Region "UI Event Handlers"

    ''' <summary>
    ''' Handler for data changing in the period text box.
    ''' </summary>
    Private Sub PeriodChanged(ByVal sender As Object, ByVal e As EventArgs) Handles updnPeriod.TextChanged
        RaiseScheduleChangeEvent()
    End Sub


    ''' <summary>
    ''' Handles any of the radio buttons values changing
    ''' </summary>
    Private Sub RadioCheckedChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles radioNthOfMonth.CheckedChanged, radioNthWeekdayOfMonth.CheckedChanged, radioNthWorkingDay.CheckedChanged

        comboMissingDateAction.Enabled = radioNthOfMonth.Checked

        comboNthDayOfWeek.Enabled = radioNthWeekdayOfMonth.Checked
        comboDayOfWeek.Enabled = radioNthWeekdayOfMonth.Checked

        comboNthWorkingDay.Enabled = radioNthWorkingDay.Checked
        comboCalendar.Enabled = radioNthWorkingDay.Checked

        RaiseScheduleChangeEvent()
    End Sub

    ''' <summary>
    ''' Handles any of the selected indexes on the combo boxes changing
    ''' </summary>
    Private Sub ComboBoxIndexChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles comboMissingDateAction.SelectedIndexChanged, comboCalendar.SelectedIndexChanged,
      comboDayOfWeek.SelectedIndexChanged, comboNthDayOfWeek.SelectedIndexChanged
        RaiseScheduleChangeEvent()
    End Sub

#End Region
#Region "Mode dropdownlist class"

    Private Class EnumToString

        Public Property ItemText() As String

        Public Property ItemValue() As Object

        Public Sub New(text As String, value As Object)
            ItemText = text
            ItemValue = value
        End Sub

        Public Overrides Function ToString() As String
            Return ItemText
        End Function
    End Class

    Private Sub lblMissingDateActionPrefix_Click(sender As Object, e As EventArgs) Handles lblMissingDateActionPrefix.Click

    End Sub

#End Region

End Class
