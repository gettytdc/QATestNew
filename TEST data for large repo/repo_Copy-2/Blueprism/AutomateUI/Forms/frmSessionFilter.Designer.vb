<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmSessionFilter
    Inherits AutomateUI.frmForm

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSessionFilter))
        Me.DataGridView1 = New System.Windows.Forms.DataGridView()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.lnkSelectNone = New System.Windows.Forms.LinkLabel()
        Me.lnkSelectAll = New System.Windows.Forms.LinkLabel()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'DataGridView1
        '
        Me.DataGridView1.AllowUserToAddRows = False
        Me.DataGridView1.AllowUserToDeleteRows = False
        resources.ApplyResources(Me.DataGridView1, "DataGridView1")
        Me.DataGridView1.BackgroundColor = System.Drawing.Color.White
        Me.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView1.Name = "DataGridView1"
        Me.DataGridView1.RowHeadersVisible = False
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'lnkSelectNone
        '
        resources.ApplyResources(Me.lnkSelectNone, "lnkSelectNone")
        Me.lnkSelectNone.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.lnkSelectNone.Name = "lnkSelectNone"
        Me.lnkSelectNone.TabStop = True
        '
        'lnkSelectAll
        '
        resources.ApplyResources(Me.lnkSelectAll, "lnkSelectAll")
        Me.lnkSelectAll.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.lnkSelectAll.Name = "lnkSelectAll"
        Me.lnkSelectAll.TabStop = True
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'frmSessionFilter
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.lnkSelectNone)
        Me.Controls.Add(Me.lnkSelectAll)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.DataGridView1)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmSessionFilter"
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents DataGridView1 As System.Windows.Forms.DataGridView
    Friend WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lnkSelectNone As System.Windows.Forms.LinkLabel
    Friend WithEvents lnkSelectAll As System.Windows.Forms.LinkLabel
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton

End Class
