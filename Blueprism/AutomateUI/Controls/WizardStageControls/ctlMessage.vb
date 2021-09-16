''' <summary>
''' A wizard stage control showing a message - just a fill-docked label.
''' </summary>
Public Class ctlMessage : Inherits ctlWizardStageControl

    ''' <summary>
    ''' The message being displayed by this control
    ''' </summary>
    Public Overrides Property Text() As String
        Get
            Return lblMessage.Text
        End Get
        Set(ByVal value As String)
            lblMessage.Text = value
        End Set
    End Property

End Class
