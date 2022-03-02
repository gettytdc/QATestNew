Imports BluePrism.Core.Expressions
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports System.Runtime.Serialization

Namespace Stages

    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsDecisionStage
    ''' 
    ''' <summary>
    ''' The Decision stage represents a an expression that when evaluated must result 
    ''' in a true or false result, if the result is true the stage pointed to by the
    ''' ontrue guid will be executed, if false the stage pointed to by the onfalse
    ''' guid will be executed.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsDecisionStage
        Inherits clsProcessStage
        Implements ILinkable, IExpressionHolder

        ''' <summary>
        ''' The id of the stage to go to when the result of the decision is True, or
        ''' Guid.Empty if there is no link.
        ''' </summary>
        Public Property OnTrue() As Guid Implements ILinkable.OnSuccess
            Get
                Return mOnTrue
            End Get
            Set(ByVal value As Guid)
                mOnTrue = value
            End Set
        End Property
        <DataMember>
        Private mOnTrue As Guid = Guid.Empty

        ''' <summary>
        ''' The id of the stage to go to when the result of the decision is False, or
        ''' Guid.Empty if there is no link.
        ''' </summary>
        Public Property OnFalse() As Guid
            Get
                Return mOnFalse
            End Get
            Set(ByVal value As Guid)
                mOnFalse = value
            End Set
        End Property
        <DataMember>
        Private mOnFalse As Guid = Guid.Empty

        ''' <summary>
        ''' Stores the expression to be evaluated at runtime.
        ''' </summary>
        <DataMember>
        Private mExpression As BPExpression = BPExpression.Empty

        ''' <summary>
        ''' Creates a new instance of the clsDecisionStage and sets its parent.
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub


        ''' <summary>
        ''' Sets the next link to the specified value.
        ''' </summary>
        ''' <param name="gID">The ID of the stage to link to.</param>
        ''' <remarks>Sets the OnTrue link if it is not populated
        ''' already, otherwise sets the OnFalse link.</remarks>
        Public Function SetNextLink(ByVal gID As Guid, ByRef sErr As String) As Boolean Implements ILinkable.SetNextLink
            If gID = Id Then sErr = My.Resources.Resources.clsDecisionStage_StageCannotLinkToItself : Return False
            If mOnTrue = Guid.Empty Then mOnTrue = gID Else mOnFalse = gID
            Return True
        End Function


        ''' <summary>
        ''' Determine if this stage links (in a forwards direction) to another stage.
        ''' </summary>
        ''' <param name="st">The stage to check against</param>
        ''' <returns>True if there is a link, False otherwise.</returns>
        Public Overrides Function LinksTo(ByVal st As clsProcessStage) As Boolean
            Return (mOnTrue = st.Id OrElse mOnFalse = st.Id)
        End Function

        ''' <summary>
        ''' Gets the expression which governs the decision.
        ''' </summary>
        ''' <remarks>This will never be null - if null is passed when setting it, an
        ''' <see cref="BPExpression.Empty">empty expression</see> is set instead.
        ''' </remarks>
        Public Property Expression() As BPExpression _
         Implements IExpressionHolder.Expression
            Get
                Return mExpression
            End Get
            Set(ByVal value As BPExpression)
                If value Is Nothing Then value = BPExpression.Empty
                mExpression = value
            End Set
        End Property

        ''' <summary>
        ''' Switches the destinations of the OnTrue and the OnFalse links.
        ''' </summary>
        ''' <param name="sType">The type of link to switch. Valid values are
        ''' "True" or "False". It does not matter which you supply; the effect will
        ''' be the same either way.</param>
        ''' <param name="sErr">String to carry an error message.</param>
        ''' <returns>Returns true on success; false otherwise. When false,
        ''' the argument sErr will be populated with an error message.</returns>
        Public Function SwitchLink(ByVal sType As String, ByRef sErr As String) As Boolean
            Select Case sType
                Case "True", "False"
                    BPUtil.SwapIf(True, mOnTrue, mOnFalse)
                Case Else
                    sErr = My.Resources.Resources.clsDecisionStage_CanTSwitchThatTypeOfLink
                    Return False
            End Select
            Return True
        End Function

        ''' <summary>
        ''' Gets a collection of stages that this stage links to. In this case it is
        ''' a list  containing either the ontrue, onfalse, or both.
        ''' </summary>
        ''' <returns>A collection of stages that this stage links to.</returns>
        Friend Overrides Function GetLinks() As ICollection(Of clsProcessStage)

            If mOnTrue <> Guid.Empty OrElse mOnFalse <> Guid.Empty Then
                Dim links As New List(Of clsProcessStage)
                If mOnTrue <> Guid.Empty Then links.Add(mParent.GetStage(mOnTrue))
                If mOnFalse <> Guid.Empty Then links.Add(mParent.GetStage(mOnFalse))
                Return links
            End If

            Return GetEmpty.ICollection(Of clsProcessStage)()
        End Function


        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of an decision stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsDecisionStage(mParent)
        End Function

        ''' <summary>
        ''' Creates a deep copy of the decision stage.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function Clone() As clsProcessStage
            Dim copy As clsDecisionStage = CType(MyBase.Clone(), clsDecisionStage)
            copy.OnTrue = mOnTrue
            copy.OnFalse = mOnFalse
            ' Expressions are not mutable so this is safe
            copy.Expression = Expression
            Return copy
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Decision
            End Get
        End Property

        ''' <summary>
        ''' Returns items referred to by this stage, currently only things defined
        ''' within the process (e.g. data items).
        ''' </summary>
        ''' <param name="inclInternal">Indicates internal references required</param>
        ''' <returns>List of dependencies</returns>
        Public Overrides Function GetDependencies(inclInternal As Boolean) As List(Of clsProcessDependency)
            Dim deps As List(Of clsProcessDependency) = MyBase.GetDependencies(inclInternal)

            If inclInternal Then
                For Each dataItem As String In Expression.GetDataItems()
                    Dim outOfScope As Boolean
                    Dim stage = mParent.GetDataStage(dataItem, Me, outOfScope)
                    If Not outOfScope AndAlso stage IsNot Nothing Then _
                        deps.Add(New clsProcessDataItemDependency(stage))
                Next
            End If

            Return deps
        End Function

        Public Overrides Function Execute(ByRef stgId As Guid, logger As CompoundLoggingEngine) As StageResult

            Dim sErr As String = Nothing
            DecisionPrologue(logger)

            'Evaluate the decision, and move to the
            'next appropriate stage...
            Dim res As clsProcessValue = Nothing

            'Evaluate the expression...
            If Not clsExpression.EvaluateExpression(Expression, res, Me, False, Nothing, sErr) Then _
             Return StageResult.InternalError(sErr)

            'The expression should have returned the correct data
            'type, and consequently the value should be either True
            'or False, so trap the error now if it isn't...
            If res.DataType <> DataType.flag OrElse res.IsNull Then _
             Return StageResult.InternalError(My.Resources.Resources.clsDecisionStage_DecisionDidNotResultInAYesNoAnswer)

            Dim isYes As Boolean = CBool(res)
            DecisionEpilogue(logger, isYes.ToString())

            'Update link to next stage, based on true/false decision result
            Dim linker As LinkType
            If isYes Then linker = LinkType.OnTrue Else linker = LinkType.OnFalse

            If Not mParent.UpdateNextStage(stgId, linker, sErr) Then _
             Return StageResult.InternalError(sErr)

            Return StageResult.OK

        End Function

        Private Sub DecisionPrologue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.DecisionPrologue(info, Me)
        End Sub

        Private Sub DecisionEpilogue(logger As CompoundLoggingEngine, ByVal sResult As String)
            Dim info = GetLogInfo()
            logger.DecisionEpiLogue(info, Me, sResult)
        End Sub

        Public Overrides Sub FromXML(ByVal e2 As System.Xml.XmlElement)
            MyBase.FromXML(e2)
            For Each e3 As Xml.XmlElement In e2.ChildNodes
                Select Case e3.Name
                    Case "ontrue"
                        Dim s As String = e3.InnerText
                        If s <> "" Then
                            mOnTrue = New Guid(s)
                        End If
                    Case "onfalse"
                        Dim s As String = e3.InnerText
                        If s <> "" Then
                            mOnFalse = New Guid(s)
                        End If
                    Case "decision"
                        Expression = BPExpression.FromNormalised(e3.GetAttribute("expression"))
                End Select
            Next
        End Sub

        Public Overrides Sub ToXml(ByVal ParentDocument As System.Xml.XmlDocument, ByVal StageElement As System.Xml.XmlElement, ByVal bSelectionOnly As Boolean)
            MyBase.ToXml(ParentDocument, StageElement, bSelectionOnly)
            Dim e2 As Xml.XmlElement
            e2 = ParentDocument.CreateElement("decision")
            e2.SetAttribute("expression", Expression.NormalForm)
            StageElement.AppendChild(e2)
            Dim bIncludeLink As Boolean
            If Not mOnTrue.Equals(Guid.Empty) Then
                bIncludeLink = True
                If bSelectionOnly Then
                    If Not mParent.IsStageSelected(mOnTrue) Then
                        bIncludeLink = False
                    End If
                End If
                If bIncludeLink Then
                    e2 = ParentDocument.CreateElement("ontrue")
                    e2.AppendChild(ParentDocument.CreateTextNode(mOnTrue.ToString()))
                    StageElement.AppendChild(e2)
                End If
            End If
            If Not mOnFalse.Equals(Guid.Empty) Then
                bIncludeLink = True
                If bSelectionOnly Then
                    If Not mParent.IsStageSelected(mOnFalse) Then
                        bIncludeLink = False
                    End If
                End If
                If bIncludeLink Then
                    e2 = ParentDocument.CreateElement("onfalse")
                    e2.AppendChild(ParentDocument.CreateTextNode(mOnFalse.ToString()))
                    StageElement.AppendChild(e2)
                End If
            End If
        End Sub

        Public Overrides Function CheckForErrors(ByVal bAttemptRepair As Boolean, ByVal SkipObjects As Boolean) As ValidationErrorList
            Dim errors As ValidationErrorList = MyBase.CheckForErrors(bAttemptRepair, SkipObjects)
            Dim dest As clsProcessStage

            'Validate the expression
            errors.AddRange(clsExpression.CheckExpressionForErrors(
             Expression.NormalForm, Me, DataType.flag, "", Nothing, Nothing))

            'Check that OnTrue is valid
            If mOnTrue = Guid.Empty Then
                errors.Add(Me, 51)
            Else
                dest = mParent.GetStage(mOnTrue)
                If dest Is Nothing Then
                    errors.Add(Me, 52)
                Else
                    If Not dest.GetSubSheetID().Equals(Me.GetSubSheetID()) Then
                        errors.Add(Me, 53)
                    End If
                End If
            End If

            'Check that OnFalse is valid
            If mOnFalse = Guid.Empty Then
                errors.Add(Me, 54)
            Else
                dest = mParent.GetStage(mOnFalse)
                If dest Is Nothing Then
                    errors.Add(Me, 55)
                Else
                    If dest.SubSheet IsNot Me.SubSheet Then errors.Add(Me, 56)
                End If
            End If

            Return errors
        End Function

    End Class

End Namespace