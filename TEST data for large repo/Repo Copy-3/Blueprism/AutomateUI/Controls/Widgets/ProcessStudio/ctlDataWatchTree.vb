Imports AutomateControls.TreeList
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

Public Class ctlDataWatchTree
    Inherits System.Windows.Forms.UserControl

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.chkDataType.Checked = True
    End Sub

    'UserControl overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents objDataList As AutomateControls.TreeList.TreeListView
    Friend WithEvents pnlHeader As AutomateUI.clsPanel
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents chkPage As System.Windows.Forms.CheckBox
    Friend WithEvents chkDataType As System.Windows.Forms.CheckBox
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As System.Windows.Forms.ColumnHeader
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlDataWatchTree))
        Dim TreeListViewItemCollectionComparer1 As AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer = New AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer()
        Me.objDataList = New AutomateControls.TreeList.TreeListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.pnlHeader = New AutomateUI.clsPanel()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.chkPage = New System.Windows.Forms.CheckBox()
        Me.chkDataType = New System.Windows.Forms.CheckBox()
        Me.pnlHeader.SuspendLayout()
        Me.SuspendLayout()
        '
        'objDataList
        '
        Me.objDataList.AllowDrop = True
        resources.ApplyResources(Me.objDataList, "objDataList")
        Me.objDataList.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2})
        TreeListViewItemCollectionComparer1.Column = 0
        TreeListViewItemCollectionComparer1.SortOrder = System.Windows.Forms.SortOrder.Ascending
        Me.objDataList.Comparer = TreeListViewItemCollectionComparer1
        Me.objDataList.FocusedItem = Nothing
        Me.objDataList.Name = "objDataList"
        Me.objDataList.SmallImageList = Me.ImageList1
        Me.objDataList.UseCompatibleStateImageBehavior = False
        '
        'ColumnHeader1
        '
        resources.ApplyResources(Me.ColumnHeader1, "ColumnHeader1")
        '
        'ColumnHeader2
        '
        resources.ApplyResources(Me.ColumnHeader2, "ColumnHeader2")
        '
        'ImageList1
        '
        Me.ImageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit
        resources.ApplyResources(Me.ImageList1, "ImageList1")
        Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
        '
        'pnlHeader
        '
        resources.ApplyResources(Me.pnlHeader, "pnlHeader")
        Me.pnlHeader.BorderColor = System.Drawing.SystemColors.InactiveCaption
        Me.pnlHeader.BorderStyle = AutomateUI.clsPanel.BorderMode.[On]
        Me.pnlHeader.BorderWidth = 1
        Me.pnlHeader.Controls.Add(Me.Label1)
        Me.pnlHeader.Controls.Add(Me.chkPage)
        Me.pnlHeader.Controls.Add(Me.chkDataType)
        Me.pnlHeader.Name = "pnlHeader"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'chkPage
        '
        resources.ApplyResources(Me.chkPage, "chkPage")
        Me.chkPage.Name = "chkPage"
        '
        'chkDataType
        '
        resources.ApplyResources(Me.chkDataType, "chkDataType")
        Me.chkDataType.Name = "chkDataType"
        '
        'ctlDataWatchTree
        '
        Me.AllowDrop = True
        Me.Controls.Add(Me.objDataList)
        Me.Controls.Add(Me.pnlHeader)
        Me.Name = "ctlDataWatchTree"
        resources.ApplyResources(Me, "$this")
        Me.pnlHeader.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

    ''' <summary>
    ''' Different views of this control.
    ''' </summary>
    Private Enum SortModes
        Alpha
        Page
        DataType
        PageAndDataType
    End Enum


    ''' <summary>
    ''' The stages displayed in this treeview
    ''' </summary>
    Private mStages As New SortedList


    ''' <summary>
    ''' Adds the supplied stage to the control.
    ''' </summary>
    ''' <param name="Stage"></param>
    Public Sub AddStage(ByVal Stage As clsProcessStage)
        If Stage.StageType = StageTypes.Data OrElse Stage.StageType = StageTypes.Collection Then
            Dim objData As clsDataStage = CType(Stage, clsDataStage)
            If Not mStages.Contains(Stage.GetStageID) Then
                mStages.Add(Stage.GetStageID, Stage)
                Me.UpdateTreeWithNewStage(objData)
            End If
        Else
            Throw New InvalidArgumentException(My.Resources.ctlDataWatchTree_ErrorBadStageTypeSentToDataWatchWindow)
        End If
    End Sub

    ''' <summary>
    ''' Removes the supplied stage from the control.
    ''' </summary>
    ''' <param name="Stage">The stage.</param>
    Public Sub RemoveStage(ByVal Stage As clsProcessStage)
        mStages.Remove(Stage.GetStageID)
        Me.RefreshTree()
    End Sub

    ''' <summary>
    ''' Adds the supplied stage to the treeview at the appropriate point. Saves
    ''' having to rebuild the entire tree.
    ''' </summary>
    ''' <param name="stage">The stage to add.</param>
    Private Sub UpdateTreeWithNewStage(ByVal stage As clsDataStage)
        If Me.objDataList.Items.Count > 0 Then
            Try

                Dim i As Integer
                Dim item As TreeListViewItem = Nothing

                Select Case Me.GetSortMode
                    Case SortModes.Alpha
                        'find the right place to insert stage within this data type node
                        Dim bFound As Boolean
                        While i < Me.objDataList.Items.Count
                            item = Me.objDataList.Items(i)
                            If item.Text.CompareTo(stage.GetName) <= 0 Then
                                bFound = True
                                Exit While
                            End If
                            i += 1
                        End While

                        'do the insertion
                        Dim newitem As TreeListViewItem = Me.InsertStageNodeIntoNodeCollection(Me.objDataList.Items, CInt(IIf(bFound, i + 1, 0)), stage)

                        If TypeOf stage Is clsCollectionStage Then
                            Me.AddCollectionFieldsToNode(newitem, CType(stage, clsCollectionStage))
                            newitem.Expand()
                        End If

                        Debug.Assert(Not newitem Is Nothing)
                        newitem.Font = New Font(Me.objDataList.Font, FontStyle.Regular)

                    Case SortModes.DataType
                        'find the right data type node
                        Dim dt As DataType = stage.GetDataType
                        Dim bFound As Boolean
                        While i < Me.objDataList.Items.Count
                            item = Me.objDataList.Items(i)
                            If item.Text.CompareTo(clsProcessDataTypes.GetFriendlyName(dt, True)) = 0 Then
                                bFound = True
                                Exit While
                            End If
                            i += 1
                        End While
                        If Not bFound Then Throw New InvalidOperationException(My.Resources.ctlDataWatchTree_InternalErrorDataTypeNodeNotFound)

                        'find the right place to insert stage within this data type node
                        bFound = False
                        Dim childitem As TreeListViewItem = Nothing
                        i = 0
                        While i < item.Items.Count
                            childitem = item.Items(i)
                            If childitem.Text.CompareTo(stage.GetName) <= 0 Then
                                bFound = True
                                Exit While
                            End If
                            i += 1
                        End While

                        'do the insertion
                        Dim newitem As TreeListViewItem = Me.InsertStageNodeIntoNodeCollection(item.Items, CInt(IIf(bFound, i + 1, 0)), stage)

                        If TypeOf stage Is clsCollectionStage Then
                            Me.AddCollectionFieldsToNode(newitem, CType(stage, clsCollectionStage))
                            newitem.Expand()
                        End If

                        Debug.Assert(Not newitem Is Nothing)
                        newitem.Font = New Font(Me.objDataList.Font, FontStyle.Regular)

                    Case SortModes.Page
                        'find the right page node
                        Dim bfound As Boolean
                        Dim Page As String = stage.Process.GetSubSheetName(stage.GetSubSheetID)
                        While i < Me.objDataList.Items.Count
                            item = Me.objDataList.Items(i)
                            If item.Text.CompareTo(Page) = 0 Then
                                bfound = True
                                Exit While
                            End If
                            i += 1
                        End While
                        If Not bfound Then Throw New InvalidOperationException(My.Resources.ctlDataWatchTree_InternalErrorPageNodeNotFound)

                        'find the right place to insert stage within this data type node
                        Dim childitem As TreeListViewItem
                        i = 0
                        bfound = False
                        While i < item.Items.Count
                            childitem = item.Items(i)
                            If childitem.Text.CompareTo(stage.GetName) <= 0 Then
                                bfound = True
                                Exit While
                            End If
                            i += 1
                        End While

                        'do the insertion
                        Dim newitem As TreeListViewItem = Me.InsertStageNodeIntoNodeCollection(item.Items, CInt(IIf(bfound, i + 1, 0)), stage)

                        If TypeOf stage Is clsCollectionStage Then
                            Me.AddCollectionFieldsToNode(newitem, CType(stage, clsCollectionStage))
                            newitem.Expand()
                        End If

                        Debug.Assert(Not newitem Is Nothing)
                        newitem.Font = New Font(Me.objDataList.Font, FontStyle.Regular)

                    Case SortModes.PageAndDataType
                        'find the right page node
                        Dim bfound As Boolean
                        Dim Page As String = stage.Process.GetSubSheetName(stage.GetSubSheetID)
                        While i < Me.objDataList.Items.Count
                            item = Me.objDataList.Items(i)
                            If item.Text.CompareTo(Page) = 0 Then
                                bfound = True
                                Exit While
                            End If
                            i += 1
                        End While
                        If Not bfound Then Throw New InvalidOperationException(My.Resources.ctlDataWatchTree_InternalErrorPageNodeNotFound)

                        'find the right data type node
                        Dim dt As DataType = stage.GetDataType
                        Dim childitem As TreeListViewItem = Nothing
                        i = 0
                        bfound = False
                        While i < item.Items.Count
                            childitem = item.Items(i)
                            If childitem.Text.CompareTo(clsProcessDataTypes.GetFriendlyName(dt, True)) = 0 Then
                                bfound = True
                                Exit While
                            End If
                            i += 1
                        End While
                        If Not bfound Then Throw New InvalidOperationException(My.Resources.ctlDataWatchTree_InternalErrorDataTypeNodeNotFound)

                        'find the right place to insert stage within this data type node
                        Dim grandchilditem As TreeListViewItem
                        i = 0
                        bfound = False
                        While i < childitem.Items.Count
                            grandchilditem = childitem.Items(i)
                            If grandchilditem.Text.CompareTo(stage.GetName) <= 0 Then
                                bfound = True
                                Exit While
                            End If
                            i += 1
                        End While

                        'do the insertion
                        Dim newitem As TreeListViewItem = Me.InsertStageNodeIntoNodeCollection(childitem.Items, CInt(IIf(bfound, i + 1, 0)), stage)

                        If TypeOf stage Is clsCollectionStage Then
                            Me.AddCollectionFieldsToNode(newitem, CType(stage, clsCollectionStage))
                            newitem.Expand()
                        End If

                        Debug.Assert(Not newitem Is Nothing)
                        newitem.Font = New Font(Me.objDataList.Font, FontStyle.Regular)

                End Select


            Finally
                Me.objDataList.BeginUpdate()
                'Me.objDataList.Items = Me.objDataList.Items
                Me.objDataList.Items.SortOrderRecursively = Me.objDataList.Sorting
                Me.objDataList.EndUpdate()
            End Try

        Else
            If Not Me.mStages.Contains(stage.GetStageID) Then
                Me.mStages.Add(stage.GetStageID, stage)
            End If
            Me.RefreshTree()
        End If
    End Sub

    ''' <summary>
    ''' Clears all nods in the tree and rebuilds the tree using the stages
    ''' contained in the member mStages. The view is according the to values
    ''' in the user interface checkboxes.
    ''' </summary>
    Public Sub RefreshTree()
        Try

            Me.objDataList.BeginUpdate()
            Me.objDataList.Items.Clear()

            If Not (Me.mStages Is Nothing OrElse Me.mStages.Count = 0) Then

                Dim item As TreeListViewItem
                Select Case Me.GetSortMode
                    Case SortModes.Alpha
                        'list is already sorted
                        For Each stage As clsDataStage In mStages.Values
                            item = AddStageNodeToNodeCollection(Me.objDataList.Items, stage)
                            item.Font = New Font(Me.objDataList.Font, FontStyle.Regular)
                            Select Case stage.StageType
                                Case StageTypes.Data
                                    'all done now. No further work
                                Case StageTypes.Collection
                                    AddCollectionFieldsToNode(item, CType(stage, clsCollectionStage))
                                Case Else
                                    Throw New InvalidOperationException(My.Resources.ctlDataWatchTree_BadItem)
                            End Select
                        Next
                    Case SortModes.DataType
                        For Each DataTypeName As String In clsProcessDataTypes.DataTypeNames
                            item = Me.AddNodeToNodeCollection(Me.objDataList.Items, clsProcessDataTypes.GetFriendlyName(DataTypeName, True))
                            item.Font = New Font(Me.objDataList.Font, FontStyle.Bold)
                            For Each stage As clsDataStage In mStages.Values
                                If stage.GetDataType.ToString = DataTypeName Then
                                    Dim child As TreeListViewItem = Me.AddStageNodeToNodeCollection(item.Items, stage)
                                    child.Font = New Font(Me.objDataList.Font, FontStyle.Regular)
                                    Select Case stage.StageType
                                        Case StageTypes.Data
                                            'all done now. No further work
                                        Case StageTypes.Collection
                                            AddCollectionFieldsToNode(child, CType(stage, clsCollectionStage))
                                        Case Else
                                            Throw New InvalidOperationException(My.Resources.ctlDataWatchTree_BadItem)
                                    End Select
                                End If
                            Next
                        Next
                    Case SortModes.Page
                        Dim p As clsProcess = CType(Me.mStages.GetByIndex(0), clsProcessStage).Process
                        For Each objSub As clsProcessSubSheet In p.SubSheets
                            item = Me.AddNodeToNodeCollection(Me.objDataList.Items, objSub.Name)
                            item.Font = New Font(Me.objDataList.Font, FontStyle.Bold)
                            For Each stage As clsDataStage In mStages.Values
                                If stage.GetSubSheetID.Equals(objSub.ID) Then
                                    Dim child As TreeListViewItem = Me.AddStageNodeToNodeCollection(item.Items, stage)
                                    child.Font = New Font(Me.objDataList.Font, FontStyle.Regular)
                                    Select Case stage.StageType
                                        Case StageTypes.Data
                                            'all done now. No further work
                                        Case StageTypes.Collection
                                            AddCollectionFieldsToNode(child, CType(stage, clsCollectionStage))
                                        Case Else
                                            Throw New InvalidOperationException(My.Resources.ctlDataWatchTree_BadItem)
                                    End Select
                                End If
                            Next
                        Next
                    Case SortModes.PageAndDataType
                        Dim p As clsProcess = CType(mStages.GetByIndex(0), clsProcessStage).Process
                        For Each objSub As clsProcessSubSheet In p.SubSheets
                            item = Me.AddNodeToNodeCollection(Me.objDataList.Items, objSub.Name)
                            item.Font = New Font(Me.objDataList.Font, FontStyle.Bold)
                            Dim child As TreeListViewItem
                            For Each DataTypeName As String In clsProcessDataTypes.DataTypeNames
                                child = Me.AddNodeToNodeCollection(item.Items, clsProcessDataTypes.GetFriendlyName(DataTypeName, True))
                                child.Font = New Font(Me.objDataList.Font, FontStyle.Bold)

                                For Each stage As clsDataStage In mStages.Values
                                    If stage.GetSubSheetID.Equals(objSub.ID) Then
                                        If stage.GetDataType.ToString = DataTypeName Then
                                            Dim grandchild As TreeListViewItem = Me.AddStageNodeToNodeCollection(child.Items, stage)
                                            grandchild.Font = New Font(Me.objDataList.Font, FontStyle.Regular)
                                            Select Case stage.StageType
                                                Case StageTypes.Data
                                                    'all done now. No further work
                                                Case StageTypes.Collection
                                                    AddCollectionFieldsToNode(grandchild, CType(stage, clsCollectionStage))
                                                Case Else
                                                    Throw New InvalidOperationException(My.Resources.ctlDataWatchTree_BadItem)
                                            End Select
                                        End If
                                    End If
                                Next
                            Next
                        Next
                End Select

            End If

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlDataWatchTree_InternalErrorCouldNotPopulateDataWatchTree0, ex.Message))
        Finally
            Me.objDataList.ExpandAll()
            Me.objDataList.EndUpdate()
        End Try
    End Sub

    ''' <summary>
    ''' Updates the tree with the latest current values of the data items.
    ''' </summary>
    Public Sub RefreshDataValues()
        Try
            Me.objDataList.BeginUpdate()
            Me.RecursivelyRefreshStageValues(Me.objDataList.Items)
            Me.objDataList.EndUpdate()
        Catch ex As Exception
            UserMessage.Show(My.Resources.ctlDataWatchTree_InternalErrorCouldNotUpdateCurrentDataItemValuesOnDataWatchWindow)
        End Try
    End Sub

    ''' <summary>
    ''' Recursively updates the current value of all the rows in the suppiled 
    ''' item collection.
    ''' </summary>
    ''' <param name="items">The item collection.</param>
    Private Sub RecursivelyRefreshStageValues(ByVal items As TreeListViewItemCollection)
        If Not items Is Nothing Then
            For Each item As TreeListViewItem In items
                If Not item.Tag Is Nothing Then
                    Dim stage As clsDataStage = CType(item.Tag, clsDataStage)
                    If Not stage Is Nothing Then
                        Select Case stage.StageType
                            Case StageTypes.Collection
                                item.Items.Clear()
                                AddCollectionFieldsToNode(item, CType(stage, clsCollectionStage))
                            Case Else
                                item.SubItems(1).Text = stage.GetValue.FormattedValue
                        End Select
                    End If
                End If
                RecursivelyRefreshStageValues(item.Items)
            Next
        End If
    End Sub

    ''' <summary>
    ''' Reads checkboxes in user interface and interprets as a SortModes value.
    ''' </summary>
    ''' <returns>Sortmodes value.</returns>
    Private Function GetSortMode() As SortModes
        If Me.chkDataType.Checked Then

            If Me.chkPage.Checked Then
                Return SortModes.PageAndDataType
            Else
                Return SortModes.DataType
            End If

        Else

            If Me.chkPage.Checked Then
                Return SortModes.Page
            Else
                Return SortModes.Alpha
            End If

        End If
    End Function

    ''' <summary>
    ''' Adds a node to the supplied collection, corresponding to the supplied node
    ''' collection.
    ''' </summary>
    ''' <param name="items">The collection.</param>
    ''' <param name="Stage">The stage to add.</param>
    ''' <returns>Returns the newly added node.</returns>
    Private Function AddStageNodeToNodeCollection(ByVal items As TreeListViewItemCollection, ByVal Stage As clsDataStage) As TreeListViewItem
        Dim i As TreeListViewItem = New TreeListViewItem(Stage.GetName)
        i.SubItems.Add(Stage.GetValue.FormattedValue)
        items.Add(i)
        i.Tag = Stage

        Return i
    End Function

    ''' <summary>
    ''' Adds a node to the supplied collection with the supplied text.
    ''' </summary>
    ''' <param name="items">The collection.</param>
    ''' <param name="nodename">The text.</param>
    ''' <returns>Returns the newly added node.</returns>
    Private Function AddNodeToNodeCollection(ByVal items As TreeListViewItemCollection, ByVal nodename As String) As TreeListViewItem
        Dim i As TreeListViewItem = New TreeListViewItem(nodename)
        items.Add(i)

        Return i
    End Function

    ''' <summary>
    ''' Inserts a node into the treeview corresponding to the stage supplied.
    ''' The node name will match the name of the stage.
    ''' </summary>
    ''' <param name="items">The node collection into which the new
    ''' node should be inserted.</param>
    ''' <param name="index">The index in the collection at which the new
    ''' node should be inserted.</param>
    ''' <param name="stage">The stage from which a new node should be created.</param>
    ''' <returns>Returns the newly created node.</returns>
    Private Function InsertStageNodeIntoNodeCollection(ByVal items As TreeListViewItemCollection, ByVal index As Integer, ByVal stage As clsDataStage) As TreeListViewItem
        Dim i As TreeListViewItem = New TreeListViewItem(stage.GetName)
        i.SubItems.Add(stage.GetValue.FormattedValue)
        items.Insert(i, index)
        i.Tag = stage

        Return i
    End Function


    ''' <summary>
    ''' Adds the fields of the supplied collection to the supplied node.
    ''' </summary>
    ''' <param name="i">The item to add new nodes to.</param>
    ''' <param name="stage">The collection stage whose nodes will be added.</param>
    Private Sub AddCollectionFieldsToNode(ByVal i As TreeListViewItem, ByVal stage As clsCollectionStage)
        If Not (i Is Nothing OrElse stage Is Nothing) Then
            If stage.GetValue().DataType = DataType.collection Then
                Dim r As clsCollectionRow = stage.CollectionCurrentRow
                If Not r Is Nothing Then
                    For Each s As String In r.FieldNames
                        Dim child As New TreeListViewItem(s)
                        child.SubItems.Add(r.Item(s).FormattedValue)
                        child.Font = New Font(Me.objDataList.Font, FontStyle.Regular)
                        i.Items.Add(child)
                    Next
                End If

                i.Tag = stage
            End If

        Else
            Throw New InvalidArgumentException(My.Resources.ctlDataWatchTree_BadParametersPassedToAddCollectionFieldsToNode)
        End If
    End Sub

    Private Sub chkDataType_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDataType.CheckedChanged, chkPage.CheckedChanged
        Me.RefreshTree()
    End Sub

    Private Sub ctlDataWatchTree_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Resize
        Try
            Me.objDataList.Columns(1).Width = Math.Min(100, Me.objDataList.Width \ 2)
            Me.objDataList.Columns(0).Width = Me.objDataList.Width - Me.objDataList.Columns(1).Width
        Catch
            'do nothing 
        End Try
    End Sub

    Private Sub ctlDataItemWatches_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles objDataList.MouseDown
        If e.Button = System.Windows.Forms.MouseButtons.Right Then
            Dim objContext As New System.Windows.Forms.ContextMenu
            Dim objSenderControl As Control = CType(sender, Control)
            objContext.MenuItems.Add(My.Resources.ctlDataWatchTree_Remove, AddressOf RemoveContext_Click)
            objSenderControl.ContextMenu = objContext



            Dim p As New Point(e.X, e.Y)
            mobjSelecteditems = Me.objDataList.SelectedItems

        End If
    End Sub

    Private mobjSelecteditems As SelectedTreeListViewItemCollection

    Public Sub RemoveContext_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        For Each item As TreeListViewItem In mobjSelecteditems
            If TypeOf item.Tag Is clsProcessStage Then
                Dim stage As clsProcessStage = CType(item.Tag, clsProcessStage)
                mStages.Remove(stage.GetStageID)
            End If
        Next
        Me.RefreshTree()

    End Sub


End Class

