<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlPathFinder
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlPathFinder))
        Me.lblHintText = New System.Windows.Forms.Label()
        Me.txtFile = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnBrowse = New AutomateControls.Buttons.StandardStyledButton()
        Me.SuspendLayout()
        '
        'lblHintText
        '
        resources.ApplyResources(Me.lblHintText, "lblHintText")
        Me.lblHintText.Name = "lblHintText"
        '
        'txtFile
        '
        resources.ApplyResources(Me.txtFile, "txtFile")
        Me.txtFile.Name = "txtFile"
        '
        'btnBrowse
        '
        resources.ApplyResources(Me.btnBrowse, "btnBrowse")
        Me.btnBrowse.Name = "btnBrowse"
        '
        'ctlPathFinder
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.txtFile)
        Me.Controls.Add(Me.lblHintText)
        Me.Controls.Add(Me.btnBrowse)
        Me.Name = "ctlPathFinder"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblHintText As System.Windows.Forms.Label
    Friend WithEvents btnBrowse As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents txtFile As AutomateControls.Textboxes.StyledTextBox

End Class
