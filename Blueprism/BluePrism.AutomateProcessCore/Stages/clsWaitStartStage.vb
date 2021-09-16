Imports System.Drawing
Imports System.Linq
Imports System.Runtime.Serialization
Imports System.Threading
Imports System.Xml
Imports BluePrism.AMI
Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.BPCoreLib
Imports BluePrism.Core.Expressions

Namespace Stages

    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsWaitStartStage
    ''' 
    ''' <summary>
    ''' The Wait start stage represents the start of a wait. A wait is like a choice
    ''' in that it is like a multiple decision, the decision that gets executed depends
    ''' on the event that is being waited for.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsWaitStartStage
        Inherits clsChoiceStartStage

        <DataMember>
        Private msTimeout As String

        ''' <summary>
        ''' Creates a new instance of the clsWaitStartStage class and sets its parent.
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
            SetDisplayWidth(30)
            SetDisplayHeight(30)
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of an Wait start stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsWaitStartStage(mParent)
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.WaitStart
            End Get
        End Property

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

            For Each c As clsWaitChoice In Choices
                If c.ElementID <> Guid.Empty Then
                    If mParent.ParentObject IsNot Nothing Then
                        deps.Add(New clsProcessElementDependency(mParent.ParentObject, c.ElementID))
                    ElseIf inclInternal Then
                        deps.Add(New clsProcessElementDependency(mParent.Name, c.ElementID))
                    End If
                End If

                For Each arg As KeyValuePair(Of String, String) In c.ArgumentValues
                    If arg.Key = "font" AndAlso arg.Value <> String.Empty Then
                        deps.Add(New clsProcessFontDependency(arg.Value.TrimStart(""""c).TrimEnd(""""c)))
                    End If

                    If inclInternal Then
                        For Each dataItem As String In BPExpression.FromNormalised(arg.Value).GetDataItems()
                            Dim outOfScope As Boolean
                            Dim stage = mParent.GetDataStage(dataItem, Me, outOfScope)
                            If Not outOfScope AndAlso stage IsNot Nothing Then _
                                deps.Add(New clsProcessDataItemDependency(stage))
                        Next
                    End If
                Next

                If inclInternal Then
                    For Each param As clsApplicationElementParameter In c.Parameters
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

        ''' <summary>
        ''' Provides access to the timeout expression, which when evaluated expresses
        ''' the timeout time in seconds or a timespan.
        ''' </summary>
        Public Property Timeout() As String
            Get
                Return msTimeout
            End Get
            Set(ByVal value As String)
                msTimeout = value
            End Set
        End Property

        ''' <summary>
        ''' Get a list of the application elements used by this wait stage.
        ''' </summary>
        ''' <returns>A List of the element IDs.</returns>
        Public Function GetElementsUsed() As List(Of Guid)
            Dim l As New List(Of Guid)
            For Each c As clsWaitChoice In Choices
                If Not l.Contains(c.ElementID) Then
                    l.Add(c.ElementID)
                End If
            Next
            Return l
        End Function

        Public Overrides Function Execute(ByRef stgId As Guid, logger As CompoundLoggingEngine) As StageResult

            Try
                Dim sErr As String = Nothing
                WaitPrologue(logger)

                'Let's failsafe and timeout immediately if not specified
                Dim millis As Integer = 0

                If Timeout <> "" Then
                    Dim val As clsProcessValue = Nothing
                    If Not clsExpression.EvaluateExpression(Timeout, val, Me, False, Nothing, sErr) Then _
                     Return StageResult.InternalError(String.Format(My.Resources.Resources.clsWaitStartStage_InvalidTimeoutExpression0, sErr))

                    If val.DataType = DataType.number Then
                        millis = CInt(CDec(val) * 1000D)

                    ElseIf val.DataType = DataType.timespan Then
                        millis = CInt(CType(val, TimeSpan).TotalMilliseconds)

                    Else
                        Return StageResult.InternalError(My.Resources.Resources.clsWaitStartStage_TimeoutExpressionMustProduceATimespanOrANumberOfSeconds)

                    End If

                    If millis < 0 Then Return StageResult.InternalError(
                     My.Resources.Resources.clsWaitStartStage_TheTimeoutValueCannotBeNegativeFound0,
                     val.FormattedValue)
                End If

                'Bug 2719 - Wait queries should be allowed without having an AMI instance
                'which is connected to a target application. Thus we check for
                'stages configured deliberately as a simple timeout:
                If Choices.Count = 0 Then

                    Dim sw As New Stopwatch()
                    sw.Start()

                    ' if iTimeout < 1s, only wait for iTimeout
                    Dim waitChunk As Integer = Math.Min(1000, millis)

                    ' Keep waiting until the requested timeout is reached or a stop request
                    ' is made to the process (see bug 6088)
                    While sw.ElapsedMilliseconds < millis
                        If mParent.ImmediateStopRequested Then Return StageResult.InternalError(
                         My.Resources.Resources.clsWaitStartStage_WaitStageAbandonedDueToAnInterruptRequest)
                        Thread.Sleep(waitChunk)
                    End While

                    'Move on to next stage
                    Dim ceStg As clsChoiceEndStage = mParent.GetChoiceEnd(Me)
                    stgId = ceStg.Id
                    If stgId.Equals(Guid.Empty) Then Return StageResult.InternalError(
                     My.Resources.Resources.clsWaitStartStage_MissingLinkFromTimeout)


                    WaitEpilogue(logger, ceStg.Name, 1)
                    Return StageResult.OK
                End If

                'Get reference to AMI
                Dim ami As clsAMI
                ami = mParent.AMI()
                If ami Is Nothing Then Return StageResult.InternalError(
                 My.Resources.Resources.clsWaitStartStage_CanTExecuteStage0AsApplicationManagerIsNotAvailable, Name)

                Dim conditions As New List(Of clsWaitInfo)
                For Each c As clsWaitChoice In Choices
                    Dim rowNum As Integer = Choices.IndexOf(c) + 1

                    'Create list of identifiers
                    Dim p As clsAppStage.Payload = clsAppStage.GetPayload(c, rowNum, Me)

                    'Get list of arguments to navigation
                    If c.Condition Is Nothing Then Return StageResult.InternalError(
                     My.Resources.Resources.clsWaitStartStage_Step0Of1HasNoConditionAssigned, rowNum, DisplayIdentifer)

                    Dim args As Dictionary(Of String, String) = clsActionStep.GetArguments(c, Me)

                    'We evaluate the expected reply into a literal value
                    Dim res As clsProcessValue = Nothing
                    clsExpression.EvaluateExpression(c.ExpectedReply, res, Me, False, Nothing, sErr)
                    If res Is Nothing Then Return StageResult.InternalError(
                     My.Resources.Resources.clsWaitStartStage_TheExpectedReply0In1IsNotAValidExpression,
                     c.ExpectedReply, DisplayIdentifer)

                    ' Get the datatype of the condition and compare it to the evaluated expression
                    Dim dt As DataType = clsProcessDataTypes.Parse(c.Condition.DataType)
                    If res.DataType <> dt Then Return StageResult.InternalError(
                     My.Resources.Resources.clsWaitStartStage_DataTypeOfCondition0DoesNotMatchDatatypeOfExpectedReply, rowNum)

                    conditions.Add(New clsWaitInfo(p.TargetElement.Type, p.Identifiers,
                     args, c.Condition, res.EncodedValue, c.ComparisonType))
                Next

                Dim errmsg As clsAMIMessage = Nothing
                Dim choiceNo As Integer = ami.DoWait(conditions, millis, errmsg)

                ' If an AMI error occurred (-1 returned)
                If choiceNo = -1 Then Return StageResult.InternalError(
                 My.Resources.Resources.clsWaitStartStage_AMIErrorOccurredIn01, DisplayIdentifer, errmsg.Message)

                ' If a timeout occurred (0 returned)
                If choiceNo = 0 Then
                    Dim ceStg As clsChoiceEndStage = mParent.GetChoiceEnd(Me)
                    stgId = ceStg.Id
                    If stgId = Guid.Empty Then Return StageResult.InternalError(
                     My.Resources.Resources.clsWaitStartStage_MissingLinkFromTimeoutIn0, DisplayIdentifer)

                    WaitEpilogue(logger, ceStg.Name, Choices.Count + 1)

                    Return StageResult.OK

                End If

                ' Otherwise the return value is the 1-based index of the choice
                ' whose condition was met

                If choiceNo > Choices.Count Then Return StageResult.InternalError(
                 My.Resources.Resources.clsWaitStartStage_ApplicationManagerReturnedAChoiceIndexBeyondTheNumberOfChoicesInTheStage01, choiceNo, Choices.Count)

                Dim choice As clsWaitChoice = TryCast(Choices(choiceNo - 1), clsWaitChoice)

                If choice.LinkTo = Guid.Empty Then Return StageResult.InternalError(
                 My.Resources.Resources.clsWaitStartStage_MissingLinkFromWaitChoice01, choiceNo, choice.Name)

                stgId = choice.LinkTo

                WaitEpilogue(logger, choice.Name, choiceNo)
                Return StageResult.OK

            Catch ex As Exception
                Return StageResult.InternalError(ex)

            End Try

        End Function

        Private Sub WaitPrologue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.WaitPrologue(info, Me)
        End Sub

        Private Sub WaitEpilogue(logger As CompoundLoggingEngine, ByVal sChoiceTaken As String, ByVal iChoiceNumber As Integer)
            Dim info = GetLogInfo()
            logger.WaitEpiLogue(info, Me, sChoiceTaken, iChoiceNumber)
        End Sub

        Public Overrides Function CreateChoice() As clsChoice
            Return New clsWaitChoice(Me)
        End Function

        Public Overrides Function Clone() As clsProcessStage
            Dim copy As clsWaitStartStage = TryCast(MyBase.Clone(), clsWaitStartStage)
            copy.Timeout = Me.Timeout
            Return copy
        End Function

        Public Overrides Sub ToXml(ByVal doc As XmlDocument,
         ByVal stageEl As XmlElement, ByVal bSelectionOnly As Boolean)
            MyBase.ToXml(doc, stageEl, bSelectionOnly)
            BPUtil.AppendTextElement(stageEl, "timeout", Timeout)
        End Sub

        Public Overrides Sub FromXML(ByVal el As XmlElement)
            MyBase.FromXML(el)
            For Each child As XmlElement In el.ChildNodes
                If child.Name = "timeout" Then Timeout = child.InnerText
            Next
        End Sub

        Private Function CheckRowForErrors(ByVal row As clsWaitChoice, ByVal rowNo As Integer, ByVal attemptRepair As Boolean) As ValidationErrorList
            Dim errors As New ValidationErrorList

            Dim targetElement As clsApplicationElement = Process.ApplicationDefinition.FindElement(row.ElementID)
            If targetElement Is Nothing Then
                errors.Add(New ValidateProcessResult(Me, "frmStagePropertiesWait.htm", 68, rowNo))
                Return errors 'exit early to avoid null reference - other checks depend on this one
            End If

            If row.Condition Is Nothing Then
                errors.Add(New ValidateProcessResult(Me, "frmStagePropertiesWait.htm", 69, rowNo))
                Return errors 'exit early to avoid null reference exception
            End If

            'Check that the selected condition is appropriate to the selected element
            Dim alternative As String = Nothing, helpMessage As String = Nothing, helpTopic As Integer
            If clsAMI.IsValidCondition(row.Condition.ID, targetElement.Type, mParent.ApplicationDefinition.ApplicationInfo, alternative, helpMessage, helpTopic) Then
                If alternative IsNot Nothing Then
                    If attemptRepair Then
                        row.Condition = clsAMI.GetConditionTypeInfo(alternative)
                    Else
                        Dim helpReference As String
                        If helpTopic > 0 Then
                            helpReference = "helpTopicsByNumber.htm#Topic" & helpTopic.ToString
                        Else
                            helpReference = "frmStagePropertiesWait.htm"
                        End If
                        errors.Add(New RepairableValidateProcessResult(Me, helpReference, 70, rowNo))
                    End If
                End If
            Else
                errors.Add(New ValidateProcessResult(Me, "frmStagePropertiesWait.htm", 77, rowNo))
            End If

            'Check params
            errors.AddRange(row.Parameters.SelectMany(Function(p) p.CheckForErrors(row.OwningStage, attemptRepair, String.Format(My.Resources.Resources.clsWaitStartStage_ForParameterInRow0, rowNo))))

            'Check expression
            errors.AddRange(clsExpression.CheckExpressionForErrors(row.ExpectedReply, Me, clsProcessDataTypes.DataTypeId(row.Condition.DataType), String.Format(My.Resources.Resources.clsWaitStartStage_InRow0, rowNo), Nothing, Nothing))

            Return errors
        End Function

        Public Overrides Function CheckForErrors(ByVal bAttemptRepair As Boolean, ByVal SkipObjects As Boolean) As ValidationErrorList
            Dim errors As ValidationErrorList = MyBase.CheckForErrors(bAttemptRepair, SkipObjects)

            For rowNo As Integer = 1 To Me.Choices.Count
                Dim choice = DirectCast(Me.Choices(rowNo - 1), clsWaitChoice)
                errors.AddRange(CheckRowForErrors(choice, rowNo, bAttemptRepair))
            Next

            Dim res As clsProcessValue = Nothing
            Dim expressionInfo As clsExpressionInfo = Nothing
            errors.AddRange(clsExpression.CheckExpressionForErrors(Me.Timeout, Me, DataType.number, My.Resources.Resources.clsWaitStartStage_ForTheTimeout, expressionInfo, res))

            Return errors
        End Function

        Friend Shared Sub DrawWatch(ByVal r As IRender, ByVal b As RectangleF)
            Dim p(5) As PointF
            With b
                p(0).X = .Left + (.Width / 2)
                p(0).Y = .Top + (.Height / 2)

                p(1).X = .Left + (.Width / 24)
                p(1).Y = .Top + (.Height / 8)

                p(2).X = .Left + (.Width / 8)
                p(2).Y = .Top + (.Height / 24)

                p(3).X = .Left + (.Width / 2)
                p(3).Y = .Top + (.Height / 2)

                p(4).X = .Right - (.Width / 8)
                p(4).Y = .Top + (.Height / 24)

                p(5).X = .Right - (.Width / 24)
                p(5).Y = .Top + (.Height / 8)

                r.DrawLine(New PointF(.Left, .Top + (.Height / 8)), New PointF(.Left + (.Width / 8), .Top))
                r.DrawLine(New PointF(.Right, .Top + (.Height / 8)), New PointF(.Right - (.Width / 8), .Top))
            End With
            r.DrawPolygon(p)

            r.FillEllipse(b)
            r.DrawEllipse(b)
        End Sub
    End Class

End Namespace
