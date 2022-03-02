<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class WebApiConfigurationSettingsForm
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(WebApiConfigurationSettingsForm))
        Me.Description = New System.Windows.Forms.Label()
        Me.intHttpRequestConnectionTimeout = New AutomateControls.StyledNumericUpDown()
        Me.intAuthServerRequestConnectionTimeout = New AutomateControls.StyledNumericUpDown()
        Me.lblHttpRequestTimeout = New System.Windows.Forms.Label()
        Me.lblAuthServerTimeout = New System.Windows.Forms.Label()
        Me.lblSecondsHttpTimeout = New System.Windows.Forms.Label()
        Me.lblSecondsAuthTimeout = New System.Windows.Forms.Label()
        Me.btnRestoreDefaults = New AutomateControls.Buttons.StandardStyledButton()
        CType(Me.intHttpRequestConnectionTimeout,System.ComponentModel.ISupportInitialize).BeginInit
        CType(Me.intAuthServerRequestConnectionTimeout,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'Description
        '
        resources.ApplyResources(Me.Description, "Description")
        Me.Description.Name = "Description"
        '
        'intHttpRequestConnectionTimeout
        '
        resources.ApplyResources(Me.intHttpRequestConnectionTimeout, "intHttpRequestConnectionTimeout")
        Me.intHttpRequestConnectionTimeout.Maximum = New Decimal(New Integer() {276447232, 23283, 0, 0})
        Me.intHttpRequestConnectionTimeout.Minimum = New Decimal(New Integer() {1316134912, 2328, 0, -2147483648})
        Me.intHttpRequestConnectionTimeout.Name = "intHttpRequestConnectionTimeout"
        Me.intHttpRequestConnectionTimeout.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'intAuthServerRequestConnectionTimeout
        '
        resources.ApplyResources(Me.intAuthServerRequestConnectionTimeout, "intAuthServerRequestConnectionTimeout")
        Me.intAuthServerRequestConnectionTimeout.Maximum = New Decimal(New Integer() {1215752192, 23, 0, 0})
        Me.intAuthServerRequestConnectionTimeout.Minimum = New Decimal(New Integer() {1215752192, 23, 0, -2147483648})
        Me.intAuthServerRequestConnectionTimeout.Name = "intAuthServerRequestConnectionTimeout"
        Me.intAuthServerRequestConnectionTimeout.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'lblHttpRequestTimeout
        '
        resources.ApplyResources(Me.lblHttpRequestTimeout, "lblHttpRequestTimeout")
        Me.lblHttpRequestTimeout.Name = "lblHttpRequestTimeout"
        '
        'lblAuthServerTimeout
        '
        resources.ApplyResources(Me.lblAuthServerTimeout, "lblAuthServerTimeout")
        Me.lblAuthServerTimeout.Name = "lblAuthServerTimeout"
        '
        'lblSecondsHttpTimeout
        '
        resources.ApplyResources(Me.lblSecondsHttpTimeout, "lblSecondsHttpTimeout")
        Me.lblSecondsHttpTimeout.Name = "lblSecondsHttpTimeout"
        '
        'lblSecondsAuthTimeout
        '
        resources.ApplyResources(Me.lblSecondsAuthTimeout, "lblSecondsAuthTimeout")
        Me.lblSecondsAuthTimeout.Name = "lblSecondsAuthTimeout"
        '
        'btnRestoreDefaults
        '
        resources.ApplyResources(Me.btnRestoreDefaults, "btnRestoreDefaults")
        Me.btnRestoreDefaults.CausesValidation = false
        Me.btnRestoreDefaults.Name = "btnRestoreDefaults"
        Me.btnRestoreDefaults.UseVisualStyleBackColor = true
        '
        'WebApiConfigurationSettingsForm
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.btnRestoreDefaults)
        Me.Controls.Add(Me.lblSecondsAuthTimeout)
        Me.Controls.Add(Me.lblSecondsHttpTimeout)
        Me.Controls.Add(Me.lblAuthServerTimeout)
        Me.Controls.Add(Me.lblHttpRequestTimeout)
        Me.Controls.Add(Me.intAuthServerRequestConnectionTimeout)
        Me.Controls.Add(Me.intHttpRequestConnectionTimeout)
        Me.Controls.Add(Me.Description)
        Me.Name = "WebApiConfigurationSettingsForm"
        resources.ApplyResources(Me, "$this")
        CType(Me.intHttpRequestConnectionTimeout,System.ComponentModel.ISupportInitialize).EndInit
        CType(Me.intAuthServerRequestConnectionTimeout,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

    Friend WithEvents Description As Label
    Friend WithEvents intHttpRequestConnectionTimeout As AutomateControls.StyledNumericUpDown
    Friend WithEvents intAuthServerRequestConnectionTimeout As AutomateControls.StyledNumericUpDown
    Friend WithEvents lblHttpRequestTimeout As Label
    Friend WithEvents lblAuthServerTimeout As Label
    Friend WithEvents lblSecondsHttpTimeout As Label
    Friend WithEvents lblSecondsAuthTimeout As Label
    Friend WithEvents btnRestoreDefaults As AutomateControls.Buttons.StandardStyledButton
End Class
