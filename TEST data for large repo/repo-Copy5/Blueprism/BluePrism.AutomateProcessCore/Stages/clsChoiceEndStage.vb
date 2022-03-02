Imports System.Runtime.Serialization

Namespace Stages
    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsChoiceEndStage
    ''' 
    ''' <summary>
    ''' The choice end stage represents the end of a choice, which is where execution
    ''' jumps to if none of the choices match. A choice is like a multiple decision.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsChoiceEndStage
        Inherits clsGroupStage

        ''' <summary>
        ''' Creates a new instance of the clsChoiceEndStage class and sets its parent.
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of an Choice end stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsChoiceEndStage(mParent)
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.ChoiceEnd
            End Get
        End Property

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult

            Dim sErr As String = Nothing
            If Not mParent.UpdateNextStage(gRunStageID, LinkType.OnSuccess, sErr) Then
                Return New StageResult(False, "Internal", sErr)
            End If
            Return New StageResult(True)

        End Function


        Public Overrides Sub SetCorner(ByVal dblX As Single, ByVal dblY As Single, ByVal iCorner As Integer)
            'Remember the current size
            Dim OldWidth As Single = Me.GetDisplayWidth
            Dim OldHeight As Single = Me.GetDisplayHeight

            'Apply the size change as requested
            MyBase.SetCorner(dblX, dblY, iCorner)

            'Revert the change if it would mean obscuring any nodes
            Dim objChoiceStart As clsChoiceStartStage = mParent.GetChoiceStart(Me)
            If objChoiceStart.Choices.Count > 0 Then
                If objChoiceStart.GetMaximumNodeDistance < objChoiceStart.Choices(objChoiceStart.Choices.Count - 1).Distance Then
                    Me.SetDisplayWidth(OldWidth)
                    Me.SetDisplayHeight(OldHeight)
                End If
            End If
        End Sub


    End Class
End Namespace
