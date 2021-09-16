Namespace Collections

    ''' Project  : BPCoreLib
    ''' Class    : clsGeneratorDictionary
    ''' <summary>
    ''' A dictionary which will auto-generate a new value object when an unrecognised
    ''' key is used to access it.<br/>
    ''' Note that this is now a full implementation of the IDictionary(Of TKey,TValue)
    ''' interface, meaning that value generation will occur regardless of whether the
    ''' variable is defined as an IDictionary or a clsGeneratorDictionary.
    ''' </summary>
    ''' <typeparam name="TKey">The key used to access this dictionary</typeparam>
    ''' <typeparam name="TValue">The value held against the key within the dictionary.
    ''' This is auto-generated on access when explicitly using this type.</typeparam>
    <Serializable, DebuggerDisplay("Count: {Count}")> _
    Public Class clsGeneratorDictionary(Of TKey, TValue As New)
        Implements IDictionary(Of TKey, TValue)

        ''' <summary>
        ''' The inner dictionary to delegate the functionality of this object to.
        ''' </summary>
        Private mDict As IDictionary(Of TKey, TValue)

        ''' <summary>
        ''' Create a generator dictionary using the default non-ordered dictionary.
        ''' </summary>
        Public Sub New()
            Me.New(Nothing, Nothing)
        End Sub

        ''' <summary>
        ''' Create a generator dictionary using the default non-ordered dictionary and 
        ''' the specified equality comparer.
        ''' </summary>
        Public Sub New(comparer As IEqualityComparer(Of TKey))
            Me.New(Nothing, comparer)
        End Sub

        ''' <summary>
        ''' Create a generator dictionary which wraps the given dictionary. This allows
        ''' an ordered or sorted dictionary to be used with value generation.
        ''' </summary>
        ''' <param name="wrapped">The dictionary to wrap within this generator
        ''' dictionary. If null, this will just use a basic 
        ''' <see cref="Dictionary(Of TKey,TValue)"/> instance.
        ''' </param>
        Public Sub New(ByVal wrapped As IDictionary(Of TKey, TValue))
            Me.New(wrapped, Nothing)
        End Sub

        ''' <summary>
        ''' Create a generator dictionary which wraps the given dictionary and uses 
        ''' the specified equality comparer. 
        ''' </summary>
        ''' <param name="wrapped">The dictionary to wrap within this generator
        ''' dictionary. If null, this will just use a basic 
        ''' <see cref="Dictionary(Of TKey,TValue)"/> instance.
        ''' </param>
        ''' <param name="comparer">The equality comparer to use with the dictionary. 
        ''' If null the default generic equality comparer will be used.</param>
        Private Sub New(ByVal wrapped As IDictionary(Of TKey, TValue),
                       comparer As IEqualityComparer(Of TKey))
            If wrapped Is Nothing Then _
                wrapped = New Dictionary(Of TKey, TValue)(comparer)
            mDict = wrapped
        End Sub

        ''' <summary>
        ''' Get the value associated with the given key, or generate a new value
        ''' if it does not exist within the dictionary.
        ''' </summary>
        ''' <param name="key">The key that the required value is mapped to</param>
        ''' <value>The value to be associated with the given key.</value>
        ''' <returns>The value to be associated with the given key or a newly
        ''' associated instance of the value's type if a pre-existing instance was 
        ''' not already held. </returns>
        Default Public Property Item(ByVal key As TKey) As TValue _
         Implements IDictionary(Of TKey, TValue).Item
            Get
                Dim val As TValue = Nothing
                If Not mDict.TryGetValue(key, val) Then
                    val = New TValue()
                    mDict(key) = val
                End If
                Return val
            End Get
            Set(ByVal value As TValue)
                mDict(key) = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the inner dictionary used to store the key/value pairs by this 
        ''' generator dictionary.
        ''' </summary>
        Public ReadOnly Property InnerDictionary() As IDictionary(Of TKey, TValue)
            Get
                Return mDict
            End Get
        End Property

#Region "Pass-through implementations"
        ' All the implementations below just delegate their functionality to the wrapped collection
        ' Only the default property is overridden in this dictionary implementation.

        Public Sub Add(ByVal item As KeyValuePair(Of TKey, TValue)) Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Add
            mDict.Add(item)
        End Sub

        Public Sub Clear() Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Clear
            mDict.Clear()
        End Sub

        Public Function Contains(ByVal item As KeyValuePair(Of TKey, TValue)) As Boolean Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Contains
            Return mDict.Contains(item)
        End Function

        Public Sub CopyTo(ByVal array() As KeyValuePair(Of TKey, TValue), ByVal arrayIndex As Integer) Implements ICollection(Of KeyValuePair(Of TKey, TValue)).CopyTo
            mDict.CopyTo(array, arrayIndex)
        End Sub

        Public ReadOnly Property Count() As Integer Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Count
            Get
                Return mDict.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly() As Boolean Implements ICollection(Of KeyValuePair(Of TKey, TValue)).IsReadOnly
            Get
                Return mDict.IsReadOnly
            End Get
        End Property

        Public Function Remove(ByVal item As KeyValuePair(Of TKey, TValue)) As Boolean Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Remove
            mDict.Remove(item)
        End Function

        Public Sub Add(ByVal key As TKey, ByVal value As TValue) Implements IDictionary(Of TKey, TValue).Add
            mDict.Add(key, value)
        End Sub

        Public Function ContainsKey(ByVal key As TKey) As Boolean Implements IDictionary(Of TKey, TValue).ContainsKey
            Return mDict.ContainsKey(key)
        End Function

        Public ReadOnly Property Keys() As ICollection(Of TKey) Implements IDictionary(Of TKey, TValue).Keys
            Get
                Return mDict.Keys
            End Get
        End Property

        Public Function Remove(ByVal key As TKey) As Boolean Implements IDictionary(Of TKey, TValue).Remove
            Return mDict.Remove(key)
        End Function

        Public Function TryGetValue(ByVal key As TKey, ByRef value As TValue) As Boolean Implements IDictionary(Of TKey, TValue).TryGetValue
            Return mDict.TryGetValue(key, value)
        End Function

        Public ReadOnly Property Values() As ICollection(Of TValue) Implements IDictionary(Of TKey, TValue).Values
            Get
                Return mDict.Values
            End Get
        End Property

        Public Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of TKey, TValue)) Implements IEnumerable(Of KeyValuePair(Of TKey, TValue)).GetEnumerator
            Return mDict.GetEnumerator()
        End Function

        Private Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return DirectCast(mDict, IEnumerable).GetEnumerator()
        End Function

#End Region

    End Class

End Namespace
