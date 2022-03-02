Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore.WebApis.Authentication

Namespace Controls.Widgets.SystemManager.WebApi.Authentication

    ''' <summary>
    ''' Panel used to configure a web api's OAuth2.0 with Jwt settings
    ''' </summary>
    Friend Class OAuth2WithJwtAuthenticationPanel : Implements IAuthenticationPanel

        ' The underlying OAuth2.0 with Jwt authentication data
        Private mJwtAuthentication As OAuth2JwtBearerTokenAuthentication

        ' The collection of credentials to use in credential drop down.
        Private mCredentials As IEnumerable(Of clsCredential)

        ' The default string to populate the credential parameter name textbox
        Private mDefaultParameterName As String

        ''' <summary>
        ''' Create a new instance of the <see cref="OAuth2WithJwtAuthenticationPanel"/> 
        ''' </summary>
        ''' <param name="authentication">The underlying data that will be modified 
        ''' by this panel</param>
        Public Sub New(authentication As OAuth2JwtBearerTokenAuthentication)

            ' This call is required by the designer.
            InitializeComponent()

            mJwtAuthentication = authentication
            txtAuthorizationUri.Text = authentication.AuthorizationServer.ToString
            txtAudience.Text = authentication.JwtConfiguration.Audience
            ctlCredentialPanel.Credential = authentication.JwtConfiguration.Credential
            txtScope.Text = authentication.JwtConfiguration.Scope
            txtSubject.Text = authentication.JwtConfiguration.Subject
            numExpiry.Value = authentication.JwtConfiguration.JwtExpiry
        End Sub

        ''' <inheritdoc/>
        Public ReadOnly Property Configuration As IAuthentication _
            Implements IAuthenticationPanel.Configuration
            Get
                Return mJwtAuthentication
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

            Dim dataChanged = Not mJwtAuthentication.JwtConfiguration.Credential.
                                  Equals(ctlCredentialPanel.Credential)
            If dataChanged Then UpdateJwtAuthenticationData()
        End Sub

        ''' <summary>
        ''' Handles the validating of the Audience textbox. 
        ''' Note the underlying data is not updated until the control loses focus 
        ''' after validation has completed successfully, as this control contains a
        ''' string which is parsed to a Uri.
        ''' </summary>
        Private Sub HandleAuthUriValidating(sender As Object, e As CancelEventArgs) _
            Handles txtAuthorizationUri.Validating

            Dim uriText = txtAuthorizationUri.Text.Trim()
            If uriText = "" Then
                UserMessage.Err(WebApi_Resources.ErrorAuthUrlCannotBeEmpty)
                e.Cancel = True
                Return
            End If

            Try
                Dim audUrl = New Uri(uriText)
            Catch ex As Exception
                UserMessage.Err(ex,
                                WebApi_Resources.ErrorInvalidUrl_Template,
                                uriText, ex.Message)
                e.Cancel = True
            End Try
        End Sub

        ''' <summary>
        ''' Handles the control validated events for Scope, Audience, Subject and Expiry, 
        ''' and updates the underlying data where required.
        ''' </summary>
        Private Sub HandleDataChanged(sender As Object, e As EventArgs) _
            Handles txtAuthorizationUri.Validated, txtAudience.Validated, txtScope.Validated,
                    numExpiry.Validated, txtSubject.Validated
            Dim dataChanged As Boolean
            With mJwtAuthentication.JwtConfiguration
                dataChanged =
                    Not mJwtAuthentication.AuthorizationServer.ToString.Equals(txtAuthorizationUri.Text) OrElse
                    Not .Scope.ToString.Equals(txtScope.Text) OrElse
                    Not .Audience.ToString.Equals(txtAudience.Text) OrElse
                    Not .Subject.ToString.Equals(txtSubject.Text) OrElse
                    Not .JwtExpiry.Equals(CType(numExpiry.Value, Integer))
            End With

            If dataChanged Then UpdateJwtAuthenticationData()
        End Sub

        ''' <summary>
        ''' Update the underlying OAuth2.0 With Jwt authentication data with the 
        ''' current values of this panel's controls and raise the ConfigurationChanged 
        ''' event
        ''' </summary>
        Private Sub UpdateJwtAuthenticationData()
            mJwtAuthentication = New OAuth2JwtBearerTokenAuthentication(
                New JwtConfiguration(
                    txtAudience.Text,
                    txtScope.Text,
                    txtSubject.Text,
                    CType(numExpiry.Value, Integer),
                    ctlCredentialPanel.Credential),
                New Uri(txtAuthorizationUri.Text)
                )

            OnConfigurationChanged(New AuthenticationChangedEventArgs(mJwtAuthentication))
        End Sub

    End Class

End Namespace
