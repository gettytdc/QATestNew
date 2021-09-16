#If UNITTESTS Then

Imports BluePrism.Server.Domain.Models

Namespace DataContractRoundTrips.Generators

    Public Class AlreadyLockedExceptionTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() _
            As IEnumerable(Of IRoundTripTestCase)

            Yield Create("Already Locked exception", New AlreadyLockedException("Item {0} is already locked", "156"))

        End Function

    End Class

End Namespace
#End If
