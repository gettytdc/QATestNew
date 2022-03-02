Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.AutomateProcessCore.Stages

Friend Class frmStagePropertiesChoice
    Inherits frmProperties
    Implements IDataItemTreeRefresher


    ''' <summary>
    ''' Strongly typed choice start reference. Points to 
    ''' mobjProcessStage, but saves casting all the time.
    ''' </summary>
    ''' <remarks></remarks>
    Private mChoiceStage As Stages.clsChoiceStartStage

    ''' <summary>
    ''' We need to hold a reference to the choice end stage 
    ''' To calculate the distance when we call ResetChoiceNodePositions
    ''' </summary>
    ''' <remarks></remarks>
    Private mChoiceEnd As Stages.clsChoiceEndStage

    ''' <summary>
    ''' Override of base class implementation so that we can collect
    '''  a strongly typed copy of stage reference.
    ''' </summary>
    ''' <value>.</value>
    ''' <returns>.</returns>
    ''' <remarks></remarks>
    Public Overrides Property ProcessStage() As BluePrism.AutomateProcessCore.clsProcessStage
        Get
            Return MyBase.ProcessStage
        End Get
        Set(ByVal value As BluePrism.AutomateProcessCore.clsProcessStage)
            Me.mChoiceStage = CType(value, Stages.clsChoiceStartStage)
            MyBase.ProcessStage = value
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the choice end stage
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ChoiceEnd() As Stages.clsChoiceEndStage
        Get
            Return mChoiceEnd
        End Get
        Set(ByVal value As Stages.clsChoiceEndStage)
            mChoiceEnd = value
        End Set
    End Property

    Public ReadOnly Property Stage As clsProcessStage Implements IDataItemTreeRefresher.Stage
        Get
            Return ProcessStage
        End Get
    End Property

    ''' <summary>
    ''' Populates user interface with stage data.
    ''' </summary>
    ''' <remarks>Calls base class implementation first.</remarks>
    Protected Overrides Sub PopulateStageData()
        MyBase.PopulateStageData()

        Me.PopulateCriteraList()
        Me.objDataItemTreeView.Populate(Me.mProcessStage)
        Me.objDataItemTreeView.ProcessViewer = Me.ProcessViewer
    End Sub

    ''' <summary>
    ''' Populates the criteria listview with a list
    ''' of criteria from the stage object.
    ''' </summary>
    ''' <remarks>mChoiceStage should be set before
    ''' calling this method.</remarks>
    Private Sub PopulateCriteraList()
        'Set up columns on listview
        Me.CtlListView1.Columns.Clear()
        Me.CtlListView1.Columns.Add(My.Resources.frmStagePropertiesChoice_ChoiceName)
        Me.CtlListView1.Columns.Add(My.Resources.frmStagePropertiesChoice_ChoiceCriterion)
        Me.CtlListView1.LastColumnAutoSize = True

        For Each ch As clsChoice In Me.mChoiceStage.Choices
            Me.AddNewRow(ch)
        Next

        'Add a new row by default if none exist
        If Me.CtlListView1.Rows.Count = 0 Then
            Me.btnAddCriterion_Click(Nothing, Nothing)
        End If

        Me.CtlListView1.UpdateView()
        Me.UpdateButtons()
    End Sub



    ''' <summary>
    ''' Adds a new row to the listview, using the supplied choice node
    ''' where appropriate. Where no choice object is supplied,
    ''' values are simply left blank in the row.
    ''' </summary>
    ''' <remarks>It is necessary to call UpdateView on the
    ''' listview afcter a call to this method.</remarks>
    ''' <param name="ch">The choice object to use when populating
    ''' the new row. Supply a null reference to leave values blank.</param>
    Private Sub AddNewRow(ByVal ch As clsChoice)
        Dim NewRow As _
         New clsChoiceListRow(CtlListView1, Me.mProcessStage, ProcessViewer, ch)

        Dim Index As Integer = Me.CtlListView1.Rows.Count
        If Me.CtlListView1.CurrentEditableRow IsNot Nothing Then
            Index = 1 + Me.CtlListView1.CurrentEditableRow.Index
        End If
        Me.CtlListView1.Rows.Insert(Index, NewRow)
        Me.CtlListView1.UpdateView()
        Me.CtlListView1.CurrentEditableRow = NewRow
    End Sub


    ''' <summary>
    ''' Writes the data in the user interface to the stage object.
    ''' </summary>
    ''' <returns>Returns true on success.</returns>
    ''' <remarks>Calls base class implementation first.
    ''' Things such as the name of the stage are set there.</remarks>
    Protected Overrides Function ApplyChanges() As Boolean
        If MyBase.ApplyChanges() Then
            Me.CtlListView1.EndEditing()

            'clear all choices existing in stage
            Me.mChoiceStage.Choices.Clear()

            'populate stage with all choices in user interface
            For Each r As clsChoiceListRow In Me.CtlListView1.Rows
                ' If we have data in either the name or the expression, add it
                If r.Choice.Name <> "" OrElse Not r.Choice.Expression.IsEmpty Then
                    r.Choice.OwningStage = mChoiceStage
                    mChoiceStage.Choices.Add(r.Choice)
                End If
            Next

            mChoiceStage.Process.ResetChoiceNodePositions(mChoiceStage, mChoiceEnd)
            Return True
        Else
            Return False
        End If
    End Function

#Region "Button Clicks"

    Private Sub btnAddCriterion_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddCriterion.Click
        Me.AddNewRow(New clsChoice(Me.mChoiceStage))
        Me.CtlListView1.UpdateView()
        Me.UpdateButtons()
    End Sub

    Private Sub btnRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemove.Click
        If Not Me.CtlListView1.CurrentEditableRow Is Nothing Then
            Me.CtlListView1.CurrentEditableRow.Remove()
        End If

        Me.CtlListView1.UpdateView()
        Me.UpdateButtons()
    End Sub

    Private Sub btnMoveDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMoveDown.Click
        Me.CtlListView1.MoveEditableRowdown()
        Me.CtlListView1.UpdateView()
        Me.UpdateButtons()
    End Sub

    Private Sub btnMoveUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMoveUp.Click
        Me.CtlListView1.MoveEditableRowUp()
        Me.CtlListView1.UpdateView()
        Me.UpdateButtons()
    End Sub

#End Region

    Private Sub CtlListView1_SelectedRowChanged( _
     ByVal sender As Object, ByVal e As ListRowChangedEventArgs) _
     Handles CtlListView1.EditableRowChanged
        Me.UpdateButtons()
    End Sub

    ''' <summary>
    ''' Enables and disables the buttons in the user interface according to whether
    ''' there is an active row selected etc.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub UpdateButtons()
        Dim SelectedRow As clsListRow = Me.CtlListView1.CurrentEditableRow
        Me.btnAddCriterion.Enabled = True
        Me.btnRemove.Enabled = Not SelectedRow Is Nothing

        Dim SelectedRowIsFirstRow As Boolean = (SelectedRow IsNot Nothing) AndAlso SelectedRow.Index = 0
        Dim SelectedRowIsLastRow As Boolean = (SelectedRow IsNot Nothing) AndAlso SelectedRow.Index = Me.CtlListView1.Rows.Count - 1

        Me.btnMoveUp.Enabled = Me.btnRemove.Enabled AndAlso (Not SelectedRowIsFirstRow)
        Me.btnMoveDown.Enabled = Me.btnRemove.Enabled AndAlso (Not SelectedRowIsLastRow)
    End Sub


    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesChoice.htm"
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
        objDataItemTreeView.Repopulate(displayStage)
    End Sub

    Public Sub Remove(stage As clsDataStage) Implements IDataItemTreeRefresher.Remove
        objDataItemTreeView.RemoveDataItemTreeNode(stage)
    End Sub
End Class