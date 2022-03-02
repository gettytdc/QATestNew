#If UNITTESTS Then
Imports BluePrism.DataPipeline

Namespace DataContractRoundTrips.Generators

    Public Class DataPipelineProcessConfigTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Yield Create(NameOf(DataPipelineProcessConfig), New DataPipelineProcessConfig(4, "test", "this is some lovely config", True))
        End Function

    End Class
End Namespace
#End If
