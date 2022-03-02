<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlSystemArchiving
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSystemArchiving))
        Me.tcSystem = New AutomateControls.SwitchPanel()
        Me.tabArchiving = New System.Windows.Forms.TabPage()
        Me.btnSwitchToAuto = New AutomateControls.Buttons.StandardStyledButton()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.ctlArchivingInterface1 = New AutomateUI.ctlArchivingInterface()
        Me.tabArchivingAuto = New System.Windows.Forms.TabPage()
        Me.btnBrowse = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnUndoChanges = New AutomateControls.Buttons.StandardStyledButton()
        Me.txtArchiveAgeNum = New AutomateControls.Textboxes.StyledTextBox()
        Me.cmbArchiveAgeUnit = New System.Windows.Forms.ComboBox()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.cmbArchiveMode = New System.Windows.Forms.ComboBox()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.btnUpdateSettings = New AutomateControls.Buttons.StandardStyledButton()
        Me.cmbArchiveResource = New System.Windows.Forms.ComboBox()
        Me.txtArchiveFolder = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.btnSwitchToManual = New AutomateControls.Buttons.StandardStyledButton()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.btnMenuStrip = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ReleaseToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.tcSystem.SuspendLayout()
        Me.tabArchiving.SuspendLayout()
        Me.tabArchivingAuto.SuspendLayout()
        Me.btnMenuStrip.SuspendLayout()
        Me.SuspendLayout()
        '
        'tcSystem
        '
        Me.tcSystem.Controls.Add(Me.tabArchiving)
        Me.tcSystem.Controls.Add(Me.tabArchivingAuto)
        resources.ApplyResources(Me.tcSystem, "tcSystem")
        Me.tcSystem.Name = "tcSystem"
        Me.tcSystem.SelectedIndex = 0
        '
        'tabArchiving
        '
        Me.tabArchiving.Controls.Add(Me.btnSwitchToAuto)
        Me.tabArchiving.Controls.Add(Me.Label8)
        Me.tabArchiving.Controls.Add(Me.ctlArchivingInterface1)
        resources.ApplyResources(Me.tabArchiving, "tabArchiving")
        Me.tabArchiving.Name = "tabArchiving"
        Me.tabArchiving.UseVisualStyleBackColor = True
        '
        'btnSwitchToAuto
        '
        resources.ApplyResources(Me.btnSwitchToAuto, "btnSwitchToAuto")
        Me.btnSwitchToAuto.Name = "btnSwitchToAuto"
        '
        'Label8
        '
        Me.Label8.BackColor = System.Drawing.Color.Transparent
        resources.ApplyResources(Me.Label8, "Label8")
        Me.Label8.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label8.Name = "Label8"
        '
        'ctlArchivingInterface1
        '
        resources.ApplyResources(Me.ctlArchivingInterface1, "ctlArchivingInterface1")
        Me.ctlArchivingInterface1.Cursor = System.Windows.Forms.Cursors.Default
        Me.ctlArchivingInterface1.Name = "ctlArchivingInterface1"
        '
        'tabArchivingAuto
        '
        Me.tabArchivingAuto.Controls.Add(Me.btnBrowse)
        Me.tabArchivingAuto.Controls.Add(Me.btnUndoChanges)
        Me.tabArchivingAuto.Controls.Add(Me.txtArchiveAgeNum)
        Me.tabArchivingAuto.Controls.Add(Me.cmbArchiveAgeUnit)
        Me.tabArchivingAuto.Controls.Add(Me.Label13)
        Me.tabArchivingAuto.Controls.Add(Me.cmbArchiveMode)
        Me.tabArchivingAuto.Controls.Add(Me.Label12)
        Me.tabArchivingAuto.Controls.Add(Me.btnUpdateSettings)
        Me.tabArchivingAuto.Controls.Add(Me.cmbArchiveResource)
        Me.tabArchivingAuto.Controls.Add(Me.txtArchiveFolder)
        Me.tabArchivingAuto.Controls.Add(Me.Label10)
        Me.tabArchivingAuto.Controls.Add(Me.Label11)
        Me.tabArchivingAuto.Controls.Add(Me.btnSwitchToManual)
        Me.tabArchivingAuto.Controls.Add(Me.Label9)
        resources.ApplyResources(Me.tabArchivingAuto, "tabArchivingAuto")
        Me.tabArchivingAuto.Name = "tabArchivingAuto"
        Me.tabArchivingAuto.UseVisualStyleBackColor = True
        '
        'btnBrowse
        '
        resources.ApplyResources(Me.btnBrowse, "btnBrowse")
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'btnUndoChanges
        '
        resources.ApplyResources(Me.btnUndoChanges, "btnUndoChanges")
        Me.btnUndoChanges.Name = "btnUndoChanges"
        Me.btnUndoChanges.UseVisualStyleBackColor = True
        '
        'txtArchiveAgeNum
        '
        resources.ApplyResources(Me.txtArchiveAgeNum, "txtArchiveAgeNum")
        Me.txtArchiveAgeNum.Name = "txtArchiveAgeNum"
        '
        'cmbArchiveAgeUnit
        '
        resources.ApplyResources(Me.cmbArchiveAgeUnit, "cmbArchiveAgeUnit")
        Me.cmbArchiveAgeUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbArchiveAgeUnit.FormattingEnabled = True
        Me.cmbArchiveAgeUnit.Items.AddRange(New Object() {resources.GetString("cmbArchiveAgeUnit.Items"), resources.GetString("cmbArchiveAgeUnit.Items1"), resources.GetString("cmbArchiveAgeUnit.Items2"), resources.GetString("cmbArchiveAgeUnit.Items3")})
        Me.cmbArchiveAgeUnit.Name = "cmbArchiveAgeUnit"
        '
        'Label13
        '
        Me.Label13.ForeColor = System.Drawing.Color.Black
        resources.ApplyResources(Me.Label13, "Label13")
        Me.Label13.Name = "Label13"
        '
        'cmbArchiveMode
        '
        resources.ApplyResources(Me.cmbArchiveMode, "cmbArchiveMode")
        Me.cmbArchiveMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbArchiveMode.FormattingEnabled = True
        Me.cmbArchiveMode.Items.AddRange(New Object() {resources.GetString("cmbArchiveMode.Items"), resources.GetString("cmbArchiveMode.Items1")})
        Me.cmbArchiveMode.Name = "cmbArchiveMode"
        '
        'Label12
        '
        Me.Label12.ForeColor = System.Drawing.Color.Black
        resources.ApplyResources(Me.Label12, "Label12")
        Me.Label12.Name = "Label12"
        '
        'btnUpdateSettings
        '
        resources.ApplyResources(Me.btnUpdateSettings, "btnUpdateSettings")
        Me.btnUpdateSettings.Name = "btnUpdateSettings"
        Me.btnUpdateSettings.UseVisualStyleBackColor = True
        '
        'cmbArchiveResource
        '
        resources.ApplyResources(Me.cmbArchiveResource, "cmbArchiveResource")
        Me.cmbArchiveResource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbArchiveResource.FormattingEnabled = True
        Me.cmbArchiveResource.Name = "cmbArchiveResource"
        '
        'txtArchiveFolder
        '
        resources.ApplyResources(Me.txtArchiveFolder, "txtArchiveFolder")
        Me.txtArchiveFolder.Name = "txtArchiveFolder"
        '
        'Label10
        '
        Me.Label10.ForeColor = System.Drawing.Color.Black
        resources.ApplyResources(Me.Label10, "Label10")
        Me.Label10.Name = "Label10"
        '
        'Label11
        '
        Me.Label11.ForeColor = System.Drawing.Color.Black
        resources.ApplyResources(Me.Label11, "Label11")
        Me.Label11.Name = "Label11"
        '
        'btnSwitchToManual
        '
        resources.ApplyResources(Me.btnSwitchToManual, "btnSwitchToManual")
        Me.btnSwitchToManual.Name = "btnSwitchToManual"
        '
        'Label9
        '
        Me.Label9.BackColor = System.Drawing.Color.Transparent
        resources.ApplyResources(Me.Label9, "Label9")
        Me.Label9.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label9.Name = "Label9"
        '
        'btnMenuStrip
        '
        Me.btnMenuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ReleaseToolStripMenuItem})
        Me.btnMenuStrip.Name = "MenuStrip"
        resources.ApplyResources(Me.btnMenuStrip, "MenuStrip")
        '
        'ReleaseToolStripMenuItem
        '
        Me.ReleaseToolStripMenuItem.Image = Global.AutomateUI.My.Resources.ToolImages.Archive_Lock_16x16
        Me.ReleaseToolStripMenuItem.Name = "ReleaseToolStripMenuItem"
        resources.ApplyResources(Me.ReleaseToolStripMenuItem, "ReleaseToolStripMenuItem")
        '
        'ctlSystemArchiving
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.tcSystem)
        Me.Name = "ctlSystemArchiving"
        resources.ApplyResources(Me, "$this")
        Me.tcSystem.ResumeLayout(False)
        Me.tabArchiving.ResumeLayout(False)
        Me.tabArchivingAuto.ResumeLayout(False)
        Me.tabArchivingAuto.PerformLayout()
        Me.btnMenuStrip.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents tcSystem As AutomateControls.SwitchPanel
    Friend WithEvents tabArchiving As System.Windows.Forms.TabPage
    Friend WithEvents btnSwitchToAuto As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents ctlArchivingInterface1 As AutomateUI.ctlArchivingInterface
    Friend WithEvents tabArchivingAuto As System.Windows.Forms.TabPage
    Friend WithEvents btnBrowse As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnUndoChanges As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents txtArchiveAgeNum As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents cmbArchiveAgeUnit As System.Windows.Forms.ComboBox
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents cmbArchiveMode As System.Windows.Forms.ComboBox
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents btnUpdateSettings As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents cmbArchiveResource As System.Windows.Forms.ComboBox
    Friend WithEvents txtArchiveFolder As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents btnSwitchToManual As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents btnMenuStrip As ContextMenuStrip
    Friend WithEvents ReleaseToolStripMenuItem As ToolStripMenuItem
End Class
