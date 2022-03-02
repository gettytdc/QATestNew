Imports BluePrism.Images

Public Class clsReorderableTreeview
    Inherits AutomateControls.Trees.FilterableTreeView

    Public Event BeforeShowContextMenu As EventHandler

    Public Sub New()
        MyBase.New()

        'we want this to be true by default.
        Me.AllowDrop = True
    End Sub


#Region "Event Declarations"


    ''' <summary>
    ''' Event raised when a node is moved within the tree.
    ''' </summary>
    ''' <param name="e">EventArgs.</param>

    Public Event NodeMoved(ByVal e As TreeViewEditEventArgs)


    ''' <summary>
    ''' Event raised just before removing a node. Gives observers the opportunity
    ''' to cancel the operation.
    ''' </summary>
    ''' <param name="e">EventArgs.</param>

    Public Event BeforeNodeMoved(ByVal e As TreeViewCancelEditEventArgs)


    ''' <summary>
    ''' Event raised when a node is duplicated within the tree.
    ''' </summary>
    ''' <param name="e">EventArgs. The node referenced here is the new node (ie
    ''' the copy).</param>

    Public Event NodeCopied(ByVal e As TreeViewEditEventArgs)


    ''' <summary>
    ''' Event raised just before copying a node. Gives observers the opportunity
    ''' to cancel the operation.
    ''' </summary>
    ''' <param name="e">EventArgs.</param>

    Public Event BeforeNodeCopied(ByVal e As TreeViewCancelEditEventArgs)


    ''' <summary>
    ''' Event raised when a node is deleted.
    ''' </summary>
    ''' <param name="e">EventArgs.</param>

    Public Event NodeDeleted(ByVal e As TreeViewEditEventArgs)


    ''' <summary>
    ''' Event raised just before deleted a node. Gives observers the opportunity
    ''' to cancel the operation.
    ''' </summary>
    ''' <param name="e">EventArgs.</param>

    Public Event NodeDeleting(ByVal e As TreeViewCancelEditEventArgs)



    ''' <summary>
    ''' Class representing information about changes made to treeview.
    ''' </summary>

    Public Class TreeViewEditEventArgs
        Public Sub New(ByVal Node As TreeNode, ByVal SourceParentNode As TreeNode, ByVal SourceCollection As TreeNodeCollection, ByVal TargetCollection As TreeNodeCollection)
            Me.Node = Node
            Me.SourceParentNode = SourceParentNode
            Me.SourceCollection = SourceCollection
            Me.TargetCollection = TargetCollection
        End Sub


        ''' <summary>
        ''' The node to be moved/copied/deleted etc
        ''' </summary>
        Public Node As TreeNode

        ''' <summary>
        ''' The parent of the node, before the edit operation took place.
        ''' </summary>
        Public SourceParentNode As TreeNode

        ''' <summary>
        ''' The treenode collection in which the node resided
        ''' before the edit operation. May or may not be the same
        ''' as the TargetCollection (eg for a move this will probably
        ''' be different).
        ''' </summary>
        Public SourceCollection As TreeNodeCollection

        ''' <summary>
        ''' The treenode collection in which the node will reside
        ''' after the edit operation. May be null (eg in the event that
        ''' the node is being deleted).
        ''' </summary>
        ''' <remarks></remarks>
        Public TargetCollection As TreeNodeCollection

    End Class


    ''' <summary>
    ''' Class representing information about proposed changes to be made
    ''' to treeview.
    ''' </summary>

    Public Class TreeViewCancelEditEventArgs
        Inherits System.ComponentModel.CancelEventArgs

        Public Sub New(ByVal Node As TreeNode, ByVal TargetNodeCollection As TreeNodeCollection, ByVal IntendedIndex As Integer)
            Me.Node = Node
            Me.TargetNodeCollection = TargetNodeCollection
            Me.IntendedIndex = IntendedIndex
        End Sub


        ''' <summary>
        ''' The node to be moved/copied/deleted etc
        ''' </summary>

        Public Node As TreeNode

        ''' <summary>
        ''' The intended destination after the move/copy etc.
        ''' </summary>

        Public TargetNodeCollection As TreeNodeCollection

        ''' <summary>
        ''' The intended index of insertion during the move/copy etc.
        ''' </summary>

        Public IntendedIndex As Integer

        ''' <summary>
        ''' Used only after setting cancel to true - the reason for denying the change.
        ''' Shown to the user as a message prepended with the text
        ''' 
        ''' "The operation was disallowed for the following reason: "
        ''' </summary>

        Public DenialReason As String
    End Class

#End Region


#Region "Members And Properties"

    ''' <summary>
    ''' The message given to users if treeview edit operation is denied
    ''' </summary>
    Public mEditOperationDeniedMessage As String =
     My.Resources.TheOperationWasDisallowedForTheFollowingReason & vbCrLf & vbCrLf


    ' Flag indicating if reordering is allowed
    Private mAllowReorder As Boolean

    ''' <summary>
    ''' Determines whether reordering of nodes is allowed via a drag-drop 
    ''' operation. Reordering is allowed when this flag is set, and when
    ''' the tree is not currently filtered.
    ''' </summary>
    Public Property AllowReordering() As Boolean
        Get
            Return (mAllowReorder AndAlso Not MyBase.IsFiltered)
        End Get
        Set(ByVal value As Boolean)
            mAllowReorder = value
        End Set
    End Property



    ''' <summary>
    ''' Private member to store public property DropTargetColour()
    ''' </summary>
    Private mDropTargetColour As Color = Color.Olive

    ''' <summary>
    ''' The colour used to highlight any treenode which is the target of a
    ''' drop-operation.
    ''' 
    ''' Defaults to Olive.
    ''' </summary>
    Public Property DropTargetColour() As Color
        Get
            Return mDropTargetColour
        End Get
        Set(ByVal value As Color)
            mDropTargetColour = value
        End Set
    End Property


    ''' <summary>
    ''' The last node highlighted as a drop-target
    ''' </summary>
    Private mLastHighlightedNode As TreeNode


    ''' <summary>
    ''' The starting point of the last reversible line drawn, signifying the 
    ''' target of a drop operation (dividing two nodes).
    ''' </summary>
    Private mLastDropLineSource As Point

    ''' <summary>
    ''' The end point of the last reversible line drawn, signifying the 
    ''' target of a drop operation (dividing two nodes).
    ''' </summary>
    Private mLastDropLineDest As Point



    ''' <summary>
    ''' The node to be deleted, stored at the time the context menu is populated.
    ''' </summary>
    Private mNodeToDelete As TreeNode

    ''' <summary>
    ''' A delegate definition for a method taking no parameters.
    ''' </summary>
    Public Delegate Sub SimpleDelegate()


    ''' <summary>
    ''' Allows clients to override the implementation of the
    ''' collapse all nodes function, accessible to the user via 
    ''' the context menu.
    ''' </summary>
    Public CollapseAllImplementation As SimpleDelegate = AddressOf DoCollapseAll

    ''' <summary>
    ''' Allows clients to override the implementation of the
    ''' collapse all nodes function, accessible to the user via 
    ''' the context menu.
    ''' </summary>
    Public ExpandAllImplementation As SimpleDelegate = AddressOf DoExpandAll

#End Region


#Region "Methods"


    ''' <summary>
    ''' Determines if the first supplied node is a descendant of the second.
    ''' </summary>
    ''' <param name="Child">The first of the two nodes, possibly a descendant
    ''' of the second.</param>
    ''' <param name="Parent">The second of the two nodes, possibly an ancestor
    ''' of the first.</param>
    ''' <returns>Returns true if the first node is a descendant of the second.
    ''' </returns>
    Private Function IsDescendentNode(ByVal Child As TreeNode, ByVal Parent As TreeNode) As Boolean
        Dim PossibleParent As TreeNode = Child.Parent
        While Not PossibleParent Is Nothing
            If PossibleParent Is Parent Then Return True
            PossibleParent = PossibleParent.Parent
        End While

        Return False
    End Function

    ''' <summary>
    ''' Determines if the supplied collection is a child
    ''' of the supplied parent node.
    ''' </summary>
    ''' <param name="ChildCollection">The child collection to be
    ''' tested.</param>
    ''' <param name="ParentNode">The parent node.</param>
    ''' <returns>Returns true if the supplied collection
    ''' is a child collection of the parent node.</returns>
    ''' <remarks>The notion of being a child collection
    ''' is in the sense that
    ''' if the node tree is descended along all paths
    ''' from the parent, the child collection will be encountered
    ''' at some stage.</remarks>
    Private Function IsDescendentCollection(ByVal ChildCollection As TreeNodeCollection, ByVal ParentNode As TreeNode) As Boolean
        If ChildCollection.Count > 0 Then
            Return IsDescendentNode(ChildCollection(0), ParentNode)
        Else
            Return ChildCollection Is ParentNode.Nodes
        End If
    End Function


    ''' <summary>
    ''' Gets the treenode collection to which the supplied node belongs.
    ''' </summary>
    ''' <param name="Node">The node for which the treenodecollection is
    ''' required.</param>
    ''' <returns>As summary.</returns>
    Private Function GetNodeCollectionFromNode(ByVal Node As TreeNode) As TreeNodeCollection
        If Node.Parent Is Nothing Then
            Return Node.TreeView.Nodes
        Else
            Return Node.Parent.Nodes
        End If
    End Function


    ''' <summary>
    ''' Makes visible and selects the supplied node.
    ''' </summary>
    ''' <param name="NodeToShow">The node to be shown.</param>
    Private Sub ShowNode(ByVal NodeToShow As TreeNode)
        If Not NodeToShow Is Nothing Then
            NodeToShow.EnsureVisible()
            Me.SelectedNode = NodeToShow
        End If
    End Sub



    ''' <summary>
    ''' Threshold indicating how far away from the current mouse position
    ''' we should look to see if we are near to another node.
    ''' </summary>
    Const mcBetweenNodesThreshHold As Integer = 2



    ''' <summary>
    ''' Determines if the mouse is between two treenodes.
    ''' </summary>
    ''' <param name="MouseX">The X coordinate of the mouse location,
    ''' in screen coords.</param>
    ''' <param name="MouseY">The Y coordinate of the mouse location,
    ''' in screen coords.</param>
    ''' <returns>Returns true if the mouse is between treenodes.</returns>
    ''' <remarks>Whenever you modify either this function or
    ''' GetFirstNodeInDivision() you should check that the other still works</remarks>
    Private Function IsMouseBetweenNodes(ByVal MouseX As Integer, ByVal MouseY As Integer) As Boolean
        Dim NativePoint As Point = MyBase.PointToClient(New Point(MouseX, MouseY))
        Dim TargetNode As TreeNode = MyBase.GetNodeAt(NativePoint)

        'Find out the node just above and just below mouse point
        Dim NodeBelow As TreeNode = MyBase.GetNodeAt(NativePoint.X, NativePoint.Y + mcBetweenNodesThreshHold)
        Dim NodeAbove As TreeNode = MyBase.GetNodeAt(NativePoint.X, NativePoint.Y - mcBetweenNodesThreshHold)

        'find out if we are on the border - between nodes
        Dim InBetweenNodes As Boolean
        Dim CloseToNodeAbove As Boolean = (TargetNode IsNot NodeAbove)
        Dim CloseToNodeBelow As Boolean = (TargetNode IsNot NodeBelow)
        InBetweenNodes = CloseToNodeAbove Or CloseToNodeBelow

        Return InBetweenNodes
    End Function

    ''' <summary>
    ''' Assumes the supplied point is a valid node division point as defined
    ''' by IsMouseBetweenNodes.
    ''' 
    ''' If so then supplies details of the index at which the insertion
    ''' should take place,  and the target collectio.
    ''' </summary>
    ''' <param name="NativePoint">The point relative to this control that
    ''' determines where the mouse is. This should be between two nodes.
    ''' </param>
    ''' <param name="TargetCollection">The collection into which the
    ''' insertion should take place. Populated by reference.</param>
    ''' <param name="InsertionIndex">The index at which the insertion
    ''' should take place. Populated by reference.</param>
    ''' <remarks>Whenever you modify either this function or
    ''' IsMouseBetweenNodes you should check that the other still works</remarks>
    Private Sub GetInsertionDetailsOfDivision(ByVal NativePoint As Point, ByRef TargetCollection As TreeNodeCollection, ByRef InsertionIndex As Integer)
        Dim NodeAbove As TreeNode = MyBase.GetNodeAt(NativePoint.X, NativePoint.Y - mcBetweenNodesThreshHold)
        Dim NodeBelow As TreeNode = MyBase.GetNodeAt(NativePoint.X, NativePoint.Y + mcBetweenNodesThreshHold)

        'By default we just return the index after the node above,
        'but first some special cases ...

        Select Case True
            Case (NodeAbove Is Nothing)
                'If NodeAbove is nothing then must be at top of treeview
                TargetCollection = MyBase.Nodes
                InsertionIndex = 0

            Case NodeAbove.Nodes.Contains(NodeBelow)
                'If nodeabove is parent of actual node then need to insert at zero
                TargetCollection = NodeAbove.Nodes
                InsertionIndex = 0

            Case Else
                'Default return values
                TargetCollection = NodeAbove.Parent.Nodes
                If TargetCollection Is Nothing Then TargetCollection = MyBase.Nodes
                InsertionIndex = 1 + NodeAbove.Index
        End Select
    End Sub


    ''' <summary>
    ''' Builds and returns a new context menu.
    ''' </summary>
    ''' <returns>See summary.</returns>
    Private Function GetContextMenu() As ContextMenuStrip
        Dim cm As New ContextMenuStrip
        Dim item As ToolStripItem

        Dim NodeSelected As Boolean = (Me.SelectedNode IsNot Nothing)
        Dim CopiedDataExists As Boolean = (mCopiedNode IsNot Nothing)

        If Me.AllowReordering Then
            'Add delete item
            item = cm.Items.Add(My.Resources.cmItemDelete, ToolImages.Delete_Red_16x16, AddressOf OnDeleteClicked)
            item.Enabled = NodeSelected

            'Add cut item
            item = cm.Items.Add(My.Resources.cmItemCut, ToolImages.Cut_16x16, AddressOf OnCutClicked)
            item.Enabled = NodeSelected

            'Add copy item
            item = cm.Items.Add(My.Resources.cmItemCopy, ToolImages.Copy_16x16, AddressOf OnCopyClicked)
            item.Enabled = NodeSelected

            'Add paste item
            item = cm.Items.Add(My.Resources.cmItemPaste, ToolImages.Paste_16x16, AddressOf OnPasteClicked)
            item.Enabled = NodeSelected AndAlso CopiedDataExists

            cm.Items.Add(New ToolStripSeparator())
        End If

        'Expand All and Collapse All
        item = cm.Items.Add(My.Resources.cmItemExpandAll, Nothing, AddressOf HandleExpandAllClicked)
        item.Enabled = (Me.ExpandAllImplementation IsNot Nothing)
        item = cm.Items.Add(My.Resources.cmItemCollapseAll, Nothing, AddressOf HandleCollapseAllClicked)
        item.Enabled = (Me.CollapseAllImplementation IsNot Nothing)

        'Expand All and Collapse All Siblings
        item = cm.Items.Add(My.Resources.cmItemExpandAllSiblings, Nothing, AddressOf HandleExpandAllSiblingsClicked)
        item = cm.Items.Add(My.Resources.cmItemCollapSeAllSiblings, Nothing, AddressOf HandleCollapseAllSiblingsClicked)

        'Expand All children and Collapse All Children
        Dim HasChildren As Boolean = Me.SelectedNode IsNot Nothing AndAlso Me.SelectedNode.Nodes.Count > 0
        item = cm.Items.Add(My.Resources.cmItemExpandAllDescendants, Nothing, AddressOf HandleExpandAllChildrenClicked)
        item.Enabled = HasChildren
        item = cm.Items.Add(My.Resources.cmItemCollapseAllDescendants, Nothing, AddressOf HandleCollapseAllChildrenClicked)
        item.Enabled = HasChildren

        Return cm
    End Function

    Public Sub DeleteNode(ByVal n As TreeNode)
        If n Is Nothing Then Return

        Dim parentNodes As TreeNodeCollection = _
         GetNodeCollectionFromNode(n)

        'check with clients
        Dim e As TreeViewCancelEditEventArgs = _
         New TreeViewCancelEditEventArgs(n, parentNodes, -1)
        OnNodeDeleting(e)

        'cancel if requested by client
        If e.Cancel Then Return

        'otherwise go ahead and delete
        Dim parent As TreeNode = n.Parent
        n.Remove()
        OnNodeDeleted(New TreeViewEditEventArgs(n, parent, parentNodes, Nothing))

    End Sub

    Protected Overridable Sub OnNodeDeleting(ByVal e As TreeViewCancelEditEventArgs)
        RaiseEvent NodeDeleting(e)
    End Sub

    Protected Overridable Sub OnNodeDeleted(ByVal e As TreeViewEditEventArgs)
        RaiseEvent NodeDeleted(e)
    End Sub


#Region "Node Highlighting, drawing of drop targets etc"


    ''' <summary>
    ''' Clears the highlighting on potential drop target nodes, and clears the node
    ''' dividers that indicate that a node will be inserted between two nodes.
    ''' </summary>
    Private Sub ClearDropTargetHighlighting()
        If Not Me.mLastHighlightedNode Is Nothing Then
            Me.mLastHighlightedNode.BackColor = Me.BackColor
            Me.mLastHighlightedNode = Nothing
        End If
    End Sub


    ''' <summary>
    ''' Clears the dividing line drawn between two nodes indicating drop target.
    ''' If no such dividor exists then no action is taken.
    ''' </summary>
    Private Sub ClearNodeDividor()
        If Not (Me.mLastDropLineSource.Equals(Point.Empty) OrElse (Me.mLastDropLineDest.Equals(Point.Empty))) Then
            ControlPaint.DrawReversibleLine(Me.mLastDropLineSource, Me.mLastDropLineDest, Me.BackColor)
            Me.mLastDropLineSource = Point.Empty
            Me.mLastDropLineDest = Point.Empty
        End If
    End Sub


    ''' <summary>
    ''' Draws a node divider at the specified Y coordinate, relative to this
    ''' treeview.
    ''' 
    ''' Uses the supplied node to judge width and location of divider.
    ''' 
    ''' Clears any highlighted drop target, since this is the new 
    ''' drop target.
    ''' </summary>
    ''' <param name="NearestNode">The node to use to determine the location
    ''' and width of the divider. The divider will share the same
    ''' X location, and will share the width of this node.
    ''' 
    ''' If left null, the root node of the tree will be used instead.</param>
    ''' <param name="YCoord">The y coordinate at which to draw the
    ''' divider, relative to this control.</param>
    Private Sub DrawNodeDivider(ByVal NearestNode As TreeNode, ByVal YCoord As Integer)
        'Do regardless since our dividor is the new drop target
        Me.ClearDropTargetHighlighting()

        'plan coordinates of new line
        If NearestNode Is Nothing Then NearestNode = Me.Nodes(0)
        Dim ProposedLineStart As Point = MyBase.PointToScreen(New Point(NearestNode.Bounds.Left, YCoord))
        Dim ProposedLineEnd As Point = MyBase.PointToScreen(New Point(NearestNode.Bounds.Right, YCoord))

        'If not same as last time then clear old one and draw new line
        If Not ((ProposedLineStart.Equals(Me.mLastDropLineSource)) AndAlso (ProposedLineEnd.Equals(Me.mLastDropLineDest))) Then
            Me.ClearNodeDividor()
            ControlPaint.DrawReversibleLine(Me.mLastDropLineSource, Me.mLastDropLineDest, Me.BackColor)
            Me.mLastDropLineSource = ProposedLineStart
            Me.mLastDropLineDest = ProposedLineEnd
        End If
    End Sub


    ''' <summary>
    ''' Sets the backcolour of the supplied node.
    ''' </summary>
    ''' <param name="n">The node whose backcolour is to be set.</param>
    ''' <param name="color">The colour to which the backcolour should be
    ''' set.</param>
    Private Sub SetNodeBackColor(ByVal n As TreeNode, ByVal color As Color)
        If Not n.BackColor.Equals(color) Then
            n.BackColor = color
        End If
    End Sub


    ''' <summary>
    ''' Highlights the specified node as a droptarget, using the class property
    ''' DropTargetColour()
    ''' </summary>
    ''' <param name="TargetNode">The node to highlight.</param>
    Private Sub HighlightNodeAsDropTarget(ByVal TargetNode As TreeNode)
        If Not Me.mLastHighlightedNode Is TargetNode Then
            Me.ClearDropTargetHighlighting()
            Me.SetNodeBackColor(TargetNode, Me.DropTargetColour)
            Me.mLastHighlightedNode = TargetNode
        End If
    End Sub

#End Region


#End Region


#Region "Drag Event Handlers"

    Private Sub clsReorderableTreeview_ItemDrag(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemDragEventArgs) Handles MyBase.ItemDrag
        'This is not blocked when AllowReordering is false,
        'because user should be allowed to drag out
        'of treeview to another area


        'select the dragged item so that the user can remember what they are dragging
        Me.SelectedNode = CType(e.Item, TreeNode)


        ' now do the drag - we specify a treenode type to handle subclasses
        ' of treenode existing in this treeview
        Me.DoDragDrop(New DataObject(GetType(TreeNode).FullName, e.Item), DragDropEffects.Move Or DragDropEffects.Copy)
    End Sub

    Private Sub clsReorderableTreeview_DragOver(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles MyBase.DragOver
        If Me.AllowReordering Then
            If (e.Data.GetDataPresent(GetType(TreeNode))) Then

                'If needs be then scroll the control upward/downward to make more nodes visible
                Dim NativePoint As Point = MyBase.PointToClient(New Point(e.X, e.Y))
                Const MovingThreshHold As Integer = 16
                Dim MovingBottom As Boolean = (MyBase.Height - NativePoint.Y) < MovingThreshHold
                Dim MovingTop As Boolean = NativePoint.Y < MovingThreshHold
                Select Case True
                    Case MovingBottom
                        MyBase.TopNode = MyBase.TopNode.NextVisibleNode
                    Case MovingTop
                        MyBase.TopNode = MyBase.TopNode.PrevVisibleNode
                End Select

                'Now get on with the dragging feedback
                Dim TargetNode As TreeNode = MyBase.GetNodeAt(NativePoint)
                Dim DraggedNode As TreeNode = CType(e.Data.GetData(GetType(TreeNode)), TreeNode)

                'The node collection into which we are to insert the dragged node
                Dim TargetCollection As TreeNodeCollection = Nothing

                'find out if we are on the border - between nodes
                Dim InBetweenNodes As Boolean = Me.IsMouseBetweenNodes(e.X, e.Y)

                Dim SuitableTargetFound As Boolean
                Dim DoCancel As Boolean
                Select Case True
                    Case InBetweenNodes

                        'Find out the nature of the insertion
                        Dim InsertionIndex As Integer
                        Me.GetInsertionDetailsOfDivision(NativePoint, TargetCollection, InsertionIndex)

                        'check we are not about to insert into a descendant
                        If Me.IsDescendentCollection(TargetCollection, DraggedNode) Then
                            DoCancel = True
                        End If

                        'draw reversible line between nodes
                        If Not DoCancel Then
                            DrawNodeDivider(TargetNode, NativePoint.Y)
                            SuitableTargetFound = True
                        End If

                    Case (Not TargetNode Is Nothing)
                        'disallow dropping onto self or dropping onto descendent nodes
                        DoCancel = (TargetNode Is DraggedNode) Or (IsDescendentNode(TargetNode, DraggedNode))

                        'highlight target node
                        If Not DoCancel Then
                            Me.HighlightNodeAsDropTarget(TargetNode)
                            SuitableTargetFound = True
                        End If
                End Select

                'give the relevant UI feedback
                Select Case True
                    Case SuitableTargetFound
                        If ModifierKeys = Keys.Shift Then
                            e.Effect = DragDropEffects.Copy
                        Else
                            e.Effect = DragDropEffects.Move
                        End If
                    Case DoCancel
                        e.Effect = DragDropEffects.None
                        Exit Sub
                End Select

            End If
        End If
    End Sub



    Private Sub clsReorderableTreeview_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles MyBase.DragDrop
        If Me.AllowReordering Then
            If (e.Data.GetDataPresent(GetType(TreeNode))) Then

                Dim NativePoint As Point = MyBase.PointToClient(New Point(e.X, e.Y))
                Dim TargetNode As TreeNode = MyBase.GetNodeAt(NativePoint)
                Dim DraggedNode As TreeNode = CType(e.Data.GetData(GetType(TreeNode)), TreeNode)

                'The node collection that the dragged node came from
                Dim DraggedNodeParentCollection As TreeNodeCollection
                If DraggedNode.Parent IsNot Nothing Then
                    DraggedNodeParentCollection = DraggedNode.Parent.Nodes
                Else
                    DraggedNodeParentCollection = MyBase.Nodes
                End If

                'The node collection into which we are to insert the dragged node
                Dim TargetCollection As TreeNodeCollection = Nothing

                'find out if we are on the border - between nodes
                Dim InBetweenNodes As Boolean = Me.IsMouseBetweenNodes(e.X, e.Y)

                Dim ParentBeforeCopy As TreeNode = DraggedNode.Parent

                Dim CopyMode As Boolean = (ModifierKeys = Keys.Shift)

                Dim DoCancel As Boolean
                Dim NewNode As TreeNode = Nothing
                Select Case True
                    Case InBetweenNodes

                        'Find out the nature of the insertion
                        Dim InsertionIndex As Integer
                        Me.GetInsertionDetailsOfDivision(NativePoint, TargetCollection, InsertionIndex)
                        Dim InsertingIntoSameCollection As Boolean = TargetCollection Is DraggedNodeParentCollection

                        'cancel if the insertion would have no effect (ie dropping back into same place)
                        If Not CopyMode Then
                            If TargetCollection Is DraggedNodeParentCollection Then
                                If (InsertionIndex = DraggedNode.Index) OrElse (InsertionIndex = DraggedNode.Index + 1) Then
                                    GoTo DoExit
                                End If
                            End If
                        End If

                        'When moving a node  further down the list, the indexes
                        'will be shifted when we remove it. Hence a correction
                        'is needed to simulate that removal
                        If Not CopyMode Then
                            If (InsertionIndex >= DraggedNode.Index) Then
                                'simulates the removal of the dragged node from the same nodecollection - thus affecting the index of firstnode 
                                InsertionIndex -= 1
                            End If
                        End If

                        'check we are not about to insert into a descendant
                        If Me.IsDescendentCollection(TargetCollection, DraggedNode) Then
                            GoTo DoExit
                        End If

                        'check with clients
                        Dim EvArgs As New TreeViewCancelEditEventArgs(DraggedNode, TargetCollection, InsertionIndex)
                        If CopyMode Then
                            RaiseEvent BeforeNodeCopied(EvArgs)
                        Else
                            RaiseEvent BeforeNodeMoved(EvArgs)
                        End If

                        'cancel if requested by client
                        If EvArgs.Cancel = True Then
                            UserMessage.Show(mEditOperationDeniedMessage & EvArgs.DenialReason)
                            GoTo DoExit
                        End If

                        'do the insertion
                        Dim NodeToInsert As TreeNode = DraggedNode
                        If CopyMode Then NodeToInsert = CType(DraggedNode.Clone, TreeNode)
                        If (Not NodeToInsert.Parent Is Nothing) Then NodeToInsert.Remove()
                        TargetCollection.Insert(InsertionIndex, NodeToInsert)
                        NewNode = NodeToInsert

                    Case (Not TargetNode Is Nothing)
                        'disallow dropping onto self or dropping onto descendent nodes
                        DoCancel = (TargetNode Is DraggedNode) Or (IsDescendentNode(TargetNode, DraggedNode))

                        'check with clients
                        Dim EvArgs As New TreeViewCancelEditEventArgs(DraggedNode, TargetNode.Nodes, TargetNode.Nodes.Count + 1)
                        If CopyMode Then
                            RaiseEvent BeforeNodeCopied(EvArgs)
                        Else
                            RaiseEvent BeforeNodeMoved(EvArgs)
                        End If

                        'cancel if requested by client
                        If EvArgs.Cancel = True Then
                            UserMessage.Show(mEditOperationDeniedMessage & EvArgs.DenialReason)
                            GoTo DoExit
                        End If

                        If Not DoCancel Then
                            Dim NodeToAdd As TreeNode = DraggedNode
                            If ModifierKeys = Keys.Shift Then NodeToAdd = CType(DraggedNode.Clone, TreeNode)
                            If (Not NodeToAdd.Parent Is Nothing) Then NodeToAdd.Remove()
                            TargetNode.Nodes.Add(NodeToAdd)
                            NewNode = NodeToAdd
                        End If
                End Select

                If (Not DoCancel) AndAlso (Not NewNode Is Nothing) Then
                    ShowNode(NewNode)
                    Dim Args As New TreeViewEditEventArgs(NewNode, ParentBeforeCopy, DraggedNodeParentCollection, TargetCollection)
                    If CopyMode Then
                        RaiseEvent NodeCopied(Args)
                    Else
                        RaiseEvent NodeMoved(Args)
                    End If
                End If

DoExit:
                e.Effect = DragDropEffects.None
                Me.ClearDropTargetHighlighting()
                Me.ClearNodeDividor()
            End If
        End If
    End Sub

    Private Sub clsReorderableTreeview_GiveFeedback(ByVal sender As Object, ByVal e As System.Windows.Forms.GiveFeedbackEventArgs) Handles MyBase.GiveFeedback
        Select Case e.Effect
            Case DragDropEffects.None
                'we put this in so that if the user drags off the treeview, the highlighting disappears
                Me.ClearDropTargetHighlighting()
        End Select
    End Sub


#End Region


#Region "Mouse Events"

    Private Sub clsReorderableTreeview_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles MyBase.MouseUp
        If e.Button = System.Windows.Forms.MouseButtons.Right Then
            DisplayContextMenu(New Point(e.X, e.Y))
        End If
    End Sub

    Private Sub clsReorderableTreeview_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.F10 AndAlso e.Modifiers = Keys.Shift Then
            DisplayContextMenu(Point.Empty)
        End If
    End Sub

    Private Sub DisplayContextMenu(screenLocation As Point)
        Dim clickedNode As TreeNode = Me.SelectedNode

        If Not clickedNode Is Nothing Then
            Me.SelectedNode = clickedNode
            If Me.AllowReordering Then
                Me.mNodeToDelete = clickedNode
            End If

            If Me.ContextMenuStrip Is Nothing Then
                Me.ContextMenuStrip = GetContextMenu()
                RaiseEvent BeforeShowContextMenu(Me, EventArgs.Empty)
                Me.ContextMenuStrip.Show(Me, screenLocation)
                Me.ContextMenuStrip = Nothing
            End If
        Else
            Me.mNodeToDelete = Nothing
        End If
    End Sub


    ''' <summary>
    ''' Processes the deletion of a node via the context menu.
    ''' </summary>
    ''' <param name="sender">.</param>
    ''' <param name="e">.</param>
    Private Sub OnDeleteClicked(ByVal sender As Object, ByVal e As EventArgs)
        If AllowReordering Then DeleteNode(mNodeToDelete) : mNodeToDelete = Nothing
    End Sub

    Private Shared mCopiedNode As TreeNode

    Private Sub OnCutClicked(ByVal sender As Object, ByVal e As EventArgs)
        mCopiedNode = Nothing
        Me.OnCopyClicked(sender, e)

        If mCopiedNode IsNot Nothing Then
            Me.OnDeleteClicked(sender, e)
        End If
    End Sub

    Private Sub OnCopyClicked(ByVal sender As Object, ByVal e As EventArgs)
        If Not SelectedNode Is Nothing Then
            mCopiedNode = SelectedNode
        End If
    End Sub

    Private Sub OnPasteClicked(ByVal sender As Object, ByVal e As EventArgs)
        If mCopiedNode IsNot Nothing Then

            Dim PastedNode As TreeNode = mCopiedNode
            Dim TargetNode As TreeNode = Me.SelectedNode
            Dim ParentNodeCollection As TreeNodeCollection = Me.GetNodeCollectionFromNode(Me.mNodeToDelete)

            'check with clients
            Dim EvArgs As New TreeViewCancelEditEventArgs(PastedNode, ParentNodeCollection, TargetNode.Nodes.Count + 1)
            RaiseEvent BeforeNodeCopied(EvArgs)

            'cancel if requested by client
            If EvArgs.Cancel = True Then
                UserMessage.Show(mEditOperationDeniedMessage & EvArgs.DenialReason)
                Exit Sub
            End If

            'Otherwise, do the copy
            Dim NodeToAdd As TreeNode = CType(PastedNode.Clone, TreeNode)
            If (NodeToAdd.Parent IsNot Nothing) Then NodeToAdd.Remove()
            TargetNode.Nodes.Add(NodeToAdd)
            NodeToAdd.EnsureVisible()

            'Inform clients of change
            RaiseEvent NodeCopied(New TreeViewEditEventArgs(NodeToAdd, PastedNode.Parent, ParentNodeCollection, TargetNode.Nodes))
        End If
    End Sub


#End Region

#Region "Context Menu Handlers"

    Private Sub HandleExpandAllClicked(ByVal sender As Object, ByVal e As EventArgs)
        If Me.ExpandAllImplementation IsNot Nothing Then
            Me.BeginUpdate()
            Me.ExpandAllImplementation.Invoke()
            Me.EndUpdate()
        End If
    End Sub

    Private Sub DoExpandAll()
        MyBase.ExpandAll()
    End Sub

    Private Sub HandleCollapseAllClicked(ByVal sender As Object, ByVal e As EventArgs)
        If Me.CollapseAllImplementation IsNot Nothing Then
            Me.BeginUpdate()
            CollapseAllImplementation.Invoke()
            Me.EndUpdate()
        End If
    End Sub

    Private Sub DoCollapseAll()
        MyBase.CollapseAll()
    End Sub

    Private Sub HandleExpandAllSiblingsClicked(ByVal sender As Object, ByVal e As EventArgs)
        If Me.SelectedNode IsNot Nothing Then
            Me.BeginUpdate()

            Dim ParentCollection As TreeNodeCollection
            If SelectedNode.Level = 0 Then
                ParentCollection = SelectedNode.TreeView.Nodes
            Else
                ParentCollection = SelectedNode.Parent.Nodes
            End If

            For Each CurrentNode As TreeNode In ParentCollection
                CurrentNode.ExpandAll()
            Next

            Me.EndUpdate()
        End If
    End Sub

    Private Sub HandleCollapseAllSiblingsClicked(ByVal sender As Object, ByVal e As EventArgs)
        If Me.SelectedNode IsNot Nothing Then
            Me.BeginUpdate()

            Dim ParentCollection As TreeNodeCollection
            If SelectedNode.Level = 0 Then
                ParentCollection = SelectedNode.TreeView.Nodes
            Else
                ParentCollection = SelectedNode.Parent.Nodes
            End If

            For Each CurrentNode As TreeNode In ParentCollection
                CurrentNode.Collapse(False)
            Next

            Me.EndUpdate()
        End If
    End Sub

    Private Sub HandleExpandAllChildrenClicked(ByVal sender As Object, ByVal e As EventArgs)
        If Me.SelectedNode IsNot Nothing Then
            Me.BeginUpdate()
            Me.SelectedNode.ExpandAll()
            Me.EndUpdate()
        End If
    End Sub

    Private Sub HandleCollapseAllChildrenClicked(ByVal sender As Object, ByVal e As EventArgs)
        If Me.SelectedNode IsNot Nothing Then
            Me.BeginUpdate()
            Me.SelectedNode.Collapse(False)
            Me.EndUpdate()
        End If
    End Sub

#End Region

End Class
