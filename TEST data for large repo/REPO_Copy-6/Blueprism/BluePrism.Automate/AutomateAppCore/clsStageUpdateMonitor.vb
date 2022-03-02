Imports System.Threading
Imports System.Threading.Tasks
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports NLog

Public Class clsStageUpdateMonitor : Inherits clsLoggingEngine

    Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()

    ''' <summary>
    ''' The current stage setting to nothing
    ''' indicates no more stage updates.
    ''' </summary>
    Private mStage As clsProcessStage

    ''' <summary>
    ''' When the stage was updated
    ''' </summary>
    Private mUpdated As DateTimeOffset

    ''' <summary>
    ''' The system wide default stage warning threshold
    ''' </summary>
    Private mDefaultWarningThreshold As Integer

    ''' <summary>
    ''' Constructs a new stage update monitor and starts a thread which will
    ''' update the database every update interval
    ''' </summary>
    ''' <param name="ctx">The logging context to use for this log engine</param>
    Friend Sub New(ctx As LogContext)
        MyBase.New(ctx)
        If ctx.Process Is Nothing Then Throw New ArgumentException(NameOf(ctx),
         "The Property 'Process' within the logging context, cannot be null")

        ' Get the refresh interval for this update
        Dim refreshFreq = TimeSpan.FromSeconds(gSv.GetRuntimeResourceRefreshFrequency())

        ' Get the default stage warning threshold
        mDefaultWarningThreshold = gSv.GetStageWarningThreshold()

        Update(Context.Process.StartStage)

        Task.Run(
            Sub()
                While mStage IsNot Nothing
                    Try
                        gSv.RefreshSessionInfo(Context.SessionNo, mStage.Name, mUpdated,
                            If(mStage.OverrideDefaultWarningThreshold, mStage.WarningThreshold * 60, mDefaultWarningThreshold))
                    Catch ex As Exception
                        Dim serverFailureMessage = $"Unable to update sessions in the database for session {Context.SessionId} on stage {mStage.Name}"
                        Log.Warn(serverFailureMessage)
                    End Try
                    Thread.Sleep(refreshFreq)
                End While
            End Sub)
    End Sub

    ''' <summary>
    ''' Disposes the monitor ensuring that mStage is set to nothing which will
    ''' let the thread exit cleanly
    ''' </summary>
    Protected Overrides Sub Dispose(disposing As Boolean)
        mStage = Nothing
        MyBase.Dispose(disposing)
    End Sub

    ''' <summary>
    ''' Updates the database with the latest stage that has run.
    ''' </summary>
    ''' <param name="stage">The stage</param>
    Private Sub Update(stage As clsProcessStage)
        mStage = stage
        mUpdated = DateTimeOffset.Now
    End Sub

#Region " LogEngine Implementation"

    Public Overrides Sub ActionEpilogue(info As LogInfo, stage As clsActionStage, sObjectName As String, sActionName As String, outputs As clsArgumentList)
    End Sub

    Public Overrides Sub ActionPrologue(info As LogInfo, stage As clsActionStage, sObjectName As String, sActionName As String, inputs As clsArgumentList)
        Update(stage)
    End Sub

    Public Overrides Sub AlertEpilogue(info As LogInfo, stage As clsAlertStage, sResult As String)
    End Sub

    Public Overrides Sub AlertPrologue(info As LogInfo, stage As clsAlertStage)
        Update(stage)
    End Sub

    Public Overrides Sub CalculationEpilogue(info As LogInfo, stage As clsCalculationStage, objResult As clsProcessValue)
    End Sub

    Public Overrides Sub CalculationPrologue(info As LogInfo, stage As clsCalculationStage)
        Update(stage)
    End Sub

    Public Overrides Sub ChoiceEpilogue(info As LogInfo, stage As clsChoiceStartStage, ChoiceName As String, ChoiceNumber As Integer)
    End Sub

    Public Overrides Sub ChoicePrologue(info As LogInfo, stage As clsChoiceStartStage)
        Update(stage)
    End Sub

    Public Overrides Sub CodeEpilogue(info As LogInfo, stage As clsCodeStage, inputs As clsArgumentList, outputs As clsArgumentList)
    End Sub

    Public Overrides Sub CodePrologue(info As LogInfo, stage As clsCodeStage, inputs As clsArgumentList)
        Update(stage)
    End Sub

    Public Overrides Sub DecisionEpilogue(info As LogInfo, stage As clsDecisionStage, sResult As String)
    End Sub

    Public Overrides Sub DecisionPrologue(info As LogInfo, stage As clsDecisionStage)
        Update(stage)
    End Sub

    Public Overrides Sub LogDiagnostic(info As LogInfo, stage As clsProcessStage, sMessage As String)
    End Sub

    Public Overrides Sub LogError(info As LogInfo, stage As clsProcessStage, sErrorMessage As String)
        Update(stage)
    End Sub

    Public Overrides Sub LogExceptionScreenshot(info As LogInfo, stage As clsProcessStage, processName As String, image As BPCoreLib.clsPixRect, timestamp As DateTimeOffset)
    End Sub

    Public Overrides Sub LogStatistic(gSessionId As Guid, sName As String, objValue As clsProcessValue)
    End Sub

    Public Overrides Sub LoopEndEpilogue(info As LogInfo, stage As clsLoopEndStage, sIteration As String)
    End Sub

    Public Overrides Sub LoopEndPrologue(info As LogInfo, stage As clsLoopEndStage)
        Update(stage)
    End Sub

    Public Overrides Sub LoopStartEpilogue(info As LogInfo, stage As clsLoopStartStage, sCount As String)
    End Sub

    Public Overrides Sub LoopStartPrologue(info As LogInfo, stage As clsLoopStartStage)
        Update(stage)
    End Sub

    Public Overrides Sub MultipleCalculationEpilogue(info As LogInfo, stage As clsMultipleCalculationStage, objResults As clsArgumentList)
    End Sub

    Public Overrides Sub MultipleCalculationPrologue(info As LogInfo, stage As clsMultipleCalculationStage)
        Update(stage)
    End Sub

    Public Overrides Sub NavigateEpilogue(info As LogInfo, stage As clsNavigateStage)
    End Sub

    Public Overrides Sub NavigatePrologue(info As LogInfo, stage As clsNavigateStage)
        Update(stage)
    End Sub

    Public Overrides Sub NoteEpilogue(info As LogInfo, stage As clsNoteStage)
    End Sub

    Public Overrides Sub NotePrologue(info As LogInfo, stage As clsNoteStage)
        Update(stage)
    End Sub

    Public Overrides Sub ObjectEpilogue(info As LogInfo, stage As clsEndStage, sObjectName As String, sActionName As String, outputs As clsArgumentList)
    End Sub

    Public Overrides Sub ObjectPrologue(info As LogInfo, stage As clsStartStage, sObjectName As String, sActionName As String, inputs As clsArgumentList)
        Update(stage)
    End Sub

    Public Overrides Sub ProcessEpilogue(info As LogInfo, stage As clsEndStage, outputs As clsArgumentList)
    End Sub

    Public Overrides Sub ProcessPrologue(info As LogInfo, stage As clsStartStage, inputs As clsArgumentList)
        Update(stage)
    End Sub

    Public Overrides Sub ProcessRefEpilogue(info As LogInfo, stage As clsSubProcessRefStage, outputs As clsArgumentList)
    End Sub

    Public Overrides Sub ProcessRefPrologue(info As LogInfo, stage As clsSubProcessRefStage, inputs As clsArgumentList)
        Update(stage)
    End Sub

    Public Overrides Sub ReadEpilogue(info As LogInfo, stage As clsReadStage, outputs As clsArgumentList)
    End Sub

    Public Overrides Sub ReadPrologue(info As LogInfo, stage As clsReadStage)
        Update(stage)
    End Sub

    Public Overrides Sub ExceptionEpilogue(ByVal info As LogInfo, ByVal stage As clsExceptionStage)
    End Sub

    Public Overrides Sub ExceptionPrologue(ByVal info As LogInfo, ByVal stage As clsExceptionStage)
        Update(stage)
    End Sub

    Public Overrides Sub RecoverEpilogue(info As LogInfo, stage As clsRecoverStage)
    End Sub

    Public Overrides Sub RecoverPrologue(info As LogInfo, stage As clsRecoverStage)
        Update(stage)
    End Sub

    Public Overrides Sub ResumeEpilogue(info As LogInfo, stage As clsResumeStage)
    End Sub

    Public Overrides Sub ResumePrologue(info As LogInfo, stage As clsResumeStage)
        Update(stage)
    End Sub

    Public Overrides Sub SkillEpilogue(info As LogInfo, stage As clsSkillStage, objectName As String, skillActionName As String, outputs As clsArgumentList)
    End Sub

    Public Overrides Sub SkillPrologue(info As LogInfo, stage As clsSkillStage, objectName As String, skillActionName As String, inputs As clsArgumentList)
        Update(stage)
    End Sub

    Public Overrides Sub SubSheetEpilogue(info As LogInfo, stage As clsEndStage, outputs As clsArgumentList)
    End Sub

    Public Overrides Sub SubSheetPrologue(info As LogInfo, stage As clsStartStage, inputs As clsArgumentList)
        Update(stage)
    End Sub

    Public Overrides Sub SubSheetRefEpilogue(info As LogInfo, stage As clsSubSheetRefStage, outputs As clsArgumentList)
    End Sub

    Public Overrides Sub SubSheetRefPrologue(info As LogInfo, stage As clsSubSheetRefStage, inputs As clsArgumentList)
        Update(stage)
    End Sub

    Public Overrides Sub WaitEpilogue(info As LogInfo, stage As clsWaitStartStage, ChoiceName As String, ChoiceNumber As Integer)
    End Sub

    Public Overrides Sub WaitPrologue(info As LogInfo, stage As clsWaitStartStage)
        Update(stage)
    End Sub

    Public Overrides Sub WriteEpilogue(info As LogInfo, stage As clsWriteStage, inputs As clsArgumentList)
    End Sub

    Public Overrides Sub WritePrologue(info As LogInfo, stage As clsWriteStage)
        Update(stage)
    End Sub
#End Region

End Class
