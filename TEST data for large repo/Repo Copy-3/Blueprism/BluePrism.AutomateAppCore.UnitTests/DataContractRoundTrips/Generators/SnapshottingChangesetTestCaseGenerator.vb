#If UNITTESTS
Imports BluePrism.Data.DataModels.WorkQueueAnalysis
Imports NodaTime

Namespace DataContractRoundTrips.Generators
    Public Class SnapshottingChangesetTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim days1 = New SnapshotDayConfiguration(True, True, True, True, True, False, false)
            Dim config1 = New SnapshotConfiguration(1, True, "TestObject", SnapshotInterval.FifteenMinutes, 
                                                        TimeZoneInfo.GetSystemTimeZones.First(), LocalTime.Midnight, LocalTime.Noon, days1)

            Dim days2 = New SnapshotDayConfiguration(True, True, True, True, True, True, True)
            Dim config2 = New SnapshotConfiguration(1, False, "TestObject2", SnapshotInterval.SixHours , 
                                                    TimeZoneInfo.GetSystemTimeZones.First(), LocalTime.Noon, LocalTime.MaxValue , days2)

            Dim oldQueueIds = New List(Of Integer)(New Integer() {1,2,5})
            Dim newQueueIds = New List(Of Integer)(New Integer() {5, 24, 158})

            Dim table As New DataTable()
            table.Columns.Add(new DataColumn("Id", gettype(integer)))
            table.Columns.Add(new DataColumn("ConfigId", gettype(integer)))
            table.Columns.Add(new DataColumn("Enabled", gettype(boolean)))
            Dim row = table.NewRow()
            row(0) = 24
            row(1) = 100
            row(2) = true
            table.Rows.Add(row)

            Dim row2 = table.NewRow()
            row2(0) = 158
            row2(1) = 105
            row2(2) = false
            table.Rows.Add(row2)

            Dim configTable = New QueueConfigurationsDataTable(table, new Dictionary(Of Integer, Integer))

            Dim testChangeset = New SnapshottingChangeset(config1, config2, oldQueueIds, newQueueIds, configTable)
            Yield Create("Snapshotting Changeset", testChangeset)
        End Function
    End Class
End NameSpace
#End If

