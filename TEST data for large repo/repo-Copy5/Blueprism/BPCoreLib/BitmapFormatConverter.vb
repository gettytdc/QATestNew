Imports System.Drawing
Imports System.Drawing.Imaging

''' <summary>
''' Helper format conversion functions for Bitmaps
''' </summary>
''' <remarks></remarks>
Public Class BitmapFormatConverter
    ''' <summary>
    ''' Normalises the given bitmap to 24 bits (Format24bppRgb PixelFormat), ensuring 
    ''' that the alpha channel is ignored.
    ''' </summary>
    ''' <param name="bmp">The bitmap to normalise. If a new bitmap
    ''' had to be created for the format, the old one will have been disposed of.
    ''' </param>
    ''' <returns>
    ''' A new Bitmap converted <see cref="Drawing.Imaging.PixelFormat.Format24bppRgb"/>
    ''' pixel format or the existing Bitmap if it was already of the required format
    ''' </returns>
    Public Shared Function NormaliseBitmap(bmp As Bitmap) As Bitmap
        Return NormaliseBitmap(bmp, True)
    End Function

    ''' <summary>
    ''' Normalises the given bitmap to 24 bits (Format24bppRgb PixelFormat), ensuring 
    ''' that the alpha channel is ignored.
    ''' </summary>
    ''' <param name="bmp">The bitmap to normalise. On return, this bitmap will have
    ''' a pixel format of <see cref="Drawing.Imaging.PixelFormat.Format24bppRgb"/>.
    ''' </param>
    ''' <param name="disposeOfObsoleteBitmap">True to dispose of the obsolete bitmap
    ''' if it has to be replaced; False to leave that to the caller</param>
    ''' <returns>
    ''' A new Bitmap converted <see cref="Drawing.Imaging.PixelFormat.Format24bppRgb"/>
    ''' pixel format or the existing Bitmap if it was already of the required format
    ''' </returns>
    Public Shared Function NormaliseBitmap(bmp As Bitmap, _
        disposeOfObsoleteBitmap As Boolean) As Bitmap
        ' Convert into 24bpp if it's not already there.
        If bmp.PixelFormat <> PixelFormat.Format24bppRgb Then
            ' Roundabout way of cloning because a simple Clone(size,pixelformat)
            ' would sometimes throw out of memory errors - the generic GDI+ error
            ' indicating "something is wrong".
            Dim clone As _
                    New Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb)
            Using g As Graphics = Graphics.FromImage(clone)
                g.DrawImage(bmp, New Rectangle(Point.Empty, clone.Size))
            End Using
            If disposeOfObsoleteBitmap Then bmp.Dispose()
            Return clone
        Else
            Return bmp
        End If
    End Function
End Class