Imports AutomateControls

''' Project  : Automate
''' Class    : ctlHeader
''' 
''' <summary>
''' This class implements a bespoke header bar for the listview like control in 
''' ctlInputsOutputsConditions.
''' </summary>
Friend Class ctlListHeader
    Inherits Control
    Implements System.Runtime.Serialization.ISerializable

    ''' <summary>
    ''' An event that is raised every time the columns are resized.
    ''' </summary>
    ''' <param name="Column">The column being resized.</param>
    Public Event ColumnResizing(ByVal Column As ctlListColumn)

    ''' <summary>
    ''' Event raised after a column has been resized.
    ''' </summary>
    ''' <param name="Column">The column that was resized.</param>
    Public Event ColumnResized(ByVal Column As ctlListColumn)

    ''' <summary>
    ''' An event that is raised every time a column header is clicked.
    ''' </summary>
    Public Event ColumnClicked As System.Windows.Forms.ColumnClickEventHandler

    ''' <summary>
    ''' The column the mouse was over when the mouse button was depressed,
    ''' if any.
    ''' </summary>
    Private mMouseDownColumn As ctlListColumn

    Private mToolTip As ToolTip


    Public Sub New()
        mColumns = New clsColumnCollection(Me)
        MyBase.DoubleBuffered = True
        mToolTip = New ToolTip()

        mColumnTextFormat = New StringFormat()
        mColumnTextFormat.Trimming = StringTrimming.EllipsisCharacter

        mFillColumnIndex = -1
    End Sub

    ''' <summary>
    ''' Private member to store public property OwningListView()
    ''' </summary>
    Private mOwningListView As ctlListView
    ''' <summary>
    ''' The listview which owns this column header, if any.
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
    ''' holds the columns
    ''' </summary>
    Private mColumns As clsColumnCollection

    ''' <summary>
    ''' Gets and sets the columns
    ''' </summary>
    Public Property Columns() As clsColumnCollection
        Get
            Return mColumns
        End Get
        Set(ByVal Value As clsColumnCollection)
            mColumns = Value
        End Set
    End Property


    ''' <summary>
    ''' Private member to store public property ScrollOffset()
    ''' </summary>
    Private mScrollOffset As Integer

    ''' <summary>
    ''' The offset from the left hand side, in pixels due to scrolling.
    ''' When used in conjunction with a ctllistview object for example,
    ''' this keeps the column headers aligned with the columns of the
    ''' listview.
    ''' 
    ''' This will be a value of zero or more, indicating how many pixels
    ''' will be skipped from the left hand side before painting begins.
    ''' </summary>
    ''' <value>.</value>
    Public Property ScrollOffset() As Integer
        Get
            Return mScrollOffset
        End Get
        Set(ByVal value As Integer)
            If Not value = mScrollOffset Then
                mScrollOffset = value
                Me.Invalidate()
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets the 'automate list view item sorter' (catchy) currently working on
    ''' this list header. Note that if this header is orphaned, ie. not currently
    ''' attached to a list view, this will always return null.
    ''' Also, if no sorter is currently set, this will return null.
    ''' </summary>
    Friend ReadOnly Property Sorter() As clsAutomateListViewItemSorter
        Get
            If mOwningListView IsNot Nothing Then
                Return mOwningListView.Sorter
            End If
            Return Nothing
        End Get
    End Property


    ''' <summary>
    ''' Clears all columns from this header.
    ''' </summary>
    ''' <remarks>The rows of the corresponding listview
    ''' must be cleared separately, if desired.</remarks>
    Public Sub Clear()
        Me.Columns.Clear()
    End Sub


    Const FontVerticalOffset As Integer = 3
    Const FontHorizOffset As Integer = 2
    Const IconHorizMargin As Integer = 5
    Const IconVertMargin As Integer = 6
    Const IconWidth As Integer = 8
    Const IconHeight As Integer = 8
    Private mColumnTextFormat As StringFormat


    ''' <summary>
    ''' In the onpaint event we draw the columns using controlpaintstyle
    ''' this makes columns appear in the current windows theme.
    ''' </summary>
    ''' <param name="e"></param>
    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        Dim brBrush As New SolidBrush(Me.ForeColor)
        Dim iLeft As Integer = -1 * Me.ScrollOffset
        Dim iTop As Integer = 0

        'draw each column, with its text
        For Each c As ctlListColumn In mColumns
            ControlPaintStyle.DrawListViewHeader(e.Graphics, New Rectangle(iLeft, iTop, c.Width, Me.Height), c.Hover)

            'draw sort icon
            Dim IconTriangle(2) As Point
            Dim IconLeft As Integer = iLeft + IconHorizMargin
            Dim IconMiddle As Integer = IconLeft + (IconWidth \ 2) + (IconWidth Mod 2)
            Dim IconTop As Integer = iTop + IconVertMargin
            Dim TextLeft As Integer = IconLeft + IconWidth + FontHorizOffset
            Select Case c.SortOrder
                Case SortOrder.Ascending
                    IconTriangle(0) = New Point(iLeft + IconHorizMargin + IconWidth, IconTop + IconHeight)
                    IconTriangle(1) = New Point(IconLeft, IconTop + IconHeight)
                    IconTriangle(2) = New Point(IconMiddle, IconTop)
                Case SortOrder.Descending
                    IconTriangle(0) = New Point(IconLeft + IconWidth, IconTop)
                    IconTriangle(1) = New Point(IconMiddle, IconTop + IconHeight)
                    IconTriangle(2) = New Point(IconLeft, IconTop)
                Case Else
                    TextLeft = iLeft + FontHorizOffset
            End Select
            e.Graphics.FillPolygon(Brushes.DarkGray, IconTriangle)
            Dim Pen1 As Pen = New Pen(SystemColors.ControlDark)
            e.Graphics.DrawLine(New Pen(SystemColors.ControlLightLight), IconTriangle(0), IconTriangle(1))
            e.Graphics.DrawLine(Pen1, IconTriangle(1), IconTriangle(2))
            e.Graphics.DrawLine(Pen1, IconTriangle(2), IconTriangle(0))

            'draw text
            Dim OccupiedWidth As Integer = TextLeft - iLeft
            e.Graphics.DrawString(c.Text, Me.Font, brBrush, New RectangleF(TextLeft, iTop + FontVerticalOffset, c.Width - OccupiedWidth, Me.Font.Height), mColumnTextFormat)
            iLeft += c.Width
        Next

        'fill in the space to the right with an empty header
        If iLeft < Me.ClientSize.Width Then
            ControlPaintStyle.DrawListViewHeader(e.Graphics, New Rectangle(iLeft, iTop, Me.ClientSize.Width - iLeft, Me.ClientSize.Height), False)
        End If

    End Sub

    ''' <summary>
    ''' Gets the width of the specified column header,
    ''' including its text, and any potential sorting
    ''' indicator triangle.
    ''' </summary>
    ''' <param name="Index">The index of the column to be
    ''' measured.</param>
    ''' <returns></returns>
    ''' <remarks>This is the width that would be assigned
    ''' to the column if its Width property were set to
    ''' -2.</remarks>
    Public Function GetColumnWidth(ByVal Index As Integer) As Integer
        If Index < 0 OrElse Index > Me.Columns.Count - 1 Then
            Throw New ArgumentOutOfRangeException(NameOf(Index), My.Resources.ctlListHeader_IndexMustBeAtLeast0AndLessThanTheNumberOfColumns)
        End If

        Dim Column As ctlListColumn = Me.Columns(Index)
        Dim IncludeSortIndicator As Boolean = (Me.OwningListView IsNot Nothing) AndAlso (OwningListView.Sorter IsNot Nothing)

        Dim Width As Integer = 2 * FontHorizOffset 'Twice - once for LHS and once for RHS
        If IncludeSortIndicator Then Width += IconHorizMargin + IconWidth
        Dim g As Graphics = Me.CreateGraphics
        Width += CInt(Math.Ceiling(g.MeasureString(Column.Text, Me.Font, Short.MaxValue, Me.mColumnTextFormat).Width))

        Return Width
    End Function

    ''' <summary>
    ''' Causes the control to repaint itself.
    ''' </summary>
    Public Sub RePaint()
        Me.Invalidate()
    End Sub

    ''' <summary>
    ''' overloads the mousemove handler to capture the the users cursor position and
    ''' if we are resizing, resize the appropriate column, otherwise apply the hover
    ''' effect to the correct column.
    ''' </summary>
    ''' <param name="e"></param>
    Protected Overrides Sub OnMouseMove(ByVal e As MouseEventArgs)
        MyBase.OnMouseMove(e)

        ' splitting will tell us if we are hovering near a column separator
        Dim splitting As Boolean = False
        Dim left As Integer = -ScrollOffset

        For Each c As ctlListColumn In mColumns
            ' Discover if we are currently near to resizing the columns
            If e.X > left - 8 AndAlso e.X < left + 8 Then splitting = True
            ' If a column is currently registered as resizing, set its width and
            ' fire the appropriate event
            If c.Resizing Then
                If e.X - left > 10 Then ' enforces min column width of 10
                    c.Width = e.X - left
                    OnColumnsResized(c, False)
                    'we can only be resizing one column at once so ..
                    Exit For
                End If
                ' Otherwise, if hovering, show the tooltip
            ElseIf e.X > left AndAlso e.X < left + c.Width Then
                If Not c.Hover Then
                    c.Hover = True
                    mToolTip.SetToolTip(Me, c.ToolTip)
                End If
            Else
                c.Hover = False
            End If
            left += c.Width
        Next

        If splitting Then Cursor = Cursors.VSplit Else Cursor = Cursors.Default

        Invalidate()

    End Sub

    ''' <summary>
    ''' Flag to indicate that the last column should be filled.
    ''' This needs to remain even though we have a 'fill column index' which is set
    ''' to the index for back-compatibility. There's a load of designer code out
    ''' there which is setting 'last column autosize' before adding the columns,
    ''' which, with an index, is not really possible.
    ''' </summary>
    Private mFillLastColumn As Boolean

    ''' <summary>
    ''' Property that can turn on and off the auto resizing of the last column.
    ''' Note that this is ignored if the <see cref="FillColumnIndex"/> property is
    ''' set. If it is unset (ie. set to -1) then this flag is checked when resizing.
    ''' </summary>
    ''' <value></value>
    Public Property LastColumnAutoSize() As Boolean
        Get
            ' If no fill column index set, use the flag.
            If mFillColumnIndex = -1 Then Return mFillLastColumn
            ' fill column set - is it the last column?
            Return (mFillColumnIndex = Columns.Count - 1)
        End Get
        Set(ByVal value As Boolean)
            mFillLastColumn = value
            OnColumnsResized(Nothing, True)
        End Set
    End Property

    ''' <summary>
    ''' The index of the column in this header which should be filled.
    ''' </summary>
    Private mFillColumnIndex As Integer

    ''' <summary>
    ''' Gets or sets the index of the column in this header which should be filled
    ''' with available space when resizing.
    ''' This will be -1 if the currently set index is out of range for the columns
    ''' in this header and either '<see cref="LastColumnAutoSize"/>' is set to false
    ''' or there are no columns in this header.
    ''' Otherwise the index of the column index to fill is returned.
    ''' Note that it is possible to set the fill column to an index which is
    ''' currently out of range - it will be ignored until the column collection
    ''' increases enough to include the fill column at which point it will be
    ''' activated.
    ''' </summary>
    Public Property FillColumnIndex() As Integer
        Get
            If mFillColumnIndex < 0 OrElse mFillColumnIndex > Columns.Count Then
                If mFillLastColumn Then Return Columns.Count - 1
                Return -1
            End If
            Return mFillColumnIndex
        End Get
        Set(ByVal value As Integer)
            mFillColumnIndex = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the column in this header which should be resized to fill the
    ''' remaining space in the listview when the listview is resized.
    ''' This will be null if there are no columns, or the
    ''' <see cref="FillColumnIndex"/> is out of range of this header's column
    ''' collection.
    ''' </summary>
    ''' <exception cref="ArgumentException">If an attempt is made to set the fill
    ''' column to a column not owned by this header. That's just crazy, man.
    ''' </exception>
    Public Property FillColumn() As ctlListColumn
        Get
            Dim ix As Integer = Me.FillColumnIndex
            If ix >= 0 Then Return Columns(ix)
            Return Nothing
        End Get
        Set(ByVal value As ctlListColumn)

            ' If we're setting it to null, set it to -1
            If value Is Nothing Then
                mFillColumnIndex = -1

                ' Ensure the column being set is owned by this header.
            ElseIf value.OwningHeader Is Me Then
                mFillColumnIndex = value.Index

                ' Not owned by this header... don't know what that's supposed to achieve.
            Else
                Throw New ArgumentException(
                 String.Format(My.Resources.ctlListHeader_GivenColumnDoesNotBelongToThisHeaderMe0OwningHeader1, Me.Name, value.OwningHeader.Name))
            End If
        End Set
    End Property

    Public Function TotalOfColumnWidths() As Integer
        Dim itotal As Integer = 0
        For Each c As ctlListColumn In mColumns
            itotal += c.Width
        Next
        Return itotal
    End Function

    ''' <summary>
    ''' raise an event to tell the parent control that the columns have been resized
    ''' </summary>
    Private Sub OnColumnsResized(ByVal ResizedColumn As ctlListColumn, ByVal ResizingComplete As Boolean)
        UpdateLastColumn()
        Invalidate()

        If Not ResizingComplete Then
            RaiseEvent ColumnResizing(ResizedColumn)
        Else
            RaiseEvent ColumnResized(ResizedColumn)
        End If
    End Sub

    ''' <summary>
    ''' Updates the width of the last column.
    ''' </summary>
    ''' <remarks>Relevant only if the  LastColumnAutoSize
    ''' property is set to true. The last column will be 
    ''' expanded to fill the width of the parent conrol
    ''' if needs be.</remarks>
    Public Sub UpdateLastColumn()
        Dim col As ctlListColumn = Me.FillColumn
        If col IsNot Nothing Then

            Dim total As Integer = Me.TotalOfColumnWidths
            Dim effvWidth As Integer = Me.Width
            Dim owner As ctlListView = Me.OwningListView
            If owner IsNot Nothing Then
                effvWidth = owner.EffectiveSize.Width
                If effvWidth = 0 Then effvWidth = Me.Width
            End If

            ' Fill column - diff is effective width + scroll offset - total of column widths
            Dim newWidth As Integer = col.Width + (effvWidth + Me.ScrollOffset - total)
            If newWidth > MinimumColumnWidth Then col.Width = newWidth
        End If
    End Sub

    ''' <summary>
    ''' Causes column widths to be reevaluated.
    ''' </summary>
    Public Sub UpdateColumns()
        If Me.Columns.Count > 0 Then
            Me.OnColumnsResized(Me.Columns(0), True)
        End If
    End Sub

    ''' <summary>
    ''' Private member to store public property MinimumColumnWidth()
    ''' </summary>
    Private miMinimumColumnWidth As Integer = 100
    ''' <summary>
    ''' The minimum width to apply to columns, in pixels. Defaults to 100
    ''' </summary>
    ''' <value></value>
    Public Property MinimumColumnWidth() As Integer
        Get
            Return miMinimumColumnWidth
        End Get
        Set(ByVal value As Integer)
            miMinimumColumnWidth = value
        End Set
    End Property

    ''' <summary>
    ''' When we leave the control, remove the hover property.
    ''' </summary>
    ''' <param name="e"></param>
    Protected Overrides Sub OnMouseLeave(ByVal e As System.EventArgs)
        For Each c As ctlListColumn In mColumns
            c.Hover = False
        Next
        Me.Invalidate()
    End Sub

    ''' <summary>
    ''' Detect which column needs to be resized
    ''' </summary>
    ''' <param name="e"></param>
    Protected Overrides Sub OnMouseDown(ByVal e As System.Windows.Forms.MouseEventArgs)
        Dim iLeft As Integer = -1 * Me.ScrollOffset
        Me.mMouseDownColumn = Nothing

        For Each c As ctlListColumn In mColumns
            iLeft += c.Width
            If e.X > iLeft - 8 AndAlso e.X < iLeft + 8 Then
                MyBase.Capture = True
                c.Resizing = True
                Exit For
            Else
                c.Resizing = False
                'remember which header had the mouse down so that we can raise click event
                If e.X > iLeft - c.Width AndAlso e.X < iLeft Then
                    mMouseDownColumn = c
                End If
            End If
        Next
    End Sub

    ''' <summary>
    ''' Forget which column needs to be resized, we can only resize the columns
    ''' when the mouse is down.
    ''' </summary>
    ''' <param name="e"></param>
    Protected Overrides Sub OnMouseUp(ByVal e As System.Windows.Forms.MouseEventArgs)
        Dim iLeft As Integer = -1 * Me.ScrollOffset
        Dim MouseUpColumn As ctlListColumn = Nothing
        Dim c As ctlListColumn
        Dim MouseUpColumnIndex As Integer

        MyBase.Capture = False
        Dim Resizing As Boolean

        For i As Integer = 0 To mColumns.Count - 1
            c = mColumns(i)
            If c.Resizing Then
                RaiseEvent ColumnResized(c)
                Resizing = True
            End If
            c.Resizing = False
            If e.X > iLeft AndAlso e.X < iLeft + c.Width Then
                MouseUpColumn = c
                MouseUpColumnIndex = i
            End If
            iLeft += c.Width
        Next

        If Not Resizing Then
            If MouseUpColumn Is mMouseDownColumn Then
                Try
                    RaiseEvent ColumnClicked(Me, New ColumnClickEventArgs(MouseUpColumnIndex))
                Catch ex As Exception
                    UserMessage.Show(My.Resources.ctlListHeader_UnexpectedError, ex)
                End Try
            End If
        End If

        mMouseDownColumn = Nothing
    End Sub

    Private Sub ctlListHeader_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.MouseLeave
        If Me.mToolTip IsNot Nothing Then
            Me.mToolTip.Hide(Me)
        End If
    End Sub


    Private Sub ctlListHeader_SizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.SizeChanged
        Me.OnColumnsResized(Nothing, True)
    End Sub

    Public Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext) Implements System.Runtime.Serialization.ISerializable.GetObjectData
        info.AddValue("Columns", Me.Columns)
        info.AddValue("MinimumColumnWidth", Me.MinimumColumnWidth)
        info.AddValue("LastColumnAutoSize", Me.LastColumnAutoSize)
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        Me.ResumeLayout(False)

    End Sub
End Class

