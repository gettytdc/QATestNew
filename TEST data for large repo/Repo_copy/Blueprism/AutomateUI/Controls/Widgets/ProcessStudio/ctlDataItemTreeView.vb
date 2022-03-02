Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports System.Linq
Imports BluePrism.AutomateAppCore
Imports LocaleTools
Imports BluePrism.Core.Utility

''' Project  : Automate
''' Class    : ctlDataItemTreeView
''' 
''' <summary>
''' A control to display process data items in a TreeView.
''' </summary>
Public Class ctlDataItemTreeView
    Inherits System.Windows.Forms.UserControl

    <Flags()> _
    Public Enum ToolTipStyle
        None = 0
        Name = 1
        Narrative = 2
        Sheet = 4
        StageType = 8
        DataType = 16
    End Enum

    ''' <summary>
    ''' Flags Enumeration of the view modes available within this control
    ''' </summary>
    <Flags()> _
    Public Enum ViewMode
        None = 0
        ByDataType = 1
        ByPage = 2
        AllVisible = 4
    End Enum

    ''' <summary>
    ''' Class providing a disconnected collection of treenodes, taken from a given
    ''' node collection. This allows TreeNode.Remove() to be called on nodes herein
    ''' without throwing an exception.
    ''' </summary>
    Private Class TreeNodeCollectionCopy : Inherits List(Of TreeNode)
        Public Sub New(ByVal coll As TreeNodeCollection)
            MyBase.New(New clsTypedEnumerable(Of TreeNode)(coll))
        End Sub
    End Class

    ''' <summary>
    ''' text color of private data item nodes
    ''' </summary>
    Private Shared ReadOnly PrivateItemColor As Color = Color.LightGray

    ''' <summary>
    ''' text color of data-type nodes
    ''' </summary>
    Private Shared ReadOnly DataTypeColor As Color = Color.Black

    ''' <summary>
    ''' text color of sheet nodes
    ''' </summary>
    Private Shared ReadOnly SheetNameColor As Color = Color.Black

    ''' <summary>
    ''' bits defining what details are diplayed in a tooltip.
    ''' </summary>
    Private Shared ReadOnly sToolTipStyle As ToolTipStyle = _
     ToolTipStyle.Name Or ToolTipStyle.Narrative Or ToolTipStyle.Sheet Or ToolTipStyle.DataType

    ''' <summary>
    ''' A bold font suitable for use in the treeview.
    ''' </summary>
    Private ReadOnly mBoldFont As Font

    ''' <summary>
    ''' A non-bold font suitable for use in the treeview.
    ''' </summary>
    Private ReadOnly mNonBoldFont As Font

    ''' <summary>
    ''' A guid used as a proxy for the Main Page (which uses has Guid.Empty)
    ''' </summary>
    Private mMainSheetId As Guid

    ''' <summary>
    ''' a tooltip displayed when a node is clicked
    ''' </summary>
    Private mToolTip As ToolTip

    ''' <summary>
    ''' A reference to the node showing a tooltip
    ''' </summary>
    Private mToolTipNode As TreeNode

    ''' <summary>
    ''' The four trees available for display - one for each view of the data items.
    ''' Ordered as follows: by alpha, by sheet, by datatype,by both datatype and by
    ''' sheet.
    ''' </summary>
    Private ReadOnly Property Trees() As ICollection(Of TreeView)
        Get
            Return New TreeView() { _
             trvStages, _
             trvStagesBySheet, _
             trvStagesByDataType, _
             trvStagesBySheetAndDataType _
            }
        End Get
    End Property

    ''' <summary>
    ''' The sheet ID of the stage used as the scope stage.
    ''' </summary>
    Private ReadOnly Property mCurrentSheetId() As Guid
        Get
            If mStage Is Nothing Then Return Guid.Empty
            Return mStage.GetSubSheetID()
        End Get
    End Property

    ''' <summary>
    ''' Determines whether the four trees have already been populated.
    ''' </summary>
    Private mTreesPopulated As Boolean

    ' Flag indicating whether this control has been initialised
    ' Used to determine how to handle checkbox change events
    Private mInitialised As Boolean = False

    ''' <summary>
    ''' The currently set ViewMode in this data explorer
    ''' </summary>
    Private ReadOnly Property Mode() As ViewMode
        Get
            Dim m As ViewMode = ViewMode.None
            If ViewByDataType Then m = m Or ViewMode.ByDataType
            If ViewByPage Then m = m Or ViewMode.ByPage
            If ViewAll Then m = m Or ViewMode.AllVisible
            Return m
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating if the user has items grouping by data type
    ''' </summary>
    Private ReadOnly Property ViewByDataType() As Boolean
        Get
            Return chkDataType.Checked
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating if the user has items grouping by subsheet
    ''' </summary>
    Private ReadOnly Property ViewByPage() As Boolean
        Get
            Return chkPage.Checked
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating if the user is currently viewing all items
    ''' </summary>
    Private ReadOnly Property ViewAll() As Boolean
        Get
            Return chkViewAllItems.Checked
        End Get
    End Property

    ''' <summary>
    ''' A process viewer used to launch stage properties.
    ''' </summary>
    ''' <remarks>May be null, but if null then no stage properties can be viewed.
    ''' </remarks>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Friend Property ProcessViewer() As ctlProcessViewer
        Get
            Return mProcessViewer
        End Get
        Set(ByVal value As ctlProcessViewer)
            Me.mProcessViewer = value
        End Set
    End Property
    Private mProcessViewer As ctlProcessViewer

    ''' <summary>
    ''' Set to true to have the treeview ignore the scope of data items. In this
    ''' case no items will be made greyed out.
    ''' </summary>
    Public Property IgnoreScope() As Boolean
        Get
            Return mbIgnoreScope
        End Get
        Set(ByVal value As Boolean)
            mbIgnoreScope = value
        End Set
    End Property
    Private mbIgnoreScope As Boolean

    ''' <summary>
    ''' Flag to indicate tree is to display only statistics data items. If set to 
    ''' true then only data items that are marked as statistics will be displayed.
    ''' </summary>
    Public Property StatisticsMode() As Boolean
        Get
            Return mbStatisticsMode
        End Get
        Set(ByVal value As Boolean)
            mbStatisticsMode = value
        End Set
    End Property
    Private mbStatisticsMode As Boolean

    ''' <summary>
    ''' The tree currently being shown. The other three are hidden in the background.
    ''' </summary>
    Public ReadOnly Property TreeView() As TreeView
        Get
            Return mCurrentTree
        End Get
    End Property
    Private WithEvents mCurrentTree As TreeView

    ''' <summary>
    ''' The stage used to populate this treeview, if any. Note that a process object 
    ''' may also be used to populate the treeview.
    ''' </summary>
    Public Property Stage() As clsProcessStage
        Get
            Return mStage
        End Get
        Set(ByVal value As clsProcessStage)
            mStage = value
        End Set
    End Property
    Private mStage As clsProcessStage

    '''' <summary>
    '''' The process that the data items in this treeview come from.
    '''' </summary>
    '''' <value>clsProcess</value>
    <[ReadOnly](True)> _
    Public ReadOnly Property Process() As clsProcess
        Get
            Return mProcess
        End Get
    End Property
    Private mProcess As clsProcess

    ''' <summary>
    ''' Gets or sets whether the tree view nodes have checkboxes
    ''' </summary>
    ''' <value>Boolean</value>
    Public Property CheckBoxes() As Boolean
        Get
            Return mCurrentTree.CheckBoxes
        End Get
        Set(ByVal value As Boolean)
            For Each tree As TreeView In Trees
                tree.CheckBoxes = value
            Next
        End Set
    End Property

    ''' <summary>
    ''' Constructor
    ''' </summary>
    Public Sub New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add user code after Initialize component ...
        mCurrentTree = trvStagesByDataType

        'create a tool tip object
        mToolTip = New ToolTip()
        mToolTip.AutoPopDelay = 3000
        mToolTip.InitialDelay = 500
        mToolTip.ReshowDelay = 500

        'create a guid to use as the Main Page guid
        mMainSheetId = New Guid()
        mBoldFont = New Font(mCurrentTree.Font, FontStyle.Bold)
        mNonBoldFont = New Font(mBoldFont, mBoldFont.Style And Not FontStyle.Bold)

        trvStages.DoubleClickEnabled = True
        trvStagesBySheet.DoubleClickEnabled = True
        trvStagesByDataType.DoubleClickEnabled = True
        trvStagesBySheetAndDataType.DoubleClickEnabled = True
    End Sub

 
    ''' <summary>
    ''' Populates the treeview with data items from the supplied process object.
    ''' </summary>
    ''' <param name="proc">The process to populate this control with</param>
    Public Sub Populate(ByVal proc As clsProcess)
        Populate(proc, Nothing)
    End Sub

    ''' <summary>
    ''' Populates the treeview with the data items in the process owning the supplied 
    ''' data item stage. Items that are not supposed to be visible from the 
    ''' supplied stage are shown in grey.
    ''' </summary>
    ''' <param name="stage"></param>
    Public Sub Populate(ByVal stage As clsProcessStage)
        Populate(stage.Process, stage)
    End Sub

    ''' <summary>
    ''' Populates the treeview with the data items from the given process, using
    ''' the specified stage as the scope stage - if no stage is given, no scope stage
    ''' is set and all data items are displayed.
    ''' </summary>
    ''' <param name="stg">The stage to use as the scope stage for this data item
    ''' tree view. Null indicates no scope stage.</param>
    ''' <param name="proc">The process from where the data items should be drawn.
    ''' </param>
    Private Sub Populate(ByVal proc As clsProcess, ByVal stg As clsProcessStage)
        ' If we've not yet retrieved the user's preferred view mode, do so now
        If Not mInitialised Then
            Dim mode As ViewMode = CType(gSv.GetPref(PreferenceNames.StudioTreeViewMode.ViewModePrefName, ViewMode.ByDataType), ViewMode)
            If (mode And ViewMode.ByDataType) <> 0 Then chkDataType.Checked = True
            If (mode And ViewMode.ByPage) <> 0 Then chkPage.Checked = True
            If (mode And ViewMode.AllVisible) <> 0 Then chkViewAllItems.Checked = True
            mInitialised = True
        End If
        mStage = stg
        mProcess = proc
        If Not mTreesPopulated Then PopulateTrees()
        DisplayTree()
    End Sub

    ''' <summary>
    ''' Repopulates the tree using the method used to originally populate the tree.
    ''' </summary>
    ''' <remarks>Must not be called if Populate has not yet been called.
    ''' 
    ''' The original method is used - be it by process or by stage (see the two 
    ''' overloads of the Populate method).</remarks>
    Public Sub Repopulate()
        mTreesPopulated = False
        Populate(mProcess, mStage)
    End Sub

    ''' <summary>
    ''' Repopulates the treeviews, optionally ensuring that a particular stage is 
    ''' visible afterwards.
    ''' </summary>
    ''' <param name="DisplayStage">A stage whose corresponding node is to be made 
    ''' visible after the repopulation.</param>
    Public Sub Repopulate(ByVal DisplayStage As clsDataStage)
        Me.Repopulate()
        Dim Node As TreeNode = Me.FindStageNode(DisplayStage)
        If Node IsNot Nothing Then
            Node.EnsureVisible()
            Me.mCurrentTree.SelectedNode = Node
        End If
    End Sub

    ''' <summary>
    ''' Finds the node in the current tree corresponding to the supplied stage, if 
    ''' one exists.
    ''' </summary>
    ''' <param name="Stage">The stage whose node is to be found.</param>
    ''' <returns>A treenode if a match is found, otherwise nothing.</returns>
    Private Function FindStageNode(ByVal Stage As clsDataStage) As TreeNode
        Return FindStageNode(Stage, Me.mCurrentTree.Nodes)
    End Function

    ''' <summary>
    ''' Finds the treenode corresponding to the supplied stage via a recursive search
    ''' through the supplied treenode collection.
    ''' </summary>
    ''' <param name="stg">The stage whose treenode is to be found.</param>
    ''' <param name="nodes">The node collection in which to search.</param>
    ''' <returns>A treenode if a match is found, otherwise nothing.</returns>
    Private Function FindStageNode(ByVal stg As clsDataStage, ByVal nodes As TreeNodeCollection) As TreeNode
        For Each n As TreeNode In nodes
            If TypeOf n.Tag Is clsDataStage Then
                If TypeOf n.Tag Is clsDataStage Then
                    Dim d As clsDataStage = CType(n.Tag, clsDataStage)
                    If d.GetStageID = stg.GetStageID Then
                        Return n
                    End If
                End If
            End If
            Dim retval As TreeNode = Me.FindStageNode(stg, n.Nodes)
            If retval IsNot Nothing Then Return retval
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Clears the treenodes from the treeview, in all of its possible views.
    ''' </summary>
    Public Sub ClearTreeNodes()
        'Rather than just clearing the nodes from the
        'treeviews using Nodes.Clear, we replace the entire
        'treeview. This is a crude workaround for bug 2583
        Dim NewTree As AutomateControls.Trees.FlickerFreeTreeView

        If trvStages.Nodes.Count > 0 Then
            NewTree = Me.CloneTree(trvStages)
            Me.Controls.Remove(Me.trvStages)
            trvStages = NewTree
            Me.Controls.Add(trvStages)
        End If

        If Me.trvStagesByDataType.Nodes.Count > 0 Then
            NewTree = Me.CloneTree(Me.trvStagesByDataType)
            Me.Controls.Remove(Me.trvStagesByDataType)
            Me.trvStagesByDataType = NewTree
            Me.Controls.Add(Me.trvStagesByDataType)
        End If

        If Me.trvStagesBySheet.Nodes.Count > 0 Then
            NewTree = Me.CloneTree(Me.trvStagesBySheet)
            Me.Controls.Remove(Me.trvStagesBySheet)
            Me.trvStagesBySheet = NewTree
            Me.Controls.Add(Me.trvStagesBySheet)
        End If

        If Me.trvStagesBySheetAndDataType.Nodes.Count > 0 Then
            NewTree = Me.CloneTree(Me.trvStagesBySheetAndDataType)
            Me.Controls.Remove(Me.trvStagesBySheetAndDataType)
            Me.trvStagesBySheetAndDataType = NewTree
            Me.Controls.Add(Me.trvStagesBySheetAndDataType)
        End If
    End Sub

    ''' <summary>
    ''' Clones a treeview (or rather the properties of a treeview that we care about)
    ''' </summary>
    ''' <param name="TreeToClone">The tree to be cloned.</param>
    ''' <returns>Returns a clone of the tree.</returns>
    Private Function CloneTree(ByVal TreeToClone As AutomateControls.Trees.FlickerFreeTreeView) As AutomateControls.Trees.FlickerFreeTreeView
        Dim NewTree As New AutomateControls.Trees.FlickerFreeTreeView
        NewTree.Name = TreeToClone.Name
        NewTree.Font = TreeToClone.Font
        NewTree.Indent = TreeToClone.Indent
        NewTree.ItemHeight = TreeToClone.ItemHeight
        NewTree.Location = TreeToClone.Location
        NewTree.Size = TreeToClone.Size
        NewTree.Anchor = TreeToClone.Anchor
        NewTree.TabIndex = TreeToClone.TabIndex

        Return NewTree
    End Function

    ''' <summary>
    ''' Selects and checks the first node found with the given name, starting from 
    ''' the given node.
    ''' </summary>
    ''' <param name="name">The node text</param>
    ''' <param name="start">The start node</param>
    ''' <returns>A flag to indicate a node was found</returns>
    Private Function SelectAndCheckNode(ByVal name As String, ByVal start As TreeNode, ByVal status As Boolean) As Boolean
        If SelectNode(name, start) Then
            If start.TreeView.CheckBoxes Then
                RemoveHandler start.TreeView.BeforeCheck, AddressOf TreeView_BeforeCheck
                RemoveHandler start.TreeView.AfterCheck, AddressOf TreeView_AfterCheck
                start.TreeView.SelectedNode.Checked = status
                AddHandler start.TreeView.BeforeCheck, AddressOf TreeView_BeforeCheck
                AddHandler start.TreeView.AfterCheck, AddressOf TreeView_AfterCheck
            End If
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Selects the first node found with the given name, starting from the given 
    ''' node.
    ''' </summary>
    ''' <param name="name">The node text</param>
    ''' <param name="start">The start node</param>
    ''' <returns>A flag to indicate a node was found</returns>
    Private Function SelectNode(ByVal name As String, ByVal start As TreeNode) As Boolean

        Dim child, parent As TreeNode
        Dim sibling As TreeNode = start
        Dim bFound As Boolean

        'work through the sibling nodes until found
        While (Not bFound) AndAlso (Not sibling Is Nothing)
            If sibling.Text = name Then
                parent = sibling.Parent
                'work back up expanding the ancestor nodes
                While Not parent Is Nothing
                    parent.Expand()
                    parent = parent.Parent
                End While
                'bypass the tool tip event handler
                start.TreeView.SelectedNode = sibling
                bFound = True
            Else
                'work through the children recursively
                For Each child In sibling.Nodes
                    bFound = SelectNode(name, child)
                    If bFound Then Exit For
                Next
                sibling = sibling.NextNode
            End If
        End While
        Return bFound

    End Function

    ''' <summary>
    ''' Populates four tree views displaying data items and collections in various
    ''' groupings.
    ''' </summary>
    Private Sub DisplayTree()

        'Determine which tree to show.
        If ViewByPage Then
            If ViewByDataType Then
                mCurrentTree = trvStagesBySheetAndDataType
            Else
                mCurrentTree = trvStagesBySheet
            End If
        ElseIf ViewByDataType Then
            mCurrentTree = trvStagesByDataType
        Else
            mCurrentTree = trvStages
        End If

        'Show the current tree, expand the first level of nodes
        'and expose the selected node.
        mCurrentTree.Visible = True
        mCurrentTree.BringToFront()

        If mCurrentTree.SelectedNode Is Nothing AndAlso mCurrentTree.Nodes.Count > 0 Then
            mCurrentTree.SelectedNode = mCurrentTree.Nodes(0)
        End If
        If Not mCurrentTree.SelectedNode Is Nothing Then
            mCurrentTree.SelectedNode.EnsureVisible()
        End If

    End Sub

    ''' <summary>
    ''' Populates four tree views displaying data items and collections in various
    ''' groupings.
    ''' </summary>
    Private Sub PopulateTrees()

        'make sure trees are all clear
        Me.ClearTreeNodes()

        'Apply properties of the default treeview to the others.
        For Each tree As TreeView In Trees
            tree.Top = trvStages.Top
            tree.Left = trvStages.Left
            tree.Width = trvStages.Width
            tree.Height = trvStages.Height
            tree.Anchor = trvStages.Anchor
            tree.Sorted = trvStages.Sorted
            tree.Indent = trvStages.Indent
            tree.ItemHeight = trvStages.ItemHeight
            tree.CheckBoxes = trvStages.CheckBoxes
            tree.Sorted = True
            tree.Visible = False
        Next

        Dim iDataType As DataType

        'Add data type nodes to the data type tree.
        For Each dt As clsDataTypeInfo In clsProcessDataTypes.GetAll()
            If dt.IsPublic Then
                Dim n As New TreeNode(clsProcessDataTypes.GetFriendlyName(dt.Value, True, True))
                n.Name = dt.FriendlyPlural
                n.Tag = clsProcessDataTypes.DataTypeId(dt.Name)
                n.ForeColor = DataTypeColor
                n.NodeFont = Me.mBoldFont
                trvStagesByDataType.Nodes.Add(n)
            End If
        Next

        'Add sheet nodes to both the 'sheet' and 'sheet and data type' tree.
        For Each objSub As clsProcessSubSheet In mProcess.SubSheets

            Dim nodSheet As New TreeNode(LTools.GetC(objSub.Name, "misc", "page"))
            nodSheet.Tag = objSub.ID
            nodSheet.ForeColor = SheetNameColor
            nodSheet.NodeFont = New Font(Me.Font, FontStyle.Bold)
            trvStagesBySheet.Nodes.Add(nodSheet)

            nodSheet = CType(nodSheet.Clone, TreeNode)
            trvStagesBySheetAndDataType.Nodes.Add(nodSheet)

            'Add data type nodes to each of the sheet nodes in the 'sheet and data type' tree.
            For Each dt As clsDataTypeInfo In clsProcessDataTypes.GetAll()
                If dt.IsPublic Then
                    Dim n As New TreeNode(clsProcessDataTypes.GetFriendlyName(dt.Value, True, True))
                    n.Name = dt.FriendlyPlural
                    n.Tag = clsProcessDataTypes.DataTypeId(dt.Name)
                    n.ForeColor = DataTypeColor
                    n.NodeFont = New Font(Me.Font, FontStyle.Bold)
                    nodSheet.Nodes.Add(n)
                End If
            Next
        Next


        Dim bExitFor As Boolean

        'Get all the data item and collection stages from the process.
        If Not mProcess Is Nothing Then
            For Each stg As clsDataStage In mProcess.GetStages( _
             StageTypes.Data Or StageTypes.Collection)
                If mbStatisticsMode AndAlso Not stg.Exposure = StageExposureType.Statistic Then
                    'When in statistic mode ignore any non-statistic stages.
                Else
                    Dim sheetId As Guid = stg.GetSubSheetID()
                    iDataType = stg.GetDataType

                    'Create a node for the data item.
                    Dim nodStage As New TreeNode(stg.GetName)
                    nodStage.Tag = stg
                    nodStage.NodeFont = Me.mNonBoldFont

                    'Add the node to the default tree.
                    If stg.StageType = StageTypes.Collection Then
                        'The stage is a collection so also include the fields as child nodes.
                        AddCollectionNodeWithFields(trvStages.Nodes, nodStage)
                    Else
                        'The stage must be a data item.
                        AddDataItemNode(trvStages.Nodes, nodStage)
                    End If

                    'Add a clone of the node to the 'sheet' tree. NB Re-attach the stage to the tag.
                    For Each nodSheet As TreeNode In trvStagesBySheet.Nodes
                        sheetId = CType(nodSheet.Tag, Guid)
                        nodStage = CType(nodStage.Clone, TreeNode)
                        nodStage.Tag = stg
                        If sheetId.Equals(stg.GetSubSheetID) Then
                            'The stage has the correct sheet id.
                            AddDataItemNode(nodSheet.Nodes, nodStage)
                            Exit For
                        End If
                    Next

                    'Add a clone of the node to the 'data type' tree. NB Re-attach the stage to the tag.
                    For Each n As TreeNode In trvStagesByDataType.Nodes
                        iDataType = CType(n.Tag, DataType)
                        nodStage = CType(nodStage.Clone, TreeNode)
                        nodStage.Tag = stg
                        If ((stg.StageType = StageTypes.Data) AndAlso (iDataType = stg.GetDataType)) _
                         OrElse ((stg.StageType = StageTypes.Collection) AndAlso (iDataType = DataType.collection)) Then
                            'The stage is either a data item with the correct data type
                            'or the stage a collection and the data type is 'collection'.
                            'NB This is because GetDataType returns 'unknown' for collections.
                            AddDataItemNode(n.Nodes, nodStage)
                            Exit For
                        End If
                    Next

                    'Add a clone of the node to the 'sheet and data type' tree. NB Re-attach the stage to the tag.
                    For Each nodSheet As TreeNode In trvStagesBySheetAndDataType.Nodes
                        sheetId = CType(nodSheet.Tag, Guid)
                        If sheetId.Equals(stg.GetSubSheetID) Then
                            For Each n As TreeNode In nodSheet.Nodes
                                iDataType = CType(n.Tag, DataType)
                                nodStage = CType(nodStage.Clone, TreeNode)
                                nodStage.Tag = stg

                                If (stg.StageType = StageTypes.Data AndAlso iDataType = stg.GetDataType()) _
                                 OrElse (stg.StageType = StageTypes.Collection AndAlso iDataType = DataType.collection) Then
                                    'The stage is either a data item with the correct data type
                                    'or the stage a collection and the data type is 'collection'.
                                    'NB This is because GetDataType returns 'nothing' for collections.
                                    AddDataItemNode(n.Nodes, nodStage)
                                    bExitFor = True
                                    Exit For
                                End If
                            Next
                            If bExitFor Then
                                bExitFor = False
                                Exit For
                            End If
                        End If
                    Next

                    If stg.StageType = StageTypes.Collection Then
                        Dim objCollection As clsCollectionStage = CType(stg, clsCollectionStage)
                        'Add collection fields as siblings to data items in the 'data type' tree.
                        For Each n As TreeNode In trvStagesByDataType.Nodes
                            AddNodesToDataTypeNode(n, objCollection)
                        Next

                        'Add collection fields as siblings to data items in the 'sheet and data type' tree.
                        For Each nodSheet As TreeNode In trvStagesBySheetAndDataType.Nodes
                            sheetId = CType(nodSheet.Tag, Guid)
                            If sheetId.Equals(stg.GetSubSheetID) Then
                                For Each n As TreeNode In nodSheet.Nodes
                                    AddNodesToDataTypeNode(n, objCollection)
                                Next
                            End If
                        Next

                    End If

                End If
            Next
        End If

        If mbStatisticsMode Then
            For Each n As TreeNode In New TreeNodeCollectionCopy(trvStagesByDataType.Nodes)
                If n.Nodes.Count = 0 Then n.Remove()
            Next
        End If

        ' Remove any empty sheets (except the current one)
        For Each n As TreeNode In New TreeNodeCollectionCopy(trvStagesBySheet.Nodes)
            If mCurrentSheetId.Equals(n.Tag) Then
                n.ExpandAll()
                n.EnsureVisible()
                trvStagesBySheet.SelectedNode = n
            ElseIf n.Nodes.Count = 0 Then
                n.Remove()
            End If
        Next

        For Each nPage As TreeNode In New TreeNodeCollectionCopy(trvStagesBySheetAndDataType.Nodes)
            ' We don't care if the current sheet has no data items - we show it anyway,
            ' unless we are in statistics mode.
            If mCurrentSheetId.Equals(nPage.Tag) Then
                nPage.ExpandAll()
                nPage.EnsureVisible()
                trvStagesBySheetAndDataType.SelectedNode = nPage
                ' In stats mode, the page may still be removed if it's empty. In non-stats
                ' mode, we leave the page there.
                If Not mbStatisticsMode Then Continue For
            End If

            ' If there are no data items underneath the page node, remove the page node.
            Dim leafCount As Integer = 0
            For Each n As TreeNode In New TreeNodeCollectionCopy(nPage.Nodes)
                leafCount += n.Nodes.Count
            Next
            If leafCount = 0 Then nPage.Remove()
        Next

    End Sub

    Public Sub RemoveDataItemTreeNode(item as clsDataStage)
        RemoveDataItemTreeNode_Stages(item.Name)
        RemoveDataItemTreeNode_BySheet(item.Name, item.SubSheet.Name)
        RemoveDataItemTreeNode_ByDataType(item.Name, item.DataType)
        RemoveDataItemTreeNode_BySheetAndDataType(item.Name, item.SubSheet.Name, item.DataType)
    End Sub

    Private Sub RemoveDataItemTreeNode_BySheetAndDataType(nodeText as string, sheet as string, dt as DataType)
        ' Page > DataType > Item
        dim pageNode as TreeNode = nothing
        dim typeNode as TreeNode = Nothing
        Dim nodeToDelete As TreeNode = Nothing
        Dim friendlyName = clsProcessDataTypes.GetFriendlyName(dt, bPlural:=True, bLocalize:=True)

        pageNode = trvStagesBySheetAndDataType.Nodes.
            Cast(Of TreeNode).
            FirstOrDefault(Function(x) x.Text = sheet)

        If pageNode IsNot Nothing Then
            typeNode = pageNode.Nodes.
                Cast(Of TreeNode).
                FirstOrDefault(Function(x) x.Text = friendlyName)

            If typeNode IsNot Nothing Then
                nodeToDelete = typeNode.Nodes.
                    Cast(Of TreeNode).
                    FirstOrDefault(Function(x) x.Text = nodeText)

                If nodeToDelete IsNot Nothing Then typeNode.Nodes.Remove(nodeToDelete)
            End If
        End If
    End Sub

    Private Sub RemoveDataItemTreeNode_BySheet(nodeText as string, sheet as string)
        Dim nodeToDelete As TreeNode = Nothing
        Dim parentNode As TreeNode = Nothing

        ' find the right page node as parent node
        ' Page > Item
        parentNode = trvStagesBySheet.Nodes.
            Cast(Of TreeNode).
            FirstOrDefault(Function(x) x.Text = sheet)

        If parentNode IsNot Nothing Then
            nodeToDelete = parentNode.Nodes.
                Cast(Of TreeNode).
                FirstOrDefault(Function(x) x.Text = nodeText)

            If nodeToDelete IsNot Nothing Then parentNode.Nodes.Remove(nodeToDelete)
        End If
    End Sub
    
    Private Sub RemoveDataItemTreeNode_Stages(nodeText as string)
        Dim nodeToDelete As TreeNode = Nothing

        nodeToDelete = trvStages.Nodes.
            Cast(Of TreeNode).
            FirstOrDefault(Function(x) x.Text = nodeText)

        If nodeToDelete IsNot Nothing Then
            mCurrentTree.Nodes.Remove(nodeToDelete)
        End If
    End Sub
    
    Private Sub RemoveDataItemTreeNode_ByDataType(nodeText as string, dt as DataType)
        Dim nodeToDelete As TreeNode = Nothing
        Dim parentNode As TreeNode = Nothing
        Dim friendlyName = clsProcessDataTypes.GetFriendlyName(dt, bPlural:=True, bLocalize:=True)

        ' find the correct parent node (datatype node)
        ' casts are necessary because TreeNodeCollection is not IEnumerable<T> to support linq
        ' Pages > Items
        parentNode = trvStagesByDataType.Nodes.
            Cast(Of TreeNode).
            FirstOrDefault(Function(x) x.Text = friendlyName)

        If parentNode IsNot Nothing Then
            nodeToDelete = parentNode.Nodes.
                Cast(Of TreeNode).
                FirstOrDefault(Function(x) x.Text = nodeText)

            If nodeToDelete IsNot Nothing Then parentNode.Nodes.Remove(nodeToDelete)
        End If
    End Sub

    ''' <summary>
    ''' Adds a node to a tree node collection and identifies ant private data items.
    ''' </summary>
    ''' <param name="nodes">The collection of nodes to add to</param>
    ''' <param name="node">The node to add</param>
    Private Sub AddDataItemNode(ByRef nodes As TreeNodeCollection, ByRef node As TreeNode)
        ' Check if the item is in scope
        Dim inScope As Boolean = IsInScope(DirectCast(node.Tag, clsDataStage))
        ' If not and the user isn't viewing all items, don't add it
        If Not inScope AndAlso Not ViewAll Then Return

        ' Otherwise, if it's not in scope, lowlight it
        If Not inScope Then
            'The stage object is private and not on the current sheet so grey it out
            node.ForeColor = PrivateItemColor
            node.NodeFont = Me.mNonBoldFont
        End If
        nodes.Add(node)
    End Sub

    ''' <summary>
    ''' Adds a node to a tree node to represent a collection and recursively adds
    ''' nodes beneath that representing all its descendent fields.
    ''' </summary>
    ''' <param name="nodes">The collection of nodes to add to</param>
    ''' <param name="node">The node to add</param>
    Private Sub AddCollectionNodeWithFields(ByRef nodes As TreeNodeCollection, ByRef node As TreeNode)

        Dim stg As clsCollectionStage = TryCast(node.Tag, clsCollectionStage)
        If stg IsNot Nothing Then

            Dim inScope As Boolean = IsInScope(stg)

            ' If it isn't in scope and we're not viewing all items, don't add it.
            If Not inScope AndAlso Not ViewAll Then Return

            AddNodesToCollectionNode(node, stg, Not inScope)
            ' Grey out stages which aren't in scope
            If Not inScope Then
                node.ForeColor = PrivateItemColor
                node.NodeFont = mNonBoldFont
            End If
            nodes.Add(node)
        End If
    End Sub

    ''' <summary>
    ''' Adds the node of the specified data type from the given collection to the
    ''' given tree node.
    ''' </summary>
    ''' <param name="parent">The parent node to add to</param>
    ''' <param name="stg">The collection stage with fields to add</param>
    Private Sub AddNodesToDataTypeNode(ByVal parent As TreeNode, ByVal stg As clsCollectionStage)
        AddNodesToDataTypeNode(parent, stg, Not IsInScope(stg))
    End Sub

    ''' <summary>
    ''' Adds the node of the specified data type from the given collection to the
    ''' given tree node.
    ''' </summary>
    ''' <param name="parent">The parent node to add to</param>
    ''' <param name="mgr">The collection with fields to add</param>
    Private Sub AddNodesToDataTypeNode(ByVal parent As TreeNode, _
     ByVal mgr As ICollectionDefinitionManager, ByVal disabled As Boolean)

        If mgr Is Nothing OrElse mgr.FieldCount = 0 Then Return
        If disabled AndAlso Not ViewAll Then Return

        Dim dt As DataType = DirectCast(parent.Tag, DataType)

        For Each fld As clsCollectionFieldInfo In mgr.FieldDefinitions
            If fld.DataType = dt Then
                Dim n As New TreeNode(fld.FullyQualifiedName)
                If disabled Then n.ForeColor = PrivateItemColor
                n.NodeFont = mNonBoldFont
                n.Tag = fld
                parent.Nodes.Add(n)
            End If
            If fld.HasChildren() Then AddNodesToDataTypeNode(parent, fld.Children, disabled)
        Next
    End Sub

    ''' <summary>
    ''' Adds child field nodes to a parent node representing a collection.
    ''' </summary>
    ''' <param name="parent">The parent node to add to</param>
    Private Sub AddNodesToCollectionNode( _
     ByVal parent As TreeNode, ByVal mgr As ICollectionDefinitionManager, ByVal disabled As Boolean)

        If mgr Is Nothing OrElse mgr.FieldCount = 0 Then Return
        If disabled AndAlso Not ViewAll Then Return

        For Each fld As clsCollectionFieldInfo In mgr.FieldDefinitions
            Dim n As New TreeNode(fld.FullyQualifiedName)
            If disabled Then n.ForeColor = PrivateItemColor
            n.NodeFont = mNonBoldFont
            n.Tag = fld
            parent.Nodes.Add(n)
            If fld.HasChildren() Then AddNodesToCollectionNode(n, fld.Children, disabled)
        Next
    End Sub

    ''' <summary>
    ''' Determines if a stage is in scope of the current sheet.
    ''' </summary>
    ''' <param name="stg">The stage to test.</param>
    ''' <returns>Returns true if the stage is in scope false otherwise.</returns>
    Private Function IsInScope(ByVal stg As clsDataStage) As Boolean
        Return IgnoreScope OrElse Not _
         (stg.IsPrivate AndAlso stg.GetSubSheetID() <> mCurrentSheetId)
    End Function

    ''' <summary>
    ''' Displays summary details of the selected node.
    ''' </summary>
    ''' <param name="node">The selected tree</param>
    Private Sub SetToolTip(ByRef node As TreeNode)
        Dim objStage As clsDataStage
        Dim objField As clsCollectionFieldInfo
        Dim sTip As String = "", sName As String = ""
        Dim sNarrative As String = ""
        Dim sPage As String = ""
        Dim sStageType As String = ""
        Dim sDataType As String = ""

        mToolTip.RemoveAll()

        If node Is Nothing _
         OrElse node.Tag Is Nothing _
         OrElse Not (node.Tag.GetType.Equals(GetType(clsDataStage)) _
         Or node.Tag.GetType.Equals(GetType(clsCollectionFieldInfo))) Then

            'There is no tooltip to display
            mToolTipNode = Nothing

        Else
            If node.Tag.GetType.Equals(GetType(clsDataStage)) Then

                'The node is a data item
                objStage = CType(node.Tag, clsDataStage)
                sName = My.Resources.ctlDataItemTreeView_Name & objStage.GetName & vbCrLf
                If objStage.GetNarrative <> "" Then
                    sNarrative = My.Resources.ctlDataItemTreeView_Description & objStage.GetNarrative & vbCrLf
                End If
                If objStage.GetSubSheetID.Equals(Guid.Empty) Then
                    sPage &= My.Resources.ctlDataItemTreeView_PageMainPage & vbCrLf
                Else
                    sPage &= My.Resources.ctlDataItemTreeView_Page & objStage.Process.GetSubSheetName(objStage.GetSubSheetID) & vbCrLf
                End If

                sStageType = My.Resources.ctlDataItemTreeView_StageType & objStage.StageType.ToString().ToUpper() & vbCrLf

                If objStage.StageType = StageTypes.Data Then
                    Dim objData As clsDataStage = CType(objStage, clsDataStage)
                    sDataType = My.Resources.ctlDataItemTreeView_DataType & clsProcessDataTypes.GetFriendlyName(objData.GetDataType) & vbCrLf
                ElseIf objStage.StageType = StageTypes.Collection Then
                    'This a fudge to avoid 'unknown'
                    sDataType = My.Resources.ctlDataItemTreeView_DataTypeCollection & vbCrLf
                End If

            ElseIf node.Tag.GetType.Equals(GetType(clsCollectionFieldInfo)) Then

                'The node is a collection field
                objField = CType(node.Tag, clsCollectionFieldInfo)
                sName = My.Resources.ctlDataItemTreeView_Name & objField.Name & vbCrLf
                sDataType = My.Resources.ctlDataItemTreeView_DataType & clsProcessDataTypes.GetFriendlyName(objField.DataType) & vbCrLf
                'This a fudge
                sStageType = My.Resources.ctlDataItemTreeView_StageTypeCollectionField & vbCrLf

            End If

            If (sToolTipStyle And ToolTipStyle.Name) > 0 Then
                sTip &= sName
            End If

            If (sToolTipStyle And ToolTipStyle.Narrative) > 0 Then
                sTip &= sNarrative
            End If

            If (sToolTipStyle And ToolTipStyle.Sheet) > 0 Then
                sTip &= sPage
            End If

            If (sToolTipStyle And ToolTipStyle.StageType) > 0 Then
                sTip &= sStageType
            End If

            If (sToolTipStyle And ToolTipStyle.DataType) > 0 Then
                sTip &= sDataType
            End If

            If sToolTipStyle > 0 AndAlso sTip <> "" Then
                'Display tip minus last vbcrlf
                mToolTip.SetToolTip(node.TreeView, sTip.Remove(sTip.Length - 2, 2))
                mToolTipNode = node
            End If

        End If
    End Sub

    Private Function BuildTreeContextMenu(ByVal Node As TreeNode) As ContextMenuStrip
        If Not Node Is Nothing Then
            Dim Menu As New ContextMenuStrip
            Dim Item As ToolStripMenuItem

            Item = CType(Menu.Items.Add(My.Resources.ctlDataItemTreeView_AddNewAsChild, Nothing, AddressOf HandleAddNewClicked), ToolStripMenuItem)
            Item.Enabled = (TypeOf Node.Tag Is DataType) OrElse (TypeOf Node.Tag Is Guid)

            Item = CType(Menu.Items.Add(My.Resources.ctlDataItemTreeView_InsertNew, Nothing, AddressOf HandleInsertNewClicked), ToolStripMenuItem)
            Item.Enabled = (TypeOf Node.Tag Is clsDataStage) AndAlso (Not TypeOf Node.Tag Is clsCollectionStage)

            Item = CType(Menu.Items.Add(My.Resources.ctlDataItemTreeView_Rename, Nothing, AddressOf HandleRenameClicked), ToolStripMenuItem)
            Item.Enabled = TypeOf Node.Tag Is clsDataStage


            Item = CType(Menu.Items.Add(My.Resources.ctlDataItemTreeView_Properties, Nothing, AddressOf HandlePropertiesClicked), ToolStripMenuItem)
            Item.Enabled = mProcessViewer IsNot Nothing AndAlso ((TypeOf Node.Tag Is clsDataStage) OrElse (TypeOf Node.Tag Is clsCollectionFieldInfo))

            Return Menu
        Else
            Return Nothing
        End If
    End Function

    Private Sub HandleAddNewClicked(ByVal sender As Object, ByVal e As EventArgs)
        If Me.mCurrentTree.SelectedNode IsNot Nothing Then

            Dim Node As TreeNode = Me.mCurrentTree.SelectedNode

            Dim SubSheetID As Guid
            If mStage IsNot Nothing Then
                SubSheetID = Me.mStage.GetSubSheetID
            End If
            Dim DataType As DataType = DataType.unknown

            'Collect information about the context of the new stage
            Select Case True
                Case TypeOf Node.Tag Is Guid
                    SubSheetID = CType(Node.Tag, Guid)
                    If Node.Nodes.Count > 0 Then
                        If (TypeOf Node.Nodes(0).Tag Is DataType) Then
                            DataType = CType(Node.Nodes(0).Tag, DataType)
                        End If
                    End If
                Case TypeOf Node.Tag Is DataType
                    DataType = CType(Node.Tag, DataType)
                    If (Node.Parent IsNot Nothing) AndAlso (TypeOf Node.Parent.Tag Is Guid) Then
                        SubSheetID = CType(Node.Parent.Tag, Guid)
                        If Node.Nodes.Count > 0 Then
                            If (TypeOf Node.Nodes(0).Tag Is clsDataStage) Then
                                Dim ds As clsDataStage = CType(Node.Nodes(0).Tag, clsDataStage)
                            End If
                        End If
                    End If
                Case Else
                    UserMessage.Show(My.Resources.ctlDataItemTreeView_ThisIsNotAValidLocationToAddADataItemPleaseChooseAPageNodeOrADataTypeNodeWhenAd)
                    Exit Sub
            End Select

            CreateStage(DataType, SubSheetID)
        End If
    End Sub

    Private Sub HandleInsertNewClicked(ByVal sender As Object, ByVal e As EventArgs)
        If Me.mCurrentTree.SelectedNode IsNot Nothing Then
            Dim Node As TreeNode = Me.mCurrentTree.SelectedNode

            Dim OffSet As Size = New Size(15, 15)
            Dim Location As PointF
            Dim SubSheetID As Guid
            If mStage IsNot Nothing Then
                SubSheetID = Me.mStage.GetSubSheetID
                Location = New PointF(CSng(mStage.GetDisplayX), CSng(mStage.GetDisplayY))
                Location = PointF.Add(Location, OffSet)
            End If
            Dim DataType As DataType = DataType.unknown

            If TypeOf Node.Tag Is clsDataStage Then
                Select Case True
                    Case Node.Parent Is Nothing
                        Exit Select
                    Case TypeOf Node.Parent.Tag Is DataType
                        DataType = CType(Node.Parent.Tag, DataType)
                        If Node.Parent.Parent IsNot Nothing Then
                            If TypeOf Node.Parent.Parent.Tag Is Guid Then
                                SubSheetID = CType(Node.Parent.Parent.Tag, Guid)
                                Dim d As clsDataStage = CType(Node.Tag, clsDataStage)
                                Location = New PointF(CSng(d.GetDisplayX), CSng(d.GetDisplayY))
                                Location = PointF.Add(Location, OffSet)
                            End If
                        End If
                    Case TypeOf Node.Parent.Tag Is Guid
                        SubSheetID = CType(Node.Parent.Tag, Guid)
                End Select

                CreateStage(DataType, SubSheetID)
            Else
                UserMessage.Show(My.Resources.ctlDataItemTreeView_YouMayOnlyInsertNewDataStagesNextToExistingDataStages)
            End If
        End If
    End Sub

    Private Sub HandlePropertiesClicked(ByVal sender As Object, ByVal e As EventArgs)
        If Me.mCurrentTree.SelectedNode IsNot Nothing Then
            Dim Node As TreeNode = Me.mCurrentTree.SelectedNode
            If Node IsNot Nothing Then
                ShowProperties(Node)
            End If
        End If
    End Sub

    ''' <summary>
    ''' Creates a data stage of the specified
    ''' type on the specified page.
    ''' </summary>
    ''' <param name="dtype">The type of data
    ''' stage to create</param>
    ''' <param name="SubSheetID">The ID of the sheet
    ''' on which to create the stage.</param>
    Private Sub CreateStage(ByVal dtype As DataType, ByVal SubSheetID As Guid)

        Dim dataStg As clsDataStage = clsDataStage.Create(mProcess, dtype)
        dataStg.SetSubSheetID(SubSheetID)

        'Position the stage
        Static positioner As clsProcessStagePositioner
        If positioner Is Nothing Then
            positioner = New clsProcessStagePositioner(Me.mProcess)
        End If

        Static lastposition As clsProcessStagePositioner.RelativePositions
        If mStage IsNot Nothing Then
            positioner.PositionStageAsBuddyOf(dataStg, Me.mStage, lastposition)
        End If

        Me.Repopulate(dataStg)
    End Sub

    Private Sub HandleRenameClicked(ByVal sender As Object, ByVal e As EventArgs)
        If mCurrentTree.SelectedNode IsNot Nothing Then
            mCurrentTree.LabelEdit = True
            mCurrentTree.SelectedNode.BeginEdit()
        End If
    End Sub

    ''' <summary>
    ''' Handles a data item tree item being renamed in place.
    ''' </summary>
    Private Sub HandleNodeNameChanged(ByVal sender As Object, ByVal e As NodeLabelEditEventArgs) _
     Handles mCurrentTree.AfterLabelEdit

        ' Check we're dealing with a data stage - if not, just exit, ensuring we're
        ' not editing the tree first
        Dim stg As clsDataStage = TryCast(e.Node.Tag, clsDataStage)
        If stg Is Nothing Then mCurrentTree.LabelEdit = False : Return

        Dim errmsg As String = Nothing

        'Check uniqueness of data item name, and show error if needs be
        Dim conflictStage As clsProcessStage = CollectionUtil.First(stg.FindNamingConflicts(e.Label))
        If conflictStage IsNot Nothing Then ' If we have a conflict with another stage...
            UserMessage.Show(String.Format(
             My.Resources.ctlDataItemTreeView_TheChosenNameForThisStageConflictsWithTheStage0OnPage1PleaseChooseAnother, conflictStage.Name, conflictStage.GetSubSheetName()))

            ' Check the name is valid, if the user has changed it.
        ElseIf e.Label IsNot Nothing AndAlso Not clsDataStage.IsValidDataName(e.Label, errmsg) Then
            UserMessage.Show(String.Format(My.Resources.ctlDataItemTreeView_TheChosenNameIsInvalid0, errmsg))

        Else
            ' If the user changed something, update the stage and the tree
            If e.Label <> "" AndAlso e.Label <> stg.Name Then
                ' Check that the name is a valid one
                stg.Name = e.Label
                Repopulate(stg)
            End If

            e.CancelEdit = False
            mCurrentTree.LabelEdit = False
            Return ' Make sure we don't end up cancelling the edit and re-editing

        End If

        ' Cancel the current edit, but keep the editing open and available
        e.CancelEdit = True
        Invoke(New MethodInvoker(AddressOf e.Node.BeginEdit))

    End Sub

    Private Sub TreeView_AfterCheck(ByVal sender As Object, ByVal e As TreeViewEventArgs) _
     Handles trvStages.AfterCheck, trvStagesByDataType.AfterCheck, _
     trvStagesBySheet.AfterCheck, trvStagesBySheetAndDataType.AfterCheck
        'Update the checkboxes in this tree.
        UpdateTreeViewCheckBoxes(e.Node)

        'Update the checkbox hierachy in the other trees.
        For Each tview As TreeView In Trees
            If tview IsNot sender Then UpdateTreeViewCheckBoxes(tview.SelectedNode, e.Node.Checked)
        Next
    End Sub

    ''' <summary>
    ''' Analyses the children of a node and modifies their checkboxes.
    ''' </summary>
    ''' <param name="node">The node start from</param>
    Private Sub CheckChildren(ByRef node As TreeNode)
        For Each child As TreeNode In node.Nodes
            child.Checked = node.Checked
            CheckChildren(child)
        Next
    End Sub

    ''' <summary>
    ''' Analyses the children, siblings and ancestors of a node and modifies their
    ''' checkboxes.
    ''' </summary>
    ''' <param name="node">The node start from</param>
    Private Sub UpdateTreeViewCheckBoxes(ByRef node As TreeNode)
        If Not node Is Nothing Then
            UpdateTreeViewCheckBoxes(node, node.Checked)
        End If
    End Sub

    ''' <summary>
    ''' Analyses the children, siblings and ancestors of a node and applies the given
    ''' status to their checkboxes. 
    ''' </summary>
    ''' <param name="node">The node start from</param>
    Private Sub UpdateTreeViewCheckBoxes(ByRef node As TreeNode, ByVal checked As Boolean)
        Dim bSiblingsHaveSameStatus, bSiblingsAreAllChecked As Boolean
        Dim sibling, ancestor As TreeNode

        If node Is Nothing Then
            'Do nothing
        Else

            RemoveHandler node.TreeView.AfterCheck, AddressOf TreeView_AfterCheck
            RemoveHandler node.TreeView.BeforeCheck, AddressOf TreeView_BeforeCheck

            node.Checked = checked

            'Give descendants the same status as the node.
            CheckChildren(node)

            If Not node.Parent Is Nothing Then
                'See if siblings have same status.
                bSiblingsHaveSameStatus = True
                For Each sibling In node.Parent.Nodes
                    If sibling.Checked <> node.Checked Then
                        bSiblingsHaveSameStatus = False
                        Exit For
                    End If
                Next
                If bSiblingsHaveSameStatus And node.Checked Then
                    'Siblings are all checked, so check the parent.
                    node.Parent.Checked = True
                    'Move up the tree checking ancestors as required.
                    ancestor = node.Parent.Parent
                    While Not ancestor Is Nothing
                        bSiblingsAreAllChecked = True
                        For Each sibling In ancestor.Nodes
                            If Not sibling.Checked Then
                                bSiblingsAreAllChecked = False
                                Exit For
                            End If
                        Next
                        ancestor.Checked = bSiblingsAreAllChecked
                        ancestor = ancestor.Parent
                    End While
                Else
                    'Siblings differ, so uncheck all ancestors.
                    ancestor = node.Parent
                    While Not ancestor Is Nothing
                        ancestor.Checked = False
                        ancestor = ancestor.Parent
                    End While
                End If
            End If

            AddHandler node.TreeView.BeforeCheck, AddressOf TreeView_BeforeCheck
            AddHandler node.TreeView.AfterCheck, AddressOf TreeView_AfterCheck
        End If

    End Sub

    Private Sub TreeView_BeforeCheck(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeViewCancelEventArgs) _
    Handles trvStages.BeforeCheck, _
    trvStagesByDataType.BeforeCheck, _
    trvStagesBySheet.BeforeCheck, _
    trvStagesBySheetAndDataType.BeforeCheck

        Dim trvSender, trvOther As TreeView

        'Make the node at the mouse click point the selected node
        trvSender = CType(sender, TreeView)
        trvSender.SelectedNode = e.Node

        'Find the corresponding node in the other trees
        For Each trvOther In Trees
            If Not trvOther Is trvSender AndAlso Not trvSender.SelectedNode Is Nothing Then
                SelectAndCheckNode(trvSender.SelectedNode.Text, trvOther.Nodes(0), Not e.Node.Checked)
            End If
        Next

    End Sub

    Private Sub TreeView_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) _
    Handles trvStages.MouseDown, _
    trvStagesByDataType.MouseDown, _
    trvStagesBySheet.MouseDown, _
    trvStagesBySheetAndDataType.MouseDown

        Dim trvSender, trvOther As TreeView

        'Make the node at the mouse click point the selected node
        trvSender = CType(sender, TreeView)
        trvSender.SelectedNode = trvSender.GetNodeAt(New Point(e.X, e.Y))

        'Find the corresponding node in the other trees
        For Each trvOther In Trees
            If Not trvOther Is trvSender AndAlso Not trvSender.SelectedNode Is Nothing AndAlso trvOther.Nodes.Count > 0 Then
                SelectNode(trvSender.SelectedNode.Text, trvOther.Nodes(0))
            End If
        Next
    End Sub

    Private Sub TreeView_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) _
    Handles trvStages.MouseMove, _
    trvStagesByDataType.MouseMove, _
    trvStagesBySheet.MouseMove, _
    trvStagesBySheetAndDataType.MouseMove

        Dim tree As TreeView
        Dim node As TreeNode

        tree = CType(sender, TreeView)
        node = tree.GetNodeAt(New Point(e.X, e.Y))
        If Not node Is mToolTipNode Then
            SetToolTip(node)
        End If

    End Sub

    Private Sub TreeView_MouseLeave(ByVal sender As Object, ByVal e As ItemDragEventArgs) _
    Handles trvStages.ItemDrag, _
    trvStagesByDataType.ItemDrag, _
    trvStagesBySheet.ItemDrag, _
    trvStagesBySheetAndDataType.ItemDrag

        If TypeOf e.Item Is TreeNode Then

            Dim oNode As TreeNode = CType(e.Item, TreeNode)
            Dim Cancel As Boolean = False

            If Not oNode.Tag Is Nothing Then
                Select Case True
                    Case (TypeOf oNode.Tag Is clsProcessStage)
                        Dim data As clsDataStage = CType(oNode.Tag, clsDataStage)
                        If Not IsInScope(data) Then
                            Cancel = True
                        End If
                    Case (TypeOf oNode.Tag Is clsCollectionFieldInfo)
                        Dim col As clsCollectionStage = CType(oNode.Tag, clsCollectionFieldInfo).Parent.ParentStage
                        If (col IsNot Nothing) AndAlso (Not IsInScope(col)) Then
                            Cancel = True
                        End If
                    Case Else
                        Cancel = True
                End Select
            Else
                Cancel = True
            End If


            If Not Cancel Then
                DoDragDrop(e.Item, DragDropEffects.Move)
            Else
                DoDragDrop(e.Item, DragDropEffects.None)
            End If

        End If

        mToolTip.RemoveAll()
        mToolTipNode = Nothing

    End Sub

    ''' <summary>
    ''' Handles a treenode being double clicked by showing the properties form for
    ''' that node (assuming a process viewer has been set in this treeview).
    ''' </summary>
    Private Sub HandleNodeDoubleClick(ByVal sender As Object, ByVal e As TreeNodeMouseClickEventArgs) _
     Handles trvStages.NodeMouseDoubleClick, trvStagesByDataType.NodeMouseDoubleClick, _
      trvStagesBySheet.NodeMouseDoubleClick, trvStagesBySheetAndDataType.NodeMouseDoubleClick
        If mProcessViewer IsNot Nothing Then ShowProperties(e.Node)
    End Sub

    ''' <summary>
    ''' Show the properties for the data item represented by the given treenode.
    ''' </summary>
    ''' <param name="node">The data item treenode for which the properties should be
    ''' displayed.</param>
    Private Sub ShowProperties(ByVal node As TreeNode)
        ' Abort immediately if we can't show properties for this node (or lack thereof)
        If node Is Nothing OrElse mProcessViewer Is Nothing Then Return

        ' Find the stage that it's related to, or the collection stage that the field is in
        Dim stg As clsDataStage = TryCast(node.Tag, clsDataStage)
        If stg Is Nothing Then
            Dim info As clsCollectionFieldInfo = TryCast(node.Tag, clsCollectionFieldInfo)
            If info IsNot Nothing Then stg = info.Parent.ParentStage
        End If

        ' If we found a stage, open the properties and, if the user OK'd them, repopulate
        ' this control in case the user changed the stage.
        If stg IsNot Nothing
            mProcessViewer.LaunchStageProperties(stg)
            AddHandler mProcessViewer.PropertySaved, AddressOf HandlePropertyFormSaving
        End If

    End Sub

    Private Sub HandlePropertyFormSaving(sender As Object, e As EventArgs)
        Dim propertyForm = TryCast(sender, frmProperties)
        If propertyForm Is Nothing Then Exit Sub

        Dim stage = TryCast(propertyForm.ProcessStage, clsDataStage)
        If stage IsNot Nothing Then Repopulate(stage)

        RemoveHandler mProcessViewer.PropertySaved, AddressOf HandlePropertyFormSaving
    End Sub

    Private Sub mtrvCurrentTree_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles mCurrentTree.MouseUp
        Select Case e.Button
            Case System.Windows.Forms.MouseButtons.Right
                Dim Node As TreeNode = Me.mCurrentTree.GetNodeAt(e.Location)
                If Node IsNot Nothing Then
                    Me.mCurrentTree.SelectedNode = Node
                    Dim Menu As ContextMenuStrip = Me.BuildTreeContextMenu(Node)
                    If Menu IsNot Nothing Then
                        Menu.Show(Me.mCurrentTree, e.Location)
                    End If
                End If
        End Select
    End Sub

    Private Sub HandleViewCheckboxesChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles chkPage.CheckedChanged, chkDataType.CheckedChanged, chkViewAllItems.CheckedChanged
        ' We don't do anything if these checkboxes are being set on initialisation
        If mInitialised Then
            gSv.SetUserPref(PreferenceNames.StudioTreeViewMode.ViewModePrefName, Mode)
            If sender Is chkViewAllItems Then Repopulate()
            DisplayTree()
        End If
    End Sub

    Private Sub mCurrentTree_KeyDown(sender As Object, e As KeyEventArgs) Handles mCurrentTree.KeyDown
        If e.KeyCode = Keys.F10 AndAlso e.Modifiers = Keys.Shift Then
            Dim treeNode As TreeNode = Me.mCurrentTree.SelectedNode
            If treeNode IsNot Nothing Then
                Me.mCurrentTree.SelectedNode = treeNode
                Me.BuildTreeContextMenu(treeNode)?.Show(Me.mCurrentTree.PointToScreen(Point.Empty))
            End If
        End If
    End Sub

End Class
