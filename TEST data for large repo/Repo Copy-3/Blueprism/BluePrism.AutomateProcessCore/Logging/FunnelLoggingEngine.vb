Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.BPCoreLib

Public MustInherit Class FunnelLoggingEngine : Inherits clsLoggingEngine


    Sub New(ctx As ILogContext)
        MyBase.New(ctx)
    End Sub

    Protected MustOverride Sub Log(info As LogInfo, stg As clsProcessStage, eventName As String)

    Public Overrides Sub ActionEpilogue(info As LogInfo, stg As clsActionStage, objectName As String, actionName As String, outputs As clsArgumentList)
        Log(info, stg, "endAction")
    End Sub

    Public Overrides Sub ActionPrologue(info As LogInfo, stg As clsActionStage, objectName As String, actionName As String, inputs As clsArgumentList)
        Log(info, stg, "startAction")
    End Sub

    Public Overrides Sub AlertEpilogue(info As LogInfo, stg As clsAlertStage, result As String)
        Log(info, stg, "endAlert")
    End Sub

    Public Overrides Sub AlertPrologue(info As LogInfo, stg As clsAlertStage)
        Log(info, stg, "startAlert")
    End Sub

    Public Overrides Sub CalculationEpilogue(info As LogInfo, stg As clsCalculationStage, resultVal As clsProcessValue)
        Log(info, stg, "endCalc")
    End Sub

    Public Overrides Sub CalculationPrologue(info As LogInfo, stg As clsCalculationStage)
        Log(info, stg, "startCalc")
    End Sub

    Public Overrides Sub ChoiceEpilogue(info As LogInfo, stg As clsChoiceStartStage, choiceName As String, choiceNo As Integer)
        Log(info, stg, "endChoice")
    End Sub

    Public Overrides Sub ChoicePrologue(info As LogInfo, stg As clsChoiceStartStage)
        Log(info, stg, "startChoice")
    End Sub

    Public Overrides Sub CodeEpilogue(info As LogInfo, stg As clsCodeStage, inputs As clsArgumentList, outputs As clsArgumentList)
        Log(info, stg, "endCode")
    End Sub

    Public Overrides Sub CodePrologue(info As LogInfo, stg As clsCodeStage, inputs As clsArgumentList)
        Log(info, stg, "startCode")
    End Sub

    Public Overrides Sub DecisionEpilogue(info As LogInfo, stg As clsDecisionStage, result As String)
        Log(info, stg, "endDecision")
    End Sub

    Public Overrides Sub DecisionPrologue(info As LogInfo, stg As clsDecisionStage)
        Log(info, stg, "startDecision")
    End Sub

    Public Overrides Sub ExceptionEpilogue(info As LogInfo, stg As clsExceptionStage)
        Log(info, stg, "endException")
    End Sub

    Public Overrides Sub ExceptionPrologue(info As LogInfo, stg As clsExceptionStage)
        Log(info, stg, "startException")
    End Sub

    Public Overrides Sub LogDiagnostic(info As LogInfo, stg As clsProcessStage, msg As String)
        Log(info, stg, "diag")
    End Sub

    Public Overrides Sub LogError(info As LogInfo, stg As clsProcessStage, errMessage As String)
        Log(info, stg, "error")
    End Sub

    Public Overrides Sub LogExceptionScreenshot(info As LogInfo, stg As clsProcessStage, processName As String, image As clsPixRect, timestamp As DateTimeOffset)
        Log(info, stg, "exceptionScreenshot")
    End Sub

    Public Overrides Sub LogStatistic(sessId As Guid, statName As String, val As clsProcessValue)
    End Sub

    Public Overrides Sub LoopEndEpilogue(info As LogInfo, stg As clsLoopEndStage, iter As String)
        Log(info, stg, "endLoopEnd")
    End Sub

    Public Overrides Sub LoopEndPrologue(info As LogInfo, stg As clsLoopEndStage)
        Log(info, stg, "startLoopEnd")
    End Sub

    Public Overrides Sub LoopStartEpilogue(info As LogInfo, stg As clsLoopStartStage, count As String)
        Log(info, stg, "endLoopStart")
    End Sub

    Public Overrides Sub LoopStartPrologue(info As LogInfo, stg As clsLoopStartStage)
        Log(info, stg, "startLoopStart")
    End Sub

    Public Overrides Sub MultipleCalculationEpilogue(info As LogInfo, stg As clsMultipleCalculationStage, results As clsArgumentList)
        Log(info, stg, "endMultiCalc")
    End Sub

    Public Overrides Sub MultipleCalculationPrologue(info As LogInfo, stg As clsMultipleCalculationStage)
        Log(info, stg, "startMultiCalc")
    End Sub

    Public Overrides Sub NavigateEpilogue(info As LogInfo, stg As clsNavigateStage)
        Log(info, stg, "endNavigate")
    End Sub

    Public Overrides Sub NavigatePrologue(info As LogInfo, stg As clsNavigateStage)
        Log(info, stg, "startNavigate")
    End Sub

    Public Overrides Sub NoteEpilogue(info As LogInfo, stg As clsNoteStage)
        Log(info, stg, "endNote")
    End Sub

    Public Overrides Sub NotePrologue(info As LogInfo, stg As clsNoteStage)
        Log(info, stg, "startNote")
    End Sub

    Public Overrides Sub ObjectEpilogue(info As LogInfo, stg As clsEndStage, objectName As String, actionName As String, outputs As clsArgumentList)
        Log(info, stg, "endObject")
    End Sub

    Public Overrides Sub ObjectPrologue(info As LogInfo, stg As clsStartStage, objectName As String, actionName As String, inputs As clsArgumentList)
        Log(info, stg, "startObject")
    End Sub

    Public Overrides Sub ProcessEpilogue(info As LogInfo, stg As clsEndStage, outputs As clsArgumentList)
        Log(info, stg, "endProcess")
    End Sub

    Public Overrides Sub ProcessPrologue(info As LogInfo, stg As clsStartStage, inputs As clsArgumentList)
        Log(info, stg, "startProcess")
    End Sub

    Public Overrides Sub ProcessRefEpilogue(info As LogInfo, stg As clsSubProcessRefStage, outputs As clsArgumentList)
        Log(info, stg, "endProcessRef")
    End Sub

    Public Overrides Sub ProcessRefPrologue(info As LogInfo, stg As clsSubProcessRefStage, inputs As clsArgumentList)
        Log(info, stg, "startProcessRef")
    End Sub

    Public Overrides Sub ReadEpilogue(info As LogInfo, stg As clsReadStage, outputs As clsArgumentList)
        Log(info, stg, "endRead")
    End Sub

    Public Overrides Sub ReadPrologue(info As LogInfo, stg As clsReadStage)
        Log(info, stg, "startRead")
    End Sub

    Public Overrides Sub RecoverEpilogue(info As LogInfo, stg As clsRecoverStage)
        Log(info, stg, "endRecover")
    End Sub

    Public Overrides Sub RecoverPrologue(info As LogInfo, stg As clsRecoverStage)
        Log(info, stg, "startRecover")
    End Sub

    Public Overrides Sub ResumeEpilogue(info As LogInfo, stg As clsResumeStage)
        Log(info, stg, "endResume")
    End Sub

    Public Overrides Sub ResumePrologue(info As LogInfo, stg As clsResumeStage)
        Log(info, stg, "startResume")
    End Sub

    Public Overrides Sub SkillEpilogue(info As LogInfo, stg As clsSkillStage, objectName As String, skillActionName As String, outputs As clsArgumentList)
        Log(info, stg, "endSkill")
    End Sub

    Public Overrides Sub SkillPrologue(info As LogInfo, stg As clsSkillStage, objectName As String, skillActionName As String, inputs As clsArgumentList)
        Log(info, stg, "startSkill")
    End Sub

    Public Overrides Sub SubSheetEpilogue(info As LogInfo, stg As clsEndStage, outputs As clsArgumentList)
        Log(info, stg, "endSubSheet")
    End Sub

    Public Overrides Sub SubSheetPrologue(info As LogInfo, stg As clsStartStage, inputs As clsArgumentList)
        Log(info, stg, "startSubSheet")
    End Sub

    Public Overrides Sub SubSheetRefEpilogue(info As LogInfo, stg As clsSubSheetRefStage, outputs As clsArgumentList)
        Log(info, stg, "endSubSheetRef")
    End Sub

    Public Overrides Sub SubSheetRefPrologue(info As LogInfo, stg As clsSubSheetRefStage, inputs As clsArgumentList)
        Log(info, stg, "startSubSheetRef")
    End Sub

    Public Overrides Sub WaitEpilogue(info As LogInfo, stg As clsWaitStartStage, choiceName As String, choiceNo As Integer)
        Log(info, stg, "endWait")
    End Sub

    Public Overrides Sub WaitPrologue(info As LogInfo, stg As clsWaitStartStage)
        Log(info, stg, "startWait")
    End Sub

    Public Overrides Sub WriteEpilogue(info As LogInfo, stg As clsWriteStage, inputs As clsArgumentList)
        Log(info, stg, "endWrite")
    End Sub

    Public Overrides Sub WritePrologue(info As LogInfo, stg As clsWriteStage)
        Log(info, stg, "startWrite")
    End Sub
End Class
