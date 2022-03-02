#If UNITTESTS Then
Imports BluePrism.DataPipeline.DataPipelineOutput

Namespace DataContractRoundTrips.Generators

    Public Class DataGatewayOutputTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Yield Create(NameOf(DatabaseOutput), New DatabaseOutput("name", "id"))
            Yield Create(NameOf(DatabaseOutput), New SplunkOutput("nameS", "idS"))
            Yield Create(NameOf(OutputType), New OutputType("name", "id"))
            Dim x = New DataPipelineOutputConfig() With {
                    .Id = 1,
                    .UniqueReference = New Guid(),
                    .Name = "",
                    .IsSessions = True,
                    .IsDashboards = True,
                    .OutputType = New DatabaseOutput("test", "test"),
                    .OutputOptions = {New OutputOption("op1", "op2")}.ToList(),
                    .SessionCols = "sdfsafsas",
                    .DashboardCols = "sdfasdfasdfas",
                    .DateCreated = DateTime.UtcNow,
                    .AdvancedConfiguration = "ad",
                    .IsAdvanced = True}

            Yield Create(NameOf(DataPipelineOutputConfig), x)
        End Function
    End Class
End Namespace
#End If
