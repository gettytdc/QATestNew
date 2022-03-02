
''' <summary>
''' Interface describing the configuration for the Save Prompt form
''' </summary>
Public Interface ISavePromptConfig
    ''' <summary>
    ''' The window title for the form described by this config
    ''' </summary>
    ReadOnly Property WindowTitle() As String

    ''' <summary>
    ''' The heading of the form, displayed overlaying the header image.
    ''' </summary>
    ReadOnly Property Heading() As String

    ''' <summary>
    ''' The prompt, immediately before the text area, which gives the user direction
    ''' on what should be entered in the text area.
    ''' </summary>
    ReadOnly Property Prompt() As String

    ''' <summary>
    ''' True to set the header styling to Object Studio styling
    ''' </summary>
    ReadOnly Property ObjectStudioMode() As Boolean

    ''' <summary>
    ''' The button to display in the lower left of the form, or null if no button
    ''' should be shown in the lower left
    ''' </summary>
    ReadOnly Property LeftButton() As AutomateControls.Buttons.StandardStyledButton

    ''' <summary>
    ''' The collection of buttons to display in the lower right of the form, in the
    ''' left-to-right order that they should be displayed.
    ''' </summary>
    ReadOnly Property RightButtons() As ICollection(Of AutomateControls.Buttons.StandardStyledButton)

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
    Function GetResponse(ByVal ctl As Control, _
     ByVal btnPressed As Button, ByVal txt As String) As DialogResult

End Interface
