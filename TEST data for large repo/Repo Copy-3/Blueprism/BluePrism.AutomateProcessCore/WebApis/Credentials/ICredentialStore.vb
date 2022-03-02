

Namespace WebApis.Credentials

    ''' <summary>
    ''' Provides access to credentials
    ''' </summary>
    Public Interface ICredentialStore

        ''' <summary>
        ''' Retrieves a credential based upon its name in the system and session ID.
        ''' </summary>
        ''' <param name="name">The name of the credential to return. </param>
        ''' <param name="sessionId">The id of the session requesting the credential, 
        ''' used for checking access restrictions.</param>
        ''' <returns>An ICredential instance or nothing if a credential could not be
        ''' found with the specified name.</returns>
        Function GetCredential(name As String, sessionId As Guid) As ICredential

        

    End Interface

End Namespace