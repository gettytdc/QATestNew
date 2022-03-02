Imports BluePrism.Server.Domain.Models

Namespace Collections

    ''' Project  : BPCoreLib
    ''' Class    : clsCounterMap
    ''' <summary>
    ''' A map of the given keys to a count.
    ''' </summary>
    ''' <typeparam name="TKey">The key to which a counter should be attached
    ''' </typeparam>
    <Serializable, DebuggerDisplay("Count: {Count}")>
    Public Class clsCounterMap(Of TKey)
        Implements IDictionary(Of TKey, Integer)

#Region " Inner Comparer Classes "

        ''' <summary>
        ''' Basic comparer for 2 comparables
        ''' </summary>
        Private Class BasicComparer(Of T)
            Implements IComparer(Of T)

            ''' <summary>
            ''' Compares x and y - largely delegates to their Comparable
            ''' implementation, just ensuring that nulls are treated as 'less than'
            ''' non-nulls.
            ''' </summary>
            ''' <param name="x">The first object to test</param>
            ''' <param name="y">The second object to test</param>
            ''' <returns>A positive integer, 0 or a negative integer depending on
            ''' whether x is greater than, equal to, or less than y, respectively.
            ''' </returns>
            Public Function Compare(ByVal x As T, ByVal y As T) As Integer _
             Implements IComparer(Of T).Compare
                If x Is Nothing AndAlso y Is Nothing Then Return 0
                If x Is Nothing Then Return -1
                If y Is Nothing Then Return 1
                Return DirectCast(x, IComparable(Of T)).CompareTo(y)
            End Function

        End Class

        ''' <summary>
        ''' Basic comparer for 2 non-generic comparables
        ''' </summary>
        Private Class BasicNonGenericComparer(Of T)
            Implements IComparer(Of T)

            ''' <summary>
            ''' Compares x and y - largely delegates to their Comparable
            ''' implementation, just ensuring that nulls are treated as 'less than'
            ''' non-nulls.
            ''' </summary>
            ''' <param name="x">The first object to test</param>
            ''' <param name="y">The second object to test</param>
            ''' <returns>A positive integer, 0 or a negative integer depending on
            ''' whether x is greater than, equal to, or less than y, respectively.
            ''' </returns>
            Public Function Compare(ByVal x As T, ByVal y As T) _
             As Integer Implements IComparer(Of T).Compare
                If x Is Nothing AndAlso y Is Nothing Then Return 0
                If x Is Nothing Then Return -1
                If y Is Nothing Then Return 1
                Return DirectCast(x, IComparable).CompareTo(y)
            End Function
        End Class

        ''' <summary>
        ''' Compares the counts of the given key/value pairs in reverse order, such
        ''' that the largest counts will come out first.
        ''' </summary>
        Private Class CountComparer
            Implements IComparer(Of KeyValuePair(Of TKey, Integer))

            Public Function Compare(
             ByVal x As KeyValuePair(Of TKey, Integer),
             ByVal y As KeyValuePair(Of TKey, Integer)) As Integer _
             Implements IComparer(Of KeyValuePair(Of TKey, Integer)).Compare
                Return -x.Value.CompareTo(y.Value)
            End Function
        End Class

#End Region

#Region " Static Methods "

        ''' <summary>
        ''' Gets the basic comparer for the type used by this dictionary.
        ''' </summary>
        ''' <param name="sorted">True to actually attempt to get a default comparer
        ''' for the type represented by this counter map; False to just return null.
        ''' This is a workaround for the lack of a proper ternary operator in VB.
        ''' </param>
        ''' <returns>An IComparer which can be used to compare objects of the type
        ''' being counted in this map</returns>
        ''' <exception cref="BluePrismException">If the type represented in this
        ''' class is not comparable and thus cannot be compared easily</exception>
        Private Shared Function GetComparer(ByVal sorted As Boolean) _
         As IComparer(Of TKey)

            ' If we're not sorting, we don't want an actual comparer
            If Not sorted Then Return Nothing

            ' If the type implements IComparable<TKey>...
            If GetType(TKey).GetInterface(
             GetType(IComparable(Of TKey)).FullName) IsNot Nothing Then _
             Return New BasicComparer(Of TKey)

            ' Else if the type implements IComparable...
            If GetType(TKey).GetInterface(
             GetType(IComparable).FullName) IsNot Nothing Then _
             Return New BasicNonGenericComparer(Of TKey)

            ' Else, it's an error - there is no comparer which can handle it
            Throw New InvalidTypeException(
             My.Resources.clsCounterMap_TheType0IsNotComparableNoDefaultComparerIsAvailable,
             GetType(TKey).FullName)

        End Function

#End Region

#Region " Instance Variables "

        ' The map containing the keys and counts
        Private mMap As IDictionary(Of TKey, Integer)

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new, unsorted counter map
        ''' </summary>
        Public Sub New()
            Me.New(DirectCast(Nothing, IComparer(Of TKey)))
        End Sub

        ''' <summary>
        ''' Creates a new counter map, which may or may not be sorted according to
        ''' the given flag.
        ''' </summary>
        ''' <param name="sorted">True to create a sorted dictionary; False to create
        ''' an unsorted one.</param>
        ''' <exception cref="InvalidTypeException">If <paramref name="sorted"/> was
        ''' true and the type of this counter map did not implement 
        ''' <see cref="IComparable(Of TKey)"/> or <see cref="T:IComparable"/>
        ''' </exception>
        Public Sub New(ByVal sorted As Boolean)
            Me.New(GetComparer(sorted))
        End Sub

        ''' <summary>
        ''' Creates a new counter map, using the given comparer to sort it. If no
        ''' comparer is given an unsorted map is created.
        ''' </summary>
        ''' <param name="comp">The comparer to use to sort the counter entries,
        ''' null to create an unsorted counter map</param>
        Public Sub New(ByVal comp As IComparer(Of TKey))
            If comp IsNot Nothing Then
                mMap = New SortedDictionary(Of TKey, Integer)(comp)
            Else
                mMap = New Dictionary(Of TKey, Integer)
            End If

        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' <para>
        ''' The counter associated with the given item. This is initialised when
        ''' it is first set. If accessed without being initialised the default
        ''' counter value of zero is returned.
        ''' </para>
        ''' <para>
        ''' Note that this property is 'overloaded' ('new' in C#) rather than 
        ''' overridden, meaning that the type 'clsCounterMap' must be explicitly
        ''' used to utilise this version of the property rather than either
        ''' IDictionary or Dictionary. It might be made fully IDictionary
        ''' compatible in the future, but that means that all IDictionary methods
        ''' must be reimplemented and it takes time.
        ''' </para>
        ''' </summary>
        Default Public Property Item(ByVal key As TKey) As Integer _
         Implements IDictionary(Of TKey, Integer).Item
            Get
                Dim i As Integer
                mMap.TryGetValue(key, i)
                Return i
            End Get
            Set(ByVal value As Integer)
                mMap.Item(key) = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the key within this counter map for the highest count currently
        ''' recorded within it.
        ''' </summary>
        Public ReadOnly Property HighestKey() As TKey
            Get
                Dim contender As TKey = Nothing
                Dim max As Integer = 0
                For Each pair As KeyValuePair(Of TKey, Integer) In Me
                    If pair.Value > max Then max = pair.Value : contender = pair.Key
                Next
                Return contender
            End Get
        End Property

        ''' <summary>
        ''' Gets a map of the counters in this map in reverse counter order, ie
        ''' such that the entries with the highest counts are at the beginning of
        ''' the map.
        ''' </summary>
        Public ReadOnly Property CounterOrderedMap() As IDictionary(Of TKey, Integer)
            Get
                ' Get the pairs into a list and sort using the CountComparer up top
                Dim sl As New List(Of KeyValuePair(Of TKey, Integer))(mMap)
                sl.Sort(New CountComparer())

                ' Add entries from the now sorted list into a dictionary which
                ' orders its elements according to insertion order
                Dim od As New clsOrderedDictionary(Of TKey, Integer)
                For Each pair As KeyValuePair(Of TKey, Integer) In sl
                    od(pair.Key) = pair.Value
                Next
                Return od
            End Get
        End Property

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Increments the counter associated with the given key by 1
        ''' </summary>
        ''' <param name="key">The key to increment</param>
        ''' <returns>The new counter value associated with the key</returns>
        Public Function Increment(ByVal key As TKey) As Integer
            Dim i As Integer = 0
            mMap.TryGetValue(key, i)
            i += 1
            mMap(key) = i
            Return i
        End Function

        ''' <summary>
        ''' Clears the counter map
        ''' </summary>
        Public Sub Clear() _
         Implements ICollection(Of KeyValuePair(Of TKey, Integer)).Clear
            mMap.Clear()
        End Sub

        ''' <summary>
        ''' Copies this countermap to the given array
        ''' </summary>
        ''' <param name="array">The array to which to copy this map</param>
        ''' <param name="arrayIndex">The index at which to start copying</param>
        Public Sub CopyTo(ByVal array() As KeyValuePair(Of TKey, Integer),
         ByVal arrayIndex As Integer) _
         Implements ICollection(Of KeyValuePair(Of TKey, Integer)).CopyTo
            mMap.CopyTo(array, arrayIndex)
        End Sub

        ''' <summary>
        ''' Gets a count of the counts held in this map
        ''' </summary>
        Public ReadOnly Property Count() As Integer _
         Implements ICollection(Of KeyValuePair(Of TKey, Integer)).Count
            Get
                Return mMap.Count
            End Get
        End Property

        ''' <summary>
        ''' Flag indicating if this map is readonly. Always false
        ''' </summary>
        Public ReadOnly Property IsReadOnly() As Boolean _
         Implements ICollection(Of KeyValuePair(Of TKey, Integer)).IsReadOnly
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Adds the value with the given key to this map
        ''' </summary>
        ''' <param name="key">The key to add to the map</param>
        ''' <param name="value">The value to associate with the key</param>
        Public Sub Add(ByVal key As TKey, ByVal value As Integer) _
         Implements IDictionary(Of TKey, Integer).Add
            mMap.Add(key, value)
        End Sub

        ''' <summary>
        ''' Checks if this map contains the given key.
        ''' </summary>
        ''' <param name="key">The key to test</param>
        ''' <returns>True if this map contains an entry for the given key; False
        ''' otherwise.</returns>
        Public Function ContainsKey(ByVal key As TKey) As Boolean _
         Implements IDictionary(Of TKey, Integer).ContainsKey
            Return mMap.ContainsKey(key)
        End Function

        ''' <summary>
        ''' The collection of keys held in this map
        ''' </summary>
        Public ReadOnly Property Keys() As ICollection(Of TKey) _
         Implements IDictionary(Of TKey, Integer).Keys
            Get
                Return mMap.Keys
            End Get
        End Property

        ''' <summary>
        ''' Removes the entry with the given key from this map
        ''' </summary>
        ''' <param name="key">The key to remove</param>
        ''' <returns>True if the key was found and the corresponding entry was
        ''' removed; False otherwise</returns>
        Public Function Remove(ByVal key As TKey) As Boolean _
         Implements IDictionary(Of TKey, Integer).Remove
            Return mMap.Remove(key)
        End Function

        ''' <summary>
        ''' Tries to get the value associated with the given key from this map
        ''' </summary>
        ''' <param name="key">The key to attempt to get the value with</param>
        ''' <param name="value">The value found on the map corresponding to the
        ''' given key. Zero if the key is not present in the map</param>
        ''' <returns>True if the key was found in the map; False otherwise</returns>
        Public Function TryGetValue(ByVal key As TKey, ByRef value As Integer) _
         As Boolean Implements IDictionary(Of TKey, Integer).TryGetValue
            Return mMap.TryGetValue(key, value)
        End Function

        ''' <summary>
        ''' The values held in this map
        ''' </summary>
        Public ReadOnly Property Values() As ICollection(Of Integer) _
         Implements IDictionary(Of TKey, Integer).Values
            Get
                Return mMap.Values
            End Get
        End Property

        ''' <summary>
        ''' Gets an enumerator over this map's entries.
        ''' </summary>
        ''' <returns>An enumerator over this map</returns>
        Public Function GetEnumerator() _
         As IEnumerator(Of KeyValuePair(Of TKey, Integer)) _
         Implements IEnumerable(Of KeyValuePair(Of TKey, Integer)).GetEnumerator
            Return mMap.GetEnumerator()
        End Function

#End Region

#Region " Private Interface Implementations "

        ''' <summary>
        ''' Adds the given item to the counter map
        ''' </summary>
        ''' <param name="item">The item to add</param>
        Private Sub AddItem(ByVal item As KeyValuePair(Of TKey, Integer)) _
         Implements ICollection(Of KeyValuePair(Of TKey, Integer)).Add
            mMap.Add(item)
        End Sub

        ''' <summary>
        ''' Checks if the map contains the given key/value pair
        ''' </summary>
        ''' <param name="item">The item to check for</param>
        ''' <returns>True if the item exists in this map; false otherwise</returns>
        Private Function ContainsPair(ByVal item As KeyValuePair(Of TKey, Integer)) _
         As Boolean Implements ICollection(Of KeyValuePair(Of TKey, Integer)).Contains
            Return mMap.Contains(item)
        End Function

        ''' <summary>
        ''' Removes the given item from this map
        ''' </summary>
        ''' <param name="item">The item to remove</param>
        ''' <returns>True if the item was removed, false otherwise</returns>
        Private Function RemoveItem(ByVal item As KeyValuePair(Of TKey, Integer)) _
         As Boolean Implements ICollection(Of KeyValuePair(Of TKey, Integer)).Remove
            Return mMap.Remove(item)
        End Function

        ''' <summary>
        ''' Gets a non-generic enumerator over this map's entries.
        ''' </summary>
        ''' <returns>An enumerator over this map</returns>
        Private Function GetNonGenericEnumerator() As IEnumerator _
         Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function

#End Region

    End Class

End Namespace
