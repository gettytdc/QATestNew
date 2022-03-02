<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class WebApiActionSummary
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(WebApiActionSummary))
        Me.gridActions = New AutomateControls.DataGridViews.RowBasedDataGridView()
        Me.colName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colDescription = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colMethod = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colUrl = New System.Windows.Forms.DataGridViewTextBoxColumn()
        CType(Me.gridActions,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'gridActions
        '
        resources.ApplyResources(Me.gridActions, "gridActions")
        Me.gridActions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridActions.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colName, Me.colDescription, Me.colMethod, Me.colUrl})
        Me.gridActions.MultiSelect = false
        Me.gridActions.Name = "gridActions"
        Me.gridActions.ReadOnly = true
        '
        'colName
        '
        Me.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.colName, "colName")
        Me.colName.Name = "colName"
        Me.colName.ReadOnly = true
        '
        'colDescription
        '
        Me.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colDescription.FillWeight = 50!
        resources.ApplyResources(Me.colDescription, "colDescription")
        Me.colDescription.Name = "colDescription"
        Me.colDescription.ReadOnly = true
        '
        'colMethod
        '
        Me.colMethod.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.colMethod, "colMethod")
        Me.colMethod.Name = "colMethod"
        Me.colMethod.ReadOnly = true
        '
        'colUrl
        '
        Me.colUrl.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colUrl, "colUrl")
        Me.colUrl.Name = "colUrl"
        Me.colUrl.ReadOnly = true
        '
        'WebApiActionSummary
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.gridActions)
        Me.Name = "WebApiActionSummary"
        CType(Me.gridActions,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)

End Sub

    Friend WithEvents gridActions As AutomateControls.DataGridViews.RowBasedDataGridView
    Friend WithEvents colName As DataGridViewTextBoxColumn
    Friend WithEvents colDescription As DataGridViewTextBoxColumn
    Friend WithEvents colMethod As DataGridViewTextBoxColumn
    Friend WithEvents colUrl As DataGridViewTextBoxColumn
End Class
