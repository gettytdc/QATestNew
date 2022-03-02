Imports AutomateUI.My.Resources

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlChartTile
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlChartTile))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.cmbChartType = New System.Windows.Forms.ComboBox()
        Me.dgvParameters = New System.Windows.Forms.DataGridView()
        Me.paramName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.paramValue = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.RealParamName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.cmbProcedure = New System.Windows.Forms.ComboBox()
        Me.lblParameters = New System.Windows.Forms.Label()
        Me.lblChartType = New System.Windows.Forms.Label()
        Me.lblProcedure = New System.Windows.Forms.Label()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.btnDetails = New AutomateUI.ctlRolloverButton()
        CType(Me.dgvParameters, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'cmbChartType
        '
        resources.ApplyResources(Me.cmbChartType, "cmbChartType")
        Me.cmbChartType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbChartType.FormattingEnabled = True
        Me.cmbChartType.Name = "cmbChartType"
        '
        'dgvParameters
        '
        Me.dgvParameters.AllowUserToAddRows = False
        Me.dgvParameters.AllowUserToDeleteRows = False
        Me.dgvParameters.AllowUserToResizeRows = False
        resources.ApplyResources(Me.dgvParameters, "dgvParameters")
        Me.dgvParameters.BackgroundColor = System.Drawing.SystemColors.Window
        Me.dgvParameters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvParameters.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.paramName, Me.paramValue, Me.RealParamName})
        Me.dgvParameters.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter
        Me.dgvParameters.Name = "dgvParameters"
        Me.dgvParameters.RowHeadersVisible = False
        Me.dgvParameters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'paramName
        '
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.White
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black
        Me.paramName.DefaultCellStyle = DataGridViewCellStyle1
        resources.ApplyResources(Me.paramName, "paramName")
        Me.paramName.Name = "paramName"
        Me.paramName.ReadOnly = True
        '
        'paramValue
        '
        Me.paramValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.White
        DataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black
        Me.paramValue.DefaultCellStyle = DataGridViewCellStyle2
        resources.ApplyResources(Me.paramValue, "paramValue")
        Me.paramValue.Name = "paramValue"
        '
        'RealParamName
        '
        resources.ApplyResources(Me.RealParamName, "RealParamName")
        Me.RealParamName.Name = "RealParamName"
        Me.RealParamName.ReadOnly = True
        '
        'cmbProcedure
        '
        resources.ApplyResources(Me.cmbProcedure, "cmbProcedure")
        Me.cmbProcedure.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbProcedure.FormattingEnabled = True
        Me.cmbProcedure.Name = "cmbProcedure"
        Me.cmbProcedure.Sorted = True
        '
        'lblParameters
        '
        resources.ApplyResources(Me.lblParameters, "lblParameters")
        Me.lblParameters.Name = "lblParameters"
        '
        'lblChartType
        '
        resources.ApplyResources(Me.lblChartType, "lblChartType")
        Me.lblChartType.Name = "lblChartType"
        '
        'lblProcedure
        '
        resources.ApplyResources(Me.lblProcedure, "lblProcedure")
        Me.lblProcedure.Name = "lblProcedure"
        '
        'DataGridViewTextBoxColumn1
        '
        DataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.White
        DataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black
        Me.DataGridViewTextBoxColumn1.DefaultCellStyle = DataGridViewCellStyle3
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = True
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.White
        DataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.Black
        Me.DataGridViewTextBoxColumn2.DefaultCellStyle = DataGridViewCellStyle4
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        '
        'DataGridViewTextBoxColumn3
        '
        resources.ApplyResources(Me.DataGridViewTextBoxColumn3, "DataGridViewTextBoxColumn3")
        Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
        Me.DataGridViewTextBoxColumn3.ReadOnly = True
        '
        'btnDetails
        '
        resources.ApplyResources(Me.btnDetails, "btnDetails")
        Me.btnDetails.DefaultImage = Global.AutomateUI.My.Resources.ToolImages.Information_16x16
        Me.btnDetails.DisabledImage = Global.AutomateUI.My.Resources.ToolImages.Information_16x16_Disabled
        Me.btnDetails.Name = "btnDetails"
        Me.btnDetails.RolloverImage = Global.AutomateUI.My.Resources.ToolImages.Information_16x16_Hot
        Me.btnDetails.TooltipTitle = My.Resources.ViewDocumentation
        Me.btnDetails.TooltipText = My.Resources.ctlChartTile_ClickDataSourceDocumentation
        '
        'ctlChartTile
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.btnDetails)
        Me.Controls.Add(Me.cmbChartType)
        Me.Controls.Add(Me.dgvParameters)
        Me.Controls.Add(Me.cmbProcedure)
        Me.Controls.Add(Me.lblParameters)
        Me.Controls.Add(Me.lblChartType)
        Me.Controls.Add(Me.lblProcedure)
        Me.Name = "ctlChartTile"
        resources.ApplyResources(Me, "$this")
        CType(Me.dgvParameters, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents cmbChartType As System.Windows.Forms.ComboBox
    Friend WithEvents dgvParameters As System.Windows.Forms.DataGridView
    Friend WithEvents cmbProcedure As System.Windows.Forms.ComboBox
    Friend WithEvents lblParameters As System.Windows.Forms.Label
    Friend WithEvents lblChartType As System.Windows.Forms.Label
    Friend WithEvents lblProcedure As System.Windows.Forms.Label
    Friend WithEvents btnDetails As AutomateUI.ctlRolloverButton
    Friend WithEvents paramName As DataGridViewTextBoxColumn
    Friend WithEvents paramValue As DataGridViewTextBoxColumn
    Friend WithEvents RealParamName As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn3 As DataGridViewTextBoxColumn
End Class
