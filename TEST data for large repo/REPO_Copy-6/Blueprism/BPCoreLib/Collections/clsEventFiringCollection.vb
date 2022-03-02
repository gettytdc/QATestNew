Namespace Collections

    ''' <summary>
    ''' Collection class which fires events when any of the modification methods
    ''' are successfully called.
    ''' Note that calls made directly to the wrapped collection cannot be captured,
    ''' and events won't be raised for those. In order to ensure that 
    ''' </summary>
    ''' <typeparam name="T">The type of element held by this collection</typeparam>
    <Serializable, DebuggerDisplay("Count: {Count}")> _
    Public Class clsEventFiringCollection(Of T)
        Implements ICollection(Of T)

        ''' <summary>
        ''' Event fired when an element is added to the collection. Note that there
        ''' is no way of knowing whether the collection changed as a result of the
        ''' element being added, due to the restrictions in the ICollection interface
        ''' </summary>
        ''' <param name="sender">The collection to which the item was added.</param>
        ''' <param name="item">The item that was added.</param>
        Public Event ItemAdded(ByVal sender As ICollection(Of T), ByVal item As T)

        ''' <summary>
        ''' Event fired when an element is successfully removed from the collection.
        ''' </summary>
        ''' <param name="sender">The collection from which the element was removed.
        ''' </param>
        ''' <param name="item">The item which was removed.</param>
        Public Event ItemRemoved(ByVal sender As ICollection(Of T), ByVal item As T)

        ''' <summary>
        ''' Event fired when a collection was cleared. Note that this may have no
        ''' effect on the collection (if it was empty to begin with), but the event
        ''' is fired regardless.
        ''' </summary>
        ''' <param name="sender">The collection which has been cleared.</param>
        ''' <param name="itemsCleared">A readonly collection containing the items
        ''' which have been cleared from this collection</param>
        Public Event CollectionCleared(ByVal sender As ICollection(Of T), _
         ByVal itemsCleared As ICollection(Of T))

        ''' <summary>
        ''' The collection that we are capturing modification events on.
        ''' </summary>
        Private _coll As ICollection(Of T)

        ''' <summary>
        ''' Creates a new event-firing collection wrapping the given collection.
        ''' </summary>
        ''' <param name="coll">The collection to monitor changes on.</param>
        Public Sub New(ByVal coll As ICollection(Of T))
            _coll = coll
        End Sub

        ''' <summary>
        ''' The contained collection wrapped by this event firing collection
        ''' </summary>
        Public ReadOnly Property InnerCollection() As ICollection(Of T)
            Get
                Return _coll
            End Get
        End Property

        ''' <summary>
        ''' Adds the given item to the underlying collection and raises an ItemAdded
        ''' event.
        ''' </summary>
        ''' <param name="item">The item to be added.</param>
        Public Overridable Sub Add(ByVal item As T) Implements ICollection(Of T).Add
            _coll.Add(item)
            OnItemAdded(item)
        End Sub

        ''' <summary>
        ''' Clears the underlying collection and raises a CollectionCleared event
        ''' </summary>
        Public Overridable Sub Clear() Implements ICollection(Of T).Clear
            Dim items(_coll.Count - 1) As T
            _coll.CopyTo(items, 0)
            _coll.Clear()
            OnCollectionCleared(items)
        End Sub

        ''' <summary>
        ''' Checks if the underlying collection contains the given item.
        ''' </summary>
        ''' <param name="item">The item to search for.</param>
        ''' <returns>True if the underlying collection contains the specified
        ''' element, false otherwise.</returns>
        Public Function Contains(ByVal item As T) As Boolean _
         Implements ICollection(Of T).Contains
            Return _coll.Contains(item)
        End Function

        ''' <summary>
        ''' Copies the contents of the underlying collection to the given array
        ''' starting at the given index.
        ''' </summary>
        ''' <param name="array">The array to which the collection should be copied.
        ''' </param>
        ''' <param name="arrayIndex">The index at which the copying should commence.
        ''' </param>
        Public Sub CopyTo(ByVal array() As T, ByVal arrayIndex As Integer) _
         Implements ICollection(Of T).CopyTo
            _coll.CopyTo(array, arrayIndex)
        End Sub

        ''' <summary>
        ''' Gets a count of elements from the underlying collection.
        ''' </summary>
        Public ReadOnly Property Count() As Integer _
         Implements ICollection(Of T).Count
            Get
                Return _coll.Count
            End Get
        End Property

        ''' <summary>
        ''' Checks if the underlying collection is read-only or not.
        ''' </summary>
        Public ReadOnly Property IsReadOnly() As Boolean _
         Implements ICollection(Of T).IsReadOnly
            Get
                Return _coll.IsReadOnly
            End Get
        End Property

        ''' <summary>
        ''' Removes the given element from the underlying collection, and raises an
        ''' ItemRemoved event if it was, in fact, removed.
        ''' </summary>
        ''' <param name="item">The element to remove</param>
        ''' <returns>True to indicate that the item was successfully removed from
        ''' the underlying collection; False if it didn't exist therein and thus
        ''' was not removed.</returns>
        ''' <remarks>Note that the event is only raised if the underlying collection
        ''' returns True to the remove method call.</remarks>
        Public Overridable Function Remove(ByVal item As T) As Boolean _
         Implements ICollection(Of T).Remove
            If _coll.Remove(item) Then
                OnItemRemoved(item)
                Return True
            End If
            Return False
        End Function

        ''' <summary>
        ''' Gets the enumerator over the elements in the underlying collection.
        ''' </summary>
        ''' <returns>An enumerator over the collection's elements.</returns>
        Public Function GetEnumerator() As IEnumerator(Of T) _
         Implements IEnumerable(Of T).GetEnumerator
            Return _coll.GetEnumerator()
        End Function

        ''' <summary>
        ''' Gets the enumerator over the elements in the underlying collection.
        ''' </summary>
        ''' <returns>An enumerator over the collection's elements.</returns>
        Private Function GetNonGenericEnumerator() As IEnumerator _
         Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function

        ''' <summary>
        ''' Triggers an <see cref="ItemAdded"/> event for the given item
        ''' </summary>
        ''' <param name="item">The item to raise the added event for</param>
        Protected Sub OnItemAdded(ByVal item As T)
            RaiseEvent ItemAdded(Me, item)
        End Sub

        ''' <summary>
        ''' Triggers an <see cref="ItemRemoved"/> event for the given item
        ''' </summary>
        ''' <param name="item">The item to raise the added event for</param>
        Protected Sub OnItemRemoved(ByVal item As T)
            RaiseEvent ItemRemoved(Me, item)
        End Sub

        ''' <summary>
        ''' Triggers a <see cref="CollectionCleared"/> event
        ''' </summary>
        ''' <param name="itemsCleared">A collection of items that were cleared.
        ''' </param>
        Protected Sub OnCollectionCleared(ByVal itemsCleared As ICollection(Of T))
            RaiseEvent CollectionCleared(Me, GetReadOnly.ICollection(itemsCleared))
        End Sub

    End Class

End Namespace
