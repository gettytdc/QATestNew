Imports System.Xml
Imports System.Drawing
Imports System.Runtime.Serialization

Namespace Stages
    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsChoiceStartStage
    '''
    ''' <summary>
    ''' The choice start stage represents the start of a choice. A choice
    ''' is like a multiple decision.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsChoiceStartStage
        Inherits clsGroupStage

        ''' <summary>
        ''' A List of choices (clsChoice) attached to this stage.
        ''' </summary>
        Public ReadOnly Property Choices() As List(Of clsChoice)
            Get
                If mChoices Is Nothing Then mChoices = New List(Of clsChoice)
                Return mChoices
            End Get
        End Property
        Protected mChoices As List(Of clsChoice)

        ''' <summary>
        ''' Creates a new instance of the clsChoiceStartStage class and sets its
        ''' parent.
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of an Choice Start stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsChoiceStartStage(mParent)
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.ChoiceStart
            End Get
        End Property

        ''' <summary>
        ''' Returns items referred to by this stage, curently only things defined
        ''' within the process (e.g. data items).
        ''' </summary>
        ''' <param name="inclInternal">Indicates internal references required</param>
        ''' <returns>List of dependencies</returns>
        Public Overrides Function GetDependencies(inclInternal As Boolean) As List(Of clsProcessDependency)
            Dim deps As List(Of clsProcessDependency) = MyBase.GetDependencies(inclInternal)

            If inclInternal Then
                For Each c As clsChoice In Choices
                    For Each dataItem As String In c.Expression.GetDataItems()
                        Dim outOfScope As Boolean
                        Dim stage = mParent.GetDataStage(dataItem, Me, outOfScope)
                        If Not outOfScope AndAlso stage IsNot Nothing Then _
                            deps.Add(New clsProcessDataItemDependency(stage))
                    Next
                Next
            End If

            Return deps
        End Function

        ''' <summary>
        ''' Clones the stage.
        ''' </summary>
        ''' <returns>Returns a deep clone of this stage.</returns>
        Public Overrides Function Clone() As clsProcessStage
            Dim copy = DirectCast(MyBase.Clone(), clsChoiceStartStage)

            ' Force a new list to be created by the property
            copy.mChoices = Nothing
            For Each choice As clsChoice In Me.Choices
                Dim choiceCopy As clsChoice = choice.Clone()
                choiceCopy.OwningStage = copy
                copy.Choices.Add(choiceCopy)
            Next
            Return copy
        End Function

        ''' <summary>
        ''' Determine if this stage links (in a forwards direction) to another stage.
        ''' </summary>
        ''' <param name="st">The stage to check against</param>
        ''' <returns>True if there is a link, False otherwise.</returns>
        Public Overrides Function LinksTo(ByVal st As clsProcessStage) As Boolean
            For Each ch As clsChoice In Choices
                If ch.LinkTo = st.Id Then Return True
            Next
            Return (mParent.GetChoiceEnd(Me).Id = st.Id)
        End Function

        ''' <summary>
        ''' Gets a collection of stages that this stage links to. In this case it is
        ''' all the choices on true stages.
        ''' </summary>
        ''' <returns>A collection of stages that this stage links to</returns>
        Friend Overrides Function GetLinks() As ICollection(Of clsProcessStage)
            Dim links As New List(Of clsProcessStage)
            For Each ch As clsChoice In Choices
                Dim linksto As clsProcessStage = mParent.GetStage(ch.LinkTo)
                If linksto IsNot Nothing Then links.Add(linksto)
            Next
            links.Add(mParent.GetChoiceEnd(Me))
            Return links
        End Function


        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult

            Dim sErr As String = Nothing
            ChoicePrologue(logger)

            For Each choice As clsChoice In Choices
                'Evaluate the decision, and move to the
                'next appropriate stage...
                Dim res As clsProcessValue = Nothing

                'Evaluate the expression...
                If Not clsExpression.EvaluateExpression(choice.Expression, res, Me, False, Nothing, sErr) Then _
     Return StageResult.InternalError(sErr)

                'The expression should have returned the correct data
                'type, and consequently the value should be either True
                'or False, so trap the error now if it isn't...
                If res.DataType <> DataType.flag OrElse res.IsNull Then _
                 Return StageResult.InternalError(My.Resources.Resources.clsChoiceStartStage_DecisionDidNotResultInAYesNoAnswer)

                Dim val As Boolean = CBool(res)

                ' Have we found our exit? If not, keep looking
                If Not val Then Continue For

                ' Apparently, we have; move onto the next stage and return success
                Dim nextStageId As Guid = choice.LinkTo
                If nextStageId = Nothing Then _
                 Return StageResult.InternalError(My.Resources.Resources.clsChoiceStartStage_MissingLinkFromChoice)

                gRunStageID = nextStageId

                ChoiceEpilogue(logger, choice.Name, Choices.IndexOf(choice) + 1)

                Return StageResult.OK
            Next
            Dim choiceEndStg As clsChoiceEndStage = mParent.GetChoiceEnd(Me)
            gRunStageID = choiceEndStg.Id
            If gRunStageID = Nothing Then _
             Return StageResult.InternalError(My.Resources.Resources.clsChoiceStartStage_MissingLinkFromChoiceEnd)

            ChoiceEpilogue(logger, choiceEndStg.Name, Choices.Count + 1)

            Return StageResult.OK

        End Function

        Private Sub ChoicePrologue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.ChoicePrologue(info, Me)
        End Sub

        Private Sub ChoiceEpilogue(logger As CompoundLoggingEngine, ByVal sChoiceTaken As String, ByVal iChoiceNumber As Integer)
            Dim info = GetLogInfo()
            logger.ChoiceEpiLogue(info, Me, sChoiceTaken, iChoiceNumber)
        End Sub

        Public Overrides Sub FromXML(ByVal choiceEl As XmlElement)
            MyBase.FromXML(choiceEl)
            For Each el As XmlElement In choiceEl.ChildNodes
                If el.Name <> "choices" Then Continue For
                Dim choiceStart As clsChoiceStartStage =
                 TryCast(Me, clsChoiceStartStage)

                For Each eChoice As XmlElement In el.ChildNodes
                    Select Case eChoice.Name
                        Case "choice"
                            Dim c As clsChoice = CreateChoice()
                            c.FromXML(eChoice)
                            c.OwningStage = Me
                            choiceStart.Choices.Add(c)
                    End Select
                Next
            Next
        End Sub

        ''' <summary>
        ''' Creates a new choice for this start stage
        ''' </summary>
        ''' <returns>A choice of the correct type to be added to this start stage.
        ''' </returns>
        Public Overridable Function CreateChoice() As clsChoice
            Return New clsChoice(Me)
        End Function

        Public Overrides Sub ToXml(ByVal doc As XmlDocument,
         ByVal el As XmlElement, ByVal bSelectionOnly As Boolean)
            MyBase.ToXml(doc, el, bSelectionOnly)

            Dim choicesEl As XmlElement = doc.CreateElement("choices")
            For Each objChoice As clsChoice In Me.Choices
                Dim choiceEl As XmlElement = doc.CreateElement("choice")
                objChoice.ToXML(doc, choiceEl, bSelectionOnly)
                choicesEl.AppendChild(choiceEl)
            Next
            el.AppendChild(choicesEl)

        End Sub


        Public Overrides Function CheckForErrors(ByVal bAttemptRepair As Boolean, ByVal SkipObjects As Boolean) As ValidationErrorList
            Dim errors As ValidationErrorList = MyBase.CheckForErrors(bAttemptRepair, SkipObjects)
            Dim ContainingPage As clsProcessSubSheet = Me.SubSheet

            'Check each node in turn
            For rowNo As Integer = 1 To Me.Choices.Count
                Dim node As clsChoice = Me.Choices(rowNo - 1)
                Dim nodeDesc As String = String.Format(
                 My.Resources.Resources.clsChoiceStartStage_Node0OfStage1OnPage2, rowNo, GetName(), Me.SubSheet.Name)


                ' Validate expression for choice only - not wait, which inherits
                ' from this (bonkers!)
                If StageType = StageTypes.ChoiceStart Then
                    errors.AddRange(clsExpression.CheckExpressionForErrors(
                     node.Expression, Me, DataType.flag, String.Format(My.Resources.Resources.clsChoiceStartStage_For0, nodeDesc), Nothing, Nothing))
                End If

                'Warn if node has no name - but not critical
                If String.IsNullOrEmpty(node.Name) Then
                    errors.Add(New ValidateProcessResult(Me, "frmStagePropertiesChoice.htm", 3, nodeDesc))
                End If

                'Warn if link is empty
                If node.LinkTo = Guid.Empty Then
                    errors.Add(New ValidateProcessResult(Me, 4, nodeDesc))
                Else
                    'Warn if link is invalid
                    Dim targetStage As clsProcessStage = mParent.GetStage(node.LinkTo)
                    If targetStage Is Nothing Then
                        errors.Add(New ValidateProcessResult(Me, 5, nodeDesc))
                    Else
                        'Warn if link goes to another page
                        If Not targetStage.GetSubSheetID().Equals(Me.GetSubSheetID()) Then
                            errors.Add(New ValidateProcessResult(Me, 6, nodeDesc))
                        End If
                    End If
                End If
            Next

            Return errors
        End Function

        ''' <summary>
        ''' A missing link is not considered an error on choice start
        ''' </summary>
        Protected Overrides ReadOnly Property AllowsMissingLinks() As Boolean
            Get
                Return True
            End Get
        End Property

        ''' <summary>
        ''' Gets the minimum acceptable distance of a node from the
        ''' centre of this start stage.
        ''' </summary>
        ''' <returns>Returns the offset in world coordinates of the
        ''' minimum acceptable <see cref="clsChoice.Distance">distance</see>
        ''' of a node from this choice start stage.</returns>
        Public Function GetMinimumNodeDistance() As Single
            Dim objChoiceEnd As clsChoiceEndStage = mParent.GetChoiceEnd(Me)

            ' ChoiceEnd may not exist if pasting - this method is called when
            ' setting the distance of the stage, ie. when parsing the XML, so the
            ' process may have no stages defined at the moment, hence no end stage
            If objChoiceEnd Is Nothing Then Return 0

            Dim src As PointF = Me.Location
            Dim dest As PointF = objChoiceEnd.Location
            Dim dx As Single = dest.X - src.X
            Dim dy As Single = dest.Y - src.Y
            Dim LineGradient As Double = -dy / dx 'gradient of the line joining the two stage centres

            Dim EllipseHalfWidth As Single = Me.GetDisplayWidth / 2
            Dim EllipseHalfHeight As Single = Me.GetDisplayHeight / 2
            Dim SqWidth As Double = Math.Pow(EllipseHalfWidth, 2)
            Dim SqHeight As Double = Math.Pow(EllipseHalfHeight, 2)

            Dim StartStageIntersectionOffsetX As Double
            If Double.IsNaN(LineGradient) Then
                StartStageIntersectionOffsetX = 0
            Else
                StartStageIntersectionOffsetX = Math.Sqrt((SqWidth * SqHeight) / (SqHeight + (SqWidth * Math.Pow(LineGradient, 2))))
            End If

            If dx < 0 Then StartStageIntersectionOffsetX *= -1
            Dim StartStageIntersectionX As Double = src.X + StartStageIntersectionOffsetX
            Dim Multiplier As Integer = 1
            If dx = 0 AndAlso dy > 0 Then Multiplier = -1
            Dim StartStageIntersectionY As Double = src.Y + CDbl(IIf(dx = 0, Multiplier * EllipseHalfHeight, -LineGradient * StartStageIntersectionOffsetX))

            Dim MinDistance As Single = CSng(Math.Sqrt(Math.Pow(StartStageIntersectionX - src.X, 2) + Math.Pow(StartStageIntersectionY - src.Y, 2)))
            Return MinDistance + CSng(clsChoice.DisplayWidth / 2)
        End Function


        ''' <summary>
        ''' Gets the maximum acceptable distance of a node from the
        ''' centre of this start stage.
        ''' </summary>
        ''' <returns>Returns the offset in world coordinates of the
        ''' maximum acceptable <see cref="clsChoice.Distance">distance</see>
        ''' of a node from this choice start stage.</returns>
        Public Function GetMaximumNodeDistance() As Single
            Dim objChoiceEnd As clsChoiceEndStage = mParent.GetChoiceEnd(Me)

            ' ChoiceEnd may not exist if pasting - this method is called when
            ' setting the distance of the stage, ie. when parsing the XML, so the
            ' process may have no stages defined at the moment, hence no end stage
            If objChoiceEnd Is Nothing Then Return 0

            Dim src As PointF = Me.Location
            Dim dest As PointF = objChoiceEnd.Location
            Dim dx As Single = dest.X - src.X
            Dim dy As Single = dest.Y - src.Y
            Dim LineGradient As Double = -dy / dx 'gradient of the line joining the two stage centres

            Dim EllipseHalfWidth As Single = objChoiceEnd.GetDisplayWidth / 2
            Dim EllipseHalfHeight As Single = objChoiceEnd.GetDisplayHeight / 2
            Dim SqWidth As Double = Math.Pow(EllipseHalfWidth, 2)
            Dim SqHeight As Double = Math.Pow(EllipseHalfHeight, 2)

            Dim EndStageIntersectionOffsetX As Double
            If Double.IsNaN(LineGradient) Then
                EndStageIntersectionOffsetX = 0
            Else
                EndStageIntersectionOffsetX = -Math.Sqrt((SqWidth * SqHeight) / (SqHeight + (SqWidth * Math.Pow(LineGradient, 2))))
            End If

            If dx < 0 Then EndStageIntersectionOffsetX *= -1
            Dim EndStageIntersectionX As Double = dest.X + EndStageIntersectionOffsetX
            Dim Multiplier As Integer = -1
            If dx = 0 AndAlso dy > 0 Then Multiplier = 1
            Dim EndStageIntersectionY As Double = dest.Y - CDbl(IIf(dx = 0, Multiplier * EllipseHalfHeight, LineGradient * EndStageIntersectionOffsetX))

            Dim MaxDistance As Single = CSng(Math.Sqrt(Math.Pow(EndStageIntersectionX - src.X, 2) + Math.Pow(EndStageIntersectionY - src.Y, 2)))
            Return MaxDistance - CSng(clsChoice.DisplayWidth / 2)
        End Function

        ''' <summary>
        ''' Gets the range of distances permitted for the specified node.
        ''' </summary>
        ''' <param name="NodeIndex">The zero-based index of the node of
        ''' interest.</param>
        ''' <returns>Returns an array of length 2: the first item is the minimum
        ''' acceptable distance; the second is the maximum.</returns>
        Public Function GetAllowedDistanceRangeForNode(ByVal NodeIndex As Integer) As Single()
            Dim MinDistance As Single = Me.GetMinimumNodeDistance
            If NodeIndex > 0 Then
                'Width nodes being circular, we can just naively add the display
                'width without worrying about the gradient
                MinDistance = Me.Choices(NodeIndex - 1).Distance + clsChoice.DisplayWidth
            End If

            Dim MaxDistance As Single = Me.GetMaximumNodeDistance
            If NodeIndex < Me.Choices.Count - 1 Then
                'Width nodes being circular, we can just naively add the display
                'width without worrying about the gradient
                MaxDistance = Me.Choices(NodeIndex + 1).Distance - clsChoice.DisplayWidth
            End If

            Return New Single() {MinDistance, MaxDistance}
        End Function


        Public Overrides Sub SetCorner(ByVal dblX As Single, ByVal dblY As Single, ByVal iCorner As Integer)
            'Remember the current size
            Dim OldWidth As Single = Me.GetDisplayWidth
            Dim OldHeight As Single = Me.GetDisplayHeight

            'Apply the size change as requested
            MyBase.SetCorner(dblX, dblY, iCorner)

            'Revert the change if it would mean obscuring any nodes
            If Me.Choices.Count > 0 Then
                If Me.GetMinimumNodeDistance > Me.Choices(0).Distance Then
                    Me.SetDisplayWidth(OldWidth)
                    Me.SetDisplayHeight(OldHeight)
                End If
            End If
        End Sub

    End Class
End Namespace
