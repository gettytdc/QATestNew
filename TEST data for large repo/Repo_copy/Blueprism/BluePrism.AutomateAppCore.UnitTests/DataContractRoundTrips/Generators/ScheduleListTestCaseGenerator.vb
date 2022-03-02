#If UNITTESTS Then
Imports BluePrism.Scheduling

Namespace DataContractRoundTrips.Generators

    Public Class ScheduleListTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim listNoSchedules, listWithSchedules As New ScheduleList()

            'test a list with no schedules as this is what you get when AllSchedules is set to True
            listNoSchedules.ListType = ScheduleListType.Timetable
            listNoSchedules.SetDateRange(New Date(2016, 12, 2), New Date(2017, 8, 1))
            Yield Create(
                "List with No Schedules",
                listNoSchedules,
                Function(options) options.Excluding(Function(sl) sl.Schedules),
                serializerType:=TestCaseSerializerType.NetDataContractSerializer)


            Dim scheduler As IScheduler = New InertScheduler()
            Dim sched As New SessionRunnerSchedule(scheduler)
            Dim sched2 As New SessionRunnerSchedule(scheduler)
            listWithSchedules.AddSchedule(sched)
            listWithSchedules.AddSchedule(sched2)
            Yield Create(
                "List With Schedules",
                listWithSchedules,
                Function(options) options.Excluding(Function(sl) sl.Schedules),
                serializerType:=TestCaseSerializerType.NetDataContractSerializer)

            Yield CreateWithCustomState(
                "Testing ScheduleIds",
                listWithSchedules,
                Function(sl As ScheduleList) sl.ScheduleIds,
                serializerType:=TestCaseSerializerType.NetDataContractSerializer)
        End Function
    End Class

End Namespace
#End If
