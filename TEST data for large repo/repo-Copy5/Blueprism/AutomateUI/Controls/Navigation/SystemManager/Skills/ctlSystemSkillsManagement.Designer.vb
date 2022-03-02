<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlSystemSkillsManagement
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSystemSkillsManagement))
        Me.dgvSkills = New AutomateControls.DataGridViews.RowBasedDataGridView()
        Me.colName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colCategory = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colProvider = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colCurrentVersion = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colInstalledVersions = New System.Windows.Forms.DataGridViewLinkColumn()
        Me.colWebAPI = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colEnabled = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn5 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.llDelete = New AutomateControls.BulletedLinkLabel()
        Me.llReferences = New AutomateControls.BulletedLinkLabel()
        CType(Me.dgvSkills, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'dgvSkills
        '
        resources.ApplyResources(Me.dgvSkills, "dgvSkills")
        Me.dgvSkills.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvSkills.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvSkills.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colName, Me.colCategory, Me.colProvider, Me.colCurrentVersion, Me.colInstalledVersions, Me.colWebAPI, Me.colEnabled})
        Me.dgvSkills.MultiSelect = False
        Me.dgvSkills.Name = "dgvSkills"
        Me.dgvSkills.RowTemplate.Height = 32
        '
        'colName
        '
        Me.colName.FillWeight = 50.0!
        resources.ApplyResources(Me.colName, "colName")
        Me.colName.Name = "colName"
        Me.colName.ReadOnly = True
        '
        'colCategory
        '
        Me.colCategory.FillWeight = 50.0!
        resources.ApplyResources(Me.colCategory, "colCategory")
        Me.colCategory.Name = "colCategory"
        Me.colCategory.ReadOnly = True
        '
        'colProvider
        '
        Me.colProvider.FillWeight = 50.0!
        resources.ApplyResources(Me.colProvider, "colProvider")
        Me.colProvider.Name = "colProvider"
        Me.colProvider.ReadOnly = True
        '
        'colCurrentVersion
        '
        Me.colCurrentVersion.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader
        Me.colCurrentVersion.FillWeight = 20.0!
        resources.ApplyResources(Me.colCurrentVersion, "colCurrentVersion")
        Me.colCurrentVersion.Name = "colCurrentVersion"
        Me.colCurrentVersion.ReadOnly = True
        '
        'colInstalledVersions
        '
        Me.colInstalledVersions.ActiveLinkColor = System.Drawing.Color.Blue
        Me.colInstalledVersions.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader
        Me.colInstalledVersions.FillWeight = 20.0!
        resources.ApplyResources(Me.colInstalledVersions, "colInstalledVersions")
        Me.colInstalledVersions.Name = "colInstalledVersions"
        Me.colInstalledVersions.ReadOnly = True
        Me.colInstalledVersions.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.colInstalledVersions.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'colWebAPI
        '
        Me.colWebAPI.FillWeight = 40.0!
        resources.ApplyResources(Me.colWebAPI, "colWebAPI")
        Me.colWebAPI.Name = "colWebAPI"
        Me.colWebAPI.ReadOnly = True
        '
        'colEnabled
        '
        Me.colEnabled.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader
        Me.colEnabled.FillWeight = 20.0!
        resources.ApplyResources(Me.colEnabled, "colEnabled")
        Me.colEnabled.Name = "colEnabled"
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = True
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.ReadOnly = True
        '
        'DataGridViewTextBoxColumn3
        '
        Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
        Me.DataGridViewTextBoxColumn3.ReadOnly = True
        '
        'DataGridViewTextBoxColumn4
        '
        Me.DataGridViewTextBoxColumn4.Name = "DataGridViewTextBoxColumn4"
        Me.DataGridViewTextBoxColumn4.ReadOnly = True
        '
        'DataGridViewTextBoxColumn5
        '
        Me.DataGridViewTextBoxColumn5.Name = "DataGridViewTextBoxColumn5"
        Me.DataGridViewTextBoxColumn5.ReadOnly = True
        '
        'llDelete
        '
        resources.ApplyResources(Me.llDelete, "llDelete")
        Me.llDelete.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llDelete.Name = "llDelete"
        Me.llDelete.TabStop = True
        Me.llDelete.UseCompatibleTextRendering = True
        '
        'llReferences
        '
        resources.ApplyResources(Me.llReferences, "llReferences")
        Me.llReferences.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llReferences.Name = "llReferences"
        Me.llReferences.TabStop = True
        Me.llReferences.UseCompatibleTextRendering = True
        '
        'ctlSystemSkillsManagement
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.llReferences)
        Me.Controls.Add(Me.llDelete)
        Me.Controls.Add(Me.dgvSkills)
        Me.Name = "ctlSystemSkillsManagement"
        resources.ApplyResources(Me, "$this")
        CType(Me.dgvSkills, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents dgvSkills As AutomateControls.DataGridViews.RowBasedDataGridView
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn3 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn4 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn5 As DataGridViewTextBoxColumn
    Friend WithEvents colPreviousVersions As DataGridViewLinkColumn
    Friend WithEvents colName As DataGridViewTextBoxColumn
    Friend WithEvents colCategory As DataGridViewTextBoxColumn
    Friend WithEvents colProvider As DataGridViewTextBoxColumn
    Friend WithEvents colCurrentVersion As DataGridViewTextBoxColumn
    Friend WithEvents colInstalledVersions As DataGridViewLinkColumn
    Friend WithEvents colWebAPI As DataGridViewTextBoxColumn
    Friend WithEvents colEnabled As DataGridViewCheckBoxColumn
    Friend WithEvents llDelete As AutomateControls.BulletedLinkLabel
    Friend WithEvents llReferences As AutomateControls.BulletedLinkLabel
End Class
