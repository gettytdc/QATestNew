Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore.WebApis.Authentication

Namespace Controls.Widgets.SystemManager.WebApi.Authentication

    ''' <summary>
    ''' Panel used to configure a Web API's custom authentication settings.
    ''' </summary>
    Friend Class CustomAuthenticationPanel : Implements IAuthenticationPanel

        Private mCustomAuthentication As CustomAuthentication
        Private mCredentials As IEnumerable(Of clsCredential)
        Private mDefaultParameterName As String

        ''' <inheritdoc/>
        Public Event ConfigurationChanged As AuthenticationChangedEventHandler _
            Implements IAuthenticationPanel.ConfigurationChanged

        ''' <summary>
        ''' Create a new instance of the <see cref="CustomAuthenticationPanel"/> 
        ''' </summary>
        ''' <param name="authentication">The underlying data that will modified by this panel</param>
        Public Sub New(authentication As CustomAuthentication)

            ' This call is required by the designer.
            InitializeComponent()

            mCustomAuthentication = authentication
            ctlCredentialPanel.Credential = authentication.Credential

        End Sub

        ''' <inheritdoc/>
        <Browsable(False),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public ReadOnly Property Configuration As IAuthentication _
            Implements IAuthenticationPanel.Configuration
            Get
                Return mCustomAuthentication
            End Get
        End Property

        ''' <inheritdoc/>
        <Browsable(False),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property Credentials As IEnumerable(Of clsCredential) _
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
            Dim dataChanged = Not mCustomAuthentication.Credential.Equals(ctlCredentialPanel.Credential)
            If dataChanged Then UpdateCustomAuthenticationData()
        End Sub

        ''' <summary>
        ''' Update the underlying custom authentication data with the current values
        ''' of this panel's controls and raise the ConfigurationChanged event
        ''' </summary>
        Private Sub UpdateCustomAuthenticationData()
            mCustomAuthentication =
                New CustomAuthentication(ctlCredentialPanel.Credential)
            OnConfigurationChanged(New AuthenticationChangedEventArgs(mCustomAuthentication))
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