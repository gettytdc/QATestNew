Imports BluePrism.AutomateAppCore.Auth
Imports AutomateControls
Imports BluePrism.Data.DataModels.WorkQueueAnalysis
Imports NodaTime
Imports System.Globalization
Imports BluePrism.AutomateAppCore
Imports AutomateUI.My.Resources
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.Server.Domain.Models

Public Class SnapshotConfigurationForm
    Implements IChild, IPermission, IHelp

    Private mSnapshotConfiguration As SnapshotConfiguration
    Private mConfiguredQueues As List(Of Integer)

    Private Const HelpFileName = "work-queue-analysis.htm"

    Private mOriginalConfigName As String = Nothing

    Private Const SnapshotColumnName As String = "Snapshot?"

    Public Sub New(config As SnapshotConfiguration)
        InitializeComponent()
        Me.KeyPreview = True
        mSnapshotConfiguration = config
        mOriginalConfigName = mSnapshotConfiguration.Name
        If mSnapshotConfiguration.Id <> -1 Then
            Try
                mConfiguredQueues = If(
                gSv.GetWorkQueueIdentifiersAssociatedToSnapshotConfiguration(mSnapshotConfiguration.Id)?.ToList,
                New List(Of Integer))
            Catch ex As Exception
                UserMessage.Err(ex,
                 SnapshotConfigurationForm_Resources.ErrorFailedToLoadQueues_Template,
                 ex.Message)
            End Try
        End If
    End Sub

    Private Sub SnapshotConfigurationForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadTimeControls()
        PopulateTimezoneDropdownList()
        LoadIntervalControl()
        LoadQueuesGrid()

        txtConfigName.Text = mSnapshotConfiguration.Name
        chkIsEnabled.Checked = mSnapshotConfiguration.Enabled
        cboStartTime.SelectedValue = mSnapshotConfiguration.StartTime
        cboEndTime.SelectedValue = mSnapshotConfiguration.EndTime
        cboInterval.SelectedIndex = mSnapshotConfiguration.Interval
        cboTimezone.SelectedItem = mSnapshotConfiguration.Timezone
        chkMonday.Checked = mSnapshotConfiguration.DaysOfTheWeek.Monday
        chkTuesday.Checked = mSnapshotConfiguration.DaysOfTheWeek.Tuesday
        chkWednesday.Checked = mSnapshotConfiguration.DaysOfTheWeek.Wednesday
        chkThursday.Checked = mSnapshotConfiguration.DaysOfTheWeek.Thursday
        chkFriday.Checked = mSnapshotConfiguration.DaysOfTheWeek.Friday
        chkSaturday.Checked = mSnapshotConfiguration.DaysOfTheWeek.Saturday
        chkSunday.Checked = mSnapshotConfiguration.DaysOfTheWeek.Sunday

        EnableDisableCheckBoxes()
    End Sub

    Private Sub LoadTimeControls()
        Dim listOfStartTimes = New DataTable()

        listOfStartTimes.Columns.Add("TimeValue")
        listOfStartTimes.Columns.Add("DisplayValue")
        listOfStartTimes.Columns.Add("HourValue")
        listOfStartTimes.Columns.Add("MinuteValue")

        For hour As Integer = 0 To 23
            For minute As Integer = 0 To 3
                Dim minuteValue = minute * 15
                Dim newValue = New LocalTime(hour, minuteValue)
                Dim displayValue = newValue.ToString("HH:mm", CultureInfo.InvariantCulture)
                listOfStartTimes.Rows.Add(newValue, displayValue, hour, minuteValue)
            Next
        Next

        Dim listOfEndTimes As DataTable = listOfStartTimes.Copy()

        cboStartTime.DataSource = listOfStartTimes
        cboStartTime.DisplayMember = "DisplayValue"
        cboStartTime.ValueMember = "TimeValue"

        cboEndTime.DataSource = listOfEndTimes
        cboEndTime.DisplayMember = "DisplayValue"
        cboEndTime.ValueMember = "TimeValue"
    End Sub

    Private Sub PopulateTimezoneDropdownList()
        Dim timeZones As IEnumerable(Of TimeZoneInfo)

        timeZones = TimeZoneInfo _
            .GetSystemTimeZones() _
            .Where(Function(x) x.BaseUtcOffset <= TimeSpan.FromHours(12)) _
            .ToList()

        cboTimezone.DataSource = timeZones
    End Sub

    Private Sub LoadQueuesGrid()
        Dim allQueues = Enumerable.Empty(Of clsWorkQueue)
        Dim allConfigs = Enumerable.Empty(Of SnapshotConfiguration)

        Dim queuesTable = New DataTable("WorkQueues")
        queuesTable.Columns.Add(New DataColumn(SnapshotColumnName, GetType(Boolean)))
        queuesTable.Columns.Add(New DataColumn("Name", GetType(String)))
        queuesTable.Columns.Add(New DataColumn("CurrentConfig", GetType(String)))
        queuesTable.Columns.Add(New DataColumn("Ident", GetType(Integer)))
        Try
            allConfigs = gSv.GetSnapshotConfigurations()
            allQueues = gSv.WorkQueueGetAllQueues()
        Catch ex As Exception
            UserMessage.Err(ex,
                SnapshotConfigurationForm_Resources.ErrorFailedToLoadQueues_Template,
                ex.Message)
        End Try
        For Each queue In allQueues
            Dim row As DataRow = queuesTable.NewRow()
            row("Name") = queue.Name
            row(SnapshotColumnName) = If(mConfiguredQueues?.Contains(queue.Ident), True, False)
            row("Ident") = queue.Ident
            If queue.SnapshotConfigurationId <> -1 AndAlso queue.SnapshotConfigurationId <> Nothing Then
                row("CurrentConfig") =
                    allConfigs.
                    FirstOrDefault(Function(c) c.Id = queue.SnapshotConfigurationId).
                    Name
            End If
            queuesTable.Rows.Add(row)
        Next

        dgvQueues.DataSource = queuesTable

        dgvQueues.Columns(0).HeaderText = Nothing
        dgvQueues.Columns(1).HeaderText = SnapshotConfigurationForm_Resources.QueueGrid_NameHeader
        dgvQueues.Columns(2).HeaderText = SnapshotConfigurationForm_Resources.QueueGrid_CurrentConfigHeader

        dgvQueues.Columns(1).ReadOnly = True
        dgvQueues.Columns(2).ReadOnly = True

        dgvQueues.Columns(3).Visible = False
        dgvQueues.DefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomLeft
        dgvQueues.Columns(0).DefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomLeft
        dgvQueues.Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        EnableSorting()
    End Sub

    Private Sub LoadIntervalControl()
        Dim resourceManager = WorkQueueAnalysis_Resources.ResourceManager

        cboInterval.BindToLocalisedEnumItems(Of SnapshotInterval)(resourceManager, SnapshotIntervalExtensions.ResourceTemplate)
    End Sub

    Private Sub EnableDisableCheckBoxes()
        chkSpecificDays.Checked = Not mSnapshotConfiguration.DaysOfTheWeek.IsEmpty
        chkSpecificTimes.Checked = mSnapshotConfiguration.StartTime <> LocalTime.MinValue OrElse
            mSnapshotConfiguration.EndTime <> LocalTime.MinValue
        pnlSpecificDays.Enabled = chkSpecificDays.Checked
        pnlSpecificTimes.Enabled = chkSpecificTimes.Checked
    End Sub

    Private Sub EnableSorting()
        For Each column As DataGridViewColumn In dgvQueues.Columns
            column.SortMode = DataGridViewColumnSortMode.Automatic
        Next
    End Sub

#Region "Drag And Drop Window"

    Dim mDragging As Boolean = False
    Dim mDragCursorPoint As Point
    Dim mDragFormPoint As Point

    Private Sub BorderPanel_MouseDown(sender As Object, e As MouseEventArgs) Handles BorderPanel.MouseDown   
        PanelControl_MouseDownHandler()
    End Sub

    Private Sub Panel1_MouseDown(sender As Object, e As MouseEventArgs) Handles Panel1.MouseDown
        PanelControl_MouseDownHandler()
    End Sub

    Private Sub Panel2_MouseDown(sender As Object, e As MouseEventArgs) Handles Panel2.MouseDown
        PanelControl_MouseDownHandler()
    End Sub

    Private Sub PanelControl_MouseDownHandler()
        mDragging = true
        mDragCursorPoint = Cursor.Position
        mDragFormPoint = Location
    End Sub

    Private Sub BorderPanel_MouseMove(sender As Object, e As MouseEventArgs) Handles BorderPanel.MouseMove   
        PanelControl_MouseMoveHandler()
    End Sub

    Private Sub Panel1_MouseMove(sender As Object, e As MouseEventArgs) Handles Panel1.MouseMove
        PanelControl_MouseMoveHandler()
    End Sub

    Private Sub Panel2_MouseMove(sender As Object, e As MouseEventArgs) Handles Panel2.MouseMove
        PanelControl_MouseMoveHandler()
    End Sub

    Private Sub PanelControl_MouseMoveHandler()
        If mDragging
            Dim dif = Point.Subtract(Cursor.Position, new Size(mDragCursorPoint))
            Location = Point.Add(mDragFormPoint, new Size(dif))
        End If
    End Sub

    Private Sub BorderPanel_MouseUp(sender As Object, e As MouseEventArgs) Handles BorderPanel.MouseUp
        PanelControl_MouseUpHandler()
    End Sub

    Private Sub Panel1_MouseUp(sender As Object, e As MouseEventArgs) Handles Panel1.MouseUp
        PanelControl_MouseUpHandler()
    End Sub

    Private Sub Panel2_MouseUp(sender As Object, e As MouseEventArgs) Handles Panel2.MouseUp
        PanelControl_MouseUpHandler()
    End Sub

    Private Sub PanelControl_MouseUpHandler()
        mDragging = False
    End Sub

#End Region

#Region "Event Handlers"

    Private mSuppressMultiDayCheckEvent As Boolean
    Private mSuppressSingleDayCheckEvent As Boolean

    Private Sub HandleDayCheckboxesCheckedChanged(sender As Object, e As EventArgs) Handles chkMonday.CheckedChanged, _
        chkTuesday.CheckedChanged, chkWednesday.CheckedChanged, chkThursday.CheckedChanged, chkFriday.CheckedChanged, _
        chkSaturday.CheckedChanged, chkSunday.CheckedChanged

        mSuppressSingleDayCheckEvent = True
        If Not mSuppressMultiDayCheckEvent Then
            chkMonToFri.Checked = chkMonday.Checked AndAlso chkTuesday.Checked AndAlso chkWednesday.Checked _
            AndAlso chkThursday.Checked AndAlso chkFriday.Checked

            chkSatToSun.Checked = chkSaturday.Checked AndAlso chkSunday.Checked
        End If
        mSuppressSingleDayCheckEvent = False
    End Sub

    Private Sub chkMonToFri_CheckedChanged(sender As Object, e As EventArgs) Handles chkMonToFri.CheckedChanged   
        mSuppressMultiDayCheckEvent = True
        If Not mSuppressSingleDayCheckEvent Then
            chkMonday.Checked = chkMonToFri.Checked
            chkTuesday.Checked = chkMonToFri.Checked
            chkWednesday.Checked = chkMonToFri.Checked
            chkThursday.Checked = chkMonToFri.Checked
            chkFriday.Checked = chkMonToFri.Checked
        End If
        mSuppressMultiDayCheckEvent = False
    End Sub

    Private Sub chkSatToSun_CheckedChanged(sender As Object, e As EventArgs) Handles chkSatToSun.CheckedChanged   
        mSuppressMultiDayCheckEvent = True
        If Not mSuppressSingleDayCheckEvent Then
            chkSaturday.Checked = chkSatToSun.Checked
            chkSunday.Checked = chkSatToSun.Checked
        End If
        mSuppressMultiDayCheckEvent = False
    End Sub

    Private Sub chkSpecificTimes_CheckedChanged(sender As Object, e As EventArgs) Handles chkSpecificTimes.CheckedChanged   
        pnlSpecificTimes.Enabled = chkSpecificTimes.Checked
    End Sub

    Private Sub chkSpecificDays_CheckedChanged(sender As Object, e As EventArgs) Handles chkSpecificDays.CheckedChanged   
        pnlSpecificDays.Enabled = chkSpecificDays.Checked
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    Private Sub btnOk_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Try
            btnOK.Enabled = False
            ApplyAndSaveConfiguration()
        Finally
            btnOK.Enabled = True
        End Try
    End Sub

    Private Async Sub ApplyAndSaveConfiguration()
        Try
            Dim configToSave As SnapshotConfiguration = GetConfiguration()
            Dim listQueuesToConfigure = ReadQueuesToBeConfiguredFromGrid()

            Dim validConfiguration = await ValidateConfiguration(configToSave, listQueuesToConfigure)

            If validConfiguration Then
                Dim saveSuccessful = Await Task.Run(Function() gSv.SaveConfigurationAndApplyToQueues(configToSave, mOriginalConfigName, listQueuesToConfigure))

                If saveSuccessful Then
                    DialogResult = DialogResult.OK
                    Close()
                End If
            End If
        Catch ex As NameAlreadyExistsException
            UserMessage.Err(ex, SnapshotConfigurationForm_Resources.Error_NameNotUnique)
        Catch ex As Exception
            UserMessage.Err(ex, SnapshotConfigurationForm_Resources.ErrorFailedToApplyConfigurationChanges_Template, ex.Message)
        End Try
    End Sub

    Private async Function ValidateConfiguration(configToSave As SnapshotConfiguration, listQueuesToConfigure As List(Of Integer)) As Task(Of Boolean)
        If configToSave Is Nothing Then
            Return false
        End If

        Dim snapshotLimitExceeded = Await Task.Run(Function() gSv.ConfigurationChangesWillExceedPermittedSnapshotLimit(configToSave, listQueuesToConfigure))
        If snapshotLimitExceeded Then
            UserMessage.Err(SnapshotConfigurationForm_Resources.Error_ChangesWouldExceedTheMaximumPermittedConfigRows)
            Return false
        End If

        If Not configToSave.IsValidConfiguration() Then
            ShowInvalidConfigMessage(configToSave)
            Return false
        End If

        Dim mustWarnUser = Await Task.Run(Function() gSv.ConfigurationChangesWillCauseDataDeletion(configToSave, mOriginalConfigName, listQueuesToConfigure))                
        If mustWarnUser Then 
            Dim warningMessage = SnapshotConfigurationForm_Resources.Warning_TrendDataWillBeDeletedWhenApplyingTheseChanges
            If UserMessage.YesNo(warningMessage) <> MsgBoxResult.Yes Then Return false
        End If

        Return true
    End Function

    Private Function ReadQueuesToBeConfiguredFromGrid() As List(Of Integer)
        Dim listQueuesToConfigure = New List(Of Integer)()
        Dim queues = dgvQueues.Rows
        For Each queueRow As DataGridViewRow In queues
            If CBool(queueRow.Cells(SnapshotColumnName).Value) = True Then
                listQueuesToConfigure.Add(CInt(queueRow.Cells("Ident").Value))
            End If
        Next
        Return listQueuesToConfigure
    End Function

    Private Sub btnEnableAll_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles btnEnableAll.LinkClicked
        Dim table = CType(dgvQueues.DataSource, DataTable)
        Dim snapshotColumn = table.Columns(SnapshotColumnName)

        Dim areAllRowsSelectedForSnapshot = table.AsEnumerable.All(Function(row)
                                                            Return CBool(row(snapshotColumn))
                                                        End Function)

        For Each row As DataRow In table.Rows
            row(snapshotColumn) = Not areAllRowsSelectedForSnapshot
        Next
    End Sub

    Private Sub btnEnableSelected_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles btnEnableSelected.LinkClicked
        Dim selectedRows = dgvQueues.SelectedRows
        Dim rowsToEnable = New List(Of Integer)
        Dim areAllRowsSelectedForSnapshot = True

        For Each dgvRow As DataGridViewRow In selectedRows
            rowsToEnable.Add(dgvRow.Index)
            If CBool(dgvRow.Cells.Item(SnapshotColumnName).Value) = False Then
                areAllRowsSelectedForSnapshot = False
            End If
        Next

        Dim table = CType(dgvQueues.DataSource, DataTable)
        Dim snapshotColumn = table.Columns(SnapshotColumnName)

        For Each row As DataRow In table.Rows
            If rowsToEnable.Contains(table.Rows.IndexOf(row)) Then
                row(snapshotColumn) = Not areAllRowsSelectedForSnapshot
            End If
        Next
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub


#End Region

    Private Function GetConfiguration() As SnapshotConfiguration
        Dim startTime As LocalTime = Nothing
        Dim endTime As LocalTime = Nothing

        If chkSpecificTimes.Checked Then
            Dim startRow = CType(cboStartTime.SelectedItem, DataRowView)
            startTime = New LocalTime(CInt(startRow("HourValue")), CInt(startRow("MinuteValue")))

            Dim endRow = CType(cboEndTime.SelectedItem, DataRowView)
            endTime = New LocalTime(CInt(endRow("HourValue")), CInt(endRow("MinuteValue")))
        End If

        Dim selectedIntervalItem = CType(cboInterval.SelectedItem, ComboBoxItem)
        Dim interval = CType(selectedIntervalItem.Tag, SnapshotInterval)
        Dim timezone = CType(cboTimezone.SelectedItem, TimeZoneInfo)

        Dim daysOfWeek = New SnapshotDayConfiguration(True, True, True, True, True, True, True)
        If chkSpecificDays.Checked Then
            daysOfWeek = New SnapshotDayConfiguration(chkMonday.Checked, chkTuesday.Checked, chkWednesday.Checked,
                                                      chkThursday.Checked, chkFriday.Checked, chkSaturday.Checked, chkSunday.Checked)
        End If

        Return New SnapshotConfiguration(mSnapshotConfiguration.Id, chkIsEnabled.Checked, txtConfigName.Text,
                                                      interval, timezone, startTime, endTime, daysOfWeek)
    End Function

    Private Sub ShowInvalidConfigMessage(configToSave As SnapshotConfiguration)
        Dim sb As New StringBuilder
        sb.Append(vbCrLf)
        If configToSave.NameIsNullEmptyOrWhitespace Then
            sb.Append("- ")
            sb.Append(SnapshotConfigurationForm_Resources.ErrorInvalidConfig_NameIsEmpty)
            sb.Append(vbCrLf)
        End If

        If configToSave.NameIsTooLong Then
            sb.Append("- ")
            sb.Append(SnapshotConfigurationForm_Resources.ErrorInvalidConfig_NameTooLong)
            sb.Append(vbCrLf)
        End If

        If configToSave.EndDateIsBeforeStartDate Then
            sb.Append("- ")
            sb.Append(SnapshotConfigurationForm_Resources.ErrorInvalidConfig_StartIsAfterEnd)
            sb.Append(vbCrLf)
        End If

        UserMessage.Err(
            SnapshotConfigurationForm_Resources.ErrorInvalidConfig_BaseMessage & "{0}", sb.ToString)
    End Sub

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public ReadOnly Property RequiredPermissions As ICollection(Of Permission) Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(Permission.SystemManager.System.Reporting)
        End Get
    End Property

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return HelpFileName
    End Function

    Private Sub SnapshotConfigurationForm_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        If e.KeyCode = Keys.F1 Then
            Try
                OpenHelpFile(Me, GetHelpFile())
            Catch
                UserMessage.Err(CannotOpenOfflineHelp)
            End Try
        End If
    End Sub

End Class
