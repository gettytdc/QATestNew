#If UNITTESTS Then
Imports BluePrism.Scheduling

Namespace DataContractRoundTrips.Generators

    Public Class DaySetTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim ds As New DaySet(DayOfWeek.Saturday, DayOfWeek.Sunday)

            Yield Create("Weekend", ds)

        End Function

    End Class

End Namespace
#End If
