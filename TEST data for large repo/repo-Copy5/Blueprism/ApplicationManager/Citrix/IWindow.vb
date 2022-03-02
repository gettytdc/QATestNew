Imports System.Runtime.InteropServices
Imports System.Runtime.CompilerServices

<ComImport(), TypeLibType(CShort(4160)), Guid(modClassID.IWindow)> _
Public Interface IWindow
    <DispId(1)> _
    ReadOnly Property PositionX() As Integer
    <DispId(2)> _
    ReadOnly Property PositionY() As Integer
    <DispId(3)> _
    ReadOnly Property Width() As Integer
    <DispId(4)> _
    ReadOnly Property Height() As Integer
    <DispId(5)> _
    ReadOnly Property Style() As Integer
    <DispId(6)> _
    ReadOnly Property ExtendedStyle() As Integer
    <DispId(7)> _
    ReadOnly Property Caption() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(8)> _
    ReadOnly Property SmallIconHash() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(9)> _
    ReadOnly Property LargeIconHash() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(10)> _
    ReadOnly Property Disposed() As Boolean
    <DispId(11)> _
    ReadOnly Property WindowFlags() As Integer
    <DispId(12)> _
    ReadOnly Property WindowID() As Integer
    <DispId(13)> _
    ReadOnly Property ParentID() As Integer
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(100)> _
    Sub BringToTop()
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(101)> _
    Sub Move(<[In]()> ByVal XPos As Integer, <[In]()> ByVal YPos As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(102)> _
    Sub Resize(<[In]()> ByVal Width As Integer, <[In]()> ByVal Height As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(103)> _
    Sub Restore()
End Interface





