Imports System.Runtime.InteropServices
Imports System.Runtime.CompilerServices

<ComImport(), Guid(modClassID.IScreenShot), TypeLibType(CShort(4160))> _
Public Interface IScreenShot
    <DispId(1)> _
    Property PositionX() As Integer
    <DispId(2)> _
    Property PositionY() As Integer
    <DispId(3)> _
    Property Width() As Integer
    <DispId(4)> _
    Property Height() As Integer
    <DispId(5)> _
    Property Filename() As <MarshalAs(UnmanagedType.BStr)> String
    <DispId(6)> _
    ReadOnly Property BitmapHash() As <MarshalAs(UnmanagedType.BStr)> String
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(100)> _
    Sub Save()
End Interface





