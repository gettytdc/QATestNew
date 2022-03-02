<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ErrorMessage
    Inherits System.Windows.Forms.Form

    Friend Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ErrorMessage))
        Me.txtDetail = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnDetail = New AutomateControls.Buttons.StandardStyledButton()
        Me.pnlError = New System.Windows.Forms.Panel()
        Me.txtMessage = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnCopy = New AutomateControls.Buttons.StandardStyledButton()
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.FlowLayoutPanel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtDetail
        '
        Me.txtDetail.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.txtDetail, "txtDetail")
        Me.txtDetail.Name = "txtDetail"
        Me.txtDetail.ReadOnly = True
        '
        'btnDetail
        '
        resources.ApplyResources(Me.btnDetail, "btnDetail")
        Me.btnDetail.Name = "btnDetail"
        '
        'pnlError
        '
        resources.ApplyResources(Me.pnlError, "pnlError")
        Me.pnlError.Name = "pnlError"
        '
        'txtMessage
        '
        resources.ApplyResources(Me.txtMessage, "txtMessage")
        Me.txtMessage.BackColor = System.Drawing.Color.White
        Me.txtMessage.Name = "txtMessage"
        Me.txtMessage.ReadOnly = True
        Me.txtMessage.TabStop = False
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        '
        'btnCopy
        '
        resources.ApplyResources(Me.btnCopy, "btnCopy")
        Me.btnCopy.Image = Global.BluePrism.Config.My.Resources.Resources.Copy_16x16
        Me.btnCopy.Name = "btnCopy"
        Me.btnCopy.UseVisualStyleBackColor = True
        '
        'FlowLayoutPanel1
        '
        resources.ApplyResources(Me.FlowLayoutPanel1, "FlowLayoutPanel1")
        Me.FlowLayoutPanel1.Controls.Add(Me.btnCopy)
        Me.FlowLayoutPanel1.Controls.Add(Me.btnDetail)
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        '
        'ErrorMessage
        '
        Me.AcceptButton = Me.btnOK
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.FlowLayoutPanel1)
        Me.Controls.Add(Me.txtMessage)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.txtDetail)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D
        Me.Name = "ErrorMessage"
        Me.FlowLayoutPanel1.ResumeLayout(False)
        Me.FlowLayoutPanel1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtDetail As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents btnDetail As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents pnlError As System.Windows.Forms.Panel
    Friend WithEvents btnCopy As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents txtMessage As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
End Class
