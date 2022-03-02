Imports System.Runtime.Serialization

Namespace Stages

    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsWaitEndStage
    ''' 
    ''' <summary>
    ''' The wait end stage represents the end of a wait, which is where execution
    ''' jumps to if the wait times out. A wait end is thus similar to a choice end.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsWaitEndStage
        Inherits clsChoiceEndStage

        ''' <summary>
        ''' Creates a new instance of the clsWaitEndStage class and sets its parent.
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
            SetDisplayWidth(30)
            SetDisplayHeight(30)
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of an Wait End stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsWaitEndStage(mParent)
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.WaitEnd
            End Get
        End Property

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult
            Dim sErr As String = Nothing
            If Not mParent.UpdateNextStage(gRunStageID, LinkType.OnSuccess, sErr) Then
                Return New StageResult(False, "Internal", sErr)
            End If
            Return New StageResult(True)
        End Function

    End Class

End Namespace
