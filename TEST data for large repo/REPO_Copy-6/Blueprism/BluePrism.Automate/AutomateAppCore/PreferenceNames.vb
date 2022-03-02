
''' <summary>
''' Class to hold the preference names, ensuring that all classes using these
''' prefs aren't going to fail due to a typo not caught at compile time.
''' 
''' The names are split into sections, each in their own inner class, so the
''' pref name can be retrieved using (eg)
''' Dim prefname As String = PreferenceNames.Scheduler.Active
''' </summary>
Public Module PreferenceNames

    Public Class Scheduler
        Public Const Active As String = "scheduler.active"
        Public Const CheckSeconds As String = "scheduler.check-seconds"
        Public Const RetryPeriod As String = "scheduler.retry-period"
        Public Const RetryTimes As String = "scheduler.retry-times"
        Public Const CreateSessionSleepInMilliseconds As String = "scheduler.createsessionsleepinmilliseconds"
    End Class

    Public Class UI
        Public Const DefaultDashboard As String = "UI.defaultdashboard"
        Public Const LayoutPreferences = "UI.layoutpreferences"
        Public Const HomePageLoadGraphsAsync As String = "UI.homepage.loadgraphsasync"
        Public Const LogSearchFields As String = "UI.logsearch.fields"
        Public Const DontShowUnicodeResourceWarning As String = "UI.user.dontshowunicoderesourcewarning"
        Public Const ShowTourAtStartup As String = "UI.user.showtour"
        Public Const ShowDeleteSchedulePopup As String = "UI.user.showdeleteschedulepopup"
    End Class

    Public Class Env
        Public Const EnvironmentName As String = "env.name"
        Public Const EnvironmentBackColor As String = "env.backcolor"
        Public Const EnvironmentForeColor As String = "env.forecolor"
        Public Const HideDigitalExchangeTab As String = "env.showexchangetab"
    End Class

    Public Class Session
        Public Const SessionViewList As String = "session.filters"
        Public Const SessionViewDefault As String = "session.view.default"
        Public Const SessionFilterSelectedResource As String = "session.filterbyresources"
        Public Const SessionFilterSelectedProcess As String = "session.filterbyprocesses"
        Public Const SessionViewPrefix As String = "session.view."
        Public Const SessionActionSendSignalMilliseconds As String = "session.sessionactionsendsignalmilliseconds"
    End Class

    Public Class RuntimeRefresh
        Public Const RefreshFreqenecy As String = "runtime.refreshSecs"
    End Class

    Public Class SystemSettings
        Public Const DefaultStageWarningThreshold As String = "system.settings.stagewarningthreshold"
        Public Const EnableBpaEnvironmentData As String = "system.settings.enablebpaenvironmentdata"
        Public Const CertificateExpThreshold As String = "system.settings.certificateExpThreshold"
        Public Const UseAppServerConnections As String = "system.settings.appserverresourceconnection"
        Public Const RuntimeResourceLimit As String = "system.settings.maxappserverrobotconnections"
    End Class

    Public Class TreeViewStates
        Public Const ControlRoomTreeExpandedState As String = "controlroom.tree.expandedstate"
    End Class

    Public Class AutoValidation
        Public Const AutoValidateOnOpen As String = "autovalidate.on.process.open"
        Public Const AutoValidateOnSave As String = "autovalidate.on.process.save"
        Public Const AutoValidateOnReset As String = "autovalidate.on.process.reset"
    End Class

    Public Class StudioTreeViewMode
        Public Const ViewModePrefName As String = "studio.dataitemtree.viewmode"
    End Class

    Public Class StudioTools
        Public Const LockPositions As String = "studio.tools.lock-positions"
        Public Const SavePositions As String = "studio.tools.save-positions"
    End Class

    Public Class XmlSettings
        Public Const MaxAttributeXmlLength As String = "xmlsettings.sessionlogmaxattributexmllength"
        Public Const ZipXmlTransfer As String = "xmlsettings.ziptransfer"
    End Class

    Public Class ResourceConnection
        Public Const PingTimeOutSeconds As String = "resourceconnection.pingtimeoutseconds"
        Public Const ThreadStackSize As String = "resourceconnection.threadstacksize"
        Public Const ConnectionDelay As String = "resourceconnection.delay"
        Public Const QueryCapabilities As String = "resourceconnection.querycapabilities"
        Public Const ResourceConnectionTokenTimeout As String = "resourceconnection.tokentimeout"
        Public Const CallbackReconnectIntervalSeconds As String = "resourceconnection.callbackreconnectintervalseconds"
        Public Const CallbackKeepAliveTimeMS As String = "resourceconnection.callbackkeepalivetime"
        Public Const CallbackKeepAliveTimeoutMS As String = "resourceconnection.callbackkeepalivetimeout"
        Public Const ConnectionPingTime As String = "resourceconnection.connectionpingtime"
        Public Const ProcessResourceInputSleepTime As String = "resourceconnection.processresourceinputsleeptime"
        Public Const InitializationPause As String = "resourceconnection.initialization.pause"
        Public Const ListenerEnableRunModeCache As String = "resourceconnection.listener.enablerunmodecache"
        Public Const ResourceConnectionLowThreshold As String = "resourceconnection.environmentrobotslowthreshold"
        Public Const ResourceConnectionHighThreshold As String = "resourceconnection.environmentrobotshighthreshold"
        Public Const DisableConnection As String = "resourceconnection.disableconnection"
        Public Const DontShowClientRobotConnectionCheckLow As String = "resourceconnection.dontshowclientrobotconnectionchecklow"
        Public Const DontShowClientRobotConnectionCheckHigh As String = "resourceconnection.dontshowclientrobotconnectioncheckhigh"
        Public Const RetrySleepTimerInSeconds As String = "resourceconnection.retrysleeptimerinseconds"
    End Class

    Public Class Release
        Public Const ReleaseCompressed As String = "release.compress"
    End Class

    Public Class ApplicationManager
        Public Const Tesseract As String = "applicationmanager.tesseract"
    End Class

End Module
