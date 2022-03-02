

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmApplicationDefinitionCreate
    Inherits frmWizard

    'Form overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmApplicationDefinitionCreate))
        Me.pnlApplicationType = New System.Windows.Forms.Panel()
        Me.flowAppType = New AutomateControls.FullWidthFlowLayoutScrollPane()
        Me.lblAppType = New System.Windows.Forms.Label()
        Me.pnlConclusion = New System.Windows.Forms.Panel()
        Me.lblConclusion = New System.Windows.Forms.Label()
        Me.pnlApplicationName = New System.Windows.Forms.Panel()
        Me.lblParentObject = New System.Windows.Forms.Label()
        Me.rbNone = New AutomateControls.StyledRadioButton()
        Me.cmbParentObject = New System.Windows.Forms.ComboBox()
        Me.txtAppName = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblAppName = New System.Windows.Forms.Label()
        Me.rbShared = New AutomateControls.StyledRadioButton()
        Me.rbNew = New AutomateControls.StyledRadioButton()
        Me.pnlApplicationType.SuspendLayout()
        Me.pnlConclusion.SuspendLayout()
        Me.pnlApplicationName.SuspendLayout()
        Me.SuspendLayout()
        '
        'objBluebar
        '
        Me.objBluebar.Title = "Describe the application that is automated by this business object"
        '
        'pnlApplicationType
        '
        Me.pnlApplicationType.Controls.Add(Me.flowAppType)
        Me.pnlApplicationType.Controls.Add(Me.lblAppType)
        resources.ApplyResources(Me.pnlApplicationType, "pnlApplicationType")
        Me.pnlApplicationType.Name = "pnlApplicationType"
        '
        'flowAppType
        '
        resources.ApplyResources(Me.flowAppType, "flowAppType")
        Me.flowAppType.Name = "flowAppType"
        '
        'lblAppType
        '
        resources.ApplyResources(Me.lblAppType, "lblAppType")
        Me.lblAppType.Name = "lblAppType"
        '
        'pnlConclusion
        '
        Me.pnlConclusion.BackColor = System.Drawing.SystemColors.Control
        Me.pnlConclusion.Controls.Add(Me.lblConclusion)
        resources.ApplyResources(Me.pnlConclusion, "pnlConclusion")
        Me.pnlConclusion.Name = "pnlConclusion"
        '
        'lblConclusion
        '
        resources.ApplyResources(Me.lblConclusion, "lblConclusion")
        Me.lblConclusion.BackColor = System.Drawing.SystemColors.Control
        Me.lblConclusion.Name = "lblConclusion"
        '
        'pnlApplicationName
        '
        Me.pnlApplicationName.Controls.Add(Me.lblParentObject)
        Me.pnlApplicationName.Controls.Add(Me.rbNone)
        Me.pnlApplicationName.Controls.Add(Me.cmbParentObject)
        Me.pnlApplicationName.Controls.Add(Me.txtAppName)
        Me.pnlApplicationName.Controls.Add(Me.lblAppName)
        Me.pnlApplicationName.Controls.Add(Me.rbShared)
        Me.pnlApplicationName.Controls.Add(Me.rbNew)
        resources.ApplyResources(Me.pnlApplicationName, "pnlApplicationName")
        Me.pnlApplicationName.Name = "pnlApplicationName"
        '
        'lblParentObject
        '
        resources.ApplyResources(Me.lblParentObject, "lblParentObject")
        Me.lblParentObject.Name = "lblParentObject"
        '
        'rbNone
        '
        resources.ApplyResources(Me.rbNone, "rbNone")
        Me.rbNone.Name = "rbNone"
        Me.rbNone.UseVisualStyleBackColor = True
        '
        'cmbParentObject
        '
        Me.cmbParentObject.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        resources.ApplyResources(Me.cmbParentObject, "cmbParentObject")
        Me.cmbParentObject.FormattingEnabled = True
        Me.cmbParentObject.Name = "cmbParentObject"
        Me.cmbParentObject.Sorted = True
        '
        'txtAppName
        '
        resources.ApplyResources(Me.txtAppName, "txtAppName")
        Me.txtAppName.Name = "txtAppName"
        '
        'lblAppName
        '
        resources.ApplyResources(Me.lblAppName, "lblAppName")
        Me.lblAppName.Name = "lblAppName"
        '
        'rbShared
        '
        resources.ApplyResources(Me.rbShared, "rbShared")
        Me.rbShared.Name = "rbShared"
        Me.rbShared.UseVisualStyleBackColor = True
        '
        'rbNew
        '
        resources.ApplyResources(Me.rbNew, "rbNew")
        Me.rbNew.Name = "rbNew"
        Me.rbNew.UseVisualStyleBackColor = True
        '
        'frmApplicationDefinitionCreate
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.pnlApplicationType)
        Me.Controls.Add(Me.pnlApplicationName)
        Me.Controls.Add(Me.pnlConclusion)
        Me.Name = "frmApplicationDefinitionCreate"
        Me.Title = "Describe the application that is automated by this business object"
        Me.Controls.SetChildIndex(Me.pnlConclusion, 0)
        Me.Controls.SetChildIndex(Me.pnlApplicationName, 0)
        Me.Controls.SetChildIndex(Me.pnlApplicationType, 0)
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.Controls.SetChildIndex(Me.btnBack, 0)
        Me.Controls.SetChildIndex(Me.btnNext, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.pnlApplicationType.ResumeLayout(False)
        Me.pnlApplicationType.PerformLayout()
        Me.pnlConclusion.ResumeLayout(False)
        Me.pnlApplicationName.ResumeLayout(False)
        Me.pnlApplicationName.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents pnlApplicationType As System.Windows.Forms.Panel
    Friend WithEvents lblAppType As System.Windows.Forms.Label
    Friend WithEvents pnlConclusion As System.Windows.Forms.Panel
    Friend WithEvents lblConclusion As System.Windows.Forms.Label
    Friend WithEvents pnlApplicationName As System.Windows.Forms.Panel
    Friend WithEvents txtAppName As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblAppName As System.Windows.Forms.Label
    Private WithEvents flowAppType As AutomateControls.FullWidthFlowLayoutScrollPane
    Friend WithEvents rbNone As AutomateControls.StyledRadioButton
    Friend WithEvents rbShared As AutomateControls.StyledRadioButton
    Friend WithEvents rbNew As AutomateControls.StyledRadioButton
    Friend WithEvents cmbParentObject As System.Windows.Forms.ComboBox
    Friend WithEvents lblParentObject As System.Windows.Forms.Label
End Class
