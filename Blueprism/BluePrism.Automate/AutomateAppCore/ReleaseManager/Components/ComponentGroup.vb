Imports System.Xml

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Component which can contain other components.
''' </summary>
<Serializable, DataContract([Namespace]:="bp", IsReference:=True),
 KnownType(GetType(clsSortedSet(Of PackageComponent)))>
Public MustInherit Class ComponentGroup
    Inherits PackageComponent
    Implements ICollection(Of PackageComponent)

#Region " Class-scope declarations "

    ''' <summary>
    ''' The name of the XML element which contains the members of a component group
    ''' </summary>
    Protected Const MembersXmlElementName As String = "members"

    ''' <summary>
    ''' Basic comparer which does a very simple comparison to test whether two
    ''' components are equal (tests type, Id and name only).
    ''' </summary>
    Private Class BasicDiffComparer : Implements IComparer(Of PackageComponent)

        Public Function Compare(ByVal x As PackageComponent, ByVal y As PackageComponent) _
         As Integer Implements IComparer(Of PackageComponent).Compare
            ' We want a simple equals - such that if type, id and name match then
            ' it's considered equal for the sake of the diff.
            If x.Type = y.Type AndAlso x.Name = y.Name _
             AndAlso Object.Equals(x.Id, y.Id) Then Return 0

            Return x.CompareTo(y)
        End Function
    End Class

#End Region

#Region " Member Variables "

    ' The components which make up the members of this group.
    <DataMember>
    Private mMembers As IBPSet(Of PackageComponent)

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new group component with the given name and ID.
    ''' </summary>
    ''' <param name="id">The ID of the component</param>
    ''' <param name="name">The name of the component</param>
    Protected Sub New(ByVal owner As OwnerComponent, ByVal id As Object, ByVal name As String)
        MyBase.New(owner, id, name)
    End Sub

    ''' <summary>
    ''' Creates a new process component which draws its data from the given XML
    ''' reader.
    ''' </summary>
    ''' <param name="reader">The reader whence to draw the process data.</param>
    ''' <param name="ctx">The loading context for the XML reading</param>
    Protected Sub New(ByVal owner As OwnerComponent, ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        MyBase.New(owner, reader, ctx)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The member components of this group.
    ''' </summary>
    Public Overridable ReadOnly Property Members() As ICollection(Of PackageComponent)
        Get
            Return Me
        End Get
    End Property

    ''' <summary>
    ''' Flag to indicate that, when rendered, the group should <em>not</em> have its
    ''' members shown in the component tree by default. This may be overridden by
    ''' subclasses.
    ''' <seealso cref="GroupComponent.ShowMembersInComponentTree"/>
    ''' </summary>
    Public Overridable ReadOnly Property ShowMembersInComponentTree() As Boolean
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Gets the actual collection which is holding the contents of this group.
    ''' Note that modifying this collection directly will bypass the setting of the
    ''' group property on any components in the group - this will need to be handled
    ''' carefully by any calling code
    ''' </summary>
    Protected ReadOnly Property InnerMembers() As IBPSet(Of PackageComponent)
        Get
            If mMembers Is Nothing Then mMembers = New clsSortedSet(Of PackageComponent)
            Return mMembers
        End Get
    End Property

    ''' <summary>
    ''' The type of this component - used for image keys and for order lookups
    ''' amongst other things.
    ''' <seealso cref="PackageComponentType.Keys"/>
    ''' </summary>
    Public MustOverride Overrides ReadOnly Property Type() As PackageComponentType

#End Region

#Region " XML Methods "

    ''' <summary>
    ''' Appends this group as XML to the given XML Writer.
    ''' This adds a basic start element using the typekey, then a <c>members</c>
    ''' element which contains a reference to the members of this group, as
    ''' written by a call to <see cref="PackageComponent.AppendReference"/>.
    ''' </summary>
    ''' <param name="writer">The writer to which this group is appended.</param>
    Protected Overrides Sub WriteXmlBody(ByVal writer As XmlWriter)
        AppendMemberReferences(writer)
    End Sub

    ''' <summary>
    ''' Appends the members of this group into the given XML writer as an element
    ''' named <c>members</c>
    ''' </summary>
    ''' <param name="writer">The writer to which the members of this group should
    ''' be appended.</param>
    Protected Sub AppendMemberReferences(ByVal writer As XmlWriter)
        writer.WriteStartElement(MembersXmlElementName)
        ' Append references to all the members of the group.
        For Each cd As PackageComponent In InnerMembers
            cd.AppendReference(writer)
        Next
        writer.WriteEndElement() ' members
    End Sub

    ''' <summary>
    ''' Reads the member references from the given XML reader. This assumes it
    ''' can continue reading the reader until EOF, so a subtree should be used
    ''' if this is not the case.
    ''' The reader should be positioned on or immediately before the members
    ''' element so that the member references can be read correctly 
    ''' </summary>
    ''' <param name="r">The XML reader to use to read the data.</param>
    ''' <param name="ctx">The loading context.</param>
    Protected Sub ReadMemberReferences( _
     ByVal r As XmlReader, ByVal ctx As IComponentLoadingContext)

        r.MoveToContent()
        Debug.Assert(r.NodeType = XmlNodeType.Element AndAlso r.LocalName = MembersXmlElementName)

        If r.IsEmptyElement Then Return ' No members to read

        ' Read each element, resolving them through the loading context.
        While r.Read()
            If r.NodeType = XmlNodeType.Element Then
                Dim tp As PackageComponentType = PackageComponentType.AllTypes(r.LocalName)
                Dim id As Object = tp.ConvertId(r("id"))
                Dim comp As PackageComponent = ctx.GetComponent(tp, id)

                If comp Is Nothing Then Throw New NoSuchElementException(
                 "Couldn't load the {0} with id '{1}' from the context: {2}",
                 tp.Label, id, ctx)

                Add(comp)
            End If
        End While

    End Sub

    ''' <summary>
    ''' Reads the XML body from the given XML reader.
    ''' </summary>
    ''' <param name="r">The reader providing the XML for this group.</param>
    Protected Overrides Sub ReadXmlBody( _
     ByVal r As XmlReader, ByVal ctx As IComponentLoadingContext)

        ' Read down to the start of the members element
        While Not (r.NodeType = XmlNodeType.Element AndAlso r.LocalName = MembersXmlElementName)
            If Not r.Read() Then Return ' If we've overshot and finished the reader, leave
        End While

        ReadMemberReferences(r.ReadSubtree(), ctx)

    End Sub

#End Region

#Region " Collection Implementations "

    ''' <summary>
    ''' Adds the specified component to this group.
    ''' </summary>
    ''' <param name="item">The component to add.</param>
    Public Overridable Sub Add(ByVal item As PackageComponent) _
     Implements ICollection(Of PackageComponent).Add
        If item Is Nothing Then Return
        InnerMembers.Add(item)
    End Sub

    ''' <summary>
    ''' Adds the given components to this group.
    ''' </summary>
    ''' <param name="comps">The components to add to this group</param>
    Public Sub AddAll(ByVal comps As IEnumerable(Of PackageComponent))
        If comps Is Nothing Then Return
        For Each c As PackageComponent In comps
            Add(c)
        Next
    End Sub

    ''' <summary>
    ''' Clears the members of this group.
    ''' </summary>
    Public Sub Clear() Implements ICollection(Of PackageComponent).Clear
        mMembers = Nothing
    End Sub

    ''' <summary>
    ''' Checks if this group contains the given component
    ''' </summary>
    ''' <param name="item">The item to check for.</param>
    ''' <returns>True if the item is within this group, False otherwise.</returns>
    Public Function Contains(ByVal item As PackageComponent) As Boolean _
     Implements ICollection(Of PackageComponent).Contains
        Return (mMembers IsNot Nothing AndAlso mMembers.Contains(item))
    End Function

    ''' <summary>
    ''' Checks if this group contains the given object - ie. not just a component
    ''' with the same value, but exactly the same object by reference.
    ''' </summary>
    ''' <param name="item">The component to search for within this group.</param>
    ''' <returns>True if the given object is a member of this group. False otherwise,
    ''' regardless of whether a component of the same value is held in this group.
    ''' </returns>
    Public Function ContainsByReference(ByVal item As PackageComponent) As Boolean
        If mMembers Is Nothing OrElse item Is Nothing Then Return False
        For Each comp As PackageComponent In mMembers
            If comp Is item Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' Copies this group into the given array.
    ''' </summary>
    ''' <param name="array">The array to copy into</param>
    ''' <param name="arrayIndex">The index at which this group's members should be
    ''' copied.</param>
    Public Sub CopyTo(ByVal array() As PackageComponent, ByVal arrayIndex As Integer) _
     Implements ICollection(Of PackageComponent).CopyTo
        If mMembers IsNot Nothing Then mMembers.CopyTo(array, arrayIndex)
    End Sub

    ''' <summary>
    ''' Gets the number of components held in this group.
    ''' </summary>
    Public ReadOnly Property Count() As Integer Implements ICollection(Of PackageComponent).Count
        Get
            If mMembers Is Nothing Then Return 0
            Return mMembers.Count
        End Get
    End Property

    ''' <summary>
    ''' Checks if this group is read only. It is not.
    ''' </summary>
    Public ReadOnly Property IsReadOnly() As Boolean _
     Implements ICollection(Of PackageComponent).IsReadOnly
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Attempts to remove the given component from this group, return true or false
    ''' if successful or otherwise, respectively.
    ''' </summary>
    ''' <param name="item">The item to remove.</param>
    ''' <returns>True if the item was found and removed from this group, False if it
    ''' was not in this group.</returns>
    Public Function Remove(ByVal item As PackageComponent) As Boolean _
     Implements ICollection(Of PackageComponent).Remove
        Return (mMembers IsNot Nothing AndAlso mMembers.Remove(item))
    End Function

    ''' <summary>
    ''' Gets an enumerator over the components in this group.
    ''' </summary>
    ''' <returns>An enumerator over the components in this group.</returns>
    Public Function GetEnumerator() As IEnumerator(Of PackageComponent) _
     Implements IEnumerable(Of PackageComponent).GetEnumerator
        If mMembers Is Nothing Then Return GetEmpty.IEnumerator(Of PackageComponent)()
        Return mMembers.GetEnumerator()
    End Function

    ''' <summary>
    ''' Gets an enumerator over the components in this group.
    ''' </summary>
    ''' <returns>An enumerator over the components in this group.</returns>
    Private Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

#End Region

#Region " Other methods "

    ''' <summary>
    ''' Clone this group using the specified cloning context.
    ''' The context provides the point from which components which have already
    ''' been cloned within the current context can be drawn. If they don't already
    ''' exist, the will be cloned on access, so that the full object map can be
    ''' correctly created with all references intact.
    ''' </summary>
    ''' <param name="ctx">The context in which this clone operation is taking place.
    ''' </param>
    ''' <returns>A clone of this component group.</returns>
    Protected Overrides Function Clone(ByVal ctx As CloningContext) As PackageComponent
        Dim copy As ComponentGroup = DirectCast(MyBase.Clone(ctx), ComponentGroup)
        If mMembers IsNot Nothing Then
            copy.mMembers = Nothing
            For Each origComp As PackageComponent In mMembers
                copy.Add(ctx(origComp.Type, origComp.Id))
            Next
        End If
        Return copy
    End Function

    ''' <summary>
    ''' Clones this component group's basic data - ie. all the data bar its group
    ''' data - neither its members nor its parent group are included
    ''' </summary>
    ''' <returns>A disconnected clone of this component group</returns>
    Public Overrides Function CloneDisconnected() As PackageComponent
        Dim gp As ComponentGroup = DirectCast(MyBase.CloneDisconnected(), ComponentGroup)
        gp.mMembers = Nothing
        Return gp
    End Function

    ''' <summary>
    ''' Gets a string representation of this group, including its members.
    ''' </summary>
    ''' <returns>A string representation of this group.</returns>
    Public Overrides Function ToString() As String
        Return String.Format("{0}: {1}({2}) <{3}>", _
         Me.GetType().Name, Me.Name, Me.Id, CollectionUtil.ToString(Me.Members))
    End Function

    ''' <summary>
    ''' Finds the component with the given type and name within this group.
    ''' </summary>
    ''' <param name="type">The type of component to search for.</param>
    ''' <param name="name">The name of the component to search for</param>
    ''' <returns>The component, directly in this group, which matches the given type
    ''' and name, or null if no such component exists.</returns>
    Public Function FindComponent(ByVal type As PackageComponentType, ByVal name As String) _
     As PackageComponent
        For Each comp As PackageComponent In Me
            If comp.Type = type AndAlso comp.Name = name Then Return comp
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Finds the component with the given type and ID within this group.
    ''' </summary>
    ''' <param name="type">The type of component to search for.</param>
    ''' <param name="id">The ID of the component to search for</param>
    ''' <returns>The component, directly in this group, which matches the given type
    ''' and ID, or null if no such component exists.</returns>
    Public Function FindComponent(ByVal type As PackageComponentType, ByVal id As Object) _
     As PackageComponent
        For Each comp As PackageComponent In Me
            If comp.Type = type AndAlso Object.Equals(comp.Id, id) Then Return comp
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' A very simplistic comparison method, which just checks if the data in the
    ''' given component differs from the data in this component. This implementation
    ''' checks the <see cref="m:PackageComponent.Differs">base implementation</see>
    ''' and the contents of the group to ensure that the components in the group
    ''' match.
    ''' </summary>
    ''' <param name="comp">The component to check against.</param>
    ''' <returns>True if the given component differs from this component. False if
    ''' its data is identical.</returns>
    Public Overrides Function Differs(ByVal comp As PackageComponent) As Boolean
        ' If any base stuff differs, then we don't need to even check.
        If MyBase.Differs(comp) Then Return True

        ' We need to check if we have the same members... easiest way - set arithmetic
        Dim s As New clsSet(Of PackageComponent)(Me.Members)
        s.Difference(DirectCast(comp, ComponentGroup).Members)
        Return (s.Count > 0)
    End Function

#End Region

End Class
