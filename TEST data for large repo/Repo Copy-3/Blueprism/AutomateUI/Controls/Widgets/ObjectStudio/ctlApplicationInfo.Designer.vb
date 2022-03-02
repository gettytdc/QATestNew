<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlApplicationInfo
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlApplicationInfo))
        Me.btnAppWizard = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnLaunch = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnDiag = New AutomateControls.Buttons.StandardStyledButton()
        Me.flowPane = New AutomateControls.FullWidthFlowLayoutScrollPane()
        Me.SuspendLayout()
        '
        'btnAppWizard
        '
        resources.ApplyResources(Me.btnAppWizard, "btnAppWizard")
        Me.btnAppWizard.Name = "btnAppWizard"
        Me.btnAppWizard.UseVisualStyleBackColor = True
        '
        'btnLaunch
        '
        resources.ApplyResources(Me.btnLaunch, "btnLaunch")
        Me.btnLaunch.Name = "btnLaunch"
        Me.btnLaunch.UseVisualStyleBackColor = True
        '
        'btnDiag
        '
        resources.ApplyResources(Me.btnDiag, "btnDiag")
        Me.btnDiag.Name = "btnDiag"
        Me.btnDiag.UseVisualStyleBackColor = True
        '
        'flowPane
        '
        resources.ApplyResources(Me.flowPane, "flowPane")
        Me.flowPane.Name = "flowPane"
        '
        'ctlApplicationInfo
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.flowPane)
        Me.Controls.Add(Me.btnDiag)
        Me.Controls.Add(Me.btnLaunch)
        Me.Controls.Add(Me.btnAppWizard)
        Me.Name = "ctlApplicationInfo"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents btnLaunch As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents btnDiag As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents flowPane As AutomateControls.FullWidthFlowLayoutScrollPane
    Private WithEvents btnAppWizard As AutomateControls.Buttons.StandardStyledButton

End Class
