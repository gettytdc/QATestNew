<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmUserGroupMembership
    Inherits AutomateUI.frmForm

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmUserGroupMembership))
        Me.lblDomainGroups = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.dgvSecurityGroups = New AutomateControls.DataGridViews.RowBasedDataGridView()
        Me.colName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colType = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colPath = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvRoles = New AutomateControls.DataGridViews.RowBasedDataGridView()
        Me.titleBar = New AutomateControls.TitleBar()
        Me.GrippableSplitContainer1 = New AutomateControls.GrippableSplitContainer()
        Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
        Me.TableLayoutPanel3 = New System.Windows.Forms.TableLayoutPanel()
        Me.llQueryGroups = New System.Windows.Forms.LinkLabel()
        Me.lblDomainContext = New System.Windows.Forms.Label()
        Me.colRole = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colSecurityGroup = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colUserIsMember = New System.Windows.Forms.DataGridViewTextBoxColumn()
        CType(Me.dgvSecurityGroups,System.ComponentModel.ISupportInitialize).BeginInit
        CType(Me.dgvRoles,System.ComponentModel.ISupportInitialize).BeginInit
        CType(Me.GrippableSplitContainer1,System.ComponentModel.ISupportInitialize).BeginInit
        Me.GrippableSplitContainer1.Panel1.SuspendLayout
        Me.GrippableSplitContainer1.Panel2.SuspendLayout
        Me.GrippableSplitContainer1.SuspendLayout
        Me.TableLayoutPanel2.SuspendLayout
        Me.TableLayoutPanel3.SuspendLayout
        Me.SuspendLayout
        '
        'lblDomainGroups
        '
        resources.ApplyResources(Me.lblDomainGroups, "lblDomainGroups")
        Me.lblDomainGroups.Name = "lblDomainGroups"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'dgvSecurityGroups
        '
        Me.dgvSecurityGroups.AllowUserToOrderColumns = true
        resources.ApplyResources(Me.dgvSecurityGroups, "dgvSecurityGroups")
        Me.dgvSecurityGroups.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvSecurityGroups.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colName, Me.colType, Me.colPath})
        Me.TableLayoutPanel3.SetColumnSpan(Me.dgvSecurityGroups, 2)
        Me.dgvSecurityGroups.MultiSelect = false
        Me.dgvSecurityGroups.Name = "dgvSecurityGroups"
        Me.dgvSecurityGroups.ReadOnly = true
        '
        'colName
        '
        resources.ApplyResources(Me.colName, "colName")
        Me.colName.Name = "colName"
        Me.colName.ReadOnly = true
        '
        'colType
        '
        Me.colType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells		
        resources.ApplyResources(Me.colType, "colType")
        Me.colType.Name = "colType"
        Me.colType.ReadOnly = true
        '
        'colPath
        '
        resources.ApplyResources(Me.colPath, "colPath")
        Me.colPath.Name = "colPath"
        Me.colPath.ReadOnly = true
        '
        'dgvRoles
        '
        Me.dgvRoles.AllowUserToOrderColumns = true
        resources.ApplyResources(Me.dgvRoles, "dgvRoles")
        Me.dgvRoles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvRoles.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colRole, Me.colSecurityGroup, Me.colUserIsMember})
        Me.dgvRoles.MultiSelect = false
        Me.dgvRoles.Name = "dgvRoles"
        Me.dgvRoles.ReadOnly = true
        '
        'titleBar
        '
        resources.ApplyResources(Me.titleBar, "titleBar")
        Me.titleBar.Name = "titleBar"
        Me.titleBar.TabStop = false
        '
        'GrippableSplitContainer1
        '
        resources.ApplyResources(Me.GrippableSplitContainer1, "GrippableSplitContainer1")
        Me.GrippableSplitContainer1.Name = "GrippableSplitContainer1"
        '
        'GrippableSplitContainer1.Panel1
        '
        Me.GrippableSplitContainer1.Panel1.Controls.Add(Me.TableLayoutPanel2)
        '
        'GrippableSplitContainer1.Panel2
        '
        Me.GrippableSplitContainer1.Panel2.Controls.Add(Me.TableLayoutPanel3)
        Me.GrippableSplitContainer1.TabStop = false
        '
        'TableLayoutPanel2
        '
        resources.ApplyResources(Me.TableLayoutPanel2, "TableLayoutPanel2")
        Me.TableLayoutPanel2.Controls.Add(Me.Label1, 0, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.dgvRoles, 0, 1)
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        '
        'TableLayoutPanel3
        '
        resources.ApplyResources(Me.TableLayoutPanel3, "TableLayoutPanel3")
        Me.TableLayoutPanel3.Controls.Add(Me.dgvSecurityGroups, 0, 1)
        Me.TableLayoutPanel3.Controls.Add(Me.lblDomainGroups, 0, 0)
        Me.TableLayoutPanel3.Controls.Add(Me.llQueryGroups, 1, 0)
        Me.TableLayoutPanel3.Name = "TableLayoutPanel3"
        '
        'llQueryGroups
        '
        resources.ApplyResources(Me.llQueryGroups, "llQueryGroups")
        Me.llQueryGroups.Name = "llQueryGroups"
        Me.llQueryGroups.TabStop = true
        '
        'lblDomainContext
        '
        resources.ApplyResources(Me.lblDomainContext, "lblDomainContext")
        Me.lblDomainContext.Name = "lblDomainContext"
        '
        'colRole
        '
        resources.ApplyResources(Me.colRole, "colRole")
        Me.colRole.Name = "colRole"
        Me.colRole.ReadOnly = true
        '
        'colSecurityGroup
        '
        Me.colSecurityGroup.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colSecurityGroup, "colSecurityGroup")
        Me.colSecurityGroup.Name = "colSecurityGroup"
        Me.colSecurityGroup.ReadOnly = true
        '
        'colUserIsMember
        '
        Me.colUserIsMember.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader
        resources.ApplyResources(Me.colUserIsMember, "colUserIsMember")
        Me.colUserIsMember.Name = "colUserIsMember"
        Me.colUserIsMember.ReadOnly = true
        Me.colUserIsMember.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        '
        'frmUserGroupMembership
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.lblDomainContext)
        Me.Controls.Add(Me.GrippableSplitContainer1)
        Me.Controls.Add(Me.titleBar)
        Me.Name = "frmUserGroupMembership"
        CType(Me.dgvSecurityGroups,System.ComponentModel.ISupportInitialize).EndInit
        CType(Me.dgvRoles,System.ComponentModel.ISupportInitialize).EndInit
        Me.GrippableSplitContainer1.Panel1.ResumeLayout(false)
        Me.GrippableSplitContainer1.Panel2.ResumeLayout(false)
        CType(Me.GrippableSplitContainer1,System.ComponentModel.ISupportInitialize).EndInit
        Me.GrippableSplitContainer1.ResumeLayout(false)
        Me.TableLayoutPanel2.ResumeLayout(false)
        Me.TableLayoutPanel2.PerformLayout
        Me.TableLayoutPanel3.ResumeLayout(false)
        Me.TableLayoutPanel3.PerformLayout
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Friend WithEvents lblDomainGroups As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Private WithEvents dgvSecurityGroups As AutomateControls.DataGridViews.RowBasedDataGridView
    Private WithEvents dgvRoles As AutomateControls.DataGridViews.RowBasedDataGridView
    Friend WithEvents titleBar As AutomateControls.TitleBar
    Friend WithEvents GrippableSplitContainer1 As AutomateControls.GrippableSplitContainer
    Friend WithEvents TableLayoutPanel2 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents TableLayoutPanel3 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents lblDomainContext As System.Windows.Forms.Label
    Friend WithEvents colName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colType As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colPath As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents llQueryGroups As System.Windows.Forms.LinkLabel
    Friend WithEvents colRole As DataGridViewTextBoxColumn
    Friend WithEvents colSecurityGroup As DataGridViewTextBoxColumn
    Friend WithEvents colUserIsMember As DataGridViewTextBoxColumn
End Class
