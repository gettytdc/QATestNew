#If UNITTESTS
Imports BluePrism.Data.DataModels.WorkQueueAnalysis

Namespace DataContractRoundTrips.Generators
    Public Class WorkQueueSnapshotInformationTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim testObject = New WorkQueueSnapshotInformation(1, 2, TimeZoneInfo.GetSystemTimeZones.First())
            testObject.snapshotIdsToProcess = New List(Of WorkQueueSnapshotInformation.SnapshotTriggerInformation)
            testObject.snapshotIdsToProcess.Add(New WorkQueueSnapshotInformation.SnapshotTriggerInformation() With 
                                                             {
                                                                .SnapshotId = 1, 
                                                                .SnapshotTimeOffset = DateTimeOffset.Now,
                                                                .EventType = SnapshotTriggerEventType.Snapshot
                                                             })

            Yield Create("WorkQueueSnapshotInformation", testObject)
        End Function
    End Class
End NameSpace
#End If