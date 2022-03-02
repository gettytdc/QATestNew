Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports Size = System.Drawing.Size
Imports BluePrism.BPCoreLib.Collections
Imports WindowsSystemParams = System.Windows.SystemParameters
Imports AutomateUI.Controls.Charts.Resources
Imports AutomateUI.Controls.Charts

Public Class ctlTileView
    Inherits UserControl
    Implements IDisposable

    'Available dashboard modes
    Private Enum Modes
        Create
        Edit
        View
        Preview
    End Enum
    Private mode As Modes
    Dim disposed As Boolean = False

    'Custom tile holder properties
    Public Shared IDProperty As DependencyProperty =
        DependencyProperty.RegisterAttached("ID", GetType(Guid), GetType(ctlTileView))
    Public Shared ReadOnly SizeProperty As DependencyProperty =
        DependencyProperty.RegisterAttached("Size", GetType(Size), GetType(ctlTileView))

    'Event for requesting tile refresh
    Public Delegate Sub TilesRefreshEventHandler(tilesAdded As Dictionary(Of Guid, Size),
                                                 tilesNotAdded As List(Of Guid),
                                                 tilesRemoved As List(Of Guid))
    Public Event RefreshTilesEvent As TilesRefreshEventHandler

    'Image sources
    Private zoomImage As BitmapSource
    Private unZoomImage As BitmapSource

    'Dynamic controls
    Private zoomView As Border
    Private hoverImage As Image
    Private tileMenu As ContextMenu
    Private mnuSmall As MenuItem
    Private mnuMedium As MenuItem
    Private mnuLarge As MenuItem
    Private mnuRemove As MenuItem

    'Mouse starting point (for drag & drop)
    Private mouseStart As Point

    'List if tile IDs on this board
    Private mTileIDs As New List(Of Guid)()

    'Returns true if dashboard is in create mode
    Public ReadOnly Property IsNewDashboard As Boolean
        Get
            Return (mode = Modes.Create)
        End Get
    End Property

#Region "Constructors"

    Public Sub New()


        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        zoomImage = GetImage(BluePrism.Images.ToolImages.Zoom_In_2_16x16)
        unZoomImage = GetImage(BluePrism.Images.ToolImages.Zoom_Out_2_16x16)
        CreateDynamicControls()
        AddHandler WindowsSystemParams.StaticPropertyChanged, AddressOf AccessibilitySettings_HighContrastChanged
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    ' Protected implementation of Dispose pattern.
    Protected Overridable Sub Dispose(disposing As Boolean)
        If disposed Then Return

        If disposing Then
            RemoveHandler WindowsSystemParams.StaticPropertyChanged, AddressOf AccessibilitySettings_HighContrastChanged
            RemoveHandler hoverImage.MouseLeftButtonUp, AddressOf hoverButton_MouseLeftButtonUp
            RemoveHandler mnuSmall.Click, AddressOf mnuOption_Click
            RemoveHandler mnuMedium.Click, AddressOf mnuOption_Click
            RemoveHandler mnuLarge.Click, AddressOf mnuOption_Click
            RemoveHandler mnuRemove.Click, AddressOf mnuOption_Click

        End If

        disposed = True
    End Sub


    ''' <summary>
    ''' Creates other WPF controls.
    ''' </summary>
    Private Sub CreateDynamicControls()
        'Create a zoomed tile holder (a border to contain the tile holder)
        zoomView = New Border()
        zoomView.Background = Brushes.White
        Grid.SetZIndex(zoomView, 2)

        'Create a hover button (for zooming)
        hoverImage = New Image()
        hoverImage.Width = 16
        hoverImage.Height = 16
        hoverImage.Margin = New Thickness(5)
        hoverImage.HorizontalAlignment = System.Windows.HorizontalAlignment.Right
        hoverImage.VerticalAlignment = System.Windows.VerticalAlignment.Top
        hoverImage.Cursor = Cursors.Hand
        AddHandler hoverImage.MouseLeftButtonUp, AddressOf hoverButton_MouseLeftButtonUp
        KeyboardNavigation.SetTabNavigation(hoverImage, KeyboardNavigationMode.None)
        Grid.SetZIndex(hoverImage, 3)

        'Tile context menu
        tileMenu = New ContextMenu()
        mnuSmall = New MenuItem()
        mnuSmall.Header = My.Resources.ctlTileView_Small
        AddHandler mnuSmall.Click, AddressOf mnuOption_Click
        tileMenu.Items.Add(mnuSmall)

        mnuMedium = New MenuItem()
        mnuMedium.Header = My.Resources.ctlTileView_Medium
        AddHandler mnuMedium.Click, AddressOf mnuOption_Click
        tileMenu.Items.Add(mnuMedium)

        mnuLarge = New MenuItem()
        mnuLarge.Header = My.Resources.ctlTileView_Large
        AddHandler mnuLarge.Click, AddressOf mnuOption_Click
        tileMenu.Items.Add(mnuLarge)

        tileMenu.Items.Add(New Separator())

        mnuRemove = New MenuItem()
        mnuRemove.Header = My.Resources.ctlTileView_Remove
        AddHandler mnuRemove.Click, AddressOf mnuOption_Click
        tileMenu.Items.Add(mnuRemove)

        Dim mnuRemoveAll As New MenuItem()
        mnuRemoveAll.Header = My.Resources.ctlTileView_RemoveAllTiles
        AddHandler mnuRemoveAll.Click, AddressOf mnuOption_Click
        tileMenu.Items.Add(mnuRemoveAll)
    End Sub

#End Region

#Region "Public methods"

    ''' <summary>
    ''' Initialises the dashboard for previewing a single tile.
    ''' </summary>
    ''' <param name="tile">The tile being previewed</param>
    Public Sub InitForPreview(tile As UIElement)

        Me.Content = contentPanel

        'Clear the board and remove all but the first column
        mode = Modes.Preview
        ClearTiles()
        If tileGrid.ColumnDefinitions.Count > 1 Then
            tileGrid.ColumnDefinitions.RemoveRange(1, tileGrid.ColumnDefinitions.Count - 1)
        End If

        'Create a tile holder and add the tile
        AddTileHolder(0, Guid.Empty, New Size(1, 1))
        RefreshTile(Guid.Empty, tile)

        'Remove scrollbar to allow tile to fill available space
        scrollView.Content = Nothing
        HideHintText()
    End Sub

    ''' <summary>
    ''' Initialises the dashboard for viewing.
    ''' </summary>
    ''' <param name="tileList">The list of tiles on the dashboard</param>
    Public Sub InitForView(tileList As Dictionary(Of Guid, Size))

        'Clear board and create holders for the passed tiles
        mode = Modes.View
        If hoverImage.Tag IsNot Nothing Then
            ToggleZoomView(CType(hoverImage.Tag, Viewbox))
        End If
        ClearTiles()
        Dim i As Integer = 0
        For Each tile As KeyValuePair(Of Guid, Size) In tileList
            AddTileHolder(i, tile.Key, tile.Value)
            i += 1
        Next

        'Bring the tile grid into view
        If tileGrid.Children.Count > 0 Then
            HideHintText()
            ReDrawTiles()
        Else
            ShowHintText()
        End If
    End Sub

    ''' <summary>
    ''' Initialises the dashboard for creating. A tile list can also be specified
    ''' here, used when creating a dashboard as a copy of another.
    ''' </summary>
    ''' <param name="tileList">Optional list of tiles.</param>
    Public Sub InitForCreate(Optional tileList As Dictionary(Of Guid, Size) = Nothing)

        'Clear board and bring the hint text into view
        mode = Modes.Create
        If hoverImage.Tag IsNot Nothing Then
            ToggleZoomView(CType(hoverImage.Tag, Viewbox))
        End If
        ClearTiles()
        If tileList IsNot Nothing Then
            Dim i As Integer = 0
            For Each tile As KeyValuePair(Of Guid, Size) In tileList
                AddTileHolder(i, tile.Key, tile.Value)
                i += 1
            Next
            ReDrawTiles()
        Else
            ShowHintText()
        End If
    End Sub

    ''' <summary>
    ''' Initialises the dashboard for editing.
    ''' </summary>
    Public Sub InitForEdit()
        'Wait for something to happen
        mode = Modes.Edit
        If hoverImage.Tag IsNot Nothing Then
            ToggleZoomView(CType(hoverImage.Tag, Viewbox))
        End If
        If tileGrid.Children.Count = 0 Then ShowHintText()
    End Sub

    ''' <summary>
    ''' Instructs the dashboard to rerefresh the passed tile.
    ''' </summary>
    ''' <param name="id">Tile ID</param>
    ''' <param name="tile">Tile control to display</param>
    Public Sub RefreshTile(id As Guid, tile As UIElement)
        Dim tileHolder As Viewbox
        Dim i As Integer = GetIndexOf(id)
        If i >= 0 Then
            tileHolder = CType(tileGrid.Children(i), Viewbox)
        Else
            tileHolder = CType(zoomView.Child, Viewbox)
        End If

        If tileHolder Is Nothing Then Return

        Dim border As New Border()
        border.BorderThickness = New Thickness(1)
        border.BorderBrush = Brushes.White
        border.Child = tile
        tileHolder.Child = border

    End Sub

    ''' <summary>
    ''' Returns the collection of tiles (IDs and sizes) that are curently displayed.
    ''' </summary>
    ''' <returns>Tile collection</returns>
    Public Function GetTiles() As Dictionary(Of Guid, Size)
        'Return current tile details
        Dim tileList As New Dictionary(Of Guid, Size)()
        For Each el As UIElement In tileGrid.Children
            If Not TypeOf (el) Is Viewbox Then Continue For
            tileList.Add(CType(el.GetValue(IDProperty), Guid), CType(el.GetValue(SizeProperty), Size))
        Next

        Return tileList
    End Function

#End Region

#Region "Display"

    ''' <summary>
    ''' Clear tiles and grid rows (leave the columns in place as they are fixed)
    ''' </summary>
    Private Sub ClearTiles()
        tileGrid.Children.Clear()
        tileGrid.RowDefinitions.Clear()
        mTileIDs.Clear()
    End Sub

    ''' <summary>
    ''' Shows the default hint text (when there are no tiles on the board).
    ''' Setting the grid backround to transparent allows the hint text behind to
    ''' be visible, whilst allowing the grid to respond to drag events
    ''' </summary>
    Private Sub ShowHintText()
        tileGrid.Background = Brushes.Transparent
        hint.Inlines.Clear()
        If mode = Modes.View Then
            hintText.Child = New ctlDashboardPlaceholder()
        Else
            Dim TileLibrary = My.Resources.ctlTileView_TileLibrary
            Dim resxString = My.Resources.ctlTileView_CreateADashboardByDraggingTilesFromTheTileLibraryAndDroppingThemOntoThisCanvas
            Dim i = resxString.IndexOf(TileLibrary)
            If i <> -1 Then
                hint.Inlines.Add(New Run(resxString.Substring(0, i)))
                hint.Inlines.Add(New Run(TileLibrary) With {.FontStyle = FontStyles.Italic})
                hint.Inlines.Add(New Run(resxString.Substring(i + TileLibrary.Length, resxString.Length - i - TileLibrary.Length)))
            Else
                hint.Inlines.Add(New Run(resxString))
            End If
            hintText.Child = hint
        End If
    End Sub

    ''' <summary>
    ''' Hides the default hint text (by setting the grid background).
    ''' </summary>
    Private Sub HideHintText()
        tileGrid.Background = Brushes.White
        hint.Inlines.Clear()
    End Sub

    ''' <summary>
    ''' Places a zoom button at the top right of the tile holder.
    ''' </summary>
    ''' <param name="tileHolder">The tile holder</param>
    Private Sub ShowZoomButton(tileHolder As Viewbox)
        hoverImage.Tag = tileHolder
        hoverImage.Source = zoomImage
        hoverImage.ToolTip = My.Resources.ctlTileView_ClickToZoomTile

        tileGrid.Children.Add(hoverImage)
        Grid.SetRow(hoverImage, Grid.GetRow(tileHolder))
        Grid.SetColumn(hoverImage, Grid.GetColumn(tileHolder) + Grid.GetColumnSpan(tileHolder) - 1)
    End Sub

    ''' <summary>
    ''' Places an unzoom button at the top right corner of the currently zoomed tile.
    ''' </summary>
    Private Sub ShowUnZoomButton()
        hoverImage.Source = unZoomImage
        hoverImage.ToolTip = My.Resources.ctlTileView_ClickToRestoreTile
    End Sub

    ''' <summary>
    ''' Adds a blank tile holder to the grid for the passed tile ID, at the specified
    ''' location.
    ''' </summary>
    ''' <param name="index">Index to add tile holder at</param>
    ''' <param name="id">The tile ID</param>
    ''' <param name="s">The tile size</param>
    Private Sub AddTileHolder(index As Integer, id As Guid, s As Size)
        'Add holder for tileID
        Dim tileHolder = New Viewbox()
        tileHolder.SetValue(IDProperty, id)
        tileHolder.SetValue(SizeProperty, s)
        tileHolder.AllowDrop = True
        tileHolder.ContextMenu = tileMenu
        AddHandler tileHolder.ContextMenuOpening, AddressOf tileHolder_ContextMenuOpening
        AddHandler tileHolder.MouseEnter, AddressOf tileHolder_MouseEnter
        AddHandler tileHolder.MouseLeave, AddressOf tileHolder_MouseLeave
        AddHandler tileHolder.PreviewMouseLeftButtonDown, AddressOf tileHolder_PreviewMouseLeftButtonDown
        AddHandler tileHolder.PreviewMouseMove, AddressOf tileHolder_PreviewMouseMove
        AddHandler tileHolder.DragEnter, AddressOf tileHolder_DragEnter
        AddHandler tileHolder.DragLeave, AddressOf tileHolder_DragLeave
        AddHandler tileHolder.Drop, AddressOf tileHolder_Drop
        KeyboardNavigation.SetTabNavigation(tileHolder, KeyboardNavigationMode.None)
        tileGrid.Children.Insert(index, tileHolder)
        mTileIDs.Add(id)

        'Hide hint text once tile has been added in create/edit mode
        If BoardIsEditable() AndAlso tileGrid.Children.Count = 1 Then
            HideHintText()
        End If
    End Sub

    ''' <summary>
    ''' Adjusts the size of the passed tile holder to span 1, 2 or 3 columns.
    ''' </summary>
    ''' <param name="tileHolder">The tile holder</param>
    ''' <param name="s">The new size</param>
    Private Sub ResizeTileHolder(tileHolder As Viewbox, s As Size)
        Dim tileList As New Dictionary(Of Guid, Size)()

        tileHolder.SetValue(SizeProperty, s)
        tileList.Add(CType(tileHolder.GetValue(IDProperty), Guid), s)

        RaiseEvent RefreshTilesEvent(tileList, Nothing, Nothing)
        ReDrawTiles()
    End Sub

    ''' <summary>
    ''' Removes all tiles form the dashboard.
    ''' </summary>
    Private Sub RemoveAllTileHolders()
        RaiseEvent RefreshTilesEvent(Nothing, Nothing, mTileIDs)
        ClearTiles()
        ShowHintText()
    End Sub

    ''' <summary>
    ''' Removes the tile holder containing the tile with the passed id.
    ''' </summary>
    ''' <param name="id">The tile ID</param>
    Private Sub RemoveTileHolder(id As Guid)
        'Remove tile holder from dashboard
        tileGrid.Children.RemoveAt(GetIndexOf(id))
        mTileIDs.Remove(id)
        RaiseEvent RefreshTilesEvent(Nothing, Nothing, New List(Of Guid)({id}))
        ReDrawTiles()

        'Show hint text if last tile removed in create/edit mode
        If BoardIsEditable() AndAlso tileGrid.Children.Count = 0 Then
            ShowHintText()
        End If
    End Sub

    ''' <summary>
    ''' Redraws the dashboard grid. The cells are cleared and re-drawn from the tiles
    ''' and sizes of the child elements.
    ''' </summary>
    Private Sub ReDrawTiles()
        'Clear tile board
        tileGrid.RowDefinitions.Clear()
        If tileGrid.Children.Count = 0 Then Return

        'Add first row
        Dim row As RowDefinition = New RowDefinition()
        row.Height = GridLength.Auto
        tileGrid.RowDefinitions.Add(row)
        Dim x As Integer = 0
        Dim y As Integer = 0

        'Move tiles to correct cells
        For Each el As UIElement In tileGrid.Children
            'If Not TypeOf (el) Is Viewbox Then Continue For

            Dim s As Size = CType(el.GetValue(SizeProperty), Size)
            If x + s.Width > 3 Then
                'Tile won't fit on so add new row
                row = New RowDefinition()
                row.Height = GridLength.Auto
                tileGrid.RowDefinitions.Add(row)
                x = 0
                y += 1
            End If
            Grid.SetRow(el, y)
            Grid.SetColumn(el, x)
            Grid.SetColumnSpan(el, s.Width)
            x += s.Width
        Next
    End Sub

    ''' <summary>
    ''' Toggles the zoom/unzoom button.
    ''' </summary>
    ''' <param name="tileHolder"></param>
    Private Sub ToggleZoomView(tileHolder As Viewbox)

        If TypeOf (zoomView.Child) Is Viewbox Then
            'Move tile holder from zoom viewer back into grid
            zoomView.Child = Nothing
            tileGrid.Children.Insert(CInt(zoomView.Tag), tileHolder)
            zoomView.Tag = Nothing

            'Remove zoom viewer & hover button
            contentPanel.Children.Remove(zoomView)
            contentPanel.Children.Remove(hoverImage)
            hoverImage.Tag = Nothing

            'Bring scroll view back
            Me.Content = scrollView
            scrollView.Content = contentPanel
            Dim brdr As Border = CType(tileHolder.Child, Border)
            brdr.BorderBrush = Brushes.White
        Else
            Dim border As Border = CType(tileHolder.Child, Border)
            Dim chart As IBaseChart = TryCast(border.Child, IBaseChart)
            If Not chart?.HasChartData Then Return

            'Move tile holder from the grid to zoom viewer
            '(recording the index to allow it to be restored later)
            zoomView.Tag = tileGrid.Children.IndexOf(tileHolder)
            tileGrid.Children.Remove(tileHolder)
            zoomView.Child = tileHolder

            'Move hover button from grid to outer container
            tileGrid.Children.Remove(hoverImage)
            contentPanel.Children.Add(zoomView)
            contentPanel.Children.Add(hoverImage)

            'Set-up hover button for unzooming
            ShowUnZoomButton()
            hoverImage.Focus()

            'Remove the scroll view (as we want the zoom viewer to fill the
            'visible screen, rather than the whole tile grid - which can scroll)
            scrollView.Content = Nothing
            Me.Content = contentPanel
        End If
    End Sub

    ''' <summary>
    ''' In View mode this highlights the current tile with a thin border and a zoom
    ''' button. In Create/edit mode (when dragging something over a tile) a thicker
    ''' border is used to emphasise where the thing being dragged will be dropped.
    ''' </summary>
    ''' <param name="tileHolder">The tile holder to focus</param>
    Private Sub FocusTile(tileHolder As Viewbox)
        If BoardIsEditable() Then
            tileHolder.Cursor = Cursors.Hand
            tileHolder.ToolTip = My.Resources.ctlTileView_DragTileToChangeOrder
            Dim brdr As Border = CType(tileHolder.Child, Border)
            brdr.BorderBrush = Brushes.DimGray
        ElseIf mode = Modes.View Then
            'Add hover button to top-right corner (if not already there)
            If Not TypeOf (hoverImage.Tag) Is Viewbox Then
                Dim brdr As Border = CType(tileHolder.Child, Border)
                brdr.BorderBrush = Brushes.DimGray
                Dim chart As IBaseChart = TryCast(brdr.Child, IBaseChart)
                If chart?.HasChartData Then
                    ShowZoomButton(tileHolder)
                End If
            End If
        End If
    End Sub

    ''' <summary>
    ''' Removes the highlighting effect applied when the tile given focus.
    ''' </summary>
    ''' <param name="tileHolder">The tileholder to unfocus</param>
    Private Sub UnFocusTile(tileHolder As Viewbox)
        If BoardIsEditable() Then
            tileHolder.Cursor = Cursors.Arrow
            tileHolder.ToolTip = Nothing
            Dim brdr As Border = CType(tileHolder.Child, Border)
            brdr.BorderBrush = Brushes.White
        ElseIf mode = Modes.View Then
            Dim brdr As Border = CType(tileHolder.Child, Border)
            'Remove hover button (unless mouse is directly over it)
            If Not hoverImage.IsMouseDirectlyOver Then
                If tileGrid.Children.Contains(hoverImage) Then
                    hoverImage.Tag = Nothing
                    tileGrid.Children.Remove(hoverImage)
                    If Not tileHolder.Parent Is zoomView Then
                        brdr.BorderBrush = Brushes.White
                    End If
                End If
            End If
            Dim chart As IBaseChart = TryCast(brdr.Child, IBaseChart)
            If Not chart?.HasChartData Then
                brdr.BorderBrush = Brushes.White
            End If
        End If
    End Sub


#End Region

#Region "Drag and drop handling"

    ''' <summary>
    ''' Handles left mouse down over a tile. Working in conjunction with the
    ''' MouseMove event this initiates dragging an existing tile (Create/edit modes).
    ''' </summary>
    Private Sub tileHolder_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        If BoardIsEditable() Then
            'Record start position (for drag & drop)
            mouseStart = e.GetPosition(Nothing)
        End If
    End Sub

    ''' <summary>
    ''' Handles mouse moving over a tile.
    ''' </summary>
    Private Sub tileHolder_PreviewMouseMove(sender As Object, e As MouseEventArgs)
        If BoardIsEditable() Then
            'Start drag event if mouse has moved sufficiently
            Dim mousePosition As Point = e.GetPosition(Nothing)
            Dim diff As Vector = mousePosition - mouseStart

            If e.LeftButton = MouseButtonState.Pressed AndAlso
             (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance OrElse
             Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance) Then
                Dim tile As Viewbox = CType(sender, Viewbox)
                Dim dragData As New DataObject("System.Windows.Controls.Viewbox", tile)
                DragDrop.DoDragDrop(tile, dragData, DragDropEffects.Move)
            End If
        End If
    End Sub

    ''' <summary>
    ''' Handles dragging something over an existing tile. The tile is highlighted by
    ''' increasing the border thickness.
    ''' </summary>
    Private Sub tileHolder_DragEnter(sender As Object, e As DragEventArgs)
        If CanDrop(e.Data) Then
            FocusTile(CType(sender, Viewbox))
            e.Effects = DragDropEffects.Move
        Else
            e.Effects = DragDropEffects.None
        End If
    End Sub

    ''' <summary>
    ''' Handles dragging something away from a tile. The tile highlighting is
    ''' removed.
    ''' </summary>
    Private Sub tileHolder_DragLeave(sender As Object, e As DragEventArgs)
        UnFocusTile(CType(sender, Viewbox))
    End Sub

    ''' <summary>
    ''' Handles the drop event fron an individual tile (i.e. the user has dragged
    ''' something onto this tile).
    ''' </summary>
    Private Sub tileHolder_Drop(sender As Object, e As DragEventArgs)
        'Only allow drop in create/edit mode
        If Not BoardIsEditable() Then Return

        e.Handled = True
        Dim tileOver As Viewbox = CType(sender, Viewbox)
        UnFocusTile(tileOver)

        Dim position As Integer = tileGrid.Children.IndexOf(tileOver)

        Dim tileIDs As ICollection(Of Guid) = ExtractIds(e.Data)
        If tileIDs IsNot Nothing Then
            'Drag source is the Tile Library (tile addition)
            InsertTiles(position, tileIDs)
        ElseIf e.Data.GetDataPresent(GetType(Viewbox)) Then
            'Drag source is the Dashboard (tile re-ordering)
            Dim draggedTile As Viewbox = CType(e.Data.GetData(GetType(Viewbox)), Viewbox)
            If Not tileOver Is draggedTile Then
                MoveTile(position, draggedTile)
            End If
        End If
        ReDrawTiles()
    End Sub

    ''' <summary>
    ''' Handles drop event from the grid (i.e. user has dragged something onto an
    ''' empty part of the canvas.
    ''' </summary>
    Private Sub tileGrid_Drop(sender As Object, e As DragEventArgs)
        'Only allow drop in create/edit mode
        If Not BoardIsEditable() Then Return

        'Determine where the objects are being dropped (usually this would be after
        'any existing tiles, but there could also be blank cells on the board i.e. a
        'wide tile may have to be moved to the next row if there is not enough room
        'on the current row).
        Dim position As Integer

        'Find row under current mouse position
        Dim nextTile As Viewbox = Nothing
        Dim row As Integer = GetRowAtPoint(e.GetPosition(tileGrid))
        If row >= 0 Then
            'Get contents of first cell on next row
            nextTile = GetTileHolderAtCell(row + 1, 0)
        End If
        If nextTile IsNot Nothing Then
            position = tileGrid.Children.IndexOf(nextTile)
        Else
            position = tileGrid.Children.Count
        End If

        'Determine what is being dropped (could be tiles from the Tile Library or
        'the re-ordering of an existing tile)
        Dim tileIDs As ICollection(Of Guid) = ExtractIds(e.Data)
        If tileIDs IsNot Nothing Then
            InsertTiles(position, tileIDs)
        ElseIf e.Data.GetDataPresent(GetType(Viewbox)) Then
            Dim draggedTile As Viewbox = CType(e.Data.GetData(GetType(Viewbox)), Viewbox)
            MoveTile(position, draggedTile)
        End If
        ReDrawTiles()
    End Sub

    ''' <summary>
    ''' Attempts to extract a collection of IDs from the given data object.
    ''' </summary>
    ''' <param name="obj">The data object from where to draw the IDs. The IDs are
    ''' expected to be either 1) a <see cref="List(Of Guid)"/> or 2) a semi-colon
    ''' separated string with <see cref="Guid"/>s in each element.</param>
    ''' <returns>A collection of GUIDs representing IDs found in the supplied
    ''' data object or null if no IDs could be found in the given data object.
    ''' </returns>
    Private Function ExtractIds(obj As IDataObject) As ICollection(Of Guid)
        If obj.GetDataPresent(GetType(List(Of Guid))) Then
            Return CType(obj.GetData(GetType(List(Of Guid))), ICollection(Of Guid))
        ElseIf obj.GetDataPresent(GetType(String)) Then
            Dim stringData As String = CStr(obj.GetData(GetType(String)))
            Dim ids As New List(Of Guid)()
            For Each entry As String In stringData.Split(";"c)
                Dim id As Guid
                If Guid.TryParse(entry, id) Then ids.Add(id)
            Next
            If ids.Count > 0 Then Return ids
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' Checks the passed drag/drop payload to see if it is something we can drop.
    ''' </summary>
    ''' <param name="data">Payload data</param>
    ''' <returns>True if it can be dropped onto the dashboard</returns>
    Private Function CanDrop(data As IDataObject) As Boolean
        Dim ids As ICollection(Of Guid) = ExtractIds(data)
        If ids IsNot Nothing Then
            If Not CollectionUtil.ContainsAll(mTileIDs, ids) Then
                'We have some tileIDs that don't exist on the dashboard
                Return True
            End If
        ElseIf data.GetDataPresent(GetType(Viewbox)) Then
            'We have a tile
            Return True
        End If
        Return False
    End Function

    ''' <summary>
    ''' Handles insertion of new tiles dragged frm Tile Library.
    ''' </summary>
    ''' <param name="posn">Position to start insertion at</param>
    ''' <param name="tileIDs">The list of tile IDs to insert</param>
    Private Sub InsertTiles(posn As Integer, tileIDs As ICollection(Of Guid))
        'Process incoming list and add and tiles that are not already present
        Dim tileList As New Dictionary(Of Guid, Size)()
        Dim skippedIDs As New List(Of Guid)
        For Each id As Guid In tileIDs
            If mTileIDs.Contains(id) Then
                skippedIDs.Add(id)
            Else
                Dim s As New Size(1, 1)
                AddTileHolder(posn, id, s)
                tileList.Add(id, s)
                posn += 1
            End If
        Next
        'Request tile refreshes
        RaiseEvent RefreshTilesEvent(tileList, skippedIDs, Nothing)
    End Sub

    ''' <summary>
    ''' Handles the dragging of an existing tile to new position.
    ''' </summary>
    ''' <param name="posn">Position to insert at</param>
    ''' <param name="draggedTile">The tile being dragged</param>
    Private Sub MoveTile(posn As Integer, draggedTile As UIElement)
        Dim lastPosn As Integer = tileGrid.Children.Count - 1
        'Remove dragged tile from it's cell
        tileGrid.Children.Remove(draggedTile)
        'Insert dragged tile before or after tile
        tileGrid.Children.Insert(
            CInt(IIf(posn > lastPosn, lastPosn, posn)), draggedTile)
    End Sub

#End Region

#Region "Tile holder mouse events"
    ''' <summary>
    ''' Handles mouse moving over a tile.
    ''' </summary>
    Private Sub tileHolder_MouseEnter(sender As Object, e As RoutedEventArgs)
        FocusTile(CType(sender, Viewbox))
    End Sub

    ''' <summary>
    ''' Handles mouse moving away from a tile.
    ''' </summary>
    Private Sub tileHolder_MouseLeave(sender As Object, e As RoutedEventArgs)
        UnFocusTile(CType(sender, Viewbox))
    End Sub

    ''' <summary>
    ''' Handles click of tile zoom button.
    ''' </summary>
    Private Sub hoverButton_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
        If hoverImage.Tag IsNot Nothing Then
            ToggleZoomView(CType(hoverImage.Tag, Viewbox))
        End If
    End Sub

#End Region

#Region "Tile holder context menu"

    ''' <summary>
    ''' Handles opening of tile context menu (Create/edit modes only).
    ''' </summary>
    Private Sub tileHolder_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs)
        If BoardIsEditable() Then
            Dim tileHolder As Viewbox = CType(sender, Viewbox)
            tileMenu.Tag = tileHolder
            Dim s As Size = CType(tileHolder.GetValue(SizeProperty), Size)
            Select Case s.Width
                Case 1
                    mnuSmall.IsChecked = True
                    mnuMedium.IsChecked = False
                    mnuLarge.IsChecked = False
                Case 2
                    mnuSmall.IsChecked = False
                    mnuMedium.IsChecked = True
                    mnuLarge.IsChecked = False
                Case 3
                    mnuSmall.IsChecked = False
                    mnuMedium.IsChecked = False
                    mnuLarge.IsChecked = True
            End Select
            Return
        End If
        e.Handled = True
    End Sub

    ''' <summary>
    ''' Handles selection of an option from the tile context menu
    ''' </summary>
    Private Sub mnuOption_Click(sender As Object, e As RoutedEventArgs)
        Dim mnuOption As MenuItem = CType(sender, MenuItem)
        Dim tileHolder As Viewbox = CType(CType(mnuOption.Parent, ContextMenu).Tag, Viewbox)

        If mnuOption Is mnuSmall Then
            ResizeTileHolder(tileHolder, New Size(1, 1))
        ElseIf mnuOption Is mnuMedium Then
            ResizeTileHolder(tileHolder, New Size(2, 1))
        ElseIf mnuOption Is mnuLarge Then
            ResizeTileHolder(tileHolder, New Size(3, 1))
        ElseIf mnuOption Is mnuRemove Then
            RemoveTileHolder(CType(tileHolder.GetValue(IDProperty), Guid))
        Else
            RemoveAllTileHolders()
        End If
    End Sub

#End Region

#Region "Utilities"

    ''' <summary>
    ''' Determines whether the dasboard is editable or not.
    ''' </summary>
    ''' <returns>True if dashboard is in create/edit mode</returns>
    Private Function BoardIsEditable() As Boolean
        Return (mode = Modes.Create OrElse mode = Modes.Edit)
    End Function

    ''' <summary>
    ''' Returns the index of the tile with the specified ID. If the tile could not be
    ''' located then -1 is returned.
    ''' </summary>
    ''' <param name="id">The tile ID</param>
    ''' <returns>The index of the passed tile (or -1)</returns>
    Private Function GetIndexOf(id As Guid) As Integer
        For Each el As UIElement In tileGrid.Children
            If TypeOf (el) Is Viewbox AndAlso CType(el.GetValue(IDProperty), Guid) = id Then
                Return tileGrid.Children.IndexOf(el)
            End If
        Next
        Return -1
    End Function

    ''' <summary>
    ''' Returns the row of the cell at the passed point. If the point is
    ''' not within the confines of the grid then -1 is returned.
    ''' </summary>
    ''' <param name="mouse">The point to test</param>
    ''' <returns>Row index</returns>
    Private Function GetRowAtPoint(mouse As Point) As Integer
        Dim row As Integer = -1
        Dim height As Double = 0

        For Each r As RowDefinition In tileGrid.RowDefinitions
            If mouse.Y < height Then Exit For
            row += 1
            height += r.ActualHeight
        Next
        If mouse.Y >= height Then row = -1

        Return row
    End Function

    ''' <summary>
    ''' Returns the tile holder contained within the passed grid cell (row, col), or
    ''' nothing if the cell is empty. Note that this needs to take into account tiles
    ''' that span more than one column.
    ''' </summary>
    ''' <param name="row">The row index</param>
    ''' <param name="col">The column index</param>
    ''' <returns>The tile holder (or nothing)</returns>
    Private Function GetTileHolderAtCell(row As Integer, col As Integer) As Viewbox
        If row < 0 OrElse col < 0 Then Return Nothing

        For Each el As UIElement In tileGrid.Children
            If TypeOf (el) Is Viewbox AndAlso Grid.GetRow(el) = row Then
                Dim tileColumn As Integer = Grid.GetColumn(el)
                Select Case Grid.GetColumnSpan(el)
                    Case 1
                        If tileColumn = col Then Return CType(el, Viewbox)
                    Case 2
                        If tileColumn = col OrElse tileColumn + 1 = col Then _
                            Return CType(el, Viewbox)
                    Case 3
                        If tileColumn = col OrElse tileColumn + 1 = col OrElse
                            tileColumn + 2 = col Then Return CType(el, Viewbox)
                End Select
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Converts WPF bitmap source from WinForms bitmap
    ''' </summary>
    ''' <param name="bmp">WinForms Bitmap</param>
    ''' <returns>WPF bitmap source</returns>
    Private Function GetImage(bmp As System.Drawing.Bitmap) As BitmapSource
        Return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
             bmp.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty,
             BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height))
    End Function

    Private Sub AccessibilitySettings_HighContrastChanged(ByVal sender As Object, ByVal args As PropertyChangedEventArgs)
        If args.PropertyName.Equals(NameOf(WindowsSystemParams.HighContrast)) Then
            Dim ishighcontrast = WindowsSystemParams.HighContrast
            Dim contrastTheme = If(ishighcontrast, ThemeType.HighContrast， ThemeType.Light)
            Theme.LoadThemeType(contrastTheme)
        End If
    End Sub
#End Region

End Class
