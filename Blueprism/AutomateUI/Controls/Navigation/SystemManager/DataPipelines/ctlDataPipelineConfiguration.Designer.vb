<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlDataPipelineConfiguration
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlDataPipelineConfiguration))
        Dim DataGridViewCellStyle8 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle12 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle13 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle14 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle9 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle10 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle11 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.panPicture = New System.Windows.Forms.Panel()
        Me.btnAdd2 = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnCustom2 = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.DataGatawaysPicturePanel = New System.Windows.Forms.PictureBox()
        Me.panConfigs = New System.Windows.Forms.Panel()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.dgvPipelineOutputConfigs = New System.Windows.Forms.DataGridView()
        Me.id = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.SelectRow = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.outputName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.OutputType = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.datecreated = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Copy = New AutomateControls.DataGridViews.StyledDataGridViewButtonColumn()
        Me.Delete = New AutomateControls.DataGridViews.StyledDataGridViewButtonColumn()
        Me.Manage = New AutomateControls.DataGridViews.StyledDataGridViewButtonColumn()
        Me.CheckBoxSpacerPanel = New System.Windows.Forms.Panel()
        Me.SelectAllPanel = New System.Windows.Forms.Panel()
        Me.Line3D2 = New AutomateControls.Line3D()
        Me.SelectAll = New System.Windows.Forms.CheckBox()
        Me.ManageAndHeaderPanel = New System.Windows.Forms.Panel()
        Me.DeleteSelected = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.Line3D1 = New AutomateControls.Line3D()
        Me.lblDateCreated = New System.Windows.Forms.Label()
        Me.lblOutputType = New System.Windows.Forms.Label()
        Me.lblOutputName = New System.Windows.Forms.Label()
        Me.btnManageConfigs = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.panActions = New System.Windows.Forms.Panel()
        Me.btnAddConfig = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnCustomConfig = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.panPicture.SuspendLayout
        CType(Me.DataGatawaysPicturePanel,System.ComponentModel.ISupportInitialize).BeginInit
        Me.panConfigs.SuspendLayout
        Me.Panel1.SuspendLayout
        CType(Me.dgvPipelineOutputConfigs,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SelectAllPanel.SuspendLayout
        Me.ManageAndHeaderPanel.SuspendLayout
        Me.panActions.SuspendLayout
        Me.SuspendLayout
        '
        'panPicture
        '
        Me.panPicture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.panPicture.CausesValidation = false
        Me.panPicture.Controls.Add(Me.btnAdd2)
        Me.panPicture.Controls.Add(Me.btnCustom2)
        Me.panPicture.Controls.Add(Me.DataGatawaysPicturePanel)
        resources.ApplyResources(Me.panPicture, "panPicture")
        Me.panPicture.Name = "panPicture"
        '
        'btnAdd2
        '
        resources.ApplyResources(Me.btnAdd2, "btnAdd2")
        Me.btnAdd2.BackColor = System.Drawing.Color.White
        Me.btnAdd2.Name = "btnAdd2"
        Me.btnAdd2.Text = Global.AutomateUI.My.Resources.Resources.ctlDataPipelineConfiguration_btnAddNewConfig
        Me.btnAdd2.UseVisualStyleBackColor = false
        '
        'btnCustom2
        '
        resources.ApplyResources(Me.btnCustom2, "btnCustom2")
        Me.btnCustom2.BackColor = System.Drawing.Color.White
        Me.btnCustom2.Name = "btnCustom2"
        Me.btnCustom2.Text = Global.AutomateUI.My.Resources.Resources.ctlDataPipelineConfiguration_btnViewConfig
        Me.btnCustom2.UseVisualStyleBackColor = false
        '
        'DataGatawaysPicturePanel
        '
        resources.ApplyResources(Me.DataGatawaysPicturePanel, "DataGatawaysPicturePanel")
        Me.DataGatawaysPicturePanel.Image = Global.AutomateUI.My.Resources.ComponentImages.DataGatewayConfig
        Me.DataGatawaysPicturePanel.Name = "DataGatawaysPicturePanel"
        Me.DataGatawaysPicturePanel.TabStop = false
        '
        'panConfigs
        '
        Me.panConfigs.Controls.Add(Me.Panel1)
        Me.panConfigs.Controls.Add(Me.panActions)
        resources.ApplyResources(Me.panConfigs, "panConfigs")
        Me.panConfigs.Name = "panConfigs"
        '
        'Panel1
        '
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Controls.Add(Me.dgvPipelineOutputConfigs)
        Me.Panel1.Controls.Add(Me.CheckBoxSpacerPanel)
        Me.Panel1.Controls.Add(Me.SelectAllPanel)
        Me.Panel1.Controls.Add(Me.ManageAndHeaderPanel)
        Me.Panel1.Name = "Panel1"
        '
        'dgvPipelineOutputConfigs
        '
        Me.dgvPipelineOutputConfigs.AccessibleRole = System.Windows.Forms.AccessibleRole.None
        Me.dgvPipelineOutputConfigs.AllowUserToAddRows = false
        Me.dgvPipelineOutputConfigs.AllowUserToDeleteRows = false
        Me.dgvPipelineOutputConfigs.AllowUserToResizeColumns = false
        Me.dgvPipelineOutputConfigs.AllowUserToResizeRows = false
        Me.dgvPipelineOutputConfigs.BackgroundColor = System.Drawing.SystemColors.Window
        Me.dgvPipelineOutputConfigs.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.dgvPipelineOutputConfigs.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None
        Me.dgvPipelineOutputConfigs.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None
        DataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle8.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
        DataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle8.NullValue = " "
        DataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.dgvPipelineOutputConfigs.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle8
        Me.dgvPipelineOutputConfigs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.dgvPipelineOutputConfigs.ColumnHeadersVisible = false
        Me.dgvPipelineOutputConfigs.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.id, Me.SelectRow, Me.outputName, Me.OutputType, Me.datecreated, Me.Copy, Me.Delete, Me.Manage})
        DataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle12.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle12.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
        DataGridViewCellStyle12.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle12.Padding = New System.Windows.Forms.Padding(5)
        DataGridViewCellStyle12.SelectionBackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle12.SelectionForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dgvPipelineOutputConfigs.DefaultCellStyle = DataGridViewCellStyle12
        resources.ApplyResources(Me.dgvPipelineOutputConfigs, "dgvPipelineOutputConfigs")
        Me.dgvPipelineOutputConfigs.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically
        Me.dgvPipelineOutputConfigs.Name = "dgvPipelineOutputConfigs"
        Me.dgvPipelineOutputConfigs.ReadOnly = true
        DataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle13.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle13.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
        DataGridViewCellStyle13.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle13.Padding = New System.Windows.Forms.Padding(5)
        DataGridViewCellStyle13.SelectionBackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle13.SelectionForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.dgvPipelineOutputConfigs.RowHeadersDefaultCellStyle = DataGridViewCellStyle13
        DataGridViewCellStyle14.Padding = New System.Windows.Forms.Padding(5)
        Me.dgvPipelineOutputConfigs.RowsDefaultCellStyle = DataGridViewCellStyle14
        Me.dgvPipelineOutputConfigs.RowTemplate.DefaultCellStyle.Padding = New System.Windows.Forms.Padding(3)
        Me.dgvPipelineOutputConfigs.RowTemplate.Height = 25
        Me.dgvPipelineOutputConfigs.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dgvPipelineOutputConfigs.ShowCellErrors = false
        Me.dgvPipelineOutputConfigs.ShowCellToolTips = false
        Me.dgvPipelineOutputConfigs.ShowEditingIcon = false
        '
        'id
        '
        resources.ApplyResources(Me.id, "id")
        Me.id.Name = "id"
        Me.id.ReadOnly = true
        '
        'SelectRow
        '
        Me.SelectRow.FalseValue = "0"
        resources.ApplyResources(Me.SelectRow, "SelectRow")
        Me.SelectRow.IndeterminateValue = "0"
        Me.SelectRow.Name = "SelectRow"
        Me.SelectRow.ReadOnly = true
        Me.SelectRow.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.SelectRow.TrueValue = "1"
        '
        'outputName
        '
        Me.outputName.HeaderText = Global.AutomateUI.My.Resources.Resources.ctlDataPipelineConfiguration_ConfigNameCol
        Me.outputName.Name = "outputName"
        Me.outputName.ReadOnly = true
        resources.ApplyResources(Me.outputName, "outputName")
        '
        'OutputType
        '
        Me.OutputType.HeaderText = Global.AutomateUI.My.Resources.Resources.ctlDataPipelineConfiguration_OutputTypeCol
        Me.OutputType.Name = "OutputType"
        Me.OutputType.ReadOnly = true
        resources.ApplyResources(Me.OutputType, "OutputType")
        '
        'datecreated
        '
        Me.datecreated.HeaderText = Global.AutomateUI.My.Resources.Resources.ctlDataPipelineConfiguration_DateCreatedCol
        Me.datecreated.Name = "datecreated"
        Me.datecreated.ReadOnly = true
        resources.ApplyResources(Me.datecreated, "datecreated")
        '
        'Copy
        '
        DataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle9.BackColor = System.Drawing.Color.White
        DataGridViewCellStyle9.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
        Me.Copy.DefaultCellStyle = DataGridViewCellStyle9
        Me.Copy.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        resources.ApplyResources(Me.Copy, "Copy")
        Me.Copy.Name = "Copy"
        Me.Copy.ReadOnly = true
        Me.Copy.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.Copy.Text = Global.AutomateUI.My.Resources.Resources.ctlDataPipelineConfiguration_btnCopy
        Me.Copy.UseColumnTextForButtonValue = true
        '
        'Delete
        '
        DataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle10.BackColor = System.Drawing.Color.White
        DataGridViewCellStyle10.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
        Me.Delete.DefaultCellStyle = DataGridViewCellStyle10
        Me.Delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        resources.ApplyResources(Me.Delete, "Delete")
        Me.Delete.Name = "Delete"
        Me.Delete.ReadOnly = true
        Me.Delete.Text = Global.AutomateUI.My.Resources.Resources.ctlDataPipelineConfiguration_btnDelete
        Me.Delete.UseColumnTextForButtonValue = true
        '
        'Manage
        '
        DataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle11.BackColor = System.Drawing.Color.White
        DataGridViewCellStyle11.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
        Me.Manage.DefaultCellStyle = DataGridViewCellStyle11
        Me.Manage.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        resources.ApplyResources(Me.Manage, "Manage")
        Me.Manage.Name = "Manage"
        Me.Manage.ReadOnly = true
        Me.Manage.Text = Global.AutomateUI.My.Resources.Resources.ctlDataPipelineConfiguration_btnEdit
        Me.Manage.UseColumnTextForButtonValue = true
        '
        'CheckBoxSpacerPanel
        '
        resources.ApplyResources(Me.CheckBoxSpacerPanel, "CheckBoxSpacerPanel")
        Me.CheckBoxSpacerPanel.Name = "CheckBoxSpacerPanel"
        '
        'SelectAllPanel
        '
        Me.SelectAllPanel.Controls.Add(Me.Line3D2)
        Me.SelectAllPanel.Controls.Add(Me.SelectAll)
        resources.ApplyResources(Me.SelectAllPanel, "SelectAllPanel")
        Me.SelectAllPanel.Name = "SelectAllPanel"
        '
        'Line3D2
        '
        resources.ApplyResources(Me.Line3D2, "Line3D2")
        Me.Line3D2.Name = "Line3D2"
        '
        'SelectAll
        '
        resources.ApplyResources(Me.SelectAll, "SelectAll")
        Me.SelectAll.Name = "SelectAll"
        Me.SelectAll.Text = Global.AutomateUI.My.Resources.Resources.SelectAll
        Me.SelectAll.UseVisualStyleBackColor = true
        '
        'ManageAndHeaderPanel
        '
        Me.ManageAndHeaderPanel.Controls.Add(Me.DeleteSelected)
        Me.ManageAndHeaderPanel.Controls.Add(Me.Line3D1)
        Me.ManageAndHeaderPanel.Controls.Add(Me.lblDateCreated)
        Me.ManageAndHeaderPanel.Controls.Add(Me.lblOutputType)
        Me.ManageAndHeaderPanel.Controls.Add(Me.lblOutputName)
        Me.ManageAndHeaderPanel.Controls.Add(Me.btnManageConfigs)
        resources.ApplyResources(Me.ManageAndHeaderPanel, "ManageAndHeaderPanel")
        Me.ManageAndHeaderPanel.Name = "ManageAndHeaderPanel"
        '
        'DeleteSelected
        '
        Me.DeleteSelected.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.DeleteSelected, "DeleteSelected")
        Me.DeleteSelected.Name = "DeleteSelected"
        Me.DeleteSelected.UseVisualStyleBackColor = false
        '
        'Line3D1
        '
        resources.ApplyResources(Me.Line3D1, "Line3D1")
        Me.Line3D1.Name = "Line3D1"
        '
        'lblDateCreated
        '
        resources.ApplyResources(Me.lblDateCreated, "lblDateCreated")
        Me.lblDateCreated.Name = "lblDateCreated"
        '
        'lblOutputType
        '
        resources.ApplyResources(Me.lblOutputType, "lblOutputType")
        Me.lblOutputType.Name = "lblOutputType"
        '
        'lblOutputName
        '
        resources.ApplyResources(Me.lblOutputName, "lblOutputName")
        Me.lblOutputName.Name = "lblOutputName"
        '
        'btnManageConfigs
        '
        Me.btnManageConfigs.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.btnManageConfigs, "btnManageConfigs")
        Me.btnManageConfigs.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.btnManageConfigs.Name = "btnManageConfigs"
        Me.btnManageConfigs.Text = Global.AutomateUI.My.Resources.Resources.ctlDataPipelineConfiguration_Manage
        Me.btnManageConfigs.UseVisualStyleBackColor = false
        '
        'panActions
        '
        resources.ApplyResources(Me.panActions, "panActions")
        Me.panActions.Controls.Add(Me.btnAddConfig)
        Me.panActions.Controls.Add(Me.btnCustomConfig)
        Me.panActions.Name = "panActions"
        '
        'btnAddConfig
        '
        resources.ApplyResources(Me.btnAddConfig, "btnAddConfig")
        Me.btnAddConfig.BackColor = System.Drawing.Color.White
        Me.btnAddConfig.Name = "btnAddConfig"
        Me.btnAddConfig.Text = Global.AutomateUI.My.Resources.Resources.ctlDataPipelineConfiguration_btnAddNewConfig
        Me.btnAddConfig.UseVisualStyleBackColor = false
        '
        'btnCustomConfig
        '
        resources.ApplyResources(Me.btnCustomConfig, "btnCustomConfig")
        Me.btnCustomConfig.BackColor = System.Drawing.Color.White
        Me.btnCustomConfig.Name = "btnCustomConfig"
        Me.btnCustomConfig.Text = Global.AutomateUI.My.Resources.Resources.ctlDataPipelineConfiguration_btnViewConfig
        Me.btnCustomConfig.UseVisualStyleBackColor = false
        '
        'DataGridViewTextBoxColumn1
        '
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = true
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.HeaderText = Global.AutomateUI.My.Resources.Resources.ctlDataPipelineConfiguration_ConfigNameCol
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.ReadOnly = true
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        '
        'DataGridViewTextBoxColumn3
        '
        Me.DataGridViewTextBoxColumn3.HeaderText = Global.AutomateUI.My.Resources.Resources.ctlDataPipelineConfiguration_OutputTypeCol
        Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
        Me.DataGridViewTextBoxColumn3.ReadOnly = true
        resources.ApplyResources(Me.DataGridViewTextBoxColumn3, "DataGridViewTextBoxColumn3")
        '
        'DataGridViewTextBoxColumn4
        '
        Me.DataGridViewTextBoxColumn4.HeaderText = Global.AutomateUI.My.Resources.Resources.ctlDataPipelineConfiguration_DateCreatedCol
        Me.DataGridViewTextBoxColumn4.Name = "DataGridViewTextBoxColumn4"
        Me.DataGridViewTextBoxColumn4.ReadOnly = true
        resources.ApplyResources(Me.DataGridViewTextBoxColumn4, "DataGridViewTextBoxColumn4")
        '
        'ctlDataPipelineConfiguration
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.panConfigs)
        Me.Controls.Add(Me.panPicture)
        Me.Name = "ctlDataPipelineConfiguration"
        resources.ApplyResources(Me, "$this")
        Me.panPicture.ResumeLayout(false)
        CType(Me.DataGatawaysPicturePanel,System.ComponentModel.ISupportInitialize).EndInit
        Me.panConfigs.ResumeLayout(false)
        Me.Panel1.ResumeLayout(false)
        CType(Me.dgvPipelineOutputConfigs,System.ComponentModel.ISupportInitialize).EndInit
        Me.SelectAllPanel.ResumeLayout(false)
        Me.ManageAndHeaderPanel.ResumeLayout(false)
        Me.ManageAndHeaderPanel.PerformLayout
        Me.panActions.ResumeLayout(false)
        Me.ResumeLayout(false)

End Sub
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn3 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn4 As DataGridViewTextBoxColumn
    Friend WithEvents panPicture As Panel
    Friend WithEvents DataGatawaysPicturePanel As PictureBox
    Friend WithEvents btnAdd2 As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnCustom2 As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents panConfigs As Panel
    Friend WithEvents ManageAndHeaderPanel As Panel
    Friend WithEvents lblDateCreated As Label
    Friend WithEvents lblOutputType As Label
    Friend WithEvents lblOutputName As Label
    Friend WithEvents btnManageConfigs As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents panActions As Panel
    Friend WithEvents btnAddConfig As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnCustomConfig As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents dgvPipelineOutputConfigs As DataGridView
    Friend WithEvents Panel1 As Panel
    Friend WithEvents Line3D1 As AutomateControls.Line3D
    Friend WithEvents SelectAll As CheckBox
    Friend WithEvents DeleteSelected As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents SelectAllPanel As Panel
    Friend WithEvents id As DataGridViewTextBoxColumn
    Friend WithEvents SelectRow As DataGridViewCheckBoxColumn
    Friend WithEvents outputName As DataGridViewTextBoxColumn
    Friend WithEvents OutputType As DataGridViewTextBoxColumn
    Friend WithEvents datecreated As DataGridViewTextBoxColumn
    Friend WithEvents Copy As AutomateControls.DataGridViews.StyledDataGridViewButtonColumn
    Friend WithEvents Delete As AutomateControls.DataGridViews.StyledDataGridViewButtonColumn
    Friend WithEvents Manage As AutomateControls.DataGridViews.StyledDataGridViewButtonColumn
    Friend WithEvents Line3D2 As AutomateControls.Line3D
    Friend WithEvents CheckBoxSpacerPanel As Panel
End Class
