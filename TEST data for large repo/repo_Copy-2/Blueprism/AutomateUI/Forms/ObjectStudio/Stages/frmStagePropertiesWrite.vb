Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateAppCore.Utility

Friend Class frmStagePropertiesWrite

    Private mAcceptButton As IButtonControl = Nothing

    Protected Overrides Sub PopulateStageData()
        
        MyBase.PopulateStageData()

        'Swap left and right panels
        TableLayoutPanel1.SetColumn(pnlLeft, 2)
        TableLayoutPanel1.SetColumn(pnlRight, 0)
        'Nudge contents to keep gaps as before
        lblDataExplorer.Left += 5
        objDataItemTreeView.Left += 5
        lblApplicationExplorer.Left -= 5
        CtlApplicationExplorer1.Left -= 5

        Dim Columns As New List(Of ctlListColumn)
        Columns.Add(New ctlListColumn(My.Resources.frmStagePropertiesWrite_Value, My.Resources.frmStagePropertiesWrite_TheValueToBeWrittenToTheTargetElement, 100))
        Columns.Add(New ctlListColumn(My.Resources.frmStagePropertiesWrite_Element, My.Resources.frmStagePropertiesWrite_TheElementToRecieveTheSuppliedValue, 105))
        Columns.Add(New ctlListColumn(My.Resources.frmStagePropertiesWrite_Params, My.Resources.frmStagePropertiesWrite_RuntimeInformationToBeSuppliedToDynamicElements, 50))
        Columns.Add(New ctlListColumn(My.Resources.frmStagePropertiesWrite_Type, My.Resources.frmStagePropertiesWrite_TheDataTypeOfTheChosenElement, 60))
        ctlListPair.ActionColumns = Columns
        ctlListPair.RowCreator = AddressOf CreateNewRow
        ctlListPair.HelpReferenceRetriever = AddressOf GetCurrentAction

        Dim objWrite As clsWriteStage = TryCast(mProcessStage, clsWriteStage)
        Dim lst As New List(Of IActionStep)
        For Each st As IActionStep In objWrite.Steps
            lst.Add(st)
        Next
        ctlListPair.RemoveOutputTab()
        ctlListPair.PopulateActions(lst)
        
    End Sub

    ''' <summary>
    ''' Adds a new row to the listview, using the supplied step node
    ''' where appropriate. Where no choice object is supplied,
    ''' values are simply left blank in the row.
    ''' </summary>
    ''' <remarks>It is necessary to call UpdateView on the
    ''' listview afcter a call to this method.</remarks>
    Protected Function CreateNewRow(
     ByVal owner As ctlListView, ByVal st As IActionStep) As clsListRow
        Dim NewRow As New clsReadWriteListRow(
         owner, False, mProcessStage, Me.ProcessViewer)
        NewRow.Treeview = Me.objDataItemTreeView
        NewRow.ApplicationExplorer = Me.CtlApplicationExplorer1
        NewRow.ReadWriteStep = CType(st, clsStep)

        AddHandler NewRow.ElementChanged, AddressOf HandleElementChanged
        Return NewRow
    End Function

    Private Sub HandleElementChanged(ByVal newAction As clsWriteStep)
        ctlListPair.RefreshArguments(newAction)
    End Sub
    ''' <summary>
    ''' Returns the appropriate help file for this form
    ''' </summary>
    ''' <returns>The filename of the html help file</returns>
    Public Overrides Function GetHelpFile() As String
        Return My.Resources.frmStagePropertiesWrite_FrmStagePropertiesWriteHtm
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
