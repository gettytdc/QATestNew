Imports AutomateControls
Imports BluePrism.AutomateAppCore

''' <summary>
''' Password text box that doesn't store the password as plain text at any point.
''' This inherits from the generic <see cref="SecurePasswordTextBox"></see> control
''' and sets the AllowPasting property based on the system preference. This inherited
''' control should be always be used on controls to ensure that the AllowPasting 
''' preference is applied globally.
''' </summary>
Public Class ctlAutomateSecurePassword
    Inherits SecurePasswordTextBox

    ''' <summary>
    ''' Override OnCreateControl of the base control and set the AllowPasting
    ''' property based on the system preference.
    ''' </summary>
    Protected Overrides Sub OnCreateControl()
        MyBase.OnCreateControl()
        If Not Me.DesignMode AndAlso ServerFactory.ServerAvailable Then
            AllowPasting = gSv.GetAllowPasswordPasting()
        End If
    End Sub

End Class
