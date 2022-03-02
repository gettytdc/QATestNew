Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore.WebApis.Authentication

Namespace Controls.Widgets.SystemManager.WebApi.Authentication
    ''' <summary>
    ''' Panel used to configure a web api's OAuth2.0 settings
    ''' </summary>
    Friend Class OAuth2AuthenticationPanel : Implements IAuthenticationPanel

        ' The underlying OAuth2.0 authentication data
        Private mOAuth2Authentication As OAuth2ClientCredentialsAuthentication

        ' The collection of credentials to use in credential drop down
        Private mCredentials As IEnumerable(Of clsCredential)

        ' The default string to populate the credential parameter name textbox
        Private mDefaultParameterName As String

        ''' <summary>
        ''' Create a new instance of the <see cref="OAuth2AuthenticationPanel"/> 
        ''' </summary>
        ''' <param name="authentication">The underlying data that will be modified 
        ''' by this panel</param>
        Public Sub New(authentication As OAuth2ClientCredentialsAuthentication)

            ' This call is required by the designer.
            InitializeComponent()

            mOAuth2Authentication = authentication
            txtAuthorisationURI.Text = authentication.AuthorizationServer.ToString
            ctlCredentialPanel.Credential = authentication.Credential
            txtScope.Text = authentication.Scope

        End Sub

        ''' <inheritdoc/>
        Public ReadOnly Property Configuration As IAuthentication _
            Implements IAuthenticationPanel.Configuration
            Get
                Return mOAuth2Authentication
            End Get
        End Property

        ''' <inheritdoc/>
        Public Property CredentialNames As IEnumerable(Of clsCredential) _
            Implements IAuthenticationPanel.Credentials
            Get
                Return mCredentials
            End Get
            Set(value As IEnumerable(Of clsCredential))
                mCredentials = value
                ctlCredentialPanel.Credentials = value
            End Set
        End Property

        ''' <inheritdoc/>
        Public Property DefaultParameterName As String _
            Implements IAuthenticationPanel.DefaultParameterName
            Get
                Return mDefaultParameterName
            End Get
            Set(value As String)
                mDefaultParameterName = value
                ctlCredentialPanel.DefaultParameterName = value
            End Set
        End Property

        ''' <inheritdoc/>
        Public Event ConfigurationChanged As AuthenticationChangedEventHandler _
            Implements IAuthenticationPanel.ConfigurationChanged

        ''' <inheritdoc/>
        Public Sub OnConfigurationChanged(e As AuthenticationChangedEventArgs) _
            Implements IAuthenticationPanel.OnConfigurationChanged
            RaiseEvent ConfigurationChanged(Me, e)
        End Sub

        ''' <inheritdoc/>
        Public Function ValidateParameterName(ByRef sErr As String) As Boolean _
            Implements IAuthenticationPanel.ValidateParameterName

            Dim parameterNameText = ctlCredentialPanel.txtParameterName.Text.Trim()
            If ctlCredentialPanel.chkExposeToProcess.Checked AndAlso parameterNameText = "" Then
                sErr = WebApi_Resources.ErrorEmptyAuthParameterName
                Return False
            End If
            Return True
        End Function

        ''' <summary>
        ''' Handles the data on the credential control being changed and updates the 
        ''' underlying data if changed
        ''' </summary>
        Private Sub HandleCredentialChanged(sender As Object, e As CredentialChangedEventArgs) _
            Handles ctlCredentialPanel.CredentialChanged

            Dim dataChanged =
                    Not mOAuth2Authentication.Credential.Equals(
                        ctlCredentialPanel.Credential)
            If dataChanged Then UpdateOauth2AuthenticationData()
        End Sub

        ''' <summary>
        ''' Handles the leaving of the OAuth2.0 scope textbox and updates the 
        ''' underlying data if changed
        ''' </summary>
        Private Sub HandleScopeChanged(sender As Object, e As EventArgs) _
            Handles txtScope.Leave

            Dim dataChanged = Not mOAuth2Authentication.Scope.Equals(txtScope.Text)
            If dataChanged Then UpdateOauth2AuthenticationData()
        End Sub

        ''' <summary>
        ''' Handles the validating of the OAuth2.0 Authorisation URI textbox. 
        ''' Note the underlying data is not updated until the control loses focus 
        ''' after validation has completed successfully.
        ''' </summary>
        Private Sub HandleAuthenticationUrlValidating(sender As Object, e As CancelEventArgs) _
            Handles txtAuthorisationURI.Validating

            Dim authUrlText = txtAuthorisationURI.Text.Trim()
            If authUrlText = "" Then
                UserMessage.Err(WebApi_Resources.ErrorAuthUrlCannotBeEmpty)
                e.Cancel = True
                Return
            End If

            Try
                Dim authUrl = New Uri(authUrlText)
            Catch ex As Exception
                UserMessage.Err(ex,
                                WebApi_Resources.ErrorInvalidAuthenticationUrl_Template, authUrlText, ex.Message)
                e.Cancel = True
            End Try
        End Sub

        ''' <summary>
        ''' Handles the OAuth2.0 authorisation URI textbox validated event, and 
        ''' updates the underlying data.
        ''' </summary>
        Private Sub HandleUriValidated(sender As Object, e As EventArgs) _
            Handles txtAuthorisationURI.Validated

            Dim dataChanged = Not mOAuth2Authentication.AuthorizationServer.ToString.
                                  Equals(txtAuthorisationURI.Text)
            If dataChanged Then UpdateOauth2AuthenticationData()
        End Sub

        ''' <summary>
        ''' Update the underlying OAuth2.0 authentication data with the current values
        ''' of this panel's controls and raise the ConfigurationChanged event
        ''' </summary>
        Private Sub UpdateOauth2AuthenticationData()
            mOAuth2Authentication = New OAuth2ClientCredentialsAuthentication(
                ctlCredentialPanel.Credential,
                txtScope.Text,
                New Uri(txtAuthorisationURI.Text))
            OnConfigurationChanged(New AuthenticationChangedEventArgs(mOAuth2Authentication))
        End Sub

    End Class

End Namespace
