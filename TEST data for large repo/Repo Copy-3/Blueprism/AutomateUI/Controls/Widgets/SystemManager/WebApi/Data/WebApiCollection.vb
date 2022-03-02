Imports BluePrism.AutomateProcessCore.WebApis
''' <summary>
''' A mutable collection of elements used within a WebApi. This can represent a
''' 'common' collection - ie. one which applies to all Web API actions, or an
''' action-specific collection, ie. one which applies to a specific action.
''' </summary>
''' <typeparam name="T">The type of element held in this collection. t the time of
''' writing, this is either <see cref="HttpHeader"/> or <see cref="ActionParameter"/>
''' </typeparam>
Public Class WebApiCollection(Of T) : Implements ICollection(Of T)

    ' The wrapped collection of elements
    Private ReadOnly mElements As ICollection(Of T)

    ''' <summary>
    ''' Creates a new empty web api collection, representing a common set of
    ''' elements of a specified type.
    ''' </summary>
    Public Sub New()
        mElements = New List(Of T)
    End Sub

    ''' <summary>
    ''' Gets or sets whether this WebApi collection represents an action-specific
    ''' set of elements (False), or a common set of elements (True). The default is
    ''' False - ie. a common set of elements.
    ''' </summary>
    Public Property ActionSpecific As Boolean

    ''' <summary>
    ''' Gets the count of elements in this collection
    ''' </summary>
    Public ReadOnly Property Count As Integer Implements ICollection(Of T).Count
        Get
            Return mElements.Count
        End Get
    End Property

    ''' <summary>
    ''' Gets whether this collection is readonly or modifiable
    ''' </summary>
    Public ReadOnly Property IsReadOnly As Boolean _
     Implements ICollection(Of T).IsReadOnly
        Get
            Return mElements.IsReadOnly
        End Get
    End Property

    ''' <summary>
    ''' Adds the given item to this collection.
    ''' </summary>
    ''' <param name="item">The item to add to this collection</param>
    Public Sub Add(item As T) Implements ICollection(Of T).Add
        mElements.Add(item)
    End Sub

    ''' <summary>
    ''' Clears all elements from this collection
    ''' </summary>
    Public Sub Clear() Implements ICollection(Of T).Clear
        mElements.Clear()
    End Sub

    ''' <summary>
    ''' Copies this collection into an array at a specified index
    ''' </summary>
    ''' <param name="array">The array to which this collection's contents should be
    ''' copied.</param>
    ''' <param name="arrayIndex">The index at which the copying of the collection's
    ''' contents should start</param>
    Public Sub CopyTo(array() As T, arrayIndex As Integer) _
     Implements ICollection(Of T).CopyTo
        mElements.CopyTo(array, arrayIndex)
    End Sub

    ''' <summary>
    ''' Checks if this collection contains the given item
    ''' </summary>
    ''' <param name="item">The item to check for in this collection</param>
    ''' <returns>True if this collection contains <paramref name="item"/>; False
    ''' otherwise.</returns>
    Public Function Contains(item As T) As Boolean _
     Implements ICollection(Of T).Contains
        Return mElements.Contains(item)
    End Function

    ''' <summary>
    ''' Removes the first instance of the given item from this collection
    ''' </summary>
    ''' <param name="item">The item to remove</param>
    ''' <returns>True if <paramref name="item"/> was removed; ie. if this collection
    ''' has been modified by the Remove operation.</returns>
    Public Function Remove(item As T) As Boolean Implements ICollection(Of T).Remove
        Return mElements.Remove(item)
    End Function

    ''' <summary>
    ''' Gets an enumerator over the items in this collection.
    ''' </summary>
    ''' <returns>An enumerator over the elements within this collection.</returns>
    Public Function GetEnumerator() As IEnumerator(Of T) _
     Implements IEnumerable(Of T).GetEnumerator
        Return mElements.GetEnumerator()
    End Function

    ''' <summary>
    ''' Gets an enumerator over the items in this collection.
    ''' </summary>
    ''' <returns>An enumerator over the elements within this collection.</returns>
    Private Function IEnumerable_GetEnumerator() As IEnumerator _
     Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

End Class
