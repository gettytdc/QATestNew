Imports AutomateControls
Imports BluePrism.ActiveDirectoryUserSearcher
Imports BluePrism.AutomateAppCore.Auth

Friend Class UserSettingsMappedActiveDirectoryUser : Inherits UserDetailsControl

    Private mUser As User

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Overrides Property User() As User
        Get
            Return mUser
        End Get
        Set(ByVal value As User)
            mUser = value
            If mUser IsNot Nothing Then
                txtUserName.Text = mUser.Name
                txtSid.Text = mUser.ExternalId
            End If
        End Set
    End Property

    Public Sub New()

        MyBase.New()
        'This call is required by the Windows Form Designer.
        InitializeComponent()

    End Sub

    Private Sub HandleSearchDirectoryClicked(ByVal sender As Object, ByVal e As EventArgs) Handles llSearchDirectory.LinkClicked

        Using activeDirectoryUserSearch = New frmActiveDirectoryUserSearch(User)
          
            activeDirectoryUserSearch.ShowInTaskbar = False
            Dim parent = TryCast(ParentForm, IEnvironmentColourManager)
            If parent IsNot Nothing Then
                activeDirectoryUserSearch.EnvironmentBackColor = parent.EnvironmentBackColor
                activeDirectoryUserSearch.EnvironmentForeColor = parent.EnvironmentForeColor
            End If

        If activeDirectoryUserSearch.ShowDialog(Me) = DialogResult.OK Then

                txtUserName.Text = activeDirectoryUserSearch.ctlSingleUserActiveDirectorySearch.SelectedUserUserPrincipalName
                User.Name = txtUserName.Text

                User.ExternalId = activeDirectoryUserSearch.ctlSingleUserActiveDirectorySearch.SelectedUserSid
                txtSid.Text = activeDirectoryUserSearch.ctlSingleUserActiveDirectorySearch.SelectedUserSid
        End If

        End Using

    End Sub

    Private Sub HandleUsernameEnter(ByVal sender As Object, ByVal e As EventArgs) Handles txtUserName.Enter
        txtUserName.SelectAll()
    End Sub

    Public Overrides Function AllFieldsValid() As Boolean
        Dim errorText As String = Nothing

        If Not User.IsValidUsername(User.Name, errorText, True) _
             Then Return UserMessage.Err(errorText)

        If String.IsNullOrEmpty(User.ExternalId) _
            Then Return UserMessage.Err(My.Resources.frmUserCreate_AnActiveDirectoryUserMustBeSelected)

        Return True

    End Function

End Class
