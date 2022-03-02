<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ConnectionManagerPanel
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
        Dim Label1 As System.Windows.Forms.Label
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ConnectionManagerPanel))
        Dim Label2 As System.Windows.Forms.Label
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.lvConnections = New System.Windows.Forms.ListView()
        Me.colName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.connDetailPanel = New BluePrism.Config.ConnectionDetail()
        Me.btnNew = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnDelete = New AutomateControls.Buttons.StandardStyledButton()
        Me.CreateDatabaseButton = New AutomateControls.Buttons.StandardStyledButton()
        Me.ConfigureDatabaseButton = New AutomateControls.Buttons.StandardStyledButton()
        Me.UpgradeDatabaseButton = New AutomateControls.Buttons.StandardStyledButton()
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.FlowLayoutPanel2 = New System.Windows.Forms.FlowLayoutPanel()
        Label1 = New System.Windows.Forms.Label()
        Label2 = New System.Windows.Forms.Label()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.FlowLayoutPanel1.SuspendLayout()
        Me.FlowLayoutPanel2.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        resources.ApplyResources(Label1, "Label1")
        Label1.Name = "Label1"
        '
        'Label2
        '
        resources.ApplyResources(Label2, "Label2")
        Label2.Name = "Label2"
        '
        'SplitContainer1
        '
        resources.ApplyResources(Me.SplitContainer1, "SplitContainer1")
        Me.SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Label2)
        Me.SplitContainer1.Panel1.Controls.Add(Me.lvConnections)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.connDetailPanel)
        Me.SplitContainer1.Panel2.Controls.Add(Label1)
        '
        'lvConnections
        '
        resources.ApplyResources(Me.lvConnections, "lvConnections")
        Me.lvConnections.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colName})
        Me.lvConnections.FullRowSelect = True
        Me.lvConnections.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None
        Me.lvConnections.HideSelection = False
        Me.lvConnections.MultiSelect = False
        Me.lvConnections.Name = "lvConnections"
        Me.lvConnections.UseCompatibleStateImageBehavior = False
        Me.lvConnections.View = System.Windows.Forms.View.Details
        '
        'colName
        '
        resources.ApplyResources(Me.colName, "colName")
        '
        'connDetailPanel
        '
        resources.ApplyResources(Me.connDetailPanel, "connDetailPanel")
        Me.connDetailPanel.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.connDetailPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.connDetailPanel.ConnectionSetting = Nothing
        Me.connDetailPanel.Name = "connDetailPanel"
        '
        'btnNew
        '
        resources.ApplyResources(Me.btnNew, "btnNew")
        Me.btnNew.Name = "btnNew"
        Me.btnNew.UseVisualStyleBackColor = True
        '
        'btnDelete
        '
        resources.ApplyResources(Me.btnDelete, "btnDelete")
        Me.btnDelete.Name = "btnDelete"
        Me.btnDelete.UseVisualStyleBackColor = True
        '
        'CreateDatabaseButton
        '
        resources.ApplyResources(Me.CreateDatabaseButton, "CreateDatabaseButton")
        Me.CreateDatabaseButton.Name = "CreateDatabaseButton"
        Me.CreateDatabaseButton.UseVisualStyleBackColor = True
        '
        'ConfigureDatabaseButton
        '
        resources.ApplyResources(Me.ConfigureDatabaseButton, "ConfigureDatabaseButton")
        Me.ConfigureDatabaseButton.Name = "ConfigureDatabaseButton"
        Me.ConfigureDatabaseButton.UseVisualStyleBackColor = True
        '
        'UpgradeDatabaseButton
        '
        resources.ApplyResources(Me.UpgradeDatabaseButton, "UpgradeDatabaseButton")
        Me.UpgradeDatabaseButton.Name = "UpgradeDatabaseButton"
        Me.UpgradeDatabaseButton.UseVisualStyleBackColor = True
        '
        'FlowLayoutPanel1
        '
        resources.ApplyResources(Me.FlowLayoutPanel1, "FlowLayoutPanel1")
        Me.FlowLayoutPanel1.Controls.Add(Me.ConfigureDatabaseButton)
        Me.FlowLayoutPanel1.Controls.Add(Me.UpgradeDatabaseButton)
        Me.FlowLayoutPanel1.Controls.Add(Me.CreateDatabaseButton)
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        '
        'FlowLayoutPanel2
        '
        resources.ApplyResources(Me.FlowLayoutPanel2, "FlowLayoutPanel2")
        Me.FlowLayoutPanel2.Controls.Add(Me.btnNew)
        Me.FlowLayoutPanel2.Controls.Add(Me.btnDelete)
        Me.FlowLayoutPanel2.Name = "FlowLayoutPanel2"
        '
        'ConnectionManagerPanel
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.FlowLayoutPanel2)
        Me.Controls.Add(Me.FlowLayoutPanel1)
        Me.Controls.Add(Me.SplitContainer1)
        resources.ApplyResources(Me, "$this")
        Me.Name = "ConnectionManagerPanel"
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel1.PerformLayout()
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        Me.SplitContainer1.Panel2.PerformLayout()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.FlowLayoutPanel1.ResumeLayout(False)
        Me.FlowLayoutPanel1.PerformLayout()
        Me.FlowLayoutPanel2.ResumeLayout(False)
        Me.FlowLayoutPanel2.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Private WithEvents btnNew As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents btnDelete As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents lvConnections As System.Windows.Forms.ListView
    Private WithEvents connDetailPanel As BluePrism.Config.ConnectionDetail
    Private WithEvents colName As System.Windows.Forms.ColumnHeader
    Private WithEvents CreateDatabaseButton As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents ConfigureDatabaseButton As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents UpgradeDatabaseButton As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
    Friend WithEvents FlowLayoutPanel2 As FlowLayoutPanel
End Class
