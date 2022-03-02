Imports AutomateControls.Wizard
Imports DataPipelineOutputConfigUISettings = BluePrism.DataPipeline.UI.DataPipelineOutputConfigUISettings

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlChooseSummary
    Inherits WizardPanel

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
        Me.txtSummary = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnAdvanced = New AutomateControls.Buttons.StandardStyledButton()
        Me.SuspendLayout()
        '
        'txtSummary
        '
        Me.txtSummary.Location = New System.Drawing.Point(18, 18)
        Me.txtSummary.Multiline = True
        Me.txtSummary.Name = "txtSummary"
        Me.txtSummary.ReadOnly = True
        Me.txtSummary.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtSummary.Size = New System.Drawing.Size(747, 372)
        Me.txtSummary.TabIndex = 0

        '
        'btnAdvanced
        '
        Me.btnAdvanced.Location = New System.Drawing.Point(552, 396)
        Me.btnAdvanced.Name = "btnAdvanced"
        Me.btnAdvanced.Size = New System.Drawing.Size(213, 26)
        Me.btnAdvanced.TabIndex = 1
        Me.btnAdvanced.Text = Global.AutomateUI.My.Resources.Resources.ctlChooseSummary_AdvancedConfig
        Me.btnAdvanced.UseVisualStyleBackColor = True
        '
        'ctlChooseSummary
        '
        Me.Font = DataPipelineOutputConfigUISettings.StandardFont
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.btnAdvanced)
        Me.Controls.Add(Me.txtSummary)
        Me.Name = "ctlChooseSummary"
        Me.NavigateNext = True
        Me.NavigatePrevious = True
        Me.Size = New System.Drawing.Size(785, 434)
        Me.Title = Global.AutomateUI.My.Resources.Resources.ctlChooseSummary_title
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents txtSummary As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents btnAdvanced As AutomateControls.Buttons.StandardStyledButton
End Class
