

''' <summary>
''' Control used to capture a name within a wizard.
''' </summary>
Public Class ctlNamer : Inherits ctlWizardStageControl

    ' The suggested name to use in this control
    Private mSuggestedName As String

    ''' <summary>
    ''' Creates a new namer control
    ''' </summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new namer control with the given suggested name
    ''' </summary>
    ''' <param name="suggestedName">The default name to use in this control</param>
    Public Sub New(ByVal suggestedName As String)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mSuggestedName = suggestedName
    End Sub

    ''' <summary>
    ''' The currently set name in this control.
    ''' </summary>
    Public Overrides Property Text() As String
        Get
            Return txtName.Text
        End Get
        Set(ByVal value As String)
            txtName.Text = value
        End Set
    End Property

    ''' <summary>
    ''' Handles the text box being actioned - this checks for a carriage return char
    ''' and fires an <see cref="Activated"/> event if it discovers that the enter
    ''' key was pressed.
    ''' </summary>
    Private Sub HandleTextActioned(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtName.Activated
        OnActivated(New ActivationEventArgs())
    End Sub

End Class
