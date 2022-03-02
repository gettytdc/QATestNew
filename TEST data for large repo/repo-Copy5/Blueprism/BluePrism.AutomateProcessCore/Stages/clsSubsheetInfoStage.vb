Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore.Processes

Namespace Stages
    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsSubsheetInfoStage
    ''' 
    ''' <summary>
    ''' The subsheet info stage shows information about the subsheet on the process diagram
    ''' The actual data that the subsheet info stage stores is not stored in the stage class
    ''' but in the clsProcessSubSheet object instead.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsSubsheetInfoStage
        Inherits clsProcessStage


        ''' <summary>
        ''' Creates a new instance of the clsSubsheerInfo class and sets its parent
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone method
        ''' </summary>
        ''' <returns>A new instance of an SubSheet Info stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsSubsheetInfoStage(mParent)
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.SubSheetInfo
            End Get
        End Property

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult
            Return New StageResult(False, "Internal", My.Resources.Resources.clsSubsheetInfoStage_CanTExecuteStage0 & GetName())
        End Function

        ''' <summary>
        ''' Checks for errors in this subsheet info stage.
        ''' </summary>
        ''' <param name="bAttemptRepair">Flag indicating if this call is attempting
        ''' to repair or not.</param>
        ''' <param name="SkipObjects">I... don't really know.</param>
        ''' <returns>A list of validate checks which failed for this stage.</returns>
        Public Overrides Function CheckForErrors(
         ByVal bAttemptRepair As Boolean, ByVal SkipObjects As Boolean) _
         As ValidationErrorList

            ' Let the superclass get some initial errors.
            Dim errs As ValidationErrorList =
             MyBase.CheckForErrors(bAttemptRepair, SkipObjects)

            Dim ss As clsProcessSubSheet = Me.SubSheet

            ' If this is a published action page, ensure that a description is in there.
            If mParent.ProcessType = DiagramType.Object Then
                If ss.IsNormal AndAlso ss.Published AndAlso GetNarrative() = "" Then
                    errs.Add(New ValidateProcessResult(Me, 137, String.Format(My.Resources.Resources.clsSubsheetInfoStage_Narrative0, ss.Name)))
                End If
            End If

            ' If this isn't cleanup page, ensure we have pre/post conditions
            ' Incredibly, the pre/postconditions, although edited via the info stage,
            ' are stored in the start stage!
            If ss IsNot mParent.GetCleanupPage Then
                Dim startstage As clsProcessStage = ss.StartStage
                If startstage.Preconditions.Count = 0 Then _
                 errs.Add(New ValidateProcessResult(Me, 131))
                If startstage.PostConditions.Count = 0 Then _
                 errs.Add(New ValidateProcessResult(Me, 132))
            End If

            Return errs

        End Function


    End Class
End Namespace
