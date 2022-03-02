Imports BluePrism.Core.Expressions
Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Class    : clsCalcListRow
''' 
''' <summary>
''' Class that represents the list row item for a multiple calculation stage
''' </summary>
Friend Class clsCalcListRow
    Inherits clsListRow

    ''' <summary>
    ''' The stage used in scope resolution, during editing.
    ''' </summary>
    Private mProcessStage As clsProcessStage

    ''' <summary>
    ''' Reference to the process viewer needed for the Expression edit control
    ''' </summary>
    Private mProcessViewer As ctlProcessViewer

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="Stage">The stage used in scope resolution, during editing.</param>
    ''' <param name="ProcessViewer">The process viewer needed for the Expression edit control</param>
    ''' <param name="Calc">The calculation represented by this row.</param>
    Public Sub New(ByVal lv As ctlListView, ByVal Stage As clsProcessStage, _
     ByVal ProcessViewer As ctlProcessViewer, ByVal Calc As clsCalcStep)
        MyBase.New(lv)
        Calculation = Calc
        mProcessStage = Stage
        mProcessViewer = ProcessViewer
    End Sub

    ''' <summary>
    ''' The treeview associated with this row, for editing purposes
    ''' </summary>
    Public WriteOnly Property Treeview() As ctlDataItemTreeView
        Set(ByVal value As ctlDataItemTreeView)
            mTreeview = value
        End Set
    End Property
    Private mTreeview As ctlDataItemTreeView

    ''' <summary>
    ''' The calculation represented by this row.
    ''' </summary>
    Public Property Calculation() As clsCalcStep
        Get
            Return mCalculation
        End Get
        Set(ByVal value As clsCalcStep)
            mCalculation = value
            If value Is Nothing Then SetNullValues() : Return
            With Items
                .Clear()
                .Add(New clsListItem(Me, value.Expression.LocalForm))
                .Add(New clsListItem(Me, value.StoreIn))
            End With
        End Set
    End Property
    Private mCalculation As clsCalcStep

    ''' <summary>
    ''' Sets the listrows items to empty values
    ''' </summary>
    Private Sub SetNullValues()
        Items.Clear()
        Items.Add(New clsListItem(Me, "")) 'expression
        Items.Add(New clsListItem(Me, "")) 'storein
    End Sub

    ''' <summary>
    ''' Creates an editable list row
    ''' </summary>
    Public Overrides Function CreateEditRow() As ctlEditableListRow
        Return New ctlCalcListRow(mProcessViewer, mProcessStage)
    End Function

    ''' <summary>
    ''' Ends editing of the list row
    ''' </summary>
    ''' <param name="EditRow"></param>
    Public Overrides Sub EndEdit(ByVal EditRow As ctlEditableListRow)
        Dim calcListRow As ctlCalcListRow = CType(EditRow, ctlCalcListRow)
        Calculation.Expression = _
         BPExpression.FromLocalised(calcListRow.ExpressionEdit.Text)
        Calculation.StoreIn = calcListRow.StoreIn.Text
        MyBase.EndEdit(EditRow)
    End Sub

    ''' <summary>
    ''' Begins editing of the list row
    ''' </summary>
    ''' <param name="EditRow"></param>
    Public Overrides Sub BeginEdit(ByVal EditRow As ctlEditableListRow)
        Dim calcListRow As ctlCalcListRow = CType(EditRow, ctlCalcListRow)
        calcListRow.Treeview = Me.mTreeview
        calcListRow.Calculation = Me.Calculation

        If Calculation IsNot Nothing Then
            calcListRow.ExpressionEdit.Text = Calculation.Expression.LocalForm
            calcListRow.StoreIn.Text = Calculation.StoreIn
        Else
            calcListRow.ExpressionEdit.Text = ""
            calcListRow.StoreIn.Text = ""
        End If
    End Sub

    ''' <summary>
    ''' Handles an item being dropped on this row.
    ''' </summary>
    ''' <param name="e">The args detailing the dragdrop event.</param>
    ''' <param name="locn">The location, relative to this row, in which the
    ''' item was dropped.</param>
    ''' <param name="colIndex">The column index, as calculated by the owning
    ''' control, at which the item was dropped.</param>
    Public Overrides Sub OnDragDrop(ByVal e As DragEventArgs, ByVal locn As Point, ByVal colIndex As Integer)

        Dim n As TreeNode = TryCast(e.Data.GetData(GetType(TreeNode)), TreeNode)

        If n IsNot Nothing AndAlso TypeOf n.Tag Is IDataField Then
            Dim name As String = DirectCast(n.Tag, IDataField).FullyQualifiedName
            ' Cols are : "Expression | Store In", ergo 0 == Expression
            If colIndex = 0 Then
                ' We leave this in normal form at this point just for speed - it's
                ' held in a normalised form within the calc step, so this skips the
                ' unnecessary conversion to local and then back again
                Calculation.Expression = BPExpression.FromNormalised( _
                 Calculation.Expression.NormalForm & "[" & name & "]")

            Else ' ie. Store In
                Me.Calculation.StoreIn = name

            End If

            ' Pass control to the row that was dragged onto.
            Owner.CurrentEditableRow = Me
            Owner.EditRowControl.Items(colIndex).Focus()
        End If
    End Sub

    ''' <summary>
    ''' Handles an item being dragged over this row.
    ''' </summary>
    ''' <param name="e">The args detailing the event.</param>
    ''' <param name="locn">The location, relative to this row, of the cursor.</param>
    Public Overrides Sub OnDragOver( _
     ByVal e As DragEventArgs, ByVal locn As Point, ByVal colIndex As Integer)
        Dim n As TreeNode = TryCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
        If n IsNot Nothing AndAlso TypeOf n.Tag Is IDataField Then
            e.Effect = DragDropEffects.Move
        Else
            MyBase.OnDragOver(e, locn, colIndex)
        End If
    End Sub

End Class
