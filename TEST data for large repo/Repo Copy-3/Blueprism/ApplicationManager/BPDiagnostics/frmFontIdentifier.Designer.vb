<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmFontIdentifier
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmFontIdentifier))
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.progOverall = New System.Windows.Forms.ProgressBar()
        Me.lblOverall = New System.Windows.Forms.Label()
        Me.lblTimeInfo = New System.Windows.Forms.Label()
        Me.lblFontRepository = New System.Windows.Forms.Label()
        Me.txtTargetDir = New System.Windows.Forms.TextBox()
        Me.btnBrowseFontDirectory = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lblElapsedValue = New System.Windows.Forms.Label()
        Me.lblRemainingValue = New System.Windows.Forms.Label()
        Me.btnLoad = New System.Windows.Forms.Button()
        Me.lstFonts = New System.Windows.Forms.ListView()
        Me.chName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.chScore = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.chSize = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.chBold = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.chItalic = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.chUnderlined = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.lblChooseHint = New System.Windows.Forms.Label()
        Me.btnToggleBold = New System.Windows.Forms.Button()
        Me.btnToggleAllItalic = New System.Windows.Forms.Button()
        Me.btnToggleAllUnderlined = New System.Windows.Forms.Button()
        Me.lblToggleSize = New System.Windows.Forms.Label()
        Me.txtToggleSize = New System.Windows.Forms.TextBox()
        Me.btnToggleAllSize = New System.Windows.Forms.Button()
        Me.btnBrowseSampleImage = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.pbSampleImage = New System.Windows.Forms.PictureBox()
        Me.lblSampleHint = New System.Windows.Forms.Label()
        Me.txtReferenceText = New System.Windows.Forms.TextBox()
        Me.lblExpectedText = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.btnSelectAll = New System.Windows.Forms.Button()
        Me.btnSelectNone = New System.Windows.Forms.Button()
        Me.btnBegin = New System.Windows.Forms.Button()
        Me.lblCurrentFont = New System.Windows.Forms.Label()
        Me.gbScope = New System.Windows.Forms.GroupBox()
        Me.gbTestData = New System.Windows.Forms.GroupBox()
        Me.pbColour = New System.Windows.Forms.PictureBox()
        Me.txtColour = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.rdoMatchBackground = New AutomateControls.StyledRadioButton()
        Me.rdoMatchForeground = New AutomateControls.StyledRadioButton()
        Me.gbProgress = New System.Windows.Forms.GroupBox()
        CType(Me.pbSampleImage, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.gbScope.SuspendLayout()
        Me.gbTestData.SuspendLayout()
        CType(Me.pbColour, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.gbProgress.SuspendLayout()
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
        'lblTimeInfo
        '
        resources.ApplyResources(Me.lblTimeInfo, "lblTimeInfo")
        Me.lblTimeInfo.Name = "lblTimeInfo"
        '
        'lblFontRepository
        '
        resources.ApplyResources(Me.lblFontRepository, "lblFontRepository")
        Me.lblFontRepository.Name = "lblFontRepository"
        '
        'txtTargetDir
        '
        resources.ApplyResources(Me.txtTargetDir, "txtTargetDir")
        Me.txtTargetDir.Name = "txtTargetDir"
        '
        'btnBrowseFontDirectory
        '
        resources.ApplyResources(Me.btnBrowseFontDirectory, "btnBrowseFontDirectory")
        Me.btnBrowseFontDirectory.Name = "btnBrowseFontDirectory"
        Me.btnBrowseFontDirectory.UseVisualStyleBackColor = True
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
        'btnLoad
        '
        resources.ApplyResources(Me.btnLoad, "btnLoad")
        Me.btnLoad.Name = "btnLoad"
        Me.btnLoad.UseVisualStyleBackColor = True
        '
        'lstFonts
        '
        resources.ApplyResources(Me.lstFonts, "lstFonts")
        Me.lstFonts.CheckBoxes = True
        Me.lstFonts.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.chName, Me.chScore, Me.chSize, Me.chBold, Me.chItalic, Me.chUnderlined})
        Me.lstFonts.FullRowSelect = True
        Me.lstFonts.GridLines = True
        Me.lstFonts.HideSelection = False
        Me.lstFonts.Name = "lstFonts"
        Me.lstFonts.UseCompatibleStateImageBehavior = False
        Me.lstFonts.View = System.Windows.Forms.View.Details
        '
        'chName
        '
        Me.chName.Name = "chName"
        resources.ApplyResources(Me.chName, "chName")
        '
        'chScore
        '
        Me.chScore.Name = "chScore"
        resources.ApplyResources(Me.chScore, "chScore")
        '
        'chSize
        '
        Me.chSize.Name = "chSize"
        resources.ApplyResources(Me.chSize, "chSize")
        '
        'chBold
        '
        Me.chBold.Name = "chBold"
        resources.ApplyResources(Me.chBold, "chBold")
        '
        'chItalic
        '
        Me.chItalic.Name = "chItalic"
        resources.ApplyResources(Me.chItalic, "chItalic")
        '
        'chUnderlined
        '
        Me.chUnderlined.Name = "chUnderlined"
        resources.ApplyResources(Me.chUnderlined, "chUnderlined")
        '
        'lblChooseHint
        '
        resources.ApplyResources(Me.lblChooseHint, "lblChooseHint")
        Me.lblChooseHint.Name = "lblChooseHint"
        '
        'btnToggleBold
        '
        resources.ApplyResources(Me.btnToggleBold, "btnToggleBold")
        Me.btnToggleBold.Name = "btnToggleBold"
        Me.btnToggleBold.UseVisualStyleBackColor = True
        '
        'btnToggleAllItalic
        '
        resources.ApplyResources(Me.btnToggleAllItalic, "btnToggleAllItalic")
        Me.btnToggleAllItalic.Name = "btnToggleAllItalic"
        Me.btnToggleAllItalic.UseVisualStyleBackColor = True
        '
        'btnToggleAllUnderlined
        '
        resources.ApplyResources(Me.btnToggleAllUnderlined, "btnToggleAllUnderlined")
        Me.btnToggleAllUnderlined.Name = "btnToggleAllUnderlined"
        Me.btnToggleAllUnderlined.UseVisualStyleBackColor = True
        '
        'lblToggleSize
        '
        resources.ApplyResources(Me.lblToggleSize, "lblToggleSize")
        Me.lblToggleSize.Name = "lblToggleSize"
        '
        'txtToggleSize
        '
        resources.ApplyResources(Me.txtToggleSize, "txtToggleSize")
        Me.txtToggleSize.Name = "txtToggleSize"
        '
        'btnToggleAllSize
        '
        resources.ApplyResources(Me.btnToggleAllSize, "btnToggleAllSize")
        Me.btnToggleAllSize.Name = "btnToggleAllSize"
        Me.btnToggleAllSize.UseVisualStyleBackColor = True
        '
        'btnBrowseSampleImage
        '
        resources.ApplyResources(Me.btnBrowseSampleImage, "btnBrowseSampleImage")
        Me.btnBrowseSampleImage.Name = "btnBrowseSampleImage"
        Me.btnBrowseSampleImage.UseVisualStyleBackColor = True
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'pbSampleImage
        '
        resources.ApplyResources(Me.pbSampleImage, "pbSampleImage")
        Me.pbSampleImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pbSampleImage.Image = Global.BluePrism.ApplicationManager.My.Resources.Resources.SampleText
        Me.pbSampleImage.Name = "pbSampleImage"
        Me.pbSampleImage.TabStop = False
        '
        'lblSampleHint
        '
        resources.ApplyResources(Me.lblSampleHint, "lblSampleHint")
        Me.lblSampleHint.Name = "lblSampleHint"
        '
        'txtReferenceText
        '
        resources.ApplyResources(Me.txtReferenceText, "txtReferenceText")
        Me.txtReferenceText.Name = "txtReferenceText"
        '
        'lblExpectedText
        '
        resources.ApplyResources(Me.lblExpectedText, "lblExpectedText")
        Me.lblExpectedText.Name = "lblExpectedText"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'btnSelectAll
        '
        resources.ApplyResources(Me.btnSelectAll, "btnSelectAll")
        Me.btnSelectAll.Name = "btnSelectAll"
        Me.btnSelectAll.UseVisualStyleBackColor = True
        '
        'btnSelectNone
        '
        resources.ApplyResources(Me.btnSelectNone, "btnSelectNone")
        Me.btnSelectNone.Name = "btnSelectNone"
        Me.btnSelectNone.UseVisualStyleBackColor = True
        '
        'btnBegin
        '
        resources.ApplyResources(Me.btnBegin, "btnBegin")
        Me.btnBegin.Name = "btnBegin"
        Me.btnBegin.UseVisualStyleBackColor = True
        '
        'lblCurrentFont
        '
        resources.ApplyResources(Me.lblCurrentFont, "lblCurrentFont")
        Me.lblCurrentFont.Name = "lblCurrentFont"
        '
        'gbScope
        '
        Me.gbScope.Controls.Add(Me.btnSelectAll)
        Me.gbScope.Controls.Add(Me.btnSelectNone)
        Me.gbScope.Controls.Add(Me.btnToggleAllSize)
        Me.gbScope.Controls.Add(Me.txtToggleSize)
        Me.gbScope.Controls.Add(Me.lblToggleSize)
        Me.gbScope.Controls.Add(Me.btnToggleAllUnderlined)
        Me.gbScope.Controls.Add(Me.btnToggleAllItalic)
        Me.gbScope.Controls.Add(Me.btnToggleBold)
        Me.gbScope.Controls.Add(Me.lstFonts)
        Me.gbScope.Controls.Add(Me.btnLoad)
        Me.gbScope.Controls.Add(Me.btnBrowseFontDirectory)
        Me.gbScope.Controls.Add(Me.txtTargetDir)
        Me.gbScope.Controls.Add(Me.lblFontRepository)
        Me.gbScope.Controls.Add(Me.lblChooseHint)
        resources.ApplyResources(Me.gbScope, "gbScope")
        Me.gbScope.Name = "gbScope"
        Me.gbScope.TabStop = False
        '
        'gbTestData
        '
        Me.gbTestData.Controls.Add(Me.pbColour)
        Me.gbTestData.Controls.Add(Me.txtColour)
        Me.gbTestData.Controls.Add(Me.Label4)
        Me.gbTestData.Controls.Add(Me.rdoMatchBackground)
        Me.gbTestData.Controls.Add(Me.rdoMatchForeground)
        Me.gbTestData.Controls.Add(Me.Label3)
        Me.gbTestData.Controls.Add(Me.lblExpectedText)
        Me.gbTestData.Controls.Add(Me.txtReferenceText)
        Me.gbTestData.Controls.Add(Me.btnBrowseSampleImage)
        Me.gbTestData.Controls.Add(Me.lblSampleHint)
        Me.gbTestData.Controls.Add(Me.pbSampleImage)
        Me.gbTestData.Controls.Add(Me.Label2)
        resources.ApplyResources(Me.gbTestData, "gbTestData")
        Me.gbTestData.Name = "gbTestData"
        Me.gbTestData.TabStop = False
        '
        'pbColour
        '
        Me.pbColour.BackColor = System.Drawing.Color.White
        Me.pbColour.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        resources.ApplyResources(Me.pbColour, "pbColour")
        Me.pbColour.Name = "pbColour"
        Me.pbColour.TabStop = False
        '
        'txtColour
        '
        resources.ApplyResources(Me.txtColour, "txtColour")
        Me.txtColour.Name = "txtColour"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'rdoMatchBackground
        '
        resources.ApplyResources(Me.rdoMatchBackground, "rdoMatchBackground")
        Me.rdoMatchBackground.Name = "rdoMatchBackground"
        Me.rdoMatchBackground.UseVisualStyleBackColor = True
        '
        'rdoMatchForeground
        '
        resources.ApplyResources(Me.rdoMatchForeground, "rdoMatchForeground")
        Me.rdoMatchForeground.Checked = True
        Me.rdoMatchForeground.Name = "rdoMatchForeground"
        Me.rdoMatchForeground.TabStop = True
        Me.rdoMatchForeground.UseVisualStyleBackColor = True
        '
        'gbProgress
        '
        Me.gbProgress.Controls.Add(Me.lblCurrentFont)
        Me.gbProgress.Controls.Add(Me.btnBegin)
        Me.gbProgress.Controls.Add(Me.btnCancel)
        Me.gbProgress.Controls.Add(Me.lblRemainingValue)
        Me.gbProgress.Controls.Add(Me.lblElapsedValue)
        Me.gbProgress.Controls.Add(Me.Label1)
        Me.gbProgress.Controls.Add(Me.lblTimeInfo)
        Me.gbProgress.Controls.Add(Me.progOverall)
        Me.gbProgress.Controls.Add(Me.lblOverall)
        resources.ApplyResources(Me.gbProgress, "gbProgress")
        Me.gbProgress.Name = "gbProgress"
        Me.gbProgress.TabStop = False
        '
        'frmFontIdentifier
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.gbProgress)
        Me.Controls.Add(Me.gbScope)
        Me.Controls.Add(Me.gbTestData)
        Me.Name = "frmFontIdentifier"
        CType(Me.pbSampleImage, System.ComponentModel.ISupportInitialize).EndInit()
        Me.gbScope.ResumeLayout(False)
        Me.gbScope.PerformLayout()
        Me.gbTestData.ResumeLayout(False)
        Me.gbTestData.PerformLayout()
        CType(Me.pbColour, System.ComponentModel.ISupportInitialize).EndInit()
        Me.gbProgress.ResumeLayout(False)
        Me.gbProgress.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents progOverall As System.Windows.Forms.ProgressBar
    Friend WithEvents lblOverall As System.Windows.Forms.Label
    Friend WithEvents lblTimeInfo As System.Windows.Forms.Label
    Friend WithEvents lblFontRepository As System.Windows.Forms.Label
    Friend WithEvents txtTargetDir As System.Windows.Forms.TextBox
    Friend WithEvents btnBrowseFontDirectory As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lblElapsedValue As System.Windows.Forms.Label
    Friend WithEvents lblRemainingValue As System.Windows.Forms.Label
    Friend WithEvents btnLoad As System.Windows.Forms.Button
    Friend WithEvents lstFonts As System.Windows.Forms.ListView
    Friend WithEvents chName As System.Windows.Forms.ColumnHeader
    Friend WithEvents chSize As System.Windows.Forms.ColumnHeader
    Friend WithEvents chItalic As System.Windows.Forms.ColumnHeader
    Friend WithEvents chUnderlined As System.Windows.Forms.ColumnHeader
    Friend WithEvents chBold As System.Windows.Forms.ColumnHeader
    Friend WithEvents lblChooseHint As System.Windows.Forms.Label
    Friend WithEvents btnToggleBold As System.Windows.Forms.Button
    Friend WithEvents btnToggleAllItalic As System.Windows.Forms.Button
    Friend WithEvents btnToggleAllUnderlined As System.Windows.Forms.Button
    Friend WithEvents lblToggleSize As System.Windows.Forms.Label
    Friend WithEvents txtToggleSize As System.Windows.Forms.TextBox
    Friend WithEvents btnToggleAllSize As System.Windows.Forms.Button
    Friend WithEvents btnBrowseSampleImage As System.Windows.Forms.Button
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents pbSampleImage As System.Windows.Forms.PictureBox
    Friend WithEvents lblSampleHint As System.Windows.Forms.Label
    Friend WithEvents txtReferenceText As System.Windows.Forms.TextBox
    Friend WithEvents lblExpectedText As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents btnSelectAll As System.Windows.Forms.Button
    Friend WithEvents btnSelectNone As System.Windows.Forms.Button
    Friend WithEvents btnBegin As System.Windows.Forms.Button
    Friend WithEvents lblCurrentFont As System.Windows.Forms.Label
    Friend WithEvents gbScope As System.Windows.Forms.GroupBox
    Friend WithEvents gbTestData As System.Windows.Forms.GroupBox
    Friend WithEvents gbProgress As System.Windows.Forms.GroupBox
    Friend WithEvents rdoMatchBackground As AutomateControls.StyledRadioButton
    Friend WithEvents rdoMatchForeground As AutomateControls.StyledRadioButton
    Friend WithEvents txtColour As System.Windows.Forms.TextBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents pbColour As System.Windows.Forms.PictureBox
    Friend WithEvents chScore As System.Windows.Forms.ColumnHeader
End Class
