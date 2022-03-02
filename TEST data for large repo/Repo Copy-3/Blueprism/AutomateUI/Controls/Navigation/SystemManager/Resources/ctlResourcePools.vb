Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib
Imports BluePrism.Core.Resources
Imports BluePrism.Images

Public Class ctlResourcePools : Implements IStubbornChild, IPermission, IHelp, IRefreshable

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        trvResourcePools.ImageList = ImageLists.Robots_16x16
        lstPoolsAvailableResources.SmallImageList = ImageLists.Robots_16x16
        lstPoolsAvailableResources.LargeImageList = ImageLists.Robots_32x32

        Me.Enabled = Licensing.License.CanUse(LicenseUse.ResourcePools)
    End Sub

    ''' <summary>
    ''' Handles the New Pool Button click
    ''' </summary>
    ''' <param name="sender">The sender of the event</param>
    ''' <param name="e">The event args</param>
    Private Sub btnNewPool_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNewPool.Click
        Try
            Dim n As TreeNode = InsertNewPoolIntoTree()
            trvResourcePools.LabelEdit = True
            n.BeginEdit()
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlResourcePools_UnexpectedError0, ex.Message), ex)
        Finally
            UpdatePoolButtons()
        End Try
    End Sub

    ''' <summary>
    ''' Inserts a new pool in to the pools treeview.
    ''' </summary>
    ''' <returns>The newly inselrted node</returns>
    Private Function InsertNewPoolIntoTree() As TreeNode
        Dim stub As String = My.Resources.ctlResourcePools_Pool
        Dim index As Integer = 0
        Do
            index += 1
        Loop While PoolExistsInTree(stub & index.ToString)

        Dim newName As String = stub & index.ToString

        Dim node As New TreeNode(newName, 0, 0)
        Me.trvResourcePools.Nodes.Add(node)
        Me.trvResourcePools.SelectedNode = node

        Return node
    End Function

    ''' <summary>
    ''' Determines whether a pool group already exists in the UI by inspecting
    ''' the treeview.
    ''' </summary>
    ''' <param name="poolName">The name of the pool to be tested.</param>
    ''' <returns>Returns true if the pool exists in the UI.</returns>
    Private Function PoolExistsInTree(ByVal poolName As String) As Boolean
        For Each node As TreeNode In Me.trvResourcePools.Nodes
            If node.Text = poolName Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' Updates the buttons near the resource pools treeview. In particular makes
    ''' them enabled/disabled according to context.
    ''' </summary>
    Private Sub UpdatePoolButtons()
        Dim nodeSelected As Boolean = Me.trvResourcePools.SelectedNode IsNot Nothing
        Dim selectedNodeIsTopLevel As Boolean = nodeSelected AndAlso Me.trvResourcePools.SelectedNode.Parent Is Nothing
        Me.btnNewPool.Enabled = True
        Me.btnRemoveFromPool.Enabled = nodeSelected AndAlso Not selectedNodeIsTopLevel
        Me.btnDeletePool.Enabled = nodeSelected AndAlso selectedNodeIsTopLevel
    End Sub

    ''' <summary>
    ''' Handles the AfterSelect event of the Resouce Pools Treeview
    ''' </summary>
    ''' <param name="sender">The sender of the event</param>
    ''' <param name="e">The event args</param>
    Private Sub trvResourcePools_AfterSelect(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles trvResourcePools.AfterSelect
        Try
            Me.UpdatePoolButtons()
        Catch
            'do nothing
        End Try
    End Sub

    ''' <summary>
    ''' Handles the MouseUp event of the Resource Pools Treeview
    ''' </summary>
    ''' <param name="sender">The sender of the event</param>
    ''' <param name="e">The event args</param>
    Private Sub trvResourcePools_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles trvResourcePools.MouseUp
        Try
            If e.Button = System.Windows.Forms.MouseButtons.Right Then
                Dim targetNode As TreeNode = trvResourcePools.GetNodeAt(e.X, e.Y)
                If targetNode IsNot Nothing Then
                    trvResourcePools.SelectedNode = targetNode
                    trvResourcePools.ContextMenuStrip = BuildPoolsTreeContextMenu()
                    trvResourcePools.ContextMenuStrip.Show(trvResourcePools, e.X, e.Y)
                End If
            End If
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlResourcePools_UnexpectedError0, ex.Message), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Builds a context menu with items enabled or disabled based on the currently selected node.
    ''' </summary>
    ''' <returns>The context menu strip</returns>
    Private Function BuildPoolsTreeContextMenu() As ContextMenuStrip
        Dim menu As New ContextMenuStrip

        Dim currentItem As TreeNode = trvResourcePools.SelectedNode

        Dim item As ToolStripItem
        item = menu.Items.Add(My.Resources.ctlResourcePools_Delete, ToolImages.Delete_Red_16x16, AddressOf OnDeleteClick)
        item.Enabled = currentItem IsNot Nothing AndAlso currentItem.Parent Is Nothing

        item = menu.Items.Add(My.Resources.ctlResourcePools_Remove, ToolImages.Undo_16x16, AddressOf OnRemoveClick)
        item.Enabled = currentItem IsNot Nothing AndAlso currentItem.Parent IsNot Nothing

        Return menu
    End Function

    Private Sub ctlResourcePools_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        RefreshView()
    End Sub

    ''' <summary>
    ''' Handles the click event of the remove item in the context menu
    ''' </summary>
    ''' <param name="sender">The sender of the event</param>
    ''' <param name="e">The event args</param>
    Public Sub OnRemoveClick(ByVal sender As Object, ByVal e As EventArgs)
        RemoveSeletctedResource()
    End Sub

    ''' <summary>
    ''' Handles the click event of the delete item in the context menu
    ''' </summary>
    ''' <param name="sender">The sender of the event</param>
    ''' <param name="e">The event args</param>
    Public Sub OnDeleteClick(ByVal sender As Object, ByVal e As EventArgs)
        DeleteSelectedPool()
    End Sub

    ''' <summary>
    ''' Popultates the list of available resources and the resource pools treeview
    ''' done at the same time to save two trips to the db.
    ''' </summary>
    Private Sub PopulateAvailableResourcesAndResourcePools()
        Try
            ' cache the expanded [pool] ids. Cast to guid is unncessary
            Dim expanded =
                trvResourcePools.Nodes.Cast(Of TreeNode).
                Where(Function(x) x.IsExpanded).
                Select(Function(x) x.Tag).
                ToList()

            trvResourcePools.BeginUpdate()

            'Clear existing items
            trvResourcePools.Nodes.Clear()


            Dim dt As DataTable
            dt = gSv.GetResources(ResourceAttribute.None, ResourceAttribute.Debug Or ResourceAttribute.Retired, Nothing)
            For Each r As DataRow In dt.Rows
                If (CInt(r("AttributeID")) And ResourceAttribute.Pool) > 0 Then
                    Dim item As New TreeNode With {
                        .Text = CStr(r("name")),
                        .Tag = CType(r("resourceid"), Guid)
                    }

                    trvResourcePools.Nodes.Add(item)
                End If
            Next

            lstPoolsAvailableResources.BeginUpdate()

            'Clear existing items
            lstPoolsAvailableResources.Items.Clear()

            For Each r As DataRow In dt.Rows
                If (CInt(r("AttributeID")) And ResourceAttribute.Pool) = 0 Then
                    If Not IsDBNull(r("pool")) Then 'we only want the machines that are in a pool
                        Dim poolid As Guid = CType(r("pool"), Guid)
                        For Each n As TreeNode In trvResourcePools.Nodes
                            If poolid.Equals(n.Tag) Then
                                Dim item As New TreeNode With {
                                    .Text = CStr(r("name"))
                                }
                                n.Nodes.Add(item)
                                Exit For
                            End If
                        Next
                    Else
                        Dim item As New ListViewItem(CStr(r("name")), 0)
                        lstPoolsAvailableResources.Items.Add(item)
                    End If
                End If
            Next

            lstPoolsAvailableResources.EndUpdate()
            trvResourcePools.EndUpdate()

            ' Re-expand nodes. This has to be done after .EndUpdate()
            trvResourcePools.Nodes.Cast(Of TreeNode).
                ToList().
                ForEach(Sub(x) If expanded.Contains(x.Tag) Then x.Expand())

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlResourcePools_UnexpectedError0, ex.Message), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Handles the AfterLabelEdit event of the Resource Pools Treeview
    ''' </summary>
    ''' <param name="sender">The sender of the event</param>
    ''' <param name="e">The event args</param>
    Private Sub trvResourcePools_AfterLabelEdit(ByVal sender As Object, ByVal e As System.Windows.Forms.NodeLabelEditEventArgs) Handles trvResourcePools.AfterLabelEdit
        Try
            Dim newName As String
            If Not e.Label Is Nothing Then
                newName = e.Label 'If the user edited the label its value will be in here
            Else
                newName = e.Node.Text 'Otherwise it will be in here.
            End If
            gSv.CreateResourcePool(newName)
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlResourcePools_UnexpectedError0, ex.Message), ex)
            e.CancelEdit = True
            e.Node.Remove()
        Finally
            trvResourcePools.LabelEdit = False
        End Try
    End Sub

    ''' <summary>
    ''' Handles the Delete Pool buttons click event.
    ''' </summary>
    ''' <param name="sender">The sender of the event</param>
    ''' <param name="e">The event args</param>
    Private Sub btnDeletePool_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDeletePool.Click
        DeleteSelectedPool()
    End Sub

    ''' <summary>
    ''' Deletes the currently selected group node, if any.
    ''' </summary>
    Private Sub DeleteSelectedPool()
        Try
            Dim selected As TreeNode = Me.trvResourcePools.SelectedNode
            If selected IsNot Nothing Then
                If selected.Parent Is Nothing Then
                    gSv.DeleteResourcePool(selected.Text)
                    selected.Remove()

                    For Each node As TreeNode In selected.Nodes
                        Dim item As New ListViewItem(node.Text, 0)
                        lstPoolsAvailableResources.Items.Add(item)
                    Next
                Else
                    UserMessage.Show(My.Resources.ctlResourcePools_CanOnlyDeleteTopLevelNodesRepresentingGroups)
                End If
            Else
                UserMessage.Show(My.Resources.ctlResourcePools_PleaseFirstSelectAGroupToBeDeleted)
            End If
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlResourcePools_UnexpectedError0, ex.Message), ex)
        Finally
            UpdatePoolButtons()
        End Try
    End Sub



    ''' <summary>
    ''' Handles the DragOver event for the Resource Pools Treeview 
    ''' </summary>
    ''' <param name="sender">The sender of the event</param>
    ''' <param name="e">The event args</param>
    Private Sub trvResourcePools_DragOver(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles trvResourcePools.DragOver
        Try
            If e.Data.GetDataPresent(GetType(List(Of String))) Then
                e.Effect = DragDropEffects.Move
            End If
        Catch
            e.Effect = DragDropEffects.None
        End Try
    End Sub

    ''' <summary>
    ''' Handles the DragDrop event for the Resource Pools Treeview
    ''' </summary>
    ''' <param name="sender">The sender of the event</param>
    ''' <param name="e">The event args</param>
    Private Sub trvResourcePools_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles trvResourcePools.DragDrop
        Try
            e.Effect = DragDropEffects.None 'Set to none until successful

            If e.Data.GetDataPresent(GetType(List(Of String))) Then
                Dim resources As List(Of String) = TryCast(e.Data.GetData(GetType(List(Of String))), List(Of String))

                Dim screenLocation As Point = New Point(e.X, e.Y)
                Dim localLocation As Point = Me.trvResourcePools.PointToClient(screenLocation)
                Dim targetNode As TreeNode = trvResourcePools.GetNodeAt(localLocation)

                If Not targetNode Is Nothing AndAlso targetNode.Parent Is Nothing Then

                    For Each res As String In resources
                        If mMovingResource Then gSv.RemoveResourceFromPool(res)
                        gSv.AddResourceToPool(targetNode.Text, res)
                        Dim node As New TreeNode(res)
                        targetNode.Nodes.Add(node)
                        e.Effect = DragDropEffects.Move
                    Next
                    targetNode.Expand()
                End If
            End If
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlResourcePools_UnexpectedError0, ex.Message), ex)
        Finally
            UpdatePoolButtons()
        End Try
    End Sub

    ''' <summary>
    ''' Handles the ItemDrag event of the Available Resources Listview
    ''' </summary>
    ''' <param name="sender">The sender of the event</param>
    ''' <param name="e">The event args</param>
    Private Sub lstPoolsAvailableResources_ItemDrag(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemDragEventArgs) Handles lstPoolsAvailableResources.ItemDrag
        DoItemDrag(lstPoolsAvailableResources)
    End Sub



    ''' <summary>
    ''' Holds a Boolean indicating whether we are moving a resource from one pool to another
    ''' </summary>
    Private mMovingResource As Boolean = False

    ''' <summary>
    ''' Handles the ItemDrag event of the Resource Pools Treeview
    ''' </summary>
    ''' <param name="sender">The sender of the event</param>
    ''' <param name="e">The event args</param>
    Private Sub trvResourcePools_ItemDrag(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemDragEventArgs) Handles trvResourcePools.ItemDrag
        Dim item As TreeNode = CType(e.Item, TreeNode)
        If Not item Is Nothing Then
            If Not item.Parent Is Nothing Then

                Dim resources As New List(Of String)
                resources.Add(item.Text)
                mMovingResource = True
                If lstPoolsAvailableResources.DoDragDrop(resources, DragDropEffects.Move) = DragDropEffects.Move Then
                    item.Remove()
                End If
                mMovingResource = False
            End If
        End If
    End Sub

    ''' <summary>
    ''' Removes the selected resource from the pool
    ''' </summary>
    Private Sub RemoveSeletctedResource()
        Try
            Dim selected As TreeNode = Me.trvResourcePools.SelectedNode
            If selected IsNot Nothing Then
                If Not selected.Parent Is Nothing Then
                    Dim resName As String = selected.Text
                    Dim parentName As String = selected.Parent.Text
                    gSv.RemoveResourceFromPool(resName)
                    selected.Remove()
                    Dim item As New ListViewItem(selected.Text, 0)
                    lstPoolsAvailableResources.Items.Add(item)
                Else
                    UserMessage.Show(My.Resources.ctlResourcePools_CanOnlyRemoveResourceFromAPool)
                End If
            End If

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlResourcePools_UnexpectedError0, ex.Message), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Handles the Resize event of the ResourcePools TabPage
    ''' </summary>
    ''' <param name="sender">The sender of the event</param>
    ''' <param name="e">The event args</param>
    Private Sub ctlResourcePools_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        Try
            Me.SuspendLayout()

            'Layout pools view
            Dim rightMargin As Integer = Me.ClientSize.Width - Me.btnRemoveFromPool.Right
            Dim leftMargin As Integer = Me.lstPoolsAvailableResources.Left
            Dim middleMargin As Integer = 4
            Dim minRHWidth As Integer = Me.btnNewPool.Width + Me.btnRemoveFromPool.Width + Me.btnDeletePool.Width + 2 * middleMargin
            Dim availWidth As Integer = (Me.ClientSize.Width - leftMargin - rightMargin - middleMargin)
            Me.lstPoolsAvailableResources.Width = Math.Min(availWidth \ 2, Me.ClientSize.Width - minRHWidth - rightMargin - middleMargin - leftMargin)
            Me.trvResourcePools.Left = Me.lstPoolsAvailableResources.Right + middleMargin
            Me.lblResourcePools.Left = Me.trvResourcePools.Left
            Me.trvResourcePools.Width = Me.btnRemoveFromPool.Right - Me.trvResourcePools.Left
            Me.btnNewPool.Left = Me.trvResourcePools.Left
            Me.btnDeletePool.Left = Me.btnNewPool.Right + middleMargin
            Me.btnNewPool.Top = Me.btnRemoveFromPool.Top
            Me.btnDeletePool.Top = Me.btnRemoveFromPool.Top
        Finally
            Me.ResumeLayout(True)
        End Try
    End Sub

    ''' <summary>
    ''' Handles the Click event of the Remove from pool Button
    ''' </summary>
    ''' <param name="sender">The sender of the event</param>
    ''' <param name="e">The event args</param>
    Private Sub btnRemoveFromPool_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveFromPool.Click
        RemoveSeletctedResource()
    End Sub

    Private Sub DoItemDrag(ByVal listView As ListView)
        If listView.SelectedItems.Count > 0 Then
            Dim resources As New List(Of String)
            Dim resourcesToRemove As New List(Of ListViewItem)
            For Each it As ListViewItem In listView.SelectedItems
                resources.Add(it.Text)
                resourcesToRemove.Add(it)
            Next

            If listView.DoDragDrop(resources, DragDropEffects.Move) = DragDropEffects.Move Then
                For Each res As ListViewItem In resourcesToRemove
                    res.Remove()
                Next
            End If
        End If
    End Sub

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public ReadOnly Property RequiredPermissions() As System.Collections.Generic.ICollection(Of BluePrism.AutomateAppCore.Auth.Permission) Implements BluePrism.AutomateAppCore.Auth.IPermission.RequiredPermissions
        Get
            Return Permission.ByName("Resources - Pools")
        End Get
    End Property

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "helpResourcePools.htm"
    End Function

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        Return True
    End Function

    Public Sub RefreshView() Implements IRefreshable.RefreshView
        UpdatePoolButtons()
        PopulateAvailableResourcesAndResourcePools()
    End Sub
End Class
