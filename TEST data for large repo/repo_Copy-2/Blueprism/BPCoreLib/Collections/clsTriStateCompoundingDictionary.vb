Namespace Collections

    ''' <summary>
    ''' Dictionary which accumulates tri state values
    ''' </summary>
    ''' <typeparam name="TKey">The type of key</typeparam>
    <Serializable, DebuggerDisplay("Count: {Count}")>
    Public Class clsTriStateCompoundingDictionary(Of TKey) : Implements IDictionary(Of TKey, TriState)

        ' The backing store used to hold the values
        Private mBackingStore As IDictionary(Of TKey, TriState)

        ''' <summary>
        ''' Creates a new, empty three state compounding dictionary.
        ''' </summary>
        Public Sub New()
            mBackingStore = New Dictionary(Of TKey, TriState)
        End Sub


        ''' <summary>
        ''' The count of elements within this dictionary.
        ''' </summary>
        Public ReadOnly Property Count() As Integer _
         Implements ICollection(Of KeyValuePair(Of TKey, TriState)).Count
            Get
                Return mBackingStore.Count
            End Get
        End Property

        ''' <summary>
        ''' Whether this dictionary is read only or not - it is not
        ''' </summary>
        Public ReadOnly Property IsReadOnly() As Boolean _
         Implements ICollection(Of KeyValuePair(Of TKey, TriState)).IsReadOnly
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Gets the tristate value for the given key from this dictionary.
        ''' </summary>
        ''' <param name="key">The key for which the tristate is required.</param>
        ''' <exception cref="ArgumentNullException">If the key is null</exception>
        ''' <exception cref="KeyNotFoundException">If the property is retrieved and the
        ''' key is not found.</exception>
        Default Public Property Item(ByVal key As TKey) As TriState _
         Implements IDictionary(Of TKey, TriState).Item
            Get
                Return mBackingStore.Item(key)
            End Get
            Set(ByVal value As TriState)
                Add(key, value)
            End Set
        End Property

        ''' <summary>
        ''' All the keys held in this dictionary.
        ''' </summary>
        Public ReadOnly Property Keys() As ICollection(Of TKey) _
         Implements IDictionary(Of TKey, TriState).Keys
            Get
                Return mBackingStore.Keys
            End Get
        End Property

        ''' <summary>
        ''' The values held in this dictionary.
        ''' </summary>
        Public ReadOnly Property Values() As ICollection(Of TriState) _
         Implements IDictionary(Of TKey, TriState).Values
            Get
                Return mBackingStore.Values
            End Get
        End Property

        ''' <summary>
        ''' Adds the given key/value pair to the dictionary or, more accurately, it
        ''' merges it into the dictionary - testing the value against the current value
        ''' and compounding the result into the dictionary.
        ''' </summary>
        ''' <param name="item">The item to be added</param>
        Public Sub Add(ByVal item As KeyValuePair(Of TKey, TriState)) _
         Implements ICollection(Of KeyValuePair(Of TKey, TriState)).Add
            Add(item.Key, item.Value)
        End Sub


        ''' <summary>
        ''' Compounds the given boolean value into the dictionary, testing it against the
        ''' current tristate value held against the key and storing the result into the
        ''' dictionary. A value of 'true' is interpreted as <see cref="TriState.Enabled"/>
        ''' and 'false' as <see cref="TriState.Disabled"/>
        ''' </summary>
        ''' <param name="key">The key to be added</param>
        ''' <param name="value">The value to be compounded into the dictionary.</param>
        Public Sub Add(ByVal key As TKey, ByVal value As Boolean)
            ' Rubbish vb non-ternary operator equivalent
            If value Then Add(key, TriState.Enabled) Else Add(key, TriState.Disabled)
        End Sub

        ''' <summary>
        ''' Adds the given key/value pair to the dictionary or, more accurately, it
        ''' merges it into the dictionary - testing the value against the current value
        ''' and compounding the result into the dictionary.
        ''' </summary>
        ''' <param name="key">The key to be added</param>
        ''' <param name="value">The value to be compounded into the dictionary.</param>
        Public Sub Add(ByVal key As TKey, ByVal value As TriState) _
         Implements IDictionary(Of TKey, TriState).Add
            Dim curr As TriState
            If mBackingStore.TryGetValue(key, curr) Then
                ' If the current value doesn't match the new value, set the new value
                ' to be indeterminate (unless it already is, in which case don't bother)
                If curr <> value AndAlso curr <> TriState.Indeterminate Then _
                 mBackingStore(key) = TriState.Indeterminate
            Else
                ' Otherwise, no current value, just initialise to this
                mBackingStore(key) = value
            End If
        End Sub

        ''' <summary>
        ''' Attempts to get the entry in this dictionary referenced by the given key.
        ''' </summary>
        ''' <param name="key">The key whose entry is required.</param>
        ''' <param name="value">If an entry with the required key was found, on exit,
        ''' this will contain the entry value held against it.</param>
        ''' <returns>True if an entry with the given key was found; False otherwise.
        ''' </returns>
        Public Function TryGetValue(ByVal key As TKey, ByRef value As TriState) As Boolean _
         Implements IDictionary(Of TKey, TriState).TryGetValue
            Return mBackingStore.TryGetValue(key, value)
        End Function

        ''' <summary>
        ''' Checks if the value held in this dictionary against the given key is enabled
        ''' </summary>
        ''' <param name="key">The key to test</param>
        ''' <returns>True if the compound value in this dictionary for the specified key
        ''' is <see cref="TriState.Enabled"/>.</returns>
        Public Function IsEnabled(ByVal key As TKey) As Boolean
            Dim ts As TriState
            If mBackingStore.TryGetValue(key, ts) Then Return (ts = TriState.Enabled)
            Return False
        End Function

        ''' <summary>
        ''' Checks if the value held in this dictionary against the given key is disabled
        ''' </summary>
        ''' <param name="key">The key to test</param>
        ''' <returns>True if the compound value in this dictionary for the specified key
        ''' is <see cref="TriState.Disabled"/>.</returns>
        Public Function IsDisabled(ByVal key As TKey) As Boolean
            Dim ts As TriState
            If mBackingStore.TryGetValue(key, ts) Then Return (ts = TriState.Disabled)
            Return False
        End Function

        ''' <summary>
        ''' Removes the given item from this dictionary.
        ''' </summary>
        ''' <param name="item">The item to remove</param>
        ''' <returns>True if the item was found and removed, false otherwise.</returns>
        Public Function Remove(ByVal item As KeyValuePair(Of TKey, TriState)) As Boolean _
         Implements ICollection(Of KeyValuePair(Of TKey, TriState)).Remove
            Return mBackingStore.Remove(item)
        End Function

        ''' <summary>
        ''' Removes the entry with the given key from this dictionary.
        ''' </summary>
        ''' <param name="key">The key identifying the entry to remove</param>
        ''' <returns>True if the entry was found and removed; False otherwise.</returns>
        Public Function Remove(ByVal key As TKey) As Boolean _
         Implements IDictionary(Of TKey, TriState).Remove
            Return mBackingStore.Remove(key)
        End Function

        ''' <summary>
        ''' Clears this dictionary
        ''' </summary>
        Public Sub Clear() Implements ICollection(Of KeyValuePair(Of TKey, TriState)).Clear
            mBackingStore.Clear()
        End Sub

        ''' <summary>
        ''' Checks if this dictionary contains the given item
        ''' </summary>
        ''' <param name="item">The item to check</param>
        ''' <returns>True if the dictionary contains the given item; False otherwise.
        ''' </returns>
        Public Function Contains(ByVal item As KeyValuePair(Of TKey, TriState)) As Boolean _
         Implements ICollection(Of KeyValuePair(Of TKey, TriState)).Contains
            Return mBackingStore.Contains(item)
        End Function

        ''' <summary>
        ''' Checks if this dictionary contains an entry with the given key
        ''' </summary>
        ''' <param name="key">The key to look for in this dictionary</param>
        ''' <returns>True if the key represents an entry in this dictionary; False
        ''' otherwise.</returns>
        Public Function ContainsKey(ByVal key As TKey) As Boolean _
         Implements IDictionary(Of TKey, TriState).ContainsKey
            Return mBackingStore.ContainsKey(key)
        End Function

        ''' <summary>
        ''' Copies this dictionary into the given array
        ''' </summary>
        ''' <param name="array">The array to which this dictionary should be copied.
        ''' </param>
        ''' <param name="arrayIndex">The index within the array at which the copying of
        ''' the dictionary data should commence.</param>
        Public Sub CopyTo(ByVal array() As KeyValuePair(Of TKey, TriState), ByVal arrayIndex As Integer) Implements ICollection(Of KeyValuePair(Of TKey, TriState)).CopyTo
            mBackingStore.CopyTo(array, arrayIndex)
        End Sub

        ''' <summary>
        ''' Gets an enumerator over the entries in this dictionary.
        ''' </summary>
        ''' <returns>An enumerator over this dictionary's entries.</returns>
        Public Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of TKey, TriState)) _
         Implements IEnumerable(Of KeyValuePair(Of TKey, TriState)).GetEnumerator
            Return mBackingStore.GetEnumerator()
        End Function

        ''' <summary>
        ''' Gets an enumerator over the entries in this dictionary.
        ''' </summary>
        ''' <returns>An enumerator over this dictionary's entries.</returns>
        Private Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return DirectCast(mBackingStore, IEnumerable).GetEnumerator()
        End Function

    End Class
End Namespace
