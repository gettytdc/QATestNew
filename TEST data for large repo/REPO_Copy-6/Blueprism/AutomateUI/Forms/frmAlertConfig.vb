Imports System.Text.RegularExpressions
Imports AutomateControls
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.BPCoreLib
Imports BluePrism.Scheduling

''' <summary>
''' A form to manage a user's alert configuration and display alert history.
''' </summary>
Public Class frmAlertConfig : Implements IHelp, IPermission

#Region " Class-scope Members "

    ''' <summary>
    ''' The mode used to view the alert config form
    ''' </summary>
    Public Enum ViewMode
        ProcessConfig
        SchedulerConfig
        History
        Email
        SMS
    End Enum

    ''' <summary>
    ''' The current instance
    ''' </summary>
    Private Shared mExistingInstance As frmAlertConfig

#End Region

#Region " Instance Members "

    ''' <summary>
    ''' This user's ID. This should never be empty; user credentials are essential
    ''' to all operations for monitoring process alerts.
    ''' </summary>
    Private mUser As User

    ''' <summary>
    ''' The current history date.
    ''' </summary>
    ''' <remarks></remarks>
    Private mHistoryDate As DateTime

    ''' <summary>
    ''' The history data.
    ''' </summary>
    ''' <remarks></remarks>
    Private mHistoryData As DataTable

    ''' <summary>
    ''' The index of the "Date" column in the alert history gridview.
    ''' This is used when formatting the cell
    ''' </summary>
    Private mDateColumnIndex As Integer

    ''' <summary>
    ''' The index of the "Method" column in the alert history gridview.
    ''' This is used when formatting the cell (from integer => enum name)
    ''' </summary>
    Private mMethodColumnIndex As Integer

    ''' <summary>
    ''' The index of the "Type" column in the alert history gridview.
    ''' This is used when formatting the cell (from integer => enum name)
    ''' </summary>
    Private mTypeColumnIndex As Integer

    ''' <summary>
    ''' Records whether the user has permission to view the history tab
    ''' </summary>
    Private mCanViewHistory As Nullable(Of Boolean)

    ''' <summary>
    ''' Records whether the user has permission to view the config tab
    ''' </summary>
    Private mCanViewConfig As Nullable(Of Boolean)

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Starts the form with the user's config details and alert history. 
    ''' </summary>
    ''' <param name="user">The user object</param>
    ''' <param name="Mode">The view mode</param>
    Public Sub New(ByVal user As User, Optional ByVal Mode As ViewMode = ViewMode.ProcessConfig)
        Debug.Assert(user IsNot Nothing, "User not set in frmAlertConfig")

        InitializeComponent()

        mUser = user
        If user IsNot Nothing Then _
         Text = String.Format(My.Resources.BluePrismAlertsForUser0, user.Name)

        SetViewMode(Mode)
        FixProcessColumnWidths()
        PopulateCheckBoxTags()
        PopulateProcesses()
        PopulateSchedules()
        PopulateConfig()
        UpdateHistory(DateTime.Today)
        mExistingInstance = Me

        tcMain.TabPages.Remove(Me.tabPermissionDenied)

    End Sub

#End Region

#Region " Properties "

    Private ReadOnly Property CanViewHistory() As Boolean
        Get
            If Not mCanViewHistory.HasValue Then _
             mCanViewHistory = mUser.HasPermission("Subscribe to Process Alerts")
            Return mCanViewHistory.Value
        End Get
    End Property


    Private ReadOnly Property CanViewConfig() As Boolean
        Get
            If Not mCanViewConfig.HasValue Then _
             mCanViewConfig = mUser.HasPermission("Configure Process Alerts")
            Return mCanViewConfig.Value
        End Get
    End Property

    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("Configure Process Alerts")
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Disables permission checking on the form. Useful for example
    ''' if a sysadmin wants to configure the form on behalf of another
    ''' user.
    ''' </summary>
    Public Sub DisablePermissionChecking()
        mCanViewConfig = True
        mCanViewHistory = True
    End Sub

    ''' <summary>
    ''' Indicates if an frmAlertConfig form exists.
    ''' </summary>
    ''' <returns></returns>
    Public Shared ReadOnly Property InstanceExists() As Boolean
        Get
            Return (mExistingInstance IsNot Nothing _
             AndAlso Not mExistingInstance.IsDisposed)
        End Get
    End Property

    ''' <summary>
    ''' Gets a new instance or the existing instance if it exists.
    ''' </summary>
    ''' <param name="user">The user</param>
    ''' <param name="Mode">The mode</param>
    ''' <returns></returns>
    Public Shared Function GetInstance(ByVal user As User, Optional ByVal Mode As ViewMode = ViewMode.ProcessConfig) As frmAlertConfig

        If mExistingInstance Is Nothing OrElse mExistingInstance.IsDisposed OrElse mExistingInstance.Disposing Then
            Return New frmAlertConfig(user, Mode)
        Else
            If mExistingInstance.mUser.Id <> user.Id Then
                mExistingInstance.mUser = user
                mExistingInstance.PopulateProcesses()
                mExistingInstance.PopulateSchedules()
                mExistingInstance.PopulateConfig()
                mExistingInstance.UpdateHistory(DateTime.Today)
            End If
            mExistingInstance.SetViewMode(Mode)
            Return mExistingInstance
        End If

    End Function

    ''' <summary>
    ''' Sets the view mode, if available (according to user permissions etc)
    ''' </summary>
    ''' <param name="Mode">The viewmode requested.</param>
    ''' <returns>Returns true if the requested mode was made available, false
    ''' otherwise.</returns>
    Public Function SetViewMode(ByVal Mode As ViewMode, Optional ByVal SuppressMessages As Boolean = False) As Boolean
        Select Case Mode
            Case frmAlertConfig.ViewMode.History
                If CanViewHistory Then
                    Me.tcMain.SelectedTab = Me.tabHistory
                    Me.HidePermissionDeniedTab()
                    Return True
                End If
            Case Else
                If CanViewConfig Then
                    Me.tcMain.SelectedTab = Me.tabProcesses
                    Me.HidePermissionDeniedTab()
                    Return True
                End If
        End Select

        If Not (CanViewHistory OrElse CanViewConfig) Then
            Me.ShowPermissionDeniedTab()
        Else
            If Not SuppressMessages Then
                UserMessage.ShowFloating(Me, ToolTipIcon.Info, My.Resources.PermissionDenied,
                 My.Resources.TheRequestedTabCannotBeDisplayedBecauseTheCurrentUserDoesNotHavePermissionToDoSo, New Point(Me.tcMain.Left, Me.tabHistory.Top), 2000)
            End If
        End If

        Return False
    End Function

    ''' <summary>
    ''' Updates the history tab if the date given doesn't match the currently
    ''' displayed data.
    ''' </summary>
    ''' <param name="historyDate">The day to display</param>
    Public Sub UpdateHistory(historyDate As DateTime)
        Try

            If mHistoryDate = historyDate Then Return

            mHistoryDate = historyDate

            Dim dtHistory = gSv.GetAlertHistory(mUser, mHistoryDate)

            Me.gridHistory.DataSource = dtHistory
            gridHistory.Columns("Date").HeaderText = My.Resources.frmAlertConfig_Date
            gridHistory.Columns("Process").HeaderText = My.Resources.frmAlertConfig_Process
            gridHistory.Columns("Message").HeaderText = My.Resources.frmAlertConfig_Message
            gridHistory.Columns("Resource").HeaderText = My.Resources.frmAlertConfig_Resource
            gridHistory.Columns("ResourceID").HeaderText = My.Resources.frmAlertConfig_ResourceID
            gridHistory.Columns("ProcessID").HeaderText = My.Resources.frmAlertConfig_ProcessID
            gridHistory.Columns("Process").HeaderText = My.Resources.frmAlertConfig_Process
            gridHistory.Columns("Schedule").HeaderText = My.Resources.frmAlertConfig_Schedule
            gridHistory.Columns("Task").HeaderText = My.Resources.frmAlertConfig_Task
            gridHistory.Columns("Type").HeaderText = My.Resources.frmAlertConfig_Type
            gridHistory.Columns("Method").HeaderText = My.Resources.frmAlertConfig_Method

            lblDate.Text = mHistoryDate.ToLongDateString
            btnNext.Enabled = mHistoryDate < Today
            btnExport.Enabled = dtHistory.Rows.Count > 0

        Catch ex As Exception
            UserMessage.Show(
             My.Resources.AConfigurationErrorHasOccurredWhileReadingAlertHistoryDetails, ex)
        End Try
    End Sub

    ''' <summary>
    ''' Populates the tags in the checkboxes with the enum value that they represent
    ''' </summary>
    Private Sub PopulateCheckBoxTags()

        chkShowHistory.Tag = AlertNotificationType.MessageBox
        chkPopUp.Tag = AlertNotificationType.PopUp
        chkTaskbar.Tag = AlertNotificationType.Taskbar
        chkSound.Tag = AlertNotificationType.Sound

        chkScheduleShowHistory.Tag = AlertNotificationType.MessageBox
        chkSchedulePopup.Tag = AlertNotificationType.PopUp
        chkScheduleTaskbar.Tag = AlertNotificationType.Taskbar
        chkScheduleCompleted.Tag = AlertNotificationType.Sound

        chkComplete.Tag = AlertEventType.ProcessComplete
        chkFailed.Tag = AlertEventType.ProcessFailed
        chkPending.Tag = AlertEventType.ProcessPending
        chkRunning.Tag = AlertEventType.ProcessRunning
        chkStage.Tag = AlertEventType.Stage
        chkStopped.Tag = AlertEventType.ProcessStopped

        chkScheduleStarted.Tag = AlertEventType.ScheduleStarted
        chkScheduleCompleted.Tag = AlertEventType.ScheduleCompleted
        chkScheduleTerminated.Tag = AlertEventType.ScheduleTerminated

        chkTaskStarted.Tag = AlertEventType.TaskStarted
        chkTaskCompleted.Tag = AlertEventType.TaskCompleted
        chkTaskTerminated.Tag = AlertEventType.TaskTerminated

    End Sub

    ''' <summary>
    ''' Displays the current processes.
    ''' </summary>
    Private Sub PopulateProcesses()

        lvProcesses.Items.Clear()
        lvProcesses.BeginUpdate()

        Try
            Dim procDetails As DataTable = gSv.GetAlertProcessDetails(mUser.Id)
            For Each row As DataRow In procDetails.Rows
                Dim item As ListViewItem = lvProcesses.Items.Add(CStr(row("Name")))
                item.SubItems.Add(CStr(row("Description")))
                item.Checked = CBool(row("Checked"))
                item.Tag = CType(row("ProcessID"), Guid)
            Next
            lvProcesses.ListViewItemSorter = New clsListViewSorter(lvProcesses)

        Catch ex As Exception
            UserMessage.Show(
             My.Resources.AConfigurationErrorHasOccurredWhileReadingProcessDetails, ex)
        Finally
            lvProcesses.EndUpdate()
        End Try

    End Sub

    ''' <summary>
    ''' Populates the schedule listview with the current active schedules.
    ''' </summary>
    Private Sub PopulateSchedules()
        lvSchedules.Items.Clear()
        lvSchedules.BeginUpdate()
        Try
            Dim store As New DatabaseBackedScheduleStore(New InertScheduler(), gSv)
            Dim subscribedIds As ICollection(Of Integer) = gSv.GetSubscribedScheduleAlerts(mUser.Id)
            For Each sched As SessionRunnerSchedule In store.GetActiveSchedules()
                Dim item As ListViewItem = lvSchedules.Items.Add(sched.Name)
                item.SubItems.Add(sched.Description)
                item.Checked = subscribedIds.Contains(sched.Id)
                item.Tag = sched
            Next
        Catch ex As Exception
            UserMessage.Show(
             My.Resources.AConfigurationErrorHasOccurredWhileReadingScheduleDetails, ex)
        Finally
            lvSchedules.EndUpdate()
        End Try
    End Sub

    ''' <summary>
    ''' Reads the user's config details from the DB and applies them to the form
    ''' controls.
    ''' </summary>
    Private Sub PopulateConfig()
        If mUser Is Nothing Then Return

        TickBitwiseCheckboxes(gpProcessNotifMethods, mUser.AlertNotifications)
        TickBitwiseCheckboxes(gpScheduleNotifMethods, mUser.AlertNotifications)
        TickBitwiseCheckboxes(gpProcessAlertTypes, mUser.SubscribedAlerts)
        TickBitwiseCheckboxes(gpScheduleAlertTypes, mUser.SubscribedAlerts)
        TickBitwiseCheckboxes(gpTaskAlertTypes, mUser.SubscribedAlerts)

    End Sub

    ''' <summary>
    ''' Updates the user's config details.
    ''' </summary>
    ''' <remarks></remarks>
    Private Function UpdateConfig() As Boolean
        If mUser Is Nothing Then Return False

        'Get the choices from the checkboxes.
        Dim tp As AlertEventType = DirectCast(
         AccumulateBitwiseCheckboxes(gpProcessAlertTypes, gpScheduleAlertTypes, gpTaskAlertTypes),
         AlertEventType)

        Dim notifs As AlertNotificationType = DirectCast(
         AccumulateBitwiseCheckboxes(gpProcessNotifMethods, gpScheduleNotifMethods),
         AlertNotificationType)

        Try
            mUser.AlertNotifications = notifs
            mUser.SubscribedAlerts = tp

            'Check that this machine is registered in the table
            'used for counting machines (for licensing purposes)
            Dim MachineName As String = ResourceMachine.GetName()
            If Not gSv.IsAlertMachineRegistered(MachineName) Then
                'Not registered - attempt to register
                Try
                    gSv.RegisterAlertMachine(MachineName)
                Catch ex As Exception
                    UserMessage.Show(String.Format(My.Resources.FailedToRegisterForAlerts0, ex.Message), ex)
                    Return False
                End Try
            End If

            Dim procs As New List(Of Guid)
            For Each procItem As ListViewItem In lvProcesses.Items
                If procItem.Checked Then procs.Add(CType(procItem.Tag, Guid))
            Next
            Dim scheds As New List(Of Integer)
            For Each schedItem As ListViewItem In lvSchedules.Items
                If schedItem.Checked Then scheds.Add(CType(schedItem.Tag, SessionRunnerSchedule).Id)
            Next
            gSv.UpdateAlertConfig(mUser, procs, scheds)

            Return True

        Catch ex As Exception
            Return UserMessage.Err(ex,
             My.Resources.AConfigurationErrorHasOccurredWhileUpdatingUserDetails)

        End Try

    End Function

    ''' <summary>
    ''' Keeps the process listview column width proportion at about 1:2
    ''' </summary>
    Private Sub FixProcessColumnWidths()
        lvProcesses.Columns(0).Width = CInt(lvProcesses.Width / 3)
        lvProcesses.Columns(1).Width = CInt(lvProcesses.Columns(0).Width * 2 - 30)
    End Sub

    ''' <summary>
    ''' Checks that at least one process is selected.
    ''' </summary>
    ''' <returns></returns>
    Private Function ProcessesAreSelected() As Boolean

        For Each i As ListViewItem In Me.lvProcesses.Items
            If i.Checked Then
                Return True
            End If
        Next
        Return False

    End Function

    ''' <summary>
    ''' Checks that the given alert configuration data is in a valid state to be
    ''' saved, giving out appropriate error messages to the user if they are not.
    ''' </summary>
    ''' <param name="name">The name of the items being checked. This should be 
    ''' lowercase and plural, as it is used in the error messages in this way.
    ''' </param>
    ''' <param name="lview">The listview containing the items which can be checked to
    ''' indicate that alerting should occur for those items.</param>
    ''' <param name="notifPanel">The notification type panel to check.</param>
    ''' <param name="hasData">Output parameter to indicate whether any data was found
    ''' when validating the given panels. Note that this can return False even if
    ''' notification data was found, since the data could relate to a different
    ''' component.</param>
    ''' <param name="alertTypePanels">The alert type panels to check.</param>
    ''' <returns>True if the configuration provided was valid; False if any 
    ''' validation errors were found and reported to the user.</returns>
    Private Function CheckValid(ByVal name As String,
     ByVal lview As ListView, ByVal notifPanel As Control,
     ByRef hasData As Boolean,
     ByVal ParamArray alertTypePanels() As Control) As Boolean

        Dim hasCheckedItems As Boolean = AreAnyChecked(lview)
        Dim hasNotif As Boolean = AreAnyChecked(notifPanel)
        Dim hasAlertType As Boolean = False
        For Each ctl As Control In alertTypePanels
            hasAlertType = hasAlertType OrElse AreAnyChecked(ctl)
        Next
        ' Notifications could be set for processes - they are 'global', ie a 
        ' notification type set in processes will also apply in schedules even if
        ' no schedules / schedule alert types are enabled.
        hasData = hasAlertType OrElse hasCheckedItems

        ' No data at all is valid.
        If Not hasData Then Return True

        If Not hasNotif Then
            UserMessage.Show(
             My.Resources.PleaseEnsureThatYouHaveSelectedAtLeastOneNotificationMethod)
            Return False
        End If

        If Not hasAlertType Then
            UserMessage.Show(
            String.Format(My.Resources.PleaseEnsureThatYouHaveSelectedAtLeastOneAlertTypeFor0, name))
            Return False
        End If

        If Not hasCheckedItems Then
            UserMessage.Show(
             String.Format(My.Resources.PleaseEnsureThatAtLeastOneOfThe0IsChecked, name))
            Return False
        End If

        Return True

    End Function

    ''' <summary>
    ''' Saves the config to the DB.
    ''' </summary>
    ''' <returns>True if successful</returns>
    Private Function Apply() As Boolean

        ' Ensure that the current page's notifications have been echoed
        EchoNotificationChanges(tcMain.SelectedTab)

        Dim anyProcesses As Boolean = False
        If Not CheckValid(My.Resources.frmAlertConfig_Processes, lvProcesses,
         gpProcessNotifMethods, anyProcesses, gpProcessAlertTypes) Then
            Return False
        End If

        Dim anySchedules As Boolean = False
        If Not CheckValid(My.Resources.frmAlertConfig_Schedules, lvSchedules,
         gpScheduleNotifMethods, anySchedules, gpScheduleAlertTypes, gpTaskAlertTypes) Then
            Return False
        End If

        Return UpdateConfig()

    End Function

    ''' <summary>
    ''' Save's the day's history as a CSV file.
    ''' </summary>
    Private Sub ExportHistory()

        Dim sCSV As String = ""

        'Get the column headings
        For Each c As DataGridViewColumn In gridHistory.Columns
            If c.Visible Then
                sCSV &= c.HeaderText & ","
            End If
        Next
        sCSV &= vbCrLf

        'Get the data
        Dim val As Object
        For Each r As DataGridViewRow In gridHistory.Rows
            For Each c As DataGridViewColumn In gridHistory.Columns
                If c.Visible Then
                    Select Case c.Name
                        Case "Method"
                            val = GetLocalizedFriendlyName("AlertNotificationType_", CType(r.Cells("Method").Value, AlertNotificationType).ToString)
                        Case "Type"
                            val = GetLocalizedFriendlyName("AlertEventType_", CType(r.Cells("Type").Value, AlertEventType).ToString)
                        Case Else
                            val = r.Cells(c.Name).Value
                    End Select
                    sCSV &= val.ToString & ","
                End If
            Next
            sCSV &= vbCrLf
        Next

        'Open the dialogue
        dialogSave.FileName = String.Format(My.Resources.x0AlertHistory1, ApplicationProperties.ApplicationName, Format(mHistoryDate, "yyyyMMdd"))
        If dialogSave.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            Try
                System.IO.File.WriteAllText(dialogSave.FileName, sCSV, System.Text.Encoding.UTF8
                                            )
            Catch ex As Exception
                UserMessage.Show(My.Resources.AConfigurationErrorHasOccurredWhileExportingHistoryDetails, ex)
            End Try
        End If

    End Sub

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        If Not Me.tabPermissionDenied.Visible Then
            Return "frmAlertConfig.htm"
        Else
            'User permissions help reference
            Return "helpTopicsByNumber.htm#Topic1048586"
        End If
    End Function

    ''' <summary>
    ''' Returns a localised friendly name for this dependency object.
    ''' </summary>
    Public Function GetLocalizedFriendlyName(prefix As String, Attribute As String) As String
        Dim resxKey = prefix & Regex.Replace(Attribute, " ", "")
        Dim res As String = My.Resources.Resources.ResourceManager.GetString($"{resxKey}")
        Return CStr(IIf(res Is Nothing, Attribute, res))
    End Function

#Region "Events"

    ''' <summary>
    ''' Encapsulates the names of the columns
    ''' </summary>
    ''' <remarks></remarks>
    Private Class ColumnNames
        Public Shared AlertDate As String = "Date"
        Public Shared Method As String = "Method"
        Public Shared Type As String = "Type"
    End Class

    ''' <summary>
    ''' Sets up the data grid once the data has been attached.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleDataBindingComplete(
     ByVal sender As Object, ByVal e As DataGridViewBindingCompleteEventArgs) _
     Handles gridHistory.DataBindingComplete

        If DesignMode Then Exit Sub

        'Probably don't need to do this, but just in case.
        RemoveHandler gridHistory.DataBindingComplete, AddressOf HandleDataBindingComplete

        mDateColumnIndex = gridHistory.Columns(ColumnNames.AlertDate).Index
        mMethodColumnIndex = gridHistory.Columns(ColumnNames.Method).Index
        mTypeColumnIndex = gridHistory.Columns(ColumnNames.Type).Index

        'Start handling the cell formatting.
        AddHandler gridHistory.CellFormatting, AddressOf HandleCellFormatting

    End Sub

    ''' <summary>
    ''' Formats the cells displayed on screen.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleCellFormatting(
     ByVal sender As Object, ByVal e As DataGridViewCellFormattingEventArgs)

        If DesignMode Then Exit Sub

        Select Case e.ColumnIndex
            Case mDateColumnIndex
                e.Value = CDate(e.Value).ToLocalTime
            Case mMethodColumnIndex
                e.Value = GetLocalizedFriendlyName("AlertNotificationType_", CType(e.Value, AlertNotificationType).ToString())
            Case mTypeColumnIndex
                e.Value = GetLocalizedFriendlyName("AlertEventType_", CType(e.Value, AlertEventType).ToString())
        End Select

    End Sub

    ''' <summary>
    ''' Update without closing.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleApplyClicked(ByVal sender As System.Object, ByVal e As System.EventArgs) _
     Handles btnApply.Click
        Apply()
    End Sub

    ''' <summary>
    ''' Update and close.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleOkClicked(ByVal sender As System.Object, ByVal e As System.EventArgs) _
     Handles btnOk.Click
        If Apply() Then Close()
    End Sub

    ''' <summary>
    ''' Close without updating.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleCancelClicked(ByVal sender As System.Object, ByVal e As System.EventArgs) _
     Handles btnCancel.Click
        Close()
    End Sub

    ''' <summary>
    ''' Show the previous day's history.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandlePrevClicked(ByVal sender As System.Object, ByVal e As System.EventArgs) _
     Handles btnPrevious.Click
        UpdateHistory(mHistoryDate.AddDays(-1))
    End Sub

    ''' <summary>
    ''' Show the next day's history.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleNextClicked(ByVal sender As System.Object, ByVal e As System.EventArgs) _
     Handles btnNext.Click
        UpdateHistory(mHistoryDate.AddDays(1))
    End Sub

    ''' <summary>
    ''' Save's the day's history as a CSV file.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleExportClicked(ByVal sender As System.Object, ByVal e As System.EventArgs) _
     Handles btnExport.Click
        ExportHistory()
    End Sub

#Region "Checkbox Utility methods"

    ''' <summary>
    ''' Checks if any of the configuration checkboxes are checked for the given view
    ''' mode. This only has meaning for the <see cref="ViewMode.ProcessConfig"/>
    ''' and <see cref="ViewMode.SchedulerConfig"/> modes.
    ''' </summary>
    ''' <param name="view">The view mode to check to see if any configuration has
    ''' been enabled.</param>
    ''' <returns>True if any checkboxes, not including the listviews which list the
    ''' processes and schedules, have been checked for the specified view. False if
    ''' the view is something other than process or scheduler config, or if not
    ''' checked checkboxes were found.</returns>
    Public Function AreAnyCheckedByViewMode(ByVal view As ViewMode) As Boolean
        Select Case view
            Case ViewMode.ProcessConfig
                Return AreAnyChecked(tabProcesses)
            Case ViewMode.SchedulerConfig
                Return AreAnyChecked(tabSchedules)
            Case Else
                Return False
        End Select
    End Function

    ''' <summary>
    ''' Checks the list view items on the given listview to see if any of them are
    ''' checked.
    ''' </summary>
    ''' <param name="lv">The listview to check to see if any of its items are
    ''' checked. A null value is allowed and is treated as being a listview with
    ''' no items on it.</param>
    ''' <returns></returns>
    Private Function AreAnyChecked(ByVal lv As ListView) As Boolean
        If lv Is Nothing Then Return False
        For Each item As ListViewItem In lv.Items
            If item.Checked Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' Checks if there are any checkboxes checked in the given control or any nested
    ''' controls. This will <em>not</em> check any list views found on the container.
    ''' </summary>
    ''' <param name="container">The container control to check for the state of any
    ''' checkboxes.</param>
    ''' <returns>True if any of the found checkboxes were set checked, False if
    ''' either no checkboxes were found or any found were unchecked.</returns>
    Private Function AreAnyChecked(ByVal container As Control) As Boolean
        Return AreAnyChecked(container, False)
    End Function

    ''' <summary>
    ''' Checks if there are any checkboxes checked in the given control or any nested
    ''' controls. This will also check the checked value of listview items on any
    ''' listviews found, depending on the value of the
    ''' <paramref name="includeListViews"/> parameter.
    ''' </summary>
    ''' <param name="container">The container control to check for the state of any
    ''' checkboxes.</param>
    ''' <param name="includeListViews">True to also check the checked state of any
    ''' list view items in any listviews found within the container.</param>
    ''' <returns>True if any of the found checkboxes were set checked, False if
    ''' either no checkboxes were found or any found were unchecked.</returns>
    Private Function AreAnyChecked(ByVal container As Control, ByVal includeListViews As Boolean) _
     As Boolean

        For Each ctl As Control In container.Controls

            ' If it's a checkbox, check its value
            Dim cb As CheckBox = TryCast(ctl, CheckBox)
            If cb IsNot Nothing AndAlso cb.Checked Then Return True

            ' if it's a listview and we're including them, see if any of those are checked.
            If includeListViews AndAlso AreAnyChecked(TryCast(ctl, ListView)) Then
                Return True
            End If

            ' If it's a nested container, see if any checkboxes are checked on there.
            If ctl.Controls.Count > 0 AndAlso AreAnyChecked(ctl, includeListViews) Then
                Return True
            End If

        Next

        ' We've got through all controls without finding a checked checkbox
        Return False

    End Function

    ''' <summary>
    ''' Sets the checked value of all the checkboxes on the given container, or any
    ''' nested containers or listviews.
    ''' </summary>
    ''' <param name="container">The container on which all the checkboxes should be
    ''' set.</param>
    ''' <param name="checked">True to set all the checkboxes found to be checked;
    ''' False to set them all to be unchecked.</param>
    Private Sub CheckAllBoxes(ByVal container As Control, ByVal checked As Boolean)
        For Each ctl As Control In container.Controls
            ' If it's a checkbox, set its value and move onto the next control
            Dim cb As CheckBox = TryCast(ctl, CheckBox)
            If cb IsNot Nothing Then
                cb.Checked = checked
                Continue For
            End If

            ' If it's a listview check/uncheck each item therein.
            Dim lv As ListView = TryCast(ctl, ListView)
            If lv IsNot Nothing Then
                For Each item As ListViewItem In lv.Items
                    item.Checked = checked
                Next
                Continue For
            End If

            ' If it's a nested container, recurse into that to deal with any
            ' checkboxes on there.
            If ctl.Controls.Count > 0 Then
                CheckAllBoxes(ctl, checked)
                Continue For
            End If

            ' Not a checkbox, and not a nested control - ignore it and move on.

        Next

    End Sub

    ''' <summary>
    ''' Goes through all checkboxes in the given container and any nested
    ''' containers and examines their tag values. If their tags are integers,
    ''' then they are bitwise compared to the given value and checked if there
    ''' is a bit match, and unchecked if not.
    ''' </summary>
    ''' <param name="container">The container to check the checkboxes within
    ''' for a bitmask which corresponds to the given value.</param>
    ''' <param name="value">The value to compare against the bitmasks in the
    ''' checkboxes to determine their state.</param>
    Private Sub TickBitwiseCheckboxes(ByVal container As Control, ByVal value As Integer)
        For Each ctl As Control In container.Controls

            ' If it's a checkbox, set its value and move onto the next control
            Dim cb As CheckBox = TryCast(ctl, CheckBox)
            If cb IsNot Nothing Then
                ' Only affect the state if its tag is an integer
                Try
                    Dim t As Object = cb.Tag
                    If t IsNot Nothing AndAlso Type.GetTypeCode(t.GetType()) = TypeCode.Int32 Then
                        cb.Checked = (DirectCast(t, Integer) And value) <> 0
                    End If

                Catch ex As Exception

                End Try
                Continue For
            End If

            ' If it's a nested container, recurse into that to deal with any
            ' checkboxes on there.
            If ctl.Controls.Count > 0 Then
                TickBitwiseCheckboxes(ctl, value)
                Continue For
            End If

        Next
    End Sub

    ''' <summary>
    ''' Goes through the checkboxes in the given container and ORs together any
    ''' it finds with an Integer tag, returning the result.
    ''' </summary>
    ''' <param name="containers">The containers on which the required checkboxes
    ''' reside.</param>
    ''' <returns>The ORed together values from the checkboxes with integer Tag
    ''' values found within the given container.</returns>
    Private Function AccumulateBitwiseCheckboxes(
     ByVal ParamArray containers() As Control) As Integer
        Dim accum As Integer = 0
        For Each cont As Control In containers
            accum = AccumulateBitwiseCheckboxes(accum, cont)
        Next
        Return accum
    End Function

    ''' <summary>
    ''' Goes through the checkboxes in the given container and ORs together any
    ''' it finds with an Integer tag, returning the result. The given 
    ''' accumulator is used as a starting value for the ORing of the tags.
    ''' </summary>
    ''' <param name="container">The container on which the required checboxes
    ''' reside.</param>
    ''' <param name="accum">The accumulator - the initial value to use for this
    ''' iteration of the method.</param>
    ''' <returns>The ORed together values from the checkboxes with integer Tag
    ''' values found within the given container.</returns>
    Private Function AccumulateBitwiseCheckboxes(ByVal accum As Integer,
     ByVal container As Control) As Integer

        For Each ctl As Control In container.Controls

            ' If it's a checkbox, set its value and move onto the next control
            Dim cb As CheckBox = TryCast(ctl, CheckBox)
            ' We apply it only if the box is checked - we don't mask it if it
            ' is unchecked.
            If cb IsNot Nothing And cb.Checked Then
                ' If the tag is an integer, OR it to the accumulator
                Dim t As Object = cb.Tag
                If t IsNot Nothing AndAlso Type.GetTypeCode(t.GetType()) = TypeCode.Int32 Then
                    accum = (accum Or DirectCast(t, Integer))
                End If
                Continue For
            End If

            ' If it's a nested container, recurse into that to deal with any
            ' checkboxes on there.
            If ctl.Controls.Count > 0 Then
                accum = AccumulateBitwiseCheckboxes(accum, ctl)
                Continue For
            End If

        Next
        Return accum

    End Function



#End Region

    ''' <summary>
    ''' Clears all check boxes
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleClearClicked(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnClearProcesses.Click, btnClearSchedules.Click
        CheckAllBoxes(clsUserInterfaceUtils.GetAncestor(Of TabPage)(CType(sender, Control)), False)
    End Sub

    Private Sub HandleHelpClicked(ByVal sender As System.Object, ByVal e As System.EventArgs) _
     Handles btnHelp.Click
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    ''' <summary>
    ''' Paints a border because DataGridView comes in black.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleHistoryTabPaint(ByVal sender As Object, ByVal e As PaintEventArgs) ' Handles tabHistory.Paint

        e.Graphics.DrawRectangle(New Pen(System.Drawing.SystemColors.ActiveBorder),
         gridHistory.Location.X - 1,
         gridHistory.Location.Y,
         gridHistory.Width + 1,
         gridHistory.Height)

    End Sub

#End Region

    Private Sub ShowPermissionDeniedTab()
        'Show the permission denied tab
        If Not tcMain.TabPages.Contains(Me.tabPermissionDenied) Then
            tcMain.TabPages.Add(Me.tabPermissionDenied)
        End If

        tcMain.SelectedTab = tabPermissionDenied
        tcMain.Enabled = False
    End Sub

    Private Sub HidePermissionDeniedTab()
        tcMain.TabPages.Remove(Me.tabPermissionDenied)
        tcMain.Enabled = True
    End Sub

    Private Function IsConfigTab(ByVal tab As TabPage) As Boolean
        Return (tab Is tabProcesses OrElse tab Is tabSchedules)
    End Function

    Public Function IsConfigTabSelected() As Boolean
        Return IsConfigTab(tcMain.SelectedTab)
    End Function

    Private Sub frmAlertConfig_Load(ByVal sender As Object, ByVal e As System.EventArgs) _
     Handles Me.Load
        If Not CanViewConfig AndAlso IsConfigTabSelected() Then
            SetViewMode(ViewMode.History)
        End If
    End Sub


    Private Sub HandleTabSelecting(ByVal sender As Object, ByVal e As TabControlCancelEventArgs) _
     Handles tcMain.Selecting

        ' If current user don't have the rights to view the selecting tab, cancel and show
        ' a tooltippy message
        If (Not CanViewHistory AndAlso e.TabPage Is tabHistory) OrElse
         (Not CanViewConfig AndAlso IsConfigTab(e.TabPage)) Then

            e.Cancel = True

            ' show message for 2 secs - left aligned and vertically centred with this form.
            Dim ypos As Integer = PointToClient(tabHistory.PointToScreen(tabHistory.Location)).Y
            UserMessage.ShowFloating(Me, ToolTipIcon.Info, My.Resources.PermissionDenied,
             My.Resources.TheRequestedTabCannotBeDisplayedBecauseTheCurrentUserDoesNotHaveTheAppropriateP,
             New Point(tcMain.Left, ypos), 2000)

        End If

    End Sub

    ''' <summary>
    ''' The 'notification methods' are global - ie. change it for schedules and
    ''' the change is echoed to processes and vice versa.
    ''' This handles the tab changing and ensures the echo is done when the tab
    ''' is deselected
    ''' </summary>
    Private Sub HandleTabChanging( _
     ByVal sender As Object, ByVal e As TabControlCancelEventArgs) Handles tcMain.Deselecting
        EchoNotificationChanges(e.TabPage)
    End Sub

    ''' <summary>
    ''' The 'notification methods' are global - ie. change it for schedules and
    ''' the change is echoed to processes and vice versa.
    ''' This echoes the values held in the given tab page across any other relevant
    ''' tab pages.
    ''' </summary>
    ''' <param name="source">The tab page from which the notification values should
    ''' be taken. These values are echoed across any other tab pages which hold the
    ''' notification values.
    ''' If the given page doesn't hold notification values it is ignored.
    ''' </param>
    Private Sub EchoNotificationChanges(ByVal source As TabPage)
        If Object.ReferenceEquals(source, tabProcesses) Then
            chkSchedulePlaySound.Checked = chkSound.Checked
            chkSchedulePopup.Checked = chkPopUp.Checked
            chkScheduleShowHistory.Checked = chkShowHistory.Checked
            chkScheduleTaskbar.Checked = chkTaskbar.Checked
        ElseIf Object.ReferenceEquals(source, tabSchedules) Then
            chkSound.Checked = chkSchedulePlaySound.Checked
            chkPopUp.Checked = chkSchedulePopup.Checked
            chkShowHistory.Checked = chkScheduleShowHistory.Checked
            chkTaskbar.Checked = chkScheduleTaskbar.Checked
        End If
    End Sub

#End Region

End Class
