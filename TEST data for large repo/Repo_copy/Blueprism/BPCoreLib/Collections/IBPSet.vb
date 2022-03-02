
Namespace Collections

    ''' Project  : BPCoreLib
    ''' Class    : IBPSet
    ''' <summary>
    ''' Interface describing a set of a specific type.
    ''' The primary definition of a set within this context is a collection 
    ''' of unique elements.
    ''' Implementations of this class may rely strongly on useful overrides
    ''' of GetHashCode() and Equals() on any contained elements, in order 
    ''' to optimise checking for uniqueness within the set.
    ''' </summary>
    ''' <typeparam name="T">The type that this set collects together</typeparam>
    Public Interface IBPSet(Of T)
        Inherits ICollection(Of T)

        ''' <summary>
        ''' Adds the given element to the set, and returns a boolean
        ''' to indicating whether the set has changed as a result of
        ''' the add.
        ''' In other words, this verifies that the given element did
        ''' not already exist within the set before the 'Add' method
        ''' was called.
        ''' </summary>
        ''' <param name="element">The element to add to this set.
        ''' </param>
        ''' <returns>True if the given element did not already exist
        ''' within the set; False if the provided element is already
        ''' in the set and therefore it was not 'added'.</returns>
        ''' <exception cref="NotSupportedException">If this set does
        ''' not allow modification.</exception>
        ''' <remarks> Note that although this function overloads the
        ''' existing Add method in ICollection(Of T), it is expected
        ''' to perform exactly the same action - the only difference
        ''' is in the return type. ISet's Add() returns a boolean to
        ''' indicate that the element was added. ICollection's Add()
        ''' returns nothing.
        ''' </remarks>
        Overloads Function Add(ByVal element As T) As Boolean

        ''' <summary>
        ''' Union this set with the given collection. When this method
        ''' returns, this set will contain a union of all the items it
        ''' contained previously, as well as all of the items that are
        ''' in the specified collection.
        ''' Note that if multiple instances would occur, this set will
        ''' contain only single instances of each element. ie. all the
        ''' elements in this set will appear at most once.
        ''' </summary>
        ''' <param name="items">The items to union with this set.
        ''' </param>
        Sub Union(ByVal items As IEnumerable(Of T))

        ''' <summary>
        ''' Intersects this set with the given collection.
        ''' When this method returns, this set will contain all the items
        ''' that it contained previously, <em>if and only if</em> they
        ''' were also contained in the provided collection.
        ''' </summary>
        ''' <param name="items">The items to intersect with this set.
        ''' </param>
        ''' <remarks></remarks>
        Sub Intersect(ByVal items As IEnumerable(Of T))

        ''' <summary>
        ''' Sometimes called 'symmetric difference' in set theory, this
        ''' causes the set to contain the different elements between
        ''' itself and the given collection.
        ''' When this method returns, this set will contain all the items
        ''' that it contained previously which were not contained in the
        ''' given collection <em>and</em> all the items in the given
        ''' collection which were not contained in this set.
        ''' </summary>
        ''' <param name="items">The items to difference with this set
        ''' </param>
        Sub Difference(ByVal items As IEnumerable(Of T))

        ''' <summary>
        ''' Subtracts the given collection of items from this set.
        ''' When this method returns, this set will contain all the items
        ''' that it contained previously <em>except</em> those which also
        ''' appeared in the given collection.
        ''' </summary>
        ''' <param name="items">The items to subtract from this set.</param>
        Sub Subtract(ByVal items As IEnumerable(Of T))

    End Interface

End Namespace
