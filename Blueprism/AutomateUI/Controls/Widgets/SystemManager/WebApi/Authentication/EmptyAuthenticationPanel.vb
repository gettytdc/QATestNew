Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateAppCore

Namespace Controls.Widgets.SystemManager.WebApi.Authentication

    ''' <summary>
    ''' Empty panel used when a Web API's authentication is set to None
    ''' </summary>
    Friend Class EmptyAuthenticationPanel : Implements IAuthenticationPanel

        ''' <inheritdoc/>
        Public Event ConfigurationChanged As AuthenticationChangedEventHandler Implements IAuthenticationPanel.ConfigurationChanged

        ''' <summary>
        ''' Create a new instance of the <see cref="EmptyAuthenticationPanel"/> used when
        ''' a Web API's authentication is set to None.
        ''' </summary>
        Public Sub New()
            ' This call is required by the designer.
            InitializeComponent()
        End Sub

        ''' <inheritdoc/>
        <Browsable(False),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public ReadOnly Property Configuration As IAuthentication Implements IAuthenticationPanel.Configuration
            Get
                Return New EmptyAuthentication()
            End Get
        End Property

        ''' <inheritdoc/>
        Public Property Credentials As IEnumerable(Of clsCredential) Implements IAuthenticationPanel.Credentials

        ''' <inheritdoc/>
        Public Property DefaultParameterName As String Implements IAuthenticationPanel.DefaultParameterName

        ''' <inheritdoc/>
        Public Sub OnConfigurationChanged(e As AuthenticationChangedEventArgs) Implements IAuthenticationPanel.OnConfigurationChanged
            RaiseEvent ConfigurationChanged(Me, e)
        End Sub

        ''' <inheritdoc/>
        Private Function ValidateParameterName(ByRef sErr As String) As Boolean _
            Implements IAuthenticationPanel.ValidateParameterName
            ' No parameter required for empty auth
            Return True
        End Function

    End Class
End NameSpace