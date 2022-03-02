Namespace Collections

    ''' Project  : BPCoreLib
    ''' Class    : clsCovariantSet
    ''' <summary>
    ''' A class to enable covariance in a set.
    ''' </summary>
    ''' <typeparam name="S">The type of set required.</typeparam>
    ''' <typeparam name="T">The type of set currently held.</typeparam>
    <Serializable, DebuggerDisplay("Count: {Count}")> _
    Public Class clsCovariantSet(Of S, T As {S})
        Inherits clsCovariantCollection(Of S, T)
        Implements IBPSet(Of S)

        ''' <summary>
        ''' Creates a new covariant set which wrapps the given set.
        ''' </summary>
        ''' <param name="tSet">The set whose data should be exposed as a set
        ''' of type 'S'</param>
        Public Sub New(ByVal tSet As IBPSet(Of T))
            MyBase.New(tSet)
        End Sub

        ''' <summary>
        ''' Gets the wrapped collection as a set of type T.
        ''' </summary>
        Protected ReadOnly Property WrappedSet() As IBPSet(Of T)
            Get
                Return DirectCast(WrappedCollection, IBPSet(Of T))
            End Get
        End Property

        ''' <summary>
        ''' Adds the given element to the wrapped set, returning whether
        ''' it was unique and thus added, or already existed in the set
        ''' and thus was not added.
        ''' </summary>
        ''' <param name="element">The element to add.</param>
        ''' <returns>True if the set did not already contain the given
        ''' element and it has been added; false if the set already contained
        ''' the given element and it has therefore not been added.</returns>
        Public Overloads Function Add(ByVal element As S) As Boolean Implements IBPSet(Of S).Add
            Return WrappedSet.Add(DirectCast(element, T))
        End Function

        ''' <summary>
        ''' Unfortunately, this is a 'contravariant' conversion so it can't be
        ''' done simply, unless a 'ContravariantEnumerable' is written...
        ''' </summary>
        ''' <param name="items">The items to cast into type T's</param>
        ''' <returns>The given enumerable *copied* into a new enumerable of
        ''' type T. Note that unlike these clsCovariant classes, this is not
        ''' wrapped - it is a shallow copy of the given enumerable.
        ''' Since these enumerables exist only to manipulate this set, and the
        ''' reference isn't held past the relevant public method exiting, this
        ''' should not be a problem.</returns>
        Private Function Cast(ByVal items As IEnumerable(Of S)) As IEnumerable(Of T)
            Dim lst As New List(Of T)()
            For Each item As T In items
                lst.Add(item)
            Next
            Return lst
        End Function

        ''' <summary>
        ''' Causes this set to contain those elements which are either in this
        ''' set but not in the given set, or those that are in the given set but
        ''' not in this set - this is called 'symmetric difference' in set theory,
        ''' but acts similarly to XOR in computer terminology
        ''' </summary>
        ''' <param name="items">The items with which this set should perform a
        ''' symmetric difference operation.</param>
        Public Sub Difference(ByVal items As IEnumerable(Of S)) Implements IBPSet(Of S).Difference
            WrappedSet.Difference(Cast(items))
        End Sub

        ''' <summary>
        ''' Causes this set to contain only those elements which occur in both
        ''' this set and the given enumerable.
        ''' </summary>
        ''' <param name="items">The items with which this set should perform an
        ''' intersect operation.</param>
        Public Sub Intersect(ByVal items As IEnumerable(Of S)) Implements IBPSet(Of S).Intersect
            WrappedSet.Intersect(Cast(items))
        End Sub

        ''' <summary>
        ''' Causes this set to contain only those elements which occur in this set
        ''' but not in the given enumerable.
        ''' </summary>
        ''' <param name="items">The items which should be removed from this set.
        ''' </param>
        Public Sub Subtract(ByVal items As IEnumerable(Of S)) Implements IBPSet(Of S).Subtract
            WrappedSet.Subtract(Cast(items))
        End Sub

        ''' <summary>
        ''' Causes this set to contain an instance of each element which exists in
        ''' this set, or in the given enumerable, or both.
        ''' </summary>
        ''' <param name="items">The items to be appended to this set.</param>
        Public Sub Union(ByVal items As IEnumerable(Of S)) Implements IBPSet(Of S).Union
            WrappedSet.Union(Cast(items))
        End Sub
    End Class

End Namespace
