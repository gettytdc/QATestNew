Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports Dashboard = BluePrism.AutomateAppCore.Dashboard
Imports DashboardTypes = BluePrism.AutomateAppCore.DashboardTypes

Imports BluePrism.Images

Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateAppCore.Auth.Permission
Imports LocaleTools
Imports AutomateUI
Imports AutomateControls.Forms
Imports System.Collections.Concurrent
Imports AutomateControls.UIState.UIElements
Imports BluePrism.Server.Domain.Models

Public Class ctlDashboard
    Implements IPermission, IStubbornChild, IHelp, IEnvironmentColourManager, IRefreshable

#Region "Member variables, constants and enums"

    'The home page node
    Private mHomePageNode As TreeNode

    'The personal dashboards group node
    Private mPersonalNode As TreeNode

    'The global dashboards group node
    Private mGlobalNode As TreeNode

    'The published dashboards group node
    Private mPublishedNode As TreeNode

    'The dashboard node currently in view
    Private mViewingNode As TreeNode

    'The dashboard node currently being edited
    Private mEditingNode As TreeNode

    'Expanded group nodes
    Private mExpandedGroups As New List(Of String)

    'Is the setting to send published dashboards to data gateways enabled
    Private ReadOnly mAreDashboardsDGEnabled As Boolean

    'Cache to store the edited items.
    Private mEditCache As New ConcurrentDictionary(Of Guid, Tile)

    ''' <summary>
    ''' Custom sorter for dashboard tree, order is:
    ''' Home page
    ''' Personal dashboards (with children sorted alphabetically)
    ''' Global dashboards (with children sorted alphabetically)
    ''' Published dashboards (with children sorted alphabetically)
    ''' </summary>
    Public Class DashboardSorter
        Implements IComparer
        Public Property Parent As ctlDashboard

        Public Sub New(oParent As ctlDashboard)
            Me.Parent = oParent
        End Sub

        Public Function Compare(x As Object, y As Object) As Integer _
         Implements IComparer.Compare
            Dim tx As TreeNode = CType(x, TreeNode)
            Dim ty As TreeNode = CType(y, TreeNode)

            If tx.Parent Is Nothing Then
                'Sort dashboard groups
                If tx Is Parent.mHomePageNode Then Return -1
                If tx Is Parent.mPersonalNode AndAlso ty Is Parent.mGlobalNode Then Return -1
                If tx Is Parent.mPersonalNode AndAlso ty Is Parent.mPublishedNode Then Return -1
                If tx Is Parent.mGlobalNode AndAlso ty Is Parent.mPublishedNode Then Return -1
                Return 1
            Else
                'Sort dashboards alphabetically
                Return tx.Text.CompareTo(ty.Text)
            End If

        End Function
    End Class

#End Region

#Region " Constructors "

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        'Load tree view images
        ilTreeImages.Images.Add("HOME", ToolImages.Home_16x16)
        ilTreeImages.Images.Add("VIEW", ToolImages.Window_16x16)
        ilTreeImages.Images.Add("DEFVIEW", ToolImages.Window_Lock_16x16)
        ilTreeImages.Images.Add("TILE", ToolImages.Chart_Pie_16x16)
        ilTreeImages.Images.Add("FOPEN", ComponentImages.Folder_Open_16x16)
        ilTreeImages.Images.Add("FCLOSED", ComponentImages.Folder_Closed_16x16)

        mAreDashboardsDGEnabled = gSv.GetDataPipelineSettings().SendPublishedDashboardsToDataGateways()
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The node representing the dashboard (I think?) which is currently being
    ''' edited. This ensures that drag/drop is disabled on the tile tree if edit
    ''' mode has been entered, and re-enabled (permission allowing) if not editing
    ''' </summary>
    Private Property EditingNode As TreeNode
        Get
            Return mEditingNode
        End Get
        Set(value As TreeNode)
            If mEditingNode Is value Then Return
            ' Make sure that drag/drop is disabled while editing a dashboard
            TilesGroupTreeControl.AllowDrop = (value Is Nothing AndAlso CanEditTiles())
            mEditingNode = value
        End Set
    End Property

    Private ReadOnly Property DefaultDashboardId As Guid
        Get
            Return Guid.Empty
        End Get
    End Property

#End Region

#Region "Dashboard tree methods"

    'Loads the dashboard views avalable to the current user
    Private Async Function LoadDashboards() As Task

        Await Task.Run(Sub()
                           Try
                               Dim dashboardList As List(Of Dashboard)

                               'Determine user's home page ID. Dashboard with ID = Guid.Empty is always present
                               Dim usersHomePageId As Guid = gSv.GetPref(PreferenceNames.UI.DefaultDashboard, Guid.Empty)

                               Try
                                   dashboardList = gSv.GetDashboardList()
                                   If Not dashboardList.Any() Then
                                       Throw New NoSuchDashboardException(usersHomePageId)
                                   End If
                               Catch ex As Exception
                                   Invoke(Sub() ShowError(My.Resources.ctlDashboard_FailedToRetrieveDashboardViews, ex))
                                   Return
                               End Try

                               'Create grouping nodes
                               Invoke(Sub() mViewTree.Nodes.Clear())

                               Invoke(Sub()
                                          mHomePageNode = AddDashboardGroup(My.Resources.ctlDashboard_HomePageGroup, "HOME")
                                          mPersonalNode = AddDashboardGroup(My.Resources.ctlDashboard_MyDashboardsGroup, "FCLOSED")
                                          mGlobalNode = AddDashboardGroup(My.Resources.ctlDashboard_GlobalDashboardsGroup, "FCLOSED")
                                          mPublishedNode = AddDashboardGroup(My.Resources.ctlDashboard_PublishedDashboardsGroup, "FCLOSED")
                                      End Sub)

                               'Add dashboards to tree view
                               Dim hpFound As Boolean = False
                               For Each dash As Dashboard In dashboardList
                                   If dash.ID = DefaultDashboardId Then _
                                 dash.Name = LTools.Get(dash.Name, "tile", "dash")
                                   If dash.ID = usersHomePageId Then
                                       hpFound = True
                                       Invoke(Sub() SetHomePageNode(dash))
                                   End If

                                   Invoke(Sub() AddDashboard(dash.Type, dash))
                               Next

                               'If home dashboard no longer present then show global default
                               Invoke(Sub()
                                          If mExpandedGroups.Contains(My.Resources.ctlDashboard_MyDashboardsGroup) AndAlso mPersonalNode.Nodes.Count > 0 Then _
                                        mPersonalNode.Expand()
                                          If mExpandedGroups.Contains(My.Resources.ctlDashboard_GlobalDashboardsGroup) AndAlso mGlobalNode.Nodes.Count > 0 Then _
                                        mGlobalNode.Expand()
                                          If mExpandedGroups.Contains(My.Resources.ctlDashboard_PublishedDashboardsGroup) AndAlso mPublishedNode.Nodes.Count > 0 Then _
                                        mPublishedNode.Expand()
                                          If Not hpFound Then
                                              SetHomePageNode(dashboardList.Find(Function(d) d.ID = Guid.Empty))
                                          End If
                                      End Sub)

                               Invoke(Sub()
                                          mViewTree.TreeViewNodeSorter = New DashboardSorter(Me)
                                          mViewTree.Sort()
                                          mPersonalNode?.Expand()
                                          mViewTree.SelectedNode = mHomePageNode
                                      End Sub)
                           Catch ex As ObjectDisposedException
                               ' Frequent switching of tabs can cause the dashboard to be disposed while loading, this stops an error 
                               ' being displayed which is unimportant.
                           Catch ex As Exception
                               Invoke(Sub() ShowError(My.Resources.ctlDashboard_FailedToRetrieveDashboardViews, ex))
                           End Try
                       End Sub)
    End Function

    'Adds a dashboard group to the tree
    Private Function AddDashboardGroup(name As String, img As String) As TreeNode
        Return mViewTree.Nodes.Add(name, name, img, img)
    End Function

    'Adds a dashboard node to the tree
    Private Function AddDashboard(type As DashboardTypes, dashboard As Dashboard) As TreeNode
        Dim dashboardNode As New TreeNode(dashboard.Name)
        dashboardNode.Name = dashboard.ID.ToString()
        dashboardNode.Tag = dashboard
        If dashboard.ID = Guid.Empty Then
            dashboardNode.ImageKey = "DEFVIEW"
            dashboardNode.SelectedImageKey = "DEFVIEW"
        Else
            dashboardNode.ImageKey = "VIEW"
            dashboardNode.SelectedImageKey = "VIEW"
        End If
        Select Case type
            Case DashboardTypes.Personal
                mPersonalNode.Nodes.Add(dashboardNode)
            Case DashboardTypes.Global
                mGlobalNode.Nodes.Add(dashboardNode)
            Case DashboardTypes.Published
                mPublishedNode.Nodes.Add(dashboardNode)
        End Select

        Return dashboardNode
    End Function

    'Updates the home page node with the user's default dashboard
    Private Sub SetHomePageNode(dash As Dashboard)
        mHomePageNode.Name = dash.ID.ToString()
        mHomePageNode.Text = dash.Name
        mHomePageNode.Tag = dash
    End Sub

    'Updates the user's preference
    Private Sub SetHomePage(dash As Dashboard)
        If CType(mHomePageNode.Tag, Dashboard) IsNot dash Then
            Try
                gSv.SetHomePage(dash)
            Catch ex As Exception
                ShowError(My.Resources.ctlDashboard_FailedToSetHomePageDashboard, ex)
            End Try
        End If
        SetHomePageNode(dash)
    End Sub

    'Refresh the current on screen tile, without re-initialising tiles 
    Private Sub RefreshDashboard(dashNode As TreeNode, Optional refreshFromServer As Boolean = True)
        mViewingNode = dashNode
        tRefresh.Stop()
        RefreshTiles(Nothing)
        tRefresh.Start()
    End Sub

    'Gives focus to the selected dashboard
    Private Sub SelectDashboard(dashNode As TreeNode, Optional refreshFromServer As Boolean = True)
        mViewingNode = dashNode
        Dim dash As Dashboard = DirectCast(dashNode.Tag, Dashboard)

        If refreshFromServer Then

            Try
                dash.Tiles = gSv.GetDashboardTiles(dash.ID)
            Catch ex As Exception
                ShowError(My.Resources.ctlDashboard_FailedToRetrieveTilesForDashboard, ex)
                Return
            End Try
        End If

        Dim tileList As New Dictionary(Of Guid, Size)
        For Each tile As DashboardTile In dash.Tiles
            tileList.Add(tile.Tile.ID, tile.Size)
        Next

        tRefresh.Stop()
        tileView.InitForView(tileList)
        RefreshTiles(Nothing)
        SetTitleBar(dashNode)
        tRefresh.Start()
    End Sub

    'Creates a new dashboard node
    Private Sub CreateDashboard(type As DashboardTypes)
        If type = DashboardTypes.Personal And Not mPersonalNode.IsExpanded() Then
            mPersonalNode.Expand()
        ElseIf type = DashboardTypes.Global And Not mGlobalNode.IsExpanded() Then
            mGlobalNode.Expand()
        ElseIf type = DashboardTypes.Published And Not mPublishedNode.IsExpanded() Then
            mPublishedNode.Expand()
        End If
        EditDashboard(AddDashboard(type, New Dashboard(type, Guid.NewGuid(), My.Resources.ctlDashboard_NewDashboard)), True)
    End Sub

    'Switches the dashboard control to edit mode
    Private Sub EditDashboard(node As TreeNode, create As Boolean)
        RefreshToolStripMenuItem.Enabled = False
        EditingNode = node
        mViewTree.SelectedNode = node
        mViewingNode = node

        Dim dash As Dashboard = CType(EditingNode.Tag, Dashboard)
        If create Then
            tileView.InitForCreate()
        Else
            tileView.InitForEdit()
        End If
        SetTitleBar(EditingNode)
    End Sub

    'Handles save/cancel dashboard edit
    Private Sub EndEditDashboard(saveChanges As Boolean)
        Dim dash As Dashboard = CType(EditingNode.Tag, Dashboard)
        Dim oldName As String = dash.Name
        If saveChanges Then
            If txtDashboardTitle.Text = String.Empty Then
                UserMessage.Show(My.Resources.ctlDashboard_PleaseEnterANameForThisDashboard)
                txtDashboardTitle.Focus()
                Return
            ElseIf txtDashboardTitle.Text.Length > 100 Then
                UserMessage.Show(My.Resources.ctlDashboard_DashboardNameCannotBeGreaterThan100Characters)
                txtDashboardTitle.Focus()
                Return
            ElseIf DashboardNameInUse(txtDashboardTitle.Text) Then
                UserMessage.Show(String.Format(My.Resources.ctlDashboard_A0DashboardAlreadyExistsWithTheName1,
                                dash.Type.ToString.ToLower,
                                txtDashboardTitle.Text))
                txtDashboardTitle.Focus()
                Return
            End If
            dash.Name = EditingNode.Text
            dash.Tiles.Clear()
            For Each tile As KeyValuePair(Of Guid, Size) In tileView.GetTiles()
                Dim dashTile As New DashboardTile()
                dashTile.Tile = gSv.GetTileDefinition(tile.Key)
                dashTile.Size = tile.Value
                dash.Tiles.Add(dashTile)
            Next
            If dash.Tiles.Count = 0 Then
                If Not Confirm(My.Resources.ctlDashboard_NoTilesHaveBeenSelectedTheBluePrismLogoWillBeDisplayedInsteadDoYouWantToContinue) Then Return
            End If
            If tileView.IsNewDashboard Then
                gSv.CreateDashboard(dash)
            Else
                gSv.UpdateDashboard(dash)
                If dash.Type = DashboardTypes.Published AndAlso
                 Not dash.Name.Equals(oldName) AndAlso
                 mAreDashboardsDGEnabled Then
                    Dim popup = New PopupForm(My.Resources.ctlDashboard_PublishedDashboardsChanged,
                                               My.Resources.ctlDashboard_DataGatewaysConfigMayNeedEditing,
                                               My.Resources.btnOk)
                    AddHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
                    popup.ShowInTaskbar = False
                    popup.ShowDialog()
                End If
            End If

        Else
            If Not Confirm(My.Resources.ctlDashboard_AnyChangesToThisDashboardWillBeDiscardedAreYouSure) Then Return

            If tileView.IsNewDashboard Then
                EditingNode.Parent.Nodes.Remove(EditingNode)
                EditingNode = Nothing
                mViewTree.SelectedNode = mHomePageNode
            Else
                DashboardTitleChanged(dash.Name)
            End If
        End If

        mTreeTab.SelectedTab = mViewTab
        EditingNode = Nothing
        Dim currentNode As TreeNode = mViewTree.SelectedNode
        mViewTree.Sort()
        mViewTree.SelectedNode = currentNode
        SelectDashboard(mViewTree.SelectedNode)
        mEditCache.Clear()
        RefreshToolStripMenuItem.Enabled = True

    End Sub

    'Deletes the current dashboard
    Private Async Function DeleteDashboard(node As TreeNode) As Task
        If Not Confirm(My.Resources.ctlDashboard_AreYouSureYouWantToDeleteThisDashboard) Then Return
        Try
            Dim dash As Dashboard = CType(node.Tag, Dashboard)
            gSv.DeleteDashboard(dash)
        Catch ex As Exception
            ShowError(My.Resources.ctlDashboard_FailedToDeleteDashboard, ex)
        End Try

        Try
            Await LoadDashboards()
        Catch ex As Exception
            ShowError(My.Resources.ctlDashboard_FailedToRetrieveDashboardViews, ex)
        End Try

        If CType(node.Tag, Dashboard).Type = DashboardTypes.Published Then
            Dim popup = New PopupForm(My.Resources.ctlDashboard_PublishedDashboardsChanged,
                                       My.Resources.ctlDashboard_DataGatewaysConfigMayNeedEditing,
                                       My.Resources.btnOk)
            AddHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
            popup.ShowInTaskbar = False
            popup.ShowDialog()
        End If
        mViewTree.SelectedNode = mHomePageNode
    End Function

    'Copies the selected dashboard
    Private Sub CopyDashboard(type As DashboardTypes)
        If mViewTree.SelectedNode Is Nothing Then Exit Sub

        Dim fromDash As Dashboard = CType(mViewTree.SelectedNode.Tag, Dashboard)
        Dim toDash As New Dashboard(type, Guid.NewGuid(), String.Format(My.Resources.ctlDashboard_CopyOf0, fromDash.Name))

        Dim tileList As New Dictionary(Of Guid, Size)
        For Each dashTile As DashboardTile In fromDash.Tiles
            tileList.Add(dashTile.Tile.ID, dashTile.Size)
        Next

        EditingNode = AddDashboard(type, toDash)
        mViewTree.SelectedNode = EditingNode

        tileView.InitForCreate(tileList)
        RefreshTiles(tileList)

        SetTitleBar(EditingNode)
        RefreshToolStripMenuItem.Enabled = False
    End Sub
#End Region

#Region "Dashboard tree event handlers"

    Private Sub mViewTree_BeforeSelect(sender As Object, e As TreeViewCancelEventArgs) _
     Handles mViewTree.BeforeSelect
        'Disallow selection of group nodes
        If (Editing() AndAlso e.Node IsNot EditingNode) Then
            e.Cancel = True
        End If
    End Sub

    Private Sub mViewTree_AfterSelect(sender As Object, e As TreeViewEventArgs) _
     Handles mViewTree.AfterSelect
        'Load and select dashboard
        If NodeIsDashboard(e.Node) AndAlso Not Editing() Then
            SelectDashboard(e.Node)
        End If
    End Sub

    Private Sub mViewTree_AfterExpand(sender As Object, e As TreeViewEventArgs) _
     Handles mViewTree.AfterExpand
        'Switch image to open group
        e.Node.ImageKey = "FOPEN"
        e.Node.SelectedImageKey = "FOPEN"
        If Not mExpandedGroups.Contains(e.Node.Name) Then _
         mExpandedGroups.Add(e.Node.Name)
    End Sub

    Private Sub mViewTree_AfterCollapse(sender As Object, e As TreeViewEventArgs) _
     Handles mViewTree.AfterCollapse
        'Switch image to closed group
        e.Node.ImageKey = "FCLOSED"
        e.Node.SelectedImageKey = "FCLOSED"
        If mExpandedGroups.Contains(e.Node.Name) Then _
         mExpandedGroups.Remove(e.Node.Name)
    End Sub

    Private Sub mViewTree_NodeMouseClick(sender As Object, e As System.Windows.Forms.TreeNodeMouseClickEventArgs) _
     Handles mViewTree.NodeMouseClick
        If e.Button = System.Windows.Forms.MouseButtons.Right Then
            mViewTree.SelectedNode = e.Node
        End If
    End Sub

#End Region

#Region "Dashboard tree context menu"

    Private Sub mViewMenu_Opening(sender As Object, e As CancelEventArgs) _
     Handles mDashboardMenu.Opening

        'Clear context menu options
        InitialiseMenu(mDashboardMenu, True)
        If mViewTree.SelectedNode Is Nothing OrElse Editing() Then Return

        'New dashboard(s)
        mnuNewPersonalDashboard.Enabled = CanEditPersonal()
        mnuNewGlobalDashboard.Enabled = CanEditGlobal()
        mnuNewPublishedDashboard.Enabled = CanEditPublished()

        'Expand/collapse
        mnuExpandAll.Enabled = True
        mnuCollapseAll.Enabled = True

        If Not NodeIsDashboard(mViewTree.SelectedNode) Then Return

        'Edit selected dashboard
        If CanEditDashboard() Then mnuEditDashboard.Enabled = True

        'Set as homepage
        If CType(mHomePageNode.Tag, Dashboard).ID <>
         CType(mViewTree.SelectedNode.Tag, Dashboard).ID Then mnuSetHomePage.Enabled = True

        ' Disable the 'set as home page' if the selected dashboard is published
        mnuSetHomePage.Enabled = (CType(mViewTree.SelectedNode.Tag, Dashboard).Type <> DashboardTypes.Published)

        'Copy to...
        mnuCopyAsPersonal.Enabled = CanEditPersonal()
        mnuCopyAsGlobal.Enabled = CanEditGlobal()
        mnuCopyAsPublished.Enabled = CanEditPublished()

        'Delete selected dashboard
        If CanDeleteDashboard() Then mnuDeleteDashboard.Enabled = True

    End Sub

    Private Sub mnuNewPersonalView_Click(sender As Object, e As EventArgs) _
        Handles mnuNewPersonalDashboard.Click

        'Create new personal dashboard
        CreateDashboard(DashboardTypes.Personal)
    End Sub

    Private Sub mnuNewGlobalView_Click(sender As Object, e As EventArgs) _
        Handles mnuNewGlobalDashboard.Click

        'Create new global dashboard
        CreateDashboard(DashboardTypes.Global)
    End Sub
    Private Sub mnuNewPublishedView_Click(sender As Object, e As EventArgs) _
        Handles mnuNewPublishedDashboard.Click

        'Create new published dashboard
        CreateDashboard(DashboardTypes.Published)
    End Sub

    Private Sub mnuEditView_Click(sender As Object, e As EventArgs) _
     Handles mnuEditDashboard.Click
        'Edit selected dashboard
        EditDashboard(mViewTree.SelectedNode, False)
    End Sub

    Private Sub mnuSetHomePage_Click(sender As Object, e As EventArgs) _
     Handles mnuSetHomePage.Click
        'Set current dasboard as home page
        SetHomePage(CType(mViewTree.SelectedNode.Tag, Dashboard))
    End Sub

    Private Sub mnuExpandAll_Click(sender As Object, e As EventArgs) _
        Handles mnuExpandAll.Click
        mViewTree.ExpandAll()
    End Sub

    Private Sub mnuCollapseAll_Click(sender As Object, e As EventArgs) _
    Handles mnuCollapseAll.Click
        Dim currNode = mViewTree.SelectedNode
        mViewTree.CollapseAll()
        If currNode.Parent IsNot Nothing Then _
            mViewTree.SelectedNode = currNode.Parent Else _
            mViewTree.SelectedNode = currNode
    End Sub

    Private Sub mnuCopyToPersonal_Click(sender As Object, e As EventArgs) _
        Handles mnuCopyAsPersonal.Click

        'Create a new personal dashboard from the selected
        CopyDashboard(DashboardTypes.Personal)
    End Sub

    Private Sub mnuCopyToGlobal_Click(sender As Object, e As EventArgs) Handles mnuCopyAsGlobal.Click
        'Create a new global dashboard from the selected
        CopyDashboard(DashboardTypes.Global)
    End Sub

    ''' <summary>
    ''' Copy the current dashboard as a published dashboard.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub mnuCopyToPublished_Click(sender As Object, e As EventArgs) Handles mnuCopyAsPublished.Click
        'Create a new Published dashboard from the selected
        CopyDashboard(DashboardTypes.Published)
    End Sub

    Private Async Sub mnuDeleteView_Click(sender As Object, e As EventArgs) _
     Handles mnuDeleteDashboard.Click
        'Delete selected dashboard
        Await DeleteDashboard(mViewTree.SelectedNode)
    End Sub

#End Region

#Region "Tile tree methods"

    ''' <summary>
    ''' Instruct treeview control to refresh
    ''' </summary>
    Private Sub RefreshViewHandle() Handles TilesGroupTreeControl.RefreshView
        TilesGroupTreeControl.UpdateView(True)
    End Sub

    ''' <summary>
    ''' Handles a tile create request being generated from the group tree control.
    ''' </summary>
    Private Sub HandleCreateTile(sender As Object, e As CreateGroupMemberEventArgs) _
     Handles TilesGroupTreeControl.CreateRequested
        ' Show a blank dashboard tile form to create the new tile
        Using frm As New frmDashboardTile()
            frm.SetEnvironmentColours(Me)
            frm.ShowInTaskbar = False
            frm.ShowDialog()

            ' If the user cancelled, skip now
            If frm.DialogResult <> DialogResult.OK Then Return

            ' Create a group member from the tile to pass back to the sender
            e.CreatedItem = New TileGroupMember(frm.TileValue)
        End Using
    End Sub

    Private Sub HandleEditTile(sender As Object, e As GroupMemberEventArgs) _
     Handles TilesGroupTreeControl.EditRequested
        ProcessTileActivated(e.Target)
    End Sub

    'Implements the Edit Tile action
    Private Function EditTile(id As Guid) As Tile
        Dim tile As Tile = Nothing
        Try
            tile = gSv.GetTileDefinition(id)
            'cache the current tile
            mEditCache.TryAdd(id, New Tile(tile))

        Catch ex As Exception
            ShowError(My.Resources.ctlDashboard_FailedToRetrieveTileDefinition, ex)
            Return Nothing
        End Try

        Using frm As New frmDashboardTile(False, Nothing, tile)
            frm.SetEnvironmentColours(Me)
            frm.ShowInTaskbar = False
            frm.ShowDialog()
            If frm.DialogResult <> DialogResult.OK Then Return Nothing
            Return frm.TileValue
        End Using
    End Function

    'Implements the View Tile action
    Private Sub ViewTile(id As Guid)
        Dim tile As Tile = Nothing
        Try
            tile = gSv.GetTileDefinition(id)
        Catch ex As Exception
            ShowError(My.Resources.ctlDashboard_FailedToRetrieveTileDefinition, ex)
            Return
        End Try

        Using frm As New frmDashboardTile(True, Nothing, tile)
            frm.SetEnvironmentColours(Me)
            frm.ShowInTaskbar = False
            frm.ShowDialog()
        End Using
    End Sub

    ''' <summary>
    ''' Handles the copying of the currently selected tile. No-ops if the currently
    ''' selected node is not a tile or if no node is selected.
    ''' </summary>
    Private Sub HandleCopyTile(sender As Object, e As CloneGroupMemberEventArgs) _
     Handles TilesGroupTreeControl.CloneRequested
        If TypeOf e.Original Is TileGroupMember Then _
         e.CreatedItem = New TileGroupMember(gSv.CopyTile(e.Original.IdAsGuid))
    End Sub

    ''' <summary>
    ''' Handles the copying of the currently selected tile. No-ops if the currently
    ''' selected node is not a tile or if no node is selected.
    ''' </summary>
    Private Sub HandleDeleteTile(sender As Object, e As GroupMemberEventArgs) _
     Handles TilesGroupTreeControl.DeleteRequested
        Dim tileMem = TryCast(e.Target, TileGroupMember)
        If tileMem Is Nothing Then Return

        Dim gp As IGroup = tileMem.Owner
        If gp Is Nothing Then Return

        Dim msg As String
        Try
            If gSv.IsTileInUse(tileMem.IdAsGuid) Then
                msg = My.Resources.ctlDashboard_ThisTileIsCurrentlyDisplayedOnOneOrMoreDashboardsAreYouSureYouWantToDeleteIt
            Else
                msg = My.Resources.ctlDashboard_AreYouSureYouWantToDeleteThisTile
            End If
            If Not Confirm(msg) Then Return

            gSv.DeleteTile(tileMem.IdAsGuid)
            TilesGroupTreeControl.UpdateView(True)

        Catch ex As Exception
            ShowError(My.Resources.ctlDashboard_FailedToDeleteTile, ex)

        End Try

        If Not Editing() AndAlso mViewingNode IsNot Nothing Then _
            SelectDashboard(mViewingNode)
    End Sub

    ''' <summary>
    ''' Handles an item being activated in the group member tree representing the
    ''' tiles, ie. a tile being double-clicked or 'enter' pressed on it.
    ''' </summary>
    Private Sub HandleTileActivated(sender As Object, e As GroupMemberEventArgs) _
     Handles TilesGroupTreeControl.ItemActivated
        'Double-click on tile node launches tile editor (in view mode if necessary)
        TilesGroupTreeControl.EditItem()
        e.CancelUpdate = True
    End Sub

    ''' <summary>
    ''' Process the tile update and screen refresh
    ''' </summary>
    ''' <param name="target"></param>
    Private Sub ProcessTileActivated(ByRef target As IGroupMember)
        If CanEditTiles() Then
            Dim tile = EditTile(target.IdAsGuid)

            ' If the tile was edited, update the target member with the new data
            If tile IsNot Nothing Then

                With DirectCast(target, TileGroupMember)
                    .Name = tile.Name
                    .Description = tile.Description
                    .TileType = tile.Type
                End With

                If Not Editing() AndAlso mViewingNode IsNot Nothing Then
                    SelectDashboard(mViewingNode)
                Else
                    'Refresh the tile, which has not been stored yet.
                    RefreshDashboardTile(tile, target.IdAsGuid)
                    RefreshDashboard(mViewingNode, False)
                End If
            End If
        Else
            ViewTile(target.IdAsGuid)
        End If
    End Sub

    Private Sub RefreshDashboardTile(tile As Tile, tileGuid As Guid)
        If mViewingNode IsNot Nothing Then
            Dim dashboard As Dashboard = DirectCast(mViewingNode.Tag, Dashboard)
            Dim dashboardTile = dashboard.Tiles.FirstOrDefault(Function(t)
                                                                   Return t.Tile.ID.Equals(tileGuid)
                                                               End Function)
            If dashboardTile IsNot Nothing Then

                With dashboardTile
                    .Tile.Name = tile.Name
                    .Tile.Description = tile.Description
                    .Tile.XMLProperties = tile.XMLProperties
                    .Tile.Type = tile.Type
                End With
            End If
        End If
    End Sub

#End Region

#Region "Tile Rendering"

    Private Sub RefreshTiles(addedTiles As Dictionary(Of Guid, Size),
                             Optional skippedTiles As List(Of Guid) = Nothing,
                             Optional deletedTiles As List(Of Guid) = Nothing)
        If mViewingNode Is Nothing Then Exit Sub

        'Add any new tiles to dashboard
        Dim dash As Dashboard = CType(mViewingNode.Tag, Dashboard)
        If addedTiles IsNot Nothing Then
            For Each tile As KeyValuePair(Of Guid, Size) In addedTiles
                Dim dashTile As New DashboardTile()
                dashTile.Tile = gSv.GetTileDefinition(tile.Key)
                dashTile.Size = tile.Value
                dash.Tiles.Add(dashTile)
            Next
        End If

        'Remove any deleted tiles (and don't bother refreshing)
        If deletedTiles IsNot Nothing Then
            dash.Tiles.RemoveAll(Function(t) deletedTiles.Contains(t.Tile.ID))
            Return
        End If

        'Refresh passed list of tiles (or all if list is empty)
        Dim now As Date = Date.Now
        Dim skippedTileNames As New List(Of String)
        For Each tile As DashboardTile In dash.Tiles
            If addedTiles Is Nothing OrElse addedTiles.ContainsKey(tile.Tile.ID) Then
                RefreshTile(tile, now)
            End If
            If skippedTiles IsNot Nothing AndAlso skippedTiles.Contains(tile.Tile.ID) Then
                skippedTileNames.Add(tile.Tile.Name)
            End If
        Next

        'Report any tiles that were not added
        If skippedTileNames.Count = 0 Then
            Return
        ElseIf skippedTileNames.Count = 1 Then
            UserMessage.OK(String.Format(
             My.Resources.Tile0WasNotAddedBecauseItAlreadyExistsOnThisDashboard,
             skippedTileNames(0)))
        Else
            UserMessage.OK(LTools.Format(
             My.Resources.Tile012WereNotAddedBecauseTheyAlreadyExistOnThisDashboard,
             "NAME", skippedTileNames(0),
             "COUNT", skippedTileNames.Count - 1))
        End If

    End Sub

    Private Sub RefreshTile(tile As DashboardTile, now As Date)
        'Refresh the tile
        Dim chart As New ChartTile(tile.Tile)
        tileView.RefreshTile(tile.Tile.ID, chart.Build(tile.Size))
        tile.LastRefreshed = now
    End Sub

    Private Sub tRefresh_Tick(sender As Object, e As EventArgs) Handles tRefresh.Tick
        'Handles auto-refresh
        If Editing() OrElse mViewingNode Is Nothing Then Exit Sub

        Dim now As Date = Date.Now
        Dim tileList As New Dictionary(Of Guid, Size)
        For Each tile As DashboardTile In CType(mViewingNode.Tag, Dashboard).Tiles
            If tile.Tile.RefreshInterval > 0 Then
                If tile.LastRefreshed = Date.MinValue OrElse
                 tile.LastRefreshed.AddSeconds(tile.Tile.RefreshInterval) <= now Then
                    RefreshTile(tile, now)
                End If
            End If
        Next

    End Sub

#End Region

#Region "Title/Picturebox button events"

    Private Sub txtDashboardTitle_TextChanged(sender As Object, e As EventArgs) _
     Handles txtDashboardTitle.TextChanged
        DashboardTitleChanged(txtDashboardTitle.Text)
    End Sub

    Private Sub SaveToolStripMenuItem_Click(sender As Object, e As EventArgs) _
        Handles SaveToolStripMenuItem.Click
        If Editing() Then
            EndEditDashboard(True)
        End If
    End Sub

    Private Sub EditToolStripMenuItem_Click(sender As Object, e As EventArgs) _
     Handles EditToolStripMenuItem.Click
        If Not Editing() Then
            EditDashboard(mViewingNode, False)
        End If
    End Sub

    Private Sub CloseToolStripMenuItem_Click(sender As Object, e As EventArgs) _
     Handles CloseToolStripMenuItem.Click
        If Editing() Then
            EndEditDashboard(False)
        End If
    End Sub

    Private Sub RefreshToolStripMenuItem_Click(sender As Object, e As EventArgs) _
     Handles RefreshToolStripMenuItem.Click
        SelectDashboard(mViewingNode, True)
    End Sub

#End Region

#Region "Utility functions"

    'Returns true if the tree node is a dashboard
    Private Function NodeIsDashboard(n As TreeNode) As Boolean
        If n IsNot Nothing AndAlso TypeOf (n.Tag) Is Dashboard Then Return True
        Return False
    End Function

    'Returns true if the selected dashboard is being edited
    Private Function Editing() As Boolean
        If EditingNode Is Nothing Then Return False
        Return True
    End Function

    'Returns dashboard type for selected node
    Private Function DashboardType() As DashboardTypes
        If NodeIsDashboard(mViewTree.SelectedNode) Then
            Return CType(mViewTree.SelectedNode.Tag, Dashboard).Type
        End If
        Return Nothing
    End Function

    'Returns true if user can edit the selected dashboard
    Private Function CanEditDashboard() As Boolean
        If mViewTree.SelectedNode Is Nothing Then Return False

        If DashboardType() = DashboardTypes.Published AndAlso CanEditPublished() Then Return True
        If DashboardType() = DashboardTypes.Global AndAlso CanEditGlobal() Then Return True
        If DashboardType() = DashboardTypes.Personal AndAlso CanEditPersonal() Then Return True
        Return False
    End Function

    'Returns true if the selected dashboard can be deleted
    Private Function CanDeleteDashboard() As Boolean
        If mViewTree.SelectedNode Is Nothing Then Return False
        If DashboardType() = DashboardTypes.Global AndAlso Not CanEditGlobal() Then Return False
        If DashboardType() = DashboardTypes.Personal AndAlso Not CanEditPersonal() Then Return False

        'Cannot delete the default global dashboard
        Dim selDash As Dashboard = CType(mViewTree.SelectedNode.Tag, Dashboard)
        If selDash.ID = Guid.Empty Then Return False

        'Cannot delete user's own home page
        Dim homeDash As Dashboard = CType(mHomePageNode.Tag, Dashboard)
        If (selDash.ID = homeDash.ID) Then Return False

        Return True
    End Function

    'Returns true if user can edit personal dashboards
    Private Function CanEditPersonal() As Boolean
        Return User.Current.HasPermission(Analytics.DesignPersonalDashboards)
    End Function

    'Returns true if user can edit global dashboards
    Private Function CanEditGlobal() As Boolean
        Return User.Current.HasPermission(Analytics.DesignGlobalDashboards)
    End Function

    'Returns true if user can edit published dashboards
    Private Function CanEditPublished() As Boolean
        Return User.Current.HasPermission(Analytics.DesignPublishedDashboards)
    End Function

    'Returns true if user can edit tile library
    Private Function CanEditTiles() As Boolean
        Return User.Current.HasPermission(Analytics.CreateEditDeleteTiles)
    End Function

    'Returns true if user can edit at least one type of dashboard
    Private Function CanEditAtLeastOneDashboardType() As Boolean
        Return CanEditPersonal() OrElse CanEditGlobal() OrElse CanEditPublished()
    End Function

    'Returns true if user can view dashboards
    Private Function CanViewDashboards() As Boolean
        Return User.Current.HasPermission(Analytics.ViewDashboards)
    End Function

    'Handles the updating of the title bar
    Private Sub SetTitleBar(node As TreeNode)

        EditToolStripMenuItem.Enabled = False
        SaveToolStripMenuItem.Enabled = False
        CloseToolStripMenuItem.Enabled = False

        If node Is Nothing Then
            lblAreaTitle.Text = My.Resources.ctlDashboard_NoDashboardsAvailable
            Return
        End If

        If Not Editing() Then
            If node Is mHomePageNode Then
                lblAreaTitle.Text = My.Resources.ctlDashboard_HomePage & node.Text
            Else
                lblAreaTitle.Text = node.Parent.Text & My.Resources.ctlDashboard_Colin & node.Text
            End If
            txtDashboardTitle.Visible = False
            If CanEditDashboard() Then
                EditToolStripMenuItem.Enabled = True
            End If
        Else
            lblAreaTitle.Text = Nothing
            txtDashboardTitle.Visible = True
            txtDashboardTitle.Text = node.Text
            txtDashboardTitle.Focus()
            SaveToolStripMenuItem.Enabled = True
            CloseToolStripMenuItem.Enabled = True
        End If
    End Sub

    'Clears context menu ready for adding available options
    Private Sub InitialiseMenu(menu As ContextMenuStrip, visible As Boolean)
        For i As Integer = 0 To menu.Items.Count - 1
            menu.Items(i).Visible = visible
            If TypeOf menu.Items(i) Is ToolStripMenuItem Then
                CType(menu.Items(i), ToolStripMenuItem).Enabled = False
            End If
        Next
    End Sub

    'Returns true if user has confirmed action
    Private Function Confirm(msg As String, ParamArray args() As Object) As Boolean
        Return (UserMessage.YesNo(msg, args) = MsgBoxResult.Yes)
    End Function

    'Display error to user
    Private Sub ShowError(msg As String, ex As Exception)
        UserMessage.Show(msg & My.Resources.ctlDashboard_Hyphen & ex.Message, ex)
    End Sub

    'Checks if dashboard name is used in the tree
    Private Function DashboardNameInUse(name As String) As Boolean
        Dim selectedNode As New TreeNode
        Dim nodelist As TreeNodeCollection
        Select Case DashboardType()
            Case DashboardTypes.Personal
                selectedNode = mPersonalNode
            Case DashboardTypes.Global
                selectedNode = mGlobalNode
            Case DashboardTypes.Published
                selectedNode = mPublishedNode
            Case Else
                Return True
        End Select
        nodelist = selectedNode.Nodes
        For Each n As TreeNode In nodelist
            If CType(n.Tag, Dashboard).ID <> CType(EditingNode.Tag, Dashboard).ID Then
                If String.Compare(n.Text, name, True) = 0 Then Return True
            End If
        Next
        Return False
    End Function

    'Handle dashboard renaming
    Private Sub DashboardTitleChanged(newName As String)
        If mHomePageNode.Name = EditingNode.Name Then mHomePageNode.Text = newName
        For Each n As TreeNode In mPersonalNode.Nodes
            If n.Name = EditingNode.Name Then n.Text = newName
        Next
        For Each n As TreeNode In mGlobalNode.Nodes
            If n.Name = EditingNode.Name Then n.Text = newName
        Next
        For Each n As TreeNode In mPublishedNode.Nodes
            If n.Name = EditingNode.Name Then n.Text = newName
        Next
    End Sub

    Private Sub HandleOnBtnOKClick(sender As Object, e As EventArgs)
        Dim popup = CType(sender, PopupForm)
        RemoveHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
        popup.Close()
    End Sub

#End Region

#Region "Interface implementations"

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(Permission.Analytics.GroupName)
        End Get
    End Property

    Private WithEvents mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        If Editing() Then
            If Not Confirm(My.Resources.ctlDashboard_AnyChangesTo0WillBeDiscardedAreYouSure, txtDashboardTitle.Text) Then
                Return False
            End If
            'revert any changes if required
            RevertTileChanges()
        End If
        EditingNode = Nothing
        Return True
    End Function

    Private Sub RevertTileChanges()
        For Each keyValue In mEditCache
            Dim title = keyValue.Value
            gSv.UpdateTile(title, title.Name, title.XMLProperties)
        Next
        mEditCache.Clear()
    End Sub

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "dashboards-ui.htm"
    End Function

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color _
     Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return pnlArea.BackColor
        End Get
        Set(value As Color)
            pnlArea.BackColor = value
            mMenuButton.BackColor = value
        End Set
    End Property

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
     Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return pnlArea.ForeColor
        End Get
        Set(value As Color)
            pnlArea.ForeColor = value
        End Set
    End Property

    Public Sub RefreshView() Implements IRefreshable.RefreshView
        RefreshTiles(Nothing)
    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Handles the loading of the dashboard data from the database and the
    ''' organisation of the data once loaded
    ''' </summary>
    Protected Overrides Async Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        If DesignMode Then Return

        'Create handler for tile refresh event
        AddHandler tileView.RefreshTilesEvent, AddressOf RefreshTiles

        'Reveal tabs depending on user permissions
        mTreeTab.TabPages.Clear()

        If CanViewDashboards() OrElse CanEditAtLeastOneDashboardType() Then
            'Load available dashboard views
            mTreeTab.TabPages.Add(mViewTab)
        Else
            'Initialise empty title bar
            SetTitleBar(Nothing)
        End If

        If CanEditTiles() OrElse CanEditAtLeastOneDashboardType() Then
            'Load tile library
            mTreeTab.TabPages.Add(mTileTab)

            Dim loadedTiles As IGroupTree = GetGroupStore().GetTree(GroupTreeType.Tiles, Nothing, Nothing, False, False, False)
            TilesGroupTreeControl.AddTree(loadedTiles, True)
            TilesGroupTreeControl.UpdateView()
        End If

        If CanViewDashboards() OrElse CanEditAtLeastOneDashboardType() Then
            'Loading dashboards here so if an exception is thrown when loading the control, the background is not black

            Try
                Await LoadDashboards()
            Catch ex As Exception
                ShowError(My.Resources.ctlDashboard_FailedToRetrieveDashboardViews, ex)
            End Try

            'Start auto-refresh timer
            tRefresh.Start()
        End If
    End Sub

    Private Sub Parent_ImportCompleted() Handles mParent.ImportCompleted
        If CanEditTiles() OrElse CanEditAtLeastOneDashboardType() Then
            TilesGroupTreeControl.ClearGroupCache()
            BeginInvoke(New FunctionDelegate(AddressOf TilesGroupTreeControl.UpdateView))
        End If
    End Sub
    Private Delegate Sub FunctionDelegate()
    Private Sub ClearCacheOnGroupNameChange() Handles TilesGroupTreeControl.NameChanged
        TilesGroupTreeControl.ClearGroupCache()
    End Sub

#End Region

End Class
