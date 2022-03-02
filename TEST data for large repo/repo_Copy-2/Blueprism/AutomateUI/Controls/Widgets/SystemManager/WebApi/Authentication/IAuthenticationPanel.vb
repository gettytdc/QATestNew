Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore.WebApis.Authentication

NameSpace Controls.Widgets.SystemManager.WebApi.Authentication

    ''' <summary>
    ''' Interface that describes a panel used to make changes to the configuration of
    ''' a Web API's authentication settings.
    ''' </summary>
    Friend Interface IAuthenticationPanel

        ''' <summary>
        ''' The underlying Authentication configuration
        ''' </summary>
        ReadOnly Property Configuration As IAuthentication

        ''' <summary>
        ''' The event raised when a change has been made to the configuration on the 
        ''' panel
        ''' </summary>
        Event ConfigurationChanged As AuthenticationChangedEventHandler

        ''' <summary>
        ''' The method to call when a change has been the the configuration on this
        ''' panel, which should then raise the <see cref="ConfigurationChanged"/>
        ''' event
        ''' </summary>
        ''' <param name="e">The event args object</param>
        Sub OnConfigurationChanged(e As AuthenticationChangedEventArgs)

        ''' <summary>
        ''' Gets a list of credentials within the system, which can be used to
        ''' populate the credentials styled combo box on the panel.
        ''' </summary>
        Property Credentials As IEnumerable(Of clsCredential)

        ''' <summary>
        ''' Gets a string representing the default Credential Parameter Name, 
        ''' according to they type of authentication being used
        ''' </summary>
        Property DefaultParameterName As String

        ''' <summary>
        ''' Validates the Parameter Name in the Authentication Credential Panel, 
        ''' ensuring that if Expose to Process is set to true then the parameter name 
        ''' is not null or empty.
        ''' </summary>
        ''' <param name="sErr">A ByRef string parameter to return the correct 
        ''' internationalised error message (if required) according to the type of 
        ''' authentication being used</param>
        ''' <returns>Boolean representing whether the validation was successful or 
        ''' not</returns>
        Function ValidateParameterName(ByRef sErr As String) As Boolean


    End Interface
End NameSpace