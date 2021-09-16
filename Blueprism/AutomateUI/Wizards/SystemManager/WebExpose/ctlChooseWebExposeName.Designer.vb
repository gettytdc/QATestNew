Imports AutomateControls.Wizard

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlChooseWebExposeName
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlChooseWebExposeName))
        Me.lblName = New System.Windows.Forms.Label()
        Me.txtExposeName = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnCorrect = New AutomateControls.Buttons.StandardStyledButton()
        Me.lblInvalid = New System.Windows.Forms.Label()
        Me.forceSoapDocumentCheckbox = New System.Windows.Forms.CheckBox()
        Me.UseLegacyNamespaceCheckbox = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout()
        '
        'lblName
        '
        resources.ApplyResources(Me.lblName, "lblName")
        Me.lblName.Name = "lblName"
        '
        'txtExposeName
        '
        resources.ApplyResources(Me.txtExposeName, "txtExposeName")
        Me.txtExposeName.Name = "txtExposeName"
        '
        'btnCorrect
        '
        resources.ApplyResources(Me.btnCorrect, "btnCorrect")
        Me.btnCorrect.CausesValidation = False
        Me.btnCorrect.Name = "btnCorrect"
        Me.btnCorrect.UseVisualStyleBackColor = True
        '
        'lblInvalid
        '
        resources.ApplyResources(Me.lblInvalid, "lblInvalid")
        Me.lblInvalid.Name = "lblInvalid"
        '
        'forceSoapDocumentCheckbox
        '
        resources.ApplyResources(Me.forceSoapDocumentCheckbox, "forceSoapDocumentCheckbox")
        Me.forceSoapDocumentCheckbox.Name = "forceSoapDocumentCheckbox"
        Me.forceSoapDocumentCheckbox.UseVisualStyleBackColor = True
        '
        'UseLegacyNamespaceCheckbox
        '
        resources.ApplyResources(Me.UseLegacyNamespaceCheckbox, "UseLegacyNamespaceCheckbox")
        Me.UseLegacyNamespaceCheckbox.Name = "UseLegacyNamespaceCheckbox"
        Me.UseLegacyNamespaceCheckbox.UseVisualStyleBackColor = True
        '
        'ctlChooseWebExposeName
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.UseLegacyNamespaceCheckbox)
        Me.Controls.Add(Me.forceSoapDocumentCheckbox)
        Me.Controls.Add(Me.lblInvalid)
        Me.Controls.Add(Me.btnCorrect)
        Me.Controls.Add(Me.txtExposeName)
        Me.Controls.Add(Me.lblName)
        Me.Name = "ctlChooseWebExposeName"
        Me.NavigateNext = True
        Me.NavigatePrevious = True
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblName As System.Windows.Forms.Label
    Friend WithEvents txtExposeName As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents btnCorrect As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lblInvalid As System.Windows.Forms.Label
    Friend WithEvents forceSoapDocumentCheckbox As CheckBox
    Friend WithEvents UseLegacyNamespaceCheckbox As CheckBox
End Class
