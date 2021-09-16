<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlConflictSet

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
        Me.components = New System.ComponentModel.Container()
        Dim DataGridViewCellStyle5 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle6 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle7 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlConflictSet))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.mContents = New System.Windows.Forms.DataGridView()
        Me.pnlErrors = New System.Windows.Forms.Panel()
        Me.pnlLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ResolutionToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.ImageColumn = New System.Windows.Forms.DataGridViewImageColumn()
        Me.ComponentColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.IssueColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ResolutionColumn = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.ArgumentsColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        CType(Me.mContents, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlLayout.SuspendLayout()
        Me.SuspendLayout()
        '
        'mContents
        '
        Me.mContents.AllowUserToAddRows = False
        Me.mContents.AllowUserToDeleteRows = False
        Me.mContents.AllowUserToResizeRows = False
        Me.mContents.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.mContents.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.mContents.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.ImageColumn, Me.ComponentColumn, Me.IssueColumn, Me.ResolutionColumn, Me.ArgumentsColumn})
        resources.ApplyResources(Me.mContents, "mContents")
        Me.mContents.MultiSelect = False
        Me.mContents.Name = "mContents"
        Me.mContents.RowHeadersVisible = False
        Me.mContents.RowTemplate.Height = 42
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
        Me.pnlLayout.Controls.Add(Me.mContents, 0, 1)
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
        'ImageColumn
        '
        Me.ImageColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.ImageColumn.FillWeight = 20.0!
        resources.ApplyResources(Me.ImageColumn, "ImageColumn")
        Me.ImageColumn.Name = "ImageColumn"
        Me.ImageColumn.ReadOnly = True
        Me.ImageColumn.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'ComponentColumn
        '
        DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.InactiveBorder
        Me.ComponentColumn.DefaultCellStyle = DataGridViewCellStyle1
        Me.ComponentColumn.FillWeight = 60.0!
        resources.ApplyResources(Me.ComponentColumn, "ComponentColumn")
        Me.ComponentColumn.Name = "ComponentColumn"
        Me.ComponentColumn.ReadOnly = True
        '
        'IssueColumn
        '
        DataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.InactiveBorder
        DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.IssueColumn.DefaultCellStyle = DataGridViewCellStyle2
        Me.IssueColumn.FillWeight = 180.0!
        resources.ApplyResources(Me.IssueColumn, "IssueColumn")
        Me.IssueColumn.Name = "IssueColumn"
        Me.IssueColumn.ReadOnly = True
        '
        'ResolutionColumn
        '
        Me.ResolutionColumn.AutoComplete = False
        DataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.HighlightText
        Me.ResolutionColumn.DefaultCellStyle = DataGridViewCellStyle3
        Me.ResolutionColumn.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox
        Me.ResolutionColumn.FillWeight = 110.0!
        resources.ApplyResources(Me.ResolutionColumn, "ResolutionColumn")
        Me.ResolutionColumn.Name = "ResolutionColumn"
        '
        'ArgumentsColumn
        '
        Me.ArgumentsColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        DataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.InactiveCaption
        Me.ArgumentsColumn.DefaultCellStyle = DataGridViewCellStyle4
        Me.ArgumentsColumn.FillWeight = 30.0!
        resources.ApplyResources(Me.ArgumentsColumn, "ArgumentsColumn")
        Me.ArgumentsColumn.Name = "ArgumentsColumn"
        Me.ArgumentsColumn.ReadOnly = True
        Me.ArgumentsColumn.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.ArgumentsColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'ctlConflictSet
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.pnlLayout)
        Me.Name = "ctlConflictSet"
        resources.ApplyResources(Me, "$this")
        CType(Me.mContents, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlLayout.ResumeLayout(False)
        Me.pnlLayout.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents mContents As DataGridView
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn3 As DataGridViewTextBoxColumn
    Friend WithEvents pnlErrors As Panel
    Friend WithEvents pnlLayout As TableLayoutPanel
    Friend WithEvents ResolutionToolTip As ToolTip
    Friend WithEvents ImageColumn As DataGridViewImageColumn
    Friend WithEvents ComponentColumn As DataGridViewTextBoxColumn
    Friend WithEvents IssueColumn As DataGridViewTextBoxColumn
    Friend WithEvents ResolutionColumn As DataGridViewComboBoxColumn
    Friend WithEvents ArgumentsColumn As DataGridViewTextBoxColumn
End Class
