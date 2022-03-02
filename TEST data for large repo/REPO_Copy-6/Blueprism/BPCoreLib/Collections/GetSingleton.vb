Namespace Collections

    ''' <summary>
    ''' Provides a mechanism to get read-only empty collections.
    ''' </summary>
    Public Class GetSingleton

#Region " Static Get Methods "

        ''' <summary>
        ''' Gets a singleton generic enumerator.
        ''' </summary>
        ''' <typeparam name="T">The type of element to enumerate over</typeparam>
        ''' <returns>A single element enumerator over one object of type T</returns>
        Public Shared Function IEnumerator(Of T)(ByVal singleElement As T) _
         As IEnumerator(Of T)
            Return New SingletonEnumerator(Of T)(singleElement)
        End Function

        ''' <summary>
        ''' Gets a singleton generic collection.
        ''' </summary>
        ''' <typeparam name="T">The type of element which is in the collection.
        ''' </typeparam>
        ''' <returns>A single element readonly collection of the given type.</returns>
        Public Shared Function IReadOnlyCollection(Of T)(ByVal singleElement As T) _
         As IReadOnlyCollection(Of T)
            Return New SingletonCollection(Of T)(singleElement)
        End Function

        ''' <summary>
        ''' Gets a singleton generic collection.
        ''' </summary>
        ''' <typeparam name="T">The type of element which is in the collection.
        ''' </typeparam>
        ''' <returns>A single element readonly collection of the given type.</returns>
        Public Shared Function ICollection(Of T)(ByVal singleElement As T) _
         As ICollection(Of T)
            Return New SingletonCollection(Of T)(singleElement)
        End Function

        ''' <summary>
        ''' Gets a singleton generic dictionary
        ''' </summary>
        ''' <typeparam name="TKey">The type of the dictionary key</typeparam>
        ''' <typeparam name="TValue">The type of the dictionary value</typeparam>
        ''' <param name="singleKey">The key to set in the singleton</param>
        ''' <param name="singleValue">The value to set in the singleton</param>
        ''' <returns>A dictionary containing a single key/value pair</returns>
        Public Shared Function IDictionary(Of TKey, TValue)(
         ByVal singleKey As TKey, ByVal singleValue As TValue) _
         As IDictionary(Of TKey, TValue)
            Return New SingletonDictionary(Of TKey, TValue)(singleKey, singleValue)
        End Function

        ''' <summary>
        ''' Gets a singleton generic dictionary
        ''' </summary>
        ''' <typeparam name="TKey">The type of the dictionary key</typeparam>
        ''' <typeparam name="TValue">The type of the dictionary value</typeparam>
        ''' <param name="singleKey">The key to set in the singleton</param>
        ''' <param name="singleValue">The value to set in the singleton</param>
        ''' <returns>A dictionary containing a single key/value pair</returns>
        Public Shared Function IReadOnlyDictionary(Of TKey, TValue)(
         ByVal singleKey As TKey, ByVal singleValue As TValue) _
         As IReadOnlyDictionary(Of TKey, TValue)
            Return New SingletonDictionary(Of TKey, TValue)(singleKey, singleValue)
        End Function

        ''' <summary>
        ''' Gets a singleton generic set.
        ''' </summary>
        ''' <typeparam name="T">The type of set required.</typeparam>
        ''' <param name="singleElement">The single element to hold in the set.
        ''' </param>
        ''' <returns>A set with the single element held in it.</returns>
        ''' <remarks>The returned set is read only - any attempts to modify it
        ''' will cause exceptions to be thrown.</remarks>
        Public Shared Function IBPSet(Of T)(ByVal singleElement As T) As IBPSet(Of T)
            Return New SingletonSet(Of T)(singleElement)
        End Function

        ''' <summary>
        ''' Gets a singleton generic list
        ''' </summary>
        ''' <typeparam name="T">The type of element held in the list</typeparam>
        ''' <param name="singleElement">The single element to hold in the list.
        ''' </param>
        ''' <returns>A list with a single element held in it</returns>
        ''' <remarks>The returned list is readonly - any attempt to modify it will
        ''' cause an exception to be thrown.</remarks>
        Public Shared Function IList(Of T)(ByVal singleElement As T) As IList(Of T)
            Return New SingletonList(Of T)(singleElement)
        End Function

        ''' <summary>
        ''' Gets a singleton generic list
        ''' </summary>
        ''' <typeparam name="T">The type of element held in the list</typeparam>
        ''' <param name="singleElement">The single element to hold in the list.
        ''' </param>
        ''' <returns>A list with a single element held in it</returns>
        ''' <remarks>The returned list is readonly - any attempt to modify it will
        ''' cause an exception to be thrown.</remarks>
        Public Shared Function IReadOnlyList(Of T)(ByVal singleElement As T) _
         As IReadOnlyList(Of T)
            Return New SingletonList(Of T)(singleElement)
        End Function

#End Region

#Region " SingletonSet "

        ''' <summary>
        ''' Class implementing a singleton (read-only) set.
        ''' </summary>
        ''' <typeparam name="T">The type of element held in the set.</typeparam>
        <Serializable> _
        Private Class SingletonSet(Of T) : Inherits SingletonCollection(Of T)
            Implements IBPSet(Of T)

            Public Sub New(ByVal val As T)
                MyBase.New(Val)
            End Sub

            Private Overloads Function AddSet(ByVal element As T) As Boolean _
             Implements IBPSet(Of T).Add
                FireReadOnly()
            End Function

            Public Sub Difference(ByVal items As IEnumerable(Of T)) _
             Implements IBPSet(Of T).Difference
                FireReadOnly()
            End Sub

            Public Sub Intersect(ByVal items As IEnumerable(Of T)) _
             Implements IBPSet(Of T).Intersect
                FireReadOnly()
            End Sub

            Public Sub Subtract(ByVal items As IEnumerable(Of T)) _
             Implements IBPSet(Of T).Subtract
                FireReadOnly()
            End Sub

            Public Sub Union(ByVal items As IEnumerable(Of T)) _
             Implements IBPSet(Of T).Union
                FireReadOnly()
            End Sub

        End Class

#End Region

#Region " SingletonList "

        ''' <summary>
        ''' Class implementing a singleton (read-only) list
        ''' </summary>
        ''' <typeparam name="T">The type of element within the list</typeparam>
        <Serializable>
        Private Class SingletonList(Of T) : Inherits SingletonCollection(Of T)
            Implements IList(Of T), IReadOnlyList(Of T)

            Public Sub New(ByVal elem As T)
                MyBase.New(elem)
            End Sub

            Public Function IndexOf(ByVal item As T) As Integer _
             Implements IList(Of T).IndexOf
                If Object.Equals(item, Element) Then Return 0 Else Return -1
            End Function

            Public Sub Insert(ByVal index As Integer, ByVal item As T) _
             Implements IList(Of T).Insert
                FireReadOnly()
            End Sub

            Default Public Property Item(ByVal index As Integer) As T _
             Implements IList(Of T).Item
                Get
                    If index = 0 Then Return Element
                    Throw New ArgumentOutOfRangeException(
                     String.Format(My.Resources.SingletonList_Index0IsNotValidForThisListOf1Element, index))
                End Get
                Set(ByVal value As T)
                    FireReadOnly()
                End Set
            End Property

            Private ReadOnly Property IReadOnlyList_Item(index As Integer) As T _
             Implements IReadOnlyList(Of T).Item
                Get
                    Return Me(index)
                End Get
            End Property

            Public Sub RemoveAt(ByVal index As Integer) _
             Implements IList(Of T).RemoveAt
                FireReadOnly()
            End Sub

        End Class

#End Region

#Region " SingletonDictionary "

        ''' <summary>
        ''' Creates a new singleton dictionary wrapping the single key/value pair
        ''' provided in the constructor.
        ''' The value can be changed using the default property after the singleton
        ''' has been created, but entries cannot be added or removed from the map.
        ''' </summary>
        ''' <typeparam name="K">The type of key in the singleton dictionary
        ''' </typeparam>
        ''' <typeparam name="V">The type of value in the singleton dictionary
        ''' </typeparam>
        <Serializable>
        Private Class SingletonDictionary(Of K, V)
            Inherits SingletonCollection(Of KeyValuePair(Of K, V))
            Implements IDictionary(Of K, V), IReadOnlyDictionary(Of K, V)

#Region " Constructor(s) "

            ''' <summary>
            ''' Creates a new singleton dictionary wrapping the given key and value.
            ''' </summary>
            ''' <param name="key">The key for the single dictionary element</param>
            ''' <param name="value">The value for the single dictionary element
            ''' </param>
            ''' <exception cref="ArgumentNullException">If the given key was null.
            ''' </exception>
            Public Sub New(ByVal key As K, ByVal value As V)
                MyBase.New(New KeyValuePair(Of K, V)(key, value))
                If key Is Nothing Then Throw New ArgumentNullException(NameOf(key))
            End Sub

#End Region

#Region " Properties "

            ''' <summary>
            ''' Gets the value of the element with the given key from this dictionary
            ''' </summary>
            ''' <param name="key">The key to get from this dictionary</param>
            ''' <exception cref="ArgumentNullException">If the given key was null.
            ''' </exception>
            ''' <exception cref="KeyNotFoundException">If this was a get and the
            ''' given key didn't match the key on this singleton dictionary.
            ''' </exception>
            ''' <exception cref="NotSupportedException">If trying to set a value in
            ''' this dictionary</exception>
            Default Public Property Item(ByVal key As K) As V _
             Implements IDictionary(Of K, V).Item
                Get
                    Dim v As V = Nothing
                    If TryGetValue(key, v) Then Return v
                    Throw New KeyNotFoundException(My.Resources.SingletonDictionary_KeyNotFound)
                End Get
                Set(ByVal value As V)
                    FireReadOnly()
                End Set
            End Property

            ''' <summary>
            ''' Gets the keys, er, key on this dictionary.
            ''' </summary>
            Public ReadOnly Property Keys() As ICollection(Of K) _
             Implements IDictionary(Of K, V).Keys
                Get
                    Return New SingletonCollection(Of K)(Element.Key)
                End Get
            End Property

            ''' <summary>
            ''' Gets the values in this dictionary.
            ''' </summary>
            Public ReadOnly Property Values() As ICollection(Of V) _
             Implements IDictionary(Of K, V).Values
                Get
                    Return New SingletonCollection(Of V)(Element.Value)
                End Get
            End Property

            Private ReadOnly Property IReadOnlyDictionary_Item(key As K) As V _
             Implements IReadOnlyDictionary(Of K, V).Item
                Get
                    Return Me(key)
                End Get
            End Property

            Private ReadOnly Property IReadOnlyDictionary_Keys As IEnumerable(Of K) _
             Implements IReadOnlyDictionary(Of K, V).Keys
                Get
                    Return Keys
                End Get
            End Property

            Private ReadOnly Property IReadOnlyDictionary_Values As IEnumerable(Of V) _
             Implements IReadOnlyDictionary(Of K, V).Values
                Get
                    Return Values
                End Get
            End Property

#End Region

#Region " Methods "

            ''' <summary>
            ''' Tries to get the value corresponding to the given key.
            ''' </summary>
            ''' <param name="key">The key to look up.</param>
            ''' <param name="value">The placeholder for the value corresponding to the
            ''' key. Unchanged if the given key did not match the key in this map</param>
            ''' <returns>True if the specified key matched the key on this dictionary,
            ''' False otherwise.</returns>
            Public Function TryGetValue(ByVal key As K, ByRef value As V) As Boolean _
             Implements IDictionary(Of K, V).TryGetValue,
              IReadOnlyDictionary(Of K, V).TryGetValue
                If key Is Nothing Then Throw New ArgumentNullException(NameOf(key))
                If Not key.Equals(Element.Key) Then Return False
                value = Element.Value
                Return True
            End Function

            ''' <summary>
            ''' Checks if this dictionary contains a specified key
            ''' </summary>
            ''' <param name="key">The key to test</param>
            ''' <returns>True if the given key equals the key held in this
            ''' dictionary; False otherwise.</returns>
            Public Function ContainsKey(ByVal key As K) As Boolean Implements _
             IDictionary(Of K, V).ContainsKey, IReadOnlyDictionary(Of K, V).ContainsKey
                Return Object.Equals(key, Element.Key)
            End Function

            ''' <summary>
            ''' Not supported
            ''' </summary>
            ''' <param name="key"></param>
            ''' <param name="value"></param>
            ''' <exception cref="NotSupportedException">When called</exception>
            Private Sub DictAdd(ByVal key As K, ByVal value As V) _
             Implements IDictionary(Of K, V).Add
                FireReadOnly()
            End Sub

            ''' <summary>
            ''' Not supported
            ''' </summary>
            ''' <param name="key"></param>
            ''' <exception cref="NotSupportedException">When called</exception>
            Private Function DictRemove(ByVal key As K) As Boolean _
             Implements IDictionary(Of K, V).Remove
                FireReadOnly()
            End Function

#End Region

        End Class

#End Region

#Region " SingletonCollection "

        ''' <summary>
        ''' Class implementing a singleton (and read-only) collection.
        ''' </summary>
        ''' <typeparam name="T">The type of singleton collection</typeparam>
        <Serializable>
        Private Class SingletonCollection(Of T)
            Implements ICollection(Of T), IReadOnlyCollection(Of T)

            Private mElement As T

            Public Sub New(ByVal el As T)
                mElement = el
            End Sub

            Protected Sub FireReadOnly()
                Throw New NotSupportedException(My.Resources.SingletonCollection_ThisCollectionCannotBeModified)
            End Sub

            Protected ReadOnly Property Element() As T
                Get
                    Return mElement
                End Get
            End Property

            Public Sub Add(ByVal item As T) Implements ICollection(Of T).Add
                FireReadOnly()
            End Sub

            Public Sub Clear() Implements ICollection(Of T).Clear
                FireReadOnly()
            End Sub

            Public Function Contains(ByVal item As T) As Boolean Implements ICollection(Of T).Contains
                Return Object.Equals(mElement, item)
            End Function

            Public Sub CopyTo(ByVal array() As T, ByVal arrayIndex As Integer) Implements ICollection(Of T).CopyTo
                array(arrayIndex) = mElement
            End Sub

            Public ReadOnly Property Count() As Integer Implements ICollection(Of T).Count, IReadOnlyCollection(Of T).Count
                Get
                    Return 1
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
                Return New SingletonEnumerator(Of T)(mElement)
            End Function

            Private Function GetNonGenericEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
                Return GetEnumerator()
            End Function
        End Class

#End Region

        ''' <summary>
        ''' An enumerator which enumerates over a single instance of a class
        ''' </summary>
        ''' <typeparam name="T">The type that this enumerator enumerates over.
        ''' </typeparam>
        <Serializable>
        Private Class SingletonEnumerator(Of T)
            Implements IEnumerator(Of T)

            ''' <summary>
            ''' Enum mapping the state of the enumerator.
            ''' </summary>
            Private Enum State

                ''' <summary>
                ''' The state before the enumerator has been moved.
                ''' </summary>
                Before

                ''' <summary>
                ''' The state when this enumerator has been moved once,
                ''' ie. it is currently pointing to the single element it is
                ''' enumerating over.
                ''' </summary>
                OnElement

                ''' <summary>
                ''' The state after the enumerator has been (successfully) moved
                ''' twice, ie. it is beyond the end of the enumerator.
                ''' </summary>
                After

                ''' <summary>
                ''' The state of the enumerator once it has been disposed.
                ''' </summary>
                Disposed

            End Enum

            ''' <summary>
            ''' The current state of this enumerator.
            ''' </summary>
            Private mState As State

            ''' <summary>
            ''' The instance that this enumerator is enumerating over.
            ''' </summary>
            Private mInst As T

            ''' <summary>
            ''' Creates a new singleton enumerator over the given element.
            ''' </summary>
            ''' <param name="instance">The single element to enumerate over</param>
            Public Sub New(ByVal instance As T)
                mState = State.Before
                mInst = instance
            End Sub

            ''' <summary>
            ''' Checks that this enumerator has not been disposed. Throws an
            ''' exception if it has.
            ''' </summary>
            ''' <exception cref="InvalidOperationException">If this enumerator has
            ''' been disposed.</exception>
            Private Sub CheckNotDisposed()
                If mState = State.Disposed Then Throw New InvalidOperationException(
                 My.Resources.SingletonEnumerator_ThisEnumeratorHasBeenDisposed)
            End Sub

            ''' <summary>
            ''' Gets the current element in this enumerator. This will fail if the
            ''' enumerator is currently set before the element, or after it.
            ''' </summary>
            ''' <exception cref="InvalidOperationException">If the enumerator is 
            ''' currently pointing before the first element or after the last element
            ''' of its contained elements.</exception>
            Public ReadOnly Property Current() As T _
             Implements IEnumerator(Of T).Current
                Get
                    CheckNotDisposed()
                    If mState <> State.OnElement Then
                        Throw New InvalidOperationException(String.Format(
                         CStr(IIf(mState = State.Before,
                          My.Resources.SingletonEnumerator_EnumeratorIsPositionedBeforeTheFirstElement,
                          My.Resources.SingletonEnumerator_EnumeratorIsPositionedAfterTheLastElement))
                        ))
                    End If
                    Return mInst
                End Get
            End Property

            ''' <summary>
            ''' Non-generic implementation of Current()
            ''' </summary>
            ''' <exception cref="InvalidOperationException">If the enumerator is 
            ''' currently pointing before the first element or after the last element
            ''' of its contained elements.</exception>
            Private ReadOnly Property Current1() As Object _
             Implements IEnumerator.Current
                Get
                    Return Current()
                End Get
            End Property

            ''' <summary>
            ''' Attempts to move to the next element in the enumerator, returning a
            ''' flag indicating whether it is currently pointing to an element or not
            ''' </summary>
            ''' <returns>True if the enumerator is pointing to its contained element,
            ''' False if it has gone beyond the element (ie. beyond the last element)
            ''' </returns>
            Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
                CheckNotDisposed()
                If mState = State.Before Then
                    mState = State.OnElement
                    Return True
                End If
                mState = State.After
                Return False
            End Function

            ''' <summary>
            ''' Resets the enumerator so that it is pointing to before the first
            ''' element again.
            ''' </summary>
            Public Sub Reset() Implements IEnumerator.Reset
                CheckNotDisposed()
                mState = State.Before
            End Sub

            ''' <summary>
            ''' Disposes of this enumerator - after a call to this, the other
            ''' operations in the enumerator will fail when called.
            ''' </summary>
            Public Sub Dispose() Implements IDisposable.Dispose
                mInst = Nothing
                mState = State.Disposed
            End Sub

        End Class

    End Class
End Namespace
