Namespace Collections

    ''' <summary>
    ''' Provides a mechanism to get read-only empty collections.
    ''' </summary>
    Public Class GetEmpty

        ''' <summary>
        ''' Gets an empty generic enumerator.
        ''' </summary>
        ''' <typeparam name="T">The type of element not to enumerate over</typeparam>
        ''' <returns>An empty enumerator over no objects of type T</returns>
        Public Shared Function IEnumerator(Of T)() As IEnumerator(Of T)
            Return New EmptyEnumerator(Of T)
        End Function

        ''' <summary>
        ''' Gets an empty IEnumerable. Under the hood this is actually an
        ''' <see cref="EmptyCollection(Of Object)"/>, which is an arbitrary choice of
        ''' something which implements IEnumerator.
        ''' </summary>
        ''' <returns>An empty enumerable with no entries</returns>
        Public Shared Function IEnumerable() As IEnumerable
            Return IEnumerable(Of Object)()
        End Function

        ''' <summary>
        ''' Gets an empty IEnumerable. Under the hood this is actually an
        ''' <see cref="EmptyCollection(Of T)"/>, but the main thing is that it
        ''' implements IEnumerator(Of T).
        ''' </summary>
        ''' <returns>An empty enumerable with no entries of type T</returns>
        Public Shared Function IEnumerable(Of T)() As IEnumerable(Of T)
            Return New EmptyCollection(Of T)
        End Function

        ''' <summary>
        ''' Gets an empty generic collection.
        ''' </summary>
        ''' <typeparam name="T">The type of element which isn't in the collection.
        ''' </typeparam>
        ''' <returns>An empty readonly collection of the given type.</returns>
        Public Shared Function ICollection(Of T)() As ICollection(Of T)
            Return New EmptyCollection(Of T)
        End Function

        ''' <summary>
        ''' Gets an empty generic list
        ''' </summary>
        ''' <typeparam name="T">The type of element which isn't in the list.
        ''' </typeparam>
        ''' <returns>An empty readonly list of the given type</returns>
        Public Shared Function IList(Of T)() As IList(Of T)
            Return New EmptyList(Of T)
        End Function

        ''' <summary>
        ''' Gets an empty generic set
        ''' </summary>
        ''' <typeparam name="T">The type of element which isn't in the set.
        ''' </typeparam>
        ''' <returns>An empty readonly set of the given type.</returns>
        Public Shared Function IBPSet(Of T)() As IBPSet(Of T)
            Return New EmptySet(Of T)
        End Function

        ''' <summary>
        ''' Gets an empty generic dictionary.
        ''' </summary>
        ''' <typeparam name="TKey">The type of key not held in this map</typeparam>
        ''' <typeparam name="TValue">The type of value not held in this map
        ''' </typeparam>
        Public Shared Function IDictionary(Of TKey, TValue)() As IDictionary(Of TKey, TValue)
            Return New EmptyDictionary(Of TKey, TValue)
        End Function

#Region " Implementations "

        ''' <summary>
        ''' Class implementing an empty dictionary.
        ''' </summary>
        ''' <typeparam name="TKey">The type of key not held in this map</typeparam>
        ''' <typeparam name="TValue">The type of value not held in this map
        ''' </typeparam>
        <Serializable, DebuggerDisplay("Count: {Count}")> _
        Private Class EmptyDictionary(Of TKey, TValue)
            Inherits EmptyCollection(Of KeyValuePair(Of TKey, TValue))
            Implements IDictionary(Of TKey, TValue)

            Public Sub Add1(ByVal key As TKey, ByVal value As TValue) Implements IDictionary(Of TKey, TValue).Add
                FireReadOnly()
            End Sub

            Public Function ContainsKey(ByVal key As TKey) As Boolean Implements IDictionary(Of TKey, TValue).ContainsKey
                Return False
            End Function

            Default Public Property Item(ByVal key As TKey) As TValue Implements IDictionary(Of TKey, TValue).Item
                Get
                    Throw New KeyNotFoundException(My.Resources.EmptyDictionary_ThisDictionaryIsEmpty)
                End Get
                Set(ByVal value As TValue)
                    FireReadOnly()
                End Set
            End Property

            Public ReadOnly Property Keys() As ICollection(Of TKey) Implements IDictionary(Of TKey, TValue).Keys
                Get
                    Return GetEmpty.ICollection(Of TKey)()
                End Get
            End Property

            Public Function Remove1(ByVal key As TKey) As Boolean Implements IDictionary(Of TKey, TValue).Remove
                FireReadOnly()
            End Function

            Public Function TryGetValue(ByVal key As TKey, ByRef value As TValue) As Boolean Implements IDictionary(Of TKey, TValue).TryGetValue
                Return False
            End Function

            Public ReadOnly Property Values() As ICollection(Of TValue) Implements IDictionary(Of TKey, TValue).Values
                Get
                    Return GetEmpty.ICollection(Of TValue)()
                End Get
            End Property
        End Class

        ''' <summary>
        ''' Class implementing an empty (and read-only) collection.
        ''' </summary>
        ''' <typeparam name="T">The type of empty collection</typeparam>
        <Serializable, DebuggerDisplay("Count: {Count}")>
        Private Class EmptyCollection(Of T) : Implements ICollection(Of T), ICollection

            Private ReadOnly mLock As New Object()

            Protected Sub FireReadOnly()
                Throw New NotSupportedException(My.Resources.EmptyCollection_ThisCollectionCannotBeModified)
            End Sub

            Public Sub Add(ByVal item As T) Implements ICollection(Of T).Add
                FireReadOnly()
            End Sub

            Public Sub Clear() Implements ICollection(Of T).Clear
                FireReadOnly()
            End Sub

            Public Function Contains(ByVal item As T) As Boolean Implements ICollection(Of T).Contains
                Return False
            End Function

            Public Sub CopyTo(ByVal array() As T, ByVal arrayIndex As Integer) Implements ICollection(Of T).CopyTo
            End Sub

            Public ReadOnly Property Count() As Integer Implements ICollection(Of T).Count, ICollection.Count
                Get
                    Return 0
                End Get
            End Property

            Public ReadOnly Property IsReadOnly() As Boolean Implements ICollection(Of T).IsReadOnly
                Get
                    Return True
                End Get
            End Property

            Public Function Remove(ByVal item As T) As Boolean Implements ICollection(Of T).Remove
                FireReadOnly()
            End Function

            Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
                Return New EmptyEnumerator(Of T)
            End Function

            Private Function GetNonGenericEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
                Return GetEnumerator()
            End Function

            Public ReadOnly Property IsSynchronized() As Boolean Implements System.Collections.ICollection.IsSynchronized
                Get
                    Return False
                End Get
            End Property

            Public ReadOnly Property SyncRoot() As Object Implements System.Collections.ICollection.SyncRoot
                Get
                    Return mLock
                End Get
            End Property

            Private Sub NonGenericCopyTo(ByVal array As System.Array, ByVal index As Integer) Implements System.Collections.ICollection.CopyTo
                CopyTo(DirectCast(array, T()), index)
            End Sub

        End Class

        ''' <summary>
        ''' Class implementing an empty (readonly) List.
        ''' </summary>
        ''' <typeparam name="T">The type of element, er, not held in the list.
        ''' </typeparam>
        <Serializable, DebuggerDisplay("Count: {Count}")>
        Private Class EmptyList(Of T) : Inherits EmptyCollection(Of T) : Implements IList(Of T)

            Public Function IndexOf(ByVal item As T) As Integer Implements IList(Of T).IndexOf
                Return -1
            End Function

            Public Sub Insert(ByVal index As Integer, ByVal item As T) Implements IList(Of T).Insert
                FireReadOnly()
            End Sub

            Default Public Property Item(ByVal index As Integer) As T Implements IList(Of T).Item
                Get
                    Throw New ArgumentOutOfRangeException(
                     My.Resources.EmptyList_ListIsEmptyItHasNoItemsToRetrieve)
                End Get
                Set(ByVal value As T)
                    FireReadOnly()
                End Set
            End Property

            Public Sub RemoveAt(ByVal index As Integer) Implements IList(Of T).RemoveAt
                FireReadOnly()
            End Sub
        End Class

        ''' <summary>
        ''' Class implementing an empty (read-only) set
        ''' </summary>
        ''' <typeparam name="T">The type of empty set</typeparam>
        <Serializable, DebuggerDisplay("Count: {Count}")>
        Private Class EmptySet(Of T) : Inherits EmptyCollection(Of T) : Implements IBPSet(Of T)

            Private Function AddSet(ByVal el As T) As Boolean Implements IBPSet(Of T).Add
                FireReadOnly()
            End Function

            Public Sub Difference(ByVal items As IEnumerable(Of T)) Implements IBPSet(Of T).Difference
                FireReadOnly()
            End Sub

            Public Sub Intersect(ByVal items As IEnumerable(Of T)) Implements IBPSet(Of T).Intersect
                FireReadOnly()
            End Sub

            Public Sub Subtract(ByVal items As IEnumerable(Of T)) Implements IBPSet(Of T).Subtract
                FireReadOnly()
            End Sub

            Public Sub Union(ByVal items As IEnumerable(Of T)) Implements IBPSet(Of T).Union
                FireReadOnly()
            End Sub
        End Class

        ''' <summary>
        ''' An empty enumerator class.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        <Serializable>
        Private Class EmptyEnumerator(Of T) : Implements IEnumerator(Of T)

            ''' <summary>
            ''' Throws an exception when called
            ''' </summary>
            Public ReadOnly Property Current() As T Implements IEnumerator(Of T).Current
                Get
                    Throw New InvalidOperationException(My.Resources.EmptyEnumerator_ThereAreNoElementsInThisEnumerator)
                End Get
            End Property

            ''' <summary>
            ''' Throws an exception when called
            ''' </summary>
            Private ReadOnly Property Current1() As Object Implements IEnumerator.Current
                Get
                    Return Current
                End Get
            End Property

            ''' <summary>
            ''' Does nothing. Returns False
            ''' </summary>
            ''' <returns>False</returns>
            Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
                Return False
            End Function

            ''' <summary>
            ''' No effect
            ''' </summary>
            Public Sub Reset() Implements IEnumerator.Reset
            End Sub

            ''' <summary>
            ''' No effect
            ''' </summary>
            Public Sub Dispose() Implements IDisposable.Dispose
            End Sub

        End Class

#End Region

    End Class

End Namespace
