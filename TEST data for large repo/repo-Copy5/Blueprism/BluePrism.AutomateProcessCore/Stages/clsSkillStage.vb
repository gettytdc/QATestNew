Imports System.Xml
Imports BluePrism.Skills
Imports System.Runtime.Serialization

Namespace Stages
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsSkillStage
        Inherits clsLinkableStage

        <DataMember>
        Private mActionName As String

        <DataMember>
        Private mSkillId As Guid

        Private ReadOnly Property StageName As String
            Get
                Return GetShortText()
            End Get
        End Property

        Public ReadOnly Property SkillId As Guid
            Get
                Return mSkillId
            End Get
        End Property

        Public Property ActionName As String
            Get
                Return mActionName
            End Get
            Set(value As String)
                mActionName = value
            End Set
        End Property

        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Skill
            End Get
        End Property

        Protected Overrides Function GetDefaultStageWidth() As Single
            Return 120
        End Function

        Protected Overrides Function GetDefaultStageHeight() As Single
            Return 60
        End Function

        Public Sub New(parent As clsProcess)
            MyBase.New(parent)
        End Sub

        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsSkillStage(mParent)
        End Function

        Public Overrides Function Clone() As clsProcessStage
            Dim copy = CType(MyBase.Clone, clsSkillStage)
            copy.mSkillId = SkillId
            copy.ActionName = ActionName
            Return copy
        End Function

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult

            Dim inputs As clsArgumentList = Nothing
            Dim outputs As clsArgumentList = Nothing
            Dim errorMessage As String = Nothing
            Dim skill As SkillDetails = mParent.GetSkill(SkillId)

            If skill Is Nothing Then _
                Return StageResult.InternalError(String.Format(My.Resources.Resources.clsSkillStage_TheSpecifiedSkillInStage0IsNotAvailable, StageName) &
                     My.Resources.Resources.clsSkillStage_PleaseEnsureItIsInstalledAndHasNotBeenDisabled)

            If mParent.mChildWaitProcess Is Nothing Then
                'Get the inputsXML for the action
                If Not mParent.ReadDataItemsFromParameters(GetParameters(), Me, inputs, errorMessage, False, False) Then _
                    Return StageResult.InternalError(errorMessage)

                SkillPrologue(logger, skill.SkillName, ActionName, inputs)
            End If

            Dim actionResult = ExecuteWebApiAction(logger, skill.WebApiName, inputs, outputs)

            If Not actionResult.Success Then
                mParent.mChildWaitProcess = Nothing
                Return actionResult
            End If

            SkillEpilogue(logger, skill.SkillName, ActionName, outputs)

            If Not HandleSuccess(outputs, gRunStageID, errorMessage) Then _
                Return StageResult.InternalError(errorMessage)

            Return New StageResult(True)
        End Function

        Public Function GetImageBytes() As Byte()
            Return mParent.GetSkill(SkillId)?.ImageBytes
        End Function

        Public Function GetCategory() As SkillCategory
            Return If(mParent.GetSkill(SkillId)?.Category, SkillCategory.Unknown)
        End Function

        Private Function ExecuteWebApiAction(logger As CompoundLoggingEngine, webApiName As String, inputs As clsArgumentList, ByRef outputs As clsArgumentList) As StageResult
            If webApiName = "" Then _
                Return StageResult.InternalError(My.Resources.Resources.clsSkillStage_NoWebAPISpecifiedForSkill)

            Dim webApi = mParent.GetBusinessObjectRef(webApiName)
            If webApi Is Nothing Then
                Return StageResult.InternalError(String.Format(My.Resources.Resources.clsSkillStage_TheWebAPI0IsNotAvailable, webApiName) &
                     My.Resources.Resources.clsSkillStage_PleaseEnsureThatItIsInstalledAndHasNotBeenDisabled)
            ElseIf Not webApi.Valid Then
                Return StageResult.InternalError(
                String.Format(My.Resources.Resources.clsSkillStage_TheWebAPI0IsNotValidTheLastErrorMessageRecordedWas1, webApiName, webApi.ErrorMessage))
            End If

            Dim logWithLogger = Sub(message As String) LogDiagnostics(logger, message)

            If (clsAPC.Diagnostics And clsAPC.Diags.LogWebServices) <> 0 Then _
                                           AddHandler CType(webApi, IDiagnosticEmitter).Diags, logWithLogger

            If String.IsNullOrEmpty(ActionName) Then
                Return StageResult.InternalError(My.Resources.Resources.clsSkillStage_NoResourceSpecifiedForAction)
            End If

            Dim action = webApi.GetAction(ActionName)
            If action Is Nothing Then _
                                       Return StageResult.InternalError(String.Format(My.Resources.Resources.clsSkillStage_TheWebAPI0DoesNotSupportTheAction1, webApiName, Me.ActionName))

            Dim actionResult = webApi.DoAction(ActionName, Me, inputs, outputs)

            RemoveHandler CType(webApi, IDiagnosticEmitter).Diags, logWithLogger

            Return actionResult
        End Function

        Private Function HandleSuccess(outputs As clsArgumentList, ByRef runStageId As Guid, ByRef errorMessage As String) As Boolean

            mParent.mChildWaitProcess = Nothing

            If outputs IsNot Nothing And Not mParent.StoreParametersInDataItems(Me, outputs, errorMessage) Then _
                Return False

            If Not mParent.UpdateNextStage(runStageId, LinkType.OnSuccess, errorMessage) Then _
                Return False

            Return True
        End Function

        Private Sub LogDiagnostics(logger As CompoundLoggingEngine, ByVal message As String)
            Dim info = GetLogInfo()
            logger.LogDiagnostic(info, Me, message)
        End Sub

        Private Sub SkillPrologue(logger As CompoundLoggingEngine, objectName As String, actionName As String, inputs As clsArgumentList)
            Dim info = GetLogInfo()
            logger.SkillPrologue(info, Me, objectName, actionName, inputs)
        End Sub

        Private Sub SkillEpilogue(logger As CompoundLoggingEngine, objectName As String, actionName As String, outputs As clsArgumentList)
            Dim info = GetLogInfo()
            logger.SkillEpilogue(info, Me, objectName, actionName, outputs)
        End Sub

        Public Overrides Sub SetContext(context As Object)
            mSkillId = CType(context, Guid)
        End Sub

        Public Overrides Sub FromXML(stageElement As XmlElement)
            MyBase.FromXML(stageElement)
            For Each node As XmlElement In stageElement.ChildNodes
                Select Case node.Name
                    Case "skill"
                        mSkillId = New Guid(node.GetAttribute("id"))
                        ActionName = node.GetAttribute("action")
                End Select
            Next
        End Sub

        Public Overrides Sub ToXml(parentDocument As XmlDocument, stageElement As XmlElement, selectionOnly As Boolean)
            MyBase.ToXml(parentDocument, stageElement, selectionOnly)
            Dim skillElement = parentDocument.CreateElement("skill")
            skillElement.SetAttribute("id", SkillId.ToString())
            skillElement.SetAttribute("action", ActionName)
            stageElement.AppendChild(skillElement)
        End Sub

        Public Overrides Function CheckForErrors(attemptRepair As Boolean, skipObjects As Boolean) As ValidationErrorList

            Dim errors As ValidationErrorList = MyBase.CheckForErrors(attemptRepair, skipObjects)
            Dim skill = mParent.GetSkill(SkillId)

            If (skill Is Nothing) Then
                errors.Add(New ValidateProcessResult(Me, String.Empty, 146, StageName))
            Else
                Dim webApi = mParent.GetBusinessObjectRef(skill.WebApiName)
                If webApi Is Nothing Then errors.Add(New ValidateProcessResult(Me, "helpBusinessObjects.htm", 98, skill.WebApiName))
            End If

            If GetNarrative().Length = 0 Then
                errors.Add(New ValidateProcessResult(Me, 129))
            End If

            Return errors
        End Function
        Public Overrides Function GetDependencies(includeInternal As Boolean) As List(Of clsProcessDependency)
            Dim dependencies = MyBase.GetDependencies(includeInternal)

            If SkillId <> Guid.Empty Then
                dependencies.Add(New clsProcessSkillDependency(SkillId))
            End If

            Return dependencies
        End Function

    End Class

End Namespace
