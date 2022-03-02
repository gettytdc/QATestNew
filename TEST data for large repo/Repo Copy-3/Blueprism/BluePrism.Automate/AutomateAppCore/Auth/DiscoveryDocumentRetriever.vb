Imports Microsoft.IdentityModel.Protocols
Imports Microsoft.IdentityModel.Protocols.OpenIdConnect

Namespace Auth

    Public Class DiscoveryDocumentRetriever : Implements IDiscoveryDocumentRetriever
        Public Function GetDiscoveryDocument(discoveryDocumentUrl As String) _
            As OpenIdConnectConfiguration Implements IDiscoveryDocumentRetriever.GetDiscoveryDocument

            Dim configurationManager = New ConfigurationManager(Of OpenIdConnectConfiguration)(
                discoveryDocumentUrl, New OpenIdConnectConfigurationRetriever(), New HttpDocumentRetriever())

            Return configurationManager.GetConfigurationAsync(Nothing).GetAwaiter.GetResult()
        End Function

    End Class


End Namespace
