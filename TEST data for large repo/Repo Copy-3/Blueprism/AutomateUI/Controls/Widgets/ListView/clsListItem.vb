Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Extensions
Imports BluePrism.AutomateProcessCore

''' <summary>
''' Class which represents a single item, usually corresponding to a single
''' <see cref="clsProcessValue">Process Value</see> within a
''' <see cref="clsListRow">List Row</see>
''' </summary>
Friend Class clsListItem
    Implements IDisposable

#Region " Static members and constants "

    ''' <summary>
    ''' Utility brush used when drawing text that is designed to appear
    ''' as if it were disabled
    ''' </summary>
    Public Shared ReadOnly DisabledTextBrush As Brush = SystemBrushes.GrayText

    ''' <summary>
    ''' Utility brush used when drawing normal text 
    ''' </summary>
    Public Shared ReadOnly NormalTextBrush As Brush = SystemBrushes.WindowText

    ''' <summary>
    ''' A dictionary of brushes suitable for use for painting data associated
    ''' with a particular data type
    ''' </summary>
    Public Shared ReadOnly DataColourTextBrushes As IDictionary(Of DataType, Brush) = _
     GenerateBrushMap()

    ''' <summary>
    ''' Details of how text will be displayed
    ''' </summary>
    Private Shared ReadOnly StrFormat As StringFormat = CreateStringFormat()

#End Region

#Region " Static methods "

    ''' <summary>
    ''' Creates the string format object used in all instances of this class.
    ''' </summary>
    ''' <returns>A StringFormat object for use in all instances of this class.
    ''' </returns>
    Private Shared Function CreateStringFormat() As StringFormat
        Dim sf As New StringFormat(StringFormatFlags.NoWrap)
        sf.Trimming = StringTrimming.EllipsisCharacter
        sf.Alignment = StringAlignment.Near
        Return sf
    End Function

    ''' <summary>
    ''' Creates a readonly dictionary mapping a colour brush against the datatype
    ''' that the colour represents.
    ''' </summary>
    ''' <returns>A dictionary of Brush keyed on DataType to use for all instances
    ''' of this class.</returns>
    Private Shared Function GenerateBrushMap() As IDictionary(Of DataType, Brush)
        Dim map As New Dictionary(Of DataType, Brush)
        For Each dti As clsDataTypeInfo In clsProcessDataTypes.GetAll()
            map(dti.Value) = _
             New SolidBrush(DataItemColour.GetDataItemColor(dti.Value))
        Next
        Return GetReadOnly.IDictionary(map)
    End Function

    ''' <summary>
    ''' Gets all available data types, in a format acceptable to the AvailableValues
    ''' property.
    ''' </summary>
    Public Shared Function GetDataTypeAvailableValues() As Dictionary(Of String, String)
        Dim map As New Dictionary(Of String, String)
        For Each dti As clsDataTypeInfo In clsProcessDataTypes.GetAll()
            map(dti.Name) = clsDataTypeInfo.GetLocalizedFriendlyName(dti.Value) 'dti.FriendlyName
        Next
        Return map
    End Function

#End Region

#Region " Public memvars "

    ''' <summary>
    ''' The dictionary of available values to be presented. May be null.
    ''' Key is stored in result, value is displayed on screen
    ''' </summary>
    Public AvailableValues As Dictionary(Of String, String)

    ''' <summary>
    ''' When true, the item will be drawn with a colourful border.
    ''' </summary>
    Public Highlighted As Boolean

    ''' <summary>
    ''' The outer color to use when highlighting this item.
    ''' Used when the Highlighted member is true.
    ''' </summary>
    Public HighlightOuterColour As Color

    ''' <summary>
    ''' The inner color to use when highlighting this item.
    ''' Used when the Highlighted member is true.
    ''' </summary>
    Public HighlightInnerColour As Color

#End Region

#Region " Private memvars "

    ' The value represented by this item.
    Private mValue As clsProcessValue

    ' The brush used when painting the background.
    Private mBackgroundBrush As Brush

    ' The brush used when painting the background, when
    ' this item is selected.
    Private mSelectedBackgroundBrush As Brush

    ' Private member to store public property TextBrush()
    Private mTextBrush As Brush

    ' Reference to the row owning this item
    Private mParentRow As clsListRow

    ' The point at which the mouse was depressed on this control, as reported
    ' by the OnMouseDown method.
    Private mMouseDownPoint As Point

    ' The point at which the mouse was released on this control, as reported
    ' by the OnMouseUp method.
    Private mMouseUpPoint As Point

    ' Member for IDisposable to detect redundant calls.
    Private mDisposedValue As Boolean = False

    ' Flag indicating multiline text support
    Private mMultiline As Boolean

    ' The font to use to draw this item
    Private mFont As Font

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty list item.
    ''' </summary>
    Public Sub New()
        Me.New(Nothing, DirectCast(Nothing, clsProcessValue))
    End Sub

    ''' <summary>
    ''' Creates a new list item, set as part of the given list row.
    ''' </summary>
    ''' <param name="parent">The list row which contains this item.</param>
    Public Sub New(ByVal parent As clsListRow)
        Me.New(parent, DirectCast(Nothing, clsProcessValue))
    End Sub

    ''' <summary>
    ''' Creates a new list item as part of an existing list row with a string value
    ''' </summary>
    ''' <param name="parent">The list row which contains this item.</param>
    ''' <param name="strValue">The value that this item represents.</param>
    Public Sub New(ByVal parent As clsListRow, ByVal strValue As String)
        Me.New(parent, New clsProcessValue(strValue))
    End Sub

    ''' <summary>
    ''' Creates a new list item, set as part of the given list row, and set to
    ''' represent the given value.
    ''' </summary>
    ''' <param name="parent">The list row which contains this item.</param>
    ''' <param name="value">The value that this item represents.</param>
    Public Sub New(ByVal parent As clsListRow, ByVal value As clsProcessValue)
        mParentRow = parent
        mValue = value
        mBackgroundBrush = Brushes.White ' *Not* to be disposed of. That would be bad
        mSelectedBackgroundBrush = _
         New SolidBrush(AutomateControls.ColourScheme.Default.ListViewSelectedRowBackground)
        mTextBrush = NormalTextBrush
        mMultiline = True
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The point at which the mouse was depressed on this control, as reported
    ''' by the OnMouseDown method.
    ''' </summary>
    Protected Property MouseDownPoint() As Point
        Get
            Return mMouseDownPoint
        End Get
        Set(ByVal value As Point)
            mMouseDownPoint = value
        End Set
    End Property

    ''' <summary>
    ''' The point at which the mouse was released on this control, as reported
    ''' by the OnMouseUp method.
    ''' </summary>
    Protected Property MouseUpPoint() As Point
        Get
            Return mMouseUpPoint
        End Get
        Set(ByVal value As Point)
            mMouseUpPoint = value
        End Set
    End Property

    ''' <summary>
    ''' The brush to use when drawing text
    ''' </summary>
    Public Property TextBrush() As Brush
        Get
            Return mTextBrush
        End Get
        Set(ByVal value As Brush)
            mTextBrush = value
        End Set
    End Property

    ''' <summary>
    ''' The font set in this list item. If not explicitly set, this will inherit
    ''' the font set in the owning ListView control.
    ''' </summary>
    Public Property Font() As Font
        Get
            If mFont IsNot Nothing Then Return mFont
            If mParentRow IsNot Nothing AndAlso mParentRow.Owner IsNot Nothing Then _
             Return mParentRow.Owner.Font
            Return Nothing
        End Get
        Set(ByVal value As Font)
            mFont = value
        End Set
    End Property

    ''' <summary>
    ''' The value represented by this list item
    ''' </summary>
    Public Property Value() As clsProcessValue
        Get
            Return mValue
        End Get
        Set(ByVal value As clsProcessValue)
            mValue = value
        End Set
    End Property

    ''' <summary>
    ''' Flag indicating if this item supports multiline text or not. This only has
    ''' any meaning if this list item represents a text value.
    ''' The default is true - ie. the list item supports multiline.
    ''' </summary>
    <DefaultValue(True)> _
    Public Property Multiline() As Boolean
        Get
            Return mMultiline
        End Get
        Set(ByVal value As Boolean)
            mMultiline = value
        End Set
    End Property

    ''' <summary>
    ''' The text that is used to display this list item.
    ''' </summary>
    Protected Overridable ReadOnly Property Text() As String
        Get
            If mValue Is Nothing Then Return ""
            Return mValue.FormattedValue
        End Get
    End Property

#End Region

#Region " Member methods "

    ''' <summary>
    ''' Draws the item onto the supplied graphics canvas.
    ''' </summary>
    ''' <param name="g">The graphics object to use when painting.</param>
    ''' <param name="bounds">The bounding rectangle of the
    ''' area available to the item, in coordinates understood by the
    ''' graphics object.</param>
    Public Sub Draw(ByVal g As Graphics, ByVal bounds As Rectangle)
        If Highlighted Then
            If mParentRow IsNot Nothing AndAlso mParentRow.Owner IsNot Nothing Then
                Using p As New Pen(HighlightOuterColour)
                    Dim highlightBounds As Rectangle = bounds

                    'Draw outer rectangle
                    highlightBounds.Inflate(-1, -1)
                    g.DrawRectangle(p, highlightBounds)

                    'Draw inner rectangle in different colour
                    highlightBounds.Inflate(-1, -1)
                    p.Color = HighlightInnerColour
                    g.DrawRectangle(p, highlightBounds)
                End Using
            End If
        End If

        DrawString(g, bounds, Text)

    End Sub

    ''' <summary>
    ''' Draws the specified text representing this item in the given bounds, on
    ''' the given graphics context.
    ''' </summary>
    ''' <param name="g">The graphics context to use to draw the string.</param>
    ''' <param name="bounds">The bounds within which the string should be
    ''' drawn.</param>
    ''' <param name="txt">The text to draw</param>
    Private Sub DrawString(ByVal g As Graphics, ByVal bounds As Rectangle, ByVal txt As String)

        If String.IsNullOrEmpty(txt) Then Return ' Nothing to do? Then do nothing...

        'Centre the text vertically by measuring its height
        'Ensure the string is short enough for the measure string operation. The limit is
        '2046 for bidi strings see: https://msdn.microsoft.com/en-us/library/windows/desktop/ms535829%28v=vs.85%29.aspx
        Dim textHeight =
            CInt(g.MeasureString(txt.Truncate(2046), Font, bounds.Width, StrFormat).Height)

        Dim vOffset As Integer = Math.Max(0, bounds.Height - textHeight) \ 2
        bounds.Y += vOffset
        bounds.Height -= vOffset

        'Offset from the left, somewhat
        Const hOffset As Integer = 3
        bounds.X += hOffset
        bounds.Width -= hOffset

        'Ensure the string is short enough for the drawstring operation. The limit
        'by experimentation is 32000, 32001 throws an exception.
        g.DrawString(txt.Truncate(32000), Font, mTextBrush, bounds, StrFormat)
    End Sub


    ''' <summary>
    ''' Gets the preferred width of an item. This allows for items of certain
    ''' types to have an appropriate width for their corresponding edit control
    ''' (eg the timespan edit control is very wide).
    ''' </summary>
    Public Overridable Function GetPreferredSize(ByVal proposedSize As Size) As Size
        If mValue IsNot Nothing Then
            Select Case mValue.DataType
                Case DataType.date
                    Return New Size(110, ctlListView.DefaultRowHeight)
                Case DataType.datetime
                    Return New Size(165, ctlListView.DefaultRowHeight)
                Case DataType.flag
                    Return New Size(110, ctlListView.DefaultRowHeight)
                Case DataType.password
                    Return New Size(80, ctlListView.DefaultRowHeight)
                Case DataType.time
                    Return New Size(100, ctlListView.DefaultRowHeight)
                Case DataType.timespan
                    Return New Size(310, ctlListView.DefaultRowHeight)
                Case DataType.number
                    Return New Size(80, ctlListView.DefaultRowHeight)
                Case Else
                    Return New Size(150, ctlListView.DefaultRowHeight)
            End Select
        Else
            Return proposedSize
        End If
    End Function

    ''' <summary>
    ''' Informs the item that it has been mouse-downed at this point. This
    ''' corresponds to a click on the item when there is no current edit row. Eg
    ''' if the item is mimicking a checkbox by rendering it, then it needs to
    ''' process this click.
    ''' </summary>
    ''' <param name="Location">The point, in coordinates relative to the local
    ''' item, at which the click took place</param>
    Friend Overridable Sub OnMouseDown(ByVal Location As Point)
        mMouseDownPoint = Location
    End Sub

    ''' <summary>
    ''' Informs the item that it has been mouse-uped at this point. This
    ''' corresponds to a click on the item when there is no current edit row. Eg
    ''' if the item is mimicking a checkbox by rendering it, then it needs to
    ''' process this click.
    ''' </summary>
    ''' <param name="Location">The point, in coordinates relative to the local
    ''' item, at which the click took place</param>
    Friend Overridable Sub OnMouseUp(ByVal Location As Point)
        mMouseUpPoint = Location
    End Sub

#End Region

#Region " IDisposable Support "

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not mDisposedValue Then
            If disposing Then
                If mSelectedBackgroundBrush IsNot Nothing Then _
                 mSelectedBackgroundBrush.Dispose()
                If mFont IsNot Nothing Then mFont.Dispose()
            End If
            mDisposedValue = True
        End If
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

End Class
