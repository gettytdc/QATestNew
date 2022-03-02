Imports system.Runtime.Serialization

''' Project  : Automate
''' Class    : ctlListColumn
''' 
''' <summary>
''' Helper class that represents a column header
''' </summary>
Friend Class ctlListColumn
    Implements ISerializable

#Region " Constants, Events and Memvars "

    ''' <summary>
    ''' A magic value to use in the <see cref="Width"/> property to indicate that
    ''' the width should be set to the largest default width of the items under
    ''' this column.
    ''' </summary>
    Public Const AutoWidthLargestItem As Integer = -1

    ''' <summary>
    ''' A magic value to use in the <see cref="Width"/> property to indicate that
    ''' the width should be set to the column text's width.
    ''' </summary>
    Public Const AutoWidthColumn As Integer = -2

    ''' <summary>
    ''' A magic value to use in the <see cref="Width"/> property to indicate that
    ''' the width should be set to either the width of the column text, or the
    ''' largest default width of the items under this column, whichever is larger.
    ''' </summary>
    Public Const AutoWidthLargestColumnOrItem As Integer = -3

    ''' <summary>
    ''' The default width to use in a column with no width set.
    ''' </summary>
    Private Const DefaultWidth As Integer = 200

    ''' <summary>
    ''' Event fired when this column is resized.
    ''' </summary>
    ''' <param name="width">The new width of the column</param>
    Public Event Resized(ByVal width As Integer)

    ''' <summary>
    ''' Flag to indicate the hover state of this column
    ''' </summary>
    Private mHover As Boolean

    ''' <summary>
    ''' The text for this column - this represents the label displayed in the
    ''' column header of the associated listview.
    ''' </summary>
    Private mText As String

    ''' <summary>
    ''' The name of this column. This can be used to reference the column within
    ''' the listviews column collection.
    ''' If not explicitly set, this will adopt the <see cref="Text"/> property
    ''' of the column with all space characters replaced by underscores.
    ''' </summary>
    Private mName As String

    ''' <summary>
    ''' The list header which owns this column
    ''' </summary>
    Private mOwningHeader As ctlListHeader

    ''' <summary>
    ''' Flag to indicate the current resize state of this column.
    ''' </summary>
    Private mResizing As Boolean

    ''' <summary>
    ''' The width of this column.
    ''' </summary>
    Private mWidth As Integer = 200

    ''' <summary>
    ''' The sort order for elements under this column.
    ''' </summary>
    Private mSortOrder As SortOrder = System.Windows.Forms.SortOrder.None

    ''' <summary>
    ''' The tooltip for this column
    ''' </summary>
    Private mToolTip As String

    ' The tag associated with this column
    Private mTag As Object

#End Region

#Region " (Non-serialization) Constructors "

    Public Sub New()
        Me.New("Column Title", Nothing, DefaultWidth)
    End Sub

    Public Sub New(ByVal title As String)
        Me.New(title, Nothing, DefaultWidth)
    End Sub

    Public Sub New(ByVal title As String, ByVal tooltip As String)
        Me.New(title, tooltip, DefaultWidth)
    End Sub

    Public Sub New(ByVal title As String, ByVal width As Integer)
        Me.New(title, Nothing, width)
    End Sub

    Public Sub New(ByVal title As String, ByVal tooltip As String, ByVal width As Integer)
        mText = title
        mToolTip = tooltip
        Me.Width = width
    End Sub

#End Region

#Region " ISerializable constructor and implementation "

    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        mHover = info.GetBoolean("Hover")
        mResizing = info.GetBoolean("Resizing")
        mText = info.GetString("Text")
        mWidth = info.GetInt32("Width")
    End Sub

    Public Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext) Implements System.Runtime.Serialization.ISerializable.GetObjectData
        info.AddValue("Hover", mHover)
        info.AddValue("Resizing", mResizing)
        info.AddValue("Text", mText)
        info.AddValue("Width", mWidth)
    End Sub

#End Region

#Region " Public properties "

    ''' <summary>
    ''' The name of this list column.
    ''' If the name has not been set explicitly, this will adopt the 
    ''' <see cref="Text"/> of the column with all space characters replaced by
    ''' underscores.
    ''' </summary>
    Public Property Name() As String
        Get
            If String.IsNullOrEmpty(mName) Then Return Me.Text.Replace(" "c, "_"c)
            Return mName
        End Get
        Set(ByVal value As String)
            mName = value
        End Set
    End Property

    ''' <summary>
    ''' The text of this column.
    ''' This is what is displayed in the column header.
    ''' </summary>
    Public Property Text() As String
        Get
            Return mText
        End Get
        Set(ByVal Value As String)
            mText = Value
        End Set
    End Property

    ''' <summary>
    ''' The arbitrary data object associated with this column.
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
    ''' Flag indicating the current hover state of this column.
    ''' </summary>
    Public Property Hover() As Boolean
        Get
            Return mHover
        End Get
        Set(ByVal Value As Boolean)
            mHover = Value
        End Set
    End Property

    ''' <summary>
    ''' The index of this column in the column collection of the owning header.
    ''' This will be -1 if it is not part of a column collection.
    ''' </summary>
    Public ReadOnly Property Index() As Integer
        Get
            If Me.mOwningHeader IsNot Nothing Then
                For i As Integer = 0 To Me.mOwningHeader.Columns.Count - 1
                    If Me.mOwningHeader.Columns(i) Is Me Then Return i
                Next
            End If

            Return -1
        End Get
    End Property

    ''' <summary>
    ''' The sort order for this column.
    ''' If set to anything other than <see cref="SortOrder.None"/>, an 
    ''' appropriate arrow will be rendered on the column header to indicate
    ''' the sort order.
    ''' </summary>
    Public ReadOnly Property SortOrder() As SortOrder
        Get
            If mOwningHeader IsNot Nothing Then
                Dim sorter As clsAutomateListViewItemSorter = mOwningHeader.Sorter
                If sorter IsNot Nothing Then
                    If Me.Index = sorter.SortColumn Then Return sorter.Order
                End If
            End If
            Return SortOrder.None
        End Get
    End Property

    ''' <summary>
    ''' The header control which owns this column, if any. Null if it is not
    ''' attached to a header.
    ''' </summary>
    Public Property OwningHeader() As ctlListHeader
        Get
            Return mOwningHeader
        End Get
        Set(ByVal value As ctlListHeader)
            mOwningHeader = value
        End Set
    End Property

    ''' <summary>
    ''' The tooltip currently set in this list column.
    ''' </summary>
    Public Property ToolTip() As String
        Get
            Return mToolTip
        End Get
        Set(ByVal value As String)
            mToolTip = value
        End Set
    End Property

    ''' <summary>
    ''' Gives the x-coordinate of the right-hand edge of this column (ie the
    ''' width of this column added to the sum of the widths of all columns to the
    ''' left of this one).
    ''' </summary>
    Public ReadOnly Property Right() As Integer
        Get
            If Not Me.OwningHeader Is Nothing Then
                Return Me.Left + Me.Width
            End If
        End Get
    End Property

    ''' <summary>
    ''' Gives the x-coordinate of the left-hand edge of this column (ie the sum
    ''' of the widths of all columns to the left of this one).
    ''' </summary>
    Public ReadOnly Property Left() As Integer
        Get
            If Not Me.OwningHeader Is Nothing Then
                Dim iTotal As Integer = 0
                For Each c As ctlListColumn In Me.mOwningHeader.Columns
                    If c Is Me Then Exit For
                    iTotal += c.Width
                Next

                Return iTotal
            End If
        End Get
    End Property

    ''' <summary>
    ''' The current resizing state of this column - true if it is currently in
    ''' the process of being resized, false otherwise. I think.
    ''' </summary>
    Public Property Resizing() As Boolean
        Get
            Return mResizing
        End Get
        Set(ByVal value As Boolean)
            mResizing = value
            If value Then mHover = False ' Can't hover while resizing
        End Set
    End Property

    ''' <summary>
    ''' Sets the width of the column, in pixels.
    ''' </summary>
    ''' <value>
    ''' <para>
    ''' Set to <see cref="AutoWidthLargestItem"/> to auto-size to the longest
    ''' <em>default</em> size of any item in the column.
    ''' </para><para>
    ''' Set to <see cref="AutoWidthColumn"/> to auto-size to the width of the
    ''' column <see cref="Text">Text</see>.
    ''' </para><para>
    ''' Set to <see cref="AutoWidthLargestColumnOrItem"/> to auto-size to the
    ''' maximum resulting size of either <see cref="AutoWidthLargestItem"/> or
    ''' <see cref="AutoWidthColumn"/>.
    ''' </para>
    ''' </value>
    ''' <remarks>Note that the resulting size will still
    ''' be subject to the MinimumColumnSize policy set
    ''' by the OwningColumnHeader</remarks>
    Public Property Width() As Integer
        Get
            Return mWidth
        End Get
        Set(ByVal value As Integer)
            Select Case value

                Case AutoWidthLargestItem
                    mWidth = GetWidestItemWidth()

                Case AutoWidthColumn
                    If Me.OwningHeader IsNot Nothing Then
                        mWidth = mOwningHeader.GetColumnWidth(Me.Index)
                    End If

                Case AutoWidthLargestColumnOrItem
                    Dim Max As Integer = Integer.MinValue
                    If mOwningHeader IsNot Nothing Then
                        Max = Math.Max(Max, mOwningHeader.GetColumnWidth(Me.Index))
                    End If
                    Max = Math.Max(Max, GetWidestItemWidth)
                    mWidth = Max

                Case Else
                    mWidth = value
            End Select

            If Me.OwningHeader IsNot Nothing Then
                mWidth = Math.Max(mWidth, Me.OwningHeader.MinimumColumnWidth)
            End If

            RaiseEvent Resized(mWidth)
        End Set
    End Property

#End Region

#Region " Private methods "

    ''' <summary>
    ''' Gets the <em>preferred</em> width of the widest item contained in this
    ''' column of the owning listview.
    ''' </summary>
    ''' <returns>Returns the width, as described in the summary, or returns 0 if
    ''' this column is not part of a column header or listview.</returns>
    Private Function GetWidestItemWidth() As Integer
        If Me.OwningHeader IsNot Nothing Then
            If Me.OwningHeader.OwningListView IsNot Nothing Then
                Dim max As Integer = Integer.MinValue
                For Each r As clsListRow In Me.OwningHeader.OwningListView.Rows
                    If r.Items.Count > Me.Index Then
                        max = Math.Max(max, r.Items(Me.Index).GetPreferredSize(Size.Empty).Width)
                    End If
                Next
                Return max
            End If
        End If

        Return 0
    End Function

#End Region

End Class

''' Project  : Automate
''' Class    : clsColumnCollection
''' 
''' <summary>
''' A typed collection class that takes ctlListColumns.
''' </summary>
Friend Class clsColumnCollection
    Inherits CollectionBase

    ''' <summary>
    ''' The listheader owning this column collection.
    ''' </summary>
    Private mHeader As ctlListHeader

    ''' <summary>
    ''' Creates a new column collection owned by the given header.
    ''' </summary>
    ''' <param name="parent">The list header which owns this collection.</param>
    Public Sub New(ByVal parent As ctlListHeader)
        Me.mHeader = parent
    End Sub

    ''' <summary>
    ''' Adds the given column to this collection, ensuring that it meets the
    ''' header's minimum width constraints and that its owning header property
    ''' is set correctly.
    ''' After adding, this invalidates the owning header to ensure that the
    ''' new column is painted.
    ''' </summary>
    ''' <param name="col">The column to add to this collection.</param>
    ''' <returns>The column after it has been added and displayed.</returns>
    Public Function Add(ByVal col As ctlListColumn) As ctlListColumn
        If col.Width < mHeader.MinimumColumnWidth Then
            col.Width = mHeader.MinimumColumnWidth
        End If

        col.OwningHeader = mHeader
        List.Add(col)
        mHeader.Invalidate()
        Return col
    End Function

    Public Function Add(ByVal sTitle As String) As ctlListColumn
        Return Add(New ctlListColumn(sTitle))
    End Function

    Public Function Add(ByVal sTitle As String, ByVal sTooltip As String) As ctlListColumn
        Return Add(New ctlListColumn(sTitle, sTooltip))
    End Function

    Public Function Add(ByVal sTitle As String, ByVal sTooltip As String, ByVal Width As Integer) As ctlListColumn
        Return Add(New ctlListColumn(sTitle, sTooltip, Width))
    End Function

    Public Function Add(ByVal sTitle As String, ByVal iWidth As Integer) As ctlListColumn
        Return Add(New ctlListColumn(sTitle, iWidth))
    End Function

    ''' <summary>
    ''' Checks if this collection contains the given column
    ''' </summary>
    ''' <param name="col">The column to search for</param>
    ''' <returns>True if this collection contains the given column, false
    ''' otherwise.</returns>
    Public Function Contains(ByVal col As ctlListColumn) As Boolean
        Return List.Contains(col)
    End Function

    ''' <summary>
    ''' Gets or sets the column at the given index.
    ''' </summary>
    ''' <param name="index">The index at which the column should be retrieved or
    ''' set.</param>
    ''' <exception cref="ArgumentOutOfRangeException">If the given index was less
    ''' than zero or beyond the end of this collection.</exception>
    Default Public Property Item(ByVal index As Integer) As ctlListColumn
        Get
            Return CType(MyBase.List(index), ctlListColumn)
        End Get
        Set(ByVal Value As ctlListColumn)
            MyBase.List(index) = Value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the column in this collection with the given name.
    ''' </summary>
    ''' <param name="name">The name to use for the given list column.
    ''' Note that after adding to this collection, the column's 
    ''' <see cref="ctlListColumn.Name"/> property will be set to that used to
    ''' reference it in this collection.</param>
    Default Public Property Item(ByVal name As String) As ctlListColumn
        Get
            For Each col As ctlListColumn In Me.List
                If col.Name = name Then Return col
            Next
            Return Nothing
        End Get
        Set(ByVal value As ctlListColumn)
            If value Is Nothing Then Return ' Ignore nulls - they're just not worth it.
            value.Name = name
            Dim i As Integer = Me.List.IndexOf(value)
            If i >= 0 Then
                List(i) = value
                Return
            End If
            List.Add(value)
        End Set
    End Property

End Class
