<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlFileOrDatabaseChooser
    Inherits AutomateUI.ctlWizardStageControl

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlFileOrDatabaseChooser))
        Me.rdoDatabase = New AutomateControls.StyledRadioButton()
        Me.rdoFile = New AutomateControls.StyledRadioButton()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'rdoDatabase
        '
        resources.ApplyResources(Me.rdoDatabase, "rdoDatabase")
        Me.rdoDatabase.Checked = True
        Me.rdoDatabase.Name = "rdoDatabase"
        Me.rdoDatabase.TabStop = True
        Me.rdoDatabase.UseVisualStyleBackColor = True
        '
        'rdoFile
        '
        resources.ApplyResources(Me.rdoFile, "rdoFile")
        Me.rdoFile.Name = "rdoFile"
        Me.rdoFile.UseVisualStyleBackColor = True
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'ctlFileOrDatabaseChooser
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.rdoFile)
        Me.Controls.Add(Me.rdoDatabase)
        Me.Name = "ctlFileOrDatabaseChooser"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents rdoDatabase As AutomateControls.StyledRadioButton
    Friend WithEvents rdoFile As AutomateControls.StyledRadioButton
    Friend WithEvents Label1 As System.Windows.Forms.Label

End Class
