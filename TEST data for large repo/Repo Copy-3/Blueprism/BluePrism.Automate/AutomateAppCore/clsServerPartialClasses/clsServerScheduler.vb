Imports System.Data.SqlClient
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.clsServer.Constants
Imports BluePrism.AutomateAppCore.DataMonitor
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Data
Imports BluePrism.Scheduling
Imports BluePrism.Scheduling.Calendar
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.Scheduler
Imports BluePrism.Scheduling.ScheduleData
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Server.Domain.Models.Extensions
Imports BluePrism.Utilities.Functional

' Partial class which separates the scheduler from the rest of the clsServer
' methods just in order to keep the file size down to a sane level and make it
' easier to actually find functions
Partial Public Class clsServer

    ''' <summary>
    ''' Increments the data version number for the scheduler. This version number
    ''' is monitored by interested parties to see if any of the scheduler data
    ''' has changed, so any alterations to timing or configuration of a schedule
    ''' should ensure that the data version number is updated.
    ''' </summary>
    ''' <param name="con">The connection over which the data version for the
    ''' scheduler should be incremented.</param>
    ''' <returns>The new version number for the scheduler.</returns>
    ''' <remarks>This is a simple shortcut method to ensure that the data name
    ''' isn't mistyped somewhere when incrementing the data version - it could
    ''' easily go unnoticed until a release.</remarks>
    Private Function IncrementSchedulerDataVersion(ByVal con As IDatabaseConnection) As Long
        Return IncrementDataVersion(con, DataNames.Scheduler)
    End Function

    ''' <summary>
    ''' Checks if the scheduler data version number has been updated from the
    ''' specified value, returning the new version number into the provided
    ''' ByRef parameter if it has changed.
    ''' </summary>
    ''' <param name="verno">The current version number held in the system, and
    ''' on exit of the method, the current version number on the database.
    ''' </param>
    ''' <returns>True if the version number on the database differed from that
    ''' provided; False otherwise.</returns>
    <SecuredMethod(False)>
    Public Function HasSchedulerDataUpdated(ByRef verno As Long) As Boolean Implements IServer.HasSchedulerDataUpdated
        CheckPermissions()
        Return HasDataUpdated(DataNames.Scheduler, verno)
    End Function

#Region "PublicHoliday/Calendar handling"

    ''' <summary>
    ''' Gets the public holiday schema from the database
    ''' </summary>
    ''' <returns>The PublicHolidaySchema representing the public holidays held
    ''' on the database.</returns>
    <SecuredMethod(True)>
    Public Function GetPublicHolidaySchema() As PublicHolidaySchema Implements IServer.GetPublicHolidaySchema
        CheckPermissions()
        Using con = GetConnection()

            Return New PublicHolidaySchema(con.ExecuteReturnDataTable(New SqlCommand(
             " select " &
             "   g.name as groupname," &
             "   h.id," &
             "   h.name as holidayname," &
             "   h.dd," &
             "   h.mm," &
             "   h.dayofweek," &
             "   h.nthofmonth," &
             "   h.eastersunday," &
             "   h.relativetoholiday," &
             "   h.relativedaydiff, " &
             "   h.excludesaturday, " &
             "   h.shiftdaytypeid, " &
             "   h.relativedayofweek " &
             " from BPAPublicHoliday h " &
             "   left join BPAPublicHolidayGroupMember m on m.publicholidayid = h.id " &
             "   left join BPAPublicHolidayGroup g on m.publicholidaygroupid = g.id"
            )))

        End Using

    End Function


    ''' <summary>
    ''' Gets the current calendar data from the database.
    ''' </summary>
    ''' <returns>
    ''' A Dictionary of calendar objects keyed by ID.
    ''' </returns>
    <SecuredMethod(True)>
    Public Function GetAllCalendars() As IDictionary(Of Integer, ScheduleCalendar) Implements IServer.GetAllCalendars

        CheckPermissions()
        Using conn As SqlConnection = GetRawConnection()

            Dim cmd As New SqlCommand()
            Dim ds As New DataSet()
            Dim da As SqlDataAdapter

            cmd.Connection = conn

            ' Calendar
            cmd.CommandText =
             " select " &
             "   c.id," &
             "   c.name as calendarname," &
             "   c.workingweek," &
             "   g.name as holidaygroupname " &
             " from BPACalendar c " &
             "   left join BPAPublicHolidayGroup g on c.publicholidaygroupid = g.id"
            Dim calTable As DataTable = ds.Tables.Add("Calendar")
            da = New SqlDataAdapter(cmd)
            da.Fill(calTable)

            ' PublicHolidayOverride
            cmd.CommandText = "select calendarid, publicholidayid from BPAPublicHolidayWorkingDay"
            Dim phoTable As DataTable = ds.Tables.Add("PublicHolidayOverride")
            da = New SqlDataAdapter(cmd)
            da.Fill(phoTable)

            'NonWorkingDay
            cmd.CommandText = "select calendarid, nonworkingday from BPANonWorkingDay"
            Dim nwdTable As DataTable = ds.Tables.Add("NonWorkingDay")
            da = New SqlDataAdapter(cmd)
            da.Fill(nwdTable)

            ' Define the relationships...
            ds.Relations.Add("calendar-publicholidayoverrides",
             calTable.Columns("id"), phoTable.Columns("calendarid"))
            ds.Relations.Add("calendar-nonworkingdays",
             calTable.Columns("id"), nwdTable.Columns("calendarid"))

            ' Populte the calendar objects server side so that dates are not
            ' converted on the client
            Return ScheduleCalendar.LoadCalendars(ds,
                                                  GetPublicHolidaySchema())

        End Using

    End Function

    ''' <summary>
    ''' Updates the auxiliary calendar records with the values in the given
    ''' object. This will ensure a clean slate by deleting any existing records
    ''' and then inserting new ones.
    ''' </summary>
    ''' <param name="con">The connection on which the updating should occur.
    ''' Note that if the connection currently has a transaction registered, that
    ''' will be used, but no transaction is created (or committed) within this
    ''' method.</param>
    ''' <param name="cal">The calendar whose auxiliary records should be updated.
    ''' </param>
    Private Sub UpdateAuxiliaryCalendarRecords(
     ByVal con As IDatabaseConnection,
     ByVal cal As ScheduleCalendar)

        ' Put all this into a buffer and just send it to the server en masse -
        ' just so it saves unnecessary round trips

        Dim sb As New StringBuilder(
         "delete from BPAPublicHolidayWorkingDay where calendarid=@calendarid; " &
         "delete from BPANonWorkingDay where calendarid=@calendarid; "
        )
        Dim cmd As New SqlCommand()
        cmd.Parameters.AddWithValue("@calendarid", cal.Id)

        If cal.PublicHolidayOverrides.Count > 0 Then

            sb.Append("insert into BPAPublicHolidayWorkingDay (calendarid, publicholidayid) ")
            Dim count As Integer = 0
            For Each hol As PublicHoliday In cal.PublicHolidayOverrides
                If count > 0 Then sb.Append(" union all ")
                count += 1
                sb.AppendFormat("select @calendarid, @publicholidayid{0}", count)
                cmd.Parameters.AddWithValue(String.Format("@publicholidayid{0}", count), hol.Id)
            Next
            sb.Append("; ")
        End If

        If cal.NonWorkingDays.Count > 0 Then
            sb.Append("insert into BPANonWorkingDay (calendarid, nonworkingday) ")
            Dim count As Integer = 0
            For Each day As DateTime In cal.NonWorkingDays
                If count > 0 Then sb.Append(" union all ")
                count += 1
                sb.AppendFormat("select @calendarid, @nonworkingday{0}", count)
                cmd.Parameters.AddWithValue(String.Format("@nonworkingday{0}", count), day)
            Next
            sb.Append("; ")
        End If

        cmd.CommandText = sb.ToString()
        con.Execute(cmd)

    End Sub

    ''' <summary>
    ''' Creates the given calendar on the database
    ''' </summary>
    ''' <param name="cal">The calendar to create</param>
    ''' <returns>The generated integer ID for the calendar</returns>
    <SecuredMethod(True)>
    Public Function CreateCalendar(ByVal cal As ScheduleCalendar) As Integer Implements IServer.CreateCalendar
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            Dim id As Integer = CreateCalendar(con, cal)
            con.CommitTransaction()
            Return id
        End Using
    End Function

    ''' <summary>
    ''' Creates the given calendar on the database
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="cal">The calendar to create</param>
    ''' <returns>The generated integer ID for the calendar</returns>
    Private Function CreateCalendar(ByVal con As IDatabaseConnection,
                                    ByVal cal As ScheduleCalendar) As Integer
        Return CreateCalendar(con, cal, True)
    End Function

    ''' <summary>
    ''' Creates the given calendar on the database
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="cal">The calendar to create</param>
    ''' <param name="incrementVersion">Controls whether version data that is monitored
    ''' to check for updates is incremented following the change. If this change is
    ''' part of a batch, then the version update may be deferred until all changes
    ''' in the batch have been made.</param>
    ''' <returns>The generated integer ID for the calendar</returns>
    Private Function CreateCalendar(ByVal con As IDatabaseConnection,
                                    ByVal cal As ScheduleCalendar,
                                    incrementVersion As Boolean) As Integer

        Dim cmd As New SqlCommand(
          " declare @groupid int; " &
          " set @groupid = (" &
          "   select g.id from BPAPublicHolidayGroup g where g.name = @phgroupname" &
          " ); " &
          " insert into BPACalendar (name, description, publicholidaygroupid, workingweek)" &
          " values (@name, @description, @groupid, @workingweek); " &
          "" &
          " select scope_identity();"
        )
        With cmd.Parameters
            .AddWithValue("@name", cal.Name)
            .AddWithValue("@description", cal.Description)
            Dim group As Object = cal.PublicHolidayGroup
            If group Is Nothing Then group = DBNull.Value
            .AddWithValue("@phgroupname", group)
            .AddWithValue("@workingweek", cal.WorkingWeek.ToInt())
        End With

        ' Check for duplicate names
        Try
            cal.Id = CInt(con.ExecuteReturnScalar(cmd))

        Catch ex As SqlException When ex.Number = DatabaseErrorCode.UniqueConstraintError
            Throw New AlreadyExistsException(
             My.Resources.clsServer_ACalendarWithTheName0AlreadyExists, cal.Name)

        End Try

        UpdateAuxiliaryCalendarRecords(con, cal)
        If incrementVersion Then
            IncrementSchedulerDataVersion(con)
        End If

        RecordCalendarEvent(con, CalendarEventCode.Created, cal)

        Return cal.Id

    End Function

    ''' <summary>
    ''' Updates the given calendar on the database
    ''' </summary>
    ''' <param name="cal">The calendar to update</param>
    <SecuredMethod(Permission.SystemManager.System.Calendars)>
    Public Sub UpdateCalendar(ByVal cal As ScheduleCalendar) Implements IServer.UpdateCalendar
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            UpdateCalendar(con, cal)
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Updates the given calendar on the database
    ''' </summary>
    ''' <param name="con">The connection to the database to use.</param>
    ''' <param name="cal">The calendar to update</param>
    Private Sub UpdateCalendar(con As IDatabaseConnection,
                               cal As ScheduleCalendar)
        UpdateCalendar(con, cal, True)
    End Sub
    ''' <summary>
    ''' Updates the given calendar on the database
    ''' </summary>
    ''' <param name="con">The connection to the database to use.</param>
    ''' <param name="cal">The calendar to update</param>
    ''' <param name="incrementVersion">Controls whether version data that is monitored
    ''' to check for updates is incremented following the change. If this change is
    ''' part of a batch, then the version update may be deferred until all changes
    ''' in the batch have been made.</param>
    Private Sub UpdateCalendar(con As IDatabaseConnection,
                               cal As ScheduleCalendar,
                               incrementVersion As Boolean)

        Dim cmd As New SqlCommand(
          " update BPACalendar set" &
          "   name=@name," &
          "   description=@description," &
          "   publicholidaygroupid = (" &
          "     select id from BPAPublicHolidayGroup " &
          "       where name=@publicholidaygroupname" &
          "   )," &
          "   workingweek=@workingweek " &
          " where id=@id"
        )

        With cmd.Parameters
            .AddWithValue("@id", cal.Id)
            .AddWithValue("@name", cal.Name)
            .AddWithValue("@description", cal.Description)
            .AddWithValue("@publicholidaygroupname",
             IIf(cal.PublicHolidayGroup Is Nothing, DBNull.Value, cal.PublicHolidayGroup))
            .AddWithValue("@workingweek", cal.WorkingWeek.ToInt())
        End With

        con.Execute(cmd)

        UpdateAuxiliaryCalendarRecords(con, cal)

        If incrementVersion Then
            IncrementSchedulerDataVersion(con)
        End If

        RecordCalendarEvent(con, CalendarEventCode.Modified, cal)

    End Sub

    ''' <summary>
    ''' Gets the calendar with the given name, or null if no such calendar
    ''' exists.
    ''' </summary>
    ''' <param name="con">The connection to the database.</param>
    ''' <param name="name">The name of the desired calendar</param>
    ''' <param name="schema">The public holiday schema to create the calendar with
    ''' </param>
    ''' <returns>The calendar with the given name or null if no such calendar exists.
    ''' </returns>
    Private Function GetCalendar(ByVal con As IDatabaseConnection,
     ByVal name As String, ByVal schema As PublicHolidaySchema) As ScheduleCalendar

        Dim cmd As New SqlCommand(
         " select c.id, c.name, c.description, ph.name as publicholidaygroupname, c.workingweek" &
         " from BPACalendar c" &
         "   left join BPAPublicHolidayGroup ph on c.publicholidaygroupid = ph.id " &
         " where c.name = @name"
        )
        cmd.Parameters.AddWithValue("@name", name)
        Using reader = con.ExecuteReturnDataReader(cmd)
            If Not reader.Read() Then Return Nothing

            Dim prov As New ReaderDataProvider(reader)
            Dim cal As New ScheduleCalendar(schema)
            cal.Id = prov.GetValue("id", 0)
            cal.Name = prov.GetString("name")
            cal.Description = prov.GetString("description")
            cal.PublicHolidayGroup = prov.GetString("publicholidaygroupname")
            cal.WorkingWeek = New DaySet(prov.GetValue("workingweek", 0))
            Return cal
        End Using

    End Function

    ''' <summary>
    ''' Deletes the given calendar from the database, along with any child
    ''' records (public holiday overrides and non-working days)
    ''' </summary>
    ''' <param name="cal">The calendar to delete, with the ID set to the ID
    ''' which needs to be deleted</param>
    ''' <exception cref="InvalidOperationException">If a record exists which
    ''' requires the specified calendar, and thus it cannot be deleted.
    ''' </exception>
    <SecuredMethod(Permission.SystemManager.System.Calendars)>
    Public Sub DeleteCalendar(ByVal cal As ScheduleCalendar) Implements IServer.DeleteCalendar
        CheckPermissions()
        Using con = GetConnection()
            ' Try and delete the calendar.
            ' If it fails with a FK error, it means that there is a trigger
            ' which relies on the calendar, and it cannot therefore be deleted.
            ' If it fails with anything else, rethrow the exception
            Try

                Dim cmd As New SqlCommand("delete from BPACalendar where id=@id")
                cmd.Parameters.AddWithValue("@id", cal.Id)
                con.Execute(cmd)
                IncrementSchedulerDataVersion(con)

                RecordCalendarEvent(con, CalendarEventCode.Deleted, cal)

            Catch ex As SqlClient.SqlException ' Check for FK problem

                If ex.Number = DatabaseErrorCode.ForeignKeyError Then
                    Dim sb As New StringBuilder()
                    sb.AppendFormat(
                     My.Resources.clsServer_TheCalendar0CannotBeDeletedBecauseItIsInUseInTheFollowingSchedules, cal.Name)

                    Dim cmd As New SqlCommand(
                     " select s.name " &
                     " from BPASchedule s " &
                     "   join BPAScheduleTrigger t on t.scheduleid = s.id " &
                     "   join BPACalendar c on t.CalendarId = c.id " &
                     " where c.id = @id")
                    cmd.Parameters.AddWithValue("@id", cal.Id)
                    Using reader = con.ExecuteReturnDataReader(cmd)
                        While reader.Read()
                            sb.Append(vbCrLf).Append("* ").Append(reader("name"))
                        End While
                    End Using
                    Throw New CalendarInUseException(sb.ToString())
                End If

                ' If we get here, then we found an error, but not an FK one..
                ' just rethrow it.
                Throw
            End Try
        End Using
    End Sub

#End Region

#Region "Schedules and Tasks"

#Region "Schedules"

    ''' <summary>
    ''' Checks the given schedule to see if it has been modified on the database
    ''' since it was last refreshed. If further changes have been made on the
    ''' database, the schedule is reloaded, otherwise it is left alone.
    ''' </summary>
    ''' <param name="schedule">The schedule to check to see if it has changed.
    ''' After this method returns, if the database has changed this will contain
    ''' the new version of the schedule from the database. If the schedule has
    ''' been deleted, this will contain null.</param>
    ''' <returns>True if the schedule on the database has changed since the
    ''' version passed in; False if it remains the same.</returns>
    <SecuredMethod(True)>
    Public Function SchedulerRefreshIfChanged(ByRef schedule As SessionRunnerSchedule) As Boolean Implements IServer.SchedulerRefreshIfChanged
        CheckPermissions()
        Dim scheduleData As ScheduleDatabaseData = Nothing

        Using con = GetConnection()
            Dim cmd As New SqlCommand(
             "select versionno from BPASchedule where id=@scheduleid")
            cmd.Parameters.AddWithValue("@scheduleid", schedule.Id)

            Dim ver As Integer = IfNull(con.ExecuteReturnScalar(cmd), -1)

            ' If null, then it's been deleted (ie. ID no longer represents a schedule)
            ' We indicate this by returning True (schedule has been updated) and setting
            ' the ref parameter to Nothing (schedule has been updated and is now nothing)
            If ver = -1 Then schedule = Nothing : Return True

            ' Otherwise we have a version number to check
            If schedule.RequiresRefresh(ver) Then
                scheduleData = SchedulerGetScheduleData(con, schedule.Id)
            End If
        End Using

        If Not scheduleData Is Nothing Then
            schedule = New SessionRunnerSchedule(Nothing, scheduleData)
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Gets all active schedules currently stored on the database.
    ''' </summary>
    ''' <returns>A collection of all schedule objects which are not retired
    ''' from the database.</returns>
    <SecuredMethod(False)>
    Public Function SchedulerGetActiveSchedules(ByRef versionNo As Long) _
     As ICollection(Of SessionRunnerSchedule) Implements IServer.SchedulerGetActiveSchedules
        CheckPermissions()
        Dim scheduleData As ICollection(Of ScheduleDatabaseData)

        Using con = GetConnection()
            ' Put this in a transaction so we're guaranteed to get the
            ' schedules which correspond to a specific version number.
            con.BeginTransaction()
            versionNo = GetDataVersion(con, "Scheduler")
            scheduleData = SchedulerGetSchedulesWithRetiredBit(con, False)

            ' Really, there should be no data changes here, so the implicit
            ' rollback in the 'End Using' would probably be fine, but in
            ' future, a data change may be introduced and it would fail
            ' without this commit here - in a very non-obvious way/place.
            con.CommitTransaction()
        End Using

        Return ConvertAndSortScheduleCollection(scheduleData)
    End Function

    ''' <summary>
    ''' Gets all retired schedules currently stored on the database.
    ''' </summary>
    ''' <returns>A collection of database-backed schedules which represent the
    ''' currently retired schedules.</returns>
    ''' <remarks></remarks>
    <SecuredMethod(False)>
    Public Function SchedulerGetRetiredSchedules() As ICollection(Of SessionRunnerSchedule) Implements IServer.SchedulerGetRetiredSchedules
        CheckPermissions()
        Dim scheduleData As ICollection(Of ScheduleDatabaseData)
        Using con = GetConnection()
            scheduleData = SchedulerGetSchedulesWithRetiredBit(con, True)
        End Using

        Return ConvertAndSortScheduleCollection(scheduleData)
    End Function

    Private Function ConvertAndSortScheduleCollection(scheduleData As ICollection(Of ScheduleDatabaseData)) As ICollection(Of SessionRunnerSchedule)
        Dim schedules = scheduleData.Select(Function(x) New SessionRunnerSchedule(Nothing, x)).ToList()
        schedules.Sort(Function(x, y) String.Compare(x.Name, y.Name, StringComparison.Ordinal))

        Return schedules
    End Function

    ''' <summary>
    ''' Gets the schedules with the retired bit set to the specified value.
    ''' </summary>
    ''' <param name="con">The connection from which the schedules should
    ''' be drawn.</param>
    ''' <param name="retired">True to get retired schedules; false to get
    ''' active schedules.</param>
    ''' <returns>A collection of schedules which represent either active
    ''' or retired schedules depending on the <paramref name="retired"/>
    ''' parameter</returns>
    Private Function SchedulerGetSchedulesWithRetiredBit(
     ByVal con As IDatabaseConnection,
     ByVal retired As Boolean) As ICollection(Of ScheduleDatabaseData)
        Dim ids As New List(Of Integer)
        Dim cmd As New SqlCommand("SELECT id FROM BPASchedule WHERE retired=@retired AND name IS NOT NULL")
        cmd.Parameters.AddWithValue("@retired", retired) ' IIf(retired, 1, 0))

        Using reader = con.ExecuteReturnDataReader(cmd)
            While reader.Read()
                ids.Add(reader.GetInt32(0))
            End While
        End Using
        Return GetSchedulesFromDatabase(con, ids)

    End Function

    ''' <summary>
    ''' Gets a summary of all schedules currently stored on the database.
    ''' </summary>
    ''' <param name="scheduleParameters"> The parameters defining filtering for schedules </param>
    ''' <returns>A collection of all schedule objects from the database.</returns>
    <SecuredMethod(False)>
    Public Function SchedulerGetScheduleSummaries(scheduleParameters As ScheduleParameters) As ICollection(Of ScheduleSummary) Implements IServer.SchedulerGetScheduleSummaries
        CheckPermissions()
        Using con = GetConnection()
            Return GetScheduleSummariesFromDatabase(con, scheduleParameters)
        End Using
    End Function

    Private Function GetScheduleSummariesFromDatabase(connection As IDatabaseConnection, scheduleParameters As ScheduleParameters) As List(Of ScheduleSummary)
        Using command = mDatabaseCommandFactory("")
            Dim whereClauses = scheduleParameters.GetSqlWhereClauses(command)
            command.CommandText = $"select top {scheduleParameters.ItemsPerPage} s.id,
           s.name,
           s.description,
           s.initialtaskid,
           s.retired,
           (select count(id) from BPATask t where t.scheduleid = s.id) as taskscount,
           st.unittype,
           st.period,
           st.startpoint,
           st.endpoint,
           st.dayset,
           st.nthofmonth,
           st.startdate,
           st.enddate,
           st.timezoneid,
           st.utcoffset,
           c.id as calendarid,
           c.name as calendarname
           from BPASchedule s
           inner join BPAScheduleTrigger st on st.scheduleid = s.id
           left join BPACalendar c on st.calendarid = c.id
           where st.usertrigger = 1 and deletedname is null {String.Join("", whereClauses.Select(Function(x) $" and {x.SqlText}").ToArray())}
           order by s.name asc"

            whereClauses.ForEach(Sub(x) x.Parameters.ForEach(Sub(param) command.Parameters.Add(param)).Evaluate()).Evaluate()

            Dim scheduleSummaries As New List(Of ScheduleSummary)

            Using reader = connection.ExecuteReturnDataReader(command)
                Dim readerDataProvider As New ReaderDataProvider(reader)
                While reader.Read() : scheduleSummaries.Add(New ScheduleSummary(readerDataProvider)) : End While
            End Using

            Return scheduleSummaries
        End Using
    End Function

    Private Function GetSchedulesFromDatabase(connection As IDatabaseConnection, ids As ICollection(Of Integer)) As List(Of ScheduleDatabaseData)
        Dim schedules = New List(Of ScheduleDatabaseData)

        For Each id As Integer In ids
            Dim schedule = SchedulerGetScheduleData(connection, id)
            schedules.Add(schedule)
        Next

        Return schedules
    End Function

    <SecuredMethod(False)>
    Public Function SchedulerGetScheduleSummary(scheduleId As Integer) As ScheduleSummary Implements IServer.SchedulerGetScheduleSummary
        CheckPermissions()
        Using con = GetConnection()
            Return GetScheduleSummary(con, scheduleId)
        End Using
    End Function

    Private Function GetScheduleSummary(connection As IDatabaseConnection, scheduleId As Integer) As ScheduleSummary
        Using command = mDatabaseCommandFactory($"
            select s.id,
            s.name,
            s.description,
            s.initialtaskid,
            s.retired,
            (select count(id) from BPATask t where t.scheduleid = s.id) as taskscount,
            st.unittype,
            st.period,
            st.startpoint,
            st.endpoint,
            st.dayset,
            st.nthofmonth,
            st.startdate,
            st.enddate,
            st.timezoneid,
            st.utcoffset,
            c.id as calendarid,
            c.name as calendarname
            from BPASchedule s
            inner join BPAScheduleTrigger st on st.scheduleid = s.id
            left join BPACalendar c on st.calendarid = c.id
            where s.id = @scheduleId and st.usertrigger = 1 and deletedname is null")

            command.AddParameter("@scheduleId", scheduleId)

            Using reader = connection.ExecuteReturnDataReader(command)
                If reader.Read()
                    Return New ScheduleSummary(New ReaderDataProvider(reader))
                Else
                    Throw New NoSuchScheduleException(scheduleId)
                End If
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Gets the schedule represented by the given ID.
    ''' </summary>
    ''' <param name="id">The ID of the schedule which is required.</param>
    ''' <returns>The fully populated schedule which corresponds to the given ID,
    ''' or Nothing if no such schedule exists.</returns>
    <SecuredMethod(False)>
    Public Function SchedulerGetSchedule(ByVal id As Integer) As ISchedule Implements IServer.SchedulerGetSchedule
        CheckPermissions()
        Dim scheduleData As ScheduleDatabaseData

        Using con = GetConnection()
            scheduleData = SchedulerGetScheduleData(con, id)
        End Using

        Return New SessionRunnerSchedule(Nothing, scheduleData)
    End Function

    Private Function SchedulerGetSchedule(ByVal con As IDatabaseConnection, ByVal id As Integer) As SessionRunnerSchedule
        CheckPermissions()
        Return New SessionRunnerSchedule(Nothing, SchedulerGetScheduleData(con, id))
    End Function

    ''' <summary>
    ''' Gets the schedule with the given name from the database.
    ''' Since schedule's names must be unique, there should only be at most one
    ''' schedule with the given name on the database.
    ''' </summary>
    ''' <param name="name">The name of the schedule required.</param>
    ''' <returns>The schedule with the given name or Nothing if no schedule with
    ''' the given name was found.</returns>
    <SecuredMethod(False)>
    Public Function SchedulerGetSchedule(ByVal name As String) As ISchedule Implements IServer.SchedulerGetSchedule
        CheckPermissions()
        Dim scheduleData As ScheduleDatabaseData

        Using con = GetConnection()
            scheduleData = SchedulerGetScheduleData(con, name)
        End Using

        Return New SessionRunnerSchedule(Nothing, scheduleData)
    End Function

    ''' <summary>
    ''' Gets the schedule with the given name from the database.
    ''' Since schedule's names must be unique, there should only be at most one
    ''' schedule with the given name on the database.
    ''' </summary>
    ''' <param name="connection">The connection over which the schedule with the given
    ''' name should be retrieved.</param>
    ''' <param name="name">The name of the schedule required.</param>
    ''' <returns>The schedule with the given name or Nothing if no schedule with
    ''' the given name was found.</returns>
    Private Function SchedulerGetScheduleData(ByVal connection As IDatabaseConnection, ByVal name As String) _
     As ScheduleDatabaseData
        Dim cmd As New SqlCommand("select id from BPASchedule where name=@name")
        cmd.Parameters.AddWithValue("@name", name)
        Dim obj As Object = connection.ExecuteReturnScalar(cmd)
        If obj Is Nothing Then Return Nothing
        Return SchedulerGetScheduleData(connection, DirectCast(obj, Integer))
    End Function

    ''' <summary>
    ''' Gets the schedule represented by the given ID over the provided
    ''' connection.
    ''' </summary>
    ''' <param name="connection">The connection over which the schedule should be
    ''' retrieved.</param>
    ''' <param name="id">The ID of the schedule which is required.</param>
    ''' <returns>The fully populated schedule which corresponds to the given ID,
    ''' or Nothing if no such schedule exists.</returns>
    Private Function SchedulerGetScheduleData(
     ByVal connection As IDatabaseConnection, ByVal id As Integer) As ScheduleDatabaseData
        Dim schedule As ScheduleDatabaseData

        Using reader = GetScheduleFromDatabase(connection, id)
            If Not reader.Read() Then Return Nothing

            Dim provider As New ReaderDataProvider(reader)
            schedule = New ScheduleDatabaseData(provider)

            reader.NextResult()
            While reader.Read()
                Dim task = New ScheduleTaskDatabaseData(provider)
                schedule.Tasks.Add(task)
            End While

            reader.NextResult()
            While reader.Read()
                Dim taskSession = New ScheduleTaskSessionDatabaseData(provider)
                schedule.TaskSessions.Add(taskSession)
            End While

            reader.NextResult()
            While reader.Read()
                Dim trigger = New ScheduleTriggerDatabaseData(provider)
                schedule.Triggers.Add(trigger)
            End While
        End Using

        ' This can't be done inside the creation block above as the DataReader needs to be reused when querying the permissions
        ' For the logged in user so must be done outside of the USING statement.
        schedule.TaskSessions.ForEach(
                Sub(x)
                    x.CanCurrentUserSeeProcess = GetEffectiveMemberPermissionsForProcess(connection, x.ProcessId).HasPermission(mLoggedInUser, Permission.ProcessStudio.ImpliedViewProcess)
                    x.CanCurrentUserSeeResource = GetEffectiveMemberPermissionsForResource(connection, x.ResourceId).HasPermission(mLoggedInUser, Permission.Resources.ImpliedViewResource)
                End Sub)

        Return schedule
    End Function

    Private Function GetScheduleFromDatabase(connection As IDatabaseConnection, scheduleId As Integer) As IDataReader
        Dim query As String
        query = "SELECT id, [name], [description], initialtaskid, versionno, retired " &
                "FROM BPASchedule " &
                "WHERE id = @id; " &
                "SELECT id, [name], [description], failfastonerror, delayafterend, onsuccess, onfailure " &
                "FROM BPATask " &
                "WHERE scheduleid = @id; " &
                "SELECT ts.taskid, ts.id, ts.processid, ts.resourcename, ts.processparams, r.resourceid " &
                "FROM BPATaskSession ts " &
                "INNER JOIN BPATask t ON ts.taskid = t.id " &
                "INNER JOIN BPAResource r ON ts.resourcename = r.[name] " &
                "WHERE t.scheduleid = @id; " &
                "SELECT unittype, mode, [priority], startdate, enddate, [period], startpoint, [endpoint], " &
                       "dayset, calendarid, nthofmonth, missingdatepolicy, usertrigger, timezoneId, utcoffset " &
                "FROM BPAScheduleTrigger " &
                "WHERE scheduleid = @id;"

        Using command = New SqlCommand()
            command.CommandType = CommandType.Text
            command.CommandText = query
            command.Parameters.AddWithValue("@id", scheduleId)

            Return connection.ExecuteReturnDataReader(command)
        End Using

    End Function

    ''' <summary>
    ''' Creates the given schedule and all its dependents on the database.
    ''' </summary>
    ''' <param name="schedule">The schedule to be created</param>
    ''' <returns>The schedule after it has been created with its ID and all its
    ''' task IDs set appropriately</returns>
    ''' <exception cref="Server.Domain.Models.NameAlreadyExistsException">If either the name of the
    ''' schedule is already in use on the database, or if 2 tasks on the schedule
    ''' have the same name.</exception>
    <SecuredMethod(Permission.Scheduler.CreateSchedule)>
    Public Function SchedulerCreateSchedule(
     ByVal schedule As SessionRunnerSchedule) _
     As SessionRunnerSchedule Implements IServer.SchedulerCreateSchedule
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            SchedulerCreateSchedule(con, schedule)
            con.CommitTransaction()
            Return schedule
        End Using

    End Function

    ''' <summary>
    ''' Creates the given schedule and all its dependents on the database.
    ''' </summary>
    ''' <param name="schedule">The schedule to be created</param>
    ''' <exception cref="Server.Domain.Models.NameAlreadyExistsException">If either the name of the
    ''' schedule is already in use on the database, or if 2 tasks on the schedule
    ''' have the same name.</exception>
    ''' <remarks>This operates on the schedule in place, ensuring that all
    ''' references are kept up to date.</remarks>
    Private Sub SchedulerCreateSchedule(con As IDatabaseConnection,
                                        schedule As SessionRunnerSchedule)
        SchedulerCreateSchedule(con, schedule, True)

    End Sub
    ''' <summary>
    ''' Creates the given schedule and all its dependents on the database.
    ''' </summary>
    ''' <param name="schedule">The schedule to be created</param>
    ''' <exception cref="Server.Domain.Models.NameAlreadyExistsException">If either the name of the
    ''' schedule is already in use on the database, or if 2 tasks on the schedule
    ''' have the same name.</exception>
    ''' <param name="incrementVersion">Controls whether version data that is monitored
    ''' to check for updates is incremented following the change. If this change is
    ''' part of a batch, then the version update may be deferred until all changes
    ''' in the batch have been made.</param>
    ''' <remarks>This operates on the schedule in place, ensuring that all
    ''' references are kept up to date.</remarks>
    Private Sub SchedulerCreateSchedule(con As IDatabaseConnection,
                                        schedule As SessionRunnerSchedule,
                                        incrementVersion As Boolean)

        If schedule.Id > 0 Then Throw New InvalidOperationException(
         My.Resources.clsServer_SchedulerAlreadyHasAnIDCannotRecreateIt)

        Dim cmd As New SqlCommand()
        cmd.Parameters.AddWithValue("@name", schedule.Name)

        ' We need to check the name manually now that the unique constraint has gone
        ' from the database
        cmd.CommandText = "select 1 from BPASchedule where name = @name"
        If con.ExecuteReturnScalar(cmd) IsNot Nothing Then
            Throw New BluePrism.Server.Domain.Models.NameAlreadyExistsException(
             My.Resources.clsServer_TheScheduleName0IsAlreadyInUse, schedule.Name)
        End If

        ' First insert the schedule - needs to be in its own batch so
        ' we can grab hold of the schedule ID.
        cmd.CommandText =
         "insert into BPASchedule (name, description, versionno) " &
         "values (@name, @description, 1); " & _
 _
         "select scope_identity()"

        cmd.Parameters.AddWithValue("@description", schedule.Description)

        schedule.Id = CInt(con.ExecuteReturnScalar(cmd))

        ' Now to add all the stub tasks...
        ' Note that we leave the OnSuccess and OnFailure for now, since they are
        ' foreign keys and will cause an error if set on the database before the
        ' corresponding task record is added.
        Dim taskNames As New clsSet(Of String)(StringComparer.InvariantCultureIgnoreCase)
        For Each task As ScheduledTask In schedule
            If Not taskNames.Add(task.Name.ToLower()) Then
                Throw New BluePrism.Server.Domain.Models.NameAlreadyExistsException(
                 My.Resources.clsServer_TheTaskName0IsPresentMoreThanOnceInThisSchedule, task.Name)
            End If
            SchedulerCreateStubTask(con, task)
        Next
        ' Probably pointless, but we don't need it any more...
        taskNames = Nothing

        ' Once we've updated all the tasks, make sure that the OnSuccess and OnFailure
        ' are pointing to the correct task records.
        SchedulerUpdateAllChainedTaskIds(con, schedule)

        ' At this point, the initialtaskid will have been set by the
        ' CreateTask() for whichever task it was pointing to (if it
        ' is actually set at all)
        If schedule.InitialTaskId > 0 Then
            cmd.CommandText =
             "update BPASchedule " &
             "  set initialtaskid=@taskid, versionno=versionno+1 " &
             "   where id=@scheduleid; " &
             "select versionno from BPASchedule where id=@scheduleid"
            With cmd.Parameters
                .Clear()
                .AddWithValue("@taskid", schedule.InitialTaskId)
                .AddWithValue("@scheduleid", schedule.Id)
            End With
            con.Execute(cmd)
        End If

        ' Now the triggers...
        SchedulerUpdateTriggersOnSchedule(con, schedule)

        AuditRecordScheduleEvent(con,
         New ScheduleAuditEvent(ScheduleEventCode.ScheduleCreated, mLoggedInUser, schedule.Id))

        ' And let interested parties know that the scheduler data has changed
        If incrementVersion Then
            IncrementSchedulerDataVersion(con)
        End If

    End Sub

    ''' <summary>
    ''' Updates the given schedule on the database and returns it with any IDs
    ''' populated as appropriate
    ''' </summary>
    ''' <param name="schedule">The schedule to update</param>
    ''' <returns>The schedule after update with all IDs appropriately populated
    ''' </returns>
    ''' <exception cref="Server.Domain.Models.NameAlreadyExistsException">If either the name of the
    ''' schedule is already in use on the database, or if 2 tasks on the schedule
    ''' have the same name.</exception>
    <SecuredMethod(Permission.Scheduler.EditSchedule)>
    Public Function SchedulerUpdateSchedule(
     ByVal schedule As SessionRunnerSchedule) As SessionRunnerSchedule Implements IServer.SchedulerUpdateSchedule
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            SchedulerUpdateSchedule(con, schedule)
            con.CommitTransaction()
            Return schedule
        End Using

    End Function

    ''' <summary>
    ''' Updates the given schedule, and writes the appropriate audit records for
    ''' the supplied user.
    ''' </summary>
    ''' <param name="con">The connection over which to update the schedule.
    ''' </param>
    ''' <param name="schedule">The schedule to update.</param>
    Private Sub SchedulerUpdateSchedule(con As IDatabaseConnection,
                                        schedule As SessionRunnerSchedule)
        SchedulerUpdateSchedule(con, schedule, True)
    End Sub

    Private mSchedulerGetSchedule As Func(Of IDatabaseConnection, Integer, SessionRunnerSchedule) = Function(con, id) SchedulerGetSchedule(con, id)

    Private mModifiedScheduleAuditEventGeneratorFactory As Func(Of SessionRunnerSchedule,
        SessionRunnerSchedule, IUser, IModifiedScheduleAuditEventGenerator) =
        Function(oldSchedule, newSchedule, user) New ModifiedScheduleAuditEventGenerator(oldSchedule, newSchedule, user)

    ''' <summary>
    ''' Updates the given schedule, and writes the appropriate audit records for
    ''' the supplied user.
    ''' </summary>
    ''' <param name="con">The connection over which to update the schedule.
    ''' </param>
    ''' <param name="schedule">The schedule to update.</param>
    ''' <param name="incrementVersion">Controls whether version data that is monitored
    ''' to check for updates is incremented following the change. If this change is
    ''' part of a batch, then the version update may be deferred until all changes
    ''' in the batch have been made.</param>
    Private Sub SchedulerUpdateSchedule(con As IDatabaseConnection,
                                        schedule As SessionRunnerSchedule,
                                        incrementVersion As Boolean)

        Dim list As IList(Of ScheduleAuditEvent) = schedule.GetAuditEvents()

        ' We need to deal with the 'Delete Task' ones first... otherwise
        ' we won't be able to retrieve the name of the task... it'll be deleted.
        ' Note we can't do all of them first, since we need to get the names
        ' of the newly created tasks too.
        For i As Integer = list.Count - 1 To 0 Step -1
            If list(i).Code = ScheduleEventCode.TaskRemoved Then
                AuditRecordScheduleEvent(con, list(i))
                list.RemoveAt(i)
            End If
        Next

        Dim scheduleBeforeSave = mSchedulerGetSchedule(con, schedule.Id)
        If scheduleBeforeSave.Owner Is Nothing Then scheduleBeforeSave.Owner = New InertScheduler(New DatabaseBackedScheduleStore(Me))

        SchedulerUpdateScheduleWithoutAudits(con, schedule)

        mModifiedScheduleAuditEventGeneratorFactory(scheduleBeforeSave, schedule, mLoggedInUser).
            Generate().
            ForEach(Sub(evt) AuditRecordScheduleEvent(con, evt)).
            Evaluate()

        ' Mop up the remaining audit events.
        For Each e As ScheduleAuditEvent In list
            AuditRecordScheduleEvent(con, e)
        Next

        If incrementVersion Then
            IncrementSchedulerDataVersion(con)
        End If

    End Sub

    ''' <summary>
    ''' Updates the given schedule on the given connection and returns it with
    ''' any IDs populated as appropriate
    ''' </summary>
    ''' <param name="con">The connection over which to update the schedule.
    ''' </param>
    ''' <param name="schedule">The schedule to update</param>
    Private Sub SchedulerUpdateScheduleWithoutAudits(
     ByVal con As IDatabaseConnection, ByVal schedule As SessionRunnerSchedule)

        ' Go through the tasks first - delete any which have been deleted and
        ' insert any which are not already on the database - we need to make sure
        ' they are there so that the OnSuccess / OnFailure and InitialTaskId
        ' references all work correctly.
        ' CreateTask also ensures that the Schedule.InitialTaskId is set to the
        ' new ID if it was previously pointing to a task with a temp ID.

        ' Deletions first... for now just set the name to NULL until we have
        ' had a chance to change the onsuccess / onfailure of the other tasks
        ' to their new values.
        Dim cmd As New SqlCommand()

        ' Schedule ID and name don't change - might as well add them immediately
        cmd.Parameters.AddWithValue("@scheduleid", schedule.Id)
        cmd.Parameters.AddWithValue("@name", schedule.Name)

        Dim sb As New StringBuilder("update BPATask set name=NULL where scheduleid=@scheduleid")
        If schedule.Count > 0 Then
            sb.Append(" and id not in (")
            Dim index As Integer = 0
            For Each task As ScheduledTask In schedule
                sb.AppendFormat("@id{0},", index)
                cmd.Parameters.AddWithValue("@id" & index, task.Id)
                index += 1
            Next
            sb.Length -= 1
            sb.Append(")")
        End If
        cmd.CommandText = sb.ToString()
        con.Execute(cmd)

        ' Try creating / updating the tasks - if we get a Unique Key error
        ' that means that the task has failed a 'unique name' constraint
        ' Throw the appropriate exception
        Dim lastName As String = Nothing

        ' Now create any tasks which have been added...
        Dim tasksToUpdate As IBPSet(Of ScheduledTask) = New clsSet(Of ScheduledTask)
        Dim taskNames As New clsSet(Of String)
        For Each t As ScheduledTask In schedule
            lastName = t.Name
            If Not taskNames.Add(lastName.ToLower()) Then
                Throw New BluePrism.Server.Domain.Models.NameAlreadyExistsException(
                 My.Resources.clsServer_TheTaskName0IsPresentMoreThanOnceInThisSchedule, lastName)
            End If
            If t.Id <= 0 Then SchedulerCreateStubTask(con, t) Else tasksToUpdate.Add(t)
        Next

        ' Ok - every task should have an ID set now, so the OnSuccess / OnFailure
        ' references should work correctly.
        For Each t As ScheduledTask In tasksToUpdate
            SchedulerUpdateTask(con, t)
        Next

        ' Finally, some of the tasks could have an onsuccess / onfailure with a
        ' temporary ID - ensure that they are all set to their database values.
        SchedulerUpdateAllChainedTaskIds(con, schedule)

        ' And now delete any tasks from before which have had their name set to null,
        ' Any foreign key dependencies should be fixed by now - if they're not,
        ' then it's an error.
        cmd.CommandText = "delete from BPATask where scheduleid = @scheduleid and name is null"
        con.Execute(cmd)

        ' We need to check the name manually now that the unique constraint has gone
        ' from the database
        cmd.CommandText = "select 1 from BPASchedule where id != @scheduleid and name = @name"
        If con.ExecuteReturnScalar(cmd) IsNot Nothing Then
            Throw New BluePrism.Server.Domain.Models.NameAlreadyExistsException(
             My.Resources.clsServer_TheScheduleName0IsAlreadyInUse, schedule.Name)
        End If

        ' The initial task ID should also be correctly available now too.
        cmd.CommandText =
         " update BPASchedule " &
         "   set name=@name, description=@description, " &
         "       initialtaskid=@inittaskid, versionno=versionno+1 " &
         "   where id=@scheduleid; " &
         " select versionno from BPASchedule where id=@scheduleid;"

        With cmd.Parameters
            .AddWithValue("@description", schedule.Description)
            .AddWithValue("@inittaskid", schedule.InitialTaskId)
        End With
        schedule.Version = CInt(con.ExecuteReturnScalar(cmd))

        SchedulerUpdateTriggersOnSchedule(con, schedule)

    End Sub

    ''' <summary>
    ''' Retires the given schedule, thereby removing it from active duty
    ''' </summary>
    ''' <param name="schedule">The schedule to be retired</param>
    <SecuredMethod(Permission.Scheduler.RetireSchedule)>
    Public Sub SchedulerRetireSchedule(
     ByVal schedule As SessionRunnerSchedule) Implements IServer.SchedulerRetireSchedule
        CheckPermissions()
        Using con = GetConnection()
            SchedulerRetireSchedule(con, schedule)
            AuditRecordScheduleEvent(con,
             New ScheduleAuditEvent(ScheduleEventCode.ScheduleRetired, mLoggedInUser, schedule.Id))

        End Using
    End Sub

    ''' <summary>
    ''' Retires the given schedule, thereby removing it from active duty
    ''' </summary>
    ''' <param name="con">The connection to the database to use to retire
    ''' the schedule.</param>
    ''' <param name="schedule">The schedule to be retired</param>
    Private Sub SchedulerRetireSchedule(ByVal con As IDatabaseConnection, ByVal schedule As SessionRunnerSchedule)
        Dim cmd As New SqlCommand("update BPASchedule set retired=1 where id=@scheduleid")
        cmd.Parameters.AddWithValue("@scheduleid", schedule.Id)
        con.Execute(cmd)
        IncrementSchedulerDataVersion(con)
    End Sub

    ''' <summary>
    ''' Unretires the given schedule, thereby restoring it to active duty
    ''' </summary>
    ''' <param name="schedule">The schedule to be unretired</param>
    <SecuredMethod(Permission.Scheduler.CreateSchedule)>
    Public Sub SchedulerUnretireSchedule(
            schedule As SessionRunnerSchedule) Implements IServer.SchedulerUnretireSchedule
        CheckPermissions()
        Using con = GetConnection()
            SchedulerUnretireSchedule(con, schedule)
            AuditRecordScheduleEvent(con,
             New ScheduleAuditEvent(ScheduleEventCode.ScheduleUnretired, mLoggedInUser, schedule.Id))

        End Using
    End Sub

    ''' <summary>
    ''' Unretires the given schedule, thereby restoring it to active duty
    ''' </summary>
    ''' <param name="con">The connection to the database to use to unretire
    ''' the schedule.</param>
    ''' <param name="schedule">The schedule to be unretired</param>
    Private Sub SchedulerUnretireSchedule(ByVal con As IDatabaseConnection, ByVal schedule As SessionRunnerSchedule)
        Dim cmd As New SqlCommand("update BPASchedule set retired=0 where id=@scheduleid")
        cmd.Parameters.AddWithValue("@scheduleid", schedule.Id)
        con.Execute(cmd)
        IncrementSchedulerDataVersion(con)
    End Sub

    ''' <summary>
    ''' Deletes the given schedule from the database, along with all its
    ''' dependents.
    ''' </summary>
    ''' <param name="schedule">The schedule to be deleted</param>
    <SecuredMethod(Permission.Scheduler.DeleteSchedule)>
    Public Sub SchedulerDeleteSchedule(
     ByVal schedule As SessionRunnerSchedule) Implements IServer.SchedulerDeleteSchedule
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            ' Seems odd recording the audit event before deleting the schedule,
            ' but since we're in a transaction, it should appear at an instant
            ' anyway - this makes sure that any required data from the schedule
            ' can be retrieved - obviously not possible once it's deleted.
            AuditRecordScheduleEvent(con,
             New ScheduleAuditEvent(ScheduleEventCode.ScheduleDeleted, mLoggedInUser, schedule.Id))
            SchedulerDeleteSchedule(con, schedule)
            IncrementSchedulerDataVersion(con)
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Deletes the given schedule from the database, along with all its
    ''' dependents.
    ''' </summary>
    ''' <param name="con">The connection to the database on which the
    ''' schedule should be deleted.</param>
    ''' <param name="schedule">The schedule to be deleted</param>
    Private Sub SchedulerDeleteSchedule(ByVal con As IDatabaseConnection, ByVal schedule As SessionRunnerSchedule)
        Using cmd As New SqlCommand("SELECT TOP 1 1 FROM BPAScheduleLog WHERE scheduleid=@scheduleid")
            cmd.Parameters.AddWithValue("@scheduleid", schedule.Id)

            If con.ExecuteReturnScalar(cmd) IsNot Nothing Then
                cmd.CommandText = "UPDATE BPASchedule SET deletedname=name, name=NULL WHERE id=@scheduleid"
                con.Execute(cmd)
            Else
                'If there are no log entries then we can do a cascade delete
                cmd.CommandText = "DELETE FROM BPASchedule WHERE id=@scheduleid"
                con.Execute(cmd)
            End If
        End Using
    End Sub

#End Region

#Region "Tasks and Sessions"

    ''' <summary>
    ''' Checks that the task can be safely deleted from its schedule.
    ''' ie. that it is not referenced on any tables which require its presence if
    ''' the schedule remains present (currently the BPAScheduleLogEntry only).
    ''' Note that this doesn't mean that the schedule which owns it cannot be
    ''' deleted, just that the task cannot be deleted from the schedule, with
    ''' the schedule left in place.
    ''' </summary>
    ''' <param name="id">The ID of the task to check for deletion availability
    ''' </param>
    ''' <returns>True if no error would occur when deleting the task from the
    ''' schedule.</returns>
    <SecuredMethod(True)>
    Public Function SchedulerCanDeleteTask(ByVal id As Integer) As Boolean Implements IServer.SchedulerCanDeleteTask
        CheckPermissions()
        Using con = GetConnection()
            Return SchedulerCanDeleteTask(con, id)
        End Using
    End Function

    ''' <summary>
    ''' Checks that the task can be safely deleted from its schedule.
    ''' ie. that it is not referenced on any tables which require its presence if
    ''' the schedule remains present (currently the BPAScheduleLogEntry only).
    ''' Note that this doesn't mean that the schedule which owns it cannot be
    ''' deleted, just that the task cannot be deleted from the schedule, with
    ''' the schedule left in place.
    ''' </summary>
    ''' <param name="con">The connection to use to check the task's presence on
    ''' other tables.</param>
    ''' <param name="id">The ID of the task to check for deletion availability
    ''' </param>
    ''' <returns>True if no error would occur when deleting the task from the
    ''' schedule.</returns>
    Private Function SchedulerCanDeleteTask(ByVal con As IDatabaseConnection, ByVal id As Integer) _
     As Boolean

        Dim cmd As New SqlCommand("select top 1 1 from BPAScheduleLogEntry where taskid=@id")
        cmd.Parameters.AddWithValue("@id", id)
        Return con.ExecuteReturnScalar(cmd) Is Nothing

    End Function


    ''' <summary>
    ''' Updates the sessions held in the given task. This is achieved by
    ''' first deleting all the sessions on the task then re-inserting them
    ''' </summary>
    ''' <param name="con">The connection to the database on which to update the
    ''' sessions</param>
    ''' <param name="task">The task controlling the sessions to update</param>
    Private Sub SchedulerUpdateSessionsOnTask(
     ByVal con As IDatabaseConnection, ByVal task As ScheduledTask)

        Dim cmd As New SqlCommand()
        Dim sb As New StringBuilder()
        sb.Append("delete from BPATaskSession where taskid=@taskid; ")
        cmd.Parameters.AddWithValue("@taskid", task.Id)

        If task.Sessions.Count > 0 Then
            sb.Append("insert into BPATaskSession (taskid, processid, resourcename, processparams) ")

            Dim count As Integer = 0
            For Each sess As ScheduledSession In task.Sessions
                If count > 0 Then sb.Append(" union all ")
                count += 1

                sb.AppendFormat(
                 "select @taskid, @processid{0}, @resourcename{0}, @processparams{0}", count)
                With cmd.Parameters
                    .AddWithValue("@processid" & count, sess.ProcessId)
                    .AddWithValue("@resourcename" & count, sess.ResourceName)
                    .AddWithValue("@processparams" & count,
                     IIf(sess.ArgumentsXML Is Nothing, DBNull.Value, sess.ArgumentsXML))
                End With
            Next
        End If
        cmd.CommandText = sb.ToString()
        con.Execute(cmd)
    End Sub

    ''' <summary>
    ''' Sets the task IDs for the given schedule on the database, ensuring that
    ''' any references are using the correct ID, and not a temporary task ID
    ''' or such like.
    ''' </summary>
    ''' <param name="con">The connection on which the task IDs should be set.
    ''' </param>
    ''' <param name="sched">The schedule whose tasks are now all represented
    ''' on the database and thus have valid ids.</param>
    ''' <remarks>This method should only be called when all the tasks on a
    ''' schedule have permanent unique IDs - not the temporary IDs granted
    ''' them by the Task class on first retrieval of an ID.</remarks>
    Private Sub SchedulerUpdateAllChainedTaskIds(
     ByVal con As IDatabaseConnection, ByVal sched As SessionRunnerSchedule)

        Dim cmd As New SqlCommand(
         "update BPATask " &
         "  set onsuccess=@onsuccess, onfailure=@onfailure " &
         "  where id=@taskid")

        For Each t As ScheduledTask In sched
            With cmd.Parameters
                .Clear()
                .AddWithValue("@taskid", t.Id)

                Dim onSuccId As Object = DBNull.Value
                If t.OnSuccess IsNot Nothing Then onSuccId = t.OnSuccess.Id
                .AddWithValue("@onsuccess", onSuccId)

                Dim onFailId As Object = DBNull.Value
                If t.OnFailure IsNot Nothing Then onFailId = t.OnFailure.Id
                .AddWithValue("@onfailure", onFailId)
            End With
            con.Execute(cmd)
        Next

    End Sub

    ''' <summary>
    ''' Creates the given task on the database using the given connection
    ''' </summary>
    ''' <param name="con">The connection on which the task creation should be
    ''' performed.</param>
    ''' <param name="task">The task to add. If this task already exists on the
    ''' database, it will fail</param>
    ''' <exception cref="InvalidOperationException">If the given task has already
    ''' been assigned an ID, and is thus already represented on the database,
    ''' -or- if the task has no schedule 'owner' and thus cannot be linked to the
    ''' corresponding schedule record on the database.
    ''' </exception>
    Private Function SchedulerCreateStubTask(
     ByVal con As IDatabaseConnection, ByVal task As ScheduledTask) As Integer

        Dim tempId As Integer = task.Id
        If tempId > 0 Then Throw New InvalidOperationException(
         My.Resources.clsServer_ThisTaskIsAlreadyOnTheDatabase)

        If task.Owner Is Nothing Then Throw New InvalidOperationException(
         String.Format(My.Resources.clsServer_Task0IsNotAssignedToASchedule, task.Name))

        ' Check to see if the schedule's initial task is the one we're creating
        Dim setAsInitialTask As Boolean = (task.Owner.InitialTaskId = tempId)

        Dim cmd As New SqlCommand(
         "insert into BPATask (scheduleid, name, description, failfastonerror, delayafterend) " &
         "values (@scheduleid, @name, @description, @failfast, @delayafterend); " &
         "select scope_identity()"
        )

        With cmd.Parameters
            .AddWithValue("@scheduleid", CType(task.Owner, SessionRunnerSchedule).Id)
            .AddWithValue("@name", task.Name)
            .AddWithValue("@description", task.Description)
            .AddWithValue("@failfast", task.FailFastOnError)
            .AddWithValue("@delayafterend", task.DelayAfterEnd)
        End With

        task.Id = CInt(con.ExecuteReturnScalar(cmd))

        If setAsInitialTask Then task.Owner.InitialTaskId = task.Id

        task.Owner.UpdateTaskIds(tempId, task.Id)

        SchedulerUpdateSessionsOnTask(con, task)

        Return task.Id

    End Function

    ''' <summary>
    ''' Updates the given task on the provided connection
    ''' </summary>
    ''' <param name="con">The connection on which to update the task.
    ''' </param>
    ''' <param name="task">The task to update on the database.</param>
    Private Sub SchedulerUpdateTask(ByVal con As IDatabaseConnection, ByVal task As ScheduledTask)
        If task.Id <= 0 Then
            Throw New InvalidOperationException(My.Resources.clsServer_NoIDFoundOnTheTaskCannotUpdateIt)
        End If

        Dim cmd As New SqlCommand(
         " update BPATask set" &
         "   name = @name," &
         "   description = @description," &
         "   onsuccess = @onsuccess," &
         "   onfailure = @onfailure," &
         "   failfastonerror = @failfast," &
         "   delayafterend = @delayafterend" &
         " where id = @taskid")


        With cmd.Parameters
            .AddWithValue("@name", task.Name)
            .AddWithValue("@description", task.Description)
            Dim onSuccId As Object = DBNull.Value
            If task.OnSuccess IsNot Nothing Then onSuccId = task.OnSuccess.Id
            .AddWithValue("@onsuccess", onSuccId)

            Dim onFailId As Object = DBNull.Value
            If task.OnFailure IsNot Nothing Then onFailId = task.OnFailure.Id
            .AddWithValue("@onfailure", onFailId)
            .AddWithValue("@taskid", task.Id)
            .AddWithValue("@failfast", task.FailFastOnError)
            .AddWithValue("@delayafterend", task.DelayAfterEnd)
        End With
        con.Execute(cmd)

        SchedulerUpdateSessionsOnTask(con, task)

    End Sub

    ''' <summary>
    ''' Gets the Task name given the task ID
    ''' </summary>
    ''' <param name="id">The task ID</param>
    ''' <returns>The task name</returns>
    <SecuredMethod(False)>
    Public Function SchedulerGetTaskNameFromID(ByVal id As Integer) As String Implements IServer.SchedulerGetTaskNameFromID
        CheckPermissions()
        Using con = GetConnection()
            Using cmd As New SqlCommand("SELECT name from BPATask WHERE id=@id")
                cmd.Parameters.AddWithValue("@id", id)
                Return CStr(con.ExecuteReturnScalar(cmd))
            End Using
        End Using
    End Function

    <SecuredMethod(False)>
    Public Function SchedulerGetScheduledTasks(id As Integer) As ICollection(Of Server.Domain.Models.ScheduledTask) _
        Implements IServer.SchedulerGetScheduledTasks
        CheckPermissions()
        Dim taskList = New List(Of Server.Domain.Models.ScheduledTask)()
        Using con = GetConnection()
            ThrowIfScheduleNotExist(con, id)
            ThrowIfScheduleHasBeenDeleted(con, id)
            Using cmd As New SqlCommand("select t.id, " &
                                        "t.name, " &
                                        "t.description, " &
                                        "t.onsuccess, " &
                                        "t.onfailure, " &
                                        "t1.id as successTaskID, " &
                                        "t1.name as successTaskName, " &
                                        "t2.id as FailureTaskID, " &
                                        "t2.name as FailureTaskName, " &
                                        "t.failfastonerror, " &
                                        "t.delayafterend " &
                                        "from BPATask t " &
                                        "left Join bpatask as t1 on t1.id = t.onsuccess " &
                                        "left Join bpatask as t2 on t2.id = t.onfailure " &
                                        "where t.scheduleid = @id")
                cmd.Parameters.AddWithValue("@id", id)
                Using reader = con.ExecuteReturnDataReader(cmd)
                    Dim dataProvider As New ReaderDataProvider(reader)
                    While reader.Read()
                        Dim task = New Server.Domain.Models.ScheduledTask With
                        {
                                .id = dataProvider.GetInt("id"),
                                .Name = dataProvider.GetString("name"),
                                .Description = dataProvider.GetString("description"),
                                .FailFastOnError = dataProvider.GetBool("failfastonerror"),
                                .DelayAfterEnd = dataProvider.GetInt("delayafterend"),
                                .OnSuccessTaskId = dataProvider.GetInt("successTaskID"),
                                .OnFailureTaskId = dataProvider.GetInt("FailureTaskID"),
                                .OnSuccessTaskName = dataProvider.GetString("successTaskName"),
                                .OnFailureTaskName = dataProvider.GetString("FailureTaskName")}
                        taskList.Add(task)
                    End While
                End Using
                Return taskList
            End Using
        End Using
    End Function

#End Region

#Region "Triggers"

    ''' <summary>
    ''' Refreshes the triggers on the given schedule, ensuring that the database
    ''' values matche the values on the schedule passed in.
    ''' </summary>
    ''' <param name="con">The connection on which the triggers should be updated
    ''' </param>
    ''' <param name="schedule">The schedule for which the triggers should be
    ''' updated.</param>
    Private Sub SchedulerUpdateTriggersOnSchedule(
     ByVal con As IDatabaseConnection, ByVal schedule As ISchedule)

        If schedule.Owner Is Nothing Then
            schedule.Owner = New InertScheduler(New DatabaseBackedScheduleStore(Me))
        End If

        ' Delete all the triggers currently configured for this schedule
        Dim cmd As New SqlCommand(
         "delete from BPAScheduleTrigger where scheduleid = @scheduleid")

        cmd.Parameters.AddWithValue("@scheduleid",
         CType(schedule, SessionRunnerSchedule).Id)

        con.Execute(cmd)

        ' Save all of the current trigger metadata for this schedule
        Dim metadata As ICollection(Of TriggerMetaData) = schedule.Triggers.MetaData
        If metadata.Count > 0 Then
            cmd.CommandText =
             " insert into BPAScheduleTrigger " &
             "  (scheduleid, mode, priority, unittype, period, " &
             "   startdate, enddate, startpoint, endpoint, " &
             "   dayset, calendarid, nthofmonth, missingdatepolicy, usertrigger, timezoneId, utcoffset)" &
             " values (" &
             "   @scheduleid, @mode, @priority, @unittype, @period, " &
             "   @startdate, @enddate, @startpoint, @endpoint, " &
             "   @dayset, @calendarid, @nthofmonth, " &
             "   @missingdatepolicy, @usertrigger, @timezoneId, @utcoffset" &
             " )"

            ' Note that @scheduleid is still in there from the above delete.
            With cmd.Parameters
                .Add("@priority", SqlDbType.Int)
                .Add("@mode", SqlDbType.TinyInt)
                .Add("@unittype", SqlDbType.TinyInt)
                .Add("@period", SqlDbType.Int)
                .Add("@startdate", SqlDbType.DateTime)
                .Add("@enddate", SqlDbType.DateTime)
                .Add("@startpoint", SqlDbType.Int)
                .Add("@endpoint", SqlDbType.Int)
                .Add("@dayset", SqlDbType.Int)
                .Add("@calendarid", SqlDbType.Int)
                .Add("@nthofmonth", SqlDbType.Int)
                .Add("@missingdatepolicy", SqlDbType.Int)
                .Add("@usertrigger", SqlDbType.Bit)
                .Add("@timezoneId", SqlDbType.VarChar)
                .Add("@utcoffset", SqlDbType.Int)
            End With

            ' Now go through each trigger and insert it into the database.
            For Each data As TriggerMetaData In metadata
                With cmd.Parameters
                    .Item("@priority").Value = data.Priority
                    .Item("@mode").Value = data.Mode
                    .Item("@unittype").Value = data.Interval
                    .Item("@period").Value = data.Period
                    .Item("@startdate").Value =
                        clsDBConnection.UtilDateToSqlDate(data.Start, False, False)
                    .Item("@enddate").Value =
                        clsDBConnection.UtilDateToSqlDate(data.End, False, True)
                    .Item("@startpoint").Value =
                     data.AllowedHours.StartTime.TotalSeconds
                    .Item("@endpoint").Value =
                     data.AllowedHours.EndTime.TotalSeconds
                    .Item("@dayset").Value =
                     IIf(data.Days Is Nothing, DBNull.Value, data.Days.ToInt())
                    .Item("@calendarid").Value =
                     IIf(data.CalendarId = 0, DBNull.Value, data.CalendarId)
                    .Item("@nthofmonth").Value = data.Nth
                    .Item("@missingdatepolicy").Value = data.MissingDatePolicy
                    .Item("@usertrigger").Value = data.IsUserTrigger
                    .Item("@timezoneId").Value = IIf(data.TimeZoneId Is Nothing, DBNull.Value, data.TimeZoneId)
                    .Item("@utcoffset").Value = If(data.UtcOffset Is Nothing, DBNull.Value, CType(data.UtcOffset.Value.TotalMinutes, Object))
                End With
                con.Execute(cmd)
            Next
        End If

    End Sub

#End Region

#Region "Logging"


    ''' <summary>
    ''' Gets the historical schedule log for the execution of the given
    ''' schedule at the specified instant in time.
    ''' </summary>
    ''' <param name="scheduleId">The ID of the schedule for which the log
    ''' is required.</param>
    ''' <param name="instant">The instant in time for which the log is
    ''' required.</param>
    ''' <returns>The log which represents the historical log of the
    ''' execution required schedule at the required time, or Nothing if
    ''' no log exists for that point in time.</returns>
    <SecuredMethod(False)>
    Public Function SchedulerGetLog(ByVal scheduleId As Integer, ByVal instant As DateTime) _
     As HistoricalScheduleLog Implements IServer.SchedulerGetLog
        CheckPermissions()
        Using con = GetConnection()
            Return SchedulerGetLog(con, scheduleId, instant)
        End Using

    End Function

    ''' <summary>
    ''' Gets the historical schedule log for the activation of the given
    ''' schedule at the specified instant in time.
    ''' </summary>
    ''' <param name="con">The connection to the database to use to retrieve
    ''' the log.</param>
    ''' <param name="scheduleId">The ID of the schedule for which the log
    ''' is required.</param>
    ''' <param name="instant">The instant in time for which the log is
    ''' required.</param>
    ''' <returns>The log which represents the historical log of the
    ''' execution required schedule at the required time, or Nothing if
    ''' no log exists for that point in time.</returns>
    Private Function SchedulerGetLog(
     ByVal con As IDatabaseConnection, ByVal scheduleId As Integer, ByVal instant As DateTime) _
     As HistoricalScheduleLog

        Dim cmd As New SqlCommand(
         " select id from BPAScheduleLog" &
         " where scheduleid=@scheduleid and instancetime=@instancetime")
        cmd.Parameters.AddWithValue("@scheduleid", scheduleId)
        cmd.Parameters.AddWithValue("@instancetime", instant)

        Dim oid As Object = con.ExecuteReturnScalar(cmd)
        If oid Is Nothing OrElse Convert.IsDBNull(oid) Then Return Nothing ' No log for that time..
        Dim idArr() As Integer = {CInt(oid)}

        ' Get the collection of logs for that schedule ID - we know there's only one
        ' since we passed a single log ID, so we can immediately step into it and
        ' return the only one there.
        Dim schedules = SchedulerGetLogsById(con, idArr)(scheduleId)
        If (schedules.Any) Then
            Return schedules.First()
        End If

        ' Otherwise there were no logs found for that schedule ID / log ID combo...
        ' which is weird, since we just ascertained that it was there.
        Throw New InvalidOperationException(String.Format(
         My.Resources.clsServer_LogID0NotFoundForScheduleID1, CInt(oid), scheduleId))

    End Function

    ''' <summary>
    ''' Gets the running logs for the given schedule ID.
    ''' Note that the schedule is not set in these logs by virtue of the fact
    ''' that the server doesn't have access to the schedule objects directly
    ''' </summary>
    ''' <param name="scheduleId">The ID for which the currently running logs
    ''' are required.</param>
    ''' <returns>A collection of schedule logs representing the currently
    ''' running instances of the specified schedule.</returns>
    <SecuredMethod(False)>
    Public Function SchedulerGetRunningLogs(ByVal scheduleId As Integer) _
     As ICollection(Of IScheduleLog) Implements IServer.SchedulerGetRunningLogs
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand(
             " select instancetime, starttime, heartbeat as ""lastpulse""" &
             " from BPVAnnotatedScheduleLog" &
             " where scheduleid = @scheduleid and endtime is null"
            )
            cmd.Parameters.AddWithValue("@scheduleid", scheduleId)
            Dim logs As New List(Of IScheduleLog)
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    logs.Add(New BasicReadOnlyLog(prov))
                End While
            End Using
            Return logs
        End Using

    End Function

    <SecuredMethod(False)>
    Public Function SchedulerGetCurrentAndPassedLogs(schedulerParameters As ScheduleLogParameters) _
        As ICollection(Of ScheduleLog) Implements IServer.SchedulerGetCurrentAndPassedLogs

        CheckPermissions()

        Using connection = GetConnection()

            If TypeOf schedulerParameters.ScheduleId Is DataFilters.EqualsDataFilter(Of Integer) Then
                Dim scheduleId = DirectCast(schedulerParameters.ScheduleId, DataFilters.EqualsDataFilter(Of Integer)).EqualTo
                ThrowIfScheduleNotExist(connection, scheduleId)
            End If

            Dim queryStringBuilder As New StringBuilder(500)
            queryStringBuilder.Append($"
                with rs as
                (
                    select sle.schedulelogid,
                    max(case when entrytype = 0 then  entrytime end) startTime,
                    max(case when (entrytype = 1 OR entrytype = 2) then  entrytime end) endTime,
                    count(case when entrytype = 1 then 1 end) scheduleCompleted,
                    count(case when entrytype = 5 then 1 end) taskTerminated,
                    count(case when entrytype = 4 then 1 end) taskCompleted
                    from BPAScheduleLogEntry sle
                    group by sle.schedulelogid
                ),
                logs as
                (
                    select
                        s.id as scheduleId, 
		                s.name as scheduleName, 
		                sl.servername, 
		                sl.id as scheduleLogId, 
		                rs.startTime, 
		                rs.endTime,
                        case when (rs.startTime is null) then 0                               --pending
                             when (rs.endTime is null) then 1                                 --Running
                             when (rs.scheduleCompleted = 1 and rs.taskTerminated = 1) then 9 --Partexceptioned
                             when (rs.scheduleCompleted = 0 and rs.taskCompleted = 1) then 9  --Partexceptioned
                             when (rs.scheduleCompleted = 1) then 4                           --Completed
                             else 2                                                           --Terminated
                        end as status
                    from [BPAScheduleLog] sl
                    left join rs on rs.schedulelogid=sl.id
	                join BPASchedule s on sl.scheduleid = s.id
                )
                select TOP({schedulerParameters.ItemsPerPage})
                    scheduleId, 
	                scheduleName,
	                servername,
	                scheduleLogId,
                    startTime,
                    endTime,
                    status
                from logs")

            Dim sqlCommand As New SqlCommand()
            sqlCommand.CommandTimeout = Options.Instance.SqlCommandTimeoutLong

            queryStringBuilder.Append(" where scheduleName is not null")
            Dim sqlData = schedulerParameters.
                GetSqlWhereClauses(sqlCommand).
                GetSqlWhereWithParametersStartingWithAndKeyword()

            queryStringBuilder.AppendLine(sqlData.sqlWhereClause)
            sqlCommand.Parameters.AddRange(sqlData.sqlParameters)

            queryStringBuilder.Append(" order by startTime desc, scheduleLogId desc")

            sqlCommand.CommandText = queryStringBuilder.ToString()

            Dim logs As New List(Of ScheduleLog)
            Using reader = connection.ExecuteReturnDataReader(sqlCommand)
                Dim provider As New ReaderDataProvider(reader)

                While reader.Read()

                    Dim startTime = provider.GetValue("startTime", DateTime.MinValue)
                    Dim endTime = provider.GetValue("endTime", DateTime.MinValue)

                    Dim scheduleLog = New ScheduleLog() With
                        {
                            .ScheduleId = provider.GetInt("scheduleId"),
                            .ScheduleName = provider.GetString("scheduleName"),
                            .ServerName = provider.GetString("servername"),
                            .ScheduleLogId = provider.GetInt("scheduleLogId"),
                            .Status = provider.GetValue(Of ItemStatus)("status", Nothing),
                            .startTime = If(startTime = DateTime.MinValue, Func.OptionHelper.None(Of DateTime)(), Func.OptionHelper.Some(startTime)),
                            .endTime = If(endTime = DateTime.MinValue, Func.OptionHelper.None(Of DateTime)(), Func.OptionHelper.Some(endTime))
                        }
                    logs.Add(scheduleLog)
                End While
            End Using
            Return logs
        End Using
    End Function


    Private Sub ThrowIfScheduleNotExist(con As IDatabaseConnection, scheduleId As Integer)
        Dim cmd = mDatabaseCommandFactory("select id from BPASchedule where id = @scheduleid")
        cmd.AddParameter("@scheduleid", scheduleId)

        Using reader = con.ExecuteReturnDataReader(cmd)
            If Not reader.Read() Then Throw New NoSuchScheduleException(scheduleId)
        End Using
    End Sub

    Private Sub ThrowIfScheduleHasBeenDeleted(con As IDatabaseConnection, scheduleId As Integer)
        Dim cmd = mDatabaseCommandFactory("select id from BPASchedule where id = @scheduleId and deletedname is not null")
        cmd.AddParameter("@scheduleid", scheduleId)

        Using reader = con.ExecuteReturnDataReader(cmd)
            If reader.Read() Then Throw New DeletedScheduleException(scheduleId)
        End Using
    End Sub

    Private Sub ThrowIfTaskNotExist(con As IDatabaseConnection, taskId As Integer)
        Dim cmd = mDatabaseCommandFactory("select id from BPATask where id = @taskid")
        cmd.AddParameter("@taskid", taskId)

        Using reader = con.ExecuteReturnDataReader(cmd)
            If Not reader.Read() Then Throw New NoSuchTaskException(taskId)
        End Using
    End Sub

    Private Sub ThrowIfScheduleHasBeenDeletedForTask(con As IDatabaseConnection, taskId As Integer)
        Dim cmd = mDatabaseCommandFactory("select s.id " &
                                          "from BPATask t " &
                                          "join BPASchedule s on s.id = t.scheduleid " &
                                          "where t.id = @taskId and s.deletedname is not null")
        cmd.AddParameter("@taskId", taskId)

        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim dataProvider As New ReaderDataProvider(reader)
            If reader.Read() Then Throw New DeletedScheduleException(dataProvider.GetInt("id"))
        End Using
    End Sub

    ''' <summary>
    ''' Gets the schedule logs for the given schedule between the given dates.
    ''' Note that the dates used here are exclusive.
    ''' </summary>
    ''' <param name="scheduleId">The ID of the schedule for which the logs
    ''' are required.</param>
    ''' <param name="after">The date after which the logs are required.
    ''' </param>
    ''' <param name="before">The date before which the logs are required.
    ''' </param>
    ''' <param name="reasons">A collection of activation reasons to filter the
    ''' logs on. A null or empty value indicates that all logs should be
    ''' returned regardless of their activation reason.</param>
    ''' <returns>A non-null collection of historical logs for execution
    ''' instances of the specified schedule which fell between the given
    ''' dates.</returns>
    <SecuredMethod(False)>
    Public Function SchedulerGetLogs(ByVal scheduleId As Integer,
     ByVal after As DateTime, ByVal before As DateTime,
     ByVal ParamArray reasons() As TriggerActivationReason) _
     As ICollection(Of HistoricalScheduleLog) Implements IServer.SchedulerGetLogs
        CheckPermissions()
        ' Set the boundary dates to their SQL equivalents
        ' If not a boundary date, alter the times by 1s to deal with the fact that
        ' the BETWEEN clause in SQL is inclusive and these values are supposed to be exclusive
        If after = DateTime.MinValue Then after = MinSQLDate Else after = after.AddSeconds(1)
        If before = DateTime.MaxValue Then before = MaxSQLDate Else before = before.AddSeconds(-1)

        Using con = GetConnection()

            Dim cmd As New SqlCommand()
            With cmd.Parameters
                .AddWithValue("@scheduleid", scheduleId)
                .AddWithValue("@startTime", after)
                .AddWithValue("@endTime", before)
            End With

            Dim sb As New StringBuilder(
             " select id from BPAScheduleLog" &
             " where scheduleid=@scheduleid" &
             "   and instancetime between @startTime and @endTime")
            If reasons.Length > 0 Then
                sb.Append("   and firereason in (")
                Dim i As Integer = 0
                For Each r As TriggerActivationReason In reasons
                    If i > 0 Then sb.Append(","c)
                    i += 1
                    sb.Append("@reason").Append(i)
                    cmd.Parameters.AddWithValue("@reason" & i, r)
                Next
                sb.Append(")"c)
            End If

            cmd.CommandText = sb.ToString()

            Dim logIds As New List(Of Integer)
            Using reader = con.ExecuteReturnDataReader(cmd)
                While reader.Read() : logIds.Add(reader.GetInt32(0)) : End While
            End Using

            If logIds.Count = 0 Then _
             Return GetEmpty.ICollection(Of HistoricalScheduleLog)()

            Return SchedulerGetLogsById(con, logIds)(scheduleId)

        End Using

    End Function

    ''' <summary>
    ''' Gets the schedule logs which correspond to the given log IDs, using
    ''' the specified connection.
    ''' </summary>
    ''' <param name="con">The connection to the database to use to retrieve
    ''' the logs.</param>
    ''' <param name="ids">The IDs of the logs required.</param>
    ''' <returns>A dictionary mapping the collection of logs to their
    ''' corresponding schedule ID.</returns>
    Private Function SchedulerGetLogsById(
      ByVal con As IDatabaseConnection, ByVal ids As ICollection(Of Integer)) _
      As IDictionary(Of Integer, ICollection(Of HistoricalScheduleLog))

        ' The logs, referenced by schedule id - <scheduleid: list-of-logs>
        Dim logs As New clsGeneratorDictionary(Of Integer, List(Of HistoricalScheduleLog))

        ' Map referenced by log ID - <logid: log>
        Dim map As New Dictionary(Of Integer, HistoricalScheduleLog)

        Dim cmd As New SqlCommand()

        Dim index As Integer = 0
        While index < ids.Count

            Dim sb As New StringBuilder(
             " select id," &
             "   scheduleid," &
             "   instancetime," &
             "   firereason," &
             "   servername as schedulername" &
             " from BPAScheduleLog" &
             " where id in (")
            cmd.Parameters.Clear()

            For Each id As Integer In New clsWindowedEnumerable(Of Integer)(ids, index, MaxSqlParams)
                cmd.Parameters.AddWithValue("@id" & index, id)
                sb.AppendFormat("@id{0},", index)
                index += 1
            Next
            sb.Length -= 1
            sb.Append(")")
            cmd.CommandText = sb.ToString()

            Using reader = con.ExecuteReturnDataReader(cmd)

                While reader.Read()

                    ' Each row returned contains the details for a single log
                    ' Create and add it to the list for that schedule in the dictionary.
                    Dim provider As New ReaderDataProvider(reader)

                    Dim log As New HistoricalScheduleLog(provider)

                    ' Add to the appopriate entry in the dictionary
                    logs(provider.GetValue("scheduleid", 0)).Add(log)

                    ' Also add to the flat list (for dealing with entries later)
                    map(log.Id) = log

                End While

            End Using

        End While

        ' We now have a dictionary containing all the logs that we need to
        ' populate with entries.
        index = 0
        While index < map.Count
            cmd.Parameters.Clear()

            Dim sb As New StringBuilder(
             " select * " &
             " from BPAScheduleLogEntry " &
             " where schedulelogid in (")

            For Each log As HistoricalScheduleLog In
             New clsWindowedEnumerable(Of HistoricalScheduleLog)(map.Values, index, MaxSqlParams)

                sb.AppendFormat("@logid{0},", index)
                cmd.Parameters.AddWithValue("@logid" & index, log.Id)
                index += 1

            Next
            sb.Length -= 1 ' remove that last comma
            sb.Append(") order by id")
            cmd.CommandText = sb.ToString()
            Using reader = con.ExecuteReturnDataReader(cmd)
                While reader.Read()
                    Dim provider As New ReaderDataProvider(reader)
                    map(provider.GetValue("schedulelogid", 0)).Add(New ScheduleLogEntry(provider))
                End While
            End Using
        End While

        Dim collLogs As New Dictionary(Of Integer, ICollection(Of HistoricalScheduleLog))
        For Each key As Integer In logs.Keys
            collLogs(key) = logs(key)
        Next
        Return collLogs

    End Function

    ''' <summary>
    ''' Creates a new schedule log with the given ID and instant in time
    ''' on the database, returning the newly created schedule log ID.
    ''' </summary>
    ''' <param name="scheduleId">The ID of the schedule for which this
    ''' log is being created.</param>
    ''' <param name="instant">The instant in time that this log represents
    ''' the execution for.</param>
    ''' <param name="reason">The reason that the schedule was activated.</param>
    ''' <returns>The ID of the newly created schedule log</returns>
    ''' <param name="schedulerName">The name of the scheduler which is creating the
    ''' log.</param>
    ''' <exception cref="AlreadyActivatedException">If an entry was found on the
    ''' database for the specified schedule at the specified instance time.
    ''' </exception>
    <SecuredMethod(True)>
    Public Function SchedulerCreateLog(
     ByVal scheduleId As Integer, ByVal instant As DateTime,
     ByVal reason As TriggerActivationReason, ByVal schedulerName As String) As Integer Implements IServer.SchedulerCreateLog
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand(
              " insert into BPAScheduleLog" &
              "   (scheduleid, instancetime, firereason, heartbeat, servername) " &
              "   values (@scheduleid, @instant, @reason, getutcdate(), @server); " &
              " select scope_identity()")
            With cmd.Parameters
                .AddWithValue("@scheduleid", scheduleId)
                .AddWithValue("@instant", instant)
                .AddWithValue("@reason", reason)
                .AddWithValue("@server", schedulerName)
            End With

            ' The only UNQ failure in BPAScheduleLog is if an entry for the same
            ' scheduleid/instancetime already exists
            Dim result = con.ExecuteScalarExpectError(cmd, DatabaseErrorCode.UniqueConstraintError)
            If result Is Nothing Then Throw New AlreadyActivatedException(
                My.Resources.clsServer_Schedule0AlreadyActivatedAt1U, scheduleId, instant)
            Return CInt(result)
        End Using

    End Function

    ''' <summary>
    ''' Pulses the schedule log with the given ID, effectively indicating that
    ''' the log is still in use - ie. the schedule is still being executed by
    ''' the scheduler.
    ''' </summary>
    ''' <param name="logId">The log ID which should be pulsed with the current
    ''' date/time</param>
    <SecuredMethod(True)>
    Public Sub SchedulerPulseLog(ByVal logId As Integer, ByVal schedulerName As String) Implements IServer.SchedulerPulseLog
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand(
             " update BPAScheduleLog set" &
             "   heartbeat = getutcdate()," &
             "   servername = @server" &
             " where id = @id")
            With cmd.Parameters
                .AddWithValue("@id", logId)
                .AddWithValue("@server", schedulerName)
            End With
            con.Execute(cmd)
        End Using
    End Sub

    ''' <summary>
    ''' Writes a schedule log event record to the database with the given
    ''' values.
    ''' </summary>
    ''' <param name="logId">The ID of the schedule log that this event is part of
    ''' </param>
    ''' <param name="entry">The entry to be added - note that the date on this
    ''' entry is ignored and the current date/time on the database is used
    ''' for the entry.</param>
    ''' <returns>The date/time of the entry after it has been inserted onto the
    ''' database - note that this time will be in UTC.</returns>
    <SecuredMethod(True)>
    Public Function SchedulerAddLogEntry(
     ByVal logId As Integer,
     ByVal entry As ScheduleLogEntry) As Date Implements IServer.SchedulerAddLogEntry
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            Dim dt As Date = SchedulerAddLogEntry(con, logId, entry)
            con.CommitTransaction()
            Return dt
        End Using

    End Function

    ''' <summary>
    ''' Writes a schedule log event record to the database with the given
    ''' values.
    ''' </summary>
    ''' <param name="logId">The ID of the schedule log that this event is part of.
    ''' </param>
    ''' <param name="entry">The entry to be added - note that the date on this
    ''' entry is ignored and the current date/time on the database is used
    ''' for the entry.</param>
    ''' <returns>The date/time of the entry after it has been inserted onto the
    ''' database - note that this time will be in UTC.</returns>
    Private Function SchedulerAddLogEntry(
     ByVal con As IDatabaseConnection,
     ByVal logId As Integer,
     ByVal entry As ScheduleLogEntry) As Date

        Dim cmd As New SqlCommand(
         " declare @sessionid integer; " &
         " select @sessionid=sessionnumber from BPASession where sessionnumber = @sessno; " &
         "" &
         " insert into BPAScheduleLogEntry" &
         "   (schedulelogid, entrytype, entrytime, taskid, " &
         "     logsessionnumber, terminationreason, stacktrace) values" &
         "   (@logid, @entrytype, GetUTCDate(), @taskid, @sessionid, @termreason, @stacktrace); " &
         " select l.scheduleid, e.entrytime " &
         "   from BPAScheduleLog l " &
         "     join BPAScheduleLogEntry e on l.id = e.schedulelogid " &
         " where e.id = scope_identity()"
        )

        'The termination reason is limited to 255 characters in the database. We need
        'to make sure we don't try and insert more than that. (See bug #8219)
        Dim reason As String = Nothing
        If entry.HasTerminationReason() Then
            reason = entry.TerminationReason
            If reason.Length() > 255 Then
                reason = reason.Substring(0, 255)
            End If
        End If

        With cmd.Parameters
            .AddWithValue("@logid", logId)
            .AddWithValue("@entrytype", CInt(entry.EntryType))
            .AddWithValue("@taskid", IIf(entry.HasTask(), entry.TaskId, DBNull.Value))
            .AddWithValue("@sessno", IIf(entry.HasSession(), entry.SessionLogNo, DBNull.Value))
            .AddWithValue("@termreason", IIf(reason IsNot Nothing, reason, DBNull.Value))
            .AddWithValue("@stacktrace", IIf(entry.HasStackTrace(), entry.StackTrace, DBNull.Value))
        End With

        Debug.Print("Writing log entry: {0}", entry)
        Dim schedId As Integer = 0
        Dim dt As Date = Nothing

        Using reader = con.ExecuteReturnDataReader(cmd)
            If reader.Read() Then
                schedId = CInt(reader("scheduleid"))
                dt = CDate(reader("entrytime"))
            End If
        End Using
        entry.EntryTime = dt

        ' Write any alerts that are tied in with this log entry.
        Select Case entry.EntryType
            Case ScheduleLogEventType.ScheduleStarted
                CreateScheduleAlert(con, AlertEventType.ScheduleStarted, schedId, entry.TaskId)
            Case ScheduleLogEventType.ScheduleCompleted
                CreateScheduleAlert(con, AlertEventType.ScheduleCompleted, schedId, entry.TaskId)
            Case ScheduleLogEventType.ScheduleTerminated
                CreateScheduleAlert(con, AlertEventType.ScheduleTerminated, schedId, entry.TaskId)
            Case ScheduleLogEventType.TaskStarted
                CreateScheduleAlert(con, AlertEventType.TaskStarted, schedId, entry.TaskId)
            Case ScheduleLogEventType.TaskCompleted
                CreateScheduleAlert(con, AlertEventType.TaskCompleted, schedId, entry.TaskId)
            Case ScheduleLogEventType.TaskTerminated
                CreateScheduleAlert(con, AlertEventType.TaskTerminated, schedId, entry.TaskId)
        End Select


        Return dt

    End Function

#End Region

#Region "Timetables and Reports"

    ''' <summary>
    ''' Gets a list of clsScheduleList from the database with the given ScheduleListType
    ''' </summary>
    ''' <param name="type">The type of list to get</param>
    ''' <returns>A list of clsScheduleList but with only the name and ID populated.</returns>
    <SecuredMethod(False)>
    Public Function SchedulerGetScheduleLists(ByVal type As ScheduleListType) As ICollection(Of ScheduleList) Implements IServer.SchedulerGetScheduleLists
        CheckPermissions()
        Dim ids As New List(Of Integer)
        Using con = GetConnection()

            Dim cmd As SqlCommand = New SqlCommand("SELECT id FROM BPAScheduleList WHERE listtype=@listtype")
            cmd.Parameters.AddWithValue("@listtype", type)

            Using dr = con.ExecuteReturnDataReader(cmd)
                While dr.Read
                    ids.Add(CInt(dr("id")))
                End While
            End Using

            Return SchedulerGetScheduleListsById(con, ids)

        End Using

    End Function

    ''' <summary>
    ''' Gets a list of all schedule lists which correspond to the given IDs.
    ''' </summary>
    ''' <param name="con">The connection to use to access the lists.</param>
    ''' <param name="ids">The collection of IDs to retrieve the lists for.</param>
    ''' <returns>A list of schedulelist objects corresponding to the IDs specified.
    ''' </returns>
    Private Function SchedulerGetScheduleListsById(
     ByVal con As IDatabaseConnection, ByVal ids As IList(Of Integer)) _
     As IList(Of ScheduleList)

        Dim allLists As New List(Of ScheduleList)
        If ids.Count = 0 Then Return allLists

        Dim sb As New StringBuilder("SELECT * FROM BPAScheduleList WHERE id in (")
        Dim cmd As New SqlCommand()
        Dim i As Integer = 0
        For Each id As Integer In ids
            i += 1
            sb.AppendFormat("@id{0},", i)
            cmd.Parameters.AddWithValue("id" & i, id)
        Next
        sb.Length -= 1
        cmd.CommandText = sb.Append(")").ToString()

        Using dr = con.ExecuteReturnDataReader(cmd)
            While dr.Read()

                Dim provider As New ReaderDataProvider(dr)
                Dim schedList As New ScheduleList()
                schedList.ID = provider.GetValue("id", 0)
                schedList.ListType = provider.GetValue("listtype", ScheduleListType.Report)
                schedList.Name = provider.GetValue("name", "")
                schedList.Description = provider.GetValue("description", "")
                schedList.DaysDistance = provider.GetValue("daysdistance", 0)
                schedList.RelativeDate = provider.GetValue("relativedate", ScheduleRelativeDate.None)
                schedList.AbsoluteDate = provider.GetValue("absolutedate", DateTime.MinValue)
                schedList.AllSchedules = provider.GetValue("allschedules", False)

                allLists.Add(schedList)

            End While
        End Using

        cmd.CommandText = "SELECT * FROM BPAScheduleListSchedule WHERE schedulelistid=@id"
        Dim idParam As SqlParameter = cmd.Parameters.Add("@id", SqlDbType.Int)
        For Each list As ScheduleList In allLists
            idParam.Value = list.ID
            Using dr = con.ExecuteReturnDataReader(cmd)
                While dr.Read()
                    list.ScheduleIds.Add(CInt(dr("scheduleid")))
                End While
            End Using
        Next

        Return allLists

    End Function

    ''' <summary>
    ''' Gets the entire schedule list by the given ID on the specified connection
    ''' </summary>
    ''' <param name="con">The connection with which the list should be retrieved.
    ''' </param>
    ''' <param name="id">The ID of the schedule list required.</param>
    ''' <returns>A clsScheduleList object representing the list with the given ID,
    ''' or null if no such list exists on the database.</returns>
    ''' <remarks>Note that the store is <em>not</em> set in the returned list, and
    ''' should be set at the first opportunity.</remarks>
    Private Function SchedulerGetScheduleListById(
     ByVal con As IDatabaseConnection, ByVal id As Integer) As ScheduleList

        Dim all As IList(Of ScheduleList) =
         SchedulerGetScheduleListsById(con, New Integer() {id})
        If all.Count = 0 Then Return Nothing
        Return all(0)

    End Function

    ''' <summary>
    ''' Gets the schedule list corresponding to the given ID, or null if the ID
    ''' did not match any schedule lists on the database.
    ''' </summary>
    ''' <param name="id">The ID of the schedule list to retrieve.</param>
    ''' <returns>The schedule list corresponding to the given ID.</returns>
    <SecuredMethod(False)>
    Public Function SchedulerGetScheduleList(ByVal id As Integer) As ScheduleList Implements IServer.SchedulerGetScheduleList
        CheckPermissions()
        Using con = GetConnection()
            Return SchedulerGetScheduleListById(con, id)
        End Using
    End Function

    ''' <summary>
    ''' Gets the complete schedule list corresponding to the given name and
    ''' type, or null if no such list exists.
    ''' </summary>
    ''' <param name="name">The name of the required list.</param>
    ''' <param name="type">The type of the required list.</param>
    ''' <returns>The schedule list corresponding to the given name and type,
    ''' or null if no such list existed on the database.</returns>
    <SecuredMethod(False)>
    Public Function SchedulerGetScheduleList(
     ByVal name As String, ByVal type As ScheduleListType) As ScheduleList Implements IServer.SchedulerGetScheduleList
        CheckPermissions()
        Using con = GetConnection()

            Dim cmd As New SqlCommand(
             "select id from BPAScheduleList where name=@name and listtype=@type")

            cmd.Parameters.AddWithValue("@name", name)
            cmd.Parameters.AddWithValue("@type", type)

            Dim oid As Object = con.ExecuteReturnScalar(cmd)
            If oid Is Nothing OrElse Convert.IsDBNull(oid) Then Return Nothing
            Return SchedulerGetScheduleListById(con, CInt(oid))

        End Using

    End Function

    ''' <summary>
    ''' Creates a clsScheduleList
    ''' </summary>
    ''' <param name="list">The clsScheduleList to create</param>
    <SecuredMethod(Permission.Scheduler.EditSchedule)>
    Public Function SchedulerCreateScheduleList(ByVal list As ScheduleList) As Integer Implements IServer.SchedulerCreateScheduleList
        CheckPermissions()
        Try
            CreateScheduleList(list)
        Catch ex As SqlException
            ConvertKnownErrorsAndThrow(list.Name, ex)
        End Try
    End Function

    Private Function CreateScheduleList(ByRef list As ScheduleList) As Integer
        Using con = GetConnection()

            Using cmd As New SqlCommand(
            "INSERT INTO BPAScheduleList (" &
            "listtype, " &
            "name, " &
            "description, " &
            "daysdistance, " &
            "relativedate, " &
            "absolutedate, " &
            "allschedules) " &
            "VALUES (" &
            "@listtype, " &
            "@name, " &
            "@description, " &
            "@daysdistance, " &
            "@relativedate, " &
            "@absolutedate, " &
            "@allschedules);" &
            "SELECT scope_identity()")

                With cmd.Parameters
                    .AddWithValue("@listtype", list.ListType)
                    .AddWithValue("@name", list.Name)
                    .AddWithValue("@description", list.Description)
                    .AddWithValue("@daysdistance", list.DaysDistance)
                    .AddWithValue("@relativedate", list.RelativeDate)
                    .AddWithValue("@absolutedate", clsDBConnection.UtilDateToSqlDate(list.AbsoluteDate, True, True))
                    .AddWithValue("@allschedules", list.AllSchedules)
                End With

                list.ID = CInt(con.ExecuteReturnScalar(cmd))
            End Using

            If Not list.AllSchedules Then
                Using cmd As New SqlCommand("INSERT INTO BPAScheduleListSchedule (schedulelistid, scheduleid) VALUES (@id, @scheduleid)")
                    Dim p As New SqlParameter("@scheduleid", SqlDbType.Int)
                    With cmd.Parameters
                        .Add(p)
                        .AddWithValue("@id", list.ID)
                    End With
                    For Each scheduleId As Integer In list.ScheduleIds
                        p.Value = scheduleId
                        con.Execute(cmd)
                    Next
                End Using
            End If

            Return list.ID
        End Using
    End Function

    Private Sub ConvertKnownErrorsAndThrow(scheduleListName As String, sqlException As SqlException)
        Dim duplicateConstraintViolationCode = 2627
        If sqlException.Number = duplicateConstraintViolationCode Then
            Throw New BluePrism.Server.Domain.Models.NameAlreadyExistsException(
                        String.Format(GetLocalisedResourceString("TheName0IsAlreadyInUse"), scheduleListName), sqlException)
        End If

        Throw sqlException
    End Sub

    ''' <summary>
    ''' Updates a clsScheduleList
    ''' </summary>
    ''' <param name="list">the clsScheduleList to update</param>
    <SecuredMethod(Permission.Scheduler.EditSchedule)>
    Public Sub SchedulerUpdateScheduleList(ByVal list As ScheduleList) Implements IServer.SchedulerUpdateScheduleList
        CheckPermissions()
        Try
            UpdateScheduleList(list)
        Catch sqlex As SqlException
            ConvertKnownErrorsAndThrow(list.Name, sqlex)
        End Try
    End Sub

    Private Sub UpdateScheduleList(ByRef list As ScheduleList)
        Using con = GetConnection()
            con.BeginTransaction()

            Using cmd As New SqlCommand(
             "UPDATE BPAScheduleList SET " &
             "name=@name, " &
             "description=@description, " &
             "daysdistance=@daysdistance, " &
             "relativedate=@relativedate, " &
             "absolutedate=@absolutedate, " &
             "allschedules=@allschedules " &
             "WHERE id=@id")

                With cmd.Parameters
                    .AddWithValue("@name", list.Name)
                    .AddWithValue("@description", list.Description)
                    .AddWithValue("@daysdistance", list.DaysDistance)
                    .AddWithValue("@relativedate", list.RelativeDate)
                    .AddWithValue("@absolutedate", clsDBConnection.UtilDateToSqlDate(list.AbsoluteDate, True, True))
                    .AddWithValue("@allschedules", list.AllSchedules)
                    .AddWithValue("@id", list.ID)
                End With

                con.Execute(cmd)
            End Using

            Using cmd As New SqlCommand("DELETE FROM BPAScheduleListSchedule WHERE schedulelistid=@id")
                cmd.Parameters.AddWithValue("@id", list.ID)
                con.Execute(cmd)
            End Using

            If Not list.AllSchedules Then
                Using cmd As New SqlCommand("INSERT INTO BPAScheduleListSchedule (schedulelistid, scheduleid) VALUES (@id, @scheduleid)")
                    Dim p As New SqlParameter("@scheduleid", SqlDbType.Int)
                    With cmd.Parameters
                        .Add(p)
                        .AddWithValue("@id", list.ID)
                    End With
                    For Each scheduleId As Integer In list.ScheduleIds
                        p.Value = scheduleId
                        con.Execute(cmd)
                    Next
                End Using
            End If

            con.CommitTransaction()

        End Using
    End Sub

    ''' <summary>
    ''' Deletes a clsScheduleList
    ''' </summary>
    ''' <param name="listid">the id of the ScheduleList to delete</param>
    <SecuredMethod(Permission.Scheduler.EditSchedule)>
    Public Sub SchedulerDeleteScheduleList(ByVal listid As Integer) Implements IServer.SchedulerDeleteScheduleList
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            Using cmd As New SqlCommand("DELETE FROM BPAScheduleList WHERE id=@id")
                cmd.Parameters.AddWithValue("@id", listid)
                con.Execute(cmd)
            End Using

            Using cmd As New SqlCommand("DELETE FROM BPAScheduleListSchedule WHERE schedulelistid=@id")
                cmd.Parameters.AddWithValue("@id", listid)
                con.Execute(cmd)
            End Using

            con.CommitTransaction()

        End Using
    End Sub

  
    <SecuredMethod(False)>
    Public Function SchedulerGetSessionsWithinTask(ByVal taskId As Integer) As ICollection(Of Server.Domain.Models.ScheduledSession) _
        Implements IServer.SchedulerGetSessionsWithinTask
        CheckPermissions()

        Using con = GetConnection()
            ThrowIfTaskNotExist(con, taskId)
            ThrowIfScheduleHasBeenDeletedForTask(con, taskId)
            Dim cmd = mDatabaseCommandFactory(
                "select " &
                "p.[name] as ProcessName, " &
                "ts.resourcename as ResourceName " &
                "from BPATask t " &
                "join BPATaskSession ts on ts.taskid = t.id " &
                "join BPAProcess p on p.processid = ts.processid " &
                "where t.id = @id " &
                "order by ts.id")
            cmd.AddParameter("@id", taskId)
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim dataProvider As New ReaderDataProvider(reader)
                Dim sessionsList = New List(Of Server.Domain.Models.ScheduledSession)()
                While reader.Read()
                    Dim session = New Server.Domain.Models.ScheduledSession With
                    {
                        .ProcessName = dataProvider.GetString("ProcessName"),
                        .ResourceName = dataProvider.GetString("ResourceName")
                    }
                    sessionsList.Add(session)
                End While
                Return sessionsList
            End Using
        End Using
    End Function
#End Region

#End Region

    ''' <summary>
    ''' Updates the Scheduler configuration settings.
    ''' </summary>
    ''' <param name="active">Indicates whether the Scheduler is active or not</param>
    ''' <param name="checkSeconds">The number of seconds to check for missed schedules</param>
    ''' <param name="retryTimes">The number of seconds to wait between retries</param>
    ''' <param name="retryPeriod">The number of times to retry</param>
    <SecuredMethod(Permission.SystemManager.System.Scheduler)>
    Public Sub SetSchedulerConfig(active As Boolean, checkSeconds As Integer,
                                  retryTimes As Integer, retryPeriod As Integer) Implements IServer.SetSchedulerConfig
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            SetSystemPref(con, PreferenceNames.Scheduler.Active, active)
            SetSystemPref(con, PreferenceNames.Scheduler.CheckSeconds, checkSeconds)
            SetSystemPref(con, PreferenceNames.Scheduler.RetryTimes, retryTimes)
            SetSystemPref(con, PreferenceNames.Scheduler.RetryPeriod, retryPeriod)

            Dim audit = String.Format(My.Resources.clsServer_Active0MissedScheduleCheck1RetryPeriod2RetryAttempts3,
                                      active.ToString(), checkSeconds, retryPeriod, retryTimes)
            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifySchedulerSettings, audit)
            con.CommitTransaction()
        End Using
    End Sub

End Class
