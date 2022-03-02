<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlSystemDocumentProcessingQueueManagement
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSystemDocumentProcessingQueueManagement))
        Me.mDocumentTypeQueueGrid = New System.Windows.Forms.DataGridView()
        Me.documentTypeColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.queueColumn = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        CType(Me.mDocumentTypeQueueGrid, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'mDocumentTypeQueueGrid
        '
        Me.mDocumentTypeQueueGrid.AllowUserToAddRows = False
        Me.mDocumentTypeQueueGrid.AllowUserToDeleteRows = False
        Me.mDocumentTypeQueueGrid.AllowUserToOrderColumns = True
        Me.mDocumentTypeQueueGrid.BackgroundColor = System.Drawing.SystemColors.ButtonFace
        Me.mDocumentTypeQueueGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.mDocumentTypeQueueGrid.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.documentTypeColumn, Me.queueColumn})
        resources.ApplyResources(Me.mDocumentTypeQueueGrid, "mDocumentTypeQueueGrid")
        Me.mDocumentTypeQueueGrid.Name = "mDocumentTypeQueueGrid"
        Me.mDocumentTypeQueueGrid.RowHeadersVisible = False
        '
        'documentTypeColumn
        '
        Me.documentTypeColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.documentTypeColumn, "documentTypeColumn")
        Me.documentTypeColumn.Name = "documentTypeColumn"
        '
        'queueColumn
        '
        Me.queueColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.queueColumn, "queueColumn")
        Me.queueColumn.Name = "queueColumn"
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        '
        'ctlSystemDocumentProcessingQueueManagement
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.mDocumentTypeQueueGrid)
        Me.Name = "ctlSystemDocumentProcessingQueueManagement"
        resources.ApplyResources(Me, "$this")
        CType(Me.mDocumentTypeQueueGrid, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(false)

End Sub

    Friend WithEvents mDocumentTypeQueueGrid As DataGridView
    Friend WithEvents documentTypeColumn As DataGridViewTextBoxColumn
    Friend WithEvents queueColumn As DataGridViewComboBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
End Class
