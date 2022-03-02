Imports AutomateControls.Wizard
Imports DataPipelineOutputConfigUISettings = BluePrism.DataPipeline.UI.DataPipelineOutputConfigUISettings

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlChooseSessionLogFields
    Inherits WizardPanel

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlChooseSessionLogFields))
        Me.lblConfigName = New System.Windows.Forms.Label()
        Me.lblSelectedConfigName = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.lblChooseFields = New System.Windows.Forms.Label()
        Me.btnDeselectAll = New AutomateControls.BulletedLinkLabel()
        Me.btnSelectAll = New AutomateControls.BulletedLinkLabel()
        Me.chkListSessionLogFields = New System.Windows.Forms.CheckedListBox()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblConfigName
        '
        resources.ApplyResources(Me.lblConfigName, "lblConfigName")
        Me.lblConfigName.Name = "lblConfigName"
        '
        'lblSelectedConfigName
        '
        resources.ApplyResources(Me.lblSelectedConfigName, "lblSelectedConfigName")
        Me.lblSelectedConfigName.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.lblSelectedConfigName.Name = "lblSelectedConfigName"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.Color.FromArgb(CType(CType(208, Byte), Integer), CType(CType(238, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.Panel1.Controls.Add(Me.lblChooseFields)
        Me.Panel1.Controls.Add(Me.btnDeselectAll)
        Me.Panel1.Controls.Add(Me.btnSelectAll)
        Me.Panel1.Controls.Add(Me.chkListSessionLogFields)
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Name = "Panel1"
        '
        'lblChooseFields
        '
        resources.ApplyResources(Me.lblChooseFields, "lblChooseFields")
        Me.lblChooseFields.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblChooseFields.Name = "lblChooseFields"
        '
        'btnDeselectAll
        '
        resources.ApplyResources(Me.btnDeselectAll, "btnDeselectAll")
        Me.btnDeselectAll.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.btnDeselectAll.Name = "btnDeselectAll"
        Me.btnDeselectAll.TabStop = True
        Me.btnDeselectAll.UseCompatibleTextRendering = True
        '
        'btnSelectAll
        '
        resources.ApplyResources(Me.btnSelectAll, "btnSelectAll")
        Me.btnSelectAll.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.btnSelectAll.Name = "btnSelectAll"
        Me.btnSelectAll.TabStop = True
        Me.btnSelectAll.UseCompatibleTextRendering = True
        '
        'chkListSessionLogFields
        '
        Me.chkListSessionLogFields.BackColor = System.Drawing.Color.FromArgb(CType(CType(208, Byte), Integer), CType(CType(238, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.chkListSessionLogFields.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.chkListSessionLogFields.CheckOnClick = True
        resources.ApplyResources(Me.chkListSessionLogFields, "chkListSessionLogFields")
        Me.chkListSessionLogFields.FormattingEnabled = True
        Me.chkListSessionLogFields.Name = "chkListSessionLogFields"
        '
        'ctlChooseSessionLogFields
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.lblSelectedConfigName)
        Me.Controls.Add(Me.lblConfigName)
        resources.ApplyResources(Me, "$this")
        Me.Name = "ctlChooseSessionLogFields"
        Me.NavigatePrevious = True
        Me.Title = Global.AutomateUI.My.Resources.Resources.ctlChooseSessionLogFields_title
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblConfigName As Label
    Friend WithEvents lblSelectedConfigName As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents Panel1 As Panel
    Friend WithEvents lblChooseFields As Label
    Friend WithEvents btnDeselectAll As AutomateControls.BulletedLinkLabel
    Friend WithEvents btnSelectAll As AutomateControls.BulletedLinkLabel
    Friend WithEvents chkListSessionLogFields As CheckedListBox
End Class
