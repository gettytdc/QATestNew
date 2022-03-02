Imports System.Data.SqlClient
Imports System.Globalization
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.clsWorkQueueItem
Imports BluePrism.AutomateAppCore.clsWorkQueuesBusinessObject
Imports BluePrism.AutomateAppCore.DataMonitor
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateAppCore.WorkQueues
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Data
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Server.Domain.Models.DataFilters
Imports BluePrism.Server.Domain.Models.Extensions
Imports BluePrism.Utilities.Functional
Imports LocaleTools
Imports SortOrder = BluePrism.AutomateAppCore.QueueSortOrder

' Partial class which separates the work queues from the rest of the clsServer
' methods just in order to keep the file size down to a sane level and make it
' easier to actually find functions
Partial Public Class clsServer

    ''' <summary>
    ''' Constant used to skip the housekeeping of the tags - ie. the removal of
    ''' unassigned tags when a tag is removed.
    ''' <seealso cref="WorkQueueItemApplyTags">Used in the ApplyTags method.
    ''' </seealso>
    ''' </summary>
    Private Const SkipTagHousekeeping As Boolean = False

    ''' <summary>
    ''' Helper method to create a wildcarded version of the given tag.
    ''' This will ensure that :-
    ''' <list>
    ''' <item>Any "%" characters are treated as literal</item>
    ''' <item>Any "_" characters are treated as literal</item>
    ''' <item>Any escaped wildcards ("**" - we don't bother escaping ?'s - how would
    ''' you specify 'any 2 characters' in a search if we did) are converted to
    ''' represent a single literal asterisk</item>
    ''' <item>Any other wildcards are converted to the database specific wildcard
    ''' character ("%" for 0..many chars, "_" for single char)</item>
    ''' </list>
    ''' </summary>
    ''' <param name="tag">The tag with optional wildcards within</param>
    ''' <returns>A database-specific version of the tag which can be used in a
    ''' LIKE clause with the expected results. Note that if no changes were made to
    ''' the argument, then the reference to that string is returned rather than a
    ''' reference to a new string with the same value.
    ''' </returns>
    Friend Shared Function ApplyWildcard(ByVal tag As String) As String
        ' We need to:
        ' - escape opening square brackets,
        ' - ensure that percent signs are escaped
        ' - same for underscore characters, then
        ' - replace any escaped asterisks with a single asterisk
        ' - replace any remaining asterisks with the db wildcard char (%)
        ' - and replace any remaining question marks with the equivalent db char (_)
        Return BPUtil.ReplaceAny(tag,
         "[", "[[]",
         "%", "[%]",
         "_", "[_]",
         "**", "*",
         "*", "%",
         "?", "_")
    End Function

    ''' <summary>
    ''' Adds the tag filter clauses to an existing query. It assumes that the
    ''' buffer already has the query along with an existing where clause in it; it
    ''' will append the tag checks with an 'and [not] exists...', and add sql
    ''' parameters to the provided command which correspond to the added query text
    ''' </summary>
    ''' <param name="sb">The buffer which contains the query created thus far and
    ''' requires the tag check SQL appended to it</param>
    ''' <param name="tagMask">The tag mask to apply to the query</param>
    ''' <param name="cmd">The SQL Command into which any new parameters should be
    ''' added.</param>
    ''' <param name="inclVirtual">True to include virtual tags, eg. the virtual
    ''' 'Exception: xxx' tag applied with an exception reason when an item is marked
    ''' as an exception</param>
    Private Sub AddTagFilter(
     ByVal sb As StringBuilder,
     ByVal tagMask As clsTagMask,
     ByVal cmd As SqlCommand,
     ByVal inclVirtual As Boolean)
        ' Ignore null masks
        If tagMask Is Nothing Then Return

        Dim paramInd As Integer = 0
        Dim template As String
        If inclVirtual Then
            template = "
and {0} (
    exists (
        select 1
        from BPAWorkQueueItemTag it
        inner join BPATag t on it.tagid = t.id
            where it.queueitemident = i.ident and t.tag {1} @tag{2}
    )
    or (i.exception is not null and i.exceptionreasontag {1} @tag{2})
) "
        Else
            template = "
and {0} exists (
    select 1
    from BPAWorkQueueItemTag it
    inner join BPATag t on it.tagid = t.id
        where it.queueitemident = i.ident and t.tag {1} @tag{2}
)"
        End If

        For Each tag As String In tagMask.OnTags
            Dim wildCardedOnTag As String = ApplyWildcard(tag)

            sb.AppendFormat(template,
                "", ' {0}
                IIf(wildCardedOnTag Is tag, "=", "like"), ' {1}
                paramInd) ' {2}

            cmd.Parameters.Add("@tag" & paramInd, SqlDbType.NVarChar, 255).Value = wildCardedOnTag


            paramInd += 1
        Next

        For Each tag As String In tagMask.OffTags
            Dim wildCardedOffTag As String = ApplyWildcard(tag)
            sb.AppendFormat(template,
                            "not", ' {0}
                            IIf(wildCardedOffTag Is tag, "=", "like"), ' {1}
                            paramInd) ' {2}

            cmd.Parameters.Add("@tag" & paramInd, SqlDbType.NVarChar, 255).Value = wildCardedOffTag

            paramInd += 1
        Next

    End Sub

    ''' <summary>
    ''' Gets the identity value of the work queue with the given name.
    ''' </summary>
    ''' <param name="con">The connection over which the work queue ident should be
    ''' retrieved.</param>
    ''' <param name="name">The name of the work queue for which the ident is required
    ''' </param>
    ''' <returns>The ident value of the specified work queue.</returns>
    ''' <exception cref="NoSuchQueueException">If no work queue with the specified
    ''' name was found on the database.</exception>
    Private Function GetWorkQueueIdent(
     ByVal con As IDatabaseConnection, ByVal name As String) As Integer
        Dim cmd As New SqlCommand("select ident from BPAWorkQueue where name = @name")
        cmd.Parameters.AddWithValue("@name", name)

        Dim ident As Integer = IfNull(con.ExecuteReturnScalar(cmd), 0)
        If ident <> 0 Then Return ident

        Throw New NoSuchQueueException(name, CultureInfo.CreateSpecificCulture(mLoggedInUserLocale))

    End Function

    ''' <summary>
    ''' Will throw an exception if the id of queue doesn't belong to a queue.
    ''' </summary>
    ''' <param name="con">Current database connection.</param>
    ''' <param name="workQueueId">The id of the work queue in question.</param>
    ''' <exception cref="NoSuchQueueException">If no work queue with the specified
    ''' id was found on the database.</exception>

    Private Shared Sub ThrowIfQueueNotExist(con As IDatabaseConnection, workQueueId As Guid)

        Dim cmd As New SqlCommand()
        cmd.Parameters.AddWithValue("@id", workQueueId)

        cmd.CommandText = "select ident from BPAWorkQueue where id = @id"
        Using reader = con.ExecuteReturnDataReader(cmd)
            If Not reader.Read() Then Throw New NoSuchQueueException(workQueueId)
        End Using

    End Sub

    ''' <summary>
    ''' Creates the given work queue over the specified connection, ensuring that
    ''' the new queue's identity is set once it has successfully completed.
    ''' If the given queue has no <see cref="clsWorkQueue.Id"/> set, this will
    ''' generate one and assign it.
    ''' </summary>
    ''' <param name="workQueue">The queue to create</param>
    ''' <param name="isCommand">Indicates whether the request to delete the work queue has come from an AutomateC command.</param>
    ''' <returns>The given work queue with its ID and identity set.</returns>
    <SecuredMethod(Permission.SystemManager.Workflow.WorkQueueConfiguration)>
    Public Function CreateWorkQueue(workQueue As clsWorkQueue, Optional isCommand As Boolean = False) As clsWorkQueue Implements IServer.CreateWorkQueue
        CheckPermissions()
        Using con = GetConnection()
            CreateWorkQueue(con, workQueue, isCommand)
            Return workQueue
        End Using
    End Function

    ''' <summary>
    ''' Creates the given work queue over the specified connection, ensuring that
    ''' the new queue's identity is set once it has successfully completed.
    ''' If the given queue has no <see cref="clsWorkQueue.Id"/> set, this will
    ''' generate one and assign it.
    ''' </summary>
    ''' <param name="con">The connection to the database to use.</param>
    ''' <param name="workQueue">The queue to create</param>
    Private Sub CreateWorkQueue(con As IDatabaseConnection, workQueue As clsWorkQueue, Optional isCommand As Boolean = False)
        If workQueue.Id = Nothing Then workQueue.Id = Guid.NewGuid()

        Dim cmd As New SqlCommand(
            "IF NOT Exists (SELECT 1 FROM BPAWorkQueue where name = @name) " &
            " begin" &
            "     insert into BPAWorkQueue " &
            "       (id,name,keyfield,running,maxattempts,encryptid," &
            "         processid,resourcegroupid) " &
            "       values (@id,@name,@keyfield,@running,@maxattempts,@encryptid," &
            "         @processid,@resourcegroupid);" &
            "     select cast(scope_identity() as int)" &
            " end " &
            " else " &
            " begin " &
            "     select null" &
            " end"
        )
        With cmd.Parameters
            .AddWithValue("@id", workQueue.Id)
            .AddWithValue("@name", workQueue.Name)
            .AddWithValue("@keyfield", workQueue.KeyField)
            .AddWithValue("@running", workQueue.IsRunning)
            .AddWithValue("@maxattempts", workQueue.MaxAttempts)
            .AddWithValue("@encryptid", IIf(workQueue.EncryptionKeyID = 0, DBNull.Value, workQueue.EncryptionKeyID))
            If workQueue.IsActive Then
                .AddWithValue("@processid", workQueue.ProcessId)
                .AddWithValue("@resourcegroupid", workQueue.ResourceGroupId)
            Else
                .AddWithValue("@processid", DBNull.Value)
                .AddWithValue("@resourcegroupid", DBNull.Value)
            End If
        End With
        workQueue.Ident = BPUtil.IfNull(con.ExecuteReturnScalar(cmd), 0)

        If isCommand Then
            AuditRecordWorkQueueEvent(WorkQueueEventCode.CreateQueue, workQueue.Id, workQueue.Name, String.Format(My.Resources.clsServer_WorkQueues_QueueCreatedUsingCreatequeueCommandLineOptionNewQueueIDIs0, workQueue.Id))
        Else
            AuditRecordWorkQueueEvent(WorkQueueEventCode.CreateQueue, workQueue.Id, workQueue.Name, String.Format(My.Resources.clsServer_WorkQueues_NewQueueNameIs0, workQueue.Name))
        End If
    End Sub

    ''' <summary>
    ''' Deletes the work queue with the specified name over the given connection
    ''' </summary>
    ''' <param name="name">The name of the work queue to delete.</param>
    ''' <param name="queueHasSnapshotConfiguration">Indicates whether the a work queue has snapshot configured.</param>
    ''' <param name="isCommand">Indicates whether the request to delete the work queue has come from an AutomateC command.</param>
    ''' <exception cref="NoSuchQueueException">If no queue with the given name could
    ''' be found on the database.</exception>
    ''' <exception cref="ForeignKeyDependencyException">If the queue is not empty and
    ''' therefore cannot be deleted.</exception>
    <SecuredMethod(Permission.SystemManager.Workflow.WorkQueueConfiguration)>
    Public Sub DeleteWorkQueue(name As String, Optional queueHasSnapshotConfiguration As Boolean = False, Optional isCommand As Boolean = False) Implements IServer.DeleteWorkQueue
        CheckPermissions()
        Using connection = GetConnection()
            connection.BeginTransaction()

            DeleteWorkQueue(connection, name, isCommand)
            If queueHasSnapshotConfiguration Then
                IncrementDataVersion(connection, DataNames.ConfiguredSnapshots)
            End If

            connection.CommitTransaction()
        End Using
    End Sub

    <SecuredMethod(Permission.SystemManager.Workflow.WorkQueueConfiguration)>
    Public Sub DeleteWorkQueue(workQueueId As Guid) Implements IServer.DeleteWorkQueue
        CheckPermissions()
        Using connection = GetConnection()
            connection.BeginTransaction()

            DeleteWorkQueue(connection, workQueueId)

            connection.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Deletes the work queue with the specified name over the given connection
    ''' </summary>
    ''' <param name="con">The connection over which the queue should be deleted.
    ''' </param>
    ''' <param name="name">The name of the work queue to delete.</param>
    ''' <exception cref="NoSuchQueueException">If no queue with the given name could
    ''' be found on the database.</exception>
    ''' <exception cref="QueueNotEmptyException">If the queue is not empty and
    ''' therefore cannot be deleted.</exception>
    ''' <exception cref="ForeignKeyDependencyException">If the queue is an active
    ''' queue and it has a session assigned to it.</exception>
    ''' <exception cref="SqlException">If any other SQL errors occur</exception>
    Private Sub DeleteWorkQueue(con As IDatabaseConnection, name As String, isCommand As Boolean)
        Dim cmd As New SqlCommand()
        cmd.Parameters.AddWithValue("@name", name)

        Dim id As Guid, ident As Integer
        cmd.CommandText = "select id, ident from BPAWorkQueue where name = @name"
        Using reader = con.ExecuteReturnDataReader(cmd)
            If Not reader.Read() Then Throw New NoSuchQueueException(name, CultureInfo.CreateSpecificCulture(mLoggedInUserLocale))
            Dim prov As New ReaderDataProvider(reader)
            id = prov.GetGuid("id")
            ident = prov.GetValue("ident", 0)
        End Using

        DeleteWorkQueue(con, id, name, ident, isCommand)
    End Sub

    Private Sub DeleteWorkQueue(con As IDatabaseConnection, workQueueId As Guid)
        Dim cmd As New SqlCommand()
        cmd.Parameters.AddWithValue("@id", workQueueId)

        Dim name As String, ident As Integer
        cmd.CommandText = "select name, ident from BPAWorkQueue where id = @id"
        Using reader = con.ExecuteReturnDataReader(cmd)
            If Not reader.Read() Then Throw New NoSuchQueueException(workQueueId)
            Dim prov As New ReaderDataProvider(reader)
            name = prov.GetString("name")
            ident = prov.GetValue("ident", 0)
        End Using

        DeleteWorkQueue(con, workQueueId, name, ident)
    End Sub

    Private Sub DeleteWorkQueue(con As IDatabaseConnection, id As Guid, name As String, ident As Integer, Optional isCommand As Boolean = False)

        Using cmd = mDatabaseCommandFactory("delete from BPAWorkQueue where name = @name")
            cmd.AddParameter("@name", name)

            ' Check if the work queue is empty
            If HasAnyItems(con, ident) Then Throw New QueueNotEmptyException(name, CultureInfo.CreateSpecificCulture(mLoggedInUserLocale))

            ' Check if there are any sessions assigned to it
            Dim sessNos = GetActiveQueueSessionNos(con, ident)
            If sessNos.Count > 0 Then Throw New ForeignKeyDependencyException(
                LTools.Format(My.Resources.clsServer_TheQueueNAMECannotBeDeletedBecauseItStillHasCOUNTPluralOne1SessionOtherSessionA, "NAME", name, "COUNT", sessNos.Count))

            con.Execute(cmd)

            If isCommand Then
                AuditRecordWorkQueueEvent(WorkQueueEventCode.DeleteQueue, Nothing, name, My.Resources.clsServer_WorkQueues_QueueDeletedUsingDeletequeueCommandLineOption)
            Else
                AuditRecordWorkQueueEvent(con, WorkQueueEventCode.DeleteQueue, id, name, String.Format(My.Resources.QueueIDWas0, id))
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Adds a work queue log entry for each of the given item IDs. This will add a
    ''' single entry in the log table for each of the item IDs, using the given
    ''' operation code. The queue identity and key value will be retrieved from the
    ''' latest attempt of that item currently on the database.
    ''' </summary>
    ''' <param name="con">The connection over which the database should be accessed
    ''' </param>
    ''' <param name="op">The type of operation which is being logged.</param>
    ''' <param name="idents">The identities of all of the items for which the
    ''' specified operation is to be logged</param>
    Private Sub WorkQueueLogAddEntries(ByVal con As IDatabaseConnection,
     ByVal op As WorkQueueOperation, ByVal idents As ICollection(Of Long))

        'Log events for MI
        LogMIQueueItemEvents(con, idents, op)

        ' If the txn is not needed, don't bother adding it
        If Not Licensing.License.TransactionModel Then Return

        Dim cmd As New SqlCommand()
        cmd.Parameters.AddWithValue("@op", op)

        UpdateMultipleIds(con, cmd, idents, "id",
         " insert into BPAWorkQueueLog" &
         "   (eventtime, queueident, queueop, itemid, keyvalue)" &
         " select getutcdate(), i.queueident, @op, i.id, i.keyvalue" &
         "   from BPAWorkQueueItem i" &
         "   where i.ident in ("
        )

    End Sub

    ''' <summary>
    ''' Adds a work queue log entry for each of the given item IDs. This will add a
    ''' single entry in the log table for each of the item IDs, using the given
    ''' operation code. The queue identity and key value will be retrieved from the
    ''' latest attempt of that item currently on the database.
    ''' </summary>
    ''' <param name="con">The connection over which the database should be accessed
    ''' </param>
    ''' <param name="op">The type of operation which is being logged.</param>
    ''' <param name="itemids">The IDs of all of the items for which the specified
    ''' operation is to be logged</param>
    Private Sub WorkQueueLogAddEntries(ByVal con As IDatabaseConnection,
     ByVal op As WorkQueueOperation, ByVal itemids As ICollection(Of Guid))

        'Log events for MI
        LogMIQueueItemEvents(con, itemids, op)

        ' If the txn is not needed, don't bother adding it
        If Not Licensing.License.TransactionModel Then Return

        Dim cmd As New SqlCommand()
        cmd.Parameters.AddWithValue("@op", op)

        UpdateMultipleIds(con, cmd, itemids, "id",
         " insert into BPAWorkQueueLog" &
         "   (eventtime, queueident, queueop, itemid, keyvalue)" &
         " select getutcdate(), i.queueident, @op, i.id, i.keyvalue" &
         "   from BPAWorkQueueItem i" &
         "     left join BPAWorkQueueItem inext on i.id = inext.id and inext.attempt = i.attempt + 1" &
         "   where inext.id is null and i.id in ("
        )

    End Sub

    ''' <summary>
    ''' Adds a work queue log entry for the transactional reporting on the work
    ''' queues, getting the queue identity and key value from the latest attempt of
    ''' the specified item found in the queue.
    ''' </summary>
    ''' <param name="con">The connection over which the database should be accessed
    ''' </param>
    ''' <param name="op">The type of operation which is being logged.</param>
    ''' <param name="itemid">The ID of the item which has been operated on,
    ''' Guid.Empty if not appropriate.</param>
    Private Sub WorkQueueLogAddEntry(ByVal con As IDatabaseConnection,
     ByVal op As WorkQueueOperation, ByVal itemid As Guid)
        WorkQueueLogAddEntries(con, op, New Guid() {itemid})
    End Sub


    ''' <summary>
    ''' Adds a work queue log entry for the transactional reporting on the work
    ''' queues, getting the queue identity and key value from the item found in the
    ''' queue.
    ''' </summary>
    ''' <param name="con">The connection over which the database should be accessed
    ''' </param>
    ''' <param name="op">The type of operation which is being logged.</param>
    ''' <param name="itemident">The identity of the item which has been operated on,
    ''' 0 if not appropriate.</param>
    Private Sub WorkQueueLogAddEntry(ByVal con As IDatabaseConnection,
     ByVal op As WorkQueueOperation, ByVal itemident As Long)
        WorkQueueLogAddEntries(con, op, New Long() {itemident})
    End Sub

    ''' <summary>
    ''' Counts the work queue log entries which fell within the given dates
    ''' (inclusive), and which were written for the given operation types.
    ''' </summary>
    ''' <param name="startDate">The inclusive start date/time from which the log
    ''' entries should be counted.</param>
    ''' <param name="endDate">The inclusive end date/time to which the log entries
    ''' should be counted</param>
    ''' <param name="ops">The operations which should be counted and returned.
    ''' </param>
    ''' <returns>A map containing the count of log records against their respective
    ''' operation type. The transaction count held against the key
    ''' <see cref="WorkQueueOperation.None"/> represents a total of all accumulated
    ''' counts in the query.</returns>
    <SecuredMethod()>
    Public Function WorkQueueLogCountEntries(
     ByVal queueNames As ICollection(Of String),
     ByVal startDate As Date,
     ByVal endDate As Date,
     ByVal ops As ICollection(Of WorkQueueOperation)) _
     As IDictionary(Of WorkQueueOperation, Integer) Implements IServer.WorkQueueLogCountEntries

        CheckPermissions()

        If queueNames Is Nothing OrElse queueNames.Count = 0 Then
            Throw New ArgumentException(
             My.Resources.clsServer_AtLeastOneQueueMustBeProvidedInWhichToCountWorkQueueLogEntries,
NameOf(queueNames))
        End If

        Dim generatorMap As New clsGeneratorDictionary(Of WorkQueueOperation, Integer)
        If ops Is Nothing OrElse ops.Count = 0 Then Return generatorMap

        Using con = GetConnection()

            Dim cmd As New SqlCommand()

            Dim queueIdents As New List(Of Integer)
            Dim unknownQueues As New clsSet(Of String)

            cmd.CommandText = "select ident from BPAWorkQueue where name=@name"
            cmd.Parameters.Add("name", SqlDbType.NVarChar)
            For Each qName As String In queueNames
                cmd.Parameters("name").Value = qName
                Dim retValue As Object = con.ExecuteReturnScalar(cmd)
                If DBNull.Value.Equals(retValue) OrElse retValue Is Nothing Then
                    unknownQueues.Add(qName)
                Else
                    queueIdents.Add(DirectCast(retValue, Integer))
                End If
            Next

            If unknownQueues.Count > 0 Then
                Throw New ArgumentException(
                 My.Resources.clsServer_InvalidQueueNamesFound & unknownQueues.ToString(), NameOf(queueNames))
            End If

            With cmd.Parameters
                .Clear()
                .AddWithValue("@startDate", startDate)
                .AddWithValue("@endDate", endDate)
            End With

            Dim sb As New StringBuilder(
             " select " &
             "   l.queueop, " &
             "   count(*) as txncount" &
             " from BPAWorkQueueLog l" &
             "   join  BPAWorkQueue q on l.queueident = q.ident" &
             " where l.eventtime between @startDate and @endDate" &
             "   and l.queueop in (")

            Dim i As Integer = 0
            For Each op As WorkQueueOperation In ops
                i += 1
                sb.AppendFormat("@op{0},", i)
                cmd.Parameters.AddWithValue("@op" & i, op)
            Next
            sb.Length -= 1
            sb.Append(
             "   )" &
             "   and q.ident in ("
            )
            i = 0
            For Each ident As Integer In queueIdents
                i += 1
                sb.AppendFormat("@ident{0},", i)
                cmd.Parameters.AddWithValue("@ident" & i, ident)
            Next
            sb.Length -= 1
            sb.Append(
             "   )" &
             " group by l.queueop"
            )

            cmd.CommandText = sb.ToString()

            Dim total As Integer = 0
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As IDataProvider = New ReaderDataProvider(reader)
                While reader.Read()
                    Dim count As Integer = prov.GetValue("txncount", 0)
                    total += count
                    generatorMap(prov.GetValue("queueop", WorkQueueOperation.None)) = count
                End While
            End Using
            ' Set the total count into the 'none' operation.
            generatorMap(WorkQueueOperation.None) = total

            Return generatorMap

        End Using

    End Function

    ''' <summary>
    ''' Truncate work queue logging table
    ''' </summary>
    ''' <param name="con">The connection over which the database should be accessed
    ''' </param>
    Private Sub WorkQueueLogTruncate(ByVal con As IDatabaseConnection)
        ' Check if there's any data in the BPAWorkQueueLog table.
        ' If there is, check if the current user has ALTER permission on that table.
        ' If they do, truncate it (it's quicker), otherwise delete it (slower but
        ' doesn't require ALTER permissions to do).
        Dim cmd As New SqlCommand(
         " if exists (select 1 from BPAWorkQueueLog)" &
         " begin" &
         "     if has_perms_by_name(quotename(schema_name()) + '.' + " &
         "               quotename('BPAWorkQueueLog'), 'OBJECT', 'ALTER') = 1" &
         "         truncate table BPAWorkQueueLog;" &
         "     else" &
         "         delete from BPAWorkQueueLog;" &
         " end"
        )
        con.Execute(cmd)
    End Sub

    ''' <summary>
    ''' Copies a work item into a work queue, optionally modifying some of its
    ''' metadata in the transition
    ''' </summary>
    ''' <param name="itemId">The identifier of the item </param>
    ''' <param name="queueName">The name of the queue to copy the item to</param>
    ''' <param name="sessId">The session ID within which this method was invoked,
    ''' or <see cref="Guid.Empty"/> if it occurred outside a session</param>
    ''' <param name="defer">The date to defer the new item to, or
    ''' <see cref="DateTime.MinValue"/> to leave the item undeferred</param>
    ''' <param name="priority">The priority of the new item, or -1 to use the
    ''' priority from the item being copied</param>
    ''' <param name="tagMask">The mask to apply to the tags copied from the original
    ''' item before creating a new item. Note that this will only affect the tags
    ''' set on the new item - it does not modify the tags applied to the original
    ''' work item.</param>
    ''' <param name="status">The status of the new item, or null/empty string to
    ''' use the status from the work item being copied.</param>
    ''' <returns>The ID of the newly created work item in the target queue.</returns>
    ''' <exception cref="NoSuchWorkItemException">If no work item could be found with
    ''' the given <paramref name="itemId"/></exception>
    ''' <exception cref="NoSuchQueueException">If no work queue could be found with
    ''' the given <paramref name="queueName"/></exception>
    ''' <exception cref="OperationFailedException">If the new ID was not available
    ''' from the adding of the new work item</exception>
    ''' <exception cref="FieldLengthException">If the key value found in the given
    ''' data was longer than that accepted by the database.</exception>
    ''' <exception cref="Exception">If any other errors occur while copying the
    ''' specified work item to a queue.</exception>
    <SecuredMethod()>
    Public Function CopyWorkItem(
     itemId As Guid, queueName As String, sessId As Guid, defer As Date,
     priority As Integer, tagMask As String, status As String) As Guid Implements IServer.CopyWorkItem
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            Dim newId As Guid = CopyWorkItem(
                con, itemId, queueName, sessId, defer, priority, tagMask, status)
            con.CommitTransaction()
            Return newId
        End Using
    End Function

    ''' <summary>
    ''' Copies a work item into a work queue, optionally modifying some of its
    ''' metadata in the transition
    ''' </summary>
    ''' <param name="con">The connection to the database</param>
    ''' <param name="itemId">The identifier of the item </param>
    ''' <param name="queueName">The name of the queue to copy the item to</param>
    ''' <param name="sessId">The session ID within which this method was invoked,
    ''' or <see cref="Guid.Empty"/> if it occurred outside a session</param>
    ''' <param name="defer">The date to defer the new item to, or
    ''' <see cref="DateTime.MinValue"/> to leave the item undeferred</param>
    ''' <param name="priority">The priority of the new item, or -1 to use the
    ''' priority from the item being copied</param>
    ''' <param name="tagMask">The mask to apply to the tags copied from the original
    ''' item before creating a new item. Note that this will only affect the tags
    ''' set on the new item - it does not modify the tags applied to the original
    ''' work item.</param>
    ''' <param name="status">The status of the new item, or null/empty string to
    ''' use the status from the work item being copied.</param>
    ''' <returns>The ID of the newly created work item in the target queue.</returns>
    ''' <exception cref="NoSuchWorkItemException">If no work item could be found with
    ''' the given <paramref name="itemId"/></exception>
    ''' <exception cref="NoSuchQueueException">If no work queue could be found with
    ''' the given <paramref name="queueName"/></exception>
    ''' <exception cref="OperationFailedException">If the new ID was not available
    ''' from the adding of the new work item</exception>
    ''' <exception cref="FieldLengthException">If the key value found in the given
    ''' data was longer than that accepted by the database.</exception>
    ''' <exception cref="Exception">If any other errors occur while copying the
    ''' specified work item to a queue.</exception>
    Private Function CopyWorkItem(con As IDatabaseConnection,
     itemId As Guid, queueName As String, sessId As Guid, defer As Date,
     priority As Integer, tagMask As String, status As String) As Guid

        Dim item As clsWorkQueueItem = WorkQueueGetItem(con, itemId)
        If item Is Nothing Then Throw New NoSuchWorkItemException(itemId)

        ' Use the provided values if they are there, otherwise fall back to the
        ' values in the item that we're copying
        Dim tagString As String
        With New clsTagMask(item.TagString)
            .ApplyTags(tagMask)
            tagString = .ToString()
        End With
        If status Is Nothing Then status = item.Status
        If priority = -1 Then priority = item.Priority

        ' First we add the item to the target queue
        Dim ids As ICollection(Of Guid) = WorkQueueAddItems(
         con, queueName, item.Data.Rows, sessId, defer, priority, tagString, status)
        Dim newId = ids.FirstOrDefault()
        If newId = Guid.Empty Then Throw New OperationFailedException(
         My.Resources.clsServer_FailedToAddItemTo0NoErrorGiven, queueName)

        ' Then we mark the existing item as complete
        WorkQueueMarkComplete(con, sessId, itemId)

        Return newId

    End Function

    ''' <summary>
    ''' Add one or more items to the given work queue.
    ''' Each work queue item is inserted into the database in it's own transaction.
    ''' </summary>
    ''' <param name="queuename">The name of the queue to add to</param>
    ''' <param name="data">A clsCollection with a row for each item to be added to
    ''' the queue.</param>
    ''' <param name="sessionId">The ID of the session which is adding this item to
    ''' the queue.</param>
    ''' <param name="defer">A Date to defer processing until, or Date.MinValue to
    ''' make the items available immediately.</param>
    ''' <param name="tags">A semi-colon separated set of tags which should be
    ''' applied to the items being added. A "+" prefix is ignored, any tags with
    ''' "-" prefixes are also ignored (you can't remove tags from an item which
    ''' has none)</param>
    ''' <param name="priority">The priority for the items being added. Items with
    ''' lower (numerically) priority values are retrieved from the queue ahead of
    ''' higher ones.</param>
    ''' <param name="status">The initial status required for the added items.</param>
    ''' <returns>Returns The collection of IDs which were created as a result of this
    ''' method invocation.</returns>
    ''' <exception cref="FieldLengthException">If the key value found in the given
    ''' data was longer than that accepted by the database. If this is thrown, then
    ''' no data will have been added to the database.</exception>
    ''' <exception cref="NoSuchQueueException">If no queue was found in the system
    ''' with the given name.</exception>
    ''' <exception cref="ArgumentException">If there is no data in the given
    ''' collection</exception>
    ''' <exception cref="Exception">If any other errors occur while adding the
    ''' specified work queue items to the database.</exception>
    <SecuredMethod()>
    Public Function WorkQueueAddItems(
     queuename As String,
     data As IEnumerable(Of clsCollectionRow),
     sessionId As Guid,
     defer As Date,
     priority As Integer,
     tags As String,
     status As String) As ICollection(Of Guid) Implements IServer.WorkQueueAddItems
        CheckPermissions()
        If Not String.IsNullOrEmpty(status) AndAlso status.Length > 255 Then Throw New FieldLengthException(
            String.Format(My.Resources.Exception_The0ValueExceedsTheMaximumLengthOf1CharactersValue2, "Status", MaxLengths.Status, status))
        Using con = GetConnection()
            Return WorkQueueAddItems(
                con, queuename, data, sessionId, defer, priority, tags, status)
        End Using
    End Function

    ''' <summary>
    ''' Add one or more items to the given work queue from the BP API.
    ''' Each work queue item request is inserted into the database in one transaction.
    ''' If all work queue item requests succeed the db transaction will be committed.
    ''' If any work queue item request fails the db transaction will be rolled back.
    ''' </summary>
    ''' <param name="queuename">The name of the queue to add to</param>
    ''' <param name="workQueueItems">A list of work queue item requests.</param>
    ''' <returns>Returns The collection of IDs which were created as a result of this
    ''' method invocation.</returns>
    ''' <exception cref="FieldLengthException">If the key value found in the given
    ''' data was longer than that accepted by the database. If this is thrown, then
    ''' no data will have been added to the database.</exception>
    ''' <exception cref="NoSuchQueueException">If no queue was found in the system
    ''' with the given name.</exception>
    ''' <exception cref="ArgumentException">If there is no data in the given
    ''' collection</exception>
    ''' <exception cref="Exception">If any other errors occur while adding the
    ''' specified work queue items to the database.</exception>
    <SecuredMethod()>
    Public Function WorkQueueAddItemsAPI(queueName As String,
                                         workQueueItems As IEnumerable(Of CreateWorkQueueItemRequest)) As IEnumerable(Of Guid) Implements IServer.WorkQueueAddItemsAPI
        CheckPermissions()
        Using con = GetConnection()
            Dim results = New List(Of Guid)
            Try
                Dim workQueueDetails = GetWorkQueueDetails(con, queueName)

                con.BeginTransaction()

                For Each workQueueItem In workQueueItems
                    Dim insertResults = WorkQueueAddItems(con, queueName, workQueueItem.Data.Rows, Nothing, workQueueItem.Defer, workQueueItem.Priority, workQueueItem.Tags, workQueueItem.Status, workQueueDetails)
                    results.AddRange(insertResults)
                Next

                AddWorkQueueItemsAddedAuditEvents(con, queueName, workQueueDetails, results)

                con.CommitTransaction()
                Return results
            Catch ex As Exception
                If (con IsNot Nothing AndAlso con.InTransaction) Then
                    con.RollbackTransaction()
                End If
                Throw
            End Try
        End Using
    End Function

    ''' <summary>
    ''' Adds audit events for the work queue items added via API
    ''' Work queue items are batched into 20 for audit event inserts
    ''' given item ID.
    ''' </summary>
    ''' <param name="con">The Sql connection object.</param>
    ''' <param name="queueName">The work queue items added to.</param>
    ''' <param name="workQueueDetails">The work queue details.</param>
    ''' <param name="createdWorkQueueItemIds">The work queue item ids created.</param>
    Private Sub AddWorkQueueItemsAddedAuditEvents(con As IDatabaseConnection, queueName As String, workQueueDetails As AddWorkQueueItemWorkQueueDetails, createdWorkQueueItemIds As List(Of Guid))
        Dim batchedResults = createdWorkQueueItemIds.Batch(20)
        For Each batchResult In batchedResults
            Dim createdWorkQueueIds = String.Join(", ", batchResult)
            Dim comment = String.Format(My.Resources.clsServer_WorkQueueItemsAddedAPIComment, batchResult.Count, workQueueDetails.QueueId, createdWorkQueueIds)
            AuditRecordWorkQueueEvent(con, WorkQueueEventCode.WorkQueueItemsAddedAPI, workQueueDetails.QueueId, queueName, comment)
        Next
    End Sub

    ''' <summary>
    ''' Gets the work queue details used in add work queue items method
    ''' given item ID.
    ''' </summary>
    ''' <param name="con">The Sql connection object.</param>
    ''' <param name="queueName">The work queue name getting data for.</param>
    ''' <returns>Returns the work queue details object</returns>
    Private Function GetWorkQueueDetails(con As IDatabaseConnection, queueName As String) As AddWorkQueueItemWorkQueueDetails

        Dim cmd As New SqlCommand("select id,ident,keyfield,encryptid from BPAWorkQueue where name=@name")
        cmd.Parameters.AddWithValue("@name", queueName)

        Dim workQueueDetails = New AddWorkQueueItemWorkQueueDetails
        Using reader = con.ExecuteReturnDataReader(cmd)
            If Not reader.Read() Then Throw New NoSuchQueueException(queueName, CultureInfo.CreateSpecificCulture(mLoggedInUserLocale))
            Dim prov As New ReaderDataProvider(reader)

            workQueueDetails.QueueId = prov.GetValue("id", Guid.Empty)
            workQueueDetails.QueueIdent = prov.GetValue("ident", 0)
            workQueueDetails.Keyfield = prov.GetString("keyfield")
            workQueueDetails.EncryptID = prov.GetValue("encryptid", 0)

        End Using
        Return workQueueDetails
    End Function

    ''' <summary>
    ''' Add one or more items to the given work queue.
    ''' Each work queue item is inserted into the database in it's own transaction.
    ''' </summary>
    ''' <param name="queuename">The name of the queue to add to</param>
    ''' <param name="data">A clsCollection with a row for each item to be added to
    ''' the queue.</param>
    ''' <param name="sessionId">The ID of the session which is adding this item to
    ''' the queue.</param>
    ''' <param name="defer">A Date to defer processing until, or Date.MinValue to
    ''' make the items available immediately.</param>
    ''' <param name="tags">A semi-colon separated set of tags which should be
    ''' applied to the items being added. A "+" prefix is ignored, any tags with
    ''' "-" prefixes are also ignored (you can't remove tags from an item which
    ''' has none)</param>
    ''' <param name="priority">The priority for the items being added. Items with
    ''' lower (numerically) priority values are retrieved from the queue ahead of
    ''' higher ones.</param>
    ''' <param name="status">The initial status required for the added items.</param>
    ''' <param name="workQueueDetails">An optional parameter to pass in the work queue
    ''' details instead of loading for each request. Utilised by the new API method.</param>
    ''' <returns>Returns The collection of IDs which were created as a result of this
    ''' method invocation.</returns>
    ''' <exception cref="FieldLengthException">If the key value found in the given
    ''' data was longer than that accepted by the database. If this is thrown, then
    ''' no data will have been added to the database.</exception>
    ''' <exception cref="NoSuchQueueException">If no queue was found in the system
    ''' with the given name.</exception>
    ''' <exception cref="ArgumentException">If there is no data in the given
    ''' collection</exception>
    ''' <exception cref="Exception">If any other errors occur while adding the
    ''' specified work queue items to the database.</exception>
    Private Function WorkQueueAddItems(
     con As IDatabaseConnection,
     queuename As String,
     data As IEnumerable(Of clsCollectionRow),
     sessionId As Guid,
     defer As Date,
     priority As Integer,
     tags As String,
     status As String,
     Optional workQueueDetails As AddWorkQueueItemWorkQueueDetails = Nothing) As ICollection(Of Guid)
        If data.Count = 0 Then _
         Throw New ArgumentException(My.Resources.clsServer_NoDataFoundForNewWorkQueueItems)

        Dim tagger As clsTagMask = Nothing
        If tags <> "" Then tagger = New clsTagMask(tags, True)

        ' If it still ends up empty, nullify it so that there's only one test to
        ' see if there are tags to set on these items
        If tagger IsNot Nothing AndAlso tagger.OnTags.Count = 0 Then _
         tagger = Nothing

        Dim queueid As Guid
        Dim queueident As Integer
        Dim keyfield As String
        Dim encryptID As Integer

        If workQueueDetails Is Nothing Then
            workQueueDetails = GetWorkQueueDetails(con, queuename)
        End If
        queueid = workQueueDetails.QueueId
        queueident = workQueueDetails.QueueIdent
        keyfield = workQueueDetails.Keyfield
        encryptID = workQueueDetails.EncryptID

        Dim cmd As New SqlCommand()

        ' (Partially) Populate the collection into queue item objects - this
        ' allows the object to validate its fields - primarily the key value
        ' field. It also guarantees that no data is inserted into the database
        ' if any of the data fails validation.
        Dim items As New List(Of clsWorkQueueItem)
        For Each row As clsCollectionRow In data
            Dim qi As New clsWorkQueueItem(Guid.NewGuid())
            If keyfield <> "" Then
                Dim pv As clsProcessValue = Nothing
                If row.TryGetValue(keyfield, pv) Then qi.KeyValue = pv.EncodedValue
            End If
            qi.DataRow = row
            items.Add(qi)
        Next

        ' We need the IDs for all the tags we need to apply
        Dim tagMap As New Dictionary(Of String, Integer)

        If tagger IsNot Nothing Then

            ' Keep track of the tags not held on the system so we can add them
            Dim missing As New List(Of String)

            ' Set up the command to reap the tag IDs
            cmd.Parameters.Clear()
            Dim param As SqlParameter =
             cmd.Parameters.Add("@tag", SqlDbType.NVarChar)
            cmd.CommandText =
             "select id from BPATag where tag = @tag"

            ' Go through each tag we want to add and get its ID; if it's not
            ' there, save it into the missing list
            For Each tag As String In tagger.OnTags
                param.Value = tag
                Dim id As Integer = IfNull(con.ExecuteReturnScalar(cmd), 0)
                If id = 0 Then missing.Add(tag) Else tagMap(tag) = id
            Next


            ' If there are any missing, add them and get their IDs
            If missing.Count > 0 Then
                cmd.CommandText =
                    "MERGE BPATag  WITH(HOLDLOCK) AS target " &
                    "USING (SELECT @tag tag ) AS source " &
                    "   ON source.tag = target.tag " &
                    "WHEN NOT MATCHED THEN " &
                    "   INSERT (tag) VALUES (@tag) " &
                    "WHEN MATCHED THEN " &
                    "   UPDATE SET tag = @tag " &
                    "OUTPUT inserted.id;"
                ' The param @tag from above will work in this command too
                For Each tag As String In missing
                    If tag.Length > 255 Then
                        Throw New InvalidOperationException(My.Resources.clsServerWorkQueues_TagTooLong)
                    End If
                    param.Value = tag
                    tagMap(tag) = CInt(con.ExecuteReturnScalar(cmd))
                Next
            End If

        End If

        ' We're starting from scratch here, so dump the current params
        cmd.Parameters.Clear()

        ' So we build up the query to add the items - we add each one
        ' individually so we can retrieve the item identity - and each one gets
        ' its own transaction
        Dim sb As New StringBuilder(255)
        sb.Append(
        " declare @ident bigint" &
        " insert into BPAWorkQueueItem (" &
        "   id," &
        "   queueid," &
        "   queueident," &
        "   keyvalue," &
        "   status," &
        "   sessionid," &
        "   attempt," &
        "   loaded," &
        "   encryptid," &
        "   data," &
        "   deferred," &
        "   priority" &
        " ) values (" &
        "   @id," &
        "   @queueid," &
        "   @queueident," &
        "   @keyvalue," &
        "   @status," &
        "   @sessionid," &
        "   1," &
        "   GETUTCDATE()," &
        "   @encryptid," &
        "   @data," &
        "   @deferred," &
        "   @priority" &
        " )" & _
 _
        " set @ident=scope_identity()"
        )

        ' We can safely use the same query for all items since they all must
        ' have each of the specified tags assigned
        If tagMap.Count > 0 Then
            ' Ultimately we want a :
            '   insert into BPAWorkQueueItemTag(queueitemident,tagid)
            '     select @ident, @tagid1
            '      union all
            '     select @ident, @tagid2
            '      union all
            '     select @ident, @tagid3
            ' type command to add all the tags in one go
            sb.Append(
             " insert into BPAWorkQueueItemTag (queueitemident,tagid) ")

            ' These IDs go in literally because a) it's safe - they are numbers
            ' and b) it's a lot simpler than messing about with parameters.
            Dim prefix As String = ""
            For Each tagId As Integer In tagMap.Values
                sb.AppendFormat(" {0} select @ident, {1} ", prefix, tagId)
                prefix = "union all"
            Next

        End If
        ' Finally we want to return the identity of the work item so we can
        ' update the object itself
        sb.Append(" select @ident")

        cmd.CommandText = sb.ToString()

        For Each item As clsWorkQueueItem In items

            cmd.Parameters.Clear()
            With cmd.Parameters
                .AddWithValue("@id", item.ID)
                .AddWithValue("@queueid", queueid)
                .AddWithValue("@queueident", queueident)
                .AddWithValue("@priority", priority)
                .AddWithValue("@keyvalue", item.KeyValue)
                .AddWithValue("@sessionid",
                 IIf(sessionId = Nothing, DBNull.Value, sessionId))
                .AddWithValue("@status", IIf(status Is Nothing, "", status))
                .AddWithValue("@encryptid", IIf(encryptID = 0, DBNull.Value, encryptID))
                .AddWithValue("@data", Encrypt(encryptID, item.DataXml))
                .AddWithValue("@deferred",
                 IIf(defer = Date.MinValue, DBNull.Value, defer))
            End With
            ' The scalar returned is the queueitemident field.
            item.Ident = CLng(con.ExecuteReturnScalar(cmd))

            ' Log work queue item operation (if required)
            WorkQueueLogAddEntry(con, WorkQueueOperation.ItemCreated, item.Ident)

        Next

        ' Now pick out our item IDs so we can return them.
        Dim itemIds As New List(Of Guid)
        For Each item As clsWorkQueueItem In items
            itemIds.Add(item.ID)
        Next

        Return itemIds

    End Function

    ''' <summary>
    ''' Sets the data in the given collection into the work item specified by the
    ''' given item ID.
    ''' </summary>
    ''' <param name="id">The ID of the item to set the data on.</param>
    ''' <param name="data">The data to set on the item. This should contain one row
    ''' exactly, and, if the queue has a key value and the item has a key value, then
    ''' the data should have the same key value in it.</param>
    ''' <exception cref="KeyValueDifferenceException">If the key value in the given
    ''' data doesn't match the key value held on the item.</exception>
    <SecuredMethod()>
    Public Sub WorkQueueItemSetData(ByVal id As Guid, ByVal data As clsCollection) Implements IServer.WorkQueueItemSetData
        CheckPermissions()
        Using con = GetConnection()
            WorkQueueItemSetData(con, id, data)
        End Using
    End Sub

    ''' <summary>
    ''' Sets the data in the given collection into the work item specified by the
    ''' given item ID.
    ''' </summary>
    ''' <param name="id">The ID of the item to set the data on.</param>
    ''' <param name="data">The data to set on the item. This should contain one row
    ''' exactly, and, if the queue has a key value and the item has a key value, then
    ''' the data should have the same key value in it.</param>
    ''' <exception cref="KeyValueDifferenceException">If the key value in the given
    ''' data doesn't match the key value held on the item.</exception>
    Private Sub WorkQueueItemSetData(ByVal con As IDatabaseConnection,
     ByVal id As Guid, ByVal data As clsCollection)

        ' Some sanity checks before we go forward.
        If id = Nothing Then Throw New ArgumentNullException(NameOf(id))
        If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
        If data.Count <> 1 Then _
         Throw New BluePrismException(My.Resources.clsServer_InvalidNumberOfRowsInData0, data.Count)

        ' Set up the command with the item ID param - leave other data until we need it.
        Dim cmd As New SqlCommand()
        cmd.Parameters.AddWithValue("@id", id)

        ' First we want to check the key value.. for this we need the queue that the
        ' item belongs to
        Dim keyField As String
        Dim keyValue As String
        Dim ident As Long
        Dim encryptID As Integer

        ' We can group the fields to get the latest identity field for the item at the
        ' same time, and the encrypter name to use for the data
        cmd.CommandText =
         " select q.keyfield, q.encryptid, i.keyvalue, max(i.ident) as ident" &
         " from BPAWorkQueueItem i" &
         "   join BPAWorkQueue q on i.queueident = q.ident" &
         " where i.id = @id" &
         " group by q.keyfield, q.encryptid, i.keyvalue"

        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            If Not reader.Read() Then Throw New NoSuchWorkItemException(id)
            keyField = prov.GetString("keyfield")
            keyValue = prov.GetString("keyvalue")
            ident = prov.GetValue("ident", 0L)
            encryptID = prov.GetValue("encryptid", 0)
        End Using

        ' Check that the key value in the collection matches the key value on the item,
        ' if a) there is one on the item and b) the collection contains a key field
        If keyValue <> "" AndAlso keyField <> "" Then
            Dim row As clsCollectionRow = data.Row(0)
            Dim val As clsProcessValue = Nothing
            If row.TryGetValue(keyField, val) Then
                If val.EncodedValue <> keyValue Then Throw New KeyValueDifferenceException(
                 My.Resources.clsServer_TheProvidedDataHasADifferentKeyValueToTheWorkItemTheKeyField0ItemValue1DataValu,
                 keyField, keyValue, val.EncodedValue)
            End If
        End If

        ' OK, we're definitely going ahead now... might as well set the parameters
        With cmd.Parameters
            .Clear() ' We don't need the item ID any more.
            .AddWithValue("@encryptid", IIf(encryptID = 0, DBNull.Value, encryptID))
            .AddWithValue("@data", Encrypt(encryptID, data.GenerateXML()))
            .AddWithValue("@ident", ident)
        End With

        cmd.CommandText =
         " update BPAWorkQueueItem set " &
         "   encryptid = @encryptid, data = @data" &
         " where ident = @ident"

        con.Execute(cmd)

    End Sub

    ''' <summary>
    ''' Sets the priority of a work queue item to a specified priority
    ''' </summary>
    ''' <param name="itemId">The ID of an item whose priority should be set.</param>
    ''' <param name="priority">The priority to set in the item</param>
    <SecuredMethod()>
    Public Sub WorkQueueSetPriority(ByVal itemId As Guid, ByVal priority As Integer) Implements IServer.WorkQueueSetPriority
        CheckPermissions()
        Using con = GetConnection()
            WorkQueueSetPriority(con, itemId, priority)
        End Using
    End Sub

    ''' <summary>
    ''' Sets the priority of a work queue item to a specified priority
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="itemId">The ID of an item whose priority should be set.</param>
    ''' <param name="priority">The priority to set in the item</param>
    Private Sub WorkQueueSetPriority(ByVal con As IDatabaseConnection,
     ByVal itemId As Guid, ByVal priority As Integer)
        Dim cmd As New SqlCommand(
         " update i" &
         " set i.priority = @priority" &
         " from BPAWorkQueueItem i" &
         "   left join BPAWorkQueueItem inext on i.id = inext.id " &
         "     And inext.attempt = i.attempt + 1" &
         " where inext.id Is null And i.id = @id"
        )

        With cmd.Parameters
            .AddWithValue("@id", itemId)
            .AddWithValue("@priority", priority)
        End With

        If con.ExecuteReturnRecordsAffected(cmd) = 0 Then _
         Throw New NoSuchElementException(My.Resources.clsServer_NoQueueItemFoundWithId0, itemId)

    End Sub

    ''' <summary>
    ''' Gets the next work queue item, using the given filters
    ''' </summary>
    ''' <param name="sessionId">The session ID to set in the item, if appropriate.
    ''' Guid.Empty if it should be set to NULL.</param>
    ''' <param name="queuename">The name of the queue for which the next item is
    ''' required.</param>
    ''' <param name="keyfilter">If present, limits the selection to one with the
    ''' specified key. Otherwise, it doesn't limit the selection by key.</param>
    ''' <param name="tagMask">If present, limits the selection to items with the
    ''' specified tag mask, in the form "+wanted tag; -unwanted; +also wanted"
    ''' where "+" denotes tags which must be present, and "-" denotes tags which
    ''' must not be present. If not present, it doesn't limit the selection by tag
    ''' </param>
    ''' <returns>The work queue item which matches the given constraints, or null if
    ''' there was no pending work queue item matching those constraints.</returns>
    ''' <exception cref="Exception">If any errors occur (database or otherwise) when
    ''' attempting to get the next item</exception>
    <SecuredMethod()>
    Public Function WorkQueueGetNext(
     ByVal sessionId As Guid,
     ByVal queuename As String,
     ByVal keyfilter As String,
     ByVal tagMask As clsTagMask) As clsWorkQueueItem Implements IServer.WorkQueueGetNext
        CheckPermissions()
        Using con = GetConnection()
            GetWorkQueueIdent(con, queuename)
            Return WorkQueueGetNext(con, sessionId, queuename, keyfilter, tagMask)
        End Using
    End Function

    <SecuredMethod()>
    Public Function WorkQueueGetById(queueName As String, sessionid As Guid, workQueueItemId As Guid) As clsWorkQueueItem Implements IServer.WorkQueueGetById
        CheckPermissions()
        Using con = GetConnection()
            Using cmd As New SqlCommand("usp_getitembyid")
                cmd.CommandType = CommandType.StoredProcedure
                With cmd.Parameters
                    .AddWithValue("@queueName", queueName)
                    .AddWithValue("@sessionId", sessionid)
                    .AddWithValue("@workQueueItemId", workQueueItemId)
                End With

                Dim item As clsWorkQueueItem = Nothing

                Try
                    Using reader = con.ExecuteReturnDataReader(cmd)
                        If Not reader.Read() Then Throw New NoSuchQueueItemException(workQueueItemId)
                        Dim prov As New ReaderDataProvider(reader)
                        item = New clsWorkQueueItem(prov)
                        item.DataXml = Decrypt(prov.GetInt("encryptid"), prov.GetString("data"))
                    End Using

                Catch sqlex As SqlException
                    If sqlex.Number = DatabaseErrorCode.WorkQueueItemNotFound Then
                        Throw New NoSuchQueueItemException(workQueueItemId)
                    ElseIf sqlex.Number = DatabaseErrorCode.WorkQueueItemLocked Then
                        Throw New LockedQueueItemException(workQueueItemId)
                    ElseIf sqlex.Number = DatabaseErrorCode.WorkQueueItemDeferred Then
                        Throw New DeferredQueueItemException(workQueueItemId)
                    ElseIf sqlex.Number = DatabaseErrorCode.WorkQueueItemNotActive Then
                        Throw New NotActiveQueueItemException(workQueueItemId)
                    End If
                End Try

                Return item

            End Using
        End Using
    End Function

    ''' <summary>
    ''' Gets the next work queue item, using the given filters
    ''' </summary>
    ''' <param name="con">The connection over which the item should be retrieved.
    ''' </param>
    ''' <param name="sessId">The session ID to set in the item, if appropriate.
    ''' Guid.Empty if it should be set to NULL.</param>
    ''' <param name="queuename">The name of the queue for which the next item is
    ''' required.</param>
    ''' <param name="keyfilter">If present, limits the selection to one with the
    ''' specified key. Otherwise, it doesn't limit the selection by key.</param>
    ''' <param name="tagMask">If present, limits the selection to items with the
    ''' specified tag mask, in the form "+wanted tag; -unwanted; +also wanted"
    ''' where "+" denotes tags which must be present, and "-" denotes tags which
    ''' must not be present. If not present, it doesn't limit the selection by tag
    ''' </param>
    ''' <returns>The work queue item which matches the given constraints, or null if
    ''' there was no pending work queue item matching those constraints.</returns>
    ''' <exception cref="NoSuchQueueException">If no queue with the specified queue
    ''' name was found.</exception>
    Private Function WorkQueueGetNext(
     ByVal con As IDatabaseConnection,
     ByVal sessId As Guid,
     ByVal queuename As String,
     ByVal keyfilter As String,
     ByVal tagMask As clsTagMask) As clsWorkQueueItem

        Using cmd As New SqlCommand()
            cmd.CommandText = "usp_getnextcase"
            cmd.CommandType = CommandType.StoredProcedure
            With cmd.Parameters
                .AddWithValue("@sess", IIf(sessId = Nothing, DBNull.Value, sessId))
                .AddWithValue("@queuename", queuename)
                .AddWithValue("@keyfilter", IIf(keyfilter = "", DBNull.Value, keyfilter))
                Dim i As Integer = 0
                If tagMask IsNot Nothing Then
                    For Each tag As String In tagMask.OnTags
                        i += 1
                        If i > 9 Then Throw New NotSupportedException(
                         My.Resources.clsServer_CannotSpecifyMoreThan9TagsToFilterOn)
                        .AddWithValue("@ontag" & i, ApplyWildcard(tag))
                    Next
                    i = 0
                    For Each tag As String In tagMask.OffTags
                        i += 1
                        If i > 9 Then Throw New NotSupportedException(
                         My.Resources.clsServer_CannotSpecifyMoreThan9TagsToFilterOut)
                        .AddWithValue("@offtag" & i, ApplyWildcard(tag))
                    Next
                End If
            End With

            Dim item As clsWorkQueueItem
            Using reader = con.ExecuteReturnDataReader(cmd)
                If Not reader.Read() Then Return Nothing ' No items waiting to be worked
                Dim prov As New ReaderDataProvider(reader)
                item = New clsWorkQueueItem(prov)
                item.DataXml =
                 Decrypt(prov.GetInt("encryptid"), prov.GetString("data"))

            End Using

            ' Log work queue item operation (if required)
            If Licensing.License.TransactionModel Then
                WorkQueueLogAddEntry(con, WorkQueueOperation.ItemLocked, item.Ident)
            End If

            Return item
        End Using
    End Function

    ''' <summary>
    ''' Helper method to update multiple work queue items - this is mainly in place
    ''' because of the awkward constraint limiting the number of items to circa 2000
    ''' The code to work around the constraint is messy, so rather than having it
    ''' in every bulk work queue item method, it's in the one place.
    ''' </summary>
    ''' <param name="query">The query to use <b>preceding</b> the ID tests in the
    ''' where clause. Note that the entirety of the query must be present up to the
    ''' point where the id checks would be done. eg. if id checks are all that is
    ''' required to identify the records to be updated, the query should end with the
    ''' word "WHERE". If other tests are required, the query should contain those
    ''' tests and end with an 'AND' or 'OR' as appropriate, eg. "[rest of query...]
    ''' WHERE sessionid is null AND"
    ''' </param>
    ''' <param name="params">The dictionary containing the parameters which are
    ''' to be used for all the specified work items. An empty dictionary (or Nothing)
    ''' indicates no parameters bar the work queue item IDs</param>
    ''' <param name="items">The items which need to be updated. This method will
    ''' use the identities of the items to update them, <em>only if</em> every
    ''' item in the list has a valid identity set. Otherwise, it will use the ID
    ''' of the item (thereby updating all instances of the item rather than a single
    ''' instance).</param>
    ''' <param name="tableAlias">The alias used within the given query to identify
    ''' the work queue item table. By default, no alias is used, but if the given
    ''' query creates an alias and the column name 'id' is ambiguous, one may be
    ''' required here.</param>
    ''' <exception cref="SqlException">If any database errors occur while attempting
    ''' to update the list of work queue items.</exception>
    Private Function PerformBulkQueueItemUpdate(
     ByVal query As String,
     ByVal params As Dictionary(Of String, Object),
     ByVal items As IList(Of clsWorkQueueItem),
     Optional ByVal tableAlias As String = Nothing) As Integer

        If items.Count = 0 Then Return 0 ' avoid allocating resources if nowt to do
        Using con = GetConnection()
            Return PerformBulkQueueItemUpdate(query, params, items, con, tableAlias)
        End Using

    End Function

    ''' <summary>
    ''' Helper method to update multiple work queue items on a specified connection.
    ''' This is mainly in place because of the awkward constraint limiting
    ''' the number of items to circa 2000.
    ''' The code to work around the constraint is messy, so rather than having it
    ''' in every bulk work queue item method, it's in the one place.
    ''' Note that this method expects an initialised IDatabaseConnection instance to
    ''' do its work on - it will not dispose of the connection once it's finished,
    ''' meaning that this can be used within a transaction without causing an
    ''' implicit commit / rollback.
    ''' </summary>
    ''' <param name="query">The query to use <b>preceding</b> the ID tests in the
    ''' where clause. Note that the entirety of the query must be present up to the
    ''' point where the id checks would be done. eg. if id checks are all that is
    ''' required to identify the records to be updated, the query should end with the
    ''' word "WHERE". If other tests are required, the query should contain those
    ''' tests and end with an 'AND' or 'OR' as appropriate, eg. "[rest of query...]
    ''' WHERE sessionid is null AND"
    ''' </param>
    ''' <param name="params">The dictionary containing the parameters which are
    ''' to be used for all the specified work items. An empty dictionary (or Nothing)
    ''' indicates no parameters bar the work queue item IDs</param>
    ''' <param name="items">The items which need to be updated. This method will
    ''' use the identities of the items to update them, <em>only if</em> every
    ''' item in the list has a valid identity set. Otherwise, it will use the ID
    ''' of the item (thereby updating all instances of the item rather than a single
    ''' instance).</param>
    ''' <param name="con">The connection on which the update should be performed
    ''' </param>
    ''' <param name="tableAlias">The alias used within the given query to identify
    ''' the work queue item table. By default, no alias is used, but if the given
    ''' query creates an alias and the column name 'id' is ambiguous, one may be
    ''' required here.</param>
    ''' <returns>The total number of records which were updated in this update.
    ''' </returns>
    ''' <exception cref="SqlException">If any database errors occur while attempting
    ''' to update the list of work queue items.</exception>
    ''' <exception cref="NullReferenceException">If no connection was passed in the
    ''' 'con' parameter of this method.</exception>
    ''' <remarks>This could be a lot easier to follow if it used the
    ''' <see cref="clsWindowedEnumerable(Of T)"/> class or the
    ''' <see cref="UpdateMultipleIDs"/> method. It doesn't because neither existed
    ''' at the time this method was written.</remarks>
    Private Function PerformBulkQueueItemUpdate(
     ByVal query As String,
     ByVal params As Dictionary(Of String, Object),
     ByVal items As IList(Of clsWorkQueueItem),
     ByVal con As IDatabaseConnection,
     Optional ByVal tableAlias As String = Nothing) As Integer

        ' Let's see if we have the idents we can use.
        Dim useIdents As Boolean = True
        For Each item As clsWorkQueueItem In items
            If item.Ident = 0 Then
                useIdents = False
                Exit For
            End If
        Next

        Dim cmd As New SqlCommand()
        Dim numParams As Integer
        Dim affectedCount As Integer = 0

        If params Is Nothing Then numParams = 0 Else numParams = params.Count

        For index As Integer = 0 To items.Count - 1 Step MaxSqlParams - numParams

            ' This only occurs each ~2000 records - the expense of building the query
            ' and params up again is minimal in comparison.
            cmd.Parameters.Clear()
            If params IsNot Nothing Then
                For Each name As String In params.Keys
                    cmd.Parameters.AddWithValue(name, params(name))
                Next
            End If
            ' The length of the sublist that we want to process
            Dim len As Integer = Math.Min(MaxSqlParams - numParams, items.Count - index)

            Dim sb As New StringBuilder(query)
            Dim idName As String = CStr(IIf(useIdents, "ident", "id"))
            If Not String.IsNullOrEmpty(tableAlias) Then idName = tableAlias + "." + idName
            sb.Append(" "c).Append(idName).Append(" in (")

            ' For each ID we're marking, add a param name and value
            ' We need to name the id params the same so SQL Server can compile the query
            ' We want to keep the numbers as close to the last iteration as
            ' possible (ie. param names) to give SQL Server a chance to cache
            ' the execution plan - subsequent updates go much faster this way
            For j As Integer = 0 To len - 1
                sb.AppendFormat("@id{0},", j)
                If useIdents Then
                    cmd.Parameters.AddWithValue("@id" & j, items(j + index).Ident)
                Else
                    cmd.Parameters.AddWithValue("@id" & j, items(j + index).ID)
                End If
            Next j
            sb.Length -= 1 ' Excise the last comma
            cmd.CommandText = sb.Append(")"c).ToString()
            affectedCount += con.ExecuteReturnRecordsAffected(cmd)

        Next index

        Return affectedCount

    End Function


    ''' <summary>
    ''' Utility method to take a single GUID item ID and return a single element
    ''' list of clsWorkQueueItems for use in the bulk update methods.
    ''' </summary>
    ''' <param name="itemId">The ID for which the list is required</param>
    ''' <returns>The single element list containing the WorkQueueItem object
    ''' representing the Item with the given ID</returns>
    Private Function MakeItemArray(ByVal itemId As Guid) As IList(Of clsWorkQueueItem)
        Return GetSingleton.IList(New clsWorkQueueItem(itemId))
    End Function


    ''' <summary>
    ''' Adds the given tag to the work queue item identified by the specified ID.
    ''' </summary>
    ''' <param name="itemId">The ID of the work queue item to add a tag to</param>
    ''' <param name="tag">The tag to add to the work queue item</param>
    <SecuredMethod()>
    Public Sub WorkQueueItemAddTag(
     ByVal itemId As Guid,
     ByVal tag As String,
     ByVal lockedByCurrentProcess As Boolean) Implements IServer.WorkQueueItemAddTag
        CheckPermissions()

        If String.IsNullOrEmpty(tag) Then
            Throw New MissingDataException(My.Resources.clsServer_CannotAddAnEmptyTag)
        End If

        WorkQueueItemApplyTags(
            MakeItemArray(itemId), New List(Of String)(New String() {"+" & tag}), lockedByCurrentProcess)

    End Sub

    ''' <summary>
    ''' Removes the specified tag from the work queue item identified by the given ID
    ''' </summary>
    ''' <param name="itemId">The ID of the work queue item to remove a tag from
    ''' </param>
    ''' <param name="tag">The tag to remove from the work queue item</param>
    ''' <param name="lockedByCurrentProcess">Is the Work Queue Item locked by current process</param>
    <SecuredMethod()>
    Public Sub WorkQueueItemRemoveTag(
     ByVal itemId As Guid,
     ByVal tag As String,
     ByVal lockedByCurrentProcess As Boolean) Implements IServer.WorkQueueItemRemoveTag
        CheckPermissions()

        If String.IsNullOrEmpty(tag) Then
            Throw New MissingDataException(My.Resources.clsServer_CannotAddAnEmptyTag)
        End If

        WorkQueueItemApplyTags(
         MakeItemArray(itemId), New List(Of String)(New String() {"-" & tag}), lockedByCurrentProcess)

    End Sub

    ''' <summary>
    ''' Applies the given tags to the specified work queue items.
    ''' The tags can prefixed with "+" or "-" to add or remove tags respectively. If
    ''' no prefix is found on a given tag, addition is assumed.
    ''' Note that tags are only applied to / removed from the latest instance of the
    ''' work queue items specified.
    ''' </summary>
    ''' <param name="items">The work queue items to have the specified tags applied</param>
    ''' <param name="tags">The tags to apply, optionally prefixed by "+" or "-"</param>
    ''' <param name="lockedByCurrentProcess">Is the Work Queue Item locked by current process</param>
    Private Sub WorkQueueItemApplyTags(
     ByVal items As IList(Of clsWorkQueueItem),
     ByVal tags As List(Of String),
     ByVal lockedByCurrentProcess As Boolean)

        If items.Count > 0 OrElse tags.Count > 0 Then
            Dim tagger As New clsTagMask()
            For Each tag As String In tags
                tagger.ApplyTag(tag)
            Next

            ' So now we get down to the act of writing all this to the database.
            Using con = GetConnection()

                Dim added As ICollection(Of String) = tagger.OnTags
                Dim removed As ICollection(Of String) = tagger.OffTags

                ' Okay we want this to appear as one from a DB perspective, so BEGIN TRAN it
                con.BeginTransaction()

                ' Should only be able to perform this action if not locked by another process
                If Not lockedByCurrentProcess AndAlso CheckIfWorkQueueItemsAreLocked(items, con) Then
                    Throw New BluePrismException(My.Resources.clsServer_WorkQueueItemIsLockedByAnotherProcess)
                End If

                ' We want to work only on the latest identities...
                WorkQueuePopulateLatestIdents(con, items, True)

                If removed.Count > 0 Then

                    ' First, remove those set for removal, cos that's easier
                    ' I'm assuming the tags will not hit the 2000 SQL params
                    ' limit (?), but the items might (if this is performed
                    ' from the GUI), so send it through bulk queue item handling
                    Dim sb As New StringBuilder(
                         "delete BPAWorkQueueItemTag " &
                         "from BPAWorkQueueItemTag it " &
                         "    join BPAWorkQueueItem i on it.queueitemident = i.ident " &
                         "    join BPATag t on it.tagid = t.id " &
                         "where t.tag in (")
                    ' add the tags
                    Dim ind As Integer = 0
                    Dim params As New Dictionary(Of String, Object)
                    For Each tag As String In removed
                        ind += 1
                        Dim tagVar As String = String.Format("@tag{0}", ind)
                        sb.Append(tagVar).Append(","c)
                        params(tagVar) = tag
                    Next
                    sb.Length -= 1
                    sb.Append(") AND") ' prepared for PerformBulkUpdate()
                    PerformBulkQueueItemUpdate(sb.ToString(), params, items, con, "i")

                End If

                If added.Count > 0 Then
                    Dim sb As New StringBuilder(512)
                    ' Dim cmd As New SqlCommand()
                    Dim i As Integer = 0 ' Counter for tags and then, later, items too.
                    Dim params As New Dictionary(Of String, Object)

                    ' First, make sure the tag records themselves exist.
                    For Each tag As String In added
                        i += 1
                        sb.AppendFormat(
                             " if not exists (select 1 from BPATag where tag = @tag{0})" &
                             "   insert into BPATag (tag) values (@tag{0});", i
                            )
                        params("@tag" & i) = tag
                    Next

                    sb.AppendFormat(
                         " insert into BPAWorkQueueItemTag (queueitemident,tagid)" &
                         " select i.ident, t.id" &
                         "   from BPATag t, BPAWorkQueueItem i " &
                         "   where t.tag in ("
                        )
                    ' There are tags defined from 1 to i, they're already set as params...
                    For j As Integer = 1 To i
                        sb.AppendFormat("@tag{0},", j)
                    Next
                    ' Remove that last comma
                    sb.Length -= 1
                    sb.Append(") and ")

                    PerformBulkQueueItemUpdate(sb.ToString(), params, items, con, "i")

                End If

                ' Clean up the tags - ie. remove redundant ones.
                If removed.Count > 0 AndAlso Not SkipTagHousekeeping Then
                    Try
                        Dim cmd As New SqlCommand()
                        Dim sb As New StringBuilder(
                             " delete BPATag" &
                             " from BPATag t" &
                             " where not exists (" &
                             "     select 1 from BPAWorkQueueItemTag where tagid = t.id" &
                             "   ) and t.tag in ("
                            )

                        Dim ind As Integer = 0
                        For Each tag As String In removed
                            ind += 1
                            sb.AppendFormat("@tag{0},", ind)
                            cmd.Parameters.AddWithValue("@tag" & ind, tag)
                        Next
                        sb.Length -= 1
                        cmd.CommandText = sb.Append(")").ToString()
                        con.Execute(cmd)

                    Catch sqle As SqlException When sqle.Number = DatabaseErrorCode.DeadlockVictim
                        ' Deadlock on BPATag - ignore it, it's only housekeeping anyway and
                        ' it will be done the next time a tag is removed.

                    End Try

                End If

                con.CommitTransaction()

            End Using ' Automatically rolls back any dangling transaction when Dispose() is called
        End If
    End Sub

    ''' <summary>
    ''' Checks if any of the items passed in are locked.
    ''' Built to allow the use of multiple WorkQueueItems like the rest of this
    ''' functionality, however is only implemented with a single @item
    ''' </summary>
    ''' <param name="items">List of Work Queue items, however only implemented using a single one</param>
    ''' <param name="con"></param>
    ''' <returns>Returns the boolean locksExist</returns>
    Private Function CheckIfWorkQueueItemsAreLocked(
         ByVal items As IList(Of clsWorkQueueItem),
         ByVal con As IDatabaseConnection) As Boolean

        Dim locksExist = False
        mSqlHelper.SelectMultipleIds(
            con,
            items.Select(Function(e) e.ID),
            Sub(prov) locksExist = prov.GetBool("LocksExist"),
            "select " &
            "case " &
            "    when exists " &
            "         ( " &
            "              select 1 " &
            "                   from BPAWorkQueueItem qi " &
            "                   join BPACaseLock cl " &
            "                   on     qi.ident = cl.id " &
            "                   where  qi.id in ({multiple-ids})) then 1 " &
            "        else 0 " &
            "end as LocksExist"
        )

        Return locksExist

    End Function

    ''' <summary>
    ''' Gets the latest entered IDENTITY values for the IDs in the given list of work
    ''' queue items, overwriting them into the objects as specified.
    ''' Conceptually, this gets the identities of the latest <em>instance</em> of
    ''' each of the given work queue items.
    ''' </summary>
    ''' <param name="items">The items whose identities are to be populated</param>
    ''' <param name="overwriteAllIdentities">True to force the latest identities into
    ''' the items given, overwriting any that are already there. False to only add
    ''' identities to items which have no identity set (ie. have an ident value of 0)
    ''' </param>
    ''' <exception cref="SqlException">If any database errors occur while attempting
    ''' to retrieve the identities of the items.</exception>
    Private Sub WorkQueuePopulateLatestIdents(
     ByVal items As ICollection(Of clsWorkQueueItem),
     ByVal overwriteAllIdentities As Boolean)
        Using con = GetConnection()
            WorkQueuePopulateLatestIdents(con, items, overwriteAllIdentities)
        End Using

    End Sub

    ''' <summary>
    ''' Gets the latest entered IDENTITY values for the IDs in the
    ''' given list of work queue items. Conceptually, this gets the identities of the
    ''' latest <em>instance</em> of each of the given work queue items.
    ''' </summary>
    ''' <param name="con">The connection on which to retrieve the latest Ident values
    ''' </param>
    ''' <param name="items">The items whose identities are to be populated</param>
    ''' <param name="overwriteAllIdentities">True to force the latest identities into
    ''' the items given, overwriting any that are already there. False to only add
    ''' identities to items which have no identity set (ie. have an ident value of 0)
    ''' </param>
    ''' <exception cref="SqlException">If any database errors occur while attempting
    ''' to retrieve the identities of the items.</exception>
    ''' <exception cref="NullReferenceException">If no connection was given
    ''' </exception>
    Private Sub WorkQueuePopulateLatestIdents(
     ByVal con As IDatabaseConnection,
     ByVal items As ICollection(Of clsWorkQueueItem),
     ByVal overwriteAllIdentities As Boolean)

        Dim cmd As New SqlCommand("select max(ident) from BPAWorkQueueItem where id = @id")
        cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier)
        For Each item As clsWorkQueueItem In items
            cmd.Parameters("@id").Value = item.ID
            If overwriteAllIdentities OrElse item.Ident = 0 Then
                item.Ident = CLng(con.ExecuteReturnScalar(cmd))
            End If
        Next

    End Sub

    ''' <summary>
    ''' Gets the latest states of all items in the given list.
    ''' This gets the latest item entry with each of the items IDs and returns
    ''' an item corresponding to that entry which represents the latest instance
    ''' of that item. Note that the only fields populated are the Id, Ident,
    ''' KeyValue and CurrentState fields - the return should be used internally
    ''' for checking state and not returned to the client code without the
    ''' remaining data being filled first.
    ''' Also note that if the items given contain 2 (or more) instances of the
    ''' same item, only one instance (the latest) will be returned for that
    ''' item.
    ''' </summary>
    ''' <param name="con">The connection over which the latest state data
    ''' should be retrieved.</param>
    ''' <param name="items">The items for which the latest instance state is
    ''' required. The only field queried on items in this list is the Id field.
    ''' </param>
    ''' <returns>A list of partially populated work queue items, containing the
    ''' ID, Ident, KeyValue and CurrentState of the latest instance of each item
    ''' specified in the input list.</returns>
    Private Function WorkQueueGetLatestStates(
     ByVal con As IDatabaseConnection, ByVal items As ICollection(Of clsWorkQueueItem)) _
     As IList(Of clsWorkQueueItem)

        ' Shortcut out if nothing there...
        If items Is Nothing OrElse items.Count = 0 Then Return New List(Of clsWorkQueueItem)

        ' Find the latest instances of the items in the list.
        Dim sb As New StringBuilder(
         " select i.ident, i.id, i.keyvalue, i.state" &
         " from BPVWorkQueueItem i" &
         "   left join BPAWorkQueueItem inext on i.id = inext.id" &
         "     and inext.attempt = i.attempt + 1" &
         " where inext.id is null" &
         "   and i.id in ("
        )
        Dim cmd As New SqlCommand()
        Dim i As Integer = 0
        For Each item As clsWorkQueueItem In items
            i += 1
            cmd.Parameters.AddWithValue("@id" & i, item.ID)
            sb.AppendFormat("@id{0},", i)
        Next
        sb.Length -= 1
        sb.Append(")")
        cmd.CommandText = sb.ToString()

        Dim stateItems As New List(Of clsWorkQueueItem)

        ' Read through them all, set the limited data into each item and
        ' and add them to the list
        Using reader = con.ExecuteReturnDataReader(cmd)

            Dim prov As ReaderDataProvider = New ReaderDataProvider(reader)
            While reader.Read()

                Dim item As New clsWorkQueueItem(
                 prov.GetValue("id", Guid.Empty),
                 prov.GetValue("ident", 0L),
                 prov.GetValue("keyvalue", ""))
                item.CurrentState = prov.GetValue("state", clsWorkQueueItem.State.None)

                stateItems.Add(item)

            End While

        End Using

        Return stateItems

    End Function

    ''' <summary>
    ''' Attempts to force a retry of the given list of work queue items.
    ''' Note that this operates on the <em>item</em>, not the retry attempt - meaning
    ''' that if an instance is passed which has a later retry attempt, the state of
    ''' the latest attempt is treated as the state of the item.
    ''' This method will fail if any of the given items are in an invalid state,
    ''' ie. they still have a pending / locked instance or if a later instance has
    ''' completed successfully.
    ''' </summary>
    ''' <param name="items">The items for which a retry attempt should be forced
    ''' through disregarding the attempt number and the queue's maxattempts value.
    ''' </param>
    ''' <returns>A collection of messages indicating which items in the given list
    ''' were not force retried and explaining why for each one.</returns>
    ''' <exception cref="InvalidWorkItemStateException">If the retry failed due
    ''' to one of the specified items being either pending or completed. The
    ''' exception's detail message will be human-readable.</exception>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Function WorkQueueForceRetry(
        items As ICollection(Of clsWorkQueueItem),
        queueId As Guid,
        manualChange As Boolean) As ICollection(Of String) Implements IServer.WorkQueueForceRetry

        CheckPermissions()
        ' Shortcircuit out of here...
        If items Is Nothing OrElse items.Count = 0 Then _
         Return GetEmpty.ICollection(Of String)()

        Using con = GetConnection()

            ' Get the latest instances (and their states) for each item required to
            ' be retried.
            Dim latestInstances As IList(Of clsWorkQueueItem) =
             WorkQueueGetLatestStates(con, items)

            Dim invalidStates As IDictionary(Of String, String) =
             WorkQueueCheckForInvalidStates(latestInstances, clsWorkQueueItem.State.Exceptioned)

            ' We want to figure out which ones we want to actually process then,
            ' ie. those which are not in invalid states for forcing a retry
            Dim todo As New List(Of clsWorkQueueItem)
            For Each item As clsWorkQueueItem In latestInstances
                If Not invalidStates.ContainsKey(item.KeyValue) Then todo.Add(item)
            Next

            Dim retriedIds As ICollection(Of Guid) =
             WorkQueueRetryItems(con, todo, False, False)

            ' Log work queue item operation (if required)
            WorkQueueLogAddEntries(con, WorkQueueOperation.ItemForceRetried, retriedIds)

            If todo.Count > 0 AndAlso manualChange Then
                If items.Count = 1 Then
                    Dim item As clsWorkQueueItem = items(0)
                    Dim itemKey = If(String.IsNullOrEmpty(item.KeyValue),
                        My.Resources.clsServer_ItemKeyNotSet,
                        String.Format(My.Resources.clsServer_ItemKeyIs0, item.KeyValue))
                    Dim comment = String.Format(My.Resources.clsServer_0QueueIDIs1AndItemIDIs2, itemKey, queueId, item.ID)
                    AuditManualQueueChangeEvent(WorkQueueEventCode.ForceRetry, queueId, comment)
                Else
                    Dim itemIds = New List(Of Guid)
                    itemIds.AddRange(From item In items Select item.ID)

                    For Each comment In GetMultipleIdsComments(My.Resources.clsServer_MultipleIdsRetried, itemIds, queueId)
                        AuditManualQueueChangeEvent(WorkQueueEventCode.ForceRetry, queueId, comment)
                    Next
                End If

            End If

            Return invalidStates.Values

        End Using
    End Function

    ''' <summary>
    ''' Creates a cloned retry instance of each of the given work queue items.
    ''' </summary>
    ''' <param name="con">The DB connection over which the DB queries should be sent
    ''' </param>
    ''' <param name="items">The items to create a retry of.</param>
    ''' <param name="honourMaxAttempts">True to indicate that the instance should
    ''' only be created if the item has not been retried the maximum number of times
    ''' as specified in the queue.</param>
    ''' <param name="keepLocked">True to indicate that a lock marker should be placed
    ''' on the newly created retry instances.</param>
    ''' <returns>The IDs of the items that have been retried from the given list
    ''' </returns>
    Private Function WorkQueueRetryItems(
     ByVal con As IDatabaseConnection,
     ByVal items As ICollection(Of clsWorkQueueItem),
     ByVal honourMaxAttempts As Boolean,
     ByVal keepLocked As Boolean) As ICollection(Of Guid)

        If items.Count = 0 Then Return GetEmpty.ICollection(Of Guid)()

        Dim i As Integer ' Used to build up IDs/Idents in the queries.
        Dim idsToRetry As New clsSet(Of Guid)
        Dim itemsToRetry As New Dictionary(Of Guid, Long)

        Dim cmd As New SqlCommand()

        ' Create the temp table we need first
        cmd.CommandText =
         " if object_id('tempdb..#retry_itemidents') is not null drop table #retry_itemidents;" &
         " create table #retry_itemidents (ident bigint not null, id uniqueidentifier not null);"

        con.Execute(cmd)


        ' We ensure we only pick up the instances for which there is no later
        ' instance (hence the seemingly redundant join)
        Dim sb As New StringBuilder(
         " select i.id, i.ident" &
         " from BPAWorkQueueItem i"
        )

        ' If we're complying with the queue's maxattempts join on the queue table
        ' ensuring that the attempt we're using is at least 1 less than the
        ' maxattempts value
        If honourMaxAttempts Then
            sb.Append(
             "   join BPAWorkQueue q on q.ident = i.queueident" &
             " where i.attempt < q.maxattempts and")
        Else
            sb.Append(" where")
        End If

        sb.Append(
         " i.ident in (")

        i = 0
        For Each item As clsWorkQueueItem In items
            If i > 0 Then sb.Append(","c)
            i += 1
            sb.Append("@ident").Append(i)
            cmd.Parameters.AddWithValue("@ident" & i, item.Ident)
        Next
        sb.Append(")")
        cmd.CommandText = sb.ToString()

        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            While reader.Read()
                itemsToRetry(prov.GetValue("id", Guid.Empty)) =
                 prov.GetValue("ident", 0L)
            End While
        End Using

        ' If there's nothing to do... well, do nothing
        If itemsToRetry.Count = 0 Then Return itemsToRetry.Keys

        cmd.Parameters.Clear()

        ' I've added SQL comments to indicate which part of the select maps onto
        ' which column in the insert - primarily cos of VB's inability to cope
        ' with comments which aren't at end of (even a '_' separated) line.
        UpdateMultipleIds(Of Guid)(con, cmd, itemsToRetry.Keys, Nothing,
         " insert into BPAWorkQueueItem" &
         "   (id,queueid,keyvalue,status,attempt,loaded,completed," &
         "   exception,exceptionreason,deferred,worktime,prevworktime,data,encryptid,queueident," &
         "   sessionid,priority) " &
         " output inserted.ident, inserted.id into #retry_itemidents (ident,id)" &
         " select i.id," &
         "   i.queueid," &
         "   i.keyvalue," &
         "   i.status," &
         "   i.attempt + 1," &
         "   i.loaded, /* i.loaded */ " &
         "   null, /* i.completed */" &
         "   null, /* i.exception */" &
         "   null, /* i.exceptionreason */" &
         "   i.deferred," &
         "   i.worktime, /* worktime := total worktime */" &
         "   i.worktime, /* prevworktime */" &
         "   i.data," &
         "   i.encryptid," &
         "   i.queueident," &
         "   i.sessionid," &
         "   i.priority" &
         " from BPAWorkQueueItem i " &
         "   left join BPAWorkQueueItem inext on i.id = inext.id and inext.attempt = i.attempt + 1" &
         " where inext.id is null and i.id in ("
        )

        ' From here on, #retry_itemidents has all the data we need; dump the params
        cmd.Parameters.Clear()

        If keepLocked Then
            cmd.CommandText =
             " insert into BPACaseLock (id, locktime, sessionid, lockid)" &
             " select i.ident, getutcdate(), i.sessionid, newid()" &
             "   from BPAWorkQueueItem i" &
             "     join #retry_itemidents ii on i.ident = ii.ident"
            con.Execute(cmd)
        End If

        ' We now need to copy the tag assignments from the old item record to the new one
        Try
            cmd.CommandText =
         " insert into BPAWorkQueueItemTag (queueitemident, tagid)" &
         " select" &
         "   ii.ident," &
         "   it.tagid" &
         " from BPAWorkQueueItem i" &
         "   join #retry_itemidents ii on i.ident = ii.ident" &
         "   join BPAWorkQueueItem ilast on ilast.id = ii.id and ilast.attempt = i.attempt - 1" &
         "   join BPAWorkQueueItemTag it on ilast.ident = it.queueitemident"
            con.Execute(cmd)
        Catch
            'If a tag is deleted before this statement is run, it's possible a foreign key constraint
            'is thrown here. In this case, we should continue as if the tag has been deleted it shouldn't
            'be re-applied.
        End Try

        cmd.CommandText =
         " drop table #retry_itemidents;"
        con.Execute(cmd)

        Return itemsToRetry.Keys

    End Function

    ''' <summary>
    ''' Marks the given list of work queue items as exceptions
    ''' </summary>
    ''' <param name="sessionId">The ID of the session which is marking the given
    ''' items as exceptions - this will be set into the record representing each item
    ''' as the session id which last operated on the item.</param>
    ''' <param name="items">The list of work queue items to mark as exceptions</param>
    ''' <param name="reason">The reason to enter for the exception</param>
    ''' <param name="retry">True to indicate a retry should be performed if the work
    ''' queue allows any more attempts than the item has had; False to indicate that
    ''' it shouldn't</param>
    ''' <param name="keepLocked">True to keep the clones of the specified work
    ''' queue item lockeds after a retry has been initiated.</param>
    ''' <param name="retriedItemCount">After execution, this will contain the count
    ''' of items for which a retry instance has been created.</param>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Sub WorkQueueMarkException(
     sessionId As Guid,
     items As IList(Of clsWorkQueueItem),
     reason As String,
     retry As Boolean,
     keepLocked As Boolean,
     ByRef retriedItemCount As Integer,
     queueId As Guid,
     manualChange As Boolean) Implements IServer.WorkQueueMarkException
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            WorkQueueMarkException(
             con, sessionId, items, reason, retry, keepLocked, retriedItemCount, queueId, manualChange)
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Marks the given list of work queue items as exceptions
    ''' </summary>
    ''' <param name="sessionId">The ID of the session which is marking the given
    ''' items as exceptions - this will be set into the record representing each item
    ''' as the session id which last operated on the item.</param>
    ''' <param name="items">The list of work queue items to mark as exceptions</param>
    ''' <param name="reason">The reason to enter for the exception</param>
    ''' <param name="retry">True to indicate a retry should be performed if the work
    ''' queue allows any more attempts than the item has had; False to indicate that
    ''' it shouldn't</param>
    ''' <param name="keepLocked">True to keep the clones of the specified work
    ''' queue item lockeds after a retry has been initiated.</param>
    ''' <param name="retriedItemCount">After execution, this will contain the count
    ''' of items for which a retry instance has been created.</param>
    Private Sub WorkQueueMarkException(
     con As IDatabaseConnection,
     sessionId As Guid,
     items As IList(Of clsWorkQueueItem),
     reason As String,
     retry As Boolean,
     keepLocked As Boolean,
     ByRef retriedItemCount As Integer,
     queueId As Guid,
     manualChange As Boolean)

        ' OK - we have a couple of things to do here -
        ' we need to exception the item itself, then figure out if we are
        ' performing a retry or not.
        ' If so, we clone the item, up the attempt count on the new item, and
        ' optionally lock it (depending on the 'keep locked' variable).

        ' First, separate off all the IDs into their own collection
        Dim ids As New clsSet(Of Guid)
        For Each item As clsWorkQueueItem In items
            If item.ID <> Nothing Then ids.Add(item.ID)
        Next
        If ids.Count = 0 Then Throw New BluePrismException(
         My.Resources.clsServer_NoQueueItemsToWork0ItemSGivenWithNoIDs, items.Count)

        Dim cmd As New SqlCommand()

        ' Create a temp table for the item idents that we're marking.
        ' We do this separately for a couple of reasons -
        ' 1) if we don't, SQL Server loses the temp table in between (strange, but
        '    appears to bear out - see http://stackoverflow.com/a/6651224/430967 )
        ' 2) we don't want to drop it and recreate it for each batch of IDs we're
        '    updating if we're updating > 2100 items
        cmd.CommandText =
         " if object_id('tempdb..#except_itemidents') is not null drop table #except_itemidents;" &
         " create table #except_itemidents (ident bigint not null, id uniqueidentifier not null);"

        con.Execute(cmd)

        ' Update the current instance - make sure we're not overwriting
        ' any older ones (inext.id is null check ensures we only update latest attempt)
        With cmd.Parameters
            .AddWithValue("@reason", reason)
            .AddWithValue("@session", IIf(sessionId = Guid.Empty, DBNull.Value, sessionId))
        End With
        UpdateMultipleIds(con, cmd, ids, Nothing,
         " update i set " &
         "   i.exception = getutcdate(), " &
         "   i.exceptionreason=@reason," &
         "   i.worktime=isnull(i.worktime+datediff(s,cl.locktime,getutcdate()), i.worktime)," &
         "   i.sessionid = " &
         "     case " &
         "       when @session is null then i.sessionid " &
         "       else @session " &
         "     end " &
         " output inserted.ident, inserted.id into #except_itemidents" &
         " from BPAWorkQueueItem i " &
         "   left join BPACaseLock cl on i.ident = cl.id" &
         "   left join BPAWorkQueueItem inext on i.id=inext.id and inext.attempt=i.attempt+1" &
         " where inext.id is null and i.id in ("
        )

        ' Delete any locks on these items, and select the ids of the updated records
        UnlockCases(con, ids)

        cmd.Parameters.Clear()
        cmd.CommandText = "select ident,id from #except_itemidents;"

        ' WorkQueueRetryItems works on the idents of the items, so we need to
        ' ensure that they are up to date with the latest attempt of the item.
        Dim identLookup As New Dictionary(Of Guid, Long)
        Using reader = con.ExecuteReturnDataReader(cmd)
            While reader.Read()
                identLookup(reader.GetGuid(1)) = reader.GetInt64(0)
            End While
        End Using

        For Each item As clsWorkQueueItem In items
            Dim ident As Long
            If identLookup.TryGetValue(item.ID, ident) Then item.Ident = ident
        Next

        Dim retriedIds As ICollection(Of Guid)
        If retry Then
            retriedIds = WorkQueueRetryItems(con, items, True, keepLocked)
        Else
            ' If we aren't retrying all items, see if any are flagged for a queue session exception retry based on queue
            If items.Any(Function(x) x.SessionExceptionRetry) Then
                Dim retryThese = items.Where(Function(x) x.SessionExceptionRetry).ToList()
                retriedIds = WorkQueueRetryItems(con, retryThese, True, keepLocked)
            Else
                retriedIds = GetEmpty.ICollection(Of Guid)()
            End If
        End If

        ' Set the byref retry count for any interested callers
        retriedItemCount = retriedIds.Count

        ' First get all the 'final exception' IDs by removing all retried IDs
        ids.Subtract(retriedIds)

        ' So now [ids] contains all 'final exceptions' (not retried) and [retriedIds]
        ' contains all 'retried exceptions'
        WorkQueueLogAddEntries(con, WorkQueueOperation.ItemCompletedWithException, ids)
        WorkQueueLogAddEntries(con, WorkQueueOperation.ItemRetryInitiated, retriedIds)

        ' Just a little bit of cleanup
        cmd.CommandText = " drop table #except_itemidents;"
        con.Execute(cmd)

        If manualChange Then
            If items.Count = 1 Then
                Dim item As clsWorkQueueItem = items(0)
                Dim itemKey = If(String.IsNullOrEmpty(item.KeyValue), My.Resources.clsServer_ItemKeyNotSet,
                                 String.Format(My.Resources.clsServer_ItemKeyIs0, item.KeyValue))
                Dim comment = String.Format(My.Resources.clsServer_0QueueIDIs1AndItemIDIs2, itemKey, queueId, item.ID)
                AuditManualQueueChangeEvent(WorkQueueEventCode.ManualException, queueId, comment)
            Else
                Dim itemIds = New List(Of Guid)
                itemIds.AddRange(From item In items Select item.ID)

                For Each comment In GetMultipleIdsComments(My.Resources.clsServer_MultipleItemsMarkedAsException, itemIds, queueId)
                    AuditManualQueueChangeEvent(WorkQueueEventCode.ManualException, queueId, comment)
                Next
            End If
        End If

    End Sub

    Private Shared Function GetMultipleIdsComments(commentResource As String, items As IList(Of Guid), queueId As Guid) As IEnumerable(Of String)
        Dim idsToAppend = New List(Of String)
        Dim idLists As New Dictionary(Of String, Integer)
        For i = 0 To items.Count - 1
            idsToAppend.Add(items(i).ToString)
            If (i + 1) Mod 20 = 0 Then
                idLists.Add(String.Join(", ", idsToAppend), idsToAppend.Count)
                idsToAppend.Clear()
            End If
        Next

        If idsToAppend.Any() Then
            idLists.Add(String.Join(", ", idsToAppend), idsToAppend.Count)
        End If

        Return (From idList In idLists Select String.Format(commentResource, idList.Value, queueId, idList.Key)).ToList()

    End Function

    Private Sub AuditManualQueueChangeEvent(
     eventCode As WorkQueueEventCode,
     queueId As Guid,
     comment As String)
        Dim queueName = String.Empty
        WorkQueueGetQueueName(queueId, queueName)

        AuditRecordWorkQueueEvent(
            eventCode, queueId,
            queueName, comment)
    End Sub

    ''' <summary>
    ''' Checks if there are any work queue items registered to the queue with the
    ''' given identity. Note that if the identity does not represent a queue in this
    ''' environment, this will return False.
    ''' </summary>
    ''' <param name="con">The connection over which to do the check</param>
    ''' <param name="ident">The identity of the queue for which the check is required
    ''' </param>
    ''' <returns>True if any work queue items are found referring to a queue with the
    ''' given identity; False otherwise.</returns>
    Private Function HasAnyItems(con As IDatabaseConnection, ident As Integer) As Boolean
        ' select a single '1' value if there exist any work queue items for the queue
        Dim cmd As New SqlCommand(
            "select top 1 1 from BPAWorkQueueItem where queueident = @ident")
        cmd.Parameters.AddWithValue("@ident", ident)
        ' True if '1' is returned (ie. item found); False otherwise
        Return (IfNull(con.ExecuteReturnScalar(cmd), 0) = 1)
    End Function


    ''' <summary>
    ''' Gets a collection of session numbers for sessions which are assigned to the
    ''' active work queue with the given ident.
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="ident">The identity of the active queue for which the sessions
    ''' are required.</param>
    ''' <returns>A list of session numbers holding the sessions that a queue has
    ''' assigned to it.</returns>
    Private Function GetActiveQueueSessionNos(
     con As IDatabaseConnection, ident As Integer) As ICollection(Of Integer)
        Dim cmd As New SqlCommand(
            "select sessionnumber from BPASession where queueid = @ident")
        cmd.Parameters.AddWithValue("@ident", ident)
        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim sessionNos As New List(Of Integer)
            While reader.Read()
                sessionNos.Add(reader.GetInt32(0))
            End While
            Return sessionNos
        End Using
    End Function

    ''' <summary>
    ''' Acquires the active lock on a specific active queue, returning the key which
    ''' represents the lock if the acquiring of it was successful.
    ''' </summary>
    ''' <param name="ident">The identity of the queue whose active lock should be
    ''' acquired.</param>
    ''' <returns>The GUID representing the lock acquired on the queue</returns>
    ''' <exception cref="NoSuchQueueException">If the given <paramref name="ident"/>
    ''' did not correspond to a work queue.</exception>
    ''' <exception cref="ActiveQueueLockFailedException">If the lock could not be
    ''' acquired on the required active queue.</exception>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Function AcquireActiveQueueLock(ident As Integer) As Guid Implements IServer.AcquireActiveQueueLock
        CheckPermissions()
        Using con = GetConnection()
            Return AcquireActiveQueueLock(con, ident)
        End Using
    End Function

    ''' <summary>
    ''' Acquires the active lock on a specific active queue, returning the key which
    ''' represents the lock if the acquiring of it was successful.
    ''' </summary>
    ''' <param name="con">The connection to the database</param>
    ''' <param name="ident">The identity of the queue whose active lock should be
    ''' acquired.</param>
    ''' <returns>The GUID representing the lock acquired on the queue</returns>
    ''' <exception cref="NoSuchQueueException">If the given <paramref name="ident"/>
    ''' did not correspond to a work queue.</exception>
    ''' <exception cref="ActiveQueueLockFailedException">If the lock could not be
    ''' acquired on the required active queue.</exception>
    Private Function AcquireActiveQueueLock(
     con As IDatabaseConnection, ident As Integer) As Guid
        Dim token As Guid = Guid.NewGuid()
        Dim cmd As New SqlCommand()
        cmd.CommandText =
         " update BPAWorkQueue set " &
         "   activelock = @token," &
         "   activelocktime = getutcdate()," &
         "   activelockname = @lockname" &
         " where ident = @id and activelock is null; " &
         "" &
         " select name, activelock, activelockname, activelocktime" &
         " from BPAWorkQueue where ident = @id;"
        With cmd.Parameters
            .AddWithValue("@id", ident)
            .AddWithValue("@token", token)
            .AddWithValue("@lockname", If(mLoggedInMachine, "<null>"))
        End With
        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            If Not reader.Read() Then Throw New NoSuchQueueException(ident)

            Dim returnedLock As Guid = prov.GetGuid("activelock")
            ' If the returned lock is the one we passed in - lock acquired
            If token = returnedLock Then Return token

            ' Otherwise, it failed to acquire the lock because someone else has it
            Throw New ActiveQueueLockFailedException(
                prov.GetString("name"), prov.GetString("activelockname"),
                prov.GetValue("activelocktime", Date.MinValue))

        End Using
    End Function

    ''' <summary>
    ''' Releases the active queue lock for a specified queue and lock token.
    ''' </summary>
    ''' <param name="ident">The identity of the work queue to release the active lock
    ''' from</param>
    ''' <param name="token">The token generated when the active lock was acquired.
    ''' </param>
    ''' <exception cref="NoSuchQueueException">If the identity did not represent a
    ''' work queue in the database.</exception>
    ''' <exception cref="IncorrectLockTokenException">If the token given did not
    ''' match the lock token held on the active queue</exception>
    ''' <remarks>Note that if the work queue has no active lock set on it, this
    ''' method is effectively a no-op, ie. no error is raised, but the queue is not
    ''' locked when the method exits.</remarks>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Sub ReleaseActiveQueueLock(ident As Integer, token As Guid) Implements IServer.ReleaseActiveQueueLock
        CheckPermissions()
        Using con = GetConnection()
            ReleaseActiveQueueLock(con, ident, token)
        End Using
    End Sub

    ''' <summary>
    ''' Releases the active queue lock for a specified queue and lock token.
    ''' </summary>
    ''' <param name="con">The connection to the database</param>
    ''' <param name="ident">The identity of the work queue to release the active lock
    ''' from</param>
    ''' <param name="token">The token generated when the active lock was acquired.
    ''' </param>
    ''' <exception cref="NoSuchQueueException">If the identity did not represent a
    ''' work queue in the database.</exception>
    ''' <exception cref="IncorrectLockTokenException">If the token given did not
    ''' match the lock token held on the active queue</exception>
    ''' <remarks>Note that if the work queue has no active lock set on it, this
    ''' method is effectively a no-op, ie. no error is raised, but the queue is not
    ''' locked when the method exits.</remarks>
    Private Sub ReleaseActiveQueueLock(
     con As IDatabaseConnection, ident As Integer, token As Guid)
        Dim cmd As New SqlCommand()
        cmd.CommandText =
         " update BPAWorkQueue set " &
         "   activelock = null," &
         "   activelocktime = null," &
         "   activelockname = null" &
         " where ident = @id and (activelock is null or activelock = @lock);" &
         " select activelock, activelockname from BPAWorkQueue where ident = @id;"
        With cmd.Parameters
            .AddWithValue("@id", ident)
            .AddWithValue("@lock", token)
        End With

        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            If Not reader.Read() Then Throw New NoSuchQueueException(ident)

            Dim returnedLock As Guid = prov.GetGuid("activelock")
            ' The returned lock *should* be Guid.Empty to indicate that this method
            ' correctly released it
            If returnedLock = Guid.Empty Then Return

            ' Otherwise, we can't release the lock because it's being held by
            ' someone else
            Throw New IncorrectLockTokenException(
                My.Resources.clsServer_LockCannotBeReleasedHeldUsingTheName0,
                prov.GetString("activelockname"))

        End Using

    End Sub

    ''' <summary>
    ''' Sets the target session for multiple queues
    ''' </summary>
    '''
    <SecuredMethod(Permission.ControlRoom.ManageQueuesReadOnly, Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Sub SetTargetSessionCountForMultipleActiveQueues(activeQueueDetails As IList(Of ActiveQueueTargetSessionCount)) Implements IServer.SetTargetSessionCountForMultipleActiveQueues
        CheckPermissions()
        Using dbConnection = GetConnection()

            For Each queue As ActiveQueueTargetSessionCount In activeQueueDetails
                If Not IsActiveQueueControllable(dbConnection, GetProcessAndResourceGroupForActiveQueue(queue.QueueId, dbConnection)) Then Throw New PermissionException(
                    My.Resources.clsServer_YouDoNotHavePermissionToControlThisActiveQueue)
            Next

            Dim spr_setTargetSessions = "usp_SetTargetSessionsForMultipleWorkQueues"
            Using command = New SqlCommand(spr_setTargetSessions)

                Dim tableValuedParameter = New DataTable()
                Dim queueIdColumnName = "queueId"
                Dim targetSessionAmountColumnName = "targetSessionAmount"

                command.CommandType = CommandType.StoredProcedure

                tableValuedParameter.Columns.Add(New DataColumn(queueIdColumnName, GetType(Integer)))
                tableValuedParameter.Columns.Add(New DataColumn(targetSessionAmountColumnName, GetType(Integer)))

                activeQueueDetails.ToList().ForEach(Sub(targetSessionDetail)
                                                        Dim newRow = tableValuedParameter.NewRow()
                                                        newRow(queueIdColumnName) = targetSessionDetail.QueueId
                                                        newRow(targetSessionAmountColumnName) = targetSessionDetail.TargetSessionCount
                                                        tableValuedParameter.Rows.Add(newRow)
                                                    End Sub)

                command.Parameters.AddWithValue("@tvpTargetSessionDetails", tableValuedParameter)

                dbConnection.Execute(command)
            End Using
        End Using
    End Sub



    ''' <summary>
    ''' Sets the target session count for a specific queue
    ''' </summary>
    ''' <param name="queueIdent">The identity of the queue</param>
    ''' <param name="target">The target sessions for the queue</param>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesReadOnly, Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Sub SetTargetSessionCount(queueIdent As Integer, target As Integer) Implements IServer.SetTargetSessionCount
        CheckPermissions()
        Using con = GetConnection()
            If Not IsActiveQueueControllable(con, GetProcessAndResourceGroupForActiveQueue(queueIdent, con)) Then Throw New PermissionException(
                My.Resources.clsServer_YouDoNotHavePermissionToControlThisActiveQueue)
            SetTargetSessionCount(con, queueIdent, target)
        End Using
    End Sub

    ''' <summary>
    ''' Sets the target session count for a specific queue
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="queueIdent">The identity of the queue</param>
    ''' <param name="target">The target sessions for the queue</param>
    Private Sub SetTargetSessionCount(
     con As IDatabaseConnection, queueIdent As Integer, target As Integer)
        Dim cmd As New SqlCommand(
            "update BPAWorkQueue set targetsessions = @target where ident = @ident")
        With cmd.Parameters
            .AddWithValue("@target", target)
            .AddWithValue("@ident", queueIdent)
        End With
        con.Execute(cmd)
    End Sub

    ''' <summary>
    ''' Gets the target number of sessions set in an active work queue.
    ''' </summary>
    ''' <param name="con">The connection to the database to use.</param>
    ''' <param name="queueIdent">The identity of the active queue for which the
    ''' target number of sessions is required.</param>
    ''' <returns>The target number of sessions on the specified queue, or zero if
    ''' there were no target sessions set in the queue or no queue with the given
    ''' identity could be found.</returns>
    Private Function GetTargetSessionCount(
     con As IDatabaseConnection, queueIdent As Integer) As Integer
        Dim cmd As New SqlCommand(
            "select targetsessions from BPAWorkQueue where ident = @ident")
        cmd.Parameters.AddWithValue("@ident", queueIdent)
        Return IfNull(con.ExecuteReturnScalar(cmd), 0)
    End Function

    ''' <summary>
    ''' Gets the <see cref="clsWorkQueue.Name">names</see> of the work queues which
    ''' have a process with the given ID assigned to them as the process to execute
    ''' in their role as an active queue.
    ''' </summary>
    ''' <param name="con">The connection to use</param>
    ''' <param name="procId">The ID of the process to check for</param>
    ''' <returns>A collection of queue names which correspond to the queues which
    ''' have the given process assigned to them</returns>
    Private Function GetQueuesAssignedWithProcess(
     con As IDatabaseConnection, procId As Guid) As ICollection(Of String)
        If procId = Guid.Empty Then Return GetEmpty.ICollection(Of String)()
        Dim cmd As New SqlCommand(
            "select name from BPAWorkQueue where processid = @procid")
        cmd.Parameters.AddWithValue("@procid", procId)
        Dim names As New List(Of String)
        Using reader = con.ExecuteReturnDataReader(cmd)
            While reader.Read()
                names.Add(reader.GetString(0))
            End While
        End Using
        Return names


    End Function

    ''' <summary>
    ''' Mark a Work Queue item as an exception. This should only be used if the
    ''' caller already has the item locked, but no check is made at this level.
    ''' </summary>
    ''' <param name="sessionId">The session ID that this work queue item has
    ''' been worked under, empty if not part of a session.</param>
    ''' <param name="itemid">The ID of the item to mark</param>
    ''' <param name="reason">The reason for the exception</param>
    ''' <param name="retry">True if the item should be retried, up to the maximum
    ''' number of retries specified for the queue, or False to make the exception
    ''' permanent.</param>
    ''' <param name="keepLocked">True to keep the clone of the specified work
    ''' queue item locked after a retry has been initiated.</param>
    ''' <param name="retried">True if the item had a retry generated after this
    ''' instance; False if no retry was generated, either due to
    ''' <paramref name="retry"/> being False, or because the work queue's maxattempts
    ''' had been hit for this item.</param>
    <SecuredMethod()>
    Public Sub WorkQueueMarkException(
     sessionId As Guid,
     itemid As Guid,
     reason As String,
     retry As Boolean,
     keepLocked As Boolean,
     ByRef retried As Boolean,
     queueId As Guid,
     manualChange As Boolean) Implements IServer.WorkQueueMarkException
        CheckPermissions()
        Dim retryCount As Integer = 0
        retried = False

        ' Had to stop this forward calling as the permissions are different on other
        ' public prototype.
        Using con = GetConnection()
            con.BeginTransaction()
            If queueId = Nothing Then
                queueId = WorkQueueGetQueueIdForItem(itemid, con)
            End If
            WorkQueueMarkException(con, sessionId, MakeItemArray(itemid), reason,
                                   retry, keepLocked, retryCount, queueId, manualChange)
            con.CommitTransaction()
        End Using

        retried = (retryCount = 1)

    End Sub

    ''' <summary>
    ''' Marks each of items currently locked by the given session with an exception.
    ''' </summary>
    ''' <param name="con">The connection to the database to use.</param>
    ''' <param name="sessionId">The ID of the session to mark held items for.
    ''' </param>
    ''' <param name="reason">The exception reason to set in the item.</param>
    Private Sub WorkQueueMarkExceptionsForSession(
     ByVal con As IDatabaseConnection, ByVal sessionId As Guid, ByVal reason As String, isTerminated As Boolean)

        ' Build up a list of items - all we need in there is the ID in order to
        ' provide that to WorkQueueMarkException()
        Dim items As New List(Of clsWorkQueueItem)
        Dim cmd As New SqlCommand(
         " select i.id, wq.sessionexceptionretry" &
         " from BPAWorkQueueItem i" &
         "  inner join BPACaseLock cl on i.ident = cl.id" &
         "  inner join BPAWorkQueue wq on i.queueident = wq.ident" &
         " where cl.sessionid = @sessionid")
        cmd.Parameters.AddWithValue("@sessionid", sessionId)

        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            While reader.Read()
                Dim queueItem = New clsWorkQueueItem(prov.GetValue("id", Guid.Empty)) With {
                    .SessionExceptionRetry = prov.GetValue("sessionexceptionretry", False) AndAlso isTerminated
                }
                items.Add(queueItem)
            End While
        End Using

        If items.Count > 0 Then
            Dim queueID = WorkQueueGetQueueIdForItem(items(0).ID, con)
            WorkQueueMarkException(con:=con, sessionId:=sessionId, items:=items, reason:=reason, retry:=False, keepLocked:=False, retriedItemCount:=0, queueId:=queueID, manualChange:=True)
        End If

    End Sub

    ''' <summary>
    ''' Gets the Item ID to which the given instance ID belongs.
    ''' </summary>
    ''' <param name="ident">The identity of the work queue item for which the item ID
    ''' is required</param>
    ''' <param name="con">The connection on which to get the item ID.</param>
    ''' <returns>The Item ID of the specified work queue item.</returns>
    ''' <exception cref="NoSuchWorkItemException">If no work queue item could be
    ''' found which corresponded to the given identity.</exception>
    Private Function WorkQueueGetItemIdFromIdent(
     ByVal ident As Long,
     ByVal con As IDatabaseConnection) As Guid

        Dim cmd As New SqlCommand("select id from BPAWorkQueueItem where ident=@ident")
        cmd.Parameters.AddWithValue("@ident", ident)

        Using reader = con.ExecuteReturnDataReader(cmd)
            If reader.Read() Then
                Return CType(reader("id"), Guid)
            Else
                Throw New NoSuchWorkItemException(ident)
            End If
        End Using

    End Function

    ''' <summary>
    ''' Gets the work queue ID that the item with the given ID belongs to
    ''' </summary>
    ''' <param name="itemid">The ID of the work queue item for which the
    ''' queue ID is required.</param>
    ''' <param name="con">The valid, open connection with which to connect
    ''' to the database</param>
    ''' <returns>The ID of the work queue containing the item identified by
    ''' the given item ID.</returns>
    ''' <exception cref="NoSuchWorkItemException">If no work queue item with the
    ''' given ID could be found.</exception>
    Private Function WorkQueueGetQueueIdForItem(
     ByVal itemid As Guid,
     ByVal con As IDatabaseConnection) As Guid

        Dim cmd As New SqlCommand("select i.queueid from BPAWorkQueueItem i where i.id = @id")
        cmd.Parameters.AddWithValue("@id", itemid)
        Dim val As Object = con.ExecuteReturnScalar(cmd)
        If val IsNot Nothing AndAlso Not val.Equals(DBNull.Value) Then
            Return CType(val, Guid)
        Else
            Throw New NoSuchWorkItemException(itemid)
        End If

    End Function

    ''' <summary>
    ''' Gets all the related retry instances of the specified work queue item.
    ''' One of item ID or Ident must be provided. This will use the item ID over the
    ''' identity if both are present.
    ''' </summary>
    ''' <param name="itemid">The (Guid) Item ID of the work queue item for which all
    ''' instances are required.</param>
    ''' <param name="ident">The (Long) identity of the work queue item instance, for
    ''' which all related instances are required.</param>
    ''' <param name="items">The collection of work queue item instances which
    ''' represent retries of the specified item.</param>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesFullAccess, Permission.ControlRoom.ManageQueuesReadOnly)>
    Public Sub WorkQueueGetAllRetryInstances(
     ByVal itemid As Guid,
     ByVal ident As Long,
     ByRef items As ICollection(Of clsWorkQueueItem)) Implements IServer.WorkQueueGetAllRetryInstances
        CheckPermissions()
        Using con = GetConnection()

            ' "Normalise" the ID so that we are using the GUID of the item.
            Dim id As Guid
            If itemid <> Nothing Then
                id = itemid
            ElseIf ident <> Nothing Then
                id = WorkQueueGetItemIdFromIdent(ident, con)
            Else
                Throw New MissingDataException(My.Resources.clsServer_NoIDGivenCannotRetrieveInstancesOfWorkQueueItem)
            End If

            ' We also need the Queue ID.
            Dim queueId As Guid = WorkQueueGetQueueIdForItem(id, con)

            ' Create a filter to search for items with the given ID.
            Dim filter As New WorkQueueFilter()
            filter.ItemId = id
            filter.SortColumn = QueueSortColumn.ByAttempt
            filter.SortOrder = QueueSortOrder.Ascending

            ' Perform the search... if it worked, then the results will already
            ' be populated, so just return true. Conversely, if it failed, then
            ' the error message will likewise be populated.
            WorkQueuesGetQueueFilteredContents(
                 con, queueId, filter, Nothing, items)

        End Using
    End Sub

    ''' <summary>
    ''' Unlocks the work queue cases with the given identities
    ''' </summary>
    ''' <param name="con">The connection to the database</param>
    ''' <param name="idents">The collection of identities to unlock</param>
    ''' <returns>The number of records matched in the change</returns>
    Private Function UnlockCases(
     ByVal con As IDatabaseConnection, ByVal idents As ICollection(Of Long)) As Integer
        UpdateMultipleIDs(con, idents, "Update BPAWorkQueueItem SET lockid = null, locktime = null where ident in (")

        Return UpdateMultipleIDs(con, idents, " delete from BPACaseLock where id in (")
    End Function

    ''' <summary>
    ''' Unlocks the work queue cases with the given identities
    ''' </summary>
    ''' <param name="con">The connection to the database</param>
    ''' <param name="ids">The collection of identities to unlock</param>
    ''' <returns>The number of records matched in the change</returns>
    Private Function UnlockCases(
     ByVal con As IDatabaseConnection, ByVal ids As ICollection(Of Guid)) As Integer
        UpdateMultipleIDs(con, ids, "Update BPAWorkQueueItem SET lockid = null, locktime = null where id in (")

        Return UpdateMultipleIDs(con, ids,
         " delete cl " &
         " from BPACaseLock cl " &
         "   join BPAWorkQueueItem i on cl.id = i.ident" &
         " where i.id in ("
        )
    End Function

    ''' <summary>
    ''' Mark a Work Queue item as complete. This should only be used if the caller
    ''' already has the item locked, but no check is made at this level.
    ''' </summary>
    ''' <param name="sessId">The ID of the session which is marking the item as
    ''' complete - this will be stored within the item as the last session operating
    ''' on the work queue item.</param>
    ''' <param name="itemId">The ID of the item to mark</param>
    <SecuredMethod()>
    Public Sub WorkQueueMarkComplete(
     ByVal sessId As Guid,
     ByVal itemId As Guid) Implements IServer.WorkQueueMarkComplete
        CheckPermissions()
        Using con = GetConnection()
            WorkQueueMarkComplete(con, sessId, itemId)
        End Using
    End Sub

    ''' <summary>
    ''' Mark a Work Queue item as complete. This should only be used if the caller
    ''' already has the item locked, but no check is made at this level.
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="sessId">The ID of the session which is marking the item as
    ''' complete - this will be stored within the item as the last session operating
    ''' on the work queue item.</param>
    ''' <param name="itemId">The ID of the item to mark</param>
    Private Sub WorkQueueMarkComplete(
     con As IDatabaseConnection, sessId As Guid, itemId As Guid)

        Using cmd As New SqlCommand(
         " update i set" &
         "     i.exception = null," &
         "     i.completed = getutcdate()," &
         "     i.worktime = i.worktime + isnull(datediff(s,cl.locktime,getutcdate()),0), " &
         "     i.sessionid = " &
         "       case " &
         "         when @session is null then i.sessionid " &
         "         else @session " &
         "       end " &
         "   from BPAWorkQueueItem i" &
             "     left join BPACaseLock cl on cl.id = i.ident" &
         "     left join BPAWorkQueueItem inext on i.id = inext.id and inext.attempt = i.attempt + 1" &
         "   where inext.id is null and i.id = @id"
        )

            With cmd.Parameters
                .AddWithValue("@id", itemId)
                .AddWithValue("@session", IIf(sessId = Guid.Empty, DBNull.Value, sessId))
            End With
            con.Execute(cmd)
        End Using
        UnlockCases(con, GetSingleton.ICollection(itemId))

        ' Log work queue item operation (if required)
        WorkQueueLogAddEntry(con, WorkQueueOperation.ItemCompletedSuccessfully, itemId)
    End Sub

    <SecuredMethod()>
    Public Sub WorkQueueDefer(
     sessionId As Guid,
     itemid As Guid,
     until As Date,
     queueID As Guid) Implements IServer.WorkQueueDefer
        CheckPermissions()
        Using con = GetConnection()
            If queueID = Nothing Then
                queueID = WorkQueueGetQueueIdForItem(itemid, con)
            End If

            con.BeginTransaction()
            ' we only want to update the latest instance of the work queue item, so go get it.
            Dim item As clsWorkQueueItem = New clsWorkQueueItem(itemid)
            WorkQueuePopulateLatestIdents(con, New clsWorkQueueItem() {item}, True)

            Dim cmd As New SqlCommand(
                 " update i set" &
                 "   i.deferred=@until," &
                 "   i.worktime=i.worktime+isnull(datediff(s,cl.locktime,getutcdate()),0), " &
                 "   i.sessionid = " &
                 "     case " &
                 "       when @session is null then i.sessionid " &
                 "       else @session " &
                 "     end " &
                 " from BPAWorkQueueItem i" &
                 "   left join BPACaseLock cl on i.ident = cl.id" &
                 "   left join BPAWorkQueueItem inext on inext.id = i.id and inext.attempt = i.attempt + 1" &
                 " where i.ident = @ident" &
                 "   and inext.id is null"
                )
            With cmd.Parameters
                .AddWithValue("@ident", item.Ident)
                .AddWithValue("@until", until)
                .AddWithValue("@session", IIf(sessionId = Guid.Empty, DBNull.Value, sessionId))
            End With
            con.Execute(cmd)
            ' Log work queue item operation (if required)
            WorkQueueLogAddEntry(con, WorkQueueOperation.ItemDeferred, itemid)
            UnlockCases(con, GetSingleton.ICollection(item.Ident))
            con.CommitTransaction()
        End Using

    End Sub

    ''' <summary>
    ''' Ensures that the given list of work queue items has a valid state, according
    ''' to the given state parameters.
    ''' </summary>
    ''' <param name="items">The items whose state should be checked.</param>
    ''' <param name="errMsgPrefix">The prefix of any error message to be set in the
    ''' exception should any items have an invalid state.</param>
    ''' <param name="allowed">The states which should be considered valid for the
    ''' purposes of this method.</param>
    ''' <exception cref="InvalidWorkItemStateException">If any of the items in
    ''' the given list have a state other than those provided.</exception>
    Private Sub WorkQueueEnsureValidStates(
     ByVal items As IList(Of clsWorkQueueItem),
     ByVal errMsgPrefix As String,
     ByVal ParamArray allowed() As State)
        WorkQueueCheckForInvalidStates(items, errMsgPrefix, True, allowed)
    End Sub

    ''' <summary>
    ''' Checks for the given work items having invalid states, returning a map of the
    ''' state message keyed against the item key for any items in invalid states.
    ''' </summary>
    ''' <param name="items">The items whose state should be checked.</param>
    ''' <param name="allowed">The states which should be considered valid for the
    ''' purposes of this method.</param>
    ''' <returns>A map, keyed against the item's key value, containing a
    ''' user-presentable error message about the invalid state of the work item.
    ''' </returns>
    Private Function WorkQueueCheckForInvalidStates(
     ByVal items As IList(Of clsWorkQueueItem),
     ByVal ParamArray allowed() As State) As IDictionary(Of String, String)
        Return WorkQueueCheckForInvalidStates(items, Nothing, False, allowed)
    End Function

    ''' <summary>
    ''' Checks for the given work items having invalid states, returning a map of the
    ''' state message keyed against the item key for any items in invalid states.
    ''' Optionally, the combined state messages can be thrown in an <see
    ''' cref="InvalidWorkItemStateException" /> rather than just returned.
    ''' </summary>
    ''' <param name="items">The items whose state should be checked.</param>
    ''' <param name="errMsgPrefix">The prefix of any error message to be set in the
    ''' exception should any items have an invalid state and an exception be
    ''' required.</param>
    ''' <param name="allowed">The states which should be considered valid for the
    ''' purposes of this method.</param>
    ''' <returns>A map, keyed against the item's key value, containing a
    ''' user-presentable error message about the invalid state of the work item.
    ''' </returns>
    ''' <exception cref="InvalidWorkItemStateException">If any of the items in
    ''' the given list have a state other than those provided.</exception>
    Private Function WorkQueueCheckForInvalidStates(
     ByVal items As IList(Of clsWorkQueueItem),
     ByVal errMsgPrefix As String,
     ByVal exceptionOnInvalidState As Boolean,
     ByVal ParamArray allowed() As State) As IDictionary(Of String, String)

        Dim allowedStates As New clsSet(Of State)(allowed)
        Dim errors As New Dictionary(Of String, String)

        For Each item As clsWorkQueueItem In items

            ' If we're allowed this state, then move onto the next item.
            If allowedStates.Contains(item.CurrentState) Then Continue For

            Select Case item.CurrentState

                Case clsWorkQueueItem.State.Completed
                    errors(item.KeyValue) = String.Format(
                     My.Resources.clsServer_TheItemWithKey0HasBeenCompleted, item.KeyValue)

                Case clsWorkQueueItem.State.Locked
                    errors(item.KeyValue) = String.Format(
                     My.Resources.clsServer_TheItemWithKey0HasALockedInstance, item.KeyValue)

                Case clsWorkQueueItem.State.Pending, clsWorkQueueItem.State.Deferred
                    errors(item.KeyValue) = String.Format(
                     My.Resources.clsServer_TheItemWithKey0HasAPendingInstance, item.KeyValue)

                Case clsWorkQueueItem.State.Exceptioned
                    errors(item.KeyValue) = String.Format(
                     My.Resources.clsServer_TheItemWithKey0HasBeenTerminated, item.KeyValue)

                Case clsWorkQueueItem.State.None    ' Should never happen, but just in case...
                    errors(item.KeyValue) = String.Format(
                     My.Resources.clsServer_TheItemWithKey0HasIndeterminateState, item.KeyValue)

            End Select
        Next

        If exceptionOnInvalidState AndAlso errors.Count > 0 Then _
         Throw New InvalidWorkItemStateException(
          CollectionUtil.Join(errors.Values, vbCrLf))

        Return errors

    End Function

    ''' <summary>
    ''' Defers a list of work queue items. This is used from the Control Room user
    ''' interface, and only operates on unlocked items that are pending (i.e. not
    ''' completed or exceptioned).
    ''' </summary>
    ''' <param name="items">The items to have their defer date set</param>
    ''' <param name="until">The defer date/time to use on the given items. UTC, as
    ''' always.</param>
    ''' <exception cref="InvalidWorkItemStateException">If any of the items
    ''' have later instances which are pending or completed.</exception>
    <SecuredMethod()>
    Public Sub WorkQueueDefer(
                              ByVal items As IList(Of clsWorkQueueItem),
                              until As DateTime,
                              queueID As Guid,
                              manualChange As Boolean) Implements IServer.WorkQueueDefer
        CheckPermissions()
        If items.Count = 0 Then Return
        Using con = GetConnection()

            con.BeginTransaction()

            Dim latestInstances As IList(Of clsWorkQueueItem) = WorkQueueGetLatestStates(con, items)

            WorkQueueEnsureValidStates(latestInstances,
                                       My.Resources.clsServer_TheSpecifiedItemsCouldNotBeDeferredBecause,
                                       clsWorkQueueItem.State.Deferred, clsWorkQueueItem.State.Pending)

            Dim query As String = "update BPAWorkQueueItem set deferred=@until where"
            Dim params As New Dictionary(Of String, Object)
            params("@until") = until

            PerformBulkQueueItemUpdate(query, params, items, con)

            Dim comment As String
            If items.Count = 1 Then
                Dim item As clsWorkQueueItem = items(0)
                Dim itemKey = If(String.IsNullOrEmpty(item.KeyValue),
                                 My.Resources.clsServer_ItemKeyNotSet,
                                 String.Format(My.Resources.clsServer_ItemKeyIs0, item.KeyValue))
                comment = String.Format(My.Resources.clsServer_0QueueIDIs1AndItemIDIs2TheItemWasDeferredFrom3Until4,
                                        itemKey, queueID, item.ID, item.Deferred, item.Deferred, until)
            Else
                comment = String.Format(My.Resources.clsServer_0ItemsWereDeferredUntil2QueueIDIs1,
                                        items.Count, queueID, until)
            End If

            If manualChange Then
                AuditManualQueueChangeEvent(
                    WorkQueueEventCode.ManualDefer,
                    queueID, comment)
            End If

            con.CommitTransaction()

        End Using

    End Sub

    ''' <summary>
    ''' Update the status of a Work Queue item. This should only be used if the
    ''' caller already has the item locked, but no check is made at this level.
    ''' </summary>
    ''' <param name="itemid">The ID of the item to update</param>
    ''' <param name="status">The new status to set</param>
    ''' <exception cref="ArgumentOutOfRangeException">If the given status is too
    ''' long and its data was rejected by the database.</exception>
    ''' <exception cref="SqlException">If any other database errors occur while
    ''' attempting to update the status.</exception>
    <SecuredMethod()>
    Public Sub WorkQueueUpdateStatus(ByVal itemid As Guid, ByVal status As String) Implements IServer.WorkQueueUpdateStatus
        CheckPermissions()
        WorkQueueUpdateStatus(MakeItemArray(itemid), status)
    End Sub

    ''' <summary>
    ''' Updates the status of a list of work queue items.
    ''' </summary>
    ''' <param name="items">The list of work queue item objects to update</param>
    ''' <param name="status">The status to use for the given items.</param>
    ''' <exception cref="ArgumentOutOfRangeException">If the given status is too
    ''' long and its data was rejected by the database.</exception>
    ''' <exception cref="SqlException">If any other database errors occur while
    ''' attempting to update the status.</exception>
    <SecuredMethod()>
    Public Sub WorkQueueUpdateStatus(
     ByVal items As IList(Of clsWorkQueueItem),
     ByVal status As String) Implements IServer.WorkQueueUpdateStatus
        CheckPermissions()
        ' Okay - we want to get the latest identites for these items
        ' *only if they're not already set*.
        ' - If it's coming from the business object, the GUID represents the latest
        '   instance of the item only. (Ident should = 0)
        ' - If coming from the GUI, this could be performed on any specific instance
        '   of the item. (Ident should <> 0)
        ' Thus, we populate the latest, but leave any which are already set to that
        ' which they are currently set at. (overwriteAllItems = False)
        WorkQueuePopulateLatestIdents(items, False)
        Dim params As New Dictionary(Of String, Object)
        params("@status") = status
        Try
            PerformBulkQueueItemUpdate(
             "update BPAWorkQueueItem set status=@status where", params, items)

        Catch sqle As SqlException When sqle.Number = DatabaseErrorCode.DataTooLongError
            Throw New ArgumentOutOfRangeException(NameOf(status), sqle)

        End Try

    End Sub

    ''' <summary>
    ''' Updates the given work queue on the database and returns it with the most
    ''' up to date details (note that statistics are not updated by this method)
    ''' </summary>
    ''' <param name="wq">The work queue to update on the database.</param>
    ''' <returns>The work queue after updating on the database.</returns>
    <SecuredMethod(Permission.SystemManager.Workflow.WorkQueueConfiguration)>
    Public Function UpdateWorkQueue(ByVal wq As clsWorkQueue) As clsWorkQueue Implements IServer.UpdateWorkQueue
        CheckPermissions()
        Using con = GetConnection()
            UpdateWorkQueue(con, wq)
            Return wq
        End Using
    End Function

    ''' <summary>
    ''' Updates the given work queue on the database along with the running status and returns it with the most
    ''' up to date details (note that statistics are not updated by this method)
    ''' </summary>
    ''' <param name="wq">The work queue to update on the database.</param>
    <SecuredMethod(Permission.SystemManager.Workflow.WorkQueueConfiguration)>
    Public Sub UpdateWorkQueueWithStatus(ByVal wq As clsWorkQueue) Implements IServer.UpdateWorkQueueWithStatus
        CheckPermissions()
        Using con = GetConnection()
            UpdateWorkQueueWithStatus(con, wq)
        End Using
    End Sub

    ''' <summary>
    ''' Sets the name and other details of an existing queue.
    ''' </summary>
    ''' <param name="workQueue">The queue to update.</param>
    ''' <exception cref="ArgumentNullException">If no name was given for the queue.
    ''' </exception>
    ''' <exception cref="NameAlreadyExistsException">If a queue with the specified
    ''' name (and a different ID) already exists on the database.</exception>
    ''' <exception cref="BluePrismException">If the given queue name was too long
    ''' for the database.</exception>
    ''' <exception cref="SqlException">If any other database errors occur.
    ''' </exception>
    Private Sub UpdateWorkQueue(con As IDatabaseConnection, workQueue As clsWorkQueue)

        If workQueue.Name = "" Then Throw New ArgumentNullException("name", My.Resources.clsServer_TheWorkQueueHasNoNameAssociatedWithIt)

        Try
            ' Initialise the command and set the queueid param, which we need for all queries
            Dim cmd As New SqlCommand()
            cmd.Parameters.AddWithValue("@queueid", workQueue.Id)

            ' The actual update command
            cmd.CommandText =
             " update BPAWorkQueue set" &
             "   name = @queuename," &
             "   keyfield = @keyfield," &
             "   maxattempts = @maxattempts," &
             "   encryptid = @encryptid," &
             "   processid = @processid," &
             "   resourcegroupid = @resourcegroupid," &
             "   targetsessions = @targetsessions," &
             "   sessionexceptionretry = @sessionexceptionretry" &
             " where id = @queueid"

            ' Add the remaining params - queueid is already there.
            With cmd.Parameters
                .AddWithValue("@queuename", workQueue.Name)
                .AddWithValue("@keyfield", workQueue.KeyField)
                .AddWithValue("@maxattempts", workQueue.MaxAttempts)
                .AddWithValue("@encryptid", IIf(workQueue.EncryptionKeyID = 0, DBNull.Value, workQueue.EncryptionKeyID))
                .AddWithValue("@sessionexceptionretry", workQueue.SessionExceptionRetry)
                If workQueue.IsActive Then
                    .AddWithValue("@processid", workQueue.ProcessId)
                    .AddWithValue("@resourcegroupid", workQueue.ResourceGroupId)
                    .AddWithValue("@targetsessions", workQueue.TargetSessionCount)
                Else
                    .AddWithValue("@processid", DBNull.Value)
                    .AddWithValue("@resourcegroupid", DBNull.Value)
                    .AddWithValue("@targetsessions", 0)
                End If
            End With

            ' Check how many queues were updated, throw errors if anything other than 1
            Dim affected As Integer = con.ExecuteReturnRecordsAffected(cmd)
            If affected = 0 Then
                Throw New BluePrismException(
                 My.Resources.clsServer_CommandCompletedWithoutErrorButNoQueuesWereUpdated)

            ElseIf affected > 1 Then
                Throw New BluePrismException(
                 My.Resources.clsServer_UnexpectedNumberOfRecordsUpdated0, affected)

            End If

            If workQueue.IsActive Then

                If workQueue.ProcessId <> Guid.Empty Then
                    workQueue.ProcessName = GetProcessNameById(con, workQueue.ProcessId)
                    workQueue.IsAssignedProcessHidden = Not New MemberPermissions(GetEffectiveGroupPermissionsForProcess(con, workQueue.ProcessId)).HasAnyPermissions(mLoggedInUser)
                End If

                If workQueue.ResourceGroupId <> Guid.Empty Then
                    Dim groupPermissions = GetEffectiveGroupPermissions(con, workQueue.ResourceGroupId)
                    Dim hasPermission = groupPermissions.HasPermission(mLoggedInUser, Permission.Resources.ImpliedViewResource)
                    workQueue.IsResourceGroupHidden = Not hasPermission
                    workQueue.ResourceGroupName = GetGroup(workQueue.ResourceGroupId).Name
                End If

            End If
            ' Ensure the encryption key is marked as the 'original' one in the
            ' queue object, now that that value is stored on the database.
            workQueue.CommitEncryptionKey()

            AuditRecordWorkQueueEvent(WorkQueueEventCode.ModifyQueue, workQueue.Id, workQueue.Name,
                                          String.Format(My.Resources.clsServer_WorkQueues_QueueIDIs0NewConfig12345, workQueue.Id, workQueue.Name, workQueue.KeyField, workQueue.MaxAttempts, IIf(workQueue.IsEncrypted, My.Resources.clsServer_WorkQueues_Encrypted, My.Resources.clsServer_WorkQueues_NotEncrypted),
                                                        workQueue.EncryptionKeyID))

        Catch sqlEx As SqlException When sqlEx.Number = DatabaseErrorCode.UniqueConstraintError OrElse sqlEx.Number = DatabaseErrorCode.UniqueIndexError
            'Assume uniqueness constraint on name column violated
            Throw New NameAlreadyExistsException(My.Resources.clsServer_CannotRenameQueueAnotherQueueWithTheName0AlreadyExists, workQueue.Name)
        End Try

    End Sub

    ''' <summary>
    ''' Sets the name and other details of an existing queue.
    ''' </summary>
    ''' <param name="wq">The queue to update.</param>
    ''' <exception cref="ArgumentException">If no name was given for the queue.
    ''' </exception>
    ''' <exception cref="NameAlreadyExistsException">If a queue with the specified
    ''' name (and a different ID) already exists on the database.</exception>
    ''' <exception cref="BluePrismException">If the given queue name was too long
    ''' for the database.</exception>
    ''' <exception cref="SqlException">If any other database errors occur.
    ''' </exception>
    Private Sub UpdateWorkQueueWithStatus(ByVal con As IDatabaseConnection, ByVal wq As clsWorkQueue)
        If String.IsNullOrWhiteSpace(wq.Name) Then Throw New ArgumentException(
         My.Resources.clsServer_TheWorkQueueHasNoNameAssociatedWithIt, "name")

        Try
            ' Initialise the command and set the queueid param, which we need for all queries
            Dim cmd As New SqlCommand()
            cmd.Parameters.AddWithValue("@queueid", wq.Id)

            ' The actual update command
            cmd.CommandText =
             " update BPAWorkQueue set" &
             "   name = @queuename," &
             "   keyfield = @keyfield," &
             "   maxattempts = @maxattempts," &
             "   encryptid = @encryptid," &
             "   processid = @processid," &
             "   resourcegroupid = @resourcegroupid," &
             "   targetsessions = @targetsessions," &
             "   running = @running" &
             " where id = @queueid"

            ' Add the remaining params - queueid is already there.
            With cmd.Parameters
                .AddWithValue("@queuename", wq.Name)
                .AddWithValue("@keyfield", wq.KeyField)
                .AddWithValue("@maxattempts", wq.MaxAttempts)
                .AddWithValue("@encryptid", IIf(wq.EncryptionKeyID = 0, DBNull.Value, wq.EncryptionKeyID))
                .AddWithValue("@running", wq.IsRunning)
                If wq.IsActive Then
                    .AddWithValue("@processid", wq.ProcessId)
                    .AddWithValue("@resourcegroupid", wq.ResourceGroupId)
                    .AddWithValue("@targetsessions", wq.TargetSessionCount)
                Else
                    .AddWithValue("@processid", DBNull.Value)
                    .AddWithValue("@resourcegroupid", DBNull.Value)
                    .AddWithValue("@targetsessions", 0)
                End If
            End With

            ' Check how many queues were updated, throw errors if anything other than 1
            Dim affected As Integer = con.ExecuteReturnRecordsAffected(cmd)
            If affected = 0 Then
                Throw New BluePrismException(
                 My.Resources.clsServer_CommandCompletedWithoutErrorButNoQueuesWereUpdated)

            ElseIf affected > 1 Then
                Throw New BluePrismException(
                 My.Resources.clsServer_UnexpectedNumberOfRecordsUpdated0, affected)

            End If

            If wq.IsActive Then

                If wq.ProcessId <> Guid.Empty Then
                    wq.ProcessName = GetProcessNameById(con, wq.ProcessId)
                    wq.IsAssignedProcessHidden = Not New MemberPermissions(GetEffectiveGroupPermissionsForProcess(con, wq.ProcessId)).HasAnyPermissions(mLoggedInUser)
                End If

                If wq.ResourceGroupId <> Guid.Empty Then
                    Dim groupPermissions = GetEffectiveGroupPermissions(con, wq.ResourceGroupId)
                    Dim hasPermission = groupPermissions.HasPermission(mLoggedInUser, Permission.Resources.ImpliedViewResource)
                    wq.IsResourceGroupHidden = Not hasPermission
                    wq.ResourceGroupName = GetGroup(wq.ResourceGroupId).Name
                End If

            End If
            ' Ensure the encryption key is marked as the 'original' one in the
            ' queue object, now that that value is stored on the database.
            wq.CommitEncryptionKey()

            AuditRecordWorkQueueEvent(WorkQueueEventCode.ModifyQueue, wq.Id, wq.Name,
                                          String.Format(My.Resources.clsServer_WorkQueues_QueueIDIs0NewConfig12345, wq.Id, wq.Name, wq.KeyField, wq.MaxAttempts, IIf(wq.IsEncrypted, My.Resources.clsServer_WorkQueues_Encrypted, My.Resources.clsServer_WorkQueues_NotEncrypted),
                                                        wq.EncryptionKeyID))

        Catch sqlEx As SqlException When sqlEx.Number = DatabaseErrorCode.UniqueConstraintError
            'Assume uniqueness constraint on name column violated
            Throw New NameAlreadyExistsException(
             My.Resources.clsServer_CannotRenameQueueAnotherQueueWithTheName0AlreadyExists,
             wq.Name)

        End Try

    End Sub

    ''' <summary>
    ''' Unlocks an individual Work Queue Item from a Business Object.
    ''' </summary>
    ''' <param name="id">The ID of the item to unlock</param>
    ''' <returns>True if the queue item was unlocked as a result of this call;
    ''' false if it had already been unlocked</returns>
    <SecuredMethod()>
    Public Function WorkQueueUnlockItem(id As Guid) As Boolean Implements IServer.WorkQueueUnlockItem
        CheckPermissions()
        Using con = GetConnection()
            Return UnlockCases(con, GetSingleton.ICollection(id)) = 1
        End Using
    End Function

    ''' <summary>
    ''' Manually unlocks an individual Work Queue Item.
    ''' </summary>
    ''' <param name="workQueueItem">The Work Queue Item to unlock</param>
    ''' <param name="queueId">The ID of the Work Queue that stores the Work Queue Item to unlock</param>
    ''' <returns>True if the queue item was unlocked as a result of this call;
    ''' false if it had already been unlocked</returns>
    <SecuredMethod()>
    Public Function WorkQueueUnlockItem(workQueueItem As clsWorkQueueItem, queueId As Guid) As Boolean Implements IServer.WorkQueueUnlockItem
        CheckPermissions()
        Using con = GetConnection()
            Dim isUnlocked = UnlockCases(con, GetSingleton.ICollection(workQueueItem.ID)) = 1
            If isUnlocked Then AuditRecordWorkQueueEvent(WorkQueueEventCode.ManualUnlock, queueId, "",
                                                         String.Format(My.Resources.clsServer_WorkQueues_ItemKeyIs0QueueIDIs1AndItemIDIs2,
                                                                       workQueueItem.KeyValue, queueId, workQueueItem.ID))
            Return isUnlocked
        End Using
    End Function

    ''' <summary>
    ''' Check if an item with the given key already exists in the queue.
    ''' </summary>
    ''' <param name="queuename">The name of the queue to check</param>
    ''' <param name="key">The key to check</param>
    ''' <param name="pending">True to search any currently pending items (including
    ''' locked items and deferred items with a deferral date that has passed); False
    ''' to exclude them from the search.</param>
    ''' <param name="deferred">True to search any currently deferred items (with a
    ''' deferral date in the future); False to exclude them from the search.</param>
    ''' <param name="completed">True to search any completed items; False to exclude
    ''' them from the search.</param>
    ''' <param name="terminated">True to search any exceptioned items; False to
    ''' exclude them from the search.</param>
    ''' <returns>On return, True an item exists with the given key or False otherwise
    ''' </returns>
    ''' <exception cref="Exception">If any errors occur while checking the database.
    ''' </exception>
    <SecuredMethod()>
    Public Function WorkQueueIsItemInQueue(
     ByVal queuename As String,
     ByVal key As String,
     ByVal pending As Boolean,
     ByVal deferred As Boolean,
     ByVal completed As Boolean,
     ByVal terminated As Boolean) As ICollection(Of Guid) Implements IServer.WorkQueueIsItemInQueue
        CheckPermissions()
        Using con = GetConnection()

            GetWorkQueueIdent(con, queuename)

            Dim sb As New StringBuilder(
             " declare @now datetime;" &
             " set @now = getutcdate();" & _
 _
             " select i.id" &
             " from BPVWorkQueueItem i" &
             "   join BPAWorkQueue q on i.queueident = q.ident" &
             "   left join BPAWorkQueueItem inext on i.id = inext.id and inext.attempt = i.attempt + 1" &
             " where q.name = @queuename" &
             "   and inext.id is null" &
             "   and i.keyvalue = @key ")

            If pending OrElse deferred OrElse completed OrElse terminated Then
                sb.Append(" and i.state in (")
                If pending Then sb.Append("@pending,@locked,")
                If deferred Then sb.Append("@deferred,")
                If completed Then sb.Append("@completed,")
                If terminated Then sb.Append("@exceptioned,")
                ' Remove the last comma
                sb.Length -= 1
                sb.Append(")")
            Else
                sb.Append(" and i.state not in(@pending, @locked, @deferred, @completed, @exceptioned)")

            End If

            Dim cmd As New SqlCommand(sb.ToString())
            With cmd.Parameters
                .AddWithValue("@key", key)
                .AddWithValue("@queuename", queuename)
                .AddWithValue("@pending", clsWorkQueueItem.State.Pending)
                .AddWithValue("@locked", clsWorkQueueItem.State.Locked)
                .AddWithValue("@deferred", clsWorkQueueItem.State.Deferred)
                .AddWithValue("@completed", clsWorkQueueItem.State.Completed)
                .AddWithValue("@exceptioned", clsWorkQueueItem.State.Exceptioned)
            End With

            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                Dim ids As New clsSet(Of Guid)
                While reader.Read()
                    ids.Add(prov.GetValue("id", Guid.Empty))
                End While
                Return ids
            End Using

        End Using

    End Function

#Region "GetItem(s) (various overloads - single vs multiple, by ID vs by IDENT)"

    ''' <summary>
    ''' Gets the latest work queue item instance from the database which has the
    ''' specified ID, or gets nothing if no item instance with that ID was found.
    ''' </summary>
    ''' <param name="id">The item ID of the work queue item which is required.
    ''' </param>
    ''' <returns>The work queue item object representing the latest retry instance
    ''' of the queue item with the specified ID, or null if no such ID was found
    ''' on the database.
    ''' </returns>
    <SecuredMethod()>
    Public Function WorkQueueGetItem(ByVal id As Guid) As clsWorkQueueItem Implements IServer.WorkQueueGetItem
        CheckPermissions()
        Return WorkQueueGetItems({id}).FirstOrDefault()
    End Function

    ''' <summary>
    ''' Gets the latest work queue item instance from the database which has the
    ''' specified ID, or gets nothing if no item instance with that ID was found.
    ''' </summary>
    ''' <param name="id">The item ID of the work queue item which is required.
    ''' </param>
    ''' <returns>The work queue item object representing the latest retry instance
    ''' of the queue item with the specified ID, or null if no such ID was found
    ''' on the database.
    ''' </returns>
    Private Function WorkQueueGetItem(
     con As IDatabaseConnection, id As Guid) As clsWorkQueueItem
        Return WorkQueueGetItems(con, {id}).FirstOrDefault()
    End Function

    ''' <summary>
    ''' Gets the work queue item objects which are identified by the given
    ''' collection of IDs. This will retrieve from the database the latest work
    ''' queue item instance for each ID given, if such an item was found.
    ''' </summary>
    ''' <param name="ids">The IDs for the required work queue items.</param>
    ''' <returns>The collection of work queue items which represent the latest
    ''' retry of the items with the specified IDs.</returns>
    Private Function WorkQueueGetItems(ByVal ids As ICollection(Of Guid)) _
     As ICollection(Of clsWorkQueueItem)
        Using con = GetConnection()
            Return WorkQueueGetItems(con, ids)
        End Using
    End Function

    ''' <summary>
    ''' Gets the work queue item objects which are identified by the given
    ''' collection of IDs. This will retrieve from the database the latest work
    ''' queue item instance for each ID given, if such an item was found.
    ''' </summary>
    ''' <param name="con">The connection from which the work queue item data should
    ''' be retrieved.</param>
    ''' <param name="ids">The IDs for the required work queue items.</param>
    ''' <returns>The collection of work queue items which represent the latest
    ''' retry of the items with the specified IDs.</returns>
    Private Function WorkQueueGetItems(
     ByVal con As IDatabaseConnection, ByVal ids As ICollection(Of Guid)) _
     As ICollection(Of clsWorkQueueItem)

        Dim idents As New List(Of Long)
        Using cmd As New SqlCommand("select max(ident) from BPAWorkQueueItem where id = @id")
            cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier)
            For Each id As Guid In ids
                cmd.Parameters("@id").Value = id
                idents.Add(CLng(con.ExecuteReturnScalar(cmd)))
            Next
        End Using
        Return WorkQueueGetItems(con, idents)

    End Function

    ''' <summary>
    ''' Gets the work queue item objects which are identified by the given collection
    ''' of identities.
    ''' </summary>
    ''' <param name="con">The connection from which the work queue item data should
    ''' be retrieved.</param>
    ''' <param name="idents">The 'ident's of the work queue items required - this
    ''' is the table-wide unique identity of the work queue item retry instance,
    ''' which differs from the <em>globally</em> unique ID which is the item ID and
    ''' represents the queue item including all its retries.</param>
    ''' <returns>A collection of work queue items representing the data which
    ''' was held in the database against the specified identities.</returns>
    Private Function WorkQueueGetItems(
     ByVal con As IDatabaseConnection, ByVal idents As ICollection(Of Long)) _
     As ICollection(Of clsWorkQueueItem)

        ' Shortcut out if there's nothing there.
        If idents.Count = 0 Then Return New List(Of clsWorkQueueItem)

        ' Create the dictionary which we are going to use for the bulk of the work.
        ' We get multiple rows back - one for each tag, so we get or create the
        ' queue item in the dictionary, keyed on ident accordingly.
        Dim dict As New SortedDictionary(Of Long, clsWorkQueueItem)
        For Each ident As Long In idents
            dict(ident) = New clsWorkQueueItem(Nothing, ident)
        Next

        Dim cmd As New SqlCommand()
        Dim sb As New StringBuilder(
          " select " &
          "   i.encryptid," &
          "   i.id," &
          "   i.priority," &
          "   i.ident," &
          "   i.keyvalue," &
          "   i.status," &
          "   t.tag," &
          "   s.runningresourcename," &
          "   i.attempt," &
          "   i.loaded," &
          "   i.lastupdated," &
          "   i.worktime," &
          "   i.attemptworktime," &
          "   i.deferred," &
          "   cl.locktime," &
          "   i.completed," &
          "   i.exception," &
          "   i.exceptionreason," &
          "   i.data" &
          " from BPAWorkQueueItem i" &
          "   left join BPACaseLock cl on i.ident = cl.id" &
          "   left join BPAWorkQueueItemTag it on i.ident = it.queueitemident" &
          "   left join BPATag t on it.tagid = t.id" &
          "   left join BPVSessionInfo s on i.sessionid = s.sessionid" &
          " where i.ident in ("
         )

        Dim i As Integer = 0
        For Each ident As Long In dict.Keys
            i += 1
            sb.AppendFormat("@id{0},", i)
            cmd.Parameters.AddWithValue("@id" & i, ident)
        Next
        sb.Length -= 1
        sb.Append(")")

        cmd.CommandText = sb.ToString()

        Using reader = con.ExecuteReturnDataReader(cmd)

            Dim prov As New ReaderDataProvider(reader)
            While reader.Read()
                Dim item As clsWorkQueueItem = dict(prov.GetValue("ident", 0L))
                ' Set all the data if we've not already done so.
                ' "loaded" is a good indicator since (in theory) it always has a value
                ' No real harm done if the theory is wrong. It's just reducing unnecessary work
                If item.Loaded = Nothing Then
                    item.ID = prov.GetValue("id", Guid.Empty)
                    item.KeyValue = prov.GetValue("keyvalue", "")
                    item.Priority = prov.GetValue("priority", 0)
                    item.Status = prov.GetValue("status", "")
                    item.Resource = prov.GetString("runningresourcename")
                    item.Attempt = prov.GetValue("attempt", 0)
                    item.Loaded = prov.GetValue("loaded", Date.MinValue)
                    item.Deferred = prov.GetValue("deferred", Date.MinValue)
                    item.Locked = prov.GetValue("locked", Date.MinValue)
                    item.CompletedDate = prov.GetValue("completed", Date.MinValue)
                    item.Worktime = prov.GetValue("worktime", 0)
                    item.AttemptWorkTime = prov.GetValue("attemptworktime", 0)
                    item.ExceptionDate = prov.GetValue("exception", Date.MinValue)
                    item.ExceptionReason = prov.GetString("exceptionreason")
                    item.DataXml = Decrypt(prov.GetValue("encryptid", 0), prov.GetString("data"))
                End If
                Dim tag As String = prov.GetString("tag")
                If tag IsNot Nothing Then item.AddTag(tag)

            End While

        End Using

        ' Take the dictionary values and add them to a list.
        ' We could conceivably just return dict.Values here, but we'd be passing back
        ' a whole load of data which is useless to the calling code (ie. the keys).
        Return New List(Of clsWorkQueueItem)(dict.Values)

    End Function

#End Region

    ''' <summary>
    ''' Deletes an item from a queue.
    ''' </summary>
    ''' <param name="id">The ID of the work queue item to be deleted.</param>
    ''' <returns>True if successful, False otherwise</returns>
    <SecuredMethod()>
    Public Function WorkQueueDeleteItem(ByVal id As Guid) As Boolean Implements IServer.WorkQueueDeleteItem
        CheckPermissions()
        Try
            Using con = GetConnection()
                LogMIQueueItemDeletion(con, {id})

                Dim cmd As New SqlCommand(
                 " delete from BPAWorkQueueItem where id=@itemid"
                )
                cmd.Parameters.AddWithValue("@itemid", id)

                If con.ExecuteReturnRecordsAffected(cmd) > 0 Then
                    ' Log work queue item operation (if required)
                    WorkQueueLogAddEntry(con, WorkQueueOperation.ItemDeleted, id)
                    Return True
                End If

                Throw New Exception(My.Resources.clsServer_NoItemWasDeleted)
                Return False

            End Using

        Catch sqle As SqlException When sqle.Number = DatabaseErrorCode.ForeignKeyError
            Throw New Exception(My.Resources.clsServer_AnAttemptOfThisWorkItemIsCurrentlyLockedSoItCannotBeDeleted)
        Catch ex As Exception
            Throw ex
        End Try

    End Function

    ''' <summary>
    ''' Gets the pending items from a queue.
    ''' </summary>
    ''' <param name="name">The name of the queue.</param>
    ''' <param name="key">The keyvalue on which to filter the items.</param>
    ''' <param name="tags">The tag mask with which toe filter the items.</param>
    ''' <param name="max">The maximum number of rows to return.</param>
    ''' <param name="skip">The number of IDs to skip before starting to return values
    ''' </param>
    ''' <returns>A collection of GUIDs representing the IDs which need to be returned
    ''' for the specified queue.</returns>
    ''' <exception cref="SqlException">If any database errors occur while attempting
    ''' to get the pending items from the queue.</exception>
    <SecuredMethod()>
    Public Function WorkQueueGetPending(
     ByVal name As String,
     ByVal key As String,
     ByVal tags As clsTagMask,
     ByVal max As Integer, ByVal skip As Integer) As ICollection(Of Guid) Implements IServer.WorkQueueGetPending
        CheckPermissions()
        Using con = GetConnection()

            GetWorkQueueIdent(con, name)

            Dim cmd As New SqlCommand()

            Dim sb As New StringBuilder(String.Format(" select top {0} i.id" &
              " from BPAWorkQueueItem i" &
              "   join BPAWorkQueue q on q.ident = i.queueident" &
              "   left join BPACaseLock cl on i.ident = cl.id" &
              " where q.name = @queuename" &
              "   and cl.id is null" &
              "   and i.finished is null" &
              "   and (i.deferred is null or i.deferred <= getutcdate()) ",
              IIf(max > 0, (max + skip), "100 percent")
            ))
            cmd.Parameters.AddWithValue("@queuename", name)

            'Figure out a WHERE clause for filtering on the key value, if required...
            If key <> "" Then
                sb.Append(" and i.keyvalue = @keyfilter")
                cmd.Parameters.AddWithValue("@keyfilter", key)
            End If

            ' If we're applying / masking certain tags, add them to the WHERE clause too
            AddTagFilter(sb, tags, cmd, False)

            sb.Append(" order by i.priority,i.loaded,i.ident")

            cmd.CommandText = sb.ToString()

            Dim ids As New List(Of Guid)
            Using reader = con.ExecuteReturnDataReader(cmd)
                While reader.Read()
                    If skip > 0 Then ' Keep skipping til we've gone beyond the skip count..
                        skip -= 1
                    Else
                        ids.Add(reader.GetGuid(0))
                    End If
                End While
            End Using
            Return ids

        End Using

    End Function

    ''' <summary>
    ''' Gets the currently locked work queue items in the specified queue,
    ''' with tags corresponding to the given mask.
    ''' </summary>
    ''' <param name="queuename">The name of the queue for which the locked items
    ''' are required.</param>
    ''' <param name="tagMask">Only items which match the given tag mask will be
    ''' returned.</param>
    ''' <returns>A dictionary of lock times mapped against item IDs representing
    ''' the work queue items within the given queue which are currently locked.
    ''' The dictionary will be in ascending lock date order.
    ''' </returns>
    ''' <exception cref="NoSuchQueueException">If the given queue name did not
    ''' represent a queue on this system.</exception>
    <SecuredMethod()>
    Public Function WorkQueueGetLocked(
     ByVal queuename As String, ByVal keyFilter As String, ByVal tagMask As clsTagMask) _
     As IDictionary(Of Guid, Date) Implements IServer.WorkQueueGetLocked
        CheckPermissions()
        Using con = GetConnection()

            Dim queueIdent As Integer = GetWorkQueueIdent(con, queuename)

            Dim cmd As New SqlCommand()
            Dim sb As New StringBuilder(" select i.id, cl.locktime " &
             " from BPAWorkQueueItem i" &
             "   join BPACaseLock cl on i.ident = cl.id" &
             " where i.queueident = @queueident"
            )
            cmd.Parameters.AddWithValue("@queueident", queueIdent)
            If keyFilter <> "" Then
                sb.Append(
                 "   and i.keyvalue = @keyfilter"
                )
                cmd.Parameters.AddWithValue("@keyfilter", keyFilter)
            End If

            AddTagFilter(sb, tagMask, cmd, False)

            sb.Append(" order by cl.locktime")
            cmd.CommandText = sb.ToString()

            Dim dict As New clsOrderedDictionary(Of Guid, Date)
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    dict(prov.GetValue("id", Guid.Empty)) =
                     prov.GetValue("locktime", Date.MinValue)
                End While
            End Using
            Return dict

        End Using

    End Function

    ''' <summary>
    ''' Gets completed items which were marked as such within the given date range.
    ''' </summary>
    ''' <param name="queuename">The name of the queue to get items from</param>
    ''' <param name="startDate">The start threshold date to use. Items marked
    ''' as completed before this date will be ignored. Pass DateTime.MinValue for
    ''' no threshold</param>
    ''' <param name="endDate">The end threshold date to use. Items marked as
    ''' complete after this date will be ignored. Pass DateTime.MaxValue for no
    ''' threshold</param>
    ''' <param name="max">Indicates the maximum number of rows to return. Pass
    ''' zero for an unlimited number of rows.</param>
    ''' <returns>The collection of Item IDs which were completed within the given
    ''' search constraints.</returns>
    <SecuredMethod()>
    Public Function WorkQueueGetCompleted(
     ByVal queuename As String,
     ByVal startDate As DateTime,
     ByVal endDate As DateTime,
     ByVal keyFilter As String,
     ByVal tags As clsTagMask,
     ByVal max As Integer) As ICollection(Of Guid) Implements IServer.WorkQueueGetCompleted
        CheckPermissions()
        Using con = GetConnection()
            ' Get the actioned items - we don't need to enforce the latest attempt
            ' since there cannot be any more attempts after an item has completed
            Return WorkQueueGetActionedItems(con, "completed", False,
             queuename, startDate, endDate, keyFilter, tags, max)
        End Using

    End Function

    ''' <summary>
    ''' Gets exception items which were marked as such within the given date range.
    ''' </summary>
    ''' <param name="queuename">The name of the queue to get items from</param>
    ''' <param name="startDate">The start threshold date to use. Items marked
    ''' as an exception before this date will be ignored. Pass DateTime.MinValue for
    ''' no threshold</param>
    ''' <param name="endDate">The end threshold date to use. Items marked as
    ''' an exception after this date will be ignored. Pass DateTime.MaxValue for no
    ''' threshold</param>
    ''' <param name="maxrows">Indicates the maximum number of rows to return. Pass
    ''' zero for an unlimited number of rows.</param>
    ''' <param name="resourceName">Filters items by the resource that worked on them.
    ''' </param>
    ''' <returns>A collection of GUIDs representing the IDs of the work queue items
    ''' which were (finally, ie. without a retry) marked with an exception between
    ''' the given dates.</returns>
    <SecuredMethod()>
    Public Function WorkQueueGetExceptions(
     ByVal queuename As String,
     ByVal startDate As DateTime,
     ByVal endDate As DateTime,
     ByVal keyFilter As String,
     ByVal tags As clsTagMask,
     ByVal maxrows As Integer,
     ByVal resourceName As String) As ICollection(Of Guid) Implements IServer.WorkQueueGetExceptions
        CheckPermissions()
        Using con = GetConnection()
            Return WorkQueueGetActionedItems(con, "exception", True, queuename, startDate, endDate, keyFilter, tags, maxrows, resourceName)
        End Using
    End Function

    ''' <summary>
    ''' Gets the Item IDs of the work items which have been actioned in some way
    ''' which leaves a date/time value in a field in the queue item table.
    ''' The two typical uses for this are getting items which were marked as
    ''' complete (fieldName:="completed") and marked as exception ("exception").
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="fieldName">The name of the field to check</param>
    ''' <param name="queuename">The name of the queue that should be searched
    ''' </param>
    ''' <param name="startDate">The start threshold date to use. Items actioned
    ''' before this date will be ignored</param>
    ''' <param name="endDate">The end threshold date to use. Items actioned
    ''' after this date will be ignored</param>
    ''' <param name="keyFilter">The key for the item(s) which should be searched
    ''' </param>
    ''' <param name="tags">The tag mask filtering the items which should be searched
    ''' </param>
    ''' <param name="maxrows">The maximum number of item IDs to return. If zero,
    ''' this will return all item IDs found.</param>
    ''' <param name="resourceName">Filters items by the resource that worked on them.
    ''' </param>
    ''' <returns>The collection of item IDs matching the specified constraints</returns>
    Private Function WorkQueueGetActionedItems(
     con As IDatabaseConnection,
     fieldName As String,
     ensureLatestAttempt As Boolean,
     queuename As String,
     startDate As DateTime,
     endDate As DateTime,
     keyFilter As String,
     tags As clsTagMask,
     maxrows As Integer,
     Optional resourceName As String = Nothing
     ) As ICollection(Of Guid)

        Dim queueIdent As Integer = GetWorkQueueIdent(con, queuename)

        Dim cmd As New SqlCommand()
        Dim sb As New StringBuilder()

        ' Build up the query - there's quite a few options, so it's a bit convoluted
        sb.AppendFormat(
         " select {0} i.id" &
         " from BPAWorkQueueItem i" &
         "   {1}{2}" &
         " where " &
         "   i.queueident = @queueident" &
         "   {3}" &
         "   and i.{4} between @startdate and @enddate" &
         "   {5}",
         If(maxrows > 0, "top " & maxrows, ""), '{0}
         If(ensureLatestAttempt, '{1}
          "   left join BPAWorkQueueItem inext on " &
          "     i.id = inext.id and inext.attempt = i.attempt + 1", ""),
         If(Not String.IsNullOrEmpty(resourceName), $" inner join ( select s.sessionid from BPASession s " &
         "inner join BPAResource r on s.starterresourceid = r.resourceid and r.name = @resourcename ) r on i.sessionid = r.sessionid", ""), '{2}
         If(ensureLatestAttempt, "and inext.id is null", ""),'{3}
         fieldName,'{4}
         If(keyFilter <> "", "and i.keyvalue = @keyvalue", "")'{5}
             )


        ' If we're applying / masking certain tags, add them to the WHERE clause too
        AddTagFilter(sb, tags, cmd, True)

        ' Order clause comes last
        sb.Append(" order by i.loaded")

        cmd.CommandText = sb.ToString()

        With cmd.Parameters
            .AddWithValue("@queueident", queueIdent)
            .AddWithValue("@startdate", clsDBConnection.UtilDateToSqlDate(startDate))
            .AddWithValue("@enddate", clsDBConnection.UtilDateToSqlDate(endDate))
            If keyFilter <> "" Then .AddWithValue("@keyvalue", keyFilter)
            If Not String.IsNullOrEmpty(resourceName) Then .AddWithValue("@resourceName", resourceName)
        End With

        Dim list As New List(Of Guid)
        Using reader = con.ExecuteReturnDataReader(cmd)
            While reader.Read()
                list.Add(reader.GetGuid(0))
            End While
        End Using
        Return list

    End Function

    ''' <summary>
    ''' Clears worked Work Queue items with the IDs specified from the queue
    ''' specified. If neither a queue ID or a list of item IDs are given, this will
    ''' clear all worked items from all queues.
    ''' </summary>
    ''' <param name="queueId">The ID of the queue from which the items should be
    ''' cleared. If no ID is given, this will clear worked items from all queues.
    ''' </param>
    ''' <param name="selectedQueueItems">The items to clear. If null, all worked
    ''' items in the specified queue(s) will be cleared. An empty collection will
    ''' result in no items being cleared.
    ''' </param>
    ''' <returns>Number of deleted items</returns>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Function WorkQueueClearWorked(
     queueId As Guid,
     selectedQueueItems As IList(Of clsWorkQueueItem),
     extraInformation As String,
     manualChange As Boolean) As Integer Implements IServer.WorkQueueClearWorked
        CheckPermissions()

        Dim itemIDs = selectedQueueItems.Select(Function(i) i.ID)
        If itemIDs.Count = 0 Then
            Return 0
        End If

        Using con = GetConnection()

            ' only delete items which are worked means *items* not instances -
            ' thus, we keep any items for which there are still pending instances
            ' First, we get the affected IDs into a collection, primarily to
            ' allow for the work queue log entries to be written.
            Dim cmd As New SqlCommand()

            Dim sb As New StringBuilder()
            sb.AppendFormat(
             " select i.id" &
             " from BPAWorkQueueItem i" &
             "   left join BPAWorkQueueItem inext " &
             "     on i.id = inext.id and inext.attempt = i.attempt + 1" &
             " where inext.id is null" &
             "   and i.finished is not null" &
             "   {0}",
             IIf(queueId = Nothing, "", "and i.queueid = @queueId")
            )

            ' Let's get the IDs which we need to delete
            Dim deleteIds As New List(Of Guid)

            ' If we have no specific item Ids, we can just go ahead and run this
            ' as is, otherwise we need to specify the IDs in the query.
            If itemIDs IsNot Nothing Then
                sb.Append(" and i.id in (")
                Dim i As Integer = 1
                For Each id As Guid In itemIDs
                    sb.AppendFormat("@id{0},", i)
                    cmd.Parameters.AddWithValue("@id" & i, id)
                    i += 1
                Next
                sb.Length -= 1  'deletes last comma
                sb.Append(")"c)
            End If

            cmd.CommandText = sb.ToString()
            If queueId <> Nothing Then _
             cmd.Parameters.AddWithValue("@queueId", queueId)

            ' Make sure that we have plenty of time to do it - deleting items can
            ' take quite a while - 10 minutes really should suffice
            cmd.CommandTimeout = Options.Instance.SqlCommandTimeoutLong

            Using reader = con.ExecuteReturnDataReader(cmd)
                While reader.Read()
                    deleteIds.Add(DirectCast(reader("id"), Guid))
                End While
            End Using

            ' Set the ByRef deleted count
            Dim deleted As Integer = deleteIds.Count

            'remove to list
            deleteIds.
             Select(Function(x, i) New With {Key .Index = i, Key .Value = x}).
             GroupBy(Function(x) (x.Index \ 1000)).
             Select(Function(x) x.Select(Function(v) v.Value).ToList()).
             ForEach(Sub(deleteIdsGroup)
                         con.BeginTransaction()
                         Try
                             LogMIQueueItemDeletion(con, deleteIdsGroup)
                             UpdateMultipleIds(con, New SqlCommand(), deleteIdsGroup, Nothing,
                               "delete from BPAWorkQueueItem where id in (")

                             If Not manualChange Then
                                 ' Log work queue item operation (if required)
                                 WorkQueueLogAddEntries(con, WorkQueueOperation.ItemDeleted, deleteIdsGroup)
                             Else
                                 If deleteIdsGroup.Count = 1 Then
                                     Dim comment = String.Format(My.Resources.clsServer_QueueID0ItemID1,
                                                 queueId, deleteIdsGroup.First) & extraInformation
                                     AuditManualQueueChangeEvent(WorkQueueEventCode.DeleteProcessFromQueue, queueId, comment)
                                 Else
                                     For Each comment In GetMultipleIdsComments(My.Resources.clsServer_MultipleItemsCleared, deleteIdsGroup, queueId)
                                         AuditManualQueueChangeEvent(WorkQueueEventCode.DeleteProcessFromQueue, queueId, comment & extraInformation)
                                     Next
                                 End If
                             End If
                             con.CommitTransaction()
                         Catch
                             con.RollbackTransaction()
                             Throw
                         End Try
                     End Sub).
             Evaluate()

            Return deleted

        End Using

    End Function

    ''' <summary>
    ''' Clears all worked items from a queue.
    ''' </summary>
    ''' <param name="queueID">The ID of the queue to be updated.</param>
    ''' <returns>The number of deleted items</returns>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Function WorkQueueClearAllWorked(ByVal queueID As Guid) As Integer Implements IServer.WorkQueueClearAllWorked
        CheckPermissions()
        Return WorkQueueClearWorked(queueID, Nothing, "", False)
    End Function

    ''' <summary>
    ''' Clears worked Work Queue items, which were processed before the supplied
    ''' date.
    ''' </summary>
    ''' <param name="queueName">The name of the queue for which the items should
    ''' be cleared - null or empty to clear from all queues.</param>
    ''' <param name="thresholdDate">The threshold date to use. Items completed or
    ''' marked as an exception before this date will be deleted.</param>
    ''' <returns>The number of items deleted.</returns>
    ''' <exception cref="NoSuchQueueException">If the specified queue does not exist
    ''' in the current environment</exception>
    ''' <exception cref="Exception">If the function fails for any other reason.
    ''' </exception>
    <SecuredMethod()>
    Public Function WorkQueueClearWorkedByDate(
     queueName As String,
     thresholdDate As DateTime,
     allQueues As Boolean) As Integer Implements IServer.WorkQueueClearWorkedByDate
        CheckPermissions()
        Using connection = GetConnection()

            If allQueues.Equals(False) AndAlso String.IsNullOrEmpty(queueName) Then
                Throw New InvalidArgumentException(My.Resources.clsServer_DeleteAllWorkQueueParametersNoQueueConflict)
            ElseIf allQueues.Equals(True) AndAlso Not String.IsNullOrEmpty(queueName) Then
                Throw New InvalidArgumentException(My.Resources.clsServer_DeleteAllWorkQueueParametersIncludedQueueNameConflict)
            End If

            ' First get the queue ID (if one is specified)
            Dim queueIdent As Integer = 0
            If Not String.IsNullOrEmpty(queueName) Then

                Using queueCommand As New SqlCommand()
                    ' Get the ID of the specified queue - fail if it doesn't exist
                    queueCommand.CommandText = "select q.ident from BPAWorkQueue q where q.name = @name"
                    queueCommand.Parameters.AddWithValue("@name", queueName)
                    queueIdent = IfNull(connection.ExecuteReturnScalar(queueCommand), 0)

                    ' If we have no queue ID, we have no queue
                    If queueIdent = 0 Then
                        Throw New NoSuchQueueException(queueName, CultureInfo.CreateSpecificCulture(mLoggedInUserLocale))
                    End If
                End Using
            End If

            Dim deleteIds As New List(Of Guid)

            ' Get the IDs which we need to delete
            Using itemCommand As New SqlCommand()
                itemCommand.CommandText = String.Format(
                 " select i.id" &
                 " from BPAWorkQueueItem i" &
                 "   left join BPAWorkQueueItem inext " &
                 "     on i.id = inext.id and inext.attempt = i.attempt + 1" &
                 " where inext.id is null" &
                 "   and i.finished <= @threshold" &
                 "   {0}",
                 IIf(queueIdent = 0, "", "and i.queueident = @queueident"))

                With itemCommand.Parameters
                    .AddWithValue("@threshold", clsDBConnection.UtilDateToSqlDate(thresholdDate))
                    If queueIdent <> 0 Then .AddWithValue("@queueident", queueIdent)
                End With

                ' Make sure that we have plenty of time to do it - deleting items can
                ' take quite a while - 10 minutes really should suffice
                itemCommand.CommandTimeout = Options.Instance.SqlCommandTimeoutLong
                Using reader = connection.ExecuteReturnDataReader(itemCommand)
                    While reader.Read()
                        deleteIds.Add(DirectCast(reader("id"), Guid))
                    End While
                End Using
            End Using

            ' Set the ByRef deleted count
            Dim deleted As Integer = deleteIds.Count

            'remove to list
            Using deleteCommand As New SqlCommand()
                deleteIds.
                 Select(Function(x, i) New With {Key .Index = i, Key .Value = x}).
                 GroupBy(Function(x) (x.Index \ 1000)).
                 Select(Function(x) x.Select(Function(v) v.Value).ToList()).
                 ForEach(Sub(deleteIdsGroup)
                             connection.BeginTransaction()
                             Try
                                 LogMIQueueItemDeletion(connection, deleteIdsGroup)
                                 UpdateMultipleIds(connection, deleteCommand, deleteIdsGroup, Nothing,
                               "delete from BPAWorkQueueItem where id in (")

                                 ' Log work queue item operation (if required)
                                 WorkQueueLogAddEntries(connection, WorkQueueOperation.ItemDeleted, deleteIdsGroup)
                                 connection.CommitTransaction()
                             Catch
                                 connection.RollbackTransaction()
                                 Throw
                             End Try
                         End Sub).
                 Evaluate()
            End Using

            Return deleted

        End Using
    End Function

    ''' <summary>
    ''' Identical to <see cref="WorkQueueGetQueue(Guid)"/> other than it does NOT make a call to CheckPermissions()
    ''' </summary>
    Private Function InternalGetQueue(id As Guid) _
        As WorkQueueWithGroup
        Using con = GetConnection()
            Return GetQueueById(con, id)
        End Using
    End Function

    <SecuredMethod()>
    Public Function WorkQueueGetQueue(ByVal id As Guid) _
     As WorkQueueWithGroup Implements IServer.WorkQueueGetQueue
        CheckPermissions()
        Using con = GetConnection()
            Return GetQueueById(con, id)
        End Using
    End Function

    <SecuredMethod()>
    Public Function WorkQueueGetQueue(ByVal ident As Integer) _
     As clsWorkQueue Implements IServer.WorkQueueGetQueue
        CheckPermissions()
        Using con = GetConnection()
            Return GetQueueByIdent(con, ident)
        End Using
    End Function

    <SecuredMethod()>
    Public Function WorkQueueGetQueue(ByVal name As String) _
     As clsWorkQueue Implements IServer.WorkQueueGetQueue
        CheckPermissions()
        Using con = GetConnection()
            Return GetQueueByName(con, name)
        End Using
    End Function

    ''' <summary>
    ''' Gets the list of work queues currently registered within the system,
    ''' not including any summary / statistical data.
    ''' </summary>
    ''' <returns>The list of work queues populated by the basic data of the
    ''' work queue without any summary of their owned items.</returns>
    ''' <exception cref="SqlException">If any database errors occur while attempting
    ''' retrieve the work queues.</exception>
    <SecuredMethod()>
    Public Function WorkQueueGetAllQueues() As IList(Of clsWorkQueue) Implements IServer.WorkQueueGetAllQueues
        CheckPermissions()
        Return WorkQueueGetQueues(False)
    End Function

    ''' <summary>
    ''' Gets the list of sorted work queues currently registered within the system,
    ''' not including any summary / statistical data.
    ''' </summary>
    ''' <param name="workQueueParameters">The parameters to sort and order by.</param>
    ''' <returns>The list of work queues populated by the basic data of the
    ''' work queue without any summary of their owned items.</returns>
    ''' <exception cref="SqlClient.SqlException">If any database errors occur while attempting
    ''' retrieve the work queues.</exception>
    <SecuredMethod()>
    Public Function WorkQueueGetQueues(workQueueParameters As WorkQueueParameters) As IList(Of WorkQueueWithGroup) Implements IServer.WorkQueueGetQueues
        CheckPermissions()
        Return GetQueuesFilteredAndOrdered(workQueueParameters)
    End Function

    <SecuredMethod()>
    Public Function WorkQueueGetAllQueueNames() As IBPSet(Of String) Implements IServer.WorkQueueGetAllQueueNames
        CheckPermissions()

        Dim names As New clsSortedSet(Of String)
        Using con = GetConnection()
            Using cmd As New SqlCommand("select [name] from BPAWorkQueue")
                Using dr = con.ExecuteReturnDataReader(cmd)
                    Dim prov = New ReaderDataProvider(dr)

                    While dr.Read()
                        names.Add(prov.GetString("name"))
                    End While
                End Using
            End Using
        End Using
        Return names
    End Function

    ''' <summary>
    ''' Gets the list of work queues currently registered within the system,
    ''' including or omitting summary statistical data regarding the items in the
    ''' queues, depending on the specified flag.
    ''' </summary>
    ''' <param name="includeStats">True to include item statistics in the returned
    ''' objects; False to only return the basic information regarding the queues.
    ''' </param>
    ''' <returns>The list of work queue objects in the system.</returns>
    ''' <exception cref="SqlException">If any database errors occur while attempting
    ''' retrieve the work queues.</exception>
    Private Function WorkQueueGetQueues(ByVal includeStats As Boolean) As IList(Of clsWorkQueue)

        Using con = GetConnection()
            Try
                con.BeginTransaction(IsolationLevel.Snapshot)

                Dim result As IList(Of clsWorkQueue) = Nothing
                If includeStats Then
                    result = GetQueuesAndStats(con, Nothing)
                Else
                    result = GetQueuesWithoutStats(con)
                End If

                con.CommitTransaction()
                Return result
            Catch ex As Exception
                'If this fails before the transaction commits then roll it back here
                If con.InTransaction Then
                    'Rolling back is appropriate here as it is just a SELECT, we do not want to rethrow as it would not restore the connection to Read Committed
                    con.RollbackTransaction()
                End If
                'Rethrow the exception so it is handled normally
                Throw ex
            Finally
                con.ResetConnectionDefaultIsolationLevel()
            End Try
        End Using
    End Function

    ''' <summary>
    ''' Gets the work queues corresponding to a set of identities.
    ''' </summary>
    ''' <param name="filter">The collection of identities of the required queues.
    ''' A null value will return all queues; an empty collection will return no
    ''' queues at all.</param>
    ''' <returns>A list of work queue objects representing the required queues.
    ''' </returns>
    <SecuredMethod()>
    Public Function WorkQueueGetQueuesFiltered(
     filter As ICollection(Of Integer)) As IList(Of clsWorkQueue) Implements IServer.WorkQueueGetQueuesFiltered
        CheckPermissions()
        Using con = GetConnection()
            Return GetQueuesAndStats(con, filter)
        End Using
    End Function

    ''' <summary>
    ''' Gets the work queue data using the given connection and SQL query.
    ''' </summary>
    ''' <param name="con">The connection over which the data should be retrieved.
    ''' </param>
    ''' <param name="sql">The query to return the work queue data. This should return
    ''' columns with the names specified in the <see cref="clsWorkQueue"/>
    ''' constructor.
    ''' </param>
    ''' <param name="params">The dictionary of parameters and their values.</param>
    ''' <returns>The list of work queues in the system as provided by the given
    ''' SQL query text.</returns>
    Private Function GetQueues(
                              ByVal con As IDatabaseConnection,
                              command As IDbCommand) _
     As IList(Of clsWorkQueue)

        ' On large queues (>2m records) this can be pretty slow...
        command.CommandTimeout = Options.Instance.SqlCommandTimeoutLong

        Dim list As New List(Of clsWorkQueue)
        Using reader = con.ExecuteReturnDataReader(command)
            Dim prov As New ReaderDataProvider(reader)
            Dim gpStore = mDependencyResolver.Resolve(Of IGroupStore)()
            While reader.Read()
                list.Add(New clsWorkQueue(prov) With {.GroupStore = gpStore})
            End While
        End Using

        ' We also need to get the active data for these queues if they are active
        For Each q As clsWorkQueue In list
            UpdateActiveQueueData(con, q)
        Next

        ' Set flags if queue has assigned process / resource group but user cannot see them
        For Each q As clsWorkQueue In list

            q.IsAssignedProcessHidden = q.ProcessId <> Guid.Empty AndAlso
                Not New MemberPermissions(GetEffectiveGroupPermissionsForProcess(con, q.ProcessId)).HasAnyPermissions(mLoggedInUser)

            Dim groupPermissions = GetEffectiveGroupPermissions(q.ResourceGroupId)
            Dim hasPermission = groupPermissions.HasPermission(mLoggedInUser, Permission.Resources.ImpliedViewResource)

            q.IsResourceGroupHidden = q.ResourceGroupId <> Guid.Empty AndAlso
                Not hasPermission
        Next

        Return list

    End Function

    ''' <summary>
    ''' Gets all registered work queues, with no summary data populated
    ''' </summary>
    ''' <param name="con">The connection from which the queues should be retrieved
    ''' </param>
    ''' <returns>The work queues registered in this environment without any
    ''' of the item statistics.</returns>
    Private Function GetQueuesWithoutStats(ByVal con As IDatabaseConnection) _
     As IList(Of clsWorkQueue)
        Using command = mDatabaseCommandFactory(
         " select q.id," &
         "   q.ident," &
         "   q.name," &
         "   q.keyfield," &
         "   q.running," &
         "   q.maxattempts," &
         "   q.encryptid," &
         "   q.processid," &
         "   q.snapshotconfigurationid," &
         "   p.name as processname," &
         "   q.resourcegroupid," &
         "   g.name as resourcegroupname," &
         "   0 as total," &
         "   0 as completed," &
         "   0 as pending," &
         "   0 as exceptioned," &
         "   0 as totalworktime," &
         "   0.0 as averageworkedtime," &
         "   0 as locked" &
         " from BPAWorkQueue q " &
         " left join BPAProcess p on q.processid = p.processid " &
         " left join BPAGroup g on g.id = q.resourcegroupid " &
         " order by q.name")
            Return GetQueues(con, command)
        End Using
    End Function

    Private Function GetQueuesFilteredAndOrdered(workQueueParameters As WorkQueueParameters) _
        As IList(Of WorkQueueWithGroup)

        Dim result = New List(Of WorkQueueWithGroup)

        Using con = GetConnection()
            Using sqlCommand = New SqlCommand()

                Dim queryBuilder = New StringBuilder()
                queryBuilder.AppendLine(
                    $"select top ({workQueueParameters.ItemsPerPage})
	                q.id,
                    q.ident,
                    q.name,
                    q.keyfield,
                    q.running,
                    q.maxattempts,
                    q.encryptid,
                    q.processid,
                    q.snapshotconfigurationid,
                    (select [name] from [dbo].[BPAProcess] where q.processid = [BPAProcess].processid ) processname,
                    q.resourcegroupid,
                    (select name from [dbo].BPAGroup where BPAGroup.id = q.resourcegroupid) as resourcegroupname,
                    ISNULL(a.total, 0) as total,
                    ISNULL(a.completed, 0) as completed,
                    ISNULL(a.pending, 0) as pending,
                    ISNULL(a.exceptioned, 0) as exceptioned,
                    ISNULL(a.totalWorkTime, 0) as totalworktime,
                    ISNULL(a.averageWorktime, 0.0) as averageworkedtime,
                    ISNULL(a.locked, 0) as locked,
	                q.targetsessions,
	                g.groupname,
	                g.groupid
                    from BPAWorkQueue q
                    left join BPAWorkQueueItemAggregate a on a.queueIdent = q.ident
	                left join BPAGroupQueue gq on gq.memberid = q.ident
	                left join (select id as groupid, name as groupname from BPAGroup) g on g.groupid = gq.groupid")


                sqlCommand.CommandTimeout = Options.Instance.SqlCommandTimeoutLong

                Dim sqlData = workQueueParameters.
                     GetSqlWhereClauses(sqlCommand).
                      GetSqlWhereWithParametersStartingWithWhereKeyword()

                If sqlData.sqlWhereClause.Any Then
                    queryBuilder.AppendLine(sqlData.sqlWhereClause)
                    sqlCommand.Parameters.AddRange(sqlData.sqlParameters)
                End If

                queryBuilder.Append($" order by {workQueueParameters.GetSqlOrderByClauses()}")

                sqlCommand.CommandText = queryBuilder.ToString()


                Using reader = con.ExecuteReturnDataReader(sqlCommand)
                    Dim prov As New ReaderDataProvider(reader)
                    While reader.Read()
                        Dim workQueue = New WorkQueueWithGroup()
                        With workQueue
                            .Id = prov.GetGuid("id")
                            .Ident = prov.GetInt("ident")
                            .Name = prov.GetString("name")
                            .IsRunning = prov.GetValue("running", True)
                            .KeyField = prov.GetValue("keyfield", "")
                            .MaxAttempts = prov.GetInt("maxattempts", 3)
                            .EncryptionKeyId = prov.GetInt("encryptid")
                            .PendingItemCount = prov.GetInt("pending")
                            .CompletedItemCount = prov.GetInt("completed")
                            .ExceptionedItemCount = prov.GetInt("exceptioned")
                            .LockedItemCount = prov.GetInt("locked")
                            .TotalItemCount = prov.GetInt("total")
                            .AverageWorkTime = TimeSpan.FromSeconds(prov.GetValue("averageworkedtime", 0L))
                            .TotalCaseDuration = TimeSpan.FromSeconds(prov.GetValue("totalworktime", 0L))
                            .ProcessId = prov.GetGuid("processid")
                            .ResourceGroupId = prov.GetGuid("resourcegroupid")
                            .TargetSessionCount = prov.GetInt("targetsessions")
                            .GroupName = prov.GetString("groupname")
                            .GroupId = prov.GetGuid("groupid")
                        End With

                        result.Add(workQueue)
                    End While
                End Using
            End Using
        End Using

        Return result
    End Function

    ''' <summary>
    ''' Gets the registered work queue with the specified ID, or null if no queue
    ''' with the given ID was found.
    ''' </summary>
    ''' <param name="con">The database connection to use to access the database.
    ''' </param>
    ''' <param name="id">The ID of the required queue.</param>
    ''' <returns>A work queue object representing the queue with the given ID, or
    ''' null if no such queue was found. Note that the returned queue does not
    ''' include queue statistics.</returns>
    Private Function GetQueueById(ByVal con As IDatabaseConnection, ByVal id As Guid) As WorkQueueWithGroup
        ' Get the first queue returned with this SQL (or null if nothing was returned)
        Dim result As WorkQueueWithGroup = Nothing

        Using sqlCommand = New SqlCommand()

            Dim queryBuilder = New StringBuilder()
            queryBuilder.AppendLine(
                $"select top (1)
                q.id,
                q.ident,
                q.name,
                q.keyfield,
                q.running,
                q.maxattempts,
                q.encryptid,
                q.processid,
                q.snapshotconfigurationid,   
                q.resourcegroupid,
                isnull(a.total, 0) as total,
                isnull(a.completed, 0) as completed,
                isnull(a.pending, 0) as pending,
                isnull(a.exceptioned, 0) as exceptioned,
                isnull(a.totalWorkTime, 0) as totalworktime,
                isnull(a.averageWorktime, 0.0) as averageworkedtime,
                isnull(a.locked, 0) as locked,
                q.targetsessions,
                g.name as groupname,
                g.id as groupid,
                p.name as processname,
                g.name as resourcegroupname
                from BPAWorkQueue q
                left join BPAProcess p on q.processid = p.processid
                left join BPAWorkQueueItemAggregate a on a.queueIdent = q.ident
                left join BPAGroupQueue gq on gq.memberid = q.ident
                left join BPAGroup g on g.id = gq.groupid
                where q.id=@queueid")

            sqlCommand.CommandText = queryBuilder.ToString()

            sqlCommand.Parameters.AddWithValue("@queueid", id)

            Using reader = con.ExecuteReturnDataReader(sqlCommand)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    Dim workQueue = New WorkQueueWithGroup()
                    With workQueue
                        .Id = prov.GetGuid("id")
                        .Ident = prov.GetInt("ident")
                        .Name = prov.GetString("name")
                        .IsRunning = prov.GetValue("running", True)
                        .KeyField = prov.GetValue("keyfield", "")
                        .MaxAttempts = prov.GetInt("maxattempts", 3)
                        .EncryptionKeyId = prov.GetInt("encryptid")
                        .PendingItemCount = prov.GetInt("pending")
                        .CompletedItemCount = prov.GetInt("completed")
                        .ExceptionedItemCount = prov.GetInt("exceptioned")
                        .LockedItemCount = prov.GetInt("locked")
                        .TotalItemCount = prov.GetInt("total")
                        .AverageWorkTime = TimeSpan.FromSeconds(prov.GetValue("averageworkedtime", 0L))
                        .TotalCaseDuration = TimeSpan.FromSeconds(prov.GetValue("totalworktime", 0L))
                        .ProcessId = prov.GetGuid("processid")
                        .ResourceGroupId = prov.GetGuid("resourcegroupid")
                        .TargetSessionCount = prov.GetInt("targetsessions")
                        .GroupName = prov.GetString("groupname")
                        .GroupId = prov.GetGuid("groupid")
                    End With

                    result = workQueue
                End While
            End Using

            Return result
        End Using

    End Function

    ''' <summary>
    ''' Gets the registered work queue with the specified ID, or null if no queue
    ''' with the given ID was found.
    ''' </summary>
    ''' <param name="con">The database connection to use to access the database.
    ''' </param>
    ''' <param name="ident">The Ident of the required queue.</param>
    ''' <returns>A work queue object representing the queue with the given ID, or
    ''' null if no such queue was found. Note that the returned queue does not
    ''' include queue statistics.</returns>
    Private Function GetQueueByIdent(con As IDatabaseConnection, ident As Integer) As clsWorkQueue
        ' Get the first queue returned with this SQL (or null if nothing was returned)
        Using command = mDatabaseCommandFactory(
         " select q.id," &
         "   q.ident," &
         "   q.name," &
         "   q.keyfield," &
         "   q.running," &
         "   q.maxattempts," &
         "   q.encryptid," &
         "   q.processid," &
         "   p.name as processname," &
         "   q.snapshotconfigurationid," &
         "   q.resourcegroupid," &
         "   q.sessionexceptionretry," &
         "   g.name as resourcegroupname," &
         "   0 as total," &
         "   0 as completed," &
         "   0 as pending," &
         "   0 as exceptioned," &
         "   0 as totalworktime," &
         "   0.0 as averageworkedtime" &
         " from BPAWorkQueue q " &
         " left join BPAProcess p on q.processid = p.processid " &
         " left join BPAGroup g on g.id = q.resourcegroupid " &
         " where q.ident=@queueident")

            command.AddParameter("@queueident", ident)

            Return GetQueues(con, command).SingleOrDefault()
        End Using
    End Function

    ''' <summary>
    ''' Return the first queue found by the given name or null if no such queue is found
    ''' </summary>
    Private Function GetQueueByName(con As IDatabaseConnection, name As String) As clsWorkQueue

        Using command = mDatabaseCommandFactory(
         " select q.id," &
         "   q.ident," &
         "   q.name," &
         "   q.keyfield," &
         "   q.running," &
         "   q.maxattempts," &
         "   q.encryptid," &
         "   q.processid," &
         "   p.name as processname," &
         "   q.snapshotconfigurationid," &
         "   q.resourcegroupid," &
         "   g.name as resourcegroupname," &
         "   0 as total," &
         "   0 as completed," &
         "   0 as pending," &
         "   0 as exceptioned," &
         "   0 as totalworktime," &
         "   0.0 as averageworkedtime" &
         " from BPAWorkQueue q " &
         " left join BPAProcess p on q.processid = p.processid " &
         " left join BPAGroup g on g.id = q.resourcegroupid " &
         " where q.name=@queuename")

            command.AddParameter("@queuename", name)

            Return GetQueues(con, command).SingleOrDefault()

        End Using
    End Function

    ''' <summary>
    ''' Gets the statistics on the given work queues.
    ''' </summary>
    ''' <param name="queues">The work queues to update</param>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesReadOnly, Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Function GetQueueStatsList(
     queues As ICollection(Of clsWorkQueue)) As ICollection(Of clsWorkQueue) Implements IServer.GetQueueStatsList
        CheckPermissions()
        Using con = GetConnection()
            ' If an empty filter list is provided get all of the queues
            If queues Is Nothing Then queues = GetQueuesWithoutStats(con)
            If Not queues?.Any Then Return New List(Of clsWorkQueue)

            Dim map = queues.ToDictionary(Of Integer, clsWorkQueue)(Function(x) x.Ident, Function(y) y)

            Dim sb As New StringBuilder(200)
            sb.Append(
                "
                select
                queueIdent as ident,
                total,
                completed,
                pending,
                deferred,
                exceptioned,
                totalworktime,
                averageWorktime as averageworkedtime,
                locked
                from BPAWorkQueueItemAggregate
                "
                )

            Dim queueFilter = " where queueIdent in ({queue-idents}) "
            sb.Append(queueFilter.Replace("{queue-idents}",
                                          String.Join(",", map.Keys)))
            Dim cmd As New SqlCommand With {
                    .CommandTimeout = Options.Instance.SqlCommandTimeout,
                    .CommandText = sb.ToString()
                }

            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    Dim test As clsWorkQueue = Nothing
                    Dim unused = map.TryGetValue(prov.GetValue("ident", 0), test)
                    test?.SetStatistics(prov)
                End While
            End Using
            Return queues
        End Using
    End Function

    ''' <summary>
    ''' Updates the active work queue data for a queue, returning the updated queue
    ''' </summary>
    ''' <param name="q">The queue to update the active queue data on</param>
    ''' <returns>The queue object, with the active queue data updated.</returns>
    ''' <remarks>If the given queue is not an active queue (within the object - ie.
    ''' there is no checking done on the database), nothing will occur.</remarks>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesReadOnly, Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Function UpdateActiveQueueData(q As clsWorkQueue) As clsWorkQueue _
     Implements IServer.UpdateActiveQueueData
        CheckPermissions()
        Using con = GetConnection()
            UpdateActiveQueueData(con, q)
            Return q
        End Using
    End Function

    ''' <summary>
    ''' Updates the active queue data for the given queue.
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="q">The queue whose active data should be updated.</param>
    ''' <remarks>If the given queue is not an active queue (within the object - ie.
    ''' there is no checking done on the database), nothing will occur.</remarks>
    Private Sub UpdateActiveQueueData(con As IDatabaseConnection, q As clsWorkQueue)

        If Not q.IsActive Then Return

        ' Get the active sessions for the queue
        q.Sessions = GetSessionsForQueue(con, q.Ident)
        q.TargetSessionCount = GetTargetSessionCount(con, q.Ident)
        Dim gp = GetGroup(q.ResourceGroupId)
        Dim resources As New HashSet(Of Guid)
        gp.Scan(Of ResourceGroupMember)(
            Sub(m) If Not m.IsRetired Then resources.Add(m.IdAsGuid)
        )

    End Sub


    ''' <summary>
    ''' Gets all registered work queues along with some statistics about the items
    ''' within the queues.
    ''' </summary>
    ''' <param name="filter">A list of queue idents to limit the returned results
    ''' to. If a null list is given no filtering is performed, if an empty list is
    ''' given no results will be returned.</param>
    ''' <returns>The work queues with their associated summary data.</returns>
    Private Function GetQueuesAndStats(ByVal con As IDatabaseConnection, filter As ICollection(Of Integer)) As IList(Of clsWorkQueue)

        If filter?.Count = 0 Then
            'Return an empty result list when an empty filter was supplied.
            Return New List(Of clsWorkQueue)
        End If

        Dim template = "
WITH workedtimestats AS
(
    SELECT
        q.ident,
        AVG(CAST(i.attemptworktime AS FLOAT)) AS averageworkedtime
    FROM BPAWorkQueue q
    LEFT JOIN BPAWorkQueueItem i
        ON i.queueident = q.ident
    WHERE i.finished IS NOT NULL
    GROUP BY q.ident
)
SELECT
    mainstats.id,
    mainstats.ident,
    mainstats.name,
    mainstats.keyfield,
    mainstats.running,
    mainstats.maxattempts,
    mainstats.encryptid,
    mainstats.processid,
    mainstats.resourcegroupid,
    mainstats.total,
    mainstats.completed,
    mainstats.pending,
    mainstats.deferred,
    mainstats.exceptioned,
    mainstats.totalworktime,
    wts.averageworkedtime
FROM
(
    SELECT
        q.id,
        q.ident,
        q.name,
        q.keyfield,
        q.running,
        q.maxattempts,
        q.encryptid,
        q.processid,
        q.resourcegroupid,
        COUNT(i.ident) AS total,
        COUNT(i.completed) AS completed,
        COUNT(i.ident)-COUNT(i.finished) AS pending,
        COUNT (CASE WHEN i.deferred  IS NULL OR i.deferred < GETUTCDATE() THEN NULL ELSE 1 END) AS deferred,
        COUNT(i.exception) AS exceptioned,
        ISNULL(SUM(CONVERT(bigint,i.attemptworktime)),0) AS totalworktime
    FROM BPAWorkQueue q
    LEFT JOIN BPAWorkQueueItem i
        ON i.queueident = q.ident
    {0}
    GROUP BY
        q.id,
        q.ident,
        q.name,
        q.keyfield,
        q.running,
        q.maxattempts,
        q.encryptid,
        q.processid,
        q.resourcegroupid
) AS mainstats
LEFT JOIN workedtimestats AS wts
    ON mainstats.ident = wts.ident
ORDER BY mainstats.name;
"
        Dim where = ""
        Dim params As New Dictionary(Of String, Object)
        If filter IsNot Nothing Then
            Dim paramNames = filter.
                    Select(Function(ident, index) $"@ident{index}").
                    ToArray()
            where = $"where q.ident in ({String.Join(", ", paramNames)})"
            filter.
                ForEach(Sub(ident, index) params.Add(paramNames(index), ident)).
                Evaluate()
        End If

        Dim sql = String.Format(template, where)

        Using command = mDatabaseCommandFactory(sql)
            For Each param In params
                command.AddParameter(param.Key, param.Value)
            Next

            Return GetQueues(con, command)
        End Using
    End Function

    ''' <summary>
    ''' Gets the name of the queue with the supplied ID.
    ''' </summary>
    ''' <param name="queueID">The ID of the queue fo interest.</param>
    ''' <param name="queuename">Carries back the name of the queue, on successful
    ''' completion.</param>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesReadOnly, Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Sub WorkQueueGetQueueName(ByVal queueID As Guid, ByRef queuename As String) Implements IServer.WorkQueueGetQueueName
        CheckPermissions()
        Dim con = GetConnection()
        Dim cmd As New SqlCommand("SELECT [Name] FROM BPAWorkQueue WHERE [id]=@queueid")
        With cmd.Parameters
            .AddWithValue("@queueid", queueID)
        End With
        queuename = CStr(con.ExecuteReturnScalar(cmd))
        con.Close()
    End Sub

    ''' <summary>
    ''' Gets the id of the queue with the supplied name.
    ''' </summary>
    ''' <param name="queuename">The name of the queue fo interest.</param>
    ''' <param name="queueID">Carries back the id of the queue, on successful
    ''' completion.</param>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesReadOnly, Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Sub WorkQueueGetQueueID(ByVal queuename As String, ByRef queueID As Guid) Implements IServer.WorkQueueGetQueueID
        CheckPermissions()
        Dim con = GetConnection()
        Dim cmd As New SqlCommand("SELECT [id] FROM BPAWorkQueue WHERE [name]=@queuename")
        With cmd.Parameters
            .AddWithValue("@queuename", queuename)
        End With
        queueID = CType(con.ExecuteReturnScalar(cmd), Guid)
        con.Close()
    End Sub

    ''' <summary>
    ''' Toggles the running status of the queue with the given ID and returns the
    ''' new running status of the queue.
    ''' </summary>
    ''' <param name="queueId">The ID of the queue whose running status should be
    ''' toggled.</param>
    ''' <returns>True if the queue's running status was toggled such that it is now
    ''' in a running state; False if it was toggled and is now in a not running
    ''' state. If the queue ID was not recognised, this will do no work and return
    ''' false (a nonexistent queue is, by definition, not running)</returns>
    <SecuredMethod(Permission.SystemManager.Workflow.WorkQueueConfiguration)>
    Public Function ToggleQueueRunningStatus(ByVal queueId As Guid) As Boolean Implements IServer.ToggleQueueRunningStatus
        CheckPermissions()
        Using con = GetConnection()
            Return ToggleQueueRunningStatus(con, queueId)
        End Using
    End Function

    ''' <summary>
    ''' Toggles the running status of the queue with the given ID and returns the
    ''' new running status of the queue.
    ''' </summary>
    ''' <param name="con">The connection to the database over which the running
    ''' status of the queue should be toggled.</param>
    ''' <param name="queueId">The ID of the queue whose running status should be
    ''' toggled.</param>
    ''' <returns>True if the queue's running status was toggled such that it is now
    ''' in a running state; False if it was toggled and is now in a not running
    ''' state. If the queue ID was not recognised, this will do no work and return
    ''' false (a nonexistent queue is, by definition, not running)</returns>
    Private Function ToggleQueueRunningStatus(
     ByVal con As IDatabaseConnection, ByVal queueId As Guid) As Boolean
        Dim cmd As New SqlCommand(
         " update BPAWorkQueue set running = ~running where id = @id" &
         " select running from BPAWorkQueue where id = @id"
        )
        cmd.Parameters.AddWithValue("@id", queueId)
        Return IfNull(con.ExecuteReturnScalar(cmd), False)
    End Function

    ''' <summary>
    ''' Updates the running status of the queues referenced by the given IDs to the
    ''' specified status.
    ''' </summary>
    ''' <param name="ids">The IDs of the queues whose running status should be set.
    ''' </param>
    ''' <param name="running">True to enable the queues' running statuses, false to
    ''' disable them</param>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Sub SetQueueRunningStatus(ByVal ids As ICollection(Of Guid), ByVal running As Boolean) Implements IServer.SetQueueRunningStatus
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            ' It's rather a naive implementation, but it's going to be a max of
            ' double digits of queues, and it's better than getting a new connection
            ' each time (which is what was happening when the GUI was doing this)
            For Each id As Guid In ids
                SetQueueRunningStatus(id, running)
            Next
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Updates the 'running' status of a queue, to either pause or resume the
    ''' queue's operation.
    ''' </summary>a
    ''' <param name="queueID">The ID of the queue of interest.</param>
    ''' <param name="running">True to resume the queue, False to pause queue.</param>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Sub SetQueueRunningStatus(ByVal queueId As Guid, ByVal running As Boolean) Implements IServer.SetQueueRunningStatus
        CheckPermissions()
        Using con = GetConnection()
            SetQueueRunningStatus(con, queueId, running)
        End Using
    End Sub

    ''' <summary>
    ''' Updates the 'running' status of a queue, to either pause or resume the
    ''' queue's operation.
    ''' </summary>
    ''' <param name="con">The connection over which to set the queue's status</param>
    ''' <param name="queueID">The ID of the queue of interest.</param>
    ''' <param name="running">True to resume the queue, False to pause queue.</param>
    Private Sub SetQueueRunningStatus(
     ByVal con As IDatabaseConnection, ByVal queueId As Guid, ByVal running As Boolean)
        Dim cmd As New SqlCommand(
         "update BPAWorkQueue set running = @running where id = @id")
        With cmd.Parameters
            .AddWithValue("@running", running)
            .AddWithValue("@id", queueId)
        End With
        con.Execute(cmd)
    End Sub

    ''' <summary>
    ''' Gets the item positions for all the item IDs in the given dictionary
    ''' This will set the value corresponding to the given item ID to the position
    ''' within the given work queue.
    ''' </summary>
    ''' <param name="queueID">The globally unique id of the work queue</param>
    ''' <param name="items">The map of items, keyed on the item guid. The position
    ''' of the item in the queue will be set in here - if the item is considered not
    ''' to be in the queue (<i>eg.</i> it is currently locked, or is marked with an
    ''' exception, or has reached the maximum number of attempts for that queue) then
    ''' the position value will be unchanged by this method.</param>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesReadOnly, Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Sub WorkQueueGetItemPositions(
     ByVal queueID As Guid,
     ByRef items As IDictionary(Of Long, clsWorkQueueItem)) Implements IServer.WorkQueueGetItemPositions
        CheckPermissions()

        Using con = GetConnection()
            ' Use the same query as the GetNext method - that way we can be fairly
            ' sure that the order returned is equivalent
            Dim cmd As New SqlCommand(
                " select i.ident " &
                " from BPAWorkQueueItem i " &
                "   left join BPACaseLock cl on i.ident = cl.id" &
                " where i.queueid = @queueid" &
                "   and cl.id is null " &
                "   and i.finished is null " &
                "   and (i.deferred is null or i.deferred <= GETUTCDATE())" &
                " order by i.priority, i.loaded, i.ident"
            )

            cmd.Parameters.AddWithValue("@queueid", queueID)

            Dim res = con.ExecuteReturnDataReader(cmd)
            Dim updated As Integer = 0
            Dim rowNum As Integer = 0
            Dim size As Integer = items.Count

            ' Read all rows until we've updated each one in the queue.
            While (res.Read() AndAlso updated < size)
                rowNum += 1
                Dim ident As Long = CType(res("ident"), Long)
                If items.ContainsKey(ident) Then
                    items(ident).Position = rowNum
                    updated += 1
                End If
            End While
        End Using

    End Sub

    ''' <summary>
    ''' Gets a session ID (which is present in the database) for the given work
    ''' queue item. This will represent the session which last updated the given
    ''' work queue item, by either 'completing' it, marking it with an exception,
    ''' deferring it, or, failing any of those, by retrieving it and locking it.
    '''
    ''' If the session ID for the given item is not set (because the item has not
    ''' been actioned yet), or if the session it refers to no longer exists on
    ''' the database (eg. it has been archived), then Guid.Empty is returned.
    ''' </summary>
    ''' <param name="itemIdent">The identity of the work queue item for which
    ''' the session is required </param>
    ''' <param name="seqNo">The sequence number closest to the time when the
    ''' given work queue item was actioned. Note that if the exact time could
    ''' not be found, this will set the sequence number to that of the next time
    ''' <i>before</i> the time the item was actioned.
    ''' If no action time could be found on the work queue item (it checks for the
    ''' latest of the following dates/times: completed, exceptioned, locked, loaded),
    ''' this will be set to -1.
    ''' Also if the action time is longer than 30 seconds after the nearest
    ''' log entry, it will be set to -1
    ''' </param>
    ''' <returns>The GUID representing the session which actioned this work
    ''' queue item. </returns>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesReadOnly, Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Function WorkQueueGetValidSessionId(
     ByVal itemIdent As Long, ByRef seqNo As Integer) As Guid Implements IServer.WorkQueueGetValidSessionId
        CheckPermissions()
        seqNo = -1 ' just in case

        Dim sessionId As Guid = Nothing
        Dim actionDate As Date = Nothing

        Using con = GetConnection()

            ' First off - some small explanations here.
            ' The startdatetime of BPAProcess is held in local time (and is
            ' used throughout the program as local time), so it needs to be
            ' converted to UTC to work with the queue item's time.
            ' We need a subselect so that we can filter on the 'actionedtime'
            ' which is generated from the varying different times on the work
            ' queue item.
            Dim cmd As New SqlCommand()
            cmd.CommandText =
             " select sessionid, lastupdated, locked from BPVWorkQueueItem where ident = @ident"
            cmd.Parameters.AddWithValue("@ident", itemIdent)

            Using reader = con.ExecuteReturnDataReader(cmd)

                If Not reader.Read() Then Return Nothing ' No log here....

                Dim prov As New ReaderDataProvider(reader)
                sessionId = prov.GetValue("sessionid", Guid.Empty)
                ' Get the latest of the last updated date or the locked date
                ' Ensure it's marked as being a UTC date
                actionDate = Date.SpecifyKind(clsUtility.Max(
                 prov.GetValue("lastupdated", Date.MinValue),
                 prov.GetValue("locked", Date.MinValue)),
                 DateTimeKind.Utc)

            End Using

            cmd.CommandText =
            " select top 1 isnull(l1.logid, l2.logid) as logid" &
            " from BPVSession s" &
            "   left join BPASessionLog_NonUnicode l1 on s.sessionnumber = l1.sessionnumber" &
            "   left join BPASessionLog_Unicode l2 on s.sessionnumber = l2.sessionnumber" &
            " where s.sessionid = @id" &
            "   and (l1.startdatetime is null or l1.startdatetime <= @date)" &
            "   and (l2.startdatetime is null or l2.startdatetime <= @date)" &
            " order by l1.logid desc, l2.logid desc"

            With cmd.Parameters
                .Clear()
                .AddWithValue("@id", sessionId)
                .AddWithValue("@date", actionDate.ToLocalTime())
            End With

            Using reader = con.ExecuteReturnDataReader(cmd)

                If Not reader.Read() Then Return sessionId ' No log entries before 'action' time.
                Dim prov As New ReaderDataProvider(reader)
                seqNo = prov.GetValue("logid", -1) ' -1 indicates not found...

                Return sessionId

            End Using

        End Using

    End Function

    ''' <summary>
    ''' Gets the xml representation of the named filter.
    ''' </summary>
    ''' <param name="FilterName">The name of the filter of interest.</param>
    ''' <param name="FilterXML">Carries back the xml requested.</param>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesReadOnly, Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Sub WorkQueueGetFilterXML(ByVal FilterName As String, ByRef FilterXML As String) Implements IServer.WorkQueueGetFilterXML
        CheckPermissions()

        If String.IsNullOrEmpty(FilterName) Then
            Throw New ArgumentException(My.Resources.clsServer_WorkQueueGetFilterXML_FilterNameCannotBeBlank)
        End If

        Dim con = GetConnection()
        Try


            Dim cmd As New SqlCommand("SELECT FilterXML FROM BPAWorkQueueFilter WHERE FilterName=@filtername")
            With cmd.Parameters
                .AddWithValue("@filtername", FilterName)
            End With

            Dim RetVal As Object = con.ExecuteReturnScalar(cmd)
            If RetVal IsNot Nothing Then
                FilterXML = CStr(RetVal)
            Else
                Throw New BluePrismException(String.Format(My.Resources.clsServer_NoFilterFoundInTheDatabaseWithTheName0, FilterName))
            End If
        Catch ex As BluePrismException
            Throw New Exception(ex.Message)
        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.clsServer_ErrorInteractingWithDatabase0, ex.Message), ex)
        Finally
            con.Close()
        End Try
    End Sub

    ''' <summary>
    ''' Creates a filter with the supplied details.
    ''' </summary>
    ''' <param name="filterName">The name of the filter to create.</param>
    ''' <param name="filterXML">The xml of the new filter.</param>
    ''' <param name="filterID">Carries back the ID of the new filter</param>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesFullAccess, Permission.ControlRoom.ManageQueuesReadOnly)>
    Public Sub WorkQueueCreateFilter(ByVal filterName As String, ByRef filterXML As String, ByRef filterID As Guid) Implements IServer.WorkQueueCreateFilter
        CheckPermissions()
        Using con = GetConnection()
            Try
                Dim NewID As Guid = Guid.NewGuid
                Dim cmd As New SqlCommand("INSERT INTO BPAWorkQueueFilter (FilterID, FilterName, FilterXML) VALUES (@filterid, @filtername, @filterxml)")
                With cmd.Parameters
                    .AddWithValue("@filterid", NewID)
                    .AddWithValue("@filtername", filterName)
                    .AddWithValue("@filterxml", filterXML)
                End With

                con.Execute(cmd)
                filterID = NewID
            Catch sqlEx As SqlException When sqlEx.Number = DatabaseErrorCode.UniqueConstraintError
                Throw sqlEx
            End Try
        End Using

    End Sub

    ''' <summary>
    ''' Updates the xml of the specified filter.
    ''' </summary>
    ''' <param name="filterName">The name of the filter to be updated.</param>
    ''' <param name="filterXML">The new filter xml.</param>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Function WorkQueueUpdateFilter(ByVal filterName As String, ByVal filterXML As String) As Integer Implements IServer.WorkQueueUpdateFilter
        CheckPermissions()

        Using con = GetConnection()
            Dim cmd As New SqlCommand("UPDATE BPAWorkQueueFilter SET FilterXML=@filterxml WHERE FilterName=@filtername")
            With cmd.Parameters
                .AddWithValue("@filtername", filterName)
                .AddWithValue("@filterxml", filterXML)
            End With

            Dim affectedRecords As Integer = CInt(con.ExecuteReturnRecordsAffected(cmd))
            Return affectedRecords

        End Using

    End Function

    ''' <summary>
    ''' Deletes the filter with the specified name.
    ''' </summary>
    ''' <param name="filterName">The name of the filter to delete.</param>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesFullAccess, Permission.ControlRoom.ManageQueuesReadOnly)>
    Public Sub WorkQueueDeleteFilter(filterName As String) Implements IServer.WorkQueueDeleteFilter
        CheckPermissions()

        If String.IsNullOrEmpty(filterName) Then
            Throw New ArgumentException(My.Resources.clsServer_WorkQueueDeleteFilter_FilterNameCannotBeBlank, NameOf(filterName))
        End If

        Using con = GetConnection()
            con.BeginTransaction()

            Using cmd As New SqlCommand(
                " update BPAWorkQueue set DefaultFilterID = null " &
                "   where DefaultFilterID = " &
                "     (select FilterID from BPAWorkQueueFilter " &
                "       where FilterName=@filtername)")
                With cmd.Parameters
                    .AddWithValue("@filtername", filterName)
                End With
                con.Execute(cmd)
            End Using

            Using cmd As New SqlCommand(
                " delete from BPAWorkQueueFilter where FilterName=@filtername")

                With cmd.Parameters
                    .AddWithValue("@filtername", filterName)
                End With

                Dim affected = con.ExecuteReturnRecordsAffected(cmd)
                If affected <= 0 Then Throw New BluePrismException(
                        My.Resources.clsServer_CommandCompletedSuccesfullyButNoItemsWereDeleted)
            End Using

            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Gets a list of the names of the filters stored in the database.
    ''' </summary>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesReadOnly, Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Function WorkQueueGetFilterNames() As List(Of String) Implements IServer.WorkQueueGetFilterNames
        CheckPermissions()

        Using con = GetConnection()
            Dim NewID As Guid = Guid.NewGuid
            Dim cmd As New SqlCommand("SELECT FilterName FROM BPAWorkQueueFilter")

            Dim filterNames = New List(Of String)

            Dim dr = CType(con.ExecuteReturnDataReader(cmd), SqlDataReader)
            If dr IsNot Nothing AndAlso dr.HasRows Then
                While dr.Read
                    filterNames.Add(CStr(dr.Item("FilterName")))
                End While
            End If

            Return filterNames
        End Using
    End Function

    ''' <summary>
    ''' Sets the default filter on a queue.
    ''' </summary>
    ''' <param name="queueID">The queue to be updated.</param>
    ''' <param name="filterName">The name of the default filter to be used
    ''' on this queue.</param>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesReadOnly, Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Function WorkQueueSetDefaultFilter(ByVal queueID As Guid, ByVal filterName As String) As Boolean Implements IServer.WorkQueueSetDefaultFilter
        CheckPermissions()
        Using con = GetConnection()

            Dim NewID As Guid = Guid.NewGuid
            Dim SQL As New System.Text.StringBuilder("DECLARE @FilterID uniqueidentifier")
            SQL.Append(vbCrLf & "SET @FilterID = (SELECT TOP 1 FilterID FROM BPAWorkQueueFilter WHERE FilterName=@filtername)")
            SQL.Append(vbCrLf & "UPDATE BPAWorkQueue SET DefaultFilterID=@FilterID WHERE [id]=@queueid")
            SQL.Append(vbCrLf & "SELECT @FilterID")

            Dim cmd As New SqlCommand(SQL.ToString)
            With cmd.Parameters
                .AddWithValue("@filtername", filterName)
                .AddWithValue("@queueid", queueID)
            End With

            Dim FilterID As Object = con.ExecuteReturnScalar(cmd)
            If FilterID IsNot Nothing Then
                Return True
            Else
                Return False
            End If
        End Using
    End Function

    ''' <summary>
    ''' Gets the default filter associated with a queue.
    ''' </summary>
    ''' <param name="QueueID">The ID of the queue of interest.</param>
    ''' <param name="FilterID">Carries back the ID of the default filter configured
    ''' against this queue, or Guid.Empty if none.</param>
    ''' <param name="FilterName">Carries back the name of the filter identified by
    ''' FilterID, or Nothing where appropriate.</param>
    ''' <returns>True if successful, False otherwise</returns>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesReadOnly, Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Function WorkQueueGetDefaultFilter(ByVal queueID As Guid, ByRef filterID As Guid, ByRef filterName As String) As Boolean Implements IServer.WorkQueueGetDefaultFilter
        CheckPermissions()

        Using con = GetConnection()

            Dim NewID As Guid = Guid.NewGuid
            Dim cmd As New SqlCommand("SELECT Q.DefaultFilterID, F.FilterName FROM BPAWorkQueue Q LEFT JOIN BPAWorkQueueFilter F ON (F.FilterID = Q.DefaultFilterID) WHERE [id]=@queueid")
            With cmd.Parameters
                .AddWithValue("@queueid", queueID)
            End With

            Dim Reader = con.ExecuteReturnDataReader(cmd)
            If Reader.Read() Then
                filterID = Guid.Empty
                Dim oID As Object = Reader("DefaultFilterID")
                If Not TypeOf oID Is DBNull Then filterID = CType(oID, Guid)
                filterName = TryCast(Reader("FilterName"), String)
                Return True
            Else
                Return False
            End If
        End Using
    End Function


    ''' <summary>
    ''' Gets the data collection in XML format for the queue item identified by the
    ''' given identity value.
    ''' </summary>
    ''' <param name="ident">The identity of the item for which the data is required.
    ''' </param>
    ''' <returns>The data XML corresponding to the given item, or an empty string if
    ''' the item had no data associated with it.</returns>
    <SecuredMethod()>
    Public Function WorkQueueItemGetDataXml(ByVal ident As Long) As String Implements IServer.WorkQueueItemGetDataXml
        CheckPermissions()
        Using con = GetConnection()
            Return WorkQueueItemGetDataXml(con, Nothing, ident)
        End Using
    End Function

    ''' <summary>
    ''' Gets the data collection in XML format for the (latest) queue item attempt
    ''' identified by the given ID value.
    ''' </summary>
    ''' <param name="id">The item ID for the item whose data is required. The latest
    ''' attempt of this item will be used to retrieve the data.</param>
    ''' <returns>The data XML corresponding to the given item, or an empty string if
    ''' the item had no data associated with it.</returns>
    <SecuredMethod()>
    Public Function WorkQueueItemGetDataXml(ByVal id As Guid) As String Implements IServer.WorkQueueItemGetDataXml
        CheckPermissions()
        Using con = GetConnection()
            Return WorkQueueItemGetDataXml(con, id, 0L)
        End Using
    End Function

    ''' <summary>
    ''' Gets the data collection in XML format for the (latest) queue item attempt
    ''' identified by the given ID value, or for the specific queue item identified
    ''' by the given identity.
    ''' </summary>
    ''' <param name="id">The item ID for the item whose data is required. The latest
    ''' attempt of this item will be used to retrieve the data.</param>
    ''' <param name="ident">The identity of the specific item attempt for which the
    ''' data is required. </param>
    ''' <returns>The data XML corresponding to the given item, or an empty string if
    ''' the item had no data associated with it.</returns>
    Private Function WorkQueueItemGetDataXml(
     ByVal con As IDatabaseConnection, ByVal id As Guid, ByVal ident As Long) As String
        Dim cmd As New SqlCommand()
        If ident = 0L Then
            cmd.CommandText = "select max(ident) from BPAWorkQueueItem where id = @id"
            With cmd.Parameters
                .AddWithValue("@id", id)
                ident = CLng(con.ExecuteReturnScalar(cmd))
                .Clear()
            End With
        End If
        If ident = 0L Then Throw New BluePrismException(My.Resources.clsServer_NoItemIdentityAvailableToGetTheDataFor)

        cmd.CommandText =
         " select q.encryptid, i.data" &
         "   from BPAWorkQueueItem i" &
         "     join BPAWorkQueue q on i.queueident = q.ident" &
         "   where i.ident = @ident"
        cmd.Parameters.AddWithValue("@ident", ident)

        Using reader = con.ExecuteReturnDataReader(cmd)
            If Not reader.Read() Then Throw New BluePrismException(
             My.Resources.clsServer_FailedToReadTheDataForTheQueueItemWithIdentity0, ident)

            Dim prov As New ReaderDataProvider(reader)
            Dim decryptedData As String =
             Decrypt(prov.GetValue("encryptid", 0), prov.GetString("data"))
            If decryptedData Is Nothing Then Return "" Else Return decryptedData

        End Using

    End Function

    ''' <summary>
    ''' Retrieves the items within the queue.
    ''' </summary>
    ''' <param name="workQueueId">The Id of the work queue we want our items from.</param>
    ''' <param name="workQueueItemParameters">The parameters defining the search, order and paging.</param>
    ''' <returns>A collection of items within the work queue.</returns>
    ''' <exception cref="NoSuchQueueException">If no work queue with the specified
    ''' id was found on the database.</exception>
    <SecuredMethod()>
    Public Function WorkQueueGetQueueItems(workQueueId As Guid, workQueueItemParameters As WorkQueueItemParameters) As ICollection(Of clsWorkQueueItem) Implements IServer.WorkQueueGetQueueItems
        CheckPermissions()

        Dim queryStringBuilder As New StringBuilder(500)
        queryStringBuilder.Append($"select top ({workQueueItemParameters.ItemsPerPage})
	                    id,
                        ident,
                        keyvalue,
                        priority,
                        status,
                        attempt,
                        loaded,
                        lastupdated,
                        worktime,
                        attemptworktime,
                        deferred,
                        locked,
                        completed,
                        exception,
                        exceptionreason,
                        runningresourcename,
                        tag
                    from BPAWorkQueueItem i
                    outer apply
                    (
	                    (select stuff(
		                    (select  ',' + tag from
			                    (select bt.tag from BPATag AS bt
				                    inner join BPAWorkQueueItemTag AS bwqit ON bwqit.tagid = bt.id
				                    where bwqit.queueitemident=i.ident
			                    ) as t for xml path(''))
		                    ,1,1,'')tag)   
                    ) t
                    left join (select id as lockId, locktime as locked from BPACaseLock) lk on i.ident = lk.lockId
                    left join (select sessionid, runningresourcename from BPVSessionInfo) s on i.sessionid = s.sessionid
                    where queueid = @workQueueId")

        Dim sqlCommand As New SqlCommand()
        sqlCommand.Parameters.AddWithValue("@workQueueId", workQueueId)
        sqlCommand.CommandTimeout = Options.Instance.SqlCommandTimeoutLong

        Dim sqlData = workQueueItemParameters.
            GetSqlWhereClauses(sqlCommand).
            GetSqlWhereWithParametersStartingWithAndKeyword()

        queryStringBuilder.AppendLine(sqlData.sqlWhereClause)
        sqlCommand.Parameters.AddRange(sqlData.sqlParameters)

        queryStringBuilder.Append($" order by {workQueueItemParameters.GetSqlOrderByClauses()}")

        sqlCommand.CommandText = queryStringBuilder.ToString()

        Return GetWorkQueueItems(sqlCommand, workQueueId)
    End Function

    Private Function GetWorkQueueItems(sqlCommand As IDbCommand, workQueueId As Guid) As ICollection(Of clsWorkQueueItem)
        Dim result = New List(Of clsWorkQueueItem)()

        Using connection = GetConnection()

            ThrowIfQueueNotExist(connection, workQueueId)

            Using reader = connection.ExecuteReturnDataReader(sqlCommand)

                Dim dataProvider As New ReaderDataProvider(reader)

                While reader.Read()

                    Dim item = New clsWorkQueueItem(
                        dataProvider.GetValue("id", Guid.Empty),
                        dataProvider.GetValue(Of Long)("ident", 0),
                        dataProvider.GetValue("keyvalue", ""))

                    item.Priority = dataProvider.GetInt("priority")
                    item.Status = dataProvider.GetValue("status", "")
                    item.Attempt = dataProvider.GetInt("attempt")
                    item.Loaded = dataProvider.GetValue("loaded", Date.MinValue)
                    item.LastUpdated = dataProvider.GetValue("lastupdated", Date.MinValue)
                    item.Worktime = dataProvider.GetInt("worktime")
                    item.AttemptWorkTime = dataProvider.GetValue("attemptworktime", 0)
                    item.Deferred = dataProvider.GetValue("deferred", Date.MinValue)
                    item.Locked = dataProvider.GetValue("locked", Date.MinValue)
                    item.CompletedDate = dataProvider.GetValue("completed", Date.MinValue)
                    item.ExceptionDate = dataProvider.GetValue("exception", Date.MinValue)
                    item.ExceptionReason = dataProvider.GetValue("exceptionreason", "")
                    item.Resource = dataProvider.GetString("runningresourcename")

                    Dim tag = dataProvider.GetValue("tag", "")
                    If tag.Length > 0 Then item.AddTag(tag)
                    result.Add(item)
                End While

            End Using
        End Using

        Return result
    End Function

    ''' <summary>
    ''' Retrieves the filtered contents of a specific work queue.
    ''' </summary>
    ''' <param name="queueID">The ID of the queue of interest.</param>
    ''' <param name="filter">The filtering information used to retrieve
    ''' the desired data.</param>
    ''' <param name="totalItems">The total number of items matching this query.
    ''' This is the number of rows that would be returned if the maxrows
    ''' member of the query were unlimited.</param>
    ''' <param name="results">The results of the data retrieval in the form of a
    ''' collection of <see cref="clsWorkQueueItem"/> objects.
    ''' </param>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesReadOnly, Permission.ControlRoom.ManageQueuesFullAccess)>
    Public Sub WorkQueuesGetQueueFilteredContents(
     ByVal queueID As Guid,
     ByVal filter As WorkQueueFilter,
     ByRef totalItems As Integer,
     ByRef results As ICollection(Of clsWorkQueueItem)) Implements IServer.WorkQueuesGetQueueFilteredContents
        CheckPermissions()
        Using con = GetConnection()
            WorkQueuesGetQueueFilteredContents(
             con, queueID, filter, totalItems, results)
        End Using
    End Sub


    ''' <summary>
    ''' Retrieves the filtered contents of a specific work queue on the provided
    ''' DB connection.
    ''' </summary>
    ''' <param name="con">The valid, open connection on which the queue's
    ''' contents should be retrieved. This is here to enable searching to
    ''' be performed within an externally defined transaction.</param>
    ''' <param name="queueID">The ID of the queue of interest.</param>
    ''' <param name="filter">The filtering information used to retrieve
    ''' the desired data.</param>
    ''' <param name="totalItems">The total number of items matching this query.
    ''' This is the number of rows that would be returned if the maxrows
    ''' member of the query were unlimited.</param>
    ''' <param name="results">The results of the data retrieval in the form of a
    ''' collection of <see cref="clsWorkQueueItem"/> objects.
    ''' </param>
    Private Sub WorkQueuesGetQueueFilteredContents(
     ByVal con As IDatabaseConnection,
     ByVal queueID As Guid,
     ByVal filter As WorkQueueFilter,
     ByRef totalItems As Integer,
     ByRef results As ICollection(Of clsWorkQueueItem))
        If filter.MaxRows <= 0 Then
            Throw New ArgumentException(My.Resources.clsServer_MaxRowsMustBeAtLeast1)
        End If
        ' Build up the where clause - we need it for both the select query for the
        ' Data and the query to find the max number of rows
        Dim sbWhere As New StringBuilder("i.queueid=@queueid")
        filter.AppendWhereClause(sbWhere, "i")
        Dim whereClause As String = sbWhere.ToString()

        ' The "order by" column
        Dim OrdCol As String = filter.GetSortColumnDBName()

        Dim sb As New StringBuilder()

        ' First get a total count, then the full query to return the items

        ' This uses a temp table to store the idents in, to ensure we get the
        ' correct numbers (without the count skewed by left joining the tags)

        ' After the total count, the first select populates the temp table with
        ' the idents - the extra '{3}' in the 'select top {1} i.ident, {3}'
        ' is there so that the ordering can be done on the chosen column -
        ' this is the primary reason that tags can't be sorted on - the sort
        ' has to be on the work queue item table. The other reason is that
        ' tags have arbitrary order, so you may get different results each
        ' time you sort on tags with the same dataset.

        ' The temp table then has all the ident values for all items which
        ' match the filter and are paged to the appopriate place within the
        ' sorted table (as well as some which need to be skipped - a drawback
        ' of the 'TOP' syntax).
        ' So we join the temp table to BPAWorkQueueItem, now left-joining in
        ' the tags too and return all our values.
        '
        ' At this point the calculated columns are processed (actually if
        ' sorting on a (non-persisted) calculated column, they will be
        ' processed in the temp table insert too - no way around that, really)
        ' meaning that they are only calculated for the page of items that
        ' we will be returning (with the exception mentioned above).

        ' Small note about the local @queueid variable and the option(recompile)
        ' Sql Server performs 'parameter sniffing' to calculate the best query
        ' plan for the *first* time it sees a query - every matching query after
        ' that will use that plan whether it's suitable or not. The local
        ' variable disables parameter sniffing for the queue ID, and the
        ' option(recompile) forces a recompile of the query - in my tests, it
        ' needs both in order to work efficiently.
        sb.AppendFormat(
             " declare @queueid uniqueidentifier; " &
             " set @queueid = @qid; " &
             " select count(*) as TotalItems" &
             " from BPVWorkQueueItem i" &
             "   left join BPVSessionInfo s on i.sessionid = s.sessionid" &
             " where {0}; " & _
 _
             " if object_id('tempdb..#idents') is not null drop table #idents;" &
             " create table #idents (" &
             "   ord int identity primary key," &
             "   itemident bigint not null" &
             " );" & _
 _
             " insert into #idents (itemident)" &
             " select ident from ( " &
             "   select " &
             "     i.ident as ident," &
             "     row_number() over (order by {1} {2}) as rownum" &
             "   from BPVWorkQueueItem i" &
             "     left join BPVSessionInfo s on i.sessionid=s.sessionid" &
             "   where {0}" &
             " ) as a" &
             " where a.rownum between @startInd and @endInd" &
             " order by a.rownum;" & _
 _
             " select i.id as id," &
             "       i.ident as ident," &
             "       i.keyvalue as keyvalue," &
             "       i.priority as priority," &
             "       i.status as status," &
             "       it.tag as tag," &
             "       i.attempt as attempt," &
             "       i.loaded as loaded," &
             "       i.lastupdated as lastupdated," &
             "       i.worktime as worktime," &
             "       i.attemptworktime," &
             "       i.deferred as deferred," &
             "       i.locked as locked," &
             "       i.completed as completed," &
             "       i.exception as exception," &
             "       i.exceptionreason," &
             "       i.queuepositiondate as queuepositiondate," &
             "       s.runningresourcename" &
             " from #idents idents" &
             "   join BPVWorkQueueItem i on idents.itemident = i.ident" &
             "   left join BPViewWorkQueueItemTag it on i.ident = it.queueitemident" &
             "   left join BPVSessionInfo s on i.sessionid = s.sessionid" &
             " order by idents.ord;",
             whereClause,
             OrdCol,
             IIf(filter.SortOrder = SortOrder.Descending, "desc", "asc")
            )

        ' And now the data
        Dim cmd As New SqlCommand(sb.ToString())
        With cmd.Parameters
            .AddWithValue("@qid", queueID)
            .AddWithValue("@startInd", filter.StartIndex + 1)
            .AddWithValue("@endInd", filter.StartIndex + filter.MaxRows)
        End With
        filter.AddParams(cmd)
        totalItems = 0
        cmd.CommandTimeout = Options.Instance.SqlCommandTimeoutLong

        ' Perform the query - return into a data reader
        Using reader = con.ExecuteReturnDataReader(cmd)

            ' Pop into a data provider to easily get our results.
            Dim prov As New ReaderDataProvider(reader)

            ' First get the total items, and from that, figure out how many we
            ' need to skip in our loop
            If reader.Read() Then
                totalItems = prov.GetValue("TotalItems", 0)
                If filter.StartIndex > 0 AndAlso filter.StartIndex >= totalItems Then
                    filter.StartIndex -= filter.MaxRows ' Point it to new last page
                End If

                filter.StartIndex = Math.Min(filter.StartIndex, totalItems - 1)
            End If

            ' Go over the data reader, and build up a collection of work queue items
            ' The map is so that we can do a quick lookup - due to the join with the
            ' tags, we're going to get multiple rows back per work queue item, so
            ' we only want to create one and set it in the dictionary.
            ' Finding it in a dictionary is much quicker than doing so in a list
            ' The list is so that we can maintain the order of the results from the
            ' database - there's no equivalent of a java LinkedHashMap which just
            ' wraps these two collections into one handy class.
            Dim orderedDictionary As New clsOrderedDictionary(Of Long, clsWorkQueueItem)
            results = orderedDictionary.Values

            If Not reader.NextResult Then
                ' Does this ever happen? No - at least, it *should* never happen.
                Throw New InvalidOperationException(
                 My.Resources.clsServer_DatabaseShouldHaveReturnedTwoResultsItReturnedOnlyOne)
            End If

            While reader.Read()
                Dim ident As Long = prov.GetValue(Of Long)("ident", 0)
                Dim item As clsWorkQueueItem = Nothing

                If Not orderedDictionary.TryGetValue(ident, item) Then
                    ' We only need to initialise the item when we create it.
                    ' After that, it's just the join from the tags which is causing it
                    ' to appear in the data reader
                    item = New clsWorkQueueItem(
                     prov.GetValue("id", Guid.Empty),
                     ident, prov.GetValue("keyvalue", ""))

                    orderedDictionary(ident) = item
                    ' Load up the rest of the data
                    item.Priority = prov.GetValue("priority", 0)
                    item.Status = prov.GetValue("status", "")
                    item.Resource = prov.GetString("runningresourcename")
                    item.Attempt = prov.GetValue("attempt", 0)
                    item.Loaded = prov.GetValue("loaded", Date.MinValue)
                    item.Deferred = prov.GetValue("deferred", Date.MinValue)
                    item.Locked = prov.GetValue("locked", Date.MinValue)
                    item.CompletedDate = prov.GetValue("completed", Date.MinValue)
                    item.Worktime = prov.GetValue("worktime", 0)
                    item.AttemptWorkTime = prov.GetValue("attemptworktime", 0)
                    item.ExceptionDate = prov.GetValue("exception", Date.MinValue)
                    item.ExceptionReason = prov.GetValue("exceptionreason", "")
                    ' Add it to the list of items we're returning
                    results.Add(item)
                End If
                Dim tag As String = prov.GetValue("tag", "")
                If tag.Length > 0 Then item.AddTag(tag)
            End While
        End Using
    End Sub

    ''' <summary>
    ''' Gets report data using the given parameters - returns all data inside a
    ''' report data object.
    ''' </summary>
    ''' <param name="params">The parameters defining the report data to be
    ''' retrieved.</param>
    ''' <returns>A report data object with the pertinent data prepopulated.</returns>
    ''' <exception cref="ArgumentException">If any of the following are true :-
    ''' <list>
    ''' <item>No queue name was provided</item>
    ''' <item>No start date was provided (either 'added' or 'finished')</item>
    ''' <item>No state was set to be included (unworked, deferred, completed or
    ''' exceptioned)</item></list></exception>
    ''' <exception cref="SqlException">If any database errors occur.</exception>
    <SecuredMethod()>
    Public Function WorkQueueGetReportData(ByVal params As ReportParams) As ReportData Implements IServer.WorkQueueGetReportData
        CheckPermissions()
        Using con = GetConnection()
            ' The transaction is only to ensure that any data is consistent - it
            ' shouldn't actually do any updates and it can be rolled back safely
            ' and thus is not committed (or explicitly rolled back) after the
            ' data access is complete
            con.BeginTransaction()
            Return WorkQueueGetReportData(con, params)
        End Using

    End Function

    ''' <summary>
    ''' Gets report data using the given parameters - returns all data inside a
    ''' report data object.
    ''' </summary>
    ''' <param name="con">The connection on which the report should be run.</param>
    ''' <param name="params">The parameters defining the report data to be
    ''' retrieved.</param>
    ''' <returns>A report data object with the pertinent data prepopulated.</returns>
    ''' <exception cref="ArgumentException">If any of the following are true :-
    ''' <list>
    ''' <item>No queue name was provided</item>
    ''' <item>No start date was provided (either 'added' or 'finished')</item>
    ''' <item>No state was set to be included (unworked, deferred, completed or
    ''' exceptioned)</item></list></exception>
    ''' <exception cref="SqlException">If any database errors occur.</exception>
    Private Function WorkQueueGetReportData(
     ByVal con As IDatabaseConnection, ByVal params As ReportParams) As ReportData
        GetWorkQueueIdent(con, params.Name)
        ' Some sanity checks before we get cracking...
        If String.IsNullOrEmpty(params.Name) Then
            Throw New ArgumentException(My.Resources.clsServer_YouMustProvideAQueueName, My.Resources.clsServer_WorkQueueGetReportData_Name)
        End If
        If params.AddedStartDate = Nothing AndAlso params.FinishStartDate = Nothing Then
            Throw New ArgumentException(My.Resources.clsServer_WorkQueueGetReportData_YouMustProvideAtLeastOneStartDate, My.Resources.clsServer_WorkQueueGetReportData_StartDate)
        End If
        If Not (params.Unworked OrElse params.Deferred OrElse params.Completed OrElse params.Exceptioned) Then
            Throw New ArgumentException(My.Resources.clsServer_WorkQueueGetReportData_YouMustIncludeAtLeastOneQueueItemState,
             My.Resources.clsServer_WorkQueueGetReportData_UnworkedDeferredCompletedExceptioned)
        End If

        Dim cmd As New SqlCommand() ' The command (ie. SQL batch) we're building up
        Dim i As Integer = 0 ' Counter for the multiple params (resources, tags and 'states')

        ' Make sure that any previous incarnation of the temp table is gone, and
        ' create it anew.
        Dim sb As New StringBuilder(" declare @individuals table (" &
         "   ident bigint not null primary key," &
         "   id uniqueidentifier not null," &
         "   state tinyint not null," &
         "   createdate datetime not null," &
         "   lastupdateddate datetime not null," &
         "   finishdate datetime null," &
         "   worktime int not null default 0" &
         " );" & _
 _
         " insert into @individuals"
        )

        ' The query which handles the population of the temp table differs depending
        ' on whether we are looking at each record as a separate entity, or as a
        ' part of the greater whole.
        ' The arguments here map onto the format placeholders in the query below and
        ' change the query appropriately
        Dim args() As String
        If params.IsEachAttemptSeparate Then
            args = New String() {"attemptworktime", "", ""}
        Else
            args = New String() {"worktime",
             "left join BPAWorkQueueItem inext on inext.id=i.id and inext.attempt=i.attempt+1",
             "and inext.id is null"}
        End If

        sb.AppendFormat(
         "   select" &
         "     i.ident, " &
         "     i.id," &
         "     i.state," &
         "     i.loaded," &
         "     i.lastupdated," &
         "     i.finished," &
         "     isnull(i.{0},0)" &
         "   from BPVWorkQueueItem i" &
         "     join BPAWorkQueue q on i.queueid = q.id" &
         "     {1}" &
         "     left join BPVSession s on i.sessionid = s.sessionid" &
         "     left join BPAResource r on r.resourceid = s.runningresourceid" &
         "   where q.name = @queuename" &
         "     {2}", DirectCast(args, String())
        )

        ' The only parameter used in the above block
        cmd.Parameters.AddWithValue("@queuename", params.Name)

        ' Get out the resource names and check for them
        If params.ResourceNames.Count > 0 Then
            sb.AppendFormat(
             "      and r.name in ("
            )
            For Each resource As String In params.ResourceNames
                i += 1
                sb.AppendFormat("@res{0},", i)
                cmd.Parameters.AddWithValue("@res" & i, resource)
            Next
            ' Remove the trailing comma and close the in(...) parantheses
            sb.Length -= 1
            sb.Append(")")
        End If

        ' Show those with the specified tags
        AddTagFilter(sb, params.Tags, cmd, True)

        ' The sub-selects done - just a little filtering on the outside to limit the temp
        ' table to the data we actually need to report on.
        sb.Append(" and")
        If params.AddedStartDate <> Nothing Then
            sb.Append(" i.loaded between @startdate_add and @enddate_add and")
            With cmd.Parameters
                .AddWithValue("@startdate_add", clsDBConnection.UtilDateToSqlDate(params.AddedStartDate, False, False))
                .AddWithValue("@enddate_add", clsDBConnection.UtilDateToSqlDate(params.AddedEndDate, False, False))
            End With
        End If
        If params.FinishStartDate <> Nothing Then
            sb.Append(" i.finished between @startdate_fin and @enddate_fin and")
            With cmd.Parameters
                .AddWithValue("@startdate_fin", clsDBConnection.UtilDateToSqlDate(params.FinishStartDate, False, False))
                .AddWithValue("@enddate_fin", clsDBConnection.UtilDateToSqlDate(params.FinishEndDate, False, False))
            End With
        End If
        ' The states are defined in the main select query above. Basically :-
        ' 0 = pending; 1 = locked; 2 = deferred; 3 = completed; 4 = exceptioned
        Dim states As New List(Of QueueItemState)
        If params.Unworked Then states.AddRange(New QueueItemState() {QueueItemState.Pending, QueueItemState.Locked})
        If params.Deferred Then states.Add(QueueItemState.Deferred)
        If params.Completed Then states.Add(QueueItemState.Completed)
        If params.Exceptioned Then states.Add(QueueItemState.Exceptioned)

        ' A bit of a minor optimisation - WQI.state is no longer a persisted field, so if we're
        ' searching for all possible states then just don't bother searching at all - otherwise
        ' we're doing a calculation for each item and then including it anyway.
        If states.Count >= [Enum].GetValues(GetType(QueueItemState)).Length Then
            sb.Append(" 1=1")

        Else
            sb.Append(" i.state in (")
            For Each state As Integer In states
                i += 1
                sb.AppendFormat("@state{0},", i)
                cmd.Parameters.AddWithValue("@state" & i, state)
            Next

            ' last-comma-be-gone
            sb.Length -= 1
            sb.Append(")")

        End If

        sb.Append(";"c)

        ' Now for the actual reports - these are performed on the temp table which only
        ' contains the data we're interested in. Thus the actual reports should be
        ' relatively fast.

        ' The basic counts and aggregate times
        sb.Append(
         " select count(*)  as ""itemcount""," &
         "     sum(i.worktime) as ""total""," &
         "     min(i.worktime) as ""minimum""," &
         "     max(i.worktime) as ""maximum""," &
         "     avg(i.worktime) as ""mean""" &
         " from @individuals i;"
        )

        ' The median - avg of the 'upper median' and 'lower median'"
        sb.Append(
         " select ((i1.median + i2.median) / 2) as ""avg-median""" &
         " from (" &
         "     select max(i1.worktime) as median from (" &
         "         select top 50 percent i.worktime from @individuals i" &
         "         order by worktime asc" &
         "     ) i1" &
         " ) i1" &
         " cross join (" &
         "     select min(i2.worktime) as median from (" &
         "         select top 50 percent i.worktime from @individuals i " &
         "         order by worktime desc" &
         "     ) i2" &
         " ) i2;"
        )

        ' The Item IDs
        sb.Append(
         " select distinct i.id from @individuals i;"
        )

        cmd.CommandText = sb.ToString()

        Dim data As New ReportData()

        Using reader = con.ExecuteReturnDataReader(cmd)

            Dim prov As IDataProvider = New ReaderDataProvider(reader)

            ' basic should always be there, since 'itemcount' will always return a non-null result
            If Not reader.Read() Then Throw New InvalidOperationException(My.Resources.clsServer_NoBasicDataFound)

            ' The basics
            data.Count = prov.GetValue("itemcount", 0)
            data.TotalTime = New TimeSpan(prov.GetValue("total", 0) * TimeSpan.TicksPerSecond)
            data.LeastTime = New TimeSpan(prov.GetValue("minimum", 0) * TimeSpan.TicksPerSecond)
            data.MostTime = New TimeSpan(prov.GetValue("maximum", 0) * TimeSpan.TicksPerSecond)
            data.MeanTime = New TimeSpan(prov.GetValue("mean", 0) * TimeSpan.TicksPerSecond)

            ' Median is in the next result of the batch - this may return no rows
            If Not reader.NextResult Then Throw New InvalidOperationException(My.Resources.clsServer_NoMedianTimeFound)
            If reader.Read() Then
                data.MedianTime =
                 New TimeSpan(prov.GetValue("avg-median", 0) * TimeSpan.TicksPerSecond)
            Else
                data.MedianTime = TimeSpan.Zero
            End If

            ' And the item data is to follow - again, this may return no rows
            If Not reader.NextResult Then Throw New InvalidOperationException(My.Resources.clsServer_NoItemIDsFound)
            While reader.Read()
                data.Items.Add(prov.GetValue("id", Guid.Empty))
            End While

        End Using

        Return data

    End Function

    ''' <summary>
    ''' Re-encrypts a batch of work queue items according to the encryption scheme
    ''' associated with the particular queue.
    ''' </summary>
    ''' <param name="batchSize">The batch size</param>
    ''' <returns>The number of queue items updated (or -1 if there are no remaining
    ''' items)</returns>
    <SecuredMethod("Security - Manage Encryption Schemes")>
    Public Function ReEncryptQueueItems(batchSize As Integer) As Integer Implements IServer.ReEncryptQueueItems
        CheckPermissions()
        Dim count As Integer = 0

        'Setup command to select queue items requiring re-encryption (items
        'could be un-encrypted, so encryptid can be null here)
        Dim cmdSelect As New SqlCommand("select top " & batchSize &
             " i.ident as 'ID', q.encryptid as 'NewEncryptID'," &
             " i.encryptid as 'OldEncryptID', i.data as 'Data'" &
             " from BPAWorkQueueItem i inner join BPAWorkQueue q on q.ident=i.queueident" &
             " where isnull(i.encryptid, 0)<>isnull(q.encryptid, 0)")

        'Setup command to update queue items
        Dim cmdUpdate As New SqlCommand("update BPAWorkQueueItem" &
             " set encryptid=@encryptid, data=@data" &
             " where ident=@ident and isnull(encryptid, 0)<>isnull(@encryptid, 0)")
        With cmdUpdate.Parameters
            .Add("@encryptid", SqlDbType.Int).SourceColumn = "OldEncryptID"
            .Add("@data", SqlDbType.Text).SourceColumn = "Data"
            .Add("@ident", SqlDbType.BigInt).SourceColumn = "ID"
        End With

        'Process queue items in batches up to passed max
        Using con = GetConnection()
            Dim dt As DataTable = con.ExecuteReturnDataTable(cmdSelect)
            If dt.Rows.Count = 0 Then Return -1

            For Each row As DataRow In dt.Rows
                Dim oldEncryptID As Integer = 0
                If Not row("OldEncryptID").Equals(DBNull.Value) Then _
                 oldEncryptID = CInt(row("OldEncryptID"))
                Dim newEncryptID As Integer = 0
                If Not row("NewEncryptID").Equals(DBNull.Value) Then _
                 newEncryptID = CInt(row("NewEncryptID"))

                row("Data") = Encrypt(newEncryptID, Decrypt(oldEncryptID, CStr(row("Data"))))
                row("OldEncryptID") = IIf(newEncryptID = 0, DBNull.Value, newEncryptID)
            Next
            count += con.UpdateDataAdapter(dt, cmdUpdate)
        End Using

        Return count
    End Function

    ''' <summary>
    ''' Indicates whether or not the current user has permission to control the
    ''' specified active queue
    ''' </summary>
    ''' <param name="queueIdent">The Ident of the active queue to check</param>
    ''' <returns>True if the user has permission to control the queue</returns>
    <SecuredMethod(Permission.ControlRoom.ManageQueuesFullAccess,
                   Permission.ControlRoom.ManageQueuesReadOnly)>
    Public Function IsActiveQueueControllable(queueIdent As Integer) As Boolean Implements IServer.IsActiveQueueControllable
        CheckPermissions()
        Using con = GetConnection()
            Return IsActiveQueueControllable(con, GetProcessAndResourceGroupForActiveQueue(queueIdent, con))
        End Using
    End Function

    Private Function IsActiveQueueControllable(queueIdent As Integer, connection As IDatabaseConnection) As Boolean
        CheckPermissions()
        Return IsActiveQueueControllable(connection, GetProcessAndResourceGroupForActiveQueue(queueIdent, connection))
    End Function


    Private Function GetProcessAndResourceGroupForActiveQueue(queueIdent As Integer, connection As IDatabaseConnection) As (processId As Guid, resourceGroupId As Guid)
        Dim command As New SqlCommand("SELECT processid, resourcegroupid FROM BPAWorkQueue WHERE ident=@queueIdent")
        command.Parameters.AddWithValue("@queueIdent", queueIdent)
        Dim results As (processId As Guid, resourceGroupId As Guid)

        Dim dataTable As DataTable = connection.ExecuteReturnDataTable(command)
        For Each row As DataRow In dataTable.Rows
            Guid.TryParse(row("processid")?.ToString, results.processId)
            Guid.TryParse(row("resourcegroupid")?.ToString, results.resourceGroupId)
        Next

        Return results
    End Function

    <SecuredMethod(True)>
    Public Sub UpdateActiveQueueMI() Implements IServer.UpdateActiveQueueMI
        CheckPermissions()
        Using connection = GetConnection()
            Using command = New SqlCommand("usp_UpdateWorkQueueItemAggregate")
                command.CommandType = CommandType.StoredProcedure
                connection.Execute(command)
            End Using
        End Using
    End Sub

    <SecuredMethod(True)>
    Public Sub ClearInternalAuthTokens() Implements IServer.ClearInternalAuthTokens
        CheckPermissions()
        Using connection = GetConnection()
            Using command = New SqlCommand("usp_clearinternalauthtokens")
                command.CommandType = CommandType.StoredProcedure
                connection.Execute(command)
            End Using
        End Using
    End Sub

    Private Function IsActiveQueueControllable(con As IDatabaseConnection, queue As (processId As Guid, resourceGroupId As Guid)) As Boolean
        Return IsActiveQueueControllable(con, queue.processId, queue.resourceGroupId)
    End Function

    Private Function IsActiveQueueControllable(con As IDatabaseConnection, processId As Guid, resourceGroupId As Guid) As Boolean
        If processId = Guid.Empty OrElse
           resourceGroupId = Guid.Empty Then Return False

        If Not GetEffectiveMemberPermissionsForProcess(con, processId).
                HasPermission(mLoggedInUser, Permission.ProcessStudio.ImpliedExecuteProcess) Then Return False

        For Each resourceMember In GetGroup(con, resourceGroupId, True, Nothing, True).FlattenedContents(Of clsSortedSet(Of IGroupMember))(False)
            If Not GetEffectiveMemberPermissionsForResource(con, resourceMember.IdAsGuid).
                    HasPermission(mLoggedInUser, Permission.Resources.ControlResource) Then Return False
        Next

        Return True
    End Function

    Private Function GetActiveQueuesInfo(connection As IDatabaseConnection) _
        As List(Of (Id As Integer, processId As Guid, resourceGroupId As Guid))
        Dim results = New List(Of (Integer, Guid, Guid))
        Dim sql = "select
                        q.[ident],
                        q.[processid],
                        q.[resourcegroupid]
                    from BPAWorkQueue q
                    where
                        q.[processid] is not null
                        and q.[resourcegroupid] is not null"

        Using cmd As New SqlCommand(sql)
            Using reader = connection.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    Dim item = (prov.GetValue("ident", 0),
                                prov.GetGuid("processId"),
                                prov.GetGuid("resourcegroupId"))
                    results.Add(item)
                End While
            End Using
        End Using
        Return results
    End Function

    <SecuredMethod(Permission.ControlRoom.ManageQueuesFullAccess,
                   Permission.ControlRoom.ManageQueuesReadOnly)>
    Public Function GetControllableActiveQueueIds() As ICollection(Of Integer) Implements IServer.GetControllableActiveQueueIds
        CheckPermissions()
        Using connection = GetConnection()
            Dim queues = GetActiveQueuesInfo(connection).Select(Function(x) x.Id).ToList()
            Return queues.Where(Function(x) IsActiveQueueControllable(x, connection)).ToList()
        End Using
    End Function

End Class
