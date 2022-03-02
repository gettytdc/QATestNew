Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib.Collections
Imports System.Runtime.CompilerServices
Imports BluePrism.Server.Domain.Models
Imports BluePrism.BPCoreLib

Namespace Groups

    Public Module IGroupExtensions

#Region " Inner Classes "

        ''' <summary>
        ''' Class to hold the result from a path search in a group.
        ''' </summary>
        Friend Structure PathSearchResult
            Public Property FoundMember As IGroupMember
            Public Property FailGroup As IGroup
            Public Property FailName As String
            Private mMatches As ICollection(Of IGroupMember)
            Public Property FailGroupMatches As ICollection(Of IGroupMember)
                Set(value As ICollection(Of IGroupMember))
                    mMatches = value
                End Set
                Get
                    If mMatches Is Nothing Then _
                        Return GetEmpty.ICollection(Of IGroupMember)()
                    Return mMatches
                End Get
            End Property
            Public Property FailSpecificType As GroupMemberType
            Public ReadOnly Property Success As Boolean
                Get
                    Return (FoundMember IsNot Nothing)
                End Get
            End Property
        End Structure

#End Region

#Region " Private (Non-Extension) Methods "

        ''' <summary>
        ''' Gets the single owner of the given group members.
        ''' </summary>
        ''' <param name="mems">The members for which the single owner is required
        ''' </param>
        ''' <returns>The single owner of all the given members, or null if either:
        ''' there were no members -or- the members had no owner -or- the members had
        ''' any different owners</returns>
        Private Function GetSingleOwner(mems As IEnumerable(Of IGroupMember)) As IGroup
            ' We're trying to find the *single* owner of all the members
            Dim own As IGroup = Nothing
            For Each owningGp As IGroup In mems.Select(Function(gm) gm.Owner)
                ' If own is not set, set it now and continue in the loop
                If own Is Nothing Then own = owningGp : Continue For
                ' If it is, but not to this member's owner then we have > 1 owners,
                ' set to null and exit - we're not interested in multiple owners
                If own IsNot owningGp Then Return Nothing
            Next
            Return own
        End Function

#End Region

        ''' <summary>
        ''' If we are filtering a tree should we show this group.
        ''' </summary>
        ''' <param name="this">the group in question</param>
        ''' <param name="showingAllUnRestrictedGroups">Whether to show all unrestricted sub groups</param>
        ''' <returns>whether or not to show this group on the tree</returns>
        <Extension>
        Public Function ShouldShow(this As IGroup, showingAllUnRestrictedGroups As Boolean) As Boolean
            Return showingAllUnRestrictedGroups OrElse
                   Not (this.ContainsHiddenMembers AndAlso this.Count = 0)
        End Function


        ''' <summary>
        ''' Extension method to combine two predicates in a short circuited logical AND
        ''' </summary>
        ''' <typeparam name="T">the type of the predicate</typeparam>
        ''' <param name="left">The LHS of the AND</param>
        ''' <param name="right">The RHS of the AND</param>
        ''' <returns>A combined predicate</returns>
        <Extension>
        Public Function [AndAlso](Of T)(left As Predicate(Of T), right As Predicate(Of T)) As Predicate(Of T)
            Return Function(x)
                       Return left(x) AndAlso right(x)
                   End Function
        End Function

        ''' <summary>
        ''' Returns the subgroup with the passed ID. Note this also recursively
        ''' checks descendants too.
        ''' </summary>
        ''' <param name="groupID">The group ID to look for</param>
        ''' <returns>The group (or Nothing if not found)</returns>
        <Extension>
        Public Function FindSubGroup(this As IGroup, groupID As Guid) As IGroup
            Return DirectCast(SearchFirst(
             this, Function(m) m.IsGroup AndAlso m.IdAsGuid = groupID), IGroup)
        End Function

        ''' <summary>
        ''' Gets all owners of the given group member within this group and its
        ''' subtree.
        ''' </summary>
        ''' <param name="mem">The member for whom all owning groups is required.
        ''' </param>
        ''' <returns>A collection of all groups which contain the given group member.
        ''' </returns>
        <Extension>
        Public Function FindAllOwnersOf(
         this As IGroup, mem As IGroupMember) As ICollection(Of IGroup)
            Dim groups As New clsSet(Of IGroup)
            Scan(Of IGroup)(this, Sub(g) If g.Contains(mem) Then groups.Add(g))
            Return groups
        End Function

        ''' <summary>
        ''' Recursively searches this group (using a depth first search) for members
        ''' matching a predicate.
        ''' </summary>
        ''' <param name="del">The predicate determining whether a member is matched
        ''' or not</param>
        ''' <returns>A collection of group members which matched the predicate.
        ''' </returns>
        <Extension>
        Public Function Search(
         this As IGroup, del As Func(Of IGroupMember, Boolean)) _
         As ICollection(Of IGroupMember)
            Return Search(this, True, del)
        End Function

        ''' <summary>
        ''' Recursively searches this group for members matching a predicate.
        ''' </summary>
        ''' <param name="pred">The predicate determining whether a member is matched
        ''' or not</param>
        ''' <param name="depthFirst">True to perform a depth-first search; False to
        ''' perform a breadth first search (basically, search subgroups first or
        ''' search leaf elements first)</param>
        ''' <returns>A collection of group members which matched the predicate.
        ''' </returns>
        <Extension>
        Public Function Search(
         this As IGroup,
         depthFirst As Boolean,
         pred As Func(Of IGroupMember, Boolean)) As ICollection(Of IGroupMember)
            Dim matches As New List(Of IGroupMember)
            Scan(this, depthFirst, Sub(m) If pred(m) Then matches.Add(m))
            Return matches
        End Function

        ''' <summary>
        ''' Finds the group member within this group and its subtree with the given
        ''' ID.
        ''' </summary>
        ''' <param name="id">The ID to search for within this group and is subtree.
        ''' </param>
        ''' <returns>The group member corresponding to the given ID or null if no
        ''' such group member was found</returns>
        <Extension>
        Public Function FindById(this As IGroup, id As Object) As IGroupMember
            Return SearchFirst(this, Function(m) Object.Equals(m.Id, id))
        End Function

        ''' <summary>
        ''' Finds all group members within this group and its subtree with the given
        ''' ID.
        ''' </summary>
        ''' <param name="id">The ID to search for within this group and is subtree.
        ''' </param>
        ''' <returns>The group members corresponding to the given ID or an empty
        ''' collection if no such group member was found</returns>
        <Extension>
        Public Function FindAllById(
         this As IGroup, id As Object) As ICollection(Of IGroupMember)
            Return Search(this, Function(m) Object.Equals(m.Id, id))
        End Function

        ''' <summary>
        ''' Recursively searches this group (in depth-first order) for any elements
        ''' which have a specific member type
        ''' </summary>
        ''' <param name="this">The group to search in</param>
        ''' <param name="tp">The member type to search for</param>
        ''' <returns>True if a group member of the specified type was found in this
        ''' group or its subtree</returns>
        <Extension>
        Public Function ContainsInSubtree(
         this As IGroup, tp As GroupMemberType) As Boolean
            Return ContainsInSubtree(this, Function(m) m.MemberType = tp)
        End Function

        ''' <summary>
        ''' Recursively searches this group (in depth-first order) for any elements
        ''' which match a predicate
        ''' </summary>
        ''' <param name="this">The group to search in</param>
        ''' <param name="pred">The predicate to match</param>
        ''' <returns>True if a group member was found in this group or its subtree
        ''' which matched the predicate</returns>
        <Extension>
        Public Function ContainsInSubtree(
         this As IGroup, pred As Func(Of IGroupMember, Boolean)) As Boolean
            Return (this.SearchFirst(pred) IsNot Nothing)
        End Function

        ''' <summary>
        ''' Recursively searches this group (in depth-first order) for the first
        ''' member matching a predicate
        ''' </summary>
        ''' <param name="pred">The predicate determining whether a member is matched
        ''' or not</param>
        ''' <returns>The group member which matched the predicate, or null if no such
        ''' group member was found in this group or its subgroups.</returns>
        <Extension>
        Public Function SearchFirst(
         this As IGroup, pred As Func(Of IGroupMember, Boolean)) As IGroupMember
            Return SearchFirst(this, True, pred)
        End Function

        ''' <summary>
        ''' Recursively searches this group for the first member matching a predicate
        ''' </summary>
        ''' <param name="pred">The predicate determining whether a member is matched
        ''' or not</param>
        ''' <param name="depthFirst">True to perform a depth-first search; False to
        ''' perform a breadth first search (basically, search subgroups first or
        ''' search leaf elements first)</param>
        ''' <returns>The group member which matched the predicate, or null if no such
        ''' group member was found in this group or its subgroups.</returns>
        <Extension>
        Public Function SearchFirst(
         this As IGroup,
         depthFirst As Boolean,
         pred As Func(Of IGroupMember, Boolean)) As IGroupMember
            Return Scan(this, depthFirst, Function(m) pred(m))
        End Function

        ''' <summary>
        ''' Calls a delegate on all group members of a specified type in this group.
        ''' Note that this operates only on the direct children of this group, not
        ''' on the entire subtree
        ''' </summary>
        ''' <typeparam name="T">The type of group member to act upon. Only members of
        ''' this type will be passed to the action delegate</typeparam>
        ''' <param name="act">The delegate describing the action to perform</param>
        <Extension>
        Public Sub ForEach(Of T As {IGroupMember, Class})(
         this As IGroup, act As Action(Of T))
            ForEach(this, Sub(m) If TypeOf m Is T Then act(DirectCast(m, T)))
        End Sub

        ''' <summary>
        ''' Calls a delegate on all group members in this group. Note that this
        ''' operates only on the direct children of this group, not on the entire
        ''' subtree
        ''' </summary>
        ''' <param name="act">The delegate describing the action to perform</param>
        <Extension>
        Public Sub ForEach(this As IGroup, act As Action(Of IGroupMember))
            For Each mem As IGroupMember In this : act(mem) : Next
        End Sub

        ''' <summary>
        ''' Performs a recursive scan on all members in this group of the given type,
        ''' in depth-first order.
        ''' </summary>
        ''' <typeparam name="T">The type of group member to operate on. Any members
        ''' which are not of this type will not be actioned by this scan (though they
        ''' will still be traversed - eg. a group will be searched even if 'Group' is
        ''' not the type being scanned)</typeparam>
        ''' <param name="act">The delegate which performs the action on the member.
        ''' </param>
        <Extension>
        Public Sub Scan(Of T As {IGroupMember})(
         this As IGroup, act As Action(Of T))
            Scan(Of T)(this, True, act)
        End Sub

        ''' <summary>
        ''' Performs a recursive scan on all members in this group of the given type.
        ''' </summary>
        ''' <typeparam name="T">The type of group member to operate on. Any members
        ''' which are not of this type will not be actioned by this scan (though they
        ''' will still be traversed - eg. a group will be searched even if 'Group' is
        ''' not the type being scanned)</typeparam>
        ''' <param name="act">The delegate which performs the action on the member.
        ''' </param>
        <Extension>
        Public Sub Scan(Of T As {IGroupMember})(
         this As IGroup, depthFirst As Boolean, act As Action(Of T))
            Scan(this, depthFirst,
                Sub(m)
                    If TypeOf m Is T Then act(DirectCast(m, T))
                End Sub)
        End Sub

        ''' <summary>
        ''' Performs a recursive scan on all members in this group, in depth-first
        ''' order.
        ''' </summary>
        ''' <param name="act">The delegate which performs the action on the member.
        ''' </param>
        <Extension>
        Public Sub Scan(this As IGroup, act As Action(Of IGroupMember))
            Scan(this, True, act)
        End Sub

        ''' <summary>
        ''' Performs a recursive scan on all members in this group.
        ''' </summary>
        ''' <param name="act">The delegate which performs the action on the member.
        ''' </param>
        ''' <param name="depthFirst">True to perform the scan in depth-first order;
        ''' False to perform the scan in breadth-first order.</param>
        <Extension>
        Public Sub Scan(
         this As IGroup, depthFirst As Boolean, act As Action(Of IGroupMember))
            ' The mechanics of the scan is exactly the same as an interruptible scan
            ' only without the possibility of interruption...
            Scan(this, depthFirst,
                Function(m)
                    act(m)
                    Return False
                End Function
            )
        End Sub

        ''' <summary>
        ''' Performs a recursive scan (which can be interrupted by the delegate
        ''' function if it finds what it is looking for) on all members in this group
        ''' </summary>
        ''' <param name="fun">The function to determine whether to interrupt the scan
        ''' or not which performs the action on the member. If the delegate returns
        ''' 'false', the scan will continue; if the delegate returns 'true' the scan
        ''' will be interrupted and the groupmember that the delegate returned 'true'
        ''' for is returned by this method.
        ''' </param>
        ''' <param name="depthFirst">True to perform the scan in depth-first order;
        ''' False to perform the scan in breadth-first order.</param>
        ''' <returns>The member on which the scan was interrupted, or null if the
        ''' scan was not actually interrupted while it was executing.</returns>
        <Extension>
        Public Function Scan(
         this As IGroup,
         depthFirst As Boolean,
         fun As Func(Of IGroupMember, Boolean)) As IGroupMember
            ' Very first thing, we call the action on this node
            If fun(this) Then Return this

            ' Keep the skipped members separate so we don't have to traverse our
            ' whole list twice (not much of a saving, but still)
            Dim skipped As New List(Of IGroupMember)(this.Count)

            ' Handle the first pass directly on the contents collection
            For Each m As IGroupMember In this
                Dim innerGp As IGroup = TryCast(m, IGroup)
                If innerGp Is Nothing AndAlso Not depthFirst Then
                    ' member / breadth-first: process in this pass
                    If fun(m) Then Return m

                ElseIf innerGp IsNot Nothing AndAlso depthFirst Then
                    ' group / depth-first: process in this pass
                    Dim ret As IGroupMember = innerGp.Scan(depthFirst, fun)
                    If ret IsNot Nothing Then Return ret

                Else
                    ' member + depth-first or group + breadth-first;
                    ' either way, skip it in this pass
                    skipped.Add(m)

                End If
            Next

            ' Handle the second pass from the skipped collection
            For Each m As IGroupMember In skipped
                Dim innerGp As IGroup = TryCast(m, IGroup)
                If innerGp Is Nothing Then
                    If fun(m) Then Return m
                Else
                    Dim ret As IGroupMember = innerGp.Scan(depthFirst, fun)
                    If ret IsNot Nothing Then Return ret
                End If
            Next

            ' We got all the way through this group without interruption:
            ' nothing to return
            Return Nothing

        End Function

        ''' <summary>
        ''' Gets the group member from a group with a specified path.
        ''' </summary>
        ''' <param name="this">The group from which to retrieve the member</param>
        ''' <param name="path">The path to the member required, separated by '/'. If
        ''' any path element names contain a '/', the element should be wrapped in
        ''' quotes; quotes characters within element names can be escaped by doubling
        ''' them - this is only necessary if the element name is being wrapped in
        ''' quotes because it contains a '/' or if the name begins with a quotes
        ''' character.</param>
        ''' <returns>The group member found at the given path, relative to this
        ''' group. From the final group in the path, this method will prefer an
        ''' item (ie. non-group) member over a subgroup if one of each exists with
        ''' the same name; if only a group with the name exists, that will be
        ''' returned.</returns>
        ''' <exception cref="ArgumentNullException">If <paramref name="path"/> is
        ''' null.</exception>
        ''' <exception cref="ArgumentException">If the path has no entries in it.
        ''' </exception>
        ''' <exception cref="NoSuchElementException">If any subgroups/members with
        ''' names matching those in the path could not be found in the model.
        ''' </exception>
        ''' <exception cref="TooManyElementsException">If the final name in the path
        ''' matched more than 1 non-group item in the last subgroup searched.
        ''' </exception>
        ''' <exception cref="Server.Domain.Models.GroupException">If the first element in
        ''' <paramref name="path"/> is not this group.</exception>
        <Extension>
        Public Function GetMemberAtPath(this As IGroup, path As String) As IGroupMember
            Return GetMemberAtPath(this, GroupMemberType.None, path.SplitQuoted("/"c))
        End Function

        ''' <summary>
        ''' Gets the group member from a group with a specified path.
        ''' </summary>
        ''' <param name="this">The group from which to retrieve the member</param>
        ''' <param name="tp">The type of group member to return, or
        ''' <see cref="GroupMemberType.None"/> to return the member there with the
        ''' specified name.</param>
        ''' <param name="path">The path to the member required, separated by '/'. If
        ''' any path element names contain a '/', the element should be wrapped in
        ''' quotes; quotes characters within element names can be escaped by doubling
        ''' them - this is only necessary if the element name is being wrapped in
        ''' quotes because it contains a '/' or if the name begins with a quotes
        ''' character.</param>
        ''' <returns>The group member found at the given path, relative to this
        ''' group. From the final group in the path, if no <paramref name="tp">type
        ''' </paramref> is given, this method will prefer a non-group member over a
        ''' subgroup if one of each exists with the same name; if only a group with
        ''' the name exists, that will be returned.</returns>
        ''' <exception cref="ArgumentNullException">If <paramref name="path"/> is
        ''' null.</exception>
        ''' <exception cref="NoSuchElementException">If any subgroups/members with
        ''' names matching those in the path could not be found in the model.
        ''' </exception>
        ''' <exception cref="TooManyElementsException">If the final name in the path
        ''' matched more than 1 non-group item in the last subgroup searched.
        ''' </exception>
        <Extension>
        Public Function GetMemberAtPath(
         this As IGroup, tp As GroupMemberType, path As String) As IGroupMember
            Return GetMemberAtPath(this, tp, path.SplitQuoted("/"c))
        End Function

        ''' <summary>
        ''' Gets the group member from a group with a specified path.
        ''' </summary>
        ''' <param name="this">The group from which to retrieve the member</param>
        ''' <param name="path">The path to the member required, separated by '/'. If
        ''' any path element names contain a '/', the element should be wrapped in
        ''' quotes; quotes characters within element names can be escaped by doubling
        ''' them - this is only necessary if the element name is being wrapped in
        ''' quotes because it contains a '/' or if the name begins with a quotes
        ''' character.</param>
        ''' <returns>The group member found at the given path, relative to this
        ''' group. From the final group in the path, this method will prefer an
        ''' item (ie. non-group) member over a subgroup if one of each exists with
        ''' the same name; if only a group with the name exists, that will be
        ''' returned.</returns>
        ''' <exception cref="ArgumentNullException">If <paramref name="path"/> is
        ''' null.</exception>
        ''' <exception cref="NoSuchElementException">If any subgroups/members with
        ''' names matching those in the path could not be found in the model.
        ''' </exception>
        ''' <exception cref="TooManyElementsException">If the final name in the path
        ''' matched more than 1 non-group item in the last subgroup searched.
        ''' </exception>
        <Extension>
        Friend Function GetMemberAtPath(
         this As IGroup, path As IEnumerable(Of String)) As IGroupMember
            Return GetMemberAtPath(this, GroupMemberType.None, path)
        End Function

        ''' <summary>
        ''' Gets the group member from a group with a specified path.
        ''' </summary>
        ''' <param name="this">The group from which to retrieve the member</param>
        ''' <param name="type">The type of group member to return, or
        ''' <see cref="GroupMemberType.None"/> to return the member there with the
        ''' specified name.</param>
        ''' <param name="path">The path to the member required, separated by '/'. If
        ''' any path element names contain a '/', the element should be wrapped in
        ''' quotes; quotes characters within element names can be escaped by doubling
        ''' them - this is only necessary if the element name is being wrapped in
        ''' quotes because it contains a '/' or if the name begins with a quotes
        ''' character.</param>
        ''' <returns>The group member found at the given path, relative to this
        ''' group. From the final group in the path, if no <paramref name="type"/> is
        ''' given, this method will prefer a non-group member over a subgroup if one
        ''' of each exists with the same name; if only a group with the name exists,
        ''' that will be returned. If the enumerable contains no elements, this
        ''' group will be returned.</returns>
        ''' <exception cref="ArgumentNullException">If <paramref name="path"/> is
        ''' null.</exception>
        ''' <exception cref="NoSuchElementException">If any subgroups/members with
        ''' names matching those in the path could not be found in the model.
        ''' </exception>
        ''' <exception cref="TooManyElementsException">If the final name in the path
        ''' matched more than 1 non-group item in the last subgroup searched.
        ''' </exception>
        <Extension>
        Friend Function GetMemberAtPath(
         this As IGroup, type As GroupMemberType, path As IEnumerable(Of String)) _
         As IGroupMember
            Dim result As PathSearchResult = FindMemberAtPath(this, type, path)
            If result.Success Then Return result.FoundMember

            ' Otherwise it could be a number of things... 
            ' Check for missing subgroup
            If result.FailSpecificType = GroupMemberType.Group AndAlso
             result.FailGroupMatches.Count = 0 Then
                Throw New NoSuchElementException(
                 "No subgroup named '{0}' found in group '{1}'.",
                 result.FailName, result.FailGroup.Path())

                ' Now... generic missing element
            ElseIf result.FailSpecificType = GroupMemberType.None AndAlso
             result.FailGroupMatches.Count = 0 Then
                Throw New NoSuchElementException(
                 "No member named '{0}' found in group '{1}'",
                 result.FailName, result.FailGroup.Path())

                ' And specific missing element
            ElseIf result.FailGroupMatches.Count = 0 Then
                Throw New NoSuchElementException(
                 "No member of type {0} named '{1}' found in group '{2}'",
                 result.FailSpecificType, result.FailName, result.FailGroup.Path())

                ' And, finally, too many elements
            Else
                Throw New TooManyElementsException(
                 "Too many items found with name '{0}' in group '{1}'. " &
                 "Types found:{2}{3}",
                 result.FailName, result.FailGroup.Path(), vbCrLf,
                 String.Join(",", result.FailGroupMatches.Select(Function(m) m.MemberType.ToString())))

            End If

        End Function

        ''' <summary>
        ''' Checks whether a member can be found from a group with a specified path.
        ''' </summary>
        ''' <param name="this">The group from which to retrieve the member</param>
        ''' <param name="type">The type of group member to return, or
        ''' <see cref="GroupMemberType.None"/> to return the member there with the
        ''' specified name.</param>
        ''' <param name="path">The path to the member required, separated by '/'. If
        ''' any path element names contain a '/', the element should be wrapped in
        ''' quotes; quotes characters within element names can be escaped by doubling
        ''' them - this is only necessary if the element name is being wrapped in
        ''' quotes because it contains a '/' or if the name begins with a quotes
        ''' character.</param>
        ''' <returns>True if a single member was identified at
        ''' <paramref name="path"/>; False otherwise (note that if
        ''' <paramref name="type"/> is <see cref="GroupMemberType.None"/> and there
        ''' exist multiple members of different types at that path, this will return
        ''' false to indicate that a <em>single</em> group member could not be
        ''' identified at that path.</returns>
        <Extension>
        Public Function HasMemberAtPath(
         this As IGroup, type As GroupMemberType, path As String) As Boolean
            Return HasMemberAtPath(this, type, path.SplitQuoted("/"c))
        End Function

        ''' <summary>
        ''' Checks whether a member can be found from a group with a specified path.
        ''' </summary>
        ''' <param name="this">The group from which to retrieve the member</param>
        ''' <param name="path">The path to the member required, separated by '/'. If
        ''' any path element names contain a '/', the element should be wrapped in
        ''' quotes; quotes characters within element names can be escaped by doubling
        ''' them - this is only necessary if the element name is being wrapped in
        ''' quotes because it contains a '/' or if the name begins with a quotes
        ''' character.</param>
        ''' <returns>True if a single member was identified at
        ''' <paramref name="path"/>; False otherwise (note that if there exist
        ''' multiple members of different types at that path, this will return
        ''' false to indicate that a <em>single</em> group member could not be
        ''' identified at that path.</returns>
        <Extension>
        Public Function HasMemberAtPath(this As IGroup, path As String) As Boolean
            Return HasMemberAtPath(this, GroupMemberType.None, path.SplitQuoted("/"c))
        End Function

        ''' <summary>
        ''' Checks whether a member can be found from a group with a specified path.
        ''' </summary>
        ''' <param name="this">The group from which to retrieve the member</param>
        ''' <param name="type">The type of group member to return, or
        ''' <see cref="GroupMemberType.None"/> to return the member there with the
        ''' specified name.</param>
        ''' <param name="path">The path to the member required, separated by '/'. If
        ''' any path element names contain a '/', the element should be wrapped in
        ''' quotes; quotes characters within element names can be escaped by doubling
        ''' them - this is only necessary if the element name is being wrapped in
        ''' quotes because it contains a '/' or if the name begins with a quotes
        ''' character.</param>
        ''' <returns>True if a single member was identified at
        ''' <paramref name="path"/>; False otherwise (note that if
        ''' <paramref name="type"/> is <see cref="GroupMemberType.None"/> and there
        ''' exist multiple members of different types at that path, this will return
        ''' false to indicate that a <em>single</em> group member could not be
        ''' identified at that path.</returns>
        <Extension>
        Private Function HasMemberAtPath(
         this As IGroup, type As GroupMemberType, path As IEnumerable(Of String)) _
         As Boolean
            Return FindMemberAtPath(this, type, path).Success
        End Function

        ''' <summary>
        ''' Finds the group member from a group with a specified path.
        ''' </summary>
        ''' <param name="this">The group from which to retrieve the member</param>
        ''' <param name="type">The type of group member to return, or
        ''' <see cref="GroupMemberType.None"/> to return the member there with the
        ''' specified name.</param>
        ''' <param name="path">The path to the member required, separated by '/'. If
        ''' any path element names contain a '/', the element should be wrapped in
        ''' quotes; quotes characters within element names can be escaped by doubling
        ''' them - this is only necessary if the element name is being wrapped in
        ''' quotes because it contains a '/' or if the name begins with a quotes
        ''' character.</param>
        ''' <returns>The result of the search - if successful, then
        ''' <see cref="PathSearchResult.FoundMember"/> will return the member found
        ''' at the given path, relative to this group. From the final group in the
        ''' path, if no <paramref name="type"/> is given, this method will prefer a
        ''' non-group member over a subgroup if one of each exists with the same
        ''' name; if only a group with the name exists, that will be returned.
        ''' If the enumerable contains no elements, this group will be returned.
        ''' If any part of the path navigation failed, details of the failure will
        ''' be returned in the various other properties of the returned object.
        ''' </returns>
        ''' <exception cref="ArgumentNullException">If <paramref name="path"/> is
        ''' null.</exception>
        <Extension>
        Friend Function FindMemberAtPath(
         this As IGroup,
         type As GroupMemberType,
         path As String) As PathSearchResult
            Return FindMemberAtPath(this, type, path.SplitQuoted("/"c))
        End Function

        ''' <summary>
        ''' Finds the group member from a group with a specified path.
        ''' </summary>
        ''' <param name="this">The group from which to retrieve the member</param>
        ''' <param name="type">The type of group member to return, or
        ''' <see cref="GroupMemberType.None"/> to return the member there with the
        ''' specified name.</param>
        ''' <param name="path">The path to the member required, separated by '/'. If
        ''' any path element names contain a '/', the element should be wrapped in
        ''' quotes; quotes characters within element names can be escaped by doubling
        ''' them - this is only necessary if the element name is being wrapped in
        ''' quotes because it contains a '/' or if the name begins with a quotes
        ''' character.</param>
        ''' <returns>The result of the search - if successful, then
        ''' <see cref="PathSearchResult.FoundMember"/> will return the member found
        ''' at the given path, relative to this group. From the final group in the
        ''' path, if no <paramref name="type"/> is given, this method will prefer a
        ''' non-group member over a subgroup if one of each exists with the same
        ''' name; if only a group with the name exists, that will be returned.
        ''' If the enumerable contains no elements, this group will be returned.
        ''' If any part of the path navigation failed, details of the failure will
        ''' be returned in the various other properties of the returned object.
        ''' </returns>
        ''' <exception cref="ArgumentNullException">If <paramref name="path"/> is
        ''' null.</exception>
        <Extension>
        Private Function FindMemberAtPath(
         this As IGroup,
         type As GroupMemberType,
         path As IEnumerable(Of String)) As PathSearchResult

            If path Is Nothing Then Throw New ArgumentNullException(NameOf(path))
            Dim pathCount As Integer = path.Count()
            If pathCount = 0 Then Return New PathSearchResult() With {
                .FoundMember = this
            }

            ' The current group being searched within the iteration of the loop
            Dim curr As IGroup = this
            ' The previous group searched, typically the owner of 'curr'
            Dim prev As IGroup = Nothing
            ' The iteration counter
            Dim iter As Integer = 0
            ' Flag to register an initial blank entry (ie. starts with "/...")
            Dim firstEntryBlank As Boolean = False
            ' The matches found within the last iteration of the loop
            Dim matches As New List(Of IGroupMember)
            ' The name searched for (and found) in the previous iteration of the loop
            Dim lastName As String = Nothing

            ' Go through each name in the path; find the members whose name match
            ' that name. If there are more names, use the member which is a group;
            ' If no match was found, or none of the matches were a group, error
            For Each name As String In path
                iter += 1
                ' If we don't have a group at this stage, then the 'prev' group
                ' didn't contain an IGroup with the name 'lastName'
                If curr Is Nothing Then Return New PathSearchResult() With {
                    .FailGroup = prev,
                    .FailName = lastName,
                    .FailSpecificType = GroupMemberType.Group
                }

                lastName = name

                If name = "" Then
                    ' We allow the first path element to be blank (starts with '/')
                    If iter = 1 Then
                        ' If there's only one entry in the path and it's blank:
                        ' treat that as 'return this group'
                        If pathCount = 1 Then Return New PathSearchResult() With {
                            .FoundMember = this
                        }
                        ' Otherwise move onto elem 2
                        Continue For
                    End If

                    ' We also allow the last path element to be blank - it means
                    ' to return the group specified (eg. "/Group 1/Subgroup 2/" means 
                    ' to return the group named 'Subgroup 2' under 'Group 1'.
                    If iter = pathCount Then Return New PathSearchResult() With {
                        .FoundMember = curr
                    }

                    ' Blanks anywhere else are invalid and should throw NoSuchElems

                End If

                matches.Clear()

                ' Search the current group for the next name in the path
                For Each m As IGroupMember In curr
                    If m.Name = name Then matches.Add(m)
                Next

                If matches.Count = 0 Then Return New PathSearchResult() With {
                    .FailGroup = curr,
                    .FailName = name
                }

                prev = curr
                curr = TryCast(matches.FirstOrDefault(Function(m) m.IsGroup), IGroup)

            Next

            ' Out of the loop, matches should contain the list of matched members for
            ' the last entry in the path.

            ' If the caller specified a type, find the match which has that type, or
            ' error if none of them did
            If type <> GroupMemberType.None Then
                Dim found = matches.FirstOrDefault(Function(m) m.MemberType = type)
                If found Is Nothing Then Return New PathSearchResult() With {
                    .FailGroup = curr,
                    .FailName = lastName,
                    .FailSpecificType = type
                }

                Return New PathSearchResult() With {.FoundMember = found}

            Else
                ' Otherwise, we want to return the match found for that last element.
                ' If there is a single item (ie. non-group) with the name, we return
                ' that, regardless of whether a group with the same name exists.
                ' If no item exists, but a group with the name exists, we return that
                ' Otherwise (ie. >1 non-group match) we raise an error indicating
                ' that we can't return a single member which matches that path

                ' Separate out all the non-groups into a different list
                Dim items = matches.Where(Function(m) Not m.IsGroup).ToList()

                ' If there is one item, return that
                If items.Count = 1 Then Return New PathSearchResult() With {
                    .FoundMember = items(0)
                }

                ' If there are no items, but there is a group, return the group
                If items.Count = 0 AndAlso matches.Count = 1 Then _
                    Return New PathSearchResult() With {.FoundMember = matches(0)}

                ' We shouldn't be here unless we have multiple non-group matches
                Debug.Assert(items.Count > 1)

                ' If > 1 match with the name and no type specified, prefer 
                Return New PathSearchResult() With {
                    .FailGroup = curr,
                    .FailName = lastName,
                    .FailGroupMatches = matches
                }

            End If

        End Function

        ''' <summary>
        ''' Checks if this group contains all of the given group members.
        ''' </summary>
        ''' <param name="mems">The members to check this group for</param>
        ''' <returns>True if this group contains the all of the given members, false
        ''' otherwise. Note that passing an empty collection will return true.
        ''' </returns>
        <Extension>
        Public Function ContainsAll(
         this As IGroup, mems As IEnumerable(Of IGroupMember)) As Boolean
            For Each mem As IGroupMember In mems
                If Not this.Contains(mem) Then Return False
            Next
            Return True
        End Function

        ''' <summary>
        ''' Gets the contents of this group and all subgroups flattened into a single
        ''' collection of group members. Note that the returned collection will not
        ''' contain any groups - only the members of this and all subgroups.
        ''' </summary>
        ''' <typeparam name="T">The type of ICollection that this should return</typeparam>
        ''' <returns>A readonly ICollection of IGroupMember</returns>
        <Extension>
        Public Function FlattenedContents(Of T As {New, ICollection(Of IGroupMember)})(this As IGroup, poolsAsGroups As Boolean) _
         As ICollection(Of IGroupMember)
            Dim collection = New T()
            this.FlattenInto(collection, poolsAsGroups)
            Return GetReadOnly.ICollection(collection)
        End Function

        ''' <summary>
        ''' Recursively flattens this group's contents into a collection, ie. adds
        ''' this group and all its descendants' contents into the collection, not
        ''' including any groups themselves.
        ''' </summary>
        ''' <param name="coll">The collection into which this group should be added.
        ''' </param>
        <Extension>
        Friend Sub FlattenInto(this As IGroup, coll As ICollection(Of IGroupMember), poolsAsGroups As Boolean)
            For Each mem As IGroupMember In this
                Dim g As IGroup = TryCast(mem, IGroup)

                If poolsAsGroups AndAlso g IsNot Nothing AndAlso g.MemberType = GroupMemberType.Pool Then
                    coll.Add(mem)
                Else
                    If g IsNot Nothing Then g.FlattenInto(coll, poolsAsGroups) Else coll.Add(mem)
                End If
            Next
        End Sub

        ''' <summary>
        ''' Checks if the <see cref="User.Current">current user</see> is able to move
        ''' the given group members into this group. This is possible if they have
        ''' edit permission for this tree and the group members are not already all
        ''' members of this group.
        ''' </summary>
        ''' <param name="mems">The members to check to see if they can be moved into
        ''' this group.</param>
        ''' <returns>True if the members can be moved into this group and the current
        ''' user has the appropriate permission to do so.</returns>
        <Extension>
        Public Function CanMoveInto(
         this As IGroup, mems As IEnumerable(Of IGroupMember)) As Boolean

            ' Don't allow pools to be dragged to root
            If this.IsRoot AndAlso mems.Any(Function(c) c.RawMember.MemberType = GroupMemberType.Pool) Then Return False

            ' Don't allow pool members to be dragged into pools (yet!)
            If this.RawMember.MemberType = GroupMemberType.Pool Then Return False

            ' (Probably) a quick check
            If Not mems.Any() Then Return False

            ' Check that the user has edit permission to edit this tree
            Dim t As IGroupTree = this.Tree
            If t IsNot Nothing AndAlso Not t.HasEditPermission(User.Current) _
             Then Return False

            ' If the dragged members includes any ancestors of this group, then we
            ' can't do anything with it
            If this.Ancestry().Any(Function(g) mems.Contains(g)) Then Return False

            Return True

        End Function

        ''' <summary>
        '''  Checks if a collection of group members is capable of being copied into
        ''' this group.
        ''' </summary>
        ''' <param name="mems"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CanCopyInto(
         this As IGroup, mems As IEnumerable(Of IGroupMember)) As Boolean
            If mems.Any(Function(x) x.IsGroup) Then Return False

            ' Basically, this has the same restrictions as a move operation
            If Not this.CanMoveInto(mems) Then Return False

            ' If this is the root group, the members can't be copied into it
            If this.IsRoot Then Return False

            ' If all the members are owned by the root group, they can't be copied
            If mems.All(Function(gm) gm.IsInRoot()) Then Return True

            Return True

        End Function

        ''' <summary>
        ''' The number of subgroups directly held by this group
        ''' </summary>
        <Extension>
        Public Function SubgroupCount(this As IGroup) As Integer
            Return this.AsEnumerable().
               Count(Function(x) x.IsGroup AndAlso Not x.IsPool)
        End Function

        ''' <summary>
        ''' Gets the number of items (ie. group members which are not subgroups)
        ''' directly held by this group.
        ''' </summary>
        <Extension>
        Public Function ItemCount(this As IGroup) As Integer
            Return (this.Count - this.SubgroupCount())
        End Function

        ''' <summary>
        ''' Gets the number of items (ie. group members which are not subgroups) held
        ''' in this group or its subtree - ie. in all descendants of this group.
        ''' </summary>
        <Extension>
        Public Function ItemTotalCount(this As IGroup) As Integer
            ' We use a set to build up the tally of items to ensure that we don't
            ' count the same item (in different groups) twice.
            Dim items As New clsSet(Of IGroupMember)
            this.Scan(Sub(m) If Not m.IsGroup Then items.Add(m))
            Return items.Count
        End Function

        <Extension>
        Public Function FullNameExcludingRoot(this As IGroup) As String
            Dim nameList = New List(Of String) From {this.Name}
            Dim owner = this.Owner
            While Not owner.IsRoot
                nameList.Add(owner.Name)
                owner = owner.Owner
            End While
            nameList.Reverse()
            Return String.Join("/", nameList)
        End Function

    End Module

End Namespace
