<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlOwnerComponent
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
        Dim lblBy As System.Windows.Forms.Label
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlOwnerComponent))
        Dim lblName As System.Windows.Forms.Label
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.lblContents = New System.Windows.Forms.Label()
        Me.lblUserAction = New System.Windows.Forms.Label()
        Me.txtName = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtDescription = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblCreatedDate = New System.Windows.Forms.Label()
        Me.lblUser = New System.Windows.Forms.Label()
        Me.mSplitter = New System.Windows.Forms.SplitContainer()
        Me.ctxContentsMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnuExpandAll = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCollapseAll = New System.Windows.Forms.ToolStripMenuItem()
        Me.gridContents = New System.Windows.Forms.DataGridView()
        Me.colContentsIcon = New AutomateControls.DataGridViews.ImageListColumn()
        Me.colInfo = New System.Windows.Forms.DataGridViewImageColumn()
        Me.lblDescription = New System.Windows.Forms.Label()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.mToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.cTree = New AutomateUI.ctlComponentTree()
        Me.colContentsName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        lblBy = New System.Windows.Forms.Label()
        lblName = New System.Windows.Forms.Label()
        CType(Me.mSplitter, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.mSplitter.Panel1.SuspendLayout()
        Me.mSplitter.SuspendLayout()
        Me.ctxContentsMenu.SuspendLayout()
        CType(Me.gridContents, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblBy
        '
        resources.ApplyResources(lblBy, "lblBy")
        lblBy.Name = "lblBy"
        '
        'lblName
        '
        resources.ApplyResources(lblName, "lblName")
        lblName.Name = "lblName"
        '
        'lblContents
        '
        resources.ApplyResources(Me.lblContents, "lblContents")
        Me.lblContents.Name = "lblContents"
        '
        'lblUserAction
        '
        resources.ApplyResources(Me.lblUserAction, "lblUserAction")
        Me.lblUserAction.Name = "lblUserAction"
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        Me.txtName.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.txtName.ForeColor = System.Drawing.SystemColors.ControlText
        Me.txtName.Name = "txtName"
        Me.txtName.ReadOnly = True
        '
        'txtDescription
        '
        Me.txtDescription.AcceptsReturn = True
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        Me.txtDescription.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.txtDescription.Name = "txtDescription"
        Me.txtDescription.ReadOnly = True
        '
        'lblCreatedDate
        '
        resources.ApplyResources(Me.lblCreatedDate, "lblCreatedDate")
        Me.lblCreatedDate.Name = "lblCreatedDate"
        '
        'lblUser
        '
        resources.ApplyResources(Me.lblUser, "lblUser")
        Me.lblUser.Name = "lblUser"
        '
        'mSplitter
        '
        resources.ApplyResources(Me.mSplitter, "mSplitter")
        Me.mSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.mSplitter.Name = "mSplitter"
        '
        'mSplitter.Panel1
        '
        Me.mSplitter.Panel1.Controls.Add(Me.cTree)
        Me.mSplitter.Panel1.Controls.Add(Me.lblContents)
        Me.mSplitter.Panel1.Controls.Add(Me.gridContents)
        '
        'ctxContentsMenu
        '
        Me.ctxContentsMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuExpandAll, Me.mnuCollapseAll})
        Me.ctxContentsMenu.Name = "ctxContentsMenu"
        resources.ApplyResources(Me.ctxContentsMenu, "ctxContentsMenu")
        '
        'mnuExpandAll
        '
        Me.mnuExpandAll.Image = Global.AutomateUI.My.Resources.ToolImages.Expand_All_16x16
        Me.mnuExpandAll.Name = "mnuExpandAll"
        resources.ApplyResources(Me.mnuExpandAll, "mnuExpandAll")
        '
        'mnuCollapseAll
        '
        Me.mnuCollapseAll.Image = Global.AutomateUI.My.Resources.ToolImages.Collapse_All_16x16
        Me.mnuCollapseAll.Name = "mnuCollapseAll"
        resources.ApplyResources(Me.mnuCollapseAll, "mnuCollapseAll")
        '
        'gridContents
        '
        Me.gridContents.AllowUserToAddRows = False
        Me.gridContents.AllowUserToDeleteRows = False
        Me.gridContents.AllowUserToResizeColumns = False
        Me.gridContents.AllowUserToResizeRows = False
        resources.ApplyResources(Me.gridContents, "gridContents")
        Me.gridContents.BackgroundColor = System.Drawing.SystemColors.Window
        Me.gridContents.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.gridContents.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal
        Me.gridContents.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None
        Me.gridContents.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridContents.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colContentsIcon, Me.colContentsName, Me.colInfo})
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.gridContents.DefaultCellStyle = DataGridViewCellStyle1
        Me.gridContents.GridColor = System.Drawing.SystemColors.ControlLight
        Me.gridContents.Name = "gridContents"
        Me.gridContents.ReadOnly = True
        Me.gridContents.RowHeadersVisible = False
        Me.gridContents.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing
        Me.gridContents.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.gridContents.ShowEditingIcon = False
        Me.gridContents.StandardTab = True
        '
        'colContentsIcon
        '
        Me.colContentsIcon.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.colContentsIcon, "colContentsIcon")
        Me.colContentsIcon.Name = "colContentsIcon"
        Me.colContentsIcon.ReadOnly = True
        Me.colContentsIcon.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.colContentsIcon.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'colInfo
        '
        resources.ApplyResources(Me.colInfo, "colInfo")
        Me.colInfo.Name = "colInfo"
        Me.colInfo.ReadOnly = True
        Me.colInfo.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'lblDescription
        '
        resources.ApplyResources(Me.lblDescription, "lblDescription")
        Me.lblDescription.Name = "lblDescription"
        '
        'lblTitle
        '
        Me.lblTitle.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(114, Byte), Integer), CType(CType(198, Byte), Integer))
        resources.ApplyResources(Me.lblTitle, "lblTitle")
        Me.lblTitle.ForeColor = System.Drawing.Color.White
        Me.lblTitle.Name = "lblTitle"
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        '
        'cTree
        '
        Me.cTree.AllowDrop = True
        resources.ApplyResources(Me.cTree, "cTree")
        Me.cTree.ContextMenuStrip = Me.ctxContentsMenu
        Me.cTree.Name = "cTree"
        Me.cTree.ShowNodeToolTips = True
        Me.cTree.Sorted = True
        '
        'colContentsName
        '
        Me.colContentsName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colContentsName, "colContentsName")
        Me.colContentsName.Name = "colContentsName"
        Me.colContentsName.ReadOnly = True
        '
        'ctlOwnerComponent
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.Controls.Add(Me.lblDescription)
        Me.Controls.Add(lblName)
        Me.Controls.Add(Me.mSplitter)
        Me.Controls.Add(Me.lblUser)
        Me.Controls.Add(Me.lblCreatedDate)
        Me.Controls.Add(lblBy)
        Me.Controls.Add(Me.lblUserAction)
        Me.Controls.Add(Me.txtDescription)
        Me.Controls.Add(Me.txtName)
        Me.Controls.Add(Me.lblTitle)
        Me.Name = "ctlOwnerComponent"
        resources.ApplyResources(Me, "$this")
        Me.mSplitter.Panel1.ResumeLayout(false)
        Me.mSplitter.Panel1.PerformLayout
        CType(Me.mSplitter,System.ComponentModel.ISupportInitialize).EndInit
        Me.mSplitter.ResumeLayout(false)
        Me.ctxContentsMenu.ResumeLayout(false)
        CType(Me.gridContents,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Private WithEvents lblUser As System.Windows.Forms.Label
    Private WithEvents txtName As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents txtDescription As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents lblCreatedDate As System.Windows.Forms.Label
    Private WithEvents gridContents As System.Windows.Forms.DataGridView
    Protected WithEvents mSplitter As System.Windows.Forms.SplitContainer
    Private WithEvents lblUserAction As System.Windows.Forms.Label
    Private WithEvents lblDescription As System.Windows.Forms.Label
    Protected lblTitle As System.Windows.Forms.Label
    Friend WithEvents colContentsIcon As AutomateControls.DataGridViews.ImageListColumn
    Friend WithEvents colContentsName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colInfo As System.Windows.Forms.DataGridViewImageColumn
    Friend WithEvents cTree As AutomateUI.ctlComponentTree
    Friend WithEvents ctxContentsMenu As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents mnuExpandAll As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCollapseAll As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents lblContents As Label
    Friend WithEvents mToolTip As ToolTip
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
End Class
