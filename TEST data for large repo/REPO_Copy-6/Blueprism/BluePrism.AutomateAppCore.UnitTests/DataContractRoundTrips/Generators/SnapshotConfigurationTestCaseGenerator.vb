#If UNITTESTS Then
Imports BluePrism.Data.DataModels.WorkQueueAnalysis
Imports NodaTime

Namespace DataContractRoundTrips.Generators
    Public Class SnapshotConfigurationTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim testObject1 = New SnapshotDayConfiguration(True, True, True, True, True, False, False)
            Dim testObject2 = New SnapshotConfiguration(1, True, "TestObject", SnapshotInterval.FifteenMinutes,
                                                        TimeZoneInfo.GetSystemTimeZones.First(), LocalTime.Midnight, LocalTime.Noon, testObject1)

            Yield Create("Snapshot Configuration", testObject2)
        End Function
    End Class

    Public Class SnapshotConfigurationCollectionTestCaseGenerator
        Inherits TestCaseGenerator
        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim days1 = New SnapshotDayConfiguration(True, True, True, True, True, False, False)
            Dim testObject1 = New SnapshotConfiguration(1, True, "TestObject", SnapshotInterval.FifteenMinutes,
                                                        TimeZoneInfo.GetSystemTimeZones.First(), LocalTime.Midnight, LocalTime.Noon, days1)

            Dim days2 = New SnapshotDayConfiguration(True, True, True, True, True, False, False)
            Dim testObject2 = New SnapshotConfiguration(1, True, "TestObject2", SnapshotInterval.FifteenMinutes,
                                                        TimeZoneInfo.GetSystemTimeZones.First(), LocalTime.Midnight, LocalTime.Noon, days2)

            Dim testList = New List(Of SnapshotConfiguration)
            testList.Add(testObject1)
            testList.Add(testobject2)
            Yield Create("Snapshot Configuration List", testList)
        End Function

    End Class
End Namespace
#End If

