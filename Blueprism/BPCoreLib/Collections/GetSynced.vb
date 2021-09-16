Namespace Collections

    ''' <summary>
    ''' Static class used to get thread-safe collections, ensuring that if multiple
    ''' threads are accessing the same data, any single-stage operations performed
    ''' on that data are atomic and synchronized.
    ''' 
    ''' Any collections returned by the static methods here are subclasses of the
    ''' <see cref="SynchronizedDelegator"/> class, which means that multi-stage
    ''' operations can be made atomic and synchronized too, using the same lock as
    ''' the single calls to the collections methods.
    ''' eg. The following code ensures that no other operations are performed on the
    ''' synchronized dictionary in between the ContainsKey() and Add() calls.
    ''' 
    ''' <code>
    ''' IDictionary&lt;int, String&gt; dict = GetSynced.IDictionary(srcDict);
    ''' SyncedDelegator delegator = dict as SynchronizedDelegator;
    ''' delegator.PerformThreadSafeOperation(delegate(){
    '''     if (!dict.ContainsKey(key))
    '''         dict.Add(key, value);
    ''' });
    ''' </code>
    ''' 
    ''' The code in VB is a bit more cumbersome (who would have guessed) because
    ''' there is no anonymous method syntax in VB... not our version anyway.
    ''' However, the same effect can be achieved by passing the <c>AddressOf</c> a
    ''' delegate to the <see cref="SynchronizedDelegator.PerformThreadSafeOperation">
    ''' PerformThreadSafeOperation</see> method.
    ''' </summary>
    Public Class GetSynced


#Region " Static Get Methods "

        ''' <summary>
        ''' Gets a synchronized ICollection over the given collection. This ensures
        ''' that all calls made to the collection or objects returned from the
        ''' collection (defined in the ICollection interface) are synchronized on a
        ''' single lock, ensuring thread safety.
        ''' </summary>
        ''' <typeparam name="T">The type of element in the collection</typeparam>
        ''' <param name="coll">The collection to wrap</param>
        ''' <returns>An ICollection which synchronizes access to all of the interface
        ''' methods used to access the underlying data.</returns>
        ''' <remarks>The returned object is an instance of
        ''' <see cref="SynchronizedDelegator"/> which provides a way to invoke blocks
        ''' of code within the same locking scope as the collection.</remarks>
        Public Shared Function ICollection(Of T)(ByVal coll As ICollection(Of T)) _
         As ICollection(Of T)
            If TypeOf coll Is SyncCollection(Of T) Then Return coll
            Return New SyncCollection(Of T)(coll)
        End Function

        ''' <summary>
        ''' Gets a synchronized IList over the given list. This ensures that all
        ''' calls made to the list or objects returned from the list (defined in the
        ''' IList interface) are synchronized on a single lock, ensuring thread
        ''' safety.
        ''' </summary>
        ''' <typeparam name="T">The type of element in the list</typeparam>
        ''' <param name="lst">The list to wrap</param>
        ''' <returns>An IList which synchronizes access to all of the interface
        ''' methods used to access the underlying data.</returns>
        ''' <remarks>The returned object is an instance of
        ''' <see cref="SynchronizedDelegator"/> which provides a way to invoke blocks
        ''' of code within the same locking scope as the collection.</remarks>
        Public Shared Function IList(Of T)(ByVal lst As IList(Of T)) As IList(Of T)
            If TypeOf lst Is SyncList(Of T) Then Return lst
            Return New SyncList(Of T)(lst)
        End Function

        ''' <summary>
        ''' Gets a synchronized ISet over the given collection. This ensures that all
        ''' calls made to the set or objects returned from the set (defined in the
        ''' ISet interface) are synchronized on a single lock, ensuring thread safety
        ''' </summary>
        ''' <typeparam name="T">The type of element in the set</typeparam>
        ''' <param name="s">The set to wrap</param>
        ''' <returns>An ISet which synchronizes access to all of the interface
        ''' methods used to access the underlying data.</returns>
        ''' <remarks>The returned object is an instance of
        ''' <see cref="SynchronizedDelegator"/> which provides a way to invoke blocks
        ''' of code within the same locking scope as the collection.</remarks>
        Public Shared Function ISet(Of T)(ByVal s As IBPSet(Of T)) As IBPSet(Of T)
            If TypeOf s Is SyncSet(Of T) Then Return s
            Return New SyncSet(Of T)(s)
        End Function

        ''' <summary>
        ''' Gets a synchronized IDictionary over the given dictionary. This ensures
        ''' that all calls made to the dictionary or objects returned from the
        ''' dictionary (defined in the IDictionary interface, and including any
        ''' collections returned by the dictionary - eg. the Keys property) are
        ''' synchronized on a single lock, ensuring thread safety.
        ''' </summary>
        ''' <typeparam name="TKey">The type of key used in the dictionary</typeparam>
        ''' <typeparam name="TValue">The type of value in the dictionary</typeparam>
        ''' <param name="dict">The dictionary to wrap</param>
        ''' <returns>An IDictionary which synchronizes access to all of the interface
        ''' methods used to access the underlying data.</returns>
        ''' <remarks>The returned object is an instance of
        ''' <see cref="SynchronizedDelegator"/> which provides a way to invoke blocks
        ''' of code within the same locking scope as the collection.</remarks>
        Public Shared Function IDictionary(Of TKey, TValue)(
         ByVal dict As IDictionary(Of TKey, TValue)) As IDictionary(Of TKey, TValue)
            If TypeOf dict Is SyncDictionary(Of TKey, TValue) Then Return dict
            Return New SyncDictionary(Of TKey, TValue)(dict)
        End Function

#End Region

#Region " SyncEnumerator "

        ''' <summary>
        ''' Enumerator which performs all of its operations within the scope of a
        ''' lock handle on a specified object.
        ''' </summary>
        Private Class SyncEnumerator(Of T) : Implements IEnumerator(Of T)
            Private ReadOnly mLock As Object

            Private mEnum As IEnumerator(Of T)

            Public Sub New(ByVal enu As IEnumerator(Of T), ByVal lock As Object)
                mLock = lock
                mEnum = enu
            End Sub

            Public ReadOnly Property Current() As T Implements IEnumerator(Of T).Current
                Get
                    SyncLock mLock
                        Return mEnum.Current
                    End SyncLock
                End Get
            End Property

            Private ReadOnly Property NonGenericCurrent() As Object Implements IEnumerator.Current
                Get
                    SyncLock mLock
                        Return mEnum.Current
                    End SyncLock
                End Get
            End Property

            Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
                SyncLock mLock
                    Return mEnum.MoveNext()
                End SyncLock
            End Function

            Public Sub Reset() Implements IEnumerator.Reset
                SyncLock mLock
                    mEnum.Reset()
                End SyncLock
            End Sub

            Public Sub Dispose() Implements IDisposable.Dispose
                mEnum.Dispose()
            End Sub

        End Class

#End Region

#Region " SyncEnumerable "

        <Serializable>
        Private Class SyncEnumerable(Of T)
            Inherits SynchronizedDelegator : Implements IEnumerable(Of T)

            Private mEnumerable As IEnumerable(Of T)

            Public Sub New(ByVal enu As IEnumerable(Of T))
                Me.New(enu, New Object())
            End Sub

            Protected Sub New(ByVal enu As IEnumerable(Of T), ByVal lock As Object)
                MyBase.New(lock)
                If enu Is Nothing Then Throw New ArgumentNullException(
                 NameOf(enu), My.Resources.SyncEnumerable_CannotCreateASyncEnumerableFromNull)
                mEnumerable = enu
            End Sub

            Protected ReadOnly Property WrappedEnumerable() As IEnumerable(Of T)
                Get
                    Return mEnumerable
                End Get
            End Property

            Public Function GetEnumerator() As IEnumerator(Of T) _
             Implements IEnumerable(Of T).GetEnumerator
                SyncLock mLock
                    Return _
                     New SyncEnumerator(Of T)(mEnumerable.GetEnumerator(), mLock)
                End SyncLock
            End Function

            Private Function GetNonGenericEnumerator() As IEnumerator _
             Implements IEnumerable.GetEnumerator
                Return GetEnumerator()
            End Function

        End Class

#End Region

#Region " SyncCollection "

        <Serializable, DebuggerDisplay("Count: {Count}")>
        Private Class SyncCollection(Of T) : Inherits SyncEnumerable(Of T) : Implements ICollection(Of T)

            Public Sub New(ByVal coll As ICollection(Of T))
                MyBase.New(coll)
            End Sub

            Protected Sub New(ByVal coll As ICollection(Of T), ByVal lock As Object)
                MyBase.New(coll, lock)
            End Sub

            Protected ReadOnly Property WrappedCollection() As ICollection(Of T)
                Get
                    Return DirectCast(WrappedEnumerable, ICollection(Of T))
                End Get
            End Property

            Public Sub Add(ByVal item As T) Implements ICollection(Of T).Add
                SyncLock mLock
                    WrappedCollection.Add(item)
                End SyncLock
            End Sub

            Public Sub Clear() Implements ICollection(Of T).Clear
                SyncLock mLock
                    WrappedCollection.Clear()
                End SyncLock
            End Sub

            Public Function Contains(ByVal item As T) As Boolean Implements ICollection(Of T).Contains
                SyncLock mLock
                    Return WrappedCollection.Contains(item)
                End SyncLock
            End Function

            Public Sub CopyTo(ByVal array() As T, ByVal arrayIndex As Integer) Implements ICollection(Of T).CopyTo
                SyncLock mLock
                    WrappedCollection.CopyTo(array, arrayIndex)
                End SyncLock
            End Sub

            Public ReadOnly Property Count() As Integer Implements ICollection(Of T).Count
                Get
                    SyncLock mLock
                        Return WrappedCollection.Count
                    End SyncLock
                End Get
            End Property

            Public ReadOnly Property IsReadOnly() As Boolean Implements ICollection(Of T).IsReadOnly
                Get
                    SyncLock mLock
                        Return WrappedCollection.IsReadOnly
                    End SyncLock
                End Get
            End Property

            Public Function Remove(ByVal item As T) As Boolean Implements ICollection(Of T).Remove
                SyncLock mLock
                    Return WrappedCollection.Remove(item)
                End SyncLock
            End Function

        End Class

#End Region

#Region " SyncList "

        <Serializable, DebuggerDisplay("Count: {Count}")>
        Private Class SyncList(Of T) : Inherits SyncCollection(Of T) : Implements IList(Of T)

            Public Sub New(ByVal lst As IList(Of T))
                MyBase.New(lst)
            End Sub

            Protected ReadOnly Property WrappedList() As IList(Of T)
                Get
                    Return DirectCast(WrappedEnumerable, IList(Of T))
                End Get
            End Property

            Public Function IndexOf(ByVal item As T) As Integer Implements IList(Of T).IndexOf
                SyncLock mLock
                    Return WrappedList.IndexOf(item)
                End SyncLock
            End Function

            Public Sub Insert(ByVal index As Integer, ByVal item As T) Implements IList(Of T).Insert
                SyncLock mLock
                    WrappedList.Insert(index, item)
                End SyncLock
            End Sub

            Default Public Property Item(ByVal index As Integer) As T Implements IList(Of T).Item
                Get
                    SyncLock mLock
                        Return WrappedList(index)
                    End SyncLock
                End Get
                Set(ByVal value As T)
                    SyncLock mLock
                        WrappedList(index) = value
                    End SyncLock
                End Set
            End Property

            Public Sub RemoveAt(ByVal index As Integer) Implements IList(Of T).RemoveAt
                SyncLock mLock
                    WrappedList.RemoveAt(index)
                End SyncLock
            End Sub
        End Class
#End Region

#Region " SyncSet "

        <Serializable, DebuggerDisplay("Count: {Count}")>
        Private Class SyncSet(Of T) : Inherits SyncCollection(Of T) : Implements IBPSet(Of T)

            Public Sub New(ByVal wrapSet As IBPSet(Of T))
                MyBase.New(wrapSet)
            End Sub

            Protected ReadOnly Property WrappedSet() As IBPSet(Of T)
                Get
                    Return DirectCast(WrappedEnumerable, IBPSet(Of T))
                End Get
            End Property

            Public Overloads Function Add(ByVal element As T) As Boolean Implements IBPSet(Of T).Add
                SyncLock mLock
                    Return WrappedSet.Add(element)
                End SyncLock
            End Function

            Public Sub Difference(ByVal items As IEnumerable(Of T)) Implements IBPSet(Of T).Difference
                SyncLock mLock
                    WrappedSet.Difference(items)
                End SyncLock
            End Sub

            Public Sub Intersect(ByVal items As IEnumerable(Of T)) Implements IBPSet(Of T).Intersect
                SyncLock mLock
                    WrappedSet.Intersect(items)
                End SyncLock
            End Sub

            Public Sub Subtract(ByVal items As IEnumerable(Of T)) Implements IBPSet(Of T).Subtract
                SyncLock mLock
                    WrappedSet.Subtract(items)
                End SyncLock
            End Sub

            Public Sub Union(ByVal items As IEnumerable(Of T)) Implements IBPSet(Of T).Union
                SyncLock mLock
                    WrappedSet.Union(items)
                End SyncLock
            End Sub
        End Class

#End Region

#Region " SyncDictionary "

        ''' <summary>
        ''' Dictionary which ensures that any operations performed on it are
        ''' thread-safe synchronized on a common lock, including those on sub-collections or
        ''' enumerators returned by its methods / properties.
        ''' </summary>
        <Serializable, DebuggerDisplay("Count: {Count}")>
        Private Class SyncDictionary(Of TKey, TValue)
            Inherits SyncCollection(Of KeyValuePair(Of TKey, TValue))
            Implements IDictionary(Of TKey, TValue)

            Public Sub New(ByVal dict As IDictionary(Of TKey, TValue))
                MyBase.New(dict)
            End Sub

            Protected ReadOnly Property WrappedDictionary() As IDictionary(Of TKey, TValue)
                Get
                    Return DirectCast(WrappedEnumerable, IDictionary(Of TKey, TValue))
                End Get
            End Property

            Public Overloads Sub Add(ByVal key As TKey, ByVal value As TValue) Implements IDictionary(Of TKey, TValue).Add
                SyncLock mLock
                    If Not WrappedDictionary.ContainsKey(key) Then
                        WrappedDictionary.Add(key, value)
                    End If
                End SyncLock
            End Sub

            Public Function ContainsKey(ByVal key As TKey) As Boolean Implements IDictionary(Of TKey, TValue).ContainsKey
                SyncLock mLock
                    Return WrappedDictionary.ContainsKey(key)
                End SyncLock
            End Function

            Default Public Property Item(ByVal key As TKey) As TValue Implements IDictionary(Of TKey, TValue).Item
                Get
                    SyncLock mLock
                        Return WrappedDictionary(key)
                    End SyncLock
                End Get
                Set(ByVal value As TValue)
                    SyncLock mLock
                        WrappedDictionary(key) = value
                    End SyncLock
                End Set
            End Property

            Public ReadOnly Property Keys() As ICollection(Of TKey) Implements IDictionary(Of TKey, TValue).Keys
                Get
                    SyncLock mLock
                        ' Ensure that we return a collection using the same lock as
                        ' this map so that any operations on the collection are
                        ' synced with this dictionary.
                        Return New _
                         SyncCollection(Of TKey)(WrappedDictionary.Keys, mLock)
                    End SyncLock
                End Get
            End Property

            Public Overloads Function Remove(ByVal key As TKey) As Boolean Implements IDictionary(Of TKey, TValue).Remove
                SyncLock mLock
                    Return WrappedDictionary.Remove(key)
                End SyncLock
            End Function

            Public Function TryGetValue(ByVal key As TKey, ByRef value As TValue) As Boolean Implements IDictionary(Of TKey, TValue).TryGetValue
                SyncLock mLock
                    Return WrappedDictionary.TryGetValue(key, value)
                End SyncLock
            End Function

            Public ReadOnly Property Values() As ICollection(Of TValue) Implements IDictionary(Of TKey, TValue).Values
                Get
                    SyncLock mLock
                        ' Ensure that we return a collection using the same lock as
                        ' this map so that any operations on the collection are
                        ' synced with this dictionary.
                        Return New _
                         SyncCollection(Of TValue)(WrappedDictionary.Values, mLock)
                    End SyncLock
                End Get
            End Property

        End Class

#End Region

    End Class

End Namespace
