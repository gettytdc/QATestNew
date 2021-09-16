Imports System.Runtime.InteropServices
Imports System.Runtime.CompilerServices

<ComImport(), TypeLibType(CShort(4160)), Guid(modClassID.IKeyboard)> _
Public Interface IKeyboard
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(100)> _
    Sub SendKeyDown(<[In]()> ByVal keyId As Integer)
    <MethodImpl(MethodImplOptions.InternalCall, MethodCodeType:=MethodCodeType.Runtime), DispId(101)> _
    Sub SendKeyUp(<[In]()> ByVal keyId As Integer)
End Interface


