Imports System.Windows.Forms
Imports AutomateControls
Imports BluePrism.Server.Domain.Models
Imports Microsoft.Win32
Imports System.Net

Namespace Utility

    Public Module OfflineHelpHelper
        Private Const BluePrismWebsite As String = "https://www.blueprism.com/"
        Private Const BluePrismKey As String = "SOFTWARE\Blue Prism Limited\Automate"
        Private Const InstallDir As String = "InstallDir"
        Private Const HelpConnectionsHtml = "helpConnections.htm"
        Private Const AcknowledgementsOnlineFile = "acknowledgements.htm"
        Private mHelpConnectionsUrl As String = Nothing
        Private mAcknowledgementLocation As String = Nothing

        Public Sub OpenHelpFile(control As Control, helpFileName As String)
            If HasDisplayedOfflineDatabaseHelp(control) = False Then
                HelpLauncher.ShowTopic(control, helpFileName, If(IsOfflineHelpEnabled(), gSv.GetOfflineHelpBaseUrl, Nothing))
            End If
        End Sub

        Public Sub OpenHelpFileSearch(control As Control)
            If HasDisplayedOfflineDatabaseHelp(control) = False Then
                HelpLauncher.ShowSearch(control, If(IsOfflineHelpEnabled(), gSv.GetOfflineHelpBaseUrl, Nothing))
            End If
        End Sub

        Public Sub OpenHelpFileHome(control As Control)
            If HasDisplayedOfflineDatabaseHelp(control) = False Then
                HelpLauncher.ShowContents(control, If(IsOfflineHelpEnabled(), gSv.GetOfflineHelpBaseUrl, Nothing))
            End If
        End Sub

        Public Sub OpenAcknowledgements()
            HelpLauncher.ShowAcknowledgements(AcknowledgementsOnlineFile, If(IsOfflineHelpEnabled(), gSv.GetOfflineHelpBaseUrl, Nothing))
        End Sub

        Private Sub OpenDatabaseOfflineHelpFile(control As Control)
            HelpLauncher.ShowTopic(control, HelpConnectionsHtml, GetDatabaseOfflineHelpUrl())
        End Sub

        Private Function IsOfflineHelpEnabled() As Boolean
            Try
                Return Auth.User.LoggedIn AndAlso gSv.OfflineHelpEnabled
            Catch ex As PermissionException
                Return False
            End Try
        End Function

        Private Function GetKey(view As RegistryView) As String
            Dim openKey As RegistryKey = Nothing
            Dim installLocation As String = Nothing

            Using key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view)
                openKey = key.OpenSubKey(BluePrismKey)
                If openKey IsNot Nothing Then
                    installLocation = CStr(openKey.GetValue(InstallDir))
                End If
            End Using

            openKey?.Close()

            Return installLocation
        End Function


        Private Function GetInstallBase() As String

            Dim installLocation As String = Nothing

            Try
                Dim view = RegistryView.Registry64                                             'x64 or x32 on respective platforms
                installLocation = GetKey(view)
            Catch
            End Try

            If installLocation Is Nothing Then
                Try
                    Dim view = RegistryView.Registry32                                             'wow6432node on x64
                    installLocation = GetKey(view)
                Catch
                End Try
            End If

            Return installLocation
        End Function

        Private Function GetDatabaseOfflineHelpUrl() As String

            If mHelpConnectionsUrl IsNot Nothing Then Return mHelpConnectionsUrl

            Dim installBase As String = GetInstallBase()

            If installBase IsNot Nothing Then
                mHelpConnectionsUrl = installBase
            End If

            Return mHelpConnectionsUrl

        End Function

        Private Function GetAcknowledgementsOfflineLocation() As String

            If mAcknowledgementLocation IsNot Nothing Then Return mAcknowledgementLocation

            Dim installBase As String = GetInstallBase()

            If installBase IsNot Nothing Then
                mAcknowledgementLocation = installBase
            End If

            Return mAcknowledgementLocation

        End Function

        Private Function IsBluePrismWebsiteReachable() As Boolean
            Try
                Using client = New WebClient()
                    Using client.OpenRead(BluePrismWebsite)
                        Return True
                    End Using
                End Using
            Catch
            End Try

            Return False
        End Function

        Private Function HasDisplayedOfflineDatabaseHelp(control As Control) As Boolean
            Try
                If Auth.User.LoggedIn = False AndAlso IsBluePrismWebsiteReachable() = False Then
                    OpenDatabaseOfflineHelpFile(control)
                    Return True
                End If
            Catch
            End Try

            Return False
        End Function

    End Module

End Namespace
