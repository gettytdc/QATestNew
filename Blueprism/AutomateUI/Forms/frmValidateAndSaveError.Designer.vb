<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmValidateAndSaveError
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmValidateAndSaveError))
        Me.btnProcessValidation = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnDiscardOrUnpublish = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnReturnToStudio = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.lblBody = New System.Windows.Forms.Label()
        Me.BlueBar = New AutomateControls.TitleBar()
        Me.SuspendLayout()
        '
        'btnProcessValidation
        '
        resources.ApplyResources(Me.btnProcessValidation, "btnProcessValidation")
        Me.btnProcessValidation.Image = Global.AutomateUI.My.Resources.ToolImages.Notebook_Tick_16x16
        Me.btnProcessValidation.Name = "btnProcessValidation"
        Me.btnProcessValidation.UseVisualStyleBackColor = False
        '
        'btnDiscardOrUnpublish
        '
        resources.ApplyResources(Me.btnDiscardOrUnpublish, "btnDiscardOrUnpublish")
        Me.btnDiscardOrUnpublish.Name = "btnDiscardOrUnpublish"
        Me.btnDiscardOrUnpublish.UseVisualStyleBackColor = False
        '
        'btnReturnToStudio
        '
        resources.ApplyResources(Me.btnReturnToStudio, "btnReturnToStudio")
        Me.btnReturnToStudio.Name = "btnReturnToStudio"
        Me.btnReturnToStudio.UseVisualStyleBackColor = False
        '
        'lblBody
        '
        resources.ApplyResources(Me.lblBody, "lblBody")
        Me.lblBody.Name = "lblBody"
        '
        'BlueBar
        '
        resources.ApplyResources(Me.BlueBar, "BlueBar")
        Me.BlueBar.Name = "BlueBar"
        Me.BlueBar.TabStop = False
        '
        'frmValidateAndSaveError
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.lblBody)
        Me.Controls.Add(Me.btnProcessValidation)
        Me.Controls.Add(Me.btnDiscardOrUnpublish)
        Me.Controls.Add(Me.btnReturnToStudio)
        Me.Controls.Add(Me.BlueBar)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmValidateAndSaveError"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents BlueBar As AutomateControls.TitleBar
    Friend WithEvents btnProcessValidation As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnDiscardOrUnpublish As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lblBody As System.Windows.Forms.Label
    Friend WithEvents btnReturnToStudio As AutomateControls.Buttons.StandardStyledButton
End Class
