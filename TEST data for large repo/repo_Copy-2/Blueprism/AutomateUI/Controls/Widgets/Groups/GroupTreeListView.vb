Imports System.Reflection
Imports AutomateControls
Imports AutomateControls.TreeList
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Images
Imports NLog

Public Class GroupTreeListView : Inherits TreeListView

#Region " GroupTreeListView Comparer "

    ''' <summary>
    ''' Comparer for items in the GroupTreeListView
    ''' </summary>
    Public Class GroupTreeListViewComparer : Implements ITreeListViewItemComparer



        ' The sort order
        Public Property SortOrder As System.Windows.Forms.SortOrder Implements ITreeListViewItemComparer.SortOrder

        ' The column to sort on
        Public Property Column As Integer Implements ITreeListViewItemComparer.Column

        ''' <summary>
        ''' Compares two items in the TreeListView.
        ''' </summary>
        ''' <param name="x">The first item</param>
        ''' <param name="y">The second item</param>
        ''' <returns>The result of the comparison</returns>
        Public Function Compare(x As Object, y As Object) As Integer Implements ITreeListViewItemComparer.Compare
            Dim a = CType(x, TreeListViewItem)
            Dim b = CType(y, TreeListViewItem)

            ' Ensure groups are kept separate from other items, and are always
            ' shown at the top
            If ItemIsGroup(a) AndAlso Not ItemIsGroup(b) Then
                Return -1
            ElseIf Not ItemIsGroup(a) AndAlso ItemIsGroup(b) Then
                Return 1
            End If

            If (Column < a.SubItems.Count AndAlso Column < b.SubItems.Count) Then
                ' If we have groups and one is the default group then ensure
                ' it is always displayed first
                If ItemIsGroup(a) AndAlso CType(a.Tag, IGroup).IsDefault Then
                    Return -1
                ElseIf ItemIsGroup(b) AndAlso CType(b.Tag, IGroup).IsDefault Then
                    Return 1
                End If
                ' The two nodes we're comparing are both groups or both non-groups
                ' Just compare by text in that case, using the current culture.
                Return If(SortOrder = SortOrder.Descending, -1, 1) *
                    String.Compare(a.SubItems(Column).Text, b.SubItems(Column).Text,
                    StringComparison.CurrentCultureIgnoreCase)
            End If

            Return 0
        End Function

        ''' <summary>
        ''' Determines whether or not the passed TreeListViewItem represents a group.
        ''' </summary>
        ''' <param name="item">The item to check</param>
        ''' <returns>True if the item is a group, otherwise False</returns>
        Private Function ItemIsGroup(item As TreeListViewItem) As Boolean
            If item Is Nothing Then Return False
            Dim grp = TryCast(item.Tag, IGroup)
            If grp Is Nothing Then Return False
            Return True
        End Function

    End Class

#End Region

#Region " Class-scope Declarations "

    ''' <summary>
    ''' Filter which always allows all group members
    ''' </summary>
    Private Shared Function TrueFilter(m As IGroupMember) As Boolean
        Return True
    End Function

#End Region

#Region " Published Events "

    ''' <summary>
    ''' Event fired when a group member of a collection of group members is dropped
    ''' onto a group member within this control.
    ''' </summary>
    Public Event GroupMemberDropped As GroupMemberDropEventHandler

    ''' <summary>
    ''' Event fired when a group item is activated in this control
    ''' </summary>
    Public Event GroupItemActivated As GroupMemberEventHandler

    ''' <summary>
    ''' Event fired when a group tree list view item is added to this control
    ''' </summary>
    Public Event GroupTreeListViewItemAdded As GroupTreeListViewItemEventHandler

    ''' <summary>
    ''' Event fired when a group tree list view item is updated in this control
    ''' </summary>
    Public Event GroupTreeListViewItemUpdated As GroupTreeListViewItemEventHandler

    ''' <summary>
    ''' Event fired before a collection of group members is to be dragged. This can
    ''' cancel the drag operation before it starts.
    ''' </summary>
    Public Event GroupMembersBeforeDrag As GroupMultipleMemberEventHandler

    ''' <summary>
    ''' Event fired after this control loads the tree and filters it, and before it
    ''' updates the view from it.
    ''' </summary>
    ''' <remarks>Note that this event may not be fired on the UI thread - if that is
    ''' required, the handler should <see cref="Invoke"/> a call back to the UI
    ''' thread.</remarks>
    Public Event TreeLoaded As GroupTreeEventHandler

#End Region

#Region " Member Variables "

    ' The type of tree being shown in this treelistview
    Private mTreeType As GroupTreeType

    ' The tree currently on display
    Private mTree As IGroupTree

    ' The filter applied to this tree list view
    Private mFilter As Predicate(Of IGroupMember)

    ' Whether to show the contents of the tree in flattened form or not
    Private mShowFlat As Boolean

    ' The definitions of the subitems (columns) displayed in this control
    Private mSubitemDefns As IDictionary(Of String, PropertyInfo)

    ' Whether the subitem definitions have changed and the columns need to be updated
    Private mSubitemDefnsChanged As Boolean

    ' Background worker which loads the tree
    Private WithEvents mLoader As New BackgroundWorker()

    ' Indicates an immediate reload is queued after the current loading is finished
    Private mReloadQueued As Boolean

    Private mDragging As Boolean

    ' The currently highlighted node, if there is one
    Private mHighlightedNode As TreeListViewItem

    ' The backcolor of the highlighted node before it was highlighted
    Private mSavedBackColor As Color

    ' The forecolor of the highlighted node before it was highlighted
    Private mSavedForeColor As Color

    ' Force the reload of tree items from the store.
    Private mReloadFromStore As Boolean



#End Region

#Region " Auto-Properties "

    <Browsable(True), Category("Appearance"), DefaultValue(True), Description(
        "Whether to show groups which have no members after filtering")>
    Public Property ShowEmptyGroups As Boolean = True

    <Browsable(True), Category("Behavior"), DefaultValue(True), Description(
        "Indicates if dragging and dropping onto group nodes is allowed")>
    Public Property AllowDropOnGroups As Boolean = True

    <Browsable(True), Category("Behavior"), DefaultValue(True), Description(
        "Indicates if dragging and dropping onto member nodes is allowed")>
    Public Property AllowDropOnMembers As Boolean = True

    <Browsable(True), Category("Behavior"), DefaultValue(True), Description(
        "Indicates if dragging and dropping into blank space is allowed")>
    Public Property AllowDropInSpace As Boolean = True

    <Browsable(True), Category("Behavior"), DefaultValue(False), Description(
        "Indicates if the dragging of groups is allowed")>
    Public Property AllowDragOfGroups As Boolean = False

    <Browsable(True), Category("Behavior"), DefaultValue(True), Description(
        "Indicates if dragging of group members is allowed")>
    Public Property AllowDragOfMembers As Boolean = True

    <Browsable(True), Category("Appearance"), DefaultValue(True), Description(
        "Highlight the group/member being dragged over with valid data")>
    Public Property ShowMemberHighlights As Boolean = True

    <Browsable(True), Category("Behaviour"), DefaultValue(False), Description(
    "Allows the control to save the groups which have been left expanded on a by user basis")>
    Public Property SaveExpandedGroups As Boolean
#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty group tree list view.
    ''' </summary>
    Public Sub New()
        SmallImageList = ImageLists.Components_16x16
        LargeImageList = ImageLists.Components_32x32
        mShowFlat = False
        MultiLevelSelect = True
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the filter to apply to the group tree list view.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Overridable Property Filter As Predicate(Of IGroupMember)
        Get
            If mFilter Is Nothing Then Return AddressOf TrueFilter
            Return mFilter
        End Get
        Set(value As Predicate(Of IGroupMember))
            If value = mFilter Then Return
            mFilter = value
            If Not DesignMode AndAlso IsHandleCreated Then UpdateView(True)
        End Set
    End Property

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property GetRetired As Boolean

    ''' <summary>
    ''' Gets or sets whether this treeview is sortable or not. Typically, it is not.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Shadows Property Sortable As Boolean
        Get
            Return MyBase.Sortable
        End Get
        Set(value As Boolean)
            MyBase.Sortable = value
        End Set
    End Property

    ''' <summary>
    ''' The type of tree represented by this group tree listview.
    ''' </summary>
    <Browsable(True), Category("Data"), DefaultValue(GroupTreeType.None),
     Description("The type of tree to load into this control")>
    Public Property TreeType As GroupTreeType
        Get
            Return mTreeType
        End Get
        Set(value As GroupTreeType)
            If value = mTreeType Then Return
            mTreeType = value
            ' Note that this needs to update from the database because the actual
            ' type of tree to display has changed
            If Not DesignMode AndAlso IsHandleCreated Then UpdateView(True)
        End Set
    End Property

    ''' <summary>
    ''' Gets the model used by this tree list view, if it has been loaded.
    ''' Note that this will be null until the model has been loaded from the
    ''' database.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property Tree As IGroupTree
        Get
            Return mTree
        End Get
    End Property

    ''' <summary>
    ''' The selected group members, or an empty collection if nothing is selected.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property SelectedMembers As ICollection(Of IGroupMember)
        Get
            Return GetSelectedMembers(Of IGroupMember)()
        End Get
    End Property

    ''' <summary>
    ''' Gets the first selected item or null if nothing is selected.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property FirstSelectedItem As IGroupMember
        Get
            Return SelectedMembers.FirstOrDefault()
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets whether to show the data in this control as flat or as
    ''' hierarchical
    ''' </summary>
    <Browsable(True), Category("Appearance"), DefaultValue(False), Description(
        "Shows the tree in flattened form rather than hierarchical form")>
    Public Property ShowFlat As Boolean
        Get
            Return mShowFlat
        End Get
        Set(value As Boolean)
            mShowFlat = value
            If Not DesignMode AndAlso IsHandleCreated Then UpdateView(False)
        End Set
    End Property

    ''' <summary>
    ''' The subitems to display for entries in this group tree list view.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Protected Property SubitemDefinitions As IDictionary(Of String, PropertyInfo)
        Get
            If mSubitemDefns Is Nothing Then
                Dim dict As New clsEventFiringDictionary(Of String, PropertyInfo)(
                    New clsOrderedDictionary(Of String, PropertyInfo)())
                AddHandler dict.ItemAdded, Sub() mSubitemDefnsChanged = True
                AddHandler dict.ItemRemoved, Sub() mSubitemDefnsChanged = True
                AddHandler dict.ItemSet, Sub() mSubitemDefnsChanged = True
                mSubitemDefns = dict
            End If
            Return mSubitemDefns
        End Get
        Set(value As IDictionary(Of String, PropertyInfo))
            ' Set it to null and exit immediately if that's our game
            If value Is Nothing Then mSubitemDefns = Nothing : Return
            Dim dict = SubitemDefinitions
            For Each pair As KeyValuePair(Of String, PropertyInfo) In value
                dict(pair.Key) = pair.Value
            Next
            If IsHandleCreated Then UpdateColumns()
            mSubitemDefnsChanged = False
        End Set
    End Property

    ''' <summary>
    ''' Gets whether a column update is required for this control. This is the case
    ''' if the subitem definitions have changed or if the columns have not yet been
    ''' added.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Protected ReadOnly Property ColumnUpdateRequired As Boolean
        Get
            Return (mSubitemDefnsChanged OrElse Columns.Count = 0)
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
    Protected Property HighlightedNode As TreeListViewItem
        Get
            Return mHighlightedNode
        End Get
        Set(value As TreeListViewItem)
            If value Is mHighlightedNode Then Return
            If mHighlightedNode IsNot Nothing Then
                mHighlightedNode.BackColor = mSavedBackColor
                mHighlightedNode.ForeColor = mSavedForeColor
                mHighlightedNode = Nothing
            End If
            If value IsNot Nothing AndAlso ShowMemberHighlights Then
                mSavedBackColor = value.BackColor
                mSavedForeColor = value.ForeColor
                Dim env = UIUtil.GetAncestor(Of IEnvironmentColourManager)(Me)
                If env IsNot Nothing Then
                    value.BackColor = env.EnvironmentBackColor
                    value.ForeColor = env.EnvironmentForeColor
                Else
                    value.BackColor = SystemColors.Highlight
                    value.ForeColor = SystemColors.HighlightText
                End If
                mHighlightedNode = value
            End If
        End Set
    End Property

#End Region

#Region " Drag/Drop Handling "

    ''' <summary>
    ''' Handles an item being dragged from this treelistview.
    ''' </summary>
    Protected Overrides Sub OnItemDrag(ByVal e As ItemDragEventArgs)
        MyBase.OnItemDrag(e)
        Dim item As ListViewItem = CType(e.Item, ListViewItem)
        If item Is Nothing Then Return

        Dim enu As IEnumerable
        If SelectedItems.Contains(item) _
         Then enu = SelectedItems _
         Else enu = GetSingleton.ICollection(item)

        Dim members As New List(Of IGroupMember)
        For Each it As ListViewItem In enu
            Dim mem As IGroupMember = TryCast(it.Tag, IGroupMember)
            If mem Is Nothing Then Continue For
            Dim gp As IGroup = TryCast(mem, IGroup)
            If gp IsNot Nothing Then
                If AllowDragOfGroups Then members.Add(gp)
            Else
                If AllowDragOfMembers Then members.Add(mem)
            End If
        Next

        If members.Count > 0 Then
            Dim cancelEventArgs As New GroupMultipleMemberEventArgs(members)
            OnGroupMembersBeforeDrag(cancelEventArgs)
            If Not cancelEventArgs.Cancel Then
                If DoDragDrop(Of ICollection(Of IGroupMember))(
                 members, DragDropEffects.Move) = DragDropEffects.Move Then UpdateView()
            End If
        End If

        HighlightedNode = Nothing

    End Sub

    ''' <summary>
    ''' Handles an item being dragged into this treelistview.
    ''' </summary>
    Protected Overrides Sub OnDragEnter(ByVal e As DragEventArgs)
        MyBase.OnDragEnter(e)
        e.Effect =
            If(IsValidDragData(e.Data), DragDropEffects.Move, DragDropEffects.None)
    End Sub

    ''' <summary>
    ''' Handles an drag event leaving this control, ensuring that no nodes are
    ''' highlighted.
    ''' </summary>
    Protected Overrides Sub OnDragLeave(e As EventArgs)
        MyBase.OnDragLeave(e)
        HighlightedNode = Nothing
    End Sub

    ''' <summary>
    ''' Handles an item being dragged over this treelistview
    ''' </summary>
    Protected Overrides Sub OnDragOver(ByVal e As DragEventArgs)
        MyBase.OnDragOver(e)

        ' Assume it can't be dragged...
        e.Effect = DragDropEffects.None

        ' If the data is not valid, we can't handle it
        If Not IsValidDragData(e.Data) Then Return

        ' Get the target group member - if we don't have one, we can't handle it
        Dim item As TreeListViewItem = Nothing
        Dim mem As IGroupMember = GetTargetMember(e, item)
        e.Effect = If(mem IsNot Nothing, DragDropEffects.Move, DragDropEffects.None)
        HighlightedNode = item

    End Sub

    ''' <summary>
    ''' Handles a collection of group members being dropped into this treelistview.
    ''' </summary>
    Protected Overrides Sub OnDragDrop(ByVal e As DragEventArgs)
        MyBase.OnDragDrop(e)

        ' Get the target group member; abort now if we can't ascertain one.
        Dim mem As IGroupMember = GetTargetMember(e)
        If mem Is Nothing Then Return

        ' See if what was dropped was a collection of group members
        Dim draggedMembers As ICollection(Of IGroupMember) =
            e.Data.GetData(Of ICollection(Of IGroupMember))()

        ' If it was, fire the appropriate event
        If draggedMembers IsNot Nothing Then
            Dim dropArgs As New GroupMemberDropEventArgs(mem, draggedMembers)
            OnGroupMemberDropped(dropArgs)
            UpdateView()
            SelectMembers(draggedMembers)
            Focus()
        End If

        HighlightedNode = Nothing

    End Sub

#End Region

#Region " Event Invokers "

    ''' <summary>
    ''' Raises the <see cref="TreeLoaded"/> event.
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    ''' <remarks>Note that this event may not be fired on the UI thread - if that is
    ''' required, the handler should <see cref="Invoke"/> a call back to the UI
    ''' thread.</remarks>
    Protected Overridable Sub OnTreeLoaded(e As GroupTreeEventArgs)
        RaiseEvent TreeLoaded(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="GroupMemberDropped"/> event
    ''' </summary>
    Protected Overridable Sub OnGroupMemberDropped(e As GroupMemberDropEventArgs)
        RaiseEvent GroupMemberDropped(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="GroupTreeListViewItemAdded"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnGroupTreeListViewItemAdded(
     e As GroupTreeListViewItemEventArgs)
        RaiseEvent GroupTreeListViewItemAdded(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="GroupTreeListViewItemUpdated"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnGroupTreeListViewItemUpdated(
     e As GroupTreeListViewItemEventArgs)
        RaiseEvent GroupTreeListViewItemUpdated(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="GroupItemActivated"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnGroupItemActivated(e As GroupMemberEventArgs)
        RaiseEvent GroupItemActivated(Me, e)
    End Sub

    ''' <summary>
    ''' Raises a <see cref="GroupMembersBeforeDrag"/> event
    ''' </summary>
    Protected Overridable Sub OnGroupMembersBeforeDrag(
     e As GroupMultipleMemberEventArgs)
        RaiseEvent GroupMembersBeforeDrag(Me, e)
    End Sub

    Private Sub HandleNodeExpanded(sender As Object, e As TreeListViewEventArgs) _
     Handles Me.AfterExpand
        SetExpanded(e.Item, True)
    End Sub
    Private Sub HandleNodeCollapsed(sender As Object, e As TreeListViewEventArgs) _
     Handles Me.AfterCollapse
        SetExpanded(e.Item, False)
    End Sub
    Private Sub SetExpanded(n As TreeListViewItem, expand As Boolean)
        Dim groupMember = TryCast(n.Tag, IGroupMember)
        If groupMember Is Nothing Then Return

        Dim rawGroup = TryCast(mTree.RawTree.Root.FindById(groupMember.Id), IGroup)
        If rawGroup IsNot Nothing Then
            rawGroup.Expanded = expand
            Try
                If SaveExpandedGroups AndAlso rawGroup.IdAsGuid <> Guid.Empty AndAlso rawGroup.MemberType <> GroupMemberType.Pool Then
                    gSv.SaveTreeNodeExpandedState(rawGroup.IdAsGuid, expand, rawGroup.TreeType)
                End If
            Catch ex As Exception
                Dim log = LogManager.GetCurrentClassLogger()
                log.Error(ex)
            End Try
        End If

    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Selects a group member in this control
    ''' </summary>
    ''' <param name="mem">The group member to select; null to clear the selection.
    ''' </param>
    Public Sub SelectMember(mem As IGroupMember)
        SelectMembers(If(mem Is Nothing,
         GetEmpty.ICollection(Of IGroupMember)(), GetSingleton.ICollection(mem)))
    End Sub

    ''' <summary>
    ''' Selects a set of group members in this control
    ''' </summary>
    ''' <param name="selectedMembers">The group members to select</param>
    Public Sub SelectMembers(selectedMembers As IEnumerable(Of IGroupMember))
        SelectedItems.Clear()
        Dim lastNode As TreeListViewItem = Nothing
        For Each groupMember As IGroupMember In selectedMembers
            For Each node As TreeListViewItem In Items.FindByTag(groupMember, True)
                lastNode = node
                node.Selected = True
                node.EnsureVisible()
            Next
        Next
        If lastNode IsNot Nothing Then FocusedItem = lastNode
    End Sub

    ''' <summary>
    ''' Gets the selected members of the given type as a collection
    ''' </summary>
    ''' <typeparam name="T">The type of group member to get the selected members of.
    ''' </typeparam>
    ''' <returns>A collection of group members of the specified type that were
    ''' selected. If any selected items were not of the given type, they are not
    ''' included in the returned collection.</returns>
    Public Function GetSelectedMembers(Of T As {IGroupMember, Class})() _
     As ICollection(Of T)
        Dim s As New List(Of T)
        For Each m As TreeListViewItem In SelectedItems
            Dim mem = TryCast(m.Tag, T)
            If mem Is Nothing Then Continue For
            s.Add(mem)
        Next
        Return s
    End Function

    ''' <summary>
    ''' Checks if the data represented in the given data object is valid for dragging
    ''' and dropping in this control
    ''' </summary>
    ''' <param name="data">The data to check if it can be dropped in this control.
    ''' </param>
    ''' <returns>True if data within the given data object is valid for dropping into
    ''' this control.</returns>
    Private Function IsValidDragData(data As IDataObject) As Boolean
        Return data.GetDataPresent(GetType(ICollection(Of IGroupMember)))
    End Function

    ''' <summary>
    ''' Gets the target member of a drag/drop operation
    ''' </summary>
    ''' <param name="e">The drag event for which the target member is required.
    ''' </param>
    ''' <returns>The group member which is the target of the drag event, or null if
    ''' there was no target member or if the 'AllowDrop' properties in this control
    ''' disallow dropping in the current drag location.</returns>
    Protected Function GetTargetMember(e As DragEventArgs) As IGroupMember
        Return GetTargetMember(e, Nothing)
    End Function

    ''' <summary>
    ''' Gets the target member of a drag/drop operation
    ''' </summary>
    ''' <param name="e">The drag event for which the target member is required.
    ''' </param>
    ''' <param name="item">The item which corresponds to the target member returned.
    ''' </param>
    ''' <returns>The group member which is the target of the drag event, or null if
    ''' there was no target member or if the 'AllowDrop' properties in this control
    ''' disallow dropping in the current drag location.</returns>
    Protected Function GetTargetMember(
     e As DragEventArgs, ByRef item As TreeListViewItem) As IGroupMember
        item = GetItemAt(PointToClient(New Point(e.X, e.Y)))
        Dim m As IGroupMember
        If item Is Nothing Then
            If Not AllowDropInSpace Then Return Nothing
            ' If we're dropping into space, we're technically dropping into the
            ' root node, which is how we want to handle it
            m = mTree.Root
            ' This control never shows the root node, so we have no target member
            item = Nothing
        Else
            m = TryCast(item.Tag, IGroupMember)
        End If
        Dim gp As IGroup = TryCast(m, IGroup)
        If gp IsNot Nothing Then
            If AllowDropOnGroups Then Return gp Else item = Nothing : Return Nothing
        Else
            If AllowDropOnMembers Then Return m Else item = Nothing : Return Nothing
        End If

    End Function

    ''' <summary>
    ''' Method called when the underlying control is created, and a handle has been
    ''' assigned to it.
    ''' This loads the data from the group store for the tree type and adds it to
    ''' this control.
    ''' </summary>
    Protected Overrides Sub OnCreateControl()
        MyBase.OnCreateControl()
        If Not DesignMode Then UpdateView(False)
    End Sub

    ''' <summary>
    ''' Methods called when a double click is detected on this control.
    ''' If the double click is on an item (ie. not on a group), then a
    ''' <see cref="GroupItemActivated"/> event is raised.
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overrides Sub OnDoubleClick(e As EventArgs)
        MyBase.OnDoubleClick(e)
        Dim mem = FirstSelectedItem
        If mem IsNot Nothing Then OnGroupItemActivated(New GroupMemberEventArgs(mem))
    End Sub

    ''' <summary>
    ''' Refreshes this control
    ''' </summary>
    ''' <remarks>A bit of a placeholder this; I want to be able to fully refresh the
    ''' data too, without changing the current state of any nodes in the control,
    ''' but that's quite hard and I'm not sure how to do it yet.</remarks>
    Public Overrides Sub Refresh()
        MyBase.Refresh()
    End Sub

    ''' <summary>
    ''' Drops and creates the columns defined in this view.
    ''' There will always be a 'Name' column, other than that the columns are
    ''' determined by the subitems set in the control
    ''' </summary>
    Protected Sub UpdateColumns()
        ' Clear all but the first column
        For i As Integer = Columns.Count - 1 To 1 Step -1
            Columns.RemoveAt(i)
        Next

        ' If we don't have a first column, create it to hold the name
        If Columns.Count = 0 Then
            With Columns.Add("Name", My.Resources.GroupTreeListView_Name, 500)
                .ImageIndex = -1
                .Width = -2
                .TextAlign = HorizontalAlignment.Left
            End With
        End If

        ' And add the necessary subitems
        For Each entry In SubitemDefinitions
            With Columns.Add(entry.Key, 500, HorizontalAlignment.Left)
                .ImageIndex = -1
                .Tag = entry.Value
                .Width = -2
            End With
        Next

    End Sub

    ''' <summary>
    ''' Adds an entry to a collection representing a specified group member and, if
    ''' appropriate, its contents.
    ''' </summary>
    ''' <param name="m">The member for which a new tree list view item is needed.
    ''' </param>
    ''' <param name="items">The item collection to which the new tree list view item
    ''' should be added.</param>
    ''' <returns>The newly created tree list view item, after adding into the
    ''' specified collection.</returns>
    Protected Function AddEntry(
     m As IGroupMember, items As TreeListViewItemCollection) As TreeListViewItem
        Return AddEntry(m, items, GetEmpty.ICollection(Of IGroupMember), Nothing)
    End Function

    ''' <summary>
    ''' Adds an entry to a collection representing a specified group member and, if
    ''' appropriate, its contents.
    ''' </summary>
    ''' <param name="m">The member for which a new tree list view item is needed.
    ''' </param>
    ''' <param name="items">The item collection to which the new tree list view item
    ''' should be added.</param>
    ''' <param name="selected">The collection of group members which should be set
    ''' as selected when adding this entry (and its subtree)</param>
    ''' <returns>The newly created tree list view item, after adding into the
    ''' specified collection.</returns>
    Protected Function AddEntry(
     m As IGroupMember,
     items As TreeListViewItemCollection,
     selected As ICollection(Of IGroupMember),
     focused As IGroupMember) As TreeListViewItem

        ' Default the text colour
        Dim colour = Color.Black
        Dim resource As ResourceGroupMember = TryCast(m, ResourceGroupMember)
        If resource IsNot Nothing Then colour = Color.FromArgb(resource.InfoColour)

        ' Create a new node based on the name.
        Dim node As New TreeListViewItem(m.Name) With {
            .Tag = m,
            .ImageKey = m.ImageKey,
            .ForeColor = colour,
            .IsExpanded = If(TryCast(m, IGroup)?.Expanded, False)
        }
        If (node.Text.Equals("Default") And m.IsGroup) Then
            node.Text = CStr(IIf(TryCast(m, IGroup).IsDefault, My.Resources.GroupMemberComboBox_AddEntry_Default, node.Text))
        End If
        items.Add(node)
        If selected.Contains(m) Then node.Selected = True

        Dim g As IGroup = TryCast(m, IGroup)
        If g IsNot Nothing Then
            node.Font = New Font(Me.Font, FontStyle.Bold)
            For Each mem As IGroupMember In g
                Dim memNode = AddEntry(mem, node.Items, selected, focused)
                If selected.Contains(mem) Then memNode.Selected = True
                If Object.Equals(focused, mem) Then memNode.Focused = True
            Next
        Else
            For Each lookup In SubitemDefinitions
                If lookup.Key = My.Resources.GroupTreeListView_Name Then Continue For
                Try
                    Dim val As Object = lookup.Value.GetValue(m, Nothing)
                    node.SubItems.Add(New ListViewItem.ListViewSubItem() With {
                            .Text = If(val Is Nothing, "", val.ToString()),
                            .ForeColor = colour})
                Catch ex As Exception
                    node.SubItems.Add(String.Format(My.Resources.GroupTreeListView_Error0, ex.Message))
                End Try
            Next
        End If
        OnGroupTreeListViewItemAdded(New GroupTreeListViewItemEventArgs(node))
        Return node

    End Function

    ''' <summary>
    ''' Recursively updates the item collection representing the given group.
    ''' </summary>
    ''' <param name="group">The group which is represented by the given item collection.
    ''' </param>
    ''' <param name="items">The items which together form the UI representation of
    ''' the given group.</param>
    Private Sub UpdateNode(group As IGroup, items As TreeListViewItemCollection)
        Dim validNodes As New clsSet(Of TreeListViewItem)

        Dim sortedGroupMembers =
                group.
                Where(Function(x) If(TryCast(x, IGroup)?.IsDefault, False)).
                Concat(group.OfType(Of IGroupMember))

        For Each groupMember In sortedGroupMembers

            ' If a node doesn't yet exist for this member at current level in tree,
            ' create it; either way ensure the node has the most up to date information
            Dim added = False
            Dim node = items.FindByTag(groupMember, False).FirstOrDefault()
            If node Is Nothing Then
                InvokeIfRequired(Sub()
                                     node = New TreeListViewItem(groupMember.Name)
                                     SetupTreeListViewNode(groupMember, node)
                                     items.Add(node)
                                     added = True
                                 End Sub)
            End If

            ' Default the text colour
            Dim colour = Color.Black
            Dim resource As ResourceGroupMember = TryCast(groupMember, ResourceGroupMember)
            If resource IsNot Nothing Then colour = Color.FromArgb(resource.InfoColour)

            ' Local copy required to avoid behaviour differences between VB compiler versions
            Dim localGroupMember = groupMember
            InvokeIfRequired(
                Sub()
                    node.ImageKey = localGroupMember.ImageKey
                    node.Text = localGroupMember.Name
                    SetupTreeListViewNode(groupMember, node)
                    node.ForeColor = colour
                End Sub)
            validNodes.Add(node)

            ' If the node represents a group add its submembers
            InvokeIfRequired(
                Sub()
                    Dim subGroup As IGroup = TryCast(localGroupMember, IGroup)
                    If subGroup IsNot Nothing Then
                        UpdateNode(subGroup, node.Items)
                    Else
                        ' Otherwise add the subitems for the group member
                        While node.SubItems.Count > 1
                            node.SubItems.RemoveAt(node.SubItems.Count - 1)
                        End While
                        For Each lookup In SubitemDefinitions
                            If lookup.Key = My.Resources.GroupTreeListView_Name Then Continue For
                            Try
                                Dim val As Object = lookup.Value.GetValue(localGroupMember, Nothing)
                                node.SubItems.Add(New ListViewItem.ListViewSubItem() With {
                                .Text = If(val Is Nothing, "", val.ToString()),
                                .ForeColor = colour})
                            Catch ex As Exception
                                node.SubItems.Add(String.Format(My.Resources.GroupTreeListView_Error0, ex.Message))
                            End Try
                        Next
                    End If
                End Sub)
            Dim args As New GroupTreeListViewItemEventArgs(node)
            If added _
             Then OnGroupTreeListViewItemAdded(args) _
             Else OnGroupTreeListViewItemUpdated(args)
        Next

        Dim removalList = items.OfType(Of TreeListViewItem)() _
        .Where(Function(x) Not validNodes.Contains(x)) _
        .ToList()

        removalList.ForEach(Sub(y) InvokeIfRequired(Sub() items.Remove(y)))

        ' If there's nothing left in the items and this isn't the root node and
        ' we're *not* showing empty groups, remove the node representing the group
        ' (we can't really know whether the group is empty or not until the entire
        ' subtree has been processed)
        If Not ShowEmptyGroups AndAlso Not group.IsRoot AndAlso items.Count = 0 Then
            items.Parent.Remove()
        End If
    End Sub

    Private Sub SetupTreeListViewNode(groupMember As IGroupMember, node As TreeListViewItem)
        Dim group = TryCast(groupMember, IGroup)

        ' Pools don't contain the expanded attribute by default
        If (groupMember.IsPool) Then
            Dim groupMem = TryCast(mTree.RawTree.Root.FindById(groupMember.Id), IGroupMember)
            group = TryCast(groupMem, IGroup)
        End If

        node.Tag = groupMember
        node.IsExpanded = If(group?.Expanded, False)
        If (node.Text.Equals("Default") AndAlso groupMember.IsGroup) Then
            node.Text = CStr(IIf(group.IsDefault, My.Resources.GroupMemberComboBox_AddEntry_Default, node.Text))
        End If
    End Sub

    ''' <summary>
    ''' Handles the loading of the tree work, as well as the <see cref="TreeLoaded"/>
    ''' event work - both could take some time and are effectively background tasks
    ''' which operate on data alone.
    ''' </summary>
    Private Sub HandleLoaderDoWork() Handles mLoader.DoWork

        If mTreeType = GroupTreeType.Resources Then
            mReloadFromStore = True
        End If

        Dim store = GetGroupStore()
        mTree = store.GetTree(mTreeType,
                                        Filter,
                                        Nothing,
                                        mReloadFromStore,
                                        False,
                                        GetRetired)

        mReloadFromStore = False
        OnTreeLoaded(New GroupTreeEventArgs(mTree))
    End Sub

    ''' <summary>
    ''' Handles the tree loading completing; this updates the view to match the
    ''' newly loaded data.
    ''' </summary>
    Private Sub HandleLoaderCompleted() Handles mLoader.RunWorkerCompleted
        ' If a data-changing load request came in while the loader was working,
        ' kick it off again immediately; otherwise update the view with the data.
        If mReloadQueued Then
            mLoader.RunWorkerAsync()
            mReloadQueued = False
        Else
            SyncLock mLoader
                UpdateView(False, True)
            End SyncLock
        End If
    End Sub

    ''' <summary>
    ''' Refreshes the view from the current data held in this control
    ''' </summary>
    Public Sub UpdateView()
        UpdateView(False, False)
    End Sub

    ''' <summary>
    ''' Updates the view in this TreeListView to match the data held within it -
    ''' ie. it refreshes the columns and the tree
    ''' </summary>
    Friend Sub UpdateView(reloadFromStore As Boolean)
        UpdateView(reloadFromStore, False)
    End Sub

    ''' <summary>
    ''' Instruct the background worker thread to reload the tree from memory.  
    ''' </summary>
    Public Sub RefreshLocalStore()
        If mLoader.IsBusy Then
            mReloadQueued = True
            Return
        End If
        mReloadFromStore = False
        mLoader.RunWorkerAsync()
    End Sub

    ''' <summary>
    ''' Updates the view in this TreeListView to match the data held within it -
    ''' ie. it refreshes the columns and the tree
    ''' </summary>
    Private Sub UpdateView(reloadFromStore As Boolean, fromBackground As Boolean)
        ' Just get this out of the way now - this would cause an infinite loop
        If reloadFromStore AndAlso fromBackground Then Throw New ArgumentException(
            My.Resources.GroupTreeListView_CannotReloadFromStoreWhenCalledFromTheBackgroundLoader)

        If mTreeType = GroupTreeType.None Then
            Items.Clear()
            Return
        End If

        ' We want to go through the current treeview and compare it to what's in the
        ' tree we have set.
        ' If we have no tree to go through, load it in the background (unless this
        ' is being called from the post-background worker code, which suggests that
        ' there is just nothing to show)
        If mTree Is Nothing AndAlso fromBackground Then Return
        If mTree Is Nothing OrElse reloadFromStore Then
            ' If the background worker is already going, there's no need to go again
            If mLoader.IsBusy Then
                ' If this was an explicit 'load from store' request, queue up a
                ' reload to ensure that we have the right data (explicit requests
                ' typically come when the tree type has changed, entirely changing
                ' the data that this control should be displaying).
                If reloadFromStore Then mReloadQueued = True
                Return
            End If
            mReloadFromStore = reloadFromStore
            ' Get the latest tree in the background and come back to update the view
            mLoader.RunWorkerAsync()
            Return
        End If

        BeginUpdate()
        Comparer = New GroupTreeListViewComparer()
        If ColumnUpdateRequired Then UpdateColumns()

        If Not ShowFlat Then
            UpdateNode(mTree.Root, Items)

        Else
            ' Save the currently selected members
            Dim selected As New List(Of IGroupMember)
            For Each item As TreeListViewItem In SelectedItems
                Dim mem = TryCast(item.Tag, IGroupMember)
                If mem IsNot Nothing Then selected.Add(mem)
            Next
            ' and the focused one
            Dim focused As IGroupMember = Nothing
            Dim focNode As TreeListViewItem = FocusedItem
            If focNode IsNot Nothing Then focused = TryCast(focNode.Tag, IGroupMember)

            Items.Clear()
            For Each mem As IGroupMember In mTree.Root.FlattenedContents(Of clsSortedSet(Of IGroupMember))(False)
                If Filter(mem) Then
                    AddEntry(mem, Items, selected, focused)
                End If
            Next



        End If

        EndUpdate()

    End Sub

    ''' <summary>
    ''' Gets the group members at the given location
    ''' </summary>
    ''' <param name="p">The location at which the associated group member is required
    ''' </param>
    ''' <returns>The group member represented by the list view item at the given
    ''' location, or null if there was no such item there or it had no group member
    ''' associated with it.</returns>
    Public Function GetMemberAt(p As Point) As IGroupMember
        Dim item = GetItemAt(p)
        If item Is Nothing Then Return Nothing
        Return TryCast(item.Tag, IGroupMember)
    End Function

    ''' <summary>
    ''' Disposes of this control.
    ''' </summary>
    ''' <param name="disposing">True if being called from a Dispose() call; False if
    ''' being called from a destructor</param>
    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then

            ' Ensure the loader is stopped
            Dim loader = mLoader
            If loader IsNot Nothing Then loader.Dispose()

            ' Just disassociating with any data / classes which could remain in
            ' memory, ensuring that this control becomes eligible for collection
            mLoader = Nothing
            mTree = Nothing
            mFilter = Nothing
            mSubitemDefns = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub

#End Region

End Class

