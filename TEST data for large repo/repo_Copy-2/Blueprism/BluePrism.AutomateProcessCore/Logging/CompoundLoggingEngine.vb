Imports System.Linq
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.BPCoreLib

Public Class CompoundLoggingEngine
    Inherits clsLoggingEngine
    Implements ICollection(Of clsLoggingEngine)

    Private mEngines As IBPSet(Of clsLoggingEngine)

    Public Sub New()
        Me.New(EmptyLogContext.Empty)
    End Sub

    Public Sub New(ctx As IProcessLogContext)
        MyBase.New(ctx)
        mEngines = New clsOrderedSet(Of clsLoggingEngine)
    End Sub

    Public ReadOnly Property Engines As IEnumerable(Of clsLoggingEngine)
        Get
            Return mEngines.AsEnumerable()
        End Get
    End Property

    Public Sub Union(other As CompoundLoggingEngine)
        mEngines.Union(other.mEngines)
    End Sub

    Private Sub CallLog(log As Action(Of clsLoggingEngine))
        For Each e In Engines : log(e) : Next
    End Sub

    Public Overrides Sub ActionPrologue(info As LogInfo, stg As clsActionStage, objectName As String, actionName As String, inputs As clsArgumentList)
        CallLog(Sub(e) e.ActionPrologue(info, stg, objectName, actionName, inputs))
    End Sub

    Public Overrides Sub ActionEpiLogue(info As LogInfo, stg As clsActionStage, objectName As String, actionName As String, outputs As clsArgumentList)
        CallLog(Sub(e) e.ActionEpilogue(info, stg, objectName, actionName, outputs))
    End Sub

    Public Overrides Sub AlertEpiLogue(info As LogInfo, stg As clsAlertStage, result As String)
        CallLog(Sub(e) e.AlertEpilogue(info, stg, result))
    End Sub

    Public Overrides Sub CalculationEpiLogue(info As LogInfo, stg As clsCalculationStage, resultVal As clsProcessValue)
        CallLog(Sub(e) e.CalculationEpilogue(info, stg, resultVal))
    End Sub

    Public Overrides Sub ChoiceEpiLogue(info As LogInfo, stg As clsChoiceStartStage, choiceName As String, choiceNo As Integer)
        CallLog(Sub(e) e.ChoiceEpilogue(info, stg, choiceName, choiceNo))
    End Sub

    Public Overrides Sub CodeEpiLogue(info As LogInfo, stg As clsCodeStage, inputs As clsArgumentList, outputs As clsArgumentList)
        CallLog(Sub(e) e.CodeEpilogue(info, stg, inputs, outputs))
    End Sub

    Public Overrides Sub DecisionEpiLogue(info As LogInfo, stg As clsDecisionStage, result As String)
        CallLog(Sub(e) e.DecisionEpilogue(info, stg, result))
    End Sub

    Public Overrides Sub LogDiagnostic(info As LogInfo, stg As clsProcessStage, msg As String)
        CallLog(Sub(e) e.LogDiagnostic(info, stg, msg))
    End Sub

    Public Overrides Sub LogExceptionScreenshot(info As LogInfo, stage As clsProcessStage, processName As String, image As clsPixRect, timestamp As DateTimeOffset)
        CallLog(Sub(e) e.LogExceptionScreenshot(info, stage, processName, image, timestamp))
    End Sub

    Public Overrides Sub ObjectEpiLogue(info As LogInfo, stg As clsEndStage, objectName As String, actionName As String, outputs As clsArgumentList)
        CallLog(Sub(e) e.ObjectEpilogue(info, stg, objectName, actionName, outputs))
    End Sub

    Public Overrides Sub ProcessEpiLogue(info As LogInfo, stg As clsEndStage, outputs As clsArgumentList)
        CallLog(Sub(e) e.ProcessEpilogue(info, stg, outputs))
    End Sub

    Public Overrides Sub SubSheetEpiLogue(info As LogInfo, stg As clsEndStage, outputs As clsArgumentList)
        CallLog(Sub(e) e.SubSheetEpilogue(info, stg, outputs))
    End Sub

    Public Overrides Sub LogError(info As LogInfo, stg As clsProcessStage, errMessage As String)
        CallLog(Sub(e) e.LogError(info, stg, errMessage))
    End Sub

    Public Overrides Sub LoopEndEpiLogue(info As LogInfo, stg As clsLoopEndStage, iter As String)
        CallLog(Sub(e) e.LoopEndEpilogue(info, stg, iter))
    End Sub

    Public Overrides Sub LoopStartEpiLogue(info As LogInfo, stg As clsLoopStartStage, count As String)
        CallLog(Sub(e) e.LoopStartEpilogue(info, stg, count))
    End Sub

    Public Overrides Sub MultipleCalculationEpiLogue(info As LogInfo, stg As clsMultipleCalculationStage, results As clsArgumentList)
        CallLog(Sub(e) e.MultipleCalculationEpilogue(info, stg, results))
    End Sub

    Public Overrides Sub NavigateEpiLogue(info As LogInfo, stg As clsNavigateStage)
        CallLog(Sub(e) e.NavigateEpilogue(info, stg))
    End Sub

    Public Overrides Sub NoteEpiLogue(info As LogInfo, stg As clsNoteStage)
        CallLog(Sub(e) e.NoteEpilogue(info, stg))
    End Sub

    Public Overrides Sub ReadEpiLogue(info As LogInfo, stg As clsReadStage, outputs As clsArgumentList)
        CallLog(Sub(e) e.ReadEpilogue(info, stg, outputs))
    End Sub

    Public Overrides Sub RecoverEpiLogue(info As LogInfo, stg As clsRecoverStage)
        CallLog(Sub(e) e.RecoverEpilogue(info, stg))
    End Sub

    Public Overrides Sub ResumeEpiLogue(info As LogInfo, stg As clsResumeStage)
        CallLog(Sub(e) e.ResumeEpilogue(info, stg))
    End Sub

    Public Overrides Sub ObjectPrologue(info As LogInfo, stg As clsStartStage, objectName As String, actionName As String, inputs As clsArgumentList)
        CallLog(Sub(e) e.ObjectPrologue(info, stg, objectName, actionName, inputs))
    End Sub

    Public Overrides Sub ProcessPrologue(info As LogInfo, stg As clsStartStage, inputs As clsArgumentList)
        CallLog(Sub(e) e.ProcessPrologue(info, stg, inputs))
    End Sub

    Public Overrides Sub SubSheetPrologue(info As LogInfo, stg As clsStartStage, inputs As clsArgumentList)
        CallLog(Sub(e) e.SubSheetPrologue(info, stg, inputs))
    End Sub

    Public Overrides Sub ProcessRefProLogue(info As LogInfo, stg As clsSubProcessRefStage, inputs As clsArgumentList)
        CallLog(Sub(e) e.ProcessRefPrologue(info, stg, inputs))
    End Sub

    Public Overrides Sub ProcessRefEpiLogue(info As LogInfo, stg As clsSubProcessRefStage, outputs As clsArgumentList)
        CallLog(Sub(e) e.ProcessRefEpilogue(info, stg, outputs))
    End Sub

    Public Overrides Sub SubSheetRefProLogue(info As LogInfo, stg As clsSubSheetRefStage, inputs As clsArgumentList)
        CallLog(Sub(e) e.SubSheetRefPrologue(info, stg, inputs))
    End Sub

    Public Overrides Sub SubSheetRefEpiLogue(info As LogInfo, stg As clsSubSheetRefStage, outputs As clsArgumentList)
        CallLog(Sub(e) e.SubSheetRefEpilogue(info, stg, outputs))
    End Sub

    Public Overrides Sub WaitEpiLogue(info As LogInfo, stg As clsWaitStartStage, choiceName As String, choiceNo As Integer)
        CallLog(Sub(e) e.WaitEpilogue(info, stg, choiceName, choiceNo))
    End Sub

    Public Overrides Sub WriteEpiLogue(info As LogInfo, stg As clsWriteStage, inputs As clsArgumentList)
        CallLog(Sub(e) e.WriteEpilogue(info, stg, inputs))
    End Sub

    Public Overrides Sub LogStatistic(sessId As Guid, statName As String, val As clsProcessValue)
        CallLog(Sub(e) e.LogStatistic(sessId, statName, val))
    End Sub

    Public Overrides Sub AlertPrologue(info As LogInfo, stg As clsAlertStage)
        CallLog(Sub(e) e.AlertPrologue(info, stg))
    End Sub

    Public Overrides Sub CalculationPrologue(info As LogInfo, stg As clsCalculationStage)
        CallLog(Sub(e) e.CalculationPrologue(info, stg))
    End Sub

    Public Overrides Sub ChoicePrologue(info As LogInfo, stg As clsChoiceStartStage)
        CallLog(Sub(e) e.ChoicePrologue(info, stg))
    End Sub

    Public Overrides Sub CodePrologue(info As LogInfo, stg As clsCodeStage, inputs As clsArgumentList)
        CallLog(Sub(e) e.CodePrologue(info, stg, inputs))
    End Sub

    Public Overrides Sub DecisionPrologue(info As LogInfo, stg As clsDecisionStage)
        CallLog(Sub(e) e.DecisionPrologue(info, stg))
    End Sub

    Public Overrides Sub ExceptionEpilogue(info As LogInfo, stg As clsExceptionStage)
        CallLog(Sub(e) e.ExceptionEpilogue(info, stg))
    End Sub

    Public Overrides Sub ExceptionPrologue(info As LogInfo, stg As clsExceptionStage)
        CallLog(Sub(e) e.ExceptionPrologue(info, stg))
    End Sub

    Public Overrides Sub LoopEndPrologue(info As LogInfo, stg As clsLoopEndStage)
        CallLog(Sub(e) e.LoopEndPrologue(info, stg))
    End Sub

    Public Overrides Sub LoopStartPrologue(info As LogInfo, stg As clsLoopStartStage)
        CallLog(Sub(e) e.LoopStartPrologue(info, stg))
    End Sub

    Public Overrides Sub MultipleCalculationPrologue(info As LogInfo, stg As clsMultipleCalculationStage)
        CallLog(Sub(e) e.MultipleCalculationPrologue(info, stg))
    End Sub

    Public Overrides Sub NavigatePrologue(info As LogInfo, stg As clsNavigateStage)
        CallLog(Sub(e) e.NavigatePrologue(info, stg))
    End Sub

    Public Overrides Sub NotePrologue(info As LogInfo, stg As clsNoteStage)
        CallLog(Sub(e) e.NotePrologue(info, stg))
    End Sub

    Public Overrides Sub ReadPrologue(info As LogInfo, stg As clsReadStage)
        CallLog(Sub(e) e.ReadPrologue(info, stg))
    End Sub

    Public Overrides Sub RecoverPrologue(info As LogInfo, stg As clsRecoverStage)
        CallLog(Sub(e) e.RecoverPrologue(info, stg))
    End Sub

    Public Overrides Sub ResumePrologue(info As LogInfo, stg As clsResumeStage)
        CallLog(Sub(e) e.ResumePrologue(info, stg))
    End Sub

    Public Overrides Sub WaitPrologue(info As LogInfo, stg As clsWaitStartStage)
        CallLog(Sub(e) e.WaitPrologue(info, stg))
    End Sub

    Public Overrides Sub WritePrologue(info As LogInfo, stg As clsWriteStage)
        CallLog(Sub(e) e.WritePrologue(info, stg))
    End Sub

    Public Overrides Sub SkillPrologue(info As LogInfo, stg As clsSkillStage, objectName As String, skillActionName As String, inputs As clsArgumentList)
        CallLog(Sub(e) e.SkillPrologue(info, stg, objectName, skillActionName, inputs))
    End Sub

    Public Overrides Sub SkillEpilogue(info As LogInfo, stg As clsSkillStage, objectName As String, skillActionName As String, outputs As clsArgumentList)
        CallLog(Sub(e) e.SkillEpilogue(info, stg, objectName, skillActionName, outputs))
    End Sub

    Public Overrides Sub SetEnvironmentVariable(info As LogInfo, loggableMessage As String)
        CallLog(Sub(e) e.SetEnvironmentVariable(info, loggableMessage))
    End Sub
    Public Overrides Sub CreateSessionLog(info As LogInfo)
        CallLog(Sub(e) e.CreateSessionLog(info))
    End Sub
    Public Overrides Sub ImmediateStop(info As LogInfo, stopReason As String)
        CallLog(Sub(e) e.ImmediateStop(info, stopReason))
    End Sub
    Public Overrides Sub UnexpectedException(info As LogInfo)
        CallLog(Sub(e) e.UnexpectedException(info))
    End Sub

    Public Sub Add(item As clsLoggingEngine) Implements ICollection(Of clsLoggingEngine).Add
        If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
        mEngines.Add(item)
    End Sub

    Public Sub Clear() Implements ICollection(Of clsLoggingEngine).Clear
        mEngines.Clear()
    End Sub

    Public Function Contains(item As clsLoggingEngine) As Boolean Implements ICollection(Of clsLoggingEngine).Contains
        Return mEngines.Contains(item)
    End Function

    Public Sub CopyTo(array() As clsLoggingEngine, arrayIndex As Integer) Implements ICollection(Of clsLoggingEngine).CopyTo
        mEngines.CopyTo(array, arrayIndex)
    End Sub

    Public ReadOnly Property Count As Integer Implements ICollection(Of clsLoggingEngine).Count
        Get
            Return mEngines.Count
        End Get
    End Property

    Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of clsLoggingEngine).IsReadOnly
        Get
            Return mEngines.IsReadOnly
        End Get
    End Property

    Public Function Remove(item As clsLoggingEngine) As Boolean Implements ICollection(Of clsLoggingEngine).Remove
        If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
        Return mEngines.Remove(item)
    End Function

    Public Function GetEnumerator() As IEnumerator(Of clsLoggingEngine) Implements IEnumerable(Of clsLoggingEngine).GetEnumerator
        Return mEngines.GetEnumerator()
    End Function

    Private Function GetBasicEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then For Each e In Engines : e.Dispose() : Next
        MyBase.Dispose(disposing)
    End Sub
End Class
