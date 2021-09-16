Imports System.Runtime.InteropServices
Imports System.Runtime.CompilerServices


<ComImport(), TypeLibType(CShort(4096)), InterfaceType(CShort(2)), Guid(modClassID.IClientEvents)> _
Public Interface IClientEvents
    <PreserveSig(), MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(-609)> _
    Sub OnReadyStateChange(<[In]()> ByVal lReadyState As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(2)> _
    Sub OnClick(<[In]()> ByVal MouseButton As Integer, <[In]()> ByVal PosX As Integer, <[In]()> ByVal PosY As Integer, <[In]()> ByVal KeyMask As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(3)> _
    Sub OnConnect()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(4)> _
    Sub OnConnectFailed()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(5)> _
    Sub OnLogon()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(6)> _
    Sub OnLogonFailed()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(7)> _
    Sub OnDisconnect()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(8)> _
    Sub OnPublishedApp()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(9)> _
    Sub OnPublishedAppFailed()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(10)> _
    Sub OnICAFile()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(11)> _
    Sub OnICAFileFailed()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(12)> _
    Sub OnInitializing()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(13)> _
    Sub OnConnecting()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(14)> _
    Sub OnInitialProp()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(15)> _
    Sub OnDisconnectFailed()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(16)> _
    Sub OnLogoffFailed()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(17)> _
    Sub OnChannelDataReceived(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal ChannelName As String)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(18)> _
    Sub OnWindowSized(<[In]()> ByVal WndType As Integer, <[In]()> ByVal Width As Integer, <[In]()> ByVal Height As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(19)> _
    Sub OnWindowMoved(<[In]()> ByVal WndType As Integer, <[In]()> ByVal XPos As Integer, <[In]()> ByVal YPos As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(20)> _
    Sub OnWindowCreated(<[In]()> ByVal WndType As Integer, <[In]()> ByVal XPos As Integer, <[In]()> ByVal YPos As Integer, <[In]()> ByVal Width As Integer, <[In]()> ByVal Height As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(21)> _
    Sub OnWindowDestroyed(<[In]()> ByVal WndType As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(22)> _
    Sub OnWindowDocked()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(23)> _
    Sub OnWindowUndocked()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(24)> _
    Sub OnWindowMinimized()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(25)> _
    Sub OnWindowMaximized()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(26)> _
    Sub OnWindowRestored()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(27)> _
    Sub OnWindowFullscreened()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(28)> _
    Sub OnWindowHidden(<[In]()> ByVal WndType As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(29)> _
    Sub OnWindowDisplayed(<[In]()> ByVal WndType As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(30)> _
    Sub OnWindowCloseRequest()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(31)> _
    Sub OnDisconnectSessions(<[In]()> ByVal hCommand As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(32)> _
    Sub OnDisconnectSessionsFailed(<[In]()> ByVal hCommand As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(33)> _
    Sub OnLogoffSessions(<[In]()> ByVal hCommand As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(34)> _
    Sub OnLogoffSessionsFailed(<[In]()> ByVal hCommand As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(35)> _
    Sub OnSessionSwitch(<[In]()> ByVal hOldSession As Integer, <[In]()> ByVal hNewSession As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(36)> _
    Sub OnSessionEventPending(<[In]()> ByVal hSession As Integer, <[In]()> ByVal EventNum As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(37)> _
    Sub OnSessionAttach(<[In]()> ByVal hSession As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(38)> _
    Sub OnSessionDetach(<[In]()> ByVal hSession As Integer)
End Interface





