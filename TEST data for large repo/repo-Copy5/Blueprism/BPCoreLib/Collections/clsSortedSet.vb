Imports System.Runtime.Serialization


Namespace Collections

    ''' Project  : BPCoreLib
    ''' Class    : clsSortedSet
    ''' <summary>
    ''' <para>
    ''' Set class which retains the values it holds in order. By default this will use
    ''' their natural order, but it can be overridden with a comparer.
    ''' This is achieved by wrapping a SortedDictionary rather than a standard
    ''' implementation.
    ''' </para>
    ''' <seealso cref="clsSet(Of T)">clsSet</seealso>
    ''' </summary>
    ''' <typeparam name="T">The type of elements that this set contains. If not using
    ''' an 'IComparer' to compare values, this must implement the IComparable
    ''' interface to allow natural order of the objects to be calculated.</typeparam>
    <Serializable, DebuggerDisplay("Count: {Count}"), DataContract(Namespace:="bp")>
    Public Class clsSortedSet(Of T)
        Inherits clsSet(Of T)

        ''' <summary>
        ''' Creates a new empty sorted set which uses the natural order of the contained
        ''' objects (provided by their IComparable implementation) to sort the elements.
        ''' </summary>
        ''' <exception cref="ApplicationException">If the type of this sorted set cannot
        ''' be assigned to IComparable or IComparable(Of T).</exception>
        Public Sub New()
            Me.New(Nothing, Nothing)
        End Sub

        ''' <summary>
        ''' Creates a new sorted set which uses the natural order of the contained
        ''' objects (provided by their IComparable implementation) to sort the elements.
        ''' The set will be initialised with the given collection of items.
        ''' </summary>
        ''' <param name="items">The items to add to this set on creation, or null if an
        ''' empty set is required.</param>
        ''' <exception cref="ApplicationException">If the type of this sorted set cannot
        ''' be assigned to IComparable or IComparable(Of T).</exception>
        Public Sub New(ByVal items As ICollection(Of T))
            Me.New(Nothing, items)
        End Sub

        ''' <summary>
        ''' <para>
        ''' Creates a new sorted set based on the given comparer. If no comparer is
        ''' given, this will use the natural order of the objects to determine the order
        ''' within this set.
        ''' </para><para>
        ''' Note that if no comparer is given, the type 'T' must be assignable to
        ''' IComparable(or IComparable(Of T)) in order to determine each objects position
        ''' within the set. If the type cannot be cast as an IComparer, an exception is
        ''' thrown at this point.
        ''' </para>
        ''' </summary>
        ''' <param name="comp">The comparer to use to determine the order of objects
        ''' within the set, or nothing to indicate the natural order of the objects
        ''' (as determined by their IComparable implementation) should be used.</param>
        ''' <exception cref="ApplicationException">If no comparer was given and the type 
        ''' of this sorted set cannot be assigned to IComparable or IComparable(Of T).
        ''' </exception>
        Public Sub New(ByVal comp As IComparer(Of T))
            Me.New(comp, Nothing)
        End Sub

        ''' <summary>
        ''' <para>
        ''' Creates a new sorted set based on the given comparer. If no comparer is
        ''' given, this will use the natural order of the objects to determine the order
        ''' within this set.
        ''' This will be initialised with the given collection of items.
        ''' </para><para>
        ''' Note that if no comparer is given, the type 'T' must be assignable to
        ''' IComparable(or IComparable(Of T)) in order to determine each objects position
        ''' within the set. If the type cannot be cast as an IComparer, an exception is
        ''' thrown at this point.
        ''' </para>
        ''' </summary>
        ''' <param name="comp">The comparer to use to determine the order of objects
        ''' within the set, or nothing to indicate the natural order of the objects
        ''' (as determined by their IComparable implementation) should be used.</param>
        ''' <param name="items">The items which should be initialised into this set on
        ''' creation, or null if an empty set is required.</param>
        ''' <exception cref="ApplicationException">If no comparer was given and the type 
        ''' of this sorted set cannot be assigned to IComparable or IComparable(Of T).
        ''' </exception>
        Public Sub New(ByVal comp As IComparer(Of T), ByVal items As IEnumerable(Of T))
            ' Create the set wrapping a sorted dictionary
            MyBase.New(New SortedDictionary(Of T, Object)(comp), items)

            ' If we have to use natural order, check that we can assign to an IComparable
            ' Fail otherwise.
            ' Note that if we didn't do this here, SortedDictionary would do the same
            ' thing the first time it tried to compare two objects... it makes more sense
            ' to highlight the problem at creation time rather than 'at runtime when a
            ' second object is added to the set'... which could easily get missed when
            ' developing and leave a fatal error in released code.
            If comp Is Nothing Then
                If Not GetType(IComparable).IsAssignableFrom(GetType(T)) AndAlso _
                 Not GetType(IComparable(Of T)).IsAssignableFrom(GetType(T)) Then
                    Throw New InvalidOperationException(
                     String.Format(My.Resources.clsSortedSet_CannotCreateASortedSetForANonComparableTypeIfNotSupplyingASeparateComparerTypeIs0, GetType(T).ToString()))
                End If
            End If
        End Sub

    End Class

End Namespace
