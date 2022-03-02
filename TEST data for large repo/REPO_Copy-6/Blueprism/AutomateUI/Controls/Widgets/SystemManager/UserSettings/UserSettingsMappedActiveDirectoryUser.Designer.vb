<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class UserSettingsMappedActiveDirectoryUser
    Inherits UserDetailsControl

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(UserSettingsMappedActiveDirectoryUser))
        Me.lblUserName = New System.Windows.Forms.Label()
        Me.llSearchDirectory = New System.Windows.Forms.LinkLabel()
        Me.lblSid = New System.Windows.Forms.Label()
        Me.txtSid = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtUserName = New AutomateControls.Textboxes.StyledTextBox()
        Me.SuspendLayout()
        '
        'lblUserName
        '
        resources.ApplyResources(Me.lblUserName, "lblUserName")
        Me.lblUserName.Name = "lblUserName"
        '
        'llSearchDirectory
        '
        resources.ApplyResources(Me.llSearchDirectory, "llSearchDirectory")
        Me.llSearchDirectory.Name = "llSearchDirectory"
        Me.llSearchDirectory.TabStop = True
        '
        'lblSid
        '
        resources.ApplyResources(Me.lblSid, "lblSid")
        Me.lblSid.Name = "lblSid"
        '
        'txtSid
        '
        resources.ApplyResources(Me.txtSid, "txtSid")
        Me.txtSid.BorderColor = System.Drawing.Color.Empty
        Me.txtSid.Name = "txtSid"
        '
        'txtUserName
        '
        resources.ApplyResources(Me.txtUserName, "txtUserName")
        Me.txtUserName.BorderColor = System.Drawing.Color.Empty
        Me.txtUserName.Name = "txtUserName"
        '
        'UserSettingsMappedActiveDirectoryUser
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.lblSid)
        Me.Controls.Add(Me.txtSid)
        Me.Controls.Add(Me.llSearchDirectory)
        Me.Controls.Add(Me.lblUserName)
        Me.Controls.Add(Me.txtUserName)
        Me.Name = "UserSettingsMappedActiveDirectoryUser"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

    Friend WithEvents txtUserName As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblUserName As Label
    Friend WithEvents llSearchDirectory As LinkLabel
    Friend WithEvents lblSid As Label
    Friend WithEvents txtSid As AutomateControls.Textboxes.StyledTextBox
End Class
