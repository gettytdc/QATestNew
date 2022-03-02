<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ADGroupSelectorForm
    Inherits AutomateControls.Forms.AutomateForm

    'Form overrides dispose to clean up the component list.
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ADGroupSelectorForm))
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.txtDomain = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnFind = New AutomateControls.Buttons.StandardStyledButton()
        Me.dgvGroup = New AutomateControls.DataGridViews.RowBasedDataGridView()
        Me.colName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colType = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colPath = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.txtFilter = New AutomateControls.Textboxes.StyledTextBox()
        Me.tlp = New System.Windows.Forms.TableLayoutPanel()
        Me.panel1 = New System.Windows.Forms.Panel()
        Me.txtLocation = New AutomateControls.GuidanceTextBox()
        Me.lblLocation = New System.Windows.Forms.Label()
        Me.btnFilterTypeTooltip = New System.Windows.Forms.PictureBox()
        Me.cboFilterType = New System.Windows.Forms.ComboBox()
        Me.lblFilter = New System.Windows.Forms.Label()
        Me.label1 = New System.Windows.Forms.Label()
        Me.mToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.tBar = New AutomateControls.TitleBar()
        CType(Me.dgvGroup, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tlp.SuspendLayout()
        Me.panel1.SuspendLayout()
        CType(Me.btnFilterTypeTooltip, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        '
        'txtDomain
        '
        resources.ApplyResources(Me.txtDomain, "txtDomain")
        Me.txtDomain.Name = "txtDomain"
        Me.txtDomain.ReadOnly = True
        '
        'btnFind
        '
        resources.ApplyResources(Me.btnFind, "btnFind")
        Me.btnFind.Name = "btnFind"
        Me.btnFind.UseVisualStyleBackColor = True
        '
        'dgvGroup
        '
        Me.dgvGroup.AllowUserToOrderColumns = True
        resources.ApplyResources(Me.dgvGroup, "dgvGroup")
        Me.dgvGroup.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvGroup.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colName, Me.colType, Me.colPath})
        Me.dgvGroup.MultiSelect = False
        Me.dgvGroup.Name = "dgvGroup"
        Me.dgvGroup.ReadOnly = True
        '
        'colName
        '
        resources.ApplyResources(Me.colName, "colName")
        Me.colName.Name = "colName"
        Me.colName.ReadOnly = True
        '
        'colType
        '
        resources.ApplyResources(Me.colType, "colType")
        Me.colType.Name = "colType"
        Me.colType.ReadOnly = True
        '
        'colPath
        '
        resources.ApplyResources(Me.colPath, "colPath")
        Me.colPath.Name = "colPath"
        Me.colPath.ReadOnly = True
        '
        'txtFilter
        '
        resources.ApplyResources(Me.txtFilter, "txtFilter")
        Me.txtFilter.Name = "txtFilter"
        '
        'tlp
        '
        resources.ApplyResources(Me.tlp, "tlp")
        Me.tlp.Controls.Add(Me.panel1, 0, 0)
        Me.tlp.Name = "tlp"
        '
        'panel1
        '
        Me.panel1.Controls.Add(Me.txtLocation)
        Me.panel1.Controls.Add(Me.lblLocation)
        Me.panel1.Controls.Add(Me.btnFilterTypeTooltip)
        Me.panel1.Controls.Add(Me.dgvGroup)
        Me.panel1.Controls.Add(Me.cboFilterType)
        Me.panel1.Controls.Add(Me.lblFilter)
        Me.panel1.Controls.Add(Me.label1)
        Me.panel1.Controls.Add(Me.txtDomain)
        Me.panel1.Controls.Add(Me.btnFind)
        Me.panel1.Controls.Add(Me.txtFilter)
        resources.ApplyResources(Me.panel1, "panel1")
        Me.panel1.Name = "panel1"
        '
        'txtLocation
        '
        resources.ApplyResources(Me.txtLocation, "txtLocation")
        Me.txtLocation.Name = "txtLocation"
        '
        'lblLocation
        '
        resources.ApplyResources(Me.lblLocation, "lblLocation")
        Me.lblLocation.Name = "lblLocation"
        '
        'btnFilterTypeTooltip
        '
        resources.ApplyResources(Me.btnFilterTypeTooltip, "btnFilterTypeTooltip")
        Me.btnFilterTypeTooltip.Name = "btnFilterTypeTooltip"
        Me.btnFilterTypeTooltip.TabStop = False
        '
        'cboFilterType
        '
        Me.cboFilterType.DisplayMember = "Text"
        Me.cboFilterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboFilterType.FormattingEnabled = True
        resources.ApplyResources(Me.cboFilterType, "cboFilterType")
        Me.cboFilterType.Name = "cboFilterType"
        Me.cboFilterType.ValueMember = "Tag"
        '
        'lblFilter
        '
        resources.ApplyResources(Me.lblFilter, "lblFilter")
        Me.lblFilter.Name = "lblFilter"
        '
        'label1
        '
        resources.ApplyResources(Me.label1, "label1")
        Me.label1.Name = "label1"
        '
        'mToolTip
        '
        Me.mToolTip.AutomaticDelay = 150
        Me.mToolTip.AutoPopDelay = 7500
        Me.mToolTip.InitialDelay = 150
        Me.mToolTip.ReshowDelay = 30
        '
        'tBar
        '
        resources.ApplyResources(Me.tBar, "tBar")
        Me.tBar.Name = "tBar"
        '
        'ADGroupSelectorForm
        '
        Me.AcceptButton = Me.btnFind
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.CancelButton = Me.btnCancel
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.tBar)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.tlp)
        Me.Name = "ADGroupSelectorForm"
        CType(Me.dgvGroup, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tlp.ResumeLayout(False)
        Me.panel1.ResumeLayout(False)
        Me.panel1.PerformLayout()
        CType(Me.btnFilterTypeTooltip, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Protected WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Protected WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents txtDomain As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents btnFind As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents dgvGroup As AutomateControls.DataGridViews.RowBasedDataGridView
    Private WithEvents txtFilter As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents tlp As System.Windows.Forms.TableLayoutPanel
    Private WithEvents panel1 As System.Windows.Forms.Panel
    Private WithEvents lblFilter As System.Windows.Forms.Label
    Private WithEvents label1 As System.Windows.Forms.Label
    Friend WithEvents cboFilterType As System.Windows.Forms.ComboBox
    Friend WithEvents btnFilterTypeTooltip As System.Windows.Forms.PictureBox
    Friend WithEvents mToolTip As System.Windows.Forms.ToolTip
    Private WithEvents lblLocation As System.Windows.Forms.Label
    Friend WithEvents tBar As AutomateControls.TitleBar
    Friend WithEvents txtLocation As AutomateControls.GuidanceTextBox
    Friend WithEvents colName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colType As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colPath As System.Windows.Forms.DataGridViewTextBoxColumn
End Class
