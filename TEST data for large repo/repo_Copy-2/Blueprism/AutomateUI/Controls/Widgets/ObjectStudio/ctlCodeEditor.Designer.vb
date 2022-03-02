<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlCodeEditor
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlCodeEditor))
        Me.mEditor = New ScintillaNET.Scintilla()
        CType(Me.mEditor, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'mEditor
        '
        resources.ApplyResources(Me.mEditor, "mEditor")
        Me.mEditor.Name = "mEditor"
        Me.mEditor.Styles.BraceBad.FontName = "Verdana" & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0)
        Me.mEditor.Styles.BraceLight.FontName = "Verdana" & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0)
        Me.mEditor.Styles.CallTip.FontName = "Segoe UI" & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0)
        Me.mEditor.Styles.ControlChar.FontName = "Verdana" & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0)
        Me.mEditor.Styles.Default.FontName = "Verdana" & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0)
        Me.mEditor.Styles.IndentGuide.FontName = "Verdana" & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0)
        Me.mEditor.Styles.LastPredefined.FontName = "Verdana" & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0)
        Me.mEditor.Styles.LineNumber.FontName = "Verdana" & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0)
        Me.mEditor.Styles.Max.FontName = "Verdana" & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0) & char.ConvertFromUtf32(0)
        '
        'ctlCodeEditor
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.mEditor)
        Me.Name = "ctlCodeEditor"
        CType(Me.mEditor, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents mEditor As ScintillaNet.Scintilla

End Class
