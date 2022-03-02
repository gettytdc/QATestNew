<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ButtonWithTick
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ButtonWithTick))
        Me.btnButton = New AutomateControls.Buttons.StandardStyledButton()
        Me.pbTick = New System.Windows.Forms.PictureBox()
        CType(Me.pbTick, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btnButton
        '
        Me.btnButton.ForeColor = System.Drawing.Color.Black
        resources.ApplyResources(Me.btnButton, "btnButton")
        Me.btnButton.Name = "btnButton"
        Me.btnButton.UseVisualStyleBackColor = True
        '
        'pbTick
        '
        Me.pbTick.Image = Global.AutomateUI.My.Resources.ToolImages.Tick_16x16
        resources.ApplyResources(Me.pbTick, "pbTick")
        Me.pbTick.Name = "pbTick"
        Me.pbTick.TabStop = False
        '
        'ButtonWithTick
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.pbTick)
        Me.Controls.Add(Me.btnButton)
        Me.Name = "ButtonWithTick"
        CType(Me.pbTick, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btnButton As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents pbTick As System.Windows.Forms.PictureBox

End Class
