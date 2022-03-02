#If UNITTESTS Then
Namespace DataContractRoundTrips.Generators


    Public Class DateRangeTestGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim range As New BluePrism.BPCoreLib.clsDateRange(
                DateTime.Today, DateTime.Today.AddDays(12))
            Yield Create("Simple Date Range", range)

        End Function
    End Class

End Namespace
#End If
