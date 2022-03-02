Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore.WebApis.Authentication

Namespace Controls.Widgets.SystemManager.WebApi.Authentication

    ''' <summary>
    ''' Panel used to configure a Web API's bearer token authentication settings.
    ''' </summary>
    Friend Class BearerTokenAuthenticationPanel : Implements IAuthenticationPanel

        ' The underlying basic authentication data
        Private mBearerAuthentication As BearerTokenAuthentication

        ' The collection of credentials to use in credential drop down
        Private mCredentials As IEnumerable(Of clsCredential)

        ' The default string to populate the credential parameter name textbox
        Private mDefaultParameterName As String

        ''' <inheritdoc/>
        Public Event ConfigurationChanged As AuthenticationChangedEventHandler _
            Implements IAuthenticationPanel.ConfigurationChanged

        ''' <summary>
        ''' Create a new instance of the <see cref="BearerTokenAuthenticationPanel"/> 
        ''' </summary>
        ''' <param name="authentication">The underlying data that will modified by this panel</param>
        Public Sub New(authentication As BearerTokenAuthentication)

            ' This call is required by the designer.
            InitializeComponent()

            mBearerAuthentication = authentication
            ctlCredentialPanel.Credential = authentication.Credential

        End Sub

        ''' <inheritdoc/>
        <Browsable(False),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public ReadOnly Property Configuration As IAuthentication _
            Implements IAuthenticationPanel.Configuration
            Get
                Return mBearerAuthentication
            End Get
        End Property

        ''' <inheritdoc/>
        <Browsable(False),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
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
        Public Property DefaultParameterName() As String Implements IAuthenticationPanel.DefaultParameterName
            Get
                Return mDefaultParameterName
            End Get
            Set(value As String)
                mDefaultParameterName = value
                ctlCredentialPanel.DefaultParameterName = value
            End Set
        End Property

        ''' <inheritdoc/>
        Public Sub OnConfigurationChanged(e As AuthenticationChangedEventArgs) _
            Implements IAuthenticationPanel.OnConfigurationChanged
            RaiseEvent ConfigurationChanged(Me, e)
        End Sub

        ''' <summary>
        ''' Handles the data on the credential control being changed and updates the 
        ''' underlying data
        ''' </summary>
        Private Sub HandleCredentialChanged(sender As Object, e As CredentialChangedEventArgs) _
            Handles ctlCredentialPanel.CredentialChanged
            Dim dataChanged = Not mBearerAuthentication.Credential.Equals(ctlCredentialPanel.Credential)
            If dataChanged Then UpdateBearerTokenAuthenticationData()
        End Sub

        ''' <summary>
        ''' Update the underlying bearer token authentication data with the current values
        ''' of this panel's controls and raise the ConfigurationChanged event
        ''' </summary>
        Private Sub UpdateBearerTokenAuthenticationData()
            mBearerAuthentication =
                New BearerTokenAuthentication(ctlCredentialPanel.Credential)
            OnConfigurationChanged(New AuthenticationChangedEventArgs(mBearerAuthentication))
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

    End Class

End Namespace