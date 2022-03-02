Imports BluePrism.AutomateAppCore
Imports AutomateControls


''' Project  : Automate
''' Class    : clsListViewSorter
''' 
''' <summary>
''' An implementation of IComparer to sort ListView objects.
''' </summary>
Public Class clsListViewSorter
    Implements System.Collections.IComparer

    Private mColInd As Integer
    Private mOrder As SortOrder
    Private WithEvents mListView As FlickerFreeListView
    Private mColTypes() As Type

    Public Event BeforeSort As BeforeSortEventHandler

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
            Return mColTypes
        End Get
        Set(ByVal Value As Type())
            mColTypes = Value
        End Set
    End Property

    ''' <summary>
    ''' The zero-based index of the column to be sorted.
    ''' </summary>
    ''' <value>The column index</value>
    Public Property SortColumn() As Integer
        Set(ByVal Value As Integer)
            mColInd = Value
        End Set
        Get
            Return mColInd
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the sort order.
    ''' </summary>
    ''' <value>The sort order</value>
    Public Property Order() As SortOrder
        Set(ByVal Value As SortOrder)
            mOrder = Value
            mListView.SetSortIcon(mColInd, mOrder)
        End Set
        Get
            Return mOrder
        End Get
    End Property

    ''' <summary>
    ''' Creates a new list view sorter which sorts the given list view.
    ''' </summary>
    ''' <param name="lview">The ListView to sort</param>
    Public Sub New(ByVal lview As FlickerFreeListView)

        mListView = lview
        mListView.SetSortIcon(mColInd, mOrder)

    End Sub

    ''' <summary>
    ''' Handles a column on the host listview being clicked
    ''' </summary>
    Private Sub HandleColumnClick(ByVal sender As Object,
     ByVal e As ColumnClickEventArgs) Handles mListView.ColumnClick

        If e.Column = mColInd Then
            ' Reverse the current sort direction for this column.
            If mOrder = SortOrder.Ascending _
             Then mOrder = SortOrder.Descending _
             Else mOrder = SortOrder.Ascending

        Else
            ' Set the column number that is to be sorted; default to ascending.
            mColInd = e.Column
            mOrder = SortOrder.Ascending

        End If

        Dim beforeSortEventArgs = New BeforeSortEventArgs(mOrder, mColInd)
        RaiseEvent BeforeSort(Me, beforeSortEventArgs)

        If Not beforeSortEventArgs.CancelSort Then
            mListView.Sort(mColInd, mOrder)
        Else
            mListView.SetSortIcon(e.Column, mOrder)
        End If

    End Sub

    ''' <summary>
    ''' Compares the two list view items, based on the type registered for the
    ''' column being sorted (or their text if no column type is registered for the
    ''' currently selected column).
    ''' </summary>
    ''' <param name="x">The first item to compare</param>
    ''' <param name="y">The second item to compare</param>
    ''' <returns>A negative integer, zero, or a positive integer if x is less than,
    ''' equal to, or greater than y, respectively.</returns>
    Private Function Compare(ByVal x As Object, ByVal y As Object) As Integer _
     Implements IComparer.Compare

        ' Cast the objects to be compared to ListViewItem objects.
        Dim xItem As ListViewItem = CType(x, ListViewItem)
        Dim yItem As ListViewItem = CType(y, ListViewItem)

        ' Get their text, since we're most likely to be comparing that.
        Dim xText As String = Nothing
        If mColInd < xItem.SubItems.Count Then
            xText = xItem.SubItems(mColInd).Text
        End If

        Dim yText As String = Nothing
        If mColInd < yItem.SubItems.Count Then
            yText = yItem.SubItems(mColInd).Text
        End If

        ' Fallback compare result - just compare the strings (case insensitively)
        Dim compareResult As Integer = String.Compare(xText, yText, True)

        ' Get the datatype if there is one - ie. if types are set *and* the
        ' type is set specifically for this column
        Dim dType As Type = Nothing
        If mColTypes IsNot Nothing AndAlso mColInd < mColTypes.Length Then _
         dType = mColTypes(mColInd)

        ' Go through each type - attempt to convert it and compare. If it fails at
        ' any point, the fallback compare result will be used.
        If dType Is GetType(Date) Then
            Dim xDate As Date
            Dim yDate As Date
            If Date.TryParse(xText, xDate) AndAlso Date.TryParse(yText, yDate) Then _
             compareResult = xDate.CompareTo(yDate)

        ElseIf dType Is GetType(TimeSpan) Then
            Dim xTime As TimeSpan
            Dim yTime As TimeSpan
            If clsUtility.TryParse(xText, xTime) AndAlso clsUtility.TryParse(yText, yTime) Then _
             compareResult = xTime.CompareTo(yTime)

        ElseIf dType Is GetType(Integer) OrElse dType Is GetType(Decimal) Then
            Dim xDec As Decimal
            Dim yDec As Decimal
            If Decimal.TryParse(xText, xDec) AndAlso Decimal.TryParse(yText, yDec) Then _
             compareResult = xDec.CompareTo(yDec)

        ElseIf dType Is GetType(Image) Then
            compareResult = xItem.ImageIndex.CompareTo(yItem.ImageIndex)

        ElseIf dType Is GetType(IComparable) Then
            Dim xComp As IComparable = TryCast(xItem.Tag, IComparable)
            Dim yComp As IComparable = TryCast(yItem.Tag, IComparable)
            If xComp IsNot Nothing AndAlso yComp IsNot Nothing Then _
             compareResult = xComp.CompareTo(yComp)

        End If

        ' Calculate the correct return value based on the object comparison.
        If mOrder = SortOrder.Ascending Then
            ' Ascending sort is selected, return typical result of compare operation.
            Return compareResult

        ElseIf mOrder = SortOrder.Descending Then
            ' Descending sort is selected, return negative result of compare operation.
            Return -compareResult

        Else
            ' Return '0' to indicate that they are equal.
            Return 0

        End If

    End Function

    Public Delegate Sub BeforeSortEventHandler(sender As Object, e As BeforeSortEventArgs)

    Public Class BeforeSortEventArgs : Inherits EventArgs
        Public ReadOnly Property SortOrder As SortOrder
        Public ReadOnly Property ColumnIndex As Integer
        Public Property CancelSort As Boolean = False
        Public Sub New(sortOrder As SortOrder, columnIndex As Integer)
            Me.SortOrder = sortOrder
            Me.ColumnIndex = columnIndex
        End Sub
    End Class

End Class
