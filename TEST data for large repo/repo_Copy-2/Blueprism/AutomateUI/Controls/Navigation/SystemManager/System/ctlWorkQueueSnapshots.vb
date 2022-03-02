Imports AutomateControls
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore
Imports BluePrism.Data.DataModels.WorkQueueAnalysis
Imports AutomateUI.My.Resources

Public Class ctlWorkQueueSnapshots : Implements IStubbornChild, IHelp, IPermission

    Private Const HelpFilename = "work-queue-analysis.htm"

    Private Const AssociatedWorkQueuesDisplayLimit = 5
    Private Const ConfigurationNameMaximumWidth = 500
    Private Const ConfigurationNameMinimumWidth = 50
    Private Const ConfigurationNameDefaultWidth = 240
    Private Const ConfigurationEnabledDefaultWidth = 85
    Private Const ConfigurationTimezoneDefaultWidth = 150
    Private Const ConfigurationStartTimeDefaultWidth = 75
    Private Const ConfigurationDayOfWeekDefaultWidth = 75

    Private mParent As frmApplication
    
    Private mConfigurations As List(Of SnapshotConfiguration) = Nothing

    Public Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set
            mParent = Value
        End Set
    End Property

    ''' <summary>
    ''' We are implementing this interface as it will prevent the control from being cached in memory after initial
    ''' creation. This is important as the configuration might get changed on another client and won't be refreshed
    ''' in the current client unless the user switches tabs which could cause confusion. Therefore this way the data
    ''' is refreshed every time the control is accessed therefore the information is more up to date.
    ''' </summary>
    ''' <returns>A boolean value indicating if the control can be closed (always True).</returns>
    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        Return True
    End Function

    Public ReadOnly Property RequiredPermissions As ICollection(Of Permission) Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(Permission.SystemManager.System.Reporting)
        End Get
    End Property

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        RefreshConfigurationsView()
    End Sub

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return HelpFilename
    End Function

    Private Function GetConfigurationsFromServer() As ICollection(Of SnapshotConfiguration)
        Try
           Return gSv.GetSnapshotConfigurations()
        Catch ex As Exception
            UserMessage.Err(ex, ex.Message)
            Return Nothing
        End Try
    End Function

    Private Sub RefreshConfigurationsView()
        mConfigurations = DirectCast(GetConfigurationsFromServer(), List(Of SnapshotConfiguration))
        If mConfigurations Is Nothing Then Return

        Dim listOfConfigurations = mConfigurations.Select(Function(x) New SnapshotConfigurationViewModel(x)).ToList()
        Dim listOfConfigurationsDataTable = ConvertToDataTable(listOfConfigurations)

        SnapshotConfigurationDataGridView.DataSource = listOfConfigurationsDataTable
        SetColumnHeadings()
        SetDefaultColumnWidths()
        SetMinimumColumnWidths()
        EnableSorting()

        SnapshotConfigurationDataGridView.Columns(3).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
    End Sub

    Private Sub SetDefaultColumnWidths()
        SnapshotConfigurationDataGridView.Columns(0).Width = ConfigurationNameDefaultWidth
        SnapshotConfigurationDataGridView.Columns(1).Width = ConfigurationEnabledDefaultWidth
        SnapshotConfigurationDataGridView.Columns(4).Width = ConfigurationStartTimeDefaultWidth
        SnapshotConfigurationDataGridView.Columns(5).Width = ConfigurationStartTimeDefaultWidth
        SnapshotConfigurationDataGridView.Columns(6).Width = ConfigurationDayOfWeekDefaultWidth
        SnapshotConfigurationDataGridView.Columns(7).Width = ConfigurationDayOfWeekDefaultWidth
        SnapshotConfigurationDataGridView.Columns(8).Width = ConfigurationDayOfWeekDefaultWidth
        SnapshotConfigurationDataGridView.Columns(9).Width = ConfigurationDayOfWeekDefaultWidth
        SnapshotConfigurationDataGridView.Columns(10).Width = ConfigurationDayOfWeekDefaultWidth
        SnapshotConfigurationDataGridView.Columns(11).Width = ConfigurationDayOfWeekDefaultWidth
        SnapshotConfigurationDataGridView.Columns(12).Width = ConfigurationDayOfWeekDefaultWidth
    End Sub

    Private Sub SetMinimumColumnWidths()
        SnapshotConfigurationDataGridView.Columns(0).MinimumWidth = ConfigurationNameMinimumWidth
        SnapshotConfigurationDataGridView.Columns(1).MinimumWidth = ConfigurationNameMinimumWidth
        SnapshotConfigurationDataGridView.Columns(2).MinimumWidth = ConfigurationNameMinimumWidth
        SnapshotConfigurationDataGridView.Columns(3).MinimumWidth = ConfigurationTimezoneDefaultWidth
        SnapshotConfigurationDataGridView.Columns(4).MinimumWidth = ConfigurationNameMinimumWidth
        SnapshotConfigurationDataGridView.Columns(5).MinimumWidth = ConfigurationNameMinimumWidth
        SnapshotConfigurationDataGridView.Columns(6).MinimumWidth = ConfigurationNameMinimumWidth
        SnapshotConfigurationDataGridView.Columns(7).MinimumWidth = ConfigurationNameMinimumWidth
        SnapshotConfigurationDataGridView.Columns(8).MinimumWidth = ConfigurationNameMinimumWidth
        SnapshotConfigurationDataGridView.Columns(9).MinimumWidth = ConfigurationNameMinimumWidth
        SnapshotConfigurationDataGridView.Columns(10).MinimumWidth = ConfigurationNameMinimumWidth
        SnapshotConfigurationDataGridView.Columns(11).MinimumWidth = ConfigurationNameMinimumWidth
        SnapshotConfigurationDataGridView.Columns(12).MinimumWidth = ConfigurationNameMinimumWidth
    End Sub

    Private Sub SetColumnHeadings()
        SnapshotConfigurationDataGridView.Columns(0).HeaderText = ctlWorkQueueSnapshots_Resources.ConfigurationColumnHeader_Name
        SnapshotConfigurationDataGridView.Columns(1).HeaderText = ctlWorkQueueSnapshots_Resources.ConfigurationColumnHeader_Enabled
        SnapshotConfigurationDataGridView.Columns(2).HeaderText = ctlWorkQueueSnapshots_Resources.ConfigurationColumnHeader_Interval
        SnapshotConfigurationDataGridView.Columns(3).HeaderText = ctlWorkQueueSnapshots_Resources.ConfigurationColumnHeader_Timezone
        SnapshotConfigurationDataGridView.Columns(4).HeaderText = ctlWorkQueueSnapshots_Resources.ConfigurationColumnHeader_Starttime
        SnapshotConfigurationDataGridView.Columns(5).HeaderText = ctlWorkQueueSnapshots_Resources.ConfigurationColumnHeader_Endtime
        SnapshotConfigurationDataGridView.Columns(6).HeaderText = ctlWorkQueueSnapshots_Resources.ConfigurationColumnHeader_Sunday
        SnapshotConfigurationDataGridView.Columns(7).HeaderText = ctlWorkQueueSnapshots_Resources.ConfigurationColumnHeader_Monday
        SnapshotConfigurationDataGridView.Columns(8).HeaderText = ctlWorkQueueSnapshots_Resources.ConfigurationColumnHeader_Tuesday
        SnapshotConfigurationDataGridView.Columns(9).HeaderText = ctlWorkQueueSnapshots_Resources.ConfigurationColumnHeader_Wednesday
        SnapshotConfigurationDataGridView.Columns(10).HeaderText = ctlWorkQueueSnapshots_Resources.ConfigurationColumnHeader_Thursday
        SnapshotConfigurationDataGridView.Columns(11).HeaderText = ctlWorkQueueSnapshots_Resources.ConfigurationColumnHeader_Friday
        SnapshotConfigurationDataGridView.Columns(12).HeaderText = ctlWorkQueueSnapshots_Resources.ConfigurationColumnHeader_Saturday
    End Sub

    Private Sub EnableSorting()
        For Each column As DataGridViewColumn In SnapshotConfigurationDataGridView.Columns
            column.SortMode = DataGridViewColumnSortMode.Automatic
        Next
    End Sub

    Private Function ConvertToDataTable(Of T)(list As IList(Of T)) As DataTable
        Dim entityType As Type = GetType(T)
        Dim table As New DataTable()
        Dim properties As PropertyDescriptorCollection = TypeDescriptor.GetProperties(entityType)
        For Each prop As PropertyDescriptor In properties
            table.Columns.Add(prop.Name, If(Nullable.GetUnderlyingType(prop.PropertyType), prop.PropertyType))
        Next
        For Each item As T In list
            Dim row As DataRow = table.NewRow()
            For Each prop As PropertyDescriptor In properties
                row(prop.Name) = If(prop.GetValue(item), DBNull.Value)
            Next
            
            table.Rows.Add(row)
        Next
        Return table
    End Function

    Private Sub DeleteConfigurationLinkLabel_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles DeleteConfigurationLinkLabel.LinkClicked
        Try
            DeleteSelectedConfiguration()
        Catch ex As Exception
            UserMessage.Err(ex, ex.message)
        End Try
    End Sub

    Private Sub DeleteSelectedConfiguration()
        Dim selectedConfigurationId = GetSelectedConfigurationId()
        If (selectedConfigurationId <> -1) Then
            Dim nameOfSelectedConfiguration = GetNameOfSelectedConfiguration()
            If ValidateConfigurationNotLinkedToWorkQueues(selectedConfigurationId) Then
                Dim displayMessage = ctlWorkQueueSnapshots_Resources.DeleteSnapshotConfigurationConfirmation_Template
                Dim confirmation = UserMessage.YesNoCancel(String.Format(displayMessage, GetNameOfSelectedConfiguration()))

                If confirmation = MsgBoxResult.Yes Then
                    gSv.DeleteSnapshotConfiguration(selectedConfigurationId, nameOfSelectedConfiguration)
                    RefreshConfigurationsView()
                End If
            End If
        End If
    End Sub

    Private Function ValidateConfigurationNotLinkedToWorkQueues(configurationId As Integer) As Boolean
        If configurationId = -1 Then return False

        Dim associatedWorkQueues = gSv.GetWorkQueueNamesAssociatedToSnapshotConfiguration(configurationId)

        If associatedWorkQueues IsNot Nothing AndAlso associatedWorkQueues.Any()
            DisplayAssociatedWorkQueuesMessage(associatedWorkQueues)
            Return False
        Else
            Return True
        End If
    End Function

    Private sub DisplayAssociatedWorkQueuesMessage(associatedWorkQueues As IEnumerable(Of String))
        Dim stringBuilder = New StringBuilder()
        Dim listOfAssociatedWorkQueues = associatedWorkQueues.ToList()
        Dim numberOfQueuesNotDisplayed = listOfAssociatedWorkQueues.Count - AssociatedWorkQueuesDisplayLimit

        stringBuilder.Append(Environment.NewLine)
        listOfAssociatedWorkQueues.OrderBy(Function(x) x)
        listOfAssociatedWorkQueues.Take(AssociatedWorkQueuesDisplayLimit).ToList().ForEach(
            Sub(x)
                stringBuilder.AppendFormat("- {0}{1}", x, Environment.NewLine)
            End Sub)
        
        If numberOfQueuesNotDisplayed > 0 Then
            Dim message = String.Format(ctlWorkQueueSnapshots_Resources.XNumberOfQueuesNotDisplayed_Template, numberOfQueuesNotDisplayed)
            stringBuilder.AppendFormat("- {0}{1}", message, Environment.NewLine)
        End If

        Dim messagePart1Container = ctlWorkQueueSnapshots_Resources.SnapshotConfigurationAssociatedToWorkQueues_Template
        Dim messagePart1Formatted = String.Format(messagePart1Container, stringBuilder.ToString())
            Dim messagePart2 = ctlWorkQueueSnapshots_Resources.YouMustDissociateWorkQueues
        Dim formattedDisplayMessage = $"{messagePart1Formatted}{Environment.NewLine}{messagePart2}"
            
        UserMessage.Show(formattedDisplayMessage)
    End sub

    Private Function GetSelectedConfigurationId() As Integer
        If SnapshotConfigurationDataGridView.SelectedRows.Count() = 0 Then Return -1
        
        Dim nameOfSelectedConfiguration = GetNameOfSelectedConfiguration()
        Return GetConfigurationIdFromName(nameOfSelectedConfiguration)
    End Function

    Private Function GetNameOfSelectedConfiguration() As String
        Const nameColumnIdentifier As String = NameOf(SnapshotConfiguration.Name)

        Return SnapshotConfigurationDataGridView.SelectedRows(0)?.Cells().Item(nameColumnIdentifier).Value.ToString()
    End Function

    Private Function GetConfigurationIdFromName(nameOfSelectedConfiguration As string) As Integer
        If Not mConfigurations.Any() Then Return -1

        Dim selectedConfiguration = mConfigurations.FirstOrDefault(Function(x) x.Name = nameOfSelectedConfiguration)
        If selectedConfiguration IsNot Nothing Then
            Return selectedConfiguration.Id
        Else
            Return -1
        End If
    End Function
     Private Sub HandleAddConfiguration(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles AddConfigurationLinkLabel.LinkClicked
        Dim newConfig As New SnapshotConfiguration(-1,
                                                False,
                                                ctlWorkQueueSnapshots_Resources.DefaultConfigurationName,
                                                SnapshotInterval.OneHour,
                                                TimeZoneInfo.Local,
                                                Nothing,
                                                Nothing,
                                                New SnapshotDayConfiguration(False, False, False, False, False, False, False))
        Dim f As New SnapshotConfigurationForm(newConfig)
         if f.ShowDialog() = DialogResult.OK
            RefreshConfigurationsView()
        End If
    End Sub
    

    Private Sub HandleEditConfiguration(sender As Object, e As EventArgs) Handles EditConfigurationLinkLabel.LinkClicked, SnapshotConfigurationDataGridView.MouseDoubleClick
        Dim selectedConfig As SnapshotConfiguration = Nothing

        If SnapshotConfigurationDataGridView.SelectedRows.Count > 0 Then
        Dim configName = SnapshotConfigurationDataGridView.SelectedRows(0)?.Cells("Name").Value.ToString
        selectedConfig = gSv.GetSnapshotConfigurationByName(configName)

        Dim f As New SnapshotConfigurationForm(selectedConfig)
            If f.ShowDialog() = DialogResult.OK Then
                RefreshConfigurationsView()
            End If
        End If
    End Sub
End Class
