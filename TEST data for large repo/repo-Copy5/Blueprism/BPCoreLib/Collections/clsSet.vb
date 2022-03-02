Imports System.Runtime.Serialization

Namespace Collections

    ''' Project  : BPCoreLib
    ''' Class    : clsSet
    ''' <summary>
    ''' <para>
    ''' Basic set class, which allows little functionality above a standard
    ''' 'ICollection' implementation, but allows faster direct access than a
    ''' list-based class  (O(log n) over O(n)) because it uses a Dictionary in the
    ''' background to store the keys.</para><para>
    ''' The primary difference when compared to a collection is that a set can
    ''' contain an item only once.
    ''' </para><para>
    ''' The only set-specific methods provided here are union, intersect, difference
    ''' and subtract methods, which all modify this object based on the input
    ''' collection.
    ''' </para>
    ''' </summary>
    ''' <typeparam name="T">The type of elements that this set contains</typeparam>
    ''' <remarks>The default implementation of a set has arbitrary order, meaning
    ''' that the order of its contained elements cannot be guaranteed. If order is
    ''' important, the clsSortedSet class will sort its elements based on the given
    ''' Comparer or the natural order of the elements themselves.
    ''' </remarks>
    <Serializable, DebuggerDisplay("Count: {Count}"), DataContract(Namespace:="bp")>
    Public Class clsSet(Of T)

        Implements IBPSet(Of T), ICollection(Of T), IEnumerable(Of T)

        ' The comparer set in the backing dictionary of this set on creation
        ' Null if no comparer was set by this class.
        <DataMember(Name:="mc", EmitDefaultValue:=False)>
        Private mComparer As IEqualityComparer(Of T)

        ' The dictionary used by this set to maintain its keys
        Private mStore As IDictionary(Of T, Object)


        ''' <summary>
        ''' Added to help serialization, dictionaries are expensive to serialize and we only use the key collection
        ''' </summary>
        ''' <returns></returns>
        <DataMember(Name:="i")>
        Private Property Items As ICollection(Of T)
            Get
                Return mStore.Keys
            End Get
            Set(value As ICollection(Of T))
                mStore = value.ToDictionary(Of T, Object)(Function(x) x, Function(y) Nothing)
            End Set
        End Property

        ''' <summary>
        ''' The dictionary that this set is wrapping, available for subclasses only.
        ''' The keys represent the 'values' in this collection.
        ''' The value within the dictionary will always be 'Nothing'.
        ''' </summary>
        Protected Overridable ReadOnly Property Dictionary() _
         As IDictionary(Of T, Object)
            Get
                Return mStore
            End Get
        End Property

#Region "Constructors"

        ''' <summary>
        ''' Creates a new set wrapping a basic Dictionary object.
        ''' </summary>
        Public Sub New()
            Me.New(DirectCast(Nothing, IDictionary(Of T, Object)), Nothing, Nothing)
        End Sub

        ''' <summary>
        ''' Creates a new set wrapping a basic dictionary object with the given
        ''' equality comparer.
        ''' </summary>
        ''' <param name="comp">The equality comparer to use for this set.</param>
        Public Sub New(ByVal comp As IEqualityComparer(Of T))
            Me.New(Nothing, comp, Nothing)
        End Sub

        ''' <summary>
        ''' Creates a new set wrapping a basic dictionary object with the given
        ''' equality comparer and initially containing the given items.
        ''' </summary>
        ''' <param name="comp">The equality comparer to use for this set.</param>
        ''' <param name="items">The items to add to the set initiallly</param>
        Public Sub New(
         ByVal comp As IEqualityComparer(Of T), ByVal items As IEnumerable(Of T))
            Me.New(Nothing, comp, items)
        End Sub

        ''' <summary>
        ''' Creates a new set wrapping a basic dictionary object, and initially
        ''' containing the given items
        ''' </summary>
        ''' <param name="items">The items to initialise this set with.</param>
        Public Sub New(ByVal items As IEnumerable(Of T))
            Me.New(Nothing, Nothing, items)
        End Sub

        ''' <summary>
        ''' Creates a new set wrapping the given items.
        ''' </summary>
        ''' <param name="items">The items to wrap</param>
        Public Sub New(ByVal ParamArray items() As T)
            Me.New(Nothing, Nothing, DirectCast(items, IEnumerable(Of T)))
        End Sub

        ''' <summary>
        ''' Creates a new set wrapping the given dictionary
        ''' </summary>
        ''' <param name="dict">The dictionary whose keys should be represent the
        ''' values within this set. If null is passed, a basic (unsorted) dictionary
        ''' will be useed.</param>
        Protected Sub New(ByVal dict As IDictionary(Of T, Object))
            Me.New(dict, Nothing, Nothing)
        End Sub


        ''' <summary>
        ''' Creates a new set wrapping the given dictionary
        ''' </summary>
        ''' <param name="dict">The dictionary whose keys should be represent the
        ''' values within this set. If null is passed, a basic (unsorted) dictionary
        ''' will be useed.</param>
        ''' <param name="items">The initial items to add into this set.</param>
        Protected Sub New(
         ByVal dict As IDictionary(Of T, Object), ByVal items As IEnumerable(Of T))
            Me.New(dict, Nothing, items)
        End Sub

        ''' <summary>
        ''' Creates a new set wrapping the given dictionary, initialised with the
        ''' given collection of items.
        ''' </summary>
        ''' <param name="dict">The dictionary which should be used to store the
        ''' elements in this set as keys.</param>
        ''' <param name="comp">The equality comparer to use - this is ignored if a
        ''' dictionary is passed in <paramref name="dict"/> - ie. it is only relevant
        ''' when creating the default dictionary to use for this set.</param>
        ''' <param name="items">The initial items to add into this set.</param>
        Protected Sub New(
         ByVal dict As IDictionary(Of T, Object),
         ByVal comp As IEqualityComparer(Of T),
         ByVal items As IEnumerable(Of T))

            ' Use the given dictionary if there was one.
            If dict Is Nothing Then _
             dict = New Dictionary(Of T, Object)(comp) : mComparer = comp
            mStore = dict

            ' Anything there? Okay - well, add it then.
            If items IsNot Nothing Then Union(items)

        End Sub

#End Region

#Region "ISet Implementation"

        ''' <summary>
        ''' Adds the given item to this set, not indicating whether it actually
        ''' modified the collection as a result or not.
        ''' <seealso cref="Add">clsSet.Add(T)</seealso>
        ''' </summary>
        ''' <param name="item">The item to add to this set</param>
        Public Overridable Sub JustAdd(ByVal item As T) _
         Implements ICollection(Of T).Add
            Dictionary(item) = Nothing
        End Sub

        ''' <summary>
        ''' Adds the given item to this set, indicating whether it actually modified
        ''' the collection as a result or not.
        ''' </summary>
        ''' <param name="item">The item to add to this set</param>
        ''' <returns>true if the given item was added, causing this set to be
        ''' modified; false if the item already existed within this set.</returns>
        Public Overridable Function Add(ByVal item As T) As Boolean _
         Implements IBPSet(Of T).Add
            If Dictionary.ContainsKey(item) Then Return False
            Dictionary(item) = Nothing
            Return True
        End Function

        ''' <summary>
        ''' Unions this set and the given collection.
        ''' What this means is that after this method has been called, all items in
        ''' both this set and the given collection will exist within this set.
        ''' </summary>
        ''' <param name="items">The collection of items to union this set with.
        ''' </param>
        Public Overridable Sub Union(ByVal items As IEnumerable(Of T)) _
         Implements IBPSet(Of T).Union
            For Each item As T In items
                Add(item)
            Next
        End Sub

        ''' <summary>
        ''' Intersects this set and the given collection.
        ''' In practice, this means that any items in this set that are also in the
        ''' given set are retained within this set. Any items in this set which
        ''' <em>don't</em> exist in the given set are removed.
        ''' </summary>
        ''' <param name="items">The collection to intersect this set with</param>
        Public Overridable Sub Intersect(ByVal items As IEnumerable(Of T)) _
         Implements IBPSet(Of T).Intersect

            Dim matches As New clsSet(Of T)(mComparer)
            For Each item As T In items
                If Me.Contains(item) Then
                    matches.Add(item)
                End If
            Next

            Dim removers As New clsSet(Of T)(mComparer)
            For Each item As T In Me
                If Not matches.Contains(item) Then
                    removers.Add(item)
                End If
            Next

            For Each item As T In removers
                Me.Remove(item)
            Next
        End Sub

        ''' <summary>
        ''' (Symmetric) Differences this set with the given collection. It is the set
        ''' theory equivalent of the 'XOR' (exclusive OR) operation.
        ''' Basically, after this method is called, this set will contain :
        ''' <list>
        ''' <item>all items in this set that were not in the given set, and</item>
        ''' <item>all items in the given set that were not in this set</item>
        ''' </list>
        ''' It will explicitly <em>not</em> include any items that existed both
        ''' within this set and the other set.
        ''' </summary>
        ''' <param name="items">The collection to get the symmetric difference with
        ''' </param>
        Public Overridable Sub Difference(ByVal items As IEnumerable(Of T)) _
         Implements IBPSet(Of T).Difference
            For Each item As T In items
                If Contains(item) Then Remove(item) Else Add(item)
            Next
        End Sub

        ''' <summary>
        ''' Subtracts the given set of items from this set.
        ''' Meaning that after this method has been run, this set will contain all
        ''' the items it had before <em>except</em> those which also existed in the
        ''' given collection.
        ''' </summary>
        ''' <param name="items">The items to subtract from this set</param>
        Public Overridable Sub Subtract(ByVal items As IEnumerable(Of T)) _
         Implements IBPSet(Of T).Subtract
            For Each item As T In items
                If Contains(item) Then Remove(item)
            Next
        End Sub

#End Region

#Region "ICollection Implementation"

        ''' <summary>
        ''' Clears this set
        ''' </summary>
        Public Overridable Sub Clear() Implements ICollection(Of T).Clear
            Dictionary.Clear()
        End Sub

        ''' <summary>
        ''' Checks if this set contains the given item.
        ''' </summary>
        ''' <param name="item">The item to check for within this set</param>
        ''' <returns>True if the given item was found in this set, False otherwise.
        ''' </returns>
        Public Overridable Function Contains(ByVal item As T) As Boolean _
         Implements ICollection(Of T).Contains
            Return Dictionary.ContainsKey(item)
        End Function

        ''' <summary>
        ''' Copies the contents of this set into the given array at the given array
        ''' index.
        ''' </summary>
        ''' <param name="array">The array to copy the items from this set to</param>
        ''' <param name="arrayIndex">The index at which to copy the items</param>
        Public Overridable Sub CopyTo(ByVal array() As T, ByVal arrayIndex As Integer) _
         Implements ICollection(Of T).CopyTo
            Dictionary.Keys.CopyTo(array, arrayIndex)
        End Sub

        ''' <summary>
        ''' The count of items held within this set.
        ''' </summary>
        Public Overridable ReadOnly Property Count() As Integer _
         Implements ICollection(Of T).Count
            Get
                Return Dictionary.Count
            End Get
        End Property

        ''' <summary>
        ''' Flag indicating whether this set is read only or not
        ''' </summary>
        Public Overridable ReadOnly Property IsReadOnly() As Boolean _
         Implements ICollection(Of T).IsReadOnly
            Get
                Return Dictionary.IsReadOnly
            End Get
        End Property

        ''' <summary>
        ''' Removes the given item from this set
        ''' </summary>
        ''' <param name="item">The item to remove</param>
        ''' <returns>true if the given item was found and removed; false if the item
        ''' was not found in this set and thus this set remains unchanged.</returns>
        Public Overridable Function Remove(ByVal item As T) As Boolean _
         Implements ICollection(Of T).Remove
            Return Dictionary.Remove(item)
        End Function

#End Region

#Region "IEnumerable Implementation"

        ''' <summary>
        ''' Gets the enumerator for this set
        ''' </summary>
        ''' <returns>The enumerator which can be used to enumerate over the elements
        ''' in this set.</returns>
        Public Overridable Function GetEnumerator() As IEnumerator(Of T) _
         Implements IEnumerable(Of T).GetEnumerator
            Return Dictionary.Keys.GetEnumerator()
        End Function

        ''' <summary>
        ''' Gets the (untyped) enumerator for this set
        ''' </summary>
        ''' <returns>The enumerator which can be used to enumerate over the elements
        ''' in this set.</returns>
        Public Overridable Function GetEnumerator1() As System.Collections.IEnumerator _
         Implements IEnumerable.GetEnumerator
            Return Dictionary.Keys.GetEnumerator()
        End Function

#End Region

#Region "Object overrides"

        ''' <summary>
        ''' Gets a string representation of this set.
        ''' This is just a brace-enclosed list of the string representations of each
        ''' of the items within the set
        ''' </summary>
        ''' <returns>A string with the contents of this set described in it.
        ''' </returns>
        Public Overrides Function ToString() As String

            Dim sb As New StringBuilder("{")
            For Each item As T In Me
                sb.Append(item).Append(", ")
            Next
            ' Remove the last comma separator + space
            If sb.Length > 1 Then sb.Length -= 2
            Return sb.Append("}").ToString()

        End Function

#End Region

    End Class

End Namespace
