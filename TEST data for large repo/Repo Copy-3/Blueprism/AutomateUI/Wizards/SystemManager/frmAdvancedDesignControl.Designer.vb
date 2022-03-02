<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAdvancedDesignControl
    Inherits frmForm

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
    End Sub

    'Form overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAdvancedDesignControl))
        Me.tblChecks = New System.Windows.Forms.DataGridView()
        Me.colCheckID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colEnabled = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.colDescription = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colSeverity = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnOk = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnHelp = New AutomateControls.Buttons.StandardStyledButton()
        Me.titleBar = New AutomateControls.TitleBar()
        CType(Me.tblChecks, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'tblChecks
        '
        Me.tblChecks.AllowUserToAddRows = False
        Me.tblChecks.AllowUserToDeleteRows = False
        Me.tblChecks.AllowUserToResizeRows = False
        resources.ApplyResources(Me.tblChecks, "tblChecks")
        Me.tblChecks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.tblChecks.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colCheckID, Me.colEnabled, Me.colDescription, Me.colSeverity})
        Me.tblChecks.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter
        Me.tblChecks.Name = "tblChecks"
        Me.tblChecks.RowHeadersVisible = False
        Me.tblChecks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'colCheckID
        '
        resources.ApplyResources(Me.colCheckID, "colCheckID")
        Me.colCheckID.Name = "colCheckID"
        Me.colCheckID.ReadOnly = True
        '
        'colEnabled
        '
        Me.colEnabled.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader
        resources.ApplyResources(Me.colEnabled, "colEnabled")
        Me.colEnabled.Name = "colEnabled"
        Me.colEnabled.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'colDescription
        '
        Me.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colDescription, "colDescription")
        Me.colDescription.Name = "colDescription"
        Me.colDescription.ReadOnly = True
        '
        'colSeverity
        '
        Me.colSeverity.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.colSeverity, "colSeverity")
        Me.colSeverity.Name = "colSeverity"
        Me.colSeverity.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'btnOk
        '
        resources.ApplyResources(Me.btnOk, "btnOk")
        Me.btnOk.Name = "btnOk"
        Me.btnOk.UseVisualStyleBackColor = True
        '
        'btnHelp
        '
        resources.ApplyResources(Me.btnHelp, "btnHelp")
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'titleBar
        '
        resources.ApplyResources(Me.titleBar, "titleBar")
        Me.titleBar.Name = "titleBar"
        '
        'frmAdvancedDesignControl
        '
        Me.AcceptButton = Me.btnOk
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.CancelButton = Me.btnCancel
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.tblChecks)
        Me.Controls.Add(Me.btnOk)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.titleBar)
        Me.Name = "frmAdvancedDesignControl"
        CType(Me.tblChecks, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents titleBar As AutomateControls.TitleBar
    Friend WithEvents tblChecks As System.Windows.Forms.DataGridView
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnOk As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnHelp As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents colCheckID As DataGridViewTextBoxColumn
    Friend WithEvents colEnabled As DataGridViewCheckBoxColumn
    Friend WithEvents colDescription As DataGridViewTextBoxColumn
    Friend WithEvents colSeverity As DataGridViewComboBoxColumn
End Class
