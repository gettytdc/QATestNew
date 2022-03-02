<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlSystemFonts
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSystemFonts))
        Me.fontMgr = New BluePrism.CharMatching.UI.FontManager()
        Me.SuspendLayout()
        '
        'fontMgr
        '
        Me.fontMgr.BackColor = System.Drawing.SystemColors.ControlLightLight
        resources.ApplyResources(Me.fontMgr, "fontMgr")
        Me.fontMgr.Name = "fontMgr"
        Me.fontMgr.Store = Nothing
        '
        'ctlSystemFonts
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.fontMgr)
        Me.Name = "ctlSystemFonts"
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents fontMgr As BluePrism.CharMatching.UI.FontManager

End Class
