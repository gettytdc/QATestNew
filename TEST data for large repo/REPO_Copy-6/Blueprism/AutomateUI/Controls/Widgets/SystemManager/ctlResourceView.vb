Imports System.Threading
Imports AutomateControls.TreeList
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.Core.Resources
Imports BluePrism.Images
Imports BluePrism.Server.Domain.Models
Imports BluePrism.UIAutomation.Classes.SearchBar
Imports NLog

''' Project  : Automate
''' Class    : ctlResourceView
''' <summary>
''' This class provides a control which shows a 'Resource View' as seen in Control
''' Room and System Manager.
''' </summary>
Friend Class ctlResourceView : Inherits GroupTreeListView : Implements IChild

#Region " Member Variables "

    Private Shared ReadOnly Log As ILogger = LogManager.GetCurrentClassLogger()

    ' The last location of a mousemove event - In Win7+, mousemoves occur without
    ' any change to the mouse location; this shortcuts out if the locn is unchanged
    Private mLastMouseMoveLocn As Point = Point.Empty

    ' Cached reference to the resources last displayed.
    Private mResources As Dictionary(Of Guid, IResourceMachine)

    ' Flag indicating if we are showing resource pool (contents) in this viewer
    Private mShowPoolContents As Boolean

    ' Flag indicating if we should show only connected resource
    Private mConnectedOnly As Boolean

    'Flag indicating we should only show unavailable resources (those which have any
    'status other than connected
    Private mUnavailableOnly As Boolean

    Private mCancellationTokenSource As CancellationTokenSource = New CancellationTokenSource()

    ''' <summary>
    ''' Event handler for the refresh
    ''' </summary>
    Public Event RequestRefresh As EventHandler

    'Handles search and autofill of the search control
    Private ReadOnly mSearchFilter As FilterQuery

#End Region

#Region " Auto-Properties "

    ''' <summary>
    ''' Indicates whether to override the hiding of local machines, which are usually 
    ''' not viewable on remote machines
    ''' </summary>
    <Browsable(True), Category("Behavior"), DefaultValue(False),
     Description("Set to show all local resources, even those on other machines")>
    Public Property ShowLocalResourcesOnAllMachines As Boolean

    ''' <summary>
    ''' Attributes which must be possesed by resources in order to be listed
    ''' in this control. Leave blank (as zero/none) to have no effect.
    ''' </summary>
    <Browsable(True), Category("Behavior"), DefaultValue(ResourceAttribute.None),
     Description("The attributes which must be present to show a resource")>
    Public Property WithAttributes As ResourceAttribute

    ''' <summary>
    ''' Attributes which may not be possesed by resources listed
    ''' in this control. Leave blank (as zero/none) to have no effect.
    ''' </summary>
    ''' <value></value>
    <Browsable(True), Category("Behavior"), DefaultValue(ResourceAttribute.None),
     Description("The attributes which must be absent to show a resource")>
    Public Property WithoutAttributes As ResourceAttribute

    ''' <summary>
    ''' The application form ultimately hosting this resource view control
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new resource view control.
    ''' </summary>
    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()
        'Add any initialization after the InitializeComponent() call

        SubitemDefinitions = New Dictionary(Of String, System.Reflection.PropertyInfo) From {
            {My.Resources.ctlResourceView_State, GetType(ResourceGroupMember).GetProperty("StatusText")},
            {My.Resources.ctlResourceView_SessionInfo, GetType(ResourceGroupMember).GetProperty("Info")},
            {My.Resources.ctlResourceView_Members, GetType(ResourceGroupMember).GetProperty("ChildText")},
            {My.Resources.ctlResourceView_Connection, GetType(ResourceGroupMember).GetProperty("ConnectionStateText")},
            {My.Resources.ctlResourceView_LatestConnectionMessage, GetType(ResourceGroupMember).GetProperty("LatestConnectionMessage")}
        }

        mSearchFilter = New FilterQuery(My.Resources.Name.ToLower(), My.Resources.SearchKeyWordIn.ToLower())
        mSearchFilter.AddColumnNode(New SearchTreeNode(My.Resources.Name.ToLower(), New List(Of SearchTreeNode)))
        mSearchFilter.AddColumnNode(New SearchTreeNode(My.Resources.ctlResourceView_State.ToLower(), New List(Of SearchTreeNode)))
        mSearchFilter.AddColumnNode(New SearchTreeNode(My.Resources.ctlResourceView_SessionInfo.ToLower(), New List(Of SearchTreeNode)))
        mSearchFilter.AddColumnNode(New SearchTreeNode(My.Resources.ctlResourceView_Connection.ToLower(), New List(Of SearchTreeNode)))
        mSearchFilter.AddColumnNode(New SearchTreeNode(My.Resources.ctlResourceView_LatestConnectionMessage.ToLower(), New List(Of SearchTreeNode)))

    End Sub

#End Region

#Region " Properties "
    ''' <summary>
    ''' Gets or sets the filter to apply to the group tree list view.
    ''' </summary>
    Public Overrides Property Filter As Predicate(Of IGroupMember)
        Get
            ' Append the requirement to the IsResourceSuitableForDisplay() call to the filter
            ' being applied to the tree.
            Return AddressOf CheckRequirements
        End Get
        Set(value As Predicate(Of IGroupMember))
            ' It shouldn't ever happen, but make sure we don't cross the streams.
            If value = New Predicate(Of IGroupMember)(AddressOf CheckRequirements) Then
                Return
            End If
            MyBase.Filter = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets whether the pool contents are visible in this resource view.
    ''' </summary>
    <Browsable(True), Category("Behavior"), DefaultValue(False), Description(
     "Shows or hides the contents of a resource pool")>
    Public Property ShowPoolContents As Boolean
        Get
            Return mShowPoolContents
        End Get
        Set(value As Boolean)
            mShowPoolContents = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets whether the resource viewer should show only connected resources
    ''' </summary>
    <Browsable(True), Category("Behavior"), DefaultValue(False), Description(
     "Set to show only available resources in this control")>
    Public Property ShowAvailableResourcesOnly As Boolean
        Get
            Return mConnectedOnly
        End Get
        Set(value As Boolean)
            If value = mConnectedOnly Then Return
            mConnectedOnly = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets whether the resource viewer should show only unavailable resources
    ''' </summary>
    <Browsable(True), Category("Behavior"), DefaultValue(False), Description(
     "Set to show only unavailable resources in this control")>
    Public Property ShowUnavailableResourcesOnly As Boolean
        Get
            Return mUnavailableOnly
        End Get
        Set(value As Boolean)
            If value = mUnavailableOnly Then Return
            mUnavailableOnly = value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Return a list of the names of the selected resources. 
    ''' If a group is selected, all resources in that group (including sub-groups) are returned.
    ''' If a pool is selected, all resources in that pool are returned.
    ''' </summary>
    ''' <returns></returns>
    Public Function GetSelectedResources() As List(Of String)

        Dim resources = New List(Of String)

        For Each member In SelectedMembers

            ' if selected item is a Group, then get all the names of the resources in the group, including sub-groups.
            If member.IsGroup Then
                Dim group = CType(member, IGroup)
                group.Scan(Of ResourceGroupMember)(Sub(x) resources.AddRange(GetResourceNames(x)))

            Else
                ' if selected item is a resource, get the resource name.
                ' if selected item is a pool, get the names of resources in the pool.
                resources.AddRange(GetResourceNames(DirectCast(member, ResourceGroupMember)))
            End If
        Next

        ' The same resource could be a member of multiple groups, and therefore may have been selected more than once.
        ' Remove any duplicates
        Return resources.Distinct().ToList()
    End Function

    ''' <summary>
    ''' Returns the name of the Resource, or if resource is a pool, returns the names of the resources in the pool.
    ''' </summary>
    ''' <param name="resource"></param>
    ''' <returns></returns>
    Private Function GetResourceNames(resource As ResourceGroupMember) As List(Of String)
        Dim retval = New List(Of String)
        If resource.IsPool Then
            retval.AddRange(resource.PoolMembers.Select(Function(x) x.Name))
        Else
            retval.Add(resource.Name)
        End If
        Return retval
    End Function

    Private mThreadActive As Integer = 0

    ''' <summary>
    ''' Refreshes the list of resources with the latest statuses obtained from the 
    ''' connection manager
    ''' </summary>
    Public Sub RefreshView()
        If IsDisposed Then Return

        ' A race condition can occur meaning that RefreshView is invoked after the
        ' parent has been removed - quick check to ensure that's not the case here.
        Dim frm = ParentAppForm
        If frm Is Nothing Then Return

        If Interlocked.CompareExchange(mThreadActive, 1, 0) = 0 Then

            If ColumnUpdateRequired Then
                UpdateColumns()
            End If

            UpdateTreeFromResources(frm?.ConnectionManager?.GetResources(mConnectedOnly))
            UpdateView()
            mThreadActive = 0
        End If
    End Sub

    ''' <summary>
    ''' Refreshes the list of resources with the latest statuses
    ''' and with the supplied list of resources.
    ''' <param name="allResources">A List of clsResourceMachine
    ''' objects to be added. This must be a complete list, with no filtering
    ''' as the control will do its own filtering and may change the settings
    ''' after this call has been made. Can also be Nothing, to account for
    ''' sloppy coding.</param>
    ''' </summary>
    Private Sub UpdateTreeFromResources(allResources As Dictionary(Of Guid, IResourceMachine))
        Dim updatedResourceMachines As New HashSet(Of IResourceMachine)
        mResources = allResources

        Try
            ' Tree is occasionally null briefly when terminating application
            Dim treeContents = Tree?.Root?.FlattenedContents(Of List(Of IGroupMember))(False)
            If treeContents IsNot Nothing Then
                UpdateResourceTreeNodeIfChanged(
                treeContents,
                mResources,
                updatedResourceMachines,
                gSv.GetPoolResourceIds())
                AddResourceTreeNodeIfNew(updatedResourceMachines)
            End If
        Catch ex As Exception
            Dim currProcId As Integer = Process.GetCurrentProcess().Id
            If ex.InnerException IsNot Nothing Then
                Log.Error(ex.InnerException, "UpdateTreeFromResources: Inner Exception", currProcId)
            End If
            Log.Error(ex, "UpdateTreeFromResources: Call Stack: {1}-- Process Id:{0}-- Exception: ", currProcId, ex.StackTrace)
            RaiseEvent RequestRefresh(Me, New EventArgs)
        End Try
    End Sub

    ''' <summary>
    ''' Update a single resource node
    ''' </summary>
    ''' <param name="resourceGroupMembers">The nodes we are currently processing</param>
    ''' <param name="updatedResourceMachines">list of all resources from the server</param>
    ''' <param name="updated">list of updated resources</param>
    ''' <param name="pools">list of pools and their members</param>
    Private Sub UpdateResourceTreeNodeIfChanged(resourceGroupMembers As IEnumerable(Of IGroupMember),
                                                updatedResourceMachines As IDictionary(Of Guid, IResourceMachine),
                                                updated As ISet(Of IResourceMachine),
                                                pools As IDictionary(Of Guid, ICollection(Of Guid)))
        If resourceGroupMembers.Count > 0 Then
            mSearchFilter.PruneColumnNodes()
        End If
        For Each resourceGroupMember As ResourceGroupMember In resourceGroupMembers
            Dim updatedResourceMachine As IResourceMachine = Nothing
            updatedResourceMachines.TryGetValue(resourceGroupMember.IdAsGuid, updatedResourceMachine)

            If updatedResourceMachine Is Nothing Then Continue For

            If IsResourceSuitableForDisplay(updatedResourceMachine) Then
                resourceGroupMember?.MapUpdatedProperties(updatedResourceMachine)

                If resourceGroupMember?.IsPool Then RefreshPoolMembership(resourceGroupMember, updatedResourceMachine, pools)
                updated.Add(updatedResourceMachine)
            End If

            'update autofill search tree
            Try
                mSearchFilter.AddLeafToColumnNode(My.Resources.Name.ToLower(), New SearchTreeNode(updatedResourceMachine.Name, Nothing))
                mSearchFilter.AddLeafToColumnNode(My.Resources.ctlResourceView_State.ToLower(), New SearchTreeNode(ResourceInfo.GetResourceStatusFriendlyName(updatedResourceMachine.DisplayStatus), Nothing))
                mSearchFilter.AddLeafToColumnNode(My.Resources.ctlResourceView_SessionInfo.ToLower(), New SearchTreeNode($"{updatedResourceMachine.Info}", Nothing))
                mSearchFilter.AddLeafToColumnNode(My.Resources.ctlResourceView_Connection.ToLower(), New SearchTreeNode(ResourceGroupMember.ConnectionStateToStateText(updatedResourceMachine.ConnectionState), Nothing))
                mSearchFilter.AddLeafToColumnNode(My.Resources.ctlResourceView_LatestConnectionMessage.ToLower(), New SearchTreeNode($"{updatedResourceMachine.LastError}", Nothing))
            Catch ex As NullReferenceException
                Log.Error(ex, "Exception in search tree creation: Name: {0}, State: {1}, SessionInfo: {2}, Connection: {3}, LastConnMessage: {4}",
                    updatedResourceMachine.Name,
                    ResourceInfo.GetResourceStatusFriendlyName(updatedResourceMachine.DisplayStatus),
                    $"{updatedResourceMachine.Info}",
                    ResourceGroupMember.ConnectionStateToStateText(updatedResourceMachine.ConnectionState),
                    $"{updatedResourceMachine.LastError}")
            End Try

        Next

    End Sub

    Private Sub AddResourceTreeNodeIfNew(updatedResourceMachines As ISet(Of IResourceMachine))
        Dim newResources = mResources.
            Select(Function(x) x.Value).
            Where(Function(x) Not updatedResourceMachines.Contains(x))

        For Each r In newResources
            If IsResourceSuitableForDisplay(r) AndAlso
               gSv.GetEffectiveGroupPermissionsForResource(r.Id).HasPermission(ParentAppForm.CurrentUser, Permission.Resources.ImpliedViewResource) Then
                If Not r.HasAttribute(ResourceAttribute.Pool) Then
                    Me.Tree.DefaultGroup.Add(New ResourceGroupMember(r))
                Else
                    Dim matchTree = CType(Tree.RawTree.Root.FlattenedContents(Of List(Of IGroupMember))(True), IReadOnlyCollection(Of IGroupMember)).
                        FirstOrDefault(Function(f) f.IdAsGuid = r.Id)?.Owner?.IdAsGuid

                    If matchTree IsNot Nothing Then
                        Tree.Root.FindSubGroup(matchTree.Value).Add(New ResourceGroupMember(r))
                    End If
                End If
            End If
        Next
    End Sub

    Private Sub RefreshPoolMembership(resourceGroupMember As ResourceGroupMember, resourceMachine As IResourceMachine, pools As IDictionary(Of Guid, ICollection(Of Guid)))
        Dim poolMembers As ICollection(Of Guid) = Nothing
        pools.TryGetValue(resourceGroupMember.IdAsGuid, poolMembers)
        resourceGroupMember?.SetPoolMembers(poolMembers)

        For Each poolMember In resourceGroupMember.PoolMembers
            If resourceMachine.ChildResources IsNot Nothing Then
                Dim upd = resourceMachine.ChildResources.FirstOrDefault(Function(n) n.Id = poolMember.IdAsGuid)
                poolMember.MapUpdatedProperties(upd)
            End If
        Next
    End Sub

    ''' <summary>
    ''' Additional filter applied to this control, ensuring that a resource group
    ''' member meets the requirements imposed by the control as well as the filter.
    ''' This basically checks properties based on the current state of the control
    ''' (eg. allowing local resources, connectivity, etc).
    ''' </summary>
    ''' <param name="m">The group member whose requirements should be checked</param>
    ''' <returns>True if the member meets the requirements and can be shown; False
    ''' otherwise.</returns>
    Private Function CheckRequirements(m As IGroupMember) As Boolean
        If Not MyBase.Filter(m) Then Return False
        If m.IsGroup Then Return True
        If mResources Is Nothing Then Return True
        If Not mResources.ContainsKey(m.IdAsGuid) Then Return False
        Return IsResourceSuitableForDisplay(mResources(m.IdAsGuid))
    End Function

    ''' <summary>
    ''' Determines if a resource is suitable for display.</summary>
    ''' <param name="res">The resource to check. A null resource will always return
    ''' false.</param>
    ''' <returns>True if the resource meets requirements, False otherwise.</returns>
    Private Function IsResourceSuitableForDisplay(res As IResourceMachine) As Boolean
        ' Shortcut for not finding a resource - can't find it? Can't display it
        If res Is Nothing Then Return False

        'Search Filer
        If mSearchFilter.SearchText.Length <> 0 Then
            If Not IsResourceInSearch(res) Then Return False
        End If

        ' If we can't display local resources running on other machines,
        ' filter it out
        If Not ShowLocalResourcesOnAllMachines AndAlso res.Local AndAlso
         res.Name <> ResourceMachine.GetName() Then Return False

        ' If we have required attributes set and the resource doesn't match any
        ' of them, filter it out
        If WithAttributes <> ResourceAttribute.None AndAlso
         (res.Attributes And WithAttributes) = 0 Then Return False

        ' If we have unacceptable attributes set and the resource matches any of
        ' them, filter it out.
        If WithoutAttributes <> ResourceAttribute.None AndAlso
         (res.Attributes And WithoutAttributes) <> 0 Then Return False

        ' If we're only displaying available resources, and the given resource is
        ' not available, filter it out.
        If ShowAvailableResourcesOnly AndAlso
         Not res.IsConnected Then Return False

        'If we're only displaying unavailable resources and the resource is 
        'available then filter it out
        If ShowUnavailableResourcesOnly AndAlso
            res.IsConnected Then Return False

        'If we passed all the above tests, we can see this resource
        Return True

    End Function

    Private Function IsResourceInSearch(res As IResourceMachine) As Boolean
        Select Case mSearchFilter.ColumnName
            Case My.Resources.Name.ToLower()
                Return res.Name.ToString.ToLower.Contains(mSearchFilter.SearchText)
            Case My.Resources.ctlResourceView_State.ToLower()
                Return ResourceInfo.GetResourceStatusFriendlyName(res.DisplayStatus).ToLower.Contains(mSearchFilter.SearchText)
            Case My.Resources.ctlResourceView_SessionInfo.ToLower()
                If res.Info IsNot Nothing Then
                    If res.Attributes <> ResourceAttribute.Pool AndAlso
                    res.Info.ToString.ToLower.Contains(mSearchFilter.SearchText) Then
                        Return True
                    End If
                End If
            Case My.Resources.ctlResourceView_Connection.ToLower()
                Return ConnectionStateToStateText(res.ConnectionState).ToLower.Contains(mSearchFilter.SearchText)
            Case My.Resources.ctlResourceView_LatestConnectionMessage.ToLower()
                If res.LastError IsNot Nothing Then
                    Return res.LastError.ToString.ToLower.Contains(mSearchFilter.SearchText)
                End If
        End Select
        Return False
    End Function


    Private Function ConnectionStateToStateText(ConnectionState As ResourceConnectionState) As String
        Return ResourceGroupMember.ConnectionStateToStateText(ConnectionState)
    End Function

    Public Sub ResetSearch()
        mSearchFilter.Evaluate(String.Empty)
        UpdateView()
        ResortColumn(columnIndex:=0)
    End Sub

    Public Function FitlerResources(query As String) As List(Of String)
        mSearchFilter.Evaluate(query)
        UpdateView()
        ResortColumn(columnIndex:=0)
        Return mSearchFilter.GetAutoFills()
    End Function

    ''' <summary>
    ''' Sets the toolip for a listed item.
    ''' </summary>
    Public Sub SetToolTip()
        Dim p As Point = Me.PointToClient(Cursor.Position)
        Dim mem As ResourceGroupMember = TryCast(Me.GetMemberAt(p), ResourceGroupMember)
        If mem IsNot Nothing Then
            If ttResStatus.Tag IsNot mem Then
                If mem.Attributes.HasFlag(ResourceAttribute.LoginAgent) AndAlso
                    mem.ConnectionState = ResourceConnectionState.Connected Then
                    ttResStatus.SetToolTip(Me, String.Format(My.Resources.ctlResourceView_0AsLoginAgent, mem.ConnectionStateText))
                Else
                    ttResStatus.SetToolTip(Me, mem.ConnectionStateText)
                End If
                ttResStatus.Tag = mem
            End If
        Else
            ttResStatus.RemoveAll()
            ttResStatus.Tag = Nothing
        End If
    End Sub

    ''' <summary>
    ''' Event handler for right click.
    ''' </summary>
    ''' <param name="p">The location to of the mouse</param>
    Private Sub ShowContextMenu(p As Point)
        Dim resourceItem As TreeListViewItem

        If p = Point.Empty Then
            resourceItem = SelectedItems.FirstOrDefault()
        Else
            resourceItem = GetItemAt(p)
        End If

        Dim resourceMember = If(resourceItem Is Nothing, Nothing, TryCast(resourceItem.Tag, ResourceGroupMember))

        Dim ctxMenu As New ContextMenuStrip()

        Dim showScreenshotMenu As New ToolStripMenuItem(My.Resources.ctlResourceView_ShowLatestScreenCapture, Nothing, AddressOf ShowScreenshot)
        If resourceMember IsNot Nothing Then

            Dim allowed = gSv.GetAllowResourceScreenshot()
            If allowed Then
                Try
                    Dim available = gSv.CheckExceptionScreenshotAvailable(resourceMember.IdAsGuid)
                    If available Then
                        showScreenshotMenu.Enabled = True
                        showScreenshotMenu.Tag = resourceMember
                    Else
                        showScreenshotMenu.ToolTipText = My.Resources.ctlResourceView_ThereAreNoScreenCapturesAvailableForThisResource
                        showScreenshotMenu.Enabled = False
                    End If
                Catch ex As PermissionException
                    showScreenshotMenu.ToolTipText = My.Resources.ctlResourceView_YouDoNotHavePermissionToViewScreenCaptures
                    showScreenshotMenu.Enabled = False
                End Try
            Else
                showScreenshotMenu.ToolTipText = My.Resources.ctlResourceView_ScreenCapturesAreDisabledInThisEnvironment
                showScreenshotMenu.Enabled = False
            End If
        Else
            showScreenshotMenu.ToolTipText = My.Resources.ctlResourceView_NoResourceSelected
            showScreenshotMenu.Enabled = False
        End If

        If ParentAppForm?.ConnectionManager?.QueryCapabilities Then
            Dim capMenu As New ToolStripMenuItem(My.Resources.ctlResourceView_Capabilities)
            If resourceItem IsNot Nothing Then
                AddCapabilities(capMenu, resourceItem)
                capMenu.Enabled = True
            Else
                capMenu.Enabled = False
            End If

            ctxMenu.Items.Add(capMenu)
        End If

        Dim availOnlyMenuItem As New ToolStripMenuItem(My.Resources.ctlResourceView_AvailableResourcesOnly, Nothing, AddressOf mnuAvailableOnly_Click)
        availOnlyMenuItem.Checked = ShowAvailableResourcesOnly

        Dim unavailOnlyMenuItem As New ToolStripMenuItem(My.Resources.ctlResourceView_UnavailableResourcesOnly, Nothing, AddressOf mnuUnavailableOnly_Click)
        unavailOnlyMenuItem.Checked = ShowUnavailableResourcesOnly

        ctxMenu.Items.Add(showScreenshotMenu)
        ctxMenu.Items.Add(availOnlyMenuItem)
        ctxMenu.Items.Add(unavailOnlyMenuItem)
        ctxMenu.Items.Add(New ToolStripSeparator())
        ctxMenu.Items.Add(New ToolStripMenuItem(My.Resources.ctlResourceView_ExpandAll, ToolImages.Expand_All_16x16, Sub() ExpandAll()))
        ctxMenu.Items.Add(New ToolStripMenuItem(My.Resources.ctlResourceView_CollapseAll, ToolImages.Collapse_All_16x16, Sub() CollapseAll()))

        HandleResourceDetailsMenuItem(resourceMember, ctxMenu)

        ctxMenu.Show(Me, p)
    End Sub

    Private Sub HandleResourceDetailsMenuItem(resourceMember As ResourceGroupMember, ctxMenu As ContextMenuStrip)
        Dim resourceDetailsMenuItem = New ToolStripMenuItem(My.Resources.ResourceView_ResourceInfo, Nothing)
        Dim resourceName As String

        If resourceMember IsNot Nothing AndAlso Not resourceMember.IsPool Then
            Try
                gSv.CheckResourceDetailPermission()

                If resourceMember.HasAttribute(ResourceAttribute.DefaultInstance) Then
                    Const localPort = ":-1"
                    resourceName = resourceMember.Name + localPort
                Else
                    resourceName = resourceMember.Name
                End If

                resourceDetailsMenuItem = New ToolStripMenuItem(My.Resources.ResourceView_ResourceInfo,
                                                                    Nothing,
                                                                    Sub() ParentAppForm.StartForm(New FrmResourceDetailsViewer(resourceName), True))

                If Not gSv.GetPref(PreferenceNames.SystemSettings.EnableBpaEnvironmentData, True) Then
                    resourceDetailsMenuItem.DisableMenuItemAndSetTooltip(My.Resources.ctlResourceView_EnableSaveEnvironmentData)
                End If

            Catch ex As PermissionException
                resourceDetailsMenuItem.DisableMenuItemAndSetTooltip(My.Resources.ctlResourceView_YouDoNotHavePermissionToViewResourceDetails)
            Finally
                ctxMenu.Items.Add(New ToolStripSeparator())
                ctxMenu.Items.Add(resourceDetailsMenuItem)
            End Try
        Else
            resourceDetailsMenuItem.DisableMenuItemAndSetTooltip(My.Resources.ctlResourceView_NoResourceSelected)
        End If
    End Sub

    ''' <summary>
    ''' Add capabilities to the resource capabilities menu
    ''' </summary>
    ''' <param name="capMenu">The capabilities menu</param>
    ''' <param name="resourceItem">The resource Item</param>
    Private Sub AddCapabilities(capMenu As ToolStripMenuItem, resourceItem As ListViewItem)
        Dim resource = ParentAppForm.ConnectionManager.GetResource(resourceItem.SubItems(0).Text)
        If Not resource Is Nothing Then
            If Not resource.CapabilitiesFriendly Is Nothing Then
                Dim s As String
                For Each s In resource.CapabilitiesFriendly
                    capMenu.DropDownItems.Add(s)
                Next
            Else
                capMenu.DropDownItems.Add(My.Resources.ctlResourceView_None)
            End If
        Else
            capMenu.DropDownItems.Add(My.Resources.ctlResourceView_None)
        End If
    End Sub

    ''' <summary>
    ''' Shows a screenshot in the screenshot viewer
    ''' </summary>
    Private Sub ShowScreenshot(sender As Object, e As EventArgs)
        Try
            Dim menuItem = TryCast(sender, ToolStripItem)
            If menuItem Is Nothing Then Return

            Dim resourceMember = TryCast(menuItem.Tag, ResourceGroupMember)
            If resourceMember Is Nothing Then Return

            Dim viewer As New frmScreenshotViewer()
            viewer.ResourceId = resourceMember.IdAsGuid
            viewer.Show(Me)

        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.ctlResourceView_ErrorShowingScreenshot)
        End Try
    End Sub

    ''' <summary>
    ''' Handles the Available Resource Only option on the context menu.
    ''' </summary>
    Private Sub mnuAvailableOnly_Click(ByVal sender As Object, ByVal e As EventArgs)
        ShowAvailableResourcesOnly = Not ShowAvailableResourcesOnly
        If ShowAvailableResourcesOnly Then ShowUnavailableResourcesOnly = False
        If mResources IsNot Nothing Then
            RefreshView()
        End If
    End Sub

    ''' <summary>
    ''' Handles a tree loaded event, ensuring that the data is up to date with the
    ''' data from the resource connection manager.
    ''' </summary>
    ''' <remarks>Note that this event may not be fired on the UI thread - as it
    ''' stands, this operates only on the resource connection manager and the
    ''' returned data, so that is not required, but be aware of the fact in case any
    ''' UI elements end up being updated in this method.</remarks>
    Protected Overrides Sub OnTreeLoaded(e As GroupTreeEventArgs)
        If ParentAppForm Is Nothing Then Return
        ParentAppForm.ConnectionManager.GetLatestDBResourceInfo((ResourceAttribute.Retired Or ResourceAttribute.Debug))
        UpdateTreeFromResources(ParentAppForm.ConnectionManager.GetResources(mConnectedOnly))
    End Sub


    ''' <summary>
    ''' Handles the Unavailable Resource Only option on the context menu.
    ''' </summary>
    Private Sub mnuUnavailableOnly_Click(ByVal sender As Object, ByVal e As EventArgs)
        ShowUnavailableResourcesOnly = Not ShowUnavailableResourcesOnly
        If ShowUnavailableResourcesOnly Then ShowAvailableResourcesOnly = False
        If mResources IsNot Nothing Then
            RefreshView()
        End If
    End Sub

    ''' <summary>
    ''' Raises the <see cref="GroupTreeListViewItemAdded"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overrides Sub OnGroupTreeListViewItemAdded(e As GroupTreeListViewItemEventArgs)
        MyBase.OnGroupTreeListViewItemAdded(e)

        Dim rgm = TryCast(e.Item.Tag, ResourceGroupMember)
        If rgm Is Nothing Then Return

        For Each poolMem As ResourceGroupMember In rgm.PoolMembers
            AddEntry(poolMem, e.Item.Items)
        Next
    End Sub

    ''' <summary>
    ''' Raises the <see cref="GroupTreeListViewItemAdded"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overrides Sub OnGroupTreeListViewItemUpdated(e As GroupTreeListViewItemEventArgs)
        MyBase.OnGroupTreeListViewItemUpdated(e)

        Dim rgm = TryCast(e.Item.Tag, ResourceGroupMember)
        If rgm Is Nothing Then Return

        ' Remove any existing items that this item has
        e.Item.Items.Clear()

        For Each poolMem As ResourceGroupMember In rgm.PoolMembers
            AddEntry(poolMem, e.Item.Items)
        Next
    End Sub

    ''' <summary>
    ''' Handles the <see cref="GroupMembersBeforeDrag"/> event, ensuring that pool
    ''' members are not dragged.
    ''' </summary>
    Protected Overrides Sub OnGroupMembersBeforeDrag(e As GroupMultipleMemberEventArgs)
        MyBase.OnGroupMembersBeforeDrag(e)
        ' If any resources are pool members, don't allow the drag
        For Each rgm In e.Members.OfType(Of ResourceGroupMember)()
            If rgm.IsPoolMember Then e.Cancel = True : Return
        Next
    End Sub

    ''' <summary>
    ''' Handles the <see cref="DragOver"/> event, ensuring that dragging over pool
    ''' members is discreetly discouraged
    ''' </summary>
    Protected Overrides Sub OnDragOver(e As DragEventArgs)
        MyBase.OnDragOver(e)
        ' If we can't accept this dragged thing anyway, no point in looking further
        If e.Effect = DragDropEffects.None Then Return

        ' Get the target resource; if it's not one, then carry on - not our concern
        Dim mem = TryCast(GetTargetMember(e), ResourceGroupMember)
        If mem Is Nothing Then Return

        ' If the target is a pool member, we can't allow dragging onto that
        If mem.IsPoolMember _
         Then HighlightedNode = Nothing : e.Effect = DragDropEffects.None

    End Sub

    ''' <summary>
    ''' Handles a MouseDown event in this control. This creates the context menu.
    ''' </summary>
    Protected Overrides Sub OnMouseDown(ByVal e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        If e.Button = MouseButtons.Right Then
            MultiSelect = False
            ShowContextMenu(e.Location)
        Else
            MultiSelect = True
        End If
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)

        If e.KeyCode = Keys.F10 AndAlso e.Modifiers = Keys.Shift Then
            ShowContextMenu(Point.Empty)
        End If
    End Sub

    ''' <summary>
    ''' Handles a MouseMove on this resource view.
    ''' </summary>
    Protected Overrides Sub OnMouseMove(ByVal e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        ' On Windows 7, it seems this can be called repeatedly even when the mouse
        ' hasn't moved - make sure we don't do anything we don't need to.
        If e.Location = mLastMouseMoveLocn Then Return
        mLastMouseMoveLocn = e.Location
        SetToolTip()
    End Sub

    ''' <summary>
    ''' Handles a MouseLeave event on this resource view.
    ''' </summary>
    Protected Overrides Sub OnMouseLeave(ByVal e As EventArgs)
        MyBase.OnMouseLeave(e)
        ttResStatus.Tag = Nothing
    End Sub

    ''' <summary>
    ''' Disposes of this control
    ''' </summary>
    ''' <param name="disposing">True if called explicitly; False if called via a
    ''' finalizer</param>
    Protected Overrides Sub Dispose(disposing As Boolean)
        mCancellationTokenSource.Cancel()
        MyBase.Dispose(disposing)
        If disposing Then
            ParentAppForm = Nothing
        End If
    End Sub

#End Region
End Class
