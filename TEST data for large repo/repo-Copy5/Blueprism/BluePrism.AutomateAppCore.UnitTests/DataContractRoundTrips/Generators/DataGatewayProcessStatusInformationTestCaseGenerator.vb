#If UNITTESTS Then

Imports BluePrism.DataPipeline

Namespace DataContractRoundTrips.Generators

    Public Class DataGatewayProcessStatusInformationTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim testObject = New DataGatewayProcessStatusInformation() With {
                    .Id = 1,
                    .Name = "Test Object",
                    .ErrorMessage = "This is an error message, send help.",
                    .Status = DataGatewayProcessState.Online
                    }

            Yield Create("DataGatewayProcessStatusInformation", testObject)

        End Function
    End Class

End Namespace
#End If
