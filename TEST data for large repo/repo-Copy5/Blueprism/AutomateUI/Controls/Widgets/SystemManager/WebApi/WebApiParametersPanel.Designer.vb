<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class WebApiParametersPanel
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(WebApiParametersPanel))
        Me.gridParams = New System.Windows.Forms.DataGridView()
        Me.colDelete = New System.Windows.Forms.DataGridViewImageColumn()
        Me.colName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colDescription = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ColDataType = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.colValue = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colExpose = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        CType(Me.gridParams, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'gridParams
        '
        resources.ApplyResources(Me.gridParams, "gridParams")
        Me.gridParams.AllowUserToResizeRows = False
        Me.gridParams.BackgroundColor = System.Drawing.SystemColors.ControlLight
        Me.gridParams.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.gridParams.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colDelete, Me.colName, Me.colDescription, Me.ColDataType, Me.colValue, Me.colExpose})
        Me.gridParams.Name = "gridParams"
        Me.gridParams.RowHeadersVisible = False
        '
        'colDelete
        '
        Me.colDelete.FillWeight = 25.0!
        resources.ApplyResources(Me.colDelete, "colDelete")
        Me.colDelete.Name = "colDelete"
        '
        'colName
        '
        Me.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.colName.FillWeight = 175.0!
        resources.ApplyResources(Me.colName, "colName")
        Me.colName.Name = "colName"
        '
        'colDescription
        '
        Me.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.colDescription.FillWeight = 62.5!
        resources.ApplyResources(Me.colDescription, "colDescription")
        Me.colDescription.Name = "colDescription"
        '
        'ColDataType
        '
        Me.ColDataType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.ColDataType, "ColDataType")
        Me.ColDataType.Name = "ColDataType"
        '
        'colValue
        '
        Me.colValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colValue.FillWeight = 62.5!
        resources.ApplyResources(Me.colValue, "colValue")
        Me.colValue.Name = "colValue"
        '
        'colExpose
        '
        Me.colExpose.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader
        resources.ApplyResources(Me.colExpose, "colExpose")
        Me.colExpose.Name = "colExpose"
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        '
        'DataGridViewTextBoxColumn3
        '
        Me.DataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.DataGridViewTextBoxColumn3, "DataGridViewTextBoxColumn3")
        Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
        '
        'WebApiParametersPanel
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.gridParams)
        Me.Name = "WebApiParametersPanel"
        CType(Me.gridParams,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)

End Sub
    
    Private WithEvents gridParams As DataGridView
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn3 As DataGridViewTextBoxColumn
    Friend WithEvents colDelete As DataGridViewImageColumn
    Friend WithEvents colName As DataGridViewTextBoxColumn
    Friend WithEvents colDescription As DataGridViewTextBoxColumn
    Friend WithEvents ColDataType As DataGridViewComboBoxColumn
    Friend WithEvents colValue As DataGridViewTextBoxColumn
    Friend WithEvents colExpose As DataGridViewCheckBoxColumn
End Class
