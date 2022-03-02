Namespace Controls.Widgets.SystemManager.WebApi.Request

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class TemplateBodyContentPanel
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(TemplateBodyContentPanel))
        Me.txtTemplate = New ScintillaNET.Scintilla()
        Me.ctlToolTip = New System.Windows.Forms.ToolTip(Me.components)
        CType(Me.txtTemplate,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'txtTemplate
        '
        Me.txtTemplate.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom)  _
            Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.txtTemplate.Font = New System.Drawing.Font("Consolas", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
        Me.txtTemplate.Location = New System.Drawing.Point(0, 0)
        Me.txtTemplate.Name = "txtTemplate"
        Me.txtTemplate.Size = New System.Drawing.Size(404, 250)
        Me.txtTemplate.Styles.BraceBad.FontName = "Verdana"&ChrW(0)
        Me.txtTemplate.Styles.BraceLight.FontName = "Verdana"&ChrW(0)
        Me.txtTemplate.Styles.ControlChar.FontName = "Verdana"&ChrW(0)
        Me.txtTemplate.Styles.Default.BackColor = System.Drawing.SystemColors.Window
        Me.txtTemplate.Styles.Default.FontName = "Verdana"&ChrW(0)
        Me.txtTemplate.Styles.IndentGuide.FontName = "Verdana"&ChrW(0)
        Me.txtTemplate.Styles.LastPredefined.FontName = "Verdana"&ChrW(0)
        Me.txtTemplate.Styles.LineNumber.FontName = "Verdana"&ChrW(0)
        Me.txtTemplate.Styles.Max.FontName = "Verdana"&ChrW(0)
        Me.txtTemplate.TabIndex = 27
        Me.ctlToolTip.SetToolTip(Me.txtTemplate, resources.GetString("txtTemplate.ToolTip"))
        '
        'TemplateBodyContentPanel
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.txtTemplate)
        Me.Name = "TemplateBodyContentPanel"
        Me.Size = New System.Drawing.Size(404, 250)
        CType(Me.txtTemplate,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)

End Sub

        Private WithEvents txtTemplate As ScintillaNET.Scintilla
        Friend WithEvents ctlToolTip As ToolTip
    End Class

End Namespace