Imports System.Collections.Concurrent
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Reflection
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports System.Timers
Imports System.Web
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Resources.ResourceMachine
Imports BluePrism.AutomateAppCore.Commands
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.ClientServerResources.Core.Enums
Imports BluePrism.Common.Security
Imports BluePrism.Core.Logging
Imports BluePrism.Core.Resources
Imports BluePrism.Server.Domain.Models
Imports NLog
Imports BluePrism.ClientServerResources.Core.Events

''' Project  : Automate
''' Class    : clsListener
'''
''' <summary>
''' Provides the functionality of a Resource PC listening for incoming client
''' connections and handling requests and responses. This is the 'other end' of
''' clsTalker.
''' </summary>
''' <remarks>
''' NOTE: Dropped connections are not immediately detected. They are only noticed
''' during attempts to send status updates to all clients, and then apparently only
''' on the second attempt.
''' </remarks>
Public Class clsListener
    Implements ISessionNotifier
    Implements IListener

    Private Shared ReadOnly Log As ILogger = LogManager.GetCurrentClassLogger()

    ''' <summary>
    ''' The current version of the listener protocol. This is used to ensure that
    ''' a Listener and Talker are compatible. It should very rarely change. Version 1
    ''' starts at Automate V3 - previous versions were not independently numbered.
    ''' </summary>
    Public Const ListenerVersion As Integer = 4

    Private mPingTimeoutSeconds As Integer

    ''' <summary>
    ''' Timer to periodically refresh the resource's status to the db.
    ''' </summary>
    Private WithEvents mRefreshTimer As Timers.Timer

    Private ReadOnly mCommandFactoryFactory As Func(Of ListenerClient, ICommandFactory)


    ''' <summary>
    ''' Keeps track of changed session variables, in between polls. The key is a
    ''' String made up of the session ID and the variable name.
    ''' </summary>
    Private mChangedVars As New Dictionary(Of String, clsSessionVariable)

    ''' <summary>
    ''' Deals with a session variable being changed. Called by the runner.
    ''' </summary>
    ''' <param name="var">The updated session variable.</param>
    ''' <remarks>The information about the change is stored in a buffer which is a
    ''' Dictionary keyed on session ID and variable name. Thus, repeated updates to
    ''' the same variable's value in between flushes will result in only the most
    ''' recent change being present.</remarks>
    Private Sub VarChanged(ByVal var As clsSessionVariable) Implements ISessionNotifier.VarChanged
        Dim key As String = var.SessionID.ToString() & var.Name
        SyncLock mChangedVars
            mChangedVars(key) = var
        End SyncLock
    End Sub

    ''' <summary>
    ''' This should be called periodically to send session variable change
    ''' notifications. They are batched up and sent this way to avoid flooding the
    ''' output stream in the event that a process is doing something silly.
    ''' </summary>
    Private Sub DoVarChangeNotifications()
        SyncLock mChangedVars
            For Each var As clsSessionVariable In mChangedVars.Values
                AddNotification("VAR {0} {1}",
                 var.sessionID, var.ToEscapedString(True))
            Next
            mChangedVars.Clear()
        End SyncLock
    End Sub

    ''' <summary>
    ''' Event raised if the listener thread fails for any reason. This could be, for
    ''' example, because the database connection was lost or a database transaction
    ''' timed out. If this event is raised, the listener will be stopped, and the
    ''' Startup method can be called to start it again.
    ''' </summary>
    ''' <param name="sReason">A description of why the listener thread failed.</param>
    Public Event Failed(ByVal sReason As String) Implements IListener.Failed

    Private Sub RaiseFailed(message As String)
        Log.Debug("Fail raised, err {0}", message)
        RaiseEvent Failed(message)
    End Sub

    ''' <summary>
    ''' Stops all monitoring activity (includes stopping the active TcpListener if available),
    ''' updates the status to stopped and raises the Failed event.
    ''' </summary>
    ''' <param name="listener">The listener that will be stopped if not nothing</param>
    ''' <param name="msg">The error message that will be displayed on failure</param>
    Private Sub StopAndRaiseFailed(listener As TcpListener, msg As String)
        StopRunning(listener)
        RaiseFailed(msg)
    End Sub

    ''' <summary>
    ''' Stops all monitoring activity (includes stopping the active TcpListener if available)
    ''' and updates the status to stopped. This leaves the listener in a state where it
    ''' can be started again using the StartUp function
    ''' </summary>
    ''' <param name="listener">The listener that will be stopped if not nothing</param>
    Private Sub StopRunning(listener As TcpListener)
        ClearUpdateResourceInfoTimer()
        listener?.Stop()
        RunState = ResourcePcRunState.Stopped
    End Sub

    ''' <summary>
    ''' Event fired when a verbose information message is to be distributed to
    ''' interested parties
    ''' </summary>
    ''' <param name="message">The verbose message information to disseminate</param>
    Public Event Verbose(ByVal message As String) Implements IListener.Verbose

    Private Sub RaiseVerbose(ByVal message As String)
        Log.Debug(message)
        RaiseEvent Verbose(message)
    End Sub
    Private Sub RaiseVerbose(ByVal msg As String, ByVal ParamArray args() As Object)
        Log.Debug(msg, args)
        RaiseVerbose(String.Format(msg, args))
    End Sub

    ''' <summary>
    ''' Event raised to provide information on the status of the listener. This
    ''' information could be logged, for example. The Resource PC Form outputs this
    ''' information to the status window.
    ''' </summary>
    ''' <param name="message">The information message</param>
    Public Event Info(ByVal message As String) Implements IListener.Info

    Public Sub RaiseInfo(ByVal message As String) Implements ISessionNotifier.RaiseInfo, IListener.RaiseInfo
        Log.Info(message)
        RaiseEvent Info(message)
    End Sub
    Public Sub RaiseInfo(ByVal msg As String, ByVal ParamArray args() As Object) Implements ISessionNotifier.RaiseInfo, IListener.RaiseInfo
        Log.Info(msg, args)
        RaiseInfo(String.Format(msg, args))
    End Sub

    ''' <summary>
    ''' Event raised to provide a warning on the status of the listener. This
    ''' warning could be logged, for example. The Resource PC Form outputs this
    ''' information to the status window.
    ''' </summary>
    ''' <param name="message">The warning message</param>
    Public Event Warn(ByVal message As String) Implements IListener.Warn

    Private Sub RaiseWarn(ByVal message As String) Implements ISessionNotifier.RaiseWarn
        Log.Warn(message)
        RaiseEvent Warn(message)
    End Sub
    Private Sub RaiseWarn(ByVal msg As String, ByVal ParamArray args() As Object) Implements ISessionNotifier.RaiseWarn
        Log.Warn(msg, args)
        RaiseWarn(String.Format(msg, args))
    End Sub

    ''' <summary>
    ''' Event raised to provide a error message on the status of the listener. This
    ''' error could be logged, for example. The Resource PC Form outputs this
    ''' information to the status window.
    '''
    ''' There is also a RaiseError that can be called by a child class (e.g. a
    ''' command processor) to raise the event.
    ''' </summary>
    ''' <param name="message">The error message</param>
    Public Event Err(ByVal message As String) Implements IListener.Err

    Public Sub RaiseError(ByVal message As String) Implements ISessionNotifier.RaiseError, IListener.RaiseError
        Log.Error(message)
        RaiseEvent Err(message)
    End Sub
    Public Sub RaiseError(ByVal msg As String, ByVal ParamArray args() As Object) Implements ISessionNotifier.RaiseError, IListener.RaiseError
        Log.Error(msg, args)
        RaiseError(String.Format(msg, args))
    End Sub


    ''' <summary>
    ''' Event raised when the Listener wants the whole Resource PC to be shut down.
    ''' </summary>
    Public Event ShutdownResource() Implements IListener.ShutdownResource

    Private Sub RaiseShutdownResource()
        RaiseEvent ShutdownResource()
    End Sub

    ''' <summary>
    ''' True when one-time initialisation has been done.
    ''' </summary>
    Private mInited As Boolean = False

    Public Property RunState As ResourcePcRunState Implements IListener.RunState

    Private mtListener As Thread
    Private mtArchiver As Thread = Nothing


    ''' <summary>
    ''' True if this Resource is the Auto Archiver. This can change at any time,
    ''' when a PoolUpdate is done.
    ''' </summary>
    Private mIsAutoArchiver As Boolean = False

    ''' <summary>
    ''' The Resource ID of this Resource PC, as registered in the database. This is
    ''' set when the listener has started and registration has been done.
    ''' </summary>
    Private mResourceID As Guid

    ''' <summary>
    ''' The name of this Resource PC, as registered in the database.
    ''' </summary>
    Public Property ResourceName As String Implements IListener.ResourceName

    ''' <summary>
    ''' The Pool ID that this Resource PC is a member of, or Guid.Empty if none.
    ''' </summary>
    Public Property PoolId As Guid Implements IListener.PoolId

    ''' <summary>
    ''' If this Resource PC is a member of a Pool, this contains the Resource ID of
    ''' the controller for that pool.
    ''' </summary>
    Private mPoolControllerID As Guid

    ''' <summary>
    ''' True if the Resource this listener is acting for is the controller of a
    ''' Resource Pool. False in any other circumstances.
    ''' </summary>
    Public ReadOnly Property IsController As Boolean Implements IListener.IsController
        Get
            If PoolId.Equals(Guid.Empty) Then Return False
            Return mPoolControllerID.Equals(mResourceID)
        End Get
    End Property

    ''' <summary>
    ''' True if the resource this listener is acting for is a Login Agent Resource.
    ''' False in all other circumstances.
    ''' </summary>
    Public ReadOnly Property IsLoginAgent() As Boolean Implements IListener.IsLoginAgent
        Get
            Return ResourceOptions.IsLoginAgent
        End Get
    End Property

    ''' <summary>
    ''' A clsResourceConnectionManager for managing connections to other resource
    ''' PCs. This is used when this Resource PC is the controller of a pool.
    ''' </summary>
    Public ReadOnly Property ResourceConnections As IResourceConnectionManager Implements IListener.ResourceConnections
        Get
            Return mResourceConnections
        End Get
    End Property

    Private WithEvents mResourceConnections As IResourceConnectionManager

    ''' <summary>
    ''' Collection of RunnerRecord objects, one for each session on this Resource
    ''' PC. Valid only during the lifetime of the ListenThread. The key is always the
    ''' associated session ID, in Guid.ToString format
    ''' </summary>
    Public Property Runners As RunnerRecordList Implements IListener.Runners

    ''' <summary>
    ''' A list of notification strings that should go to all connected clients at
    ''' the next convenient moment. Use the AddNotification method to add new
    ''' entries to this list.
    ''' </summary>
    Private mNotifications As New Queue(Of String)
    Private mNotificationsLockObject As Object = New Object

    ''' <summary>
    ''' Current availaibility of this resource PC. Can be one of
    ''' "Background", "Foreground", "Exclusive", "None".
    '''
    ''' The value is a description of the type of process/business object that
    ''' is ALLOWED to be run - not a description of the type of process(es)
    ''' that are already running.
    ''' </summary>
    Public Property Availability As BusinessObjectRunMode Implements IListener.Availability

    ''' <summary>
    ''' The name of an Automate user who has exclusive access to this
    ''' Resource PC, or an empty string for open access.
    ''' </summary>
    Public Property UserName As String Implements IListener.UserName

    ''' <summary>
    ''' As for msUserName, but in Guid form.
    ''' </summary>
    Public Property UserId As Guid = Guid.Empty Implements IListener.UserId

    ''' <summary>
    ''' The SSL certificate we're using - retrieved via the given hash.
    ''' </summary>
    Private mSSLCert As X509Certificate

    ''' <summary>
    ''' The content of our 'action page'
    ''' </summary>
    Private mActionPage As String

    ''' <summary>
    ''' Dictionary of client connections - each is a ListenerClient representing the
    ''' connection and current state.
    ''' </summary>
    Public Property Clients As New List(Of ListenerClient) Implements IListener.Clients

    Private Property NewClients As New ConcurrentBag(Of ListenerClient)

    ''' <summary>
    ''' The options used to start the runtime resource owning this listener
    ''' </summary>
    Public Property ResourceOptions As Resources.ResourcePCStartUpOptions Implements IListener.ResourceOptions

    Private ReadOnly mAscrOn As Boolean
    Private ReadOnly mEnableRunModeCache As Boolean

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    Public Sub New()
        RunState = ResourcePcRunState.Stopped

        Dim memberPermissionsFactory = Function(groupPermissions As IGroupPermissions) New MemberPermissions(groupPermissions)

        mCommandFactoryFactory = Function(client) New CommandFactory(client, Me, gSv, memberPermissionsFactory)

        mPingTimeoutSeconds = gSv.GetPref(PreferenceNames.ResourceConnection.PingTimeOutSeconds, 30)

        mAscrOn = gSv.GetPref(SystemSettings.UseAppServerConnections, False)
        mEnableRunModeCache = gSv.GetPref(ResourceConnection.ListenerEnableRunModeCache, False)
    End Sub

    ''' <summary>
    ''' The resource ID for the resource that this listener is operating on behalf
    ''' of.
    ''' </summary>
    Public ReadOnly Property ResourceId() As Guid Implements IListener.ResourceId
        Get
            Return mResourceID
        End Get
    End Property

    ''' <summary>
    ''' One-time initialisation - this stuff is done once only, even if the listener
    ''' is restarted multiple times.
    ''' </summary>
    ''' <param name="sErr">Contains an error description on failure</param>
    ''' <returns>True if successful, False otherwise</returns>
    Private Function Init(ByRef sErr As String) As Boolean

        Runners = New RunnerRecordList()
        Availability = BusinessObjectRunMode.Exclusive

        Return True

    End Function

    ''' <summary>
    ''' Attempts to find the runner record dealing with a particular session ID.
    ''' </summary>
    ''' <param name="sessId">The ID of the session for which the runner record is
    ''' required.</param>
    ''' <returns>The runner record associated with the given session ID or null if
    ''' no such record was found in the runner record list maintained by this
    ''' listener instance.</returns>
    Public Function FindRunner(ByVal sessId As Guid) As ListenerRunnerRecord Implements IListener.FindRunner
        Return Runners.FindRunner(sessId)
    End Function

    ''' <summary>
    ''' Start the listener.
    ''' </summary>
    ''' <param name="startupOptions">The options used to start this resource</param>
    ''' <param name="sErr">Contains an error description on failure</param>
    ''' <returns>True if successful, False otherwise</returns>
    Public Function Startup(startupOptions As Resources.ResourcePCStartUpOptions, ByRef sErr As String) As Boolean Implements IListener.Startup
        Try

            'Make sure we have a server connection...
            If Not ServerFactory.CheckServerAvailability() Then
                sErr = "Server unavailable"
                Return False
            End If

            'Check for calling at inappropriate times...
            If Not RunState = ResourcePcRunState.Stopped Then
                sErr = "Attempt to start listener when it was already running"
                Return False
            End If

            'Make sure the connection to SQL server is active
            gSv.EnsureDatabaseConnection()

            'Get single sign-on setting
            ResourceOptions = startupOptions

            'Do one-time initialisation if necessary.
            If Not mInited Then
                If Not Init(sErr) Then Return False
                mInited = True
            End If

            RunState = ResourcePcRunState.Running

            UserName = ResourceOptions.Username
            ' an anonymous resource will have a blank username...
            If UserName = "" Then UserId = Guid.Empty _
            Else UserId = gSv.TryGetUserID(UserName)

            'Get our 'action page' from the embedded resource, for ease of access
            'later. NOTE: this is potentially obsolete as the HTTP Post request required
            'to retrieve the Action page is not generated from anywhere within the product.
            Dim asm As Assembly = Assembly.GetExecutingAssembly()
            Using strm As Stream = asm.GetManifestResourceStream(
             "BluePrism.AutomateAppCore.ResourcePCActionPage.html")
                If strm IsNot Nothing Then
                    Using reader As StreamReader = New StreamReader(strm)
                        Dim s As String = reader.ReadToEnd()
                        'Replace references to our port now, since they will never change again.
                        'Other similar stuff is replaced as and when it is used.
                        s = s.Replace("$$AUTOMATE-port-$$", ResourceOptions.Port.ToString())
                        mActionPage = s
                    End Using
                Else
                    mActionPage = ""
                End If
            End Using

            mtListener = New Thread(New ThreadStart(AddressOf ListenThread))
            mtListener.Start()
            Return True
        Catch e As Exception
            sErr = "Exception while starting listener: " & e.Message
            Return False
        End Try
    End Function


    ''' <summary>
    ''' Starts the process of shutting down the ResourcePC when a shutdown command has been received
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub InitiateShutdown(reason As String, waitForSessions As Boolean) Implements IListener.InitiateShutdown
        AddNotification("SHUTTING DOWN")
        RaiseInfo(reason)

        If (waitForSessions) Then
            RunState = ResourcePcRunState.WaitingToStop
            Dim waitForSessionsThread = New Thread(AddressOf ShutdownWhenSessionsComplete)
            waitForSessionsThread.Start()
        Else
            RaiseShutdownResource()
        End If

    End Sub

    ''' <summary>
    ''' Waits for all running sessions to complete, then initiates shutdown by raising
    ''' the ShutdownResource event - run on separate thread from InitiateShutdown
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ShutdownWhenSessionsComplete()

        Dim waitTime = TimeSpan.FromSeconds(45)
        Dim reportIntervalTime = TimeSpan.FromSeconds(5)
        Dim retryTime = TimeSpan.FromSeconds(1)
        Dim nextReportTime = TimeSpan.Zero

        Dim watch = Stopwatch.StartNew()

        While Runners.RunningCount > 0

            If (watch.Elapsed > nextReportTime) Then
                RaiseInfo("Waiting for sessions to finish before shutting down. Running: {0}", Runners.RunningCount)
                nextReportTime = watch.Elapsed.Add(reportIntervalTime)
            End If

            Thread.Sleep(retryTime)

            If (watch.Elapsed > waitTime) Then
                RaiseInfo("Sessions did not complete within allowed time")
                Exit While
            End If

        End While

        RaiseShutdownResource()
    End Sub

    ''' <summary>
    ''' Stop the listener. This method blocks until the listener
    ''' is actually stopped, which could take some time.
    '''
    ''' <remarks>Causes all running sessions to be stopped.</remarks>
    ''' </summary>
    Public Function Shutdown(ByRef sErr As String) As Boolean Implements IListener.Shutdown

        'Unregister ourself (unless the server is not available, in which case we can't - the
        'server connection broke! and we don't want to wait 2 minutes just in case it comes back either)
        If ServerFactory.ServerAvailable AndAlso Not ServerFactory.ServerManager.ServerPending Then
            Dim name = GetName(ResourceOptions.Port)
            If IsLoginAgent Then
                gSv.DeregisterLoginAgent(name)
            Else
                gSv.DeregisterResourcePC(name)
            End If
        End If

        'Check for calling at inappropriate times...
        If RunState = ResourcePcRunState.Stopping Then
            sErr = "Attempt to Shutdown listener when already stopping"
            Return False
        End If
        If RunState = ResourcePcRunState.Stopped Then
            sErr = "Attempt to Shutdown listener when already stopped"
            Return False
        End If

        ' The code running under the listener thread loops while in running state - changing state
        ' will cause the thread to end
        RunState = ResourcePcRunState.Stopping
        mtListener.Join()
        Debug.Assert(RunState = ResourcePcRunState.Stopped)
        'Wait for the archiver to stop as well.
        If mtArchiver IsNot Nothing Then
            mtArchiver.Join()
            mtArchiver = Nothing
        End If

        If mResourceConnections IsNot Nothing Then
            mResourceConnections.Dispose()
            mResourceConnections = Nothing
        End If

        'end all running sessions
        Return EndRunningSessions("Resource shut down. Running processes must be stopped as a result.", sErr)
    End Function

    ''' <summary>
    ''' Sends as signal to all running sessions requesting that they exit cleanly
    ''' at the first opportunity.
    '''
    ''' An entry will be entered in the log of each session indicating that the
    ''' session has been ended by the currently logged in user on the local
    ''' machine, together with the reason that the process engine has been terminated.
    '''
    ''' </summary>
    ''' <param name="reason">A reason why the processes are being ended. This
    ''' reason will be added to the log of each session ended.</param>
    ''' <param name="sErr">A string to carry an error message.</param>
    ''' <returns>unless an exception occurs; false otherwise.</returns>
    Private Function EndRunningSessions(
     ByVal reason As String, ByRef sErr As String) As Boolean

        If Runners Is Nothing Then sErr = "Runners not initialised" : Return False

        Try
            Runners.StopRunners(reason)
            Return True

        Catch ex As Exception
            sErr = ex.Message
            Return False

        End Try
    End Function


    ''' <summary>
    ''' A queue of session status settings to be retried.
    ''' </summary>
    Private Class SessionStatusRetryItem
        Public sessionID As Guid
        Public status As RunnerStatus
        Public retryDelay As TimeSpan   'Time to next try
        Public Property SessionExceptionDetail As SessionExceptionDetail
        Public Sub New(id As Guid, runnerStatus As RunnerStatus, exceptionDetail As SessionExceptionDetail)
            sessionID = id
            status = runnerStatus
            retryDelay = TimeSpan.FromSeconds(5)
            SessionExceptionDetail = exceptionDetail
        End Sub
    End Class

    ' The list of session status retries to be processed
    Private mSessionStatusRetries As New List(Of SessionStatusRetryItem)

    ' The date/time that the session status retries were last checked.
    Private mLastCheckedSessionStatus As DateTime = DateTime.UtcNow

    ''' <summary>
    ''' Called by a Runner when it fails to set the status of a session in the
    ''' database. This allows the problem to be logged, and the setting to be
    ''' retried.
    ''' </summary>
    ''' <param name="rr">The RunnerRecord related to the failure. The status in
    ''' this record should be the status that failed to be set.</param>
    ''' <param name="errmsg">The associated error message.</param>
    Private Sub HandleSessionStatusFailure(rr As RunnerRecord, errmsg As String) Implements ISessionNotifier.HandleSessionStatusFailure

        Dim sessionId = DirectCast(rr, ListenerRunnerRecord).SessionID
        RaiseEvent Err(String.Format("Failed to set session status on {0} to {1} : {2}",
         sessionId.ToString(), rr.Status.ToString(), errmsg))
        SyncLock mSessionStatusRetries
            mSessionStatusRetries.Add(New SessionStatusRetryItem(sessionId, rr.Status, rr.SessionExceptionDetail))
        End SyncLock
    End Sub

    ''' <summary>
    ''' Call periodically to process retries of session status setting.
    ''' </summary>
    Private Sub ProcessSessionStatusRetries()
        Dim elapsed As TimeSpan = Date.UtcNow - mLastCheckedSessionStatus
        mLastCheckedSessionStatus = Date.UtcNow
        SyncLock mSessionStatusRetries
            Dim toremove As New List(Of SessionStatusRetryItem)
            For Each s As SessionStatusRetryItem In mSessionStatusRetries

                ' Remove the elapsed time from the item's time to wait
                s.retryDelay -= elapsed

                ' If we still have time to wait, skip this item
                If s.retryDelay > TimeSpan.Zero Then Continue For
                Try
                    Select Case s.status
                        Case RunnerStatus.FAILED
                            gSv.SetSessionTerminated(s.sessionID,
                                                     DateTimeOffset.Now,
                                                     s.SessionExceptionDetail)
                        Case RunnerStatus.COMPLETED
                            gSv.SetSessionCompleted(s.sessionID, DateTimeOffset.Now)
                        Case RunnerStatus.STOPPED
                            gSv.SetSessionStopped(s.sessionID, DateTimeOffset.Now)
                        Case Else
                            RaiseError(
                             "Unable to retry session status setting of {0}", s.status)
                    End Select
                    toremove.Add(s)
                Catch
                    'Dont remove on error
                End Try
            Next
            For Each s As SessionStatusRetryItem In toremove
                mSessionStatusRetries.Remove(s)
            Next
        End SyncLock
    End Sub


    ''' <summary>
    ''' Get the number of current active connections to this Listener.
    ''' These include (a) connections from running instances of
    ''' Control Room, and (b) connections from Automate Web Service.
    ''' </summary>
    ''' <returns>The number of active connections</returns>
    Public Function GetActiveConnections() As Integer Implements IListener.GetActiveConnections
        Return Clients.Count
    End Function

    ''' <summary>
    ''' Get the number of processes active on this Listener.
    ''' </summary>
    ''' <returns>The number of running processes</returns>
    Public Function GetActiveSessionCount() As Integer Implements IListener.GetActiveSessionCount
        If Runners Is Nothing Then Return 0
        Return Runners.ActiveCount
    End Function


    ''' <summary>
    ''' Get the number of processes pending on this Listener.
    ''' </summary>
    ''' <returns>The number of pending processes</returns>
    Public Function GetPendingSessionCount() As Integer Implements IListener.GetPendingSessionCount
        If Runners Is Nothing Then Return 0
        Return Runners.PendingCount
    End Function


    ''' <summary>
    ''' Handles notifications of Resource status changes from our connection manager.
    ''' When these occur, it means something has changed status-wise on one of our
    ''' pool members, so we need to pass that notification along.
    ''' </summary>
    Private Sub ResourceStatusChanged(
     sender As Object, ByVal e As ResourcesChangedEventArgs) _
     Handles mResourceConnections.ResourceStatusChanged

        'Only do this if we're the controller - if we're just a pool member
        'we don't want to pass notifications from the controller back to it
        'otherwise we set up a loop!
        If IsController() Then
            AddNotification("POOL MEMBER CHANGED")
        End If

    End Sub


    ''' <summary>
    ''' Update the connection manager (clsResourceConnectionManager) we're currently
    ''' using - i.e. if we need one but don't have one, create one, or if we have one
    ''' but it's not required, get rid of it.
    ''' </summary>
    Private Sub UpdateConnectionManager(oldPoolId As Guid)

        If PoolId.Equals(Guid.Empty) Then
            'If we're not in a pool, we don't need one at all.
            If mResourceConnections IsNot Nothing Then
                mResourceConnections.Dispose()
                mResourceConnections = Nothing
            End If
            Return
        End If

        Dim mode As IResourceConnectionManager.Modes
        If mPoolControllerID.Equals(mResourceID) Then
            mode = IResourceConnectionManager.Modes.Controller
        Else
            mode = IResourceConnectionManager.Modes.PoolMember
        End If

        If mResourceConnections IsNot Nothing Then
            If mode = mResourceConnections.Mode AndAlso PoolId = oldPoolId Then
                Return
            End If
            mResourceConnections.Dispose()
        End If
        mResourceConnections = New PersistentConnectionManager(mode, Nothing, PoolId, mPoolControllerID)
    End Sub


    ''' <summary>
    ''' Update diagnostics settings if necessary. The value passed in will have been
    ''' retrieved from the database. For performance reasons, its retrieval is a
    ''' side-effect of other necessary periodic functionality.
    ''' </summary>
    Private Sub UpdateDiags(ByVal diags As Integer)
        If diags <> clsAPC.Diagnostics Then
            clsAPC.Diagnostics = CType(diags, clsAPC.Diags)
            RaiseEvent Info("Diagnostics settings changed to " & clsAPC.GetDiagnosticsString())
        End If
    End Sub


    ''' <summary>
    ''' The archiving thread.
    ''' </summary>
    Private Sub ArchiveThread()
        Try

            Dim last As DateTime = gSv.GetArchivingLastComplete()
            If DateTime.Now - last < New TimeSpan(1, 0, 0) Then Return

            'Get the settings...
            Dim age As String = Nothing
            Dim delete As Boolean
            Dim resource As Guid
            Dim folder As String = Nothing
            gSv.GetAutoArchivingSettings(resource, folder, age, delete)

            'Check the resource, just in case, although we shouldn't have got here
            'if it's not us. (It could happen, very rarely, in a short period of
            'minutes after it's changed in System Manager.)
            If mResourceID <> resource Then
                RaiseEvent Info("Started to archive, but discovered that this machine is no longer the archiver")
                Return
            End If

            ' Check for orphaned sessions first
            Dim orphans As ICollection(Of Integer) = gSv.GetOrphanedSessionNumbers()
            If orphans.Count > 0 Then
                RaiseInfo("Found {0} orphaned sessions. Deleting...", orphans.Count)
                gSv.ArchiveSessions(orphans)
            End If

            'Calculate the 'to date', which is the date and time before which session
            'end times must be if they are to be included in this archive...
            Dim toDate As DateTime
            Dim i As Integer
            If Not Integer.TryParse(age.Substring(0, age.Length - 1), i) Then
                RaiseEvent Err("Error: invalid archiving age specified")
                Return
            End If
            Dim now As Date = DateTime.UtcNow()
            Select Case age.Substring(age.Length - 1, 1)
                Case "y"
                    toDate = DateAdd(DateInterval.Year, -i, now)
                Case "m"
                    toDate = DateAdd(DateInterval.Month, -i, now)
                Case "w"
                    toDate = DateAdd(DateInterval.Day, -i * 7, now)
                Case "d"
                    toDate = DateAdd(DateInterval.Day, -i, now)
                Case Else
                    RaiseEvent Err("Error: Invalid archiving age type specified")
                    Return
            End Select

            'Do the archiving...
            Dim arch As New clsArchiver(folder)
            Dim sErr As String = Nothing
            If Not arch.CreateArchive(DateTime.MinValue, toDate, delete, Nothing, False, sErr) Then
                RaiseEvent Err("Archiving failed - " & sErr)
                Return
            End If

            gSv.SetArchivingLastComplete(DateTime.Now)
            RaiseEvent Info("Archiving completed")

        Catch ex As Exception
            RaiseEvent Err("Exception during archiving : " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Generate the response to a request for WSDL.
    ''' </summary>
    ''' <param name="response">A response object to be populated.</param>
    ''' <param name="serviceName">The process/service name that was requested.</param>
    Private Sub GenerateWSDL(ByRef response As ListenerResponse, ByVal serviceName As String)

        Dim sErr As String = Nothing

        ' Set default http response properties
        response.HTTP = True
        response.HTTPContentType = "text/plain"
        response.HTTPCharset = "utf-8"

        Dim pt As DiagramType
        Dim procid As Guid = GetWSProcess(serviceName, response, pt)
        If procid.Equals(Guid.Empty) Then
            response.Text = "No such process"
            response.HTTPStatus = HttpStatusCode.BadRequest
            Return
        End If

        'We've identified a WSDL request for a valid process
        'so generate the WSDL and send it back to the client...
        Dim procxml As String = Nothing
        Dim lastmod As DateTime

        Try
            If Not clsAPC.ProcessLoader.GetProcessXML(procid, procxml, lastmod, sErr) Then
                response.Text = "Failed to retrieve process definition"
                response.HTTPStatus = HttpStatusCode.InternalServerError
                Return
            End If
        Catch e As PermissionException
            response.Text = e.Message
            response.HTTPStatus = HttpStatusCode.Unauthorized
            Return
        End Try

        Dim wsdlproc As clsProcess = Nothing
        Try
            wsdlproc = clsProcess.FromXML(clsGroupObjectDetails.Empty, procxml, False, sErr)
            If wsdlproc Is Nothing Then
                'We couldn't create the process object...
                response.Text = "Failed to create process object"
                response.HTTPStatus = HttpStatusCode.InternalServerError
                Return
            End If

            Dim webServiceDetails = gSv.GetProcessWSDetails(procid)

            response.Text = clsProcessWSDL.Generate(serviceName, wsdlproc, WebServiceBaseURL(), webServiceDetails.IsDocumentLiteral())
        Catch ex As Exception
            response.Text = "Failed to generate WSDL - " & ex.Message
            response.HTTPStatus = HttpStatusCode.InternalServerError
            Return
        Finally
            If wsdlproc IsNot Nothing Then
                wsdlproc.Dispose()
            End If
        End Try

        response.HTTPContentType = "text/xml"
    End Sub

    ''' <summary>
    ''' Generate the Web Services index page.
    ''' </summary>
    ''' <param name="response">A response object to be populated.</param>
    Private Sub GenerateWSIndex(ByRef response As ListenerResponse)

        response.HTTP = True
        response.HTTPContentType = "text/html"
        response.HTTPCharset = "utf-8"

        'The Strings to top, tail and insert headers into the html page text with the correct tags. here for edit ease...
        Dim htmlPageTop As String = "<html><head><title>Blue Prism Web Services</title>" & vbCrLf & "<style>body{font-family:Arial,Helvetica,sans-serif} h2 {color:rgb(0,61,204);}</style></head><body>"
        Dim headerWebProcess As String = "<h2>Web service - Processes</h2>"
        Dim headerWebObjects As String = "<br><h2>Web service - Business objects</h2>"
        Dim htmlPageBottom As String = "</body></html>"

        Dim wsindex As New StringBuilder()
        wsindex.Append(htmlPageTop & headerWebProcess)

        Try
            Dim dtProcesses As DataTable

            'Populate the data table with processes using the function search return criteria want:- published, dont want:- retired
            dtProcesses = gSv.GetProcesses(ProcessAttributes.PublishedWS, ProcessAttributes.Retired)
            wsindex.Append(MakeWSDLHyperlinks(dtProcesses))

            wsindex.Append(headerWebObjects)

            'Populate the data table with business objects(true) with the same search criteria as above
            dtProcesses = gSv.GetProcesses(ProcessAttributes.PublishedWS, ProcessAttributes.Retired, True)
            wsindex.Append(MakeWSDLHyperlinks(dtProcesses))

        Catch ex As Exception ' append the exception to the output so far (review this!)
            wsindex.Append("<p>Failed to complete index - " & ex.Message & "</p>")
        End Try

        wsindex.Append(htmlPageBottom)
        response.Text = wsindex.ToString()

    End Sub


    ''' <summary>
    ''' Creates formatted hyperlinks to the WSDL for a set of processes.
    ''' </summary>
    ''' <param name="processes">A DataTable from the BPAProcess table, already
    ''' filtered to just those that are published as Web Services.</param>
    ''' <returns>An HTML fragment with a paragraph per row, linking to the WSDL
    ''' for each process.</returns>
    Private Function MakeWSDLHyperlinks(ByVal processes As DataTable) As String

        Dim response As New StringBuilder()
        If processes.Rows.Count = 0 Then
            response.Append("<p>None</p>")
        Else
            Using reader As DataTableReader = processes.CreateDataReader()
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    Dim name As String = prov.GetString("wspublishname")
                    Dim description As String = prov.GetString("Description")
                    Dim wsdlUrl As String = WebServiceBaseURL() & name & "?wsdl"

                    response.Append("<p><b>" & name & "</b> - " & description & "<br><a href=""" & wsdlUrl & """ >" & wsdlUrl & "</a></p>")
                End While
            End Using

        End If
        Return response.ToString()

    End Function

    ''' <summary>
    ''' Returns the base URL for the web services hosted by this resource. If a
    ''' prefix has been specified on start-up then this overrides the device name
    ''' </summary>
    ''' <returns>The base URL</returns>
    Private Function WebServiceBaseURL() As String
        If String.IsNullOrEmpty(ResourceOptions.WebServiceAddressPrefix) Then
            ' Construct the URL using this resource's details
            Dim hostname = ""
            Dim port As Integer
            Dim ssl As Boolean
            Dim requiresSecure As Boolean
            gSv.GetResourceAddress(ResourceName, hostname, port, ssl, requiresSecure)
            Return String.Format("{0}://{1}{2}/ws/",
                If(ssl, "https", "http"), hostname, If(port = If(ssl, 443, 80), "", ":" & port))
        Else
            ' Override with the specified prefix
            Return String.Format("{0}/ws/", ResourceOptions.WebServiceAddressPrefix)
        End If
    End Function



    ''' <summary>
    ''' Handle a Web Service POST request, either setting the relevant process (or
    ''' part of) running, or raising a SOAP fault.
    '''
    ''' If the request is for a Process, a new session is always started.
    '''
    ''' For a Business Object, a new session is started for the Initialize call.
    ''' Otherwise the existing instance, as specified by the session parameter in
    ''' the POST data, is used.
    '''
    ''' If the request is succesful, a Process starts running on behalf of the
    ''' caller (on a different thread), as referenced by the returned "runner
    ''' record", which can be used to keep track of its eventual completion or
    ''' failure.
    ''' </summary>
    ''' <param name="response">A response object, in which a response should be
    ''' set if and ONLY if, no runner record is created. Normally the response
    ''' will be a SOAP fault.</param>
    ''' <param name="procid">The ID of the process being requested.</param>
    ''' <param name="procname">The name of the process being requested.</param>
    ''' <param name="proctype">The type of the process being requested.</param>
    ''' <param name="userid">The ID of the user making the request.</param>
    ''' <param name="postdata">The actual HTTP post data.</param>
    ''' <returns>The new RunnerRecord for the client, or Nothing if one wasn't
    ''' created. Note that it is possible for there to be both no runner returned
    ''' AND no response - in this case the request is queued for action on an
    ''' auto-instance.</returns>
    Private Function HandleWSRequest(
       response As ListenerResponse,
       procid As Guid, ByVal procname As String,
       proctype As DiagramType, ByVal userid As Guid,
       postdata As String, token As clsAuthToken) As ListenerRunnerRecord

        Dim sErr As String = Nothing
        Dim newRunner As ListenerRunnerRecord = Nothing

        'We need the process in order to match the parameters (specifically, to be
        'able to translate the parameter names from web-service-safe format to our
        'own more liberal ones.
        'TODO: There is a perfomance and synchronisation issue here:
        ' a) performance - it will get this process again later (or will already
        '    have it, for a business object that's already initialised)
        ' b) synchronisation - for a business object that's already initialised,
        '    this will be looking at the database version, not the running version,
        '    which could be different if someone saves an updated version in between
        'If we're dealing with a business object, and it's already initialised, then
        'we already have an instance of a process. In all other cases, we're obviously
        'going to create one shortly. So in theory at least, it's possible to avoid
        'creating another here. In theory.
        Dim procxml As String = Nothing
        Dim lastmod As DateTime
        If Not clsAPC.ProcessLoader.GetProcessXML(procid, procxml, lastmod, sErr) Then
            Throw New BluePrismException("Process could not be loaded - {0}", sErr)
        End If
        Dim proc As clsProcess = clsProcess.FromXML(Options.Instance.GetExternalObjectsInfo(), procxml, False, sErr)
        If proc Is Nothing Then
            Throw New BluePrismException("Error in process - {0}", sErr)
        End If

        Dim method As String = Nothing
        Dim session As String = Nothing
        Dim inputs As clsArgumentList = clsProcessWSDL.ProcessSOAPInputs(postdata, proc, method, session, sErr)
        proc.Dispose()
        If inputs Is Nothing Then
            Throw New BluePrismException("Couldn't process soap inputs - {0}", sErr)
        End If

        'Handle the case of running a Process, or running the Initialise method on
        'a Business Object - both result in the creation of a new session.
        If proctype = DiagramType.Process OrElse method = "Initialise" Then

            'Make sure a session wasn't specified...
            If session IsNot Nothing Then
                Throw New BluePrismException("Not valid to specify a session in this context")
            End If

            'For a process, the only supported method has
            'the same name as the process, so confirm that
            'is what was requested...
            If proctype = DiagramType.Process AndAlso method <> procname Then
                Throw New BluePrismException("Method {0} is not supported. The only valid method for this process is {1}", method, procname)
            End If

            'Start the process running...
            Dim sessionid = Guid.NewGuid
            Dim runnerRequest = New RunnerRequest() With {
                        .SessionId = sessionid,
                        .ProcessId = procid,
                        .StarterUserId = userid,
                        .StarterResourceId = mResourceID,
                        .RunningResourceId = mResourceID,
                        .QueueIdent = 0,
                        .AutoInstance = False,
                        .AuthorisationToken = token}
            Try
                newRunner = CreateRunner(runnerRequest)
            Catch ex As Exception
                Throw New BluePrismException("Couldn't create session - {0}", ex.Message)
            End Try
            If proctype = DiagramType.Process Then
                'Only need to do this for a Process. A Business
                'Object will already be automatically running
                'the Initialise page!
                Availability = Runners.Availability
                newRunner.mStartupParams = inputs
                newRunner.StartThread()
            End If
            Return newRunner

        End If

        'We're running a Business Object, and not the Initialize method, so we
        'use an existing session.
        If session Is Nothing Then
            Throw New BluePrismException("A session must be specified for this method")
        End If

        'Handle a request for an automatic business object instance.
        If session = "auto" Then

            'You can't call CleanUp on auto-sessions. Likewise for Initialise, but
            'that's already covered above.
            If method = "CleanUp" Then
                Throw New BluePrismException("Not valid to call CleanUp on an auto session")
            End If

            ' Find the existing auto-instance for this process, or, if here is no
            ' existing auto-instance, so create one. The act of creating it will set
            ' it running the initialise page, so we won't be able to use it
            ' immediately, but it will come to be usable via the normal queuing
            ' mechanism.
            Try
                newRunner = Runners.FindOrCreateAuto(procid, Me, userid, mResourceID, token)
            Catch ex As Exception
                Throw New BluePrismException("Failed to create auto-instance - {0}", ex.Message)
            End Try


            Try
                Dim requiredRunMode = GetRequiredRunMode(procid)

                If newRunner.Status <> RunnerStatus.IDLE Or Not IsAvailable(requiredRunMode) Then
                    'We can't use it yet, it's busy. We're effectively queued.
                    Return Nothing
                End If
            Catch ex As Exception
                Throw New BluePrismException("Failed to establish availability - {0}", ex.Message)
            End Try

            newRunner.SetUser(userid)

            'Fire it up - similar to what is done below for a normal call.
            newRunner.Status = RunnerStatus.PENDING
            Availability = Runners.Availability
            newRunner.mStartupParams = inputs
            newRunner.mAction = method
            newRunner.StartThread()
            Return newRunner

        End If

        'This is a normal request for an action on an existing Business Object
        'session. Find the appropriate runner record for it...
        Dim gSessionID As Guid
        Try
            gSessionID = New Guid(session)
        Catch e As Exception
            Throw New BluePrismException("Invalid session ID")
        End Try

        newRunner = Runners.FindRunner(gSessionID)
        If newRunner Is Nothing Then
            Throw New BluePrismException("Session not found")
        End If
        If newRunner.mAutoInstance Then
            'Unlikely to happen, but it shouldn't be allowed.
            Throw New BluePrismException("Invalid specific selection of auto-instance")
        End If

        'Make sure the session is ready to accept a new
        'request...
        If newRunner.Status <> RunnerStatus.IDLE Then
            Throw New BluePrismException("The session must be IDLE. It is currently {0}", newRunner.Status.ToString())
        End If

        newRunner.SetUser(userid)

        'Now need to set the status to pending temporarily, so the IDLE
        'status doesn't get confused with the one that happens when it
        'finishes. The status will almost immediately go to RUNNING when
        'the runner thread starts up.
        newRunner.Status = RunnerStatus.PENDING
        Availability = Runners.Availability
        newRunner.mStartupParams = inputs
        newRunner.mAction = method
        newRunner.StartThread()
        Return newRunner

    End Function


    ''' <summary>
    ''' Handle the running of a Web Service (complete Process, or Business Object
    ''' action) on behalf of a client. This is called repeatedly until the underlying
    ''' process finishes.
    ''' </summary>
    ''' <param name="response">A response object, in which a response should be
    ''' set if and ONLY if, running is complete (as signified by a return value of
    ''' True.</param>
    ''' <param name="runner"></param>
    ''' <param name="procid">The ID of the process being requested.</param>
    ''' <param name="procname">The name of the process being requested.</param>
    ''' <param name="proctype">The type of the process being requested.</param>
    ''' <returns>True if the running is now complete, False if it's still going. In
    ''' the latter case, we expect to be called again.</returns>
    Private Function HandleWSRunning(ByVal response As ListenerResponse,
     ByVal runner As ListenerRunnerRecord,
     ByVal procid As Guid, ByVal procname As String,
     ByVal proctype As DiagramType) As Boolean

        Dim sErr As String = Nothing

        'If it's still running, we don't need to do anything yet.
        If runner.Status = RunnerStatus.PENDING OrElse
         runner.Status = RunnerStatus.RUNNING Then
            Return False
        End If

        'Note that a Process ends at a COMPLETED state, a Business Object
        'action returns to IDLE after completing each action. Anything remaining must
        'be an error condition.
        If runner.Status <> RunnerStatus.COMPLETED AndAlso
         runner.Status <> RunnerStatus.IDLE Then
            If proctype = DiagramType.Object Then
                'If a Business Object errored during an action, it's still capable of
                'performing another, so it goes back to IDLE once we've captured the
                'error.
                runner.Status = RunnerStatus.IDLE
                runner.mReviewStatus = True
            End If
            response.SOAPFault("Process " & runner.Status.ToString() & " - " & runner.mFailReason)
            Return True
        End If

        Dim sSession As String = Nothing
        Dim sMethod As String
        If proctype = DiagramType.Object Then
            sMethod = runner.mAction
            If sMethod = "Initialise" Then
                sSession = runner.SessionID.ToString()
            End If
        Else
            sMethod = procname
        End If
        Try

            'TODO: We need the process, but there's a performance issue/
            'possible improvement if we can make it work. See the other
            'TODO nearby for more detail.
            Dim sProcessXML As String = Nothing
            Dim lastmod As DateTime
            If Not clsAPC.ProcessLoader.GetProcessXML(procid, sProcessXML, lastmod, sErr) Then
                response.SOAPFault("Process could not be loaded - " & sErr)
                Return True
            End If
            Dim proc As clsProcess = clsProcess.FromXML(Options.Instance.GetExternalObjectsInfo(), sProcessXML, False, sErr)
            If proc Is Nothing Then
                response.SOAPFault("Error in process - " & sErr)
                Return True
            End If

            Dim webServiceDetails = gSv.GetProcessWSDetails(procid)

            response.HTTP = True
            response.HTTPContentType = "text/xml"
            response.HTTPCharset = "utf-8"
            response.Text = clsProcessWSDL.OutputsToSOAP(runner.mOutputs, proc, sMethod, sSession, webServiceDetails.IsDocumentLiteral(), webServiceDetails.UseLegacyNamespaceStructure())
            If (clsAPC.Diagnostics And clsAPC.Diags.LogWebServices) <> 0 Then
                RaiseEvent Info("Sent SOAP response:" & response.Text)
            End If
        Catch ex As Exception
            response.SOAPFault(ex.Message)
        End Try
        If proctype = DiagramType.Object Then
            If runner.mAction = "CleanUp" Then
                runner.Status = RunnerStatus.STOPPED
                runner.mReviewStatus = True
            Else
                runner.Status = RunnerStatus.IDLE
                runner.mReviewStatus = True
            End If
        End If
        Return True

    End Function

    ''' <summary>
    ''' Checks the database for any running sessions and checks their current status
    ''' in the runners held by this listener, resolving any on the database to match
    ''' the state held in the runners.
    ''' </summary>
    Private Sub ResolveRunningSessions()

        Using reader As IDataReader = gSv.GetRunningOrStoppingSessions(mResourceID).CreateDataReader()
            Dim prov As New ReaderDataProvider(reader)
            While reader.Read()

                ' Find the runner record with the running session ID
                Dim sessId As Guid = prov.GetValue("sessionid", Guid.Empty)
                Dim r As RunnerRecord = Runners.FindRunner(sessId)

                ' If it isn't in this listeners runner list, set it on the
                ' database as terminated
                Try
                    If r Is Nothing Then
                        gSv.SetSessionTerminated(sessId, DateTimeOffset.Now, SessionExceptionDetail.InternalError(
                                                 "Unexpected termination of session"))
                        RaiseInfo("Resolved abnormal termination " &
                         "of session {0} from previous run", sessId)

                    ElseIf r.IsActive Then
                        RaiseWarn(
                         "Session {0} still active from previous run", sessId)

                    Else
                        Select Case r.Status
                            Case RunnerStatus.COMPLETED
                                gSv.SetSessionCompleted(sessId, DateTimeOffset.Now)
                                RaiseInfo("Resolved belated completion " &
                                 "of session {0} from previous run", sessId)

                            Case RunnerStatus.STOPPED
                                gSv.SetSessionStopped(sessId, DateTimeOffset.Now)
                                RaiseInfo("Resolved stopping " &
                                 "of session {0} from previous run", sessId)

                            Case Else
                                gSv.SetSessionTerminated(sessId,
                                                         DateTimeOffset.Now,
                                                         r.SessionExceptionDetail)
                                RaiseWarn("Resolved session {0} from previous run; " &
                                 "Ending status of last run was: {1}", sessId, r.Status)

                        End Select

                    End If

                Catch ex As Exception
                    RaiseError(
                     "Error trying to resolve session {0} from previous run: {1} ",
                     sessId, ex.ToString())

                End Try

            End While

        End Using

    End Sub

    ''' <summary>
    ''' Attempt to find and shutdown a Login Agent resource on this machine.
    ''' </summary>
    Private Sub TryStopLoginAgent()
        Dim port As Integer = 0
        Try
            ' Get all resources registered on this machine
            Dim thisHostName As String = Split(ResourceName, ":")(0)
            Dim resourceList = gSv.GetResourcesForHost(ResourceAttribute.LoginAgent, ResourceAttribute.None, thisHostName)
            If resourceList.Rows.Count = 0 Then Exit Sub

            ' Find the port used by an active Login Agent
            For Each dr As DataRow In resourceList.Rows
                Dim status = CType(dr("statusid"), ResourceDBStatus)
                If status = ResourceDBStatus.Ready Then
                    port = GetPortFromName(CStr(dr("name")))
                End If
            Next
            'If no port found then assume no Login Agent resource is running
            If port = 0 Then Exit Sub
            Dim loginAgentResourceName As String
            If port = DefaultPort Then
                loginAgentResourceName = thisHostName
            Else
                loginAgentResourceName = thisHostName & ":" & port
            End If

            ' Attempt to shut it down
            Using t As New clsTalker(mPingTimeoutSeconds)
                Dim sErr As String = Nothing
                If Not t.Connect(loginAgentResourceName, sErr) Then
                    RaiseWarn(String.Format(
                              "Failed to shutdown Login Agent Resource on " &
                              "port {0}: Failed to connect.", port))
                    Return
                End If
                If Not t.Authenticate() Then
                    RaiseWarn(String.Format(
                              "Failed to shutdown Login Agent Resource on " &
                              "port {0}: Failed to authenticate user.", port))
                    Return
                End If
                If Not t.Say("shutdown waitforsessions", "OK") Then
                    RaiseWarn(String.Format(
                              "Failed to shutdown Login Agent Resource on " &
                              "port {0}.", port))
                    Return
                End If
                RaiseInfo(String.Format("Waiting for Login Agent Resource on port {0} to shutdown...", port))
                WaitForLoginAgentShutdown(t)
            End Using

        Catch ex As Exception
            ' Ignore any exceptions so that this doesn't prevent resource startup
            RaiseWarn(String.Format("Failed to shutdown Login Agent Resource on " &
                                    "port {0}: {1}", port, ex.Message))
        End Try
    End Sub

    ''' <summary>
    ''' Waits for another instance of Resource PC started by the login agent to shut
    ''' down. Blocks until the service appears to have shutdown or after a specific
    ''' time limit.
    ''' </summary>
    Private Sub WaitForLoginAgentShutdown(talker As clsTalker)

        Dim waitTime = TimeSpan.FromSeconds(45)
        Dim reportIntervalTime = TimeSpan.FromSeconds(5)
        Dim retryTime = TimeSpan.FromSeconds(1)
        Dim nextReportTime = TimeSpan.Zero

        Dim pingTimeout = TimeSpan.FromMilliseconds(1000)

        Dim watch = Stopwatch.StartNew()

        While talker.Say("ping", "pong", pingTimeout.Milliseconds)

            If (watch.Elapsed > nextReportTime) Then
                RaiseInfo("Login Agent Resource is still running")
                nextReportTime = watch.Elapsed.Add(reportIntervalTime)
            End If

            Thread.Sleep(retryTime)

            If (watch.Elapsed > waitTime) Then
                RaiseInfo("Login Agent Resource did not shut down within allowed time")
                Return
            End If

        End While

        ' Allow some additional time in case there is a delay between when resource shuts
        ' down and port becomes available
        Thread.Sleep(TimeSpan.FromSeconds(1))

        RaiseInfo("Login Agent Resource has stopped running")

    End Sub

    ''' <summary>
    ''' Setup a timer to periodically refresh the db with the state of this resource
    ''' </summary>
    Private Sub SetUpdateResourceInfoTimer()

        ' get the db configurable setting for refresh freq and
        ' create a timer to call the delegate when it's time to update
        mRefreshTimer = New Timers.Timer() With {
            .Interval = gSv.GetRuntimeResourceRefreshFrequency() * 1000,
            .AutoReset = False,
            .Enabled = True
        }

    End Sub

    ''' <summary>
    ''' Stop the timer that triggers a periodic database update of the resource's
    ''' statue
    ''' </summary>
    Private Sub ClearUpdateResourceInfoTimer()
        ' Localise the timer and nullify the memvar before disposing so we don't
        ' have a race condition where the timer handling tries to enable a disposed
        ' timer
        Dim timer = mRefreshTimer
        mRefreshTimer = Nothing
        timer?.Dispose()
    End Sub

    ''' <summary>
    ''' Update the database with the state of this resource pc
    ''' </summary>
    Private Sub RefreshDBWithCurrentState()
        Try
            gSv.RefreshResourcePC(ResourceName,
                                  ResourceDBStatus.Ready,
                                  Runners.ActiveOrPendingCount,
                                  Runners.ActiveCount)

        Catch ex As Exception
            StopAndRaiseFailed(Nothing, $"failed to update resourcepc details on the database - {ex.ToString}")
        End Try

    End Sub


    Private Sub HandleRefreshStateTimerElapsed(source As Object, e As ElapsedEventArgs) _
     Handles mRefreshTimer.Elapsed
        Try
            RefreshDBWithCurrentState()
        Finally
            Dim timer = mRefreshTimer
            If timer IsNot Nothing Then timer.Enabled = True
        End Try
    End Sub

    Private Sub UpdatePoolMembership(errorLogger As Action(Of String))

        Dim isControllerNow As Boolean = False
        Dim sErr As String = ""
        Dim diagnosticResult As Integer
        Dim oldPoolId As Guid = PoolId

        Dim isController = Not PoolId.Equals(Guid.Empty) AndAlso mResourceID.Equals(mPoolControllerID)
        Try
            gSv.PoolUpdate(mResourceID, isController, PoolId, mPoolControllerID, isControllerNow, diagnosticResult, mIsAutoArchiver)

            UpdateDiags(diagnosticResult)
            If Not PoolId.Equals(oldPoolId) OrElse isControllerNow Then
                Dim poolType As String
                If mPoolControllerID.Equals(mResourceID) Then
                    poolType = "Controller"
                Else
                    poolType = "Member"
                End If
                RaiseEvent Info(poolType & " of pool " & PoolId.ToString())
                AddNotification("POOL STATUS CHANGED")
                UpdateConnectionManager(oldPoolId)
            End If
        Catch ex As Exception
            errorLogger($"Unable to update pool information. {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' The main processing thread for the listener.
    ''' </summary>
    Private Sub ListenThread()
        Try
            Dim sErr As String = Nothing
            Dim cultureName = CultureInfo.CurrentCulture().NativeName
            'Get our certificate first
            If ResourceOptions.SSLCertHash IsNot Nothing Then
                For Each loc As StoreLocation In {StoreLocation.CurrentUser, StoreLocation.LocalMachine}
                    Dim store = New X509Store(StoreName.My, loc)
                    store.Open(OpenFlags.ReadOnly)
                    Dim col = store.Certificates.Find(X509FindType.FindByThumbprint, ResourceOptions.SSLCertHash, True)
                    mSSLCert = col.OfType(Of X509Certificate).FirstOrDefault()
                    If mSSLCert IsNot Nothing Then Exit For
                Next
                If mSSLCert Is Nothing Then
                    StopAndRaiseFailed(Nothing, $"Unable to find the requested SSL certificate ({ResourceOptions.SSLCertHash}) - ensure that it is installed in the local certificate store and is valid.")
                    Return
                End If
            Else
                If Not ResourceOptions.IsAuto AndAlso gSv.GetRequireSecuredResourceConnections() Then
                    StopAndRaiseFailed(Nothing, "Secured Resource Connections are required and no certificate has been specified")
                    Return
                End If
            End If

            ResourceName = ResourceMachine.GetName(ResourceOptions.Port)

            'Attempt to shutdown Login Agent (if running)
            If Not IsLoginAgent Then TryStopLoginAgent()

            'Start listening on our port first, because we need to know we can do so before
            'we register with the database, in case another instance is already running...
            Dim listener As TcpListener = TcpListener.Create(ResourceOptions.Port)
            Try
                listener.Start()
                RaiseInfo("Listener started.")
            Catch e As Exception
                StopAndRaiseFailed(Nothing, $"Unable to start listener - is this machine already running as a Resource PC? : {e.Message}")
                Return
            End Try

            'Now we can register with the database...
            Dim resourceAttribs As ResourceAttribute
            If ResourceOptions.IsLocal Then
                resourceAttribs = ResourceAttribute.Local
            Else
                resourceAttribs = ResourceAttribute.None
            End If

            If IsLoginAgent Then resourceAttribs = resourceAttribs Or ResourceAttribute.LoginAgent
            If Not ResourceOptions.IsPublic Then resourceAttribs = resourceAttribs Or ResourceAttribute.Private
            If ResourceOptions.IsAuto Then resourceAttribs = resourceAttribs Or ResourceAttribute.DefaultInstance

            Try
                Log.Debug("Register resource PC {resource}", New With {ResourceName, clsUtility.GetFQDN(), resourceAttribs, UserId, cultureName})
                gSv.RegisterResourcePC(ResourceName, clsUtility.GetFQDN(), ResourceDBStatus.Ready,
                                              ResourceOptions.SSLCertHash IsNot Nothing, resourceAttribs,
                                              True, UserId, cultureName)


            Catch ex As Exception
                StopAndRaiseFailed(listener, $"Unable to register with the database - {ex.Message}")
                Return
            End Try

            ' If we had set the archive lock previously and it is still set then release the lock.
            ' Likely due to an error during archiving.
            Try
                Dim archivingResourceName As String = String.Empty
                Dim lastUpdated As DateTime
                If gSv.IsArchiveLockSet(archivingResourceName, lastUpdated) AndAlso archivingResourceName = ResourceName Then
                    gSv.ReleaseArchiveLock()
                End If
            Catch ex As Exception
                ' No reason to stop the listener. Log a warning and carry on.
                RaiseEvent Warn("Unable to release Archive lock during startup. " & ex.Message)
            End Try



            mResourceID = gSv.GetResourceId(ResourceName)
            If mResourceID = Guid.Empty Then
                Log.Warn("Resource id {0} cannot be found", mResourceID)
            End If

            Dim lastRegistered As Date = Date.Now
            Dim lastVarNotify As Date = Date.Now
            Dim lastArchiveCheck As Date = Date.Now

            ' Update the database state of any sessions held by this listener. This
            ' thread can be restarted if the DB connection is lost, but the runners
            ' continue to process the sessions, so we can't guarantee that they are not
            ' running at the moment.
            Try
                ResolveRunningSessions()

            Catch e As Exception
                RaiseError("Error while resolving sessions on resume: {0}", e.Message)
                listener.Stop()
                RunState = ResourcePcRunState.Stopped
                Exit Sub
            End Try

            UpdatePoolMembership(Sub(x) StopAndRaiseFailed(listener, x))
            UpdateConnectionManager(Guid.Empty)

            'Pick up any pending sessions from the database...
            Try
                Using dr As IDataReader = gSv.GetPendingSessions().CreateDataReader()
                    Dim prov As New ReaderDataProvider(dr)
                    While dr.Read()
                        If Not mResourceID.Equals(prov.GetGuid("runningresourceid")) _
                     Then Continue While
                        Dim origId As Guid = prov.GetGuid("sessionid")
                        If Runners.FindRunner(origId) Is Nothing Then
                            Dim newRunner As ListenerRunnerRecord = Nothing
                            Dim runnerRequest As New RunnerRequest() With {
                            .ResumeSession = True,
                            .SessionId = origId,
                            .ProcessId = prov.GetGuid("processid"),
                             .StarterUserId = prov.GetGuid("starteruserid"),
                            .StarterResourceId = prov.GetGuid("starterresourceid"),
                            .RunningResourceId = prov.GetGuid("runningresourceid"),
                            .QueueIdent = prov.GetValue("queueid", 0),
                            .AutoInstance = False,
                            .AuthorisationToken = Nothing}
                            Try
                                newRunner = CreateRunner(runnerRequest)
                                RaiseInfo("Restored pending session {0}", origId)
                                AddNotification("RESUMED {0} {1}", origId, origId)
                            Catch
                                'If we couldn't re-create the session, set it to be failed.
                                Try
                                    gSv.SetSessionTerminated(origId, DateTimeOffset.Now,
                                                             SessionExceptionDetail.InternalError("Failed to recreate session"))
                                    RaiseError("Failed to recreate session {0}: {1}",
                                                origId, sErr)
                                    AddNotification("END {0} {1}", origId, RunnerStatus.FAILED)
                                Catch ex As Exception
                                    RaiseError("Failed to terminate session {0}: {1}", origId, ex.Message)
                                End Try
                            End Try
                        End If
                    End While
                End Using
            Catch ex As Exception
                RaiseError("Exception while restoring pending sessions - {0}", ex.Message)
            End Try


            'List of clients to remove from Clients, which we use to avoid removing one
            'from the collection during iteration.
            Dim clientsToRemove As New List(Of IListenerClient)

            'Calculate the 'availability' of this Resource PC before we start. This needs
            'to be done here to take into account any restored pending sessions we have
            'picked up from the database. It will also set the Resource PC status to Ready.
            Availability = Runners.Availability

            Dim disconnectcheckcount As Integer = 0

            ' Kick off the more frequent update thread on a timer
            SetUpdateResourceInfoTimer()

            ' Start polling for new connections
            Dim connectionsThread As New Thread(AddressOf AcceptIncommingConnections)
            connectionsThread.IsBackground = True
            connectionsThread.Start(listener)

            Try
                'Main loop - stays in here until told to stop...
                Do
                    While Not NewClients.IsEmpty
                        Dim client As ListenerClient = Nothing
                        If NewClients.TryTake(client) Then
                            Clients.Add(client)
                        End If
                    End While

                    'Review status as required...
                    Dim bNeedRecalc As Boolean = False
                    SyncLock Runners.Lock
                        For Each rr In Runners
                            If rr.mReviewStatus Then
                                Select Case rr.Status
                                    Case RunnerStatus.STOPPED, RunnerStatus.COMPLETED, RunnerStatus.FAILED, RunnerStatus.IDLE, RunnerStatus.PENDING
                                        Dim sNot2 As String
                                        If rr.Status = RunnerStatus.PENDING Then
                                            sNot2 = "PENDING " & rr.SessionID.ToString()
                                        Else
                                            sNot2 = "END " & rr.SessionID.ToString() & " " & rr.Status.ToString()
                                            'Need to flush our variable change notifications before notifying that
                                            'a session has ended, otherwise they can appear out of order.
                                            DoVarChangeNotifications()
                                            lastVarNotify = Date.Now
                                        End If
                                        AddNotification(sNot2)
                                        NotifyStatus()
                                        bNeedRecalc = True
                                End Select
                                rr.mReviewStatus = False
                            End If
                        Next
                    End SyncLock
                    If bNeedRecalc Then Availability = Runners.Availability

                    'Process session variable notifications periodically...
                    If Date.Now - lastVarNotify > TimeSpan.FromSeconds(2) Then
                        DoVarChangeNotifications()
                        lastVarNotify = Date.Now
                    End If

                    'Process session status setting retries...
                    ProcessSessionStatusRetries()

                    'Update database registration if it hasn't been done for a while. This ensures
                    'that the connection is kept alive - for example, when using a remote server via
                    '.NET remoting, the remote clsServer object will be destroyed if it is not
                    'accessed for 5(?) minutes.
                    If Date.Now - lastRegistered > TimeSpan.FromMinutes(2) Then
                        Try
                            gSv.RegisterResourcePC(ResourceName, clsUtility.GetFQDN(),
                                               ResourceDBStatus.Ready, ResourceOptions.SSLCertHash IsNot Nothing,
                                               resourceAttribs, True, UserId, cultureName)
                        Catch ex As Exception
                            StopAndRaiseFailed(listener, $"Unable to register with the database - {ex.Message}")
                            Return
                        End Try

                        UpdatePoolMembership(Sub(x) StopAndRaiseFailed(listener, x))
                        lastRegistered = Date.Now
                    End If

                    'If we're the Auto-Archiving Resource, we need to periodically check if
                    'it's time to do some archiving...
                    If mIsAutoArchiver AndAlso Date.Now - lastArchiveCheck > New TimeSpan(0, 45, 0) Then
                        mtArchiver = New Thread(New ThreadStart(AddressOf ArchiveThread))
                        mtArchiver.Start()
                        lastArchiveCheck = Date.Now
                    End If

                    disconnectcheckcount = (disconnectcheckcount + 1) Mod 20


                    'Read pending input and process commands from all
                    'attached clients...
                    For Each client In Clients
                        Try


                            'Need to specifically check if the client is no longer
                            'connected (But not every time round the loop, for performance
                            'reasons).
                            If disconnectcheckcount = 0 AndAlso client.ShouldClose() Then
                                Log.Debug("Adding client for removal, 1")
                                clientsToRemove.Add(client)
                                Continue For
                            End If

                            'Create a default 'null' response. This will be amended as
                            'necessary when the command is processed. Sometimes it is
                            'deliberately left untouched, resulting in no response at
                            'all during this iteration (but one is expected to come
                            'later!)
                            Dim response As New ListenerResponse()

                            If Not client.mWSRunner Is Nothing Then
                                ' We're running a process for an HTTP Web Service client.
                                If HandleWSRunning(response, client.mWSRunner, client.WebServiceProcessId, client.WebServiceProcessName, client.WebServiceProcessType) Then
                                    client.mWSRunner = Nothing
                                End If
                                GoTo sendoutput

                            ElseIf client.WebServiceQueuedPost IsNot Nothing Then

                                ' We have a queued web service POST
                                Try
                                    Dim nr = HandleWSRequest(response, client.WebServiceProcessId, client.WebServiceProcessName, client.WebServiceProcessType, client.UserId, client.WebServiceQueuedPost, client.AuthenticatedUser.AuthorisationToken)
                                    If nr IsNot Nothing Then
                                        client.mWSRunner = nr
                                        client.WebServiceQueuedPost = Nothing
                                    End If
                                Catch ex As Exception
                                    response.SOAPFault(ex.Message)
                                    client.WebServiceQueuedPost = Nothing
                                End Try

                                GoTo sendoutput

                            ElseIf client.mbPendingHTTPPost Then

                                If client.HTTPPayloadRecieved() Then
                                    ' Our POST parameters have arrived.
                                    client.mbPendingHTTPPost = False
                                    Dim sPostData = client.ReadHTTPPayload()

                                    If client.mbPOSTWSExec Then

                                        If (clsAPC.Diagnostics And clsAPC.Diags.LogWebServices) <> 0 Then
                                            RaiseEvent Info("Received SOAP request:" & sPostData)
                                        End If

                                        'Check credentials when trying to use a process
                                        'as a web service...
                                        Dim authed As Boolean = False
                                        If client.mHTTPCredentials IsNot Nothing AndAlso client.mHTTPCredentials.Substring(0, 6) = "Basic " Then
                                            'Decode the supplied credentials...
                                            Dim bb As Byte() = Convert.FromBase64String(client.mHTTPCredentials.Substring(6))
                                            Dim cr As String = Encoding.UTF8.GetString(bb)
                                            Dim up As String() = cr.Split(New Char() {":"c}, 2)
                                            If up.Length = 2 Then
                                                client.AuthenticatedUser = gSv.ValidateWebServiceRequest(up(0), New SafeString(up(1)), client.WebServiceProcessId)
                                                If client.UserSet Then authed = True
                                            End If
                                        End If

                                        Dim wsPermissions As New WebServicesPermissions(gSv)
                                        Dim canExecuteWebService = wsPermissions.CanExecuteWebService(
                                            client.WebServiceProcessType,
                                            client.AuthenticatedUser,
                                            client.WebServiceProcessId)
                                        If Not authed OrElse Not canExecuteWebService.Success Then
                                            response.HTTP = True
                                            response.Text = canExecuteWebService.ErrorMessage
                                            response.HTTPStatus = HttpStatusCode.Unauthorized
                                            response.HTTPHeaders.Add("WWW-Authenticate: Basic realm=""BluePrism""")
                                            GoTo sendoutput
                                        End If

                                        Try
                                            Dim nr = HandleWSRequest(response, client.WebServiceProcessId, client.WebServiceProcessName, client.WebServiceProcessType, client.UserId, sPostData, client.AuthenticatedUser.AuthorisationToken)
                                            If nr IsNot Nothing Then
                                                client.mWSRunner = nr
                                            Else
                                                client.WebServiceQueuedPost = sPostData
                                            End If
                                        Catch ex As Exception
                                            response.SOAPFault(ex.Message)
                                            client.WebServiceQueuedPost = Nothing
                                        End Try

                                        GoTo sendoutput

                                    ElseIf client.mbPOSTExec Then
                                        'Execute the requested commands
                                        Dim parms() As String = sPostData.Split("&"c)
                                        For Each s As String In parms
                                            Dim ss As String() = s.Split("="c)
                                            'We are just ignoring the name of each parameter
                                            'and treating the value as a command...
                                            Dim commandResult = client.CommandFactory.ExecCommand(System.Web.HttpUtility.UrlDecode(ss(1)))
                                            response.Text &= commandResult.output
                                            clientsToRemove.AddRange(commandResult.clientsToRemove)
                                        Next
                                        response.HTTP = True
                                        response.HTTPContentType = "text/plain"
                                        response.HTTPCharset = "utf-8"
                                        GoTo sendoutput
                                    Else
                                        ' This code is potentially obsolete as it requires
                                        ' a "POST /automate " HTTP request, which is not
                                        ' something that happens from within the product.                                    '
                                        ' Not removing the code just in case someone is using
                                        ' it by externally making the request, although this
                                        ' seems unlikely given that it just returns the
                                        ' 'Action Page'.

                                        'Parse the parameters in the posted data and populate
                                        'the following variables accordingly...
                                        Dim parms() As String = sPostData.Split("&"c)
                                        Dim sUser As String = "", sPassword As String = "", sProcess As String = "", sParams As String = ""
                                        Dim sDestUrl_Err As String = "", sDestUrl_Ok As String = ""
                                        For Each s As String In parms
                                            Dim ss As String() = s.Split("="c)
                                            Dim sValue As String = System.Web.HttpUtility.UrlDecode(ss(1))
                                            'Escape the value for the javascript...
                                            sValue = sValue.Replace("""", "\""")
                                            Select Case ss(0).ToLower()
                                                Case "user"
                                                    sUser = sValue
                                                Case "password"
                                                    sPassword = sValue
                                                Case "process"
                                                    sProcess = sValue
                                                Case "params"
                                                    sParams = sValue
                                                Case "desturl_err"
                                                    sDestUrl_Err = sValue
                                                Case "desturl_ok"
                                                    sDestUrl_Ok = sValue
                                            End Select
                                        Next
                                        'Output a page that does what was requested...
                                        response.Text = mActionPage
                                        response.Text = response.Text.Replace("$$AUTOMATE-user-$$", sUser)
                                        response.Text = response.Text.Replace("$$AUTOMATE-password-$$", sPassword)
                                        response.Text = response.Text.Replace("$$AUTOMATE-process-$$", sProcess)
                                        response.Text = response.Text.Replace("$$AUTOMATE-params-$$", sParams)
                                        response.Text = response.Text.Replace("$$AUTOMATE-desturl_err-$$", sDestUrl_Err)
                                        response.Text = response.Text.Replace("$$AUTOMATE-desturl_ok-$$", sDestUrl_Ok)
                                        response.HTTP = True
                                        response.HTTPContentType = "text/html"
                                        response.HTTPCharset = "utf-8"
                                        GoTo sendoutput
                                    End If
                                Else
                                    'Not enough data arrived yet
                                    GoTo sendoutput
                                End If
                            End If


                            'Check the input buffer for a carriage return.
                            'If there is one, everything preceding it is
                            'a command waiting to be processed...
                            Dim sCommand As String
                            Try
                                sCommand = client.ReadLine()
                            Catch e As Exception
                                Log.Debug($"Failed to readline, {e}")
                            End Try



                            If sCommand IsNot Nothing Then
                                    Log.Debug("Listener [{0}] Line Received: {1}", client.RemoteHostIdentity, LogOutputHelper.Sanitize(sCommand))
                                    If sCommand.Length = 0 Then

                                        ' Handler for a blank line - terminates an HTTP request
                                        ' but otherwise is meaningless
                                        If client.mbPendingHTTPRequest Then
                                            client.mbPendingHTTPRequest = False
                                            If client.mHTTPRequests Is Nothing Then

                                                'Give the client a 100 Continue status if it
                                                'wanted one, so it will send us the body of the
                                                'POST. If it didn't expect that, then it is going
                                                'to send us the body anyway, so we say nothing.
                                                If client.mHTTPContinueExpected Then
                                                    response.Text = "HTTP/1.1 100 Continue" & vbCrLf & vbCrLf
                                                End If

                                                'If we're doing a POST, we'll wait for the
                                                'next line which will be our posted parameters.
                                                client.mbPendingHTTPPost = True
                                            Else

                                                If client.mHTTPRequests.Count = 1 AndAlso client.mHTTPRequests(0).StartsWith("ws/") Then
                                                    Dim sHTTPReq As String = client.mHTTPRequests(0)
                                                    If sHTTPReq = "ws/" Then
                                                        'Without any additional path, show the index of services...
                                                        GenerateWSIndex(response)
                                                        GoTo sendoutput
                                                    End If
                                                    'This is a web service request, but the only possible
                                                    'GET request it could be is for the WSDL...
                                                    If sHTTPReq.Substring(sHTTPReq.Length - 5).ToLower() <> "?wsdl" Then
                                                        response.HTTP = True
                                                        response.HTTPStatus = HttpStatusCode.NotFound
                                                        response.HTTPContentType = "text/plain"
                                                        response.HTTPCharset = "utf-8"
                                                        response.Text = "A ws GET request must be for the WSDL"
                                                        GoTo sendoutput
                                                    Else
                                                        Dim procname As String = sHTTPReq.Substring(3, sHTTPReq.Length - 3 - 5)
                                                        GenerateWSDL(response, procname)
                                                        GoTo sendoutput
                                                    End If

                                                Else
                                                    'Execute the requested commands
                                                    For Each sCmd As String In client.mHTTPRequests
                                                        Dim commandResult = client.CommandFactory.ExecCommand(sCmd)
                                                        response.Text &= commandResult.output
                                                        clientsToRemove.AddRange(commandResult.clientsToRemove)
                                                    Next
                                                    response.HTTP = True
                                                    response.HTTPContentType = "text/plain"
                                                    response.HTTPCharset = "utf-8"
                                                End If
                                            End If
                                        Else
                                            response.Text = "Say something!" & vbCrLf
                                        End If

                                    ElseIf client.mbPendingHTTPRequest Then
                                        ' HTTP request headers, which we're mostly ignoring!
                                        If sCommand.StartsWith("Content-Length:", StringComparison.InvariantCultureIgnoreCase) Then
                                            Try
                                                client.miHTTPPOSTLength = Integer.Parse(sCommand.Substring(16))
                                            Catch ex As Exception
                                                'If a dodgy length is sent, we will just ignore
                                                'the posted body.
                                                client.miHTTPPOSTLength = 0
                                            End Try
                                        ElseIf sCommand.StartsWith("Expect:", StringComparison.InvariantCultureIgnoreCase) AndAlso
                                        sCommand.Contains("100-continue") Then
                                            client.mHTTPContinueExpected = True
                                        ElseIf sCommand.StartsWith("Authorization:", StringComparison.InvariantCultureIgnoreCase) Then
                                            client.mHTTPCredentials = sCommand.Substring(15)
                                        ElseIf sCommand.StartsWith("Connection:", StringComparison.InvariantCultureIgnoreCase) AndAlso
                                        sCommand.Contains("close") Then
                                            client.mHTTPKeepAlive = False
                                        End If

                                    ElseIf sCommand.StartsWith("GET /") AndAlso ResourceOptions.HTTPEnabled Then

                                        ' Handler for an HTTP GET command

                                        If Not client.ParseHTTPCommand(sCommand, response) Then
                                            GoTo sendoutput
                                        End If
                                        client.mbPendingHTTPRequest = True
                                        client.mHTTPContinueExpected = False

                                        Dim sHTTPReq As String = client.mHTTPResource.Substring(1)

                                        'A normal GET request, i.e. not a web service request,
                                        'is just a list of commands separated by &...
                                        'The only other possibility is a WSDL request, which
                                        'will be handled in the same way.
                                        'We queue these, because we can't handle them until we
                                        'have received the rest of the headers.
                                        client.mHTTPRequests = New List(Of String)
                                        Dim cmds As String() = sHTTPReq.Split("&"c)
                                        For Each s As String In cmds
                                            client.mHTTPRequests.Add(System.Web.HttpUtility.UrlDecode(s))
                                        Next

                                    ElseIf sCommand.StartsWith("POST /ws/") AndAlso ResourceOptions.HTTPEnabled Then
                                        'Handle a POST that runs a process as a web service...

                                        If Not client.ParseHTTPCommand(sCommand, response) Then
                                            GoTo sendoutput
                                        End If

                                        Dim procname As String = client.mHTTPResource.Substring(4)
                                        Dim procid As Guid = GetWSProcess(procname, response, client.WebServiceProcessType)
                                        If procid.Equals(Guid.Empty) Then GoTo sendoutput

                                        client.PreventClosure()
                                        client.mbPendingHTTPRequest = True
                                        client.mHTTPContinueExpected = False
                                        client.mHTTPRequests = Nothing
                                        client.mbPOSTExec = False
                                        client.mbPOSTWSExec = True
                                        client.WebServiceProcessId = procid
                                        client.WebServiceProcessName = procname

                                    ElseIf (sCommand.StartsWith("POST /automate ") Or sCommand.StartsWith("POST /automateexec ")) AndAlso
                                ResourceOptions.HTTPEnabled Then

                                        'We currently handle two kinds of POSTs here, see the
                                        'docs for client.mbPOSTExec for enlightenment.

                                        If Not client.ParseHTTPCommand(sCommand, response) Then
                                            GoTo sendoutput
                                        End If

                                        client.mbPendingHTTPRequest = True
                                        client.mHTTPContinueExpected = False
                                        client.mHTTPRequests = Nothing
                                        If sCommand.StartsWith("POST /automateexec ") Then
                                            client.mbPOSTExec = True
                                        Else
                                            ' Not entirely sure whether this ever gets called
                                            ' Requires a POST /automate HTTP request, which is
                                            ' not something that happens from within the product.
                                            client.mbPOSTExec = False
                                        End If
                                        client.mbPOSTWSExec = False

                                    Else
                                        'Normal terminal command...
                                        Dim commandResult = client.CommandFactory.ExecCommand(sCommand)
                                        response.Text = commandResult.output
                                        Log.Debug("Terminal command response {command}", New With {sCommand, response.Text})
                                        clientsToRemove.AddRange(commandResult.clientsToRemove)
                                    End If

                                End If

sendoutput:
                                If response.Text IsNot Nothing Then
                                    client.AllowClosure()
                                    Dim request = New StringBuilder()
                                    'If the output is intended to be sent in HTTP format
                                    'then wrap it up with some headers...
                                    If response.HTTP Then
                                        If client.mHTTPVersion <> "HTTP/0.9" Then
                                            request.AppendLine($"{client.mHTTPVersion} {CInt(response.HTTPStatus)} {HttpWorkerRequest.GetStatusDescription(response.HTTPStatus)}")
                                            If response.Text.Length > 0 Then
                                                request.AppendLine($"Content-Type: {response.HTTPContentType}; charset={response.HTTPCharset}")
                                            End If
                                            request.AppendLine($"Content-Length: {response.ContentLength.ToString()}")
                                            request.AppendLine("Cache-Control: no-cache")
                                            'Send the connection close header unless we are keeping
                                            'the connection alive (the client must have requested
                                            'this)
                                            If Not client.mHTTPKeepAlive Then
                                                request.AppendLine("Connection: close")
                                            End If
                                            'Send any additional HTTP headers...
                                            For Each hdr As String In response.HTTPHeaders
                                                request.AppendLine(hdr)
                                            Next
                                            request.AppendLine()
                                        End If
                                    End If

                                    request.Append(response.Text)
                                    Dim msg = request.ToString()
                                    Dim bdata = Encoding.UTF8.GetBytes(msg)

                                    Try
                                        If Not client.TcpClientDisposed AndAlso Not client.IsDisconnected() Then
                                            Log.Debug("Listener [{0}] Send Line: {1}", client.RemoteHostIdentity, LogOutputHelper.Sanitize(msg))
                                            If Not client.TcpClientDisposed AndAlso Not client.IsDisconnected Then
                                                client.Send(bdata)
                                            End If

                                            'If this was an HTTP response and we are not meant to
                                            'be keeping the connection alive, we can get rid of this
                                            'client now...
                                            If response.HTTP AndAlso Not client.mHTTPKeepAlive Then
                                                client.Close()
                                                clientsToRemove.Add(client)
                                                client.RemoveReason = "HTTP non-keepalive"
                                            End If
                                        End If
                                    Catch e As Exception
                                    End Try

                                End If

                            Catch e As Exception
                                Log.Error(e, "Exception in main listener loop")
                            client.Close()
                            clientsToRemove.Add(client)
                            client.mException = e
                        End Try
                    Next

                    RemoveUnwantedClients(listener, clientsToRemove)

                    SendNotifications(listener, clientsToRemove)

                    Thread.Sleep(50)

                Loop While IsRunningRunstate

                'Clean up and signal main process that we've finished...
                For Each client In Clients
                    client.Close()
                Next

                Clients.Clear()

                StopRunning(listener)
            Catch e As Exception
                StopAndRaiseFailed(listener, $"Exception - {e.Message}")
            End Try
        Catch ex As Exception
            StopAndRaiseFailed(Nothing, $"Exception - {ex.Message}")
            Return
        End Try
    End Sub

    Private Sub SendNotifications(listener As TcpListener, clientsToRemove As List(Of IListenerClient))
        'Send updates to all connected clients who want them, to notify of
        'status changes...
        Dim sNot As String, sMessage As String
        SyncLock mNotificationsLockObject
            While mNotifications.Count > 0
                sNot = mNotifications.Dequeue()
                sMessage = ">>" & sNot & vbCrLf
                Dim bdata As Byte() = Encoding.UTF8.GetBytes(sMessage)
                For Each client In Clients
                    If client.SendStatusUpdates AndAlso (Not sNot.StartsWith("VAR ") OrElse client.SendSessionVariableUpdates) Then
                        Try
                            Log.Debug("Listener [{0}] Send Line: {1}", client.RemoteHostIdentity, LogOutputHelper.Sanitize(sMessage))

                            If Not client.TcpClientDisposed AndAlso Not client.IsDisconnected() Then _
                                client.Send(bdata)
                        Catch e As Exception
                            Log.Trace(e, "Exception while reading session variables")
                            clientsToRemove.Add(client)
                            client.mException = e
                        End Try
                    End If
                Next
                RemoveUnwantedClients(listener, clientsToRemove)
            End While

        End SyncLock
    End Sub

    Private Sub RemoveUnwantedClients(listener As TcpListener, clientsToRemove As List(Of IListenerClient))
        'After iterating clients, remove the ones we wanted to remove...
        For Each client As ListenerClient In clientsToRemove
            If client.IsErrored Then
                RaiseEvent Warn(client.DisconnectMessage)
            Else
                RaiseEvent Verbose(client.DisconnectMessage)
            End If
            Clients.Remove(client)
            If client.IsControllerConnection Then
                UpdatePoolMembership(Sub(x) StopAndRaiseFailed(listener, x))
            End If
        Next
        clientsToRemove.Clear()
    End Sub

    Private ReadOnly Property IsRunningRunstate As Boolean
        Get
            Return RunState = ResourcePcRunState.Running OrElse RunState = ResourcePcRunState.WaitingToStop
        End Get
    End Property


    Private Sub AcceptIncommingConnections(obj As Object)
        Dim listener = CType(obj, TcpListener)

        While IsRunningRunstate

            Try
                'need to check for pending, otherwise the thread can never exit.
                If listener.Pending Then

                    Dim tcpClient = listener.AcceptTcpClient()
                    Dim client = New ListenerClient(mCommandFactoryFactory, tcpClient, mSSLCert, mPingTimeoutSeconds)
                    Log.Trace("Accepting incoming TCP connection {connection}", New With {client.RemoteHostIdentity})
                    Try
                        Dim bdata As Byte()
                        'Send a message and disconnect immediately if we are in local mode and the client is not the local machine...
                        If ResourceOptions.IsLocal AndAlso Not client.IsLocal() Then
                            RaiseEvent Warn("Rejected connection from " & client.RemoteHostIdentity)
                            Dim msg = "Local connections only - you are " & client.RemoteHostIdentity & " - disconnecting."
                            bdata = Encoding.UTF8.GetBytes(msg & vbCrLf)
                            Log.Debug("Listener [{0}] Send Line: {1}", client.RemoteHostIdentity, msg)
                            client.Send(bdata)
                            client.Close()
                        Else
                            If mAscrOn Then
                                SyncLock mNotificationsLockObject
                                    mNotifications.Clear()
                                End SyncLock
                            End If
                            NewClients.Add(client)
                            RaiseEvent Verbose("New connection from " & client.RemoteHostIdentity)
                        End If
                    Catch e As Exception
                        Log.Info(e, "Unable to process new connection")
                    End Try
                Else
                    'allow the thread to run, and check the IsRunningRunState
                    Thread.Sleep(100)
                End If

            Catch sslex As SslAuthenticationException
                Dim errorText = "Unable to accept incoming connection because the certificate (/sslcert) cannot be used for inbound connections. Ensure the logged in user has permission to read the certificate private key."
                Log.Info(sslex, errorText)
                RaiseEvent Verbose(errorText)
            Catch ex As Exception
                Log.Info(ex, "Unable to accept incoming connection")
                RaiseEvent Verbose("Unable to accept incoming connection - " & ex.Message)
            End Try

        End While

    End Sub

    ''' <summary>
    ''' Validate and get the ID of a process to be used via the HTTP Web Service
    ''' interface.
    ''' </summary>
    ''' <param name="procname">The name of the process</param>
    ''' <param name="response">A ListenerResponse object in its default state. In the event
    ''' that the process is not available, Guid.Empty is returned and this object is
    ''' populated with details. Otherwise, it is left unchanged.</param>
    ''' <param name="proctype">On successful return, contains the type of the process.
    ''' In the event of failure, the value of this is undefined.</param>
    ''' <returns>The ID of the process, or Guid.Empty if the process is not available,
    ''' in which case 'response' will have been filled in with the relevant details to
    ''' be returned to the client.</returns>
    Private Function GetWSProcess(ByVal procname As String, ByVal response As ListenerResponse, ByRef proctype As DiagramType) As Guid

        'Find the ID of the process, and at the same time determine whether it is a
        'business object or a normal process...
        proctype = DiagramType.Process
        Dim procid As Guid = gSv.GetProcessIDByWSName(procname, False)
        If procid = Guid.Empty Then
            proctype = DiagramType.Object
            procid = gSv.GetProcessIDByWSName(procname, True)
            If procid = Guid.Empty Then
                'There isn't a process in the database with the
                'requested name...
                response.Text = "The requested process does not exist"
                response.HTTPStatus = HttpStatusCode.NotFound
                response.HTTP = True
                Return Guid.Empty
            End If
        End If

        'Make sure the process is valid for ws access...
        Dim attr As ProcessAttributes
        Try
            attr = gSv.GetProcessAttributes(procid)
        Catch ex As Exception
            response.Text = "Failed to get process attributes - " & ex.Message
            response.HTTPStatus = HttpStatusCode.NotFound
            response.HTTP = True
            Return Guid.Empty
        End Try
        If (attr And ProcessAttributes.PublishedWS) = 0 Then
            response.Text = "The requested process is not published"
            response.HTTPStatus = HttpStatusCode.Forbidden
            response.HTTP = True
            Return Guid.Empty
        End If
        If (attr And ProcessAttributes.Retired) <> 0 Then
            response.Text = "The requested process is retired"
            response.HTTPStatus = HttpStatusCode.Forbidden
            response.HTTP = True
            Return Guid.Empty
        End If
        Return procid
    End Function

    Public Function CreateRunner(runnerRequest As RunnerRequest) As ListenerRunnerRecord Implements IListener.CreateRunner
        Thread.Yield()
        'Check that our license will allow us to create another concurrent session.
        'Note that if we're a 'local' resource PC, our sessions don't count towards
        'that licensing limit, so we skip the check altogether.
        If Not ResourceOptions.IsLocal AndAlso Not gSv.CanCreateSession(runnerRequest.SessionId) Then
            Throw New OperationFailedException(Licensing.GetOperationDisallowedMessage(
                Licensing.SessionLimitReachedMessage))
        End If

        Dim reqdRunMode As BusinessObjectRunMode
        Try
            reqdRunMode = GetRequiredRunMode(runnerRequest.ProcessId)
        Catch ex As Exception
            Log.Debug("Failed to get runner mode {runner}", New With {runnerRequest.ProcessId, ex.Message, ex})
            Throw New OperationFailedException(ex, My.Resources.clsListenerFailedToGetEffectiveRunMode & ex.Message)
        End Try

        ''If the resource is not available then say so...
        If Not IsAvailable(reqdRunMode) Then
            Throw New OperationFailedException("UNAVAILABLE")
        End If

        'Create the pending session in the database if necessary...
        Dim sessionNo As Integer
        Dim sProcessXML As String = Nothing
        If Not runnerRequest.ResumeSession Then
            Try
                If gSv.GetControllingUserPermissionSetting Then
                    sProcessXML = gSv.GetProcessXmlForCreatedSession(
                    runnerRequest.ProcessId, runnerRequest.QueueIdent, runnerRequest.AuthorisationToken?.ToString, runnerRequest.StarterResourceId,
                    runnerRequest.RunningResourceId, DateTimeOffset.Now, runnerRequest.SessionId, sessionNo)
                Else
                    gSv.CreatePendingSession(
                    runnerRequest.ProcessId, runnerRequest.QueueIdent, runnerRequest.StarterUserId, runnerRequest.StarterResourceId,
                    runnerRequest.RunningResourceId, DateTimeOffset.Now, runnerRequest.SessionId, sessionNo)

                End If

                gSv.CreateProcessAlert(
                    AlertEventType.ProcessPending, runnerRequest.SessionId, runnerRequest.StarterResourceId)
            Catch ex As Exception
                Throw New OperationFailedException("ERROR : " & ex.Message)
            End Try

        Else
            sessionNo = gSv.GetSessionNumber(runnerRequest.SessionId)
        End If

        Dim lastmod As DateTime
        Dim sErr As String = Nothing
        If Not clsAPC.ProcessLoader.GetProcessXML(runnerRequest.ProcessId, sProcessXML, lastmod, sErr) Then
            Throw New OperationFailedException(sErr)
        End If

        'Get some info - actually all we want is the process type.
        Dim sCreatedBy As String = Nothing, dCreateDate As Date, sModifiedBy As String = Nothing, dModifiedDate As Date
        Dim ttype As DiagramType
        If Not gSv.GetProcessInfo(runnerRequest.ProcessId, sCreatedBy, dCreateDate, sModifiedBy, dModifiedDate, ttype) Then
            Throw New OperationFailedException("ERROR: Failed to get process info")
        End If
        ' and the process name
        Dim procName As String = gSv.GetProcessNameByID(runnerRequest.ProcessId)

        'Create runner record...
        Dim newRunner As New ListenerRunnerRecord(sProcessXML, runnerRequest.ProcessId, runnerRequest.SessionId, sessionNo, mResourceID, runnerRequest.StarterUserId, Me)
        newRunner.mRunMode = reqdRunMode
        If runnerRequest.AutoInstance Then newRunner.mAutoInstance = True

        Runners.Add(newRunner)

        'For a business object, we need to run the initialise page...
        If ttype = DiagramType.Object Then
            Availability = Runners.Availability
            newRunner.mStartupParams = Nothing
            newRunner.mAction = clsProcess.InitPageName
            newRunner.StartThread()
        End If

        RaiseInfo("Created session: {0}; process: {1}; Run Mode: {2}",
         runnerRequest.SessionId, procName, reqdRunMode)

        Return newRunner

    End Function

    Private ReadOnly mRunModeCache As Dictionary(Of Guid, BusinessObjectRunMode) = New Dictionary(Of Guid, BusinessObjectRunMode)

    Private Function GetRequiredRunMode(processId As Guid) As BusinessObjectRunMode
        If mEnableRunModeCache AndAlso mRunModeCache.ContainsKey(processId) Then
            Return mRunModeCache(processId)
        End If

        Dim extRunModes As New Dictionary(Of String, BusinessObjectRunMode)
        Dim externalObjectDetails = CType(Options.Instance.GetExternalObjectsInfo(), clsGroupObjectDetails)

        Dim comGroup As New clsGroupObjectDetails(externalObjectDetails.Permissions)
        For Each comObj In externalObjectDetails.Children
            If TypeOf (comObj) Is clsCOMObjectDetails Then _
                    comGroup.Children.Add(comObj)
        Next
        Using objRefs As New clsGroupBusinessObject(comGroup, Nothing, Nothing)
            extRunModes = objRefs.GetNonVBORunModes()
        End Using

        Dim runMode = gSv.GetEffectiveRunMode(processId, extRunModes)

        If mEnableRunModeCache Then
            mRunModeCache.Add(processId, runMode)
        End If

        Return runMode
    End Function

    Private Function IsAvailable(requiredRunMode As BusinessObjectRunMode) As Boolean
        If Availability = BusinessObjectRunMode.Background Then _
            Return requiredRunMode = BusinessObjectRunMode.Background

        If Availability = BusinessObjectRunMode.Foreground Then _
                Return requiredRunMode = BusinessObjectRunMode.Background _
                 OrElse requiredRunMode = BusinessObjectRunMode.Foreground

        If Availability = BusinessObjectRunMode.Exclusive Then _
                Return requiredRunMode = BusinessObjectRunMode.Background _
                 OrElse requiredRunMode = BusinessObjectRunMode.Foreground _
                 OrElse requiredRunMode = BusinessObjectRunMode.Exclusive

        Return False
    End Function

    ''' <summary>
    ''' Send notification of the current status (i.e. processes running, pending)
    ''' to connected clients who are interested.
    ''' </summary>
    Public Sub NotifyStatus() Implements ISessionNotifier.NotifyStatus, IListener.NotifyStatus
        Try
            If mRefreshTimer IsNot Nothing Then mRefreshTimer.Stop()
            RefreshDBWithCurrentState()
        Finally
            If mRefreshTimer IsNot Nothing Then mRefreshTimer.Start()
        End Try

        AddNotification("STATUS Running:{0} Pending:{1}", GetActiveSessionCount(), GetPendingSessionCount())
    End Sub


    ''' <summary>
    ''' Add a notification message, to be sent to all connected clients
    ''' </summary>
    ''' <param name="msg">The notification message</param>
    Public Sub AddNotification(ByVal msg As String) Implements ISessionNotifier.AddNotification, IListener.AddNotification
        SyncLock mNotificationsLockObject
            Log.Debug($"AddNotification: {msg}")
            mNotifications.Enqueue(msg)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Adds a formatted notification message to be sent to all connected clients
    ''' </summary>
    ''' <param name="formattedMsg">The message, with format placeholders.</param>
    ''' <param name="args">The arguments to set into the given message.</param>
    Public Sub AddNotification(ByVal formattedMsg As String, ByVal ParamArray args() As Object) Implements ISessionNotifier.AddNotification, IListener.AddNotification
        AddNotification(String.Format(formattedMsg, args))
    End Sub


End Class
