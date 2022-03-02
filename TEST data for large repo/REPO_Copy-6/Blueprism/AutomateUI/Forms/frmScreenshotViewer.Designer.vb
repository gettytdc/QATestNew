<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmScreenshotViewer

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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmScreenshotViewer))
        Me.ControlContextMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.SaveToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ScrollPanel = New System.Windows.Forms.Panel()
        Me.LoadingProgessBar = New System.Windows.Forms.ProgressBar()
        Me.ScreenshotPictureBox = New System.Windows.Forms.PictureBox()
        Me.ScreenshotLoader = New System.ComponentModel.BackgroundWorker()
        Me.SaveFileDialog = New System.Windows.Forms.SaveFileDialog()
        Me.StatusStrip = New System.Windows.Forms.StatusStrip()
        Me.ResourceLabel = New System.Windows.Forms.ToolStripStatusLabel()
        Me.LocalTimeLabel = New System.Windows.Forms.ToolStripStatusLabel()
        Me.UTCTimeLabel = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ProcessLabel = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ControlContextMenu.SuspendLayout()
        Me.ScrollPanel.SuspendLayout()
        CType(Me.ScreenshotPictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.StatusStrip.SuspendLayout()
        Me.SuspendLayout()
        '
        'ControlContextMenu
        '
        Me.ControlContextMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SaveToolStripMenuItem})
        Me.ControlContextMenu.Name = "ContextMenuStrip1"
        resources.ApplyResources(Me.ControlContextMenu, "ControlContextMenu")
        '
        'SaveToolStripMenuItem
        '
        Me.SaveToolStripMenuItem.Name = "SaveToolStripMenuItem"
        resources.ApplyResources(Me.SaveToolStripMenuItem, "SaveToolStripMenuItem")
        '
        'ScrollPanel
        '
        resources.ApplyResources(Me.ScrollPanel, "ScrollPanel")
        Me.ScrollPanel.BackColor = System.Drawing.Color.White
        Me.ScrollPanel.Controls.Add(Me.LoadingProgessBar)
        Me.ScrollPanel.Controls.Add(Me.ScreenshotPictureBox)
        Me.ScrollPanel.Name = "ScrollPanel"
        '
        'LoadingProgessBar
        '
        resources.ApplyResources(Me.LoadingProgessBar, "LoadingProgessBar")
        Me.LoadingProgessBar.Name = "LoadingProgessBar"
        '
        'ScreenshotPictureBox
        '
        Me.ScreenshotPictureBox.ContextMenuStrip = Me.ControlContextMenu
        resources.ApplyResources(Me.ScreenshotPictureBox, "ScreenshotPictureBox")
        Me.ScreenshotPictureBox.Name = "ScreenshotPictureBox"
        Me.ScreenshotPictureBox.TabStop = False
        '
        'ScreenshotLoader
        '
        Me.ScreenshotLoader.WorkerReportsProgress = True
        '
        'SaveFileDialog
        '
        Me.SaveFileDialog.DefaultExt = "png"
        resources.ApplyResources(Me.SaveFileDialog, "SaveFileDialog")
        '
        'StatusStrip
        '
        Me.StatusStrip.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.StatusStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ResourceLabel, Me.LocalTimeLabel, Me.UTCTimeLabel, Me.ProcessLabel})
        resources.ApplyResources(Me.StatusStrip, "StatusStrip")
        Me.StatusStrip.Name = "StatusStrip"
        '
        'ResourceLabel
        '
        Me.ResourceLabel.Name = "ResourceLabel"
        resources.ApplyResources(Me.ResourceLabel, "ResourceLabel")
        '
        'LocalTimeLabel
        '
        Me.LocalTimeLabel.Name = "LocalTimeLabel"
        resources.ApplyResources(Me.LocalTimeLabel, "LocalTimeLabel")
        '
        'UTCTimeLabel
        '
        Me.UTCTimeLabel.Name = "UTCTimeLabel"
        resources.ApplyResources(Me.UTCTimeLabel, "UTCTimeLabel")
        '
        'ProcessLabel
        '
        Me.ProcessLabel.Name = "ProcessLabel"
        resources.ApplyResources(Me.ProcessLabel, "ProcessLabel")
        '
        'frmScreenshotViewer
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.ScrollPanel)
        Me.Controls.Add(Me.StatusStrip)
        Me.Name = "frmScreenshotViewer"
        Me.ControlContextMenu.ResumeLayout(False)
        Me.ScrollPanel.ResumeLayout(False)
        Me.ScrollPanel.PerformLayout()
        CType(Me.ScreenshotPictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.StatusStrip.ResumeLayout(False)
        Me.StatusStrip.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ControlContextMenu As ContextMenuStrip
    Friend WithEvents SaveToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ScrollPanel As Panel
    Friend WithEvents ScreenshotPictureBox As PictureBox
    Friend WithEvents ScreenshotLoader As BackgroundWorker
    Friend WithEvents LoadingProgessBar As ProgressBar
    Friend WithEvents SaveFileDialog As SaveFileDialog
    Friend WithEvents StatusStrip As StatusStrip
    Friend WithEvents ResourceLabel As ToolStripStatusLabel
    Friend WithEvents LocalTimeLabel As ToolStripStatusLabel
    Friend WithEvents UTCTimeLabel As ToolStripStatusLabel
    Friend WithEvents ProcessLabel As ToolStripStatusLabel
End Class
