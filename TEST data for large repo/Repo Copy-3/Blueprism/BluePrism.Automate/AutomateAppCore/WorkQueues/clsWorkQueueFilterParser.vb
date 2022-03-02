Imports System.Xml
Imports System.Globalization
Imports System.Text.RegularExpressions
Imports BluePrism.Core.Xml

''' <summary>
''' Facilitates the building of a Work Queue UI filter, by the incremental
''' application of successive constraints.
''' </summary>
Public Class clsWorkQueueFilterBuilder

    ''' <summary>
    ''' The filter resulting from the build
    ''' </summary>
    Public ReadOnly Property Filter() As WorkQueueFilter
        Get
            Return mFilter
        End Get
    End Property
    Private mFilter As New WorkQueueFilter

    ''' <summary>
    ''' Applies a constraint to the filter under construction
    ''' </summary>
    ''' <param name="columnName">The column name against which the constraint
    ''' is to be applied. This is the key of the column name, as it appears in
    ''' the UI.</param>
    ''' <param name="filterValue">The constraint value to be applied.</param>
    ''' <returns>True if the constraint is successfully applied, False otherwise.</returns>
    Public Function ApplyConstraint(ByVal columnName As String, ByRef filterValue As String) As Boolean
        Return ApplyConstraint(Date.Now, columnName, filterValue)
    End Function

    ''' <summary>
    ''' Applies a constraint to the filter under construction
    ''' </summary>
    ''' <param name="baseDate">The date/time to use as base date to which any
    ''' relative date constraints will be applied. This is typically just
    ''' <see cref="DateTime.Now"/> and is only likely to be different for unit tests
    ''' which want to apply known dates.
    ''' </param>
    ''' <param name="columnName">The column name against which the constraint
    ''' is to be applied. This is the key of the column name, as it appears in
    ''' the UI.</param>
    ''' <param name="filterValue">The constraint value to be applied. This is passed
    ''' ByRef as it can be modified if rounding takes place on a timespan column</param>
    ''' <returns>True if the constraint is successfully applied, False otherwise.</returns>
    Friend Function ApplyConstraint(baseDate As Date, ByVal columnName As String, ByRef filterValue As String) As Boolean
        Const DatePattern As String = "([\d\s:\/.-]{3,16})" 'very loose date pattern - it's up to DateTime.TryParse to make sense of it
        Const DateComparison As String = "^\s?((<|>)?=?)?\s?" + DatePattern + "\s?$" 'match whole string only as the pattern is so loose
        Static Reg_LastNUnits As New Regex("Last\s(\d+)\s?(minute|min|hour|hr|h|day|d)s?", RegexOptions.IgnoreCase Or RegexOptions.Singleline Or RegexOptions.Compiled)
        Static Reg_NextNUnits As New Regex("Next\s(\d+)\s?(minute|min|hour|hr|h|day|d)s?", RegexOptions.IgnoreCase Or RegexOptions.Singleline Or RegexOptions.Compiled)
        Static Reg_DateMatch As New Regex(DateComparison, RegexOptions.IgnoreCase Or RegexOptions.Singleline Or RegexOptions.Compiled)

        Const IntPattern As String = "\s*?((?:<|>)?=?)?\s*(\d+)\s*?"
        Static Reg_IntValueMatch As New Regex(IntPattern, RegexOptions.Compiled)

        Const TimespanPattern As String = "\s*?((?:<|>)?=?)?\s*([\d.:]+)\s*?"
        Static Reg_TimespanValueMatch As New Regex(TimespanPattern, RegexOptions.Compiled)
        Static OneDayAsTimespan As New TimeSpan(1, 0, 0, 0)

        filterValue = System.Text.RegularExpressions.Regex.Replace(filterValue.Trim(), "\s+", " ")
        Dim filterValueAfterRounding As String = ""
        Select Case clsWorkQueueFilterBuilder.GetColumnType(columnName)
            Case ColumnTypes.DateAndTimeOfPastEvent, ColumnTypes.DateAndTimeOfPossibleEvent
                If filterValue = "" OrElse filterValue.Equals("All", StringComparison.CurrentCultureIgnoreCase) Then
                    Return True
                ElseIf filterValue.Equals("Today", StringComparison.CurrentCultureIgnoreCase) Then
                    ApplyFilterStartDate(mFilter, columnName, baseDate.Date)
                    ApplyFilterEndDate(mFilter, columnName, baseDate.Date.Add(OneDayAsTimespan))
                    Return True
                ElseIf filterValue.Equals("Yesterday", StringComparison.CurrentCultureIgnoreCase) Then
                    ApplyFilterStartDate(mFilter, columnName, baseDate.Date.Subtract(OneDayAsTimespan))
                    ApplyFilterEndDate(mFilter, columnName, baseDate.Date)
                    Return True
                Else
                    Dim m As Match = Reg_LastNUnits.Match(filterValue)
                    If m.Success Then
                        Dim quantity As Integer
                        Dim startDate As DateTime = Date.MinValue
                        If Integer.TryParse(m.Groups(1).Value, quantity) Then
                            Dim unit As String = m.Groups(2).Value.ToLowerInvariant
                            Select Case unit
                                Case "minute", "min"
                                    startDate = baseDate - TimeSpan.FromMinutes(quantity)
                                Case "hour", "hr", "h"
                                    startDate = baseDate - TimeSpan.FromHours(quantity)
                                Case "day", "d"
                                    startDate = baseDate.Date - TimeSpan.FromDays(quantity)
                                Case Else
                                    Debug.Print("Unrecognised unit: {0}", unit)
                            End Select
                        End If
                        If startDate <> Date.MinValue Then
                            ApplyFilterStartDate(mFilter, columnName, startDate)
                            Return True
                        End If
                    Else
                        Return CheckForDateMatch(mFilter, columnName, filterValue, Reg_DateMatch)
                    End If
                End If

            Case ColumnTypes.DateAndTimeOfFutureEvent
                If filterValue = "" OrElse filterValue.Equals("All", StringComparison.CurrentCultureIgnoreCase) Then
                    Return True
                ElseIf filterValue.Equals("Today", StringComparison.CurrentCultureIgnoreCase) Then
                    ApplyFilterStartDate(mFilter, columnName, baseDate.Date)
                    ApplyFilterEndDate(mFilter, columnName, baseDate.Date.Add(OneDayAsTimespan))
                    Return True
                ElseIf filterValue.Equals("Tomorrow", StringComparison.CurrentCultureIgnoreCase) Then
                    ApplyFilterStartDate(mFilter, columnName, baseDate.Date.Add(OneDayAsTimespan))
                    ApplyFilterEndDate(mFilter, columnName, baseDate.Date.Add(OneDayAsTimespan).Add(OneDayAsTimespan))
                    Return True
                Else
                    Dim m As Match = Reg_NextNUnits.Match(filterValue)
                    If m.Success Then
                        Dim quantity As Integer
                        Dim units As String = m.Groups(2).Value.ToLowerInvariant
                        Dim endDate As DateTime = Date.MinValue
                        If Integer.TryParse(m.Groups(1).Value, quantity) Then
                            Select Case units
                                Case "minute", "min"
                                    endDate = baseDate + TimeSpan.FromMinutes(quantity)
                                Case "hour", "hr", "h"
                                    endDate = baseDate + TimeSpan.FromHours(quantity)
                                Case "day", "d"
                                    endDate = baseDate.Date + TimeSpan.FromDays(quantity)
                                Case Else
                                    Debug.Print("Unrecognised unit: {0}", units)
                            End Select
                        End If
                        ApplyFilterStartDate(mFilter, columnName, baseDate)
                        ApplyFilterEndDate(mFilter, columnName, endDate)
                        Return True
                    Else
                        Return CheckForDateMatch(mFilter, columnName, filterValue, Reg_DateMatch)
                    End If
                End If
            Case ColumnTypes.IntegerColumn
                If filterValue.Equals("All", StringComparison.CurrentCultureIgnoreCase) OrElse filterValue = "" Then
                    Return True
                Else
                    Dim match As Match = Reg_IntValueMatch.Match(filterValue)
                    Dim found As Boolean = False
                    While match.Success
                        If match.Captures.Count > 0 Then
                            ProcessNumericFilter(mFilter, columnName, match.Groups(1).Value, match.Groups(2).Value)
                            found = True
                        End If
                        match = match.NextMatch()
                    End While
                    Return found
                End If

            Case ColumnTypes.ImageColumn
                'Not very tidy - we just assume this is the "icon" column
                Select Case filterValue
                    Case "Tick"
                        mFilter.AddStates(QueueItemState.Completed)

                    Case "Padlock"
                        mFilter.AddStates(QueueItemState.Locked)

                    Case "Person"
                        mFilter.AddStates(QueueItemState.Exceptioned)

                    Case "Ellipsis"
                        mFilter.AddStates(QueueItemState.Pending, QueueItemState.Deferred)

                    Case Else
                        mFilter.ClearStates()

                End Select
                Return True

            Case ColumnTypes.TimespanColumn
                If filterValue.Equals("All", StringComparison.CurrentCultureIgnoreCase) OrElse filterValue = "" Then
                    Return True
                Else
                    Dim match As Match = Reg_TimespanValueMatch.Match(filterValue)
                    Dim found As Boolean = False
                    ' multi-part filter terms cannot contain one (or more) expressions using equality
                    ' so set this to True when we match an expression using equality
                    Dim eqFound As Boolean = False

                    While match.Success
                        'if we process a new match and have already found equality, return false as this doesn't make sense
                        If eqFound Then
                            mFilter.MinWorkTime = Integer.MinValue
                            mFilter.MaxWorkTime = Integer.MaxValue
                            Return False
                        End If

                        If match.Captures.Count > 0 Then
                            Dim op As String = match.Groups(1).Value
                            If (op = "=" OrElse op = "") Then
                                If filterValueAfterRounding <> "" Then
                                    'we've already processed one part so doesn't make sense for this part to be equality
                                    mFilter.MinWorkTime = Integer.MinValue
                                    mFilter.MaxWorkTime = Integer.MaxValue
                                    Return False
                                Else
                                    eqFound = True
                                End If
                            End If

                            Dim val As String = match.Groups(2).Value
                            If ProcessTimespanFilter(mFilter, columnName, op, val) Then
                                found = True
                                'update the UI with the true filter value if any rounding has taken place
                                If filterValueAfterRounding = "" Then
                                    filterValueAfterRounding = op & val
                                Else
                                    filterValueAfterRounding = filterValueAfterRounding & ", " & op & val
                                End If
                            End If
                        End If
                        match = match.NextMatch()
                    End While
                    filterValue = filterValueAfterRounding
                    Return found
                End If

            Case Else
                Dim value As String = filterValue.Trim
                If value <> "All" Then
                    ApplyFilterTextValue(mFilter, columnName, value)
                End If
                Return True
        End Select
    End Function

    ''' <summary>
    ''' Checks to see if a date filter bound can be parsed from the supplied text.
    ''' Checks each part of the string separated by commas and returns False if any
    ''' one is invalid or cannot be parsed
    ''' </summary>
    ''' <param name="filter">The filter to be updated, if a value can be parsed.</param>
    ''' <param name="columnName">The column of interest.</param>
    ''' <param name="userString">The string value supplied by the user.</param>
    ''' <param name="singleDateMatcher">A regex object used to match a date expression.</param>
    Private Function CheckForDateMatch(ByVal filter As WorkQueueFilter, ByVal columnName As String, ByVal userString As String, ByVal singleDateMatcher As Regex) As Boolean
        Dim values As String() = userString.Split(","c)
        Dim filterItems As New List(Of Match)

        For Each val As String In values
            If val.Trim = "" Then Continue For
            Dim singleDateMatch As Match = singleDateMatcher.Match(val)
            Dim dt As DateTime 'check that the date is actually a valid date
            If singleDateMatch.Success AndAlso singleDateMatch.Groups.Count > 2 AndAlso
                DateTime.TryParse(singleDateMatch.Groups(3).Value, dt) AndAlso IsDate(dt) _
                Then
                'this part of the input is valid - keep the match to filter on if the rest of the string is valid
                filterItems.Add(singleDateMatch)
            Else
                'we only validate the input if all parts of the string are valid
                'return False immediately without doing any filtering
                Return False
            End If
        Next

        If filterItems.Count = 0 Then
            Return False
        Else
            'Now the whole string has been checked, we can apply the filters
            For Each m As Match In filterItems
                ProcessDateFilter(filter, columnName, m.Groups(1).Value, m.Groups(3).Value)
            Next

            Return True
        End If
    End Function


    ''' <summary>
    ''' Updates the supplied filter with the appropriate datetime values
    ''' </summary>
    ''' <param name="filter">The filter to be updated.</param>
    ''' <param name="columnName">The name of the column of interest.</param>
    ''' <param name="op">The operator limiting the range of this column.</param>
    ''' <param name="value">The bounding value for this column, as a datetime.</param>
    Private Sub ProcessDateFilter(ByVal filter As WorkQueueFilter, ByVal columnName As String, ByVal op As String, ByVal value As String)
        Dim parsedDate As DateTime = Nothing
        ' We need to keep the parsed (local) date so that we can check if the user
        ' entered a date without a time element.
        If DateTime.TryParse(value, Nothing, DateTimeStyles.AssumeLocal, parsedDate) Then

            ' But the actual filtering is done with the UTC date, so get that too
            Dim realDate As DateTime = parsedDate.ToUniversalTime()

            Select Case op
                Case ">", ">="
                    ApplyFilterStartDate(filter, columnName, realDate)
                Case "<"
                    ApplyFilterEndDate(filter, columnName, realDate)
                Case "<="
                    ' "<= {date}" implies all items which occurred on that date
                    ' also, so make sure we extend the end date to cover that date
                    If parsedDate.TimeOfDay = TimeSpan.Zero Then
                        ApplyFilterEndDate(filter, columnName,
                         realDate + TimeSpan.FromDays(1))
                    Else ' "<= {datetime}" - include the minute specified by the user
                        ApplyFilterEndDate(filter, columnName,
                         realDate + TimeSpan.FromMinutes(1))
                    End If
                Case "", "="
                    If parsedDate.TimeOfDay = TimeSpan.Zero Then
                        'a literal date. Include everything from this day
                        ApplyFilterStartDate(filter, columnName, realDate)
                        ApplyFilterEndDate(filter, columnName,
                         realDate + TimeSpan.FromDays(1))
                    Else
                        ' match an "exact moment in time"
                        ' We seem to operate on minute-granularity with this filter,
                        ' so we must capture everything which occurred in the
                        ' specified minute
                        ApplyFilterStartDate(filter, columnName, realDate)
                        ApplyFilterEndDate(filter, columnName,
                         realDate + TimeSpan.FromMinutes(1))
                    End If
            End Select
        End If
    End Sub

    ''' <summary>
    ''' Updates the supplied filter with the appropriate numerical values
    ''' </summary>
    ''' <param name="filter">The filter to be updated.</param>
    ''' <param name="columnName">The name of the column of interest.</param>
    ''' <param name="op">The operator limiting the range of this column.</param>
    ''' <param name="value">The bounding value for this column, as a number.</param>
    Private Sub ProcessNumericFilter(ByVal filter As WorkQueueFilter, ByVal columnName As String, ByVal op As String, ByVal value As String)
        Dim realValue As Integer
        If Integer.TryParse(value, realValue) Then
            Select Case op
                Case ""
                    ApplyFilterMinNumericValue(filter, columnName, realValue)
                    ApplyFilterMaxNumericValue(filter, columnName, realValue)
                Case "<="
                    ApplyFilterMaxNumericValue(filter, columnName, realValue)
                Case ">="
                    ApplyFilterMinNumericValue(filter, columnName, realValue)
                Case "<"
                    ApplyFilterMaxNumericValue(filter, columnName, realValue - 1)
                Case ">"
                    ApplyFilterMinNumericValue(filter, columnName, realValue + 1)
            End Select
        End If
    End Sub

    ''' <summary>
    ''' Updates the supplied filter with the appropriate timespan values, rounding up 
    ''' or down as appropriate to the operator and value entered
    ''' </summary>
    ''' <param name="filter">The filter to be updated.</param>
    ''' <param name="columnName">The name of the column of interest.</param>
    ''' <param name="op">The operator limiting the range of this column.</param>
    ''' <param name="value">The bounding value for this column, as a string.</param>
    ''' <returns>False to indicate that the value cannot be parsed</returns>
    Private Function ProcessTimespanFilter(ByVal filter As WorkQueueFilter,
                                           ByVal columnName As String,
                                           ByRef op As String, ByRef value As String) _
                                           As Boolean

        'Expand to something the parser will accept, where necessary
        Static Reg_SecsAndMilliSecs As New Regex("^\d+\.\d+$", RegexOptions.Compiled)
        Static Reg_MinsAndSecsAndMilliSecs As New Regex("^\d+:\d+\.\d+$", RegexOptions.Compiled)
        Static Reg_MinsAndSecs As New Regex("^\d+:\d+$", RegexOptions.Compiled)
        Static Reg_Seconds As New Regex("^\d+$", RegexOptions.Compiled)

        Select Case True
            Case Reg_SecsAndMilliSecs.IsMatch(value) OrElse
                 Reg_Seconds.IsMatch(value)
                value = "00:00:" & value
            Case Reg_MinsAndSecsAndMilliSecs.IsMatch(value) OrElse
                 Reg_MinsAndSecs.IsMatch(value)
                value = "00:" & value
        End Select

        Dim realValue As TimeSpan
        Dim canParse As Boolean
        If TimeSpan.TryParse(value, realValue) Then
            value = realValue.ToString
            ApplyTimeSpanRounding(filter, columnName, op, value, realValue)
            canParse = True
        Else
            ' let the user know when an invalid timespan is entered
            canParse = False
        End If
        ' format the string in accordance with the way values are displayed in the 
        ' column before it is passed back to make any update to the UI
        If value.StartsWith("00:") Then value = value.Remove(0, 3)
        If value.EndsWith(".000") Then value = value.Remove(value.IndexOf(".000", 0))
        Return canParse

    End Function

    ''' <summary>
    ''' Method to apply the correct maximum and minimum filter values, depending on 
    ''' the filter string entered. Will also apply rounding to a timespan filter 
    ''' value where filter value contains milliseconds, rounding up or 
    ''' down as appropriate for the operator and value supplied. Where rounding does 
    ''' take place, the operator and value are updated (passed ByRef) so the true 
    ''' values being filtered on can be displyed in the UI.
    ''' </summary>
    ''' <param name="filter">The filter to be updated</param>
    ''' <param name="columnName">The name of the column of interest.</param>
    ''' <param name="op">The operator limiting the range of this column</param>
    ''' <param name="value">The bounding value for this column as a String</param>
    ''' <param name="realValue">The bounding value parsed into a Timespan</param>
    Private Sub ApplyTimeSpanRounding(ByVal filter As WorkQueueFilter,
                                           ByVal columnName As String,
                                           ByRef op As String, ByRef value As String,
                                           ByVal realValue As TimeSpan)
        Dim roundedRealValue As TimeSpan
        Select Case op
            Case "", "="
                ' Any millisecs will be rounded up or down correctly by Cint
                roundedRealValue = New TimeSpan(0, 0, CInt(realValue.TotalSeconds))
                ApplyFilterMinTimespanValue(filter, columnName, roundedRealValue)
                ApplyFilterMaxTimespanValue(filter, columnName, roundedRealValue)
                If realValue.Milliseconds <> 0 Then _
                    value = roundedRealValue.ToString
            Case "<="
                ' If there are millisecs, round down to the nearest second and 
                ' this is the max. If no millisecs, take the input as the max
                ' (both equivalent to just using the integer part as max)
                roundedRealValue = New TimeSpan(realValue.Days, realValue.Hours,
                                                realValue.Minutes, realValue.Seconds,
                                                0)
                ApplyFilterMaxTimespanValue(filter, columnName, roundedRealValue)
                If realValue.Milliseconds <> 0 Then _
                    value = roundedRealValue.ToString

            Case "<"
                ' If there are millisecs, round the number down to nearest second 
                ' and this is the max (equality is allowed after rounding)
                If realValue.Milliseconds <> 0 Then
                    Dim roundedSecs As Integer =
                         CInt(Math.Floor(realValue.TotalSeconds))
                    roundedRealValue = New TimeSpan(0, 0, roundedSecs)
                    value = roundedRealValue.ToString
                    op = "<="
                Else
                    ' if no millisecs, subtract 1 second to preserve the strictness 
                    ' of the inequality
                    roundedRealValue = New TimeSpan(realValue.Days, realValue.Hours,
                                                    realValue.Minutes,
                                                    realValue.Seconds - 1,
                                                    0)
                End If
                ApplyFilterMaxTimespanValue(filter, columnName, roundedRealValue)

            Case ">="
                ' If there are millisecs, round up to the nearest second first
                ' other wise use the input as min (equivalent to ceiling function)
                Dim roundedSecs As Integer =
                     CInt(Math.Ceiling(realValue.TotalSeconds))
                roundedRealValue = New TimeSpan(0, 0, roundedSecs)
                ApplyFilterMinTimespanValue(filter, columnName, roundedRealValue)
                If realValue.Milliseconds <> 0 Then _
                    value = roundedRealValue.ToString

            Case ">"
                If realValue.Milliseconds <> 0 Then
                    ' If there are millisecs, round up to the nearest second first 
                    ' and use this as min (equality now allowed)
                    Dim roundedSecs As Integer =
                        CInt(Math.Ceiling(realValue.TotalSeconds))
                    roundedRealValue = New TimeSpan(0, 0, roundedSecs)
                    value = roundedRealValue.ToString
                    op = ">="
                Else
                    ' If no decimal part, add one second to preserve the strictness
                    ' of the inequality
                    roundedRealValue = New TimeSpan(realValue.Days, realValue.Hours,
                                                    realValue.Minutes,
                                                    realValue.Seconds + 1,
                                                    0)
                End If
                ApplyFilterMinTimespanValue(filter, columnName, roundedRealValue)
        End Select

    End Sub

    ''' <summary>
    ''' Applies a start date filter to the specified column.
    ''' </summary>
    ''' <param name="filter">The filter to be updated.</param>
    ''' <param name="columnName">The name of the column of interest.</param>
    ''' <param name="startDate">The start date filter, in UTC time, to be applied
    ''' to this column.</param>
    Private Sub ApplyFilterStartDate(ByVal filter As WorkQueueFilter, ByVal columnName As String, ByVal startDate As DateTime)
        Select Case columnName
            Case "Created"
                filter.LoadedStartDate = startDate
            Case "Exception Date"
                filter.ExceptionStartDate = startDate
            Case "Last Updated"
                filter.LastUpdatedStartDate = startDate
            Case "Next Review"
                filter.NextReviewStartDate = startDate
            Case "Completed"
                filter.CompletedStartDate = startDate
        End Select
    End Sub

    ''' <summary>
    ''' Applies an end date filter to the specified column.
    ''' </summary>
    ''' <param name="filter">The filter to be updated.</param>
    ''' <param name="columnName">The name of the column of interest.</param>
    ''' <param name="endDate">The end date filter, in UTC time, to be applied
    ''' to this column.</param>
    Private Sub ApplyFilterEndDate(ByVal filter As WorkQueueFilter, ByVal columnName As String, ByVal endDate As DateTime)
        Select Case columnName
            Case "Created"
                filter.LoadedEndDate = endDate
            Case "Exception Date"
                filter.ExceptionEndDate = endDate
            Case "Last Updated"
                filter.LastUpdatedEndDate = endDate
            Case "Next Review"
                filter.NextReviewEndDate = endDate
            Case "Completed"
                filter.CompletedEndDate = endDate
        End Select
    End Sub

    ''' <summary>
    ''' Applies a mimimum numeric value filter to the specified column.
    ''' </summary>
    ''' <param name="filter">The filter to be updated.</param>
    ''' <param name="columnName">The name of the column of interest.</param>
    ''' <param name="minValue">The minimum value to be applied
    ''' to this column.</param>
    Private Sub ApplyFilterMinNumericValue(ByVal filter As WorkQueueFilter, ByVal columnName As String, ByVal minValue As Integer)
        Select Case columnName
            Case "Attempt" : filter.MinAttempt = minValue
            Case "Priority" : filter.MinPriority = minValue
        End Select
    End Sub

    ''' <summary>
    ''' Applies a maximum numeric value filter to the specified column.
    ''' </summary>
    ''' <param name="filter">The filter to be updated.</param>
    ''' <param name="columnName">The name of the column of interest.</param>
    ''' <param name="maxValue">The maximum value to be applied
    ''' to this column.</param>
    Private Sub ApplyFilterMaxNumericValue(ByVal filter As WorkQueueFilter, ByVal columnName As String, ByVal maxValue As Integer)
        Select Case columnName
            Case "Attempt" : filter.MaxAttempt = maxValue
            Case "Priority" : filter.MaxPriority = maxValue
        End Select
    End Sub

    ''' <summary>
    ''' Applies a text value filter to the specified column.
    ''' </summary>
    ''' <param name="filter">The filter to be updated.</param>
    ''' <param name="columnName">The name of the column of interest.</param>
    ''' <param name="value">The value to be applied to this column.</param>
    Private Sub ApplyFilterTextValue(ByVal filter As WorkQueueFilter, ByVal columnName As String, ByVal value As String)
        Select Case columnName
            Case "Item Key"
                filter.ItemKey = value
            Case "Status"
                filter.Status = value
            Case "Tags"
                filter.Tags = value
            Case "Resource"
                filter.Resource = value
        End Select
    End Sub

    ''' <summary>
    ''' Applies a mimimum timespan value filter to the specified column.
    ''' </summary>
    ''' <param name="filter">The filter to be updated.</param>
    ''' <param name="columnName">The name of the column of interest.</param>
    ''' <param name="minValue">The minimum value to be applied
    ''' to this column.</param>
    Private Sub ApplyFilterMinTimespanValue(ByVal filter As WorkQueueFilter, ByVal columnName As String, ByVal minValue As TimeSpan)
        Select Case columnName
            Case "Total Work Time"
                ' The mimimum may already have been set by a previous part of the filter value so check if we do really 
                ' need to reset the minimum
                Dim minSeconds = CInt(minValue.TotalSeconds)
                If minSeconds > filter.MinWorkTime Then
                    filter.MinWorkTime = minSeconds
                End If
        End Select
    End Sub

    ''' <summary>
    ''' Applies a maximum timespan value filter to the specified column.
    ''' </summary>
    ''' <param name="filter">The filter to be updated.</param>
    ''' <param name="columnName">The name of the column of interest.</param>
    ''' <param name="maxValue">The minimum value to be applied
    ''' to this column.</param>
    Private Sub ApplyFilterMaxTimespanValue(ByVal filter As WorkQueueFilter, ByVal columnName As String, ByVal maxValue As TimeSpan)
        Select Case columnName
            Case "Total Work Time"
                Dim maxSeconds = CInt(maxValue.TotalSeconds)
                If maxSeconds < filter.MaxWorkTime Then
                    filter.MaxWorkTime = maxSeconds
                End If
        End Select
    End Sub

    ''' <summary>
    ''' Gets the type of the column with the specified name.
    ''' </summary>
    ''' <param name="columnName">The name of the column of interest.</param>
    ''' <returns>Returns the column's type</returns>
    Public Shared Function GetColumnType(ByVal columnName As String) As ColumnTypes
        Select Case columnName
            Case "Created", "Last Updated"
                Return ColumnTypes.DateAndTimeOfPastEvent
            Case "Completed", "Exception Date"
                Return ColumnTypes.DateAndTimeOfPossibleEvent
            Case "Next Review"
                Return ColumnTypes.DateAndTimeOfFutureEvent
            Case "Position", "Attempt", "Priority"
                Return ColumnTypes.IntegerColumn
            Case "Icon"
                Return ColumnTypes.ImageColumn
            Case "Total Work Time"
                Return ColumnTypes.TimespanColumn
            Case Else
                Return ColumnTypes.Other
        End Select
    End Function


    ''' <summary>
    ''' The various column types
    ''' </summary>
    ''' <remarks>A column's type affects how it is filtered.</remarks>
    Public Enum ColumnTypes
        DateAndTimeOfPastEvent
        DateAndTimeOfFutureEvent
        DateAndTimeOfPossibleEvent
        DateAndTimeGeneral
        IntegerColumn
        TimespanColumn
        ImageColumn
        Other
    End Enum


End Class



''' <summary>
''' Represents the collective filter settings in all of the filter combo boxes.
''' </summary>
Public Class WorkQueueUIFilter
    ''' <summary>
    ''' The key of the image to be used in the "Icon" column
    ''' </summary>
    Public IconKey As String
    ''' <summary>
    ''' The text to be used in the "Item Key" column filter.
    ''' </summary>
    Public ItemKeyFilter As String
    ''' <summary>
    ''' The text to be used in the Priority column filter
    ''' </summary>
    ''' <remarks></remarks>
    Public PriorityFilter As String
    ''' <summary>
    ''' The text to be used in the "Status" column filter.
    ''' </summary>
    Public StatusFilter As String
    ''' <summary>
    '''  The text for the "Tags" column filter
    ''' </summary>
    Public TagsFilter As String
    ''' <summary>
    ''' The text to be used in the 'Resource' column filter.
    ''' </summary>
    Public ResourceFilter As String
    ''' <summary>
    ''' The text to be used in the "Attempt" column filter.
    ''' </summary>
    Public AttemptFilter As String
    ''' <summary>
    ''' The text to be used in the "Created" column filter.
    ''' </summary>
    Public CreatedFilter As String
    ''' <summary>
    ''' The text to be used in the "Last Updated" column filter.
    ''' </summary>
    Public LastUpdatedFilter As String
    ''' <summary>
    ''' The text to be used in the "Next Review" column filter.
    ''' </summary>
    Public NextReviewFilter As String
    ''' <summary>
    ''' The text to be used in the "Completed" column filter.
    ''' </summary>
    Public CompletedFilter As String
    ''' <summary>
    ''' The text to be used in the "Total Work Time" column filter.
    ''' </summary>
    Public TotalWorkTimeFilter As String
    ''' <summary>
    ''' The text to be used in the "Exception Date" column filter.
    ''' </summary>
    Public ExceptionDateFilter As String
    ''' <summary>
    ''' The text to be used in the "Exception Reason" column filter.
    ''' </summary>
    Public ExceptionReasonFilter As String

    ''' <summary>
    ''' The maximum number of rows to be returned in teh view.
    ''' </summary>
    Public MaxRows As Integer

    ''' <summary>
    ''' Creates a UI Filter corresponding to that stored in the database with
    ''' the specified name.
    ''' </summary>
    ''' <param name="FilterName">The name of the filter of interest.</param>
    Public Shared Function FromName(ByVal FilterName As String) As WorkQueueUIFilter
        Dim xml As String = Nothing, sErr As String = Nothing

        Try
            gSv.WorkQueueGetFilterXML(FilterName, xml)
        Catch ex As Exception
            Throw New InvalidOperationException("Failed to get filter XML from database - " & ex.Message)
        End Try
        Return FromXML(xml)
    End Function

    ''' <summary>
    ''' Sets the filter text from the given XML Element, if it has any child text
    ''' nodes
    ''' </summary>
    ''' <param name="el">The element containing the filter text.</param>
    ''' <param name="filterText">The place to set the filter text.</param>
    Private Shared Sub SetFromElement(ByVal el As XmlElement, ByRef filterText As String)
        If el.ChildNodes.Count > 0 Then filterText = el.ChildNodes(0).Value
    End Sub

    ''' <summary>
    ''' Deserialises supplied stream into a filter object.
    ''' </summary>
    ''' <param name="xml">The XML representation of the filter, as created by ToXML.
    ''' </param>
    ''' <returns>Returns an object corresponding to the serialised representation.</returns>
    ''' <remarks>This also supports a ridiculous and obsolete SOAP formatted version
    ''' which should never have been used, but was, and also a thing that called
    ''' itself the "Item ID" but was actually the key. Such filters can only have
    ''' been created by very early releases of the work queues functionality.</remarks>
    Private Shared Function FromXML(ByVal xml As String) As WorkQueueUIFilter
        Dim x As New ReadableXmlDocument(xml)
        Dim p As XmlElement
        If x.DocumentElement.Name = "SOAP-ENV:Envelope" Then
            p = CType(CType(x.DocumentElement.ChildNodes(0), XmlElement).ChildNodes(0), XmlElement)
        ElseIf x.DocumentElement.Name = "WorkQueueFilter" Then
            p = x.DocumentElement
        Else
            Throw New InvalidOperationException("Invalid work queue filter XML root " & x.DocumentElement.Name)
        End If
        Dim filter As New WorkQueueUIFilter()
        For Each c As XmlElement In p.ChildNodes
            Select Case c.Name
                Case "IconKey"
                    SetFromElement(c, filter.IconKey)
                Case "ItemIDFilter", "ItemKeyFilter"
                    'Note, ItemIDFilter is for backwards compatibility with an earlier confused
                    'implementation.
                    SetFromElement(c, filter.ItemKeyFilter)
                Case "StatusFilter"
                    SetFromElement(c, filter.StatusFilter)
                Case "TagsFilter"
                    SetFromElement(c, filter.TagsFilter)
                Case "ResourceFilter"
                    SetFromElement(c, filter.ResourceFilter)
                Case "AttemptsFilter", "AttemptFilter" ' #4560 : The former for back-compatibility
                    SetFromElement(c, filter.AttemptFilter)
                Case "CreatedFilter"
                    SetFromElement(c, filter.CreatedFilter)
                Case "LastUpdatedFilter"
                    SetFromElement(c, filter.LastUpdatedFilter)
                Case "NextReviewFilter"
                    SetFromElement(c, filter.NextReviewFilter)
                Case "CompletedFilter"
                    SetFromElement(c, filter.CompletedFilter)
                Case "TotalWorkTimeFilter"
                    SetFromElement(c, filter.TotalWorkTimeFilter)
                Case "ExceptionDateFilter"
                    SetFromElement(c, filter.ExceptionDateFilter)
                Case "ExceptionReasonFilter"
                    SetFromElement(c, filter.ExceptionReasonFilter)
                Case "MaxRows"
                    If c.ChildNodes.Count > 0 Then filter.MaxRows = Integer.Parse(c.ChildNodes(0).Value)
            End Select
        Next
        Return filter
    End Function

    ''' <summary>
    ''' Serialises this instance to xml.
    ''' </summary>
    Public Function ToXML() As String
        Dim x As New XmlDocument()
        Dim root As XmlElement = x.CreateElement("WorkQueueFilter")
        x.AppendChild(root)
        Dim e As XmlElement

        e = x.CreateElement("IconKey")
        e.AppendChild(x.CreateTextNode(IconKey))
        root.AppendChild(e)

        e = x.CreateElement("ItemKeyFilter")
        e.AppendChild(x.CreateTextNode(ItemKeyFilter))
        root.AppendChild(e)

        e = x.CreateElement("ResourceFilter")
        e.AppendChild(x.CreateTextNode(ResourceFilter))
        root.AppendChild(e)


        e = x.CreateElement("StatusFilter")
        e.AppendChild(x.CreateTextNode(StatusFilter))
        root.AppendChild(e)

        e = x.CreateElement("TagsFilter")
        e.AppendChild(x.CreateTextNode(TagsFilter))
        root.AppendChild(e)

        e = x.CreateElement("AttemptFilter")
        e.AppendChild(x.CreateTextNode(AttemptFilter))
        root.AppendChild(e)

        e = x.CreateElement("CreatedFilter")
        e.AppendChild(x.CreateTextNode(CreatedFilter))
        root.AppendChild(e)

        e = x.CreateElement("LastUpdatedFilter")
        e.AppendChild(x.CreateTextNode(LastUpdatedFilter))
        root.AppendChild(e)

        e = x.CreateElement("NextReviewFilter")
        e.AppendChild(x.CreateTextNode(NextReviewFilter))
        root.AppendChild(e)

        e = x.CreateElement("CompletedFilter")
        e.AppendChild(x.CreateTextNode(CompletedFilter))
        root.AppendChild(e)

        e = x.CreateElement("TotalWorkTimeFilter")
        e.AppendChild(x.CreateTextNode(TotalWorkTimeFilter))
        root.AppendChild(e)

        e = x.CreateElement("ExceptionDateFilter")
        e.AppendChild(x.CreateTextNode(ExceptionDateFilter))
        root.AppendChild(e)

        e = x.CreateElement("ExceptionReasonFilter")
        e.AppendChild(x.CreateTextNode(ExceptionReasonFilter))
        root.AppendChild(e)

        e = x.CreateElement("MaxRows")
        e.AppendChild(x.CreateTextNode(MaxRows.ToString()))
        root.AppendChild(e)

        Return x.OuterXml

    End Function

    ''' <summary>
    ''' Returns the equivalent database content filter.
    ''' </summary>
    Public Function ToContentFilter() As WorkQueueFilter
        Dim fp As New clsWorkQueueFilterBuilder
        fp.ApplyConstraint("Icon", If(Me.IconKey, ""))
        fp.ApplyConstraint("Item Key", Me.ItemKeyFilter)
        fp.ApplyConstraint("Status", Me.StatusFilter)
        fp.ApplyConstraint("Tags", Me.TagsFilter)
        fp.ApplyConstraint("Attempt", Me.AttemptFilter)
        fp.ApplyConstraint("Created", Me.CreatedFilter)
        fp.ApplyConstraint("Last Updated", Me.LastUpdatedFilter)
        fp.ApplyConstraint("Next Review", Me.NextReviewFilter)
        fp.ApplyConstraint("Completed", Me.CompletedFilter)
        fp.ApplyConstraint("TotalWorkItem", Me.TotalWorkTimeFilter)
        fp.ApplyConstraint("Exception Date", Me.ExceptionDateFilter)
        fp.ApplyConstraint("Exception Reason", Me.ExceptionReasonFilter)

        Dim f As WorkQueueFilter = fp.Filter
        f.MaxRows = Me.MaxRows

        Return f
    End Function
End Class
