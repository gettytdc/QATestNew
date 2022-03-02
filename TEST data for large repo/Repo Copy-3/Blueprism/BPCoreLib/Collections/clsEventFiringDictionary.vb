Namespace Collections

    ''' <summary>
    ''' Dictionary implementation which fires events when the underlying dictionary
    ''' is modified (through this interface)
    ''' </summary>
    ''' <typeparam name="TKey">The type of key in this dictionary.</typeparam>
    ''' <typeparam name="TValue">The type of value in this dictionary.</typeparam>
    <Serializable, DebuggerDisplay("Count: {Count}")>
    Public Class clsEventFiringDictionary(Of TKey, TValue)
        Inherits clsEventFiringCollection(Of KeyValuePair(Of TKey, TValue))
        Implements IDictionary(Of TKey, TValue)


#Region " Event Declarations "

        ''' <summary>
        ''' Event fired when an item is set using the default property in this
        ''' dictionary implementation.
        ''' </summary>
        ''' <param name="dict">The source dictionary on which the item was set.
        ''' </param>
        ''' <param name="pair">The key value pair which was set into the dictionary.
        ''' </param>
        ''' <param name="prevValue">The previous value corresponding to the key that
        ''' was set, if there was one. Null otherwise.</param>
        Public Event ItemSet(ByVal dict As IDictionary(Of TKey, TValue), _
         ByVal pair As KeyValuePair(Of TKey, TValue), ByVal prevValue As TValue)

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new event firing dictionary wrapped around a basic
        ''' <see cref="Dictionary(Of TKey,TValue)"/> implementation.
        ''' If a more specific dictionary implementation is required, see the other
        ''' constructor.
        ''' </summary>
        Public Sub New()
            Me.New(New Dictionary(Of TKey, TValue))
        End Sub

        ''' <summary>
        ''' Creates a new event firing dictionary wrapping the given dictionary.
        ''' </summary>
        ''' <param name="dict">The dictionary for which modification events are
        ''' required.</param>
        Public Sub New(ByVal dict As IDictionary(Of TKey, TValue))
            MyBase.New(dict)
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' The inner dictionary wrapped by this object.
        ''' </summary>
        Public ReadOnly Property InnerDictionary() As IDictionary(Of TKey, TValue)
            Get
                Return DirectCast(InnerCollection, IDictionary(Of TKey, TValue))
            End Get
        End Property

        ''' <summary>
        ''' The value associated with the given key.
        ''' </summary>
        ''' <param name="key">The key for which the associated value is required.
        ''' </param>
        ''' <value>The value corresponding to the given key</value>
        ''' <exception cref="ArgumentNullException">If the given key was null.
        ''' </exception>
        ''' <exception cref="KeyNotFoundException">If the property is retrieved and
        ''' the given key was not found in this dictionary.</exception>
        ''' <exception cref="NotSupportedException">If the underlying dictionary is
        ''' read only and an attempt is made to set the property.</exception>
        Default Public Property Item(ByVal key As TKey) As TValue _
         Implements IDictionary(Of TKey, TValue).Item
            Get
                Return InnerDictionary(key)
            End Get
            Set(ByVal value As TValue)
                Dim oldValue As TValue = Nothing
                InnerDictionary.TryGetValue(key, oldValue)
                InnerDictionary(key) = value
                OnItemSet(New KeyValuePair(Of TKey, TValue)(key, value), oldValue)
            End Set
        End Property

        ''' <summary>
        ''' The values in a collection from the underlying dictionary.
        ''' </summary>
        Public ReadOnly Property Values() As ICollection(Of TValue) _
         Implements IDictionary(Of TKey, TValue).Values
            Get
                Return InnerDictionary.Values
            End Get
        End Property

        ''' <summary>
        ''' The keys currently held on this dictionary.
        ''' </summary>
        Public ReadOnly Property Keys() As ICollection(Of TKey) _
         Implements IDictionary(Of TKey, TValue).Keys
            Get
                Return InnerDictionary.Keys
            End Get
        End Property

#End Region

#Region " Methods "

        ''' <summary>
        ''' Adds an entry with the given key and value to this dictionary.
        ''' </summary>
        ''' <param name="key">The key for the new entry in the map</param>
        ''' <param name="value">The value for the new entry in the map</param>
        ''' <exception cref="ArgumentNullException">If key is null</exception>
        ''' <exception cref="ArgumentException">If an entry represented by the given
        ''' key already exists in the dictionary.</exception>
        ''' <exception cref="NotSupportedException">If the underlying dictionary is
        ''' read-only</exception>
        Public Overloads Sub Add(ByVal key As TKey, ByVal value As TValue) _
         Implements IDictionary(Of TKey, TValue).Add
            Add(New KeyValuePair(Of TKey, TValue)(key, value))
        End Sub

        ''' <summary>
        ''' Checks if this dictionary contains the given key.
        ''' </summary>
        ''' <param name="key">The key to search for in the dictionary.</param>
        ''' <returns>True if the key is held in this dictionary; False otherwise.
        ''' </returns>
        ''' <exception cref="ArgumentNullException">If key is null</exception>
        Public Function ContainsKey(ByVal key As TKey) As Boolean _
         Implements IDictionary(Of TKey, TValue).ContainsKey
            Return InnerDictionary.ContainsKey(key)
        End Function

        ''' <summary>
        ''' Removes the entry in this dictionary with the given key
        ''' </summary>
        ''' <param name="key">The key representing the entry to remove from this
        ''' dictionary</param>
        ''' <returns>True if the entry represented by the given key was removed;
        ''' False if no such entry was found or if the entry was not removed for
        ''' other reasons specific to the underlying dictionary implementation.
        ''' </returns>
        ''' <exception cref="ArgumentNullException">If key is null</exception>
        ''' <exception cref="NotSupportedException">If the underlying dictionary
        ''' is read only</exception>
        Public Overloads Function Remove(ByVal key As TKey) As Boolean _
         Implements IDictionary(Of TKey, TValue).Remove
            Dim val As TValue = Nothing
            If Not InnerDictionary.TryGetValue(key, val) Then Return False
            If Not InnerDictionary.Remove(key) Then Return False
            OnItemRemoved(New KeyValuePair(Of TKey, TValue)(key, val))
            Return True
        End Function

        ''' <summary>
        ''' Tries to get the value corresponding to the given key from this map.
        ''' </summary>
        ''' <param name="key">The key for which the corresponding value is required.
        ''' </param>
        ''' <param name="value">The reference into which the corresponding value
        ''' should be stored if an entry is present in the dictionary.</param>
        ''' <returns>True if the dictionary contained an entry corresponding to the
        ''' given key and the value was thus set; False otherwise.</returns>
        ''' <exception cref="ArgumentNullException">If key is null</exception>
        Public Function TryGetValue(ByVal key As TKey, ByRef value As TValue) As Boolean _
         Implements IDictionary(Of TKey, TValue).TryGetValue
            Return InnerDictionary.TryGetValue(key, value)
        End Function

        ''' <summary>
        ''' Fires the <see cref="ItemSet"/> for the given key / values.
        ''' </summary>
        ''' <param name="pair">The newly set entry in the dictionary.</param>
        ''' <param name="oldValue">The old value corresponding to the key in the
        ''' associated entry - null if there was no entry in the dictionary
        ''' corresponding to that key prior to the set operation which triggered
        ''' the event.</param>
        Protected Overridable Sub OnItemSet( _
         ByVal pair As KeyValuePair(Of TKey, TValue), ByVal oldValue As TValue)
            RaiseEvent ItemSet(Me, pair, oldValue)
        End Sub

#End Region

    End Class
End Namespace
