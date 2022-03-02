<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class WebApiForm
    Inherits AutomateControls.Forms.HelpButtonForm

    'Form overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(WebApiForm))
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnOk = New AutomateControls.Buttons.StandardStyledButton()
        Me.ctlTitleBar = New AutomateControls.TitleBar()
        Me.pnlBottomStrip = New System.Windows.Forms.Panel()
        Me.AssociatedToSkillWarningLabel = New System.Windows.Forms.Label()
        Me.apiManager = New AutomateUI.WebApiManager()
        Me.pnlBottomStrip.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.CausesValidation = False
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'btnOk
        '
        resources.ApplyResources(Me.btnOk, "btnOk")
        Me.btnOk.Name = "btnOk"
        Me.btnOk.UseVisualStyleBackColor = True
        '
        'ctlTitleBar
        '
        resources.ApplyResources(Me.ctlTitleBar, "ctlTitleBar")
        Me.ctlTitleBar.Name = "ctlTitleBar"
        '
        'pnlBottomStrip
        '
        Me.pnlBottomStrip.CausesValidation = False
        Me.pnlBottomStrip.Controls.Add(Me.AssociatedToSkillWarningLabel)
        Me.pnlBottomStrip.Controls.Add(Me.btnOk)
        Me.pnlBottomStrip.Controls.Add(Me.btnCancel)
        resources.ApplyResources(Me.pnlBottomStrip, "pnlBottomStrip")
        Me.pnlBottomStrip.Name = "pnlBottomStrip"
        '
        'AssociatedToSkillWarningLabel
        '
        resources.ApplyResources(Me.AssociatedToSkillWarningLabel, "AssociatedToSkillWarningLabel")
        Me.AssociatedToSkillWarningLabel.Name = "AssociatedToSkillWarningLabel"
        '
        'apiManager
        '
        resources.ApplyResources(Me.apiManager, "apiManager")
        Me.apiManager.Name = "apiManager"
        '
        'WebApiForm
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.CancelButton = Me.btnCancel
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.pnlBottomStrip)
        Me.Controls.Add(Me.ctlTitleBar)
        Me.Controls.Add(Me.apiManager)
        Me.HelpButton = True
        Me.Name = "WebApiForm"
        Me.pnlBottomStrip.ResumeLayout(False)
        Me.pnlBottomStrip.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents apiManager As WebApiManager
    Private WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents btnOk As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents ctlTitleBar As AutomateControls.TitleBar
    Friend WithEvents pnlBottomStrip As Panel
    Friend WithEvents AssociatedToSkillWarningLabel As Label
End Class
