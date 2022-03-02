<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class WebApiAuthenticationPanel
    Inherits System.Windows.Forms.UserControl

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(WebApiAuthenticationPanel))
        Me.cmbAuthType = New System.Windows.Forms.ComboBox()
        Me.pnlEditAuth = New System.Windows.Forms.TableLayoutPanel()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.lblAuthType = New System.Windows.Forms.Label()
        Me.pnlConfiguration = New System.Windows.Forms.Panel()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.pnlEditAuth.SuspendLayout
        Me.Panel1.SuspendLayout
        Me.SuspendLayout
        '
        'cmbAuthType
        '
        resources.ApplyResources(Me.cmbAuthType, "cmbAuthType")
        Me.cmbAuthType.DisplayMember = "Key"
        Me.cmbAuthType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbAuthType.FormattingEnabled = true
        Me.cmbAuthType.Name = "cmbAuthType"
        Me.cmbAuthType.ValueMember = "Value"
        '
        'pnlEditAuth
        '
        resources.ApplyResources(Me.pnlEditAuth, "pnlEditAuth")
        Me.pnlEditAuth.Controls.Add(Me.Panel1, 0, 0)
        Me.pnlEditAuth.Controls.Add(Me.pnlConfiguration, 0, 1)
        Me.pnlEditAuth.Name = "pnlEditAuth"
        '
        'Panel1
        '
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Controls.Add(Me.lblAuthType)
        Me.Panel1.Controls.Add(Me.cmbAuthType)
        Me.Panel1.Name = "Panel1"
        '
        'lblAuthType
        '
        resources.ApplyResources(Me.lblAuthType, "lblAuthType")
        Me.lblAuthType.Name = "lblAuthType"
        '
        'pnlConfiguration
        '
        resources.ApplyResources(Me.pnlConfiguration, "pnlConfiguration")
        Me.pnlConfiguration.Name = "pnlConfiguration"
        '
        'DataGridViewTextBoxColumn1
        '
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        '
        'DataGridViewTextBoxColumn2
        '
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        '
        'DataGridViewTextBoxColumn3
        '
        resources.ApplyResources(Me.DataGridViewTextBoxColumn3, "DataGridViewTextBoxColumn3")
        Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
        '
        'DataGridViewTextBoxColumn4
        '
        resources.ApplyResources(Me.DataGridViewTextBoxColumn4, "DataGridViewTextBoxColumn4")
        Me.DataGridViewTextBoxColumn4.Name = "DataGridViewTextBoxColumn4"
        '
        'WebApiAuthenticationPanel
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.pnlEditAuth)
        Me.Name = "WebApiAuthenticationPanel"
        Me.pnlEditAuth.ResumeLayout(false)
        Me.Panel1.ResumeLayout(false)
        Me.Panel1.PerformLayout
        Me.ResumeLayout(false)

End Sub
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn3 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn4 As DataGridViewTextBoxColumn
    Friend WithEvents pnlEditAuth As TableLayoutPanel
    Friend WithEvents pnlConfiguration As Panel
    Friend WithEvents cmbAuthType As ComboBox
    Friend WithEvents lblAuthType As Label
    Friend WithEvents Panel1 As Panel
End Class
