#If UNITTESTS Then
Imports System.Reflection
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Scheduling
Imports BluePrism.Scheduling.Calendar
Imports BluePrism.Scheduling.Triggers
Imports BPScheduler.UnitTests

Namespace DataContractRoundTrips.Generators

    ''' <summary>
    ''' Class which generates test cases for all the different Trigger types (aside from
    ''' the following which are abstract classes:
    ''' - BluePrism.Scheduling.Triggers.PeriodicTrigger()
    ''' - BluePrism.Scheduling.Triggers.EveryNthCalendarDayInNthMonth()
    ''' - BluePrism.Scheduling.Triggers.EveryNWeeksOnNthDayInCalendar()
    ''' - BluePrism.Scheduling.Triggers.EveryNDaysWithinCalendar()
    ''' </summary>
    Public Class TriggersTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim range As New clsTimeRange(
                New TimeSpan(2, 30, 45), New TimeSpan(8, 56, 45))

            Dim store As New DummyStore()

            Dim days As New DaySet(DayOfWeek.Monday, DayOfWeek.Tuesday,
                                   DayOfWeek.Wednesday, DayOfWeek.Thursday,
                                   DayOfWeek.Friday)
            Dim cal As New ScheduleCalendar(store.GetSchema())

            store.SaveCalendar(cal)

            Dim mockDataProvider As New DictionaryDataProvider(New Hashtable() From {
                                                                  {"userid", Guid.NewGuid()},
                                                                  {"username", "Donald"},
                                                                  {"created", DateTime.UtcNow},
                                                                  {"expiry", DateTime.UtcNow + TimeSpan.FromDays(7)},
                                                                  {"passwordexpiry", DateTime.UtcNow + TimeSpan.FromDays(1)},
                                                                  {"alerteventtypes", AlertEventType.ProcessRunning},
                                                                  {"alertnotificationtypes", AlertNotificationType.Sound},
                                                                  {"lastsignedin", DateTime.UtcNow - TimeSpan.FromMinutes(35)},
                                                                  {"isdeleted", False},
                                                                  {"passwordexpirywarninginterval", 3},
                                                                  {"locked", False},
                                                                  {"passworddurationweeks", 4}
                                                                  })

            Dim scheduler = New InertScheduler(store)
            Dim schedule, schedule2, schedule3 As New SessionRunnerSchedule(scheduler) With {
                    .Name = "Dummy Schedule"
                    }

            ' Generate the test cases - in many cases, the PrimaryMetaData property
            ' is used to indirectly test that non-public fields have been roundtripped
            ' In others we retrieve non-public state (calendars) to compare with original.

            'BluePrism.Scheduling.Triggers.EveryNSeconds()
            Dim ns As New EveryNSeconds(55)
            Yield Create("Trigger - EveryNSecs", ns)

            ns = New EveryNSeconds(55)
            ns.Schedule = schedule
            ns.Priority = 33
            ns.Mode = TriggerMode.Indeterminate
            ns.IsUserTrigger = True
            Yield Create("Trigger - EveryNSecs - Misc BaseTrigger properties", ns,
                         Function(options) options.IgnoringCyclicReferences(),
                         Sub(t) t.Schedule.Owner = scheduler,
                         serializerType:=TestCaseSerializerType.NetDataContractSerializer)

            'BluePrism.Scheduling.Triggers.EveryNDays()
            Dim nd As New EveryNDays(2)
            Yield Create("Trigger - EveryNDays", nd)

            'BluePrism.Scheduling.Triggers.EveryNDaysWithinDaySet()
            Dim ndwds As New EveryNDaysWithinDaySet(days)
            Yield Create("Trigger - EveryNDaysWithinDaySet", ndwds)
            Yield CreateWithCustomState("Trigger - EveryNDaysWithinDaySet - Compare calendars", ndwds, Function(t) GetCalendar(Of DaySetCalendar)(t))

            'BluePrism.Scheduling.Triggers.EveryNMinutes()
            Dim nm As New EveryNMinutes(40)
            Yield Create("Trigger - EveryNMins", nm)

            'BluePrism.Scheduling.Triggers.EveryNMinutesWithinRange()
            Dim nmwr As New EveryNMinutesWithinRange(4, range)
            Yield Create("Trigger - EveryNMinsWithinRange", nmwr)

            'BluePrism.Scheduling.Triggers.EveryNHours()
            Dim nh As New EveryNHours(4)
            Yield Create("EveryNHours", nh)

            'BluePrism.Scheduling.Triggers.EveryNHoursWithinRange()
            Dim nhwr As New EveryNHoursWithinRange(4, range)
            Yield Create("Trigger - EveryNHoursWithinRange", nhwr)

            'BluePrism.Scheduling.Triggers.EveryNthKnownCalendarDayInNthMonth()
            Dim nkcdinm As New EveryNthKnownCalendarDayInNthMonth(
                NthOfMonth.Second, cal)
            Yield Create(
                "Trigger - EveryNthKnownCalendarDayInNthMonth", nkcdinm)
            Yield CreateWithCustomState(
                "Trigger - EveryNthKnownCalendarDayInNthMonth - Compare calendar",
                nkcdinm,
                Function(t) GetCalendar(Of ScheduleCalendar)(t))

            'BluePrism.Scheduling.Triggers.EveryNthOfNthMonth()
            Dim nonm As New EveryNthOfNthMonth(
                5, NonExistentDatePolicy.FirstSupportedDayInNextMonth)
            Yield Create("Trigger - NthOfNthMonth", nonm)

            'BluePrism.Scheduling.Triggers.EveryNWeeks()
            Dim nw As New EveryNWeeks(12)
            Yield Create("EveryNWeeks", nw)

            'BluePrism.Scheduling.Triggers.EveryNWeeksOnNthDayInKnownCalendar()
            Dim nwondikc As New EveryNWeeksOnNthDayInKnownCalendar(
                8, NthOfWeek.Last, cal)
            Yield Create(
                "Trigger - EveryNWeeksOnNthDayInKnownCalendar", nwondikc)

            Yield CreateWithCustomState(
                "Trigger - EveryNWeeksOnNthDayInKnownCalendar - Compare calendar",
                nwondikc, Function(t) GetCalendar(Of ScheduleCalendar)(t))

            'BluePrism.Scheduling.Triggers.EveryNYears()
            Dim ny As New EveryNYears(5)
            Yield Create("Trigger - EveryNYears", ny)

            'BluePrism.Scheduling.Triggers.NeverTrigger()
            Dim never As New NeverTrigger
            Yield Create("Trigger - Never", never)

            'BluePrism.Scheduling.Triggers.BaseTriggerInstance 
            Dim inst As New BaseTriggerInstance(
                nd, DateTime.Today, TriggerMode.Indeterminate)
            Yield Create("Trigger - BaseTriggerInstance", inst)

            Dim once As New OnceTrigger(TriggerMode.Fire, DateTime.Today)
            Yield Create("Once", once)


            'BluePrism.Scheduling.Triggers.TriggerGroup()
            Dim grp As New TriggerGroup
            Dim t1 As New EveryNDays(2)
            Dim t2 As New EveryNWeeks(4)
            grp.Add(t1)
            grp.Add(t2)

            ' first test the group itself
            Yield Create("Trigger - Group", grp,
                         Function(options) options.IgnoringCyclicReferences(),
                         serializerType:=TestCaseSerializerType.NetDataContractSerializer)

            ' then a trigger in the group
            Yield Create("Trigger in a group", t2,
                         Function(options) options.IgnoringCyclicReferences(),
                         serializerType:=TestCaseSerializerType.NetDataContractSerializer)


            'BluePrism.Scheduling.Triggers.EveryNDaysWithinIdentifiedCalendar()
            Dim ndwic As New EveryNDaysWithinIdentifiedCalendar(cal.Id)
            schedule.AddTrigger(ndwic)

            Yield CreateWithCustomState(
                "Trigger - EveryNDaysWithinIdentifiedCal",
                ndwic,
                Function(t)
                    t.Schedule = schedule
                    Return t
                End Function,
                Function(o) o.
                                           IgnoringCyclicReferences.
                                           Excluding(Function(t) t.SelectedMemberInfo.Name = "Owner"),
                TestCaseSerializerType.NetDataContractSerializer
                )

            Yield CreateWithCustomState(
                "Trigger - EveryNDaysWithinIdentifiedCal - Compare calendar",
                ndwic,
                Function(t)
                    t.Schedule = schedule2
                    Return GetCalendar(Of ScheduleCalendar)(t)
                End Function,
                serializerType:=TestCaseSerializerType.NetDataContractSerializer)


            'BluePrism.Scheduling.Triggers.EveryNWeeksOnNthDayInIdentifiedCalendar()
            Dim nwondiic As New EveryNWeeksOnNthDayInIdentifiedCalendar(
                9, cal.Id)
            schedule2.AddTrigger(nwondiic)
            Yield CreateWithCustomState(
                "Trigger - EveryNWeeksOnNthDayInIdentifiedCal", nwondiic,
                Function(t)
                    t.Schedule = schedule2
                    Return t
                End Function,
                Function(o) o.
                                           IgnoringCyclicReferences.
                                           Excluding(Function(t) t.SelectedMemberInfo.Name = "Owner"),
                TestCaseSerializerType.NetDataContractSerializer)

            Yield CreateWithCustomState(
                "Trigger - EveryNWeeksOnNthDayInIdentifiedCal - Compare calendar",
                nwondiic,
                Function(t)
                    t.Schedule = schedule2
                    Return GetCalendar(Of ScheduleCalendar)(t)
                End Function,
                serializerType:=TestCaseSerializerType.NetDataContractSerializer)

            'BluePrism.Scheduling.Triggers.EveryNthIdentifiedCalendarDayInNthMonth()
            Dim nicdinm As New EveryNthIdentifiedCalendarDayInNthMonth(
                NthOfMonth.Fourth, cal.Id)
            schedule3.AddTrigger(nicdinm)
            Yield CreateWithCustomState(
                "Trigger - EveryNthIdentifiedCalendarDayInNthMonth", nicdinm,
                Function(t)
                    t.Schedule = schedule3
                    Return t
                End Function,
                Function(o) o.
                                           IgnoringCyclicReferences.
                                           Excluding(Function(t) t.SelectedMemberInfo.Name = "Owner"),
                TestCaseSerializerType.NetDataContractSerializer)

            Yield CreateWithCustomState(
                "Trigger - EveryNthIdentifiedCalendarDayInNthMonth - Compare calendar",
                nicdinm,
                Function(t)
                    t.Schedule = schedule3
                    Return GetCalendar(Of ScheduleCalendar)(t)
                End Function,
                serializerType:=TestCaseSerializerType.NetDataContractSerializer)

        End Function


        Private Function GetCalendar(Of T)(trigger As Object) As T
            Dim prop = trigger.GetType().GetProperty("Calendar", BindingFlags.Instance Or BindingFlags.NonPublic)
            Return CType(prop.GetValue(trigger, Nothing), T)
        End Function
    End Class
End Namespace
#End If
