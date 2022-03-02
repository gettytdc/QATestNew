Imports BluePrism.AutomateAppCore.Auth

Namespace Groups

    ''' <summary>
    ''' Interface describing a tree containing groups and group members.
    ''' </summary>
    Public Interface IGroupTree

        ''' <summary>
        ''' Gets the raw (ie. unfiltered) tree that is backing this tree. For
        ''' unfiltered (ie. raw) trees, this property will return the instance itself
        ''' </summary>
        ReadOnly Property RawTree As GroupTree

        ''' <summary>
        ''' The type of tree this object represents.
        ''' </summary>
        ReadOnly Property TreeType As GroupTreeType

        ''' <summary>
        ''' Gets the image key associated with this tree, or an empty string if it
        ''' has no image key associated with it.
        ''' </summary>
        ReadOnly Property ImageKey As String

        ''' <summary>
        ''' Gets the member types supported by this tree
        ''' </summary>
        ReadOnly Property SupportedMembers As ICollection(Of GroupMemberType)

        ''' <summary>
        ''' The root of the group tree. Every tree starts with a single root, which
        ''' owns the top level groups and usually the items not assigned to any group
        ''' </summary>
        ReadOnly Property Root As IGroup

        ''' <summary>
        ''' The backing store in use for this group tree. This is automatically
        ''' called when updates to the tree structure are made.
        ''' </summary>
        ''' <remarks>This should never be null - if no store is set in this tree, a
        ''' <see cref="NullGroupStore"/> should be returned, effectively a group
        ''' store implementation which 'no-op's everything.</remarks>
        ReadOnly Property Store As IGroupStore

        ''' <summary>
        ''' Returns true if this tree has a default group.
        ''' A tree with a default group will prevent creating or / moving items (except for groups) in the root node.
        ''' Instead, items will be created in or moved into the default group.
        ''' This will return true even if the current user has no permissions on the default group, and cannot see it.
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property HasDefaultGroup As Boolean

        ''' <summary>
        ''' Returns true if this tree contains a default group which the current user can see.
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property CanAccessDefaultGroup As Boolean

        ReadOnly Property CombinedAttributes As TreeAttributes

        ''' <summary>
        ''' Returns this tree's default group. Returns null if 
        ''' there isn't one, or if the current user is unable to see the default group.
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property DefaultGroup As IGroup
        ''' <summary>
        ''' Checks to see if the given user has permission to edit this group tree.
        ''' </summary>
        ''' <param name="u">The user to check has permssion</param>
        Function HasEditPermission(u As User) As Boolean

        ''' <summary>
        ''' Gets a filtered view of this tree using the given predicate.
        ''' </summary>
        ''' <param name="f">The predicate which determines the group members which
        ''' should be visible in the tree.</param>
        ''' <returns>A group tree using data from this tree with a filter applied.
        ''' </returns>
        Function GetFilteredView(f As Predicate(Of IGroupMember)) As IGroupTree

        ''' <summary>
        ''' Gets a filtered view of this tree using the given predicate.
        ''' </summary>
        ''' <param name="f">The predicate which determines the group members which
        ''' should be visible in the tree.</param>
        ''' <param name="g">group filter predicate</param>
        ''' <param name="poolsAsGroups">switch to show pools as groups or members
        ''' </param>
        ''' <returns>A group tree using data from this tree with a filter applied.
        ''' </returns>
        Function GetFilteredView(f As Predicate(Of IGroupMember),
                                 g As Predicate(Of IGroup),
                                 poolsAsGroups As Boolean) As IGroupTree

        ''' <summary>
        ''' Reloads this tree from the backing store
        ''' </summary>
        Sub Reload()



    End Interface

End Namespace
