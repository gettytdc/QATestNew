Imports BluePrism.AutomateAppCore
Imports BluePrism.BPCoreLib

Public Class CompulsorySummaryPromptConfig : Inherits OkCancelSavePromptConfig

    ''' <summary>
    ''' Creates a new CompulsorySummary prompt config object with the given
    ''' properties
    ''' </summary>
    ''' <param name="formText">The text for the form</param>
    ''' <param name="headingText">The text for the heading</param>
    ''' <param name="subheadingText">The text for the subheading</param>
    ''' <param name="promptText">The text for the actual user prompt</param>
    Public Sub New(ByVal formText As String, _
     ByVal headingText As String, ByVal subheadingText As String, ByVal promptText As String)
        MyBase.New(formText, headingText, subheadingText, promptText)
    End Sub

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
    Public Overrides Function GetResponse(ByVal ctl As Control, _
     ByVal btnPressed As Button, ByVal txt As String) As DialogResult

        ' If the user has OK'd the prompt and not entered any text, and the administrator
        ' has required that edit summaries are compulsory, prompt the user for a
        ' description and leave the save prompt open.
        If btnPressed Is btnOk AndAlso txt.Trim() = "" _
         AndAlso Options.Instance.EditSummariesAreCompulsory Then
            UserMessage.Show(ctl, String.Format(
             My.Resources.Your0AdministratorHasRequestedThatAllChangesToProcessesBeAccompaniedByAnEditSum, ApplicationProperties.ApplicationName))
            Return DialogResult.None
        End If

        Return MyBase.GetResponse(ctl, btnPressed, txt)

    End Function

End Class
