Imports AutomateControls.Wizard

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlChooseWebServiceName
    Inherits WizardPanel

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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlChooseWebServiceName))
        Me.lName = New System.Windows.Forms.Label()
        Me.txtName = New AutomateControls.Textboxes.StyledTextBox()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.lblNameInUse = New System.Windows.Forms.Label()
        Me.btnCorrect = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.SuspendLayout
        '
        'lName
        '
        resources.ApplyResources(Me.lName, "lName")
        Me.lName.Name = "lName"
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        Me.txtName.BorderColor = System.Drawing.Color.Empty
        Me.txtName.Name = "txtName"
        '
        'Timer1
        '
        Me.Timer1.Interval = 1000
        '
        'lblNameInUse
        '
        resources.ApplyResources(Me.lblNameInUse, "lblNameInUse")
        Me.lblNameInUse.Name = "lblNameInUse"
        '
        'btnCorrect
        '
        resources.ApplyResources(Me.btnCorrect, "btnCorrect")
        Me.btnCorrect.BackColor = System.Drawing.Color.White
        Me.btnCorrect.Name = "btnCorrect"
        Me.btnCorrect.UseVisualStyleBackColor = false
        '
        'ctlChooseWebServiceName
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.btnCorrect)
        Me.Controls.Add(Me.lblNameInUse)
        Me.Controls.Add(Me.lName)
        Me.Controls.Add(Me.txtName)
        Me.Name = "ctlChooseWebServiceName"
        Me.NavigatePrevious = true
        resources.ApplyResources(Me, "$this")
        Me.Title = Global.AutomateUI.My.Resources.Resources.ctlChooseWebServiceName_PleaseChooseANameToGiveToThisWebService
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Friend WithEvents lName As System.Windows.Forms.Label
    Friend WithEvents txtName As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents lblNameInUse As System.Windows.Forms.Label
    Friend WithEvents btnCorrect As AutomateControls.Buttons.StandardStyledButton

End Class
