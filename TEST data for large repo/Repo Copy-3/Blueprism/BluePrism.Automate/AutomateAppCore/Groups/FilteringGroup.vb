Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.AutomateAppCore.Auth
Imports System.Windows.Forms
Imports BluePrism.Server.Domain.Models

Namespace Groups

    ''' <summary>
    ''' A group which applies some filtering to its contents
    ''' </summary>
    Public Class FilteringGroup : Implements IGroup

#Region " Member Variables "

        ' The filtering tree which owns this group
        Protected mTree As IFilteringGroupTree

        ' The ID of this group
        Private mId As Guid

        ' Cache for the source as recalculated every time it's accessed
        Private mSourceOrNullCache As IGroup
        ' Flag that the cached value (mSourceOrNullCache) should be recalculated
        Private mRefreshCache As Boolean

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new filtering group within the given tree from the given group.
        ''' </summary>
        ''' <param name="treeVal">The filtering tree which owns this group</param>
        ''' <param name="srcGroup">The group which provides the basis for this
        ''' filtering group</param>
        Friend Sub New(treeVal As IFilteringGroupTree, srcGroup As IGroup)
            If treeVal Is Nothing Then Throw New ArgumentNullException(NameOf(treeVal))
            If srcGroup Is Nothing Then Throw New ArgumentNullException(NameOf(srcGroup))
            mTree = treeVal
            mId = srcGroup.IdAsGuid
            Me.SortFieldName = srcGroup.SortFieldName
            Me.SortOrder = srcGroup.SortOrder
            Me.Expanded = srcGroup.Expanded
            Debug.Assert(mId <> Guid.Empty OrElse srcGroup.IsRoot)
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets whether this group remains in the backing model that this object
        ''' is providing a view of
        ''' </summary>
        Public ReadOnly Property IsInModel As Boolean _
         Implements IGroupMember.IsInModel
            Get
                Return (SourceOrNull IsNot Nothing)
            End Get
        End Property

        ''' <summary>
        ''' The source group for this filtering group - ie. the group object in the
        ''' source tree that the path held in this filtering group represents
        ''' </summary>
        Protected ReadOnly Property Source As IGroup
            Get
                Dim gp As IGroup = SourceOrNull
                If gp Is Nothing Then Throw New NotInModelException(
                    "The group with ID '{0}' no longer exists in the model", mId)
                Return gp
            End Get
        End Property

        ''' <summary>
        ''' The source group for this filtering group - ie. the group object in the
        ''' source tree that the path held in this filtering group represents; or
        ''' null if this group no longer exists in the model.
        ''' </summary>
        Protected Friend ReadOnly Property SourceOrNull As IGroup
            Get
                
                If mSourceOrNullCache Is Nothing OrElse mRefreshCache Then
                    mSourceOrNullCache = mTree.GetSourceGroup(mId)
                    mRefreshCache = False
                End If
                Return mSourceOrNullCache
             
            End Get
        End Property


        ''' <summary>
        ''' Gets the raw (ie. unfiltered) group member that is backing this group.
        ''' For unfiltered (ie. raw) groups, this property will return the instance
        ''' itself.
        ''' </summary>
        Public ReadOnly Property RawMember As GroupMember _
         Implements IGroupMember.RawMember
            Get
                Return RawGroup
            End Get
        End Property

        ''' <summary>
        ''' Gets the raw (ie. unfiltered) group that is backing this group. For
        ''' unfiltered (ie. raw) groups, this property will return the instance
        ''' itself.
        ''' </summary>
        Public ReadOnly Property RawGroup As Group Implements IGroup.RawGroup
            Get
                Return Source.RawGroup
            End Get
        End Property

        ''' <summary>
        ''' Gets the number of filtered group members and groups in this group
        ''' </summary>
        Public ReadOnly Property Count As Integer _
         Implements ICollection(Of IGroupMember).Count
            Get
                Dim gp As IGroup = SourceOrNull
                Return If(gp Is Nothing, 0,
                    gp.Where(AddressOf mTree.PassesFilter).Count())
            End Get
        End Property

        ''' <summary>
        ''' Checks if this group is empty or whether contains any elements which
        ''' match the filter.
        ''' </summary>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Dim gp As IGroup = SourceOrNull
                Return (gp Is Nothing OrElse
                        Not gp.Where(AddressOf mTree.PassesFilter).Any())
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this group is readonly or not. It is not
        ''' </summary>
        Public ReadOnly Property IsReadOnly As Boolean _
         Implements ICollection(Of IGroupMember).IsReadOnly
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this group represents the root of the tree.
        ''' </summary>
        Public ReadOnly Property IsRoot As Boolean Implements IGroup.IsRoot
            Get
                Dim gp = SourceOrNull
                Return (gp IsNot Nothing AndAlso gp.IsRoot)
            End Get
        End Property

        ''' <summary>
        ''' Gets the supported group member types for this group.
        ''' </summary>
        Public ReadOnly Property SupportedTypes As ICollection(Of GroupMemberType) _
         Implements IGroup.SupportedTypes
            Get
                Dim gp = SourceOrNull
                Return If(gp IsNot Nothing, gp.SupportedTypes,
                    GetEmpty.ICollection(Of GroupMemberType)())
            End Get
        End Property

        ''' <summary>
        ''' Gets the type of tree that this group is part of
        ''' </summary>
        Public ReadOnly Property TreeType As GroupTreeType _
         Implements IGroup.TreeType
            Get
                Dim gp = SourceOrNull
                Return If(gp IsNot Nothing, gp.TreeType, GroupTreeType.None)
            End Get
        End Property

        ''' <summary>
        ''' Gets the dependency for this group, or null if it has no dependency
        ''' information.
        ''' </summary>
        Public ReadOnly Property Dependency As clsProcessDependency _
         Implements IGroupMember.Dependency
            Get
                Dim gp = SourceOrNull
                Return If(gp IsNot Nothing, gp.Dependency, Nothing)
            End Get
        End Property

        ''' <summary>
        ''' Gets the ID of this group.
        ''' </summary>
        Public Property Id As Object Implements IGroupMember.Id
            Get
                Dim gp = SourceOrNull
                Return If(gp IsNot Nothing, gp.Id, Guid.Empty)
            End Get
            Set(value As Object)
                Source.Id = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the imagekey for this group to display when it is not expanded
        ''' </summary>
        Public ReadOnly Property ImageKey As String Implements IGroupMember.ImageKey
            Get
                Dim gp = SourceOrNull
                Return If(gp IsNot Nothing, gp.ImageKey, "")
            End Get
        End Property

        ''' <summary>
        ''' Gets the imagekey for this group to display when it is expanded
        ''' </summary>
        Public ReadOnly Property ImageKeyExpanded As String _
         Implements IGroupMember.ImageKeyExpanded
            Get
                Dim gp = SourceOrNull
                Return If(gp IsNot Nothing, gp.ImageKeyExpanded, "")
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this group member represents a group or not. It does.
        ''' </summary>
        Public Overridable ReadOnly Property IsGroup As Boolean _
         Implements IGroupMember.IsGroup
            Get
                Return True
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this group is locked or not.
        ''' </summary>
        ''' <remarks>This is hardcoded to return False for performance, because
        ''' groups cannot currently be locked.</remarks>
        Public ReadOnly Property IsLocked As Boolean _
         Implements IGroupMember.IsLocked
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this group is retired or not
        ''' </summary>
        ''' <remarks>This is hardcoded to return False for performance, because
        ''' groups cannot currently be retired.</remarks>
        Public ReadOnly Property IsRetired As Boolean _
         Implements IGroupMember.IsRetired
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Gets the type of group member that this object represents
        ''' </summary>
        Public Overridable ReadOnly Property MemberType As GroupMemberType _
         Implements IGroupMember.MemberType
            Get
                Return GroupMemberType.Group
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the name of this group
        ''' </summary>
        Public Property Name As String Implements IGroupMember.Name
            Get
                Dim gp = SourceOrNull
                Return If(gp IsNot Nothing, gp.Name, "")
            End Get
            Set(value As String)
                ' Change the name in the source and update the path to match it
                Source.Name = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the owner of this group
        ''' </summary>
        Public Overridable Property Owner As IGroup Implements IGroupMember.Owner
            Get
                Dim gp = SourceOrNull
                If gp Is Nothing OrElse gp.Owner Is Nothing Then Return Nothing
                Return New FilteringGroup(mTree, gp.Owner)
            End Get
            Set(value As IGroup)
                ' If this is being set as a filtering group, get the source group
                ' from within it and pass that to this group's source instead.
                Dim fg = TryCast(value, FilteringGroup)
                If fg IsNot Nothing Then value = fg.Source
                Source.Owner = value
            End Set
        End Property

        ''' <summary>
        ''' The tree that this group forms a part of.
        ''' </summary>
        Public ReadOnly Property Tree As IGroupTree Implements IGroupMember.Tree
            Get
                Return mTree
            End Get
        End Property

        ''' <summary>
        ''' Get the effective permissios for this Group
        ''' </summary>
        ''' <returns></returns>
        Public Property Permissions As IMemberPermissions Implements IGroupMember.Permissions
            Set(value As IMemberPermissions)
                Source.Permissions = value
            End Set
            Get
                Dim gp = SourceOrNull
                Return If(gp IsNot Nothing, gp.Permissions,
                    New MemberPermissions(Nothing))
            End Get
        End Property

        ''' <summary>
        ''' Is this group the default group for the tree
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsDefault As Boolean Implements IGroup.IsDefault
            Get
                Return Source.IsDefault
            End Get
        End Property

        ''' <summary>
        ''' Does this group contain hidden members the user cannot access.
        ''' </summary>
        ''' <returns></returns>
        Public Property ContainsHiddenMembers As Boolean Implements IGroup.ContainsHiddenMembers
            Get
                Return Source.ContainsHiddenMembers
            End Get
            Set(value As Boolean)
                Source.ContainsHiddenMembers = value
            End Set
        End Property

        ''' <summary>
        ''' Returns if the given group represents a pool
        ''' </summary>
        Public Overridable ReadOnly Property IsPool As Boolean Implements IGroupMember.IsPool
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Returns true if the given group is a group member
        ''' </summary>
        Public ReadOnly Property IsMember As Boolean Implements IGroupMember.IsMember
            Get
                Return False
            End Get
        End Property

        Public Property SortFieldName As String Implements IGroup.SortFieldName

        Public Property SortOrder As SortOrder Implements IGroup.SortOrder

        Public Property Expanded As Boolean Implements IGroup.Expanded

#End Region

#Region " Methods "

        ''' <summary>
        ''' Resets the locked state of this group member. Does nothing if this member
        ''' has no 'locked' concept or if it is not currently locked.
        ''' </summary>
        Public Sub ResetLock() Implements IGroupMember.ResetLock
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
            Dim gp = SourceOrNull
            Return If(gp Is Nothing, gt Is Nothing, gp.IsInTree(gt))
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
         As IGroupMember Implements IGroupMember.GetFilteredView
            Dim gp = SourceOrNull
            If gp Is Nothing Then Return Nothing
            ' If the context is the same tree that we're already in, there's no need
            ' to create a new version of this group
            If filtree Is mTree Then Return Me
            ' Otherwise, pass the source group into a new filtering group
            Return New FilteringGroup(filtree, gp)
        End Function

        ''' <summary>
        ''' Adds a collection of group members to this group
        ''' </summary>
        ''' <param name="members">The members to add to this group</param>
        Public Overridable Sub AddRange(members As IEnumerable(Of IGroupMember)) _
         Implements IGroup.AddRange
            Source.AddRange(members)
        End Sub

        ''' <summary>
        ''' Creates a subgroup within this group.
        ''' </summary>
        ''' <param name="name">The name of the group to create</param>
        ''' <returns>The subgroup with the specified name created within this group.
        ''' </returns>
        Public Overridable Function CreateGroup(name As String) As IGroup _
         Implements IGroup.CreateGroup
            Return New FilteringGroup(mTree, Source.CreateGroup(name))
        End Function

        ''' <inheritdoc/>
        Public Sub UpdateGroupName(name As String) Implements IGroup.UpdateGroupName
            Throw New NotImplementedException()
        End Sub

        Public Sub UpdateGroupName(name As String, oldName As String) Implements IGroup.UpdateGroupName
            Throw New NotImplementedException()
        End Sub

        ''' <summary>
        ''' Deletes this group
        ''' </summary>
        Public Overridable Sub Delete() Implements IGroupMember.Delete
            Source.Delete()
        End Sub

        ''' <summary>
        ''' Removes a collection of members from this group
        ''' </summary>
        ''' <param name="members">The members to remove</param>
        ''' <returns>True if the removal of the given members changed the contents
        ''' of this group.</returns>
        Public Overridable Function RemoveAll(members As IEnumerable(Of IGroupMember)) As Boolean _
         Implements IGroup.RemoveAll
            Return Source.RemoveAll(members)
        End Function

        ''' <summary>
        ''' Clones an orphaned version of this group, orphaned from any owner or
        ''' the tree.
        ''' </summary>
        ''' <returns>A filtered clone of this group, orphaned from the tree</returns>
        Public Overridable Function CloneOrphaned() As IGroupMember _
         Implements IGroupMember.CloneOrphaned
            Return New FilteringGroup(
                mTree, DirectCast(Source.CloneOrphaned(), IGroup))
        End Function

        ''' <summary>
        ''' Copies this group to the target group, returning the resultant group
        ''' member
        ''' </summary>
        ''' <param name="targetGroup">The group to which this group should be copied.
        ''' </param>
        ''' <returns>The new group member which resulted from the copy operation.
        ''' </returns>
        Public Overridable Function CopyTo(targetGroup As IGroup) As IGroupMember _
         Implements IGroupMember.CopyTo
            Dim m = DirectCast(Source, IGroupMember).CopyTo(targetGroup)
            Return New FilteringGroup(mTree, DirectCast(m, IGroup))
        End Function

        ''' <summary>
        ''' Moves this group to a target group, returning the resultant group member.
        ''' </summary>
        ''' <param name="targetGroup">The group to which this object should be moved.
        ''' </param>
        ''' <returns>The group member which resulted from the move.</returns>
        Public Overridable Function MoveTo(targetGroup As IGroup) As IGroupMember _
         Implements IGroupMember.MoveTo
            Return New FilteringGroup(
                mTree, DirectCast(Source.MoveTo(targetGroup), IGroup))
        End Function

        ''' <summary>
        ''' Removes this group from its owning group
        ''' </summary>
        Public Overridable Sub Remove() Implements IGroupMember.Remove
            DirectCast(Source, IGroupMember).Remove()
        End Sub

        ''' <summary>
        ''' Adds a group member to this group
        ''' </summary>
        ''' <param name="item">The member to add to this group</param>
        Public Overridable Sub Add(item As IGroupMember) _
         Implements ICollection(Of IGroupMember).Add
            Source.Add(item)
        End Sub

        ''' <summary>
        ''' Removes all group members from this group.
        ''' </summary>
        Public Overridable Sub Clear() Implements ICollection(Of IGroupMember).Clear
            Source.Clear()
        End Sub

        ''' <summary>
        ''' Checks if this filtering group contains the given item. Note that if an
        ''' item does not pass the filter in this group, it will return false even if
        ''' the source group in the primary tree does contain such an item.
        ''' </summary>
        ''' <param name="item">The item to check to see if it is contained within
        ''' this group.</param>
        ''' <returns>True if the item passes the filter used in this group and is
        ''' contained within this group; False otherwise</returns>
        Public Function Contains(item As IGroupMember) As Boolean _
         Implements ICollection(Of IGroupMember).Contains
            Return mTree.PassesFilter(item) AndAlso Source.Contains(item)
        End Function

        ''' <summary>
        ''' Copies the members in this filtering group to an array.
        ''' </summary>
        ''' <param name="array">The array to which the group members should be copied
        ''' </param>
        ''' <param name="arrayIndex">The index at which the copying of the members
        ''' should begin</param>
        Public Overridable Sub CopyTo(array() As IGroupMember, arrayIndex As Integer) _
         Implements ICollection(Of IGroupMember).CopyTo
            For Each m As IGroupMember In Me
                array(arrayIndex) = m
                arrayIndex += 1
            Next
        End Sub

        ''' <summary>
        ''' Removes the given item from this group.
        ''' </summary>
        ''' <param name="item">The item to remove</param>
        ''' <returns>True if the item was removed and this group was altered as a
        ''' result; False otherwise.</returns>
        Public Function Remove(item As IGroupMember) As Boolean _
         Implements ICollection(Of IGroupMember).Remove
            Return DirectCast(Source, ICollection(Of IGroupMember)).Remove(item)
        End Function

        ''' <summary>
        ''' Gets an enumerator over the group members contained in this group
        ''' </summary>
        ''' <returns>An enumerator over the members in this filtered group.</returns>
        Public Function GetEnumerator() As IEnumerator(Of IGroupMember) _
         Implements IEnumerable(Of IGroupMember).GetEnumerator
            If Not IsInModel Then Return GetEmpty.IEnumerator(Of IGroupMember)()
            Dim query = Source.
                Where(AddressOf mTree.PassesFilter).
                Select(Function(m) m.GetFilteredView(mTree)).
                Sort(Me.SortFieldName, Me.SortOrder)
            Return query.GetEnumerator()
        End Function

        ''' <summary>
        ''' Gets an enumerator over the group members contained in this group
        ''' </summary>
        ''' <returns>An enumerator over the members in this filtered group.</returns>
        Private Function GetUntypedEnumerator() As IEnumerator _
         Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function

        ''' <summary>
        ''' Compares this group to another object.
        ''' </summary>
        ''' <param name="obj">The object to compare against.</param>
        ''' <returns>A 32-bit signed integer that indicates whether this instance
        ''' precedes, follows, or appears in the same position in the sort order as
        ''' the value parameter.
        ''' Less than zero - This instance precedes value.
        ''' Zero - This instance has the same position in the sort order as value.
        ''' Greater than zero - This instance follows value.</returns>
        Public Function CompareTo(obj As Object) As Integer _
         Implements IComparable.CompareTo
            Dim gp = SourceOrNull

            ' If this group is not in the model, it is less than any group that is
            ' If the comparing obj is not filtering, assume it's in the model; if it
            ' is filtering, check if it's in the model - considered equal if they are
            ' both not in the model;
            Dim fg = TryCast(obj, FilteringGroup)
            If gp Is Nothing Then _
             Return If(fg IsNot Nothing AndAlso Not fg.IsInModel, 0, -1)

            ' Otherwise, default to the group comparisons
            Return gp.CompareTo(obj)
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
            Return (CompareTo(obj) = 0)
        End Function

        ''' <summary>
        ''' Gets the hashcode for this group member; this is an integer hash based on
        ''' the type and ID of the member.
        ''' </summary>
        ''' <returns>A hash for this group member</returns>
        Public Overrides Function GetHashCode() As Integer
            Dim gp = SourceOrNull
            Return If(gp IsNot Nothing, gp.GetHashCode(), 0)
        End Function

        ''' <summary>
        ''' Gets a string representation this group member.
        ''' </summary>
        ''' <returns>A String representation of this group member.</returns>
        Public Overrides Function ToString() As String
            Return "(Filtered) Group:" & Name
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
        Public Sub AppendFrom(prov As IDataProvider) Implements IGroupMember.AppendFrom
            ' No extra data for a filtering group
        End Sub

        ''' <summary>
        '''  Can this group be deleted
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function CanDeleteGroup(ByRef reason As String) As Boolean Implements IGroup.CanDeleteGroup
            Return Source.CanDeleteGroup(reason)
        End Function

        ''' <summary>
        ''' Can access rights be changed for this group
        ''' </summary>
        ''' <param name="reason">If the rights cannot be changed, this returns the reason why</param>
        ''' <returns>True if the rights can be changed, otherwise False</returns>
        Public Overridable Function CanChangeAccessRights(ByRef reason As String) As Boolean Implements IGroup.CanChangeAccessRights
            Return Source.CanChangeAccessRights(reason)
        End Function

        ''' <summary>
        ''' Checks if the given user has the permission to view this group
        ''' </summary>
        ''' <param name="user">the user in question</param>
        ''' <returns>true if the user has permission</returns>
        Public Function HasViewPermission(user As IUser) As Boolean Implements IGroupMember.HasViewPermission
            Return Source.HasViewPermission(user)
        End Function

        ''' <summary>
        ''' Checks if this member can be removed from it's group by the passed user.
        ''' </summary>
        ''' <param name="user">The user intending to remove it</param>
        ''' <returns>True if the operation is allowed, otherwise False</returns>
        Public Function CanBeRemovedFromGroup(user As IUser) As Boolean Implements IGroupMember.CanBeRemovedFromGroup
            Return Source.CanBeRemovedFromGroup(user)
        End Function


        ''' <summary>
        ''' Extracts a localised name, only if the type is correct.  Otherwise the name is simply returned
        ''' </summary>
        ''' <param name="localiser">localisation function</param>
        ''' <returns>Display string</returns>
        Public Function GetLocalisedName(localiser As Func(Of IGroupMember, String)) As String Implements IGroupMember.GetLocalisedName
            'The it's the root item, then i could (will) be localised.
            If IsRoot Then
                Return localiser(Me)
            End If
            Return Name
        End Function

        ''' <summary>
        ''' Clear cache data related to the IGroupMember
        ''' </summary>
        Public Sub ClearLocalGroupCache() Implements IGroupMember.ClearLocalGroupCache
            mRefreshCache = True
        End Sub

        ''' <summary>
        ''' Validate that the state of the group is correct.
        ''' </summary>
        Public Sub ValidateGroupState() Implements IGroup.ValidateGroupState
            Source?.ValidateGroupState()
        End Sub

#End Region
    End Class
End Namespace
