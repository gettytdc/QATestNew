#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Common.Security

Namespace DataContractRoundTrips.Generators

    Public Class CredentialTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim credential As clsCredential = CreateCredential()
            Yield Create("Standard", credential, Function(options) options.IgnoringCyclicReferences())

            credential = CreateCredential()
            Yield Create("Standard - BearerToken CredentialType", credential, Function(options) options.IgnoringCyclicReferences())

        End Function

        Private Function CreateCredential() As clsCredential

            Dim credential = New clsCredential()
            credential.ID = Guid.NewGuid()
            credential.Name = "Test credentials"
            credential.Type = CredentialType.General
            credential.Username = "Frank"
            credential.Password = New SafeString("password123")
            credential.Description = "Test credentials"
            credential.ExpiryDate = New DateTime(2025, 1, 1)
            credential.ProcessIDs = New clsSet(Of Guid)(Guid.NewGuid(), Guid.NewGuid())
            credential.ResourceIDs = New clsSet(Of Guid)(Guid.NewGuid(), Guid.NewGuid())
            credential.Roles.Add(New Role(Role.DefaultNames.ProcessAdministrators))
            credential.Properties = New Dictionary(Of String, SafeString) From {{"Property 1", New SafeString("Test Value 1")}, {"Property 2", New SafeString("Test Value 2")}}
            credential.IsInvalid = True
            credential.EncryptionKeyID = 1
            Return credential

        End Function

    End Class

End Namespace
#End If
