<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAutoFontsProgress
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAutoFontsProgress))
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.progOverall = New System.Windows.Forms.ProgressBar()
        Me.lblOverall = New System.Windows.Forms.Label()
        Me.lblCurrentFamily = New System.Windows.Forms.Label()
        Me.progCurrentFamily = New System.Windows.Forms.ProgressBar()
        Me.lblCurrentFontFamily = New System.Windows.Forms.Label()
        Me.lblCurrentPreciseFont = New System.Windows.Forms.Label()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.lblTimeInfo = New System.Windows.Forms.Label()
        Me.chkNonSymbolFontsOnly = New System.Windows.Forms.CheckBox()
        Me.lblScope = New System.Windows.Forms.Label()
        Me.btnBegin = New System.Windows.Forms.Button()
        Me.txtTargetDir = New System.Windows.Forms.TextBox()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtMinSize = New System.Windows.Forms.TextBox()
        Me.lblMinSize = New System.Windows.Forms.Label()
        Me.txtMaxSize = New System.Windows.Forms.TextBox()
        Me.lblMaxSize = New System.Windows.Forms.Label()
        Me.txtStepSize = New System.Windows.Forms.TextBox()
        Me.lblStepSize = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lblElapsedValue = New System.Windows.Forms.Label()
        Me.lblRemainingValue = New System.Windows.Forms.Label()
        Me.progCurrentFont = New System.Windows.Forms.ProgressBar()
        Me.lblCurrentFont = New System.Windows.Forms.Label()
        Me.lblOptions = New System.Windows.Forms.Label()
        Me.chkRegenerateExistingOnly = New System.Windows.Forms.CheckBox()
        Me.chkSkipExisting = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout()
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'progOverall
        '
        resources.ApplyResources(Me.progOverall, "progOverall")
        Me.progOverall.Name = "progOverall"
        '
        'lblOverall
        '
        resources.ApplyResources(Me.lblOverall, "lblOverall")
        Me.lblOverall.Name = "lblOverall"
        '
        'lblCurrentFamily
        '
        resources.ApplyResources(Me.lblCurrentFamily, "lblCurrentFamily")
        Me.lblCurrentFamily.Name = "lblCurrentFamily"
        '
        'progCurrentFamily
        '
        resources.ApplyResources(Me.progCurrentFamily, "progCurrentFamily")
        Me.progCurrentFamily.Name = "progCurrentFamily"
        '
        'lblCurrentFontFamily
        '
        resources.ApplyResources(Me.lblCurrentFontFamily, "lblCurrentFontFamily")
        Me.lblCurrentFontFamily.Name = "lblCurrentFontFamily"
        '
        'lblCurrentPreciseFont
        '
        resources.ApplyResources(Me.lblCurrentPreciseFont, "lblCurrentPreciseFont")
        Me.lblCurrentPreciseFont.Name = "lblCurrentPreciseFont"
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'lblTimeInfo
        '
        resources.ApplyResources(Me.lblTimeInfo, "lblTimeInfo")
        Me.lblTimeInfo.Name = "lblTimeInfo"
        '
        'chkNonSymbolFontsOnly
        '
        resources.ApplyResources(Me.chkNonSymbolFontsOnly, "chkNonSymbolFontsOnly")
        Me.chkNonSymbolFontsOnly.Checked = True
        Me.chkNonSymbolFontsOnly.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkNonSymbolFontsOnly.Name = "chkNonSymbolFontsOnly"
        Me.chkNonSymbolFontsOnly.UseVisualStyleBackColor = True
        '
        'lblScope
        '
        resources.ApplyResources(Me.lblScope, "lblScope")
        Me.lblScope.Name = "lblScope"
        '
        'btnBegin
        '
        resources.ApplyResources(Me.btnBegin, "btnBegin")
        Me.btnBegin.Name = "btnBegin"
        Me.btnBegin.UseVisualStyleBackColor = True
        '
        'txtTargetDir
        '
        resources.ApplyResources(Me.txtTargetDir, "txtTargetDir")
        Me.txtTargetDir.Name = "txtTargetDir"
        '
        'btnBrowse
        '
        resources.ApplyResources(Me.btnBrowse, "btnBrowse")
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtMinSize
        '
        resources.ApplyResources(Me.txtMinSize, "txtMinSize")
        Me.txtMinSize.Name = "txtMinSize"
        '
        'lblMinSize
        '
        resources.ApplyResources(Me.lblMinSize, "lblMinSize")
        Me.lblMinSize.Name = "lblMinSize"
        '
        'txtMaxSize
        '
        resources.ApplyResources(Me.txtMaxSize, "txtMaxSize")
        Me.txtMaxSize.Name = "txtMaxSize"
        '
        'lblMaxSize
        '
        resources.ApplyResources(Me.lblMaxSize, "lblMaxSize")
        Me.lblMaxSize.Name = "lblMaxSize"
        '
        'txtStepSize
        '
        resources.ApplyResources(Me.txtStepSize, "txtStepSize")
        Me.txtStepSize.Name = "txtStepSize"
        '
        'lblStepSize
        '
        resources.ApplyResources(Me.lblStepSize, "lblStepSize")
        Me.lblStepSize.Name = "lblStepSize"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'lblElapsedValue
        '
        resources.ApplyResources(Me.lblElapsedValue, "lblElapsedValue")
        Me.lblElapsedValue.Name = "lblElapsedValue"
        '
        'lblRemainingValue
        '
        resources.ApplyResources(Me.lblRemainingValue, "lblRemainingValue")
        Me.lblRemainingValue.Name = "lblRemainingValue"
        '
        'progCurrentFont
        '
        resources.ApplyResources(Me.progCurrentFont, "progCurrentFont")
        Me.progCurrentFont.Name = "progCurrentFont"
        '
        'lblCurrentFont
        '
        resources.ApplyResources(Me.lblCurrentFont, "lblCurrentFont")
        Me.lblCurrentFont.Name = "lblCurrentFont"
        '
        'lblOptions
        '
        resources.ApplyResources(Me.lblOptions, "lblOptions")
        Me.lblOptions.Name = "lblOptions"
        '
        'chkRegenerateExistingOnly
        '
        resources.ApplyResources(Me.chkRegenerateExistingOnly, "chkRegenerateExistingOnly")
        Me.chkRegenerateExistingOnly.Name = "chkRegenerateExistingOnly"
        Me.chkRegenerateExistingOnly.UseVisualStyleBackColor = True
        '
        'chkSkipExisting
        '
        resources.ApplyResources(Me.chkSkipExisting, "chkSkipExisting")
        Me.chkSkipExisting.Checked = True
        Me.chkSkipExisting.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkSkipExisting.Name = "chkSkipExisting"
        Me.chkSkipExisting.UseVisualStyleBackColor = True
        '
        'frmAutoFontsProgress
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.chkSkipExisting)
        Me.Controls.Add(Me.chkRegenerateExistingOnly)
        Me.Controls.Add(Me.lblOptions)
        Me.Controls.Add(Me.progCurrentFont)
        Me.Controls.Add(Me.lblCurrentFont)
        Me.Controls.Add(Me.lblRemainingValue)
        Me.Controls.Add(Me.lblElapsedValue)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtStepSize)
        Me.Controls.Add(Me.lblStepSize)
        Me.Controls.Add(Me.txtMaxSize)
        Me.Controls.Add(Me.lblMaxSize)
        Me.Controls.Add(Me.txtMinSize)
        Me.Controls.Add(Me.lblMinSize)
        Me.Controls.Add(Me.btnBrowse)
        Me.Controls.Add(Me.txtTargetDir)
        Me.Controls.Add(Me.btnBegin)
        Me.Controls.Add(Me.lblScope)
        Me.Controls.Add(Me.chkNonSymbolFontsOnly)
        Me.Controls.Add(Me.lblTimeInfo)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.progCurrentFamily)
        Me.Controls.Add(Me.progOverall)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.lblCurrentPreciseFont)
        Me.Controls.Add(Me.lblCurrentFontFamily)
        Me.Controls.Add(Me.lblCurrentFamily)
        Me.Controls.Add(Me.lblOverall)
        Me.Name = "frmAutoFontsProgress"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents btnCancel As System.Windows.Forms.Button
    Private WithEvents progOverall As System.Windows.Forms.ProgressBar
    Private WithEvents lblOverall As System.Windows.Forms.Label
    Private WithEvents lblCurrentFamily As System.Windows.Forms.Label
    Private WithEvents progCurrentFamily As System.Windows.Forms.ProgressBar
    Private WithEvents lblCurrentFontFamily As System.Windows.Forms.Label
    Private WithEvents lblCurrentPreciseFont As System.Windows.Forms.Label
    Private WithEvents btnOK As System.Windows.Forms.Button
    Private WithEvents lblTimeInfo As System.Windows.Forms.Label
    Private WithEvents chkNonSymbolFontsOnly As System.Windows.Forms.CheckBox
    Private WithEvents lblScope As System.Windows.Forms.Label
    Private WithEvents btnBegin As System.Windows.Forms.Button
    Private WithEvents lblOutputDir As System.Windows.Forms.Label
    Private WithEvents txtTargetDir As System.Windows.Forms.TextBox
    Private WithEvents btnBrowse As System.Windows.Forms.Button
    Private WithEvents txtMinSize As System.Windows.Forms.TextBox
    Private WithEvents lblMinSize As System.Windows.Forms.Label
    Private WithEvents txtMaxSize As System.Windows.Forms.TextBox
    Private WithEvents lblMaxSize As System.Windows.Forms.Label
    Private WithEvents txtStepSize As System.Windows.Forms.TextBox
    Private WithEvents lblStepSize As System.Windows.Forms.Label
    Private WithEvents Label1 As System.Windows.Forms.Label
    Private WithEvents lblElapsedValue As System.Windows.Forms.Label
    Private WithEvents lblRemainingValue As System.Windows.Forms.Label
    Private WithEvents progCurrentFont As System.Windows.Forms.ProgressBar
    Private WithEvents lblCurrentFont As System.Windows.Forms.Label
    Private WithEvents lblOptions As System.Windows.Forms.Label
    Private WithEvents chkRegenerateExistingOnly As System.Windows.Forms.CheckBox
    Private WithEvents chkSkipExisting As System.Windows.Forms.CheckBox
End Class
