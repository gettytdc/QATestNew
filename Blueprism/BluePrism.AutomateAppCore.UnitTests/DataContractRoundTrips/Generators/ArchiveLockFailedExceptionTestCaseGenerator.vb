#If UNITTESTS Then

Imports BluePrism.Server.Domain.Models

Namespace DataContractRoundTrips.Generators

    Public Class ArchiveLockFailedExceptionTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() _
            As IEnumerable(Of IRoundTripTestCase)

            Yield Create("Archive Lock Failed exception",
                         New ArchiveLockFailedException("The archive lock failed at {1}: {0}", "45678", DateTime.UtcNow))

        End Function

    End Class

End Namespace
#End If
