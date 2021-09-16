Imports System.Globalization
Imports System.Security.Cryptography
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports BluePrism.AMI
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib
Imports NLog

Namespace Resources

    Public MustInherit Class ResourceRunnerBase
        Implements IResourceRunner

        Private Shared ReadOnly NotificationCulture As CultureInfo = New Globalization.CultureInfo("en")
        Protected WithEvents mView As IResourcePCView
        Protected mLogExternally As Boolean
        Private mShuttingDown As Boolean
        Private Shared ReadOnly mLogger As Logger = LogManager.GetCurrentClassLogger()

        Protected Sub New(view As IResourcePCView)
            If view Is Nothing Then Throw New ArgumentNullException(NameOf(view))

            mView = view
        End Sub

        Protected ReadOnly Property View As IResourcePCView
            Get
                Return mView
            End Get
        End Property

        Public MustOverride Function SessionsRunning() As Boolean Implements IResourceRunner.SessionsRunning

        Public Function IsRunning() As Boolean Implements IResourceRunner.IsRunning
            Return View.FormDialogResult = DialogResult.None AndAlso Not mShuttingDown
        End Function

        Public Overridable Sub Init(Optional startedCallback As Action = Nothing) Implements IResourceRunner.Init

            View.Init()

            DisplayNotification(ResourceNotificationLevel.Comment, String.Format(GetEnglishTranslation("InitialisingEventsLoggedIn_0_Time"), TimeZone.CurrentTimeZone.StandardName))

            ' Start up resource functionality
            BeforeStartUp()
            Task.Factory.StartNew(Sub()
                                      RunStartSteps()
                                      startedCallback?.Invoke()
                                  End Sub)
        End Sub

        Private Sub RunStartSteps()
            If Not CoreStart() Then Return
            If Not Start Then Return
            DisplayNotification(ResourceNotificationLevel.Comment, GetEnglishTranslation("StartupSequenceComplete"))
            CheckForFIPSCompliance()
        End Sub

        Protected Overridable Sub BeforeStartUp()
        End Sub

        Protected Overridable Function Start() As Boolean
        End Function

        Private Sub CheckForFIPSCompliance()
            If CryptoConfig.AllowOnlyFipsAlgorithms Then 
                If gSv.DBEncryptionSchemesAreFipsCompliant().Count > 0 Then _
                    DisplayNotification(ResourceNotificationLevel.Comment,
                                         My.Resources.ThereAreEncryptionSchemesInYourDatabaseThatAreNotFIPSCompliant)
            End If
        End Sub


        Protected Overridable Function CoreStart() As Boolean

            Dim configOptions = Options.Instance
            DisplayNotification(ResourceNotificationLevel.Comment, String.Format(GetEnglishTranslation("CheckingConnection0"), configOptions.CurrentConnectionName))
            'Check the connection....
            Try
                If Not configOptions.DbConnectionSetting.IsComplete Then
                    DisplayNotification(ResourceNotificationLevel.Warning, GetEnglishTranslation("NoConnectionConfigured"))
                    Return False
                End If

                If Not ServerFactory.ServerAvailable Then
                    Try
                        ServerFactory.CurrentConnectionValid()
                    Catch ex As Exception
                        DisplayNotification(ResourceNotificationLevel.Error, String.Format(GetEnglishTranslation("CannotStartNoConnection0"), ex.Message))
                        Return False
                    End Try
                End If

                If ShouldRegisterServerConnectionHandlers() Then ServerFactory.ServerManager.RegisterHandlers(AddressOf ConnectionPending, AddressOf ConnectionRestored)

                UpdateConnectedTo()

            Catch ex As Exception
                DisplayNotification(ResourceNotificationLevel.Error, String.Format(GetEnglishTranslation("FailedException0"), ex.Message))
                Return False
            End Try

            'Initialise AMI...
            DisplayNotification(ResourceNotificationLevel.Comment, GetEnglishTranslation("InitialisingAMI"))
            Dim sErr As String = Nothing
            If Not clsAMI.Init(sErr) Then
                Dim errorMessage = String.Format(GetEnglishTranslation("FailedApplicationManagerInitialisationError0"), sErr)
                DisplayNotification(ResourceNotificationLevel.Error, errorMessage)
                Return False
            End If

            UpdateLogExternally(Nothing)

            Return True
        End Function

        Protected Sub DisplayNotification(level As ResourceNotificationLevel, message As String)
            Dim notification As New ResourceNotification(level, message, Date.Now)
            DisplayNotification(notification)
        End Sub

        Protected Overridable Sub DisplayNotification(notification As ResourceNotification)
            View.DisplayNotification(notification)
            LogExternally(notification)
        End Sub

        Protected Function GetEnglishTranslation(resourceKey As String) As String
            Return My.Resources.ResourceManager.GetString(resourceKey, NotificationCulture)
        End Function

        Protected Overridable Sub ViewRefreshing Handles mView.RefreshStatus
        End Sub

        Private Sub RestartRequested Handles mView.RestartRequested
            If AllowRestart Then
                Restart()
            End If
        End Sub

        Protected MustOverride Function AllowRestart() As Boolean

        Private Sub Restart()
            DisplayNotification(ResourceNotificationLevel.Comment, GetEnglishTranslation("Restarting"))
            [Stop](False)
            Init()
            View.DisplayRestarting()
        End Sub

        Private Sub CloseRequested(sender As Object, args As FormClosingEventArgs) Handles mView.CloseRequested
            If mShuttingDown Then Return
            If AllowShutDown Then
                Shutdown()
            Else
                args.Cancel = True
            End If
        End Sub

        Private Sub ShutdownRequested Handles mView.ShutdownRequested
            If AllowShutDown Then
                Shutdown()
            End If
        End Sub

        Protected MustOverride Function AllowShutDown() As Boolean

        Public Sub Shutdown() Implements IResourceRunner.Shutdown
            View.BeginRunOnUIThread(Sub()
                                        mShuttingDown = True
                                        View.DisplayShuttingDown()
                                        [Stop](True)
                                        View.CloseForm()
                                    End Sub)
        End Sub

        ''' <summary>
        ''' Provides any stop steps for runner implementation. Called when stopping, restarting
        ''' and also during dispose
        ''' </summary>
        Protected MustOverride Sub [Stop](shuttingDown As Boolean)

        Protected Overridable Function ShouldRegisterServerConnectionHandlers() As Boolean
            Return True
        End Function

        Private Sub ConnectionPending()
            DisplayNotification(ResourceNotificationLevel.Comment, GetEnglishTranslation("ServerConnectionLost"))
            Thread.Sleep(5000)
        End Sub

        Private Sub UpdateConnectedTo()
            DisplayNotification(ResourceNotificationLevel.Comment, String.Format(GetEnglishTranslation("ConnectedTo0"), gSv.GetConnectedTo()))
        End Sub

        Private Sub ConnectionRestored()
            DisplayNotification(ResourceNotificationLevel.Comment, GetEnglishTranslation("ServerConnectionRestored"))
            UpdateConnectedTo()
        End Sub

        ''' <summary>
        ''' Checks if this resource is currently set to log to windows externally.
        ''' This setting is checked no more than once per minute, and if the setting
        ''' cannot be retrieved for whatever reason (no listener, no database) then
        ''' the assumption is made that it <em>should</em> log to the event log.
        ''' </summary>
        Protected Sub UpdateLogExternally(state As Object)

            Dim currValue As Boolean = mLogExternally
            mLogExternally = ShouldLogExternally()

            If currValue <> mLogExternally Then
                DisplayNotification(ResourceNotificationLevel.Comment,
                                    If(mLogExternally, GetEnglishTranslation("ActivityLogSetToLogExternally"),
                                       GetEnglishTranslation("ActivityLogSetToNotLogExternally")))
            End If

            If mLogExternally Then
                EnsureEventLogSource()
            End If
        End Sub

        ''' <summary>
        ''' Ensures creation of a Windows Event Log source based on the name of this 
        ''' resource PC, displaying an error if this fails. This is only run once
        ''' during the lifetime of the form.
        ''' </summary>
        ''' <remarks>
        ''' During setup, a Resource PC is normally run once with elevated permissions to 
        ''' create the event source for a specific port number.
        ''' </remarks>
        Protected Sub EnsureEventLogSource()
            Static checked As Boolean = False

            If checked Then Return
            Try
                Dim source = GetEventLogSource()
                EventLogHelper.CreateSource(source, EventLogHelper.DefaultLogName)
            Catch exception As Exception
                mLogger.Error(exception, "Error creating Windows event log source for Resource PC")
                DisplayNotification(ResourceNotificationLevel.Warning, GetEnglishTranslation("frmResourcePC_WindowsEventLogSourceCreationFailed"))
                UseFallbackEventLogSource()
            End Try
            checked = True
        End Sub

        Protected MustOverride Function GetEventLogSource() As String

        Public Sub UseFallbackEventLogSource()
            GlobalDiagnosticsContext.Set("AppEventLogSource", EventLogHelper.DefaultSource)
        End Sub

        Protected Sub LogExternally(notification As ResourceNotification)
            If mLogExternally Then
                Select Case notification.Level
                    Case ResourceNotificationLevel.Error
                        mLogger.Error(notification.Text)
                    Case ResourceNotificationLevel.Warning
                        mLogger.Warn(notification.Text)
                    Case ResourceNotificationLevel.Comment
                        mLogger.Info(notification.Text)
                    Case ResourceNotificationLevel.Verbose
                        mLogger.Debug(notification.Text)
                End Select
            End If
        End Sub

        Protected Overridable Function ShouldLogExternally() As Boolean
            Return True
        End Function

        Public Overridable Sub Dispose() Implements IDisposable.Dispose
            [Stop](True)
        End Sub

    End Class
End Namespace