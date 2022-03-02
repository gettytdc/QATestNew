#If UNITTESTS Then
Imports BluePrism.Scheduling
Imports BluePrism.Scheduling.Triggers

Namespace DataContractRoundTrips.Generators

    Public Class SessionRunnerScheduleTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim scheduler As New InertScheduler()

            Dim schedule1 As New SessionRunnerSchedule(scheduler) With {
                    .Id = 1,
                    .Version = 2
                    }

            Yield Create("Empty", schedule1,
                         Function(options) options.IgnoringCyclicReferences() _
                            .Excluding(Function(s) s.Owner))

            Dim schedule2 As New SessionRunnerSchedule(scheduler) With {
                    .Id = 2,
                    .Version = 2
                    }
            schedule2.AddTrigger(New EveryNDays(5))
            schedule2.AddTrigger(New EveryNDays(3))

            Dim task1 = CreateTask(6561, "Task 1")
            Dim task2 = CreateTask(6562, "Task 2")
            schedule2.Add(task1, True)
            schedule2.Add(task2)
            schedule2.Retired = True
            SetField(schedule2, "mAbortMessage", "Test message")
            SetField(schedule2, "mInhibitAuditEvents", True)
            SetField(schedule2, "mExecuting", True)
            SetField(schedule2, "mAbortLock", True)

            Yield Create("With Triggers and Tasks", schedule2,
                         Function(options) options.
                            IgnoringCyclicReferences().
                            Excluding(Function(s) s.Owner))

            Yield CreateWithCustomState("Private mAbortMessage", schedule2,
                                        Function(a) GetField(a, "mAbortMessage"))

            Yield CreateWithCustomState("Private mInhibitAuditEvents", schedule2,
                                        Function(a) GetField(a, "mInhibitAuditEvents"))

            Yield CreateWithCustomState("Private mExecuting", schedule2,
                                        Function(a) GetField(a, "mExecuting"))

            Yield CreateWithCustomState("Private mAbortLock", schedule2,
                                        Function(a) GetField(a, "mAbortLock"))

            Dim runNowSchedule As New SessionRunnerSchedule(scheduler) With {
                    .Id = 3,
                    .Version = 2
                    }
            runNowSchedule.AddTrigger(New OnceTrigger(DateTime.UtcNow))
            runNowSchedule.AddAuditEvent(New ScheduleAuditEvent(
                ScheduleEventCode.ScheduleManuallyTriggered, runNowSchedule.Id, 0, Nothing, Nothing))
            Yield Create("WithAuditEvents", runNowSchedule,
                         Function(options) options.IgnoringCyclicReferences().Excluding(Function(s) s.Owner))

        End Function

        Private Function CreateTask(id As Integer, name As String) As ScheduledTask
            Dim t As New ScheduledTask() With {
                    .Id = id,
                    .Name = name,
                    .Description = "Description " & name,
                    .OnSuccess = New ScheduledTask() With {.Id = 623, .Name = "Success"},
                    .OnFailure = New ScheduledTask() With {.Id = 624, .Name = "Failure"},
                    .FailFastOnError = True
                    }
            t.AddSession(New ScheduledSession(0, Guid.NewGuid, "1", Guid.NewGuid, False, False, Nothing))
            t.AddSession(New ScheduledSession(0, Guid.NewGuid, "2", Guid.NewGuid, False, False, Nothing))
            Return t
        End Function

    End Class

End Namespace
#End If
