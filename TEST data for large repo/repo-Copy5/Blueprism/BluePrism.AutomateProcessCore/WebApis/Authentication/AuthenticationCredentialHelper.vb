Imports BluePrism.AutomateProcessCore.WebApis.Credentials
Imports BluePrism.Server.Domain.Models

Namespace WebApis.Authentication
    ''' <summary>
    ''' Class containing helper methods for <see cref="AuthenticationCredential"/>
    ''' </summary>
    Public Class AuthenticationCredentialHelper
        Implements IAuthenticationCredentialHelper

        Private ReadOnly mCredentialStore As ICredentialStore

        Sub New(credentialStore As ICredentialStore)
            mCredentialStore = credentialStore
        End Sub

        ''' <inheritdoc />
        Public Function GetCredential(authenticationCredential As AuthenticationCredential,
                                       actionParameters As Dictionary(Of String, clsProcessValue),
                                                                            sessionId As Guid) _
                                       As ICredential Implements IAuthenticationCredentialHelper.GetCredential

            Dim credentialName = GetCredentialName(authenticationCredential, actionParameters)

            Dim credential = mCredentialStore.GetCredential(credentialName, sessionId)
            If credential Is Nothing Then Throw New MissingItemException(credentialName)
            Return credential

        End Function

        ''' <summary>
        ''' Gets the name of the credential based on the configuration of the 
        ''' Authentication Credential and the input parameters specified for an 
        ''' action. If the authentication credential configuration is set up to be
        ''' exposed to a process, and the input parameters have a value for the 
        ''' credential name then it will use that value. Otherwise, the name will be 
        ''' the one stored in this Authentication 
        ''' Credential object.
        ''' </summary>
        ''' <param name="actionParameters">Input Parameter values supplied for an 
        ''' action</param>
        ''' <returns>The name of the credential</returns>
        ''' <exception cref="MissingItemException">Exception thrown if the credential 
        ''' name is missing from the configuration and parameters</exception>
        Public Function GetCredentialName(authenticationCredential As AuthenticationCredential,
                                          actionParameters As Dictionary(Of String, clsProcessValue)) As String

            Dim configuredName = authenticationCredential.CredentialName
            Dim configuredNameEmpty = String.IsNullOrWhiteSpace(configuredName)

            If authenticationCredential.ExposeToProcess Then
                Dim parameterName = authenticationCredential.InputParameterName
                Dim parameterValue As clsProcessValue = Nothing
                Dim parameterValid = actionParameters.TryGetValue(parameterName, parameterValue) AndAlso
                    parameterValue IsNot Nothing AndAlso
                    Not String.IsNullOrWhiteSpace(CType(parameterValue, String))
                If parameterValid Then
                    Return CType(parameterValue, String)
                ElseIf configuredNameEmpty Then
                    Dim message = String.Format(WebApiResources.CredentialParameterMissingErrorTemplate,
                                                parameterName)
                    Throw New MissingItemException(message)
                End If
            End If

            If configuredNameEmpty Then
                Throw New MissingItemException(WebApiResources.CredentialNotSelectedError)
            Else
                Return configuredName
            End If

        End Function


    End Class
End Namespace
