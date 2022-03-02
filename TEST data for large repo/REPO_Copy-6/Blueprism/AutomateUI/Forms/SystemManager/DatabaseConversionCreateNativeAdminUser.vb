Imports AutomateControls
Imports AutomateControls.Forms
Imports AutomateUI.My.Resources
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Server.Domain.Models

Public Class DatabaseConversionCreateNativeAdminUser : Inherits AutomateForm : Implements IChild, IEnvironmentColourManager

    Protected mParent As frmApplication
    Private Const mControlSpacing As Integer = 16
    Private Const mLabelInputSpacing As Integer = 3

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        InitializeUserSettingsControl()
    End Sub

    Private Sub InitializeUserSettingsControl()
        UserSettingsNameAndPassword.Label2.Text = DatabaseConversionCreateNativeAdminUser_Resources.Username
        UserSettingsNameAndPassword.Label2.Font = New Font("Segoe UI", 12.0!, FontStyle.Regular, GraphicsUnit.Pixel)
        UserSettingsNameAndPassword.Label1.Text = DatabaseConversionCreateNativeAdminUser_Resources.Password
        UserSettingsNameAndPassword.Label1.Font = New Font("Segoe UI", 12.0!, FontStyle.Regular, GraphicsUnit.Pixel)
        UserSettingsNameAndPassword.Label4.Text = DatabaseConversionCreateNativeAdminUser_Resources.ConfirmPassword
        UserSettingsNameAndPassword.Label4.Font = New Font("Segoe UI", 12.0!, FontStyle.Regular, GraphicsUnit.Pixel)
        UserSettingsNameAndPassword.lblWarningMessage.Font = New Font("Segoe UI", 12.0!, FontStyle.Regular, GraphicsUnit.Pixel)
        UserSettingsNameAndPassword.txtUsername.Height = UserSettingsNameAndPassword.txtPassword.Height

        UserSettingsNameAndPassword.txtPassword.Clear()
        UserSettingsNameAndPassword.txtConfirmPassword.Clear()

        UserSettingsNameAndPassword.User = New User(AuthMode.Native, Guid.NewGuid(), String.Empty, Nothing)

        UserSettingsNameAndPassword.txtUsername.Location = New Point(8, UserSettingsNameAndPassword.Label2.Location.Y + UserSettingsNameAndPassword.Label2.Height + mLabelInputSpacing)
        UserSettingsNameAndPassword.Label1.Location = New Point(8, 60)
        UserSettingsNameAndPassword.txtPassword.Location = New Point(8, UserSettingsNameAndPassword.Label1.Location.Y + UserSettingsNameAndPassword.Label1.Height + mLabelInputSpacing)
        UserSettingsNameAndPassword.Label4.Location = New Point(8, UserSettingsNameAndPassword.txtPassword.Location.Y + UserSettingsNameAndPassword.txtPassword.Height + mControlSpacing)
        UserSettingsNameAndPassword.txtConfirmPassword.Location = New Point(8, UserSettingsNameAndPassword.Label4.Location.Y + UserSettingsNameAndPassword.Label4.Height + mLabelInputSpacing)
        UserSettingsNameAndPassword.pnlWarning.Location = New Point(8, UserSettingsNameAndPassword.txtConfirmPassword.Location.Y + UserSettingsNameAndPassword.txtConfirmPassword.Height + mLabelInputSpacing)

    End Sub

    Private Sub btnNext_Click(sender As Object, e As EventArgs) Handles btnNext.Click
        If UserSettingsNameAndPassword.AllFieldsValid() Then

            Dim expiryDate As New Date(2099, 12, 31)
            UserSettingsNameAndPassword.User.Expiry = expiryDate

            Dim systemAdminRole = SystemRoleSet.Current.SingleOrDefault(Function(x) x.SystemAdmin())
            UserSettingsNameAndPassword.User.Roles.Add(systemAdminRole)

            DialogResult = DialogResult.OK
            Close()
        End If
    End Sub

    Public Overloads Function DisplayDialog() As NativeAdminUserModel
        If ShowDialog() = DialogResult.OK Then
            Return New NativeAdminUserModel(UserSettingsNameAndPassword.User, UserSettingsNameAndPassword.Password)
        End If

        Return Nothing
    End Function

    Friend Overridable Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return titleBar.BackColor
        End Get
        Set(value As Color)
            titleBar.BackColor = value
        End Set
    End Property

    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return titleBar.TitleColor
        End Get
        Set(value As Color)
            titleBar.TitleColor = value
        End Set
    End Property

End Class
