Imports AutomateControls
Imports AutomateUI.My.Resources
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.My.Resources

Public Class ctlSystemWebConnectionSettings : Implements IPermission, IStubbornChild, IHelp
    Private mParent As frmApplication
    Private mSavePromptActioned As Boolean = False

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Dim settings = gSv.GetWebConnectionSettings()
        numConnectionLimit.Value = settings.ConnectionLimit
        numMaxIdleTime.Value = settings.MaxIdleTime

        Dim connectionTimeout = settings.ConnectionLeaseTimeout
        If connectionTimeout Is Nothing Then
            chkConnectionTimeout.Checked = False
            ConnectionTimeoutEnabled = False
        Else
            chkConnectionTimeout.Checked = True
            numConnectionTimeout.Value = CDec(settings.ConnectionLeaseTimeout)
            ConnectionTimeoutEnabled = True
        End If

        For Each uriSetting In settings.UriSpecificSettings
            dgvUriSettings.Rows.Add(Nothing,
                                    uriSetting.BaseUri,
                                    uriSetting.ConnectionLimit,
                                    uriSetting.MaxIdleTime,
                                    uriSetting.ConnectionLeaseTimeout)
        Next
    End Sub

    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(Permission.SystemManager.BusinessObjects.WebServicesWebApi)
        End Get
    End Property

    Public Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Private Sub btnSaveChanges_Click(sender As Object, e As EventArgs) Handles btnSaveChanges.Click
        Save()
    End Sub

    Private Function Save() As Boolean
        Try
            Dim connectionTimeout As Integer?
            If chkConnectionTimeout.Checked Then
                connectionTimeout = Decimal.ToInt32(numConnectionTimeout.Value)
            End If
            Dim settings = New WebConnectionSettings(
                Decimal.ToInt32(numConnectionLimit.Value),
                Decimal.ToInt32(numMaxIdleTime.Value),
                connectionTimeout,
                GetUriSettings())

            gSv.UpdateWebConnectionSettings(settings)

            Dim message = New StringBuilder()
            message.AppendLine(ctlSystemWebConnectionSettings_Resources.SuccessfulSaveNotification)
            message.AppendLine()
            message.AppendLine(ctlSystemWebConnectionSettings_Resources.ResetAdvice)
            UserMessage.OK(message.ToString())
            Return True
        Catch ex As Exception
            UserMessage.Err(ex.Message)
            Return False
        End Try
    End Function

    Private Function GetUriSettings() As IEnumerable(Of UriWebConnectionSettings)
        Dim result = New List(Of UriWebConnectionSettings)
        Dim getValue = Function(row As DataGridViewRow, col As DataGridViewColumn) As String
                           Return row.Cells.Item(col.Name).Value?.ToString()
                       End Function

        For Each row As DataGridViewRow In dgvUriSettings.Rows
            If row.IsNewRow Then Continue For

            Dim uriString = getValue(row, colUri)
            If uriString Is Nothing Then Continue For

            Dim connLimitText = getValue(row, colConnectionLimit)
            If String.IsNullOrEmpty(connLimitText) Then _
                Throw New ArgumentException(WebConnectionSettingsResources.Error_MissingConnectionLimit)

            Dim connectionLimit As Integer
            If Not Integer.TryParse(connLimitText, connectionLimit) Then _
                Throw New ArgumentException(String.Format(WebConnectionSettingsResources.Error_InvalidConnectionLimit_Template, connLimitText))

            Dim nonNullTimeout As Integer
            Dim connTimeoutText = getValue(row, colConnectionTimeout)

            Dim connectionTimeout As Integer? = Nothing
            If Not String.IsNullOrEmpty(connTimeoutText) Then
                If Not Integer.TryParse(connTimeoutText, nonNullTimeout) Then _
                    Throw New ArgumentException(String.Format(WebConnectionSettingsResources.Error_InvalidConnectionLeaseTimeout_Template, connTimeoutText))

                connectionTimeout = nonNullTimeout
            End If

            Dim maxIdleTimeText = getValue(row, colMaxIdleTime)
            If String.IsNullOrEmpty(maxIdleTimeText) Then _
                Throw New ArgumentException(WebConnectionSettingsResources.Error_MissingMaxIdleTime)

            Dim maxIdleTime As Integer
            If Not Integer.TryParse(maxIdleTimeText, maxIdleTime) Then _
                Throw New ArgumentException(String.Format(WebConnectionSettingsResources.Error_InvalidMaxIdleTime_Template, maxIdleTimeText))
            result.Add(New UriWebConnectionSettings(uriString, connectionLimit, connectionTimeout, maxIdleTime))
        Next

        Return result
    End Function

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave

        If mSavePromptActioned Then Return True

        Try
            For Each row As DataGridViewRow In dgvUriSettings.Rows
                Dim uriString = row.Cells.Item(colUri.Name).Value?.ToString()
                If uriString Is Nothing Then Continue For
                Dim uri = New Uri(uriString)
            Next
            Dim savedSettings = gSv.GetWebConnectionSettings()
            Dim connectionTimeout As Integer?
            If chkConnectionTimeout.Checked Then
                connectionTimeout = Decimal.ToInt32(numConnectionTimeout.Value)
            End If
            Dim currentSettings = New WebConnectionSettings(
                Decimal.ToInt32(numConnectionLimit.Value),
                Decimal.ToInt32(numMaxIdleTime.Value),
                connectionTimeout,
                GetUriSettings())
            If savedSettings.Equals(currentSettings) Then Return True
        Catch
            ' We must have unsaved uri changes if one of them is invalid, so continue
        End Try

        Dim msg As String = ctlSystemWebConnectionSettings_ThereAreUnsavedChangesToTheWebConnectionSettingsDoYouWantToSaveTheChanges

        Dim response = UserMessage.YesNoCancel(msg, True)
        Dim result As Boolean
        Select Case response
            Case MsgBoxResult.No : result = True
            Case MsgBoxResult.Yes : result = Save()
            Case Else : result = False
        End Select

        mSavePromptActioned = result

        Return result

    End Function

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "Web API/HTML/api-web-connection.htm"
    End Function

    Private Sub dgvUriSettings_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvUriSettings.CellContentClick
        Dim shouldDelete = e.ColumnIndex = colDelete.Index AndAlso
                                dgvUriSettings.Rows().Count > 1 AndAlso
                                e.RowIndex <> dgvUriSettings.Rows().Count - 1

        If shouldDelete Then dgvUriSettings.Rows.RemoveAt(e.RowIndex)
    End Sub

    Private Sub CellFormatting(sender As Object,
                    e As DataGridViewCellFormattingEventArgs) Handles dgvUriSettings.CellFormatting

        If e.ColumnIndex = colDelete.Index Then
            e.Value = ToolImages.Bin_16x16
        End If
    End Sub

    Private Sub dgvUriSettings_DefaultValuesNeeded(sender As Object, e As DataGridViewRowEventArgs) _
        Handles dgvUriSettings.DefaultValuesNeeded

        e.Row.Cells(colConnectionLimit.Name).Value = numConnectionLimit.Value
        e.Row.Cells(colMaxIdleTime.Name).Value = numMaxIdleTime.Value
        If chkConnectionTimeout.Checked Then
            e.Row.Cells(colConnectionTimeout.Name).Value = Decimal.ToInt32(numConnectionTimeout.Value)
        End If

    End Sub

    Private Sub dgvUriSettings_EditingControlShowing(sender As Object,
                                                     e As DataGridViewEditingControlShowingEventArgs) _
        Handles dgvUriSettings.EditingControlShowing

        Dim txtEdit = DirectCast(e.Control, TextBox)
        AddHandler txtEdit.KeyPress, AddressOf Handle_dgvCellKeyPress
    End Sub

    Private Sub Handle_dgvCellKeyPress(sender As Object, e As KeyPressEventArgs)
        Dim numericColumnIndexes = {colConnectionLimit.Index,
                                    colMaxIdleTime.Index,
                                    colConnectionTimeout.Index}

        Dim isNumericColumn As Boolean =
                numericColumnIndexes.Contains(dgvUriSettings.CurrentCell.ColumnIndex)

        e.Handled = isNumericColumn AndAlso ShouldNotHandle(e.KeyChar)
    End Sub

    Private Function ShouldNotHandle(c As Char) As Boolean
        Return Not Char.IsDigit(c) AndAlso Not Char.IsControl(c)
    End Function

    Private Sub chkLease_CheckedChanged(sender As Object, e As EventArgs) Handles chkConnectionTimeout.CheckedChanged
        ConnectionTimeoutEnabled = chkConnectionTimeout.Checked
    End Sub

    Private Property ConnectionTimeoutEnabled As Boolean
        Get
            Return numConnectionTimeout.Enabled
        End Get
        Set(value As Boolean)
            If Not value Then
                numConnectionTimeout.Text = String.Empty
            ElseIf numConnectionTimeout.Text = String.Empty Then
                numConnectionTimeout.Value = numConnectionTimeout.Minimum
                numConnectionTimeout.Text = numConnectionTimeout.Value.ToString
            End If
            numConnectionTimeout.Enabled = value
        End Set
    End Property


End Class
