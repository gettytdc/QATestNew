<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class WebApiActionPanel
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(WebApiActionPanel))
        Me.txtName = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblName = New System.Windows.Forms.Label()
        Me.lblDescription = New System.Windows.Forms.Label()
        Me.txtDescription = New AutomateControls.Textboxes.StyledTextBox()
        Me.cbEnabled = New System.Windows.Forms.CheckBox()
        Me.cbEnableRequestOutputParameter = New System.Windows.Forms.CheckBox()
        Me.cbDisableSendingOfRequest = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        Me.txtName.Name = "txtName"
        '
        'lblName
        '
        resources.ApplyResources(Me.lblName, "lblName")
        Me.lblName.Name = "lblName"
        '
        'lblDescription
        '
        resources.ApplyResources(Me.lblDescription, "lblDescription")
        Me.lblDescription.Name = "lblDescription"
        '
        'txtDescription
        '
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        Me.txtDescription.Name = "txtDescription"
        '
        'cbEnabled
        '
        resources.ApplyResources(Me.cbEnabled, "cbEnabled")
        Me.cbEnabled.Name = "cbEnabled"
        Me.cbEnabled.UseVisualStyleBackColor = true
        '
        'cbEnableRequestOutputParameter
        '
        resources.ApplyResources(Me.cbEnableRequestOutputParameter, "cbEnableRequestOutputParameter")
        Me.cbEnableRequestOutputParameter.Name = "cbEnableRequestOutputParameter"
        Me.cbEnableRequestOutputParameter.UseVisualStyleBackColor = true
        '
        'cbDisableSendingOfRequest
        '
        resources.ApplyResources(Me.cbDisableSendingOfRequest, "cbDisableSendingOfRequest")
        Me.cbDisableSendingOfRequest.Name = "cbDisableSendingOfRequest"
        Me.cbDisableSendingOfRequest.UseMnemonic = false
        Me.cbDisableSendingOfRequest.UseVisualStyleBackColor = true
        '
        'WebApiActionPanel
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.cbDisableSendingOfRequest)
        Me.Controls.Add(Me.cbEnableRequestOutputParameter)
        Me.Controls.Add(Me.cbEnabled)
        Me.Controls.Add(Me.txtDescription)
        Me.Controls.Add(Me.lblDescription)
        Me.Controls.Add(Me.txtName)
        Me.Controls.Add(Me.lblName)
        Me.Name = "WebApiActionPanel"
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

    Private WithEvents txtName As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents lblName As Label
    Private WithEvents lblDescription As Label
    Private WithEvents txtDescription As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents cbEnabled As CheckBox
    Friend WithEvents cbEnableRequestOutputParameter As CheckBox
    Friend WithEvents cbDisableSendingOfRequest As CheckBox
End Class
