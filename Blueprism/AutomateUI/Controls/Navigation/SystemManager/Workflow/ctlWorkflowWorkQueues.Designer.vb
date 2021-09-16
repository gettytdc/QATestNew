<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlWorkflowWorkQueues
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
        Dim SplitContainer1 As System.Windows.Forms.SplitContainer
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlWorkflowWorkQueues))
        Dim Panel1 As System.Windows.Forms.Panel
        Dim Label7 As System.Windows.Forms.Label
        Dim lblMaxAttempts As System.Windows.Forms.Label
        Dim Label5 As System.Windows.Forms.Label
        Dim Label4 As System.Windows.Forms.Label
        Dim Label3 As System.Windows.Forms.Label
        Dim Label9 As System.Windows.Forms.Label
        Dim Label8 As System.Windows.Forms.Label
        Me.tvQueueGroups = New AutomateUI.GroupTreeControl()
        Me.btnNewQueue = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnDeleteQueue = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.Label1 = New System.Windows.Forms.Label()
        Me.panQueueDetail = New System.Windows.Forms.Panel()
        Me.cbRetryQueueException = New System.Windows.Forms.CheckBox()
        Me.flpConfigQueueInfo = New System.Windows.Forms.FlowLayoutPanel()
        Me.pbInfo = New System.Windows.Forms.PictureBox()
        Me.tbInfo = New AutomateControls.Textboxes.StyledTextBox()
        Me.lnkReferences = New AutomateControls.BulletedLinkLabel()
        Me.lnkSetPerms = New AutomateControls.BulletedLinkLabel()
        Me.cbActiveQueue = New System.Windows.Forms.CheckBox()
        Me.cmbQueueKey = New AutomateControls.StyledComboBox()
        Me.lblEncryptedUsing = New System.Windows.Forms.Label()
        Me.cbEncrypted = New System.Windows.Forms.CheckBox()
        Me.numMaxAttempts = New AutomateControls.StyledNumericUpDown()
        Me.btnApply = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.txtKeyName = New AutomateControls.Textboxes.StyledTextBox()
        Me.lnkToggleQueueStatus = New AutomateControls.BulletedLinkLabel()
        Me.txtQueueStatus = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtQueueName = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblKeyNameHint = New System.Windows.Forms.Label()
        Me.lblQueueNameHint = New System.Windows.Forms.Label()
        Me.gpActiveQueues = New System.Windows.Forms.GroupBox()
        Me.cmbResourceGroup = New AutomateUI.GroupComboBox()
        Me.cmbProcess = New AutomateUI.GroupMemberComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Panel1 = New System.Windows.Forms.Panel()
        Label7 = New System.Windows.Forms.Label()
        lblMaxAttempts = New System.Windows.Forms.Label()
        Label5 = New System.Windows.Forms.Label()
        Label4 = New System.Windows.Forms.Label()
        Label3 = New System.Windows.Forms.Label()
        Label9 = New System.Windows.Forms.Label()
        Label8 = New System.Windows.Forms.Label()
        CType(SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        SplitContainer1.Panel1.SuspendLayout()
        SplitContainer1.Panel2.SuspendLayout()
        SplitContainer1.SuspendLayout()
        Panel1.SuspendLayout()
        Me.panQueueDetail.SuspendLayout()
        Me.flpConfigQueueInfo.SuspendLayout()
        CType(Me.pbInfo, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numMaxAttempts, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.gpActiveQueues.SuspendLayout()
        Me.SuspendLayout()
        '
        'SplitContainer1
        '
        resources.ApplyResources(SplitContainer1, "SplitContainer1")
        SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        SplitContainer1.Panel1.Controls.Add(Me.tvQueueGroups)
        SplitContainer1.Panel1.Controls.Add(Panel1)
        SplitContainer1.Panel1.Controls.Add(Me.Label1)
        resources.ApplyResources(SplitContainer1.Panel1, "SplitContainer1.Panel1")
        '
        'SplitContainer1.Panel2
        '
        SplitContainer1.Panel2.Controls.Add(Me.panQueueDetail)
        SplitContainer1.Panel2.Controls.Add(Me.Label2)
        resources.ApplyResources(SplitContainer1.Panel2, "SplitContainer1.Panel2")
        SplitContainer1.TabStop = False
        '
        'tvQueueGroups
        '
        Me.tvQueueGroups.CloneDragEnabled = False
        resources.ApplyResources(Me.tvQueueGroups, "tvQueueGroups")
        Me.tvQueueGroups.Name = "tvQueueGroups"
        Me.tvQueueGroups.SelectedNodes = CType(resources.GetObject("tvQueueGroups.SelectedNodes"), System.Collections.Generic.List(Of System.Windows.Forms.TreeNode))
        '
        'Panel1
        '
        Panel1.Controls.Add(Me.btnNewQueue)
        Panel1.Controls.Add(Me.btnDeleteQueue)
        resources.ApplyResources(Panel1, "Panel1")
        Panel1.Name = "Panel1"
        '
        'btnNewQueue
        '
        resources.ApplyResources(Me.btnNewQueue, "btnNewQueue")
        Me.btnNewQueue.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.btnNewQueue.Name = "btnNewQueue"
        Me.btnNewQueue.UseVisualStyleBackColor = False
        '
        'btnDeleteQueue
        '
        resources.ApplyResources(Me.btnDeleteQueue, "btnDeleteQueue")
        Me.btnDeleteQueue.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.btnDeleteQueue.Name = "btnDeleteQueue"
        Me.btnDeleteQueue.UseVisualStyleBackColor = False
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Name = "Label1"
        '
        'panQueueDetail
        '
        Me.panQueueDetail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.panQueueDetail.Controls.Add(Me.cbRetryQueueException)
        Me.panQueueDetail.Controls.Add(Me.flpConfigQueueInfo)
        Me.panQueueDetail.Controls.Add(Me.lnkReferences)
        Me.panQueueDetail.Controls.Add(Me.lnkSetPerms)
        Me.panQueueDetail.Controls.Add(Me.cbActiveQueue)
        Me.panQueueDetail.Controls.Add(Me.cmbQueueKey)
        Me.panQueueDetail.Controls.Add(Me.lblEncryptedUsing)
        Me.panQueueDetail.Controls.Add(Me.cbEncrypted)
        Me.panQueueDetail.Controls.Add(Me.numMaxAttempts)
        Me.panQueueDetail.Controls.Add(Label7)
        Me.panQueueDetail.Controls.Add(lblMaxAttempts)
        Me.panQueueDetail.Controls.Add(Me.btnApply)
        Me.panQueueDetail.Controls.Add(Me.txtKeyName)
        Me.panQueueDetail.Controls.Add(Me.lnkToggleQueueStatus)
        Me.panQueueDetail.Controls.Add(Me.txtQueueStatus)
        Me.panQueueDetail.Controls.Add(Me.txtQueueName)
        Me.panQueueDetail.Controls.Add(Me.lblKeyNameHint)
        Me.panQueueDetail.Controls.Add(Me.lblQueueNameHint)
        Me.panQueueDetail.Controls.Add(Label5)
        Me.panQueueDetail.Controls.Add(Label4)
        Me.panQueueDetail.Controls.Add(Label3)
        Me.panQueueDetail.Controls.Add(Me.gpActiveQueues)
        resources.ApplyResources(Me.panQueueDetail, "panQueueDetail")
        Me.panQueueDetail.Name = "panQueueDetail"
        '
        'cbRetryQueueException
        '
        resources.ApplyResources(Me.cbRetryQueueException, "cbRetryQueueException")
        Me.cbRetryQueueException.Name = "cbRetryQueueException"
        '
        'flpConfigQueueInfo
        '
        Me.flpConfigQueueInfo.Controls.Add(Me.pbInfo)
        Me.flpConfigQueueInfo.Controls.Add(Me.tbInfo)
        resources.ApplyResources(Me.flpConfigQueueInfo, "flpConfigQueueInfo")
        Me.flpConfigQueueInfo.Name = "flpConfigQueueInfo"
        '
        'pbInfo
        '
        Me.pbInfo.Image = Global.AutomateUI.My.Resources.ToolImages.Information_16x16_Hot
        resources.ApplyResources(Me.pbInfo, "pbInfo")
        Me.pbInfo.Name = "pbInfo"
        Me.pbInfo.TabStop = False
        '
        'tbInfo
        '
        Me.tbInfo.BackColor = System.Drawing.SystemColors.Window
        Me.tbInfo.BorderColor = System.Drawing.Color.Empty
        Me.tbInfo.BorderStyle = System.Windows.Forms.BorderStyle.None
        resources.ApplyResources(Me.tbInfo, "tbInfo")
        Me.tbInfo.Name = "tbInfo"
        Me.tbInfo.ReadOnly = True
        '
        'lnkReferences
        '
        resources.ApplyResources(Me.lnkReferences, "lnkReferences")
        Me.lnkReferences.BackColor = System.Drawing.Color.Transparent
        Me.lnkReferences.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.lnkReferences.Name = "lnkReferences"
        Me.lnkReferences.TabStop = True
        '
        'lnkSetPerms
        '
        resources.ApplyResources(Me.lnkSetPerms, "lnkSetPerms")
        Me.lnkSetPerms.BackColor = System.Drawing.Color.Transparent
        Me.lnkSetPerms.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.lnkSetPerms.Name = "lnkSetPerms"
        Me.lnkSetPerms.TabStop = True
        '
        'cbActiveQueue
        '
        resources.ApplyResources(Me.cbActiveQueue, "cbActiveQueue")
        Me.cbActiveQueue.Name = "cbActiveQueue"
        '
        'cmbQueueKey
        '
        resources.ApplyResources(Me.cmbQueueKey, "cmbQueueKey")
        Me.cmbQueueKey.Checkable = False
        Me.cmbQueueKey.DisplayMember = "Value"
        Me.cmbQueueKey.FormattingEnabled = True
        Me.cmbQueueKey.Items.AddRange(New Object() {resources.GetString("cmbQueueKey.Items"), resources.GetString("cmbQueueKey.Items1"), resources.GetString("cmbQueueKey.Items2"), resources.GetString("cmbQueueKey.Items3")})
        Me.cmbQueueKey.Name = "cmbQueueKey"
        Me.cmbQueueKey.ValueMember = "Value"
        '
        'lblEncryptedUsing
        '
        resources.ApplyResources(Me.lblEncryptedUsing, "lblEncryptedUsing")
        Me.lblEncryptedUsing.BackColor = System.Drawing.Color.Transparent
        Me.lblEncryptedUsing.ForeColor = System.Drawing.Color.Black
        Me.lblEncryptedUsing.Name = "lblEncryptedUsing"
        '
        'cbEncrypted
        '
        resources.ApplyResources(Me.cbEncrypted, "cbEncrypted")
        Me.cbEncrypted.Name = "cbEncrypted"
        '
        'numMaxAttempts
        '
        resources.ApplyResources(Me.numMaxAttempts, "numMaxAttempts")
        Me.numMaxAttempts.Maximum = New Decimal(New Integer() {999999, 0, 0, 0})
        Me.numMaxAttempts.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.numMaxAttempts.Name = "numMaxAttempts"
        Me.numMaxAttempts.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'Label7
        '
        resources.ApplyResources(Label7, "Label7")
        Label7.BackColor = System.Drawing.Color.Transparent
        Label7.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Label7.Name = "Label7"
        '
        'lblMaxAttempts
        '
        resources.ApplyResources(lblMaxAttempts, "lblMaxAttempts")
        lblMaxAttempts.BackColor = System.Drawing.Color.Transparent
        lblMaxAttempts.ForeColor = System.Drawing.Color.Black
        lblMaxAttempts.Name = "lblMaxAttempts"
        '
        'btnApply
        '
        resources.ApplyResources(Me.btnApply, "btnApply")
        Me.btnApply.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.btnApply.Name = "btnApply"
        Me.btnApply.UseVisualStyleBackColor = False
        '
        'txtKeyName
        '
        resources.ApplyResources(Me.txtKeyName, "txtKeyName")
        Me.txtKeyName.BorderColor = System.Drawing.Color.Empty
        Me.txtKeyName.Name = "txtKeyName"
        '
        'lnkToggleQueueStatus
        '
        resources.ApplyResources(Me.lnkToggleQueueStatus, "lnkToggleQueueStatus")
        Me.lnkToggleQueueStatus.BackColor = System.Drawing.Color.Transparent
        Me.lnkToggleQueueStatus.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.lnkToggleQueueStatus.Name = "lnkToggleQueueStatus"
        Me.lnkToggleQueueStatus.TabStop = True
        '
        'txtQueueStatus
        '
        resources.ApplyResources(Me.txtQueueStatus, "txtQueueStatus")
        Me.txtQueueStatus.BorderColor = System.Drawing.Color.Empty
        Me.txtQueueStatus.Name = "txtQueueStatus"
        Me.txtQueueStatus.ReadOnly = True
        '
        'txtQueueName
        '
        resources.ApplyResources(Me.txtQueueName, "txtQueueName")
        Me.txtQueueName.BorderColor = System.Drawing.Color.Empty
        Me.txtQueueName.Name = "txtQueueName"
        '
        'lblKeyNameHint
        '
        resources.ApplyResources(Me.lblKeyNameHint, "lblKeyNameHint")
        Me.lblKeyNameHint.BackColor = System.Drawing.Color.Transparent
        Me.lblKeyNameHint.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.lblKeyNameHint.Name = "lblKeyNameHint"
        '
        'lblQueueNameHint
        '
        resources.ApplyResources(Me.lblQueueNameHint, "lblQueueNameHint")
        Me.lblQueueNameHint.BackColor = System.Drawing.Color.Transparent
        Me.lblQueueNameHint.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.lblQueueNameHint.Name = "lblQueueNameHint"
        '
        'Label5
        '
        resources.ApplyResources(Label5, "Label5")
        Label5.BackColor = System.Drawing.Color.Transparent
        Label5.ForeColor = System.Drawing.Color.Black
        Label5.Name = "Label5"
        '
        'Label4
        '
        Label4.BackColor = System.Drawing.Color.Transparent
        resources.ApplyResources(Label4, "Label4")
        Label4.ForeColor = System.Drawing.Color.Black
        Label4.Name = "Label4"
        '
        'Label3
        '
        resources.ApplyResources(Label3, "Label3")
        Label3.BackColor = System.Drawing.Color.Transparent
        Label3.ForeColor = System.Drawing.Color.Black
        Label3.Name = "Label3"
        '
        'gpActiveQueues
        '
        resources.ApplyResources(Me.gpActiveQueues, "gpActiveQueues")
        Me.gpActiveQueues.Controls.Add(Me.cmbResourceGroup)
        Me.gpActiveQueues.Controls.Add(Me.cmbProcess)
        Me.gpActiveQueues.Controls.Add(Label9)
        Me.gpActiveQueues.Controls.Add(Label8)
        Me.gpActiveQueues.Name = "gpActiveQueues"
        Me.gpActiveQueues.TabStop = False
        '
        'cmbResourceGroup
        '
        resources.ApplyResources(Me.cmbResourceGroup, "cmbResourceGroup")
        Me.cmbResourceGroup.Checkable = False
        Me.cmbResourceGroup.FormattingEnabled = True
        Me.cmbResourceGroup.Name = "cmbResourceGroup"
        Me.cmbResourceGroup.SelectedItemsXML = "<GroupComboBox><SelectedItems /></GroupComboBox>"
        Me.cmbResourceGroup.ShowEmptyGroups = True
        Me.cmbResourceGroup.TreeType = BluePrism.AutomateAppCore.Groups.GroupTreeType.Resources
        '
        'cmbProcess
        '
        resources.ApplyResources(Me.cmbProcess, "cmbProcess")
        Me.cmbProcess.Checkable = False
        Me.cmbProcess.FormattingEnabled = True
        Me.cmbProcess.Name = "cmbProcess"
        Me.cmbProcess.SelectedItemsXML = "<GroupComboBox><SelectedItems /></GroupComboBox>"
        Me.cmbProcess.TreeType = BluePrism.AutomateAppCore.Groups.GroupTreeType.Processes
        '
        'Label9
        '
        resources.ApplyResources(Label9, "Label9")
        Label9.BackColor = System.Drawing.Color.Transparent
        Label9.ForeColor = System.Drawing.Color.Black
        Label9.Name = "Label9"
        '
        'Label8
        '
        resources.ApplyResources(Label8, "Label8")
        Label8.BackColor = System.Drawing.Color.Transparent
        Label8.ForeColor = System.Drawing.Color.Black
        Label8.Name = "Label8"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.ForeColor = System.Drawing.Color.Black
        Me.Label2.Name = "Label2"
        '
        'ctlWorkflowWorkQueues
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(SplitContainer1)
        Me.Name = "ctlWorkflowWorkQueues"
        resources.ApplyResources(Me, "$this")
        SplitContainer1.Panel1.ResumeLayout(False)
        SplitContainer1.Panel1.PerformLayout()
        SplitContainer1.Panel2.ResumeLayout(False)
        SplitContainer1.Panel2.PerformLayout()
        CType(SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        SplitContainer1.ResumeLayout(False)
        Panel1.ResumeLayout(False)
        Me.panQueueDetail.ResumeLayout(False)
        Me.panQueueDetail.PerformLayout()
        Me.flpConfigQueueInfo.ResumeLayout(False)
        Me.flpConfigQueueInfo.PerformLayout()
        CType(Me.pbInfo, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numMaxAttempts, System.ComponentModel.ISupportInitialize).EndInit()
        Me.gpActiveQueues.ResumeLayout(False)
        Me.gpActiveQueues.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents btnNewQueue As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents tvQueueGroups As AutomateUI.GroupTreeControl
    Private WithEvents Label1 As System.Windows.Forms.Label
    Private WithEvents panQueueDetail As System.Windows.Forms.Panel
    Private WithEvents lnkSetPerms As AutomateControls.BulletedLinkLabel
    Private WithEvents cmbQueueKey As AutomateControls.StyledComboBox
    Private WithEvents lblEncryptedUsing As System.Windows.Forms.Label
    Private WithEvents cbEncrypted As System.Windows.Forms.CheckBox
    Private WithEvents numMaxAttempts As AutomateControls.StyledNumericUpDown
    Private WithEvents btnApply As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents txtKeyName As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents lnkToggleQueueStatus As AutomateControls.BulletedLinkLabel
    Private WithEvents txtQueueStatus As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents txtQueueName As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents lblKeyNameHint As System.Windows.Forms.Label
    Private WithEvents lblQueueNameHint As System.Windows.Forms.Label
    Private WithEvents Label2 As System.Windows.Forms.Label
    Private WithEvents btnDeleteQueue As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents gpActiveQueues As System.Windows.Forms.GroupBox
    Private WithEvents cbActiveQueue As System.Windows.Forms.CheckBox
    Private WithEvents cmbProcess As AutomateUI.GroupMemberComboBox
    Private WithEvents cmbResourceGroup As AutomateUI.GroupComboBox
    Private WithEvents lnkReferences As AutomateControls.BulletedLinkLabel
    Friend WithEvents flpConfigQueueInfo As FlowLayoutPanel
    Friend WithEvents pbInfo As PictureBox
    Friend WithEvents tbInfo As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents cbRetryQueueException As CheckBox
End Class
