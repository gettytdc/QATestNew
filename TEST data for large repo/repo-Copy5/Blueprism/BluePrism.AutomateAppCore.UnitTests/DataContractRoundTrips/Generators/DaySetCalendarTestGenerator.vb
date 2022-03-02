#If UNITTESTS Then
Imports BluePrism.Scheduling
Imports BluePrism.Scheduling.Calendar

Namespace DataContractRoundTrips.Generators

    Public Class DaySetCalendarTestGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim days As New DaySet(DayOfWeek.Monday, DayOfWeek.Tuesday,
                                   DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday)
            Dim cal As New DaySetCalendar(days)
            Yield Create("Monday - Friday", cal)

            Dim daysEmpty As New DaySet()
            Dim calEmpty As New DaySetCalendar(daysEmpty)
            Yield Create("With Empty Day Set", calEmpty)
        End Function
    End Class

End Namespace
#End If
