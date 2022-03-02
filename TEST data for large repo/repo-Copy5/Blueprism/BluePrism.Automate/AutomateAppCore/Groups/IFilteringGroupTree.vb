
Imports System.Runtime.InteropServices

Namespace Groups

    ''' <summary>
    ''' Interface describing a group tree which is performing some form of filtering
    ''' </summary>
    Public Interface IFilteringGroupTree : Inherits IGroupTree

        ''' <summary>
        ''' Checks if a group members passes the filter defined in this tree
        ''' </summary>
        ''' <param name="m">The member to test against the filter</param>
        ''' <returns>True if the member passes the filter defined in this tree;
        ''' False otherwise.</returns>
        Function PassesFilter(m As IGroupMember) As Boolean

        ''' <summary>
        ''' Gets the group from the source tree with the given ID
        ''' </summary>
        ''' <param name="id">The ID of the required group</param>
        ''' <returns>The group, from the source tree, with the specified ID or null
        ''' if it could not be found there.</returns>
        Function GetSourceGroup(id As Guid) As IGroup

        ''' <summary>
        ''' Attempts to get the source group member from the raw tree that this
        ''' object provides a filtered view of.
        ''' </summary>
        ''' <param name="tp">The type of member to look for - if the value
        ''' <see cref="GroupMemberType.None"/> is provided, any single item at the
        ''' path will cause this method to return true; any other value for the type
        ''' will look for that type of member specifically and only return true if
        ''' such a member exists at the path.</param>
        ''' <param name="path">The path to the member</param>
        ''' <param name="mem">On exit, the group member that was found at the given
        ''' path or null if no such member was found in the source tree.</param>
        ''' <returns>True if a single item / group of the specified type can be found
        ''' in the model represented by this filtering tree at the specified path.
        ''' False otherwise.</returns>
        Function TryGetSource(
         tp As GroupMemberType, path As String, <Out> ByRef mem As IGroupMember) _
         As Boolean


    End Interface

End Namespace
