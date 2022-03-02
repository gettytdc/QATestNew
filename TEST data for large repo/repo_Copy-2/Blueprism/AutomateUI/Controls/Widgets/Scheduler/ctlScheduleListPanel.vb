Imports System.Globalization
Imports AutomateControls.UIState.UIElements
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth

Public Class ctlScheduleListPanel : Inherits UserControl : Implements IRefreshable

#Region "RelativeDateTime Structure"

    ''' <summary>
    ''' Structure which can be used in the relative date combo boxes to 
    ''' represent (and hold values for) relative dates.
    ''' </summary>
    Protected Structure RelativeDateItem

        ' The value in this structure.
        Private mValue As ScheduleRelativeDate

        ''' <summary>
        ''' Creates a new relative date item wrapping the given
        ''' relative date
        ''' </summary>
        ''' <param name="value">The relative date that this instance
        ''' represents.</param>
        Public Sub New(ByVal value As ScheduleRelativeDate)
            mValue = value
        End Sub

        ''' <summary>
        ''' The relative date value of this object.
        ''' </summary>
        Public ReadOnly Property Value() As ScheduleRelativeDate
            Get
                Return mValue
            End Get
        End Property

        ''' <summary>
        ''' Gets a string representation of this relative date.
        ''' </summary>
        ''' <returns>"Today", "Tomorrow", "Yesterday" or an empty string,
        ''' depending on the value of this object.</returns>
        Public Overrides Function ToString() As String
            Select Case mValue
                Case ScheduleRelativeDate.Today : Return My.Resources.ctlScheduleListPanel_Today
                Case ScheduleRelativeDate.Tomorrow : Return My.Resources.ctlScheduleListPanel_Tomorrow
                Case ScheduleRelativeDate.Yesterday : Return My.Resources.ctlScheduleListPanel_Yesterday
                Case Else : Return ""
            End Select
        End Function

        ''' <summary>
        ''' Checks if the 2 relative date item values are equal.
        ''' </summary>
        ''' <param name="d1">The first relative date item to check.</param>
        ''' <param name="d2">The second relative date item to check</param>
        ''' <returns>True if the 2 instances represent the same relative date
        ''' value; false otherwise.</returns>
        Public Shared Operator =(ByVal d1 As RelativeDateItem, ByVal d2 As RelativeDateItem) As Boolean
            Return d1.Value = d2.Value
        End Operator

        ''' <summary>
        ''' Checks if the 2 relative date item values are not equal.
        ''' </summary>
        ''' <param name="d1">The first relative date item to check.</param>
        ''' <param name="d2">The second relative date item to check</param>
        ''' <returns>True if the 2 instances represent different relative date
        ''' values; false if they represent the same value.</returns>
        Public Shared Operator <>(ByVal d1 As RelativeDateItem, ByVal d2 As RelativeDateItem) As Boolean
            Return d1.Value <> d2.Value
        End Operator

    End Structure

#End Region

    ''' <summary>
    ''' Event fired when a link referring to a schedule instance has been
    ''' clicked in the schedule instance list.
    ''' </summary>
    ''' <param name="instance">The schedule instance which was referred to
    ''' by the link click.</param>
    ''' <param name="linkText">The text on the link that was clicked.</param>
    <Category("Action")>
    <Description("Occurs when a link is clicked on a schedule instance in the schedule list")>
    Public Event LinkClick(ByVal instance As IScheduleInstance, ByVal linkText As String)

#Region "Predefined RelativeDateItems"

    Protected Shared ReadOnly RelativeNone As New RelativeDateItem(ScheduleRelativeDate.None)
    Protected Shared ReadOnly RelativeToday As New RelativeDateItem(ScheduleRelativeDate.Today)
    Protected Shared ReadOnly RelativeTomorrow As New RelativeDateItem(ScheduleRelativeDate.Tomorrow)
    Protected Shared ReadOnly RelativeYesterday As New RelativeDateItem(ScheduleRelativeDate.Yesterday)

#End Region

    ' The list that this panel is displaying
    Protected mList As ScheduleList

    ' Flag indicating if this panel is set to be readonly or not
    Private mReadonly As Boolean

    ' Whether to use the culture aware calendar
    Private mUseCultureAwarePicker As Boolean

    ''' <summary>
    ''' Creates a new schedule list panel.
    ''' </summary>
    Public Sub New()

        mUseCultureAwarePicker = Options.Instance.UseCultureCalendar

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        comboRelativeDate.Items.Clear()
        For Each item As RelativeDateItem In GetRelativeDateItems()
            comboRelativeDate.Items.Add(item)
        Next
        If comboRelativeDate.Items.Count > 0 Then ' Assume we want the first item (usually 'Today')
            comboRelativeDate.SelectedIndex = 1
        End If

        lblShowDays.Text = ShowVerbing
        lblDaysDistance.Text = DaysVerbing
        If lblShowDays.Width > updnDaysDistance.Left Then
            Dim incrSize = lblShowDays.Width - updnDaysDistance.Left
            updnDaysDistance.Left = updnDaysDistance.Left + incrSize
            lblDaysDistance.Left = lblDaysDistance.Left + incrSize
            incrSize = If(lblDaysDistance.Right > radioRelativeDate.Left, lblDaysDistance.Right - radioRelativeDate.Left, incrSize)
            radioRelativeDate.Left = radioRelativeDate.Left + incrSize
            comboRelativeDate.Left = comboRelativeDate.Left + incrSize
            radioAbsoluteDate.Left = radioAbsoluteDate.Left + incrSize
            dateAbsoluteDate.Left = dateAbsoluteDate.Left + incrSize
        End If

        Me.ReadOnly = Not (
         User.LoggedIn AndAlso User.Current.HasPermission("Edit Schedule")
        )

        If (mUseCultureAwarePicker) Then
            cultureDateAbsoluteDate.Location = dateAbsoluteDate.Location
            cultureDateAbsoluteDate.Width = dateAbsoluteDate.Width
            cultureDateAbsoluteDate.Visible = True
            cultureDateAbsoluteDate.Culture = New CultureInfo(CultureInfo.CurrentCulture.Name)
            dateAbsoluteDate.Visible = False
        Else
            cultureDateAbsoluteDate.Visible = False
        End If


    End Sub


    ''' <summary>
    ''' Gets the list of relative date items which is held in this list
    ''' control. Subclasses must implement this to return the list of relative
    ''' date items which is relevant to them.
    ''' </summary>
    ''' <returns>A non-null list of relative date items to display in this
    ''' schedule list panel.</returns>
    Protected Overridable Function GetRelativeDateItems() As IList(Of RelativeDateItem)
        Return New List(Of RelativeDateItem)
    End Function

    ''' <summary>
    ''' The verb for the label which sits between the 'days' and the date to
    ''' which those days apply. ie.
    ''' In the statement "Show 5 days [label] Today", this should return the
    ''' text which goes into [label].
    ''' </summary>
    Protected Overridable ReadOnly Property DaysVerbing() As String
        Get
            Return ""
        End Get
    End Property

    ''' <summary>
    ''' The verb for the label which sits between "Show the" and the days distance
    ''' updown textbox.
    ''' </summary>
    Protected Overridable ReadOnly Property ShowVerbing() As String
        Get
            Return ""
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the relative date held in this list panel.
    ''' </summary>
    Public Property RelativeDate() As ScheduleRelativeDate
        Get
            If radioAbsoluteDate.Checked Then Return ScheduleRelativeDate.None
            Dim di As RelativeDateItem =
             CType(clsUserInterfaceUtils.GetSelectedItem(comboRelativeDate), RelativeDateItem)
            Return di.Value
        End Get
        Set(ByVal value As ScheduleRelativeDate)
            comboRelativeDate.SelectedItem = New RelativeDateItem(value)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the fixed date held in this list panel
    ''' </summary>
    Public Property AbsoluteDate() As Date
        Get
            Return If(mUseCultureAwarePicker, cultureDateAbsoluteDate.Value, dateAbsoluteDate.Value)
        End Get
        Set(ByVal value As Date)
            If value = Date.MinValue OrElse value = Date.MaxValue Then
                value = Now
            End If
            dateAbsoluteDate.Value = value
            cultureDateAbsoluteDate.Value = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the readonly state of this panel.
    ''' </summary>
    Public Property [ReadOnly]() As Boolean
        Get
            Return mReadonly
        End Get
        Set(ByVal value As Boolean)
            mReadonly = value

            txtName.ReadOnly = value
            txtDesc.ReadOnly = value
            updnDaysDistance.ReadOnly = value
            updnDaysDistance.Increment = CInt(IIf(value, 0, 1))
            radioAbsoluteDate.AutoCheck = Not value
            radioRelativeDate.AutoCheck = Not value
            listboxSchedules.Enabled = Not value
            clsUserInterfaceUtils.ShowReadOnlyControl(value, comboRelativeDate, txtRelativeDate)
            clsUserInterfaceUtils.ShowReadOnlyControl(value, dateAbsoluteDate, txtAbsoluteDate)
            clsUserInterfaceUtils.ShowReadOnlyControl(value, cultureDateAbsoluteDate, txtAbsoluteDate)
        End Set
    End Property

    ''' <summary>
    ''' Populates this control with the given list configuration, and displays the
    ''' schedule instances loaded from it.
    ''' </summary>
    ''' <param name="list">The list to populate this panel with.</param>
    Public Sub Populate(list As ScheduleList)
        InnerPopulate(list)
        btnExport.Enabled = (gridScheduleList.Rows.Count > 0)
    End Sub

    ''' <summary>
    ''' Performs the population of the list and its data into this control.
    ''' </summary>
    ''' <param name="list">The list to populate this panel with.</param>
    Protected Overridable Sub InnerPopulate(ByVal list As ScheduleList)

        mList = Nothing

        txtName.Text = list.Name
        txtDesc.Text = list.Description
        If list.DaysDistance = 0 Then list.DaysDistance = 1
        updnDaysDistance.Value = list.DaysDistance

        radioRelativeDate.Checked = (list.RelativeDate <> ScheduleRelativeDate.None)
        radioAbsoluteDate.Checked = (list.RelativeDate = ScheduleRelativeDate.None)

        Me.RelativeDate = list.RelativeDate
        Me.AbsoluteDate = list.AbsoluteDate

        listboxSchedules.Items.Clear()
        Dim allSchedulesItem As ListViewItem = listboxSchedules.Items.Add(My.Resources.ctlScheduleListPanel_AllSchedules)
        allSchedulesItem.Checked = list.AllSchedules
        For Each schedule As SessionRunnerSchedule In list.Store.GetActiveSchedules
            Dim it As ListViewItem = listboxSchedules.Items.Add(schedule.Name)
            it.Tag = schedule
            it.Checked = list.Schedules.Contains(schedule)
            If list.AllSchedules Then
                it.Checked = True
                it.ForeColor = Color.Gray
            End If
        Next
        columnSchedules.Width = -1

        mList = list

    End Sub

    ''' <summary>
    ''' Refreshes the data in this list.
    ''' </summary>
    Public Sub RefreshView() Implements IRefreshable.RefreshView
        Populate(mList)
    End Sub

#Region "UI Event Handlers"

    ''' <summary>
    ''' Handles the 'Refresh' button being clicked.
    ''' </summary>
    Private Sub HandleRefreshClicked(ByVal sender As Object, ByVal e As EventArgs) _
     Handles buttonRefresh.Click
        RefreshView()
    End Sub

    ''' <summary>
    ''' Handles the 'Export' button being clicked
    ''' </summary>
    Private Sub HandleExportClicked(sender As Object, e As EventArgs) Handles btnExport.Click
        Using f As New frmScheduleListExport() With {.ExportList = mList}
            f.ShowInTaskbar = False
            f.ShowDialog()
        End Using
    End Sub

    ''' <summary>
    ''' Handles the name of the list being changed.
    ''' </summary>
    Private Sub NameChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtName.TextChanged
        If mList IsNot Nothing Then mList.Name = txtName.Text
    End Sub

    ''' <summary>
    ''' Handles the description of the list being changed.
    ''' </summary>
    Private Sub DescriptionChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtDesc.TextChanged
        If mList IsNot Nothing Then mList.Description = txtDesc.Text
    End Sub

    ''' <summary>
    ''' Handles the 'fixed' date of the list changing
    ''' </summary>
    Private Sub dateAbsoluteDate_ValueChanged(ByVal sender As Object, ByVal e As EventArgs) Handles dateAbsoluteDate.ValueChanged, cultureDateAbsoluteDate.ValueChanged
        If mList IsNot Nothing Then mList.AbsoluteDate = If(mUseCultureAwarePicker, cultureDateAbsoluteDate.Value, dateAbsoluteDate.Value)
    End Sub

    ''' <summary>
    ''' Handles the state of a schedule for the list being changed.
    ''' </summary>
    Private Sub listboxSchedules_ItemCheck(ByVal sender As Object, ByVal e As ItemCheckEventArgs) Handles listboxSchedules.ItemCheck
        If mList IsNot Nothing Then
            Dim item As SessionRunnerSchedule = TryCast(listboxSchedules.Items(e.Index).Tag, SessionRunnerSchedule)
            If item Is Nothing Then ' All Schedules
                SetAllSchedules(e.NewValue = CheckState.Checked)
            Else
                If mList.AllSchedules Then
                    e.NewValue = e.CurrentValue
                Else
                    If e.NewValue = CheckState.Checked Then
                        mList.AddSchedule(item)
                    Else
                        mList.RemoveSchedule(item)
                    End If
                End If
            End If
        End If
    End Sub

    ''' <summary>
    ''' Sets the 'all schedules' value for this list to be the given value.
    ''' </summary>
    ''' <param name="state">True to indicate that this list should include all
    ''' schedules; False to indicate that it should not.</param>
    Private Sub SetAllSchedules(ByVal state As Boolean)
        'Set the mlist.Allschedules before the loop when checkstate = false
        'But after the loop when checkstate = true, this is because in the loop
        'we do it.Checked, which then fires off the ItemCheck Event, so in one 
        'instance we need to suppress it ItemCheck event before the loop, and 
        'in the other we need to supress it after the loop
        If Not state Then mList.AllSchedules = False

        For Each it As ListViewItem In listboxSchedules.Items
            If it.Tag IsNot Nothing Then
                If Not state Then
                    it.Checked = False
                    it.ForeColor = Color.Black
                Else
                    it.Checked = True
                    it.ForeColor = Color.Gray
                End If
            End If
        Next

        If state Then mList.AllSchedules = True
    End Sub


    ''' <summary>
    ''' Handler for the days distance text field being validated.
    ''' Ensures that the value is set in the list object.
    ''' </summary>
    Private Sub txtDaysDistance_Validated(ByVal sender As Object, ByVal e As EventArgs) _
     Handles updnDaysDistance.Validated
        mList.DaysDistance = CInt(updnDaysDistance.Value)
    End Sub

    ''' <summary>
    ''' Handler for the relative/absolute radio buttons being changed.
    ''' </summary>
    Private Sub RadioCheckChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles radioAbsoluteDate.CheckedChanged, radioRelativeDate.CheckedChanged
        If mList IsNot Nothing Then
            mList.RelativeDate = Me.RelativeDate
            mList.AbsoluteDate = If(mUseCultureAwarePicker, Me.cultureDateAbsoluteDate.Value, Me.dateAbsoluteDate.Value)
        End If
        comboRelativeDate.Enabled = radioRelativeDate.Checked
        dateAbsoluteDate.Enabled = radioAbsoluteDate.Checked
        cultureDateAbsoluteDate.Enabled = radioAbsoluteDate.Checked
    End Sub

    ''' <summary>
    ''' Handler for the relative date combo box changing
    ''' </summary>
    Private Sub RelativeDateChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles comboRelativeDate.SelectedIndexChanged
        If mList IsNot Nothing Then mList.RelativeDate = Me.RelativeDate
    End Sub

    ''' <summary>
    ''' Handle the contents of the schedule instance list being clicked.
    ''' This just checks to see if a link has been clicked, if it has it chains
    ''' to the LinkClick event for any interested parties (ie. subclasses)
    ''' </summary>
    Private Sub gridScheduleList_CellContentClick(
     ByVal sender As Object, ByVal e As DataGridViewCellEventArgs) _
     Handles gridScheduleList.CellContentClick

        If e.RowIndex = -1 Then Return ' User clicked the column header

        Dim row As DataGridViewRow = gridScheduleList.Rows(e.RowIndex)
        Dim cell As DataGridViewCell = row.Cells(e.ColumnIndex)

        ' We're only interested in the links which use datagridviewlinkcell
        If Not TypeOf cell Is DataGridViewLinkCell Then Exit Sub

        cell.Selected = False ' Don't highlight the cell - it looks nasty
        RaiseEvent LinkClick(TryCast(row.Tag, IScheduleInstance), CStr(cell.Value))

    End Sub



#End Region

End Class
