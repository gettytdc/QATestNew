
Imports System.IO
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Config
Imports BluePrism.Core.Utility

''' Project  : Automate
''' Class    : Branding
''' 
''' <summary>
''' Branding and related user interface stuff. This is here to centralise stuff that
''' would otherwise be scattered and repeated throughout the code, and also to allow
''' overriding by licensing, etc.
''' </summary>
Public Class Branding

    ''' <summary>
    ''' Margins for the large logo, when placed on the welcome screen, etc.
    ''' </summary>
    Public Const LargeLogoMarginRight As Integer = 10
    Public Const LargeLogoMarginBottom As Integer = 1

    Private ReadOnly mOptions As IOptions

    Public Sub New(options As IOptions)
        mOptions = options
    End Sub

    ''' <summary>
    ''' Get the title for the main application form.
    ''' </summary>
    ''' <returns>The title.</returns>
    Public Function GetTitle() As String

        Dim title = GetOverrideTitle()
        If (title Is Nothing) Then title = GetDefaultTitle()

        Dim version = Me.GetBluePrismVersion(fieldCount:=3)
        Return $"{title} - v{version}"

    End Function

    Private Function GetOverrideTitle() As String

        Dim overrideTitle As String = GetLicenseTitleOverride()
        If overrideTitle IsNot Nothing Then
            mOptions.LastBrandingTitle = overrideTitle
            Return overrideTitle
        End If

        'If there's no license, allow override by the last previously used
        'branding.
        If Not IsLicensed() Then
            Dim lastBrandingTitle = mOptions.LastBrandingTitle
            If lastBrandingTitle IsNot Nothing Then Return lastBrandingTitle
        End If

        'Otherwise, maybe we want unbranded?
        If mOptions.Unbranded Then
            Return GetUnbrandedTitle()
        End If

        Return Nothing

    End Function

    Public Overridable Function GetDefaultTitle() As String
        Return String.Format(My.Resources.AppName0RoboticProcessAutomationSoftware,
                             ApplicationProperties.ApplicationName)
    End Function

    Public Overridable Function GetUnbrandedTitle() As String
        Return My.Resources.Automate
    End Function

    Public Overridable Function GetLicenseTitleOverride() As String
        Return Licensing.License.OverrideTitle()
    End Function

    Public Overridable Function IsLicensed() As Boolean
        Return Licensing.License.IsLicensed
    End Function

    ''' <summary>
    ''' Get the icon, if it is to be overridden.
    ''' </summary>
    ''' <returns>A System.Drawing.Image.</returns>
    Public Shared Function GetIcon() As Icon
        Dim configOptions = Options.Instance
        'Allow override by license key...
        Dim over As String = Licensing.License.OverrideIcon()
        If over IsNot Nothing Then
            configOptions.LastBrandingIcon = over
        End If

        'If there's no license, allow override by the last previously used
        'branding.
        If over Is Nothing AndAlso Not Licensing.License.IsLicensed Then
            over = configOptions.LastBrandingIcon
        End If

        'Otherwise, maybe we want unbranded?
        If over Is Nothing AndAlso configOptions.Unbranded Then
            Dim ii As Icon = CType(GetType(Form).
                        GetProperty("DefaultIcon", System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.Static).GetValue(Nothing, Nothing), Icon)
            Using s As New MemoryStream()
                ii.Save(s)
                over = Convert.ToBase64String(s.ToArray())
            End Using
        End If

        If over IsNot Nothing Then
            Try
                Using s As New MemoryStream(Convert.FromBase64String(over))
                    Return New Icon(s)
                End Using
            Catch
                'If something goes wrong here, just don't override the logo. The
                'alternative is repeatedly popping up annoying messages.
            End Try
        End If

        'Otherwise, no override
        Return Nothing

    End Function


    ''' <summary>
    ''' Get the large product logo, as used on the welcome screen (for example).
    ''' </summary>
    ''' <returns>A System.Drawing.Image, or Nothing to not have a logo.</returns>
    Public Shared Function GetLargeLogo() As Image

        Dim configOptions = Options.Instance

        'Allow override by license key...
        Dim over As String = Licensing.License.OverrideLargeLogo()
        If over IsNot Nothing Then
            configOptions.LastBrandingLargeLogo = over
        End If

        'If there's no license, allow override by the last previously used
        'branding.
        If over Is Nothing AndAlso Not Licensing.License.IsLicensed Then
            over = configOptions.LastBrandingLargeLogo
        End If

        If over IsNot Nothing Then
            Try
                Using s As New MemoryStream(Convert.FromBase64String(over))
                    Return DirectCast(Image.FromStream(s), Bitmap)
                End Using
            Catch
                'If something goes wrong here, just don't override the logo. The
                'alternative is repeatedly popping up annoying messages.
            End Try
        End If

        'Otherwise, maybe we want unbranded?
        If configOptions.Unbranded Then
            Return Nothing
        End If

        'Otherwise, the standard embedded logo.
        Return LogoImages.blueprism_s

    End Function


End Class

