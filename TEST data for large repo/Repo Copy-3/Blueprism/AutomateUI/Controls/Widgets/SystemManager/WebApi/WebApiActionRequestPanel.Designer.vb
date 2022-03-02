Namespace Controls.Widgets.SystemManager.WebApi

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class WebApiActionRequestPanel
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(WebApiActionRequestPanel))
        Me.ctlToolTip = New System.Windows.Forms.ToolTip(Me.components)
            Me.txtUrlPath = New AutomateControls.Textboxes.StyledTextBox()
            Me.pnlRequestConfigurationTable = New System.Windows.Forms.TableLayoutPanel()
        Me.pnlRequestConfiguration = New System.Windows.Forms.Panel()
        Me.cmbBodyType = New System.Windows.Forms.ComboBox()
        Me.lblMethod = New System.Windows.Forms.Label()
        Me.cmbMethod = New System.Windows.Forms.ComboBox()
        Me.lblActionUrl = New System.Windows.Forms.Label()
        Me.lblBody = New System.Windows.Forms.Label()
        Me.pnlBodyTypeConfiguration = New System.Windows.Forms.Panel()
        Me.pnlRequestConfigurationTable.SuspendLayout
        Me.pnlRequestConfiguration.SuspendLayout
        Me.SuspendLayout
        '
        'txtUrlPath
        '
        resources.ApplyResources(Me.txtUrlPath, "txtUrlPath")
        Me.txtUrlPath.Name = "txtUrlPath"
        Me.ctlToolTip.SetToolTip(Me.txtUrlPath, resources.GetString("txtUrlPath.ToolTip"))
        '
        'pnlRequestConfigurationTable
        '
        resources.ApplyResources(Me.pnlRequestConfigurationTable, "pnlRequestConfigurationTable")
        Me.pnlRequestConfigurationTable.Controls.Add(Me.pnlRequestConfiguration, 0, 0)
        Me.pnlRequestConfigurationTable.Controls.Add(Me.pnlBodyTypeConfiguration, 0, 1)
        Me.pnlRequestConfigurationTable.Name = "pnlRequestConfigurationTable"
        '
        'pnlRequestConfiguration
        '
        Me.pnlRequestConfiguration.Controls.Add(Me.cmbBodyType)
        Me.pnlRequestConfiguration.Controls.Add(Me.lblMethod)
        Me.pnlRequestConfiguration.Controls.Add(Me.cmbMethod)
        Me.pnlRequestConfiguration.Controls.Add(Me.lblActionUrl)
        Me.pnlRequestConfiguration.Controls.Add(Me.txtUrlPath)
        Me.pnlRequestConfiguration.Controls.Add(Me.lblBody)
        resources.ApplyResources(Me.pnlRequestConfiguration, "pnlRequestConfiguration")
        Me.pnlRequestConfiguration.Name = "pnlRequestConfiguration"
        '
        'cmbBodyType
        '
        resources.ApplyResources(Me.cmbBodyType, "cmbBodyType")
        Me.cmbBodyType.DisplayMember = "Text"
        Me.cmbBodyType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbBodyType.Name = "cmbBodyType"
        Me.cmbBodyType.ValueMember = "Tag"
        '
        'lblMethod
        '
        resources.ApplyResources(Me.lblMethod, "lblMethod")
        Me.lblMethod.Name = "lblMethod"
        '
        'cmbMethod
        '
        resources.ApplyResources(Me.cmbMethod, "cmbMethod")
        Me.cmbMethod.Name = "cmbMethod"
        '
        'lblActionUrl
        '
        resources.ApplyResources(Me.lblActionUrl, "lblActionUrl")
        Me.lblActionUrl.Name = "lblActionUrl"
        '
        'lblBody
        '
        resources.ApplyResources(Me.lblBody, "lblBody")
        Me.lblBody.Name = "lblBody"
        '
        'pnlBodyTypeConfiguration
        '
        resources.ApplyResources(Me.pnlBodyTypeConfiguration, "pnlBodyTypeConfiguration")
        Me.pnlBodyTypeConfiguration.Name = "pnlBodyTypeConfiguration"
        '
        'WebApiActionRequestPanel
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.pnlRequestConfigurationTable)
        Me.Name = "WebApiActionRequestPanel"
        resources.ApplyResources(Me, "$this")
        Me.pnlRequestConfigurationTable.ResumeLayout(false)
        Me.pnlRequestConfiguration.ResumeLayout(false)
        Me.pnlRequestConfiguration.PerformLayout
        Me.ResumeLayout(false)

End Sub
        Friend WithEvents ctlToolTip As ToolTip
        Friend WithEvents pnlRequestConfigurationTable As TableLayoutPanel
        Friend WithEvents pnlRequestConfiguration As Panel
        Private WithEvents cmbBodyType As ComboBox
        Private WithEvents lblMethod As Label
        Private WithEvents cmbMethod As ComboBox
        Private WithEvents lblActionUrl As Label
        Private WithEvents txtUrlPath As AutomateControls.Textboxes.StyledTextBox
        Private WithEvents lblBody As Label
        Friend WithEvents pnlBodyTypeConfiguration As Panel
    End Class

End Namespace