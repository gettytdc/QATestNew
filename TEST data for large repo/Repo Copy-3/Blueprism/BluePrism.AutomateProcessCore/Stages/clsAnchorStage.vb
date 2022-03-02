Imports System.Runtime.Serialization

Namespace Stages
    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsAnchorStage
    ''' 
    ''' <summary>
    ''' The anchor stage represents a point on the process diagram where several links
    ''' join together, it is also useful for links that need to go round a corner.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsAnchorStage
        Inherits clsLinkableStage

        ''' <summary>
        ''' Creates a new instance of the clsAnchorStageClass, and sets the width and
        ''' height to the correct values.
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
            SetDisplayWidth(10)
            SetDisplayHeight(10)
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of an anchor stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsAnchorStage(mParent)
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Anchor
            End Get
        End Property

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult
            Dim sErr As String = Nothing
            If Not mParent.UpdateNextStage(gRunStageID, LinkType.OnSuccess, sErr) Then
                Return New StageResult(False, "Internal", sErr)
            End If
            Return New StageResult(True)

        End Function

        Public Overrides Function GetShortText() As String
            Return String.Empty
        End Function
    End Class
End Namespace
