Imports System.Runtime.InteropServices
Imports System.Runtime.CompilerServices

<ComImport(), TypeLibType(CShort(4160)), Guid(modClassID.IClient)> _
Public Interface IClient
    <DispId(-516)> _
    Property TabStop() As Boolean
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(-552)> _
    Sub AboutBox()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(1)> _
    Sub ClearProps()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(2)> _
    Function GetPropCount() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(3)> _
    Sub DeleteProp(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal Name As String)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(4)> _
    Sub DeletePropByIndex(<[In]()> ByVal Index As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(5)> _
    Function GetPropNameByIndex(<[In]()> ByVal Index As Integer) As <MarshalAs(UnmanagedType.BStr)> String
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(6)> _
    Sub ResetProps()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(7)> _
    Sub SetProp(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal Name As String, <[In](), MarshalAs(UnmanagedType.BStr)> ByVal Value As String)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(8)> _
    Function GetPropValue(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal Name As String) As <MarshalAs(UnmanagedType.BStr)> String
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(9)> _
    Function GetPropValueByIndex(<[In]()> ByVal Index As Integer) As <MarshalAs(UnmanagedType.BStr)> String
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(10)> _
    Sub Connect()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(11)> _
    Sub Disconnect()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(12)> _
    Sub Logoff()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(13)> _
    Sub LoadIcaFile(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal File As String)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(14)> _
    Sub RunPublishedApplication(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal AppName As String, <[In](), MarshalAs(UnmanagedType.BStr)> ByVal Arguments As String)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(15)> _
    Sub SetSessionEndAction(<[In]()> ByVal Action As ICASessionEndAction)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(16)> _
    Function IsConnected() As Boolean
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(17)> _
    Function GetInterfaceVersion() As <MarshalAs(UnmanagedType.BStr)> String
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(18)> _
    Function GetClientIdentification() As <MarshalAs(UnmanagedType.BStr)> String
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(19)> _
    Function GetSessionString(<[In]()> ByVal Index As ICASessionString) As <MarshalAs(UnmanagedType.BStr)> String
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(20)> _
    Function GetSessionCounter(<[In]()> ByVal Index As ICASessionCounter) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(21)> _
    Function GetNotificationReason() As ICAEvent
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(22)> _
    Sub Startup()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(23)> _
    Function GetLastError() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(24)> _
    Function GetLastClientError() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(25)> _
    Function ScaleEnable() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(26)> _
    Function ScaleDisable() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(27)> _
    Function ScaleUp() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(28)> _
    Function ScaleDown() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(29)> _
    Function ScaleSize(<[In]()> ByVal Width As Integer, <[In]()> ByVal Height As Integer) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(30)> _
    Function ScalePercent(<[In]()> ByVal Percent As Integer) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(31)> _
    Function ScaleToFit() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(32)> _
    Function ScaleDialog() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(33)> _
    Function CreateChannels(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal ChannelNames As String) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(34)> _
    Function SendChannelData(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal ChannelName As String, <[In](), MarshalAs(UnmanagedType.BStr)> ByVal Data As String, <[In]()> ByVal DataSize As Integer, <[In]()> ByVal DataType As ICAVCDataType) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(35)> _
    Function GetChannelCount() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(36)> _
    Function GetChannelName(<[In]()> ByVal ChannelIndex As Integer) As <MarshalAs(UnmanagedType.BStr)> String
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(37)> _
    Function GetChannelNumber(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal ChannelName As String) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(38)> _
    Function GetGlobalChannelCount() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(39)> _
    Function GetGlobalChannelName(<[In]()> ByVal ChannelIndex As Integer) As <MarshalAs(UnmanagedType.BStr)> String
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(40)> _
    Function GetGlobalChannelNumber(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal ChannelName As String) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(41)> _
    Function GetMaxChannelCount() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(42)> _
    Function GetMaxChannelWrite() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(43)> _
    Function GetMaxChannelRead() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(44)> _
    Function SetChannelFlags(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal ChannelName As String, <[In]()> ByVal Flags As Integer) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(45)> _
    Function GetChannelFlags(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal ChannelName As String) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(46)> _
    Function GetChannelDataSize(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal ChannelName As String) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(47)> _
    Function GetChannelDataType(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal ChannelName As String) As ICAVCDataType
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(48)> _
    Function GetChannelData(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal ChannelName As String, <[In]()> ByVal DataType As ICAVCDataType) As <MarshalAs(UnmanagedType.BStr)> String
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(49)> _
    Function EnumerateServers() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(50)> _
    Function EnumerateApplications() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(51)> _
    Function EnumerateFarms() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(52)> _
    Function GetEnumNameCount(<[In]()> ByVal hndEnum As Integer) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(53)> _
    Function GetEnumNameByIndex(<[In]()> ByVal hndEnum As Integer, <[In]()> ByVal hndIndex As Integer) As <MarshalAs(UnmanagedType.BStr)> String
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(54)> _
    Function CloseEnumHandle(<[In]()> ByVal hndEnum As Integer) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(56)> _
    Function GetWindowWidth(<[In]()> ByVal WndType As ICAWindowType, <[In]()> ByVal WndFlags As Integer) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(57)> _
    Function GetWindowHeight(<[In]()> ByVal WndType As ICAWindowType, <[In]()> ByVal WndFlags As Integer) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(58)> _
    Function SetWindowSize(<[In]()> ByVal WndType As ICAWindowType, <[In]()> ByVal Width As Integer, <[In]()> ByVal Height As Integer, <[In]()> ByVal WndFlags As Integer) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(59)> _
    Function GetWindowXPosition(<[In]()> ByVal WndType As ICAWindowType, <[In]()> ByVal WndFlags As Integer) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(60)> _
    Function GetWindowYPosition(<[In]()> ByVal WndType As ICAWindowType, <[In]()> ByVal WndFlags As Integer) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(61)> _
    Function SetWindowPosition(<[In]()> ByVal WndType As ICAWindowType, <[In]()> ByVal XPos As Integer, <[In]()> ByVal YPos As Integer, <[In]()> ByVal WndFlags As Integer) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(62)> _
    Function DisplayWindow(<[In]()> ByVal WndType As ICAWindowType) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(63)> _
    Function HideWindow(<[In]()> ByVal WndType As ICAWindowType) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(64)> _
    Function UndockWindow() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(65)> _
    Function DockWindow() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(66)> _
    Function PlaceWindowOnTop() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(67)> _
    Function PlaceWindowOnBottom() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(68)> _
    Function MinimizeWindow() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(69)> _
    Function MaximizeWindow() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(70)> _
    Function RestoreWindow() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(71)> _
    Function ShowTitleBar() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(72)> _
    Function HideTitleBar() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(73)> _
    Function EnableSizingBorder() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(74)> _
    Function DisableSizingBorder() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(75)> _
    Function FullScreenWindow() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(76)> _
    Function FocusWindow() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(77)> _
    Function IsWindowDocked() As Boolean
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(78)> _
    Function GetSessionWidth() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(79)> _
    Function GetSessionHeight() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(80)> _
    Function GetSessionColorDepth() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(81)> _
    Function GetScreenWidth() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(82)> _
    Function GetScreenHeight() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(83)> _
    Function GetScreenColorDepth() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(84)> _
    Function NewWindow(<[In]()> ByVal XPos As Integer, <[In]()> ByVal YPos As Integer, <[In]()> ByVal Width As Integer, <[In]()> ByVal Height As Integer, <[In]()> ByVal Flags As Integer) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(85)> _
    Function DeleteWindow() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(86)> _
    Function GetErrorMessage(<[In]()> ByVal ErrCode As Integer) As <MarshalAs(UnmanagedType.BStr)> String
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(87)> _
    Function GetClientErrorMessage(<[In]()> ByVal ErrCode As Integer) As <MarshalAs(UnmanagedType.BStr)> String
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(88)> _
    Function EnableKeyboardInput() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(89)> _
    Function DisableKeyboardInput() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(90)> _
    Function IsKeyboardInputEnabled() As Boolean
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(91)> _
    Function EnableMouseInput() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(92)> _
    Function DisableMouseInput() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(93)> _
    Function IsMouseInputEnabled() As Boolean
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(94)> _
    Function GetClientNetworkName() As <MarshalAs(UnmanagedType.BStr)> String
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(95)> _
    Function GetClientAddressCount() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(96)> _
    Function GetClientAddress(<[In]()> ByVal Index As Integer) As <MarshalAs(UnmanagedType.BStr)> String
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(97)> _
    Function AttachSession(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal pSessionId As String) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(98)> _
    Function DetachSession(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal pSessionId As String) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(99)> _
    Function GetCachedSessionCount() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(100)> _
    Function IsSessionAttached(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal pSessionId As String) As Boolean
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(101)> _
    Function IsSessionDetached(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal pSessionId As String) As Boolean
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(102)> _
    Function IsSessionRunning(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal pSessionId As String) As Boolean
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(103)> _
    Function SetSessionId(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal pSessionId As String) As Integer
    <DispId(-525)> _
    Property ReadyState() As Integer
    <DispId(1024)> _
    Property Address() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1025)> _
    Property Application() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1026)> _
    Property AudioBandwidthLimit() As ICASoundQuality
    <DispId(1027)> _
    Property Border() As Integer
    <DispId(1028)> _
    Property CDMAllowed() As Boolean
    <DispId(1029)> _
    Property ClientAudio() As Boolean
    <DispId(1030)> _
    Property ClientName() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1031)> _
    Property COMAllowed() As Boolean
    <DispId(1032)> _
    Property Compress() As Boolean
    <DispId(1033)> _
    ReadOnly Property Connected() As Boolean
    <DispId(1034)> _
    Property ConnectionEntry() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1035)> _
    Property CPMAllowed() As Boolean
    <DispId(1036)> _
    Property CustomMessage() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1037)> _
    Property Description() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1038)> _
    Property DesiredColor() As ICAColorDepth
    <DispId(1039)> _
    Property DesiredHRes() As Integer
    <DispId(1040)> _
    Property DesiredVRes() As Integer
    <DispId(1041)> _
    Property Domain() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1042)> _
    Property Encrypt() As Boolean
    <DispId(1043)> _
    ReadOnly Property Height() As Integer
    <DispId(1044)> _
    Property ICAFile() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1045)> _
    Property IconIndex() As Integer
    <DispId(1046)> _
    Property IconPath() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1047)> _
    Property InitialProgram() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1048)> _
    Property IPXBrowserAddress() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1049)> _
    Property NetbiosBrowserAddress() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1050)> _
    ReadOnly Property NotificationReason() As ICAEvent
    <DispId(1051)> _
    Property PersistentCacheEnabled() As Boolean
    <DispId(1052)> _
    Property ProtocolSupport() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1053)> _
    Property Reliable() As Boolean
    <DispId(1054)> _
    Property SessionEndAction() As ICASessionEndAction
    <DispId(1055)> _
    Property Start() As Boolean
    <DispId(1056)> _
    Property TCPBrowserAddress() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1057)> _
    Property TransportDriver() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1058)> _
    Property UIActive() As Boolean
    <DispId(1059)> _
    Property UpdatesAllowed() As Boolean
    <DispId(1060)> _
    Property Username() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1061)> _
    ReadOnly Property Version() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1062)> _
    Property VSLAllowed() As Boolean
    <DispId(1063)> _
    ReadOnly Property Width() As Integer
    <DispId(1064)> _
    Property WinstationDriver() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1065)> _
    Property WorkDirectory() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1068)> _
    Property AppsrvIni() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1069)> _
    Property ModuleIni() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1070)> _
    Property WfclientIni() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1071)> _
    ReadOnly Property ClientPath() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1072)> _
    ReadOnly Property ClientVersion() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1075)> _
    Property LogAppend() As Boolean
    <DispId(1076)> _
    Property LogConnect() As Boolean
    <DispId(1077)> _
    Property LogErrors() As Boolean
    <DispId(1078)> _
    Property LogFile() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1079)> _
    Property LogFlush() As Boolean
    <DispId(1080)> _
    Property LogKeyboard() As Boolean
    <DispId(1081)> _
    Property LogReceive() As Boolean
    <DispId(1082)> _
    Property LogTransmit() As Boolean
    <DispId(1073)> _
    Property Title() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1085)> _
    Property Launch() As Boolean
    <ComAliasName("stdole.OLE_COLOR"), DispId(1087)> _
    Property BackgroundColor() As <ComAliasName("stdole.OLE_COLOR")> UInt32
    <DispId(1088), ComAliasName("stdole.OLE_COLOR")> _
    Property BorderColor() As <ComAliasName("stdole.OLE_COLOR")> UInt32
    <DispId(1089), ComAliasName("stdole.OLE_COLOR")> _
    Property TextColor() As <ComAliasName("stdole.OLE_COLOR")> UInt32
    <DispId(1090)> _
    Property EncryptionLevelSession() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1091)> _
    Property HttpBrowserAddress() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1102)> _
    Property BrowserProtocol() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1103)> _
    Property LocHTTPBrowserAddress() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1104)> _
    Property LocIPXBrowserAddress() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1105)> _
    Property LocNETBIOSBrowserAddress() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1106)> _
    Property LocTCPBrowserAddress() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1107)> _
    Property DoNotUseDefaultCSL() As Boolean
    <DispId(1108)> _
    Property ICAPortNumber() As Integer
    <DispId(1109)> _
    Property KeyboardTimer() As Integer
    <DispId(1110)> _
    Property MouseTimer() As Integer
    <DispId(1111)> _
    Property Scrollbars() As Boolean
    <DispId(1112)> _
    Property ScalingHeight() As Integer
    <DispId(1113)> _
    Property ScalingMode() As ICAScalingMode
    <DispId(1114)> _
    Property ScalingPercent() As Integer
    <DispId(1115)> _
    Property ScalingWidth() As Integer
    <DispId(1116)> _
    Property VirtualChannels() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1117)> _
    Property UseAlternateAddress() As Integer
    <DispId(1118)> _
    Property BrowserRetry() As Integer
    <DispId(1119)> _
    Property BrowserTimeout() As Integer
    <DispId(1120)> _
    Property LanaNumber() As Integer
    <DispId(1121)> _
    Property ICASOCKSProtocolVersion() As Integer
    <DispId(1122)> _
    Property ICASOCKSProxyHost() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1123)> _
    Property ICASOCKSProxyPortNumber() As Integer
    <DispId(1124)> _
    Property ICASOCKSRFC1929Username() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1126)> _
    Property ICASOCKSTimeout() As Integer
    <DispId(1127)> _
    Property SSLEnable() As Boolean
    <DispId(1128)> _
    Property SSLProxyHost() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1129)> _
    Property SSLCiphers() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1130)> _
    Property SSLNoCACerts() As Integer
    <DispId(1131)> _
    Property SSLCommonName() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1132)> _
    Property AUTHUsername() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1134)> _
    Property XmlAddressResolutionType() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1135)> _
    Property AutoScale() As Boolean
    <DispId(1136)> _
    Property AutoAppResize() As Boolean
    <DispId(1139)> _
    Property Hotkey1Char() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1140)> _
    Property Hotkey1Shift() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1141)> _
    Property Hotkey2Char() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1142)> _
    Property Hotkey2Shift() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1143)> _
    Property Hotkey3Char() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1144)> _
    Property Hotkey3Shift() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1145)> _
    Property Hotkey4Char() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1146)> _
    Property Hotkey4Shift() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1147)> _
    Property Hotkey5Char() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1148)> _
    Property Hotkey5Shift() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1149)> _
    Property Hotkey6Char() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1150)> _
    Property Hotkey6Shift() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1196)> _
    Property Hotkey7Char() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1152)> _
    Property Hotkey7Shift() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1153)> _
    Property Hotkey8Char() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1154)> _
    Property Hotkey8Shift() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1155)> _
    Property Hotkey9Char() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1156)> _
    Property Hotkey9Shift() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1157)> _
    Property Hotkey10Char() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1158)> _
    Property Hotkey10Shift() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1159)> _
    Property ControlWindowText() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1160)> _
    Property CacheICAFile() As Boolean
    <DispId(1161)> _
    Property ScreenPercent() As Integer
    <DispId(1163)> _
    Property TWIMode() As Boolean
    <DispId(1164)> _
    Property TransportReconnectEnabled() As Boolean
    <DispId(1165)> _
    Property TransportReconnectDelay() As Integer
    <DispId(1166)> _
    Property TransportReconnectRetries() As Integer
    <DispId(1167)> _
    Property AutoLogonAllowed() As Boolean
    <DispId(1168)> _
    Property EnableSessionSharingClient() As Boolean
    <DispId(1169)> _
    Property SessionSharingName() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1170)> _
    Property SessionSharingLaunchOnly() As Boolean
    <DispId(1171)> _
    Property DisableCtrlAltDel() As Boolean
    <DispId(1172)> _
    Property SessionCacheEnable() As Boolean
    <DispId(1173)> _
    Property SessionCacheTimeout() As Integer
    <DispId(1175)> _
    ReadOnly Property Session() As <MarshalAs(UnmanagedType.Interface)> ISession
    <DispId(1176)> _
    Property OutputMode() As OutputMode
    <DispId(1178)> _
    Property SessionExitTimeout() As Integer
    <DispId(1179)> _
    Property EnableSessionSharingHost() As Boolean
    <DispId(1180)> _
    Property LongCommandLine() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1181)> _
    Property TWIDisableSessionSharing() As Boolean
    <DispId(1182)> _
    Property SessionSharingKey() As <MarshalAs(UnmanagedType.BStr)> String
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(104)> _
    Function DisconnectSessions(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal pGroupId As String) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(105)> _
    Function LogoffSessions(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal pGroupId As String) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(106)> _
    Function SetSessionGroupId(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal pGroupId As String) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(107)> _
    Function GetSessionHandle() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(108)> _
    Function SwitchSession(<[In]()> ByVal hSession As Integer) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(109)> _
    Function GetSessionCount() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(110)> _
    Function GetSessionHandleByIndex(<[In]()> ByVal Index As Integer) As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(111)> _
    Function GetSessionGroupCount(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal pGroupId As String) As Integer
    <DispId(1191)> _
    Property IPCLaunch() As Boolean
    <DispId(1192)> _
    Property AudioDuringDetach() As Boolean
    <DispId(1193)> _
    Property Hotkey11Char() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(1194)> _
    Property Hotkey11Shift() As <MarshalAs(UnmanagedType.BStr)> String
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(112)> _
    Function IsPassThrough() As Boolean
    <DispId(1195)> _
    Property VirtualCOMPortEmulation() As Boolean
End Interface

<Guid("33AEC7E1-6EE6-41BF-9368-65126FAC8418")> _
Public Enum ICASessionString
    ' Fields
    SessionDomain = 2
    SessionServer = 0
    SessionUsername = 1
End Enum



<Guid("33AEC7E4-6EE6-41BF-9368-65126FAC8418")> _
Public Enum ICASessionEndAction
    ' Fields
    SessionEndDefault = 0
    SessionEndRestart = 1
End Enum

<Guid("33AEC7E2-6EE6-41BF-9368-65126FAC8418")> _
Public Enum ICASessionCounter
    ' Fields
    SessionAverageLatency = 7
    SessionIncomingBytes = 0
    SessionIncomingErrors = 4
    SessionIncomingFrames = 2
    SessionLastLatency = 6
    SessionLatencyDeviation = 8
    SessionOutgoingBytes = 1
    SessionOutgoingErrors = 5
    SessionOutgoingFrames = 3
End Enum

<Guid("33AEC7E3-6EE6-41BF-9368-65126FAC8418")> _
Public Enum ICAEvent
    ' Fields
    EventChannelDataReceived = 15
    EventConnect = 1
    EventConnectFail = 2
    EventConnecting = 11
    EventDisconnect = 5
    EventDisconnectFailed = 13
    EventIcaFilePresent = 8
    EventInitializing = 10
    EventInitialProp = 12
    EventLoadIcaFileFailed = 9
    EventLogin = 3
    EventLoginFail = 4
    EventLogoffFailed = 14
    EventNone = 0
    EventRunPubishedApp = 6
    EventRunPubishedAppFail = 7
    EventWindowCreated = 18
    EventWindowDestroyed = 19
    EventWindowDisplayed = 27
    EventWindowDocked = 20
    EventWindowFullscreened = 25
    EventWindowHidden = 26
    EventWindowMaximized = 23
    EventWindowMinimized = 22
    EventWindowMoved = 17
    EventWindowRestored = 24
    EventWindowSized = 16
    EventWindowUndocked = 21
End Enum

<Guid("33AEC7E9-6EE6-41BF-9368-65126FAC8418")> _
Public Enum ICAVCDataType
    ' Fields
    DataTypeBinary = 2
    DataTypeBinaryString = 1
    DataTypeString = 0
End Enum


<Guid("33AEC7E8-6EE6-41BF-9368-65126FAC8418")> _
Public Enum ICAWindowType
    ' Fields
    WindowTypeClient = 2
    WindowTypeContainer = 3
    WindowTypeControl = 1
    WindowTypeICAClientObject = 0
End Enum


<Guid("33AEC7E6-6EE6-41BF-9368-65126FAC8418")> _
Public Enum ICASoundQuality
    ' Fields
    SoundQualityHigh = 0
    SoundQualityLow = 2
    SoundQualityMedium = 1
    SoundQualityNone = -1
End Enum


<Guid("33AEC7E5-6EE6-41BF-9368-65126FAC8418")> _
Public Enum ICAColorDepth
    ' Fields
    Color16 = 1
    Color16Bit = 4
    Color24Bit = 8
    Color256 = 2
End Enum


<Guid("33AEC7E7-6EE6-41BF-9368-65126FAC8418")> _
Public Enum ICAScalingMode
    ' Fields
    ScalingModeAutoSize = 3
    ScalingModeDisabled = 0
    ScalingModePercent = 1
    ScalingModeSize = 2
End Enum



<Guid("33AEC7EA-6EE6-41BF-9368-65126FAC8418")> _
Public Enum OutputMode
    ' Fields
    OutputModeNonHeadless = 0
    OutputModeNormal = 1
    OutputModeRenderless = 2
    OutputModeWindowless = 3
End Enum





