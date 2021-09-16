<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmLoading
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
        Dim mLoadingGraphic As System.Windows.Forms.PictureBox
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmLoading))
        mLoadingGraphic = New System.Windows.Forms.PictureBox()
        CType(mLoadingGraphic,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'mLoadingGraphic
        '
        resources.ApplyResources(mLoadingGraphic, "mLoadingGraphic")
        mLoadingGraphic.Name = "mLoadingGraphic"
        mLoadingGraphic.TabStop = false
        '
        'frmLoading
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me, "$this")
        Me.ControlBox = false
        Me.Controls.Add(mLoadingGraphic)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.MaximizeBox = false
        Me.MinimizeBox = false
        Me.Name = "frmLoading"
        Me.ShowIcon = false
        Me.ShowInTaskbar = false
        CType(mLoadingGraphic,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)

End Sub
End Class
