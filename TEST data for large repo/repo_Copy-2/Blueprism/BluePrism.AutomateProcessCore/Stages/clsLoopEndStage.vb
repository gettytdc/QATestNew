Imports System.Runtime.Serialization

Namespace Stages

    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsLoopEndStage
    ''' 
    ''' <summary>
    ''' The loop end stage represents the point at which the loop will repeat or when
    ''' the the loop has finished the enpoint of the loop. 
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsLoopEndStage
        Inherits clsGroupStage

        ''' <summary>
        ''' Creates a new instance of the clsLoopEndStage class, and sets the parent.
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method.
        ''' </summary>
        ''' <returns>A new instance of an loop end stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsLoopEndStage(mParent)
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.LoopEnd
            End Get
        End Property

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult

            LoopEndPrologue(logger)

            Dim loopStart As clsLoopStartStage = mParent.GetLoopStart(Me)
            If loopStart Is Nothing Then
                Return New StageResult(False, "Internal", My.Resources.Resources.clsLoopEndStage_LoopEndWithoutMatchingLoopStart)
            End If
            If loopStart.LoopType <> "ForEach" Then
                Return New StageResult(False, "Internal", My.Resources.Resources.clsLoopEndStage_UnsupportedLoopType)
            End If

            'Get the collection we're looping over...
            Dim col As clsCollection
            Try
                col = loopStart.GetTargetCollection()
            Catch ex As Exception
                Return New StageResult(False, "Internal", ex.Message)
            End Try

            Dim sIteration As String
            Dim TempRunStageID As Guid
            If col.ContinueIterate() Then
                TempRunStageID = mParent.GetNextStage(loopStart.GetStageID(), LinkType.OnSuccess)
                sIteration = String.Format(My.Resources.Resources.clsLoopEndStage_0Of1, col.CurrentRowIndex.ToString(), col.Count.ToString())
            Else
                TempRunStageID = mParent.GetNextStage(GetStageID(), LinkType.OnSuccess)
                sIteration = String.Format(My.Resources.Resources.clsLoopEndStage_0Of1, col.Count.ToString(), col.Count.ToString())
            End If

            LoopEndEpilogue(logger, sIteration)

            If TempRunStageID.Equals(Guid.Empty) Then
                Return New StageResult(False, "Internal", My.Resources.Resources.clsLoopEndStage_MissingLinkFromLoopEnd)
            Else
                gRunStageID = TempRunStageID
            End If

            Return New StageResult(True)

        End Function

        Private Sub LoopEndPrologue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.LoopEndPrologue(info, Me)
        End Sub

        Private Sub LoopEndEpilogue(logger As CompoundLoggingEngine, ByVal sIteration As String)
            Dim info = GetLogInfo()
            logger.LoopEndEpiLogue(info, Me, sIteration)
        End Sub

        Public Overrides Function CheckForErrors(ByVal bAttemptRepair As Boolean, ByVal SkipObjects As Boolean) As ValidationErrorList
            Dim Errors As ValidationErrorList = MyBase.CheckForErrors(bAttemptRepair, SkipObjects)

            If mgGroupID.Equals(Guid.Empty) Then
                Errors.Add(New ValidateProcessResult(Me, 103))
            Else
                Dim GroupStart As clsGroupStage = mParent.GetLoopStart(Me)
                If GroupStart Is Nothing Then
                    Errors.Add(New ValidateProcessResult(Me, 103))
                Else
                    'Check for loop start being on different page.
                    'Needs to be done inside else clause to avoid
                    'null reference exception!
                    If Not GroupStart.GetSubSheetID().Equals(Me.GetSubSheetID) Then
                        Errors.Add(New ValidateProcessResult(Me, 105))
                    End If
                End If
            End If

            Return Errors
        End Function

    End Class

End Namespace
