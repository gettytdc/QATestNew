Imports System.Runtime.InteropServices
Imports System.Runtime.CompilerServices

<ComImport(), Guid(modClassID.IMouse), TypeLibType(CShort(4160))> _
Public Interface IMouse
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(100)> _
    Sub SendMouseDown(<[In]()> ByVal buttonId As Integer, <[In]()> ByVal modifiers As Integer, <[In]()> ByVal XPos As Integer, <[In]()> ByVal YPos As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(101)> _
    Sub SendMouseUp(<[In]()> ByVal buttonId As Integer, <[In]()> ByVal modifiers As Integer, <[In]()> ByVal XPos As Integer, <[In]()> ByVal YPos As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(102)> _
    Sub SendMouseMove(<[In]()> ByVal buttonId As Integer, <[In]()> ByVal modifiers As Integer, <[In]()> ByVal XPos As Integer, <[In]()> ByVal YPos As Integer)
End Interface



