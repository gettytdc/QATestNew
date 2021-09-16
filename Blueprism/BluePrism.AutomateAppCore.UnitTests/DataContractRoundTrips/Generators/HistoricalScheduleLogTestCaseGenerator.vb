#If UNITTESTS Then
Namespace DataContractRoundTrips.Generators


    Public Class HistoricalScheduleLogTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() _
            As IEnumerable(Of IRoundTripTestCase)

            Dim scheduleLog1 = New HistoricalScheduleLog(1, "Scheduler 1",
                                                         TriggerActivationReason.Suppress,
                                                         DateTime.UtcNow)
            Yield CreateWithCustomState("Empty Schedule log", scheduleLog1,
                                        Function(s) GetCompareData(s))

            Yield CreateWithCustomState("Properties (empty)", scheduleLog1,
                                        Function(s) New With {s.Count})


            Dim scheduleLog2 = New HistoricalScheduleLog(1, "Scheduler 1",
                                                         TriggerActivationReason.Suppress,
                                                         DateTime.UtcNow)
            scheduleLog2.Schedule = New SessionRunnerSchedule(Nothing)
            scheduleLog2.Schedule.Name = "Schedule 2"
            Dim logEntry1 = New ScheduleLogEntry(ScheduleLogEventType.SessionCompleted,
                                                 1, 101)
            scheduleLog2.Add(logEntry1)

            Yield CreateWithCustomState("Session completed successfully",
                                        scheduleLog2, Function(s) GetCompareData(s))


            Yield CreateWithCustomState("Properties (completed)", scheduleLog2,
                                        Function(s) New With {s.Count})


            Dim scheduleLog3 = New HistoricalScheduleLog(1, "Scheduler 1",
                                                         TriggerActivationReason.Suppress,
                                                         DateTime.UtcNow)
            Dim logEntry2 = New ScheduleLogEntry(ScheduleLogEventType.ScheduleTerminated,
                                                 2, 102, "User error")
            scheduleLog3.Add(logEntry2)
            Yield CreateWithCustomState("SessionTerminated", scheduleLog3,
                                        Function(s) GetCompareData(s))


            Yield CreateWithCustomState("Properties (terminated)", scheduleLog3,
                                        Function(s) New With {s.Count})
        End Function

        ''' <summary>
        ''' Get the data to compare before and after the round trip from the log we have
        ''' created
        ''' </summary>
        ''' <param name="log">The test log to get the data from</param>
        ''' <returns>The data as a CompareData object</returns>
        Private Function GetCompareData(log As HistoricalScheduleLog) As CompareData

            Dim data = New CompareData

            data.id = log.Id
            data.reason = log.ActivationReason
            data.schedulerName = log.SchedulerName
            data.startTime = log.StartTime
            data.endTime = log.EndTime
            data.instanceTime = log.InstanceTime

            Return data
        End Function

        ''' <summary>
        ''' Class to define the structure of the data we want to compare on
        ''' </summary>
        Public Class CompareData
            Public Property id As Integer
            Public Property schedulerName As String
            Public Property reason As BluePrism.AutomateAppCore.TriggerActivationReason
            Public Property startTime As Date
            Public Property endTime As Date
            Public Property instanceTime As Date
        End Class

    End Class

End Namespace
#End If
