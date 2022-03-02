Option Strict On

Imports System.Collections.Generic
Imports System.IO
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.BPCoreLib
Imports BluePrism.CharMatching
Imports System.Reflection

Public Class frmFontEditor

    ''' <summary>
    ''' The font data we are currently working with, and a flag to say whether it
    ''' has been modified or not.
    ''' </summary>
    Private mFont As FontData
    Private mFontModified As Boolean

    ''' <summary>
    ''' Currently selected text colour - initialised to Color.Empty, to signify that we
    ''' will take a guess the first time a capture happens.
    ''' </summary>
    Private mSelectedTextCol As Color = Color.Empty

    ''' <summary>
    ''' Our parent form. Must be set before the form is shown.
    ''' </summary>
    Public mParent As frmMain

    ''' <summary>
    ''' The image we have captured.
    ''' </summary>
    Private mCapturedImage As Bitmap

    ''' <summary>
    ''' The zoom scale for the captured image window.
    ''' </summary>
    Private mCapturedImageZoom As Integer


    ''' <summary>
    ''' Called when the form is loaded.
    ''' </summary>
    Private Sub frmFontEditor_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If mParent Is Nothing Then Close()
        NewFont()
        mCapturedImage = Nothing
        mCapturedImageZoom = 1
        UpdateCapturedImageWindow()
        UpdateGreys()
    End Sub

    ''' <summary>
    ''' Create a new font for editing.
    ''' </summary>
    Private Sub NewFont()
        mFont = New FontData()
        mFontModified = False
        UpdateFontDisplay()
    End Sub


    ''' <summary>
    ''' Update the UI status, i.e. what is greyed out and what is not.
    ''' </summary>
    Private Sub UpdateGreys()
        lblTextCol.Visible = mCapturedImage IsNot Nothing
        btnAddToFont.Enabled = False
        lblTextCol.Text = String.Format(My.Resources.TextColour0, mSelectedTextCol.Name)

        btnAddToFont.Enabled = False
        For Each c As Control In pnlDetected.Controls
            If TypeOf c Is TextBox AndAlso CType(c, TextBox).Text.Length > 0 Then
                btnAddToFont.Enabled = True
                Exit For
            End If
        Next

        btnZoomIn.Enabled = mCapturedImageZoom < 5 AndAlso mCapturedImage IsNot Nothing
        btnZoomOut.Enabled = mCapturedImageZoom > 1 AndAlso mCapturedImage IsNot Nothing
        If mCapturedImage Is Nothing Then
            lblCapZoom.Text = ""
        Else
            lblCapZoom.Text = String.Format(My.Resources.x0X, mCapturedImageZoom)
        End If

    End Sub

    ''' <summary>
    ''' Capture a bitmap from the current target application, or from any old
    ''' application if nothing has been launched or attached to.
    ''' </summary>
    Private Sub btnCapture_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCapture.Click

        'Get the connected target app from our parent. If there
        'isn't one, create a new one so we can just spy any old
        'application...
        Dim t As clsTargetApp = Nothing
        If mParent.mAMI IsNot Nothing AndAlso mParent.mAMI.GetTargetApp IsNot Nothing Then
            t = mParent.mAMI.GetTargetApp
        Else
            t = New clsLocalTargetApp()
        End If

        'Capture the image using the spy tool...
        Dim sResponse As String = t.ProcessQuery("spybitmap")
        Dim sResultType As String = Nothing
        Dim sResult As String = Nothing
        clsQuery.ParseResponse(sResponse, sResultType, sResult)
        Select Case sResultType
            Case "BITMAP"
            Case "ERROR"
                ShowMessage(String.Format(My.Resources.CaptureFailed0, sResult))
                Exit Sub
            Case "CANCEL"
                Exit Sub
            Case Else
                ShowMessage(String.Format(My.Resources.CaptureErrorUnexpectedResultType0, sResultType))
                Exit Sub
        End Select

        Activate()

        Dim b As Bitmap = clsPixRect.ParseBitmap(sResult)
        mCapturedImage = b
        UpdateCapturedImageWindow()

        'Take a guess at the text colour if it has not already been selected...
        If mSelectedTextCol = Color.Empty Then
            mSelectedTextCol = GuessTextCol(b)
        End If

        UpdateDetectedCharacters()
        UpdateGreys()

    End Sub


    ''' <summary>
    ''' Guess what the text colour is in the given bitmap. We just look for the darkest
    ''' colour.
    ''' </summary>
    ''' <param name="bmp">The bitmap to look at.</param>
    ''' <returns>The best guess at the text colour.</returns>
    Private Function GuessTextCol(ByVal bmp As Bitmap) As Color
        Dim guess As Color = Color.FromArgb(255, 255, 255, 255)
        For y As Integer = 0 To bmp.Height - 1
            For x As Integer = 0 To bmp.Width - 1
                Dim c As Color = bmp.GetPixel(x, y)
                If c.GetBrightness() < guess.GetBrightness() Then
                    guess = c
                End If
            Next
        Next
        Return guess
    End Function


    ''' <summary>
    ''' Clicking the captured image chooses the pixel under the mouse pointer as the
    ''' text colour.
    ''' </summary>
    Private Sub pbCaptured_MouseClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles pbCaptured.MouseClick
        If e.Button = Windows.Forms.MouseButtons.Left Then
            If mCapturedImage Is Nothing Then Exit Sub
            Try
                mSelectedTextCol = mCapturedImage.GetPixel(e.X \ mCapturedImageZoom, e.Y \ mCapturedImageZoom)
                UpdateDetectedCharacters()
                UpdateGreys()
            Catch ex As Exception
                'Don't do anything - probably the click was outside the bounds of an image
                'that is smaller than the window.
            End Try
        End If
    End Sub


    ''' <summary>
    ''' Get a black and white representation of the selected image, where black is
    ''' any pixel in the currently selected 'text colour', and white is anything
    ''' else, i.e. the background.
    ''' </summary>
    ''' <param name="bm1">The source image</param>
    ''' <returns>A new black and white image</returns>
    Private Function GetBlackAndWhite(ByVal bm1 As Bitmap, ByVal ActiveColour As Color) As Bitmap

        Dim c As Color
        Dim bm2 As New Bitmap(bm1.Width, bm1.Height, Imaging.PixelFormat.Format32bppArgb)

        For y As Integer = 0 To bm1.Height - 1
            For x As Integer = 0 To bm1.Width - 1
                c = bm1.GetPixel(x, y)
                If c.ToArgb = ActiveColour.ToArgb Then
                    bm2.SetPixel(x, y, Color.FromArgb(255, 0, 0, 0))
                Else
                    bm2.SetPixel(x, y, Color.FromArgb(255, 255, 255, 255))
                End If
            Next
        Next
        Return bm2

    End Function



    ''' <summary>
    ''' Scan the captured image for characters.
    ''' </summary>
    ''' <returns>A list of bitmaps that were found.</returns>
    Private Function GetCharactersFromCapturedImage(ByVal ActiveColour As Color) As List(Of Bitmap)
        Return Me.GetCharactersFromImage(mCapturedImage, ActiveColour)
    End Function

    ''' <summary>
    ''' Scans the supplied image for characters, using vertical slicing.
    ''' </summary>
    ''' <param name="CapturedImage">The image to scan</param>
    ''' <returns>A list of bitmaps that were found.</returns>
    Private Function GetCharactersFromImage(ByVal CapturedImage As Bitmap, ByVal ActiveColour As Color) As List(Of Bitmap)

        Dim lst As New List(Of Bitmap)
        If CapturedImage Is Nothing Then Return lst
        Dim bmp As Bitmap = GetBlackAndWhite(CapturedImage, ActiveColour)

        Dim bSpaceColumn As Boolean
        Dim iFirstCharacterColumn As Integer = -1
        Dim iFirstSpaceColumn As Integer = -1
        Dim iSpaceWidth As Integer = 0

        For x As Integer = 0 To bmp.Width - 1
            For y As Integer = 0 To bmp.Height - 1
                If bmp.GetPixel(x, y).ToArgb = Color.Black.ToArgb Then
                    'The left hand edge of a character has been found.
                    If iFirstCharacterColumn = -1 Then
                        iFirstCharacterColumn = x
                    End If
                    bSpaceColumn = False
                    Exit For
                End If
                bSpaceColumn = True
            Next

            If bSpaceColumn Then

                If iFirstCharacterColumn = -1 Then
                    If lst.Count = 0 Then
                        'This space column is to the left of the first character.
                    Else
                        iSpaceWidth += 1
                    End If
                Else
                    'The left edge of a character has been found and 
                    'this space column is at the right hand edge.
                    If iSpaceWidth > 0 Then
                        'There was a space on the left of this character.
                        'Count this column for the next character
                        iSpaceWidth = 1
                    End If

                    'Add the character bitmap
                    iFirstSpaceColumn = x
                    Dim extractedbmp As Bitmap = bmp.Clone(New Rectangle(iFirstCharacterColumn, 0, iFirstSpaceColumn - iFirstCharacterColumn, bmp.Height), Imaging.PixelFormat.Format32bppRgb)
                    lst.Add(extractedbmp)

                    'Reset for the next column.
                    iFirstCharacterColumn = -1
                End If
            End If

        Next
        Return lst
    End Function

    ''' <summary>
    ''' Scans the supplied image for characters, using advanced slicing.
    ''' </summary>
    ''' <param name="CapturedImage">The image to scan</param>
    ''' <returns>A list of bitmaps that were found.</returns>
    Private Function GetCharactersFromImageAdvanced(ByVal CapturedImage As Bitmap, ByVal ActiveColour As Color) As List(Of Bitmap)
        AutoTrimImage(CapturedImage, ActiveColour)
        AutoTrimImageSides(CapturedImage, ActiveColour)
        Return Me.SplitImageAdvanced(CapturedImage, ActiveColour)

        'This may be faster than the above, in some circumstances.
        'Dim RetList As New List(Of Bitmap)
        'For Each bm As Bitmap In me.GetCharactersFromImage(capturedimage,activecolour)
        '   Debug.WriteLine("Doing " & lst.IndexOf(bm))
        '   Me.AutoTrimImage(bm, ActiveColour)
        '   RetList.AddRange(SplitImage(bm, ActiveColour))
        'Next
        'Return RetList
    End Function

    ''' <summary>
    ''' Trims any non-active "white" columns from the left and right hand side
    ''' of the supplied image.
    ''' </summary>
    ''' <param name="BM">The image to be trimmed.</param>
    ''' <param name="ActiveColour">The active (text) colour.</param>
    Private Shared Sub AutoTrimImageSides(ByRef BM As Bitmap, ByVal ActiveColour As Color)
        Dim FirstNonEmptyColumn As Integer = -1
        For x As Integer = 0 To BM.Width - 1
            Dim Empty As Boolean = True
            For y As Integer = 0 To BM.Height - 1
                If BM.GetPixel(x, y).ToArgb = ActiveColour.ToArgb Then
                    Empty = False
                    Exit For
                End If
            Next
            If Not Empty Then
                FirstNonEmptyColumn = x
                Exit For
            End If
        Next

        Dim LastNonEmptyColumn As Integer = Integer.MaxValue
        For x As Integer = BM.Width - 1 To 0 Step -1
            Dim Empty As Boolean = True
            For y As Integer = 0 To BM.Height - 1
                If BM.GetPixel(x, y).ToArgb = ActiveColour.ToArgb Then
                    Empty = False
                    Exit For
                End If
            Next
            If Not Empty Then
                LastNonEmptyColumn = x
                Exit For
            End If
        Next

        If LastNonEmptyColumn < BM.Height OrElse FirstNonEmptyColumn > 0 Then
            Dim NewWidth As Integer = LastNonEmptyColumn - FirstNonEmptyColumn + 1
            BM = BM.Clone(New Rectangle(FirstNonEmptyColumn, 0, NewWidth, BM.Height), BM.PixelFormat)
        End If
    End Sub

    ''' <summary>
    ''' Splits the supplied image into its smallest contiguous components,
    ''' in all places possible.
    ''' </summary>
    ''' <param name="BM">The image to be sliced.</param>
    ''' <param name="ActiveColour">The active colour (ie the text colour).</param>
    ''' <returns>Returns the segments sliced from this image, minus the intervening
    ''' white space.</returns>
    ''' <remarks>Use with caution - your % symbol will be split into 3 parts,
    ''' for exmample.</remarks>
    Private Function SplitImageAdvanced(ByVal BM As Bitmap, ByVal ActiveColour As Color) As List(Of Bitmap)
        AutoTrimImageSides(BM, ActiveColour)
        Dim Images As New List(Of Bitmap)

        Dim Path As List(Of Point) = Me.FindPathThroughImage(BM, ActiveColour)
        If Path IsNot Nothing Then
            Dim LeftMost As Integer = Integer.MaxValue
            Dim RightMost As Integer = Integer.MinValue
            For Each p As Point In Path
                LeftMost = Math.Min(LeftMost, p.X)
                RightMost = Math.Max(RightMost, p.X)
            Next

            Dim LeftImage As New Bitmap(RightMost, BM.Height)
            Dim g As Graphics = Graphics.FromImage(LeftImage)
            g.Clear(Color.White)
            For y As Integer = 0 To BM.Height - 1
                Dim LocalRightMost As Integer = -1
                For i As Integer = 0 To Path.Count - 1
                    If Path(i).Y = y Then
                        LocalRightMost = Math.Max(LocalRightMost, Path(i).X)
                    End If
                Next

                For x As Integer = 0 To LocalRightMost - 1
                    LeftImage.SetPixel(x, y, BM.GetPixel(x, y))
                Next
            Next
            AutoTrimImageSides(LeftImage, ActiveColour)
            Images.AddRange(SplitImageAdvanced(LeftImage, ActiveColour))

            Dim RightImage As New Bitmap(BM.Width - LeftMost, BM.Height)
            g = Graphics.FromImage(RightImage)
            g.Clear(Color.White)
            For y As Integer = 0 To BM.Height - 1
                Dim LocalRightMost As Integer = -1
                For i As Integer = 0 To Path.Count - 1
                    If Path(i).Y = y Then
                        LocalRightMost = Math.Max(LocalRightMost, Path(i).X)
                    End If
                Next

                For x As Integer = 0 To (BM.Width - 1) - LocalRightMost - 1
                    RightImage.SetPixel(x + LocalRightMost - LeftMost, y, BM.GetPixel(LocalRightMost + 1 + x, y))
                Next
            Next
            AutoTrimImageSides(RightImage, ActiveColour)
            Images.AddRange(SplitImageAdvanced(RightImage, ActiveColour))
        Else
            Images.Add(BM)
        End If

        Return Images
    End Function

    ''' <summary>
    ''' Finds a path through the supplied image, from the top row to the bottom row,
    ''' avoiding pixels of the specifed "Active" colour. This may be a diagonal path,
    ''' as well as a straight path from top to bottom.
    ''' </summary>
    ''' <param name="BM">The image to find a path through.</param>
    ''' <param name="ActiveColour">The colour to be worked around.</param>
    ''' <returns>Returns a sequence of points, representing a path through the image.
    ''' </returns>
    ''' <remarks>Can be useful for splitting two characters which are too close
    ''' together for a vertical line to separate them, but which are non-the-less
    ''' distinct. This happens often in italic fonts. Note however, that it will split
    ''' double quote marks ("), percentage signs, etc, so use with caution.</remarks>
    Private Function FindPathThroughImage(ByVal BM As Bitmap, ByVal ActiveColour As Color) As List(Of Point)
        For x As Integer = 0 To BM.Width - 1
            If BM.GetPixel(x, 0).ToArgb = Color.White.ToArgb Then
                Dim L As New List(Of Point)
                L.Add(New Point(x, 0))
                Dim DisAllowedPoints As New List(Of Point)
                Dim Res As List(Of Point) = FindPathThroughImage(BM, ActiveColour, L, DisAllowedPoints)
                If Res IsNot Nothing Then Return Res
            End If
        Next

        Return Nothing
    End Function

    ''' <summary>
    ''' Finds a path through the supplied image, from the end of the supplied
    ''' part-path, down to the bottom row,
    ''' avoiding pixels of the specifed "Active" colour. This may be a diagonal path,
    ''' as well as a straight path from top to bottom.
    ''' </summary>
    ''' <param name="BM">The image to find a path through.</param>
    ''' <param name="ActiveColour">The colour to be worked around.</param>
    ''' <returns>Returns a sequence of points, representing a path through the image.
    ''' </returns>
    ''' <param name="PathSoFar">The trial path found through the image so far.</param>
    ''' <param name="DisAllowedPoints">Points which must not be visited by the path.
    ''' Used primarily by this function to optimise its recursive call, but
    ''' you may specify for own in addition. Safe to ignore.</param>
    ''' <remarks>Can be useful for splitting two characters which are too close
    ''' together for a vertical line to separate them, but which are non-the-less
    ''' distinct. This happens often in italic fonts. Note however, that it will split
    ''' double quote marks ("), percentage signs, etc, so use with caution.</remarks>
    Private Function FindPathThroughImage(ByVal BM As Bitmap, ByVal ActiveColour As Color, ByVal PathSoFar As List(Of Point), ByVal DisAllowedPoints As List(Of Point)) As List(Of Point)
        Dim LastPoint As Point = PathSoFar.Item(PathSoFar.Count - 1)
        If LastPoint.Y = BM.Height - 1 Then Return PathSoFar

        'We can navigate down, left or right
        'We prefer down because it's likely to find a result faster
        Dim Increments As Size() = {New Size(0, 1), New Size(-1, 0), New Size(1, 0)}


        For Each inc As Size In Increments
            Dim NewPoint As Point = Point.Add(LastPoint, inc)
            If Not (PathSoFar.Contains(NewPoint) OrElse DisAllowedPoints.Contains(NewPoint)) Then
                If NewPoint.X >= 0 AndAlso NewPoint.X <= BM.Width - 1 Then
                    Dim ColourFound As Color = BM.GetPixel(NewPoint.X, NewPoint.Y)
                    If ColourFound.ToArgb <> ActiveColour.ToArgb Then
                        Dim TrialPath As New List(Of Point)
                        TrialPath.AddRange(PathSoFar)
                        TrialPath.Add(NewPoint)
                        DisAllowedPoints.Add(NewPoint)
                        Dim res As List(Of Point) = Me.FindPathThroughImage(BM, ActiveColour, TrialPath, DisAllowedPoints)
                        If res IsNot Nothing Then Return res
                    End If
                End If
            End If
        Next

        Return Nothing
    End Function


    ''' <summary>
    ''' Update the detected characters panel. Removes any existing controls and
    ''' creates a new set based on the captured image and the selected text colour.
    ''' </summary>
    Private Sub UpdateDetectedCharacters()
        LayoutCharacters(GetCharactersFromCapturedImage(Me.mSelectedTextCol))
    End Sub


    Private Sub LayoutCharacters(ByVal CharacterImages As List(Of Bitmap))
        'Remove any controls we have added previously.
        While pnlDetected.Controls.Count > 0
            Dim c As Control = pnlDetected.Controls(0)
            pnlDetected.Controls.Remove(c)
            c.Dispose()
        End While

        Dim iTop As Integer = 3
        Dim xpos As Integer = 3

        For Each bmp As Bitmap In CharacterImages
            Dim ControlName As String = Guid.NewGuid.ToString.Substring(0, 8)

            Dim txt As New TextBox()
            txt.Name = "txt_" & ControlName
            txt.Location = New Point(xpos, iTop)
            txt.Width = Math.Max(20, bmp.Width)
            txt.Font = New Font("Segoe UI", 8.25)
            AddHandler txt.KeyUp, AddressOf CharMapTxt_KeyUp
            AddHandler txt.TextChanged, AddressOf CharMapTxt_TextChanged

            'See if we can autodetect text
            Dim Result As String = Nothing, sErr As String = Nothing
            Dim iCol As Integer = mSelectedTextCol.R << 16 + mSelectedTextCol.G << 8 + mSelectedTextCol.B

            Try
                txt.Text = FontReader.ReadTextSingleLine(
                 False, New clsPixRect(bmp), mFont, iCol)
            Catch
            End Try
            pnlDetected.Controls.Add(txt)

            Dim pb As New PictureBox()
            pb.Name = "pb_" & ControlName
            pb.Location = New Point(xpos, iTop + txt.Height + 5)
            pb.Height = bmp.Height
            pb.Width = bmp.Width
            pb.Image = bmp
            pnlDetected.Controls.Add(pb)
            AddHandler pb.DoubleClick, AddressOf HandlePBoxDoubleClick
            AddHandler pb.MouseClick, AddressOf HandleDetectedCharacterImageClick

            txt.Tag = pb

            Dim ch As New CheckBox
            ch.Name = "ch_" & ControlName
            ch.Checked = txt.Text = "" 'if the character was successfully detected then we can probably ignore
            ch.Location = New Point(xpos, pb.Bottom + 5)
            ch.Anchor = AnchorStyles.Top Or AnchorStyles.Left
            ch.Width = txt.Width
            ch.TabStop = False
            pnlDetected.Controls.Add(ch)
            ch.BringToFront()

            xpos += txt.Width + 5
        Next
    End Sub


    ''' <summary>
    ''' Event handler called when a key is released in one of the text boxes in the
    ''' detected characters panel.
    ''' </summary>
    Private Sub CharMapTxt_KeyUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs)
        UpdateGreys()
    End Sub

    Private Sub CharMapTxt_TextChanged(ByVal sender As Object, ByVal e As EventArgs)
        'We assume that if the user changes the text, he will want to
        'incorporate this change into the font
        Dim txt As TextBox = TryCast(sender, TextBox)
        Dim NameFragment As String = txt.Name.Substring(4)
        Dim ch As CheckBox = TryCast(pnlDetected.Controls("ch_" & NameFragment), CheckBox)
        If ch IsNot Nothing Then
            ch.Checked = txt.Text <> ""
        End If
    End Sub



    ''' <summary>
    ''' Load some font data - handles the Load button.
    ''' </summary>
    Private Sub btnLoad_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnLoad.Click
        Try
            Dim f As New OpenFileDialog()
            f.DefaultExt = "xml"
            If Not String.IsNullOrEmpty(LastFontFile) Then
                f.InitialDirectory = LastFontFile
            End If
            If f.ShowDialog(Me) = DialogResult.OK Then
                LastFontFile = f.FileName
                mFont = New FontData(File.ReadAllText(f.FileName))
                mFontModified = False
                UpdateFontDisplay()
            End If
        Catch ex As Exception
            ShowMessage(My.Resources.LoadFailed & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' The last font file retrieved from a load/save font dialog, if any.
    ''' </summary>
    Private LastFontFile As String = "c:\blueprism\ApplicationManager\FontReader\"

    ''' <summary>
    ''' Save current font data - handles the Save button.
    ''' </summary>
    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        Try
            Dim f As New SaveFileDialog()
            If Not String.IsNullOrEmpty(LastFontFile) Then
                f.InitialDirectory = LastFontFile
            End If
            If f.ShowDialog(Me) = DialogResult.OK Then
                LastFontFile = f.FileName
                File.WriteAllText(f.FileName, mFont.GetXML())
                mFontModified = False
            End If
        Catch ex As Exception
            ShowMessage(My.Resources.SaveFailed & ex.Message)
        End Try
    End Sub


    ''' <summary>
    ''' Create a new font - handles the New button.
    ''' </summary>
    Private Sub btnNew_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNew.Click
        NewFont()
    End Sub


    ''' <summary>
    ''' Show an error message dialog.
    ''' </summary>
    ''' <param name="message">The message text.</param>
    Private Sub ShowMessage(ByVal message As String)
        MsgBox(message, , Me.Text)
    End Sub

    ''' <summary>
    ''' Add new data from the 'detected characters' box to the font.
    ''' </summary>
    Private Sub btnAddToFont_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddToFont.Click

        'Go through each TextBox control...
        For Each c As Control In pnlDetected.Controls
            If TypeOf c Is TextBox Then
                Dim txt As TextBox = CType(c, TextBox)
                'See if the user has named this character...
                If txt.Text.Length > 0 Then

                    'And check the user has selected this character
                    Dim NameFragment As String = txt.Name.Substring(4)
                    Dim ch As CheckBox = TryCast(pnlDetected.Controls("ch_" & NameFragment), CheckBox)
                    If ch IsNot Nothing AndAlso ch.Checked Then
                        Dim bmp As Bitmap = CType(CType(txt.Tag, PictureBox).Image, Bitmap)
                        Dim fc As New CharData(txt.Text.Chars(0), bmp)
                        mFont.AddCharacter(fc, True)
                        mFontModified = True
                    End If

                End If
            End If
        Next
        If mFontModified Then
            UpdateFontDisplay()
        End If

    End Sub

    ''' <summary>
    ''' Update the window displaying the current font data.
    ''' </summary>
    Private Sub UpdateFontDisplay()

        'Remove any controls we have added previously.
        While pnlFontData.Controls.Count > 0
            Dim c As Control = pnlFontData.Controls(0)
            pnlFontData.Controls.Remove(c)
            c.Dispose()
        End While

        Dim iTop As Integer = 3
        Dim xpos As Integer = 3

        'Get the list of characters in the font, but instead of the internal sort order
        'of widest first, we need to present it in character order...
        Dim lst As New List(Of CharData)
        lst.AddRange(mFont.Characters)
        lst.Sort(CharData.CharOnlyComparer)

        For Each c As CharData In lst

            Dim txt As New TextBox()
            txt.Location = New Point(xpos, iTop)
            txt.Width = Math.Max(20, c.Width)
            txt.Font = New Font("Segoe UI", 8.25)
            txt.Text = c.Value
            pnlFontData.Controls.Add(txt)

            Dim pb As New PictureBox()
            Dim bmp As Bitmap = CreateBitmapFromCharacter(c)
            pb.Location = New Point(xpos, iTop + txt.Height + 5)
            pb.Height = bmp.Height
            pb.Width = bmp.Width
            pb.Image = bmp
            pb.Tag = c
            pnlFontData.Controls.Add(pb)
            AddHandler pb.DoubleClick, AddressOf HandlePBoxDoubleClick

            txt.Tag = c
            xpos += txt.Width + 5

        Next


    End Sub

    Private Sub HandlePBoxDoubleClick(ByVal Sender As Object, ByVal e As EventArgs)
        Dim pb As PictureBox = TryCast(Sender, PictureBox)
        If pb IsNot Nothing Then
            Dim FontChar As CharData = CType(pb.Tag, CharData)
            Dim f As New frmFontCharPopup
            Try
                f.SetCharacterImage(pb.Image, FontChar.StateMask)

                f.ShowDialog()
            Finally
                f.Dispose()
            End Try
        End If
    End Sub


    ''' <summary>
    ''' Create a Bitmap that shows the data stored in the given font character.
    ''' </summary>
    ''' <param name="c">The CharData</param>
    ''' <returns>A new Bitmap</returns>
    Private Function CreateBitmapFromCharacter(ByVal c As CharData) As Bitmap
        Return c.ToBitmap
    End Function


    ''' <summary>
    ''' Update the captured image window.
    ''' </summary>
    Private Sub UpdateCapturedImageWindow()

        'Very simple if we don't have an image...
        If mCapturedImage Is Nothing Then
            pbCaptured.Image = Nothing
            Exit Sub
        End If

        Dim b As New Bitmap(mCapturedImage, mCapturedImage.Width * mCapturedImageZoom, mCapturedImage.Height * mCapturedImageZoom)
        pbCaptured.Image = b

        Me.pbCaptured.Width = Math.Max(Me.pbCaptured.Image.Width * mCapturedImageZoom, Me.Panel1.Width)
        Me.pbCaptured.Height = Math.Max(Me.pbCaptured.Image.Height * mCapturedImageZoom, Me.Panel1.Height)
        Me.Capture = True
    End Sub

    Private Sub btnZoomIn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnZoomIn.Click
        If mCapturedImageZoom < 5 Then
            mCapturedImageZoom += 1
            UpdateCapturedImageWindow()
            UpdateGreys()
        End If
    End Sub

    Private Sub btnZoomOut_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnZoomOut.Click
        If mCapturedImageZoom > 1 Then
            mCapturedImageZoom -= 1
            UpdateCapturedImageWindow()
            UpdateGreys()
        End If
    End Sub

    ''' <summary>
    ''' The last bitmap file opened, if any.
    ''' </summary>
    Private mLastBitmapFile As String


    Private Sub btnLoadBitmap_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnLoadBitmap.Click
        Dim f As OpenFileDialog = Nothing
        Try
            f = New OpenFileDialog
            If Not String.IsNullOrEmpty(mLastBitmapFile) Then
                f.InitialDirectory = mLastBitmapFile
            End If
            f.ShowDialog()
            If f.FileName <> "" Then
                mLastBitmapFile = f.FileName
                Dim b As New Bitmap(f.FileName)
                mCapturedImage = b
                UpdateCapturedImageWindow()

                'Take a guess at the text colour if it has not already been selected...
                If mSelectedTextCol = Color.Empty Then
                    mSelectedTextCol = GuessTextCol(b)
                End If

                UpdateDetectedCharacters()
                UpdateGreys()
            End If
        Catch ex As Exception
            MsgBox(ex.ToString)
        Finally
            If f IsNot Nothing Then f.Dispose()
        End Try
    End Sub


    Private Sub Panel1_SizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Panel1.SizeChanged
        If Me.pbCaptured.Image IsNot Nothing Then
            Me.pbCaptured.Height = Math.Max(Me.pbCaptured.Image.Height, Me.Panel1.Height)
            Me.pbCaptured.Width = Math.Max(Me.pbCaptured.Image.Width, Me.Panel1.Width)
        End If
    End Sub


    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If MsgBox(My.Resources.WarningYouShouldOnlyTrimYourFontOnceALLOfYourCharactersHaveBeenAddedAreYouSureY, MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            Dim f As New frmFontTrim
            Try
                f.ShowFont(mFont)
            Catch ex As Exception
                MsgBox(String.Format(My.Resources.UnexpectedError0, ex.ToString))
            Finally
                f.Dispose()
                Me.UpdateFontDisplay()
            End Try
        End If
    End Sub



    Private Sub llSelectAll_LinkClicked(ByVal sender As Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles llSelectAll.LinkClicked
        For Each c As Control In pnlDetected.Controls
            Dim ch As CheckBox = TryCast(c, CheckBox)
            If ch IsNot Nothing Then
                ch.Checked = True
            End If
        Next
    End Sub

    Private Sub llSelectNone_LinkClicked(ByVal sender As Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles llSelectNone.LinkClicked
        For Each c As Control In pnlDetected.Controls
            Dim ch As CheckBox = TryCast(c, CheckBox)
            If ch IsNot Nothing Then
                ch.Checked = False
            End If
        Next
    End Sub


    Private Sub llClear_LinkClicked(ByVal sender As Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles llClear.LinkClicked
        Dim ToRemove As New List(Of Control)

        For Each c As Control In pnlDetected.Controls
            Dim ch As CheckBox = TryCast(c, CheckBox)
            If ch IsNot Nothing Then
                If Not ch.Checked Then
                    Debug.WriteLine(ch.Bounds.ToString)
                    Dim NameFragment As String = ch.Name.Substring(3)
                    Dim txt As TextBox = TryCast(pnlDetected.Controls("txt_" & NameFragment), TextBox)
                    Dim pb As PictureBox = TryCast(pnlDetected.Controls("pb_" & NameFragment), PictureBox)

                    If txt IsNot Nothing AndAlso pb IsNot Nothing Then
                        ToRemove.Add(txt)
                        ToRemove.Add(ch)
                        ToRemove.Add(pb)
                    End If
                End If
            End If
        Next

        For Each c As Control In ToRemove
            Me.pnlDetected.Controls.Remove(c)
        Next

        Me.RelayoutDetectedControls()
    End Sub

    ''' <summary>
    ''' Performs layout on the middle band of textbox/image/checkbox triplets,
    ''' which display the individual characters detected in the sample image.
    ''' </summary>
    Private Sub RelayoutDetectedControls()
        Me.pnlDetected.AutoScrollPosition = Point.Empty

        Dim spacing As Integer = 5
        Dim iLeft As Integer = spacing
        For Each c As Control In Me.pnlDetected.Controls
            Dim txt As TextBox = TryCast(c, TextBox)
            If txt IsNot Nothing Then
                txt.Left = iLeft
                iLeft += txt.Width + spacing

                Dim NameFragment As String = txt.Name.Substring(4)
                Dim ch As CheckBox = TryCast(pnlDetected.Controls("ch_" & NameFragment), CheckBox)
                Dim pb As PictureBox = TryCast(pnlDetected.Controls("pb_" & NameFragment), PictureBox)

                If ch IsNot Nothing AndAlso pb IsNot Nothing Then
                    ch.Left = txt.Left
                    pb.Left = txt.Left
                End If
            End If
        Next
    End Sub


    Private Function BuildCapturedImageContextMenu() As ContextMenuStrip
        Dim cm As New ContextMenuStrip
        Dim Asm As [Assembly]

        Static SliceTop As Image = Nothing
        If SliceTop Is Nothing Then
            Asm = [Assembly].GetExecutingAssembly()
            Using ss As IO.Stream = Asm.GetManifestResourceStream("BluePrism.ApplicationManager.SliceTop.png")
                If ss IsNot Nothing Then
                    SliceTop = Image.FromStream(ss, True)
                End If
            End Using
        End If

        Static SliceBottom As Image = Nothing
        If SliceBottom Is Nothing Then
            Asm = [Assembly].GetExecutingAssembly()
            Using ss As IO.Stream = Asm.GetManifestResourceStream("BluePrism.ApplicationManager.SliceBottom.png")
                If ss IsNot Nothing Then
                    SliceBottom = Image.FromStream(ss, True)
                End If
            End Using
        End If

        Static SliceBoth As Image = Nothing
        If SliceBoth Is Nothing Then
            Asm = [Assembly].GetExecutingAssembly()
            Using ss As IO.Stream = Asm.GetManifestResourceStream("BluePrism.ApplicationManager.AutoTrimTopAndBottom.png")
                If ss IsNot Nothing Then
                    SliceBoth = Image.FromStream(ss, True)
                End If
            End Using
        End If

        Dim mi As ToolStripItem = cm.Items.Add(My.Resources.SliceTopRowTrimOnePixel, SliceTop, AddressOf OnSliceTopClicked)
        mi.Enabled = Me.mCapturedImage IsNot Nothing AndAlso Me.mCapturedImage.Height > 0

        mi = cm.Items.Add(My.Resources.SliceBottomRowTrimOnePixel, SliceBottom, AddressOf OnSliceBottomClicked)
        mi.Enabled = Me.mCapturedImage IsNot Nothing AndAlso Me.mCapturedImage.Height > 0

        mi = cm.Items.Add(My.Resources.AutoTrimTopAndBottom, SliceBoth, AddressOf OnAutoTrimClicked)
        mi.Enabled = Me.mCapturedImage IsNot Nothing AndAlso Me.mCapturedImage.Height > 0

        Return cm
    End Function

    Private Sub OnSliceTopClicked(ByVal Sender As Object, ByVal e As EventArgs)
        Try
            If mCapturedImage IsNot Nothing Then
                If mCapturedImage.Height > 1 Then
                    Dim NewBitmap As New Bitmap(mCapturedImage.Width, mCapturedImage.Height - 1)
                    mCapturedImage = Me.mCapturedImage.Clone(New Rectangle(0, 1, mCapturedImage.Width, mCapturedImage.Height - 1), mCapturedImage.PixelFormat)
                Else
                    mCapturedImage = Nothing
                End If
            End If
        Catch ex As Exception
            MsgBox(String.Format(My.Resources.UnexpectedError0, ex.ToString))
        Finally
            Me.UpdateCapturedImageWindow()
        End Try
    End Sub

    Private Sub OnSliceBottomClicked(ByVal Sender As Object, ByVal e As EventArgs)
        Try
            If mCapturedImage IsNot Nothing Then
                If mCapturedImage.Height > 1 Then
                    Dim NewBitmap As New Bitmap(mCapturedImage.Width, mCapturedImage.Height - 1)
                    mCapturedImage = Me.mCapturedImage.Clone(New Rectangle(0, 0, mCapturedImage.Width, mCapturedImage.Height - 1), mCapturedImage.PixelFormat)
                Else
                    mCapturedImage = Nothing
                End If
            End If
        Catch ex As Exception
            MsgBox(String.Format(My.Resources.UnexpectedError0, ex.ToString))
        Finally
            Me.UpdateCapturedImageWindow()
        End Try
    End Sub

    Private Sub OnAutoTrimClicked(ByVal Sender As Object, ByVal e As EventArgs)
        Try
            AutoTrimImage(mCapturedImage, Me.mSelectedTextCol)
            Me.UpdateCapturedImageWindow()
        Catch ex As Exception
            MsgBox(String.Format(My.Resources.UnexpectedError0, ex.ToString))
        End Try
    End Sub

    Private Sub pbCaptured_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles pbCaptured.MouseUp
        If e.Button = Windows.Forms.MouseButtons.Right Then
            BuildCapturedImageContextMenu().Show(Me.pbCaptured, e.Location)
        End If
    End Sub

    Private Sub GenerateFromInstalledFontToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GenerateFromInstalledFontToolStripMenuItem.Click
        Dim f As frmSysFontPicker = Nothing
        Try
            f = New frmSysFontPicker
            If f.ShowDialog = Windows.Forms.DialogResult.OK Then
                Dim style As FontStyle = FontStyle.Regular
                If f.chkBold.Checked Then style = style Or FontStyle.Bold
                If f.chkItalic.Checked Then style = style Or FontStyle.Italic
                If f.chkUnderlined.Checked Then style = style Or FontStyle.Underline
                Dim WindowsFont As New Font(CStr(f.cmbFontFamily.Text), Single.Parse(CStr(f.cmbSize.Text)), style)
                Dim FP As FontPackage = CreateFont(WindowsFont, Nothing)
                Me.mFont = FP.mFont
                Me.UpdateFontDisplay()

                If MsgBox(My.Resources.DoYouWantToTryYourNewFontOnAnAutomaticallyGeneratedSample, MsgBoxStyle.YesNo, My.Resources.TestFont) = MsgBoxResult.Yes Then
                    'Update UI
                    Me.mCapturedImage = FP.TestImage
                    Me.mSelectedTextCol = Color.Black
                    Me.UpdateCapturedImageWindow()
                    UpdateDetectedCharacters()
                    UpdateGreys()
                End If
            End If
        Catch ex As Exception
            MsgBox(String.Format(My.Resources.UnexpectedError0, ex.ToString))
        Finally
            If f IsNot Nothing Then
                f.Dispose()
            End If
        End Try
    End Sub

    Private Sub AutogenerateAllAvailableFontsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AutogenerateAllAvailableFontsToolStripMenuItem.Click
        Dim f As frmFontIdentifier = Nothing
        Try
            f = New frmFontIdentifier
            f.ShowDialog()
        Catch ex As Exception
            MsgBox(String.Format(My.Resources.UnexpectedError0, ex.ToString))
        Finally
            If f IsNot Nothing Then
                f.Dispose()
            End If
        End Try
    End Sub


    ''' <summary>
    ''' Finds the index of the first non-blank row in the supplied image.
    ''' </summary>
    ''' <param name="BM">The image to be inspected.</param>
    ''' <param name="ActiveColour">The colour to search for.</param>
    ''' <returns>The index of the first row from the top in the supplied image,
    ''' containing the specified colour, or -1 if no such row exists.</returns>
    Private Shared Function FindFirstNonBlankIndex(ByVal BM As Bitmap, ByVal ActiveColour As Color) As Integer
        'Find the first non-blank row
        Dim y As Integer = 0
        Dim FirstNonWhiteRowIndex As Integer = -1
        Do
            For x As Integer = 0 To BM.Width - 1
                If BM.GetPixel(x, y).ToArgb = ActiveColour.ToArgb Then
                    FirstNonWhiteRowIndex = y
                    Exit Do
                End If
            Next
            y += 1
        Loop While y < BM.Height - 1

        Return FirstNonWhiteRowIndex
    End Function

    ''' <summary>
    ''' Finds the index of the last non-blank row in the supplied image.
    ''' </summary>
    ''' <param name="BM">The image to be inspected.</param>
    ''' <param name="ActiveColour">The colour to search for.</param>
    ''' <returns>The index of the last row from the bottom in the supplied image,
    ''' containing the specified colour, or -1 if no such row exists.</returns>
    Private Shared Function FindLastNonBlankRowIndex(ByVal BM As Bitmap, ByVal ActiveColour As Color) As Integer
        'Find the last non-blank row
        Dim y As Integer = BM.Height - 1
        Dim LastNonWhiteRowIndex As Integer = Integer.MaxValue
        Do
            For x As Integer = 0 To BM.Width - 1
                If BM.GetPixel(x, y).ToArgb = ActiveColour.ToArgb Then
                    LastNonWhiteRowIndex = y
                    Exit Do
                End If
            Next
            y -= 1
        Loop While y > 0

        Return LastNonWhiteRowIndex
    End Function

    ''' <summary>
    ''' Strips any completely white space from the top/bottom of the supplied image
    ''' </summary>
    ''' <param name="BM">The image to be processed.</param>
    Private Shared Sub AutoTrimImage(ByRef BM As Bitmap, ByVal ActiveColour As Color)
        Dim FirstNonWhiteRowIndex As Integer = FindFirstNonBlankIndex(BM, ActiveColour)
        Dim LastNonWhiteRowIndex As Integer = FindLastNonBlankRowIndex(BM, ActiveColour)

        'Exclude all rows above and below these bounds
        If FirstNonWhiteRowIndex > 0 OrElse LastNonWhiteRowIndex < BM.Height - 1 Then
            TrimImage(BM, Math.Min(0, FirstNonWhiteRowIndex), BM.Height - 1 - Math.Max(BM.Height - 1, LastNonWhiteRowIndex))
        End If
    End Sub

    ''' <summary>
    ''' Trims the specified number of rows above and below the supplied image.
    ''' </summary>
    ''' <param name="BM">The image to be trimmed.</param>
    ''' <param name="RowsAbove">The number of rows to be trimmed from the top of
    ''' the image.</param>
    ''' <param name="RowsBelow">The number of rows to be trimmed from the bottom of
    ''' the image.</param>
    Private Shared Sub TrimImage(ByRef BM As Bitmap, ByVal RowsAbove As Integer, ByVal RowsBelow As Integer)
        Dim NewHeight As Integer = BM.Height - RowsAbove - RowsBelow
        If NewHeight > 0 Then
            Dim NewBitmap As New Bitmap(BM.Width, NewHeight)
            BM = BM.Clone(New Rectangle(0, RowsAbove, BM.Width, NewHeight), BM.PixelFormat)
        Else
            BM = Nothing
        End If
    End Sub

    Private Shared Sub AddCharacterToFont(ByVal BPFont As FontData, ByVal WindowsFont As Font, ByVal Ch As Char, ByVal SF As StringFormat)
        Dim bmap As New Bitmap(60, 60)
        Dim Gr As Graphics = Graphics.FromImage(bmap)
        Gr.Clear(Color.White)
        Gr.DrawString(Ch, WindowsFont, Brushes.Black, 1, 1, SF)
        AutoTrimImageSides(bmap, Color.Black)

        BPFont.AddCharacter(New CharData(Ch, bmap), True)
    End Sub

    ''' <summary>
    ''' Creates a Blue Prism font, based on the supplied windows font.
    ''' </summary>
    ''' <param name="WindowsFont">The windows font of interest.</param>
    ''' <returns>Returns a blue prism font package, containing a font
    ''' corresponding to the supplied font, and a test image.</returns>
    Friend Shared Function CreateFont(ByVal WindowsFont As Font, ByVal UpdateProgressCallback As frmAutoFontsProgress.UpdateProgressBarTemplate) As FontPackage

        If UpdateProgressCallback IsNot Nothing Then UpdateProgressCallback.Invoke(0)

        'Generate the alphabet, one character per bitmap
        Dim SF As New StringFormat()
        Dim BPFont As New FontData()
        For i As Integer = 33 To 126 'a-z, A-Z, 0-9 and punctuation
            AddCharacterToFont(BPFont, WindowsFont, ChrW(i), SF)
            Threading.Thread.Sleep(5)
        Next
        AddCharacterToFont(BPFont, WindowsFont, "£"c, SF) 'the £ symbol

        'Trim the spare white space, above and below each character, whilst
        'respecting the tallest and lowest-hanging characters.
        For i As Integer = 0 To 93
        Next
        Dim FontHeight As Integer = 0
        Dim MinRowsAbove As Integer = Integer.MaxValue
        Dim MinRowsBelow As Integer = Integer.MaxValue
        For Each c As CharData In BPFont.Characters
            Dim RowsAbove As Integer = frmFontTrim.GetBlankRowCount(c, False)
            MinRowsAbove = Math.Min(RowsAbove, MinRowsAbove)

            Dim RowsBelow As Integer = frmFontTrim.GetBlankRowCount(c, True)
            MinRowsBelow = Math.Min(RowsBelow, MinRowsBelow)

            FontHeight = Math.Max(FontHeight, c.Height)
        Next
        For Each c As CharData In BPFont.Characters
            c.Strip(Direction.Top, MinRowsAbove)
            c.Strip(Direction.Bottom, MinRowsBelow)
        Next

        'Set space width
        BPFont.SpaceWidth = GetSpaceWidth(WindowsFont, SF)

        'For Each filename As String In Directory.GetFiles("C:\blueprism\qa\code\fontrec\fontsspaced")
        '   Dim TempFont1 As New clsFont()
        '   TempFont1.SetXML(File.ReadAllText(filename))
        '   Dim Spacewidth As Integer = TempFont1.SpaceWidth

        '   Dim FullPath As String = Path.Combine("C:\blueprism\qa\code\fontrec\fonts", New FileInfo(filename).Name)

        '   Dim FontToSave As New clsFont
        '   FontToSave.SetXML(File.ReadAllText(FullPath))
        '   FontToSave.SpaceWidth = Spacewidth
        '   FontToSave.Characters.Sort()
        '   File.WriteAllText(FullPath, FontToSave.GetXML)
        'Next

        Dim TempBitMap As New Bitmap(200, 200)
        Dim MeasuregG As Graphics = Graphics.FromImage(TempBitMap)

        'Get kerning information for each and every pair
        Dim KernValues As New Dictionary(Of String, Integer)
        Dim CompletedCount As Integer = 0
        Dim TotalPairs As Integer = BPFont.Characters.Count * BPFont.Characters.Count
        For Each fChar1 As CharData In BPFont.Characters
            For Each fChar2 As CharData In BPFont.Characters
                Dim str1 As String = fChar1.Value
                Dim str2 As String = fChar2.Value

                'Measure the width of these two characters together
                Dim TestVal As String = str1 & str2
                Dim s As SizeF = MeasuregG.MeasureString(TestVal, WindowsFont, New Size(Integer.MaxValue, Integer.MaxValue), SF)
                Dim CombinedBitMap As New Bitmap(CInt(Math.Ceiling(s.Width)) + 4, CInt(Math.Ceiling(s.Height)) + 2)
                Dim G As Graphics = Graphics.FromImage(CombinedBitMap)
                G.Clear(Color.White)
                G.DrawString(TestVal, WindowsFont, Brushes.Black, 1, 1, SF)
                AutoTrimImageSides(CombinedBitMap, Color.Black)
                Dim CombinedWidth As Integer = CombinedBitMap.Width
                G.Dispose()

                'Get the width of the left char alone
                TestVal = str1
                s = MeasuregG.MeasureString(TestVal, WindowsFont, New Size(Integer.MaxValue, Integer.MaxValue), SF)
                Dim LeftBitMap As New Bitmap(CInt(Math.Ceiling(s.Width)) + 4, CInt(Math.Ceiling(s.Height)) + 2)
                G = Graphics.FromImage(LeftBitMap)
                G.Clear(Color.White)
                G.DrawString(TestVal, WindowsFont, Brushes.Black, 1, 1, SF)
                AutoTrimImageSides(LeftBitMap, Color.Black)
                Dim LeftCharWidth As Integer = LeftBitMap.Width
                G.Dispose()

                'Get the width of the right char alone
                TestVal = str2
                s = MeasuregG.MeasureString(TestVal, WindowsFont, New Size(Integer.MaxValue, Integer.MaxValue), SF)
                Dim RightBitMap As New Bitmap(CInt(Math.Ceiling(s.Width)) + 4, CInt(Math.Ceiling(s.Height)) + 2)
                G = Graphics.FromImage(RightBitMap)
                G.Clear(Color.White)
                G.DrawString(TestVal, WindowsFont, Brushes.Black, 1, 1, SF)
                AutoTrimImageSides(RightBitMap, Color.Black)
                Dim RightCharWidth As Integer = RightBitMap.Width
                G.Dispose()


                'The kerning value is usually just the difference in width
                'between the combined, vs the sum of the parts. However,
                'where the width remains unchanged, the LH char must be
                'completely pushed forward into the RH character's space
                '(in which case there is no saying how far inwards the LH
                'character is pushed, and therefore how big the kerning value is.
                'Eg who is to say that in an italic font, the '_ pair may have
                'the apostrophe centred over the underscore?
                'Therefore the following more complicated routine is employed
                'for negatively kerned pairs.
                Dim KernValue As Integer = CombinedWidth - LeftCharWidth - RightCharWidth
                If -KernValue >= LeftCharWidth Then

                    'We build a single temporary font character as a composite of
                    'the two, at various kerning intervals. Once a full strict
                    'match is achieved then we have found the appropriate kerning
                    'distance
                    Debug.Assert(LeftBitMap.Height = RightBitMap.Height AndAlso LeftBitMap.Height = CombinedBitMap.Height)
                    Dim Canvas As CharCanvas = CharCanvas.FromPixRectByForegroundColour(New clsPixRect(CombinedBitMap), 0)
                    For PossibleKernValue As Integer = KernValue To -RightBitMap.Width Step -1
                        Dim CompositeData(CombinedBitMap.Width - 1, CombinedBitMap.Height - 1) As Boolean
                        'Copy the RH char, aligning as far right as poss
                        For x As Integer = 0 To RightBitMap.Width - 1
                            For y As Integer = 0 To RightBitMap.Height - 1
                                Dim RightBMIsBlack As Boolean = RightBitMap.GetPixel(x, y).ToArgb = Color.Black.ToArgb
                                CompositeData(CombinedBitMap.Width - RightBitMap.Width + x, y) = RightBMIsBlack
                            Next
                        Next
                        'Copy the LH char, aligning to the RHS, minus width of RH char,
                        'plus the proposed kerning distance.
                        For x As Integer = 0 To LeftBitMap.Width - 1
                            For y As Integer = 0 To LeftBitMap.Height - 1
                                Dim LeftBMIsBlack As Boolean = LeftBitMap.GetPixel(x, y).ToArgb = Color.Black.ToArgb
                                Dim Xcoord As Integer = CombinedBitMap.Width - LeftBitMap.Width - RightBitMap.Width - PossibleKernValue + x
                                CompositeData(Xcoord, y) = CompositeData(Xcoord, y) OrElse LeftBMIsBlack
                            Next
                        Next

                        Dim TempChar As New CharData("."c, New Mask(CompositeData))
                        If TempChar.IsAtPosition(Point.Empty, Canvas, Nothing, True, True) Then
                            KernValue = PossibleKernValue
                            Exit For
                        End If
                    Next
                End If
                KernValues.Add(str1 & str2, KernValue)
                Threading.Thread.Sleep(1)

                'Compute the "invasions" within the overlap area. The following
                'variables record the start/end indices relative to the combined
                'bitmap
                Dim LeftCharStartIndex_X As Integer = Math.Max(0, -LeftCharWidth - KernValue)
                Dim LeftCharEndIndex_X As Integer = LeftCharStartIndex_X + LeftCharWidth - 1
                Dim RightCharStartIndex_X As Integer = CombinedWidth - RightCharWidth
                Dim RightCharEndIndex_X As Integer = RightCharStartIndex_X + RightCharWidth - 1

                Dim Mask1 As PixelState(,) = fChar1.StateMask.CopiedValue
                Dim Mask2 As PixelState(,) = fChar2.StateMask.CopiedValue

                'Find out where the 'top' of the characters is, in this large sample
                'making the assumption that the top is at the same position as when
                'left hand char is rendered alone
                Dim Top As Integer = -1
                Dim GlyphHeight As Integer = fChar1.Height
                Dim TempCanvas As CharCanvas = CharCanvas.FromPixRectByForegroundColour(New clsPixRect(LeftBitMap), 0)
                For y As Integer = 0 To LeftBitMap.Height - 1
                    Dim WasNonStrict As Boolean
                    If fChar1.IsAtPosition(New Point(0, y), TempCanvas, WasNonStrict, True, True) Then
                        Top = y
                        Exit For
                    End If
                Next
                If Top < 0 Then Throw New InvalidOperationException(String.Format(My.Resources.FailedToFindTopOfLeftCharacterIn01, str1, str2))

                For x As Integer = 0 To CombinedWidth - 1
                    For y As Integer = 0 To GlyphHeight - 1
                        Dim InsideLeftChar As Boolean = x >= LeftCharStartIndex_X AndAlso x <= LeftCharEndIndex_X
                        Dim InsideRightChar As Boolean = x >= RightCharStartIndex_X AndAlso x <= RightCharEndIndex_X
                        If InsideLeftChar AndAlso InsideRightChar Then
                            Dim LeftCharRelativeX As Integer = x - LeftCharStartIndex_X
                            Dim RightCharRelativeX As Integer = x - RightCharStartIndex_X
                            Dim LeftCharBlack As Boolean = fChar1.Mask(LeftCharRelativeX, y) ' fChar1.mabData(LeftCharRelativeX, y)
                            Dim RightCharBlack As Boolean = fChar2.Mask(RightCharRelativeX, y)
                            If RightCharBlack Then Mask1(LeftCharRelativeX, y) = PixelState.NoCheck
                            If LeftCharBlack Then Mask2(RightCharRelativeX, y) = PixelState.NoCheck
                        End If
                    Next
                Next

                CompletedCount += 1
                If UpdateProgressCallback IsNot Nothing Then UpdateProgressCallback.Invoke(CompletedCount / TotalPairs)

                fChar1.StateMask = New PixelStateMask(Mask1)
                fChar2.StateMask = New PixelStateMask(Mask2)
            Next
        Next
        BPFont.SetKernValues(KernValues)

        Dim Name As String = WindowsFont.FontFamily.Name & " " & WindowsFont.Size.ToString
        If WindowsFont.Bold Then Name &= " Bold"
        If WindowsFont.Italic Then Name &= " Italic"
        If WindowsFont.Underline Then Name &= " Underlined"
        Return New FontPackage(WindowsFont.FontFamily.Name, Name, BPFont, Nothing, "")
    End Function


    Private Shared Function GetSpaceWidth(ByVal WindowsFont As Font, ByVal SF As StringFormat) As Integer
        'Determine the typical kerning width
        Dim KernText As String = "ll"
        Dim rubbish As New Bitmap(1, 1)
        Dim G As Graphics = Graphics.FromImage(rubbish)
        Dim s As SizeF = G.MeasureString(KernText, WindowsFont, New Size(Integer.MaxValue, Integer.MaxValue), SF)
        Dim TestBitMap As New Bitmap(CInt(Math.Ceiling(s.Width)) + 4, CInt(Math.Ceiling(s.Height)) + 2)
        G = Graphics.FromImage(TestBitMap)
        G.Clear(Color.White)
        G.DrawString(KernText, WindowsFont, Brushes.Black, 1, 1, SF)
        Dim HalfHeight As Integer = TestBitMap.Height \ 2
        Dim First As Integer = -1
        Dim Second As Integer = -1
        Dim HadSpace As Boolean = False
        For i As Integer = 0 To TestBitMap.Width - 1
            If TestBitMap.GetPixel(i, HalfHeight).ToArgb = Color.Black.ToArgb Then
                If Not HadSpace Then
                    First = i
                Else
                    If HadSpace Then
                        Second = i
                        Exit For
                    End If
                End If
            Else
                If First <> -1 Then HadSpace = True
            End If
        Next
        Dim TwoLKern As Integer = Second - First - 1
        AutoTrimImageSides(TestBitMap, Color.Black)
        Dim TwoLKernImageWidth As Integer = TestBitMap.Width

        'Determine the typical space width
        Dim SpaceText As String = "l l"
        rubbish = New Bitmap(1, 1)
        G = Graphics.FromImage(rubbish)
        s = G.MeasureString(SpaceText, WindowsFont, New Size(Integer.MaxValue, Integer.MaxValue), SF)
        TestBitMap = New Bitmap(CInt(Math.Ceiling(s.Width)) + 4, CInt(Math.Ceiling(s.Height)) + 2)
        G = Graphics.FromImage(TestBitMap)
        G.Clear(Color.White)
        G.DrawString(SpaceText, WindowsFont, Brushes.Black, 1, 1, SF)
        HalfHeight = TestBitMap.Height \ 2
        First = -1
        Second = -1
        AutoTrimImageSides(TestBitMap, Color.Black)

        Return TestBitMap.Width - TwoLKernImageWidth
    End Function



    ''' <summary>
    ''' Represents a font and its associated goodies.
    ''' </summary>
    Friend Class FontPackage
        Friend Sub New(ByVal FamilyName As String, ByVal Name As String, ByVal F As FontData, ByVal Test As Bitmap, ByVal TestText As String)
            Me.mFont = F
            Me.TestImage = Test
            Me.TestImageText = TestText
            mFamilyName = FamilyName
            mName = Name
        End Sub
        Public mFont As FontData
        Public TestImage As Bitmap
        Public TestImageText As String
        Public mName As String
        Public mFamilyName As String

        Public Sub Save(ByVal ParentDirectory As String)
            If Not Directory.Exists(ParentDirectory) Then
                Directory.CreateDirectory(ParentDirectory)
            End If
            File.WriteAllText(Path.Combine(ParentDirectory, "font_" & Me.mName & ".xml"), mFont.GetXML)
        End Sub
    End Class


    Private Sub HandleDetectedCharacterImageClick(ByVal Sender As Object, ByVal e As MouseEventArgs)
        Try
            If e.Button = Windows.Forms.MouseButtons.Right Then
                mClickCharacterPB = CType(Sender, PictureBox)
                Dim cm As ContextMenuStrip = Me.BuildCDetectedCharacterImageContextMenu
                mClickCharacterPB.ContextMenuStrip = cm
                cm.Show(mClickCharacterPB, Point.Empty)
            End If
        Catch ex As Exception
            MsgBox(String.Format(My.Resources.UnexpectedError0, ex.ToString))
        End Try
    End Sub

    ''' <summary>
    ''' The picturebox in the "detected" characters area that was last clicked.
    ''' </summary>
    Private mClickCharacterPB As PictureBox

    Private Function BuildCDetectedCharacterImageContextMenu() As ContextMenuStrip
        Dim cm As New ContextMenuStrip

        Dim mi As ToolStripItem = cm.Items.Add(My.Resources.AttemptNonVerticalSplit, Nothing, AddressOf OnAttemptNonVerticalSplitClick)
        mi = cm.Items.Add(My.Resources.Show, Nothing, AddressOf OnShowCharacterClick)

        Return cm
    End Function


    Private Sub OnAttemptNonVerticalSplitClick(ByVal Sender As Object, ByVal e As EventArgs)
        Try
            Dim Images As List(Of Bitmap) = Me.SplitImageAdvanced(CType(mClickCharacterPB.Image, Bitmap), Me.mSelectedTextCol)
            If Images.Count > 1 Then
                Dim ExistingCharacters As New List(Of Bitmap)
                For Each c As Control In Me.pnlDetected.Controls
                    If TypeOf c Is PictureBox Then
                        If c IsNot mClickCharacterPB Then
                            ExistingCharacters.Add(CType(CType(c, PictureBox).Image, Bitmap))
                        Else
                            ExistingCharacters.AddRange(Images)
                        End If
                    End If
                Next

                Me.LayoutCharacters(ExistingCharacters)
            End If
        Catch ex As Exception
            MsgBox(String.Format(My.Resources.UnexpectedError0, ex.ToString))
        End Try
    End Sub

    Private Sub OnShowCharacterClick(ByVal Sender As Object, ByVal e As EventArgs)
        If mClickCharacterPB IsNot Nothing Then
            Dim f As New frmFontCharPopup
            Try
                f.SetCharacterImage(mClickCharacterPB.Image, Nothing)
                f.ShowDialog()
            Finally
                f.Dispose()
            End Try
        End If
    End Sub


    Private Sub AutomaticFontIdentifierToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AutomaticFontIdentifierToolStripMenuItem.Click
        Try
            Dim f As New frmFontIdentifier()
            f.ShowDialog()
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
    End Sub
End Class

