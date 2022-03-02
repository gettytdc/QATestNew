Imports System.Globalization
Imports BluePrism.BPCoreLib

Namespace Operations
    ''' <inheritDoc/>
    Friend Class MouseOperationsProvider : Implements IMouseOperationsProvider

        ''' <inheritDoc/>
        Public Sub DragFrom(x As Integer, y As Integer) Implements IMouseOperationsProvider.DragFrom
            SetCursorPos(x, y)
            mouse_event(MOUSEEVENTF.LEFTDOWN, 0, 0, 0, 0)
            mouse_event(MOUSEEVENTF.MOVE, 15, 15, 0, 0) 'The extra nudge is to convince applications that we really are dragging
        End Sub

        ''' <inheritDoc/>
        Public Sub DropAt(x As Integer, y As Integer) Implements IMouseOperationsProvider.DropAt
            SetCursorPos(x, y)
            mouse_event(MOUSEEVENTF.LEFTUP, 0, 0, 0, 0)
        End Sub

        ''' <inheritDoc/>
        Public Sub ClickAt(
                x As Integer,
                y As Integer,
                doubleClick As Boolean,
                button As MouseButton) _
             Implements IMouseOperationsProvider.ClickAt

            SetCursorPos(x, y)
            SingleClick(button)
            If doubleClick
                SingleClick(button)
            End If
        End Sub

        ''' <inheritDoc/>
        Public Function ParseMouseButton(ByVal buttonString As String) _
                As MouseButton _
                Implements IMouseOperationsProvider.ParseMouseButton

            Dim result = MouseButton.Left

            If buttonString IsNot Nothing 
                Select Case buttonString.ToLower(CultureInfo.InvariantCulture)
                    Case "right"
                        result = MouseButton.Right
                    Case "left"
                        result = MouseButton.Left
                    Case Else
                        Throw New InvalidOperationException(
                            String.Format(My.Resources.FailedToInterpretValue0AsAValidButton, buttonString))
                End Select
            End If

            Return result
        End Function


        Private Sub SingleClick(ByVal mouseButton As MouseButton)
            Dim cbuttons As Integer
            Dim dwExtraInfo As Integer
            Dim mevent As Integer

            Select Case mouseButton
                Case MouseButton.Left
                    mevent = MOUSEEVENTF.LEFTDOWN Or MOUSEEVENTF.LEFTUP
                Case MouseButton.Right
                    mevent = MOUSEEVENTF.RIGHTDOWN Or MOUSEEVENTF.RIGHTUP
                Case MouseButton.Middle
                    mevent = MOUSEEVENTF.MIDDLEDOWN Or MOUSEEVENTF.MIDDLEUP
                Case MouseButton.LeftDown
                    mevent = MOUSEEVENTF.LEFTDOWN
                Case MouseButton.LeftUp
                    mevent = MOUSEEVENTF.LEFTUP
            End Select
            mouse_event(mevent, 0&, 0&, cbuttons, dwExtraInfo)

        End Sub
    End Class
End Namespace