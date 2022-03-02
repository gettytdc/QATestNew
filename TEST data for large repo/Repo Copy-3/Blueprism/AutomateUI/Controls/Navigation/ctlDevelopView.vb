
Imports System.IO
Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.BPCoreLib
Imports ProcessEventCode = BluePrism.AutomateAppCore.ProcessEventCode
Imports WizardType = AutomateUI.frmWizard.WizardType

''' <summary>
''' Control used to display the 'Develop' view in the client.
''' </summary>
Public Class ctlDevelopView
    Implements IPermission, IChild, IHelp, IEnvironmentColourManager, IRefreshable, IStubbornChild

#Region " Member Variables "

    ' The frmApplication instance ultimately holding this control
    Private WithEvents AppForm As frmApplication

    ' The store used by this view for groups access
    Private mStore As IGroupStore

    ' The background colour of the environment
    Private mEnvBackColor As Color = ColourScheme.Default.EnvironmentBackColor

    ' The foreground colour of the environment
    Private mEnvForeColor As Color = ColourScheme.Default.EnvironmentForeColor

    ' The "View" context menu item
    Private WithEvents ViewItemMenuItem As ToolStripMenuItem = New ToolStripMenuItem()

    ' The "Edit" context menu item 
    Private WithEvents EditItemMenuItem As ToolStripMenuItem = New ToolStripMenuItem()

    ' Menu item to select wheter unrestricted but empty parent groups are shown.
    Private WithEvents ShowAllUnRestrictedMenuItem As ToolStripMenuItem = New ToolStripMenuItem()

    ' Flag to remember if we are showing all unrestricted groups or not
    Private mShowAllUnRestrictedGroups As Boolean = False


#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new DevelopView control
    ''' </summary>
    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ViewItemMenuItem.Text = My.Resources.ctlDevelopView_View
        EditItemMenuItem.Text = My.Resources.ctlDevelopView_Edit
        ShowAllUnRestrictedMenuItem.Text = My.Resources.AutomateUI_Controls.ShowAllUnrestrictedGroups
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Sets the frmApplication in this control
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return AppForm
        End Get
        Set(value As frmApplication)
            AppForm = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the environment-specific back colour in use in this environment.
    ''' Only set to the database-held values after login.
    ''' </summary>
    ''' <remarks>Note that this only affects the UI owned directly by this form - ie.
    ''' setting the colour here will not update the database</remarks>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color _
     Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return mEnvBackColor
        End Get
        Set(value As Color)
            If value = mEnvBackColor Then Return
            mEnvBackColor = value
            Dim envMgr = TryCast(DetailPanel, IEnvironmentColourManager)
            If envMgr IsNot Nothing Then envMgr.EnvironmentBackColor = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the environment-specific fore colour in use in this environment.
    ''' Only set to the database-held values after login.
    ''' </summary>
    ''' <remarks>Note that this only affects the UI owned directly by this form - ie.
    ''' setting the colour here will not update the database</remarks>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
     Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return mEnvForeColor
        End Get
        Set(value As Color)
            If value = mEnvForeColor Then Return
            mEnvForeColor = value
            Dim envMgr = TryCast(DetailPanel, IEnvironmentColourManager)
            If envMgr IsNot Nothing Then envMgr.EnvironmentForeColor = value
        End Set
    End Property

    ''' <summary>
    ''' The permissions, any of which are required in order to enter this form
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property RequiredPermissions As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("Process Studio", "Object Studio")
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the detail panel held in this view control
    ''' </summary>
    Private Property DetailPanel As Control
        Get
            With splitPanel.Panel2.Controls
                If .Count = 0 Then Return Nothing
                Return .Item(0)
            End With
        End Get
        Set(value As Control)
            Dim currPanel As Control = DetailPanel
            If value Is currPanel Then Return
            RemoveEventListenersFromDetailControl(currPanel)
            With splitPanel.Panel2.Controls
                .Clear()
                If value IsNot Nothing Then
                    value.Dock = DockStyle.Fill
                    Dim envMgr = TryCast(value, IEnvironmentColourManager)
                    If envMgr IsNot Nothing Then
                        envMgr.EnvironmentBackColor = mEnvBackColor
                        envMgr.EnvironmentForeColor = mEnvForeColor
                    End If
                    .Add(value)
                    AddEventListenersToDetailControl(value)
                End If
            End With
        End Set
    End Property

#End Region

#Region " Methods "

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        Return gtGroups.CanLeave()
    End Function

    ''' <summary>
    ''' Handles an import being completed as reported by the application form hosting
    ''' this develop view control
    ''' </summary>
    Private Sub HandleImportCompleted(sender As Object, e As EventArgs) _
     Handles AppForm.ImportCompleted
        'Clear tree cache 
        gtGroups.ClearGroupCache()
        'refresh view through an invoke to avoid cross thread issues
        BeginInvoke(New FunctionDelegate(AddressOf RefreshView))
    End Sub

    ''' <summary>
    ''' Adds any necessary event listeners to the provided detail panel
    ''' </summary>
    ''' <param name="ctl">The detail control to which the appropriate event listeners
    ''' should be added.</param>
    Private Sub AddEventListenersToDetailControl(ctl As Control)
        Dim gdp = TryCast(ctl, GroupDetailPanel)
        If gdp IsNot Nothing Then
            AddHandler gdp.RefreshRequested,
                AddressOf HandleRefreshRequested
            AddHandler gdp.GroupMemberCreateRequested,
                AddressOf HandleGroupMemberCreateRequestedFromDetailPanel
            AddHandler gdp.GroupMemberCompareRequested,
                AddressOf HandleGroupMemberCompareRequested
            AddHandler gdp.GroupMemberActivated,
                AddressOf HandleGroupMemberActivated
            AddHandler gdp.GroupMemberPreview,
                AddressOf HandleGroupMemberPreview
            AddHandler gdp.GroupMemberContextMenuOpening,
                AddressOf HandleDetailPanelGroupMemberContextMenuOpening
            AddHandler gdp.GroupMemberDeleteRequested,
                AddressOf HandleGroupMemberDeleteRequested
            AddHandler gdp.GroupMemberUnlockRequested,
                AddressOf HandleGroupMemberUnlockRequested
            AddHandler gdp.GroupContentsChanged,
                AddressOf HandleGroupContentsChanged
        End If
        Dim pdp = TryCast(ctl, ProcessDetailPanel)
        If pdp IsNot Nothing Then
            AddHandler pdp.GroupMemberActivated,
                AddressOf HandleGroupMemberActivated
            AddHandler pdp.GroupMemberDeleteRequested,
                AddressOf HandleGroupMemberDeleteRequested
            AddHandler pdp.GroupMemberPreview,
                AddressOf HandleGroupMemberPreview
            AddHandler pdp.RefreshRequested,
                AddressOf HandleRefreshRequested

        End If
    End Sub

    ''' <summary>
    ''' Removes any necessary event listeners from the provided detail panel
    ''' </summary>
    ''' <param name="ctl">The detail control to which the appropriate event listeners
    ''' should be added.</param>
    Private Sub RemoveEventListenersFromDetailControl(ctl As Control)
        Dim gdp = TryCast(ctl, GroupDetailPanel)
        If gdp IsNot Nothing Then
            RemoveHandler gdp.RefreshRequested,
                AddressOf HandleRefreshRequested
            RemoveHandler gdp.GroupMemberCreateRequested,
                AddressOf HandleGroupMemberCreateRequestedFromDetailPanel
            RemoveHandler gdp.GroupMemberCompareRequested,
                AddressOf HandleGroupMemberCompareRequested
            RemoveHandler gdp.GroupMemberActivated,
                AddressOf HandleGroupMemberActivated
            RemoveHandler gdp.GroupMemberPreview,
                AddressOf HandleGroupMemberPreview
            RemoveHandler gdp.GroupMemberContextMenuOpening,
                AddressOf HandleDetailPanelGroupMemberContextMenuOpening
            RemoveHandler gdp.GroupMemberDeleteRequested,
                AddressOf HandleGroupMemberDeleteRequested
            RemoveHandler gdp.GroupMemberUnlockRequested,
                AddressOf HandleGroupMemberUnlockRequested
            RemoveHandler gdp.GroupContentsChanged,
                AddressOf HandleGroupContentsChanged


        End If
        Dim pdp = TryCast(ctl, ProcessDetailPanel)
        If pdp IsNot Nothing Then
            RemoveHandler pdp.GroupMemberActivated,
                AddressOf HandleGroupMemberActivated
            RemoveHandler pdp.GroupMemberDeleteRequested,
                AddressOf HandleGroupMemberDeleteRequested
        End If
    End Sub

    ''' <summary>
    ''' Handles the loading of this control, loading the trees it should be
    ''' displaying from the store set within the control.
    ''' </summary>
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Dim store As IGroupStore = GetGroupStore()
        ' Populate trees - note that forcing a reload is necessary to avoid showing stale data
        ' ctlDevelopView coordinates refreshing of cached tree data as instances are saved / unlocked
        ' but this doesn't happen while the form is closed
        gtGroups.ExtraContextMenuItems.Add(ViewItemMenuItem)
        gtGroups.ExtraContextMenuItems.Add(EditItemMenuItem)
        gtGroups.ExtraContextMenuItems.Add(ShowAllUnRestrictedMenuItem)
        ShowAllUnRestrictedMenuItem.Checked = mShowAllUnRestrictedGroups

        gtGroups.ManageAccessRightsEnabled = True
        gtGroups.ValidateMemberMovements = True

        LoadProcessTree(store)
        LoadObjectTree(store)

        ' Register with any open frmProcess instances
        For Each frm In frmProcess.GetAllInstances()
            AddHandlers(frm)
        Next

    End Sub

    ''' <summary>
    ''' Gets the help file associated with this control.
    ''' Er, 'frmProcessStudio.htm', apparently.
    ''' </summary>
    ''' <returns>The help file to display to help with this control</returns>
    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "frmProcessStudio.htm"
    End Function

    ''' <summary>
    ''' Handles a context menu opening on the group tree control. This ensures that
    ''' any create menu items are disabled if the user doesn't have permission to
    ''' create the item in question.
    ''' </summary>
    Private Sub HandleGroupMemberContextMenuOpening(
     sender As Object, e As GroupMemberContexMenuOpeningEventArgs) _
     Handles gtGroups.ContextMenuOpening
        Dim treeDef = e.Target.Tree.TreeType.GetTreeDefinition()
        For Each item In e.ContextMenu.Items.OfType(Of CreateGroupMemberMenuItem)()
            Dim gp As IGroup
            If gtGroups.SelectedGroup IsNot Nothing Then
                gp = gtGroups.SelectedGroup
            ElseIf gtGroups.SelectedMember IsNot Nothing Then
                gp = gtGroups.SelectedMembersGroup
            Else
                Continue For
            End If

            If gp.IsRoot AndAlso gp.Tree.HasDefaultGroup Then
                gp = gp.Tree.DefaultGroup
            End If
            item.Enabled = If(gp IsNot Nothing, gp.Permissions.HasPermission(User.Current, treeDef.CreateItemPermission), False)
        Next

        ' Enable, or disable View and Edit menu items on the context menu based on users permissions.
        ' If the selected group member is not a process or object (ie. it it's a group) then hide these menu items

        If e.Target.MemberType = GroupMemberType.Process OrElse e.Target.MemberType = GroupMemberType.Object Then
            Dim processGroupMember = CType(e.Target, ProcessBackedGroupMember)
            ViewItemMenuItem.Enabled = e.Target.Permissions.HasPermission(User.Current, GetImpliedViewPermission(processGroupMember))
            ViewItemMenuItem.Visible = True
            EditItemMenuItem.Enabled = e.Target.Permissions.HasPermission(User.Current, GetImpliedEditPermission(processGroupMember))
            EditItemMenuItem.Visible = True
        Else
            ViewItemMenuItem.Visible = False
            EditItemMenuItem.Visible = False
        End If


    End Sub

    ''' <summary>
    ''' Handles a group being selected in the group tree
    ''' </summary>
    Private Sub HandleGroupSelected() Handles gtGroups.GroupSelected
        Try
            Dim itemsWithoutMeta As Dictionary(Of Guid, IMemberMetaData) = gtGroups.SelectedGroup?.
                OfType(Of IMemberMetaData).
                Where(Function(x) Not x.HasMetaData).
                ToDictionary(Function(k) DirectCast(k, IGroupMember).IdAsGuid, Function(v) v)

            If itemsWithoutMeta?.Any Then
                Dim processMeta = gSv.GetProcessMetaInfo(itemsWithoutMeta.Keys.ToArray)
                processMeta.ToList().ForEach(Sub(i) itemsWithoutMeta(i.ProcessId).UpdateMetaInfo(i))
            End If
        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.GetProcessMetaDataError)
        End Try

        DetailPanel = New GroupDetailPanel() With {
            .DisplayedGroup = gtGroups.SelectedGroup,
            .DefaultGroup = gtGroups.FirstGroup
            }
    End Sub

    ''' <summary>
    ''' Handles an item being selected in the group tree
    ''' </summary>
    Private Sub HandleItemSelected() Handles gtGroups.ItemSelected
        Try
            Dim mem = TryCast(gtGroups.SelectedMember, ProcessBackedGroupMember)

            If mem Is Nothing Then Throw New InvalidCastException($"Unable to cast {NameOf(gtGroups.SelectedMember)} to {GetType(ProcessBackedGroupMember)}")
            If Not mem.HasMetaData Then
                mem.UpdateMetaInfo(gSv.GetProcessMetaInfo({mem.IdAsGuid}).First())
            End If

            DetailPanel = New ProcessDetailPanel() With {
                .ProcessMember = mem
            }
        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.GetProcessMetaDataError)
        Finally
            RefreshViewPanel()
        End Try
    End Sub

    ''' <summary>
    ''' Force a redraw of the view and the panel.
    ''' </summary>
    Private Sub RefreshViewHandle() Handles gtGroups.RefreshView
        RefreshViewPanel()
        RefreshView()
    End Sub

    ''' <summary>
    ''' Handles a 'Create Item' being requested from within the groups tree.
    ''' </summary>
    Private Sub HandleGroupMemberCreateRequested(sender As Object, e As CreateGroupMemberEventArgs) _
     Handles gtGroups.CreateRequested
        e.CreatedItem = Create(e.Type, If(e.TargetGroup IsNot Nothing, e.TargetGroup.IdAsGuid(), Guid.Empty))
    End Sub

    Private Sub HandleGroupMemberCreateRequestedFromDetailPanel(sender As Object, e As CreateGroupMemberEventArgs)
        gtGroups.UpdateSelectedMember()
        gtGroups.CreateNewItemInSelectedGroup(e.Type)
    End Sub


    ''' <summary>
    ''' Handles a compare members being requested from within the detail view.
    ''' </summary>
    Private Sub HandleGroupMemberCompareRequested(sender As Object, e As GroupMultipleMemberEventArgs)

        Try
            Dim selectedItems As ICollection(Of IGroupMember) = e.Members

            Dim processesToCompare = New List(Of ProcessBackedGroupMember)
            Dim fileName As String = Nothing

            ' Check correct number selected (ie. precisely 1)
            If selectedItems.Count = 1 Then
                processesToCompare.Add(TryCast(selectedItems(0), ProcessBackedGroupMember))

                Dim compareDialog As New frmProcessCompare(processesToCompare(0).ProcessType)
                compareDialog.SetEnvironmentColours(Me)
                compareDialog.ShowInTaskbar = False

                If compareDialog.ShowDialog() = DialogResult.OK Then
                    If compareDialog.ProcessLocation = FileOrDatabaseStage.ProcessLocationType.Database Then
                        processesToCompare.Add(compareDialog.SelectedMember)
                    Else
                        fileName = compareDialog.FileName
                    End If
                Else
                    Return
                End If

            ElseIf selectedItems.Count = 2 Then

                processesToCompare.AddRange(selectedItems.Select(Function(x) TryCast(x, ProcessBackedGroupMember)))

            Else
                UserMessage.Err(My.Resources.ctlDevelopView_PleaseChooseTwoAvailableHistoryEntriesToCompare)
                Return
            End If

            Dim canCompareProcess = Function(p As ProcessBackedGroupMember) _
                gSv.GetEffectiveMemberPermissionsForProcess(p.IdAsGuid).
                HasPermission(User.Current,
                    If(p.ProcessType = DiagramType.Process,
                    Permission.ProcessStudio.ImpliedViewProcess,
                    Permission.ObjectStudio.ImpliedViewBusinessObject)
                )

            If Not processesToCompare.All(canCompareProcess) Then
                UserMessage.ShowPermissionMessage()
                Return
            End If

            If fileName Is Nothing Then
                ParentAppForm.StartForm(
                    frmProcessComparison.FromGroupMembers(processesToCompare(0), processesToCompare(1)))
            Else
                ParentAppForm.StartForm(
                    frmProcessComparison.Fromfile(processesToCompare(0), New FileInfo(fileName)))
            End If

        Catch ex As Exception
            UserMessage.Err(
                ex, My.Resources.ctlDevelopView_ErrorAttemptingToCompareProcesses0, ex.Message)
        End Try
    End Sub


    ''' <summary>
    ''' Adds the handlers to the given process form which monitor the saving and
    ''' closing of the form in order to update the view of the controls in this
    ''' develop view.
    ''' </summary>
    ''' <param name="frm">The form to which the handlers should be added</param>
    Private Sub AddHandlers(frm As frmProcess)
        If frm Is Nothing Then Return
        ' Just make sure that we haven't already got handlers on there for this ctl
        RemoveHandlers(frm)
        AddHandler frm.Saved, AddressOf HandleProcessSaved
        AddHandler frm.FormClosed, AddressOf HandleProcessFormClosed
    End Sub

    ''' <summary>
    ''' Removes the handlers from the given process form which monitor the saving and
    ''' closing of the form in order to update the view of the controls.
    ''' </summary>
    ''' <param name="frm">The form from which the handlers should be removed.</param>
    Private Sub RemoveHandlers(frm As frmProcess)
        If frm Is Nothing Then Return
        RemoveHandler frm.Saved, AddressOf HandleProcessSaved
        RemoveHandler frm.FormClosed, AddressOf HandleProcessFormClosed
    End Sub



    ''' <summary>
    ''' Create a process or object
    ''' </summary>
    ''' <param name="type">Group member type to create</param>
    ''' <returns>The created member (or nothing if user aborted)</returns>
    Public Function Create(type As GroupMemberType, group As Guid) As IGroupMember
        Dim wt As frmWizard.WizardType = type.GetWizardType()

        ' Go through the 'create process' process.
        Using f As New frmProcessCreate(wt)
            f.SetEnvironmentColours(Me)
            f.ShowInTaskbar = False
            ' If the user cancelled, we abort
            Dim dialogResult = f.ShowDialog()

            If f.RefreshList AndAlso dialogResult = DialogResult.Cancel Then RefreshView()

            If dialogResult <> DialogResult.OK Then
                Return Nothing
            End If

            Try
                Dim processName As String = f.GetChosenProcessName
                Dim processDescription As String = f.GetChosenProcessDescription
                Dim processType = f.GetChosenProcessType

                Using proc As clsProcess = clsProcess.CreateProcess(
                                processType, Options.Instance.GetExternalObjectsInfo(),
                                processName, processDescription, User.CurrentName)
                    ' We have to do this in pieces because clsProcess is not
                    ' serializable
                    Dim xml = proc.GenerateXML(False)
                    Dim isVBO = (proc.ProcessType = DiagramType.Object)
                    gSv.CreateProcess(
                        proc.Id, proc.Name, proc.Version, proc.Description,
                        xml, False, isVBO, proc.GetDependencies(False), group)

                    ' If the attributes are set, ensure they go onto the db too.
                    If proc.Attributes <> ProcessAttributes.None Then _
                     gSv.SetProcessAttributes(proc.Id, proc.Attributes)

                    Dim mem As ProcessBackedGroupMember
                    If proc.ProcessType = DiagramType.Process _
                     Then mem = New ProcessGroupMember() _
                     Else mem = New ObjectGroupMember()

                    mem.Id = proc.Id
                    mem.Name = proc.Name
                    mem.Description = proc.Description
                    If f.OpenCreatedProcess Then
                        mem.Permissions = gSv.GetEffectiveMemberPermissions(mem)
                        OpenProcessInProcessStudio(mem)
                    End If

                    Return mem
                End Using

            Catch ex As Exception
                UserMessage.Err(ex, My.Resources.ctlDevelopView_FailedToCreateProcess0, ex.Message)
                Return Nothing

            End Try
        End Using

    End Function

    ''' <summary>
    ''' Handles refresh from the burger menu
    ''' </summary>
    Private Sub HandleRefreshRequested(sender As Object, e As EventArgs)
        RequestRefresh()
    End Sub

    Private Sub RequestRefresh()
        Dim reapplyFilter = False

        If gtGroups.IsFiltered Then
            gtGroups.ApplyEmptyFilter()
            reapplyFilter = True
        End If

        gtGroups?.Clear()
        Dim store As IGroupStore = GetGroupStore()
        LoadProcessTree(store)
        LoadObjectTree(store)

        If reapplyFilter Then
            gtGroups.RefreshTreeFilter()
        End If
    End Sub

    Private Sub RequestRefreshWithHistory()
        Dim sm = gtGroups.SelectedMember
        RequestRefresh()
        gtGroups.ClearGroupCache()
        'reselect where we where 
        If sm IsNot Nothing Then gtGroups.SelectedMember = sm
        Dim refresher = TryCast(DetailPanel, IRefreshable)
        If refresher IsNot Nothing Then refresher.RefreshView()
    End Sub


    ''' <summary>
    ''' Handles a process being saved in a frmProcess spawned from this control
    ''' </summary>
    Private Sub HandleProcessSaved(sender As Object, e As ProcessEventArgs)
        ' If this control is disposed, we want to ensure that it is removed from the
        ' form which is still sending it events...
        ' This shouldn't really happen (it removes itself from all frmProcess
        ' instances when it is disposed explicitly), but it can't hurt to check
        If IsDisposed Then
            RemoveHandlers(TryCast(sender, frmProcess))
        Else
            RequestRefreshWithHistory()
        End If
    End Sub

    ''' <summary>
    ''' Handles a process form being closed, ensuring that the view is refreshed when
    ''' it occurs
    ''' </summary>
    Private Sub HandleProcessFormClosed(sender As Object, e As FormClosedEventArgs)
        Dim frm = TryCast(sender, frmProcess)
        If frm Is Nothing Then Return
        Try
            Dim node = gtGroups.GetNodeFor(frm.GetProcessID())
            Dim processMember = TryCast(node?.Tag, ProcessBackedGroupMember)
            processMember?.UpdateLockStatus()
            If IsDisposed Then
                RemoveHandlers(TryCast(sender, frmProcess))
            Else
                RequestRefreshWithHistory()
            End If
        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.ctlDevelopView_ErrorClosingForm0, ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Invoke the refresh of the view, without refreshing the data store.
    ''' </summary>
    Private Sub RefreshViewPanel()
        Dim refresher = TryCast(DetailPanel, IRefreshable)
        refresher?.RefreshView()
    End Sub


    ''' <summary>
    ''' Refreshes the view.
    ''' </summary>
    Public Sub RefreshView() Implements IRefreshable.RefreshView
        gtGroups.ClearGroupCache()
        RefreshView(True)
    End Sub

    Private Delegate Sub FunctionDelegate()

    Private Sub RefreshView(reloadFromStore As Boolean)
        gtGroups.UpdateView(reloadFromStore)

        ' Once a unified model is in place, none of this should be necessary, bar
        ' the 'IRefreshable' call at the end.
        Dim gp = TryCast(gtGroups.SelectedMember, IGroup)
        Dim procMem = TryCast(gtGroups.SelectedMember, ProcessBackedGroupMember)
        If gp IsNot Nothing Then
            Dim pan = TryCast(DetailPanel, GroupDetailPanel)
            If pan IsNot Nothing Then pan.DisplayedGroup = gp

        ElseIf procMem IsNot Nothing Then
            Dim pan = TryCast(DetailPanel, ProcessDetailPanel)
            If pan IsNot Nothing Then pan.ProcessMember = procMem

        End If

        Dim refresher = TryCast(DetailPanel, IRefreshable)
        If refresher IsNot Nothing Then refresher.RefreshView()
    End Sub

    ''' <summary>
    ''' Handles a group member being activated by opening the given group member for
    ''' editing.
    ''' </summary>
    Private Sub HandleGroupMemberActivated(
     sender As Object, e As GroupMemberEventArgs) _
     Handles gtGroups.ItemActivated

        Dim procMem = TryCast(e.Target, ProcessBackedGroupMember)
        OpenProcessInProcessStudio(procMem)
    End Sub


    ''' <summary>
    ''' Handles a group member being previewed by opening the given group member for viewing
    ''' or debugging based on the user's permissions
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleGroupMemberPreview(
     sender As Object, e As GroupMemberEventArgs)

        Dim procMem = TryCast(e.Target, ProcessBackedGroupMember)
        If procMem Is Nothing Then Return

        Dim editMode As ProcessViewMode = GetLeastRestrictiveViewModeForPreview(procMem)
        OpenProcessInProcessStudio(procMem, editMode)

    End Sub


    ''' <summary>
    ''' Handles the Group Detail panel context menu opening.
    ''' Used to enable or disable certain menu items based on user permissions.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleDetailPanelGroupMemberContextMenuOpening(
        sender As Object, e As GroupMemberContexMenuOpeningEventArgs)

        Dim editMenuItem = e.ContextMenu.Items.Find("EditMenuItem", False)
        Dim viewMenuItem = e.ContextMenu.Items.Find("ViewMenuItem", False)
        Dim compareMenuItem = e.ContextMenu.Items.Find("CompareMenuItem", False)
        Dim compareToMenuItem = e.ContextMenu.Items.Find("CompareToMenuItem", False)

        If (editMenuItem.Count = 0 OrElse viewMenuItem.Count = 0 OrElse
                compareMenuItem.Count = 0 OrElse compareToMenuItem.Count = 0) Then
            Throw New ArgumentException(My.Resources.ctlDevelopView_ContextMenuDoesNotContainExpectedMenuItems)
        End If

        Dim processGroupMember = CType(e.Target, ProcessBackedGroupMember)
        editMenuItem(0).Enabled = e.Target.Permissions.HasPermission(User.Current, GetImpliedEditPermission(processGroupMember))
        viewMenuItem(0).Enabled = e.Target.Permissions.HasPermission(User.Current, GetImpliedViewPermission(processGroupMember))
        If Not e.Target.Permissions.HasPermission(User.Current, GetImpliedViewPermission(processGroupMember)) Then
            compareMenuItem(0).Enabled = False
            compareToMenuItem(0).Enabled = False
        End If
    End Sub


    Private Sub LoadProcessStudio(procmem As ProcessBackedGroupMember, editMode As ProcessViewMode)
        Dim wizardType As WizardType = procmem.MemberType.GetWizardType()
        Dim processId As Guid = procmem.IdAsGuid

        If Not CheckPermission(procmem, editMode) Then Return

        'If the process is already open in the appropriate mode then just focus it
        Dim frm As frmProcess = frmProcess.GetInstance(processId, editMode)
        If frm IsNot Nothing Then
            AddHandlers(frm)
            frm.Activate()
            Return
        End If

        Dim sErr As String = Nothing
        'If process is locked and/or an autosave object exists,
        'ask the user what they want to do with it
        If gSv.ProcessIsLocked(processId, Nothing, Nothing) OrElse gSv.AutoSaveBackupSessionExistsForProcess(processId) Then
            Dim f As New frmAutosavePrompt(processId, wizardType)
            f.SetEnvironmentColoursFromAncestor(Me)
            f.ShowInTaskbar = False
            If f.ShowDialog() <> DialogResult.OK Then
                'User has bailed out - do not open anything
                Return
            End If

            'User has chosen a course of action
            Select Case f.ChosenOutcome
                Case frmAutosavePrompt.OutComes.ViewProcess
                    If wizardType = frmWizard.WizardType.BusinessObject Then
                        editMode = ProcessViewMode.PreviewObject
                    Else
                        editMode = ProcessViewMode.PreviewProcess
                    End If
                    'Now allow to run through to below, where it is opened
                    'in this mode

                Case frmAutosavePrompt.OutComes.EditAutosaveVersion

                    'Recover autosaved version
                    Dim sBackupXML As String = Nothing
                    Try
                        gSv.AutoSaveGetBackupXML(processId, sBackupXML)
                    Catch ex As Exception
                        UserMessage.Show(String.Format(My.Resources.ctlDevelopView_FailedToRetrieveXmlInAutosavedBackupRecord0, ex.Message))
                        Return
                    End Try

                    Try
                        If f.UnlockRequested Then gSv.UnlockProcess(processId)
                    Catch ex As Exception
                        UserMessage.Err(ex,
                         String.Format(My.Resources.ctlDevelopView_ErrorWhilstUnlockingProcess0, ex.Message))
                        Return
                    End Try

                    If Not CheckPermission(procmem, editMode) Then Return

                    Dim procFrm As New frmProcess(editMode, sBackupXML, processId)
                    AddHandlers(procFrm)

                    AppForm.StartForm(procFrm)
                    Return

                Case frmAutosavePrompt.OutComes.CompareAutosaveVersions
                    If wizardType = frmWizard.WizardType.BusinessObject Then
                        editMode = ProcessViewMode.CompareObjects
                    Else
                        editMode = ProcessViewMode.CompareProcesses
                    End If

                    If Not CheckPermission(procmem, editMode) Then Return

                    'open the two versions in process comparison
                    Try
                        AppForm.StartForm(frmProcessComparison.FromBackup(processId))
                    Catch err As Exception
                        UserMessage.Err(My.Resources.ctlDevelopView_ErrorComparingBackupVersion, err)
                    End Try
                    Return

                Case frmAutosavePrompt.OutComes.EditOriginalVersion, frmAutosavePrompt.OutComes.EditProcess
                    Try
                        If f.UnlockRequested Then gSv.UnlockProcess(processId)
                    Catch ex As Exception
                        UserMessage.Err(ex,
                         String.Format(My.Resources.ctlDevelopView_ErrorWhilstUnlockingProcess0, ex.Message))
                        Return
                    End Try

            End Select

        End If

        'No autosave backup exists - plain simple open
        frm = New frmProcess(editMode, processId, "", "")
        AddHandlers(frm)

        AppForm.StartForm(frm)
    End Sub

    ''' <summary>
    ''' Check if the current user can launch the process using the view mode specified. 
    ''' Works the same as HasPermissionsForViewMode(), except this one displays an error message if user 
    ''' does not have permission.
    ''' </summary>
    ''' <param name="member"></param>
    ''' <param name="editmode"></param>
    ''' <returns></returns>
    Private Function CheckPermission(member As ProcessBackedGroupMember, editmode As ProcessViewMode) As Boolean

        If Not HasPermissionsForViewMode(member, editmode) Then
            UserMessage.Show(String.Format(
                My.Resources.AutomateUI_Controls.ctlDevelopView_CheckPermission_YouDoNotHavePermissionToAccess,
                ApplicationProperties.ApplicationName), 1048586)
            Return False
        End If
        Return True
    End Function


    ''' <summary>
    ''' Returns the appropriate 'Implied View' permission based on if the process is a Process or Business Object
    ''' </summary>
    ''' <param name="member"></param>
    ''' <returns></returns>
    Private Function GetImpliedViewPermission(member As ProcessBackedGroupMember) As String()
        Return If(member.MemberType = GroupMemberType.Process,
            Permission.ProcessStudio.ImpliedViewProcess,
            Permission.ObjectStudio.ImpliedViewBusinessObject)
    End Function

    ''' <summary>
    ''' Returns the appropriate 'Implied Edit' permission based on if the process is a Process or Business Object
    ''' </summary>
    ''' <param name="member"></param>
    ''' <returns></returns>
    Private Function GetImpliedEditPermission(member As ProcessBackedGroupMember) As String()
        Return If(member.MemberType = GroupMemberType.Process,
            Permission.ProcessStudio.ImpliedEditProcess,
            Permission.ObjectStudio.ImpliedEditBusinessObject)
    End Function

    ''' <summary>
    ''' Returns true if the user has appropriate permissions to open the process using the selected view mode.
    ''' </summary>
    ''' <param name="member">Process to open</param>
    ''' <param name="editmode">View mode to open process with</param>
    ''' <returns></returns>
    Private Function HasPermissionsForViewMode(member As ProcessBackedGroupMember, editmode As ProcessViewMode) As Boolean
        Dim perms As String() = Nothing
        Dim additionalPerms As String() = Nothing
        Select Case editmode
            Case ProcessViewMode.PreviewProcess
                perms = Permission.ProcessStudio.ImpliedViewProcess
            Case ProcessViewMode.PreviewObject
                perms = Permission.ObjectStudio.ImpliedViewBusinessObject
            Case ProcessViewMode.EditProcess
                perms = Permission.ProcessStudio.ImpliedEditProcess
            Case ProcessViewMode.EditObject
                perms = Permission.ObjectStudio.ImpliedEditBusinessObject
            Case ProcessViewMode.AdHocTestProcess
                perms = Permission.ProcessStudio.ImpliedExecuteProcess
                additionalPerms = Permission.ProcessStudio.ImpliedViewProcess
            Case ProcessViewMode.AdHocTestObject
                perms = Permission.ObjectStudio.ImpliedExecuteBusinessObject
                additionalPerms = Permission.ObjectStudio.ImpliedViewBusinessObject
            Case ProcessViewMode.CompareProcesses
                perms = Permission.ProcessStudio.ImpliedViewProcess
            Case ProcessViewMode.CompareObjects
                perms = Permission.ObjectStudio.ImpliedViewBusinessObject
            Case ProcessViewMode.DebugProcess
                perms = Permission.ProcessStudio.ImpliedExecuteProcess
                additionalPerms = Permission.ProcessStudio.ImpliedViewProcess
            Case ProcessViewMode.DebugObject
                perms = Permission.ObjectStudio.ImpliedExecuteBusinessObject
                additionalPerms = Permission.ObjectStudio.ImpliedViewBusinessObject
        End Select

        Return member.Permissions.HasPermission(User.Current, perms) AndAlso
            (additionalPerms Is Nothing OrElse member.Permissions.HasPermission(User.Current, additionalPerms))
    End Function

    ''' <summary>
    ''' Handles a group member unlock request
    ''' </summary>
    Private Sub HandleGroupMemberUnlockRequested(
     sender As Object, e As GroupMemberEventArgs)
        Try
            Dim pgm = TryCast(e.Target, ProcessBackedGroupMember)
            If pgm Is Nothing Then Return
            Dim thisMachine = ResourceMachine.GetName()

            ' Show a message if unlock isn't going to work due to unlock machine being different from machine that locked.
            If (Not String.IsNullOrWhiteSpace(pgm.LockMachineName) AndAlso thisMachine <> pgm.LockMachineName) Then
                UserMessage.Show(String.Format(My.Resources.ctlDevelopView_ThisProcessWasLockedOnMachine0ItCanOnlyBeUnlockedFromTheSameMachine, pgm.LockMachineName))
                Exit Sub
            End If

            If (Not gSv.UnlockProcess(pgm.IdAsGuid)) Then
                UserMessage.Show(My.Resources.ctlDevelopView_FailedToUnlockProcess)
                Exit Sub
            End If

            Try
                gSv.AuditRecordProcessEvent(ProcessEventCode.UnlockProcess, pgm.IdAsGuid, My.Resources.ctlDevelopView_UnlockedFrom & thisMachine, "", "")
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_WarningAuditRecordingFailedWithError0, ex.Message))
            End Try

            ' It would be nicer to update from the database, but it won't be
            ' performant with multiple processes / models - wait until the model
            ' centralisation work is done, and this will do until that time.
            pgm.ResetLock()
            gtGroups.UpdateView(True)

            DirectCast(DetailPanel, IRefreshable).RefreshView()

        Catch ex As Exception
            UserMessage.Err(
                ex, String.Format(My.Resources.ctlDevelopView_ErrorWhilstUnlockingProcess0, ex.Message))

        End Try
    End Sub


    ''' <summary>
    ''' Handles a group member delete request coming in from a child control
    ''' </summary>
    Private Sub HandleGroupMemberDeleteRequested(
     sender As Object, e As GroupMemberEventArgs) Handles gtGroups.DeleteRequested
        Dim permName As String
        Select Case e.Target.MemberType
            Case GroupMemberType.Object : permName = Permission.ObjectStudio.DeleteBusinessObject
            Case GroupMemberType.Process : permName = Permission.ProcessStudio.DeleteProcess
            Case Else : Return ' Not something we can process...
        End Select

        If Not User.Current.HasPermission(permName) Then
            UserMessage.Err(
             My.Resources.ctlDevelopView_YouDoNotHavePermissionToDeleteThis0IfYouBelieveThatThisIsIncorrectThenPleaseCon,
             e.Target.MemberType.GetLocalizedFriendlyName().ToLower(), ApplicationProperties.ApplicationName)
        Else
            Dim res As DialogResult = AppForm.StartForm(
             New frmProcessDelete(DirectCast(e.Target, ProcessBackedGroupMember)),
             True)
            If res = DialogResult.OK Then
                Dim gpPanel = TryCast(DetailPanel, GroupDetailPanel)
                If gpPanel IsNot Nothing Then HandleGroupSelected()
                gtGroups.ClearGroupCache()
                RefreshView(True)
            End If
        End If
    End Sub


    ''' <summary>
    ''' Handles the contents of a group changing - this may change the given group,
    ''' but we need to update the whole tree because the root may be affected by
    ''' the change (eg. removing an item from a group, may leave it in the root).
    ''' </summary>
    Private Sub HandleGroupContentsChanged(
     sender As Object, e As GroupMemberEventArgs)
        gtGroups.ClearGroupCache()
        gtGroups.UpdateView(True)
        RefreshViewPanel()
    End Sub


    ''' <summary>
    ''' Handles the group tree context menu click event for the 'View' menu item 
    ''' Opens the selected process in process studio using the 'Preview' or 'Debug' view mode - depending on user permissions.
    ''' </summary>
    ''' <param name="Sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleViewMenuItemClicked(sender As Object, e As EventArgs) Handles ViewItemMenuItem.Click

        Dim processMember = CType(gtGroups.SelectedMember, ProcessBackedGroupMember)
        OpenProcessInProcessStudio(processMember, GetLeastRestrictiveViewModeForPreview(processMember))
    End Sub


    ''' <summary>
    ''' Handles the group tree context menu click event for the 'Edit' menu item 
    ''' Opens the selected process in process studio using the 'Edit' view mode.
    ''' </summary>
    ''' <param name="Sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleEditMenuItemClicked(sender As Object, e As EventArgs) Handles EditItemMenuItem.Click
        'by activating the tree node, it will emulate how double click works.
        gtGroups.ActivateTreeNode(CType(gtGroups.SelectedMember, ProcessBackedGroupMember))
    End Sub


    ''' <summary>
    ''' Opens the process in process studio using the least restrictive view mode possible based on the users permissions
    ''' </summary>
    ''' <param name="member"></param>
    Private Sub OpenProcessInProcessStudio(member As ProcessBackedGroupMember)
        Dim processViewMode = GetLeastRestrictiveViewMode(member)
        OpenProcessInProcessStudio(member, processViewMode)
    End Sub

    ''' <summary>
    ''' Opens the process in ProcessStudio using the specified view mode
    ''' </summary>
    ''' <param name="member"></param>
    ''' <param name="viewMode"></param>
    Private Sub OpenProcessInProcessStudio(member As ProcessBackedGroupMember, viewMode As ProcessViewMode)
        If member Is Nothing Then Return

        Try
            Cursor = Cursors.WaitCursor
            LoadProcessStudio(member, viewMode)
            member.UpdateLockStatus()
            gtGroups.RefreshPadlock(member)

            ' to avoid another call to the server to get locked status
            ' we can overwrite the DetailPanel member, if they are the same
            Dim pdp = TryCast(DetailPanel, ProcessDetailPanel)
            If pdp IsNot Nothing AndAlso
                    Equals(pdp.ProcessMember.Id, member.Id) Then
                pdp.ProcessMember = member
            End If

        Finally
            Cursor = Cursors.Arrow
        End Try
        RefreshViewPanel()
    End Sub


    Private Function GetLeastRestrictiveViewModeForPreview(member As ProcessBackedGroupMember) As ProcessViewMode
        Dim viewMode = ProcessViewMode.PreviewProcess

        If member.MemberType = GroupMemberType.Process Then
            If HasPermissionsForViewMode(member, ProcessViewMode.DebugProcess) Then
                viewMode = ProcessViewMode.DebugProcess
            ElseIf HasPermissionsForViewMode(member, ProcessViewMode.PreviewProcess) Then
                viewMode = ProcessViewMode.PreviewProcess
            End If
        End If

        If member.MemberType = GroupMemberType.Object Then
            If HasPermissionsForViewMode(member, ProcessViewMode.DebugObject) Then
                viewMode = ProcessViewMode.DebugObject
            ElseIf HasPermissionsForViewMode(member, ProcessViewMode.PreviewObject) Then
                viewMode = ProcessViewMode.PreviewObject
            End If
        End If

        Return viewMode
    End Function

    ''' <summary>
    ''' Returns the least restrictive Process view mode permitted by the users permissions on the ProcessBackedGroupMember
    ''' </summary>
    ''' <param name="member"></param>
    ''' <returns></returns>
    Private Function GetLeastRestrictiveViewMode(member As ProcessBackedGroupMember) As ProcessViewMode
        Dim processViewMode As ProcessViewMode

        If member.ProcessType = DiagramType.Process Then
            processViewMode = ProcessViewMode.PreviewProcess
            If member.Permissions.HasPermission(User.Current, Permission.ProcessStudio.ImpliedEditProcess) Then
                processViewMode = ProcessViewMode.EditProcess
            ElseIf member.Permissions.HasPermission(User.Current, Permission.ProcessStudio.ImpliedExecuteProcess) Then
                processViewMode = ProcessViewMode.DebugProcess
            ElseIf member.Permissions.HasPermission(User.Current, Permission.ProcessStudio.ImpliedViewProcess) Then
                processViewMode = ProcessViewMode.PreviewProcess
            End If
        End If

        If member.ProcessType = DiagramType.Object Then
            processViewMode = ProcessViewMode.PreviewObject
            If member.Permissions.HasPermission(User.Current, Permission.ObjectStudio.ImpliedEditBusinessObject) Then
                processViewMode = ProcessViewMode.EditObject
            ElseIf member.Permissions.HasPermission(User.Current, Permission.ObjectStudio.ImpliedExecuteBusinessObject) Then
                processViewMode = ProcessViewMode.DebugObject
            ElseIf member.Permissions.HasPermission(User.Current, Permission.ObjectStudio.ImpliedViewBusinessObject) Then
                processViewMode = ProcessViewMode.PreviewObject
            End If
        End If

        Return processViewMode
    End Function


    ''' <summary>
    ''' Disposes of this control
    ''' </summary>
    ''' <param name="disposing">True if disposing of this control explicitly; False
    ''' if disposing through a finalizer.</param>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing Then
                If components IsNot Nothing Then components.Dispose()
                ' Make sure that this control is not still registered on any open
                ' frmProcess instances
                For Each frm In frmProcess.GetAllInstances()
                    RemoveHandlers(frm)
                Next
                ' Remove any listeners on the app form, and remove the association
                ' with this view
                AppForm = Nothing

            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    ''' <summary>
    ''' Toggle the showing of unrestricted groups with no visible contents.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ShowAllUnRestrictedMenuItem_Click(sender As Object, e As EventArgs) Handles ShowAllUnRestrictedMenuItem.Click
        mShowAllUnRestrictedGroups = Not mShowAllUnRestrictedGroups
        ShowAllUnRestrictedMenuItem.Checked = mShowAllUnRestrictedGroups
        gtGroups.UpdateView(True)
    End Sub

    Private Sub LoadProcessTree(store As IGroupStore)
        Try
            Dim processesTree As IGroupTree = store.GetTree(GroupTreeType.Processes,
                                                            GroupMember.NotRetired,
                                                            Function(x) x.ShouldShow(mShowAllUnRestrictedGroups),
                                                            True,
                                                            False,
                                                            False)
            gtGroups.AddTree(processesTree)
        Catch ex As Exception
            UserMessage.Err(
             ex, My.Resources.ctlDevelopView_AnErrorOccurredWhileLoadingTheProcessTree0, ex.Message)
        End Try
    End Sub

    Private Sub LoadObjectTree(store As IGroupStore)
        Try
            Dim objectsTree As IGroupTree = store.GetTree(GroupTreeType.Objects,
                                                          GroupMember.NotRetired,
                                                          Function(x) x.ShouldShow(mShowAllUnRestrictedGroups),
                                                          True,
                                                          False,
                                                          False)
            gtGroups.AddTree(objectsTree)
        Catch ex As Exception
            UserMessage.Err(
             ex, My.Resources.ctlDevelopView_AnErrorOccurredWhileLoadingTheObjectTree0, ex.Message)
        End Try
    End Sub

#End Region

End Class
