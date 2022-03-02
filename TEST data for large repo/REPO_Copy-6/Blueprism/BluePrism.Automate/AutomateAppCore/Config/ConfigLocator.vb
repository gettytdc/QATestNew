Imports System.IO
Imports Microsoft.Win32

<Serializable>
Public Class ConfigLocator : Implements IConfigLocator

    Private Enum Locations
        User
        Machine
        RegistryDefined
    End Enum

    ''' <summary>
    ''' Returns the instance of the ConfigLocator
    ''' </summary>
    ''' <param name="userSpecific">When no registry key exists ensure the config is
    ''' loaded from the default user specific location</param>
    ''' <returns></returns>
    Public Shared Function Instance(Optional userSpecific As Boolean = False) As IConfigLocator
        If mInstance Is Nothing Then
            If ConfigLocationExists Then
                mInstance = New ConfigLocator(Locations.RegistryDefined)
            ElseIf userSpecific Then
                mInstance = New ConfigLocator(Locations.User)
            Else
                mInstance = New ConfigLocator(Locations.Machine)
            End If
        End If
        Return mInstance
    End Function
    Private Shared mInstance As IConfigLocator

    Private Sub New(location As Locations)
        mLocation = location
    End Sub

    Private mLocation As Locations

    ''' <summary>
    ''' The special folder representing the folder in which the config file backing
    ''' this object will be / is stored.
    ''' </summary>
    Private Function SpecialFolder(location As Locations) As Environment.SpecialFolder
        ' Get the special folder to use - APPDATA for user options; COMMONAPPDATA otherwise.
        Select Case location
            Case Locations.User
                Return Environment.SpecialFolder.ApplicationData
            Case Else
                Return Environment.SpecialFolder.CommonApplicationData
        End Select
    End Function

    Private Function DefaultDirectory() As String
        Return DefaultDirectory(mLocation)
    End Function

    Private Function DefaultDirectory(location As Locations) As String
        Return Path.Combine(Environment.GetFolderPath(SpecialFolder(location)), "Blue Prism Limited", "Automate V3")
    End Function

    Private ReadOnly Property RegistryDefinedDirectory As String
        Get
            Using key = CurrentUser(ConfigLocationKey)
                If key Is Nothing Then Return DefaultDirectory()
                Return CStr(key.GetValue(ConfigLocationName, DefaultDirectory()))
            End Using
        End Get
    End Property

    ''' <summary>
    ''' The directory into which the file representing this config is saved
    ''' </summary>
    Public ReadOnly Property MachineConfigDirectory As DirectoryInfo Implements IConfigLocator.MachineConfigDirectory
        Get
            If mMachineConfigDirectory Is Nothing Then
                Select Case mLocation
                    Case Locations.User, Locations.Machine
                        mMachineConfigDirectory = New DirectoryInfo(DefaultDirectory)
                    Case Else
                        mMachineConfigDirectory = New DirectoryInfo(RegistryDefinedDirectory)
                End Select
            End If
            Return mMachineConfigDirectory
        End Get
    End Property

    Private mMachineConfigDirectory As DirectoryInfo

    Public ReadOnly Property UserConfigDirectory As DirectoryInfo Implements IConfigLocator.UserConfigDirectory
        Get
            If mUserConfigDirectory Is Nothing Then
                Select Case mLocation
                    Case Locations.RegistryDefined
                        mUserConfigDirectory = New DirectoryInfo(RegistryDefinedDirectory)
                    Case Else
                        mUserConfigDirectory = New DirectoryInfo(DefaultDirectory(Locations.User))
                End Select
            End If
            Return mUserConfigDirectory
        End Get
    End Property

    Private mUserConfigDirectory As DirectoryInfo

    Private Const ConfigLocationKey = "SOFTWARE\Blue Prism Limited\Automate"

    Private Const ConfigLocationName = "ConfigLocation"

    Public Shared ReadOnly Property ConfigLocationExists() As Boolean
        Get
            Using key = CurrentUser(ConfigLocationKey)
                If key Is Nothing Then Return False
                If Not key.GetValueNames().Contains(ConfigLocationName) Then Return False
                Dim keyValue = CStr(key.GetValue(ConfigLocationName))
                Return Not String.IsNullOrEmpty(keyValue)
            End Using
        End Get
    End Property

    Private Shared Function CurrentUser(subKey As String) As RegistryKey
        Try
            Dim view = RegistryView.Registry64 'x64 or x32 on respective platforms
            Using key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view)
                Dim openKey = key.OpenSubKey(subKey)
                If openKey IsNot Nothing Then Return openKey
            End Using
        Catch
        End Try

        Try
            Dim view = RegistryView.Registry32 'wow6432node on x64
            Using key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view)
                Dim openKey = key.OpenSubKey(subKey)
                If openKey IsNot Nothing Then Return openKey
            End Using
        Catch
        End Try

        Return Nothing
    End Function

    Public ReadOnly Property LocationTypeName As String Implements IConfigLocator.LocationTypeName
        Get
            Select Case mLocation
                Case Locations.User
                    Return My.Resources.ConfigLocatorUserSpecific
                Case Locations.Machine
                    Return My.Resources.ConfigLocatorMachineWide
                Case Else
                    Return My.Resources.ConfigLocatorRegistryDefined
            End Select
        End Get
    End Property
End Class
