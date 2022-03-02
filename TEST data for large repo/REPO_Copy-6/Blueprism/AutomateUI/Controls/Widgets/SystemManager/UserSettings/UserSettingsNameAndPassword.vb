
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Common.Security

''' Project  : Automate
''' Class    : ctlUserSettingsNameAndPassword
''' 
''' <summary>
''' A control for displaying and managing use settings.
''' </summary>
Friend Class UserSettingsNameAndPassword
    Inherits UserDetailsControl

#Region " Windows Form Designer generated code "

    'UserControl overrides dispose to clean up the component list.
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
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtPassword As ctlMaskedSecurePasswordTextBox
    Friend WithEvents txtConfirmPassword As ctlMaskedSecurePasswordTextBox
    Friend WithEvents pnlWarning As Panel
    Friend WithEvents pbWarning As PictureBox
    Friend WithEvents lblWarningMessage As Label
    Friend WithEvents txtUsername As AutomateControls.Textboxes.StyledTextBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(UserSettingsNameAndPassword))
        Me.txtUsername = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtPassword = New AutomateUI.ctlMaskedSecurePasswordTextBox()
        Me.txtConfirmPassword = New AutomateUI.ctlMaskedSecurePasswordTextBox()
        Me.pnlWarning = New System.Windows.Forms.Panel()
        Me.pbWarning = New System.Windows.Forms.PictureBox()
        Me.lblWarningMessage = New System.Windows.Forms.Label()
        Me.pnlWarning.SuspendLayout
        CType(Me.pbWarning,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'txtUsername
        '
        resources.ApplyResources(Me.txtUsername, "txtUsername")
        Me.txtUsername.BorderColor = System.Drawing.Color.Empty
        Me.txtUsername.Name = "txtUsername"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'txtPassword
        '
        resources.ApplyResources(Me.txtPassword, "txtPassword")
        Me.txtPassword.BorderColor = System.Drawing.Color.Empty
        Me.txtPassword.Name = "txtPassword"
        Me.txtPassword.UsePlaceholder = true
        '
        'txtConfirmPassword
        '
        resources.ApplyResources(Me.txtConfirmPassword, "txtConfirmPassword")
        Me.txtConfirmPassword.BorderColor = System.Drawing.Color.Empty
        Me.txtConfirmPassword.Name = "txtConfirmPassword"
        Me.txtConfirmPassword.UsePlaceholder = true
        '
        'pnlWarning
        '
        resources.ApplyResources(Me.pnlWarning, "pnlWarning")
        Me.pnlWarning.Controls.Add(Me.pbWarning)
        Me.pnlWarning.Controls.Add(Me.lblWarningMessage)
        Me.pnlWarning.Name = "pnlWarning"
        '
        'pbWarning
        '
        Me.pbWarning.Image = Global.AutomateUI.My.Resources.ToolImages.Warning_16x16
        resources.ApplyResources(Me.pbWarning, "pbWarning")
        Me.pbWarning.Name = "pbWarning"
        Me.pbWarning.TabStop = false
        '
        'lblWarningMessage
        '
        resources.ApplyResources(Me.lblWarningMessage, "lblWarningMessage")
        Me.lblWarningMessage.Name = "lblWarningMessage"
        '
        'UserSettingsNameAndPassword
        '
        Me.Controls.Add(Me.pnlWarning)
        Me.Controls.Add(Me.txtConfirmPassword)
        Me.Controls.Add(Me.txtPassword)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtUsername)
        Me.Controls.Add(Me.Label2)
        Me.Name = "UserSettingsNameAndPassword"
        resources.ApplyResources(Me, "$this")
        Me.pnlWarning.ResumeLayout(false)
        Me.pnlWarning.PerformLayout
        CType(Me.pbWarning,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

#End Region

#Region " Member Variables "
    Private mUseBluePrismAccessibility As Boolean

    <DefaultValue(False), Category("Accessibility")>
    Friend Property UseBluePrismAccessibility() As Boolean
        Get
            UseBluePrismAccessibility = mUseBluePrismAccessibility
        End Get
        Set(ByVal Value As Boolean)
            mUseBluePrismAccessibility = Value
        End Set
    End Property

    ' The user whose name/password is being modified
    Private mUser As User

    Private mInitialUsername As String

    ' Flag indicating if the password has changed since being set in this control
    Private mPasswordChanged As Boolean = False

#End Region

#Region " Constructors "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Sets the user on this form.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Overrides Property User() As User
        Get
            Return mUser
        End Get
        Set(ByVal value As User)
            mUser = value
            If mUser Is Nothing Then
                Username = String.Empty
            Else
                Username = mUser.Name
                If mInitialUsername = String.Empty Then mInitialUsername = mUser.Name
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the username held in this control
    ''' </summary>
    Private Property Username() As String
        Get
            Return txtUsername.Text
        End Get
        Set(ByVal value As String)
            txtUsername.Text = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the password held in this control. Note that this also sets the
    ''' 'Confirm Password' field when set explicitly using this property.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property Password() As SafeString
        Get
            Return txtPassword.SecurePassword
        End Get
        Set(ByVal value As SafeString)
            txtPassword.SecurePassword = value
            txtConfirmPassword.SecurePassword = value
            mPasswordChanged = False
        End Set
    End Property

    ''' <summary>
    ''' Gets the new password set in this control, or null if a new password has not
    ''' been set.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property NewPassword() As SafeString
        Get
            If Not mPasswordChanged Then Return Nothing
            Return Password
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the 'confirm password' value in this control
    ''' </summary>
    Private Property ConfirmPassword() As SafeString
        Get
            Return txtConfirmPassword.SecurePassword
        End Get
        Set(ByVal value As SafeString)
            txtConfirmPassword.SecurePassword = value
        End Set
    End Property

    Private ReadOnly Property HasPassword As Boolean
        Get
            Return txtPassword.SecurePassword.Length > 0 OrElse Not mPasswordChanged
        End Get
    End Property

    Private ReadOnly Property HasUsername As Boolean
        Get
            Return txtUsername.Text <> ""
        End Get
    End Property

#End Region

#Region " Methods "

    Public Function HasUsernameChanged() As Boolean
        Return mInitialUsername <> User.Name
    End Function

    ''' <summary>
    ''' Handles the user name text item being changed
    ''' </summary>
    Private Sub HandleUsernameChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtUsername.Validated
        If mUser IsNot Nothing Then mUser.Name = txtUsername.Text
    End Sub

    ''' <summary>
    ''' Handles the password being changed.
    ''' </summary>
    Private Sub HandlePasswordChanged(ByVal sender As Object, ByVal e As EventArgs) _
        Handles txtPassword.Validated

        mPasswordChanged = True
    End Sub

    ''' <summary>
    ''' Handles the username field being entered by selecting the current value.
    ''' </summary>
    Private Sub HandleUsernameEnter(ByVal sender As Object, ByVal e As EventArgs) _
        Handles txtUsername.Enter
        txtUsername.SelectAll()
    End Sub

    ''' <summary>
    ''' Handles the password field being entered; this will blank the password fields
    ''' if no changes have been made in this control.
    ''' </summary>
    Private Sub HandlePasswordEnter(ByVal sender As Object, ByVal e As EventArgs) _
        Handles txtPassword.Enter
        If Not mPasswordChanged Then
            txtConfirmPassword.RemovePlaceholder()
        End If

    End Sub

    ''' <summary>
    ''' Checks that all fields have valid values.
    ''' </summary>
    ''' <returns>True if all valid</returns>
    Public Overrides Function AllFieldsValid() As Boolean

        If HasNothingChanged() Then Return True

        If IsInvalidUserName() Then Return False

        Return ValidatePassword()

    End Function

    Public Function HasNothingChanged() As Boolean
        If (HasUsername AndAlso HasPassword) Then
            If Not HasUsernameChanged() AndAlso Not mPasswordChanged Then Return True
        End If
        Return False
    End Function

    Public Function IsInvalidUserName() As Boolean
        Dim sErr As String = Nothing
        If Not User.IsValidUsername(Username, sErr) Then
            HandleError(sErr)
            Return True
        End If
        Return False
    End Function

    Private Function ValidatePassword() As Boolean

        Dim userForm As frmUserSettings = TryCast(FindForm(), frmUserSettings)
        If userForm Is Nothing Then
            Try
                Return gSv.IsValidPassword(Password, ConfirmPassword)
            Catch ex As Exception
                Return HandleError(ex.Message)
            End Try
        Else
            If mPasswordChanged Then
                If HasPassword Then
                    Try
                        Return gSv.IsValidPassword(Password, ConfirmPassword)
                    Catch ex As Exception
                        Return HandleError(ex.Message)
                    End Try
                Else
                    Dim msg As String =
                    My.Resources.UserSettingsNameAndPassword_YouEnteredABlankPasswordIfYouContinueThePasswordForThisUserWillRemainUnchangedW

                    If UserMessage.YesNo(msg) = MsgBoxResult.No Then
                        gSv.UpdateUser(mUser, Nothing)
                        userForm.Close()
                    End If
                    Return False
                End If
            Else
                Return True
            End If
        End If
    End Function

    Private Function HandleError(err As String) As Boolean
        If mUseBluePrismAccessibility Then
            lblWarningMessage.Text = err
            pnlWarning.Visible = True
        Else
            Return UserMessage.Err(err)
        End If
    End Function

#End Region

End Class
