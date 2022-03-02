Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.InteropServices
Imports System.Security.Principal
Imports BluePrism.Server.Domain.Models
Imports Microsoft.Win32

''' <summary>
''' Helper class to aid in checking on the User Access Control status of the current
''' running process.
''' </summary>
Public NotInheritable Class UacHelper

    ' The registry key in which the UAC status can be found.
    Private Const UacRegistryKey As String =
     "Software\Microsoft\Windows\CurrentVersion\Policies\System"

    ' The registry value to look for to discover the UAC status
    Private Const UacRegistryValue As String = "EnableLUA"

    ' Flag indicating that standard rights are required from the process token
    Private Const STANDARD_RIGHTS_READ As UInteger = &H20000
    ' Flag indicating that an access token query is required from the process token
    Private Const TOKEN_QUERY As UInteger = &H8
    ' Mask combining the 2 required token access flags
    Private Const TOKEN_READ As UInteger = (STANDARD_RIGHTS_READ Or TOKEN_QUERY)

    ''' <summary>
    ''' Opens the access token associated with the running process
    ''' </summary>
    ''' <param name="ProcessHandle">A handle to the process for which the token is
    ''' required.</param>
    ''' <param name="DesiredAccess">The access desired from the token</param>
    ''' <param name="TokenHandle">After a successful call, contains a handle to the
    ''' access token associated with the specified process.</param>
    ''' <returns>True on a successful call; False otherwise - LastError is set on an
    ''' unsuccessful call</returns>
    <DllImport("advapi32.dll", SetLastError:=True)>
    Private Shared Function OpenProcessToken(
     ByVal ProcessHandle As IntPtr, ByVal DesiredAccess As UInt32, ByRef TokenHandle As IntPtr) _
     As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    ''' <summary>
    ''' Retrieves specific information about an access token
    ''' </summary>
    ''' <param name="TokenHandle">Handle to the token for which specifiec information
    ''' is required.</param>
    ''' <param name="TokenInformationClass">The type of information required.</param>
    ''' <param name="TokenInformation">Pointer to buffer in which the output
    ''' information is written on a successful call.</param>
    ''' <param name="TokenInformationLength">The size in bytes of of the buffer
    ''' pointed to by <paramref name="TokenInformation"/>.</param>
    ''' <param name="ReturnLength">The number of bytes needed for the buffer defined
    ''' in <paramref name="TokenInformation"/>. If this value is larger than the
    ''' value specified in <paramref name="TokenInformationLength"/> the call will
    ''' fail and nothing is stored in the buffer.</param>
    ''' <returns>True on a successful call; False otherwise.</returns>
    <DllImport("advapi32.dll", SetLastError:=True), CLSCompliant(False)>
    Public Shared Function GetTokenInformation(
     ByVal TokenHandle As IntPtr, ByVal TokenInformationClass As TOKEN_INFORMATION_CLASS,
     ByVal TokenInformation As IntPtr, ByVal TokenInformationLength As UInteger,
     ByRef ReturnLength As UInteger) As Boolean
    End Function

    ''' <summary>
    ''' Enumeration of the information types that can be requested from a process
    ''' access token
    ''' </summary>
    Public Enum TOKEN_INFORMATION_CLASS
        TokenUser = 1
        TokenGroups
        TokenPrivileges
        TokenOwner
        TokenPrimaryGroup
        TokenDefaultDacl
        TokenSource
        TokenType
        TokenImpersonationLevel
        TokenStatistics
        TokenRestrictedSids
        TokenSessionId
        TokenGroupsAndPrivileges
        TokenSessionReference
        TokenSandBoxInert
        TokenAuditPolicy
        TokenOrigin
        TokenElevationType
        TokenLinkedToken
        TokenElevation
        TokenHasRestrictions
        TokenAccessInformation
        TokenVirtualizationAllowed
        TokenVirtualizationEnabled
        TokenIntegrityLevel
        TokenUIAccess
        TokenMandatoryPolicy
        TokenLogonSid
        MaxTokenInfoClass
    End Enum

    ''' <summary>
    ''' Enumeration of the elevation types available in the process access token
    ''' </summary>
    Public Enum TOKEN_ELEVATION_TYPE
        TokenElevationTypeDefault = 1
        TokenElevationTypeFull
        TokenElevationTypeLimited
    End Enum

    ''' <summary>
    ''' Checks if UAC is enabled in the current process.
    ''' This will always return false for operating systems earlier than Vista.
    ''' </summary>
    Public Shared ReadOnly Property IsUacEnabled() As Boolean
        Get
            Dim key As RegistryKey = Registry.LocalMachine.OpenSubKey(UacRegistryKey, False)
            Return Object.Equals(key.GetValue(UacRegistryValue), 1)
        End Get
    End Property

    ''' <summary>
    ''' Checks if this process is elevated or not - it is elevated if it has
    ''' requested an elevation token (for UAC-enabled Vista or Win7 systems) or, if
    ''' UAC is disabled or unavailable and the user is in an Administrator role.
    ''' </summary>
    Public Shared ReadOnly Property IsProcessElevated() As Boolean
        Get
            ' If UAC is not enabled, check if the current user is in an administrator role
            If Not IsUacEnabled Then
                Dim identity As WindowsIdentity = WindowsIdentity.GetCurrent()
                Dim principal As New WindowsPrincipal(identity)
                Dim result As Boolean = principal.IsInRole(WindowsBuiltInRole.Administrator)
                Return result
            End If
            ' Otherwise check the elevation state of the process token
            Dim tokenHandle As IntPtr
            If Not OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_READ, tokenHandle) Then
                Throw New BluePrismException(
                 My.Resources.UacHelper_CouldNotGetProcessTokenWin32ErrorCode & Marshal.GetLastWin32Error())
            End If

            Dim elevationResult As TOKEN_ELEVATION_TYPE =
             TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault

            Dim elevationResultSize As Integer = Marshal.SizeOf(CInt(elevationResult))
            Dim returnedSize As UInteger = 0
            Dim elevationTypePtr As IntPtr = Marshal.AllocHGlobal(elevationResultSize)

            Dim success As Boolean = GetTokenInformation(tokenHandle,
             TOKEN_INFORMATION_CLASS.TokenElevationType, elevationTypePtr,
             CUInt(elevationResultSize), returnedSize)

            If Not success Then Throw New InvalidOperationException(
             My.Resources.UacHelper_UnableToDetermineTheCurrentElevation)

            Return (Marshal.ReadInt32(elevationTypePtr) =
             TOKEN_ELEVATION_TYPE.TokenElevationTypeFull)

        End Get
    End Property

    '''' <summary>
    '''' Checks if the user has write access to the given directory path.
    '''' </summary>
    '''' <param name="dirPath">The directory path to check if the current process has
    '''' write access to or not</param>
    '''' <returns>True if the current process can safely write to the given directory,
    '''' false otherwise.</returns>
    '''' <remarks>This value should be cached where possible - or at least not called
    '''' repeatedly within a loop, since it is not the most performant test ever.
    '''' </remarks>
    'Public Shared Function HasWriteAccessTo(ByVal dirPath As String) As Boolean
    '   ' FIXME: .NET 4 required before UACHelper.HasWriteAccessTo() can work
    '   ' This would be really nice to have, but it's ridiculously complicated
    '   ' in pre-.net 4 development. No little shortcuts worked, so I've disabled
    '   ' the whole thing and mummified it in greencode to be opened in 2076 when
    '   ' we upgrade to .net 4
    '   'Dim perms As New PermissionSet(PermissionState.None)
    '   'perms.AddPermission(New FileIOPermission(FileIOPermissionAccess.Write, dirPath))
    '   'Return perms.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet)
    'End Function


    Public Shared Function GetUacShield(size As Integer) As Image
        Dim image As Image

        Dim shield = SystemIcons.Shield.ToBitmap()
        shield.MakeTransparent()
        Dim g As Graphics
        image = New Bitmap(size, size)

        g = Graphics.FromImage(image)

        g.CompositingMode = CompositingMode.SourceOver
        g.DrawImage(shield, New Rectangle(0, 0, size, size))
        Return image
    End Function
End Class
