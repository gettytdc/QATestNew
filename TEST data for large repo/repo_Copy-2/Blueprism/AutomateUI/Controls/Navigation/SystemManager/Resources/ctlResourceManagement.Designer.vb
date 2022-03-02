<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlResourceManagement
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlResourceManagement))
        Me.menuLoggingLevel = New System.Windows.Forms.ToolStripMenuItem()
        Me.DefaultToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.KeyStagesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AllStagesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ErrorsOnlyToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem1 = New System.Windows.Forms.ToolStripSeparator()
        Me.LogMemoryUsageToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.IncludeMemoryCleanupToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.LogWebServiceCommunicationToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ResetFQDNToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuEventLogEnabled = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuSortResources = New System.Windows.Forms.ToolStripMenuItem()
        Me.SortByNameAscending = New System.Windows.Forms.ToolStripMenuItem()
        Me.SortByNameDescending = New System.Windows.Forms.ToolStripMenuItem()
        Me.SortByLogLevelAscending = New System.Windows.Forms.ToolStripMenuItem()
        Me.SortByLogLevelDescending = New System.Windows.Forms.ToolStripMenuItem()
        Me.lResources = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lstResourcesRetired = New System.Windows.Forms.ListView()
        Me.mSplit = New System.Windows.Forms.SplitContainer()
        Me.mResourceGroupTree = New AutomateUI.GroupTreeControl()
        Me.lResourcesHint = New System.Windows.Forms.Label()
        CType(Me.mSplit, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.mSplit.Panel1.SuspendLayout()
        Me.mSplit.Panel2.SuspendLayout()
        Me.mSplit.SuspendLayout()
        Me.SuspendLayout()
        '
        'menuLoggingLevel
        '
        Me.menuLoggingLevel.DropDownItems.AddRange(
            New System.Windows.Forms.ToolStripItem() {
                Me.DefaultToolStripMenuItem,
                Me.KeyStagesToolStripMenuItem,
                Me.AllStagesToolStripMenuItem,
                Me.ErrorsOnlyToolStripMenuItem,
                Me.ToolStripMenuItem1,
                Me.LogMemoryUsageToolStripMenuItem,
                Me.IncludeMemoryCleanupToolStripMenuItem,
                Me.LogWebServiceCommunicationToolStripMenuItem})

        Me.menuLoggingLevel.Name = "menuLoggingLevel"
        resources.ApplyResources(Me.menuLoggingLevel, "menuLoggingLevel")
        '
        'DefaultToolStripMenuItem
        '
        Me.DefaultToolStripMenuItem.Name = "DefaultToolStripMenuItem"
        resources.ApplyResources(Me.DefaultToolStripMenuItem, "DefaultToolStripMenuItem")
        '
        'KeyStagesToolStripMenuItem
        '
        Me.KeyStagesToolStripMenuItem.Name = "KeyStagesToolStripMenuItem"
        resources.ApplyResources(Me.KeyStagesToolStripMenuItem, "KeyStagesToolStripMenuItem")
        '
        'AllStagesToolStripMenuItem
        '
        Me.AllStagesToolStripMenuItem.Name = "AllStagesToolStripMenuItem"
        resources.ApplyResources(Me.AllStagesToolStripMenuItem, "AllStagesToolStripMenuItem")
        '
        'ErrorsOnlyToolStripMenuItem
        '
        Me.ErrorsOnlyToolStripMenuItem.Name = "ErrorsOnlyToolStripMenuItem"
        resources.ApplyResources(Me.ErrorsOnlyToolStripMenuItem, "ErrorsOnlyToolStripMenuItem")
        '
        'ToolStripMenuItem1
        '
        Me.ToolStripMenuItem1.Name = "ToolStripMenuItem1"
        resources.ApplyResources(Me.ToolStripMenuItem1, "ToolStripMenuItem1")
        '
        'LogMemoryUsageToolStripMenuItem
        '
        Me.LogMemoryUsageToolStripMenuItem.Name = "LogMemoryUsageToolStripMenuItem"
        resources.ApplyResources(Me.LogMemoryUsageToolStripMenuItem, "LogMemoryUsageToolStripMenuItem")
        '
        'IncludeMemoryCleanupToolStripMenuItem
        '
        Me.IncludeMemoryCleanupToolStripMenuItem.Name = "IncludeMemoryCleanupToolStripMenuItem"
        resources.ApplyResources(Me.IncludeMemoryCleanupToolStripMenuItem, "IncludeMemoryCleanupToolStripMenuItem")
        '
        'LogWebServiceCommunicationToolStripMenuItem
        '
        Me.LogWebServiceCommunicationToolStripMenuItem.Name = "LogWebServiceCommunicationToolStripMenuItem"
        resources.ApplyResources(Me.LogWebServiceCommunicationToolStripMenuItem, "LogWebServiceCommunicationToolStripMenuItem")
        '
        'ResetFQDNToolStripMenuItem
        '
        Me.ResetFQDNToolStripMenuItem.Name = "ResetFQDNToolStripMenuItem"
        resources.ApplyResources(Me.ResetFQDNToolStripMenuItem, "ResetFQDNToolStripMenuItem")
        '
        'menuEventLogEnabled
        '
        Me.menuEventLogEnabled.AutoToolTip = True
        Me.menuEventLogEnabled.CheckOnClick = True
        Me.menuEventLogEnabled.Name = "menuEventLogEnabled"
        resources.ApplyResources(Me.menuEventLogEnabled, "menuEventLogEnabled")
        '
        'menuSortResources
        '
        Me.menuSortResources.DropDownItems.AddRange(
            New System.Windows.Forms.ToolStripItem() {
                Me.SortByNameAscending,
                Me.SortByNameDescending,
                Me.SortByLogLevelAscending,
                Me.SortByLogLevelDescending})

        Me.menuSortResources.Name = "menuSortResources"
        resources.ApplyResources(Me.menuSortResources, "menuSortResources")
        '
        'SortByNameAscending
        '
        Me.SortByNameAscending.Name = "SortByNameAscending"
        resources.ApplyResources(Me.SortByNameAscending, "SortByNameAscending")
        '
        'SortByNameDescending
        '
        Me.SortByNameDescending.Name = "SortByNameDescending"
        resources.ApplyResources(Me.SortByNameDescending, "SortByNameDescending")
        '
        'SortByLogLevelAscending
        '
        Me.SortByLogLevelAscending.Name = "SortByLogLevelAscending"
        resources.ApplyResources(Me.SortByLogLevelAscending, "SortByLogLevelAscending")
        '
        'SortByLogLevelDescending
        '
        Me.SortByLogLevelDescending.Name = "SortByLogLevelDescending"
        resources.ApplyResources(Me.SortByLogLevelDescending, "SortByLogLevelDescending")
        '
        'lResources
        '
        Me.lResources.BackColor = System.Drawing.Color.Transparent
        resources.ApplyResources(Me.lResources, "lResources")
        Me.lResources.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.lResources.Name = "lResources"
        '
        'Label1
        '
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.Label1.Name = "Label1"
        '
        'lstResourcesRetired
        '
        Me.lstResourcesRetired.AllowDrop = True
        resources.ApplyResources(Me.lstResourcesRetired, "lstResourcesRetired")
        Me.lstResourcesRetired.Name = "lstResourcesRetired"
        Me.lstResourcesRetired.UseCompatibleStateImageBehavior = False
        Me.lstResourcesRetired.View = System.Windows.Forms.View.List
        '
        'mSplit
        '
        resources.ApplyResources(Me.mSplit, "mSplit")
        Me.mSplit.Name = "mSplit"
        Me.mSplit.TabStop = false
        '
        'mSplit.Panel1
        '
        Me.mSplit.Panel1.Controls.Add(Me.mResourceGroupTree)
        Me.mSplit.Panel1.Controls.Add(Me.lResourcesHint)
        Me.mSplit.Panel1.Controls.Add(Me.lResources)
        '
        'mSplit.Panel2
        '
        Me.mSplit.Panel2.Controls.Add(Me.lstResourcesRetired)
        Me.mSplit.Panel2.Controls.Add(Me.Label1)
        '
        'mResourceGroupTree
        '
        resources.ApplyResources(Me.mResourceGroupTree, "mResourceGroupTree")
        Me.mResourceGroupTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.mResourceGroupTree.EnableMultipleSelect = true
        Me.mResourceGroupTree.Name = "mResourceGroupTree"
        '
        'lResourcesHint
        '
        Me.lResourcesHint.BackColor = System.Drawing.Color.Transparent
        resources.ApplyResources(Me.lResourcesHint, "lResourcesHint")
        Me.lResourcesHint.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.lResourcesHint.Name = "lResourcesHint"
        '
        'ctlResourceManagement
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.mSplit)
        Me.Name = "ctlResourceManagement"
        Me.mSplit.Panel1.ResumeLayout(False)
        Me.mSplit.Panel2.ResumeLayout(False)
        CType(Me.mSplit, System.ComponentModel.ISupportInitialize).EndInit()
        Me.mSplit.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents lResources As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lstResourcesRetired As System.Windows.Forms.ListView
    Private WithEvents menuLoggingLevel As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DefaultToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents KeyStagesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AllStagesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ErrorsOnlyToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents LogMemoryUsageToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents IncludeMemoryCleanupToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents LogWebServiceCommunicationToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ResetFQDNToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents menuEventLogEnabled As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mSplit As System.Windows.Forms.SplitContainer
    Friend WithEvents mResourceGroupTree As AutomateUI.GroupTreeControl
    Friend WithEvents lResourcesHint As System.Windows.Forms.Label
    Private WithEvents menuSortResources As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SortByNameAscending As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SortByNameDescending As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SortByLogLevelAscending As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SortByLogLevelDescending As System.Windows.Forms.ToolStripMenuItem
End Class
