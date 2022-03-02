Imports BluePrism.AutomateProcessCore

Friend Class clsAutomateListViewItemSorter
    Implements IComparer(Of clsListRow)
    Implements IDisposable

    ''' <summary>
    ''' The sortorder.
    ''' </summary>
    Private mSortOrder As SortOrder

    ''' <summary>
    ''' 
    ''' </summary>
    Private mComparer As CaseInsensitiveComparer

    ''' <summary>
    ''' The owning listview.
    ''' </summary>
    Private mListView As ctlListView

    ''' <summary>
    ''' The data types of each column.
    ''' </summary>
    Private mColumnDataTypes() As DataType

    ''' <summary>
    ''' Previous sorts, so that we can sort using
    ''' multiple columns.
    ''' </summary>
    Private mSortColumnHistory As New System.Collections.Generic.List(Of Integer)

    ''' <summary>
    ''' The default sort orders for each column.
    ''' </summary>
    Private mColumnDefaultSortOrder() As SortOrder

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="listview">The ListView to sort</param>
    Public Sub New(ByVal listview As ctlListView)

        mListView = listview

        AddHandler mListView.ColumnClicked, AddressOf thelistview_ColumnClick

        ' Initialize the column to '0'
        Me.SortColumn = 0

        ' Initialize the sort order to 'None'.
        mSortOrder = SortOrder.None

        ' Initialize the CaseInsensitiveComparer object.
        mComparer = New CaseInsensitiveComparer()

    End Sub


    ''' <summary>
    ''' Gets or sets the column data types.
    ''' </summary>
    ''' <value>An array of Types</value>
    Public Property ColumnDataTypes() As DataType()
        Get
            Return mColumnDataTypes
        End Get
        Set(ByVal Value() As DataType)
            mColumnDataTypes = Value
            If Value.Length < mListView.Columns.Count Then
                Throw New InvalidOperationException(My.Resources.NotEnoughDatatypesInTheColumns)
            End If
        End Set
    End Property


    ''' <summary>
    ''' Gets or sets the default sort order in which the will be sorted when first 
    ''' clicked. Note that a value of SortOrder.None in this array really means that
    ''' the sort order will be left unchanged when you switch columns.
    ''' </summary>
    ''' <value>An array of SortOrder</value>
    Public Property ColumnDefaultSortOrder() As SortOrder()
        Get
            Return mColumnDefaultSortOrder
        End Get
        Set(ByVal Value() As SortOrder)
            mColumnDefaultSortOrder = Value
            If Value.Length < mListView.Columns.Count Then
                Throw New InvalidOperationException(My.Resources.NotEnoughDefaultSortOrdersInTheColumns)
            End If
        End Set
    End Property


    ''' <summary>
    ''' Gets or sets the sort order.
    ''' </summary>
    ''' <value>The sort order</value>
    Public Property Order() As SortOrder
        Set(ByVal Value As SortOrder)
            mSortOrder = Value
        End Set
        Get
            Return mSortOrder
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the sort column.
    ''' </summary>
    ''' <value>The column index</value>
    ''' <returns>The zero-based index of the column being sorted, or -1 if unset.
    ''' </returns>
    Public Property SortColumn() As Integer
        Set(ByVal Value As Integer)
            Me.mSortColumnHistory.Add(Value)

            If (mColumnDataTypes IsNot Nothing) AndAlso (mSortColumnHistory.Count > Me.mColumnDataTypes.Length) Then
                mSortColumnHistory.RemoveAt(0)
            End If
        End Set
        Get
            If (Me.mSortColumnHistory.Count > 0) Then
                Return Me.mSortColumnHistory.Item(Me.mSortColumnHistory.Count - 1)
            Else
                Return -1
            End If
        End Get
    End Property


    Private Sub thelistview_ColumnClick(ByVal sender As Object, ByVal e As ColumnClickEventArgs)
        If (e.Column = SortColumn) Then
            ' Reverse the current sort direction for this column.
            If (Order = SortOrder.Ascending) Then
                Order = SortOrder.Descending
            Else
                Order = SortOrder.Ascending
            End If
        Else
            ' Set the column number that is to be sorted; default to ascending
            ' if the default for the column has not been overridden
            SortColumn = e.Column
            Dim defaultSortOrder As SortOrder = SortOrder.None
            If mColumnDefaultSortOrder IsNot Nothing _
             AndAlso e.Column >= 0 _
             AndAlso e.Column < mColumnDefaultSortOrder.Length Then
                defaultSortOrder = mColumnDefaultSortOrder(e.Column)
            End If

            If defaultSortOrder = SortOrder.None Then
                Order = SortOrder.Ascending
            Else
                Order = defaultSortOrder
            End If
        End If

        ' Perform the sort with these new sort options.
        PerformSort()
    End Sub

    ''' <summary>
    ''' Performs a sort on the wrapped listview using the current settings
    ''' </summary>
    Public Sub PerformSort()
        mListView.SuspendLayout()
        Cursor.Current = Cursors.WaitCursor
        Try
            mListView.SortRows(Me)
        Finally
            mListView.ResumeLayout(True)
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Function Compare(ByVal x As clsListRow, ByVal y As clsListRow) As Integer Implements System.Collections.Generic.IComparer(Of AutomateUI.clsListRow).Compare
        Dim compareResult As Integer

        'Go through each sort column in turn until inequality found
        For i As Integer = mSortColumnHistory.Count - 1 To 0 Step -1

            Dim sortInd As Integer = mSortColumnHistory(i)
            compareResult = 0

            Dim xVal As clsProcessValue = x.Items(sortInd).Value
            Dim yVal As clsProcessValue = y.Items(sortInd).Value
            If xVal IsNot Nothing AndAlso yVal IsNot Nothing Then
                If mColumnDataTypes IsNot Nothing AndAlso mColumnDataTypes.Length > sortInd Then
                    Dim tp As DataType = mColumnDataTypes(sortInd)

                    xVal.TryCastInto(tp, xVal)
                    yVal.TryCastInto(tp, yVal)
                End If

                compareResult = xVal.CompareTo(yVal)
            End If

            'Quit loop if not equal
            If compareResult <> 0 Then Exit For
        Next


        ' Calculate the correct return value based on the object comparison.
        If (mSortOrder = SortOrder.Ascending) Then
            ' Ascending sort is selected, return typical result of compare operation.
            Return compareResult
        ElseIf (mSortOrder = SortOrder.Descending) Then
            ' Descending sort is selected, return negative result of compare operation.
            Return (-compareResult)
        Else
            ' Return '0' to indicate that they are equal.
            Return 0
        End If

    End Function

#Region " IDisposable Support "

    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                RemoveHandler mListView.ColumnClicked, AddressOf thelistview_ColumnClick
            End If
        End If
        Me.disposedValue = True
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

End Class
