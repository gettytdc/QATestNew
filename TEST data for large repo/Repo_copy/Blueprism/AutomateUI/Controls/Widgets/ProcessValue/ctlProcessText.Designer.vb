Partial Public Class ctlProcessText
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlProcessText))
        Me.btnEditButton = New AutomateControls.Buttons.StandardStyledButton()
        Me.txtValue = New AutomateUI.MultilineTextBox()
        Me.SuspendLayout()
        '
        'btnEditButton
        '
        resources.ApplyResources(Me.btnEditButton, "btnEditButton")
        Me.btnEditButton.Name = "btnEditButton"
        '
        'txtValue
        '
        Me.txtValue.AcceptsReturn = True
        resources.ApplyResources(Me.txtValue, "txtValue")
        Me.txtValue.Name = "txtValue"
        '
        'ctlProcessText
        '
        Me.Controls.Add(Me.txtValue)
        Me.Controls.Add(Me.btnEditButton)
        Me.Name = "ctlProcessText"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents txtValue As MultilineTextBox
    Private WithEvents btnEditButton As AutomateControls.Buttons.StandardStyledButton
End Class
