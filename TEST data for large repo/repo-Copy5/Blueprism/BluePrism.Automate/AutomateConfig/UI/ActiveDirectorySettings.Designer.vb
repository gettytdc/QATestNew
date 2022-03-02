<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ActiveDirectorySettings
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ActiveDirectorySettings))
        Me.Label2 = New System.Windows.Forms.Label()
        Me.lblDomain = New System.Windows.Forms.Label()
        Me.lblDomainInfo = New System.Windows.Forms.Label()
        Me.lblVerified = New System.Windows.Forms.Label()
        Me.pbTick = New System.Windows.Forms.PictureBox()
        Me.lblUnverified = New System.Windows.Forms.Label()
        Me.ToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.btnAdminGroup = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.txtAdminGroupName = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtAdminGroupPath = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnVerifyDomain = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.txtActiveDirectoryDomain = New AutomateControls.Textboxes.StyledTextBox()
        CType(Me.pbTick, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'lblDomain
        '
        resources.ApplyResources(Me.lblDomain, "lblDomain")
        Me.lblDomain.Name = "lblDomain"
        '
        'lblDomainInfo
        '
        resources.ApplyResources(Me.lblDomainInfo, "lblDomainInfo")
        Me.lblDomainInfo.Name = "lblDomainInfo"
        '
        'lblVerified
        '
        resources.ApplyResources(Me.lblVerified, "lblVerified")
        Me.lblVerified.BackColor = System.Drawing.Color.Transparent
        Me.lblVerified.ForeColor = System.Drawing.Color.Green
        Me.lblVerified.Name = "lblVerified"
        '
        'pbTick
        '
        resources.ApplyResources(Me.pbTick, "pbTick")
        Me.pbTick.Name = "pbTick"
        Me.pbTick.TabStop = False
        '
        'lblUnverified
        '
        resources.ApplyResources(Me.lblUnverified, "lblUnverified")
        Me.lblUnverified.ForeColor = System.Drawing.SystemColors.ControlDark
        Me.lblUnverified.Name = "lblUnverified"
        '
        'ToolTip
        '
        Me.ToolTip.AutomaticDelay = 150
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.btnAdminGroup, 1, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.txtAdminGroupName, 0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'btnAdminGroup
        '
        resources.ApplyResources(Me.btnAdminGroup, "btnAdminGroup")
        Me.btnAdminGroup.Name = "btnAdminGroup"
        Me.btnAdminGroup.UseVisualStyleBackColor = False
        '
        'txtAdminGroupName
        '
        resources.ApplyResources(Me.txtAdminGroupName, "txtAdminGroupName")
        Me.txtAdminGroupName.BorderColor = System.Drawing.Color.Empty
        Me.txtAdminGroupName.Name = "txtAdminGroupName"
        Me.txtAdminGroupName.ReadOnly = True
        '
        'txtAdminGroupPath
        '
        resources.ApplyResources(Me.txtAdminGroupPath, "txtAdminGroupPath")
        Me.txtAdminGroupPath.BorderColor = System.Drawing.Color.Empty
        Me.txtAdminGroupPath.Name = "txtAdminGroupPath"
        Me.txtAdminGroupPath.ReadOnly = True
        '
        'btnVerifyDomain
        '
        resources.ApplyResources(Me.btnVerifyDomain, "btnVerifyDomain")
        Me.btnVerifyDomain.Name = "btnVerifyDomain"
        Me.btnVerifyDomain.UseVisualStyleBackColor = False
        '
        'txtActiveDirectoryDomain
        '
        resources.ApplyResources(Me.txtActiveDirectoryDomain, "txtActiveDirectoryDomain")
        Me.txtActiveDirectoryDomain.BorderColor = System.Drawing.Color.Empty
        Me.txtActiveDirectoryDomain.Name = "txtActiveDirectoryDomain"
        '
        'ActiveDirectorySettings
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Controls.Add(Me.txtAdminGroupPath)
        Me.Controls.Add(Me.btnVerifyDomain)
        Me.Controls.Add(Me.lblDomainInfo)
        Me.Controls.Add(Me.txtActiveDirectoryDomain)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.lblDomain)
        Me.Controls.Add(Me.lblUnverified)
        Me.Controls.Add(Me.lblVerified)
        Me.Controls.Add(Me.pbTick)
        Me.Name = "ActiveDirectorySettings"
        resources.ApplyResources(Me, "$this")
        CType(Me.pbTick, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents lblDomain As System.Windows.Forms.Label
    Public WithEvents txtAdminGroupName As AutomateControls.Textboxes.StyledTextBox
    Public WithEvents txtActiveDirectoryDomain As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents btnAdminGroup As AutomateControls.Buttons.StandardStyledButton
    Public WithEvents txtAdminGroupPath As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblDomainInfo As System.Windows.Forms.Label
    Friend WithEvents btnVerifyDomain As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lblVerified As System.Windows.Forms.Label
    Friend WithEvents pbTick As System.Windows.Forms.PictureBox
    Friend WithEvents lblUnverified As System.Windows.Forms.Label
    Friend WithEvents ToolTip As System.Windows.Forms.ToolTip
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
End Class
