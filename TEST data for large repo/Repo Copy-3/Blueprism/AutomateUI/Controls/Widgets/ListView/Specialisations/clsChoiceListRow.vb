Imports BluePrism.Core.Expressions
Imports BluePrism.AutomateProcessCore

Friend Class clsChoiceListRow
    Inherits clsListRow


    ''' <summary>
    ''' The choice represented by this row.
    ''' </summary>
    Public Property Choice() As clsChoice
        Get
            Return mChoice
        End Get
        Set(ByVal value As clsChoice)
            mChoice = value

            If mChoice IsNot Nothing Then
                With Items
                    .Clear()
                    .Add(New clsListItem(Me, Choice.Name))
                    .Add(New clsListItem(Me, Choice.Expression.LocalForm))
                End With
            Else
                Me.SetNullValues()
            End If
        End Set
    End Property
    ''' <summary>
    ''' Private member to store public property Choice
    ''' </summary>
    ''' <remarks></remarks>
    Private mChoice As clsChoice

    Private Sub SetNullValues()
        With Items
            .Clear()
            .Add(New clsListItem(Me, "")) 'name
            .Add(New clsListItem(Me, "")) 'expression
        End With
    End Sub

    ''' <summary>
    ''' The stage used in scope resolution, during editing.
    ''' </summary>
    Private mobjProcessStage As clsProcessStage

    ''' <summary>
    ''' Reference to the process viewer needed for the Expression edit control
    ''' </summary>
    Private mProcessViewer As ctlProcessViewer

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="Stage">The stage used in scope resolution, during editing.</param>
    ''' <param name="ProcessViewer">The process viewer needed for the Expression edit control</param>
    ''' <param name="Choice">The choice represented by this row.</param>
    Public Sub New(ByVal lv As ctlListView, ByVal Stage As clsProcessStage, _
     ByVal ProcessViewer As ctlProcessViewer, ByVal Choice As clsChoice)
        MyBase.New(lv)
        Me.Choice = Choice
        Me.mobjProcessStage = Stage
        Me.mProcessViewer = ProcessViewer
    End Sub

    Public Overrides Function CreateEditRow() As ctlEditableListRow
        Dim NewRow As New ctlEditableListRow

        Dim NameEditArea As New AutomateControls.Textboxes.StyledTextBox
        If Not Me.Choice Is Nothing Then NameEditArea.Text = Me.Choice.Name
        Dim NameItem As New ctlEditableListItem(NameEditArea)

        Dim ExpressionEditArea As New ctlExpressionEdit
        ExpressionEditArea.ProcessViewer = mProcessViewer
        ExpressionEditArea.Stage = mobjProcessStage
        ExpressionEditArea.IsDecision = True
        If Choice IsNot Nothing Then _
         ExpressionEditArea.Text = Choice.Expression.LocalForm
        ExpressionEditArea.ColourText()
        Dim ExpressionItem As New ctlEditableListItem(ExpressionEditArea)

        NewRow.Items.Add(NameItem)
        NewRow.Items.Add(ExpressionItem)
        If Choice IsNot Nothing AndAlso Choice.LinkTo <> Guid.Empty Then _
         NewRow.Tag = Choice.LinkTo

        Return NewRow
    End Function

    Public Overrides Sub EndEdit(ByVal EditRow As ctlEditableListRow)
        Me.Choice.Name = EditRow.Items(0).NestedControl.Text
        Me.Choice.Expression = _
         BPExpression.FromLocalised(EditRow.Items(1).NestedControl.Text)
        MyBase.EndEdit(EditRow)
    End Sub

    Public Overrides Sub BeginEdit(ByVal EditRow As ctlEditableListRow)
        If Choice IsNot Nothing Then
            EditRow.Items(0).NestedControl.Text = Choice.Name
            EditRow.Items(1).NestedControl.Text = Choice.Expression.LocalForm
        Else
            EditRow.Items(0).NestedControl.Text = ""
            EditRow.Items(1).NestedControl.Text = ""
        End If
    End Sub

    ''' <summary>
    ''' Handles an item being dragged over this row.
    ''' </summary>
    ''' <param name="e">The args detailing the event.</param>
    ''' <param name="locn">The location, relative to this row, of the cursor.</param>
    Public Overrides Sub OnDragOver( _
     ByVal e As DragEventArgs, ByVal locn As Point, ByVal colIndex As Integer)
        If colIndex = 1 Then
            Dim n As TreeNode = TryCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
            If n IsNot Nothing AndAlso TypeOf n.Tag Is IDataField Then
                e.Effect = DragDropEffects.Move
                Return
            End If
        End If

        ' Else ie. colIndex = 0 or n is null, not a treenode, or not a datafield node
        MyBase.OnDragOver(e, locn, colIndex)

    End Sub

    ''' <summary>
    ''' Handles an item being dropped on this row.
    ''' </summary>
    ''' <param name="e">The args detailing the dragdrop event.</param>
    ''' <param name="locn">The location, relative to this row, in which the
    ''' item was dropped.</param>
    ''' <param name="colIndex">The column index, as calculated by the owning
    ''' control, at which the item was dropped.</param>
    Public Overrides Sub OnDragDrop( _
     ByVal e As DragEventArgs, ByVal locn As Point, ByVal colIndex As Integer)
        ' We're only interested in column 1
        If colIndex <> 1 Then Return

        ' And the dragged data must be a tree node
        Dim n As TreeNode = TryCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
        If n Is Nothing Then Return

        ' And its tag must be a datafield
        Dim df As IDataField = TryCast(n.Tag, IDataField)
        If df Is Nothing Then Return

        ' Append the expression
        Choice.Expression = BPExpression.FromNormalised( _
         Choice.Expression.NormalForm & "[" & df.FullyQualifiedName & "]")

        ' Pass control to the row that was dragged onto.
        Owner.CurrentEditableRow = Me
        Owner.EditRowControl.Items(colIndex).Focus()

    End Sub

End Class
