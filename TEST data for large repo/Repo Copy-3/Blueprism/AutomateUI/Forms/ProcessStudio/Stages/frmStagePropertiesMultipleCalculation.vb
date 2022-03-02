Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateAppCore.Utility

Friend Class frmStagePropertiesMultipleCalculation
    Implements IDataItemTreeRefresher

    ''' <summary>
    ''' The multiple calculation stage that this dialog is editing
    ''' </summary>
    Protected ReadOnly Property Stage() As clsMultipleCalculationStage
        Get
            Return DirectCast(ProcessStage, Stages.clsMultipleCalculationStage)
        End Get
    End Property

    Private ReadOnly Property IDataItemTreeRefresher_Stage As clsProcessStage Implements IDataItemTreeRefresher.Stage
        Get
            Return Stage()
        End Get
    End Property

    Protected Overrides Sub PopulateStageData()
        MyBase.PopulateStageData()

        PopulateExpressionList()
        mDataItemTreeView.Populate(mProcessStage)
        mDataItemTreeView.ProcessViewer = ProcessViewer
    End Sub

    Private Sub PopulateExpressionList()

        mCalculationListView.Columns.Clear()
        mCalculationListView.Columns.Add(My.Resources.frmStagePropertiesMultipleCalculation_Expression)
        mCalculationListView.Columns.Add(My.Resources.frmStagePropertiesMultipleCalculation_StoreIn)
        mCalculationListView.LastColumnAutoSize = True

        For Each ca As clsCalcStep In Stage.Steps
            AddNewRow(ca)
        Next

        'Add a new row by default if none exist
        If mCalculationListView.Rows.Count = 0 Then
            btnAddCriterion_Click(Nothing, Nothing)
        End If

        mCalculationListView.UpdateView()
        UpdateButtons()
    End Sub

    Private Sub AddNewRow(ByVal calcStep As clsCalcStep)
        Dim row As New clsCalcListRow(mCalculationListView, mProcessStage, ProcessViewer, calcStep)
        row.Treeview = mDataItemTreeView

        Dim Index As Integer = mCalculationListView.Rows.Count
        If mCalculationListView.CurrentEditableRow IsNot Nothing Then
            Index = 1 + mCalculationListView.CurrentEditableRow.Index
        End If
        mCalculationListView.Rows.Insert(Index, row)
        mCalculationListView.UpdateView()
        mCalculationListView.CurrentEditableRow = row

    End Sub

    ''' <summary>
    ''' Writes the data in the user interface to the stage object.
    ''' </summary>
    ''' <returns>Returns true on success.</returns>
    ''' <remarks>Calls base class implementation first.
    ''' Things such as the name of the stage are set there.</remarks>
    Protected Overrides Function ApplyChanges() As Boolean
        If Not MyBase.ApplyChanges() Then Return False

        mCalculationListView.EndEditing()
        With Stage.Steps

            'clear all choices existing in stage
            .Clear()

            'populate stage with all (populated) choices in user interface
            For Each r As clsCalcListRow In mCalculationListView.Rows
                Dim stp As clsCalcStep = r.Calculation
                If Not stp.Expression.IsEmpty OrElse stp.StoreIn <> "" Then
                    .Add(stp)
                End If
            Next

        End With

        Return True

    End Function

    ''' <summary>
    ''' Enables and disables the buttons in the user interface according to whether
    ''' there is an active row selected etc.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub UpdateButtons()
        Dim SelectedRow As clsListRow = mCalculationListView.CurrentEditableRow
        btnAddCriterion.Enabled = True
        btnRemove.Enabled = Not SelectedRow Is Nothing

        Dim SelectedRowIsFirstRow As Boolean = (SelectedRow IsNot Nothing) AndAlso SelectedRow.Index = 0
        Dim SelectedRowIsLastRow As Boolean = (SelectedRow IsNot Nothing) AndAlso SelectedRow.Index = mCalculationListView.Rows.Count - 1

        btnMoveUp.Enabled = btnRemove.Enabled AndAlso (Not SelectedRowIsFirstRow)
        btnMoveDown.Enabled = btnRemove.Enabled AndAlso (Not SelectedRowIsLastRow)
    End Sub

    Private Sub CtlListView1_SelectedRowChanged( _
     ByVal sender As Object, ByVal e As ListRowChangedEventArgs) _
     Handles mCalculationListView.EditableRowChanged
        UpdateButtons()
    End Sub

    Private Sub btnAddCriterion_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddCriterion.Click
        AddNewRow(New clsCalcStep(Stage))
        mCalculationListView.UpdateView()
        UpdateButtons()
    End Sub

    Private Sub btnRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemove.Click
        If Not mCalculationListView.CurrentEditableRow Is Nothing Then
            mCalculationListView.CurrentEditableRow.Remove()
        End If

        mCalculationListView.UpdateView()
        UpdateButtons()
    End Sub

    Private Sub btnMoveDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMoveDown.Click
        mCalculationListView.MoveEditableRowdown()
        mCalculationListView.UpdateView()
        UpdateButtons()
    End Sub

    Private Sub btnMoveUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMoveUp.Click
        mCalculationListView.MoveEditableRowUp()
        mCalculationListView.UpdateView()
        UpdateButtons()
    End Sub

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesMultipleCalculation.htm"
    End Function

    ''' <summary>
    ''' Opens the help file whether online or offline.
    ''' </summary>
    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    Public Sub Repopulate(displayStage As clsDataStage) Implements IDataItemTreeRefresher.Repopulate
        mDataItemTreeView.Repopulate(displayStage)
    End Sub

    Public Sub Remove(stage As clsDataStage) Implements IDataItemTreeRefresher.Remove
        mDataItemTreeView.RemoveDataItemTreeNode(stage)
    End Sub
End Class
