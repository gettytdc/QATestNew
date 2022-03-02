Imports System.Runtime.InteropServices
Imports Microsoft.Win32.SafeHandles
Imports System.IO
Imports System.Text
Imports System.Collections.Generic
Imports System.Management
Imports System.Data


Public Class BluePrismLoginAgent : Implements IDisposable

#Region " Global Code "

    <DllImport("user32.dll")> _
    Public Shared Function LockWorkStation() As Boolean
    End Function

    <DllImport("user32.dll")> _
    Public Shared Sub ExitWindowsEx(ByVal uFlags As Integer, ByVal dwReason As Integer)
    End Sub

    <DllImport("kernel32.dll", SetLastError:=True)> _
     Public Shared Function CreateFile( _
      ByVal pipeName As String, _
      ByVal dwDesiredAccess As UInt32, _
      ByVal dwShareMode As UInt32, _
      ByVal lpSecurityAttributes As IntPtr, _
      ByVal dwCreationDisposition As UInt32, _
      ByVal dwFlagsAndAttributes As UInt32, _
      ByVal hTemplate As IntPtr) As SafeFileHandle
    End Function

    Public Const GENERIC_READ As UInt32 = &H80000000UI ' "UI" suffix = unsigned int
    Public Const GENERIC_WRITE As UInt32 = &H40000000
    Public Const OPEN_EXISTING As UInt32 = 3
    Public Const FILE_FLAG_OVERLAPPED As UInt32 = &H40000000

    Private Const PipeName As String = "BluePrismCredentialProviderPipe"

    Public Enum ExitWindowsFlags
        LogOff = &H0
        ShutDown = &H1
        Reboot = &H2
        PowerOff = &H8
        RestartApps = &H40

        Force = &H4
        ForceIfHung = &H10
    End Enum

    Public Sub New()
        AddHandler Microsoft.Win32.SystemEvents.SessionSwitch, AddressOf SessionSwitch
    End Sub

    Private m_Locked As Boolean

    Public Sub SessionSwitch(ByVal Sender As Object, ByVal e As Microsoft.Win32.SessionSwitchEventArgs)
        Select Case e.Reason
            Case Microsoft.Win32.SessionSwitchReason.SessionLock
                m_Locked = True
            Case Microsoft.Win32.SessionSwitchReason.SessionUnlock
                m_Locked = False
        End Select
    End Sub

    Public Sub ExitWindowsDelay(ByVal o As Object)
        Dim args As List(Of Object) = DirectCast(o, List(Of Object))
        Dim Flags As Integer = DirectCast(args(0), Integer)
        Dim Delay As TimeSpan = DirectCast(args(1), TimeSpan)
        System.Threading.Thread.Sleep(Delay)
        ExitWindowsEx(Flags, 0)
    End Sub

    Public Overloads Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overloads Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            RemoveHandler Microsoft.Win32.SystemEvents.SessionSwitch, AddressOf SessionSwitch
        End If
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
    End Sub

#End Region

#Region " Log In "

    Public Sub Login(ByVal username As String, ByVal password As String)

        Using h As SafeFileHandle = CreateFile( _
         PipeName, _
         GENERIC_READ Or GENERIC_WRITE, _
         0, _
         IntPtr.Zero, _
         OPEN_EXISTING, _
         FILE_FLAG_OVERLAPPED, _
         IntPtr.Zero)

            If h.IsInvalid Then Throw New Exception("Named pipe handle is invalid")

            Using fs As New FileStream(h, FileAccess.ReadWrite)
                Dim sw As New StreamWriter(fs, Encoding.UTF8)
                sw.Write("{0}{3}{1}{3}{2}", _
                 Environment.UserDomainName, username, password, vbLf)
                sw.Flush()
            End Using

        End Using

    End Sub

#End Region

#Region " Log Off "

    Public Sub Log_Off(ByVal force As Boolean, ByVal Delay As TimeSpan)
        Dim Flags As ExitWindowsFlags = ExitWindowsFlags.LogOff
        If force Then
            Flags = Flags And ExitWindowsFlags.Force
        End If
        Dim t As New Threading.Thread(AddressOf ExitWindowsDelay)
        Dim args As New List(Of Object)
        args.Add(Flags)
        args.Add(Delay)
        t.Start(args)
    End Sub

#End Region

#Region " Change Password "

    Public Sub Change_Password(ByVal oldPass As String, ByVal newPass As String)
        Try
            Dim e As New System.DirectoryServices.DirectoryEntry(String.Format("WinNT://{0}/{1},User", Environment.UserDomainName, Environment.UserName))
            e.Invoke("ChangePassword", oldPass, newPass)
        Catch ex As System.Reflection.TargetInvocationException
            Throw ex.InnerException
        End Try
    End Sub

#End Region

#Region " Lock Screen "

    Public Sub Lock_Screen()
        LockWorkStation()
    End Sub

#End Region

#Region " Unlock Screen "

    Public Sub Get_Username(ByRef Username As String)
        Username = Environment.UserName
    End Sub

#End Region

#Region " Is Locked "

    Public Sub Locked_(ByRef Locked As Boolean)
        Locked = m_Locked
    End Sub

#End Region


    Public Function GetLoggedInUsers() As ICollection(Of String)
        Dim users As New List(Of String)

        Dim options As New ConnectionOptions()
        Dim scope As New ManagementScope("\\localhost", options)

        Dim query As New ObjectQuery("select * from Win32_ComputerSystem")
        Dim searcher As New ManagementObjectSearcher(scope, query)
        Dim results As ManagementObjectCollection = searcher.[Get]()

        For Each result As ManagementObject In results
            Dim username As String = TryCast(result("UserName"), String)
            If username IsNot Nothing Then
                users.Add(username)
            End If
        Next

        Return users
    End Function

#Region "Is Logged In"

    Public Sub Is_Logged_In(ByRef Logged_In As Boolean)
        Logged_In = (GetLoggedInUsers().Count > 0)
    End Sub

#End Region

#Region "Logged In Users"

    Public Sub Logged_In_Users(ByRef Users As DataTable)
        Users = New DataTable()
        Const userNameCol As String = "Name"
        Users.Columns.Add(userNameCol)
        For Each username As String In GetLoggedInUsers()
            Dim row As DataRow = Users.NewRow()
            row(userNameCol) = username
            Users.Rows.Add(row)
        Next
    End Sub

#End Region

End Class
