Imports BluePrism.Server.Domain.Models
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports System.Xml

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsApplicationElement
''' 
''' <summary>
''' Represents a member node of the application definition tree.
''' </summary>
<DebuggerDisplay("Member: {FullPath}", Name:="{mName}")> _
Public MustInherit Class clsApplicationMember

#Region " Class-scope declarations "

    ''' <summary>
    ''' A comparer which tests the ID of the application member to see if it matches
    ''' the ID of another member.
    ''' </summary>
    Public Class IDComparer : Implements IEqualityComparer(Of clsApplicationMember)

        ''' <summary>
        ''' Tests if the two members are equal by testing their IDs
        ''' </summary>
        ''' <param name="x">The first member to test</param>
        ''' <param name="y">The second member to test</param>
        ''' <returns>True if the two members are both null or both non-null members
        ''' with the same ID.</returns>
        Public Overloads Function Equals( _
         ByVal x As clsApplicationMember, ByVal y As clsApplicationMember) _
          As Boolean Implements IEqualityComparer(Of clsApplicationMember).Equals
            If x Is y Then Return True
            If x Is Nothing OrElse y Is Nothing Then Return False
            Return (x.ID = y.ID)
        End Function

        ''' <summary>
        ''' Gets a hashcode for the given member based on its ID.
        ''' </summary>
        ''' <param name="obj">The member whose hashcode is required</param>
        ''' <returns>A hash of the members ID, or 0 if the member was null.</returns>
        Public Overloads Function GetHashCode(ByVal obj As clsApplicationMember) _
         As Integer Implements IEqualityComparer(Of clsApplicationMember).GetHashCode
            If obj Is Nothing Then Return 0
            Return obj.ID.GetHashCode() Xor 614
        End Function
    End Class

    ''' <summary>
    ''' Returns a clsApplicationElement corresponding to the xml given. The root node
    ''' must be "element".
    ''' </summary>
    ''' <param name="e">The XML element containing the data for which an application
    ''' element is required</param>
    ''' <returns>An ApplicationElement containing the data from the given XML element
    ''' or null if the given element was null or did not represent an application
    ''' element.</returns>
    Public Shared Function CreateFromXML(ByVal e As XmlElement) _
     As clsApplicationMember
        If e Is Nothing Then Return Nothing

        Dim mem As clsApplicationMember
        Select Case e.Name
            Case "group" : mem = New clsApplicationElementGroup()
            Case "element" : mem = New clsApplicationElement()
            Case "region-container" : mem = New clsRegionContainer()
            Case "region" : mem = New clsApplicationRegion()
            Case Else : Return Nothing
        End Select
        Try
            mem.FromXml(e)
        Catch
            Return Nothing
        End Try
        Return mem
    End Function

#End Region

#Region " Member Variables "

    ' The ID of this member
    Private mID As Guid

    ' The name of this member
    Private mName As String

    ' The parent of this member
    Private mParent As clsApplicationMember

    ' Child application members keyed on their ID
    Private mChildren As IDictionary(Of Guid, clsApplicationMember)

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new named application member with a newly generated ID
    ''' </summary>
    ''' <param name="memberName">The name of the member to create</param>
    Public Sub New(ByVal memberName As String)
        mChildren = New clsOrderedDictionary(Of Guid, clsApplicationMember)
        Name = memberName
        ID = Guid.NewGuid()
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the XML element name for application members of this type.
    ''' </summary>
    Friend MustOverride ReadOnly Property XmlName() As String

    ''' <summary>
    ''' The readonly collection of application members which are children of this
    ''' member.
    ''' </summary>
    Public ReadOnly Property ChildMembers() As ICollection(Of clsApplicationMember)
        Get
            Return mChildren.Values
        End Get
    End Property

    ''' <summary>
    ''' Checks if this member has any child members.
    ''' </summary>
    Public ReadOnly Property HasChildren() As Boolean
        Get
            Return mChildren.Count > 0
        End Get
    End Property

    ''' <summary>
    ''' A unique identifier for this application member.
    ''' </summary>
    Public Property ID() As Guid
        Get
            Return mID
        End Get
        Set(ByVal value As Guid)
            Dim oldId As Guid = mID
            mID = value
            If mParent IsNot Nothing AndAlso oldId <> value Then
                mParent.mChildren.Remove(oldId)
                mParent.mChildren(mID) = Me
            End If
        End Set
    End Property

    ''' <summary>
    ''' Friendly name for this application member. Not necessarily unique across
    ''' all member objects.
    ''' </summary>
    Public Property Name() As String
        Get
            Return mName
        End Get
        Set(ByVal value As String)
            mName = value
        End Set
    End Property

    ''' <summary>
    ''' The 'full path' for this member, in a human-readable format. This is the
    ''' name of the member itself, and the names of all parent members going up the
    ''' tree.
    ''' </summary>
    Public ReadOnly Property FullPath() As String
        Get
            Dim path As String = mName
            Dim m As clsApplicationMember = Me
            While m.Parent IsNot Nothing
                m = m.Parent
                path = m.Name & " / " & path
            End While
            Return path
        End Get
    End Property

    ''' <summary>
    ''' Gets the root member for this member - ie. the top level member for which
    ''' there is no further parent. Note that this may be the member called if it
    ''' is not associated in any hierarchy.
    ''' </summary>
    Public ReadOnly Property Root() As clsApplicationMember
        Get
            If mParent IsNot Nothing Then Return mParent.Root
            Return Me
        End Get
    End Property

    ''' <summary>
    ''' The parent of this member (possibly Nothing).
    ''' </summary>
    Public Property Parent() As clsApplicationMember
        Get
            Return mParent
        End Get
        Set(ByVal value As clsApplicationMember)
            If value IsNot mParent Then
                Me.RemoveFromParent()
                mParent = value
            End If
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Recursively resolves the region relationships in the app model tree on and
    ''' below this member. This ensures that any regions are added to their region
    ''' containers so that if they are edited / utilised, the relationship can be
    ''' discovered.
    ''' </summary>
    ''' <remarks>
    ''' This assumes that the container ID on the application regions is set
    ''' correctly
    ''' </remarks>
    Friend Sub ResolveRegionRelationships()
        ResolveRegionRelationships(Me, Me)
    End Sub

    ''' <summary>
    ''' Recursively resolves the region relationship for the given member and its
    ''' descendents. It will check the given member to see if it is a region with a
    ''' registered container. If so, it will add the member to that container within
    ''' the definition. Either way, it will then perform the same check recursively
    ''' on the member's descendents.
    ''' </summary>
    ''' <param name="mem">The member on and beneath which region relationships need
    ''' to be resolved</param>
    ''' <param name="root">The root member on which to search for region members
    ''' </param>
    Protected Friend Sub ResolveRegionRelationships( _
     ByVal mem As clsApplicationMember, ByVal root As clsApplicationMember)
        ' If an app model doesn't have a root application element, this can be
        ' called with null - potentially other times too. Just ignore null members
        If mem Is Nothing Then Return

        ' Otherwise, Test if the member is a region - if it is, ensure that it is
        ' added as a registered region on its container (if it has one registered)
        Dim reg As clsApplicationRegion = TryCast(mem, clsApplicationRegion)
        If reg IsNot Nothing AndAlso reg.ContainerId <> Nothing Then
            Dim cont As clsRegionContainer = _
             root.FindMember(Of clsRegionContainer)(reg.ContainerId)
            If cont IsNot Nothing Then cont.Regions.Add(reg)
        End If

        ' Then recurse through its children to resolve the other relationships
        For Each childMem As clsApplicationMember In mem.ChildMembers
            ResolveRegionRelationships(childMem, root)
        Next

    End Sub

    ''' <summary>
    ''' Loads the basic information about this application member from the given XML
    ''' element. There may be constraints on the XML element defined by subclasses
    ''' which may cause exceptions to be thrown if called with invalid element data.
    ''' </summary>
    ''' <param name="el">The element from which to draw the data for this member.
    ''' </param>
    ''' <exception cref="ArgumentNullException">If the given XML element is null.
    ''' </exception>
    ''' <exception cref="InvalidValueException">If the given XML element does not
    ''' have the correct name.</exception>
    Public Overridable Sub FromXml(ByVal el As XmlElement)
        If el Is Nothing Then Throw New ArgumentNullException(
         NameOf(el), My.Resources.Resources.clsApplicationMember_CannotLoadApplicationMemberDataFromANullXMLElement)

        If el.Name <> XmlName Then Throw New InvalidValueException(
         My.Resources.Resources.clsApplicationMember_ExpectedXMLElementNamed0Found1, XmlName, el.Name)

        Me.Name = el.GetAttribute("name")

        'load xml child nodes
        For Each xe As XmlElement In el.ChildNodes
            Select Case xe.Name
                Case "id"
                    ID = New Guid(xe.InnerText)
                Case "element", "region-container", "region", "group"
                    AddMember(CreateFromXML(xe))
            End Select
        Next

    End Sub

    ''' <summary>
    ''' Writes the data surrounding this application member to XML and appends it
    ''' to an Xml document.
    ''' </summary>
    ''' <param name="doc">The document to which the produced XML element should be
    ''' appended</param>
    ''' <returns>The XML element with this member's data in it</returns>
    Public Overridable Function ToXml(ByVal doc As XmlDocument) As XmlElement
        Dim el As XmlElement = doc.CreateElement(XmlName)

        'append name as xml-attribute
        el.SetAttribute("name", Me.Name)

        'add ID as child node
        BPUtil.AppendTextElement(el, "id", mID.ToString())

        'add each child as child xml-node
        For Each m As clsApplicationMember In mChildren.Values
            el.AppendChild(m.ToXml(doc))
        Next

        Return el
    End Function

    ''' <summary>
    ''' Recursively searches for an application member with the given name, after
    ''' checking its own name.
    ''' </summary>
    ''' <param name="name">The name of the member to find.</param>
    ''' <returns>Returns the first member found (including itself) with a matching
    ''' name or null if no such match is found.</returns>
    Public Function FindMemberByName(ByVal name As String) As clsApplicationMember
        If name = Me.Name Then Return Me
        For Each childMem As clsApplicationMember In ChildMembers
            Dim matchMem As clsApplicationMember = childMem.FindMemberByName(name)
            If matchMem IsNot Nothing Then Return matchMem
        Next
        Return Nothing
    End Function


    ''' <summary>
    ''' Recursively searches for an application member with the given ID, after
    ''' checking its own ID.
    ''' </summary>
    ''' <param name="memId">The ID of the member to find.</param>
    ''' <returns>Returns the first member found (including itself) with a matching ID
    ''' or null if no such match is found.</returns>
    Public Function FindMember(ByVal memId As Guid) As clsApplicationMember
        If memId = Me.mID Then Return Me
        For Each childMem As clsApplicationMember In ChildMembers
            Dim matchMem As clsApplicationMember = childMem.FindMember(memId)
            If matchMem IsNot Nothing Then Return matchMem
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Recursively searches for an application member with the given ID and of the
    ''' given type.
    ''' </summary>
    ''' <typeparam name="T">The type of member required</typeparam>
    ''' <param name="memId">The ID of the member to find.</param>
    ''' <returns>Returns the first member found (including itself) with a matching ID
    ''' or null if no such match is found or if the member in question is not of the
    ''' required type.</returns>
    Public Function FindMember(Of T As clsApplicationMember)(ByVal memId As Guid) As T
        Return TryCast(FindMember(memId), T)
    End Function

    ''' <summary>
    ''' Takes the member data (ID, Name, Parent, Child Members) from the given
    ''' application member - note that, where the child members are concerned, this
    ''' is a take, rather than a copy. After this method has been called, the source
    ''' member will have no parent and no children, and this member object will have
    ''' taken its place in the hierarchical structure it was in.
    ''' </summary>
    ''' <param name="target">The target member who is being replaced by this object.
    ''' </param>
    Public Overridable Sub ReplaceMember(ByVal target As clsApplicationMember)
        If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
        mID = target.mID
        mName = target.mName
        mChildren.Clear()
        For Each child As clsApplicationMember In _
         New List(Of clsApplicationMember)(target.ChildMembers)
            AddMember(child)
        Next

        Dim parent As clsApplicationMember = target.Parent
        If parent IsNot Nothing Then
            Dim index As Integer = parent.GetChildMemberIndex(target)
            target.RemoveFromParent()
            parent.InsertMember(index, Me)
        End If
    End Sub

    ''' <summary>
    ''' Gets the child with the specified ID.
    ''' </summary>
    ''' <param name="ID">The ID of the child sought.</param>
    ''' <returns>Returns a clsApplicationMember with the matching
    ''' ID, or nothing if no such member exists.</returns>
    Public Function GetMember(ByVal ID As Guid) As clsApplicationMember
        Return mChildren(ID)
    End Function

    ''' <summary>
    ''' Adds the supplied object to the list of members.
    ''' </summary>
    ''' <param name="Member">The new member to be added; must not be
    ''' null.</param>
    ''' <returns>Returns true on success; false otherwise.</returns>
    ''' <remarks>See also InsertMember.</remarks>
    Public Function AddMember(ByVal member As clsApplicationMember) As Boolean
        If member Is Nothing Then Return False
        mChildren.Add(member.ID, member)
        member.Parent = Me
        Return True
    End Function

    ''' <summary>
    ''' Inserts the supplied member at the specified index.
    ''' </summary>
    ''' <param name="index">The index at which to insert the member.</param>
    ''' <param name="Member">The member to insert as a child.</param>
    ''' <returns>returns true if the given member was inserted into the children of
    ''' this member; false if it was not.</returns>
    ''' <remarks>If the member to be inserted already exists in the collection at the
    ''' point of insertion, the operation is aborted (ie. this will return false to
    ''' indicate that the member was not inserted).
    ''' </remarks>
    ''' <exception cref="ArgumentOutOfRangeException">If the index at which the
    ''' child member should be inserted was negative or beyond the size of this
    ''' member's children collection.</exception>
    Public Function InsertMember( _
     ByVal index As Integer, ByVal member As clsApplicationMember) As Boolean

        If index < 0 OrElse index > mChildren.Count Then Throw New ArgumentOutOfRangeException(
         My.Resources.Resources.clsApplicationMember_Index, My.Resources.Resources.clsApplicationMember_ParameterIndexMustBeBetweenZeroAndTheSizeOfTheNumberOfMembersInclusive)

        If member Is Nothing Then Return False

        'A revised collection of children, which will
        'include the new member at the correct index
        Dim newChildren As New clsOrderedDictionary(Of Guid, clsApplicationMember)
        Dim inserted As Boolean = False
        Dim skippedMember As Boolean = False

        Dim i As Integer = 0
        For Each pair As KeyValuePair(Of Guid, clsApplicationMember) In mChildren
            ' If we've reached the desired index, insert the new member.
            ' Set it to null to indicate that it has been inserted
            If i = index Then
                ' Double check that the given member is not already here.
                If pair.Value Is member Then Return False

                newChildren.Add(member.ID, member)
                member.Parent = Me
                inserted = True
            End If

            ' Check if this is the member we're inserting - if it is, we skip it,
            ' it's a move of the member to a different position in this collection
            ' so we've either already inserted it, or it's yet to be inserted.
            ' We also ensure that we're not incrementing our counter in this case,
            ' since that's effectively tracking the posn in the new collection
            If pair.Value Is member Then skippedMember = True : Continue For

            ' Otherwise, just keep adding to the new collection from the old
            newChildren.Add(pair.Key, pair.Value)
            i += 1
        Next

        ' If we haven't added it yet (ie. index == mChildren.Count), add it now
        If Not inserted Then
            Debug.Assert(index = mChildren.Count OrElse skippedMember)
            newChildren.Add(member.ID, member)
            member.Parent = Me
        End If

        mChildren = newChildren
        Return True

    End Function

    ''' <summary>
    ''' Gets the index of the supplied child.
    ''' </summary>
    ''' <param name="mem">The child member whose index is to be returned.</param>
    ''' <returns>Returns the index of the child or -1 if it doesn't exist.</returns>
    Public Function GetChildMemberIndex(ByVal mem As clsApplicationMember) As Integer
        Return GetChildMemberIndex(mem.ID)
    End Function

    ''' <summary>
    ''' Gets the index of the supplied child.
    ''' </summary>
    ''' <param name="memId">The ID of the child member whose
    ''' index is to be returned.</param>
    ''' <returns>Returns the index of the child with the
    ''' supplied ID if it exists or -1 otherwise.</returns>
    Public Function GetChildMemberIndex(ByVal memId As Guid) As Integer
        Dim index As Integer = 0
        For Each id As Guid In mChildren.Keys
            If id = memId Then Return index
            index += 1
        Next
        Return -1
    End Function

    ''' <summary>
    ''' Removes the specified member from the collection of child members.
    ''' </summary>
    ''' <param name="mem">The child to be removed.</param>
    ''' <returns>Returns true on success; false otherwise.</returns>
    Public Overridable Function RemoveMember(ByVal mem As clsApplicationMember) As Boolean
        If mem Is Nothing Then Return False
        If Not mChildren.Values.Contains(mem) Then Return False
        mChildren.Remove(mem.ID)
        mem.Parent = Nothing
        Return True
    End Function

    ''' <summary>
    ''' Returns a deep clone of this object (ie recursive down tree of children),
    ''' without copying the reference to the parent of this object. The parent-child
    ''' references of all children will point to the corresponding clone
    ''' rather than the original, but this parent node will need to have its parent
    ''' reference ammended.
    ''' </summary>
    ''' <returns>A deep clone of this application member</returns>
    Public Function Clone() As clsApplicationMember
        Return Clone(True)
    End Function

    ''' <summary>
    ''' Deep clones this application member recursively, optionally ensuring that any
    ''' region relationships are resolved after the clone has completed.
    ''' </summary>
    ''' <param name="resolveRegions">True to tie up any regions to their region
    ''' containers after the clone has completed. This ensures that each can be
    ''' reached from the other.</param>
    ''' <returns>A deep clone of this application member, with region relationships
    ''' resolved as specified.</returns>
    Private Function Clone(ByVal resolveRegions As Boolean) _
     As clsApplicationMember
        Dim e As clsApplicationMember = InnerClone()
        If resolveRegions Then e.ResolveRegionRelationships()
        Return e
    End Function

    ''' <summary>
    ''' Inner method to create a deep clone of this member. This is the method which
    ''' subclasses will override to append further actions to the cloning method.
    ''' </summary>
    ''' <returns>A deep clone of this application member, with none of the region
    ''' relationships resolved - ie. all the <see cref="clsRegionContainer"/> objects
    ''' in the tree should be empty of regions. They will be resolved in the next
    ''' step of the Clone() method.</returns>
    Protected Overridable Function InnerClone() As clsApplicationMember
        Dim e As clsApplicationMember = _
         DirectCast(MemberwiseClone(), clsApplicationMember)

        e.mChildren = New clsOrderedDictionary(Of Guid, clsApplicationMember)
        For Each child As clsApplicationMember In mChildren.Values
            Dim copy As clsApplicationMember = _
             DirectCast(child.InnerClone(), clsApplicationMember)
            copy.Parent = e
            e.AddMember(copy)
        Next

        Return e
    End Function

    ''' <summary>
    ''' Checks if this application member matches the given string in a search /
    ''' filter operation. What this means is dependent on the type of member
    ''' being matched - the default implementation checks only the name of the
    ''' member.
    ''' </summary>
    ''' <param name="text">The text to check for in this member.</param>
    ''' <param name="partialMatch">True to indicate a partial match should count
    ''' as a match. False indicates that the whole text should match.</param>
    ''' <param name="caseSensitive">True to indicate that the search should be
    ''' case sensitive, False to indicate that the case should not be taken into
    ''' account when checking for a match.</param>
    ''' <returns>True if the given string matches this member; False otherwise.
    ''' </returns>
    Public Overridable Function Matches(ByVal text As String, _
     ByVal partialMatch As Boolean, ByVal caseSensitive As Boolean) As Boolean
        Return BPUtil.IsMatch(mName, Text, partialMatch, caseSensitive)
    End Function

    ''' <summary>
    ''' Finds conflicting members in the set of descendents under the new parent,
    ''' and the set of descendents under this member.
    ''' </summary>
    ''' <param name="newParent">The new parent</param>
    Public Function FindConflicts(ByVal newParent As clsApplicationMember) _
     As ICollection(Of clsApplicationMember)

        Dim comp As New IDComparer()
        Dim newChildren As New clsSet(Of clsApplicationMember)(comp)
        GetAllDescendents(newParent, newChildren)

        Dim existing As New clsSet(Of clsApplicationMember)(comp)
        GetAllDescendents(Me, existing)

        existing.Intersect(newChildren)

        Return existing

    End Function

    ''' <summary>
    ''' Gets a collection of all the decendent IDs of a given parent, including the
    ''' given parents ID.
    ''' </summary>
    ''' <param name="parent">The parent</param>
    ''' <param name="children">A collection to hold the resulting descendent
    ''' children</param>
    Public Sub GetAllDescendents(ByVal parent As clsApplicationMember, _
     ByRef children As clsSet(Of clsApplicationMember))
        children.Add(parent)
        For Each member As clsApplicationMember In parent.ChildMembers
            GetAllDescendents(member, children)
        Next
    End Sub

    ''' <summary>
    ''' Creates a new ID for this application element group and all of its child
    ''' members, returning the newly created ID for members that existing in the
    ''' given collection of conflicting IDs
    ''' </summary>
    Public Function CreateNewID() As Guid
        Return CreateNewID(Nothing)
    End Function

    ''' <summary>
    ''' Creates a new ID for this application element group and all of its child
    ''' members, returning the newly created ID for members that existing in the
    ''' given collection of conflicting IDs
    ''' </summary>
    ''' <param name="newIdFilter">A filter which decides the elements which should
    ''' be given new IDs - null indicates that all elements should get new IDs
    ''' </param>
    Public Function CreateNewID(ByVal newIdFilter As Predicate(Of clsApplicationMember)) As Guid
        If newIdFilter Is Nothing OrElse newIdFilter(Me) Then
            ID = Guid.NewGuid()
        End If
        ' Iterate over a disconnected list since we'll be changing the underlying
        ' dictionary inside the loop as we get new IDs for the descendants
        For Each child As clsApplicationMember In New List(Of clsApplicationMember)(ChildMembers)
            child.CreateNewID(newIdFilter)
        Next
        Return mID
    End Function

    ''' <summary>
    ''' Creates a disconnected copy of this application member, with a new IDs
    ''' for the conflicting IDs contained in the given collection.
    ''' The element will have no assigned parent element.
    ''' </summary>
    ''' <param name="newIdFilter">A filter which decides the elements which should
    ''' be given new IDs - null indicates that all elements should get new IDs
    ''' </param>
    Public Function ConstrainedClone(ByVal newIdFilter As Predicate(Of clsApplicationMember)) As clsApplicationMember
        Dim el As clsApplicationMember = DirectCast(Clone(), clsApplicationMember)
        el.Parent = Nothing
        el.CreateNewID(newIdFilter)
        Return el
    End Function

    ''' <summary>
    ''' Creates a disconnected copy of this application member, with a full set of
    ''' new IDs for all of the descendants, and no assigned parent element.
    ''' </summary>
    ''' <returns>A copy of this member and all its descendants with new IDs and no
    ''' assigned parent.</returns>
    Public Function Copy() As clsApplicationMember
        Dim el As clsApplicationMember = DirectCast(Clone(), clsApplicationMember)
        el.Parent = Nothing
        el.CreateNewID()
        Return el
    End Function

    ''' <summary>
    ''' Removes this member from its parent, if such a parent exists. Otherwise
    ''' does nothing
    ''' </summary>
    Public Sub RemoveFromParent()
        If mParent IsNot Nothing Then
            mParent.RemoveMember(Me)
            mParent = Nothing
        End If
    End Sub

#End Region

End Class
