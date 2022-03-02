Namespace Collections

    ''' Project: BPCoreLib
    ''' Class: GetReadOnly
    ''' <summary>
    ''' Class to get read only versions of collections in order to ensure that
    ''' a collection cannot be changed when it is returned.
    ''' These are just thin wrappers around the given collection which throw
    ''' an exception if any of their modification methods are called.
    ''' Note that none of these create copies of the underlying collection - they
    ''' just use them as backing collections.
    ''' </summary>
    Public Class GetReadOnly

#Region " Static Get Methods "

        ''' <summary>
        ''' Gets a read-only wrapper around the given collection.
        ''' </summary>
        ''' <typeparam name="T">The type of element held in the given collection.
        ''' </typeparam>
        ''' <param name="coll">The collection to wrap</param>
        ''' <returns>A collection instance which inhibits writing access to the
        ''' collection.</returns>
        Public Shared Function ICollection(Of T)(ByVal coll As ICollection(Of T)) _
         As ICollection(Of T)
            If TypeOf coll Is ReadOnlyCollection(Of T) Then Return coll
            Return New ReadOnlyCollection(Of T)(coll)
        End Function

        ''' <summary>
        ''' Gets a read-only wrapper around the given collection.
        ''' </summary>
        ''' <typeparam name="T">The type of element held in the given collection.
        ''' </typeparam>
        ''' <param name="coll">The collection to wrap</param>
        ''' <returns>A collection instance which inhibits writing access to the
        ''' collection.</returns>
        Public Shared Function IReadOnlyCollection(Of T)(ByVal coll As ICollection(Of T)) _
         As IReadOnlyCollection(Of T)
            If TypeOf coll Is ReadOnlyCollection(Of T) Then _
             Return DirectCast(coll, ReadOnlyCollection(Of T))
            Return New ReadOnlyCollection(Of T)(coll)
        End Function

        ''' <summary>
        ''' Gets a read-only collection containing the given values.
        ''' </summary>
        ''' <typeparam name="T">The type of element held in the given collection.
        ''' </typeparam>
        ''' <param name="values">The values to wrap into a readonly collection.
        ''' </param>
        ''' <returns>A collection instance which inhibits writing access to the
        ''' collection.</returns>
        Public Shared Function ICollectionFrom(Of T)(
         ByVal ParamArray values As T()) As ICollection(Of T)
            Return ICollection(values)
        End Function

        ''' <summary>
        ''' Gets a read-only collection containing the given values.
        ''' </summary>
        ''' <typeparam name="T">The type of element held in the given collection.
        ''' </typeparam>
        ''' <param name="values">The values to wrap into a readonly collection.
        ''' </param>
        ''' <returns>A collection instance which inhibits writing access to the
        ''' collection.</returns>
        Public Shared Function IReadOnlyCollectionFrom(Of T)(
         ByVal ParamArray values As T()) As IReadOnlyCollection(Of T)
            Return IReadOnlyCollection(values)
        End Function

        ''' <summary>
        ''' Gets a read-only wrapper around the given list.
        ''' </summary>
        ''' <typeparam name="T">The type of element held in the given list.
        ''' </typeparam>
        ''' <param name="lst">The list to wrap</param>
        ''' <returns>A list instance which inhibits writing access to the list.
        ''' </returns>
        Public Shared Function IList(Of T)(ByVal lst As IList(Of T)) As IList(Of T)
            If TypeOf lst Is ReadOnlyList(Of T) Then Return lst
            Return New ReadOnlyList(Of T)(lst)
        End Function

        ''' <summary>
        ''' Gets a read-only wrapper around the given list.
        ''' </summary>
        ''' <typeparam name="T">The type of element held in the given list.
        ''' </typeparam>
        ''' <param name="lst">The list to wrap</param>
        ''' <returns>A list instance which inhibits writing access to the list.
        ''' </returns>
        Public Shared Function IReadOnlyList(Of T)(ByVal lst As IList(Of T)) _
         As IReadOnlyList(Of T)
            If TypeOf lst Is ReadOnlyList(Of T) Then _
             Return DirectCast(lst, ReadOnlyList(Of T))
            Return New ReadOnlyList(Of T)(lst)
        End Function

        ''' <summary>
        ''' Gets a read-only list containing the given elements.
        ''' </summary>
        ''' <typeparam name="T">The type of element held in the list.</typeparam>
        ''' <param name="e">The enumerable containing the elements to add to the
        ''' list.</param>
        ''' <returns>A List containing the provided elements.</returns>
        Public Shared Function IList(Of T)(ByVal e As IEnumerable(Of T)) _
         As IList(Of T)
            If TypeOf e Is IList(Of T) Then Return IList(DirectCast(e, IList(Of T)))
            Dim lst As New List(Of T)
            For Each elem As T In e
                lst.Add(elem)
            Next
            Return IList(Of T)(lst)
        End Function

        ''' <summary>
        ''' Gets a read-only list containing the given elements.
        ''' </summary>
        ''' <typeparam name="T">The type of element held in the list.</typeparam>
        ''' <param name="els">The elements to add to the list.</param>
        ''' <returns>A List containing the provided elements.</returns>
        Public Shared Function IListFrom(Of T)(
         ByVal ParamArray els() As T) As IList(Of T)
            Return IList(els)
        End Function

        ''' <summary>
        ''' Gets a read-only list containing the given elements.
        ''' </summary>
        ''' <typeparam name="T">The type of element held in the list.</typeparam>
        ''' <param name="els">The elements to add to the list.</param>
        ''' <returns>A List containing the provided elements.</returns>
        Public Shared Function IReadOnlyListFrom(Of T)(
         ByVal ParamArray els() As T) As IReadOnlyList(Of T)
            Return IReadOnlyList(els)
        End Function

        ''' <summary>
        ''' Gets a read-only wrapper around the given set.
        ''' </summary>
        ''' <typeparam name="T">The type of element held in the given set.
        ''' </typeparam>
        ''' <param name="s">The set to wrap</param>
        ''' <returns>A set instance which inhibits writing access to the set.
        ''' </returns>
        Public Shared Function ISet(Of T)(ByVal s As IBPSet(Of T)) As IBPSet(Of T)
            If TypeOf s Is ReadOnlySet(Of T) Then Return s
            Return New ReadOnlySet(Of T)(s)
        End Function

        ''' <summary>
        ''' Gets a read-only set containing the given elements.
        ''' </summary>
        ''' <typeparam name="T">The type of element held in the set.</typeparam>
        ''' <param name="e">The elements to add to the set - this will follow
        ''' normal set rules as implemented by <see cref="clsSet(Of T)"/>, ie. it
        ''' will contain no duplicates and will not be in a guaranteed order.</param>
        ''' <returns>A Set containing the provided elements.</returns>
        Public Shared Function IBPSet(Of T)(ByVal e As IEnumerable(Of T)) As IBPSet(Of T)
            If TypeOf e Is IBPSet(Of T) Then Return ISet(DirectCast(e, IBPSet(Of T)))
            Dim s As New clsSet(Of T)
            For Each elem As T In e
                s.Add(elem)
            Next
            Return IBPSet(Of T)(s)
        End Function

        ''' <summary>
        ''' Gets a read-only set containing the given elements.
        ''' </summary>
        ''' <typeparam name="T">The type of element held in the set.</typeparam>
        ''' <param name="els">The elements to add to the set - this will follow
        ''' normal set rules as implemented by <see cref="clsSet(Of T)"/>, ie. it
        ''' will contain no duplicates and will not be in a guaranteed order.</param>
        ''' <returns>A Set containing the provided elements.</returns>
        Public Shared Function ISetFrom(Of T)( _
         ByVal ParamArray els() As T) As IBPSet(Of T)
            Return IBPSet(Of T)(els)
        End Function

        ''' <summary>
        ''' Gets a read-only wrapper around the given dictionary.
        ''' </summary>
        ''' <typeparam name="K">The type of element used as a key in the given
        ''' dictionary. </typeparam>
        ''' <typeparam name="V">The type of element used as a value in the given
        ''' dictionary.</typeparam>
        ''' <param name="d">The dictionary to wrap</param>
        ''' <returns>A dictionary instance which inhibits writing access to the 
        ''' dictionary.</returns>
        Public Shared Function IDictionary(Of K, V)(
         ByVal d As IDictionary(Of K, V)) As IDictionary(Of K, V)
            If TypeOf d Is ReadOnlyDictionary(Of K, V) Then Return d
            Return New ReadOnlyDictionary(Of K, V)(d)
        End Function

        ''' <summary>
        ''' Gets a read-only wrapper around the given dictionary.
        ''' </summary>
        ''' <typeparam name="K">The type of element used as a key in the given
        ''' dictionary. </typeparam>
        ''' <typeparam name="V">The type of element used as a value in the given
        ''' dictionary.</typeparam>
        ''' <param name="d">The dictionary to wrap</param>
        ''' <returns>A dictionary instance which inhibits writing access to the 
        ''' dictionary.</returns>
        Public Shared Function IReadOnlyDictionary(Of K, V)(
         ByVal d As IDictionary(Of K, V)) As IReadOnlyDictionary(Of K, V)
            If TypeOf d Is ReadOnlyDictionary(Of K, V) Then _
             Return DirectCast(d, ReadOnlyDictionary(Of K, V))
            Return New ReadOnlyDictionary(Of K, V)(d)
        End Function

#End Region

#Region " ReadOnlyCollection "

        ''' <summary>
        ''' Class to wrap a collection and provide read only access to its elements
        ''' </summary>
        ''' <typeparam name="T">The type of element in the collection.</typeparam>
        <Serializable, DebuggerDisplay("Count: {Count}")> _
        Private Class ReadOnlyCollection(Of T)
            Implements ICollection(Of T), ICollection, IReadOnlyCollection(Of T)

            Private ReadOnly mLock As New Object()

            Private mColl As ICollection(Of T)

            Public Sub New(ByVal coll As ICollection(Of T))
                mColl = coll
            End Sub

            Protected ReadOnly Property WrappedCollection() As ICollection(Of T)
                Get
                    Return mColl
                End Get
            End Property

            Protected Sub ThrowReadOnly()
                Throw New InvalidOperationException(My.Resources.ReadOnlyCollection_ThisCollectionIsReadOnlyAndThusItCannotBeModified)
            End Sub

            Public Sub Add(ByVal item As T) Implements ICollection(Of T).Add
                ThrowReadOnly()
            End Sub

            Public Sub Clear() Implements ICollection(Of T).Clear
                ThrowReadOnly()
            End Sub

            Public Function Contains(ByVal item As T) As Boolean Implements ICollection(Of T).Contains
                Return mColl.Contains(item)
            End Function

            Public Sub CopyTo(ByVal array() As T, ByVal arrayIndex As Integer) Implements ICollection(Of T).CopyTo
                mColl.CopyTo(array, arrayIndex)
            End Sub

            Public ReadOnly Property Count() As Integer Implements ICollection(Of T).Count, ICollection.Count, IReadOnlyCollection(Of T).Count
                Get
                    Return mColl.Count
                End Get
            End Property

            Public ReadOnly Property IsReadOnly() As Boolean Implements ICollection(Of T).IsReadOnly
                Get
                    Return True
                End Get
            End Property

            Public Function Remove(ByVal item As T) As Boolean Implements ICollection(Of T).Remove
                ThrowReadOnly()
            End Function

            Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
                Return mColl.GetEnumerator()
            End Function

            Private Function GetNonGenericEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
                Return GetEnumerator()
            End Function

            Public ReadOnly Property IsSynchronized() As Boolean Implements ICollection.IsSynchronized
                Get
                    Return False
                End Get
            End Property

            Public ReadOnly Property SyncRoot() As Object Implements ICollection.SyncRoot
                Get
                    Return mLock
                End Get
            End Property

            Private Sub NonGenericCopyTo(ByVal array As System.Array, ByVal index As Integer) Implements ICollection.CopyTo
                CopyTo(DirectCast(array, T()), index)
            End Sub

        End Class

#End Region

#Region " ReadOnlyList "

        ''' <summary>
        ''' Class to wrap a list and provide read only access to its elements
        ''' </summary>
        ''' <typeparam name="T">The type of element in the list.</typeparam>
        <Serializable, DebuggerDisplay("Count: {Count}")> _
        Private Class ReadOnlyList(Of T)
            Inherits ReadOnlyCollection(Of T)
            Implements IList(Of T), IList, IReadOnlyList(Of T)

            Public Sub New(ByVal lst As IList(Of T))
                MyBase.New(lst)
            End Sub

            Protected ReadOnly Property WrappedList() As IList(Of T)
                Get
                    Return DirectCast(WrappedCollection, IList(Of T))
                End Get
            End Property

            Public Function IndexOf(ByVal item As T) As Integer Implements IList(Of T).IndexOf
                Return WrappedList.IndexOf(item)
            End Function

            Public Sub Insert(ByVal index As Integer, ByVal item As T) Implements IList(Of T).Insert
                ThrowReadOnly()
            End Sub

            Default Public Property Item(ByVal index As Integer) As T Implements IList(Of T).Item
                Get
                    Return WrappedList.Item(index)
                End Get
                Set(ByVal value As T)
                    ThrowReadOnly()
                End Set
            End Property

            Public Property NonGenericItem(ByVal index As Integer) As Object Implements IList.Item
                Get
                    Return Item(index)
                End Get
                Set(ByVal value As Object)
                    Item(index) = DirectCast(value, T)
                End Set
            End Property

            Public Sub RemoveAt(ByVal index As Integer) Implements IList(Of T).RemoveAt, IList.RemoveAt
                ThrowReadOnly()
            End Sub

            Public Function NonGenericAdd(ByVal value As Object) As Integer Implements IList.Add
                Add(DirectCast(value, T))
            End Function

            Public Overloads Sub Clear() Implements IList.Clear
                MyBase.Clear()
            End Sub

            Public Overloads ReadOnly Property IsReadOnly() As Boolean Implements IList.IsReadOnly
                Get
                    Return MyBase.IsReadOnly
                End Get
            End Property

            Private Function NonGenericContains(ByVal value As Object) As Boolean Implements IList.Contains
                Return Contains(DirectCast(value, T))
            End Function

            Private Function NonGenericIndexOf(ByVal value As Object) As Integer Implements IList.IndexOf
                Return IndexOf(DirectCast(value, T))
            End Function

            Private Sub NonGenericInsert(ByVal index As Integer, ByVal value As Object) Implements IList.Insert
                Insert(index, DirectCast(value, T))
            End Sub

            Public ReadOnly Property IsFixedSize() As Boolean Implements IList.IsFixedSize
                Get
                    Return True
                End Get
            End Property

            Private ReadOnly Property IReadOnlyList_Item(index As Integer) As T _
             Implements IReadOnlyList(Of T).Item
                Get
                    Return Me(index)
                End Get
            End Property

            Public Sub NonGenericRemove(ByVal value As Object) Implements IList.Remove
                Remove(DirectCast(value, T))
            End Sub

        End Class

#End Region

#Region " ReadOnlySet "

        ''' <summary>
        ''' Class to wrap a set and provide read only access to its elements
        ''' </summary>
        ''' <typeparam name="T">The type of element in the set.</typeparam>
        <Serializable, DebuggerDisplay("Count: {Count}")> _
        Private Class ReadOnlySet(Of T)
            Inherits ReadOnlyCollection(Of T)
            Implements IBPSet(Of T), ISet(Of T)

            Public Sub New(ByVal s As IBPSet(Of T))
                MyBase.New(s)
            End Sub

            Public Overloads Function Add1(ByVal element As T) As Boolean Implements IBPSet(Of T).Add, ISet(Of T).Add
                ThrowReadOnly()
            End Function

            Public Sub Difference(ByVal items As IEnumerable(Of T)) Implements IBPSet(Of T).Difference
                ThrowReadOnly()
            End Sub

            Public Sub Intersect(ByVal items As IEnumerable(Of T)) Implements IBPSet(Of T).Intersect, ISet(Of T).IntersectWith
                ThrowReadOnly()
            End Sub

            Public Sub Subtract(ByVal items As IEnumerable(Of T)) Implements IBPSet(Of T).Subtract, ISet(Of T).ExceptWith
                ThrowReadOnly()
            End Sub

            Public Sub Union(ByVal items As IEnumerable(Of T)) Implements IBPSet(Of T).Union, ISet(Of T).UnionWith
                ThrowReadOnly()
            End Sub

            Public Sub SymmetricExceptWith(other As IEnumerable(Of T)) Implements ISet(Of T).SymmetricExceptWith
                ThrowReadOnly()
            End Sub

            Public Function IsSubsetOf(other As IEnumerable(Of T)) As Boolean Implements ISet(Of T).IsSubsetOf
                If other Is Nothing Then Throw New ArgumentNullException(NameOf(other))
                Return All(Function(item) other.Contains(item))
            End Function

            Public Function IsSupersetOf(other As IEnumerable(Of T)) As Boolean Implements ISet(Of T).IsSupersetOf
                If other Is Nothing Then Throw New ArgumentNullException(NameOf(other))
                Return other.All(Function(item) Contains(item))
            End Function

            Public Function IsProperSupersetOf(other As IEnumerable(Of T)) As Boolean Implements ISet(Of T).IsProperSupersetOf
                If other Is Nothing Then Throw New ArgumentNullException(NameOf(other))
                Return Count <> other.Count() AndAlso IsSupersetOf(other)
            End Function

            Public Function IsProperSubsetOf(other As IEnumerable(Of T)) As Boolean Implements ISet(Of T).IsProperSubsetOf
                If other Is Nothing Then Throw New ArgumentNullException(NameOf(other))
                Return Count <> other.Count() AndAlso IsSubsetOf(other)
            End Function

            Public Function Overlaps(other As IEnumerable(Of T)) As Boolean Implements ISet(Of T).Overlaps
                If other Is Nothing Then Throw New ArgumentNullException(NameOf(other))
                Dim otherSet As New clsSet(Of T)(other)
                otherSet.Intersect(Me)
                Return (otherSet.Count > 0)
            End Function

            Public Function SetEquals(other As IEnumerable(Of T)) As Boolean Implements ISet(Of T).SetEquals
                If other Is Nothing Then Throw New ArgumentNullException(NameOf(other))
                Dim otherSet As New clsSet(Of T)(other)
                If Count <> otherSet.Count Then Return False
                otherSet.Intersect(Me)
                Return (Count = otherSet.Count)
            End Function
        End Class

#End Region

#Region " ReadOnlyDictionary "

        ''' <summary>
        ''' Class to wrap a dictionary and provide read only access to its elements
        ''' </summary>
        ''' <typeparam name="K">The type of element used as a key in the dictionary.
        ''' </typeparam>
        ''' <typeparam name="V">The type of element used as a value in the dictionary
        ''' </typeparam>
        <Serializable, DebuggerDisplay("Count: {Count}")> _
        Private Class ReadOnlyDictionary(Of K, V)
            Inherits ReadOnlyCollection(Of KeyValuePair(Of K, V))
            Implements IDictionary(Of K, V), IReadOnlyDictionary(Of K, V)

            Public Sub New(ByVal dict As IDictionary(Of K, V))
                MyBase.New(dict)
            End Sub

            Protected ReadOnly Property WrappedDictionary() As IDictionary(Of K, V)
                Get
                    Return DirectCast(WrappedCollection, IDictionary(Of K, V))
                End Get
            End Property

            Private Sub DictAdd(ByVal key As K, ByVal value As V) Implements IDictionary(Of K, V).Add
                ThrowReadOnly()
            End Sub

            Public Function ContainsKey(ByVal key As K) As Boolean Implements IDictionary(Of K, V).ContainsKey, IReadOnlyDictionary(Of K, V).ContainsKey
                Return WrappedDictionary.ContainsKey(key)
            End Function

            Default Public Property Item(ByVal key As K) As V Implements IDictionary(Of K, V).Item
                Get
                    Return WrappedDictionary.Item(key)
                End Get
                Set(ByVal value As V)
                    ThrowReadOnly()
                End Set
            End Property

            Public ReadOnly Property Keys() As ICollection(Of K) Implements IDictionary(Of K, V).Keys
                Get
                    Return WrappedDictionary.Keys
                End Get
            End Property

            Public Function DictRemove(ByVal key As K) As Boolean Implements IDictionary(Of K, V).Remove
                ThrowReadOnly()
            End Function

            Public Function TryGetValue(ByVal key As K, ByRef value As V) As Boolean Implements IDictionary(Of K, V).TryGetValue, IReadOnlyDictionary(Of K, V).TryGetValue
                Return WrappedDictionary.TryGetValue(key, value)
            End Function

            Public ReadOnly Property Values() As ICollection(Of V) Implements IDictionary(Of K, V).Values
                Get
                    Return WrappedDictionary.Values
                End Get
            End Property

            Private ReadOnly Property IReadOnlyDictionary_Item(key As K) As V Implements IReadOnlyDictionary(Of K, V).Item
                Get
                    Return Me(key)
                End Get
            End Property

            Private ReadOnly Property IReadOnlyDictionary_Keys As IEnumerable(Of K) Implements IReadOnlyDictionary(Of K, V).Keys
                Get
                    Return Keys
                End Get
            End Property

            Private ReadOnly Property IReadOnlyDictionary_Values As IEnumerable(Of V) Implements IReadOnlyDictionary(Of K, V).Values
                Get
                    Return Values
                End Get
            End Property
        End Class

#End Region

    End Class

End Namespace
