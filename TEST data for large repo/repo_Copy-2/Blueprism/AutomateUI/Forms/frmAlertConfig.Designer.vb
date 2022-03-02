

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAlertConfig
    Inherits frmForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAlertConfig))
        Me.tcMain = New System.Windows.Forms.TabControl()
        Me.tabProcesses = New System.Windows.Forms.TabPage()
        Me.btnClearProcesses = New AutomateControls.Buttons.StandardStyledButton()
        Me.lvProcesses = New AutomateControls.FlickerFreeListView()
        Me.Column0 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.Column1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.gpProcessAlertTypes = New System.Windows.Forms.GroupBox()
        Me.chkFailed = New System.Windows.Forms.CheckBox()
        Me.chkStopped = New System.Windows.Forms.CheckBox()
        Me.chkComplete = New System.Windows.Forms.CheckBox()
        Me.chkStage = New System.Windows.Forms.CheckBox()
        Me.chkRunning = New System.Windows.Forms.CheckBox()
        Me.chkPending = New System.Windows.Forms.CheckBox()
        Me.gpProcessNotifMethods = New System.Windows.Forms.GroupBox()
        Me.chkSound = New System.Windows.Forms.CheckBox()
        Me.chkTaskbar = New System.Windows.Forms.CheckBox()
        Me.chkShowHistory = New System.Windows.Forms.CheckBox()
        Me.chkPopUp = New System.Windows.Forms.CheckBox()
        Me.tabSchedules = New System.Windows.Forms.TabPage()
        Me.btnClearSchedules = New AutomateControls.Buttons.StandardStyledButton()
        Me.gpTaskAlertTypes = New System.Windows.Forms.GroupBox()
        Me.chkTaskStarted = New System.Windows.Forms.CheckBox()
        Me.chkTaskCompleted = New System.Windows.Forms.CheckBox()
        Me.chkTaskTerminated = New System.Windows.Forms.CheckBox()
        Me.gpScheduleAlertTypes = New System.Windows.Forms.GroupBox()
        Me.chkScheduleStarted = New System.Windows.Forms.CheckBox()
        Me.chkScheduleCompleted = New System.Windows.Forms.CheckBox()
        Me.chkScheduleTerminated = New System.Windows.Forms.CheckBox()
        Me.lvSchedules = New System.Windows.Forms.ListView()
        Me.colSchedName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colSchedDesc = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.gpScheduleNotifMethods = New System.Windows.Forms.GroupBox()
        Me.chkSchedulePlaySound = New System.Windows.Forms.CheckBox()
        Me.chkScheduleTaskbar = New System.Windows.Forms.CheckBox()
        Me.chkScheduleShowHistory = New System.Windows.Forms.CheckBox()
        Me.chkSchedulePopup = New System.Windows.Forms.CheckBox()
        Me.tabHistory = New System.Windows.Forms.TabPage()
        Me.gridHistory = New System.Windows.Forms.DataGridView()
        Me.btnExport = New AutomateControls.Buttons.StandardStyledButton()
        Me.lblDate = New System.Windows.Forms.Label()
        Me.btnPrevious = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnNext = New AutomateControls.Buttons.StandardStyledButton()
        Me.tabPermissionDenied = New System.Windows.Forms.TabPage()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btnOk = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnHelp = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.dialogSave = New System.Windows.Forms.SaveFileDialog()
        Me.btnApply = New AutomateControls.Buttons.StandardStyledButton()
        Me.tcMain.SuspendLayout()
        Me.tabProcesses.SuspendLayout()
        Me.gpProcessAlertTypes.SuspendLayout()
        Me.gpProcessNotifMethods.SuspendLayout()
        Me.tabSchedules.SuspendLayout()
        Me.gpTaskAlertTypes.SuspendLayout()
        Me.gpScheduleAlertTypes.SuspendLayout()
        Me.gpScheduleNotifMethods.SuspendLayout()
        Me.tabHistory.SuspendLayout()
        CType(Me.gridHistory, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tabPermissionDenied.SuspendLayout()
        Me.SuspendLayout()
        '
        'tcMain
        '
        resources.ApplyResources(Me.tcMain, "tcMain")
        Me.tcMain.Controls.Add(Me.tabProcesses)
        Me.tcMain.Controls.Add(Me.tabSchedules)
        Me.tcMain.Controls.Add(Me.tabHistory)
        Me.tcMain.Controls.Add(Me.tabPermissionDenied)
        Me.tcMain.Name = "tcMain"
        Me.tcMain.SelectedIndex = 0
        '
        'tabProcesses
        '
        Me.tabProcesses.Controls.Add(Me.btnClearProcesses)
        Me.tabProcesses.Controls.Add(Me.lvProcesses)
        Me.tabProcesses.Controls.Add(Me.gpProcessAlertTypes)
        Me.tabProcesses.Controls.Add(Me.gpProcessNotifMethods)
        resources.ApplyResources(Me.tabProcesses, "tabProcesses")
        Me.tabProcesses.Name = "tabProcesses"
        Me.tabProcesses.UseVisualStyleBackColor = True
        '
        'btnClearProcesses
        '
        resources.ApplyResources(Me.btnClearProcesses, "btnClearProcesses")
        Me.btnClearProcesses.Name = "btnClearProcesses"
        Me.btnClearProcesses.UseVisualStyleBackColor = True
        '
        'lvProcesses
        '
        resources.ApplyResources(Me.lvProcesses, "lvProcesses")
        Me.lvProcesses.CheckBoxes = True
        Me.lvProcesses.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.Column0, Me.Column1})
        Me.lvProcesses.FullRowSelect = True
        Me.lvProcesses.Items.AddRange(New System.Windows.Forms.ListViewItem() {CType(resources.GetObject("lvProcesses.Items"), System.Windows.Forms.ListViewItem)})
        Me.lvProcesses.Name = "lvProcesses"
        Me.lvProcesses.UseCompatibleStateImageBehavior = False
        Me.lvProcesses.View = System.Windows.Forms.View.Details
        '
        'Column0
        '
        resources.ApplyResources(Me.Column0, "Column0")
        '
        'Column1
        '
        resources.ApplyResources(Me.Column1, "Column1")
        '
        'gpProcessAlertTypes
        '
        resources.ApplyResources(Me.gpProcessAlertTypes, "gpProcessAlertTypes")
        Me.gpProcessAlertTypes.Controls.Add(Me.chkFailed)
        Me.gpProcessAlertTypes.Controls.Add(Me.chkStopped)
        Me.gpProcessAlertTypes.Controls.Add(Me.chkComplete)
        Me.gpProcessAlertTypes.Controls.Add(Me.chkStage)
        Me.gpProcessAlertTypes.Controls.Add(Me.chkRunning)
        Me.gpProcessAlertTypes.Controls.Add(Me.chkPending)
        Me.gpProcessAlertTypes.Name = "gpProcessAlertTypes"
        Me.gpProcessAlertTypes.TabStop = False
        '
        'chkFailed
        '
        resources.ApplyResources(Me.chkFailed, "chkFailed")
        Me.chkFailed.Name = "chkFailed"
        Me.chkFailed.UseVisualStyleBackColor = True
        '
        'chkStopped
        '
        resources.ApplyResources(Me.chkStopped, "chkStopped")
        Me.chkStopped.Name = "chkStopped"
        Me.chkStopped.UseVisualStyleBackColor = True
        '
        'chkComplete
        '
        resources.ApplyResources(Me.chkComplete, "chkComplete")
        Me.chkComplete.Name = "chkComplete"
        Me.chkComplete.UseVisualStyleBackColor = True
        '
        'chkStage
        '
        resources.ApplyResources(Me.chkStage, "chkStage")
        Me.chkStage.Name = "chkStage"
        Me.chkStage.UseVisualStyleBackColor = True
        '
        'chkRunning
        '
        resources.ApplyResources(Me.chkRunning, "chkRunning")
        Me.chkRunning.Name = "chkRunning"
        Me.chkRunning.UseVisualStyleBackColor = True
        '
        'chkPending
        '
        resources.ApplyResources(Me.chkPending, "chkPending")
        Me.chkPending.Name = "chkPending"
        Me.chkPending.UseVisualStyleBackColor = True
        '
        'gpProcessNotifMethods
        '
        resources.ApplyResources(Me.gpProcessNotifMethods, "gpProcessNotifMethods")
        Me.gpProcessNotifMethods.Controls.Add(Me.chkSound)
        Me.gpProcessNotifMethods.Controls.Add(Me.chkTaskbar)
        Me.gpProcessNotifMethods.Controls.Add(Me.chkShowHistory)
        Me.gpProcessNotifMethods.Controls.Add(Me.chkPopUp)
        Me.gpProcessNotifMethods.Name = "gpProcessNotifMethods"
        Me.gpProcessNotifMethods.TabStop = False
        '
        'chkSound
        '
        resources.ApplyResources(Me.chkSound, "chkSound")
        Me.chkSound.Name = "chkSound"
        Me.chkSound.UseVisualStyleBackColor = True
        '
        'chkTaskbar
        '
        resources.ApplyResources(Me.chkTaskbar, "chkTaskbar")
        Me.chkTaskbar.Name = "chkTaskbar"
        Me.chkTaskbar.UseVisualStyleBackColor = True
        '
        'chkShowHistory
        '
        resources.ApplyResources(Me.chkShowHistory, "chkShowHistory")
        Me.chkShowHistory.Name = "chkShowHistory"
        Me.chkShowHistory.UseVisualStyleBackColor = True
        '
        'chkPopUp
        '
        resources.ApplyResources(Me.chkPopUp, "chkPopUp")
        Me.chkPopUp.Name = "chkPopUp"
        Me.chkPopUp.UseVisualStyleBackColor = True
        '
        'tabSchedules
        '
        Me.tabSchedules.Controls.Add(Me.btnClearSchedules)
        Me.tabSchedules.Controls.Add(Me.gpTaskAlertTypes)
        Me.tabSchedules.Controls.Add(Me.gpScheduleAlertTypes)
        Me.tabSchedules.Controls.Add(Me.lvSchedules)
        Me.tabSchedules.Controls.Add(Me.gpScheduleNotifMethods)
        resources.ApplyResources(Me.tabSchedules, "tabSchedules")
        Me.tabSchedules.Name = "tabSchedules"
        Me.tabSchedules.UseVisualStyleBackColor = True
        '
        'btnClearSchedules
        '
        resources.ApplyResources(Me.btnClearSchedules, "btnClearSchedules")
        Me.btnClearSchedules.Name = "btnClearSchedules"
        Me.btnClearSchedules.UseVisualStyleBackColor = True
        '
        'gpTaskAlertTypes
        '
        resources.ApplyResources(Me.gpTaskAlertTypes, "gpTaskAlertTypes")
        Me.gpTaskAlertTypes.Controls.Add(Me.chkTaskStarted)
        Me.gpTaskAlertTypes.Controls.Add(Me.chkTaskCompleted)
        Me.gpTaskAlertTypes.Controls.Add(Me.chkTaskTerminated)
        Me.gpTaskAlertTypes.Name = "gpTaskAlertTypes"
        Me.gpTaskAlertTypes.TabStop = False
        '
        'chkTaskStarted
        '
        resources.ApplyResources(Me.chkTaskStarted, "chkTaskStarted")
        Me.chkTaskStarted.Name = "chkTaskStarted"
        Me.chkTaskStarted.UseVisualStyleBackColor = True
        '
        'chkTaskCompleted
        '
        resources.ApplyResources(Me.chkTaskCompleted, "chkTaskCompleted")
        Me.chkTaskCompleted.Name = "chkTaskCompleted"
        Me.chkTaskCompleted.UseVisualStyleBackColor = True
        '
        'chkTaskTerminated
        '
        resources.ApplyResources(Me.chkTaskTerminated, "chkTaskTerminated")
        Me.chkTaskTerminated.Name = "chkTaskTerminated"
        Me.chkTaskTerminated.UseVisualStyleBackColor = True
        '
        'gpScheduleAlertTypes
        '
        resources.ApplyResources(Me.gpScheduleAlertTypes, "gpScheduleAlertTypes")
        Me.gpScheduleAlertTypes.Controls.Add(Me.chkScheduleStarted)
        Me.gpScheduleAlertTypes.Controls.Add(Me.chkScheduleCompleted)
        Me.gpScheduleAlertTypes.Controls.Add(Me.chkScheduleTerminated)
        Me.gpScheduleAlertTypes.Name = "gpScheduleAlertTypes"
        Me.gpScheduleAlertTypes.TabStop = False
        '
        'chkScheduleStarted
        '
        resources.ApplyResources(Me.chkScheduleStarted, "chkScheduleStarted")
        Me.chkScheduleStarted.Name = "chkScheduleStarted"
        Me.chkScheduleStarted.UseVisualStyleBackColor = True
        '
        'chkScheduleCompleted
        '
        resources.ApplyResources(Me.chkScheduleCompleted, "chkScheduleCompleted")
        Me.chkScheduleCompleted.Name = "chkScheduleCompleted"
        Me.chkScheduleCompleted.UseVisualStyleBackColor = True
        '
        'chkScheduleTerminated
        '
        resources.ApplyResources(Me.chkScheduleTerminated, "chkScheduleTerminated")
        Me.chkScheduleTerminated.Name = "chkScheduleTerminated"
        Me.chkScheduleTerminated.UseVisualStyleBackColor = True
        '
        'lvSchedules
        '
        resources.ApplyResources(Me.lvSchedules, "lvSchedules")
        Me.lvSchedules.CheckBoxes = True
        Me.lvSchedules.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colSchedName, Me.colSchedDesc})
        Me.lvSchedules.FullRowSelect = True
        Me.lvSchedules.Items.AddRange(New System.Windows.Forms.ListViewItem() {CType(resources.GetObject("lvSchedules.Items"), System.Windows.Forms.ListViewItem)})
        Me.lvSchedules.Name = "lvSchedules"
        Me.lvSchedules.UseCompatibleStateImageBehavior = False
        Me.lvSchedules.View = System.Windows.Forms.View.Details
        '
        'colSchedName
        '
        resources.ApplyResources(Me.colSchedName, "colSchedName")
        '
        'colSchedDesc
        '
        resources.ApplyResources(Me.colSchedDesc, "colSchedDesc")
        '
        'gpScheduleNotifMethods
        '
        resources.ApplyResources(Me.gpScheduleNotifMethods, "gpScheduleNotifMethods")
        Me.gpScheduleNotifMethods.Controls.Add(Me.chkSchedulePlaySound)
        Me.gpScheduleNotifMethods.Controls.Add(Me.chkScheduleTaskbar)
        Me.gpScheduleNotifMethods.Controls.Add(Me.chkScheduleShowHistory)
        Me.gpScheduleNotifMethods.Controls.Add(Me.chkSchedulePopup)
        Me.gpScheduleNotifMethods.Name = "gpScheduleNotifMethods"
        Me.gpScheduleNotifMethods.TabStop = False
        '
        'chkSchedulePlaySound
        '
        resources.ApplyResources(Me.chkSchedulePlaySound, "chkSchedulePlaySound")
        Me.chkSchedulePlaySound.Name = "chkSchedulePlaySound"
        Me.chkSchedulePlaySound.UseVisualStyleBackColor = True
        '
        'chkScheduleTaskbar
        '
        resources.ApplyResources(Me.chkScheduleTaskbar, "chkScheduleTaskbar")
        Me.chkScheduleTaskbar.Name = "chkScheduleTaskbar"
        Me.chkScheduleTaskbar.UseVisualStyleBackColor = True
        '
        'chkScheduleShowHistory
        '
        resources.ApplyResources(Me.chkScheduleShowHistory, "chkScheduleShowHistory")
        Me.chkScheduleShowHistory.Name = "chkScheduleShowHistory"
        Me.chkScheduleShowHistory.UseVisualStyleBackColor = True
        '
        'chkSchedulePopup
        '
        resources.ApplyResources(Me.chkSchedulePopup, "chkSchedulePopup")
        Me.chkSchedulePopup.Name = "chkSchedulePopup"
        Me.chkSchedulePopup.UseVisualStyleBackColor = True
        '
        'tabHistory
        '
        Me.tabHistory.Controls.Add(Me.gridHistory)
        Me.tabHistory.Controls.Add(Me.btnExport)
        Me.tabHistory.Controls.Add(Me.lblDate)
        Me.tabHistory.Controls.Add(Me.btnPrevious)
        Me.tabHistory.Controls.Add(Me.btnNext)
        resources.ApplyResources(Me.tabHistory, "tabHistory")
        Me.tabHistory.Name = "tabHistory"
        Me.tabHistory.UseVisualStyleBackColor = True
        '
        'gridHistory
        '
        Me.gridHistory.AllowUserToAddRows = False
        Me.gridHistory.AllowUserToDeleteRows = False
        resources.ApplyResources(Me.gridHistory, "gridHistory")
        Me.gridHistory.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells
        Me.gridHistory.BackgroundColor = System.Drawing.SystemColors.Window
        Me.gridHistory.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.gridHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridHistory.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically
        Me.gridHistory.Name = "gridHistory"
        Me.gridHistory.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None
        Me.gridHistory.RowHeadersVisible = False
        Me.gridHistory.ShowCellErrors = False
        '
        'btnExport
        '
        resources.ApplyResources(Me.btnExport, "btnExport")
        Me.btnExport.Name = "btnExport"
        Me.btnExport.UseVisualStyleBackColor = True
        '
        'lblDate
        '
        resources.ApplyResources(Me.lblDate, "lblDate")
        Me.lblDate.Name = "lblDate"
        '
        'btnPrevious
        '
        resources.ApplyResources(Me.btnPrevious, "btnPrevious")
        Me.btnPrevious.Name = "btnPrevious"
        Me.btnPrevious.UseVisualStyleBackColor = True
        '
        'btnNext
        '
        resources.ApplyResources(Me.btnNext, "btnNext")
        Me.btnNext.Name = "btnNext"
        Me.btnNext.UseVisualStyleBackColor = True
        '
        'tabPermissionDenied
        '
        Me.tabPermissionDenied.Controls.Add(Me.Label2)
        Me.tabPermissionDenied.Controls.Add(Me.Label1)
        resources.ApplyResources(Me.tabPermissionDenied, "tabPermissionDenied")
        Me.tabPermissionDenied.Name = "tabPermissionDenied"
        Me.tabPermissionDenied.UseVisualStyleBackColor = True
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'btnOk
        '
        resources.ApplyResources(Me.btnOk, "btnOk")
        Me.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnOk.Name = "btnOk"
        Me.btnOk.UseVisualStyleBackColor = True
        '
        'btnHelp
        '
        resources.ApplyResources(Me.btnHelp, "btnHelp")
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'dialogSave
        '
        resources.ApplyResources(Me.dialogSave, "dialogSave")
        Me.dialogSave.InitialDirectory = "c:\"
        '
        'btnApply
        '
        resources.ApplyResources(Me.btnApply, "btnApply")
        Me.btnApply.Name = "btnApply"
        Me.btnApply.UseVisualStyleBackColor = True
        '
        'frmAlertConfig
        '
        Me.AcceptButton = Me.btnOk
        Me.CancelButton = Me.btnCancel
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.btnApply)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btnOk)
        Me.Controls.Add(Me.tcMain)
        Me.Name = "frmAlertConfig"
        Me.tcMain.ResumeLayout(False)
        Me.tabProcesses.ResumeLayout(False)
        Me.gpProcessAlertTypes.ResumeLayout(False)
        Me.gpProcessAlertTypes.PerformLayout()
        Me.gpProcessNotifMethods.ResumeLayout(False)
        Me.gpProcessNotifMethods.PerformLayout()
        Me.tabSchedules.ResumeLayout(False)
        Me.gpTaskAlertTypes.ResumeLayout(False)
        Me.gpTaskAlertTypes.PerformLayout()
        Me.gpScheduleAlertTypes.ResumeLayout(False)
        Me.gpScheduleAlertTypes.PerformLayout()
        Me.gpScheduleNotifMethods.ResumeLayout(False)
        Me.gpScheduleNotifMethods.PerformLayout()
        Me.tabHistory.ResumeLayout(False)
        Me.tabHistory.PerformLayout()
        CType(Me.gridHistory, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tabPermissionDenied.ResumeLayout(False)
        Me.tabPermissionDenied.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents gpProcessAlertTypes As System.Windows.Forms.GroupBox
    Private WithEvents gpProcessNotifMethods As System.Windows.Forms.GroupBox
    Private WithEvents chkShowHistory As System.Windows.Forms.CheckBox
    Private WithEvents chkPopUp As System.Windows.Forms.CheckBox
    Private WithEvents chkPending As System.Windows.Forms.CheckBox
    Private WithEvents chkTaskbar As System.Windows.Forms.CheckBox
    Private WithEvents chkStopped As System.Windows.Forms.CheckBox
    Private WithEvents chkComplete As System.Windows.Forms.CheckBox
    Private WithEvents chkStage As System.Windows.Forms.CheckBox
    Private WithEvents chkRunning As System.Windows.Forms.CheckBox
    Private WithEvents chkFailed As System.Windows.Forms.CheckBox
    Private WithEvents btnOk As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents btnHelp As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents lvProcesses As AutomateControls.FlickerFreeListView
    Private WithEvents Column0 As System.Windows.Forms.ColumnHeader
    Private WithEvents Column1 As System.Windows.Forms.ColumnHeader
    Private WithEvents lblDate As System.Windows.Forms.Label
    Private WithEvents btnPrevious As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents btnNext As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents btnExport As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents dialogSave As System.Windows.Forms.SaveFileDialog
    Private WithEvents chkSound As System.Windows.Forms.CheckBox
    Private WithEvents btnApply As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents gridHistory As System.Windows.Forms.DataGridView
    Private WithEvents Label2 As System.Windows.Forms.Label
    Private WithEvents Label1 As System.Windows.Forms.Label
    Private WithEvents tabSchedules As System.Windows.Forms.TabPage
    Private WithEvents tcMain As System.Windows.Forms.TabControl
    Private WithEvents tabProcesses As System.Windows.Forms.TabPage
    Private WithEvents tabHistory As System.Windows.Forms.TabPage
    Private WithEvents colSchedName As System.Windows.Forms.ColumnHeader
    Private WithEvents colSchedDesc As System.Windows.Forms.ColumnHeader
    Private WithEvents chkScheduleTerminated As System.Windows.Forms.CheckBox
    Private WithEvents chkScheduleCompleted As System.Windows.Forms.CheckBox
    Private WithEvents chkScheduleStarted As System.Windows.Forms.CheckBox
    Private WithEvents gpScheduleNotifMethods As System.Windows.Forms.GroupBox
    Private WithEvents chkSchedulePlaySound As System.Windows.Forms.CheckBox
    Private WithEvents chkScheduleTaskbar As System.Windows.Forms.CheckBox
    Private WithEvents chkScheduleShowHistory As System.Windows.Forms.CheckBox
    Private WithEvents chkSchedulePopup As System.Windows.Forms.CheckBox
    Private WithEvents tabPermissionDenied As System.Windows.Forms.TabPage
    Private WithEvents gpScheduleAlertTypes As System.Windows.Forms.GroupBox
    Private WithEvents gpTaskAlertTypes As System.Windows.Forms.GroupBox
    Private WithEvents chkTaskStarted As System.Windows.Forms.CheckBox
    Private WithEvents chkTaskCompleted As System.Windows.Forms.CheckBox
    Private WithEvents chkTaskTerminated As System.Windows.Forms.CheckBox
    Private WithEvents lvSchedules As System.Windows.Forms.ListView
    Private WithEvents btnClearSchedules As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents btnClearProcesses As AutomateControls.Buttons.StandardStyledButton

End Class
