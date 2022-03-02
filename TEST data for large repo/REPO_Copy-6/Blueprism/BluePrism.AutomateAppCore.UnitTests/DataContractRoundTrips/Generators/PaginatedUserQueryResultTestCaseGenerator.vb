#If UNITTESTS Then
Imports BluePrism.Core.ActiveDirectory.DirectoryServices
Imports BluePrism.Core.ActiveDirectory.UserQuery
Imports Moq

Namespace DataContractRoundTrips.Generators

    Public Class PaginatedUserQueryResultTestCaseGenerator : Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Yield Create("Paginated User Query Result- Success", PaginatedUserQueryResult.Success(Users().ToList(), 150))
            Yield Create("Paginated User Query Result - Invalid Credentials", PaginatedUserQueryResult.InvalidCredentials())
            Yield Create("Paginated User Query Result - Invalid Query", PaginatedUserQueryResult.InvalidQuery())
        End Function

        Private Iterator Function Users() As IEnumerable(Of ActiveDirectoryUser)
            Yield New ActiveDirectoryUser("some.person@some.domain.com", "S-1-1-76-18423748-3438888550-264708130-6117", "CN=some,CN=person,CN=admin,OU=system,DC=some,DC=domain,DC=COM", True)
            Yield New ActiveDirectoryUser(Nothing, Nothing, Nothing, False)
            Yield New ActiveDirectoryUser(String.Empty, String.Empty, String.Empty, False)
        End Function

    End Class


End Namespace
#End If
