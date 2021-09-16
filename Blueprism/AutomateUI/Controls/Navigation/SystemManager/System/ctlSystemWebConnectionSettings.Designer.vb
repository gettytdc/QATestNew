<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlSystemWebConnectionSettings
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSystemWebConnectionSettings))
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.TableLayoutPanel4 = New System.Windows.Forms.TableLayoutPanel()
        Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
        Me.lblConnectionLimit = New System.Windows.Forms.Label()
        Me.numConnectionLimit = New AutomateControls.StyledNumericUpDown()
        Me.lblMaxIdleTime = New System.Windows.Forms.Label()
        Me.numMaxIdleTime = New AutomateControls.StyledNumericUpDown()
        Me.chkConnectionTimeout = New System.Windows.Forms.CheckBox()
        Me.numConnectionTimeout = New AutomateControls.StyledNumericUpDown()
        Me.dgvUriSettings = New System.Windows.Forms.DataGridView()
        Me.colDelete = New System.Windows.Forms.DataGridViewImageColumn()
        Me.colUri = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colConnectionLimit = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colMaxIdleTime = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colConnectionTimeout = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.btnSaveChanges = New AutomateControls.Buttons.StandardStyledButton()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.TableLayoutPanel4.SuspendLayout()
        Me.TableLayoutPanel2.SuspendLayout()
        CType(Me.numConnectionLimit, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numMaxIdleTime, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numConnectionTimeout, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgvUriSettings, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.GroupBox1, 0, 1)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'GroupBox1
        '
        resources.ApplyResources(Me.GroupBox1, "GroupBox1")
        Me.GroupBox1.Controls.Add(Me.TableLayoutPanel4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.TabStop = False
        '
        'TableLayoutPanel4
        '
        resources.ApplyResources(Me.TableLayoutPanel4, "TableLayoutPanel4")
        Me.TableLayoutPanel4.Controls.Add(Me.TableLayoutPanel2, 0, 0)
        Me.TableLayoutPanel4.Controls.Add(Me.dgvUriSettings, 0, 1)
        Me.TableLayoutPanel4.Controls.Add(Me.btnSaveChanges, 0, 2)
        Me.TableLayoutPanel4.Name = "TableLayoutPanel4"
        '
        'TableLayoutPanel2
        '
        resources.ApplyResources(Me.TableLayoutPanel2, "TableLayoutPanel2")
        Me.TableLayoutPanel2.Controls.Add(Me.lblConnectionLimit, 0, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.numConnectionLimit, 1, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.lblMaxIdleTime, 0, 1)
        Me.TableLayoutPanel2.Controls.Add(Me.numMaxIdleTime, 1, 1)
        Me.TableLayoutPanel2.Controls.Add(Me.chkConnectionTimeout, 0, 2)
        Me.TableLayoutPanel2.Controls.Add(Me.numConnectionTimeout, 1, 2)
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        '
        'lblConnectionLimit
        '
        resources.ApplyResources(Me.lblConnectionLimit, "lblConnectionLimit")
        Me.lblConnectionLimit.Name = "lblConnectionLimit"
        '
        'numConnectionLimit
        '
        resources.ApplyResources(Me.numConnectionLimit, "numConnectionLimit")
        Me.numConnectionLimit.Maximum = New Decimal(New Integer() {2147483647, 0, 0, 0})
        Me.numConnectionLimit.Minimum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.numConnectionLimit.Name = "numConnectionLimit"
        Me.numConnectionLimit.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'lblMaxIdleTime
        '
        resources.ApplyResources(Me.lblMaxIdleTime, "lblMaxIdleTime")
        Me.lblMaxIdleTime.Name = "lblMaxIdleTime"
        '
        'numMaxIdleTime
        '
        resources.ApplyResources(Me.numMaxIdleTime, "numMaxIdleTime")
        Me.numMaxIdleTime.Maximum = New Decimal(New Integer() {2147483647, 0, 0, 0})
        Me.numMaxIdleTime.Minimum = New Decimal(New Integer() {5, 0, 0, 0})
        Me.numMaxIdleTime.Name = "numMaxIdleTime"
        Me.numMaxIdleTime.Value = New Decimal(New Integer() {5, 0, 0, 0})
        '
        'chkConnectionTimeout
        '
        resources.ApplyResources(Me.chkConnectionTimeout, "chkConnectionTimeout")
        Me.chkConnectionTimeout.Name = "chkConnectionTimeout"
        Me.chkConnectionTimeout.UseVisualStyleBackColor = True
        '
        'numConnectionTimeout
        '
        resources.ApplyResources(Me.numConnectionTimeout, "numConnectionTimeout")
        Me.numConnectionTimeout.Maximum = New Decimal(New Integer() {2147483647, 0, 0, 0})
        Me.numConnectionTimeout.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.numConnectionTimeout.Name = "numConnectionTimeout"
        Me.numConnectionTimeout.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'dgvUriSettings
        '
        Me.dgvUriSettings.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvUriSettings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvUriSettings.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colDelete, Me.colUri, Me.colConnectionLimit, Me.colMaxIdleTime, Me.colConnectionTimeout})
        resources.ApplyResources(Me.dgvUriSettings, "dgvUriSettings")
        Me.dgvUriSettings.Name = "dgvUriSettings"
        Me.dgvUriSettings.RowHeadersVisible = False
        '
        'colDelete
        '
        Me.colDelete.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.colDelete, "colDelete")
        Me.colDelete.Name = "colDelete"
        '
        'colUri
        '
        Me.colUri.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colUri.FillWeight = 73.92513!
        resources.ApplyResources(Me.colUri, "colUri")
        Me.colUri.Name = "colUri"
        '
        'colConnectionLimit
        '
        Me.colConnectionLimit.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.colConnectionLimit.FillWeight = 101.3988!
        resources.ApplyResources(Me.colConnectionLimit, "colConnectionLimit")
        Me.colConnectionLimit.Name = "colConnectionLimit"
        Me.colConnectionLimit.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.colConnectionLimit.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'colMaxIdleTime
        '
        Me.colMaxIdleTime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.colMaxIdleTime.FillWeight = 111.0397!
        resources.ApplyResources(Me.colMaxIdleTime, "colMaxIdleTime")
        Me.colMaxIdleTime.Name = "colMaxIdleTime"
        Me.colMaxIdleTime.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.colMaxIdleTime.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'colConnectionTimeout
        '
        Me.colConnectionTimeout.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.colConnectionTimeout.FillWeight = 113.6364!
        resources.ApplyResources(Me.colConnectionTimeout, "colConnectionTimeout")
        Me.colConnectionTimeout.Name = "colConnectionTimeout"
        Me.colConnectionTimeout.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.colConnectionTimeout.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'btnSaveChanges
        '
        resources.ApplyResources(Me.btnSaveChanges, "btnSaveChanges")
        Me.btnSaveChanges.Name = "btnSaveChanges"
        Me.btnSaveChanges.UseVisualStyleBackColor = True
        '
        'DataGridViewTextBoxColumn1
        '
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = True
        '
        'ctlSystemWebConnectionSettings
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Name = "ctlSystemWebConnectionSettings"
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel1.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.TableLayoutPanel4.ResumeLayout(False)
        Me.TableLayoutPanel4.PerformLayout()
        Me.TableLayoutPanel2.ResumeLayout(False)
        Me.TableLayoutPanel2.PerformLayout()
        CType(Me.numConnectionLimit, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numMaxIdleTime, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numConnectionTimeout, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.dgvUriSettings, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents lblMaxIdleTime As Label
    Friend WithEvents lblConnectionLimit As Label
    Friend WithEvents numMaxIdleTime As NumericUpDown
    Friend WithEvents numConnectionLimit As NumericUpDown
    Friend WithEvents TableLayoutPanel2 As TableLayoutPanel
    Friend WithEvents TableLayoutPanel4 As TableLayoutPanel
    Friend WithEvents dgvUriSettings As DataGridView
    Friend WithEvents btnSaveChanges As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
    Friend WithEvents numConnectionTimeout As NumericUpDown
    Friend WithEvents chkConnectionTimeout As CheckBox
    Friend WithEvents colDelete As DataGridViewImageColumn
    Friend WithEvents colUri As DataGridViewTextBoxColumn
    Friend WithEvents colConnectionLimit As DataGridViewTextBoxColumn
    Friend WithEvents colMaxIdleTime As DataGridViewTextBoxColumn
    Friend WithEvents colConnectionTimeout As DataGridViewTextBoxColumn
End Class
