#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.BackgroundJobs

Namespace DataContractRoundTrips.Generators

    Public Class BackgroundJobDataTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim progress1 As New BackgroundJobData(BackgroundJobStatus.Success, 95, "Test description", New DateTime(2017, 1, 1, 12, 30, 22))
            Yield Create("Success Status and Properties", progress1)

            Dim progress2 As New BackgroundJobData(BackgroundJobStatus.Failure, 95, "Test description", New DateTime(2017, 1, 1, 12, 30, 22), New BackgroundJobError(New Exception("Oh no")))
            Yield Create("Failure Status and Properties", progress2)

        End Function

    End Class

End Namespace
#End If
