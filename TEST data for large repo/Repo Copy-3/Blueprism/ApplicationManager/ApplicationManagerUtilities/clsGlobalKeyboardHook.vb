Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections

''' Project  : ApplicationManagerUtilities
''' Class    : clsGlobalKeyboardHook
''' 
''' <summary>
''' Keyboard hook intended for use with the spy tool.
''' </summary>
Public Class clsGlobalKeyboardHook
    Implements IDisposable

#Region " Class Scope Declarations "

    ''' <summary>
    ''' The keys which we treat as ALT keys in the hooked procedure
    ''' </summary>
    ''' <remarks>Not sure why the cursor keys are considered ALT keys but they were
    ''' in Win32 Hooks.cpp so they are here...
    ''' Note also that <see cref="HookedKeys"/> contains all of the AltKeys as well
    ''' to ensure that they are processed by the application</remarks>
    Private Shared AltKeys As IBPSet(Of Keys) = GetReadOnly.ISetFrom( _
     Keys.Up, Keys.Down, Keys.Left, Keys.Right, _
     Keys.Alt, Keys.LMenu, Keys.RMenu, Keys.Menu _
    )

    ''' <summary>
    ''' The keys that we hook into and pass onto to the Blue Prism listener/spytool
    ''' </summary>
    Private Shared HookedKeys As IBPSet(Of Keys) = GetReadOnly.ISetFrom( _
     Keys.Up, Keys.Down, Keys.Left, Keys.Right, _
     Keys.Alt, Keys.LMenu, Keys.RMenu, Keys.Menu, _
     Keys.Control, Keys.LControlKey, Keys.RControlKey _
    )

#Region " Native Imports / Definitions "

    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
    Private Shared Function SetWindowsHookEx( _
     ByVal hookType As HookType, ByVal lpfn As HookProc, ByVal hMod As IntPtr, _
     ByVal dwThreadId As UInteger) As IntPtr
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
    Public Shared Function UnhookWindowsHookEx(ByVal hhk As IntPtr) _
     As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
    Private Shared Function CallNextHookEx(ByVal hhk As IntPtr, _
     ByVal nCode As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    End Function

    <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
    Public Shared Function GetModuleHandle(ByVal lpModuleName As String) As IntPtr
    End Function

    Private Enum HookType As Integer
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

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure KBDLLHOOKSTRUCT
        Public vkCode As UInt32
        Public scanCode As UInt32
        Public flags As KBDLLHOOKSTRUCTFlags
        Public time As UInt32
        Public dwExtraInfo As UIntPtr
    End Structure

    <Flags()> _
    Public Enum KBDLLHOOKSTRUCTFlags As UInt32
        LLKHF_EXTENDED = &H1
        LLKHF_INJECTED = &H10
        LLKHF_ALTDOWN = &H20
        LLKHF_UP = &H80
    End Enum

    Private Delegate Function HookProc(ByVal code As Integer, _
     ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr

#End Region

#End Region

#Region " Published Events "

    ''' <summary>
    ''' Event raised when a key is depressed. Not all key events will be raised
    ''' - only those of interest to the spy tool.
    ''' </summary>
    ''' <param name="e">Details of the key event.</param>
    Public Event KeyDown As GlobalKeyEventHandler

    ''' <summary>
    ''' Event raised when a key is released. Not all key events will be raised
    ''' - only those of interest to the spy tool.
    ''' </summary>
    ''' <param name="e">Details of the key event.</param>
    Public Event KeyUp As GlobalKeyEventHandler

#End Region

#Region " Member Variables "

    ' The callback method for the keyboard hook
    Private mProcMethod As HookProc

    ' The handle pointing to the hook applied to the keyboard
    Private mHookHandle As IntPtr

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates and attaches a new global keyboard hook
    ''' </summary>
    Public Sub New()
        ' We store this in a member to ensure that the garbage collector does not
        ' attempt to clean it up while the hook is still in scope.
        mProcMethod = AddressOf HandleHookCall

        '' Hooks the keyboard, storing the handle so that we can unhook on disposal
        mHookHandle = HookKeyboard()
    End Sub

#End Region

#Region " Methods "

    ''' <summary>
    ''' Attaches a low level keyboard hook within windows, using the
    ''' <see cref="HandleHookCall"/> method as the callback.
    ''' </summary>
    ''' <returns>The keyboard hook handle created after successfully hooking the
    ''' keyboard; Zero indicates a failure to hook the keyboard.</returns>
    Private Function HookKeyboard() As IntPtr
        Using curProcess As Process = Process.GetCurrentProcess()
            Using curModule As ProcessModule = curProcess.MainModule
                Return SetWindowsHookEx(HookType.WH_KEYBOARD_LL, _
                 mProcMethod, GetModuleHandle(curModule.ModuleName), 0)
            End Using
        End Using

    End Function

    ''' <summary>
    ''' Raises the <see cref="KeyDown"/> event with the given arguments.
    ''' </summary>
    Protected Overridable Sub OnKeyDown(ByVal e As GlobalKeyEventArgs)
        RaiseEvent KeyDown(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="KeyUp"/> event with the given arguments.
    ''' </summary>
    Protected Overridable Sub OnKeyUp(ByVal e As GlobalKeyEventArgs)
        RaiseEvent KeyUp(Me, e)
    End Sub

    ''' <summary>
    ''' Handles the keyboard hooking callback
    ''' </summary>
    ''' <param name="nCode">A code indicating if this call should be processed or
    ''' just immediately passed on by this method - if the value is negative it
    ''' should be passed on, otherwise it can be processed</param>
    ''' <param name="wParam">The WPARAM structure of the message - this will contain
    ''' the Windows Message that is being fired.</param>
    ''' <param name="lParam">The LPARAM structure of the message - this is a pointer
    ''' to a KBDLLHOOKSTRUCT structure</param>
    ''' <returns>An IntPtr value of zero if the message is to be returned and
    ''' processed by the target application, or non-zero if it has been consumed by
    ''' this method.</returns>
    Private Function HandleHookCall(ByVal nCode As Integer, _
     ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
        If nCode >= 0 Then
            Dim kbs As KBDLLHOOKSTRUCT = _
             BPUtil.PtrToStructure(Of KBDLLHOOKSTRUCT)(lParam)

            Dim handler As Action(Of GlobalKeyEventArgs) = Nothing
            Select Case CType(wParam, WindowMessages)
                Case WindowMessages.WM_KEYDOWN, WindowMessages.WM_SYSKEYDOWN
                    handler = AddressOf OnKeyDown
                Case WindowMessages.WM_KEYUP, WindowMessages.WM_SYSKEYUP
                    handler = AddressOf OnKeyUp
            End Select

            Dim vk As Keys = CType(kbs.vkCode, Keys)

            If handler IsNot Nothing AndAlso HookedKeys.Contains(vk) Then
                ' Invoke in a BG thread to avoid clogging up the global hook
                handler.BeginInvoke(New GlobalKeyEventArgs(vk), Nothing, Nothing)
                ' We allow ALT keys back to the application; we trap control keys
                ' with us so that we don't cause things to happen in the target
                ' app while the user is trying to communicate with us
                If AltKeys.Contains(vk) _
                 Then Return IntPtr.Zero _
                 Else Return New IntPtr(1)
            End If

        End If
        Return CallNextHookEx(mHookHandle, nCode, wParam, lParam)

    End Function

#End Region

#Region " Dispose Handling "

    Private disposedValue As Boolean = False ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If mHookHandle <> IntPtr.Zero Then UnhookWindowsHookEx(mHookHandle)
            mHookHandle = IntPtr.Zero
        End If
        disposedValue = True
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

#End Region

End Class

