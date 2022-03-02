
Public Class ctlClient
    Inherits Windows.Forms.AxHost
    Implements IClientEvents
    Implements IClient

    Public Event OnConnect()
    Public Event OnDisconnect()

    Public Sub New()
        MyBase.New(modClassID.AxClient)
    End Sub

    Private ocx As IClient

    Protected Overrides Sub AttachInterfaces()
        ocx = TryCast(MyBase.GetOcx, IClient)
    End Sub

    Private cookie As ConnectionPointCookie

    Protected Overrides Sub CreateSink()
        cookie = New ConnectionPointCookie(Me.ocx, Me, GetType(IClientEvents))
    End Sub

    Protected Overrides Sub DetachSink()
        cookie.Disconnect()
    End Sub



#Region "Event Handlers"
    Private Sub RaiseChannelDataReceived(ByVal ChannelName As String) Implements IClientEvents.OnChannelDataReceived
    End Sub

    Private Sub RaiseClick(ByVal MouseButton As Integer, ByVal PosX As Integer, ByVal PosY As Integer, ByVal KeyMask As Integer) Implements IClientEvents.OnClick
    End Sub

    Private Sub RaiseConnect() Implements IClientEvents.OnConnect
        RaiseEvent OnConnect()
    End Sub

    Private Sub RaiseConnectFailed() Implements IClientEvents.OnConnectFailed
    End Sub

    Private Sub RaiseConnecting() Implements IClientEvents.OnConnecting
    End Sub

    Private Sub RaiseDisconnect() Implements IClientEvents.OnDisconnect
        RaiseEvent OnDisconnect()
    End Sub

    Private Sub RaiseDisconnectFailed() Implements IClientEvents.OnDisconnectFailed
    End Sub

    Private Sub RaiseDisconnectSessions(ByVal hCommand As Integer) Implements IClientEvents.OnDisconnectSessions
    End Sub

    Private Sub RaiseDisconnectSessionsFailed(ByVal hCommand As Integer) Implements IClientEvents.OnDisconnectSessionsFailed
    End Sub

    Private Sub RaiseICAFile() Implements IClientEvents.OnICAFile
    End Sub

    Private Sub RaiseICAFileFailed() Implements IClientEvents.OnICAFileFailed
    End Sub

    Private Sub RaiseInitializing() Implements IClientEvents.OnInitializing
    End Sub

    Private Sub RaiseInitialProp() Implements IClientEvents.OnInitialProp
    End Sub

    Private Sub RaiseLogoffFailed() Implements IClientEvents.OnLogoffFailed
    End Sub

    Private Sub RaiseLogoffSessions(ByVal hCommand As Integer) Implements IClientEvents.OnLogoffSessions
    End Sub

    Private Sub RaiseLogoffSessionsFailed(ByVal hCommand As Integer) Implements IClientEvents.OnLogoffSessionsFailed
    End Sub

    Private Sub RaiseLogon() Implements IClientEvents.OnLogon
    End Sub

    Private Sub RaiseLogonFailed() Implements IClientEvents.OnLogonFailed
    End Sub

    Private Sub RaisePublishedApp() Implements IClientEvents.OnPublishedApp
    End Sub

    Private Sub RaisePublishedAppFailed() Implements IClientEvents.OnPublishedAppFailed
    End Sub

    Private Sub RaiseReadyStateChange(ByVal lReadyState As Integer) Implements IClientEvents.OnReadyStateChange
    End Sub

    Private Sub RaiseSessionAttach(ByVal hSession As Integer) Implements IClientEvents.OnSessionAttach
    End Sub

    Private Sub RaiseSessionDetach(ByVal hSession As Integer) Implements IClientEvents.OnSessionDetach
    End Sub

    Private Sub RaiseSessionEventPending(ByVal hSession As Integer, ByVal EventNum As Integer) Implements IClientEvents.OnSessionEventPending
    End Sub

    Private Sub RaiseSessionSwitch(ByVal hOldSession As Integer, ByVal hNewSession As Integer) Implements IClientEvents.OnSessionSwitch
    End Sub

    Private Sub RaiseWindowCloseRequest() Implements IClientEvents.OnWindowCloseRequest
    End Sub

    Private Sub RaiseWindowCreated(ByVal WndType As Integer, ByVal XPos As Integer, ByVal YPos As Integer, ByVal Width As Integer, ByVal Height As Integer) Implements IClientEvents.OnWindowCreated
    End Sub

    Private Sub RaiseWindowDestroyed(ByVal WndType As Integer) Implements IClientEvents.OnWindowDestroyed
    End Sub

    Private Sub RaiseWindowDisplayed(ByVal WndType As Integer) Implements IClientEvents.OnWindowDisplayed
    End Sub

    Private Sub RaiseWindowDocked() Implements IClientEvents.OnWindowDocked
    End Sub

    Private Sub RaiseWindowFullscreened() Implements IClientEvents.OnWindowFullscreened
    End Sub

    Private Sub RaiseWindowHidden(ByVal WndType As Integer) Implements IClientEvents.OnWindowHidden
    End Sub

    Private Sub RaiseWindowMaximized() Implements IClientEvents.OnWindowMaximized
    End Sub

    Private Sub RaiseWindowMinimized() Implements IClientEvents.OnWindowMinimized
    End Sub

    Private Sub RaiseWindowMoved(ByVal WndType As Integer, ByVal XPos As Integer, ByVal YPos As Integer) Implements IClientEvents.OnWindowMoved
    End Sub

    Private Sub RaiseWindowRestored() Implements IClientEvents.OnWindowRestored
    End Sub

    Private Sub RaiseWindowSized(ByVal WndType As Integer, ByVal Width As Integer, ByVal Height As Integer) Implements IClientEvents.OnWindowSized
    End Sub

    Private Sub RaiseWindowUndocked() Implements IClientEvents.OnWindowUndocked
    End Sub
#End Region

    Public Sub AboutBox() Implements IClient.AboutBox
        ocx.AboutBox()
    End Sub

    Public Property Address() As String Implements IClient.Address
        Get
            Return ocx.Address
        End Get
        Set(ByVal value As String)
            ocx.Address = value
        End Set
    End Property

    Public Property Application() As String Implements IClient.Application
        Get
            Return ocx.Application
        End Get
        Set(ByVal value As String)
            ocx.Application = value
        End Set
    End Property

    Public Property AppsrvIni() As String Implements IClient.AppsrvIni
        Get
            Return ocx.AppsrvIni
        End Get
        Set(ByVal value As String)
            ocx.AppsrvIni = value
        End Set
    End Property

    Public Function AttachSession(ByVal pSessionId As String) As Integer Implements IClient.AttachSession
        ocx.AttachSession(pSessionId)
    End Function

    Public Property AudioBandwidthLimit() As ICASoundQuality Implements IClient.AudioBandwidthLimit
        Get
            Return ocx.AudioBandwidthLimit
        End Get
        Set(ByVal value As ICASoundQuality)
            ocx.AudioBandwidthLimit = value
        End Set
    End Property

    Public Property AudioDuringDetach() As Boolean Implements IClient.AudioDuringDetach
        Get
            Return ocx.AudioDuringDetach
        End Get
        Set(ByVal value As Boolean)
            ocx.AudioDuringDetach = value
        End Set
    End Property

    Public Property AUTHUsername() As String Implements IClient.AUTHUsername
        Get
            Return ocx.AUTHUsername
        End Get
        Set(ByVal value As String)
            ocx.AUTHUsername = value
        End Set
    End Property

    Public Property AutoAppResize() As Boolean Implements IClient.AutoAppResize
        Get
            Return ocx.AutoAppResize
        End Get
        Set(ByVal value As Boolean)
            ocx.AutoAppResize = value
        End Set
    End Property

    Public Property AutoLogonAllowed() As Boolean Implements IClient.AutoLogonAllowed
        Get
            Return ocx.AutoLogonAllowed
        End Get
        Set(ByVal value As Boolean)
            ocx.AutoLogonAllowed = value
        End Set
    End Property

    Public Property AutoScale() As Boolean Implements IClient.AutoScale
        Get
            Return ocx.AutoScale
        End Get
        Set(ByVal value As Boolean)
            ocx.AutoScale = value
        End Set
    End Property

    Public Property BackgroundColor() As UInteger Implements IClient.BackgroundColor
        Get
            Return ocx.BackgroundColor
        End Get
        Set(ByVal value As UInteger)
            ocx.BackgroundColor = value
        End Set
    End Property

    Public Property Border() As Integer Implements IClient.Border
        Get
            Return ocx.Border
        End Get
        Set(ByVal value As Integer)
            ocx.Border = value
        End Set
    End Property

    Public Property BorderColor() As UInteger Implements IClient.BorderColor
        Get
            Return ocx.BorderColor
        End Get
        Set(ByVal value As UInteger)
            ocx.BorderColor = value
        End Set
    End Property

    Public Property BrowserProtocol() As String Implements IClient.BrowserProtocol
        Get
            Return ocx.BrowserProtocol
        End Get
        Set(ByVal value As String)
            ocx.BrowserProtocol = value
        End Set
    End Property

    Public Property BrowserRetry() As Integer Implements IClient.BrowserRetry
        Get
            Return ocx.BrowserRetry
        End Get
        Set(ByVal value As Integer)
            ocx.BrowserRetry = value
        End Set
    End Property

    Public Property BrowserTimeout() As Integer Implements IClient.BrowserTimeout
        Get
            Return ocx.BrowserTimeout
        End Get
        Set(ByVal value As Integer)
            ocx.BrowserTimeout = value
        End Set
    End Property

    Public Property CacheICAFile() As Boolean Implements IClient.CacheICAFile
        Get
            Return ocx.CacheICAFile
        End Get
        Set(ByVal value As Boolean)
            ocx.CacheICAFile = value
        End Set
    End Property

    Public Property CDMAllowed() As Boolean Implements IClient.CDMAllowed
        Get
            Return ocx.CDMAllowed
        End Get
        Set(ByVal value As Boolean)
            ocx.CDMAllowed = value
        End Set
    End Property

    Public Sub ClearProps() Implements IClient.ClearProps
        ocx.ClearProps()
    End Sub

    Public Property ClientAudio() As Boolean Implements IClient.ClientAudio
        Get
            Return ocx.ClientAudio
        End Get
        Set(ByVal value As Boolean)
            ocx.ClientAudio = value
        End Set
    End Property

    Public Property ClientName() As String Implements IClient.ClientName
        Get
            Return ocx.ClientName
        End Get
        Set(ByVal value As String)
            ocx.ClientName = value
        End Set
    End Property

    Public ReadOnly Property ClientPath() As String Implements IClient.ClientPath
        Get
            Return ocx.ClientPath
        End Get
    End Property

    Public ReadOnly Property ClientVersion() As String Implements IClient.ClientVersion
        Get
            Return ocx.ClientVersion
        End Get
    End Property

    Public Function CloseEnumHandle(ByVal hndEnum As Integer) As Integer Implements IClient.CloseEnumHandle
        Return ocx.CloseEnumHandle(hndEnum)
    End Function

    Public Property COMAllowed() As Boolean Implements IClient.COMAllowed
        Get
            Return ocx.COMAllowed
        End Get
        Set(ByVal value As Boolean)
            ocx.COMAllowed = value
        End Set
    End Property

    Public Property Compress() As Boolean Implements IClient.Compress
        Get
            Return ocx.Compress
        End Get
        Set(ByVal value As Boolean)
            ocx.Compress = value
        End Set
    End Property

    Public Sub Connect() Implements IClient.Connect
        ocx.Connect()
    End Sub

    Public ReadOnly Property Connected() As Boolean Implements IClient.Connected
        Get
            Return ocx.Connected
        End Get
    End Property

    Public Property ConnectionEntry() As String Implements IClient.ConnectionEntry
        Get
            Return ocx.ConnectionEntry()
        End Get
        Set(ByVal value As String)
            ocx.ConnectionEntry = value
        End Set
    End Property

    Public Property ControlWindowText() As String Implements IClient.ControlWindowText
        Get
            Return ocx.ControlWindowText
        End Get
        Set(ByVal value As String)
            ocx.ControlWindowText = value
        End Set
    End Property

    Public Property CPMAllowed() As Boolean Implements IClient.CPMAllowed
        Get
            Return ocx.CPMAllowed
        End Get
        Set(ByVal value As Boolean)
            ocx.CPMAllowed = value
        End Set
    End Property

    Public Function CreateChannels(ByVal ChannelNames As String) As Integer Implements IClient.CreateChannels
        Return ocx.CreateChannels(ChannelNames)
    End Function

    Public Property CustomMessage() As String Implements IClient.CustomMessage
        Get
            Return ocx.CustomMessage
        End Get
        Set(ByVal value As String)
            ocx.CustomMessage = value
        End Set
    End Property

    Public Sub DeleteProp(ByVal Name As String) Implements IClient.DeleteProp
        ocx.DeleteProp(Name)
    End Sub

    Public Sub DeletePropByIndex(ByVal Index As Integer) Implements IClient.DeletePropByIndex
        ocx.DeletePropByIndex(Index)
    End Sub

    Public Function DeleteWindow() As Integer Implements IClient.DeleteWindow
        Return ocx.DeleteWindow
    End Function

    Public Property Description() As String Implements IClient.Description
        Get
            Return ocx.Description
        End Get
        Set(ByVal value As String)
            ocx.Description = value
        End Set
    End Property

    Public Property DesiredColor() As ICAColorDepth Implements IClient.DesiredColor
        Get
            Return ocx.DesiredColor
        End Get
        Set(ByVal value As ICAColorDepth)
            ocx.DesiredColor = value
        End Set
    End Property

    Public Property DesiredHRes() As Integer Implements IClient.DesiredHRes
        Get
            Return ocx.DesiredHRes
        End Get
        Set(ByVal value As Integer)
            ocx.DesiredHRes = value
        End Set
    End Property

    Public Property DesiredVRes() As Integer Implements IClient.DesiredVRes
        Get
            Return ocx.DesiredVRes
        End Get
        Set(ByVal value As Integer)
            ocx.DesiredVRes = value
        End Set
    End Property

    Public Function DetachSession(ByVal pSessionId As String) As Integer Implements IClient.DetachSession
        Return ocx.DetachSession(pSessionId)
    End Function

    Public Property DisableCtrlAltDel() As Boolean Implements IClient.DisableCtrlAltDel
        Get
            Return ocx.DisableCtrlAltDel
        End Get
        Set(ByVal value As Boolean)
            ocx.DisableCtrlAltDel = value
        End Set
    End Property

    Public Function DisableKeyboardInput() As Integer Implements IClient.DisableKeyboardInput
        Return ocx.DisableKeyboardInput()
    End Function

    Public Function DisableMouseInput() As Integer Implements IClient.DisableMouseInput
        Return ocx.DisableMouseInput()
    End Function

    Public Function DisableSizingBorder() As Integer Implements IClient.DisableSizingBorder
        Return ocx.DisableSizingBorder
    End Function

    Public Sub Disconnect() Implements IClient.Disconnect
        ocx.Disconnect()
    End Sub

    Public Function DisconnectSessions(ByVal pGroupId As String) As Integer Implements IClient.DisconnectSessions
        Return ocx.DisconnectSessions(pGroupId)
    End Function

    Public Function DisplayWindow(ByVal WndType As ICAWindowType) As Integer Implements IClient.DisplayWindow
        Return ocx.DisplayWindow(WndType)
    End Function

    Public Function DockWindow() As Integer Implements IClient.DockWindow
        Return ocx.DockWindow
    End Function

    Public Property Domain() As String Implements IClient.Domain
        Get
            Return ocx.Domain
        End Get
        Set(ByVal value As String)
            ocx.Domain = value
        End Set
    End Property

    Public Property DoNotUseDefaultCSL() As Boolean Implements IClient.DoNotUseDefaultCSL
        Get
            Return ocx.DoNotUseDefaultCSL
        End Get
        Set(ByVal value As Boolean)
            ocx.DoNotUseDefaultCSL = value
        End Set
    End Property

    Public Function EnableKeyboardInput() As Integer Implements IClient.EnableKeyboardInput
        Return ocx.EnableKeyboardInput
    End Function

    Public Function EnableMouseInput() As Integer Implements IClient.EnableMouseInput
        Return ocx.EnableMouseInput
    End Function

    Public Property EnableSessionSharingClient() As Boolean Implements IClient.EnableSessionSharingClient
        Get
            Return ocx.EnableSessionSharingClient
        End Get
        Set(ByVal value As Boolean)
            ocx.EnableSessionSharingClient = value
        End Set
    End Property

    Public Property EnableSessionSharingHost() As Boolean Implements IClient.EnableSessionSharingHost
        Get
            Return ocx.EnableSessionSharingHost
        End Get
        Set(ByVal value As Boolean)
            ocx.EnableSessionSharingHost = value
        End Set
    End Property

    Public Function EnableSizingBorder() As Integer Implements IClient.EnableSizingBorder
        Return ocx.EnableSizingBorder
    End Function

    Public Property Encrypt() As Boolean Implements IClient.Encrypt
        Get
            Return ocx.Encrypt
        End Get
        Set(ByVal value As Boolean)
            ocx.Encrypt = value
        End Set
    End Property

    Public Property EncryptionLevelSession() As String Implements IClient.EncryptionLevelSession
        Get
            Return ocx.EncryptionLevelSession
        End Get
        Set(ByVal value As String)
            ocx.EncryptionLevelSession = value
        End Set
    End Property

    Public Function EnumerateApplications() As Integer Implements IClient.EnumerateApplications
        Return ocx.EnumerateApplications
    End Function

    Public Function EnumerateFarms() As Integer Implements IClient.EnumerateFarms
        Return ocx.EnumerateFarms
    End Function

    Public Function EnumerateServers() As Integer Implements IClient.EnumerateServers
        Return ocx.EnumerateServers
    End Function

    Public Function FocusWindow() As Integer Implements IClient.FocusWindow
        Return ocx.FocusWindow
    End Function

    Public Function FullScreenWindow() As Integer Implements IClient.FullScreenWindow
        Return ocx.FullScreenWindow
    End Function

    Public Function GetCachedSessionCount() As Integer Implements IClient.GetCachedSessionCount
        Return ocx.GetCachedSessionCount
    End Function

    Public Function GetChannelCount() As Integer Implements IClient.GetChannelCount
        Return ocx.GetChannelCount
    End Function

    Public Function GetChannelData(ByVal ChannelName As String, ByVal DataType As ICAVCDataType) As String Implements IClient.GetChannelData
        Return ocx.GetChannelData(ChannelName, DataType)
    End Function

    Public Function GetChannelDataSize(ByVal ChannelName As String) As Integer Implements IClient.GetChannelDataSize
        Return ocx.GetChannelDataSize(ChannelName)
    End Function

    Public Function GetChannelDataType(ByVal ChannelName As String) As ICAVCDataType Implements IClient.GetChannelDataType
        Return ocx.GetChannelDataType(ChannelName)
    End Function

    Public Function GetChannelFlags(ByVal ChannelName As String) As Integer Implements IClient.GetChannelFlags
        Return ocx.GetChannelFlags(ChannelName)
    End Function

    Public Function GetChannelName(ByVal ChannelIndex As Integer) As String Implements IClient.GetChannelName
        Return ocx.GetChannelName(ChannelIndex)
    End Function

    Public Function GetChannelNumber(ByVal ChannelName As String) As Integer Implements IClient.GetChannelNumber
        Return ocx.GetChannelNumber(ChannelName)
    End Function

    Public Function GetClientAddress(ByVal Index As Integer) As String Implements IClient.GetClientAddress
        Return ocx.GetClientAddress(Index)
    End Function

    Public Function GetClientAddressCount() As Integer Implements IClient.GetClientAddressCount
        Return ocx.GetClientAddressCount
    End Function

    Public Function GetClientErrorMessage(ByVal ErrCode As Integer) As String Implements IClient.GetClientErrorMessage
        Return ocx.GetClientErrorMessage(ErrCode)
    End Function

    Public Function GetClientIdentification() As String Implements IClient.GetClientIdentification
        Return ocx.GetClientIdentification
    End Function

    Public Function GetClientNetworkName() As String Implements IClient.GetClientNetworkName
        Return ocx.GetClientNetworkName
    End Function

    Public Function GetEnumNameByIndex(ByVal hndEnum As Integer, ByVal hndIndex As Integer) As String Implements IClient.GetEnumNameByIndex
        Return ocx.GetEnumNameByIndex(hndEnum, hndIndex)
    End Function

    Public Function GetEnumNameCount(ByVal hndEnum As Integer) As Integer Implements IClient.GetEnumNameCount
        Return ocx.GetEnumNameCount(hndEnum)
    End Function

    Public Function GetErrorMessage(ByVal ErrCode As Integer) As String Implements IClient.GetErrorMessage
        Return ocx.GetErrorMessage(ErrCode)
    End Function

    Public Function GetGlobalChannelCount() As Integer Implements IClient.GetGlobalChannelCount
        Return ocx.GetGlobalChannelCount
    End Function

    Public Function GetGlobalChannelName(ByVal ChannelIndex As Integer) As String Implements IClient.GetGlobalChannelName
        Return ocx.GetGlobalChannelName(ChannelIndex)
    End Function

    Public Function GetGlobalChannelNumber(ByVal ChannelName As String) As Integer Implements IClient.GetGlobalChannelNumber
        Return ocx.GetGlobalChannelNumber(ChannelName)
    End Function

    Public Function GetInterfaceVersion() As String Implements IClient.GetInterfaceVersion
        Return ocx.GetInterfaceVersion
    End Function

    Public Function GetLastClientError() As Integer Implements IClient.GetLastClientError
        Return ocx.GetLastClientError
    End Function

    Public Function GetLastError() As Integer Implements IClient.GetLastError
        Return ocx.GetLastError
    End Function

    Public Function GetMaxChannelCount() As Integer Implements IClient.GetMaxChannelCount
        Return ocx.GetMaxChannelCount
    End Function

    Public Function GetMaxChannelRead() As Integer Implements IClient.GetMaxChannelRead
        Return ocx.GetMaxChannelRead
    End Function

    Public Function GetMaxChannelWrite() As Integer Implements IClient.GetMaxChannelWrite
        Return ocx.GetMaxChannelWrite
    End Function

    Public Function GetNotificationReason() As ICAEvent Implements IClient.GetNotificationReason
        Return ocx.GetNotificationReason
    End Function

    Public Function GetPropCount() As Integer Implements IClient.GetPropCount
        Return ocx.GetPropCount
    End Function

    Public Function GetPropNameByIndex(ByVal Index As Integer) As String Implements IClient.GetPropNameByIndex
        Return ocx.GetPropNameByIndex(Index)
    End Function

    Public Function GetPropValue(ByVal Name As String) As String Implements IClient.GetPropValue
        Return ocx.GetPropValue(Name)
    End Function

    Public Function GetPropValueByIndex(ByVal Index As Integer) As String Implements IClient.GetPropValueByIndex
        Return ocx.GetPropValueByIndex(Index)
    End Function

    Public Function GetScreenColorDepth() As Integer Implements IClient.GetScreenColorDepth
        Return ocx.GetScreenColorDepth
    End Function

    Public Function GetScreenHeight() As Integer Implements IClient.GetScreenHeight
        Return ocx.GetScreenHeight
    End Function

    Public Function GetScreenWidth() As Integer Implements IClient.GetScreenWidth
        Return ocx.GetScreenWidth
    End Function

    Public Function GetSessionColorDepth() As Integer Implements IClient.GetSessionColorDepth
        Return ocx.GetSessionColorDepth
    End Function

    Public Function GetSessionCount() As Integer Implements IClient.GetSessionCount
        Return ocx.GetSessionCount
    End Function

    Public Function GetSessionCounter(ByVal Index As ICASessionCounter) As Integer Implements IClient.GetSessionCounter
        Return ocx.GetSessionCounter(Index)
    End Function

    Public Function GetSessionGroupCount(ByVal pGroupId As String) As Integer Implements IClient.GetSessionGroupCount
        Return ocx.GetSessionGroupCount(pGroupId)
    End Function

    Public Function GetSessionHandle() As Integer Implements IClient.GetSessionHandle
        Return ocx.GetSessionHandle
    End Function

    Public Function GetSessionHandleByIndex(ByVal Index As Integer) As Integer Implements IClient.GetSessionHandleByIndex
        Return ocx.GetSessionHandleByIndex(Index)
    End Function

    Public Function GetSessionHeight() As Integer Implements IClient.GetSessionHeight
        Return ocx.GetSessionHeight
    End Function

    Public Function GetSessionString(ByVal Index As ICASessionString) As String Implements IClient.GetSessionString
        Return ocx.GetSessionString(Index)
    End Function

    Public Function GetSessionWidth() As Integer Implements IClient.GetSessionWidth
        Return ocx.GetSessionWidth
    End Function

    Public Function GetWindowHeight(ByVal WndType As ICAWindowType, ByVal WndFlags As Integer) As Integer Implements IClient.GetWindowHeight
        Return ocx.GetWindowHeight(WndType, WndFlags)
    End Function

    Public Function GetWindowWidth(ByVal WndType As ICAWindowType, ByVal WndFlags As Integer) As Integer Implements IClient.GetWindowWidth
        Return ocx.GetWindowWidth(WndType, WndFlags)
    End Function

    Public Function GetWindowXPosition(ByVal WndType As ICAWindowType, ByVal WndFlags As Integer) As Integer Implements IClient.GetWindowXPosition
        Return ocx.GetWindowXPosition(WndType, WndFlags)
    End Function

    Public Function GetWindowYPosition(ByVal WndType As ICAWindowType, ByVal WndFlags As Integer) As Integer Implements IClient.GetWindowYPosition
        Return ocx.GetWindowYPosition(WndType, WndFlags)
    End Function

    Public ReadOnly Property ClientHeight() As Integer Implements IClient.Height
        Get
            Return ocx.Height
        End Get
    End Property

    Public Function HideTitleBar() As Integer Implements IClient.HideTitleBar
        Return ocx.HideTitleBar
    End Function

    Public Function HideWindow(ByVal WndType As ICAWindowType) As Integer Implements IClient.HideWindow
        Return ocx.HideWindow(WndType)
    End Function

    Public Property Hotkey10Char() As String Implements IClient.Hotkey10Char
        Get
            Return ocx.Hotkey10Char
        End Get
        Set(ByVal value As String)
            ocx.Hotkey10Char = value
        End Set
    End Property

    Public Property Hotkey10Shift() As String Implements IClient.Hotkey10Shift
        Get
            Return ocx.Hotkey10Shift
        End Get
        Set(ByVal value As String)
            ocx.Hotkey10Shift = value
        End Set
    End Property

    Public Property Hotkey11Char() As String Implements IClient.Hotkey11Char
        Get
            Return ocx.Hotkey11Char
        End Get
        Set(ByVal value As String)
            ocx.Hotkey11Char = value
        End Set
    End Property

    Public Property Hotkey11Shift() As String Implements IClient.Hotkey11Shift
        Get
            Return ocx.Hotkey11Shift
        End Get
        Set(ByVal value As String)
            ocx.Hotkey11Shift = value
        End Set
    End Property

    Public Property Hotkey1Char() As String Implements IClient.Hotkey1Char
        Get
            Return ocx.Hotkey1Char
        End Get
        Set(ByVal value As String)
            ocx.Hotkey1Char = value
        End Set
    End Property

    Public Property Hotkey1Shift() As String Implements IClient.Hotkey1Shift
        Get
            Return ocx.Hotkey1Shift
        End Get
        Set(ByVal value As String)
            ocx.Hotkey1Shift = value
        End Set
    End Property

    Public Property Hotkey2Char() As String Implements IClient.Hotkey2Char
        Get
            Return ocx.Hotkey2Char
        End Get
        Set(ByVal value As String)
            ocx.Hotkey2Char = value
        End Set
    End Property

    Public Property Hotkey2Shift() As String Implements IClient.Hotkey2Shift
        Get
            Return ocx.Hotkey2Shift
        End Get
        Set(ByVal value As String)
            ocx.Hotkey2Shift = value
        End Set
    End Property

    Public Property Hotkey3Char() As String Implements IClient.Hotkey3Char
        Get
            Return ocx.Hotkey3Char
        End Get
        Set(ByVal value As String)
            ocx.Hotkey3Char = value
        End Set
    End Property

    Public Property Hotkey3Shift() As String Implements IClient.Hotkey3Shift
        Get
            Return ocx.Hotkey3Shift
        End Get
        Set(ByVal value As String)
            ocx.Hotkey3Shift = value
        End Set
    End Property

    Public Property Hotkey4Char() As String Implements IClient.Hotkey4Char
        Get
            Return ocx.Hotkey4Char
        End Get
        Set(ByVal value As String)
            ocx.Hotkey4Char = value
        End Set
    End Property

    Public Property Hotkey4Shift() As String Implements IClient.Hotkey4Shift
        Get
            Return ocx.Hotkey4Shift
        End Get
        Set(ByVal value As String)
            ocx.Hotkey4Shift = value
        End Set
    End Property

    Public Property Hotkey5Char() As String Implements IClient.Hotkey5Char
        Get
            Return ocx.Hotkey5Char
        End Get
        Set(ByVal value As String)
            ocx.Hotkey5Char = value
        End Set
    End Property

    Public Property Hotkey5Shift() As String Implements IClient.Hotkey5Shift
        Get
            Return ocx.Hotkey5Shift
        End Get
        Set(ByVal value As String)
            ocx.Hotkey5Shift = value
        End Set
    End Property

    Public Property Hotkey6Char() As String Implements IClient.Hotkey6Char
        Get
            Return ocx.Hotkey6Char
        End Get
        Set(ByVal value As String)
            ocx.Hotkey6Char = value
        End Set
    End Property

    Public Property Hotkey6Shift() As String Implements IClient.Hotkey6Shift
        Get
            Return ocx.Hotkey6Shift
        End Get
        Set(ByVal value As String)
            ocx.Hotkey6Shift = value
        End Set
    End Property

    Public Property Hotkey7Char() As String Implements IClient.Hotkey7Char
        Get
            Return ocx.Hotkey7Char
        End Get
        Set(ByVal value As String)
            ocx.Hotkey7Char = value
        End Set
    End Property

    Public Property Hotkey7Shift() As String Implements IClient.Hotkey7Shift
        Get
            Return ocx.Hotkey7Shift
        End Get
        Set(ByVal value As String)
            ocx.Hotkey7Shift = value
        End Set
    End Property

    Public Property Hotkey8Char() As String Implements IClient.Hotkey8Char
        Get
            Return ocx.Hotkey8Char
        End Get
        Set(ByVal value As String)
            ocx.Hotkey8Char = value
        End Set
    End Property

    Public Property Hotkey8Shift() As String Implements IClient.Hotkey8Shift
        Get
            Return ocx.Hotkey8Shift
        End Get
        Set(ByVal value As String)
            ocx.Hotkey8Shift = value
        End Set
    End Property

    Public Property Hotkey9Char() As String Implements IClient.Hotkey9Char
        Get
            Return ocx.Hotkey9Char
        End Get
        Set(ByVal value As String)
            ocx.Hotkey9Char = value
        End Set
    End Property

    Public Property Hotkey9Shift() As String Implements IClient.Hotkey9Shift
        Get
            Return ocx.Hotkey9Shift
        End Get
        Set(ByVal value As String)
            ocx.Hotkey9Shift = value
        End Set
    End Property

    Public Property HttpBrowserAddress() As String Implements IClient.HttpBrowserAddress
        Get
            Return ocx.HttpBrowserAddress
        End Get
        Set(ByVal value As String)
            ocx.HttpBrowserAddress = value
        End Set
    End Property

    Public Property ICAFile() As String Implements IClient.ICAFile
        Get
            Return ocx.ICAFile
        End Get
        Set(ByVal value As String)
            ocx.ICAFile = value
        End Set
    End Property

    Public Property ICAPortNumber() As Integer Implements IClient.ICAPortNumber
        Get
            Return ocx.ICAPortNumber
        End Get
        Set(ByVal value As Integer)
            ocx.ICAPortNumber = value
        End Set
    End Property

    Public Property ICASOCKSProtocolVersion() As Integer Implements IClient.ICASOCKSProtocolVersion
        Get
            Return ocx.ICASOCKSProtocolVersion
        End Get
        Set(ByVal value As Integer)
            ocx.ICASOCKSProtocolVersion = value
        End Set
    End Property

    Public Property ICASOCKSProxyHost() As String Implements IClient.ICASOCKSProxyHost
        Get
            Return ocx.ICASOCKSProxyHost
        End Get
        Set(ByVal value As String)
            ocx.ICASOCKSProxyHost = value
        End Set
    End Property

    Public Property ICASOCKSProxyPortNumber() As Integer Implements IClient.ICASOCKSProxyPortNumber
        Get
            Return ocx.ICASOCKSProxyPortNumber
        End Get
        Set(ByVal value As Integer)
            ocx.ICASOCKSProxyPortNumber = value
        End Set
    End Property

    Public Property ICASOCKSRFC1929Username() As String Implements IClient.ICASOCKSRFC1929Username
        Get
            Return ocx.ICASOCKSRFC1929Username
        End Get
        Set(ByVal value As String)
            ocx.ICASOCKSRFC1929Username = value
        End Set
    End Property

    Public Property ICASOCKSTimeout() As Integer Implements IClient.ICASOCKSTimeout
        Get
            Return ocx.ICASOCKSTimeout
        End Get
        Set(ByVal value As Integer)
            ocx.ICASOCKSTimeout = value
        End Set
    End Property

    Public Property IconIndex() As Integer Implements IClient.IconIndex
        Get
            Return ocx.IconIndex
        End Get
        Set(ByVal value As Integer)
            ocx.IconIndex = value
        End Set
    End Property

    Public Property IconPath() As String Implements IClient.IconPath
        Get
            Return ocx.IconPath
        End Get
        Set(ByVal value As String)
            ocx.IconPath = value
        End Set
    End Property

    Public Property InitialProgram() As String Implements IClient.InitialProgram
        Get
            Return ocx.InitialProgram
        End Get
        Set(ByVal value As String)
            ocx.InitialProgram = value
        End Set
    End Property

    Public Property IPCLaunch() As Boolean Implements IClient.IPCLaunch
        Get
            Return ocx.IPCLaunch
        End Get
        Set(ByVal value As Boolean)
            ocx.IPCLaunch = value
        End Set
    End Property

    Public Property IPXBrowserAddress() As String Implements IClient.IPXBrowserAddress
        Get
            Return ocx.IPXBrowserAddress
        End Get
        Set(ByVal value As String)
            ocx.IPXBrowserAddress = value
        End Set
    End Property

    Public Function IsConnected() As Boolean Implements IClient.IsConnected
        Return ocx.IsConnected
    End Function

    Public Function IsKeyboardInputEnabled() As Boolean Implements IClient.IsKeyboardInputEnabled
        Return ocx.IsKeyboardInputEnabled
    End Function

    Public Function IsMouseInputEnabled() As Boolean Implements IClient.IsMouseInputEnabled
        Return ocx.IsMouseInputEnabled
    End Function

    Public Function IsPassThrough() As Boolean Implements IClient.IsPassThrough
        Return ocx.IsPassThrough
    End Function

    Public Function IsSessionAttached(ByVal pSessionId As String) As Boolean Implements IClient.IsSessionAttached
        Return ocx.IsSessionAttached(pSessionId)
    End Function

    Public Function IsSessionDetached(ByVal pSessionId As String) As Boolean Implements IClient.IsSessionDetached
        Return ocx.IsSessionDetached(pSessionId)
    End Function

    Public Function IsSessionRunning(ByVal pSessionId As String) As Boolean Implements IClient.IsSessionRunning
        Return ocx.IsSessionRunning(pSessionId)
    End Function

    Public Function IsWindowDocked() As Boolean Implements IClient.IsWindowDocked
        Return ocx.IsWindowDocked
    End Function

    Public Property KeyboardTimer() As Integer Implements IClient.KeyboardTimer
        Get
            Return ocx.KeyboardTimer
        End Get
        Set(ByVal value As Integer)
            ocx.KeyboardTimer = value
        End Set
    End Property

    Public Property LanaNumber() As Integer Implements IClient.LanaNumber
        Get
            Return ocx.LanaNumber
        End Get
        Set(ByVal value As Integer)
            ocx.LanaNumber = value
        End Set
    End Property

    Public Property Launch() As Boolean Implements IClient.Launch
        Get
            Return ocx.Launch
        End Get
        Set(ByVal value As Boolean)
            ocx.Launch = value
        End Set
    End Property

    Public Sub LoadIcaFile(ByVal File As String) Implements IClient.LoadIcaFile
        ocx.LoadIcaFile(File)
    End Sub

    Public Property LocHTTPBrowserAddress() As String Implements IClient.LocHTTPBrowserAddress
        Get
            Return ocx.LocHTTPBrowserAddress
        End Get
        Set(ByVal value As String)
            ocx.LocHTTPBrowserAddress = value
        End Set
    End Property

    Public Property LocIPXBrowserAddress() As String Implements IClient.LocIPXBrowserAddress
        Get
            Return ocx.LocIPXBrowserAddress
        End Get
        Set(ByVal value As String)
            ocx.LocIPXBrowserAddress = value
        End Set
    End Property

    Public Property LocNETBIOSBrowserAddress() As String Implements IClient.LocNETBIOSBrowserAddress
        Get
            Return ocx.LocNETBIOSBrowserAddress
        End Get
        Set(ByVal value As String)
            ocx.LocNETBIOSBrowserAddress = value
        End Set
    End Property

    Public Property LocTCPBrowserAddress() As String Implements IClient.LocTCPBrowserAddress
        Get
            Return ocx.LocTCPBrowserAddress
        End Get
        Set(ByVal value As String)
            ocx.LocTCPBrowserAddress = value
        End Set
    End Property

    Public Property LogAppend() As Boolean Implements IClient.LogAppend
        Get
            Return ocx.LogAppend
        End Get
        Set(ByVal value As Boolean)
            ocx.LogAppend = value
        End Set
    End Property

    Public Property LogConnect() As Boolean Implements IClient.LogConnect
        Get
            Return ocx.LogConnect
        End Get
        Set(ByVal value As Boolean)
            ocx.LogConnect = value
        End Set
    End Property

    Public Property LogErrors() As Boolean Implements IClient.LogErrors
        Get
            Return ocx.LogErrors
        End Get
        Set(ByVal value As Boolean)
            ocx.LogErrors = value
        End Set
    End Property

    Public Property LogFile() As String Implements IClient.LogFile
        Get
            Return ocx.LogFile
        End Get
        Set(ByVal value As String)
            ocx.LogFile = value
        End Set
    End Property

    Public Property LogFlush() As Boolean Implements IClient.LogFlush
        Get
            Return ocx.LogFlush
        End Get
        Set(ByVal value As Boolean)
            ocx.LogFlush = value
        End Set
    End Property

    Public Property LogKeyboard() As Boolean Implements IClient.LogKeyboard
        Get
            Return ocx.LogKeyboard
        End Get
        Set(ByVal value As Boolean)
            ocx.LogKeyboard = value
        End Set
    End Property

    Public Sub Logoff() Implements IClient.Logoff
        ocx.Logoff()
    End Sub

    Public Function LogoffSessions(ByVal pGroupId As String) As Integer Implements IClient.LogoffSessions
        Return ocx.LogoffSessions(pGroupId)
    End Function

    Public Property LogReceive() As Boolean Implements IClient.LogReceive
        Get
            Return ocx.LogReceive
        End Get
        Set(ByVal value As Boolean)
            ocx.LogReceive = value
        End Set
    End Property

    Public Property LogTransmit() As Boolean Implements IClient.LogTransmit
        Get
            Return ocx.LogTransmit
        End Get
        Set(ByVal value As Boolean)
            ocx.LogTransmit = value
        End Set
    End Property

    Public Property LongCommandLine() As String Implements IClient.LongCommandLine
        Get
            Return ocx.LongCommandLine
        End Get
        Set(ByVal value As String)
            ocx.LongCommandLine = value
        End Set
    End Property

    Public Function MaximizeWindow() As Integer Implements IClient.MaximizeWindow
        Return ocx.MaximizeWindow
    End Function

    Public Function MinimizeWindow() As Integer Implements IClient.MinimizeWindow
        Return ocx.MinimizeWindow
    End Function

    Public Property ModuleIni() As String Implements IClient.ModuleIni
        Get
            Return ocx.ModuleIni
        End Get
        Set(ByVal value As String)
            ocx.ModuleIni = value
        End Set
    End Property

    Public Property MouseTimer() As Integer Implements IClient.MouseTimer
        Get
            Return ocx.MouseTimer
        End Get
        Set(ByVal value As Integer)
            ocx.MouseTimer = value
        End Set
    End Property

    Public Property NetbiosBrowserAddress() As String Implements IClient.NetbiosBrowserAddress
        Get
            Return ocx.NetbiosBrowserAddress
        End Get
        Set(ByVal value As String)
            ocx.NetbiosBrowserAddress = value
        End Set
    End Property

    Public Function NewWindow(ByVal XPos As Integer, ByVal YPos As Integer, ByVal Width As Integer, ByVal Height As Integer, ByVal Flags As Integer) As Integer Implements IClient.NewWindow
        Return ocx.NewWindow(XPos, YPos, Width, Height, Flags)
    End Function

    Public ReadOnly Property NotificationReason() As ICAEvent Implements IClient.NotificationReason
        Get
            Return ocx.NotificationReason
        End Get
    End Property

    Public Property OutputMode() As OutputMode Implements IClient.OutputMode
        Get
            Return ocx.OutputMode
        End Get
        Set(ByVal value As OutputMode)
            ocx.OutputMode = value
        End Set
    End Property

    Public Property PersistentCacheEnabled() As Boolean Implements IClient.PersistentCacheEnabled
        Get
            Return ocx.PersistentCacheEnabled
        End Get
        Set(ByVal value As Boolean)
            ocx.PersistentCacheEnabled = value
        End Set
    End Property

    Public Function PlaceWindowOnBottom() As Integer Implements IClient.PlaceWindowOnBottom
        Return ocx.PlaceWindowOnBottom
    End Function

    Public Function PlaceWindowOnTop() As Integer Implements IClient.PlaceWindowOnTop
        Return ocx.PlaceWindowOnTop
    End Function

    Public Property ProtocolSupport() As String Implements IClient.ProtocolSupport
        Get
            Return ocx.ProtocolSupport
        End Get
        Set(ByVal value As String)
            ocx.ProtocolSupport = value
        End Set
    End Property

    Public Property ReadyState() As Integer Implements IClient.ReadyState
        Get
            Return ocx.ReadyState
        End Get
        Set(ByVal value As Integer)
            ocx.ReadyState = value
        End Set
    End Property

    Public Property Reliable() As Boolean Implements IClient.Reliable
        Get
            Return ocx.Reliable
        End Get
        Set(ByVal value As Boolean)
            ocx.Reliable = value
        End Set
    End Property

    Public Sub ResetProps() Implements IClient.ResetProps
        ocx.ResetProps()
    End Sub

    Public Function RestoreWindow() As Integer Implements IClient.RestoreWindow
        Return ocx.RestoreWindow
    End Function

    Public Sub RunPublishedApplication(ByVal AppName As String, ByVal Arguments As String) Implements IClient.RunPublishedApplication
        ocx.RunPublishedApplication(AppName, Arguments)
    End Sub

    Public Function ScaleDialog() As Integer Implements IClient.ScaleDialog
        Return ocx.ScaleDialog
    End Function

    Public Function ScaleDisable() As Integer Implements IClient.ScaleDisable
        Return ocx.ScaleDisable
    End Function

    Public Function ScaleDown() As Integer Implements IClient.ScaleDown
        Return ocx.ScaleDown
    End Function

    Public Function ScaleEnable() As Integer Implements IClient.ScaleEnable
        Return ocx.ScaleEnable
    End Function

    Public Function ScalePercent(ByVal Percent As Integer) As Integer Implements IClient.ScalePercent
        Return ocx.ScalePercent(Percent)
    End Function

    Public Function ScaleSize(ByVal Width As Integer, ByVal Height As Integer) As Integer Implements IClient.ScaleSize
        Return ocx.ScaleSize(Width, Height)
    End Function

    Public Function ScaleToFit() As Integer Implements IClient.ScaleToFit
        Return ocx.ScaleToFit
    End Function

    Public Function ScaleUp() As Integer Implements IClient.ScaleUp
        Return ocx.ScaleUp
    End Function

    Public Property ScalingHeight() As Integer Implements IClient.ScalingHeight
        Get
            Return ocx.ScalingHeight
        End Get
        Set(ByVal value As Integer)
            ocx.ScalingHeight = value
        End Set
    End Property

    Public Property ScalingMode() As ICAScalingMode Implements IClient.ScalingMode
        Get
            Return ocx.ScalingMode
        End Get
        Set(ByVal value As ICAScalingMode)
            ocx.ScalingMode = value
        End Set
    End Property

    Public Property ScalingPercent() As Integer Implements IClient.ScalingPercent
        Get
            Return ocx.ScalingPercent
        End Get
        Set(ByVal value As Integer)
            ocx.ScalingPercent = value
        End Set
    End Property

    Public Property ScalingWidth() As Integer Implements IClient.ScalingWidth
        Get
            Return ocx.ScalingWidth
        End Get
        Set(ByVal value As Integer)
            ocx.ScalingWidth = value
        End Set
    End Property

    Public Property ScreenPercent() As Integer Implements IClient.ScreenPercent
        Get
            Return ocx.ScreenPercent
        End Get
        Set(ByVal value As Integer)
            ocx.ScreenPercent = value
        End Set
    End Property

    Public Property Scrollbars() As Boolean Implements IClient.Scrollbars
        Get
            Return ocx.Scrollbars
        End Get
        Set(ByVal value As Boolean)
            ocx.Scrollbars = value
        End Set
    End Property

    Public Function SendChannelData(ByVal ChannelName As String, ByVal Data As String, ByVal DataSize As Integer, ByVal DataType As ICAVCDataType) As Integer Implements IClient.SendChannelData
        Return ocx.SendChannelData(ChannelName, Data, DataSize, DataType)
    End Function

    Public ReadOnly Property Session() As ISession Implements IClient.Session
        Get
            Return ocx.Session
        End Get
    End Property

    Public Property SessionCacheEnable() As Boolean Implements IClient.SessionCacheEnable
        Get
            Return ocx.SessionCacheEnable
        End Get
        Set(ByVal value As Boolean)
            ocx.SessionCacheEnable = value
        End Set
    End Property

    Public Property SessionCacheTimeout() As Integer Implements IClient.SessionCacheTimeout
        Get
            Return ocx.SessionCacheTimeout
        End Get
        Set(ByVal value As Integer)
            ocx.SessionCacheTimeout = value
        End Set
    End Property

    Public Property SessionEndAction() As ICASessionEndAction Implements IClient.SessionEndAction
        Get
            Return ocx.SessionEndAction
        End Get
        Set(ByVal value As ICASessionEndAction)
            ocx.SessionEndAction = value
        End Set
    End Property

    Public Property SessionExitTimeout() As Integer Implements IClient.SessionExitTimeout
        Get
            Return ocx.SessionExitTimeout
        End Get
        Set(ByVal value As Integer)
            ocx.SessionExitTimeout = value
        End Set
    End Property

    Public Property SessionSharingKey() As String Implements IClient.SessionSharingKey
        Get
            Return ocx.SessionSharingKey
        End Get
        Set(ByVal value As String)
            ocx.SessionSharingKey = value
        End Set
    End Property

    Public Property SessionSharingLaunchOnly() As Boolean Implements IClient.SessionSharingLaunchOnly
        Get
            Return ocx.SessionSharingLaunchOnly
        End Get
        Set(ByVal value As Boolean)
            ocx.SessionSharingLaunchOnly = value
        End Set
    End Property

    Public Property SessionSharingName() As String Implements IClient.SessionSharingName
        Get
            Return ocx.SessionSharingName
        End Get
        Set(ByVal value As String)
            ocx.SessionSharingName = value
        End Set
    End Property

    Public Function SetChannelFlags(ByVal ChannelName As String, ByVal Flags As Integer) As Integer Implements IClient.SetChannelFlags
        Return ocx.SetChannelFlags(ChannelName, Flags)
    End Function

    Public Sub SetProp(ByVal Name As String, ByVal Value As String) Implements IClient.SetProp
        ocx.SetProp(Name, Value)
    End Sub

    Public Sub SetSessionEndAction(ByVal Action As ICASessionEndAction) Implements IClient.SetSessionEndAction
        ocx.SetSessionEndAction(Action)
    End Sub

    Public Function SetSessionGroupId(ByVal pGroupId As String) As Integer Implements IClient.SetSessionGroupId
        Return ocx.SetSessionGroupId(pGroupId)
    End Function

    Public Function SetSessionId(ByVal pSessionId As String) As Integer Implements IClient.SetSessionId
        Return ocx.SetSessionId(pSessionId)
    End Function

    Public Function SetWindowPosition(ByVal WndType As ICAWindowType, ByVal XPos As Integer, ByVal YPos As Integer, ByVal WndFlags As Integer) As Integer Implements IClient.SetWindowPosition
        Return ocx.SetWindowPosition(WndType, XPos, YPos, WndFlags)
    End Function

    Public Function SetWindowSize(ByVal WndType As ICAWindowType, ByVal Width As Integer, ByVal Height As Integer, ByVal WndFlags As Integer) As Integer Implements IClient.SetWindowSize
        Return ocx.SetWindowSize(WndType, Width, Height, WndFlags)
    End Function

    Public Function ShowTitleBar() As Integer Implements IClient.ShowTitleBar
        Return ocx.ShowTitleBar
    End Function

    Public Property SSLCiphers() As String Implements IClient.SSLCiphers
        Get
            Return ocx.SSLCiphers
        End Get
        Set(ByVal value As String)
            ocx.SSLCiphers = value
        End Set
    End Property

    Public Property SSLCommonName() As String Implements IClient.SSLCommonName
        Get
            Return ocx.SSLCommonName
        End Get
        Set(ByVal value As String)
            ocx.SSLCommonName = value
        End Set
    End Property

    Public Property SSLEnable() As Boolean Implements IClient.SSLEnable
        Get
            Return ocx.SSLEnable
        End Get
        Set(ByVal value As Boolean)
            ocx.SSLEnable = value
        End Set
    End Property

    Public Property SSLNoCACerts() As Integer Implements IClient.SSLNoCACerts
        Get
            Return ocx.SSLNoCACerts
        End Get
        Set(ByVal value As Integer)
            ocx.SSLNoCACerts = value
        End Set
    End Property

    Public Property SSLProxyHost() As String Implements IClient.SSLProxyHost
        Get
            Return ocx.SSLProxyHost
        End Get
        Set(ByVal value As String)
            ocx.SSLProxyHost = value
        End Set
    End Property

    Public Property Start() As Boolean Implements IClient.Start
        Get
            Return ocx.Start
        End Get
        Set(ByVal value As Boolean)
            ocx.Start = value
        End Set
    End Property

    Public Sub Startup() Implements IClient.Startup
        ocx.Startup()
    End Sub

    Public Function SwitchSession(ByVal hSession As Integer) As Integer Implements IClient.SwitchSession
        Return ocx.SwitchSession(hSession)
    End Function

    Public Property ClientTabStop() As Boolean Implements IClient.TabStop
        Get
            Return ocx.TabStop
        End Get
        Set(ByVal value As Boolean)
            ocx.TabStop = value
        End Set
    End Property

    Public Property TCPBrowserAddress() As String Implements IClient.TCPBrowserAddress
        Get
            Return ocx.TCPBrowserAddress
        End Get
        Set(ByVal value As String)
            ocx.TCPBrowserAddress = value
        End Set
    End Property

    Public Property TextColor() As UInteger Implements IClient.TextColor
        Get
            Return ocx.TextColor
        End Get
        Set(ByVal value As UInteger)
            ocx.TextColor = value
        End Set
    End Property

    Public Property Title() As String Implements IClient.Title
        Get
            Return ocx.Title
        End Get
        Set(ByVal value As String)
            ocx.Title = value
        End Set
    End Property

    Public Property TransportDriver() As String Implements IClient.TransportDriver
        Get
            Return ocx.TransportDriver
        End Get
        Set(ByVal value As String)
            ocx.TransportDriver = value
        End Set
    End Property

    Public Property TransportReconnectDelay() As Integer Implements IClient.TransportReconnectDelay
        Get
            Return ocx.TransportReconnectDelay
        End Get
        Set(ByVal value As Integer)
            ocx.TransportReconnectDelay = value
        End Set
    End Property

    Public Property TransportReconnectEnabled() As Boolean Implements IClient.TransportReconnectEnabled
        Get
            Return ocx.TransportReconnectEnabled
        End Get
        Set(ByVal value As Boolean)
            ocx.TransportReconnectEnabled = value
        End Set
    End Property

    Public Property TransportReconnectRetries() As Integer Implements IClient.TransportReconnectRetries
        Get
            Return ocx.TransportReconnectRetries
        End Get
        Set(ByVal value As Integer)
            ocx.TransportReconnectRetries = value
        End Set
    End Property

    Public Property TWIDisableSessionSharing() As Boolean Implements IClient.TWIDisableSessionSharing
        Get
            Return ocx.TWIDisableSessionSharing
        End Get
        Set(ByVal value As Boolean)
            ocx.TWIDisableSessionSharing = value
        End Set
    End Property

    Public Property TWIMode() As Boolean Implements IClient.TWIMode
        Get
            Return ocx.TWIMode
        End Get
        Set(ByVal value As Boolean)
            ocx.TWIMode = value
        End Set
    End Property

    Public Property UIActive() As Boolean Implements IClient.UIActive
        Get
            Return ocx.UIActive
        End Get
        Set(ByVal value As Boolean)
            ocx.UIActive = value
        End Set
    End Property

    Public Function UndockWindow() As Integer Implements IClient.UndockWindow
        Return ocx.UndockWindow
    End Function

    Public Property UpdatesAllowed() As Boolean Implements IClient.UpdatesAllowed
        Get
            Return ocx.UpdatesAllowed
        End Get
        Set(ByVal value As Boolean)
            ocx.UpdatesAllowed = value
        End Set
    End Property

    Public Property UseAlternateAddress() As Integer Implements IClient.UseAlternateAddress
        Get
            Return ocx.UseAlternateAddress
        End Get
        Set(ByVal value As Integer)
            ocx.UseAlternateAddress = value
        End Set
    End Property

    Public Property Username() As String Implements IClient.Username
        Get
            Return ocx.Username
        End Get
        Set(ByVal value As String)
            ocx.Username = value
        End Set
    End Property

    Public ReadOnly Property Version() As String Implements IClient.Version
        Get
            Return ocx.Version
        End Get
    End Property

    Public Property VirtualChannels() As String Implements IClient.VirtualChannels
        Get
            Return ocx.VirtualChannels
        End Get
        Set(ByVal value As String)
            ocx.VirtualChannels = value
        End Set
    End Property

    Public Property VirtualCOMPortEmulation() As Boolean Implements IClient.VirtualCOMPortEmulation
        Get
            Return ocx.VirtualCOMPortEmulation
        End Get
        Set(ByVal value As Boolean)
            ocx.VirtualCOMPortEmulation = value
        End Set
    End Property

    Public Property VSLAllowed() As Boolean Implements IClient.VSLAllowed
        Get
            Return ocx.VSLAllowed
        End Get
        Set(ByVal value As Boolean)
            ocx.VSLAllowed = value
        End Set
    End Property

    Public Property WfclientIni() As String Implements IClient.WfclientIni
        Get
            Return ocx.WfclientIni
        End Get
        Set(ByVal value As String)
            ocx.WfclientIni = value
        End Set
    End Property

    Public ReadOnly Property ClientWidth() As Integer Implements IClient.Width
        Get
            Return ocx.Width
        End Get
    End Property

    Public Property WinstationDriver() As String Implements IClient.WinstationDriver
        Get
            Return ocx.WinstationDriver
        End Get
        Set(ByVal value As String)
            ocx.WinstationDriver = value
        End Set
    End Property

    Public Property WorkDirectory() As String Implements IClient.WorkDirectory
        Get
            Return ocx.WorkDirectory
        End Get
        Set(ByVal value As String)
            ocx.WorkDirectory = value
        End Set
    End Property

    Public Property XmlAddressResolutionType() As String Implements IClient.XmlAddressResolutionType
        Get
            Return ocx.XmlAddressResolutionType
        End Get
        Set(ByVal value As String)
            ocx.XmlAddressResolutionType = value
        End Set
    End Property
End Class
