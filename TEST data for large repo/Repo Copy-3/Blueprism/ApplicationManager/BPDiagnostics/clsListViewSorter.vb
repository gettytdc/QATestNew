Imports System.Runtime.InteropServices.APIs

''' Project  : ApplicationManager
''' Class    : clsListViewSorter
''' 
''' <summary>
''' An implementation of IComparer to sort ListView objects.
''' </summary>
Public Class clsListViewSorter
    Implements System.Collections.IComparer

    Private ColumnToSort As Integer
    Private OrderOfSort As SortOrder
    Private ObjectCompare As CaseInsensitiveComparer
    Private theListView As ListView
    Private mColumnDataTypes() As Type

    ''' <summary>
    ''' Gets the image associated with the supplied sort order.
    ''' </summary>
    ''' <param name="Order">The sort order of interest.</param>
    ''' <returns>The image that is used with the supplied sort
    ''' order, if any.</returns>
    Public Function GetImage(ByVal Order As SortOrder) As Image
        Return clsSortImages.Instance.GetImage(Order)
    End Function

    ''' <summary>
    ''' Gets or sets the column data types.
    ''' </summary>
    ''' <value>An array of Types</value>
    Public Property ColumnDataTypes() As Type()
        Get
            Return mColumnDataTypes
        End Get
        Set(ByVal Value As Type())
            mColumnDataTypes = Value
            If Value.Length < theListView.Columns.Count Then
                Throw New InvalidOperationException(My.Resources.NotEnoughDatatypesInTheColumns)
            End If
        End Set
    End Property

    ''' <summary>
    ''' The zero-based index of the column to be sorted.
    ''' </summary>
    ''' <value>The column index</value>
    Public Property SortColumn() As Integer

        Set(ByVal Value As Integer)
            ColumnToSort = Value
        End Set

        Get
            Return ColumnToSort
        End Get

    End Property

    ''' <summary>
    ''' Gets or sets the sort order.
    ''' </summary>
    ''' <value>The sort order</value>
    Public Property Order() As SortOrder
        Set(ByVal Value As SortOrder)
            OrderOfSort = Value
            SetImage(OrderOfSort)
        End Set

        Get
            Return OrderOfSort
        End Get
    End Property


    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="listview">The ListView to sort</param>
    Public Sub New(ByVal listview As ListView)

        theListView = listview

        AddHandler theListView.ColumnClick, AddressOf thelistview_ColumnClick

        ' Initialize the column to '0'
        ColumnToSort = 0

        ' Initialize the sort order to 'None'.
        OrderOfSort = SortOrder.None

        ' Initialize the CaseInsensitiveComparer object.
        ObjectCompare = New CaseInsensitiveComparer

        SetImage(OrderOfSort)

    End Sub

    Private Sub thelistview_ColumnClick(ByVal sender As Object, ByVal e As ColumnClickEventArgs)

        If (e.Column = ColumnToSort) Then
            ' Reverse the current sort direction for this column.
            If (Order = SortOrder.Ascending) Then
                Order = SortOrder.Descending
            Else
                Order = SortOrder.Ascending
            End If
        Else
            ' Set the column number that is to be sorted; default to ascending.
            SortColumn = e.Column
            Order = SortOrder.Ascending
        End If

        ' Perform the sort with these new sort options.
        theListView.SuspendLayout()
        Cursor.Current = Cursors.WaitCursor
        theListView.Sort()
        SetImage(Order)
        theListView.ResumeLayout(True)
        Cursor.Current = Cursors.Default

    End Sub

    Private Sub SetImage(ByVal sortOrder As SortOrder)
        Dim hwnd As IntPtr
        Dim lret As IntPtr
        Dim iCol As Integer

        'Assign the ImageList to the header control.
        'The header control includes all columns.
        'Get a handle to the header control.
        hwnd = APIsUser32.SendMessage(theListView.Handle, APIsEnums.ListViewMessages.GETHEADER, 0, 0)

        Dim imgSortHeader As ImageList = clsSortImages.Instance.GetImageList()

        'Add the ImageList to the header control.
        lret = APIsUser32.SendMessage(hwnd, APIsEnums.HeaderControlMessages.SETIMAGELIST, 0, imgSortHeader.Handle)

        'The code to follow uses successive images in the ImageList to loop  'through all columns and place successive columns in the ColumnHeader.
        'This code uses LVCOLUMN to define alignment. By using LVCOLUMN here, 
        'you reset the alignment if it was defined in the designer. 
        'If you need to set the alignment, you must change the code below to set it here.
        For iCol = 0 To theListView.Columns.Count - 1
            'Use the LVM_SETCOLUMN message to set the column's image index. 
            Dim column As APIsStructs.LVCOLUMN

            column.mask = APIsEnums.LVCF.FMT Or APIsEnums.LVCF.IMAGE
            column.fmt = APIsEnums.LVCFMT.IMAGE
            If iCol = SortColumn Then
                'The image to use from the Image List.
                If sortOrder = sortOrder.Ascending Then
                    column.iImage = clsSortImages.ArrowType.Ascending
                End If
                If sortOrder = sortOrder.Descending Then
                    column.iImage = clsSortImages.ArrowType.Descending
                End If
            Else
                column.iImage = clsSortImages.ArrowType.Blank
            End If

            column.cchTextMax = 0
            column.cx = 0
            column.iOrder = 0
            column.iSubItem = 0
            column.pszText = IntPtr.op_Explicit(0)
            'Send the LVM_SETCOLUMN message.
            'The column to which you are assigning the image is defined in the third parameter.
            lret = APIsUser32.SendMessage(theListView.Handle, APIsEnums.ListViewMessages.SETCOLUMN, iCol, column)
        Next

    End Sub

    Private Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements IComparer.Compare
        Dim compareResult As Integer
        Dim listViewItemX As ListViewItem
        Dim listViewItemY As ListViewItem
        Dim firstDate As System.DateTime
        Dim secondDate As System.DateTime

        ' Cast the objects to be compared to ListViewItem objects.
        listViewItemX = CType(x, ListViewItem)
        listViewItemY = CType(y, ListViewItem)

        If Not mColumnDataTypes Is Nothing Then
            If (mColumnDataTypes(ColumnToSort) Is GetType(DateTime)) OrElse (mColumnDataTypes(ColumnToSort) Is GetType(Date)) Then
                If Not listViewItemX.SubItems(ColumnToSort).Text = "" _
                 AndAlso Not listViewItemY.SubItems(ColumnToSort).Text = "" _
                  AndAlso DateTime.TryParse(listViewItemX.SubItems(ColumnToSort).Text, firstDate) _
                   AndAlso DateTime.TryParse(listViewItemY.SubItems(ColumnToSort).Text, secondDate) Then
                    compareResult = DateTime.Compare(firstDate, secondDate)
                Else
                    compareResult = ObjectCompare.Compare(listViewItemX.SubItems(ColumnToSort).Text, listViewItemY.SubItems(ColumnToSort).Text)
                End If

            ElseIf mColumnDataTypes(ColumnToSort) Is GetType(Integer) OrElse mColumnDataTypes(ColumnToSort) Is GetType(Decimal) Then
                Dim Val1 As String = listViewItemX.SubItems(ColumnToSort).Text
                Dim Val2 As String = listViewItemY.SubItems(ColumnToSort).Text
                If Val1 = "" Then Val1 = Integer.MinValue.ToString
                If Val2 = "" Then Val1 = Integer.MinValue.ToString
                Dim dec1, dec2 As Decimal
                If Decimal.TryParse(Val1, dec1) _
                  AndAlso Decimal.TryParse(Val2, dec2) Then
                    compareResult = Decimal.Compare(dec1, dec2)
                Else
                    compareResult = 0
                End If
            ElseIf mColumnDataTypes(ColumnToSort) Is GetType(Image) Then
                'compare the index of the imagelist
                If listViewItemX.ImageIndex <> listViewItemY.ImageIndex Then
                    compareResult = listViewItemX.ImageIndex.CompareTo(listViewItemY.ImageIndex)
                Else
                    compareResult = listViewItemX.ImageKey.CompareTo(listViewItemY.ImageKey)
                End If
            Else
                compareResult = ObjectCompare.Compare(listViewItemX.SubItems(ColumnToSort).Text, listViewItemY.SubItems(ColumnToSort).Text)
            End If
        Else
            compareResult = ObjectCompare.Compare(listViewItemX.SubItems(ColumnToSort).Text, listViewItemY.SubItems(ColumnToSort).Text)
        End If

        ' Calculate the correct return value based on the object comparison.
        If (OrderOfSort = SortOrder.Ascending) Then
            ' Ascending sort is selected, return typical result of compare operation.
            Return compareResult
        ElseIf (OrderOfSort = SortOrder.Descending) Then
            ' Descending sort is selected, return negative result of compare operation.
            Return (-compareResult)
        Else
            ' Return '0' to indicate that they are equal.
            Return 0
        End If
    End Function
End Class
