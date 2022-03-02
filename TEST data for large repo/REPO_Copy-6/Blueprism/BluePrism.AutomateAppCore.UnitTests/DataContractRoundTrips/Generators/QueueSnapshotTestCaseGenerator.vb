#If UNITTESTS Then
Imports BluePrism.Data.DataModels.WorkQueueAnalysis
Imports NodaTime

Namespace DataContractRoundTrips.Generators


    Public Class QueueSnapshotTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim testObject = New QueueSnapshot(1, 1, LocalTime.FromSecondsSinceMidnight(0), IsoDayOfWeek.Monday, SnapshotInterval.FifteenMinutes, SnapshotTriggerEventType.InterimSnapshot)

            Yield Create("Configured Snapshot", testObject)
        End Function
    End Class
End Namespace
#End If
