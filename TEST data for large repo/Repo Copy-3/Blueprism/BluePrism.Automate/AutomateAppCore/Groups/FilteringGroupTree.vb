
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.TreeviewProcessing
Imports System.Runtime.InteropServices

Namespace Groups

    ''' <summary>
    ''' Special case of a group tree which applies some filtering to its contents
    ''' </summary>
    Public Class FilteringGroupTree : Implements IFilteringGroupTree

        ' The tree being filtered
        Private WithEvents mTree As IGroupTree

        ' The predicate which filters the tree
        Private mPredicate As Predicate(Of IGroupMember)

        ''' <summary>
        ''' Creats a new filtering tree based on a given source tree
        ''' </summary>
        ''' <param name="srcTree">The tree to filter within this new object.</param>
        ''' <param name="memFilter">The predicate which determines which group
        ''' members should be shown in the tree. Note that groups are shown
        ''' regardless of the filter, only non-group items will be passed through
        ''' the filter first.</param>
        ''' <param name="groupFilter">predicate to filter groups</param>
        ''' <param name="poolsAsGroups">show pools as groups or resources</param>
        Public Sub New(
         srcTree As IGroupTree,
         memFilter As Predicate(Of IGroupMember),
         groupFilter As Predicate(Of IGroup),
         poolsAsGroups As Boolean)
            If srcTree Is Nothing Then Throw New ArgumentNullException(NameOf(srcTree))

            ' only show pools as groups or members, never both.
            If poolsAsGroups Then
                Dim noPoolMembers As Predicate(Of IGroupMember) = Function(x)
                                                                      Dim rgm = TryCast(x, ResourceGroupMember)
                                                                      Return rgm Is Nothing OrElse Not rgm.IsPool
                                                                  End Function
                If memFilter IsNot Nothing Then
                    memFilter = memFilter.AndAlso(noPoolMembers)
                Else
                    memFilter = noPoolMembers
                End If
            Else
                Dim noPoolGroups As Predicate(Of IGroup) = Function(x)
                                                               Return TryCast(x, ResourcePool) Is Nothing
                                                           End Function
                If groupFilter IsNot Nothing Then
                    groupFilter = groupFilter.AndAlso(noPoolGroups)
                Else
                    groupFilter = noPoolGroups
                End If
            End If

            If memFilter Is Nothing Then memFilter = Function(m) True
            If groupFilter Is Nothing Then groupFilter = Function(m) True

            mTree = srcTree

            ' Apply the source tree's filter as well as the one being given to ensure
            ' that we correctly identify elements that belong in the filtered tree.
            Dim combinedFilter As Predicate(Of IGroupMember) = Function(m) (m.IsGroup AndAlso groupFilter(TryCast(m, IGroup))) _
                                                                    OrElse ((Not m.IsGroup) AndAlso memFilter(m))

            Dim filTree = TryCast(srcTree, IFilteringGroupTree)
            If filTree IsNot Nothing Then
                mPredicate = Function(m) filTree.PassesFilter(m) AndAlso combinedFilter(m)
            Else
                mPredicate = combinedFilter
            End If

        End Sub


        ''' <summary>
        ''' Gets the raw (ie. unfiltered) tree that is backing this filtering tree
        ''' </summary>
        Public ReadOnly Property RawTree As GroupTree Implements IGroupTree.RawTree
            Get
                Return mTree.RawTree
            End Get
        End Property

        ''' <summary>
        ''' Gets a filtered view of this tree using the given predicate.
        ''' </summary>
        ''' <param name="f">The predicate which determines the group members which
        ''' should be visible in the tree.</param>
        ''' <returns>A group tree using data from the source tree with a different
        ''' filter applied.
        ''' </returns>
        Public Function GetFilteredView(f As Predicate(Of IGroupMember)) As IGroupTree _
         Implements IGroupTree.GetFilteredView
            Return New FilteringGroupTree(mTree, f, Nothing, False)
        End Function

        ''' <summary>
        ''' Gets a filtered view of this tree using the given predicate.
        ''' </summary>
        ''' <param name="f">The predicate which determines the group members which
        ''' should be visible in the tree.</param>
        ''' <param name="g">predicate to filter groups</param>
        ''' <param name="poolsAsGroups">show pools as groups or members</param>
        ''' <returns>A group tree using data from the source tree with a different
        ''' filter applied.
        ''' </returns>
        Public Function GetFilteredView(f As Predicate(Of IGroupMember),
                                        g As Predicate(Of IGroup),
                                        poolsAsGroups As Boolean) As IGroupTree _
         Implements IGroupTree.GetFilteredView
            Return New FilteringGroupTree(mTree, f, g, poolsAsGroups)
        End Function

        ''' <summary>
        ''' Checks if the given group member passes the filter set in this tree.
        ''' </summary>
        ''' <param name="gm">The member to test</param>
        ''' <returns>True if the member passes the filter in this tree; False
        ''' otherwise</returns>
        Friend Function PassesFilter(gm As IGroupMember) As Boolean _
         Implements IFilteringGroupTree.PassesFilter

            Return mPredicate(gm)
        End Function

        ''' <summary>
        ''' Gets the group from the source tree with the given ID
        ''' </summary>
        ''' <param name="id">The ID of the required group</param>
        ''' <returns>The group, from the source tree, with the specified ID or null
        ''' if it could not be found there.</returns>
        Public Function GetSourceGroup(id As Guid) As IGroup _
         Implements IFilteringGroupTree.GetSourceGroup
            Return RawTree.Root.FindSubGroup(id)
        End Function

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
        Friend Function TryGetSource(
         tp As GroupMemberType, path As String, <Out> ByRef mem As IGroupMember) _
         As Boolean Implements IFilteringGroupTree.TryGetSource
            With RawTree.Root.FindMemberAtPath(GroupMemberType.Group, path)
                mem = .FoundMember
                Return .Success
            End With
        End Function

        ''' <summary>
        ''' Checks if the given user has edit permissions for this tree.
        ''' </summary>
        ''' <param name="u">The user to test to ensure they have the permissions
        ''' necessary to edit the tree</param>
        ''' <returns>True if the current user has the appropriate permissions to
        ''' edit the tree; False otherwise.</returns>
        Public Function HasEditPermission(u As User) As Boolean _
         Implements IGroupTree.HasEditPermission
            Return mTree.HasEditPermission(u)
        End Function

        ''' <summary>
        ''' The image key associated with this tree - this should match up to an
        ''' image in the <see cref="BluePrism.Images.ImageLists.Keys.Component"/>
        ''' image list.
        ''' </summary>
        Public ReadOnly Property ImageKey As String Implements IGroupTree.ImageKey
            Get
                Return mTree.ImageKey
            End Get
        End Property

        ''' <summary>
        ''' Gets the root group of this tree
        ''' </summary>
        Public ReadOnly Property Root As IGroup Implements IGroupTree.Root
            Get
                Return New FilteringGroup(Me, mTree.Root)
            End Get
        End Property

        Public ReadOnly Property Store As IGroupStore Implements IGroupTree.Store
            Get
                Return mTree.Store
            End Get
        End Property

        ''' <summary>
        ''' Gets the supported member types for this tree
        ''' </summary>
        Public ReadOnly Property SupportedMembers As ICollection(Of GroupMemberType) _
         Implements IGroupTree.SupportedMembers
            Get
                Return mTree.SupportedMembers
            End Get
        End Property

        ''' <summary>
        ''' Gets the tree type of this tree.
        ''' </summary>
        Public ReadOnly Property TreeType As GroupTreeType Implements IGroupTree.TreeType
            Get
                Return mTree.TreeType
            End Get
        End Property

        Public ReadOnly Property CombinedAttributes As TreeAttributes _
         Implements IGroupTree.CombinedAttributes
            Get
                Return mTree.CombinedAttributes
            End Get
        End Property

        ''' <summary>
        ''' Returns true if this tree has a default group.
        ''' A tree with a default group will prevent creating or / moving items (except for groups) in the root node.
        ''' Instead, items will be created in or moved into the default group.
        ''' This will return true even if the current user has no permissions on the default group, and cannot see it.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HasDefaultGroup As Boolean _
            Implements IGroupTree.HasDefaultGroup
            Get
                Return mTree.HasDefaultGroup
            End Get
        End Property

        ''' <summary>
        ''' Returns true if this tree contains a default group which the current user can see.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CanAccessDefaultGroup As Boolean _
            Implements IGroupTree.CanAccessDefaultGroup
            Get
                Return mTree.CanAccessDefaultGroup
            End Get
        End property


        ''' <summary>
        ''' Returns this tree's default group. Returns null if 
        ''' there isn't one, or if the current user is unable to see the default group.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property DefaultGroup As IGroup _
            Implements IGroupTree.DefaultGroup
            Get
                Return mTree.DefaultGroup
            End Get
        End Property


        ''' <summary>
        ''' Reloads this tree from the store
        ''' </summary>
        Public Sub Reload() Implements IGroupTree.Reload
            mTree.Reload()
        End Sub

    End Class

End Namespace
