Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateAppCore
Imports BluePrism.DatabaseInstaller
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.Server.Domain.Models
Imports System.Data.SqlClient

Public Class ConnectionManagerPanel

    Private DefaultConnectionName As String = My.Resources.ConnectionManagerPanel_Default_Connection

    Private Const LocalDbName As String = "LocalDB Connection"
    ''' <summary>
    ''' Gets the existing connections from the config file and populates the 
    ''' user interface. The active connection will be selected.
    ''' </summary>
    Friend Sub LoadConnections()

        lvConnections.BeginUpdate()
        lvConnections.Items.Clear()
        Dim configOptions = Options.Instance
        For Each c As clsDBConnectionSetting In configOptions.Connections
            Dim item As New ListViewItem(c.ConnectionName)
            item.Name = c.ConnectionName
            item.Tag = c.Clone()
            item.Selected = False
            lvConnections.Items.Add(item)
        Next

        'if no connections in list then create a new one
        If lvConnections.Items.Count = 0 Then
            Dim item As New ListViewItem(DefaultConnectionName)
            item.Name = DefaultConnectionName
            Dim cs As clsDBConnectionSetting = CType(configOptions.DbConnectionSetting.Clone(), clsDBConnectionSetting)
            If cs.ConnectionName = "" Then cs.ConnectionName = DefaultConnectionName
            item.Tag = cs
            item.Selected = False
            lvConnections.Items.Add(item)
        End If


        'make the current connection selected
        Dim curr As clsDBConnectionSetting = configOptions.DbConnectionSetting
        For Each item As ListViewItem In lvConnections.Items
            Dim setting As clsDBConnectionSetting = TryCast(item.Tag, clsDBConnectionSetting)
            If setting IsNot Nothing AndAlso curr.ConnectionName = setting.ConnectionName Then
                item.Selected = True
                Exit For
            End If
        Next
        UpdateControls(curr)

        colName.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent)
        lvConnections.EndUpdate()

        If lvConnections.SelectedItems.Count = 0 Then connDetailPanel.ConnectionSetting = Nothing

    End Sub

    Private Sub UpdateControls(connection As clsDBConnectionSetting)
        Dim shouldEnable = connection Is Nothing OrElse Not connection.ConnectionName = LocalDbName
        btnDelete.Enabled = shouldEnable
        UpdateButtons(connDetailPanel.ConnectionSetting)

        For Each control As Control In connDetailPanel.panMain.Controls
            control.Enabled = shouldEnable
        Next
    End Sub


    ''' <summary>
    ''' Writes out the configuration represented in the user interface to the
    ''' config file. Shows error message to user if an error is encountered.
    ''' </summary>
    ''' <exception cref="BluePrismException">If an error occurred while attempting to
    ''' save the options to the file.</exception>
    Friend Sub SaveConnections()
        Dim configOptions = Options.Instance
        'first clear all connections from the connections collection
        configOptions.ClearAllConnections()
        'now write each connection to the connections collection
        For Each item As ListViewItem In Me.lvConnections.Items
            Dim setting As clsDBConnectionSetting = TryCast(item.Tag, clsDBConnectionSetting)
            If setting IsNot Nothing Then configOptions.AddConnection(setting)
        Next

        'Write the current (selected) connection as the active one
        configOptions.DbConnectionSetting = connDetailPanel.ConnectionSetting
        Try
            configOptions.Save()
        Catch ex As Exception
            Throw New BluePrismException(
                My.Resources.CouldNotWriteToConfigurationFile0, ex.Message)
        End Try

    End Sub

    ''' <summary>
    ''' Creates a unique connection name beginning with the specified stub
    ''' </summary>
    ''' <param name="stub">The stub for the connection name - a number is inserted
    ''' to this in order to create the connection name. Format string is expected,
    ''' eg: "Connection {0}"</param>
    ''' <returns>A name generated from the given stub with a number appended to it,
    ''' unique amongst the connection held in this connection manager.</returns>
    Private Function CreateUniqueConnectionName(ByVal stub As String) As String
        Dim i As Integer = 0
        Dim name As String
        Do
            i += 1
            name = String.Format(stub, i)
        Loop While lvConnections.Items.ContainsKey(name)
        Return name
    End Function

    ''' <summary>
    ''' Handles a new connection being requested
    ''' </summary>
    Private Sub HandleNewConnection(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnNew.Click
        Dim item As New ListViewItem(CreateUniqueConnectionName(My.Resources.ConnectionManagerPanel_unique_connection_format))
        item.Name = item.Text
        item.Tag = New clsDBConnectionSetting(item.Text)
        lvConnections.Items.Add(item)
        lvConnections.SelectedItems.Clear()
        item.Selected = True
        colName.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent)
    End Sub

    ''' <summary>
    ''' Handles a connection being deleted.
    ''' </summary>
    Private Sub HandleDeleteConnection(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnDelete.Click

        If lvConnections.Items.Count = 1 Then MessageBox.Show(
         My.Resources.YouMayNotDeleteTheLastConnectionHoweverYouMayRenameItAndChangeAllOfItsOtherProp) : Return

        If lvConnections.SelectedItems.Count = 0 Then MessageBox.Show(
         My.Resources.PleaseFirstSelectAConnectionToDelete) : Return

        Dim item As ListViewItem = lvConnections.SelectedItems(0)
        Dim index As Integer = item.Index
        item.Remove()
        While index > lvConnections.Items.Count - 1
            index -= 1
        End While
        lvConnections.Items(index).Selected = True

    End Sub

    ''' <summary>
    ''' Handles a connection being selected in the listview.
    ''' </summary>
    Private Sub HandleConnectionSelected(ByVal sender As Object, ByVal e As EventArgs) _
     Handles lvConnections.SelectedIndexChanged

        Dim setting As clsDBConnectionSetting = Nothing
        If lvConnections.SelectedItems.Count > 0 Then
            setting = TryCast(lvConnections.SelectedItems(0).Tag, clsDBConnectionSetting)
            UpdateButtons(setting)
        End If
        connDetailPanel.ConnectionSetting = setting
        UpdateControls(setting)
    End Sub

    ''' <summary>
    ''' Update the database create/upgrade/configure buttons
    ''' </summary>
    ''' <param name="setting"></param>
    Private Sub UpdateButtons(setting As clsDBConnectionSetting)
        If setting Is Nothing Then
            UpgradeButtonsEnabled(False)
        Else
            Select Case setting.ConnectionType
                Case ConnectionType.BPServer, ConnectionType.None
                    UpgradeButtonsEnabled(False)
                Case Else
                    UpgradeButtonsEnabled(True)
            End Select
        End If
    End Sub

    ''' <summary>
    ''' Update the enabled property of the database create/upgrade/configure buttons
    ''' </summary>
    ''' <param name="enabled"></param>
    Private Sub UpgradeButtonsEnabled(enabled As Boolean)
        CreateDatabaseButton.Enabled = enabled
        UpgradeDatabaseButton.Enabled = enabled
        ConfigureDatabaseButton.Enabled = enabled
    End Sub

    Public Shared Property IsConnectionNameUnique As Boolean = True

    ''' <summary>
    ''' Handles the name being validated in the connection detail panel
    ''' </summary>
    Private Sub HandleNameValidated(ByVal sender As Object, ByVal e As EventArgs) _
     Handles connDetailPanel.NameValidated
        Dim setting = connDetailPanel.ConnectionSetting

        For Each item As ListViewItem In lvConnections.Items
            If item.Tag Is setting Then Continue For ' We ignore changes to the current connection
            If item.Text = ConnectionDetail.CurrentConnectionName Then
                IsConnectionNameUnique = False
                Return
            End If
        Next
        IsConnectionNameUnique = True
        ' See if the name already exists in the listview
        For Each item As ListViewItem In lvConnections.Items
            If item.Tag Is setting Then
                item.Text = setting.ConnectionName
                item.Name = setting.ConnectionName
                colName.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent)
                Return
            End If
        Next
    End Sub

    ''' <summary>
    ''' Handles the connection type changing
    ''' </summary>
    Private Sub HandleConnectionTypeChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles connDetailPanel.ConnectionTypeChanged
        Dim setting = connDetailPanel.ConnectionSetting
        UpdateButtons(setting)
    End Sub

    Private Function RequiredFieldsCompleted() As Boolean

        Dim warning = New StringBuilder()
        warning.AppendLine(My.Resources.PleaseCompleteTheseFields)
        Dim defaultWarningLength = warning.Length

        Dim requiresUserCredentials As Boolean = ConnectionDetail.SelectedConnectionIndex = 0 OrElse
            ConnectionDetail.SelectedConnectionIndex = 3

        If ConnectionDetail.IsConnectionNameEmpty Then warning.AppendLine(My.Resources.ConnectionName)
        If ConnectionDetail.IsDBServerEmpty Then warning.AppendLine(My.Resources.DatabaseServer)
        If ConnectionDetail.IsDBNameEmpty Then warning.AppendLine(My.Resources.DatabaseName)


        If requiresUserCredentials AndAlso ConnectionDetail.IsUserIDEmpty Then warning.AppendLine(My.Resources.UserID)

        If warning.Length <> defaultWarningLength Then
            MessageBox.Show(warning.ToString())
            Return False
        End If

        Try
            Dim setting = connDetailPanel.ConnectionSetting
            Dim builder = New SqlConnectionStringBuilder(setting.ExtraParams)
            Return True
        Catch ex As Exception
            UserMessage.Err(My.Resources.InvalidAdditionalSqlParametersEntered, ex)
        End Try
        Return False
    End Function

    Private Sub CreateDatabase(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CreateDatabaseButton.Click
        If Not RequiredFieldsCompleted() Then Return
        Using f As New CreateDatabaseForm()
            f.ConnectionSetting = connDetailPanel.ConnectionSetting
            f.ShowInTaskbar = False
            f.ShowDialog()
        End Using
    End Sub

    Private Sub UpgradeDatabase(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles UpgradeDatabaseButton.Click
        If Not RequiredFieldsCompleted() Then Return
        Using f As New UpgradeDatabaseForm()
            f.ConnectionSetting = connDetailPanel.ConnectionSetting
            f.ShowInTaskbar = False
            f.ShowDialog()
        End Using
    End Sub

    Private Sub ConfigureDatabase(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ConfigureDatabaseButton.Click

        If Not RequiredFieldsCompleted() Then Return
        Dim setting = connDetailPanel.ConnectionSetting
        Dim factory = DependencyResolver.Resolve(Of Func(Of ISqlDatabaseConnectionSetting, TimeSpan, String, String, IInstaller))
        Dim installer = factory(
            setting.CreateSqlSettings(),
            Options.Instance.DatabaseInstallCommandTimeout,
            ApplicationProperties.ApplicationName,
            clsServer.SingleSignOnEventCode)

        Try
            installer.GetCurrentDBVersion()
        Catch ex As Exception
            MessageBox.Show(String.Format(My.Resources.CannotConfigureThisDatabase0, ex.Message))
        End Try
        If installer.GetNumberOfUsers() <> 0 Then
            MessageBox.Show(My.Resources.ThisDatabaseIsAlreadyConfigured)
            Return
        End If

        Using f As New CreateDatabaseForm()
            f.Configure = True
            f.ConnectionSetting = setting
            f.ShowDialog()
        End Using
    End Sub

End Class
