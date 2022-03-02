
Imports System.Runtime.Serialization

Namespace Stages
    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.clsResumeStage
    ''' 
    ''' <summary>
    ''' A class representing a Resume stage
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsResumeStage
        Inherits clsLinkableStage

        ''' <summary>
        ''' Constructor for the clsResumeStage class
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        ''' <summary>
        ''' Creates a new instance of this stage for the purposes of cloning
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsResumeStage(mParent)
        End Function

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult

            Dim sErr As String = Nothing
            ResumePrologue(logger)

            If Not mParent.ClearRecovery() Then
                Return New StageResult(False, "Internal", My.Resources.Resources.clsResumeStage_EncounteredResumeStageWhenNotInRecoveryMode)
            End If

            'Move to next stage...
            If Not mParent.UpdateNextStage(gRunStageID, LinkType.OnSuccess, sErr) Then
                Return New StageResult(False, "Internal", sErr)
            End If

            ResumeEpilogue(logger)
            Return New StageResult(True)

        End Function

        Private Sub ResumePrologue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.ResumePrologue(info, Me)
        End Sub

        Private Sub ResumeEpilogue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.ResumeEpiLogue(info, Me)
        End Sub

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Resume
            End Get
        End Property

    End Class
End Namespace
