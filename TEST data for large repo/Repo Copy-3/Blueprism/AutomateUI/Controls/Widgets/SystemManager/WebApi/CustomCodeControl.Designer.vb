Namespace Controls.Widgets.SystemManager.WebApi

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class CustomCodeControl
        Inherits System.Windows.Forms.UserControl

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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(CustomCodeControl))
        Me.btnCheckCode = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.lstParameters = New System.Windows.Forms.ListBox()
        Me.ctlCodeEditor = New AutomateUI.ctlCodeEditor()
        Me.pnlHost = New System.Windows.Forms.TableLayoutPanel()
        Me.pnlCheckCode = New System.Windows.Forms.Panel()
        Me.splCode = New AutomateControls.FlickerFreeSplitContainer()
        Me.pnlHost.SuspendLayout
        Me.pnlCheckCode.SuspendLayout
        CType(Me.splCode,System.ComponentModel.ISupportInitialize).BeginInit
        Me.splCode.Panel1.SuspendLayout
        Me.splCode.Panel2.SuspendLayout
        Me.splCode.SuspendLayout
        Me.SuspendLayout
        '
        'btnCheckCode
        '
        resources.ApplyResources(Me.btnCheckCode, "btnCheckCode")
        Me.btnCheckCode.BackColor = System.Drawing.Color.White
        Me.btnCheckCode.Name = "btnCheckCode"
        Me.btnCheckCode.UseVisualStyleBackColor = false
        '
        'lstParameters
        '
        Me.lstParameters.DisplayMember = "Text"
        resources.ApplyResources(Me.lstParameters, "lstParameters")
        Me.lstParameters.FormattingEnabled = true
        Me.lstParameters.Name = "lstParameters"
        Me.lstParameters.SelectionMode = System.Windows.Forms.SelectionMode.None
        '
        'ctlCodeEditor
        '
        Me.ctlCodeEditor.BackgroundColour = System.Drawing.SystemColors.Window
        Me.ctlCodeEditor.Code = ""
        resources.ApplyResources(Me.ctlCodeEditor, "ctlCodeEditor")
        Me.ctlCodeEditor.Name = "ctlCodeEditor"
        Me.ctlCodeEditor.ReadOnly = false
        '
        'pnlHost
        '
        resources.ApplyResources(Me.pnlHost, "pnlHost")
        Me.pnlHost.Controls.Add(Me.pnlCheckCode, 0, 1)
        Me.pnlHost.Controls.Add(Me.splCode, 0, 0)
        Me.pnlHost.Name = "pnlHost"
        '
        'pnlCheckCode
        '
        Me.pnlCheckCode.Controls.Add(Me.btnCheckCode)
        resources.ApplyResources(Me.pnlCheckCode, "pnlCheckCode")
        Me.pnlCheckCode.Name = "pnlCheckCode"
        '
        'splCode
        '
        resources.ApplyResources(Me.splCode, "splCode")
        Me.splCode.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.splCode.Name = "splCode"
        '
        'splCode.Panel1
        '
        Me.splCode.Panel1.Controls.Add(Me.ctlCodeEditor)
        '
        'splCode.Panel2
        '
        Me.splCode.Panel2.Controls.Add(Me.lstParameters)
        '
        'CustomCodeControl
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.pnlHost)
        Me.Name = "CustomCodeControl"
        resources.ApplyResources(Me, "$this")
        Me.pnlHost.ResumeLayout(false)
        Me.pnlCheckCode.ResumeLayout(false)
        Me.splCode.Panel1.ResumeLayout(false)
        Me.splCode.Panel2.ResumeLayout(false)
        CType(Me.splCode,System.ComponentModel.ISupportInitialize).EndInit
        Me.splCode.ResumeLayout(false)
        Me.ResumeLayout(false)

End Sub
        Private WithEvents btnCheckCode As AutomateControls.Buttons.StandardStyledButton
        Friend WithEvents lstParameters As ListBox
        Private WithEvents ctlCodeEditor As ctlCodeEditor
        Friend WithEvents pnlHost As TableLayoutPanel
        Friend WithEvents pnlCheckCode As Panel
        Friend WithEvents splCode As AutomateControls.FlickerFreeSplitContainer
    End Class
End Namespace
