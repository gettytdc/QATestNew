Imports System.Runtime.Serialization

Namespace Groups

    ''' <summary>
    ''' A group which applies some filtering to its contents
    ''' </summary>
    <Serializable>
    <DataContract([Namespace]:="bp")>
    Public Class FilteringPool
        Inherits FilteringGroup

#Region " Constructors "

        ''' <summary>
        ''' Creates a new filtering group within the given tree from the given group.
        ''' </summary>
        ''' <param name="treeVal">The filtering tree which owns this group</param>
        ''' <param name="srcGroup">The group which provides the basis for this
        ''' filtering group</param>
        Friend Sub New(treeVal As IFilteringGroupTree, srcGroup As IGroup)
            MyBase.New(treeVal, srcGroup)
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Returns if the given group represents a pool
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property IsPool As Boolean
            Get
                Return True
            End Get
        End Property

        ''' <summary>
        ''' Gets the type of group member that this object represents
        ''' </summary>
        Public Overrides ReadOnly Property MemberType As GroupMemberType
            Get
                Return GroupMemberType.Pool
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the owner of this group
        ''' </summary>
        Public Overrides Property Owner As IGroup
            Get
                Dim gp = SourceOrNull
                If gp Is Nothing OrElse gp.Owner Is Nothing Then Return Nothing
                Return New FilteringPool(mTree, gp.Owner)
            End Get
            Set(value As IGroup)
                MyBase.Owner = value
            End Set
        End Property

#End Region

#Region " Methods "


        ''' <summary>
        ''' Gets a filtered view of this group member, using the given filtering
        ''' group tree to provide the filtering context for this group member.
        ''' </summary>
        ''' <param name="filtree">The filtering tree which the filtered view of the
        ''' member should form part of</param>
        ''' <returns>A clone of this group member which operates within the context
        ''' of the filtered tree: <paramref name="filtree"/>.</returns>
        Public Overrides Function GetFilteredView(filtree As IFilteringGroupTree) As IGroupMember
            Dim gp = MyBase.SourceOrNull
            If gp Is Nothing Then Return Nothing
            ' If the context is the same tree that we're already in, there's no need
            ' to create a new version of this group
            If filtree Is MyBase.Tree Then Return Me
            ' Otherwise, pass the source group into a new filtering group
            Return New FilteringPool(filtree, gp)
        End Function

        ''' <summary>
        ''' Adds a collection of group members to this group
        ''' </summary>
        ''' <param name="members">The members to add to this group</param>
        Public Overrides Sub AddRange(members As IEnumerable(Of IGroupMember))
            Throw New InvalidOperationException("You cannot currently add members under a pool")
        End Sub

        ''' <summary>
        ''' Creates a subgroup within this group.
        ''' </summary>
        ''' <param name="name">The name of the group to create</param>
        ''' <returns>The subgroup with the specified name created within this group.
        ''' </returns>
        Public Overrides Function CreateGroup(name As String) As IGroup
            Throw New InvalidOperationException("You cannot create a subgroup under a pool")
        End Function

        ''' <summary>
        ''' Deletes this group
        ''' </summary>
        Public Overrides Sub Delete()
            Throw New InvalidOperationException("You cannot delete a subgroup under a pool")
        End Sub

        ''' <summary>
        ''' Removes a collection of members from this group
        ''' </summary>
        ''' <param name="members">The members to remove</param>
        ''' <returns>True if the removal of the given members changed the contents
        ''' of this group.</returns>
        Public Overrides Function RemoveAll(members As IEnumerable(Of IGroupMember)) As Boolean
            Throw New InvalidOperationException("You cannot currently remove all on a pool")
        End Function

        ''' <summary>
        ''' Clones an orphaned version of this group, orphaned from any owner or
        ''' the tree.
        ''' </summary>
        ''' <returns>A filtered clone of this group, orphaned from the tree</returns>
        Public Overrides Function CloneOrphaned() As IGroupMember
            Throw New InvalidOperationException("You cannot perform this operation on a pool")
        End Function

        ''' <summary>
        ''' Removes this group from its owning group
        ''' </summary>
        Public Overrides Sub Remove()
            Throw New InvalidOperationException("You cannot perform this operation on a pool")
        End Sub

        ''' <summary>
        ''' Adds a group member to this group
        ''' </summary>
        ''' <param name="item">The member to add to this group</param>
        Public Overrides Sub Add(item As IGroupMember)
            Throw New InvalidOperationException("You cannot perform this operation on a pool")
        End Sub

        ''' <summary>
        ''' Removes all group members from this group.
        ''' </summary>
        Public Overrides Sub Clear()
            Throw New InvalidOperationException("You cannot perform this operation on a pool")
        End Sub

        ''' <summary>
        ''' Copies this group to the target group, returning the resultant group
        ''' member
        ''' </summary>
        ''' <param name="targetGroup">The group to which this group should be copied.
        ''' </param>
        ''' <returns>The new group member which resulted from the copy operation.
        ''' </returns>
        Public Overrides Function CopyTo(targetGroup As IGroup) As IGroupMember
            Throw New InvalidOperationException("You cannot copyTo on a pool")
        End Function

        ''' <summary>
        ''' Gets a string representation this group member.
        ''' </summary>
        ''' <returns>A String representation of this group member.</returns>
        Public Overrides Function ToString() As String
            Return "(Filtered) Pool:" & Name
        End Function

        ''' <summary>
        '''  Can this group be deleted
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function CanDeleteGroup(ByRef reason As String) As Boolean
            reason = "Pools cannot be delete in this way"
            Return False
        End Function

        ''' <summary>
        ''' Can access rights be changed for this group
        ''' </summary>
        ''' <param name="reason">If the rights cannot be changed, this returns the reason why</param>
        ''' <returns>True if the rights can be changed, otherwise False</returns>
        Public Overrides Function CanChangeAccessRights(ByRef reason As String) As Boolean
            reason = "Pools cannot have access rights"
            Return False
        End Function

#End Region

    End Class

End Namespace

