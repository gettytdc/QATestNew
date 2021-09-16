#If UNITTESTS Then
Imports BluePrism.Scheduling
Imports BluePrism.Scheduling.Calendar
Imports BPScheduler.UnitTests

Namespace DataContractRoundTrips.Generators.Builders

    ''' <summary>
    ''' Creates PublicHolidaySchema instances for testing. Currently populates a fixed range of
    ''' UK bank holidays.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class PublicHolidaySchemaBuilder

        Public Function Build() As PublicHolidaySchema

            Dim dt As New DataTable
            With dt.Columns
                .Add("groupname", GetType(String))
                .Add("id", GetType(Integer))
                .Add("holidayname", GetType(String))
                .Add("dd", GetType(Integer))
                .Add("mm", GetType(Integer))
                .Add("dayofweek", GetType(Integer))
                .Add("nthofmonth", GetType(Integer))
                .Add("eastersunday", GetType(Boolean))
                .Add("relativetoholiday", GetType(Integer))
                .Add("relativedaydiff", GetType(Integer))
                .Add("excludesaturday", GetType(Boolean))
                .Add("shiftdaytypeid", GetType(Integer))
                .Add("relativedayofweek", GetType(Integer))

            End With
            With dt.Rows
                .Add(DummyStore.Holiday.EnglandAndWales, 1, "Easter Sunday", 0, 0, Nothing, NthOfMonth.None, True, Nothing, 0, Nothing)
                .Add(DummyStore.Holiday.EnglandAndWales, 2, "Christmas Day", 25, 12, Nothing, NthOfMonth.None, False, Nothing, 0, Nothing)
                .Add(DummyStore.Holiday.EnglandAndWales, 3, "New Years' Day", 1, 1, Nothing, NthOfMonth.None, False, Nothing, 0, Nothing)
                .Add(DummyStore.Holiday.RepublicOfIreland, 5, "St Patrick's Day", 17, 3, Nothing, NthOfMonth.None, False, Nothing, 0, Nothing)
                .Add(DummyStore.Holiday.EnglandAndWales, 8, "May Day", 0, 5, DayOfWeek.Monday, NthOfMonth.First, False, Nothing, 0, Nothing)
                .Add(DummyStore.Holiday.EnglandAndWales, 9, "May Bank Holiday", 0, 5, DayOfWeek.Monday, NthOfMonth.First, False, Nothing, 0, Nothing)
                .Add(DummyStore.Holiday.EnglandAndWales, 10, "Spring Bank Holiday", 0, 5, DayOfWeek.Monday, NthOfMonth.Last, False, Nothing, 0, Nothing)
                .Add(DummyStore.Holiday.EnglandAndWales, 11, "June Bank Holiday", 0, 6, DayOfWeek.Monday, NthOfMonth.First, False, Nothing, 0, Nothing)
                .Add(DummyStore.Holiday.NorthernIreland, 12, "Orangemen's Day", 12, 7, Nothing, NthOfMonth.None, False, Nothing, 0, Nothing)
                .Add(DummyStore.Holiday.EnglandAndWales, 13, "August Bank Holiday", 0, 8, DayOfWeek.Monday, NthOfMonth.First, False, Nothing, 0, Nothing)
                .Add(DummyStore.Holiday.EnglandAndWales, 14, "Summer Bank Holiday", 0, 8, DayOfWeek.Monday, NthOfMonth.First, False, Nothing, 0, Nothing)
                .Add(DummyStore.Holiday.Scotland, 15, "Summer Bank Holiday", 0, 8, DayOfWeek.Monday, NthOfMonth.Last, False, Nothing, 0, Nothing)
                .Add(DummyStore.Holiday.Scotland, 16, "October Bank Holiday", 0, 10, DayOfWeek.Monday, NthOfMonth.Last, False, Nothing, 0, Nothing)
                'Ensure relative fields are something other than default values
                .Add(DummyStore.Holiday.Scotland, 17, "Not a real holiday", 2, 10, DayOfWeek.Monday, NthOfMonth.Last, True, 16, 2, Nothing)
            End With

            Return New PublicHolidaySchema(dt)
        End Function

    End Class

End Namespace
#End If
