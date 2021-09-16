Imports System.Runtime.Serialization

<TypeConverter(GetType(System.Windows.Forms.ListViewItemConverter))>
Friend Class ctlEditableListRow
    Inherits Control
    Implements ISerializable

    ''' <summary>
    ''' Holds the items of the row
    ''' </summary>
    Private WithEvents mobjItems As clsCollectionWithEvents(Of ctlEditableListItem)

    ''' <summary>
    ''' Indicates whether the row is selected
    ''' </summary>
    Private mbSelected As Boolean

    ''' <summary>
    ''' Indicates that a new row was selected.
    ''' </summary>
    ''' <param name="sender">The row which has been selected.</param>
    ''' <param name="Index">The index of the column which has
    ''' focus.</param>
    Public Event Selected(ByVal sender As ctlEditableListRow, ByVal Index As Integer)

    Public Event DeSelected(ByVal sender As Object, ByVal e As EventArgs)

    Public Event ListRowKeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs)

    Public Event EditControlKeyPreview(ByVal sender As Object, ByVal e As PreviewKeyDownEventArgs)




    ''' <summary>
    ''' Private member to store public property OwningListView()
    ''' </summary>
    Private mOwningListView As ctlListView
    ''' <summary>
    ''' The listview owning this row.
    ''' </summary>
    Public Property OwningListView() As ctlListView
        Get
            Return mOwningListView
        End Get
        Set(ByVal value As ctlListView)
            mOwningListView = value
        End Set
    End Property

    ''' <summary>
    ''' Private member to store public property RowAdded()
    ''' </summary>
    ''' <remarks></remarks>
    Private mRowAdded As Boolean = False

    ''' <summary>
    ''' Indicates whether this row belongs to a listview.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property HasOwner() As Boolean
        Get
            Return (Me.OwningListView IsNot Nothing)
        End Get
    End Property

    ''' <summary>
    ''' The default constructor ensures that a collection is prepared
    ''' to hold the items
    ''' </summary>
    Public Sub New()
        Me.Height = 25
        mobjItems = New clsCollectionWithEvents(Of ctlEditableListItem)
    End Sub

    ''' <summary>
    ''' Creates a row.
    ''' </summary>
    ''' <param name="ListView">The listview owning this row.</param>
    Public Sub New(ByVal ListView As ctlListView)
        Me.New()
        Me.mOwningListView = ListView
    End Sub

    ''' <summary>
    ''' Private member to store public property HighlightedBackColour()
    ''' </summary>
    Private mHighlightedBackColour As Color
    ''' <summary>
    ''' The colour used to highlight this row when it is selected.
    ''' Defaults to the corresponding property on the parent
    ''' ctlListview control, but can be overridden on a row
    ''' by row basis.
    ''' 
    ''' Set to Color.Empty to revert to parent control's highlightedbackcolour
    ''' </summary>
    ''' <value></value>
    Public Property HighlightedBackColour() As Color
        Get
            If Not mHighlightedBackColour = Color.Empty Then
                Return mHighlightedBackColour
            Else
                If Not Me.OwningListView Is Nothing Then
                    Return Me.OwningListView.HighlightedRowBackColour
                Else
                    Return Color.White
                End If
            End If
        End Get
        Set(ByVal value As Color)
            mHighlightedBackColour = value
        End Set
    End Property

    ''' <summary>
    ''' Private member to store public property HighlightedOutline()
    ''' </summary>
    Private mHighlightedForeColor As Color
    ''' <summary>
    ''' The colour used to outline this row when selected. Goes hand in hand with
    ''' HighlightedBackColour and has same relationship with parent listview.
    ''' </summary>
    ''' <value></value>
    Public Property HighlightedForeColor() As Color
        Get
            If Not Me.mHighlightedForeColor = Color.Empty Then
                Return mHighlightedForeColor
            Else
                If Not Me.OwningListView Is Nothing Then
                    Return Me.OwningListView.HighlightedForeColor
                Else
                    Return Color.White
                End If
            End If
        End Get
        Set(ByVal value As Color)
            mHighlightedForeColor = value
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the collection of items
    ''' </summary>
    ''' <value></value>
    Public ReadOnly Property Items() As clsCollectionWithEvents(Of ctlEditableListItem)
        Get
            Return mobjItems
        End Get
    End Property


    ''' <summary>
    ''' Handles the adding of an item to the item collection.
    ''' </summary>
    ''' <param name="NewItem">The newly added item.</param>
    Private Sub HandleItemAdded(ByVal NewItem As ctlEditableListItem) Handles mobjItems.ItemAdded
        NewItem.ParentRow = Me
        Me.Controls.Add(NewItem)
        AddHandler NewItem.Selected, AddressOf OnSelected
        AddHandler NewItem.ListItemKeyDown, AddressOf OnListItemKeyDown
        Me.UpdateTabIndices()
    End Sub

    ''' <summary>
    ''' Handles the insertion of an item.
    ''' </summary>
    ''' <param name="Index">The index at which the insertion
    ''' took place.</param>
    ''' <param name="NewItem">The newly inserted item.</param>
    Private Sub HandleItemInserted(ByVal Index As Integer, ByVal NewItem As ctlEditableListItem) Handles mobjItems.ItemInserted
        Me.HandleItemAdded(NewItem)
        Me.UpdateTabIndices()
    End Sub

    ''' <summary>
    ''' Handles the addition of several items.
    ''' </summary>
    ''' <param name="NewItems">The newly added items.</param>
    Private Sub HandleItemRangedAdded(ByVal NewItems As List(Of ctlEditableListItem)) Handles mobjItems.ItemRangeAdded
        For Each item As ctlEditableListItem In NewItems
            Me.HandleItemAdded(item)
        Next
        Me.UpdateTabIndices()
    End Sub

    ''' <summary>
    ''' Handles the removing of items from the item collection.
    ''' </summary>
    ''' <param name="RemovedItem">The item removed.</param>
    Private Sub HandleItemRemoved(ByVal RemovedItem As ctlEditableListItem, ByVal OldIndex As Integer) Handles mobjItems.ItemRemoved
        RemoveHandler RemovedItem.Selected, AddressOf OnSelected
        Me.Controls.Remove(RemovedItem)
        RemovedItem.ParentRow = Nothing
        Me.UpdateTabIndices()
    End Sub

    ''' <summary>
    ''' Updates the tab index on each of the items.
    ''' </summary>
    Private Sub UpdateTabIndices()
        For i As Integer = 0 To mobjItems.Count - 1
            mobjItems(i).TabIndex = i
        Next
    End Sub

    ''' <summary>
    ''' Handles the clearing of the item collection.
    ''' </summary>
    ''' <param name="ClearedItems">The items removed.</param>
    Private Sub HandleItemsCleared(ByVal ClearedItems As List(Of ctlEditableListItem)) Handles mobjItems.CollectionCleared
        Dim index As Integer = 0
        For Each item As ctlEditableListItem In ClearedItems
            Me.HandleItemRemoved(item, index)
            index += 1
        Next
    End Sub

    ''' <summary>
    ''' Handles the keydown event in the item usercontrol.  Required to use cursor up and down
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub OnListItemKeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs)
        'Then we need to raise the liseview event to update the blue highlighting
        RaiseEvent ListRowKeyDown(Me, e)
    End Sub

    ''' <summary>
    ''' Handles the Key preview event passed up from any editcontrol that is used.  Required to use cursor up
    ''' and down for editcontrols.  Could not get keydown to work for our editcontrols so was forced to use 
    ''' preview.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub OnEditControlKeyPreview(ByVal sender As Object, ByVal e As PreviewKeyDownEventArgs)
        'Then we need to raise the liseview event to update the blue highlighting
        RaiseEvent EditControlKeyPreview(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the selected event to tell the parent listview which item
    ''' has been selected.
    ''' </summary>
    ''' <param name="sender"></param>
    Private Sub OnSelected(ByVal sender As ctlEditableListItem)
        Me.IsSelected = True
        RaiseEvent Selected(Me, Me.Items.IndexOf(sender))
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Overridable Sub OnDeSelected(ByVal sender As Object, ByVal e As EventArgs)
        RaiseEvent DeSelected(Me, e)
    End Sub

    ''' <summary>
    ''' Used to make the selected items background a different color
    ''' </summary>
    ''' <value></value>
    Public Property IsSelected() As Boolean
        Get
            Return mbSelected
        End Get
        Set(ByVal Value As Boolean)
            If Not Value = Me.mbSelected Then
                mbSelected = Value

                For Each i As ctlEditableListItem In Items
                    i.IsSelected = Value
                Next
                If Not Value Then OnDeSelected(Me, EventArgs.Empty)
                Me.Invalidate()
            End If
        End Set
    End Property

    ''' <summary>
    ''' The onpaint method is overriden to draw some lines representing the columns
    ''' </summary>
    ''' <param name="e">The paint event arguments</param>
    Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)

        e.Graphics.Clear(Color.FromKnownColor(KnownColor.Silver))
    End Sub


    ''' <summary>
    ''' Must be overridden to update the controls so that they match the widths 
    ''' specified in the ctlListItems
    ''' </summary>
    Public Overridable Sub UpdateControls()
        Dim iLeft As Integer = LeftMargin
        For Each item As ctlEditableListItem In mobjItems
            item.Left = iLeft
            item.Height = Me.Height - BottomMargin
            item.Width -= LeftMargin + RightMargin
            iLeft += (LeftMargin + item.Width + RightMargin)
        Next
    End Sub

    Public Const BottomMargin As Integer = 1
    Public Const LeftMargin As Integer = 1
    Public Const RightMargin As Integer = 1

    Public Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext) Implements System.Runtime.Serialization.ISerializable.GetObjectData
        info.AddValue("HighlightedBackColour", Me.mHighlightedBackColour)
        info.AddValue("HighlightedForeColor", Me.mHighlightedForeColor)
        info.AddValue("Items", Me.Items)
        info.AddValue("IsSelected", Me.IsSelected)
    End Sub

    ''' <summary>
    ''' Gets the index of the column which has keyboard focus.
    ''' </summary>
    ''' <returns>Returns the zero-based index of the column
    ''' which has keyboard focus.</returns>
    Public Function GetFocusedColumnIndex() As Integer
        For i As Integer = 0 To Me.Items.Count - 1
            If Me.Items(i).NestedControl IsNot Nothing Then
                If Me.Items(i).NestedControl.Focused Then
                    Return i
                End If
                For Each c As Control In Me.Items(i).NestedControl.Controls
                    If c.Focused Then Return i
                Next
            End If
        Next

        'Default to the first column if none found
        Return 0
    End Function

    ''' <summary>
    ''' Places keyboard focus in the specified column.
    ''' </summary>
    ''' <param name="NewIndex">The zero-based index of the column
    ''' which is to receive keyboard focus.</param>
    Public Sub SetFocusedColumnIndex(ByVal NewIndex As Integer)
        If NewIndex >= 0 AndAlso NewIndex < Me.Items.Count Then
            Dim Item As ctlEditableListItem = Me.Items(NewIndex)
            If Item.NestedControl IsNot Nothing Then
                Item.NestedControl.Focus()
            End If
        End If
    End Sub


    ''' <summary>
    ''' Override the dispose method to ensure all rows are disposed of cleanly
    ''' </summary>
    ''' <param name="disposing"></param>
    ''' <remarks></remarks>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            For Each i As ctlEditableListItem In Items
                i.Dispose()
            Next
        End If

        MyBase.Dispose(disposing)
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        Me.ResumeLayout(False)

    End Sub
End Class
