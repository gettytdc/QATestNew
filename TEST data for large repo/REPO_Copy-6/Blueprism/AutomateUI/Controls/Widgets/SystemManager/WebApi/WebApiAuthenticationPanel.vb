Imports AutomateUI.Controls.Widgets.SystemManager.WebApi
Imports AutomateUI.Controls.Widgets.SystemManager.WebApi.Authentication
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore.WebApis.Authentication

''' <summary>
''' Panel used to configure the authentication used in a Web API
''' </summary>
Friend Class WebApiAuthenticationPanel : Implements IGuidanceProvider

    Public Event AuthenticationTypeChanged As AuthenticationChangedEventHandler

    ' The underlying authentication data being edited
    Private mAuthentication As AuthenticationWrapper

    ' The currently selected authentication type
    Private mAuthenticationType As AuthenticationType?

    ' The configuration panel used to edit the authentication details
    Private WithEvents mConfigurationPanel As IAuthenticationPanel

    ' Collection of the system's credentials, used to populate credential's
    ' combo boxes on the configuration panel
    Private mCredentials As IEnumerable(Of clsCredential)

    ''' <summary>
    ''' Creates a new instance of the WebApiAuthenticationPanel panel.
    ''' </summary>
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    ''' <summary>
    ''' Gets the correct default text for the credential parameter name used to load
    ''' the Parameter Name textbox on the Authentication Credential Panel, according 
    ''' to the type of authentication being used.
    ''' </summary>
    Public Property DefaultParameterName() As String

    ''' <summary>
    ''' The configuration panel used to configure the underlying authentication details
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Private Property ConfigurationPanel As IAuthenticationPanel
        Get
            Return mConfigurationPanel
        End Get
        Set(value As IAuthenticationPanel)
            Try
                SuspendLayout()

                mConfigurationPanel = value
                mConfigurationPanel.Credentials = Credentials
                mConfigurationPanel.DefaultParameterName = DefaultParameterName
                Dim ctl = TryCast(mConfigurationPanel, Control)
                If ctl Is Nothing Then Return
                With pnlConfiguration.Controls
                    .Clear()
                    If value IsNot Nothing Then
                        ctl.Dock = DockStyle.Fill
                        .Add(ctl)
                    End If
                End With
            Finally
                ResumeLayout()
            End Try
        End Set
    End Property


    ''' <summary>
    ''' Gets or sets the underlying authentication data
    ''' </summary>
    <Browsable(False),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Authentication As AuthenticationWrapper
        Get
            Return mAuthentication
        End Get
        Set(value As AuthenticationWrapper)
            mAuthentication = value
            AuthenticationType = value.Authentication.Type
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the currently selected Authentication Type
    ''' </summary>
    ''' <returns></returns>
    Public Property AuthenticationType As AuthenticationType
        Get
            Return mAuthenticationType.Value
        End Get
        Set(value As AuthenticationType)
            Try
                SuspendLayout()

                RemoveHandler cmbAuthType.SelectedIndexChanged, AddressOf HandleAuthenticationTypeChanged

                Dim hasTypeChanged = mAuthenticationType.HasValue AndAlso mAuthenticationType.Value <> value

                mAuthenticationType = value

                ' Populate the drop down if it hasn't already been done
                If cmbAuthType.DataSource Is Nothing Then
                    cmbAuthType.Items.Clear()
                    Dim authTypes = [Enum].GetValues(GetType(AuthenticationType)).Cast(Of AuthenticationType)
                    Dim dict = authTypes.ToDictionary(Function(a) GetAuthenticationTypeDescription(a), Function(a) a)

                    cmbAuthType.DataSource = New BindingSource(dict, Nothing)
                End If

                ' If the authentication type was set in code, ensure the combo box
                ' value is also updated
                Dim currentAuthType = CType(cmbAuthType.SelectedItem, KeyValuePair(Of String, AuthenticationType)).Value
                If currentAuthType <> value Then
                    Dim itemToSelect = cmbAuthType.Items.Cast(Of
                                KeyValuePair(Of String, AuthenticationType)).FirstOrDefault(Function(k) k.Value = value)
                    cmbAuthType.SelectedItem = itemToSelect
                End If

                SetDefaultParameterName()

                ' If the type has has changed i.e. been modified by a user, then 
                ' create a blank configuration for the type that is now selected
                If hasTypeChanged Then mAuthentication.Authentication = CreateNewConfiguration(mAuthenticationType.Value)

                LoadAuthenticationPanel()

                AddHandler cmbAuthType.SelectedIndexChanged, AddressOf HandleAuthenticationTypeChanged

            Finally
                ResumeLayout()
            End Try
        End Set
    End Property

    ''' <summary>
    ''' Gets a tuple of the description and value of a given authentication type
    ''' </summary>
    ''' <param name="type">The authenticaion type used</param>
    ''' <returns>A tuple of the description and value</returns>
    Private Shared Function GetAuthenticationTypeDescription(type As AuthenticationType) As String
        Return AuthenticationTypeExtensions.Getdescription(type)
    End Function

    ''' <summary>
    ''' Lazily loads a collection of all the credentials in the system.
    ''' </summary>
    ''' <returns>A collection of the system's credentials</returns>
    Private ReadOnly Property Credentials() As IEnumerable(Of clsCredential)
        Get
            If mCredentials Is Nothing Then

                Dim existingCredentials As ICollection(Of clsCredential) = Nothing

                Try
                    existingCredentials = gSv.GetAllCredentialsInfo()
                Catch ex As Exception
                    Dim errorMessage = String.Format(WebApi_Resources.
                                                     ErrorFailedToPopulateCredentialsList_Template,
                                                     ex.Message)
                    UserMessage.Show(errorMessage)
                End Try

                mCredentials = existingCredentials

            End If

            Return mCredentials
        End Get
    End Property

    ''' <summary>
    ''' Gets the guidance text for this panel.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property GuidanceText As String _
     Implements IGuidanceProvider.GuidanceText
        Get
            Select Case mAuthenticationType
                Case AuthenticationType.None
                    Return WebApi_Resources.GuidanceEmptyAuthenticationPanel
                Case AuthenticationType.Basic
                    Return WebApi_Resources.GuidanceBasicAuthenticationPanel
                Case AuthenticationType.BearerToken
                    Return WebApi_Resources.GuidanceBearerTokenAuthenticationPanel
                Case AuthenticationType.OAuth2ClientCredentials
                    Return WebApi_Resources.GuidanceOAuth2AuthenticationPanel
                Case AuthenticationType.OAuth2JwtBearerToken
                    Return WebApi_Resources.GuidanceOAuth2WithJwtAuthenticationPanel
                Case AuthenticationType.Custom
                    Return WebApi_Resources.GuidanceCustomAuthenticationPanel
                Case Else
                    Throw New NotImplementedException(
                        $"Guidance text not set up for {mAuthenticationType.ToString()}")
            End Select

        End Get
    End Property

    ''' <summary>
    ''' Create a brand new default instance of the <see cref="IAuthentication"/>
    ''' linked to the specified authentication type
    ''' </summary>
    ''' <param name="authenticationType">The type used to determine which
    ''' implementation of <see cref="IAuthentication"/></param> to create
    ''' <returns>A brand new instance of an implementation  of
    ''' <see cref="IAuthentication"/></returns>
    Private Function CreateNewConfiguration(authenticationType As AuthenticationType) As IAuthentication

        Select Case authenticationType
            Case AuthenticationType.Basic
                Return New BasicAuthentication(
                                New AuthenticationCredential(String.Empty, True, DefaultParameterName),
                                False)
            Case AuthenticationType.BearerToken
                Return New BearerTokenAuthentication(
                                New AuthenticationCredential(String.Empty, True, DefaultParameterName))
            Case AuthenticationType.OAuth2ClientCredentials
                Return New OAuth2ClientCredentialsAuthentication(
                    New AuthenticationCredential(String.Empty, True, DefaultParameterName),
                    String.Empty,
                    New Uri("http://example.com/"))
            Case AuthenticationType.OAuth2JwtBearerToken
                Return New OAuth2JwtBearerTokenAuthentication(
                                New JwtConfiguration(String.Empty,
                                                     String.Empty,
                                                     String.Empty,
                                                     3600,
                                                     New AuthenticationCredential(String.Empty, True, DefaultParameterName)),
                                New Uri("http://example.com/"))
            Case AuthenticationType.None
                Return New EmptyAuthentication()
            Case AuthenticationType.Custom
                Return New CustomAuthentication(New AuthenticationCredential(String.Empty, True, DefaultParameterName))
            Case Else
                Throw New NotImplementedException()
        End Select
    End Function

    ''' <summary>
    ''' Sets the default parameter name string which is made available to the 
    ''' authentication panel and its child credential control, according to the type 
    ''' of authentication being used 
    ''' </summary>
    Private Sub SetDefaultParameterName()
        Select Case AuthenticationType
            Case AuthenticationType.Basic
                DefaultParameterName = "Basic Authentication Credential Name"
            Case AuthenticationType.BearerToken
                DefaultParameterName = "Bearer Token Authentication Credential Name"
            Case AuthenticationType.OAuth2ClientCredentials
                DefaultParameterName = "OAuth 2 (Client Credentials) Authentication Credential Name"
            Case AuthenticationType.OAuth2JwtBearerToken
                DefaultParameterName = "OAuth 2 (JWT Bearer Token) Authentication Credential Name"
            Case AuthenticationType.Custom
                DefaultParameterName = "Custom Authentication Credential Name"
            Case AuthenticationType.None
                DefaultParameterName = String.Empty
            Case Else
                Throw New NotImplementedException()
        End Select
    End Sub

    ''' <summary>
    ''' Load the authentication panel for the currently selected type with the
    ''' correct data loaded
    ''' </summary>
    Private Sub LoadAuthenticationPanel()
        Select Case mAuthentication.Authentication.Type
            Case AuthenticationType.None
                ConfigurationPanel = New EmptyAuthenticationPanel
            Case AuthenticationType.Basic
                ConfigurationPanel =
                    New BasicAuthenticationPanel(
                        DirectCast(mAuthentication.Authentication, BasicAuthentication))
            Case AuthenticationType.BearerToken
                ConfigurationPanel =
                    New BearerTokenAuthenticationPanel(
                        DirectCast(mAuthentication.Authentication, BearerTokenAuthentication))
            Case AuthenticationType.OAuth2ClientCredentials
                ConfigurationPanel =
                    New OAuth2AuthenticationPanel(
                        DirectCast(mAuthentication.Authentication, OAuth2ClientCredentialsAuthentication))
            Case AuthenticationType.OAuth2JwtBearerToken
                ConfigurationPanel =
                    New OAuth2WithJwtAuthenticationPanel(
                        DirectCast(mAuthentication.Authentication, OAuth2JwtBearerTokenAuthentication))
            Case AuthenticationType.Custom
                ConfigurationPanel =
                    New CustomAuthenticationPanel(
                        DirectCast(mAuthentication.Authentication, CustomAuthentication))
            Case Else
                Throw New NotImplementedException(
                    $"Panel not implemented for {mAuthentication.Authentication.Type.ToString()}")
        End Select
    End Sub

    ''' <summary>
    ''' Handles the authentication type combo box value changing, and sets the
    ''' selected authentication type property
    ''' </summary>
    Private Sub HandleAuthenticationTypeChanged(sender As Object, e As EventArgs) Handles cmbAuthType.SelectedIndexChanged
        AuthenticationType = CType(cmbAuthType.SelectedItem, KeyValuePair(Of String, AuthenticationType)).Value
        OnAuthenticationTypeChanged(New AuthenticationChangedEventArgs(mAuthentication.Authentication))
    End Sub

    ''' <summary>
    ''' Handles the data in the configuration panel changing and updates the 
    ''' undeerlying authentication data's configuration property.
    ''' </summary>
    Private Sub HandleConfigurationChanged(sender As Object, e As AuthenticationChangedEventArgs) _
        Handles mConfigurationPanel.ConfigurationChanged
        mAuthentication.Authentication = e.Authentication
    End Sub

    Private Sub OnAuthenticationTypeChanged(e As AuthenticationChangedEventArgs)
        RaiseEvent AuthenticationTypeChanged(Me, e)
    End Sub

    ''' <summary>
    ''' Handles the validation of this panel so that the parameter name field can be 
    ''' validated as required in the child IAuthentication panel, displaying the 
    ''' correct internationalised error message to the user based on the type of authentication being used
    ''' </summary>
    Private Sub HandlePanelValidating(sender As Object, e As CancelEventArgs) _
        Handles Me.Validating

        Dim sErr As String = String.Empty
        If Not mConfigurationPanel.ValidateParameterName(sErr) Then
            e.Cancel = True
            UserMessage.Err(sErr)
        End If

    End Sub

End Class