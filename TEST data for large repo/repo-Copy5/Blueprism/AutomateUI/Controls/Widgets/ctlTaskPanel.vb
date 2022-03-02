''' <summary>
''' A user control to allow navigation between the main areas of the product
''' </summary>
Public Class ctlTaskPanel : Inherits UserControl

#Region " Class-scope Declarations "

    ''' <summary>
    ''' The margin separating an item from the bounds of its neighbours.
    ''' </summary>
    Private Const ItemMargin As Integer = 0

    ''' <summary>
    ''' The margin at the top of the panel to use before rendering the icons
    ''' </summary>
    Private Const TopMargin As Integer = 24

    ''' <summary>
    ''' Class to represent a single task panel item
    ''' </summary>
    Private Class TaskPanelItem

        ''' <summary>
        ''' The padding (in pixels) for each task panel item
        ''' </summary>
        Friend Const Padding As Integer = 12

        ' The task panel hosting this item
        Private mOuter As ctlTaskPanel

        ' The handler to be called for this item
        Private mHandler As EventHandler

        ' The colour of the highlight box
        Private mHighlightColor As Color

        ''' <summary>
        ''' Gets or sets whether this item is selected or not
        ''' </summary>
        Public Property Selected() As Boolean

        ''' <summary>
        ''' Gets or sets whether this item is hot (ie. being hovered) or not
        ''' </summary>
        Public Property Hot As Boolean

        ''' <summary>
        ''' Gets or sets the name of this item
        ''' </summary>
        Public Property Name() As String

        ''' <summary>
        ''' Gets or sets the icon for this item
        ''' </summary>
        Public Property Icon() As Image

        ''' <summary>
        ''' Gets or sets the icon to show for a disabled task
        ''' </summary>
        Public Property IconDisabled As Image

        ''' <summary>
        ''' Gets or sets the icon to show for a hot task
        ''' </summary>
        Public Property IconHot As Image

        ''' <summary>
        ''' The effective icon to display for this task panel item, given its current
        ''' state. If the more specific icon is not set, this will always return the
        ''' <see cref="Icon"/> value
        ''' </summary>
        Public ReadOnly Property EffectiveIcon As Image
            Get
                ' Check our state and return the correct icon, falling back to the
                ' default icon if the appropriate icon is not set
                If Not Enabled Then Return If(IconDisabled, Icon)
                If Hot Then Return If(IconHot, Icon)
                Return Icon
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the location of this item
        ''' </summary>
        Public Property Location() As Point

        ''' <summary>
        ''' Creates a new task panel item
        ''' </summary>
        ''' <param name="outer">The hosting task panel</param>
        ''' <param name="taskName">The name of the task</param>
        ''' <param name="taskIcon">The icon representing the task</param>
        ''' <param name="taskHandler">The handler for the task</param>
        Public Sub New(outer As ctlTaskPanel,
                       taskName As String,
                       taskIcon As Image,
                       taskIconHot As Image,
                       taskIconDisabled As Image,
                       taskEnabled As Boolean,
                       taskHandler As EventHandler)
            mOuter = outer
            Selected = False
            Hot = False
            Name = taskName
            Icon = taskIcon
            IconHot = taskIconHot
            IconDisabled = taskIconDisabled
            Enabled = taskEnabled
            mHandler = taskHandler
            mHighlightColor = Color.Empty
        End Sub

        ''' <summary>
        ''' The colour to use to highlight a task
        ''' </summary>
        Public Property HighlightColor As Color
            Get
                Return If(mHighlightColor = Color.Empty,
                          mOuter.HighlightColor,
                          mHighlightColor)
            End Get
            Set(value As Color)
                mHighlightColor = value
            End Set
        End Property

        ''' <summary>
        ''' Gets whether this task panel item is clickable or not (ie. whether it has
        ''' an attached handler)
        ''' </summary>
        Public ReadOnly Property Clickable() As Boolean
            Get
                Return mHandler IsNot Nothing
            End Get
        End Property

        Public Property Enabled As Boolean

        ''' <summary>
        ''' Gets the X coordinate of this item
        ''' </summary>
        Public ReadOnly Property X As Integer
            Get
                Return Location.X
            End Get
        End Property

        ''' <summary>
        ''' Gets the Y coordinate of this item
        ''' </summary>
        Public ReadOnly Property Y As Integer
            Get
                Return Location.Y
            End Get
        End Property

        ''' <summary>
        ''' Gets the bounding rectangle of this item
        ''' </summary>
        Public ReadOnly Property Bounds() As Rectangle
            Get
                Return New Rectangle(Location, Size)
            End Get
        End Property

        ''' <summary>
        ''' Gets the size of this item
        ''' </summary>
        Public ReadOnly Property Size As Size
            Get
                Return New Size(Width, Height)
            End Get
        End Property

        ''' <summary>
        ''' Gets the width of this item
        ''' </summary>
        Public ReadOnly Property Width As Integer
            Get
                Return mOuter.Width
            End Get
        End Property

        ''' <summary>
        ''' Gets the height of this item
        ''' </summary>
        Public ReadOnly Property Height As Integer
            Get
                Return (2 * Padding) + Icon.Size.Height
            End Get
        End Property

        ''' <summary>
        ''' Paints this item using the given graphics context
        ''' </summary>
        ''' <param name="g">The graphics context to use to draw this item</param>
        Friend Sub DrawItem(g As Graphics)
            Dim rect As Rectangle = Bounds
            If Selected AndAlso Clickable Then
                ' Expand the rect so we only render the top and bottom lines
                Dim highlightRect As Rectangle = rect
                highlightRect.Inflate(1, 1)
                Using b As New SolidBrush(HighlightColor)
                    g.FillRectangle(b, highlightRect)
                End Using
                g.DrawRectangle(SystemPens.Highlight, highlightRect)
            End If

            ' Find the centre of the bounds for the icon
            Dim img As Image = EffectiveIcon
            g.DrawImage(img, New Rectangle(
                rect.X + ((rect.Width - img.Width) \ 2),
                rect.Y + ((rect.Height - img.Height) \ 2),
                img.Width, img.Height)
            )
        End Sub

        ''' <summary>
        ''' Fires the handler held by this item
        ''' </summary>
        Public Sub Invoke()
            Try
                mHandler(Me, EventArgs.Empty)
            Catch e As Exception
                UserMessage.Show(My.Resources.TaskPanelItem_Error, e)
            End Try
        End Sub

    End Class

#End Region

#Region " Member Variables "

    ' The collection of items being displayed in this panel
    Private mItems As ICollection(Of TaskPanelItem)

    ' Flag indicating if we are within a BeginUpdate()...EndUpdate() block
    Private mUpdating As Boolean

    Private mHoverItem As TaskPanelItem

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a blank task panel
    ''' </summary>
    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        SetStyle(ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.UserPaint Or
                 ControlStyles.DoubleBuffer, True)

        mItems = New List(Of TaskPanelItem)

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The color of the border line.
    ''' </summary>
    ''' <value></value>
    <Browsable(True), Category("Appearance"),
     DefaultValue(GetType(Color), "ControlLightLight"),
     Description("The colour of the border line")>
    Public Property BorderColor() As Color = SystemColors.ControlLightLight

    ''' <summary>
    ''' The colour of the item highlight box
    ''' </summary>
    <Browsable(True), Category("Appearance"),
     DefaultValue(GetType(Color), "194, 224, 255"),
     Description("The colour to use to highlight a task")>
    Public Property HighlightColor As Color = Color.FromArgb(194, 224, 255)

    ''' <summary>
    ''' The task panel items held on this control
    ''' </summary>
    Private ReadOnly Property Items() As ICollection(Of TaskPanelItem)
        Get
            Return mItems
        End Get
    End Property

#End Region

#Region " Event Override Methods "

    ''' <summary>
    ''' Handles the painting for this task panel
    ''' </summary>
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        ' Ensure that we're no longer set as updating
        mUpdating = False

        ' Carry on
        MyBase.OnPaint(e)

        Dim g As Graphics = e.Graphics

        g.Clear(Me.BackColor)
        Using p As New Pen(Me.BorderColor)
            g.DrawLine(p, Width - 1, 0, Width - 1, Me.Height)
        End Using

        For Each item As TaskPanelItem In mItems
            item.DrawItem(g)
        Next

    End Sub

    ''' <summary>
    ''' Handles the mouse being moved over this panel
    ''' </summary>
    Protected Overrides Sub OnMouseMove(ByVal e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        CoolAllItems()
        Dim item As TaskPanelItem = HeatItem(e.Location)
        If item Is Nothing Then mTooltip.Hide(Me)
        If item IsNot mHoverItem Then
            mHoverItem = item
            mTooltipTimer.Start()
        End If
        Invalidate()
    End Sub

    ''' <summary>
    ''' Handles the tooltip timer ticking over
    ''' </summary>
    Private Sub HandleTick() Handles mTooltipTimer.Tick
        If mHoverItem Is Nothing Then
            mTooltip.Hide(Me)
        Else
            mTooltip.SetToolTip(Me, mHoverItem.Name)
        End If
        mTooltipTimer.Stop()
    End Sub

    ''' <summary>
    ''' Handles the mouse leaving this panel
    ''' </summary>
    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        CoolAllItems()
        mTooltip.Hide(Me)
        Invalidate()
    End Sub

    ''' <summary>
    ''' Handles the mouse being clicked on this panel
    ''' </summary>
    Protected Overrides Sub OnMouseClick(e As MouseEventArgs)
        MyBase.OnMouseClick(e)
        For Each item As TaskPanelItem In mItems
            If item.Bounds.Contains(e.Location) Then
                If item.Enabled Then
                    item.Invoke()
                    Exit For
                End If
            End If
        Next
        Invalidate()
    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Sets a task panel item as 'Hot' and a given location within this panel
    ''' </summary>
    ''' <param name="p">The point at which to set the 'hot' item</param>
    ''' <returns>The 'heated' task panel item if one was found at the given point,
    ''' or null if no such item was found</returns>
    Private Function HeatItem(ByVal p As Point) As TaskPanelItem
        For Each item As TaskPanelItem In mItems
            If item.Bounds.Contains(p) Then
                item.Hot = True
                Return item
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Removes the 'Hot' state from all task panel items.
    ''' </summary>
    Private Sub CoolAllItems()
        For Each item As TaskPanelItem In mItems
            item.Hot = False
        Next
    End Sub

    ''' <summary>
    ''' Lays out the child tasks in this panel
    ''' </summary>
    Private Sub LayoutItems()
        Dim y As Integer = TopMargin + TaskPanelItem.Padding
        For Each item As TaskPanelItem In mItems
            item.Location = New Point(0, y)
            y += (item.Height + (2 * ItemMargin))
        Next
        Me.Invalidate()
    End Sub

    ''' <summary>
    ''' Checks if this panel has a task item with the given name
    ''' </summary>
    ''' <param name="name">The name of the required task item</param>
    ''' <returns>True if this task panel contains a task item with the given name;
    ''' False otherwise</returns>
    Public Function HasItem(ByVal name As String) As Boolean
        For Each item As TaskPanelItem In mItems
            If item.Name = name Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' Sets an item enabled or disabled by its name
    ''' </summary>
    ''' <param name="name">The name of the item whose enabled state should change.
    ''' </param>
    ''' <param name="enabled">True to enable the item; False to disable it.</param>
    Public Sub SetItemEnabled(ByVal name As String, ByVal enabled As Boolean)
        For Each item As TaskPanelItem In mItems
            If item.Name = name Then
                item.Enabled = enabled
                Exit Sub
            End If
        Next
    End Sub

    ''' <summary>
    ''' Sets an item selected, ensuring that all other items are unselected
    ''' </summary>
    ''' <param name="name">The name of the item which should be selected</param>
    ''' <remarks>Note that if the name does not match an item in this task panel, it
    ''' will effectively set all items to be unselected.</remarks>
    Public Sub SetSelectedItem(name As String)
        For Each item As TaskPanelItem In mItems
            item.Selected = (item.Name = name)
        Next
        Invalidate()
    End Sub

    ''' <summary>
    ''' Gets the name of the selected item within this task panel, or null if no item
    ''' is currently selected.
    ''' </summary>
    ''' <returns>The name of the selected item, or null if there are no selected
    ''' items</returns>
    Public Function GetSelectedItem() As String
        For Each item As TaskPanelItem In mItems
            If item.Selected Then Return item.Name
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Clears all of the items in this task panel
    ''' </summary>
    Public Sub Clear()
        Items.Clear()
        If Not mUpdating Then Invalidate()
    End Sub

    ''' <summary>
    ''' Adds a task to this task panel, with no 'hot' or 'disabled' icon variant
    ''' </summary>
    ''' <param name="name">The name of the task</param>
    ''' <param name="ico">The icon representing the task</param>
    ''' <param name="handler">The event handler for the task</param>
    Public Sub Add(name As String, ico As Image, enabled As Boolean, handler As EventHandler)
        Add(name, ico, Nothing, Nothing, enabled, handler)
    End Sub

    ''' <summary>
    ''' Adds a task to this task panel, with no 'hot' icon variant
    ''' </summary>
    ''' <param name="name">The name of the task</param>
    ''' <param name="ico">The icon representing the task</param>
    ''' <param name="icoDisabled">The disabled variant of the icon</param>
    ''' <param name="enabled">True if the task should be enabled; False otherwise.
    ''' </param>
    ''' <param name="handler">The event handler for the task</param>
    Public Sub Add(name As String, ico As Image, icoDisabled As Image, enabled As Boolean, handler As EventHandler)
        Add(name, ico, Nothing, icoDisabled, enabled, handler)
    End Sub

    ''' <summary>
    ''' Adds a task to this task panel
    ''' </summary>
    ''' <param name="name">The name of the task</param>
    ''' <param name="ico">The icon representing the task</param>
    ''' <param name="icoHot">The hot variant of the icon</param>
    ''' <param name="icoDisabled">The disabled variant of the icon</param>
    ''' <param name="enabled">True if the task should be enabled; False otherwise.
    ''' </param>
    ''' <param name="handler">The event handler for the task</param>
    Public Sub Add(name As String, ico As Image, icoHot As Image, icoDisabled As Image, enabled As Boolean, handler As EventHandler)
        Items.Add(New TaskPanelItem(Me, name, ico, icoHot, icoDisabled, enabled, handler))
        If Not mUpdating Then LayoutItems()
    End Sub

    ''' <summary>
    ''' Instructs this panel to begin an update - it will hold off laying out and
    ''' invalidating the items until a corresponding <see cref="EndUpdate"/> is
    ''' called, or until it is being painted whichever comes first
    ''' </summary>
    Public Sub BeginUpdate()
        mUpdating = True
    End Sub

    ''' <summary>
    ''' Instructs this panel to end an update - at this point it will lay out its
    ''' child tasks and invalidate itself for repainting.
    ''' </summary>
    Public Sub EndUpdate()
        mUpdating = False
        LayoutItems()
    End Sub

#End Region

End Class
