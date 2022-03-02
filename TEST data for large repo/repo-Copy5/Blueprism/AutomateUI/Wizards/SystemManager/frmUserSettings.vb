Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Server.Domain.Models

''' Project  : Automate
''' Class    : frmUserSettings
''' 
''' <summary>
''' A wizard to manage user settings.
''' </summary>
Friend Class frmUserSettings
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
    Friend WithEvents mUserLogin As AutomateUI.UserSettingsNameAndPassword
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents TabPage3 As System.Windows.Forms.TabPage
    Friend WithEvents mUserExpiry As AutomateUI.ctlUserSettingsExpiry
    Friend WithEvents mUserAuth As AutomateUI.ctlAuth
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmUserSettings))
        Me.tabUser = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.mUserLogin = New AutomateUI.UserSettingsNameAndPassword()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.mUserExpiry = New AutomateUI.ctlUserSettingsExpiry()
        Me.TabPage3 = New System.Windows.Forms.TabPage()
        Me.mUserAuth = New AutomateUI.ctlAuth()
        Me.tabUser.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        Me.TabPage3.SuspendLayout()
        Me.SuspendLayout()
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
        Me.tabUser.Controls.Add(Me.TabPage2)
        Me.tabUser.Controls.Add(Me.TabPage3)
        Me.tabUser.Name = "tabUser"
        Me.tabUser.SelectedIndex = 0
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.mUserLogin)
        resources.ApplyResources(Me.TabPage1, "TabPage1")
        Me.TabPage1.Name = "TabPage1"
        '
        'mUserLogin
        '
        Me.mUserLogin.BackColor = System.Drawing.SystemColors.ControlLightLight
        resources.ApplyResources(Me.mUserLogin, "mUserLogin")
        Me.mUserLogin.Name = "mUserLogin"
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.mUserExpiry)
        resources.ApplyResources(Me.TabPage2, "TabPage2")
        Me.TabPage2.Name = "TabPage2"
        '
        'mUserExpiry
        '
        Me.mUserExpiry.BackColor = System.Drawing.SystemColors.ControlLightLight
        resources.ApplyResources(Me.mUserExpiry, "mUserExpiry")
        Me.mUserExpiry.Name = "mUserExpiry"
        Me.mUserExpiry.User = Nothing
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
        'frmUserSettings
        '
        resources.ApplyResources(Me, "$this")
        Me.CancelButton = Nothing
        Me.Controls.Add(Me.tabUser)
        Me.Name = "frmUserSettings"
        Me.Title = "Set the name and password for this user"
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.Controls.SetChildIndex(Me.btnBack, 0)
        Me.Controls.SetChildIndex(Me.btnNext, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.Controls.SetChildIndex(Me.tabUser, 0)
        Me.tabUser.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage2.ResumeLayout(False)
        Me.TabPage3.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region " Member Variables "

    ' The user whose settings are being modified by this control
    Private mUser As User

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new user settings form
    ''' </summary>
    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        SetMaxSteps(0)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The user whose settings are being modified in this form
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property User() As User
        Get
            Return mUser
        End Get
        Set(ByVal value As User)
            mUser = value
            mUserAuth.User = value
            mUserExpiry.User = value
            mUserLogin.User = value
            If mUser Is Nothing Then
                Title = My.Resources.frmUserSettings_EditingSettings
            ElseIf mUser.AuthType = AuthMode.AuthenticationServer Then
                Title = String.Format(My.Resources.frmUserSettings_SetTheRolesAndPermissionsForUser0, mUser.Name)
            ElseIf mUser.AuthType = AuthMode.AuthenticationServerServiceAccount Then
                Title = String.Format(My.Resources.frmUserSettings_SetTheRolesAndPermissionsForServiceAccount0, mUser.Name)
                Text = My.Resources.frmUserSettings_ServiceAccountSettings
            Else
                Title = String.Format(My.Resources.frmUserSettings_EditingSettingsForUser0, mUser.Name)
            End If

        End Set
    End Property

    ''' <summary>
    ''' Gets the associated permission level.
    ''' </summary>
    ''' <value>The permission level</value>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("System Manager")
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Moves the wizard along to the next step.
    ''' </summary>
    Protected Overrides Sub UpdatePage()

        If mUser.IsAuthenticationServerUserOrServiceAccount() Then
            tabUser.TabPages.Remove(TabPage1)
            tabUser.TabPages.Remove(TabPage2)
        End If

        ' Don't do anything in the pre-display call
        If GetStep() = 0 Then Return

        Try
            If Not mUserLogin.AllFieldsValid() Then Rollback() : Return

            Dim newPass = mUserLogin.NewPassword
            DialogResult = DialogResult.Ignore
            If (newPass IsNot Nothing OrElse
                mUser.HasChanged()) Then

                If User.Current.Id = mUser.Id AndAlso mUserLogin.HasUsernameChanged() AndAlso
                    UserMessage.YesNo(My.Resources.frmUserSettings_EditingYourOwnUserForcedLogout) = MsgBoxResult.No Then
                    Exit Sub
                End If

                If mUser.IsAuthenticationServerUserOrServiceAccount() Then
                    gSv.UpdateAuthenticationServerUser(mUser)
                Else
                    gSv.UpdateUser(mUser, newPass)
                End If

                DialogResult = DialogResult.OK
            End If
            Close()
        Catch ex As Exception
            UserMessage.Err(ex, ex.Message)
            User = gSv.GetUser(User.Id)
        End Try
    End Sub

    ''' <summary>
    ''' Handles the user tab being changed
    ''' </summary>
    Private Sub HandleTabChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles tabUser.SelectedIndexChanged

        If mUser.AuthType = AuthMode.AuthenticationServer Then
            Title = String.Format(My.Resources.frmUserSettings_SetTheRolesAndPermissionsForUser0, mUser.Name)
        ElseIf mUser.AuthType = AuthMode.AuthenticationServerServiceAccount Then
            Title = String.Format(My.Resources.frmUserSettings_SetTheRolesAndPermissionsForServiceAccount0, mUser.Name)
        Else
            Select Case tabUser.SelectedIndex
                Case 0 : Title = My.Resources.frmUserSettings_SetTheNameAndPasswordForThisUser
                Case 1 : Title = My.Resources.frmUserSettings_SetTheExpiryDatesThisUser
                Case 2 : Title = My.Resources.frmUserSettings_SetTheRolesAndPermissionsForThisUser
            End Select
        End If

    End Sub

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmUserSettings.htm"
    End Function

#End Region

End Class
