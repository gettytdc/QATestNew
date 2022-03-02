#If Nada Then

Imports System.Text.RegularExpressions
Imports BluePrism.AutomateAppCore

Public Class ctlBundler

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        mInputTree.ShowComponents = ctlComponentTree_OLD.ShowIf.All
        mOutputTree.ShowComponents = ctlComponentTree_OLD.ShowIf.None

    End Sub

    ' The map of root treenodes against their key representation
    Private mRootNodeMap As IDictionary(Of String, TreeNode)

    ''' <summary>
    ''' The output components currently registered within this bundler
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OutputComponents() As ICollection(Of PackageComponent)
        Get
            Return mOutputTree.Components
        End Get
        Set(ByVal value As ICollection(Of PackageComponent))
            mOutputTree.Components = value
        End Set
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' The root nodes in either tree keyed against what they represent, in the form
    ''' of "{treeview}.{model}" where {treeview} is one of "input" or "output", and
    ''' {model} is one of "snapshot", "process", "object", "model", "queue",
    ''' "schedule" or "calendar".
    ''' The value in the dictionary is the treenode corresponding to the associated
    ''' key.
    ''' </summary>
    ''' -----------------------------------------------------------------------------
    Private ReadOnly Property RootNodes() As IDictionary(Of String, TreeNode)
        Get
            If mRootNodeMap Is Nothing Then mRootNodeMap = New Dictionary(Of String, TreeNode)
            Return mRootNodeMap
        End Get
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Clears any entries in the root node map whose key matches the given regular
    ''' expression.
    ''' </summary>
    ''' <param name="rx">The regular expression used to determine which entries in
    ''' the root node map to clear</param>
    ''' -----------------------------------------------------------------------------
    Private Sub ClearRootNodeMap(ByVal rx As Regex)
        ' Shortcut outta here.
        If mRootNodeMap Is Nothing OrElse mRootNodeMap.Count = 0 Then Return

        Dim forDeletion As New List(Of String) ' where we store keys to delete
        For Each key As String In RootNodes.Keys
            If rx.IsMatch(key) Then forDeletion.Add(key)
        Next
        For Each key As String In forDeletion
            mRootNodeMap.Remove(key)
        Next
    End Sub

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Adds a node with the given name and image key to the specified collection.
    ''' </summary>
    ''' <param name="nodes">The node collection to which the node is added.</param>
    ''' <param name="name">The name and key of the node to add</param>
    ''' <param name="imageKey">The key to use to identify the image and the selected
    ''' image for the node.</param>
    ''' <returns>The node created as a result of this call.</returns>
    ''' -----------------------------------------------------------------------------
    Private Function AddNode( _
     ByVal nodes As TreeNodeCollection, ByVal name As String, ByVal imageKey As String) As TreeNode
        If imageKey Is Nothing Then
            Return nodes.Add(name, name)
        Else
            Return nodes.Add(name, name, imageKey, imageKey)
        End If

    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Creates the root nodes on the given tree view, their root node map keys
    ''' prefixed with the given value.
    ''' </summary>
    ''' <param name="view">The treeview on which the nodes should be created.
    ''' </param>
    ''' <param name="prefix">The prefix for the nodes to use to store the references
    ''' in the <see cref="RootNodes">Root Node Map</see>.</param>
    ''' -----------------------------------------------------------------------------
    Private Sub CreateRootNodes(ByVal view As TreeView, ByVal prefix As String)
        ' First ensure that there's nothing on there
        If view.Nodes.Count > 0 Then
            view.Nodes.Clear()
            ClearRootNodeMap(New Regex("^" & Regex.Escape(prefix) & "\..*$"))
        End If

        Dim map As IDictionary(Of String, TreeNode) = RootNodes
        map(prefix & ".snapshot") = AddNode(view.Nodes, "Snapshots", "snapshot")
        map(prefix & ".process") = AddNode(view.Nodes, "Business Processes", "process")
        map(prefix & ".object") = AddNode(view.Nodes, "Business Objects", "object")
        map(prefix & ".model") = AddNode(view.Nodes, "Application Models", "model")
        map(prefix & ".queue") = AddNode(view.Nodes, "Work Queues", "queue")
        map(prefix & ".schedule") = AddNode(view.Nodes, "Schedules", "schedule")
        map(prefix & ".calendar") = AddNode(view.Nodes, "Calendars", "calendar")

    End Sub

End Class

#End If