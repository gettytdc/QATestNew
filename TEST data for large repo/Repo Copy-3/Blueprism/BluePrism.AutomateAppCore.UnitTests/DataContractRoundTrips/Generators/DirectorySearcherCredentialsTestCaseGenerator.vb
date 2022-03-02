#If UNITTESTS Then
Imports BluePrism.Common.Security
Imports BluePrism.Core.ActiveDirectory.DirectoryServices

Namespace DataContractRoundTrips.Generators

    Public Class DirectorySearcherCredentialsTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Yield Create("Directory Searcher Credentials", New DirectorySearcherCredentials("test user", "password&^%$".AsSecureString()))
        End Function
    End Class


End Namespace
#End If
