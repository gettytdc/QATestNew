<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlProcessImage
    Inherits UserControl

    'Control overrides dispose to clean up the component list.
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

    'Required by the Control Designer
    Private components As System.ComponentModel.IContainer
    Friend WithEvents btnExport As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnImport As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnView As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnClear As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents txtInfo As AutomateControls.Textboxes.StyledTextBox

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlProcessImage))
        Me.txtInfo = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnExport = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnImport = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnView = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnClear = New AutomateControls.Buttons.StandardStyledButton()
        Me.SuspendLayout()
        '
        'txtInfo
        '
        resources.ApplyResources(Me.txtInfo, "txtInfo")
        Me.txtInfo.Name = "txtInfo"
        Me.txtInfo.ReadOnly = True
        '
        'btnExport
        '
        resources.ApplyResources(Me.btnExport, "btnExport")
        Me.btnExport.Name = "btnExport"
        Me.btnExport.UseVisualStyleBackColor = True
        '
        'btnImport
        '
        resources.ApplyResources(Me.btnImport, "btnImport")
        Me.btnImport.Name = "btnImport"
        Me.btnImport.UseVisualStyleBackColor = True
        '
        'btnView
        '
        resources.ApplyResources(Me.btnView, "btnView")
        Me.btnView.Name = "btnView"
        Me.btnView.UseVisualStyleBackColor = True
        '
        'btnClear
        '
        resources.ApplyResources(Me.btnClear, "btnClear")
        Me.btnClear.Name = "btnClear"
        Me.btnClear.UseVisualStyleBackColor = True
        '
        'ctlProcessImage
        '
        Me.Controls.Add(Me.txtInfo)
        Me.Controls.Add(Me.btnClear)
        Me.Controls.Add(Me.btnView)
        Me.Controls.Add(Me.btnImport)
        Me.Controls.Add(Me.btnExport)
        Me.Name = "ctlProcessImage"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)
        Me.PerformLayout()

End Sub


End Class

