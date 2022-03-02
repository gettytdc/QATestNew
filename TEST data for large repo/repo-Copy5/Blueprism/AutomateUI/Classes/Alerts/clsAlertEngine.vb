
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages

Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections

Imports AlertNotificationType = BluePrism.AutomateAppCore.AlertNotificationType
Imports BluePrism.Server.Domain.Models

''' <summary>
''' This class is used to periodically poll the database for alerts and display them
''' according to the user's alert configuration.
''' 
''' It can also write alert records to the DB by handling alert events raised when
''' process sessions run.
''' </summary>
Public Class clsAlertEngine : Implements IDisposable

#Region "Members"

    ''' <summary>
    ''' The resource id.
    ''' </summary>
    Private mResourceID As Guid

    ' The user that the alert engine was initialised with
    Private mUser As User

    ''' <summary>
    ''' The DB poll timer.
    ''' </summary>
    Private WithEvents mPollDatabaseTimer As System.Windows.Forms.Timer

    ''' <summary>
    ''' The pop up form.
    ''' </summary>
    Private WithEvents mAlertPopUp As frmAlertPopUp

    ''' <summary>
    ''' The alert config form.
    ''' </summary>
    Private mAlertConfig As frmAlertConfig

    Public ReadOnly Property ConfigForm() As frmAlertConfig
        Get
            Return mAlertConfig
        End Get
    End Property

    ''' <summary>
    ''' The automate form.
    ''' </summary>
    Private WithEvents mApplication As frmApplication

    ''' <summary>
    ''' The alert monitor form.
    ''' </summary>
    Private mAlertMonitor As frmAlertMonitor

    ''' <summary>
    ''' A flag indicating that the process alert handler is in place.
    ''' </summary>
    Private mIsHandlingStageAlerts As Boolean

    ''' <summary>
    ''' A context menu item
    ''' </summary>
    Private mClearMenuItem As ToolStripMenuItem

    ''' <summary>
    ''' Used in a SQL IN clause to ignore alerts from particular sessions.
    ''' </summary>
    Private mSessionsToIgnore As ICollection(Of Guid)

    ''' <summary>
    ''' Used in the stage alert handler to bypass any stages that have
    ''' produced errors in the current session.
    ''' </summary>
    Private mAlertStagesToIgnore As Dictionary(Of Guid, List(Of clsAlertStage))

#End Region

    ''' <summary>
    ''' The user of this alert engine. This is lazily populated and then never
    ''' updated, meaning that if the user's details are changed elsewhere, they will
    ''' not be reflected in the object returned by this property.
    ''' </summary>
    Public ReadOnly Property AlertsUser() As User
        Get
            Return mUser
        End Get
    End Property

#Region " Constructors "

    ''' <summary>
    ''' Used in 'monitor only/command line' mode.
    ''' </summary>
    ''' <param name="monitor">The monitor form</param>
    ''' <param name="user">The user</param>
    ''' <param name="resourceName">The resource</param>
    Public Sub New(ByVal monitor As frmAlertMonitor, _
     ByVal user As User, ByVal resourceName As String)

        mAlertMonitor = monitor

        mUser = user
        mResourceID = gSv.GetResourceId(resourceName)

        mSessionsToIgnore = New clsSet(Of Guid)

        StartPollingDatabase()

    End Sub

    ''' <summary>
    ''' Used to start reading process alerts from the DB.
    ''' </summary>
    ''' <param name="appForm">The application form</param>
    ''' <param name="user">The user</param>
    ''' <param name="resourceName">The resource</param>
    Public Sub New(ByVal appForm As frmApplication, _
     ByVal user As User, ByVal resourceName As String)

        mApplication = appForm
        mUser = user
        mResourceID = gSv.GetResourceId(resourceName)

        SetAlertNotifyIcon(appForm)

        Try
            appForm.AlertNotifyIcon.Visible = mUser.IsAlertSubscriber
            StartPollingDatabase()

        Catch ex As Exception
            UserMessage.Err(ex,
             My.Resources.BluePrismAlertsHasEncounteredAProblemAndCannotStart)

        End Try

    End Sub

    ''' <summary>
    ''' Used to start writing process alerts to the DB.
    ''' </summary>
    ''' <param name="username">The user</param>
    ''' <param name="resourceName">The resource</param>
    Public Sub New(ByVal username As String, ByVal resourceName As String)

        Try
            ' Ignore empty usernames / missing users - this constructor can be called
            ' with no username (public resource PC), so just allow it through
            If username <> "" Then mUser = User.GetUser(username)
        Catch nsee As NoSuchElementException
        End Try
        mResourceID = gSv.GetResourceId(resourceName)

        StartHandlingStageAlerts()

    End Sub

#End Region

#Region "Dispose"

    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                If mPollDatabaseTimer IsNot Nothing Then mPollDatabaseTimer.Stop()
                RemoveHandler clsProcess.StageAlert, AddressOf Me.StageAlertHandler
                frmAlertPopUp.ClearAllAlerts()
            End If
        End If
        Me.disposedValue = True
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region


    ''' <summary>
    ''' Starts writing any stage alerts to the DB.
    ''' </summary>
    Public Sub StartHandlingStageAlerts()

        If Not mIsHandlingStageAlerts Then
            AddHandler clsProcess.StageAlert, AddressOf Me.StageAlertHandler
            mIsHandlingStageAlerts = True
        End If

    End Sub

    ''' <summary>
    ''' Checks if the user is monitoring alerts.
    ''' </summary>
    ''' <returns></returns>
    Public Function UserIsAlertSubscriber() As Boolean
        If mUser Is Nothing Then Return False
        Try
            Return mUser.IsAlertSubscriber()
        Catch
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Displays the history form.
    ''' </summary>
    Public Sub ShowHistory()

        If mAlertConfig Is Nothing OrElse mAlertConfig.IsDisposed Then
            mAlertConfig = frmAlertConfig.GetInstance(AlertsUser, frmAlertConfig.ViewMode.History)
            mAlertConfig.Show()
        Else
            mAlertConfig.UpdateHistory(DateTime.Today)
            mAlertConfig.SetViewMode(frmAlertConfig.ViewMode.History)
        End If
        mAlertConfig.BringToFront()

    End Sub

    ''' <summary>
    ''' Displays the config form.
    ''' </summary>
    Public Sub ShowConfig()

        If mAlertConfig Is Nothing OrElse mAlertConfig.IsDisposed Then
            mAlertConfig = frmAlertConfig.GetInstance(AlertsUser, frmAlertConfig.ViewMode.ProcessConfig)
            mAlertConfig.Show()
        Else
            mAlertConfig.SetViewMode(frmAlertConfig.ViewMode.ProcessConfig)
        End If
        mAlertConfig.BringToFront()

    End Sub

    ''' <summary>
    ''' Stores a session ID which should be ignored in future retrievals of alerts
    ''' </summary>
    Public Sub IgnoreSession(ByVal SessionID As Guid)
        mSessionsToIgnore.Add(SessionID)
    End Sub



    ''' <summary>
    ''' Writes an alert to the DB.
    ''' </summary>
    ''' <param name="Stage">The stage</param>
    ''' <param name="Message">The stage message</param>
    Private Sub StageAlertHandler(ByVal Stage As clsAlertStage, ByVal Message As String)

        If Stage Is Nothing Then
            Exit Sub
        End If

        Dim sMessage As String
        Dim gProcessID, gSessionID As Guid
        Dim parentProcess As clsProcess = Nothing


        sMessage = Stage.GetName & My.Resources.EqualSign & Message

        'Find the top level process.
        parentProcess = Stage.Process
        While parentProcess IsNot Nothing
            If parentProcess.ParentProcess Is Nothing Then
                Exit While
            Else
                parentProcess = parentProcess.ParentProcess
            End If
        End While
        gProcessID = gSv.GetProcessIDByName(parentProcess.Name)    'Why can't it just use clsProcess.GetProcessID()!
        gSessionID = parentProcess.Session.ID

        If mAlertStagesToIgnore IsNot Nothing Then
            If mAlertStagesToIgnore.ContainsKey(gSessionID) Then
                If mAlertStagesToIgnore(gSessionID).Contains(Stage) Then
                    'This stage has failed before in this session, so ignore it.
                    Exit Sub
                End If
            End If
        End If

        Try
            gSv.CreateStageAlert(gSessionID, sMessage)
        Catch ex As Exception

            Dim err As String = ex.Message
            'CreateAlerts has failed for some reason, so ignore this stage in this session.
            If mAlertStagesToIgnore Is Nothing Then
                mAlertStagesToIgnore = New Dictionary(Of Guid, List(Of clsAlertStage))
            End If
            If Not mAlertStagesToIgnore.ContainsKey(gSessionID) Then
                mAlertStagesToIgnore.Add(gSessionID, New List(Of clsAlertStage))
            End If
            If Not mAlertStagesToIgnore(gSessionID).Contains(Stage) Then
                mAlertStagesToIgnore(gSessionID).Add(Stage)
            End If

            UserMessage.Show(String.Format(My.Resources.BluePrismAlertsHasEncounteredAProblemWithTheStage0InProcess1AndWillNotReportAny, Stage.GetName, Stage.Process.Name, err))

        End Try

    End Sub

    ''' <summary>
    ''' Starts the polling timer.
    ''' </summary>
    Private Sub StartPollingDatabase()

        Try
            gSv.ClearOldAlerts(mUser.Id)
            mPollDatabaseTimer = New System.Windows.Forms.Timer()
            mPollDatabaseTimer.Interval = 5000
            mPollDatabaseTimer.Enabled = True
            mPollDatabaseTimer.Start()
        Catch ex As Exception
            UserMessage.Show(My.Resources.BluePrismAlertsHasEncounteredAProblemAndCannotStart, ex)
        End Try

    End Sub


    ''' <summary>
    ''' The timer tick event handler.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub PollDatabaseForAlerts(ByVal sender As Object, ByVal e As System.EventArgs) Handles mPollDatabaseTimer.Tick

        'Stop handling tick events.
        RemoveHandler mPollDatabaseTimer.Tick, AddressOf PollDatabaseForAlerts


        Dim aPopUpAlerts As New List(Of Alert)
        Dim aMessageBoxAlerts As New List(Of Alert)
        Dim aTaskBarAlerts As New List(Of Alert)
        Dim aSoundAlerts As New List(Of Alert)

        Dim al As Alert = Nothing

        Try

            If mUser.IsAlertSubscriber() Then
                'The user is monitoring alerts.
                If mApplication IsNot Nothing Then _
                 mApplication.AlertNotifyIcon.Visible = True

            Else
                'The user is not monitoring alerts.
                If mApplication IsNot Nothing Then _
                 mApplication.AlertNotifyIcon.Visible = False

                'Clear any pop up alerts waiting.
                If mAlertPopUp IsNot Nothing Then mAlertPopUp.ClearAlerts()


                'Replace the tick handler and bail out.
                AddHandler mPollDatabaseTimer.Tick, AddressOf PollDatabaseForAlerts
                Exit Sub

            End If

            Dim dt As DataTable =
             gSv.UpdateAndAcknowledgeAlerts(mSessionsToIgnore, mResourceID, mUser)

            'Collect the alert information
            If Not dt Is Nothing Then
                For Each r As DataRow In dt.Rows
                    Dim prov As New DataRowDataProvider(r)

                    al = New Alert()
                    al.Message = prov.GetValue("Message", "")
                    al.SessionID = prov.GetValue("SessionID", Guid.Empty)
                    al.Process = prov.GetValue("ProcessName", "")
                    al.ProcessID = prov.GetValue("ProcessID", Guid.Empty)
                    al.Resource = prov.GetValue("ResourceName", "")
                    al.ResourceID = prov.GetValue("ResourceID", Guid.Empty)
                    al.ScheduleName = prov.GetValue("ScheduleName", "")
                    al.TaskName = prov.GetValue("TaskName", "")
                    al.AlertDate = prov.GetValue("Date", Date.MinValue).ToLocalTime()

                    Select Case prov.GetValue("AlertNotificationType", AlertNotificationType.None)

                        Case AlertNotificationType.PopUp
                            aPopUpAlerts.Add(al)

                        Case AlertNotificationType.MessageBox
                            aMessageBoxAlerts.Add(al)

                        Case AlertNotificationType.Taskbar
                            aTaskBarAlerts.Add(al)

                        Case AlertNotificationType.Sound
                            aSoundAlerts.Add(al)

                    End Select
                Next
            End If


        Catch ex As Exception
            mPollDatabaseTimer.Stop()
            UserMessage.Show(My.Resources.BluePrismAlertsHasEncounteredAProblemAndCannotContinue, ex)

        End Try

        If al IsNot Nothing Then
            'Use the last alert found in the taskbar.
            DoTaskBarAlert(al)

            'Refresh the history form if it is on display.
            If mAlertConfig IsNot Nothing AndAlso Not mAlertConfig.IsDisposed Then
                mAlertConfig.UpdateHistory(DateTime.Today)
            End If

        End If

        DoPopUpAlerts(aPopUpAlerts)
        DoMessageBoxAlerts(aMessageBoxAlerts)
        DoSoundAlerts(aSoundAlerts)

        'Start handling tick events again.
        AddHandler mPollDatabaseTimer.Tick, AddressOf PollDatabaseForAlerts

    End Sub

    ''' <summary>
    ''' Executes sound alerts
    ''' </summary>
    ''' <param name="aSoundAlerts">The alerts</param>
    Private Sub DoSoundAlerts(ByVal aSoundAlerts As List(Of clsAlertEngine.Alert))

        If aSoundAlerts.Count > 0 Then
            System.Media.SystemSounds.Exclamation.Play()
        End If

    End Sub

    ''' <summary>
    ''' Executes pop up alerts.
    ''' </summary>
    ''' <param name="aPopUpAlerts">The alerts</param>
    Private Sub DoPopUpAlerts(ByVal aPopUpAlerts As List(Of clsAlertEngine.Alert))

        'Display any pop up alerts found.
        If aPopUpAlerts.Count > 0 Then
            If mAlertPopUp Is Nothing OrElse mAlertPopUp.IsDisposed Then
                mAlertPopUp = New frmAlertPopUp(Me, aPopUpAlerts)
                mAlertPopUp.Show()
            Else
                mAlertPopUp.AddAlerts(aPopUpAlerts)
            End If
        End If

    End Sub

    ''' <summary>
    ''' Executes message box alerts
    ''' </summary>
    ''' <param name="aMessageBoxAlerts">The alerts</param>
    Private Sub DoMessageBoxAlerts(ByVal aMessageBoxAlerts As List(Of clsAlertEngine.Alert))

        'Display the alert history form for any message alerts found.
        If aMessageBoxAlerts.Count > 0 Then
            If mAlertConfig Is Nothing OrElse mAlertConfig.IsDisposed Then
                mAlertConfig = frmAlertConfig.GetInstance(AlertsUser, frmAlertConfig.ViewMode.History)
                mAlertConfig.Show()
            Else
                mAlertConfig.UpdateHistory(DateTime.Today)
            End If
        End If

    End Sub

    ''' <summary>
    ''' Displays the specified alert in a popup box near the taskbar
    ''' </summary>
    ''' <param name="al">The alert to display</param>
    Private Sub DoTaskBarAlert(ByVal al As Alert)


        If mApplication IsNot Nothing Then
            mClearMenuItem.Enabled = True
            mApplication.AlertNotifyIcon.Visible = True
            mApplication.AlertNotifyIcon.Icon = mApplication.AlertNotifyIcon2.Icon

            mApplication.AlertNotifyIcon.Text = al.GetLongMessage()

        ElseIf mAlertMonitor IsNot Nothing Then
            mAlertMonitor.Notify(al)

        End If

    End Sub

    ''' <summary>
    ''' Activates the automate taskbar icon.
    ''' </summary>
    ''' <param name="app">The Application form to use to host the context menu.
    ''' </param>
    Private Sub SetAlertNotifyIcon(ByVal app As frmApplication)

        If app Is Nothing Then Return

        Dim menu As New ContextMenuStrip()

        app.AlertNotifyIcon.ContextMenuStrip = menu
        app.AlertNotifyIcon.Tag = app.AlertNotifyIcon.Icon
        app.AlertNotifyIcon.Text = String.Format(My.Resources.App0ProcessAlertMonitor, ApplicationProperties.ApplicationName)

        mClearMenuItem = New ToolStripMenuItem(My.Resources.ClearAlert, Nothing, AddressOf Menu_Clear)
        mClearMenuItem.Enabled = False
        menu.Items.Add(mClearMenuItem)
        Dim histItem As New ToolStripMenuItem(My.Resources.ViewHistory, Nothing, AddressOf Menu_History)
        histItem.Enabled = AlertsUser.HasPermission("Subscribe to Process Alerts")
        menu.Items.Add(histItem)
        Dim configItem As New ToolStripMenuItem(My.Resources.ConfigureAlerts, Nothing, AddressOf Menu_Config)
        configItem.Enabled = AlertsUser.HasPermission("Configure Process Alerts")
        menu.Items.Add(configItem)

    End Sub


    ''' <summary>
    ''' Clears the current taskbar alert.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Menu_Clear(ByVal sender As System.Object, ByVal e As System.EventArgs)

        mApplication.AlertNotifyIcon.Icon = CType(mApplication.AlertNotifyIcon.Tag, Icon)
        mApplication.AlertNotifyIcon.Text = ""
        mClearMenuItem.Enabled = False

    End Sub

    ''' <summary>
    ''' Displays the config form.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Menu_Config(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim bShowDialog As Boolean

        If mAlertConfig Is Nothing OrElse mAlertConfig.IsDisposed OrElse mAlertConfig.Disposing Then
            If frmAlertConfig.InstanceExists Then
                'The config form is already open
                mAlertConfig = frmAlertConfig.GetInstance(AlertsUser, frmAlertConfig.ViewMode.ProcessConfig)
            Else
                'Open a new config form
                mAlertConfig = New frmAlertConfig(AlertsUser, frmAlertConfig.ViewMode.ProcessConfig)
                bShowDialog = True
            End If
        Else
            'The config form is already open
            mAlertConfig.SetViewMode(frmAlertConfig.ViewMode.ProcessConfig)
        End If

        If bShowDialog Then
            mAlertConfig.ShowInTaskbar = False
            mAlertConfig.SetViewMode(frmAlertConfig.ViewMode.ProcessConfig)
            mAlertConfig.ShowDialog()
            mAlertConfig.Dispose()
            mAlertConfig = Nothing
        Else
            mAlertConfig.Visible = True
            If mAlertConfig.WindowState = FormWindowState.Minimized Then
                mAlertConfig.WindowState = FormWindowState.Normal
            End If
        End If

    End Sub

    ''' <summary>
    ''' Displays the config form in history mode.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Menu_History(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim bShowDialog As Boolean

        If mAlertConfig Is Nothing OrElse mAlertConfig.IsDisposed OrElse mAlertConfig.Disposing Then
            If frmAlertConfig.InstanceExists Then
                'The config form is already open
                mAlertConfig = frmAlertConfig.GetInstance(AlertsUser, frmAlertConfig.ViewMode.History)
            Else
                'Open a new config form
                mAlertConfig = New frmAlertConfig(AlertsUser, frmAlertConfig.ViewMode.History)
                bShowDialog = True
            End If
        Else
            'The config form is already open
            mAlertConfig.SetViewMode(frmAlertConfig.ViewMode.History)
        End If

        If bShowDialog Then
            mAlertConfig.SetViewMode(frmAlertConfig.ViewMode.History)
            mAlertConfig.ShowInTaskbar = False
            mAlertConfig.ShowDialog()
            mAlertConfig.Dispose()
            mAlertConfig = Nothing
        Else
            mAlertConfig.Visible = True
            If mAlertConfig.WindowState = FormWindowState.Minimized Then
                mAlertConfig.WindowState = FormWindowState.Normal
            End If
        End If

    End Sub


    ''' <summary>
    ''' Closes the config form when Automate closes.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Application_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) _
     Handles mApplication.FormClosed

        If Not mAlertConfig Is Nothing Then
            mAlertConfig.Close()
        End If

    End Sub



    ''' <summary>
    ''' A simple class to hold alert details.
    ''' </summary>
    Public Class Alert
        Public Message As String
        Public Process As String
        Public ProcessID As Guid
        Public SessionID As Guid
        Public Resource As String
        Public ResourceID As Guid
        Public AlertDate As Date
        Public ScheduleName As String
        Public TaskName As String

        ''' <summary>
        ''' Gets a short message for this alert
        ''' </summary>
        ''' <returns>A short message suitable for displaying in popup which shows the
        ''' alert detail from this object.</returns>
        Public Function GetShortMessage() As String
            Dim sb As New StringBuilder()
            If TaskName <> "" Then
                sb.AppendFormat(
                 My.Resources.Date3HHMmSsTask1On200,
                 vbCrLf, TaskName, ScheduleName, AlertDate)
            ElseIf ScheduleName <> "" Then
                sb.AppendFormat(
                 My.Resources.Date2HHMmSsSchedule100,
                 vbCrLf, ScheduleName, AlertDate)
            Else
                sb.AppendFormat(My.Resources.Date3HHMmSs1On200,
                 vbCrLf, Process, Resource, AlertDate)
            End If

            Return sb.Append(Message).ToString()

        End Function

        ''' <summary>
        ''' Gets a long message for this alert
        ''' </summary>
        ''' <returns>A longer message which displays the same information as the
        ''' short message but with more verbosity.</returns>
        Public Function GetLongMessage() As String

            Dim sb As New StringBuilder()
            If TaskName <> "" Then
                sb.AppendFormat(
                 My.Resources.ScheduleAlertRegardingTheTask1OnSchedule2At3HHMmSs,
                 vbCrLf, TaskName, ScheduleName, AlertDate)
            ElseIf ScheduleName <> "" Then
                sb.AppendFormat(My.Resources.ScheduleAlertRegardingTheSchedule10At2HHMmSs,
                 vbCrLf, ScheduleName, AlertDate)
            Else
                sb.AppendFormat(My.Resources.ProcessAlertFrom203HHMmSs10,
                 vbCrLf, Process, Resource, AlertDate)
            End If
            sb.Append(Message)
            ' Truncate if too long
            If sb.Length >= 64 Then
                sb.Length = 60
                sb.Append(My.Resources.Ellipsis)
            End If
            Return sb.ToString()

        End Function
    End Class

End Class
