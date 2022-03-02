<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmApplication

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmApplication))
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.BottomToolStripPanel = New System.Windows.Forms.ToolStripPanel()
        Me.TopToolStripPanel = New System.Windows.Forms.ToolStripPanel()
        Me.RightToolStripPanel = New System.Windows.Forms.ToolStripPanel()
        Me.LeftToolStripPanel = New System.Windows.Forms.ToolStripPanel()
        Me.statusBar = New System.Windows.Forms.StatusStrip()
        Me.btnSignout = New System.Windows.Forms.ToolStripButton()
        Me.lblStatus = New System.Windows.Forms.ToolStripStatusLabel()
        Me.AlertNotifyIcon = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.AlertNotifyIcon2 = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.btnFile = New AutomateControls.Buttons.FlatStyleStyledButton()
        Me.mnuFile = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.NewToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.OpenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ImportToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ImportProcessObjectToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ImportReleaseSkillToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExportToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExportProcessObjectToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExportNewReleaseToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem1 = New System.Windows.Forms.ToolStripSeparator()
        Me.ConnectionsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem2 = New System.Windows.Forms.ToolStripSeparator()
        Me.ExitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.tcModuleSwitcher = New AutomateControls.DisablingTabControl()
        Me.tpWelcome = New System.Windows.Forms.TabPage()
        Me.tpDesignStudio = New System.Windows.Forms.TabPage()
        Me.tpControlRoom = New System.Windows.Forms.TabPage()
        Me.tpReview = New System.Windows.Forms.TabPage()
        Me.tpReleaseManager = New System.Windows.Forms.TabPage()
        Me.tpDigitalExchange = New System.Windows.Forms.TabPage()
        Me.tpSystemManager = New System.Windows.Forms.TabPage()
        Me.tpMyProfile = New System.Windows.Forms.TabPage()
        Me.mnuMyProfile = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ChangePasswordToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SignOutToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuUsers = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mTaskPanel = New AutomateUI.ctlTaskPanel()
        Me.mnuHelp = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.HelpTopicMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.HelpSeperator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.HelpOpenMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.HelpSeperator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.GuidedTourToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.HelpAPIDocumentationMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.HelpRequestSupportMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.HelpSeparator3 = New System.Windows.Forms.ToolStripSeparator()
        Me.HelpAboutMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.panMain = New System.Windows.Forms.Panel()
        Me.mConnectionChangeWorker = New System.ComponentModel.BackgroundWorker()
        Me.AcknowledgementsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.statusBar.SuspendLayout()
        Me.mnuFile.SuspendLayout()
        Me.tcModuleSwitcher.SuspendLayout()
        Me.mnuMyProfile.SuspendLayout()
        Me.mnuHelp.SuspendLayout()
        Me.SuspendLayout()
        '
        'PictureBox1
        '
        resources.ApplyResources(Me.PictureBox1, "PictureBox1")
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.TabStop = False
        '
        'BottomToolStripPanel
        '
        resources.ApplyResources(Me.BottomToolStripPanel, "BottomToolStripPanel")
        Me.BottomToolStripPanel.Name = "BottomToolStripPanel"
        Me.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal
        Me.BottomToolStripPanel.RowMargin = New System.Windows.Forms.Padding(3, 0, 0, 0)
        '
        'TopToolStripPanel
        '
        resources.ApplyResources(Me.TopToolStripPanel, "TopToolStripPanel")
        Me.TopToolStripPanel.Name = "TopToolStripPanel"
        Me.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal
        Me.TopToolStripPanel.RowMargin = New System.Windows.Forms.Padding(3, 0, 0, 0)
        '
        'RightToolStripPanel
        '
        resources.ApplyResources(Me.RightToolStripPanel, "RightToolStripPanel")
        Me.RightToolStripPanel.Name = "RightToolStripPanel"
        Me.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal
        Me.RightToolStripPanel.RowMargin = New System.Windows.Forms.Padding(3, 0, 0, 0)
        '
        'LeftToolStripPanel
        '
        resources.ApplyResources(Me.LeftToolStripPanel, "LeftToolStripPanel")
        Me.LeftToolStripPanel.Name = "LeftToolStripPanel"
        Me.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal
        Me.LeftToolStripPanel.RowMargin = New System.Windows.Forms.Padding(3, 0, 0, 0)
        '
        'statusBar
        '
        resources.ApplyResources(Me.statusBar, "statusBar")
        Me.statusBar.BackColor = System.Drawing.Color.FromArgb(CType(CType(13, Byte), Integer), CType(CType(42, Byte), Integer), CType(CType(72, Byte), Integer))
        Me.statusBar.ForeColor = System.Drawing.Color.White
        Me.statusBar.GripMargin = New System.Windows.Forms.Padding(0)
        Me.statusBar.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.btnSignout, Me.lblStatus})
        Me.statusBar.Name = "statusBar"
        Me.statusBar.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode
        '
        'btnSignout
        '
        Me.btnSignout.BackColor = System.Drawing.Color.FromArgb(CType(CType(13, Byte), Integer), CType(CType(42, Byte), Integer), CType(CType(72, Byte), Integer))
        Me.btnSignout.ForeColor = System.Drawing.Color.White
        resources.ApplyResources(Me.btnSignout, "btnSignout")
        Me.btnSignout.Margin = New System.Windows.Forms.Padding(-1, 1, 0, 0)
        Me.btnSignout.Name = "btnSignout"
        '
        'lblStatus
        '
        resources.ApplyResources(Me.lblStatus, "lblStatus")
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Spring = True
        '
        'AlertNotifyIcon
        '
        resources.ApplyResources(Me.AlertNotifyIcon, "AlertNotifyIcon")
        '
        'AlertNotifyIcon2
        '
        resources.ApplyResources(Me.AlertNotifyIcon2, "AlertNotifyIcon2")
        '
        'btnFile
        '
        Me.btnFile.BackColor = System.Drawing.Color.FromArgb(CType(CType(13, Byte), Integer), CType(CType(42, Byte), Integer), CType(CType(72, Byte), Integer))
        Me.btnFile.ContextMenuStrip = Me.mnuFile
        Me.btnFile.FlatAppearance.BorderSize = 0
        resources.ApplyResources(Me.btnFile, "btnFile")
        Me.btnFile.ForeColor = System.Drawing.Color.White
        Me.btnFile.Name = "btnFile"
        Me.btnFile.UseVisualStyleBackColor = False
        '
        'mnuFile
        '
        Me.mnuFile.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.NewToolStripMenuItem, Me.OpenToolStripMenuItem, Me.ImportToolStripMenuItem, Me.ExportToolStripMenuItem, Me.ToolStripMenuItem1, Me.ConnectionsToolStripMenuItem, Me.ToolStripMenuItem2, Me.ExitToolStripMenuItem})
        Me.mnuFile.Name = "ContextMenuStrip1"
        resources.ApplyResources(Me.mnuFile, "mnuFile")
        '
        'NewToolStripMenuItem
        '
        Me.NewToolStripMenuItem.Name = "NewToolStripMenuItem"
        resources.ApplyResources(Me.NewToolStripMenuItem, "NewToolStripMenuItem")
        '
        'OpenToolStripMenuItem
        '
        Me.OpenToolStripMenuItem.Name = "OpenToolStripMenuItem"
        resources.ApplyResources(Me.OpenToolStripMenuItem, "OpenToolStripMenuItem")
        '
        'ImportToolStripMenuItem
        '
        Me.ImportToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ImportProcessObjectToolStripMenuItem, Me.ImportReleaseSkillToolStripMenuItem})
        Me.ImportToolStripMenuItem.Name = "ImportToolStripMenuItem"
        resources.ApplyResources(Me.ImportToolStripMenuItem, "ImportToolStripMenuItem")
        '
        'ImportProcessObjectToolStripMenuItem
        '
        Me.ImportProcessObjectToolStripMenuItem.Name = "ImportProcessObjectToolStripMenuItem"
        resources.ApplyResources(Me.ImportProcessObjectToolStripMenuItem, "ImportProcessObjectToolStripMenuItem")
        '
        'ImportReleaseSkillToolStripMenuItem
        '
        Me.ImportReleaseSkillToolStripMenuItem.Name = "ImportReleaseSkillToolStripMenuItem"
        resources.ApplyResources(Me.ImportReleaseSkillToolStripMenuItem, "ImportReleaseSkillToolStripMenuItem")
        '
        'ExportToolStripMenuItem
        '
        Me.ExportToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ExportProcessObjectToolStripMenuItem, Me.ExportNewReleaseToolStripMenuItem})
        Me.ExportToolStripMenuItem.Name = "ExportToolStripMenuItem"
        resources.ApplyResources(Me.ExportToolStripMenuItem, "ExportToolStripMenuItem")
        '
        'ExportProcessObjectToolStripMenuItem
        '
        Me.ExportProcessObjectToolStripMenuItem.Name = "ExportProcessObjectToolStripMenuItem"
        resources.ApplyResources(Me.ExportProcessObjectToolStripMenuItem, "ExportProcessObjectToolStripMenuItem")
        '
        'ExportNewReleaseToolStripMenuItem
        '
        Me.ExportNewReleaseToolStripMenuItem.Name = "ExportNewReleaseToolStripMenuItem"
        resources.ApplyResources(Me.ExportNewReleaseToolStripMenuItem, "ExportNewReleaseToolStripMenuItem")
        '
        'ToolStripMenuItem1
        '
        Me.ToolStripMenuItem1.Name = "ToolStripMenuItem1"
        resources.ApplyResources(Me.ToolStripMenuItem1, "ToolStripMenuItem1")
        '
        'ConnectionsToolStripMenuItem
        '
        Me.ConnectionsToolStripMenuItem.Name = "ConnectionsToolStripMenuItem"
        resources.ApplyResources(Me.ConnectionsToolStripMenuItem, "ConnectionsToolStripMenuItem")
        '
        'ToolStripMenuItem2
        '
        Me.ToolStripMenuItem2.Name = "ToolStripMenuItem2"
        resources.ApplyResources(Me.ToolStripMenuItem2, "ToolStripMenuItem2")
        '
        'ExitToolStripMenuItem
        '
        Me.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        resources.ApplyResources(Me.ExitToolStripMenuItem, "ExitToolStripMenuItem")
        '
        'tcModuleSwitcher
        '
        Me.tcModuleSwitcher.Controls.Add(Me.tpWelcome)
        Me.tcModuleSwitcher.Controls.Add(Me.tpDesignStudio)
        Me.tcModuleSwitcher.Controls.Add(Me.tpControlRoom)
        Me.tcModuleSwitcher.Controls.Add(Me.tpReview)
        Me.tcModuleSwitcher.Controls.Add(Me.tpReleaseManager)
        Me.tcModuleSwitcher.Controls.Add(Me.tpDigitalExchange)
        Me.tcModuleSwitcher.Controls.Add(Me.tpSystemManager)
        Me.tcModuleSwitcher.Controls.Add(Me.tpMyProfile)
        resources.ApplyResources(Me.tcModuleSwitcher, "tcModuleSwitcher")
        Me.tcModuleSwitcher.DrawBorder = False
        Me.tcModuleSwitcher.Name = "tcModuleSwitcher"
        Me.tcModuleSwitcher.SelectedIndex = 0
        Me.tcModuleSwitcher.TabStop = False
        '
        'tpWelcome
        '
        resources.ApplyResources(Me.tpWelcome, "tpWelcome")
        Me.tpWelcome.Name = "tpWelcome"
        Me.tpWelcome.UseVisualStyleBackColor = True
        '
        'tpDesignStudio
        '
        resources.ApplyResources(Me.tpDesignStudio, "tpDesignStudio")
        Me.tpDesignStudio.Name = "tpDesignStudio"
        Me.tpDesignStudio.UseVisualStyleBackColor = True
        '
        'tpControlRoom
        '
        resources.ApplyResources(Me.tpControlRoom, "tpControlRoom")
        Me.tpControlRoom.Name = "tpControlRoom"
        Me.tpControlRoom.UseVisualStyleBackColor = True
        '
        'tpReview
        '
        resources.ApplyResources(Me.tpReview, "tpReview")
        Me.tpReview.Name = "tpReview"
        Me.tpReview.UseVisualStyleBackColor = True
        '
        'tpReleaseManager
        '
        resources.ApplyResources(Me.tpReleaseManager, "tpReleaseManager")
        Me.tpReleaseManager.Name = "tpReleaseManager"
        Me.tpReleaseManager.UseVisualStyleBackColor = True
        '
        'tpDigitalExchange
        '
        resources.ApplyResources(Me.tpDigitalExchange, "tpDigitalExchange")
        Me.tpDigitalExchange.Name = "tpDigitalExchange"
        Me.tpDigitalExchange.UseVisualStyleBackColor = True
        '
        'tpSystemManager
        '
        resources.ApplyResources(Me.tpSystemManager, "tpSystemManager")
        Me.tpSystemManager.Name = "tpSystemManager"
        Me.tpSystemManager.UseVisualStyleBackColor = True
        '
        'tpMyProfile
        '
        Me.tpMyProfile.ContextMenuStrip = Me.mnuMyProfile
        resources.ApplyResources(Me.tpMyProfile, "tpMyProfile")
        Me.tpMyProfile.Name = "tpMyProfile"
        Me.tpMyProfile.UseVisualStyleBackColor = True
        '
        'mnuMyProfile
        '
        Me.mnuMyProfile.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ChangePasswordToolStripMenuItem, Me.SignOutToolStripMenuItem})
        Me.mnuMyProfile.Name = "mnuMyProfile"
        resources.ApplyResources(Me.mnuMyProfile, "mnuMyProfile")
        '
        'ChangePasswordToolStripMenuItem
        '
        Me.ChangePasswordToolStripMenuItem.Name = "ChangePasswordToolStripMenuItem"
        resources.ApplyResources(Me.ChangePasswordToolStripMenuItem, "ChangePasswordToolStripMenuItem")
        '
        'SignOutToolStripMenuItem
        '
        Me.SignOutToolStripMenuItem.Name = "SignOutToolStripMenuItem"
        resources.ApplyResources(Me.SignOutToolStripMenuItem, "SignOutToolStripMenuItem")
        '
        'mnuUsers
        '
        Me.mnuUsers.Name = "ContextMenuStrip1"
        resources.ApplyResources(Me.mnuUsers, "mnuUsers")
        '
        'mTaskPanel
        '
        resources.ApplyResources(Me.mTaskPanel, "mTaskPanel")
        Me.mTaskPanel.BackColor = System.Drawing.SystemColors.Control
        Me.mTaskPanel.BorderColor = System.Drawing.SystemColors.ControlLight
        Me.mTaskPanel.Name = "mTaskPanel"
        Me.mTaskPanel.TabStop = False
        '
        'mnuHelp
        '
        Me.mnuHelp.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.HelpTopicMenuItem, Me.HelpSeperator1, Me.HelpOpenMenuItem, Me.HelpSeperator2, Me.AcknowledgementsToolStripMenuItem, Me.GuidedTourToolStripMenuItem, Me.HelpAPIDocumentationMenuItem, Me.HelpRequestSupportMenuItem, Me.HelpSeparator3, Me.HelpAboutMenuItem})
        Me.mnuHelp.Name = "ContextMenuStrip1"
        resources.ApplyResources(Me.mnuHelp, "mnuHelp")
        '
        'HelpTopicMenuItem
        '
        Me.HelpTopicMenuItem.Name = "HelpTopicMenuItem"
        resources.ApplyResources(Me.HelpTopicMenuItem, "HelpTopicMenuItem")
        '
        'HelpSeperator1
        '
        Me.HelpSeperator1.Name = "HelpSeperator1"
        resources.ApplyResources(Me.HelpSeperator1, "HelpSeperator1")
        '
        'HelpOpenMenuItem
        '
        Me.HelpOpenMenuItem.Name = "HelpOpenMenuItem"
        resources.ApplyResources(Me.HelpOpenMenuItem, "HelpOpenMenuItem")
        '
        'HelpSeperator2
        '
        Me.HelpSeperator2.Name = "HelpSeperator2"
        resources.ApplyResources(Me.HelpSeperator2, "HelpSeperator2")
        '
        'GuidedTourToolStripMenuItem
        '
        resources.ApplyResources(Me.GuidedTourToolStripMenuItem, "GuidedTourToolStripMenuItem")
        Me.GuidedTourToolStripMenuItem.Name = "GuidedTourToolStripMenuItem"
        '
        'HelpAPIDocumentationMenuItem
        '
        resources.ApplyResources(Me.HelpAPIDocumentationMenuItem, "HelpAPIDocumentationMenuItem")
        Me.HelpAPIDocumentationMenuItem.Name = "HelpAPIDocumentationMenuItem"
        '
        'HelpRequestSupportMenuItem
        '
        Me.HelpRequestSupportMenuItem.Name = "HelpRequestSupportMenuItem"
        resources.ApplyResources(Me.HelpRequestSupportMenuItem, "HelpRequestSupportMenuItem")
        '
        'HelpSeparator3
        '
        Me.HelpSeparator3.Name = "HelpSeparator3"
        resources.ApplyResources(Me.HelpSeparator3, "HelpSeparator3")
        '
        'HelpAboutMenuItem
        '
        Me.HelpAboutMenuItem.Name = "HelpAboutMenuItem"
        resources.ApplyResources(Me.HelpAboutMenuItem, "HelpAboutMenuItem")
        '
        'panMain
        '
        resources.ApplyResources(Me.panMain, "panMain")
        Me.panMain.Name = "panMain"
        '
        'mConnectionChangeWorker
        '
        Me.mConnectionChangeWorker.WorkerSupportsCancellation = True
        '
        'AcknowledgementsToolStripMenuItem
        '
        Me.AcknowledgementsToolStripMenuItem.Name = "AcknowledgementsToolStripMenuItem"
        resources.ApplyResources(Me.AcknowledgementsToolStripMenuItem, "AcknowledgementsToolStripMenuItem")
        '
        'frmApplication
        '
        Me.BackColor = System.Drawing.SystemColors.Window
        resources.ApplyResources(Me, "$this")
        Me.ContextMenuStrip = Me.mnuMyProfile
        Me.Controls.Add(Me.panMain)
        Me.Controls.Add(Me.tcModuleSwitcher)
        Me.Controls.Add(Me.btnFile)
        Me.Controls.Add(Me.mTaskPanel)
        Me.Controls.Add(Me.statusBar)
        Me.ForeColor = System.Drawing.SystemColors.WindowText
        Me.HelpButton = True
        Me.Name = "frmApplication"
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.statusBar.ResumeLayout(False)
        Me.statusBar.PerformLayout()
        Me.mnuFile.ResumeLayout(False)
        Me.tcModuleSwitcher.ResumeLayout(False)
        Me.mnuMyProfile.ResumeLayout(False)
        Me.mnuHelp.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Private WithEvents statusBar As System.Windows.Forms.StatusStrip
    Private WithEvents lblStatus As System.Windows.Forms.ToolStripStatusLabel
    Private WithEvents mTaskPanel As AutomateUI.ctlTaskPanel
    Friend WithEvents AlertNotifyIcon As System.Windows.Forms.NotifyIcon
    Friend WithEvents AlertNotifyIcon2 As System.Windows.Forms.NotifyIcon
    Friend WithEvents BottomToolStripPanel As System.Windows.Forms.ToolStripPanel
    Friend WithEvents TopToolStripPanel As System.Windows.Forms.ToolStripPanel
    Friend WithEvents RightToolStripPanel As System.Windows.Forms.ToolStripPanel
    Friend WithEvents LeftToolStripPanel As System.Windows.Forms.ToolStripPanel
    Friend WithEvents btnSignout As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnFile As AutomateControls.Buttons.FlatStyleStyledButton
    Friend WithEvents tcModuleSwitcher As AutomateControls.DisablingTabControl
    Friend WithEvents tpWelcome As System.Windows.Forms.TabPage
    Friend WithEvents tpDesignStudio As System.Windows.Forms.TabPage
    Friend WithEvents tpReleaseManager As System.Windows.Forms.TabPage
    Friend WithEvents tpControlRoom As System.Windows.Forms.TabPage
    Friend WithEvents mnuFile As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents NewToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OpenToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ImportToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ExportToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ExportProcessObjectToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ExportNewReleaseToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ConnectionsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ExitToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuUsers As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents mnuHelp As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents HelpOpenMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents HelpSeparator3 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents HelpAboutMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tpReview As System.Windows.Forms.TabPage
    Private WithEvents panMain As System.Windows.Forms.Panel
    Friend WithEvents HelpTopicMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents HelpSeperator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents HelpSeperator2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents HelpAPIDocumentationMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents HelpRequestSupportMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tpSystemManager As System.Windows.Forms.TabPage
    Private WithEvents mConnectionChangeWorker As System.ComponentModel.BackgroundWorker
    Friend WithEvents GuidedTourToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents tpDigitalExchange As TabPage
    Friend WithEvents tpMyProfile As TabPage
    Friend WithEvents mnuMyProfile As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents ChangePasswordToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SignOutToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ImportReleaseSkillToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ImportProcessObjectToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AcknowledgementsToolStripMenuItem As ToolStripMenuItem
End Class
