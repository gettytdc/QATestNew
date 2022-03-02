Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports System.Drawing
Imports BluePrism.BPCoreLib

Public Class clsGlobalMouseHook
    Implements IDisposable

    ''' <summary>
    ''' Signature for the callback required by the windows
    ''' API for the mouse hooking procedure.
    ''' </summary>
    Private Delegate Function HookProc(ByVal nCode As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As Integer

    ''' <summary>
    ''' A reference to the callback method used in the
    ''' mouse hooking.
    ''' </summary>
    Private mMouseHookProcedure As HookProc

    ''' <summary>
    ''' Holds a reference to the hooking pointer
    ''' </summary>
    Private mhHook As IntPtr

    ''' <summary>
    ''' Event raised when the mouse is moved across the screen.
    ''' </summary>
    Public Event MouseMoved(ByVal location As Point)

    ''' <summary>
    ''' Event raised when a mouse button is pressed.
    ''' </summary>
    Public Event MouseDown(ByVal g As clsGlobalMouseEventArgs)

    ''' <summary>
    ''' Event raised when a mouse button is released.
    ''' </summary>
    Public Event MouseUp(ByVal g As clsGlobalMouseEventArgs)

    ''' <summary>
    ''' Constructor
    ''' </summary>
    Public Sub New()
        Me.HookMouse()
    End Sub


    ''' <summary>
    ''' Creates the mouse hook
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub HookMouse()
        Try
            If mhHook.Equals(IntPtr.Zero) Then
                mMouseHookProcedure = New HookProc(AddressOf Me.MouseHookProc)
                Dim h As IntPtr = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName)
                mhHook = SetWindowsHookEx(HookTypes.WH_MOUSE_LL, mMouseHookProcedure, h, 0)
                If mhHook.Equals(IntPtr.Zero) Then
                    Throw New System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error)
                End If
            End If

        Catch ex As Exception
        End Try
    End Sub

    ''' <summary>
    ''' Removes the mouse hook
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub UnHookMouse()
        Try
            If Not mhHook.Equals(IntPtr.Zero) Then
                If UnhookWindowsHookEx(mhHook) = 0 Then
                    'If the function fails, the return value is zero. 
                    Throw New System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error)
                End If
            End If
        Catch ex As Exception
        End Try
        GC.Collect()
        mhHook = IntPtr.Zero
    End Sub


    ''' <summary>
    ''' This procedure is called when there is an event captured by the mouse hook
    ''' </summary>
    ''' <param name="nCode"></param>
    ''' <param name="wParam"></param>
    ''' <param name="lParam"></param>
    ''' <returns></returns>
    Private Function MouseHookProc(ByVal nCode As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As Integer
        If nCode >= 0 Then

            Dim s As MouseHookStruct = CType(Marshal.PtrToStructure(lParam, GetType(MouseHookStruct)), MouseHookStruct)
            Dim up As Boolean
            Dim b As MouseButtons
            Select Case wParam.ToInt32
                Case WindowMessages.WM_MOUSEMOVE
                    RaiseEvent MouseMoved(s.pt)
                    Return CallNextHookEx(mhHook, nCode, wParam, lParam)

                Case WindowMessages.WM_LBUTTONDOWN
                    up = False
                    b = MouseButtons.Left
                Case WindowMessages.WM_RBUTTONDOWN
                    up = False
                    b = MouseButtons.Right
                Case WindowMessages.WM_LBUTTONUP
                    up = True
                    b = MouseButtons.Left
                Case WindowMessages.WM_RBUTTONUP
                    up = True
                    b = MouseButtons.Right
                Case Else
                    Return CallNextHookEx(mhHook, nCode, wParam, lParam)
            End Select

            Dim g As New clsGlobalMouseEventArgs(b, s.pt)
            If up Then
                RaiseEvent MouseUp(g)
            Else
                RaiseEvent MouseDown(g)
            End If

            If g.Cancel Then
                Return 1
            Else
                Return CallNextHookEx(mhHook, nCode, wParam, lParam)
            End If
        Else
            Return CallNextHookEx(mhHook, nCode, wParam, lParam)
        End If
    End Function

#Region " IDisposable Support "

    Private mDisposedValue As Boolean = False ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not mDisposedValue Then
            UnHookMouse()
        End If
        mDisposedValue = True
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

Public Class clsGlobalMouseEventArgs
    Public Sub New(ByVal buttons As MouseButtons, ByVal loc As System.Drawing.Point)
        Button = buttons
        Location = loc
        Cancel = False
    End Sub

    Public Button As MouseButtons
    Public Location As System.Drawing.Point
    Public Cancel As Boolean                    'Set to True to prevent the button press
    'being passed on to the next event handler.

End Class
