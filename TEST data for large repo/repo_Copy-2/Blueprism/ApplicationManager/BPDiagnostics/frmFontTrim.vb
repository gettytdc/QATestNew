Option Strict On

Imports BluePrism.BPCoreLib
Imports BluePrism.CharMatching

Public Class frmFontTrim

    ''' <summary>
    ''' The font being trimmed.
    ''' </summary>
    Private mFont As FontData

    ''' <summary>
    ''' Shows the form to the user, allowing them to trim the supplied font. This may
    ''' only be called once, since it calls dispose.
    ''' </summary>
    ''' <param name="Font">The font to be trimmed.</param>
    Friend Sub ShowFont(ByVal Font As FontData)
        mFont = Font

        Dim FontHeight As Integer = 0
        Dim MinRowsAbove As Integer = Integer.MaxValue
        Dim MinRowsBelow As Integer = Integer.MaxValue
        For Each c As CharData In Font.Characters
            Dim RowsAbove As Integer = GetBlankRowCount(c, False)
            MinRowsAbove = Math.Min(RowsAbove, MinRowsAbove)

            Dim RowsBelow As Integer = GetBlankRowCount(c, True)
            MinRowsBelow = Math.Min(RowsBelow, MinRowsBelow)

            FontHeight = Math.Max(FontHeight, c.Height)
        Next

        lblSpaceAbove.Text = String.Format(My.Resources.SpaceAbove0, MinRowsAbove)
        txtRowsAbove.Text = MinRowsAbove.ToString
        txtRowsAbove.Enabled = MinRowsAbove > 0
        btnGoAbove.Enabled = Me.txtRowsAbove.Enabled

        lblSpaceBelow.Text = String.Format(My.Resources.SpaceBelow0, MinRowsBelow)
        txtRowsBelow.Text = MinRowsBelow.ToString
        txtRowsBelow.Enabled = MinRowsBelow > 0
        btnGoBelow.Enabled = Me.txtRowsBelow.Enabled

        lblFontHeight.Text = String.Format(My.Resources.FontHeight0, FontHeight)

        Try
            ShowDialog()
        Finally
            Dispose()
        End Try
    End Sub

    ''' <summary>
    ''' Counts the number of blank rows at the top, or bottom, of the supplied
    ''' character.
    ''' </summary>
    ''' <param name="c">The character of interest.</param>
    ''' <param name="WorkUpwards">If true, counts the empty rows at the bottom,
    ''' otherwise counts the rows at the top.</param>
    ''' <returns>Returns the number of empty rows at the top/bottom of the character
    ''' until a non-blank row is encountered.</returns>
    Friend Shared Function GetBlankRowCount(ByVal c As CharData, ByVal WorkUpwards As Boolean) As Integer
        Dim Increment As Integer
        Dim StartIndex As Integer
        Dim EndIndex As Integer
        If WorkUpwards Then
            Increment = -1
            StartIndex = c.Height - 1
            EndIndex = 0
        Else
            Increment = 1
            StartIndex = 0
            EndIndex = c.Height - 1
        End If

        'Find out how many blank rows above the font character there are
        Dim BlankRows As Integer
        For j As Integer = StartIndex To EndIndex Step Increment
            Dim RowClear As Boolean = True
            For i As Integer = 0 To c.Width - 1
                If c.Mask(i, j) Then
                    RowClear = False
                    Exit For
                End If
            Next
            If RowClear Then
                BlankRows += 1
            Else
                Exit For
            End If
        Next

        Return BlankRows
    End Function

    Private Sub btnGoAbove_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnGoAbove.Click
        Dim TrimQuantity As Integer
        If Integer.TryParse(Me.txtRowsAbove.Text, TrimQuantity) Then
            If TrimQuantity <= 0 Then
                MsgBox(My.Resources.TrimValueMustBePositive)
                Exit Sub
            End If
        Else
            MsgBox(My.Resources.InvalidNumberOfRows)
        End If

        For Each c As CharData In mFont.Characters
            c.Strip(Direction.Top, TrimQuantity)
        Next

        Me.txtRowsAbove.Enabled = False
        Me.btnGoAbove.Enabled = False
    End Sub

    Private Sub btnGobelow_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnGoBelow.Click
        Dim TrimQuantity As Integer
        If Integer.TryParse(Me.txtRowsBelow.Text, TrimQuantity) Then
            If TrimQuantity <= 0 Then
                MsgBox(My.Resources.TrimValueMustBePositive)
                Exit Sub
            End If
        Else
            MsgBox(My.Resources.InvalidNumberOfRows)
        End If

        For Each c As CharData In mFont.Characters
            c.Strip(Direction.Bottom, TrimQuantity)
        Next

        Me.txtRowsBelow.Enabled = False
        Me.btnGoBelow.Enabled = False
    End Sub

    Private Sub btnClose_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnClose.Click
        Me.DialogResult = Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub
End Class
