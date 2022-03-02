<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.DataGridView1 = New System.Windows.Forms.DataGridView()
        Me.colParameter = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colValue = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colBtn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.cmbApplicationSubType = New System.Windows.Forms.ComboBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.cmbApplicationType = New System.Windows.Forms.ComboBox()
        Me.btnLaunch = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btnDiscover = New System.Windows.Forms.Button()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AdvancedToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.FontEditorToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SettingsCaptureToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DocumentGeneratorToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuAppManConfig = New System.Windows.Forms.ToolStripMenuItem()
        Me.TerminalTestToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.LoadSnapshotToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.GenerateSupportInformationToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.HelpToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AboutToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.btnDisconnect = New System.Windows.Forms.Button()
        Me.btnInspect = New System.Windows.Forms.Button()
        Me.btnSnapshot = New System.Windows.Forms.Button()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.GroupBox1.SuspendLayout()
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.MenuStrip1.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        resources.ApplyResources(Me.GroupBox1, "GroupBox1")
        Me.GroupBox1.Controls.Add(Me.DataGridView1)
        Me.GroupBox1.Controls.Add(Me.cmbApplicationSubType)
        Me.GroupBox1.Controls.Add(Me.Label4)
        Me.GroupBox1.Controls.Add(Me.cmbApplicationType)
        Me.GroupBox1.Controls.Add(Me.btnLaunch)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.TabStop = False
        '
        'DataGridView1
        '
        Me.DataGridView1.AllowUserToAddRows = False
        Me.DataGridView1.AllowUserToDeleteRows = False
        resources.ApplyResources(Me.DataGridView1, "DataGridView1")
        Me.DataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.DataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells
        Me.DataGridView1.BackgroundColor = System.Drawing.SystemColors.Control
        Me.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView1.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colParameter, Me.colValue, Me.colBtn})
        Me.DataGridView1.MultiSelect = False
        Me.DataGridView1.Name = "DataGridView1"
        '
        'colParameter
        '
        resources.ApplyResources(Me.colParameter, "colParameter")
        Me.colParameter.Name = "colParameter"
        Me.colParameter.ReadOnly = True
        '
        'colValue
        '
        resources.ApplyResources(Me.colValue, "colValue")
        Me.colValue.Name = "colValue"
        '
        'colBtn
        '
        Me.colBtn.FillWeight = 10.0!
        resources.ApplyResources(Me.colBtn, "colBtn")
        Me.colBtn.Name = "colBtn"
        '
        'cmbApplicationSubType
        '
        resources.ApplyResources(Me.cmbApplicationSubType, "cmbApplicationSubType")
        Me.cmbApplicationSubType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbApplicationSubType.FormattingEnabled = True
        Me.cmbApplicationSubType.Name = "cmbApplicationSubType"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'cmbApplicationType
        '
        resources.ApplyResources(Me.cmbApplicationType, "cmbApplicationType")
        Me.cmbApplicationType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbApplicationType.FormattingEnabled = True
        Me.cmbApplicationType.Name = "cmbApplicationType"
        '
        'btnLaunch
        '
        resources.ApplyResources(Me.btnLaunch, "btnLaunch")
        Me.btnLaunch.Name = "btnLaunch"
        Me.ToolTip1.SetToolTip(Me.btnLaunch, resources.GetString("btnLaunch.ToolTip"))
        Me.btnLaunch.UseVisualStyleBackColor = True
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'btnDiscover
        '
        resources.ApplyResources(Me.btnDiscover, "btnDiscover")
        Me.btnDiscover.Name = "btnDiscover"
        Me.ToolTip1.SetToolTip(Me.btnDiscover, resources.GetString("btnDiscover.ToolTip"))
        Me.btnDiscover.UseVisualStyleBackColor = True
        '
        'MenuStrip1
        '
        Me.MenuStrip1.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem, Me.ToolsToolStripMenuItem, Me.HelpToolStripMenuItem})
        resources.ApplyResources(Me.MenuStrip1, "MenuStrip1")
        Me.MenuStrip1.Name = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ExitToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        resources.ApplyResources(Me.FileToolStripMenuItem, "FileToolStripMenuItem")
        '
        'ExitToolStripMenuItem
        '
        Me.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        resources.ApplyResources(Me.ExitToolStripMenuItem, "ExitToolStripMenuItem")
        '
        'ToolsToolStripMenuItem
        '
        Me.ToolsToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.AdvancedToolStripMenuItem, Me.FontEditorToolStripMenuItem, Me.SettingsCaptureToolStripMenuItem, Me.DocumentGeneratorToolStripMenuItem, Me.mnuAppManConfig, Me.TerminalTestToolStripMenuItem, Me.LoadSnapshotToolStripMenuItem, Me.GenerateSupportInformationToolStripMenuItem})
        Me.ToolsToolStripMenuItem.Name = "ToolsToolStripMenuItem"
        resources.ApplyResources(Me.ToolsToolStripMenuItem, "ToolsToolStripMenuItem")
        '
        'AdvancedToolStripMenuItem
        '
        Me.AdvancedToolStripMenuItem.Name = "AdvancedToolStripMenuItem"
        resources.ApplyResources(Me.AdvancedToolStripMenuItem, "AdvancedToolStripMenuItem")
        '
        'FontEditorToolStripMenuItem
        '
        Me.FontEditorToolStripMenuItem.Name = "FontEditorToolStripMenuItem"
        resources.ApplyResources(Me.FontEditorToolStripMenuItem, "FontEditorToolStripMenuItem")
        '
        'SettingsCaptureToolStripMenuItem
        '
        Me.SettingsCaptureToolStripMenuItem.Name = "SettingsCaptureToolStripMenuItem"
        resources.ApplyResources(Me.SettingsCaptureToolStripMenuItem, "SettingsCaptureToolStripMenuItem")
        '
        'DocumentGeneratorToolStripMenuItem
        '
        Me.DocumentGeneratorToolStripMenuItem.Name = "DocumentGeneratorToolStripMenuItem"
        resources.ApplyResources(Me.DocumentGeneratorToolStripMenuItem, "DocumentGeneratorToolStripMenuItem")
        '
        'mnuAppManConfig
        '
        Me.mnuAppManConfig.Name = "mnuAppManConfig"
        resources.ApplyResources(Me.mnuAppManConfig, "mnuAppManConfig")
        '
        'TerminalTestToolStripMenuItem
        '
        Me.TerminalTestToolStripMenuItem.Name = "TerminalTestToolStripMenuItem"
        resources.ApplyResources(Me.TerminalTestToolStripMenuItem, "TerminalTestToolStripMenuItem")
        '
        'LoadSnapshotToolStripMenuItem
        '
        Me.LoadSnapshotToolStripMenuItem.Name = "LoadSnapshotToolStripMenuItem"
        resources.ApplyResources(Me.LoadSnapshotToolStripMenuItem, "LoadSnapshotToolStripMenuItem")
        '
        'GenerateSupportInformationToolStripMenuItem
        '
        Me.GenerateSupportInformationToolStripMenuItem.Name = "GenerateSupportInformationToolStripMenuItem"
        resources.ApplyResources(Me.GenerateSupportInformationToolStripMenuItem, "GenerateSupportInformationToolStripMenuItem")
        '
        'HelpToolStripMenuItem
        '
        Me.HelpToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.AboutToolStripMenuItem})
        Me.HelpToolStripMenuItem.Name = "HelpToolStripMenuItem"
        resources.ApplyResources(Me.HelpToolStripMenuItem, "HelpToolStripMenuItem")
        '
        'AboutToolStripMenuItem
        '
        Me.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem"
        resources.ApplyResources(Me.AboutToolStripMenuItem, "AboutToolStripMenuItem")
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.FileName = "OpenFileDialog1"
        '
        'btnDisconnect
        '
        resources.ApplyResources(Me.btnDisconnect, "btnDisconnect")
        Me.btnDisconnect.Name = "btnDisconnect"
        Me.ToolTip1.SetToolTip(Me.btnDisconnect, resources.GetString("btnDisconnect.ToolTip"))
        Me.btnDisconnect.UseVisualStyleBackColor = True
        '
        'btnInspect
        '
        resources.ApplyResources(Me.btnInspect, "btnInspect")
        Me.btnInspect.Name = "btnInspect"
        Me.ToolTip1.SetToolTip(Me.btnInspect, resources.GetString("btnInspect.ToolTip"))
        Me.btnInspect.UseVisualStyleBackColor = True
        '
        'btnSnapshot
        '
        resources.ApplyResources(Me.btnSnapshot, "btnSnapshot")
        Me.btnSnapshot.Name = "btnSnapshot"
        Me.ToolTip1.SetToolTip(Me.btnSnapshot, resources.GetString("btnSnapshot.ToolTip"))
        Me.btnSnapshot.UseVisualStyleBackColor = True
        '
        'StatusStrip1
        '
        Me.StatusStrip1.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripStatusLabel1})
        resources.ApplyResources(Me.StatusStrip1, "StatusStrip1")
        Me.StatusStrip1.Name = "StatusStrip1"
        '
        'ToolStripStatusLabel1
        '
        Me.ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        resources.ApplyResources(Me.ToolStripStatusLabel1, "ToolStripStatusLabel1")
        '
        'GroupBox3
        '
        resources.ApplyResources(Me.GroupBox3, "GroupBox3")
        Me.GroupBox3.Controls.Add(Me.lblStatus)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.TabStop = False
        '
        'lblStatus
        '
        resources.ApplyResources(Me.lblStatus, "lblStatus")
        Me.lblStatus.Name = "lblStatus"
        '
        'frmMain
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.btnSnapshot)
        Me.Controls.Add(Me.btnInspect)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.btnDisconnect)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.btnDiscover)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.MenuStrip1)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "frmMain"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents btnDiscover As System.Windows.Forms.Button
    Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
    Friend WithEvents FileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ExitToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AdvancedToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OpenFileDialog1 As System.Windows.Forms.OpenFileDialog
    Friend WithEvents btnLaunch As System.Windows.Forms.Button
    Friend WithEvents cmbApplicationType As System.Windows.Forms.ComboBox
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents FontEditorToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents btnDisconnect As System.Windows.Forms.Button
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents ToolStripStatusLabel1 As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents DocumentGeneratorToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents HelpToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AboutToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SettingsCaptureToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuAppManConfig As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TerminalTestToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents btnInspect As System.Windows.Forms.Button
    Friend WithEvents btnSnapshot As System.Windows.Forms.Button
    Friend WithEvents cmbApplicationSubType As System.Windows.Forms.ComboBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents DataGridView1 As System.Windows.Forms.DataGridView
    Friend WithEvents colParameter As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colValue As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colBtn As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents LoadSnapshotToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents GenerateSupportInformationToolStripMenuItem As ToolStripMenuItem
End Class
