<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlWorkQueueSnapshots
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlWorkQueueSnapshots))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.SnapshotConfigurationDataGridView = New System.Windows.Forms.DataGridView()
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.AddConfigurationLinkLabel = New AutomateControls.BulletedLinkLabel()
        Me.EditConfigurationLinkLabel = New AutomateControls.BulletedLinkLabel()
        Me.DeleteConfigurationLinkLabel = New AutomateControls.BulletedLinkLabel()
        Me.TableLayoutPanel1.SuspendLayout
        CType(Me.SnapshotConfigurationDataGridView,System.ComponentModel.ISupportInitialize).BeginInit
        Me.FlowLayoutPanel1.SuspendLayout
        Me.SuspendLayout
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.SnapshotConfigurationDataGridView, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.FlowLayoutPanel1, 0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'SnapshotConfigurationDataGridView
        '
        Me.SnapshotConfigurationDataGridView.AllowUserToAddRows = false
        Me.SnapshotConfigurationDataGridView.AllowUserToDeleteRows = false
        Me.SnapshotConfigurationDataGridView.AllowUserToResizeRows = false
        DataGridViewCellStyle1.BackColor = System.Drawing.Color.Gainsboro
        Me.SnapshotConfigurationDataGridView.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle1
        Me.SnapshotConfigurationDataGridView.BackgroundColor = System.Drawing.SystemColors.Window
        Me.SnapshotConfigurationDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleVertical
        Me.SnapshotConfigurationDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.[Single]
        Me.SnapshotConfigurationDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        resources.ApplyResources(Me.SnapshotConfigurationDataGridView, "SnapshotConfigurationDataGridView")
        Me.SnapshotConfigurationDataGridView.MultiSelect = false
        Me.SnapshotConfigurationDataGridView.Name = "SnapshotConfigurationDataGridView"
        Me.SnapshotConfigurationDataGridView.ReadOnly = true
        Me.SnapshotConfigurationDataGridView.RowHeadersVisible = false
        Me.SnapshotConfigurationDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'FlowLayoutPanel1
        '
        resources.ApplyResources(Me.FlowLayoutPanel1, "FlowLayoutPanel1")
        Me.FlowLayoutPanel1.Controls.Add(Me.AddConfigurationLinkLabel)
        Me.FlowLayoutPanel1.Controls.Add(Me.EditConfigurationLinkLabel)
        Me.FlowLayoutPanel1.Controls.Add(Me.DeleteConfigurationLinkLabel)
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        '
        'AddConfigurationLinkLabel
        '
        resources.ApplyResources(Me.AddConfigurationLinkLabel, "AddConfigurationLinkLabel")
        Me.AddConfigurationLinkLabel.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64,Byte),Integer), CType(CType(64,Byte),Integer), CType(CType(64,Byte),Integer))
        Me.AddConfigurationLinkLabel.Name = "AddConfigurationLinkLabel"
        Me.AddConfigurationLinkLabel.TabStop = true
        Me.AddConfigurationLinkLabel.UseCompatibleTextRendering = true
        '
        'EditConfigurationLinkLabel
        '
        resources.ApplyResources(Me.EditConfigurationLinkLabel, "EditConfigurationLinkLabel")
        Me.EditConfigurationLinkLabel.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64,Byte),Integer), CType(CType(64,Byte),Integer), CType(CType(64,Byte),Integer))
        Me.EditConfigurationLinkLabel.Name = "EditConfigurationLinkLabel"
        Me.EditConfigurationLinkLabel.TabStop = true
        Me.EditConfigurationLinkLabel.UseCompatibleTextRendering = true
        '
        'DeleteConfigurationLinkLabel
        '
        resources.ApplyResources(Me.DeleteConfigurationLinkLabel, "DeleteConfigurationLinkLabel")
        Me.DeleteConfigurationLinkLabel.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64,Byte),Integer), CType(CType(64,Byte),Integer), CType(CType(64,Byte),Integer))
        Me.DeleteConfigurationLinkLabel.Name = "DeleteConfigurationLinkLabel"
        Me.DeleteConfigurationLinkLabel.TabStop = true
        Me.DeleteConfigurationLinkLabel.UseCompatibleTextRendering = true
        '
        'ctlWorkQueueSnapshots
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Name = "ctlWorkQueueSnapshots"
        resources.ApplyResources(Me, "$this")
        Me.TableLayoutPanel1.ResumeLayout(false)
        Me.TableLayoutPanel1.PerformLayout
        CType(Me.SnapshotConfigurationDataGridView,System.ComponentModel.ISupportInitialize).EndInit
        Me.FlowLayoutPanel1.ResumeLayout(false)
        Me.FlowLayoutPanel1.PerformLayout
        Me.ResumeLayout(false)

End Sub

    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents SnapshotConfigurationDataGridView As DataGridView
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
    Friend WithEvents AddConfigurationLinkLabel As AutomateControls.BulletedLinkLabel
    Friend WithEvents EditConfigurationLinkLabel As AutomateControls.BulletedLinkLabel
    Friend WithEvents DeleteConfigurationLinkLabel As AutomateControls.BulletedLinkLabel
End Class
