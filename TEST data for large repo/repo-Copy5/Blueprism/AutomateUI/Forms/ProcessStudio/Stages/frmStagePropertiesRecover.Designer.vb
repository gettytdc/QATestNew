<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmStagePropertiesRecover
    Inherits AutomateUI.frmProperties

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesRecover))
        Me.chkLimitAttempts = New System.Windows.Forms.CheckBox()
        Me.numMaxAttempts = New AutomateControls.StyledNumericUpDown()
        Me.lblMaxAttempts = New System.Windows.Forms.Label()
        CType(Me.numMaxAttempts, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'mTitleBar
        '
        resources.ApplyResources(Me.mTitleBar, "mTitleBar")
        '
        'chkLimitAttempts
        '
        resources.ApplyResources(Me.chkLimitAttempts, "chkLimitAttempts")
        Me.chkLimitAttempts.Name = "chkLimitAttempts"
        Me.chkLimitAttempts.UseVisualStyleBackColor = True
        '
        'numMaxAttempts
        '
        resources.ApplyResources(Me.numMaxAttempts, "numMaxAttempts")
        Me.numMaxAttempts.Name = "numMaxAttempts"
        '
        'lblMaxAttempts
        '
        resources.ApplyResources(Me.lblMaxAttempts, "lblMaxAttempts")
        Me.lblMaxAttempts.Name = "lblMaxAttempts"
        '
        'frmStagePropertiesRecover
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.lblMaxAttempts)
        Me.Controls.Add(Me.numMaxAttempts)
        Me.Controls.Add(Me.chkLimitAttempts)
        Me.Name = "frmStagePropertiesRecover"
        Me.Controls.SetChildIndex(Me.mTitleBar, 0)
        Me.Controls.SetChildIndex(Me.txtName, 0)
        Me.Controls.SetChildIndex(Me.txtDescription, 0)
        Me.Controls.SetChildIndex(Me.chkLimitAttempts, 0)
        Me.Controls.SetChildIndex(Me.numMaxAttempts, 0)
        Me.Controls.SetChildIndex(Me.lblMaxAttempts, 0)
        CType(Me.numMaxAttempts, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents chkLimitAttempts As CheckBox
    Friend WithEvents numMaxAttempts As AutomateControls.StyledNumericUpDown
    Friend WithEvents lblMaxAttempts As Label
End Class
