#If UNITTESTS Then
Imports BluePrism.BPCoreLib

Namespace DataContractRoundTrips.Generators

    Public Class TimeRangeTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim tr As New clsTimeRange(New TimeSpan(11, 30, 59), New TimeSpan(12, 31, 58))

            Yield Create("Simple", tr)

        End Function

    End Class

End Namespace
#End If
