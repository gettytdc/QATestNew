Imports System.Runtime.Serialization

Namespace Stages

    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsNoteStage
    ''' 
    ''' <summary>
    ''' The note stage allows for arbitary notes to be added to the process diagram
    ''' These can also be linked to for the purpose of adding a note to the logs.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsNoteStage
        Inherits clsLinkableStage

        ''' <summary>
        ''' Creates a new instance of the clsNoteStage class and sets its parent
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
            Narrative = My.Resources.Resources.clsNoteStage_NewNote
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of an note stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsNoteStage(mParent)
        End Function


        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Note
            End Get
        End Property

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult

            Dim sErr As String = Nothing
            NotePrologue(logger)

            If Not mParent.UpdateNextStage(gRunStageID, LinkType.OnSuccess, sErr) Then
                Return New StageResult(False, "Internal", sErr)
            End If

            NoteEpilogue(logger)
            Return New StageResult(True)

        End Function

        Private Sub NotePrologue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.NotePrologue(info, Me)
        End Sub

        Private Sub NoteEpilogue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.NoteEpiLogue(info, Me)
        End Sub

        ''' <summary>
        ''' Missing links out of note stages are not considered an error
        ''' </summary>
        Protected Overrides ReadOnly Property AllowsMissingLinks() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides Function GetShortText() As String
            Return GetNarrative()
        End Function
    End Class
End Namespace
