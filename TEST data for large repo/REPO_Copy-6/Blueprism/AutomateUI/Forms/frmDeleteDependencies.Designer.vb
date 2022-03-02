<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmDeleteDependencies
    Inherits AutomateControls.Forms.HelpButtonForm

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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmDeleteDependencies))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.tBar = New AutomateControls.TitleBar()
        Me.dgvItems = New System.Windows.Forms.DataGridView()
        Me.img = New System.Windows.Forms.DataGridViewImageColumn()
        Me.Item = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.refs = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.cbDelete = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnDelete = New AutomateControls.Buttons.StandardStyledButton()
        CType(Me.dgvItems,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'tBar
        '
        resources.ApplyResources(Me.tBar, "tBar")
        Me.tBar.Name = "tBar"
        '
        'dgvItems
        '
        Me.dgvItems.AllowUserToAddRows = False
        Me.dgvItems.AllowUserToDeleteRows = False
        Me.dgvItems.AllowUserToResizeRows = False
        resources.ApplyResources(Me.dgvItems, "dgvItems")
        Me.dgvItems.BackgroundColor = System.Drawing.Color.White
        Me.dgvItems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvItems.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.img, Me.Item, Me.refs, Me.cbDelete})
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle1.Font = New System.Drawing.Font("Segoe UI", 8.25!)
        DataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dgvItems.DefaultCellStyle = DataGridViewCellStyle1
        Me.dgvItems.Name = "dgvItems"
        Me.dgvItems.RowHeadersVisible = false
        '
        'img
        '
        Me.img.Frozen = true
        resources.ApplyResources(Me.img, "img")
        Me.img.Name = "img"
        Me.img.ReadOnly = true
        '
        'Item
        '
        Me.Item.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.Item, "Item")
        Me.Item.Name = "Item"
        Me.Item.ReadOnly = true
        '
        'refs
        '
        resources.ApplyResources(Me.refs, "refs")
        Me.refs.Name = "refs"
        Me.refs.ReadOnly = true
        '
        'cbDelete
        '
        resources.ApplyResources(Me.cbDelete, "cbDelete")
        Me.cbDelete.Name = "cbDelete"
        '
        'ImageList1
        '
        Me.ImageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit
        resources.ApplyResources(Me.ImageList1, "ImageList1")
        Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = true
        '
        'btnDelete
        '
        resources.ApplyResources(Me.btnDelete, "btnDelete")
        Me.btnDelete.Name = "btnDelete"
        Me.btnDelete.UseVisualStyleBackColor = true
        '
        'frmDeleteDependencies
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.btnDelete)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.dgvItems)
        Me.Controls.Add(Me.tBar)
        Me.HelpButton = true
        Me.MaximizeBox = false
        Me.MinimizeBox = false
        Me.Name = "frmDeleteDependencies"
        CType(Me.dgvItems,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)

End Sub
    Friend WithEvents tBar As AutomateControls.TitleBar
    Friend WithEvents dgvItems As System.Windows.Forms.DataGridView
    Friend WithEvents ImageList1 As System.Windows.Forms.ImageList
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnDelete As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents img As System.Windows.Forms.DataGridViewImageColumn
    Friend WithEvents Item As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents refs As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents cbDelete As System.Windows.Forms.DataGridViewCheckBoxColumn

End Class
