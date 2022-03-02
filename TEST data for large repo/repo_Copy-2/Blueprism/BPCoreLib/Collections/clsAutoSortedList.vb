Imports BluePrism.Server.Domain.Models

Namespace Collections

    ''' <summary>
    ''' Class representing a list which maintains the sort order of its elements when
    ''' they are added to the list.
    ''' The elements should be comparable, either by implementing either
    ''' <see cref="IComparable(Of T)"/> or <see cref="System.IComparable"/> or by
    ''' providing an instance of <see cref="IComparer(Of T)"/> to provide the
    ''' comparison function with which the elements can be sorted.
    ''' </summary>
    ''' <typeparam name="T">The type of element held within this list</typeparam>
    ''' <remarks>Note that most of the methods on this class assume that this list
    ''' remains sorted - ie. that the elements in the list do not change value in
    ''' such a way that their position in the list would be different if added again;
    ''' If any of the elements change value in such a way and this list is not
    ''' resorted, the list will become unstable - eg. Contains() may return false
    ''' when the element is within the list but sorted on an old value; Add() may
    ''' add an element to an entirely inappropriate location in the list.</remarks>
    <Serializable, DebuggerDisplay("Count: {Count}")>
    Public Class clsAutoSortedList(Of T) : Implements IList(Of T)

#Region " Member Variables "

        ' The comparer used to determine the order of the list
        Private mComp As IComparer(Of T)

        ' The list in which the elements are stored.
        Private mList As List(Of T) = New List(Of T)

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new empty list
        ''' </summary>
        Public Sub New()
            Me.New(Comparer(Of T).Default, Nothing)
        End Sub

        ''' <summary>
        ''' Creates a new empty list, whose elements will be sorted according to the
        ''' given comparer
        ''' </summary>
        ''' <param name="comp">The comparer to use to compare elements within the
        ''' list to provide the sort order of the list.</param>
        Public Sub New(ByVal comp As IComparer(Of T))
            Me.New(comp, Nothing)
        End Sub

        ''' <summary>
        ''' Creates a new sorted list containing the elements of the given collection
        ''' </summary>
        ''' <param name="elements">The enumerable of elements which should provide
        ''' the elements to populate this list with.</param>
        Public Sub New(ByVal elements As IEnumerable(Of T))
            Me.New(Comparer(Of T).Default, elements)
        End Sub

        ''' <summary>
        ''' Creates a new sorted list using the specified comparer 
        ''' </summary>
        ''' <param name="comp">The comparer with which the given elements should be
        ''' compared.</param>
        ''' <param name="elements">The initial elements to set into this list.
        ''' </param>
        ''' <exception cref="ArgumentNullException">If the given comparer is null.
        ''' </exception>
        Public Sub New(
         ByVal comp As IComparer(Of T), ByVal elements As IEnumerable(Of T))
            If comp Is Nothing Then Throw New ArgumentNullException(NameOf(comp))
            mComp = comp
            If elements IsNot Nothing Then AddRange(elements)
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' The number of elements held in this list.
        ''' </summary>
        Public ReadOnly Property Count() As Integer _
         Implements ICollection(Of T).Count
            Get
                Return mList.Count
            End Get
        End Property

        ''' <summary>
        ''' Gets a flag indicating if this list is read only. It is not.
        ''' </summary>
        Public ReadOnly Property IsReadOnly() As Boolean _
         Implements ICollection(Of T).IsReadOnly
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the element in this list at the specified index.
        ''' Note that when setting an element, this property makes a best effort to
        ''' maintain the sort order of the list, throwing an exception if 
        ''' </summary>
        ''' <param name="index">The index at which the item should be set / got from.
        ''' </param>
        ''' <exception cref="InvalidValueException">If the setting of the given item
        ''' at the specified index would break the sort order of the list.
        ''' Note that if the list's sort order has been corrupted already (by an item
        ''' changing its value), this exception may also be thrown</exception>
        Default Public Property Item(ByVal index As Integer) As T _
         Implements IList(Of T).Item
            Get
                Return mList(index)
            End Get
            ' Actually, this 'protected' is of limited use - if anyone is using it
            ' as an IList<T> they can set value with that
            Protected Set(ByVal value As T)
                ' Check either side of the index to ensure that it's in the right
                ' place for this item's value.
                EnsureSetItemValid(index, value, False)
                ' Otherwise, we're good to go - mList can handle the bound checks
                ' and appropriate exceptions for them
                mList(index) = value
            End Set
        End Property

#End Region

#Region " Search / Insert Point Methods "

        ''' <summary>
        ''' Performs a binary search of this list for the given item
        ''' </summary>
        ''' <param name="item">The item to search for</param>
        ''' <returns>The index of the given item within the list if is within the
        ''' list; if it is not, this returns <c>(-(insertionPoint - 1))</c> where
        ''' <c>insertionPoint</c> represents the point at which the item should be
        ''' inserted into the list to maintain the sorting of the list</returns>
        ''' <remarks>This idea came from the java implementation of
        ''' Arrays.binarySearch via StackOverflow : http://tinyurl.com/d7nul8y
        ''' </remarks>
        Public Function BinarySearch(ByVal item As T) As Integer
            Return BinarySearch(item, 0, Count)
        End Function

        ''' <summary>
        ''' Performs a binary search of this list for the given item with the
        ''' specified low and high bounds - the high bounds should be 1 beyond the
        ''' upper limit index to search - eg. <c>BinarySearch(item, 0, Me.Count)</c>
        ''' will perform a search in the entire list.
        ''' </summary>
        ''' <param name="item">The item to search for</param>
        ''' <param name="lowBound">The low bound of the list to start searching from
        ''' </param>
        ''' <param name="hiBound">The high bounds of the list to search up to and not
        ''' including - eg. <see cref="Count"/> would search to the end of the list,
        ''' <c>5</c> would search up to index 4 of the list.</param>
        ''' <returns>The index of the given item within the list if is within the
        ''' list; if it is not, this returns <c>(-(insertionPoint - 1))</c> where
        ''' <c>insertionPoint</c> represents the point at which the item should be
        ''' inserted into the list to maintain the sorting of the list</returns>
        ''' <remarks>This idea came from the java implementation of
        ''' Arrays.binarySearch via StackOverflow : http://tinyurl.com/d7nul8y
        ''' </remarks>
        Protected Function BinarySearch(ByVal item As T,
         ByVal lowBound As Integer, ByVal hiBound As Integer) As Integer
            Dim lowInd As Integer = lowBound
            Dim hiInd As Integer = hiBound - 1
            While lowInd <= hiInd
                Dim midInd As Integer = (lowInd + hiInd) \ 2
                Dim compValue As Integer = mComp.Compare(Me(midInd), item)
                If compValue < 0 Then
                    lowInd = midInd + 1
                ElseIf compValue > 0 Then
                    hiInd = midInd - 1
                Else
                    Return midInd
                End If
            End While
            ' No instance of item found, return a negative with 1 less than the
            ' low point - this means that it wasn't found (-ve value), and provides
            ' a hint for a suitable insertion point in the array (-result - 1)
            ' if that is required.
            Return -lowInd - 1
        End Function

        ''' <summary>
        ''' Finds the insert point for the given item. This may be an index at which
        ''' there is currently an element of the same value, or it could be the
        ''' 'length' of the list indicating that the item should be appended to the
        ''' end of the list.
        ''' </summary>
        ''' <param name="item">The item for which the insertion point is required.
        ''' </param>
        ''' <returns>The appropriate insertion point for the given item</returns>
        Protected Function FindInsertPoint(ByVal item As T) As Integer
            Dim index As Integer = BinarySearch(item)

            ' If we've found the item, insert at the same point - the order of the
            ' actual objects is arbitrary if their sortable value is the same
            If index >= 0 Then Return index

            ' Otherwise, we have a hint at the correct insertion point for the item
            Return -index - 1
        End Function

#End Region

#Region " Interface Implementation Methods "

        ''' <summary>
        ''' Adds the given item to this list, ensuring that it remains sorted
        ''' </summary>
        ''' <param name="item">The item to add</param>
        Public Sub Add(ByVal item As T) Implements ICollection(Of T).Add
            mList.Insert(FindInsertPoint(item), item)
        End Sub

        ''' <summary>
        ''' Adds all of the items in the given enumerable to this list, ensuring that
        ''' the sort order of the list is maintained
        ''' </summary>
        ''' <param name="items">The items to add to this list</param>
        Public Sub AddRange(ByVal items As IEnumerable(Of T))

            ' Build capacity first for speed, if we can...
            Dim coll As ICollection(Of T) = TryCast(items, ICollection(Of T))
            If coll IsNot Nothing Then
                Dim reqdCapacity As Integer = Count + coll.Count
                ' Ensure capacity and add 10% on top for further growth
                If mList.Capacity < reqdCapacity Then _
                 mList.Capacity = reqdCapacity + ((reqdCapacity) \ 10)
            End If

            ' There's no shortcut way of doing this really; we have to re-search
            ' the list each time for each item we add.
            For Each item As T In items
                Add(item)
            Next
        End Sub

        ''' <summary>
        ''' Clears this list of all items
        ''' </summary>
        Public Sub Clear() Implements ICollection(Of T).Clear
            mList.Clear()
        End Sub

        ''' <summary>
        ''' Checks if this list contains the given item
        ''' </summary>
        ''' <param name="item">The item to search for</param>
        ''' <returns>True if the given item is within this list.</returns>
        Public Function Contains(ByVal item As T) As Boolean _
         Implements ICollection(Of T).Contains
            Return BinarySearch(item) >= 0
        End Function

        ''' <summary>
        ''' Copies the contents of this list to the given array at the specified
        ''' index.
        ''' </summary>
        ''' <param name="array">The array to which this list should be copied</param>
        ''' <param name="arrayIndex">The index at the array to which the list should
        ''' be copied.</param>
        Public Sub CopyTo(ByVal array() As T, ByVal arrayIndex As Integer) _
         Implements ICollection(Of T).CopyTo
            mList.CopyTo(array, arrayIndex)
        End Sub

        ''' <summary>
        ''' Gets the index of the given item within this list, or -1 if the element
        ''' was not found.
        ''' </summary>
        ''' <param name="item">The item to search for within this list.</param>
        ''' <returns>The index at which the given item exists within this list;
        ''' -1 if the item is not in the list.</returns>
        Public Function IndexOf(ByVal item As T) As Integer Implements IList(Of T).IndexOf
            Dim ind As Integer = BinarySearch(item)
            If ind >= 0 Then Return ind Else Return -1
        End Function

        ''' <summary>
        ''' Inserts the given item to the given index.
        ''' </summary>
        ''' <param name="index">The index to which the item should be inserted
        ''' </param>
        ''' <param name="item">The item to insert</param>
        ''' <exception cref="InvalidValueException">If the inserting of the given
        ''' item at the specified index would break the sort order of the list. Note
        ''' that if the list's sort order has been corrupted already (by an item
        ''' changing its value), this exception may also be thrown</exception>
        Private Sub Insert(ByVal index As Integer, ByVal item As T) _
         Implements IList(Of T).Insert
            ' I don't like having the Insert() method here at all, but if it is, we
            ' might as well sanity check it first.
            EnsureSetItemValid(index, item, True)
            mList.Insert(index, item)
        End Sub

        ''' <summary>
        ''' Removes the item at the given index.
        ''' </summary>
        ''' <param name="index">The index at which the item should be removed.
        ''' </param>
        Public Sub RemoveAt(ByVal index As Integer) Implements IList(Of T).RemoveAt
            mList.RemoveAt(index)
        End Sub

        ''' <summary>
        ''' Removes the first instance of the given item from the list, returning a
        ''' flag indicating if an item was removed or not.
        ''' </summary>
        ''' <param name="item">The item to remove from this list, if present.</param>
        ''' <returns>True if an instance of the given item was found in the list and
        ''' removed; False if the item was not found in the list.</returns>
        Public Function Remove(ByVal item As T) As Boolean _
         Implements ICollection(Of T).Remove
            Return mList.Remove(item)
        End Function

        ''' <summary>
        ''' Gets an enumerator over the elements of this list.
        ''' </summary>
        ''' <returns>An enumerator over the elements of this list</returns>
        Public Function GetEnumerator() As IEnumerator(Of T) _
         Implements IEnumerable(Of T).GetEnumerator
            Return mList.GetEnumerator()
        End Function

        ''' <summary>
        ''' Gets an enumerator over the elements of this list.
        ''' </summary>
        ''' <returns>An enumerator over the elements of this list</returns>
        Private Function GetEnumerator1() As System.Collections.IEnumerator _
         Implements System.Collections.IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function

#End Region

#Region " Other Methods "

        ''' <summary>
        ''' Ensures that the setting of the given item at the specified index is
        ''' valid - ie. that it will not break the sort order of the list.
        ''' </summary>
        ''' <param name="index">The index at which the item is set to go into the
        ''' list.</param>
        ''' <param name="item">The item being set in the list</param>
        ''' <param name="inserting">True to indicate that the item is being inserted,
        ''' ie. that the item currently at <paramref name="index"/> will be pushed to
        ''' the next index and the given item will take its place; False to indicate
        ''' that the item is being set, ie. that the item currently at
        ''' <paramref name="index"/> will be overwritten by the given item.</param>
        ''' <exception cref="InvalidValueException">If the setting/inserting of the
        ''' given item at the specified index would break the sort order of the list.
        ''' Note that if the list's sort order has been corrupted already (by an item
        ''' changing its value), this exception may also be thrown</exception>
        Private Sub EnsureSetItemValid(
         ByVal index As Integer, ByVal item As T, ByVal inserting As Boolean)

            ' Check below the index
            Dim belowInd As Integer = index - 1

            ' Check 1 above the index if setting the item in place; check the actual
            ' index if inserting the item.
            Dim aboveInd As Integer = CInt(IIf(inserting, index, index + 1))

            ' If [index - 1] > value then order is wrong.
            If belowInd > 0 AndAlso mComp.Compare(mList(belowInd), item) > 0 Then
                Throw New InvalidValueException(
                 CStr(IIf(inserting, My.Resources.clsAutoSortedList_Inserting0AtIndex1WouldCauseTheListToGoOutOfOrder,
                          My.Resources.clsAutoSortedList_Setting0AtIndex1WouldCauseTheListToGoOutOfOrder)), item, index)
            End If

            ' if value > [index + 1] then order is wrong
            If aboveInd < Count - 1 AndAlso
             mComp.Compare(item, mList(aboveInd)) > 0 Then
                Throw New InvalidValueException(
                 CStr(IIf(inserting, My.Resources.clsAutoSortedList_Inserting0AtIndex1WouldCauseTheListToGoOutOfOrder,
                          My.Resources.clsAutoSortedList_Setting0AtIndex1WouldCauseTheListToGoOutOfOrder)), item, index)
            End If

        End Sub

        ''' <summary>
        ''' Gets a string representation of this sorted list.
        ''' </summary>
        ''' <returns>A string representation of this list.</returns>
        Public Overrides Function ToString() As String
            Return CollectionUtil.ToString(mList)
        End Function

#End Region

    End Class

End Namespace
