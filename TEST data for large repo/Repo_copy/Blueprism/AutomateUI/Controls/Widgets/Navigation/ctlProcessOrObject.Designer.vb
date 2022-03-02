<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlProcessOrObject
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlProcessOrObject))
        Me.pbObject = New System.Windows.Forms.PictureBox()
        Me.pbProcesses = New System.Windows.Forms.PictureBox()
        Me.rdoObject = New AutomateControls.StyledRadioButton()
        Me.rdoProcess = New AutomateControls.StyledRadioButton()
        CType(Me.pbObject, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.pbProcesses, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'pbObject
        '
        Me.pbObject.Image = Global.AutomateUI.My.Resources.ComponentImages.Class_32x32
        resources.ApplyResources(Me.pbObject, "pbObject")
        Me.pbObject.Name = "pbObject"
        Me.pbObject.TabStop = False
        '
        'pbProcesses
        '
        Me.pbProcesses.Image = Global.AutomateUI.My.Resources.ComponentImages.Procedure_32x32
        resources.ApplyResources(Me.pbProcesses, "pbProcesses")
        Me.pbProcesses.Name = "pbProcesses"
        Me.pbProcesses.TabStop = False
        '
        'rdoObject
        '
        resources.ApplyResources(Me.rdoObject, "rdoObject")
        Me.rdoObject.ForeColor = System.Drawing.Color.SteelBlue
        Me.rdoObject.Name = "rdoObject"
        Me.rdoObject.UseVisualStyleBackColor = True
        '
        'rdoProcess
        '
        resources.ApplyResources(Me.rdoProcess, "rdoProcess")
        Me.rdoProcess.Checked = True
        Me.rdoProcess.ForeColor = System.Drawing.Color.SteelBlue
        Me.rdoProcess.Name = "rdoProcess"
        Me.rdoProcess.TabStop = True
        Me.rdoProcess.UseVisualStyleBackColor = True
        '
        'ctlProcessOrObject
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.rdoProcess)
        Me.Controls.Add(Me.rdoObject)
        Me.Controls.Add(Me.pbObject)
        Me.Controls.Add(Me.pbProcesses)
        Me.Name = "ctlProcessOrObject"
        CType(Me.pbObject, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.pbProcesses, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents pbObject As System.Windows.Forms.PictureBox
    Friend WithEvents pbProcesses As System.Windows.Forms.PictureBox
    Friend WithEvents rdoObject As AutomateControls.StyledRadioButton
    Friend WithEvents rdoProcess As AutomateControls.StyledRadioButton

End Class
