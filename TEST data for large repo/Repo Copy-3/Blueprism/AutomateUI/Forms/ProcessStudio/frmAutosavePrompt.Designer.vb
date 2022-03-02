<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAutosavePrompt
    Inherits frmWizard

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAutosavePrompt))
        Me.pnlRecover = New System.Windows.Forms.Panel()
        Me.lblPrompt = New System.Windows.Forms.Label()
        Me.lblInitialMessage = New System.Windows.Forms.Label()
        Me.lblComparisonHint = New System.Windows.Forms.Label()
        Me.rdoCompare = New AutomateControls.StyledRadioButton()
        Me.lblIgnoreUnsavedHints = New System.Windows.Forms.Label()
        Me.lblOpenUnsavedHints = New System.Windows.Forms.Label()
        Me.rdoOpenOriginalVersion = New AutomateControls.StyledRadioButton()
        Me.rdoOpenUnsavedVersion = New AutomateControls.StyledRadioButton()
        Me.pnlUnlock = New System.Windows.Forms.Panel()
        Me.rdoUnlockProcess = New AutomateControls.StyledRadioButton()
        Me.rdoViewProcess = New AutomateControls.StyledRadioButton()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.pnlRecover.SuspendLayout()
        Me.pnlUnlock.SuspendLayout()
        Me.SuspendLayout()
        '
        'objBluebar
        '
        resources.ApplyResources(Me.objBluebar, "objBluebar")
        '
        'pnlRecover
        '
        resources.ApplyResources(Me.pnlRecover, "pnlRecover")
        Me.pnlRecover.Controls.Add(Me.lblPrompt)
        Me.pnlRecover.Controls.Add(Me.lblInitialMessage)
        Me.pnlRecover.Controls.Add(Me.lblComparisonHint)
        Me.pnlRecover.Controls.Add(Me.rdoCompare)
        Me.pnlRecover.Controls.Add(Me.lblIgnoreUnsavedHints)
        Me.pnlRecover.Controls.Add(Me.lblOpenUnsavedHints)
        Me.pnlRecover.Controls.Add(Me.rdoOpenOriginalVersion)
        Me.pnlRecover.Controls.Add(Me.rdoOpenUnsavedVersion)
        Me.pnlRecover.Name = "pnlRecover"
        '
        'lblPrompt
        '
        resources.ApplyResources(Me.lblPrompt, "lblPrompt")
        Me.lblPrompt.Name = "lblPrompt"
        '
        'lblInitialMessage
        '
        resources.ApplyResources(Me.lblInitialMessage, "lblInitialMessage")
        Me.lblInitialMessage.Name = "lblInitialMessage"
        '
        'lblComparisonHint
        '
        resources.ApplyResources(Me.lblComparisonHint, "lblComparisonHint")
        Me.lblComparisonHint.Name = "lblComparisonHint"
        '
        'rdoCompare
        '
        resources.ApplyResources(Me.rdoCompare, "rdoCompare")
        Me.rdoCompare.ButtonHeight = 21
        Me.rdoCompare.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.rdoCompare.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.rdoCompare.FocusDiameter = 16
        Me.rdoCompare.FocusThickness = 3
        Me.rdoCompare.FocusYLocation = 9
        Me.rdoCompare.ForceFocus = True
        Me.rdoCompare.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.rdoCompare.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.rdoCompare.MouseLeaveColor = System.Drawing.Color.White
        Me.rdoCompare.Name = "rdoCompare"
        Me.rdoCompare.RadioButtonDiameter = 12
        Me.rdoCompare.RadioButtonThickness = 2
        Me.rdoCompare.RadioYLocation = 7
        Me.rdoCompare.StringYLocation = 1
        Me.rdoCompare.TabStop = True
        Me.rdoCompare.TextColor = System.Drawing.Color.Black
        Me.rdoCompare.UseVisualStyleBackColor = True
        '
        'lblIgnoreUnsavedHints
        '
        resources.ApplyResources(Me.lblIgnoreUnsavedHints, "lblIgnoreUnsavedHints")
        Me.lblIgnoreUnsavedHints.Name = "lblIgnoreUnsavedHints"
        '
        'lblOpenUnsavedHints
        '
        resources.ApplyResources(Me.lblOpenUnsavedHints, "lblOpenUnsavedHints")
        Me.lblOpenUnsavedHints.Name = "lblOpenUnsavedHints"
        '
        'rdoOpenOriginalVersion
        '
        resources.ApplyResources(Me.rdoOpenOriginalVersion, "rdoOpenOriginalVersion")
        Me.rdoOpenOriginalVersion.ButtonHeight = 21
        Me.rdoOpenOriginalVersion.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.rdoOpenOriginalVersion.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.rdoOpenOriginalVersion.FocusDiameter = 16
        Me.rdoOpenOriginalVersion.FocusThickness = 3
        Me.rdoOpenOriginalVersion.FocusYLocation = 9
        Me.rdoOpenOriginalVersion.ForceFocus = True
        Me.rdoOpenOriginalVersion.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.rdoOpenOriginalVersion.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.rdoOpenOriginalVersion.MouseLeaveColor = System.Drawing.Color.White
        Me.rdoOpenOriginalVersion.Name = "rdoOpenOriginalVersion"
        Me.rdoOpenOriginalVersion.RadioButtonDiameter = 12
        Me.rdoOpenOriginalVersion.RadioButtonThickness = 2
        Me.rdoOpenOriginalVersion.RadioYLocation = 7
        Me.rdoOpenOriginalVersion.StringYLocation = 1
        Me.rdoOpenOriginalVersion.TabStop = True
        Me.rdoOpenOriginalVersion.TextColor = System.Drawing.Color.Black
        Me.rdoOpenOriginalVersion.UseVisualStyleBackColor = True
        '
        'rdoOpenUnsavedVersion
        '
        resources.ApplyResources(Me.rdoOpenUnsavedVersion, "rdoOpenUnsavedVersion")
        Me.rdoOpenUnsavedVersion.ButtonHeight = 21
        Me.rdoOpenUnsavedVersion.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.rdoOpenUnsavedVersion.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.rdoOpenUnsavedVersion.FocusDiameter = 16
        Me.rdoOpenUnsavedVersion.FocusThickness = 3
        Me.rdoOpenUnsavedVersion.FocusYLocation = 9
        Me.rdoOpenUnsavedVersion.ForceFocus = True
        Me.rdoOpenUnsavedVersion.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.rdoOpenUnsavedVersion.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.rdoOpenUnsavedVersion.MouseLeaveColor = System.Drawing.Color.White
        Me.rdoOpenUnsavedVersion.Name = "rdoOpenUnsavedVersion"
        Me.rdoOpenUnsavedVersion.RadioButtonDiameter = 12
        Me.rdoOpenUnsavedVersion.RadioButtonThickness = 2
        Me.rdoOpenUnsavedVersion.RadioYLocation = 7
        Me.rdoOpenUnsavedVersion.StringYLocation = 1
        Me.rdoOpenUnsavedVersion.TabStop = True
        Me.rdoOpenUnsavedVersion.TextColor = System.Drawing.Color.Black
        Me.rdoOpenUnsavedVersion.UseVisualStyleBackColor = True
        '
        'pnlUnlock
        '
        Me.pnlUnlock.Controls.Add(Me.rdoUnlockProcess)
        Me.pnlUnlock.Controls.Add(Me.rdoViewProcess)
        Me.pnlUnlock.Controls.Add(Me.Label1)
        Me.pnlUnlock.Controls.Add(Me.Label2)
        Me.pnlUnlock.Controls.Add(Me.Label3)
        resources.ApplyResources(Me.pnlUnlock, "pnlUnlock")
        Me.pnlUnlock.Name = "pnlUnlock"
        '
        'rdoUnlockProcess
        '
        resources.ApplyResources(Me.rdoUnlockProcess, "rdoUnlockProcess")
        Me.rdoUnlockProcess.ButtonHeight = 21
        Me.rdoUnlockProcess.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.rdoUnlockProcess.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.rdoUnlockProcess.FocusDiameter = 16
        Me.rdoUnlockProcess.FocusThickness = 3
        Me.rdoUnlockProcess.FocusYLocation = 9
        Me.rdoUnlockProcess.ForceFocus = True
        Me.rdoUnlockProcess.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.rdoUnlockProcess.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.rdoUnlockProcess.MouseLeaveColor = System.Drawing.Color.White
        Me.rdoUnlockProcess.Name = "rdoUnlockProcess"
        Me.rdoUnlockProcess.RadioButtonDiameter = 12
        Me.rdoUnlockProcess.RadioButtonThickness = 2
        Me.rdoUnlockProcess.RadioYLocation = 7
        Me.rdoUnlockProcess.StringYLocation = 1
        Me.rdoUnlockProcess.TabStop = True
        Me.rdoUnlockProcess.TextColor = System.Drawing.Color.Black
        Me.rdoUnlockProcess.UseVisualStyleBackColor = True
        '
        'rdoViewProcess
        '
        resources.ApplyResources(Me.rdoViewProcess, "rdoViewProcess")
        Me.rdoViewProcess.ButtonHeight = 21
        Me.rdoViewProcess.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.rdoViewProcess.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.rdoViewProcess.FocusDiameter = 16
        Me.rdoViewProcess.FocusThickness = 3
        Me.rdoViewProcess.FocusYLocation = 9
        Me.rdoViewProcess.ForceFocus = True
        Me.rdoViewProcess.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.rdoViewProcess.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.rdoViewProcess.MouseLeaveColor = System.Drawing.Color.White
        Me.rdoViewProcess.Name = "rdoViewProcess"
        Me.rdoViewProcess.RadioButtonDiameter = 12
        Me.rdoViewProcess.RadioButtonThickness = 2
        Me.rdoViewProcess.RadioYLocation = 7
        Me.rdoViewProcess.StringYLocation = 1
        Me.rdoViewProcess.TabStop = True
        Me.rdoViewProcess.TextColor = System.Drawing.Color.Black
        Me.rdoViewProcess.UseVisualStyleBackColor = True
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'frmAutosavePrompt
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.pnlRecover)
        Me.Controls.Add(Me.pnlUnlock)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Name = "frmAutosavePrompt"
        Me.Title = "Recover unsaved work"
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.Controls.SetChildIndex(Me.btnBack, 0)
        Me.Controls.SetChildIndex(Me.btnNext, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.Controls.SetChildIndex(Me.pnlUnlock, 0)
        Me.Controls.SetChildIndex(Me.pnlRecover, 0)
        Me.pnlRecover.ResumeLayout(False)
        Me.pnlRecover.PerformLayout()
        Me.pnlUnlock.ResumeLayout(False)
        Me.pnlUnlock.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents pnlRecover As System.Windows.Forms.Panel
    Friend WithEvents lblIgnoreUnsavedHints As System.Windows.Forms.Label
    Friend WithEvents lblOpenUnsavedHints As System.Windows.Forms.Label
    Friend WithEvents rdoOpenOriginalVersion As AutomateControls.StyledRadioButton
    Friend WithEvents lblPrompt As System.Windows.Forms.Label
    Friend WithEvents rdoOpenUnsavedVersion As AutomateControls.StyledRadioButton
    Friend WithEvents lblComparisonHint As System.Windows.Forms.Label
    Friend WithEvents rdoCompare As AutomateControls.StyledRadioButton
    Friend WithEvents pnlUnlock As System.Windows.Forms.Panel
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents rdoUnlockProcess As AutomateControls.StyledRadioButton
    Friend WithEvents rdoViewProcess As AutomateControls.StyledRadioButton
    Friend WithEvents lblInitialMessage As System.Windows.Forms.Label
End Class
