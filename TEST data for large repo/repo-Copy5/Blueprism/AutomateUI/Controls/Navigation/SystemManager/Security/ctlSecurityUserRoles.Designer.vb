<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlSecurityUserRoles
    Inherits System.Windows.Forms.UserControl

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSecurityUserRoles))
        Me.btnApply = New AutomateControls.Buttons.StandardStyledButton()
        Me.ctlRoles = New AutomateUI.ctlAuth()
        Me.SuspendLayout()
        '
        'btnApply
        '
        resources.ApplyResources(Me.btnApply, "btnApply")
        Me.btnApply.Name = "btnApply"
        Me.btnApply.UseVisualStyleBackColor = True
        '
        'ctlRoles
        '
        resources.ApplyResources(Me.ctlRoles, "ctlRoles")
        Me.ctlRoles.EditMode = AutomateUI.AuthEditMode.ManageRoles
        Me.ctlRoles.Name = "ctlRoles"
        '
        'ctlSecurityUserRoles
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.btnApply)
        Me.Controls.Add(Me.ctlRoles)
        Me.Name = "ctlSecurityUserRoles"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlRoles As AutomateUI.ctlAuth
    Friend WithEvents btnApply As AutomateControls.Buttons.StandardStyledButton

End Class
