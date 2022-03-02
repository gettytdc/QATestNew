''' <summary>
''' Basic Save Prompt configuration which has an OK and a Cancel button
''' </summary>
Public Class OkCancelSavePromptConfig : Implements ISavePromptConfig

    ' The OK button
    Protected btnOk As AutomateControls.Buttons.StandardStyledButton

    ' The Cancel button
    Protected btnCancel As AutomateControls.Buttons.StandardStyledButton

    ' The window text for the prompt form
    Private mFormText As String

    ' The title of the prompt form
    Private mHeading As String

    ' The subtitle of the prompt form
    Private mSubheading As String

    ' The prompt, requesting the users input in the form
    Private mPrompt As String

    ''' <summary>
    ''' Creates a new Save Prompt Config object with an OK and Cancel button and
    ''' the specified text values.
    ''' </summary>
    ''' <param name="formText">The window title for the prompt form</param>
    ''' <param name="headingText">The heading text for the prompt form</param>
    ''' <param name="subheadingText">The subheading text for the form</param>
    ''' <param name="promptText">The prompt for the user</param>
    Public Sub New(ByVal formText As String, _
     ByVal headingText As String, ByVal subheadingText As String, ByVal promptText As String)
        btnOk = New AutomateControls.Buttons.StandardStyledButton()
        btnOk.UseVisualStyleBackColor = True
        btnOk.Text = "OK"
        btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        btnCancel.UseVisualStyleBackColor = True
        btnCancel.Text = My.Resources.btnCancel
        mFormText = formText
        mHeading = headingText
        mSubheading = subheadingText
        mPrompt = promptText
    End Sub

    ''' <summary>
    ''' The window title for the form described by this config
    ''' </summary>
    Public Overridable ReadOnly Property WindowTitle() As String _
     Implements ISavePromptConfig.WindowTitle
        Get
            Return mFormText
        End Get
    End Property

    ''' <summary>
    ''' The heading of the form, displayed overlaying the header image.
    ''' </summary>
    Public Overridable ReadOnly Property Heading() As String Implements ISavePromptConfig.Heading
        Get
            Return mHeading
        End Get
    End Property

    ''' <summary>
    ''' The prompt, immediately before the text area, which gives the user direction
    ''' on what should be entered in the text area.
    ''' </summary>
    Public Overridable ReadOnly Property Prompt() As String Implements ISavePromptConfig.Prompt
        Get
            Return mPrompt
        End Get
    End Property

    ''' <summary>
    ''' True to set the header styling to Object Studio styling
    ''' </summary>
    Public Overridable ReadOnly Property ObjectStudioMode() As Boolean _
     Implements ISavePromptConfig.ObjectStudioMode
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' The button to display in the lower left of the form
    ''' </summary>
    Public ReadOnly Property LeftButton() As AutomateControls.Buttons.StandardStyledButton Implements ISavePromptConfig.LeftButton
        Get
            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' The collection of buttons to display in the lower right of the form, in the
    ''' left-to-right order that they should be displayed.
    ''' </summary>
    Public ReadOnly Property RightButtons() As ICollection(Of AutomateControls.Buttons.StandardStyledButton) _
     Implements ISavePromptConfig.RightButtons
        Get
            Return New AutomateControls.Buttons.StandardStyledButton() {btnOk, btnCancel}
        End Get
    End Property

    ''' <summary>
    ''' Gets the appropriate response for the save prompt form given the button
    ''' pressed and the current text that the user has entered.
    ''' </summary>
    ''' <param name="ctl">The owner control - typically the window which is asking
    ''' for the appropriate response.</param>
    ''' <param name="btnPressed">The button which the user has pressed.</param>
    ''' <param name="txt">The text that the user has entered</param>
    ''' <returns>A dialog result that the form should return to the calling class, or
    ''' <see cref="DialogResult.None"/> if the dialog should remain open.</returns>
    Public Overridable Function GetResponse(ByVal ctl As Control,
     ByVal btnPressed As Button, ByVal txt As String) As DialogResult _
     Implements ISavePromptConfig.GetResponse
        If btnPressed Is btnOk Then Return DialogResult.OK
        If btnPressed Is btnCancel Then Return DialogResult.Cancel
        Throw New InvalidOperationException(My.Resources.UnrecognisedButton & btnPressed.Text)
    End Function

End Class
