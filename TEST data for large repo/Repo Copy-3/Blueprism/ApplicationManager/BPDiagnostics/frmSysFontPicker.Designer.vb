<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmSysFontPicker
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSysFontPicker))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.cmbFontFamily = New System.Windows.Forms.ComboBox()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.chkBold = New System.Windows.Forms.CheckBox()
        Me.chkItalic = New System.Windows.Forms.CheckBox()
        Me.chkUnderlined = New System.Windows.Forms.CheckBox()
        Me.cmbSize = New System.Windows.Forms.ComboBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.FlowLayoutPanel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'cmbFontFamily
        '
        resources.ApplyResources(Me.cmbFontFamily, "cmbFontFamily")
        Me.cmbFontFamily.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbFontFamily.FormattingEnabled = True
        Me.cmbFontFamily.Name = "cmbFontFamily"
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'chkBold
        '
        resources.ApplyResources(Me.chkBold, "chkBold")
        Me.chkBold.Name = "chkBold"
        Me.chkBold.UseVisualStyleBackColor = True
        '
        'chkItalic
        '
        resources.ApplyResources(Me.chkItalic, "chkItalic")
        Me.chkItalic.Name = "chkItalic"
        Me.chkItalic.UseVisualStyleBackColor = True
        '
        'chkUnderlined
        '
        resources.ApplyResources(Me.chkUnderlined, "chkUnderlined")
        Me.chkUnderlined.Name = "chkUnderlined"
        Me.chkUnderlined.UseVisualStyleBackColor = True
        '
        'cmbSize
        '
        Me.cmbSize.FormattingEnabled = True
        resources.ApplyResources(Me.cmbSize, "cmbSize")
        Me.cmbSize.Name = "cmbSize"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'FlowLayoutPanel1
        '
        Me.FlowLayoutPanel1.Controls.Add(Me.chkBold)
        Me.FlowLayoutPanel1.Controls.Add(Me.chkItalic)
        Me.FlowLayoutPanel1.Controls.Add(Me.chkUnderlined)
        resources.ApplyResources(Me.FlowLayoutPanel1, "FlowLayoutPanel1")
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        '
        'frmSysFontPicker
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.FlowLayoutPanel1)
        Me.Controls.Add(Me.cmbSize)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.cmbFontFamily)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label2)
        Me.Name = "frmSysFontPicker"
        Me.FlowLayoutPanel1.ResumeLayout(False)
        Me.FlowLayoutPanel1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents cmbFontFamily As System.Windows.Forms.ComboBox
    Friend WithEvents btnOK As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents chkBold As System.Windows.Forms.CheckBox
    Friend WithEvents chkItalic As System.Windows.Forms.CheckBox
    Friend WithEvents chkUnderlined As System.Windows.Forms.CheckBox
    Friend WithEvents cmbSize As System.Windows.Forms.ComboBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
End Class
