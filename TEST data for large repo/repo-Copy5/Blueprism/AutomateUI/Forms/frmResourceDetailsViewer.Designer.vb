<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmResourceDetailsViewer

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmResourceDetailsViewer))
        Me.lblFqdnTitle = New System.Windows.Forms.Label()
        Me.lblPortTitle = New System.Windows.Forms.Label()
        Me.lblVersionTitle = New System.Windows.Forms.Label()
        Me.lblFirstConnectedTitle = New System.Windows.Forms.Label()
        Me.lblLastUpdatedTitle = New System.Windows.Forms.Label()
        Me.lblApplicationServerTitle = New System.Windows.Forms.Label()
        Me.lblFqdn = New System.Windows.Forms.Label()
        Me.lblPort = New System.Windows.Forms.Label()
        Me.lblVersion = New System.Windows.Forms.Label()
        Me.lblFirstConnected = New System.Windows.Forms.Label()
        Me.lblLastUpdated = New System.Windows.Forms.Label()
        Me.lblApplicationServer = New System.Windows.Forms.Label()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.lblEnvironmentTypeTitle = New System.Windows.Forms.Label()
        Me.lblEnvironmentType = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'lblFqdnTitle
        '
        resources.ApplyResources(Me.lblFqdnTitle, "lblFqdnTitle")
        Me.lblFqdnTitle.Name = "lblFqdnTitle"
        '
        'lblPortTitle
        '
        resources.ApplyResources(Me.lblPortTitle, "lblPortTitle")
        Me.lblPortTitle.Name = "lblPortTitle"
        '
        'lblVersionTitle
        '
        resources.ApplyResources(Me.lblVersionTitle, "lblVersionTitle")
        Me.lblVersionTitle.Name = "lblVersionTitle"
        '
        'lblFirstConnectedTitle
        '
        resources.ApplyResources(Me.lblFirstConnectedTitle, "lblFirstConnectedTitle")
        Me.lblFirstConnectedTitle.Name = "lblFirstConnectedTitle"
        '
        'lblLastUpdatedTitle
        '
        resources.ApplyResources(Me.lblLastUpdatedTitle, "lblLastUpdatedTitle")
        Me.lblLastUpdatedTitle.Name = "lblLastUpdatedTitle"
        '
        'lblApplicationServerTitle
        '
        resources.ApplyResources(Me.lblApplicationServerTitle, "lblApplicationServerTitle")
        Me.lblApplicationServerTitle.Name = "lblApplicationServerTitle"
        '
        'lblFqdn
        '
        resources.ApplyResources(Me.lblFqdn, "lblFqdn")
        Me.lblFqdn.Name = "lblFqdn"
        '
        'lblPort
        '
        resources.ApplyResources(Me.lblPort, "lblPort")
        Me.lblPort.Name = "lblPort"
        '
        'lblVersion
        '
        resources.ApplyResources(Me.lblVersion, "lblVersion")
        Me.lblVersion.Name = "lblVersion"
        '
        'lblFirstConnected
        '
        resources.ApplyResources(Me.lblFirstConnected, "lblFirstConnected")
        Me.lblFirstConnected.Name = "lblFirstConnected"
        '
        'lblLastUpdated
        '
        resources.ApplyResources(Me.lblLastUpdated, "lblLastUpdated")
        Me.lblLastUpdated.Name = "lblLastUpdated"
        '
        'lblApplicationServer
        '
        resources.ApplyResources(Me.lblApplicationServer, "lblApplicationServer")
        Me.lblApplicationServer.Name = "lblApplicationServer"
        '
        'btnClose
        '
        resources.ApplyResources(Me.btnClose, "btnClose")
        Me.btnClose.Name = "btnClose"
        Me.btnClose.UseVisualStyleBackColor = True
        '
        'lblEnvironmentTypeTitle
        '
        resources.ApplyResources(Me.lblEnvironmentTypeTitle, "lblEnvironmentTypeTitle")
        Me.lblEnvironmentTypeTitle.Name = "lblEnvironmentTypeTitle"
        '
        'lblEnvironmentType
        '
        resources.ApplyResources(Me.lblEnvironmentType, "lblEnvironmentType")
        Me.lblEnvironmentType.Name = "lblEnvironmentType"
        '
        'FrmResourceDetailsViewer
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.lblEnvironmentType)
        Me.Controls.Add(Me.lblEnvironmentTypeTitle)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.lblApplicationServer)
        Me.Controls.Add(Me.lblLastUpdated)
        Me.Controls.Add(Me.lblFirstConnected)
        Me.Controls.Add(Me.lblVersion)
        Me.Controls.Add(Me.lblPort)
        Me.Controls.Add(Me.lblFqdn)
        Me.Controls.Add(Me.lblApplicationServerTitle)
        Me.Controls.Add(Me.lblLastUpdatedTitle)
        Me.Controls.Add(Me.lblFirstConnectedTitle)
        Me.Controls.Add(Me.lblVersionTitle)
        Me.Controls.Add(Me.lblPortTitle)
        Me.Controls.Add(Me.lblFqdnTitle)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.HelpButton = True
        Me.Name = "FrmResourceDetailsViewer"
        Me.ShowInTaskbar = False
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblFqdnTitle As Label
    Friend WithEvents lblPortTitle As Label
    Friend WithEvents lblVersionTitle As Label
    Friend WithEvents lblFirstConnectedTitle As Label
    Friend WithEvents lblLastUpdatedTitle As Label
    Friend WithEvents lblApplicationServerTitle As Label
    Friend WithEvents lblFqdn As Label
    Friend WithEvents lblPort As Label
    Friend WithEvents lblVersion As Label
    Friend WithEvents lblFirstConnected As Label
    Friend WithEvents lblLastUpdated As Label
    Friend WithEvents lblApplicationServer As Label
    Friend WithEvents btnClose As Button
    Friend WithEvents lblEnvironmentTypeTitle As Label
    Friend WithEvents lblEnvironmentType As Label
End Class
