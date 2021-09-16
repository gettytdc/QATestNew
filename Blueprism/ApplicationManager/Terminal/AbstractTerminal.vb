Public MustInherit Class AbstractTerminal
    Implements IDisposable

    ''' <summary>
    ''' This method is obsolete, use Launch or Attach instead. Provided for backwards
    ''' compatability this method connects to the specified host or session.
    ''' </summary>
    ''' <param name="SessionProfile">The path to a file on disk, representing the
    ''' desired session. This value must be appropriate to the type of target terminal,
    ''' which means that the callee must know about the type. Not relevant for all
    ''' terminal types, however at least one of either SessionProfile or
    ''' SessionShortName must be supplied in each case.</param>
    ''' <param name="SessionShortName">A letter from A..Z representing the session ID.
    ''' Not relevant for all terminal types - see above.</param>
    ''' <param name="sErr">On failure, contains an error message.</param>
    ''' <returns>Returns True if successful, False otherwise..</returns>
    <Obsolete> _
    Public MustOverride Function ConnectToHostOrSession(ByVal SessionProfile As String, ByVal SessionShortName As String, ByRef sErr As String) As Boolean

    ''' <summary>
    ''' This method is obsolete, use Detach or Terminate instead. Provided for
    ''' backwards compatability this method disconnects from the target.
    ''' </summary>
    ''' <remarks>It is safe to do this at any time,
    ''' even if no session is connected. However, the return
    ''' value will not necessarly be true in such circumstances.</remarks>
    ''' <param name="sErr">Carries back an error message in the
    ''' event of an error. Relevant only when the return
    ''' value is false.</param>
    ''' <returns>Returns true on success.</returns>
    <Obsolete> _
    Public MustOverride Function DisconnectFromHost(ByRef sErr As String) As Boolean

    ''' <summary>
    ''' Launch the terminal emulator
    ''' </summary>
    ''' <param name="sessionProfile">The path to a file on disk, representing the
    ''' desired session. This value must be appropriate to the type of target terminal,
    ''' which means that the callee must know about the type. Not relevant for all
    ''' terminal types, however at least one of either SessionProfile or
    ''' SessionShortName must be supplied in each case.</param>
    ''' <param name="sErr">On failure, contains an error message.</param>
    Public MustOverride Function Launch(sessionProfile As String, ByRef sErr As String) As Boolean

    ''' <summary>
    ''' Attach to an existing terminal emulator session.
    ''' </summary>
    ''' <param name="SessionShortName">A letter from A..Z representing the session ID.
    ''' Not relevant for all terminal types - see above.</param>
    ''' <param name="sErr">On failure, contains an error message.</param>
    Public MustOverride Function Attach(sessionShortName As String, ByRef sErr As String) As Boolean

    ''' <summary>
    ''' Dttach the terminal emulator session.
    ''' </summary>
    ''' <param name="sErr">On failure, contains an error message.</param>
    Public MustOverride Function Detach(ByRef sErr As String) As Boolean

    ''' <summary>
    ''' Terminate the terminal emulator.
    ''' </summary>
    ''' <param name="sErr">On failure, contains an error message.</param>
    Public MustOverride Function Terminate(ByRef sErr As String) As Boolean

    ''' <summary>
    ''' Gets the text from the specified cells.
    ''' </summary>
    ''' <param name="StartRow">The start row of the first cell, indexed
    ''' from 1.</param>
    ''' <param name="StartColumn">The start column of the first cell, indexed
    ''' from 1.</param>
    ''' <param name="Length">The number of cells from which to
    ''' read text</param>
    ''' <param name="Value">Carries back the text read from the terminal.</param>
    ''' <param name="sErr">Carries back an error message in the
    ''' event of an error. Relevant only when the return
    ''' value is false.</param>
    ''' <returns>Returns true on success.</returns>
    Public MustOverride Function GetText(ByVal StartRow As Integer, ByVal StartColumn As Integer, ByVal Length As Integer, ByRef Value As String, ByRef sErr As String) As Boolean

    ''' <summary>
    ''' Sets the text in the specified cells.
    ''' </summary>
    ''' <param name="StartRow">The start row of the first cell, indexed
    ''' from 1.</param>
    ''' <param name="StartColumn">The start column of the first cell, indexed
    ''' from 1.</param>
    ''' <param name="Value">The text to set.</param>
    ''' <param name="sErr">Carries back an error message in the
    ''' event of an error. Relevant only when the return
    ''' value is false.</param>
    ''' <returns>Returns true on success.</returns>
    Public MustOverride Function SetText(ByVal StartRow As Integer, ByVal StartColumn As Integer, ByVal Value As String, ByRef sErr As String) As Boolean

    ''' <summary>
    ''' This method Is obsolete, use SendControlKeys instead. Provided for
    ''' backwards compatability this method sends a keystroke to the application.
    ''' </summary>
    ''' <param name="Key">A string representation of a keycode.
    ''' Clients should be aware of type of the target mainframe
    ''' and format the keycode accordingly. We abstracted the concept of a
    ''' keystroke and clients are able to send non-specific keycodes relative
    ''' to the implementation which is what SendControlKeys is for.</param>
    ''' <param name="sErr">Carries back an error message in the
    ''' event of an error. Relevant only when the return
    ''' value is false.</param>
    ''' <returns>Returns true on success.</returns>
    <Obsolete>
    Public MustOverride Function SendKeystroke(ByVal Key As String, ByRef sErr As String) As Boolean

    ''' <summary>
    ''' Sends a control keys to the application.
    ''' </summary>
    ''' <param name="keys">A string representation of the keycode sequences</param>
    ''' <param name="sErr">Carries back an error message in the event of an
    ''' error. Relevant only when the return value is false.</param>
    ''' <returns>Returns true on success.</returns>
    Public MustOverride Function SendControlKeys(keys As String, ByRef sErr As String) As Boolean

    ''' <summary>
    ''' Gets the window title of the terminal emulator owning, the
    ''' presentation space.
    ''' </summary>
    ''' <param name="Value">Carries back the title of the window.</param>
    ''' <param name="sErr">Carries back a message in the event of
    ''' an error (check return value of function for error status).</param>
    ''' <returns>Returns true on success. When false, check the sErr parameter
    ''' for an error message.</returns>
    Public MustOverride Function GetWindowTitle(ByRef Value As String, ByRef sErr As String) As Boolean

    ''' <summary>
    ''' Sets the window title of the terminal emulator owning, the
    ''' presentation space.
    ''' </summary>
    ''' <param name="Value">The new title for the window.</param>
    ''' <param name="sErr">Carries back a message in the event of
    ''' an error (check return value of function for error status).</param>
    ''' <returns>Returns true on success. When false, check the sErr parameter
    ''' for an error message.</returns>
    Public MustOverride Function SetWindowTitle(ByVal Value As String, ByRef sErr As String) As Boolean

    ''' <summary>
    ''' Gets the position of the cursor within the presentation space.
    ''' </summary>
    ''' <param name="col">Carries back the value of the column at which
    ''' the cursor is located. Indexed from 1; max value is the number of columns.</param>
    ''' <param name="row">Carries back the value of the row at which
    ''' the cursor is located. Indexed from 1; max value is the number of rows.</param>
    ''' <param name="sErr">Carries back a message in the event of an error.</param>
    ''' <returns>Returns true on succes. When false, see the sErr parameter.</returns>
    Public MustOverride Function GetCursorPosition(ByRef row As Integer, ByRef col As Integer, ByRef sErr As String) As Boolean

    ''' <summary>
    ''' Sets the position of the cursor in the presentation space.
    ''' </summary>
    ''' <param name="col">The x coordinate of the new cursor position, indexed from 1.
    ''' Maximum allowable value is the number of columns.</param>
    ''' <param name="row">The y coordinate of the new cursor position, indexed from 1.
    ''' Maximum allowable value is the number of rows.</param>
    ''' <param name="sErr">Carries back a message in the event of an error.</param>
    ''' <returns>Returns true on succes. When false, see the sErr parameter.</returns>
    Public MustOverride Function SetCursorPosition(ByVal row As Integer, ByVal col As Integer, ByRef sErr As String) As Boolean

    ''' <summary>
    ''' Runs an emulator macro synchronously, returning once the macro has completed.
    ''' </summary>
    ''' <param name="MacroName">The name of the macro to be run. Full file paths
    ''' are not supported.</param>
    ''' <param name="sErr">Carries back a message in the event of an error.</param>
    ''' <returns>Returns true on success, false otherwise. When false, return to the
    ''' sErr parameter.</returns>
    Public MustOverride Function RunEmulatorMacro(ByVal MacroName As String, ByRef sErr As String) As Boolean

    ''' <summary>
    ''' Gets the size of the session in terms of rows and columns.
    ''' </summary>
    ''' <param name="numRows">Carries back the number of rows.</param>
    ''' <param name="numColumns">Carries back the number of columns</param>
    ''' <param name="sErr">On failure, contains an error message.</param>
    ''' <returns>Returns True on success.</returns>
    Public MustOverride Function GetSessionSize(ByRef numRows As Integer, ByRef numColumns As Integer, ByRef sErr As String) As Boolean

    ''' <summary>
    ''' Determines whether or not the object is connected to a session.
    ''' </summary>
    ''' <returns>True if connected, False otherwise.</returns>
    Public MustOverride Function IsConnected() As Boolean


    ''' <summary>
    ''' Types of area selection available in terminal types.
    ''' </summary>
    Public Enum SelectionType
        ''' <summary>
        ''' Continuous selection equivalent to
        ''' a start point and a length.
        ''' </summary>
        Continuous
        ''' <summary>
        ''' A block selection of rectangular form
        ''' from its start point to its end point.
        ''' </summary>
        Block
    End Enum

    ''' <summary>
    ''' Selects the specified area of the presentation space.
    ''' </summary>
    ''' <param name="StartRow">The row at which the selection begins, indexed from 1.</param>
    ''' <param name="EndRow">The row at which the selection ends, indexed from 1.</param>
    ''' <param name="StartColumn">The column at which the selection begins, indexed
    ''' from 1.</param> 
    ''' <param name="EndColumn">The column at which the selection ends, indexed
    ''' from 1.</param>
    ''' <param name="sErr">Carries an error message in the event of an error.</param>
    ''' <param name="Type">The type of selection.</param>
    ''' <returns>Returns True on success.</returns>
    ''' <remarks>Relevant only for terminals which can select areas of the presentation
    ''' space. A terminal which can not do this will simply return False each time.
    ''' </remarks>
    Public MustOverride Function SelectArea(ByVal StartRow As Integer, ByVal StartColumn As Integer, ByVal EndRow As Integer, ByVal EndColumn As Integer, ByVal Type As SelectionType, ByRef sErr As String) As Boolean

    ''' <summary>
    ''' The time for which to sleep between polls of the terminal API whilst waiting for
    ''' a change (e.g. wait for idle host, or wait for string).
    ''' </summary>
    Public MustOverride Property SleepTime() As Integer


    ''' <summary>
    ''' The timeout value (in milliseconds) for wait requests
    ''' </summary>
    Public MustOverride Property WaitTimeout() As Integer

    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
    End Sub
End Class
