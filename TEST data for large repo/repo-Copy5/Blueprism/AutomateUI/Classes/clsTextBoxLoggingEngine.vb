Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Resources


''' Project  : Automate
''' Class    : AutomateUI.clsTextBoxLoggingEngine
''' 
''' <summary>
''' A logging engine that outputs results to a rich text box.
''' </summary>
Public Class clsTextBoxLoggingEngine : Inherits clsLoggingEngine

    ''' <summary>
    ''' The maximum number of messages that can be placed in the queue
    ''' </summary>
    Private Const MaximumMessages As Integer = 10

    ''' <summary>
    ''' The rich text box to log messages to
    ''' </summary>
    Private mTextBox As ctlRichTextBox

    ''' <summary>
    ''' A queue to store messages that cannot be logged to an invisible rich text box
    ''' </summary>
    Private mMessages As Queue(Of Message)

    ''' <summary>
    ''' Class used to keep messages in queue
    ''' </summary>
    Private Class Message
        Public Sub New()
            Me.New(Nothing)
        End Sub
        Public Sub New(ByVal txt As String)
            Text = txt
        End Sub
        Public Sub New(ByVal txt As String, ByVal ParamArray args() As Object)
            If txt IsNot Nothing Then Text = String.Format(txt, args)
        End Sub
        Public Text As String
        Public LogInhibit As Boolean
    End Class

    ''' <summary>
    ''' Constructor takes a rich text box
    ''' </summary>
    ''' <param name="textBox">The rich text box to log to</param>
    Public Sub New(ByVal proc As clsProcess, ByVal textBox As ctlRichTextBox)
        MyBase.New(New LogContext(
         Environment.UserName, gSv.GetResourceId(ResourceMachine.GetName()), proc, True, Guid.Empty, 0))
        mMessages = New Queue(Of Message)
        mTextBox = textBox
    End Sub

    ''' <summary>
    ''' Toggles the rich text box between single line and multiline view
    ''' </summary>
    Public Sub Toggle()
        mTextBox.Multiline = Not mTextBox.Multiline
        If mTextBox.Multiline Then ShowQueueMessages()
    End Sub

    ''' <summary>
    ''' Returns the toggle state.
    ''' </summary>
    ''' <returns></returns>
    Public Function Toggled() As Boolean
        Return mTextBox.Multiline
    End Function

    ''' <summary>
    ''' Clears the log
    ''' </summary>
    Public Sub Clear()
        mMessages.Clear()
        mTextBox.Clear()
    End Sub

    ''' <summary>
    ''' Logs details about a stage
    ''' </summary>
    ''' <param name="stage">The stage to log</param>
    ''' <param name="msg">Extra detail</param>
    Private Sub LogStage(
     ByVal stage As clsProcessStage, Optional ByVal msg As String = "")
        Dim m = New Message
        Select Case stage.Process.ProcessType
            Case ProcessType.Process
                m = New Message(My.Resources.x012Process3Subsheet456,
                    Now,
                    clsStageTypeName.GetLocalizedFriendlyName(stage.StageType.ToString).ToUpper(),
                    stage.Name,
                    stage.Process.Name,
                    If(stage.SubSheet.Name = "Main Page", My.Resources.frmStagePropertiesSubSheetInfo_MainPage, stage.SubSheet.Name),
                    msg,
                    vbCrLf)
            Case ProcessType.BusinessObject
                m = New Message(My.Resources.x012Object3Subsheet456,
                    Now,
                    clsStageTypeName.GetLocalizedFriendlyName(stage.StageType.ToString).ToUpper(),
                    stage.Name,
                    stage.Process.Name,
                    If(stage.SubSheet.Name = "Initialise" OrElse stage.SubSheet.Name = "Clean Up", LocaleTools.LTools.GetC(stage.SubSheet.Name, "misc", "page"), stage.SubSheet.Name),
                    msg,
                    vbCrLf)
        End Select
        m.LogInhibit = False
        If stage.LogInhibit = LogInfo.InhibitModes.Always OrElse
         (stage.LogInhibit = LogInfo.InhibitModes.OnSuccess AndAlso msg = "") Then
            m.LogInhibit = True
        End If

        AddToQueue(m)
        WriteTextBoxMessage(m)
    End Sub

    ''' <summary>
    ''' Writes a message to the textbox.
    ''' </summary>
    ''' <param name="msg">The message to write</param>
    Private Overloads Sub WriteTextBoxMessage(ByVal msg As Message)
        If mTextBox.IsHandleCreated Then
            If Not mTextBox.Multiline Then mTextBox.Clear()

            If msg.LogInhibit _
             Then mTextBox.AppendText(msg.Text, Color.Gray) _
             Else mTextBox.AppendText(msg.Text)

            If mTextBox.Multiline Then mTextBox.ScrollToCaret()

        End If
    End Sub

    ''' <summary>
    ''' Adds a message to the queue, the queue will never exceed MaximumMessages
    ''' </summary>
    ''' <param name="msg">The message to queue</param>
    Private Sub AddToQueue(ByVal msg As Message)
        If mMessages.Count >= MaximumMessages Then mMessages.Dequeue()
        mMessages.Enqueue(msg)
    End Sub

    ''' <summary>
    ''' Shows the messages in the queue
    ''' </summary>
    Private Sub ShowQueueMessages()
        mTextBox.Clear()
        For Each msg As Message In mMessages : WriteTextBoxMessage(msg) : Next
        mTextBox.ScrollToCaret()
    End Sub

    Public Overrides Sub ActionPrologue(ByVal info As LogInfo, ByVal stage As clsActionStage, ByVal sObjectName As String, ByVal sActionName As String, ByVal inputs As clsArgumentList)
        LogStage(stage)
    End Sub

    Public Overrides Sub ActionEpilogue(ByVal info As LogInfo, ByVal stage As clsActionStage, ByVal sObjectName As String, ByVal sActionName As String, ByVal outputs As clsArgumentList)
        LogStage(stage)
    End Sub

    Public Overrides Sub AlertPrologue(info As LogInfo, stage As Stages.clsAlertStage)
    End Sub

    Public Overrides Sub AlertEpilogue(ByVal info As LogInfo, ByVal stage As clsAlertStage, ByVal sResult As String)
        LogStage(stage)
    End Sub

    Public Overrides Sub CalculationPrologue(ByVal info As LogInfo, ByVal stage As Stages.clsCalculationStage)
    End Sub

    Public Overrides Sub CalculationEpilogue(ByVal info As LogInfo, ByVal stage As clsCalculationStage, ByVal objResult As BluePrism.AutomateProcessCore.clsProcessValue)
        LogStage(stage)
    End Sub

    Public Overrides Sub ChoicePrologue(ByVal info As LogInfo, ByVal stage As clsChoiceStartStage)
    End Sub

    Public Overrides Sub ChoiceEpilogue(ByVal info As LogInfo, ByVal stage As clsChoiceStartStage, ByVal ChoiceName As String, ByVal ChoiceNumber As Integer)
        LogStage(stage)
    End Sub

    Public Overrides Sub CodePrologue(ByVal info As LogInfo, ByVal stage As clsCodeStage, ByVal inputs As clsArgumentList)
    End Sub

    Public Overrides Sub CodeEpilogue(ByVal info As LogInfo, ByVal stage As clsCodeStage, ByVal inputs As clsArgumentList, ByVal outputs As clsArgumentList)
        LogStage(stage)
    End Sub

    Public Overrides Sub DecisionPrologue(ByVal info As LogInfo, ByVal stage As clsDecisionStage)
    End Sub

    Public Overrides Sub DecisionEpilogue(ByVal info As LogInfo, ByVal stage As clsDecisionStage, ByVal sResult As String)
        LogStage(stage)
    End Sub

    Public Overrides Sub LogDiagnostic(ByVal info As LogInfo, ByVal stage As BluePrism.AutomateProcessCore.clsProcessStage, ByVal sMessage As String)
        LogStage(stage)
    End Sub

    Public Overrides Sub LogExceptionScreenshot(info As LogInfo, stage As clsProcessStage, processName As String, image As clsPixRect, timestamp As DateTimeOffset)
        If Context.ScreenshotAllowed AndAlso Context.Debugging Then
            LogStage(stage, My.Resources.ScreenCaptureNotSavedDueToRunningInDebugMode)
        End If
    End Sub

    Public Overrides Sub ObjectEpilogue(ByVal info As LogInfo, ByVal stage As clsEndStage, ByVal sObjectName As String, ByVal sActionName As String, ByVal outputs As clsArgumentList)
        LogStage(stage)
    End Sub

    Public Overrides Sub ProcessEpilogue(ByVal info As LogInfo, ByVal stage As clsEndStage, ByVal outputs As clsArgumentList)
        LogStage(stage)
    End Sub

    Public Overrides Sub SubSheetEpilogue(ByVal info As LogInfo, ByVal stage As clsEndStage, ByVal outputs As clsArgumentList)
        LogStage(stage)
    End Sub

    Public Overrides Sub LogError(ByVal info As LogInfo, ByVal stage As clsProcessStage, ByVal sErrorMessage As String)
        LogStage(stage, My.Resources.LogError & sErrorMessage)
    End Sub

    Public Overrides Sub LoopEndPrologue(ByVal info As LogInfo, ByVal stage As clsLoopEndStage)
    End Sub

    Public Overrides Sub LoopEndEpilogue(ByVal info As LogInfo, ByVal stage As clsLoopEndStage, ByVal sIteration As String)
        LogStage(stage)
    End Sub

    Public Overrides Sub LoopStartPrologue(ByVal info As LogInfo, ByVal stage As clsLoopStartStage)
    End Sub
    Public Overrides Sub LoopStartEpilogue(ByVal info As LogInfo, ByVal stage As clsLoopStartStage, ByVal sCount As String)
        LogStage(stage)
    End Sub

    Public Overrides Sub NavigatePrologue(ByVal info As LogInfo, ByVal stage As clsNavigateStage)
    End Sub

    Public Overrides Sub NavigateEpilogue(ByVal info As LogInfo, ByVal stage As clsNavigateStage)
        LogStage(stage)
    End Sub

    Public Overrides Sub NotePrologue(ByVal info As LogInfo, ByVal stage As clsNoteStage)
        LogStage(stage)
    End Sub

    Public Overrides Sub NoteEpilogue(ByVal info As LogInfo, ByVal stage As clsNoteStage)
    End Sub

    Public Overrides Sub ReadPrologue(ByVal info As LogInfo, ByVal stage As clsReadStage)
    End Sub

    Public Overrides Sub ReadEpilogue(ByVal info As LogInfo, ByVal stage As clsReadStage, ByVal outputs As clsArgumentList)
        LogStage(stage)
    End Sub

    Public Overrides Sub ObjectPrologue(ByVal info As LogInfo, ByVal stage As clsStartStage, ByVal sObjectName As String, ByVal sActionName As String, ByVal inputs As clsArgumentList)
        LogStage(stage)
    End Sub

    Public Overrides Sub ProcessPrologue(ByVal info As LogInfo, ByVal stage As clsStartStage, ByVal inputs As clsArgumentList)
        LogStage(stage)
    End Sub

    Public Overrides Sub SubSheetPrologue(ByVal info As LogInfo, ByVal stage As clsStartStage, ByVal inputs As clsArgumentList)
        LogStage(stage)
    End Sub

    Public Overrides Sub ProcessRefPrologue(ByVal info As LogInfo, ByVal stage As clsSubProcessRefStage, ByVal inputs As clsArgumentList)
        LogStage(stage)
    End Sub

    Public Overrides Sub ProcessRefEpilogue(ByVal info As LogInfo, ByVal stage As clsSubProcessRefStage, ByVal outputs As clsArgumentList)
        LogStage(stage)
    End Sub

    Public Overrides Sub SkillPrologue(info As LogInfo, stage As clsSkillStage, objectName As String, skillActionName As String, inputs As clsArgumentList)
        LogStage(stage)
    End Sub

    Public Overrides Sub SkillEpilogue(info As LogInfo, stage As clsSkillStage, objectName As String, skillActionName As String, outputs As clsArgumentList)
        LogStage(stage)
    End Sub

    Public Overrides Sub SubSheetRefPrologue(ByVal info As LogInfo, ByVal stage As clsSubSheetRefStage, ByVal inputs As clsArgumentList)
        LogStage(stage)
    End Sub

    Public Overrides Sub WaitPrologue(ByVal info As LogInfo, ByVal stage As clsWaitStartStage)
    End Sub

    Public Overrides Sub WaitEpilogue(ByVal info As LogInfo, ByVal stage As clsWaitStartStage, ByVal ChoiceName As String, ByVal ChoiceNumber As Integer)
        LogStage(stage)
    End Sub

    Public Overrides Sub WritePrologue(ByVal info As LogInfo, ByVal stage As clsWriteStage)
    End Sub

    Public Overrides Sub WriteEpilogue(ByVal info As LogInfo, ByVal stage As clsWriteStage, ByVal inputs As clsArgumentList)
        LogStage(stage)
    End Sub

    Public Overrides Sub ExceptionPrologue(ByVal info As LogInfo, ByVal stage As clsExceptionStage)
    End Sub

    Public Overrides Sub ExceptionEpilogue(ByVal info As LogInfo, ByVal stage As clsExceptionStage)
    End Sub

    Public Overrides Sub RecoverPrologue(ByVal info As LogInfo, ByVal stage As clsRecoverStage)
    End Sub

    Public Overrides Sub RecoverEpilogue(ByVal info As LogInfo, ByVal stage As clsRecoverStage)
        LogStage(stage)
    End Sub

    Public Overrides Sub ResumePrologue(ByVal info As LogInfo, ByVal stage As clsResumeStage)
    End Sub

    Public Overrides Sub ResumeEpilogue(ByVal info As LogInfo, ByVal stage As clsResumeStage)
        LogStage(stage)
    End Sub

    Public Overrides Sub LogStatistic(ByVal gSessionId As System.Guid, ByVal sName As String, ByVal objValue As BluePrism.AutomateProcessCore.clsProcessValue)
        Return
    End Sub

    Public Overrides Sub SubSheetRefEpilogue(ByVal info As LogInfo, ByVal stage As clsSubSheetRefStage, ByVal outputs As clsArgumentList)
        LogStage(stage)
    End Sub

    Public Overrides Sub MultipleCalculationPrologue(ByVal info As LogInfo, ByVal stage As Stages.clsMultipleCalculationStage)
    End Sub

    Public Overrides Sub MultipleCalculationEpilogue(ByVal info As LogInfo, ByVal stage As clsMultipleCalculationStage, ByVal objResults As clsArgumentList)
        LogStage(stage)
    End Sub
End Class
