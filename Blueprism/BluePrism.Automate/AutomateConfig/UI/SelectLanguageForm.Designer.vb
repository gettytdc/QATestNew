<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SelectLanguageForm
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SelectLanguageForm))
        Me.ExitButton = New AutomateControls.Buttons.FlatStyleStyledButton()
        Me.ListPanel = New System.Windows.Forms.Panel()
        Me.BorderPanel = New System.Windows.Forms.Panel()
        Me.NextButton = New AutomateControls.Buttons.StandardStyledButton()
        Me.SubTitle = New AutomateControls.Textboxes.ReadOnlyTextBox()
        Me.Title = New AutomateControls.Textboxes.ReadOnlyTextBox()
        Me.BorderPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'ExitButton
        '
        Me.ExitButton.FlatAppearance.BorderColor = System.Drawing.Color.White
        Me.ExitButton.FlatAppearance.BorderSize = 0
        Me.ExitButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White
        Me.ExitButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.ExitButton, "ExitButton")
        Me.ExitButton.ForeColor = System.Drawing.Color.White
        Me.ExitButton.Image = Global.BluePrism.Config.My.Resources.Resources.cross_blue
        Me.ExitButton.Name = "ExitButton"
        Me.ExitButton.UseVisualStyleBackColor = True
        '
        'ListPanel
        '
        resources.ApplyResources(Me.ListPanel, "ListPanel")
        Me.ListPanel.Name = "ListPanel"
        '
        'BorderPanel
        '
        Me.BorderPanel.BackColor = System.Drawing.Color.White
        Me.BorderPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.BorderPanel.Controls.Add(Me.SubTitle)
        Me.BorderPanel.Controls.Add(Me.Title)
        Me.BorderPanel.Controls.Add(Me.NextButton)
        Me.BorderPanel.Controls.Add(Me.ExitButton)
        Me.BorderPanel.Controls.Add(Me.ListPanel)
        resources.ApplyResources(Me.BorderPanel, "BorderPanel")
        Me.BorderPanel.Name = "BorderPanel"
        '
        'NextButton
        '
        Me.NextButton.BackColor = System.Drawing.Color.White
        Me.NextButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(17, Byte), Integer), CType(CType(126, Byte), Integer), CType(CType(194, Byte), Integer))
        resources.ApplyResources(Me.NextButton, "NextButton")
        Me.NextButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(17, Byte), Integer), CType(CType(126, Byte), Integer), CType(CType(194, Byte), Integer))
        Me.NextButton.Name = "NextButton"
        Me.NextButton.UseVisualStyleBackColor = False
        '
        'SubTitle
        '
        Me.SubTitle.AccessibleRole = System.Windows.Forms.AccessibleRole.StaticText
        Me.SubTitle.BackColor = System.Drawing.Color.White
        Me.SubTitle.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.SubTitle.Cursor = System.Windows.Forms.Cursors.Default
        resources.ApplyResources(Me.SubTitle, "SubTitle")
        Me.SubTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.SubTitle.Name = "SubTitle"
        Me.SubTitle.ReadOnly = True
        Me.SubTitle.SelectionEnabled = False
        '
        'Title
        '
        Me.Title.AccessibleRole = System.Windows.Forms.AccessibleRole.StaticText
        Me.Title.BackColor = System.Drawing.Color.White
        Me.Title.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.Title.Cursor = System.Windows.Forms.Cursors.Default
        resources.ApplyResources(Me.Title, "Title")
        Me.Title.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.Title.Name = "Title"
        Me.Title.ReadOnly = True
        Me.Title.SelectionEnabled = False
        '
        'SelectLanguageForm
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.BorderPanel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "SelectLanguageForm"
        Me.BorderPanel.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents ExitButton As AutomateControls.Buttons.FlatStyleStyledButton
    Private WithEvents ListPanel As Panel
    Friend WithEvents BorderPanel As Panel
    Friend WithEvents NextButton As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents Title As AutomateControls.Textboxes.ReadOnlyTextBox
    Friend WithEvents SubTitle As AutomateControls.Textboxes.ReadOnlyTextBox
End Class
