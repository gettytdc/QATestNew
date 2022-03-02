Imports AutomateControls
Imports BluePrism.AutomateProcessCore

''' <summary>
''' Represents the data on a row within a list view.
''' </summary>
Friend Class clsListRow : Implements IDisposable

#Region " Published Events "

    ''' <summary>
    ''' Event fired when an edit control in this list row has been activated.
    ''' </summary>
    ''' <param name="sender">The sender of the event, ie. this list row.</param>
    ''' <param name="valueControl">The activatable process value control which
    ''' has been activated.</param>
    ''' <param name="e">The args defining the event.</param>
    Friend Event EditControlActivated(ByVal sender As clsListRow, _
     ByVal valueControl As IActivatableProcessValue, ByVal e As EventArgs)

    ''' <summary>
    ''' Event raised when editing has ended on this control
    ''' </summary>
    Public Event EditingEnded(ByVal Sender As clsListRow)

#End Region

#Region " Member Variables "

    ''' <summary>
    ''' The point at which the mouse was depressed on this control, as reported
    ''' by the OnMouseDown method.
    ''' </summary>
    Protected mMouseDownPoint As Point

    ''' <summary>
    ''' The point at which the mouse was released on this control, as reported
    ''' by the OnMouseUp method.
    ''' </summary>
    Protected mMouseUpPoint As Point

    ' The list of items (ie. cells) represented in this row
    Private mItems As IList(Of clsListItem)

    ' Generic data held in this row
    Private mTag As Object

    ' Flag indicating selected state of this row
    Private mSelected As Boolean

    ' The owner listview of this row
    Private mOwner As ctlListView

    ' Flag indicating if this row has been disposed or not
    Private mIsDisposed As Boolean

    ' Flag indicating if this row is currently being edited or not
    Private mEditing As Boolean

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty ListRow
    ''' </summary>
    Private Sub New()
        Me.New(Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new empty ListRow owned by the given listview
    ''' </summary>
    ''' <param name="owningListView">The listview which owns this row.</param>
    Public Sub New(ByVal owningListView As ctlListView)
        mItems = New List(Of clsListItem)
        mOwner = owningListView
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The items contained in this row
    ''' </summary>
    Public ReadOnly Property Items() As IList(Of clsListItem)
        Get
            Return mItems
        End Get
    End Property

    ''' <summary>
    ''' The listview owning this row, if any.
    ''' </summary>
    Public Property Owner() As ctlListView
        Get
            Return mOwner
        End Get
        Set(ByVal value As ctlListView)
            mOwner = value
        End Set
    End Property

    ''' <summary>
    ''' A place for people to stuff things
    ''' </summary>
    Public Property Tag() As Object
        Get
            Return mTag
        End Get
        Set(ByVal value As Object)
            mTag = value
        End Set
    End Property

    ''' <summary>
    ''' Indicates whether the row is selected in the UI.
    ''' This is independent of all other rows: more than
    ''' one row can be selected at once.
    ''' </summary>
    Public Property Selected() As Boolean
        Get
            Return mSelected
        End Get
        Set(ByVal value As Boolean)
            mSelected = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the index of this row in its parent's row
    ''' collection.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The index as described in summary, or
    ''' -1 if the row has no index.</returns>
    Public ReadOnly Property Index() As Integer
        Get
            If Owner IsNot Nothing Then
                Return Owner.Rows.IndexOf(Me)
            Else
                Return -1
            End If
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Adds a list item representing the given process value to this list row.
    ''' </summary>
    ''' <param name="pv">The process value to add into a list item.</param>
    ''' <returns>The item created by adding the given value.</returns>
    Public Function AddItem(ByVal pv As clsProcessValue) As clsListItem
        Dim item As New clsListItem(Me, pv)
        mItems.Add(item)
        Return item
    End Function

    ''' <summary>
    ''' Adds a list item representing the given text value to this list row.
    ''' </summary>
    ''' <param name="txt">The text value to add into a list item.</param>
    ''' <returns>The item created by adding the given value.</returns>
    Public Function AddItem(ByVal txt As String) As clsListItem
        Return AddItem(txt, True)
    End Function

    ''' <summary>
    ''' Adds a list item representing the given text value to this list row.
    ''' </summary>
    ''' <param name="txt">The text value to add into a list item.</param>
    ''' <returns>The item created by adding the given value.</returns>
    Public Function AddItem(ByVal txt As String, ByVal multiline As Boolean) _
     As clsListItem
        Dim item As clsListItem = AddItem(New clsProcessValue(txt))
        item.Multiline = multiline
        Return item
    End Function

    ''' <summary>
    ''' Ensures that the row is visible, by scrolling it
    ''' into view.
    ''' </summary>
    ''' <remarks>If the row has no parent listview, then has no effect.</remarks>
    Public Sub EnsureVisible()
        If Owner IsNot Nothing Then Owner.ScrollToRow(Me)
    End Sub

    ''' <summary>
    ''' Removes this row from its parent listview, if it has one.
    ''' </summary>
    ''' <remarks>If this row is not part of a listview then has no effect.</remarks>
    Public Sub Remove()
        If Owner IsNot Nothing Then Owner.Rows.Remove(Me)
    End Sub

    ''' <summary>
    ''' Instructs the row to populate the supplied edit row with its
    ''' data, ready for editing.
    ''' </summary>
    ''' <param name="EditRow">The edit row to be used when editing this row.</param>
    Public Overridable Sub BeginEdit(ByVal EditRow As ctlEditableListRow)
        mEditing = True
        For i As Integer = 0 To Me.Items.Count - 1
            Dim item As clsListItem = Me.Items(i)
            Dim ctl As Control = EditRow.Items(i).NestedControl

            Select Case True
                Case TypeOf ctl Is IProcessValue
                    CType(ctl, IProcessValue).Value = item.Value

                Case TypeOf ctl Is clsEnumeratedValueCombobox(Of String)
                    CType(ctl, clsEnumeratedValueCombobox(Of String)).ChosenValue = _
                     CStr(item.Value)

                Case TypeOf ctl Is CheckBox
                    CType(ctl, CheckBox).Checked = CBool(item.Value)

                Case Else
                    ctl.Text = item.Value.FormattedValue
            End Select
        Next
    End Sub

    ''' <summary>
    ''' Commits the data contained in the supplied edit row, following an
    ''' edit operation.
    ''' </summary>
    ''' <param name="EditRow">The edit row containing data to be committed to this
    ''' row.</param>
    Public Overridable Sub EndEdit(ByVal editRow As ctlEditableListRow)
        For i As Integer = 0 To Me.Items.Count - 1
            Dim item As clsListItem = Me.Items(i)
            Dim ctl As Control = editRow.Items(i).NestedControl
            ' If we find the currently focused control, go back to the previous
            ' control in the tabbing order in order to force validation processing
            'If ctl.Focused Then _
            ' ctl.TopLevelControl.GetNextControl(ctl, False).Focus()

            Select Case True
                Case TypeOf ctl Is IProcessValue
                    item.Value = CType(ctl, IProcessValue).Value
                Case TypeOf ctl Is clsEnumeratedValueCombobox(Of String)
                    item.Value = _
                     CType(ctl, clsEnumeratedValueCombobox(Of String)).ChosenValue
                Case TypeOf ctl Is clsDataTypesComboBox
                    item.Value = clsProcessDataTypes.GetFriendlyName(CType(ctl, clsDataTypesComboBox).ChosenDataType)
                Case TypeOf ctl Is clsEnumeratedValueCombobox(Of Object)
                    item.Value = CType(ctl, clsEnumeratedValueCombobox(Of Object)).ChosenValue.ToString()
                Case TypeOf ctl Is CheckBox
                    item.Value = CType(ctl, CheckBox).Checked
                Case Else
                    item.Value = ctl.Text
            End Select
        Next

        OnEditingEnded()

    End Sub

    ''' <summary>
    ''' Raises the <see cref="EditingEnded" /> event
    ''' </summary>
    Protected Overridable Sub OnEditingEnded()
        mEditing = False
        RaiseEvent EditingEnded(Me)
    End Sub


    ''' <summary>
    ''' Creates an edit row suitable for editing the values in this row
    ''' </summary>
    ''' <returns>Returns an editable listview row, populated with the data in
    ''' this row, ready to edit.</returns>
    Public Overridable Function CreateEditRow() As ctlEditableListRow
        Dim editableRow As New ctlEditableListRow()

        For index As Integer = 0 To Me.Items.Count - 1
            Dim ctl As Control = CreateEditControl(Items(index))

            If ctl IsNot Nothing AndAlso
               (TypeOf ctl Is ctlProcessText OrElse
                TypeOf ctl Is ctlCollectionDefinition) Then
                ctl.ImeMode = ImeMode.Off
            End If

            ' Ensure that the control stretches as the column stretches... and,
            ' er, stretches down. For some reason. Not a clue.
            ctl.Anchor = _
             AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right

            Dim activator As IActivatableProcessValue = _
             TryCast(ctl, IActivatableProcessValue)
            If activator IsNot Nothing Then _
             AddHandler activator.Activated, AddressOf HandleEditControlActivated

            Dim editableItem As New ctlEditableListItem(ctl)
            editableItem.Width = Owner.Columns(index).Width
            editableRow.Items.Add(editableItem)

        Next

        Return editableRow
    End Function

    ''' <summary>
    ''' Creates an edit control for the given item
    ''' </summary>
    ''' <param name="item">The list item for which a control is required.</param>
    ''' <returns>A control suitable for editing the given item</returns>
    Protected Overridable Function CreateEditControl(ByVal item As clsListItem) _
     As Control

        If item.AvailableValues IsNot Nothing Then
            Return CreatePickerControl(item)

        Else
            'Create a generic iProcessValue edit control
            Dim dt As DataType = DataType.text
            If item.Value IsNot Nothing Then dt = item.Value.DataType

            Return clsProcessValueControl.GetControl(dt, item.Multiline)
        End If

    End Function

    ''' <summary>
    ''' Creates a combobox picker control for items which stem from an 
    ''' enumeration.
    ''' </summary>
    ''' <param name="Item">The item for which a picker control should be created.</param>
    Protected Overridable Function CreatePickerControl(ByVal item As clsListItem) As Control
        'Create a picker for the enum values
        Dim cb As New clsEnumeratedValueCombobox(Of String)
        For Each pair As KeyValuePair(Of String, String) In item.AvailableValues
            cb.Items.Add(New ComboBoxItem(pair.Value, pair.Key))
        Next
        Return cb
    End Function


    ''' <summary>
    ''' Handler for an edit control for a process value being activated.
    ''' </summary>
    ''' <param name="sender">The control which was activated</param>
    ''' <param name="e">The arguments definining the event.</param>
    Private Sub HandleEditControlActivated( _
     ByVal sender As IActivatableProcessValue, ByVal e As EventArgs)
        RaiseEvent EditControlActivated(Me, sender, e)
    End Sub

    ''' <summary>
    ''' Draws the current row at the specified position
    ''' </summary>
    ''' <param name="g">The graphics object to use.</param>
    ''' <param name="top">The vertical position (in coordinates understood
    ''' by the graphics object) at which drawing should begin. The
    ''' left hand side is assumed to be zero; column widths are retrieved
    ''' from Owner.</param>
    Public Overridable Sub Draw(ByVal g As Graphics, ByVal top As Integer)
        If Me.Owner IsNot Nothing Then

            'Iterate through the columns, drawing each one for this row
            Dim Left As Integer = 0
            For ColumnIndex As Integer = 0 To Me.Owner.Columns.Count - 1
                Dim rect As New Rectangle(Left, top, _
                 Owner.Columns(ColumnIndex).Width, ctlListView.DefaultRowHeight)
                Items(ColumnIndex).Draw(g, rect)
                Left += rect.Width
            Next

        End If
    End Sub

    ''' <summary>
    ''' Informs the row that it has been mouse-downed at this point. This corresponds
    ''' to a click on the row when there is no current edit row. Eg if one of the
    ''' row's item is mimicking a checkbox by rendering it, then it needs to process
    ''' this click.
    ''' </summary>
    ''' <param name="locn">The point, in coordinates relative to the local
    ''' row, at which the click took place</param>
    Public Overridable Sub OnMouseDown(ByVal locn As Point)
        If Not Items.Any() Then Return
        ' FIXME: As far as I can tell, this does nothing to 'locX'... is it meant to?
        Dim locX As Integer
        Dim index As Integer
        For i As Integer = 0 To Owner.Columns.Count - 1
            If locX - Owner.Columns(i).Width > 0 Then
                locX -= Owner.Columns(i).Width
                index = i
            Else
                Exit For
            End If
        Next

        Items(index).OnMouseDown(New Point(locX, locn.Y))
        mMouseDownPoint = locn
    End Sub

    ''' <summary>
    ''' Informs the row that it has been mouse-uped at this point. This corresponds
    ''' to a click on the row when there is no current edit row. Eg if one of the
    ''' row's item is mimicking a checkbox by rendering it, then it needs to process
    ''' this click.
    ''' </summary>
    ''' <param name="locn">The point, in coordinates relative to the local
    ''' row, at which the click took place</param>
    Public Overridable Sub OnMouseUp(ByVal locn As Point)
        If Not Items.Any() Then Return
        ' FIXME: As far as I can tell, this does nothing to 'locX'... is it meant to?
        Dim locX As Integer
        Dim index As Integer
        For i As Integer = 0 To Owner.Columns.Count - 1
            If locX - Owner.Columns(i).Width > 0 Then
                locX -= Owner.Columns(i).Width
                index = i
            Else
                Exit For
            End If
        Next

        Items(index).OnMouseUp(New Point(locX, locn.Y))
        mMouseUpPoint = locn
    End Sub

    ''' <summary>
    ''' Informs the row that something is being dragged over it. The row
    ''' may choose to process and accept the drop request. By default
    ''' it will be rejected here
    ''' </summary>
    ''' <param name="e">Information about the proposed drag event.</param>
    ''' <param name="locn">The point at which the drag event is taking
    ''' place, in coordinates relative to the row. This should be used in
    ''' preference to the X, Y values in the event args.</param>
    Public Overridable Sub OnDragOver( _
     ByVal e As DragEventArgs, ByVal locn As Point, ByVal colIndex As Integer)
        e.Effect = DragDropEffects.None
    End Sub


    ''' <summary>
    ''' Informs the row that something is being dropped onto it. The row
    ''' may choose to process and accept the drop request. By default
    ''' it will be rejected here
    ''' </summary>
    ''' <param name="e">Information about the proposed drag event.</param>
    ''' <param name="locn">The point at which the drag event is taking
    ''' place, in coordinates relative to the row. This should be used in
    ''' preference to the X, Y values in the event args.</param>
    ''' <param name="colIndex">The index of the column into which the
    ''' drop event occurred, as calculated by the ctlListView which
    ''' propogated the event.</param>
    Public Overridable Sub OnDragDrop( _
     ByVal e As DragEventArgs, ByVal locn As Point, ByVal colIndex As Integer)
        e.Effect = DragDropEffects.None
    End Sub

    ''' <summary>
    ''' Informs thet row that the drag operation has left one of the cells
    ''' in this row (or the row entirely).
    ''' </summary>
    ''' <param name="VacatedCellIndex">The zero-based index of the cell that
    ''' was last visited by the drag-drop operation.</param>
    Public Overridable Sub OnDragLeave(ByVal VacatedCellIndex As Integer)
        If VacatedCellIndex > -1 AndAlso VacatedCellIndex < Me.Items.Count Then
            Me.Items(VacatedCellIndex).Highlighted = False
            If Me.Owner IsNot Nothing Then
                Me.Owner.InvalidateRow(Me)
            End If
        End If
    End Sub

    ''' <summary>
    ''' Disposes of this list row
    ''' </summary>
    ''' <param name="explicit">True to indicate an explicit call to Dispose(), false
    ''' to indicate that it is being called via object finalizer.</param>
    Protected Overridable Sub Dispose(ByVal explicit As Boolean)
        If Not mIsDisposed Then
            If explicit Then
                If mItems IsNot Nothing Then
                    For Each item As clsListItem In Items
                        item.Dispose()
                    Next
                End If
                mItems = Nothing
            End If
        End If
        mIsDisposed = True
    End Sub

    ''' <summary>
    ''' Explicitly disposes of this list row
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

End Class
