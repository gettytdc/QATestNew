<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlPackageDetails

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
        Dim lblReleases As System.Windows.Forms.Label
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlPackageDetails))
        Me.gridReleaseHistory = New System.Windows.Forms.DataGridView()
        Me.colReleaseIcon = New AutomateControls.DataGridViews.ImageListColumn()
        Me.colReleaseName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colReleaseDate = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colReleaseUser = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.btnNewRelease = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnEditPackage = New AutomateControls.Buttons.StandardStyledButton()
        lblReleases = New System.Windows.Forms.Label()
        CType(Me.mSplitter,System.ComponentModel.ISupportInitialize).BeginInit
        Me.mSplitter.Panel1.SuspendLayout
        Me.mSplitter.Panel2.SuspendLayout
        Me.mSplitter.SuspendLayout
        CType(Me.gridReleaseHistory,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'mSplitter
        '
        '
        'mSplitter.Panel1
        '
        Me.mSplitter.Panel1.Controls.Add(Me.btnEditPackage)
        resources.ApplyResources(Me.mSplitter, "mSplitter")
        '
        'mSplitter.Panel2
        '
        Me.mSplitter.Panel2.Controls.Add(Me.gridReleaseHistory)
        Me.mSplitter.Panel2.Controls.Add(Me.btnNewRelease)
        Me.mSplitter.Panel2.Controls.Add(lblReleases)
        Me.mSplitter.Panel2Collapsed = false
        '
        'lblTitle
        '
        resources.ApplyResources(Me.lblTitle, "lblTitle")
        '
        'lblReleases
        '
        resources.ApplyResources(lblReleases, "lblReleases")
        lblReleases.Name = "lblReleases"
        '
        'gridReleaseHistory
        '
        Me.gridReleaseHistory.AllowUserToAddRows = false
        Me.gridReleaseHistory.AllowUserToDeleteRows = false
        Me.gridReleaseHistory.AllowUserToResizeColumns = false
        Me.gridReleaseHistory.AllowUserToResizeRows = false
        resources.ApplyResources(Me.gridReleaseHistory, "gridReleaseHistory")
        Me.gridReleaseHistory.BackgroundColor = System.Drawing.SystemColors.Window
        Me.gridReleaseHistory.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.gridReleaseHistory.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal
        Me.gridReleaseHistory.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None
        Me.gridReleaseHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridReleaseHistory.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colReleaseIcon, Me.colReleaseName, Me.colReleaseDate, Me.colReleaseUser})
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
        DataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.gridReleaseHistory.DefaultCellStyle = DataGridViewCellStyle1
        Me.gridReleaseHistory.GridColor = System.Drawing.SystemColors.ControlLight
        Me.gridReleaseHistory.Name = "gridReleaseHistory"
        Me.gridReleaseHistory.ReadOnly = true
        Me.gridReleaseHistory.RowHeadersVisible = false
        Me.gridReleaseHistory.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing
        Me.gridReleaseHistory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.gridReleaseHistory.ShowEditingIcon = false
        Me.gridReleaseHistory.StandardTab = true
        '
        'colReleaseIcon
        '
        Me.colReleaseIcon.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.colReleaseIcon, "colReleaseIcon")
        Me.colReleaseIcon.Name = "colReleaseIcon"
        Me.colReleaseIcon.ReadOnly = true
        Me.colReleaseIcon.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.colReleaseIcon.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'colReleaseName
        '
        Me.colReleaseName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colReleaseName, "colReleaseName")
        Me.colReleaseName.Name = "colReleaseName"
        Me.colReleaseName.ReadOnly = true
        '
        'colReleaseDate
        '
        Me.colReleaseDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.colReleaseDate, "colReleaseDate")
        Me.colReleaseDate.Name = "colReleaseDate"
        Me.colReleaseDate.ReadOnly = true
        '
        'colReleaseUser
        '
        Me.colReleaseUser.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.colReleaseUser, "colReleaseUser")
        Me.colReleaseUser.Name = "colReleaseUser"
        Me.colReleaseUser.ReadOnly = true
        '
        'btnNewRelease
        '
        resources.ApplyResources(Me.btnNewRelease, "btnNewRelease")
        Me.btnNewRelease.Name = "btnNewRelease"
        Me.btnNewRelease.UseVisualStyleBackColor = true
        '
        'btnEditPackage
        '
        resources.ApplyResources(Me.btnEditPackage, "btnEditPackage")
        Me.btnEditPackage.Name = "btnEditPackage"
        Me.btnEditPackage.UseVisualStyleBackColor = true
        '
        'ctlPackageDetails
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.Name = "ctlPackageDetails"
        Me.Controls.SetChildIndex(Me.lblTitle, 0)
        Me.Controls.SetChildIndex(Me.mSplitter, 0)
        Me.mSplitter.Panel1.ResumeLayout(false)
        Me.mSplitter.Panel1.PerformLayout
        Me.mSplitter.Panel2.ResumeLayout(false)
        Me.mSplitter.Panel2.PerformLayout
        CType(Me.mSplitter,System.ComponentModel.ISupportInitialize).EndInit
        Me.mSplitter.ResumeLayout(false)
        CType(Me.gridReleaseHistory,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Private WithEvents btnNewRelease As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents gridReleaseHistory As System.Windows.Forms.DataGridView
    Friend WithEvents colIcon As System.Windows.Forms.DataGridViewImageColumn
    Friend WithEvents colReleaseIcon As AutomateControls.DataGridViews.ImageListColumn
    Friend WithEvents colReleaseName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colReleaseDate As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colReleaseUser As System.Windows.Forms.DataGridViewTextBoxColumn
    Private WithEvents btnEditPackage As AutomateControls.Buttons.StandardStyledButton

End Class
