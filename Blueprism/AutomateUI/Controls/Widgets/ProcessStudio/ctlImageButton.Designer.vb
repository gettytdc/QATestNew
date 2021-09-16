<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlRolloverButton
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlRolloverButton))
        Me.pbButtonImage = New System.Windows.Forms.PictureBox()
        Me.mToolTip = New System.Windows.Forms.ToolTip(Me.components)
        CType(Me.pbButtonImage, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'pbButtonImage
        '
        Me.pbButtonImage.BackColor = System.Drawing.Color.Transparent
        resources.ApplyResources(Me.pbButtonImage, "pbButtonImage")
        Me.pbButtonImage.Name = "pbButtonImage"
        Me.pbButtonImage.TabStop = False
        '
        'ctlRolloverButton
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.pbButtonImage)
        Me.Name = "ctlRolloverButton"
        CType(Me.pbButtonImage, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents pbButtonImage As System.Windows.Forms.PictureBox
    Friend WithEvents mToolTip As System.Windows.Forms.ToolTip

End Class
