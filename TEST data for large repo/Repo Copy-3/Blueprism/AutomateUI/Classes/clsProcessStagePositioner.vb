Imports BluePrism.Core.Expressions
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages

''' <summary>
''' Class designed for choosing the new location
''' of a process stage, in an existing process
''' diagram containing other stages.
''' </summary>
''' <remarks>Typically used for positioning
''' auto-created data items, associated with an
''' existing stage, or for offsetting the position of
''' a stage which is about to be pasted on top of another.</remarks>
Public Class clsProcessStagePositioner

    ''' <summary>
    ''' The process whose stages are being manipulated.
    ''' </summary>
    Private mProcess As clsProcess

    ''' <summary>
    ''' Creates a new ProcessStagePositioner instance.
    ''' </summary>
    ''' <param name="Process">The process whose stages are
    ''' being manipulated.</param>
    Public Sub New(ByVal Process As clsProcess)
        mProcess = Process
    End Sub

    ''' <summary>
    ''' The relative positions available for
    ''' the placement of stages.
    ''' </summary>
    Public Enum RelativePositions
        East
        South
        West
        North
    End Enum

    ''' <summary>
    ''' Gets the projected datatype from an expression
    ''' </summary>
    ''' <param name="stg">The stage to use as the scope stage for the expression.
    ''' </param>
    ''' <param name="localExpr">The local form of an expression from which to draw
    ''' the resultant data type.</param>
    ''' <returns>The projected datatype from the given expression.</returns>
    Public Shared Function DataTypeFromExpression( _
     ByVal stg As clsProcessStage, ByVal localExpr As String) As DataType
        Return DataTypeFromExpression(stg, BPExpression.FromLocalised(localExpr))
    End Function

    ''' <summary>
    ''' Gets the projected datatype from an expression
    ''' </summary>
    ''' <param name="stg">The stage to use as the scope stage for the expression.
    ''' </param>
    ''' <param name="expr">The expression from which to draw the resultant data
    ''' type.</param>
    ''' <returns>The projected datatype from the given expression.</returns>
    Public Shared Function DataTypeFromExpression( _
     ByVal stg As clsProcessStage, ByVal expr As BPExpression) As DataType
        'Decide the data type, defaulting to Text if there is no valid expression
        Dim dtype As DataType = DataType.text

        If stg IsNot Nothing Then
            Dim objResult As clsProcessValue = Nothing
            Dim ExpressionInfo As New clsExpressionInfo
            Dim sErr As String = Nothing
            If clsExpression.EvaluateExpression( _
             expr, objResult, stg, True, ExpressionInfo, sErr) Then
                dtype = objResult.DataType
            End If
        End If

        Return dtype
    End Function

    ''' <summary>
    ''' Creates a data item with the given name, from the given stage.
    ''' </summary>
    ''' <param name="name">The name of the data item to create</param>
    ''' <param name="stg">The stage creating the data item</param>
    ''' <param name="dt">The data type of the required data item.</param>
    ''' <param name="lastStageAdded">The last stage added - this will be updated
    ''' if the data item creation was successful with the new data stage.</param>
    ''' <param name="lastRelativePosition">The relative position of the last
    ''' stage created - this will be updated if the data item creation was
    ''' successful with the new data stage's position.</param>
    ''' <returns>The data stage created by this method, or null if no data
    ''' stage was created. Note that this will handle the error message itself,
    ''' rather than passing an error message back out.</returns>
    Public Shared Function CreateDataItem( _
     ByVal name As String, ByVal stg As clsProcessStage, ByVal dt As DataType, _
     ByRef lastStageAdded As clsProcessStage, ByRef lastRelativePosition As RelativePositions) As clsDataStage

        If stg Is Nothing Then Return Nothing

        Dim proc As clsProcess = stg.Process

        'Check uniqueness of data item name, and show error if needs be
        For Each match As clsDataStage In proc.GetStages(name, StageTypes.Data, StageTypes.Collection)
            If match.IsInScope(stg) Then
                UserMessage.Show(String.Format(
                 My.Resources.TheChosenNameForThisStageConflictsWithTheStage0OnPage1PleaseChooseAnother, match.GetName(), match.GetSubSheetName()))
                Return Nothing
            End If
        Next

        'Create the data item
        Dim dataStg As clsDataStage
        Select Case dt
            Case DataType.collection
                dataStg = CType(proc.AddStage(StageTypes.Collection, name), clsDataStage)
            Case Else
                dataStg = CType(proc.AddStage(StageTypes.Data, name), clsDataStage)
        End Select
        dataStg.SetDataType(dt)
        dataStg.SetSubSheetID(stg.GetSubSheetID())

        'Find the stage next to which we shall place the new stage
        Dim existingStage As clsProcessStage = Nothing
        If lastStageAdded IsNot Nothing Then
            existingStage = proc.GetStage(lastStageAdded.GetStageID())
        End If
        If existingStage Is Nothing Then
            existingStage = stg
        End If

        'Do the placement
        lastRelativePosition = New clsProcessStagePositioner(proc).PositionStageAsBuddyOf( _
         dataStg, existingStage, lastRelativePosition)

        lastStageAdded = dataStg

        Return dataStg

    End Function



    ''' <summary>
    ''' Gets the next position in sequence.
    ''' </summary>
    ''' <param name="CurrentPosition">The position
    ''' whose successor is desired.</param>
    ''' <returns>Returns the position which succeeds
    ''' the supplied position, in sequence.</returns>
    Private Function GetNextPosition(ByVal CurrentPosition As RelativePositions) As RelativePositions
        Select Case CurrentPosition
            Case RelativePositions.North
                Return RelativePositions.East
            Case RelativePositions.East
                Return RelativePositions.South
            Case RelativePositions.South
                Return RelativePositions.West
            Case RelativePositions.West
                Return RelativePositions.North
        End Select
    End Function

    ''' <summary>
    ''' Positions a stage nearby an existing stage.
    ''' </summary>
    ''' <param name="NewStage">The stage whose position is to
    ''' be determined.</param>
    ''' <param name="BaseStage">The existing stage, near to which
    ''' the new stage should be positioned.</param>
    ''' <returns>Returns the relative position used to place the
    ''' stage. Note that this may not necessarily be relative to the requested
    ''' stage, since there may not be room near the requested stage;
    ''' instead it may be relative to another, nearby stage.</returns>
    Public Function PositionStageAsBuddyOf(ByVal NewStage As clsProcessStage, ByVal BaseStage As clsProcessStage, Optional ByVal PreferredPosition As RelativePositions = RelativePositions.East) As RelativePositions
        Me.PositionStageAsBuddyOf(NewStage, BaseStage, New Generic.List(Of clsProcessStage), PreferredPosition)
    End Function

    ''' <summary>
    ''' Positions a stage nearby an existing stage.
    ''' </summary>
    ''' <param name="NewStage">The stage whose position is to
    ''' be determined.</param>
    ''' <param name="BaseStage">The existing stage, near to which
    ''' the new stage should be positioned.</param>
    ''' <returns>Returns the relative position used to place the
    ''' stage. Note that this may not necessarily be relative to the requested
    ''' stage, since there may not be room near the requested stage;
    ''' instead it may be relative to another, nearby stage.</returns>
    Private Function PositionStageAsBuddyOf(ByVal NewStage As clsProcessStage, ByVal BaseStage As clsProcessStage, ByVal TriedStages As Generic.List(Of clsProcessStage), Optional ByVal PreferredPosition As RelativePositions = RelativePositions.East) As RelativePositions

        '*********************************************
        '                     PLAN
        ' 1. Rotate through the 'north', 'south', 'east',
        '    'west' positions (NESW) to see if we can plonk
        '    the new stage next to the existing stage.
        '
        ' 2. If the above is not possible then apply the
        '    same process recursively, using the stages
        '    already at the NESW positions as the base stage,
        '    BUT with the exception that the first attempt to
        '    place the stage should correspond to the direction
        '    already travelled in. Eg if after step 1 fails, we travel
        '    east to the adjacent stage, then the first attempt
        '    to place the stage should be at the east position
        '    (rather than the north position).
        '
        ' 3. Note that this algorithm must terminate, since
        '    there are only a finite number of stages - at
        '    some point we must hit the edge of the process
        '    map, since we persist in the same direction with
        '    each recursive step.
        '*********************************************

        TriedStages.Add(BaseStage)

        'Step 1 as described above
        Dim OriginalRelativePos As RelativePositions = PreferredPosition
        Dim CurrentRelativePos As RelativePositions = PreferredPosition
        Do
            Dim NewBounds As RectangleF = Me.GetProposedStageBounds(NewStage, BaseStage, CurrentRelativePos, 1)
            If Me.CanPlaceStageAt(NewBounds, BaseStage.GetSubSheetID, NewStage.GetStageID) Then
                Me.SetStageBounds(NewStage, NewBounds)
                Return CurrentRelativePos
            Else
                CurrentRelativePos = Me.GetNextPosition(CurrentRelativePos)
            End If
        Loop While CurrentRelativePos <> OriginalRelativePos

        'Step 2 as described above
        Dim DistanceMultiplier As Integer = 1
Start:
        OriginalRelativePos = PreferredPosition
        CurrentRelativePos = OriginalRelativePos
        Do
            Dim ProposedBounds As RectangleF = Me.GetProposedStageBounds(NewStage, BaseStage, CurrentRelativePos, DistanceMultiplier)
            If Me.CanPlaceStageAt(ProposedBounds, BaseStage.GetSubSheetID, NewStage.GetStageID) Then
                Me.SetStageBounds(NewStage, ProposedBounds)
                Return CurrentRelativePos
            Else
                Dim BlockingStage As clsProcessStage = Me.mProcess.GetStageAtPosition(Me.GetRectCentre(ProposedBounds))
                If BlockingStage IsNot Nothing AndAlso (Not TriedStages.Contains(BlockingStage)) Then
                    Return Me.PositionStageAsBuddyOf(NewStage, BlockingStage, TriedStages, CurrentRelativePos)
                Else
                    'We might reach here if the blocking thing is a link
                    CurrentRelativePos = Me.GetNextPosition(CurrentRelativePos)
                End If
            End If
        Loop While CurrentRelativePos <> OriginalRelativePos

        'We might drop out to here if a stage is surrounded by blocking links, for example
        'In this case we need to place the stage farther away from the base stage
        DistanceMultiplier += 1
        GoTo Start
    End Function

    ''' <summary>
    ''' Gets the point at the centre of the supplied rectangle.
    ''' </summary>
    ''' <param name="Rect">The rectangle of interest.</param>
    Private Function GetRectCentre(ByVal Rect As RectangleF) As PointF
        Return New PointF(Rect.X + Rect.Width / 2, Rect.Y + Rect.Height / 2)
    End Function

    ''' <summary>
    ''' Sets the bounds of the specified stage.
    ''' </summary>
    ''' <param name="Stage">The stage whose bounds are to be set.</param>
    ''' <param name="NewBounds">The new bounds of the stage</param>
    Private Sub SetStageBounds(ByVal Stage As clsProcessStage, ByVal NewBounds As RectangleF)
        'These two values differ because the stage's location is its centre,
        'whereas we use the term 'bounds' in the absolute sense
        Stage.SetDisplayX(NewBounds.X + NewBounds.Width / 2)
        Stage.SetDisplayY(NewBounds.Y + NewBounds.Height / 2)

        Stage.SetDisplayWidth(NewBounds.Width)
        Stage.SetDisplayHeight(NewBounds.Height)
    End Sub

    ''' <summary>
    ''' Gets the bounds of the supplied stage in its proposed
    ''' new location relative to the existing stage.
    ''' </summary>
    ''' <param name="StageToPlace">The new stage whose bounds
    ''' are to be calculated.</param>
    ''' <param name="ExistingStage">The existing stage, next
    ''' to which the new stage is to be placed.</param>
    ''' <param name="Position">The relative position of the new
    ''' stage.</param>
    ''' <param name="DistanceMultiplier">Determines how many multiples
    ''' of the width of the StageToPlace will exist between the
    ''' existing stage and the StageToPlace</param>
    ''' <returns>Returns proposed bounds of the new stage.</returns>
    ''' <remarks>This function simply naively places the stage
    ''' using a fixed margin, whilst accounting for the size
    ''' of the new stage. It is up to the callee to check
    ''' whether it intersects with any existing stages/links.</remarks>
    Private Function GetProposedStageBounds(ByVal StageToPlace As clsProcessStage, ByVal ExistingStage As clsProcessStage, ByVal Position As RelativePositions, ByVal DistanceMultiplier As Integer) As RectangleF
        Dim ProposedBounds As RectangleF
        ProposedBounds.Size = New SizeF(StageToPlace.GetDisplayWidth, StageToPlace.GetDisplayHeight)

        Select Case Position
            Case RelativePositions.North
                ProposedBounds.X = ExistingStage.GetDisplayBounds.Left
                ProposedBounds.Y = ExistingStage.GetDisplayBounds.Top - clsProcess.GridUnitSize - (DistanceMultiplier * StageToPlace.GetDisplayHeight)
            Case RelativePositions.South
                ProposedBounds.X = ExistingStage.GetDisplayBounds.Left
                ProposedBounds.Y = ExistingStage.GetDisplayBounds.Bottom + ((DistanceMultiplier - 1) * StageToPlace.GetDisplayBounds.Height) + clsProcess.GridUnitSize
            Case RelativePositions.West
                ProposedBounds.Y = ExistingStage.GetDisplayBounds.Top
                ProposedBounds.X = ExistingStage.GetDisplayBounds.Left - clsProcess.GridUnitSize - (DistanceMultiplier * StageToPlace.GetDisplayWidth)
            Case RelativePositions.East
                ProposedBounds.Y = ExistingStage.GetDisplayBounds.Top
                ProposedBounds.X = ExistingStage.GetDisplayBounds.Right + ((DistanceMultiplier - 1) * StageToPlace.GetDisplayBounds.Width) + clsProcess.GridUnitSize
        End Select

        Return ProposedBounds
    End Function


    ''' <summary>
    ''' Determines whether a stage can be placed
    ''' at the specified location.
    ''' </summary>
    ''' <param name="ProposedBounds">The proposed bounds of the new stage.</param>
    ''' <returns>Returns true if the stage does not intersect
    ''' any existing stages or links, false otherwise.</returns>
    Private Function CanPlaceStageAt(ByVal ProposedBounds As RectangleF, ByVal PageID As Guid, ByVal StageToExclude As Guid) As Boolean
        For Each ExistingStage As clsProcessStage In mProcess.GetStages(PageID)
            If ExistingStage.GetStageID <> StageToExclude AndAlso ExistingStage.StageType <> StageTypes.Block Then
                If ExistingStage.GetDisplayBounds.IntersectsWith(ProposedBounds) Then Return False
                If StageLinksIntersectsRectangle(ExistingStage, ProposedBounds) Then Return False
            End If
        Next

        Return True
    End Function

    ''' <summary>
    ''' Determines wether a stage's link(s) intersect with the
    ''' supplied rectangle.
    ''' </summary>
    ''' <param name="SourceStage">The stage whose links are of
    ''' interest.</param>
    ''' <param name="Rectangle">The rectangle to be tested.</param>
    ''' <returns>Returns true if any of the stage's links
    ''' intersect the supplied rectangle.</returns>
    Private Function StageLinksIntersectsRectangle(ByVal SourceStage As clsProcessStage, ByVal Rectangle As RectangleF) As Boolean
        Select Case SourceStage.StageType
            Case StageTypes.ChoiceStart, StageTypes.WaitStart
                'Check each choice/wait node's link in turn
                Dim WaitStart As Stages.clsChoiceStartStage = CType(SourceStage, Stages.clsChoiceStartStage)
                For Each node As clsChoice In CType(SourceStage, Stages.clsChoiceStartStage).Choices
                    If node.LinkTo <> Guid.Empty Then
                        Dim Target As clsProcessStage = mProcess.GetStage(node.LinkTo)
                        If Target IsNot Nothing Then
                            If Me.LineIntersectsRectangle(node.Location, Target.Location, Rectangle) Then
                                Return True
                            End If
                        End If
                    End If
                Next

                'Check the link from choice start to choice end
                Dim WaitEnd As Stages.clsChoiceEndStage = WaitStart.Process.GetChoiceEnd(WaitStart)
                If Me.LineIntersectsRectangle(WaitStart.Location, WaitEnd.Location, Rectangle) Then
                    Return True
                End If

            Case StageTypes.Decision
                'Check true link
                Dim Target As clsProcessStage = mProcess.GetStage(CType(SourceStage, Stages.clsDecisionStage).OnTrue)
                If Target IsNot Nothing Then
                    If Me.LineIntersectsRectangle(SourceStage.Location, Target.Location, Rectangle) Then
                        Return True
                    End If
                End If
                'Create false link
                Target = mProcess.GetStage(CType(SourceStage, Stages.clsDecisionStage).OnFalse)
                If Target IsNot Nothing Then
                    If Me.LineIntersectsRectangle(SourceStage.Location, Target.Location, Rectangle) Then
                        Return True
                    End If
                End If

            Case Else
                'Simply need to check the one 'on success' link
                If TypeOf SourceStage Is clsLinkableStage Then
                    Dim Target As clsProcessStage = mProcess.GetStage(CType(SourceStage, clsLinkableStage).OnSuccess)
                    If Target IsNot Nothing Then
                        If Me.LineIntersectsRectangle(SourceStage.Location, Target.Location, Rectangle) Then
                            Return True
                        End If
                    End If
                End If
        End Select

        Return False
    End Function

    ''' <summary>
    ''' Determines whether a line (with finite length)
    ''' intersects a rectangle.
    ''' </summary>
    ''' <param name="LineStart">The starting point of the line.</param>
    ''' <param name="LineEnd">The end point of the line.</param>
    ''' <param name="Rectangle">The rectangle to be tested.</param>
    ''' <returns>Returns true if the entities intersect.</returns>
    Private Function LineIntersectsRectangle(ByVal LineStart As PointF, ByVal LineEnd As PointF, ByVal Rectangle As RectangleF) As Boolean
        'Calculate the smallest rectangle enclosing the line
        Dim MinX As Single = Math.Min(LineStart.X, LineEnd.X)
        Dim MaxX As Single = Math.Max(LineStart.X, LineEnd.X)
        Dim Miny As Single = Math.Min(LineStart.Y, LineEnd.Y)
        Dim Maxy As Single = Math.Max(LineStart.Y, LineEnd.Y)
        Dim LineBounds As RectangleF = New RectangleF(MinX, Miny, MaxX - MinX, Maxy - Miny)

        'Vertical or horizontal lines are not considered to intersect
        'by the framework, so just give them a width/height of 1
        LineBounds.Width = Math.Max(LineBounds.Width, 1)
        LineBounds.Height = Math.Max(LineBounds.Height, 1)

        'Trivial check to quit early
        If Not LineBounds.IntersectsWith(Rectangle) Then Return False

        'Sample points on the line using increments of 
        'one half of the rectangle's smallest dimension
        Dim VecX As Single = LineEnd.X - LineStart.X
        Dim Vecy As Single = LineEnd.Y - LineStart.Y
        Dim LineLength As Double = Math.Sqrt(Math.Pow(VecX, 2) + Math.Pow(Vecy, 2))
        Dim Increment As Single = Math.Min(Rectangle.Width, Rectangle.Height) / 2
        Dim SamplePoint As PointF
        Dim Sine As Double = Vecy / LineLength
        Dim Cosine As Double = VecX / LineLength
        Dim LinePosition As Double = 0
        While LinePosition <= LineLength
            SamplePoint.X = LineStart.X + CSng(Cosine * LinePosition)
            SamplePoint.Y = LineStart.Y + CSng(Sine * LinePosition)
            If Rectangle.Contains(SamplePoint) Then Return True
            LinePosition += Increment
        End While

        Return False
    End Function


End Class
