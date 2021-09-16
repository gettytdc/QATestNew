Imports AutomateControls.Wizard
Imports AutomateUI.Wizards.SystemManager.DataGateways.Helpers
Imports BluePrism.DataPipeline.DataPipelineOutput

Public Class ctlChooseSessionLogFields
    Inherits WizardPanel
    Implements IDataGatewaysWizardPanel

    Private ReadOnly mConfig As DataPipelineOutputConfig

    Private mSelectedSessionLogFields As New Dictionary(Of String, Boolean)

    Public Sub New(dataPipelineConfigOutput As DataPipelineOutputConfig)
        InitializeComponent()
        mConfig = dataPipelineConfigOutput
    End Sub

    Public Sub OnOpen() Implements IDataGatewaysWizardPanel.OnOpen
        RefreshLabels()
        ClearHistoricData()
        LoadSessionLogFieldsIntoCheckList()

        If mConfig.SelectedSessionLogFields IsNot Nothing AndAlso mConfig.SelectedSessionLogFields.Count > 0 Then
            For Each CheckedField In mConfig.SelectedSessionLogFields

                Dim index = chkListSessionLogFields.Items.IndexOf(New SessionLogField("", CheckedField))
                If index >= 0 Then chkListSessionLogFields.SetItemChecked(index, True)
                mSelectedSessionLogFields(CheckedField) = True
            Next
        Else
            SetAllSessionLogsDataStatus(True)
            SetAllSessionLogsCheckedState(CheckState.Checked)
        End If

        IsInputCorrect()

        If mSelectedSessionLogFields.Any(Function(X) X.Value) Then
            NavigateNext = True
            UpdateNavigate()
        End If
    End Sub

    Public Sub Closing() Implements IDataGatewaysWizardPanel.Closing
        Dim checkedFields As New List(Of String)
        For Each item In mSelectedSessionLogFields
            If item.Value = True Then checkedFields.Add(item.Key)
        Next
        mConfig.SelectedSessionLogFields = checkedFields
    End Sub

#Region "Private methods"
    Private Sub RefreshLabels()
        lblSelectedConfigName.Text = mConfig.Name
        lblChooseFields.Text = String.Format(My.Resources.lblChooseDataToSend_text, GetLocalizedFriendlyNameToLower(mConfig.OutputType.Id))
    End Sub

    Private Sub ClearHistoricData()
        If chkListSessionLogFields.Items.Count > 0 Then
            chkListSessionLogFields.Items.Clear()
        End If
        If mSelectedSessionLogFields.Count > 0 Then
            mSelectedSessionLogFields.Clear()
        End If
    End Sub

    Private Sub IsInputCorrect()
        If mSelectedSessionLogFields.Any(Function(X) X.Value) Then
            NavigateNext = True
        Else
            NavigateNext = False
        End If
        UpdateNavigate()
    End Sub

    Private Sub LoadSessionLogFieldsIntoCheckList()
        For Each sessionLogField In mConfig.SessionLogFields
            chkListSessionLogFields.Items.Add(
                                    New SessionLogField(My.Resources.ResourceManager.GetString(String.Format("ctlChooseSessionLogFields_{0}", sessionLogField), My.Resources.Culture),
                                    sessionLogField.ToString()), False)

            mSelectedSessionLogFields.Add(sessionLogField.ToString(), False)
        Next
    End Sub

    Private Sub SetAllSessionLogsDataStatus(status As Boolean)
        Dim fieldList = chkListSessionLogFields.Items.Cast(Of SessionLogField).Select(Function(x) x.Name).ToList()
        For Each field In fieldList
            mSelectedSessionLogFields(field) = status
        Next
    End Sub

    Private Sub SetAllSessionLogsCheckedState(checkedState As CheckState)
        For i As Integer = 0 To chkListSessionLogFields.Items.Count - 1
            chkListSessionLogFields.SetItemCheckState(i, checkedState)
        Next
    End Sub

    Private Sub SelectedIndexChanged(sender As Object, e As EventArgs) Handles chkListSessionLogFields.SelectedIndexChanged
        chkListSessionLogFields.ClearSelected()
        Dim fieldsList = chkListSessionLogFields.Items.Cast(Of SessionLogField).Select(Function(x) x.Name).ToList()

        For Each field In fieldsList
            mSelectedSessionLogFields(field) = chkListSessionLogFields.CheckedItems.Cast(Of SessionLogField).Select(Function(x) x.Name).Contains(field)
        Next
        IsInputCorrect()

    End Sub

    Private Sub btnDeselectAll_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles btnDeselectAll.LinkClicked
        SetAllSessionLogsDataStatus(False)
        SetAllSessionLogsCheckedState(CheckState.Unchecked)
        IsInputCorrect()
    End Sub

    Private Sub btnSelectAll_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles btnSelectAll.LinkClicked
        SetAllSessionLogsDataStatus(True)
        SetAllSessionLogsCheckedState(CheckState.Checked)
        IsInputCorrect()
    End Sub

#End Region

    Private Class SessionLogField
        Public Property Text As String
        Public Property Name As String

        Public Sub New(textInput As String, nameInput As String)
            Text = textInput
            Name = nameInput
        End Sub

        Public Overrides Function Equals(obj As Object) As Boolean
            If (obj Is Nothing) OrElse obj Is DBNull.Value Then
                Return False
            End If

            Return DirectCast(obj, SessionLogField).Name = Me.Name
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return Me.Name.GetHashCode()
        End Function

        Public Overrides Function ToString() As String
            Return Text
        End Function

    End Class

End Class
