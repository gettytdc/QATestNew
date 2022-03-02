<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlStoreInEdit

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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlStoreInEdit))
        Me.btnAutoCreate = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.txtStoreInValue = New AutomateControls.Textboxes.StyledTextBox()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.SuspendLayout()
        '
        'btnAutoCreate
        '
        resources.ApplyResources(Me.btnAutoCreate, "btnAutoCreate")
        Me.btnAutoCreate.Image = Global.AutomateUI.My.Resources.ComponentImages.Field_16x16
        Me.btnAutoCreate.Name = "btnAutoCreate"
        Me.ToolTip1.SetToolTip(Me.btnAutoCreate, resources.GetString("btnAutoCreate.ToolTip"))
        Me.btnAutoCreate.UseVisualStyleBackColor = False
        '
        'txtStoreInValue
        '
        Me.txtStoreInValue.AllowDrop = True
        resources.ApplyResources(Me.txtStoreInValue, "txtStoreInValue")
        Me.txtStoreInValue.BorderColor = System.Drawing.Color.Empty
        Me.txtStoreInValue.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtStoreInValue.Name = "txtStoreInValue"
        '
        'ToolTip1
        '
        Me.ToolTip1.AutomaticDelay = 0
        Me.ToolTip1.AutoPopDelay = 20000
        Me.ToolTip1.InitialDelay = 200
        Me.ToolTip1.ReshowDelay = 40
        Me.ToolTip1.ShowAlways = True
        '
        'ctlStoreInEdit
        '
        Me.AllowDrop = True
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.White
        Me.Controls.Add(Me.txtStoreInValue)
        Me.Controls.Add(Me.btnAutoCreate)
        resources.ApplyResources(Me, "$this")
        Me.Name = "ctlStoreInEdit"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents btnAutoCreate As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents txtStoreInValue As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents ToolTip1 As System.Windows.Forms.ToolTip

End Class
