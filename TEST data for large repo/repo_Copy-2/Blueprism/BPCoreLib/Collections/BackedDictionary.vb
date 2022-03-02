Namespace Collections

    ''' <summary>
    ''' Base class for dictionaries backed with other dictionaries. All IDictionary
    ''' method implementations are virtual so that they can be easily extended and
    ''' overridden.
    ''' </summary>
    ''' <typeparam name="TKey">The type of the key in this dictionary and in the
    ''' backed dictionary.</typeparam>
    ''' <typeparam name="TValue">The type of the value in this dictionary and in the
    ''' backed dictionary.</typeparam>
    ''' <remarks>This doesn't add anything to the backed dictionary given to it on
    ''' construction, as such it's pointless without being extended and overridden.
    ''' </remarks>
    <Serializable, DebuggerDisplay("Count: {Count}")>
    Public MustInherit Class BackedDictionary(Of TKey, TValue)
        Implements IDictionary(Of TKey, TValue)

#Region " Member Vars "

        ' The dictionary backed in this object.
        Private ReadOnly mDict As IDictionary(Of TKey, TValue)

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new backed dictionary using the given map as the backing map.
        ''' </summary>
        ''' <param name="backingMap">The dictionary to back this dictionary with.
        ''' </param>
        Public Sub New(ByVal backingMap As IDictionary(Of TKey, TValue))
            If backingMap Is Nothing Then _
             Throw New ArgumentNullException(NameOf(backingMap))
            mDict = backingMap
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' The dictionary which is backing this object.
        ''' </summary>
        Protected ReadOnly Property BackingMap() As IDictionary(Of TKey, TValue)
            Get
                Return mDict
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the value associated with the specified key.
        ''' </summary>
        ''' <param name="key">The key of the value to get or set.</param>
        ''' <value></value>
        ''' <returns>The value associated with the specified key. If the specified
        ''' key is not found, a get operation throws a KeyNotFoundException, and a
        ''' set operation creates a new element with the specified key.</returns>
        ''' <exception cref="ArgumentNullException">if <paramref name="key"/> is null
        ''' </exception>
        ''' <exception cref="KeyNotFoundException">If the property is retrieved and
        ''' key does not exist in the collection.</exception>
        Default Public Overridable Property Item(ByVal key As TKey) As TValue _
         Implements IDictionary(Of TKey, TValue).Item
            Get
                Return mDict.Item(key)
            End Get
            Set(ByVal value As TValue)
                mDict.Item(key) = value
            End Set
        End Property

        ''' <summary>
        ''' Gets a collection containing the keys in the Dictionary
        ''' </summary>
        ''' <returns>A KeyCollection containing the keys in the Dictionary</returns>
        Public Overridable ReadOnly Property Keys() As ICollection(Of TKey) _
         Implements IDictionary(Of TKey, TValue).Keys
            Get
                Return mDict.Keys
            End Get
        End Property

        ''' <summary>
        ''' Gets an ICollection containing the values in the Dictionary
        ''' </summary>
        ''' <returns>An ICollection containing the values in the Dictionary</returns>
        Public Overridable ReadOnly Property Values() As ICollection(Of TValue) _
         Implements IDictionary(Of TKey, TValue).Values
            Get
                Return mDict.Values
            End Get
        End Property

        ''' <summary>
        ''' Gets a count of all elements in the dictionary
        ''' </summary>
        Public Overridable ReadOnly Property Count() As Integer _
         Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Count
            Get
                Return mDict.Count
            End Get
        End Property

        ''' <summary>
        ''' Determines if the backing map, and thus this dictionary, are readonly
        ''' </summary>
        Public Overridable ReadOnly Property IsReadOnly() As Boolean _
         Implements ICollection(Of KeyValuePair(Of TKey, TValue)).IsReadOnly
            Get
                Return mDict.IsReadOnly
            End Get
        End Property

#End Region

#Region " Methods "

        ''' <summary>
        ''' Adds the given key/value pair into this dictionary
        ''' </summary>
        ''' <param name="item">The item to add</param>
        ''' <exception cref="NotSupportedException">If the IDictionary is read-only
        ''' </exception> 
        ''' <exception cref="ArgumentException">If an element with the same key
        ''' already exists in the IDictionary.</exception>
        ''' <exception cref="ArgumentNullException">If the <paramref name="item"/>
        ''' parameter is null</exception>
        Public Overridable Sub Add(ByVal item As KeyValuePair(Of TKey, TValue)) _
         Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Add
            mDict.Add(item)
        End Sub

        ''' <summary>
        ''' Removes all items from the dictionary
        ''' </summary>
        ''' <exception cref="NotSupportedException">If the IDictionary is read-only
        ''' </exception> 
        Public Overridable Sub Clear() _
         Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Clear
            mDict.Clear()
        End Sub

        ''' <summary>
        ''' Determines whether the Dictionary contains a specific key/value pair - ie
        ''' if the dictionary contains an entry with the given key <em>and</em> value
        ''' </summary>
        ''' <param name="item">The key value pair to locate in the dictionary</param>
        ''' <returns>True if an entry in the dictionary has the given key and value;
        ''' False otherwise.</returns>
        Public Overridable Function Contains( _
         ByVal item As KeyValuePair(Of TKey, TValue)) _
         As Boolean Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Contains
            Return mDict.Contains(item)
        End Function

        ''' <summary>
        ''' Copies the key/value pairs of the IDictionary to an Array, starting at a
        ''' particular Array index.
        ''' </summary>
        ''' <param name="array">The one-dimensional Array that is the destination of
        ''' the elements copied from dictionary. The Array must have zero-based
        ''' indexing.
        ''' </param>
        ''' <param name="arrayIndex">The zero-based index in array at which copying
        ''' begins.</param>
        ''' <exception cref="ArgumentOutOfRangeException">if
        ''' <paramref name="arrayIndex"/> is less than 0.</exception>
        ''' <exception cref="ArgumentNullException">if <paramref name="array"/> is
        ''' null. </exception>
        ''' <exception cref="ArgumentException">if <paramref name="array"/> is
        ''' multidimensional. -or- <paramref name="arrayIndex"/> is equal to or
        ''' greater than the length of <paramref name="array"/>. -or- The number of
        ''' elements in the source dictionary is greater than the available space
        ''' from <paramref name="arrayIndex"/> to the end of the destination array.
        ''' </exception>
        Public Overridable Sub CopyTo( _
         ByVal array() As KeyValuePair(Of TKey, TValue), ByVal arrayIndex As Integer) _
         Implements ICollection(Of KeyValuePair(Of TKey, TValue)).CopyTo
            mDict.CopyTo(array, arrayIndex)
        End Sub

        ''' <summary>
        ''' Removes the first occurrence of a specific object from the dictionary
        ''' </summary>
        ''' <param name="item">The object to remove from the dictionary</param>
        ''' <returns>true if item was successfully removed from the dictionary;
        ''' false otherwise. This method also returns false if item is not found in
        ''' the original dictionary</returns>
        ''' <exception cref="NotSupportedException">If the dictionary is readonly
        ''' </exception>
        ''' <remarks>In the case of a dictionary, this should only remove the key
        ''' value pair if the value corresponding to the specified key in the
        ''' dictionary matches that in the parameter, although this is obviously
        ''' dependent on subclass implementations.</remarks>
        Public Overridable Function Remove( _
         ByVal item As KeyValuePair(Of TKey, TValue)) As Boolean _
         Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Remove
            Return mDict.Remove(item)
        End Function

        ''' <summary>
        ''' Adds the specified key and value to the dictionary.
        ''' </summary>
        ''' <param name="key">The key of the element to add.</param>
        ''' <param name="value">The value of the element to add. The value can be
        ''' null for reference types</param>
        ''' <exception cref="ArgumentNullException">If <paramref name="key"/> is null
        ''' </exception>
        ''' <exception cref="ArgumentException">If an element with the same key
        ''' already exists in the dictionary</exception>
        Public Overridable Sub Add(ByVal key As TKey, ByVal value As TValue) _
         Implements IDictionary(Of TKey, TValue).Add
            mDict.Add(key, value)
        End Sub

        ''' <summary>
        ''' Determines whether the Dictionary contains the specified key
        ''' </summary>
        ''' <param name="key">The key to locate in the Dictionary</param>
        ''' <returns>true if the Dictionary contains an element with the specified
        ''' key; otherwise, false</returns>
        ''' <exception cref="ArgumentNullException">If the given key is null.
        ''' </exception>
        Public Overridable Function ContainsKey(ByVal key As TKey) As Boolean _
         Implements IDictionary(Of TKey, TValue).ContainsKey
            Return mDict.ContainsKey(key)
        End Function

        ''' <summary>
        ''' Removes the value with the specified key from the Dictionary
        ''' </summary>
        ''' <param name="key">The key of the element to remove</param>
        ''' <returns>true if the element is successfully found and removed;
        ''' otherwise, false. This method returns false if key is not found in the
        ''' Dictionary </returns>
        ''' <exception cref="ArgumentNullException">if <paramref name="key"/> is null
        ''' </exception>
        Public Overridable Function Remove(ByVal key As TKey) As Boolean _
         Implements IDictionary(Of TKey, TValue).Remove
            Return mDict.Remove(key)
        End Function

        ''' <summary>
        ''' Gets the value associated with the specified key
        ''' </summary>
        ''' <param name="key">The key whose value to get</param>
        ''' <param name="value">When this method returns, the value associated with
        ''' the specified key, if the key is found; otherwise, the default value for
        ''' the type of the value parameter.</param>
        ''' <returns>true if the dictionary contains an element with the specified
        ''' key; otherwise, false</returns>
        ''' <exception cref="ArgumentNullException">if <paramref name="key"/> is null
        ''' </exception>
        Public Overridable Function TryGetValue( _
         ByVal key As TKey, ByRef value As TValue) As Boolean _
         Implements IDictionary(Of TKey, TValue).TryGetValue
            Return mDict.TryGetValue(key, value)
        End Function

        ''' <summary>
        ''' Returns an enumerator that iterates through the collection
        ''' </summary>
        ''' <returns>An <see cref="IEnumerator(Of T)"/> that can be used to iterate
        ''' through the collection</returns>
        Public Overridable Function GetEnumerator() _
         As IEnumerator(Of KeyValuePair(Of TKey, TValue)) _
         Implements IEnumerable(Of KeyValuePair(Of TKey, TValue)).GetEnumerator
            Return mDict.GetEnumerator()
        End Function

        ''' <summary>
        ''' Returns an enumerator that iterates through the collection
        ''' </summary>
        ''' <returns>An <see cref="IEnumerator"/> that can be used to iterate through
        ''' the collection</returns>
        Public Overridable Function GetNonGenericEnumerator() As IEnumerator _
         Implements IEnumerable.GetEnumerator
            Return DirectCast(mDict, IEnumerable).GetEnumerator()
        End Function

#End Region

    End Class

End Namespace
