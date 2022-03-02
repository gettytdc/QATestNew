<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlReleaseDifference

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
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlReleaseDifference))
        Me.gridDiffs = New System.Windows.Forms.DataGridView()
        Me.colType = New AutomateControls.DataGridViews.ImageListColumn()
        Me.colName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colDiff = New System.Windows.Forms.DataGridViewTextBoxColumn()
        CType(Me.gridDiffs, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'gridDiffs
        '
        Me.gridDiffs.AllowUserToAddRows = False
        Me.gridDiffs.AllowUserToDeleteRows = False
        Me.gridDiffs.AllowUserToResizeRows = False
        Me.gridDiffs.BackgroundColor = System.Drawing.Color.White
        Me.gridDiffs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridDiffs.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colType, Me.colName, Me.colDiff})
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.ControlLight
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.gridDiffs.DefaultCellStyle = DataGridViewCellStyle1
        resources.ApplyResources(Me.gridDiffs, "gridDiffs")
        Me.gridDiffs.Name = "gridDiffs"
        Me.gridDiffs.ReadOnly = True
        Me.gridDiffs.RowHeadersVisible = False
        Me.gridDiffs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'colType
        '
        Me.colType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.colType, "colType")
        Me.colType.Name = "colType"
        Me.colType.ReadOnly = True
        Me.colType.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'colName
        '
        Me.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.colName, "colName")
        Me.colName.Name = "colName"
        Me.colName.ReadOnly = True
        '
        'colDiff
        '
        Me.colDiff.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colDiff, "colDiff")
        Me.colDiff.Name = "colDiff"
        Me.colDiff.ReadOnly = True
        '
        'ctlReleaseDifference
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.gridDiffs)
        Me.Name = "ctlReleaseDifference"
        CType(Me.gridDiffs, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents colType As AutomateControls.DataGridViews.ImageListColumn
    Friend WithEvents colName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colDiff As System.Windows.Forms.DataGridViewTextBoxColumn
    Private WithEvents gridDiffs As System.Windows.Forms.DataGridView

End Class
