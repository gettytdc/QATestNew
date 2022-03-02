<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmReferenceViewer
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmReferenceViewer))
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.imgList = New System.Windows.Forms.ImageList(Me.components)
        Me.tBar = New AutomateControls.TitleBar()
        Me.dgvReferences = New System.Windows.Forms.DataGridView()
        Me.ssWarning = New System.Windows.Forms.StatusStrip()
        Me.lblWarning = New System.Windows.Forms.ToolStripStatusLabel()
        Me.img = New System.Windows.Forms.DataGridViewImageColumn()
        Me.objName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Description = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.View = New System.Windows.Forms.DataGridViewLinkColumn()
        CType(Me.dgvReferences, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ssWarning.SuspendLayout()
        Me.SuspendLayout()
        '
        'imgList
        '
        Me.imgList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit
        resources.ApplyResources(Me.imgList, "imgList")
        Me.imgList.TransparentColor = System.Drawing.Color.Transparent
        '
        'tBar
        '
        resources.ApplyResources(Me.tBar, "tBar")
        Me.tBar.Name = "tBar"
        '
        'dgvReferences
        '
        Me.dgvReferences.AllowUserToAddRows = False
        Me.dgvReferences.AllowUserToDeleteRows = False
        Me.dgvReferences.AllowUserToResizeRows = False
        resources.ApplyResources(Me.dgvReferences, "dgvReferences")
        Me.dgvReferences.BackgroundColor = System.Drawing.Color.White
        Me.dgvReferences.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvReferences.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.img, Me.objName, Me.Description, Me.View})
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle2.Font = New System.Drawing.Font("Segoe UI", 8.25!)
        DataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dgvReferences.DefaultCellStyle = DataGridViewCellStyle2
        Me.dgvReferences.MultiSelect = False
        Me.dgvReferences.Name = "dgvReferences"
        Me.dgvReferences.ReadOnly = True
        Me.dgvReferences.RowHeadersVisible = False
        Me.dgvReferences.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'ssWarning
        '
        Me.ssWarning.BackColor = System.Drawing.Color.White
        Me.ssWarning.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.lblWarning})
        resources.ApplyResources(Me.ssWarning, "ssWarning")
        Me.ssWarning.Name = "ssWarning"
        '
        'lblWarning
        '
        resources.ApplyResources(Me.lblWarning, "lblWarning")
        Me.lblWarning.Name = "lblWarning"
        Me.lblWarning.Spring = True
        '
        'img
        '
        Me.img.Frozen = True
        resources.ApplyResources(Me.img, "img")
        Me.img.Name = "img"
        Me.img.ReadOnly = True
        Me.img.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'objName
        '
        Me.objName.Frozen = True
        resources.ApplyResources(Me.objName, "objName")
        Me.objName.Name = "objName"
        Me.objName.ReadOnly = True
        '
        'Description
        '
        Me.Description.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.Description, "Description")
        Me.Description.Name = "Description"
        Me.Description.ReadOnly = True
        '
        'View
        '
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.View.DefaultCellStyle = DataGridViewCellStyle1
        resources.ApplyResources(Me.View, "View")
        Me.View.Name = "View"
        Me.View.ReadOnly = True
        Me.View.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'frmReferenceViewer
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.ssWarning)
        Me.Controls.Add(Me.dgvReferences)
        Me.Controls.Add(Me.tBar)
        Me.HelpButton = True
        Me.Name = "frmReferenceViewer"
        Me.ShowInTaskbar = False
        CType(Me.dgvReferences, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ssWarning.ResumeLayout(False)
        Me.ssWarning.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents imgList As System.Windows.Forms.ImageList
    Friend WithEvents tBar As AutomateControls.TitleBar
    Friend WithEvents dgvReferences As System.Windows.Forms.DataGridView
    Friend WithEvents ssWarning As StatusStrip
    Friend WithEvents lblWarning As ToolStripStatusLabel
    Friend WithEvents img As DataGridViewImageColumn
    Friend WithEvents objName As DataGridViewTextBoxColumn
    Friend WithEvents Description As DataGridViewTextBoxColumn
    Friend WithEvents View As DataGridViewLinkColumn
End Class
