Imports System.Runtime.Serialization
Imports System.Xml

Namespace Stages
    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.clsRecoverStage
    ''' 
    ''' <summary>
    ''' A class representing a Recovery stage
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsRecoverStage
        Inherits clsLinkableStage

        Public Property MaxAttempts As Integer

        Public Property LimitAttempts As Boolean

        Friend ReadOnly Property AttemptsExceeded As Boolean
            Get
                If Not LimitAttempts Then Return False
                Return Attempts >= MaxAttempts
            End Get
        End Property

        Public Sub ResetAttempts()
            Attempts = 0
        End Sub

        Private Property Attempts As Integer

        ''' <summary>
        ''' Constructor for the clsRecoverStartStage class
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        Public Overrides Sub FromXML(e2 As XmlElement)
            MyBase.FromXML(e2)
            For Each child As XmlNode In e2.ChildNodes
                Select Case child.Name
                    Case "attempts"
                        LimitAttempts = True
                        Dim s = child.InnerText
                        If Not String.IsNullOrEmpty(s) Then
                            MaxAttempts = Integer.Parse(s)
                        End If
                End Select
            Next
        End Sub

        Public Overrides Sub ToXml(ParentDocument As XmlDocument, StageElement As XmlElement, bSelectionOnly As Boolean)
            MyBase.ToXml(ParentDocument, StageElement, bSelectionOnly)

            If LimitAttempts Then
                Dim attempts = ParentDocument.CreateElement("attempts")
                Dim text = ParentDocument.CreateTextNode(MaxAttempts.ToString)
                attempts.AppendChild(text)
                StageElement.AppendChild(attempts)
            End If

        End Sub

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult

            Dim sErr As String = Nothing
            RecoverPrologue(logger)

            If LimitAttempts Then Attempts += 1

            'Move to next stage...
            If Not mParent.UpdateNextStage(gRunStageID, LinkType.OnSuccess, sErr) Then
                Return New StageResult(False, "Internal", sErr)
            End If

            RecoverEpilogue(logger)
            Return New StageResult(True)
        End Function

        Private Sub RecoverPrologue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.RecoverPrologue(info, Me)
        End Sub

        Private Sub RecoverEpilogue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.RecoverEpiLogue(info, Me)
        End Sub

        ''' <summary>
        ''' Creates a new instance of this stage for the purposes of cloning
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsRecoverStage(mParent)
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Recover
            End Get
        End Property

        Public Overrides Function Clone() As clsProcessStage
            Dim copy = CType(MyBase.Clone(), clsRecoverStage)
            copy.Attempts = Attempts
            copy.MaxAttempts = MaxAttempts
            copy.LimitAttempts = LimitAttempts
            Return copy
        End Function
    End Class

End Namespace