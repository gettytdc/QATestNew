<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmApplicationTreeNavigator
    Inherits frmForm

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim Label3 As System.Windows.Forms.Label
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmApplicationTreeNavigator))
        Me.splitAppTree = New AutomateControls.GrippableSplitContainer()
        Me.btnCollapse = New AutomateControls.ArrowButton()
        Me.cbShowInvisible = New System.Windows.Forms.CheckBox()
        Me.filTreeElements = New AutomateControls.Trees.TreeViewAndFilter()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.gridAttrs = New AutomateControls.DataGridViews.RowBasedDataGridView()
        Me.colName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colValue = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.mWorker = New System.ComponentModel.BackgroundWorker()
        Me.btnRefresh = New AutomateControls.Buttons.StandardStyledButton()
        Me.ttTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.ctxMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnuRegionEditor = New System.Windows.Forms.ToolStripMenuItem()
        Label3 = New System.Windows.Forms.Label()
        CType(Me.splitAppTree, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitAppTree.Panel1.SuspendLayout()
        Me.splitAppTree.Panel2.SuspendLayout()
        Me.splitAppTree.SuspendLayout()
        CType(Me.gridAttrs, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ctxMenu.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label3
        '
        resources.ApplyResources(Label3, "Label3")
        Label3.Name = "Label3"
        '
        'splitAppTree
        '
        resources.ApplyResources(Me.splitAppTree, "splitAppTree")
        Me.splitAppTree.Name = "splitAppTree"
        '
        'splitAppTree.Panel1
        '
        Me.splitAppTree.Panel1.Controls.Add(Me.btnCollapse)
        Me.splitAppTree.Panel1.Controls.Add(Me.cbShowInvisible)
        Me.splitAppTree.Panel1.Controls.Add(Me.filTreeElements)
        Me.splitAppTree.Panel1.Controls.Add(Me.Label1)
        '
        'splitAppTree.Panel2
        '
        Me.splitAppTree.Panel2.Controls.Add(Me.gridAttrs)
        Me.splitAppTree.Panel2.Controls.Add(Me.Label2)
        Me.splitAppTree.SplitLineColor = System.Drawing.SystemColors.InactiveBorder
        Me.splitAppTree.TabStop = False
        '
        'btnCollapse
        '
        resources.ApplyResources(Me.btnCollapse, "btnCollapse")
        Me.btnCollapse.Name = "btnCollapse"
        Me.ttTip.SetToolTip(Me.btnCollapse, resources.GetString("btnCollapse.ToolTip"))
        Me.btnCollapse.UseVisualStyleBackColor = True
        '
        'cbShowInvisible
        '
        resources.ApplyResources(Me.cbShowInvisible, "cbShowInvisible")
        Me.cbShowInvisible.Name = "cbShowInvisible"
        Me.cbShowInvisible.UseVisualStyleBackColor = True
        '
        'filTreeElements
        '
        resources.ApplyResources(Me.filTreeElements, "filTreeElements")
        Me.filTreeElements.Filterer = Nothing
        Me.filTreeElements.HotTracking = True
        Me.filTreeElements.Indent = 15
        Me.filTreeElements.Name = "filTreeElements"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'gridAttrs
        '
        resources.ApplyResources(Me.gridAttrs, "gridAttrs")
        Me.gridAttrs.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells
        Me.gridAttrs.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None
        Me.gridAttrs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridAttrs.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colName, Me.colValue})
        Me.gridAttrs.MultiSelect = False
        Me.gridAttrs.Name = "gridAttrs"
        Me.gridAttrs.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None
        '
        'colName
        '
        Me.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.colName, "colName")
        Me.colName.Name = "colName"
        Me.colName.ReadOnly = True
        '
        'colValue
        '
        Me.colValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colValue, "colValue")
        Me.colValue.Name = "colValue"
        Me.colValue.ReadOnly = True
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'mWorker
        '
        Me.mWorker.WorkerReportsProgress = True
        Me.mWorker.WorkerSupportsCancellation = True
        '
        'btnRefresh
        '
        resources.ApplyResources(Me.btnRefresh, "btnRefresh")
        Me.btnRefresh.Name = "btnRefresh"
        Me.btnRefresh.UseVisualStyleBackColor = True
        '
        'ttTip
        '
        Me.ttTip.AutoPopDelay = 5000
        Me.ttTip.InitialDelay = 50
        Me.ttTip.ReshowDelay = 100
        '
        'ctxMenu
        '
        Me.ctxMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuRegionEditor})
        Me.ctxMenu.Name = "ctxMenu"
        resources.ApplyResources(Me.ctxMenu, "ctxMenu")
        '
        'mnuRegionEditor
        '
        Me.mnuRegionEditor.Name = "mnuRegionEditor"
        resources.ApplyResources(Me.mnuRegionEditor, "mnuRegionEditor")
        '
        'frmApplicationTreeNavigator
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Label3)
        Me.Controls.Add(Me.btnRefresh)
        Me.Controls.Add(Me.splitAppTree)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.btnCancel)
        Me.Name = "frmApplicationTreeNavigator"
        Me.splitAppTree.Panel1.ResumeLayout(False)
        Me.splitAppTree.Panel1.PerformLayout()
        Me.splitAppTree.Panel2.ResumeLayout(False)
        Me.splitAppTree.Panel2.PerformLayout()
        CType(Me.splitAppTree, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitAppTree.ResumeLayout(False)
        CType(Me.gridAttrs, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ctxMenu.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents splitAppTree As AutomateControls.GrippableSplitContainer
    Private WithEvents Label1 As System.Windows.Forms.Label
    Private WithEvents filTreeElements As AutomateControls.Trees.TreeViewAndFilter
    Private WithEvents Label2 As System.Windows.Forms.Label
    Private WithEvents mWorker As System.ComponentModel.BackgroundWorker
    Private WithEvents colName As System.Windows.Forms.DataGridViewTextBoxColumn
    Private WithEvents colValue As System.Windows.Forms.DataGridViewTextBoxColumn
    Private WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents gridAttrs As AutomateControls.DataGridViews.RowBasedDataGridView
    Private WithEvents btnRefresh As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents cbShowInvisible As System.Windows.Forms.CheckBox
    Private WithEvents btnCollapse As AutomateControls.ArrowButton
    Private WithEvents ttTip As System.Windows.Forms.ToolTip
    Private WithEvents ctxMenu As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents mnuRegionEditor As System.Windows.Forms.ToolStripMenuItem
End Class
