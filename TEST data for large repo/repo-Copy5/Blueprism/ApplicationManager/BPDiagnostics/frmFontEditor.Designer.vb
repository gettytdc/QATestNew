<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmFontEditor
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmFontEditor))
        Me.btnCapture = New System.Windows.Forms.Button()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.pnlDetected = New System.Windows.Forms.Panel()
        Me.lblTextCol = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.btnLoad = New System.Windows.Forms.Button()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnNew = New System.Windows.Forms.Button()
        Me.btnAddToFont = New System.Windows.Forms.Button()
        Me.pnlFontData = New System.Windows.Forms.Panel()
        Me.btnZoomIn = New System.Windows.Forms.Button()
        Me.btnZoomOut = New System.Windows.Forms.Button()
        Me.lblCapZoom = New System.Windows.Forms.Label()
        Me.btnLoadBitmap = New System.Windows.Forms.Button()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.pbCaptured = New System.Windows.Forms.PictureBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.llSelectAll = New System.Windows.Forms.LinkLabel()
        Me.llSelectNone = New System.Windows.Forms.LinkLabel()
        Me.llClear = New System.Windows.Forms.LinkLabel()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.FontToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.GenerateFromInstalledFontToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AutogenerateAllAvailableFontsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AutomaticFontIdentifierToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.Panel1.SuspendLayout()
        CType(Me.pbCaptured, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.MenuStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnCapture
        '
        resources.ApplyResources(Me.btnCapture, "btnCapture")
        Me.btnCapture.Name = "btnCapture"
        Me.btnCapture.UseVisualStyleBackColor = True
        '
        'Label5
        '
        resources.ApplyResources(Me.Label5, "Label5")
        Me.Label5.Name = "Label5"
        '
        'pnlDetected
        '
        resources.ApplyResources(Me.pnlDetected, "pnlDetected")
        Me.pnlDetected.BackColor = System.Drawing.SystemColors.Control
        Me.pnlDetected.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlDetected.Name = "pnlDetected"
        '
        'lblTextCol
        '
        resources.ApplyResources(Me.lblTextCol, "lblTextCol")
        Me.lblTextCol.Name = "lblTextCol"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'btnLoad
        '
        resources.ApplyResources(Me.btnLoad, "btnLoad")
        Me.btnLoad.Name = "btnLoad"
        Me.btnLoad.UseVisualStyleBackColor = True
        '
        'btnSave
        '
        resources.ApplyResources(Me.btnSave, "btnSave")
        Me.btnSave.Name = "btnSave"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnNew
        '
        resources.ApplyResources(Me.btnNew, "btnNew")
        Me.btnNew.Name = "btnNew"
        Me.btnNew.UseVisualStyleBackColor = True
        '
        'btnAddToFont
        '
        resources.ApplyResources(Me.btnAddToFont, "btnAddToFont")
        Me.btnAddToFont.Name = "btnAddToFont"
        Me.ToolTip1.SetToolTip(Me.btnAddToFont, resources.GetString("btnAddToFont.ToolTip"))
        Me.btnAddToFont.UseVisualStyleBackColor = True
        '
        'pnlFontData
        '
        resources.ApplyResources(Me.pnlFontData, "pnlFontData")
        Me.pnlFontData.BackColor = System.Drawing.SystemColors.Control
        Me.pnlFontData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlFontData.Name = "pnlFontData"
        '
        'btnZoomIn
        '
        resources.ApplyResources(Me.btnZoomIn, "btnZoomIn")
        Me.btnZoomIn.Name = "btnZoomIn"
        Me.btnZoomIn.UseVisualStyleBackColor = True
        '
        'btnZoomOut
        '
        resources.ApplyResources(Me.btnZoomOut, "btnZoomOut")
        Me.btnZoomOut.Name = "btnZoomOut"
        Me.btnZoomOut.UseVisualStyleBackColor = True
        '
        'lblCapZoom
        '
        resources.ApplyResources(Me.lblCapZoom, "lblCapZoom")
        Me.lblCapZoom.Name = "lblCapZoom"
        '
        'btnLoadBitmap
        '
        resources.ApplyResources(Me.btnLoadBitmap, "btnLoadBitmap")
        Me.btnLoadBitmap.Name = "btnLoadBitmap"
        Me.btnLoadBitmap.UseVisualStyleBackColor = True
        '
        'Panel1
        '
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Controls.Add(Me.pbCaptured)
        Me.Panel1.Name = "Panel1"
        '
        'pbCaptured
        '
        Me.pbCaptured.BackColor = System.Drawing.SystemColors.Desktop
        resources.ApplyResources(Me.pbCaptured, "pbCaptured")
        Me.pbCaptured.Name = "pbCaptured"
        Me.pbCaptured.TabStop = False
        '
        'Button1
        '
        resources.ApplyResources(Me.Button1, "Button1")
        Me.Button1.Name = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'llSelectAll
        '
        resources.ApplyResources(Me.llSelectAll, "llSelectAll")
        Me.llSelectAll.Name = "llSelectAll"
        Me.llSelectAll.TabStop = True
        Me.ToolTip1.SetToolTip(Me.llSelectAll, resources.GetString("llSelectAll.ToolTip"))
        '
        'llSelectNone
        '
        resources.ApplyResources(Me.llSelectNone, "llSelectNone")
        Me.llSelectNone.Name = "llSelectNone"
        Me.llSelectNone.TabStop = True
        Me.ToolTip1.SetToolTip(Me.llSelectNone, resources.GetString("llSelectNone.ToolTip"))
        '
        'llClear
        '
        resources.ApplyResources(Me.llClear, "llClear")
        Me.llClear.Name = "llClear"
        Me.llClear.TabStop = True
        Me.ToolTip1.SetToolTip(Me.llClear, resources.GetString("llClear.ToolTip"))
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem, Me.FontToolStripMenuItem})
        resources.ApplyResources(Me.MenuStrip1, "MenuStrip1")
        Me.MenuStrip1.Name = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ExitToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        resources.ApplyResources(Me.FileToolStripMenuItem, "FileToolStripMenuItem")
        '
        'ExitToolStripMenuItem
        '
        Me.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        resources.ApplyResources(Me.ExitToolStripMenuItem, "ExitToolStripMenuItem")
        '
        'FontToolStripMenuItem
        '
        Me.FontToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.GenerateFromInstalledFontToolStripMenuItem, Me.AutogenerateAllAvailableFontsToolStripMenuItem, Me.AutomaticFontIdentifierToolStripMenuItem})
        Me.FontToolStripMenuItem.Name = "FontToolStripMenuItem"
        resources.ApplyResources(Me.FontToolStripMenuItem, "FontToolStripMenuItem")
        '
        'GenerateFromInstalledFontToolStripMenuItem
        '
        Me.GenerateFromInstalledFontToolStripMenuItem.Name = "GenerateFromInstalledFontToolStripMenuItem"
        resources.ApplyResources(Me.GenerateFromInstalledFontToolStripMenuItem, "GenerateFromInstalledFontToolStripMenuItem")
        '
        'AutogenerateAllAvailableFontsToolStripMenuItem
        '
        Me.AutogenerateAllAvailableFontsToolStripMenuItem.Name = "AutogenerateAllAvailableFontsToolStripMenuItem"
        resources.ApplyResources(Me.AutogenerateAllAvailableFontsToolStripMenuItem, "AutogenerateAllAvailableFontsToolStripMenuItem")
        '
        'AutomaticFontIdentifierToolStripMenuItem
        '
        Me.AutomaticFontIdentifierToolStripMenuItem.Name = "AutomaticFontIdentifierToolStripMenuItem"
        resources.ApplyResources(Me.AutomaticFontIdentifierToolStripMenuItem, "AutomaticFontIdentifierToolStripMenuItem")
        '
        'frmFontEditor
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.llClear)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.llSelectAll)
        Me.Controls.Add(Me.llSelectNone)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.btnLoadBitmap)
        Me.Controls.Add(Me.lblCapZoom)
        Me.Controls.Add(Me.btnZoomOut)
        Me.Controls.Add(Me.btnZoomIn)
        Me.Controls.Add(Me.pnlFontData)
        Me.Controls.Add(Me.btnAddToFont)
        Me.Controls.Add(Me.btnNew)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.btnLoad)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.lblTextCol)
        Me.Controls.Add(Me.pnlDetected)
        Me.Controls.Add(Me.btnCapture)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.MenuStrip1)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "frmFontEditor"
        Me.Panel1.ResumeLayout(False)
        CType(Me.pbCaptured, System.ComponentModel.ISupportInitialize).EndInit()
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnCapture As System.Windows.Forms.Button
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents pnlDetected As System.Windows.Forms.Panel
    Friend WithEvents lblTextCol As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents btnLoad As System.Windows.Forms.Button
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents btnNew As System.Windows.Forms.Button
    Friend WithEvents btnAddToFont As System.Windows.Forms.Button
    Friend WithEvents pnlFontData As System.Windows.Forms.Panel
    Friend WithEvents btnZoomIn As System.Windows.Forms.Button
    Friend WithEvents btnZoomOut As System.Windows.Forms.Button
    Friend WithEvents lblCapZoom As System.Windows.Forms.Label
    Friend WithEvents btnLoadBitmap As System.Windows.Forms.Button
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents pbCaptured As System.Windows.Forms.PictureBox
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents llSelectAll As System.Windows.Forms.LinkLabel
    Friend WithEvents llSelectNone As System.Windows.Forms.LinkLabel
    Friend WithEvents llClear As System.Windows.Forms.LinkLabel
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
    Friend WithEvents FileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ExitToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents FontToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents GenerateFromInstalledFontToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AutogenerateAllAvailableFontsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AutomaticFontIdentifierToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
End Class
