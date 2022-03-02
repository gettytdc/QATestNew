Namespace Controls.Widgets.SystemManager.WebApi

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class HttpHeaderPanel
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(HttpHeaderPanel))
        Me.gridHeaders = New System.Windows.Forms.DataGridView()
        Me.ctlToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.colHeaderName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colHeaderValue = New System.Windows.Forms.DataGridViewTextBoxColumn()
        CType(Me.gridHeaders,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'gridHeaders
        '
        resources.ApplyResources(Me.gridHeaders, "gridHeaders")
        Me.gridHeaders.BackgroundColor = System.Drawing.SystemColors.ControlLight
        Me.gridHeaders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridHeaders.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colHeaderName, Me.colHeaderValue})
        Me.gridHeaders.Name = "gridHeaders"
        Me.gridHeaders.RowHeadersVisible = false
        Me.ctlToolTip.SetToolTip(Me.gridHeaders, resources.GetString("gridHeaders.ToolTip"))
        '
        'colHeaderName
        '
        Me.colHeaderName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colHeaderName, "colHeaderName")
        Me.colHeaderName.Name = "colHeaderName"
        '
        'colHeaderValue
        '
        Me.colHeaderValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colHeaderValue, "colHeaderValue")
        Me.colHeaderValue.Name = "colHeaderValue"
        '
        'HttpHeaderPanel
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.gridHeaders)
        Me.Name = "HttpHeaderPanel"
        Me.ctlToolTip.SetToolTip(Me, resources.GetString("$this.ToolTip"))
        CType(Me.gridHeaders,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)

End Sub

        Private WithEvents gridHeaders As DataGridView
        Friend WithEvents ctlToolTip As ToolTip
        Friend WithEvents colHeaderName As DataGridViewTextBoxColumn
        Friend WithEvents colHeaderValue As DataGridViewTextBoxColumn
    End Class

End Namespace