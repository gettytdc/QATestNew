Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth


Friend Class frmExternalUserSettings
    Inherits frmWizard
    Implements IPermission

#Region " Windows Form Designer generated code "

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents tabUser As System.Windows.Forms.TabControl
    Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents mExternalUserDetails As UserSettingsNameAndExternalId
    Friend WithEvents mUserAuth As ctlAuth
    Friend WithEvents TabPage3 As System.Windows.Forms.TabPage
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmExternalUserSettings))
        Me.tabUser = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.mExternalUserDetails = New AutomateUI.UserSettingsNameAndExternalId()
        Me.TabPage3 = New System.Windows.Forms.TabPage()
        Me.mUserAuth = New AutomateUI.ctlAuth()
        Me.tabUser.SuspendLayout
        Me.TabPage1.SuspendLayout
        Me.TabPage3.SuspendLayout
        Me.SuspendLayout
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        '
        'btnNext
        '
        resources.ApplyResources(Me.btnNext, "btnNext")
        '
        'btnBack
        '
        resources.ApplyResources(Me.btnBack, "btnBack")
        '
        'objBluebar
        '
        resources.ApplyResources(Me.objBluebar, "objBluebar")
        '
        'tabUser
        '
        resources.ApplyResources(Me.tabUser, "tabUser")
        Me.tabUser.Controls.Add(Me.TabPage1)
        Me.tabUser.Controls.Add(Me.TabPage3)
        Me.tabUser.Name = "tabUser"
        Me.tabUser.SelectedIndex = 0
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.mExternalUserDetails)
        resources.ApplyResources(Me.TabPage1, "TabPage1")
        Me.TabPage1.Name = "TabPage1"
        '
        'mExternalUserDetails
        '
        Me.mExternalUserDetails.BackColor = System.Drawing.SystemColors.ControlLightLight
        resources.ApplyResources(Me.mExternalUserDetails, "mExternalUserDetails")
        Me.mExternalUserDetails.Name = "mExternalUserDetails"
        '
        'TabPage3
        '
        Me.TabPage3.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.TabPage3.Controls.Add(Me.mUserAuth)
        resources.ApplyResources(Me.TabPage3, "TabPage3")
        Me.TabPage3.Name = "TabPage3"
        '
        'mUserAuth
        '
        resources.ApplyResources(Me.mUserAuth, "mUserAuth")
        Me.mUserAuth.EditMode = AutomateUI.AuthEditMode.ManageUser
        Me.mUserAuth.Name = "mUserAuth"
        '
        'frmExternalUserSettings
        '
        Me.CancelButton = Nothing
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.tabUser)
        Me.Name = "frmExternalUserSettings"
        Me.Title = "Set the name and external identity for this user"
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.Controls.SetChildIndex(Me.btnBack, 0)
        Me.Controls.SetChildIndex(Me.btnNext, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.Controls.SetChildIndex(Me.tabUser, 0)
        Me.tabUser.ResumeLayout(false)
        Me.TabPage1.ResumeLayout(false)
        Me.TabPage3.ResumeLayout(false)
        Me.ResumeLayout(false)

End Sub

#End Region

#Region " Member Variables "

    ' The user whose settings are being modified by this control
    Private mUser As User

#End Region

#Region " Constructors "

   Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        SetMaxSteps(0)
    End Sub

#End Region

#Region " Properties "

    
    <Browsable(False), _
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Property User() As User
        Get
            Return mUser
        End Get
        Set(ByVal value As User)
            mUser = value
            mUserAuth.User = value
            mExternalUserDetails.User = value
            If mUser Is Nothing _
             Then Title = My.Resources.frmUserSettings_EditingSettings _
             Else Title = String.Format(My.Resources.frmExternalUserSettings_EditingSettingsForExternalUser0, mUser.Name)
        End Set
    End Property

   Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("System Manager")
        End Get
    End Property

#End Region

#Region " Methods "

    Protected Overrides Sub UpdatePage()

        ' Don't do anything in the pre-display call
        If GetStep() = 0 Then Return
        Try
            If Not mExternalUserDetails.AllFieldsValid() Then Rollback() : Return
            Dim newExternalId = mExternalUserDetails.ExternalUserId
            If newExternalId IsNot Nothing OrElse mUser.HasChanged() Then
                gSv.UpdateExternalUser(mUser)
                DialogResult = DialogResult.OK
            Else
                DialogResult = DialogResult.Ignore
            End If
            Close()
        Catch ex As Exception
            UserMessage.Err(ex, ex.Message)
            User = gSv.GetUser(User.Id)
        End Try
    End Sub

    Private Sub HandleTabChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles tabUser.SelectedIndexChanged
        Select Case tabUser.SelectedIndex
            Case 0 : Title =  If (mUser Is Nothing, 
                                  My.Resources.frmUserSettings_EditingSettings, 
                                  String.Format(
                                      My.Resources.frmExternalUserSettings_EditingSettingsForExternalUser0, 
                                      mUser.Name))
            Case 1 : Title = My.Resources.frmUserSettings_SetTheRolesAndPermissionsForThisUser
        End Select
    End Sub

   
    Public Overrides Function GetHelpFile() As String
        Return "frmUserSettings.htm"
    End Function

#End Region

End Class
