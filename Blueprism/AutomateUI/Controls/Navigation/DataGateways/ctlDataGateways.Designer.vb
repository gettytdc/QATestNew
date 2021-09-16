Namespace Controls.Navigation.DataGateways
    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class ctlDataGateways
        Inherits System.Windows.Forms.UserControl

        'UserControl overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()>
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Dim TreeListViewItemCollectionComparer1 As AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer = New AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer()
            Me.MainTableLayoutPanel = New System.Windows.Forms.TableLayoutPanel()
            Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
            Me.Label1 = New System.Windows.Forms.Label()
            Me.Label2 = New System.Windows.Forms.Label()
            Me.ProcessTreeListView = New AutomateControls.TreeList.TreeListView()
            Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.mDataGatewayProcessesContextMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
            Me.btnStartDataGatewayProcess = New System.Windows.Forms.ToolStripMenuItem()
            Me.btnStopDataGatewayProcess = New System.Windows.Forms.ToolStripMenuItem()
            Me.RefreshTimer = New System.Windows.Forms.Timer(Me.components)
            Me.MainTableLayoutPanel.SuspendLayout()
            Me.FlowLayoutPanel1.SuspendLayout()
            Me.mDataGatewayProcessesContextMenu.SuspendLayout()
            Me.SuspendLayout()
            '
            'MainTableLayoutPanel
            '
            Me.MainTableLayoutPanel.ColumnCount = 1
            Me.MainTableLayoutPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.MainTableLayoutPanel.Controls.Add(Me.FlowLayoutPanel1, 0, 0)
            Me.MainTableLayoutPanel.Controls.Add(Me.ProcessTreeListView, 0, 1)
            Me.MainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill
            Me.MainTableLayoutPanel.Location = New System.Drawing.Point(0, 0)
            Me.MainTableLayoutPanel.Name = "MainTableLayoutPanel"
            Me.MainTableLayoutPanel.RowCount = 2
            Me.MainTableLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
            Me.MainTableLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.MainTableLayoutPanel.Size = New System.Drawing.Size(752, 652)
            Me.MainTableLayoutPanel.TabIndex = 0
            '
            'FlowLayoutPanel1
            '
            Me.FlowLayoutPanel1.Controls.Add(Me.Label1)
            Me.FlowLayoutPanel1.Controls.Add(Me.Label2)
            Me.FlowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
            Me.FlowLayoutPanel1.Location = New System.Drawing.Point(3, 3)
            Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
            Me.FlowLayoutPanel1.Size = New System.Drawing.Size(746, 14)
            Me.FlowLayoutPanel1.TabIndex = 0
            Me.FlowLayoutPanel1.WrapContents = False
            '
            'Label1
            '
            Me.Label1.AutoSize = True
            Me.Label1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold)
            Me.Label1.Location = New System.Drawing.Point(3, 0)
            Me.Label1.Name = "Label1"
            Me.Label1.Size = New System.Drawing.Size(132, 13)
            Me.Label1.TabIndex = 1
            Me.Label1.Text = Global.AutomateUI.My.Resources.Resources.ctlDataGateways_DataGatewayProcesses
            '
            'Label2
            '
            Me.Label2.AutoSize = True
            Me.Label2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label2.Location = New System.Drawing.Point(141, 0)
            Me.Label2.Name = "Label2"
            Me.Label2.Size = New System.Drawing.Size(396, 13)
            Me.Label2.TabIndex = 2
            Me.Label2.Text = Global.AutomateUI.My.Resources.Resources.ctlDataGateways_DataGatewayProcessesDescription
            '
            'ProcessTreeListView
            '
            Me.ProcessTreeListView.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2, Me.ColumnHeader3})
            TreeListViewItemCollectionComparer1.Column = 0
            TreeListViewItemCollectionComparer1.SortOrder = System.Windows.Forms.SortOrder.Ascending
            Me.ProcessTreeListView.Comparer = TreeListViewItemCollectionComparer1
            Me.ProcessTreeListView.ContextMenuStrip = Me.mDataGatewayProcessesContextMenu
            Me.ProcessTreeListView.Dock = System.Windows.Forms.DockStyle.Fill
            Me.ProcessTreeListView.FocusedItem = Nothing
            Me.ProcessTreeListView.Location = New System.Drawing.Point(3, 23)
            Me.ProcessTreeListView.Name = "ProcessTreeListView"
            Me.ProcessTreeListView.Size = New System.Drawing.Size(746, 626)
            Me.ProcessTreeListView.TabIndex = 1
            Me.ProcessTreeListView.UseCompatibleStateImageBehavior = False
            '
            'ColumnHeader1
            '
            Me.ColumnHeader1.Text = Global.AutomateUI.My.Resources.Resources.ctlDataGateways_ProcessGrid_Name
            Me.ColumnHeader1.Width = 100
            '
            'ColumnHeader2
            '
            Me.ColumnHeader2.Text = Global.AutomateUI.My.Resources.Resources.ctlDataGateways_ProcessGrid_Status
            Me.ColumnHeader2.Width = 100
            '
            'ColumnHeader3
            '
            Me.ColumnHeader3.Text = Global.AutomateUI.My.Resources.Resources.ctlDataGateways_ProcessGrid_Details
            Me.ColumnHeader3.Width = 100
            '
            'mDataGatewayProcessesContextMenu
            '
            Me.mDataGatewayProcessesContextMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.btnStartDataGatewayProcess, Me.btnStopDataGatewayProcess})
            Me.mDataGatewayProcessesContextMenu.Name = "mDataGatewayProcessesContextMenu"
            Me.mDataGatewayProcessesContextMenu.Size = New System.Drawing.Size(99, 48)
            '
            'btnStartDataGatewayProcess
            '
            Me.btnStartDataGatewayProcess.Name = "btnStartDataGatewayProcess"
            Me.btnStartDataGatewayProcess.Size = New System.Drawing.Size(98, 22)
            Me.btnStartDataGatewayProcess.Text = "Start"
            '
            'btnStopDataGatewayProcess
            '
            Me.btnStopDataGatewayProcess.Name = "btnStopDataGatewayProcess"
            Me.btnStopDataGatewayProcess.Size = New System.Drawing.Size(98, 22)
            Me.btnStopDataGatewayProcess.Text = "Stop"
            '
            'RefreshTimer
            '
            Me.RefreshTimer.Interval = 10000
            '
            'ctlDataGateways
            '
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
            Me.Controls.Add(Me.MainTableLayoutPanel)
            Me.Name = "ctlDataGateways"
            Me.Size = New System.Drawing.Size(752, 652)
            Me.MainTableLayoutPanel.ResumeLayout(False)
            Me.FlowLayoutPanel1.ResumeLayout(False)
            Me.FlowLayoutPanel1.PerformLayout()
            Me.mDataGatewayProcessesContextMenu.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub

        Friend WithEvents MainTableLayoutPanel As TableLayoutPanel
        Friend WithEvents RefreshTimer As Timer
        Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
        Friend WithEvents Label1 As Label
        Friend WithEvents Label2 As Label
        Friend WithEvents ProcessTreeListView As AutomateControls.TreeList.TreeListView
        Friend WithEvents ColumnHeader1 As ColumnHeader
        Friend WithEvents ColumnHeader2 As ColumnHeader
        Friend WithEvents ColumnHeader3 As ColumnHeader
        Friend WithEvents mDataGatewayProcessesContextMenu As ContextMenuStrip
        Friend WithEvents btnStartDataGatewayProcess As ToolStripMenuItem
        Friend WithEvents btnStopDataGatewayProcess As ToolStripMenuItem
    End Class
End Namespace