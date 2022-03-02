Imports System.Drawing

Public Class clsProcessSelectionContainer
    Implements IEnumerable

    ''' <summary>
    ''' The collection of clsProcessSelection objects.
    ''' </summary>
    Private mcolSelections As New List(Of clsProcessSelection)

    ''' <summary>
    ''' Notifies clients of all changes including deletions, additions etc.
    ''' </summary>
    Public Event SelectionChanged()

    ''' <summary>
    ''' Counts the number of clsProcessSelection objects contained.
    ''' </summary>
    ''' <value></value>
    Public ReadOnly Property Count() As Integer
        Get
            Return Me.mcolSelections.Count
        End Get
    End Property

    ''' <summary>
    ''' Adds the supplied selection object to the end of the selection.
    ''' </summary>
    ''' <param name="value">The selection to add.</param>
    Public Sub Add(ByVal value As clsProcessSelection)
        Me.mcolSelections.Add(value)
        RaiseEvent SelectionChanged()
    End Sub

    ''' <summary>
    ''' Adds a selection object to the front of the selection collection. By
    ''' convention, the first item is the primary selection.
    ''' </summary>
    ''' <param name="S">The selection to add.</param>
    Public Sub AddPrimarySelection(ByVal S As clsProcessSelection)
        Me.mcolSelections.Insert(0, S)
        RaiseEvent SelectionChanged()
    End Sub

    ''' <summary>
    ''' Clears all selections, leaving the selection entirely empty.
    ''' </summary>
    Public Sub ClearAllSelections()
        Me.mcolSelections.Clear()
        RaiseEvent SelectionChanged()
    End Sub

    ''' <summary>
    ''' The clsProcessSelection items in this selection container
    ''' </summary>
    ''' <param name="index">The zero-based index of the item to retrieve.</param>
    Default Public ReadOnly Property Item(ByVal index As Integer) As clsProcessSelection
        Get
            Debug.Assert(index >= 0)
            Debug.Assert(index <= Me.Count - 1)
            Return CType(Me.mcolSelections(index), clsProcessSelection)
        End Get
    End Property

    ''' <summary>
    ''' Removes the item at the specified index.
    ''' </summary>
    ''' <param name="index">The zero-based index of the item to remove.</param>
    Public Sub Remove(ByVal index As Integer)
        Me.mcolSelections.RemoveAt(index)
        RaiseEvent SelectionChanged()
    End Sub

    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return mcolSelections.GetEnumerator
    End Function

    ''' <summary>
    ''' Gets the primary selection, if it exists.
    ''' </summary>
    ''' <value>The primary selection, or a null reference if no selection exists.</value>
    Public ReadOnly Property PrimarySelection() As clsProcessSelection
        Get
            If Me.mcolSelections.Count > 0 Then
                'by convention, the first item is the primary selection.
                Return CType(Me.mcolSelections(0), clsProcessSelection)
            Else
                Return Nothing
            End If
        End Get
    End Property

    ''' <summary>
    ''' Gets the smallest rectangle containing every item
    ''' in the selection.
    ''' </summary>
    ''' <param name="ParentProcess">The process from which the
    ''' selection originates. Used for resolving stage IDs into
    ''' stage object references.</param>
    ''' <value>The bounding rectangle of the entire selection,
    ''' in world coordinates.</value>
    Public ReadOnly Property SelectionBounds(ByVal ParentProcess As clsProcess) As RectangleF
        Get
            Dim Retval As RectangleF = Rectangle.Empty
            For Each selection As clsProcessSelection In Me.mcolSelections
                Dim ItemBounds As RectangleF = RectangleF.Empty
                Select Case selection.mtType
                    Case clsProcessSelection.SelectionType.Stage
                        Dim Stage As clsProcessStage = ParentProcess.GetStage(selection.mgStageID)
                        ItemBounds = Stage.GetDisplayBounds
                    Case clsProcessSelection.SelectionType.ChoiceNode
                        Dim Node As clsChoice = CType(ParentProcess.GetStage(selection.mgStageID), Stages.clsChoiceStartStage).Choices(selection.miChoiceIndex)
                        ItemBounds = Node.DisplayBounds
                End Select

                If Retval.IsEmpty Then
                    Retval = ItemBounds
                Else
                    Retval = RectangleF.Union(Retval, ItemBounds)
                End If
            Next
            Return Retval
        End Get
    End Property
End Class