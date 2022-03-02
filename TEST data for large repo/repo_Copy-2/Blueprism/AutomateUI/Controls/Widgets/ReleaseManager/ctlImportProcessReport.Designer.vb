<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlImportProcessReport
    Inherits ctlWizardStageControl

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlImportProcessReport))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle5 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle6 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle7 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle8 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle9 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.pnlLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.pnlErrors = New System.Windows.Forms.Panel()
        Me.mContents = New System.Windows.Forms.DataGridView()
        Me.ComponentColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ImageColumn = New System.Windows.Forms.DataGridViewImageColumn()
        Me.IssueColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ResolutionColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ImportedAsColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.btnExport = New AutomateControls.Buttons.StandardStyledButton()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewImageColumn1 = New System.Windows.Forms.DataGridViewImageColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.pnlLayout.SuspendLayout
        CType(Me.mContents,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'pnlLayout
        '
        resources.ApplyResources(Me.pnlLayout, "pnlLayout")
        Me.pnlLayout.Controls.Add(Me.pnlErrors, 0, 0)
        Me.pnlLayout.Controls.Add(Me.mContents, 0, 1)
        Me.pnlLayout.Name = "pnlLayout"
        '
        'pnlErrors
        '
        resources.ApplyResources(Me.pnlErrors, "pnlErrors")
        Me.pnlErrors.Name = "pnlErrors"
        '
        'mContents
        '
        Me.mContents.AllowUserToAddRows = false
        Me.mContents.AllowUserToDeleteRows = false
        Me.mContents.AllowUserToResizeRows = false
        Me.mContents.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.mContents.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.mContents.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.ComponentColumn, Me.ImageColumn, Me.IssueColumn, Me.ResolutionColumn, Me.ImportedAsColumn})
        resources.ApplyResources(Me.mContents, "mContents")
        Me.mContents.MultiSelect = false
        Me.mContents.Name = "mContents"
        Me.mContents.RowHeadersVisible = false
        Me.mContents.RowTemplate.Height = 28
        '
        'ComponentColumn
        '
        Me.ComponentColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.InactiveBorder
        Me.ComponentColumn.DefaultCellStyle = DataGridViewCellStyle1
        Me.ComponentColumn.FillWeight = 96.15385!
        resources.ApplyResources(Me.ComponentColumn, "ComponentColumn")
        Me.ComponentColumn.Name = "ComponentColumn"
        Me.ComponentColumn.ReadOnly = true
        '
        'ImageColumn
        '
        Me.ImageColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle2.NullValue = CType(resources.GetObject("DataGridViewCellStyle2.NullValue"),Object)
        Me.ImageColumn.DefaultCellStyle = DataGridViewCellStyle2
        Me.ImageColumn.FillWeight = 115.3846!
        resources.ApplyResources(Me.ImageColumn, "ImageColumn")
        Me.ImageColumn.Name = "ImageColumn"
        Me.ImageColumn.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'IssueColumn
        '
        DataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.InactiveBorder
        Me.IssueColumn.DefaultCellStyle = DataGridViewCellStyle3
        Me.IssueColumn.FillWeight = 96.15385!
        resources.ApplyResources(Me.IssueColumn, "IssueColumn")
        Me.IssueColumn.Name = "IssueColumn"
        Me.IssueColumn.ReadOnly = true
        '
        'ResolutionColumn
        '
        Me.ResolutionColumn.FillWeight = 96.15385!
        resources.ApplyResources(Me.ResolutionColumn, "ResolutionColumn")
        Me.ResolutionColumn.Name = "ResolutionColumn"
        Me.ResolutionColumn.ReadOnly = true
        Me.ResolutionColumn.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.ResolutionColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'ImportedAsColumn
        '
        DataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.InactiveBorder
        Me.ImportedAsColumn.DefaultCellStyle = DataGridViewCellStyle4
        resources.ApplyResources(Me.ImportedAsColumn, "ImportedAsColumn")
        Me.ImportedAsColumn.Name = "ImportedAsColumn"
        Me.ImportedAsColumn.ReadOnly = true
        Me.ImportedAsColumn.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.ImportedAsColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'btnExport
        '
        resources.ApplyResources(Me.btnExport, "btnExport")
        Me.btnExport.Name = "btnExport"
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        DataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.InactiveBorder
        Me.DataGridViewTextBoxColumn1.DefaultCellStyle = DataGridViewCellStyle5
        Me.DataGridViewTextBoxColumn1.FillWeight = 96.15385!
        Me.DataGridViewTextBoxColumn1.Frozen = true
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = true
        '
        'DataGridViewImageColumn1
        '
        Me.DataGridViewImageColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        DataGridViewCellStyle6.NullValue = CType(resources.GetObject("DataGridViewCellStyle6.NullValue"),Object)
        Me.DataGridViewImageColumn1.DefaultCellStyle = DataGridViewCellStyle6
        Me.DataGridViewImageColumn1.FillWeight = 30!
        Me.DataGridViewImageColumn1.Frozen = true
        resources.ApplyResources(Me.DataGridViewImageColumn1, "DataGridViewImageColumn1")
        Me.DataGridViewImageColumn1.Name = "DataGridViewImageColumn1"
        Me.DataGridViewImageColumn1.ReadOnly = true
        Me.DataGridViewImageColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'DataGridViewTextBoxColumn2
        '
        DataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.InactiveBorder
        Me.DataGridViewTextBoxColumn2.DefaultCellStyle = DataGridViewCellStyle7
        Me.DataGridViewTextBoxColumn2.FillWeight = 96.15385!
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.ReadOnly = true
        '
        'DataGridViewTextBoxColumn3
        '
        DataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.HighlightText
        Me.DataGridViewTextBoxColumn3.DefaultCellStyle = DataGridViewCellStyle8
        Me.DataGridViewTextBoxColumn3.FillWeight = 96.15385!
        resources.ApplyResources(Me.DataGridViewTextBoxColumn3, "DataGridViewTextBoxColumn3")
        Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
        Me.DataGridViewTextBoxColumn3.ReadOnly = true
        Me.DataGridViewTextBoxColumn3.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.DataGridViewTextBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'DataGridViewTextBoxColumn4
        '
        Me.DataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        DataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.InactiveCaption
        Me.DataGridViewTextBoxColumn4.DefaultCellStyle = DataGridViewCellStyle9
        Me.DataGridViewTextBoxColumn4.FillWeight = 30!
        resources.ApplyResources(Me.DataGridViewTextBoxColumn4, "DataGridViewTextBoxColumn4")
        Me.DataGridViewTextBoxColumn4.Name = "DataGridViewTextBoxColumn4"
        Me.DataGridViewTextBoxColumn4.ReadOnly = true
        Me.DataGridViewTextBoxColumn4.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.DataGridViewTextBoxColumn4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'ctlImportProcessReport
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.btnExport)
        Me.Controls.Add(Me.pnlLayout)
        Me.Name = "ctlImportProcessReport"
        resources.ApplyResources(Me, "$this")
        Me.pnlLayout.ResumeLayout(false)
        Me.pnlLayout.PerformLayout
        CType(Me.mContents,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)

End Sub

    Friend WithEvents pnlLayout As TableLayoutPanel
    Friend WithEvents pnlErrors As Panel
    Friend WithEvents mContents As DataGridView
    Friend WithEvents ArgumentsColumn As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewImageColumn1 As DataGridViewImageColumn
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn3 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn4 As DataGridViewTextBoxColumn
    Protected WithEvents btnExport As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents ComponentColumn As DataGridViewTextBoxColumn
    Friend WithEvents ImageColumn As DataGridViewImageColumn
    Friend WithEvents IssueColumn As DataGridViewTextBoxColumn
    Friend WithEvents ResolutionColumn As DataGridViewTextBoxColumn
    Friend WithEvents ImportedAsColumn As DataGridViewTextBoxColumn
End Class
