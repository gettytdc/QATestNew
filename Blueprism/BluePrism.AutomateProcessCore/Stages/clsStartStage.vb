Imports System.Drawing
Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore.Processes

Namespace Stages

    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsStartStage
    ''' 
    ''' <summary>
    ''' The start stage represents the startpoint of a process or subsheet.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsStartStage
        Inherits clsLinkableStage

        ''' <summary>
        ''' Creates a new instance of the clsStartStage class and sets its parent.
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of an start stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsStartStage(mParent)
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Start
            End Get
        End Property

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult

            Dim sErr As String = Nothing

            If GetSubSheetID().Equals(mParent.GetMainPage().ID) Then
                'This is the main start stage, not a subsheet
                'start...

                'Process startup parameters...
                If Not mParent.mInputParameters Is Nothing Then
                    If Not mParent.StoreParametersInDataItems(Me, mParent.mInputParameters, sErr, False) Then
                        Return New StageResult(False, "Internal", sErr)
                    End If
                End If
                If Not mParent.UpdateNextStage(gRunStageID, LinkType.OnSuccess, sErr) Then
                    Return New StageResult(False, "Internal", sErr)
                End If
                'Log it...
                ProcessPrologue(logger, mParent.mInputParameters)

            Else
                'This is a subsheet start stage...
                mParent.ResetRecoveryAttempts(GetSubSheetID())
                mParent.ResetDataItemCurrentValues(GetSubSheetID())
                If Not mParent.mRunInputs Is Nothing Then
                    If Not mParent.StoreParametersInDataItems(Me, mParent.mRunInputs, sErr, False) Then
                        Return New StageResult(False, "Internal", sErr)
                    End If
                End If
                If Not mParent.UpdateNextStage(gRunStageID, LinkType.OnSuccess, sErr) Then
                    Return New StageResult(False, "Internal", sErr)
                End If

                'Log it...
                Dim objProcessSubSheet As clsProcessSubSheet = mParent.GetSubSheetByID(Me.GetSubSheetID)
                If mParent.ProcessType = DiagramType.Object AndAlso objProcessSubSheet.Published Then
                    ObjectPrologue(logger, mParent.Name, objProcessSubSheet.Name, mParent.mRunInputs)
                Else
                    SubSheetPrologue(logger, mParent.mRunInputs)
                End If


            End If

            Return New StageResult(True)

        End Function

        Private Sub ObjectPrologue(logger As CompoundLoggingEngine, sObjectName As String, sActionName As String, inputs As clsArgumentList)
            Dim info = GetLogInfo()
            logger.ObjectPrologue(info, Me, sObjectName, sActionName, inputs)
        End Sub

        Private Sub ProcessPrologue(logger As CompoundLoggingEngine, inputs As clsArgumentList)
            Dim info = GetLogInfo()
            logger.ProcessPrologue(info, Me, inputs)
        End Sub

        Private Sub SubSheetPrologue(logger As CompoundLoggingEngine, inputs As clsArgumentList)
            Dim info = GetLogInfo()
            logger.SubSheetPrologue(info, Me, inputs)
        End Sub

        Public Shared Sub DrawTerminal(ByVal r As IRender, b As RectangleF)
            Using gp As New Drawing2D.GraphicsPath
                With b
                    gp.AddArc(.X, .Y, .Width / 2, .Height, 90, 180)
                    gp.AddLine(.X + .Width / 4, .Y, .X + .Width - .Width / 4, .Y)
                    gp.AddArc(.X + .Width / 2, .Y, .Width / 2, .Height, 270, 180)
                    gp.AddLine(.X + .Width / 4, .Y + .Height, .X + .Width - .Width / 4, .Y + .Height)
                    gp.CloseFigure()

                    r.FillPath(gp)
                    r.DrawPath(gp)
                End With
            End Using
        End Sub

        Public Overrides Function GetShortText() As String
            Return "Start"
        End Function

    End Class

End Namespace
