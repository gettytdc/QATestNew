Imports System.Windows.Forms
Imports BluePrism.BPCoreLib

Namespace Groups

    Public Interface IGroup : Inherits IGroupMember, ICollection(Of IGroupMember)

        ''' <summary>
        ''' Gets the raw (ie. unfiltered) group that is backing this group. For
        ''' unfiltered (ie. raw) groups, this property will return the instance
        ''' itself.
        ''' </summary>
        ReadOnly Property RawGroup As Group

        ''' <summary>
        ''' Gets the type of tree that this group sits within
        ''' </summary>
        ReadOnly Property TreeType As GroupTreeType

        ''' <summary>
        ''' The types of members supported by this group
        ''' </summary>
        ReadOnly Property SupportedTypes As ICollection(Of GroupMemberType)

        ''' <summary>
        ''' Is this group the default group for the tree
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsDefault As Boolean

        ''' <summary>
        ''' Does this group contain hidden members the user cannot access.
        ''' </summary>
        ''' <returns></returns>
        Property ContainsHiddenMembers As Boolean

        Property SortFieldName As String

        Property SortOrder As SortOrder

        Property Expanded As Boolean

        ''' <summary>
        ''' Adds a range of group members to this group
        ''' </summary>
        ''' <param name="members">The members to add to this group</param>
        Sub AddRange(members As IEnumerable(Of IGroupMember))

        ''' <summary>
        ''' Removes a range of group members from this group
        ''' </summary>
        ''' <param name="members">The members to remove from this group.</param>
        ''' <returns>True if any of the given members were removed from this group.
        ''' False otherwise.</returns>
        Function RemoveAll(members As IEnumerable(Of IGroupMember)) As Boolean

        ''' <summary>
        ''' Creates an empty subgroup within this group.
        ''' </summary>
        ''' <param name="name">The name of the group to create</param>
        ''' <returns>The newly created subgroup within this group.</returns>
        Function CreateGroup(name As String) As IGroup

        ''' <summary>
        ''' Updates a group's name
        ''' </summary>
        ''' <param name="newName">The new name to update to</param>
        Sub UpdateGroupName(newName As String)

        ''' <summary>
        ''' Updates a group's name with validation
        ''' </summary>
        ''' <param name="newName">The new name to update to</param>
        Sub UpdateGroupName(newName As String,oldName As String)

        ''' <summary>
        '''  Can this group be deleted
        ''' </summary>
        ''' <returns></returns>
        Function CanDeleteGroup(ByRef reason As String) As Boolean

        ''' <summary>
        ''' Can access rights be changed for this group
        ''' </summary>
        ''' <param name="reason">If the rights cannot be changed, this returns the reason why</param>
        ''' <returns>True if the rights can be changed, otherwise False</returns>
        Function CanChangeAccessRights(ByRef reason As String) As Boolean

        ''' <summary>
        ''' Validate that the state of the group is correct against the server
        ''' </summary>
        Sub ValidateGroupState()

    End Interface

End Namespace

