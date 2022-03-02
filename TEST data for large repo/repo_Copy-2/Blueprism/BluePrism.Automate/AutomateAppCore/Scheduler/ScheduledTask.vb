Imports System.Xml
Imports System.Threading
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Scheduling
Imports System.Runtime.Serialization
Imports System.Threading.Tasks
Imports BluePrism.Scheduling.ScheduleData
Imports LocaleTools
Imports NLog
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateAppCore.Sessions
Imports BluePrism.Core.Resources
Imports BluePrism.Server.Domain.Models
Imports BluePrism.ClientServerResources.Core.Events
Imports BluePrism.ClientServerResources.Core.Enums

''' <summary>
''' The result of a task execution
''' </summary>
Public Enum TaskResult
    Success
    Failure
End Enum

''' <summary>
''' Class to represent a basic task
''' </summary>
<Serializable>
<DataContract([Namespace]:="bp")>
<KnownType(GetType(ScheduledSession))>
Public Class ScheduledTask
    Inherits DescribedNamedObject

    Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()

#Region " Temporary ID Handling "

    ' A temporary ID to use before the task is granted a proper ID.
    ' Initially at zero, this is decremented before the first temporary
    ' ID is assigned by the GetTemporaryId() method, and then
    ' decremented as each subsequent temp ID is generated.
    Private Shared tempId As Integer = 0

    ''' <summary>
    ''' Assigns a temporary ID to the given task. This ensures that the
    ''' ID given the task is unique to that task, but is negative,
    ''' indicating that it is temporary until a proper ID is assigned to
    ''' it (typically by the schedule store)
    ''' </summary>
    Private Shared Sub AssignTemporaryId(ByVal t As ScheduledTask)
        ' Interlocked.Decrement is guaranteed atomic and is designed to be used
        ' in a highly threaded system, so there's no need for external locking.
        t.Id = Interlocked.Decrement(tempId)
    End Sub

    ''' <summary>
    ''' Checks if this task has a temporary ID or not - the implication being
    ''' that if it has a temporary ID, it has not been saved to the database.
    ''' </summary>
    ''' <returns>True if this task has a temporary ID, False otherwise.
    ''' </returns>
    Public Function HasTemporaryId() As Boolean
        Return Me.Id < 0
    End Function

#End Region

#Region " SingleExecutionMonitor class "

    ''' <summary>
    ''' Utility class to ensure that a task is only running once at any given
    ''' time - an exception is thrown by this class if a task is executed while
    ''' it is running within another thread.
    ''' </summary>
    ''' <remarks>
    ''' This should be placed as the subject of a using block - eg. <code>
    ''' 
    ''' Using monitor As New SingleExecutionMonitor(Me)
    ''' 
    '''   ' Do work...
    ''' 
    ''' End Using
    ''' 
    ''' </code>
    ''' </remarks>
    Private Class SingleExecutionMonitor
        Implements IDisposable

        ' The dictionary of locks for tasks - keyed on the task ID,
        ' which, when executing, should be set and non-temporary
        Private Shared Locks As New Dictionary(Of Integer, Object)

        ' The task that this monitor is locking on
        Private mTask As ScheduledTask

        ''' <summary>
        ''' Creates a new monitor locking on the given task. This will also
        ''' actually enact the locking mechanism, only released when this
        ''' instance is disposed of.
        ''' </summary>
        ''' <param name="task">The task which should be locked for single
        ''' execution.</param>
        ''' <exception cref="AlreadyRunningException">If a lock could not be
        ''' entered for this task, meaning that it is already locked by a
        ''' monitor object in another thread.</exception>
        Public Sub New(ByVal task As ScheduledTask)

            ' While messing with the dictionary, ensure a lock on that or
            ' 2 threads might end up locking the same task on different
            ' objects, which kinda ruins the entire point of this class
            Dim tasklock As Object
            SyncLock Locks
                If Not Locks.ContainsKey(task.Id) Then
                    Locks(task.Id) = New Object()
                End If
                tasklock = Locks(task.Id)
            End SyncLock

            If Not Monitor.TryEnter(tasklock, 1) Then
                Throw New AlreadyRunningException(
                 String.Format(My.Resources.SingleExecutionMonitor_Task0IsAlreadyRunning, task.Name))
            End If

            mTask = task

        End Sub

        ''' <summary>
        ''' Exits the monitor on the task within this object.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            Monitor.Exit(Locks(mTask.Id))
        End Sub

    End Class

#End Region

#Region " Member variables "

    ' The timeout (in milliseconds) to wait for a resource to become valid.
    Private Const ResourceValidTimeout As Integer = 60 * 1000

    ' The timeout (in milliseconds) to wait for *all* sessions to be created
    Private Const CreateAllSessionsTimeout As Integer = 10 * 60 * 1000

    ' Lock object used when monitoring the session creation
    ' I'd like this to be readonly, but it needs to be a different object when
    ' the task is Clone()'d. 
    Private SESSION_LOCK As New Object()

    ' The sessions which this task is responsible for running
    <DataMember>
    Private mSessions As IList(Of ISession)

    ' The ID of this task
    <DataMember>
    Private mId As Integer

    ' The task to perform after successful execution of this task
    <DataMember>
    Private mOnSuccess As ScheduledTask

    ' The task to perform after failed execution of this task
    <DataMember>
    Private mOnFailure As ScheduledTask

    ' The schedule which owns this task
    <DataMember>
    Private mOwner As SessionRunnerSchedule

    ' Flag indicating that this task should fail on any error when it is executing sessions
    <DataMember>
    Private mFailOnAnyError As Boolean

#End Region

#Region " Properties "

    ''' <summary>
    ''' The id of this task
    ''' </summary>
    Public Property Id() As Integer
        Get
            If mId = 0 Then
                ' no ID assigned yet..
                ' Get a temporary one until we have one assigned.
                AssignTemporaryId(Me)
            End If
            Return mId
        End Get
        Set(ByVal value As Integer)
            If mId <> value Then
                MarkDataChanged("Id", mId, value)
                mId = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' The task to perform after this task has successfully completed
    ''' </summary>
    Public Property OnSuccess() As ScheduledTask
        Get
            Return mOnSuccess
        End Get
        Set(ByVal value As ScheduledTask)
            If Not Object.ReferenceEquals(mOnSuccess, value) Then
                MarkDataChanged("OnSuccess", mOnSuccess, value)
                mOnSuccess = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' The task to perform after this task has failed.
    ''' </summary>
    Public Property OnFailure() As ScheduledTask
        Get
            Return mOnFailure
        End Get
        Set(ByVal value As ScheduledTask)
            If Not Object.ReferenceEquals(mOnFailure, value) Then
                MarkDataChanged("OnFailure", mOnFailure, value)
                mOnFailure = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' The owner of this task
    ''' </summary>
    Public Property Owner() As SessionRunnerSchedule
        Get
            Return mOwner
        End Get
        Set(ByVal value As SessionRunnerSchedule)
            If Not Object.ReferenceEquals(mOwner, value) Then
                MarkDataChanged("Owner", mOwner, value)
                mOwner = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' The collection of sessions that this task will initiate
    ''' when it is executed.
    ''' </summary>
    Public ReadOnly Property Sessions() As ICollection(Of ISession)
        Get
            Return GetReadOnly.ICollection(mSessions)
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the flag indicating whether all errors occurring in the
    ''' communication with resources / creation and running of sessions should
    ''' be regarded as a task failure. By default, any errors will instantly
    ''' cause the task to fail and clean up any sessions which have been created
    ''' already. If set to false, this will cause the task to ignore any errors
    ''' and always indicate success.
    ''' </summary>
    Public Property FailFastOnError() As Boolean
        Get
            Return mFailOnAnyError
        End Get
        Set(ByVal value As Boolean)
            mFailOnAnyError = value
        End Set
    End Property

    <DataMember>
    Public Property DelayAfterEnd As Integer

#End Region

#Region " Logging "

    ''' <summary>
    ''' Logs the given info message to the event log
    ''' </summary>
    ''' <param name="msg">The message which forms the event log entry with
    ''' formatting placeholders</param>
    ''' <param name="args">The arguments to use for the placeholders in the message.
    ''' </param>
    Private Sub LogInfo(msg As String, ParamArray args() As Object)
        Log.Info(Function() $"Schedule '{mOwner.Name}' - Task '{Name}' - {String.Format(msg, args)}")
    End Sub

#End Region

#Region " Object overloads "

    ''' <summary>
    ''' Gets the string representation of this task.
    ''' </summary>
    ''' <returns>The task's ID and name</returns>
    Public Overrides Function ToString() As String
        Return Name
    End Function

    ''' <summary>
    ''' Checks if this task is equal to the given object or not.
    ''' </summary>
    ''' <param name="obj">The object to check for equality.
    ''' </param>
    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Dim t As ScheduledTask = TryCast(obj, ScheduledTask)
        Return (t IsNot Nothing AndAlso t.Id = Me.Id AndAlso t.Name = Me.Name)
    End Function

    ''' <summary>
    ''' Gets a hashcode for this task.
    ''' </summary>
    ''' <returns>An integer hash for this object.</returns>
    Public Overrides Function GetHashCode() As Integer
        Return Id
    End Function

#End Region

#Region " Constructor & Populate method "

    ''' <summary>
    ''' Creates a new orphaned task - ie. a task with no owner.
    ''' </summary>
    Public Sub New()
        mSessions = New List(Of ISession)
        mFailOnAnyError = True
        ResetChanged()
    End Sub

    Public Sub Populate(taskData As ScheduleTaskDatabaseData)
        Me.Id = taskData.Id
        Me.Name = taskData.Name
        Me.Description = taskData.Description
        Me.FailFastOnError = taskData.FailFastOnError
        Me.DelayAfterEnd = taskData.DelayAfterEnd
        Dim taskId As Integer = taskData.OnSuccess
        If taskId <> 0 Then Me.OnSuccess = Owner.GetOrCreateTaskWithId(taskId)
        taskId = taskData.OnFailure
        If taskId <> 0 Then Me.OnFailure = Owner.GetOrCreateTaskWithId(taskId)
    End Sub

#End Region

#Region " Session Collection Manipulation "

    ''' <summary>
    ''' Adds the given session to this task
    ''' </summary>
    ''' <param name="sess">The session to add to this task</param>
    Public Sub AddSession(ByVal sess As ScheduledSession)
        Log.Debug($"Adding session {sess.SessionId} from resource {sess.ResourceId}")
        mSessions.Add(sess)
        MarkDataChanged("SessionAdded", Nothing, sess)
    End Sub

    ''' <summary>
    ''' Removes the given session from this task
    ''' </summary>
    ''' <param name="sess">The session which should be removed.</param>
    Public Sub RemoveSession(ByVal sess As ScheduledSession)
        Log.Debug($"Remove session {sess.SessionId} from resource {sess.ResourceId}")
        mSessions.Remove(sess)
        MarkDataChanged("SessionRemoved", sess, Nothing)
    End Sub

    ''' <summary>
    ''' Clears the sessions currently held in this task.
    ''' </summary>
    Public Sub ClearSessions()
        Log.Debug($"Clearing all sessions")
        For Each sess As ScheduledSession In Sessions
            MarkDataChanged("SessionRemoved", sess, Nothing)
        Next
        mSessions.Clear()
    End Sub

#End Region

#Region " Session Management, Execution & Termination "

    ''' <summary>
    ''' Handles the session being created within the resource connection manager.
    ''' This ensures that the ScheduledSession object being held by this task is
    ''' updated with the ID of its corresponding session.
    ''' </summary>
    Private Sub HandleSessionCreated(
     sender As Object, ByVal e As SessionCreateEventArgs)

        ' If this isn't a session created from a scheduled session, we're not
        ' interested
        If e.ScheduledSessionId = 0 Then Return

        ' Find the session in this task with the scheduled session ID
        Dim sess = Sessions.OfType(Of ScheduledSession).FirstOrDefault(
            Function(s) s.Id = e.ScheduledSessionId)

        ' If it's not there, then we're not interested
        If sess Is Nothing Then Return
        Log.Debug($"Session {e.SessionId} created, for {e.ResourceId}")

        sess.SessionId = e.SessionId

        ' Check if it failed. 'ErrorMessage' will hold the reason.
        If e.State <> SessionCreateState.Created Then
            sess.ErrorMessage = e.ErrorMessage
            sess.Data = e.Data
            sess.SessionId = Guid.Empty
        End If

        ' Tell the controlling thread that a session has been created (or 
        ' has failed to be so) - allows it to check whether all have been
        ' successfully created yet.
        SyncLock SESSION_LOCK
            Monitor.PulseAll(SESSION_LOCK)
        End SyncLock

    End Sub

    ''' <summary>
    ''' Clears the session logs set in the sessions held within this task.
    ''' </summary>
    Private Sub ClearAllSessionData()
        ' Clear any sessions held at the moment.
        For Each sess As ScheduledSession In Sessions
            sess.ClearSessionData()
        Next
    End Sub

    ''' <summary>
    ''' Checks to see if all the sessions in this task have session
    ''' logs created for them.
    ''' </summary>
    ''' <returns>True if all session logs have been created; False if any
    ''' sessions do not have any logs assigned to them.</returns>
    ''' <exception cref="ScheduledSessionCreationFailedException">If any of the
    ''' sessions currently held in this task have an error message set in them.
    ''' </exception>
    Private Function AreAllSessionLogsCreated(
     ByVal sessionsToCheck As ICollection(Of ISession)) As Boolean
        Dim allCreated As Boolean = True
       
        For Each sess As ScheduledSession In sessionsToCheck
            Dim hasSessionNumber = sess.HasSessionNumber()
            allCreated = allCreated AndAlso hasSessionNumber

            If sess.HasErrorMessage Then _
             Throw New ScheduledSessionCreationFailedException(sess)
        Next

        Return allCreated
    End Function

    ''' <summary>	
    ''' Connects to the resource specified in the given scheduled session object	
    ''' using the given manager.	
    ''' </summary>	
    ''' <param name="manager">The resource connection manager to use to connect	
    ''' to the required resource</param>	
    ''' <param name="sess">The session containing the resource to which a	
    ''' connection should be made</param>	
    ''' <exception cref="ScheduleException">If the resource could not be	
    ''' connected to for some reason. The message will indicate the reason.	
    ''' </exception>	
    Private Sub ConnectToResource(
     ByVal manager As IResourceConnectionManager, ByVal sess As ScheduledSession)
        ' Attempt to connect to the resource. Retry up to OfflineResourceRetries times.	
        Dim connected As Boolean = False
        ' Get the number of retries and period between retries from the prefs.	
        Dim maxRetries As Integer = SchedulerConfig.RetryTimes
        Dim retryAfterSecs As Integer = SchedulerConfig.RetryPeriod
        ' The number of retries left before we give up	
        Dim remainingRetries As Integer = maxRetries
        While Not connected AndAlso remainingRetries > 0
            ' Make sure the manager is using the latest resource info 	
            manager.GetLatestDBResourceInfo(ResourceAttribute.Retired)
            Dim resource As IResourceMachine = manager.GetResource(sess.ResourceId)
            If resource Is Nothing Then
                Log.Error("The Resource {resource} cannot be found within the connection manager", New With {resource})
                Throw New ScheduleException("The resource {resource} cannot be found within the connection manager")
            End If
            Dim state As ResourceConnectionState = Nothing
            ' Wait no more than ResourceValidTimeout seconds for a valid connection...	
            connected = resource.AwaitValidConnection(ResourceValidTimeout, False, state)
            If Not connected Then
                LogInfo("Not connected to {0} ({1}); State:{2}; Retries: {3}",
                        sess.ResourceName, sess.ResourceId, state, remainingRetries)
                ' If the resultant state is still 'connecting' then we hit the timeout.	
                If state = ResourceConnectionState.Connecting Then
                    Throw New ScheduleException(
                     My.Resources.ScheduledTask_Resource0TimedOutWaitingForValidConnection,
                     sess.ResourceName)
                ElseIf state = ResourceConnectionState.Offline Then
                    ' Offline, keep trying to see if we can get a connection	
                    Thread.Sleep(retryAfterSecs * 1000)
                    remainingRetries -= 1
                    If remainingRetries = 0 Then
                        Throw New ScheduleException(
                         My.Resources.ScheduledTask_Resource0IsOfflineRetried1Times,
                         sess.ResourceName, maxRetries)
                    End If
                Else ' Otherwise, the resource is unavailable for some reason.	
                    If remainingRetries = 10 Then
                        Throw New ScheduleException(
                         My.Resources.ScheduledTask_Resource0WasUnavailableTheStateGivenWas1,
                         sess.ResourceName, state)
                    Else
                        Dim retryCount As Integer = maxRetries - remainingRetries
                        Throw New ScheduleException(
                         LTools.Format(My.Resources.ScheduledTask_ResourceNAMEWasUnavailableAfterCOUNTPluralOne1RetryOtherRetries, "NAME", sess.ResourceName, "COUNT", retryCount) &
                         String.Format(My.Resources.ScheduledTask_TheFinalStateGivenWas0, state)
                        )
                    End If
                End If
            End If
        End While
    End Sub


    ''' <summary>
    ''' Creates the sessions required by this task and prepares them within
    ''' the returned resource connection manager.
    ''' </summary>
    ''' <param name="manager">The manager to use to talk to the resources and
    ''' create the relevant sessions</param>
    ''' <param name="timeoutMillis">The timeout in milliseconds for the creation
    ''' of all sessions. If this is a positive integer, it will return on the
    ''' first check after the timeout period expires.</param>
    ''' <returns>A collection of scheduled sessions for which the resource was
    ''' connected to and the session was created. If FailOnError is true, this
    ''' will always be all the sessions in the task, otherwise, it will be the
    ''' sessions which were created successfully - the failed ones will have been
    ''' logged and discarded.</returns>
    ''' <exception cref="ScheduleAbortException">If the schedule is marked as
    ''' being aborted while the sessions were being created</exception>
    ''' <exception cref="ScheduleException">If any errors occur attempting to
    ''' connect to the resources or create the sessions and the
    ''' <see cref="FailFastOnError"/> property is set to True in this task
    ''' </exception>
    Private Function CreateSessions(ByVal manager As IResourceConnectionManager,
     ByVal timeoutMillis As Integer) As ICollection(Of ISession)



        ' The list of successful sessions that we build up as we go along
        Dim successfulSessions As New List(Of ISession)

        ' Add the handler which monitors the sessions to make sure they
        ' have been created / raise an alert if they've failed.
        AddHandler manager.SessionCreate, AddressOf HandleSessionCreated

        Dim sw As New Stopwatch()
        sw.Start()

        Try

            ' Basically, we go through each of the scheduled sessions and send
            ' a 'create' message using the provided connection manager... then
            ' we wait for responses to come in until they are all created (or
            ' any have failed creation for any reason)
            Dim errmsg As String = Nothing
            SyncLock SESSION_LOCK

                'Check all the connections are valid
                Dim abortReason = CheckConnections(manager, successfulSessions)

                If successfulSessions.Count = 0 Then Return successfulSessions
                Log.Debug($"Checked connections for task with {Sessions.Count} sessions, found {successfulSessions.Count} valid connections")

                ' Now we've verified that they are all online, create the
                ' sessions on them.
                For Each sess As ScheduledSession In New List(Of ISession)(successfulSessions)
                    Try
                        Dim session As New CreateSessionData(sess.ResourceId, sess.ProcessId, Nothing, Nothing, Nothing) With {
                                .ScheduledSessionId = sess.Id
                                }
                        sess.SessionId = manager.SendCreateSession({session}).First()
                        Log.Debug($"ScheduledTask: Creating session {sess.SessionId}")

                    Catch ex As Exception
                        LogInfo("Failed to create session for {0}: {1}", sess, ex.Message)

                        ' Set the error message in the session
                        sess.ErrorMessage = String.Format(
                                    My.Resources.ScheduledTask_CouldNotCreateSessionFor0Message1, sess, ex.Message)

                        ' If FailOnError is set, immediately exit the method
                        If FailFastOnError Then Throw New ScheduleException(sess.ErrorMessage)

                        ' Otherwise, remove the session from our collection of successful sessions
                        successfulSessions.Remove(sess)
                    End Try
                Next

                ' This thread is 'Pulsed' when a session creation is handled,
                ' so check if the creation means all session logs (that we're
                ' interested in) have been created or not yet.
                While True
                    Log.Debug("ScheduledTask:Waiting for resources to report created")
                    Try
                        If AreAllSessionLogsCreated(successfulSessions) Then
                            Log.Debug("ScheduledTask: all sessions created")
                            Exit While
                        End If

                    Catch scfe As ScheduledSessionCreationFailedException
                        LogInfo("Exception when checking all session logs created: {0}",
                                scfe)
                        If FailFastOnError Then Throw
                        ' Else, remove it from our successful sessions and carry on
                        successfulSessions.Remove(scfe.Session)

                    End Try
                    If mOwner.IsAborting(abortReason) Then Throw New ScheduleAbortException(
                     My.Resources.ScheduledTask_TaskAborted0, abortReason)


                    Monitor.Wait(SESSION_LOCK, 5000)
                    If timeoutMillis > 0 AndAlso sw.ElapsedMilliseconds > timeoutMillis Then
                        Log.Debug("Timed out waiting for resources to report back")

                        If successfulSessions IsNot Nothing Then
                            For Each filteredSession As ScheduledSession In successfulSessions.Where(Function(x) Not CType(x, ScheduledSession).HasSessionNumber).ToList() 
                                Log.Debug($"ScheduledTask: Session {filteredSession.SessionId} SessionNumber = {filteredSession.HasSessionNumber} ")
                            Next
                        End If

                        Throw New ScheduleException(My.Resources.ScheduledTask_TimedOutWaitingForAllSessionsToStart)
                    End If


                End While

            End SyncLock

        Finally

            ' My work is done here....
            RemoveHandler manager.SessionCreate, AddressOf HandleSessionCreated

        End Try

        Return successfulSessions

    End Function

    Private Function CheckConnections(manager As IResourceConnectionManager, successfulSessions As List(Of ISession)) As String
        Dim abortReason As String = Nothing

        ' Resource info on 30s timer, robot status may have changed inbetween. Call function directly to be sure.
        manager.GetLatestDBResourceInfo()
        For Each sess As ScheduledSession In Sessions
            sess.ClearSessionData()
            ' If the schedule is aborting, we just straight abort here -
            ' no recording of error message, just leave
            If mOwner.IsAborting(abortReason) Then Throw New ScheduleAbortException(My.Resources.ScheduledTask_TaskAborted0, abortReason)

            Dim resource = manager?.GetResource(sess.ResourceName)
            If resource Is Nothing OrElse resource.DBStatus = ResourceMachine.ResourceDBStatus.Offline Then
                sess.ErrorMessage = "Resource is offline, cannot be used by the scheduler"

                If FailFastOnError Then
                    Log.Debug($"Resource {sess.ResourceName} onffline,  for scheduletask, failing fast ")
                    Throw New ResourceUnavailableException($"Resource {sess.ResourceName} is recorded as being offline")
                Else
                    Log.Debug($"Resource {sess.ResourceName} offline,  for scheduletask ")
                End If
            Else
                Dim useAscr = gSv.GetPref(PreferenceNames.SystemSettings.UseAppServerConnections, False)
                If useAscr Then
                    ' If using ascr, just assume it's ok!. We will know when we connect.
                    If resource.DBStatus = ResourceMachine.ResourceDBStatus.Ready Then
                        successfulSessions.Add(sess)
                        Log.Debug($"Resource {resource.Name} online, ok for scheduletask ")
                    End If
                Else
                    Try
                        ' Connect to the resource, adding it to our list of
                        ' successful sessions if the connect works correctly
                        ConnectToResource(manager, sess)
                        successfulSessions.Add(sess)
                    Catch ex As Exception
                        LogInfo("Failed to connect to {0}: {1}", sess.ResourceName, ex)
                        ' Set the error message in the session - rethrow the
                        ' exception if FailOnError is set for this task
                        sess.ErrorMessage = ex.Message
                        If FailFastOnError Then Throw

                    End Try

                    If mOwner.IsAborting(abortReason) Then Throw New ScheduleAbortException(
                        My.Resources.ScheduledTask_TaskAborted0, abortReason)

                End If
            End If
        Next

        Return abortReason
    End Function

    ''' <summary>
    ''' Event Handler for the session status changing within the Resource
    ''' Connection Manager which is handling connections to the resource PCs
    ''' running the sessions within this task.
    ''' 
    ''' This just ensures that the executing thread is informed of the change
    ''' by notifying it over the SESSION_LOCK object.
    ''' </summary>
    Private Sub HandleStatusChanged(
     sender As Object, ByVal e As ResourcesChangedEventArgs)
        SyncLock SESSION_LOCK
            Monitor.PulseAll(SESSION_LOCK)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Coalesces the session state of the sessions identified by the keys in
    ''' the given dictionary into a single session status which indicates the
    ''' status of this task. The status of the task is worked out differently,
    ''' depending on the current value of the <see cref="FailFastOnError"/>
    ''' property.
    ''' All the current individual statuses of the sessions are returned in
    ''' the dictionary parameter.
    ''' </summary>
    ''' <param name="dict">The dictionary whose keys contain the IDs of the
    ''' sessions whose statuses should be coalesced. This will have the
    ''' dictionary's entries populated with the latest status for each
    ''' session on exiting of this method.</param>
    ''' <returns>The effective status of this task as coalesced from the
    ''' current statuses of its sessions. This means that, if 
    ''' <see cref="FailFastOnError"/> is set to True and :-<list>
    ''' <item><em>any</em> of the sessions have failed or stopped, this
    ''' will return <see cref="SessionStatus.Terminated"/></item>
    ''' <item><em>any</em> of the sessions are pending or running, this
    ''' will return <see cref="SessionStatus.Running"/></item>
    ''' <item><em>all</em> of the sessions are completed, this will return
    ''' <see cref="SessionStatus.Completed"/></item>
    ''' </list>
    ''' If <see cref="FailFastOnError" /> is set to False, the coalescing works
    ''' slightly differently. If :-<list>
    ''' <item><em>any</em> of the sessions are pending or running, this
    ''' will return <see cref="SessionStatus.Running"/>, even if other
    ''' sessions have failed</item>
    ''' <item><em>all</em> of the sessions have failed or stopped, this
    ''' will return <see cref="SessionStatus.Terminated"/></item>
    ''' <item><em>any</em> of the sessions are completed, this will return
    ''' <see cref="SessionStatus.Completed"/></item>
    ''' </list>
    ''' Note that, in either case, any other status than those mentioned
    ''' above will cause an exception to be thrown.
    ''' </returns>
    ''' <exception cref="InvalidDataException">If the status of one of the
    ''' sessions specified was not one of :-<list>
    ''' <item><see cref="SessionStatus.Terminated"/></item>
    ''' <item><see cref="SessionStatus.Stopped"/></item>
    ''' <item><see cref="SessionStatus.Completed"/></item>
    ''' <item><see cref="SessionStatus.Pending"/></item>
    ''' <item><see cref="SessionStatus.Running"/></item>
    ''' <item><see cref="SessionStatus.StopRequested"/></item>
    ''' </list>
    ''' </exception>
    ''' <remarks>Note that this will never return a session status of
    ''' <see cref="SessionStatus.StopRequested"/>, even if all sessions have such a
    ''' status - they are treated as a <see cref="SessionStatus.Running"/> status.
    ''' </remarks>
    Private Function CoalesceSessionStatus(
     ByRef dict As IDictionary(Of Guid, SessionStatus)) As SessionStatus

        dict = gSv.GetSessionStatus(dict)

        ' The coalescing status, differs depending on current FailOnError value
        Dim status As SessionStatus

        If FailFastOnError Then
            ' Assume completed. This will be changed to 'Running' if any running
            ' or pending sessions are found. If any aborted sessions are found,
            ' the method breaks out and returns.
            ' So: 'Failed' trumps 'Running' and 'Running' trumps 'Completed'
            status = SessionStatus.Completed
        Else
            ' Assume failed. This will be changed to 'Completed' if any completed
            ' sessions are found. If any running or pending sessions are found,
            ' the method breaks out and returns
            ' So: 'Running' trumps 'Completed' and 'Completed' trumps 'Failed'
            status = SessionStatus.Terminated

        End If
        ' Keeps track of failure counts - disregarded if FailOnError is true,
        ' otherwise checked to see if all sessions failed
        Dim failCount As Integer = 0

        For Each entry As KeyValuePair(Of Guid, SessionStatus) In dict

            Select Case entry.Value

                Case SessionStatus.Terminated, SessionStatus.Stopped
                    ' Any tasks failed or stopped, mean this task has failed...
                    ' Might as well stop now - this is the ace of spades of
                    ' session statuses.
                    If FailFastOnError Then Return SessionStatus.Terminated

                    ' Otherwise, just carry on - that's our assumed default anyway
                    ' if we're not failing on error

                Case SessionStatus.Pending, SessionStatus.Running,
                    SessionStatus.StopRequested
                    ' If we're not failing on error and anything is still
                    ' running / pending, then we can't possibly have failed (yet)
                    If Not FailFastOnError Then Return SessionStatus.Running

                    ' any pending or running mean this task is still running...
                    ' override the assumed 'completed' status
                    status = SessionStatus.Running

                Case SessionStatus.Completed
                    ' If we're not failing on error, update the status to show
                    ' that (at least) one session has completed, meaning that
                    ' no matter what the coalesced status is not 'Failed'
                    If Not FailFastOnError Then status = SessionStatus.Completed

                    ' If we're failing on error, we just carry on - completed is
                    ' our assumed default already


                Case Else ' "Debugging" / "All" / other - invalid in scheduler
                    Throw New InvalidDataException(String.Format(
                     My.Resources.ScheduledTask_InvalidSessionStateForSession01, entry.Key, entry.Value))

            End Select

        Next entry

        Return status

    End Function

    ''' <summary>
    ''' Checks if all sessions defined in the given map are finished or not.
    ''' Finished in this sense means completed, stopped or failed -  anything
    ''' else implies that the session is still ongoing.
    ''' The specific statuses of the sessions are returned in the 
    ''' <paramref name="dict"/> parameter
    ''' </summary>
    ''' <param name="dict">The dictionary mapping session IDs onto their
    ''' respective statuses.</param>
    ''' <returns>True if all sessions defined by the IDs in the keys of the
    ''' dictionary are 'finished', ie. completed, stopped or failed; False if
    ''' any of the sessions have any other status.</returns>
    Private Function AreAllSessionsFinished(ByRef dict As IDictionary(Of Guid, SessionStatus)) _
     As Boolean

        dict = gSv.GetSessionStatus(dict)

        For Each status As SessionStatus In dict.Values
            Select Case status
                Case SessionStatus.Completed, SessionStatus.Terminated, SessionStatus.Stopped
                    Continue For
                Case Else
                    Return False
            End Select
        Next

        Return True

    End Function

    ''' <summary>
    ''' Generates a log entry of the specified type for this task and the 
    ''' given session
    ''' </summary>
    ''' <param name="entryType">The type of log entry required.</param>
    ''' <param name="sess">The session to create an entry for</param>
    ''' <returns>An initialised log entry for the required type, associated
    ''' with this task and the provided session.</returns>
    Private Function GenEntry(
     ByVal entryType As ScheduleLogEventType, ByVal sess As ScheduledSession) As ScheduleLogEntry
        Return GenEntry(entryType, sess, Nothing)
    End Function

    ''' <summary>
    ''' Generates a log entry of the specified type for this task and the 
    ''' given session - with the termination reason provided.
    ''' </summary>
    ''' <param name="entryType">The type of log entry required.</param>
    ''' <param name="sess">The session to create an entry for</param>
    ''' <param name="reason">The reason for the termination.</param>
    ''' <returns>An initialised log entry for the required type, associated
    ''' with this task and the provided session.</returns>
    Private Function GenEntry(
     ByVal entryType As ScheduleLogEventType, ByVal sess As ScheduledSession, ByVal reason As String) _
     As ScheduleLogEntry

        Return New ScheduleLogEntry(entryType, Me.Id, sess.SessionNumber, reason)

    End Function

    ''' <summary>
    ''' Executes this task.
    ''' This creates, starts and monitors the sessions defined in this task
    ''' </summary>
    ''' <param name="log">The logger to use to record progress of the execution
    ''' of this task.</param>
    ''' <exception cref="AlreadyRunningException">If another thread is executing
    ''' this task when this method is called.</exception>
    Public Function Execute(ByVal log As ISessionRunnerScheduleLogger) As TaskResult
        Try
            Return InternalExecute(log)

        Finally
            ' Ensure that any hanging session data is deleted once the task
            ' has finished executing.
            ClearAllSessionData()
        End Try
    End Function

    ''' <summary>
    ''' Executes this task.
    ''' This creates, starts and monitors the sessions defined in this task
    ''' </summary>
    ''' <param name="log">The logger to use to record progress of the execution
    ''' of this task.</param>
    ''' <exception cref="AlreadyRunningException">If another thread is executing
    ''' this task when this method is called.</exception>
    Private Function InternalExecute(ByVal log As ISessionRunnerScheduleLogger) As TaskResult

        ' Only allow execution of a task once at any one time
        Using executionMonitor As New SingleExecutionMonitor(Me)

            Dim errmsg As String = Nothing
            Dim statuses As IDictionary(Of Guid, SessionStatus) =
             New Dictionary(Of Guid, SessionStatus)

            ' Keep a set of the started sessions - this is here so that we can
            ' delete any sessions which weren't started when the task terminated.
            Dim started As IBPSet(Of Guid) = New clsSet(Of Guid)

            ' Keep a Set of the 'finished' session IDs - ie. those sessions which
            ' have been logged as Completed or Terminated already, so that a
            ' session isn't logged as finished twice within the same task.
            Dim finished As IBPSet(Of Guid) = New clsSet(Of Guid)

            ' Create the sessions first - we want to ensure that the connections
            ' are valid and available, and the resources are capable of running
            ' the sessions before we start executing anything.
            Dim manager As IResourceConnectionManager = Me.Owner.ResourceConnectionManager

            ' Create all the sessions first - before we set any off, make sure
            ' that all session can be created.
            Dim successfulSessions As ICollection(Of ISession)
            Try
                successfulSessions = CreateSessions(manager, CreateAllSessionsTimeout)

            Catch ex As Exception
                LogInfo(
                 "Error creating sessions - terminating remaining sessions: {0}", ex)
                ' Ensure any created sessions are deleted.
                ' Note that if FailOnError is false, CreateSessions() knows about it,
                ' so it will only throw an exception in the case of a fatal error.
                TerminateRemainingSessions(manager, started, finished, log,
                 My.Resources.ScheduledTask_SessionCreationFailed)
                Throw

            End Try

            ' If we've got nothing, that's pretty much the only thing that counts
            ' as a task failure, even if 'FailOnError' is set to false
            If successfulSessions.Count = 0 Then
                ScheduledTask.Log.Debug($"ScheduledTask: Created {successfulSessions.Count} sessions")
                For Each sess As ScheduledSession In Sessions
                    log.LogEvent(New ScheduleLogEntry(
                     ScheduleLogEventType.SessionFailedToStart,
                     Me.Id, 0, sess.ErrorMessage))
                Next
                Throw New ScheduleException(
                 My.Resources.ScheduledTask_NoSessionsWereSuccessfullyCreated)
            End If

            ' Log any failed sessions
            Dim failedSessions As New clsSet(Of ISession)(Sessions)
            failedSessions.Subtract(successfulSessions)
            ScheduledTask.Log.Debug($"ScheduledTask: Failed to start {failedSessions.Count} sessions")
            For Each sess As ScheduledSession In failedSessions
                LogInfo("Session '{0}' failed to start: {1}", sess, sess.ErrorMessage)
                log.LogEvent(
                 GenEntry(ScheduleLogEventType.SessionFailedToStart, sess, sess.ErrorMessage))
            Next

            ' We want to listen for status changes on our resources.
            AddHandler manager.ResourceStatusChanged, AddressOf HandleStatusChanged

            ' Set them going...
            SyncLock SESSION_LOCK

                Dim abortReason As String = Nothing

                For Each sess As ScheduledSession In New List(Of ISession)(successfulSessions)
                    If mOwner.IsAborting(abortReason) Then Throw New ScheduleAbortException(
                     My.Resources.ScheduledTask_TaskAborted0, abortReason)

                    log.LogEvent(GenEntry(ScheduleLogEventType.SessionStarted, sess))
                    Try
                        manager.SendStartSession({New StartSessionData(sess.ResourceId, sess.ProcessId, sess.SessionId, sess.ArgumentsXML, Nothing, Nothing, sess.Id)})
                        ScheduledTask.Log.Debug($"ScheduledTask: Starting session {sess.SessionId}")
                        started.Add(sess.SessionId)
                        ' Add an entry to the dictionary for the created session
                        statuses(sess.SessionId) = Nothing
                    Catch ex As Exception
                        Dim msg As String = String.Format(My.Resources.ScheduledTask_CouldNotStartSession01, sess.SessionId, errmsg)


                        log.LogEvent(GenEntry(ScheduleLogEventType.SessionTerminated, sess, msg))

                        If FailFastOnError Then
                            TerminateRemainingSessions(manager, started, finished, log, msg)
                            Throw New ScheduleException(msg)
                        End If
                    End Try
                Next

                ' Now we need to monitor the statuses of all the running sessions.
                ' The created sessions each have an entry in the 'statuses'
                ' dictionary keyed on their session ID.
                Dim status As SessionStatus = CoalesceSessionStatus(statuses)
                While status = SessionStatus.Running
                    ' Now wait for them to finish or error - check that the schedule
                    ' is not being aborted every 5 secs
                    Monitor.Wait(SESSION_LOCK, 5000)
                    If mOwner.IsAborting(abortReason) Then Exit While

                    status = CoalesceSessionStatus(statuses)
                    Debug.Print("Got status: {0}", status)
                    MarkFinishedSessions(statuses, finished, log)
                End While

                If abortReason IsNot Nothing Then
                    TerminateRemainingSessions(manager, started, finished, log,
                     My.Resources.ScheduledTask_TaskAborted & abortReason)
                    Throw New ScheduleAbortException(My.Resources.ScheduledTask_TaskAborted0, abortReason)
                End If

                ' Coalesced status is not running... if it's failed or 'unknown'
                ' then treat it as a failure.
                If status <> SessionStatus.Completed Then
                    Dim msg As String = String.Format(
                     My.Resources.ScheduledTask_StatusOf0FoundTerminatingRemainingSessions, status)
                    TerminateRemainingSessions(manager, started, finished, log, msg)
                    Throw New ScheduleException(msg)
                End If

            End SyncLock

            ' Are we required to stop here for a while?
            If DelayAfterEnd > 0 Then
                Dim timeoutDuration = TimeSpan.FromSeconds(DelayAfterEnd)

                Task.Delay(timeoutDuration).Wait()
            End If

            Return TaskResult.Success

        End Using

    End Function

    ''' <summary>
    ''' Attempts to terminate any sessions in this task which are not already
    ''' marked as finished in the supplied set
    ''' </summary>
    ''' <param name="manager">The resource connection manager through which
    ''' the sessions can be manipulated.</param>
    ''' <param name="started">The IDs of the sessions which have been started.
    ''' </param>
    ''' <param name="finished">The set of session IDs which have already been
    ''' logged as complete or terminated within the execution context of this
    ''' task.</param>
    ''' <param name="log">The logger on which the recording of session statuses
    ''' is handled.</param>
    ''' <param name="reason">The reason for the termination of the sessions.
    ''' </param>
    Private Sub TerminateRemainingSessions(
     ByVal manager As IResourceConnectionManager,
     ByVal started As IBPSet(Of Guid),
     ByVal finished As IBPSet(Of Guid),
     ByVal log As ISessionRunnerScheduleLogger,
     ByVal reason As String)

        For Each session As ScheduledSession In Sessions
            If session.SessionId = Guid.Empty Then Continue For ' No session id set - skip it.

            Dim id As Guid = session.SessionId
            ' If it's already finished, skip it
            If Not finished.Contains(id) Then
                ' If it's not yet been started, just delete it.
                If Not started.Contains(id) Then
                    Try
                        manager.SendDeleteSession({New DeleteSessionData(session.ResourceId, session.ProcessId, session.SessionId, Nothing, session.Id)})
                    Catch ex As Exception
                        LogInfo("Failed to delete session {0}: {1}", session, ex.Message)
                    End Try
                Else
                    Try
                        manager.SendStopSession({New StopSessionData(session.ResourceId, session.SessionId, session.Id)})
                        log.LogEvent(GenEntry(ScheduleLogEventType.SessionTerminated, session, reason))
                        finished.Add(id)
                    Catch ex As Exception
                        Throw New ScheduleException(My.Resources.ScheduledTask_CouldNotStopSession0, session)
                    End Try


                End If

            End If
        Next

    End Sub

    ''' <summary>
    ''' Logs an entry for any sessions which have completed or terminated
    ''' according to the given statuses. This ensures that an entry is only
    ''' written once for each session by adding the 'finished' session IDs to
    ''' the specified <paramref name="finished"/> set.
    ''' </summary>
    ''' <param name="statuses">The statuses for each session mapped to its
    ''' session ID.</param>
    ''' <param name="finished">The set of sessions which have already had a
    ''' log entry written indicating that they have finished.</param>
    ''' <param name="log">The logger to which log entries should be written.
    ''' </param>
    Private Sub MarkFinishedSessions(
     ByVal statuses As IDictionary(Of Guid, SessionStatus),
     ByVal finished As IBPSet(Of Guid),
     ByVal log As ISessionRunnerScheduleLogger)

        For Each sess As ScheduledSession In Sessions
            Dim id As Guid = sess.SessionId
            If finished.Contains(id) Then Continue For ' Skip those that are already handled
            If Not statuses.ContainsKey(id) Then Continue For ' Skip those sessions that were not successfully created

            Select Case statuses(id)

                Case SessionStatus.Terminated
                    log.LogEvent(GenEntry(ScheduleLogEventType.SessionTerminated, sess,
                         String.Format(My.Resources.ScheduledTask_TheSession0Terminated, id.ToString())))
                    finished.Add(id)

                Case SessionStatus.Stopped
                    log.LogEvent(GenEntry(ScheduleLogEventType.SessionTerminated, sess,
                         String.Format(My.Resources.ScheduledTask_TheSession0WasStopped, id.ToString())))
                    finished.Add(id)

                Case SessionStatus.Completed
                    log.LogEvent(GenEntry(ScheduleLogEventType.SessionCompleted, sess))
                    finished.Add(id)

                    ' Otherwise, do nothing... still running.
            End Select
        Next
    End Sub

#End Region

#Region " Copying "

    ''' <summary>
    ''' Gets a copy of this task and its sessions.
    ''' Note that the owning schedule and the onsuccess / onfailure tasks
    ''' are not copied, just the bare task (name, description) and its sessions. 
    ''' </summary>
    ''' <returns>A clone of this task with deep-cloned sessions and shallow
    ''' cloned owner and onsuccess / onfailure tasks.</returns>
    Public Overridable Overloads Function Copy() As ScheduledTask

        Dim task As ScheduledTask = DirectCast(MemberwiseClone(), ScheduledTask)
        task.SESSION_LOCK = New Object()
        task.mSessions = New List(Of ISession)
        For Each sess As ISession In Me.mSessions
            task.mSessions.Add(DirectCast(sess.Clone(), ISession))
        Next

        ' We want to give a separate ID to this one. A temp one to indicate that
        ' it's not yet on the database.
        AssignTemporaryId(task)

        ' We don't copy the owner or onsuccess / onfailure elements.
        ' The former because it creates too much confusion in the GUI / data model,
        ' and the latter purely as a result of the former (having onsuccess point
        ' to a task with a different owner, just doesn't work)
        ' So remove all references :-
        task.mOwner = Nothing
        task.mOnFailure = Nothing
        task.mOnSuccess = Nothing

        Return task

    End Function

    ''' <summary>
    ''' Copies this task to the given schedule, returning the copied task if
    ''' successfully copied.
    ''' </summary>
    ''' <param name="sched">The schedule to which this task should be copied.
    ''' Ignored if this argument is null.</param>
    ''' <returns>The task that was copied from this task to the specified
    ''' schedule or null if the task was not copied for any reason (eg. if the
    ''' given schedule was null).</returns>
    Public Function CopyTo(ByVal sched As SessionRunnerSchedule) As ScheduledTask

        ' Check we're pasting into a schedule
        If sched Is Nothing Then Return Nothing

        Dim clonedTask As ScheduledTask = Copy()

        ' If we're pasting within the same schedule, try and ensure that we
        ' keep the onsuccess and onfailure components of the task.
        If Me.Owner Is sched Then
            clonedTask.OnSuccess = Me.OnSuccess
            clonedTask.OnFailure = Me.OnFailure
        End If

        ' Ensure we have a unique name
        clonedTask.Name = sched.GetUniqueTaskName(clonedTask.Name)

        sched.Add(clonedTask)

        Return clonedTask

    End Function

#End Region

#Region " XML Input/Output "

    ''' <summary>
    ''' Writes this task out to XML via the given writer
    ''' </summary>
    ''' <param name="writer">The XML writer to which this task should be written
    ''' </param>
    Public Sub ToXml(ByVal writer As XmlWriter)
        ToXml(writer, True, True)
    End Sub

    ''' <summary>
    ''' Writes this task out to XML via the given writer
    ''' </summary>
    ''' <param name="writer">The XML writer to which this task should be written
    ''' </param>
    ''' <param name="includeSessions">True to include the session information in the
    ''' task. False to skip sessions</param>
    ''' <param name="includeResources">True to include the resource names that the
    ''' sessions are scheduled to run on; False to just output the process name from
    ''' the session. Note that if <paramref name="includeSessions"/> is false, this
    ''' argument is ignored.</param>
    Public Sub ToXml(ByVal writer As XmlWriter, ByVal includeSessions As Boolean, ByVal includeResources As Boolean)
        writer.WriteStartElement("task")
        writer.WriteAttributeString("name", Me.Name)
        If Owner.InitialTask Is Me Then writer.WriteAttributeString("initial-task", "true")
        writer.WriteAttributeString("failfastonerror", XmlConvert.ToString(FailFastOnError))
        writer.WriteAttributeString("delayafterend", XmlConvert.ToString(DelayAfterEnd))

        writer.WriteElementString("description", Me.Description)
        If mOnSuccess IsNot Nothing Then writer.WriteElementString("onsuccess", mOnSuccess.Name)
        If mOnFailure IsNot Nothing Then writer.WriteElementString("onfailure", mOnFailure.Name)
        If includeSessions Then
            writer.WriteStartElement("sessions")
            For Each sess As ScheduledSession In Me.Sessions
                writer.WriteStartElement("session")
                writer.WriteAttributeString("process-name", sess.ProcessName)
                If includeResources Then
                    writer.WriteAttributeString("resource-name", sess.ResourceName)
                End If
                writer.WriteEndElement()
            Next
            writer.WriteEndElement() 'sessions
        End If

        writer.WriteEndElement() 'task
    End Sub

    ''' <summary>
    ''' Loads this task with data from the given XML reader, returning whether the
    ''' task loaded was configured as the initial task or not.
    ''' </summary>
    ''' <param name="r">The XML reader from which to draw the task data. This should
    ''' be positioned immediately before a task element.</param>
    ''' <returns>True if the task loaded from the reader was the initial task for
    ''' the current schedule; False if it was not marked as such.</returns>
    ''' <remarks>Note that this ignores any session information from the reader - it
    ''' only loads name, description, onsuccess, onfailure and whether the task is
    ''' the initial task or not.</remarks>
    Public Function FromXml(ByVal r As XmlReader) As Boolean

        r.MoveToContent()

        If r.NodeType <> XmlNodeType.Element OrElse r.LocalName <> "task" Then
            Throw New BluePrismException(
             My.Resources.ScheduledTask_CannotReadATaskFromANodeOfType0Named1,
             r.NodeType, r.LocalName)
        End If

        Me.Name = r("name")
        Dim initTask As Boolean = (r("initial-task") IsNot Nothing)

        ' Fail Fast - if not present, assume True, otherwise parse the present value.
        Dim failValue As String = r("failfastonerror")
        mFailOnAnyError = (failValue Is Nothing OrElse XmlConvert.ToBoolean(failValue))

        Dim delayafterend As String = r("delayafterend")
        Me.DelayAfterEnd = If(delayafterend Is Nothing, 0, XmlConvert.ToInt32(delayafterend))

        While r.Read()
            If r.NodeType = XmlNodeType.Element Then
                Select Case r.LocalName
                    Case "description"
                        If r.IsEmptyElement Then
                            Me.Description = ""
                        Else
                            r.Read()
                            Me.Description = r.Value
                        End If

                    Case "onsuccess"
                        r.Read()
                        Dim t As New ScheduledTask()
                        t.Name = r.Value
                        mOnSuccess = t

                    Case "onfailure"
                        r.Read()
                        Dim t As New ScheduledTask()
                        t.Name = r.Value
                        mOnFailure = t

                    Case "sessions"
                        Using subTree = r.ReadSubtree()
                            Const CurrentUserCanSeeSession As Boolean = False
                            Const CurrentUserCanSeeResource As Boolean = False
                            While subTree.Read
                                If subTree.NodeType = XmlNodeType.Element AndAlso subTree.LocalName = "session" Then
                                    Dim processName As String = r("process-name")
                                    Dim resourceName As String = r("resource-name")
                                    Dim processId As Guid = gSv.GetProcessIDByName(processName)
                                    Dim resourceId As Guid = gSv.GetResourceId(resourceName)
                                    If processId <> Guid.Empty AndAlso resourceId <> Guid.Empty Then
                                        Dim s As New ScheduledSession(0, processId, resourceName, Guid.Empty,
                                                                      CurrentUserCanSeeSession, CurrentUserCanSeeResource, Nothing)
                                        AddSession(s)
                                    End If
                                End If
                            End While
                        End Using
                End Select
            End If
        End While

        Return initTask

    End Function

#End Region

End Class
