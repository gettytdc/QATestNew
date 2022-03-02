Imports Microsoft.IdentityModel.Protocols.OpenIdConnect

Namespace Auth
    Public Interface IDiscoveryDocumentRetriever
        Function GetDiscoveryDocument(discoveryDocumentUrl As String) As OpenIdConnectConfiguration
    End Interface

End NameSpace