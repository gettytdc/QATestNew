
Imports BluePrism.BPCoreLib

Namespace Resources
    Public Class ListenerResourceRunner
        Inherits ResourceRunnerBase

        Private ReadOnly mOptions As ResourcePCStartUpOptions
        Private WithEvents mListener As clsListener
        Private mLogExternallyUpdater As Threading.Timer
        Private mListenerMonitor As Threading.Timer
        Private mNeedsRestart As Boolean
        Private mShutdownConfirmed As Boolean

        Public Sub New(options As ResourcePCStartUpOptions, view As IResourcePcView)
            MyBase.New(view)
            mOptions = options
        End Sub

        Protected Overrides Function GetEventLogSource() As String
            Return EventLogHelper.GetResourcePcSource(mOptions.Port)
        End Function

        Protected Overrides Function ShouldLogExternally() As Boolean
            Try
                ' If we have all the data we need to check external logging,
                ' do so, otherwise leave it as it is
                Dim resourceId = mListener?.ResourceId

                If resourceId IsNot Nothing AndAlso resourceId <> Guid.Empty AndAlso ServerFactory.ServerAvailable Then
                    Return gSv.IsResourceEventLoggingEnabled(resourceId.Value)
                End If

            Catch ex As Exception
                ' Any errors - assume that we should be logging externally
                Return True

            End Try
        End Function

        Protected Overrides Sub BeforeStartUp()
            If mListener Is Nothing Then
                mListener = New clsListener
            End If
        End Sub


        Protected Overrides Function Start() As Boolean
            'Start a timer to update the log to event log flag
            If mLogExternallyUpdater Is Nothing Then
                mLogExternallyUpdater = New Threading.Timer(AddressOf UpdateLogExternally, Nothing, 0, 60000)
            End If

            If mListener Is Nothing Then
                DisplayNotification(ResourceNotificationLevel.Error, GetEnglishTranslation("FailedListenerNotInitialised"))
                Return False
            End If

            'Start up the listener
            Dim sErr = ""
            If Not mListener.Startup(mOptions, sErr) Then
                DisplayNotification(ResourceNotificationLevel.Error, String.Format(GetEnglishTranslation("FailedListenerDidNotStart0"), sErr))
                Return False
            End If

            If Not mOptions.HTTPEnabled Then
                DisplayNotification(ResourceNotificationLevel.Comment, GetEnglishTranslation("HTTPCommunicationIsDisabled"))
                Return False
            End If

            ' Check listener every 2 minutes
            If mListenerMonitor Is Nothing Then
                mListenerMonitor = New Threading.Timer(AddressOf CheckListener, Nothing, 0, 120000)
            End If

            Return True
        End Function

        Protected Overrides Function ShouldRegisterServerConnectionHandlers() As Boolean
            'When using built in resourcepc we don't need to register connection dropping handlers.
            Return Not mOptions.IsAuto
        End Function

        ''' <summary>
        ''' Check whether the listener needs restarting and restart if necessary. This 
        ''' method is running on a separate thread using the mListenerMonitor Timer which
        ''' will run the check every 2 minutes
        ''' </summary>
        Private Sub CheckListener(state As Object)
            If mListener Is Nothing Then Return

            If mNeedsRestart Then
                Dim sErr As String = Nothing
                If Not mListener.Startup(mOptions, sErr) Then
                    DisplayNotification(ResourceNotificationLevel.Error, String.Format(GetEnglishTranslation("CouldNotRestartListener0"), sErr))
                    DisplayNotification(ResourceNotificationLevel.Comment, GetEnglishTranslation("WillRetryIn2Minutes"))
                Else
                    DisplayNotification(ResourceNotificationLevel.Comment, GetEnglishTranslation("ListenerRestarted"))
                    mNeedsRestart = False
                End If
            End If
        End Sub

        Private Sub HandleFailed(message As String) Handles mListener.Failed
            DisplayNotification(ResourceNotificationLevel.Error, String.Format(GetEnglishTranslation("ListenerFailed0"), message))
            DisplayNotification(ResourceNotificationLevel.Comment, GetEnglishTranslation("WillRetryIn2Minutes"))

            mNeedsRestart = True
        End Sub

        Private Sub HandleVerbose(message As String) Handles mListener.Verbose
            DisplayNotification(ResourceNotificationLevel.Verbose, message)
        End Sub

        Private Sub HandleInfo(message As String) Handles mListener.Info
            DisplayNotification(ResourceNotificationLevel.Comment, message)
        End Sub

        Private Sub HandleWarn(message As String) Handles mListener.Warn
            DisplayNotification(ResourceNotificationLevel.Warning, message)
        End Sub

        Private Sub HandleError(message As String) Handles mListener.Err
            DisplayNotification(ResourceNotificationLevel.Error, message)
        End Sub

        Private Sub HandleListenerShutdown() Handles mListener.ShutdownResource
            Shutdown()
            CheckListener(Nothing)
        End Sub

        Protected Overrides Sub ViewRefreshing()
            If mListener IsNot Nothing Then
                Dim activeSessionCount = mListener.GetActiveSessionCount()
                Dim pendingSessionCount = mListener.GetPendingSessionCount()
                Dim activeConnectionsStatus = GetActiveConnectionsStatus()
                View.DisplayStatus(activeSessionCount, pendingSessionCount, activeConnectionsStatus)
            End If
        End Sub

        Protected Overrides Function AllowRestart() As Boolean
            Dim activeSessionCount = GetActiveSessionCount()
            Return activeSessionCount = 0 OrElse View.ConfirmRestart(activeSessionCount)
        End Function

        Protected Overrides Sub [Stop](shuttingDown As Boolean)
            mLogExternallyUpdater?.Dispose()
            mLogExternallyUpdater = Nothing
            mListener?.Shutdown(String.Empty)
            mListener = Nothing
            mListenerMonitor?.Dispose()
            mListenerMonitor = Nothing
        End Sub

        Public Overrides Function SessionsRunning() As Boolean
            If mListener Is Nothing OrElse mListener.Runners Is Nothing Then
                Return True
            Else
                Return mListener.Runners.RunningCount = 0
            End If
        End Function

        Public Function GetActiveSessionCount() As Integer
            Return If(mListener?.GetActiveSessionCount(), 0)
        End Function

        Private Function GetActiveConnectionsStatus() As String
            If mNeedsRestart Then
                Return My.Resources.OfflineRestartPending
            End If

            Return String.Format(My.Resources.ActiveConnections0, If(mListener?.GetActiveConnections(), 0))
        End Function

        Protected Overrides Function AllowShutDown() As Boolean
            ' If the listener triggers the form shutdown (for login agent) then the 
            ' listener will be set to nothing
            If mListener Is Nothing OrElse mShutdownConfirmed = True Then Return True

            Dim activeSessionCount As Integer = mListener.GetActiveSessionCount()
            If activeSessionCount = 0 Then Return True
            ' Remember response so we don't ask again - triggered by view closing
            mShutdownConfirmed = View.ConfirmShutdown(activeSessionCount)
            Return mShutdownConfirmed
        End Function

    End Class
End Namespace
