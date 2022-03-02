<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmDashboardTile
    Inherits AutomateControls.Forms.HelpButtonForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmDashboardTile))
        Me.objTitleBar = New AutomateControls.TitleBar()
        Me.txtTileName = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtDescription = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblType = New System.Windows.Forms.Label()
        Me.cmbTileType = New System.Windows.Forms.ComboBox()
        Me.lblRefresh = New System.Windows.Forms.Label()
        Me.cmbTileRefresh = New System.Windows.Forms.ComboBox()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.ehDashboard = New System.Windows.Forms.Integration.ElementHost()
        Me.tileView = New AutomateUI.ctlTileView()
        Me.tcTileDetails = New System.Windows.Forms.TabControl()
        Me.tpDetails = New System.Windows.Forms.TabPage()
        Me.panTile = New System.Windows.Forms.Panel()
        Me.tpPreview = New System.Windows.Forms.TabPage()
        Me.tcTileDetails.SuspendLayout()
        Me.tpDetails.SuspendLayout()
        Me.tpPreview.SuspendLayout()
        Me.SuspendLayout()
        '
        'objTitleBar
        '
        resources.ApplyResources(Me.objTitleBar, "objTitleBar")
        Me.objTitleBar.Name = "objTitleBar"
        Me.objTitleBar.SubtitleFont = New System.Drawing.Font("Segoe UI", 8.25!)
        Me.objTitleBar.SubtitlePosition = New System.Drawing.Point(10, 35)
        Me.objTitleBar.TabStop = False
        Me.objTitleBar.TitleFont = New System.Drawing.Font("Segoe UI", 8.25!)
        '
        'txtTileName
        '
        resources.ApplyResources(Me.txtTileName, "txtTileName")
        Me.txtTileName.Name = "txtTileName"
        '
        'txtDescription
        '
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        Me.txtDescription.Name = "txtDescription"
        '
        'lblType
        '
        resources.ApplyResources(Me.lblType, "lblType")
        Me.lblType.Name = "lblType"
        '
        'cmbTileType
        '
        Me.cmbTileType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbTileType.FormattingEnabled = True
        resources.ApplyResources(Me.cmbTileType, "cmbTileType")
        Me.cmbTileType.Name = "cmbTileType"
        Me.cmbTileType.Sorted = True
        '
        'lblRefresh
        '
        resources.ApplyResources(Me.lblRefresh, "lblRefresh")
        Me.lblRefresh.Name = "lblRefresh"
        '
        'cmbTileRefresh
        '
        resources.ApplyResources(Me.cmbTileRefresh, "cmbTileRefresh")
        Me.cmbTileRefresh.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbTileRefresh.FormattingEnabled = True
        Me.cmbTileRefresh.Name = "cmbTileRefresh"
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'ehDashboard
        '
        resources.ApplyResources(Me.ehDashboard, "ehDashboard")
        Me.ehDashboard.Name = "ehDashboard"
        Me.ehDashboard.Child = Me.tileView
        '
        'tcTileDetails
        '
        resources.ApplyResources(Me.tcTileDetails, "tcTileDetails")
        Me.tcTileDetails.Controls.Add(Me.tpDetails)
        Me.tcTileDetails.Controls.Add(Me.tpPreview)
        Me.tcTileDetails.Name = "tcTileDetails"
        Me.tcTileDetails.SelectedIndex = 0
        '
        'tpDetails
        '
        Me.tpDetails.Controls.Add(Me.panTile)
        resources.ApplyResources(Me.tpDetails, "tpDetails")
        Me.tpDetails.Name = "tpDetails"
        Me.tpDetails.UseVisualStyleBackColor = True
        '
        'panTile
        '
        resources.ApplyResources(Me.panTile, "panTile")
        Me.panTile.Name = "panTile"
        '
        'tpPreview
        '
        Me.tpPreview.Controls.Add(Me.ehDashboard)
        resources.ApplyResources(Me.tpPreview, "tpPreview")
        Me.tpPreview.Name = "tpPreview"
        Me.tpPreview.UseVisualStyleBackColor = True
        '
        'frmDashboardTile
        '
        Me.AcceptButton = Me.btnOK
        Me.CancelButton = Me.btnCancel
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.tcTileDetails)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.cmbTileRefresh)
        Me.Controls.Add(Me.lblRefresh)
        Me.Controls.Add(Me.cmbTileType)
        Me.Controls.Add(Me.lblType)
        Me.Controls.Add(Me.txtDescription)
        Me.Controls.Add(Me.txtTileName)
        Me.Controls.Add(Me.objTitleBar)
        Me.HelpButton = True
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmDashboardTile"
        Me.tcTileDetails.ResumeLayout(False)
        Me.tpDetails.ResumeLayout(False)
        Me.tpPreview.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents objTitleBar As AutomateControls.TitleBar
    Friend WithEvents txtTileName As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents txtDescription As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblType As System.Windows.Forms.Label
    Friend WithEvents cmbTileType As System.Windows.Forms.ComboBox
    Friend WithEvents lblRefresh As System.Windows.Forms.Label
    Friend WithEvents cmbTileRefresh As System.Windows.Forms.ComboBox
    Friend WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents ehDashboard As System.Windows.Forms.Integration.ElementHost
    Friend WithEvents tcTileDetails As System.Windows.Forms.TabControl
    Friend WithEvents tpDetails As System.Windows.Forms.TabPage
    Friend WithEvents tpPreview As System.Windows.Forms.TabPage
    Friend WithEvents panTile As System.Windows.Forms.Panel
    Friend tileView As AutomateUI.ctlTileView

End Class
