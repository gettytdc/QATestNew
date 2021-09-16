Imports System.Runtime.InteropServices

''' Project  : Automate
''' Class    : MultilineTextBox
'''
''' <summary>
''' Wrapper around textbox control that doesn't steal mousescroll events.
''' </summary>
Public Class MultilineTextBox
    Inherits AutomateControls.Textboxes.StyledTextBox

    Public Sub New()
        MyBase.New()
        Me.Multiline = True
    End Sub

    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
        'Send the WM_MouseScroll event to the parent
        If (m.Msg = &H20A) Then
            SendMessage(Me.Parent.Handle, m.Msg, m.WParam, m.LParam)
        Else
            MyBase.WndProc(m)
        End If
    End Sub

    <DllImport("user32.dll")> _
    Private Shared Sub SendMessage(ByVal hWnd As IntPtr, ByVal msg As Int32, ByVal wp As IntPtr, ByVal lp As IntPtr)
    End Sub

End Class
