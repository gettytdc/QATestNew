Imports System.Runtime.InteropServices
Imports Accessibility
Imports System.ComponentModel
Imports System.Drawing
Imports System.Runtime.InteropServices.ComTypes

Imports System.Security
Imports System.Security.Permissions

Imports Microsoft.Win32.SafeHandles

<CLSCompliant(False)>
Public Module modWin32

    ''' <summary>
    ''' Class which hosts P/Invoke methods which utilise SafeHandles rather than raw
    ''' IntPtr handles.
    ''' </summary>
    Public Class SafeMethods
        <DllImport("kernel32.dll")>
        Public Shared Function OpenProcess(ByVal dwDesiredAccess As ProcessAccess, <MarshalAs(UnmanagedType.Bool)> ByVal bInheritHandle As Boolean, ByVal dwProcessId As Integer) As SafeProcessHandle
        End Function
    End Class

    <DllImport("Kernel32.dll", SetLastError:=True, CallingConvention:=CallingConvention.Winapi)>
    Public Function IsWow64Process(ByVal hProcess As IntPtr, ByRef wow64Process As Boolean) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    'Declare the wrapper managed MouseHookStruct class.
    <StructLayout(LayoutKind.Sequential)>
    Public Structure MouseHookStruct
        Public pt As POINTAPI
        Public hWnd As Integer
        Public wHitTestCode As Integer
        Public dwExtraInfo As Integer
    End Structure

    <DllImport("kernel32.dll")>
    Public Function FlushInstructionCache(ByVal hProcess As IntPtr, ByVal lpBaseAddress As IntPtr, ByVal dwSize As IntPtr) As Boolean
    End Function

    ' The string ID of the IDispatch interface
    Private Const IDispatchId As String = "00020400-0000-0000-C000-000000000046"

    ' The string ID of the IAccessible interface
    Private Const IAccessibleId As String = "618736E0-3C3D-11CF-810C-00AA00389B71"

    ''' <summary>
    ''' The interface ID which represents the IAccessible interface
    ''' </summary>
    Public ReadOnly IID_IACCESSIBLE As New Guid(IAccessibleId)

    ''' <summary>
    ''' The interface ID which represents the IDispatch interface
    ''' </summary>
    Public ReadOnly IID_IDISPATCH As New Guid(IDispatchId)

    ''' <summary>
    ''' A .net representation of the IDispatch interface.
    ''' We can't just call it IDispatch because any project referencing ManagedSpyLib
    ''' would not be able to use it - that project includes oleidl.h via Mem.h and
    ''' various others in the chain - and oleidl.h defines IDispatch in an
    ''' unhelpfully non-public but still blocking the type name manner. Which is nice
    ''' </summary>
    <ComImport(),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid(IDispatchId)>
    Public Interface IManagedDispatch
        Function GetTypeInfoCount() As Integer

        Function GetTypeInfo(
         <[In](), MarshalAs(UnmanagedType.U4)> ByVal iTInfo As Integer,
         <[In](), MarshalAs(UnmanagedType.U4)> ByVal lcid As Integer) _
         As <MarshalAs(UnmanagedType.Interface)> ITypeInfo

        Sub GetIDsOfNames(
         <[In]()> ByRef riid As Guid,
         <[In](), MarshalAs(UnmanagedType.LPArray)> ByVal rgszNames() As String,
         <[In](), MarshalAs(UnmanagedType.U4)> ByVal cNames As Integer,
         <[In](), MarshalAs(UnmanagedType.U4)> ByVal lcid As Integer,
         <[Out](), MarshalAs(UnmanagedType.LPArray)> ByVal rgDispId() As Integer)

    End Interface

    Public Declare Function AccessibleObjectFromWindow Lib "oleacc" (
     ByVal Hwnd As Int32,
     ByVal dwId As Int32,
     ByRef riid As Guid,
     <MarshalAs(UnmanagedType.Interface)> ByRef ppvObject As IAccessible) As Int32

    Public Declare Function AccessibleObjectFromWindow Lib "oleacc" (
     ByVal Hwnd As IntPtr,
     ByVal dwId As Int32,
     ByRef riid As Guid,
     <MarshalAs(UnmanagedType.Interface)> ByRef ppvObject As IAccessible) As Int32

    Public Declare Function AccessibleObjectFromWindow Lib "oleacc" (
    ByVal Hwnd As Int32,
    ByVal dwId As Int32,
    ByRef riid As Guid,
    <MarshalAs(UnmanagedType.Interface)> ByRef ppvObject As IManagedDispatch) As Int32

    <DllImport("oleacc.dll")>
    Public Function AccessibleChildren(ByVal paccContainer As IAccessible, ByVal iChildStart As Integer, ByVal cChildren As Integer, <[Out]()> ByVal rgvarChildren() As Object, ByRef pcObtained As Integer) As Int32
    End Function


    'This is the Import for the SetWindowsHookEx function.
    'Use this function to install a thread-specific hook.
    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall)>
    Public Function SetWindowsHookEx(ByVal idHook As Integer, ByVal lpfn As [Delegate], ByVal hInstance As IntPtr, ByVal threadId As Integer) As IntPtr
    End Function

    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall)>
    Public Function SetWindowsHookEx(ByVal hookType As HookTypes, ByVal hookProc As [Delegate], ByVal hInstance As IntPtr, ByVal nThreadId As Integer) As IntPtr
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetClientRect(ByVal hwnd As IntPtr, ByRef rc As RECT) As Boolean
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetClassName(ByVal hWnd As IntPtr, ByVal lpClassName As StringBuilder, ByVal nMaxCount As Integer) As Integer
    End Function

    'This is the Import for the UnhookWindowsHookEx function.
    'Call this function to uninstall the hook.
    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall)>
    Public Function UnhookWindowsHookEx(ByVal idHook As Integer) As Boolean
    End Function

    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall)>
    Public Function UnhookWindowsHookEx(ByVal hookHandle As IntPtr) As Integer
    End Function

    'This is the Import for the CallNextHookEx function.
    'Use this function to pass the hook information to the next hook procedure in chain.
    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall)>
    Public Function CallNextHookEx(ByVal idHook As Integer, ByVal nCode As Integer, ByVal wParam As Integer, ByVal lParam As IntPtr) As Integer
    End Function

    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall)>
    Public Function CallNextHookEx(ByVal hookHandle As IntPtr, ByVal nCode As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As Integer
    End Function

    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Auto, ExactSpelling:=True)>
    Public Function SetCapture(ByVal hwnd As IntPtr) As IntPtr
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function WindowFromPoint(ByVal p As POINTAPI) As IntPtr
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function WindowFromPoint(ByVal pt As System.Drawing.Point) As IntPtr
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function ScreenToClient(ByVal hWnd As IntPtr, ByRef lpPoint As POINTAPI) As Boolean
    End Function

    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Function GetWindowTextLength(ByVal hwnd As IntPtr) As Integer
    End Function

    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Function GetWindowText(ByVal hWnd As IntPtr, ByVal lpString As System.Text.StringBuilder, ByVal nMaxCount As Integer) As Integer
    End Function

    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Function GetWindowText(ByVal hwnd As Int32,
      ByVal lpString As StringBuilder,
      ByVal cch As Integer) As Integer
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetTopWindow(ByVal hwnd As IntPtr) As IntPtr
    End Function

    Public Declare Auto Function GetWindowInfo Lib "user32" (ByVal hwnd As IntPtr, ByRef pwi As WINDOWINFO) As Boolean

    <DllImport("user32", SetLastError:=True)>
    Public Function ClientToScreen(ByVal hwnd As IntPtr, ByRef lpPoint As POINTAPI) As Boolean
    End Function

    <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
    Public Function CloseHandle(ByVal hObject As IntPtr) As Boolean
    End Function

    <DllImport("kernel32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Public Function CreateProcess(ByVal lpApplicationName As String,
  ByVal lpCommandLine As String, ByRef lpProcessAttributes As SECURITY_ATTRIBUTES,
   ByRef lpThreadAttributes As SECURITY_ATTRIBUTES, ByVal bInheritHandles As Boolean,
   ByVal dwCreationFlags As Integer, ByVal lpEnvironment As IntPtr, ByVal lpCurrentDirectory As String,
   <[In]()> ByRef lpStartupInfo As STARTUPINFO,
   <[Out]()> ByRef lpProcessInformation As PROCESS_INFORMATION) As Boolean

    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Public Function TerminateProcess(ByVal hProcess As IntPtr, ByVal uExitCode As System.UInt32) As Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Public Function GetExitCodeProcess(ByVal hProcess As IntPtr, ByRef lpExitCode As System.UInt32) As Boolean
    End Function



    ' Engineered from iepmapi.h in the Windows SDK  
    <StructLayout(LayoutKind.Sequential)>
    Public Structure IELAUNCHURLINFO
        Public cbSize As Integer
        Public dwCreationFlags As Integer
    End Structure




    '/ <summary>  
    '/ Launch a URL with Internet Explorer. Works with IE's protected  
    '/ mode.  
    '/ </summary>  
    '/ <param name="url">The URI to navigate to.</param>  
    '/ <param name="pProcInfo">Process information struct by reference  
    '/ that will contain the opened process ID.</param>  
    '/ <param name="lpInfo">The launch information struct.</param>  
    '/ <returns>Returns a value indicating whether the native call was  
    '/ successful.</returns>  
    <DllImport("ieframe.dll")>
    Public Function IELaunchURL(<MarshalAs(UnmanagedType.LPWStr)> ByVal url As String, ByRef pProcInfo As PROCESS_INFORMATION, ByRef lpInfo As IELAUNCHURLINFO) As Integer
    End Function


    <DllImport("user32", SetLastError:=True)>
    Public Function GetWindowRect(ByVal hwnd As Integer, ByRef lpRect As RECT) As Integer
    End Function

    ''' <summary>
    ''' Copies a visual window into the specified device context (DC), typically a
    ''' printer DC
    ''' </summary>
    ''' <param name="hwnd">A handle to the window that will be copied.</param>
    ''' <param name="hDC">A handle to the device context.</param>
    ''' <param name="nFlags">The drawing options. The default is 0 in which the
    ''' entire window is copied. Otherwise, it can be: <see
    ''' cref="PrintWindowFlags.PW_CLIENTONLY"/> which causes only the client area of
    ''' the window to be copied to hdcBlt.</param>
    ''' <returns>True on success; False on failure</returns>
    <DllImport("User32.dll", SetLastError:=True)>
    Public Function PrintWindow(
     ByVal hwnd As IntPtr, ByVal hDC As IntPtr, ByVal nFlags As UInteger) As Boolean
    End Function

    ''' <summary>
    ''' Overload for PrintWindow that prints the whole window rather than just the
    ''' client area.
    ''' </summary>
    ''' <param name="hwnd">A handle to the window that will be copied.</param>
    ''' <param name="hDC">A handle to the device context.</param>
    ''' <returns>True on success; False on failure</returns>
    Public Function PrintWindow(ByVal hwnd As IntPtr, ByVal hDC As IntPtr) As Boolean
        Return PrintWindow(hwnd, hDC, 0)
    End Function

    ''' <summary>
    ''' The flags used for PrintWindow calls
    ''' </summary>
    Public Enum PrintWindowFlags
        PW_CLIENTONLY = &H1
    End Enum

    <DllImport("user32", CharSet:=CharSet.Auto, SetLastError:=True, ExactSpelling:=True)>
    Public Function GetAncestor(ByVal hwnd As IntPtr, ByVal gaFlags As Integer) As IntPtr
    End Function

    Public Const GA_PARENT As Integer = 1
    Public Const GA_ROOT As Integer = 2
    Public Const GA_ROOTOWNER As Integer = 3

    <DllImport("user32", CharSet:=CharSet.Auto, SetLastError:=True, ExactSpelling:=True)>
    Public Function SetWindowPos(ByVal hwnd As IntPtr, ByVal hwndInsertAfter As IntPtr, ByVal x As Integer, ByVal y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As Integer) As Boolean

    End Function


    Public Enum SWP
        HWND_TOP = 0
        SWP_FRAMECHANGED = &H20
        SWP_HIDeWINDOW = &H80
        SWP_NOACTIVATE = &H10
        SWP_NOCOPYBITS = &H100
        SWP_NOMOVE = &H2
        SWP_NOOWNERZORDER = &H200
        SWP_NOREDRAW = &H8
        SWP_NOSIZE = &H1
        SWP_NOZORDER = &H4
        SWP_SHOWWINDOW = &H40
    End Enum


    <DllImport("User32", CharSet:=CharSet.Auto, SetLastError:=True)>
    Public Function VkKeyScan(ByVal ch As Char) As Int16
    End Function

    <DllImport("User32", CharSet:=CharSet.Auto, SetLastError:=True)>
    Public Function MapVirtualKey(ByVal uCode As Integer, ByVal uMapType As Integer) As Integer
    End Function

    <DllImport("user32", SetLastError:=True)>
    Public Sub mouse_event(ByVal dwFlags As Integer, ByVal dx As Integer, ByVal dy As Integer, ByVal cbuttons As Integer, ByVal dwExtraInfo As Integer)
    End Sub

    <DllImport("user32", SetLastError:=True)>
    Public Sub keybd_event(ByVal bVirtualKey As Integer, ByVal bScanCode As Integer, ByVal dwFlags As Integer, ByVal dwExtraInfo As Integer)
    End Sub

    Public Const KEYEVENTF_EXTENDEDKEY As Integer = 1
    Public Const KEYEVENTF_KEYUP As Integer = 2

    <DllImport("kernel32.dll", SetLastError:=True)>
    Public Function OpenThread(ByVal dwDesiredAccess As Integer, ByVal bInheritHandle As Boolean, ByVal dwThreadId As Integer) As IntPtr
    End Function

    Public Const THREAD_GET_CONTEXT As Int32 = 8
    Public Const THREAD_QUERY_INFORMATION As Int32 = &H40
    Public Const THREAD_SET_CONTEXT As Int32 = &H10
    Public Const THREAD_SUSPEND_RESUME As Int32 = 2

    <DllImport("kernel32.dll", SetLastError:=True)>
    Public Function GetThreadContext(ByVal hThread As IntPtr, ByRef lpContext As CONTEXT) As Boolean
    End Function
    <DllImport("kernel32.dll", SetLastError:=True, EntryPoint:="GetThreadContext")>
    Public Function GetThreadContext64(ByVal hThread As IntPtr, ByRef lpContext As CONTEXT_x64) As Boolean
    End Function
    <DllImport("kernel32.dll", SetLastError:=True, EntryPoint:="GetThreadContext")>
    Public Function GetThreadContext64p(ByVal hThread As IntPtr, ByVal lpContext As IntPtr) As Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Public Function SetThreadContext(ByVal hThread As IntPtr, ByRef lpContext As CONTEXT) As Boolean
    End Function
    <DllImport("kernel32.dll", SetLastError:=True, EntryPoint:="SetThreadContext")>
    Public Function SetThreadContext64(ByVal hThread As IntPtr, ByRef lpContext As CONTEXT_x64) As Boolean
    End Function
    <DllImport("kernel32.dll", SetLastError:=True, EntryPoint:="SetThreadContext")>
    Public Function SetThreadContext64p(ByVal hThread As IntPtr, ByVal lpContext As IntPtr) As Boolean
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Public Structure CONTEXT

        Public ContextFlags As Int32

        Public Dr0 As Int32
        Public Dr1 As Int32
        Public Dr2 As Int32
        Public Dr3 As Int32
        Public Dr6 As Int32
        Public Dr7 As Int32

        'FloatSave...
        Public ControlWord, StatusWord, TagWord, ErrorOffset As Int32
        Public ErrorSelector, DataOffset, DataSelector As Int32
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=80)> Public RegisterArea() As Byte
        Public Cr0NpxState As Int32

        Public SegGs As Int32
        Public SegFs As Int32
        Public SegEs As Int32
        Public SegDs As Int32

        Public Edi As Int32
        Public Esi As Int32
        Public Ebx As Int32
        Public Edx As Int32
        Public Ecx As Int32
        Public Eax As Int32

        Public Ebp As Int32
        Public Eip As IntPtr
        Public SegCs As Int32
        Public EFlags As Int32
        Public Esp As Int32
        Public SegSs As Int32

        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=512)> Public ExtendedRegisters() As Byte

    End Structure

    Public Const CONTEXT_CONTROL As Integer = &H1
    Public Const CONTEXT_INTEGER As Integer = &H2
    Public Const CONTEXT_X86 = &H10000
    Public Const CONTEXT_AMD64 As Integer = &H100000


    <StructLayout(LayoutKind.Sequential, Pack:=16)>
    Public Structure CONTEXT_x64
        Public P1Home As Int64
        Public P2Home As Int64
        Public P3Home As Int64
        Public P4Home As Int64
        Public P5Home As Int64
        Public P6Home As Int64
        Public ContextFlags As Int32
        Public MxCsr As Int32
        Public SegCs As Int16
        Public SegDs As Int16
        Public SegEs As Int16
        Public SegFs As Int16
        Public SegGs As Int16
        Public SegSs As Int16
        Public EFlags As Int32
        Public Dr0 As Int64
        Public Dr1 As Int64
        Public Dr2 As Int64
        Public Dr3 As Int64
        Public Dr6 As Int64
        Public Dr7 As Int64
        Public Rax As Int64
        Public Rcx As Int64
        Public Rdx As Int64
        Public Rbx As Int64
        Public Rsp As Int64
        Public Rbp As Int64
        Public Rsi As Int64
        Public Rdi As Int64
        Public R8 As Int64
        Public R9 As Int64
        Public R10 As Int64
        Public R11 As Int64
        Public R12 As Int64
        Public R13 As Int64
        Public R14 As Int64
        Public R15 As Int64
        Public Rip As IntPtr
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=512)> Public FPStuff() As Byte
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=16 * 26)> Public VectorRegister() As Byte
        Public VectorControl As Int64
        Public DebugControl As Int64
        Public LastBranchToRip As Int64
        Public LastBranchFromRip As Int64
        Public LastExceptionToRip As Int64
        Public LastExceptionFromRip As Int64
    End Structure


    <DllImport("user32", CharSet:=CharSet.Auto, SetLastError:=True)>
    Public Function PostMessage(ByVal hwnd As Integer, ByVal msg As Integer, ByVal wparam As Integer, ByVal lparam As Integer) As Integer
    End Function

    <DllImport("user32", CharSet:=CharSet.Auto, SetLastError:=True)>
    Public Function PostMessage(ByVal hwnd As IntPtr, ByVal msg As Integer, ByVal wparam As Integer, ByVal lparam As Integer) As Integer
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function PostMessage(ByVal hwnd As IntPtr, ByVal msg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As Boolean
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function PostMessage(ByVal hwnd As IntPtr, ByVal msg As Integer, ByVal wParam As Integer, ByVal lParam As IntPtr) As Boolean
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function PostMessage(ByVal hwnd As HandleRef, ByVal msg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As Boolean
    End Function

    Public Function PostMessage(hwnd As HandleRef, msg As WindowMessages, wParam As Integer, lParam As Integer) As Boolean
        Return PostMessage(hwnd, CInt(msg), New IntPtr(wParam), New IntPtr(lParam))
    End Function

    <DllImport("User32", CharSet:=CharSet.Auto, SetLastError:=True)>
    Public Function SendInput(ByVal cInputs As Integer, ByRef pInputs As INPUT(), ByVal cbSize As Integer) As Integer
    End Function

    Public Enum InputType
        INPUT_MOUSE = 0
        INPUT_KEYBOARD = 1
        INPUT_HARDWARE = 2
    End Enum

    <StructLayout(LayoutKind.Explicit)>
    Public Structure INPUT
        <FieldOffset(0)> Public dwType As Integer
        <FieldOffset(4)> Public mouseInput As MOUSEINPUT
        <FieldOffset(4)> Public keyboardInput As KEYBDINPUT
        <FieldOffset(4)> Public hardwareInput As HARDWAREINPUT
    End Structure

    <StructLayout(LayoutKind.Explicit)>
    Public Structure KEYBDINPUT
        <FieldOffset(0)> Public wVk As Short
        <FieldOffset(2)> Public wScan As Short
        <FieldOffset(4)> Public dwFlags As Integer
        <FieldOffset(8)> Public time As Integer
        <FieldOffset(12)> Public dwExtraInfo As IntPtr
    End Structure

    <StructLayout(LayoutKind.Explicit)>
    Public Structure HARDWAREINPUT
        <FieldOffset(0)> Public uMsg As Integer
        <FieldOffset(4)> Public wParamL As Short
        <FieldOffset(6)> Public wParamH As Short
    End Structure

    <StructLayout(LayoutKind.Explicit)>
    Public Structure MOUSEINPUT
        <FieldOffset(0)> Public dx As Integer
        <FieldOffset(4)> Public dy As Integer
        <FieldOffset(8)> Public mouseData As Integer
        <FieldOffset(12)> Public dwFlags As Integer
        <FieldOffset(16)> Public time As Integer
        <FieldOffset(20)> Public dwExtraInfo As IntPtr
    End Structure

    <Flags()>
    Public Enum KEYEVENTF As Integer
        NONEORKEYDOWN = 0
        EXTENDEDKEY = 1
        KEYUP = 2
        [UNICODE] = 4
        SCANCODE = 8
    End Enum

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function AttachThreadInput(ByVal idAttach As System.UInt32, ByVal idAttachTo As System.UInt32, ByVal fAttach As Boolean) As Boolean
    End Function

    <DllImport("kernel32.dll")>
    Public Function GetCurrentThread() As IntPtr
    End Function

    <DllImport("kernel32.dll")>
    Public Function SetThreadPriority(ByVal hThread As IntPtr, ByVal nPriority As modWin32.ThreadPriority) As Boolean
    End Function

    Public Enum ThreadPriority As Integer
        THREAD_MODE_BACKGROUND_BEGIN = &H10000
        THREAD_MODE_BACKGROUND_END = &H20000
        ABOVE_NORMAL = 1
        BELOW_NORMAL = -1
        HIGHEST = 2
        IDLE = -15
        LOWEST = -2
        NORMAL = 0
        CRITICAL = 15
    End Enum


    <DllImport("kernel32.dll")>
    Public Function ResumeThread(ByVal hThread As Integer) As Integer
    End Function

    <DllImport("kernel32.dll")>
    Public Function SuspendThread(ByVal hThread As Integer) As Integer
    End Function

    <DllImport("kernel32.dll")>
    Public Function Module32First(ByVal hSnapshot As IntPtr, ByVal lpme As IntPtr) As Boolean
    End Function

    <DllImport("kernel32.dll")>
    Public Function CreateToolhelp32Snapshot(ByVal dwFlags As SnapshotFlags, ByVal th32ProcessID As Integer) As IntPtr
    End Function

    <DllImport("kernel32.dll")>
    Public Function Process32First(ByVal hSnapshot As IntPtr, ByRef lppe As PROCESSENTRY32) As Boolean
    End Function

    <DllImport("kernel32.dll")>
    Public Function Process32Next(ByVal hSnapshot As IntPtr, ByRef lppe As PROCESSENTRY32) As Boolean
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Public Structure PROCESSENTRY32
        Public dwSize As Integer
        Public cntUsage As Integer
        Public th32ProcessID As Integer
        Public th32DefaultHeapID As IntPtr
        Public th32ModuleID As Integer
        Public cntThreads As Integer
        Public th32ParentProcessID As Integer
        Public pcPriClassBase As Integer
        Public dwFlags As Integer
        <VBFixedString(260), MarshalAs(UnmanagedType.ByValTStr, SizeConst:=260)> Public szExeFile As String
    End Structure

    <Flags()>
    Public Enum SnapshotFlags As Integer
        HeapList = &H1
        Process = &H2
        Thread = &H4
        [Module] = &H8
        Module32 = &H10
        Inherit = &H80000000
        All = &H1F
    End Enum

    <StructLayout(LayoutKind.Sequential)>
    Public Class MODULEENTRY32
        Public Const sizeofModuleName As Integer = 256
        Public Const sizeofFileName As Integer = 260
        Public dwSize As Integer
        Public th32ModuleID As Integer
        Public th32ProcessID As Integer
        Public GlblcntUsage As Integer
        Public ProccntUsage As Integer
        Public modBaseAddr As IntPtr = IntPtr.Zero
        Public modBaseSize As Integer
        Public hModule As IntPtr = IntPtr.Zero
    End Class

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function SendMessage(ByVal hWnd As IntPtr, msg As WindowMessages, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function SendMessage(ByVal hWnd As IntPtr, msg As WindowMessages, ByVal wParam As Integer, ByVal lParam As IntPtr) As IntPtr
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function SendMessage(ByVal hWnd As Integer, ByVal Msg As Integer, ByVal wParam As Integer, ByVal lParam As NMUPDOWN) As Integer
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As Integer, ByVal lParam As NMUPDOWN) As IntPtr
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function SendMessage(ByVal hwnd As HandleRef, ByVal msg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    End Function

    Public Function SendMessage(hwnd As HandleRef, msg As WindowMessages, wParam As Integer, lParam As Integer) As Integer
        Return SendMessage(hwnd, CInt(msg), New IntPtr(wParam), New IntPtr(lParam)).ToInt32()
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function SendMessage(ByVal hwnd As Integer, ByVal wMsg As Integer, ByVal wParam As Integer, ByVal lParam As Integer) As Integer
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function SendMessage(ByVal hwnd As IntPtr, ByVal wMsg As Integer, ByVal wParam As Integer, ByVal lParam As Integer) As IntPtr
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function SendMessage(ByVal hwnd As IntPtr, ByVal wMsg As Integer, ByVal wParam As IntPtr, ByVal lParam As Integer) As IntPtr
    End Function

    <DllImport("User32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
    Public Function SendMessage(ByVal hwnd As Integer, ByVal wMsg As Integer, ByVal wParam As Integer, ByVal lParam As String) As Integer
    End Function

    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Function SendMessage(ByVal hwnd As IntPtr, ByVal msg As Int32, ByVal wParam As Int32, <MarshalAs(UnmanagedType.LPWStr)> ByVal lParam As StringBuilder) As Integer
    End Function

    ''' <summary>
    ''' Sends a Win32 message in the form of a string, using an IntPtr windows
    ''' handle rather than an integer (to better cope with 64-bit handles).
    ''' </summary>
    ''' <param name="hwnd">The window handle</param>
    ''' <param name="msg">The windows message</param>
    ''' <param name="wParam">The wide param for the message, in int32 form.</param>
    ''' <param name="lParam">The lparam for the message - expected to be the (secure)
    ''' string to pass in the message.</param>
    ''' <returns>The LRESULT response from the SendMessage call</returns>
    Public Function SendMessage(hwnd As IntPtr, msg As Integer, wParam As Integer, lParam As SecureString) As Integer
        Dim ptr As IntPtr = Marshal.SecureStringToGlobalAllocUnicode(lParam) ' Marshal.SecureStringToBSTR(lParam)
        Try
            SendMessageString(hwnd, msg, wParam, ptr)
        Finally
            If ptr <> Nothing Then Marshal.ZeroFreeGlobalAllocUnicode(ptr)
            ' Marshal.ZeroFreeBSTR(ptr)
        End Try
    End Function

    <DllImport("User32.dll", SetLastError:=True, EntryPoint:="SendMessageW", CharSet:=CharSet.Unicode, ExactSpelling:=True)>
    Public Function SendMessageString(ByVal hWnd As Integer, ByVal Msg As Integer, ByVal Param As Integer, <MarshalAs(UnmanagedType.VBByRefStr)> ByRef lParam As String) As Integer
    End Function

    ''' <summary>
    ''' Sends a Win32 message in the form of a string, using an IntPtr windows
    ''' handle rather than an integer (to better cope with 64-bit handles).
    ''' </summary>
    ''' <param name="hwnd">The window handle</param>
    ''' <param name="msg">The windows message</param>
    ''' <param name="wParam">The wide param for the message, in int32 form.</param>
    ''' <param name="lParam">The lparam for the message - expected to be the string
    ''' to pass in the message.</param>
    ''' <returns>The LRESULT response from the SendMessage call</returns>
    <DllImport("User32.dll", SetLastError:=True, EntryPoint:="SendMessageW", CharSet:=CharSet.Unicode, ExactSpelling:=True)>
    Public Function SendMessageString(ByVal hwnd As IntPtr, ByVal msg As Integer, ByVal wParam As Integer, <MarshalAs(UnmanagedType.VBByRefStr)> ByRef lParam As String) As Integer
    End Function

    ''' <summary>
    ''' Sends a Win32 message in the form of a string, using an IntPtr windows
    ''' handle rather than an integer (to better cope with 64-bit handles).
    ''' </summary>
    ''' <param name="hwnd">The window handle</param>
    ''' <param name="msg">The windows message</param>
    ''' <param name="wParam">The wide param for the message, in int32 form.</param>
    ''' <param name="lParam">The lparam for the message - expected to be the string
    ''' to pass in the message.</param>
    ''' <returns>The LRESULT response from the SendMessage call</returns>
    <DllImport("User32.dll", SetLastError:=True, EntryPoint:="SendMessageW", CharSet:=CharSet.Unicode, ExactSpelling:=True)>
    Public Function SendMessageString(ByVal hwnd As IntPtr, ByVal msg As Integer, ByVal wParam As Integer, ByVal lParam As IntPtr) As Integer
    End Function

    <DllImport("user32", SetLastError:=True)>
    Public Function SetCursorPos(ByVal x As Integer, ByVal y As Integer) As Integer
    End Function

    <DllImport("user32", CharSet:=CharSet.Auto, SetLastError:=True)>
    Public Function SetWindowText(ByVal hwnd As Integer, <MarshalAs(UnmanagedType.VBByRefStr)> ByRef lpString As String) As Boolean
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetKeyState(ByVal nVirtKey As VirtualKeyCode) As Integer
    End Function

    Public Function IsKeyDown(ByVal keys As VirtualKeyCode) As Boolean
        Return (GetKeyState(keys) And &H8000) <> 0
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetAsyncKeyState(ByVal vKey As VirtualKeyCode) As Short
    End Function


    <DllImport("User32.dll", SetLastError:=True)>
    Public Function RedrawWindow(ByVal hwnd As IntPtr, ByRef lprcUpdate As RECT, ByVal hrgnUpdate As IntPtr, ByVal fuRedraw As Integer) As Integer
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function RedrawWindow(ByVal hWnd As IntPtr, ByVal lprcUpdate As IntPtr, ByVal hrgnUpdate As IntPtr, ByVal flags As Integer) As Integer
    End Function

    <DllImport("user32", SetLastError:=True)>
    Public Function GetDesktopWindow() As IntPtr
    End Function

    <DllImport("user32", SetLastError:=True)>
    Public Function GetWindowDC(ByVal hWnd As IntPtr) As IntPtr
    End Function

    ''' <summary>
    ''' Gets a device context to the client area of the window (as opposed to the
    ''' entire window as returned in GetWindowDC) defined by the specified handle.
    ''' </summary>
    ''' <param name="hwnd">The window handle</param>
    ''' <returns>A pointer to the device context for the client area of the
    ''' specified window.</returns>
    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetDC(ByVal hwnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32", SetLastError:=True)>
    Public Function ReleaseDC(ByVal hWnd As IntPtr, ByVal hDC As IntPtr) As IntPtr
    End Function

    Declare Function GetDeviceCaps Lib "gdi32" (ByVal hDC As IntPtr, ByVal nIndex As Int32) As Int32
    Public Const WU_LOGPIXELSX As Integer = 88
    Public Const WU_LOGPIXELSY As Integer = 90

    Public Declare Function CreateCompatibleDC Lib "gdi32" (ByVal hDC As IntPtr) As IntPtr
    Public Declare Function CreateCompatibleBitmap Lib "gdi32" (ByVal hDC As IntPtr, ByVal width As Integer, ByVal height As Integer) As IntPtr
    Public Declare Function DeleteObject Lib "gdi32" (ByVal hObject As IntPtr) As Boolean
    Public Declare Function DeleteDC Lib "gdi32" (ByVal hDC As IntPtr) As Boolean
    Public Declare Function SelectObject Lib "gdi32" (ByVal hDC As IntPtr, ByVal hObject As IntPtr) As IntPtr
    Public Const SRCCOPY As Integer = &HCC0020   'BitBlt dwRop parameter
    Public Declare Function BitBlt Lib "gdi32" (ByVal hObject As IntPtr, ByVal nXDest As Integer, ByVal nYDest As Integer, ByVal nWidth As Integer, ByVal nHeight As Integer, ByVal hObjectSource As IntPtr, ByVal nXSrc As Integer, ByVal nYSrc As Integer, ByVal dwRop As Integer) As Boolean


    <DllImport("user32", SetLastError:=True)>
    Public Function ChildWindowFromPoint(ByVal h As Integer, ByVal p As POINTAPI) As Int32
    End Function

    Public Enum CWPFlags
        ''' <summary>
        ''' Does not skip any child windows
        ''' </summary>
        CWP_ALL = 0
        ''' <summary>
        ''' Skips invisible child windows
        ''' </summary>
        CWP_SKIPINVISIBLE = 1
        ''' <summary>
        ''' Skips disabled child windows
        ''' </summary>
        CWP_SKIPDISABLED = 2
        ''' <summary>
        ''' Skips transparent child windows
        ''' </summary>
        ''' <remarks></remarks>
        CWP_SKIPTRANSPARENT = 4
    End Enum
    <DllImport("user32", SetLastError:=True)>
    Public Function RealChildWindowFromPoint(ByVal hwndParent As IntPtr, ByVal ptParentClientCoords As POINTAPI) As IntPtr
    End Function

    <DllImport("user32", SetLastError:=True)>
    Public Function ChildWindowFromPointEx(ByVal hWndParent As IntPtr, ByVal P As POINTAPI, ByVal uFlags As CWPFlags) As IntPtr
    End Function

    <DllImport("user32", SetLastError:=True)>
    Public Function GetWindowRect(ByVal hwnd As IntPtr, ByRef lpRect As RECT) As Integer
    End Function

    <DllImport("kernel32.dll")>
    Public Function ResumeThread(ByVal hThread As IntPtr) As Integer
    End Function

    <DllImport("kernel32.dll")>
    Public Function GetCurrentThreadId() As Integer
    End Function

    <DllImport("user32", CharSet:=CharSet.Auto, SetLastError:=True, ExactSpelling:=True)>
    Public Function IsWindow(ByVal hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32", CharSet:=CharSet.Auto, SetLastError:=True, ExactSpelling:=True)>
    Public Function SetForegroundWindow(ByVal handle As IntPtr) As Boolean
    End Function

    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Auto, ExactSpelling:=True)>
    Public Function GetForegroundWindow() As IntPtr
    End Function

    <DllImport("RemoteFunctions.dll", CharSet:=CharSet.Auto, SetLastError:=True, ExactSpelling:=True)>
    Public Function DoSetForegroundWindowInRemoteMemory(ByVal hProcess As IntPtr, ByVal hWnd As IntPtr) As Integer
    End Function

    <Flags()>
    Public Enum CreationFlags
        CREATE_BREAKAWAY_FROM_JOB = &H1000000
        CREATE_DEFAULT_ERROR_MODE = &H4000000
        CREATE_NEW_CONSOLE = &H10
        NORMAL_PRIORITY_CLASS = &H20
        CREATE_NEW_PROCESS_GROUP = &H200
        CREATE_NO_WINDOW = &H8000000
        CREATE_PROTECTED_PROCESS = &H40000
        CREATE_PRESERVE_CODE_AUTHZ_LEVEL = &H2000000
        CREATE_SEPARATE_WOW_VDM = &H1000
        CREATE_SUSPENDED = &H4
        CREATE_UNICODE_ENVIRONMENT = &H400
        DEBUG_ONLY_THIS_PROCESS = &H2
        DEBUG_PROCESS = &H1
        DETACHED_PROCESS = &H8
        EXTENDED_STARTUPINFO_PRESENT = &H80000
    End Enum

    Public Enum GetWndConsts
        GW_HWNDFIRST = 0
        GW_HWNDLAST = 1
        GW_HWNDNEXT = 2
        GW_HWNDPREV = 3
        GW_OWNER = 4
        GW_CHILD = 5
        GW_ENABLEDPOPUP = 6
        GW_MAX = 6
    End Enum

    <StructLayout(LayoutKind.Sequential)>
    Public Structure POINTAPI
        Public x As Integer
        Public y As Integer

        Public Sub New(ByVal xcoord As Integer, ByVal ycoord As Integer)
            x = xcoord
            y = ycoord
        End Sub
        ''' <summary>
        ''' Converts the given POINTAPI into a Point structure.
        ''' This is widening since all POINTAPI values can be safely represented as
        ''' a Point. As such, the compiler will implicitly convert a POINTAPI to
        ''' a Point without an explicit cast.
        ''' </summary>
        ''' <param name="val">The POINTAPI value to convert</param>
        ''' <returns>A Point with the same value as the given POINTAPI</returns>
        Public Shared Widening Operator CType(ByVal val As POINTAPI) As Point
            Return New Point(val.x, val.y)
        End Operator

        ''' <summary>
        ''' Converts the given Point into a POINTAPI structure.
        ''' This is widening since all Point values can be safely represented as
        ''' a POINTAPI. As such, the compiler will implicitly convert a Point to
        ''' a POINTAPI without an explicit cast.
        ''' </summary>
        ''' <param name="val">The Point value to convert</param>
        ''' <returns>A POINTAPI with the same value as the given Point</returns>
        Public Shared Widening Operator CType(ByVal val As Point) As POINTAPI
            Return New POINTAPI(val.X, val.Y)
        End Operator

    End Structure

    <StructLayout(LayoutKind.Sequential)> Structure WINDOWINFO
        Dim cbSize As UInt32
        Dim rcWindow As RECT
        Dim rcClient As RECT
        Dim dwStyle As UInt32
        Dim dwExStyle As UInt32
        Dim dwWindowStatus As Int32
        Dim cxWindowBorders As UInt32
        Dim cyWindowBorders As UInt32
        Dim atomWindowType As UInt16
        Dim wCreatorVersion As UInt16
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure KEYBOARDHOOKSTRUCT
        Dim vkCode As VirtualKeyCode
        Dim scanCode As Integer
        Dim flags As Integer
        Dim time As Integer
        Dim dwExtraInfo As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure PROCESS_INFORMATION
        ''' <summary>
        ''' Handle to the process.
        ''' </summary>
        Public hProcess As IntPtr
        ''' <summary>
        ''' Handle to the thread.
        ''' </summary>
        Public hThread As IntPtr
        ''' <summary>
        ''' The ID of the process.
        ''' </summary>
        Public dwProcessId As Integer
        ''' <summary>
        ''' The ID of the thread.
        ''' </summary>
        Public dwThreadId As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure PROCESS_BASIC_INFORMATION
        Public Reserved1 As IntPtr
        Public PebBaseAddress As IntPtr
        Public Reserved2_0 As IntPtr
        Public Reserved2_1 As IntPtr
        Public UniqueProcessId As IntPtr
        Public InheritedFromUniqueProcessId As IntPtr
    End Structure

    <DllImport("ntdll.dll")>
    Private Function NtQueryInformationProcess(ByVal processHandle As IntPtr, ByVal processInformationClass As Integer, ByRef processInformation As PROCESS_BASIC_INFORMATION, ByVal processInformationLength As Integer, ByVal returnLength As Integer) As Integer
    End Function

    Public Function GetParentProcessID(ByVal handle As IntPtr) As Integer
        Dim pbi As New PROCESS_BASIC_INFORMATION
        Dim returnLength As Integer = Nothing
        Dim status As Integer = NtQueryInformationProcess(handle, 0, pbi, Marshal.SizeOf(pbi), returnLength)
        If (status <> 0) Then
            Throw New Win32Exception(status)
        End If

        Try
            Return pbi.InheritedFromUniqueProcessId.ToInt32
        Catch ex As ArgumentException
            Return Nothing
        End Try
    End Function

    <DllImport("kernel32.dll")>
    Public Function GetCurrentProcessId() As Integer
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Public Structure SECURITY_ATTRIBUTES
        Public nLength As Integer
        Public lpSecurityDescriptor As SECURITY_DESCRIPTOR
        Public bInheritHandle As Integer
    End Structure

    <StructLayoutAttribute(LayoutKind.Sequential)>
    Public Structure SECURITY_DESCRIPTOR
        Public revision As Byte
        Public size As Byte
        Public control As Short
        Public owner As IntPtr
        Public group As IntPtr
        Public sacl As IntPtr
        Public dacl As IntPtr
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure STARTUPINFO
        Public cb As Integer
        Public cbReserved2 As Short
        Public dwFillAttribute As Integer
        Public dwFlags As Integer
        Public dwX As Integer
        Public dwXCountChars As Integer
        Public dwXSize As Integer
        Public dwY As Integer
        Public dwYCountChars As Integer
        Public dwYSize As Integer
        Public hStdError As Integer
        Public hStdInput As Integer
        Public hStdOutput As Integer
        Public lpDesktop As String
        Public lpReserved As String
        Public lpReserved2 As Integer
        Public lpTitle As String
        Public wShowWindow As Short
    End Structure

    <Flags()>
    Public Enum ThreadAccess
        DIRECT_IMPERSONATION = &H200
        GET_CONTEXT = &H8
        IMPERSONATE = &H100
        QUERY_INFORMATION = &H40
        SET_CONTEXT = &H10
        SET_INFORMATION = &H20
        SET_THREAD_TOKEN = &H80
        SUSPEND_RESUME = &H2
        TERMINATE = &H1
    End Enum

    Public Enum MOUSEEVENTF As Integer
        ABSOLUTE = &H8000
        LEFTDOWN = &H2
        LEFTUP = &H4
        MIDDLEDOWN = &H20
        MIDDLEUP = &H40
        MOVE = &H1
        RIGHTDOWN = &H8
        RIGHTUP = &H10
    End Enum

    Public Const WS_ACTIVECAPTION As Integer = 1

    Public Enum WindowStyles
        WS_OVERLAPPED = &H0
        WS_POPUP = &H80000000
        WS_CHILD = &H40000000
        WS_MINIMIZE = &H20000000
        WS_VISIBLE = &H10000000
        WS_DISABLED = &H8000000
        WS_CLIPSIBLINGS = &H4000000
        WS_CLIPCHILDREN = &H2000000
        WS_MAXIMIZE = &H1000000
        WS_BORDER = &H800000
        WS_DLGFRAME = &H400000
        WS_VSCROLL = &H200000
        WS_HSCROLL = &H100000
        WS_SYSMENU = &H80000
        WS_THICKFRAME = &H40000
        WS_GROUP = &H20000
        WS_TABSTOP = &H10000
        WS_MINIMIZEBOX = &H20000
        WS_MAXIMIZEBOX = &H10000
        WS_CAPTION = WS_BORDER Or WS_DLGFRAME
        WS_TILED = WS_OVERLAPPED
        WS_ICONIC = WS_MINIMIZE
        WS_SIZEBOX = WS_THICKFRAME
        WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW
        WS_OVERLAPPEDWINDOW = WS_OVERLAPPED Or WS_CAPTION Or WS_SYSMENU Or WS_THICKFRAME Or WS_MINIMIZEBOX Or WS_MAXIMIZEBOX
        WS_POPUPWINDOW = WS_POPUP Or WS_BORDER Or WS_SYSMENU
        WS_CHILDWINDOW = WS_CHILD

        WS_EX_ACCEPTFILES = &H10&
        WS_EX_DLGMODALFRAME = &H1&
        WS_EX_NOPARENTNOTIFY = &H4&
        WS_EX_TOPMOST = &H8&
        WS_EX_TRANSPARENT = &H20&
        WS_EX_TOOLWINDOW = &H80&
        WS_EX_CONTROLPARENT = &H10000
        WS_EX_STATICEDGE = &H20000
        WS_EX_APPWINDOW = &H40000
    End Enum



    Public Enum HookTypes
        WH_JOURNALRECORD = 0
        WH_JOURNALPLAYBACK = 1
        WH_KEYBOARD = 2
        WH_GETMESSAGE = 3
        WH_CALLWNDPROC = 4
        WH_CBT = 5
        WH_SYSMSGFILTER = 6
        WH_MOUSE = 7
        WH_HARDWARE = 8
        WH_DEBUG = 9
        WH_SHELL = 10
        WH_FOREGROUNDIDLE = 11
        WH_CALLWNDPROCRET = 12
        WH_KEYBOARD_LL = 13
        WH_MOUSE_LL = 14
    End Enum

    Public Enum VirtualKeyCode As Integer
        VK_TAB = 9
        VK_RETURN = &HD
        VK_SHIFT = &H10
        VK_CONTROL = &H11
        VK_MENU = &H12          'Alt
        VK_LSHIFT = &HA0
        VK_RSHIFT = &HA1
        VK_LCONTROL = &HA2
        VK_RCONTROL = &HA3
        VK_LMENU = &HA4
        VK_RMENU = &HA5
        VK_HOME = &H24
        VK_ESCAPE = &H1B
        VK_SPACE = &H20
        VK_INSERT = &H2D
        VK_DELETE = &H2E
        VK_UP = &H26
        VK_DOWN = &H28
        VK_LEFT = &H25
        VK_RIGHT = &H27
        VK_END = &H23
        VK_PRIOR = &H21 'PAGE UP key
        VK_NEXT = &H22 'PAGE DOWN key
        VK_F1 = &H70
        VK_F2 = &H71
        VK_F3 = &H72
        VK_F4 = &H73
        VK_F5 = &H74
        VK_F6 = &H75
        VK_F7 = &H76
        VK_F8 = &H77
        VK_F9 = &H78
        VK_F10 = &H79
        VK_F11 = &H7A
        VK_F12 = &H7B
        VK_DIVIDE = &H6F
        VK_NUMLOCK = &H90
    End Enum


    'Bitfields in wParam for WM_xBUTTONy messages where x=L/R and y=UP/DOWN
    Public Const MK_CONTROL As Int32 = 8
    Public Const MK_SHIFT As Int32 = 4

    Public Enum WindowMessages As Integer
        WM_ACTIVATE = &H6
        WM_ACTIVATEAPP = &H1C
        WM_AFXFIRST = &H360
        WM_AFXLAST = &H37F
        WM_APP = &H8000
        WM_ASKCBFORMATNAME = &H30C
        WM_CANCELJOURNAL = &H4B
        WM_CANCELMODE = &H1F
        WM_CAPTURECHANGED = &H215
        WM_CHANGECBCHAIN = &H30D
        WM_CHAR = &H102
        WM_CHARTOITEM = &H2F
        WM_CHILDACTIVATE = &H22
        WM_CLEAR = &H303
        WM_CLOSE = &H10
        WM_COMMAND = &H111
        WM_COMPACTING = &H41
        WM_COMPAREITEM = &H39
        WM_CONTEXTMENU = &H7B
        WM_COPY = &H301
        WM_COPYDATA = &H4A
        WM_CREATE = &H1
        WM_CTLCOLORBTN = &H135
        WM_CTLCOLORDLG = &H136
        WM_CTLCOLOREDIT = &H133
        WM_CTLCOLORLISTBOX = &H134
        WM_CTLCOLORMSGBOX = &H132
        WM_CTLCOLORSCROLLBAR = &H137
        WM_CTLCOLORSTATIC = &H138
        WM_CUT = &H300
        WM_DEADCHAR = &H103
        WM_DELETEITEM = &H2D
        WM_DESTROY = &H2
        WM_DESTROYCLIPBOARD = &H307
        WM_DEVICECHANGE = &H219
        WM_DEVMODECHANGE = &H1B
        WM_DISPLAYCHANGE = &H7E
        WM_DRAWCLIPBOARD = &H308
        WM_DRAWITEM = &H2B
        WM_DROPFILES = &H233
        WM_ENABLE = &HA
        WM_ENDSESSION = &H16
        WM_ENTERIDLE = &H121
        WM_ENTERMENULOOP = &H211
        WM_ENTERSIZEMOVE = &H231
        WM_ERASEBKGND = &H14
        WM_EXITMENULOOP = &H212
        WM_EXITSIZEMOVE = &H232
        WM_FONTCHANGE = &H1D
        WM_GETDLGCODE = &H87
        WM_GETFONT = &H31
        WM_GETHOTKEY = &H33
        WM_GETICON = &H7F
        WM_GETMINMAXINFO = &H24
        WM_GETOBJECT = &H3D
        WM_GETSYSMENU = &H313
        WM_GETTEXT = &HD
        WM_GETTEXTLENGTH = &HE
        WM_HANDHELDFIRST = &H358
        WM_HANDHELDLAST = &H35F
        WM_HELP = &H53
        WM_HOTKEY = &H312
        WM_HSCROLL = &H114
        WM_HSCROLLCLIPBOARD = &H30E
        WM_ICONERASEBKGND = &H27
        WM_IME_CHAR = &H286
        WM_IME_COMPOSITION = &H10F
        WM_IME_COMPOSITIONFULL = &H284
        WM_IME_CONTROL = &H283
        WM_IME_ENDCOMPOSITION = &H10E
        WM_IME_KEYDOWN = &H290
        WM_IME_KEYLAST = &H10F
        WM_IME_KEYUP = &H291
        WM_IME_NOTIFY = &H282
        WM_IME_REQUEST = &H288
        WM_IME_SELECT = &H285
        WM_IME_SETCONTEXT = &H281
        WM_IME_STARTCOMPOSITION = &H10D
        WM_INITDIALOG = &H110
        WM_INITMENU = &H116
        WM_INITMENUPOPUP = &H117
        WM_INPUTLANGCHANGE = &H51
        WM_INPUTLANGCHANGEREQUEST = &H50
        WM_KEYDOWN = &H100
        WM_KEYFIRST = &H100
        WM_KEYLAST = &H108
        WM_KEYUP = &H101
        WM_KILLFOCUS = &H8
        WM_MDIACTIVATE = &H222
        WM_MDICASCADE = &H227
        WM_MDICREATE = &H220
        WM_MDIDESTROY = &H221
        WM_MDIGETACTIVE = &H229
        WM_MDIICONARRANGE = &H228
        WM_MDIMAXIMIZE = &H225
        WM_MDINEXT = &H224
        WM_MDIREFRESHMENU = &H234
        WM_MDIRESTORE = &H223
        WM_MDISETMENU = &H230
        WM_MDITILE = &H226
        WM_MEASUREITEM = &H2C
        WM_MENUCHAR = &H120
        WM_MENUCOMMAND = &H126
        WM_MENUDRAG = &H123
        WM_MENUGETOBJECT = &H124
        WM_MENURBUTTONUP = &H122
        WM_MENUSELECT = &H11F
        WM_MOUSEACTIVATE = &H21
        WM_MOUSEFIRST = &H200
        WM_MOUSEHOVER = &H2A1
        WM_MOUSELAST = &H20A
        WM_MOUSELEAVE = &H2A3
        WM_MOUSEMOVE = &H200
        WM_MOUSEWHEEL = &H20A
        WM_MOVE = &H3
        WM_MOVING = &H216
        WM_NCACTIVATE = &H86
        WM_NCCALCSIZE = &H83
        WM_NCCREATE = &H81
        WM_NCDESTROY = &H82
        WM_NCHITTEST = &H84
        WM_NCLBUTTONDBLCLK = &HA3
        WM_NCLBUTTONDOWN = &HA1
        WM_NCLBUTTONUP = &HA2
        WM_NCMBUTTONDBLCLK = &HA9
        WM_NCMBUTTONDOWN = &HA7
        WM_NCMBUTTONUP = &HA8
        WM_NCMOUSEHOVER = &H2A0
        WM_NCMOUSELEAVE = &H2A2
        WM_NCMOUSEMOVE = &HA0
        WM_NCPAINT = &H85
        WM_NCRBUTTONDBLCLK = &HA6
        WM_NCRBUTTONDOWN = &HA4
        WM_NCRBUTTONUP = &HA5
        WM_NEXTDLGCTL = &H28
        WM_NEXTMENU = &H213
        WM_NOTIFY = &H4E
        WM_REFLECT = WM_USER + &H1C00
        WM_NOTIFYFORMAT = &H55
        WM_NULL = &H0
        WM_PAINT = &HF
        WM_PAINTCLIPBOARD = &H309
        WM_PAINTICON = &H26
        WM_PALETTECHANGED = &H311
        WM_PALETTEISCHANGING = &H310
        WM_PARENTNOTIFY = &H210
        WM_PASTE = &H302
        WM_PENWINFIRST = &H380
        WM_PENWINLAST = &H38F
        WM_POWER = &H48
        WM_PRINT = &H317
        WM_PRINTCLIENT = &H318
        WM_QUERYDRAGICON = &H37
        WM_QUERYENDSESSION = &H11
        WM_QUERYNEWPALETTE = &H30F
        WM_QUERYOPEN = &H13
        WM_QUEUESYNC = &H23
        WM_QUIT = &H12
        WM_LBUTTONDOWN = &H201
        WM_LBUTTONUP = &H202
        WM_LBUTTONDBLCLK = &H203
        WM_RBUTTONDOWN = &H204
        WM_RBUTTONUP = &H205
        WM_RBUTTONDBLCLK = &H206
        WM_MBUTTONDOWN = &H207
        WM_MBUTTONUP = &H208
        WM_MBUTTONDBLCLK = &H209
        WM_RENDERALLFORMATS = &H306
        WM_RENDERFORMAT = &H305
        WM_SETCURSOR = &H20
        WM_SETFOCUS = &H7
        WM_SETFONT = &H30
        WM_SETHOTKEY = &H32
        WM_SETICON = &H80
        WM_SETREDRAW = &HB
        WM_SETTEXT = &HC
        WM_SETTINGCHANGE = &H1A
        WM_SHOWWINDOW = &H18
        WM_SIZE = &H5
        WM_SIZECLIPBOARD = &H30B
        WM_SIZING = &H214
        WM_SPOOLERSTATUS = &H2A
        WM_STYLECHANGED = &H7D
        WM_STYLECHANGING = &H7C
        WM_SYNCPAINT = &H88
        WM_SYSCHAR = &H106
        WM_SYSCOLORCHANGE = &H15
        WM_SYSCOMMAND = &H112
        WM_SYSDEADCHAR = &H107
        WM_SYSKEYDOWN = &H104
        WM_SYSKEYUP = &H105
        WM_TCARD = &H52
        WM_TIMECHANGE = &H1E
        WM_TIMER = &H113
        WM_UNDO = &H304
        WM_UNINITMENUPOPUP = &H125
        WM_USER = &H400
        WM_USERCHANGED = &H54
        WM_VKEYTOITEM = &H2E
        WM_VSCROLL = &H115
        WM_VSCROLLCLIPBOARD = &H30A
        WM_WINDOWPOSCHANGED = &H47
        WM_WINDOWPOSCHANGING = &H46
        WM_WININICHANGE = &H1A
        EM_GETSEL = &HB0
        EM_SETSEL = &HB1
        EM_GETRECT = &HB2
        EM_SETRECT = &HB3
        EM_SETRECTNP = &HB4
        EM_SCROLL = &HB5
        EM_LINESCROLL = &HB6
        EM_SCROLLCARET = &HB7
        EM_GETMODIFY = &HB8
        EM_SETMODIFY = &HB9
        EM_GETLINECOUNT = &HBA
        EM_LINEINDEX = &HBB
        EM_SETHANDLE = &HBC
        EM_GETHANDLE = &HBD
        EM_GETTHUMB = &HBE
        EM_LINELENGTH = &HC1
        EM_REPLACESEL = &HC2
        EM_GETLINE = &HC4
        EM_LIMITTEXT = &HC5
        EM_CANUNDO = &HC6
        EM_UNDO = &HC7
        EM_FMTLINES = &HC8
        EM_LINEFROMCHAR = &HC9
        EM_SETTABSTOPS = &HCB
        EM_SETPASSWORDCHAR = &HCC
        EM_EMPTYUNDOBUFFER = &HCD
        EM_GETFIRSTVISIBLELINE = &HCE
        EM_SETREADONLY = &HCF
        EM_SETWORDBREAKPROC = &HD0
        EM_GETWORDBREAKPROC = &HD1
        EM_GETPASSWORDCHAR = &HD2
        EM_SETMARGINS = &HD3
        EM_GETMARGINS = &HD4
        EM_SETLIMITTEXT = EM_LIMITTEXT
        EM_GETLIMITTEXT = &HD5
        EM_POSFROMCHAR = &HD6
        EM_CHARFROMPOS = &HD7
        EM_SETIMESTATUS = &HD8
        EM_GETIMESTATUS = &HD9
        BM_GETCHECK = &HF0
        BM_SETCHECK = &HF1
        BM_GETSTATE = &HF2
        BM_SETSTATE = &HF3
        BM_SETSTYLE = &HF4
        BM_CLICK = &HF5
        BM_GETIMAGE = &HF6
        BM_SETIMAGE = &HF7
        STM_SETICON = &H170
        STM_GETICON = &H171
        STM_SETIMAGE = &H172
        STM_GETIMAGE = &H173
        STM_MSGMAX = &H174
        DM_GETDEFID = (WM_USER + 0)
        DM_SETDEFID = (WM_USER + 1)
        DM_REPOSITION = (WM_USER + 2)
        'Listbox messages
        LB_ADDSTRING = &H180
        LB_INSERTSTRING = &H181
        LB_DELETESTRING = &H182
        LB_SELITEMRANGEEX = &H183
        LB_RESETCONTENT = &H184
        LB_SETSEL = &H185
        LB_SETCURSEL = &H186
        LB_GETSEL = &H187
        LB_GETCURSEL = &H188
        LB_GETTEXT = &H189
        LB_GETTEXTLEN = &H18A
        LB_GETCOUNT = &H18B
        LB_SELECTSTRING = &H18C
        LB_DIR = &H18D
        LB_GETTOPINDEX = &H18E
        LB_FINDSTRING = &H18F
        LB_GETSELCOUNT = &H190
        LB_GETSELITEMS = &H191
        LB_SETTABSTOPS = &H192
        LB_GETHORIZONTALEXTENT = &H193
        LB_SETHORIZONTALEXTENT = &H194
        LB_SETCOLUMNWIDTH = &H195
        LB_ADDFILE = &H196
        LB_SETTOPINDEX = &H197
        LB_GETITEMRECT = &H198
        LB_GETITEMDATA = &H199
        LB_SETITEMDATA = &H19A
        LB_SELITEMRANGE = &H19B
        LB_SETANCHORINDEX = &H19C
        LB_GETANCHORINDEX = &H19D
        LB_SETCARETINDEX = &H19E
        LB_GETCARETINDEX = &H19F
        LB_SETITEMHEIGHT = &H1A0
        LB_GETITEMHEIGHT = &H1A1
        LB_FINDSTRINGEXACT = &H1A2
        LB_SETLOCALE = &H1A5
        LB_GETLOCALE = &H1A6
        LB_SETCOUNT = &H1A7
        LB_INITSTORAGE = &H1A8
        LB_ITEMFROMPOINT = &H1A9
        LB_MULTIPLEADDSTRING = &H1B1
        LB_GETLISTBOXINFO = &H1B2
        LB_MSGMAX_501 = &H1B3
        LB_MSGMAX_WCE4 = &H1B1
        LB_MSGMAX_4 = &H1B0
        LB_MSGMAX_PRE4 = &H1A8
        'combobox messages
        CB_GETEDITSEL = &H140
        CB_LIMITTEXT = &H141
        CB_SETEDITSEL = &H142
        CB_ADDSTRING = &H143
        CB_DELETESTRING = &H144
        CB_DIR = &H145
        CB_GETCOUNT = &H146
        CB_GETCURSEL = &H147
        CB_GETLBTEXT = &H148
        CB_GETLBTEXTLEN = &H149
        CB_INSERTSTRING = &H14A
        CB_RESETCONTENT = &H14B
        CB_FINDSTRING = &H14C
        CB_SELECTSTRING = &H14D
        CB_SETCURSEL = &H14E
        CB_SHOWDROPDOWN = &H14F
        CB_GETITEMDATA = &H150
        CB_SETITEMDATA = &H151
        CB_GETDROPPEDCONTROLRECT = &H152
        CB_SETITEMHEIGHT = &H153
        CB_GETITEMHEIGHT = &H154
        CB_SETEXTENDEDUI = &H155
        CB_GETEXTENDEDUI = &H156
        CB_GETDROPPEDSTATE = &H157
        CB_FINDSTRINGEXACT = &H158
        CB_SETLOCALE = &H159
        CB_GETLOCALE = &H15A
        CB_GETTOPINDEX = &H15B
        CB_SETTOPINDEX = &H15C
        CB_GETHORIZONTALEXTENT = &H15D
        CB_SETHORIZONTALEXTENT = &H15E
        CB_GETDROPPEDWIDTH = &H15F
        CB_SETDROPPEDWIDTH = &H160
        CB_INITSTORAGE = &H161
        CB_MULTIPLEADDSTRING = &H163
        CB_GETCOMBOBOXINFO = &H164
        CB_MSGMAX_501 = &H165
        CB_MSGMAX_WCE400 = &H163
        CB_MSGMAX_400 = &H162
        CB_MSGMAX_PRE400 = &H15B
        SBM_SETPOS = &HE0
        SBM_GETPOS = &HE1
        SBM_SETRANGE = &HE2
        SBM_SETRANGEREDRAW = &HE6
        SBM_GETRANGE = &HE3
        SBM_ENABLE_ARROWS = &HE4
        SBM_SETSCROLLINFO = &HE9
        SBM_GETSCROLLINFO = &HEA
        SBM_GETSCROLLBARINFO = &HEB
        LVM_FIRST = &H1000 ' ListView messages
        TV_FIRST = &H1100 ' TreeView messages
        HDM_FIRST = &H1200 ' Header messages
        TCM_FIRST = &H1300 ' Tab control messages
        PGM_FIRST = &H1400 ' Pager control messages
        ECM_FIRST = &H1500 ' Edit control messages
        BCM_FIRST = &H1600 ' Button control messages
        CBM_FIRST = &H1700 ' Combobox control messages
        ' Common control shared messages
        CCM_FIRST = &H2000
        CCM_LAST = (CCM_FIRST + &H200)
        CCM_SETBKCOLOR = (CCM_FIRST + 1)
        CCM_SETCOLORSCHEME = (CCM_FIRST + 2)
        CCM_GETCOLORSCHEME = (CCM_FIRST + 3)
        CCM_GETDROPTARGET = (CCM_FIRST + 4)
        CCM_SETUNICODEFORMAT = (CCM_FIRST + 5)
        CCM_GETUNICODEFORMAT = (CCM_FIRST + 6)
        CCM_SETVERSION = (CCM_FIRST + &H7)
        CCM_GETVERSION = (CCM_FIRST + &H8)
        CCM_SETNOTIFYWINDOW = (CCM_FIRST + &H9)
        CCM_SETWINDOWTHEME = (CCM_FIRST + &HB)
        CCM_DPISCALE = (CCM_FIRST + &HC)
        HDM_GETITEMCOUNT = (HDM_FIRST + 0)
        HDM_INSERTITEMA = (HDM_FIRST + 1)
        HDM_INSERTITEMW = (HDM_FIRST + 10)
        HDM_DELETEITEM = (HDM_FIRST + 2)
        HDM_GETITEMA = (HDM_FIRST + 3)
        HDM_GETITEMW = (HDM_FIRST + 11)
        HDM_SETITEMA = (HDM_FIRST + 4)
        HDM_SETITEMW = (HDM_FIRST + 12)
        HDM_LAYOUT = (HDM_FIRST + 5)
        HDM_HITTEST = (HDM_FIRST + 6)
        HDM_GETITEMRECT = (HDM_FIRST + 7)
        HDM_SETIMAGELIST = (HDM_FIRST + 8)
        HDM_GETIMAGELIST = (HDM_FIRST + 9)
        HDM_ORDERTOINDEX = (HDM_FIRST + 15)
        HDM_CREATEDRAGIMAGE = (HDM_FIRST + 16)
        HDM_GETORDERARRAY = (HDM_FIRST + 17)
        HDM_SETORDERARRAY = (HDM_FIRST + 18)
        HDM_SETHOTDIVIDER = (HDM_FIRST + 19)
        HDM_SETBITMAPMARGIN = (HDM_FIRST + 20)
        HDM_GETBITMAPMARGIN = (HDM_FIRST + 21)
        HDM_SETUNICODEFORMAT = CCM_SETUNICODEFORMAT
        HDM_GETUNICODEFORMAT = CCM_GETUNICODEFORMAT
        HDM_SETFILTERCHANGETIMEOUT = (HDM_FIRST + 22)
        HDM_EDITFILTER = (HDM_FIRST + 23)
        HDM_CLEARFILTER = (HDM_FIRST + 24)
        TB_ENABLEBUTTON = (WM_USER + 1)
        TB_CHECKBUTTON = (WM_USER + 2)
        TB_PRESSBUTTON = (WM_USER + 3)
        TB_HIDEBUTTON = (WM_USER + 4)
        TB_INDETERMINATE = (WM_USER + 5)
        TB_MARKBUTTON = (WM_USER + 6)
        TB_ISBUTTONENABLED = (WM_USER + 9)
        TB_ISBUTTONCHECKED = (WM_USER + 10)
        TB_ISBUTTONPRESSED = (WM_USER + 11)
        TB_ISBUTTONHIDDEN = (WM_USER + 12)
        TB_ISBUTTONINDETERMINATE = (WM_USER + 13)
        TB_ISBUTTONHIGHLIGHTED = (WM_USER + 14)
        TB_SETSTATE = (WM_USER + 17)
        TB_GETSTATE = (WM_USER + 18)
        TB_ADDBITMAP = (WM_USER + 19)
        TB_ADDBUTTONSA = (WM_USER + 20)
        TB_INSERTBUTTONA = (WM_USER + 21)
        TB_ADDBUTTONS = (WM_USER + 20)
        TB_INSERTBUTTON = (WM_USER + 21)
        TB_DELETEBUTTON = (WM_USER + 22)
        TB_GETBUTTON = (WM_USER + 23)
        TB_BUTTONCOUNT = (WM_USER + 24)
        TB_COMMANDTOINDEX = (WM_USER + 25)
        TB_SAVERESTOREA = (WM_USER + 26)
        TB_SAVERESTOREW = (WM_USER + 76)
        TB_CUSTOMIZE = (WM_USER + 27)
        TB_ADDSTRINGA = (WM_USER + 28)
        TB_ADDSTRINGW = (WM_USER + 77)
        TB_GETITEMRECT = (WM_USER + 29)
        TB_BUTTONSTRUCTSIZE = (WM_USER + 30)
        TB_SETBUTTONSIZE = (WM_USER + 31)
        TB_SETBITMAPSIZE = (WM_USER + 32)
        TB_AUTOSIZE = (WM_USER + 33)
        TB_GETTOOLTIPS = (WM_USER + 35)
        TB_SETTOOLTIPS = (WM_USER + 36)
        TB_SETPARENT = (WM_USER + 37)
        TB_SETROWS = (WM_USER + 39)
        TB_GETROWS = (WM_USER + 40)
        TB_SETCMDID = (WM_USER + 42)
        TB_CHANGEBITMAP = (WM_USER + 43)
        TB_GETBITMAP = (WM_USER + 44)
        TB_GETBUTTONTEXTA = (WM_USER + 45)
        TB_GETBUTTONTEXTW = (WM_USER + 75)
        TB_REPLACEBITMAP = (WM_USER + 46)
        TB_SETINDENT = (WM_USER + 47)
        TB_SETIMAGELIST = (WM_USER + 48)
        TB_GETIMAGELIST = (WM_USER + 49)
        TB_LOADIMAGES = (WM_USER + 50)
        TB_GETRECT = (WM_USER + 51)
        TB_SETHOTIMAGELIST = (WM_USER + 52)
        TB_GETHOTIMAGELIST = (WM_USER + 53)
        TB_SETDISABLEDIMAGELIST = (WM_USER + 54)
        TB_GETDISABLEDIMAGELIST = (WM_USER + 55)
        TB_SETSTYLE = (WM_USER + 56)
        TB_GETSTYLE = (WM_USER + 57)
        TB_GETBUTTONSIZE = (WM_USER + 58)
        TB_SETBUTTONWIDTH = (WM_USER + 59)
        TB_SETMAXTEXTROWS = (WM_USER + 60)
        TB_GETTEXTROWS = (WM_USER + 61)
        TB_GETOBJECT = (WM_USER + 62)
        TB_GETHOTITEM = (WM_USER + 71)
        TB_SETHOTITEM = (WM_USER + 72)
        TB_SETANCHORHIGHLIGHT = (WM_USER + 73)
        TB_GETANCHORHIGHLIGHT = (WM_USER + 74)
        TB_MAPACCELERATORA = (WM_USER + 78)
        TB_GETINSERTMARK = (WM_USER + 79)
        TB_SETINSERTMARK = (WM_USER + 80)
        TB_INSERTMARKHITTEST = (WM_USER + 81)
        TB_MOVEBUTTON = (WM_USER + 82)
        TB_GETMAXSIZE = (WM_USER + 83)
        TB_SETEXTENDEDSTYLE = (WM_USER + 84)
        TB_GETEXTENDEDSTYLE = (WM_USER + 85)
        TB_GETPADDING = (WM_USER + 86)
        TB_SETPADDING = (WM_USER + 87)
        TB_SETINSERTMARKCOLOR = (WM_USER + 88)
        TB_GETINSERTMARKCOLOR = (WM_USER + 89)
        TB_SETCOLORSCHEME = CCM_SETCOLORSCHEME
        TB_GETCOLORSCHEME = CCM_GETCOLORSCHEME
        TB_SETUNICODEFORMAT = CCM_SETUNICODEFORMAT
        TB_GETUNICODEFORMAT = CCM_GETUNICODEFORMAT
        TB_MAPACCELERATORW = (WM_USER + 90)
        TB_GETBITMAPFLAGS = (WM_USER + 41)
        TB_GETBUTTONINFOW = (WM_USER + 63)
        TB_SETBUTTONINFOW = (WM_USER + 64)
        TB_GETBUTTONINFOA = (WM_USER + 65)
        TB_SETBUTTONINFOA = (WM_USER + 66)
        TB_INSERTBUTTONW = (WM_USER + 67)
        TB_ADDBUTTONSW = (WM_USER + 68)
        TB_HITTEST = (WM_USER + 69)
        TB_SETDRAWTEXTFLAGS = (WM_USER + 70)
        TB_GETSTRINGW = (WM_USER + 91)
        TB_GETSTRINGA = (WM_USER + 92)
        TB_GETMETRICS = (WM_USER + 101)
        TB_SETMETRICS = (WM_USER + 102)
        TB_SETWINDOWTHEME = CCM_SETWINDOWTHEME
        RB_INSERTBANDA = (WM_USER + 1)
        RB_DELETEBAND = (WM_USER + 2)
        RB_GETBARINFO = (WM_USER + 3)
        RB_SETBARINFO = (WM_USER + 4)
        RB_GETBANDINFO = (WM_USER + 5)
        RB_SETBANDINFOA = (WM_USER + 6)
        RB_SETPARENT = (WM_USER + 7)
        RB_HITTEST = (WM_USER + 8)
        RB_GETRECT = (WM_USER + 9)
        RB_INSERTBANDW = (WM_USER + 10)
        RB_SETBANDINFOW = (WM_USER + 11)
        RB_GETBANDCOUNT = (WM_USER + 12)
        RB_GETROWCOUNT = (WM_USER + 13)
        RB_GETROWHEIGHT = (WM_USER + 14)
        RB_IDTOINDEX = (WM_USER + 16)
        RB_GETTOOLTIPS = (WM_USER + 17)
        RB_SETTOOLTIPS = (WM_USER + 18)
        RB_SETBKCOLOR = (WM_USER + 19)
        RB_GETBKCOLOR = (WM_USER + 20)
        RB_SETTEXTCOLOR = (WM_USER + 21)
        RB_GETTEXTCOLOR = (WM_USER + 22)
        RB_SIZETORECT = (WM_USER + 23)
        RB_SETCOLORSCHEME = CCM_SETCOLORSCHEME
        RB_GETCOLORSCHEME = CCM_GETCOLORSCHEME
        RB_BEGINDRAG = (WM_USER + 24)
        RB_ENDDRAG = (WM_USER + 25)
        RB_DRAGMOVE = (WM_USER + 26)
        RB_GETBARHEIGHT = (WM_USER + 27)
        RB_GETBANDINFOW = (WM_USER + 28)
        RB_GETBANDINFOA = (WM_USER + 29)
        RB_MINIMIZEBAND = (WM_USER + 30)
        RB_MAXIMIZEBAND = (WM_USER + 31)
        RB_GETDROPTARGET = (CCM_GETDROPTARGET)
        RB_GETBANDBORDERS = (WM_USER + 34)
        RB_SHOWBAND = (WM_USER + 35)
        RB_SETPALETTE = (WM_USER + 37)
        RB_GETPALETTE = (WM_USER + 38)
        RB_MOVEBAND = (WM_USER + 39)
        RB_SETUNICODEFORMAT = CCM_SETUNICODEFORMAT
        RB_GETUNICODEFORMAT = CCM_GETUNICODEFORMAT
        RB_GETBANDMARGINS = (WM_USER + 40)
        RB_SETWINDOWTHEME = CCM_SETWINDOWTHEME
        RB_PUSHCHEVRON = (WM_USER + 43)
        TTM_ACTIVATE = (WM_USER + 1)
        TTM_SETDELAYTIME = (WM_USER + 3)
        TTM_ADDTOOLA = (WM_USER + 4)
        TTM_ADDTOOLW = (WM_USER + 50)
        TTM_DELTOOLA = (WM_USER + 5)
        TTM_DELTOOLW = (WM_USER + 51)
        TTM_NEWTOOLRECTA = (WM_USER + 6)
        TTM_NEWTOOLRECTW = (WM_USER + 52)
        TTM_RELAYEVENT = (WM_USER + 7)
        TTM_GETTOOLINFOA = (WM_USER + 8)
        TTM_GETTOOLINFOW = (WM_USER + 53)
        TTM_SETTOOLINFOA = (WM_USER + 9)
        TTM_SETTOOLINFOW = (WM_USER + 54)
        TTM_HITTESTA = (WM_USER + 10)
        TTM_HITTESTW = (WM_USER + 55)
        TTM_GETTEXTA = (WM_USER + 11)
        TTM_GETTEXTW = (WM_USER + 56)
        TTM_UPDATETIPTEXTA = (WM_USER + 12)
        TTM_UPDATETIPTEXTW = (WM_USER + 57)
        TTM_GETTOOLCOUNT = (WM_USER + 13)
        TTM_ENUMTOOLSA = (WM_USER + 14)
        TTM_ENUMTOOLSW = (WM_USER + 58)
        TTM_GETCURRENTTOOLA = (WM_USER + 15)
        TTM_GETCURRENTTOOLW = (WM_USER + 59)
        TTM_WINDOWFROMPOINT = (WM_USER + 16)
        TTM_TRACKACTIVATE = (WM_USER + 17)
        TTM_TRACKPOSITION = (WM_USER + 18)
        TTM_SETTIPBKCOLOR = (WM_USER + 19)
        TTM_SETTIPTEXTCOLOR = (WM_USER + 20)
        TTM_GETDELAYTIME = (WM_USER + 21)
        TTM_GETTIPBKCOLOR = (WM_USER + 22)
        TTM_GETTIPTEXTCOLOR = (WM_USER + 23)
        TTM_SETMAXTIPWIDTH = (WM_USER + 24)
        TTM_GETMAXTIPWIDTH = (WM_USER + 25)
        TTM_SETMARGIN = (WM_USER + 26)
        TTM_GETMARGIN = (WM_USER + 27)
        TTM_POP = (WM_USER + 28)
        TTM_UPDATE = (WM_USER + 29)
        TTM_GETBUBBLESIZE = (WM_USER + 30)
        TTM_ADJUSTRECT = (WM_USER + 31)
        TTM_SETTITLEA = (WM_USER + 32)
        TTM_SETTITLEW = (WM_USER + 33)
        TTM_POPUP = (WM_USER + 34)
        TTM_GETTITLE = (WM_USER + 35)
        TTM_SETWINDOWTHEME = CCM_SETWINDOWTHEME
        SB_SETTEXTA = (WM_USER + 1)
        SB_SETTEXTW = (WM_USER + 11)
        SB_GETTEXTA = (WM_USER + 2)
        SB_GETTEXTW = (WM_USER + 13)
        SB_GETTEXTLENGTHA = (WM_USER + 3)
        SB_GETTEXTLENGTHW = (WM_USER + 12)
        SB_SETPARTS = (WM_USER + 4)
        SB_GETPARTS = (WM_USER + 6)
        SB_GETBORDERS = (WM_USER + 7)
        SB_SETMINHEIGHT = (WM_USER + 8)
        SB_SIMPLE = (WM_USER + 9)
        SB_GETRECT = (WM_USER + 10)
        SB_ISSIMPLE = (WM_USER + 14)
        SB_SETICON = (WM_USER + 15)
        SB_SETTIPTEXTA = (WM_USER + 16)
        SB_SETTIPTEXTW = (WM_USER + 17)
        SB_GETTIPTEXTA = (WM_USER + 18)
        SB_GETTIPTEXTW = (WM_USER + 19)
        SB_GETICON = (WM_USER + 20)
        SB_SETUNICODEFORMAT = CCM_SETUNICODEFORMAT
        SB_GETUNICODEFORMAT = CCM_GETUNICODEFORMAT
        SB_SETBKCOLOR = CCM_SETBKCOLOR
        SB_SIMPLEID = &HFF
        TBM_GETPOS = (WM_USER)
        TBM_GETRANGEMIN = (WM_USER + 1)
        TBM_GETRANGEMAX = (WM_USER + 2)
        TBM_GETTIC = (WM_USER + 3)
        TBM_SETTIC = (WM_USER + 4)
        TBM_SETPOS = (WM_USER + 5)
        TBM_SETRANGE = (WM_USER + 6)
        TBM_SETRANGEMIN = (WM_USER + 7)
        TBM_SETRANGEMAX = (WM_USER + 8)
        TBM_CLEARTICS = (WM_USER + 9)
        TBM_SETSEL = (WM_USER + 10)
        TBM_SETSELSTART = (WM_USER + 11)
        TBM_SETSELEND = (WM_USER + 12)
        TBM_GETPTICS = (WM_USER + 14)
        TBM_GETTICPOS = (WM_USER + 15)
        TBM_GETNUMTICS = (WM_USER + 16)
        TBM_GETSELSTART = (WM_USER + 17)
        TBM_GETSELEND = (WM_USER + 18)
        TBM_CLEARSEL = (WM_USER + 19)
        TBM_SETTICFREQ = (WM_USER + 20)
        TBM_SETPAGESIZE = (WM_USER + 21)
        TBM_GETPAGESIZE = (WM_USER + 22)
        TBM_SETLINESIZE = (WM_USER + 23)
        TBM_GETLINESIZE = (WM_USER + 24)
        TBM_GETTHUMBRECT = (WM_USER + 25)
        TBM_GETCHANNELRECT = (WM_USER + 26)
        TBM_SETTHUMBLENGTH = (WM_USER + 27)
        TBM_GETTHUMBLENGTH = (WM_USER + 28)
        TBM_SETTOOLTIPS = (WM_USER + 29)
        TBM_GETTOOLTIPS = (WM_USER + 30)
        TBM_SETTIPSIDE = (WM_USER + 31)
        TBM_SETBUDDY = (WM_USER + 32)
        TBM_GETBUDDY = (WM_USER + 33)
        TBM_SETUNICODEFORMAT = CCM_SETUNICODEFORMAT
        TBM_GETUNICODEFORMAT = CCM_GETUNICODEFORMAT
        DL_BEGINDRAG = (WM_USER + 133)
        DL_DRAGGING = (WM_USER + 134)
        DL_DROPPED = (WM_USER + 135)
        DL_CANCELDRAG = (WM_USER + 136)
        UDM_SETRANGE = (WM_USER + 101)
        UDM_GETRANGE = (WM_USER + 102)
        UDM_SETPOS = (WM_USER + 103)
        UDM_GETPOS = (WM_USER + 104)
        UDM_SETBUDDY = (WM_USER + 105)
        UDM_GETBUDDY = (WM_USER + 106)
        UDM_SETACCEL = (WM_USER + 107)
        UDM_GETACCEL = (WM_USER + 108)
        UDM_SETBASE = (WM_USER + 109)
        UDM_GETBASE = (WM_USER + 110)
        UDM_SETRANGE32 = (WM_USER + 111)
        UDM_GETRANGE32 = (WM_USER + 112)
        UDM_SETUNICODEFORMAT = CCM_SETUNICODEFORMAT
        UDM_GETUNICODEFORMAT = CCM_GETUNICODEFORMAT
        UDM_SETPOS32 = (WM_USER + 113)
        UDM_GETPOS32 = (WM_USER + 114)
        PBM_SETRANGE = (WM_USER + 1)
        PBM_SETPOS = (WM_USER + 2)
        PBM_DELTAPOS = (WM_USER + 3)
        PBM_SETSTEP = (WM_USER + 4)
        PBM_STEPIT = (WM_USER + 5)
        PBM_SETRANGE32 = (WM_USER + 6)
        PBM_GETRANGE = (WM_USER + 7)
        PBM_GETPOS = (WM_USER + 8)
        PBM_SETBARCOLOR = (WM_USER + 9)
        PBM_SETBKCOLOR = CCM_SETBKCOLOR
        HKM_SETHOTKEY = (WM_USER + 1)
        HKM_GETHOTKEY = (WM_USER + 2)
        HKM_SETRULES = (WM_USER + 3)
        LVM_SETUNICODEFORMAT = CCM_SETUNICODEFORMAT
        LVM_GETUNICODEFORMAT = CCM_GETUNICODEFORMAT
        LVM_GETBKCOLOR = (LVM_FIRST + 0)
        LVM_SETBKCOLOR = (LVM_FIRST + 1)
        LVM_GETIMAGELIST = (LVM_FIRST + 2)
        LVM_SETIMAGELIST = (LVM_FIRST + 3)
        LVM_GETITEMCOUNT = (LVM_FIRST + 4)
        LVM_GETITEMA = (LVM_FIRST + 5)
        LVM_GETITEMW = (LVM_FIRST + 75)
        LVM_SETITEMA = (LVM_FIRST + 6)
        LVM_SETITEMW = (LVM_FIRST + 76)
        LVM_INSERTITEMA = (LVM_FIRST + 7)
        LVM_INSERTITEMW = (LVM_FIRST + 77)
        LVM_DELETEITEM = (LVM_FIRST + 8)
        LVM_DELETEALLITEMS = (LVM_FIRST + 9)
        LVM_GETCALLBACKMASK = (LVM_FIRST + 10)
        LVM_SETCALLBACKMASK = (LVM_FIRST + 11)
        LVM_GETNEXTITEM = (LVM_FIRST + 12)
        LVM_FINDITEM = (LVM_FIRST + 13)
        LVM_GETITEMRECT = (LVM_FIRST + 14)
        LVM_SETITEMPOSITION = (LVM_FIRST + 15)
        LVM_GETITEMPOSITION = (LVM_FIRST + 16)
        LVM_GETSTRINGWIDTHA = (LVM_FIRST + 17)
        LVM_GETSTRINGWIDTHW = (LVM_FIRST + 87)
        LVM_HITTEST = (LVM_FIRST + 18)
        LVM_ENSUREVISIBLE = (LVM_FIRST + 19)
        LVM_SCROLL = (LVM_FIRST + 20)
        LVM_REDRAWITEMS = (LVM_FIRST + 21)
        LVM_ARRANGE = (LVM_FIRST + 22)
        LVM_EDITLABELA = (LVM_FIRST + 23)
        LVM_EDITLABELW = (LVM_FIRST + 118)
        LVM_GETEDITCONTROL = (LVM_FIRST + 24)
        LVM_GETCOLUMNA = (LVM_FIRST + 25)
        LVM_GETCOLUMNW = (LVM_FIRST + 95)
        LVM_SETCOLUMNA = (LVM_FIRST + 26)
        LVM_SETCOLUMNW = (LVM_FIRST + 96)
        LVM_INSERTCOLUMNA = (LVM_FIRST + 27)
        LVM_INSERTCOLUMNW = (LVM_FIRST + 97)
        LVM_DELETECOLUMN = (LVM_FIRST + 28)
        LVM_GETCOLUMNWIDTH = (LVM_FIRST + 29)
        LVM_SETCOLUMNWIDTH = (LVM_FIRST + 30)
        LVM_GETHEADER = (LVM_FIRST + 31)
        LVM_CREATEDRAGIMAGE = (LVM_FIRST + 33)
        LVM_GETVIEWRECT = (LVM_FIRST + 34)
        LVM_GETTEXTCOLOR = (LVM_FIRST + 35)
        LVM_SETTEXTCOLOR = (LVM_FIRST + 36)
        LVM_GETTEXTBKCOLOR = (LVM_FIRST + 37)
        LVM_SETTEXTBKCOLOR = (LVM_FIRST + 38)
        LVM_GETTOPINDEX = (LVM_FIRST + 39)
        LVM_GETCOUNTPERPAGE = (LVM_FIRST + 40)
        LVM_GETORIGIN = (LVM_FIRST + 41)
        LVM_UPDATE = (LVM_FIRST + 42)
        LVM_SETITEMSTATE = (LVM_FIRST + 43)
        LVM_GETITEMSTATE = (LVM_FIRST + 44)
        LVM_GETITEMTEXTA = (LVM_FIRST + 45)
        LVM_GETITEMTEXTW = (LVM_FIRST + 115)
        LVM_SETITEMTEXTA = (LVM_FIRST + 46)
        LVM_SETITEMTEXTW = (LVM_FIRST + 116)
        LVM_SETITEMCOUNT = (LVM_FIRST + 47)
        LVM_SORTITEMS = (LVM_FIRST + 48)
        LVM_SETITEMPOSITION32 = (LVM_FIRST + 49)
        LVM_GETSELECTEDCOUNT = (LVM_FIRST + 50)
        LVM_GETITEMSPACING = (LVM_FIRST + 51)
        LVM_GETISEARCHSTRINGA = (LVM_FIRST + 52)
        LVM_GETISEARCHSTRINGW = (LVM_FIRST + 117)
        LVM_SETICONSPACING = (LVM_FIRST + 53)
        LVM_SETEXTENDEDLISTVIEWSTYLE = (LVM_FIRST + 54)
        LVM_GETEXTENDEDLISTVIEWSTYLE = (LVM_FIRST + 55)
        LVM_GETSUBITEMRECT = (LVM_FIRST + 56)
        LVM_SUBITEMHITTEST = (LVM_FIRST + 57)
        LVM_SETCOLUMNORDERARRAY = (LVM_FIRST + 58)
        LVM_GETCOLUMNORDERARRAY = (LVM_FIRST + 59)
        LVM_SETHOTITEM = (LVM_FIRST + 60)
        LVM_GETHOTITEM = (LVM_FIRST + 61)
        LVM_SETHOTCURSOR = (LVM_FIRST + 62)
        LVM_GETHOTCURSOR = (LVM_FIRST + 63)
        LVM_APPROXIMATEVIEWRECT = (LVM_FIRST + 64)
        LVM_SETWORKAREAS = (LVM_FIRST + 65)
        LVM_GETWORKAREAS = (LVM_FIRST + 70)
        LVM_GETNUMBEROFWORKAREAS = (LVM_FIRST + 73)
        LVM_GETSELECTIONMARK = (LVM_FIRST + 66)
        LVM_SETSELECTIONMARK = (LVM_FIRST + 67)
        LVM_SETHOVERTIME = (LVM_FIRST + 71)
        LVM_GETHOVERTIME = (LVM_FIRST + 72)
        LVM_SETTOOLTIPS = (LVM_FIRST + 74)
        LVM_GETTOOLTIPS = (LVM_FIRST + 78)
        LVM_SORTITEMSEX = (LVM_FIRST + 81)
        LVM_SETBKIMAGEA = (LVM_FIRST + 68)
        LVM_SETBKIMAGEW = (LVM_FIRST + 138)
        LVM_GETBKIMAGEA = (LVM_FIRST + 69)
        LVM_GETBKIMAGEW = (LVM_FIRST + 139)
        LVM_SETSELECTEDCOLUMN = (LVM_FIRST + 140)
        LVM_SETTILEWIDTH = (LVM_FIRST + 141)
        LVM_SETVIEW = (LVM_FIRST + 142)
        LVM_GETVIEW = (LVM_FIRST + 143)
        LVM_INSERTGROUP = (LVM_FIRST + 145)
        LVM_SETGROUPINFO = (LVM_FIRST + 147)
        LVM_GETGROUPINFO = (LVM_FIRST + 149)
        LVM_REMOVEGROUP = (LVM_FIRST + 150)
        LVM_MOVEGROUP = (LVM_FIRST + 151)
        LVM_MOVEITEMTOGROUP = (LVM_FIRST + 154)
        LVM_SETGROUPMETRICS = (LVM_FIRST + 155)
        LVM_GETGROUPMETRICS = (LVM_FIRST + 156)
        LVM_ENABLEGROUPVIEW = (LVM_FIRST + 157)
        LVM_SORTGROUPS = (LVM_FIRST + 158)
        LVM_INSERTGROUPSORTED = (LVM_FIRST + 159)
        LVM_REMOVEALLGROUPS = (LVM_FIRST + 160)
        LVM_HASGROUP = (LVM_FIRST + 161)
        LVM_SETTILEVIEWINFO = (LVM_FIRST + 162)
        LVM_GETTILEVIEWINFO = (LVM_FIRST + 163)
        LVM_SETTILEINFO = (LVM_FIRST + 164)
        LVM_GETTILEINFO = (LVM_FIRST + 165)
        LVM_SETINSERTMARK = (LVM_FIRST + 166)
        LVM_GETINSERTMARK = (LVM_FIRST + 167)
        LVM_INSERTMARKHITTEST = (LVM_FIRST + 168)
        LVM_GETINSERTMARKRECT = (LVM_FIRST + 169)
        LVM_SETINSERTMARKCOLOR = (LVM_FIRST + 170)
        LVM_GETINSERTMARKCOLOR = (LVM_FIRST + 171)
        LVM_SETINFOTIP = (LVM_FIRST + 173)
        LVM_GETSELECTEDCOLUMN = (LVM_FIRST + 174)
        LVM_ISGROUPVIEWENABLED = (LVM_FIRST + 175)
        LVM_GETOUTLINECOLOR = (LVM_FIRST + 176)
        LVM_SETOUTLINECOLOR = (LVM_FIRST + 177)
        LVM_CANCELEDITLABEL = (LVM_FIRST + 179)
        LVM_MAPINDEXTOID = (LVM_FIRST + 180)
        LVM_MAPIDTOINDEX = (LVM_FIRST + 181)
        TVM_INSERTITEMA = (TV_FIRST + 0)
        TVM_INSERTITEMW = (TV_FIRST + 50)
        TVM_DELETEITEM = (TV_FIRST + 1)
        TVM_EXPAND = (TV_FIRST + 2)
        TVM_GETITEMRECT = (TV_FIRST + 4)
        TVM_GETCOUNT = (TV_FIRST + 5)
        TVM_GETINDENT = (TV_FIRST + 6)
        TVM_SETINDENT = (TV_FIRST + 7)
        TVM_GETIMAGELIST = (TV_FIRST + 8)
        TVM_SETIMAGELIST = (TV_FIRST + 9)
        TVM_GETNEXTITEM = (TV_FIRST + 10)
        TVM_SELECTITEM = (TV_FIRST + 11)
        TVM_GETITEM = (TV_FIRST + 12)
        TVM_GETITEMW = (TV_FIRST + 62)
        TVM_SETITEMA = (TV_FIRST + 13)
        TVM_SETITEMW = (TV_FIRST + 63)
        TVM_EDITLABELA = (TV_FIRST + 14)
        TVM_EDITLABELW = (TV_FIRST + 65)
        TVM_GETEDITCONTROL = (TV_FIRST + 15)
        TVM_GETVISIBLECOUNT = (TV_FIRST + 16)
        TVM_HITTEST = (TV_FIRST + 17)
        TVM_CREATEDRAGIMAGE = (TV_FIRST + 18)
        TVM_SORTCHILDREN = (TV_FIRST + 19)
        TVM_ENSUREVISIBLE = (TV_FIRST + 20)
        TVM_SORTCHILDRENCB = (TV_FIRST + 21)
        TVM_ENDEDITLABELNOW = (TV_FIRST + 22)
        TVM_GETISEARCHSTRINGA = (TV_FIRST + 23)
        TVM_GETISEARCHSTRINGW = (TV_FIRST + 64)
        TVM_SETTOOLTIPS = (TV_FIRST + 24)
        TVM_GETTOOLTIPS = (TV_FIRST + 25)
        TVM_SETINSERTMARK = (TV_FIRST + 26)
        TVM_SETUNICODEFORMAT = CCM_SETUNICODEFORMAT
        TVM_GETUNICODEFORMAT = CCM_GETUNICODEFORMAT
        TVM_SETITEMHEIGHT = (TV_FIRST + 27)
        TVM_GETITEMHEIGHT = (TV_FIRST + 28)
        TVM_SETBKCOLOR = (TV_FIRST + 29)
        TVM_SETTEXTCOLOR = (TV_FIRST + 30)
        TVM_GETBKCOLOR = (TV_FIRST + 31)
        TVM_GETTEXTCOLOR = (TV_FIRST + 32)
        TVM_SETSCROLLTIME = (TV_FIRST + 33)
        TVM_GETSCROLLTIME = (TV_FIRST + 34)
        TVM_SETINSERTMARKCOLOR = (TV_FIRST + 37)
        TVM_GETINSERTMARKCOLOR = (TV_FIRST + 38)
        TVM_GETITEMSTATE = (TV_FIRST + 39)
        TVM_SETLINECOLOR = (TV_FIRST + 40)
        TVM_GETLINECOLOR = (TV_FIRST + 41)
        TVM_MAPACCIDTOHTREEITEM = (TV_FIRST + 42)
        TVM_MAPHTREEITEMTOACCID = (TV_FIRST + 43)
        CBEM_INSERTITEMA = (WM_USER + 1)
        CBEM_SETIMAGELIST = (WM_USER + 2)
        CBEM_GETIMAGELIST = (WM_USER + 3)
        CBEM_GETITEMA = (WM_USER + 4)
        CBEM_SETITEMA = (WM_USER + 5)
        CBEM_DELETEITEM = CB_DELETESTRING
        CBEM_GETCOMBOCONTROL = (WM_USER + 6)
        CBEM_GETEDITCONTROL = (WM_USER + 7)
        CBEM_SETEXTENDEDSTYLE = (WM_USER + 14)
        CBEM_GETEXTENDEDSTYLE = (WM_USER + 9)
        CBEM_SETUNICODEFORMAT = CCM_SETUNICODEFORMAT
        CBEM_GETUNICODEFORMAT = CCM_GETUNICODEFORMAT
        CBEM_SETEXSTYLE = (WM_USER + 8)
        CBEM_GETEXSTYLE = (WM_USER + 9)
        CBEM_HASEDITCHANGED = (WM_USER + 10)
        CBEM_INSERTITEMW = (WM_USER + 11)
        CBEM_SETITEMW = (WM_USER + 12)
        CBEM_GETITEMW = (WM_USER + 13)
        TCM_GETIMAGELIST = (TCM_FIRST + 2)
        TCM_SETIMAGELIST = (TCM_FIRST + 3)
        TCM_GETITEMCOUNT = (TCM_FIRST + 4)
        TCM_GETITEMA = (TCM_FIRST + 5)
        TCM_GETITEMW = (TCM_FIRST + 60)
        TCM_SETITEMA = (TCM_FIRST + 6)
        TCM_SETITEMW = (TCM_FIRST + 61)
        TCM_INSERTITEMA = (TCM_FIRST + 7)
        TCM_INSERTITEMW = (TCM_FIRST + 62)
        TCM_DELETEITEM = (TCM_FIRST + 8)
        TCM_DELETEALLITEMS = (TCM_FIRST + 9)
        TCM_GETITEMRECT = (TCM_FIRST + 10)
        TCM_GETCURSEL = (TCM_FIRST + 11)
        TCM_SETCURSEL = (TCM_FIRST + 12)
        TCM_HITTEST = (TCM_FIRST + 13)
        TCM_SETITEMEXTRA = (TCM_FIRST + 14)
        TCM_ADJUSTRECT = (TCM_FIRST + 40)
        TCM_SETITEMSIZE = (TCM_FIRST + 41)
        TCM_REMOVEIMAGE = (TCM_FIRST + 42)
        TCM_SETPADDING = (TCM_FIRST + 43)
        TCM_GETROWCOUNT = (TCM_FIRST + 44)
        TCM_GETTOOLTIPS = (TCM_FIRST + 45)
        TCM_SETTOOLTIPS = (TCM_FIRST + 46)
        TCM_GETCURFOCUS = (TCM_FIRST + 47)
        TCM_SETCURFOCUS = (TCM_FIRST + 48)
        TCM_SETMINTABWIDTH = (TCM_FIRST + 49)
        TCM_DESELECTALL = (TCM_FIRST + 50)
        TCM_HIGHLIGHTITEM = (TCM_FIRST + 51)
        TCM_SETEXTENDEDSTYLE = (TCM_FIRST + 52)
        TCM_GETEXTENDEDSTYLE = (TCM_FIRST + 53)
        TCM_SETUNICODEFORMAT = CCM_SETUNICODEFORMAT
        TCM_GETUNICODEFORMAT = CCM_GETUNICODEFORMAT
        ACM_OPENA = (WM_USER + 100)
        ACM_OPENW = (WM_USER + 103)
        ACM_PLAY = (WM_USER + 101)
        ACM_STOP = (WM_USER + 102)
        MCM_FIRST = &H1000
        MCM_GETCURSEL = (MCM_FIRST + 1)
        MCM_SETCURSEL = (MCM_FIRST + 2)
        MCM_GETMAXSELCOUNT = (MCM_FIRST + 3)
        MCM_SETMAXSELCOUNT = (MCM_FIRST + 4)
        MCM_GETSELRANGE = (MCM_FIRST + 5)
        MCM_SETSELRANGE = (MCM_FIRST + 6)
        MCM_GETMONTHRANGE = (MCM_FIRST + 7)
        MCM_SETDAYSTATE = (MCM_FIRST + 8)
        MCM_GETMINREQRECT = (MCM_FIRST + 9)
        MCM_SETCOLOR = (MCM_FIRST + 10)
        MCM_GETCOLOR = (MCM_FIRST + 11)
        MCM_SETTODAY = (MCM_FIRST + 12)
        MCM_GETTODAY = (MCM_FIRST + 13)
        MCM_HITTEST = (MCM_FIRST + 14)
        MCM_SETFIRSTDAYOFWEEK = (MCM_FIRST + 15)
        MCM_GETFIRSTDAYOFWEEK = (MCM_FIRST + 16)
        MCM_GETRANGE = (MCM_FIRST + 17)
        MCM_SETRANGE = (MCM_FIRST + 18)
        MCM_GETMONTHDELTA = (MCM_FIRST + 19)
        MCM_SETMONTHDELTA = (MCM_FIRST + 20)
        MCM_GETMAXTODAYWIDTH = (MCM_FIRST + 21)
        MCM_SETUNICODEFORMAT = CCM_SETUNICODEFORMAT
        MCM_GETUNICODEFORMAT = CCM_GETUNICODEFORMAT
        DTM_FIRST = &H1000
        DTM_GETSYSTEMTIME = (DTM_FIRST + 1)
        DTM_SETSYSTEMTIME = (DTM_FIRST + 2)
        DTM_GETRANGE = (DTM_FIRST + 3)
        DTM_SETRANGE = (DTM_FIRST + 4)
        DTM_SETFORMATA = (DTM_FIRST + 5)
        DTM_SETFORMATW = (DTM_FIRST + 50)
        DTM_SETMCCOLOR = (DTM_FIRST + 6)
        DTM_GETMCCOLOR = (DTM_FIRST + 7)
        DTM_GETMONTHCAL = (DTM_FIRST + 8)
        DTM_SETMCFONT = (DTM_FIRST + 9)
        DTM_GETMCFONT = (DTM_FIRST + 10)
        PGM_SETCHILD = (PGM_FIRST + 1)
        PGM_RECALCSIZE = (PGM_FIRST + 2)
        PGM_FORWARDMOUSE = (PGM_FIRST + 3)
        PGM_SETBKCOLOR = (PGM_FIRST + 4)
        PGM_GETBKCOLOR = (PGM_FIRST + 5)
        PGM_SETBORDER = (PGM_FIRST + 6)
        PGM_GETBORDER = (PGM_FIRST + 7)
        PGM_SETPOS = (PGM_FIRST + 8)
        PGM_GETPOS = (PGM_FIRST + 9)
        PGM_SETBUTTONSIZE = (PGM_FIRST + 10)
        PGM_GETBUTTONSIZE = (PGM_FIRST + 11)
        PGM_GETBUTTONSTATE = (PGM_FIRST + 12)
        PGM_GETDROPTARGET = CCM_GETDROPTARGET
        BCM_GETIDEALSIZE = (BCM_FIRST + &H1)
        BCM_SETIMAGELIST = (BCM_FIRST + &H2)
        BCM_GETIMAGELIST = (BCM_FIRST + &H3)
        BCM_SETTEXTMARGIN = (BCM_FIRST + &H4)
        BCM_GETTEXTMARGIN = (BCM_FIRST + &H5)
        EM_SETCUEBANNER = (ECM_FIRST + 1)
        EM_GETCUEBANNER = (ECM_FIRST + 2)
        EM_SHOWBALLOONTIP = (ECM_FIRST + 3)
        EM_HIDEBALLOONTIP = (ECM_FIRST + 4)
        CB_SETMINVISIBLE = (CBM_FIRST + 1)
        CB_GETMINVISIBLE = (CBM_FIRST + 2)
        LM_HITTEST = (WM_USER + &H300)
        LM_GETIDEALHEIGHT = (WM_USER + &H301)
        LM_SETITEM = (WM_USER + &H302)
        LM_GETITEM = (WM_USER + &H303)
    End Enum

    ''' <summary>
    ''' WM_PRINT flags from winuser.h
    ''' </summary>
    Public Enum PrintFlags
        PRF_CHECKVISIBLE = &H1
        PRF_NONCLIENT = &H2
        PRF_CLIENT = &H4
        PRF_ERASEBKGND = &H8
        PRF_CHILDREN = &H10
        PRF_OWNED = &H20
    End Enum

    ''' <summary>
    ''' Edit Control Notification Codes
    ''' </summary>
    Public Enum EditControlNotifCode
        EN_SETFOCUS = &H100
        EN_KILLFOCUS = &H200
        EN_CHANGE = &H300
        EN_UPDATE = &H400
        EN_ERRSPACE = &H500
        EN_MAXTEXT = &H501
        EN_HSCROLL = &H601
        EN_VSCROLL = &H602
    End Enum


    <StructLayout(LayoutKind.Sequential)>
    Public Structure TBBUTTONINFO
        Public cbSize As Int32
        Public dwMask As TBIFlags
        Public idCommand As Int32
        Public iImage As Int32
        Public fsState As Byte
        Public fsStyle As Byte
        Public cx As Short
        Public lParam As IntPtr
        Public pszText As IntPtr
        Public cchText As Int32
    End Structure

    ''' <summary>
    ''' Determines whether a value is a pointer to a resource, or
    ''' the index of an item in a collection. See win32 docs for further
    ''' details
    ''' </summary>
    ''' <param name="Value">The value to be tested.</param>
    ''' <returns>Returns true if the value is an integer resource.
    ''' When false, the value is a pointer to something else (usually a 
    ''' string).</returns>
    ''' <remarks></remarks>
    Public Function IS_INTRESOURCE(ByVal Value As Integer) As Boolean
        Return (Value >> 16) = 0
    End Function

    ''' <summary>
    ''' Creates a "long" (32 bit) integer from two short (16) bit integers.
    ''' </summary>
    ''' <param name="LoWord">The value of the least significant word</param>
    ''' <param name="HiWord">The value of the most significant word</param>
    ''' <returns>Returns the two values, joined into one 32 bit integer, as
    ''' per the corresponding win32 api function.</returns>
    Public Function MakeLong(ByVal LoWord As Short, ByVal HiWord As Short) As Int32
        Return (HiWord << 16) Or LoWord
    End Function


    <StructLayout(LayoutKind.Sequential)>
    Public Structure TBBUTTON
        Dim iBitmap As Integer
        Dim idCommand As Integer
        Dim fsState As Byte
        Dim fsStyle As Byte
        Dim bReserved1 As Byte
        Dim bReserved2 As Byte
        Dim dwData As Integer
        Dim iString As Integer
    End Structure


    Public Enum TBIFlags
        TBIF_BYINDEX = &H80000000
        TBIF_COMMAND = 32
        TBIF_IMAGE = 1
        TBIF_LPARAM = 16
        TBIF_SIZE = 64
        TBIF_STATE = 4
        TBIF_STYLE = 8
        TBIF_TEXT = 2
    End Enum

    Public Enum TBSTYLE
        TBSTYLE_BUTTON = &H0
        TBSTYLE_SEP = &H1
        TBSTYLE_CHECK = &H2
        TBSTYLE_GROUP = &H4
        TBSTYLE_CHECKGROUP = (TBSTYLE_GROUP Or TBSTYLE_CHECK)
        TBSTYLE_DROPDOWN = &H8
        TBSTYLE_AUTOSIZE = &H10
        TBSTYLE_NOPREFIX = &H20
        TBSTYLE_TOOLTIPS = &H100
        TBSTYLE_WRAPABLE = &H200
        TBSTYLE_ALTDRAG = &H400
        TBSTYLE_FLAT = &H800
        TBSTYLE_LIST = &H1000
        TBSTYLE_CUSTOMERASE = &H2000
        TBSTYLE_REGISTERDROP = &H4000
        TBSTYLE_TRANSPARENT = &H8000
        TBSTYLE_EX_DRAWDDARROWS = &H1
    End Enum


    Public Enum BTNS
        BTNS_BUTTON = TBSTYLE.TBSTYLE_BUTTON
        BTNS_SEP = TBSTYLE.TBSTYLE_SEP
        BTNS_CHECK = TBSTYLE.TBSTYLE_CHECK
        BTNS_GROUP = TBSTYLE.TBSTYLE_GROUP
        BTNS_CHECKGROUP = TBSTYLE.TBSTYLE_CHECKGROUP
        BTNS_DROPDOWN = TBSTYLE.TBSTYLE_DROPDOWN
        BTNS_AUTOSIZE = TBSTYLE.TBSTYLE_AUTOSIZE
        BTNS_NOPREFIX = TBSTYLE.TBSTYLE_NOPREFIX
        BTNS_WHOLEDROPDOWN = &H80
        BTNS_SHOWTEXT = &H40
    End Enum

    Public Enum TBSTATE
        TBSTATE_CHECKED = &H1
        TBSTATE_PRESSED = &H2
        TBSTATE_ENABLED = &H4
        TBSTATE_WRAP = &H20
        TBSTATE_ELLIPSES = &H40
        TBSTATE_INDETERMINATE = &H10
        TBSTATE_HIDDEN = &H8
        TBSTATE_MARKED = &H80
    End Enum



    Private Const H_MAX As Integer = &HFFFF + 1
    Public Enum NotificationMessages
        NM_FIRST = H_MAX                        '(0U-  0U)       // generic to all controls
        NM_LAST = H_MAX - &H99  '               (0U- 99U)
        NM_OUTOFMEMORY = (NM_FIRST - 1)
        NM_CLICK = (NM_FIRST - 2)
        NM_DBLCLK = (NM_FIRST - 3)
        NM_RETURN = (NM_FIRST - 4)
        NM_RCLICK = (NM_FIRST - 5)
        NM_RDBLCLK = (NM_FIRST - 6)
        NM_SETFOCUS = (NM_FIRST - 7)
        NM_KILLFOCUS = (NM_FIRST - 8)
    End Enum

    ''' <summary>
    ''' Gets the id of a windows control.
    ''' </summary>
    ''' <param name="hwndCtl">The handle of the control whose
    ''' ID is sought.</param>
    ''' <returns></returns>
    ''' <remarks>The ID is created by the owning application
    ''' when the window is created, and should not be confused with
    ''' the window handle.</remarks>
    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetDlgCtrlID(ByVal hwndCtl As IntPtr) As Integer
    End Function


    ''' <summary>
    ''' Structure used in combination with WM_NOTIFY messages
    ''' </summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure NMHDR
        ''' <summary>
        ''' A window handle to the control sending the message.
        ''' </summary>
        Public hwndFrom As IntPtr
        ''' <summary>
        ''' An identifier of the control sending the message
        ''' </summary>
        Public idFrom As Int32
        ''' <summary>
        ''' A notification code. This member can be one of the common
        ''' notification codes, or it can be a control-specific notification code.
        ''' </summary>
        Public code As Int32
        Public Sub New(ByVal code As Int32, ByVal idFrom As Int32, ByVal hwndFrom As IntPtr)
            Me.code = code
            Me.idFrom = idFrom
            Me.hwndFrom = hwndFrom
        End Sub
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure NMDATETIMECHANGE
        Public hdr As NMHDR
        Public flags As Int32
        Public systime As SYSTEMTIME
        Public Sub New(ByVal code As Int32, ByVal idFrom As Int32, ByVal hwndFrom As IntPtr, ByVal flags As Int32, ByVal systime As SYSTEMTIME)
            hdr.code = code
            hdr.idFrom = idFrom
            hdr.hwndFrom = hwndFrom
            Me.flags = flags
            Me.systime = systime
        End Sub
    End Structure

    Public Structure NMTTDISPINFO
        Public hdr As NMHDR
        Public lpszText As IntPtr
        Public szText As IntPtr
        Public hinst As IntPtr
        Public uFlags As UInt32
        Public lParam As Int32
        Public hbmp As IntPtr
    End Structure



    <StructLayout(LayoutKind.Sequential)>
    Public Structure NMMOUSE
        Public hdr As NMHDR
        Public dwItemSpec As Int32
        Public dwItemData As IntPtr
        Public pt As POINTAPI
        Public dwHitInfo As IntPtr
    End Structure

    Public Enum TTN
        TTN_FIRST = 0 - 520
        TTN_GETDISPINFO = (TTN_FIRST - 10)
        TTN_SHOW = (TTN_FIRST - 1)
        TTN_POP = (TTN_FIRST - 2)
    End Enum

    Public Enum TTF
        TTF_IDISHWND = &H1
        TTF_CENTERTIP = &H2
        TTF_RTLREADING = &H4
        TTF_SUBCLASS = &H10
        TTF_TRACK = &H20
        TTF_ABSOLUTE = &H80
        TTF_TRANSPARENT = &H100
        TTF_DI_SETITEM = &H8000
    End Enum

    ''' <summary>
    ''' Contains information about an LVN_ITEMACTIVATE
    ''' notification message.
    ''' </summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure NMITEMACTIVATE
        ''' <summary>
        ''' NMHDR structure that contains information about this notification message.
        ''' </summary>
        Public hdr As NMHDR
        ''' <summary>
        ''' Index of the list-view item. If the item index is not used for
        ''' the notification, this member will contain -1.
        ''' </summary>
        Public iItem As Integer
        ''' <summary>
        ''' One-based index of the subitem. If the subitem index is not used for the notification or
        ''' the notification does not apply to a subitem, this member will contain zero.
        ''' </summary>
        Public iSubItem As Integer
        ''' <summary>
        ''' New item state. This member is zero for notification messages that do not use it.
        ''' </summary>
        Public uNewState As Integer
        ''' <summary>
        ''' Old item state. This member is zero for notification messages that do not use it.
        ''' </summary>
        Public uOldState As Integer
        ''' <summary>
        ''' Set of flags that indicate the item attributes that have
        ''' changed. This member is zero for notifications that do
        ''' not use it. Otherwise, it can have the same values as
        ''' the mask member of the LVITEM structure.
        ''' </summary>
        Public uChanged As Integer
        ''' <summary>
        ''' POINT structure that indicates the location at which the event occurred. This
        ''' member is undefined for notification messages that do not use it.
        ''' </summary>
        Public ptAction As POINTAPI
        ''' <summary>
        ''' Application-defined value of the item. This member is
        ''' undefined for notification messages that do not use it.
        ''' </summary>
        Public lParam As Integer
        ''' <summary>
        ''' Modifier keys that were pressed at the time of the activation.
        ''' This member contains zero or a combination of the flags
        ''' LVKF_ALT, LVKF_CONTROL, LVKF_SHIFT. 
        ''' </summary>
        Public uKeyFlags As Integer
    End Structure

    Public Enum TBS
        TBS_AUTOTICKS = &H1
        TBS_VERT = &H2
        TBS_HORZ = &H0
        TBS_TOP = &H4
        TBS_BOTTOM = &H0
        TBS_LEFT = &H4
        TBS_RIGHT = &H0
        TBS_BOTH = &H8
        TBS_NOTICKS = &H10
        TBS_ENABLESELRANGE = &H20
        TBS_FIXEDLENGTH = &H40
        TBS_NOTHUMB = &H80
        TBS_TOOLTIPS = &H100
    End Enum

    Public Enum TB
        TB_LINEUP = 0
        TB_LINEDOWN = 1
        TB_PAGEUP = 2
        TB_PAGEDOWN = 3
        TB_THUMBPOSITION = 4
        TB_THUMBTRACK = 5
        TB_TOP = 6
        TB_BOTTOM = 7
        TB_ENDTRACK = 8
    End Enum

    Public Enum UDS
        UDS_WRAP = &H1
        UDS_SETBUDDYINT = &H2
        UDS_ALIGNRIGHT = &H4
        UDS_ALIGNLEFT = &H8
        UDS_AUTOBUDDY = &H10
        UDS_ARROWKEYS = &H20
        UDS_HORZ = &H40
        UDS_NOTHOUSANDS = &H80
        UDS_HOTTRACK = &H100
    End Enum

    Public Enum SB
        SB_ENDSCROLL = 8
        SB_LEFT = 6
        SB_RIGHT = 7
        SB_LINELEFT = 0
        SB_LINERIGHT = 1
        SB_PAGELEFT = 2
        SB_PAGERIGHT = 3
        SB_THUMBPOSITION = 4
        SB_THUMBTRACK = 5
        SB_BOTTOM = 7
        SB_LINEDOWN = 1
        SB_LINEUP = 0
        SB_PAGEDOWN = 3
        SB_PAGEUP = 2
        SB_TOP = 6
    End Enum

    Public Enum TreeviewNextItemFlags
        TVGN_ROOT = &H0
        TVGN_NEXT = &H1
        TVGN_PREVIOUS = &H2
        TVGN_PARENT = &H3
        TVGN_CHILD = &H4
        TVGN_FIRSTVISIBLE = &H5
        TVGN_NEXTVISIBLE = &H6
        TVGN_PREVIOUSVISIBLE = &H7
        TVGN_DROPHILITE = &H8
        TVGN_CARET = &H9
#If (WIN32_IE >= &H400) Then
TVGN_LASTVISIBLE = &HA
#End If
    End Enum

    Public Enum TreeviewExpandFlags
        TVE_COLLAPSE = &H1
        TVE_EXPAND = &H2
        TVE_TOGGLE = &H3
#If (Win32_IE >= &H300) Then
          TVE_EXPANDPARTIAL   =   &H4000  
#End If
        TVE_COLLAPSERESET = &H8000
    End Enum

    Public Enum TreeviewGetItemFlags
        TVIF_TEXT = &H1
        TVIF_IMAGE = &H2
        TVIF_PARAM = &H4
        TVIF_STATE = &H8
        TVIF_HANDLE = &H10
        TVIF_SELECTEDIMAGE = &H20
        TVIF_CHILDREN = &H40
#If (WIN32_IE >= &H400) Then
TVIF_INTEGRAL = &H80
#End If
    End Enum

    Public Enum TreeviewSelectItemFlags
        TVGN_FIRSTVISIBLE = &H5
        TVGN_DROPHILITE = &H8
        TVGN_CARET = &H9
    End Enum

    Public Enum ListviewNextItemFlags
        LVNI_ALL = &H0
        LVNI_FOCUSED = &H1
        LVNI_SELECTED = &H2
        LVNI_CUT = &H4
        LVNI_DROPHILITED = &H8
        LVNI_ABOVE = &H100
        LVNI_BELOW = &H200
        LVNI_TOLEFT = &H400
        LVNI_TORIGHT = &H800
    End Enum

    ''' <summary>
    ''' Listview extended style flags
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum LVS_EX
        LVS_EX_BORDERSELECT = &H8000
        LVS_EX_CHECKBOXES = &H4
        LVS_EX_DOUBLEBUFFER = &H10000
        LVS_EX_FLATSB = &H100
        LVS_EX_FULLROWSELECT = &H20
        LVS_EX_GRIDLINES = &H1
        LVS_EX_HEADERDRAGDROP = &H10
        LVS_EX_HIDELABELS = &H20000
        LVS_EX_INFOTIP = &H400
        LVS_EX_LABELTIP = &H4000
        LVS_EX_MULTIWORKAREAS = &H2000
        LVS_EX_ONECLICKACTIVATE = &H40
        LVS_EX_REGIONAL = &H200
        LVS_EX_SINGLEROW = &H40000
        LVS_EX_SNAPTOGRID = &H80000
        LVS_EX_SUBITEMIMAGES = &H2
        LVS_EX_TWOCLICKACTIVATE = &H80
        LVS_EX_TRACKSELECT = &H8
        LVS_EX_UNDERLINEHOT = &H800
        LVS_EX_UNDERLINECOLD = &H1000
    End Enum

    ''' <summary>
    ''' Values used in conjunction with the LVM_GETITEMRECT message
    ''' </summary>
    Public Enum LVIR
        ''' <summary>
        ''' Retrieves the  bounding rectangle of the entire item, including the icon and label.
        ''' </summary>
        LVIR_BOUNDS = &H0
        ''' <summary>
        ''' Retrieves the bounding rectangle of the icon or small icon.
        ''' </summary>
        LVIR_ICON = &H1
        ''' <summary>
        ''' Retrieves the bounding rectangle of the item text.
        ''' </summary>
        LVIR_LABEL = &H2
        ''' <summary>
        ''' Retrieves the union of the LVIR_ICON and LVIR_LABEL rectangles, but excludes columns in report view
        ''' </summary>
        LVIR_SELECTBOUNDS = &H3
    End Enum

    ''' <summary>
    ''' Structure used in comnjunction with listview hit tests.
    ''' </summary>
    ''' <remarks>See also LVM_HITTEST</remarks>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure LVHITTESTINFO
        ''' <summary>
        ''' The position to hit test, in client coordinates.
        ''' </summary>
        Public pt As POINTAPI
        ''' <summary>
        ''' A combination of values indicating the result of the hit test
        ''' </summary>
        Public flags As LVHT
        ''' <summary>
        ''' Receives the index of the matching item. Or if hit-testing
        ''' a subitem, this value represents the subitem's parent item.
        ''' </summary>
        Public iItem As Integer
        ''' <summary>
        ''' Receives the index of the matching subitem. When
        ''' hit-testing an item, this member will be zero.
        ''' </summary>
        Public iSubItem As Integer
        Public Sub New(ByVal XCoord As Integer, ByVal YCoord As Integer)
            pt = New POINTAPI(XCoord, YCoord)
        End Sub
        Public Sub New(ByVal Point As POINTAPI)
            pt = Point
        End Sub
    End Structure

    ''' <summary>
    ''' Flags for the listview tit test.
    ''' </summary>
    ''' <remarks>See also LVHITTESTINFO</remarks>
    Public Enum LVHT
        LVHT_NOWHERE = 1
        LVHT_ONITEMICON = 2
        LVHT_ONITEMLABEL = 4
        LVHT_ONITEMSTATEICON = 8
        LVHT_ONITEM = LVHT_ONITEMICON Or LVHT_ONITEMLABEL Or LVHT_ONITEMSTATEICON
        LVHT_ABOVE = 8
        LVHT_BELOW = 16
        LVHT_TORIGHT = 32
        LVHT_TOLEFT = 64
    End Enum

    Public Enum ButtonCheckStates
        BST_UNCHECKED = 0
        BST_CHECKED = 1
        BST_INDETERMINATE = 2
        BST_PUSHED = 4
        BST_FOCUS = 8
    End Enum

    ''' <summary>
    ''' Button styles
    ''' </summary>
    Public Enum BS
        BS_3STATE = 5
        BS_AUTO3STATE = 6
        BS_AUTOCHECKBOX = 3
        BS_AUTORADIOBUTTON = 9
        BS_BITMAP = 128
        BS_BOTTOM = &H800
        BS_CENTER = &H300
        BS_CHECKBOX = 2
        BS_DEFPUSHBUTTON = 1
        BS_GROUPBOX = 7
        BS_ICON = 64
        BS_LEFT = 256
        BS_LEFTTEXT = 32
        BS_MULTILINE = &H2000
        BS_NOTIFY = &H4000
        BS_OWNERDRAW = &HB
        BS_PUSHBUTTON = 0
        BS_PUSHLIKE = 4096
        BS_RADIOBUTTON = 4
        BS_RIGHT = 512
        BS_RIGHTBUTTON = 32
        BS_TEXT = 0
        BS_TOP = &H400
        BS_USERBUTTON = 8
        BS_VCENTER = &HC00
        BS_FLAT = &H8000
    End Enum

    <StructLayout(LayoutKind.Sequential)>
    Public Structure TCITEM
        Public mask As Int32
        Public dwState As Int32
        Public dwStateMask As Int32
        Public pszText As IntPtr
        Public cchTextMax As Int32
        Public iImage As Int32
        Public lParam As Int32
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure SYSTEMTIME
        Public Year As Int16
        Public Month As Int16
        Public DayOfWeek As Int16
        Public Day As Int16
        Public Hour As Int16
        Public Minute As Int16
        Public Second As Int16
        Public MilliSecond As Int16

        ''' <summary>
        ''' Gets the underlying value as a DateTime.
        ''' </summary>
        Public Function ToDateTime() As DateTime
            Return New DateTime(Year, Month, Day, Hour, Minute, Second, MilliSecond)
        End Function

        Public Shared Function FromDateTime(ByVal D As DateTime) As SYSTEMTIME
            Dim RetVal As New SYSTEMTIME
            RetVal.Day = System.Convert.ToInt16(D.Day)
            RetVal.DayOfWeek = System.Convert.ToInt16(D.DayOfWeek)
            RetVal.Hour = System.Convert.ToInt16(D.Hour)
            RetVal.MilliSecond = System.Convert.ToInt16(D.Millisecond)
            RetVal.Minute = System.Convert.ToInt16(D.Minute)
            RetVal.Month = System.Convert.ToInt16(D.Month)
            RetVal.Second = System.Convert.ToInt16(D.Second)
            RetVal.Year = System.Convert.ToInt16(D.Year)

            Return RetVal
        End Function
    End Structure

    '   <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
    'Public Function FileTimeToSystemTime(<[In]()> ByRef lpFileTime As ComTypes.FILETIME, <Out()> ByRef lpSystemTime As SYSTEMTIME) As Boolean
    'End Function

    '   <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Auto)> _
    '   Public Function SystemTimeToFileTime(<[In]()> ByRef lpSystemTime As SYSTEMTIME, <[Out]()> ByRef lpFileTime As ComTypes.FILETIME) As Boolean
    '   End Function

    'Flags returned by sendmessage in response
    'to messages such as DTM_GetRange and MCM_GetRange
    Public Enum GDTR
        GDTR_MIN = 1
        GDTR_MAX = 2
    End Enum

    Public Const GDT_ERROR As Integer = -1
    Public Const GDT_VALID As Integer = 0
    Public Const GDT_NONE As Integer = 1

    ''' <summary>
    ''' Listbox sendmessage return values.
    ''' </summary>
    Public Enum LB
        LB_OKAY = 0
        LB_ERR = (-1)
        LB_ERRSPACE = (-2)
    End Enum

    ''' <summary>
    ''' Enumeration of listbox states.
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum LBS
        LBS_NOTIFY = &H1&
        LBS_SORT = &H2&
        LBS_NOREDRAW = &H4&
        LBS_MULTIPLESEL = &H8&
        LBS_OWNERDRAWFIXED = &H10&
        LBS_OWNERDRAWVARIABLE = &H20&
        LBS_HASSTRINGS = &H40&
        LBS_USETABSTOPS = &H80&
        LBS_NOINTEGRALHEIGHT = &H100&
        LBS_MULTICOLUMN = &H200&
        LBS_WANTKEYBOARDINPUT = &H400&
        LBS_EXTENDEDSEL = &H800&
        LBS_DISABLENOSCROLL = &H1000&
        LBS_NODATA = &H2000&
        LBS_STANDARD = (LBS_NOTIFY Or LBS_SORT Or WindowStyles.WS_VSCROLL Or WindowStyles.WS_BORDER)
    End Enum

    ''' <summary>
    ''' Combo Box sendmessage return Values
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum CB
        CB_OKAY = 0
        CB_ERR = (-1)
        CB_ERRSPACE = (-2)
    End Enum

    ''' <summary>
    ''' Structure representing a win32 treeview item
    ''' </summary>
    ''' <remarks></remarks>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure TVITEM
        Public mask As Integer
        Public hItem As IntPtr
        Public state As System.Int32
        Public stateMask As Integer
        Public pszText As IntPtr
        Public cchTextMax As Integer
        Public iImage As Integer
        Public iSelectedImage As Integer
        Public cChildren As Integer
        Public lParam As IntPtr
    End Structure

    Public Enum TVITEMStates
        TVIS_FOCUSED = &H1
        TVIS_SELECTED = &H2
        TVIS_CUT = &H4
        TVIS_DROPHILITED = &H8
        TVIS_BOLD = &H10
        TVIS_EXPANDED = &H20
        TVIS_EXPANDEDONCE = &H40
#If (WIN32_IE >= &H300) Then
TVIS_EXPANDPARTIAL = &H80
#End If
        TVIS_OVERLAYMASK = &HF00
        TVIS_STATEIMAGEMASK = &HF000
        TVIS_USERMASK = &HF000
    End Enum

    ''' <summary>
    ''' Convenience class that interprets a TVItem
    ''' structure as a treeview item (node).
    ''' </summary>
    ''' <remarks>The text property is initialised
    ''' by the caller, because its location often
    ''' lies in another process' memory, making it
    ''' more convenient for the caller to retrieve
    ''' it.
    ''' </remarks>
    Public Class TreeviewItem
        Private mInternalItem As TVITEM
        Public Sub New(ByVal Item As TVITEM, ByVal ItemText As String)
            Me.mInternalItem = Item
            msText = ItemText
        End Sub
        Private msText As String
        ''' <summary>
        ''' The text on the treeview item.
        ''' </summary>
        Public ReadOnly Property Text() As String
            Get
                Return msText
            End Get
        End Property
        ''' <summary>
        ''' Indicates whether the treeview item
        ''' is displayed using a bold font.
        ''' </summary>
        Public ReadOnly Property IsBold() As Boolean
            Get
                Return (mInternalItem.state And TVITEMStates.TVIS_BOLD) > 0
            End Get
        End Property
        ''' <summary>
        ''' Indicates whether the item is expanded,
        ''' revealing its children.
        ''' </summary>
        Public ReadOnly Property IsExpanded() As Boolean
            Get
                Return (mInternalItem.state And TVITEMStates.TVIS_EXPANDED) > 0
            End Get
        End Property
        ''' <summary>
        ''' Indicates whether the item is selected.
        ''' </summary>
        Public ReadOnly Property IsSelected() As Boolean
            Get
                Return (mInternalItem.state And TVITEMStates.TVIS_SELECTED) > 0
            End Get
        End Property
    End Class

    Public Enum TVN
        TVN_FIRST = -400
        TVN_LAST = -499

        TVN_SELCHANGING = (TVN_FIRST - 1)
        TVN_SELCHANGED = (TVN_FIRST - 2)
        TVN_GETDISPINFO = (TVN_FIRST - 3)
        TVN_SETDISPINFO = (TVN_FIRST - 4)
        TVN_ITEMEXPANDING = (TVN_FIRST - 5)
        TVN_ITEMEXPANDED = (TVN_FIRST - 6)
        TVN_BEGINDRAG = (TVN_FIRST - 7)
        TVN_BEGINRDRAG = (TVN_FIRST - 8)
        TVN_DELETEITEM = (TVN_FIRST - 9)
        TVN_BEGINLABELEDIT = (TVN_FIRST - 10)
        TVN_ENDLABELEDIT = (TVN_FIRST - 11)
        TVN_KEYDOWN = (TVN_FIRST - 12)
        TVN_GETINFOTIPA = (TVN_FIRST - 13)
        TVN_GETINFOTIPW = (TVN_FIRST - 14)
        TVN_SINGLEEXPAND = (TVN_FIRST - 15)


        TVN_SELCHANGINGW = (TVN_FIRST - 50)
        TVN_SELCHANGEDW = (TVN_FIRST - 51)
        TVN_GETDISPINFOW = (TVN_FIRST - 52)
        TVN_SETDISPINFOW = (TVN_FIRST - 53)
        TVN_ITEMEXPANDINGW = (TVN_FIRST - 54)
        TVN_ITEMEXPANDEDW = (TVN_FIRST - 55)
        TVN_BEGINDRAGW = (TVN_FIRST - 56)
        TVN_BEGINRDRAGW = (TVN_FIRST - 57)
        TVN_DELETEITEMW = (TVN_FIRST - 58)
        TVN_BEGINLABELEDITW = (TVN_FIRST - 59)
        TVN_ENDLABELEDITW = (TVN_FIRST - 60)
    End Enum

    ''' <summary>
    ''' Structure used for treeview state change notifications. Used
    ''' in conjunction with the message WM_NOTIFY and the notification
    ''' TVN_ITEMCHANGED.
    ''' </summary>
    Public Structure NMTVITEMCHANGE
        ''' <summary>
        ''' NMHDR structure that contains information about the notification.
        ''' </summary>
        Public hdr As NMHDR
        ''' <summary>
        ''' Specifies the attribute which has changed.
        ''' The only supported attribute is the value TVIF_STATE.
        ''' The change is the state attribute.
        ''' </summary>
        Public uChanged As Integer
        ''' <summary>
        ''' Handle to the changed tree-view item.
        ''' </summary>
        Public hItem As Int32
        ''' <summary>
        ''' Flag that specifies the new item state.
        ''' </summary>
        Public uStateNew As Integer
        ''' <summary>
        ''' Flag that specifies the item's previous state.
        ''' </summary>
        Public uStateOld As Integer
        ''' <summary>
        ''' Reserved for application specific data. For example,
        ''' a value to associate with the item.
        ''' </summary>
        Public lParam As Integer
    End Structure

    ''' <summary>
    ''' Structure used in notifications relating to up/down controls.
    ''' </summary>
    Public Structure NMUPDOWN
        ''' <summary>
        ''' NMHDR structure that contains additional information
        ''' about the notification message.
        ''' </summary>
        Public hdr As NMHDR
        ''' <summary>
        ''' Signed integer value that represents the up-down
        ''' control's current position.
        ''' </summary>
        Public iPos As Integer
        ''' <summary>
        ''' Signed integer value that represents the proposed
        ''' change in the up-down control's position.
        ''' </summary>
        Public iDelta As Integer

        Public Sub New(ByVal hdr As NMHDR, ByVal ipos As Integer, ByVal idelta As Integer)
            Me.hdr = hdr
            Me.iPos = ipos
            Me.iDelta = idelta
        End Sub
    End Structure

    Public Enum UDN
        UDN_FIRST = (0 - 721)
        UDN_LAST = (0 - 740)
        UDN_DELTAPOS = (UDN_FIRST - 1)
    End Enum

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetScrollRange(ByVal hWnd As IntPtr, ByVal nBar As ScrollBarDirection, ByRef lpMinPos As Integer, ByRef lpMaxPos As Integer) As Boolean
    End Function

    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Function GetScrollPos(ByVal hWnd As IntPtr, ByVal nBar As ScrollBarDirection) As Integer
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function SetScrollPos(ByVal hWnd As IntPtr, ByVal nBar As ScrollBarDirection, ByVal nPos As Integer, ByVal bRedraw As Boolean) As Integer
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetScrollInfo(ByVal hWnd As IntPtr, ByVal fnBar As ScrollBarDirection, ByRef lpsi As IntPtr) As Integer
    End Function


    <StructLayout(LayoutKind.Sequential)>
    Public Structure SCROLLINFO
        Public cbSize As Integer
        Public fMask As ScrollInfoMask
        Public nMin As Integer
        Public nMax As Integer
        Public nPage As Integer
        Public nPos As Integer
        Public nTrackPos As Integer
    End Structure

    Public Enum ScrollBarDirection
        SB_HORZ = 0
        SB_VERT = 1
        SB_CTL = 2
        SB_BOTH = 3
    End Enum

    Public Enum ScrollInfoMask
        SIF_RANGE = &H1
        SIF_PAGE = &H2
        SIF_POS = &H4
        SIF_DISABLENOSCROLL = &H8
        SIF_TRACKPOS = &H10
        SIF_ALL = (SIF_RANGE Or SIF_PAGE Or SIF_POS Or SIF_TRACKPOS)
    End Enum

    ''' <summary>
    ''' Mask flags associated with the TCITEM structure.
    ''' </summary>
    Public Enum TCIF
        ''' <summary>
        ''' The pszText member is valid, or is requested.
        ''' </summary>
        TCIF_TEXT = 1
        ''' <summary>
        ''' The iImage member is valid, or is requested.
        ''' </summary>
        TCIF_IMAGE = 2
    End Enum

    <StructLayout(LayoutKind.Sequential)>
    Public Structure LVFINDINFO
        Public flags As Int32
        Public psz As IntPtr
        Public lParam As Int32
        Public pt As POINTAPI
        Public vkDirection As Int32
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure LV_ITEM
        Public mask As Int32
        Public iItem As Int32
        Public iSubItem As Int32
        Public state As Int32
        Public stateMask As Int32
        Public pszText As IntPtr
        Public cchTextMax As Int32
        Public iImage As Int32
        Public lParam As Int32
        Public iIndent As Int32
    End Structure



    Public Enum LVFI
        ''' <summary>
        ''' Searches for a match between this structure's
        ''' lParam member and the lParam member of an item's
        ''' LVITEM structure
        ''' </summary>
        LVFI_PARAM = &H1
        ''' <summary>
        ''' Searches based on the item text. Unless additional
        ''' values are specified, the item text of the matching
        ''' item must exactly match the string pointed to by the
        ''' psz member. However, the search is case-insensitive.
        ''' </summary>
        LVFI_STRING = &H2
        ''' <summary>
        ''' Checks to see if the item text begins with the
        ''' string pointed to by the psz member. This value
        ''' implies use of LVFI_STRING.
        ''' </summary>
        LVFI_PARTIAL = &H8
        ''' <summary>
        ''' Continues the search at the beginning if no match
        ''' is found. If this flag is used by itself, it is
        ''' assumed that a string search is wanted.
        ''' </summary>
        LVFI_WRAP = &H20
        ''' <summary>
        ''' Finds the item nearest to the position specified in
        ''' the pt member, in the direction specified by the
        ''' vkDirection member. This flag is supported only by
        ''' large icon and small icon modes. If LVFI_NEARESTXY
        ''' is specified, all other flags are ignored.
        ''' </summary>
        LVFI_NEARESTXY = &H40
    End Enum

    ''' <summary>
    ''' Flags used with the LV_ITEM structure to get/set
    ''' the mask member's text, state, image, etc. The flags
    ''' are inserted into the mask member of the LV_ITEM structure.
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum LVIF
        ''' <summary>
        ''' The pszText member of the LV_ITEM structure is valid or is requested.
        ''' </summary>
        LVIF_TEXT = &H1
        ''' <summary>
        ''' The iImage member of the LV_ITEM structure is valid or is requested.
        ''' </summary>
        LVIF_IMAGE = &H2
        ''' <summary>
        ''' The state member of the LV_ITEM structure is valid or is requested.
        ''' </summary>
        LVIF_STATE = &H8
    End Enum

    Public Enum LVIS
        LVIS_FOCUSED = &H1
        LVIS_SELECTED = &H2
        LVIS_CUT = &H4
        LVIS_DROPHILITED = &H8
        LVIS_OVERLAYMASK = &HF00
        LVIS_STATEIMAGEMASK = &HF000
    End Enum




    <StructLayout(LayoutKind.Sequential)>
    Public Structure HDITEM
        Public mask As System.Int32
        Public cxy As Int32
        Public pszText As IntPtr
        Public hbm As IntPtr
        Public cchTextMax As Int32
        Public fmt As Int32
        Public lParam As Int32
        Public iImage As Int32
        Public iOrder As Int32
    End Structure

    ''' <summary>
    ''' Statemask flags used in conjunction with the HDItem
    ''' structure.
    ''' </summary>
    Public Enum HDI
        HDI_WIDTH = &H1
        HDI_HEIGHT = HDI_WIDTH
        HDI_TEXT = &H2
        HDI_FORMAT = &H4
        HDI_LPARAM = &H8
        HDI_BITMAP = &H10
        HDI_IMAGE = &H20
        HDI_DI_SETITEM = &H40
        HDI_ORDER = &H80
    End Enum

    ''' <summary>
    ''' Reserves or commits a region of memory within
    ''' the virtual address space of a specified process.
    ''' The function initializes the memory it allocates
    ''' to zero, unless MEM_RESET is used.
    ''' </summary>
    ''' <param name="hProcess">The handle to a process.
    ''' The function allocates memory within the virtual
    ''' address space of this process. The handle must
    ''' have the PROCESS_VM_OPERATION access right. For
    ''' more information, see Process Security and
    ''' Access Rights.</param>
    ''' <param name="lpAddress">The pointer that specifies
    ''' a desired starting address for the region of pages
    ''' that you want to allocate. If you are reserving
    ''' memory, the function rounds this address down to
    ''' the nearest multiple of the allocation granularity.
    ''' 
    ''' If you are committing memory that is already reserved,
    ''' the function rounds this address down to the nearest
    ''' page boundary. To determine the size of a page and the
    ''' allocation granularity on the host computer, use the
    ''' GetSystemInfo function.
    ''' 
    ''' If lpAddress is NULL, the function determines where
    ''' to allocate the region.</param>
    ''' <param name="dwSize">The size of the region of memory
    ''' to allocate, in bytes. If lpAddress is NULL, the
    ''' function rounds dwSize up to the next page boundary.
    '''
    ''' If lpAddress is not NULL, the function allocates all
    ''' pages that contain one or more bytes in the range from
    ''' lpAddress to lpAddress+dwSize. This means, for example,
    ''' that a 2-byte range that straddles a page boundary
    ''' causes the function to allocate both pages.</param>
    ''' <param name="flAllocationType">The type of memory allocation.
    ''' See http://msdn2.microsoft.com/en-us/library/aa366890.aspx</param>
    ''' <param name="flProtect">The memory protection for the region
    ''' of pages to be allocated. If the pages are being committed,
    ''' you can specify any one of the memory protection constants.
    '''
    ''' Protection attributes specified when protecting a page cannot
    ''' conflict with those specified when allocating a page.</param>
    ''' <returns>If the function succeeds, the return value is the base
    ''' address of the allocated region of pages.</returns>
    ''' <remarks></remarks>
    <DllImport("kernel32.dll")>
    Public Function VirtualAllocEx(ByVal hProcess As IntPtr, ByVal lpAddress As IntPtr, ByVal dwSize As Integer, ByVal flAllocationType As Integer, ByVal flProtect As Integer) As IntPtr
    End Function

    <DllImport("kernel32.dll")>
    Public Function VirtualFreeEx(ByVal hProcess As IntPtr, ByVal lpAddress As IntPtr, ByVal dwSize As Int32, ByVal dwFreeType As Int32) As Boolean
    End Function

    <DllImport("kernel32.dll")>
    Public Function VirtualProtect(ByVal lpAddress As IntPtr, ByVal dwSize As Int32, ByVal flNewProtect As Int32, ByRef lpflOldProtect As Int32) As Boolean
    End Function


    ''' <summary>
    ''' Writes data to an area of memory in a specified process.
    ''' The entire area to be written to must be accessible or
    ''' the operation fails.
    ''' </summary>
    ''' <param name="hProcess">A handle to the process memory to
    ''' be modified. The handle must have PROCESS_VM_WRITE and
    ''' PROCESS_VM_OPERATION access to the process.</param>
    ''' <param name="lpBaseAddress">A pointer to the base address
    ''' in the specified process to which data is written. Before
    ''' data transfer occurs, the system verifies that all data
    ''' in the base address and memory of the specified size is
    ''' accessible for write access, and if it is not accessible,
    ''' the function fails.</param>
    ''' <param name="lpBuffer">A pointer to the buffer that contains
    ''' data to be written in the address space of the specified
    ''' process.</param>
    ''' <param name="nSize">The number of bytes to be written to
    ''' the specified process.</param>
    ''' <param name="lpNumberOfBytesWritten">A pointer to a variable
    ''' that receives the number of bytes transferred into the
    ''' specified process. This parameter is optional. If
    ''' lpNumberOfBytesWritten is NULL, the parameter is
    ''' ignored</param>
    ''' <returns>If the function succeeds, the return value is nonzero.
    '''
    ''' If the function fails, the return value is 0 (zero). To get extended
    ''' error information, call GetLastError. The function fails if the
    ''' requested write operation crosses into an area of the process
    ''' that is inaccessible.</returns>
    ''' <remarks></remarks>
    Public Declare Function WriteProcessMemory Lib "kernel32" (ByVal hProcess As IntPtr, ByVal lpBaseAddress As IntPtr, ByVal lpBuffer As IntPtr, ByVal nSize As Int32, ByVal lpNumberOfBytesWritten As IntPtr) As Integer

    ''' <summary>
    ''' Reads data located in another process' memory space, into
    ''' a local buffer.
    ''' </summary>
    ''' <param name="hProcess">A handle to the process with memory
    ''' that is being read. The handle must have PROCESS_VM_READ
    ''' access to the process</param>
    ''' <param name="lpBaseAddress">A pointer to the base address
    ''' in the specified process from which to read. Before any
    ''' data transfer occurs, the system verifies that all data in
    ''' the base address and memory of the specified size is
    ''' accessible for read access, and if it is not accessible
    ''' the function fails</param>
    ''' <param name="lpBuffer">A pointer to a buffer that receives
    ''' the contents from the address space of the specified
    ''' process</param>
    ''' <param name="nSize">The number of bytes to be read from
    ''' the specified process.</param>
    ''' <param name="lpNumberOfBytesWritten">A pointer to a variable
    ''' that receives the number of bytes transferred into the
    ''' specified buffer. If lpNumberOfBytesRead is NULL, the parameter
    ''' is ignored</param>
    ''' <returns>If the function succeeds, the return value is nonzero.
    '''
    ''' If the function fails, the return value is 0 (zero). To get
    ''' extended error information, call GetLastError.
    '''
    ''' The function fails if the requested read operation crosses 
    ''' into an area of the process that is inaccessible.</returns>
    ''' <remarks></remarks>
    Public Declare Function ReadProcessMemory Lib "kernel32" (ByVal hProcess As IntPtr, ByVal lpBaseAddress As IntPtr, ByVal lpBuffer As IntPtr, ByVal nSize As Int32, ByVal lpNumberOfBytesWritten As IntPtr) As Integer

    ''' <summary>
    ''' Used in conjunction with the function VirtualAllocEx.
    ''' 
    ''' Allocates physical storage in memory or in the paging file
    ''' on disk for the specified reserved memory pages. The
    ''' function initializes the memory to zero.
    '''
    ''' To reserve and commit pages in one step, call VirtualAllocEx
    ''' with MEM_COMMIT | MEM_RESERVE.
    '''
    ''' The VirtualAllocEx function fails if you attempt to commit a page that has
    ''' not been reserved. The resulting error code is ERROR_INVALID_ADDRESS.
    '''
    ''' An attempt to commit a page that is already committed does not
    ''' cause the VirtualAllocEx function to fail. This means that you can commit
    ''' pages without first determining the current commitment state
    ''' of each page.
    ''' </summary>
    Public Const MEM_COMMIT As Integer = &H1000
    ''' <summary>
    ''' Used in conjunction with the function VirtualAllocEx.
    ''' 
    ''' Reserves a range of the process's virtual address space without
    ''' allocating any actual physical storage in memory or in the
    ''' paging file on disk.
    '''
    ''' You commit reserved pages by calling VirtualAllocEx again with
    ''' MEM_COMMIT. To reserve and commit pages in one step, call
    ''' VirtualAllocEx with MEM_COMMIT | MEM_RESERVE.
    '''
    ''' Other memory allocation functions, such as malloc and LocalAlloc,
    ''' cannot use reserved memory until it has been released.
    ''' </summary>
    Public Const MEM_RESERVE As Integer = &H2000

    ''' <summary>
    ''' Used in conjunction with the function VirtualAllocEx.
    ''' 
    ''' Indicates that data in the memory range specified by lpAddress
    ''' and dwSize is no longer of interest. The pages should not be read
    ''' from or written to the paging file. However, the memory block will
    ''' be used again later, so it should not be decommitted. This value
    ''' cannot be used with any other value.
    ''' 
    ''' Using this value does not guarantee that the range operated on with
    ''' MEM_RESET will contain zeroes. If you want the range to contain zeroes,
    ''' decommit the memory and then recommit it.
    '''
    ''' When you use MEM_RESET, the VirtualAllocEx function ignores the value of
    ''' fProtect. However, you must still set fProtect to a valid protection value,
    ''' such as PAGE_NOACCESS.
    '''
    ''' VirtualAllocEx returns an error if you use MEM_RESET and the range of
    ''' memory is mapped to a file. A shared view is only acceptable if it is
    ''' mapped to a paging file.
    ''' </summary>
    Public Const MEM_RELEASE As Integer = &H8000

    ''' <summary>
    ''' Used in conjunction with the function VirtualAllocEx.
    ''' Enables both read and write access to the committed region of pages.
    ''' </summary>
    Public Const PAGE_READWRITE As Integer = &H4

    <DllImport("kernel32.dll")>
    Public Function OpenProcess(
     ByVal dwDesiredAccess As ProcessAccess,
     <MarshalAs(UnmanagedType.Bool)> ByVal bInheritHandle As Boolean,
     ByVal dwProcessId As Int32) As IntPtr
    End Function

    <Flags()>
    Public Enum ProcessAccess
        PROCESS_TERMINATE = &H1
        PROCESS_CREATE_THREAD = &H2
        PROCESS_SET_SESSIONID = &H4
        PROCESS_VM_OPERATION = &H8
        PROCESS_VM_READ = &H10
        PROCESS_VM_WRITE = &H20
        PROCESS_DUP_HANDLE = &H40
        PROCESS_CREATE_PROCESS = &H80
        PROCESS_SET_QUOTA = &H100
        PROCESS_SET_INFORMATION = &H200
        PROCESS_QUERY_INFORMATION = &H400
        PROCESS_SUSPEND_RESUME = &H800
        STANDARD_RIGHTS_REQUIRED = &HF0000
        SYNCHRONIZE = &H100000

        PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED _
         Or SYNCHRONIZE _
         Or &HFFF

        ''' <summary>
        ''' PROCESS_CREATE_THREAD |
        ''' PROCESS_QUERY_INFORMATION |
        ''' PROCESS_VM_OPERATION |
        ''' PROCESS_VM_WRITE
        ''' </summary>
        UNHOOK_RIGHTS = PROCESS_CREATE_THREAD _
         Or PROCESS_QUERY_INFORMATION _
         Or PROCESS_VM_OPERATION _
         Or PROCESS_VM_WRITE

        ''' <summary>
        ''' PROCESS_CREATE_THREAD |
        ''' PROCESS_QUERY_INFORMATION |
        ''' PROCESS_VM_OPERATION |
        ''' PROCESS_VM_WRITE |
        ''' PROCESS_VM_READ
        ''' </summary>
        CREATE_THREAD = PROCESS_CREATE_THREAD _
         Or PROCESS_QUERY_INFORMATION _
         Or PROCESS_VM_OPERATION _
         Or PROCESS_VM_WRITE _
         Or PROCESS_VM_READ

        ''' <summary>
        ''' PROCESS_VM_OPERATION |
        ''' PROCESS_VM_READ |
        ''' PROCESS_VM_WRITE |
        ''' SYNCHRONIZE |
        ''' PROCESS_TERMINATE
        ''' </summary>
        ATTACH_RIGHTS = PROCESS_VM_OPERATION _
         Or PROCESS_VM_READ _
         Or PROCESS_VM_WRITE _
         Or SYNCHRONIZE _
         Or PROCESS_TERMINATE

        ''' <summary>
        ''' PROCESS_QUERY_INFORMATION |
        ''' PROCESS_CREATE_THREAD |
        ''' PROCESS_VM_OPERATION |
        ''' PROCESS_VM_WRITE
        ''' </summary>
        HOOK_RIGHTS = PROCESS_QUERY_INFORMATION _
         Or PROCESS_CREATE_THREAD _
         Or PROCESS_VM_OPERATION _
         Or PROCESS_VM_WRITE

    End Enum

    'List box Notification messages:
    Public Const LBN_SELCHANGE As Integer = 1
    Public Const LBN_DBLCLK As Integer = 2
    Public Const LBN_SELCANCEL As Integer = 3
    Public Const LBN_SETFOCUS As Integer = 4
    Public Const LBN_KILLFOCUS As Integer = 5


    ''' <summary>
    ''' Combo box notification messages.
    ''' </summary>
    Public Enum CBN
        ''' <summary>
        ''' Notification message sent when the user changes the
        ''' current selection in the list box of a combo box
        ''' </summary>
        CBN_SELCHANGE = &H1
        ''' <summary>
        ''' Notification message sent after the user has taken an
        ''' action that may have altered the text in the edit control
        ''' portion of a combo box. Unlike the CBN_EDITUPDATE
        ''' notification message, this notification message is sent
        ''' after the system updates the screen.
        ''' </summary>
        CBN_EDITCHANGE = &H5
        ''' <summary>
        ''' Notification message sent when the edit control portion of
        ''' a combo box is about to display altered text. This
        ''' notification message is sent after the control has formatted
        ''' the text, but before it displays the text.
        ''' </summary>
        CBN_EDITUPDATE = &H6
        ''' <summary>
        ''' Notification message sent when the list box of a combo box
        ''' is about to be made visible.
        ''' </summary>
        CBN_DROPDOWN = &H7
        ''' <summary>
        ''' Notification message sent when the list box of a combo box has been closed.
        ''' </summary>
        CBN_CLOSEUP = &H8
        ''' <summary>
        ''' Notification message sent when the user selects a list item,
        ''' or selects an item and then closes the list. It indicates that
        ''' the user's selection is to be processed.
        ''' </summary>
        CBN_SELENDOK = &H9
    End Enum

    ''' <summary>
    ''' DateTimePicker notification messages
    ''' </summary>
    Public Enum DTN
        DTN_FIRST2 = -753
        DTN_DATETIMECHANGE = DTN_FIRST2 - 6
    End Enum

    ''' <summary>
    ''' Tab control notification messages.
    ''' </summary>
    Public Enum TCN
        TCN_FIRST = -550
        ''' <summary>
        ''' Notifies a tab control's parent window that the
        ''' currently selected tab has changed. Used in
        ''' conjunction with WM_NOTIFY.
        ''' </summary>
        TCN_SELCHANGE = (TCN_FIRST - 1)
        ''' <summary>
        ''' Notifies a tab control's parent window that the
        ''' currently selected tab is about to change. Used in
        ''' conjunction with WM_NOTIFY.
        ''' </summary>
        ''' <remarks></remarks>
        TCN_SELCHANGING = (TCN_FIRST - 2)
        ''' <summary>
        ''' Sent by a tab control when it has the TCS_EX_REGISTERDROP
        ''' extended style and an object is dragged over a tab item
        ''' in the control. Used in conjunction with WM_NOTIFY.
        ''' </summary>
        TCN_GETOBJECT = (TCN_FIRST - 3)
    End Enum

    ' This GetWindowLong should not retrieve pointers (which may be 32 or 64 bit) - 
    ' for that use GetWindowLongPtr

    <DllImport("User32.dll", SetLastError:=True, EntryPoint:="GetWindowLong")>
    Public Function GetWindowLong(
     ByVal hWnd As IntPtr, <MarshalAs(UnmanagedType.I4)> ByVal nInd As GWL) As Int32
    End Function

    ' There are 2 mechanisms for getting a long pointer from a window, depending on
    ' whether this is running in 32 or 64 bit mode.

    <DllImport("user32.dll", EntryPoint:="GetWindowLong")>
    Private Function GetWindowLongPtr32(ByVal hWnd As IntPtr,
     <MarshalAs(UnmanagedType.I4)> ByVal nInd As GWL) As IntPtr
    End Function

    <DllImport("user32.dll", EntryPoint:="GetWindowLongPtr")>
    Private Function GetWindowLongPtr64(ByVal hWnd As IntPtr,
     <MarshalAs(UnmanagedType.I4)> ByVal nInd As GWL) As IntPtr
    End Function

    ' This method is required because Win32 does not support GetWindowLongPtr directly
    Public Function GetWindowLongPtr(ByVal hWnd As IntPtr, ByVal nInd As GWL) As IntPtr
        If IntPtr.Size = 8 Then
            Return GetWindowLongPtr64(hWnd, nInd)
        Else
            Return GetWindowLongPtr32(hWnd, nInd)
        End If
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function SetWindowLong(ByVal hWnd As IntPtr, ByVal nInd As GWL, ByVal dwNewLong As Int32) As Integer
    End Function

    Public Enum GWL
        GWL_WNDPROC = (-4)
        GWL_HINSTANCE = (-6)
        GWL_HWNDPARENT = (-8)
        GWL_STYLE = (-16)
        GWL_EXSTYLE = (-20)
        GWL_USERDATA = (-21)
        GWL_ID = (-12)
    End Enum

    ''' <summary>
    ''' Edit Control Styles - from winuser.h
    ''' See https://msdn.microsoft.com/en-us/library/windows/desktop/bb775464(v=vs.85).aspx
    ''' for details.
    ''' </summary>
    <Flags>
    Public Enum ES
        ES_LEFT = &H0
        ES_CENTER = &H1
        ES_RIGHT = &H2
        ES_MULTILINE = &H4
        ES_UPPERCASE = &H8
        ES_LOWERCASE = &H10
        ES_PASSWORD = &H20
        ES_AUTOVSCROLL = &H40
        ES_AUTOHSCROLL = &H80
        ES_NOHIDESEL = &H100
        ES_OEMCONVERT = &H400
        ES_READONLY = &H800
        ES_WANTRETURN = &H1000
        ES_NUMBER = &H2000
    End Enum


    ''' <summary>
    ''' Styles for a Month Calendar
    ''' control (SysMonthCal32).
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum MCS
        MCS_DAYSTATE = 1
        MCS_MULTISELECT = 2
        MCS_WEEKNUMBERS = 4
        MCS_NOTODAYCIRCLE = 8
        MCS_NOTODAY = 16
    End Enum

    <DllImport("User32.dll", SetLastError:=True, EntryPoint:="GetWindow")>
    Public Function GetWindow(ByVal hWnd As IntPtr, ByVal uCmd As Integer) As IntPtr
    End Function

    Public Const GW_HWNDNEXT As Integer = 2
    Public Const GW_HWNDPREV As Integer = 3

    Public Const RDW_FRAME As Integer = &H400
    Public Const RDW_INVALIDATE As Integer = &H1
    Public Const RDW_UPDATENOW As Integer = &H100
    Public Const RDW_ALLCHILDREN As Integer = &H80

    Public Declare Function WaitForInputIdle Lib "user32.dll" (ByVal hProcess As IntPtr, ByVal dwMilliseconds As Int32) As Int32

    <DllImport("kernel32.dll", SetLastError:=True)>
    Public Function WaitForSingleObject(ByVal handle As IntPtr, ByVal milliseconds As Integer) As Integer
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Public Function WaitForMultipleObjects(nCount As UInt32, lpHandles As IntPtr(), bWaitAll As Boolean, dwMilliseconds As UInt32) As WaitResults
    End Function

    <DllImport("kernel32.dll")>
    Public Function CreateEvent(ByVal lpEventAttributes As IntPtr, ByVal bManualReset As Boolean, ByVal bInitialState As Boolean, ByVal lpName As String) As IntPtr
    End Function

    <DllImport("kernel32.dll")>
    Public Function SetEvent(hEvent As IntPtr) As Boolean
    End Function


    ''' <summary>
    ''' Return values for the WaitForSingleObject function
    ''' </summary>
    Public Enum WaitResults
        WAIT_OBJECT_0 = &H0
        WAIT_OBJECT_1 = &H1
        WAIT_OBJECT_2 = &H2
        WAIT_FAILED = &HFFFFFFFF
        WAIT_TIMEOUT = &H102
        WAIT_ABANDONED = &H80
    End Enum

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetParent(ByVal hWnd As IntPtr) As IntPtr
    End Function

    <DllImport("User32.dll", SetLastError:=True, EntryPoint:="SetForegroundWindow")>
    Public Function SetForegroundWindow(ByVal hwnd As Int32) As Integer
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function InvalidateRect(ByVal hWnd As IntPtr, ByVal lpRect As IntPtr, ByVal bErase As Integer) As Integer
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function UpdateWindow(ByVal hWnd As IntPtr) As Integer
    End Function

    <DllImport("oleacc.dll")>
    Public Function AccessibleObjectFromPoint(ByVal pt As POINTAPI, <Out(), MarshalAs(UnmanagedType.Interface)> ByRef accObj As IAccessible, <Out()> ByRef ChildID As Object) As IntPtr
    End Function

    <DllImport("oleacc.dll")>
    Public Function WindowFromAccessibleObject(ByVal pacc As Accessibility.IAccessible, ByRef phwnd As IntPtr) As Integer
    End Function

    <DllImport("oleacc.dll")>
    Public Function GetRoleText(ByVal dwRole As Integer, <[Out]()> ByVal lpszRole As StringBuilder, ByVal cchRoleMax As Integer) As Integer
    End Function

    <DllImport("oleacc.dll")>
    Public Function GetStateText(ByVal dwStateBit As Integer, <Out()> ByVal lpszStateBit As StringBuilder, ByVal cchStateBitMax As Integer) As Integer
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetMenuString(ByVal hMenu As IntPtr, ByVal uIDItem As Integer, <MarshalAs(UnmanagedType.LPStr)> ByVal lpString As StringBuilder, ByVal nMaxCount As Integer, ByVal uFlag As Integer) As Integer
    End Function

    ''' <summary>
    ''' Gets the number of items in a menu.
    ''' </summary>
    ''' <param name="WindowHandle">Handle to the menu of
    ''' interest.</param>
    ''' <returns>Returns the number of items in the specified
    ''' menu, or -1 in the event of an error.</returns>
    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetMenu(ByVal WindowHandle As IntPtr) As IntPtr
    End Function

    ''' <summary>
    ''' Gets the submenu at the specified child position of
    ''' the parent window.
    ''' </summary>
    ''' <param name="MenuHandle">Handle to the parent menu of
    ''' interest.</param>
    ''' <param name="nPos">The zero-based index of the menu item
    ''' which activates the submenu.</param>
    ''' <returns>Returns a handle to the submenu, or null if no
    ''' such menu exists.</returns>
    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetSubMenu(ByVal MenuHandle As IntPtr, ByVal nPos As Integer) As IntPtr
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetMenuItemCount(ByVal hMenu As IntPtr) As Integer
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetMenuItemInfo(<[In]()> ByVal hMenu As IntPtr, <[In]()> ByVal uItem As Integer, <[In]()> ByVal fByPosition As Integer, <[In](), Out()> ByRef lpmii As MENUITEMINFO) As Boolean
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetMenuState(ByVal hMenu As Integer, ByVal uItem As Integer, ByVal fByPosition As Boolean, ByRef lpmii As IntPtr) As Integer
    End Function


    <DllImport("User32.dll", SetLastError:=True, EntryPoint:="GetMenuInfo")>
    Public Function GetMenuInfo(<[In]()> ByVal hMenu As IntPtr, <Out()> ByRef lpcmi As MENUINFO) As Integer
    End Function



    ''' <summary>
    ''' Gets the bounds of the specified menu item.
    ''' </summary>
    ''' <param name="hWnd">The window containing the menu of interest.</param>
    ''' <param name="hMenu">The menu of interest.</param>
    ''' <param name="uItem">The zero-based position of the item of interest.</param>
    ''' <param name="lprcItem">A pointer to a RECT structure to carry back the
    ''' bounds of the menu item of interest.</param>
    ''' <returns>Non-zero to indicate success; zero otherwise.</returns>
    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetMenuItemRect(ByVal hWnd As IntPtr, ByVal hMenu As IntPtr, ByVal uItem As Integer, ByRef lprcItem As IntPtr) As Boolean
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetMenuItemID(ByVal hMenu As IntPtr, ByVal nPos As Integer) As Integer
    End Function

    Public Enum MIM
        MIM_MAXHEIGHT = &H1
        MIM_BACKGROUND = &H2
        MIM_HELPID = &H4
        MIM_MENUDATA = &H8
        MIM_STYLE = &H10
        MIM_APPLYTOSUBMENUS = &H80000000
    End Enum

    <StructLayout(LayoutKind.Sequential)>
    Public Structure MENUINFO
        Dim cbSize As Int32
        Dim fMask As Integer
        Dim dwStyle As Integer
        Dim cyMax As Int32
        Dim hbrBack As IntPtr
        Dim dwContextHelpID As Int32
        Dim dwMenuData As Int32
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure MENUITEMINFO
        Public cbSize As Integer
        Public fMask As Integer
        Public fType As Integer
        Public fState As Integer
        Public wID As Integer
        Public hSubMenu As IntPtr
        Public hbmpChecked As IntPtr
        Public hbmpUnchecked As IntPtr
        Public dwItemData As IntPtr
        Public dwTypeData As IntPtr
        Public cch As Integer
        Public hbmpItem As IntPtr
    End Structure

    ''' <summary>
    ''' Menu item info flags
    ''' </summary>
    Public Enum MIIM
        MIIM_STATE = &H1
        MIIM_ID = &H2
        MIIM_SUBMENU = &H4
        MIIM_CHECKMARKS = &H8
        MIIM_TYPE = &H10
        MIIM_DATA = &H20
        MIIM_STRING = &H40
        MIIM_BITMAP = &H80
        MIIM_FTYPE = &H100
    End Enum

    ''' <summary>
    ''' Menu flags used with GetMenuString function, and similar
    ''' </summary>
    Public Enum MF
        MF_INSERT = &H0
        MF_CHANGE = &H80
        MF_APPEND = &H100
        MF_DELETE = &H200
        MF_REMOVE = &H1000

        MF_BYCOMMAND = &H0
        MF_BYPOSITION = &H400

        MF_SEPARATOR = &H800

        MF_ENABLED = &H0
        MF_GRAYED = &H1
        MF_DISABLED = &H2

        MF_UNCHECKED = &H0
        MF_CHECKED = &H8
        MF_USECHECKBITMAPS = &H200

        MF_STRING = &H0
        MF_BITMAP = &H4
        MF_OWNERDRAW = &H100

        MF_POPUP = &H10
        MF_MENUBARBREAK = &H20
        MF_MENUBREAK = &H40

        MF_UNHILITE = &H0
        MF_HILITE = &H80

        MF_DEFAULT = &H1000
        MF_SYSMENU = &H2000
        MF_HELP = &H4000
        MF_RIGHTJUSTIFY = &H4000

        MF_MOUSESELECT = &H8000
        MF_END = &H80  ' Obsolete -- only used by old RES files 
    End Enum

    Public Enum MFT
        MFT_STRING = MF.MF_STRING
        MFT_BITMAP = MF.MF_BITMAP
        MFT_MENUBARBREAK = MF.MF_MENUBARBREAK
        MFT_MENUBREAK = MF.MF_MENUBREAK
        MFT_OWNERDRAW = MF.MF_OWNERDRAW
        MFT_RADIOCHECK = &H200
        MFT_SEPARATOR = MF.MF_SEPARATOR
        MFT_RIGHTORDER = &H2000
        MFT_RIGHTJUSTIFY = MF.MF_RIGHTJUSTIFY
    End Enum


    Public Enum MFS
        MFS_GRAYED = &H3
        MFS_DISABLED = MFS_GRAYED
        MFS_CHECKED = MF.MF_CHECKED
        MFS_HILITE = MF.MF_HILITE
        MFS_ENABLED = MF.MF_ENABLED
        MFS_UNCHECKED = MF.MF_UNCHECKED
        MFS_UNHILITE = MF.MF_UNHILITE
        MFS_DEFAULT = MF.MF_DEFAULT
    End Enum

    ''' <summary>
    ''' Menu style information
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum MNS
        MNS_NOCHECK = &H80000000
        MNS_MODELESS = &H40000000
        MNS_DRAGDROP = &H20000000
        MNS_AUTODISMISS = &H10000000
        MNS_NOTIFYBYPOS = &H8000000
        MNS_CHECKORBMP = &H4000000
    End Enum

    ''' <summary>
    ''' Class to pass info via the lparam when calling EnumWindows or EnumChildWindows.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class clsEnumWindowsInfo
        Public TargetPID As Integer
        Public Tag As Object

        ''' <summary>
        ''' Attach process matching data.
        ''' </summary>
        Public AllProcesses As IDictionary(Of Integer, Tuple(Of String, Func(Of String)))
        Public MatchResults As ISet(Of Integer)
        Public TitleMatcher As Object
        Public ProcessNameMatcher As Object
        Public UsernameMatcher As Object
        Public VisibleOnly As Boolean
        Public UseWMI As Boolean
    End Class

    Delegate Function EnumWindowsProc(ByVal hWnd As System.IntPtr, ByVal lParam As clsEnumWindowsInfo) As Boolean

    ''' <summary>
    ''' Enumerates all top-level windows on the screen by passing
    ''' the handle to each window, in turn, to an
    ''' application-defined callback function
    ''' </summary>
    ''' <param name="lpEnumFunc">Pointer to an application-defined callback function. For more information, see EnumWindowsProc.</param>
    ''' <param name="lParam">Specifies an application-defined value to be passed to the callback function.</param>
    ''' <returns>If the function succeeds, the return value is nonzero.</returns>
    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Function EnumWindows(ByVal lpEnumFunc As EnumWindowsProc, ByVal lParam As clsEnumWindowsInfo) As Boolean
    End Function
    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Function EnumChildWindows(ByVal hWndParent As System.IntPtr,
       ByVal lpEnumFunc As EnumWindowsProc, ByVal lParam As clsEnumWindowsInfo) As Boolean
    End Function

    ''' <summary>
    ''' Enumerates all top-level windows on the screen by passing the handle to each
    ''' window, in turn, to an application-defined callback function
    ''' </summary>
    ''' <param name="lpEnumFunc">The function to call with the window handle of the
    ''' enumerated window as its first argument and <paramref name="lParam"/> as its
    ''' second. The function should return true to continue enumeration; false to
    ''' halt enumeration</param>
    ''' <param name="lParam">The state data to pass to each callback function call.
    ''' </param>
    ''' <returns>True on success; False on failure</returns>
    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Function EnumWindows(
     ByVal lpEnumFunc As EnumWindowsProc, ByVal lParam As IntPtr) As Boolean
    End Function

    ''' <summary>
    ''' Enumerates all top-level windows on the screen by passing the handle to each
    ''' window, in turn, to an application-defined callback function
    ''' </summary>
    ''' <param name="proc">The function to call with the window handle of the
    ''' enumerated window as its argument. The function should return true to
    ''' continue enumeration; false to halt enumeration</param>
    Public Sub EnumWindows(proc As Func(Of IntPtr, Boolean))
        EnumWindows(Function(hwnd, info) proc(hwnd), IntPtr.Zero)
    End Sub

    ''' <summary>
    ''' Enumerates all descendent windows of the given window, passing each window
    ''' handle in turn to an application-defined callback function
    ''' </summary>
    ''' <param name="hWndParent">The window handle of the parent window whose
    ''' descendent windows are to be enumerated.</param>
    ''' <param name="lpEnumFunc">The function to call with the window handle of the
    ''' enumerated window as its first argument and <paramref name="lParam"/> as its
    ''' second. The function should return true to continue enumeration; false to
    ''' halt enumeration</param>
    ''' <param name="lParam">The state data to pass to each callback function call.
    ''' </param>
    ''' <returns>True on success; False on failure</returns>
    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Function EnumChildWindows(ByVal hWndParent As IntPtr,
       ByVal lpEnumFunc As EnumWindowsProc, ByVal lParam As IntPtr) As Boolean
    End Function

    ''' <summary>
    ''' Enumerates all descendent windows of the given window, passing each window
    ''' handle in turn to an application-defined callback function
    ''' </summary>
    ''' <param name="hwndParent">The window handle of the parent window whose 
    ''' descendent windows should be enumerated</param>
    ''' <param name="proc">The function to call with the window handle of the child
    ''' window. The function should return true to continue enumeration; false to
    ''' halt enumeration.</param>
    Public Sub EnumChildWindows(hwndParent As IntPtr, proc As Func(Of IntPtr, Boolean))
        EnumChildWindows(hwndParent, Function(hwnd, info) proc(hwnd), IntPtr.Zero)
    End Sub

    ''' <summary>
    ''' Retrieves the identifier of the thread that created the specified window and,
    ''' optionally, the identifier of the process that created the window.
    ''' </summary>
    ''' <param name="hwnd">A handle to the window. </param>
    ''' <param name="lpdwProcessId">A pointer to a variable that receives the process
    ''' identifier. If this parameter is not NULL, GetWindowThreadProcessId copies
    ''' the identifier of the process to the variable; otherwise, it does not.
    ''' </param>
    ''' <returns>The return value is the identifier of the thread that created the
    ''' window.</returns>
    ''' <remarks>http://msdn.microsoft.com/en-us/library/ms633522%28v=vs.85%29.aspx</remarks>
    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetWindowThreadProcessId(
     ByVal hwnd As IntPtr,
     ByRef lpdwProcessId As Integer) As Integer
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function RealGetWindowClass(ByVal hWnd As IntPtr,
       ByVal lpString As StringBuilder,
       ByVal nMaxCount As Integer) As Integer
    End Function

    ''' <summary>
    ''' Gets the 'real' window class using <see cref="RealGetWindowClass"/> and
    ''' wrapping it directly into a string so the caller doesn't have to
    ''' </summary>
    ''' <param name="hwnd">The handle of the window for which the 'real' class is
    ''' required.</param>
    ''' <returns>The real window class - ie. the base type of the window, not
    ''' necessarily the precise specialisation of the instance</returns>
    ''' <remarks>The maximum length for a windows class name is 256 chars (including
    ''' null terminator - see http://tinyurl.com/y73x6sel) - this limits the name to
    ''' 255 chars accordingly.</remarks>
    Public Function GetRealWindowClass(hwnd As IntPtr) As String
        Dim sb As New StringBuilder(255)
        RealGetWindowClass(hwnd, sb, sb.Capacity)
        Return sb.ToString()
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function FindWindow(ByVal lpClassName As String, ByVal lpWindowName As String) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Function FindWindowEx(ByVal parentHandle As IntPtr, ByVal childAfter As IntPtr, ByVal lclassName As String, ByVal windowTitle As String) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Public Function IsWindowVisible(ByVal hWnd As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Function ShowWindow(ByVal hWnd As IntPtr, ByVal nCmdShow As Integer) As Boolean
    End Function

    Public Enum ShowWindowCommands
        SW_HIDE = 0
        SW_SHOWNORMAL = 1
        SW_NORMAL = 1
        SW_SHOWMINIMIZED = 2
        SW_SHOWMAXIMIZED = 3
        SW_MAXIMIZE = 3
        SW_SHOWNOACTIVATE = 4
        SW_SHOW = 5
        SW_MINIMIZE = 6
        SW_SHOWMINNOACTIVE = 7
        SW_SHOWNA = 8
        SW_RESTORE = 9
        SW_SHOWDEFAULT = 10
        SW_FORCEMINIMIZE = 11
        SW_MAX = 11
    End Enum

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function RegisterWindowMessage(ByVal lpString As String) As Integer
    End Function

    <DllImport("oleacc.dll")>
    Public Sub ObjectFromLresult(ByVal lResult As IntPtr, <MarshalAs(UnmanagedType.LPStruct)> ByVal refiid As Guid, ByVal wParam As IntPtr, <MarshalAs(UnmanagedType.IUnknown), Out()> ByRef pDoc As Object)
    End Sub

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function SendMessageTimeout(ByVal hWnd As IntPtr, ByVal Msg As Int32, ByVal wParam As Int32, ByVal lParam As Int32, ByVal fuFlags As SendMessageTimeoutFlags, ByVal uTimeout As UInteger, <Out()> ByRef lpdwResult As Int32) As IntPtr
    End Function

    <Flags()>
    Public Enum SendMessageTimeoutFlags
        SMTO_NORMAL = 0
        SMTO_BLOCK = 1
        SMTO_ABORTIFHUNG = 2
        SMTO_NOTIMEOUTIFNOTHUNG = 8
    End Enum

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function PeekMessage(<Out()> ByRef msg As MSG, ByVal hwnd As IntPtr, ByVal msgMin As Integer, ByVal msgMax As Integer, ByVal remove As Integer) As Boolean
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function GetMessage(<Out()> ByRef msg As MSG, ByVal hWnd As IntPtr, ByVal uMsgFilterMin As Integer, ByVal uMsgFilterMax As Integer) As Boolean
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function SetTimer(hWnd As IntPtr, nIDEvent As IntPtr, uElapse As Integer, lpTimerFunc As TimerProc) As IntPtr
    End Function

    Public Delegate Sub TimerProc(hWnd As IntPtr, uMsg As Integer, nIDEvent As IntPtr, dwTime As UInteger)

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function KillTimer(hWnd As IntPtr, nIDEvent As IntPtr) As Boolean
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function TranslateMessage(<[In](), Out()> ByRef msg As MSG) As Boolean
    End Function

    <DllImport("User32.dll", SetLastError:=True)>
    Public Function DispatchMessage(ByRef msg As MSG) As IntPtr
    End Function

    Public Const PM_NOREMOVE As Integer = &H0
    Public Const PM_REMOVE As Integer = &H1
    Public Const PM_NOYIELD As Integer = &H2


    ''' <summary>
    ''' A non blocking message pump. This can safely be called from a non-ui thread
    ''' or a ui thread.
    ''' </summary>
    Public Sub MessagePump()
        Dim wMsg As MSG
        If PeekMessage(wMsg, IntPtr.Zero, 0, 0, PM_REMOVE) Then
            TranslateMessage(wMsg)
            DispatchMessage(wMsg)
        End If
    End Sub

    ''' <summary>
    ''' A non blocking message pump that will sleep n milliseconds if there is no
    ''' message waiting. This can safely be called from a non-ui thread or a ui
    ''' thread.
    ''' </summary>
    ''' <param name="sleep">The number of milliseconds to sleep</param>
    Public Sub MessagePump(ByVal sleep As Integer)
        Dim wMsg As MSG
        If PeekMessage(wMsg, IntPtr.Zero, 0, 0, PM_REMOVE) Then
            TranslateMessage(wMsg)
            DispatchMessage(wMsg)
        Else
            Threading.Thread.Sleep(sleep)
        End If
    End Sub

    ''' <summary>
    ''' This message pump blocks and should only be called from a UI thread
    ''' for example when spying. Do not use it for queries in general or else
    ''' the query will lock when you run the process in control room.
    ''' </summary>
    Public Sub BlockingMessagePump()
        Dim wMsg As MSG
        If GetMessageWithTimeout(wMsg, 10000) Then
            TranslateMessage(wMsg)
            DispatchMessage(wMsg)
        End If
    End Sub

    Public Function GetMessageWithTimeout(ByRef message As MSG, timeoutMS As Integer) As Boolean
        Dim timerId = SetTimer(IntPtr.Zero, IntPtr.Zero, timeoutMS, Nothing)
        Dim result = GetMessage(message, IntPtr.Zero, 0, 0)
        KillTimer(IntPtr.Zero, timerId)

        If Not result Then Return False

        If message.message = WindowMessages.WM_TIMER AndAlso
                 message.hwnd = IntPtr.Zero AndAlso
                 message.wParam = timerId Then
            Throw New TimeoutException("Didn't receive a response to the spying action within the allotted timeout period")
        End If

        Return True
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Public Structure MSG
        Public hwnd As IntPtr
        Public lParam As IntPtr
        Public message As Integer
        Public pt_x As Integer
        Public pt_y As Integer
        Public time As Integer
        Public wParam As IntPtr
    End Structure


    <DllImport("kernel32.dll", CharSet:=CharSet.Ansi)>
    Public Function GetProcAddress(ByVal hModule As IntPtr, ByVal procName As String) As IntPtr
    End Function

    <DllImport("kernel32.dll", CharSet:=CharSet.Auto)>
    Public Function GetModuleHandle(ByVal lpModuleName As String) As IntPtr
    End Function

    <DllImport("kernel32.dll")>
    Public Function WriteProcessMemory(ByVal hProcess As IntPtr, ByVal lpBaseAddress As IntPtr, ByVal lpBuffer As Byte(), ByVal nSize As Integer, <[Out]()> ByVal lpNumberOfBytesWritten As Integer) As Boolean
    End Function

    <DllImport("kernel32.dll")>
    Public Function CreateRemoteThread(ByVal hProcess As IntPtr, ByVal lpThreadAttributes As IntPtr, ByVal dwStackSize As Integer, ByVal lpStartAddress As IntPtr, ByVal lpParameter As IntPtr, ByVal dwCreationFlags As Integer, ByVal lpThreadId As IntPtr) As IntPtr
    End Function

    <DllImport("psapi.dll")>
    Public Function EnumProcessModules(
    ByVal hProcess As IntPtr,
    <MarshalAs(UnmanagedType.LPArray, ArraySubType:=UnmanagedType.U4), [In](), [Out]()>
    ByVal lphModule As IntPtr(),
    ByVal dwSize As Integer,
    ByRef lpcbNeeded As Integer) As Boolean
    End Function

    <DllImport("psapi.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Public Function GetModuleFileNameEx(ByVal hProcess As IntPtr, ByVal hModule As IntPtr, ByVal baseName() As Byte, ByVal size As Integer) As Integer
    End Function

    <Flags()>
    Public Enum AllocationFlags As Integer
        PAGE_READWRITE = &H4
        PAGE_EXECUTE_READWRITE = &H40
        MEM_COMMIT = &H1000
        MEM_RELEASE = &H8000
    End Enum

    Public Const INFINITE As Integer = &HFFFFFFFF      'Infinite timeout

    <DllImport("ole32.dll")>
    Public Function CreateStreamOnHGlobal(ByVal hGlobal As IntPtr, ByVal fDeleteOnRelease As Boolean, ByRef ppstm As System.Runtime.InteropServices.ComTypes.IStream) As Long
    End Function

    <DllImport("ole32.dll")>
    Public Function CoUnmarshalInterface(ByVal Stream As System.Runtime.InteropServices.ComTypes.IStream, <[In]()> ByRef rrid As Guid, <[Out](), MarshalAs(UnmanagedType.IUnknown)> ByRef ppv As Object) As Integer
    End Function

    <DllImport("gdi32.dll")>
    Public Function GetOutlineTextMetrics(ByVal hdc As IntPtr, ByVal cbData As UInt32, ByVal ptrZero As IntPtr) As UInt32
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Public Structure OUTLINETEXTMETRIC
        Public otmSize As UInt32
        Public otmTextMetrics As TEXTMETRIC
        Public otmFiller As Byte
        Public otmPanoseNumber As PANOSE
        Public otmfsSelection As UInt32
        Public otmfsType As UInt32
        Public otmsCharSlopeRise As Int32
        Public otmsCharSlopeRun As Int32
        Public otmItalicAngle As Int32
        Public otmEMSquare As UInt32
        Public otmAscent As Int32
        Public otmDescent As Int32
        Public otmLineGap As UInt32
        Public otmsCapEmHeight As UInt32
        Public otmsXHeight As UInt32
        Public otmrcFontBox As RECT
        Public otmMacAscent As Int32
        Public otmMacDescent As Int32
        Public otmMacLineGap As UInt32
        Public otmusMinimumPPEM As UInt32
        Public otmptSubscriptSize As modWin32.POINTAPI
        Public otmptSubscriptOffset As modWin32.POINTAPI
        Public otmptSuperscriptSize As modWin32.POINTAPI
        Public otmptSuperscriptOffset As modWin32.POINTAPI
        Public otmsStrikeoutSize As UInt32
        Public otmsStrikeoutPosition As Int32
        Public otmsUnderscoreSize As Int32
        Public otmsUnderscorePosition As Int32
        Public otmpFamilyName As UInt32
        Public otmpFaceName As UInt32
        Public otmpStyleName As UInt32
        Public otmpFullName As UInt32
    End Structure


    <StructLayout(LayoutKind.Sequential)>
    Public Structure PANOSE
        Public bFamilyType As PanoseFontFamilyTypes
        Public bSerifStyle As Byte
        Public bWeight As Byte
        Public bProportion As Byte
        Public bContrast As Byte
        Public bStrokeVariation As Byte
        Public bArmStyle As Byte
        Public bLetterform As Byte
        Public bMidline As Byte
        Public bXHeight As Byte
    End Structure

    <Serializable(), StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Auto)>
    Public Structure TEXTMETRIC
        Public tmHeight As Int32
        Public tmAscent As Int32
        Public tmDescent As Int32
        Public tmInternalLeading As Int32
        Public tmExternalLeading As Int32
        Public tmAveCharWidth As Int32
        Public tmMaxCharWidth As Int32
        Public tmWeight As Int32
        Public tmOverhang As Int32
        Public tmDigitizedAspectX As Int32
        Public tmDigitizedAspectY As Int32
        Public tmFirstChar As Char
        Public tmLastChar As Char
        Public tmDefaultChar As Char
        Public tmBreakChar As Char
        Public tmItalic As Byte
        Public tmUnderlined As Byte
        Public tmStruckOut As Byte
        Public tmPitchAndFamily As Byte
        Public tmCharSet As Byte
    End Structure

    Public Enum PanoseFontFamilyTypes As Byte
        ''' <summary>
        '''  Any
        ''' </summary>
        PAN_ANY = 0
        ''' <summary>
        ''' No Fit
        ''' </summary>
        PAN_NO_FIT = 1
        ''' <summary>
        ''' Text and Display
        ''' </summary>
        PAN_FAMILY_TEXT_DISPLAY = 2
        ''' <summary>
        ''' Script
        ''' </summary>
        PAN_FAMILY_SCRIPT = 3
        ''' <summary>
        ''' Decorative
        ''' </summary>
        PAN_FAMILY_DECORATIVE = 4
        ''' <summary>
        ''' Pictorial                      
        ''' </summary>
        PAN_FAMILY_PICTORIAL = 5
    End Enum

    ''' <summary>
    ''' SafeHandle for use with processes
    ''' </summary>
    <SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode:=True)>
    Public NotInheritable Class SafeProcessHandle
        Inherits SafeHandleZeroOrMinusOneIsInvalid

        Private Sub New()
            Me.New(IntPtr.Zero, True)
        End Sub

        Public Sub New(ByVal existingHandle As IntPtr, ByVal ownsHandle As Boolean)
            MyBase.New(ownsHandle)
            SetHandle(existingHandle)
        End Sub

        Protected Overrides Function ReleaseHandle() As Boolean
            Return CloseHandle(handle)
        End Function

        Public ReadOnly Property RawHandle() As IntPtr
            Get
                Return handle
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Gets details of the last win32 error message, including error
    ''' code and friendly text.
    ''' </summary>
    Public Function GetLastWin32Error() As String
        'The following is a cheat method of getting the message
        'text instead of using the FormatMessage function
        Dim ex As New Win32Exception(Marshal.GetLastWin32Error())
        Return _
         String.Format(My.Resources.modWin32_LastWin32ErrorCodeWas01, ex.NativeErrorCode, ex.Message)
    End Function

    <DllImport("advapi32.dll", CharSet:=CharSet.Unicode)>
    Private Function LogonUser(ByVal lpszUsername As String, ByVal lpszDomain As String, ByVal lpszPassword As IntPtr, ByVal dwLogonType As Integer, ByVal dwLogonProvider As Integer, ByRef phToken As IntPtr) As Boolean
    End Function

    Public Function LogonUser(ByVal username As String, ByVal domainName As String, ByVal password As SecureString, ByVal logonType As Integer, ByVal logonProvider As Integer) As IntPtr
        Dim pwPtr As IntPtr = Marshal.SecureStringToGlobalAllocUnicode(password)
        Try
            Dim tokenHandle As IntPtr
            If Not LogonUser(username, domainName, pwPtr, logonType, logonProvider, tokenHandle) Then
                Return IntPtr.Zero
            End If
            Return tokenHandle
        Finally
            If pwPtr <> Nothing Then Marshal.ZeroFreeGlobalAllocUnicode(pwPtr)
        End Try
    End Function

    <DllImport("shell32.dll", SetLastError:=True)>
    Private Function CommandLineToArgvW(<MarshalAs(UnmanagedType.LPWStr)> lpCmdLine As String, ByRef pNumArgs As Integer) As IntPtr
    End Function

    <DllImport("kernel32.dll")>
    Private Function LocalFree(hMem As IntPtr) As IntPtr
    End Function

    ''' <summary>
    ''' Split a command line argument into an array of commands. This replicates the 
    ''' functionality in Environment.GetCommandLineArgs() and thus has the same 
    ''' parsing and escaping logic.
    ''' </summary>
    Public Function SplitArgs(unsplitArgumentLine As String) As String()
        Dim numberOfArgs As Integer
        Dim ptrToSplitArgs As IntPtr
        Dim args As String()

        If String.IsNullOrWhiteSpace(unsplitArgumentLine) Then Return New String() {}

        ptrToSplitArgs = CommandLineToArgvW(unsplitArgumentLine, numberOfArgs)

        ' CommandLineToArgvW returns NULL upon failure.
        If ptrToSplitArgs = IntPtr.Zero Then
            Throw New ArgumentException(My.Resources.modWin32_UnableToSplitArgument, New Win32Exception())
        End If

        ' Make sure the memory ptrToSplitArgs to is freed, even upon failure.
        Try
            args = New String(numberOfArgs - 1) {}

            ' ptrToSplitArgs is an array of pointers to null terminated Unicode strings.
            ' Copy each of these strings into our split argument array.
            For i As Integer = 0 To numberOfArgs - 1
                args(i) = Marshal.PtrToStringUni(Marshal.ReadIntPtr(ptrToSplitArgs, i * IntPtr.Size))
            Next

            Return args
        Finally
            ' Free memory obtained by CommandLineToArgW.
            LocalFree(ptrToSplitArgs)
        End Try
    End Function

    Public Enum SERVICE_ERROR
        SERVICE_ERROR_IGNORE = 0
        SERVICE_ERROR_NORMAL = 1
        SERVICE_ERROR_SEVERE = 2
        SERVICE_ERROR_CRITICAL = 3
    End Enum

    <Flags()>
    Public Enum ACCESS_MASK : Uint32
        DELETE = &H10000
        READ_CONTROL = &H20000
        WRITE_DAC = &H40000
        WRITE_OWNER = &H80000
        SYNCHRONIZE = &H100000

        STANDARD_RIGHTS_REQUIRED = &HF0000

        STANDARD_RIGHTS_READ = &H20000
        STANDARD_RIGHTS_WRITE = &H20000
        STANDARD_RIGHTS_EXECUTE = &H20000

        STANDARD_RIGHTS_ALL = &H1F0000

        SPECIFIC_RIGHTS_ALL = &HFFFF

        ACCESS_SYSTEM_SECURITY = &H1000000

        MAXIMUM_ALLOWED = &H2000000

        GENERIC_READ = &H80000000
        GENERIC_WRITE = &H40000000
        GENERIC_EXECUTE = &H20000000
        GENERIC_ALL = &H10000000

        DESKTOP_READOBJECTS = &H1
        DESKTOP_CREATEWINDOW = &H2
        DESKTOP_CREATEMENU = &H4
        DESKTOP_HOOKCONTROL = &H8
        DESKTOP_JOURNALRECORD = &H10
        DESKTOP_JOURNALPLAYBACK = &H20
        DESKTOP_ENUMERATE = &H40
        DESKTOP_WRITEOBJECTS = &H80
        DESKTOP_SWITCHDESKTOP = &H100

        WINSTA_ENUMDESKTOPS = &H1
        WINSTA_READATTRIBUTES = &H2
        WINSTA_ACCESSCLIPBOARD = &H4
        WINSTA_CREATEDESKTOP = &H8
        WINSTA_WRITEATTRIBUTES = &H10
        WINSTA_ACCESSGLOBALATOMS = &H20
        WINSTA_EXITWINDOWS = &H40
        WINSTA_ENUMERATE = &H100
        WINSTA_READSCREEN = &H200

        WINSTA_ALL_ACCESS = &H37F
    End Enum

    Public Enum SERVICE_ACCESS As Integer
        SERVICE_QUERY_CONFIG = &H1
        SERVICE_CHANGE_CONFIG = &H2
        SERVICE_QUERY_STATUS = &H4
        SERVICE_ENUMERATE_DEPENDENTS = &H8
        SERVICE_START = &H10
        SERVICE_STOP = &H20
        SERVICE_PAUSE_CONTINUE = &H40
        SERVICE_INTERROGATE = &H80
        SERVICE_USER_DEFINED_CONTROL = &H100
        SERVICE_ALL_ACCESS = (ACCESS_MASK.STANDARD_RIGHTS_REQUIRED Or
            SERVICE_QUERY_CONFIG Or
            SERVICE_CHANGE_CONFIG Or
            SERVICE_QUERY_STATUS Or
            SERVICE_ENUMERATE_DEPENDENTS Or
            SERVICE_START Or
            SERVICE_STOP Or
            SERVICE_PAUSE_CONTINUE Or
            SERVICE_INTERROGATE Or
            SERVICE_USER_DEFINED_CONTROL)

        GENERIC_READ = ACCESS_MASK.STANDARD_RIGHTS_READ Or
            SERVICE_QUERY_CONFIG Or
            SERVICE_QUERY_STATUS Or
            SERVICE_INTERROGATE Or
            SERVICE_ENUMERATE_DEPENDENTS

        GENERIC_WRITE = ACCESS_MASK.STANDARD_RIGHTS_WRITE Or
        SERVICE_CHANGE_CONFIG

        GENERIC_EXECUTE = ACCESS_MASK.STANDARD_RIGHTS_EXECUTE Or
            SERVICE_START Or
            SERVICE_STOP Or
            SERVICE_PAUSE_CONTINUE Or
            SERVICE_USER_DEFINED_CONTROL


        ACCESS_SYSTEM_SECURITY = ACCESS_MASK.ACCESS_SYSTEM_SECURITY
        DELETE = ACCESS_MASK.DELETE
        READ_CONTROL = ACCESS_MASK.READ_CONTROL
        WRITE_DAC = ACCESS_MASK.WRITE_DAC
        WRITE_OWNER = ACCESS_MASK.WRITE_OWNER
    End Enum

    <Flags()>
    Public Enum SERVICE_TYPE As Integer
        SERVICE_KERNEL_DRIVER = &H1
        SERVICE_FILE_SYSTEM_DRIVER = &H2
        SERVICE_WIN32_OWN_PROCESS = &H10
        SERVICE_WIN32_SHARE_PROCESS = &H20
        SERVICE_INTERACTIVE_PROCESS = &H100
    End Enum

    Public Enum SERVICE_START As Integer
        SERVICE_BOOT_START = &H0
        SERVICE_SYSTEM_START = &H1
        SERVICE_AUTO_START = &H2
        SERVICE_DEMAND_START = &H3
        SERVICE_DISABLED = &H4
    End Enum

    <Flags()>
    Public Enum SCM_ACCESS As Integer
        STANDARD_RIGHTS_REQUIRED = &HF0000
        SC_MANAGER_CONNECT = &H1
        SC_MANAGER_CREATE_SERVICE = &H2
        SC_MANAGER_ENUMERATE_SERVICE = &H4
        SC_MANAGER_LOCK = &H8
        SC_MANAGER_QUERY_LOCK_STATUS = &H10
        SC_MANAGER_MODIFY_BOOT_CONFIG = &H20
        SC_MANAGER_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED And SC_MANAGER_CONNECT And SC_MANAGER_CREATE_SERVICE And SC_MANAGER_ENUMERATE_SERVICE And SC_MANAGER_LOCK And SC_MANAGER_QUERY_LOCK_STATUS And SC_MANAGER_MODIFY_BOOT_CONFIG
    End Enum


    <DllImport("advapi32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Function CreateService(ByVal hSCManager As IntPtr, ByVal serviceName As String,
                ByVal displayName As String, ByVal desiredAccess As Integer, ByVal serviceType As Integer,
                ByVal startType As Integer, ByVal errorcontrol As Int32, ByVal binaryPathName As String,
                ByVal loadOrderGroup As String, ByVal TagBY As Int32, ByVal dependencides As String,
                ByVal serviceStartName As String, ByVal password As String) As IntPtr
    End Function

    <DllImport("advapi32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
    Private Function OpenSCManager(ByVal machineName As String, ByVal databaseName As String, ByVal desiredAccess As Int32) As IntPtr
    End Function

    <DllImport("advapi32.dll", SetLastError:=True)>
    Private Function CloseServiceHandle(ByVal serviceHandle As IntPtr) As Boolean
    End Function

    ''' <summary>
    ''' Create a Windows Service for the specified executable. The service will be configured
    ''' using the default settings. This is to mimic what would happen if the service was 
    ''' created using the sc.exe create command from an elevated command prompt.
    ''' </summary>
    ''' <param name="pathName">The path to the service executable (including arguments)</param>
    ''' <param name="serviceName">The name of the Windows Service</param>
    Public Sub CreateService(pathName As String, serviceName As String)

        Dim scHandle = IntPtr.Zero
        Dim serviceHandle = IntPtr.Zero

        Try
            scHandle = OpenSCManager(Nothing, Nothing, SCM_ACCESS.SC_MANAGER_CREATE_SERVICE)
            If scHandle.Equals(IntPtr.Zero) Then
                Throw New Win32Exception(Marshal.GetLastWin32Error())
            End If

            serviceHandle = CreateService(scHandle, serviceName, Nothing, SERVICE_ACCESS.SERVICE_CHANGE_CONFIG, SERVICE_TYPE.SERVICE_WIN32_OWN_PROCESS,
                        SERVICE_START.SERVICE_DEMAND_START, SERVICE_ERROR.SERVICE_ERROR_NORMAL, pathName, Nothing, 0, Nothing, Nothing, Nothing)

            If serviceHandle.Equals(IntPtr.Zero) Then
                Throw New Win32Exception(Marshal.GetLastWin32Error())
            End If

        Finally
            CloseServiceHandle(serviceHandle)
            CloseServiceHandle(scHandle)
        End Try

    End Sub

End Module
