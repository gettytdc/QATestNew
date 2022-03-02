<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CtlSecurityUsers
    Inherits System.Windows.Forms.UserControl

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.splitUsers = New System.Windows.Forms.SplitContainer()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.gtUsers = New AutomateUI.GroupTreeControl()
        Me.panUser = New AutomateUI.ctlUserDetails()
        Me.lvUsers = New AutomateUI.ctlSelectUser()
        CType(Me.splitUsers, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitUsers.Panel1.SuspendLayout()
        Me.splitUsers.Panel2.SuspendLayout()
        Me.splitUsers.SuspendLayout()
        Me.SuspendLayout()
        '
        'splitUsers
        '
        Me.splitUsers.Dock = System.Windows.Forms.DockStyle.Fill
        Me.splitUsers.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.splitUsers.Location = New System.Drawing.Point(0, 0)
        Me.splitUsers.Name = "splitUsers"
        '
        'splitUsers.Panel1
        '
        Me.splitUsers.Panel1.Controls.Add(Me.gtUsers)
        Me.splitUsers.Panel1MinSize = 1
        '
        'splitUsers.Panel2
        '
        Me.splitUsers.Panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.splitUsers.Panel2.Controls.Add(Me.panUser)
        Me.splitUsers.Panel2.Controls.Add(Me.lvUsers)
        Me.splitUsers.Panel2MinSize = 50
        Me.splitUsers.Size = New System.Drawing.Size(618, 451)
        Me.splitUsers.SplitterDistance = 200
        Me.splitUsers.TabIndex = 1
        Me.splitUsers.TabStop = false

        '
        'gtUsers
        '
        Me.gtUsers.Dock = System.Windows.Forms.DockStyle.Fill
        Me.gtUsers.ItemCreateEnabled = True
        Me.gtUsers.ItemDeleteEnabled = True
        Me.gtUsers.Location = New System.Drawing.Point(0, 0)
        Me.gtUsers.Margin = New System.Windows.Forms.Padding(3, 3, 0, 3)
        Me.gtUsers.Name = "gtUsers"
        Me.gtUsers.Padding = New System.Windows.Forms.Padding(5)
        Me.gtUsers.Size = New System.Drawing.Size(200, 451)
        Me.gtUsers.TabIndex = 0
        '
        'panUser
        '
        Me.panUser.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.panUser.Dock = System.Windows.Forms.DockStyle.Fill
        Me.panUser.Location = New System.Drawing.Point(0, 0)
        Me.panUser.Name = "panUser"
        Me.panUser.Size = New System.Drawing.Size(414, 451)
        Me.panUser.TabIndex = 1
        Me.panUser.UserMember = Nothing
        '
        'lvUsers
        '
        Me.lvUsers.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lvUsers.DisplayedGroup = Nothing
        Me.lvUsers.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lvUsers.Location = New System.Drawing.Point(0, 0)
        Me.lvUsers.Name = "lvUsers"
        Me.lvUsers.Size = New System.Drawing.Size(414, 451)
        Me.lvUsers.TabIndex = 0
        '
        'CtlSecurityUsers
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.splitUsers)
        Me.Name = "ctlSecurityUsers"
        Me.Size = New System.Drawing.Size(618, 451)
        Me.splitUsers.Panel1.ResumeLayout(False)
        Me.splitUsers.Panel2.ResumeLayout(False)
        CType(Me.splitUsers, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitUsers.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents splitUsers As System.Windows.Forms.SplitContainer
    Private WithEvents lvUsers As AutomateUI.ctlSelectUser
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents gtUsers As AutomateUI.GroupTreeControl
    Friend WithEvents panUser As AutomateUI.ctlUserDetails

End Class
