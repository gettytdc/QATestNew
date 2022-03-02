#If UNITTESTS Then
Imports BluePrism.DataPipeline

Namespace DataContractRoundTrips.Generators

    Public Class DataPipelineSettingsTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Yield Create(NameOf(DataPipelineSettings), New DataPipelineSettings(True, True, 30, True,
                                                                                New List(Of PublishedDashboardSettings) From {
                                                                                   New PublishedDashboardSettings(Guid.NewGuid(),
                                                                                                                  "dashboard1",
                                                                                                                  10,
                                                                                                                  DateTime.MinValue)
                                                                                   },
                                                                                True, "", False, 1433))
        End Function

    End Class
End Namespace
#End If
