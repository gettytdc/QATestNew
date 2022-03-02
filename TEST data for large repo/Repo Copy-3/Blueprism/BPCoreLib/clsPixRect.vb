
Imports System.Drawing.Imaging
Imports System.Drawing
Imports System.Text.RegularExpressions
Imports System.IO

Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Server.Domain.Models

''' Project  : BPCoreLib
''' Class    : clsPixRect
''' 
''' <summary>
''' Represents a rectangular array of pixels, each with an 8 bit R,G,B colour. Similar
''' to a Bitmap but optimised for the access requirements of automation
''' such as fast scanning and searching.
''' 
''' PixRects have a standardised textual representation, as in  the following example:
'''
'''   10,10,137224,Base64Data
'''
''' The first two items are the width and height, followed by the integer value of
''' the pixel format used in the data. This is always going to be 137224 at the time
''' of writing (PixelFormat.Format24bppRgb == 137224). Finally, the base 64 data is a
''' PNG encoded image, with non-deterministic metadata stripped out. This
''' representation can be obtained from an existing instance via the ToString()
''' method, and a constructor is available to create a new instance from this
''' representation.
'''
''' Note that the old format of:
'''
'''   [width],[height],[data]
'''
''' is also supported and will be normalised when the pixrect is loaded. The string
''' can be converted into normal form using the
''' <see cref="clsPixRect.NormaliseString"/> method
''' </summary>
Public Class clsPixRect


    ''' <summary>
    ''' Regex detailing the normalised text form of a pixrect object.
    ''' </summary>
    Private Shared ReadOnly mNormalisedRegex As _
     New Regex("^(\d+),(\d+),(\d+),(.*)$", RegexOptions.None, DefaultRegexTimeout)

    ''' <summary>
    ''' Regex detailing the legacy text form of a pixrect object
    ''' </summary>
    Private Shared ReadOnly mLegacyRegex As _
     New Regex("^(\d+),(\d+),()(.*)$", RegexOptions.None, DefaultRegexTimeout)

    Private mPixels() As Integer

    ''' <summary>
    ''' The width, in pixels.
    ''' </summary>
    Public ReadOnly Property Width() As Integer
        Get
            Return mWidth
        End Get
    End Property
    Private mWidth As Integer

    ''' <summary>
    ''' The height, in pixels.
    ''' </summary>
    Public ReadOnly Property Height() As Integer
        Get
            Return mHeight
        End Get
    End Property
    Private mHeight As Integer

    ''' <summary>
    ''' Create a new PixRect with the given width and height. All pixels are initially
    ''' black.
    ''' </summary>
    ''' <param name="iWidth">The width</param>
    ''' <param name="iHeight">The height</param>
    Public Sub New(ByVal iWidth As Integer, ByVal iHeight As Integer)
        mWidth = iWidth
        mHeight = iHeight
        ReDim mPixels(iWidth * iHeight - 1)
    End Sub

    ''' <summary>
    ''' Create a new PixRect using the given Bitmap as the source.
    ''' </summary>
    ''' <param name="b">The Bitmap to use as the source.</param>
    Public Sub New(ByVal b As Bitmap)
        CopyBitmap(b)
    End Sub

    ''' <summary>
    ''' Create a new instance from the given textual representation. Throws an exception
    ''' if the representation is invalid.
    ''' </summary>
    ''' <param name="s">The textual representation - see the main class documentation.
    ''' </param>
    Public Sub New(ByVal s As String)
        Using bmp As Bitmap = ParseBitmap(s)
            If bmp Is Nothing Then _
             Throw New InvalidFormatException(My.Resources.clsPixRect_InvalidPixRectFormat)
            CopyBitmap(bmp)
        End Using
    End Sub

    ''' <summary>
    ''' Create a new PixRect using the given array of pixel data.
    ''' </summary>
    ''' <param name="pixels">The pixel data - this is not cloned, so becomes owned
    ''' by the new PixRect.</param>
    ''' <param name="width">The width of the pixel data.</param>
    ''' <param name="height">The height of the pixel data.</param>
    Private Sub New(ByVal pixels() As Integer, ByVal width As Integer, ByVal height As Integer)
        If pixels.Length <> width * height Then Throw New InvalidFormatException(
         My.Resources.clsPixRect_PixelsArrayMustHaveLengthEqualToTheSpecifiedWidthMultipliedByTheSpecifiedHeight)

        mWidth = width
        mHeight = height
        mPixels = pixels
    End Sub


    ''' <summary>
    ''' Copy the given Bitmap into this PixRect instance.
    ''' </summary>
    ''' <param name="b">The Bitmap to copy</param>
    Private Sub CopyBitmap(ByVal b As Bitmap)
        mWidth = b.Width
        mHeight = b.Height
        ReDim mPixels(mWidth * mHeight - 1)
        Dim iPos As Integer = 0
        For y As Integer = 0 To mHeight - 1
            For x As Integer = 0 To mWidth - 1
                mPixels(iPos) = b.GetPixel(x, y).ToArgb() And &HFFFFFF
                iPos += 1
            Next
        Next
    End Sub

    ''' <summary>
    ''' Creates a new Bitmap based on the underlying PixRect.
    ''' </summary>
    ''' <returns>A new Bitmap. The caller is responsible for the
    ''' disposal of this Bitmap.</returns>
    Public Function ToBitmap() As Bitmap
        Dim b As New Bitmap(mWidth, mHeight, PixelFormat.Format24bppRgb)
        Dim iPos As Integer = 0
        For y As Integer = 0 To mHeight - 1
            For x As Integer = 0 To mWidth - 1
                b.SetPixel(x, y, Color.FromArgb(mPixels(iPos) Or &HFF000000))
                iPos += 1
            Next
        Next
        Return b
    End Function

    ''' <summary>
    ''' Convert the underlying PixRect to a textual representation.
    ''' </summary>
    ''' <returns>The textual representation - see the main class documentation.
    ''' </returns>
    Public Overrides Function ToString() As String
        Using b As Bitmap = ToBitmap()
            Return BitmapToString(b)
        End Using
    End Function

    ''' <summary>
    ''' Gets the integer value of the pixel at the given point.
    ''' </summary>
    ''' <param name="x">The x co-ordinate of the required pixel</param>
    ''' <param name="y">The y co-ordinate of the required pixel</param>
    Private Function GetPixel(ByVal x As Integer, ByVal y As Integer) As Integer
        Return mPixels(y * mWidth + x)
    End Function

    ''' <summary>
    ''' Checks if the given subimage is within this image at the specified offset.
    ''' </summary>
    ''' <param name="subimage">The subimage to test for within this image</param>
    ''' <param name="offset">The offset from the top left of this image to check for
    ''' the given subimage.</param>
    ''' <returns>True if the given subimage existed at the specified offset within
    ''' this image; False otherwise.</returns>
    Private Function IsSubImage(
     ByVal subimage As clsPixRect, ByVal offset As Point) As Boolean
        For x As Integer = 0 To subimage.mWidth - 1
            For y As Integer = 0 To subimage.mHeight - 1
                If GetPixel(offset.X + x, offset.Y + y) <> subimage.GetPixel(x, y) Then _
                 Return False
            Next
        Next
        ' All pixels matched - super. That's a bingo.
        Return True
    End Function

    ''' <summary>
    ''' Checks if this pix rect contains the given sub-image.
    ''' </summary>
    ''' <param name="subimage">The image to search this image for</param>
    ''' <returns>True if this image contains the given subimage within it</returns>
    Public Function Contains(ByVal subimage As clsPixRect) As Boolean
        Return Contains(subimage, Nothing)
    End Function

    ''' <summary>
    ''' Checks if this pix rect contains the given sub-image.
    ''' </summary>
    ''' <param name="subimage">The image to search this image for</param>
    ''' <param name="locn">The location at which the subimage was found, or
    ''' <see cref="Point.Empty"/> if the subimage was not found.</param>
    ''' <returns>True if this image contains the given subimage within it, False if
    ''' the subimage was not found.</returns>
    Public Function Contains(ByVal subimage As clsPixRect, ByRef locn As Point) As Boolean

        For i As Integer = 0 To mWidth - subimage.mWidth
            For j As Integer = 0 To mHeight - subimage.mHeight
                Dim pt As New Point(i, j)
                If IsSubImage(subimage, pt) Then locn = pt : Return True
            Next
        Next
        locn = Point.Empty
        Return False
    End Function


    ''' <summary>
    ''' Enumeration of the PNG Chunks supported by the StripChunk method.
    ''' </summary>
    Public Enum PngChunk

        ''' <summary>
        ''' No chunk - specifying this in a StripChunks() call is effectively a no-op
        ''' </summary>
        None = 0

        ''' <summary>
        '''  Image Gamma information
        ''' </summary>
        gAMA

        ''' <summary>
        ''' Image last-modification time
        ''' </summary>
        tIME

        ''' <summary>
        ''' Physical pixel dimensions
        ''' </summary>
        pHYs

        ''' <summary>
        '''  Standard RGB color space
        ''' </summary>
        sRGB

        ''' <summary>
        ''' Image histogram
        ''' </summary>
        hIST

        ''' <summary>
        ''' Additional text information
        ''' </summary>
        tEXt

        ''' <summary>
        ''' International textual data
        ''' </summary>
        iTXt

        ''' <summary>
        ''' Compressed textual data
        ''' </summary>
        zTXt


    End Enum

    ''' <summary>
    ''' Saves the given input image into a PNG-encoded byte array, stripping the
    ''' specified chunks from the result before doing so.
    ''' encoded byte array, less the specified chunks
    ''' </summary>
    ''' <param name="input">The input bitmap from which the specified chunks
    ''' should be stripped.</param>
    ''' <param name="chunks">The chunks to strip from the image</param>
    ''' <returns>The PNG-encoded image, without the specified PNG chunks, written
    ''' into a byte array</returns>
    Public Shared Function StripPngChunks(
     ByVal input As Bitmap, ByVal ParamArray chunks As PngChunk()) As Byte()
        Dim data As Byte()
        Using initMs As New MemoryStream()
            input.Save(initMs, ImageFormat.Png)
            data = initMs.ToArray()
        End Using
        If chunks Is Nothing OrElse chunks.Length = 0 Then Return data

        ' Create a set containing the names of all of the chunks we want to strip
        Dim strips As New clsSet(Of String)
        For Each ch As PngChunk In chunks
            ' Ignore empty chunks
            If ch <> PngChunk.None Then strips.Add(ch.ToString())
        Next
        If strips.Count = 0 Then Return data

        ' PNG Format:
        ' Header (8 bytes)
        ' Chunk * {
        '   datalength: 4 bytes;
        '   type/name: 4 bytes;
        '   data: [datalength] bytes;
        '   crc: 4 bytes
        ' }
        ' See http://www.libpng.org/pub/png/spec/1.2/PNG-Contents.html for full spec

        Using ms As New MemoryStream()
            ' Write the header
            ms.Write(data, 0, 8)

            Dim offset As Integer = 8

            ' Go through each chunk one at a time. Check the name against those
            ' that we want to strip. If it's there, skip the entire chunk, otherwise
            ' write it and move onto the next chunk
            ' "offset" always points to the beginning of a chunk and is moved on in
            ' chunk sizes (ie. 12 bytes + datalength) as it progresses
            While offset <= data.Length - 12

                ' First chunk: length
                Dim lenSection(3) As Byte
                lenSection(0) = data(offset)
                lenSection(1) = data(offset + 1)
                lenSection(2) = data(offset + 2)
                lenSection(3) = data(offset + 3)
                If BitConverter.IsLittleEndian Then Array.Reverse(lenSection)

                Dim len As Integer = BitConverter.ToInt32(lenSection, 0)

                ' Second chunk: name
                Dim name As String =
                 Encoding.ASCII.GetString(data, offset + 4, 4)

                ' Write the data if we're not stripping it out
                If Not strips.Contains(name) Then ms.Write(data, offset, 12 + len)

                ' ..and move onto the next chunk
                offset += 12 + len

            End While

            Return ms.ToArray()
        End Using
    End Function

    ''' <summary>
    ''' Convert a bitmap directly to the string representation 
    ''' and avoid the unneccecery convertion to and from an array
    ''' saving time and memory.
    ''' </summary>
    ''' <param name="b">The bitmap to convert</param>
    ''' <returns>a string representation (compatible with one produced by ToString
    ''' above)</returns>
    Public Shared Function BitmapToString(ByVal b As Bitmap) As String
        If b Is Nothing Then Return ""

        ' Save the original bitmap so we can detect if it has been normalized (ie.
        ' that we've created a local Bitmap object that we need to dispose of)
        Dim originalBmp As Bitmap = b

        ' Normalise to 24bpp - don't dispose of the old bitmap if it had to be
        ' replaced - it's not ours to dispose
        b = BitmapFormatConverter.NormaliseBitmap(b, False)

        Try
            ' Strip ancillary data (time information, physical resolution, metadata and
            ' other data specific to display/rendering) from the generated image. The 
            ' ancillary data included by GDI will vary according to environment resulting 
            ' in different values in the serialized data for 2 images with identical pixel 
            ' data. Note that this the data we remove is restricted to purely informational
            ' data chunks and those that have caused problems in production / testing (see bug 7133 
            ' (Bugzilla) and bg-844 (Yodiz)). A more thorough analysis of whether we can remove _all_
            ' ancillary data chunks might be of use in the future.
            Dim arr() As Byte = StripPngChunks(b, PngChunk.pHYs, PngChunk.tIME, PngChunk.gAMA,
                PngChunk.sRGB, PngChunk.hIST, PngChunk.tEXt, PngChunk.iTXt, PngChunk.zTXt)
            Return String.Format("{0},{1},{2},{3}", b.Width, b.Height,
             CInt(PixelFormat.Format24bppRgb), Convert.ToBase64String(arr))

        Finally
            ' If the bitmap has been normalised, dispose of the new one which was
            ' created by us; if it has not, we leave it to the caller to dispose of
            If b IsNot originalBmp Then b.Dispose()

        End Try

    End Function

    ''' <summary>
    ''' Get a bitmask map of the pixrect, where each bit is True if the original
    ''' pixel matched the specified colour, or False otherwise.
    ''' </summary>
    ''' <param name="iMatchColour">The colour to match against.</param>
    ''' <returns>A two-dimensional Boolean array, matching the width and height
    ''' of the pixrect.</returns>
    Public Function GetBitmaskMap(ByVal iMatchColour As Integer) As Boolean(,)
        'Create an array of Booleans where True represents a
        'pixel that is part of the text and False any other pixel.
        Dim x As Integer, y As Integer
        Dim bSourceData(mWidth - 1, mHeight - 1) As Boolean
        For y = 0 To mHeight - 1
            For x = 0 To mWidth - 1
                bSourceData(x, y) = (mPixels(y * mWidth + x) = iMatchColour)
            Next
        Next
        Return bSourceData
    End Function

    ''' <summary>
    ''' Get a bitmask map of the pixrect, where each bit is True if the original
    ''' pixel fails the specified background colour, or False otherwise.
    ''' </summary>
    ''' <param name="iBackgroundColour">The colour to match against.</param>
    ''' <returns>A two-dimensional Boolean array, matching the width and height
    ''' of the pixrect.</returns>
    Public Function GetBitmaskMapByBackground(ByVal iBackgroundColour As Integer) As Boolean(,)
        'Create an array of Booleans where True represents a
        'pixel that is part of the text and False any other pixel.
        'Anything not matching the background colour is text.
        Dim x As Integer, y As Integer
        Dim bSourceData(mWidth - 1, mHeight - 1) As Boolean
        For y = 0 To mHeight - 1
            For x = 0 To mWidth - 1
                bSourceData(x, y) = (mPixels(y * mWidth + x) <> iBackgroundColour)
            Next
        Next
        Return bSourceData
    End Function

    ''' <summary>
    ''' Creates an instance of a black-and-white clsPixRect
    ''' representing the supplied BitMask.
    ''' </summary>
    ''' <param name="BitMask">The bitmask from which to create a clsPixRect</param>
    ''' <returns>Returns a new clsPixRect representing the supplied bitmask.</returns>
    Public Shared Function FromBitMask(ByVal BitMask As Boolean(,)) As clsPixRect
        Dim Width As Integer = BitMask.GetLength(0)
        Dim Height As Integer = BitMask.GetLength(1)

        Dim retval As New clsPixRect(Width, Height)
        ReDim retval.mPixels(Width * Height - 1)
        For x As Integer = 0 To Width - 1
            For y As Integer = 0 To Height - 1
                retval.mPixels(x + y * Width) = CInt(IIf(BitMask(x, y), 0, &HFFFFFF))
            Next
        Next

        Return retval
    End Function

    ''' <summary>
    ''' Validate a string representation of a PixRect.
    ''' </summary>
    ''' <returns>True if the string representation is valid.</returns>
    Public Shared Function ValidateText(ByVal str As String) As Boolean
        If str Is Nothing Then Return False
        Return (mNormalisedRegex.IsMatch(str) OrElse mLegacyRegex.IsMatch(str))
    End Function

    ''' <summary>
    ''' Extracts the top level data from the given pic string.
    ''' </summary>
    ''' <param name="picStr">The string from which to draw the data</param>
    ''' <param name="sz">The size as indicated by the values in the string</param>
    ''' <param name="base64Data">The base64-encoded data within the string</param>
    ''' <exception cref="InvalidFormatException">If the string was not in the
    ''' required format.</exception>
    Private Shared Sub ExtractData(
     ByVal picStr As String, ByRef sz As Size, ByRef base64Data As String)

        ' Try normalised first, then legacy
        Dim m As Match = mNormalisedRegex.Match(picStr)
        If Not m.Success Then m = mLegacyRegex.Match(picStr)
        If Not m.Success Then _
         Throw New InvalidFormatException(My.Resources.clsPixRect_InvalidPixRectFormat)

        sz = New Size(CInt(m.Groups(1).Value), CInt(m.Groups(2).Value))
        ' Groups(3) is the PixelFormat - currently always 24bpp so we can ignore it
        base64Data = m.Groups(4).Value

    End Sub


    ''' <summary>
    ''' Retrieve the dimensions from a textual represtation of a PixRect.
    ''' </summary>
    ''' <param name="s">A PixRect represented in textual form</param>
    ''' <returns>The size of the bitmap as indicated by the given string, or
    ''' <see cref="Size.Empty"/> if the string was not in a valid format.</returns>
    Public Shared Function ParseDimensions(ByVal s As String) As Size
        Try
            Dim sz As Size
            ExtractData(s, sz, Nothing)
            Return sz
        Catch
            Return Size.Empty
        End Try
    End Function

    ''' <summary>
    ''' Checks if the given pixrect string represents a normalised pixrect or not
    ''' </summary>
    ''' <param name="str">The string to test</param>
    ''' <returns>True if the given string is recognisable as a normalised pixrect;
    ''' False otherwise</returns>
    Public Shared Function IsNormalised(ByVal str As String) As Boolean
        Return mNormalisedRegex.IsMatch(str)
    End Function

    ''' <summary>
    ''' Normalises the given pixrect string such that it indicates that it has been
    ''' normalised - ie. it is set to 24bpp and has had nondeterministic PNG chunks
    ''' stripped out.
    ''' </summary>
    ''' <param name="str">The string to normalise</param>
    ''' <returns>A string representing a normalised pixrect.</returns>
    Public Shared Function NormaliseString(ByVal str As String) As String
        ' Try normalised first, then legacy
        Dim m As Match = mNormalisedRegex.Match(str)
        If m.Success Then Return str

        ' Not a success... try legacy
        m = mLegacyRegex.Match(str)
        If Not m.Success Then _
         Throw New InvalidFormatException(My.Resources.clsPixRect_InvalidPixRectFormat)

        ' We have a valid legacy pixrect, so normalise it.
        Dim pic As New clsPixRect(str)
        Return pic.ToString()

    End Function

    ''' <summary>
    ''' Tests whether the two strings represent the same image after image
    ''' normalisation
    ''' </summary>
    ''' <param name="picStr1">The first pic string to test</param>
    ''' <param name="picStr2">The second pic string to test</param>
    ''' <returns>True if the two strings represent the same string after
    ''' normalisation</returns>
    Public Shared Function Matches(
     ByVal picStr1 As String, ByVal picStr2 As String) As Boolean
        ' Null or empty only matches null or empty
        If picStr1 = "" Then Return (picStr2 = "")
        ' Normalise the strings before testing
        picStr1 = NormaliseString(picStr1)
        picStr2 = NormaliseString(picStr2)
        ' Then it's a simple equality test
        Return (picStr1 = picStr2)
    End Function

    ''' <summary>
    ''' Parse a textual representation of a PixRect directly into a Bitmap without
    ''' actually creating a PixRect. This is quicker, when that is the only desired
    ''' end result.
    ''' </summary>
    ''' <param name="picStr">A PixRect represented in textual form</param>
    ''' <returns>A new Bitmap. The caller is responsible for the disposal of this
    ''' Bitmap. Returns Nothing if the input data was invalid.</returns>
    ''' <exception cref="InvalidFormatException">If the string was not in the
    ''' required format or if the size reported in the string differed from that
    ''' found in the image data.</exception>
    Public Shared Function ParseBitmap(ByVal picStr As String) As Bitmap

        Dim bmp As Bitmap = Nothing
        Try

            Dim sz As Size
            Dim encodedData As String = Nothing
            ExtractData(picStr, sz, encodedData)

            Using s As New MemoryStream(Convert.FromBase64String(encodedData))
                bmp = DirectCast(Image.FromStream(s), Bitmap)
            End Using

            bmp = BitmapFormatConverter.NormaliseBitmap(bmp)

            If bmp.Size <> sz Then Throw New InvalidFormatException(
             My.Resources.clsPixRect_InvalidDimensionsReportedSize0ActualSize1,
             sz, bmp.Size)

            Return bmp

        Catch
            If bmp IsNot Nothing Then bmp.Dispose()
            Return Nothing

        End Try

    End Function

#Region " Capture Methods "

    ''' <summary>
    ''' Captures an entire window from the screen as a PixRect
    ''' </summary>
    ''' <param name="hwnd">The window handle to the window</param>
    ''' <returns>A PixRect object containing the capture of the window</returns>
    Public Shared Function Capture(ByVal hWnd As IntPtr) As clsPixRect
        Using img As Bitmap = CaptureBitmap(hWnd, Rectangle.Empty)
            Return New clsPixRect(img)
        End Using
    End Function

    ''' <summary>
    ''' Capture an area of a window from the screen as a clsPixRect
    ''' </summary>
    ''' <param name="hWnd">The handle of the target window</param>
    ''' <param name="r">The rectangle, relative to the window, defining the region to
    ''' be captured - if the rectangle is <see cref="Rectangle.IsEmpty">empty</see>,
    ''' the entire window is captured.</param>
    Public Shared Function Capture(ByVal hWnd As IntPtr, ByVal r As Rectangle) _
     As clsPixRect
        Using img As Bitmap = CaptureBitmap(hWnd, r)
            Return New clsPixRect(img)
        End Using
    End Function

    ''' <summary>
    ''' Capture an area of a window from the screen as a PixRect compatible string
    ''' </summary>
    ''' <param name="hWnd">The handle of the target window</param>
    ''' <param name="r">The rectangle, relative to the window, defining the region to
    ''' be captured - if the rectangle is <see cref="Rectangle.IsEmpty">empty</see>,
    ''' the entire window is captured.</param>
    Public Shared Function CapturePixRectString(
     ByVal hWnd As IntPtr, ByVal r As Rectangle) As String
        Using img As Bitmap = CaptureBitmap(hWnd, r)
            Return BitmapToString(img)
        End Using
    End Function

    ''' <summary>
    ''' Capture an area of a window from the screen as a bitmap, normalising the pixels
    ''' to 24 bits, ensuring the alpha channel is ignored
    ''' </summary>
    ''' <param name="hWnd">The handle of the target window</param>
    ''' <param name="r">The rectangle, relative to the window, defining the region to
    ''' be captured - if the rectangle is <see cref="Rectangle.IsEmpty">empty</see>,
    ''' the entire window is captured.</param>
    ''' <remarks>The bitmap that this returns should be disposed of once it is
    ''' finished with, or a clsPixRect/String is created from it.</remarks>
    Private Shared Function CaptureBitmap(
     ByVal hWnd As IntPtr, ByVal r As Rectangle) As Bitmap

        Dim bitmap = WindowCapturer.CaptureBitmap(hWnd, r)

        bitmap = BitmapFormatConverter.NormaliseBitmap(bitmap)

        Return bitmap

    End Function

    ''' <summary>
    ''' Captures the entire screen as a PixRect
    ''' </summary>
    Public Shared Function CaptureScreen() As clsPixRect
        Return Capture(modWin32.GetDesktopWindow(), Rectangle.Empty)
    End Function

#End Region

    ''' <summary>
    ''' Gets a list of colours appearing in the entire image together with
    ''' information about how frequently that colour appears.
    ''' </summary>
    ''' <returns>Returns a dictionary of colours in which the key is the colour and
    ''' the value is the frequency with which that colour appears (ie the number of
    ''' pixels having that colour). The order of the colours in the dictionary is
    ''' determined by the order in which they are encountered iterating from left to
    ''' right, one row at a time.</returns>
    Public Function GetColourDistribution() As IDictionary(Of Integer, Integer)
        Return GetColourDistribution(0, Width * Height - 1)
    End Function

    ''' <summary>
    ''' Gets a list of colours appearing in the image together with information
    ''' about how frequently that colour appears.
    ''' </summary>
    ''' <param name="StartIndex">The zero-based start index (inclusive)
    ''' of the portion of interest within the image.</param>
    ''' <param name="EndIndex">The zero-based end index (inclusive) of
    ''' the portion of interest within the image.</param>
    ''' <returns>Returns a dictionary of colours in which the key is
    ''' the colour and the value is the frequency with which that colour
    ''' appears (ie the number of pixels having that colour). The order
    ''' of the colours in the dictionary is determined by the order in which
    ''' they are encountered iterating from left to right, one row
    ''' at a time.</returns>
    Public Function GetColourDistribution(ByVal startIndex As Integer, ByVal endIndex As Integer) As IDictionary(Of Integer, Integer)

        If startIndex < 0 OrElse startIndex >= mWidth * Height Then Throw New ArgumentException(My.Resources.clsPixRect_StartIndexMustBeWithinTheBoundsOfTheArray)
        If endIndex < 0 OrElse endIndex >= mWidth * Height Then Throw New ArgumentException(My.Resources.clsPixRect_EndIndexMustBeWithinTheBoundsOfTheArray)
        If startIndex > endIndex Then Throw New ArgumentException(My.Resources.clsPixRect_StartIndexMustNotExceedEndIndex)

        Dim colours As New clsOrderedDictionary(Of Integer, Integer)

        If mPixels IsNot Nothing Then
            For index As Integer = startIndex To endIndex
                Dim ICol As Integer = mPixels(index)
                If colours.ContainsKey(ICol) Then
                    colours(ICol) += 1
                Else
                    colours.Add(ICol, 1)
                End If
            Next
        End If

        Return colours
    End Function


    ''' <summary>
    ''' Gets the most frequently occurring colour.
    ''' </summary>
    ''' <returns>Retuns the most frequently occurring colour as an integer
    ''' representing the RRGGBB format. Where the most frequently occuring
    ''' colour is non-unique, the first modal colour found in the dictionary
    ''' output by the GetColourDistribution dictionary is returned.</returns>
    Public Function GetModalColoursByRow() As Integer()
        Dim modalColours(mHeight - 1) As Integer

        For y As Integer = 0 To mHeight - 1
            Dim modalFreq As Integer = 0
            Dim modalColour As Integer = Nothing
            For Each kvp As KeyValuePair(Of Integer, Integer) _
             In GetColourDistribution(y * mWidth, (y + 1) * mWidth - 1)
                If kvp.Value > modalFreq Then
                    modalFreq = kvp.Value
                    modalColour = kvp.Key
                End If
            Next

            modalColours(y) = modalColour
        Next

        Return modalColours
    End Function


    ''' <summary>
    ''' Gets the most frequently occurring colour in the whole image.
    ''' </summary>
    ''' <returns>Retuns the most frequently occurring colour as an integer
    ''' representing the RRGGBB format. Where the most frequently occuring
    ''' colour is non-unique, the first modal colour found in the dictionary
    ''' output by the GetColourDistribution dictionary is returned.</returns>
    Public Function GetModalColour() As Integer
        Dim modalFreq As Integer = 0
        Dim modalColour As Integer = Nothing
        For Each kvp As KeyValuePair(Of Integer, Integer) In GetColourDistribution()
            If kvp.Value > modalFreq Then
                modalFreq = kvp.Value
                modalColour = kvp.Key
            End If
        Next

        If modalFreq > 0 Then
            Return modalColour
        Else
            Throw New InvalidOperationException(My.Resources.clsPixRect_UnableToDetermineModalColour)
        End If
    End Function


    ''' <summary>
    ''' Returns a similar pixrect in which the colours have been modified to fit
    ''' a 16 colour palette. Colours will be assigned to the nearest match within
    ''' the palette.
    ''' </summary>
    ''' <param name="avoidColour">A colour onto which other colours should not be
    ''' mapped, unless it matches exactly. Eg if a background colour has been
    ''' specified as part of font recognition then no colour should be mapped
    ''' onto this colour unless it matches exactly. Without this control, light
    ''' grey text might be turned white, in which case it would blend into a
    ''' white background.</param>
    ''' <param name="ignoreColour">Any colour to ignore. Pixels matching this colour
    ''' exactly will be ignored. As a consequence, the returned image may be made
    ''' up of a palette of more than 16 colours. Eg if font text is specified
    ''' as being a very particular colour then we don't want to convert this
    ''' to some other colour.</param>
    ''' <returns>Returns a new clsPixrect object in the new colour palette.</returns>
    Public Function To16ColourPixRect(ByVal avoidColour As Integer, ByVal ignoreColour As Integer) As clsPixRect
        Static palette As New List(Of Color)
        If palette.Count = 0 Then
            palette.Add(Color.White)
            palette.Add(Color.Silver)
            palette.Add(Color.Gray)
            palette.Add(Color.Black)
            palette.Add(Color.Red)
            palette.Add(Color.Maroon)
            palette.Add(Color.Yellow)
            palette.Add(Color.Olive)
            palette.Add(Color.Lime)
            palette.Add(Color.Green)
            palette.Add(Color.Aqua)
            palette.Add(Color.Teal)
            palette.Add(Color.Blue)
            palette.Add(Color.Navy)
            palette.Add(Color.Fuchsia)
            palette.Add(Color.Purple)
        End If

        'Make sure no colour in the palette clashes "avoid colour". We don't
        'want to map colours onto the avoid colour
        For index As Integer = 0 To palette.Count - 1
            Dim c As Color = palette(index)
            If (c.ToArgb() And &HFFFFFF) = avoidColour Then
                palette(index) = Color.FromArgb(((avoidColour + &H20) Mod &HFFFFFF) Or &HFF000000)
            End If
        Next

        Dim newPixels(mPixels.Length - 1) As Integer
        For index As Integer = 0 To mPixels.Length - 1
            Dim i As Integer = mPixels(index)
            If i <> ignoreColour Then
                Dim minDistance As Double = Double.MaxValue
                Dim chosenColour As Color
                For Each testColour As Color In palette
                    Dim distance As Double = GetColourDistance(Color.FromArgb(i Or &HFF000000), testColour)
                    If distance < minDistance Then
                        minDistance = distance
                        chosenColour = testColour
                    End If
                Next

                newPixels(index) = chosenColour.ToArgb() And &HFFFFFF
            Else
                newPixels(index) = i
            End If
        Next

        Return New clsPixRect(newPixels, mWidth, mHeight)

    End Function


    ''' <summary>
    ''' Computes a numeric value as a measure of the similarity of two colours.
    ''' </summary>
    ''' <param name="c1">The first colour of interest. The order of the two
    ''' parameters is not important.</param>
    ''' <param name="c2">The second colour of interest. The order of the two
    ''' parameters is not important.</param>
    ''' <returns>Returns a numeric value representing the distance between
    ''' two colours. A value of zero indicates that the colours are identical.
    ''' This function is an equivalence relation (symmetric, transitive and
    ''' reflexive).</returns>
    Private Function GetColourDistance(ByVal c1 As Color, ByVal c2 As Color) As Double
        'http://www.compuphase.com/cmetric.htm
        Dim avgR As Integer = (CInt(c1.R) + CInt(c2.R)) \ 2
        Dim weightedRDistance As Double = (2 + (avgR / 255)) * Math.Pow(CInt(c1.R) - CInt(c2.R), 2)
        Dim weightedGDistance As Double = 4 * Math.Pow(CInt(c1.G) - CInt(c2.G), 2)
        Dim weightedBDistance As Double = (2 - (avgR / 255)) * Math.Pow(CInt(c1.B) - CInt(c2.B), 2)
        Return weightedRDistance + weightedGDistance + weightedBDistance
    End Function

    ''' <summary>
    ''' Checks if the given object matches this one. It is considered equal if it is
    ''' a non-null PixRect object representing exactly the same image as this object.
    ''' </summary>
    ''' <param name="obj">The object to test if it's equal to this.</param>
    ''' <returns>True if the given object is a non-null PixRect representing the
    ''' same image as this object.</returns>
    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Return Equals(TryCast(obj, clsPixRect))
    End Function

    ''' <summary>
    ''' Checks if the image defined in this pixrect object matches that described in
    ''' the given object.
    ''' </summary>
    ''' <param name="p">The PixRect object to compare against</param>
    ''' <returns>True if the two PixRect objects match exactly, false otherwise.
    ''' </returns>
    Public Overloads Function Equals(ByVal p As clsPixRect) As Boolean
        Return (p IsNot Nothing AndAlso
         mWidth = p.mWidth AndAlso mHeight = p.mHeight AndAlso
         CollectionUtil.AreEqual(mPixels, p.mPixels)
        )
    End Function


    ''' <summary>
    ''' Erase a particular colour in the given range of rows, replacing it with
    ''' another. All colours not of that particular colour are replaced with a
    ''' different 'alternative' colour!?
    ''' </summary>
    ''' <param name="y1">The Y coordinate of the top row.</param>
    ''' <param name="y2">The Y coordinate of the bottom row.</param>
    ''' <param name="colourToErase">The colour to erase.</param>
    ''' <param name="replacementColour">The colour to replace it with.</param>
    ''' <param name="alternativeReplacementColour">The alternative colour, to
    ''' replace non-matching colours with.</param>
    Public Sub EraseColourInRows(ByVal y1 As Integer, ByVal y2 As Integer, ByVal colourToErase As Integer, ByVal replacementColour As Integer, ByVal alternativeReplacementColour As Integer)

        If y1 < 0 Or y1 >= mHeight Then
            Throw New ArgumentException(My.Resources.clsPixRect_Y1IsOutOfBounds)
        End If
        If y2 < 0 Or y2 >= mHeight Then
            Throw New ArgumentException(My.Resources.clsPixRect_Y2IsOutOfBounds)
        End If
        If y1 > y2 Then Throw New ArgumentException(My.Resources.clsPixRect_Y1MustNotExceedY2)

        For y As Integer = y1 To y2
            For x As Integer = 0 To mWidth - 1
                Dim index As Integer = x + y * mWidth
                If mPixels(index) = colourToErase Then
                    mPixels(index) = replacementColour
                Else
                    mPixels(index) = alternativeReplacementColour
                End If
            Next
        Next
    End Sub

End Class


