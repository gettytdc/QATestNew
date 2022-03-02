Imports AutomateControls

Namespace Classes
    Public Class UserMessageWrapper : Implements IUserMessage

        Public Sub Show(prompt As String) Implements IUserMessage.Show
            UserMessage.Show(prompt)
        End Sub

        Public Sub ShowExceptionMessage(exception As Exception) Implements IUserMessage.ShowExceptionMessage
            UserMessage.ShowExceptionMessage(exception)
        End Sub

        Public Sub Show(owner As Control, prompt As String) Implements IUserMessage.Show
            UserMessage.Show(owner, prompt)
        End Sub

        Public Sub Show(prompt As String, helpTopicNumber As Integer) Implements IUserMessage.Show
            UserMessage.Show(prompt, helpTopicNumber)
        End Sub

        Public Sub ShowPermissionMessage() Implements IUserMessage.ShowPermissionMessage
            UserMessage.ShowPermissionMessage()
        End Sub

        Public Sub ShowCantExecuteProcessMessage() Implements IUserMessage.ShowCantExecuteProcessMessage
            UserMessage.ShowCantExecuteProcessMessage()
        End Sub

        Public Sub Show(prompt As String, helpTopicNumber As Integer, helpPage As String) Implements IUserMessage.Show
            UserMessage.Show(prompt, helpTopicNumber, helpPage)
        End Sub

        Public Sub Show(owner As Control, prompt As String, helpTopicNumber As Integer, helpPage As String) Implements IUserMessage.Show
            UserMessage.Show(owner, prompt, helpTopicNumber, helpPage)
        End Sub

        Public Function ShowError(message As String) As Boolean Implements IUserMessage.ShowError
            Return UserMessage.Err(message)
        End Function

        Public Function ShowError(message As String, ParamArray arguments As Object()) As Boolean Implements IUserMessage.ShowError
            Return UserMessage.Err(message, arguments)
        End Function

        Public Function ShowError(exception As Exception, message As String, ParamArray arguments As Object()) As Boolean Implements IUserMessage.ShowError
            Return UserMessage.Err(exception, message, arguments)
        End Function

        Public Function ShowError(exception As Exception, message As String) As Boolean Implements IUserMessage.ShowError
            Return UserMessage.Err(exception, message)
        End Function

        Public Sub Show(prompt As String, exception As Exception, Optional topic As Integer = -1, Optional helpPage As String = "") Implements IUserMessage.Show
            UserMessage.Show(prompt, exception, topic, helpPage)
        End Sub

        Public Sub ShowDetail(prompt As String, detail As String, Optional topic As Integer = -1, Optional helpPage As String = "") Implements IUserMessage.ShowDetail
            UserMessage.ShowDetail(prompt, detail, topic, helpPage)
        End Sub

        Public Sub Show(prompt As String, x As Integer, y As Integer) Implements IUserMessage.Show
            UserMessage.Show(prompt, x, y)
        End Sub

        Public Sub Show(prompt As String, location As Point) Implements IUserMessage.Show
            UserMessage.Show(prompt, location)
        End Sub

        Public Sub ShowFloating(parent As Control, icon As ToolTipIcon, title As String, prompt As String, location As Point, Optional duration As Integer = UserMessage.DefaultFloatingDuration, Optional inBalloonStyle As Boolean = False) Implements IUserMessage.ShowFloating
            UserMessage.ShowFloating(parent, icon, title, prompt, location, duration, inBalloonStyle)
        End Sub

        Public Sub ShowFloating(parent As Control, icon As ToolTipIcon, title As String, prompt As String, location As Point, inBalloonStyle As Boolean) Implements IUserMessage.ShowFloating
            UserMessage.ShowFloating(parent, icon, title, prompt, location, inBalloonStyle)
        End Sub

        Public Sub CancelFloatingMessage(parent As Control) Implements IUserMessage.CancelFloatingMessage
            UserMessage.CancelFloatingMessage(parent)
        End Sub

        Public Function GetDefaultWidthOfUserMessageForm() As Integer Implements IUserMessage.GetDefaultWidthOfUserMessageForm
            Return UserMessage.GetDefaultWidthOfUserMessageForm()
        End Function

        Public Sub ShowBlueBarMessage(prompt As String, comment As String, blueBarTitle As String, colorManager As IEnvironmentColourManager, Optional helpTopicNumber As Integer = -1, Optional helpPage As String = "", Optional windowWidth As Integer = 0, Optional windowHeight As Integer = 0) Implements IUserMessage.ShowBlueBarMessage
            UserMessage.ShowBlueBarMessage(prompt, comment, blueBarTitle, colorManager, helpTopicNumber, helpPage, windowWidth, windowHeight)
        End Sub

        Public Function ShowYesNo(prompt As String) As Boolean Implements IUserMessage.ShowYesNo
            Return UserMessage.Yes(prompt)
        End Function

        Public Function ShowYesNo(promptFormat As String, ParamArray arguments As Object()) As Boolean Implements IUserMessage.ShowYesNo
            Return UserMessage.Yes(promptFormat, arguments)
        End Function

        Public Function ShowYesNoMessageBox(prompt As String) As MsgBoxResult Implements IUserMessage.ShowYesNoMessageBox
            Return UserMessage.YesNo(prompt)
        End Function

        Public Function ShowYesNoMessageBox(promptFormat As String, ParamArray arguments As Object()) As MsgBoxResult Implements IUserMessage.ShowYesNoMessageBox
            Return UserMessage.YesNo(promptFormat, arguments)
        End Function

        Public Function ShowTwoButton(prompt As String, yesButtonText As String, noButtonText As String) As MsgBoxResult Implements IUserMessage.ShowTwoButton
            Return UserMessage.TwoButtonsWithCustomText(prompt, yesButtonText, noButtonText)
        End Function

        Public Function ShowTwoButton(promptFormat As String, yesButtonText As String, noButtonText As String, ParamArray arguments As Object()) As MsgBoxResult Implements IUserMessage.ShowTwoButton
            Return UserMessage.TwoButtonsWithCustomText(promptFormat, yesButtonText, noButtonText, arguments)
        End Function

        Public Function ShowYesNoCancel(prompt As String, Optional autoResize As Boolean = False) As MsgBoxResult Implements IUserMessage.ShowYesNoCancel
            Return UserMessage.YesNoCancel(prompt, autoResize)
        End Function

        Public Function ShowOk(prompt As String) As MsgBoxResult Implements IUserMessage.ShowOk
            Return UserMessage.OK(prompt)
        End Function

        Public Function ShowOk(promptFormat As String, ParamArray arguments As Object()) As MsgBoxResult Implements IUserMessage.ShowOk
            Return UserMessage.OK(promptFormat, arguments)
        End Function

        Public Function ShowOkCancel(prompt As String) As MsgBoxResult Implements IUserMessage.ShowOkCancel
            Return UserMessage.OkCancel(prompt)
        End Function

        Public Function ShowOkCancel(promptFormat As String, ParamArray arguments As Object()) As MsgBoxResult Implements IUserMessage.ShowOkCancel
            Return UserMessage.OkCancel(promptFormat, arguments)
        End Function

        Public Function ShowOkCancelWithComboBox(prompt As String, items As ICollection(Of String), ByRef comboBoxText As String, Optional okButtonText As String = "Ok", Optional cancelButtonText As String = "Cancel") As MsgBoxResult Implements IUserMessage.ShowOkCancelWithComboBox
            Return UserMessage.OkCancelWithComboBox(prompt, items, comboBoxText, okButtonText, cancelButtonText)
        End Function

        Public Function ShowYesNoCancelWithLinkLabel(prompt As String, labelText As String, labelHandler As EventHandler, Optional yesButtonText As String = "", Optional noButtonText As String = "", Optional cancelButtonText As String = "") As MsgBoxResult Implements IUserMessage.ShowYesNoCancelWithLinkLabel
            Return UserMessage.YesNoCancelWithLinkLabel(prompt, labelText, labelHandler, yesButtonText, noButtonText, cancelButtonText)
        End Function
    End Class
End Namespace