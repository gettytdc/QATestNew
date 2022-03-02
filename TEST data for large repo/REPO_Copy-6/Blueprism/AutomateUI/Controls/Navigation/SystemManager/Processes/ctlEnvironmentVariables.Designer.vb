<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlEnvironmentVariables
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlEnvironmentVariables))
        Me.btnRemoveEnvVar = New AutomateControls.BulletedLinkLabel()
        Me.btnAddEnvironmentVariable = New AutomateControls.BulletedLinkLabel()
        Me.btnEnvironmentVariableApply = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnFindReferences = New AutomateControls.BulletedLinkLabel()
        Me.lvEnvVars = New AutomateUI.ctlListView()
        Me.SuspendLayout()
        '
        'btnRemoveEnvVar
        '
        resources.ApplyResources(Me.btnRemoveEnvVar, "btnRemoveEnvVar")
        Me.btnRemoveEnvVar.BackColor = System.Drawing.Color.Transparent
        Me.btnRemoveEnvVar.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.btnRemoveEnvVar.Name = "btnRemoveEnvVar"
        Me.btnRemoveEnvVar.TabStop = True
        Me.btnRemoveEnvVar.UseCompatibleTextRendering = True
        '
        'btnAddEnvironmentVariable
        '
        resources.ApplyResources(Me.btnAddEnvironmentVariable, "btnAddEnvironmentVariable")
        Me.btnAddEnvironmentVariable.BackColor = System.Drawing.Color.Transparent
        Me.btnAddEnvironmentVariable.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.btnAddEnvironmentVariable.Name = "btnAddEnvironmentVariable"
        Me.btnAddEnvironmentVariable.TabStop = True
        Me.btnAddEnvironmentVariable.UseCompatibleTextRendering = True
        '
        'btnEnvironmentVariableApply
        '
        resources.ApplyResources(Me.btnEnvironmentVariableApply, "btnEnvironmentVariableApply")
        Me.btnEnvironmentVariableApply.Name = "btnEnvironmentVariableApply"
        Me.btnEnvironmentVariableApply.UseVisualStyleBackColor = True
        '
        'btnFindReferences
        '
        resources.ApplyResources(Me.btnFindReferences, "btnFindReferences")
        Me.btnFindReferences.BackColor = System.Drawing.Color.Transparent
        Me.btnFindReferences.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.btnFindReferences.Name = "btnFindReferences"
        Me.btnFindReferences.TabStop = True
        Me.btnFindReferences.UseCompatibleTextRendering = True
        '
        'lvEnvVars
        '
        Me.lvEnvVars.AllowDrop = True
        resources.ApplyResources(Me.lvEnvVars, "lvEnvVars")
        Me.lvEnvVars.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.lvEnvVars.CurrentEditableRow = Nothing
        Me.lvEnvVars.FillColumn = Nothing
        Me.lvEnvVars.FrameBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lvEnvVars.HighlightedForeColor = System.Drawing.SystemColors.HighlightText
        Me.lvEnvVars.HighlightedRowBackColour = System.Drawing.Color.FromArgb(CType(CType(182, Byte), Integer), CType(CType(202, Byte), Integer), CType(CType(234, Byte), Integer))
        Me.lvEnvVars.HighlightedRowOutline = System.Drawing.Color.Red
        Me.lvEnvVars.LastColumnAutoSize = False
        Me.lvEnvVars.MinimumColumnWidth = 200
        Me.lvEnvVars.Name = "lvEnvVars"
        Me.lvEnvVars.Readonly = False
        Me.lvEnvVars.RowHeight = 26
        Me.lvEnvVars.Rows.Capacity = 0
        Me.lvEnvVars.Sortable = False
        '
        'ctlEnvironmentVariables
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.btnFindReferences)
        Me.Controls.Add(Me.btnEnvironmentVariableApply)
        Me.Controls.Add(Me.btnRemoveEnvVar)
        Me.Controls.Add(Me.btnAddEnvironmentVariable)
        Me.Controls.Add(Me.lvEnvVars)
        Me.Name = "ctlEnvironmentVariables"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnRemoveEnvVar As AutomateControls.BulletedLinkLabel
    Friend WithEvents btnAddEnvironmentVariable As AutomateControls.BulletedLinkLabel
    Friend WithEvents lvEnvVars As AutomateUI.ctlListView
    Friend WithEvents btnEnvironmentVariableApply As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnFindReferences As AutomateControls.BulletedLinkLabel

End Class
