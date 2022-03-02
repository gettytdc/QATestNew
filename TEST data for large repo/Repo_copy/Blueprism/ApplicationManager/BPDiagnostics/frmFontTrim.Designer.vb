<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmFontTrim
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmFontTrim))
        Me.lblSpaceAbove = New System.Windows.Forms.Label()
        Me.lblSpaceBelow = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtRowsAbove = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.btnGoAbove = New System.Windows.Forms.Button()
        Me.btnGoBelow = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txtRowsBelow = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.lblFontHeight = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'lblSpaceAbove
        '
        resources.ApplyResources(Me.lblSpaceAbove, "lblSpaceAbove")
        Me.lblSpaceAbove.Name = "lblSpaceAbove"
        '
        'lblSpaceBelow
        '
        resources.ApplyResources(Me.lblSpaceBelow, "lblSpaceBelow")
        Me.lblSpaceBelow.Name = "lblSpaceBelow"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'txtRowsAbove
        '
        resources.ApplyResources(Me.txtRowsAbove, "txtRowsAbove")
        Me.txtRowsAbove.Name = "txtRowsAbove"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'btnGoAbove
        '
        resources.ApplyResources(Me.btnGoAbove, "btnGoAbove")
        Me.btnGoAbove.Name = "btnGoAbove"
        Me.btnGoAbove.UseVisualStyleBackColor = True
        '
        'btnGoBelow
        '
        resources.ApplyResources(Me.btnGoBelow, "btnGoBelow")
        Me.btnGoBelow.Name = "btnGoBelow"
        Me.btnGoBelow.UseVisualStyleBackColor = True
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'txtRowsBelow
        '
        resources.ApplyResources(Me.txtRowsBelow, "txtRowsBelow")
        Me.txtRowsBelow.Name = "txtRowsBelow"
        '
        'Label6
        '
        resources.ApplyResources(Me.Label6, "Label6")
        Me.Label6.Name = "Label6"
        '
        'btnClose
        '
        resources.ApplyResources(Me.btnClose, "btnClose")
        Me.btnClose.Name = "btnClose"
        Me.btnClose.UseVisualStyleBackColor = True
        '
        'lblFontHeight
        '
        resources.ApplyResources(Me.lblFontHeight, "lblFontHeight")
        Me.lblFontHeight.Name = "lblFontHeight"
        '
        'frmFontTrim
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.lblFontHeight)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.btnGoBelow)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.txtRowsBelow)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.btnGoAbove)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txtRowsAbove)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.lblSpaceBelow)
        Me.Controls.Add(Me.lblSpaceAbove)
        Me.Name = "frmFontTrim"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblSpaceAbove As System.Windows.Forms.Label
    Friend WithEvents lblSpaceBelow As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtRowsAbove As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents btnGoAbove As System.Windows.Forms.Button
    Friend WithEvents btnGoBelow As System.Windows.Forms.Button
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents txtRowsBelow As System.Windows.Forms.TextBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents lblFontHeight As System.Windows.Forms.Label
End Class
