''' <summary>
''' Provides a collection which raises events when important
''' things happen, such as new items being added, or items
''' being removed.
''' </summary>
''' <typeparam name="T">The generic type of the items 
''' contained in the collection.</typeparam>
Public Class clsCollectionWithEvents(Of T)
    Implements System.Collections.Generic.IEnumerable(Of T)

    ''' <summary>
    ''' Event raised when an item is added to the collection.
    ''' </summary>
    ''' <param name="NewItem">The newly added item.</param>
    Public Event ItemAdded(ByVal NewItem As T)
    ''' <summary>
    ''' Event raised when a range of items is added to the collection.
    ''' </summary>
    ''' <param name="NewItems">The newly added items.</param>
    Public Event ItemRangeAdded(ByVal NewItems As List(Of T))
    ''' <summary>
    ''' Event raised when new item inserted.
    ''' </summary>
    ''' <param name="index">The index at which the item was
    ''' inserted.</param>
    ''' <param name="NewItem">The newly inserted item.</param>
    Public Event ItemInserted(ByVal index As Integer, ByVal NewItem As T)
    ''' <summary>
    ''' Event raised when an item is removed.
    ''' </summary>
    ''' <param name="RemovedItem">The item that was 
    ''' removed.</param>
    Public Event ItemRemoved(ByVal RemovedItem As T, ByVal OldIndex As Integer)
    ''' <summary>
    ''' Event raised when an item is replaced in the collection.
    ''' </summary>
    ''' <param name="index">The index of the item being replaced.</param>
    ''' <param name="OldItem">The item replaced.</param>
    ''' <param name="NewItem">The new item.</param>
    Public Event ItemChanged(ByVal index As Integer, ByVal OldItem As T, ByVal NewItem As T)
    ''' <summary>
    ''' Event raised when the capacity of the collection is changed.
    ''' </summary>
    ''' <param name="OldCapacity">The old capacity.</param>
    ''' <param name="NewCapacity">The new capacity.</param>
    Public Event CapacityChanged(ByVal OldCapacity As Integer, ByVal NewCapacity As Integer)
    ''' <summary>
    ''' Event raised when all items in the collection are cleared.
    ''' </summary>
    Public Event CollectionCleared(ByVal ItemsCleared As List(Of T))

    ''' <summary>
    ''' Event raised when the collection is sorted.
    ''' </summary>
    Public Event CollectionSorted()


    Public Sub Insert(ByVal index As Integer, ByVal value As T)
        Me.mInternalCollection.Insert(index, value)
        RaiseEvent ItemInserted(index, value)
    End Sub

    ''' <summary>
    ''' Collection used internally to provide essential
    ''' collection functionality.
    ''' </summary>
    Private mInternalCollection As New List(Of T)

    ''' <summary>
    ''' See corresponding method on Collection.
    ''' </summary>
    Public Sub Add(ByVal value As T)
        Me.mInternalCollection.Add(value)
        RaiseEvent ItemAdded(value)
    End Sub

    ''' <summary>
    ''' See corresponding method on Collection.
    ''' </summary>
    Public Sub AddRange(ByVal values As List(Of T))
        Me.mInternalCollection.AddRange(values)
        RaiseEvent ItemRangeAdded(values)
    End Sub

    ''' <summary>
    ''' See corresponding method on Collection.
    ''' </summary>
    Public Function AsReadOnly() As ObjectModel.ReadOnlyCollection(Of T)
        Return Me.mInternalCollection.AsReadOnly
    End Function

    ''' <summary>
    ''' See corresponding method on Collection.
    ''' </summary>
    Public Property Capacity() As Integer
        Get
            Return Me.mInternalCollection.Capacity
        End Get
        Set(ByVal value As Integer)
            Dim OldCapacity As Integer = Me.mInternalCollection.Capacity
            Me.mInternalCollection.Capacity = value
            If Not value = OldCapacity Then RaiseEvent CapacityChanged(OldCapacity, value)
        End Set
    End Property

    ''' <summary>
    ''' See corresponding method on Collection.
    ''' </summary>
    Public Sub Clear()
        Dim Items As New List(Of T)
        For Each item As T In Me.mInternalCollection
            Items.Add(item)
        Next
        Me.mInternalCollection.Clear()
        RaiseEvent CollectionCleared(Items)
    End Sub

    ''' <summary>
    ''' See corresponding method on Collection.
    ''' </summary>
    Public Function Remove(ByVal value As T) As Boolean
        Dim OldIndex As Integer = Me.IndexOf(value)
        Dim Success As Boolean = Me.mInternalCollection.Remove(value)
        If Success Then RaiseEvent ItemRemoved(value, OldIndex)
        Return Success
    End Function

    ''' <summary>
    ''' See corresponding method on Collection.
    ''' </summary>
    Public Function Contains(ByVal value As T) As Boolean
        Return Me.mInternalCollection.Contains(value)
    End Function

    ''' <summary>
    ''' See corresponding method on Collection.
    ''' </summary>
    Public ReadOnly Property Count() As Integer
        Get
            Return Me.mInternalCollection.Count
        End Get
    End Property


    ''' <summary>
    ''' See corresponding method on Collection.
    ''' </summary>
    Default Public Property Item(ByVal index As Integer) As T
        Get
            Return Me.mInternalCollection.Item(index)
        End Get
        Set(ByVal value As T)
            Dim OldValue As T = Me.mInternalCollection.Item(index)
            If Not (CType(value, Object) Is CType(OldValue, Object)) Then
                Me.mInternalCollection.Item(index) = value
                RaiseEvent ItemChanged(index, OldValue, value)
            End If
        End Set
    End Property


    ''' <summary>
    ''' See corresponding method on Collection.
    ''' </summary>
    Public Function IndexOf(ByVal value As T) As Integer
        Return Me.mInternalCollection.IndexOf(value)
    End Function

    ''' <summary>
    ''' See corresponding method on Collection.
    ''' </summary>
    Public Function IndexOf(ByVal value As T, ByVal index As Integer) As Integer
        Return Me.mInternalCollection.IndexOf(value, index)
    End Function

    ''' <summary>
    ''' See corresponding method on Collection.
    ''' </summary>
    Public Function IndexOf(ByVal value As T, ByVal index As Integer, ByVal count As Integer) As Integer
        Return Me.mInternalCollection.IndexOf(value, index, count)
    End Function


    ''' <summary>
    ''' See corresponding method on Collection.
    ''' </summary>
    Public Sub Sort()
        Me.mInternalCollection.Sort()
        RaiseEvent CollectionSorted()
    End Sub

    ''' <summary>
    ''' See corresponding method on Collection.
    ''' </summary>
    Public Sub Sort(ByVal comparer As IComparer(Of T))
        Me.mInternalCollection.Sort(comparer)
        RaiseEvent CollectionSorted()
    End Sub

    ''' <summary>
    ''' See corresponding method on Collection.
    ''' </summary>
    Public Sub Sort(ByVal index As Integer, ByVal count As Integer, ByVal comparer As IComparer(Of T))
        Me.mInternalCollection.Sort(index, count, comparer)
        RaiseEvent CollectionSorted()
    End Sub

    ''' <summary>
    ''' See corresponding method on Collection.
    ''' </summary>
    Public Sub Sort(ByVal comparison As System.Comparison(Of T))
        Me.mInternalCollection.Sort(comparison)
        RaiseEvent CollectionSorted()
    End Sub

    ''' <summary>
    ''' See corresponding method on Collection.
    ''' </summary>
    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of T) Implements System.Collections.Generic.IEnumerable(Of T).GetEnumerator
        Return Me.mInternalCollection.GetEnumerator
    End Function

    ''' <summary>
    ''' See corresponding method on Collection.
    ''' </summary>
    Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return Me.mInternalCollection.GetEnumerator
    End Function


End Class

