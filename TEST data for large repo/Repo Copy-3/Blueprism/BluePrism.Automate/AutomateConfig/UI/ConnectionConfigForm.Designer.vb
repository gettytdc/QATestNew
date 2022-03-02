Imports AutomateControls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ConnectionConfigForm
    Inherits Forms.HelpButtonForm

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ConnectionConfigForm))
        Me.titleBar = New AutomateControls.TitleBar()
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.lineBottom = New AutomateControls.Line3D()
        Me.connManager = New BluePrism.Config.ConnectionManagerPanel()
        Me.FlowLayoutPanel1.SuspendLayout
        Me.SuspendLayout
        '
        'titleBar
        '
        resources.ApplyResources(Me.titleBar, "titleBar")
        Me.titleBar.Name = "titleBar"
        '
        'FlowLayoutPanel1
        '
        Me.FlowLayoutPanel1.Controls.Add(Me.btnCancel)
        Me.FlowLayoutPanel1.Controls.Add(Me.btnOK)
        resources.ApplyResources(Me.FlowLayoutPanel1, "FlowLayoutPanel1")
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = true
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        Me.btnOK.UseVisualStyleBackColor = true
        '
        'lineBottom
        '
        resources.ApplyResources(Me.lineBottom, "lineBottom")
        Me.lineBottom.Name = "lineBottom"
        '
        'connManager
        '
        resources.ApplyResources(Me.connManager, "connManager")
        Me.connManager.Name = "connManager"
        '
        'ConnectionConfigForm
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.lineBottom)
        Me.Controls.Add(Me.FlowLayoutPanel1)
        Me.Controls.Add(Me.connManager)
        Me.Controls.Add(Me.titleBar)
        Me.HelpButton = true
        Me.MaximizeBox = false
        Me.MinimizeBox = false
        Me.Name = "ConnectionConfigForm"
        Me.FlowLayoutPanel1.ResumeLayout(false)
        Me.ResumeLayout(false)

End Sub
    Private WithEvents connManager As BluePrism.Config.ConnectionManagerPanel
    Private WithEvents titleBar As AutomateControls.TitleBar
    Friend WithEvents FlowLayoutPanel1 As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lineBottom As AutomateControls.Line3D
End Class
