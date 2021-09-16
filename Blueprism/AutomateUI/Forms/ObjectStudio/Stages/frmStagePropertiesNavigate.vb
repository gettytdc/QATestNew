Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Core.Expressions
Imports BluePrism.AutomateAppCore.Utility

Friend Class frmStagePropertiesNavigate

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()
    End Sub


    ''' <summary>
    ''' The navigate stage this form represents.
    ''' </summary>
    ''' <remarks>This reference points to the same
    ''' stage as mobjprocessstage, but means that
    ''' we don't have to do type casting all the time.
    ''' </remarks>
    Private mNavigateStage As Stages.clsNavigateStage

    Private mAcceptButton As IButtonControl = Nothing


    ''' <summary>
    ''' Populates the user interface with stage data.
    ''' </summary>
    Protected Overrides Sub PopulateStageData()
        MyBase.PopulateStageData()

        'Populate application definition in application explorer
        Dim appDefinition As clsApplicationDefinition = mProcessStage.Process.ApplicationDefinition
        ctlApplicationExplorer.LoadApplicationDefinition(appDefinition)

        Dim columns As New List(Of ctlListColumn)
        If appDefinition.ApplicationInfo IsNot Nothing AndAlso appDefinition.IsBrowserAppDefinition() Then
            columns.Add(New ctlListColumn(My.Resources.frmStagePropertiesNavigate_Element, 150))
            columns.Add(New ctlListColumn(My.Resources.frmStagePropertiesNavigate_Params, 40))
            columns.Add(New ctlListColumn(My.Resources.frmStagePropertiesNavigate_Action, 110))
            columns.Add(New ctlListColumn(My.Resources.frmStagePropertiesNavigate_InputsSet, 50))
        Else
            columns.Add(New ctlListColumn(My.Resources.frmStagePropertiesNavigate_Element, 200))
            columns.Add(New ctlListColumn(My.Resources.frmStagePropertiesNavigate_Params, 50))
            columns.Add(New ctlListColumn(My.Resources.frmStagePropertiesNavigate_Action, 150))
            columns.Add(New ctlListColumn(My.Resources.frmStagePropertiesNavigate_InputsSet, 75))

            TableLayoutPanel1.ColumnStyles(2).SizeType = SizeType.Absolute
            TableLayoutPanel1.ColumnStyles(2).Width = 0
            ctlListPair.RemoveOutputTab()
        End If

        ctlListPair.RowCreator = AddressOf CreateNavigationRow
        ctlListPair.ActionColumns = columns
        ctlListPair.HelpReferenceRetriever = AddressOf GetAction
        ctlListPair.SetTreeview(objDataItemTreeView)

        ctlListPair.ApplicationDefinition = appDefinition
        ctlListPair.Stage = mProcessStage
        objDataItemTreeView.ProcessViewer = ProcessViewer
        objDataItemTreeView.Populate(mProcessStage)

        Dim lst As New List(Of IActionStep)
        For Each st As clsStep In mNavigateStage.Steps
            lst.Add(CType(st, IActionStep))
        Next

        ctlListPair.PopulateActions(lst)
    End Sub


    ''' <summary>
    ''' Updates the stage object with the values in the user interface.
    ''' </summary>
    ''' <returns>Returns True on success, False otherwise.</returns>
    ''' <remarks>See base class documentation for further info.</remarks>
    Protected Overrides Function ApplyChanges() As Boolean

        If Not MyBase.ApplyChanges() Then Return False

        ctlListPair.EndEditing()

        'Populate list of navigations in stage object
        mNavigateStage.Steps.Clear()
        For Each row As clsNavigateListRow In ctlListPair.ActionRows
            If row.NavigationAction IsNot Nothing Then _
             mNavigateStage.Steps.Add(row.NavigationAction)
        Next

        mNavigateStage.PauseAfterStepExpression =
            BPExpression.FromLocalised(ctlListPair.PauseAfterStep)

        Return True

    End Function


    ''' <summary>
    ''' The process stage.
    ''' </summary>
    Public Overrides Property ProcessStage As clsProcessStage
        Get
            Return MyBase.ProcessStage
        End Get
        Set
            'This needs to come before the call to
            'base class implementation because
            'base implementation calls PopulateStageData
            mNavigateStage = CType(Value, Stages.clsNavigateStage)
            MyBase.ProcessStage = Value
        End Set
    End Property


    ''' <summary>
    ''' Creates a navigation row representing the supplied navigation action.
    ''' </summary>
    ''' <param name="st">The IActionArgumentStep to represent. We know, although the
    ''' compiler doesn't, that this must be a clsStep.</param>
    ''' <returns>The newly created row.</returns>
    Private Function CreateNavigationRow(
     ByVal owner As ctlListView, ByVal st As IActionStep) As clsListRow

        Dim navAction As clsStep = CType(st, clsStep)
        Dim row As New clsNavigateListRow(owner, mNavigateStage)
        If navAction IsNot Nothing AndAlso (Not navAction.ElementId.Equals(Guid.Empty)) Then
            row.NavigationAction = CType(navAction, clsNavigateStep)
        End If
        row.UpdateInputsStatus()


        AddHandler row.ActionChanged, AddressOf HandleActionChanged
        row.ApplicationExplorer = ctlApplicationExplorer

        Return row

    End Function


    Private Sub HandleActionChanged(ByVal newAction As clsNavigateStep)

        Dim actionID As String = ""
        If newAction.Action IsNot Nothing Then
            actionID = newAction.Action.ID
        End If
        ctlListPair.RefreshArguments(newAction, objDataItemTreeView)

    End Sub


    ''' <summary>
    ''' Gets help file for this form.
    ''' </summary>
    ''' <returns>Returns filename for help file.</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesNavigate.htm"
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
    ''' Gets the action used in the selected row, if any.
    ''' </summary>
    ''' <returns>Returns the AMI ID of the action being used.</returns>
    Private Function GetAction() As String
        If ctlListPair.ctlActions.CurrentEditableRow IsNot Nothing Then
            Dim navRow = CType(ctlListPair.ctlActions.CurrentEditableRow, clsNavigateListRow)
            If navRow.NavigationAction IsNot Nothing AndAlso navRow.NavigationAction.Action IsNot Nothing Then
                Return navRow.NavigationAction.ActionId
            End If
        End If

        Return String.Empty
    End Function

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
