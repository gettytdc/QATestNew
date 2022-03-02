
Imports BluePrism.AutomateAppCore.Auth
Imports System.Runtime.Serialization

Namespace Groups

    ''' <summary>
    ''' Object which represents a group tree - ie a tree of components stemming from
    ''' a single root group.
    ''' </summary>
    <Serializable>
    <DataContract([Namespace]:="bp", Name:="g", IsReference:=True)>
    <KnownType("GetAllKnownTypes")>
    Public Class GroupTree : Implements IGroupTree

        ''' <summary>
        ''' Gets the known types which are in use in this class.
        ''' </summary>
        ''' <returns>An enumerable of known types used in this tree</returns>
        Public Shared Function GetAllKnownTypes() As IEnumerable(Of Type)
            Return GroupMember.GetMemberTypes()
        End Function

        ' The root group of this tree
        <DataMember(Name:="r")>
        Private mRoot As TreeRoot

        ' The type of this tree
        <DataMember(Name:="t")>
        Private mType As GroupTreeType

        ' The data version of this tree
        <DataMember(Name:="v")>
        Private mVersion As Long

        ' The store backing this tree
        <NonSerialized>
        Private mStore As IGroupStore

        <DataMember(Name:="d", EmitDefaultValue:=False)>
        Private mHasDefaultGroup As Boolean

        <DataMember(Name:="c")>
        Private mCombinedAttributes As TreeAttributes

        ''' <summary>
        ''' Creates a new group tree
        ''' </summary>
        ''' <param name="tp">The type of group tree to create</param>
        ''' <param name="ver">The version of the data that this tree represents.
        ''' </param>
        Friend Sub New(tp As GroupTreeType, ver As Long, hasDefaultGroup As Boolean, combinedAttributes As TreeAttributes)
            mType = tp
            mRoot = New TreeRoot(Me)
            mHasDefaultGroup = hasDefaultGroup
            mCombinedAttributes = combinedAttributes

        End Sub

        ''' <summary>
        ''' Creates a new group tree with no version information
        ''' </summary>
        ''' <param name="tp">The type of group tree to create</param>
        Friend Sub New(tp As GroupTreeType, hasDefaultGroup As Boolean)
            Me.New(tp, 0L, hasDefaultGroup, New TreeAttributes())
        End Sub

        ''' <summary>
        ''' Creates a new group tree with no version information
        ''' </summary>
        ''' <param name="tp">The type of group tree to create</param>
        Friend Sub New(tp As GroupTreeType)
            Me.New(tp, 0L, False, New TreeAttributes())
        End Sub
        ''' <summary>
        ''' Gets the raw (ie. unfiltered) tree that is backing this tree. For
        ''' unfiltered (ie. raw) trees, this property will return the instance itself
        ''' </summary>
        Private ReadOnly Property RawTree As GroupTree Implements IGroupTree.RawTree
            Get
                Return Me
            End Get
        End Property



        ''' <summary>
        ''' Gets the data name of this tree, used for keeping track of data versions
        ''' within the database.
        ''' </summary>
        Friend ReadOnly Property DataName As String
            Get
                Dim attr = mType.GetTreeDefinition()
                Return If(attr Is Nothing, Nothing, attr.DataName)
            End Get
        End Property

        ''' <summary>
        ''' The data version that this tree object represents
        ''' </summary>
        Public ReadOnly Property DataVersion As Long
            Get
                Return mVersion
            End Get
        End Property

        ''' <summary>
        ''' The type of tree this object represents.
        ''' </summary>
        Public ReadOnly Property TreeType As GroupTreeType _
         Implements IGroupTree.TreeType
            Get
                Return mType
            End Get
        End Property

        Public ReadOnly Property CombinedAttributes As TreeAttributes _
         Implements IGroupTree.CombinedAttributes
            Get
                Return mCombinedAttributes
            End Get
        End Property

        ''' <summary>
        ''' Gets the image key associated with this tree, or an empty string if it
        ''' has no image key associated with it.
        ''' </summary>
        Public ReadOnly Property ImageKey As String _
         Implements IGroupTree.ImageKey
            Get
                Return Definition.ImageKey
            End Get
        End Property

        ''' <summary>
        ''' Gets the member types supported by this tree
        ''' </summary>
        Public ReadOnly Property SupportedMembers As ICollection(Of GroupMemberType) _
         Implements IGroupTree.SupportedMembers
            Get
                Return Definition.SupportedMemberTypes
            End Get
        End Property

        ''' <summary>
        ''' The root of the group tree. Every tree starts with a single root, which
        ''' owns the top level groups and usually the items not assigned to any group
        ''' </summary>
        ''' <remarks>This just returns the <see cref="Root"/> property as an IGroup.
        ''' </remarks>
        Private ReadOnly Property RootGroup As IGroup Implements IGroupTree.Root
            Get
                Return Root
            End Get
        End Property

        ''' <summary>
        ''' The root of the group tree. Every tree starts with a single root, which
        ''' owns the top level groups and usually the items not assigned to any group
        ''' </summary>
        Public ReadOnly Property Root As Group
            Get
                Return mRoot
            End Get
        End Property

        ''' <summary>
        ''' Gets the tree definition attribute for this tree, or an empty attribute
        ''' if this tree has no definition associated with it.
        ''' </summary>
        Public ReadOnly Property Definition As TreeDefinitionAttribute
            Get
                Return TreeType.GetTreeDefinition()
            End Get
        End Property

        ''' <summary>
        ''' The backing store in use for this group tree. This is automatically
        ''' called when updates to the tree structure are made.
        ''' </summary>
        ''' <remarks>This will never be null - if no store is set in this tree, a
        ''' <see cref="NullGroupStore"/> will be returned, effectively a group store
        ''' implementation which 'no-op's everything.</remarks>
        Public Property Store As IGroupStore
            Get
                Return If(mStore Is Nothing, NullGroupStore.Instance, mStore)
            End Get
            Friend Set(value As IGroupStore)
                mStore = value
            End Set
        End Property

        ''' <summary>
        ''' Implementation of <see cref="IGroupTree.Store"/> which satisfies the VB
        ''' compiler's silly issue with a property with a getter and a setter not
        ''' matching the requirement of having a property with a getter.
        ''' Entirely redundant, wholly unnecessary, absolutely no point in using it -
        ''' just use <see cref="Store"/> instead.
        ''' </summary>
        Private ReadOnly Property AnnoyingUnnecessaryInterfaceStore As IGroupStore _
         Implements IGroupTree.Store
            Get
                Return Store
            End Get
        End Property

        ''' <summary>
        ''' Returns true if this tree has a default group.
        ''' A tree with a default group will prevent creating or / moving items (except for groups) in the root node.
        ''' Instead, items will be created in or moved into the default group.
        ''' This will return true even if the current user has no permissions on the default group and cannot see it.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HasDefaultGroup As Boolean _
            Implements IGroupTree.HasDefaultGroup
            Get
                Return mHasDefaultGroup
            End Get
        End Property

        ''' <summary>
        ''' Returns true if this tree contains a default group which the current user can see.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CanAccessDefaultGroup As Boolean _
            Implements IGroupTree.CanAccessDefaultGroup
            Get
                Return Root.OfType(Of IGroup).Any(Function(g) g.IsDefault)
            End Get
        End Property


        ''' <summary>
        ''' Returns this tree's default group. Returns null if 
        ''' there isn't one, or if the current user is unable to see the default group.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property DefaultGroup As IGroup _
            Implements IGroupTree.DefaultGroup
            Get
                Return Root.OfType(Of IGroup).FirstOrDefault(Function(g) g.IsDefault)
            End Get
        End Property

        ''' <summary>
        ''' Reloads this tree from the backing store
        ''' </summary>
        Public Sub Reload() Implements IGroupTree.Reload
            Store.Refresh(Me)
        End Sub

        ''' <summary>
        ''' Updates this tree with the data from the given tree
        ''' </summary>
        ''' <param name="newTree">The new tree, of the same type as this tree, with
        ''' the data to take into this tree.</param>
        ''' <remarks>This is here so that we can swap out the root with data from a
        ''' different tree without having to expose that root any further than this
        ''' class.</remarks>
        Friend Sub UpdateDataFrom(newTree As GroupTree)
            If newTree Is Nothing OrElse newTree Is Me Then Return
            DirectCast(mRoot, TreeRoot).SetTree(Nothing)
            mRoot = newTree.mRoot
            DirectCast(mRoot, TreeRoot).SetTree(Me)
        End Sub

        ''' <summary>
        ''' Checks to see if the given user has permission to edit this group tree.
        ''' </summary>
        ''' <param name="u">The user to check has permssion</param>
        Public Function HasEditPermission(u As User) As Boolean _
         Implements IGroupTree.HasEditPermission
            Dim p As Permission = Definition.EditPermission
            Return (p Is Nothing OrElse u.HasPermission(p))
        End Function

        ''' <summary>
        ''' Gets a filtered view of this tree using the given predicate.
        ''' </summary>
        ''' <param name="f">The predicate which determines the group members which
        ''' should be visible in the tree.</param>
        ''' <returns>A group tree using data from this tree with a filter applied.
        ''' </returns>
        Public Function GetFilteredView(f As Predicate(Of IGroupMember)) As IGroupTree _
         Implements IGroupTree.GetFilteredView
            Return New FilteringGroupTree(Me, f, Nothing, False)
        End Function

        ''' <summary>
        ''' Gets a filtered view of this tree using the given predicate.
        ''' </summary>
        ''' <param name="f">The predicate which determines the group members which
        ''' should be visible in the tree.</param>
        ''' <param name="g">A predicate that filters groups</param>
        ''' <param name="poolsAsGroups">swich to say whether to display
        ''' pools as group or members</param>
        ''' <returns>A group tree using data from the source tree with a different
        ''' filter applied.
        ''' </returns>
        Public Function GetFilteredView(f As Predicate(Of IGroupMember),
                                        g As Predicate(Of IGroup),
                                        poolsAsGroups As Boolean) As IGroupTree _
         Implements IGroupTree.GetFilteredView
            Return New FilteringGroupTree(Me, f, g, poolsAsGroups)
        End Function

    End Class
End Namespace
