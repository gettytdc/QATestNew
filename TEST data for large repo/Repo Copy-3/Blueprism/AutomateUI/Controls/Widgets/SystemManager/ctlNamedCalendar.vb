Imports System.Globalization

Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Scheduling.Calendar
Imports BluePrism.Scheduling
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore
Imports LocaleTools
Imports AutomateControls
Imports AutomateControls.Forms

''' Project  : Automate
''' Class    : ctlNamedCalendar
''' <summary>
''' Control to create, amend and delete named calendars within Blue Prism.
''' </summary>
Public Class ctlNamedCalendar

    ''' <summary>
    ''' The maximum number of characters allowed in the calendar name
    ''' </summary>
    Private Const MAX_CHARS_IN_NAME As Integer = 128

    ''' <summary>
    ''' The label to display to indicate that no public holiday group in the
    ''' public holiday group combo box.
    ''' </summary>
    Private NO_PH_GROUP_SELECTED_LABEL As String = "<None>"

    ''' <summary>
    ''' Whether to use the culture aware calendar picker
    ''' </summary>
    Private mUseCultureAwarePicker As Boolean
    Private mSavePromptTriggered As Boolean

#Region " Dummy classes used within the designer "

    ''' <summary>
    ''' Dummy Store - only really used for the forms designed, so it doesn't
    ''' break when attempting to load calendars from the database.
    ''' </summary>
    Private Class DummyStore
        Implements IScheduleCalendarStore

        Private mDummySchema As New PublicHolidaySchema(New DataTable())

        Public Sub SaveCalendar(ByVal cal As ScheduleCalendar) Implements IScheduleCalendarStore.SaveCalendar
        End Sub

        Public Event CalendarsUpdated() Implements IScheduleCalendarStore.CalendarsUpdated

        Public Sub DeleteCalendar(ByVal cal As ScheduleCalendar) Implements IScheduleCalendarStore.DeleteCalendar
        End Sub

        Public Function GetAllCalendars() As System.Collections.Generic.ICollection(Of ScheduleCalendar) Implements IScheduleCalendarStore.GetAllCalendars
            Return New ScheduleCalendar() {}
        End Function

        Public Function GetCalendar(ByVal id As Integer) As ScheduleCalendar Implements IScheduleCalendarStore.GetCalendar
            Return Nothing
        End Function

        Public Function GetCalendar(ByVal name As String) As ScheduleCalendar Implements IScheduleCalendarStore.GetCalendar
            Return Nothing
        End Function

        Public Function GetSchema() As PublicHolidaySchema Implements IScheduleCalendarStore.GetSchema
            Return mDummySchema
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
        End Sub

    End Class

#End Region

#Region " Member Variables "

    ' The store that is used to get and set the calendars
    Private mStore As IScheduleCalendarStore

    ' The checkboxes used to capture the working week.
    Private mWorkingWeekCheckboxes As ICollection(Of CheckBox)

    ' Flag indicating if the user should be prompted to save changes when the
    ' calendars combo box is changed.
    Private mPromptUserToSaveChanges As Boolean

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new NamedCalendar control backed by a dummy store - ie. a store
    ''' with no data (and no capacity for saving data.
    ''' </summary>
    ''' <remarks>This is only ever used by the designer, and should not be called
    ''' by any production code.</remarks>
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        mUseCultureAwarePicker = Options.Instance.UseCultureCalendar
        ' If we need to use the culture aware picker then replace the standard control with it
        If (mUseCultureAwarePicker) Then
            mCultureDatePicker.Location = mOtherHolidayDatePicker.Location
            mCultureDatePicker.Visible = True
            mCultureDatePicker.Culture = New CultureInfo(CultureInfo.CurrentCulture.Name)
            mOtherHolidayDatePicker.Visible = False
        End If




        ' Set the tooltips for the save / delete buttons.
        Dim tt As New ToolTip()
        tt.SetToolTip(mNewCalendarButton, My.Resources.ctlNamedCalendar_CreateANewCalendar)
        tt.SetToolTip(mSaveCalendarButton, My.Resources.ctlNamedCalendar_SaveTheCurrentCalendar)
        tt.SetToolTip(mDeleteCalendarButton, My.Resources.ctlNamedCalendar_DeleteTheCurrentCalendar)
        tt.SetToolTip(btnReferences, My.Resources.ctlNamedCalendar_FindReferencesToTheCurrentCalendar)
        NO_PH_GROUP_SELECTED_LABEL = My.Resources.ctlNamedCalendar_None

        ' Set up the working week checkboxes to update the selected calendar
        mWorkingWeekCheckboxes = New clsSet(Of CheckBox)

        InitDayCheckBox(mMondayCheckbox, DayOfWeek.Monday)
        InitDayCheckBox(mTuesdayCheckbox, DayOfWeek.Tuesday)
        InitDayCheckBox(mWednesdayCheckbox, DayOfWeek.Wednesday)
        InitDayCheckBox(mThursdayCheckbox, DayOfWeek.Thursday)
        InitDayCheckBox(mFridayCheckbox, DayOfWeek.Friday)
        InitDayCheckBox(mSaturdayCheckbox, DayOfWeek.Saturday)
        InitDayCheckBox(mSundayCheckbox, DayOfWeek.Sunday)
    End Sub

    ' Convert string collection to KeyValueStringComboBoxItem with localised strings
    Public Function ConvertStringsToLocalisedComboBox(list As ICollection(Of String)) As ICollection(Of KeyValueStringComboBoxItem)
        Return (From item In list Select New KeyValueStringComboBoxItem(item, LTools.GetC(item, "holiday"))).ToList()
    End Function


    Public Sub Populate()
        ' Add all the other calendars described in the store.
        For Each cal As ScheduleCalendar In Store.GetAllCalendars()
            mCalendarCombo.Items.Add(cal)
        Next

        Dim schema As PublicHolidaySchema = Store.GetSchema()
        ' Add a blank entry to allow choosing no public holiday group
        mPublicHolidayCombo.Items.Add(New KeyValueStringComboBoxItem(NO_PH_GROUP_SELECTED_LABEL))
        For Each group As KeyValueStringComboBoxItem In ConvertStringsToLocalisedComboBox(schema.GetGroups())
            mPublicHolidayCombo.Items.Add(group)
        Next

        ' Make sure that any non-allowed controls are disabled if there are no calendars.
        CheckCalendarCount()

        ' Make sure that the first item in the list is selected (if there's any there)
        If mCalendarCombo.Items.Count > 0 Then mCalendarCombo.SelectedIndex = 0

        mPromptUserToSaveChanges = True

    End Sub

    Public Property Store() As IScheduleCalendarStore
        Get
            Return mStore
        End Get
        Set(ByVal value As IScheduleCalendarStore)
            mStore = value
        End Set
    End Property

#End Region

#Region " Calendars "

    ''' <summary>
    ''' Gets the currently selected schedule calendar in the calendar combo box.
    ''' If the 'blank' schedule at the top of the list is selected, this will
    ''' return a schedule object which is not on the database.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedCalendar() As ScheduleCalendar
        Get
            If mCalendarCombo.Items.Count = 0 Then Return Nothing
            ' If 'nothing' is selected, use the 'blank' calendar at the first index
            ' in the combo box.
            Dim ind As Integer = mCalendarCombo.SelectedIndex
            If ind = -1 Then ind = 0
            Return TryCast(mCalendarCombo.Items(ind), ScheduleCalendar)
        End Get
        Set(ByVal value As ScheduleCalendar)
            mCalendarCombo.SelectedItem = value
        End Set
    End Property

    ''' <summary>
    ''' A calendar representing the current value set in the UI. Note that this
    ''' calendar is not saved or created into the calendar store - it is only a
    ''' standalone representation of the UI state in a calendar form. As such, when
    ''' the value is set, the object is not retained - the UI elements are updated
    ''' with its value, but nothing else.
    ''' </summary>
    ''' <remarks>Also, when setting, the ID, name and description of the calendar are
    ''' ignored since they cannot be modified in the UI. When getting the current
    ''' value, they are returned from the currently selected calendar.</remarks>
    Private Property CurrentValue() As ScheduleCalendar
        Get
            Dim sel As ScheduleCalendar = SelectedCalendar
            If sel Is Nothing Then Return Nothing
            Dim cal As New ScheduleCalendar(mStore.GetSchema())
            cal.Name = sel.Name
            cal.Description = sel.Description
            cal.Id = sel.Id
            cal.WorkingWeek = Me.WorkingWeek
            cal.NonWorkingDays = Me.NonWorkingDays
            cal.PublicHolidayGroup = Me.SelectedPublicHolidayGroup
            cal.PublicHolidayOverrides = Me.PublicHolidayOverrides
            Return cal
        End Get
        Set(ByVal value As ScheduleCalendar)
            If value Is Nothing Then Return
            Me.WorkingWeek = value.WorkingWeek
            Me.NonWorkingDays = value.NonWorkingDays
            Me.SelectedPublicHolidayGroup = value.PublicHolidayGroup
            Me.PublicHolidayOverrides = value.PublicHolidayOverrides
        End Set
    End Property

    ''' <summary>
    ''' Gets a flag indicating if the current calendar has changed since it was
    ''' selected / last saved.
    ''' </summary>
    Public ReadOnly Property Changed() As Boolean
        Get
            Dim cal As ScheduleCalendar = SelectedCalendar
            If cal Is Nothing Then Return False
            ' It's changed if it has never been saved (ie. Id = 0) or if the current
            ' value in the controls doesn't match the value in the model.
            Return cal.Id = 0 OrElse Not cal.Equals(CurrentValue)
        End Get
    End Property

    ''' <summary>
    ''' Handles the changing of the selected calendar.
    ''' </summary>
    Private Sub HandleSelectedCalendarChanging(
     ByVal sender As Object, ByVal e As CancelEventArgs) _
     Handles mCalendarCombo.SelectedIndexChanging

        ' Ignore any events which occur before we have a handle created - it can't
        ' have been from user input and nothing could possibly have changed.
        If Not mPromptUserToSaveChanges Then Return

        Select Case AskUserToSaveChanges()
            Case DialogResult.Yes : SaveCurrentCalendar()
            Case DialogResult.Cancel : e.Cancel = True
                ' "Case Else" : do nothing - no need to save, no need to cancel
                '               Either there was no need to save or user said 'No'
        End Select

    End Sub

    ''' <summary>
    ''' Checks to see if any changes have been made to the current calendar value,
    ''' and prompts the user to save them if they have.
    ''' </summary>
    ''' <returns>The response from the user regarding whether to save changes.
    ''' One of:- <list>
    ''' <item><see cref="DialogResult.Yes"/> if the data should be saved;</item>
    ''' <item><see cref="DialogResult.No"/> if the data should not be saved;</item>
    ''' <item><see cref="DialogResult.Cancel"/> if the moving away from the data
    ''' should be cancelled.</item>
    ''' </list></returns>
    Private Function AskUserToSaveChanges() As DialogResult
        ' Default to 'No' - ie. don't save changes if there are none to save
        If Not Changed Then Return DialogResult.No

        Dim cal As ScheduleCalendar = SelectedCalendar

        Dim popupForm = New YesNoCancelPopupForm(My.Resources.PopupForm_UnsavedChanges,
                                                 My.Resources.ctlControlRoom_YouHaveNotYetSavedYourChangesInTheSchedulerWouldYouLikeToSaveYourChangesBeforeL,
                                                 String.Empty)

        popupForm.ShowInTaskbar = False
        Return popupForm.ShowDialog()


    End Function

    ''' <summary>
    ''' Handler for a different calendar being selected.
    ''' </summary>
    ''' <param name="sender">The calendar combo box</param>
    ''' <param name="e">The event args</param>
    Private Sub mCalendarCombo_SelectedIndexChanged(
     ByVal sender As Object, ByVal e As EventArgs) _
     Handles mCalendarCombo.SelectedIndexChanged
        ' Now set up all the new data
        CurrentValue = SelectedCalendar
    End Sub

    ''' <summary>
    ''' Gets the calendar names for all the calendars held in this control
    ''' </summary>
    ''' <returns>The names of all the calendars in this control, including
    ''' the first 'blank' calendar.</returns>
    Private Function GetCalendarNames() As ICollection(Of String)

        Dim strList As New List(Of String)
        For Each sched As ScheduleCalendar In mCalendarCombo.Items
            strList.Add(sched.Name)
        Next
        Return strList

    End Function

    ''' <summary>
    ''' Gets a prompt for the user to enter a calendar name, incorporating the
    ''' specified error message and its arguments.
    ''' </summary>
    ''' <param name="errorMessage">The error message to display, null to
    ''' indicate that no error message should be incorporated.</param>
    ''' <param name="errorArgs">The arguments to use when formatting the error
    ''' message.</param>
    ''' <returns>The prompt for the user with the given error message and args
    ''' included.</returns>
    Private Function PromptWithError(
     ByVal errorMessage As String, ByVal ParamArray errorArgs() As Object) As String

        Dim prompt As String = My.Resources.ctlNamedCalendar_PleaseChooseANameForYourCalendar
        If String.IsNullOrEmpty(errorMessage) Then Return prompt ' No error messages round 'yur.

        Return New StringBuilder(My.Resources.ctlNamedCalendar_Error).AppendFormat(errorMessage, errorArgs).Append(vbCrLf) _
         .Append(prompt).ToString()

    End Function

    ''' <summary>
    ''' Provides the user with a combo box to enable them to choose a name
    ''' for the calendar. Does some basic validation to ensure that a name
    ''' is entered and it is not too long.
    ''' </summary>
    ''' <param name="name">The default name to enter into the text field which
    ''' accepts the new calendar name.</param>
    ''' <returns>The name chosen by the user or null if the user cancelled
    ''' </returns>
    Private Function ChooseName(ByVal name As String) As String

        Dim prompt As String = PromptWithError(Nothing)

        ' Loop round until the user clicks 'Cancel' or enters a valid name
        While frmInputBox.HasEnteredText(prompt, My.Resources.ctlNamedCalendar_NewCalendar, name, MAX_CHARS_IN_NAME)

            name = name.Trim()

            If name.Length = 0 Then
                prompt = PromptWithError(My.Resources.ctlNamedCalendar_YouMustProvideANameForTheCalendar)
                ' Retry - ie. reshow the combo box until correct or cancelled.
            ElseIf GetCalendarNames().Contains(name) Then
                prompt = PromptWithError(My.Resources.ctlNamedCalendar_TheName0IsAlreadyTaken, name)
            Else
                ' The name was valid - return it.
                Return name
            End If

        End While

        ' Cancelled - return null to indicate no name
        Return Nothing

    End Function

    ''' <summary>
    ''' Checks the calendar count within this control and ensures that any affected
    ''' child controls are enabled or disabled as appropriate.
    ''' </summary>
    Private Sub CheckCalendarCount()
        If mCalendarCombo.Items.Count = 0 Then

            mCalendarCombo.Enabled = False
            mSaveCalendarButton.Enabled = False
            mDeleteCalendarButton.Enabled = False
            mPublicHolidayGroup.Enabled = False
            mOtherHolidaysGroup.Enabled = False
            mWorkingWeekGroup.Enabled = False

        ElseIf Not mSaveCalendarButton.Enabled Then

            mCalendarCombo.Enabled = True
            mSaveCalendarButton.Enabled = True
            mDeleteCalendarButton.Enabled = True
            mPublicHolidayGroup.Enabled = True
            mOtherHolidaysGroup.Enabled = True
            mWorkingWeekGroup.Enabled = True

        End If
    End Sub

    ''' <summary>
    ''' Event handler for the 'New Calendar' button being pressed.
    ''' This prompts for a name and creates a new calendar, adding it to the
    ''' list (at the bottom) and setting a load of default values.
    ''' </summary>
    ''' <param name="sender">The 'New Calendar' button</param>
    ''' <param name="e">Blankness, as far as the compiler can see.</param>
    Private Sub HandleNewClicked(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mNewCalendarButton.Click

        ' Ensure that any changes have been saved before creating a new calendar
        ' over the top of the current one.
        If AskUserToSaveChanges() = DialogResult.Cancel Then Return

        Dim name As String = ChooseName(Nothing)
        If name IsNot Nothing Then
            Dim cal As New ScheduleCalendar(mStore.GetSchema())
            cal.Name = name
            cal.WorkingWeek = DaySet.FiveDayWeek
            ' We've already checked to see if the user wants to save changes above.
            ' If they said 'No' "Changed" will still be true and they would get
            ' asked again - ensure that this does not occur.
            mPromptUserToSaveChanges = False
            mCalendarCombo.SelectedIndex = mCalendarCombo.Items.Add(cal)
            mPromptUserToSaveChanges = True

            ' We've created a calendar, check to see if anything needs re-enabling
            CheckCalendarCount()
        End If

    End Sub

    ''' <summary>
    ''' Event Handler for the 'Save Calendar' button being pressed.
    ''' This prompts for a name for the calendar and then either creates or
    ''' updates the calendar in the store, depending on whether the name matched
    ''' a calendar held within this control.
    ''' If the calendar already existed, the calendar held in the calendar
    ''' combo box is updated with the new values.
    ''' If the calendar is new, it is appended to the end of the calendar
    ''' combo box.
    ''' Either way, the newly saved calendar is selected in the control after
    ''' the calendar has been saved.
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="e">The arguments</param>
    Private Sub HandleSaveClicked(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mSaveCalendarButton.Click
        SaveCurrentCalendar()
    End Sub


    ''' <summary>
    ''' Updates the currently selected calendar with the current values and
    ''' commits those changes to the database.
    ''' </summary>
    Private Sub SaveCurrentCalendar()

        ' The calendar which is to be saved.
        Dim cal As ScheduleCalendar = SelectedCalendar

        Try

            cal.WorkingWeek = Me.WorkingWeek
            cal.NonWorkingDays = Me.NonWorkingDays
            cal.PublicHolidayGroup = Me.SelectedPublicHolidayGroup
            cal.PublicHolidayOverrides = Me.PublicHolidayOverrides

            mStore.SaveCalendar(cal)

        Catch ex As Exception

            UserMessage.Show(String.Format(My.Resources.ctlNamedCalendar_ErrorWhileSavingCalendar0, cal.ToString), ex)

        End Try

    End Sub

    ''' <summary>
    ''' Handles the 'Delete Calendar' button being clicked.
    ''' This checks if the currently selected calendar is in the store, before
    ''' attempting to delete it. If deletion is successful, the calendar is 
    ''' removed from the calendar combo box and the 'blank' calendar is selected
    ''' in the control
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The args</param>
    Private Sub HandleDeleteClicked(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mDeleteCalendarButton.Click

        Dim cal As ScheduleCalendar = SelectedCalendar

        Dim res As DialogResult = MessageBox.Show(Me,
         String.Format(My.Resources.ctlNamedCalendar_AreYouSureYouWantToDeleteThe0Calendar, cal.ToString),
         My.Resources.ctlNamedCalendar_DeleteCalendar, MessageBoxButtons.OKCancel, MessageBoxIcon.Question)

        If res <> DialogResult.OK Then Return

        Try
            ' Only delete from the database if it's on the database
            If cal.Id <> 0 Then mStore.DeleteCalendar(cal)

        Catch ioe As InvalidOperationException
            ' InvalidOperation = Calendar being used by a schedule...
            MessageBox.Show(ioe.Message, My.Resources.ctlNamedCalendar_CalendarInUse,
             MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return

        Catch ex As Exception
            UserMessage.Show(My.Resources.ctlNamedCalendar_ErrorOccurredWhileDeletingTheCalendar, ex)
            Return

        End Try

        Dim index As Integer = mCalendarCombo.SelectedIndex
        mCalendarCombo.BeginUpdate()
        mCalendarCombo.Items.Remove(cal)

        If index > 0 Then
            mCalendarCombo.SelectedIndex = index - 1
        ElseIf mCalendarCombo.Items.Count > 0 Then
            mCalendarCombo.SelectedIndex = 0
        End If
        mCalendarCombo.EndUpdate()

        ' We've deleted a calendar, check to see if there any left...
        CheckCalendarCount()
    End Sub

    Private Sub HandleReferencesClicked(sender As Object, e As EventArgs) _
     Handles btnReferences.Click
        CType(Parent, IChild).ParentAppForm.FindReferences(
            New clsProcessCalendarDependency(SelectedCalendar.Name))
    End Sub


#End Region

#Region " Public Holidays "

    ''' <summary>
    ''' Gets/Sets the currently selected public holiday group, or an empty string
    ''' if no public holiday group is selected
    ''' </summary>
    <Browsable(False), _
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Property SelectedPublicHolidayGroup() As String
        Get

            If Not TypeOf mPublicHolidayCombo.SelectedItem Is KeyValueStringComboBoxItem Then Return ""
            Dim str As String = DirectCast(mPublicHolidayCombo.SelectedItem, KeyValueStringComboBoxItem).Value
            If str = NO_PH_GROUP_SELECTED_LABEL Then Return ""
            Return str
        End Get
        Set(ByVal value As String)
            If value = "" Then value = NO_PH_GROUP_SELECTED_LABEL
            mPublicHolidayCombo.SelectedItem = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the currently checked collection of public holiday overrides.
    ''' </summary>
    <Browsable(False), _
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Property PublicHolidayOverrides() As IBPSet(Of PublicHoliday)
        Get
            Dim hols As New clsSet(Of PublicHoliday)
            For i As Integer = 0 To mPublicHolidayListBox.Items.Count - 1
                If Not mPublicHolidayListBox.GetItemChecked(i) Then
                    hols.Add(DirectCast(mPublicHolidayListBox.Items(i), PublicHoliday))
                End If
            Next
            Return hols
        End Get

        Set(ByVal value As IBPSet(Of PublicHoliday))

            For i As Integer = 0 To mPublicHolidayListBox.Items.Count - 1
                mPublicHolidayListBox.SetItemChecked(i, _
                 Not value.Contains(DirectCast(mPublicHolidayListBox.Items(i), PublicHoliday)))
            Next

        End Set
    End Property

    ''' <summary>
    ''' Sets each of the checked list box items in the given object to be either
    ''' checked or unchecked depending on the <paramref name="check"/>
    ''' parameter.
    ''' </summary>
    ''' <param name="lister">The CheckedListBox object to set the checked values
    ''' on. </param>
    ''' <param name="check">True to check all the items in the list box; False
    ''' to uncheck all the items.</param>
    Private Sub SetCheckedOnAll(ByVal lister As CheckedListBox, ByVal check As Boolean)
        For i As Integer = 0 To lister.Items.Count - 1
            lister.SetItemChecked(i, check)
        Next
    End Sub

    ''' <summary>
    ''' Handler for the public holiday group selection being changed.
    ''' This replaces the items in the public holiday overrides list with
    ''' the public holidays corresponding to the newly selected group. If
    ''' the blank group (ie. no group) is selected, the overrides list box
    ''' will be emptied.
    ''' </summary>
    ''' <param name="sender">The public holiday combo box.</param>
    ''' <param name="e">The event args</param>
    Private Sub mPublicHolidayCombo_SelectedIndexChanged(
     ByVal sender As Object, ByVal e As EventArgs) Handles mPublicHolidayCombo.SelectedIndexChanged

        Dim schema As PublicHolidaySchema = mStore.GetSchema()
        Dim group As String = Me.SelectedPublicHolidayGroup
        If group = "" Then
            lblIncluding.Enabled = False
            mPublicHolidayListBox.Items.Clear()
        Else
            lblIncluding.Enabled = True
            mPublicHolidayListBox.BeginUpdate()
            mPublicHolidayListBox.Items.Clear()
            For Each holiday As PublicHoliday In schema.GetHolidays(group)
                holiday.LocalName = If(Me.IncludeDatesCheckbox.Checked, $"{LTools.GetC(holiday.Name, "holiday")} ({holiday.GetNextOccurrence().ToShortDateString()})", LTools.GetC(holiday.Name, "holiday"))
                mPublicHolidayListBox.Items.Add(holiday)
            Next
            SetCheckedOnAll(mPublicHolidayListBox, True)
            mPublicHolidayListBox.EndUpdate()
        End If

    End Sub

    Private Sub IncludeDatesCheckbox_CheckedChanged(
     ByVal sender As Object, ByVal e As EventArgs) Handles IncludeDatesCheckbox.CheckedChanged
        Me.mPublicHolidayCombo_SelectedIndexChanged(sender, e)
    End Sub

#End Region

#Region " Other Holidays / Non Working Days "

    ''' <summary>
    ''' Gets the collection of 'non working days' / 'other holidays'
    ''' which are currently set in this control
    ''' </summary>
    Private Property NonWorkingDays() As IBPSet(Of Date)

        Get
            Dim dates As New clsSortedSet(Of Date)
            For Each dw As DateWrapper In mOtherHolidaysList.Items
                dates.Add(dw.Value)
            Next
            Return dates
        End Get

        Set(ByVal value As IBPSet(Of Date))

            ' We want to display these sorted, so ensure the set is sorted first.
            Dim sorted As IBPSet(Of Date) = TryCast(value, clsSortedSet(Of Date))
            If sorted Is Nothing Then sorted = New clsSortedSet(Of Date)(value)

            mOtherHolidaysList.BeginUpdate()
            mOtherHolidaysList.Items.Clear()
            For Each dt As Date In sorted
                mOtherHolidaysList.Items.Add(New DateWrapper(dt))
            Next
            mOtherHolidaysList.EndUpdate()
        End Set

    End Property

    ''' <summary>
    ''' Structure to use to store dates on the 'non working day' list - this
    ''' just formats the date in a consistent manner.
    ''' </summary>
    Private Structure DateWrapper

        ''' <summary>
        ''' The date being wrapped
        ''' </summary>
        Private mDate As Date

        ''' <summary>
        ''' Creates a new date wrapper, wrapping the given date.
        ''' </summary>
        ''' <param name="dt">The date to be wrapped</param>
        Public Sub New(ByVal dt As Date)
            mDate = dt
        End Sub

        ''' <summary>
        ''' Creates a new date wrapper, wrapping the date in the given string.
        ''' </summary>
        ''' <param name="dateString">The date represented by a string. This must
        ''' be in the format defined by LongDatePattern - ie. "dd MMMM yyyy"</param>
        Public Sub New(ByVal dateString As String)
            Me.New(Date.ParseExact(dateString, Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongDatePattern, Nothing))
        End Sub

        ''' <summary>
        ''' Gets the wrapped date value held by this wrapped date
        ''' </summary>
        Public Property Value() As Date
            Get
                Return mDate
            End Get
            Set(ByVal val As Date)
                mDate = val
            End Set
        End Property

        ''' <summary>
        ''' Gets a string representation of this wrapped date. This is just the
        ''' date being wrapped in this instance formatted using LongDatePattern
        ''' </summary>
        ''' <returns>This date in the format "dd MMMM yyyy" localised for current culture</returns>
        Public Overrides Function ToString() As String
            Return mDate.ToString(Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongDatePattern)
        End Function

    End Structure

    ''' <summary>
    ''' Event handler to deal with the 'Add Other Holiday' / 'Add Non-working
    ''' Day' link label being clicked.
    ''' This adds the currently selected date in the date picker to the non-
    ''' working days list, ensuring that date order is maintained and that there
    ''' are no duplicates.
    ''' </summary>
    ''' <param name="sender">The link label which fired this event.</param>
    ''' <param name="e">Arguments detailing the event</param>
    Private Sub AddNonWorkingDayClicked( _
     ByVal sender As System.Object, ByVal e As LinkLabelLinkClickedEventArgs) _
     Handles mOtherHolidaysAddLink.LinkClicked

        Dim cal As ScheduleCalendar = SelectedCalendar
        Dim holidayDate As DateTime = If(mUseCultureAwarePicker, mCultureDatePicker.Value.Date, mOtherHolidayDatePicker.Value.Date)

        ' Get the chosen date/time
        Dim holiday As DateWrapper = New DateWrapper(holidayDate)

        ' Go through the list - if it's there, discard it (ie. don't re-add it)...
        ' if we pass the date, insert into the list there - maintains ascending date order
        For i As Integer = 0 To mOtherHolidaysList.Items.Count - 1
            Dim dateValue As Date = DirectCast(mOtherHolidaysList.Items(i), DateWrapper).Value
            If holidayDate = dateValue Then Return
            If holidayDate < dateValue Then
                mOtherHolidaysList.Items.Insert(i, holiday)
                Return
            End If
        Next i
        ' If we get here, then we got through the list without ever reaching the
        ' chosen date... ergo we should append it to the end of the list.
        mOtherHolidaysList.Items.Add(holiday)

    End Sub

    ''' <summary>
    ''' Event handler to deal with the 'Remove Other Holiday' / 'Remove
    ''' non-working day' link label being clicked.
    ''' This goes through all selected dates in the list of non-working days
    ''' and removes them from the list.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The event args.</param>
    Private Sub RemoveNonWorkingDayClicked( _
     ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) _
     Handles mOtherHolidaysRemoveLink.LinkClicked

        If e.Button <> MouseButtons.Left Then Return

        Dim index As Integer = mOtherHolidaysList.SelectedIndex
        While index >= 0

            Dim item As Date = DirectCast(mOtherHolidaysList.Items(index), DateWrapper).Value
            mOtherHolidaysList.Items.RemoveAt(index)
            index = mOtherHolidaysList.SelectedIndex

        End While

    End Sub

#End Region

#Region " Working Week "

    ''' <summary>
    ''' Gets the working week DaySet currently configured on this control.
    ''' </summary>
    <Browsable(False), _
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Property WorkingWeek() As DaySet
        Get
            Dim days As New DaySet()
            For Each cb As CheckBox In mWorkingWeekCheckboxes
                SetIntoDaySet(cb, days)
            Next
            Return days
        End Get
        Set(ByVal value As DaySet)
            For Each cb As CheckBox In mWorkingWeekCheckboxes
                CheckIfSet(cb, value)
            Next
        End Set
    End Property

    ''' <summary>
    ''' Initialises the 'DayOfWeek' checkboxes.
    ''' </summary>
    ''' <param name="cb">The checkbox to initialise</param>
    ''' <param name="day">The day of the week that the checkbox should represent
    ''' </param>
    Private Sub InitDayCheckBox(ByVal cb As CheckBox, ByVal day As DayOfWeek)
        cb.Tag = day
        mWorkingWeekCheckboxes.Add(cb)
    End Sub

    ''' <summary>
    ''' Checks the given day-of-week checkbox if its corresponding day in the
    ''' specified day set is present.
    ''' </summary>
    ''' <param name="cb">The checkbox to check. The Tag for this checkbox should
    ''' contain a DayOfWeek value.</param>
    ''' <param name="days">The dayset to check to see if the day represented by
    ''' this checkbox is contained within.</param>
    Private Sub CheckIfSet(ByVal cb As CheckBox, ByVal days As DaySet)
        cb.Checked = days.Contains(DirectCast(cb.Tag, DayOfWeek))
    End Sub

    ''' <summary>
    ''' Sets the day represented by the given day-of-week checkbox into the day
    ''' set, or clear it if the checkbox is checked or unchecked respectively.
    ''' </summary>
    ''' <param name="cb">The checkbox from which to draw the value. The Tag for
    ''' this checkbox should contain a DayOfWeek value.</param>
    ''' <param name="days">The dayset into which the day represented by the 
    ''' given checkbox should be set or cleared.</param>
    Private Sub SetIntoDaySet(ByVal cb As CheckBox, ByVal days As DaySet)
        Dim day As DayOfWeek = DirectCast(cb.Tag, DayOfWeek)
        If cb.Checked Then
            days.Set(day)
        Else
            days.Unset(day)
        End If
    End Sub

#End Region

#Region " Other methods "

    ''' <summary>
    ''' Checks if this control can safely be left - it can if there are no changes
    ''' made to the current calendar.
    ''' </summary>
    ''' <returns>True if there are no unsaved changes on the current calendar, or if
    ''' the user has instructed that they be discarded.</returns>
    Public Function CanLeave() As Boolean
        ' Ask to save changes first. If the user cancels then we cannot leave.
        ' If they answer yes or no, save or don't save as appropriate and allow
        ' exit from this control
        If Not mSavePromptTriggered Then
            Select Case AskUserToSaveChanges()
                Case DialogResult.Yes : SaveCurrentCalendar()
                Case DialogResult.Cancel : Return False
                Case DialogResult.No : mSavePromptTriggered = true
            End Select
        End If

        Return True
    End Function

    ''' <summary>
    ''' Sets the default values for this control. The default values are as 
    ''' follows :- <list>
    ''' <item>The 'blank' calendar is selected in the calendar combo box</item>
    ''' <item>The working week has Mon-Fri checked and Sat-Sun unchecked</item>
    ''' <item>No public holiday group (or overrides) are selected</item>
    ''' <item>No 'Other Holidays' are selected.</item>
    ''' </list>
    ''' </summary>
    Private Sub SetDefaultValues()
        SetDefaultValues(0)
    End Sub

    ''' <summary>
    ''' Sets the default values for the 'blank' calendar (ie. the calendar at
    ''' index 0 in the calendar combo box) in this control, after which the
    ''' 'preferred' calendar is selected. The default values for the blank
    ''' calendar are as follows :- <list>
    ''' <item>The working week has Mon-Fri checked and Sat-Sun unchecked</item>
    ''' <item>No public holiday group (or overrides) are selected</item>
    ''' <item>No 'Other Holidays' are selected.</item>
    ''' </list>
    ''' If preferredIndex is valid, it is selected, otherwise the blank calendar
    ''' entry is selected.
    ''' </summary>
    Private Sub SetDefaultValues(ByVal preferredIndex As Integer)
        ' First reset the 'blank' calendar - ie. the one at index 0
        mCalendarCombo.SelectedIndex = 0
        Me.WorkingWeek = DaySet.FiveDayWeek
        Me.SelectedPublicHolidayGroup = ""
        Me.NonWorkingDays = New clsSet(Of Date)()
        ' Set the preferred index if there is one given
        If preferredIndex > 0 AndAlso preferredIndex < mCalendarCombo.Items.Count Then
            mCalendarCombo.SelectedIndex = preferredIndex
            ' Otherwise, leave it where it is - ie. zero.
        End If
    End Sub

#End Region

End Class
