Namespace Collections

    ''' <summary>
    ''' Class to add strong typing to a non-generic collection
    ''' </summary>
    ''' <typeparam name="T">The type of element held in the underlying collection.
    ''' </typeparam>
    ''' <remarks>Note that ICollection offers no mutation properties or set methods,
    ''' so this collection object is, by necessity, read only.</remarks>
    <Serializable, DebuggerDisplay("Count: {Count}")>
    Public Class clsTypedCollection(Of T) : Inherits clsTypedEnumerable(Of T)
        Implements ICollection(Of T)

        ''' <summary>
        ''' Creates a new typed collection around the given non-generic collection.
        ''' </summary>
        ''' <param name="coll">The collection to wrap</param>
        Public Sub New(ByVal coll As ICollection)
            MyBase.New(coll)
        End Sub

        ''' <summary>
        ''' The wrapped collection
        ''' </summary>
        Protected ReadOnly Property WrappedCollection() As ICollection
            Get
                Return DirectCast(WrappedEnumerable, ICollection)
            End Get
        End Property

        ''' <summary>
        ''' Tries (and fails) to add the given item to the underlying collection.
        ''' Since the ICollection interface offers no way to add an item, this
        ''' implementation just fails with an InvalidOperationException
        ''' </summary>
        ''' <param name="item">The item to not add</param>
        ''' <exception cref="InvalidOperationException">When called.</exception>
        Public Overridable Sub Add(ByVal item As T) Implements ICollection(Of T).Add
            ' ICollection doesn't have an Add() - I never realised
            Throw New InvalidOperationException(My.Resources.clsTypedCollection_CannotAddToANonGenericCollection)
        End Sub

        ''' <summary>
        ''' Tries (and fails) to clear the wrapped collection. Since the ICollection
        ''' interface offers no way to clear itself, this implementation just fails
        ''' with an InvalidOperationException
        ''' </summary>
        ''' <exception cref="InvalidOperationException">When called</exception>
        Public Overridable Sub Clear() Implements ICollection(Of T).Clear
            ' ICollection is read-only. Hmm.
            Throw New InvalidOperationException(My.Resources.clsTypedCollection_WrappedICollectionIsReadOnly)
        End Sub

        ''' <summary>
        ''' Checks if the wrapped collection contains the given item.
        ''' </summary>
        ''' <param name="item">The item to search for</param>
        ''' <returns>True if the given item is in this collection, false otherwise.
        ''' </returns>
        Public Overridable Function Contains(ByVal item As T) As Boolean _
         Implements ICollection(Of T).Contains
            For Each el As T In Me
                If Object.Equals(el, item) Then Return True
            Next
            Return False
        End Function

        ''' <summary>
        ''' Copies the wrapped collection into the given array at the specified
        ''' index.
        ''' </summary>
        ''' <param name="array">The array into which the collection should be copied
        ''' </param>
        ''' <param name="arrayIndex">The index at which to start copying the
        ''' collection</param>
        Public Sub CopyTo(ByVal array() As T, ByVal arrayIndex As Integer) _
         Implements ICollection(Of T).CopyTo
            WrappedCollection.CopyTo(array, arrayIndex)
        End Sub

        ''' <summary>
        ''' The count of elements in the collection
        ''' </summary>
        Public Overridable ReadOnly Property Count() As Integer Implements ICollection(Of T).Count
            Get
                Return WrappedCollection.Count
            End Get
        End Property

        ''' <summary>
        ''' Checks if this is a read-only or not. It is.
        ''' </summary>
        Public Overridable ReadOnly Property IsReadOnly() As Boolean _
         Implements ICollection(Of T).IsReadOnly
            Get
                Return True
            End Get
        End Property

        ''' <summary>
        ''' Tries (and fails) to remove the given item from this collection. Since
        ''' the ICollection interface offers no way to remove an item, this simply
        ''' fails with an InvalidOperationException.
        ''' </summary>
        ''' <param name="item">The item to fail to remove.</param>
        ''' <returns>Nothing</returns>
        ''' <exception cref="InvalidOperationException">When called</exception>
        Public Overridable Function Remove(ByVal item As T) As Boolean _
         Implements ICollection(Of T).Remove
            Throw New InvalidOperationException(My.Resources.clsTypedCollection_WrappedICollectionIsReadOnly)
        End Function

    End Class

End Namespace
