Imports System.Drawing
Imports System.Runtime.Serialization

Namespace Stages

    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsBlockStage
    ''' 
    ''' <summary>
    ''' Class to represent blocks on the process diagram.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsBlockStage
        Inherits clsProcessStage

        Public Sub New(ByVal objParent As clsProcess)
            MyBase.New(objParent)
        End Sub

        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsBlockStage(mParent)
        End Function

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult
            Return New StageResult(False, "Internal", "Cannot execute a block")
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Block
            End Get
        End Property

        Public Overrides Function IsAtPosition(ByVal dblX As Single, ByVal dblY As Single, ByVal gSubSheet As System.Guid) As Boolean
            If gSubSheet = Me.GetSubSheetID Then
                Dim b As New Drawing.Bitmap(1, 1)
                Dim g As Drawing.Graphics = Drawing.Graphics.FromImage(b)
                Dim labelsize As Drawing.SizeF = g.MeasureString(Me.GetName, Me.Font)
                g.Dispose()
                b.Dispose()
                Dim bounds As New Drawing.RectangleF(Me.GetDisplayX, Me.GetDisplayY, labelsize.Width, labelsize.Height)
                Return bounds.Contains(dblX, dblY)
            Else
                Return False
            End If
        End Function

        Public Overrides Sub SetCorner(ByVal sngX As Single, ByVal sngY As Single, ByVal iCorner As Integer)
            Dim sngWidth As Single, sngHeight As Single
            Dim oldX As Single = GetDisplayX()
            Dim oldY As Single = GetDisplayY()
            Dim oldW As Single = GetDisplayWidth()
            Dim oldH As Single = GetDisplayHeight()

            Select Case iCorner
                Case 1
                    sngWidth = (sngX - oldX)
                    sngHeight = (sngY - oldY)
                    sngX = oldX
                    sngY = oldY
                Case 2
                    sngWidth = (oldW - (sngX - oldX))
                    sngHeight = (sngY - oldY)
                    sngY = oldY
                Case 3
                    sngWidth = (oldW - (sngX - oldX))
                    sngHeight = (oldH - (sngY - oldY))
                Case 4
                    sngHeight = (oldH - (sngY - oldY))
                    sngWidth = (sngX - oldX)
                    sngX = oldX
            End Select

            If sngWidth > 5 Then
                SetDisplayX(sngX)
                SetDisplayWidth(sngWidth)
            End If
            If sngHeight > 5 Then
                SetDisplayY(sngY)
                SetDisplayHeight(sngHeight)
            End If
        End Sub

        Public Overrides Function GetDisplayBounds() As RectangleF
            Return New Drawing.RectangleF(GetDisplayX, GetDisplayY, GetDisplayWidth, GetDisplayHeight)
        End Function

    End Class

End Namespace