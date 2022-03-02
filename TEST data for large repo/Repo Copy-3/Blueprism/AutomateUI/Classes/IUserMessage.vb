Imports AutomateControls

Namespace Classes
    Public Interface IUserMessage
        Sub Show(prompt As String)
        Sub Show(owner As Control, prompt As String)
        Sub Show(prompt As String, helpTopicNumber As Integer)
        Sub ShowExceptionMessage(exception As Exception)
        Sub ShowPermissionMessage()
        Sub ShowCantExecuteProcessMessage()
        Sub Show(prompt As String, helpTopicNumber As Integer, helpPage As String)
        Sub Show(owner As Control, prompt As String, helpTopicNumber As Integer, helpPage As String)
        Function ShowError(message As String) As Boolean
        Function ShowError(message As String, ByVal ParamArray arguments() As Object) As Boolean
        Function ShowError(exception As Exception, message As String, ByVal ParamArray arguments() As Object) As Boolean
        Function ShowError(exception As Exception, message As String) As Boolean
        Sub Show(prompt As String, exception As Exception, Optional topic As Integer = -1, Optional helpPage As String = "")
        Sub ShowDetail(prompt As String, detail As String, Optional topic As Integer = -1, Optional helpPage As String = "")
        Sub Show(prompt As String, x As Integer, y As Integer)
        Sub Show(prompt As String, location As Point)
        Sub ShowFloating(parent As Control, icon As ToolTipIcon, title As String, prompt As String, location As Point, Optional duration As Integer = UserMessage.DefaultFloatingDuration, Optional inBalloonStyle As Boolean = False)
        Sub ShowFloating(parent As Control, icon As ToolTipIcon, title As String, prompt As String, location As Point, inBalloonStyle As Boolean)
        Sub CancelFloatingMessage(parent As Control)
        Function GetDefaultWidthOfUserMessageForm() As Integer
        Sub ShowBlueBarMessage(prompt As String, comment As String, blueBarTitle As String, colorManager As IEnvironmentColourManager, Optional ByVal helpTopicNumber As Integer = -1, Optional ByVal helpPage As String = "", Optional ByVal windowWidth As Integer = 0, Optional ByVal windowHeight As Integer = 0)
        Function ShowYesNo(prompt As String) As Boolean
        Function ShowYesNo(promptFormat As String, ByVal ParamArray arguments() As Object) As Boolean
        Function ShowYesNoMessageBox(prompt As String) As MsgBoxResult
        Function ShowYesNoMessageBox(promptFormat As String, ByVal ParamArray arguments() As Object) As MsgBoxResult
        Function ShowTwoButton(prompt As String, yesButtonText As String, noButtonText As String) As MsgBoxResult
        Function ShowTwoButton(promptFormat As String, yesButtonText As String, noButtonText As String, ByVal ParamArray arguments() As Object) As MsgBoxResult
        Function ShowYesNoCancel(prompt As String, Optional ByVal autoResize As Boolean = False) As MsgBoxResult
        Function ShowOk(prompt As String) As MsgBoxResult
        Function ShowOk(promptFormat As String, ByVal ParamArray arguments() As Object) As MsgBoxResult
        Function ShowOkCancel(prompt As String) As MsgBoxResult
        Function ShowOkCancel(promptFormat As String, ByVal ParamArray arguments() As Object) As MsgBoxResult
        Function ShowOkCancelWithComboBox(prompt As String, items As ICollection(Of String), ByRef comboBoxText As String, Optional okButtonText As String = "Ok", Optional cancelButtonText As String = "Cancel") As MsgBoxResult
        Function ShowYesNoCancelWithLinkLabel(prompt As String, labelText As String, labelHandler As EventHandler, Optional yesButtonText As String = "", Optional noButtonText As String = "", Optional cancelButtonText As String = "") As MsgBoxResult
    End Interface
End Namespace