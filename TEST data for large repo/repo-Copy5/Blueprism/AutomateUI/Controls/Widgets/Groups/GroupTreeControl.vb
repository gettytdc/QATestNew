Imports AutomateControls
Imports BluePrism.Images
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib
Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports AutomateUI.Classes.UserInterface
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Core.Collections
Imports BluePrism.Utilities.Functional
Imports BluePrism.AutomateAppCore.GroupPermissionLogic
Imports BluePrism.AutomateAppCore.Resources.ResourceMachine
Imports NLog
Imports AutomateControls.Trees
Imports BluePrism.Core.Utility
Imports BluePrism.Core.Extensions
Imports BluePrism.Server.Domain.Models
Imports AutomateControls.Forms

''' <summary>
''' A tree control on which groups and their members are displayed
''' </summary>
Public Class GroupTreeControl

#Region " Class-scope Declarations "

    ''' <summary>
    ''' Placeholder for new items created in the tree, before they have been created
    ''' (ie. before they have a name given to them by the user)
    ''' </summary>
    Private Class NewItemPlaceholder
        Public Property Type As GroupMemberType
    End Class

    ''' <summary>
    ''' Placeholder for new groups created in the tree, before they have been created
    ''' (ie. before they have a name given to them by the user)
    ''' </summary>
    Private Class NewGroupPlaceholder
    End Class

#End Region

#Region " Published Events "

    ''' <summary>
    ''' Event fired when a group is selected.
    ''' </summary>
    Public Event GroupSelected As EventHandler

    ''' <summary>
    ''' Event fired when an item is selected - ie. a group member which is not a
    ''' group. Note that if a group node is selected, a <see cref="GroupSelected"/>
    ''' event is fired <em>instead of</em> this event.
    ''' </summary>
    Public Event ItemSelected As EventHandler

    ''' <summary>
    ''' Cause the screen to refresh.
    ''' </summary>
    Public Event RefreshView As EventHandler

    ''' <summary>
    ''' Event fired when another node has been requested to be selected, before the
    ''' current member is deselected.
    ''' </summary>
    Public Event MemberDeselecting As CancelEventHandler

    ''' <summary>
    ''' Event fired when a name change is about to be committed
    ''' </summary>
    Public Event NameChanging As NameChangingEventHandler

    ''' <summary>
    ''' Event fired when a name change has been committed.
    ''' </summary>
    Public Event NameChanged As NameChangedEventHandler

    ''' <summary>
    ''' Event fired when an item creation has been requested by the user
    ''' </summary>
    Public Event CreateRequested As CreateGroupMemberEventHandler

    ''' <summary>
    ''' Event fired when an item clone has been requested by the user
    ''' </summary>
    Public Event CloneRequested As CloneGroupMemberEventHandler

    Public Event AddingNodeToTree As AddGroupToTreeNodeEventHandler

    ''' <summary>
    ''' Event fired when a 'delete' request is made within this control
    ''' </summary>
    Public Event DeleteRequested As GroupMemberEventHandler

    Public Event EditRequested As GroupMemberEventHandler

    ''' <summary>
    ''' Event fired when a group member of a collection of group members is dropped
    ''' onto a group member within this control. Note that the
    ''' <see cref="GroupMemberDropEventArgs.Contents">contents</see> of this event
    ''' include only those group members which were processed by this control - if
    ''' any were not processed for any reason, they are not included in this event.
    ''' </summary>
    Public Event GroupMemberDropped As GroupMemberDropEventHandler

    ''' <summary>
    ''' Event fired when a member is being dropped which allows the code to show
    ''' a dialog and stop the drop if necessary.
    ''' </summary>
    Public Event GroupMemberRequestDrop As GroupMemberRequestDropEventHandler

    ''' <summary>
    ''' Event fired when a context menu is opening, giving a listener the chance to
    ''' enable/disable context menu items before it does.
    ''' </summary>
    ''' <remarks>
    ''' Note that the GroupTreeControl-specific items in the menu have already been
    ''' processed at the time this event is thrown and are not directly available for
    ''' changing state - only really those registered in the
    ''' <see cref="ExtraContextMenuItems"/> collection and, presumably, saved as
    ''' member variables in the object which created those items.
    ''' </remarks>
    Public Event ContextMenuOpening As GroupMemberContexMenuOpeningEventHandler

    ''' <summary>
    ''' Event fired when an item - ie. a group member other than a group - is
    ''' activated, either by double clicking or by pressing the enter key while the
    ''' tree has focus and the item is selected.
    ''' </summary>
    Public Event ItemActivated As GroupMemberEventHandler

    Public Event NodeDoubleClick As TreeNodeMouseClickEventHandler

#End Region

#Region " Member Variables "

    ' Map of group nodes with the treenode which acts as the root, or null if the
    ' root is not displayed, and thus there is no treenode for the root
    Private mTrees As IDictionary(Of IGroupTree, TreeNode)

    ' The member which is considered to be the target of a context menu operation
    ' Transitory and should be set up/torn down when an operation initiates
    Private mTarget As IGroupMember

    ' The last clicked treenode - even if selection changes programmatically, this
    ' remains set as the last node clicked by the user
    Private mLastClick As TreeNode

    ' Whether the creation of items is allowed by this control
    Private mItemCreateEnabled As Boolean

    ' The current tree in which a 'Create Member' command is being handled
    Private mCurrCreateMenuTree As IGroupTree

    ' The 'Create Member' menu items available, keyed on the corresponding tree.
    Private mCreateMenuItems As _
        New Dictionary(Of IGroupTree, ICollection(Of ToolStripMenuItem))

    ' The extra context menu items to append to the context menu for this control
    Private WithEvents mExtraContextMenuItems _
        As ObservableCollection(Of ToolStripMenuItem)

    ' Flag indicating that a 'BeforeSelect' handler is in progress
    Private mHandlingBeforeSelect As Boolean

    ' Flag indicating if a drag operation is currently ongoing
    Private mDragging As Boolean

    ' The currently highlighted node, if there is one
    Private mHighlightedNode As TreeNode

    ' The backcolor of the highlighted node before it was highlighted
    Private mSavedBackColor As Color

    ' The forecolor of the highlighted node before it was highlighted
    Private mSavedForeColor As Color

    Private ReadOnly mBinarySearchProvider As IBinarySearchProvider = New BinarySearchProvider()

    ' The logic class to apply the correct permissions to group moves
    Private mGroupPermissionLogic As GroupPermissionLogic

    Private mEnvColours As IEnvironmentColourManager

    ' A cache to store group information.  Cache for one of the most expensive operations
    Private ReadOnly mGroupInfoCache As Dictionary(Of IGroup, List(Of GroupInfo)) = New Dictionary(Of IGroup, List(Of GroupInfo))

    Private ReadOnly mTypesThatDoNotStoreReload As List(Of Type) = New List(Of Type) _
        From {GetType(ProcessGroupMember),
        GetType(ObjectGroupMember)}

    Private mIsFiltered As Boolean = False

    ' A collection of selected nodes, used when MultiSelect is enabled
    Private mSelectedNodes As List(Of TreeNode) = New List(Of TreeNode)

    ' The first node in the collection
    Private mFirstNode As TreeNode

    Private mAddingTree As Boolean = False
#End Region

#Region " Auto-properties "

    ''' <summary>
    ''' Property indicating whether item edits are supported by this control
    ''' </summary>
    <Browsable(True), Category("Behavior"), DefaultValue(False), Description(
     "Allows items to be edited by this control")>
    Public Property ItemEditEnabled As Boolean = False

    ''' <summary>
    ''' Property indicating whether item clones are supported by this control
    ''' </summary>
    <Browsable(True), Category("Behavior"), DefaultValue(False), Description(
     "Allows items to be renamed by this control")>
    Public Property ItemCloneEnabled As Boolean = False

    ''' <summary>
    ''' Property indicating whether item renames are supported by this control
    ''' </summary>
    <Browsable(True), Category("Behavior"), DefaultValue(False), Description(
     "Allows items to be renamed by this control")>
    Public Property ItemRenameEnabled As Boolean = False

    ''' <summary>
    ''' Property indicating whether item deletes are supported by this control
    ''' </summary>
    <Browsable(True), Category("Behavior"), DefaultValue(False), Description(
     "Allows items to be deleted by this control")>
    Public Property ItemDeleteEnabled As Boolean = False


    ''' <summary>
    ''' Property indicating whether item renames are supported by this control
    ''' </summary>
    <Browsable(True), Category("Behavior"), DefaultValue(True), Description(
     "Allows groups to be renamed by this control")>
    Public Property GroupRenameEnabled As Boolean = True

    ''' <summary>
    ''' Property indicating whether  renames are supported by this control
    ''' </summary>
    <Browsable(True), Category("Behavior"), DefaultValue(False), Description(
     "Controls whether items or groups can be modified within this control")>
    Public Property [ReadOnly] As Boolean = False

    <Browsable(True), Category("Behavior"), DefaultValue(False), Description(
        "Controls whether the 'Manage Access Rights' menu item should be available within this control")>
    Public Property ManageAccessRightsEnabled As Boolean = False

    <Browsable(True), Category("Behavior"), DefaultValue(True), Description(
        "Controls whether items can be cloned via ctrl + drag")>
    Public Property CloneDragEnabled As Boolean = True

    <Browsable(True), Category("Behavior"), DefaultValue(True), Description(
        "Controls whether items can be double clicked")>
    Public Property EnableDoubleClick As Boolean = True

      <Browsable(True), Category("Behavior"), DefaultValue(False), Description(
        "Controls whether multiple items can be selected")>
    Public Property EnableMultipleSelect As Boolean = False
#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty group tree control
    ''' </summary>
    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()



        ' Add any initialization after the InitializeComponent() call.
        mTrees = New clsOrderedDictionary(Of IGroupTree, TreeNode)

        tvGroups.ImageList = ImageLists.Components_16x16
        tvGroups.DrawMode = TreeViewDrawMode.OwnerDrawText

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets whether drag and drop is allowed on this control and, by
    ''' extension, the treeview held by this control
    ''' </summary>
    <Browsable(True), Category("Behavior"), Description(
    "Indicates whether the control can accept data that the user drags onto it")>
    Public Overrides Property AllowDrop As Boolean
        Get
            Return MyBase.AllowDrop
        End Get
        Set(value As Boolean)
            MyBase.AllowDrop = value
            tvGroups.AllowDrop = value
        End Set
    End Property
        
    ''' <summary>
    ''' Property indicating whether creating of items is offered and reported
    ''' by this control
    ''' </summary>
    <Browsable(True), Category("Behavior"), DefaultValue(False), Description(
     "Allows item creation events to be fired by this control")>
    Public Property ItemCreateEnabled As Boolean
        Get
            Return mItemCreateEnabled
        End Get
        Set(value As Boolean)
            If mItemCreateEnabled = value Then Return
            mItemCreateEnabled = value
            ' Ensure that the context menu is dynamically handled again in the future
            SetCreateItemMenuItems(Nothing)
        End Set
    End Property


    ''' <summary>
    ''' Property indicating whether the control should validate the moving or
    ''' copying of members within the tree against group permissions rules
    ''' </summary>
    <Browsable(True), Category("Behaviour"), DefaultValue(False), Description(
    "Allows group based permissions to be checked when moving/copying members")>
    Public Property ValidateMemberMovements As Boolean

    <Browsable(True), Category("Behaviour"), DefaultValue(False), Description(
    "Allows the control to save the groups which have been left expanded on a by user basis")>
    Public Property SaveExpandedGroups as Boolean

    ''' <summary>
    ''' Gets or sets the selected member in this treeview.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedMember As IGroupMember
        Get
            Dim n As TreeNode = tvGroups.SelectedNode
            If n Is Nothing Then Return Nothing
            Return TryCast(n.Tag, IGroupMember)
        End Get
        Set(value As IGroupMember)
            If value Is Nothing Then tvGroups.SelectedNode = Nothing : Return
            Dim n As TreeNode = GetNodeFor(value)
            If n IsNot Nothing Then tvGroups.SelectedNode = n
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the selected group. If the selected node is not a group, this
    ''' will return null.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedGroup As IGroup
        Get
            Return TryCast(SelectedMember, IGroup)
        End Get
        Set(value As IGroup)
            SelectedMember = value
        End Set
    End Property

    ''' <summary>
    ''' The group which has been selected, or which is the owner of the member which
    ''' has been selected.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend ReadOnly Property SelectedMembersGroup As IGroup
        Get
            Dim g As IGroup = SelectedGroup
            If g IsNot Nothing Then Return g
            Dim item As IGroupMember = SelectedMember
            If item IsNot Nothing Then Return item.Owner
            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Gets the root group of the selected member (whether displayed or not)
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Private ReadOnly Property SelectedRoot As IGroup
        Get
            Dim m As IGroupMember = SelectedMember
            Return If(m Is Nothing, Nothing, m.RootGroup)
        End Get
    End Property

    ''' <summary>
    ''' The group being hovered over (directly - ie. not the target group which may
    ''' or may not be being directly hovered over).
    ''' Setting this kicks off a hover timer which ticks over after a short period
    ''' and auto-expands the group that was set.
    ''' </summary>
    Private Property HoverGroup As IGroup
        Get
            Return DirectCast(timerGroupHover.Tag, IGroup)
        End Get
        Set(value As IGroup)
            If timerGroupHover.Tag Is value Then Return
            timerGroupHover.Tag = value
            ' We want to stop the timer if we're changing the target group, so
            ' disable it first
            timerGroupHover.Stop()
            ' Then enable it if we're setting to a group
            If value IsNot Nothing Then timerGroupHover.Start()
        End Set
    End Property

    ''' <summary>
    ''' Gets whether a drag operation initiated by this control is currently ongoing
    ''' or not. Note that a drag that was initiated by another control (eg. dragging
    ''' from another instance of GroupTreeControl into this instance) will *not*
    ''' cause this property to return true
    ''' </summary>
    Private ReadOnly Property IsDragging As Boolean
        Get
            Return mDragging
        End Get
    End Property

    ''' <summary>
    ''' Gets whether any create menu items have been created by this group tree
    ''' control and are available (ie. visible) to the user.
    ''' </summary>
    Private ReadOnly Property AnyCreateMenuItemsAvailable As Boolean
        Get
            Return ctxGroup.Items.OfType(Of CreateGroupMemberMenuItem).Any(
                Function(mi) mi.Available)
        End Get
    End Property

    ''' <summary>
    ''' Gets the target member of a context menu operation, or null if there is no
    ''' target member set. Note that this may be a member which is not represented
    ''' by a visual element in the treeview (eg. a root node when roots are not
    ''' visible).
    ''' </summary>
    Private ReadOnly Property TargetMember As IGroupMember
        Get
            Return mTarget
        End Get
    End Property

    ''' <summary>
    ''' Gets the target group of a context menu operation, or null if there is no
    ''' target member set or the target member is not a group. Note that this may be
    ''' a group which is not represented by a visual element in the treeview (eg. a
    ''' root node when roots are not visible).
    ''' </summary>
    Private ReadOnly Property TargetGroup As IGroup
        Get
            Return TryCast(TargetMember, IGroup)
        End Get
    End Property

    ''' <summary>
    ''' The group that an operation should be initiated on - ie. the group that the
    ''' target member belongs to or the target member itself if it is a group.
    ''' </summary>
    Private ReadOnly Property OperatingGroup As IGroup
        Get
            If TargetMember Is Nothing Then Return Nothing
            ' Otherwise, get the target group
            Dim parentGroup As IGroup = TargetGroup
            ' If it's not a group, get its owner and we work from that
            If parentGroup Is Nothing Then parentGroup = TargetMember.Owner
            ' Just in case...
            If parentGroup Is Nothing Then parentGroup = TargetMember.RootGroup
            Return parentGroup
        End Get
    End Property

    ''' <summary>
    ''' Gets the (modfiable) collection of extra context menu items which should be
    ''' displayed when this group tree control's context menu is displayed.
    ''' </summary>
    ''' <seealso cref="ContextMenuOpening" />
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend ReadOnly Property ExtraContextMenuItems As ICollection(Of ToolStripMenuItem)
        Get
            If mExtraContextMenuItems Is Nothing Then _
             mExtraContextMenuItems = New ObservableCollection(Of ToolStripMenuItem)
            Return mExtraContextMenuItems
        End Get
    End Property

    ''' <summary>
    ''' Sets a node to be highlighted. This uses the environment colours found in an
    ''' ancestor control which implements <see cref="IEnvironmentColourManager"/> for
    ''' the highlight colours, or, if no such ancestor is found, it will use the
    ''' standard system highlight colours.
    ''' </summary>
    ''' <value>The node to set as the highlighted node - there can be only one, so if
    ''' a node is currently highlighted, it will be reset before the given node is
    ''' highlighted. A value of null will remove the highlight from any currently
    ''' highlighted node.</value>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Private Property HighlightedNode As TreeNode
        Get
            Return mHighlightedNode
        End Get
        Set(value As TreeNode)
            If value Is mHighlightedNode Then Return
            If mHighlightedNode IsNot Nothing Then
                mHighlightedNode.BackColor = mSavedBackColor
                mHighlightedNode.ForeColor = mSavedForeColor
                mHighlightedNode = Nothing
            End If
            If value IsNot Nothing Then
                mSavedBackColor = value.BackColor
                mSavedForeColor = value.ForeColor

                If mEnvColours Is Nothing Then _
                    mEnvColours = UIUtil.GetAncestor(Of IEnvironmentColourManager)(Me)
                If mEnvColours IsNot Nothing Then
                    value.BackColor = mEnvColours.EnvironmentBackColor
                    value.ForeColor = mEnvColours.EnvironmentForeColor
                Else
                    value.BackColor = SystemColors.Highlight
                    value.ForeColor = SystemColors.HighlightText
                End If
                mHighlightedNode = value
            End If
        End Set
    End Property

    ReadOnly Property FirstGroup As IGroup
        Get
            Return CollectionUtil.First(mTrees.Keys)?.DefaultGroup
        End Get
    End Property

    ReadOnly Property DefaultGroup(member As IGroupMember) As IGroupMember
        Get
            Return member.RootGroup.FirstOrDefault(Function(x) TypeOf x Is IGroup AndAlso CType(x, IGroup).IsDefault)
        End Get
    End Property

    ReadOnly Property IsFiltered As Boolean
        Get
            Return mIsFiltered
        End Get
    End Property

    Public Property SelectedNodes As List(Of TreeNode)
        Get
            Return mSelectedNodes
        End Get
        Set
            mSelectedNodes.Clear()
            mSelectedNodes.AddRange(Value)
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Replaces an old tree from this tree control with a new tree of the same type.
    ''' This can be used to apply, modify or remove a filter.
    ''' </summary>
    ''' <param name="oldTree">The existing tree in this control to be swapped out
    ''' </param>
    ''' <param name="newTree">The tree to replace the old tree with</param>
    ''' <exception cref="ArgumentNullException">If <paramref name="oldTree"/> or
    ''' <paramref name="newTree"/> is null.</exception>
    ''' <exception cref="IncompatibleException">If the
    ''' <see cref="IGroupTree.TreeType">TreeType</see> of <paramref name="newTree"/>
    ''' did not match that of <paramref name="oldTree"/></exception>
    ''' <remarks>This has no effect if the two tree references point to the same
    ''' tree object</remarks>
    Friend Sub ReplaceTree(oldTree As IGroupTree, newTree As IGroupTree, Optional forceReplace As Boolean = False)

        If oldTree Is Nothing Then Throw New ArgumentNullException(NameOf(oldTree))
        If newTree Is Nothing Then Throw New ArgumentNullException(NameOf(newTree))
        If oldTree Is newTree AndAlso forceReplace = False Then Return

        If oldTree.TreeType <> newTree.TreeType Then Throw New IncompatibleException(
            My.Resources.GroupTreeControl_NewTreeMustHaveTheSameTypeAsOldTreeExpected0Found1,
            oldTree.TreeType, newTree.TreeType)

        Dim pair = mTrees.FirstOrDefault(Function(p) p.Key Is oldTree)

        If pair.Key Is Nothing Then Throw New NoSuchElementException(
            My.Resources.GroupTreeControl_TheTree0WasNotFoundInThisGroupTreeControl, oldTree)

        Dim node = pair.Value

        '' So we now have 'oldTree' and 'node'; the former is the tree to be removed
        '' and 'node' is the node to remove it from *if showRoot was true when the
        '' tree was added* or null, if it was not.

        ' Update the trees collection
        mTrees.Remove(oldTree)
        mTrees(newTree) = node

        ' Update the create menu items
        Dim menuItems As ICollection(Of ToolStripMenuItem) = Nothing
        If mCreateMenuItems.TryGetValue(oldTree, menuItems) Then
            mCreateMenuItems.Remove(oldTree)
            mCreateMenuItems(newTree) = menuItems
        End If

        ' And tell the view to update itself from the tree(s)
        UpdateView()

    End Sub

    ''' <summary>
    ''' Clears all trees and other view data from this control, effectively setting
    ''' it back to an initial state.
    ''' </summary>
    Public Sub Clear()
        mTrees.Clear()
        mTarget = Nothing
        mLastClick = Nothing
        mCurrCreateMenuTree = Nothing
        For Each pair In mCreateMenuItems
            For Each mi In pair.Value
                ctxGroup.Items.Remove(mi)
            Next
        Next
        mCreateMenuItems.Clear()
        tvGroups.Nodes.Clear()
        ClearGroupCache()
    End Sub

    ''' <summary>
    ''' Adds a tree to this view control, rooted at the given group, showing a node
    ''' representing the root within the control.
    ''' </summary>
    ''' <param name="gpTree">The tree to display</param>
    Public Sub AddTree(gpTree As IGroupTree)
        AddTree(gpTree, True)
    End Sub

    ''' <summary>
    ''' Adds a tree to this view control, rooted at the given group, showing a node
    ''' representing the root within the control.
    ''' </summary>
    ''' <param name="gpTree">The tree to display</param>
    ''' <param name="showRoot">True to show the root node in the control; false to
    ''' show its contents only</param>
    ''' <exception cref="AlreadyExistsException">If a tree represented by the given
    ''' group node already exists in this control</exception>
    Public Sub AddTree(gpTree As IGroupTree, showRoot As Boolean)
        Try
            mAddingTree = True
            If mTrees.ContainsKey(gpTree) Then Throw New AlreadyExistsException(
            My.Resources.GroupTreeControl_ControlAlreadyContainsTheTree0, gpTree.Root.Name)
            If showRoot Then
                Dim rootNode As TreeNode = AddMember(gpTree.Root, tvGroups.Nodes, True)
                ' We want to expand the top node only
                If rootNode IsNot Nothing Then
                    rootNode.Expand()
                    mTrees.Add(gpTree, rootNode)
                End If

            Else
                mTrees.Add(gpTree, Nothing)
                For Each child As IGroupMember In gpTree.Root
                    AddMember(child, tvGroups.Nodes)
                Next
            End If

            ' If we have nothing selected and we have any nodes at all, select the first
            If tvGroups.SelectedNode Is Nothing AndAlso tvGroups.Nodes.Count > 0 Then
                tvGroups.SelectedNode = tvGroups.Nodes(0)
            End If
        Finally
            mAddingTree = False
        End Try
    End Sub

    Private Function ConfirmUserGroupDelete(group As IGroup) As DialogResult
        Dim message = String.Format(My.Resources.GroupTreeControl_AreYouSureYouWantToDeleteTheGroup0, group.Name)

        If group.SubgroupCount > 0 Then
            message =
                $"{message}{vbCrLf}{My.Resources.GroupTreeControl_AnySubgroupsWillAlsoBeDeletedAndAnyItemsWillBeDisassociatedFromAffectedGroups}"
        ElseIf group.Count > 0 Then
            message =
                $"{message}{vbCrLf}{My.Resources.GroupTreeControl_AnyItemsInTheGroupWillBeDisassociatedFromIt}"
        End If

        Return MessageBox.Show(
            message,
            My.Resources.GroupTreeControl_DeleteGroupPrompt,
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Question)
    End Function

    Private Sub ValidateGroupDelete(group As IGroup)
        ' Display warning if deleting root group and it contains items.
        If group.IsRoot OrElse group.IsDefault Then
            Throw New GroupDeletedException(My.Resources.GroupTreeControl_YouCannotDeleteTheRootGroup)
        End If

        If group.IsInRoot() AndAlso
            group.Any() Then
            Throw New GroupDeletedException(My.Resources.GroupTreeControl_YouMustRemoveAllItemsFromThisGroupBeforeDeletion)
        End If

        ' Dont allow delete if this group contains hidden items.
        If group.ContainsHiddenMembers Then
            Throw New GroupDeletedException(My.Resources.GroupTreeControl_ThisGroupContainsHiddenItemsTheseMustBeRemovedBeforeItCanBeDeleted)
        End If
    End Sub

    Private Sub ValidateUserCanDeleteGroup(group As IGroup)
        Dim accessRightsOrEditPermission =
            group.Permissions.HasPermission(
                User.Current, group.Tree.TreeType.GetTreeDefinition().AccessRightsPermission) OrElse
            group.Permissions.HasPermission(
                User.Current, group.Tree.TreeType.GetTreeDefinition().EditPermission)

        ' Must have manage/edit access rights on this group to delete it.
        If group.Permissions.IsRestricted AndAlso Not accessRightsOrEditPermission Then
            Throw New PermissionException(My.Resources.GroupTreeControl_YouDoNotHavePermissionToDeleteThisGroup)
        End If

        Dim noAccessRightsAndEditPermission =
            Not group.Permissions.HasPermission(
                User.Current, group.Tree.TreeType.GetTreeDefinition().AccessRightsPermission) AndAlso
            group.Permissions.HasPermission(
                User.Current, group.Tree.TreeType.GetTreeDefinition().EditPermission)

        'if the user has edit and the branch is not empty then don't allow delete
        If group.Permissions.IsRestricted AndAlso
            group.Count > 0 AndAlso
            noAccessRightsAndEditPermission AndAlso
            Not group.Owner.Permissions.IsRestricted Then
            Throw New PermissionException(My.Resources.GroupTreeControl_YouDoNotHavePermissionToDeleteThisGroup)
        End If

        ' Must have manage access rights on all direct subgroups.
        If noAccessRightsAndEditPermission AndAlso
            Not group.Owner.Permissions.IsRestricted AndAlso
            group.Any(Function(x) x.IsGroup AndAlso
                            x.Permissions.IsRestricted) Then
            Throw New PermissionException(My.Resources.GroupTreeControl_YouCannotDeleteThisGroupYouDoNotHavePermissionToMoveOneOrMoreOfTheSubGroups)
        End If
    End Sub

    ''' <summary>
    ''' Handles the <see cref="ExtraContextMenuItems"/> collection changing, making
    ''' sure that the context menu in this tree control is kept up to date with the
    ''' changes.
    ''' </summary>
    Private Sub HandleContextItemsChanged(
     sender As Object, e As NotifyCollectionChangedEventArgs) _
     Handles mExtraContextMenuItems.CollectionChanged
        ' We don't analyse the change to the collection - we just compare the context
        ' menu to the collection and update the context menu if it doesn't match

        Dim intoExtras As Boolean = False

        ' OK, go through the context menu -
        ' all the items up to sepExtraItems are fixed; leave them alone.
        ' Any after that should match (ie. ref equal) the items in ExtraContextEtc..
        ' As soon as any don't match, remove the remaining toolstrip items and
        ' replace them with the remaining extra items in the collection
        Dim enu As IEnumerator(Of ToolStripMenuItem) =
            ExtraContextMenuItems.GetEnumerator()
        For i As Integer = 0 To ctxGroup.Items.Count - 1
            Dim item As ToolStripItem = ctxGroup.Items(i)
            If item Is sepExtraItems Then
                intoExtras = True

            ElseIf intoExtras Then
                ' We're into the extra ones - only menu items from here on in
                Dim mi = TryCast(item, ToolStripMenuItem)
                If mi Is Nothing Then Continue For ' just in case

                ' Move to the next extra item in our collection
                Dim extra As ToolStripMenuItem = Nothing
                If enu.MoveNext() Then extra = enu.Current

                ' If it's the same as the one in the context strip, carry on
                If mi Is enu Then Continue For

                ' Otherwise, it's different, remove mi and all remaining items in
                ' the context menu and append all items from the collection
                For j As Integer = ctxGroup.Items.Count - 1 To i Step -1
                    ctxGroup.Items.RemoveAt(j)
                Next j
                Do
                    ctxGroup.Items.Add(enu.Current)
                Loop While enu.MoveNext()

                ' Now leave the loop - we've modified the collection now, so the
                ' test in the for loop is invalid anyway, so escape before we're
                ' kicked out.
                Exit For

            End If
        Next i

        ' If we get here and there's anything left in the enumerator, it suggests
        ' that we've not added any from the collection to the context menu yet
        ' (ie. the iteration of the loop after 'sepExtraItems' was not reached)
        ' Go through the enumerator and add the contents to the menu now
        While enu.MoveNext() : ctxGroup.Items.Add(enu.Current) : End While

    End Sub

    ''' <summary>
    ''' Sets the create item menu items in the context menu to be those supported
    ''' by the given tree
    ''' </summary>
    ''' <param name="tree">The tree for which the 'Create Item' menu items are
    ''' required in the context menu.</param>
    Private Sub SetCreateItemMenuItems(tree As IGroupTree)
        ' If the tree is what is currently set, we have nothing to do
        If tree Is mCurrCreateMenuTree Then Return

        ' First remove any which are there...
        For Each itms As ICollection(Of ToolStripMenuItem) In mCreateMenuItems.Values
            For Each mi As ToolStripMenuItem In itms
                ctxGroup.Items.Remove(mi)
            Next
        Next
        mCurrCreateMenuTree = Nothing

        ' If we were asked to 'reset' the menu items, job done...
        If tree Is Nothing Then Return

        ' Otherwise, if ItemCreateEnabled is false, we can't add anything else
        If Not ItemCreateEnabled Then Return

        ' Can't do anything on Pools
        If mTarget.IsPool Then Return

        ' OK, first we see if we have to generate a new list of menu items already
        ' created for this tree.
        Dim lst As ICollection(Of ToolStripMenuItem) = Nothing
        If Not mCreateMenuItems.TryGetValue(tree, lst) Then
            ' We don't have the menu items for this tree, so generate it now
            lst = New List(Of ToolStripMenuItem)
            mCreateMenuItems(tree) = lst

            For Each tp As GroupMemberType In tree.SupportedMembers
                ' Create Group is handled separately
                If tp = GroupMemberType.Group Then Continue For
                ' Otherwise, create a new menu item for this type.
                lst.Add(New CreateGroupMemberMenuItem(
                        tp, AddressOf HandleCreateItemClick))
            Next
        End If

        ' Now we go through the list and insert them into the context menu
        ' Enable/Disable them depending on the edit permission for this user.
        Dim canEdit As Boolean = tree.HasEditPermission(User.Current)
        Dim i As Integer = 0
        For Each mi As ToolStripMenuItem In lst
            mi.Enabled = canEdit
            ctxGroup.Items.Insert(i, mi)
            i += 1
        Next

    End Sub

    ''' <summary>
    ''' Recursively adds a member to a specified node collection, adding all its
    ''' contents too, if the member represents a group.
    ''' </summary>
    ''' <param name="member">The member to add</param>
    ''' <param name="nodes">The node collection to add the member and all its
    ''' descendants to</param>
    Private Function AddMember(member As IGroupMember, nodes As TreeNodeCollection) _
     As TreeNode
        Return AddMember(member, nodes, False)
    End Function

    ''' <summary>
    ''' Recursively adds a member to a specified node collection, adding all its
    ''' contents too, if the member represents a group.
    ''' </summary>
    ''' <param name="member">The member to add</param>
    ''' <param name="nodes">The node collection to add the member and all its
    ''' descendants to</param>
    ''' <param name="inAdditionOrder">True to handle this level of adding members in
    ''' order they were added rather than the natural order of the members. This
    ''' value is <em>not</em> recursed through the subtree of the member and is
    ''' typically only used to organise the root nodes of multiple displayed trees.
    ''' </param>
    Private Function AddMember(
     member As IGroupMember, nodes As TreeNodeCollection, inAdditionOrder As Boolean) _
     As TreeNode
        Dim eventArgs = New AddingGroupTreeNodeEventArgs(member)
        RaiseEvent AddingNodeToTree(Me, eventArgs)
        If eventArgs.DoNotAdd Then
            Return Nothing
        End If

        Dim n As New TreeNode(member.Name) With {
            .ImageKey = member.ImageKey,
            .SelectedImageKey = member.ImageKey,
            .Tag = member,
            .Text = member.GetLocalisedName(Function(s)
                                                Return TreeDefinitionAttribute.GetLocalizedFriendlyName(s.Name)
                                            End Function),
            .Name = member.Name
        }

        If inAdditionOrder Then
            nodes.Add(n)
        Else
            InsertNodeInOrder(n, nodes)
        End If
        Dim gp As IGroup = TryCast(member, IGroup)
        If gp IsNot Nothing Then
            For Each child As IGroupMember In gp : AddMember(child, n.Nodes, inAdditionOrder) : Next
            If gp.Expanded Then
                n.Expand()
            End If
        End If
        Return n
    End Function

    ''' <summary>
    ''' Inserts a treenode into a collection at the correct point according to the
    ''' ordering rules of <see cref="IGroupMember"/> objects. Note that this will
    ''' abort the insertion if a node representing the group member already exists
    ''' in the given node collection.
    ''' </summary>
    ''' <param name="n">The tree node to insert.</param>
    ''' <param name="nodes">The node collection into which the node should be added.
    ''' </param>
    Private Sub InsertNodeInOrder(n As TreeNode, nodes As TreeNodeCollection)
        Dim mem As IGroupMember = GetMemberFor(n)

        ' default group nodes always first in collection
        Dim group = TryCast(mem, IGroup)
        If group IsNot Nothing AndAlso group.IsDefault Then
            nodes.Insert(0, n)
            Return
        End If

        ' Do a binary search to pick out the correct insertion point in the nodes
        ' collection. This is largely based on the Arrays.binarySearch() method in
        ' java, because that's what I had to hand to act as an aide memoire
        Dim lower = 0
        Dim upper = nodes.Count - 1
        While lower <= upper
            Dim middle = lower + ((upper - lower) \ 2)

            ' The current node in the node collection
            Dim curr As IGroupMember = GetMemberFor(nodes(middle))

            ' The result of a comparison:
            ' < 0 : mem precedes curr
            ' = 0 : mem equals curr
            ' > 0 : mem exceeds curr
            Dim comp As Integer = mem.CompareTo(curr)

            ' New member is later than comp, move lower up
            If comp > 0 Then lower = middle + 1

            ' New member is before comp, move upper down
            If comp < 0 Then upper = middle - 1

            ' Equals, skip drop - we're moving to a group which already
            ' contains the member... we don't want a duplicate.
            If comp = 0 Then Return

        End While

        ' We've found the insertion point at the lower bound
        nodes.Insert(lower, n)
    End Sub

    ''' <summary>
    ''' Checks if the given node represents the given group member.
    ''' </summary>
    ''' <param name="groupMember">
    ''' The member to check for. If null, this will return false.
    ''' </param>
    ''' <param name="tree">The tree of the member to check for - this is a
    ''' convenience simply to save on looking up the tree on the member for every
    ''' node in the tree (it's an indirect reference for filtered members)</param>
    ''' <returns>True if the given treenode represents the given group member, false
    ''' if either parameter is null or the node does not represent the group member.
    ''' </returns>
    Private Shared Function IsNodeForMember(
        groupMember As GroupInfo,
        tree As IGroupTree) As Func(Of GroupInfo, Boolean)

        Return Function(nodeMember)
                   If groupMember Is Nothing Then Return False

                   ' Check if the node's member has been orphaned; when the tree is reloaded
                   ' the root can be replaced, meaning that any members below the root get
                   ' orphaned. We want to ensure that the nodes representing those orphaned
                   ' members are updated with the newly reloaded members from the new root.
                   Dim orphaned = (nodeMember.Group.Tree Is Nothing)

                   ' If not orphaned, ensure it's in the same tree as the one we're comparing
                   If Not orphaned AndAlso Not nodeMember.Group.IsInTree(tree) Then Return False

                   Return groupMember Is nodeMember OrElse (
                       groupMember.MemberType = nodeMember.MemberType AndAlso
                       groupMember.IdString.Equals(nodeMember.IdString, StringComparison.Ordinal) AndAlso
                       groupMember.Path.Equals(nodeMember.Path, StringComparison.Ordinal))

               End Function

    End Function

    ''' <summary>
    ''' Gets the treenode which represents the given group member, or null if it is
    ''' not represented by a treenode
    ''' </summary>
    ''' <param name="mem">The member for which the node is required.</param>
    ''' <returns>The node which represents the given member, or null if it is not
    ''' represented as a treenode</returns>
    Private Function GetNodeFor(mem As IGroupMember) As TreeNode
        If mem Is Nothing Then Return Nothing
        Dim tree = mem.Tree

        Return tvGroups.FindNode(
            Function(treeNode) _
                If(
                    TryCast(treeNode.Tag, IGroupMember)?.
                    Map(Function(x) New GroupInfo(x)).
                    Map(IsNodeForMember(New GroupInfo(mem), tree)),
                    False))
    End Function

    ''' <summary>
    ''' Gets the treenode with the group member with the given Id, or null if it is
    ''' not represented by a treenode
    ''' </summary>
    ''' <param name="Id">The Id</param>
    ''' <returns>The node for the given Id, or null if it is not
    ''' represented as a treenode</returns>
    Public Function GetNodeFor(id As Guid) As TreeNode
        If id = Guid.Empty Then Return Nothing

        Return tvGroups.FindNode(
            Function(treeNode) _
                If(TryCast(treeNode.Tag, IGroupMember)?.Id, Guid.Empty).ToString() = id.ToString())
    End Function

    ''' <summary>
    ''' Gets the group member which the given node represents, or null if it does not
    ''' represent a group member (in which case, why is it here?)
    ''' </summary>
    ''' <param name="n">The node for which the group member is required</param>
    ''' <returns>The group member which the node represents, or null if it does not
    ''' represent a group member</returns>
    Private Function GetMemberFor(n As TreeNode) As IGroupMember
        If n Is Nothing Then Return Nothing
        Return TryCast(n.Tag, IGroupMember)
    End Function

    ''' <summary>
    ''' Gets the group which owns the member represented by the given node
    ''' <em>position</em>, or null if there is no such group. Note that if the node
    ''' does not yet represent a member (ie. it is newly created), it will still
    ''' return the group represented by the parent node.
    ''' </summary>
    ''' <param name="n">The node for which the owning group is required.</param>
    ''' <returns>The group represented by the node's parent, or null if the node has
    ''' no parent.</returns>
    Private Function GetOwningGroupFor(n As TreeNode) As IGroup
        If n Is Nothing Then Return Nothing
        If n.Parent Is Nothing Then
            ' This is a root node, and presumably roots are not being shown.
            ' If there's not a single tree in this control then we can't intuit
            ' which tree the node might belong to, so bail now
            If mTrees.Count <> 1 Then Return Nothing
            ' If there's a single tree, we can use the root of that tree as the
            ' owning group
            Return mTrees.Keys.First().Root
        End If
        Return DirectCast(GetMemberFor(n.Parent), IGroup)
    End Function

    ''' <summary>
    ''' Gets the group member at the given point, relative to the screen.
    ''' </summary>
    ''' <param name="x">The x screen coord at which the group member is required.
    ''' </param>
    ''' <param name="y">The y screen coord at which the group member is required.
    ''' </param>
    ''' <returns>The group member represented by the node found at the specified
    ''' location</returns>
    Private Function GetMemberAt(x As Integer, y As Integer) As IGroupMember
        Return GetMemberAt(New Point(x, y))
    End Function

    ''' <summary>
    ''' Gets the group member at the given point, relative to the screen.
    ''' </summary>
    ''' <param name="pt">The screen point at which the group member is required.
    ''' </param>
    ''' <returns>The group member represented by the node found at the specified
    ''' location.</returns>
    Private Function GetMemberAt(pt As Point) As IGroupMember
        Return GetMemberFor(tvGroups.GetNodeAt(tvGroups.PointToClient(pt)))
    End Function

    ''' <summary>
    ''' Determines whether the user is allowed to edit the label for the given node.
    ''' </summary>
    ''' <param name="n">The node to check if we are allowed to edit</param>
    ''' <returns>True if the user is allowed to edit the label for the given node;
    ''' False otherwise</returns>
    Protected Overridable Function CanEditLabel(n As TreeNode) As Boolean
        ' Can't do anything if this is readonly
        If [ReadOnly] Then Return False

        ' If it is a root node, it cannot be renamed
        If mTrees.Values.Contains(n) Then Return False

        ' Check the member to see what we can do with it
        Dim m As IGroupMember = GetMemberFor(n)

        ' If no member there, it means that we're creating a new group - allowed
        If m Is Nothing Then Return True

        ' See if this node represents a group
        Dim gp As IGroup = TryCast(m, IGroup)
        If gp IsNot Nothing Then
            If Not gp.Permissions.HasPermission(User.Current,
                gp.TreeType.GetTreeDefinition().EditPermission) Then Return False
            ' Otherwise, we delegate to the 'AllowRenameOfGroups' setting
            Return GroupRenameEnabled
        End If

        ' Otherwise, it's an item... easy
        Return ItemRenameEnabled
    End Function

    ''' <summary>
    ''' Sets a group member into a treenode, unregistering any member that the node
    ''' already has assigned to it
    ''' </summary>
    ''' <param name="m">The group member to assign to a node</param>
    ''' <param name="into">The node into which the group member is set</param>
    ''' <exception cref="AlreadyExistsException">If <paramref name="m"/> is already
    ''' registered with a treenode other than the one given.</exception>
    Private Sub SetMemberInto(m As IGroupMember, into As TreeNode)
        Dim curr As IGroupMember = GetMemberFor(into)
        Dim currNode As TreeNode = GetNodeFor(curr)
        If currNode IsNot Nothing AndAlso currNode IsNot into AndAlso
            currNode.TreeView IsNot Nothing Then Throw New AlreadyExistsException(
                My.Resources.GroupTreeControl_0IsAlreadyAssignedToNode1, m.Name, currNode.FullPath)

        into.Tag = m
    End Sub

    ''' <summary>
    ''' Checks if the given node is a placeholder for new group or item.
    ''' </summary>
    ''' <param name="n">The node to determine if it is a placeholder for a new group
    ''' or item.</param>
    ''' <returns>True if the node represents a new group/item placeholder, False
    ''' otherwise.</returns>
    Private Function IsPlaceholder(n As TreeNode) As Boolean
        Dim tp As Type = If(n.Tag Is Nothing, Nothing, n.Tag.GetType())
        Return (tp Is GetType(NewItemPlaceholder) OrElse
                tp Is GetType(NewGroupPlaceholder))
    End Function

    ''' <summary>
    ''' Updates the view in this group tree control to match the data held within it.
    ''' Note that this does not refresh the data from the store.
    ''' </summary>
    Public Sub UpdateView()
        UpdateView(False)
    End Sub

    ''' <summary>
    ''' Updates mTarget to the currently selected member.
    ''' </summary>
    Public Sub UpdateSelectedMember()
        Dim mem As IGroupMember = SelectedMember
        ' If nothing is selected, a point outside the tree(s) was selected
        If mem Is Nothing Then
            ' If we have more than one tree, we can't have clicks outside a node
            ' Otherwise, we use the root node as the target member
            If mTrees.Count = 1 Then _
             mTarget = CollectionUtil.First(mTrees.Keys).Root
        Else
            ' If we have a selected member, we have a target
            mTarget = mem
        End If
    End Sub

    ''' <summary>
    ''' Creates a new item in the currently selected group.
    ''' </summary>
    ''' <param name="tp"></param>
    Public Sub CreateNewItemInSelectedGroup(tp As GroupMemberType)
        Dim typeName As String = ""
        Try
            Dim gp As IGroup = OperatingGroup
            If gp Is Nothing Then Return

            ' If creating an item on the root node, and we have a default group
            ' then create in the default group instead.
            ' This isn't entirely necessary since this would be handled in
            ' IGroup.Add - but checking for it here allows us to 
            ' expand the default group so the user can see where the newly
            ' created item has gone.

            If gp.IsRoot AndAlso gp.Tree.HasDefaultGroup Then
                gp = gp.Tree.DefaultGroup
            End If


            If tp = GroupMemberType.None Then Return
            typeName = tp.GetLocalizedFriendlyName(True)

            'So here we need to validate the current state of the group.
            gp.ValidateGroupState()

            Dim arg As New CreateGroupMemberEventArgs(gp, tp)
            OnCreateRequested(arg)

            ' Let's see if we have a new member...
            Dim mem As IGroupMember = arg.CreatedItem

            ' Nothing created - it was a no-op
            If mem Is Nothing Then Return


            '' Otherwise, we want to add the new member to the group it was created in
            gp.Add(mem)

            'Remove the owners group from the cache.  If it exists.
            FlushGroupFromCache(mem.Owner)

            ' We need to ensure the group member matches the filter that is applied
            ' in the hosting tree (see bug 9388 for what happens if we don't)
            Dim gt = TryCast(gp.Tree, IFilteringGroupTree)
            If gt IsNot Nothing Then mem = mem.GetFilteredView(gt)

            ' And update the tree
            Dim parentNode As TreeNode = GetNodeFor(gp)
            Dim newNode As TreeNode
            If parentNode IsNot Nothing Then
                newNode = AddMember(mem, parentNode.Nodes)
                parentNode.Expand()
            Else
                newNode = AddMember(mem, tvGroups.Nodes)
            End If
            If newNode IsNot Nothing Then
                newNode.EnsureVisible()
            End If

            tvGroups.SelectedNode = newNode
            RaiseEvent RefreshView(Me, New EventArgs())
        Catch gre As GroupRenameException
            UserMessage.Show(My.Resources.GroupTreeControl_TheGroupHasBeenModifiedOnTheServerClickOKToRefresh)
            'refresh the screen
            RaiseEvent RefreshView(Me, New EventArgs())
        Catch ex As Exception
            UserMessage.Err(
                ex, My.Resources.GroupTreeControl_AnErrorOccurredCreatingANew0, typeName)

        End Try

    End Sub

    ''' <summary>
    ''' Updates the view in this group tree control to match the data held within it.
    ''' </summary>
    Friend Sub UpdateView(reloadFromStore As Boolean)
        Dim log = LogManager.GetCurrentClassLogger()

        Dim timer = New Stopwatch()
        timer.Start()

        If mTrees.Count = 0 Then
            tvGroups.Nodes.Clear()
            Return
        End If

        ' We want to go through the current treeview and compare it to what's in the
        ' tree we have set
        If reloadFromStore Then
            ' Get the latest tree
            For Each t As IGroupTree In mTrees.Keys : t.Reload() : Next
        End If

        tvGroups.BeginUpdate()
        UpdateAllTreeNodes()
        tvGroups.EndUpdate()

        RefreshTreeFilter()

        log.Debug($"GroupTreeControl.UpdateView reload={reloadFromStore.ToString()}, took {String.Format("{0:0.00}", timer.ElapsedMilliseconds)}ms")

    End Sub

    ''' <summary>
    ''' Instruct the group tree control to redraw the screen.
    ''' </summary>
    Private Sub UpdateAllTreeNodes()
        For Each t As IGroupTree In mTrees.Keys
            Dim n As TreeNode = mTrees(t)
            ' Make sure that the root group is kept up to date too
            If n IsNot Nothing Then n.Tag = t.Root
            If n Is Nothing AndAlso tvGroups.IsFiltered Then
                tvGroups.FilterByText(String.Empty)
            End If
            UpdateNode(t.Root, If(n Is Nothing, tvGroups.Nodes, n.Nodes))
        Next
    End Sub

    ''' <summary>
    ''' Recursively updates the node collection representing the given group.
    ''' </summary>
    ''' <param name="group">The group which is represented by the given node collection.
    ''' </param>
    ''' <param name="nodes">The treenodes which together form the UI representation
    ''' of the given group.</param>
    Private Sub UpdateNode(group As IGroup, nodes As TreeNodeCollection)

        ClearCacheValuesNodes(nodes)
        Dim validNodes As New HashSet(Of TreeNode)
        Dim tree As IGroupTree = group.Tree

        ' The following function contains a number of optimisations which greatly improve performance
        ' but may detract from readability. Please make sure you understand what each part is doing
        ' before editing.

        ' A dictionary of tree nodes with a key of IdString, and a value of 
        ' List(Of Anonymous Type { .TreeNode, .GroupInfo }) containing all the tree nodes that
        ' share that IdString. This list should usually only contain a single item, but it
        ' takes into account situations where the Id may be the same for two nodes (e.g. two nodes
        ' where Id = Guid.Empty)
        Dim treeNodeDictionary =
            nodes.Cast(Of TreeNode).
                Select(Function(x) New With {.TreeNode = x, .Group = TryCast(x.Tag, IGroupMember)}).
                Where(Function(x) x.Group IsNot Nothing).
                Select(Function(x) New With {x.TreeNode, .GroupInfo = New GroupInfo(x.Group)}).
                GroupBy(Function(k) k.GroupInfo.IdString).
                ToDictionary(Function(g) g.Key, Function(g) g.ToList())

        ' The tree node group informations. This is ordered by the ID string to enable the binary
        ' search later.
        Dim treeNodes =
            treeNodeDictionary.
                SelectMany(Function(x) x.Value.Select(Function(v) v.GroupInfo)).
                OrderBy(Function(x) x.IdString).
                ToList()

        ' A list of ID hashes for the tree nodes. This is used by the binary search
        Dim treeNodeIds = treeNodes.Select(Function(x) x.IdString).ToList()

        ' The group informations for the group nodes. This is ordered by the ID string to optimise the case when
        ' the binary search does not yield a result.
        Dim groupNodes As List(Of GroupInfo)
        If Not mGroupInfoCache.ContainsKey(group) Then
            'This is very slow to process, we don't want to keep doing it every time
            groupNodes =
            group.
                Select(Function(x) New GroupInfo(x.Id.ToString(), x.MemberType, x.Path, x)).
                OrderBy(Function(x) x.IdString).
                ToList()

            mGroupInfoCache.Add(group, groupNodes)
        Else
            groupNodes = mGroupInfoCache(group)
        End If



        For Each groupMember In groupNodes

            ' Attempt to find a matching ID by binary search. This greatly speeds up the process of
            ' finding the nodes as if the ID matches then we've normally found what we're looking for
            Dim index = mBinarySearchProvider.IndexOf(treeNodeIds, groupMember.IdString, Function(x, y) If(x < y, -1, If(x > y, 1, 0)))

            Dim node As TreeNode = Nothing

            If index > -1 Then

                ' Use IsNodeForMember, which uses further checks beyond ID, to find which of the 
                ' nodes that have the matching ID string is the matching node.
                ' If we don't have the item we're after then 'node' will remain null and the fallback
                ' case will be used.
                node = treeNodeDictionary(treeNodeIds(index)).
                            FirstOrDefault(Function(x) x.GroupInfo.Map(IsNodeForMember(groupMember, tree)))?.
                            TreeNode

            End If

            If node Is Nothing Then
                ' Fallback case. We've not managed to find it by binary search so have to use the old
                ' brute-force method. If that still doesn't yield a result then a new node is created
                ' and added.

                node = treeNodes.
                            FirstOrDefault(IsNodeForMember(groupMember, tree))?.
                            Group.
                            Map(Function(x) treeNodeDictionary(x.Id.ToString())).
                            FirstOrDefault(Function(x) x.GroupInfo.Map(IsNodeForMember(groupMember, tree)))?.TreeNode

                If node Is Nothing Then node = AddMember(groupMember.Group, nodes)

            End If

            ' Some group members won't be added to the tree, such as in the event user doesn't have permissions to 
            ' view the group. Continue with the next group member.
            If node Is Nothing Then Continue For

            ' Ensure that the node is up to date
            node.ImageKey = If(node.IsExpanded AndAlso node.Nodes.Count > 0,
                               groupMember.Group.ImageKeyExpanded, groupMember.Group.ImageKey)
            node.SelectedImageKey = node.ImageKey
            node.Text = groupMember.Group.GetLocalisedName(Function(s)
                                                               Return tree.TreeType.GetTreeDefinition().GetLocalizedNameForGroupMember(s.Name)
                                                           End Function)



            node.Tag = groupMember.Group
            validNodes.Add(node)

            Dim subGroup = TryCast(groupMember.Group, IGroup)
            If subGroup IsNot Nothing Then UpdateNode(subGroup, node.Nodes)
        Next
        ' Now any nodes we have which aren't in validNodes have been deleted, so we
        ' remove them from our tree, ensuring that selection is passed from any
        ' deleted node to the nearest equivalent in the new tree.
        nodes.OfType(Of TreeNode)() _
             .Where(Function(x) Not validNodes.Contains(x)) _
             .ToList.ForEach(Sub(y)
                                 If y.IsSelected And Not validNodes.Any(Function(x) x.Tag.Equals(y.Tag)) Then
                                     Dim newSel = FindEquivalentMemberNode(
                                                    validNodes, TryCast(y.Tag, GroupMember))
                                     If newSel?.TreeView?.SelectedNode IsNot Nothing Then newSel.TreeView.SelectedNode = newSel
                                 End If
                                 nodes.Remove(y)
                             End Sub)
    End Sub

    ''' <summary>
    ''' Finds a treenode which represents the equivalent of the given group member.
    ''' This is used primarily when the data has changed, and we want to ensure that
    ''' a selection remains selected when the tree is being updated. This checks the
    ''' given nodes for a unique node representing a group member with:
    ''' <list>
    ''' <item>The same <see cref="GroupMember.MemberType">MemberType</see> and either
    ''' </item>
    ''' <item>The same <see cref="GroupMember.Id">Id</see></item> -or-
    ''' <item>The same <see cref="GroupMember.Name">Name</see> if no nodes exist with
    ''' the same ID (note that if multiple members with the same type and name exist,
    ''' this will not return anything as it cannot determine the correct one to
    ''' return).</item>
    ''' </list>
    ''' </summary>
    ''' <param name="nodes">The nodes to search for an equivalent member</param>
    ''' <param name="findMem">The member for which an equivalent node is required.
    ''' A null value will always result in null being returned.</param>
    ''' <returns>The node from <paramref name="nodes"/> which is the closest match to
    ''' representing <paramref name="findMem"/>, or null if no such node was found.
    ''' Note that if no node representing the same type and ID is found, and multiple
    ''' nodes representing the same type and name are found, this method will return
    ''' null as it will not be able to determine which of the contenders is the
    ''' correct one to return.</returns>
    Private Function FindEquivalentMemberNode(
     nodes As IEnumerable(Of TreeNode), findMem As GroupMember) As TreeNode
        ' Shortcut out for null members
        If findMem Is Nothing Then Return Nothing

        ' Search the nodes for one which represents an equivalent group member
        Dim contenders As New List(Of TreeNode)
        For Each n In nodes
            ' Basically, the rules are:
            ' * Different types? Not the same
            ' * Same type & ID? The same, regardless of other aspects
            ' * Same type & Name? The same, unless another exists with the same ID.
            Dim mem = TryCast(n.Tag, GroupMember)
            If mem Is Nothing OrElse mem.MemberType <> findMem.MemberType Then _
             Continue For
            If mem.Id.Equals(findMem.Id) Then Return n
            If mem.Name = findMem.Name Then contenders.Add(n)
        Next
        If contenders.Count = 1 Then Return contenders(0)
        Return Nothing

    End Function

    ''' <summary>
    ''' Handles the activating of the given group member, if one is present.
    ''' </summary>
    ''' <param name="member">The member being activated. If the given value is null or
    ''' a group object, no <see cref="ItemActivated"/> event is generated, otherwise
    ''' the event is fired and the view is updated.</param>
    Private Sub HandleActivation(member As IGroupMember, Optional reloadFromStore As Boolean = True)
        If member Is Nothing Then Return

        ' We're not interested in groups
        If member.IsGroup Then Return

        Dim eventArgs = New GroupMemberEventArgs(member, reloadFromStore)
        OnItemActivated(eventArgs)
        RefreshPadlock(member)
    End Sub

    ''' <summary>
    ''' Update the padlock image on the required node.
    ''' </summary>
    ''' <param name="member"></param>
    Public Sub RefreshPadlock(member As IGroupMember)
        If member Is Nothing Then Throw New ArgumentNullException(NameOf(member))
        tvGroups.BeginUpdate()
        UpdateNodeImage(member)
        tvGroups.EndUpdate()
    End Sub

    ''' <summary>
    ''' Update the node image key
    ''' </summary>
    ''' <param name="member"></param>
    Private Sub UpdateNodeImage(member As IGroupMember)
        Dim node = tvGroups.Nodes.Find(member.Name, True).
            FirstOrDefault(Function(x) CType(x.Tag, IGroupMember).IdAsGuid = member.IdAsGuid)

        If node IsNot Nothing Then
            node.ImageKey = If(node.IsExpanded AndAlso node.Nodes.Count > 0,
                               member.ImageKeyExpanded, member.ImageKey)
            node.SelectedImageKey = node.ImageKey
        End If
    End Sub


    ''' <summary>
    ''' Allows the controlling form a way to signal a single item has been activated.
    ''' </summary>
    ''' <param name="member"></param>
    Public Sub ActivateTreeNode(member As IGroupMember)
        HandleActivation(member)
    End Sub



    ''' <summary>
    ''' Gets the target group of a drag operation if a valid group can be determined
    ''' for a given set of group members, while hovering over a specified member.
    ''' dragging into 
    ''' </summary>
    ''' <param name="hoverMem">The member that is currently being hovered over. A
    ''' null value is treated as 'in the whitespace below the tree'.</param>
    ''' <param name="mems">The members which are being dragged</param>
    ''' <returns>The valid group that is being targeted by the drag operation, which
    ''' may differ from <paramref name="hoverMem"/>, or null if no valid group was
    ''' found.</returns>
    Private Function GetDragTargetGroup(
     hoverMem As IGroupMember, mems As ICollection(Of IGroupMember)) As IGroup
        Dim gp As IGroup

        If hoverMem Is Nothing Then
            ' If we have more than one tree displayed, we can't guess what tree the
            ' user is trying to drag into, so don't try.
            ' Equally, no trees indicates nowhere to drag - again don't try
            If mTrees.Count <> 1 Then Return Nothing

            ' Otherwise, we assume the root of the tree
            gp = mTrees.Keys.First().Root
        Else
            gp = TryCast(hoverMem, IGroup)
            If gp Is Nothing Then gp = hoverMem.Owner
        End If


        ' If this is the root node and the tree has a default group...
        If gp.IsRoot AndAlso gp.Tree.HasDefaultGroup Then

            ' If user has no access to default group, then 
            ' can't drag items into root node.
            If Not gp.Tree.CanAccessDefaultGroup Then Return Nothing

            ' Items dragged onto the root node should be placed in the default folder.
            ' Groups can be dragged onto the root node.
            'If mems contains both groups and items then just do nothing.

            Dim containsGroups = mems.Any(Function(x) x.IsGroup)
            Dim containsItems = mems.Any(Function(x) Not x.IsGroup)
            If containsGroups AndAlso containsItems Then
                Return Nothing
            End If

            If containsItems Then gp = gp.Tree.DefaultGroup

        End If

        ' Make sure the target tree can cope with the members being dragged
        Dim targTree As IGroupTree = gp.Tree
        Dim allowedMembers = targTree.SupportedMembers
        For Each m As IGroupMember In mems
            ' Ensure that the target group can cope with the given type of member
            If Not allowedMembers.Contains(m.MemberType) Then Return Nothing



            ' And check that the tree types match, if one is set in the incoming
            ' member object (this is primarily to inhibit process groups being
            ' dragged into object trees or such like).
            Dim inTree As IGroupTree = m.Tree
            If inTree IsNot Nothing AndAlso inTree.TreeType <> targTree.TreeType Then _
             Return Nothing

        Next

        ' If the stuff we're dragging includes the current group, we can't do
        ' anything with that.
        If mems.Contains(gp) Then Return Nothing

        ' The target group already contains the members being dragged - nothing to do
        If gp.ContainsAll(mems) Then Return Nothing

        ' If the target is a descendant of any of the dragged members... we can't
        ' allow that
        For Each g As IGroup In gp.Ancestry
            If mems.Contains(g) Then Return Nothing
        Next

        ' OK, we're happy at this point that the items can be dragged into our
        ' identified group, return that.
        Return gp

    End Function

    ''' <summary>
    ''' Gets the group to use as the drag target for the given collection of group
    ''' members at the specified <em>screen</em>-relative location.
    ''' </summary>
    ''' <param name="screenLocn">The location, relative to the screen, that the
    ''' drag operation is performing.</param>
    ''' <param name="mems">The collection of group members making up the payload to
    ''' be dragged/dropped.</param>
    ''' <returns>The group which should be considered the target of a drag/drop
    ''' operation, or null if the given group members cannot be dropped at the
    ''' specified location.</returns>
    Private Function GetDragTargetGroup(
     screenLocn As Point, mems As ICollection(Of IGroupMember)) As IGroup
        If CollectionUtil.IsNullOrEmpty(mems) Then Return Nothing
        Return GetDragTargetGroup(GetMemberAt(screenLocn), mems)
    End Function

    ''' <summary>
    ''' Delegate to show a confimration warning if a group move may affect the permissions of the group
    ''' </summary>
    ''' <param name="message">The identity of the message to display</param>
    ''' <param name="informOnly">If true, displays a dialog box with only an 'OK' button.</param>
    ''' <param name="args">Arguments to populate the format string</param>
    ''' <returns></returns>
    Public Shared Function ShowConfirmDialog(message As MessageID, informOnly As Boolean, ParamArray args() As String) As Boolean
        Dim resourceMessage As String
        Select Case message
            Case MessageID.Warn_Message_Inherit_Perms
                resourceMessage = My.Resources.ctlDevelopView_Resources.Warn_Message_Inherit_Perms
            Case MessageID.Warn_Message_Member_Inherit_Perms
                resourceMessage = My.Resources.ctlDevelopView_Resources.Warn_Message_Member_Inherit_Perms
            Case MessageID.Warn_Message_Overwrite_Parent
                resourceMessage = My.Resources.ctlDevelopView_Resources.Warn_Message_Overwrite_Parent
            Case MessageID.Warn_Message_Lose_Perms
                resourceMessage = My.Resources.ctlDevelopView_Resources.Warn_Message_Lose_Perms
            Case MessageID.Warn_Message_Member_Lose_Perms
                resourceMessage = My.Resources.ctlDevelopView_Resources.Warn_Message_Member_Lose_Perms
            Case MessageID.Warn_Message_Overwrite_Ancestor
                resourceMessage = My.Resources.ctlDevelopView_Resources.Warn_Message_Overwrite_Ancestor
            Case MessageID.Warn_Copy_Unrestricted_to_Restricted
                resourceMessage = My.Resources.ctlDevelopView_Resources.Warn_Copy_Unrestricted_to_Restricted
            Case MessageID.Warn_Copy_Restricted_to_Unrestricted
                resourceMessage = My.Resources.ctlDevelopView_Resources.Warn_Copy_Restricted_to_Unrestricted
            Case MessageID.Warn_Copy_Restricted_Diff_Ancestor
                resourceMessage = My.Resources.ctlDevelopView_Resources.Warn_Copy_Restricted_Diff_Ancestor
            Case MessageID.Inform_Insufficient_Permissions_Move
                resourceMessage = My.Resources.ctlDevelopView_Resources.Insufficient_Permissions_Move
            Case MessageID.Inform_Insufficient_Permissions_Restricted_Target
                resourceMessage = My.Resources.ctlDevelopView_Resources.Insufficient_Permissions_Restricted_Target
            Case MessageID.Inform_Insufficient_Permissions_Restricted_Source
                resourceMessage = My.Resources.ctlDevelopView_Resources.Insufficient_Permissions_Restricted_Source
            Case MessageID.Inform_Insufficient_Permissions_Restricted_Move
                resourceMessage = My.Resources.ctlDevelopView_Resources.Insufficient_Permissions_Restricted_Move
            Case MessageID.Inform_Insufficient_Permissions_Same_Target_And_Source
                resourceMessage = My.Resources.ctlDevelopView_Resources.Insufficient_Permissions_Restricted_Same_Source_And_Target
            Case Else
                Throw New ArgumentOutOfRangeException(NameOf(message), My.Resources.ctlDevelopView_Resources.ErrorMsg_NoMessage)
        End Select

        Dim warningText = String.Format(resourceMessage, args)

        Dim buttons As MessageBoxButtons = CType(IIf(informOnly, MessageBoxButtons.OK, MessageBoxButtons.OKCancel), MessageBoxButtons)
        Return BPMessageBox.ShowDialog(warningText, My.Resources.ctlDevelopView_Resources.ctlDevelopView_ShowConfirmDialog_Warning, buttons) = DialogResult.OK

    End Function

    Public Sub RefreshTreeFilter()
        ApplyTreeFilter(txtFilter.Text)
    End Sub

    Public Sub ApplyEmptyFilter()
        ApplyTreeFilter(String.Empty)
    End Sub

    Public Sub ClearTreeFilter()
        txtFilter.Clear()
    End Sub

    Private Sub ApplyTreeFilter(text As String)
        ClearGroupCache()
        If String.IsNullOrWhiteSpace(text) Then
            tvGroups.ClearFilter()
            Return
        End If

        If tvGroups.IsFiltered Then
            tvGroups.ClearFilter()
        End If

        Dim filter As TreeNodeFilter = Function(t, n) n.Text.Contains(t, StringComparison.CurrentCultureIgnoreCase)

        If filter <> Nothing Then
            tvGroups.FilterByNode(Function(n As TreeNode) filter(text, n))
        Else
            tvGroups.FilterByText(text)
        End If
        mIsFiltered = tvGroups.IsFiltered
    End Sub
    
#End Region

#Region " Internal Event Management "

    ''' <summary>
    ''' Raises the <see cref="GroupSelected"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnGroupSelected(e As EventArgs)
        RaiseEvent GroupSelected(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="ItemSelected"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnItemSelected(e As EventArgs)
        RaiseEvent ItemSelected(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="MemberDeselecting"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnMemberDeselecting(e As CancelEventArgs)
        RaiseEvent MemberDeselecting(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="NameChanging"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnNameChanging(e As NameChangingEventArgs)
        RaiseEvent NameChanging(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="NameChanged"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnNameChanged(e As NameChangedEventArgs)
        RaiseEvent NameChanged(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="CreateRequested"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnCreateRequested(e As CreateGroupMemberEventArgs)
        RaiseEvent CreateRequested(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="CloneRequested"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnCloneRequested(e As CloneGroupMemberEventArgs)
        RaiseEvent CloneRequested(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="DeleteRequested"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnDeleteRequested(e As GroupMemberEventArgs)
        RaiseEvent DeleteRequested(Me, e)
    End Sub

    Protected Overridable Sub OnEditRequested(e As GroupMemberEventArgs)
        RaiseEvent EditRequested(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="GroupMemberDropped"/> event
    ''' </summary>
    Protected Overridable Sub OnGroupMemberRequestDrop(e As GroupMemberRequestDropEventArgs)
        ' Validate drop action against group permission rules
        If ValidateMemberMovements Then
            Dim fromGroup = If(e.Member.IsGroup, e.Member, e.Member.Owner)

            If mGroupPermissionLogic Is Nothing Then mGroupPermissionLogic = New GroupPermissionLogic()
            If Not mGroupPermissionLogic.ValidateMoveMember(e.Member, fromGroup, e.Target, e.IsCopy,
                AddressOf ShowConfirmDialog, User.Current) Then
                e.Cancel = True
                Exit Sub
            End If
        End If

        RaiseEvent GroupMemberRequestDrop(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="GroupMemberDropped"/> event
    ''' </summary>
    Protected Overridable Sub OnGroupMemberDropped(e As GroupMemberDropEventArgs)
        RaiseEvent GroupMemberDropped(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="ContextMenuOpening"/> event
    ''' </summary>
    Protected Overridable Sub OnContextMenuOpening(e As GroupMemberContexMenuOpeningEventArgs)
        RaiseEvent ContextMenuOpening(Me, e)
    End Sub

#End Region

#Region " Treeview Event Handlers "

    ''' <summary>
    ''' Raises the <see cref="ItemActivated"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the member activated</param>
    Protected Overridable Sub OnItemActivated(e As GroupMemberEventArgs)
        RaiseEvent ItemActivated(Me, e)
    End Sub

    ''' <summary>
    ''' Handles a treenode being double clicked, ensuring that the appropriate event
    ''' (if any) is propogated from it.
    ''' </summary>
    Private Sub HandleDoubleClickNode(
     sender As Object, e As TreeNodeMouseClickEventArgs) _
     Handles tvGroups.NodeMouseDoubleClick
        ' We're only concerned with double left-clicks
        If e.Button <> MouseButtons.Left OrElse Not EnableDoubleClick OrElse Control.ModifierKeys = Keys.Shift OrElse Control.ModifierKeys = Keys.Control OrElse mSelectedNodes.Count > 1 Then Return
        HandleActivation(GetMemberFor(e.Node), Not mTypesThatDoNotStoreReload.Any(Function(type) type Is e.Node.Tag.GetType()))
        RaiseEvent NodeDoubleClick(sender, e)
    End Sub

    ''' <summary>
    ''' Handles the enter key being pressed while the treeview has focus, ensuring
    ''' that the appropriate event (if any) is propogated from it.
    ''' </summary>
    Private Sub HandleEnterKeyPressed(sender As Object, e As PreviewKeyDownEventArgs) _
     Handles tvGroups.PreviewKeyDown
        ' We're only concerned with the enter key
        If e.KeyCode <> Keys.Enter Then Return

        ' If the selected node has children then enter expands / collapses it
        Dim n As TreeNode = tvGroups.SelectedNode
        If n IsNot Nothing AndAlso n.Nodes.Count > 0 AndAlso Not n.IsEditing Then
            n.Toggle()
            ' Carry on anyway... we may need to do something with groups later, so
            ' path of least surprise and all that.
        End If

        HandleActivation(SelectedMember)
    End Sub

    ''' <summary>
    ''' Handles the treeview being resized, ensuring that the currently selected node
    ''' is still visible.
    ''' </summary>
    Private Sub HandleTreeViewResize() Handles tvGroups.Resize
        If tvGroups.SelectedNode IsNot Nothing Then tvGroups.SelectedNode.EnsureVisible()
    End Sub

    ''' <summary>
    ''' Handles the event fired when a node is clicked, ensuring that it is selected
    ''' (including when a node is right-clicked)
    ''' </summary>
    Private Sub HandleNodeClick(sender As Object, e As TreeNodeMouseClickEventArgs) _
     Handles tvGroups.NodeMouseClick
        mLastClick = e.Node

        If EnableMultipleSelect AndAlso mSelectedNodes.Count > 0 Then
            Return
        End If

        Dim clickedNode = tvGroups.GetNodeAt(e.X, e.Y)
        If clickedNode Is Nothing Then Return

        If NodeBounds(clickedNode).Contains(e.X, e.Y) Then
            tvGroups.SelectedNode = clickedNode
        End If

        ' If they left-clicked this will already have been done once...
        ' We want to make sure that any cancelled selections only occur once
        If e.Button = MouseButtons.Right Then tvGroups.SelectedNode = e.Node
    End Sub

    ''' <summary>
    ''' Handles the event fired before a node is selected, ensuring that a
    ''' <see cref="MemberDeselecting"/> event is fired before the node is deselected.
    ''' </summary>
    Private Sub HandleBeforeSelect(sender As Object, e As TreeViewCancelEventArgs) _
     Handles tvGroups.BeforeSelect

        ' Set the 'handling before select' flag - if an event listener opens up a
        ' dialog (eg. MessageBox), the BeforeSelect handler is entered twice, once
        ' which propagates the dialog, and once after the dialog is closed.
        ' This mechanism ensures that we don't handle the event the second time
        If mHandlingBeforeSelect Then Return
        mHandlingBeforeSelect = True
        Try
            Dim m As IGroupMember = SelectedMember
            If m Is Nothing Then Return ' Nothing to 'unselect'

            'If the node being selected is a group and we are in the middle of a multiSelect then we should cancel
            Dim bControl = ModifierKeys = Keys.Control
            Dim bShift = ModifierKeys = Keys.Shift
            'MultiSelect Code
            If EnableMultipleSelect Then
                ' Cancel the select if we are already dragging 
                If IsDragging AndAlso mSelectedNodes.Contains(e.Node) Then
                    e.Cancel = True
                    Return
                End If

                If bControl AndAlso mSelectedNodes.Contains(e.Node) Then
                    e.Cancel = True
                    mSelectedNodes.Remove(e.Node)
                    Return
                End If

                Dim siblings = mFirstNode IsNot Nothing AndAlso mFirstNode.Parent IsNot Nothing AndAlso mFirstNode.Parent.Nodes.Contains(e.Node)
                If bShift AndAlso Not siblings Then
                    'If the nodes are not siblings don't permit the select
                    e.Cancel = True
                    mSelectedNodes.Remove(e.Node)
                    Return
                End If

                If Not bShift Then
                    mFirstNode = e.Node
                End If
            End If

            Dim newSelect = TryCast(e.Node.Tag, IGroupMember)
            If (bControl OrElse bShift) AndAlso newSelect.IsGroup Then
                e.Cancel = True
                Return
            End If

            Dim ce As New CancelEventArgs()
            OnMemberDeselecting(ce)
            e.Cancel = ce.Cancel
            ' If we've created a new node and that's what we're selecting, we need to
            ' delete the new node again... we can't have it sat there without a group
            ' member
            If e.Cancel AndAlso GetMemberFor(e.Node) Is Nothing Then e.Node.Remove()
            
        Finally
            mHandlingBeforeSelect = False

        End Try
    End Sub

    ''' <summary>
    ''' Handles the event fired after a node is selected, ensuring that either a
    ''' <see cref="GroupSelected"/> or <see cref="ItemSelected"/> event is fired.
    ''' </summary>
    Private Sub HandleAfterSelect(sender As Object, ByVal e As TreeViewEventArgs) Handles tvGroups.AfterSelect
        If IsDragging Then
            Return
        End If
        Dim memberGroup As IGroupMember = SelectedMember
        ' Nothing selected - nothing to report
        If memberGroup Is Nothing Then Return
        Dim group As IGroup = TryCast(memberGroup, IGroup)
        If group IsNot Nothing Then

            LoadingFormHelper.ShowForm(
                Sub() OnGroupSelected(EventArgs.Empty))
            RefreshPadlock(group)
        Else
            OnItemSelected(EventArgs.Empty)
        End If
        AddToSelectedNodes(e)
    End Sub

    Private Sub AddToSelectedNodes(e As TreeViewEventArgs)
        If EnableMultipleSelect Then
            Dim member = TryCast(e.Node.Tag, IGroupMember)
            If member IsNot Nothing AndAlso member.IsGroup Then
                mSelectedNodes.Clear()
                Return
            End If

            Dim bControl = ModifierKeys = Keys.Control
            Dim bShift = ModifierKeys = Keys.Shift

            Select Case True
                Case bControl
                    If Not SelectedMember.IsGroup AndAlso Not mSelectedNodes.Contains(e.Node) Then
                        mSelectedNodes.Add(e.Node)
                    Else
                        mSelectedNodes.Remove(e.Node)
                    End If
                        Case bShift
                    PerformShiftSelect(e)
                Case Else
                    If mSelectedNodes.Count > 0 Then
                        mSelectedNodes.Clear()
                    End If

                    mSelectedNodes.Add(e.Node)
                    mFirstNode = e.Node
            End Select

            tvGroups.Invalidate()
        End If
    End Sub

    Private Sub PerformShiftSelect(e As TreeViewEventArgs)

        If SelectedMember.IsGroup Then Return

        Dim shiftedNodes = New List(Of TreeNode)

        Dim lastNode = e.Node

        Dim indexFirst = mFirstNode.Index
        Dim indexLast = lastNode.Index
        Dim indexCurrent = indexFirst

        Dim n = mFirstNode

        If indexFirst <= indexLast Then
            While indexCurrent <= indexLast
                shiftedNodes.Add(n)

                n = n.NextNode
                indexCurrent += 1
            End While
        Else
            While indexCurrent >= indexLast
                shiftedNodes.Add(n)
                n = n.PrevNode
                indexCurrent -= 1
            End While

        End If
        mSelectedNodes.Clear()
        mSelectedNodes.AddRange(shiftedNodes)
    End Sub


    ''' <summary>
    ''' Handles before a node's label is edited. This just ensures that the root
    ''' nodes label is not edited.
    ''' </summary>
    Private Sub HandleBeforeLabelEdit(sender As Object, e As NodeLabelEditEventArgs) _
     Handles tvGroups.BeforeLabelEdit
        If Not CanEditLabel(e.Node) Then e.CancelEdit = True : Return
        For Each n As TreeNode In mTrees.Values
            If n Is e.Node Then e.CancelEdit = True : Return
        Next
    End Sub

    ''' <summary>
    ''' Handles a keydown event on the treeview, allowing the node to be renamed
    ''' when F2 is pressed.
    ''' </summary>
    Private Sub HandleKeyDown(sender As Object, e As KeyEventArgs) _
     Handles tvGroups.KeyDown
        If e.KeyCode = Keys.F2 Then
            Dim n = tvGroups.SelectedNode
            If n IsNot Nothing Then n.BeginEdit()
        ElseIf e.KeyCode = Keys.Enter AndAlso Not tvGroups.SelectedNode.IsEditing Then
            ' This inhibits the default sound when pressing enter in the treeview,
            ' allowing items to be activated, groups to be toggled, etc. in silence
            e.Handled = True
            e.SuppressKeyPress = True
        ElseIf e.KeyCode = Keys.Delete Then
            If TypeOf SelectedMember Is IGroup Then
                OnDeleteGroupClick()
            ElseIf SelectedMember IsNot Nothing Then
                OnDeleteRequested(New GroupMemberEventArgs(SelectedMember))
            End If
        ElseIf e.KeyCode = Keys.F10 AndAlso e.Modifiers = Keys.Shift Then
            ctxGroup.Show(tvGroups.PointToScreen(Point.Empty))
        End If
    End Sub

    ''' <summary>
    ''' Handles after a node's label has been edited
    ''' </summary>
    Private Sub HandleAfterLabelEdit(sender As Object, e As NodeLabelEditEventArgs) _
     Handles tvGroups.AfterLabelEdit

        If Not User.LoggedIn Then Exit Sub
        Dim nce As New NameChangingEventArgs(e.Node.Text, e.Label)
        OnNameChanging(nce)
        If nce.Cancel Then
            e.CancelEdit = True
            e.Node.BeginEdit()
            Return
        End If
        ' If it's blank, then we want to abort creation of it
        If e.Label = "" Then
            e.CancelEdit = True
            If IsPlaceholder(e.Node) Then
                If mLastClick IsNot Nothing Then tvGroups.SelectedNode = mLastClick
                e.Node.Remove()
            End If
            Return
        End If

        Dim tag As Object = e.Node.Tag

        Try
            If IsPlaceholder(e.Node) Then
                Dim ng As NewGroupPlaceholder = TryCast(tag, NewGroupPlaceholder)
                Dim owner As IGroup = GetOwningGroupFor(e.Node)
                If owner Is Nothing Then Throw New MissingItemException(
                    My.Resources.GroupTreeControl_NoOwningGroupFoundForNode0, e.Node)

                ' ASSERT: NewItemPlaceholder never got implemented (it's done using
                ' request events instead), so only groups should be done this way
                Debug.Assert(ng IsNot Nothing)

                If ng IsNot Nothing Then
                    ' We're creating a new group
                    Dim gp As IGroup = owner.CreateGroup(e.Label)
                    If gp IsNot Nothing Then SetMemberInto(gp, e.Node)

                    ClearGroupCacheAtTreeLevel(owner)

                    UpdateView()
                End If
            Else
                Dim mem As IGroupMember = GetMemberFor(e.Node)
                Dim g As IGroup = TryCast(mem, IGroup)
                If g IsNot Nothing Then
                    g.Name = e.Label
                    SetMemberInto(g, e.Node)

                ElseIf mem IsNot Nothing Then
                    mem.Name = e.Label
                    Throw New NotImplementedException("Not sure how to do this yet")

                End If
            End If
            OnNameChanged(New NameChangedEventArgs(e.Node.Text, e.Label))
            If tvGroups.IsFiltered Then
                UpdateView()
            End If
        Catch ex As GroupCreateException
            UserMessage.Show(ex.Message)
            'If it's a new entry, make sure it doesn't stick around in the tree
            If IsPlaceholder(e.Node) Then

                If mLastClick IsNot Nothing Then
                    tvGroups.SelectedNode = mLastClick

                End If
                e.Node.Remove()
            End If
            e.CancelEdit = True
            e.Node.EndEdit(True)
            'We need to wait for AfterLabelEdit function to complete before performing refresh.
            BeginInvoke(AddressOf OnRefreshView)
        Catch gex As GroupRenameException
            UserMessage.Show(My.Resources.GroupTreeControl_TheItemYouAreTryingToMoveHasAlreadyBeenMovedRenamedOrDeletedClickOKToRefreshTheTree)
            e.CancelEdit = True
            e.Node.EndEdit(True)
            'We need to wait for AfterLabelEdit function to complete before performing refresh.
            BeginInvoke(AddressOf OnRefreshView)
        Catch ex As Exception
            UserMessage.Err(ex, String.Format(My.Resources.GroupTreeControl_ErrorCreatingNewItem0, ex.Message))
            e.CancelEdit = True
            ' If it's a new entry, make sure it doesn't stick around in the tree
            If IsPlaceholder(e.Node) Then
                If mLastClick IsNot Nothing Then tvGroups.SelectedNode = mLastClick
                e.Node.Remove()
            End If
        End Try
    End Sub

    Private Sub OnRefreshView()
        RaiseEvent RefreshView(Me, EventArgs.Empty)
    End Sub

    Private Sub HandleNodeExpanded(sender As Object, e As TreeViewEventArgs) Handles tvGroups.AfterExpand
        HandleNodeToggle(e, True)
    End Sub

    Private Sub HandleNodeCollapsed(sender As Object, e As TreeViewEventArgs) Handles tvGroups.AfterCollapse
        HandleNodeToggle(e, False)
    End Sub

    Private Sub HandleNodeToggle(e As TreeViewEventArgs, expanded As Boolean)
        Dim node = e.Node, member = GetMemberFor(node)
        If member Is Nothing Then Return
        node.ImageKey = If(expanded, member.ImageKeyExpanded, member.ImageKey)
        node.SelectedImageKey = If(expanded, member.ImageKeyExpanded, member.ImageKey)
        Dim group As IGroup = TryCast(member, IGroup)
        If group IsNot Nothing Then
            group.Expanded = expanded

            Try
                If SaveExpandedGroups AndAlso group.IdAsGuid <> Guid.Empty AndAlso Not mAddingTree AndAlso group.MemberType <> GroupMemberType.Pool Then
                    gSv.SaveTreeNodeExpandedState(group.IdAsGuid, expanded, group.TreeType)
                End If
            Catch ex As Exception
                Dim log = LogManager.GetCurrentClassLogger()
                log.Error(ex)
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Handles a drag operation starting in the treeview
    ''' </summary>
    Private Sub HandleDrag(ByVal sender As Object, ByVal e As ItemDragEventArgs) _
     Handles tvGroups.ItemDrag
        mDragging = True

        ' No dragging if we're all readonly
        If [ReadOnly] Then Return

        ' Get the item being dragged
        Dim n As TreeNode = TryCast(e.Item, TreeNode)
        If n Is Nothing Then Return
        ' Ensure the node is selected
        tvGroups.SelectedNode = n

        ' Get the member and check that we're allowed to move
        Dim mem As IGroupMember = GetMemberFor(n)
        Debug.Assert(mem IsNot Nothing)
        If mem Is Nothing Then Return

        ' Can't currently drag pool members
        If mem.MemberType = GroupMemberType.Resource Then
            Dim rgm = TryCast(mem, ResourceGroupMember)
            If rgm IsNot Nothing And rgm.IsPoolMember Then Return
        End If

        ' No edit permission for this tree? Can't move/copy anything in it
        If Not mem.Tree.HasEditPermission(User.Current) Then Return

        ' We can't drag the root nodes
        If mTrees.Values.Contains(n) Then Return

        ' We're allowed to move, whatever
        Dim allowed = DragDropEffects.Move
        ' If the member's owner is the root node, we can't copy
        If Not (mem.IsInRoot Or mem.RawMember.MemberType = GroupMemberType.Pool) AndAlso CloneDragEnabled Then
            allowed = allowed Or DragDropEffects.Copy
        End If

        Dim obj As New DataObject()
        obj.SetData(Of ICollection(Of IGroupMember))(GetSingleton.ICollection(mem))
        ' bug 8940: dashboard control is in AutomateControls so it has no visibility
        ' of any data types, but it still needs the data, so we encode the IDs into
        ' a string, specifically for that control.

        ' If this is a group, we need to enumerate the (non-group) members within the
        ' group (and subgroups)
        If mem.IsGroup Then
            Dim ids As New clsSet(Of Object)
            DirectCast(mem, IGroup).Scan(
                Sub(m) If Not m.IsGroup AndAlso m.Id IsNot Nothing Then ids.Add(m.Id))
            If ids.Count > 0 Then obj.SetData(CollectionUtil.Join(ids, ";"))

        Else ' if not a group, just set the ID (if present)
            If mem.Id IsNot Nothing Then obj.SetData(mem.Id.ToString())
        End If

        If EnableMultipleSelect AndAlso mSelectedNodes.Count > 1 Then
           
            'load the obj with all the selected items
            Dim selectedMembers As ICollection(Of IGroupMember) = New List(Of IGroupMember)
            For each node As TreeNode In mSelectedNodes
                selectedMembers.Add(TryCast(node.tag, IGroupMember))
            Next
            obj.SetData(Of ICollection(Of IGroupMember))(selectedMembers)
        End If
        
        Try
            tvGroups.DoDragDrop(obj, allowed)
        Finally
            mDragging = False
        End Try

        ' Make sure that nothing is highlighted when the drag/drop is over
        HighlightedNode = Nothing

    End Sub

    ''' <summary>
    ''' Handles a drag operation being interrupted by being focused somewhere other
    ''' than the treeview.
    ''' </summary>
    Private Sub HandleDragLeave(sender As Object, e As EventArgs) _
     Handles tvGroups.DragLeave
        HighlightedNode = Nothing
        HoverGroup = Nothing
    End Sub

    ''' <summary>
    ''' Handles a dragging operation occurring over the treeview
    ''' </summary>
    Private Sub HandleDragging(sender As Object, e As DragEventArgs) _
     Handles tvGroups.DragOver

        ' Assume that we can't drag/drop - we override the value if we can
        e.Effect = DragDropEffects.None

        ' No dragging if we're all readonly
        If [ReadOnly] Then Return

        Dim mems As ICollection(Of IGroupMember) =
         e.Data.GetData(Of ICollection(Of IGroupMember))()

        Dim targ As IGroup = GetDragTargetGroup(New Point(e.X, e.Y), mems)
        Dim targNod As TreeNode = GetNodeFor(targ)

        ' Set the hover group into the timer - if not a group it just disables it
        HoverGroup = If(targNod Is Nothing OrElse targNod.IsExpanded, Nothing, targ)
        If targ Is Nothing Then HighlightedNode = Nothing : Return

        ' Test for the Ctrl key and set the effects accordingly
        If e.HasKeyState(DragKeyState.CtrlKey) Then
            e.Effect = If(targ.CanCopyInto(mems),
                          DragDropEffects.Copy, DragDropEffects.None)
        Else
            ' Otherwise if we're allowed to move, set that effect
            e.Effect = If(targ.CanMoveInto(mems),
                          DragDropEffects.Move, DragDropEffects.None)
        End If
        HighlightedNode = If(e.Effect = DragDropEffects.None, Nothing, targNod)

    End Sub

    ''' <summary>
    ''' Handles an item being dragged from another control
    ''' </summary>
    Private Sub HandleDragEnter(sender As Object, e As DragEventArgs) _
     Handles tvGroups.DragEnter

        ' No dragging if we're all readonly
        If [ReadOnly] Then Return

        ' If the item being dragged is not a collection of group members,
        ' then we can't handle it.
        If e.Data.GetData(Of ICollection(Of IGroupMember))() Is Nothing Then
            e.Effect = DragDropEffects.None
            Return
        End If

        ' Otherwise, the drag is valid - set the appropriate effect.
        ' Test for the Ctrl key and set the effects accordingly
        If e.HasKeyState(DragKeyState.CtrlKey) Then
            e.Effect = If(e.AllowedEffect.HasFlag(DragDropEffects.Copy),
                          DragDropEffects.Copy, DragDropEffects.None)
        Else
            ' Otherwise if we're allowed to move, set that effect
            e.Effect = If(e.AllowedEffect.HasFlag(DragDropEffects.Move),
                          DragDropEffects.Move, DragDropEffects.None)
        End If

    End Sub

    ''' <summary>
    ''' Handles the 'drop' event of a 'drag and drop' operation occuring within the
    ''' treeview.
    ''' </summary>
    Private Sub HandleDrop(sender As Object, e As DragEventArgs) _
     Handles tvGroups.DragDrop

        ' No dragging if we're all readonly
        If [ReadOnly] Then Return
        HighlightedNode = Nothing
        HoverGroup = Nothing

        Dim mems = e.Data.GetData(Of ICollection(Of IGroupMember))()
        If mems Is Nothing Then Return

        Dim targetGroup As IGroup = GetDragTargetGroup(New Point(e.X, e.Y), mems)
        If targetGroup Is Nothing Then Return

        ' Can't drag resources into a pool (yet). 
        If targetGroup.RawGroup.MemberType = GroupMemberType.Pool Then Return

        Dim processedMems As New List(Of IGroupMember)

        Try

            ClearGroupCache()

            ' Get the node for the group we're moving it to
            Dim n As TreeNode = GetNodeFor(targetGroup)
            For Each mem As IGroupMember In mems

                ' Can't drag/drop default group.
                Dim group = TryCast(mem, IGroup)
                If group IsNot Nothing Then
                    If group.IsDefault Then Continue For
                End If

                ' Note that if this drag/drop is into the root group and the roots
                ' are not displayed, n may well be null. We're only really interested
                ' in the node collection anyway
                Dim targNodes As TreeNodeCollection =
                    If(n Is Nothing, tvGroups.Nodes, n.Nodes)

                ' And the node which is holding the member right now
                Dim memNode As TreeNode = GetNodeFor(mem)

                ' Note that memNode may be null if this member is being dragged
                ' in from somewhere else, and n may be null if it is being dragged
                ' into the root and the root is not being displayed
                If e.Effect = DragDropEffects.Move Then

                    ' Check if we have a node; if we don't, that implies that this
                    ' is being dropped from outside the treeview (ie. outside the
                    ' model), so we need to add it, not move it.
                    If memNode Is Nothing Then

                        ' We need to add it to the target group first
                        targetGroup.Add(mem)

                    Else
                        ' Move the member to the new group; data first
                        ' If it succeeds (returns non-null), we remove the original
                        ' node - the view will be updated later, but the node being
                        ' in the tree will confuse when it tests if a node for this
                        ' member already exists or not
                        Dim selectedParentGroup = mem.Owner
                        Dim args As New GroupMemberRequestDropEventArgs(mem, targetGroup, False)
                        OnGroupMemberRequestDrop(args)
                        If args.Cancel Then Continue For
                        If mem.MoveTo(targetGroup) IsNot Nothing Then

                            If mem.IsGroup Then
                                FlushGroupFromCache(CType(mem, IGroup))
                                SubGroupCacheReset(mem)
                            End If
                            memNode.Remove()
                        End If
                    End If
                ElseIf e.Effect = DragDropEffects.Copy Then
                    If mem.MemberType = GroupMemberType.Pool Then Return

                    Dim args As New GroupMemberRequestDropEventArgs(mem, targetGroup, True)
                    OnGroupMemberRequestDrop(args)
                    If args.Cancel Then Continue For

                    ' Deal with the data
                    mem.CopyTo(targetGroup)
                    ClearGroupCacheAtTreeLevel(targetGroup)
                    ClearGroupCacheAtTreeLevel(mem.Owner)
                End If
                If mem IsNot Nothing Then processedMems.Add(mem)

            Next

            UpdateView(False)

            If n IsNot Nothing Then
                tvGroups.SelectedNode = n
                ' Make sure that the group containing the members is expanded
                n.Expand()
            End If

            ' Indicate to any listeners that group members have been dropped.
            ' Note that only those which were actually processed are included, not
            ' those which got skipped for any reason.
            If processedMems.Count > 0 Then
                OnGroupMemberDropped(New GroupMemberDropEventArgs(targetGroup, processedMems))
                mSelectedNodes.Clear()
            End If
        Catch ge As GroupMoveException
            'need to move this into resources.
            UserMessage.Show(My.Resources.GroupTreeControl_ErrorMoveGroupTree)

            'flush cache at correct levels
            Dim srcId = GetMemberFor(GetNodeFor(ge.SourceId))
            Dim destId = GetMemberFor(GetNodeFor(ge.DestinationId))
            If srcId IsNot Nothing Then FlushItemFromCache(srcId)
            If destId IsNot Nothing Then FlushItemFromCache(destId)

            'refresh the screen
            RaiseEvent RefreshView(Me, New EventArgs())
        Catch gde As GroupDeletedException
            UserMessage.Show(My.Resources.GroupTreeControl_TheItemYouAreTryingToMoveHasAlreadyBeenMovedRenamedOrDeletedClickOKToRefreshTheTree)
            RaiseEvent RefreshView(Me, New EventArgs())
        Catch noSuchElementEx As NoSuchElementException
            UserMessage.Show(My.Resources.GroupTreeControl_ErrorTreeElementRemoved)
            RaiseEvent RefreshView(Me, New EventArgs())
        Catch ex As Exception
            UserMessage.Err(ex,
             My.Resources.GroupTreeControl_AnErrorOccurredWhileUpdatingTheTree0, ex.Message)

        End Try

    End Sub

    Private Sub HandleApplyFilter(sender As Object, e As FilterEventArgs) Handles txtFilter.FilterTextChanged
        ApplyTreeFilter(e.FilterText)
    End Sub

    Private Sub HandleClearFilter(sender As Object, e As EventArgs) Handles txtFilter.FilterCleared
        tvGroups.ClearFilter()
        ClearGroupCache()
        mIsFiltered = tvGroups.IsFiltered
    End Sub

    Private Sub HandleSearchClick(sender As Object, e As EventArgs) Handles txtFilter.FilterIconClick
        txtFilter.SelectAll()
    End Sub

    Private Sub SubGroupCacheReset(group As IGroupMember)
        Dim n As TreeNode = GetNodeFor(group)

        'if the nodes has has no sub-nodes, then nothing to clear
        If n.Nodes Is Nothing Then Return

        For Each groupChild As TreeNode In n.Nodes
            Dim groupItem = TryCast(groupChild.Tag, IGroupMember)
            If groupItem IsNot Nothing AndAlso groupItem.IsGroup Then
                FlushGroupFromCache(CType(groupItem, IGroup))
                SubGroupCacheReset(groupItem)
            End If
        Next
    End Sub
    
#End Region

#Region " Context Menu Event Handlers "


    ''' <summary>
    ''' Handles a 'Create Item' request being made.
    ''' Note that this is added dynamically to the toolstrip menu items that are
    ''' stored in the <see cref="mCreateMenuItems"/> collection.
    ''' </summary>
    Private Sub HandleCreateItemClick(sender As Object, e As EventArgs)
        Dim mi = TryCast(sender, ToolStripMenuItem)
        If mi Is Nothing Then Return

        Dim tp = DirectCast(mi.Tag, GroupMemberType)
        CreateNewItemInSelectedGroup(tp)
    End Sub

    ''' <summary>
    ''' Handles a 'Create Group' request being made
    ''' </summary>
    Private Sub HandleCreateGroupClick() Handles menuCreateGroup.Click
        ' Otherwise, get the target group
        Dim parentGroup As IGroup = OperatingGroup
        If parentGroup Is Nothing Then Return

        Dim parentNode As TreeNode = GetNodeFor(parentGroup)
        ' Create a node
        Dim nodes = If(parentNode Is Nothing, tvGroups.Nodes, parentNode.Nodes)
        Dim n = nodes.Add(Nothing, "",
         ImageLists.Keys.Component.OpenGroup, ImageLists.Keys.Component.OpenGroup)

        ' Add a placeholder so we know what we're trying to create later
        n.Tag = New NewGroupPlaceholder()

        ' Make sure it's selected
        tvGroups.SelectedNode = n

        ' If selection was cancelled, make sure we exit as this point - effectively,
        ' that means that the CreateGroup was cancelled
        If tvGroups.SelectedNode IsNot n Then n.Remove() : Return

        ' Otherwise, ensure that the node is in view
        n.EnsureVisible()

        ' And being editing it - the AfterLabelEdit handler will deal with updating
        ' the model
        n.BeginEdit()

    End Sub

    ''' <summary>
    ''' Handles a 'Rename Group' request being made
    ''' </summary>
    Private Sub HandleRenameGroupClick() Handles menuRenameGroup.Click
        Dim mem As IGroupMember = TargetMember
        If mem Is Nothing Then Return

        Dim n As TreeNode = GetNodeFor(mem)
        If n Is Nothing Then Return
        If Not CanEditLabel(n) Then Return

        Debug.Assert(n.IsSelected)

        n.BeginEdit()
    End Sub

    Private Sub OnDeleteGroupClick()
        UpdateSelectedMember()
        HandleDeleteGroupClick()
    End Sub

    ''' <summary>
    ''' Handles a 'Delete Group' request being made
    ''' </summary>
    Private Sub HandleDeleteGroupClick() Handles menuDeleteGroup.Click
        Dim g As IGroup = TargetGroup
        Dim n As TreeNode = GetNodeFor(g)
        If g Is Nothing Then Return

        Debug.Assert(n IsNot Nothing,
                     My.Resources.GroupTreeControl_NoNodeForTheGroupWeReRemovingThatCanTBeRight)

        ' Validate the state
        Try
            ValidateGroupDelete(g)
            ValidateUserCanDeleteGroup(g)
        Catch ex As Exception When _
                TypeOf ex Is GroupDeletedException OrElse TypeOf ex Is PermissionException
            MessageBox.Show(ex.Message, My.Resources.GroupTreeControl_DeleteGroup, MessageBoxButtons.OK)
            Return
        End Try


        ' The state has been validated. Confirm with the user
        If ConfirmUserGroupDelete(g) <> DialogResult.OK Then Return

        ' Perform the delete
        Try
            ClearGroupCacheAtTreeLevel(g)
            g.Delete()
            UpdateView()

        Catch ex As Exception
            UserMessage.Err(ex,
             My.Resources.GroupTreeControl_AnErrorOccurredWhileAttemptingToDeleteTheGroup0,
             ex.Message)

        End Try

    End Sub

    ''' <summary>
    ''' Expands all the nodes in the treeview
    ''' </summary>
    Private Sub HandleExpandAll(sender As Object, e As EventArgs) _
     Handles menuExpandAll.Click
        tvGroups.ExpandAll()
    End Sub

    ''' <summary>
    ''' Collapses all the nodes in the treeview
    ''' </summary>
    Private Sub HandleCollapseAll(sender As Object, e As EventArgs) _
     Handles menuCollapseAll.Click
        tvGroups.CollapseAll()
    End Sub

    ''' <summary>
    ''' Handles a group member being removed from its owning group
    ''' </summary>
    Private Sub HandleRemoveFromGroup(sender As Object, e As EventArgs) _
     Handles menuRemoveFromGroup.Click
        If EnableMultipleSelect AndAlso mSelectedNodes.Count > 1 Then
            For Each n As TreeNode In mSelectedNodes
                Dim groupMember = TryCast(n.Tag, IGroupMember)
                If groupMember Is Nothing Then Return
                TryRemoveMember(groupMember)
            Next
        Else
            Dim groupMember As IGroupMember = TargetMember
            If groupMember Is Nothing Then Return
            TryRemoveMember(groupMember)
        End If
        ClearGroupCache()
        RaiseEvent RefreshView(Me, New EventArgs())
    End Sub

    Private Sub TryRemoveMember(groupMember As IGroupMember)

        Try
            If mGroupPermissionLogic Is Nothing Then mGroupPermissionLogic = New GroupPermissionLogic()
            ClearCacheValues()
            If mGroupPermissionLogic.ValidateMoveMember(groupMember, groupMember.Owner,
                                                        DefaultGroup(groupMember), False, AddressOf ShowConfirmDialog, User.Current) Then

                ' Remove the member from its group - if it reappears in the root as result, a
                ' view update will generate the node required to display it...
                groupMember.Remove()
            End If
        Catch ex As Exception
            UserMessage.Err(ex, String.Format(My.Resources.GroupTreeControl_UnableToRemoveSpecifiedProcessFromGroup0, ex.Message))
        End Try
    End Sub

    ''' <summary>
    ''' Handles an 'Access Rights' request being made
    ''' </summary>
    Private Sub HandleManagePermissionsClick() Handles menuPermissions.Click
        Dim grp As IGroup = TargetGroup
        If grp IsNot Nothing Then
            ' If the member is a group then show the permission restriction
            ' configuration for it
            Using f As New frmGroupPermissions(grp)
                f.SetEnvironmentColoursFromAncestor(Me)
                f.ShowInTaskbar = False
                f.StartPosition = FormStartPosition.CenterParent
                If f.ShowDialog() = DialogResult.OK Then
                    FlushGroupFromCache(grp)
                    UpdateView(True)
                End If
            End Using
        Else
            ' If the member is not a group then show the effective permissions
            ' for it
            Dim mem As IGroupMember = TargetMember
            If mem IsNot Nothing Then
                Using f As New frmEffectivePermissions(mem)
                    f.SetEnvironmentColoursFromAncestor(Me)
                    f.ShowInTaskbar = False
                    f.StartPosition = FormStartPosition.CenterParent
                    f.ShowDialog()
                End Using
            End If
        End If
    End Sub


    ''' <summary>
    ''' Handles a context menu opening within the tree, ensuring that it only opens
    ''' when appropriate (ie. when manipulation is enabled), and the items are
    ''' shown/hidden/enabled/disabled as the context dictates.
    ''' </summary>
    Private Sub HandleContextMenuOpening(sender As Object, e As CancelEventArgs) _
     Handles ctxGroup.Opening
        ' Assume no target
        mTarget = Nothing

        ' Rules for allowing context menus to manipulate nodes
        ' 1) Not ReadOnly
        ' 2) A node is selected (which is used as the target) -or-
        ' 3) No node is selected and a single tree being displayed (in which case
        '    the 'root' node is used as the target)
        
        ' No manipulations for readonly nodes or no trees
        If [ReadOnly] OrElse mTrees.Count = 0 Then e.Cancel = True : Return

        UpdateSelectedMember()

        ' At this point, if we don't have a target member, there's nothing to edit
        If mTarget Is Nothing Then e.Cancel = True : Return

        ' Now we need to figure out what we can do, given the target we have
        ' If it's a group, determine whether creating an item is enabled
        ' Rename Item is not enabled/disabled if they are not supported -
        ' rather they are just hidden if they cannot be done.
        menuRenameItem.Visible = ItemRenameEnabled
        menuCloneItem.Visible = ItemCloneEnabled
        menuDeleteItem.Visible = ItemDeleteEnabled
        menuEditItem.Visible = ItemEditEnabled
        ClearCacheValues()

        ' Get the name of the type 
        Dim itemType As String = Nothing
        If Not mTarget.IsGroup Then
            ' If the target is an item, just get the name of the item type
            itemType = mTarget.MemberType.GetLocalizedFriendlyName()
        Else
            ' Get the first non-group type supported by the group (effv by the tree)
            Dim tp = DirectCast(mTarget, IGroup).SupportedTypes.FirstOrDefault(
                 Function(t) t <> GroupMemberType.Group
            )
            itemType = If(tp = GroupMemberType.None, My.Resources.GroupTreeControl_Item, tp.GetLocalizedFriendlyName())
        End If

        ' If the user can has permission to, they can call the Access Rights
        ' form. This also checks to see if we're trying to manage a folder (group)
        ' if not, don't enable the control.
        Dim gp As IGroup = TryCast(mTarget, IGroup)
        menuPermissions.Enabled = False
        menuPermissions.ToolTipText = String.Empty
        If menuPermissions.Available Then
            menuPermissions.Enabled = Not mTarget.IsRoot
            menuPermissions.Text = My.Resources.GroupTreeControl_AccessRights

            If mTarget.IsGroup Then
                ' Disable access rights if it contains hidden items
                menuPermissions.Enabled = If(mTarget.Id Is Nothing, False,
                    gp.CanChangeAccessRights(menuPermissions.ToolTipText))
            Else
                menuPermissions.Enabled = True
            End If
        End If
        If EnableMultipleSelect AndAlso mSelectedNodes.Count > 1 Then
            menuPermissions.Enabled = False
        End If
        
        Dim permission = mTarget.Tree.TreeType.GetTreeDefinition().EditPermission

        Dim hasPermission = False
        If permission Is Nothing Then

            ' Queues and Users don't have a top level root node permissions object.
            hasPermission = True
        Else
            hasPermission = If(mTarget.IsRoot, User.Current.HasPermission(permission),
                                               mTarget.Permissions.HasPermission(User.Current, permission))
        End If

        ' If the user can rename them in this control, check their permission and
        ' the target item to fill out the menu label and enabled state
        If menuRenameItem.Available Then
            menuRenameItem.Enabled = hasPermission AndAlso mTarget.IsMember
            menuRenameItem.Text = String.Format(My.Resources.GroupTreeControl_Rename0, itemType).ToSentenceCase()
        Else
            ' Probably redundant, but I guess we don't want to enable them just in
            ' case the user can trigger them some other way (keyboard shortcuts etc)
            menuRenameItem.Enabled = False
        End If

        ' Same with the 'Clone Item' menu item
        If menuCloneItem.Available Then
            menuCloneItem.Enabled = hasPermission AndAlso mTarget.IsMember
            menuCloneItem.Text = String.Format(My.Resources.GroupTreeControl_Clone0, itemType).ToSentenceCase()
        Else
            menuCloneItem.Enabled = False
        End If

        ' Likewise with the 'Delete Item' menu item
        Dim treeDef = mTarget.Tree.TreeType.GetTreeDefinition()
        menuDeleteItem.Text = String.Format(My.Resources.GroupTreeControl_Delete0, itemType).ToSentenceCase()

        menuDeleteItem.Enabled =
            menuDeleteItem.Available AndAlso mTarget.IsMember AndAlso
            mTarget.Permissions.HasPermission(User.Current, treeDef.DeleteItemPermission)

        menuEditItem.Text = String.Format(My.Resources.GroupTreeControl_Edit0, itemType).ToSentenceCase()

        menuEditItem.Enabled =
            menuEditItem.Available AndAlso mTarget.IsMember AndAlso
            mTarget.Permissions.HasPermission(User.Current, treeDef.EditPermission)

        ' Only show the dependencies option if the target has a dependency object
        menuFindRefs.Visible = (mTarget.Dependency IsNot Nothing)

        ' We add the 'create item' menu items dynamically based on the types
        ' available in the tree selected
        SetCreateItemMenuItems(mTarget.Tree)

        Dim isNonRootGroup As Boolean = (gp IsNot Nothing AndAlso Not gp.IsRoot)

        ' Can't remove from group if owner is a root (ie. it has no real group)
        ' or if it is actually a group (that's more like 'Delete Group')
        menuRemoveFromGroup.Enabled = mTarget.CanBeRemovedFromGroup(User.Current)

        ' In fact, it really shouldn't be there for group nodes
        menuRemoveFromGroup.Visible = mTarget.IsMember

        ' 'Rename Group' only makes sense if non-root group is target
        menuRenameGroup.Enabled =
            isNonRootGroup AndAlso GroupRenameEnabled AndAlso hasPermission

        ' Make the 'Manage Access Rights' menu visible if it has been enabled
        ' on this control.
        menuPermissions.Visible = ManageAccessRightsEnabled And Not mTarget.IsPool

        ' Don't allow create/rename/delete for pools or poolmembers
        Dim isItemInMenuRelatedToGroup = IsMenuItemRelatedToGroup()
        menuCreateGroup.Visible = isItemInMenuRelatedToGroup
        menuRenameGroup.Visible = isItemInMenuRelatedToGroup
        menuDeleteGroup.Visible = isItemInMenuRelatedToGroup


        ' If an item is targeted it creates a group at the same level as the item
        menuCreateGroup.Enabled = hasPermission

        ' 'Delete Group' only makes sense if group is target and it is not the root and not the default group
        menuDeleteGroup.Enabled = mTarget.IsGroup _
            AndAlso gp.CanDeleteGroup(menuDeleteGroup.ToolTipText) AndAlso hasPermission AndAlso isNonRootGroup

        ' Allow other listeners to have a go at their extras
        Dim gmArgs As New GroupMemberContexMenuOpeningEventArgs(mTarget, ctxGroup)
        OnContextMenuOpening(gmArgs)

        ' If the opening is cancelled, chain that info onto our own args and exit
        If gmArgs.Cancel Then e.Cancel = True : Return

        ' Only show the top separator if any item functions are available
        sepCreateRenameDelete.Visible = (
            AnyCreateMenuItemsAvailable OrElse
            menuRenameItem.Available OrElse
            menuCloneItem.Available OrElse
            menuDeleteItem.Available
        )

        ' Only show the Item/Group separator if 'Find Refs' is available
        sepItemGroup.Visible = menuFindRefs.Available

        ' Only show the GroupAll separator if create/rename/delete or remove are available
        sepGroupAll.Visible = (
            menuCreateGroup.Available OrElse
            menuRenameGroup.Available OrElse
            menuDeleteGroup.Available OrElse
            menuRemoveFromGroup.Available
        )

        ' Only show the permissions separator if permissions are available
        sepPermissionsGroup.Visible = menuPermissions.Available

        ' Only show the extra items separator if any of the extra items are visible
        sepExtraItems.Visible = ExtraContextMenuItems.Any(Function(mi) mi.Available)

    End Sub

    ''' <summary>
    ''' Handles a 'Clone Item' request being made
    ''' </summary>
    Private Sub HandleCloneItemClick() Handles menuCloneItem.Click
        Dim gp As IGroup = OperatingGroup
        If gp Is Nothing Then Return

        Dim mem As IGroupMember = TargetMember
        If mem Is Nothing OrElse mem.IsGroup Then Return

        Try
            Dim arg As New CloneGroupMemberEventArgs(mem)
            OnCloneRequested(arg)

            ' Let's see if we have a new member...
            Dim clone As IGroupMember = arg.CreatedItem

            ' Nothing created - it was a no-op
            If clone Is Nothing Then Return

            ' Otherwise, we want to add the new member to the group it was created in
            gp.Add(clone)

            ' And update the tree
            Dim parentNode As TreeNode = GetNodeFor(gp)
            Dim newNode As TreeNode
            If parentNode IsNot Nothing Then
                newNode = AddMember(clone, parentNode.Nodes)
                parentNode.Expand()
            Else
                newNode = AddMember(clone, tvGroups.Nodes)
            End If
            If newNode IsNot Nothing Then
                newNode.EnsureVisible()
                tvGroups.SelectedNode = newNode
            End If


        Catch ex As Exception
            UserMessage.Err(
                ex, My.Resources.GroupTreeControl_AnErrorOccurredCloning01, mem.Name, ex.Message)

        End Try

    End Sub

    Private Sub HandleEditItemClick(sender As Object, e As EventArgs) _
     Handles menuEditItem.Click
        EditItem()
    End Sub

    ''' <summary>
    ''' Handles the 'Delete Item' menu item being clicked. This delegates primarily
    ''' to a handler which knows how to delete items of the type selected.
    ''' </summary>
    Private Sub HandleDeleteItemClick(sender As Object, e As EventArgs) _
     Handles menuDeleteItem.Click
        Dim mem As IGroupMember = SelectedMember
        If mem Is Nothing Then Return
        Try
            OnDeleteRequested(New GroupMemberEventArgs(mem))
            FlushGroupFromCache(mem.Owner)
        Catch ex As Exception
            UserMessage.Err(ex,
             My.Resources.GroupTreeControl_AnErrorOccurredWhileDeleting01, mem.Name, ex.Message)
        End Try
        UpdateView(True)
    End Sub

    ''' <summary>
    ''' Handles the 'View Dependencies' menu item being clicked.
    ''' </summary>
    Private Sub HandleFindReferences(sender As Object, e As EventArgs) _
     Handles menuFindRefs.Click
        ' Get the selected member - abort if there is none
        Dim mem As IGroupMember = SelectedMember
        If mem Is Nothing Then Return

        ' Get the App form hosting this control - again, bail if not there
        Dim frm = UIUtil.GetAncestor(Of frmApplication)(Me)
        If frm Is Nothing Then Return

        ' Get the dependency for the group member. Last chance to leave the room
        Dim dep As clsProcessDependency = mem.Dependency
        If dep Is Nothing Then Return

        frm.FindReferences(dep)

    End Sub

    ''' <summary>
    ''' Gets the node collection corresponding to the given root group - this will
    ''' either be the treeview's node collection or the node collection of the node
    ''' in the treeview which represents the root of the tree, depending on whether
    ''' the root is visible for the specified tree.
    ''' </summary>
    ''' <param name="root">The root group for which the node collection is required.
    ''' </param>
    ''' <returns>The node collection which represents the contents of the root of the
    ''' tree, regardles of whether the root group itself is represented in the
    ''' treeview.</returns>
    Private Function GetRootNodeCollection(root As IGroup) As TreeNodeCollection
        Dim rootNode As TreeNode = Nothing
        mTrees.TryGetValue(root.Tree, rootNode)
        Return If(rootNode Is Nothing, tvGroups.Nodes, rootNode.Nodes)
    End Function

    ''' <summary>
    ''' Draw '(default)' tag on default group tree nodes. Also draw manual highlighting as drawdefault
    ''' won't do that (or does white on white) when pushed.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleTreeViewNodeDraw(sender As Object, e As DrawTreeNodeEventArgs) _
        Handles tvGroups.DrawNode
        Dim name As String = Nothing
        Dim status As String = Nothing
        Dim resourceGroupMember = TryCast(e.Node.Tag, ResourceGroupMember)
        If resourceGroupMember Is Nothing Then
            Dim group = TryCast(e.Node.Tag, IGroup)
            If (group IsNot Nothing AndAlso group.IsInModel) Then
                name = group.Name
                If group.IsDefault Then
                    status = My.Resources.AutomateUIClasses_Resources.GroupTreeControl_Default
                End If
            End If
        Else
            name = resourceGroupMember.Name
            status = GetResourceNodeStatusText(resourceGroupMember)
        End If

        If name Is Nothing Then
            e.DrawDefault = True
            Return
        End If

        If mEnvColours Is Nothing Then mEnvColours = UIUtil.GetAncestor(Of IEnvironmentColourManager)(Me)
        If EnableMultipleSelect Then
            If Not String.IsNullOrWhiteSpace(status) Then
                Dim nodeFont = If(e.Node.NodeFont, e.Node.TreeView.Font)
                Dim font = New Font(nodeFont, FontStyle.Italic)
                e.Graphics.DrawString(status, font, Brushes.Gray, e.Bounds.Right + 1, e.Bounds.Top)
            End If
            PaintSelectedNodes(e)
        Else
            If Not e.Node.BackColor = mEnvColours.EnvironmentBackColor Then
                e.DrawDefault = True
            Else
                Dim nodeFont = If(e.Node.NodeFont, e.Node.TreeView.Font)
                Dim font = New Font(nodeFont, FontStyle.Regular)

                Using bg = New SolidBrush(e.Node.BackColor)
                    Dim textSize = e.Graphics.MeasureString(name, font)
                    e.Graphics.FillRectangle(bg, e.Bounds.Left, e.Bounds.Top, textSize.Width, textSize.Height)
                End Using
                Using fgb = New SolidBrush(e.Node.ForeColor)
                    e.Graphics.DrawString(name, font, fgb, e.Bounds.Left, e.Bounds.Top)
                End Using
            End If

            If Not String.IsNullOrWhiteSpace(status) Then
                Dim nodeFont = If(e.Node.NodeFont, e.Node.TreeView.Font)
                Dim font = New Font(nodeFont, FontStyle.Italic)
                e.Graphics.DrawString(status, font, Brushes.Gray, e.Bounds.Right + 1, e.Bounds.Top)
            Else
                e.DrawDefault = True
            End If
        End If
    End Sub

    Private Sub PaintSelectedNodes(e As DrawTreeNodeEventArgs)

        If mSelectedNodes IsNot Nothing AndAlso mSelectedNodes.Contains(e.Node) Then

            Dim g = e.Graphics

            Using brush = New SolidBrush(SystemColors.Highlight)
                    g.FillRectangle(brush, e.Bounds)
            End Using
            TextRenderer.DrawText(e.Graphics, e.Node.Text, Font, e.Bounds, Color.White, TextFormatFlags.Left Or TextFormatFlags.Top)
            
        Else
            e.DrawDefault = true
        End If
    End Sub

    Private Function GetResourceNodeStatusText(resource As ResourceGroupMember) As String
        Dim result As New StringBuilder()
        If resource.Configuration.LoggingDefault <> CombinedConfig.CombinedState.Enabled Then
            result.Append($"{My.Resources.AutomateUIClasses_Resources.GroupTreeControl_LoggingLevel}: ")
            If (resource.Configuration.LoggingAllOverride = CombinedConfig.CombinedState.Enabled) Then
                result.Append(My.Resources.AutomateUIClasses_Resources.GroupTreeControl_AllStages)
            ElseIf (resource.Configuration.LoggingErrorsOnlyOverride = CombinedConfig.CombinedState.Enabled) Then
                result.Append(My.Resources.AutomateUIClasses_Resources.GroupTreeControl_ErrorsOnly)
            ElseIf (resource.Configuration.LoggingKeyOverride = CombinedConfig.CombinedState.Enabled) Then
                result.Append(My.Resources.AutomateUIClasses_Resources.GroupTreeControl_KeyStages)
            End If
        End If
        Return If(String.IsNullOrWhiteSpace(result.ToString()), String.Empty, $" ({result.ToString()})")
    End Function

    'Returns the bounds of the specified node, including the region 
    ' occupied by the node label and any node tag displayed.
    Private Function NodeBounds(node As TreeNode) As Rectangle

        Dim bounds = node.Bounds
        If node.Tag IsNot Nothing Then

            ' Retrieve a Graphics object from the TreeView handle
            'and use it to calculate the display width of the tag.
            Dim g = tvGroups.CreateGraphics()
            Dim nodeFont = If(node.NodeFont, node.TreeView.Font)
            Dim tagWidth = CInt(g.MeasureString(node.Tag.ToString(), nodeFont).Width + 6)

            ' Adjust the node bounds using the calculated value.
            bounds.Offset(CInt(tagWidth / 2), 0)
            bounds = Rectangle.Inflate(bounds, CInt(tagWidth / 2), 0)
            g.Dispose()
        End If

        Return bounds
    End Function

    Private Function IsMenuItemRelatedToGroup() As Boolean
        Dim resource = TryCast(mTarget, ResourceGroupMember)
        Dim poolMember = mTarget.IsMember AndAlso resource IsNot Nothing AndAlso resource.IsPoolMember
        Dim poolOrPoolMember = mTarget.IsPool OrElse poolMember

        Return Not poolOrPoolMember AndAlso mTarget.MemberType = GroupMemberType.Group
    End Function

#End Region

#Region " Other Event Handlers "

    ''' <summary>
    ''' Handles the 'group hover' timer ticking over by resetting the hovered group
    ''' and expanding its node.
    ''' </summary>
    Private Sub HandleHoverTimerTick() Handles timerGroupHover.Tick
        ' Reset the group and stop the timer - we only want to do this once
        Dim gp As IGroup = HoverGroup
        HoverGroup = Nothing

        ' Just check we're not running in a disposed tree control
        If IsDisposed Then Return

        ' Get the node corresponding to the group and expand it
        Dim n As TreeNode = GetNodeFor(gp)
        If n IsNot Nothing Then n.Expand()
    End Sub

    Public Function CanLeave() As Boolean
        Return True
    End Function

    ''' <summary>
    ''' Public function that handles the editing of a member. So that it can be called from outside
    ''' GroupTreeControl for when edits are prompted externally
    ''' </summary>
    Public Sub EditItem()
        Dim mem As IGroupMember = SelectedMember
        If mem Is Nothing Then Return
        Try
            OnEditRequested(New GroupMemberEventArgs(mem))
            FlushGroupFromCache(mem.Owner)
        Catch ex As Exception
            UserMessage.Err(ex,
             My.Resources.GroupTreeControl_AnErrorOccurredWhileEditing01, mem.Name, ex.Message)
        End Try
        UpdateView()
    End Sub

    ''' <summary>
    ''' Remove the relevant item from the cache if it exists 
    ''' </summary>
    ''' <param name="groupMember"></param>
    Public Sub FlushItemFromCache(groupMember As IGroupMember)
        'If the selected member is a group, then we want to refresh the current group, not the owner
        If groupMember IsNot Nothing AndAlso
           TypeOf groupMember Is IGroup Then
            FlushGroupFromCache(DirectCast(groupMember, IGroup))
        Else
            'if the selected member is not a group then delete owner to reset cache.
            FlushGroupFromCache(groupMember.Owner)
        End If
    End Sub

    ''' <summary>
    ''' Remove a given group cache only if it exists.
    ''' </summary>
    ''' <param name="group"></param>
    Public Sub FlushGroupFromCache(group As IGroup)
        If group IsNot Nothing AndAlso mGroupInfoCache.ContainsKey(group) Then
            mGroupInfoCache.Remove(group)
        End If
        ClearCacheValues()
    End Sub


    ''' <summary>
    ''' Clears cache values associated with a given node, also clears the parent node.
    ''' </summary>
    ''' <param name="group"></param>
    Private Sub ClearGroupCacheAtTreeLevel(group As IGroup)

        Dim groupInfoList As List(Of GroupInfo) = Nothing
        If mGroupInfoCache.TryGetValue(group, groupInfoList) Then
            For Each l In groupInfoList.Where(Function(t) t.MemberType = GroupMemberType.Group)
                Dim g = CType(l.Group, IGroup)
                FlushGroupFromCache(g)
            Next
            FlushGroupFromCache(group)
        End If
        'also need to clear the owner, as this will still reference 
        If group.Owner IsNot Nothing Then FlushGroupFromCache(group.Owner)
        ClearCacheValues()
    End Sub


    ''' <summary>
    ''' Clear all the caches and reset
    ''' </summary>
    Public Sub ClearGroupCache()
        ClearCacheValues()
        'Then empty the cache
        mGroupInfoCache.Clear()
    End Sub

    ''' <summary>
    ''' Clear all the group node id cache
    ''' </summary>
    Private Sub ClearCacheValues()
        ClearCacheValuesNodes(tvGroups.Nodes)
    End Sub

    Private Sub ClearCacheValuesNodes(nodes As TreeNodeCollection)

        For Each n In nodes.Cast(Of TreeNode)
            Dim groupMember = TryCast(n.Tag, IGroupMember)
            If groupMember.IsGroup Then groupMember?.ClearLocalGroupCache()
            If n.Nodes.Count > 0 Then
                ClearCacheValuesNodes(n.Nodes)
            End If
        Next
    End Sub
    
#End Region

    Private Class GroupInfo
        Public ReadOnly Property IdString As String
        Public ReadOnly Property MemberType As GroupMemberType
        Public ReadOnly Property Path As String
        Public ReadOnly Property Group As IGroupMember

        Public Sub New(idString As String, memberType As GroupMemberType, path As String, group As IGroupMember)
            Me.IdString = idString
            Me.MemberType = memberType
            Me.Path = path
            Me.Group = group
        End Sub

        Public Sub New(group As IGroupMember)
            Me.New(If(group.Id?.ToString(), String.Empty), group.MemberType, group.Path, group)
        End Sub
    End Class
End Class
