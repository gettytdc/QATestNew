''' Project  : Automate
''' Class    : ctlMaskedSecurePasswordTextBox
''' 
''' <summary>
''' Ensures that the password mask is always displayed.
''' </summary>
Friend Class ctlMaskedSecurePasswordTextBox
    Inherits AutomateUI.ctlAutomateSecurePassword

    Public Sub New()
        MyBase.New()

        Me.UsePlaceholder = True

    End Sub

End Class
