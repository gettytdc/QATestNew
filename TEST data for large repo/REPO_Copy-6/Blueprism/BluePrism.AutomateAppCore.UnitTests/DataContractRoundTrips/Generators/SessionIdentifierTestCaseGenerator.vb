#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore

Namespace DataContractRoundTrips.Generators

    Public Class SessionIdentifierTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim identifier1 = New DigitalWorkerSessionIdentifier(Guid.NewGuid())
            Yield Create("Digital worker session identifier", identifier1)

            Dim identifier2 = New RuntimeResourceSessionIdentifier(Guid.NewGuid(), 5)
            Yield Create("Runtime resource session identifier", identifier2)
        End Function
    End Class

End Namespace
#End If
