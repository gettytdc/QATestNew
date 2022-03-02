<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmSnapshot
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSnapshot))
        Me.btnOk = New System.Windows.Forms.Button()
        Me.txtSnapshot = New System.Windows.Forms.RichTextBox()
        Me.btnCopyToClipboard = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'btnOk
        '
        resources.ApplyResources(Me.btnOk, "btnOk")
        Me.btnOk.Name = "btnOk"
        Me.btnOk.UseVisualStyleBackColor = True
        '
        'txtSnapshot
        '
        resources.ApplyResources(Me.txtSnapshot, "txtSnapshot")
        Me.txtSnapshot.Name = "txtSnapshot"
        '
        'btnCopyToClipboard
        '
        resources.ApplyResources(Me.btnCopyToClipboard, "btnCopyToClipboard")
        Me.btnCopyToClipboard.Name = "btnCopyToClipboard"
        Me.btnCopyToClipboard.UseVisualStyleBackColor = True
        '
        'frmSnapshot
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.btnCopyToClipboard)
        Me.Controls.Add(Me.txtSnapshot)
        Me.Controls.Add(Me.btnOk)
        Me.Name = "frmSnapshot"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btnOk As System.Windows.Forms.Button
    Friend WithEvents txtSnapshot As System.Windows.Forms.RichTextBox
    Friend WithEvents btnCopyToClipboard As System.Windows.Forms.Button
End Class
