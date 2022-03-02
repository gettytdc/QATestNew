<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlWorkQueueContents
    Inherits System.Windows.Forms.UserControl

    

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlWorkQueueContents))
        Me.lblQueueContents = New System.Windows.Forms.Label()
        Me.pnlListviewContainer = New AutomateControls.MonoPanel()
        Me.lstQueueContents = New AutomateControls.FlickerFreeListView()
        Me.mTooltip = New System.Windows.Forms.ToolTip(Me.components)
        Me.llShowPosition = New System.Windows.Forms.LinkLabel()
        Me.btnSaveFilter = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnDeleteFilter = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnSetDefault = New AutomateControls.Buttons.StandardStyledButton()
        Me.lnkClearFilters = New System.Windows.Forms.LinkLabel()
        Me.cmbFilterSwitcher = New System.Windows.Forms.ComboBox()
        Me.bwGetContents = New System.ComponentModel.BackgroundWorker()
        Me.bwGetPositions = New System.ComponentModel.BackgroundWorker()
        Me.mRowsPerPage = New AutomateUI.ctlRowsPerPage()
        Me.pnlListviewContainer.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblQueueContents
        '
        resources.ApplyResources(Me.lblQueueContents, "lblQueueContents")
        Me.lblQueueContents.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.lblQueueContents.ForeColor = System.Drawing.Color.Black
        Me.lblQueueContents.Name = "lblQueueContents"
        '
        'pnlListviewContainer
        '
        resources.ApplyResources(Me.pnlListviewContainer, "pnlListviewContainer")
        Me.pnlListviewContainer.Controls.Add(Me.lstQueueContents)
        Me.pnlListviewContainer.DockPadding.All = 0
        Me.pnlListviewContainer.DockPadding.Bottom = 0
        Me.pnlListviewContainer.DockPadding.Left = 0
        Me.pnlListviewContainer.DockPadding.Right = 0
        Me.pnlListviewContainer.DockPadding.Top = 0
        Me.pnlListviewContainer.IsDoubleBuffered = True
        Me.pnlListviewContainer.MouseScrollIncrement = 24
        Me.pnlListviewContainer.Name = "pnlListviewContainer"
        '
        'lstQueueContents
        '
        Me.lstQueueContents.FullRowSelect = True
        Me.lstQueueContents.GridLines = True
        Me.lstQueueContents.HideSelection = False
        resources.ApplyResources(Me.lstQueueContents, "lstQueueContents")
        Me.lstQueueContents.Name = "lstQueueContents"
        Me.lstQueueContents.UseCompatibleStateImageBehavior = False
        Me.lstQueueContents.View = System.Windows.Forms.View.Details
        '
        'llShowPosition
        '
        Me.llShowPosition.BackColor = System.Drawing.Color.Transparent
        resources.ApplyResources(Me.llShowPosition, "llShowPosition")
        Me.llShowPosition.Name = "llShowPosition"
        Me.llShowPosition.TabStop = True
        Me.mTooltip.SetToolTip(Me.llShowPosition, resources.GetString("llShowPosition.ToolTip"))
        '
        'btnSaveFilter
        '
        resources.ApplyResources(Me.btnSaveFilter, "btnSaveFilter")
        Me.btnSaveFilter.Image = Global.AutomateUI.My.Resources.ToolImages.Save_16x16
        Me.btnSaveFilter.Name = "btnSaveFilter"
        Me.mTooltip.SetToolTip(Me.btnSaveFilter, resources.GetString("btnSaveFilter.ToolTip"))
        Me.btnSaveFilter.UseVisualStyleBackColor = True
        '
        'btnDeleteFilter
        '
        resources.ApplyResources(Me.btnDeleteFilter, "btnDeleteFilter")
        Me.btnDeleteFilter.Image = Global.AutomateUI.My.Resources.ToolImages.Delete_Red_16x16
        Me.btnDeleteFilter.Name = "btnDeleteFilter"
        Me.mTooltip.SetToolTip(Me.btnDeleteFilter, resources.GetString("btnDeleteFilter.ToolTip"))
        Me.btnDeleteFilter.UseVisualStyleBackColor = True
        '
        'btnSetDefault
        '
        resources.ApplyResources(Me.btnSetDefault, "btnSetDefault")
        Me.btnSetDefault.Image = Global.AutomateUI.My.Resources.ToolImages.Wizard_16x16
        Me.btnSetDefault.Name = "btnSetDefault"
        Me.mTooltip.SetToolTip(Me.btnSetDefault, resources.GetString("btnSetDefault.ToolTip"))
        Me.btnSetDefault.UseVisualStyleBackColor = True
        '
        'lnkClearFilters
        '
        Me.lnkClearFilters.BackColor = System.Drawing.Color.Transparent
        resources.ApplyResources(Me.lnkClearFilters, "lnkClearFilters")
        Me.lnkClearFilters.Name = "lnkClearFilters"
        Me.lnkClearFilters.TabStop = True
        '
        'cmbFilterSwitcher
        '
        resources.ApplyResources(Me.cmbFilterSwitcher, "cmbFilterSwitcher")
        Me.cmbFilterSwitcher.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbFilterSwitcher.FormattingEnabled = True
        Me.cmbFilterSwitcher.Name = "cmbFilterSwitcher"
        '
        'bwGetContents
        '
        Me.bwGetContents.WorkerReportsProgress = True
        Me.bwGetContents.WorkerSupportsCancellation = True
        '
        'bwGetPositions
        '
        Me.bwGetPositions.WorkerReportsProgress = True
        Me.bwGetPositions.WorkerSupportsCancellation = True
        '
        'mRowsPerPage
        '
        Me.mRowsPerPage.CurrentPage = 1
        resources.ApplyResources(Me.mRowsPerPage, "mRowsPerPage")
        Me.mRowsPerPage.MaxRows = 0
        Me.mRowsPerPage.Name = "mRowsPerPage"
        Me.mRowsPerPage.RowsPerPage = 0
        Me.mRowsPerPage.TotalRows = 0
        '
        'ctlWorkQueueContents
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.Controls.Add(Me.btnSetDefault)
        Me.Controls.Add(Me.btnDeleteFilter)
        Me.Controls.Add(Me.btnSaveFilter)
        Me.Controls.Add(Me.cmbFilterSwitcher)
        Me.Controls.Add(Me.pnlListviewContainer)
        Me.Controls.Add(Me.mRowsPerPage)
        Me.Controls.Add(Me.lblQueueContents)
        Me.Controls.Add(Me.llShowPosition)
        Me.Controls.Add(Me.lnkClearFilters)
        Me.Name = "ctlWorkQueueContents"
        resources.ApplyResources(Me, "$this")
        Me.pnlListviewContainer.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblQueueContents As System.Windows.Forms.Label
    Friend WithEvents mRowsPerPage As ctlRowsPerPage
    Friend WithEvents pnlListviewContainer As AutomateControls.MonoPanel
    Friend WithEvents lstQueueContents As AutomateControls.FlickerFreeListView
    Friend WithEvents mTooltip As System.Windows.Forms.ToolTip
    Private WithEvents lnkClearFilters As System.Windows.Forms.LinkLabel
    Private WithEvents llShowPosition As System.Windows.Forms.LinkLabel
    Friend WithEvents cmbFilterSwitcher As System.Windows.Forms.ComboBox
    Friend WithEvents btnSaveFilter As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnDeleteFilter As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnSetDefault As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents bwGetContents As System.ComponentModel.BackgroundWorker
    Private WithEvents bwGetPositions As System.ComponentModel.BackgroundWorker

End Class
