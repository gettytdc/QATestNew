Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AMI
Imports BluePrism.AMI.clsAMI
Imports AppAMI = BluePrism.ApplicationManager.AMI
Imports AppTypes = BluePrism.ApplicationManager.AMI.clsElementTypeInfo.AppTypes
Imports AutomateControls
Imports AutomateControls.Trees
Imports AutomateControls.Forms

''' <summary>
''' Form used to navigate the application model tree for an attached application
''' </summary>
Friend Class frmApplicationTreeNavigator
    Inherits frmForm

#Region " Published Events "

    ''' <summary>
    ''' Event fired when an element has been chosen <em>and applied</em>
    ''' </summary>
    Public Event ElementChosen As ElementChosenEventHandler

    ''' <summary>
    ''' Event fired when the user has selected an element to open in the region
    ''' editor
    ''' </summary>
    Public Event RegionEditorRequest As RegionEditorRequestEventHandler

#End Region
#Region " Member Variables "

    ' The element selected in the application model tree
    Private mCurrentModelElement As clsElement

    ' A highlighter window used to highlight the current element in the model tree
    Private mHighlighter As New HighlighterWindow()

    ' A cache of screen bounds for a given element
    Private mRectangleCache As New Dictionary(Of clsElement, Rectangle)

    ' The path of the last selected treenode - set when the tree is refreshing
    Private mLastSelectedNodePath As String

    ' The last tree retrieved by this form or set on it externally
    Private mElements As ICollection(Of clsElement)

    ' The filterable treeview held within the model's 'treeview and filter' control
    Private WithEvents trvElementTree As FilterableTreeView

    ' The width of the collapsed panel at the point it was collapsed
    Private mSavedPanelSize As Integer
#End Region

#Region " Constructors / Destructors "

    ''' <summary>
    ''' Creates a new detached application navigator.
    ''' </summary>
    Public Sub New()
        Me.New(Nothing, Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new application navigator using the given AMI instance to connect
    ''' to the application of the specified type.
    ''' </summary>
    ''' <param name="ami">The AMI instance which is connected to the application
    ''' being explored</param>
    ''' <param name="appType">The application type ID, used to tell AMI how to
    ''' approach the snapshot</param>
    ''' <param name="elemTree">The tree to display initially in this form; null to
    ''' retrieve the tree from AMI when the form is loaded.</param>
    ''' <remarks>None of these arguments are expected if in DesignMode - ie. if
    ''' opening this form within the windows forms designer</remarks>
    Public Sub New(ByVal ami As clsAMI, ByVal appType As String,
     ByVal elemTree As ICollection(Of clsElement))

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        ' Set the node filter to test the identifiers in the element
        filTreeElements.Filterer = AddressOf IsNodeMatch

        ' Capture tree events so we can change the right panel on selection
        trvElementTree = filTreeElements.Tree

        ' Set the other memvars as necessary
        Me.AMI = ami
        ApplicationType = appType
        mElements = elemTree

    End Sub

    ''' <summary>
    ''' Tests to see if the given node matches the search term, testing its text
    ''' and then all of its identifier values to see if there is a value with the
    ''' given term within it (case insensitive)
    ''' </summary>
    ''' <param name="term">The search term to test with</param>
    ''' <param name="node">The node representing the element to be tested</param>
    ''' <returns>True if the given node is match for the search term; False
    ''' otherwise.</returns>
    Private Function IsNodeMatch(ByVal term As String, ByVal node As TreeNode) _
     As Boolean

        ' First test - the node text itself
        If BPUtil.IsMatch(node.Text, term, True, False) Then Return True

        ' Now we go through the properties and test them
        ' First get the element
        Dim elem As clsElement = TryCast(node.Tag, clsElement)
        If elem Is Nothing Then Return False

        ' Now test each of the properties in turn.
        For Each id As AppAMI.clsIdentifierInfo In elem.Identifiers.Values
            If BPUtil.IsMatch(id.Value, term, True, False) Then Return True
        Next

        Return False

    End Function

    ''' <summary>
    ''' Disposes of this form and anything else registered on it
    ''' </summary>
    ''' <param name="disposing">True if disposing explicitly, false otherwise</param>
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        MyBase.Dispose(disposing)
        If disposing Then
            mWorker.Dispose()
            trvElementTree = Nothing
            If components IsNot Nothing Then components.Dispose()
            If mHighlighter IsNot Nothing Then mHighlighter.Dispose()
        End If
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The AMI instance that this navigator is connected to
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property AMI As clsAMI

    ''' <summary>
    ''' The ID of the application type being navigated by this form
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ApplicationType As String

    ''' <summary>
    ''' Gets or sets whether the user is able to alter the UI at all - the Cancel
    ''' button is not affected by changing this value.
    ''' </summary>
    Private Property UIEnabled() As Boolean
        Get
            Return (btnRefresh.Enabled AndAlso btnOK.Enabled)
        End Get
        Set(ByVal value As Boolean)
            btnRefresh.Enabled = value
            btnOK.Enabled = value
            cbShowInvisible.Enabled = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the selected element in this navigator
    ''' </summary>
    Private Property SelectedElement() As clsElement
        Get
            Dim n As TreeNode = trvElementTree.SelectedNode
            If n IsNot Nothing Then Return TryCast(n.Tag, clsElement)
            Return Nothing
        End Get
        Set(ByVal value As clsElement)
            Dim n As TreeNode = trvElementTree.FindNodeWithTag(value)
            If n Is Nothing Then Return
            n.EnsureVisible()
            trvElementTree.SelectedNode = n
        End Set
    End Property

    ''' <summary>
    ''' The element tree represented within this navigator form
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ElementTree() As ICollection(Of clsElement)
        Get
            Return mElements
        End Get
        Set(ByVal value As ICollection(Of clsElement))
            mElements = value
            trvElementTree.BeginUpdate()
            Try
                trvElementTree.Nodes.Clear()
                If CollectionUtil.IsNullOrEmpty(value) Then Return

                Dim root As TreeNode = trvElementTree.Nodes.Add(My.Resources.Desktop)
                PopulateTree(root, value)
                trvElementTree.ExpandAll()
                Dim n As TreeNode = Nothing
                If mLastSelectedNodePath IsNot Nothing Then _
                 n = trvElementTree.FindNodeByPath(mLastSelectedNodePath)

                If n Is Nothing Then n = root

                trvElementTree.SelectedNode = n
                trvElementTree.TopNode = n

            Finally
                trvElementTree.EndUpdate()

            End Try
        End Set
    End Property

#End Region

#Region " Event Handling "

    ''' <summary>
    ''' Handles this form being shown by loading the element tree (or showing the
    ''' currently set one)
    ''' </summary>
    Protected Overrides Sub OnShown(ByVal e As EventArgs)
        MyBase.OnShown(e)
        If Not DesignMode Then
            If mElements Is Nothing Then RefreshTree() Else ElementTree = mElements
        End If
    End Sub

    ''' <summary>
    ''' Handles this form being closed
    ''' </summary>
    Protected Overrides Sub OnClosed(ByVal e As EventArgs)
        MyBase.OnClosed(e)
        ' Hide the highlighter window if it's there. Don't dispose just yet, however,
        ' the form might be reopened. It is disposed of in the Dispose() method.
        If mHighlighter IsNot Nothing Then mHighlighter.Visible = False
    End Sub

    ''' <summary>
    ''' Handles the 'Refresh' button being pressed.
    ''' </summary>
    Private Sub HandleRefreshTreeClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnRefresh.Click
        RefreshTree()
    End Sub

    ''' <summary>
    ''' Actually does the work of refreshing the application model tree
    ''' </summary>
    Private Sub DoRefreshTreeWork(
     ByVal sender As Object, ByVal e As DoWorkEventArgs) Handles mWorker.DoWork
        e.Result = AMI.GetElementTree(ApplicationType, cbShowInvisible.Checked,
         New BackgroundWorkerProgressMonitor(mWorker))
        ' If the worker was cancelled, ensure that the event args reflect that
        ' so we don't start trying to reference disposed controls
        If mWorker.CancellationPending Then e.Cancel = True
    End Sub

    ''' <summary>
    ''' Handles the completion of the refreshing of the application model; transfers
    ''' the model elements into the tree
    ''' </summary>
    Private Sub DoRefreshTreeComplete(
     ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) _
     Handles mWorker.RunWorkerCompleted

        ' If this form has been disposed while the background worker was running,
        ' no need to bother trying to update disposed controls
        If IsDisposed Then Return

        If e.Error IsNot Nothing Then UserMessage.Show(
         My.Resources.AnErrorOccurredRetrievingTheApplicationModel, e.Error)

        UIEnabled = True
        Cursor = Cursors.Default

        ' Cancel is already handled in the progress dialog's cancel event handler
        If e.Error IsNot Nothing OrElse e.Cancelled Then Return

        ElementTree = DirectCast(e.Result, ICollection(Of clsElement))

    End Sub

    ''' <summary>
    ''' Handles a node being selected in the element tree
    ''' </summary>
    Private Sub HandleElementTreeSelect(sender As Object, e As TreeViewEventArgs) Handles trvElementTree.AfterSelect

        Dim el As clsElement = Nothing
        If e.Node IsNot Nothing Then el = TryCast(e.Node.Tag, clsElement)

        mCurrentModelElement = el

        PopulateIdentifiers(el)

        If el Is Nothing Then
            mHighlighter.Visible = False
        ElseIf IsBrowserApplication(ApplicationType) Then
            el.Highlight()
        Else
            Dim bounds As Rectangle
            If Not mRectangleCache.TryGetValue(el, bounds) Then
                bounds = el.ScreenBounds
                mRectangleCache(el) = bounds
            End If
            mHighlighter.Visible = False
            If bounds = Rectangle.Empty Then Return

            mHighlighter.Visible = True
            mHighlighter.HighlightScreenRect = bounds
        End If

    End Sub

    ''' <summary>
    ''' Handles a right click event on a node in the element tree
    ''' </summary>
    Private Sub HandleTreeItemRightClick(
     ByVal sender As Object, ByVal e As TreeNodeMouseClickEventArgs) _
     Handles trvElementTree.NodeMouseClick
        ' Only right thinking mouse buttons handled here.
        If e.Button <> MouseButtons.Right Then Return

        Dim el As clsElement = Nothing
        If e.Node IsNot Nothing Then el = TryCast(e.Node.Tag, clsElement)

        ' Currently regions are only supported for Win32 elements
        If el.ElementType.AppType <> AppTypes.Win32 Then Return

        ' We set the element to use into the context menu so that we can identify
        ' it in the menu event handler (ensuring that it doesn't require the
        ' AfterSelect to have been performed first)
        ctxMenu.Tag = el
        ctxMenu.Show(trvElementTree, e.Location)

    End Sub

    ''' <summary>
    ''' Handles the committing of the currently selected node to the model
    ''' </summary>
    Private Sub HandleOKClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnOK.Click
        Dim el As clsElement = SelectedElement
        If el IsNot Nothing Then _
         OnElementChosen(New ElementChosenEventArgs(GetSingleton.ICollection(el)))

        DialogResult = DialogResult.OK
        Close()

    End Sub

    ''' <summary>
    ''' Handles the cancelling of the form
    ''' </summary>
    Private Sub HandleCancelClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnCancel.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    ''' <summary>
    ''' Raises the <see cref="ElementChosen"/> event.
    ''' </summary>
    Protected Overridable Sub OnElementChosen(
     ByVal e As ElementChosenEventArgs)
        RaiseEvent ElementChosen(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="RegionEditorRequest"/> event
    ''' </summary>
    Protected Overridable Sub OnRegionEditorRequest(
     ByVal e As RegionEditorRequestEventArgs)
        RaiseEvent RegionEditorRequest(Me, e)
    End Sub

    ''' <summary>
    ''' Handles the 'Show Invisible' checkbox value changing
    ''' </summary>
    Private Sub HandleShowInvisibleChanged(
     ByVal sender As Object, ByVal e As EventArgs) _
     Handles cbShowInvisible.CheckedChanged
        RefreshTree()
    End Sub

    ''' <summary>
    ''' Handles the collapse (/expand) toggle button being clicked
    ''' </summary>
    Private Sub HandleCollapse(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnCollapse.Click

        ' Don't perform any layout while we're messing around in here - it would
        ' judder all over the place if we let it.
        splitAppTree.SuspendLayout()

        Try
            ' If it's collapsed already, re-expand it to its former size; ensure
            ' that the current size of panel1 is maintained
            If splitAppTree.Panel2Collapsed Then
                ' Save the panel1 width
                Dim panel1Width As Integer = splitAppTree.Panel1.Width

                ' Increase the size of this form by the size of panel2 and the
                ' width of the splitter
                Width += mSavedPanelSize + splitAppTree.SplitterWidth
                ' Restore panel 2
                splitAppTree.Panel2Collapsed = False
                ' Restore the size of panel 1 to what it was before the change
                splitAppTree.SplitterDistance = panel1Width

                ' Toggle the direction of the arrow on the collapse toggle button
                btnCollapse.Direction = Direction.Left
                ttTip.SetToolTip(btnCollapse, My.Resources.HideAttributes)

            Else ' It's not collapse, so we collapse it, saving its width first

                ' Store the width of the collapsing panel
                mSavedPanelSize = splitAppTree.Panel2.Width
                ' Reduce that width from the size of this form
                Width -= mSavedPanelSize + splitAppTree.SplitterWidth
                ' Collapse the panel
                splitAppTree.Panel2Collapsed = True

                ' Toggle the direction of the arrow on the collapse toggle button
                btnCollapse.Direction = Direction.Right
                ttTip.SetToolTip(btnCollapse, My.Resources.ShowAttributes)

            End If

        Finally
            splitAppTree.ResumeLayout()

        End Try

    End Sub

    ''' <summary>
    ''' Handles a request to open an element in the region editor
    ''' </summary>
    Private Sub HandleOpenInRegionEditor(
     ByVal sender As Object, ByVal e As EventArgs) _
     Handles mnuRegionEditor.Click

        ' The element is stored in the context menu, so ensure it's there.
        Dim el As clsElement = TryCast(ctxMenu.Tag, clsElement)

        ' Ignore it if it's not set
        If el Is Nothing Then Return

        OnRegionEditorRequest(New RegionEditorRequestEventArgs(el))

        DialogResult = DialogResult.OK
        Close()

    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Refreshes the application model tree
    ''' </summary>
    Private Sub RefreshTree()
        UIEnabled = False
        Dim n As TreeNode = trvElementTree.SelectedNode
        If n Is Nothing _
         Then mLastSelectedNodePath = Nothing _
         Else mLastSelectedNodePath = n.FullPath

        trvElementTree.Nodes.Clear()
        gridAttrs.Rows.Clear()

        Me.Cursor = Cursors.WaitCursor
        Dim win As IWin32Window = Nothing
        Using pd As ProgressDialog = ProgressDialog.Prepare(
         Me, mWorker, My.Resources.frmApplicationTreeNavigator_PleaseWait, My.Resources.BuildingApplicationTree, win)
            pd.ProgressDisplayFunction = AddressOf CreateProgressLabel
            AddHandler pd.Cancel, AddressOf HandleProgressDialogCancel
            pd.ShowInTaskbar = False
            pd.ShowDialog(win)
        End Using

    End Sub

    ''' <summary>
    ''' Handles the progress dialog being cancelled by cancelling this form
    ''' </summary>
    Private Sub HandleProgressDialogCancel(
     ByVal sender As Object, ByVal e As EventArgs)
        UIEnabled = True
        Me.Cursor = Cursors.Default
        ElementTree = Nothing

        btnCancel.PerformClick()
    End Sub

    ''' <summary>
    ''' Creates a progress label from the given progress update data.
    ''' </summary>
    ''' <param name="data">The data returned in a progress update from the
    ''' background worker</param>
    ''' <returns>The label corresponding to the given progress data.</returns>
    Private Function CreateProgressLabel(ByVal data As Object) As String
        Return TryCast(data, String)
    End Function

    ''' <summary>
    ''' Recursively populates the given tree node with the elements in the given
    ''' collection
    ''' </summary>
    ''' <param name="t">The node whose children should be set from the given
    ''' collection of elements.</param>
    ''' <param name="children">The elements with which to populate the tree</param>
    Private Sub PopulateTree(
     ByVal t As TreeNode, ByVal children As ICollection(Of clsElement))
        For Each ce As clsElement In children
            Dim node As New TreeNode(ce.TypeLabel)
            node.Tag = ce
            t.Nodes.Add(node)
            PopulateTree(node, ce.Children)
        Next
    End Sub

    ''' <summary>
    ''' Populates the list of identifiers from the given element
    ''' </summary>
    ''' <param name="el">The element which should be used to populate the list of
    ''' attributes</param>
    Private Sub PopulateIdentifiers(ByVal el As clsElement)
        Dim row As DataGridViewRow = Nothing
        If gridAttrs.SelectedRows.Count > 0 Then row = gridAttrs.SelectedRows(0)
        Dim selectedAttr As String = ""
        If row IsNot Nothing Then selectedAttr = CStr(row.Cells(0).Value)
        Dim newSelectedRow As DataGridViewRow = Nothing

        Using WinUtil.LockDrawing(gridAttrs)
            gridAttrs.Rows.Clear()
            If el Is Nothing Then Return
            For Each id As AppAMI.clsIdentifierInfo In el.Identifiers.Values
                For Each val As String In id.Values
                    Dim ind As Integer =
                     gridAttrs.Rows.Add(id.FullyQualifiedName, val)
                    If newSelectedRow Is Nothing AndAlso id.Name = selectedAttr Then
                        newSelectedRow = gridAttrs.Rows(ind)
                        newSelectedRow.Selected = True
                    End If
                Next
            Next
            Dim dirn As Nullable(Of ListSortDirection)
            Select Case gridAttrs.SortOrder
                Case SortOrder.Ascending : dirn = ListSortDirection.Ascending
                Case SortOrder.Descending : dirn = ListSortDirection.Descending
            End Select
            If dirn.HasValue AndAlso gridAttrs.SortedColumn IsNot Nothing Then _
              gridAttrs.Sort(gridAttrs.SortedColumn, dirn.Value)

            If newSelectedRow IsNot Nothing Then _
             gridAttrs.CurrentCell = newSelectedRow.Cells(0)
        End Using
    End Sub

#End Region

End Class
