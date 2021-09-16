Imports System.Collections.Concurrent
Imports System.Runtime.InteropServices
Imports BluePrism.AutomateAppCore.Auth

Public Class ImportListener
    Inherits MarshalByRefObject

    Private Shared mFilePath As String
    Private Const SW_RESTORE As Integer = &H9

    <DllImport("user32.dll", EntryPoint:="FindWindowW")> _
    Private Shared Function FindWindowW(<MarshalAs(UnmanagedType.LPTStr)> ByVal lpClassName As String, <MarshalAs(UnmanagedType.LPTStr)> ByVal lpWindowName As String) As IntPtr
    End Function

    <DllImport("user32.dll", EntryPoint:="IsWindowVisible")> _
    Private Shared Function IsWindowVisible(ByVal hWnd As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("user32.dll", EntryPoint:="ShowWindow")> _
    Private Shared Function ShowWindow(ByVal hWnd As IntPtr, ByVal nCmdShow As Integer) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("user32.dll", EntryPoint:="IsIconic")> _
    Private Shared Function IsIconic(ByVal hWnd As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("user32.dll", EntryPoint:="SetForegroundWindow")> _
    Private Shared Function SetForegroundWindow(ByVal hWnd As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    Public Sub ImportProcess(filePath As String)

        mFilePath = filePath

        Try
            Activate_Instance()
        Catch
            'No need to catch
        End try
       
        If Not User.LoggedIn Then
            If (Not FilesToImport.FileQueue.Any(Function(f) f = mFilePath)) Then
                FilesToImport.FileQueue.Enqueue(mFilePath)
                gMainForm?.DisplayPopup(My.Resources.PleaseLogInToImport_Description)
            End If
            mFilePath = String.Empty
            Throw New InvalidOperationException("User not logged in.")

        ElseIf Not User.Current.HasPermissionToImportFile(mFilePath) Then
            gMainForm?.DisplayPopup(string.Format(My.Resources.InsufficientPermissions_DescriptionFile0, filePath.GetBluePrismFileTypeName, filePath))
            mFilePath = String.Empty
            Throw New InvalidOperationException("Insufficient Permissions")
        End If

        FilesToImport.FileQueue.Enqueue(mFilePath)
        FilesToImport.FilesInBatch += 1

        If FilesToImport.FilesInBatch = 1 Then gMainForm?.StartImportThread()
        mFilePath = String.Empty
    End Sub
    Private Sub Activate_Instance()
        Dim hwnd As IntPtr = FindWindowW(Nothing, gMainForm.Text) 'Find the window handle (Works even if the app is hidden and not shown in taskbar)
        If Not IsWindowVisible(hwnd) Or IsIconic(hwnd) Then 'If the window is minimized or hidden then Restore and Show the window
            ShowWindow(hwnd, SW_RESTORE)
        End If

        'toggle topMost for our Main form, then set it back as it was
        Dim topMostValue = gMainForm.TopMost
        If gMainForm.TopMost Then gMainForm.TopMost = False
        gMainForm.TopMost = True
        gMainForm.TopMost = topMostValue

        SetForegroundWindow(hwnd) 'Set the window as the foreground window
    End Sub

End Class

Class FilesToImport
    Public Shared Property FileQueue As New ConcurrentQueue(Of String)
    Public Shared Property FilesInBatch As Integer
End Class
