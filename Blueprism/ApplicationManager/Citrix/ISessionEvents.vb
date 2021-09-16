Imports System.Runtime.InteropServices
Imports System.Runtime.CompilerServices

<ComImport(), InterfaceType(CShort(2)), Guid(modClassID.ISessionEvent), TypeLibType(CShort(4096))> _
Public Interface ISessionEvents
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(1)> _
    Sub OnWindowCreate(<[In](), MarshalAs(UnmanagedType.Interface)> ByVal window As IWindow)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(2)> _
    Sub OnWindowDestroy(<[In](), MarshalAs(UnmanagedType.Interface)> ByVal window As IWindow)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(4)> _
    Sub OnPingAck(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal pingInfo As String, <[In]()> ByVal roundTripTime As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(5)> _
    Sub OnWindowForeground(<[In]()> ByVal WindowID As Integer)
End Interface

