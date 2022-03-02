Imports BluePrism.Server.Domain.Models
Imports System.Runtime.Serialization

Namespace Groups

    ''' <summary>
    ''' Specialization of a group which acts as a root element for a tree
    ''' </summary>
    <Serializable()>
    <DataContract([Namespace]:="bp", IsReference:=True)>
    <KnownType(GetType(ObjectGroupMember))>
    Friend Class TreeRoot : Inherits Group

        ' The tree which this instance is a root of
        <DataMember()>
        Private mTree As GroupTree

        ''' <summary>
        ''' Creates a new root group for the given tree type
        ''' </summary>
        ''' <param name="gpTree">The tree for which this object represents the root.
        ''' </param>
        ''' <exception cref="ArgumentNullException">If <paramref name="gpTree"/> is
        ''' null.</exception>
        Public Sub New(gpTree As GroupTree)
            MyBase.New()
            If gpTree Is Nothing Then Throw New ArgumentNullException(NameOf(gpTree))
            Name = TreeDefinitionAttribute.GetPluralNameFor(gpTree.TreeType)
            mTree = gpTree
            Permissions = New MemberPermissions(Nothing)
        End Sub

        ''' <summary>
        ''' The tree for which this group is the root group, or null if it is not
        ''' registered with any tree
        ''' </summary>
        ''' <remarks>Note that this should always override the corresponding property
        ''' in <see cref="GroupMember"/> or a StackOverflowException might occur when
        ''' attempting to get the tree from a group member.</remarks>
        Public Overrides ReadOnly Property Tree As IGroupTree
            Get
                Return mTree
            End Get
        End Property

        ''' <summary>
        ''' Gets the image key associated with this root element.
        ''' If this root is attached to a tree, it will delegate to the image key in
        ''' the tree; otherwise, it will use the default image key for a group.
        ''' </summary>
        Public Overrides ReadOnly Property ImageKey As String
            Get
                If mTree IsNot Nothing Then Return mTree.ImageKey
                Return MyBase.ImageKey
            End Get
        End Property

        ''' <summary>
        ''' Gets the expanded image key associated with this root element.
        ''' If this root is attached to a tree, it will delegate to the image key in
        ''' the tree; otherwise, it will use the default expanded image key for a
        ''' group.
        ''' </summary>
        Public Overrides ReadOnly Property ImageKeyExpanded As String
            Get
                If mTree IsNot Nothing Then Return mTree.ImageKey
                Return MyBase.ImageKeyExpanded
            End Get
        End Property

        ''' <summary>
        ''' Overrides the parent ID property to ensure that the parent of this root
        ''' group is never set to anything
        ''' </summary>
        Public Overrides Property Owner As IGroup
            Get
                Return Nothing
            End Get
            Set(value As IGroup)
                If value IsNot Nothing Then Throw New InvalidArgumentException(
                    "Cannot set the parent ID of a tree root")
                MyBase.Owner = value
            End Set
        End Property

        ''' <summary>
        ''' Indicates if this group represents the root of a tree or not. Note that
        ''' a disassociated group (ie. a group with no owner) <em>is not</em> a root,
        ''' according to the rules of this property; a root of a tree is the group
        ''' that is returned when <see cref="GroupTree.Root"/> is called, and is a
        ''' specific type, not a group in a specific state.
        ''' </summary>
        ''' <returns>True</returns>
        Public Overrides ReadOnly Property IsRoot As Boolean
            Get
                Return True
            End Get
        End Property

        ''' <summary>
        ''' Moves this tree root to a different tree
        ''' </summary>
        ''' <param name="gt">The tree to move this root to</param>
        ''' <remarks>I would have made this a 'Friend' setter on the Tree property,
        ''' except you can't do that in VB, rather annoyingly.</remarks>
        Friend Sub SetTree(gt As GroupTree)
            If mTree Is gt Then Return
            mTree = gt
        End Sub

    End Class

End Namespace
