Imports AutomateControls
Imports BluePrism.AMI
Imports BluePrism.CharMatching.UI
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore
Imports BluePrism.ApplicationManager.AMI

Public Class ctlSystemFonts : Implements IChild, IHelp, IPermission

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        fontMgr.Store = New clsAutomateFontStore()
        fontMgr.LoadFromStore()
    End Sub

    ''' <summary>
    ''' Handles a spy request from the font manager by calling on AMI to spy a bitmap
    ''' from a window on the operating system desktop.
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="e">The request event args - the spied image is returned in these
    ''' args if the spy was not cancelled by the user.</param>
    Private Sub fontMgr_SpyRequested( _
     ByVal sender As Object, ByVal e As SpyRequestEventArgs) _
     Handles fontMgr.SpyRequested
        Using ami As New clsAMI(New clsGlobalInfo)
            e.SpiedImage = ami.SpyBitmap()
        End Using
    End Sub

    Private Sub FindReferences(sender As Object, e As EventArgs) Handles fontMgr.ReferencesRequested
        If fontMgr.SelectedFontNames.Count <> 1 Then
            UserMessage.Show(My.Resources.ctlSystemFonts_PleaseSelectASingleFont)
            Return
        End If
        Dim fonts As New List(Of String)(fontMgr.SelectedFontNames)
        mParent.FindReferences(New clsProcessFontDependency(fonts(0)))
    End Sub

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public ReadOnly Property RequiredPermissions() As System.Collections.Generic.ICollection(Of BluePrism.AutomateAppCore.Auth.Permission) Implements BluePrism.AutomateAppCore.Auth.IPermission.RequiredPermissions
        Get
            Return Permission.ByName("System - Fonts")
        End Get
    End Property

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "helpFonts.htm"
    End Function
End Class
