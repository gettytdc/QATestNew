Namespace Collections

    ''' <summary>
    ''' A dictionary which returns a default value when asked to look up a key that
    ''' doesn't exist
    ''' </summary>
    ''' <typeparam name="TKey">The type of key in the dictionary</typeparam>
    ''' <typeparam name="TValue">The type of value in the dictionary</typeparam>
    <Serializable, DebuggerDisplay("Count: {Count}")>
    Public Class clsDefaultValueDictionary(Of TKey, TValue)
        Inherits BackedDictionary(Of TKey, TValue)

#Region " Member Variables "

        ' The default value to use for failed lookups
        Private mDefaultValue As TValue

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new basic dictionary of the given type and value, which has a
        ''' default value of null for reference types, or the type-specific default
        ''' value for value types.
        ''' </summary>
        Public Sub New()
            Me.New(New Dictionary(Of TKey, TValue), Nothing)
        End Sub

        ''' <summary>
        ''' Creates a new dictionary, backed by the given map, which has a default
        ''' value of null for reference types, or the type-specific default value for
        ''' value types.
        ''' </summary>
        ''' <param name="backingMap">The map to use to back this dictionary</param>
        Public Sub New(ByVal backingMap As IDictionary(Of TKey, TValue))
            Me.New(backingMap, Nothing)
        End Sub

        ''' <summary>
        ''' Creates a new basic dictionary of the given type and value, which has the
        ''' specified default value.
        ''' </summary>
        ''' <param name="defaultValue">The default value to return to callers when a
        ''' lookup is requested with a key which doesn't exist in this dictionary.
        ''' </param>
        Public Sub New(ByVal defaultValue As TValue)
            Me.New(New Dictionary(Of TKey, TValue), defaultValue)
        End Sub

        ''' <summary>
        ''' Creates a new dictionary, backed by the given map, which has the
        ''' specified default value.
        ''' </summary>
        ''' <param name="backingMap">The map to use to back this dictionary</param>
        ''' <param name="defaultValue">The default value to return to callers when a
        ''' lookup is requested with a key which doesn't exist in this dictionary.
        ''' </param>
        Public Sub New( _
         ByVal backingMap As IDictionary(Of TKey, TValue), ByVal defaultValue As TValue)
            MyBase.New(backingMap)
            mDefaultValue = defaultValue
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets the value associated with the given key.
        ''' </summary>
        ''' <param name="key">The key of the value to get or set.</param>
        ''' <value></value>
        ''' <returns>The value associated with the specified key. If the specified
        ''' key is not found, a get operation returns the default value set in this
        ''' dictionary, and a set operation creates a new element with the specified
        ''' key.</returns>
        ''' <exception cref="ArgumentNullException">if <paramref name="key"/> is null
        ''' </exception>
        Default Public Overrides Property Item(ByVal key As TKey) As TValue
            Get
                Dim val As TValue = Nothing
                If TryGetValue(key, val) Then Return val
                Return mDefaultValue
            End Get
            Set(ByVal value As TValue)
                MyBase.Item(key) = value
            End Set
        End Property

#End Region

    End Class

End Namespace

