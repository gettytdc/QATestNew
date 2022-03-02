Imports BluePrism.AutomateAppCore
Imports BluePrism.Images
Imports Internationalisation

Public Class ctlPackageTree

#Region " Class scope declarations "

    ''' <summary>
    ''' Event fired when the overview node is selected.
    ''' </summary>
    ''' <param name="sender">The tree from which the event was fired.</param>
    ''' <param name="packages">The collection of packages represented by the node
    ''' that was selected.</param>
    Public Event OverviewSelected(ByVal sender As Object, ByVal packages As ICollection(Of clsPackage))

    ''' <summary>
    ''' Event fired when a package node is selected
    ''' </summary>
    ''' <param name="sender">The tree from which the event was fired.</param>
    ''' <param name="pkg">The package that was selected.</param>
    Public Event PackageSelected(ByVal sender As Object, ByVal pkg As clsPackage)

    ''' <summary>
    ''' Event fired when a release node is selected.
    ''' </summary>
    ''' <param name="sender">The tree from which the event was fired.</param>
    ''' <param name="rel">The release that was selected.</param>
    Public Event ReleaseSelected(ByVal sender As Object, ByVal rel As clsRelease)

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new package tree.
    ''' </summary>
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        tvPackages.ImageList = ImageLists.Components_16x16
        AddNode(tvPackages.Nodes, ResMan.GetString("ctlPackageTree_tv_package_overview"), PackageComponentType.Package.Key)

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The packages being displayed in this package tree
    ''' </summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Packages() As ICollection(Of clsPackage)
        Get
            Return DirectCast(Root.Tag, ICollection(Of clsPackage))
        End Get
        Set(ByVal value As ICollection(Of clsPackage))
            If value Is Nothing Then Throw New ArgumentNullException(NameOf(value))
            Dim root As TreeNode = Me.Root
            root.Nodes.Clear()
            root.Tag = value
            For Each pkg As clsPackage In value
                With AddNode(root.Nodes, pkg.Name, pkg.TypeKey)
                    .Tag = pkg
                    For Each rel As clsRelease In pkg.Releases
                        AddNode(.Nodes, rel.Name, rel.TypeKey).Tag = rel
                    Next
                End With
            Next
            root.Expand()
        End Set
    End Property

    ''' <summary>
    ''' The currently selected package.
    ''' </summary>
    ''' <value>The package to select. If the package was not found in this tree
    ''' then setting it will have no effect.</value>
    ''' <returns>The currently selected package, or null if there is no node
    ''' selected or if the currently selected node does not represent a package.
    ''' </returns>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Property SelectedPackage() As clsPackage
        Get
            Dim n As TreeNode = tvPackages.SelectedNode
            If n Is Nothing Then Return Nothing Else Return TryCast(n.Tag, clsPackage)
        End Get
        Set(ByVal value As clsPackage)
            Dim n As TreeNode = FindNode(value)
            If n IsNot Nothing Then tvPackages.SelectedNode = n
        End Set
    End Property

    ''' <summary>
    ''' The currently selected release.
    ''' </summary>
    ''' <value>The release to select. If the release was not found in this tree
    ''' then setting it will have no effect.</value>
    ''' <returns>The currently selected release, or null if there is no node
    ''' selected or if the currently selected node does not represent a release.
    ''' </returns>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Property SelectedRelease() As clsRelease
        Get
            Dim n As TreeNode = tvPackages.SelectedNode
            If n Is Nothing Then Return Nothing Else Return TryCast(n.Tag, clsRelease)
        End Get
        Set(ByVal value As clsRelease)
            Dim n As TreeNode = FindNode(value)
            If n IsNot Nothing Then tvPackages.SelectedNode = n
        End Set
    End Property

    ''' <summary>
    ''' Flag indicating if the overview is selected or not.
    ''' Note that setting this to false has no effect - setting it to true will
    ''' ensure that the overview node is selected.
    ''' </summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Property SelectedOverview() As Boolean
        Get
            Return (Root Is tvPackages.SelectedNode)
        End Get
        Set(ByVal value As Boolean)
            If value Then tvPackages.SelectedNode = Root
        End Set
    End Property

    ''' <summary>
    ''' The root node of this package tree
    ''' </summary>
    Protected ReadOnly Property Root() As TreeNode
        Get
            Return tvPackages.Nodes(0)
        End Get
    End Property

#End Region

#Region " Public Methods "

    ''' <summary>
    ''' Updates a package in this tree with a replacement object.
    ''' </summary>
    ''' <param name="all">The collection of packages to store within this object,
    ''' maintaining reference equality with all the packages held in the release
    ''' manager, and in package details controls.</param>
    ''' <param name="id">The ID of the package to replace.</param>
    ''' <param name="newPkg">The package to replace the existing object with. If this
    ''' is null, it is assumed that the package should be removed from this tree (if
    ''' it exists).</param>
    Public Sub UpdatePackage( _
     ByVal all As ICollection(Of clsPackage), ByVal id As Integer, ByVal newPkg As clsPackage)

        ' Find the old package and replace it.
        Dim n As TreeNode = FindPackageNode(id)
        ' If "newPkg" is null, that means the package must be removed.
        If newPkg Is Nothing Then
            If n IsNot Nothing Then ' If it's not there, do nothing
                If n.IsSelected Then
                    If n.NextNode IsNot Nothing Then
                        tvPackages.SelectedNode = n.NextNode
                    ElseIf n.PrevNode IsNot Nothing Then
                        tvPackages.SelectedNode = n.PrevNode
                    End If
                End If
                n.Remove()
            End If
            Return
        End If
        ' If we're here, it means we actually have a package to set in the tree,
        ' either a new one or an updated one.
        Dim root As TreeNode = Me.Root
        If n Is Nothing Then
            ' It's a new package
            n = AddNode(root.Nodes, newPkg.Name, newPkg.TypeKey)
            ' Add any required releases
            For Each rel As clsRelease In newPkg.Releases
                AddNode(n.Nodes, rel.Name, rel.TypeKey).Tag = rel
            Next
        Else
            n.Text = newPkg.Name
            ' Check the releases
            If n.Nodes.Count < newPkg.Releases.Count Then
                For Each rel As clsRelease In newPkg.Releases
                    If FindNode(n.Nodes, rel) Is Nothing Then
                        AddNode(n.Nodes, rel.Name, rel.TypeKey).Tag = rel
                    End If
                Next
                If Not n.IsExpanded Then n.Expand() ' Make sure the releases are expanded
            End If
        End If
        n.Tag = newPkg
        root.Tag = all
        If Not root.IsExpanded Then root.Expand() ' Make sure the root is expanded
    End Sub


#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Adds a node with the given name and image key to the specified node
    ''' collection. Note that this will set both the name and title to the given
    ''' name, and selected and unselected image key to the given image key.
    ''' </summary>
    ''' <param name="nodes">The node collection to add to</param>
    ''' <param name="name">The name / title of the node to add.</param>
    ''' <param name="imageKey">The image key to add</param>
    ''' <returns>The node after being added to the given collection.</returns>
    Private Function AddNode(ByVal nodes As TreeNodeCollection, ByVal name As String, ByVal imageKey As String) As TreeNode
        Return nodes.Add(name, name, imageKey, imageKey)
    End Function

    ''' <summary>
    ''' Finds the node in this tree with the given tag.
    ''' </summary>
    ''' <param name="tag">The tag to search for. This will check each tag to see if
    ''' it is equal (according to <see cref="System.Object.Equals"/>) and return the
    ''' corresponding node, or null if no such tag was found.</param>
    ''' <returns>The node from this control which has the given tag, or null if no
    ''' such node was found.</returns>
    Private Function FindNode(ByVal tag As Object) As TreeNode
        Return FindNode(tvPackages.Nodes, tag)
    End Function

    ''' <summary>
    ''' Depth-first search for the node in the given node collection with the
    ''' specified tag.
    ''' </summary>
    ''' <param name="nodes">The node collection to search.</param>
    ''' <param name="tag">The tag to search for. This will check each tag to see if
    ''' it is equal (according to <see cref="System.Object.Equals"/>) and return the
    ''' corresponding node, or null if no such tag was found.</param>
    ''' <returns>The node from the given node collection, or its sub-nodes which has
    ''' the given tag, or null if no such node was found.</returns>
    Private Function FindNode(ByVal nodes As TreeNodeCollection, ByVal tag As Object) As TreeNode
        For Each n As TreeNode In nodes
            If Object.Equals(tag, n.Tag) Then Return n
            If n.Nodes.Count > 0 Then
                Dim subNode As TreeNode = FindNode(n.Nodes, tag)
                If subNode IsNot Nothing Then Return subNode
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Finds the treenode which represents the package with the given ID, or null if
    ''' no such node was found.
    ''' </summary>
    ''' <param name="id">The package ID to search for.</param>
    ''' <returns>The node representing the given package, or null if no node
    ''' representing a package with the specified ID was found.</returns>
    Private Function FindPackageNode(ByVal id As Integer) As TreeNode
        For Each n As TreeNode In Root.Nodes
            Dim pkg As clsPackage = DirectCast(n.Tag, clsPackage)
            If pkg.IdAsInteger = id Then Return n
        Next
        Return Nothing
    End Function

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Handles a node being selected
    ''' </summary>
    Private Sub HandleNodeSelected(ByVal sender As Object, ByVal e As TreeViewEventArgs) _
     Handles tvPackages.AfterSelect

        Dim n As TreeNode = e.Node

        ' ...is it a package?
        Dim pkg As clsPackage = TryCast(n.Tag, clsPackage)
        If pkg IsNot Nothing Then RaiseEvent PackageSelected(Me, pkg) : Return

        ' ...is it a release?
        Dim rel As clsRelease = TryCast(n.Tag, clsRelease)
        If rel IsNot Nothing Then RaiseEvent ReleaseSelected(Me, rel) : Return

        ' then it must be the overview...
        Dim pkgs As ICollection(Of clsPackage) = TryCast(n.Tag, ICollection(Of clsPackage))
        If pkgs IsNot Nothing Then RaiseEvent OverviewSelected(Me, pkgs) : Return

        ' If we're here, then I have no idea what's been selected...
        If Not DesignMode Then ' This can happen (validly) in design mode
            Debug.Fail("Node selected with no known tag", _
             String.Format("Unknown tag for node: {0}; Tag: {1}", n.Text, n.Tag) _
            )
        End If

    End Sub

    ''' <summary>
    ''' Handles this control being loaded. This just ensures that the overview is
    ''' selected on load of this control
    ''' </summary>
    ''' <param name="e"></param>
    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        SelectedOverview = True
    End Sub


#End Region

End Class
