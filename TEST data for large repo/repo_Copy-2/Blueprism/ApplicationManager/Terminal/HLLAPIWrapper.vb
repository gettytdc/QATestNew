Imports System.Runtime.InteropServices

Public MustInherit Class HLLAPIWrapper : Implements IDisposable
    ''' <summary>
    ''' Native Windows function needed for dynamically loading an unmanaged DLL.
    ''' </summary>
    <DllImport("kernel32.dll")> _
    Private Shared Function LoadLibrary(dllToLoad As String) As IntPtr
    End Function

    ''' <summary>
    ''' Native Windows function needed for loading the entry point in a dll
    ''' </summary>
    <DllImport("kernel32.dll")> _
    Protected Shared Function GetProcAddress(hModule As IntPtr, procName As String) As IntPtr
    End Function

    ''' <summary>
    ''' Native Windows function needed for unloading the dll
    ''' </summary>
    <DllImport("kernel32.dll")> _
    Private Shared Sub FreeLibrary(hModule As IntPtr)
    End Sub

    Public MustOverride Sub HLLAPI(ByRef func As Integer, data As Byte(), ByRef len As Integer, ByRef rc As Integer)

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="dllName">The name of the HLLAPI dll to use.</param>
    ''' <param name="entryPoint">The name of the entryPoiny in the dll.</param>
    Public Sub New(dllName As String, entryPoint As String)
        LoadDll(dllName, entryPoint)
    End Sub

    Dim mDllHandle As IntPtr

    ''' <summary>
    ''' LoadDll - Dynamically load the WHLLAPI DLL.
    ''' </summary>
    ''' <param name="dllname">The name of the dll to load</param>
    ''' <param name="entryPoint">entrypoint </param>
    Public Sub LoadDll(dllname As String, entryPoint As String)
        ' Map the WHLLAPI DLL into our address space.
        mDllHandle = LoadLibrary(dllname)

        If mDllHandle = IntPtr.Zero Then _
        Throw New DllNotFoundException(String.Format(My.Resources.TerminalEmulationCouldNotLoadTheDll0, dllname))

        GetEntryPoint(mDllHandle, entryPoint)
    End Sub

    Protected MustOverride Sub GetEntryPoint(handle As IntPtr, entryPoint As String)

    Protected Overridable Sub Dispose(disposing As Boolean)
        If disposing Then
            If mDllHandle <> IntPtr.Zero Then FreeLibrary(mDllHandle)
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
    End Sub

End Class
