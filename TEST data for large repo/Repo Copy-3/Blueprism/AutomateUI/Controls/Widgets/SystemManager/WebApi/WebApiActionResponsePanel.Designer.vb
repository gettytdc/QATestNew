Namespace Controls.Widgets.SystemManager.WebApi

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class WebApiActionResponsePanel
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(WebApiActionResponsePanel))
        Me.ResponseParameterSplitPanel = New System.Windows.Forms.SplitContainer()
        Me.gridCustomOutputParameters = New System.Windows.Forms.DataGridView()
            Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
            Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
            Me.colDelete = New System.Windows.Forms.DataGridViewImageColumn()
            Me.colParameter = New System.Windows.Forms.DataGridViewTextBoxColumn()
            Me.colDescription = New System.Windows.Forms.DataGridViewTextBoxColumn()
            Me.colDataType = New System.Windows.Forms.DataGridViewComboBoxColumn()
            Me.colMethodType = New System.Windows.Forms.DataGridViewComboBoxColumn()
            Me.colJsonPath = New System.Windows.Forms.DataGridViewTextBoxColumn()
            CType(Me.ResponseParameterSplitPanel, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.ResponseParameterSplitPanel.Panel1.SuspendLayout()
            Me.ResponseParameterSplitPanel.SuspendLayout()
            CType(Me.gridCustomOutputParameters, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'ResponseParameterSplitPanel
            '
            resources.ApplyResources(Me.ResponseParameterSplitPanel, "ResponseParameterSplitPanel")
            Me.ResponseParameterSplitPanel.Name = "ResponseParameterSplitPanel"
            '
            'ResponseParameterSplitPanel.Panel1
            '
            Me.ResponseParameterSplitPanel.Panel1.AccessibleRole = System.Windows.Forms.AccessibleRole.None
            Me.ResponseParameterSplitPanel.Panel1.Controls.Add(Me.gridCustomOutputParameters)
            '
            'gridCustomOutputParameters
            '
            Me.gridCustomOutputParameters.AllowUserToResizeRows = False
            Me.gridCustomOutputParameters.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
            Me.gridCustomOutputParameters.BackgroundColor = System.Drawing.SystemColors.ControlLight
            Me.gridCustomOutputParameters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
            Me.gridCustomOutputParameters.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colDelete, Me.colParameter, Me.colDescription, Me.colDataType, Me.colMethodType, Me.colJsonPath})
            resources.ApplyResources(Me.gridCustomOutputParameters, "gridCustomOutputParameters")
            Me.gridCustomOutputParameters.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter
            Me.gridCustomOutputParameters.MultiSelect = False
            Me.gridCustomOutputParameters.Name = "gridCustomOutputParameters"
            Me.gridCustomOutputParameters.RowHeadersVisible = False
            '
            'DataGridViewTextBoxColumn1
            '
            Me.DataGridViewTextBoxColumn1.FillWeight = 115.0!
            resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
            Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
            '
            'DataGridViewTextBoxColumn2
            '
            Me.DataGridViewTextBoxColumn2.FillWeight = 115.0!
            resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
            Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
            '
            'colDelete
            '
            Me.colDelete.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
            Me.colDelete.FillWeight = 23!
            resources.ApplyResources(Me.colDelete, "colDelete")
            Me.colDelete.Name = "colDelete"
            '
            'colParameter
            '
            Me.colParameter.FillWeight = 115.0!
            resources.ApplyResources(Me.colParameter, "colParameter")
            Me.colParameter.Name = "colParameter"
            '
            'colDescription
            '
            Me.colDescription.FillWeight = 115.0!
            resources.ApplyResources(Me.colDescription, "colDescription")
            Me.colDescription.Name = "colDescription"
            '
            'colDataType
            '
            Me.colDataType.FillWeight = 115.0!
            resources.ApplyResources(Me.colDataType, "colDataType")
            Me.colDataType.Name = "colDataType"
            '
            'colMethodType
            '
            Me.colMethodType.FillWeight = 115.0!
            resources.ApplyResources(Me.colMethodType, "colMethodType")
            Me.colMethodType.Name = "colMethodType"
            '
            'colJsonPath
            '
            Me.colJsonPath.FillWeight = 115.0!
            resources.ApplyResources(Me.colJsonPath, "colJsonPath")
            Me.colJsonPath.Name = "colJsonPath"
            '
            'WebApiActionResponsePanel
            '
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
            Me.Controls.Add(Me.ResponseParameterSplitPanel)
            resources.ApplyResources(Me, "$this")
            Me.Name = "WebApiActionResponsePanel"
            Me.ResponseParameterSplitPanel.Panel1.ResumeLayout(false)
        CType(Me.ResponseParameterSplitPanel,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResponseParameterSplitPanel.ResumeLayout(false)
        CType(Me.gridCustomOutputParameters,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)

End Sub
        Private WithEvents gridCustomOutputParameters As DataGridView
        Friend WithEvents ResponseParameterSplitPanel As SplitContainer
        Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
        Friend WithEvents DataGridViewTextBoxColumn2 As DataGridViewTextBoxColumn
        Friend WithEvents colDelete As DataGridViewImageColumn
        Friend WithEvents colParameter As DataGridViewTextBoxColumn
        Friend WithEvents colDescription As DataGridViewTextBoxColumn
        Friend WithEvents colDataType As DataGridViewComboBoxColumn
        Friend WithEvents colMethodType As DataGridViewComboBoxColumn
        Friend WithEvents colJsonPath As DataGridViewTextBoxColumn
    End Class

End Namespace
