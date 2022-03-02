<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class ctlExpressionEdit

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlExpressionEdit))
        Me.btnExpression = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.mTooltip = New System.Windows.Forms.ToolTip(Me.components)
        Me.txtExpression = New AutomateUI.ctlExpressionRichTextBox()
        Me.SuspendLayout()
        '
        'btnExpression
        '
        Me.btnExpression.BackColor = System.Drawing.SystemColors.Control
        resources.ApplyResources(Me.btnExpression, "btnExpression")
        Me.btnExpression.Image = Global.AutomateUI.My.Resources.ToolImages.Calculator_16x16
        Me.btnExpression.Name = "btnExpression"
        Me.btnExpression.UseVisualStyleBackColor = False
        '
        'mTooltip
        '
        Me.mTooltip.AutomaticDelay = 5
        Me.mTooltip.AutoPopDelay = 5000
        Me.mTooltip.InitialDelay = 10
        Me.mTooltip.ReshowDelay = 10
        '
        'txtExpression
        '
        Me.txtExpression.AllowDrop = True
        Me.txtExpression.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtExpression.DetectUrls = False
        resources.ApplyResources(Me.txtExpression, "txtExpression")
        Me.txtExpression.HideSelection = False
        Me.txtExpression.HighlightingEnabled = True
        Me.txtExpression.Name = "txtExpression"
        Me.txtExpression.PasswordChar = ChrW(0)
        '
        'ctlExpressionEdit
        '
        Me.Controls.Add(Me.txtExpression)
        Me.Controls.Add(Me.btnExpression)
        resources.ApplyResources(Me, "$this")
        Me.Name = "ctlExpressionEdit"
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents btnExpression As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents txtExpression As AutomateUI.ctlExpressionRichTextBox
    Private WithEvents mTooltip As System.Windows.Forms.ToolTip

End Class
