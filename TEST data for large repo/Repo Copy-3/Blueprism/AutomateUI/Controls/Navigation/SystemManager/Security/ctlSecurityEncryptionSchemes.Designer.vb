<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlSecurityEncryptionSchemes
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSecurityEncryptionSchemes))
        Me.lblCredEncrypter = New System.Windows.Forms.Label()
        Me.dgvKeys = New System.Windows.Forms.DataGridView()
        Me.colName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colMethod = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colLocation = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colStatus = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.llSchemeEdit = New AutomateControls.BulletedLinkLabel()
        Me.llSchemeDelete = New AutomateControls.BulletedLinkLabel()
        Me.llSchemeAdd = New AutomateControls.BulletedLinkLabel()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.cmbDefaultEncrypter = New AutomateControls.StyledComboBox()
        CType(Me.dgvKeys,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'lblCredEncrypter
        '
        resources.ApplyResources(Me.lblCredEncrypter, "lblCredEncrypter")
        Me.lblCredEncrypter.Name = "lblCredEncrypter"
        '
        'dgvKeys
        '
        Me.dgvKeys.AllowUserToAddRows = false
        Me.dgvKeys.AllowUserToDeleteRows = false
        Me.dgvKeys.AllowUserToResizeRows = false
        resources.ApplyResources(Me.dgvKeys, "dgvKeys")
        Me.dgvKeys.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvKeys.BackgroundColor = System.Drawing.Color.White
        Me.dgvKeys.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvKeys.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colName, Me.colMethod, Me.colLocation, Me.colStatus})
        Me.dgvKeys.MultiSelect = false
        Me.dgvKeys.Name = "dgvKeys"
        Me.dgvKeys.RowHeadersVisible = false
        Me.dgvKeys.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'colName
        '
        resources.ApplyResources(Me.colName, "colName")
        Me.colName.Name = "colName"
        Me.colName.ReadOnly = true
        '
        'colMethod
        '
        resources.ApplyResources(Me.colMethod, "colMethod")
        Me.colMethod.Name = "colMethod"
        Me.colMethod.ReadOnly = true
        '
        'colLocation
        '
        resources.ApplyResources(Me.colLocation, "colLocation")
        Me.colLocation.Name = "colLocation"
        Me.colLocation.ReadOnly = true
        '
        'colStatus
        '
        Me.colStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colStatus, "colStatus")
        Me.colStatus.Name = "colStatus"
        Me.colStatus.ReadOnly = true
        '
        'llSchemeEdit
        '
        resources.ApplyResources(Me.llSchemeEdit, "llSchemeEdit")
        Me.llSchemeEdit.LinkColor = System.Drawing.SystemColors.ControlText
        Me.llSchemeEdit.Name = "llSchemeEdit"
        Me.llSchemeEdit.TabStop = true
        Me.llSchemeEdit.UseCompatibleTextRendering = true
        '
        'llSchemeDelete
        '
        resources.ApplyResources(Me.llSchemeDelete, "llSchemeDelete")
        Me.llSchemeDelete.LinkColor = System.Drawing.SystemColors.ControlText
        Me.llSchemeDelete.Name = "llSchemeDelete"
        Me.llSchemeDelete.TabStop = true
        Me.llSchemeDelete.UseCompatibleTextRendering = true
        '
        'llSchemeAdd
        '
        resources.ApplyResources(Me.llSchemeAdd, "llSchemeAdd")
        Me.llSchemeAdd.LinkColor = System.Drawing.SystemColors.ControlText
        Me.llSchemeAdd.Name = "llSchemeAdd"
        Me.llSchemeAdd.TabStop = true
        Me.llSchemeAdd.UseCompatibleTextRendering = true
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.Label1.Name = "Label1"
        '
        'cmbDefaultEncrypter
        '
        resources.ApplyResources(Me.cmbDefaultEncrypter, "cmbDefaultEncrypter")
        Me.cmbDefaultEncrypter.Checkable = false
        Me.cmbDefaultEncrypter.FormattingEnabled = true
        Me.cmbDefaultEncrypter.Name = "cmbDefaultEncrypter"
        '
        'ctlSecurityEncryptionSchemes
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.cmbDefaultEncrypter)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.llSchemeEdit)
        Me.Controls.Add(Me.llSchemeDelete)
        Me.Controls.Add(Me.llSchemeAdd)
        Me.Controls.Add(Me.lblCredEncrypter)
        Me.Controls.Add(Me.dgvKeys)
        Me.Name = "ctlSecurityEncryptionSchemes"
        resources.ApplyResources(Me, "$this")
        CType(Me.dgvKeys,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Friend WithEvents lblCredEncrypter As System.Windows.Forms.Label
    Friend WithEvents dgvKeys As System.Windows.Forms.DataGridView
    Private WithEvents llSchemeEdit As AutomateControls.BulletedLinkLabel
    Private WithEvents llSchemeDelete As AutomateControls.BulletedLinkLabel
    Private WithEvents llSchemeAdd As AutomateControls.BulletedLinkLabel
    Private WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents cmbDefaultEncrypter As AutomateControls.StyledComboBox
    Friend WithEvents colName As DataGridViewTextBoxColumn
    Friend WithEvents colMethod As DataGridViewTextBoxColumn
    Friend WithEvents colLocation As DataGridViewTextBoxColumn
    Friend WithEvents colStatus As DataGridViewTextBoxColumn
End Class
