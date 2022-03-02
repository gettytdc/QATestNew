Imports System.Threading
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.ClientServerResources.Core.Enums
Imports BluePrism.Server.Domain.Models

Public MustInherit Class RunnerRecord
    Implements IRunnerRecord
    ' The thread which is running on behalf of this record
    Public mRunnerThread As Thread

    ' The current status of this runner
    Public Status As RunnerStatus

    ' Flag monitored by the owner to detect changed status
    Public mReviewStatus As Boolean

    ' Failure reason, when status is FAILED
    Public mFailReason As String

    'Run mode, Exclusive,Background,Foreground
    Public mRunMode As BusinessObjectRunMode

    'Startup parameters for the session, or Nothing if there are none.
    Public mStartupParams As clsArgumentList

    'Output parameters when completed successfully.
    Public mOutputs As clsArgumentList

    'The XML of the main process to be run. Once the process has been created,
    'prior to actually running it, this is dereferenced.
    Private mProcessXML As String

    'The active process in this runner record - exists once the process is
    'actually running - Nothing beforehand or afterwards. The fact that there
    'is or isn't an active process is significant - for example in the case of
    'a Business Object, where it may not be currently running, but has an active
    'state which will persist until the next execution of an action. Contrast
    'with a normal Process, where each execution is a new instance.
    Protected mProcess As clsProcess

    'The logging engine, when active.
    Protected mLog As clsLoggingEngine

    'True if this is the auto-instance for a Business Object.
    Public mAutoInstance As Boolean

    'The name of the action to run or be run. Null if running a process.
    Public mAction As String

    'Flag indicating that a stop has been requested on this runner
    Private mImmediateStopRequested As Boolean

    ''' <summary>
    ''' Used in conjunction with mStopRequested. Records the name of the user
    ''' who requested that the session be stopped.
    ''' </summary>
    Public mStoppingUser As String

    ''' <summary>
    ''' Used in conjunction with msStoppingUser. Records the name of the resource
    ''' from which the stop request was made.
    ''' </summary>
    Public mStoppingResource As String

    ''' <summary>
    ''' Used in conjunction with mStopRequested. Records a worded reason as to
    ''' why a process was stopped.
    ''' </summary>
    Public mStoppingReason As String

    Private ReadOnly mSessionStatusPersister As ISessionStatusPersister

    ' Lock used to ensure that when a thread is being created and started,
    ' there is no contention over which thread instance is being started.
    Private ReadOnly mThreadLock As New Object()

    Private ReadOnly Property mSessionInformation As SessionInformation

    Protected MustOverride ReadOnly Property SessionIdentifier As SessionIdentifier

    Private mSessionStarted As DateTimeOffset

    Private mSessionExceptionDetail As SessionExceptionDetail

    ''' <summary>
    ''' Creates a new Runner Record over the given process XML
    ''' </summary>
    Protected Sub New(
        ByVal procXml As String,
        ByVal procid As Guid,
        ByVal notifier As ISessionNotifier,
        sessionInfo As SessionInformation,
        ByVal sessionStatusPersister As ISessionStatusPersister)

        Me.Notifier = notifier
        Status = RunnerStatus.PENDING
        mSessionInformation = sessionInfo
        mReviewStatus = True
        mProcessXML = procXml
        mProcess = Nothing
        ProcessId = procid
        mOutputs = Nothing
        mImmediateStopRequested = False
        mAutoInstance = False
        mSessionStatusPersister = sessionStatusPersister
    End Sub

    Public ReadOnly Property SessionExceptionDetail As SessionExceptionDetail
        Get
            Return mSessionExceptionDetail
        End Get
    End Property


    ''' <summary>
    ''' Gets an object representation of the Process this runner record is
    ''' running, or Nothing if it's not running anything yet.
    ''' </summary>
    Public ReadOnly Property Process() As clsProcess
        Get
            Return mProcess
        End Get
    End Property

    ''' <summary>
    ''' Gets whether this runner record is currently active or not - ie. if it is
    ''' started and not yet finished - typically
    ''' <see cref="RunnerStatus.RUNNING">RUNNING</see> or
    ''' <see cref="RunnerStatus.IDLE">IDLE</see>
    ''' </summary>
    Public ReadOnly Property IsActive() As Boolean
        Get
            Return IsRunning OrElse IsIdle
        End Get
    End Property

    ''' <summary>
    ''' Gets whether the session being run by this record is 'busy' for the
    ''' purposes of testing availability - ie. it is either pending or running.
    ''' </summary>
    Public ReadOnly Property IsBusy() As Boolean
        Get
            Return IsPending OrElse IsRunning
        End Get
    End Property

    ''' <summary>
    ''' Gets whether this runner record has failed or not - ie. if its current
    ''' status is <see cref="RunnerStatus.FAILED"/>
    ''' </summary>
    Public ReadOnly Property IsFailed() As Boolean
        Get
            Return (Status = RunnerStatus.FAILED)
        End Get
    End Property

    ''' <summary>
    ''' Gets whether this runner record holds a
    ''' <see cref="RunnerStatus.PENDING">PENDING</see> session or not.
    ''' </summary>
    Public ReadOnly Property IsPending() As Boolean
        Get
            Return (Status = RunnerStatus.PENDING)
        End Get
    End Property

    ''' <summary>
    ''' Gets whether this runner record's session is
    ''' <see cref="RunnerStatus.RUNNING">RUNNING</see> or not
    ''' </summary>
    Public ReadOnly Property IsRunning() As Boolean
        Get
            Return (Status = RunnerStatus.RUNNING)
        End Get
    End Property

    ''' <summary>
    ''' Gets whether this runner record holds a session in an
    ''' <see cref="RunnerStatus.IDLE">IDLE</see> state or not
    ''' </summary>
    Public ReadOnly Property IsIdle() As Boolean
        Get
            Return (Status = RunnerStatus.IDLE)
        End Get
    End Property

    ''' <summary>
    ''' The logging engine in place in this runner record
    ''' </summary>
    Public MustOverride ReadOnly Property Log() As clsLoggingEngine

    ''' <summary>
    ''' The ID of the process held within this runner record.
    ''' </summary>
    Public ReadOnly Property ProcessId As Guid

    ''' <summary>
    ''' Gets the process name of the loaded process in this runner record, or
    ''' null if there is no process loaded into this record.
    ''' </summary>
    Public ReadOnly Property ProcessName() As String
        Get
            If mProcess Is Nothing Then Return Nothing
            Return mProcess.Name
        End Get
    End Property

    Public ReadOnly Property Notifier As ISessionNotifier

    ''' <summary>
    ''' Indicates whether a call has been made from outside the class to stop
    ''' the process running. If this is set to True, then msStoppingUser,
    ''' msStoppingResource and msStoppingReason MUST be set at the same time.
    ''' </summary>
    Public Property ImmediateStopRequested() As Boolean
        Get
            Return mImmediateStopRequested
        End Get
        Set(ByVal value As Boolean)
            If mProcess IsNot Nothing Then mProcess.ImmediateStopRequested = value
            mImmediateStopRequested = value
        End Set
    End Property

    Public ReadOnly Property SessionStarted As DateTimeOffset Implements IRunnerRecord.SessionStarted
        Get
            Return mSessionStarted
        End Get
    End Property

    Public ReadOnly Property Session As clsSession
        Get
            Return mProcess?.Session
        End Get
    End Property

    ''' <summary>
    ''' Stop the current process from running. Called asynchronously from another
    ''' thread while this runner is executing.
    ''' </summary>
    ''' <param name="userName">The username of the user making the request to
    ''' stop.</param>
    ''' <param name="resName">The name of the resource from which the request to
    ''' stop was made. </param>
    ''' <param name="reason">A worded reason as to why the process was stopped.
    ''' </param>
    Public Sub StopProcess(ByVal userName As String, ByVal resName As String,
        Optional ByVal reason As String = "") Implements IRunnerRecord.StopProcess
        mStoppingResource = resName
        mStoppingUser = userName
        mStoppingReason = reason
        'The flag itself MUST be set last...
        ImmediateStopRequested = True

        'If we're in idle state, we can automatically stop here, as the thread is
        'not running...
        If Status = RunnerStatus.IDLE Then
            mProcess.Dispose()
            mProcess = Nothing
            Status = RunnerStatus.STOPPED
            mFailReason = "Stopped"
            mReviewStatus = True
        End If
    End Sub

    ''' <summary>
    ''' Starts a thread to run the process/action using the properties set
    ''' within this runner record.
    ''' </summary>
    ''' <returns>The started thread which was created from this record's
    ''' properties.</returns>
    Public Function StartThread() As Thread
        SyncLock mThreadLock
            mRunnerThread = New Thread(AddressOf RunnerMethod)
            mRunnerThread.SetApartmentState(ApartmentState.STA)
            mRunnerThread.Start()
            Return mRunnerThread
        End SyncLock
    End Function

    ''' <summary>
    ''' Block until the running thread has finished doing its thing.
    ''' </summary>
    Public Sub WaitForCompletion()
        mRunnerThread.Join()
    End Sub

    ''' <summary>
    ''' The method used to run the process/action within its own thread.
    ''' </summary>
    Public Sub RunnerMethod() Implements IRunnerRecord.RunnerMethod

        Dim sErr As String = Nothing

        ' Try and start the session in its own block so that we don't end up
        ' trying to terminate it if an exception is thrown while starting it
        Try
            mSessionStarted = DateTimeOffset.Now
            mSessionStatusPersister.SetPendingSessionRunning(SessionStarted)
        Catch ex As Exception
            Status = RunnerStatus.STARTFAILED
            mFailReason = ex.ToString()
            mReviewStatus = True
            Notifier.RaiseError($"Session (ID: {mSessionInformation.SessionId}) failed to start. Process Name: {ProcessName}; Reason: {mFailReason}")
            Return
        End Try

        Try
            Status = RunnerStatus.RUNNING
            mReviewStatus = True
            Notifier.AddNotification($"STARTED {mSessionInformation.SessionId}")
            Notifier.NotifyStatus()

            'Flags to determine what has happened once we've done the running. If
            'bFailed or bStopped end up as True, sErr will contain an error message.
            'Otherwise all is well.
            Dim bStopped As Boolean = False, bFailed As Boolean = True
            Dim result = StageResult.InternalError(
                "No error information available. This should never happen!")

            Try
                CreateProcessAlert(AlertEventType.ProcessRunning)
            Catch ex As Exception
                sErr = ex.Message
            End Try

            'Create a class representation of the process so we can run it. This
            'is a potentially long-running operation.
            'Note that sometimes we already have an active process - for example
            'when running a Business Object.
            'The XML is no longer required after this.
            If mProcess Is Nothing Then
                mProcess = clsProcess.FromXML(
                    Options.Instance.GetExternalObjectsInfo(True), mProcessXML, False, sErr)
                If mProcess Is Nothing Then Throw New OperationFailedException(
                    "Failed to parse the process XML with the message: {0}", sErr)
                mProcessXML = Nothing
                ' I have no idea why clsProcess can't figure this out for itself,
                ' but it appears it can't
                mProcess.Id = ProcessId

                Log.CreateSessionLog(New LogInfo())

                'Load the parent object if required (for the shared model)
                If mProcess.ParentObject IsNot Nothing Then
                    Dim id As Guid = gSv.GetProcessIDByName(mProcess.ParentObject, True)
                    If id = Guid.Empty Then Throw New OperationFailedException(
                        "Failed to find parent object: {0}", mProcess.ParentObject)
                    mProcess.LoadParent(id)
                End If
            End If

            Dim sessionConnectionSettings = gSv.GetWebConnectionSettings()
            Dim session As New clsSession(SessionIdentifier, sessionConnectionSettings)
            mProcess.Session = session

            mProcess.SetInputParams(mStartupParams)

            ' Once we've set the params in the process, we don't need them any more
            mStartupParams = Nothing

            AddSessionVariableChangeEventHandler()

            Notifier.RaiseInfo($"Session (ID: {mSessionInformation.SessionId}) started. Process Name: {ProcessName}")

            'Run it...
            If mAction IsNot Nothing Then
                Dim gPageID As Guid
                If mAction = clsProcess.InitPageName Then
                    gPageID = mProcess.GetMainPage.ID
                Else
                    gPageID = mProcess.GetSubSheetIDSafeName(mAction)
                    If gPageID.Equals(Guid.Empty) Then
                        sErr = "No action named " & mAction
                        GoTo seterror
                    End If
                End If
                mProcess.RunPageId = gPageID
                result = mProcess.RunAction(ProcessRunAction.GotoPage)
                If Not result.Success Then GoTo seterror

            End If

            result = mProcess.RunAction(ProcessRunAction.Go)
            If Not result.Success Then GoTo seterror

            Do While mProcess.RunState = ProcessRunState.Running
                result = mProcess.RunAction(ProcessRunAction.RunNextStep)
                If ImmediateStopRequested Then
                    sErr = "Stopped by user"
                    bStopped = True
                    bFailed = False

                    Dim sUser = If(mStoppingUser <> "", CStr(String.Format(My.Resources.RunnerRecord_ProcessStoppedByUser0, mStoppingUser)), "")
                    Dim sResource As String = If(mStoppingResource <> "", CStr(String.Format(My.Resources.RunnerRecord_ProcessStoppedFromResource0, mStoppingResource)), "")
                    Dim sReason = If(mStoppingReason <> "", My.Resources.RunnerRecord_ProcessStoppedForTheFollowingreason0, "")

                    Dim sMessage = String.Format(My.Resources.RunnerRecord_ProcessStoppedByUser0FromResource1ForTheFollowingreason2, sUser, sResource, sReason, mStoppingReason)

                    Try
                        Log.ImmediateStop(New LogInfo(), sMessage)
                    Catch ex As Exception
                        Debug.Fail(
                            "Logging of stop reason failed: " & ex.ToString())
                    End Try
                    GoTo seterror

                End If

                If Not result.Success Then GoTo seterror

            Loop
            mOutputs = mProcess.GetOutputs()
            bFailed = False

seterror:
            If bFailed Then
                Status = RunnerStatus.FAILED
                mFailReason = result.GetText()
                mReviewStatus = True
                Notifier.RaiseWarn($"Session (ID: {mSessionInformation.SessionId}) terminated. Process Name: {ProcessName}; Reason: {mFailReason}")

                ' Set session as exception
                Try
                    SetSessionTerminated(SessionExceptionDetail.ProcessError(result.ExceptionType, result.ExceptionDetail))
                    OnSessionEnded()
                    CreateProcessAlert(AlertEventType.ProcessFailed)
                Catch ex As Exception
                    Notifier.HandleSessionStatusFailure(Me, ex.Message)
                End Try

            ElseIf bStopped Then
                Status = RunnerStatus.STOPPED
                mFailReason = sErr
                mReviewStatus = True
                Notifier.RaiseWarn($"Session (ID: {mSessionInformation.SessionId}) stopped. Process Name: {ProcessName}; Reason: {mFailReason}")

                Try
                    mSessionStatusPersister.SetSessionStopped()
                    OnSessionEnded()
                    CreateProcessAlert(AlertEventType.ProcessStopped)
                Catch ex As Exception
                    Notifier.HandleSessionStatusFailure(Me, ex.Message)
                End Try


            Else
                If mAction IsNot Nothing AndAlso mAction = "Initialise" Then
                    ' A Business Object goes to idle status automatically when it
                    ' has finished running its init page.
                    Status = RunnerStatus.IDLE
                    mReviewStatus = True
                Else
                    Status = RunnerStatus.COMPLETED
                    mReviewStatus = True
                End If

                Notifier.RaiseInfo($"Session (ID: {mSessionInformation.SessionId}) completed. Process Name: {ProcessName}")

                Try
                    mSessionStatusPersister.SetSessionCompleted()
                    OnSessionEnded()
                    CreateProcessAlert(AlertEventType.ProcessComplete)
                Catch ex As Exception
                    Notifier.HandleSessionStatusFailure(Me, ex.Message)
                End Try

            End If

Cleanup:
            If mAction Is Nothing OrElse Status = RunnerStatus.STOPPED Then
                mProcess.Dispose()
                mProcess = Nothing
                mLog?.Dispose()
                mLog = Nothing
            End If

        Catch ex As Exception
            Try
                ' To write log messages mProcess must be instantiated.
                mProcess = If(mProcess, New clsProcess(Nothing, DiagramType.Unset, True))

                Log.UnexpectedException(New LogInfo())

                Status = RunnerStatus.FAILED
                mFailReason = ex.GetType().Name & ": " & ex.Message
                mReviewStatus = True
                Notifier.RaiseWarn($"Session (ID: {mSessionInformation.SessionId}) terminated. Process Name: {ProcessName}; Reason: {mFailReason}")

                ' Set session as exception
                Try
                    SetSessionTerminated(SessionExceptionDetail.InternalError(ex.Message))
                    OnSessionEnded()
                    CreateProcessAlert(AlertEventType.ProcessFailed)
                Catch e As Exception
                    Notifier.HandleSessionStatusFailure(Me, e.Message)
                End Try

                If mAction Is Nothing Then
                    mProcess.Dispose()
                    mProcess = Nothing
                    mLog?.Dispose()
                    mLog = Nothing
                End If
            Catch
            End Try
        End Try

    End Sub
    Private Sub SetSessionTerminated(sessionExceptionDetail As SessionExceptionDetail)
        mSessionExceptionDetail = sessionExceptionDetail
        mSessionStatusPersister.SetSessionTerminated(sessionExceptionDetail)
    End Sub


    Protected Overridable Sub OnSessionEnded()
    End Sub

    Protected Overridable Sub AddSessionVariableChangeEventHandler()
    End Sub

    Protected Overridable Sub CreateProcessAlert(type As AlertEventType)

    End Sub

    Public Function GetSessionVariables() As IDictionary(Of String, clsProcessValue) Implements IRunnerRecord.GetSessionVariables
        Dim sessionVariables = mProcess?.Session?.GetAllVars()

        Return If(IsNothing(sessionVariables), New Dictionary(Of String, clsProcessValue), sessionVariables)
    End Function
End Class
