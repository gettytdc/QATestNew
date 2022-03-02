Imports System.Runtime.InteropServices
Imports System.Runtime.CompilerServices

<ComImport(), InterfaceType(CShort(2)), Guid(modClassID.IMouseEvents), TypeLibType(CShort(4096))> _
Public Interface IMouseEvents
    <PreserveSig(), MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(1)> _
    Sub OnMove(<[In]()> ByVal buttonState As Integer, <[In]()> ByVal modifierState As Integer, <[In]()> ByVal XPos As Integer, <[In]()> ByVal YPos As Integer)
    <PreserveSig(), MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(3)> _
    Sub OnMouseDown(<[In]()> ByVal buttonState As Integer, <[In]()> ByVal modifierState As Integer, <[In]()> ByVal XPos As Integer, <[In]()> ByVal YPos As Integer)
    <PreserveSig(), MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(2)> _
    Sub OnMouseUp(<[In]()> ByVal buttonState As Integer, <[In]()> ByVal modifierState As Integer, <[In]()> ByVal XPos As Integer, <[In]()> ByVal YPos As Integer)
    <PreserveSig(), MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(4)> _
    Sub OnDoubleClick()
End Interface



