Imports System.Runtime.InteropServices
Imports System.Runtime.CompilerServices

<ComImport(), Guid(modClassID.ISession), TypeLibType(TypeLibTypeFlags.FDual Or TypeLibTypeFlags.FDispatchable)> _
Public Interface ISession
    <DispId(1)> _
    ReadOnly Property TopLevelWindows() As <MarshalAs(UnmanagedType.Interface)> IEnumerable
    <DispId(2)> _
    ReadOnly Property Mouse() As <MarshalAs(UnmanagedType.Interface)> IMouse
    <DispId(3)> _
    ReadOnly Property Keyboard() As <MarshalAs(UnmanagedType.Interface)> IKeyboard
    <DispId(4)> _
    ReadOnly Property ForegroundWindow() As <MarshalAs(UnmanagedType.Interface)> IWindow
    <DispId(5)> _
    Property ReplayMode() As Boolean
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(100)> _
    Function CreateFullScreenShot() As <MarshalAs(UnmanagedType.Interface)> IScreenShot
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(101)> _
    Function CreateScreenShot(<[In]()> ByVal x As Integer, <[In]()> ByVal y As Integer, <[In]()> ByVal Width As Integer, <[In]()> ByVal Height As Integer) As <MarshalAs(UnmanagedType.Interface)> IScreenShot
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(102)> _
    Sub SendPingRequest(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal pingInfo As String)
End Interface





