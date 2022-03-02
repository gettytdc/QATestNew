
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class LicenseActivationHistory
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(LicenseActivationHistory))
        Me.BorderPanel = New System.Windows.Forms.Panel()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.BackImage = New System.Windows.Forms.PictureBox()
        Me.BackText = New System.Windows.Forms.Label()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.LicenseNameLabel = New System.Windows.Forms.Label()
        Me.BorderPanel.SuspendLayout
        CType(Me.BackImage,System.ComponentModel.ISupportInitialize).BeginInit
        CType(Me.PictureBox1,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'BorderPanel
        '
        Me.BorderPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.BorderPanel.Controls.Add(Me.Label2)
        Me.BorderPanel.Controls.Add(Me.Label1)
        Me.BorderPanel.Controls.Add(Me.BackImage)
        Me.BorderPanel.Controls.Add(Me.BackText)
        Me.BorderPanel.Controls.Add(Me.PictureBox1)
        Me.BorderPanel.Controls.Add(Me.TableLayoutPanel1)
        Me.BorderPanel.Controls.Add(Me.LicenseNameLabel)
        resources.ApplyResources(Me.BorderPanel, "BorderPanel")
        Me.BorderPanel.Name = "BorderPanel"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67,Byte),Integer), CType(CType(74,Byte),Integer), CType(CType(79,Byte),Integer))
        Me.Label2.Name = "Label2"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67,Byte),Integer), CType(CType(74,Byte),Integer), CType(CType(79,Byte),Integer))
        Me.Label1.Name = "Label1"
        '
        'BackImage
        '
        Me.BackImage.Image = Global.AutomateUI.ActivationWizardResources.arrow_left
        resources.ApplyResources(Me.BackImage, "BackImage")
        Me.BackImage.Name = "BackImage"
        Me.BackImage.TabStop = false
        '
        'BackText
        '
        resources.ApplyResources(Me.BackText, "BackText")
        Me.BackText.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11,Byte),Integer), CType(CType(117,Byte),Integer), CType(CType(183,Byte),Integer))
        Me.BackText.Name = "BackText"
        '
        'PictureBox1
        '
        resources.ApplyResources(Me.PictureBox1, "PictureBox1")
        Me.PictureBox1.Image = Global.AutomateUI.ActivationWizardResources.cross_blue
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.TabStop = false
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'LicenseNameLabel
        '
        resources.ApplyResources(Me.LicenseNameLabel, "LicenseNameLabel")
        Me.LicenseNameLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11,Byte),Integer), CType(CType(117,Byte),Integer), CType(CType(183,Byte),Integer))
        Me.LicenseNameLabel.Name = "LicenseNameLabel"
        '
        'LicenseActivationHistory
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me, "$this")
        Me.ControlBox = false
        Me.Controls.Add(Me.BorderPanel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "LicenseActivationHistory"
        Me.ShowIcon = false
        Me.ShowInTaskbar = false
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        Me.BorderPanel.ResumeLayout(false)
        Me.BorderPanel.PerformLayout
        CType(Me.BackImage,System.ComponentModel.ISupportInitialize).EndInit
        CType(Me.PictureBox1,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)

End Sub

    Friend WithEvents BorderPanel As Panel
    Friend WithEvents LicenseNameLabel As Label
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents BackImage As PictureBox
    Friend WithEvents BackText As Label
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
End Class
