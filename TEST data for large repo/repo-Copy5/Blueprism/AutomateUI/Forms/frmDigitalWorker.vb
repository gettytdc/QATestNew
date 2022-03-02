Imports BluePrism.Core.Utility
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Resources
Imports LocaleTools

''' Project  : Automate
''' Class    : frmDigitalWorker
''' 
''' <summary>
''' A form displayed when the PC is running as a resource PC.
''' </summary>
Friend Class frmDigitalWorker
    Inherits frmForm
    Implements IPermission
    Implements IResourcePcView

#Region " Windows Form Designer generated code "

    
    'Required by the Windows Form Designer
    Private ReadOnly mStartUpOptions As DigitalWorkerStartUpOptions
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents btnShutdown As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lblActiveConnections As System.Windows.Forms.Label
    Friend WithEvents lblRunningProcesses As System.Windows.Forms.Label
    Friend WithEvents btnHide As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents txtStatus As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents ctxMenu As System.Windows.Forms.ContextMenuStrip
    Private WithEvents menuShow As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents menuHide As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents menuRestart As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents menuExit As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents iconNotify As System.Windows.Forms.NotifyIcon
    Friend WithEvents mStatusUpdateTimer As System.Windows.Forms.Timer
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmDigitalWorker))
        Me.btnShutdown = New AutomateControls.Buttons.StandardStyledButton()
        Me.lblActiveConnections = New System.Windows.Forms.Label()
        Me.lblRunningProcesses = New System.Windows.Forms.Label()
        Me.btnHide = New AutomateControls.Buttons.StandardStyledButton()
        Me.txtStatus = New AutomateControls.Textboxes.StyledTextBox()
        Me.ctxMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.menuShow = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuHide = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuRestart = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuExit = New System.Windows.Forms.ToolStripMenuItem()
        Me.iconNotify = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.mStatusUpdateTimer = New System.Windows.Forms.Timer(Me.components)
        Me.ctxMenu.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnShutdown
        '
        resources.ApplyResources(Me.btnShutdown, "btnShutdown")
        Me.btnShutdown.Name = "btnShutdown"
        '
        'lblActiveConnections
        '
        resources.ApplyResources(Me.lblActiveConnections, "lblActiveConnections")
        Me.lblActiveConnections.Name = "lblActiveConnections"
        '
        'lblRunningProcesses
        '
        resources.ApplyResources(Me.lblRunningProcesses, "lblRunningProcesses")
        Me.lblRunningProcesses.Name = "lblRunningProcesses"
        '
        'btnHide
        '
        resources.ApplyResources(Me.btnHide, "btnHide")
        Me.btnHide.Name = "btnHide"
        '
        'txtStatus
        '
        resources.ApplyResources(Me.txtStatus, "txtStatus")
        Me.txtStatus.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.txtStatus.Name = "txtStatus"
        Me.txtStatus.ReadOnly = True
        '
        'ctxMenu
        '
        Me.ctxMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.menuShow, Me.menuHide, Me.menuRestart, Me.menuExit})
        Me.ctxMenu.Name = "ctxMenu"
        resources.ApplyResources(Me.ctxMenu, "ctxMenu")
        '
        'menuShow
        '
        Me.menuShow.Name = "menuShow"
        resources.ApplyResources(Me.menuShow, "menuShow")
        '
        'menuHide
        '
        Me.menuHide.Name = "menuHide"
        resources.ApplyResources(Me.menuHide, "menuHide")
        '
        'menuRestart
        '
        Me.menuRestart.Name = "menuRestart"
        resources.ApplyResources(Me.menuRestart, "menuRestart")
        '
        'menuExit
        '
        Me.menuExit.Name = "menuExit"
        resources.ApplyResources(Me.menuExit, "menuExit")
        '
        'iconNotify
        '
        Me.iconNotify.ContextMenuStrip = Me.ctxMenu
        resources.ApplyResources(Me.iconNotify, "iconNotify")
        '
        'mStatusUpdateTimer
        '
        Me.mStatusUpdateTimer.Interval = 120
        '
        'frmDigitalWorker
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.txtStatus)
        Me.Controls.Add(Me.btnHide)
        Me.Controls.Add(Me.lblRunningProcesses)
        Me.Controls.Add(Me.lblActiveConnections)
        Me.Controls.Add(Me.btnShutdown)
        Me.Name = "frmDigitalWorker"
        Me.ctxMenu.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

    Public Event RefreshStatus(Sender As Object, e As EventArgs) Implements IResourcePCView.RefreshStatus

    Public Event RestartRequested(Sender As Object, e As EventArgs) Implements IResourcePCView.RestartRequested

    Public Event ShutdownRequested(Sender As Object, e As EventArgs) Implements IResourcePCView.ShutdownRequested
    
    Public Event CloseRequested(Sender As Object, ByVal e As FormClosingEventArgs) Implements IResourcePCView.CloseRequested

    ''' <summary>
    ''' A queue of status messages, which can come from different threads. They are
    ''' added to this queue via AddStatusLine, then get output the the GUI from the
    ''' main GUI thread on a timer.
    ''' </summary>
    Private ReadOnly mStatusQueue As New Queue(Of ResourceNotification)

    ''' <summary>
    ''' A buffer of the status text displayed on the form. When form
    ''' created invisibly, it must be copied from here to the UI
    ''' when the form is first shown.
    ''' </summary>
    Private ReadOnly mStatusBuffer As StringBuilder
    
    Public Sub New(startUpOptions As DigitalWorkerStartUpOptions)
        'This call is required by the Windows Form Designer.
        InitializeComponent()

        mStartUpOptions = startUpOptions
        mStatusBuffer = New StringBuilder()
        mStatusUpdateTimer.Start()

        Me.Text = String.Format(My.Resources.BluePrismDigitalWorker0, mStartUpOptions.Name.FullName)

        iconNotify.Icon = Icons.ResourcePC
        iconNotify.Text = Me.Text
    End Sub

    ''' <summary>
    ''' Handles the 'Show' menu item being clicked in the context menu.
    ''' </summary>
    Private Sub HandleShow(ByVal Sender As Object, ByVal e As EventArgs) _
        Handles menuShow.Click
        Visible = True
        Activate()
        txtStatus.Text = mStatusBuffer.ToString()
    End Sub

    ''' <summary>
    ''' Handles the 'Hide' button or menu item being clicked
    ''' </summary>
    Private Sub HandleHide(ByVal sender As Object, ByVal e As EventArgs) _
        Handles btnHide.Click, menuHide.Click
        Hide()
    End Sub

    ''' <summary>
    ''' Handles the Restart menuitem being clicked in the context menu
    ''' </summary>
    Private Sub HandleRestartClicked(ByVal Sender As Object, ByVal e As EventArgs) Handles menuRestart.Click
        RaiseEvent RestartRequested(Me, EventArgs.Empty)
    End Sub

    Private Sub HandleExit(ByVal sender As Object, ByVal e As EventArgs) Handles btnShutdown.Click, menuExit.Click
        RaiseEvent ShutdownRequested (Me, EventArgs.Empty)
    End Sub

    Sub Init() Implements IResourcePCView.Init 
        Activate()
        btnShutdown.Enabled = True
        iconNotify.Visible = True
    End Sub

    ''' <summary>
    ''' Gets the associated permission level.
    ''' </summary>
    ''' <value>The permission level</value>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(Permission.Resources.ControlResource)
        End Get
    End Property

    Public ReadOnly Property FormDialogResult As DialogResult Implements IResourcePCView.FormDialogResult
        Get
            Return DialogResult
        End Get
    End Property

    Private Sub DisplayNotification(notification As ResourceNotification) Implements IResourcePCView.DisplayNotification
        AddStatusLine(notification)
    End Sub

    ''' <summary>
    ''' Update the UI (on the UI thread) every timer period (120ms)
    ''' </summary>
    Private Sub UpdateResourceStatus(sender As Object, e As EventArgs) Handles mStatusUpdateTimer.Tick

        UpdateStatusOutput()

        RaiseEvent RefreshStatus(Me, EventArgs.Empty)
       
    End Sub

    Public Sub DisplayStatus(activeSessions As Integer, pendingSessions As Integer, connectionStatus As String) Implements IResourcePCView.DisplayStatus

        lblRunningProcesses.Text = String.Format(My.Resources.RunningProcesses01Pending,
                                                 activeSessions,
                                                 pendingSessions)
        lblActiveConnections.Text = connectionStatus
    End Sub

    Public Function ConfirmShutdown(activeSessionCount As Integer) As Boolean Implements IResourcePCView.ConfirmShutdown
        Dim message = LTools.Format(My.Resources.frmResourcePC_plural_processes_exit, "COUNT", activeSessionCount, "VBCRLF", vbCrLf)
        Dim result = UserMessage.TwoButtonsWithCustomText(message, My.Resources.frmResourcePC_Exit, My.Resources.frmResourcePC_Cancel)
        Return result = MsgBoxResult.Yes
    End Function

    Public Function ConfirmRestart(activeSessionCount As Integer) As Boolean Implements IResourcePCView.ConfirmRestart
        Dim message = LTools.Format(My.Resources.frmResourcePC_plural_processes, "COUNT", activeSessionCount, "VBCRLF", vbCrLf)
        Dim result = UserMessage.TwoButtonsWithCustomText(message, My.Resources.frmResourcePC_Restart, My.Resources.frmResourcePC_Cancel)
        Return result = MsgBoxResult.Yes 
    End Function

    Public Sub DisplayRestarting() Implements IResourcePCView.DisplayRestarting
        MyBase.Activate()

        If Not Visible Then
            With iconNotify
                .BalloonTipIcon = ToolTipIcon.Info
                .BalloonTipText = String.Format(My.Resources.AutomatesResourcePcHasBeenRestartedOnPort0, mStartUpOptions.Name.FullName)
                .BalloonTipTitle = My.Resources.ResourcePCInformation
                .ShowBalloonTip(5000)
            End With
        End If
    End Sub
    
    Public Sub DisplayShuttingDown() Implements IResourcePCView.DisplayShuttingDown
        iconNotify.Visible = False
        btnShutdown.Enabled = False
    End Sub

    Public Sub ShowForm() Implements IResourcePCView.ShowForm
        Show()
    End Sub

    Public Sub CloseForm() Implements IResourcePCView.CloseForm
        If Visible Then Invoke(Sub() Close())
    End Sub

    Protected Overrides Sub OnFormClosing(ByVal e As FormClosingEventArgs)
        RaiseEvent CloseRequested(Me, e)
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As FormClosedEventArgs)
        MyBase.OnFormClosed(e)
        ' DialogResult monitored externally
        DialogResult = DialogResult.OK
    End Sub

    'Form overrides dispose to clean up the component list.
    ''' <summary>
    ''' Send any output from the status queue to the GUI.
    ''' </summary>
    Private Sub UpdateStatusOutput()
        SyncLock mStatusQueue
            While mStatusQueue.Count <> 0
                Dim entry As ResourceNotification = mStatusQueue.Dequeue()
                Dim outputText = txtStatus.Text & entry.Text & vbCrLf
                While outputText.Length > 32766
                    outputText = outputText.Substring(InStr(outputText, vbCrLf))
                End While
                txtStatus.Text = outputText  'entry.Text & vbCrLf
                txtStatus.Select(txtStatus.Text.Length - 1, 0)
                txtStatus.ScrollToCaret()
            End While
        End SyncLock
    End Sub

    Private Sub AddStatusLine(notification As ResourceNotification)
        SyncLock mStatusQueue
            mStatusBuffer.Append(notification.Text)
            mStatusBuffer.AppendLine()
            'Ensure the mStatusBuffer Content will fix inside the txtStatus textbox
            While mStatusBuffer.Length > txtStatus.MaxLength - 1
                'remove the first line
                mStatusBuffer.Remove(0, InStr(mStatusBuffer.ToString(), vbCrLf) + 1)
            End While
            mStatusQueue.Enqueue(notification)
        End SyncLock
    End Sub

    Public Sub RunOnUIThread(action As Action) Implements IResourcePCView.RunOnUIThread
        InvokeOnUiThread(action)
    End Sub
    Public Sub BeginRunOnUIThread(action As Action) Implements IResourcePCView.BeginRunOnUIThread
        BeginInvokeOnUiThread(action)
    End Sub

    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub
End Class
