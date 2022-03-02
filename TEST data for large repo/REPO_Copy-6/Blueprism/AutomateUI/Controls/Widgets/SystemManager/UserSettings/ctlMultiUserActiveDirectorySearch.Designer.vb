<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlMultiUserActiveDirectorySearch
    Inherits ActiveDirectoryUserSearchBase

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
        Me.lblTotalNumberOfSelectedUsers = New System.Windows.Forms.Label()
        Me.SuspendLayout
        '
        'lblTotalNumberOfSelectedUsers
        '
        Me.lblTotalNumberOfSelectedUsers.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.lblTotalNumberOfSelectedUsers.Location = New System.Drawing.Point(423, 402)
        Me.lblTotalNumberOfSelectedUsers.Name = "lblTotalNumberOfSelectedUsers"
        Me.lblTotalNumberOfSelectedUsers.Size = New System.Drawing.Size(280, 13)
        Me.lblTotalNumberOfSelectedUsers.TabIndex = 20
        Me.lblTotalNumberOfSelectedUsers.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'ctlMultiUserActiveDirectorySearch
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.lblTotalNumberOfSelectedUsers)
        Me.Name = "ctlMultiUserActiveDirectorySearch"
        Me.Controls.SetChildIndex(Me.lblTotalNumberOfSelectedUsers, 0)
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

    Friend WithEvents lblTotalNumberOfSelectedUsers As Label
End Class
