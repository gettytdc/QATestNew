#If UNITTESTS Then
Imports BluePrism.Scheduling
Imports BluePrism.Scheduling.Calendar
Imports BPScheduler.UnitTests

Namespace DataContractRoundTrips.Generators

    Public Class ScheduleCalendarTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim schema = New Builders.PublicHolidaySchemaBuilder().Build()
            Dim group = DummyStore.Holiday.EnglandAndWales
            Dim holidayOverride = schema.GetHolidays(group).First()

            Dim calendar = New ScheduleCalendar(schema)
            calendar.PublicHolidayGroup = group
            calendar.WorkingWeek = New DaySet(DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday)
            calendar.PublicHolidayOverrides.Add(holidayOverride)
            calendar.NonWorkingDays.Add(New DateTime(2017, 1, 1))
            calendar.NonWorkingDays.Add(New DateTime(2017, 1, 2))
            calendar.NonWorkingDays.Add(New DateTime(2017, 1, 3))
            calendar.NonWorkingDays.Add(New DateTime(2017, 1, 4))
            calendar.NonWorkingDays.Add(New DateTime(2017, 1, 5))
            calendar.NonWorkingDays.Add(New DateTime(2017, 1, 6))
            calendar.NonWorkingDays.Add(New DateTime(2017, 1, 7))
            calendar.Id = 56

            Yield Create("Fully populated", calendar)

            Yield CreateWithCustomState("Fully populated - testing run dates", calendar, Function(c) GetRunDates(c))

        End Function

        Private Function GetRunDates(calendar As ScheduleCalendar) As Dictionary(Of Date, Boolean)

            Dim runDates As New Dictionary(Of Date, Boolean)
            Dim current = New Date(2017, 1, 1)
            Dim last = New Date(2017, 12, 31)
            While current <= last
                runDates(current) = calendar.CanRun(current)
                current = current.AddDays(1)
            End While
            Return runDates

        End Function

    End Class

End Namespace
#End If
