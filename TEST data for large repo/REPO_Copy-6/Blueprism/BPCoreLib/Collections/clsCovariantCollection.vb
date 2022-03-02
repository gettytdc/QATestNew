Namespace Collections

    ''' Project  : BPCoreLib
    ''' Class    : clsCovariantCollection
    ''' <summary>
    ''' <para>
    ''' Transitional collection class which enables covariance in the generic
    ''' types of collections, such that an ICollection(Of String) can be passed
    ''' to a method expecting an ICollection(Of Object) or collections with any
    ''' other subtype:type relationship.
    ''' </para><para>
    ''' Note that any collections wrapped into a covariant collection are
    ''' affected by any changes made to <em>this</em> collection. The converse
    ''' is also true - any changes made to the wrapped collection will be
    ''' reflected in this collection.
    ''' </para>
    ''' </summary>
    ''' <typeparam name="S">The type of collection required. This should be a 
    ''' base type of the type of collection that you have. This provides an
    ''' interface to the generic ICollection of this type.</typeparam>
    ''' <typeparam name="T">The type of collection currently held. This should
    ''' be a subtype of the type required.</typeparam>
    ''' <remarks>Note that, by its nature, this class removes some of the compile
    ''' time checking of types within collections, and thus care needs to be
    ''' taken to ensure that objects added to this collection can be cast into
    ''' type 'T'... otherwise an InvalidCastException will be thrown at the 
    ''' point of trying to add the item. Likewise for removing an item, or
    ''' checking if the collection contains an item.</remarks>
    <Serializable, DebuggerDisplay("Count: {Count}")> _
    Public Class clsCovariantCollection(Of S, T As {S})
        Implements ICollection(Of S)

        ''' <summary>
        ''' The collection that this object is wrapping.
        ''' </summary>
        Private _coll As ICollection(Of T)

        ''' <summary>
        ''' Gets the collection that this collection is wrapping
        ''' </summary>
        Protected ReadOnly Property WrappedCollection() As ICollection(Of T)
            Get
                Return _coll
            End Get
        End Property

        ''' <summary>
        ''' Creates a new covariant collection wrapping the given collection as
        ''' a collection of type 'S' - ie. a base type of 'T'.
        ''' </summary>
        ''' <param name="coll">The collection that is used as the source of the
        ''' data for this collection.</param>
        Public Sub New(ByVal coll As ICollection(Of T))
            _coll = coll
        End Sub

        ''' <summary>
        ''' Adds the given item to this collection.
        ''' </summary>
        ''' <param name="item">The item to add.</param>
        ''' <exception cref="InvalidCastException">If the given item is not of
        ''' the wrapped type 'T' defined when creating this collection.
        ''' </exception>
        Public Sub Add(ByVal item As S) Implements ICollection(Of S).Add
            _coll.Add(DirectCast(item, T))
        End Sub

        ''' <summary>
        ''' Clears this collection and, thus, the wrapped collection, removing all
        ''' elements that are currently held on it.
        ''' </summary>
        Public Sub Clear() Implements ICollection(Of S).Clear
            _coll.Clear()
        End Sub

        ''' <summary>
        ''' Checks if this collection contains the given item.
        ''' </summary>
        ''' <param name="item">The item to check to see if it occurs in this
        ''' collection.</param>
        ''' <returns>True if the given item is in this collection; False
        ''' otherwise.</returns>
        ''' <exception cref="InvalidCastException">If the given item is not of
        ''' the wrapped type 'T' defined when creating this collection.
        ''' </exception>
        Public Function Contains(ByVal item As S) As Boolean Implements ICollection(Of S).Contains
            Return _coll.Contains(DirectCast(item, T))
        End Function

        ''' <summary>
        ''' Copies the contents of this collection to the given array.
        ''' </summary>
        ''' <param name="arr">The array to copy this collection's items to.
        ''' </param>
        ''' <param name="arrayIndex">The index at which the items should be
        ''' copied.</param>
        ''' <exception cref="ArgumentOutOfRangeException">arrayIndex is less
        ''' than 0</exception>
        ''' <exception cref="ArgumentNullException">arr is null</exception>
        ''' <exception cref="ArgumentException">array is multidimensional -or- 
        ''' arrayIndex is equal to or greater than the length of array -or- 
        ''' The number of elements in the source ICollection(Of T) is greater
        ''' than the available space from arrayIndex to the end of the
        ''' destination array -or- Type T cannot be cast automatically to the
        ''' type of the destination array. </exception>
        Public Sub CopyTo(ByVal arr() As S, ByVal arrayIndex As Integer) _
         Implements ICollection(Of S).CopyTo

            Dim tArr(_coll.Count - 1) As T
            _coll.CopyTo(tArr, 0)

            Array.Copy(tArr, 0, arr, arrayIndex, tArr.Length)

        End Sub

        ''' <summary>
        ''' A count of the number of elements in this collection.
        ''' </summary>
        Public ReadOnly Property Count() As Integer Implements ICollection(Of S).Count
            Get
                Return _coll.Count
            End Get
        End Property

        ''' <summary>
        ''' Flag indicating if this collection is read-only or not.
        ''' </summary>
        Public ReadOnly Property IsReadOnly() As Boolean Implements ICollection(Of S).IsReadOnly
            Get
                Return _coll.IsReadOnly
            End Get
        End Property

        ''' <summary>
        ''' Removes the given item from this collection, if it currently exists
        ''' within it.
        ''' </summary>
        ''' <param name="item">The item to remove.</param>
        ''' <returns>True if the given item was found and removed; False if it was
        ''' not found and thus not removed.</returns>
        ''' <exception cref="InvalidCastException">If the given item is not of the
        ''' wrapped type 'T' defined when creating this collection.</exception>
        Public Function Remove(ByVal item As S) As Boolean Implements ICollection(Of S).Remove
            Return _coll.Remove(DirectCast(item, T))
        End Function

        ''' <summary>
        ''' Gets an enumerator over the elements held in this collection.
        ''' </summary>
        ''' <returns>An enumerator over this collection's elements.</returns>
        Public Function GetEnumerator() As IEnumerator(Of S) Implements IEnumerable(Of S).GetEnumerator
            Return New clsCovariantEnumerator(Of S, T)(_coll.GetEnumerator())
        End Function

        ''' <summary>
        ''' Gets an enumerator over the elements held in this collection.
        ''' </summary>
        ''' <returns>An enumerator over this collection's elements.</returns>
        Private Function GetNonGenericEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            ' No need to bother with a covariance wrapper for IEnumerable
            Return _coll.GetEnumerator()
        End Function

    End Class

End Namespace
