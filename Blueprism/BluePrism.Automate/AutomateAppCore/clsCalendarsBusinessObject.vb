Imports BluePrism.Server.Domain.Models

Imports BluePrism.Scheduling
Imports BluePrism.Scheduling.Calendar

Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.My.Resources
Imports BluePrism.AutomateProcessCore.Stages
''' <summary>
''' Business object to expose the functionality of calendars to processes.
''' </summary>
Public Class clsCalendarsBusinessObject : Inherits clsInternalBusinessObject

    ' The store from which the calendars can be loaded
    Private mStore As DatabaseBackedScheduleStore

    ''' <summary>
    ''' The new constructor just creates the Internal Business Object Actions
    ''' </summary>
    ''' <param name="process">A reference to the process calling the object</param>
    ''' <param name="session">The session the object is running under</param>
    Public Sub New(ByVal process As clsProcess, ByVal session As clsSession)
        MyBase.New(process, session,
         "clsCalendarsBusinessObject",
         IboResources.clsCalendarsBusinessObject_Calendars,
         IboResources.clsCalendarsBusinessObject_ThisInternalBusinessObjectProvidesTheAbilityForProcessesToInteractWithWorkingDa,
         New GetWorkingDaysInRange(),
         New CountWorkingDaysInRange(),
         New IsWorkingDay(),
         New AddWorkingDays(),
         New IsWeekend(),
         New IsPublicHoliday(),
         New IsOtherHoliday(),
         New GetPublicHolidaysInRange(),
         New GetOtherHolidaysInRange()
        )
        ' Create a store and ensure it is checking the database for changes
        mStore = New DatabaseBackedScheduleStore(New InertScheduler(), gsv)

    End Sub

    ''' <summary>
    ''' Initialises this business object, telling the schedule store to begin
    ''' listening for changes on the database
    ''' </summary>
    ''' <returns>The result from the superclass's initialisation</returns>
    Public Overrides Function DoInit() As StageResult
        Return MyBase.DoInit()
    End Function

    ''' <summary>
    ''' Disposes of this business object, ensuring that the calendar store it uses
    ''' is disposed of too.
    ''' </summary>
    ''' <param name="disposing">True if being called explicitly, False if being
    ''' called implicitly from the finalizer</param>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            mStore.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    ''' <summary>
    ''' Disposes of this business object, ensuring that the store stops listening for
    ''' changes to the calendar on the database.
    ''' </summary>
    Public Overrides Sub DisposeTasks()
        MyBase.DisposeTasks()
        mStore.StopListeningForChanges()
    End Sub

    ''' <summary>
    ''' Gets the calendar with the given name from the current backing store.
    ''' </summary>
    ''' <param name="name">The name of the calendar required.</param>
    ''' <returns>The calendar with the given name from the store.</returns>
    ''' <exception cref="NoSuchElementException">If no calendar was found with
    ''' the given name.</exception>
    Private Function GetCalendar(ByVal name As String) As ScheduleCalendar
        Dim cal As ScheduleCalendar = mStore.GetCalendar(name)
        If cal IsNot Nothing Then Return cal
        ' Else... no calendar
        Throw New NoSuchElementException(IboResources.clsCalendarsBusinessObject_NoCalendarWithTheName0Found, name)
    End Function

    ''' <summary>
    ''' Gets the public holiday schema currently in use by this business object.
    ''' </summary>
    ''' <returns>The public holiday schema from the store held in this object.
    ''' </returns>
    Private Function GetPublicHolidaySchema() As PublicHolidaySchema
        Return mStore.GetSchema()
    End Function


    Public Overrides Function CheckLicense() As Boolean
        Return True
    End Function

    ''' <summary>
    ''' Interim class to handle the basics of calendar business object actions -
    ''' this handles the pre/post-conditions for all the actions (they are all query
    ''' actions - they have no effect), and simplifies the call 
    ''' </summary>
    Private MustInherit Class CalendarAction
        Inherits clsInternalBusinessObjectAction

        ''' <summary>
        ''' Class to hold the parameter names as constants.
        ''' </summary>
        Protected Class Params

            Public Shared CalendarName As String = NameOf(IboResources.clsCalendarsBusinessObject_Params_CalendarName)
            Public Shared FromDate As String = NameOf(IboResources.clsCalendarsBusinessObject_Params_FirstDate)
            Public Shared ToDate As String = NameOf(IboResources.clsCalendarsBusinessObject_Params_LastDate)
            Public Shared SingleDate As String = NameOf(IboResources.clsCalendarsBusinessObject_Params_Date)
            Public Shared ResultDate As String = NameOf(IboResources.clsCalendarsBusinessObject_Params_ResultDate)

            Public Shared IsWorkingDay As String = NameOf(IboResources.clsCalendarsBusinessObject_Params_IsWorkingDay)
            Public Shared IsWeekend As String = NameOf(IboResources.clsCalendarsBusinessObject_Params_IsWeekend)
            Public Shared IsPublicHoliday As String = NameOf(IboResources.clsCalendarsBusinessObject_Params_IsPublicHoliday)
            Public Shared IsOtherHoliday As String = NameOf(IboResources.clsCalendarsBusinessObject_Params_IsOtherHoliday)

            Public Shared DayCount As String = NameOf(IboResources.clsCalendarsBusinessObject_Params_Days)
            Public Shared Count As String = NameOf(IboResources.clsCalendarsBusinessObject_Params_Count)

            Public Shared WorkingDays As String = NameOf(IboResources.clsCalendarsBusinessObject_Params_WorkingDays)
            Public Shared PublicHolidays As String = NameOf(IboResources.clsCalendarsBusinessObject_Params_PublicHolidays)
            Public Shared OtherHolidays As String = NameOf(IboResources.clsCalendarsBusinessObject_Params_OtherHolidays)

            Public Shared PublicHolidayName As String = NameOf(IboResources.clsCalendarsBusinessObject_Params_Name)

        End Class
        Public Function _T(ByVal param As String) As String
            Return IboResources.ResourceManager.GetString(param, New Globalization.CultureInfo("en"))
        End Function

        ''' <summary>
        ''' Class to hold the field names as constants.
        ''' </summary>
        Protected Class Fields
            Public Const SingleDate As String = "Date"
            Public Const PublicHolidayName As String = "Name"
        End Class

        ''' <summary>
        ''' The calendar business object that this action belongs to.
        ''' </summary>
        Protected ReadOnly Property CalendarObject() As clsCalendarsBusinessObject
            Get
                Return DirectCast(Me.Parent, clsCalendarsBusinessObject)
            End Get
        End Property

        ''' <summary>
        ''' The endpoint for this action. By default, there is no change.
        ''' </summary>
        ''' <returns>"No Change"</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsCalendarsBusinessObject_CalendarAction_NoChange
        End Function

        ''' <summary>
        ''' The preconditions for this action.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetPreConditions() As Collection
            Return SingletonPreCondition(IboResources.clsCalendarsBusinessObject_CalendarAction_TheSpecifiedCalendarMustExistWithinSystemManager)
        End Function

        ''' <summary>
        ''' Gets the calendar with the given name from the current backing store.
        ''' </summary>
        ''' <param name="name">The name of the calendar required.</param>
        ''' <returns>The calendar with the given name from the store.</returns>
        ''' <exception cref="NoSuchElementException">If no calendar was found with
        ''' the given name.</exception>
        Protected Function GetCalendar(ByVal name As String) As ScheduleCalendar
            Return CalendarObject.GetCalendar(name)
        End Function

        ''' <summary>
        ''' Ensures the order of the two dates, raising an error if they are in the
        ''' wrong order.
        ''' </summary>
        ''' <param name="earliest">The earliest date to check</param>
        ''' <param name="latest">The latest date to check</param>
        ''' <exception cref="BluePrismException">If the <paramref name="earliest"/>
        ''' date is later than the <paramref name="latest"/> date.</exception>
        Protected Sub EnsureOrder(ByVal earliest As Date, ByVal latest As Date)
            EnsureOrder(earliest, latest, False)
        End Sub

        ''' <summary>
        ''' Ensures the order of the two date parameters. If the given
        ''' <paramref name="swapOnWrongOrder"/> argument is true, it sets the two ref
        ''' parameters such that, on exit of this method, <paramref name="earliest"/>
        ''' will be the earliest of the two parameters and <paramref name="latest"/>
        ''' will be the latest of them.
        ''' No change is made if the two dates have the same value.
        ''' If <paramref name="swapOnWrongOrder"/> is false, an error will be thrown
        ''' if the earliest date is found to be later than the latest date.
        ''' </summary>
        ''' <param name="earliest">The first date parameter - on exit of the method,
        ''' this will be the earliest of the two dates.</param>
        ''' <param name="latest">The second date parameter - on exit, this will be
        ''' the latest of the two dates.</param>
        ''' <exception cref="BluePrismException">If the <paramref name="earliest"/>
        ''' date is later than the <paramref name="latest"/> date and the
        ''' <paramref name="swapOnWrongOrder"/> argument was false.</exception>
        Protected Sub EnsureOrder(ByRef earliest As Date, ByRef latest As Date,
         ByVal swapOnWrongOrder As Boolean)
            ' If they are already in order, leave them alone
            If earliest <= latest Then Return

            ' Otherwise, they are the wrong way around
            ' If we're not swapping, error now.
            If Not swapOnWrongOrder Then Throw New BluePrismException(
             IboResources.clsCalendarsBusinessObject_CalendarAction_01DCannotBeLaterThan23D,
             Params.FromDate, earliest, Params.ToDate, latest)

            ' Otherwise swap them and return
            Dim temp As Date = earliest
            earliest = latest
            latest = temp
        End Sub

        ''' <summary>
        ''' Handles the processing of the calendar business object action.
        ''' This just delegates to the Perform() method.
        ''' </summary>
        ''' <param name="process">The process this object is operating on</param>
        ''' <param name="session">The session the action is begin called within
        ''' </param>
        ''' <param name="scopeStage">The stage that this action is occurring on, thus
        ''' providing scope if necessary.</param>
        ''' <param name="sErr">On unsuccessful exit, this will contain the error.
        ''' </param>
        ''' <returns>True to indicate success; False otherwise.</returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess, ByVal session As clsSession,
         ByVal scopeStage As clsProcessStage, ByRef sErr As String) As Boolean

            Try
                Dim cal As String = CStr(Inputs.GetValue(_T(Params.CalendarName)))
                If cal = "" Then Return SendError(sErr, IboResources.clsCalendarsBusinessObject_CalendarAction_NoCalendarNameSpecified)

                Perform(GetCalendar(cal), Inputs, Outputs)
                Return True
            Catch ex As Exception
                Return SendError(sErr, ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' Throws an exception indicating that an argument was missing.
        ''' </summary>
        ''' <param name="argName">The name of the parameter which had no argument
        ''' provided.</param>
        Protected Sub FireMissingArgument(ByVal argName As String)
            Throw New NoSuchElementException(
             IboResources.clsCalendarsBusinessObject_CalendarAction_TheParameter0IsRequiredAndWasNotProvided, argName)
        End Sub

        ''' <summary>
        ''' Gets all the working days in the given range.
        ''' </summary>
        ''' <param name="cal">The calendar from which to draw the working days.
        ''' </param>
        ''' <param name="fromDate">The date to start from</param>
        ''' <param name="toDate">The date to work up to</param>
        ''' <returns>A collection of all working days in the given range.</returns>
        Protected Function GetAllWorkingDays(ByVal cal As ScheduleCalendar,
         ByVal fromDate As Date, ByVal toDate As Date) As ICollection(Of Date)

            EnsureOrder(fromDate, toDate)

            Dim curr As Date = fromDate ' The working date in the loop
            Dim days As New List(Of Date)
            ' Go through each day and ask the calendar if it 'can run'
            ' (ie. is a working day)
            While curr <= toDate
                If cal.CanRun(curr) Then days.Add(curr)
                curr = curr.AddDays(1)
            End While
            Return days

        End Function

        ''' <summary>
        ''' Converts the given collection of dates into a Blue Prism Collection
        ''' containing a single date field with a name defined in
        ''' <see cref="Params.SingleDate"/>.
        ''' </summary>
        ''' <param name="dates">The collection of dates to convert</param>
        ''' <returns>The given dates in a Blue Prism collection.</returns>
        Protected Function ToBluePrismCollection(ByVal dates As ICollection(Of Date)) _
         As clsCollection
            Dim col As New clsCollection()
            col.AddField(Fields.SingleDate, DataType.date)
            For Each dt As Date In dates
                Dim row As New clsCollectionRow()
                row.Add(Fields.SingleDate, New clsProcessValue(DataType.date, dt))
                col.Add(row)
            Next
            Return col
        End Function

        ''' <summary>
        ''' Performs this action within the given session using the given inputs
        ''' and outputs.
        ''' </summary>
        ''' <param name="cal">The calendar on which the action is being called.
        ''' </param>
        ''' <param name="ins">The input arguments.</param>
        ''' <param name="outs">The output arguments.</param>
        Protected MustOverride Sub Perform(ByVal cal As ScheduleCalendar,
         ByVal ins As clsArgumentList, ByVal outs As clsArgumentList)

    End Class

    ''' <summary>
    ''' Abstract action class which provides extra helper functions for concrete
    ''' actions dealing with date ranges.
    ''' </summary>
    Private MustInherit Class CalendarRangeAction : Inherits CalendarAction

        ''' <summary>
        ''' Gets the date range from the given argument list, returning the from and
        ''' to dates in the supplied ByRef parameters.
        ''' The dates are guaranteed to be in the correct order - ie. fromDate will
        ''' be the earlier of the two dates, toDate will be the later.
        ''' </summary>
        ''' <param name="ins">The argument list to get the dates from.</param>
        ''' <param name="fromDate">The earliest of the two dates retrieved from the
        ''' list.</param>
        ''' <param name="toDate">The latest of the two dates retrieved from the list.
        ''' </param>
        ''' <exception cref="NoSuchElementException">If either of the parameters were
        ''' not found in the list - the names checked are defined in the constants :-
        ''' <see cref="Params.FromDate"/> and <see cref="Params.ToDate"/></exception>
        Protected Sub ExtractDateRange(ByVal ins As clsArgumentList,
         ByRef fromDate As Date, ByRef toDate As Date)

            Dim pvFrom As clsProcessValue = ins.GetValue(_T(Params.FromDate))
            If pvFrom Is Nothing OrElse pvFrom.IsNull Then FireMissingArgument(IboResources.ResourceManager.GetString(Params.FromDate))
            fromDate = pvFrom.GetDateValue()

            Dim pvTo As clsProcessValue = ins.GetValue(_T(Params.ToDate))
            If pvTo Is Nothing OrElse pvTo.IsNull Then FireMissingArgument(IboResources.ResourceManager.GetString(Params.ToDate))
            toDate = pvTo.GetDateValue()

            EnsureOrder(fromDate, toDate)

        End Sub

    End Class

    ''' <summary>
    ''' Abstract action class which provides extra helper functions for concrete
    ''' actions dealing with single date parameters.
    ''' </summary>
    Private MustInherit Class CalendarSingleDateAction : Inherits CalendarAction

        ''' <summary>
        ''' Extracts the single date parameter, assuming the name defined in the
        ''' constant '<see cref="Params.SingleDate"/>' and returns it, throwing
        ''' an exception if it is not present in the given arguments.
        ''' </summary>
        ''' <param name="ins">The arguments list with the single date parameter held
        ''' within it.</param>
        ''' <returns>A Date representing the data argument found in the argument list
        ''' under the name defined in <see cref="Params.SingleDate"/></returns>
        ''' <exception cref="NoSuchElementException">If no parameter with the
        ''' appropriate name was found, or if that argument was null.</exception>
        Protected Function ExtractSingleDate(ByVal ins As clsArgumentList) As Date
            Dim pv As clsProcessValue = ins.GetValue(_T(Params.SingleDate))
            If pv Is Nothing OrElse pv.IsNull Then FireMissingArgument(IboResources.ResourceManager.GetString(Params.SingleDate))
            Return pv.GetDateValue()
        End Function

    End Class

    ''' <summary>
    ''' Action to get the working days within a specific range.
    ''' </summary>
    Private Class GetWorkingDaysInRange : Inherits CalendarRangeAction

        ''' <summary>
        ''' Creates a new action.
        ''' </summary>
        Public Sub New()
            SetName(NameOf(IboResources.clsCalendarsBusinessObject_Action_GetWorkingDaysInRange))
            SetNarrative(IboResources.clsCalendarsBusinessObject_GetWorkingDaysInRange_GetsTheWorkingDaysAsConfiguredOnTheSpecifiedCalendarStartingAndEndingOnTheSpeci)

            AddParameter(Params.CalendarName, DataType.text, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_GetWorkingDaysInRange_TheNameOfTheCalendarDefinedInSystemManager)
            AddParameter(Params.FromDate, DataType.date, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_GetWorkingDaysInRange_TheFirstDateToConsiderWorkingDaysFrom)
            AddParameter(Params.ToDate, DataType.date, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_GetWorkingDaysInRange_TheLastDateToConsiderWorkingDaysUntil)

            Dim info As New clsCollectionInfo()
            info.AddField(Fields.SingleDate, DataType.date)
            info.GetField(Fields.SingleDate).Description =
             IboResources.clsCalendarsBusinessObject_GetWorkingDaysInRange_AWorkingDateAsDefinedByTheSpecifiedCalendarWhichFallsFromFirstDateUpToAndInclud
            info.GetField(Fields.SingleDate).DisplayName = IboResources.clsCalendarsBusinessObject_Params_Date

            AddParameter(Params.WorkingDays, ParamDirection.Out, info,
             IboResources.clsCalendarsBusinessObject_GetWorkingDaysInRange_TheWorkingDaysWhichFallBetweenTheSpecifiedDatesInclusive)

        End Sub

        ''' <summary>
        ''' Performs this action within the given session using the given inputs
        ''' and outputs.
        ''' </summary>
        ''' <param name="cal">The calendar on which the action is being called.</param>
        ''' <param name="ins">The input arguments.</param>
        ''' <param name="outs">The output arguments.</param>
        Protected Overrides Sub Perform(ByVal cal As ScheduleCalendar,
         ByVal ins As clsArgumentList, ByVal outs As clsArgumentList)

            Dim fromDate As Date, toDate As Date
            ExtractDateRange(ins, fromDate, toDate)

            outs.SetValue(_T(Params.WorkingDays), New clsProcessValue(
             ToBluePrismCollection(GetAllWorkingDays(cal, fromDate, toDate))))
        End Sub

    End Class

    ''' <summary>
    ''' Action to count the number of working days within a specific range.
    ''' </summary>
    Private Class CountWorkingDaysInRange : Inherits CalendarRangeAction

        ''' <summary>
        ''' Creates a new action.
        ''' </summary>
        Public Sub New()
            SetName(NameOf(IboResources.clsCalendarsBusinessObject_Action_CountWorkingDaysInRange))
            SetNarrative(IboResources.clsCalendarsBusinessObject_CountWorkingDaysInRange_CountsTheNumberOfWorkingDaysFoundWithinTheGivenRangeInclusive)

            AddParameter(Params.CalendarName, DataType.text, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_CountWorkingDaysInRange_TheNameOfTheCalendarDefinedInSystemManager)
            AddParameter(Params.FromDate, DataType.date, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_CountWorkingDaysInRange_TheFirstDateToConsiderWorkingDaysFrom)
            AddParameter(Params.ToDate, DataType.date, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_CountWorkingDaysInRange_TheLastDateToConsiderWorkingDaysUntil)

            AddParameter(Params.DayCount, DataType.number, ParamDirection.Out,
             IboResources.clsCalendarsBusinessObject_CountWorkingDaysInRange_TheNumberOfWorkingDaysFoundWithinTheSpecifiedRangeInclusive)

        End Sub

        ''' <summary>
        ''' Performs this action within the given session using the given inputs
        ''' and outputs.
        ''' </summary>
        ''' <param name="cal">The calendar on which the action is being called.</param>
        ''' <param name="ins">The input arguments.</param>
        ''' <param name="outs">The output arguments.</param>
        Protected Overrides Sub Perform(ByVal cal As ScheduleCalendar,
         ByVal ins As clsArgumentList, ByVal outs As clsArgumentList)

            Dim fromDate As Date, toDate As Date
            ExtractDateRange(ins, fromDate, toDate)

            outs.SetValue(_T(Params.DayCount),
             New clsProcessValue(GetAllWorkingDays(cal, fromDate, toDate).Count))
        End Sub

    End Class

    ''' <summary>
    ''' Action to check whether a specific day is a working day or not.
    ''' </summary>
    Private Class IsWorkingDay : Inherits CalendarSingleDateAction

        ''' <summary>
        ''' Creates a new action.
        ''' </summary>
        Public Sub New()
            SetName(NameOf(IboResources.clsCalendarsBusinessObject_Action_IsWorkingDay))
            SetNarrative(IboResources.clsCalendarsBusinessObject_IsWorkingDay_ChecksIfTheGivenDateIsAWorkingDayOrNotAccordingToTheSpecifiedCalendar)

            AddParameter(Params.CalendarName, DataType.text, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_IsWorkingDay_TheNameOfTheCalendarDefinedInSystemManager)
            AddParameter(Params.SingleDate, DataType.date, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_IsWorkingDay_TheDateToConsiderWhetherItIsAWorkingDayOrNot)

            AddParameter(Params.IsWorkingDay, DataType.flag, ParamDirection.Out,
             IboResources.clsCalendarsBusinessObject_IsWorkingDay_TrueToIndicateTheGivenDateWasAWorkingDayFalseOtherwise)

        End Sub

        ''' <summary>
        ''' Performs this action within the given session using the given inputs
        ''' and outputs.
        ''' </summary>
        ''' <param name="cal">The calendar on which the action is being called.</param>
        ''' <param name="ins">The input arguments.</param>
        ''' <param name="outs">The output arguments.</param>
        Protected Overrides Sub Perform(ByVal cal As ScheduleCalendar,
         ByVal ins As clsArgumentList, ByVal outs As clsArgumentList)
            ' If the calendar 'can run' on the given date, it is a working day
            outs.SetValue(_T(Params.IsWorkingDay),
             New clsProcessValue(cal.CanRun(ExtractSingleDate(ins))))
        End Sub

    End Class


    ''' <summary>
    ''' Action to add a number of working days to a specified date.
    ''' </summary>
    Private Class AddWorkingDays : Inherits CalendarSingleDateAction

        ''' <summary>
        ''' Creates a new action.
        ''' </summary>
        Public Sub New()
            SetName(NameOf(IboResources.clsCalendarsBusinessObject_Action_AddWorkingDays))
            SetNarrative(
             IboResources.clsCalendarsBusinessObject_AddWorkingDays_AddsTheSpecifiedNumberOfWorkingDaysToAStartDateReturningTheResultInOrderToGetTh)

            AddParameter(Params.CalendarName, DataType.text, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_AddWorkingDays_TheNameOfTheCalendarDefinedInSystemManager)
            AddParameter(Params.SingleDate, DataType.date, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_AddWorkingDays_TheDateToWhichTheNumberOfWorkingDaysShouldBeAdded)
            AddParameter(Params.DayCount, DataType.number, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_AddWorkingDays_TheNumberOfDaysToBeAddedNoteThatAValueOfZeroWillAlwaysReturnStartDateEvenIfItIs)

            AddParameter(Params.ResultDate, DataType.date, ParamDirection.Out,
             IboResources.clsCalendarsBusinessObject_AddWorkingDays_TheDateWithTheSpecifiedNumberOfDaysAddedToIt)

        End Sub

        ''' <summary>
        ''' Performs this action within the given session using the given inputs
        ''' and outputs.
        ''' </summary>
        ''' <param name="cal">The calendar on which the action is being called.</param>
        ''' <param name="ins">The input arguments.</param>
        ''' <param name="outs">The output arguments.</param>
        Protected Overrides Sub Perform(ByVal cal As ScheduleCalendar,
         ByVal ins As clsArgumentList, ByVal outs As clsArgumentList)

            Dim dt As Date = ExtractSingleDate(ins)

            Dim pvDays As clsProcessValue = ins.GetValue(_T(Params.DayCount))
            If pvDays Is Nothing OrElse pvDays.IsNull Then _
             FireMissingArgument(IboResources.ResourceManager.GetString(Params.DayCount))
            Dim days As Integer = CInt(pvDays)

            ' Simple check - a days value of zero always returns the same day, so
            ' just leave 'dt' as it is in that case
            If days <> 0 Then

                ' Get the day step we need to use to modify the date in the loop
                Dim dayStep As Integer = CInt(IIf(days > 0, 1, -1))
                ' Lose the sign from the days - that's safely in 'dayStep' now.
                days = Math.Abs(days)

                ' There's no clever way of doing this, really. This just brute forces
                ' its way through the working days of the calendar. It may be
                ' optimisable by transferring this functionality into the calendar
                ' class, but at the moment I'm not sure it's worth the effort.
                Dim workingDayCount As Integer = 0
                While workingDayCount < days
                    dt = dt.AddDays(dayStep)
                    If cal.CanRun(dt) Then workingDayCount += 1
                End While
            End If

            ' At this point 'dt' should be our date.
            outs.SetValue(_T(Params.ResultDate), New clsProcessValue(DataType.date, dt))

        End Sub

    End Class

    ''' <summary>
    ''' Action to test if a specific date is a weekend
    ''' </summary>
    Private Class IsWeekend : Inherits CalendarSingleDateAction

        ''' <summary>
        ''' Creates a new action.
        ''' </summary>
        Public Sub New()
            SetName(NameOf(IboResources.clsCalendarsBusinessObject_Action_IsWeekend))
            SetNarrative(
             IboResources.clsCalendarsBusinessObject_IsWeekend_ChecksIfTheGivenDateFallsOutsideTheWorkingWeekIeTheWeekendAsDefinedInTheSpecifi)

            AddParameter(Params.CalendarName, DataType.text, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_IsWeekend_TheNameOfTheCalendarDefinedInSystemManager)
            AddParameter(Params.SingleDate, DataType.date, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_IsWeekend_TheDateToCheck)

            AddParameter(Params.IsWeekend, DataType.flag, ParamDirection.Out,
             IboResources.clsCalendarsBusinessObject_IsWeekend_TrueToIndicateThatTheGivenDateFallsOutsideTheWorkingWeekDefinedInTheCalendarFal)

        End Sub

        ''' <summary>
        ''' Performs this action within the given session using the given inputs
        ''' and outputs.
        ''' </summary>
        ''' <param name="cal">The calendar on which the action is being called.</param>
        ''' <param name="ins">The input arguments.</param>
        ''' <param name="outs">The output arguments.</param>
        Protected Overrides Sub Perform(ByVal cal As ScheduleCalendar,
         ByVal ins As clsArgumentList, ByVal outs As clsArgumentList)
            ' Get the day of the week from the given date and see if it is set in the
            ' calendar's working week
            Dim isInWorkingWeek As Boolean =
             cal.WorkingWeek.Contains(ExtractSingleDate(ins).DayOfWeek)
            ' We actually want the opposite of that to say that it falls on a weekend
            outs.SetValue(_T(Params.IsWeekend), New clsProcessValue(Not isInWorkingWeek))
        End Sub

    End Class

    ''' <summary>
    ''' Action to test if a specific date is a public holiday
    ''' </summary>
    Private Class IsPublicHoliday : Inherits CalendarSingleDateAction

        ''' <summary>
        ''' Creates a new action.
        ''' </summary>
        Public Sub New()
            SetName(NameOf(IboResources.clsCalendarsBusinessObject_Action_IsPublicHoliday))
            SetNarrative(
             IboResources.clsCalendarsBusinessObject_IsPublicHoliday_ChecksIfTheGivenDateRepresentsAnEnabledPublicHolidayAsDefinedInTheSpecifiedCale)

            AddParameter(Params.CalendarName, DataType.text, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_IsPublicHoliday_TheNameOfTheCalendarDefinedInSystemManager)
            AddParameter(Params.SingleDate, DataType.date, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_IsPublicHoliday_TheDateToCheck)

            AddParameter(Params.IsPublicHoliday, DataType.flag, ParamDirection.Out,
             IboResources.clsCalendarsBusinessObject_IsPublicHoliday_TrueToIndicateThatTheGivenDateFallsOnAPublicHolidayFalseOtherwise)
            AddParameter(Params.PublicHolidayName, DataType.text, ParamDirection.Out,
             IboResources.clsCalendarsBusinessObject_IsPublicHoliday_IfDateIsAPublicHolidayThisContainsTheLabelOfThatPublicHolidayOtherwiseBlank)

        End Sub

        ''' <summary>
        ''' Performs this action within the given session using the given inputs
        ''' and outputs.
        ''' </summary>
        ''' <param name="cal">The calendar on which the action is being called.</param>
        ''' <param name="ins">The input arguments.</param>
        ''' <param name="outs">The output arguments.</param>
        Protected Overrides Sub Perform(ByVal cal As ScheduleCalendar,
         ByVal ins As clsArgumentList, ByVal outs As clsArgumentList)
            ' Find the public holiday if there is one.
            Dim ph As PublicHoliday = cal.FindPublicHoliday(ExtractSingleDate(ins))

            ' Set the flag appropriately
            outs.SetValue(_T(Params.IsPublicHoliday), New clsProcessValue(ph IsNot Nothing))

            ' If there is a public holiday, save the name.
            If ph IsNot Nothing Then
                outs.SetValue(_T(Params.PublicHolidayName),
                 New clsProcessValue(DataType.text, ph.Name))
            End If

        End Sub

    End Class

    ''' <summary>
    ''' Action to test if a specific date is an 'other holiday'
    ''' </summary>
    Private Class IsOtherHoliday : Inherits CalendarSingleDateAction

        ''' <summary>
        ''' Creates a new action.
        ''' </summary>
        Public Sub New()
            SetName(NameOf(IboResources.clsCalendarsBusinessObject_Action_IsOtherHoliday))
            SetNarrative(
             IboResources.clsCalendarsBusinessObject_IsOtherHoliday_ChecksIfTheGivenDateRepresentsADateDefinedAsAnOtherHolidayInTheSpecifiedCalendar)

            AddParameter(Params.CalendarName, DataType.text, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_IsOtherHoliday_TheNameOfTheCalendarDefinedInSystemManager)
            AddParameter(Params.SingleDate, DataType.date, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_IsOtherHoliday_TheDateToCheck)

            AddParameter(Params.IsOtherHoliday, DataType.flag, ParamDirection.Out,
             IboResources.clsCalendarsBusinessObject_IsOtherHoliday_TrueToIndicateThatTheGivenDateFallsOnAnotherHolidayFalseOtherwise)

        End Sub

        ''' <summary>
        ''' Performs this action within the given session using the given inputs
        ''' and outputs.
        ''' </summary>
        ''' <param name="cal">The calendar on which the action is being called.</param>
        ''' <param name="ins">The input arguments.</param>
        ''' <param name="outs">The output arguments.</param>
        Protected Overrides Sub Perform(ByVal cal As ScheduleCalendar,
         ByVal ins As clsArgumentList, ByVal outs As clsArgumentList)
            Dim isOtherHol As Boolean =
             cal.NonWorkingDays.Contains(ExtractSingleDate(ins))
            outs.SetValue(_T(Params.IsOtherHoliday), New clsProcessValue(isOtherHol))
        End Sub

    End Class

    ''' <summary>
    ''' Action to get all the enabled public holidays within a given range.
    ''' </summary>
    Private Class GetPublicHolidaysInRange : Inherits CalendarRangeAction

        ''' <summary>
        ''' Creates a new action.
        ''' </summary>
        Public Sub New()
            SetName(NameOf(IboResources.clsCalendarsBusinessObject_Action_GetPublicHolidaysInRange))
            SetNarrative(
             IboResources.clsCalendarsBusinessObject_GetPublicHolidaysInRange_GetsAllTheEnabledPublicHolidaysDefinedOnACalendarWhichFallWithinTheInclusiveDat)

            AddParameter(Params.CalendarName, DataType.text, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_GetPublicHolidaysInRange_TheNameOfTheCalendarDefinedInSystemManager)
            AddParameter(Params.FromDate, DataType.date, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_GetPublicHolidaysInRange_TheFirstDateToConsiderPublicHolidaysFrom)
            AddParameter(Params.ToDate, DataType.date, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_GetPublicHolidaysInRange_TheLastDateToConsiderPublicHolidaysUntil)

            Dim info As New clsCollectionInfo()
            info.AddField(Fields.SingleDate, DataType.date)
            info.GetField(Fields.SingleDate).Description =
             IboResources.clsCalendarsBusinessObject_GetPublicHolidaysInRange_TheDateOfThePublicHolidayEnabledOnTheCalendar
            info.GetField(Fields.SingleDate).DisplayName = IboResources.clsCalendarsBusinessObject_Params_Date

            info.AddField(Fields.PublicHolidayName, DataType.text)
            info.GetField(Fields.PublicHolidayName).Description = IboResources.clsCalendarsBusinessObject_GetPublicHolidaysInRange_TheNameOfThePublicHoliday
            info.GetField(Fields.PublicHolidayName).DisplayName = IboResources.clsEnvironmentLockingBusinessObject_Params_Name


            AddParameter(Params.PublicHolidays, ParamDirection.Out, info,
             IboResources.clsCalendarsBusinessObject_GetPublicHolidaysInRange_ThePublicHolidaysWhichFallBetweenTheSpecifiedDatesInclusive)

        End Sub

        ''' <summary>
        ''' Performs this action within the given session using the given inputs
        ''' and outputs.
        ''' </summary>
        ''' <param name="cal">The calendar on which the action is being called.</param>
        ''' <param name="ins">The input arguments.</param>
        ''' <param name="outs">The output arguments.</param>
        Protected Overrides Sub Perform(ByVal cal As ScheduleCalendar,
         ByVal ins As clsArgumentList, ByVal outs As clsArgumentList)
            Dim fromDate As Date, toDate As Date
            ExtractDateRange(ins, fromDate, toDate)

            ' Set up the output collection to use.
            Dim coll As New clsCollection()
            coll.AddField(Fields.SingleDate, DataType.date)
            coll.AddField(Fields.PublicHolidayName, DataType.text)

            If cal.PublicHolidayGroup IsNot Nothing Then
                ' Go through all the calendar's enabled public holidays, and get
                ' their occurrences between the two dates. Put into a sorted
                ' map so that they are automatically sorted into date order.
                Dim map As New SortedDictionary(Of Date, PublicHoliday)
                For Each hol As PublicHoliday In cal.PublicHolidays
                    For Each dt As Date In hol.GetOccurrences(fromDate, toDate)

                        ' If two holidays fall on the same day, keep the initial holiday in the
                        ' list. This can happen if, for example, Boxing day is static and lands
                        ' on a Monday but Christmas day lands on a Sunday and is shifted forward.
                        If Not map.ContainsKey(dt) Then map(dt) = hol
                    Next
                Next
                For Each pair As KeyValuePair(Of Date, PublicHoliday) In map
                    Dim row As New clsCollectionRow()
                    row.Add(Fields.SingleDate, New clsProcessValue(DataType.date, pair.Key))
                    row.Add(Fields.PublicHolidayName,
                     New clsProcessValue(DataType.text, pair.Value.Name))
                    coll.Add(row)
                Next
            End If

            outs.SetValue(_T(Params.PublicHolidays), New clsProcessValue(coll))

        End Sub

    End Class

    ''' <summary>
    ''' Action to get all the other holidays within a given range.
    ''' </summary>
    Private Class GetOtherHolidaysInRange : Inherits CalendarRangeAction

        ''' <summary>
        ''' Creates a new action.
        ''' </summary>
        Public Sub New()
            SetName(NameOf(IboResources.clsCalendarsBusinessObject_Action_GetOtherHolidaysInRange))
            SetNarrative(
             IboResources.clsCalendarsBusinessObject_GetOtherHolidaysInRange_GetsAllTheOtherHolidaysSpecifiedInACalendarWhichFallWithinTheInclusiveDateRange)

            AddParameter(Params.CalendarName, DataType.text, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_GetOtherHolidaysInRange_TheNameOfTheCalendarDefinedInSystemManager)
            AddParameter(Params.FromDate, DataType.date, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_GetOtherHolidaysInRange_TheFirstDateToConsiderOtherHolidaysFrom)
            AddParameter(Params.ToDate, DataType.date, ParamDirection.In,
             IboResources.clsCalendarsBusinessObject_GetOtherHolidaysInRange_TheLastDateToConsiderOtherHolidaysUntil)

            Dim info As New clsCollectionInfo()
            info.AddField(Fields.SingleDate, DataType.date)
            info.GetField(Fields.SingleDate).Description =
             IboResources.clsCalendarsBusinessObject_GetOtherHolidaysInRange_TheDateOfTheOtherHolidayDefinedOnTheCalendar
            info.GetField(Fields.SingleDate).DisplayName = IboResources.clsCalendarsBusinessObject_Params_Date


            AddParameter(Params.OtherHolidays, ParamDirection.Out, info,
             IboResources.clsCalendarsBusinessObject_GetOtherHolidaysInRange_TheOtherHolidaysWhichFallBetweenTheSpecifiedDatesInclusive)

        End Sub

        ''' <summary>
        ''' Performs this action within the given session using the given inputs
        ''' and outputs.
        ''' </summary>
        ''' <param name="cal">The calendar on which the action is being called.</param>
        ''' <param name="ins">The input arguments.</param>
        ''' <param name="outs">The output arguments.</param>
        Protected Overrides Sub Perform(ByVal cal As ScheduleCalendar,
         ByVal ins As clsArgumentList, ByVal outs As clsArgumentList)
            Dim fromDate As Date, toDate As Date
            ExtractDateRange(ins, fromDate, toDate)

            Dim dates As New List(Of Date)
            For Each dt As Date In cal.NonWorkingDays
                If dt >= fromDate AndAlso dt <= toDate Then dates.Add(dt)
            Next

            outs.SetValue(_T(Params.OtherHolidays),
             New clsProcessValue(ToBluePrismCollection(dates)))
        End Sub
    End Class
End Class
