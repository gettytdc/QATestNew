Imports System.Globalization
Imports System.Linq
Imports BluePrism.AutomateAppCore
Imports BluePrism.Core.Utility

Public Class SelectLanguageForm
    Public NewLocale As String = Nothing
    Private ReadOnly mPseudoLocalization As Boolean = False
    Private Shared mCultureList As List(Of LocaleListEntry) = New List(Of LocaleListEntry)()
    Private ReadOnly mLabelFont As Font = New Font("Segoe UI", 16, FontStyle.Regular, GraphicsUnit.Pixel)
    Private ReadOnly mLabelFontBold As Font = New Font("Segoe UI", 16, FontStyle.Bold, GraphicsUnit.Pixel)

    Public Sub New(ByVal pseudoLocalization As Boolean)
        InitializeComponent()
        Me.KeyPreview = True
        mPseudoLocalization = pseudoLocalization
#If DEBUG Then
        mPseudoLocalization = True
#End If
        If mCultureList.Count = 0 Then
            GenerateCulturesTable()
        Else
            mCultureList.FirstOrDefault(Function(x) x.ID = 0).Text = My.Resources.LocaleConfigForm_UsingWindowsLocaleSettings
        End If
        mCultureList.Sort()
        PopulateLocales()
    End Sub

    Private Sub PopulateLocales()
        ListPanel.Controls.Clear()
        Dim yPos = 26
        Dim yImage = 0
        Dim yBar = 0
        Dim xImage = 126
        Dim xWord = 150
        Dim activeWord = ""

        If mCultureList.Where(Function(x) x.Active).Count = 0 Then
            Dim currentCulture = CultureInfo.CurrentUICulture.Name
            If CultureInfo.CurrentUICulture.Parent.Name = "fr" Then currentCulture = "fr-FR"
            If CultureHelper.IsLatinAmericanSpanish() Then currentCulture = CultureHelper.LatinAmericanSpanish
            If mCultureList.FirstOrDefault(Function(x) x.Value = currentCulture) IsNot Nothing Then
                mCultureList.FirstOrDefault(Function(x) x.Value = currentCulture).Active = True
            End If
        End If

        For Each culture In mCultureList
            Dim l = New Label With {
                    .Text = culture.Text,
                    .ForeColor = Color.FromArgb(17, 126, 194),
                    .Size = New Size(clsUtility.MeasureString(mLabelFontBold, culture.Text), 24),
                    .Name = $"L{culture.ID.ToString()}",
                    .Font = mLabelFont
                }
            AddHandler l.Click, AddressOf L_Click
            ListPanel.Controls.Add(l)
            l.Location = New Point(xWord, yPos)

            If culture.Active Then
                activeWord = l.Text
                yImage = yPos + 4
                yBar = yPos + 24
                l.Font = mLabelFontBold
            End If

            yPos += 32
        Next

        Dim i = New PictureBox With {
                .Image = My.Resources.triangle_fwd_2x,
                .Size = New Size(16, 16),
                .Name = $"ActiveEntry",
                .Visible = (yImage > 0)
            }
        ListPanel.Controls.Add(i)
        i.Location = New Point(xImage, yImage)
        Dim b = New Panel() With {
                .BackColor = Color.FromArgb(17, 126, 194),
                .Height = 4,
                .Width = MeasureString(mLabelFontBold, activeWord),
                .Name = "UnderlineBar",
                .Visible = (yBar > 0)
            }
        ListPanel.Controls.Add(b)
        b.Location = New Point(xWord + 5, yBar)
    End Sub

    Private Function MeasureString(lFont As Font, lText As String) As Integer
        Using image = New Bitmap(1, 1)
            Using g = Graphics.FromImage(image)
                Return CInt(Math.Ceiling(g.MeasureString(lText, lFont).Width))
            End Using
        End Using
    End Function

    Private Sub L_Click(sender As Object, e As EventArgs)

        Dim label = CType(sender, Label)
        Dim selectedLocale = label.Name.Substring(1)
        Dim oldCulture = mCultureList.FirstOrDefault(Function(x) x.Active)

        If oldCulture IsNot Nothing Then
            mCultureList.Remove(oldCulture)
            oldCulture.Active = False
            mCultureList.Add(oldCulture)
        End If

        Dim newCulture = mCultureList.FirstOrDefault(Function(x) x.ID.ToString() = selectedLocale)

        If newCulture IsNot Nothing Then
            ResetLabelFonts()
            mCultureList.Remove(newCulture)
            newCulture.Active = True
            mCultureList.Add(newCulture)

            If newCulture.ID.ToString() IsNot Nothing Then
                Dim activeEntry = CType(ListPanel.Controls.Find("ActiveEntry", True).First(), PictureBox)
                activeEntry.Visible = True
                activeEntry.Location = New Point(126, label.Location.Y + 4)

                Dim underlineBar = CType(ListPanel.Controls.Find("UnderlineBar", True).First(), Panel)
                underlineBar.Location = New Point(label.Location.X + 5, label.Location.Y + 24)
                underlineBar.Width = MeasureString(mLabelFontBold, label.Text)
                underlineBar.Visible = True

                label.Font = mLabelFontBold
            End If

            NewLocale = newCulture.Value
        End If

        mCultureList.Sort()
    End Sub

    Private Sub ResetLabelFonts()
        For Each c As Control In ListPanel.Controls
            Dim label = TryCast(c, Label)
            If label IsNot Nothing Then label.Font = mLabelFont
        Next
    End Sub

    Private Class LocaleListEntry
        Implements IComparable

        Public ID As Integer
        Public Text As String
        Public Value As String
        Public Active As Boolean


        Private Function IComparable_CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            Return ID.CompareTo(CType(obj, LocaleListEntry).ID)
        End Function
    End Class

    Private Sub GenerateCulturesTable()
        mCultureList.Clear()
        Dim id = 0

        mCultureList.Add(New LocaleListEntry With {
                .ID = id,
                .Text = My.Resources.LocaleConfigForm_UsingWindowsLocaleSettings,
                .Value = Options.Instance.SystemLocale,
                .Active = Options.Instance.SystemLocale = NewLocale
            })

        For Each locale In Internationalisation.Locales.SupportedLocales
            id += 1

            Try
                Dim addCulture = New CultureInfo(locale)
                mCultureList.Add(New LocaleListEntry With {
                    .ID = id,
                    .Text = addCulture.TextInfo.ToTitleCase(addCulture.NativeName),
                    .Value = addCulture.Name,
                    .Active = addCulture.Name = NewLocale
                })
            Catch
            End Try
        Next

        If mPseudoLocalization Then
            Try
                Dim addCulture = New CultureInfo("gsw-LI")
                mCultureList.Add(New LocaleListEntry With {
                    .ID = mCultureList.Count,
                    .Text = "i18n TEST LANGUAGE",
                    .Value = addCulture.Name,
                    .Active = addCulture.Name = NewLocale
                })
            Catch
            End Try
        End If
    End Sub

    Private Sub BtnOK_Click(ByVal sender As Object, ByVal e As EventArgs) Handles NextButton.Click
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub BtnCancel_Click(ByVal sender As Object, ByVal e As EventArgs) Handles ExitButton.Click
        DialogResult = DialogResult.Cancel
        mCultureList.FirstOrDefault(Function(x) x.Value = CultureInfo.CurrentUICulture.Name).Active = True
        If Not String.IsNullOrEmpty(NewLocale) Then
            mCultureList.FirstOrDefault(Function(x) x.Value = NewLocale).Active = False
        End If

        Close()
    End Sub

#Region "Drag and Drop"
    Private mMouseDownLocation As Point

    Private Sub BorderPanel_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles BorderPanel.MouseDown
        If e.Button = MouseButtons.Left AndAlso e.Y < 35 Then mMouseDownLocation = e.Location
    End Sub

    Private Sub BorderPanel_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs) Handles BorderPanel.MouseMove
        If e.Button <> MouseButtons.Left OrElse e.Y > 35 Then Return
        Dim form = CType(Me, Form)
        form.Left += e.Location.X - mMouseDownLocation.X
        form.Top += e.Location.Y - mMouseDownLocation.Y
    End Sub

    Private Sub BorderPanel_MouseUp(sender As Object, e As MouseEventArgs) Handles BorderPanel.MouseUp
        'following on from a drag and drop of the screen the ListPanel scrolbar become unselectable (mouse scrolling works)
        'but tweaking the vertical scroll value this causes the control to become responsive again, invalidating or refreshing does not work.
        If e.Button = MouseButtons.Left Then
            ListPanel.VerticalScroll.Value = If(ListPanel.VerticalScroll.Value + 1 < ListPanel.VerticalScroll.Maximum, ListPanel.VerticalScroll.Value + 1, ListPanel.VerticalScroll.Maximum)
            ListPanel.VerticalScroll.Value = If(ListPanel.VerticalScroll.Value - 1 > ListPanel.VerticalScroll.Minimum, ListPanel.VerticalScroll.Value - 1, ListPanel.VerticalScroll.Minimum)
        End If
    End Sub

    Private Sub SelectLanguageForm_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        Select Case e.KeyCode
            Case Keys.Down
                SelectNextLocale(1)
            Case Keys.Up
                SelectNextLocale(-1)
        End Select
    End Sub

    Private Sub SelectNextLocale(dir As Integer)

        Dim oldCulture = mCultureList.FirstOrDefault(Function(x) x.Active)
        Dim index = oldCulture.ID + dir
        Dim label = TryCast(ListPanel.Controls.Find($"L{index}", True).FirstOrDefault, Label)
        If label Is Nothing Then Return
        ListPanel.ScrollControlIntoView(label)
        L_Click(label, EventArgs.Empty)

    End Sub

#End Region
End Class
