Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages

Friend Class frmStagePropertiesRead

    Private mAcceptButton As IButtonControl = Nothing

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()
    End Sub

    Protected Overrides Sub PopulateStageData()
        MyBase.PopulateStageData()

        Dim Columns As New List(Of ctlListColumn)
        Columns.Add(New ctlListColumn(My.Resources.frmStagePropertiesRead_Element, My.Resources.frmStagePropertiesRead_TheElementWhoseValueIsToBeRead, 100))
        Columns.Add(New ctlListColumn(My.Resources.frmStagePropertiesRead_Params, My.Resources.frmStagePropertiesRead_RuntimeInformationToBeSuppliedToDynamicElements, 50))
        Columns.Add(New ctlListColumn(My.Resources.frmStagePropertiesRead_Data, My.Resources.frmStagePropertiesRead_TheDataToBeReadFromTheElement, 100))
        Columns.Add(New ctlListColumn(My.Resources.frmStagePropertiesRead_DataType, My.Resources.frmStagePropertiesRead_TheDataTypeOfTheElement, 60))
        Columns.Add(New ctlListColumn(My.Resources.frmStagePropertiesRead_StoreIn, My.Resources.frmStagePropertiesRead_TheDestinationDataItemToReceiveTheData, 105))
        Me.ctlListPair.ActionColumns = Columns
        Me.ctlListPair.RowCreator = AddressOf CreateNewRow
        Me.ctlListPair.HelpReferenceRetriever = AddressOf GetCurrentAction

        Dim readStage As clsReadStage = TryCast(mProcessStage, clsReadStage)
        Dim lst As New List(Of IActionStep)
        For Each st As IActionStep In readStage.Steps
            lst.Add(st)
        Next
        ctlListPair.RemoveOutputTab()
        ctlListPair.PopulateActions(lst)

    End Sub


    ''' <summary>
    ''' Creates a read row representing the supplied step.
    ''' </summary>
    ''' <param name="st">The IActionArgumentStep to represent. We know, although the
    ''' compiler doesn't, that this must be a clsStep.</param>
    ''' <returns>The newly created row.</returns>
    Private Function CreateNewRow( _
     ByVal owner As ctlListView, ByVal st As IActionStep) As clsListRow

        Dim row As New clsReadWriteListRow(owner, True, mProcessStage, ProcessViewer)
        row.Treeview = objDataItemTreeView
        row.ApplicationExplorer = CtlApplicationExplorer1

        row.ReadWriteStep = CType(st, clsStep)

        AddHandler row.ActionChanged, AddressOf HandleActionChanged
        Return row

    End Function

    Private Sub HandleActionChanged(ByVal newAction As clsReadStep)
        ctlListPair.RefreshArguments(newAction)
    End Sub

    ''' <summary>
    ''' Returns the appropriate help file for this form
    ''' </summary>
    ''' <returns>The filename of the html help file</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesRead.htm"
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


    Protected Overrides Function GetCurrentAction() As String
        Dim SelectedRow As clsListRow = Me.ctlListPair.ctlActions.CurrentEditableRow
        If SelectedRow IsNot Nothing Then
            Dim objReadWriteRow As clsReadWriteListRow = CType(SelectedRow, clsReadWriteListRow)
            Dim ReadStep As clsReadStep = CType(objReadWriteRow.ReadWriteStep, clsReadStep)
            Dim ReadAction As clsActionTypeInfo = ReadStep.Action()
            If ReadAction IsNot Nothing Then
                Return ReadAction.ID
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
