Imports System.IO
Imports BluePrism.CharMatching
Imports System.Text.RegularExpressions
Imports System.Collections.Generic

Public Class frmFontIdentifier

    Private ReadOnly mRegex_LoadFile As _
        New Regex("^font_(?<FontName>([a-z]|[A-Z]|[0-9]|\s)+) (?<FontSize>[\d\.]{1,4})\s?(?<Attributes>(Bold)?\s?(Italic)?\s?(Underlined)?).xml$",
                  RegexOptions.Compiled,
                  BPCoreLib.RegexTimeout.DefaultRegexTimeout)

    Private Sub btnBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowseFontDirectory.Click
        Try
            Dim dp As New FolderBrowserDialog()
            dp.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer)
            If dp.ShowDialog = Windows.Forms.DialogResult.OK Then
                Me.txtTargetDir.Text = dp.SelectedPath
            End If
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
    End Sub
    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Try
            Me.btnCancel.Enabled = False
            Me.mbSearchCancelled = True

            'We can't join as follows, because the other thread makes
            'cross-threaded calls to update the UI on this thread, so
            'there is a potential for a deadlock under this approach
            'Me.mtWorkerThread.Join()

            'Instead we just sleep
            If Me.WorkerThread IsNot Nothing Then
                While Me.WorkerThread.IsAlive
                    System.Threading.Thread.Sleep(100)
                    Application.DoEvents()
                End While
            End If

            Me.UpdateUIEndSearch()
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
    End Sub

    Private Sub btnSelectNone_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSelectNone.Click
        Try
            Me.lstFonts.BeginUpdate()
            For Each lvi As ListViewItem In Me.lstFonts.Items
                lvi.Checked = False
            Next
        Catch ex As Exception
            MsgBox(ex.ToString)
        Finally
            Me.lstFonts.EndUpdate()
        End Try
    End Sub
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSelectAll.Click
        Try
            lstFonts.BeginUpdate()
            For Each lvi As ListViewItem In lstFonts.Items
                lvi.Checked = True
            Next
        Catch ex As Exception
            MsgBox(ex.ToString)
        Finally
            lstFonts.EndUpdate()
        End Try
    End Sub
    Private Sub btnToggleBold_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnToggleBold.Click
        Try
            Static LastBoldValue As Boolean = False

            Me.lstFonts.BeginUpdate()
            For Each lvi As ListViewItem In Me.lstFonts.Items
                If Boolean.Parse(lvi.SubItems(chBold.Name).Text) Then
                    lvi.Checked = Not LastBoldValue
                End If
            Next

            LastBoldValue = Not LastBoldValue
        Catch ex As Exception
            MsgBox(ex.ToString)
        Finally
            Me.lstFonts.EndUpdate()
        End Try
    End Sub
    Private Sub chkToggleAllItalic_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnToggleAllItalic.Click
        Try
            Static LastItalicValue As Boolean = False

            Me.lstFonts.BeginUpdate()
            For Each lvi As ListViewItem In Me.lstFonts.Items
                If Boolean.Parse(lvi.SubItems(chItalic.Name).Text) Then
                    lvi.Checked = Not LastItalicValue
                End If
            Next

            LastItalicValue = Not LastItalicValue
        Catch ex As Exception
            MsgBox(ex.ToString)
        Finally
            lstFonts.EndUpdate()
        End Try
    End Sub
    Private Sub BrowseSampleImage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowseSampleImage.Click
        Try
            Dim OFD As New OpenFileDialog()
            OFD.AddExtension = True
            OFD.CheckFileExists = True
            OFD.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            OFD.Multiselect = False
            If OFD.ShowDialog = Windows.Forms.DialogResult.OK Then
                Dim ImageFilePath As String = OFD.FileName
                Dim SampleImage As Image = Nothing
                Try
                    SampleImage = Image.FromFile(ImageFilePath)
                Catch ex As Exception
                    MsgBox(String.Format(My.Resources.ErrorLoadingImage0, ex.Message))
                    Exit Sub
                End Try
                Me.pbSampleImage.Image = SampleImage
            End If
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
    End Sub
    Private Sub btnToggleAllUnderlined_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnToggleAllUnderlined.Click
        Try
            Static LastUnderlinedValue As Boolean = False

            Me.lstFonts.BeginUpdate()
            For Each lvi As ListViewItem In Me.lstFonts.Items
                If Boolean.Parse(lvi.SubItems(chUnderlined.Name).Text) Then
                    lvi.Checked = Not LastUnderlinedValue
                End If
            Next

            LastUnderlinedValue = Not LastUnderlinedValue
        Catch ex As Exception
            MsgBox(ex.ToString)
        Finally
            lstFonts.EndUpdate()
        End Try
    End Sub
    Private Sub btnToggleAllSize_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnToggleAllSize.Click
        Try
            Dim RequestedSize As Integer
            If Not Integer.TryParse(Me.txtToggleSize.Text, RequestedSize) Then
                MsgBox(String.Format(My.Resources.InvalidSize0, Me.txtToggleSize.Text))
                Exit Sub
            End If

            'Reset the cached last boolean value each time the size changes
            Static LastSizeValue As Boolean = False
            Static LastSize As Integer = 0
            If LastSize <> RequestedSize Then
                LastSizeValue = False
                LastSize = RequestedSize
            End If

            Me.lstFonts.BeginUpdate()
            For Each lvi As ListViewItem In Me.lstFonts.Items
                If Integer.Parse(lvi.SubItems(chSize.Name).Text) = RequestedSize Then
                    lvi.Checked = Not LastSizeValue
                End If
            Next

            LastSizeValue = Not LastSizeValue
        Catch ex As Exception
            MsgBox(ex.ToString)
        Finally
            lstFonts.EndUpdate()
        End Try
    End Sub

    Private Sub btnLoad_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnLoad.Click
        Try
            If Not Directory.Exists(Me.txtTargetDir.Text) Then Throw New DirectoryNotFoundException()

            Me.lstFonts.BeginUpdate()
            Me.lstFonts.Items.Clear()

            For Each xmlFile As FileInfo In New DirectoryInfo(txtTargetDir.Text).GetFiles("*.xml")
                Dim match As Match = mRegex_LoadFile.Match(xmlFile.Name)
                If match?.Success Then
                    Dim FontName As String = xmlFile.Name.Replace(".xml", "")
                    Dim FontSize As Single = match.Groups("FontSize").Value
                    Dim Attributes As String = match.Groups("Attributes").Value
                    Dim Bold As Boolean = Attributes.Contains("Bold")
                    Dim Italic As Boolean = Attributes.Contains("Italic")
                    Dim Underlined As Boolean = Attributes.Contains("Underlined")

                    Me.lstFonts.Items.Add(CreateListviewItem(xmlFile.FullName, FontName, FontSize, Bold, Italic, Underlined))
                End If
            Next

            SetListviewEnabled(True)
            SetListviewPeripheryEnabled(True)
        Catch ex As RegexMatchTimeoutException
            MsgBox(String.Format(My.Resources.FontIdentifierRegexTimeout, ex.Message))
        Catch ex As DirectoryNotFoundException
            MsgBox(String.Format(My.Resources.TheSpecifiedDirectoryDoesNotSeemToExist0, Me.txtTargetDir.Text))
        Catch ex As Exception
            MsgBox(ex.ToString)
        Finally
            Me.lstFonts.EndUpdate()
        End Try
    End Sub

    Private Sub SetListviewEnabled(ByVal Value As Boolean)
        Me.lstFonts.Enabled = Value
    End Sub

    Private Sub SetListviewPeripheryEnabled(ByVal Value As Boolean)
        Me.btnSelectAll.Enabled = Value
        Me.btnSelectNone.Enabled = Value
        Me.btnToggleAllSize.Enabled = Value
        Me.txtToggleSize.Enabled = Value
        Me.btnToggleBold.Enabled = Value
        Me.btnToggleAllUnderlined.Enabled = Value
        Me.btnToggleAllItalic.Enabled = Value
        Me.txtTargetDir.Enabled = Value
        Me.btnBrowseFontDirectory.Enabled = Value
        Me.btnLoad.Enabled = Value
    End Sub

    Private Function CreateListviewItem(ByVal FilePath As String, ByVal FontName As String, ByVal FontSize As Single, ByVal isBold As Boolean, ByVal IsItalic As Boolean, ByVal IsUnderlined As Boolean) As ListViewItem
        Dim lvi As New ListViewItem(FontName)
        lvi.Name = Guid.NewGuid.ToString
        Dim subitem As ListViewItem.ListViewSubItem = Nothing

        subitem = lvi.SubItems.Add("9999")
        subitem.Name = chScore.Name
        subitem = lvi.SubItems.Add(FontSize.ToString)
        subitem.Name = chSize.Name
        subitem = lvi.SubItems.Add(isBold.ToString)
        subitem.Name = chBold.Name
        subitem = lvi.SubItems.Add(IsItalic.ToString)
        subitem.Name = chItalic.Name
        subitem = lvi.SubItems.Add(IsUnderlined.ToString)
        subitem.Name = chUnderlined.Name

        lvi.Tag = FilePath
        Return lvi
    End Function

    Private Sub btnBegin_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBegin.Click
        Try
            Me.progOverall.Maximum = Me.lstFonts.CheckedItems.Count
            Me.UpdateUIBeginSearch()

            Dim D As New Dictionary(Of String, String)
            For Each item As ListViewItem In Me.lstFonts.CheckedItems
                D.Add(item.Name, CStr(item.Tag))
            Next
            If D.Count > 0 Then
                WorkerThread = New System.Threading.Thread(AddressOf DoSearch)
                WorkerThread.Start(D)
            Else
                MsgBox(My.Resources.PleaseFirstSelectSomeFontsToBeTestedUsingTheCheckboxesInTheListAbove)
                Me.UpdateUIEndSearch()
            End If
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
    End Sub

    Dim WorkerThread As System.Threading.Thread

    Private Sub DoSearch(ByVal oFontPaths As Object)
        Dim FontPaths As Dictionary(Of String, String) = CType(oFontPaths, Dictionary(Of String, String))
        Dim StartTime As DateTime = DateTime.Now

        Try
            Me.mbSearchUnderway = True
            Me.mbSearchCancelled = False

            Dim PR As New BPCoreLib.clsPixRect(Me.pbSampleImage.Image)
            Dim Colour As Integer = 0
            If Not Integer.TryParse(Me.txtColour.Text, Globalization.NumberStyles.HexNumber, Nothing, Colour) Then
                MsgBox(String.Format(My.Resources.InvalidColour0, Me.txtColour.Text))
                Exit Sub
            End If

            Dim Count As Integer = 0
            'Key is GUID of listviewitem; value is file path
            For Each kvp As KeyValuePair(Of String, String) In FontPaths
                If Me.mbSearchCancelled Then Exit Sub

                Dim UpdateLabel As New AnnotateProgress(AddressOf UpdateProgressLabel)
                Me.Invoke(UpdateLabel, New Object() {StartTime, Count / Me.progOverall.Maximum, kvp.Value})

                Dim XMLPath As String = CStr(kvp.Value)
                If Not File.Exists(XMLPath) Then
                    MsgBox(String.Format(My.Resources.InvalidFilePath0, XMLPath))
                    Exit Sub
                End If
                Dim font As FontData
                Try
                    font = New FontData(System.IO.File.ReadAllText(XMLPath))
                Catch xmlEx As Exception
                    MsgBox(String.Format(My.Resources.ErrorLoadingFontFile0, xmlEx.Message))
                    Return
                End Try

                Dim sOutText As String
                Try
                    sOutText = FontReader.ReadTextSingleLine(
                     rdoMatchBackground.Checked, PR, font, Colour)
                Catch ex As Exception
                    MsgBox(String.Format(My.Resources.Error0, ex.Message))
                    Return
                End Try

                Dim ReferenceText As String = Me.txtReferenceText.Text.Replace(" ", "")
                For Each ConflictList As ICollection(Of CharData) In font.GetConflictingCharacterGroups()
                    For Each ch As CharData In ConflictList
                        sOutText = sOutText.Replace(ch.Value, "")
                        ReferenceText = ReferenceText.Replace(ch.Value, "")
                    Next
                Next

                Dim LevScore As Integer
                If ReferenceText = "" AndAlso sOutText = "" Then
                    LevScore = -1
                Else
                    LevScore = Me.ComputeLevenshteinDistance(ReferenceText, sOutText.Replace(" ", ""))
                End If
                Dim LevD As New UpdateLVItemScore(AddressOf UpdateListviewItemScore)
                Me.Invoke(LevD, New Object() {kvp.Key, LevScore})

                Count += 1
                Dim d As New UpdateProgress(AddressOf UpdateProgressBar)
                Me.Invoke(d, New Object() {(Count / Me.progOverall.Maximum)})
            Next
        Catch ex As Exception
            MsgBox(String.Format("Failure - {0}", ex.ToString))
        Finally
            Me.mbSearchUnderway = False
            Dim UIDel As New NoParms(AddressOf UpdateUIEndSearch)
            Me.Invoke(UIDel)
        End Try
    End Sub

    Private mbSearchUnderway As Boolean
    Private mbSearchCancelled As Boolean

    Private Delegate Sub UpdateLVItemScore(ByVal ItemName As String, ByVal Score As Integer)

    Private Sub UpdateListviewItemScore(ByVal ItemName As String, ByVal Score As Integer)
        Me.lstFonts.Items(ItemName).SubItems(chScore.Name).Text = Score.ToString
    End Sub

    Private Delegate Sub AnnotateProgress(ByVal D As DateTime, ByVal Progress As Double, ByVal FontName As String)

    Private Sub UpdateProgressLabel(ByVal StartTime As DateTime, ByVal PercentComplete As Double, ByVal FontName As String)
        Dim LastIndex As Integer = FontName.LastIndexOf("font_")
        Dim ExtIndex As Integer = FontName.LastIndexOf(".xml")
        If LastIndex <> -1 AndAlso ExtIndex <> -1 AndAlso ExtIndex > LastIndex Then
            FontName = FontName.Substring(LastIndex, ExtIndex - LastIndex)
        Else
            FontName = ""
        End If
        Me.lblCurrentFont.Text = FontName
        Me.lblCurrentFont.Left = Me.progOverall.Right - Me.lblCurrentFont.Width


        Dim Duration As TimeSpan = DateTime.Now.Subtract(StartTime)
        Me.lblElapsedValue.Text = String.Format(My.Resources.x0Hours1Minutes2Seconds, Duration.Hours.ToString, Duration.Minutes.ToString, Duration.Seconds.ToString)

        If PercentComplete = 0 Then
            Me.lblRemainingValue.Text = My.Resources.Unknown
        Else
            Dim Remaining As TimeSpan = New TimeSpan(0, 0, 0, 0, (Duration.TotalMilliseconds / PercentComplete)).Subtract(Duration)
            Me.lblRemainingValue.Text = String.Format(My.Resources.x0Hours1Minutes2Seconds, Remaining.Hours.ToString, Remaining.Minutes.ToString, Remaining.Seconds.ToString)
        End If
    End Sub

    Private Function ComputeLevenshteinDistance(ByVal S1 As String, ByVal S2 As String) As Integer
        Dim Distances(S1.Length, S2.Length) As Integer

        For i As Integer = 0 To S1.Length
            Distances(i, 0) = i 'deletion
        Next
        For i As Integer = 0 To S2.Length
            Distances(0, i) = i 'insertion
        Next

        For j As Integer = 1 To S2.Length
            For i As Integer = 1 To S1.Length
                If S1.Substring(i - 1, 1) = S2.Substring(j - 1, 1) Then
                    Distances(i, j) = Distances(i - 1, j - 1)
                Else
                    Distances(i, j) = Math.Min(Distances(i - 1, j) + 1, Math.Min(Distances(i, j - 1) + 1, Distances(i - 1, j - 1) + 1))
                End If
            Next
        Next

        Return Distances(S1.Length, S2.Length)
    End Function


    Private Delegate Sub UpdateProgress(ByVal Value As Double)


    Private Sub UpdateProgressBar(ByVal Value As Double)
        Value = Math.Max(0, Math.Min(Value, 1))
        Me.progOverall.Value = CInt(Math.Floor(Value * Me.progOverall.Maximum))
    End Sub

    Private Delegate Sub NoParms()


    Private Sub UpdateUIBeginSearch()
        Me.btnBegin.Enabled = False
        Me.SetListviewPeripheryEnabled(False)
        Me.btnBrowseSampleImage.Enabled = False
        Me.txtReferenceText.ReadOnly = True
        Me.rdoMatchForeground.Enabled = False
        Me.rdoMatchBackground.Enabled = False
        Me.btnCancel.Enabled = True
    End Sub

    Private Sub UpdateUIEndSearch()
        Me.SetListviewPeripheryEnabled(True)
        Me.btnBegin.Enabled = True
        Me.btnBrowseSampleImage.Enabled = True
        Me.txtReferenceText.ReadOnly = False
        Me.rdoMatchForeground.Enabled = True
        Me.rdoMatchBackground.Enabled = True
        Me.btnCancel.Enabled = False
    End Sub

    Private Sub pbSampleImage_Click(ByVal sender As System.Object, ByVal e As MouseEventArgs) Handles pbSampleImage.MouseClick
        Dim C As Color = CType(pbSampleImage.Image, Bitmap).GetPixel(e.X, e.Y)
        Me.txtColour.Text = (CInt(C.R) + (CInt(C.G) << 8) + (CInt(C.B) << 16)).ToString("X6")
        Me.pbColour.BackColor = C
    End Sub

    Private Sub lstFonts_ItemCheck(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemCheckEventArgs) Handles lstFonts.ItemCheck
        If Me.mbSearchUnderway Then e.NewValue = e.CurrentValue
    End Sub

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mListviewSorter = New clsListViewSorter(Me.lstFonts)
        mListviewSorter.Order = SortOrder.Ascending
        mListviewSorter.ColumnDataTypes = New System.Type() {GetType(String), GetType(Integer), GetType(Integer), GetType(Boolean), GetType(Boolean), GetType(Boolean)}
        Me.lstFonts.ListViewItemSorter = mListviewSorter
    End Sub

    ''' <summary>
    ''' The sorter used to sort the listview
    ''' </summary>
    Private mListviewSorter As clsListViewSorter

End Class
