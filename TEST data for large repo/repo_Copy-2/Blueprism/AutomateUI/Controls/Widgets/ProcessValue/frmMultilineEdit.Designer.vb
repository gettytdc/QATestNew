<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMultilineEdit
    Inherits System.Windows.Forms.Form

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMultilineEdit))
        Me.txtTextEntry = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.cmbLang = New System.Windows.Forms.ComboBox()
        Me.ctlCode = New AutomateUI.ctlCodeEditor()
        Me.SuspendLayout()
        '
        'txtTextEntry
        '
        resources.ApplyResources(Me.txtTextEntry, "txtTextEntry")
        Me.txtTextEntry.Name = "txtTextEntry"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'cmbLang
        '
        Me.cmbLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbLang.FormattingEnabled = True
        resources.ApplyResources(Me.cmbLang, "cmbLang")
        Me.cmbLang.Name = "cmbLang"
        '
        'ctlCode
        '
        resources.ApplyResources(Me.ctlCode, "ctlCode")
        Me.ctlCode.Code = ""
        Me.ctlCode.Name = "ctlCode"
        Me.ctlCode.ReadOnly = False
        '
        'frmMultilineEdit
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.ctlCode)
        Me.Controls.Add(Me.cmbLang)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.txtTextEntry)
        Me.Name = "frmMultilineEdit"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtTextEntry As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents cmbLang As System.Windows.Forms.ComboBox
    Friend WithEvents ctlCode As AutomateUI.ctlCodeEditor
End Class
