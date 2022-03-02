Namespace Operations
    ''' <summary>
    ''' Provides methods for interacting with the mouse cursor
    ''' </summary>
    Friend Interface IMouseOperationsProvider
        ''' <summary>
        ''' Presses the left mouse button at the given co-ordinates.
        ''' </summary>
        ''' <param name="x">The x screen co-ordinate to start the drag operation at.</param>
        ''' <param name="y">The y screen co-ordinate to start the drag operation at.</param>
        Sub DragFrom(x As Integer, y As Integer)

        ''' <summary>
        ''' Releases the left mouse button at the given co-ordinates.
        ''' </summary>
        ''' <param name="x">The x screen co-ordinate to finish the drag operation at.</param>
        ''' <param name="y">The y screen co-ordinate to finish the drag operation at.</param>
        Sub DropAt(x As Integer, y As Integer)

        ''' <summary>
        ''' Presses and releases the given mouse button at the given co-ordinates.
        ''' </summary>
        ''' <param name="x">The x screen co-ordinate to click at.</param>
        ''' <param name="y">The y screen co-ordinate to click at.</param>
        ''' <param name="doubleClick">if set to <c>true</c> then the mouse will be clicked twice.</param>
        ''' <param name="button">The button.</param>
        Sub ClickAt(
                x As Integer,
                y As Integer,
                doubleClick As Boolean,
                button As MouseButton)

        ''' <summary>
        ''' Parses the given string to a mouse button
        ''' </summary>
        ''' <param name="buttonString">The button string.</param>
        ''' <returns>The parsed value</returns>
        Function ParseMouseButton(ByVal buttonString As String) As MouseButton
    End Interface
End Namespace