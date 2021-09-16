Namespace WebApis.Authentication

    ''' <summary>
    ''' Contains configuration for a specific type of authentication within the configuration of 
    ''' a Web API, that uses a credential to store the information that will used
    ''' to uniquely identify who is making the Web API Request.
    ''' </summary>
    Public Interface ICredentialAuthentication

        ''' <summary>
        ''' The credential used to uniquely identify who is making the Web API Request.
        ''' </summary>
        ReadOnly Property Credential As AuthenticationCredential

    End Interface
End Namespace
