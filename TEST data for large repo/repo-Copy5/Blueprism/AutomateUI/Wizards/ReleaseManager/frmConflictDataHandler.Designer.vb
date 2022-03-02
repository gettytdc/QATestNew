<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmConflictDataHandler
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
        Me.conflictDataHandler = New AutomateUI.ctlConflictDataHandler()
        Me.btnOk = New AutomateControls.Buttons.StandardStyledButton()
        Me.SuspendLayout()
        '
        'conflictDataHandler
        '
        Me.conflictDataHandler.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.conflictDataHandler.Conflict = Nothing
        Me.conflictDataHandler.ConflictDataHandler = Nothing
        Me.conflictDataHandler.ConflictOption = Nothing
        Me.conflictDataHandler.Location = New System.Drawing.Point(12, 12)
        Me.conflictDataHandler.Name = "conflictDataHandler"
        Me.conflictDataHandler.Size = New System.Drawing.Size(442, 110)
        Me.conflictDataHandler.TabIndex = 0
        '
        'btnOk
        '
        Me.btnOk.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnOk.Location = New System.Drawing.Point(379, 133)
        Me.btnOk.Name = "btnOk"
        Me.btnOk.Size = New System.Drawing.Size(75, 23)
        Me.btnOk.TabIndex = 1
        Me.btnOk.Text = "OK"
        Me.btnOk.UseVisualStyleBackColor = True
        '
        'frmConflictDataHandler
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(466, 168)
        Me.Controls.Add(Me.btnOk)
        Me.Controls.Add(Me.conflictDataHandler)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmConflictDataHandler"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "frmConflictDataHandler"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents conflictDataHandler As ctlConflictDataHandler
    Friend WithEvents btnOk As AutomateControls.Buttons.StandardStyledButton
End Class
