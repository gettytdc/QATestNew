<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAppManConfig
    Inherits System.Windows.Forms.Form

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAppManConfig))
        Me.txtConfig = New System.Windows.Forms.TextBox()
        Me.btnSetDefault = New System.Windows.Forms.Button()
        Me.btnClear = New System.Windows.Forms.Button()
        Me.btnSaveAndExit = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'txtConfig
        '
        resources.ApplyResources(Me.txtConfig, "txtConfig")
        Me.txtConfig.Name = "txtConfig"
        '
        'btnSetDefault
        '
        resources.ApplyResources(Me.btnSetDefault, "btnSetDefault")
        Me.btnSetDefault.Name = "btnSetDefault"
        Me.btnSetDefault.UseVisualStyleBackColor = True
        '
        'btnClear
        '
        resources.ApplyResources(Me.btnClear, "btnClear")
        Me.btnClear.Name = "btnClear"
        Me.btnClear.UseVisualStyleBackColor = True
        '
        'btnSaveAndExit
        '
        resources.ApplyResources(Me.btnSaveAndExit, "btnSaveAndExit")
        Me.btnSaveAndExit.Name = "btnSaveAndExit"
        Me.btnSaveAndExit.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'frmAppManConfig
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnSaveAndExit)
        Me.Controls.Add(Me.btnClear)
        Me.Controls.Add(Me.btnSetDefault)
        Me.Controls.Add(Me.txtConfig)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Name = "frmAppManConfig"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtConfig As System.Windows.Forms.TextBox
    Friend WithEvents btnSetDefault As System.Windows.Forms.Button
    Friend WithEvents btnClear As System.Windows.Forms.Button
    Friend WithEvents btnSaveAndExit As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
End Class
