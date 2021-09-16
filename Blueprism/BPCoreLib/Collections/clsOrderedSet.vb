Namespace Collections

    ''' Project  : BPCoreLib
    ''' Class    : clsOrderedSet
    ''' <summary>
    ''' <para>
    ''' Set class which retains the order of the elements as they are added to the set.
    ''' This is backed by an OrderedDictionary(Of T,Object), so the order of the objects
    ''' will follow the rules detailed there.
    ''' </para><para>
    ''' <strong>Note: </strong> This set does not hold its elements in a sorted order as
    ''' such - it retains the order that elements were added. See OrderedDictionary for
    ''' the implementation behind it - its ordering of keys is identical to this class's
    ''' ordering of elements.
    ''' </para>
    ''' <seealso cref="clsSet(Of T)">clsSet</seealso>
    ''' </summary>
    ''' <typeparam name="T">The type of elements that this set contains.</typeparam>
    <Serializable, DebuggerDisplay("Count: {Count}")> _
    Public Class clsOrderedSet(Of T)
        Inherits clsSet(Of T)

        ''' <summary>
        ''' Creates a new empty ordered set
        ''' </summary>
        Public Sub New()
            Me.New(Nothing)
        End Sub

        ''' <summary>
        ''' Creates a new ordered set initialised with the given items.
        ''' </summary>
        ''' <param name="items">The items which this set should contain after it has been
        ''' initialised or null if an empty set is required.</param>
        Public Sub New(ByVal items As IEnumerable(Of T))
            MyBase.New(New clsOrderedDictionary(Of T, Object), items)
        End Sub

    End Class

End Namespace
