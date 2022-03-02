<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlUserDetails
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlUserDetails))
        Me.ctxUserMaintenance = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.NewToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.EditMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DeleteToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.UnlockToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ADRefreshToolstripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ADViewGroupMembershipToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.ProcessAlertsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuRefresh = New System.Windows.Forms.ToolStripMenuItem()
        Me.lblFullName = New System.Windows.Forms.Label()
        Me.pnlAuthType = New System.Windows.Forms.Panel()
        Me.AuthType = New System.Windows.Forms.Label()
        Me.lblAuthType = New System.Windows.Forms.Label()
        Me.pnlAD = New System.Windows.Forms.Panel()
        Me.pbUserIcon = New System.Windows.Forms.PictureBox()
        Me.lblUserName = New System.Windows.Forms.Label()
        Me.pnlNativeAuth1 = New System.Windows.Forms.Panel()
        Me.pnlNativeAuth2 = New System.Windows.Forms.Panel()
        Me.lblValidTo = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.lblPasswordExpiry = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.lblValidFrom = New System.Windows.Forms.Label()
        Me.fullNameWorker = New System.ComponentModel.BackgroundWorker()
        Me.panDetails = New System.Windows.Forms.TableLayoutPanel()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.pnlExternalAuth = New System.Windows.Forms.Panel()
        Me.lblIdentityProviderType = New System.Windows.Forms.Label()
        Me.IdentityProviderType = New System.Windows.Forms.Label()
        Me.lblExternalId = New System.Windows.Forms.Label()
        Me.lblExternalIdentityProvider = New System.Windows.Forms.Label()
        Me.ExternalID = New System.Windows.Forms.Label()
        Me.ExternalIdentityProvider = New System.Windows.Forms.Label()
        Me.pnlLastSignedIn = New System.Windows.Forms.Panel()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.lblLastSignedIn = New System.Windows.Forms.Label()
        Me.ctxUserMaintenance.SuspendLayout()
        Me.pnlAuthType.SuspendLayout()
        Me.pnlAD.SuspendLayout()
        CType(Me.pbUserIcon, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlNativeAuth1.SuspendLayout()
        Me.pnlNativeAuth2.SuspendLayout()
        Me.panDetails.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.pnlExternalAuth.SuspendLayout()
        Me.pnlLastSignedIn.SuspendLayout()
        Me.SuspendLayout()
        '
        'ctxUserMaintenance
        '
        Me.ctxUserMaintenance.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.NewToolStripMenuItem, Me.EditMenuItem, Me.DeleteToolStripMenuItem, Me.UnlockToolStripMenuItem, Me.ADRefreshToolstripMenuItem, Me.ADViewGroupMembershipToolStripMenuItem, Me.ToolStripSeparator2, Me.ProcessAlertsToolStripMenuItem, Me.menuRefresh})
        Me.ctxUserMaintenance.Name = "ctxUserMaintenance"
        resources.ApplyResources(Me.ctxUserMaintenance, "ctxUserMaintenance")
        '
        'NewToolStripMenuItem
        '
        Me.NewToolStripMenuItem.Image = Global.AutomateUI.My.Resources.AuthImages.User_Blue_Add_16x16
        Me.NewToolStripMenuItem.Name = "NewToolStripMenuItem"
        resources.ApplyResources(Me.NewToolStripMenuItem, "NewToolStripMenuItem")
        '
        'EditMenuItem
        '
        resources.ApplyResources(Me.EditMenuItem, "EditMenuItem")
        Me.EditMenuItem.Image = Global.AutomateUI.My.Resources.AuthImages.User_Blue_Edit_16x16
        Me.EditMenuItem.Name = "EditMenuItem"
        '
        'DeleteToolStripMenuItem
        '
        resources.ApplyResources(Me.DeleteToolStripMenuItem, "DeleteToolStripMenuItem")
        Me.DeleteToolStripMenuItem.Image = Global.AutomateUI.My.Resources.ToolImages.Delete_Red_16x16
        Me.DeleteToolStripMenuItem.Name = "DeleteToolStripMenuItem"
        '
        'UnlockToolStripMenuItem
        '
        resources.ApplyResources(Me.UnlockToolStripMenuItem, "UnlockToolStripMenuItem")
        Me.UnlockToolStripMenuItem.Image = Global.AutomateUI.My.Resources.AuthImages.User_Blue_Unlock_16x16
        Me.UnlockToolStripMenuItem.Name = "UnlockToolStripMenuItem"
        '
        'ADRefreshToolstripMenuItem
        '
        Me.ADRefreshToolstripMenuItem.Name = "ADRefreshToolstripMenuItem"
        resources.ApplyResources(Me.ADRefreshToolstripMenuItem, "ADRefreshToolstripMenuItem")
        '
        'ADViewGroupMembershipToolStripMenuItem
        '
        Me.ADViewGroupMembershipToolStripMenuItem.Name = "ADViewGroupMembershipToolStripMenuItem"
        resources.ApplyResources(Me.ADViewGroupMembershipToolStripMenuItem, "ADViewGroupMembershipToolStripMenuItem")
        '
        'ToolStripSeparator2
        '
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        resources.ApplyResources(Me.ToolStripSeparator2, "ToolStripSeparator2")
        '
        'ProcessAlertsToolStripMenuItem
        '
        Me.ProcessAlertsToolStripMenuItem.Image = Global.AutomateUI.My.Resources.ToolImages.Alarm_16x16
        Me.ProcessAlertsToolStripMenuItem.Name = "ProcessAlertsToolStripMenuItem"
        resources.ApplyResources(Me.ProcessAlertsToolStripMenuItem, "ProcessAlertsToolStripMenuItem")
        '
        'menuRefresh
        '
        Me.menuRefresh.Image = Global.AutomateUI.My.Resources.ToolImages.Refresh_16x16
        Me.menuRefresh.Name = "menuRefresh"
        resources.ApplyResources(Me.menuRefresh, "menuRefresh")
        '
        'lblFullName
        '
        resources.ApplyResources(Me.lblFullName, "lblFullName")
        Me.lblFullName.Name = "lblFullName"
        '
        'pnlAuthType
        '
        resources.ApplyResources(Me.pnlAuthType, "pnlAuthType")
        Me.pnlAuthType.Controls.Add(Me.AuthType)
        Me.pnlAuthType.Controls.Add(Me.lblAuthType)
        Me.pnlAuthType.Name = "pnlAuthType"
        '
        'AuthType
        '
        resources.ApplyResources(Me.AuthType, "AuthType")
        Me.AuthType.Name = "AuthType"
        '
        'lblAuthType
        '
        resources.ApplyResources(Me.lblAuthType, "lblAuthType")
        Me.lblAuthType.Name = "lblAuthType"
        '
        'pnlAD
        '
        resources.ApplyResources(Me.pnlAD, "pnlAD")
        Me.pnlAD.Controls.Add(Me.lblFullName)
        Me.pnlAD.Name = "pnlAD"
        '
        'pbUserIcon
        '
        resources.ApplyResources(Me.pbUserIcon, "pbUserIcon")
        Me.pbUserIcon.Name = "pbUserIcon"
        Me.pbUserIcon.TabStop = False
        '
        'lblUserName
        '
        resources.ApplyResources(Me.lblUserName, "lblUserName")
        Me.lblUserName.Name = "lblUserName"
        '
        'pnlNativeAuth1
        '
        resources.ApplyResources(Me.pnlNativeAuth1, "pnlNativeAuth1")
        Me.pnlNativeAuth1.Controls.Add(Me.pnlNativeAuth2)
        Me.pnlNativeAuth1.Controls.Add(Me.Label3)
        Me.pnlNativeAuth1.Controls.Add(Me.lblValidFrom)
        Me.pnlNativeAuth1.Name = "pnlNativeAuth1"
        '
        'pnlNativeAuth2
        '
        Me.pnlNativeAuth2.Controls.Add(Me.lblValidTo)
        Me.pnlNativeAuth2.Controls.Add(Me.Label10)
        Me.pnlNativeAuth2.Controls.Add(Me.lblPasswordExpiry)
        Me.pnlNativeAuth2.Controls.Add(Me.Label2)
        resources.ApplyResources(Me.pnlNativeAuth2, "pnlNativeAuth2")
        Me.pnlNativeAuth2.Name = "pnlNativeAuth2"
        '
        'lblValidTo
        '
        resources.ApplyResources(Me.lblValidTo, "lblValidTo")
        Me.lblValidTo.Name = "lblValidTo"
        '
        'Label10
        '
        resources.ApplyResources(Me.Label10, "Label10")
        Me.Label10.Name = "Label10"
        '
        'lblPasswordExpiry
        '
        resources.ApplyResources(Me.lblPasswordExpiry, "lblPasswordExpiry")
        Me.lblPasswordExpiry.Name = "lblPasswordExpiry"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'lblValidFrom
        '
        resources.ApplyResources(Me.lblValidFrom, "lblValidFrom")
        Me.lblValidFrom.Name = "lblValidFrom"
        '
        'fullNameWorker
        '
        '
        'panDetails
        '
        resources.ApplyResources(Me.panDetails, "panDetails")
        Me.panDetails.Controls.Add(Me.pnlNativeAuth1, 0, 2)
        Me.panDetails.Controls.Add(Me.pnlAuthType, 0, 1)
        Me.panDetails.Controls.Add(Me.pnlAD, 0, 0)
        Me.panDetails.Controls.Add(Me.Panel1, 0, 3)
        Me.panDetails.Name = "panDetails"
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.pnlExternalAuth)
        Me.Panel1.Controls.Add(Me.pnlLastSignedIn)
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Name = "Panel1"
        '
        'pnlExternalAuth
        '
        Me.pnlExternalAuth.Controls.Add(Me.lblIdentityProviderType)
        Me.pnlExternalAuth.Controls.Add(Me.IdentityProviderType)
        Me.pnlExternalAuth.Controls.Add(Me.lblExternalId)
        Me.pnlExternalAuth.Controls.Add(Me.lblExternalIdentityProvider)
        Me.pnlExternalAuth.Controls.Add(Me.ExternalID)
        Me.pnlExternalAuth.Controls.Add(Me.ExternalIdentityProvider)
        resources.ApplyResources(Me.pnlExternalAuth, "pnlExternalAuth")
        Me.pnlExternalAuth.Name = "pnlExternalAuth"
        '
        'lblIdentityProviderType
        '
        resources.ApplyResources(Me.lblIdentityProviderType, "lblIdentityProviderType")
        Me.lblIdentityProviderType.Name = "lblIdentityProviderType"
        '
        'IdentityProviderType
        '
        resources.ApplyResources(Me.IdentityProviderType, "IdentityProviderType")
        Me.IdentityProviderType.Name = "IdentityProviderType"
        '
        'lblExternalId
        '
        resources.ApplyResources(Me.lblExternalId, "lblExternalId")
        Me.lblExternalId.Name = "lblExternalId"
        '
        'lblExternalIdentityProvider
        '
        resources.ApplyResources(Me.lblExternalIdentityProvider, "lblExternalIdentityProvider")
        Me.lblExternalIdentityProvider.Name = "lblExternalIdentityProvider"
        '
        'ExternalID
        '
        resources.ApplyResources(Me.ExternalID, "ExternalID")
        Me.ExternalID.Name = "ExternalID"
        '
        'ExternalIdentityProvider
        '
        resources.ApplyResources(Me.ExternalIdentityProvider, "ExternalIdentityProvider")
        Me.ExternalIdentityProvider.Name = "ExternalIdentityProvider"
        '
        'pnlLastSignedIn
        '
        Me.pnlLastSignedIn.Controls.Add(Me.Label7)
        Me.pnlLastSignedIn.Controls.Add(Me.lblLastSignedIn)
        resources.ApplyResources(Me.pnlLastSignedIn, "pnlLastSignedIn")
        Me.pnlLastSignedIn.Name = "pnlLastSignedIn"
        '
        'Label7
        '
        resources.ApplyResources(Me.Label7, "Label7")
        Me.Label7.Name = "Label7"
        '
        'lblLastSignedIn
        '
        resources.ApplyResources(Me.lblLastSignedIn, "lblLastSignedIn")
        Me.lblLastSignedIn.Name = "lblLastSignedIn"
        '
        'ctlUserDetails
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ContextMenuStrip = Me.ctxUserMaintenance
        Me.Controls.Add(Me.panDetails)
        Me.Controls.Add(Me.lblUserName)
        Me.Controls.Add(Me.pbUserIcon)
        Me.Name = "ctlUserDetails"
        resources.ApplyResources(Me, "$this")
        Me.ctxUserMaintenance.ResumeLayout(False)
        Me.pnlAuthType.ResumeLayout(False)
        Me.pnlAuthType.PerformLayout()
        Me.pnlAD.ResumeLayout(False)
        Me.pnlAD.PerformLayout()
        CType(Me.pbUserIcon, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlNativeAuth1.ResumeLayout(False)
        Me.pnlNativeAuth1.PerformLayout()
        Me.pnlNativeAuth2.ResumeLayout(False)
        Me.pnlNativeAuth2.PerformLayout()
        Me.panDetails.ResumeLayout(False)
        Me.panDetails.PerformLayout()
        Me.Panel1.ResumeLayout(False)
        Me.pnlExternalAuth.ResumeLayout(False)
        Me.pnlExternalAuth.PerformLayout()
        Me.pnlLastSignedIn.ResumeLayout(False)
        Me.pnlLastSignedIn.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Public WithEvents ctxUserMaintenance As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents NewToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents EditMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DeleteToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents UnlockToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ADRefreshToolstripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ADViewGroupMembershipToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ProcessAlertsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents lblFullName As System.Windows.Forms.Label
    Friend WithEvents pnlAuthType As System.Windows.Forms.Panel
    Friend WithEvents pnlAD As System.Windows.Forms.Panel
    Friend WithEvents pbUserIcon As System.Windows.Forms.PictureBox
    Friend WithEvents lblUserName As System.Windows.Forms.Label
    Friend WithEvents pnlNativeAuth1 As System.Windows.Forms.Panel
    Friend WithEvents fullNameWorker As System.ComponentModel.BackgroundWorker
    Private WithEvents panDetails As System.Windows.Forms.TableLayoutPanel
    Private WithEvents menuRefresh As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Label3 As Label
    Friend WithEvents lblValidFrom As Label
    Friend WithEvents AuthType As Label
    Friend WithEvents lblAuthType As Label
    Friend WithEvents pnlExternalAuth As Panel
    Friend WithEvents lblExternalId As Label
    Friend WithEvents ExternalID As Label
    Friend WithEvents pnlLastSignedIn As Panel
    Friend WithEvents Label7 As Label
    Friend WithEvents lblLastSignedIn As Label
    Friend WithEvents Panel1 As Panel
    Friend WithEvents lblExternalIdentityProvider As Label
    Friend WithEvents ExternalIdentityProvider As Label
    Friend WithEvents pnlNativeAuth2 As Panel
    Friend WithEvents lblValidTo As Label
    Friend WithEvents Label10 As Label
    Friend WithEvents lblPasswordExpiry As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents lblIdentityProviderType As Label
    Friend WithEvents IdentityProviderType As Label
End Class
