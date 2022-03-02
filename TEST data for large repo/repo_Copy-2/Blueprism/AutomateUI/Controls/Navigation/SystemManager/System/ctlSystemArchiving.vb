Imports System.IO
Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.clsServer
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.Core.Resources

Public Class ctlSystemArchiving
    Implements IStubbornChild, IPermission, IHelp, IMenuButtonHandler

    ''' <summary>
    ''' Stores the index of the Archiving tab so it can be removed and put back in
    ''' the right place.
    ''' </summary>
    Private mArchivingTabIndex As Integer

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mArchivingTabIndex = tcSystem.Controls.IndexOf(tabArchiving)

    End Sub

    ''' <summary>
    ''' Show the correct tab for archiving depending on the mode.
    ''' </summary>
    ''' <param name="auto">True to show auto-archiving UI; False to show manual
    ''' archiving UI.</param>
    Private Sub ShowCorrectArchivingTab(ByVal auto As Boolean)
        If auto Then
            PopulateArchivingAuto()
            tcSystem.TabPages.Remove(tabArchiving)
            If Not tcSystem.Controls.Contains(tabArchivingAuto) Then
                tcSystem.TabPages.Insert(mArchivingTabIndex, tabArchivingAuto)
            End If
        Else
            tcSystem.TabPages.Remove(tabArchivingAuto)
            If Not tcSystem.TabPages.Contains(tabArchiving) Then
                tcSystem.TabPages.Insert(mArchivingTabIndex, tabArchiving)
            End If
        End If
    End Sub

    Private Sub ctlSystemArchiving_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If DesignMode Then Return
        ShowCorrectArchivingTab(gSv.IsAutoArchiving())

        ' Add release lock option to hamburger menu
        Dim sysman = GetAncestor(Of ctlSystemManager)()
        If sysman IsNot Nothing Then sysman.MenuButtonContextMenuStrip = btnMenuStrip
    End Sub

    ''' <summary>
    ''' Populate the auto-archiving stuff.
    ''' </summary>
    Private Sub PopulateArchivingAuto()

        Dim cur As Guid
        Dim folder As String = Nothing
        Dim age As String = Nothing
        Dim delete As Boolean
        gSv.GetAutoArchivingSettings(cur, folder, age, delete)
        cmbArchiveResource.Items.Clear()
        cmbArchiveResource.Items.Add(My.Resources.ctlSystemArchiving_None)
        Dim dt As DataTable = gSv.GetResources(ResourceAttribute.None, ResourceAttribute.Local Or ResourceAttribute.Retired Or ResourceAttribute.Debug, Nothing)
        Dim selindex As Integer = 0
        For Each dr As DataRow In dt.Rows
            Dim index As Integer = cmbArchiveResource.Items.Add(dr("Name"))
            If CType(dr("ResourceID"), Guid) = cur Then
                selindex = index
            End If
        Next
        cmbArchiveResource.SelectedIndex = selindex
        cmbArchiveMode.SelectedIndex = CInt(IIf(delete, 1, 0))
        txtArchiveFolder.Text = folder
        txtArchiveFolder.Enabled = CBool(IIf(cmbArchiveMode.SelectedIndex = 0, True, False))
        btnBrowse.Enabled = CBool(IIf(cmbArchiveMode.SelectedIndex = 0, True, False))
        Select Case age.Substring(age.Length - 1)
            Case "d"
                cmbArchiveAgeUnit.SelectedIndex = 0
            Case "w"
                cmbArchiveAgeUnit.SelectedIndex = 1
            Case "m"
                cmbArchiveAgeUnit.SelectedIndex = 2
            Case "y"
                cmbArchiveAgeUnit.SelectedIndex = 3
        End Select
        txtArchiveAgeNum.Text = age.Substring(0, age.Length - 1)
        btnUpdateSettings.Enabled = False
        btnUndoChanges.Enabled = False

    End Sub

    Private Sub btnSwitchToManual_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSwitchToManual.Click
        If Not CanLeave() Then Return
        gSv.SetAutoArchiving(False)
        ShowCorrectArchivingTab(False)
        tcSystem.SelectedTab = tabArchiving
    End Sub

    Private Sub btnSwitchToAuto_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSwitchToAuto.Click
        gSv.SetAutoArchiving(True)
        ShowCorrectArchivingTab(True)
        tcSystem.SelectedTab = tabArchivingAuto
    End Sub

    Private Sub btnUpdateSettings_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnUpdateSettings.Click
        Dim resourceName As String = cmbArchiveResource.Text
        Dim resid As Guid = gSv.GetResourceId(resourceName)
        Dim age As String = txtArchiveAgeNum.Text
        Select Case cmbArchiveAgeUnit.SelectedIndex
            Case 0
                age &= "d"
            Case 1
                age &= "w"
            Case 2
                age &= "m"
            Case 3
                age &= "y"
        End Select

        Dim delete As Boolean = cmbArchiveMode.SelectedIndex = 1

        Dim archiveFolder As String = txtArchiveFolder.Text
        'Validate archive folder if appropriate (only if the auto-archive will be done
        'on the same machine we're configuring on, and not if delete is selected)...
        If resourceName = ResourceMachine.GetName() Then
            If Not Directory.Exists(archiveFolder) AndAlso Not delete Then
                UserMessage.Show(String.Format(My.Resources.ctlSystemArchiving_TheSpecifiedArchiveFolder0DoesNotExist, txtArchiveFolder.Text))
                Exit Sub
            End If
        End If
        gSv.SetAutoArchivingSettings(resid, archiveFolder, age, delete, resourceName)
        btnUpdateSettings.Enabled = False
        btnUndoChanges.Enabled = False
    End Sub

    Private Sub btnUndoChanges_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUndoChanges.Click
        PopulateArchivingAuto()
    End Sub

    Private Sub cmbArchiveResource_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbArchiveResource.SelectedIndexChanged
        AutoArchiveSettingsChanged()
    End Sub

    Private Sub cmbArchiveMode_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbArchiveMode.SelectedIndexChanged
        AutoArchiveSettingsChanged()
    End Sub

    Private Sub txtArchiveFolder_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtArchiveFolder.TextChanged
        AutoArchiveSettingsChanged()
    End Sub

    Private Sub txtArchiveAgeNum_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtArchiveAgeNum.TextChanged
        AutoArchiveSettingsChanged()
    End Sub

    Private Sub cmbArchiveAgeUnit_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbArchiveAgeUnit.SelectedIndexChanged
        AutoArchiveSettingsChanged()
    End Sub

    ''' <summary>
    ''' Handles the hamburger context menu opening
    ''' </summary>
    Private Sub HandleContextMenuOpening() Handles btnMenuStrip.Opening
        ReleaseToolStripMenuItem.Enabled = gSv.IsArchiveLockSet(Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Handles the Release Archive Lock option selected
    ''' </summary>
    Private Sub HandleReleaseArchiveLock(sender As Object, e As EventArgs) Handles ReleaseToolStripMenuItem.Click
        Dim resource = String.Empty
        Dim lastUpdated As DateTime
        If Not gSv.IsArchiveLockSet(resource, lastUpdated) Then Exit Sub
        If UserMessage.OkCancel(
          My.Resources.ctlSystemArchiving_Resources.WarningMsg_ReleaseLock_Template,
          resource, lastUpdated) = MsgBoxResult.Ok Then
            gSv.ReleaseArchiveLock(True)
        End If
    End Sub

    Private Sub AutoArchiveSettingsChanged()
        btnUpdateSettings.Enabled = True
        btnUndoChanges.Enabled = True
        txtArchiveFolder.Enabled = CBool(IIf(cmbArchiveMode.SelectedIndex = 0, True, False))
        btnBrowse.Enabled = CBool(IIf(cmbArchiveMode.SelectedIndex = 0, True, False))
    End Sub

    Private Sub btnBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowse.Click

        If cmbArchiveResource.Text <> ResourceMachine.GetName() Then
            UserMessage.Show(My.Resources.ctlSystemArchiving_BecauseTheResourceYouHaveSelectedIsNotOnThisMachineThePathYouBrowseMayNotExistO)
        End If

        Dim ofd As New FolderBrowserDialog()
        If Directory.Exists(txtArchiveFolder.Text) Then
            ofd.SelectedPath = txtArchiveFolder.Text
        Else
            ofd.RootFolder = Environment.SpecialFolder.Desktop
        End If
        If ofd.ShowDialog() = DialogResult.OK Then
            txtArchiveFolder.Text = ofd.SelectedPath
        End If
    End Sub

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
            ctlArchivingInterface1.ParentAppForm = mParent
        End Set
    End Property

    ''' <summary>
    ''' You can't leave if you don't save your auto-archiving changes, or you've
    ''' changed some calendar data and not saved it.
    ''' </summary>
    Private Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        If btnUpdateSettings.Enabled Then
            UserMessage.Show(My.Resources.ctlSystemArchiving_YouNeedToSaveOrUndoYourAutoArchivingChangesBeforeNavigatingAway)
            Return False
        End If
        Return True
    End Function

    Public ReadOnly Property RequiredPermissions() As System.Collections.Generic.ICollection(Of BluePrism.AutomateAppCore.Auth.Permission) Implements BluePrism.AutomateAppCore.Auth.IPermission.RequiredPermissions
        Get
            Return Permission.ByName("System - Archiving")
        End Get
    End Property

    ''' <summary>
    ''' The menu strip to use for the menu button
    ''' </summary>
    Public ReadOnly Property MenuStrip As ContextMenuStrip Implements IMenuButtonHandler.MenuStrip
        Get
            Return btnMenuStrip
        End Get
    End Property

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "helpArchiving.htm"
    End Function

End Class
