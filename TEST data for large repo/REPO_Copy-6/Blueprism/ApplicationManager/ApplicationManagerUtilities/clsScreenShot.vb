Imports System.Drawing
Imports System.Drawing.Imaging
Imports BluePrism.BPCoreLib

''' <summary>
''' A class to capture images from the screen.
''' </summary>
''' <remarks></remarks>
Public Class clsScreenShot

    Private Const RDW_UPDATENOW As Integer = &H100
    Private Const RDW_INVALIDATE As Integer = &H1
    Private Const RDW_ALLCHILDREN As Integer = &H80
    Private Const RDW_ERASE As Integer = &H4

#Region "GetScreenShot"

    ''' <summary>
    ''' Gets an image of the desktop
    ''' </summary>
    ''' <returns>An image of the desktop</returns>
    ''' <remarks></remarks>
    Public Function GetScreenShot() As Image
        Return GetScreenShot(modWin32.GetDesktopWindow())
    End Function


    Public Function GetScreenShot(ByVal handle As IntPtr) As Image
        Return GetScreenShot(handle, Size.Empty)
    End Function

    ''' <summary>
    ''' Gets an image of a window
    ''' </summary>
    ''' <param name="handle">The handle</param>
    ''' <returns>An image of a window</returns>
    ''' <remarks></remarks>
    Public Function GetScreenShot(ByVal handle As IntPtr, ByVal Offset As Size) As Image


        ' get te hDC of the target window
        Dim hdcSrc As IntPtr = modWin32.GetWindowDC(handle)
        ' get the size
        Dim strWindow As New RECT
        modWin32.GetWindowRect(handle, strWindow)
        Dim iWidth As Integer = strWindow.right - strWindow.left
        Dim iHeight As Integer = strWindow.bottom - strWindow.top

        modWin32.RedrawWindow(handle, strWindow, IntPtr.Zero, RDW_UPDATENOW)

        ' create a device context we can copy to
        Dim hdcDest As IntPtr = clsGDI32.CreateCompatibleDC(hdcSrc)
        ' create a bitmap we can copy it to,
        ' using GetDeviceCaps to get the iWidth/iHeight
        Dim hBitmap As IntPtr = clsGDI32.CreateCompatibleBitmap(hdcSrc, iWidth, iHeight)
        ' select the bitmap object
        Dim hOld As IntPtr = clsGDI32.SelectObject(hdcDest, hBitmap)
        ' bitblt over
        clsGDI32.BitBlt(hdcDest, -Offset.Width, -Offset.Height, iWidth, iHeight, hdcSrc, 0, 0, clsGDI32.SRCCOPY)
        ' restore selection
        clsGDI32.SelectObject(hdcDest, hOld)
        ' clean up 
        clsGDI32.DeleteDC(hdcDest)
        modWin32.ReleaseDC(handle, hdcSrc)

        ' get a .NET image object for it
        Dim oImage As Image = Image.FromHbitmap(hBitmap)
        ' free up the Bitmap object
        clsGDI32.DeleteObject(hBitmap)

        Return oImage

    End Function

#End Region

#Region "WriteScreenShot"

    ''' <summary>
    ''' Saves the image of a window as a file
    ''' </summary>
    ''' <param name="handle">The handle</param>
    ''' <param name="filename">The file</param>
    ''' <param name="format">The image format</param>
    ''' <remarks></remarks>
    Public Sub WriteScreenShot(ByVal handle As IntPtr, ByVal filename As String, ByVal format As ImageFormat)
        Dim oImage As Image = GetScreenShot(handle)
        oImage.Save(filename, format)
    End Sub

    ''' <summary>
    ''' Saves the image of the desktop as a file
    ''' </summary>
    ''' <param name="filename">The file</param>
    ''' <param name="format">The image format</param>
    ''' <remarks></remarks>
    Public Sub WriteScreenShot(ByVal filename As String, ByVal format As ImageFormat)
        Dim oImage As Image = GetScreenShot()
        oImage.Save(filename, format)
    End Sub

#End Region

End Class
