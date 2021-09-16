

Imports System.Data.SqlClient

Imports BluePrism.BPCoreLib.Data
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Data
Imports BluePrism.Server.Domain.Models

' Partial class which separates the env locking from the rest of the clsServer
' methods just in order to keep the file size down to a sane level and make it
' easier to actually find functions
Partial Public Class clsServer

#Region " Common methods "

    ''' <summary>
    ''' Checks if a lock with the given name exists on the given connection.
    ''' </summary>
    ''' <param name="con">The connection to the database to use.</param>
    ''' <param name="name">The name of the lock to check if it exists.</param>
    ''' <returns>True if the given lock exists, false otherwise.</returns>
    Private Function CheckLockExists(ByVal con As IDatabaseConnection, ByVal name As String) As Boolean
        Dim cmd As New SqlCommand("select 1 from BPAEnvLock where name = @name")
        cmd.Parameters.AddWithValue("@name", name)
        Return (con.ExecuteReturnScalar(cmd) IsNot Nothing)
    End Function

#End Region

#Region " Acquire Lock "

    ''' <inheritdoc />
    <SecuredMethod(True)>
    Public Function AcquireEnvLock(
     ByVal name As String,
     ByVal preferredToken As String,
     ByVal sessionIdentifier As SessionIdentifier,
     ByVal comment As String,
     ByVal forceLockRelease As Integer) As String Implements IServer.AcquireEnvLock
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction(IsolationLevel.Serializable)
            Dim lock = AcquireEnvLock(con, name, preferredToken, sessionIdentifier, comment)

            If lock.token = "" AndAlso forceLockRelease <> 0 AndAlso lock.locktime.AddSeconds(forceLockRelease) < DateTime.UtcNow Then
                ReleaseEnvLock(con, name, Nothing, comment, sessionIdentifier, False)
                lock = AcquireEnvLock(con, name, preferredToken, sessionIdentifier, comment)
            End If

            con.CommitTransaction()
            Return lock.token
        End Using

    End Function

    Private Function AcquireEnvLock(
     ByVal con As IDatabaseConnection,
     ByVal name As String,
     ByVal preferredToken As String,
     ByVal sessionIdentifier As SessionIdentifier,
     ByVal comment As String) As (token As String, locktime As DateTime)

        If name = "" Then
            Throw New ArgumentException(My.Resources.clsServer_ALockNameMustBeProvided)
        End If

        Dim sessionIdColumnName = If(sessionIdentifier Is Nothing,
                                               "sessionid",
                                               GetSessionIdColumnName(sessionIdentifier.SessionIdentifierType))

        Dim sql As String = "DECLARE @tok VARCHAR(255)
                                    ,@locktime DATETIME;

                                IF @token IS NULL
                                    SET @token = cast(newid() AS VARCHAR(255));

                                IF NOT EXISTS (
                                        SELECT 1
                                        FROM BPAEnvLock WITH (
                                                ROWLOCK
                                                ,UPDLOCK
                                                )
                                        WHERE name = @name
                                        )
                                BEGIN /* By definition, it cannot be locked, so just create it     * locked and return the token */
                                    INSERT INTO BPAEnvLock (
                                        name
                                        ,token
                                        ," + sessionIdColumnName + "
                                        ,locktime
                                        ,comments
                                        )
                                    VALUES (
                                        @name
                                        ,@token
                                        ,@sessionid
                                        ,getutcdate()
                                        ,@comments
                                        );

                                    SET @tok = @token;
                                        SET @locktime = NULL;
                                END
                                ELSE
                                BEGIN /* Check if it's unlocked first. */
                                    IF EXISTS (
                                            SELECT 1
                                            FROM BPAEnvLock
                                            WHERE name = @name
                                                AND (
                                                    token IS NULL
                                                    OR " + sessionIdColumnName + " = @sessionid
                                                    )
                                            )
                                    BEGIN
                                        UPDATE BPAEnvLock
                                        SET token = @token
                                            ," + sessionIdColumnName + " = @sessionid
                                            ,locktime = getutcdate()
                                            ,comments = @comments
                                        WHERE name = @name;

                                        SET @tok = @token;
                                    END

                                    ELSE

                                    BEGIN
                                    set @locktime = (SELECT locktime
                                            FROM BPAEnvLock
                                            WHERE name = @name);

                                    END

                                END /* So at this point, @tok will either be the token used to acquire  * the lock, or null indicating that it was already locked. */

                                SELECT @tok as token
                                      ,@locktime as locktime;"

        Dim cmd As New SqlCommand(sql
        )
        With cmd.Parameters
            .AddWithValue("@name", name)
            .AddWithValue("@token", IIf(preferredToken = "", DBNull.Value, preferredToken))
            .AddWithValue("@sessionid", If(sessionIdentifier Is Nothing, DBNull.Value, CObj(sessionIdentifier.Id)))
            .AddWithValue("@comments", IIf(comment = "", DBNull.Value, comment))
        End With

        Using reader = con.ExecuteReturnDataReader(cmd)
            reader.Read()
            Dim provider As New ReaderDataProvider(reader)

            Dim token = provider.GetString("token")
            Dim lockTime = provider.GetValue("locktime", DateTime.UtcNow)

            If token Is Nothing Then
                token = String.Empty
            End If

            Return (token, lockTime)

        End Using
    End Function

#End Region

#Region " Release Lock "

    ''' <inheritdoc />
    <SecuredMethod(True)>
    Public Sub ReleaseEnvLock(
     name As String, token As String, comment As String, sessionIdentifier As SessionIdentifier, keepLock As Boolean) Implements IServer.ReleaseEnvLock
        CheckPermissions()
        Using connection = GetConnection()
            ReleaseEnvLock(connection, name, token, comment, sessionIdentifier, keepLock)
        End Using
    End Sub

    <SecuredMethod(Permission.SystemManager.Workflow.EnvironmentLocking)>
    Public Sub ManualReleaseEnvLock(name As String, token As String, comment As String) Implements IServer.ManualReleaseEnvLock
        CheckPermissions()
        Using connection = GetConnection()
            connection.BeginTransaction()
            ReleaseEnvLock(connection, name, token, comment, Nothing, True)
            AuditRecordEnvironmentLockManualUnlock(connection, EnvironmentLockEventCode.Unlock, name, comment)
            connection.CommitTransaction()
        End Using
    End Sub

    ''' <inheritdoc />
    Private Sub ReleaseEnvLock(con As IDatabaseConnection,
      name As String, token As String, comment As String, sessionIdentifier As SessionIdentifier, keepLock As Boolean)

        If Not CheckLockExists(con, name) Then
            Throw New NoSuchLockException(
             My.Resources.clsServer_NoLockWithTheName0ExistsInThisEnvironment, name)
        End If

        Dim cmd As New SqlCommand()
        With cmd.Parameters
            .AddWithValue("@name", name)
            ' We want to make sure that @token isn't null for the following check
            .AddWithValue("@token", IIf(token Is Nothing, "", token))
            .AddWithValue("@comments", IIf(comment = "", DBNull.Value, comment))
            If sessionIdentifier IsNot Nothing Then
                .AddWithValue("@sessionid", sessionIdentifier.Id)
            End If
        End With

        ' If we have a token, check it.
        ' Otherwise, this is a 'force release' which releases the lock regardless of
        ' the token.
        If token <> "" Then
            cmd.CommandText =
             " select " &
             "  case " &
             "    when token is null then 1 " &
             "    when token = @token then 2 " &
             "    else 0 " &
             "  end" &
             " from BPAEnvLock where name = @name"

            Dim state As Integer = CInt(con.ExecuteReturnScalar(cmd))

            ' "state" represents one of the following :-
            ' 0 => the user has an invalid token. Stop. Error time.
            ' 1 => the lock is currently free (return now)
            ' 2 => the user has the correct token - continue to release the lock
            Select Case state

                Case 0
                    Throw New IncorrectLockTokenException(
                     "The token provided does not match up with the token held on the lock '{0}'",
                     name)

                Case 1
                    Return

                Case Else ' Continue...

            End Select
            ' ie. the token 
        End If

        If keepLock Then
            cmd.CommandText =
            " update BPAEnvLock set" &
            "   token = null," &
            "   sessionid = null," &
            "   digitalworkersessionid = null," &
            "   locktime = null," &
            "   comments = @comments" &
            " where name = @name" &
            If(sessionIdentifier IsNot Nothing, " and " + GetSessionIdColumnName(sessionIdentifier.SessionIdentifierType) + " = @sessionid", "")
        Else
            cmd.CommandText =
            " delete from BPAEnvLock" &
            " where name = @name" &
            If(sessionIdentifier IsNot Nothing, " and " + GetSessionIdColumnName(sessionIdentifier.SessionIdentifierType) + " = @sessionid", "")
        End If

        con.Execute(cmd)

    End Sub

#End Region

#Region " Release Multiple Locks "

    ''' <inheritdoc />
    <SecuredMethod(True)>
    Public Sub ReleaseAllEnvLocks(
     token As String, comment As String, sessionIdentifier As SessionIdentifier, keepLock As Boolean) Implements IServer.ReleaseAllEnvLocks
        CheckPermissions()
        Using connection = GetConnection()
            ReleaseAllEnvLocks(connection, token, comment, sessionIdentifier, keepLock)
        End Using
    End Sub

    Private Sub ReleaseAllEnvLocks(con As IDatabaseConnection,
      token As String, comment As String, sessionIdentifier As SessionIdentifier, keeplock As Boolean)

        Dim cmd As SqlCommand
        If keeplock Then
            cmd = New SqlCommand(
            " update BPAEnvLock set " &
            "   token = null, " &
            "   sessionid = null, " &
            "   digitalworkersessionid = null, " &
            "   locktime = null, " &
            "   comments = @comments " &
            " where token = @token" &
            If(sessionIdentifier IsNot Nothing, " and " + GetSessionIdColumnName(sessionIdentifier.SessionIdentifierType) + " = @sessionid", "")
            )
            With cmd.Parameters
                .AddWithValue("@token", token)
                .AddWithValue("@comments", IIf(comment = "", DBNull.Value, comment))
                If sessionIdentifier IsNot Nothing Then
                    .AddWithValue("@sessionid", sessionIdentifier.Id)
                End If

            End With
            con.Execute(cmd)
        Else
            cmd = New SqlCommand(
            " delete from BPAEnvLock " &
            " where token = @token" &
            If(sessionIdentifier IsNot Nothing, " and " + GetSessionIdColumnName(sessionIdentifier.SessionIdentifierType) + " = @sessionid", "")
            )
            With cmd.Parameters
                .AddWithValue("@token", token)
                If sessionIdentifier IsNot Nothing Then
                    .AddWithValue("@sessionid", sessionIdentifier.Id)
                End If
            End With
        End If
        con.Execute(cmd)
    End Sub

    ''' <inheritdoc />
    <SecuredMethod(True)>
    Public Sub ReleaseEnvLocksForSession(ByVal sessionIdentifier As SessionIdentifier) Implements IServer.ReleaseEnvLocksForSession
        CheckPermissions()
        Using con = GetConnection()
            ReleaseEnvLocksForSession(con, sessionIdentifier.Id, sessionIdentifier.SessionIdentifierType)
        End Using
    End Sub

    Private Sub ReleaseEnvLocksForSession(ByVal con As IDatabaseConnection, ByVal sessionIdentifier As Guid, sessIdentiferType As SessionIdentifierType)

        Dim sessionIdColumnName = GetSessionIdColumnName(sessIdentiferType)

        Dim cmd As New SqlCommand(
            "IF EXISTS (SELECT 1 FROM [BPAEnvLock] WHERE " + sessionIdColumnName + " = @sessionid AND (token IS NOT NULL OR " + sessionIdColumnName + " IS NOT NULL OR locktime IS NOT NULL OR comments <> @comments))" &
            " BEGIN" &
            " update BPAEnvLock set" &
            "   token = null," &
            "   " + sessionIdColumnName + " = null," &
            "   locktime = null," &
            "   comments = @comments" &
            " where " + sessionIdColumnName + " = @sessionid" &
            " END"
            )
        With cmd.Parameters
            .AddWithValue("@comments", My.Resources.clsServer_AutomaticallyReleasedWhenSessionFinished)
            .AddWithValue("@sessionid", sessionIdentifier)
        End With
        con.Execute(cmd)
    End Sub

    Private Function GetSessionIdColumnName(type As SessionIdentifierType) As String
        Return If(type = SessionIdentifierType.DigitalWorker, "digitalworkersessionid", "sessionid")
    End Function

#End Region

#Region " Is Lock Held? "


    ''' <summary>
    ''' <para>
    ''' Checks if the given lock is held. The check is subtly different depending on
    ''' whether the token is provided or not.
    ''' </para><para>
    ''' If the token is provided, it will check if the specified lock is held by that
    ''' token. If the lock exists and is currently held against the specified token,
    ''' this will return true. Otherwise, it will return false.
    ''' </para><para>
    ''' If the given token is null, it will check if the specified lock is held
    ''' <em>at all</em>. So, if the lock exists and is currently held by anyone, this
    ''' will return true. Otherwise it will return false.
    ''' </para>
    ''' </summary>
    ''' <param name="name">The name of the lock to check.</param>
    ''' <param name="token">The token to check the lock against - if this is null
    ''' then this method just checks if the specified lock is held at all.</param>
    ''' <param name="comment">Output parameter which holds the comment held on the
    ''' lock object, regardless of whether the lock is currently held or not.
    ''' If the lock doesn't exist, or no comment exists on the lock record, this
    ''' will be set to null on exit of the method.</param>
    ''' <returns>True if the specified environment lock is held and if the token
    ''' was not provided or, if it was provided, if it matched the token associated
    ''' with the lock record. False in all other cases.</returns>
    <SecuredMethod(True)>
    Public Function IsEnvLockHeld(
     ByVal name As String, ByVal token As String, ByRef comment As String) As Boolean Implements IServer.IsEnvLockHeld
        CheckPermissions()
        Using con = GetConnection()
            Return IsEnvLockHeld(con, name, token, comment)
        End Using
    End Function


    ''' <summary>
    ''' <para>
    ''' Checks if the given lock is held. The check is subtly different depending on
    ''' whether the token is provided or not.
    ''' </para><para>
    ''' If the token is provided, it will check if the specified lock is held by that
    ''' token. If the lock exists and is currently held against the specified token,
    ''' this will return true. Otherwise, it will return false.
    ''' </para><para>
    ''' If the given token is null, it will check if the specified lock is held
    ''' <em>at all</em>. So, if the lock exists and is currently held by anyone, this
    ''' will return true. Otherwise it will return false.
    ''' </para>
    ''' </summary>
    ''' <param name="con">The connection to check the environment lock on.</param>
    ''' <param name="name">The name of the lock to check.</param>
    ''' <param name="token">The token to check the lock against - if this is null
    ''' then this method just checks if the specified lock is held at all.</param>
    ''' <param name="comment">Output parameter which holds the comment held on the
    ''' lock object, regardless of whether the lock is currently held or not.
    ''' If the lock doesn't exist, or no comment exists on the lock record, this
    ''' will be set to null on exit of the method.</param>
    ''' <returns>True if the specified environment lock is held and if the token
    ''' was not provided or, if it was provided, if it matched the token associated
    ''' with the lock record. False in all other cases.</returns>
    Private Function IsEnvLockHeld(
     ByVal con As IDatabaseConnection,
     ByVal name As String,
     ByVal token As String,
     ByRef comment As String) As Boolean

        ' Normalise the token to a null 
        If String.IsNullOrEmpty(token) Then token = Nothing

        Dim cmd As New SqlCommand(
         " select e.comments, e.token" &
         " from BPAEnvLock e" &
         " where e.name = @name"
        )
        With cmd.Parameters
            .AddWithValue("@name", name)
            .AddWithValue("@token", IIf(token Is Nothing, DBNull.Value, token))
        End With
        Dim reader = con.ExecuteReturnDataReader(cmd)

        ' If there's nothing to read then the lock doesn't exist; no comment - return false.
        If Not reader.Read() Then
            comment = Nothing
            Return False
        End If
        Dim prov As New ReaderDataProvider(reader)

        ' Get the comment into the byref param no matter what.
        comment = prov.GetString("comments")

        Dim currToken As String = prov.GetString("token")

        ' If we were given a token, check if the lock is currently held by that token.
        If token IsNot Nothing Then Return (token = currToken)

        ' Otherwise, we just need to know if it is locked at all.
        Return (currToken IsNot Nothing)

    End Function

#End Region

#Region " UI Support "

    ''' <summary>
    ''' Searches the environment locks using the given filter.
    ''' </summary>
    ''' <param name="flt">The filter to use to search the locks</param>
    ''' <returns>A collection of lock objects matching the given filter.</returns>
    <SecuredMethod(Permission.SystemManager.Workflow.EnvironmentLocking)>
    Public Function SearchEnvLocks(ByVal flt As clsLockFilter) As (filteredLocks As ICollection(Of clsLockInfo), totalAmountOfRows As Integer) _
                                                    Implements IServer.SearchEnvLocks
        CheckPermissions()
        Using con = GetConnection()
            Return SearchEnvLocks(con, flt)
        End Using
    End Function

    ''' <summary>
    ''' Searches the environment locks using the given filter.
    ''' </summary>
    ''' <param name="con">The connection over which the locks should be searched
    ''' </param>
    ''' <param name="flt">The filter to use to search the locks</param>
    ''' <returns>A collection of lock objects matching the given filter.</returns>
    Private Function SearchEnvLocks(ByVal con As IDatabaseConnection, ByVal flt As clsLockFilter) _
     As (filteredLocks As ICollection(Of clsLockInfo), totalAmountOfRows As Integer)
        Dim cmd As New SqlCommand()
        Dim sb As New StringBuilder(
         " select" &
         "   e.name," &
         "   e.token," &
         "   e.sessionid, " &
         "   s.processid, " &
         "   s.processname," &
         "   s.runningresourceid as ""resourceid""," &
         "   s.runningresourcename as ""resourcename""," &
         "   s.starterusername as ""username""," &
         "   e.locktime," &
         "   e.comments as ""comment""" &
         " from BPAEnvLock e" &
         "   left join BPVSessionInfo s on e.sessionid = s.sessionid"
        )

        ' Add the filter stuff.
        If flt IsNot Nothing Then
            Dim prefix As String = "where"
            If flt.State <> clsLockInfo.LockState.None Then
                sb.AppendFormat(" {0} e.token is {1} null",
                 prefix, IIf(flt.State = clsLockInfo.LockState.Held, "not", ""))
                prefix = "and"
            End If
            If flt.Name <> "" Then
                sb.AppendFormat(" {0} e.name like @name", prefix)
                prefix = "and"
                cmd.Parameters.AddWithValue("@name", "%" & flt.Name & "%")
            End If
            If flt.Resource <> "" Then
                sb.AppendFormat(" {0} s.runningresourcename like @resource", prefix)
                prefix = "and"
                cmd.Parameters.AddWithValue("@resource", "%" & flt.Resource & "%")
            End If
            If flt.Process <> "" Then
                sb.AppendFormat(" {0} s.processname like @process", prefix)
                prefix = "and"
                cmd.Parameters.AddWithValue("@process", "%" & flt.Process & "%")

            End If
            If flt.LockTime IsNot Nothing Then
                sb.AppendFormat(" {0} e.locktime between @starttime and @endtime", prefix)
                prefix = "and"
                cmd.Parameters.AddWithValue("@starttime", clsDBConnection.UtilDateToSqlDate(flt.LockTime.StartTime))
                cmd.Parameters.AddWithValue("@endtime", clsDBConnection.UtilDateToSqlDate(flt.LockTime.EndTime))
            End If
            If flt.LastComment <> "" Then
                sb.AppendFormat(" {0} e.comments like @comments", prefix)
                prefix = "and"
                cmd.Parameters.AddWithValue("@comments", "%" & flt.LastComment & "%")
            End If
        End If
        cmd.CommandText = sb.ToString()

        Dim locks As New List(Of clsLockInfo)
        Dim totalAmountOfRows = 0

        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As IDataProvider = New ReaderDataProvider(reader)
            While reader.Read()
                locks.Add(New clsLockInfo(prov))
                totalAmountOfRows += 1
            End While
        End Using

        'We have to do this now because we need to know the amount of locks in total for the number of pages to be calculated
        'and displayed in the UI.
        locks = locks.Skip((flt.CurrentPage - 1) * flt.RowsPerPage).Take(flt.RowsPerPage).ToList()

        ' For any locks with session information, check if user can access logs
        locks.ForEach(Sub(l)
                          If l.SessionId <> Guid.Empty AndAlso
                            GetEffectiveMemberPermissionsForProcess(con, l.ProcessId).HasAnyPermissions(mLoggedInUser) AndAlso
                            GetEffectiveMemberPermissionsForResource(con, l.ResourceId).HasPermission(
                                mLoggedInUser, Permission.Resources.ImpliedViewResource) Then
                              l.CanViewSessionLog = True
                          End If
                      End Sub)

        Return (locks, totalAmountOfRows)
    End Function

    ''' <summary>
    ''' Deletes the lock records with the given names.
    ''' </summary>
    ''' <param name="names">The names of the lock records which should be deleted.
    ''' </param>
    <SecuredMethod(Permission.SystemManager.Workflow.EnvironmentLocking)>
    Public Sub DeleteLocks(ByVal names As ICollection(Of String)) Implements IServer.DeleteLocks
        CheckPermissions()
        Using con = GetConnection()
            DeleteLocks(con, names)
        End Using
    End Sub

    ''' <summary>
    ''' Deletes the lock records with the given names.
    ''' </summary>
    ''' <param name="con">The connection over which the records should be deleted.
    ''' </param>
    ''' <param name="names">The names of the lock records which should be deleted.
    ''' </param>
    Private Sub DeleteLocks(ByVal con As IDatabaseConnection, ByVal names As ICollection(Of String))

        If names Is Nothing OrElse names.Count = 0 Then Return

        Dim cmd As New SqlCommand()
        Dim sb As New StringBuilder("delete from BPAEnvLock where name in (")
        Dim i As Integer = 0
        For Each name As String In names
            i += 1
            sb.AppendFormat("@n{0},", i)
            cmd.Parameters.AddWithValue("@n" & i, name)
        Next
        sb.Length -= 1
        sb.Append(") and token is null")
        cmd.CommandText = sb.ToString()
        con.Execute(cmd)

    End Sub

#End Region

#Region "Has Lock Expired?"
    ''' <summary>
    ''' This Function compares the locktime, added to the expiry time, to the
    ''' current time to check whether the lock has expired.
    ''' </summary>
    ''' <param name="name">The name of the lock to check.</param>
    ''' <param name="token">The token to check the lock against - if this is null
    ''' then this method just checks if the specified lock is held at all.</param>
    ''' <param name="expiryTime">The time in seconds before the lock should have
    ''' expired.</param>
    ''' <returns>Returns true if the locktime exists and is after the currrent
    ''' datetime.</returns>
    <SecuredMethod(True)>
    Public Function HasEnvLockExpired(
     name As String,
     token As String,
     expiryTime As Integer) As Boolean Implements IServer.HasEnvLockExpired
        CheckPermissions()
        Using con = GetConnection()
            Return HasEnvLockExpired(con, name, token, expiryTime)
        End Using
    End Function


    ''' <summary>
    ''' This Function compares the locktime, added to the expiry time, to the
    ''' current time to check whether the lock has expired.
    ''' </summary>
    ''' <param name="con">The connection to check the environment lock on.</param>
    ''' <param name="name">The name of the lock to check.</param>
    ''' <param name="token">The token to check the lock against - if this is null
    ''' then this method just checks if the specified lock is held at all.</param>
    ''' <param name="expiryTime">The time in seconds before the lock should have
    ''' expired.</param>
    ''' <returns>Returns true if the locktime exists and is after the currrent
    ''' datetime.</returns>
    Private Function HasEnvLockExpired(
     con As IDatabaseConnection,
     name As String,
     token As String,
     expiryTime As Integer) As Boolean

        ' Normalise the token to a null 
        If String.IsNullOrEmpty(token) Then token = Nothing

        Dim cmd As New SqlCommand(
         " select e.locktime" &
         " from BPAEnvLock e" &
         " where e.name = @name"
        )
        With cmd.Parameters
            .AddWithValue("@name", name)
            .AddWithValue("@token", IIf(token Is Nothing, DBNull.Value, token))
        End With
        Dim reader = con.ExecuteReturnDataReader(cmd)

        ' If there's nothing to read then the lock doesn't exist; 
        ' It must have expired or been deleted - return true.
        If Not reader.Read() Then
            Return True
        End If
        Dim prov As New ReaderDataProvider(reader)

        ' Get the locktime
        Dim lockTime = prov.GetValue("locktime", DateTime.MinValue)

        'If the locktime is populated, check if the Then time has expired.
        If Not lockTime = DateTime.MinValue Then
            If lockTime.AddSeconds(expiryTime) > DateTime.UtcNow Then Return False
        End If

        Return True

    End Function

    Private Function GetEnvironmentLockWithForceReleaseOfExpired(environmentLockName As String, expiryTimeInSeconds As Integer) As String

        If IsEnvLockHeld(environmentLockName, Nothing, Nothing) Then
            If HasEnvLockExpired(environmentLockName,
                                 Nothing,
                                 expiryTimeInSeconds) Then
                ReleaseEnvLock(environmentLockName, Nothing, "Lock Expired.", Nothing, True)
            End If
        End If

        Return AcquireEnvLock(environmentLockName, Nothing, Nothing, Environment.MachineName, expiryTimeInSeconds)
    End Function
#End Region

End Class
