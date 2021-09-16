Imports System.Collections.Concurrent
Imports System.Reflection
Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Core.Data
Imports BluePrism.Images
Imports BluePrism.Server.Domain.Models

Namespace Groups

    ''' <summary>
    ''' Base class for all nodes within a group
    ''' </summary>
    <Serializable(), DebuggerDisplay("{MemberType}: {Name}")>
    <DataContract([Namespace]:="bp", IsReference:=True)>
    <KnownType(GetType(Group))>
    <KnownType(GetType(MemberPermissions))>
    Public MustInherit Class GroupMember : Implements IItemHeader, IGroupMember

#Region " Class-scope Declarations "


        ''' <summary>
        ''' Gets all concrete group member types in this assembly
        ''' </summary>
        ''' <returns>An enumerable of all group member types found in this assembly
        ''' </returns>
        Friend Shared Function GetMemberTypes() As IEnumerable(Of Type)
            Return Assembly.GetExecutingAssembly().GetConcreteSubclasses(Of GroupMember)()
        End Function

        ''' <summary>
        ''' A filter which can be applied to a tree which allows all non-retired
        ''' group members to pass, but no others
        ''' </summary>
        Public Shared ReadOnly Property NotRetired As Predicate(Of IGroupMember)
            Get
                Return Function(m) Not m.IsRetired
            End Get
        End Property

        ''' <summary>
        ''' A filter which can be applied to a tree which allows all retired group
        ''' members to pass, but no others.
        ''' </summary>
        Public Shared ReadOnly Property Retired As Predicate(Of IGroupMember)
            Get
                Return Function(m) m.IsRetired
            End Get
        End Property

        ''' <summary>
        ''' A comparer which compares group member instances on Type and ID only.
        ''' Note that this will <em>not</em> provide a mechanism to support group
        ''' members of similar type into name order, but is essential to be able to
        ''' compare items whose names may have changed.
        ''' </summary>
        Public Shared ReadOnly TypeAndIdComparer As IComparer(Of GroupMember) =
            New TypeAndIdComparerImpl()

        ''' <summary>
        ''' Class to compare two group members, disregarding any name difference.
        ''' </summary>
        Private Class TypeAndIdComparerImpl : Implements IComparer(Of IGroupMember)

            ''' <summary>
            ''' Compares the two group members, based on type and ID only.
            ''' </summary>
            ''' <param name="x">The first group member to compare</param>
            ''' <param name="y">The second group member to compare</param>
            ''' <returns>-1, 0 or 1 if x precedes, is equal to, or exceeds y,
            ''' respectively.</returns>
            Public Function Compare(x As IGroupMember, y As IGroupMember) As Integer _
             Implements IComparer(Of IGroupMember).Compare
                ' Deal with ref-equality and nulls first...
                If x Is y Then Return 0
                If x Is Nothing Then Return -1
                If y Is Nothing Then Return -1
                ' Group by type first
                Dim comp As Integer = x.MemberType.CompareTo(x.MemberType)
                If comp <> 0 Then Return comp

                ' If we have two elements of the same type and same name (?),
                ' compare by ID
                Return x.ComparableId.CompareTo(y.ComparableId)

            End Function
        End Class

        ''' <summary>
        ''' An equality comparer for group members which tests their group membership
        ''' as well as their usual comparisons to test for the same member.
        ''' </summary>
        Public Class OwnedMemberComparer
            Implements IEqualityComparer(Of IGroupMember)

            ''' <summary>
            ''' Checks if the two group members are equal according to this comparer.
            ''' </summary>
            ''' <param name="x">The first group member to test</param>
            ''' <param name="y">The second group member to test</param>
            ''' <returns>true if x and y represent the same group member within the
            ''' same group.</returns>
            Public Overloads Function Equals(x As IGroupMember, y As IGroupMember) _
             As Boolean Implements IEqualityComparer(Of IGroupMember).Equals
                If Not x.Equals(y) Then Return False
                Dim xOwn = x.Owner, yOwn = y.Owner
                If xOwn Is Nothing Then Return (yOwn Is Nothing)
                If yOwn Is Nothing Then Return False
                Return (Object.Equals(xOwn.Id, yOwn.Id))
            End Function

            ''' <summary>
            ''' Gets an integer hash for the given group member. This is a function
            ''' of the type and name of the group member, as well as its parentgroup.
            ''' </summary>
            ''' <param name="mem">The member for which the integer hash is required.
            ''' </param>
            ''' <returns>An integer hash of the given group member.</returns>
            Public Overloads Function GetHashCode(mem As IGroupMember) As Integer _
             Implements IEqualityComparer(Of IGroupMember).GetHashCode
                Return (mem.GetHashCode() Xor
                        If(mem.Owner Is Nothing, 0, mem.Owner.GetHashCode()))
            End Function

        End Class

#End Region

#Region " Member Variables "

        ' The filtered tree that this group member is operating within
        <NonSerialized>
        Private mFilteredTree As IFilteringGroupTree

        ' The source of this group member within the filtering context
        <NonSerialized>
        Protected mSource As GroupMember

        ' The ID of the member that this node represents
        <DataMember(Name:="i")>
        Private mId As Object

        ' The name of the member that this node represents
        <DataMember(Name:="n")>
        Private mName As String

        ' The data associated with this group member
        Private mData As IDictionary(Of String, Object)

#End Region

#Region " Auto Properties "

        ''' <summary>
        ''' The owner of this group member
        ''' </summary>
        <DataMember(Name:="o")>
        Public Overridable Property Owner As IGroup Implements IGroupMember.Owner

        ''' <summary>
        ''' The current user's permissions for this node 
        ''' </summary>
        ''' <returns></returns>
        <DataMember(Name:="p")>
        Public Property Permissions As IMemberPermissions Implements IGroupMember.Permissions

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new base node with no ID. Note that this indicates to the class
        ''' that it is <em>not</em> yet <see cref="IsPersisted">persisted</see>
        ''' </summary>
        ''' <param name="memberName">The name of the member which is represented by
        ''' this node.</param>
        Friend Sub New(memberName As String)
            Me.New(Nothing, memberName)
        End Sub

        ''' <summary>
        ''' Creates a new base node
        ''' </summary>
        ''' <param name="memberId">The ID of the member which is represented by this
        ''' node</param>
        ''' <param name="memberName">The name of the member which is represented by
        ''' this node.</param>
        Friend Sub New(memberId As Object, memberName As String)
            Id = memberId
            Name = memberName
        End Sub


        ''' <summary>
        ''' Creates a new process-backed group member using data from a provider.
        ''' </summary>
        ''' <param name="prov">The provider of the data to initialise this group
        ''' member with - this expects: <list>
        ''' <item>id: [Guid, Integer or String]: The ID of the member</item>
        ''' <item>name: String: The name of the member</item>
        ''' </list></param>
        Protected Sub New(prov As IDataProvider)
            Me.New(prov.GetValue(Of Object)("id", Nothing), prov.GetString("name"))
        End Sub

#End Region

#Region " Properties "
        ''' <summary>
        ''' Checks if this object represents a group member which is in the model
        ''' </summary>
        Public ReadOnly Property IsInModel As Boolean _
         Implements IGroupMember.IsInModel
            Get
                If IsInFilterContext Then Return RawMember.IsInModel
                Return (Tree IsNot Nothing)
            End Get
        End Property

        ''' <summary>
        ''' The ID of the member which this entry in the group represents
        ''' </summary>
        ''' <remarks>This is typically a GUID, String or Integer; if it ever extends
        ''' to anything else, it should be ensured that the type is Serializable.
        ''' </remarks>
        Public Property Id As Object Implements IGroupMember.Id
            Get
                If IsInFilterContext Then Return RawMember.Id
                Return mId
            End Get
            Set(value As Object)
                If IsInFilterContext Then
                    RawMember.Id = value
                Else
                    mId = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' The owner of this member.
        ''' </summary>
        Public Overridable Property RawOwner As Group
            Get
                Return If(Owner Is Nothing, Nothing, Owner.RawGroup)
            End Get
            Set(value As Group)
                If mFilteredTree Is Nothing _
                 Then Owner = value _
                 Else Owner = New FilteringGroup(mFilteredTree, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets the image key for this group member
        ''' </summary>
        Public MustOverride ReadOnly Property ImageKey As String _
         Implements IGroupMember.ImageKey

        ''' <summary>
        ''' Gets the image key for this group member if it is expanded. Only really
        ''' applies to groups at the moment, but this may have some semantic meaning
        ''' for other elements in the future (models within objects, for instance).
        ''' </summary>
        Public Overridable ReadOnly Property ImageKeyExpanded As String _
         Implements IGroupMember.ImageKeyExpanded
            Get
                Return ImageKey
            End Get
        End Property

        ''' <summary>
        ''' The type of group member that this object represents.
        ''' </summary>
        Public MustOverride ReadOnly Property MemberType As GroupMemberType _
         Implements IGroupMember.MemberType


        ''' <summary>
        ''' The linking table between nodes of this type and their owning group.
        ''' </summary>
        Public MustOverride ReadOnly Property LinkTableName As String

        ''' <summary>
        ''' The column which holds the groupid for this member
        ''' </summary>
        Friend Overridable ReadOnly Property GroupIdColumnName As String
            Get
                Return "groupid"
            End Get
        End Property

        ''' <summary>
        ''' The column which holds the id of the member that this node represents.
        ''' </summary>
        Friend Overridable ReadOnly Property MemberIdColumnName As String
            Get
                Return "memberid"
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this group member represents a retired member or not.
        ''' By default, no member is retired, but subclasses may vary.
        ''' </summary>
        Public Overridable ReadOnly Property IsRetired As Boolean _
         Implements IGroupMember.IsRetired
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this group member represents a locked member or not.
        ''' By default, no member is locked.
        ''' </summary>
        Public Overridable ReadOnly Property IsLocked As Boolean _
         Implements IGroupMember.IsLocked
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this node is persisted to the database or not.
        ''' </summary>
        Public ReadOnly Property IsPersisted As Boolean
            Get
                Return (Id IsNot Nothing)
            End Get
        End Property

        ''' <summary>
        ''' Gets the raw member associated with this group member. If this member is
        ''' operating within a filter context, this will be the member instance in
        ''' the raw tree with no filtering applied.
        ''' </summary>
        Friend ReadOnly Property RawMember As GroupMember _
         Implements IGroupMember.RawMember
            Get
                If IsInFilterContext Then Return mSource.RawMember
                Return Me
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this group member is operating in a filter context or not -
        ''' ie. whether it is acting as a filtered proxy to the 'raw' group member
        ''' in the unfiltered tree.
        ''' </summary>
        Friend ReadOnly Property IsInFilterContext As Boolean
            Get
                Return (mFilteredTree IsNot Nothing)
            End Get
        End Property

        ''' <summary>
        ''' The name of the member which this entry in the group represents.
        ''' Note that this can never be null.
        ''' </summary>
        Public Overridable Property Name() As String Implements IGroupMember.Name
            Get
                If IsInFilterContext Then Return RawMember.Name
                Return If(mName, "")
            End Get
            Set(value As String)
                If IsInFilterContext Then
                    RawMember.Name = value
                Else
                    If mName = value Then Return
                    Dim oldName = mName
                    Dim release As IDisposable = Nothing
                    Try
                        ' If we're in a group, we need to rename via the group so
                        ' that it can update its contents collection (for Group
                        ' classes, this is a SortedSet, and the member's name is
                        ' an essential part of the sorting, so it needs to be removed
                        ' and re-added after the rename)
                        Dim gp As Group = TryCast(Owner, Group)
                        If gp IsNot Nothing Then release = gp.LockForRename(Me)
                        mName = value
                        If IsGroup And oldName IsNot Nothing Then
                            DirectCast(Me, IGroup).UpdateGroupName(mName, oldName)
                        End If
                    Catch gre As GroupRenameException
                        'what the group should be
                        mName = gre.GroupName
                        Throw
                    Catch
                        ' Revert the name
                        mName = oldName
                        ' And rethrow the error
                        Throw

                    Finally
                        If release IsNot Nothing Then release.Dispose()

                    End Try
                End If
            End Set
        End Property

        ''' <summary>
        ''' Indicates whether or not this node is a group
        ''' </summary>
        Public Overridable ReadOnly Property IsGroup As Boolean _
         Implements IGroupMember.IsGroup
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Indicates if this group represents the root of a tree or not. Note that
        ''' a disassociated group (ie. a group with no owner) <em>is not</em> a root,
        ''' according to the rules of this property; a root of a tree is the group
        ''' that is returned when <see cref="GroupTree.Root"/> is called, and is a
        ''' specific type, not a group in a specific state.
        ''' </summary>
        ''' <returns>False by default, True if this group is a tree root.</returns>
        Public Overridable ReadOnly Property IsRoot As Boolean _
         Implements IGroupMember.IsRoot
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Gets a dependency with which references to this group member can be
        ''' searched for, or null if this group member cannot be searched for
        ''' dependencies.
        ''' </summary>
        Public Overridable ReadOnly Property Dependency As clsProcessDependency _
         Implements IGroupMember.Dependency
            Get
                Return Nothing
            End Get
        End Property

        ''' <summary>
        ''' Gets the root element of the tree that this is a member of, as a
        ''' <see cref="TreeRoot"/> object, or null if this member is not actually
        ''' part of any tree.
        ''' </summary>
        Friend Overridable ReadOnly Property Root As TreeRoot
            Get
                Dim gp As IGroup = RootGroup()
                Return If(gp Is Nothing, Nothing, TryCast(gp.RawGroup, TreeRoot))
            End Get
        End Property

        ''' <summary>
        ''' Gets the tree that this group member forms a part of
        ''' </summary>
        ''' <remarks>Note that this must be overridden by the <see cref="TreeRoot"/>
        ''' class or a StackOverflowException will probably occur (certainly,
        ''' actually - TreeRoot.Tree will perpetually call TreeRoot.Tree).</remarks>
        Public Overridable ReadOnly Property Tree As IGroupTree _
         Implements IGroupMember.Tree
            Get
                If IsInFilterContext Then Return mFilteredTree
                Dim r As TreeRoot = Root
                Return If(r Is Nothing, Nothing, r.Tree)
            End Get
        End Property

        ''' <summary>
        ''' Gets the raw, unfiltered tree associated that this group member is
        ''' ultimately a part of.
        ''' </summary>
        Friend ReadOnly Property RawTree As GroupTree
            Get
                Dim t As IGroupTree = Tree
                If t IsNot Nothing Then Return t.RawTree
                Return Nothing
            End Get
        End Property

        ''' <summary>
        ''' Gets the store associated with this member; that is the store which is
        ''' associated with the tree that this member is part of. If this member is
        ''' not associated with a tree, or that tree has no store, this will return
        ''' a <see cref="NullGroupStore"/> instance - ie. a no-op store which does
        ''' not update any backing store when changes are made.
        ''' </summary>
        Protected ReadOnly Property Store As IGroupStore
            Get
                Dim t As IGroupTree = RawTree
                Return If(t Is Nothing, NullGroupStore.Instance, t.Store)
            End Get
        End Property

        ''' <summary>
        ''' Gets the full path from the root of the tree to this group member.
        ''' This is equivalent to a call to <see cref="Path"/>, but presented as a
        ''' property.
        ''' </summary>
        Public ReadOnly Property FullPath As String
            Get
                Return Path()
            End Get
        End Property

        ''' <summary>
        ''' Gets the data map associated with this group member. If it is in a filter
        ''' context, this will be the data in the raw member, not necessarily in this
        ''' object itself.
        ''' </summary>
        Protected ReadOnly Property DataMap As IDictionary(Of String, Object)
            Get
                If IsInFilterContext Then Return RawMember.DataMap
                If mData Is Nothing Then mData = New ConcurrentDictionary(Of String, Object)
                Return mData
            End Get
        End Property

        ''' <summary>
        ''' Gets the data associated with the given name
        ''' </summary>
        ''' <param name="name">The name of the data required</param>
        ''' <value>The value to set the data to</value>
        ''' <returns>The data associated with the given name or null if there is no
        ''' such data.</returns>
        Default Protected Property Data(name As String) As Object
            Get
                Dim obj As Object = Nothing
                DataMap.TryGetValue(name, obj)
                Return obj
            End Get
            Set(value As Object)
                DataMap(name) = value
            End Set
        End Property

#End Region

#Region " IItemInfo Implementation "

        ''' <summary>
        ''' The title to use for this group member in <see cref="IItemHeader">Item
        ''' Header</see> usage; by default, this is just the name of this group
        ''' member, but it may be overridden by subclasses.
        ''' </summary>
        Protected Overridable ReadOnly Property ItemTitle As String _
         Implements IItemHeader.Title
            Get
                Return Name
            End Get
        End Property

        ''' <summary>
        ''' The subtitle to use for this group member in <see cref="IItemHeader">
        ''' Item Header</see> usage; by default, this is just the type of group
        ''' member, but it may be overridden by subclasses.
        ''' </summary>
        Protected Overridable ReadOnly Property ItemSubTitle As String _
         Implements IItemHeader.SubTitle
            Get
                Return Me.MemberType.GetName()
            End Get
        End Property

        ''' <summary>
        ''' The image key to use for this group member in <see cref="IItemHeader">
        ''' Item Header</see> usage; by default this is just the ImageKey of this
        ''' member, but it may be overridden by subclasses.
        ''' </summary>
        ''' <remarks>Note that, while the <see cref="ImageKey"/> and
        ''' <see cref="ImageKeyExpanded"/> keys must reference the components list
        ''' defined in <see cref="ImageLists.Components_16x16"/> or
        ''' <see cref="ImageLists.Components_32x32"/>, this image key is dependent on
        ''' the image list set within the context of the item info being displayed.
        ''' </remarks>
        Protected Overridable ReadOnly Property ItemImageKey As String _
         Implements IItemHeader.ImageKey
            Get
                Return ImageKey
            End Get
        End Property

        Public Overridable ReadOnly Property IsPool As Boolean Implements IGroupMember.IsPool
            Get
                Return False
            End Get
        End Property

        Public Overridable ReadOnly Property IsMember As Boolean Implements IGroupMember.IsMember
            Get
                Return True
            End Get
        End Property

#End Region

#Region " Methods "

        ''' <summary>
        ''' Resets the locked state of this group member. Does nothing if this member
        ''' has no 'locked' concept or if it is not currently locked.
        ''' </summary>
        Public Overridable Sub ResetLock() Implements IGroupMember.ResetLock
        End Sub

        ''' <summary>
        ''' Checks if this member is part of a specific tree. This will check the
        ''' source tree that this group member resides in to see if it is the same
        ''' source tree as the given argument.
        ''' </summary>
        ''' <param name="gt">The tree to check to see if this member resides in it.
        ''' </param>
        ''' <returns>True if this member is in the given tree, false otherwise. Note
        ''' that if this member was in the given tree and has been removed, thus
        ''' being orphaned from any tree, this will return false.</returns>
        ''' <remarks>Note that it is enough for the trees to be of the same type for
        ''' them to be considered equal - ie. if this member is in a tree of the
        ''' same <see cref="IGroupTree.TreeType">tree type</see> as
        ''' <paramref name="gt"/>, this will return true</remarks>
        Public Function IsInTree(gt As IGroupTree) As Boolean _
         Implements IGroupMember.IsInTree
            Dim t As IGroupTree = Me.Tree

            ' Only null == null; so check both ways
            If t Is Nothing Then Return (gt Is Nothing)
            If gt Is Nothing Then Return False

            Return (t.TreeType = gt.TreeType)

        End Function

        ''' <summary>
        ''' Appends data to this group member from the given provider.
        ''' This is used in situations where a group member has several rows returned
        ''' for a single member - eg. where a member has a one-to-many relationship
        ''' with a different set of data. The situation which caused this to be
        ''' invented was for a <see cref="UserGroupMember"/> which has a list of
        ''' associated user roles defined in a different table.
        ''' </summary>
        ''' <param name="prov">The provider from which to append the data.</param>
        ''' <remarks>Subclasses should ensure that they document any meaningful
        ''' values that it expects from <paramref name="prov"/> with respect to their
        ''' override of this method.</remarks>
        Public Overridable Sub AppendFrom(prov As IDataProvider) _
         Implements IGroupMember.AppendFrom
            ' Nothing to do for most group members.
        End Sub

        ''' <summary>
        ''' Gets associated data from this member or sets it with a default value.
        ''' </summary>
        ''' <typeparam name="T">The type of data to get/set</typeparam>
        ''' <param name="name">The name of the data to address</param>
        ''' <param name="value">The value to use and set within the associated data
        ''' in this group member if it is not there already.</param>
        ''' <returns>The data associated with <paramref name="name"/> in this group
        ''' member, cast into the specified type, or <paramref name="value"/> if no
        ''' such data currently exists in this member</returns>
        Protected Function GetOrSetData(Of T)(name As String, value As T) As T
            Dim obj As Object = Data(name)
            If obj IsNot Nothing Then Return DirectCast(obj, T)
            Data(name) = value
            Return value
        End Function

        ''' <summary>
        ''' Clears an item of associated data from this group member
        ''' </summary>
        ''' <param name="name">The name of the associated data to clear</param>
        Protected Sub ClearData(name As String)
            DataMap.Remove(name)
        End Sub

        ''' <summary>
        ''' Sets associated data into this group member, taking into account the fact
        ''' that this group member may be a filtering proxy for a 'raw' group member.
        ''' </summary>
        ''' <typeparam name="T">The type of data to set</typeparam>
        ''' <param name="name">The name to associate with the data.</param>
        ''' <param name="value">The value to set in the data</param>
        Protected Sub SetData(Of T)(name As String, value As T)
            Data(name) = value
        End Sub

        ''' <summary>
        ''' Gets the data associated with the given name, taking into account the
        ''' fact that this group member may be a filtering proxy for a 'raw' group
        ''' member.
        ''' </summary>
        ''' <typeparam name="T">The type of data to get/set</typeparam>
        ''' <param name="name">The name of the data to address</param>
        ''' <param name="defaultValue">The value to return if no associated data
        ''' keyed against <paramref name="name"/> is found in this group member.
        ''' </param>
        ''' <returns>The data associated with <paramref name="name"/> in this group
        ''' member, cast into the specified type, or <paramref name="defaultValue"/>
        ''' if no such data currently exists in this member</returns>
        Protected Function GetData(Of T)(name As String, defaultValue As T) As T
            Return DirectCast(If(Data(name), defaultValue), T)
        End Function

        ''' <summary>
        ''' Gets a filtered view of this group member, using the given filtering
        ''' group tree to provide the filtering context for this group member.
        ''' </summary>
        ''' <param name="filtree">The filtering tree which the filtered view of the
        ''' member should form part of</param>
        ''' <returns>A clone of this group member which operates within the context
        ''' of the filtered tree: <paramref name="filtree"/>.</returns>
        Public Overridable Function GetFilteredView(filtree As IFilteringGroupTree) _
         As IGroupMember _
         Implements IGroupMember.GetFilteredView
            With DirectCast(CloneOrphaned(), GroupMember)
                .mFilteredTree = filtree
                .mSource = Me
                .Owner = New FilteringGroup(filtree, RawOwner)
                Return .It()
            End With
        End Function

        ''' <summary>
        ''' Moves this group member into the given group.
        ''' </summary>
        ''' <param name="target">The group to move this member to</param>
        ''' <returns>The member, after moving to the target group, or null if the
        ''' member was not moved for any reason (eg. member already exists in
        ''' target group).</returns>
        ''' <exception cref="ArgumentNullException">If <paramref name="target"/> is
        ''' null.</exception>
        Public Function MoveTo(target As IGroup) As IGroupMember _
         Implements IGroupMember.MoveTo
            ' Delegate to the owner which has more context to make these changes
            ' with as few trips to the store as possible
            Return RawOwner.MoveMember(RawMember, target.RawGroup, False)
        End Function

        ''' <summary>
        ''' Creates a shallow clone of this groupmember, copying all its references
        ''' into the new object. Note that although the copy will have its owner set
        ''' to the same as this member, the owner will not have the copy in its
        ''' collection of members.
        ''' </summary>
        ''' <returns>A memberwise clone of this group member</returns>
        Protected Overridable Function Clone() As GroupMember
            Return DirectCast(RawMember.MemberwiseClone(), GroupMember)
        End Function

        ''' <summary>
        ''' Creates an orphaned deep clone of this group member
        ''' </summary>
        ''' <returns>A clone of this group member with no owner. Note that if this
        ''' member is associated with other members (eg. it's a group), those
        ''' <em>will</em> be deep cloned in the returned member.</returns>
        ''' <remarks>Note that any associated data (ie. data referenced via the
        ''' <see cref="Data"/> property) is not deep cloned by this implementation.
        ''' If necessary, this task should be performed by subclasses which know if
        ''' the data is value/immutable/cloneable etc.</remarks>
        Public Overridable Function CloneOrphaned() As IGroupMember _
         Implements IGroupMember.CloneOrphaned
            Dim copy As GroupMember = Clone()
            ' We don't need to remove this from the owning group since it's not
            ' actually in it.
            copy.Owner = Nothing
            ' We shallow-clone the extra data such that the dictionary is a different
            ' instance, but its contents aren't; we don't have the context to know
            ' whether it needs further work or not. We leave that to subclasses.
            copy.mData = Nothing
            If mData IsNot Nothing Then
                For Each pair In mData : copy.DataMap.Add(pair) : Next
            End If
            Return copy
        End Function

        ''' <summary>
        ''' Copies this group member into the given group
        ''' </summary>
        ''' <param name="gp">The group into which this member should be copied.
        ''' </param>
        ''' <returns>The copy of the member after being added to the target group
        ''' or null if the copy was not performed (typically if attempting to copy
        ''' to the same group that the member is already within).</returns>
        ''' <exception cref="ArgumentNullException">If <paramref name="gp"/> is
        ''' null.</exception>
        ''' <exception cref="InvalidArgumentException">If this group or the target
        ''' group is the <see cref="Group.IsRoot">root</see> of the tree.
        ''' </exception>
        Public Function CopyTo(gp As IGroup) As IGroupMember _
         Implements IGroupMember.CopyTo
            ' Already in that place. Ignore it.
            If gp Is Owner Then Return Nothing
            Return RawOwner.CopyMember(Me, gp.RawGroup)
        End Function

        ''' <summary>
        ''' Removes this group member from its owning group. It will effectively be
        ''' deleted as a group entry at this point.
        ''' </summary>
        Public Overridable Sub Remove() Implements IGroupMember.Remove
            If Owner Is Nothing Then Return
            If (FindAllOwners().Count = 1 AndAlso mFilteredTree IsNot Nothing AndAlso
                mFilteredTree.DefaultGroup IsNot Nothing) Then
                MoveTo(mFilteredTree.DefaultGroup)
            Else
                RawOwner.Remove(Me)
            End If
        End Sub

        ''' <summary>
        ''' Deletes this group member from the model. This removes it from all groups
        ''' thereby moving it into the root of the tree, then deletes it from the
        ''' root of the tree. Note that this will not affect the actual object that
        ''' the group member represents (eg. the process, queue, resource, etc) so,
        ''' unless that object is also deleted, the next time the tree is loaded from
        ''' the database, a group member will be reinstated to represent it.
        ''' </summary>
        Public Overridable Sub Delete() Implements IGroupMember.Delete
            RemoveFromAllGroups()
            Dim o As Group = RawOwner
            If o IsNot Nothing Then o.Delete(Me)
        End Sub

        ''' <summary>
        ''' Compares current node to the passed one.
        ''' </summary>
        ''' <param name="value">The node to compare with</param>
        ''' <returns>A 32-bit signed integer that indicates whether this instance
        ''' precedes, follows, or appears in the same position in the sort order as
        ''' the value parameter.
        ''' Less than zero - This instance precedes value.
        ''' Zero - This instance has the same position in the sort order as value.
        ''' Greater than zero - This instance follows value.</returns>
        Public Function CompareTo(value As Object) As Integer _
         Implements IComparable.CompareTo
            Dim mem As IGroupMember = DirectCast(value, IGroupMember)

            ' Group by type first
            Dim comp As Integer = MemberType.CompareTo(mem.MemberType)
            If comp <> 0 Then Return comp

            ' Then by name
            comp = Name.CompareTo(mem.Name)
            If comp <> 0 Then Return comp

            ' If we have two elements of the same type and same name (?),
            ' compare by ID
            Dim myid = ComparableId()
            Dim theirid As IComparable

            ' If we have a null Id, then restore older code where we compare the same Guid/Id
            If mem.Id Is Nothing Then
                theirid = ComparableId()
            Else
                theirid = mem.ComparableId()
            End If

            ' If either is null, deal with that separately
            If myid Is Nothing AndAlso theirid Is Nothing Then Return 0
            If myid Is Nothing Then Return -1
            If theirid Is Nothing Then Return 1

            ' If they have different types.. well, they shouldn't have, but it has
            ' happened in certain (not consistently reproducible) cases
            comp = myid.GetType().FullName.CompareTo(theirid.GetType().FullName)

#If DEBUG Then
            ' It shouldn't occur, but, for debug builds, try and provide a bit of
            ' information as to why it did occur
            If comp <> 0 Then Debug.Fail(String.Format(
             "Comparing group members - all equal except ID type - shouldn't happen. " &
             "Type: {0}; Name: {1}; This ID: {2}; Comparing ID: {3}",
             MemberType, Name, Id, mem.Id))
#End If
            If comp <> 0 Then Return comp

            Return myid.CompareTo(theirid)

        End Function

        ''' <summary>
        ''' Checks if this group member is equal to the given group member or not.
        ''' Note that a group member is considered equal if it is of the same type
        ''' and ID as another group member; its owning group is not taken into
        ''' account.
        ''' </summary>
        ''' <param name="obj">The object to check for equality against.</param>
        ''' <returns>True if the given object is of the same type and ID as this
        ''' object; False otherwise.</returns>
        Public Overrides Function Equals(obj As Object) As Boolean
            If Me Is obj Then Return True ' Shortcut check
            Dim gm As IGroupMember = TryCast(obj, IGroupMember)
            Return (gm IsNot Nothing AndAlso CompareTo(obj) = 0)
        End Function

        ''' <summary>
        ''' Gets the hashcode for this group member; this is an integer hash based on
        ''' the type and ID of the member.
        ''' </summary>
        ''' <returns>A hash for this group member</returns>
        Public Overrides Function GetHashCode() As Integer
            Return (MemberType.GetHashCode() << 16 Or If(Id, 0).GetHashCode())
        End Function

        ''' <summary>
        ''' Gets a string representation this group member.
        ''' </summary>
        ''' <returns>A String representation of this group member.</returns>
        Public Overrides Function ToString() As String
            Return _
             If(IsInFilterContext, "(Filtered) ", "") & MemberType.GetName() & ":" & Name
        End Function

        ''' <summary>
        ''' Sets this member's properties to match that given. Note that this does
        ''' <em>not</em> copy the member's contents if it happens to be a group,
        ''' though it does copy the owner.
        ''' </summary>
        ''' <param name="mem">The group member whose properties should be copied to
        ''' this member.</param>
        Friend Sub SetTo(mem As GroupMember)
            With mem
                Me.Id = .Id
                Me.Owner = .Owner
                Me.mName = .mName
            End With
        End Sub

        ''' <summary>
        ''' Checks if the supplied user has the appropriate permission to see this member
        ''' </summary>
        ''' <param name="user"></param>
        ''' <returns></returns>
        Public Overridable Function HasViewPermission(user As IUser) As Boolean Implements IGroupMember.HasViewPermission
            Return Me.Permissions.HasPermission(user, Permission.ByName(Permission.ProcessStudio.AllProcessPermissionsAllowingTreeView)) OrElse
            Me.Permissions.HasPermission(user, Permission.ByName(Permission.ObjectStudio.AllObjectPermissionsAllowingTreeView)) OrElse
            Me.Permissions.HasPermission(user, Permission.ByName(Permission.Resources.AllResourcePermissionsAllowingTreeView))
        End Function

        ''' <summary>
        ''' Checks if this member can be removed from it's group by the passed user.
        ''' </summary>
        ''' <param name="user">The user intending to remove it</param>
        ''' <returns>True if the operation is allowed, otherwise False</returns>
        Public Function CanBeRemovedFromGroup(user As IUser) As Boolean Implements IGroupMember.CanBeRemovedFromGroup

            ' Check we exist somewhere that can be removed from
            If Not IsInGroup OrElse
                (Owner.IsDefault AndAlso FindAllOwners().Count = 1) Then Return False

            ' Check we are something that can be removed from a group
            Dim poolMember = TryCast(Me, ResourceGroupMember) IsNot Nothing AndAlso
                                CType(Me, ResourceGroupMember).IsPoolMember
            If Not IsMember OrElse poolMember Then Return False

            ' Check that current user has permission to remove from the source group
            Dim treeDefinition = Tree.TreeType.GetTreeDefinition()
            If Not Permissions.HasPermission(user, treeDefinition.EditPermission) Then Return False
            If Permissions.IsRestricted AndAlso
                Not Permissions.HasPermission(user, treeDefinition.AccessRightsPermission) Then Return False

            ' If there is a default group, then check that current user has permission to add to it
            If Tree.HasDefaultGroup Then
                If Not Tree.CanAccessDefaultGroup Then Return False
                If Not Tree.DefaultGroup.Permissions.HasPermission(user, treeDefinition.EditPermission) Then Return False
                If Tree.DefaultGroup.Permissions.IsRestricted AndAlso
                        Not Tree.DefaultGroup.Permissions.HasPermission(user, treeDefinition.AccessRightsPermission) Then Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' Extracts a localised name, only if the type is correct.  Otherwise the name is simply returned
        ''' </summary>
        ''' <param name="localiser">localisation function</param>
        ''' <returns>Display string</returns>
        Public Overridable Function GetLocalisedName(localiser As Func(Of IGroupMember, String)) As String Implements IGroupMember.GetLocalisedName
            Return Name
        End Function

        ''' <summary>
        ''' Clear any local cached data used by the group object.
        ''' </summary>
        Public Sub ClearLocalGroupCache() Implements IGroupMember.ClearLocalGroupCache
            'clear any cache data here    
        End Sub
#End Region

    End Class

End Namespace
