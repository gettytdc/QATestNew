<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlAuditLogs
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlAuditLogs))
        Me.gridAudit = New System.Windows.Forms.DataGridView()
        Me.ctxMenuAudit = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ViewSelectedLogsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SearchPanel = New System.Windows.Forms.Panel()
        Me.ResetButton = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.SearchButton = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.lblDateTo = New System.Windows.Forms.Label()
        Me.lblDateFrom = New System.Windows.Forms.Label()
        Me.dpTo = New CustomControls.DatePicker()
        Me.dpFrom = New CustomControls.DatePicker()
        Me.lblSearch = New System.Windows.Forms.Label()
        Me.txtSearch = New AutomateControls.Textboxes.StyledTextBox()
        Me.ShowHideButton = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.cboSort = New System.Windows.Forms.ComboBox()
        Me.lblSearchCopy = New System.Windows.Forms.Label()
        CType(Me.gridAudit,System.ComponentModel.ISupportInitialize).BeginInit
        Me.ctxMenuAudit.SuspendLayout
        Me.SearchPanel.SuspendLayout
        Me.SuspendLayout
        '
        'gridAudit
        '
        Me.gridAudit.AllowUserToAddRows = false
        Me.gridAudit.AllowUserToDeleteRows = false
        Me.gridAudit.AllowUserToResizeRows = false
        resources.ApplyResources(Me.gridAudit, "gridAudit")
        Me.gridAudit.BackgroundColor = System.Drawing.SystemColors.Window
        Me.gridAudit.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.gridAudit.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.[Single]
        Me.gridAudit.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridAudit.GridColor = System.Drawing.SystemColors.ControlLight
        Me.gridAudit.Name = "gridAudit"
        Me.gridAudit.ReadOnly = true
        Me.gridAudit.RowHeadersVisible = false
        Me.gridAudit.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'ctxMenuAudit
        '
        Me.ctxMenuAudit.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.ctxMenuAudit.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ViewSelectedLogsToolStripMenuItem})
        Me.ctxMenuAudit.Name = "ctxMenuAudit"
        resources.ApplyResources(Me.ctxMenuAudit, "ctxMenuAudit")
        '
        'ViewSelectedLogsToolStripMenuItem
        '
        Me.ViewSelectedLogsToolStripMenuItem.Name = "ViewSelectedLogsToolStripMenuItem"
        resources.ApplyResources(Me.ViewSelectedLogsToolStripMenuItem, "ViewSelectedLogsToolStripMenuItem")
        '
        'SearchPanel
        '
        resources.ApplyResources(Me.SearchPanel, "SearchPanel")
        Me.SearchPanel.Controls.Add(Me.ResetButton)
        Me.SearchPanel.Controls.Add(Me.SearchButton)
        Me.SearchPanel.Controls.Add(Me.lblDateTo)
        Me.SearchPanel.Controls.Add(Me.lblDateFrom)
        Me.SearchPanel.Controls.Add(Me.dpTo)
        Me.SearchPanel.Controls.Add(Me.dpFrom)
        Me.SearchPanel.Controls.Add(Me.lblSearch)
        Me.SearchPanel.Controls.Add(Me.txtSearch)
        Me.SearchPanel.Name = "SearchPanel"
        '
        'ResetButton
        '
        Me.ResetButton.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.ResetButton, "ResetButton")
        Me.ResetButton.Name = "ResetButton"
        Me.ResetButton.UseVisualStyleBackColor = false
        '
        'SearchButton
        '
        Me.SearchButton.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.SearchButton, "SearchButton")
        Me.SearchButton.Name = "SearchButton"
        Me.SearchButton.UseVisualStyleBackColor = false
        '
        'lblDateTo
        '
        resources.ApplyResources(Me.lblDateTo, "lblDateTo")
        Me.lblDateTo.Name = "lblDateTo"
        '
        'lblDateFrom
        '
        resources.ApplyResources(Me.lblDateFrom, "lblDateFrom")
        Me.lblDateFrom.Name = "lblDateFrom"
        '
        'dpTo
        '
        resources.ApplyResources(Me.dpTo, "dpTo")
        Me.dpTo.Name = "dpTo"
        Me.dpTo.PickerDayFont = New System.Drawing.Font("Tahoma", 8!)
        '
        'dpFrom
        '
        resources.ApplyResources(Me.dpFrom, "dpFrom")
        Me.dpFrom.Name = "dpFrom"
        Me.dpFrom.PickerDayFont = New System.Drawing.Font("Tahoma", 8!)
        '
        'lblSearch
        '
        resources.ApplyResources(Me.lblSearch, "lblSearch")
        Me.lblSearch.Name = "lblSearch"
        '
        'txtSearch
        '
        Me.txtSearch.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.txtSearch, "txtSearch")
        Me.txtSearch.Name = "txtSearch"
        '
        'ShowHideButton
        '
        resources.ApplyResources(Me.ShowHideButton, "ShowHideButton")
        Me.ShowHideButton.BackColor = System.Drawing.Color.White
        Me.ShowHideButton.Name = "ShowHideButton"
        Me.ShowHideButton.UseVisualStyleBackColor = false
        '
        'cboSort
        '
        resources.ApplyResources(Me.cboSort, "cboSort")
        Me.cboSort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboSort.ForeColor = System.Drawing.SystemColors.WindowText
        Me.cboSort.FormattingEnabled = true
        Me.cboSort.Items.AddRange(New Object() {resources.GetString("cboSort.Items"), resources.GetString("cboSort.Items1")})
        Me.cboSort.Name = "cboSort"
        '
        'lblSearchCopy
        '
        Me.lblSearchCopy.AutoEllipsis = true
        resources.ApplyResources(Me.lblSearchCopy, "lblSearchCopy")
        Me.lblSearchCopy.Name = "lblSearchCopy"
        '
        'ctlAuditLogs
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.cboSort)
        Me.Controls.Add(Me.ShowHideButton)
        Me.Controls.Add(Me.lblSearchCopy)
        Me.Controls.Add(Me.SearchPanel)
        Me.Controls.Add(Me.gridAudit)
        Me.Name = "ctlAuditLogs"
        resources.ApplyResources(Me, "$this")
        CType(Me.gridAudit,System.ComponentModel.ISupportInitialize).EndInit
        Me.ctxMenuAudit.ResumeLayout(false)
        Me.SearchPanel.ResumeLayout(false)
        Me.SearchPanel.PerformLayout
        Me.ResumeLayout(false)

End Sub
    Friend WithEvents gridAudit As System.Windows.Forms.DataGridView
    Private WithEvents ctxMenuAudit As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents ViewSelectedLogsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SearchPanel As Panel
    Friend WithEvents lblDateTo As Label
    Friend WithEvents lblDateFrom As Label
    Friend WithEvents dpTo As CustomControls.DatePicker
    Friend WithEvents dpFrom As CustomControls.DatePicker
    Friend WithEvents lblSearch As Label
    Friend WithEvents txtSearch As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents ResetButton As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents SearchButton As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents ShowHideButton As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents cboSort As ComboBox
    Friend WithEvents lblSearchCopy As Label
End Class
