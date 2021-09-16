#If UNITTESTS Then

Imports BluePrism.Server.Domain.Models

Namespace DataContractRoundTrips.Generators

    Public Class UserNotFoundExceptionTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() _
            As IEnumerable(Of IRoundTripTestCase)

            Yield Create("User Not Found exception",
                         New UserNotFoundException($"The user {0} could not be found in the database: {1}",
                                                   "testUser01", "Some other info"))

        End Function

    End Class

End Namespace
#End If
