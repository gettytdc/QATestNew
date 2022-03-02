<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlResourcePools
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlResourcePools))
        Me.btnNewPool = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnDeletePool = New AutomateControls.Buttons.StandardStyledButton()
        Me.lblAvailableResources = New System.Windows.Forms.Label()
        Me.lblResourcePools = New System.Windows.Forms.Label()
        Me.btnRemoveFromPool = New AutomateControls.Buttons.StandardStyledButton()
        Me.trvResourcePools = New System.Windows.Forms.TreeView()
        Me.lstPoolsAvailableResources = New System.Windows.Forms.ListView()
        Me.cName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.lblPoolsHint = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'btnNewPool
        '
        resources.ApplyResources(Me.btnNewPool, "btnNewPool")
        Me.btnNewPool.Name = "btnNewPool"
        Me.btnNewPool.UseVisualStyleBackColor = True
        '
        'btnDeletePool
        '
        resources.ApplyResources(Me.btnDeletePool, "btnDeletePool")
        Me.btnDeletePool.Name = "btnDeletePool"
        Me.btnDeletePool.UseVisualStyleBackColor = True
        '
        'lblAvailableResources
        '
        resources.ApplyResources(Me.lblAvailableResources, "lblAvailableResources")
        Me.lblAvailableResources.BackColor = System.Drawing.Color.Transparent
        Me.lblAvailableResources.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.lblAvailableResources.Name = "lblAvailableResources"
        '
        'lblResourcePools
        '
        resources.ApplyResources(Me.lblResourcePools, "lblResourcePools")
        Me.lblResourcePools.BackColor = System.Drawing.Color.Transparent
        Me.lblResourcePools.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.lblResourcePools.Name = "lblResourcePools"
        '
        'btnRemoveFromPool
        '
        resources.ApplyResources(Me.btnRemoveFromPool, "btnRemoveFromPool")
        Me.btnRemoveFromPool.Name = "btnRemoveFromPool"
        Me.btnRemoveFromPool.UseVisualStyleBackColor = True
        '
        'trvResourcePools
        '
        Me.trvResourcePools.AllowDrop = True
        resources.ApplyResources(Me.trvResourcePools, "trvResourcePools")
        Me.trvResourcePools.HideSelection = False
        Me.trvResourcePools.Name = "trvResourcePools"
        '
        'lstPoolsAvailableResources
        '
        resources.ApplyResources(Me.lstPoolsAvailableResources, "lstPoolsAvailableResources")
        Me.lstPoolsAvailableResources.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.cName})
        Me.lstPoolsAvailableResources.Name = "lstPoolsAvailableResources"
        Me.lstPoolsAvailableResources.UseCompatibleStateImageBehavior = False
        Me.lstPoolsAvailableResources.View = System.Windows.Forms.View.Details
        '
        'cName
        '
        resources.ApplyResources(Me.cName, "cName")
        '
        'lblPoolsHint
        '
        resources.ApplyResources(Me.lblPoolsHint, "lblPoolsHint")
        Me.lblPoolsHint.BackColor = System.Drawing.Color.Transparent
        Me.lblPoolsHint.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.lblPoolsHint.Name = "lblPoolsHint"
        '
        'ctlResourcePools
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.btnNewPool)
        Me.Controls.Add(Me.btnDeletePool)
        Me.Controls.Add(Me.lblAvailableResources)
        Me.Controls.Add(Me.lblResourcePools)
        Me.Controls.Add(Me.btnRemoveFromPool)
        Me.Controls.Add(Me.trvResourcePools)
        Me.Controls.Add(Me.lstPoolsAvailableResources)
        Me.Controls.Add(Me.lblPoolsHint)
        Me.Name = "ctlResourcePools"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnNewPool As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnDeletePool As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lblAvailableResources As System.Windows.Forms.Label
    Friend WithEvents lblResourcePools As System.Windows.Forms.Label
    Friend WithEvents btnRemoveFromPool As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents trvResourcePools As System.Windows.Forms.TreeView
    Friend WithEvents lstPoolsAvailableResources As System.Windows.Forms.ListView
    Friend WithEvents cName As System.Windows.Forms.ColumnHeader
    Friend WithEvents lblPoolsHint As System.Windows.Forms.Label

End Class
