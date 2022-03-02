<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class WebApiDetailPanel
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(WebApiDetailPanel))
        Me.splitConfigurationSettings = New AutomateControls.FlickerFreeSplitContainer()
        Me.btnExpandSettings = New AutomateControls.ImageButton()
        Me.frmConfigurationSettings = New AutomateUI.WebApiConfigurationSettingsForm()
        Me.lblName = New System.Windows.Forms.Label()
        Me.txtName = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblBaseUrl = New System.Windows.Forms.Label()
        Me.txtBaseUrl = New AutomateControls.Textboxes.StyledTextBox()
        Me.cbEnabled = New System.Windows.Forms.CheckBox()
        Me.ctlToolTip = New System.Windows.Forms.ToolTip(Me.components)
        CType(Me.splitConfigurationSettings,System.ComponentModel.ISupportInitialize).BeginInit
        Me.splitConfigurationSettings.Panel1.SuspendLayout
        Me.splitConfigurationSettings.Panel2.SuspendLayout
        Me.splitConfigurationSettings.SuspendLayout
        Me.SuspendLayout
        '
        'splitConfigurationSettings
        '
        resources.ApplyResources(Me.splitConfigurationSettings, "splitConfigurationSettings")
        Me.splitConfigurationSettings.Name = "splitConfigurationSettings"
        '
        'splitConfigurationSettings.Panel1
        '
        resources.ApplyResources(Me.splitConfigurationSettings.Panel1, "splitConfigurationSettings.Panel1")
        Me.splitConfigurationSettings.Panel1.Controls.Add(Me.btnExpandSettings)
        Me.ctlToolTip.SetToolTip(Me.splitConfigurationSettings.Panel1, resources.GetString("splitConfigurationSettings.Panel1.ToolTip"))
        '
        'splitConfigurationSettings.Panel2
        '
        resources.ApplyResources(Me.splitConfigurationSettings.Panel2, "splitConfigurationSettings.Panel2")
        Me.splitConfigurationSettings.Panel2.Controls.Add(Me.frmConfigurationSettings)
        Me.ctlToolTip.SetToolTip(Me.splitConfigurationSettings.Panel2, resources.GetString("splitConfigurationSettings.Panel2.ToolTip"))
        Me.ctlToolTip.SetToolTip(Me.splitConfigurationSettings, resources.GetString("splitConfigurationSettings.ToolTip"))
        '
        'btnExpandSettings
        '
        resources.ApplyResources(Me.btnExpandSettings, "btnExpandSettings")
        Me.btnExpandSettings.BackColor = System.Drawing.Color.Transparent
        Me.btnExpandSettings.Name = "btnExpandSettings"
        Me.ctlToolTip.SetToolTip(Me.btnExpandSettings, resources.GetString("btnExpandSettings.ToolTip"))
        Me.btnExpandSettings.UseVisualStyleBackColor = false
        '
        'frmConfigurationSettings
        '
        resources.ApplyResources(Me.frmConfigurationSettings, "frmConfigurationSettings")
        Me.frmConfigurationSettings.mConfigurationSettings = Nothing
        Me.frmConfigurationSettings.mRequiresAuthFields = false
        Me.frmConfigurationSettings.Name = "frmConfigurationSettings"
        Me.ctlToolTip.SetToolTip(Me.frmConfigurationSettings, resources.GetString("frmConfigurationSettings.ToolTip"))
        '
        'lblName
        '
        resources.ApplyResources(Me.lblName, "lblName")
        Me.lblName.Name = "lblName"
        Me.ctlToolTip.SetToolTip(Me.lblName, resources.GetString("lblName.ToolTip"))
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        Me.txtName.Name = "txtName"
        Me.ctlToolTip.SetToolTip(Me.txtName, resources.GetString("txtName.ToolTip"))
        '
        'lblBaseUrl
        '
        resources.ApplyResources(Me.lblBaseUrl, "lblBaseUrl")
        Me.lblBaseUrl.Name = "lblBaseUrl"
        Me.ctlToolTip.SetToolTip(Me.lblBaseUrl, resources.GetString("lblBaseUrl.ToolTip"))
        '
        'txtBaseUrl
        '
        resources.ApplyResources(Me.txtBaseUrl, "txtBaseUrl")
        Me.txtBaseUrl.Name = "txtBaseUrl"
        Me.ctlToolTip.SetToolTip(Me.txtBaseUrl, resources.GetString("txtBaseUrl.ToolTip"))
        '
        'cbEnabled
        '
        resources.ApplyResources(Me.cbEnabled, "cbEnabled")
        Me.cbEnabled.Name = "cbEnabled"
        Me.ctlToolTip.SetToolTip(Me.cbEnabled, resources.GetString("cbEnabled.ToolTip"))
        Me.cbEnabled.UseVisualStyleBackColor = true
        '
        'WebApiDetailPanel
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.splitConfigurationSettings)
        Me.Controls.Add(Me.cbEnabled)
        Me.Controls.Add(Me.txtBaseUrl)
        Me.Controls.Add(Me.lblBaseUrl)
        Me.Controls.Add(Me.txtName)
        Me.Controls.Add(Me.lblName)
        Me.Name = "WebApiDetailPanel"
        Me.ctlToolTip.SetToolTip(Me, resources.GetString("$this.ToolTip"))
        Me.splitConfigurationSettings.Panel1.ResumeLayout(false)
        Me.splitConfigurationSettings.Panel1.PerformLayout
        Me.splitConfigurationSettings.Panel2.ResumeLayout(false)
        CType(Me.splitConfigurationSettings,System.ComponentModel.ISupportInitialize).EndInit
        Me.splitConfigurationSettings.ResumeLayout(false)
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

    Private WithEvents lblName As Label
    Private WithEvents txtName As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents lblBaseUrl As Label
    Private WithEvents txtBaseUrl As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents cbEnabled As CheckBox
    Friend WithEvents ctlToolTip As ToolTip
    Friend WithEvents splitConfigurationSettings As AutomateControls.FlickerFreeSplitContainer
    Friend WithEvents btnExpandSettings As AutomateControls.ImageButton
    Friend WithEvents frmConfigurationSettings As WebApiConfigurationSettingsForm
End Class
