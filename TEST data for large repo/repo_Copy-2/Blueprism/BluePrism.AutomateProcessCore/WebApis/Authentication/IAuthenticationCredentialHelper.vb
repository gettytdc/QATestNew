Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.Server.Domain.Models

Namespace WebApis.Credentials
    Public Interface IAuthenticationCredentialHelper

        ''' <summary>
        ''' Gets the credential based on the configuration of the 
        ''' Authentication Credential and the input parameters specified for an 
        ''' action. If the authentication credential configuration is set up to be 
        ''' exposed to a process, and the input parameters have a value for the 
        ''' credential name then it will get that credential from the credential 
        ''' store. Otherwise, it will go to the credential store and get the 
        ''' credential that is specified in the Authentication Credential object.
        ''' </summary>
        ''' <param name="authenticationCredential">The configuration of the 
        ''' authentication credential</param>
        ''' <param name="actionParameters">Input Parameter values supplied for an 
        ''' action</param>
        ''' <param name="sessionId">The session which is requesting access to the 
        ''' credential, used to check restrictions</param>
        ''' <returns>The credential</returns>
        ''' <exception cref="MissingItemException">Exception thrown if the credential 
        ''' is missing from the configuration and parameters or if the credential
        ''' cannot be found in the credential store</exception>
        Function GetCredential(authenticationCredential As AuthenticationCredential,
                               actionParameters As Dictionary(Of String, clsProcessValue),
                               sessionId As Guid) As ICredential
    End Interface
End Namespace
