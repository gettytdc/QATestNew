<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlLogLevels

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlLogLevels))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle5 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle6 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle7 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.ContentsDataGridView = New System.Windows.Forms.DataGridView()
        Me.ImageColumn = New System.Windows.Forms.DataGridViewImageColumn()
        Me.ComponentColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.NumberOfStagesColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.NumberOfStagesLoggingColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.LoggingPercentageColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.pnlErrors = New System.Windows.Forms.Panel()
        Me.pnlLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        CType(Me.ContentsDataGridView, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlLayout.SuspendLayout()
        Me.SuspendLayout()
        '
        'ContentsDataGridView
        '
        Me.ContentsDataGridView.AllowUserToAddRows = False
        Me.ContentsDataGridView.AllowUserToDeleteRows = False
        Me.ContentsDataGridView.AllowUserToOrderColumns = True
        Me.ContentsDataGridView.AllowUserToResizeRows = False
        Me.ContentsDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.ContentsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.ContentsDataGridView.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.ImageColumn, Me.ComponentColumn, Me.NumberOfStagesColumn, Me.NumberOfStagesLoggingColumn, Me.LoggingPercentageColumn})
        resources.ApplyResources(Me.ContentsDataGridView, "ContentsDataGridView")
        Me.ContentsDataGridView.MultiSelect = False
        Me.ContentsDataGridView.Name = "ContentsDataGridView"
        Me.ContentsDataGridView.ReadOnly = True
        Me.ContentsDataGridView.RowHeadersVisible = False
        Me.ContentsDataGridView.RowTemplate.Height = 28
        '
        'ImageColumn
        '
        Me.ImageColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.ImageColumn.FillWeight = 30.0!
        resources.ApplyResources(Me.ImageColumn, "ImageColumn")
        Me.ImageColumn.Name = "ImageColumn"
        Me.ImageColumn.ReadOnly = True
        Me.ImageColumn.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'ComponentColumn
        '
        Me.ComponentColumn.DataPropertyName = "Name"
        DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.InactiveBorder
        Me.ComponentColumn.DefaultCellStyle = DataGridViewCellStyle1
        resources.ApplyResources(Me.ComponentColumn, "ComponentColumn")
        Me.ComponentColumn.Name = "ComponentColumn"
        Me.ComponentColumn.ReadOnly = True
        '
        'NumberOfStagesColumn
        '
        Me.NumberOfStagesColumn.DataPropertyName = "NumberOfStages"
        DataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.InactiveBorder
        Me.NumberOfStagesColumn.DefaultCellStyle = DataGridViewCellStyle2
        resources.ApplyResources(Me.NumberOfStagesColumn, "NumberOfStagesColumn")
        Me.NumberOfStagesColumn.Name = "NumberOfStagesColumn"
        Me.NumberOfStagesColumn.ReadOnly = True
        '
        'NumberOfStagesLoggingColumn
        '
        Me.NumberOfStagesLoggingColumn.DataPropertyName = "NumberOfStagesLogging"
        DataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.InactiveBorder
        Me.NumberOfStagesLoggingColumn.DefaultCellStyle = DataGridViewCellStyle3
        resources.ApplyResources(Me.NumberOfStagesLoggingColumn, "NumberOfStagesLoggingColumn")
        Me.NumberOfStagesLoggingColumn.Name = "NumberOfStagesLoggingColumn"
        Me.NumberOfStagesLoggingColumn.ReadOnly = True
        '
        'LoggingPercentageColumn
        '
        Me.LoggingPercentageColumn.DataPropertyName = "LoggingPercentage"
        DataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.InactiveBorder
        Me.LoggingPercentageColumn.DefaultCellStyle = DataGridViewCellStyle4
        resources.ApplyResources(Me.LoggingPercentageColumn, "LoggingPercentageColumn")
        Me.LoggingPercentageColumn.Name = "LoggingPercentageColumn"
        Me.LoggingPercentageColumn.ReadOnly = True
        '
        'pnlErrors
        '
        resources.ApplyResources(Me.pnlErrors, "pnlErrors")
        Me.pnlErrors.Name = "pnlErrors"
        '
        'pnlLayout
        '
        resources.ApplyResources(Me.pnlLayout, "pnlLayout")
        Me.pnlLayout.Controls.Add(Me.pnlErrors, 0, 0)
        Me.pnlLayout.Controls.Add(Me.ContentsDataGridView, 0, 1)
        Me.pnlLayout.Name = "pnlLayout"
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.DataPropertyName = "Component.Type.Plural"
        DataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.InactiveBorder
        Me.DataGridViewTextBoxColumn1.DefaultCellStyle = DataGridViewCellStyle5
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = True
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.InactiveBorder
        Me.DataGridViewTextBoxColumn2.DefaultCellStyle = DataGridViewCellStyle6
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.ReadOnly = True
        '
        'DataGridViewTextBoxColumn3
        '
        Me.DataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        DataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.InactiveCaption
        Me.DataGridViewTextBoxColumn3.DefaultCellStyle = DataGridViewCellStyle7
        Me.DataGridViewTextBoxColumn3.FillWeight = 30.0!
        resources.ApplyResources(Me.DataGridViewTextBoxColumn3, "DataGridViewTextBoxColumn3")
        Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
        Me.DataGridViewTextBoxColumn3.ReadOnly = True
        Me.DataGridViewTextBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'ctlLogLevels
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.pnlLayout)
        Me.Name = "ctlLogLevels"
        resources.ApplyResources(Me, "$this")
        CType(Me.ContentsDataGridView, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlLayout.ResumeLayout(False)
        Me.pnlLayout.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents ContentsDataGridView As DataGridView
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn3 As DataGridViewTextBoxColumn
    Friend WithEvents pnlErrors As Panel
    Friend WithEvents pnlLayout As TableLayoutPanel
    Friend WithEvents ImageColumn As DataGridViewImageColumn
    Friend WithEvents ComponentColumn As DataGridViewTextBoxColumn
    Friend WithEvents NumberOfStagesColumn As DataGridViewTextBoxColumn
    Friend WithEvents NumberOfStagesLoggingColumn As DataGridViewTextBoxColumn
    Friend WithEvents LoggingPercentageColumn As DataGridViewTextBoxColumn
End Class
