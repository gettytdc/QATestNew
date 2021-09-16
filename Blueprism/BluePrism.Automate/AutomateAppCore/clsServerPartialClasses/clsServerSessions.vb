Imports System.Data.SqlClient
Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.Sessions
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Core.Resources
Imports BluePrism.Data
Imports BluePrism.Server.Domain.Models
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.Server.Domain.Models.DataFilters
Imports BluePrism.Utilities.Functional
Imports Autofac
Imports BluePrism.AutomateAppCore.clsServerPartialClasses
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateAppCore.Sessions
Imports BluePrism.Server.Domain.Models.Extensions
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.Server.Domain.Models.DataFilters.MteSqlGenerator

Partial Public Class clsServer

    Private Const SqlMinDateTime = "convert(datetime, 0)"
    Private Const SqlMaxDateTime = "cast('9999-12-31T23:59:59Z' as datetime)"

    ''' <summary>
    ''' Checks if a 'stop request' has been made for the session with the given
    ''' identity, indicating that the request has been acknowledged if such a request
    ''' has been made.
    ''' </summary>
    ''' <param name="sessionNo">The identity of the session to check for a stop
    ''' request.</param>
    ''' <returns>True if a stop request has been entered for this session; False if
    ''' no such stop request is found, or if the session number did not correspond to
    ''' a valid session.</returns>
    <SecuredMethod(True)>
    Public Function IsStopRequested(sessionNo As Integer) As Boolean Implements IServer.IsStopRequested
        CheckPermissions()
        Return IsStopRequested(sessionNo, True)
    End Function

    ''' <summary>
    ''' Checks if a 'stop request' has been made for the session with the given
    ''' identity.
    ''' </summary>
    ''' <param name="sessionNo">The identity of the session to check for a stop
    ''' request.</param>
    ''' <param name="markAcknowledged">True to set an flag indicating that the
    ''' request has been acknowledged; False to leave the flag untouched. Note that
    ''' the ack flag is only set once - ie. if it is already set, it will not be
    ''' overwritten by this method.</param>
    ''' <returns>True if a stop request has been entered for this session; False if
    ''' no such stop request is found, or if the session number did not correspond to
    ''' a valid session.</returns>
    <SecuredMethod(True)>
    Public Function IsStopRequested(
     sessionNo As Integer, markAcknowledged As Boolean) As Boolean Implements IServer.IsStopRequested
        CheckPermissions()
        Using con = GetConnection()
            ' If we're marking acknowledged, we want this in a transaction; otherwise
            ' it doesn't really matter, since it's a readonly operation
            If markAcknowledged Then con.BeginTransaction()
            Dim req As Boolean = IsStopRequested(con, sessionNo, markAcknowledged)
            ' As above, so below
            If markAcknowledged Then con.CommitTransaction()
            Return req
        End Using
    End Function

    ''' <summary>
    ''' Checks if a 'stop request' has been made for the session with the given
    ''' identity.
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="sessionNo">The identity of the session to check for a stop
    ''' request.</param>
    ''' <param name="markAcknowledged">True to set an flag indicating that the
    ''' request has been acknowledged; False to leave the flag untouched. Note that
    ''' the ack flag is only set once - ie. if it is already set, it will not be
    ''' overwritten by this method.</param>
    ''' <returns>True if a stop request has been entered for this session; False if
    ''' no such stop request is found, or if the session number did not correspond to
    ''' a valid session.</returns>
    Private Function IsStopRequested(con As IDatabaseConnection,
     sessionNo As Integer, markAcknowledged As Boolean) As Boolean
        ' Get the stoprequested date/time
        Dim cmd As New SqlCommand(
            " select stoprequested from BPASession where sessionnumber = @sessno;")
        cmd.Parameters.AddWithValue("@sessno", sessionNo)
        Dim reqTime As Date = IfNull(con.ExecuteReturnScalar(cmd), Date.MinValue)

        ' If stop has been requested and we need to mark it as acknowledged, do so
        Dim marked As Boolean = False
        If reqTime <> Date.MinValue AndAlso markAcknowledged Then
            ' We want to ensure that we don't overwrite existing acks hence 'is null'
            cmd.CommandText =
                " update BPASession set" &
                "   stoprequestack = getutcdate()" &
                " where sessionnumber = @sessno and stoprequestack is null;"
            ' We can keep the existing parameter - @sessno

            marked = (con.ExecuteReturnRecordsAffected(cmd) > 0)

        End If

        ' Stop has been requested if we have a valid request time
        Return (reqTime <> Date.MinValue)
    End Function

    ''' <summary>
    ''' Requests a 'safe stop' of a session
    ''' </summary>
    ''' <param name="sessionNo">The identity of the session for which a stop request
    ''' should be made</param>
    ''' <returns>True if the stop request was made as a result of this call; False if
    ''' the stop request was not made by this call, either because a stop request has
    ''' already been made on the session, or no running session with the given
    ''' number was found.</returns>
    <SecuredMethod(Permission.Resources.ControlResource)>
    Public Function RequestStopSession(sessionNo As Integer) As Boolean Implements IServer.RequestStopSession
        CheckPermissions()
        Using con = GetConnection()
            Return RequestStopSession(con, sessionNo)
        End Using
    End Function

    ''' <summary>
    ''' Requests a 'safe stop' of a running session
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="sessionNo">The identity of the session for which a stop request
    ''' should be made</param>
    ''' <returns>True if the stop request was made as a result of this call; False if
    ''' the stop request was not made by this call, either because a stop request has
    ''' already been made on the session, or no running session with the given
    ''' number was found.</returns>
    ''' <remarks>This will have no effect if the session already has a stop request
    ''' made on it, or if it is not <see cref="SessionStatus.Running">running</see>
    ''' </remarks>
    Private Function RequestStopSession(con As IDatabaseConnection, sessionNo As Integer) As Boolean
        Dim cmd As New SqlCommand(
         " update BPASession set" &
         "   stoprequested = getutcdate()," &
         "   statusid = @stoppingstatus" &
         " where sessionnumber = @sessno and" &
         "   stoprequested is null and" &
         "   statusid = @runningstatus"
        )
        With cmd.Parameters
            .AddWithValue("@sessno", sessionNo)
            .AddWithValue("@runningstatus", SessionStatus.Running)
            .AddWithValue("@stoppingstatus", SessionStatus.StopRequested)
        End With

        Return (con.ExecuteReturnRecordsAffected(cmd) > 0)

    End Function

    ''' <summary>
    ''' Class to represent data about a session. This is intended for
    ''' user-readability rather than system use, since it retrieves everything back
    ''' as a name rather than an internal id. The only exception to this is the
    ''' session number which is used as an identifier for this class.
    ''' </summary>
    <Serializable()>
    <DataContract([Namespace]:="bp")>
    Public Class SessionData

        ' The integer session number for the session represented by this object..
        <DataMember>
        Private mSessionNumber As Integer

        ' The user who started the session.
        <DataMember>
        Private mUserName As String

        ' The resource on which the session is running.
        <DataMember>
        Private mResourceName As String

        ' The process which is running in this session.
        <DataMember>
        Private mProcessName As String

        Public Overrides Function Equals(ByVal o As Object) As Boolean
            Dim sd As SessionData = TryCast(o, SessionData)
            If sd Is Nothing Then Return False
            Return mSessionNumber = sd.mSessionNumber _
             AndAlso mUserName = sd.mUserName _
             AndAlso mResourceName = sd.mResourceName _
             AndAlso mProcessName = sd.mProcessName
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return &HEFFAD0BE Xor mSessionNumber
        End Function

        Public Overrides Function ToString() As String
            Return ToBuffer(New StringBuilder(64)).ToString()
        End Function

        Public Function ToBuffer(ByVal sb As StringBuilder) As StringBuilder
            Return sb.Append(String.Format(My.Resources.SessionData_SessionNo0ProcessName1OnResourceName2ByUserName3, mSessionNumber, mProcessName, mResourceName, mUserName))
        End Function


        ''' <summary>
        ''' Creates a new session data object with the given values.
        ''' </summary>
        ''' <param name="sessNo">The integer session number for the session.</param>
        ''' <param name="userName">The user who created/started the session</param>
        ''' <param name="resourceName">The resource on which the session exists
        ''' </param>
        ''' <param name="processName">The process to be run in the session.</param>
        Public Sub New(ByVal sessNo As Integer, ByVal userName As String,
         ByVal resourceName As String, ByVal processName As String)
            mSessionNumber = sessNo
            mUserName = userName
            mResourceName = resourceName
            mProcessName = processName
        End Sub

        ''' <summary>
        ''' Creates a new session data object using data from the given provider.
        ''' </summary>
        ''' <param name="prov">The provider for the data. This should contain data under
        ''' the names :<list>
        ''' <item>"sessionnumber" (int)</item>
        ''' <item>"username" (string)</item>
        ''' <item>"resourcename" (string)</item>
        ''' <item>"processname" (string)</item>
        ''' </list></param>
        Public Sub New(ByVal prov As IDataProvider)
            Me.New(
             prov.GetValue("sessionnumber", 0), prov.GetString("username"),
             prov.GetString("resourcename"), prov.GetString("processname")
            )
        End Sub

        ''' <summary>
        ''' The integer session number for the session.
        ''' </summary>
        Public ReadOnly Property SessionNumber() As Integer
            Get
                Return mSessionNumber
            End Get
        End Property

        ''' <summary>
        ''' The user who created / started the session.
        ''' </summary>
        Public ReadOnly Property UserName() As String
            Get
                Return mUserName
            End Get
        End Property

        ''' <summary>
        ''' The resource on which the session was created.
        ''' </summary>
        Public ReadOnly Property ResourceName() As String
            Get
                Return mResourceName
            End Get
        End Property

        ''' <summary>
        ''' The process to be run / running in the session.
        ''' </summary>
        Public ReadOnly Property ProcessName() As String
            Get
                Return mProcessName
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Gets the session details for the session with the given ID.
    ''' </summary>
    ''' <param name="sessionNo">The sessionnumber value for which the session data is
    ''' required.</param>
    ''' <returns>A SessionData object containing the data for the specified session,
    ''' or null if the session was not found on the database.</returns>
    <SecuredMethod(Permission.Resources.ViewResource, Permission.Resources.ViewResourceScreenCaptures, Permission.Resources.ConfigureResource, Permission.Resources.ControlResource)>
    Public Function GetSessionDetails(ByVal sessionNo As Integer) As SessionData Implements IServer.GetSessionDetails
        CheckPermissions()
        If sessionNo = 0 Then Return Nothing ' Just to avoid weird errors later
        Using con = GetConnection()
            Return GetSessionDetails(con, Nothing, sessionNo)
        End Using
    End Function

    ''' <summary>
    ''' Gets the session details for the session with the given ID.
    ''' </summary>
    ''' <param name="sessionId">The ID for which the session data is required.
    ''' </param>
    ''' <returns>A SessionData object containing the data for the specified session,
    ''' or null if the session was not found on the database.</returns>
    <SecuredMethod(Permission.Resources.ViewResource, Permission.Resources.ViewResourceScreenCaptures, Permission.Resources.ConfigureResource, Permission.Resources.ControlResource)>
    Public Function GetSessionDetails(ByVal sessionId As Guid) As SessionData Implements IServer.GetSessionDetails
        CheckPermissions()
        If sessionId = Nothing Then Return Nothing ' Just to avoid weird errors later
        Using con = GetConnection()
            Return GetSessionDetails(con, sessionId, Nothing)
        End Using
    End Function

    ''' <summary>
    ''' Gets the session details for the session with the given ID or number
    ''' </summary>
    ''' <param name="con">The connection to the database to use.</param>
    ''' <param name="sessionId">The ID for which the session data is required.
    ''' </param>
    ''' <param name="sessionNo">The session number for which the session data is
    ''' required</param>
    ''' <returns>A SessionData object containing the data for the specified session,
    ''' or null if the session was not found on the database.</returns>
    ''' <remarks>Only one of <paramref name="sessionId"/> or
    ''' <paramref name="sessionNo"/> should be provided and at least one of them
    ''' should be.</remarks>
    ''' <exception cref="ArgumentException">If neither or both of
    ''' <paramref name="sessionId"/> or <paramref name="sessionNo"/> are provided.
    ''' </exception>
    Private Function GetSessionDetails(ByVal con As IDatabaseConnection,
     ByVal sessionId As Guid, ByVal sessionNo As Integer) As SessionData

        If sessionId = Nothing AndAlso sessionNo = Nothing Then _
         Throw New ArgumentException(My.Resources.clsServer_GetSessionDetailsRequiresAnIDOrNumber)

        If sessionId <> Nothing AndAlso sessionNo <> Nothing Then _
         Throw New ArgumentException(My.Resources.clsServer_GetSessionDetailsCannotHandleBothIDAndNumber)

        Dim cmd As New SqlCommand()
        cmd.CommandText =
         " select" &
         "   s.sessionnumber  as sessionnumber," &
         "   p.name           as processname," &
         "   isnull(u.username, '[' + u.systemusername + ']')" &
         "                    as username," &
         "   r.name           as resourcename," &
         "   s.statusid       as statusid" &
         " from BPASession s" &
         "   join BPAProcess p on s.processid = p.processid" &
         "   join BPAResource r on s.runningresourceid = r.resourceid" &
         "   join BPAUser u on s.starteruserid = u.userid" &
         " where (@sessionid is not null and s.sessionid = @sessionid)" &
         "   or (@sessionno is not null and s.sessionnumber = @sessionno)"

        With cmd.Parameters
            .AddWithValue("@sessionid",
             IIf(sessionId = Nothing, DBNull.Value, sessionId))
            .AddWithValue("@sessionno",
             IIf(sessionNo = Nothing, DBNull.Value, sessionNo))
        End With

        Using reader = con.ExecuteReturnDataReader(cmd)
            ' No such session? Return nothing. That'll learn 'em.
            If Not reader.Read() Then Return Nothing
            Return New SessionData(New ReaderDataProvider(reader))

        End Using

    End Function

    ''' <summary>
    ''' Gets the name of the resource which ran a session.
    ''' </summary>
    ''' <param name="sessId">The session ID for which the running resource name is
    ''' required.</param>
    ''' <returns>The name of the resource set as running the session, or an empty
    ''' string if no resource was found.</returns>
    <SecuredMethod(Permission.Resources.ViewResource, Permission.Resources.ViewResourceScreenCaptures, Permission.Resources.ConfigureResource, Permission.Resources.ControlResource)>
    Public Function GetSessionResourceName(sessId As Guid) As String Implements IServer.GetSessionResourceName
        CheckPermissions()
        Using con = GetConnection()
            Return GetSessionResourceName(con, sessId)
        End Using
    End Function

    ''' <summary>
    ''' Gets the name of the resource which ran a session.
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="sessId">The session ID for which the running resource name is
    ''' required.</param>
    ''' <returns>The name of the resource set as running the session, or an empty
    ''' string if no resource was found.</returns>
    Private Function GetSessionResourceName(con As IDatabaseConnection, sessId As Guid) As String
        Dim cmd As New SqlCommand(
            " select runningresourcename from BPVSessionInfo where sessionid = @id"
        )
        cmd.Parameters.AddWithValue("@id", sessId)
        Return IfNull(con.ExecuteReturnScalar(cmd), "")
    End Function


    ''' <summary>
    ''' Gets the ID of the resource that this session is set to run on. This is the
    ''' *actual* resource, regardless of pool status - i.e. if the session ran on a
    ''' pool this will be the pool member doing the running, not the pool itself.
    ''' </summary>
    ''' <param name="sessionID">The session id</param>
    ''' <returns>The name ID the resource that this session is set to run on
    ''' or Guid.Empty if no resource can be found</returns>
    <SecuredMethod(Permission.Resources.ViewResource, Permission.Resources.ViewResourceScreenCaptures, Permission.Resources.ConfigureResource, Permission.Resources.ControlResource)>
    Public Function GetSessionResourceID(ByVal sessionID As Guid) As Guid Implements IServer.GetSessionResourceID
        CheckPermissions()
        Dim con = GetConnection()
        Try
            Dim cmd As New SqlCommand("SELECT runningresourceid FROM BPASession WHERE sessionid = @SessionID")
            With cmd.Parameters
                .AddWithValue("@SessionID", sessionID)
            End With
            Return IfNull(con.ExecuteReturnScalar(cmd), Guid.Empty)

        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.clsServer_FailedToGetTheSessionSResourceID0, ex.Message))
        Finally
            con.Close()
        End Try
    End Function

    ''' <summary>
    ''' Changes the user responsible for a session
    ''' </summary>
    ''' <param name="sessionid">The session id</param>
    ''' <param name="userid">The user id</param>
    <SecuredMethod(Permission.Resources.ConfigureResource)>
    Public Sub SetSessionUserID(ByVal sessionid As Guid, ByVal userid As Guid) Implements IServer.SetSessionUserID
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("update BPASession set starteruserid = @UserID where sessionid = @SessionID")
            With cmd.Parameters
                .AddWithValue("@UserID", userid)
                .AddWithValue("@SessionID", sessionid.ToString)
            End With
            con.Execute(cmd)
        End Using

    End Sub

    ''' <summary>
    ''' Updates a session's status to 'Running'.
    ''' </summary>
    ''' <param name="gsessionid">The session id</param>
    ''' <param name="startDateTime">The date/time (UTC) that the session started</param>
    <SecuredMethod(Permission.Resources.ControlResource)>
    Public Sub SetPendingSessionRunning(gsessionid As Guid, startDateTime As DateTimeOffset) Implements IServer.SetPendingSessionRunning
        CheckPermissions()
        Using con = GetConnection()
            ' UPdate BPASession
            SetPendingSessionRunning(con, gsessionid, startDateTime)
            ' Record event for MI
            LogMISessionStart(con, gsessionid)
        End Using
    End Sub

    Private Sub SetPendingSessionRunning(connection As IDatabaseConnection,
                                         sessionid As Guid,
                                         startDateTime As DateTimeOffset)
        Dim cmd As New SqlCommand("update BPASession
set statusid = @statusid,
startdatetime = @startdatetime,
starttimezoneoffset = @starttimezoneoffset
where sessionid = @sessionid")
        With cmd.Parameters
            .AddWithValue("@statusid", SessionStatus.Running)
            .AddWithValue("@startdatetime", startDateTime.DateTime)
            .AddWithValue("@starttimezoneoffset", startDateTime.Offset.TotalSeconds)
            .AddWithValue("@sessionid", sessionid.ToString)
        End With
        connection.Execute(cmd)
    End Sub

    Private Sub CreateSession(
     connection As IDatabaseConnection,
     processId As Guid,
     queueIdent As Integer,
     starterUserId As Guid,
     startResourceId As Guid,
     runResourceId As Guid,
     status As SessionStatus,
     startDateTime As DateTimeOffset,
     sessionId As Guid,
     ByRef sessNo As Integer)

        Using cmd As New SqlCommand(
            " insert into BPASession " &
            " (sessionid,processId,queueid,starteruserid,starterresourceid," &
            "  runningresourceid,statusid,startdatetime,starttimezoneoffset)" &
            " VALUES " &
            " (@sessionid,@processid,@queueid,@starteruserid,@starterresourceid," &
            "  @runningresourceid,@status,@startdatetime,@starttimezoneoffset);" &
 _
            " select scope_identity();")

            With cmd.Parameters
                .AddWithValue("@sessionid", sessionId)
                .AddWithValue("@processid", processId)
                .AddWithValue("@queueid", IIf(queueIdent = 0, DBNull.Value, queueIdent))
                .AddWithValue("@starteruserid", starterUserId)
                .AddWithValue("@starterresourceid", startResourceId)
                .AddWithValue("@runningresourceid", runResourceId)
                .AddWithValue("@status", status)
                .AddWithValue("@startdatetime", startDateTime.DateTime)
                .AddWithValue("@starttimezoneoffset", startDateTime.Offset.TotalSeconds)
            End With

            sessNo = IfNull(connection.ExecuteReturnScalar(cmd), 0)
        End Using
    End Sub

    ''' <summary>
    ''' Writes a session record into the database.
    ''' </summary>
    ''' <param name="processId">The process id</param>
    ''' <param name="starterResourceId">The starting resource id</param>
    ''' <param name="startDateTime">The date/time (UTC) that the session started</param>
    ''' <param name="runningResourceId">The running resource id</param>
    ''' <param name="sessionId">The session id</param>
    <SecuredMethod(True)>
    Public Sub CreateDebugSession(
        processId As Guid, starterResourceId As Guid,
        runningResourceId As Guid, startDateTime As DateTimeOffset,
        sessionId As Guid, ByRef sessNo As Integer) Implements IServer.CreateDebugSession
        CheckPermissions()
        Using con = GetConnection()

            ' check that the user has the execute permission for this process
            Dim m = GetEffectiveMemberPermissionsForProcess(con, processId)
            If Not m.HasPermission(mLoggedInUser, Permission.ProcessStudio.ImpliedExecuteProcess) Then
                Throw New PermissionException(My.Resources.clsServer_CreateDebugSession_CannotCreateSessionAsUserDoesNotHaveExecutePermissionOnThisProcess)
            End If

            CreateSession(con, processId, 0, GetLoggedInUserId, starterResourceId,
                    runningResourceId, SessionStatus.Debugging, startDateTime, sessionId, sessNo)
        End Using
    End Sub

    ''' <summary>
    ''' Finishes a session in the database, either by updating the end time or by
    ''' deleting it when it has not produced any log data.
    ''' </summary>
    ''' <param name="gSessionID">The session id</param>
    ''' <param name="endDateTime">The date/time that the session finished</param>
    <SecuredMethod(True)>
    Public Sub FinishDebugSession(ByVal gSessionID As Guid, endDateTime As DateTimeOffset) Implements IServer.FinishDebugSession
        CheckPermissions()
        Using con = GetConnection()
            FinishDebugSession(con, gSessionID, endDateTime)
        End Using
    End Sub

    Private Sub FinishDebugSession(connection As IDatabaseConnection, ByVal gSessionID As Guid, endDateTime As DateTimeOffset, Optional newSessionStatus As SessionStatus = SessionStatus.All)
        Dim iSessionNumber = GetSessionNumber(gSessionID)
        Dim processId = GetProcessIDBySessionNumber(connection, iSessionNumber)

        ' check that the user has the execute permission for this process
        Dim m = GetEffectiveMemberPermissionsForProcess(connection, processId)
        If Not m.HasPermission(mLoggedInUser, Permission.ProcessStudio.ImpliedExecuteProcess) Then
            Throw New PermissionException(My.Resources.clsServer_FinishDebugSession_CannotEndSessionAsUserDoesNotHaveExecutePermissionOnThisProcess)
        End If

        If GetLogsCount(iSessionNumber) = 0 Then
            Dim deletecmd As New SqlCommand("DELETE FROM BPASession WHERE SessionNumber=@SessionNumber")
            With deletecmd.Parameters
                .AddWithValue("@SessionNumber", iSessionNumber.ToString)
            End With
            connection.Execute(deletecmd)
        Else
            Dim query = New StringBuilder()
            query.Append(
                "UPDATE BPASession
                 SET enddatetime = @enddatetime,
                     endtimezoneoffset = @endtimezoneoffset"
                )

            If newSessionStatus = SessionStatus.All Then
                query.Append(" ")
            Else
                query.Append(", statusid = @statusId ")
            End If
            query.Append("WHERE SessionNumber = @sessionNumber;")

            Using command = New SqlCommand()
                command.CommandText = query.ToString()
                With command.Parameters
                    .AddWithValue("@sessionNumber", iSessionNumber)
                    .AddWithValue("@enddatetime", endDateTime.DateTime)
                    .AddWithValue("@endtimezoneoffset", endDateTime.Offset.TotalSeconds)
                    If Not newSessionStatus = SessionStatus.All Then .AddWithValue("@statusId", newSessionStatus)

                End With

                connection.Execute(command)
            End Using

            ' Release any environment locks associated with this session.
            ReleaseEnvLocksForSession(connection, gSessionID, SessionIdentifierType.RuntimeResource)
            ' Also any queue item locks associated with the session
            WorkQueueMarkExceptionsForSession(connection, gSessionID,
                                              My.Resources.clsServer_AutomaticallySetExceptionOnSessionEnd, False)
        End If
    End Sub

    ''' <summary>
    ''' Writes a session record into the database.
    ''' </summary>
    ''' <param name="processId">The process id</param>
    ''' <param name="queueIdent">The identity of the active queue for which this is a
    ''' pending session; 0 if the session is not on behalf of an active queue.
    ''' </param>
    ''' <param name="token">An authorisation token</param>
    ''' <param name="startingResource">The starting resource id</param>
    ''' <param name="runningResource">The running resource id</param>
    ''' <param name="startDateTime">The date/time (UTC) that the session started</param>
    ''' <param name="sessionId">The session ID created</param>
    ''' <param name="sessNo">The session number created</param>
    ''' <exception cref="Exception">If any errors occur while attempting to create
    ''' the pending session.</exception>
    <SecuredMethod(Permission.Resources.ControlResource)>
    Public Function GetProcessXmlForCreatedSession(
     processId As Guid,
     queueIdent As Integer,
     token As String,
     startingResource As Guid,
     runningResource As Guid,
     startDateTime As DateTimeOffset,
     sessionId As Guid,
     ByRef sessNo As Integer) As String Implements IServer.GetProcessXmlForCreatedSession
        CheckPermissions()
        Using connection = GetConnection()

            Dim webService As Boolean
            Dim user = ValidateAuthorisedUserToken(connection, token, processId, webService)

            ' check that the user has the execute permission for this process
            Dim m = GetEffectiveMemberPermissionsForProcess(connection, processId)

            If webService Then
                If GetProcessType(connection, processId) = DiagramType.Process Then
                    If Not m.HasPermission(user, Permission.ProcessStudio.ExecuteProcessAsWebService) Then
                        Throw New PermissionException(My.Resources.clsServer_CannotCreateSessionAsUserDoesNotHavePermissionToExecuteThisProcessAsAWebservice)
                    End If
                Else
                    If Not m.HasPermission(user, Permission.ObjectStudio.ExecuteBusinessObjectAsWebService) Then
                        Throw New PermissionException(My.Resources.clsServer_CannotCreateSessionAsUserDoesNotHavePermissionToExecuteThisBusinessObjectAsAWeb)
                    End If
                End If
            Else
                If Not m.HasPermission(user, Permission.ProcessStudio.ImpliedExecuteProcess) Then
                    Throw New PermissionException(My.Resources.clsServer_CannotCreateSessionAsUserDoesNotHaveExecutePermissionOnThisProcess)
                End If

                Dim inaccessible = New ProcessDependencyPermissionLogic(Me).GetInaccessibleReferences(connection, processId, user, Function(s As String) NonCachedCachedProcessIDLookup(connection, s))
                If inaccessible.Any() Then
                    Throw New PermissionException(My.Resources.clsServer_CannotCreateSessionAsUserDoesNotHaveExecutePermissionOnOneOrMoreOfThisProcesses)
                End If
            End If

            CreateSession(connection, processId, queueIdent, user.Id, startingResource, runningResource,
                          SessionStatus.Pending, startDateTime, sessionId, sessNo)

            Return GetProcessXML(connection, processId)
        End Using
    End Function

    <SecuredMethod(Permission.Resources.ControlResource)>
    Public Sub CreatePendingSession(
     processId As Guid,
     queueIdent As Integer,
     starterUserId As Guid,
     startingResource As Guid,
     runningResource As Guid,
     startDateTime As DateTimeOffset,
     sessionId As Guid,
     ByRef sessNo As Integer) Implements IServer.CreatePendingSession
        CheckPermissions()
        Using connection = GetConnection()

            If GetControllingUserPermissionSetting(connection) Then
                Throw New PermissionException(My.Resources.clsServer_CannotCreateSessionUsingUserIDBecauseTokenValidationIsEnforced)
            End If

            CreateSession(connection, processId, queueIdent, starterUserId, startingResource, runningResource,
                         SessionStatus.Pending, startDateTime, sessionId, sessNo)
        End Using
    End Sub

    Private Function ValidateAuthorisedUserToken(connection As IDatabaseConnection, token As String,
                                                 processId As Guid, ByRef webService As Boolean) As IUser

        If GetControllingUserPermissionSetting(connection) Then
            Dim authToken = New clsAuthToken(token)
            Dim invalidReason = String.Empty
            Dim authUser = ValidateAuthorisationToken(connection, authToken, processId, webService, invalidReason)
            If authUser Is Nothing Then Throw New InvalidStateException(String.Format(My.Resources.clsServer_AuthorizationError0, invalidReason))
            Return authUser
        Else
            Return mLoggedInUser
        End If

    End Function

    ''' <summary>
    ''' Gets the session ID corresponding to the given session number.
    ''' </summary>
    ''' <param name="sessionNumber">The number for which the session ID is
    ''' required.</param>
    ''' <returns>The Session ID corresponding to the given number.</returns>
    <SecuredMethod(Permission.Resources.ViewResource, Permission.Resources.ViewResourceScreenCaptures, Permission.Resources.ConfigureResource, Permission.Resources.ControlResource)>
    Public Function GetSessionID(ByVal sessionNumber As Integer) As Guid Implements IServer.GetSessionID
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("select SessionID from BPASession where SessionNumber=@num")
            cmd.Parameters.AddWithValue("@num", sessionNumber)
            Return CType(con.ExecuteReturnScalar(cmd), Guid)
        End Using
    End Function

    ''' <summary>
    ''' Gets the session number
    ''' </summary>
    ''' <param name="gSessionId">Session ID</param>
    ''' <returns>Session number, or -1 if an error occurs</returns>
    <SecuredMethod()>
    Public Function GetSessionNumber(ByVal gSessionId As Guid) As Integer Implements IServer.GetSessionNumber
        CheckPermissions()

        ' Return the session number if one is found... otherwise, return -1
        Try
            Using con = GetConnection()
                Return GetSessionNumbers(con, GetSingleton.ICollection(gSessionId)).First()
            End Using
        Catch ' If any errors occur, return -1 (?)
        End Try
        Return -1

    End Function

    <SecuredMethod>
    Public Function GetSessionScheduleNumber(sessionId As Guid) As Integer Implements IServer.GetSessionScheduleNumber
        CheckPermissions()

        Try
            Dim cmd As New SqlCommand("select sessionnumber from BPASession where sessionid = @schedid")
            cmd.AddParameter("@schedid", sessionId)
            Using con = GetConnection()
                Using reader = con.ExecuteReturnDataReader(cmd)
                    While reader.Read()
                        Return reader.GetInt32(0)
                    End While
                End Using
            End Using
        Catch ex As Exception
        End Try
        Return -1
    End Function

    ''' <summary>
    ''' Gets the session numbers for the sessions identified by the given IDs
    ''' </summary>
    ''' <param name="ids">The IDs for which the session numbers are required.
    ''' </param>
    ''' <returns>The collection of session numbers corresponding to the given
    ''' session IDs.</returns>
    <SecuredMethod(Permission.Resources.ViewResource, Permission.Resources.ViewResourceScreenCaptures, Permission.Resources.ConfigureResource, Permission.Resources.ControlResource)>
    Public Function GetSessionNumbers(ByVal ids As ICollection(Of Guid)) As ICollection(Of Integer) Implements IServer.GetSessionNumbers
        CheckPermissions()
        Using con = GetConnection()
            Return GetSessionNumbers(con, ids)
        End Using
    End Function

    ''' <summary>
    ''' Gets the session numbers for the sessions identified by the given IDs
    ''' </summary>
    ''' <param name="con">The connection over which the session numbers should
    ''' be retrieved.</param>
    ''' <param name="ids">The IDs for which the session numbers are required.
    ''' </param>
    ''' <returns>The collection of session numbers corresponding to the given
    ''' session IDs.</returns>
    Private Function GetSessionNumbers(
     ByVal con As IDatabaseConnection, ByVal ids As ICollection(Of Guid)) As ICollection(Of Integer)

        Dim list As New List(Of Integer)
        If ids.Count = 0 Then Return list

        Using cmd As New SqlCommand()

            Dim stringBuilder As New StringBuilder("select s.sessionnumber from BPASession as s where s.sessionid in (")
            Dim i As Integer = 0

            For Each id As Guid In ids
                i += 1
                stringBuilder.AppendFormat("@id{0},", i)
                cmd.Parameters.AddWithValue("@id" & i, id)
            Next
            stringBuilder.Length -= 1
            stringBuilder.Append(") ")
            stringBuilder.Append(MteSqlGenerator.MteToken)

            Dim sessionTableAlias = "s"
            Dim mteQueryBuilder = New MteSqlGenerator(stringBuilder.ToString(), sessionTableAlias, True)
            Dim sqlWithMte = mteQueryBuilder.GetQueryAndSetParameters(mLoggedInUser, cmd)

            cmd.CommandText = sqlWithMte
            Using reader = con.ExecuteReturnDataReader(cmd)
                While reader.Read()
                    list.Add(reader.GetInt32(0))
                End While
            End Using
        End Using

        Return list

    End Function

    <SecuredMethod(True)>
    Public Function DoCreateSessionCommand(sessionData As List(Of CreateSessionData)) As Guid() Implements IServer.DoCreateSessionCommand
        CheckPermissions()
        sessionData.ForEach(Sub(session) session.AuthorizationToken = RegisterAuthorisationToken(session.ProcessId))
        Return GetASCRConnectionManager().SendCreateSession(sessionData)
    End Function

    <SecuredMethod(True)>
    Public Sub DoStartSessionCommand(sessions As List(Of StartSessionData)) Implements IServer.DoStartSessionCommand
        CheckPermissions()
        sessions.ForEach(Sub(session) session.AuthorizationToken = RegisterAuthorisationToken(session.ProcessId))
        GetASCRConnectionManager().SendStartSession(sessions)
    End Sub


    <SecuredMethod(True)>
    Public Function DoSendGetSessionVariables(resId As Guid, sessId As Guid, processId As Guid) As Boolean Implements IServer.DoSendGetSessionVariables
        CheckPermissions()
        Dim token = RegisterAuthorisationToken(processId)
        GetASCRConnectionManager().SendGetSessionVariablesAsUser(token, resId, sessId)
    End Function



    <SecuredMethod(True)>
    Public Sub DoDeleteSessionCommand(sessions As List(Of DeleteSessionData)) Implements IServer.DoDeleteSessionCommand
        CheckPermissions()
        sessions.ForEach(Sub(session)
                             session.AuthorizationToken = RegisterAuthorisationToken(session.ProcessId)
                         End Sub)
        GetASCRConnectionManager().SendDeleteSession(sessions)
    End Sub

    <SecuredMethod(True)>
    Public Sub SendStopSession(sessions As List(Of StopSessionData)) Implements IServer.SendStopSession
        CheckPermissions()
        GetASCRConnectionManager().SendStopSession(sessions)
    End Sub


    <SecuredMethod(True)>
    Public Sub SendSetSessionVariable(resourceID As Guid, sessionID As Guid, vars As String) Implements IServer.SendSetSessionVariable
        CheckPermissions()
        Dim sessionVariables As New Queue(Of clsSessionVariable)
        For Each varText In vars.Split(New Char() {","c})
            sessionVariables.Enqueue(clsSessionVariable.Parse(varText & " """""))
        Next
        For Each var In sessionVariables
            var.SessionID = sessionID
            var.ResourceID = resourceID
        Next
        GetASCRConnectionManager().SendSetSessionVariable(resourceID, sessionVariables)
    End Sub

    <SecuredMethod(True)>
    Public Sub ToggleShowSessionVariables(showSessionVariables As Boolean) Implements IServer.ToggleShowSessionVariables
        CheckPermissions()
        GetASCRConnectionManager().MonitorSessionVariables = showSessionVariables
    End Sub

    Private mAscrEnabled As Boolean? = Nothing

    Private Function GetASCRConnectionManager() As IUserAuthResourceConnectionManager
        If Not mAscrEnabled.HasValue Then
            mAscrEnabled = GetPref(PreferenceNames.SystemSettings.UseAppServerConnections, False)
        End If

        If IsServer() AndAlso mAscrEnabled.HasValue AndAlso mAscrEnabled Then
            Return ResourceConnectionManagerFactory.GetResourceConnectionManager(mAscrEnabled.Value)
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' Writes a statistic into the database, either as a new record or by updating
    ''' an existing one.
    ''' </summary>
    ''' <param name="gSessionId">The session id</param>
    ''' <param name="sName">The statistic name</param>
    ''' <param name="sValue">The statistic value</param>
    ''' <param name="sDataType">The statistic data type</param>
    <SecuredMethod(True)>
    Public Sub UpdateStatistic(ByVal gSessionId As Guid, ByVal sName As String, ByVal sValue As String, ByVal sDataType As String) Implements IServer.UpdateStatistic
        CheckPermissions()
        Using con = GetConnection()

            Dim iNum As Integer
            Dim cmd As New SqlCommand("select * from BPAStatistics where sessionid=@SessionID and name=@Name")
            With cmd.Parameters
                .AddWithValue("@SessionID", gSessionId.ToString)
                .AddWithValue("@Name", sName)
            End With
            Using reader = con.ExecuteReturnDataReader(cmd)
                iNum = 0
                Do While reader.Read()
                    iNum = iNum + 1
                Loop
            End Using
            If iNum = 0 Then
                'Statistic has not been recorded for this session
                'yet, so add the record...
                Dim insertcmd As New SqlCommand("insert into BPAStatistics (sessionid,name,datatype) values (@SessionID,@Name,@DataType)")
                With insertcmd.Parameters
                    .AddWithValue("@SessionID", gSessionId.ToString)
                    .AddWithValue("@Name", sName)
                    .AddWithValue("@DataType", sDataType)
                End With
                con.Execute(insertcmd)
            End If

            'Now update the value...
            Dim updatecmd As New SqlCommand
            Dim sField As String = Nothing
            With updatecmd.Parameters
                .AddWithValue("@SessionID", gSessionId.ToString)
                .AddWithValue("@Name", sName)
                Select Case sDataType
                    Case "text"
                        sField = "value_text"
                        .AddWithValue("@Value", sValue)
                    Case "number"
                        Dim dValue As Decimal
                        dValue = CDec(sValue)
                        sField = "value_number"
                        .AddWithValue("@Value", dValue)
                    Case "date", "datetime"
                        Dim dValue = CDate(sValue)
                        sField = "value_date"
                        .AddWithValue("@Value", clsDBConnection.UtilDateToSqlDate(dValue))
                    Case "time"
                        Dim dValue As Date = Now()
                        dValue = CDate(Today() & " " & sValue)
                        sField = "value_date"
                        .AddWithValue("@Value", clsDBConnection.UtilDateToSqlDate(dValue))
                    Case "timespan"
                        Dim T As TimeSpan
                        If TimeSpan.TryParse(sValue, T) Then
                            sField = "value_number"
                            .AddWithValue("@Value", T.TotalMilliseconds.ToString)
                        Else
                            Throw New BluePrismException(My.Resources.clsServer_CouldNotInterpretValue0AsTimespan, sValue)
                        End If
                    Case "flag"
                        Dim bValue As Boolean
                        bValue = CBool(sValue)
                        If bValue Then
                            sValue = "1"
                        Else
                            sValue = "0"
                        End If
                        sField = "value_flag"
                        .AddWithValue("@Value", sValue)
                    Case Else
                        Throw New BluePrismException(My.Resources.clsServer_InvalidDataTypeOf0ForStatistic, sDataType)
                End Select
            End With

            updatecmd.CommandText = "update bpastatistics set " & sField & "=@Value where sessionid=@SessionID and name=@Name"

            con.Execute(updatecmd)

        End Using

    End Sub

#Region "Process Alerts"

    ''' <summary>
    ''' Update alert configuration information
    ''' </summary>
    ''' <param name="user">The user object to update</param>
    ''' <param name="processes">A collection of process IDs the user is interested in
    ''' </param>
    ''' <param name="schedules">The IDs of the schedules that the user is interested
    ''' in.</param>
    ''' <exception cref="SqlException">If any database errors occur while attempting
    ''' to udpate the alert configuration.</exception>
    <SecuredMethod(True)>
    Public Sub UpdateAlertConfig(
     ByVal user As User,
     ByVal processes As ICollection(Of Guid),
     ByVal schedules As ICollection(Of Integer)) Implements IServer.UpdateAlertConfig
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()

            ' First save the user changes
            UpdateUser(con, user, Nothing)

            Dim cmd As New SqlCommand()
            cmd.Parameters.AddWithValue("@UserID", user.Id)

            'Delete the user's previous choice of processes.
            cmd.CommandText =
             "delete from BPAProcessAlert where UserID = @UserID; " &
             "delete from BPAScheduleAlert where userid = @UserID"
            con.Execute(cmd)

            UpdateMultipleIds(con, cmd, processes, "processid",
             "insert into BPAProcessAlert (UserID, ProcessID) " &
             "  select @UserID, processid " &
             "  from BPAProcess where processid in ("
            )

            UpdateMultipleIds(con, cmd, schedules, "schedid",
             "insert into BPAScheduleAlert (userid, scheduleid) " &
             "  select @UserID, id " &
             "  from BPASchedule where id in ("
            )

            con.CommitTransaction()

        End Using

    End Sub

    ''' <summary>
    ''' Get alert history information.
    ''' </summary>
    ''' <param name="user">The user of interest.</param>
    ''' <param name="historyDate">The date (local time) its converted to utc
    ''' and then results are returned between this date and 1 day ahead of
    ''' this date</param>
    ''' <returns>A DataTable containing the information.</returns>
    <SecuredMethod(True)>
    Public Function GetAlertHistory(user As User, historyDate As DateTime) As DataTable Implements IServer.GetAlertHistory
        CheckPermissions()
        Using con = GetConnection()
            Dim dt = GetAlertHistory(con,
                                   user,
                                   historyDate,
                                   Function(id As Guid) GetEffectiveMemberPermissionsForProcess(con, id),
                                   Function(id As Guid) GetEffectiveMemberPermissionsForResource(con, id))
            dt.TableName = "Alerts"
            Return dt
        End Using

    End Function


    Protected Function GetAlertHistory(connection As IDatabaseConnection,
                                     user As IUser,
                                     historyDate As DateTime,
                                     getprocessPermissions As Func(Of Guid, IMemberPermissions),
                                     getResourcePermissions As Func(Of Guid, IMemberPermissions)) As DataTable

        Dim cmd As New SqlCommand(
         " select a.date as Date," &
         "   p.Name as Process," &
         "   a.Message," &
         "   r.Name as Resource," &
         "   coalesce(r.ResourceID, cast(0x0 as uniqueidentifier)) AS ResourceID," &
         "   coalesce(p.ProcessID, cast(0x0 as uniqueidentifier)) AS ProcessID," &
         "   s.Name as Schedule, " &
         "   t.Name as Task, " &
         "   a.AlertEventType as Type," &
         "   a.AlertNotificationType as Method" &
         " from BPAAlertEvent a" &
         "   left join BPAProcess p ON a.ProcessID=p.ProcessID" &
         "   left join BPAResource r ON a.ResourceID=r.ResourceID" &
         "   left join BPASchedule s on a.scheduleid=s.id" &
         "   left join BPATask t on a.taskid=t.id" &
         " where a.SubscriberUserID=@userid and" &
         "   a.subscriberdate between @date and dateadd(dd, 1, @date)" &
         " order by a.AlertEventID desc")

        With cmd.Parameters
            .AddWithValue("@userid", user.Id)
            .AddWithValue("@date", historyDate.ToUniversalTime)
        End With

        ' filter out processes which the user doesn't have permission to see.
        Dim table = connection.ExecuteReturnDataTable(cmd)
        Dim filteredRows = table.AsEnumerable().Where(Function(x)

                                                          Return getprocessPermissions(x.Field(Of Guid)("ProcessID")).HasAnyPermissions(user) AndAlso
                                              getResourcePermissions(x.Field(Of Guid)("ResourceID")).HasPermission(user, Permission.Resources.ImpliedViewResource)
                                                      End Function)

        filteredRows.ToList().ForEach(Sub(x)
                                          Dim type = CType(x(x.Table.Columns("Type")), AlertEventType)
                                          If (type <> AlertEventType.Stage) Then
                                              x(x.Table.Columns("Message")) = GetAlertEventTypeMessage(type)
                                          End If
                                      End Sub)

        If filteredRows.Any() Then Return filteredRows.CopyToDataTable()

        Return table.Clone()
    End Function

    ''' <summary>
    ''' Get process details for the alerts form.
    ''' </summary>
    ''' <param name="userid">The ID of the user in question</param>
    ''' <returns>A DataTable containing the information.</returns>
    ''' <exception cref="SqlException">If any database errors occur while attempting
    ''' to udpate the alert configuration.</exception>
    <SecuredMethod(True)>
    Public Function GetAlertProcessDetails(ByVal userid As Guid) As DataTable Implements IServer.GetAlertProcessDetails
        CheckPermissions()
        Using con = GetConnection()
            Dim dt = GetAlertProcessDetails(
                con,
                GetUser(userid),
                Function(id) GetEffectiveMemberPermissionsForProcess(con, id)
                )
            dt.TableName = "Alerts"
            Return dt
        End Using
    End Function

    ''' <summary>
    ''' Get process details for the alerts form.
    ''' </summary>
    ''' <param name="con"></param>
    ''' <param name="user">The ID of the user in question</param>
    ''' <param name="getEffectiveMemberPermissionsForProcess">A function to return the effective member permissions of a process</param>
    ''' <returns>A DataTable containing the information.</returns>
    ''' <exception cref="SqlException">If any database errors occur while attempting
    ''' to udpate the alert configuration.</exception>
    Protected Function GetAlertProcessDetails(con As IDatabaseConnection,
                                            user As IUser,
                                            getEffectiveMemberPermissionsForProcess As Func(Of Guid, IMemberPermissions)
                                            ) As DataTable
        Dim cmd As New SqlCommand(
         " select" &
         "   p.ProcessID," &
         "   p.Name," &
         "   p.Description," &
         "   case when a.UserID is null then 0 else 1 end as Checked" &
         " from BPAProcess p" &
         "   left join BPAProcessAlert a " &
         "     on a.ProcessID = p.processid and a.UserID = @userid" &
         " where p.ProcessType = 'p'" &
         "   and (p.AttributeID & @attributeid) ! = 0" &
         " order by p.Name"
)
        With cmd.Parameters
            .AddWithValue("@userid", user.Id)
            .AddWithValue("@attributeid", ProcessAttributes.Published)
        End With

        ' filter out processes which the user doesn't have permission to see.
        Dim table = con.ExecuteReturnDataTable(cmd)
        Dim rows = table.
            AsEnumerable().
            Where(Function(x) getEffectiveMemberPermissionsForProcess(x.Field(Of Guid)("ProcessID")).HasAnyPermissions(user))

        If rows.Count > 0 Then
            Return rows.CopyToDataTable()
        End If
        Return table.Clone()

    End Function

    ''' <summary>
    ''' Gets the schedule IDs for all of the schedules that the given user has an
    ''' alert subscription to.
    ''' </summary>
    ''' <param name="userid">The ID of the user for which the their schedule
    ''' subscription data is required.</param>
    ''' <returns>A collection of integers representing schedule IDs that the user is
    ''' subscribed to within the alerts system.</returns>
    ''' <exception cref="SqlException">If any database errors occur while attempting
    ''' to udpate the alert configuration.</exception>
    <SecuredMethod(True)>
    Public Function GetSubscribedScheduleAlerts(ByVal userid As Guid) As ICollection(Of Integer) Implements IServer.GetSubscribedScheduleAlerts
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("select scheduleid from BPAScheduleAlert where userid=@id")
            cmd.Parameters.AddWithValue("@id", userid)
            Dim ids As New clsSet(Of Integer)
            Dim reader = con.ExecuteReturnDataReader(cmd)
            While reader.Read()
                ids.Add(reader.GetInt32(0))
            End While
            Return ids
        End Using
    End Function

    ''' <summary>
    ''' Update and acknowledge alerts for a particular user.
    ''' </summary>
    ''' <param name="sessionsToIgnore">A comma-separated list of session IDs to be
    ''' ignored. Yes, that's right, it really is a comma-separated list of Guids in
    ''' String form. Or an empty string.</param>
    ''' <param name="resourceid">The Resource ID</param>
    ''' <param name="user">The User</param>
    ''' <returns>A DataTable containing new alerts, or Nothing if an error
    ''' occurred.</returns>
    <SecuredMethod(True)>
    Public Function UpdateAndAcknowledgeAlerts(
     ByVal sessionsToIgnore As ICollection(Of Guid),
     ByVal resourceid As Guid,
     ByVal user As User) As DataTable Implements IServer.UpdateAndAcknowledgeAlerts
        CheckPermissions()
        Using con = GetConnection()
            Dim dt = UpdateAndAcknowledgeAlerts(con,
                                              sessionsToIgnore,
                                              resourceid,
                                              user,
                                              Function(id As Guid) GetEffectiveMemberPermissionsForProcess(con, id),
                                              Function(id As Guid) GetEffectiveMemberPermissionsForResource(con, id))
            dt.TableName = "Alerts"
            Return dt
        End Using

    End Function


    Protected Function UpdateAndAcknowledgeAlerts(connection As IDatabaseConnection,
                                                ByVal sessionsToIgnore As ICollection(Of Guid),
                                                ByVal resourceid As Guid,
                                                ByVal user As IUser,
                                                getprocessPermissions As Func(Of Guid, IMemberPermissions),
                                                getResourcePermissions As Func(Of Guid, IMemberPermissions)) As DataTable
        Try
            connection.BeginTransaction()
            Dim cmd As New SqlCommand()
            ' Either UserID or ResourceId or both are used in all queries... set them once.
            With cmd.Parameters
                .AddWithValue("@ResourceID", resourceid)
                .AddWithValue("@UserID", user.Id)
            End With

            ' Update any unacknowledged alerts the user wants to ignore.
            If sessionsToIgnore IsNot Nothing AndAlso sessionsToIgnore.Count > 0 Then
                UpdateMultipleIds(connection, cmd, sessionsToIgnore, "sessid",
                 " update BPAAlertEvent set " &
                 "   SubscriberDate=getutcdate(), " &
                 "   SubscriberResourceID=@ResourceID " &
                 " where SubscriberUserID = @UserID AND " &
                 "   SubscriberDate is null and" &
                 "   SessionID in ("
                )
            End If

            ' Look for unacknowledged alerts aimed at this user.
            cmd.CommandText =
             " select a.AlertEventID, " &
             "   a.AlertNotificationType, " &
             "   a.Message, " &
             "   a.[Date], " &
             "    coalesce(a.SessionID, cast(0x0 as uniqueidentifier)) AS SessionID, " &
             "    coalesce(a.ResourceID, cast(0x0 as uniqueidentifier)) AS ResourceID, " &
             "   r.[Name] AS ResourceName, " &
             "   p.[Name] AS ProcessName, " &
             "   s.name as ScheduleName, " &
             "   t.name as TaskName, " &
             "    coalesce(p.ProcessID, cast(0x0 as uniqueidentifier))  AS ProcessID" &
             " from BPAAlertEvent a  " &
             "   left join BPAProcess p on a.ProcessID=p.ProcessID  " &
             "   left join BPAResource r ON a.ResourceID=r.ResourceID  " &
             "   left join BPASchedule s on a.scheduleid = s.id " &
             "   left join BPATask t on a.taskid = t.id " &
             " where a.SubscriberUserID = @UserID and  " &
             "   a.SubscriberDate is null " &
             " order by a.AlertEventID "
            Dim table As DataTable = connection.ExecuteReturnDataTable(cmd)

            ' Acknowledge the alerts
            cmd.CommandText =
             " update BPAAlertEvent set " &
             "   SubscriberDate=getutcdate(), " &
             "   SubscriberResourceID=@ResourceID " &
             " where SubscriberUserID = @UserID AND " &
             "   SubscriberDate is null"
            connection.Execute(cmd)

            connection.CommitTransaction()

            Dim filteredRows = table.AsEnumerable().Where(Function(x)
                                                              Return getprocessPermissions(x.Field(Of Guid)("ProcessID")).HasAnyPermissions(user) AndAlso
                                                                getResourcePermissions(x.Field(Of Guid)("ResourceID")).HasPermission(user, Permission.Resources.ImpliedViewResource)
                                                          End Function)

            If filteredRows.Any() Then Return filteredRows.CopyToDataTable()

            Return table.Clone()

        Catch ex As Exception
            Return Nothing

        End Try
    End Function

#Region "CreateAlert & variations"

    ''' <summary>
    ''' Creates a stage alert for the given session with the specified message.
    ''' This will use the resource ID running the supplied session as the resource
    ''' for the alert.
    ''' </summary>
    ''' <param name="sessionId">The ID of the session which generated the alert.
    ''' </param>
    ''' <param name="message">The message to write to the alert.</param>
    <SecuredMethod(True)>
    Public Sub CreateStageAlert(
     ByVal sessionId As Guid,
     ByVal message As String) Implements IServer.CreateStageAlert
        CheckPermissions()
        Using con = GetConnection()
            CreateProcessAlert(con, AlertEventType.Stage, sessionId, Nothing, Nothing, message)
        End Using
    End Sub

    ''' <summary>
    ''' Creates an alert of the given type for the given session, overriding the
    ''' resource ID with the given value and using the default message for that
    ''' event type.
    ''' The message for this alert will be determined by the
    ''' <see cref="GetAlertEventTypeMessage"/> method.
    ''' </summary>
    ''' <param name="type">The event type of the alert</param>
    ''' <param name="sessionId">The ID of the session which generated the alert.
    ''' </param>
    ''' <param name="resourceId">The ID of the resource to log in this alert. If
    ''' this is left <see cref="Guid.Empty">empty</see>, the running resource ID
    ''' from the session will be used.</param>
    <SecuredMethod(True)>
    Public Sub CreateProcessAlert(
     ByVal type As AlertEventType,
     ByVal sessionId As Guid,
     ByVal resourceId As Guid) Implements IServer.CreateProcessAlert
        CheckPermissions()
        Using con = GetConnection()
            CreateProcessAlert(con, type, sessionId, resourceId, Nothing, Nothing)
        End Using
    End Sub

    ''' <summary>
    ''' Creates an alert of the given type for the given session, overriding the
    ''' resource ID and process ID with the given value.
    ''' </summary>
    ''' <param name="type">The event type of the alert</param>
    ''' <param name="sessionId">The ID of the session which generated the alert.
    ''' </param>
    ''' <param name="resourceId">The ID of the resource to log in this alert. If
    ''' this is left <see cref="Guid.Empty">empty</see>, the running resource ID
    ''' from the session will be used.</param>
    ''' <param name="processId">The ID of the process to log in this alert. If
    ''' this is left <see cref="Guid.Empty">empty</see>, the process ID from the
    ''' session will be used.</param>
    ''' <param name="message">The message to write to the alert.</param>
    <SecuredMethod(True)>
    Public Sub CreateProcessAlert(
     ByVal type As AlertEventType,
     ByVal sessionId As Guid,
     ByVal resourceId As Guid,
     ByVal processId As Guid,
     ByVal message As String) Implements IServer.CreateProcessAlert
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            CreateProcessAlert(con, type, sessionId, resourceId, processId, message)
            con.CommitTransaction()
        End Using

    End Sub

    ''' <summary>
    ''' Creates a schedule alert with the given parameters
    ''' </summary>
    ''' <param name="type">The type of event to create</param>
    ''' <param name="scheduleId">The ID of the schedule to which this alert event
    ''' should refer. This should always be populated even if the alert event is
    ''' for a task.</param>
    ''' <param name="taskId">The ID of the task to which this alert event should
    ''' refer, zero if no task is referred to.</param>
    ''' <exception cref="ArgumentException">If the given schedule ID is zero.
    ''' </exception>
    ''' <exception cref="SqlException">If any errors occur on the database while
    ''' attempting to create schedule alert.</exception>
    <SecuredMethod(True)>
    Public Sub CreateScheduleAlert(
     ByVal type As AlertEventType,
     ByVal scheduleId As Integer,
     ByVal taskId As Integer) Implements IServer.CreateScheduleAlert
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            CreateScheduleAlert(con, type, scheduleId, taskId)
            con.CommitTransaction()
        End Using

    End Sub

    ''' <summary>
    ''' Creates a schedule alert with the given parameters
    ''' </summary>
    ''' <param name="con">The connection to the database to use to create the alert.
    ''' </param>
    ''' <param name="type">The type of event to create</param>
    ''' <param name="scheduleId">The ID of the schedule to which this alert event
    ''' should refer. This should always be populated even if the alert event is
    ''' for a task.</param>
    ''' <param name="taskId">The ID of the task to which this alert event should
    ''' refer, zero if no task is referred to.</param>
    ''' <exception cref="ArgumentException">If the given schedule ID is zero.
    ''' </exception>
    ''' <exception cref="SqlException">If any errors occur on the database while
    ''' attempting to create schedule alert.</exception>
    Private Sub CreateScheduleAlert(
     ByVal con As IDatabaseConnection,
     ByVal type As AlertEventType,
     ByVal scheduleId As Integer,
     ByVal taskId As Integer)

        If scheduleId = 0 Then
            Throw New ArgumentException(
             "A schedule alert cannot be created without a schedule", NameOf(scheduleId))
        End If

        Dim message = ValidateAlertsMessage(Nothing, type)

        Dim cmd As New SqlCommand()

        cmd.CommandText =
         " insert into BPAAlertEvent" &
         "   (alerteventtype, alertnotificationtype, message, date, " &
         "    sessionid, processid, resourceid, scheduleid, taskid, subscriberuserid)" &
         " select @evt,  " &
         "   @notif,  " &
         "   @msg,  " &
         "   getutcdate(),  " &
         "   null,  " &
         "   null, " &
         "   null,  " &
         "   @scheduleid,  " &
         "   @taskid,  " &
         "   u.userid " &
         " from BPAUser u  " &
         "   left join BPAScheduleAlert sa on u.userid = sa.userid " &
         " where sa.scheduleid = @scheduleid and " &
         "   u.alertnotificationtypes & @notif <> 0 and " &
         "   u.alerteventtypes & @evt <> 0; "

        With cmd.Parameters

            .AddWithValue("@evt", type)
            .AddWithValue("@msg", message)
            .AddWithValue("@scheduleid", scheduleId)
            .AddWithValue("@taskid", IIf(taskId = 0, DBNull.Value, taskId))

            ' Last parameter changes for each different type of notification that
            ' we support, so just add it and give it a type - we can set the value
            ' when we're going through the supported types
            .Add("@notif", SqlDbType.Int)

        End With

        For Each notif As AlertNotificationType In GetSupportedAlertNotificationTypes()
            cmd.Parameters("@notif").Value = notif
            con.Execute(cmd)
        Next

    End Sub

    ''' <summary>
    ''' Truncates the alerts message if it is too long. If Null is passed as the message argument, the default message for the alert type is returned.
    ''' </summary>
    ''' <param name="message"></param>
    ''' <param name="type"></param>
    ''' <returns></returns>
    Private Function ValidateAlertsMessage(message As String, type As AlertEventType) As String
        Const truncateMsg As String = "...(Truncated)"
        Const maxMessageLen As Integer = 500

        ' Use the default message for the alert type if one is not given
        If message Is Nothing Then
            message = GetAlertEventTypeMessage(type)
        End If

        ' Crop it if it's too long.
        If message.Length > maxMessageLen Then
            message = message.Substring(0, maxMessageLen - truncateMsg.Length) & truncateMsg
        End If

        Return message
    End Function

    ''' <summary>
    ''' Creates a process alert.
    ''' </summary>
    ''' <param name="con">The connection over which the alert should be created.</param>
    ''' <param name="type">The event type of the alert</param>
    ''' <param name="sessionId">The ID of the session which generated the alert</param>
    ''' <param name="resourceId">The ID of the resource to log in this alert. If
    ''' this is left <see cref="Guid.Empty">empty</see>, the running resource ID
    ''' from the session will be used.</param>
    ''' <param name="processId">The ID of the process to log in this alert. If
    ''' this is left <see cref="Guid.Empty">empty</see>, the process ID from the
    ''' session will be used.</param>
    ''' <param name="message">The message to write to the alert. If this argument is
    ''' null, the default message, as defined by
    ''' <see cref="GetAlertEventTypeMessage"/> is used. If the message is larger than
    ''' the maximum allowed length for a message, it is truncated before writing to
    ''' the database.</param>
    ''' <exception cref="ArgumentException">If the given session ID is Guid.Empty.
    ''' </exception>
    ''' <exception cref="SqlException">If any errors occur on the database while
    ''' attempting to create schedule alert.</exception>
    Private Sub CreateProcessAlert(ByVal con As IDatabaseConnection,
     ByVal type As AlertEventType,
     ByVal sessionId As Guid,
     ByVal resourceId As Guid,
     ByVal processId As Guid,
     ByVal message As String)

        If sessionId = Nothing Then Throw New ArgumentException(My.Resources.clsServer_ASessionIDMustBeProvided)

        message = ValidateAlertsMessage(message, type)

        ' Get process id and resource id from the session if they haven't been passed in as an argument.
        Dim sessionDetails = GetSessionDetails(con, sessionId, 0)

        If processId = Guid.Empty Then processId = GetProcessIDByName(con, sessionDetails.ProcessName)
        If resourceId = Guid.Empty Then resourceId = GetResourceId(con, sessionDetails.ResourceName)

        Dim cmd As New SqlCommand()

        cmd.CommandText =
         " insert into BPAAlertEvent" &
         "   (alerteventtype, alertnotificationtype, message, date, " &
         "    sessionid, processid, resourceid, scheduleid, taskid, subscriberuserid)" & _
 _
         " select @evt, " &
         "   @notif, " &
         "   @msg, " &
         "   getutcdate(), " &
         "   @sessionid, " &
         "   @processid, " &
         "   @resourceid, " &
         "   null, " &
         "   null, " &
         "   u.userid" &
         " from BPAUser u " &
         "   left join BPAProcessAlert pa on u.userid = pa.userid" &
         " where pa.ProcessID = @processid and " &
         "   u.alertnotificationtypes & @notif <> 0 and" &
         "   u.alerteventtypes & @evt <> 0;"

        With cmd.Parameters

            .AddWithValue("@evt", type)
            .AddWithValue("@msg", message)
            .AddWithValue("@sessionid", sessionId)
            .AddWithValue("@processid", processId)
            .AddWithValue("@resourceid", resourceId)

            ' Last parameter changes for each different type of notification that
            ' we support, so just add it and give it a type - we can set the value
            ' when we're going through the supported types
            .Add("@notif", SqlDbType.Int)

        End With

        For Each notif As AlertNotificationType In GetSupportedAlertNotificationTypes()
            cmd.Parameters("@notif").Value = notif
            con.Execute(cmd)
        Next
    End Sub


#End Region

    ''' <summary>
    ''' Gets a list of the alerts machines from the BPAAlertsMachines
    ''' table.
    ''' </summary>
    ''' <returns>A list of machine names.</returns>
    <SecuredMethod(
        Permission.SystemManager.Audit.Alerts,
        Permission.ProcessAlerts.SubscribeToProcessAlerts,
        Permission.ProcessAlerts.ConfigureProcessAlerts)>
    Public Function GetAlertsMachines() As List(Of String) Implements IServer.GetAlertsMachines
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("select MachineName from BPAAlertsMachines")
            Dim machines As New List(Of String)
            Using reader = con.ExecuteReturnDataReader(cmd)
                While reader.Read()
                    machines.Add(reader.GetString(0))
                End While
            End Using
            Return machines
        End Using
    End Function

    ''' <summary>
    ''' Deletes process alerts machines from the BPAAlertsMachines table.
    ''' </summary>
    ''' <param name="Machines">The list of machines to be deleted.</param>
    <SecuredMethod(True)>
    Public Sub DeleteAlertsMachines(machines As List(Of String)) Implements IServer.DeleteAlertsMachines
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand()
            'Build list of machine names
            Dim sb As New StringBuilder("delete from BPAAlertsMachines where MachineName in (")
            For i As Integer = 0 To machines.Count - 1
                If i > 0 Then sb.Append(","c)
                sb.AppendFormat("@mach{0}", i)
                cmd.Parameters.AddWithValue("mach" & i, machines(i))
            Next

            cmd.CommandText = sb.Append(")"c).ToString()
            con.Execute(cmd)
        End Using

    End Sub

    ''' <summary>
    ''' Determines if the named machine is registered for process alerts.
    ''' </summary>
    ''' <param name="MachineName">The name of the machine to be checked.</param>
    ''' <returns>Returns True if registered, False otherwise.</returns>
    <SecuredMethod(True)>
    Public Function IsAlertMachineRegistered(ByVal MachineName As String) As Boolean Implements IServer.IsAlertMachineRegistered
        CheckPermissions()
        Using con = GetConnection()
            Try
                Dim cmd As New SqlCommand(
                 "select 1 from BPAAlertsMachines where MachineName = @MachineName")
                cmd.Parameters.AddWithValue("@MachineName", MachineName)
                Return (con.ExecuteReturnScalar(cmd) IsNot Nothing)

            Catch err As Exception
                Return False

            End Try

        End Using

    End Function

    ''' <summary>
    ''' Registers the supplied machine for use in process alerts.
    ''' </summary>
    ''' <param name="MachineName">The hostanme of the machine to be registered.
    ''' </param>
    <SecuredMethod(True)>
    Public Sub RegisterAlertMachine(ByVal MachineName As String) Implements IServer.RegisterAlertMachine
        CheckPermissions()
        'Check that there aren't already too many such machines
        Dim machines As List(Of String) = GetAlertsMachines()

        If machines.Count >= Licensing.License.NumProcessAlertsPCs Then
            Throw New BluePrismException(My.Resources.clsServer_TheMaximumNumberOfDesktopsUsingProcessAlertsPermittedByYourLicenseHasBeenReached & vbCrLf & vbCrLf &
             String.Format(My.Resources.clsServer_RedundantInstallationsCanBeRemovedByYour0SystemAdministratorIfDesired, ApplicationProperties.ApplicationName))
        End If

        Using con = GetConnection()
            Dim cmd As New SqlCommand(
             "insert into BPAAlertsMachines (MachineName) values (@MachineName)")
            cmd.Parameters.AddWithValue("@MachineName", MachineName)
            con.Execute(cmd)
        End Using
    End Sub

    ''' <summary>
    ''' Clears any previously unacknowledged alerts for a particular user.
    ''' </summary>
    ''' <param name="userid">The ID of the user to delete alerts for.</param>
    <SecuredMethod(True)>
    Public Sub ClearOldAlerts(ByVal userid As Guid) Implements IServer.ClearOldAlerts
        CheckPermissions()
        Using con = GetConnection()

            ' An unacknowledged alert has a null subscriber date
            Dim cmd As New SqlCommand(
             " update BPAAlertEvent " &
             "   set SubscriberDate = getutcdate() " &
             " where SubscriberUserID=@UserID " &
             "   and SubscriberDate is null")

            cmd.Parameters.AddWithValue("@UserID", userid)
            con.Execute(cmd)

        End Using

    End Sub


#End Region

#Region " Finish Session "

    ''' <summary>
    ''' Sets the end date/time and status of the session.
    ''' </summary>
    ''' <param name="con">The connection to use to update the session.</param>
    ''' <param name="finishStatus">The end status to record on the session.</param>
    ''' <param name="sessionId">The ID of the session.</param>
    ''' <param name="endDateTime">The date/time (UTC) that the session finished</param>
    ''' <exception cref="NoSuchSessionException">If there was no started session on
    ''' the database with the given ID</exception>
    Private Sub SetSessionFinished(
                                  con As IDatabaseConnection,
                                  sessionId As Guid, finishStatus As SessionStatus,
                                  endDateTime As DateTimeOffset,
                                  sessionExceptionDetail As SessionExceptionDetail)
        Dim isTerminated = finishStatus = SessionStatus.Terminated

        If isTerminated AndAlso sessionExceptionDetail Is Nothing Then
            Throw New InvalidValueException($"{NameOf(sessionExceptionDetail)} cannot be null when the session status is {SessionStatus.Terminated}")
        End If

        If Not isTerminated AndAlso sessionExceptionDetail IsNot Nothing Then
            Throw New InvalidValueException($"{NameOf(sessionExceptionDetail)} must be null when the session status is not {SessionStatus.Terminated}")
        End If


        Dim cmd As New SqlCommand()

        Dim commandText = " if exists (" &
             "   select * from BPVSession" &
             "   where sessionid = @sessionid and startdatetime is not null)" &
             " begin" &
             "   update BPASession set" &
             "     enddatetime = @enddatetime," &
             "     endtimezoneoffset = @endtimezoneoffset," &
             "     statusid = @status," &
             "     terminationreason = @terminationReason," &
             "     exceptiontype = @exceptionType," &
             "     exceptionmessage = @exceptionMessage" &
             "   where sessionid = @sessionid" &
             "   select 1" &
             " end" &
             " else" &
             "   select 0"

        cmd.CommandText = commandText

        With cmd.Parameters
            .AddWithValue("@enddatetime", endDateTime.DateTime)
            .AddWithValue("@endtimezoneoffset", endDateTime.Offset.TotalSeconds)
            .AddWithValue("@status", finishStatus)
            .AddWithValue("@sessionid", sessionId)

            If isTerminated Then
                Dim exceptionType = If(String.IsNullOrEmpty(sessionExceptionDetail.ExceptionType), DBNull.Value, CObj(sessionExceptionDetail.ExceptionType))
                Dim exceptionMessage = If(String.IsNullOrEmpty(sessionExceptionDetail.ExceptionMessage), DBNull.Value, CObj(sessionExceptionDetail.ExceptionMessage))
                .AddWithValue("@terminationreason", sessionExceptionDetail.TerminationReason)
                .AddWithValue("@exceptiontype", exceptionType)
                .AddWithValue("@exceptionmessage", exceptionMessage)
            Else
                .AddWithValue("@terminationreason", DBNull.Value)
                .AddWithValue("@exceptiontype", DBNull.Value)
                .AddWithValue("@exceptionmessage", DBNull.Value)
            End If
        End With

        ' Check that a session was found and set as finished.
        Dim resp As Integer = CInt(con.ExecuteReturnScalar(cmd))

        ' If no such session was found, raise the necessaries
        If resp = 0 Then Throw New NoSuchSessionException(
         My.Resources.clsServer_NoStartedSessionsExistWithTheSpecifiedSessionID0, sessionId)

        ' Release any environment locks associated with this session.
        ReleaseEnvLocksForSession(con, sessionId, SessionIdentifierType.RuntimeResource)
        ' Also any queue item locks associated with the session
        WorkQueueMarkExceptionsForSession(con, sessionId,
         CStr(IIf(isTerminated, My.Resources.clsServer_AutomaticallySetExceptionOnSessionTerminated,
             My.Resources.clsServer_AutomaticallySetExceptionOnSessionEnd)),
         isTerminated)
        ' Record event for MI
        LogMISessionEnd(con, sessionId)
    End Sub

    ''' <summary>
    ''' Sets a session finished, throwing an exception if any errors occur while
    ''' attempting to do so
    ''' </summary>
    ''' <param name="sessionId">The ID of the session</param>
    ''' <param name="finishStatus">The status to record on the session.</param>
    ''' <param name="endDateTime">The date/time (UTC) that the session finished</param>
    Private Sub SetSessionFinished(sessionId As Guid,
     finishStatus As SessionStatus, endDateTime As DateTimeOffset,
                                   sessionExceptionDetail As SessionExceptionDetail)
        Using con = GetConnection()
            SetSessionFinished(con, sessionId, finishStatus, endDateTime, sessionExceptionDetail)
        End Using
    End Sub

    ''' <summary>
    ''' Updates a session's status to 'Terminated'. In the real world, this means
    ''' 'Failed' as a result of an unhandled runtime error (an exception in the
    ''' process).
    ''' </summary>
    ''' <param name="sessId">The session id</param>
    ''' <param name="endDateTime">The date/time (UTC) that the session finished</param>
    <SecuredMethod(Permission.Resources.ControlResource, Permission.Resources.AuthenticateAsResource)>
    Public Sub SetSessionTerminated(sessId As Guid, endDateTime As DateTimeOffset, sessionExceptionDetail As SessionExceptionDetail) Implements IServer.SetSessionTerminated
        CheckPermissions()
        SetSessionFinished(sessId, SessionStatus.Terminated, endDateTime, sessionExceptionDetail)
    End Sub

    ''' <summary>
    ''' Updates a session's status to 'Completed'.
    ''' </summary>
    ''' <param name="sessId">The session id</param>
    ''' <param name="endDateTime">The date/time (UTC) that the session finished</param>
    <SecuredMethod(Permission.Resources.ControlResource, Permission.Resources.AuthenticateAsResource)>
    Public Sub SetSessionCompleted(sessId As Guid, endDateTime As DateTimeOffset) Implements IServer.SetSessionCompleted
        CheckPermissions()
        SetSessionFinished(sessId, SessionStatus.Completed, endDateTime, Nothing)
    End Sub

    ''' <summary>
    ''' Updates a session's status to 'Stopped'.
    ''' </summary>
    ''' <param name="sessId">The session id</param>
    ''' <param name="endDateTime">The date/time (UTC) that the session finished</param>
    <SecuredMethod(Permission.Resources.ControlResource, Permission.Resources.AuthenticateAsResource)>
    Public Sub SetSessionStopped(sessId As Guid, endDateTime As DateTimeOffset) Implements IServer.SetSessionStopped
        CheckPermissions()
        SetSessionFinished(sessId, SessionStatus.Stopped, endDateTime, Nothing)
    End Sub

#End Region

    ''' <summary>
    ''' Deletes a session from the database.
    ''' </summary>
    ''' <param name="sessionId">The session id</param>
    <SecuredMethod(Permission.Resources.ControlResource)>
    Public Sub DeleteSession(sessionId As Guid) Implements IServer.DeleteSession
        CheckPermissions()
        Using connection = GetConnection()
            If GetControllingUserPermissionSetting(connection) Then
                Throw New PermissionException(My.Resources.clsServer_CannotDeleteSessionUsingUserIDBecauseTokenValidationIsEnforced)
            End If

            DeleteSession(connection, sessionId)
        End Using
    End Sub

    ''' <summary>
    ''' Deletes a session from the database.
    ''' </summary>
    ''' <param name="token">An authorisation token</param>
    ''' <param name="sessionId">The session id</param>
    <SecuredMethod(Permission.Resources.ControlResource)>
    Public Sub DeleteSessionAs(token As String, sessionId As Guid) Implements IServer.DeleteSessionAs
        CheckPermissions()
        Using connection = GetConnection()
            If Not ValidateUserCanPerformAction(token, sessionId, connection, Permission.ProcessStudio.AllProcessPermissions) Then
                Throw New PermissionException(My.Resources.clsServer_UserDoesNotHavePermissionToPerformThisAction)
            End If

            DeleteSession(connection, sessionId)
        End Using
    End Sub

    Private Sub DeleteSession(connection As IDatabaseConnection, sessionId As Guid)
        Using command As New SqlCommand("delete from BPASession where sessionid = @id")
            command.Parameters.AddWithValue("@id", sessionId)
            connection.Execute(command)
        End Using
    End Sub

    ''' <summary>
    ''' Get the startup parameters (in XML format) that were used to start a given
    ''' session.
    ''' </summary>
    ''' <param name="sessionID">The ID of the session.</param>
    ''' <returns>The startup parameters used, or Nothing if there weren't any. Throws
    ''' an exception if something goes wrong.</returns>
    <SecuredMethod(Permission.Resources.ViewResource, Permission.Resources.ViewResourceScreenCaptures, Permission.Resources.ConfigureResource, Permission.Resources.ControlResource)>
    Public Function GetSessionStartParams(ByVal sessionID As Guid) As String Implements IServer.GetSessionStartParams
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("select startparamsxml from BPVSession where sessionid=@id")
            cmd.Parameters.AddWithValue("@id", sessionID)
            Return IfNull(Of String)(con.ExecuteReturnScalar(cmd), Nothing)
        End Using
    End Function

    ''' <summary>
    ''' Get the status of the specified session
    ''' </summary>
    ''' <param name="sessId">The ID of a particluar session to get information about
    ''' </param>
    ''' <returns>The session status, or SessionStatus.All if unknown</returns>
    <SecuredMethod(Permission.Resources.ViewResource, Permission.Resources.ViewResourceScreenCaptures, Permission.Resources.ConfigureResource, Permission.Resources.ControlResource)>
    Public Function GetSessionStatus(ByVal sessId As Guid) As SessionStatus Implements IServer.GetSessionStatus
        CheckPermissions()
        Using con = GetConnection()
            Try
                Dim cmd As New SqlCommand(
                 " select statusid" &
                 " from BPASession" &
                 " where sessionid = @sessId")
                cmd.Parameters.AddWithValue("@sessId", sessId)
                Using reader = con.ExecuteReturnDataReader(cmd)
                    If Not reader.Read() Then Return SessionStatus.All
                    With New ReaderDataProvider(reader)
                        Return .GetValue("statusid", SessionStatus.All)
                    End With
                End Using

            Catch
                Return SessionStatus.All

            End Try
        End Using
    End Function


    ''' <summary>
    ''' Gets the statuses of all identified sessions and returns the dictionary
    ''' containing that status.
    ''' </summary>
    ''' <param name="dict">The dictionary containing the Session IDs for which the
    ''' session statuses are required</param>
    ''' <returns>The given dictionary, populated with the current session statuses.
    ''' </returns>
    <SecuredMethod(Permission.Resources.ViewResource, Permission.Resources.ViewResourceScreenCaptures, Permission.Resources.ConfigureResource, Permission.Resources.ControlResource)>
    Public Function GetSessionStatus(ByVal dict As IDictionary(Of Guid, SessionStatus)) _
     As IDictionary(Of Guid, SessionStatus) Implements IServer.GetSessionStatus
        CheckPermissions()
        If dict Is Nothing OrElse dict.Count = 0 Then Return dict ' if only vb could roll its eyes..

        ' Init all statuses to a seriously misnamed 'unset' value before getting the latest.
        For Each id As Guid In New List(Of Guid)(dict.Keys)
            dict(id) = SessionStatus.All
        Next

        Using con = GetConnection()
            Dim cmd As New SqlCommand()
            Dim sb As New StringBuilder("select sessionid, statusid from BPASession where sessionid in (")
            Dim index As Integer = 0
            For Each id As Guid In dict.Keys
                index += 1
                sb.AppendFormat("@id{0},", index)
                cmd.Parameters.AddWithValue("@id" & index, id)
            Next
            sb.Length -= 1
            cmd.CommandText = sb.Append(")").ToString()
            Using reader = con.ExecuteReturnDataReader(cmd)
                While reader.Read()

                    Dim sessionId As Guid = CType(reader("sessionid"), Guid)
                    Dim status As SessionStatus = CType(reader("statusid"), SessionStatus)

                    dict(sessionId) = status

                End While
            End Using
            Return dict
        End Using

    End Function

    ''' <summary>
    ''' Gets the first Session ID found matching the incomplete GUID string.
    ''' </summary>
    ''' <param name="sIncomplete">An incomplete GUID string.</param>
    ''' <returns>The first matching GUID found, or Guid.Empty if no match is found.</returns>
    <SecuredMethod(Permission.Resources.ViewResource, Permission.Resources.ViewResourceScreenCaptures, Permission.Resources.ConfigureResource, Permission.Resources.ControlResource)>
    Public Function GetCompleteSessionID(ByVal sIncomplete As String) As Guid Implements IServer.GetCompleteSessionID
        CheckPermissions()
        Dim cmd As New SqlCommand("SELECT sessionid FROM BPVSession WHERE sessionid LIKE @Incomplete")
        With cmd.Parameters
            .AddWithValue("@Incomplete", sIncomplete & "%")
        End With
        Return GetCompleteGuid(cmd)
    End Function

    ''' <summary>
    ''' Gets the first Resource ID found matching the incomplete GUID string.
    ''' </summary>
    ''' <param name="sIncomplete">An incomplete GUID string.</param>
    ''' <returns>The first matching GUID found, or Guid.Empty if no match is found.</returns>
    <SecuredMethod(True)>
    Public Function GetCompleteResourceID(ByVal sIncomplete As String) As Guid Implements IServer.GetCompleteResourceID
        CheckPermissions()
        Dim cmd As New SqlCommand("SELECT resourceid FROM BPAResource WHERE resourceid LIKE @Incomplete")
        With cmd.Parameters
            .AddWithValue("@Incomplete", sIncomplete & "%")
        End With
        Return GetCompleteGuid(cmd)
    End Function

    ''' <summary>
    ''' Gets the first Process ID found matching the incomplete GUID string.
    ''' </summary>
    ''' <param name="sIncomplete">An incomplete GUID string.</param>
    ''' <returns>The first matching GUID found, or Guid.Empty if no match is found.</returns>
    <SecuredMethod(True)>
    Public Function GetCompleteProcessID(ByVal sIncomplete As String) As Guid Implements IServer.GetCompleteProcessID
        CheckPermissions()
        Dim cmd As New SqlCommand("SELECT processid FROM BPAProcess WHERE processid LIKE @Incomplete")
        With cmd.Parameters
            .AddWithValue("@Incomplete", sIncomplete & "%")
        End With
        Return GetCompleteGuid(cmd)
    End Function

    ''' <summary>
    ''' Gets the first Process ID found matching the incomplete GUID string.
    ''' </summary>
    ''' <param name="sIncomplete">An incomplete Guid string.</param>
    ''' <returns>The first matching Guid found, or Guid.Empty if no match is found.</returns>
    <SecuredMethod(True)>
    Public Function GetCompletePoolID(ByVal sIncomplete As String) As Guid Implements IServer.GetCompletePoolID
        CheckPermissions()
        Dim cmd As New SqlCommand("SELECT resourceid FROM BPAResource WHERE resourceid LIKE @Incomplete AND (AttributeID & @attr) <> 0")
        With cmd.Parameters
            .AddWithValue("@Incomplete", sIncomplete & "%")
            .AddWithValue("@attr", ResourceAttribute.Pool)
        End With
        Return GetCompleteGuid(cmd)
    End Function

    ''' <summary>
    ''' Gets the first User ID found matching the incomplete GUID string.
    ''' </summary>
    ''' <param name="sIncomplete">An incomplete GUID string.</param>
    ''' <returns>The first matching GUID found, or Guid.Empty if no match is found.</returns>
    <SecuredMethod(True)>
    Public Function GetCompleteUserID(ByVal sIncomplete As String) As Guid Implements IServer.GetCompleteUserID
        CheckPermissions()
        Dim cmd As New SqlCommand("SELECT userid FROM BPAUser WHERE userid LIKE @Incomplete")
        With cmd.Parameters
            .AddWithValue("@Incomplete", sIncomplete & "%")
        End With
        Return GetCompleteGuid(cmd)
    End Function

    ''' <summary>
    ''' Gets the first GUID found matching the incomplete GUID string.
    ''' </summary>
    ''' <param name="cmd">A sqlcommand to find complete guid or guids matching a
    ''' partial guid</param>
    ''' <returns>The first matching GUID found, or Guid.Empty if no match is found.</returns>
    Private Function GetCompleteGuid(ByVal cmd As SqlCommand) As Guid
        Dim con = GetConnection()
        Dim reader As IDataReader = Nothing
        Dim id As Guid

        Try
            reader = con.ExecuteReturnDataReader(cmd)
            If reader.Read Then
                id = reader.GetGuid(0)
                If Not reader.Read Then
                    'The reader only has 1 row, so the the guid found must be the right one.
                    Return id
                Else
                    'The reader has more rows, so the partial guid must be ambiguous.
                    Return Guid.Empty
                End If
            Else
                'The reader has no rows, so no match was found.
                Return Guid.Empty
            End If
        Catch e As Exception
            Return Guid.Empty
        Finally
            If Not reader Is Nothing Then reader.Close()
            con.Close()
        End Try

    End Function

    ''' <summary>
    ''' Gets the sessions created on behalf of the queue with the given identity.
    ''' </summary>
    ''' <param name="ident">The identity of the active queue for which the
    ''' current sessions are required.</param>
    ''' <param name="statuses">The statuses of the sessions to return. If none are
    ''' supplied, all sessions will be retrieved</param>
    ''' <returns>A collection of process sessions which were created on behalf of the
    ''' given queue</returns>
    <SecuredMethod(Permission.Resources.ViewResource, Permission.Resources.ViewResourceScreenCaptures, Permission.Resources.ConfigureResource, Permission.Resources.ControlResource)>
    Public Function GetSessionsForQueue(
     ident As Integer, ParamArray statuses() As SessionStatus) _
     As ICollection(Of clsProcessSession) Implements IServer.GetSessionsForQueue
        CheckPermissions()
        Using con = GetConnection()
            Return GetSessionsForQueue(con, ident, statuses)
        End Using
    End Function

    ''' <summary>
    ''' Gets the sessions currently active on behalf of the queue with the given
    ''' identity.
    ''' </summary>
    ''' <param name="con">The connection over which to retrieve the active sessions.
    ''' </param>
    ''' <param name="ident">The identity of the active queue for which the
    ''' current sessions are required.</param>
    ''' <param name="statuses">The statuses of the sessions to return. If none are
    ''' supplied, all sessions will be retrieved</param>
    ''' <returns>A collection of process sessions which were created on behalf of the
    ''' given queue</returns>
    Private Function GetSessionsForQueue(
     con As IDatabaseConnection,
     ident As Integer,
     ParamArray statuses() As SessionStatus) As ICollection(Of clsProcessSession)
        ' Create the command
        Dim cmd As New SqlCommand()

        ' Begin the query - this is needed with or without statuses
        Dim sb As New StringBuilder(500)
        sb.Append(
         " select" &
         "   s.sessionid," &
         "   s.sessionnumber," &
         "   s.startdatetime," &
         "   s.enddatetime," &
         "   s.processid," &
         "   s.starterresourceid," &
         "   s.starteruserid," &
         "   s.runningresourceid," &
         "   s.runningosusername," &
         "   s.statusid," &
         "   s.startparamsxml," &
         "   s.processname," &
         "   s.starterresourcename," &
         "   s.starterusername," &
         "   s.runningresourcename" &
         " from BPVSessionInfo s" &
         "   where s.queueid = @ident"
        )
        cmd.Parameters.AddWithValue("@ident", ident)

        ' If statuses have been provided, limit the returned data to those given
        If statuses.Length > 0 Then
            sb.Append(" and s.statusid in (")
            Dim i As Integer = 0
            For Each status As SessionStatus In statuses
                If i <> 0 Then sb.Append(","c)
                sb.Append("@status").Append(i)
                cmd.Parameters.AddWithValue("@status" & i, CInt(status))
                i += 1
            Next
            sb.Append(")"c)
        End If

        ' Set the query into the command
        cmd.CommandText = sb.ToString()

        ' Run and extract the data into clsProcessSession objects
        Dim sessions As New List(Of clsProcessSession)

        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            While reader.Read()
                sessions.Add(New clsProcessSession(prov))
            End While
        End Using

        ' Update the session with it's permissions
        sessions.ForEach(Sub(x)
                             x.ProcessPermissions = GetEffectiveMemberPermissionsForProcess(x.ProcessID)
                             x.ResourcePermissions = GetEffectiveMemberPermissions(con, New ResourceGroupMember(x.ResourceID))
                         End Sub)

        Return sessions

    End Function


    ''' <summary>
    ''' Gets session with the given ID.
    ''' </summary>
    ''' <param name="sessionId">The sessionid value for which the session data is
    ''' required.</param>
    ''' <returns>A clsProcessSession object containing the data for the specified session,
    ''' or null if the session was not found on the database.</returns>
    <SecuredMethod(Permission.SystemManager.Audit.ProcessLogs, Permission.SystemManager.Audit.BusinessObjectsLogs)>
    Public Function GetActualSessionById(sessionId As Guid) As clsProcessSession Implements IServer.GetActualSessionById
        CheckPermissions()
        Using con = GetConnection()
            Return GetActualSessionById(con, sessionId)
        End Using
    End Function


    ''' <summary>
    ''' Gets the session with given Id
    ''' identity.
    ''' </summary>
    ''' <param name="con">The connection over which to retrieve the active session.
    ''' </param>
    ''' <param name="sessionId">The sessionid value for which the session data is
    ''' required.</param>
    ''' <returns>A clsProcessSession object containing the data for the specified session,
    ''' or null if the session was not found on the database</returns>
    Private Function GetActualSessionById(con As IDatabaseConnection, sessionId As Guid) As clsProcessSession
        Dim cmd As New SqlCommand()

        Dim queryBuilder As New StringBuilder(500)
        queryBuilder.Append(
         " select" &
         "   s.sessionid," &
         "   s.sessionnumber," &
         "   s.startdatetime," &
         "   s.starttimezoneoffset," &
         "   s.enddatetime," &
         "   s.endtimezoneoffset," &
         "   s.processid," &
         "   s.starterresourceid," &
         "   s.starteruserid," &
         "   s.runningresourceid," &
         "   s.runningosusername," &
         "   s.statusid," &
         "   s.startparamsxml," &
         "   s.processname," &
         "   s.starterresourcename," &
         "   s.starterusername," &
         "   s.runningresourcename," &
         "   s.queueid," &
         "   s.laststage," &
         "   s.exceptionmessage," &
         "   s.exceptiontype," &
         "   s.terminationreason," &
         "   s.lastupdated," &
         "   s.lastupdatedtimezoneoffset" &
         " from BPVSessionInfo s" &
         " where s.sessionid = @sessionid" &
         " and s.statusid <> 5 " &
         MteSqlGenerator.MteToken
         )

        With cmd.Parameters
            .AddWithValue("@sessionid",
                          IIf(sessionId = Nothing, DBNull.Value, sessionId))
        End With

        Const sessionTableAlias As String = "s"
        Dim sql = New MteSqlGenerator(queryBuilder.ToString(), sessionTableAlias)
        Dim query = sql.GetQueryAndSetParameters(mLoggedInUser, cmd)

        cmd.CommandText = query

        Using reader = con.ExecuteReturnDataReader(cmd)
            If Not reader.Read() Then Return Nothing
            Return New clsProcessSession(New ReaderDataProvider(reader))
        End Using
    End Function

    ''' <summary>
    ''' Gets the actual session objects from the database representing all sessions
    ''' </summary>
    ''' <returns>A collection of session objects representing all sessions in the
    ''' database.</returns>
    <SecuredMethod(Permission.SystemManager.Audit.ProcessLogs, Permission.SystemManager.Audit.BusinessObjectsLogs)>
    Public Function GetActualSessions() As ICollection(Of clsProcessSession) Implements IServer.GetActualSessions
        CheckPermissions()
        Return GetActualSessions(Guid.Empty)
    End Function

    ''' <summary>
    ''' Gets the actual session objects from the database from a specific resource
    ''' </summary>
    ''' <param name="resourceName">The name of the resource for which all sessions
    ''' are required or null to return all sessions for all resources.</param>
    ''' <returns>A collection of sessions from the given resource, or all sessions if
    ''' no resource name was given.</returns>
    <SecuredMethod(Permission.SystemManager.Audit.ProcessLogs, Permission.SystemManager.Audit.BusinessObjectsLogs)>
    Public Function GetActualSessions(resourceName As String) _
     As ICollection(Of clsProcessSession) Implements IServer.GetActualSessions
        CheckPermissions()
        Using con = GetConnection()
            Return GetActualSessions(con, Nothing, If(resourceName Is Nothing, Nothing, GetSingleton.ICollection(resourceName)))
        End Using
    End Function

    ''' <summary>
    ''' Gets the actual session objects from the database from a specific set of
    ''' resources
    ''' </summary>
    ''' <param name="resourceNames">The names of the resources for which all sessions
    ''' are required or null/empty to return all sessions for all resources.</param>
    ''' <returns>A collection of sessions from the given resources, or all sessions
    ''' if no resource name was given.</returns>
    <SecuredMethod(Permission.SystemManager.Audit.ProcessLogs, Permission.SystemManager.Audit.BusinessObjectsLogs)>
    Public Function GetActualSessions(resourceNames As ICollection(Of String)) _
     As ICollection(Of clsProcessSession) Implements IServer.GetActualSessions
        CheckPermissions()
        Using con = GetConnection()
            Return GetActualSessions(con, Nothing, resourceNames)
        End Using
    End Function

    ''' <summary>
    ''' Gets the actual session object from the database for a specific session.
    ''' </summary>
    ''' <param name="sessionId">The ID of the session for which an actual
    ''' <see cref="clsProcessSession"/> objects is required or
    ''' <see cref="Guid.Empty"/> to return all sessions.</param>
    ''' <returns>The session object corresponding to the given session ID in a
    ''' collection, -or- all sessions if <see cref="Guid.Empty"/> is given -or-
    ''' an empty collection if no session with the given ID was found, or if there
    ''' were no sessions in the database.</returns>
    <SecuredMethod(Permission.SystemManager.Audit.ProcessLogs, Permission.SystemManager.Audit.BusinessObjectsLogs)>
    Public Function GetActualSessions(sessionId As Guid) _
     As ICollection(Of clsProcessSession) Implements IServer.GetActualSessions
        CheckPermissions()
        Using con = GetConnection()
            Return GetActualSessions(con, sessionId, Nothing)
        End Using
    End Function

    <SecuredMethod(Permission.Resources.ManageResourceAccessrights,
                   Permission.Resources.ViewResource,
                   Permission.Resources.ViewResourceScreenCaptures,
                   Permission.Resources.ConfigureResource,
                   Permission.Resources.ControlResource)>
    Public Function GetSessionsForResource(resourceName As String) _
        As ICollection(Of clsProcessSession) Implements IServer.GetSessionsForResource
        CheckPermissions()
        Using con = GetConnection()
            Return GetActualSessions(con, Nothing, If(resourceName Is Nothing, Nothing, GetSingleton.ICollection(resourceName)))
        End Using
    End Function

    <SecuredMethod(Permission.Resources.ManageResourceAccessrights,
                Permission.Resources.ViewResource,
                Permission.Resources.ViewResourceScreenCaptures,
                Permission.Resources.ConfigureResource,
                Permission.Resources.ControlResource)>
    Public Function GetSessionsForAllResources() As ICollection(Of clsProcessSession) Implements IServer.GetSessionsForAllResources
        CheckPermissions()
        Using con = GetConnection()
            Return GetActualSessions(con, Nothing, Nothing)
        End Using
    End Function

    ''' <summary>
    ''' Gets actual session objects from the database, constraining to a specific
    ''' session ID or a specific resource as requested.
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="sessionId">The ID of the session for which the session object
    ''' is required, or <see cref="Guid.Empty"/> if no session ID constraint should
    ''' be applied (ie. all sessions which pass any other constraints will be
    ''' returned).</param>
    ''' <param name="resNames">The names of the resources from which the session
    ''' objects are required, or null/empty to return all otherwise unconstrained
    ''' sessions. Note that if <paramref name="sessionId"/> is set, this argument has
    ''' no effect.</param>
    ''' <returns>The session objects of the sessions which passed the constraints of
    ''' the parameters.</returns>
    Private Function GetActualSessions(
     con As IDatabaseConnection, sessionId As Guid, resNames As ICollection(Of String)) _
     As ICollection(Of clsProcessSession)
        ' Replace the null with an empty collection just to make the checking code
        ' later a bit easier to follow
        If resNames Is Nothing Then resNames = GetEmpty.ICollection(Of String)()

        ' Create the command
        Dim cmd As New SqlCommand()

        ' Begin the query - this is needed with or without statuses
        Dim sb As New StringBuilder(500)
        sb.Append(
         " select" &
         "   s.sessionid," &
         "   s.sessionnumber," &
         "   s.startdatetime," &
         "   s.starttimezoneoffset," &
         "   s.enddatetime," &
         "   s.endtimezoneoffset," &
         "   s.processid," &
         "   s.starterresourceid," &
         "   s.starteruserid," &
         "   s.runningresourceid," &
         "   s.runningosusername," &
         "   s.statusid," &
         "   s.startparamsxml," &
         "   s.processname," &
         "   s.starterresourcename," &
         "   s.starterusername," &
         "   s.runningresourcename," &
         "   s.queueid" &
         " from BPVSessionInfo s"
        )

        ' Add the constraints, if there are any
        If sessionId <> Guid.Empty Then
            sb.Append(" where s.sessionid = @sessionid")
            cmd.Parameters.AddWithValue("@sessionid", sessionId)

        ElseIf resNames.Count > 0 Then
            sb.Append(" where s.runningresourcename in (")
            Dim i As Integer = 0
            For Each res As String In resNames
                If i > 0 Then sb.Append(","c)
                i += 1
                sb.Append("@res").Append(i)
                cmd.Parameters.AddWithValue("@res" & i, res)
            Next
            sb.Append(")"c)

        End If

        ' Set the query into the command
        cmd.CommandText = sb.ToString()

        ' Run and extract the data into clsProcessSession objects
        Dim sesses As New List(Of clsProcessSession)
        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            While reader.Read() : sesses.Add(New clsProcessSession(prov)) : End While
        End Using

        ' Update the session with it's permissions
        sesses.ForEach(Sub(x)
                           x.ProcessPermissions = GetEffectiveMemberPermissionsForProcess(x.ProcessID)
                           x.ResourcePermissions = GetEffectiveMemberPermissions(con, New ResourceGroupMember(x.ResourceID))
                       End Sub)

        Return sesses

    End Function

    <SecuredMethod>
    Public Function GetActualSessionsFilteredAndOrdered(processSessionParameters As ProcessSessionParameters) As ICollection(Of clsProcessSession) Implements IServer.GetActualSessionsFilteredAndOrdered
        CheckPermissions()
        Using connection = GetConnection()
            Return GetActualSessionsFilteredAndOrdered(connection, processSessionParameters)
        End Using
    End Function

    Private Function GetActualSessionsFilteredAndOrdered(connection As IDatabaseConnection, processSessionParameters As ProcessSessionParameters) _
        As IList(Of clsProcessSession)
        Dim queryBuilder As New StringBuilder(500)
        queryBuilder.Append(
                    $" select top {processSessionParameters.ItemsPerPage}" &
                    "   s.sessionid," &
                    "   s.sessionnumber," &
                    "   s.startdatetime," &
                    "   s.starttimezoneoffset," &
                    "   s.enddatetime," &
                    "   s.endtimezoneoffset," &
                    "   s.lastupdated," &
                    "   s.lastupdatedtimezoneoffset," &
                    "   s.processid," &
                    "   s.starterresourceid," &
                    "   s.starteruserid," &
                    "   s.runningresourceid," &
                    "   s.runningosusername," &
                    "   s.statusid," &
                    "   s.startparamsxml," &
                    "   s.processname," &
                    "   s.starterresourcename," &
                    "   s.runningresourcename," &
                    "   s.queueid," &
                    "   s.laststage," &
                    "   s.starterusername," &
                    "   s.exceptionmessage," &
                    "   s.exceptiontype," &
                    "   s.terminationreason" &
                    " from BPVSessionInfo AS s" &
                    " where s.statusid <> 5"
                    )

        Using sqlCommand As New SqlCommand With
        {
            .CommandTimeout = Options.Instance.SqlCommandTimeoutLong
        }

            Dim sqlData = processSessionParameters _
                .GetSqlWhereClauses(sqlCommand) _
                .GetSqlWhereWithParametersStartingWithAndKeyword()

            sqlCommand.Parameters.AddRange(sqlData.sqlParameters)

            queryBuilder.AppendLine(sqlData.sqlWhereClause)
            queryBuilder.Append(MteSqlGenerator.MteToken)
            queryBuilder.Append($" order by {processSessionParameters.GetSqlOrderByClauses()}")


            Const sessionTableAlias As String = "s"
            Dim mteQuery = New MteSqlGenerator(queryBuilder.ToString(), sessionTableAlias)
            Dim query = mteQuery.GetQueryAndSetParameters(mLoggedInUser, sqlCommand)

            sqlCommand.CommandText = query

            Dim sessions As New List(Of clsProcessSession)
            Using reader = connection.ExecuteReturnDataReader(sqlCommand)
                Dim readerDataProvider As New ReaderDataProvider(reader)
                While reader.Read() : sessions.Add(New clsProcessSession(readerDataProvider)) : End While
            End Using

            sessions.ForEach(Sub(x)
                                 x.ProcessPermissions = GetEffectiveMemberPermissionsForProcess(x.ProcessID)
                                 x.ResourcePermissions = GetEffectiveMemberPermissions(connection, New ResourceGroupMember(x.ResourceID))
                             End Sub)

            Return sessions
        End Using
    End Function

    ''' <summary>
    ''' Counts the number of concurrent sessions currently in the database, ie. those
    ''' sessions which have a state of PENDING or RUNNING (statusid of 0 or 1,
    ''' respectively).
    ''' </summary>
    ''' <returns>The number of PENDING or RUNNING sessions registered in the database
    ''' </returns>
    <SecuredMethod(Permission.Resources.ViewResource, Permission.Resources.ViewResourceScreenCaptures, Permission.Resources.ConfigureResource, Permission.Resources.ControlResource)>
    Public Function CountConcurrentSessions() As Integer Implements IServer.CountConcurrentSessions
        CheckPermissions()
        Using con = GetConnection()
            Return CountConcurrentSessions(con, Nothing)
        End Using
    End Function

    ''' <summary>
    ''' Counts the number of concurrent sessions currently in the database, ie. those
    ''' sessions which have a state of PENDING or RUNNING (statusid of 0 or 1,
    ''' respectively), optionally excluding sessions with specified IDs.
    ''' </summary>
    ''' <param name="excluding">The session IDs to be excluded from the count. A null
    ''' or empty collection excludes no sessions from the count.</param>
    ''' <returns>The number of PENDING or RUNNING sessions registered in the database
    ''' not including those with the IDs specified in <paramref name="excluding"/>.
    ''' </returns>
    <SecuredMethod(Permission.Resources.ViewResource, Permission.Resources.ViewResourceScreenCaptures, Permission.Resources.ConfigureResource, Permission.Resources.ControlResource)>
    Public Function CountConcurrentSessions(
     ByVal excluding As ICollection(Of Guid)) As Integer Implements IServer.CountConcurrentSessions
        CheckPermissions()
        Using con = GetConnection()
            Return CountConcurrentSessions(con, excluding)
        End Using
    End Function

    ''' <summary>
    ''' Counts the number of concurrent sessions currently in the database, ie. those
    ''' sessions which have a state of PENDING or RUNNING (statusid of 0 or 1,
    ''' respectively), optionally excluding sessions with specified IDs.
    ''' </summary>
    ''' <param name="con">The connection to use to access the database.</param>
    ''' <param name="excluding">The session IDs to be excluded from the count. A null
    ''' or empty collection excludes no sessions from the count.</param>
    ''' <returns>The number of PENDING or RUNNING sessions registered in the database
    ''' not including those with the IDs specified in <paramref name="excluding"/>.
    ''' </returns>
    Private Function CountConcurrentSessions(
     ByVal con As IDatabaseConnection, ByVal excluding As ICollection(Of Guid)) As Integer
        If excluding Is Nothing Then excluding = GetEmpty.ICollection(Of Guid)()
        Dim cmd As New SqlCommand()
        Dim sb As New StringBuilder(
         " SELECT COUNT(*) FROM BPASession" &
         " WHERE statusid IN (0,1,7)")
        If excluding.Count > 0 Then
            sb.Append(" AND sessionid NOT IN (")
            Dim i As Integer = 0
            For Each id As Guid In excluding
                cmd.Parameters.AddWithValue("@id" & i, id)
                If i > 0 Then sb.Append(","c)
                sb.Append("@id").Append(i)
                i += 1
            Next
            sb.Append(")")
        End If
        cmd.CommandText = sb.ToString()
        Return CInt(con.ExecuteReturnScalar(cmd))
    End Function

    ''' <summary>
    ''' Reads pending session data from the database.
    ''' </summary>
    ''' <returns>A data table of session data</returns>
    <SecuredMethod(Permission.Resources.ViewResource, Permission.Resources.ViewResourceScreenCaptures, Permission.Resources.ConfigureResource, Permission.Resources.ControlResource, Permission.Resources.AuthenticateAsResource)>
    Public Function GetPendingSessions() As DataTable Implements IServer.GetPendingSessions
        CheckPermissions()
        Using con = GetConnection()
            Try
                Return con.ExecuteReturnDataTable(New SqlCommand(
                 " select sessionid," &
                 "   processid," &
                 "   starterresourceid," &
                 "   starteruserid," &
                 "   runningresourceid," &
                 "   queueid" &
                 " from BPVSession" &
                 " where statusid = 0"
                ))
            Catch
                Return Nothing
            End Try
        End Using
    End Function

    ''' <summary>
    ''' Reads running or stopping session details from the database.
    ''' </summary>
    ''' <param name="resourceID">The resourceid for which to get sessions</param>
    ''' <returns> A datatable with columns sessionid, startdatetime, enddatetime,
    ''' processid, starterresourceid, starteruserid and runningresourceid.</returns>
    ''' <remarks>Throws an exception if an error occurs.</remarks>
    <SecuredMethod(Permission.Resources.ViewResource, Permission.Resources.ViewResourceScreenCaptures, Permission.Resources.ConfigureResource, Permission.Resources.ControlResource, Permission.Resources.AuthenticateAsResource)>
    Public Function GetRunningOrStoppingSessions(resourceID As Guid) As DataTable Implements IServer.GetRunningOrStoppingSessions
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand(
             " select sessionid " &
             " from BPVSession  " &
             " where runningresourceid = @resourceid and " &
             "  statusid in (@running,@stopping)")

            With cmd.Parameters
                .AddWithValue("@resourceid", resourceID)
                .AddWithValue("@running", SessionStatus.Running)
                .AddWithValue("@stopping", SessionStatus.StopRequested)
            End With

            Return con.ExecuteReturnDataTable(cmd)
        End Using
    End Function


    ''' <summary>
    ''' Get sessions stopped after a certain time.
    ''' </summary>
    ''' <returns> A datatable with columns sessionid and enddatetime</returns>
    ''' <remarks>Throws an exception if an error occurs.</remarks>
    <SecuredMethod(Permission.Resources.ViewResource, Permission.Resources.ViewResourceScreenCaptures, Permission.Resources.ConfigureResource, Permission.Resources.ControlResource, Permission.Resources.AuthenticateAsResource)>
    Public Function GetSessionsEndedAfter(endedAfter As Date) As DataTable Implements IServer.GetSessionsEndedAfter
        CheckPermissions()

        Using con = GetConnection()

            Try
                con.BeginTransaction(IsolationLevel.Snapshot)

                Dim sqlQuery = $"select sessionid, enddatetime from BPASession s where isnull({GetSqlDateTimeOffset("s", "enddatetime", "endtimezoneoffset")}, {SqlMinDateTime}) > @date"

                Dim cmd As New SqlCommand(sqlQuery)


                With cmd.Parameters
                    .AddWithValue("@date", endedAfter)
                End With

                Dim result = con.ExecuteReturnDataTable(cmd)
                con.CommitTransaction()
                Return result
            Catch
                If con.InTransaction Then
                    con.RollbackTransaction()
                End If
            Finally
                con.ResetConnectionDefaultIsolationLevel()
            End Try

            Return New DataTable()
        End Using
    End Function

    ''' <inheritdoc/>
    <SecuredMethod(Permission.Resources.ManageResourceAccessrights,
                   Permission.Resources.ViewResource,
                   Permission.Resources.ViewResourceScreenCaptures,
                   Permission.Resources.ConfigureResource,
                   Permission.Resources.ControlResource)>
    Public Function GetFilteredSessions(
     processNames As ICollection(Of String),
     resourceName As ICollection(Of String),
     usernames As ICollection(Of String),
     sessStatus As SessionStatus,
     startDate As Date,
     endDate As Date,
     updatedBefore As Date,
     localButRemote As Boolean,
     excludeAllLocal As Boolean,
     maxSessionCount As Integer,
     sortInfo As SessionSortInfo) As ICollection(Of clsProcessSession) Implements IServer.GetFilteredSessions
        CheckPermissions()

        'Local variable to mitigate race condition that can see mLoggedInUser becoming NULL while evaluating permissions
        Dim loggedInUserLocal = mLoggedInUser

        If Not (loggedInUserLocal IsNot Nothing AndAlso
                loggedInUserLocal.CanSeeTree(GroupTreeType.Processes) AndAlso
                loggedInUserLocal.CanSeeTree(GroupTreeType.Resources) AndAlso
                loggedInUserLocal.CanSeeTree(GroupTreeType.Users)) Then
            Return New List(Of clsProcessSession)
        End If

        Using con = GetConnection()
            Return GetFilteredSessions(
                con, processNames, resourceName, usernames, sessStatus,
                startDate, endDate, updatedBefore, localButRemote, excludeAllLocal, maxSessionCount, sortInfo)
        End Using
    End Function

    Private Function GetFilteredSessions(
     con As IDatabaseConnection,
     processNames As ICollection(Of String),
     resourceNames As ICollection(Of String),
     usernames As ICollection(Of String),
     sessStatus As SessionStatus,
     startDate As Date,
     endDate As Date,
     updatedBefore As Date,
     localButRemote As Boolean,
     excludeAllLocal As Boolean,
     maxSessionCount As Integer,
     sortInfo As SessionSortInfo) As ICollection(Of clsProcessSession)

        Const SessionTableAlias = "s"
        Const ProcessTableAlias = "p"
        Const ResourceTableAlias = "rr"
        Const UserTableAlias = "su"
        Const StatusTableAlias = "st"
        Const processNameAlias = "processname"
        Const userNameAlias = "starterusername"
        Const resourceNameAlias = "runningresourceName"

        Dim systemThreshold = GetStageWarningThreshold(con)

        Dim orderBySql = GetFilteredSessionsOrderBySql(sortInfo, SessionTableAlias,
                                                     StatusTableAlias, processNameAlias,
                                                     resourceNameAlias, userNameAlias,
                                                     systemThreshold)

        Dim cmd As New SqlCommand()
        Dim sb As New StringBuilder(
             $"select top (@top)
                      {SessionTableAlias}.sessionid,
                      {SessionTableAlias}.sessionnumber,
                      {SessionTableAlias}.startdatetime,
                      {SessionTableAlias}.starttimezoneoffset,
                      {SessionTableAlias}.enddatetime,
                      {SessionTableAlias}.endtimezoneoffset,
                      {SessionTableAlias}.processid,
                      {ProcessTableAlias}.name as {processNameAlias},
                      isnull({UserTableAlias}.username, '[' + {UserTableAlias}.systemusername + ']') as {userNameAlias},
                      {SessionTableAlias}.runningresourceid,
                      {ResourceTableAlias}.name as {resourceNameAlias},
                      {SessionTableAlias}.statusid,
                      {SessionTableAlias}.startparamsxml,
                      {SessionTableAlias}.logginglevelsxml,
                      {SessionTableAlias}.sessionstatexml,
                      {SessionTableAlias}.queueid,
                      {SessionTableAlias}.lastupdated,
                      {SessionTableAlias}.lastupdatedtimezoneoffset,
                      {SessionTableAlias}.laststage" &
                     If(systemThreshold = 0, "", $",{SessionTableAlias}.warningthreshold ") &
                     If(orderBySql.SelectSql <> "", $",{orderBySql.SelectSql}", "") &
             $" from BPASession {SessionTableAlias}
                        LEFT join BPAProcess {ProcessTableAlias} on {SessionTableAlias}.processid = {ProcessTableAlias}.processid
                        LEFT join BPAResource {ResourceTableAlias} on {SessionTableAlias}.runningresourceid = {ResourceTableAlias}.resourceid
                        LEFT join BPAUser {UserTableAlias} on {SessionTableAlias}.starteruserid = {UserTableAlias}.userid
                        LEFT join BPAStatus {StatusTableAlias} on {SessionTableAlias}.statusid = {StatusTableAlias}.statusid and {SessionTableAlias}.statusid <> 6
              where 1=1 ")

        cmd.Parameters.AddWithValue("@top", maxSessionCount)

        ' Note the 'where 1=1' above just allows all the optional stuff below
        ' to just append AND to the string, and it's immediately stripped out
        ' by the SQL Server statement compiler

        If processNames IsNot Nothing AndAlso processNames.Count > 0 _
            AndAlso Not processNames.Contains("All") Then
            sb.Append(" and p.name in (")
            Dim i As Integer = 0
            For Each nm As String In processNames
                If i > 0 Then sb.Append(","c)
                sb.Append("@pname").Append(i)
                cmd.Parameters.AddWithValue("@pname" & i, nm)
                i += 1
            Next
            sb.Append(")")
        End If

        If resourceNames IsNot Nothing AndAlso resourceNames.Count > 0 AndAlso
            Not resourceNames.Contains("All") Then
            sb.Append($" and {ResourceTableAlias}.name in (")
            Dim i As Integer = 0
            For Each nm As String In resourceNames
                If i > 0 Then sb.Append(","c)
                sb.Append("@rname").Append(i)
                cmd.Parameters.AddWithValue("@rname" & i, nm)
                i += 1
            Next
            sb.Append(")")
        End If

        If usernames IsNot Nothing AndAlso usernames.Count > 0 AndAlso Not usernames.Contains("All") Then
            sb.Append($" and isnull({UserTableAlias}.username, {UserTableAlias}.systemusername)  in (")
            Dim i As Integer = 0
            For Each nm As String In usernames
                If i > 0 Then sb.Append(","c)
                sb.Append("@username").Append(i)
                cmd.Parameters.AddWithValue("@username" & i, nm)
                i += 1
            Next
            sb.Append(")")
        End If

        If sessStatus <> SessionStatus.All Then
            sb.Append($" And {SessionTableAlias}.statusid = @status")
            cmd.Parameters.AddWithValue("@status", sessStatus)

        Else
            sb.Append($" And {SessionTableAlias}.statusid <> @status")
            cmd.Parameters.AddWithValue("@status", SessionStatus.Debugging)
        End If

        ' startDate, endDate and updatedBefore are UTC.
        ' {ProcessTableAlias}.startdatetime, {ProcessTableAlias}.enddatetime and {ProcessTableAlias}.updatedbefore are stored in the database in local time, along with an offset from UTC in seconds.
        ' Use this offset to convert these to UTC so they can be compared correctly.
        If startDate <> Nothing Then
            sb.Append($" and @start <= isnull({GetSqlDateTimeOffset(SessionTableAlias, "startdatetime", "starttimezoneoffset")} , {SqlMaxDateTime})")
            cmd.Parameters.AddWithValue("@start", startDate)
        End If

        If endDate <> Nothing Then
            sb.Append($" and @end <= isnull({GetSqlDateTimeOffset(SessionTableAlias, "enddatetime", "endtimezoneoffset")}, {SqlMaxDateTime})")
            cmd.Parameters.AddWithValue("@end", endDate)
        End If

        If updatedBefore <> Nothing Then
            sb.Append($" and isnull({GetSqlDateTimeOffset(SessionTableAlias, "lastupdated", "lastupdatedtimezoneoffset")}, {SqlMaxDateTime}) <= @updatedbefore")
            cmd.Parameters.AddWithValue("@updatedbefore", updatedBefore)
        End If

        If excludeAllLocal Then
            sb.Append($" and {ResourceTableAlias}.attributeid & @local = 0")
            cmd.Parameters.AddWithValue("@local", ResourceAttribute.Local)

        ElseIf Not localButRemote Then
            'Exclude sessions on remote machines which are marked as 'Local'.
            'In other words, we only include it if either a) it's not marked as local,
            'or b) the resource pc name matches our own pc name. Note that this
            'does not take into account the multi-port "PC:8182" hack, as many
            'things don't currently!
            sb.Append($" and ({ResourceTableAlias}.attributeid & @local = 0 or {ResourceTableAlias}.name = @machinename)")
            cmd.Parameters.AddWithValue("@local", ResourceAttribute.Local)
            cmd.Parameters.AddWithValue("@machinename", If(CObj(mLoggedInMachine), DBNull.Value))

        End If

        If sortInfo IsNot Nothing Then sb.Append($" {orderBySql.OrderBySql}")
        cmd.CommandText = sb.ToString()

        Dim sessions As New List(Of clsProcessSession)
        Try
            con.BeginTransaction(IsolationLevel.Snapshot)
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    sessions.Add(New clsProcessSession(prov))
                End While
            End Using
            con.CommitTransaction()
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
        ' trim sessions we don't have permission on
        Dim processPermissionsCache As New Dictionary(Of Guid, IMemberPermissions)
        Dim resourcePermissionsCache As New Dictionary(Of Guid, IMemberPermissions)
        Dim retSessions As New List(Of clsProcessSession)

        For Each session In sessions
            Dim CanSeeProcess = False
            Dim CanSeeResource = False
            If Not processPermissionsCache.ContainsKey(session.ProcessID) Then
                processPermissionsCache.Add(session.ProcessID,
                    GetEffectiveMemberPermissionsForProcess(con, session.ProcessID))
            End If
            If Not resourcePermissionsCache.ContainsKey(session.ResourceID) Then
                resourcePermissionsCache.Add(session.ResourceID,
                    GetEffectiveMemberPermissions(con, New ResourceGroupMember(session.ResourceID)))
            End If
            If processPermissionsCache(session.ProcessID).HasAnyPermissions(mLoggedInUser) Then
                session.ProcessPermissions = processPermissionsCache(session.ProcessID)
                CanSeeProcess = True
            End If
            If resourcePermissionsCache(session.ResourceID).HasPermission(mLoggedInUser, Permission.Resources.ImpliedViewResource) Then
                session.ResourcePermissions = resourcePermissionsCache(session.ResourceID)
                CanSeeResource = True
            End If
            If CanSeeProcess AndAlso CanSeeResource Then retSessions.Add(session)
        Next

        Return retSessions

    End Function

    ''' <summary>
    ''' Get the SQL select statement and order by clause for the GetFilteredSessions
    ''' query. Note that this method is very tightly coupled with the behaviour in
    ''' the UI (and other classes) as we are trying to ensure as far as possible that
    ''' the column we're sorting on is exactly the same as the data that is in the
    ''' listview column in the UI.
    ''' </summary>
    Private Function GetFilteredSessionsOrderBySql(sortInfo As SessionSortInfo,
                                                 sessionTableAlias As String,
                                                 statusTableAlias As String,
                                                 processNameAlias As String,
                                                 resourceNameAlias As String,
                                                 userNameAlias As String,
                                                 warningThreshold As Integer) As OrderBySqlContainer

        Dim selectSql = ""
        Dim orderBySql = ""
        Const sortColumnAlias = "sortcolumn"

        Select Case sortInfo.Column
            Case SessionManagementColumn.SessionNum
                orderBySql = GetSqlOrderByStatement("sessionnumber", sortInfo.Direction)

            Case SessionManagementColumn.ProcessName
                orderBySql = GetSqlOrderByStatement(processNameAlias, sortInfo.Direction)

            Case SessionManagementColumn.ResourceName
                orderBySql = GetSqlOrderByStatement(resourceNameAlias, sortInfo.Direction)

            Case SessionManagementColumn.UserName
                orderBySql = GetSqlOrderByStatement(userNameAlias, sortInfo.Direction)

            Case SessionManagementColumn.Status
                If warningThreshold <> 0 Then
                    selectSql = $"case when
                                    {sessionTableAlias}.statusid <> {CInt(SessionStatus.Running)} and
                                    {sessionTableAlias}.statusid <> {CInt(SessionStatus.StopRequested)} and
                                    isnull({sessionTableAlias}.lastupdated, {SqlMinDateTime}) <> {SqlMinDateTime} and
                                    DATEADD(second, {warningThreshold}, {GetSqlDateTimeOffset(sessionTableAlias, "lastupdated", "lastupdatedtimezoneoffset")}) < getutcdate()
                                then
                                    '{StalledStatusText}'
                                else
                                    case when
                                        {sessionTableAlias}.statusid = {CInt(SessionStatus.StopRequested)}
                                    then
                                        '{StopRequestedStatusText}'
                                    else
                                        case when
                                            {sessionTableAlias}.statusid is null
                                        then
                                            '{SessionStatus.All}'
                                        else
                                            {statusTableAlias}.description
                                        end
                                    end
                                end as {sortColumnAlias}"

                    orderBySql = GetSqlOrderByStatement(sortColumnAlias, sortInfo.Direction)

                Else
                    selectSql = $"case when
                                    {sessionTableAlias}.statusid is null
                                then
                                    '{SessionStatus.All}'
                                else
                                    {statusTableAlias}.description
                                end as {sortColumnAlias}"

                    orderBySql = GetSqlOrderByStatement(sortColumnAlias, sortInfo.Direction)

                End If

            Case SessionManagementColumn.StartTime
                selectSql = $"isnull({GetSqlDateTimeOffset(sessionTableAlias, "startdatetime", "starttimezoneoffset")} , {SqlMinDateTime}) as {sortColumnAlias}"

                orderBySql = GetSqlOrderByStatement(sortColumnAlias, sortInfo.Direction)

            Case SessionManagementColumn.EndTime
                selectSql = $"case when
                                {sessionTableAlias}.enddatetime is null or
                                {sessionTableAlias}.enddatetime = {SqlMinDateTime} or
                                {sessionTableAlias}.enddatetime >= {SqlMaxDateTime}
                            then
                                CONVERT(datetime, 0)
                            else
                                {GetSqlDateTimeOffset(sessionTableAlias, "enddatetime", "endtimezoneoffset")}
                            end as {sortColumnAlias}"

                orderBySql = GetSqlOrderByStatement(sortColumnAlias, sortInfo.Direction)

            Case SessionManagementColumn.LastStage
                selectSql = $"case when
                                {sessionTableAlias}.laststage is null or
                                {sessionTableAlias}.statusid = {CInt(SessionStatus.Completed)} or
                                {sessionTableAlias}.statusid = {CInt(SessionStatus.Failed)} or
                                {sessionTableAlias}.statusid = {CInt(SessionStatus.Archived)} or
                                {sessionTableAlias}.statusid = {CInt(SessionStatus.Stopped)}
                            then
                                ''
                            else
                                {sessionTableAlias}.laststage
                            end as {sortColumnAlias}"

                orderBySql = GetSqlOrderByStatement(sortColumnAlias, sortInfo.Direction)

            Case SessionManagementColumn.LastUpdated
                selectSql = $"case when
                                {sessionTableAlias}.lastupdated is null or
                                {sessionTableAlias}.statusid = {CInt(SessionStatus.Completed)} or
                                {sessionTableAlias}.statusid = {CInt(SessionStatus.Failed)} or
                                {sessionTableAlias}.statusid = {CInt(SessionStatus.Archived)} or
                                {sessionTableAlias}.statusid = {CInt(SessionStatus.Stopped)}
                            then
                                {SqlMinDateTime}
                            else
                                {GetSqlDateTimeOffset(sessionTableAlias, "lastupdated", "lastupdatedtimezoneoffset")}
                            end as {sortColumnAlias}"

                orderBySql = GetSqlOrderByStatement(sortColumnAlias, sortInfo.Direction)

            Case Else
                Throw New NotImplementedException($"Sorting has not been implemented for '{sortInfo.Column}'.")

        End Select

        Return New OrderBySqlContainer(selectSql, orderBySql)

    End Function

    Private ReadOnly Property StalledStatusText As String = BPUtil.GetAttributeValue(Of DescriptionAttribute)(SessionStatus.Stalled).Description
    Private ReadOnly Property StopRequestedStatusText As String = BPUtil.GetAttributeValue(Of DescriptionAttribute)(SessionStatus.Stalled).Description

    Private Function GetSqlOrderByStatement(columnName As String, direction As SessionSortInfo.SortDirection) As String
        Return $"order by {columnName} {GetOrderByDirection(direction)}"
    End Function

    Private Function GetSqlDateTimeOffset(tableAlias As String, dateTimeColumn As String, offsetColumn As String) As String
        Return $"DATEADD(second, isnull(-{tableAlias}.{offsetColumn}, 0), {tableAlias}.{dateTimeColumn})"
    End Function

    Private Function GetOrderByDirection(direction As SessionSortInfo.SortDirection) As String
        Select Case direction
            Case SessionSortInfo.SortDirection.Ascending
                Return "asc"
            Case SessionSortInfo.SortDirection.Descending
                Return "desc"
            Case Else
                Throw New NotImplementedException($"The direction '{direction}' is not recognised.")
        End Select

    End Function


    ''' <summary>
    ''' Updates the 'startparamsxml' field of a session record.
    ''' </summary>
    ''' <param name="sessionId">The session id</param>
    ''' <param name="xml">The start parameters XML</param>
    <SecuredMethod(True)>
    Public Sub SetProcessStartParams(sessionId As Guid, xml As String) Implements IServer.SetProcessStartParams
        CheckPermissions()
        Using connection = GetConnection()
            If GetControllingUserPermissionSetting(connection) Then
                Throw New PermissionException(My.Resources.clsServer_CannotStartSessionUsingUserIDBecauseTokenValidationIsEnforced)
            End If

            SetProcessStartParams(connection, sessionId, xml)
        End Using
    End Sub

    ''' <summary>
    ''' Updates the 'startparamsxml' field of a session record.
    ''' </summary>
    ''' <param name="token">An authorisation token</param>
    ''' <param name="sessionId">The session id</param>
    ''' <param name="xml">The start parameters XML</param>
    <SecuredMethod(True)>
    Public Sub SetProcessStartParamsAs(token As String, sessionId As Guid, xml As String) Implements IServer.SetProcessStartParamsAs
        CheckPermissions()
        Using connection = GetConnection()
            If Not ValidateUserCanPerformAction(token, sessionId, connection, Permission.ProcessStudio.ImpliedExecuteProcess) Then
                Throw New PermissionException(My.Resources.clsServer_UserDoesNotHavePermissionToPerformThisAction)
            End If

            SetProcessStartParams(connection, sessionId, xml)
        End Using
    End Sub

    Private Sub SetProcessStartParams(connection As IDatabaseConnection, sessionId As Guid, xml As String)
        If xml IsNot Nothing Then
            Using command As New SqlCommand("UPDATE BPASession SET startparamsxml = @StartParamsXML WHERE sessionid = @SessionID")
                With command.Parameters
                    .AddWithValue("@StartParamsXML", xml)
                    .AddWithValue("@SessionID", sessionId)
                End With
                connection.Execute(command)
            End Using
        End If
    End Sub

    Private Function ValidateUserCanPerformAction(token As String, sessionId As Guid, connection As IDatabaseConnection, permissions As String()) As Boolean
        Dim processId = GetProcessIDBySessionId(connection, sessionId)
        Dim user = ValidateAuthorisedUserToken(connection, token, processId, webService:=False)
        Return GetEffectiveGroupPermissionsForProcess(connection, processId).HasPermission(user, permissions)
    End Function

    ''' <summary>
    ''' Finds any debug sessions that were not able to be stopped successfully and either deletes or ends them correctly.
    ''' </summary>
    <SecuredMethod(True)>
    Public Sub CleanupFailedDebugSessions() Implements IServer.CleanupFailedDebugSessions
        CheckPermissions()
        Using connection = GetConnection()
            Try
                connection.BeginTransaction()

                Dim failedDebugSessionIds = GetFailedDebugSessions(connection)
                If failedDebugSessionIds.Any() Then
                    failedDebugSessionIds.ForEach(Sub(x) FinishDebugSession(connection, x, Date.UtcNow, SessionStatus.Failed))
                    AuditRecordArchiveEvent(connection, ArchiveOperationEventCode.Clean, String.Empty, $"Number of sessions closed: {failedDebugSessionIds.Count}.")
                End If

                connection.CommitTransaction()
            Catch ex As Exception
                connection.RollbackTransaction()
                Throw
            End Try
        End Using
    End Sub

    Private Function GetFailedDebugSessions(connection As IDatabaseConnection) As List(Of Guid)
        Dim query = New StringBuilder(
            "SELECT sessionid
             FROM BPASession
             WHERE enddatetime IS NULL
                AND statusid = 5
                AND startdatetime <= DATEADD(DAY, -7, GETUTCDATE());"
            )
        Dim results = New List(Of Guid)

        Using command As New SqlCommand()
            command.CommandText = query.ToString()

            Using reader = connection.ExecuteReturnDataReader(command)
                While reader.Read()
                    Dim row = reader.GetGuid(0)
                    results.Add(row)
                End While
            End Using
        End Using

        Return results
    End Function
End Class
