Imports System.Data.SqlClient
Imports System.Data.SqlTypes
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.DataContracts
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.Data
Imports BluePrism.Core.Compression
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.Processes
Imports BluePrism.Server.Domain.Models

Partial Public Class clsServer

    ''' <summary>
    ''' Reads the process name from the database.
    ''' </summary>
    ''' <param name="gProcessID">The process id</param>
    ''' <returns>The process name</returns>
    <SecuredMethod(True)>
    Public Function GetProcessNameByID(ByVal gProcessID As Guid) As String Implements IServer.GetProcessNameByID
        CheckPermissions()
        Using con = GetConnection()
            Try
                Return GetProcessNameById(con, gProcessID)
            Catch
                Return ""
            End Try
        End Using
    End Function


    Private Function GetProcessIDBySessionNumber(connection As IDatabaseConnection, sessionNumber As Integer) As Guid
        Dim cmd As New SqlCommand("select processid from BPASession where sessionnumber = @sessionNum")
        cmd.Parameters.AddWithValue("@sessionNum", sessionNumber)
        Return CType(connection.ExecuteReturnScalar(cmd), Guid)
    End Function

    Private Function GetProcessIDBySessionId(connection As IDatabaseConnection, sessionId As Guid) As Guid
        Dim cmd As New SqlCommand("select processid from BPASession where sessionid = @sessionId")
        cmd.Parameters.AddWithValue("@sessionId", sessionId)
        Return CType(connection.ExecuteReturnScalar(cmd), Guid)
    End Function

    ''' <summary>
    ''' Reads the process name from the database.
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="id">The process id</param>
    ''' <returns>The process name</returns>
    Private Function GetProcessNameById(ByVal con As IDatabaseConnection, ByVal id As Guid) As String Implements IServerPrivate.GetProcessNameById
        Dim cmd As New SqlCommand("select name from BPAProcess where processid = @id")
        cmd.Parameters.AddWithValue("@id", id)
        Dim obj As Object = con.ExecuteReturnScalar(cmd)
        If obj Is Nothing Then Return "" Else Return DirectCast(obj, String)
    End Function

    ''' <summary>
    ''' Reads the process run mode from the database. The process's dependency
    ''' information must be up to date, or this value will be stale. (And this
    ''' function does not check that!)
    ''' </summary>
    ''' <param name="procid">The process id</param>
    ''' <returns>The run mode, for this process alone.</returns>
    Private Function GetProcessRunMode(con As IDatabaseConnection, procid As Guid) As BusinessObjectRunMode
        Dim cmd As New SqlCommand("select runmode from BPAProcess where processid = @procid")
        cmd.Parameters.AddWithValue("@procid", procid)
        Dim obj As Object = con.ExecuteReturnScalar(cmd)
        Return CType(obj, BusinessObjectRunMode)
    End Function

    ''' <summary>
    ''' Gets the type of the process
    ''' </summary>
    ''' <param name="processId">The process id</param>
    ''' <returns>The type of the process.</returns>
    Private Function GetProcessType(connection As IDatabaseConnection, processId As Guid) As DiagramType
        Using command As New SqlCommand("select ProcessType from BPAProcess where processid = @processid")
            command.Parameters.AddWithValue("@processid", processId)
            Dim type = CStr(connection.ExecuteReturnScalar(command))
            If (type = "P") Then Return DiagramType.Process
            If (type = "O") Then Return DiagramType.Object
            Return DiagramType.Unset
        End Using
    End Function

    ''' <summary>
    ''' Checks to see if a process is locked.
    ''' </summary>
    ''' <param name="gProcessID">The process id</param>
    ''' <param name="userName">The name of the user locking the process</param>
    ''' <param name="machineName">The  machine on which the process is locked</param>
    ''' <returns>True if the process is locked, False if unlocked</returns>
    <SecuredMethod(True)>
    Public Function ProcessIsLocked(ByVal gProcessID As Guid, ByRef userName As String, ByRef machineName As String) As Boolean Implements IServer.ProcessIsLocked
        CheckPermissions()
        Dim con = GetConnection()

        Try
            Dim cmd As New SqlCommand("SELECT u.username, l.machinename FROM bpaprocesslock l INNER JOIN bpauser u ON l.userid=u.userid WHERE l.processid=@ProcessID")
            With cmd.Parameters
                .AddWithValue("@ProcessID", gProcessID.ToString)
            End With
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)

                If reader.Read() Then
                    userName = prov.GetString("username")
                    machineName = prov.GetString("machinename")
                    Return True
                End If
            End Using

            Return False

        Finally
            con.Close()
        End Try

    End Function

    <SecuredMethod(True)>
    Public Sub GetProcessLockInfo(ByVal processID As Guid, ByRef userId As Guid, ByRef machineName As String, ByRef lockedAt As DateTime) Implements IServer.GetProcessLockInfo
        CheckPermissions()

        Using con = GetConnection()
            Dim cmd As New SqlCommand("SELECT userid, machinename, lockdatetime FROM bpaprocesslock WHERE processid=@ProcessID")
            With cmd.Parameters
                .AddWithValue("@ProcessID", processID.ToString)
            End With
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)

                If reader.Read() Then
                    userId = prov.GetGuid("userid")
                    machineName = prov.GetString("machinename")
                    lockedAt = prov.GetValue("lockdatetime", Date.MinValue)
                    Return
                End If
            End Using

            userId = Guid.Empty
            machineName = ""
            lockedAt = Date.MinValue
        End Using
    End Sub

    ''' <summary>
    ''' Marks a process as locked on the database
    ''' </summary>
    ''' <param name="con">The connection over which the process should be locked
    ''' </param>
    ''' <param name="processId">The ID of the process to lock</param>
    ''' <exception cref="AlreadyLockedException">If the process/VBO is already marked
    ''' as locked on the database.</exception>
    ''' <exception cref="LockFailedException">If the lock failed for any other
    ''' reason.</exception>
    Private Sub LockProcess(ByVal con As IDatabaseConnection, ByVal processId As Guid)
        If processId = Guid.Empty Then Throw New ArgumentNullException(
         My.Resources.clsServer_EmptyProcessIDSuppliedToLockProcess)

        Dim machineName As String = mLoggedInMachine

        Try

            Dim cmd As New SqlCommand(
             "insert into BPAProcessLock " &
             " (processID, lockdatetime,userid,machinename) " &
             " values (@ProcessID,@LockDateTime,@UserID,@MachineName)")

            With cmd.Parameters
                .AddWithValue("@ProcessID", processId)
                .AddWithValue("@LockDateTime", Now)
                .AddWithValue("@UserID", GetLoggedInUserId())
                .AddWithValue("@MachineName", machineName)
            End With

            ' Need to capture the primary key error so we now it already exists
            con.Execute(cmd)

        Catch sqlEx As SqlException _
         When sqlEx.Number = DatabaseErrorCode.UniqueConstraintError
            ' duplicate primary key - process already locked
            Dim cmd As New SqlCommand(
             " select " &
             "   p.name as processname, " &
             "   l.lockdatetime as lockdatetime," &
             "   l.userid as userid, " &
             "   l.machinename as machinename, " &
             "   u.username as username " &
             " from BPAProcess p " &
             "   join BPAProcessLock l on p.processid = l.processid" &
             "   join BPAUser u on l.userid = u.userid" &
             " where p.processid = @processid"
            )

            cmd.Parameters.AddWithValue("@processid", processId)

            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)

                Dim dLockDateTime As Date
                Dim gUserId As Guid
                Dim sUsername As String = ""
                Dim procName As String = ""

                ' Loop through rows returned in the reader
                While reader.Read()
                    procName = prov.GetString("processname")
                    dLockDateTime = prov.GetValue("lockdatetime", Date.MinValue)
                    gUserId = prov.GetValue("userid", Guid.Empty)
                    sUsername = prov.GetString("username")
                    machineName = prov.GetString("machinename")
                End While

                Throw New AlreadyLockedException(
                 My.Resources.clsServer_TheUser0StartedEditingTheProcess1On2At3,
                 sUsername, procName, machineName, dLockDateTime)

            End Using

        Catch sqlEx As SqlException When sqlEx.Number = DatabaseErrorCode.ForeignKeyError
            Throw New LockFailedException(
             My.Resources.clsServer_LockFailedDueToForeignKeyConstraintIsThisLockBeingAttemptedFromARegisteredBlueP, sqlEx)

        End Try

    End Sub

    ''' <summary>
    ''' Creates a lock record in the database for a process.
    ''' </summary>
    ''' <param name="gProcessID">The process id</param>
    <SecuredMethod(True)>
    Public Sub LockProcess(ByVal gProcessID As Guid) Implements IServer.LockProcess
        CheckPermissions()
        Using con = GetConnection()
            LockProcess(con, gProcessID)
        End Using
    End Sub

    ''' <summary>
    ''' Unlocks the process with the given ID and, if specified, allows processes 
    ''' to be unlocked by the a different machine to the currently logged in one
    ''' </summary>
    ''' <param name="procId">The ID of the process to unlock.</param>
    ''' <param name="blnAllowUnlockFromAnyMachine" >If True, can unlock the process even if 
    ''' locked by another machine to the currently logged in one.</param>
    <SecuredMethod(True)>
    Public Function UnlockProcess(procId As Guid, Optional blnAllowUnlockFromAnyMachine As Boolean = False) _
        As Boolean Implements IServer.UnlockProcess
        CheckPermissions()
        Using con = GetConnection()
            Return UnlockProcess(con, procId, blnAllowUnlockFromAnyMachine)
        End Using
    End Function

    ''' <summary>
    ''' Unlocks the process with the given ID. 
    ''' Only used for when unlocking a process as part of importing where
    ''' Edit rights are not required.
    ''' </summary>
    <SecuredMethod(True)>
    Public Function UnlockProcessImport(processId As Guid) As Boolean _
        Implements IServer.UnlockProcessImport

        CheckPermissions()
        Using con = GetConnection()
            Return DeleteProcessLock(con, processId)
        End Using
    End Function

    ''' <summary>
    ''' Unlocks the process with the given ID, optionally using the given resource
    ''' Id, over the specified connection.
    ''' </summary>
    ''' <param name="con">The connection over which the process should be unlocked.
    ''' </param>
    ''' <param name="procId">The ID of the process to unlock.</param>
    Private Function UnlockProcess(
     ByVal con As IDatabaseConnection, ByVal procId As Guid, Optional blnAllowUnlockFromAnyMachine As Boolean = False) As Boolean

        Dim processType As DiagramType = GetProcessType(con, procId)

        Dim requiredPermissions = If(processType = DiagramType.Object,
            Permission.ObjectStudio.ImpliedEditBusinessObject.ToList(),
            Permission.ProcessStudio.ImpliedEditProcess.ToList())

        If Not GetEffectiveMemberPermissionsForProcess(con, procId).HasPermission(
          GetUser(GetLoggedInUserId), requiredPermissions) Then
            Throw New BluePrismException(
                My.Resources.clsServer_YouDoNotHavePermissionToEditThis0, If(processType = DiagramType.Object,
                        My.Resources.clsServer_ChangeProcessOrObjectStatus_Object,
                        My.Resources.clsServer_ChangeProcessOrObjectStatus_Process))
        End If

        Return DeleteProcessLock(con, procId, blnAllowUnlockFromAnyMachine)

    End Function


    Private Function DeleteProcessLock(con As IDatabaseConnection, processId As Guid, Optional allowUnLockFromAnyMAchine As Boolean = False) As Boolean
        Dim cmd As New SqlCommand()
        Dim sb As New StringBuilder(
         " delete from BPAProcessLock" &
         " where processid = @processid"
        )
        cmd.Parameters.AddWithValue("@processid", processId)
        If Not allowUnLockFromAnyMAchine Then
            sb.Append(" and machinename = @machinename")
            cmd.Parameters.AddWithValue("@machinename", mLoggedInMachine)
        End If
        cmd.CommandText = sb.ToString()

        Return con.ExecuteReturnRecordsAffected(cmd) <> 0
    End Function


    ''' <summary>
    ''' Reads details of locked processes from the database.
    ''' </summary>
    ''' <returns>A hashtable of process details</returns>
    <SecuredMethod(True)>
    Public Function GetLockedProcesses(useBusinessObjects As Boolean) _
            As LockedProcessesResult _
            Implements IServer.GetLockedProcesses

        CheckPermissions()

        Using connection = GetConnection()

            Dim sqlCommand As New SqlCommand("
                SELECT 
                    a.processid AS processid,
                    a.lockdatetime AS lockdate,
                    isnull(d.name,'Unknown Resource') AS pcname,
                    isnull(b.username,'Unknown User') AS username,
                    isnull(c.name, 'Orphaned Lock') AS name
                FROM BPAProcessLock a
                LEFT OUTER JOIN BPAResource d ON a.machinename = d.name
                LEFT OUTER JOIN BPAUser b ON a.userid = b.userid
                LEFT OUTER JOIN BPAProcess c ON a.processid = c.processid
                WHERE c.ProcessType = @ProcessType")
            If useBusinessObjects Then
                sqlCommand.Parameters.AddWithValue("@ProcessType", "O")
            Else
                sqlCommand.Parameters.AddWithValue("@ProcessType", "P")
            End If
            Dim reader = connection.ExecuteReturnDataReader(sqlCommand)

            Dim noData = True

            Dim processes = New LockedProcessesResult()

            Do While reader.Read()
                noData = False

                Dim procID = CType(reader("processid"), Guid)

                Dim requiredPermissions = If(useBusinessObjects,
                    Permission.ObjectStudio.ImpliedEditBusinessObject.ToList(),
                    Permission.ProcessStudio.ImpliedEditProcess.ToList())

                If GetEffectiveMemberPermissionsForProcess(connection, procID).HasPermission(
                  GetUser(GetLoggedInUserId), requiredPermissions) Then
                    processes.LockedProcesses.Add(New LockedProcess() With
                    {
                        .Id = procID,
                        .Name = CStr(reader("name")),
                        .LockDate = CDate(reader("lockdate")),
                        .Username = CStr(reader("username")),
                        .MachineName = CStr(reader("pcname"))
                    })
                End If
            Loop
            reader.Close()

            If noData = True Then
                processes = Nothing
                Throw New MissingDataException(My.Resources.clsServer_NoProcessesAreCurrentlyLocked)
            End If

            Return processes
        End Using

    End Function

    ''' <summary>
    ''' Deletes the process or VBO with the given ID.
    ''' </summary>
    ''' <param name="procId">The ID of the process or VBO to delete</param>
    ''' <exception cref="AlreadyLockedException">If the process or VBO is currently
    ''' locked elsewhere</exception>
    ''' <exception cref="LockFailedException">If the lock could not be acquired for
    ''' any other reason</exception>
    ''' <exception cref="AuditOperationFailedException">If the writing of the audit
    ''' record failed</exception>
    ''' <exception cref="OperationFailedException">If the process or VBO could not
    ''' be deleted because there were some pending or completed sessions in the
    ''' database which referred to it</exception>
    ''' <exception cref="Exception">If any other error occurs while attempting to
    ''' delete the process/vbo</exception>
    <SecuredMethod(Permission.ProcessStudio.DeleteProcess,
                   Permission.ObjectStudio.DeleteBusinessObject)>
    Public Sub DeleteProcess(procId As Guid, deleteReason As String) Implements IServer.DeleteProcess
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            DeleteProcess(con, procId, deleteReason)
            con.CommitTransaction()
        End Using
        InvalidateCaches()
    End Sub

    ''' <summary>
    ''' Deletes the process or VBO with the given ID.
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="procId">The ID of the process or VBO to delete</param>
    ''' <param name="deleteReason">The text to set as the reason for deletion in the
    ''' audit record created for this delete operation.</param>
    ''' <exception cref="NoSuchElementException">If no process or VBO could be found
    ''' with the given ID.</exception>
    ''' <exception cref="AlreadyLockedException">If the process or VBO is currently
    ''' locked elsewhere</exception>
    ''' <exception cref="LockFailedException">If the lock could not be acquired for
    ''' any other reason</exception>
    ''' <exception cref="AuditOperationFailedException">If the writing of the audit
    ''' record failed</exception>
    ''' <exception cref="OperationFailedException">If the process or VBO could not
    ''' be deleted because there were some pending or completed sessions in the
    ''' database which referred to it</exception>
    ''' <exception cref="Exception">If any other error occurs while attempting to
    ''' delete the process/vbo</exception>
    Private Sub DeleteProcess(
     con As IDatabaseConnection, procId As Guid, deleteReason As String)
        ' Firstly lock the process
        LockProcess(con, procId)

        ' Set up the command we'll be using throughout
        Dim cmd As New SqlCommand()
        cmd.Parameters.AddWithValue("@procid", procId)

        ' Get some basic info about the process (type / name)
        cmd.CommandText =
            " select p.name, p.processtype" &
            " from BPAProcess p" &
            " where processid = @procid"

        Dim isObj As Boolean
        Dim thing As String
        Dim name As String

        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            If Not reader.Read() Then Throw New NoSuchElementException(
                My.Resources.clsServer_UnableToFindAnyProcessVBOWithTheGivenID0, procId)
            name = prov.GetString("name")
            isObj = (If(prov.GetString("processtype"), "").ToLower() = "o")
            thing = If(isObj, My.Resources.clsServer_BusinessObject, My.Resources.clsServer_Process)
        End Using

        ' Check current user has permission to delete
        Dim mem As ProcessBackedGroupMember
        Dim treeType As GroupTreeType
        If isObj Then
            treeType = GroupTreeType.Objects
            mem = New ObjectGroupMember() With {.Id = procId}
        Else
            treeType = GroupTreeType.Processes
            mem = New ProcessGroupMember() With {.Id = procId}
        End If
        Dim treeDef = treeType.GetTreeDefinition()
        If Not GetEffectiveMemberPermissions(con, mem).HasPermission(
          mLoggedInUser, treeDef.DeleteItemPermission) Then
            UnlockProcess(con, procId)
            Throw New PermissionException(
                My.Resources.clsServer_UnauthorizedUserDoesNotHavePermissionToDeleteThis0,
                TreeDefinitionAttribute.GetLocalizedFriendlyName(treeDef.SingularName))
        End If

        ' So now we check if any sessions refer to this process. For that we need...
        cmd.Parameters.AddWithValue("@statusid", SessionStatus.Pending)

        ' check whether this process has been executed before
        cmd.CommandText =
            "select count(statusid) from BPVSession " &
            "where statusid != @statusid and processid = @procid"

        If IfNull(con.ExecuteReturnScalar(cmd), 0) > 0 Then
            UnlockProcess(con, procId)
            Throw New OperationFailedException(
                My.Resources.clsServer_CouldNotDeleteThe0This0HasBeenRunBeforeAndIsRequiredForAudit, thing)
        End If

        'check whether there are sessions pending in control room
        cmd.CommandText =
            "select count(statusid) from BPVSession " &
            "where statusid = @statusid and processid = @procid"
        If IfNull(con.ExecuteReturnScalar(cmd), 0) > 0 Then
            UnlockProcess(con, procId)
            Throw New OperationFailedException(
                My.Resources.clsServer_The0CannotBeDeletedAtThisTimeBecauseThereArePendingSessionsForThis0, thing)
        End If

        ' We don't need the status ID in the params any more
        cmd.Parameters.RemoveAt("@statusid")

        ' check if the process is assigned to a queue
        cmd.CommandText =
            "select name from BPAWorkQueue where processid = @procid"
        Dim queueName As String = IfNull(con.ExecuteReturnScalar(cmd), "")
        If queueName <> "" Then Throw New OperationFailedException(
         My.Resources.clsServer_The0CannotBeDeletedAtThisTimeBecauseItAssignedToTheActiveWorkQueue1, thing, queueName)

        ' Delete all the tables which have an non-cascaded FK to BPAProcess first,
        ' then get rid of the BPAProcess record.
        cmd.CommandText =
            " delete from BPAScenario where scenarioid in (" &
            "   select scenarioid from BPAScenarioLink where processid = @procid);" &
            " delete from BPAScenarioLink where processid = @procid;" &
            " delete from BPAProcessLock where processid = @procid;" &
            " delete from BPACredentialsProcesses where processid = @procid;" &
            " delete from BPAProcessBackUp where processid = @procid;" &
            " delete from BPAProcessAlert where processid = @procid;" &
            " delete from BPAInternalAuth where processid = @procid;" &
            " delete from BPAProcess where processid = @procid;"
        con.Execute(cmd)

        ' We're also responsible for adding an audit record, do so now
        AuditRecordProcessOrVboEvent(
            con, ProcessOrVboEventCode.Delete,
            isObj, procId, String.Format(My.Resources.clsServer_Deleted0Was1, thing, name),
            Nothing, deleteReason)

    End Sub

    ''' <summary>
    ''' Write process details to the database.
    ''' </summary>
    ''' <param name="gProcessID">The process id</param>
    ''' <param name="gCreatedBy">The creating user id</param>
    ''' <param name="dCreateDate">The creation date</param>
    ''' <param name="gModifiedBy">The modifying user id</param>
    ''' <param name="dModifiedDate">The modification date</param>
    ''' <param name="bNew">A flag to indicated that the process is new</param>
    <SecuredMethod(True)>
    Public Sub SetProcessInfo(ByVal gProcessID As Guid, ByVal gCreatedBy As Guid, ByVal dCreateDate As Date, ByVal gModifiedBy As Guid, ByVal dModifiedDate As Date, ByVal bNew As Boolean) Implements IServer.SetProcessInfo
        CheckPermissions()
        Dim con = GetConnection()
        Try


            Dim cmd As SqlCommand

            If bNew Then
                cmd = New SqlCommand("UPDATE BPAProcess SET createdby = @CreatedBy, createdate = @CreateDate, lastmodifieddate = @ModifiedDate, lastmodifiedby = @ModifiedBy WHERE processid = @ProcessID")
                With cmd.Parameters
                    .AddWithValue("@CreatedBy", gCreatedBy.ToString)
                    .AddWithValue("@CreateDate", dCreateDate)
                End With
            Else
                cmd = New SqlCommand("UPDATE BPAProcess SET lastmodifieddate = @ModifiedDate, lastmodifiedby = @ModifiedBy WHERE processid = @ProcessID")
            End If
            With cmd.Parameters
                .AddWithValue("@ModifiedDate", dModifiedDate)
                .AddWithValue("@ModifiedBy", gModifiedBy.ToString)
                .AddWithValue("@ProcessID", gProcessID.ToString)
            End With


            con.Execute(cmd)
        Catch e As Exception
        Finally
            con.Close()
        End Try

    End Sub

    ''' <summary>
    ''' Reads process details from the database. There is a more detailed overload
    ''' if additional information is required.
    ''' </summary>
    ''' <param name="gProcessID">The process id</param>
    ''' <param name="sCreatedBy">The creator user name</param>
    ''' <param name="dCreateDate">The creation date</param>
    ''' <param name="sModifiedBy">The moste recent modifier user name</param>
    ''' <param name="dModifiedDate">The most recent modification date</param>
    ''' <returns>True if successful</returns>
    <SecuredMethod(True)>
    Public Function GetProcessInfo(ByVal gProcessID As Guid, ByRef sCreatedBy As String, ByRef dCreateDate As Date, ByRef sModifiedBy As String, ByRef dModifiedDate As Date) As Boolean Implements IServer.GetProcessInfo
        CheckPermissions()
        Dim type As DiagramType
        Using con = GetConnection()
            Return GetProcessInfo(con, gProcessID, sCreatedBy, dCreateDate, sModifiedBy, dModifiedDate, type)
        End Using
    End Function


    ''' <summary>
    ''' Reads process details from the database. There is a less detailed overload
    ''' that can be used when some of this information is not required.
    ''' </summary>
    ''' <param name="procId">The process id</param>
    ''' <param name="createdBy">The creator user name</param>
    ''' <param name="createdDate">The creation date</param>
    ''' <param name="modifiedBy">The moste recent modifier user name</param>
    ''' <param name="modifiedDate">The most recent modification date</param>
    ''' <param name="type">The process type - see clsProcess.Type</param>
    ''' <returns>True if successful</returns>
    <SecuredMethod(True)>
    Public Function GetProcessInfo(ByVal procId As Guid,
     ByRef createdBy As String, ByRef createdDate As Date,
     ByRef modifiedBy As String, ByRef modifiedDate As Date,
     ByRef type As DiagramType) As Boolean Implements IServer.GetProcessInfo
        CheckPermissions()
        Using con = GetConnection()
            Return GetProcessInfo(con, procId, createdBy, createdDate, modifiedBy, modifiedDate, type)
        End Using
    End Function

    ''' <summary>
    ''' Reads process details from the database. There is a less detailed overload
    ''' that can be used when some of this information is not required.
    ''' </summary>
    ''' <param name="con">The connection over which the process should be edited
    ''' </param>
    ''' <param name="procId">The process id</param>
    ''' <param name="createdBy">The creator user name</param>
    ''' <param name="createdDate">The creation date</param>
    ''' <param name="modifiedBy">The moste recent modifier user name</param>
    ''' <param name="modifiedDate">The most recent modification date</param>
    ''' <param name="type">The process type - see clsProcess.Type</param>
    ''' <returns>True if successful</returns>
    Private Function GetProcessInfo(con As IDatabaseConnection, procId As Guid, ByRef createdBy As String, ByRef createdDate As Date, ByRef modifiedBy As String, ByRef modifiedDate As Date, ByRef type As DiagramType) As Boolean

        Try

            Dim cmd As New SqlCommand(
                " select" &
                "   p.createdate, " &
                "   p.lastmodifieddate," &
                "   p.processtype," &
                "   cu.username as createdby," &
                "   mu.username as modifiedby" &
                " from BPAProcess p" &
                "   left join BPAUser cu on p.createdby=cu.userid" &
                "   left join BPAUser mu on p.lastmodifiedby=mu.userid" &
                " where processid=@processid"
                )
            cmd.Parameters.AddWithValue("@processid", procId)

            Using reader = con.ExecuteReturnDataReader(cmd)
                If Not reader.Read() Then Return False

                Dim prov As New ReaderDataProvider(reader)
                createdBy = prov.GetString("createdby")
                ' Default the dates to Now - this was being done in the UI,
                ' so this makes it consistent throughout. It also has the benefit
                ' of ensuring that the dates returned are local, as they are
                ' expected to be and as they are recorded on the database.
                createdDate = prov.GetValue("createdate", Date.Now)
                modifiedBy = prov.GetString("modifiedby")
                modifiedDate = prov.GetValue("lastmodifieddate", Date.Now)
                If prov.GetString("processtype") = "O" _
                    Then type = DiagramType.Object _
                    Else type = DiagramType.Process
            End Using
            Return True

        Catch
            Return False

        End Try
    End Function
    <SecuredMethod(True)>
    Public Function GetProcessMetaInfo(processIdList As Guid()) As ProcessMetaInfo() Implements IServer.GetProcessMetaInfo
        CheckPermissions()

        Using connection = GetConnection()
            Return GetProcessMetaInfoBulk(connection, processIdList).ToArray
        End Using
    End Function


    Private Function GetProcessMetaInfoBulk(connection As IDatabaseConnection, processIds As Guid()) As List(Of ProcessMetaInfo)
        Dim query =
                "select
                p.processid,
                p.createdate,
                p.lastmodifieddate,
                p.processtype,
                p.description,
                pl.userid as lockeduserid,
                pl.machinename as lockedby,
                pl.lockdatetime as lockedat,
                cu.username as createdby,
                mu.username as modifiedby
                from BPAProcess p
                inner join BPAUser cu on p.createdby = cu.userid
                inner join BPAUser mu on p.lastmodifiedby = mu.userid
                left join BPAProcessLock pl on p.processid = pl.processid
                where p.processid in ({multiple-ids}) "

        Dim metaInfo As New List(Of ProcessMetaInfo)
        mSqlHelper.SelectMultipleIds(connection, processIds, Sub(prov) metaInfo.Add(PopulateProcessMetaInfo(prov)), query)
        Return metaInfo
    End Function

    Private Function PopulateProcessMetaInfo(dataProvider As IDataProvider) As ProcessMetaInfo
        Return New ProcessMetaInfo() With {
                                .ProcessId = dataProvider.GetGuid("processid"),
                                .Description = dataProvider.GetString("description"),
                                .CreatedBy = dataProvider.GetString("createdby"),
                                .ModifiedBy = dataProvider.GetString("modifiedby"),
                                .LockMachineName = dataProvider.GetString("lockedby"),
                                .LockUserId = dataProvider.GetGuid("lockeduserid"),
                                .CreatedAt = dataProvider.GetValue("createdate", Date.MinValue),
                                .ModifiedAt = dataProvider.GetValue("lastmodifieddate", Date.MinValue),
                                .LockTime = dataProvider.GetValue("lockedat", Date.MinValue)
                                }
    End Function

    ''' <summary>
    ''' Updates a process in the database.
    ''' </summary>
    ''' <param name="processId">The process id</param>
    ''' <param name="name">The process name</param>
    ''' <param name="version">The process version</param>
    ''' <param name="description">The process description</param>
    ''' <param name="newXml">The process XML definition.</param>
    ''' <param name="auditComments">The audit comments</param>
    ''' <param name="auditSummary">User's summary of changes</param>
    ''' <param name="lastModified">On return, contains the DateTime that was stored for
    ''' the last modified date and time in the database.</param>
    ''' <param name="references">External process references</param>
    <SecuredMethod(True)>
    Public Sub EditProcess(processId As Guid, name As String, version As String,
                                description As String, newXml As String, auditComments As String, auditSummary As String,
                                ByRef lastModified As DateTime, references As clsProcessDependencyList) Implements IServer.EditProcess
        CheckPermissions()

        Using con = GetConnection()
            con.BeginTransaction()
            Dim isObject = GetProcessType(con, processId) = DiagramType.Object
            EditProcess(con, processId, isObject, name, version, description, newXml, lastModified, references)
            AuditRecordProcessOrVboEvent(con, ProcessOrVboEventCode.Modify, isObject, processId, auditComments, newXml, auditSummary)
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Edits the process with the given ID, using the specified values.
    ''' </summary>
    ''' <param name="con">The connection over which the process should be edited
    ''' </param>
    ''' <param name="processId">The ID of the process to edit.</param>
    ''' <param name="name">The name to set in the process</param>
    ''' <param name="ver">The version number to set in the process</param>
    ''' <param name="desc">The description to set in the process</param>
    ''' <param name="xml">The XML to set in the process</param>
    ''' <param name="lastmod">The last modified time to set in the process.</param>
    ''' <param name="refs">External process references</param>
    ''' <remarks>For best performance, assuming you have a clsProcess object when
    ''' you call this, you should also update the dependencies after you call this.
    ''' You do that using UpdateExternalDependencies().</remarks>
    Private Sub EditProcess(con As IDatabaseConnection, processId As Guid, isObject As Boolean, name As String,
                            ver As String, desc As String, xml As String, ByRef lastmod As DateTime,
                            refs As clsProcessDependencyList)

        'Need to get the date/time in a roundabout manner to ensure it will
        'not come back different when read from the database. We need the
        'values to be consistent.
        Dim lm As SqlDateTime = DateTime.Now
        lastmod = lm.Value

        Dim requiredPermissions = If(isObject,
            Permission.ObjectStudio.ImpliedEditBusinessObject.Concat({Permission.ObjectStudio.ImportBusinessObject}).ToList(),
            Permission.ProcessStudio.ImpliedEditProcess.Concat({Permission.ProcessStudio.ImportProcess}).ToList())

        If Not GetEffectiveMemberPermissionsForProcess(con, processId).HasPermission(
          GetUser(GetLoggedInUserId), requiredPermissions) Then
            Throw New BluePrismException(My.Resources.clsServer_YouDoNotHavePermissionToEditThis0,
                                            If(isObject, My.Resources.clsServer_BusinessObject, My.Resources.clsServer_Process))
        End If

        If Not ValidProcessName(name) Then
            Throw New BluePrismException(
              My.Resources.clsServer_FailedToImportTheProcessObject0DoesNotPassNameValidationPleaseEnsureThereAreNoE,
             name)
        End If

        If refs IsNot Nothing Then UpdateProcessDependencies(con, processId, refs)

        Dim cmd As New SqlCommand(
         " update BPAProcess set " &
         "   name=@name, " &
         "   version=@version, " &
         "   description=@description," &
         "   lastmodifieddate=@modifieddate," &
         "   lastmodifiedby=@modifiedby," &
         "   processxml=@xml," &
         "   runmode=@runmode," &
         "   sharedobject=@shared" &
         " where processid=@processid")
        With cmd.Parameters
            .AddWithValue("@name", name)
            .AddWithValue("@version", ver)
            .AddWithValue("@description", desc)
            .AddWithValue("@modifieddate", lastmod)
            .AddWithValue("@modifiedby", GetLoggedInUserId())
            .AddWithValue("@xml", xml)
            If refs IsNot Nothing Then
                .AddWithValue("@runmode", refs.RunMode)
                .AddWithValue("@shared", refs.IsShared)
            Else
                .AddWithValue("@runmode", BusinessObjectRunMode.Background)
                .AddWithValue("@shared", False)
            End If
            .AddWithValue("@processid", processId)
        End With
        con.Execute(cmd)
    End Sub

#Region " IsProcessNameUnique() "

    ''' <summary>
    ''' Checks if the given process name is unique amongst both processes and 
    ''' business objects in the system.
    ''' </summary>
    ''' <param name="gCurrentProcessID">The ID of a process/business object to
    ''' ignore in the search. Use Guid.Empty to bypass this check.</param>
    ''' <param name="ConflictingProcessID">Carries back the ID of the first
    ''' conflicting process or object found.</param>
    ''' <param name="sName">The name to search for.</param>
    ''' <returns>True if no process or business object exists with the given name;
    ''' False if either a process or business object was found.</returns>
    <SecuredMethod(True)>
    Public Function IsProcessNameUnique(
     ByVal gCurrentProcessID As Guid,
     ByRef ConflictingProcessID As Guid,
     ByVal sName As String) As Boolean Implements IServer.IsProcessNameUnique
        CheckPermissions()
        Using con = GetConnection()
            Return IsProcessNameUnique(con, gCurrentProcessID, ConflictingProcessID, sName, Nothing, Nothing, Nothing)
        End Using
    End Function



    ''' <summary>
    ''' Checks if the given name is currently in use in a process/VBO
    ''' </summary>
    ''' <param name="gCurrentProcessID">An ID to ignore - ie. an existing process/VBO
    ''' with this ID will not be counted as a collision.</param>
    ''' <param name="sName">The name to check</param>
    ''' <param name="bUseBusinessObjects">True to check VBOs; False to check
    ''' processes </param>
    ''' <returns>True if a process/VBO was found with the given name; False otherwise
    ''' </returns>
    <SecuredMethod(True)>
    Public Function IsProcessNameUnique(ByVal gCurrentProcessID As Guid, ByVal sName As String,
     ByVal bUseBusinessObjects As Boolean) As Boolean Implements IServer.IsProcessNameUnique
        CheckPermissions()
        Return IsProcessNameUnique(gCurrentProcessID, Guid.Empty, sName, bUseBusinessObjects, Nothing, Nothing)
    End Function

    ''' <summary>
    ''' Finds out whether the process/business object name is unique amongst
    ''' existing processes/business objects in the database.
    ''' </summary>
    ''' <param name="gCurrentProcessID">The ID of a process/business object to
    ''' ignore in the search. Use Guid.Empty to bypass this check.</param>
    ''' <param name="sName">The name to check.</param>
    ''' <param name="ConflictingProcessID">Carries back the ID of the first conflicting
    ''' process found.</param>
    ''' <returns>True if the process is unique and False if not or any other error
    ''' occurred</returns>
    <SecuredMethod(True)>
    Public Function IsProcessNameUnique(
     ByVal gCurrentProcessID As Guid, ByRef ConflictingProcessID As Guid,
     ByVal sName As String, ByVal isVBO As Nullable(Of Boolean),
     ByRef lastModifedBy As String, ByRef lastModifiedDate As Date) As Boolean Implements IServer.IsProcessNameUnique
        CheckPermissions()
        Using con = GetConnection()
            Try
                Return IsProcessNameUnique(
                 con, gCurrentProcessID, ConflictingProcessID, sName, isVBO, lastModifedBy, lastModifiedDate)
            Catch ex As Exception ' Er, error == not unique?
                Return False
            End Try
        End Using
    End Function

    ''' <summary>
    ''' Checks if the given process name is unique using the given connection.
    ''' </summary>
    ''' <param name="con">The connection to use</param>
    ''' <param name="ignoreId">The ID to ignore in the search - if, for example,
    ''' you want to check that the name for an existing process is unique, you
    ''' would set the process's ID here to ensure that the process itself isn't
    ''' treated as a collision on the name.</param>
    ''' <param name="conflictId">The ID of the conflicting process/VBO - ie. the
    ''' existing process/VBO with the given name - if such a process/VBO was found.
    ''' </param>
    ''' <param name="name">The name to check for uniqueness for</param>
    ''' <param name="isVBO">True to indicate that uniqueness should be checked
    ''' among VBOs; False to indicate processes should be checked. Null indicates
    ''' that both processes and VBOs should be checked.</param>
    ''' <returns>True to indicate that no current processes (except perhaps the
    ''' one identified by <paramref name="ignoreId"/>) have the specified name;
    ''' False to indicate that a process was found with that name - the ID of
    ''' which is returned in <paramref name="conflictId"/></returns>
    Private Function IsProcessNameUnique(ByVal con As IDatabaseConnection,
     ByVal ignoreId As Guid, ByRef conflictId As Guid,
     ByVal name As String, ByVal isVBO As Nullable(Of Boolean),
     ByRef modifiedBy As String, ByRef modifiedDate As Date) As Boolean

        Dim sb As New StringBuilder(
        "select p.processid, " &
        " p.lastmodifieddate," &
        " u.username" &
        " from BPAProcess p" &
        " join BPAUser u on p.lastmodifiedby = u.userid" &
        " where p.name = @name")

        If isVBO.HasValue Then sb.Append(" and processtype = @tp")
        If ignoreId <> Nothing Then sb.Append(" and processid <> @id")

        Using cmd As New SqlCommand(sb.ToString())
            With cmd.Parameters
                .AddWithValue("@name", name)
                If isVBO.HasValue Then .AddWithValue("@tp", IIf(isVBO.Value, "O", "P"))
                If ignoreId <> Nothing Then .AddWithValue("@id", ignoreId)
            End With

            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                If reader.Read() Then
                    conflictId = prov.GetValue("processid", Guid.Empty)
                    modifiedBy = prov.GetString("username")
                    modifiedDate = prov.GetValue("lastmodifieddate", Date.MinValue)
                    Return False
                End If
            End Using

            Return True
        End Using
    End Function

#End Region

#Region " IsProcessIDUnique() "


    ''' <summary>
    ''' Determines whether the specified ID is unique, amongst existing processes
    ''' (or business objects).
    ''' </summary>
    ''' <param name="gProcessID">The ID of interest.</param>
    ''' <param name="bUseBusinessObjects">If True, then only business objects will be
    ''' matched. If False then only processes will be matched.</param>
    ''' <returns>Returns True if the supplied ID is unique.</returns>
    <SecuredMethod(True)>
    Public Function IsProcessIDUnique(ByVal gProcessID As Guid, ByVal bUseBusinessObjects As Boolean) As Boolean Implements IServer.IsProcessIDUnique
        CheckPermissions()
        Dim con = GetConnection()

        Try
            Dim cmd As New SqlCommand("SELECT Count(name) FROM BPAProcess WHERE ProcessID = @ProcessID AND ProcessType = @ProcessType")
            With cmd.Parameters
                If bUseBusinessObjects Then
                    .AddWithValue("@ProcessType", "O")
                Else
                    .AddWithValue("@ProcessType", "P")
                End If
                .AddWithValue("@ProcessID", gProcessID.ToString)
            End With

            'No matching rows implies id is unique.
            Return CInt(con.ExecuteReturnScalar(cmd)) = 0
        Catch e As Exception
            Return False
        Finally
            con.Close()
        End Try
    End Function

#End Region


    ''' <summary>
    ''' Find a process by name.
    ''' </summary>
    ''' <param name="sName">The process name to search for</param>
    ''' <param name="IncludeBusinessObjects">When True, business objects will be
    ''' searched as well as processes. Optional; defaults to False.</param>
    ''' <returns>The process ID, or Guid.Empty if the process was not found.</returns>
    <SecuredMethod(True)>
    Public Function GetProcessIDByName(ByVal sName As String, Optional ByVal IncludeBusinessObjects As Boolean = False) As Guid Implements IServer.GetProcessIDByName
        CheckPermissions()
        Using con = GetConnection()
            Return GetProcessIDByName(con, sName, IncludeBusinessObjects)
        End Using
    End Function

    Private Function GetProcessIDByName(con As IDatabaseConnection, ByVal sName As String, Optional ByVal IncludeBusinessObjects As Boolean = False) As Guid Implements IServerPrivate.GetProcessIDByName

        Try
            'Build query
            Dim sQuery As String = "SELECT processid FROM BPAProcess WHERE name = @Name"
            If Not IncludeBusinessObjects Then
                sQuery &= " AND ProcessType='P'"
            End If

            Dim cmd As New SqlCommand(sQuery)
            With cmd.Parameters
                .AddWithValue("@Name", sName)
            End With
            Using reader = con.ExecuteReturnDataReader(cmd)

                ' Loop through rows returned in the reader
                Do While reader.Read()
                    Return CType(reader("processid"), Guid)
                Loop

                Return Guid.Empty
            End Using
        Catch
            Return Guid.Empty
        End Try
    End Function


    ''' <summary>
    ''' Find a process by published web service name.
    ''' </summary>
    ''' <param name="sName">The process name to search for</param>
    ''' <param name="IncludeBusinessObjects">When True, business objects will be
    ''' searched as well as processes. Optional; defaults to False.</param>
    ''' <returns>The process ID, or Guid.Empty if the process was not found.</returns>
    <SecuredMethod(True)>
    Public Function GetProcessIDByWSName(ByVal sName As String, Optional ByVal IncludeBusinessObjects As Boolean = False) As Guid Implements IServer.GetProcessIDByWSName
        CheckPermissions()
        Using con = GetConnection()
            Try
                'Build query
                Dim sQuery As String = "SELECT * FROM BPAProcess WHERE wspublishname = @Name"
                If Not IncludeBusinessObjects Then
                    sQuery &= " AND ProcessType='P'"
                End If

                Dim cmd As New SqlCommand(sQuery)
                With cmd.Parameters
                    .AddWithValue("@Name", sName)
                End With
                Using reader = con.ExecuteReturnDataReader(cmd)

                    ' Loop through rows returned in the reader
                    Do While reader.Read()
                        Return CType(reader("processid"), Guid)
                    Loop

                    Return Guid.Empty
                End Using
            Catch
                Return Guid.Empty
            End Try
        End Using
    End Function

    <SecuredMethod(
        Permission.ObjectStudio.CreateBusinessObject,
        Permission.ProcessStudio.CreateProcess)>
    Public Sub CloneProcess(processId As Guid, name As String, version As String,
                             description As String, xml As String, overwriteId As Boolean,
                             isObject As Boolean, references As clsProcessDependencyList,
                             group As Guid, summary As String) Implements IServer.CloneProcess
        CheckPermissions()
        Using connection = GetConnection()
            connection.BeginTransaction()

            WriteProcess(connection, processId, name, version, description, xml, overwriteId, isObject,
                          references, group, False, False)

            ' Add audit record for the clone event
            Dim typeLabel = If(isObject, My.Resources.clsServer_BusinessObject, My.Resources.clsServer_Process)
            AuditRecordProcessOrVboEvent(connection,
                        ProcessOrVboEventCode.Clone, isObject, processId,
                        String.Format(My.Resources.clsServer_NewlyCreated0NameIs1WithGuid2, typeLabel, name, processId.ToString),
                        xml, summary)

            connection.CommitTransaction()
        End Using
        InvalidateCaches()
    End Sub

    ''' <inheritdoc />
    <SecuredMethod(
        Permission.ObjectStudio.CreateBusinessObject,
        Permission.ProcessStudio.CreateProcess)>
    Public Sub CreateProcess(processId As Guid, name As String, version As String,
                             description As String, xml As String, overwriteId As Boolean,
                             isObject As Boolean, references As clsProcessDependencyList,
                             group As Guid) Implements IServer.CreateProcess
        CheckPermissions()
        Using connection = GetConnection()
            connection.BeginTransaction()

            WriteProcess(connection, processId, name, version, description, xml, overwriteId, isObject,
                          references, group, False, False)

            ' Add audit record for the creation event
            Dim typeLabel = If(isObject, My.Resources.clsServer_BusinessObject, My.Resources.clsServer_Process)
            AuditRecordProcessOrVboEvent(connection,
                        ProcessOrVboEventCode.Create, isObject, processId,
                        String.Format(My.Resources.clsServer_0CreatedWithName1, typeLabel, name),
                        xml, String.Format(My.Resources.clsServer_0Created, typeLabel))

            connection.CommitTransaction()
        End Using
        InvalidateCaches()
    End Sub

    ''' <inheritdoc />
    <SecuredMethod(
        Permission.ObjectStudio.ImportBusinessObject,
        Permission.ProcessStudio.ImportProcess)>
    Public Sub ImportProcess(processId As Guid, name As String, version As String,
                             description As String, xml As String, overwriteId As Boolean,
                             isObject As Boolean, references As clsProcessDependencyList,
                             fileName As String) Implements IServer.ImportProcess
        CheckPermissions()
        Using connection = GetConnection()
            connection.BeginTransaction()
            Dim processExists As Boolean
            WriteProcess(connection, processId, name, version, description, xml, overwriteId, isObject,
                          references, Guid.Empty, False, processExists)

            Dim comments = String.Format(
                               If(processExists,
                                   If(isObject, My.Resources.clsServer_OverwritingObjectWithObjectImportedFromFile0,
                                                My.Resources.clsServer_OverwritingProcessWithProcessImportedFromFile0),
                                   If(isObject, My.Resources.clsServer_ObjectImportedFromFile0,
                                                My.Resources.clsServer_ProcessImportedFromFile0)),
                               fileName)

            AuditRecordProcessOrVboEvent(connection, ProcessOrVboEventCode.Import, isObject,
                                         processId, comments, xml, Nothing)

            connection.CommitTransaction()
        End Using
        InvalidateCaches()
    End Sub

    ''' <summary>
    ''' Writes the given process to the database using the specified parameters over the given
    ''' database connection.
    ''' </summary>
    ''' <param name="connection">The connection to the database to use.</param>
    ''' <param name="processId">The ID to use for the new process</param>
    ''' <param name="name">The name to use for the new process</param>
    ''' <param name="version">The version of the process to save</param>
    ''' <param name="description">The description of the process.</param>
    ''' <param name="xml">The XML describing the process body.</param>
    ''' <param name="overwriteId">True to force the new process to be saved
    ''' as a new version of an existing process if a process is found with
    ''' the specified ID.</param>
    ''' <param name="isObject">True if the new process is an object; False otherwise.
    ''' </param>
    ''' <param name="references">External process references</param>
    ''' <param name="group">Group to create the process in</param>
    ''' <param name="allowCreateOnRoot">NOTE: If set to true, will allow creating processes 
    ''' on the root of the tree rather forcing them into the Default group. Should only
    ''' be used if creating a process before knowing which group it will be placed in. 
    ''' It is up to the caller to enforce the 'No items on the root of the tree if it 
    ''' contains a default group' rule. 
    ''' This flag should only be used by server methods and not exposed as part of the
    ''' public IServer interface.</param>
    ''' <param name="processExists">True if the process was overwritten with a new version.</param>
    ''' <remarks>For best performance, assuming you have a clsProcess object when
    ''' you call this, you should also update the dependencies after you call this.
    ''' You do that using UpdateExternalDependencies().</remarks>
    Private Sub WriteProcess(connection As IDatabaseConnection,
                             processId As Guid, name As String, version As String,
                             description As String, xml As String, overwriteId As Boolean,
                             isObject As Boolean, references As clsProcessDependencyList,
                             group As Guid, allowCreateOnRoot As Boolean,
                             ByRef processExists As Boolean)

        Dim thing As String = My.Resources.clsServer_Process
        If isObject Then thing = My.Resources.clsServer_BusinessObject
        Dim lastModifiedBy As String = String.Empty
        Dim lastModifiedDate As Date
        If Not IsProcessNameUnique(connection, processId, Nothing, name, False, lastModifiedBy, lastModifiedDate) Then
            Throw New BluePrismException(
             My.Resources.clsServer_FailedToCreateTheNew0BecauseAProcessWithTheName1AlreadyExistsLastModifiedBy2On3,
             thing, name, lastModifiedBy, lastModifiedDate)
        End If

        If Not IsProcessNameUnique(connection, processId, Nothing, name, True, lastModifiedBy, lastModifiedDate) Then
            Throw New BluePrismException(
              My.Resources.clsServer_FailedToCreateTheNew0BecauseAnObjectWithTheName1AlreadyExistsLastModifiedBy2On3,
             thing, name, lastModifiedBy, lastModifiedDate)
        End If

        If Not ValidProcessName(name) Then
            Throw New BluePrismException(
              My.Resources.clsServer_FailedToCreateTheNew0BecauseTheObject1DoesNotPassNameValidationPleaseEnsureTher,
             thing, name)
        End If

        Dim modTime As Date = Date.Now
        Dim modUser As Guid = GetLoggedInUserId()

        ' Check if a process with this ID already exists... if it does we must
        ' jump through a few more hoops in order to import the process, otherwise
        ' we can just insert it and carry on.
        Dim existingProcessName As String = Nothing
        Using cmd As New SqlCommand()
            cmd.Parameters.AddWithValue("@id", processId)
            cmd.CommandText = "select name, processxml from BPAProcess where processid = @id"

            Using reader = connection.ExecuteReturnDataReader(cmd)
                If reader.Read() Then
                    existingProcessName = CStr(reader("name"))
                    ' Save the old XML so that the caller can 
                    processExists = True
                End If
            End Using
        End Using

        If existingProcessName Is Nothing Then ' ie. no process with that ID

            ' Check user can create a process in this group.
            Dim treetype = If(isObject, GroupTreeType.Objects, GroupTreeType.Processes)
            Dim hasDefault = HasDefaultGroup(connection, treetype)

            If Not (allowCreateOnRoot AndAlso group = Guid.Empty) Then

                If group = Guid.Empty AndAlso hasDefault Then
                    group = GetDefaultGroupId(connection, treetype)
                End If

                Dim perms = GetEffectiveGroupPermissions(group)

                Dim hasPermission = perms.HasPermission(GetUser(GetLoggedInUserId()),
                                    If(treetype = GroupTreeType.Processes,
                                    {Permission.ProcessStudio.CreateProcess, Permission.ProcessStudio.ImportProcess},
                                    {Permission.ObjectStudio.CreateBusinessObject, Permission.ObjectStudio.ImportBusinessObject}))

                If Not hasPermission Then
                    Throw New BluePrismException(String.Format(
                        My.Resources.clsServer_YouDoNotHavePermissionToCreateThisItemInThe0GroupIfYouStillWishToSaveYourChange,
                        GetGroup(connection, group).Name))
                End If

            End If

            ' Create the process in DB
            Using cmd As New SqlCommand()
                With cmd.Parameters
                    .AddWithValue("@id", processId)
                    .AddWithValue("@type", IIf(isObject, "O", "P"))
                    .AddWithValue("@name", name)
                    .AddWithValue("@description", description)
                    .AddWithValue("@version", version)
                    .AddWithValue("@user", modUser)
                    .AddWithValue("@currdate", modTime)
                    .AddWithValue("@xml", xml)
                    If references IsNot Nothing Then
                        .AddWithValue("@runmode", references.RunMode)
                        .AddWithValue("@shared", references.IsShared)
                    Else
                        .AddWithValue("@runmode", BusinessObjectRunMode.Background)
                        .AddWithValue("@shared", False)
                    End If
                End With

                ' So just insert the new one
                cmd.CommandText =
                 " insert into BPAProcess " &
                 "   (processid, processtype, name, description, version, createdate, createdby, " &
                 "    lastmodifieddate, lastmodifiedby, processxml, runmode, sharedobject) " &
                 " values " &
                 "   (@id, @type, @name, @description, @version, @currdate, @user," &
                 "    @currdate, @user, @xml, @runmode, @shared)"

                connection.Execute(cmd)
            End Using
            If references IsNot Nothing Then UpdateProcessDependencies(connection, processId, references)

            ' Add to the group
            Dim mem As ProcessBackedGroupMember
            If Not isObject Then
                mem = New ProcessGroupMember()
            Else
                mem = New ObjectGroupMember()
            End If


            mem.Id = processId
            mem.Name = name
            mem.Description = description

            AddToGroup(connection, group, mem)

            ' Just to ensure that no caller gets the wrong idea
            processExists = False

        Else ' ie. a process with the given ID exists

            ' If we've not been told to force an overwrite, error here and now.
            If Not overwriteId Then
                Throw New BluePrismException(
                 My.Resources.clsServer_CouldNotCreateProcessWithTheGivenIDItIsAlreadyInUseByTheProcess0, existingProcessName)
            End If

            ' Otherwise, force an overwrite on the existing process
            ' Lock it.
            LockProcess(connection, processId)
            ' Edit it
            EditProcess(connection, processId, isObject, name, version, description, xml, modTime, references)
            ' And unlock it again.
            UnlockProcess(connection, processId)

        End If
    End Sub


    ''' <summary>
    ''' Validation on a Process Name
    ''' </summary>
    ''' <param name="name">The process name</param>
    ''' <returns>True or False is successful validation.</returns>
    Private Function ValidProcessName(name As String) As Boolean

        If (String.IsNullOrEmpty(name) Or
            name.Equals("") Or
            RegularExpressions.Regex.Match(name, "[\r\n]+").Success) Then
            Return False
        End If

        Return True

    End Function


    ''' <summary>
    ''' Reads a process XML definition from the database.
    ''' </summary>
    ''' <param name="procId">The process ID</param>
    ''' <returns>The XML definition of the process.</returns>
    ''' <exception cref="NoSuchElementException">If no process with the given ID was
    ''' found on the database</exception>
    ''' <exception cref="SqlException">If any database errors occcur while attempting
    ''' to retrieve the XML for the process</exception>
    <SecuredMethod(True)>
    Public Function GetProcessXML(ByVal procId As Guid) As String Implements IServer.GetProcessXML
        CheckPermissions()

        Using connection = GetConnection()
            If GetControllingUserPermissionSetting(connection) Then
                CheckProcessPermissionsForGetProcessXML(connection, procId)
            End If
            Return GetProcessXML(connection, procId)
        End Using
    End Function

    ''' <summary>
    ''' Reads a process XML definition from the database.
    ''' </summary>
    ''' <param name="id">The process ID</param>
    ''' <returns>The XML definition of the process.</returns>
    ''' <exception cref="NoSuchElementException">If no process with the given ID was
    ''' found on the database</exception>
    ''' <exception cref="SqlException">If any database errors occcur while attempting
    ''' to retrieve the XML for the process</exception>
    Private Function GetProcessXML(ByVal con As IDatabaseConnection, ByVal id As Guid) As String
        Dim cmd As New SqlCommand("select processxml from BPAProcess where processID = @id")
        cmd.CommandTimeout = Options.Instance.SqlCommandTimeoutLong
        cmd.Parameters.AddWithValue("@id", id)

        Using reader = con.ExecuteReturnDataReader(cmd)
            If Not reader.Read() Then Throw New NoSuchElementException(
             My.Resources.clsServer_TheProcessDoesNotExistInTheDatabase)
            Return DirectCast(reader("processxml"), String)
        End Using
    End Function


    Private Sub CheckProcessPermissionsForGetProcessXML(con As IDatabaseConnection, ByVal procId As Guid)

        Dim processType = GetProcessType(con, procId)
        Dim processPerms = GetEffectiveGroupPermissionsForProcess(con, procId)
        Dim requiredPerms = New List(Of String)

        requiredPerms.AddRange(If(processType = DiagramType.Process, Permission.ProcessStudio.ImpliedViewProcess, Permission.ObjectStudio.ImpliedViewBusinessObject))
        requiredPerms.Add(If(processType = DiagramType.Process, Permission.ProcessStudio.ExecuteProcess, Permission.ObjectStudio.ExecuteBusinessObject))
        requiredPerms.Add(If(processType = DiagramType.Process, Permission.ProcessStudio.ExportProcess, Permission.ObjectStudio.ExportBusinessObject))

        If Not processPerms.HasPermission(mLoggedInUser, requiredPerms) Then
            Throw New PermissionException(My.Resources.clsServer_YouDoNotHavePermissionToUseThisProcess)
        End If

    End Sub

    ''' <summary>
    ''' Returns a list of process startup arguments with empty values
    ''' </summary>
    ''' <param name="procId"></param>
    ''' <returns></returns>
    <SecuredMethod(True)>
    Public Function GetBlankProcessArguments(procId As Guid) As List(Of clsArgument) Implements IServer.GetBlankProcessArguments
        CheckPermissions()
        Using con = GetConnection()
            Return GetBlankProcessArguments(con, procId)
        End Using
    End Function


    Private Function GetBlankProcessArguments(con As IDatabaseConnection, procId As Guid) As List(Of clsArgument)

        Dim results = New List(Of clsArgument)

        Dim sXML = GetProcessXML(con, procId)
        Dim sErr = String.Empty
        Dim process = clsProcess.FromXML(Options.Instance.GetExternalObjectsInfo(), sXML, False, sErr)
        If process Is Nothing OrElse process.StartStage Is Nothing Then Return results

        results = process.StartStage.GetParameters().
            Where(Function(x) x.ParamType <> DataType.collection).
            Select(Function(x) New clsArgument(x.Name, New clsProcessValue(x.ParamType))).
            ToList()

        Return results

    End Function


    ''' <summary>
    ''' Reads the last modified date of a process from the database.
    ''' </summary>
    ''' <param name="gProcessID">The process id</param>
    ''' <returns>The date/time the process was modified, or Nothing if not found.
    ''' </returns>
    <SecuredMethod(True)>
    Public Function GetProcessLastModified(ByVal gProcessID As Guid) As Date Implements IServer.GetProcessLastModified
        CheckPermissions()

        Using con = GetConnection()
            Try
                Dim cmd As New SqlCommand("SELECT lastmodifieddate FROM BPAProcess where processID = @ProcessID")
                With cmd.Parameters
                    .AddWithValue("@ProcessID", gProcessID.ToString)
                End With
                Using reader = con.ExecuteReturnDataReader(cmd)
                    If reader.Read() Then
                        Return CDate(reader("lastmodifieddate"))
                    Else
                        Return Nothing
                    End If
                End Using
            Catch
            End Try
        End Using

        Return Nothing
    End Function

    ''' <summary>
    ''' Get the user id of the user who last modified the process.
    ''' </summary>
    ''' <param name="gProcessID">The process id</param>
    ''' <returns>The user id of the user who last modified the process, or Nothing if not found.
    ''' </returns>
    <SecuredMethod(True)>
    Public Function GetProcessLastModifiedBy(ByVal gProcessID As Guid) As Guid Implements IServer.GetProcessLastModifiedBy
        CheckPermissions()
        Using con = GetConnection()
            Using cmd As New SqlCommand("SELECT lastmodifiedby FROM BPAProcess where processID = @ProcessID")
                With cmd.Parameters
                    .AddWithValue("@ProcessID", gProcessID)
                End With
                Using reader = con.ExecuteReturnDataReader(cmd)

                    If reader.Read() Then
                        Return CType(reader("lastmodifiedby"), Guid)
                    Else
                        Return Nothing
                    End If
                End Using
            End Using
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function GetProcessXMLAndAssociatedData(gProcessID As Guid) As ProcessDetails Implements IServer.GetProcessXMLAndAssociatedData
        CheckPermissions()

        Static CheckForZipFlag As Boolean = False
        Static ZipXML As Boolean = False
        If Not CheckForZipFlag Then
            ZipXML = GetPref(PreferenceNames.XmlSettings.ZipXmlTransfer, True)
            CheckForZipFlag = True
        End If

        Using connection = GetConnection()

            If GetControllingUserPermissionSetting(connection) Then
                CheckProcessPermissionsForGetProcessXML(connection, gProcessID)
            End If

            Try
                Dim cmd As New SqlCommand("SELECT processxml,lastmodifieddate,attributeid FROM BPAProcess where processID = @ProcessID")
                cmd.CommandTimeout = Options.Instance.SqlCommandTimeoutLong
                With cmd.Parameters
                    .AddWithValue("@ProcessID", gProcessID.ToString)
                End With
                Using reader = connection.ExecuteReturnDataReader(cmd)
                    If reader.Read() Then

                        Dim rawProcessXML = CStr(reader("processxml"))
                        Dim processBytes As Byte()

                        If ZipXML Then
                            processBytes = GZipCompression.Compress(rawProcessXML)
                        Else
                            processBytes = Encoding.Unicode.GetBytes(rawProcessXML)
                        End If

                        Return New ProcessDetails() With {.Zipped = ZipXML,
                            .Xml = processBytes,
                            .LastModified = CDate(reader("lastmodifieddate")),
                            .Attributes = DirectCast(reader("attributeid"), ProcessAttributes)}
                    End If
                End Using
            Catch
            End Try
        End Using
        Return New ProcessDetails With { .Attributes = ProcessAttributes.None }
    End Function

    ''' <summary>
    ''' Get a list of the IDs of all the processes in the database. This includes
    ''' all types and statuses, without exception.
    ''' </summary>
    ''' <returns>A list of Guids, or Nothing if an error occurred.</returns>
    <SecuredMethod(True)>
    Public Function GetAllProcessIDs() As List(Of Guid) Implements IServer.GetAllProcessIDs
        CheckPermissions()

        Using con = GetConnection()
            Try
                Dim cmd As New SqlCommand("SELECT processid FROM bpaprocess")
                Using dt = con.ExecuteReturnDataTable(cmd)
                    Dim lst As New List(Of Guid)
                    For Each row As DataRow In dt.Rows
                        lst.Add(CType(row("processid"), Guid))
                    Next
                    Return lst
                End Using
            Catch
                Return Nothing
            End Try
        End Using

    End Function

    ''' <summary>
    ''' Gets all non-retired processes into a data table of the same format as that
    ''' returned by <see cref="GetProcesses"/>
    ''' </summary>
    ''' <returns>If no error occurs, returns a DataTable containing the process id,
    ''' name, version, description, status, and a 0/1 "locked" integer column
    ''' indicating if the process is locked. If an error occurs, returns Nothing
    ''' </returns>
    <SecuredMethod(True)>
    Public Function GetAvailableProcesses() As DataTable Implements IServer.GetAvailableProcesses
        CheckPermissions()
        Using con = GetConnection()
            Return GetProcesses(con, ProcessAttributes.None, ProcessAttributes.Retired, False)
        End Using
    End Function

    ''' <summary>
    ''' Gets the currently available processes matching the desired statuses.
    ''' Processes are ordered by name.
    ''' </summary>
    ''' <param name="requiredAttributes">Statuses which are required. Only processes
    ''' with this (these) status(es) will be selected. Setting this parameter to
    ''' ProcessAttributes.None will mean all processes are selected.</param>
    ''' <param name="unacceptableAttributes">Statuses which not allowed. Processes
    ''' which have this (these) statuses will not be returned even if they have the
    ''' required statuses as specified in the RequiredStatuses parameter. Setting this
    ''' to ProcessAtrributes.None will mean no processes are filtered out.</param>
    ''' <param name="useBusinessObjects">Determines whether we fetch business objects
    ''' or processes.</param>
    ''' <returns>If no error occurs, returns a DataTable containing the process id,
    ''' name, version, description, status, and a 0/1 "locked" integer column
    ''' indicating if the process is locked. If an error occurs, returns Nothing.</returns>
    <SecuredMethod(True)>
    Public Function GetProcesses(ByVal requiredAttributes As ProcessAttributes,
                                 ByVal unacceptableAttributes As ProcessAttributes,
                                 Optional ByVal useBusinessObjects As Boolean = False) As DataTable Implements IServer.GetProcesses
        CheckPermissions()
        Using con = GetConnection()
            Try
                Return GetProcesses(con,
                    requiredAttributes, unacceptableAttributes, useBusinessObjects)
            Catch
                Return Nothing
            End Try
        End Using
    End Function


    ''' <summary>
    ''' Gets the currently available processes matching the desired statuses.
    ''' Processes are ordered by name.
    ''' </summary>
    ''' <param name="requiredAttributes">Statuses which are required. Only processes
    ''' with this (these) status(es) will be selected. Setting this parameter to
    ''' ProcessAttributes.None will mean all processes are selected.</param>
    ''' <param name="unacceptableAttributes">Statuses which not allowed. Processes
    ''' which have this (these) statuses will not be returned even if they have the
    ''' required statuses as specified in the RequiredStatuses parameter. Setting this
    ''' to ProcessAtrributes.None will mean no processes are filtered out.</param>
    ''' <param name="useBusinessObjects">Determines whether we fetch business objects
    ''' or processes.</param>
    ''' <returns>If no error occurs, returns a DataTable containing the process id,
    ''' name, version, description, status, and a 00/1 "locked" integer column
    ''' indicating if the process is locked. If an error occurs, returns Nothing.</returns>
    Private Function GetProcesses(con As IDatabaseConnection,
     ByVal requiredAttributes As ProcessAttributes,
     ByVal unacceptableAttributes As ProcessAttributes,
     ByVal useBusinessObjects As Boolean) As DataTable

        Dim cmd As New SqlCommand()

        Dim sb As New StringBuilder(
         " select" &
         "   p.processid," &
         "   p.[name]," &
         "   p.version," &
         "   p.[description]," &
         "   p.attributeid," &
         "   p.wspublishname," &
         "   case isnull(cast(pl.processid as varchar(36)), '')" &
         "     when '' then 0" &
         "     else 1" &
         "   end as locked" &
         " from BPAProcess p" &
         "   left outer join BPAProcessLock pl on p.processid = pl.processid" &
         " where p.processtype = @proctype"
        )
        cmd.Parameters.AddWithValue("@proctype", IIf(useBusinessObjects, "O", "P"))

        If requiredAttributes <> ProcessAttributes.None Then
            cmd.Parameters.AddWithValue("@wanted", CInt(requiredAttributes))
            sb.Append(" and (p.AttributeID & @wanted != 0)")
        End If

        If unacceptableAttributes <> ProcessAttributes.None Then
            cmd.Parameters.AddWithValue("@unwanted", CInt(unacceptableAttributes))
            sb.Append(" and (p.AttributeID & @unwanted = 0)")
        End If

        sb.Append(" order by p.[name]")

        cmd.CommandText = sb.ToString()

        Return con.ExecuteReturnDataTable(cmd)

    End Function

    ''' <summary>
    ''' Overloaded function. Returns all processes regardless of status. To filter
    ''' on status use overloaded method.
    ''' </summary>
    ''' <returns>If no error occurs, returns a datatable containing the process' id, name, version and description, and
    ''' a 0/1 "locked" integer column indicating if the process is locked. If an
    ''' error occurs, returns nothing.</returns>
    <SecuredMethod(True)>
    Public Function GetProcesses(Optional ByVal bUseBusinessObjects As Boolean = False) As DataTable Implements IServer.GetProcesses
        CheckPermissions()
        Return GetProcesses(ProcessAttributes.None, ProcessAttributes.None, bUseBusinessObjects)
    End Function

    ''' <summary>
    ''' Overloads GetProcesses(). The results are limited to those processes whose
    ''' guid is in the supplied list.
    ''' </summary>
    ''' <param name="GuidList">A list of process ids</param>
    ''' <returns>Returns a datatable containing same information as GetProcesses()
    ''' but only returns rows corresponding to the processes specified in the 
    ''' argument 'GuidList'.</returns>
    <SecuredMethod(True)>
    Public Function GetProcesses(ByVal GuidList As List(Of Guid), Optional ByVal bUseBusinessObjects As Boolean = False) As DataTable Implements IServer.GetProcesses
        CheckPermissions()
        Dim dt As DataTable = GetProcesses(bUseBusinessObjects)
        For i As Integer = dt.Rows.Count - 1 To 0 Step -1
            If Not GuidList.Contains(CType(dt.Rows(i)("ProcessID"), Guid)) Then dt.Rows.RemoveAt(i)
        Next
        Return dt
    End Function


    ''' <summary>
    ''' Gets the number of published process (excluding retired processes).
    ''' </summary>
    ''' <returns>Returns the number of published processes.</returns>
    Friend Function GetPublishedProcessCount(con As IDatabaseConnection) As Integer
        Dim count As Integer = 0
        Dim dt As DataTable = GetProcesses(con, ProcessAttributes.Published, ProcessAttributes.Retired, False)
        If dt IsNot Nothing Then count += dt.Rows.Count

        dt = GetProcesses(con, ProcessAttributes.PublishedWS, ProcessAttributes.Retired, False)
        If dt IsNot Nothing Then count += dt.Rows.Count

        Return count
    End Function

    ''' <summary>
    ''' Publishes the specified Processes so that it is available within Control Room
    ''' </summary>
    ''' <param name="processId">The ID of the Process to publish</param>
    <SecuredMethod()>
    Public Sub PublishProcess(processId As Guid) Implements IServer.PublishProcess

        ' Check role-based permissions
        CheckPermissions(Permission.ProcessStudio.ImpliedEditProcess)

        Using con = GetConnection()
            ' Check group-based permissions
            Dim member = GetEffectiveMemberPermissionsForProcess(processId)
            If member.IsRestricted AndAlso Not member.HasPermission(mLoggedInUser, Permission.ProcessStudio.ImpliedEditProcess) Then
                Throw New PermissionException(
                    My.Resources.clsServer_YouDoNotHavePermissionToPublishThisProcess)
            End If

            ' Check process is not already published
            Dim currentAttributes = GetProcessAttributes(con, processId)
            If (currentAttributes And ProcessAttributes.Published) <> 0 Then Throw New InvalidStateException(
                My.Resources.clsServer_ThisProcessIsAlreadyPublished)

            ' Check licensie allows another published process
            If Not CanPublishProcesses(con, 1) Then Throw New LicenseRestrictionException(
                Licensing.GetOperationDisallowedMessage(Licensing.MaxPublishedProcessesLimitReached))

            ' Make the change and audit it
            con.BeginTransaction()
            AddToProcessAttributes(con, processId, ProcessAttributes.Published)
            AuditRecordProcessOrVboEvent(con, ProcessOrVboEventCode.ChangedAttributes, False, processId,
                My.Resources.clsServer_AddedThePublishedAttributeUsingThePublishCommandIssuedFromTheCommandLine, Nothing, Nothing)
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Unpublishes the specified Processes so that it is not available within Control Room
    ''' </summary>
    ''' <param name="processId">The ID of the Process to unpublish</param>
    <SecuredMethod()>
    Public Sub UnpublishProcess(processId As Guid) Implements IServer.UnpublishProcess

        ' Check role-based permissions
        CheckPermissions(Permission.ProcessStudio.ImpliedEditProcess)

        Using con = GetConnection()
            ' Check group-based permissions
            Dim member = GetEffectiveMemberPermissionsForProcess(processId)
            If member.IsRestricted AndAlso Not member.HasPermission(mLoggedInUser, Permission.ProcessStudio.ImpliedEditProcess) Then
                Throw New PermissionException(
                    My.Resources.clsServer_YouDoNotHavePermissionToUnpublishThisProcess)
            End If

            ' Check process is currently published
            Dim currentAttributes = GetProcessAttributes(con, processId)
            If (currentAttributes And ProcessAttributes.Published) = 0 Then Throw New InvalidStateException(
                My.Resources.clsServer_ThisProcessIsNotCurrentlyPublished)

            ' Make the change and audit it
            con.BeginTransaction()
            SubtractFromProcessAttributes(con, processId, ProcessAttributes.Published)
            AuditRecordProcessOrVboEvent(con, ProcessOrVboEventCode.ChangedAttributes, False, processId,
                My.Resources.clsServer_RemovedThePublishedAttributeUsingTheUnpublishCommandIssuedFromTheCommandLine, Nothing, Nothing)
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Exposes the specified Process as a Web Service.
    ''' </summary>
    ''' <param name="processId">The ID of the Process to expose</param>
    ''' <param name="details">The Web Service parameters</param>
    <SecuredMethod(Permission.SystemManager.Processes.Exposure)>
    Public Sub ExposeProcessAsWebService(processId As Guid, details As WebServiceDetails) Implements IServer.ExposeProcessAsWebService
        CheckPermissions()
        Using connection = GetConnection()
            If (CheckUserCanExposeOrConcealGroupMember(processId, GroupMemberType.Process, connection)) Then
                connection.BeginTransaction()
                ExposeAsWebservice(connection, processId, details)
                AuditRecordProcessOrVboEvent(connection, ProcessOrVboEventCode.ChangedAttributes, False, processId,
                                             My.Resources.clsServer_AddedThePublishedWebServiceAttribute, Nothing, Nothing)
                connection.CommitTransaction()
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Exposes the specified Object as a Web Service.
    ''' </summary>
    ''' <param name="objectId">The ID of the Object to expose</param>
    ''' <param name="details">The Web Service parameters</param>
    <SecuredMethod(Permission.SystemManager.BusinessObjects.Exposure)>
    Public Sub ExposeObjectAsWebService(objectId As Guid, details As WebServiceDetails) Implements IServer.ExposeObjectAsWebService
        CheckPermissions()
        Using connection = GetConnection()
            If (CheckUserCanExposeOrConcealGroupMember(objectId, GroupMemberType.Object, connection)) Then
                connection.BeginTransaction()
                ExposeAsWebservice(connection, objectId, details)
                AuditRecordProcessOrVboEvent(connection, ProcessOrVboEventCode.ChangedAttributes, True, objectId,
                                             My.Resources.clsServer_AddedThePublishedWebServiceAttribute, Nothing, Nothing)
                connection.CommitTransaction()
            End If
        End Using
    End Sub

    Private Sub ExposeAsWebservice(con As IDatabaseConnection, id As Guid, details As WebServiceDetails)

        ' Check if it is already exposed
        Dim currentAttributes = GetProcessAttributes(con, id)
        If (currentAttributes And ProcessAttributes.PublishedWS) <> 0 Then Throw New InvalidStateException(
                My.Resources.clsServer_ThisProcessObjectIsAlreadyExposedAsAWebService)

        ' Validate web service options
        If details.IsDocumentLiteral AndAlso details.UseLegacyNamespaceStructure Then Throw New BluePrismException(
                My.Resources.clsServer_YouCanNotUseALegacyNamespaceWhenYouEnforceTheDocumentLiteralEncodingType)

        ' Set the Web Service name and attribute
        SetProcessWSDetails(con, id, details)
        AddToProcessAttributes(con, id, ProcessAttributes.PublishedWS)

    End Sub

    ''' <summary>
    ''' Conceals the specified exposed Process
    ''' </summary>
    ''' <param name="processId">The ID of the Process to be concealed</param>
    <SecuredMethod(Permission.SystemManager.Processes.Exposure)>
    Public Sub ConcealProcessWebService(processId As Guid) Implements IServer.ConcealProcessWebService
        CheckPermissions()
        Using connection = GetConnection()
            If (CheckUserCanExposeOrConcealGroupMember(processId, GroupMemberType.Process, connection)) Then
                connection.BeginTransaction()
                ConcealWebService(connection, processId)
                AuditRecordProcessOrVboEvent(connection, ProcessOrVboEventCode.ChangedAttributes, False, processId,
                                             My.Resources.clsServer_RemovedThePublishedWebServiceAttribute, Nothing, Nothing)
                connection.CommitTransaction()
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Conceals the specified exposed Object
    ''' </summary>
    ''' <param name="objectId">The ID of the Object to be concealed</param>
    <SecuredMethod(Permission.SystemManager.BusinessObjects.Exposure)>
    Public Sub ConcealObjectWebService(objectId As Guid) Implements IServer.ConcealObjectWebService
        CheckPermissions()
        Using connection = GetConnection()
            If (CheckUserCanExposeOrConcealGroupMember(objectId, GroupMemberType.Object, connection)) Then
                connection.BeginTransaction()
                ConcealWebService(connection, objectId)
                AuditRecordProcessOrVboEvent(connection, ProcessOrVboEventCode.ChangedAttributes, True, objectId,
                                             My.Resources.clsServer_RemovedThePublishedWebServiceAttribute, Nothing, Nothing)
                connection.CommitTransaction()
            End If
        End Using
    End Sub

    Private Sub ConcealWebService(con As IDatabaseConnection, id As Guid)

        ' Check it is currently exposed
        Dim currentAttributes = GetProcessAttributes(con, id)
        If (currentAttributes And ProcessAttributes.PublishedWS) = 0 Then Throw New InvalidStateException(
                My.Resources.clsServer_ThisProcessObjectIsNotCurrentlyExposedAsAWebService)

        SubtractFromProcessAttributes(con, id, ProcessAttributes.PublishedWS)

    End Sub

    Private Function CheckUserCanExposeOrConcealGroupMember(groupMemberId As Guid,
                                                            groupMemberType As GroupMemberType,
                                                            connection As IDatabaseConnection) As Boolean
        Dim groupMember = GetEffectiveMemberPermissionsForProcess(connection, groupMemberId)
        Dim exposePermission As String
        Dim viewPermission As String()

        Select Case groupMemberType
            Case GroupMemberType.Process
                exposePermission = Permission.SystemManager.Processes.Exposure
                viewPermission = Permission.ProcessStudio.ImpliedViewProcess
            Case GroupMemberType.Object
                exposePermission = Permission.SystemManager.BusinessObjects.Exposure
                viewPermission = Permission.ObjectStudio.ImpliedViewBusinessObject
            Case Else
                Throw New InvalidArgumentException(My.Resources.clsServer_UnexpectedGroupMemberTypeProvided)
        End Select

        If (mLoggedInUser.HasPermission(exposePermission) AndAlso
            groupMember.HasPermission(mLoggedInUser, viewPermission)) Then
            Return True
        Else
            Throw New PermissionException(My.Resources.clsServer_YouDoNotHavePermissionToExposeThisProcess)
        End If
    End Function

    ''' <summary>
    ''' Sets the Web Service publishing name of the specified process.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="processId">The process ID</param>
    ''' <param name="details">The Web Service Parameters</param>
    Private Sub SetProcessWSDetails(con As IDatabaseConnection, processId As Guid, details As WebServiceDetails)

        Using command As New SqlCommand(" UPDATE BPAProcess SET wspublishname = @Name," &
                                    " forceLiteralForm = @Literal, " &
                                    " useLegacyNamespace = @UseLegacyNamespace " &
                                    " WHERE ProcessID = @ProcessID")
            With command.Parameters
                .AddWithValue("@Name", details.WebServiceName)
                .AddWithValue("@Literal", details.IsDocumentLiteral())
                .AddWithValue("@UseLegacyNamespace", details.UseLegacyNamespaceStructure)
                .AddWithValue("@ProcessID", processId)
            End With
            con.Execute(command)
        End Using
    End Sub

    ''' <summary>
    ''' Gets the Web Service publishing name of the specified process.
    ''' </summary>
    ''' <param name="procid">The process.</param>
    ''' <returns>Returns the name.</returns>
    <SecuredMethod(True)>
    Public Function GetProcessWSDetails(ByVal procid As Guid) As WebServiceDetails Implements IServer.GetProcessWSDetails
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand(
             "SELECT wspublishname, forceLiteralForm, useLegacyNamespace FROM BPAProcess WHERE ProcessID = @ProcessID")
            With cmd.Parameters
                .AddWithValue("@ProcessID", procid)
            End With

            Dim dataTable = con.ExecuteReturnDataTable(cmd)

            Dim dataList = dataTable.Rows.Cast(Of DataRow).ToList(0).ItemArray.ToList()

            Return New WebServiceDetails(IfNull(dataList(0).ToString(), String.Empty), CType(dataList(1), Boolean), CType(dataList(2), Boolean))
        End Using
    End Function

    ''' <summary>
    ''' Ensures that the setting of specific process attributes is valid for a given
    ''' process.
    ''' </summary>
    ''' <param name="con">The connection to use to test the process</param>
    ''' <param name="procId">The ID of the process which is having attributes set
    ''' on it.</param>
    ''' <param name="attrs">The attributes which are to be set on the process</param>
    ''' <remarks>If this method completes successfully, the validity of the setting
    ''' of the attributes is ensured; otherwise an exception will be raised which
    ''' explains the invalidity</remarks>
    ''' <exception cref="LicenseRestrictionException">If the attributes include
    ''' <see cref="ProcessAttributes.Published"/> and the licence does not allow
    ''' any further processes to be published</exception>
    ''' <exception cref="ForeignKeyDependencyException">If the attributes include
    ''' <see cref="ProcessAttributes.Retired"/> and the process is currently
    ''' <see cref="clsWorkQueue.ProcessId">assigned to an active queue</see>
    ''' </exception>
    Private Sub EnsureValidProcessAttributes(
     con As IDatabaseConnection, procId As Guid, attrs As ProcessAttributes)
        ' If publishing; ensure that we can publish another process
        If attrs.HasFlag(ProcessAttributes.Published) AndAlso
         Not CanPublishProcesses(con, 1) Then
            Throw New LicenseRestrictionException(
                Licensing.GetOperationDisallowedMessage(Licensing.MaxPublishedProcessesLimitReached))
        End If

        ' If retiring, ensure that it is not assigned to an active queue
        If attrs.HasFlag(ProcessAttributes.Retired) Then
            Dim queueNames As ICollection(Of String) =
                GetQueuesAssignedWithProcess(con, procId)
            If queueNames.Count > 0 Then
                Throw New ForeignKeyDependencyException(
                    My.Resources.clsServer_ThisProcessCannotBeRetiredItIsAssignedToTheFollowingActiveQueues0, CollectionUtil.Join(queueNames, "; "))
            End If
        End If

    End Sub

    ''' <summary>
    ''' Sets the status of the specified process.
    ''' </summary>
    ''' <param name="procId">The process to update</param>
    ''' <param name="attrs">The process attributes</param>
    ''' <remarks>If you just want to add to the status the use the method
    ''' <see cref="AddToProcessAttributes"/> or
    ''' <see cref="SubtractFromProcessAttributes"/></remarks>
    <SecuredMethod(True)>
    Public Sub SetProcessAttributes(
     ByVal procId As Guid, ByVal attrs As ProcessAttributes) Implements IServer.SetProcessAttributes
        CheckPermissions()
        Using con = GetConnection()
            SetProcessAttributes(con, procId, attrs)
        End Using
    End Sub


    Private Sub OverwriteProcessAttributes(
                                          con As IDatabaseConnection,
                                          procId As Guid,
                                          originalAttributes As ProcessAttributes,
                                          newAtrributes As ProcessAttributes)

        ' Check the licence if we are adding a published attribute
        If newAtrributes.HasFlag(ProcessAttributes.Published) AndAlso
         Not originalAttributes.HasFlag(ProcessAttributes.Published) AndAlso
         Not CanPublishProcesses(con, 1) Then
            Throw New LicenseRestrictionException(
                Licensing.GetOperationDisallowedMessage(Licensing.MaxPublishedProcessesLimitReached))
        End If

        ' If retiring, ensure that it is not assigned to an active queue
        If newAtrributes.HasFlag(ProcessAttributes.Retired) AndAlso
            Not originalAttributes.HasFlag(ProcessAttributes.Retired) Then
            Dim queueNames As ICollection(Of String) =
                GetQueuesAssignedWithProcess(con, procId)
            If queueNames.Count > 0 Then
                Throw New ForeignKeyDependencyException(
                    My.Resources.clsServer_ThisProcessCannotBeRetiredItIsAssignedToTheFollowingActiveQueues0, CollectionUtil.Join(queueNames, "; "))
            End If
        End If

        Dim cmd As New SqlCommand(
         "update BPAProcess set attributeid = @attr where processid = @procid")
        With cmd.Parameters
            .AddWithValue("@attr", CInt(newAtrributes))
            .AddWithValue("@procid", procId)
        End With
        con.Execute(cmd)

    End Sub

    ''' <summary>
    ''' Sets the attributes of the specified process. Note that this is internal to
    ''' clsServer and will throw an exception if any errors occur rather than
    ''' returning flags.
    ''' </summary>
    ''' <param name="con">The connection to the database to use to set the attributes
    ''' </param>
    ''' <param name="procId">The process to update</param>
    ''' <param name="attrs">The process attributes</param>
    Private Sub SetProcessAttributes(ByVal con As IDatabaseConnection,
     ByVal procId As Guid, ByVal attrs As ProcessAttributes)
        EnsureValidProcessAttributes(con, procId, attrs)
        Dim cmd As New SqlCommand(
         "update BPAProcess set attributeid = @attr, lastmodifieddate = @lastModified where processid = @procid")
        With cmd.Parameters
            .AddWithValue("@attr", CInt(attrs))
            .AddWithValue("@procid", procId)
            .AddWithValue("@lastModified", DateTime.UtcNow)
        End With
        con.Execute(cmd)
    End Sub

    ''' <summary>
    ''' Adds the given process attributes to the specified process.
    ''' </summary>
    ''' <param name="connection">The connection to use</param>
    ''' <param name="processId">The process to update</param>
    ''' <param name="attributes">The attributes to add</param>
    Private Sub AddToProcessAttributes(
     connection As IDatabaseConnection, processId As Guid, attributes As ProcessAttributes)
        EnsureValidProcessAttributes(connection, processId, attributes)

        Using command As New SqlCommand(" update BPAProcess set" &
                                        " attributeid = attributeid | @attrs" &
                                        " where processid = @processid")
            With command.Parameters
                .AddWithValue("@attrs", CInt(attributes))
                .AddWithValue("@processid", processId)
            End With
            connection.Execute(command)
        End Using
    End Sub

    ''' <summary>
    ''' Removes the given set of process attributes from the specified process.
    ''' </summary>
    ''' <param name="con">The connection to use</param>
    ''' <param name="procId">The process to update</param>
    ''' <param name="attrs">The attributes to subtract</param>
    Private Sub SubtractFromProcessAttributes(
     con As IDatabaseConnection, procId As Guid, attrs As ProcessAttributes)
        Dim cmd As New SqlCommand(
            " update BPAProcess set" &
            " attributeid = attributeid & ~@attrs" &
            " where processid = @procid")
        With cmd.Parameters
            .AddWithValue("@attrs", attrs)
            .AddWithValue("@procid", procId)
        End With
        con.Execute(cmd)

    End Sub

    ''' <summary>
    ''' Gets the attributes of the specified process.
    ''' </summary>
    ''' <param name="processId">The id of the process for which the attributes are
    ''' required.</param>
    ''' <returns>The process attributes corresponding to the given process ID.
    ''' </returns>
    ''' <exception cref="NoSuchElementException">If no process with the given ID
    ''' was found on the database.</exception>
    <SecuredMethod(True)>
    Public Function GetProcessAttributes(processId As Guid) As ProcessAttributes Implements IServer.GetProcessAttributes
        CheckPermissions()
        Using con = GetConnection()
            Return GetProcessAttributes(con, processId)
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function GetProcessAttributesBulk(processIds As List(Of Guid)) As Dictionary(Of Guid, ProcessAttributes) Implements IServer.GetProcessAttributesBulk
        CheckPermissions()
        Using con = GetConnection()
            Return GetProcessAttributesBulk(con, processIds)
        End Using
    End Function


    Private Function GetProcessAttributes(con As IDatabaseConnection, processId As Guid) As ProcessAttributes
        Dim cmd As New SqlCommand(
                "select attributeid from BPAProcess where processid=@id")
        cmd.Parameters.AddWithValue("@id", processId)

        Dim attributes As Object = con.ExecuteReturnScalar(cmd)
        If attributes Is Nothing Then Throw New NoSuchElementException(My.Resources.clsServer_NoSuchProcess)

        Return DirectCast(attributes, ProcessAttributes)
    End Function

    Private Function GetProcessAttributesBulk(con As IDatabaseConnection, processIds As IEnumerable(Of Guid)) As Dictionary(Of Guid, ProcessAttributes)
        Dim attributesMap = New Dictionary(Of Guid, ProcessAttributes)

        Dim sql = "select processid, attributeid from BPAProcess where processid in ({0})"
        Dim sqlIn As New Text.StringBuilder
        For i As Integer = 0 To processIds.Count - 1
            sqlIn.Append($" @p{i}, ")
        Next
        Dim finishedIn = sqlIn.ToString
        finishedIn = finishedIn.Substring(0, finishedIn.Length - 2)

        Using command As New SqlCommand(String.Format(sql, finishedIn))
            command.AddEnumerable(Of Guid)("p", processIds)
            Using reader = con.ExecuteReturnDataReader(command)
                Dim provider As New ReaderDataProvider(reader)

                While reader.Read()
                    Dim attributes = provider.GetInt("attributeId")
                    attributesMap.Add(provider.GetGuid("processid"), DirectCast(attributes, ProcessAttributes))
                End While

                If Not attributesMap.Any Then Throw New NoSuchElementException(My.Resources.clsServer_NoSuchProcess)
            End Using
        End Using
        Return attributesMap
    End Function


    ''' <summary>
    ''' Mark the specified Process as retired
    ''' </summary>
    ''' <param name="processId">The ID of the Process to retire</param>
    <SecuredMethod(Permission.SystemManager.Processes.Management)>
    Public Sub RetireProcessOrObject(processId As Guid) Implements IServer.RetireProcessOrObject

        ' Check role-based permissions
        CheckPermissions()
        Using con = GetConnection()

            con.BeginTransaction()

            CheckProcessOrObjectPermissions(con, processId, "retire", {Permission.ProcessStudio.EditProcessGroups}, {Permission.ObjectStudio.EditObjectGroups})
            CheckProcessOrObjectPermissions(con, processId, "retire", {Permission.ProcessStudio.ManageProcessAccessRights}, {Permission.ObjectStudio.ManageBusinessObjectAccessRights})

            ChangeProcessOrObjectStatus(con, processId, GetProcessType(con, processId) = DiagramType.Object, True)

            AuditRecordProcessOrVboEvent(con, ProcessOrVboEventCode.ChangedAttributes,
                                         False, processId, My.Resources.clsServer_AddedTheRetiredAttribute, Nothing, Nothing)

            con.CommitTransaction()
        End Using
        InvalidateCaches()
    End Sub

    ''' <summary>
    ''' Mark the specified Process as unretired
    ''' </summary>
    ''' <param name="processId">The ID of the Process to unretire</param>
    <SecuredMethod(Permission.SystemManager.Processes.Management)>
    Public Sub UnretireProcessOrObject(processId As Guid, targetGroupId As Guid) Implements IServer.UnretireProcessOrObject

        ' Check role-based permissions
        CheckPermissions()
        Using con = GetConnection()

            Dim isVBO = GetProcessType(con, processId) = DiagramType.Object

            Dim editPermission = If(isVBO, Permission.ObjectStudio.EditObjectGroups, Permission.ProcessStudio.EditProcessGroups)
            Dim accecssRightsPermission = If(isVBO, Permission.ObjectStudio.ManageBusinessObjectAccessRights, Permission.ProcessStudio.ManageProcessAccessRights)


            'Check group-based permissions
            Dim targetGroupPermissions = GetEffectiveGroupPermissions(con, targetGroupId)
            If targetGroupPermissions.IsRestricted AndAlso
                (Not targetGroupPermissions.HasPermission(mLoggedInUser, editPermission) OrElse
                Not targetGroupPermissions.HasPermission(mLoggedInUser, accecssRightsPermission)) Then

                Throw New PermissionException(
                    My.Resources.clsServer_TheCurrentUserDoesNotHavePermissionToUnretireThisProcess)
            End If

            con.BeginTransaction()
            ChangeProcessOrObjectStatus(con, processId, isVBO, False)
            AuditRecordProcessOrVboEvent(con, ProcessOrVboEventCode.ChangedAttributes,
                                         False, processId, My.Resources.clsServer_RemovedTheRetiredAttribute, Nothing, Nothing)
            con.CommitTransaction()
        End Using
        InvalidateCaches()
    End Sub


    Private Sub ChangeProcessOrObjectStatus(con As IDatabaseConnection, id As Guid, isVBO As Boolean, retiring As Boolean)
        Dim typeName = If(isVBO, My.Resources.clsServer_ChangeProcessOrObjectStatus_Object, My.Resources.clsServer_ChangeProcessOrObjectStatus_Process)
        Dim attributes = GetProcessAttributes(con, id)

        If retiring AndAlso (attributes And ProcessAttributes.Retired) <> 0 Then Throw New InvalidStateException(
            My.Resources.clsServer_This0IsAlreadyRetired, typeName)

        If Not retiring AndAlso (attributes And ProcessAttributes.Retired) = 0 Then Throw New InvalidStateException(
            My.Resources.clsServer_This0IsNotCurrentlyRetired, typeName)

        If retiring Then
            AddToProcessAttributes(con, id, ProcessAttributes.Retired)
        Else
            SubtractFromProcessAttributes(con, id, ProcessAttributes.Retired)
        End If
    End Sub

#Region "Auto-save stuff"

    ''' <summary>
    ''' Updates the default back up interval
    ''' </summary>
    ''' <param name="minutes">The interval</param>
    <SecuredMethod(Permission.SystemManager.System.Settings,
        Permission.ProcessStudio.CreateProcess, Permission.ProcessStudio.EditProcess,
        Permission.ObjectStudio.CreateBusinessObject, Permission.ObjectStudio.EditBusinessObject)>
    Public Sub AutoSaveWriteInterval(ByVal minutes As Long) Implements IServer.AutoSaveWriteInterval
        CheckPermissions()
        Dim con = GetConnection()
        Try

            Dim cmd As New SqlCommand("UPDATE BPASysconfig SET autosaveinterval = @Minutes WHERE id=1")
            With cmd.Parameters
                .AddWithValue("@Minutes", minutes.ToString)
            End With
            con.Execute(cmd)

            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifyAutoSaveInterval, String.Empty, minutes.ToString())
        Finally
            con.Close()
        End Try
    End Sub



    ''' <summary>
    ''' Reads the back up interval from the database.
    ''' </summary>
    ''' <returns>The interval</returns>
    <SecuredMethod()>
    Public Function AutoSaveReadInterval() As Integer Implements IServer.AutoSaveReadInterval
        CheckPermissions()
        Const DefaultInterval As Integer = 10
        Try
            Dim minutes As Object

            Dim con = GetConnection()
            Try

                Dim cmd As New SqlCommand("SELECT ISNULL(autosaveinterval, 10) FROM BPASysconfig WHERE id=1")
                minutes = con.ExecuteReturnScalar(cmd)
            Finally
                con.Close()
            End Try

            If minutes Is Nothing Then
                Return DefaultInterval
            Else
                Return CType(minutes, Short)
            End If
        Catch
            Return DefaultInterval
        End Try
    End Function


    <SecuredMethod(True)>
    Public Function AutoSaveGetBackupDateTime(processID As Guid) As DateTime Implements IServer.AutoSaveGetBackupDateTime
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("SELECT BackupDate FROM BPAProcessBackup WHERE ProcessID=@ProcessID")
            cmd.Parameters.AddWithValue("@ProcessID", processID)
            Return DateTime.SpecifyKind(CType(con.ExecuteReturnScalar(cmd), DateTime), DateTimeKind.Local)
        End Using
    End Function

    <SecuredMethod(True)>
    Public Sub AutoSaveGetBackupXML(ByVal ProcessID As Guid, ByRef sXML As String) Implements IServer.AutoSaveGetBackupXML
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("SELECT ProcessXML FROM BPAProcessBackup WHERE ProcessID=@ProcessID")
            With cmd.Parameters
                .AddWithValue("@ProcessID", ProcessID.ToString)
            End With
            sXML = CStr(con.ExecuteReturnScalar(cmd))
        End Using
    End Sub

    ''' <summary>
    ''' Indicates whether a yet to be back up of a process exists 
    ''' and therfore whether should be recovered.
    ''' </summary>
    ''' <param name="processId">The process Guid</param>
    ''' <returns>True if a back up exists.</returns>
    <SecuredMethod(True)>
    Public Function AutoSaveBackupSessionExistsForProcess(ByVal processId As Guid) As Boolean Implements IServer.AutoSaveBackupSessionExistsForProcess
        CheckPermissions()
        Return AutoSaveBackupSessionExistsForProcess(processId.ToString)
    End Function

    ''' <summary>
    ''' Indicates whether a yet to be back up of a process exists 
    ''' and therfore whether should be recovered.
    ''' </summary>
    ''' <param name="processId">The process Guid</param>
    ''' <returns>True if a back up exists.</returns>
    <SecuredMethod(True)>
    Public Function AutoSaveBackupSessionExistsForProcess(ByVal processId As String) As Boolean Implements IServer.AutoSaveBackupSessionExistsForProcess
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand(
             "SELECT 1 FROM BPAProcessBackUp WHERE processid=@id")
            cmd.Parameters.AddWithValue("@id", processId)
            Return (IfNull(con.ExecuteReturnScalar(cmd), 0) = 1)
        End Using
    End Function

    ''' <summary>
    ''' Deletes an auto-saved back up from the database
    ''' </summary>
    ''' <param name="processId">The process id</param>
    <SecuredMethod(True)>
    Public Sub DeleteProcessAutoSaves(ByVal processId As Guid) Implements IServer.DeleteProcessAutoSaves
        CheckPermissions()
        Using con = GetConnection()
            DeleteProcessAutoSaves(con, processId)
        End Using
    End Sub

    ''' <summary>
    ''' Deletes an autosaved backup on a specified connection
    ''' </summary>
    ''' <param name="con">The connection to use to delete the autosaved process.
    ''' </param>
    ''' <param name="processId">The ID of the process whose autosave should be
    ''' deleted</param>
    Private Sub DeleteProcessAutoSaves(
     ByVal con As IDatabaseConnection, ByVal processId As Guid)
        Dim cmd As New SqlCommand("delete from BPAProcessBackup where processid=@id")
        cmd.Parameters.AddWithValue("@id", processId)
        con.Execute(cmd)
    End Sub

    ''' <summary>
    ''' Backs up a process object (assuming current user still holds the lock).
    ''' </summary>
    ''' <param name="procXml">The process definition in XML format</param>
    ''' <param name="procID">The process ID</param>
    <SecuredMethod(True)>
    Public Sub CreateProcessAutoSave(ByVal procXml As String, ByVal procID As Guid) Implements IServer.CreateProcessAutoSave
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand(
             " if exists (select 1 from BPAProcessLock" &
             "  where processid=@id and userid=@userid)" &
             " begin" &
             "  if exists (select 1 from BPAProcessBackup" &
             "   where processid=@id)" &
             "   update BPAProcessBackup" &
             "     set processxml=@xml, backupdate=getdate()" &
             "     where processid=@id and userid=@userid" &
             "  else" &
             "   insert into BPAProcessBackup" &
             "     (processid, userid, processxml, backupdate)" &
             "     values (@id, @userid, @xml, getdate())" &
             " end"
            )
            With cmd.Parameters
                .AddWithValue("@id", procID)
                .AddWithValue("@userid", GetLoggedInUserId())
                .AddWithValue("@xml", procXml)
            End With
            If con.ExecuteReturnRecordsAffected(cmd) <> 1 Then
                Throw New LockUnavailableException()
            End If
        End Using
    End Sub

#End Region

End Class
