<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmSelectDependencies
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSelectDependencies))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.TitleBar = New AutomateControls.TitleBar()
        Me.ItemsDataGridView = New System.Windows.Forms.DataGridView()
        Me.ImageColumn = New System.Windows.Forms.DataGridViewImageColumn()
        Me.ItemColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.CheckBoxSelectColumn = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.ApplyChangesButton = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.CancelButtonControl = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.SelectAllCheckBox = New System.Windows.Forms.CheckBox()
        CType(Me.ItemsDataGridView, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TitleBar
        '
        resources.ApplyResources(Me.TitleBar, "TitleBar")
        Me.TitleBar.Name = "TitleBar"
        Me.TitleBar.Title = Global.AutomateUI.My.Resources.Resources.SelectDependentComponents
        '
        'ItemsDataGridView
        '
        Me.ItemsDataGridView.AllowUserToAddRows = False
        Me.ItemsDataGridView.AllowUserToDeleteRows = False
        Me.ItemsDataGridView.AllowUserToResizeRows = False
        resources.ApplyResources(Me.ItemsDataGridView, "ItemsDataGridView")
        Me.ItemsDataGridView.BackgroundColor = System.Drawing.Color.White
        Me.ItemsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.ItemsDataGridView.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.ImageColumn, Me.ItemColumn, Me.CheckBoxSelectColumn})
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle1.Font = New System.Drawing.Font("Segoe UI", 8.25!)
        DataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.ItemsDataGridView.DefaultCellStyle = DataGridViewCellStyle1
        Me.ItemsDataGridView.Name = "ItemsDataGridView"
        Me.ItemsDataGridView.RowHeadersVisible = False
        '
        'ImageColumn
        '
        Me.ImageColumn.Frozen = True
        resources.ApplyResources(Me.ImageColumn, "ImageColumn")
        Me.ImageColumn.Name = "ImageColumn"
        Me.ImageColumn.ReadOnly = True
        '
        'ItemColumn
        '
        Me.ItemColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.ItemColumn, "ItemColumn")
        Me.ItemColumn.Name = "ItemColumn"
        Me.ItemColumn.ReadOnly = True
        Me.ItemColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'CheckBoxSelectColumn
        '
        resources.ApplyResources(Me.CheckBoxSelectColumn, "CheckBoxSelectColumn")
        Me.CheckBoxSelectColumn.Name = "CheckBoxSelectColumn"
        '
        'ImageList1
        '
        Me.ImageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit
        resources.ApplyResources(Me.ImageList1, "ImageList1")
        Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
        '
        'ApplyChangesButton
        '
        resources.ApplyResources(Me.ApplyChangesButton, "ApplyChangesButton")
        Me.ApplyChangesButton.BackColor = System.Drawing.Color.White
        Me.ApplyChangesButton.Name = "ApplyChangesButton"
        Me.ApplyChangesButton.UseVisualStyleBackColor = False
        '
        'CancelButtonControl
        '
        resources.ApplyResources(Me.CancelButtonControl, "CancelButtonControl")
        Me.CancelButtonControl.BackColor = System.Drawing.Color.White
        Me.CancelButtonControl.Name = "CancelButtonControl"
        Me.CancelButtonControl.UseVisualStyleBackColor = False
        '
        'SelectAllCheckBox
        '
        resources.ApplyResources(Me.SelectAllCheckBox, "SelectAllCheckBox")
        Me.SelectAllCheckBox.Name = "SelectAllCheckBox"
        Me.SelectAllCheckBox.UseVisualStyleBackColor = True
        '
        'frmSelectDependencies
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.SelectAllCheckBox)
        Me.Controls.Add(Me.CancelButtonControl)
        Me.Controls.Add(Me.ApplyChangesButton)
        Me.Controls.Add(Me.ItemsDataGridView)
        Me.Controls.Add(Me.TitleBar)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmSelectDependencies"
        CType(Me.ItemsDataGridView, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TitleBar As AutomateControls.TitleBar
    Friend WithEvents ItemsDataGridView As System.Windows.Forms.DataGridView
    Friend WithEvents ImageList1 As System.Windows.Forms.ImageList
    Friend WithEvents ApplyChangesButton As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents CancelButtonControl As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents SelectAllCheckBox As CheckBox
    Friend WithEvents ImageColumn As DataGridViewImageColumn
    Friend WithEvents ItemColumn As DataGridViewTextBoxColumn
    Friend WithEvents CheckBoxSelectColumn As DataGridViewCheckBoxColumn
End Class
