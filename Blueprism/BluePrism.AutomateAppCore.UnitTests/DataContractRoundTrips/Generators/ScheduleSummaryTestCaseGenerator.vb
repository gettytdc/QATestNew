#If UNITTESTS Then
Imports BluePrism.Scheduling

Namespace DataContractRoundTrips.Generators

Public Class ScheduleSummaryTestCaseGenerator
    Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim scheduleSummary = New ScheduleSummary With {
                .Id = 1,
                .Name = "testName",
                .Description = "testDescription",
                .InitialTaskId = 2,
                .IsRetired = True,
                .TasksCount = 3,
                .IntervalType = IntervalType.Day,
                .TimePeriod = 4,
                .StartPoint = DateTimeOffset.UtcNow,
                .EndPoint = DateTimeOffset.UtcNow,
                .DayOfWeek = new DaySet(1),
                .DayOfMonth = NthOfMonth.First,
                .StartDate = DateTimeOffset.UtcNow,
                .EndDate = DateTimeOffset.UtcNow,
                .CalendarId = 5,
                .CalendarName = "testCalendar"
            }

            Yield Create("Schedule Summary", scheduleSummary)
        End Function
    End Class
End Namespace
#End If
