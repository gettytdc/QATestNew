<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class DatabaseConversionCreateNativeAdminUser
    Inherits AutomateControls.Forms.AutomateForm

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(DatabaseConversionCreateNativeAdminUser))
        Me.titleBar = New AutomateControls.TitleBar()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnNext = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.lblFormDescription = New System.Windows.Forms.Label()
        Me.UserSettingsNameAndPassword = New AutomateUI.UserSettingsNameAndPassword()
        Me.SuspendLayout()
        '
        'titleBar
        '
        resources.ApplyResources(Me.titleBar, "titleBar")
        Me.titleBar.Name = "titleBar"
        Me.titleBar.SubtitleFont = New System.Drawing.Font("Segoe UI", 12!, System.Drawing.FontStyle.Bold)
        Me.titleBar.TitleFont = New System.Drawing.Font("Segoe UI", 12!)
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.BackColor = System.Drawing.Color.White
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = False
        '
        'btnNext
        '
        resources.ApplyResources(Me.btnNext, "btnNext")
        Me.btnNext.BackColor = System.Drawing.Color.White
        Me.btnNext.Name = "btnNext"
        Me.btnNext.UseVisualStyleBackColor = False
        '
        'lblFormDescription
        '
        resources.ApplyResources(Me.lblFormDescription, "lblFormDescription")
        Me.lblFormDescription.Name = "lblFormDescription"
        '
        'UserSettingsNameAndPassword
        '
        resources.ApplyResources(Me.UserSettingsNameAndPassword, "UserSettingsNameAndPassword")
        Me.UserSettingsNameAndPassword.Name = "UserSettingsNameAndPassword"
        Me.UserSettingsNameAndPassword.UseBluePrismAccessibility = True
        '
        'DatabaseConversionCreateNativeAdminUser
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.UserSettingsNameAndPassword)
        Me.Controls.Add(Me.lblFormDescription)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnNext)
        Me.Controls.Add(Me.titleBar)
        Me.Name = "DatabaseConversionCreateNativeAdminUser"
        Me.ResumeLayout(False)

End Sub

    Friend WithEvents titleBar As AutomateControls.TitleBar
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnNext As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lblFormDescription As Label
    Friend WithEvents UserSettingsNameAndPassword As UserSettingsNameAndPassword
End Class
