<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlTimeChooser
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlTimeChooser))
        Me.btnAMPM = New AutomateControls.Buttons.StandardStyledButton()
        Me.txtTime = New System.Windows.Forms.MaskedTextBox()
        Me.PBClock = New System.Windows.Forms.PictureBox()
        CType(Me.PBClock, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btnAMPM
        '
        resources.ApplyResources(Me.btnAMPM, "btnAMPM")
        Me.btnAMPM.Name = "btnAMPM"
        '
        'txtTime
        '
        resources.ApplyResources(Me.txtTime, "txtTime")
        Me.txtTime.BackColor = System.Drawing.Color.White
        Me.txtTime.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtTime.CutCopyMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals
        Me.txtTime.InsertKeyMode = System.Windows.Forms.InsertKeyMode.Overwrite
        Me.txtTime.Name = "txtTime"
        Me.txtTime.SkipLiterals = False
        Me.txtTime.TextMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals
        '
        'PBClock
        '
        resources.ApplyResources(Me.PBClock, "PBClock")
        Me.PBClock.BackColor = System.Drawing.SystemColors.Control
        Me.PBClock.Name = "PBClock"
        Me.PBClock.TabStop = False
        '
        'ctlTimeChooser
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.btnAMPM)
        Me.Controls.Add(Me.txtTime)
        Me.Controls.Add(Me.PBClock)
        Me.Name = "ctlTimeChooser"
        resources.ApplyResources(Me, "$this")
        CType(Me.PBClock, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnAMPM As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents txtTime As System.Windows.Forms.MaskedTextBox
    Friend WithEvents PBClock As System.Windows.Forms.PictureBox

End Class
