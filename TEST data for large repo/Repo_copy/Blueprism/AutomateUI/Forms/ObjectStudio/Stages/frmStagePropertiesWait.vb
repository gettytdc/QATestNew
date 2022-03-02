Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateAppCore.Utility

Friend Class frmStagePropertiesWait
    Implements IDataItemTreeRefresher
    
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        Dim cols As New List(Of ctlListColumn)
        cols.Add(New ctlListColumn(My.Resources.frmStagePropertiesWait_Element, My.Resources.frmStagePropertiesWait_TheElementWhoseStateIsToBeObserved, 80))
        cols.Add(New ctlListColumn(My.Resources.frmStagePropertiesWait_Params, My.Resources.frmStagePropertiesWait_RuntimeInformationToBePassedToDynamicElements, 50))
        cols.Add(New ctlListColumn(My.Resources.frmStagePropertiesWait_Condition, My.Resources.frmStagePropertiesWait_TheConditionWhichEndsTheWaiting, 80))
        cols.Add(New ctlListColumn(My.Resources.frmStagePropertiesWait_Type, My.Resources.frmStagePropertiesWait_TheDataTypeExpectedForTheValueColumn, 40))
        cols.Add(New ctlListColumn(My.Resources.frmStagePropertiesWait_Comparison, My.Resources.frmStagePropertiesWait_HowTheValueColumnShouldBeComparedToTheStateObservedInTheTargetApplication, 70))
        cols.Add(New ctlListColumn(My.Resources.frmStagePropertiesWait_Value, My.Resources.frmStagePropertiesWait_TheStateThatCausesThisConditionToBeMetEndingTheWaiting, 100))
        
        ctlListPair.RowCreator = AddressOf CreateWaitRow
        ctlListPair.ActionColumns = cols
        ctlListPair.HelpReferenceRetriever = AddressOf getAction
        ctlListPair.RemoveOutputTab()
    End Sub

    Private mApplicationDefinition As clsApplicationDefinition
    Private mAcceptButton As IButtonControl = Nothing


    Private mWaitEndStage As Stages.clsWaitEndStage

    Public Property WaitEnd() As Stages.clsWaitEndStage
        Get
            Return mWaitEndStage
        End Get
        Set(ByVal value As Stages.clsWaitEndStage)
            mWaitEndStage = value
        End Set
    End Property

    Public ReadOnly Property Stage As clsProcessStage Implements IDataItemTreeRefresher.Stage
        Get
            Return ProcessStage
        End Get
    End Property

    Protected Overrides Sub PopulateStageData()
        MyBase.PopulateStageData()

        'Populate application definition in application explorer
        mApplicationDefinition = mProcessStage.Process.ApplicationDefinition
        objApplicationExplorer.LoadApplicationDefinition(mApplicationDefinition)
        objDataItemTreeView.Populate(mProcessStage)

        Dim waitStage As clsWaitStartStage = TryCast(mProcessStage, clsWaitStartStage)
        ctlListPair.ApplicationDefinition = mApplicationDefinition
        ctlListPair.Stage = mProcessStage
        ctlListPair.ProcessViewer = ProcessViewer
        Dim lst As New List(Of IActionStep)
        For Each st As clsWaitChoice In waitStage.Choices
            lst.Add(CType(st, IActionStep))
        Next
        ctlListPair.PopulateActions(lst)

        ctlTimeout.Stage = mProcessStage
        ctlTimeout.ProcessViewer = ProcessViewer

        If waitStage.Timeout <> "" Then
            ctlTimeout.Text = waitStage.Timeout
        Else
            ctlTimeout.Text = "5"   ' default value
        End If

        objDataItemTreeView.ProcessViewer = ProcessViewer

    End Sub


    ''' <summary>
    ''' Updates the stage object with the values in the user interface.
    ''' </summary>
    ''' <returns>Returns True on success, False otherwise.</returns>
    ''' <remarks>See base class documentation for further info.</remarks>
    Protected Overrides Function ApplyChanges() As Boolean

        Dim nameChanged As Boolean
        nameChanged = mProcessStage.GetName <> txtName.Text

        If Not MyBase.ApplyChanges() Then Return False

        ctlListPair.ctlActions.EndEditing()
        ctlListPair.ctlArguments.EndEditing()

        Dim waitStart As Stages.clsWaitStartStage = TryCast(mProcessStage, Stages.clsWaitStartStage)
        Dim oldCount As Integer = waitStart.Choices.Count
        waitStart.Choices.Clear()
        For Each objWaitRow As clsWaitListRow In ctlListPair.ctlActions.Rows
            Dim ch As clsWaitChoice = objWaitRow.WaitChoice
            If ch IsNot Nothing Then
                'Find out if this line is a blank one
                Dim hasCondition As Boolean = (ch.Condition IsNot Nothing)
                Dim hasElement As Boolean = (ch.ElementID <> Guid.Empty)
                Dim hasExpectedReply As Boolean = (Not String.IsNullOrEmpty(ch.ExpectedReply))
                Dim hasParameters As Boolean = ((ch.Parameters IsNot Nothing) AndAlso (ch.Parameters.Count > 0))
                Dim notBlank As Boolean = hasCondition OrElse hasElement OrElse hasExpectedReply OrElse hasParameters

                If notBlank Then
                    Dim e As clsApplicationElement = mApplicationDefinition.FindElement(ch.ElementID)
                    If e Is Nothing Then
                        MessageBox.Show(My.Resources.frmStagePropertiesWait_ElementCouldNotBeFound, My.Resources.frmStagePropertiesWait_MissingElement, MessageBoxButtons.OK)
                        Return False
                    End If

                    UpdateChoiceName(ch)
                    ch.OwningStage = waitStart
                    waitStart.Choices.Add(ch)
                End If
            End If
        Next
        'If there are now more choices than there were originally, we auto
        'update the node positions. Otherwise we leave them alone, because
        'to move them around would be annoying!
        If waitStart.Choices.Count > oldCount Then
            mProcessStage.Process.ResetChoiceNodePositions(waitStart, mWaitEndStage)
        End If

        waitStart.Timeout = ctlTimeout.Text

        If nameChanged Then
            Dim e As clsProcessStage = Process.GetChoiceEnd(waitStart)
            If e IsNot Nothing Then
                e.Name = String.Format(My.Resources.frmStagePropertiesWait_TimeOut0, txtName.Text)
            End If
        End If

        Return True

    End Function

    ''' <summary>
    ''' Updates the choice name to match the element name a choice condition
    ''' </summary>
    Private Sub UpdateChoiceName(ch As clsWaitChoice)
        Dim e As clsApplicationElement = mApplicationDefinition.FindElement(ch.ElementID)
        ch.Name = e.Name & " " & If(ch.Condition IsNot Nothing, ch.Condition.Name, "")
    End Sub

    ''' <summary>
    ''' Creates a wait row representing the supplied step.
    ''' </summary>
    ''' <param name="st">The IActionArgumentStep to represent. We know, although the
    ''' compiler doesn't, that this must be a clsWaitChoice.</param>
    ''' <returns>The newly created row.</returns>
    Private Function CreateWaitRow( _
     ByVal owner As ctlListView, ByVal st As IActionStep) As clsListRow

        Dim waitChoice As clsWaitChoice = CType(st, clsWaitChoice)
        Dim row As New clsWaitListRow( _
         owner, CType(mProcessStage, clsWaitStartStage), ProcessViewer)
        If waitChoice IsNot Nothing AndAlso waitChoice.ElementID <> Guid.Empty Then
            row.WaitChoice = waitChoice
        End If

        AddHandler row.ConditionChanged, AddressOf HandleConditionChanged
        row.ApplicationExplorer = objApplicationExplorer

        Return row

    End Function


    Private Sub HandleConditionChanged(ByVal newWaitChoice As clsWaitChoice)
        ctlListPair.RefreshArguments(newWaitChoice)
    End Sub


    ''' <summary>
    ''' Gets help file for this form.
    ''' </summary>
    ''' <returns>Returns filename for help file.</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesWait.htm"
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


    ''' <summary>
    ''' Gets the 'action' (it's actually a condition for this particular form) used
    ''' in the selected row, if any.
    ''' </summary>
    ''' <returns>Returns the AMI ID of the action being used.</returns>
    Private Function getAction() As String
        If ctlListPair.ctlActions.CurrentEditableRow IsNot Nothing Then
            Dim row As clsWaitListRow = CType(ctlListPair.ctlActions.CurrentEditableRow, clsWaitListRow)
            If row.WaitChoice IsNot Nothing Then
                If row.WaitChoice.Condition IsNot Nothing Then
                    Return row.WaitChoice.Condition.ID
                End If
            End If
        End If

        Return String.Empty
    End Function

    Public Sub Repopulate(displayStage As clsDataStage) Implements IDataItemTreeRefresher.Repopulate
        objDataItemTreeView.Repopulate(displayStage)
    End Sub

    Public Sub Remove(stage As clsDataStage) Implements IDataItemTreeRefresher.Remove
        objDataItemTreeView.RemoveDataItemTreeNode(stage)
    End Sub

    Private Sub OnAcceptButtonStateChanged(sender As Object, e As ValueEventArgs(Of Boolean)) _
        Handles ctlListPair.AcceptButtonToggle

        If e.Value = True Then
            AcceptButton = mAcceptButton
        Else
            mAcceptButton = AcceptButton
            AcceptButton = Nothing
        End If
    End Sub
End Class
