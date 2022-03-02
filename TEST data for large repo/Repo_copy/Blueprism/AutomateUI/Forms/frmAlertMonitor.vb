Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.BPCoreLib

''' <summary>
''' Essentially just a vehicle for showing a taskbar icon, this
''' form will always be hidden to the user.
''' </summary>
''' <remarks></remarks>
Public Class frmAlertMonitor
    Implements IPermission

#Region "members"

    ''' <summary>
    ''' The alert handling object
    ''' </summary>
    ''' <remarks></remarks>
    Private mEngine As clsAlertEngine

    ''' <summary>
    ''' A context menu item
    ''' </summary>
    ''' <remarks></remarks>
    Private mnuClear As ToolStripMenuItem

    ''' <summary>
    ''' The default notify icon is used to symbolise 'no alerts'. The
    ''' form icon is used when an alert is detected.
    ''' </summary>
    ''' <remarks></remarks>
    Private moDefaultIcon As Icon

    ''' <summary>
    ''' The alert config form.
    ''' </summary>
    ''' <remarks></remarks>
    Private moAlertConfig As frmAlertConfig

    ''' <summary>
    ''' This resource's name
    ''' </summary>
    ''' <remarks></remarks>
    Private msResource As String

    ''' <summary>
    ''' Records whether the user has permission to view the alerts history
    ''' </summary>
    Private mbCanViewHistory As Boolean


    ''' <summary>
    ''' Records whether the user has permission to view the alerts configuration
    ''' </summary>
    Private mbCanViewConfig As Boolean
#End Region

    ''' <summary>
    ''' Displays a taskbar icon and starts monitoring the 
    ''' database for alerts. Requires a user to be logged in (according to clsUser.Userguid)
    ''' </summary>
    ''' <param name="Resource"></param>
    Public Sub New(ByVal Resource As String)

        MyBase.New()
        InitializeComponent()

        msResource = Resource
        mbCanViewHistory = User.Current.HasPermission("Subscribe to Process Alerts")
        mbCanViewConfig = User.Current.HasPermission("Configure Process Alerts")

        If Not User.Current.HasPermission("Subscribe to Process Alerts") Then
            'No need to do anything else if we don't have permission
            Exit Sub
        End If

        'Set up the context menu.
        Dim mnuHistory, mnuConfig, mnuLaunch, mnuExit As ToolStripMenuItem

        mnuClear = New ToolStripMenuItem(My.Resources.frmAlertMonitor_ClearAlert, Nothing, AddressOf Menu_Clear)
        mnuClear.Enabled = False

        mnuHistory = New ToolStripMenuItem(My.Resources.frmAlertMonitor_ViewAlertHistory, Nothing, AddressOf Menu_History)
        mnuHistory.Enabled = Me.mbCanViewHistory
        mnuConfig = New ToolStripMenuItem(My.Resources.frmAlertMonitor_ConfigureAlerts, Nothing, AddressOf Menu_Config)
        mnuConfig.Enabled = Me.mbCanViewConfig
        mnuLaunch = New ToolStripMenuItem(My.Resources.frmAlertMonitor_Launch & ApplicationProperties.ApplicationName, Nothing, AddressOf Menu_Launch)
        mnuExit = New ToolStripMenuItem(My.Resources.frmAlertMonitor_Exit, Nothing, AddressOf Menu_Exit)

        Dim oMenu As New ContextMenuStrip()
        oMenu.Items.AddRange(New ToolStripItem() {mnuClear, mnuHistory, mnuConfig, mnuLaunch, New ToolStripSeparator, mnuExit})
        Me.AlertNotifyIcon.ContextMenuStrip = oMenu

        'Display the icon in the taskbar.
        AlertNotifyIcon.Visible = True
        AlertNotifyIcon.Text = String.Format("{0} Process Alert Monitor", ApplicationProperties.ApplicationName)
        moDefaultIcon = AlertNotifyIcon.Icon

        'Start monitoring.
        mEngine = New clsAlertEngine(Me, User.Current, msResource)

    End Sub


    ''' <summary>
    ''' Closes the form if the user does not have permission.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub frmAlertMonitor_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'Close form if not authorised to monitor alerts...
        If Not mEngine.AlertsUser.HasPermission("Subscribe to Process Alerts") Then
            UserMessage.Show(
             String.Format(My.Resources.frmAlertMonitor_YouDoNotHavePermissionToMonitorProcessAlertsIfYouBelieveThatThisIsIncorrectThen, ApplicationProperties.ApplicationName), 1048586)
            Close()
        End If

    End Sub

    ''' <summary>
    ''' Displays an alert
    ''' </summary>
    ''' <param name="al">The alert</param>
    ''' <remarks></remarks>
    Public Sub Notify(ByVal al As clsAlertEngine.Alert)

        Dim AlertMessage As String = String.Empty
        Dim MachineName = ResourceMachine.GetName()
        If Not gSv.IsAlertMachineRegistered(MachineName) Then
            Try
                gSv.RegisterAlertMachine(MachineName)
            Catch ex As Exception
                AlertMessage = String.Format(My.Resources.frmAlertMonitor_CouldNotRegisterMachineForAlerts0, ex.Message)
            End Try
        Else
            AlertMessage = al.GetLongMessage()
        End If


        Me.AlertNotifyIcon.Icon = Me.Icon
        mnuClear.Enabled = True

        If AlertMessage.Length < 64 Then
            Me.AlertNotifyIcon.Text = AlertMessage
        Else
            Me.AlertNotifyIcon.Text = AlertMessage.Substring(0, 60) & My.Resources.frmAlertMonitor_Ellipsis
        End If

    End Sub

    ''' <summary>
    ''' Resets the icon and clears the current message.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Menu_Clear(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Me.AlertNotifyIcon.Icon = moDefaultIcon
        Me.AlertNotifyIcon.Text = ""
        mnuClear.Enabled = False

    End Sub

    ''' <summary>
    ''' Closes down.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Menu_Exit(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.AlertNotifyIcon.Visible = False
        mEngine.Dispose()
        mEngine = Nothing
        If User.LoggedIn Then User.Logout()
        Me.Close()
    End Sub

    ''' <summary>
    ''' Displays the config form.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Menu_Config(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim bShowDialog As Boolean

        If moAlertConfig Is Nothing OrElse moAlertConfig.IsDisposed OrElse moAlertConfig.Disposing Then
            If frmAlertConfig.InstanceExists Then
                'The config form is already open
                moAlertConfig = frmAlertConfig.GetInstance(mEngine.AlertsUser, frmAlertConfig.ViewMode.ProcessConfig)
            Else
                'Open a new config form
                moAlertConfig = New frmAlertConfig(mEngine.AlertsUser, frmAlertConfig.ViewMode.ProcessConfig)
                bShowDialog = True
            End If
        Else
            'The config form is already open
            moAlertConfig.SetViewMode(frmAlertConfig.ViewMode.ProcessConfig)
        End If

        If bShowDialog Then
            moAlertConfig.SetViewMode(frmAlertConfig.ViewMode.ProcessConfig)
            moAlertConfig.ShowInTaskbar = False
            moAlertConfig.ShowDialog()
            moAlertConfig.Dispose()
            moAlertConfig = Nothing
        Else
            moAlertConfig.Visible = True
            If moAlertConfig.WindowState = FormWindowState.Minimized Then
                moAlertConfig.WindowState = FormWindowState.Normal
            End If
        End If

    End Sub

    ''' <summary>
    ''' Displays the config form in history mode.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Menu_History(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim bShowDialog As Boolean

        If moAlertConfig Is Nothing OrElse moAlertConfig.IsDisposed OrElse moAlertConfig.Disposing Then
            If frmAlertConfig.InstanceExists Then
                'The config form is already open
                moAlertConfig = frmAlertConfig.GetInstance(mEngine.AlertsUser, frmAlertConfig.ViewMode.History)
            Else
                'Open a new config form
                moAlertConfig = New frmAlertConfig(mEngine.AlertsUser, frmAlertConfig.ViewMode.History)
                bShowDialog = True
            End If
        Else
            'The config form is already open
            moAlertConfig.SetViewMode(frmAlertConfig.ViewMode.History)
        End If

        If bShowDialog Then
            moAlertConfig.SetViewMode(frmAlertConfig.ViewMode.History)
            moAlertConfig.ShowInTaskbar = False
            moAlertConfig.ShowDialog()
            moAlertConfig.Dispose()
            moAlertConfig = Nothing
        Else
            moAlertConfig.Visible = True
            If moAlertConfig.WindowState = FormWindowState.Minimized Then
                moAlertConfig.WindowState = FormWindowState.Normal
            End If
        End If

    End Sub

    ''' <summary>
    ''' Stops monitoring and starts AutomateUI. NB monitoring will
    ''' restart when Automate starts.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Menu_Launch(ByVal sender As System.Object, ByVal e As System.EventArgs)

        'Stop monitoring alerts
        Me.AlertNotifyIcon.Visible = False
        mEngine.Dispose()
        mEngine = Nothing
        If User.LoggedIn Then User.Logout()
        frmApplication.KeepProcessAlertsRunning = Nothing

        If Not moAlertConfig Is Nothing Then
            moAlertConfig.Close()
            moAlertConfig = Nothing
        End If

        'Launch Automate
        Dim oApplication As New frmApplication
        oApplication.ShowInTaskbar = False
        oApplication.ShowDialog()
        oApplication.Dispose()

        If frmApplication.KeepProcessAlertsRunning AndAlso User.LoggedIn Then
            'Restart monitoring
            Me.AlertNotifyIcon.Visible = True
            Me.AlertNotifyIcon.Icon = moDefaultIcon
            mEngine = New clsAlertEngine(Me, User.Current, msResource)
        Else
            Me.Close()
        End If

    End Sub

    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) Implements IPermission.RequiredPermissions
        Get
            ' FIXME: This is nonsense - there is no permission named 'Alert Subscriber'
            ' it's a role, which is user-customisable and thus shouldn't be in the code.
            ' I'll return to this when I fully grasp the consequences of changing it
            Return Permission.ByName("Alert Subscriber")
        End Get
    End Property

End Class
