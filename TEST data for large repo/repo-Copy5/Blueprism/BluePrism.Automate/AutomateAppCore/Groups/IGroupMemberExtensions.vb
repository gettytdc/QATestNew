Imports System.Runtime.CompilerServices
Imports System.Windows.Forms
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections

Namespace Groups

    Public Module IGroupMemberExtensions

        ''' <summary>
        ''' Gets this group member's ID as a <see cref="Guid"/>
        ''' </summary>
        ''' <param name="this">The member whose GUID id is required.</param>
        ''' <returns>The GUID which represents this group member's ID;
        ''' <see cref="Guid.Empty"/> if it has no ID set.</returns>
        ''' <exception cref="InvalidCastException">If the group member's ID was not
        ''' a <see cref="Guid"/>.</exception>
        <Extension>
        Public Function IdAsGuid(this As IGroupMember) As Guid
            Return BPUtil.IfNull(this.Id, Guid.Empty)
        End Function

        ''' <summary>
        ''' Gets this group member's ID as an <see cref="Integer"/>
        ''' </summary>
        ''' <param name="this">The member whose integer id is required.</param>
        ''' <returns>The int value which represents this group member's ID; 0 if it
        ''' has no ID set.</returns>
        ''' <exception cref="InvalidCastException">If the group member's ID was not
        ''' an <see cref="Integer"/>.</exception>
        <Extension>
        Public Function IdAsInteger(this As IGroupMember) As Integer
            Return BPUtil.IfNull(this.Id, 0)
        End Function

        ''' <summary>
        ''' Gets this group member's ID as a <see cref="String"/>
        ''' </summary>
        ''' <param name="this">The member whose string id is required.</param>
        ''' <returns>The string value which represents this group member's ID;
        ''' null if it has no ID set.</returns>
        ''' <exception cref="InvalidCastException">If the group member's ID was not
        ''' a <see cref="String"/>.</exception>
        <Extension>
        Public Function IdAsString(this As IGroupMember) As String
            Return DirectCast(this.Id, String)
        End Function

        ''' <summary>
        ''' Removes all instances of this group member from all groups that it is
        ''' part of, effectively placing it in the root of the tree.
        ''' </summary>
        <Extension>
        Public Sub RemoveFromAllGroups(this As IGroupMember)
            Dim gt As IGroupTree = this.Tree
            If gt Is Nothing Then Return

            ' Remove all the other entries from the tree
            gt.Root.Scan(
                Sub(m) If m.Path() <> this.Path() AndAlso m.Equals(this) Then m.Remove())

            ' Remove this one last - this ensures that if any member gets moved into
            ' the root of the tree, it's this one.
            this.Remove()
        End Sub

        ''' <summary>
        ''' Gets all of the owners of this group member in the current tree
        ''' </summary>
        ''' <returns>A collection of group objects representing all groups which own
        ''' this group member. This may include the root group</returns>
        <Extension>
        Public Function FindAllOwners(this As IGroupMember) As ICollection(Of IGroup)
            Dim gt As IGroupTree = this.Tree
            If gt Is Nothing Then Return GetEmpty.ICollection(Of IGroup)()
            Return gt.Root.FindAllOwnersOf(this)
        End Function

        ''' <summary>
        ''' The full hierarchical path from the top level of this tree (element 0)
        ''' down to this member (element
        ''' <see cref="ICollection(Of T).Count">Count</see>-1).
        ''' </summary>
        <Extension>
        Public Function PathElements(this As IGroupMember) _
         As ICollection(Of IGroupMember)
            Return GetReadOnly.ICollection(GetEditPath(this))
        End Function

        ''' <summary>
        ''' Gets the breadcrumbs from the root of the tree to this group member.
        ''' Note that the root of the tree is represented by an empty string, rather
        ''' than an arbitrary name (by convention, the interface uses the name of
        ''' the tree as the name of the root group, but for the purposes of the
        ''' path, at least, it has no name).
        ''' </summary>
        <Extension>
        Public Function Breadcrumbs(this As IGroupMember) As ICollection(Of String)
            Dim lst As New List(Of String)
            For Each m As IGroupMember In this.PathElements()
                ' Don't add the name for root group
                lst.Add(If(m.IsGroup AndAlso DirectCast(m, IGroup).IsRoot, "", m.Name))
            Next
            Return lst
        End Function

        ''' <summary>
        ''' The escaped path to this group member from the root of the tree.
        ''' </summary>
        ''' <remarks>
        ''' The path separator char is '/' - if any group names contain a '/' char,
        ''' their name is surrounded with quotes and any quotes in the name are
        ''' escaped by doubling.
        ''' Basically, this aligns with the rules required by the extension method
        ''' <see cref="SplitQuoted"/>, defined in <c>BluePrism.BPCoreLib</c>.
        ''' </remarks>
        <Extension>
        Public Function Path(this As IGroupMember) As String
            Dim sb As New StringBuilder()
            For Each gm As IGroupMember In this.PathElements()
                ' Add a separator if this isn't our first time through the loop
                ' or if this is the root (to indicate the root of the tree)
                ' The root element is the only one with a blank name (not allowed
                ' in 'normal' group members)
                If gm.IsRoot Then sb.Append("/"c) : Continue For
                If sb.Length > 0 AndAlso sb(sb.Length - 1) <> "/"c Then sb.Append("/"c)

                ' We only append names for non-root members
                Dim name As String = gm.Name
                ' if the element contains the separator char or starts with a
                ' quote character, we need to surround the element in quotes and
                ' escape any quote characters within it
                If name.Contains("/"c) OrElse name.FirstOrDefault() = """"c Then
                    sb.Append("""").Append(name.Replace("""", """""")).Append("""")
                Else
                    sb.Append(name)
                End If
            Next
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Gets the path to this group member from the top level group which owns
        ''' its ancestor down to this group member, in an editable list.
        ''' Effectively, this is the 'breadcrumbs' the last element is this member,
        ''' and each of the elements before it are the ancestry of the groups that
        ''' own it
        ''' </summary>
        Private Function GetEditPath(this As IGroupMember) As List(Of IGroupMember)
            Dim owner As IGroup = this.Owner
            Dim lst As List(Of IGroupMember) =
                If(owner Is Nothing, New List(Of IGroupMember), GetEditPath(owner))
            lst.Add(this)
            Return lst
        End Function

        ''' <summary>
        ''' A collection of groups, starting from the top level in this tree through
        ''' the descendants of the top level group until it reaches the group which
        ''' contains this member.
        ''' If this member is not within a group, this will return an empty
        ''' collection.
        ''' </summary>
        <Extension>
        Public Function Ancestry(this As IGroupMember) As ICollection(Of IGroup)
            If this.Owner Is Nothing Then Return GetEmpty.ICollection(Of IGroup)()
            ' Get the groups from the editpath into our own list (the only entry
            ' which might not be a group should be the last entry which
            ' represents this member.
            Dim lst As New List(Of IGroup)
            For Each m As IGroupMember In GetEditPath(this)
                If m IsNot this Then lst.Add(DirectCast(m, IGroup))
            Next
            ' And return a readonly version of it
            Return GetReadOnly.ICollection(Of IGroup)(lst)
        End Function

        ''' <summary>
        ''' Gets the root group of this group member
        ''' </summary>
        ''' <param name="this">The member for which the root group is required</param>
        ''' <returns>The root group of the member or null if it has no root group -
        ''' ie. it is not a root group and it has no owner.</returns>
        <Extension>
        Public Function RootGroup(this As IGroupMember) As IGroup
            Dim owner As IGroup = this.Owner
            If owner Is Nothing Then Return Nothing
            Return If(owner.IsRoot, owner, owner.RootGroup())
        End Function

        ''' <summary>
        ''' Gets whether this member is in an actual group, ie. not in the root of
        ''' the tree.
        ''' </summary>
        <Extension>
        Public Function IsInGroup(this As IGroupMember) As Boolean
            Return (this.Owner IsNot Nothing AndAlso Not this.Owner.IsRoot)
        End Function

        ''' <summary>
        ''' Gets whether this member is in the root of the tree.
        ''' </summary>
        <Extension>
        Public Function IsInRoot(this As IGroupMember) As Boolean
            Return (this.Owner IsNot Nothing AndAlso this.Owner.IsRoot)
        End Function


        ''' <summary>
        ''' Gets the ID of this member as a comparable object.
        ''' </summary>
        ''' <remarks>At the time of writing, the only ID types fully supported are
        ''' Integer, Guid and String, all of which implement IComparable.
        ''' </remarks>
        <Extension>
        Public Function ComparableId(this As IGroupMember) As IComparable
            If this.Id IsNot Nothing Then Return DirectCast(this.Id, IComparable)
            Return 0
        End Function

#Region " IEnumerable<IGroupMember> extensions "

        ''' <summary>
        ''' Gets the raw members for an enumerable of group members, ensuring that
        ''' there are no group members operating within a filtering context within
        ''' them.
        ''' </summary>
        ''' <param name="this">The group members to ensure are raw</param>
        ''' <returns>The raw group members corresponding to the members passed in.
        ''' </returns>
        ''' <remarks>The enumerable returned is not serializable, so it must be
        ''' entered into some other serializable collection before it can be sent to
        ''' a remote <see cref="clsServer"/> instance.</remarks>
        <Extension>
        Public Function RawMembers(this As IEnumerable(Of IGroupMember)) _
         As IEnumerable(Of GroupMember)
            Return this.Select(Function(m) m.RawMember)
        End Function

        <Extension()>
        Public Function Sort(this As IEnumerable(Of IGroupMember), sortFieldName As String, sortOrder As SortOrder) As IEnumerable(Of IGroupMember)
            ' We always attempt to order by the "IsDefault" flag because default items should always come first
            Dim ordered = this.OrderByDescending(
                        Function(x)
                            Return GetSortValue(x, "IsDefault", False)
                        End Function)
            If Not String.IsNullOrWhiteSpace(sortFieldName) And sortOrder <> SortOrder.None Then
                If sortOrder = SortOrder.Ascending Then
                    ordered = ordered.ThenBy(
                        Function(x)
                            Return GetSortValue(x, sortFieldName, 0)
                        End Function)
                Else
                    ordered = ordered.ThenByDescending(
                        Function(x)
                            Return GetSortValue(x, sortFieldName, 1)
                        End Function)
                End If

                If sortFieldName.ToLowerInvariant <> "name" Then
                    ordered = ordered.ThenBy(Function(x) x.Name)
                End If
            End If

            Return ordered
        End Function

        Private Function GetSortValue(groupMember As IGroupMember, sortFieldName As String, defaultSort As Object) As Object
            Dim sortField = groupMember?.GetType().GetProperty(sortFieldName)
            If sortField Is Nothing Then Return defaultSort
            If sortField.PropertyType.IsEnum() Then
                Return Convert.ToInt32(sortField.GetValue(groupMember, Nothing))
            Else
                Return sortField.GetValue(groupMember, Nothing)
            End If
        End Function

#End Region

    End Module

End Namespace
