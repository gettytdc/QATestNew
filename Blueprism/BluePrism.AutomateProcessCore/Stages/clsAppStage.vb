Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.Runtime.Serialization
Imports System.Xml
Imports BluePrism.AMI
Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Core.Expressions
Imports BluePrism.Server.Domain.Models

Namespace Stages
    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsAppStage
    ''' 
    ''' <summary>
    ''' This class covers stages that act on an Application managed by Application
    ''' Manager, e.g. Reader, Writer and Navigate. The specific classes for these
    ''' stage types are derived from this one - this covers the common features.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp"),
    KnownType(GetType(clsNavigateStep)), KnownType(GetType(clsReadStep)), KnownType(GetType(clsWriteStep))>
    Public MustInherit Class clsAppStage : Inherits clsLinkableStage

#Region " Inner Classes "

        ''' <summary>
        ''' Class encapsulating the target element for a step as well as a list
        ''' of identifiers which describe that element
        ''' </summary>
        Friend Class Payload
            Public TargetElement As clsApplicationElement
            Public Identifiers As List(Of clsIdentifierInfo)
        End Class

#End Region

#Region " Member Variables "

        <DataMember>
        Private WithEvents mSteps As ObservableCollection(Of clsStep) = New ObservableCollection(Of clsStep)(New List(Of clsStep))

        <DataMember>
        Private mPauseExpression As BPExpression = BPExpression.Empty

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new instance of the clsNavigateStage class and sets its parent
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets an expression detailing the amount of time to wait after
        ''' each step is performed. A null/empty value is treated as 0s.
        ''' </summary>
        Public Property PauseAfterStepExpression As BPExpression
            Get
                Return mPauseExpression
            End Get
            Set(value As BPExpression)
                mPauseExpression = value
            End Set
        End Property

        ''' <summary>
        ''' The steps involved in this stage.
        ''' </summary>
        ''' <value>A List of clsStep objects</value>
        Public ReadOnly Property Steps() As IList(Of clsStep)
            Get
                Return mSteps
            End Get
        End Property

#End Region

        Sub HandleStepCollectionChange(sender As Object, e As NotifyCollectionChangedEventArgs) _
            Handles mSteps.CollectionChanged

            If e.Action = NotifyCollectionChangedAction.Add OrElse
                e.Action = NotifyCollectionChangedAction.Replace Then

                If e.NewItems Is Nothing Then Return
                For Each item As clsStep In e.NewItems
                    If item Is Nothing Then Continue For

                    item.Owner = Me
                Next
            End If

            If e.Action = NotifyCollectionChangedAction.Remove Or
                e.Action = NotifyCollectionChangedAction.Reset Or
                e.Action = NotifyCollectionChangedAction.Replace Then

                If e.OldItems Is Nothing Then Return
                For Each item As clsStep In e.OldItems
                    If item Is Nothing Then Continue For

                    If item.Owner Is Me Then _
                        item.Owner = Nothing
                Next
            End If
        End Sub

#Region " Methods "

        ''' <summary>
        ''' Creates a step from the provided XML element to reside in the given
        ''' stage.
        ''' </summary>
        ''' <param name="el">The XML element containing the information required to
        ''' populate the step</param>
        ''' <param name="appInfo">Information regarding the application in the
        ''' parent process.</param>
        ''' <returns>The step derived from the given XML.</returns>
        Protected MustOverride Function CreateStep(
         el As XmlElement, appInfo As clsApplicationTypeInfo) As clsStep

        ''' <summary>
        ''' Checks this stage for errors
        ''' </summary>
        ''' <param name="bAttemptRepair">True to auto-repair the errors (if possible)
        ''' false to just report them</param>
        ''' <param name="SkipObjects">Has no effect on this implementation</param>
        ''' <returns>A list of validation messages from validating this stage.
        ''' </returns>
        Public Overrides Function CheckForErrors(ByVal bAttemptRepair As Boolean,
         ByVal SkipObjects As Boolean) As ValidationErrorList
            Dim errors As ValidationErrorList =
             MyBase.CheckForErrors(bAttemptRepair, SkipObjects)

            For rowNo As Integer = 1 To Steps.Count
                Dim st As clsStep = Steps(rowNo - 1)

                'Check for rows with no element defined
                If st.ElementId = Guid.Empty Then
                    errors.Add(New ValidateProcessResult(Me, 72, rowNo))
                End If

                'Check for parameter problems
                For j As Integer = 0 To st.Parameters.Count - 1
                    errors.AddRange(
                     st.Parameters(j).CheckForErrors(Me, bAttemptRepair, String.Format(My.Resources.Resources.clsAppStage_ForParameterInRow0, rowNo)))
                Next
            Next

            Return errors
        End Function

        ''' <summary>
        ''' Extracts the target element and element parameters for the given step
        ''' within the specified stage.
        ''' </summary>
        ''' <param name="stp">The step for which the payload is required.</param>
        ''' <param name="stepNo">The step number for logging purposes</param>
        ''' <param name="stg">The stage on which the step is located.</param>
        ''' <returns>The payload containing the target element and list of
        ''' identifiers for the step</returns>
        Friend Shared Function GetPayload(
         ByVal stp As IActionStep, ByVal stepNo As Integer, ByVal stg As clsProcessStage) As Payload
            Dim p As New Payload()

            Dim idents As New List(Of clsIdentifierInfo)
            Dim targetEl As clsApplicationElement =
             stg.Process.ApplicationDefinition.FindElement(stp.ElementId)
            p.TargetElement = targetEl

            'Bail out if target element not found
            If targetEl Is Nothing Then Throw New BluePrismException(
             My.Resources.Resources.clsAppStage_CannotFindTargetElementInStep0Of1, stepNo, stg.DisplayIdentifer)

            ' Get the required attributes for the action (if we have one)
            Dim reqdAttrs As ICollection(Of String) = GetEmpty.ICollection(Of String)()
            If stp.Action IsNot Nothing Then reqdAttrs = stp.Action.RequiredAttributes

            'Populate the list of identifiers
            For Each a As clsApplicationAttribute In targetEl.Attributes
                If a.InUse OrElse reqdAttrs.Contains(a.Name) Then
                    Dim ident As clsIdentifierInfo = a.ToIdentifierInfo()

                    If a.Dynamic Then
                        ident.Value = Nothing
                        'For a dynamic identifier, first find the parameter with a
                        'matching name...
                        For Each pp As clsApplicationElementParameter In stp.Parameters
                            If pp.Name = a.Name Then
                                'Found it, so evaluate the expression and use that
                                'value...
                                Dim res As clsProcessValue = Nothing
                                Dim sErr As String = Nothing
                                If Not clsExpression.EvaluateExpression(pp.Expression, res, stg,
                                 False, Nothing, sErr) Then Throw New BluePrismException(sErr)

                                ident.Value = res.EncodedValue
                                ident.ComparisonType = pp.ComparisonType
                                Exit For
                            End If
                        Next
                        'Make sure we found a parameter in the preceding loop...
                        If ident.Value Is Nothing Then Throw New BluePrismException(
                         My.Resources.Resources.clsAppStage_MissingParameterForDynamicIdentifier0In1,
                         a.Name, stg.DisplayIdentifer)

                    End If

                    idents.Add(ident)
                End If
            Next

            Dim customIdentifiers = targetEl.GetSupplementaryIdentifiers()
            idents.AddRange(customIdentifiers)
            p.Identifiers = idents

            Return p

        End Function

        ''' <summary>
        ''' Gets the payload for the specified step
        ''' </summary>
        ''' <param name="stp">The step for which the payload is required.</param>
        ''' <returns>The payload of the step - ie. the target element and the list of
        ''' identifiers</returns>
        Friend Function GetPayload(ByVal stp As clsStep) As Payload
            Return GetPayload(stp, Steps.IndexOf(stp) + 1, Me)
        End Function

        ''' <summary>
        ''' Gets the arguments for the given action step.
        ''' </summary>
        ''' <param name="stp">The step for which arguments are required. If no action
        ''' is assigned to the step (eg. write stages are not associated with an
        ''' action) then an empty dictionary is returned, otherwise the registered
        ''' arguments for the step are populated into the returned dictionary</param>
        ''' <returns>A dictionary containing the arguments for the given step - the
        ''' argument values mapped against their corresponding name.</returns>
        Protected Overridable Function GetArgs(ByVal stp As IActionStep) _
         As Dictionary(Of String, String)
            ' We only get the arguments if an action is registered at the moment -
            ' Write steps don't actually support any arguments (nor have an action
            ' registered)
            If stp.Action Is Nothing Then Return New Dictionary(Of String, String)
            Return clsActionStep.GetArguments(stp, Me)
        End Function

        Protected Function GetArgumentOutputs(ByVal stp As IActionStep) As Dictionary(Of String, String)
            ' We only get the arguments if an action is registered at the moment -
            ' Write steps don't actually support any arguments (nor have an action
            ' registered)
            If stp.Action Is Nothing Then Return New Dictionary(Of String, String)
            Return clsActionStep.GetOutputs(stp, Me)
        End Function

        ''' <summary>
        ''' Gets the resultant step interval held in this stage after resolving any
        ''' function calls or data items in the <see cref="PauseAfterStepExpression"/>.
        ''' </summary>
        ''' <returns>The step interval which results from the evaluation of the
        ''' expression given in <see cref="PauseAfterStepExpression"/></returns>
        ''' <exception cref="InvalidFormatException">If the expression could not be
        ''' evaluated.</exception>
        ''' <exception cref="InvalidTypeException">If the expression could be
        ''' evaluated, but resulted in a value of a type other than
        ''' <see cref="DataType.number"/> or <see cref="DataType.timespan"/>
        ''' </exception>
        Protected Function GetPauseAfterStep() As TimeSpan

            Dim expr = PauseAfterStepExpression
            If expr.IsEmpty Then Return TimeSpan.Zero

            Dim val As clsProcessValue = Evaluate(expr)
            If val.DataType = DataType.number Then
                Return TimeSpan.FromSeconds(CDbl(val))

            ElseIf val.DataType = DataType.timespan Then
                Return CType(val, TimeSpan)

            Else
                Throw New InvalidTypeException(
                    My.Resources.Resources.clsAppStage_PauseAfterStepMustEvaluateToATimespanOrANumberOfSeconds0EvaluatedToATypeOf1,
                    expr, val.DataType)

            End If
        End Function

        Friend Function RunQuery(logger As CompoundLoggingEngine, ByVal stp As IActionStep,
         ByVal p As Payload, ByVal args As Dictionary(Of String, String)) As String
            Dim res As String = Nothing
            Dim errmsg As clsAMIMessage = Nothing
            Dim ami As clsAMI = GetAMIForExecution()
            If stp.ActionId Is Nothing Then
                Throw New BluePrismException(My.Resources.Resources.clsAppStage_Step0In1HasNoActionAssigned,
                 1 + Steps.IndexOf(DirectCast(stp, clsStep)), DisplayIdentifer)
            End If
            If ami.DoAction(stp.ActionId, p.TargetElement.Type, p.Identifiers, args, res, errmsg) Then _
             Return res


            ' DoAction failed...
            If p.TargetElement.Diagnose Then
                For Each a As clsActionTypeInfo In mParent.ApplicationDefinition.DiagnosticActions
                    Dim diag As String = Nothing
                    Dim diagErr As clsAMIMessage = Nothing
                    If ami.DoDiagnosticAction(a.ID, diag, diagErr) Then
                        LogDiagnostic(logger, diag)
                    Else
                        Throw New BluePrismException(diagErr.Message)
                    End If
                Next
            End If
            Throw New BluePrismException(My.Resources.Resources.clsAppStage_FailedToPerformStep0In12,
             1 + Steps.IndexOf(DirectCast(stp, clsStep)), DisplayIdentifer, errmsg.Message)

        End Function

        Private Sub LogDiagnostic(logger As CompoundLoggingEngine, ByVal sMessage As String)
            Dim info = GetLogInfo()
            logger.LogDiagnostic(info, Me, sMessage)
        End Sub

        ''' <summary>
        ''' Gets the AMI instance to be used for execution of this stage, and
        ''' raises an appropriate exception if that instance is not available.
        ''' </summary>
        ''' <returns>The AMI instance to be used for execution of this stage.
        ''' </returns>
        ''' <exception cref="BluePrismException">If AMI is unavailable for use in
        ''' executing this stage.</exception>
        Private Function GetAMIForExecution() As clsAMI
            Dim ami As clsAMI = mParent.AMI
            If ami Is Nothing Then Throw New BluePrismException(
             My.Resources.Resources.clsAppStage_CanTExecute0AsApplicationManagerIsNotAvailable, DisplayIdentifer)
            Return ami
        End Function

        ''' <summary>
        ''' Generates xml for the navigations, adding each navigation as a child
        ''' node to the element e.
        ''' </summary>
        ''' <param name="doc">The parent document with which to create
        ''' new elements.</param>
        ''' <param name="el">The element to which the navigations will be
        ''' appended.</param>
        Public Overrides Sub ToXML(
         doc As XmlDocument, el As XmlElement, selOnly As Boolean)
            MyBase.ToXml(doc, el, selOnly)

            If Not PauseAfterStepExpression.IsEmpty Then el.SetAttribute(
                "interval", PauseAfterStepExpression.NormalForm)

            For Each n As clsStep In Steps
                Dim stepEl As XmlElement = doc.CreateElement("step")
                n.ToXML(doc, stepEl)
                el.AppendChild(stepEl)
            Next

        End Sub

        ''' <summary>
        ''' Generates a list of navigation action objects as represented
        ''' in the supplied xml.
        ''' </summary>
        ''' <param name="e">An xml element with root node "navigations"</param>
        Public Overrides Sub FromXML(ByVal e As XmlElement)
            MyBase.FromXML(e)

            PauseAfterStepExpression =
                BPExpression.FromNormalised(e.GetAttribute("interval"))

            Dim appInfo = ApplicationInfo
            For Each el As XmlElement In e.ChildNodes
                If el.Name = "step" Then Steps.Add(CreateStep(el, appInfo))
            Next
        End Sub

        ''' <summary>
        ''' Creates a deep clone of this application stage.
        ''' </summary>
        ''' <returns>An app stage with the same values set in it as this stage.
        ''' </returns>
        Public Overrides Function Clone() As clsProcessStage
            Dim copy = DirectCast(MyBase.Clone(), clsAppStage)

            ' We can use the same ref since BPExpression is semantically immutable
            copy.PauseAfterStepExpression = PauseAfterStepExpression

            For Each n As clsStep In Steps
                copy.Steps.Add(n.Clone())
            Next

            Return copy
        End Function

        ''' <summary>
        ''' Returns items referred to by this stage, so externally defined things
        ''' (such as model elements from a shared model) and when required things
        ''' defined within the process (e.g. data items and model elements where
        ''' model sharing not in use).
        ''' </summary>
        ''' <param name="inclInternal">Indicates internal references required</param>
        ''' <returns>List of dependencies</returns>
        Public Overrides Function GetDependencies(inclInternal As Boolean) As List(Of clsProcessDependency)
            Dim deps As List(Of clsProcessDependency) = MyBase.GetDependencies(inclInternal)

            For Each stp As clsStep In Steps
                If stp.ElementId <> Guid.Empty Then
                    'Model elements only returned as external references if
                    'there is a parent object
                    If mParent.ParentObject IsNot Nothing Then
                        deps.Add(New clsProcessElementDependency(mParent.ParentObject, stp.ElementId))
                    ElseIf inclInternal Then
                        deps.Add(New clsProcessElementDependency(mParent.Name, stp.ElementId))
                    End If
                End If

                For Each arg As KeyValuePair(Of String, String) In stp.ArgumentValues
                    If arg.Key = "font" AndAlso arg.Value <> String.Empty Then
                        deps.Add(New clsProcessFontDependency(arg.Value.TrimStart(""""c).TrimEnd(""""c)))
                    End If

                    If inclInternal Then
                        For Each dataItem As String In Core.Expressions.BPExpression.FromNormalised(arg.Value).GetDataItems()
                            Dim outOfScope As Boolean
                            Dim stage = mParent.GetDataStage(dataItem, Me, outOfScope)
                            If Not outOfScope AndAlso stage IsNot Nothing Then _
                                deps.Add(New clsProcessDataItemDependency(stage))
                        Next
                    End If
                Next

                If inclInternal Then
                    For Each param As clsApplicationElementParameter In stp.Parameters
                        For Each dataItem As String In param.Expression.GetDataItems()
                            Dim outOfScope As Boolean
                            Dim stage = mParent.GetDataStage(dataItem, Me, outOfScope)
                            If Not outOfScope AndAlso stage IsNot Nothing Then _
                                deps.Add(New clsProcessDataItemDependency(stage))
                        Next
                    Next
                End If
            Next

            Return deps
        End Function

#End Region

    End Class

End Namespace

