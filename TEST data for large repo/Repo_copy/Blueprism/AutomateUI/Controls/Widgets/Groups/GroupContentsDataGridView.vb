Imports System.Windows.Forms.Design

Imports AutomateControls
Imports AutomateControls.DataGridViews
Imports BluePrism.AutomateAppCore

Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Images

''' <summary>
''' A specialised data grid view which presents the contents of a group and allows a
''' certain amount of manipulation of those contents.
''' </summary>
<Designer(GetType(ControlDesigner))>
Public Class GroupContentsDataGridView : Inherits RowBasedDataGridView

#Region " Published Events "

    ''' <summary>
    ''' Event fired when a group member is activated (typically double-clicked)
    ''' within this contents gridview.
    ''' </summary>
    Public Event GroupMemberActivated As GroupMemberEventHandler


    ''' <summary>
    ''' Event fired when a group member is previews within this contents gridview.
    ''' </summary>
    Public Event GroupMemberPreview As GroupMemberEventHandler

    ''' <summary>
    ''' Event fired when a group member is tested this contents gridview.
    ''' </summary>
    Public Event GroupMemberTest As GroupMemberEventHandler

    ''' <summary>
    ''' Event fired when a user requests that a group member in this gridview be
    ''' deleted.
    ''' </summary>
    Public Event GroupMemberDeleteRequested As GroupMemberEventHandler

    ''' <summary>
    ''' Event fired when a user requests that a group member in this gridview be
    ''' unlocked.
    ''' </summary>
    Public Event GroupMemberUnlockRequested As GroupMemberEventHandler

    ''' <summary>
    ''' Event fired when this control changes the contents of its group
    ''' </summary>
    Public Event GroupContentsChanged As GroupEventHandler

    ''' <summary>
    ''' Event fired when a user requests that a group members in this gridview be
    ''' compared.
    ''' </summary>
    Public Event GroupMemberCompareRequested As GroupMultipleMemberEventHandler

    Public Event GroupMemberContextMenuOpening As GroupMemberContexMenuOpeningEventHandler

#End Region

#Region " Member Variables "

    ' Flag indicating if there is a delayed mouse down event pending.
    ' Used to provide better handling of multiple row dragging
    Private mDelayedMouseDown As MouseEventArgs

    ' The group that this control is currently displaying
    Private mGroup As IGroup

    ' A rectangle around the mousedown event location outside of which a drag event
    ' is started
    Private dragBoxFromMouseDown As Rectangle

    ' The column which handles the header info for this control
    Private colInfo As DataGridViewItemHeaderColumn

    ' The context menu displayed for this data grid view
    Private WithEvents mContextMenu As ContextMenuStrip

    ' The menu item dealing with opening a group member
    Private WithEvents mOpenMenuItem As ToolStripMenuItem

    ' The menu item dealing with opening a group member
    Private WithEvents mViewMenuItem As ToolStripMenuItem

    ' The menu item dealing with comparing two group members
    Private WithEvents mCompareMenuItem As ToolStripMenuItem

    ' The menu item dealing with comparing a group member to something else
    Private WithEvents mCompareToMenuItem As ToolStripMenuItem

    ' The menu item dealing with finding references
    Private WithEvents mFindRefsMenuItem As ToolStripMenuItem

    ' The menu item dealing with deleting a group member
    Private WithEvents mDeleteMenuItem As ToolStripMenuItem

    ' The menu item dealing with removing a group member from its group
    Private WithEvents mRemoveMenuItem As ToolStripMenuItem

    ' The menu item dealing with unlocking a group member
    Private WithEvents mUnlockMenuItem As ToolStripMenuItem

    ' A Dictionary containing the grids rows keyed to IGroupMember.
    Private ReadOnly mRowsByMember As New Dictionary(Of IGroupMember, DataGridViewRow)

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new group contents grid view, with or without the name column
    ''' being instantiated and added
    ''' </summary>
    Public Sub New()
        ' We need this set to ensure that the item header cells size themselves
        ' correctly
        AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells

        colInfo = New DataGridViewItemHeaderColumn With {
            .Name = "colInfo",
            .HeaderText = "",
            .ImageList = ImageLists.Components_32x32,
            .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            .SortMode = DataGridViewColumnSortMode.Automatic
        }
        Columns.Add(colInfo)

        mOpenMenuItem = New ToolStripMenuItem() With {
            .Text = My.Resources.GroupContentsDataGridView_Edit,
            .Image = ToolImages.Document_Edit_16x16,
            .Name = "EditMenuItem"
        }
        mViewMenuItem = New ToolStripMenuItem() With {
            .Text = My.Resources.GroupContentsDataGridView_View,
            .Image = ToolImages.Preview_16x16,
            .Name = "ViewMenuItem"
        }
        mCompareMenuItem = New ToolStripMenuItem() With {
            .Text = My.Resources.GroupContentsDataGridView_Compare,
            .Image = ToolImages.Compare_16x16,
            .Name = "CompareMenuItem"
        }
        mCompareToMenuItem = New ToolStripMenuItem() With {
            .Text = My.Resources.GroupContentsDataGridView_CompareTo,
            .Image = ToolImages.Compare_16x16,
            .Name = "CompareToMenuItem"
        }
        mUnlockMenuItem = New ToolStripMenuItem() With {
            .Text = My.Resources.GroupContentsDataGridView_Unlock,
            .Image = ToolImages.Unlock_16x16
        }
        mDeleteMenuItem = New ToolStripMenuItem() With {
            .Text = My.Resources.GroupContentsDataGridView_Delete,
            .Image = ToolImages.Delete_Red_16x16
        }
        mRemoveMenuItem = New ToolStripMenuItem() With {
            .Text = My.Resources.GroupContentsDataGridView_RemoveFromGroup,
            .Image = ToolImages.Folder_Remove_16x16
        }
        mFindRefsMenuItem = New ToolStripMenuItem With {
            .Text = My.Resources.GroupContentsDataGridView_FindReferences,
            .Image = ToolImages.Find_Advanced_16x16
        }

        mContextMenu = New ContextMenuStrip()
        With mContextMenu.Items
            .Add(mViewMenuItem)
            .Add(mOpenMenuItem)
            .Add(mCompareMenuItem)
            .Add(mCompareToMenuItem)
            .Add(mFindRefsMenuItem)
            .Add(mUnlockMenuItem)
            .Add(New ToolStripSeparator())
            .Add(mRemoveMenuItem)
            .Add(mDeleteMenuItem)
        End With
        ContextMenuStrip = mContextMenu
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Overload of <see cref="DataGridView.AutoSizeRowsMode"/> to set the default
    ''' value as <see cref="DataGridViewAutoSizeRowMode.AllCells"/>, which is the
    ''' default value for this class.
    ''' </summary>
    <DefaultValue(DataGridViewAutoSizeRowsMode.AllCells)>
    Public Overloads Property AutoSizeRowsMode As DataGridViewAutoSizeRowsMode
        Get
            Return MyBase.AutoSizeRowsMode
        End Get
        Set(value As DataGridViewAutoSizeRowsMode)
            MyBase.AutoSizeRowsMode = value
        End Set
    End Property

    ''' <summary>
    ''' Override of ContextMenu property to hide it from the visual designer.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Overrides Property ContextMenu As ContextMenu
        Get
            Return MyBase.ContextMenu
        End Get
        Set(value As ContextMenu)
            MyBase.ContextMenu = value
        End Set
    End Property

    ''' <summary>
    ''' Override of ContextMenuStrip property to hide it from the visual designer.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Overrides Property ContextMenuStrip As ContextMenuStrip
        Get
            Return MyBase.ContextMenuStrip
        End Get
        Set(value As ContextMenuStrip)
            MyBase.ContextMenuStrip = value
        End Set
    End Property

    ''' <summary>
    ''' The columns in this gridview, shadowed here in order to hide it from the
    ''' forms designer
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Overloads ReadOnly Property Columns As DataGridViewColumnCollection
        Get
            Return MyBase.Columns
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the group displayed in this gridview.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property IGroup As IGroup
        Get
            Return mGroup
        End Get
        Set(value As IGroup)
            If mGroup Is value Then Return
            mGroup = value
            UpdateView()
        End Set
    End Property

    Public Property DefaultGroup As IGroup

    ''' <summary>
    ''' The currently selected group members in this listview
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property SelectedMembers As ICollection(Of IGroupMember)
        Get
            Dim members As New List(Of IGroupMember)
            For Each r As DataGridViewRow In SelectedRows
                Dim mem As IGroupMember = GetMemberFor(r)
                If mem IsNot Nothing Then members.Add(mem)
            Next
            Return members
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Updates a row with the data from a group member
    ''' </summary>
    ''' <param name="row">The row to update</param>
    ''' <param name="mem">The group member to update the row with</param>
    Protected Overridable Sub UpdateRow(row As DataGridViewRow, mem As IGroupMember)
        row.Cells(colInfo.Index).Value = mem
    End Sub

    ''' <summary>
    ''' Updates the view with the data in the assigned group.
    ''' </summary>
    Public Overridable Sub UpdateView()
        If mGroup Is Nothing OrElse mGroup.Count = 0 Then Rows.Clear() : Return

        Dim updatedRows As New HashSet(Of DataGridViewRow)

        ' Add each group member to the grid.
        For Each groupMember As IGroupMember In mGroup

            Dim gridRow As DataGridViewRow = Nothing
            If Not mRowsByMember.TryGetValue(groupMember, gridRow) Then
                gridRow = AddRow(groupMember)
            End If

            If gridRow IsNot Nothing Then
                UpdateRow(gridRow, groupMember)
                updatedRows.Add(gridRow)
            End If
        Next

        ' Remove any which are not updated.
        RemoveNonUpdatedRows(updatedRows)

        ' Re-sort after an update.
        colInfo.HeaderText = TreeDefinitionAttribute.GetLocalizedFriendlyName(mGroup.TreeType.GetTreeDefinition().PluralName)
        Dim column = If(SortedColumn, colInfo)

        ' Default to ascending if sort is not set...
        Dim order = If(SortOrder = SortOrder.Descending,
                       ListSortDirection.Descending, ListSortDirection.Ascending)
        Sort(column, order)

    End Sub

    ''' <summary>
    ''' Removes any rows that are not a part of the updated row collection.
    ''' </summary>
    ''' <param name="updatedRows">The current collection of updated rows. </param>
    Private Sub RemoveNonUpdatedRows(updatedRows As HashSet(Of DataGridViewRow))

        For Each groupMember In mRowsByMember.Keys.ToList()
            Dim gridRow = mRowsByMember.Item(groupMember)
            If Not updatedRows.Contains(gridRow) Then
                Rows.Remove(gridRow)
                mRowsByMember.Remove(groupMember)
            End If
        Next
    End Sub


    ''' <summary>
    ''' Handles the context menu opening, ensuring that any disabled menu items are
    ''' actually disabled as expected.
    ''' </summary>
    Protected Overridable Sub HandleContextMenuOpening(
     sender As Object, e As CancelEventArgs) Handles mContextMenu.Opening
        Dim count As Integer = SelectedRows.Count
        If count = 0 Then Return

        Dim prime As IGroupMember = GetMemberFor(CurrentRow)
        If prime Is Nothing Then Return

        Dim mems As ICollection(Of IGroupMember) = SelectedMembers

        Dim def = mGroup.TreeType.GetTreeDefinition()
        mRemoveMenuItem.Enabled = prime.CanBeRemovedFromGroup(User.Current)
        mDeleteMenuItem.Enabled = (mems.Count = 1 AndAlso
            mGroup.Permissions.HasPermission(User.Current, def.DeleteItemPermission))
        mCompareToMenuItem.Enabled = (mems.Count = 1)
        mCompareMenuItem.Enabled = (mems.Count = 2)
        mUnlockMenuItem.Enabled = mems.Any(Function(m) m.IsLocked)
        mFindRefsMenuItem.Enabled = (
            mems.Count = 1 AndAlso prime.Dependency IsNot Nothing)

        Dim eArgs = New GroupMemberContexMenuOpeningEventArgs(prime, ContextMenuStrip)
        RaiseEvent GroupMemberContextMenuOpening(Me, eArgs)
        e.Cancel = eArgs.Cancel

    End Sub

    ''' <summary>
    ''' Handles the 'Edit' context menu item being clicked.
    ''' </summary>
    Private Sub HandleOpen() Handles mOpenMenuItem.Click
        For Each mem In SelectedMembers
            OnGroupMemberActivated(New GroupMemberEventArgs(mem))
        Next
    End Sub

    ''' <summary>
    ''' Handles the 'View' context menu item being clicked.
    ''' </summary>
    Private Sub HandleView() Handles mViewMenuItem.Click
        For Each mem In SelectedMembers
            OnGroupMemberPreview(New GroupMemberEventArgs(mem))
        Next
    End Sub


    ''' <summary>
    ''' Handles the 'Delete' context menu item being clicked.
    ''' </summary>
    Private Sub HandleDelete() Handles mDeleteMenuItem.Click
        For Each mem In SelectedMembers
            OnGroupMemberDeleteRequested(New GroupMemberEventArgs(mem))
        Next
    End Sub

    ''' <summary>
    ''' Handles the 'Find References' context menu item being clicked
    ''' </summary>
    Private Sub HandleFindRefs() Handles mFindRefsMenuItem.Click
        Dim mem As IGroupMember = Nothing
        With SelectedMembers
            If .Count = 1 Then
                mem = .First()
            Else
                Dim currMem = GetMemberFor(CurrentRow)
                If .Contains(currMem) Then mem = currMem
            End If
        End With
        If mem Is Nothing Then Return

        ' Get the App form hosting this control - again, bail if not there
        Dim frm = UIUtil.GetAncestor(Of frmApplication)(Me)
        If frm Is Nothing Then Return

        ' Get the dependency for the group member. Last chance to leave the room
        frm.FindReferences(mem.Dependency)

    End Sub

    ''' <summary>
    ''' Handles the 'Remove from Group' context menu item being clicked.
    ''' </summary>
    Private Sub HandleRemove() Handles mRemoveMenuItem.Click
        Try
            If SelectedMembers Is Nothing Then Throw New NullReferenceException(My.Resources.GroupContentsDataGridView_NoGroupMembersHaveBeenSelectedToBeRemoved)

            Dim groupPermissionLogic = New GroupPermissionLogic()

            'Get the default group for this object (it might not be named default, but IsDefault will be true)
            Dim defaultGroupForSelectedItems = SelectedMembers.First().RootGroup.First(Function(x) CType(x, IGroup).IsDefault)
            If groupPermissionLogic.ValidateMoveMember(SelectedMembers.First(), mGroup, defaultGroupForSelectedItems, False,
                                                       AddressOf GroupTreeControl.ShowConfirmDialog, User.Current) Then
                SelectedMembers.ToList().ForEach(Sub(x) x.Remove())

                UpdateView()
                OnGroupContentsChanged(New GroupEventArgs(mGroup))
            End If
        Catch ex As Exception
            UserMessage.Err(ex, String.Format(My.Resources.GroupContentsDataGridView_UnableToRemoveSpecifiedProcessSFromGroup0, ex.Message))
        End Try
    End Sub

    ''' <summary>
    ''' Handles the 'Unlock Item' context menu item being clicked.
    ''' </summary>
    Private Sub HandleUnlock() Handles mUnlockMenuItem.Click
        For Each mem In SelectedMembers.OfType(Of ProcessBackedGroupMember)()
            If mem.IsLocked Then _
             OnGroupMemberUnlockRequested(New GroupMemberEventArgs(mem))
        Next
    End Sub

    ''' <summary>
    ''' Handles the 'Compare' context menu item being clicked.
    ''' </summary>
    Private Sub HandleCompare() Handles mCompareMenuItem.Click
        OnGroupMemberCompareRequested(New GroupMultipleMemberEventArgs(SelectedMembers))
    End Sub

    ''' <summary>
    '''  Handles the 'Compare To...' context menu item being clicked.
    ''' </summary>
    Private Sub HandleCompareTo() Handles mCompareToMenuItem.Click
        OnGroupMemberCompareRequested(New GroupMultipleMemberEventArgs(SelectedMembers))
    End Sub

    ''' <summary>
    ''' Adds a row representing a specified group member into the appropriate part of
    ''' this grid view depending on how sorting is being done, additionally add the current
    ''' rows dictionary is added to.
    ''' </summary>
    ''' <param name="groupMember">The group member for whom a new row is required.</param>
    ''' <returns>The new row created for the group member, or null if a row was not
    ''' created for some reason.</returns>
    Protected Overridable Function AddRow(groupMember As IGroupMember) As DataGridViewRow
        Dim row = Rows(Rows.Add(groupMember))
        row.Tag = groupMember
        mRowsByMember.Add(groupMember, row)
        Return row
    End Function

    ''' <summary>
    ''' Gets the group member for the given row
    ''' </summary>
    ''' <param name="row">The row for which a group member is required.</param>
    ''' <returns>The group member associated with the given row or null if no group
    ''' member is associated with it, or if it was null</returns>
    Private Function GetMemberFor(row As DataGridViewRow) As IGroupMember
        If row Is Nothing Then Return Nothing
        Return TryCast(row.Tag, IGroupMember)
    End Function

    ''' <summary>
    ''' Handles a cell double click, firing a <see cref="GroupMemberActivated"/>
    ''' event if appropriate.
    ''' </summary>
    Protected Overrides Sub OnCellDoubleClick(e As DataGridViewCellEventArgs)
        MyBase.OnCellDoubleClick(e)
        If e.RowIndex = -1 Then Return
        Dim mem As IGroupMember = GetMemberFor(Rows(e.RowIndex))
        If mem IsNot Nothing Then OnGroupMemberActivated(New GroupMemberEventArgs(mem, False))
    End Sub

    ''' <summary>
    ''' Raises the <see cref="GroupMemberActivated"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnGroupMemberActivated(e As GroupMemberEventArgs)
        RaiseEvent GroupMemberActivated(Me, e)
    End Sub


    ''' <summary>
    ''' Raises the <see cref="GroupMemberPreview"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnGroupMemberPreview(e As GroupMemberEventArgs)
        RaiseEvent GroupMemberPreview(Me, e)
    End Sub


    ''' <summary>
    ''' Raises the <see cref="GroupMemberDeleteRequested"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Private Sub OnGroupMemberDeleteRequested(e As GroupMemberEventArgs)
        RaiseEvent GroupMemberDeleteRequested(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="GroupMemberUnlockRequested"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Private Sub OnGroupMemberUnlockRequested(e As GroupMemberEventArgs)
        RaiseEvent GroupMemberUnlockRequested(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="GroupContentsChanged"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Private Sub OnGroupContentsChanged(e As GroupEventArgs)
        RaiseEvent GroupContentsChanged(Me, e)
    End Sub

    Private Sub OnGroupMemberCompareRequested(e As GroupMultipleMemberEventArgs)
        RaiseEvent GroupMemberCompareRequested(Me, e)
    End Sub

    ''' <summary>
    ''' Handles the mousemove event, ensuring that the drag event is started as
    ''' appropriate if it has strayed beyond the drag rectangle since the
    ''' corresponding mousedown event occurred.
    ''' </summary>
    Protected Overrides Sub OnMouseMove(ByVal e As MouseEventArgs)
        ' If the left mouse button isn't down, we're not dragging...
        If (e.Button And MouseButtons.Left) <> MouseButtons.Left Then Return

        ' Check if we have any members selected; if not, no dragging...
        Dim mems As ICollection(Of IGroupMember) = SelectedMembers
        If mems.Count = 0 Then Return

        ' If the mouse moves outside the rectangle, start the drag.
        If dragBoxFromMouseDown <> Rectangle.Empty AndAlso
         Not dragBoxFromMouseDown.Contains(e.X, e.Y) Then
            ' Proceed with the drag and drop, passing in the member
            DoDragDrop(Of ICollection(Of IGroupMember))(
                mems, DragDropEffects.Move Or DragDropEffects.Copy)
            ' And 'disappear' the delayed mousedown (if there is one)
            mDelayedMouseDown = Nothing
        End If

        MyBase.OnMouseMove(e)
    End Sub

    ''' <summary>
    ''' Handles the mouseup event, ensuring that any delayed mousedown event is
    ''' fired.
    ''' </summary>
    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        If mDelayedMouseDown IsNot Nothing Then MyBase.OnMouseDown(mDelayedMouseDown)
        MyBase.OnMouseUp(e)
    End Sub

    ''' <summary>
    ''' Handles the mousedown event, ensuring that the location is stored so that
    ''' the drag rectangle can be determined, and mousedown event is delayed if
    ''' necessary.
    ''' </summary>
    Protected Overrides Sub OnMouseDown(ByVal e As MouseEventArgs)
        ' Get the index of the item the mouse is below.
        Dim hitInfo = HitTest(e.X, e.Y)
        Dim rowInd As Integer = hitInfo.RowIndex
        Dim colInd As Integer = hitInfo.ColumnIndex

        If rowInd <> -1 AndAlso e.Button = MouseButtons.Right Then
            ' If CTL is held down, this does nothing in Explorer, so... do nothing
            If Control.ModifierKeys.HasFlag(Keys.Control) Then Return

            ' Update the current cell to match the one clicked on
            If colInd <> -1 Then CurrentCell = Rows(rowInd).Cells(colInd)

            ' If the row is already selected, leave it (and any others) selected
            ' and let the context menu be handled as such
            If Rows(rowInd).Selected Then Return

            ' Otherwise, ensure the current row and only the current row are selected
            ClearSelection()
            Rows(rowInd).Selected = True

            ' And leave the handling alone - we're not caring about drag stuff in
            ' right mouse click handling
            Return

        End If

        mDelayedMouseDown = Nothing
        If rowInd <> -1 Then
            ' Remember the point where the mouse down occurred.
            ' The DragSize indicates the size that the mouse can move
            ' before a drag event should be started.
            Dim dragSize As Size = SystemInformation.DragSize
            ' Create a rectangle using the DragSize, with the mouse position being
            ' at the center of the rectangle.
            dragBoxFromMouseDown = New Rectangle(
                New Point(e.X - (dragSize.Width \ 2), e.Y - (dragSize.Height \ 2)),
                dragSize)

            ' If there are multiple rows selected, and the current row is one of them
            ' we don't want to deselect all the other rows until we're sure that
            ' we're not dragging, so delay the mousedown event (which does exactly
            ' that) until the mouseup that tells us we're not dragging.
            Dim selRows = SelectedRows
            If selRows.Count > 1 AndAlso selRows.Contains(Rows(rowInd)) Then
                mDelayedMouseDown = e
                Return
            End If
        Else
            ' Reset the rectangle if the mouse is not over an item in the ListBox.
            dragBoxFromMouseDown = Rectangle.Empty

        End If
        MyBase.OnMouseDown(e)

    End Sub

    ''' <summary>
    ''' Handles the dragover event; currently it just disallows any dragging within
    ''' the control itself.
    ''' </summary>
    Protected Overrides Sub OnDragOver(ByVal e As DragEventArgs)
        MyBase.OnDragOver(e)
        ' The drag/drop isn't for us - it's for other controls
        e.Effect = DragDropEffects.None
        'If DirectCast(e.KeyState, DragKeyState).HasFlag(DragKeyState.CtrlKey) Then
        '   e.Effect = DragDropEffects.Copy
        'Else
        '   e.Effect = DragDropEffects.Move
        'End If
    End Sub

    ''' <summary>
    ''' Handles the dragdrop event; currently it does not handle dropping into this
    ''' control.
    ''' </summary>
    Protected Overrides Sub OnDragDrop(ByVal e As DragEventArgs)
        MyBase.OnDragDrop(e)

        '' The mouse locations are relative to the screen, so they must be
        '' converted to client coordinates.
        'Dim clientPoint As Point = PointToClient(New Point(e.X, e.Y))
        '' Get the row index of the item the mouse is below.
        'rowIndexOfItemUnderMouseToDrop = HitTest(clientPoint.X, clientPoint.Y).RowIndex
        '' If the drag operation was a move then remove and insert the row.
        'If e.Effect = DragDropEffects.Move Then
        '   Dim rowToMove As DataGridViewRow = TryCast(e.Data.GetData(GetType(DataGridViewRow)), DataGridViewRow)
        '   Rows.RemoveAt(rowIndexFromMouseDown)
        '   Rows.Insert(rowIndexOfItemUnderMouseToDrop, rowToMove)
        'End If
    End Sub

#End Region

End Class
