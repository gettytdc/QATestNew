<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlTask
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
        Dim Label29 As System.Windows.Forms.Label
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlTask))
        Dim Label31 As System.Windows.Forms.Label
        Dim Label5 As System.Windows.Forms.Label
        Dim tabCompleteException As System.Windows.Forms.TableLayoutPanel
        Dim TreeListViewItemCollectionComparer3 As AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer = New AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer()
        Dim TreeListViewItemCollectionComparer1 As AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer = New AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer()
        Me.PostCompletionDelayNumericInput = New AutomateControls.StyledNumericUpDown()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.comboOnException = New System.Windows.Forms.ComboBox()
        Me.txtOnException = New AutomateControls.Textboxes.StyledTextBox()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.comboOnCompletion = New System.Windows.Forms.ComboBox()
        Me.txtOnCompletion = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.cbFailFast = New System.Windows.Forms.CheckBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtName = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtDescription = New AutomateControls.Textboxes.StyledTextBox()
        Me.splitProcessResourcePanel = New AutomateControls.SplitContainers.HighlightingSplitContainer()
        Me.splitActivities = New AutomateControls.SplitContainers.HighlightingSplitContainer()
        Me.listSessions = New AutomateControls.FlickerFreeListView()
        Me.columnStatus = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.columnProcesses = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.columnResources = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.menuSession = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.DeleteToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.StartupParametersToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ttFailOnErrorHelp = New System.Windows.Forms.ToolTip(Me.components)
        Me.listProcesses = New AutomateUI.ProcessBackedMemberTreeListView()
        Me.listResources = New AutomateUI.ctlResourceView()
        Label29 = New System.Windows.Forms.Label()
        Label31 = New System.Windows.Forms.Label()
        Label5 = New System.Windows.Forms.Label()
        tabCompleteException = New System.Windows.Forms.TableLayoutPanel()
        tabCompleteException.SuspendLayout()
        CType(Me.PostCompletionDelayNumericInput, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel2.SuspendLayout()
        Me.Panel1.SuspendLayout()
        CType(Me.splitProcessResourcePanel, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitProcessResourcePanel.Panel1.SuspendLayout()
        Me.splitProcessResourcePanel.Panel2.SuspendLayout()
        Me.splitProcessResourcePanel.SuspendLayout()
        CType(Me.splitActivities, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitActivities.Panel1.SuspendLayout()
        Me.splitActivities.Panel2.SuspendLayout()
        Me.splitActivities.SuspendLayout()
        Me.menuSession.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label29
        '
        resources.ApplyResources(Label29, "Label29")
        Label29.ForeColor = System.Drawing.Color.Black
        Label29.Name = "Label29"
        '
        'Label31
        '
        resources.ApplyResources(Label31, "Label31")
        Label31.ForeColor = System.Drawing.Color.Black
        Label31.Name = "Label31"
        '
        'Label5
        '
        resources.ApplyResources(Label5, "Label5")
        Label5.ForeColor = System.Drawing.Color.Black
        Label5.Name = "Label5"
        '
        'tabCompleteException
        '
        resources.ApplyResources(tabCompleteException, "tabCompleteException")
        tabCompleteException.Controls.Add(Me.PostCompletionDelayNumericInput, 1, 1)
        tabCompleteException.Controls.Add(Me.Label3, 0, 0)
        tabCompleteException.Controls.Add(Me.Panel2, 3, 0)
        tabCompleteException.Controls.Add(Me.Panel1, 1, 0)
        tabCompleteException.Controls.Add(Me.Label4, 2, 0)
        tabCompleteException.Controls.Add(Me.cbFailFast, 2, 1)
        tabCompleteException.Controls.Add(Me.Label6, 0, 1)
        tabCompleteException.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize
        tabCompleteException.Name = "tabCompleteException"
        '
        'PostCompletionDelayNumericInput
        '
        resources.ApplyResources(Me.PostCompletionDelayNumericInput, "PostCompletionDelayNumericInput")
        Me.PostCompletionDelayNumericInput.Maximum = New Decimal(New Integer() {1500, 0, 0, 0})
        Me.PostCompletionDelayNumericInput.Name = "PostCompletionDelayNumericInput"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.ForeColor = System.Drawing.Color.Black
        Me.Label3.Name = "Label3"
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.comboOnException)
        Me.Panel2.Controls.Add(Me.txtOnException)
        resources.ApplyResources(Me.Panel2, "Panel2")
        Me.Panel2.Name = "Panel2"
        '
        'comboOnException
        '
        resources.ApplyResources(Me.comboOnException, "comboOnException")
        Me.comboOnException.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboOnException.FormattingEnabled = True
        Me.comboOnException.Items.AddRange(New Object() {resources.GetString("comboOnException.Items"), resources.GetString("comboOnException.Items1"), resources.GetString("comboOnException.Items2"), resources.GetString("comboOnException.Items3")})
        Me.comboOnException.Name = "comboOnException"
        '
        'txtOnException
        '
        resources.ApplyResources(Me.txtOnException, "txtOnException")
        Me.txtOnException.Name = "txtOnException"
        Me.txtOnException.ReadOnly = True
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.comboOnCompletion)
        Me.Panel1.Controls.Add(Me.txtOnCompletion)
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Name = "Panel1"
        '
        'comboOnCompletion
        '
        resources.ApplyResources(Me.comboOnCompletion, "comboOnCompletion")
        Me.comboOnCompletion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboOnCompletion.FormattingEnabled = True
        Me.comboOnCompletion.Items.AddRange(New Object() {resources.GetString("comboOnCompletion.Items"), resources.GetString("comboOnCompletion.Items1"), resources.GetString("comboOnCompletion.Items2")})
        Me.comboOnCompletion.Name = "comboOnCompletion"
        '
        'txtOnCompletion
        '
        resources.ApplyResources(Me.txtOnCompletion, "txtOnCompletion")
        Me.txtOnCompletion.Name = "txtOnCompletion"
        Me.txtOnCompletion.ReadOnly = True
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.ForeColor = System.Drawing.Color.Black
        Me.Label4.Name = "Label4"
        '
        'cbFailFast
        '
        resources.ApplyResources(Me.cbFailFast, "cbFailFast")
        tabCompleteException.SetColumnSpan(Me.cbFailFast, 2)
        Me.cbFailFast.Name = "cbFailFast"
        Me.ttFailOnErrorHelp.SetToolTip(Me.cbFailFast, resources.GetString("cbFailFast.ToolTip"))
        Me.cbFailFast.UseVisualStyleBackColor = True
        '
        'Label6
        '
        resources.ApplyResources(Me.Label6, "Label6")
        Me.Label6.Name = "Label6"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Name = "Label1"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.ForeColor = System.Drawing.Color.Black
        Me.Label2.Name = "Label2"
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        Me.txtName.Name = "txtName"
        '
        'txtDescription
        '
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        Me.txtDescription.Name = "txtDescription"
        '
        'splitProcessResourcePanel
        '
        resources.ApplyResources(Me.splitProcessResourcePanel, "splitProcessResourcePanel")
        Me.splitProcessResourcePanel.Name = "splitProcessResourcePanel"
        Me.splitProcessResourcePanel.GripVisible = False
        '
        'splitProcessResourcePanel.Panel1
        '
        Me.splitProcessResourcePanel.Panel1.Controls.Add(Me.listProcesses)
        Me.splitProcessResourcePanel.Panel1.Controls.Add(Label29)
        '
        'splitProcessResourcePanel.Panel2
        '
        Me.splitProcessResourcePanel.Panel2.Controls.Add(Me.listResources)
        Me.splitProcessResourcePanel.Panel2.Controls.Add(Label31)
        '
        'splitActivities
        '
        resources.ApplyResources(Me.splitActivities, "splitActivities")
        Me.splitActivities.Name = "splitActivities"
        Me.splitActivities.GripVisible = False
        '
        'splitActivities.Panel1
        '
        Me.splitActivities.Panel1.Controls.Add(Me.splitProcessResourcePanel)
        '
        'splitActivities.Panel2
        '
        Me.splitActivities.Panel2.Controls.Add(Me.listSessions)
        Me.splitActivities.Panel2.Controls.Add(Label5)
        '
        'listSessions
        '
        Me.listSessions.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.columnStatus, Me.columnProcesses, Me.columnResources})
        Me.listSessions.ContextMenuStrip = Me.menuSession
        resources.ApplyResources(Me.listSessions, "listSessions")
        Me.listSessions.FullRowSelect = True
        Me.listSessions.Name = "listSessions"
        Me.listSessions.ShowItemToolTips = True
        Me.listSessions.UseCompatibleStateImageBehavior = False
        Me.listSessions.View = System.Windows.Forms.View.Details
        '
        'columnStatus
        '
        resources.ApplyResources(Me.columnStatus, "columnStatus")
        '
        'columnProcesses
        '
        resources.ApplyResources(Me.columnProcesses, "columnProcesses")
        '
        'columnResources
        '
        resources.ApplyResources(Me.columnResources, "columnResources")
        '
        'menuSession
        '
        Me.menuSession.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.DeleteToolStripMenuItem, Me.StartupParametersToolStripMenuItem})
        Me.menuSession.Name = "menuSession"
        resources.ApplyResources(Me.menuSession, "menuSession")
        '
        'DeleteToolStripMenuItem
        '
        Me.DeleteToolStripMenuItem.Name = "DeleteToolStripMenuItem"
        resources.ApplyResources(Me.DeleteToolStripMenuItem, "DeleteToolStripMenuItem")
        '
        'StartupParametersToolStripMenuItem
        '
        Me.StartupParametersToolStripMenuItem.Name = "StartupParametersToolStripMenuItem"
        resources.ApplyResources(Me.StartupParametersToolStripMenuItem, "StartupParametersToolStripMenuItem")
        '
        'ttFailOnErrorHelp
        '
        Me.ttFailOnErrorHelp.AutoPopDelay = 30000
        Me.ttFailOnErrorHelp.InitialDelay = 500
        Me.ttFailOnErrorHelp.ReshowDelay = 0
        Me.ttFailOnErrorHelp.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info
        '
        'listProcesses
        '
        Me.listProcesses.AllowDrop = True
        Me.listProcesses.AllowDropInSpace = False
        Me.listProcesses.AllowDropOnGroups = False
        TreeListViewItemCollectionComparer3.Column = 0
        TreeListViewItemCollectionComparer3.SortOrder = System.Windows.Forms.SortOrder.Ascending
        Me.listProcesses.Comparer = TreeListViewItemCollectionComparer3
        resources.ApplyResources(Me.listProcesses, "listProcesses")
        Me.listProcesses.FocusedItem = Nothing
        Me.listProcesses.ManagePermissions = False
        Me.listProcesses.MultiLevelSelect = True
        Me.listProcesses.Name = "listProcesses"
        Me.listProcesses.ShowDescription = False
        Me.listProcesses.ShowDocumentLiteralFlag = False
        Me.listProcesses.ShowEmptyGroups = False
        Me.listProcesses.ShowExposedWebServiceName = False
        Me.listProcesses.TreeType = BluePrism.AutomateAppCore.Groups.GroupTreeType.Processes
        Me.listProcesses.UseCompatibleStateImageBehavior = False
        Me.listProcesses.UseLegacyNamespaceFlag = False
        '
        'listResources
        '
        Me.listResources.AllowDrop = True
        Me.listResources.AllowDropInSpace = False
        Me.listResources.AllowDropOnGroups = False
        TreeListViewItemCollectionComparer1.Column = 0
        TreeListViewItemCollectionComparer1.SortOrder = System.Windows.Forms.SortOrder.Ascending
        Me.listResources.Comparer = TreeListViewItemCollectionComparer1
        resources.ApplyResources(Me.listResources, "listResources")
        Me.listResources.FocusedItem = Nothing
        Me.listResources.MultiLevelSelect = True
        Me.listResources.Name = "listResources"
        Me.listResources.ShowEmptyGroups = False
        Me.listResources.TreeType = BluePrism.AutomateAppCore.Groups.GroupTreeType.Resources
        Me.listResources.UseCompatibleStateImageBehavior = False
        Me.listResources.WithoutAttributes = BluePrism.Core.Resources.ResourceAttribute.Retired
        '
        'ctlTask
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.splitActivities)
        Me.Controls.Add(tabCompleteException)
        Me.Controls.Add(Me.txtDescription)
        Me.Controls.Add(Me.txtName)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.DoubleBuffered = True
        Me.Name = "ctlTask"
        resources.ApplyResources(Me, "$this")
        tabCompleteException.ResumeLayout(False)
        tabCompleteException.PerformLayout()
        CType(Me.PostCompletionDelayNumericInput, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.splitProcessResourcePanel.Panel1.ResumeLayout(False)
        Me.splitProcessResourcePanel.Panel1.PerformLayout()
        Me.splitProcessResourcePanel.Panel2.ResumeLayout(False)
        Me.splitProcessResourcePanel.Panel2.PerformLayout()
        CType(Me.splitProcessResourcePanel, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitProcessResourcePanel.ResumeLayout(False)
        Me.splitActivities.Panel1.ResumeLayout(False)
        Me.splitActivities.Panel2.ResumeLayout(False)
        Me.splitActivities.Panel2.PerformLayout()
        CType(Me.splitActivities, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitActivities.ResumeLayout(False)
        Me.menuSession.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout

End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtName As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents txtDescription As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents splitProcessResourcePanel As AutomateControls.SplitContainers.HighlightingSplitContainer
    Friend WithEvents splitActivities As AutomateControls.SplitContainers.HighlightingSplitContainer
    Private WithEvents listResources As ctlResourceView
    Friend WithEvents listSessions As AutomateControls.FlickerFreeListView
    Friend WithEvents columnProcesses As System.Windows.Forms.ColumnHeader
    Friend WithEvents columnResources As System.Windows.Forms.ColumnHeader
    Friend WithEvents menuSession As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents DeleteToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents StartupParametersToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents comboOnCompletion As System.Windows.Forms.ComboBox
    Friend WithEvents txtOnCompletion As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents comboOnException As System.Windows.Forms.ComboBox
    Friend WithEvents txtOnException As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents cbFailFast As System.Windows.Forms.CheckBox
    Private WithEvents ttFailOnErrorHelp As System.Windows.Forms.ToolTip
    Private WithEvents listProcesses As AutomateUI.ProcessBackedMemberTreeListView
    Friend WithEvents columnStatus As ColumnHeader
    Friend WithEvents Label6 As Label
    Friend WithEvents PostCompletionDelayNumericInput As AutomateControls.StyledNumericUpDown
End Class
