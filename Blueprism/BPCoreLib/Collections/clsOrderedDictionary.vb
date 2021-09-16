Imports System.Collections.Specialized

Namespace Collections

    ''' Project  : BPCoreLib
    ''' Class    : clsOrderedDictionary
    ''' <summary>
    ''' Generic equivalent of the Specialized.OrderedDictionary.
    ''' This class maintains the order of retrieved values (through the enumeration /
    ''' sub-collection methods) as the order of insertion, meaning that direct key
    ''' lookup is available as well as ordered enumeration.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Currently, this just wraps an OrderedDictionary instance and enforces type within
    ''' the public methods / properties... I've read that the OrderedDictionary has a
    ''' strange implementation due to the fact that it can be accessed by index as well 
    ''' as key. I don't think that's particularly useful, so I've not implemented it,
    ''' and, as such, we could put together our own implementation which capitalises on
    ''' the fact that we don't need index-based access (and optimises some of the search/
    ''' modification algorithms).
    ''' </para>
    ''' <para>
    ''' Note that the methods/properties which implement
    ''' <see cref="ICollection">ICollection</see> methods/properties have all been made
    ''' protected, meaning that they are only accessible through subclasses, or by
    ''' calling from an ICollection-scoped variable. This is mainly because the 
    ''' ICollection methods don't map neatly onto Dictionaries, and serve mainly to
    ''' confuse things rather than enabling anything.
    ''' </para>
    ''' </remarks>
    <Serializable, DebuggerDisplay("Count: {Count}")> _
    Public Class clsOrderedDictionary(Of TKey, TValue)
        Implements IDictionary(Of TKey, TValue)

        ''' <summary>
        ''' The wrapped dictionary, instantiated at construction time
        ''' </summary>
        Private mDict As OrderedDictionary

        ''' <summary>
        ''' Creates a new ordered dictionary
        ''' </summary>  
        Public Sub New()
            Me.New(GetEmpty.IDictionary(Of TKey, TValue))
        End Sub

        ''' <summary>
        ''' Creates a new ordered dictionary, initialised from the values in the
        ''' given dictionary.
        ''' </summary>
        ''' <param name="dict">The dictionary to initialise this object from.</param>
        ''' <exception cref="ArgumentNullException">If <paramref name="dict"/> is
        ''' null.</exception>
        Public Sub New(dict As IDictionary(Of TKey, TValue))
            mDict = New OrderedDictionary()
            If dict Is Nothing Then Throw New ArgumentNullException(NameOf(dict))
            For Each entry As KeyValuePair(Of TKey, TValue) In dict
                Add(entry.Key, entry.Value)
            Next
        End Sub

        ''' <summary>
        ''' Adds the given key-value pair to this dictionary.
        ''' </summary>
        ''' <param name="item">The item comprising of the key and value to add to this
        ''' dictionary.</param>
        Protected Sub Add(ByVal item As KeyValuePair(Of TKey, TValue)) _
         Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Add
            Add(item.Key, item.Value)
        End Sub

        ''' <summary>
        ''' Adds the given value to this dictionary, mapped to the given key.
        ''' </summary>
        ''' <param name="key">The key to map the specified value to</param>
        ''' <param name="value">The value to assign to the given key</param>
        Public Sub Add(ByVal key As TKey, ByVal value As TValue) _
         Implements IDictionary(Of TKey, TValue).Add
            mDict.Add(key, value)
        End Sub

        ''' <summary>
        ''' Clears all elements within this dictionary.
        ''' </summary>
        Public Sub Clear() Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Clear
            mDict.Clear()
        End Sub

        ''' <summary>
        ''' Checks if this dictionary contains the given key/value.
        ''' This is here because IDictionary extends ICollection... which is a bit daft,
        ''' since it doesn't exactly map onto it... nevertheless, this assumes that
        ''' you're checking the actual key/value pair, rather than just the key (which is
        ''' checkable using <see cref="ContainsKey">ContainsKey()</see>) - this obeys the
        ''' contract of ICollection, rather than assuming that since this is a dictionary,
        ''' you're only bothered about the key... which, of course, you should be.
        ''' It's insane, but that's what you have to do when you implement the half-arsed
        ''' attempt at an interface that is the IDictionary.
        ''' </summary>
        ''' <param name="item">The key/value pair to check for in this collection</param>
        ''' <returns>True if the key is present in this dictionary, and the value mapped
        ''' to is the same as that given (according to 
        ''' <see cref="M:System.Object.Equals(Object,Object)">Object.Equals</see>)
        ''' </returns>
        Protected Function Contains(ByVal item As KeyValuePair(Of TKey, TValue)) As Boolean _
         Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Contains
            Return ContainsKey(item.Key) AndAlso Object.Equals(Me(item.Key), item.Value)
        End Function

        ''' <summary>
        ''' Checks if this dictionary contains the given key
        ''' </summary>
        ''' <param name="key">The key to check for</param>
        ''' <returns>True if this dictionary contains the given key, False otherwise
        ''' </returns>
        Public Function ContainsKey(ByVal key As TKey) As Boolean _
         Implements IDictionary(Of TKey, TValue).ContainsKey
            Return mDict.Contains(key)
        End Function

        ''' <summary>
        ''' Copies the key/value pairs that represent this dictionary into the given
        ''' array.
        ''' </summary>
        ''' <param name="array">The array into which this dictionary should be copied.
        ''' </param>
        ''' <param name="arrayIndex">The index at which the pairs should be copied
        ''' into.</param>
        Public Sub CopyTo( _
         ByVal array() As KeyValuePair(Of TKey, TValue), ByVal arrayIndex As Integer) _
         Implements ICollection(Of KeyValuePair(Of TKey, TValue)).CopyTo

            For Each entry As DictionaryEntry In mDict
                array(arrayIndex) = New KeyValuePair(Of TKey, TValue)( _
                 DirectCast(entry.Key, TKey), DirectCast(entry.Value, TValue))
                arrayIndex += 1
            Next

        End Sub

        ''' <summary>
        ''' The current number of elements held within this dictionary.
        ''' </summary>
        Public ReadOnly Property Count() As Integer _
         Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Count
            Get
                Return mDict.Count
            End Get
        End Property

        ''' <summary>
        ''' Checks if this dictionary is read only or not.
        ''' </summary>
        Public ReadOnly Property IsReadOnly() As Boolean _
         Implements ICollection(Of KeyValuePair(Of TKey, TValue)).IsReadOnly
            Get
                Return mDict.IsReadOnly
            End Get
        End Property

        ''' <summary>
        ''' Removes the given key/value pair.
        ''' Like <see cref="Contains">Contains()</see>, this doesn't quite fit into the
        ''' dictionary implementation. Like Contains(), I've implemented the ICollection
        ''' contract quite strictly, meaning that the item will only be removed if the
        ''' given item matches precisely, meaning that both the key and the value match
        ''' what is currently held on the dictionary.
        ''' </summary>
        ''' <param name="item">The item to remove from this dictionary, if it exists
        ''' </param>
        ''' <returns>True if the item has been found and removed; False if it was not
        ''' found.</returns>
        Protected Function Remove(ByVal item As KeyValuePair(Of TKey, TValue)) As Boolean _
         Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Remove
            If Contains(item) Then Return Remove(item.Key)
            Return False
        End Function

        ''' <summary>
        ''' Removes the entry from the dictionary mapped to the given key.
        ''' </summary>
        ''' <param name="key">The key to remove from the dictionary.</param>
        ''' <returns>True if the key was found and the corresponding entry was 
        ''' removed from this dictionary, False otherwise.</returns>
        Public Function Remove(ByVal key As TKey) As Boolean _
         Implements IDictionary(Of TKey, TValue).Remove
            If mDict.Contains(key) Then
                mDict.Remove(key)
                Return True
            End If
            Return False
        End Function

        ''' <summary>
        ''' Gets/sets the value represented by the given key from this dictionary.
        ''' </summary>
        Default Public Property Item(ByVal key As TKey) As TValue _
         Implements IDictionary(Of TKey, TValue).Item
            Get
                Return DirectCast(mDict(key), TValue)
            End Get
            Set(ByVal value As TValue)
                mDict(key) = value
            End Set
        End Property

        ''' <summary>
        ''' Gets a collection over the keys within this dictionary. Note that due to
        ''' the nature of this implementation, the returned collection will <em>not</em>
        ''' reflect any later changes made to this dictionary.
        ''' </summary>
        Public ReadOnly Property Keys() As ICollection(Of TKey) _
         Implements IDictionary(Of TKey, TValue).Keys
            Get
                Dim list As New List(Of TKey)()
                For Each key As TKey In mDict.Keys
                    list.Add(key)
                Next
                Return list
            End Get
        End Property

        ''' <summary>
        ''' Attempts to get the value corresponding to the given key if it is held in
        ''' this dictionary.
        ''' </summary>
        ''' <param name="key">The key to use to look up in this dictionary.</param>
        ''' <param name="value">The value found mapped to the given key (or unchanged
        ''' if the key was not found)</param>
        ''' <returns>True if the key was found and <c>value</c> has been set to the
        ''' value found mapped to it; False if the key was not found.</returns>
        Public Function TryGetValue(ByVal key As TKey, ByRef value As TValue) As Boolean _
         Implements IDictionary(Of TKey, TValue).TryGetValue
            If mDict.Contains(key) Then
                value = DirectCast(mDict(key), TValue)
                Return True
            End If
            Return False
        End Function

        ''' <summary>
        ''' Gets a collection over the values held in this dictionary. Note that due to
        ''' the nature of this implementation, the returned collection will <em>not</em>
        ''' reflect any later changes made to this dictionary.
        ''' </summary>
        Public ReadOnly Property Values() As ICollection(Of TValue) _
         Implements IDictionary(Of TKey, TValue).Values
            Get
                Dim list As New List(Of TValue)()
                For Each val As TValue In mDict.Values
                    list.Add(val)
                Next
                Return list
            End Get
        End Property

        ''' <summary>
        ''' Gets an enumerator over the key/value pairs held in this dictionary.
        ''' </summary>
        ''' <returns>An enumerator over this dictionary.</returns>
        Public Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of TKey, TValue)) _
         Implements IEnumerable(Of KeyValuePair(Of TKey, TValue)).GetEnumerator
            Dim items As New List(Of KeyValuePair(Of TKey, TValue))
            For Each entry As DictionaryEntry In mDict
                items.Add(New KeyValuePair(Of TKey, TValue)( _
                 DirectCast(entry.Key, TKey), DirectCast(entry.Value, TValue)))
            Next
            Return items.GetEnumerator()
        End Function

        ''' <summary>
        ''' Gets an enumerator over the key/value pairs held in this dictionary.
        ''' </summary>
        ''' <returns>An enumerator over this dictionary.</returns>
        Protected Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function
    End Class

End Namespace
