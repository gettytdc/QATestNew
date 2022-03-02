#If UNITTESTS Then

Imports BluePrism.Server.Domain.Models

Namespace DataContractRoundTrips.Generators

    Public Class ActiveQueueLockFailedExceptionTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() _
            As IEnumerable(Of IRoundTripTestCase)

            Yield Create("Active Queue Lock Failed exception",
                         New ActiveQueueLockFailedException("QueueName", "LockName", DateTime.UtcNow))

        End Function

    End Class

End Namespace
#End If
