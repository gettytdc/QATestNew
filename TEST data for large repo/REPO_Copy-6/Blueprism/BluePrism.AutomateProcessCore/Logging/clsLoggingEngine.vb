
Option Strict On

Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.BPCoreLib

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsLoggingEngine
''' 
''' <summary>
''' A base class defining the required interface for a logging engine. The
''' application should derive its own logging engine class from this one,
''' implementing the required functionality. The completed class can then be passed
''' to clsProcess, for example, and will be used to log activity.
''' </summary>
Public MustInherit Class clsLoggingEngine : Implements IDisposable

    Private mContext As IProcessLogContext

    ' To detect when this engine has been disposed of
    Private mIsDisposed As Boolean = False

    ''' <summary>
    ''' Creates a new logging engine
    ''' </summary>
    ''' <param name="ctx">The context for the new logging engine</param>
    ''' <exception cref="ArgumentNullException">If the given context was null.
    ''' </exception>
    Public Sub New(ctx As IProcessLogContext)
        If ctx Is Nothing Then Throw New ArgumentNullException(NameOf(ctx))
        mContext = ctx
        Dim proc As clsProcess = mContext.Process
        If proc IsNot Nothing Then proc.Logger.Add(Me)
    End Sub

    ''' <summary>
    ''' The context used to initialise this logging engine
    ''' </summary>
    Public ReadOnly Property Context() As ILogContext
        Get
            Return If(TypeOf mContext Is ILogContext, DirectCast(mContext, ILogContext), Nothing)
        End Get
    End Property

    ''' <summary>
    ''' Update a statistic
    ''' </summary>
    ''' <param name="sessId">The session ID</param>
    ''' <param name="statName">The name of the statistic</param>
    ''' <param name="val">The new value, as a clsProcessValue</param>
    Public MustOverride Sub LogStatistic(sessId As Guid, statName As String, val As clsProcessValue)
    Public MustOverride Sub LogExceptionScreenshot(info As LogInfo, stg As clsProcessStage, processName As String, image As clsPixRect, timestamp As DateTimeOffset)
    Public MustOverride Sub LogError(info As LogInfo, stg As clsProcessStage, errMessage As String)
    Public MustOverride Sub LogDiagnostic(info As LogInfo, stg As clsProcessStage, msg As String)

    Public MustOverride Sub ActionPrologue(info As LogInfo, stg As clsActionStage, objectName As String, actionName As String, inputs As clsArgumentList)
    Public MustOverride Sub ActionEpilogue(info As LogInfo, stg As clsActionStage, objectName As String, actionName As String, outputs As clsArgumentList)

    Public MustOverride Sub AlertPrologue(info As LogInfo, stg As clsAlertStage)
    Public MustOverride Sub AlertEpilogue(info As LogInfo, stg As clsAlertStage, result As String)

    Public MustOverride Sub CalculationPrologue(info As LogInfo, stg As clsCalculationStage)
    Public MustOverride Sub CalculationEpilogue(info As LogInfo, stg As clsCalculationStage, resultVal As clsProcessValue)

    Public MustOverride Sub MultipleCalculationPrologue(info As LogInfo, stg As clsMultipleCalculationStage)
    Public MustOverride Sub MultipleCalculationEpilogue(info As LogInfo, stg As clsMultipleCalculationStage, results As clsArgumentList)

    Public MustOverride Sub CodePrologue(info As LogInfo, stg As clsCodeStage, inputs As clsArgumentList)
    Public MustOverride Sub CodeEpilogue(info As LogInfo, stg As clsCodeStage, inputs As clsArgumentList, outputs As clsArgumentList)

    Public MustOverride Sub DecisionPrologue(info As LogInfo, stg As clsDecisionStage)
    Public MustOverride Sub DecisionEpilogue(info As LogInfo, stg As clsDecisionStage, result As String)

    Public MustOverride Sub LoopStartPrologue(info As LogInfo, stg As clsLoopStartStage)
    Public MustOverride Sub LoopStartEpilogue(info As LogInfo, stg As clsLoopStartStage, count As String)

    Public MustOverride Sub LoopEndPrologue(info As LogInfo, stg As clsLoopEndStage)
    Public MustOverride Sub LoopEndEpilogue(info As LogInfo, stg As clsLoopEndStage, iter As String)

    Public MustOverride Sub NotePrologue(info As LogInfo, stg As clsNoteStage)
    Public MustOverride Sub NoteEpilogue(info As LogInfo, stg As clsNoteStage)

    Public MustOverride Sub NavigatePrologue(info As LogInfo, stg As clsNavigateStage)
    Public MustOverride Sub NavigateEpilogue(info As LogInfo, stg As clsNavigateStage)

    Public MustOverride Sub ReadPrologue(info As LogInfo, stg As clsReadStage)
    Public MustOverride Sub ReadEpilogue(info As LogInfo, stg As clsReadStage, outputs As clsArgumentList)

    Public MustOverride Sub WritePrologue(info As LogInfo, stg As clsWriteStage)
    Public MustOverride Sub WriteEpilogue(info As LogInfo, stg As clsWriteStage, inputs As clsArgumentList)

    Public MustOverride Sub ObjectPrologue(info As LogInfo, stg As clsStartStage, objectName As String, actionName As String, inputs As clsArgumentList)
    Public MustOverride Sub ObjectEpilogue(info As LogInfo, stg As clsEndStage, objectName As String, actionName As String, outputs As clsArgumentList)

    Public MustOverride Sub ProcessPrologue(info As LogInfo, stg As clsStartStage, inputs As clsArgumentList)
    Public MustOverride Sub ProcessEpilogue(info As LogInfo, stg As clsEndStage, outputs As clsArgumentList)

    Public MustOverride Sub SubSheetPrologue(info As LogInfo, stg As clsStartStage, inputs As clsArgumentList)
    Public MustOverride Sub SubSheetEpilogue(info As LogInfo, stg As clsEndStage, outputs As clsArgumentList)

    Public MustOverride Sub ProcessRefPrologue(info As LogInfo, stg As clsSubProcessRefStage, inputs As clsArgumentList)
    Public MustOverride Sub ProcessRefEpilogue(info As LogInfo, stg As clsSubProcessRefStage, outputs As clsArgumentList)

    Public MustOverride Sub SubSheetRefPrologue(info As LogInfo, stg As clsSubSheetRefStage, inputs As clsArgumentList)
    Public MustOverride Sub SubSheetRefEpilogue(info As LogInfo, stg As clsSubSheetRefStage, outputs As clsArgumentList)

    Public MustOverride Sub ExceptionPrologue(info As LogInfo, stg As clsExceptionStage)
    Public MustOverride Sub ExceptionEpilogue(info As LogInfo, stg As clsExceptionStage)

    Public MustOverride Sub RecoverPrologue(info As LogInfo, stg As clsRecoverStage)
    Public MustOverride Sub RecoverEpilogue(info As LogInfo, stg As clsRecoverStage)

    Public MustOverride Sub ResumePrologue(info As LogInfo, stg As clsResumeStage)
    Public MustOverride Sub ResumeEpilogue(info As LogInfo, stg As clsResumeStage)

    Public MustOverride Sub WaitPrologue(info As LogInfo, stg As clsWaitStartStage)
    Public MustOverride Sub WaitEpilogue(info As LogInfo, stg As clsWaitStartStage, choiceName As String, choiceNo As Integer)

    Public MustOverride Sub ChoicePrologue(info As LogInfo, stg As clsChoiceStartStage)
    Public MustOverride Sub ChoiceEpilogue(info As LogInfo, stg As clsChoiceStartStage, choiceName As String, choiceNo As Integer)

    Public MustOverride Sub SkillPrologue(info As LogInfo, stg As clsSkillStage, objectName As String, skillActionName As String, inputs As clsArgumentList)
    Public MustOverride Sub SkillEpilogue(info As LogInfo, stg As clsSkillStage, objectName As String, skillActionName As String, outputs As clsArgumentList)

    Public Overridable Sub SetEnvironmentVariable(info As LogInfo, loggableMessage As String)
    End Sub
    Public Overridable Sub CreateSessionLog(info As LogInfo)
    End Sub
    Public Overridable Sub ImmediateStop(info As LogInfo, stopReason As String)

    End Sub
    Public Overridable Sub UnexpectedException(info As LogInfo)

    End Sub


#Region " Disposable Handling "

    ''' <summary>
    ''' Disposes of this logging engine, ensuring that it is removed from the process
    ''' that it was logging on behalf of before it is disposed of.
    ''' </summary>
    ''' <param name="disposing">True if disposing of this object explicitly, ie.
    ''' using a Dispose() call; False if disposing as the result of a finalizer
    ''' being called by the garbage collection thread.</param>
    Protected Overridable Sub Dispose(disposing As Boolean)
        If mIsDisposed OrElse Not disposing Then Return
        Dim proc As clsProcess = mContext.Process
        If proc IsNot Nothing Then proc.Logger.Remove(Me)
        mIsDisposed = True
    End Sub

    ''' <summary>
    ''' Finalizes this logging engine, ensuring that any unmanaged data is cleaned up
    ''' as appropriate
    ''' </summary>
    Protected Overrides Sub Finalize()
        Dispose(False)
    End Sub

    ''' <summary>
    ''' Explicitly disposes of this logging engine
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        If mIsDisposed Then Return
        Try
            Dispose(True)

        Catch ex As Exception ' Ignore any errors in disposal
            Debug.Fail(
             String.Format(My.Resources.Resources.clsLoggingEngine_ExceptionWhileDisposingOfLoggingEngine0, [GetType]().FullName),
             ex.ToString())

        End Try

        GC.SuppressFinalize(Me)
    End Sub

#End Region

End Class
