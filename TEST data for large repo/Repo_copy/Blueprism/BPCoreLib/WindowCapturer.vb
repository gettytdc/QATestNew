Imports System.Drawing

''' <summary>
''' Captures images from Window on screen
''' </summary>
''' <remarks></remarks>
Public Class WindowCapturer
    ''' <summary>
    ''' Capture an area of a window from the screen as a bitmap
    ''' </summary>
    ''' <param name="hWnd">The handle of the target window</param>
    ''' <param name="r">The rectangle, relative to the window, defining the region to
    ''' be captured - if the rectangle is <see cref="Drawing.Rectangle.IsEmpty">empty</see>,
    ''' the entire window is captured.</param>
    ''' <remarks>The bitmap that this returns should be disposed of once it is
    ''' finished with</remarks>
    Public Shared Function CaptureBitmap(ByVal hWnd As IntPtr, ByVal r As Rectangle) As Bitmap

        Dim rect As RECT
        GetWindowRect(hWnd, rect)

        Dim bounds As Rectangle
        If r.IsEmpty Then
            bounds = rect
        Else
            Dim location As Point = rect.Location
            location.Offset(r.Left, r.Top)
            bounds = New Rectangle(location, r.Size)
        End If

        Dim bitmap As New Bitmap(bounds.Width, bounds.Height)
        Using g As Graphics = Graphics.FromImage(bitmap)
            g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size)
        End Using

        Return bitmap

    End Function
End Class