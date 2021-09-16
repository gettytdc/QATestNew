Imports AutomateControls
Imports AutomateControls.TreeList
Imports BluePrism.AutomateAppCore
Imports BluePrism.Core.Utility
Imports BluePrism.DataPipeline

Namespace Controls.Navigation.DataGateways
    Public Class ctlDataGateways
        Implements IRefreshable, IHelp

        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            AddHandler Me.ProcessTreeListView.ContextMenuStrip.Opening, AddressOf ContextMenuStripOpening
        End Sub

        Private Sub ContextMenuStripOpening(sender As Object, e As CancelEventArgs)
            If ProcessTreeListView.SelectedItems.Count = 0 Then
                e.Cancel = True
                Return
            End If

            Try
                Dim selectedProcesses = ProcessTreeListView.SelectedItems.GetEnumerator()
                If ProcessTreeListView.SelectedItems.Count > 1 Then
                    Me.btnStartDataGatewayProcess.Text = My.Resources.DataGatewysStartAllSelected
                    Me.btnStopDataGatewayProcess.Text = My.Resources.DataGatewaysStopAllSelected
                    Me.btnStartDataGatewayProcess.Enabled = True
                    Me.btnStopDataGatewayProcess.Enabled = True
                Else
                    Me.btnStartDataGatewayProcess.Text = My.Resources.DataGatewaysStart
                    Me.btnStopDataGatewayProcess.Text = My.Resources.DataGatewaysStop
                    'Only one process, so determine its state and show option based on that
                    selectedProcesses.MoveNext()

                    Dim selectedProcess = TryCast(selectedProcesses.Current, TreeListViewItem)
                    Dim processStatus = TryCast(selectedProcess?.Tag, DataGatewayProcessStatusInformation).Status

                    Select Case processStatus
                        Case DataGatewayProcessState.Running
                            Me.btnStartDataGatewayProcess.Enabled = False
                            Me.btnStopDataGatewayProcess.Enabled = True
                        Case DataGatewayProcessState.Starting
                            Me.btnStartDataGatewayProcess.Enabled = False
                            Me.btnStopDataGatewayProcess.Enabled = True
                        Case DataGatewayProcessState.Error
                            Me.btnStartDataGatewayProcess.Enabled = False
                            Me.btnStopDataGatewayProcess.Enabled = True
                        Case DataGatewayProcessState.UnrecoverableError
                            Me.btnStartDataGatewayProcess.Enabled = False
                            Me.btnStopDataGatewayProcess.Enabled = True
                        Case DataGatewayProcessState.Offline
                            Me.btnStartDataGatewayProcess.Enabled = False
                            Me.btnStopDataGatewayProcess.Enabled = False
                        Case DataGatewayProcessState.Online
                            Me.btnStartDataGatewayProcess.Enabled = True
                            Me.btnStopDataGatewayProcess.Enabled = False
                        Case DataGatewayProcessState.Unknown
                            Me.btnStartDataGatewayProcess.Enabled = False
                            Me.btnStopDataGatewayProcess.Enabled = False
                    End Select
                End If
            Catch ex As Exception
                UserMessage.Err(ex, ex.Message)
            End Try

        End Sub

        Public Property ControlRoom As ctlControlRoom

        Private Sub RefreshProcessStatusView()
            Try
                Dim processes = gSv.GetDataGatewayProcesses()

                ProcessTreeListView.BeginUpdate()
                ProcessTreeListView.Items.Clear()

                For Each process As DataGatewayProcessStatusInformation In processes
                    Dim index = processes.IndexOf(process)
                    Dim statusAsText = DataGatewayResources.ResourceManager.EnsureString("DataGatewayProcessState_{0}", process.Status)

                    Dim treeItem = New TreeListViewItem(process.Name)
                    treeItem.SubItems.Add(statusAsText)
                    treeItem.SubItems.Add(process.ErrorMessage)
                    treeItem.Font = New Font("Segoe UI", 8, FontStyle.Regular)
                    treeItem.Tag = process

                    ProcessTreeListView.Items.Insert(treeItem, index)
                Next

                ProcessTreeListView.ExpandAll()
                ProcessTreeListView.Items.Sort(recursively:=True)
                ProcessTreeListView.EndUpdate()

                CheckIfErrorStateChanged(processes)
            Catch ex As Exception
                RefreshTimer.Stop()
                UserMessage.Err(ex, ex.Message)
            End Try
        End Sub

        Private Sub CheckIfErrorStateChanged(processes As List(Of DataGatewayProcessStatusInformation))
            If ControlRoom Is Nothing Then Return

            If ControlRoom.DataGatewayProcessError Then
                If processes.All(Function(x) Not x.IsInErrorState) Then
                    ControlRoom.ToggleDataGatewaysIcon()
                End If
            Else
                If processes.Any(Function(x) x.IsInErrorState) Then
                    ControlRoom.ToggleDataGatewaysIcon()
                End If
            End If
        End Sub

        Private Sub ResizeColumnWidths()
            Const offset = 7
            Dim columnWidth = Convert.ToInt32((ProcessTreeListView.Width - offset) / ProcessTreeListView.Columns.Count)

            ProcessTreeListView.Columns(0).Width = columnWidth
            ProcessTreeListView.Columns(1).Width = columnWidth
            ProcessTreeListView.Columns(2).Width = columnWidth
        End Sub

        Private Sub RefreshTimer_Tick(sender As Object, e As EventArgs) Handles RefreshTimer.Tick
            RefreshTimer.Stop()
            RefreshView()
            RefreshTimer.Start()
        End Sub

        Private Sub ctlDataGateways_Load(sender As Object, e As EventArgs) Handles MyBase.Load
            ResizeColumnWidths()
            RefreshProcessStatusView()
            RefreshTimer.Interval = gSv.GetDataPipelineSettings().MonitoringFrequency * 1000
            RefreshTimer.Start()
        End Sub

        Private Sub ProcessTreeLIstView_Resize(sender As Object, e As EventArgs) Handles ProcessTreeListView.Resize
            ResizeColumnWidths()
        End Sub

        Private Sub btnStartDataGatewayProcess_Click(sender As Object, e As EventArgs) Handles btnStartDataGatewayProcess.Click
            SendCommandToSelectedProcesses(DataPipelineProcessCommand.StartProcess)
        End Sub

        Private Sub btnStopDataGatewayProcess_Click(sender As Object, e As EventArgs) Handles btnStopDataGatewayProcess.Click
            SendCommandToSelectedProcesses(DataPipelineProcessCommand.StopProcess)
        End Sub

        Private Sub SendCommandToSelectedProcesses(command As DataPipelineProcessCommand)
            If ProcessTreeListView.SelectedItems.Count = 0 Then Return

            Try
                Dim selectedProcesses = ProcessTreeListView.SelectedItems.GetEnumerator()

                While (selectedProcesses.MoveNext())
                    Dim selectedProcess = TryCast(selectedProcesses.Current, TreeListViewItem)
                    Dim processId = TryCast(selectedProcess?.Tag, DataGatewayProcessStatusInformation).Id

                    gSv.SendCommandToDatapipelineProcess(processId, command)
                End While
            Catch ex As Exception
                UserMessage.Err(ex, ex.Message)
            End Try
        End Sub

        Public Sub RefreshView() Implements IRefreshable.RefreshView
            RefreshProcessStatusView()
        End Sub

        Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
            Return "data-gateways.htm"
        End Function
    End Class

End Namespace