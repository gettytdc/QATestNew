''' Project  : Automate
''' Class    : clsNumericOnlyTextBox
''' 
''' <summary>
''' A textbox into which only numeric information may be entered.
''' </summary>
Public Class clsNumericOnlyTextBox
    Inherits AutomateControls.Textboxes.StyledTextBox

    ''' <summary>
    ''' The ascii code for the backspace character
    ''' </summary>
    Private Const BackSpaceKeyCode As Integer = 8

    ''' <summary>
    ''' Characters which may be entered into this box.
    ''' </summary>
    Private mAllowedCharacters As Integer() = {AscW("-"c), AscW("."c), BackSpaceKeyCode}


    Private Sub clsNumericOnlyTextBox_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles MyBase.KeyPress
        If (AscW(e.KeyChar) < 48 OrElse AscW(e.KeyChar) > 57) AndAlso Array.IndexOf(Me.mAllowedCharacters, AscW(e.KeyChar)) = -1 Then e.Handled = True
    End Sub

End Class
