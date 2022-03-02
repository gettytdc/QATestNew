<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmDateTimeChooser
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmDateTimeChooser))
        Me.dateChooser = New System.Windows.Forms.DateTimePicker()
        Me.panButtons = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.timeChooser = New AutomateUI.ctlTimeChooser()
        Me.cultureDateChooser = New CustomControls.DatePicker()
        Me.panButtons.SuspendLayout()
        Me.SuspendLayout()
        '
        'dateChooser
        '
        resources.ApplyResources(Me.dateChooser, "dateChooser")
        Me.dateChooser.Name = "dateChooser"
        '
        'panButtons
        '
        resources.ApplyResources(Me.panButtons, "panButtons")
        Me.panButtons.Controls.Add(Me.btnCancel)
        Me.panButtons.Controls.Add(Me.btnOK)
        Me.panButtons.Name = "panButtons"
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnOK.Name = "btnOK"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'timeChooser
        '
        resources.ApplyResources(Me.timeChooser, "timeChooser")
        Me.timeChooser.DefaultTime = System.TimeSpan.Parse("15:51:20.0025564")
        Me.timeChooser.Name = "timeChooser"
        '
        'DatePicker1
        '
        resources.ApplyResources(Me.cultureDateChooser, "DatePicker1")
        Me.cultureDateChooser.Name = "DatePicker1"
        '
        'frmDateTimeChooser
        '
        Me.AcceptButton = Me.btnOK
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.CancelButton = Me.btnCancel
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.cultureDateChooser)
        Me.Controls.Add(Me.panButtons)
        Me.Controls.Add(Me.dateChooser)
        Me.Controls.Add(Me.timeChooser)
        Me.Name = "frmDateTimeChooser"
        Me.panButtons.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents timeChooser As AutomateUI.ctlTimeChooser
    Private WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents panButtons As System.Windows.Forms.FlowLayoutPanel
    Private WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents dateChooser As System.Windows.Forms.DateTimePicker
    Friend WithEvents cultureDateChooser As CustomControls.DatePicker
End Class
