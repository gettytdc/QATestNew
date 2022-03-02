Namespace Collections

    <Serializable, DebuggerDisplay("Count: {Count}")> _
    Public Class clsEventFiringList(Of T) : Inherits clsEventFiringCollection(Of T)
        Implements IList(Of T)

#Region " Event Declarations "

        ''' <summary>
        ''' Event fired when an item is set using the default property in this
        ''' list implementation.
        ''' </summary>
        ''' <param name="sender">The source list on which the item was set.
        ''' </param>
        ''' <param name="index">The index within the list that the item was set.
        ''' </param>
        ''' <param name="prevValue">The previous value corresponding to the key that
        ''' was set, if there was one. Null otherwise.</param>
        ''' <param name="newValue">The new value at the specified index of the list.
        ''' </param>
        Public Event ListItemSet(ByVal sender As IList(Of T), _
         ByVal index As Integer, ByVal prevValue As T, ByVal newValue As T)

        ''' <summary>
        ''' Event fired when an item is added into this list.
        ''' </summary>
        ''' <param name="sender">The source list on which the item was set.
        ''' </param>
        ''' <param name="index">The index within the list that the item was added.
        ''' </param>
        ''' <param name="newValue">The new value which has been added to this list.
        ''' </param>
        Public Event ListItemAdded(ByVal sender As IList(Of T), _
         ByVal index As Integer, ByVal newValue As T)


        ''' <summary>
        ''' Event fired when an item is removed from this list.
        ''' </summary>
        ''' <param name="sender">The list which is sending the event</param>
        ''' <param name="index">The index at which the item was before removal
        ''' </param>
        ''' <param name="prevValue">The value that has been removed from the list.
        ''' </param>
        ''' <remarks>Note that this event and the
        ''' <see cref="clsEventFiringCollection(Of T).ItemRemoved"/> event are fired
        ''' when an item is removed from an event firing list.
        ''' </remarks>
        Public Event ListItemRemoved( _
         ByVal sender As IList(Of T), ByVal index As Integer, ByVal prevValue As T)


#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new event firing list around the given list
        ''' </summary>
        ''' <param name="lst">The list to wrap inside this list</param>
        Public Sub New(ByVal lst As IList(Of T))
            MyBase.New(lst)
        End Sub
#End Region

#Region " Properties "

        ''' <summary>
        ''' The inner list that this object has added events to
        ''' </summary>
        Public ReadOnly Property InnerList() As IList(Of T)
            Get
                Return DirectCast(InnerCollection, IList(Of T))
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the item in this list at the specified index.
        ''' </summary>
        ''' <param name="index"></param>
        Default Public Property Item(ByVal index As Integer) As T _
         Implements IList(Of T).Item
            Get
                Return InnerList(index)
            End Get
            Set(ByVal value As T)
                Dim old As T = Nothing
                If index >= 0 AndAlso index < Count - 1 Then old = InnerList(index)
                InnerList(index) = value
                If Not Object.Equals(old, value) Then _
                 OnListItemSet(index, old, value)

            End Set
        End Property

#End Region

#Region " Event Raising Methods "

        ''' <summary>
        ''' Raises the <see cref="ListItemSet"/> event
        ''' </summary>
        ''' <param name="index">The index within the list that the item was set.
        ''' </param>
        ''' <param name="prevValue">The previous value corresponding to the key that
        ''' was set, if there was one. Null otherwise.</param>
        ''' <param name="newValue">The new value at the specified index of the list.
        ''' </param>
        Protected Sub OnListItemSet( _
         ByVal index As Integer, ByVal prevValue As T, ByVal newValue As T)
            RaiseEvent ListItemSet(Me, index, prevValue, newValue)
        End Sub

        ''' <summary>
        ''' Raises the <see cref="ListItemAdded"/> event
        ''' </summary>
        ''' <param name="index">The index within the list that the item was added.
        ''' </param>
        ''' <param name="newValue">The new value which has been added to this list.
        ''' </param>
        Protected Sub OnListItemAdded(ByVal index As Integer, ByVal newValue As T)
            RaiseEvent ListItemAdded(Me, index, newValue)
        End Sub

        ''' <summary>
        ''' Raises the <see cref="ListItemRemoved"/> event
        ''' </summary>
        ''' <param name="index">The index at which the item was before removal
        ''' </param>
        ''' <param name="prevValue">The value that has been removed from the list.
        ''' </param>
        Protected Overridable Sub OnListItemRemoved( _
         ByVal index As Integer, ByVal prevValue As T)
            RaiseEvent ListItemRemoved(Me, index, prevValue)
        End Sub

#End Region

#Region " Other Methods "

        ''' <summary>
        ''' Gets the first index within the list that the given item occurs
        ''' </summary>
        ''' <param name="item">The item to search the list for</param>
        ''' <returns>The first index in the list at which the specified item was
        ''' found, or -1 if the item was not found in the list.</returns>
        Public Function IndexOf(ByVal item As T) As Integer _
         Implements IList(Of T).IndexOf
            Return InnerList.IndexOf(item)
        End Function

        ''' <summary>
        ''' Inserts the given item into the list at the specified index.
        ''' </summary>
        ''' <param name="index">The index at which the item should be inserted
        ''' </param>
        ''' <param name="item">The item to add to the list</param>
        Public Overridable Sub Insert(ByVal index As Integer, ByVal item As T) _
         Implements IList(Of T).Insert
            InnerList.Insert(index, item)
            OnItemAdded(item)
            OnListItemAdded(index, item)
        End Sub

        ''' <summary>
        ''' Removes the item at the specified index from this list
        ''' </summary>
        ''' <param name="index">The index at which the item should be removed</param>
        Public Overridable Sub RemoveAt(ByVal index As Integer) _
         Implements IList(Of T).RemoveAt
            Dim val As T = Nothing
            If index >= 0 AndAlso index < Count Then val = InnerList(index)
            InnerList.RemoveAt(index)
            OnItemRemoved(val)
            OnListItemRemoved(index, val)
        End Sub

        ''' <summary>
        ''' Adds the given item to the underlying collection and raises an ItemAdded
        ''' event and a ListItemAdded event.
        ''' </summary>
        ''' <param name="item">The item to be added.</param>
        Public Overrides Sub Add(ByVal item As T)
            MyBase.Add(item)
            OnListItemAdded(Count - 1, item)
        End Sub

        ''' <summary>
        ''' Removes the first found instance of the specified item from the
        ''' underlying list and raises ItemRemoved and ListItemRemoved events for
        ''' them.
        ''' </summary>
        ''' <param name="item">The item to remove</param>
        ''' <returns>True if the item was found in the list and removed; False if it
        ''' was not found and, thus, not removed</returns>
        Public Overrides Function Remove(ByVal item As T) As Boolean
            Dim index As Integer = InnerList.IndexOf(item)
            If index < 0 Then Return False
            InnerList.RemoveAt(index)
            OnItemRemoved(item)
            OnListItemRemoved(index, item)
            Return True
        End Function

#End Region

    End Class


End Namespace
