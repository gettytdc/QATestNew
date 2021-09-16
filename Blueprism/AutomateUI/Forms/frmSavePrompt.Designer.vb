<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmSavePrompt
    Inherits frmForm

    'Form overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSavePrompt))
        Me.lblUserPrompt = New System.Windows.Forms.Label()
        Me.lblSummary = New System.Windows.Forms.Label()
        Me.txtSummary = New AutomateControls.Textboxes.StyledTextBox()
        Me.panFlow = New System.Windows.Forms.FlowLayoutPanel()
        Me.panTable = New System.Windows.Forms.TableLayoutPanel()
        Me.mHeading = New AutomateControls.TitleBar()
        Me.panTable.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblUserPrompt
        '
        resources.ApplyResources(Me.lblUserPrompt, "lblUserPrompt")
        Me.lblUserPrompt.Name = "lblUserPrompt"
        '
        'lblSummary
        '
        resources.ApplyResources(Me.lblSummary, "lblSummary")
        Me.lblSummary.Name = "lblSummary"
        '
        'txtSummary
        '
        Me.txtSummary.AcceptsReturn = True
        resources.ApplyResources(Me.txtSummary, "txtSummary")
        Me.txtSummary.Name = "txtSummary"
        '
        'panFlow
        '
        resources.ApplyResources(Me.panFlow, "panFlow")
        Me.panFlow.Name = "panFlow"
        '
        'panTable
        '
        resources.ApplyResources(Me.panTable, "panTable")
        Me.panTable.Controls.Add(Me.panFlow, 1, 0)
        Me.panTable.Name = "panTable"
        '
        'mHeading
        '
        resources.ApplyResources(Me.mHeading, "mHeading")
        Me.mHeading.Name = "mHeading"
        Me.mHeading.TabStop = False
        Me.mHeading.Title = "Title"
        '
        'frmSavePrompt
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.panTable)
        Me.Controls.Add(Me.txtSummary)
        Me.Controls.Add(Me.lblSummary)
        Me.Controls.Add(Me.lblUserPrompt)
        Me.Controls.Add(Me.mHeading)
        Me.Name = "frmSavePrompt"
        Me.panTable.ResumeLayout(False)
        Me.panTable.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents mHeading As AutomateControls.TitleBar
    Friend WithEvents lblUserPrompt As System.Windows.Forms.Label
    Friend WithEvents lblSummary As System.Windows.Forms.Label
    Friend WithEvents txtSummary As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents panFlow As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents panTable As System.Windows.Forms.TableLayoutPanel
End Class
