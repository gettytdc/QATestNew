''' <summary>
''' Base class for a wizard stage control. Implements the Activated event.
''' </summary>
Public Class ctlWizardStageControl : Inherits UserControl

    ''' <summary>
    ''' Event fired when this control is activated
    ''' </summary>
    Public Event Activated(ByVal sender As Object, ByVal e As ActivationEventArgs)

    ''' <summary>
    ''' Event handler which handles this control being 'activated', that is, the
    ''' enter key being pressed in the name text box.
    ''' </summary>
    Protected Overridable Sub OnActivated(ByVal e As ActivationEventArgs)
        RaiseEvent Activated(Me, e)
    End Sub

    ''' <summary>
    ''' Gets the focus onto the first control on this stage control.
    ''' </summary>
    ''' <returns>True if a control was found and focused, false otherwise.</returns>
    Public Function GetFirstFocus() As Boolean
        Dim ctl As Control = Me
        While ctl IsNot Nothing AndAlso Not ctl.CanSelect
            ctl = GetNextControl(ctl, True)
        End While
        If ctl IsNot Nothing Then Return ctl.Focus() Else Return False
    End Function

    ''' <summary>
    ''' Reimplementation of the TextChanged event, which just adds handlers to the
    ''' parent event. RaiseEvent() should never be called on this event. This just
    ''' enables it to be set in the designer and in Intellisense (which UserControl
    ''' disables... for some reason)
    ''' </summary>
    <EditorBrowsable(EditorBrowsableState.Always), Browsable(True)> _
    Public Shadows Custom Event TextChanged As EventHandler
        AddHandler(ByVal value As EventHandler)
            AddHandler MyBase.TextChanged, value
        End AddHandler
        RemoveHandler(ByVal value As EventHandler)
            RemoveHandler MyBase.TextChanged, value
        End RemoveHandler
        RaiseEvent(ByVal sender As Object, ByVal e As EventArgs)
            Debug.Fail("RaiseEvent() called on Shadowing TextChanged event")
        End RaiseEvent
    End Event

    Protected Overrides Sub OnTextChanged(ByVal e As EventArgs)
        MyBase.OnTextChanged(e)
    End Sub

    Public Overrides Property Text() As String
        Get
            Return MyBase.Text
        End Get
        Set(ByVal value As String)
            MyBase.Text = value
        End Set
    End Property

End Class
