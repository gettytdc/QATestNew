Imports System.Runtime.Serialization

Namespace Collections

    ''' <summary>
    ''' A map of elements which are allowed access to a resource. This provides the
    ''' functionality to test for accessibility and provides a mechanism with which
    ''' 'any' element is provided access.
    ''' </summary>
    ''' <typeparam name="T">The type of constraint which determines access to a
    ''' resource</typeparam>
    <Serializable, DebuggerDisplay("Count: {Count}")>
    <DataContract(Namespace:="bp")>
    Public Class AccessSet(Of T) : Inherits clsSet(Of T)

        ' Flag indicating if 'any' is currently set
        <DataMember>
        Private mAccessToAll As Boolean

        ' Flag indicating if adding a default value (null or default(T)) means that
        ' the set should be treated as giving access to all
        <DataMember>
        Private mDefaultImpliesAccessToAll As Boolean = True

        ''' <summary>
        ''' Creates a new empty constraint map
        ''' </summary>
        Public Sub New()
            MyBase.New()
        End Sub

        ''' <summary>
        ''' Indicates whether this constraint map has granted access to all elements
        ''' </summary>
        Public Property AccessToAll() As Boolean
            Get
                Return mAccessToAll
            End Get
            Set(ByVal value As Boolean)
                If mAccessToAll = value Then Return
                mAccessToAll = value
                ' We don't need the extra entries if access to all is currently set
                If mAccessToAll Then Clear()
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a property which determines whether to treat 'default'
        ''' values just like all other values, or to interpret them as a request to
        ''' allow all contenders access to the targeted resource. A 'default' value
        ''' in this context is null (for reference types) or the default initialised
        ''' state (for value types). The default state is to treat them as requests
        ''' to provide access to all (because that's what we tend to do quite
        ''' regularly on the database, so it makes sense to make that the default)
        ''' </summary>
        Public Property DefaultItemSetsAccessToAll() As Boolean
            Get
                Return mDefaultImpliesAccessToAll
            End Get
            Set(ByVal value As Boolean)
                If mDefaultImpliesAccessToAll = value Then Return
                mDefaultImpliesAccessToAll = value
                ' If setting it to true, traverse the set of accessors and see if a
                ' default entry exists; if it does, set the 'access to all' property.
                If mDefaultImpliesAccessToAll Then
                    Dim foundDefaultValue As Boolean = False
                    For Each elem As T In Me
                        If IsDefault(elem) Then foundDefaultValue = True : Exit For
                    Next
                    If foundDefaultValue Then AccessToAll = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' Adds the given item to this access set, returning whether this collection
        ''' was modified as a result.
        ''' </summary>
        ''' <param name="item">The item to add to the result set</param>
        ''' <returns>True if the collection was changed as a result of this addition
        ''' </returns>
        Public Overrides Function Add(ByVal item As T) As Boolean
            ' If we already have access to all, adding is a no-op
            If AccessToAll Then Return False
            ' If the given item is null or default(T) then that is treated as
            ' 'access to all', treat it as such
            If mDefaultImpliesAccessToAll AndAlso IsDefault(item) Then
                AccessToAll = True
                Return True
            End If
            ' Otherwise, just add it as normal
            Return MyBase.Add(item)
        End Function

        ''' <summary>
        ''' Removes the given item from this access set, returning whether this
        ''' collection was modified as a result.
        ''' </summary>
        ''' <param name="item">The item to remove from the result set</param>
        ''' <returns>True if the collection was changed as a result of this removal
        ''' </returns>
        Public Overrides Function Remove(ByVal item As T) As Boolean
            ' If the given item is null or default(T) and that is treated as
            ' 'access to all', treat it as such
            If mDefaultImpliesAccessToAll AndAlso IsDefault(item) Then
                If Not AccessToAll Then Return False
                AccessToAll = False
                Return True
            End If
            ' Otherwise, just remove as normal
            Return MyBase.Remove(item)
        End Function


        ''' <summary>
        ''' Checks if the given element is a default value for the type of this class
        ''' </summary>
        ''' <param name="elem">The element to test</param>
        ''' <returns>True if the given element was a default value - ie. null for a
        ''' reference type or the basic initialised value for a value type.</returns>
        Private Shared Function IsDefault(ByVal elem As T) As Boolean
            Return EqualityComparer(Of T).Default.Equals(elem, Nothing)
        End Function

        ''' <summary>
        ''' Checks if the given contender has access according to this map
        ''' </summary>
        ''' <param name="contender">The element to see if it is registered as having
        ''' access according to this map</param>
        ''' <returns></returns>
        Public Function HasAccess(ByVal contender As T) As Boolean
            If mAccessToAll Then Return True
            Return Contains(contender)
        End Function

    End Class

End Namespace