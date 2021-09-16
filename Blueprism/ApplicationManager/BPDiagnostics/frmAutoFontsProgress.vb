Imports System.runtime.InteropServices
Imports BluePrism.BPCoreLib.modWin32

Public Class frmAutoFontsProgress

    Private Sub BeginGeneration(ByVal FontCount As Integer)
        Me.btnBegin.Enabled = False
        Me.btnBrowse.Enabled = False
        Me.txtTargetDir.Enabled = False
        Me.txtMinSize.Enabled = False
        Me.txtMaxSize.Enabled = False
        Me.txtStepSize.Enabled = False

        Me.btnOK.Enabled = False
        Me.btnCancel.Enabled = True
        Me.progOverall.Enabled = True
        Me.progCurrentFamily.Enabled = True
        Me.lblScope.Enabled = True
        Me.lblCurrentFamily.Enabled = True
        Me.lblCurrentFontFamily.Enabled = True
        Me.lblCurrentPreciseFont.Enabled = True
        Me.lblOverall.Enabled = True
        Me.lblTimeInfo.Enabled = True
        Me.lblElapsedValue.Enabled = True
        Me.lblRemainingValue.Enabled = True
        Me.Label1.Enabled = True

        Me.chkNonSymbolFontsOnly.Enabled = False
        Me.chkSkipExisting.Enabled = False
        Me.chkRegenerateExistingOnly.Enabled = False

        Me.progOverall.Minimum = 0
        Me.progOverall.Maximum = FontCount
        Me.progOverall.Value = 0
    End Sub

    Private Sub EndGeneration()
        For Each c As Control In Me.Controls
            c.Enabled = True
        Next
        Me.btnCancel.Enabled = False
    End Sub

    Private mStyleIndex As Integer = 0

    Private Sub GenerateFonts()

        Dim d1 As IntegerParm = AddressOf BeginGeneration
        Me.Invoke(d1, New Object() {0})
        Dim StartTime As DateTime = DateTime.Now

        'Count the number of fonts to be processed
        Dim Col As New System.Drawing.Text.InstalledFontCollection
        Dim FontCount As Integer = Col.Families.Length
        If Me.chkNonSymbolFontsOnly.Checked Then
            For Each Fam As FontFamily In Col.Families
                Dim F As Font = Me.GetFontInstance(Fam.Name)
                If F Is Nothing OrElse Me.IsFontSymbolFont(F) Then FontCount -= 1
            Next
        End If
        Me.Invoke(d1, New Object() {FontCount})

        Dim Silent As Boolean = False
        Dim SkipCount As Integer = 0
        Dim MinSize As Integer = Integer.Parse(Me.txtMinSize.Text)
        Dim Maxsize As Integer = Integer.Parse(Me.txtMaxSize.Text)
        Dim StepSize As Single = Single.Parse(Me.txtStepSize.Text)

        For Each Fam As FontFamily In Col.Families
            Dim d4 As New IntegerParm(AddressOf ResetFamilyProgress)
            Me.Invoke(d4, New Object() {CInt(1 + Math.Ceiling((Maxsize - MinSize) / StepSize)) * 8})

            Dim WindowsFont As Font = Me.GetFontInstance(Fam.Name)
            If Not (Me.chkNonSymbolFontsOnly.Checked AndAlso Me.IsFontSymbolFont(WindowsFont)) Then

                For ptSize As Single = MinSize To Maxsize Step StepSize
                    For mStyleIndex = 0 To 7
                        Dim Style As FontStyle = CType(mStyleIndex, FontStyle)
                        If Fam.IsStyleAvailable(Style) Then
                            If Not Me.mbCancelled Then
                                Dim FP As frmFontEditor.FontPackage = Nothing
                                Try
                                    Dim ThisFont As New Font(Fam.Name, ptSize, Style)
                                    Dim FontName As String = ThisFont.FontFamily.Name & " " & ThisFont.Size.ToString
                                    If ThisFont.Bold Then FontName &= " Bold"
                                    If ThisFont.Italic Then FontName &= " Italic"
                                    If ThisFont.Underline Then FontName &= " Underlined"

                                    Dim ExistsAlready As Boolean = IO.File.Exists(IO.Path.Combine(Me.txtTargetDir.Text, "font_" & FontName & ".xml"))
                                    Dim SkipThisOne As Boolean = False
                                    If (Me.chkSkipExisting.Checked AndAlso ExistsAlready) Then SkipThisOne = True
                                    If Me.chkRegenerateExistingOnly.Checked AndAlso Not ExistsAlready Then SkipThisOne = True

                                    If Not SkipThisOne Then
                                        Dim d3 As New UpdateProgress(AddressOf UpdateCurrentProgress)
                                        Me.Invoke(d3, New Object() {ThisFont.FontFamily.Name, FontName, Now() - StartTime, SkipCount})

                                        Dim d7 As New UpdateProgressBarTemplate(AddressOf UpdateCurrentFontProgressBarCallback)
                                        FP = frmFontEditor.CreateFont(ThisFont, d7)
                                        FP.Save(Me.txtTargetDir.Text)
                                        Application.DoEvents()
                                    End If

                                    Dim d5 As New NoParms(AddressOf IncrementCurrentFontProgress)
                                    Me.Invoke(d5)
                                Catch Ex As Exception
                                    If Not (Silent OrElse mbCancelled) Then
                                        Dim Name As String = String.Format(My.Resources.x012, Fam.Name, ptSize.ToString, mStyleIndex.ToString)
                                        If FP IsNot Nothing Then Name = FP.mName
                                        Select Case MsgBox(String.Format(My.Resources.FailedCreatingFont0DoYouWantToContinueWithTheNextFont11PressYesToContinueNoToQu, Name, vbCrLf), MsgBoxStyle.YesNoCancel)
                                            Case MsgBoxResult.Yes
                                                Continue For
                                            Case MsgBoxResult.No
                                                GoTo Quit
                                            Case MsgBoxResult.Cancel
                                                Silent = True
                                                Continue For
                                        End Select
                                    End If
                                End Try
                            Else
                                Exit Sub
                            End If
                        End If
                    Next
                Next

            Else
                SkipCount += 1
            End If

            Dim d6 As New NoParms(AddressOf IncrementOverallProgress)
            Me.Invoke(d6)
        Next

        Dim d2 As New NoParms(AddressOf EndGeneration)
        Me.Invoke(d2)
        Exit Sub

Quit:
        Me.Close()
    End Sub

    Delegate Sub NoParms()
    Delegate Sub IntegerParm(ByVal IntVal As Integer)
    Delegate Sub UpdateProgress(ByVal FamilyName As String, ByVal FontName As String, ByVal Duration As TimeSpan, ByVal SkipCount As Integer)

    Public Delegate Sub UpdateProgressBarTemplate(ByVal Val As Double)

    Private Sub UpdateCurrentFontProgressBarCallback(ByVal Val As Double)
        If Not Me.mbCancelled Then
            Dim d As New UpdateProgressBarTemplate(AddressOf SetCurrentFontProgress)
            Me.Invoke(d, New Object() {Val})
        Else
            'Cause an exception in the current font and therefore
            'we get a quicker exit
            Throw New InvalidOperationException(My.Resources.DeliberateCancellation)
        End If
    End Sub

    Private Sub IncrementCurrentFontProgress()
        Me.progCurrentFamily.Value += 1
    End Sub

    Private Sub IncrementOverallProgress()
        Me.progOverall.Value += 1
    End Sub

    Private Sub SetCurrentFontProgress(ByVal Val As Double)
        Me.progCurrentFont.Maximum = 100
        Me.progCurrentFont.Value = CInt(Math.Floor(Val * progCurrentFont.Maximum))

        Me.progCurrentFamily.Value = CInt(Math.Floor(((mStyleIndex + Val) / 8) * progCurrentFamily.Maximum))
    End Sub

    Private Sub UpdateCurrentProgress(ByVal FamilyName As String, ByVal FontName As String, ByVal Duration As TimeSpan, ByVal SkipCount As Integer)
        Me.lblCurrentFontFamily.Text = String.Format(My.Resources.x0, FamilyName)
        Me.lblCurrentFontFamily.Left = Me.progCurrentFamily.Right - Me.lblCurrentFontFamily.Width
        Me.lblCurrentPreciseFont.Text = String.Format(My.Resources.x0, FontName)
        Me.lblCurrentPreciseFont.Left = Me.progCurrentFamily.Right - Me.lblCurrentPreciseFont.Width
        Me.lblElapsedValue.Text = String.Format(My.Resources.x0Hours1Minutes2Seconds, Duration.Hours.ToString, Duration.Minutes.ToString, Duration.Seconds.ToString)
        Dim Remaining As TimeSpan = (New TimeSpan(0, 0, 0, 0, (Duration.TotalMilliseconds / ((Math.Max(1, Me.progCurrentFamily.Value) / (progCurrentFamily.Maximum - SkipCount)) + progOverall.Value)) * progOverall.Maximum)).Subtract(Duration)
        Me.lblRemainingValue.Text = String.Format(My.Resources.x0Hours1Minutes2Seconds, Remaining.Hours.ToString, Remaining.Minutes.ToString, Remaining.Seconds.ToString)
    End Sub

    Private Sub ResetFamilyProgress(ByVal MaxValue As Integer)
        progCurrentFamily.Minimum = 0
        progCurrentFamily.Maximum = 100
        progCurrentFamily.Value = 0
    End Sub


    ''' <summary>
    ''' Gets an instance of the specified font.
    ''' </summary>
    ''' <param name="FontName">The name of the font required.</param>
    ''' <returns>Returns an instance of the first available font style,
    ''' or nothing if none available.</returns>
    Private Function GetFontInstance(ByVal FontName As String) As Font
        'Monotype corsiva only supports Italic,bold,underlined (but not
        'regular) for example and throws an exception if you try to create
        'an instance without specifying the italic style. However, there doesn't
        'seem to be any better way (than the following) of determining which
        'styles are supported.

        Dim F As Font = Nothing
        For Each st As FontStyle In New FontStyle() {FontStyle.Regular, FontStyle.Bold, FontStyle.Italic, FontStyle.Underline}
            Try
                F = New Font(FontName, 8, st)
            Catch ex As Exception
                'do nothing
            End Try
            If F IsNot Nothing Then Return F
        Next

        Return Nothing
    End Function


    Private Function IsFontSymbolFont(ByVal F As Font) As Boolean
        Dim bm As New Bitmap(1, 1)
        Dim g As Graphics = Graphics.FromImage(bm)

        Dim hdc As IntPtr
        Dim hFontOld As IntPtr
        Dim lpOtm As IntPtr = IntPtr.Zero
        Try
            hdc = g.GetHdc()
            hFontOld = SelectObject(hdc, F.ToHfont())
            Dim bufSize As Int32 = GetOutlineTextMetrics(hdc, 0, IntPtr.Zero)

            lpOtm = Marshal.AllocCoTaskMem(bufSize)
            Marshal.WriteInt32(lpOtm, bufSize)
            Dim success As Int32 = GetOutlineTextMetrics(hdc, bufSize, lpOtm)
            If (success <> 0) Then
                Dim TM As OUTLINETEXTMETRIC = CType(Marshal.PtrToStructure(lpOtm, GetType(OUTLINETEXTMETRIC)), OUTLINETEXTMETRIC)
                Return (TM.otmPanoseNumber.bFamilyType = PanoseFontFamilyTypes.PAN_FAMILY_PICTORIAL)
            Else
                Return False
            End If
        Finally
            If lpOtm <> IntPtr.Zero Then
                Marshal.FreeCoTaskMem(lpOtm)
            End If
            SelectObject(hdc, hFontOld)
            g.ReleaseHdc(hdc)
        End Try
    End Function


    Private Sub btnOK_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnOK.Click
        Me.Close()
    End Sub


    Private mbCancelled As Boolean = False

    Private Sub btnCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Me.btnCancel.Enabled = False

        mbCancelled = True
        'We can't join as follows, because the other thread makes
        'cross-threaded calls to update the UI on this thread, so
        'there is a potential for a deadlock under this approach
        'Me.mtWorkerThread.Join()

        'Instead we just sleep
        If Me.mtWorkerThread IsNot Nothing Then
            While Me.mtWorkerThread.IsAlive
                System.Threading.Thread.Sleep(100)
                Application.DoEvents()
            End While
        End If

        Me.Close()
    End Sub

    Private Sub btnBegin_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBegin.Click
        If Not IO.Directory.Exists(Me.txtTargetDir.Text) Then
            MsgBox(My.Resources.TargetDirectoryDoesNotExist)
            Exit Sub
        End If

        Dim MaxSize As Integer
        If Integer.TryParse(Me.txtMaxSize.Text, MaxSize) Then
            If MaxSize > 60 Then
                MsgBox(My.Resources.MaxSizeIsTooLarge)
                Exit Sub
            End If
        Else
            MsgBox(My.Resources.InvalidMaximumSize)
            Exit Sub
        End If

        Dim MinSize As Integer
        If Integer.TryParse(Me.txtMinSize.Text, MinSize) Then
            If MinSize < 5 Then
                MsgBox(My.Resources.MinSizeIsTooSmall)
                Exit Sub
            End If
        Else
            MsgBox(My.Resources.InvalidMinimumSize)
            Exit Sub
        End If

        Dim StepSize As Single
        If Single.TryParse(Me.txtStepSize.Text, StepSize) Then
            If StepSize > 0 Then
                If StepSize < 0.5 Then
                    MsgBox(My.Resources.StepSizeIsTooSmall)
                    Exit Sub
                End If
            Else
                MsgBox(My.Resources.StepSizeMustBePositive)
                Exit Sub
            End If
        End If

        mtWorkerThread = New System.Threading.Thread(AddressOf GenerateFonts)
        mtWorkerThread.Start()
    End Sub

    Dim mtWorkerThread As System.Threading.Thread


    Private Sub btnBrowse_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnBrowse.Click
        Dim dp As New FolderBrowserDialog()
        dp.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        If dp.ShowDialog = Windows.Forms.DialogResult.OK Then
            Me.txtTargetDir.Text = dp.SelectedPath
        End If
    End Sub

    Private Sub chkRegenerateExistingOnly_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkRegenerateExistingOnly.CheckedChanged
        Me.txtMaxSize.Enabled = Not Me.chkRegenerateExistingOnly.Checked
        Me.txtMinSize.Enabled = Me.txtMaxSize.Enabled
        Me.txtStepSize.Enabled = Me.txtMaxSize.Enabled
    End Sub


End Class