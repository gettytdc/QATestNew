#If UNITTESTS Then

Imports BluePrism.DataPipeline

Namespace DataContractRoundTrips.Generators

    Public Class DataGatewayProcessStatusTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim testObject = New DataGatewayProcessStatus(DataGatewayProcessState.Running, "Test status message example.", Date.UtcNow)

            Yield Create("DataGatewayProcessStatus", testObject)
        End Function
    End Class

End Namespace
#End If
