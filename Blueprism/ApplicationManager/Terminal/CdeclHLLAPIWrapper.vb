Imports System.Runtime.InteropServices

Public Class CdeclHLLAPIWrapper : Inherits HLLAPIWrapper

    Public Sub New(dllName As String, entryPoint As String)
        MyBase.New(dllName, entryPoint)
    End Sub

    <UnmanagedFunctionPointer(CallingConvention.Cdecl)> _
    Public Delegate Sub HLLAPIDelegate(ByRef func As Integer, data As Byte(), ByRef len As Integer, ByRef rc As Integer)
    Private mHLLAPIDelegate As HLLAPIDelegate

    Protected Overrides Sub GetEntryPoint(handle As IntPtr, entryPoint As String)
        ' Get the addresses of the HLLAPI functions in the HLLAPI DLL.
        Dim ptrFunction As IntPtr = GetProcAddress(handle, entryPoint)

        If ptrFunction = IntPtr.Zero Then _
        Throw New EntryPointNotFoundException(String.Format(My.Resources.TerminalEmulationCouldNotFindTheEntryPoint0, entryPoint))

        ' Assign the function pointers to the Delegates defined above.
        mHLLAPIDelegate = DirectCast(Marshal.GetDelegateForFunctionPointer(ptrFunction, GetType(HLLAPIDelegate)), HLLAPIDelegate)
    End Sub

    Public Overrides Sub HLLAPI(ByRef func As Integer, data As Byte(), ByRef len As Integer, ByRef rc As Integer)
        mHLLAPIDelegate(func, data, len, rc)
    End Sub
End Class

