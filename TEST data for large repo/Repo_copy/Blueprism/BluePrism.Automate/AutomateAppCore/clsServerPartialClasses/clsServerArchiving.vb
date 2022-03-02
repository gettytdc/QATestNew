Imports System.Data.SqlClient
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.Data
Imports BluePrism.Server.Domain.Models

Partial Public Class clsServer

    ''' <summary>
    ''' Get the current archiving resource, used for automatic archiving.
    ''' </summary>
    ''' <param name="id">The new resource ID.</param>
    ''' <param name="folder">The new folder.</param>
    ''' <param name="age">The age of items to archive, e.g. "6m".</param>
    ''' <param name="delete">True to delete old items, False to archive them to
    ''' files in 'folder'.</param>
    <SecuredMethod(Permission.SystemManager.System.Archiving)>
    Public Sub SetAutoArchivingSettings(ByVal id As Guid, ByVal folder As String, ByVal age As String, ByVal delete As Boolean, resourceName As String) Implements IServer.SetAutoArchivingSettings
        CheckPermissions()
        Using con = GetConnection()
            Using cmd As New SqlCommand(
            "UPDATE BPASysConfig " &
            "SET ArchivingResource=@id, ArchivingFolder=@folder, ArchivingAge=@age, ArchivingDelete=@delete")
                cmd.Parameters.AddWithValue("@id", IIf(id = Guid.Empty, DBNull.Value, id))
                cmd.Parameters.AddWithValue("@folder", folder)
                cmd.Parameters.AddWithValue("@age", age)
                cmd.Parameters.AddWithValue("@delete", delete)
                con.Execute(cmd)
            End Using

            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifyArchive, String.Format(My.Resources.ctlSystemArchiving_NewSettingsAreModeAutomaticResource0ArchiveFolder1Age2ArchiveMode3,
                                        resourceName, folder, age, CStr(IIf(delete, My.Resources.ctlSystemArchiving_Delete, My.Resources.ctlSystemArchiving_Export))), resourceName)
        End Using
    End Sub

    ''' <summary>
    ''' Get the current archiving resource, used for automatic archiving.
    ''' </summary>
    ''' <param name="resource">On return, contains the Resource ID.</param>
    ''' <param name="folder">On return, contains the folder.</param>
    ''' <param name="age">On return, contains the selected archiving age.</param>
    ''' <param name="delete">On return, contains True if deletion is required, False
    ''' if logs should be archived to 'folder'.</param>
    <SecuredMethod()>
    Public Sub GetAutoArchivingSettings(ByRef resource As Guid, ByRef folder As String, ByRef age As String, ByRef delete As Boolean) Implements IServer.GetAutoArchivingSettings
        CheckPermissions()
        Using con  = GetConnection()
            Using cmd As New SqlCommand( _
            "SELECT ArchivingResource, ArchivingFolder, ArchivingAge, ArchivingDelete " & _
            "FROM BPASysConfig")
                Dim r As DataRow = con.ExecuteReturnDataTable(cmd).Rows(0)
                If TypeOf (r("ArchivingResource")) Is DBNull Then
                    resource = Guid.Empty
                Else
                    resource = CType(r("ArchivingResource"), Guid)
                End If
                folder = CStr(r("ArchivingFolder"))
                age = CStr(r("ArchivingAge"))
                delete = CBool(r("ArchivingDelete"))
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Set the archiving mode.
    ''' </summary>
    ''' <param name="auto">True to set the archiving mode to 'auto-archive'; False to
    ''' set it to 'manual archive'.</param>
    <SecuredMethod(Permission.SystemManager.System.Archiving)>
    Public Sub SetAutoArchiving(ByVal auto As Boolean) Implements IServer.SetAutoArchiving
        CheckPermissions()
        Using con  = GetConnection()
            Using cmd As New SqlCommand("update BPASysConfig set ArchivingMode=@Mode")
                ' 0 = manual; 1 = auto
                cmd.Parameters.AddWithValue("@Mode", IIf(auto, 1, 0))
                con.Execute(cmd)
            End Using

            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifyArchive, If(auto, My.Resources.ctlSystemArchiving_NewSettingsAreModeAutomatic,
                                      My.Resources.ctlSystemArchiving_NewSettingsAreModeManual), String.Empty)
        End Using
    End Sub

    ''' <summary>
    ''' Gets whether the current archiving mode is 'auto-archiving'.
    ''' </summary>
    ''' <returns>True if the currently configured archiving mode is auto-archiving;
    ''' False if it is set to manual archiving.</returns>
    <SecuredMethod(Permission.SystemManager.System.Archiving)>
    Public Function IsAutoArchiving() As Boolean Implements IServer.IsAutoArchiving
        CheckPermissions()
        Using con  = GetConnection()
            Return IsAutoArchiving(con)
        End Using
    End Function

    ''' <summary>
    ''' Gets whether the current archiving mode is 'auto-archiving'.
    ''' </summary>
    ''' <param name="con">The connection over which to perform the check</param>
    ''' <returns>True if the currently configured archiving mode is auto-archiving;
    ''' False if it is set to manual archiving.</returns>
    Private Function IsAutoArchiving(ByVal con As IDatabaseConnection) As Boolean
        Using cmd As New SqlCommand("select ArchivingMode from BPASysConfig")
            ' 0 = manual; 1 = auto
            Return (IfNull(con.ExecuteReturnScalar(cmd), 0) = 1)
        End Using
    End Function

    ''' <summary>
    ''' Set the archiving last complete time.
    ''' </summary>
    ''' <param name="d">The new time.</param>
    <SecuredMethod(True)>
    Public Sub SetArchivingLastComplete(ByVal d As DateTime) Implements IServer.SetArchivingLastComplete
        CheckPermissions()
        Using con  = GetConnection()
            Using cmd As New SqlCommand( _
            "UPDATE BPASysConfig " & _
            "SET ArchivingLastAuto=@d")
                cmd.Parameters.AddWithValue("@d", d)
                con.Execute(cmd)
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Get the last time auto-archiving was completed.
    ''' </summary>
    ''' <returns>The time of last completion.</returns>
    <SecuredMethod(True)>
    Public Function GetArchivingLastComplete() As DateTime Implements IServer.GetArchivingLastComplete
        CheckPermissions()
        Using con  = GetConnection()
            Using cmd As New SqlCommand( _
            "SELECT ArchivingLastAuto " & _
            "FROM BPASysConfig")
                Dim r As Object = con.ExecuteReturnScalar(cmd)
                If IsDBNull(r) Then Return DateTime.MinValue
                Return CType(r, DateTime)
            End Using
        End Using
    End Function


    ''' <summary>
    ''' Checks to see if archiving is already in progress somewhere. The field 
    ''' BPASysConfig.ArchiveInProgress holds the name of the machine running an archive 
    ''' process. If that machine is still 'alive', the local machine cannot do any
    ''' archiving. An Exception is thrown if there is any reason why the archiving
    ''' can't proceed. Otherwise, it can.
    ''' </summary>
    ''' <param name="aliveInterval">The 'alive interval' - if a machine is marked as
    ''' running an archive operation, but it hasn't registered with the database 
    ''' within this many minutes, it will be considered 'inactive' and won't act as
    ''' a barrier to this machine proceeding.</param>
    ''' <exception cref="ArchiveAlreadyInProgressException">If any active machine has
    ''' an archive lock, and thus this machine cannot proceed with an archive
    ''' operation.</exception>
    <SecuredMethod(True)>
    Public Sub ArchiveCheckCanProceed(ByVal aliveInterval As Integer) Implements IServer.ArchiveCheckCanProceed
        CheckPermissions()

        Dim cmd As New SqlCommand( _
         " select s.ArchiveInProgress" & _
         " from BPAAliveResources a " & _
         "   left join BPASysConfig s on a.MachineName = s.ArchiveInProgress " & _
         " where s.ArchiveInProgress is not null " & _
         "   and a.LastUpdated >= @aliveDate")
        cmd.Parameters.AddWithValue("@aliveDate", Now.AddMinutes(-1 * aliveInterval))

        Using con  = GetConnection()

            For Each row As DataRow In con.ExecuteReturnDataTable(cmd).Rows
                Dim machine As String = CStr(row("ArchiveInProgress"))
                If machine = mLoggedInMachine Then
                    Throw New ArchiveAlreadyInProgressException(
                     My.Resources.clsServer_ThisMachineIsAlreadyPerformingAnArchivingProcess)
                Else
                    Throw New ArchiveAlreadyInProgressException(String.Format(
                     My.Resources.clsServer_TheMachineCalled0IsCurrentlyPerformingAnArchivingProcess,
                     machine))
                End If
            Next

        End Using

    End Sub

    ''' <summary>
    ''' Acquires an archive lock for the current logged in machine.
    ''' </summary>
    <SecuredMethod(True)>
    Public Sub AcquireArchiveLock() Implements IServer.AcquireArchiveLock
        CheckPermissions()
        Using con = GetConnection()
            SetArchiveLock(con, True)
        End Using
    End Sub

    ''' <summary>
    ''' Releases the archive lock held by this resource (or optionally any resource)
    ''' </summary>
    ''' <param name="force">If set to True then the lock is released regardless of
    ''' whether it is held by this resource or not</param>
    <SecuredMethod(True)>
    Public Sub ReleaseArchiveLock(Optional force As Boolean = False) Implements IServer.ReleaseArchiveLock
        CheckPermissions()
        Using con = GetConnection()
            If Not force Then
                SetArchiveLock(con, False)
            Else
                Dim resource = ""
                If IsAchiveLockSet(con, resource, Nothing) Then
                    con.BeginTransaction()
                    SetArchiveLock(con, False, force)
                    AuditRecordSysConfigEvent(con, SysConfEventCode.ReleaseArchiveLock,
                        String.Format(My.Resources.clsServer_ArchiveLockHeldByResource0WasManuallyReleased, resource))
                    con.CommitTransaction()
                End If
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Indicates whether the archive lock is currently in place (i.e. a resource is
    ''' currently performing an archive operation). If the lock is present then the
    ''' archiving resource name (and the time it last responded) are returned.
    ''' </summary>
    ''' <param name="resource">Returns the archiving resource name (if there is one)
    ''' </param>
    ''' <param name="lastUpdated">Returns the resources last response timestamp
    ''' (see BPAAliveResources)</param>
    ''' <returns>True if the archive lock is in place, otherwise False</returns>
    <SecuredMethod(True)>
    Public Function IsAchiveLockSet(ByRef resource As String, ByRef lastUpdated As DateTime) As Boolean _
      Implements IServer.IsArchiveLockSet
        CheckPermissions()
        Using con = GetConnection()
            Return IsAchiveLockSet(con, resource, lastUpdated)
        End Using
    End Function

    ''' <summary>
    ''' Indicates whether the archive lock is currently in place (i.e. a resource is
    ''' currently performing an archive operation). If the lock is present then the
    ''' archiving resource name (and the time it last responded) are returned.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="resource">Returns the archiving resource name (if there is one)
    ''' </param>
    ''' <param name="lastUpdated">Returns the resources last response timestamp
    ''' (see BPAAliveResources)</param>
    ''' <returns>True if the archive lock is in place, otherwise False</returns>
    Private Function IsAchiveLockSet(con As IDatabaseConnection, ByRef resource As String, ByRef lastUpdated As DateTime) As Boolean
        Dim cmd As New SqlCommand("
                select c.ArchiveInProgress, r.LastUpdated from bpasysconfig c
                    left join BPAAliveResources r on r.MachineName=c.ArchiveInProgress
                where c.ArchiveInProgress is not null")
        Using reader = CType(con.ExecuteReturnDataReader(cmd), SqlDataReader)
            If Not reader.HasRows Then Return False
            reader.Read()
            Dim prov = New ReaderDataProvider(reader)
            resource = prov.GetString("ArchiveInProgress")
            lastUpdated = prov.GetValue("LastUpdated", DateTime.MinValue)
            Return True
        End Using
    End Function

    ''' <summary>
    ''' Record (or unrecord) that archiving is in progress.
    ''' </summary>
    ''' <param name="lock">True to set the archive lock; False to release it.
    ''' </param>
    ''' <param name="forceRelease">If set to True then the lock is released
    ''' regardless of whether it is held by this resource or not (only applicable
    ''' when <paramref name="lock"/> is False)</param>
    Private Sub SetArchiveLock(con As IDatabaseConnection, lock As Boolean, Optional forceRelease As Boolean = False)

        Dim mach As String = mLoggedInMachine
        If mach Is Nothing AndAlso lock Then Throw New ArchiveLockFailedException(
         My.Resources.clsServer_TheMachineNameIsNotSetInThisSessionThisIsRequiredToAcquireAnArchiveLockPleaseRe)

        Dim cmd As New SqlCommand()
        If mach IsNot Nothing Then cmd.Parameters.AddWithValue("@name", mach)

        If lock Then
            cmd.CommandText =
             " update BPASysConfig set" &
             "   ArchiveInProgress = @name" &
             " where ArchiveInProgress is null or ArchiveInProgress = @name"

        ElseIf mach IsNot Nothing AndAlso Not forceRelease Then
            cmd.CommandText =
             " update BPASysConfig set" &
             "   ArchiveInProgress = null" &
             " where ArchiveInProgress = @name"

        Else ' ie. unlocking with a null machine name
            ' We have to assume that the 'release lock' is being called by the
            ' correct machine but we have no way to double check that.
            cmd.CommandText =
             " update BPASysConfig set" &
             "   ArchiveInProgress = null"
        End If

        ' Find out how many records we update and deal with it appropriately
        Select Case con.ExecuteReturnRecordsAffected(cmd)

            Case 1 : Return ' 1 is good - that is what we expect

            Case 0 ' Nothing updated - it seems it's locked. Find out who by for the error message
                cmd = New SqlCommand("select ArchiveInProgress from BPASysConfig")
                Dim lockedBy As String =
                 IfNull(con.ExecuteReturnScalar(cmd), My.Resources.clsServer_NoNameFound)
                Throw New ArchiveLockFailedException(
                 My.Resources.clsServer_ArchiveLockHeldByMachine0LockNotAcquiredForThisOperation,
                 lockedBy)

            Case Else
                Throw New BluePrismException(
                 My.Resources.clsServer_ThereAppearToBeDuplicateLinesInTheBPASysconfigTableThisTableShouldOnlyHaveOneLi)

        End Select

    End Sub

    ''' <summary>
    ''' Gets all the session logs corresponding to the given session numbers.
    ''' Note that if the given collection contains the same session number more than
    ''' once, only one session log object will be returned for that number.
    ''' </summary>
    ''' <param name="con">The connection over which the sessions should be retrieved.
    ''' </param>
    ''' <param name="numbers">The collection of session numbers for which the session
    ''' log objects are required.</param>
    ''' <returns>A collection of session logs corresponding to the session numbers
    ''' given in the <paramref name="numbers"/> parameter.</returns>
    Private Function GetSessionLogs(
     ByVal con As IDatabaseConnection, ByVal numbers As ICollection(Of Integer)) _
     As ICollection(Of clsSessionLog)

        If numbers.Count = 0 Then Return GetEmpty.ICollection(Of clsSessionLog)()

        Dim logs As New List(Of clsSessionLog)

        ' TODO: Nicer if this used BPVSessionInfo - but that doesn't include 'Archived' sessions. Does this need it?
        mSqlHelper.SelectMultipleIds(con, numbers,
         Sub(prov) logs.Add(New clsSessionLog(prov)),
         " select " &
         "   s.sessionid," &
         "   s.sessionnumber," &
         "   s.startdatetime," &
         "   s.enddatetime," &
         "   s.processid," &
         "   p.name as processname," &
         "   s.starterresourceid," &
         "   startres.name as starterresourcename," &
         "   s.starteruserid," &
         "   u.username as starterusername," &
         "   s.runningresourceid," &
         "   runres.name as runningresourcename," &
         "   s.runningosusername," &
         "   s.statusid," &
         "   s.starttimezoneoffset, " &
         "   s.endtimezoneoffset " &
         " from BPASession s" &
         "   left join BPAProcess p on s.ProcessId = p.processid" &
         "   left join BPAResource startres on s.StarterResourceId = startres.resourceid" &
         "   left join BPAResource runres on s.RunningResourceId = runres.resourceid" &
         "   left join BPAUser u on s.starteruserid = u.userid" &
         " where s.SessionNumber in ({multiple-ids})"
        )

        Return logs

    End Function

    ''' <summary>
    ''' Gets all the session logs corresponding to the given session IDs.
    ''' Note that if the given collection contains the same session ID more than
    ''' once, only one session log object will be returned for that ID.
    ''' </summary>
    ''' <param name="ids">The collection of IDs for which the session logs are
    ''' required.</param>
    ''' <returns>The collection of session logs corresponding to the given IDs.
    ''' </returns>
    <SecuredMethod(True)>
    Public Function GetSessionLogs(ByVal ids As ICollection(Of Guid)) _
     As ICollection(Of clsSessionLog) Implements IServer.GetSessionLogs
        CheckPermissions()
        If ids.Count = 0 Then Return New List(Of clsSessionLog)
        Using con = GetConnection()
            Dim sessNos As New List(Of Integer)
            mSqlHelper.SelectMultipleIds(
             con, ids,
             Sub(prov) sessNos.Add(prov.GetValue("sessionnumber", 0)),
             " select s.sessionnumber" &
             " from BPASession s" &
             " where s.sessionid in ({multiple-ids})"
            )
            Return GetSessionLogs(con, sessNos)
        End Using
    End Function

    ''' <summary>
    ''' Gets session log objects found in the current database which satisfy the
    ''' given filter.
    ''' </summary>
    ''' <param name="fil">The filter to apply to the session logs. May be null or
    ''' empty to indicate that the collection of logs should not be filtered.</param>
    ''' <returns>The session logs which satisfy the given filter.</returns>
    <SecuredMethod(Permission.SystemManager.Audit.ProcessLogs, Permission.SystemManager.Audit.BusinessObjectsLogs)>
    Public Function GetSessionLogs(ByVal fil As clsSessionLogFilter) _
     As ICollection(Of clsSessionLog) Implements IServer.GetSessionLogs
        CheckPermissions()
        Dim dt As DataTable
        Using con = GetConnection()
            dt = GetSessionLogsTable(con, fil)
        End Using
        Dim dr As New DataTableReader(dt)
        Dim prov As New ReaderDataProvider(dr)
        Dim logs As New List(Of clsSessionLog)
        While dr.Read()
            logs.Add(New clsSessionLog(prov))
        End While
        Return logs
    End Function

    ''' <summary>
    ''' Gets session log objects found in the current database which satisfy the
    ''' given filter.
    ''' </summary>
    ''' <param name="fil">The filter to apply to the session logs. May be null or
    ''' empty to indicate that the collection of logs should not be filtered.</param>
    ''' <returns>The session logs which satisfy the given filter.</returns>
    <SecuredMethod(True)>
    Public Function GetSessionLogsTable(ByVal fil As clsSessionLogFilter) As DataTable Implements IServer.GetSessionLogsTable
        CheckPermissions()
        Using con = GetConnection()
            Return GetSessionLogsTable(con, fil)
        End Using
    End Function

    ''' <summary>
    ''' Gets session log objects found in the current database which satisfy the
    ''' given filter.
    ''' </summary>
    ''' <param name="con">The connection over which the session logs should be
    ''' retrieved.</param>
    ''' <param name="fil">The filter to apply to the session logs. May be null or
    ''' empty to indicate that the collection of logs should not be filtered.</param>
    ''' <returns>The session logs which satisfy the given filter.</returns>
    Private Function GetSessionLogsTable(con As IDatabaseConnection, fil As clsSessionLogFilter) _
     As DataTable

        ' The basic query to retrieve the appropriate session data with no filters
        ' applied
        Dim cmd As New SqlCommand(
            "SELECT s.sessionid," &
            "   s.sessionnumber," &
            "   s.startdatetime," &
            "   s.enddatetime," &
            "   s.processid," &
            "   p.name as processname," &
            "   s.starterresourceid," &
            "   startres.name as starterresourcename," &
            "   s.starteruserid," &
            "   u.username as starterusername," &
            "   s.runningresourceid," &
            "   runres.name as runningresourcename," &
            "   s.runningosusername," &
            "   s.statusid," &
            "   stat.description as statustext" &
            " FROM BPVSession s " &
            "   JOIN BPAProcess p on s.processid = p.processid " &
            "   JOIN BPAStatus stat on s.statusid = stat.statusid" &
            "   LEFT JOIN BPAResource startres ON s.starterresourceid = startres.resourceid" &
            "   LEFT JOIN BPAResource runres ON s.runningresourceid = runres.resourceid" &
            "   LEFT JOIN BPAUser u ON s.starteruserid = u.userid"
            )
                                  

        ' Build up the constraints now - if there's a filter, go through the
        ' various constraints defined on the filter and apply them to the list.
        ' The list is added en masse after the constraints have been built.
        ' The parameter collection is built up as the constraints are checked
        Dim constraints As New List(Of String)
        If fil IsNot Nothing Then
            Dim params As SqlParameterCollection = cmd.Parameters
            If fil.ProcessType <> DiagramType.Unset Then
                constraints.Add("p.processtype = @proctype")
                params.AddWithValue("@proctype",
                 IIf(fil.ProcessType = DiagramType.Process, "P", "O"))
            End If
            If fil.SessionNo <> 0 Then
                constraints.Add("s.sessionnumber = @sessno")
                params.AddWithValue("@sessno", fil.SessionNo)
            End If
            If fil.StartDateRange IsNot Nothing Then
                Dim dr As clsDateRange = fil.StartDateRange
                constraints.Add("s.startdatetime between @startbegin and @startend")
                params.AddWithValue("@startbegin", clsDBConnection.UtilDateToSqlDate(dr.StartTime))
                params.AddWithValue("@startend", clsDBConnection.UtilDateToSqlDate(dr.EndTime))
            End If
            If fil.EndDateRange IsNot Nothing Then
                Dim dr As clsDateRange = fil.EndDateRange
                constraints.Add("s.enddatetime between @endbegin and @endend")
                params.AddWithValue("@endbegin", clsDBConnection.UtilDateToSqlDate(dr.StartTime))
                params.AddWithValue("@endend", clsDBConnection.UtilDateToSqlDate(dr.EndTime))
            End If
            If fil.ProcessName <> "" Then
                constraints.Add("p.name like @procname")
                params.AddWithValue("@procname", "%" & fil.ProcessName & "%")
            End If
            If fil.Status <> "" Then
                constraints.Add("stat.description like @status")
                params.AddWithValue("@status", "%" & fil.Status & "%")
            End If
            If fil.SourceResourceName <> "" Then
                constraints.Add("startres.name like @startresname")
                params.AddWithValue("@startresname", "%" & fil.SourceResourceName & "%")
            End If
            If fil.TargetResourceName <> "" Then
                constraints.Add("runres.name like @runresname")
                params.AddWithValue("@runresname", "%" & fil.TargetResourceName & "%")
            End If
            If fil.WindowsUser <> "" Then
                constraints.Add("s.runningosusername like @winuser")
                params.AddWithValue("@winuser", "%" & fil.WindowsUser & "%")
            End If

        End If

        ' If we have any constraints add them to the command text now
        If constraints.Count > 0 Then
            cmd.CommandText &= " WHERE " & CollectionUtil.Join(constraints, " AND ")
        End If
        Dim sortSql =  " ORDER BY startdatetime"
        Select Case fil.SortColumn
            CASE "colStart"
                sortSql =  " ORDER BY startdatetime"
            CASE "colEnd" 
                sortSql = " ORDER BY enddatetime"
            CASE "colProcess"
                sortSql = " ORDER BY processname"
            CASE "colStatus"
                sortSql = " ORDER BY stat.description"
            CASE "colSource"
            CASE "colTarget"
            CASE "colWinUser"

        End Select

        sortSql &= If(fil.SortDirection = ListSortDirection.Ascending, " ASC", " DESC")

        If fil.CurrentPage > -1 And fil.RowsPerPage > -1 Then

            'Add the paging constraints so only the required rows are returned
            cmd.CommandText = "SELECT COUNT(1) OVER() AS totalRows,  " & cmd.CommandText.Substring(6)
            cmd.CommandText &= sortSql
            cmd.CommandText &= " OFFSET @pageSize * (@pageNumber -1) ROWS FETCH NEXT @pageSize ROWS ONLY"
            cmd.Parameters.Add("@pageSize", SqlDbType.Int).Value = fil.RowsPerPage
            cmd.Parameters.Add("@pageNumber", SqlDbType.Int).Value = fil.CurrentPage
        Else
            cmd.CommandText &=sortSql
        End If



        ' Get the data back into a data table
        Dim dt As DataTable = con.ExecuteReturnDataTable(cmd)
        ' Set the session ID as the primary key - useful when searching the
        ' datatable later, and indicating that sessions are (should be) unique
        ' within the table
        dt.PrimaryKey = New DataColumn() {dt.Columns("sessionid")}

        Dim permission As String
        If fil.ProcessType = DiagramType.Object Then
            permission = Auth.Permission.SystemManager.Audit.BusinessObjectsLogs
        Else
            permission = Auth.Permission.SystemManager.Audit.ProcessLogs
        End If

        If Not mLoggedInUser.HasPermission(permission) Then
            Throw New PermissionException(My.Resources.clsServer_UnauthorizedUserDoesNotHaveTheRelevantPermission)
        End If


        Dim processPermsCache As New Dictionary(Of Guid, IMemberPermissions)
        Dim resourcePermsCache As New Dictionary(Of Guid, IGroupPermissions)

        For Each row As DataRow In dt.Rows
            Dim processId = CType(row("processid"), Guid)

            If Not processPermsCache.ContainsKey(processId) Then
                processPermsCache.Add(processId,
                                      GetEffectiveMemberPermissionsForProcess(processId))
            End If
            Dim mem = processPermsCache(processId)

            Dim resourceId = CType(row("runningresourceid"), Guid)
            If Not resourcePermsCache.ContainsKey(resourceId) Then
                resourcePermsCache.Add(resourceId,
                                       GetEffectiveGroupPermissionsForResource(resourceId))
            End If
            Dim rec = resourcePermsCache(resourceId)

            If (mem.IsRestricted AndAlso
                Not mem.HasAnyPermissions(mLoggedInUser)) OrElse
               (rec.IsRestricted AndAlso
                Not rec.HasPermission(mLoggedInUser, Auth.Permission.Resources.ImpliedViewResource)) Then
                row.Delete()
            End If
        Next

        ' And return it
        Return dt

    End Function

    ''' <summary>
    ''' Gets all the session logs which occurred between the given dates, and which
    ''' ran the given process (or any session if no process is given)
    ''' </summary>
    ''' <param name="fromDate">The first date from which session logs are required.
    ''' </param>
    ''' <param name="toDate">The last at which session logs are required.</param>
    ''' <param name="processId">The ID of the process for which the session logs
    ''' are required. If this is empty, then all sessions between the two dates
    ''' will be retrieved.</param>
    ''' <returns>A collection of session log objects representing the sessions that
    ''' occurred in between the given dates.</returns>
    <SecuredMethod(True)>
    Public Function GetSessionLogs(
     ByVal fromDate As DateTime, ByVal toDate As DateTime, ByVal processId As Guid) _
     As ICollection(Of clsSessionLog) Implements IServer.GetSessionLogs
        CheckPermissions()

        Using con = GetConnection()

            Dim cmd As New SqlCommand(
             " select s.sessionnumber" &
             " from BPVSession s" &
             " where s.startdatetime >= @from and s.enddatetime <= @to" &
             "   and (@processid is null or s.processid = @processid)"
            )

            With cmd.Parameters
                .AddWithValue("@from", clsDBConnection.UtilDateToSqlDate(fromDate, False, False))
                .AddWithValue("@to", clsDBConnection.UtilDateToSqlDate(toDate, False, False))
                .AddWithValue("@processid", IIf(processId = Nothing, DBNull.Value, processId))
            End With

            Dim numbers As New List(Of Integer)
            Using reader = con.ExecuteReturnDataReader(cmd)
                While reader.Read()
                    numbers.Add(reader.GetInt32(0))
                End While
            End Using

            Return GetSessionLogs(con, numbers)

        End Using

    End Function

    ''' <summary>
    ''' Restores the given session log (but not its entry data) into the database,
    ''' giving it a new session number.
    ''' </summary>
    ''' <param name="log">The log which should have a BPASession record restored to
    ''' represent it on the database. Note that the new session number is *not* 
    ''' assigned to the log object by this method.</param>
    ''' <returns>The new session number assigned to the given session log in the
    ''' database..</returns>
    ''' <remarks>Note that no entries are restored as part of this session log
    ''' restoration, only the log and its own directly owned data.</remarks>
    ''' <exception cref="SessionAlreadyExistsException">If a session log with the
    ''' same session ID as the log provided already exists on the database.
    ''' </exception>
    ''' <exception cref="KeyNotFoundException">If a foreign key constraint failed,
    ''' meaning that a process, session or other related data didn't exist for the
    ''' given session log.</exception>
    <SecuredMethod(True)>
    Public Function RestoreSessionLog(ByVal log As clsSessionLog) As Integer Implements IServer.RestoreSessionLog
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand(
             " insert into BPASession " &
             "   (sessionid, startdatetime, enddatetime, processid, " &
             "    starterresourceid, starteruserid, runningresourceid, runningosusername, " &
             "    statusid, starttimezoneoffset, endtimezoneoffset)" &
             " values (@sessionid, @startdatetime, @enddatetime, @processid, " &
             "   @starterresourceid, @starteruserid, @runningresourceid, @runningosusername," &
             "   @statusid, @starttimezoneoffset, @endtimezoneoffset);" & _
 _
             " select scope_identity();"
            )
            log.SetInto(cmd.Parameters)
            Try
                Return CInt(con.ExecuteReturnScalar(cmd))

            Catch sqle As SqlException _
             When sqle.Number = DatabaseErrorCode.UniqueConstraintError
                ' There's 2 unique constraints on BPASession - sessionid and
                ' sessionnumber. Since we're using the IDENTITY increment for
                ' sessionnumber, this means that it failed due to sessionid already
                ' being present.
                Throw New SessionAlreadyExistsException(log.SessionId)

            Catch sqle As SqlException _
             When sqle.Number = DatabaseErrorCode.ForeignKeyError
                ' The session log has various FK relations to process, resource etc.
                Throw New KeyNotFoundException(
                 My.Resources.clsServer_ThisSessionLogRequiresDataWhichDoesNotExistOnThisDatabase, sqle)

            End Try

        End Using
    End Function

    ''' <summary>
    ''' Restores the session log data for the given collection log entries.
    ''' </summary>
    ''' <param name="entries">The entries which should be restored to the database.
    ''' </param>
    <SecuredMethod(True)>
    Public Sub RestoreSessionLogData(ByVal entries As ICollection(Of clsSessionLogEntry)) Implements IServer.RestoreSessionLogData
        CheckPermissions()

        Using con = GetConnection()
            Dim unicode As Boolean = UnicodeLoggingEnabled(con)

            Dim cmd As New SqlCommand(String.Format(
             " insert into {0} (" &
             "   sessionnumber," &
             "   stageid," &
             "   stagename," &
             "   stagetype," &
             "   processname," &
             "   pagename," &
             "   objectname," &
             "   actionname," &
             "   result," &
             "   resulttype," &
             "   startdatetime," &
             "   starttimezoneoffset," &
             "   enddatetime," &
             "   endtimezoneoffset," &
             "   attributexml," &
             "   automateworkingset," &
             "   targetappname," &
             "   targetappworkingset" &
             " ) values (" &
             "   @sessionnumber, @stageid, @stagename, @stagetype, " &
             "   @processname, @pagename, @objectname, @actionname, @result, " &
             "   @resulttype, @startdatetime, @starttimezoneoffset, " &
             "   @enddatetime, @endtimezoneoffset, @attributexml," &
             "   @automateworkingset, @targetappname, @targetappworkingset)" _
            , IIf(unicode, "BPASessionLog_Unicode", "BPASessionLog_NonUnicode")))
            clsSessionLogEntry.CreateParameters(cmd.Parameters)
            For Each entry As clsSessionLogEntry In entries
                entry.SetInto(cmd.Parameters)
                con.Execute(cmd)
            Next

        End Using

    End Sub

    ''' <summary>
    ''' Gets the correct session log table to use for the given session no.
    ''' </summary>
    ''' <param name="con">The connection to the database</param>
    ''' <param name="sessionNo">The session number for which the appropriate log
    ''' table name is required.</param>
    ''' <returns>The session log table name for the specified session</returns>
    Friend Function GetSessionLogTableName(
     con As IDatabaseConnection, sessionNo As Integer) As String
        ' A session should never be split across tables, so find the table that 
        ' contains that session
        Dim tableName = ""
        Dim sql =
            " select top 1 'BPASessionLog_Unicode'" &
            "   from BPASessionLog_Unicode" &
            "   where SessionNumber = @sessno" &
            " union " &
            " select top 1 'BPASessionLog_NonUnicode'" &
            "   from BPASessionLog_NonUnicode" &
            "   where SessionNumber = @sessno"

        Using cmd As New SqlCommand(sql)
            cmd.Parameters.AddWithValue("@sessno", sessionNo)
            tableName = IfNull(con.ExecuteReturnScalar(cmd), "")
        End Using

        If tableName = "" Then
            Throw New NoSuchSessionException("Session {0} was not found on any log table", sessionNo)
        End If

        Return tableName

    End Function

    Friend Function GetSessionLogTableName_pre65(
     con As IDatabaseConnection, sessionNo As Integer) As String
        ' A session should never be split across tables, so find the table that 
        ' contains that session

        Dim tableName = ""

        If CheckTableExists_pre65(con, "BPASessionLog_Unicode_pre65") = False Then
            Return tableName
        End If

        Dim sql =
            " select top 1 'BPASessionLog_Unicode_pre65'" &
            "   from BPASessionLog_Unicode_pre65" &
            "   where SessionNumber = @sessno" &
            " union " &
            " select top 1 'BPASessionLog_NonUnicode_pre65'" &
            "   from BPASessionLog_NonUnicode_pre65" &
            "   where SessionNumber = @sessno"

        Using cmd As New SqlCommand(sql)
            cmd.Parameters.AddWithValue("@sessno", sessionNo)
            tableName = IfNull(con.ExecuteReturnScalar(cmd), "")
        End Using

        Return tableName
    End Function

    Friend Overridable Function CheckTableExists_pre65(con As IDatabaseConnection, name As String) As Boolean
        Dim validName As String = Nothing
        Using cmd As New SqlCommand("select name from sysobjects where id = object_id(@name)")
            cmd.Parameters.AddWithValue("@name", name)
            validName = IfNull(con.ExecuteReturnScalar(cmd), "")
        End Using
        Return validName = name
    End Function

    ''' <summary>
    ''' Gets the session log data for the given <paramref name="sessionNo">session
    ''' number</paramref> after the specified <paramref name="lastLogId">sequence
    ''' number</paramref>. No more than the <paramref name="number">maximum number
    ''' </paramref> of records specified are retrieved.
    ''' </summary>
    ''' <param name="sessionNo">The session number of the session whose log entries
    ''' are required.</param>
    ''' <param name="lastLogId">The last logId - only log entries
    ''' beyond this value will be retrieved.</param>
    ''' <param name="number">The maximum number of records to retrieve.</param>
    ''' <returns>A collection of session log entries from the specified session,
    ''' starting from immediately after the last sequence number, and containing no
    ''' more elements than the maximum specified. If the returned collection is
    ''' empty, then there are no more log entries to return.</returns>
    <SecuredMethod(True)>
    Public Function GetSessionLogData(
     sessionNo As Integer, lastLogId As Long, number As Integer, sessionLogMaxAttributeXmlLength As Integer) _
     As ICollection(Of clsSessionLogEntry) Implements IServer.GetSessionLogData
        CheckPermissions()

        Using con = GetConnection()
            Dim tableName As String
            Try
                tableName = GetSessionLogTableName_pre65(con, sessionNo)
                If Not String.IsNullOrWhiteSpace(tableName) Then
                    Return GetSessionLogData_pre65(sessionNo, CType(lastLogId, Integer), number)
                End If

                tableName = GetSessionLogTableName(con, sessionNo)
            Catch ex As NoSuchSessionException
                Return New List(Of clsSessionLogEntry)
            End Try

            Dim attributeXmlLength = If(sessionLogMaxAttributeXmlLength < number, sessionLogMaxAttributeXmlLength, number)

            Dim sqlSelect = String.Format(
                 "   sl.logid," &
                 "   sl.sessionnumber," &
                 "   sl.stageid," &
                 "   sl.stagename," &
                 "   sl.stagetype," &
                 "   sl.processname," &
                 "   sl.pagename," &
                 "   sl.objectname," &
                 "   sl.actionname," &
                 "   sl.result," &
                 "   sl.resulttype," &
                 "   sl.startdatetime," &
                 "   sl.enddatetime," &
                 "   LEFT(sl.attributexml,{0}) AS attributexml, " &
                 "   sl.automateworkingset," &
                 "   sl.targetappname," &
                 "   sl.targetappworkingset," &
                 "   sl.starttimezoneoffset," &
                 "   sl.endtimezoneoffset," &
                 "   sl.attributesize ",
                 attributeXmlLength)

            Dim sql = $"WITH CommulativeSessionLog AS  
( 
    SELECT ROW_NUMBER() OVER(ORDER BY t1.logid ASC) AS RowNum, t1.logid, SUM(t1.attributesize) OVER (ORDER BY t1.logid) AS attributesizesum
    FROM {tableName} t1
    WHERE t1.sessionnumber = @sessno AND t1.logid > @lastLogId
)
SELECT {sqlSelect}
FROM {tableName} sl
INNER JOIN CommulativeSessionLog l on sl.logid = l.logid
WHERE l.attributesizesum <= @number OR l.RowNum = 1
ORDER BY l.RowNum"
            Using cmd As New SqlCommand(sql)

                cmd.Parameters.AddWithValue("@number", number)
                cmd.Parameters.AddWithValue("@sessno", sessionNo)
                cmd.Parameters.AddWithValue("@lastLogId", lastLogId)
                cmd.CommandTimeout = Options.Instance.SqlCommandTimeoutLog

                Using reader = con.ExecuteReturnDataReader(cmd)

                    Dim list As New List(Of clsSessionLogEntry)
                    Dim prov As New ReaderDataProvider(reader)
                    While reader.Read()
                        Dim entry = New clsSessionLogEntry(prov)

                        entry.AttributeSize = If(entry.AttributeSize > sessionLogMaxAttributeXmlLength, 0, entry.AttributeSize)
                        If entry.AttributeSize = 0 Then
                            entry.AttributeXml = Nothing
                        End If

                        If entry.AttributeSize > 200 Then                                             'less than 200bytes may gzip to larger or similar size depending on patterns
                            entry.AttributeXmlGzip = Encoding.UTF8.GetBytes(entry.AttributeXml)
                            Using memory = New System.IO.MemoryStream()
                                Using gzip = New System.IO.Compression.GZipStream(memory, System.IO.Compression.CompressionMode.Compress)
                                    gzip.Write(entry.AttributeXmlGzip, 0, entry.AttributeXmlGzip.Length)
                                End Using
                                entry.AttributeXmlGzip = memory.ToArray()
                            End Using
                            entry.AttributeXml = Nothing
                        End If

                        list.Add(entry)
                    End While
                    Return list

                End Using
            End Using
        End Using

    End Function

    <SecuredMethod(True)>
    Public Function GetSessionLogAttributeXml(
     sessionNo As Integer, logId As Long, offset As Long, number As Integer) _
     As ICollection(Of clsSessionLogEntry) Implements IServer.GetSessionLogAttributeXml
        CheckPermissions()

        Using con = GetConnection()
            Dim tableName As String
            Try
                tableName = GetSessionLogTableName(con, sessionNo)
            Catch ex As NoSuchSessionException
                Return New List(Of clsSessionLogEntry)
            End Try

            Dim sql = String.Format(
                 "SELECT SUBSTRING(attributexml,{0},{1}) AS attributexml FROM {2} WHERE logid = @logId AND sessionnumber = @sessno", offset, number, tableName
                )

            Using cmd As New SqlCommand(sql)

                cmd.Parameters.AddWithValue("@logId", logId)
                cmd.Parameters.AddWithValue("@sessno", sessionNo)
                cmd.CommandTimeout = Options.Instance.SqlCommandTimeoutLog

                Using reader = con.ExecuteReturnDataReader(cmd)

                    Dim list As New List(Of clsSessionLogEntry)
                    Dim prov As New ReaderDataProvider(reader)
                    While reader.Read()
                        Dim entry = New clsSessionLogEntry(prov)

                        If entry.AttributeXml.Length > 0 Then
                            entry.AttributeXmlGzip = Encoding.UTF8.GetBytes(entry.AttributeXml)
                            Using memory = New System.IO.MemoryStream()
                                Using gzip = New System.IO.Compression.GZipStream(memory, System.IO.Compression.CompressionMode.Compress)
                                    gzip.Write(entry.AttributeXmlGzip, 0, entry.AttributeXmlGzip.Length)
                                End Using
                                entry.AttributeXmlGzip = memory.ToArray()
                            End Using
                            entry.AttributeXml = Nothing
                        End If

                        list.Add(entry)
                    End While
                    Return list

                End Using
            End Using
        End Using

    End Function

    <SecuredMethod(True)>
    Public Function GetSessionLogData_pre65(
     ByVal sessionNo As Integer, ByVal lastSeqNo As Integer, ByVal number As Integer) _
     As ICollection(Of clsSessionLogEntry) Implements IServer.GetSessionLogData_pre65
        CheckPermissions()

        Using con = GetConnection()
            Dim tableName As String
            Try
                tableName = GetSessionLogTableName_pre65(con, sessionNo)
                If tableName.Length = 0 Then
                    Return New List(Of clsSessionLogEntry)
                End If
            Catch ex As NoSuchSessionException
                Return New List(Of clsSessionLogEntry)
            End Try

            Dim sql = String.Format(
                 "select top {0} sessionnumber," &
                 "   seqnum," &
                 "   stageid," &
                 "   stagename," &
                 "   stagetype," &
                 "   processname," &
                 "   pagename," &
                 "   objectname," &
                 "   actionname," &
                 "   result," &
                 "   resulttype," &
                 "   startdatetime," &
                 "   enddatetime," &
                 "   attributexml, " &
                 "   automateworkingset," &
                 "   targetappname," &
                 "   targetappworkingset" &
                 " from {1} " &
                 " where SessionNumber = @sessno and SeqNum > @lastseqno " &
                 " order by SeqNum",
                 number,
                 tableName
                )

            Using cmd As New SqlCommand(sql)

                cmd.Parameters.AddWithValue("@sessno", sessionNo)
                cmd.Parameters.AddWithValue("@lastseqno", lastSeqNo)
                cmd.CommandTimeout = Options.Instance.SqlCommandTimeoutLog

                Using reader = con.ExecuteReturnDataReader(cmd)

                    Dim list_pre65 As New List(Of clsSessionLogEntry_pre65)
                    Dim prov As New ReaderDataProvider(reader)
                    While reader.Read()
                        list_pre65.Add(New clsSessionLogEntry_pre65(prov))
                    End While

                    Dim list As New List(Of clsSessionLogEntry)
                    For Each item In list_pre65
                        list.Add(New clsSessionLogEntry(item))
                    Next
                    Return list

                End Using
            End Using
        End Using

    End Function

    ''' <summary>
    ''' Gets the session ID of any sessions which are marked as
    ''' <see cref="SessionStatus.Archived">archived</see>, meaning that they have
    ''' been archived and should have been deleted, but the operation was interrupted
    ''' </summary>
    ''' <returns>A collection of GUIDs representing the IDs of the orphaned sessions
    ''' in the database.</returns>
    <SecuredMethod(True)>
    Public Function GetOrphanedSessionIds() As ICollection(Of Guid) Implements IServer.GetOrphanedSessionIds
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand()
            cmd.CommandText =
             "select sessionid from BPASession where statusid = @status"
            cmd.Parameters.AddWithValue("@status", SessionStatus.Archived)
            Dim ids As New List(Of Guid)
            Using reader = con.ExecuteReturnDataReader(cmd)
                While reader.Read()
                    ids.Add(reader.GetGuid(0))
                End While
            End Using
            Return ids
        End Using
    End Function

    ''' <summary>
    ''' Gets the session number of any sessions which are marked as
    ''' <see cref="SessionStatus.Archived">archived</see>, meaning that they have
    ''' been archived and should have been deleted, but the operation was interrupted
    ''' </summary>
    ''' <returns>A collection of numbers representing the session numbers of the
    ''' orphaned sessions in the database.</returns>
    <SecuredMethod(True)>
    Public Function GetOrphanedSessionNumbers() As ICollection(Of Integer) Implements IServer.GetOrphanedSessionNumbers
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand()
            cmd.CommandText =
             "select sessionnumber from BPASession where statusid = @status"
            cmd.Parameters.AddWithValue("@status", SessionStatus.Archived)
            Dim nums As New List(Of Integer)
            Using reader = con.ExecuteReturnDataReader(cmd)
                While reader.Read()
                    nums.Add(reader.GetInt32(0))
                End While
            End Using
            Return nums
        End Using
    End Function

    ''' <summary>
    ''' Archives the given session from the database, as well as any alerts which
    ''' refer to that session.
    ''' </summary>
    ''' <param name="sessionNo">The session number to delete</param>
    ''' <returns>The (total) number of log records deleted</returns>    
    <SecuredMethod(True)>
    Public Function ArchiveSession(ByVal sessionNo As Integer) As Integer Implements IServer.ArchiveSession
        CheckPermissions()
        Return ArchiveSessions(New Integer() {sessionNo})
    End Function

    ''' <summary>
    ''' Archives the given sessions from the database, as well as any alerts which
    ''' refer to that session.
    ''' </summary>
    ''' <param name="sessionNos">The session numbers to delete</param>
    ''' <returns>The (total) number of log records deleted</returns>
    <SecuredMethod(True)>
    Public Function ArchiveSessions(
     ByVal sessionNos As ICollection(Of Integer)) As Integer Implements IServer.ArchiveSessions
        CheckPermissions()
        Using con = GetConnection()
            Return ArchiveSessions(con, sessionNos, Nothing)
        End Using
    End Function

    ''' <summary>
    ''' Archives the given session from the database, as well as any alerts which
    ''' refer to that session.
    ''' </summary>
    ''' <param name="con">The connection to use to access the database</param>
    ''' <param name="sessionNos">The session numbers to delete</param>
    ''' <param name="mon">The progress monitor for the session deletion. This can be
    ''' null indicating that the progress is not to be monitored</param>
    ''' <returns>The (total) number of log records deleted</returns>    
    Private Function ArchiveSessions(
     ByVal con As IDatabaseConnection,
     ByVal sessionNos As ICollection(Of Integer),
     ByVal mon As clsProgressMonitor) As Integer

        ' The number of rows to delete in a single pass of the query
        Const ChunkSize As Integer = 1000

        ' Sanity check - just so we don't end up dividing by zero later
        If sessionNos.Count = 0 Then Return 0

        ' Mark sessions as 'archived' - ie. deleted.
        Using cmd As New SqlCommand()
            cmd.Parameters.AddWithValue("@status", CInt(SessionStatus.Archived))
            UpdateMultipleIds(con, cmd, sessionNos, "sessno",
             " update BPASession set statusid = @status" &
             " where sessionnumber in ("
            )
        End Using

        ' Figure out the percentage completion for a single session
        ' Add 1 session's worth for our wrapping up
        Dim sessPercent As Double = 100.0 / (sessionNos.Count + 1)

        ' Wrap the progress monitor so that we don't have to have null checks
        ' throughout the code
        mon = New MonitorWrapper(mon)

        ArchiveDeleteAlerts(con, sessionNos)

        ' The tally of total records affected that we'll be keeping
        Dim count As Integer = 0

        ' Collection counter
        Dim i As Integer = 0

        Dim tableList As New List(Of String)(New String() {"BPASessionLog_Unicode", "BPASessionLog_NonUnicode"})
        If CheckTableExists_pre65(con, "BPASessionLog_Unicode_pre65") = True Then
            tableList.Add("BPASessionLog_Unicode_pre65")
            tableList.Add("BPASessionLog_NonUnicode_pre65")
        End If

        For Each sessNo As Integer In sessionNos
            For Each slogtable As String In tableList

                ' Find out how many entries we'll be deleting for this session
                Dim sql = String.Format(
                        " select count(*) from {0} where sessionnumber = @sessno",
                        ValidateTableName(con, slogtable))
                Dim total As Integer = 0
                Using cmd As New SqlCommand(sql)

                    ' Remove the timeout for the deletion of the records.
                    cmd.CommandTimeout = Options.Instance.SqlCommandTimeoutLog

                    ' Add the single param that we'll be using
                    cmd.Parameters.AddWithValue("@sessno", sessNo)

                    total = IfNull(con.ExecuteReturnScalar(cmd), 0)
                    If total = 0 Then Continue For

                End Using

                ' Set up the command that we'll be using. Delete 1000 records at a time
                sql = String.Format(" with cte as (" &
                                        "   select top {0} *" &
                                        "   from {1} " &
                                        "   where sessionnumber = @sessno" &
                                        " )" &
                                        " delete from cte;",
                                        ChunkSize,
                                        ValidateTableName(con, slogtable))

                Using cmd As New SqlCommand(sql)
                    cmd.Parameters.AddWithValue("@sessno", sessNo)

                    ' A local count for the session log were deleting
                    Dim sessCount As Integer = 0
                    Do

                        ' Execute the delete and count how many records were deleted
                        Dim iterCount As Integer = con.ExecuteReturnRecordsAffected(cmd)

                        ' If there was nothing deleted, the work is done...
                        If iterCount = 0 Then Exit Do

                        ' Add the count of records deleted in this iteration to the count for
                        ' this session
                        sessCount += iterCount

                        ' Add to the count for all sessions
                        count += iterCount

                        ' How much progress?
                        ' 100% of sessions processed so far (i x (100 / sessionNos.Count))
                        ' + the percentage of the amount so far
                        Dim iterPercent As Double = (iterCount / total)
                        mon.FireProgressChange(
                            CInt(Math.Floor((i * sessPercent) + (iterPercent * sessPercent))),
                            String.Format(My.Resources.clsServer_Session01Deleted23Logs4,
                            i + 1, sessionNos.Count, sessCount, total,
                            CInt(100.0 * (sessCount / total))))
                    Loop
                    i += 1
                End Using
            Next
            i += 1
        Next


        mon.FireProgressChange(
            CInt(Math.Floor(sessionNos.Count * sessPercent)),
            My.Resources.clsServer_DeletingSessionRecords)

        UpdateMultipleIDs(con, sessionNos,
             "delete from BPASession where sessionnumber in ("
            )

        Return count

    End Function

    ''' <summary>
    ''' Deletes alerts from the given session.
    ''' </summary>
    ''' <param name="con">The connection over which the alerts should be deleted.
    ''' </param>
    ''' <param name="sessNos">The session numbers to delete</param>
    Private Sub ArchiveDeleteAlerts(ByVal con As IDatabaseConnection, ByVal sessNos As ICollection(Of Integer))

        UpdateMultipleIds(con, New SqlCommand(), sessNos, "sessno",
         " delete e " &
         " from BPAAlertEvent e " &
         "   join BPASession s on e.sessionid = s.sessionid " &
         " where s.sessionnumber in ("
        )

    End Sub

    ''' <summary>
    ''' Copies archived data back into the database.
    ''' </summary>
    ''' <param name="dsSession">The archived data to be restored.</param>
    ''' <remarks>Throws an Exception in the event of failure.</remarks>
    <SecuredMethod(True)>
    Public Sub ArchiveRestoreFromDataSet(ByVal dsSession As DataSet) Implements IServer.ArchiveRestoreFromDataSet
        CheckPermissions()

        Dim iSessionNumber As Integer
        Dim gSessionId As Guid
        Dim unicode As Boolean = UnicodeLoggingEnabled()

        Dim objSqlConnection = mDBConnectionSetting.CreateSqlConnection()
        Dim dtSession As DataTable = Nothing
        Dim dtLog As DataTable = Nothing

        Try
            dtSession = dsSession.Tables("BPASession")
            dtLog = dsSession.Tables("BPASessionLog")

            'Get the session ID.
            For r As Integer = 0 To dtSession.Rows.Count - 1
                gSessionId = CType(dtSession.Rows(r)("SessionID"), Guid)
                If gSessionId.Equals(Guid.Empty) Then
                    Throw New InvalidArgumentException(My.Resources.clsServer_SessionIDMissingFromFile)
                End If
            Next

            'Remove constraints so that a new auto increment Session Number can be used.
            dtSession.Constraints.Clear()
            dtLog.Constraints.Clear()

            objSqlConnection.Open()

            'Insert into the session table.
            Dim objSqlBulkCopy As New System.Data.SqlClient.SqlBulkCopy(objSqlConnection)
            objSqlBulkCopy.BatchSize = dtSession.Rows.Count
            objSqlBulkCopy.DestinationTableName = dtSession.TableName
            objSqlBulkCopy.WriteToServer(dtSession)

            'Get the new session number created by SQL Server auto
            'increment and apply it to the logs.
            iSessionNumber = GetSessionNumber(gSessionId)
            For Each r As DataRow In dtLog.Rows
                r("SessionNumber") = iSessionNumber
            Next

            'Insert into the log table.
            objSqlBulkCopy.BatchSize = 1000
            objSqlBulkCopy.DestinationTableName = CStr(IIf(unicode, "BPASessionLog_Unicode", "BPASessionLog_NonUnicode"))
            objSqlBulkCopy.WriteToServer(dtLog)

        Finally
            If objSqlConnection IsNot Nothing Then
                objSqlConnection.Close()
            End If
        End Try

    End Sub

End Class
