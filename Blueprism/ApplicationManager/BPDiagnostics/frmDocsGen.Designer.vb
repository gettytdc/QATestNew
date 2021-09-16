<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmDocsGen
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmDocsGen))
        Me.txtSource = New System.Windows.Forms.TextBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.btnBrowseOutput = New System.Windows.Forms.Button()
        Me.chkFixLinks = New System.Windows.Forms.CheckBox()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.btnSourceGen = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtOutputDir = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.lblDocFormat = New System.Windows.Forms.Label()
        Me.btnDocs = New System.Windows.Forms.Button()
        Me.cmbDocFormat = New System.Windows.Forms.ComboBox()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.btnQDocs = New System.Windows.Forms.Button()
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtSource
        '
        resources.ApplyResources(Me.txtSource, "txtSource")
        Me.txtSource.Name = "txtSource"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.btnBrowseOutput)
        Me.GroupBox1.Controls.Add(Me.chkFixLinks)
        Me.GroupBox1.Controls.Add(Me.btnBrowse)
        Me.GroupBox1.Controls.Add(Me.btnSourceGen)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.txtOutputDir)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.txtSource)
        resources.ApplyResources(Me.GroupBox1, "GroupBox1")
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.TabStop = False
        '
        'btnBrowseOutput
        '
        resources.ApplyResources(Me.btnBrowseOutput, "btnBrowseOutput")
        Me.btnBrowseOutput.Name = "btnBrowseOutput"
        Me.btnBrowseOutput.UseVisualStyleBackColor = True
        '
        'chkFixLinks
        '
        resources.ApplyResources(Me.chkFixLinks, "chkFixLinks")
        Me.chkFixLinks.Checked = True
        Me.chkFixLinks.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkFixLinks.Name = "chkFixLinks"
        '
        'btnBrowse
        '
        resources.ApplyResources(Me.btnBrowse, "btnBrowse")
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'btnSourceGen
        '
        resources.ApplyResources(Me.btnSourceGen, "btnSourceGen")
        Me.btnSourceGen.Name = "btnSourceGen"
        Me.btnSourceGen.UseVisualStyleBackColor = True
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'txtOutputDir
        '
        resources.ApplyResources(Me.txtOutputDir, "txtOutputDir")
        Me.txtOutputDir.Name = "txtOutputDir"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.lblDocFormat)
        Me.GroupBox2.Controls.Add(Me.btnDocs)
        Me.GroupBox2.Controls.Add(Me.cmbDocFormat)
        resources.ApplyResources(Me.GroupBox2, "GroupBox2")
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.TabStop = False
        '
        'lblDocFormat
        '
        resources.ApplyResources(Me.lblDocFormat, "lblDocFormat")
        Me.lblDocFormat.Name = "lblDocFormat"
        '
        'btnDocs
        '
        resources.ApplyResources(Me.btnDocs, "btnDocs")
        Me.btnDocs.Name = "btnDocs"
        Me.btnDocs.UseVisualStyleBackColor = True
        '
        'cmbDocFormat
        '
        Me.cmbDocFormat.FormattingEnabled = True
        resources.ApplyResources(Me.cmbDocFormat, "cmbDocFormat")
        Me.cmbDocFormat.Name = "cmbDocFormat"
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.btnQDocs)
        resources.ApplyResources(Me.GroupBox3, "GroupBox3")
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.TabStop = False
        '
        'btnQDocs
        '
        resources.ApplyResources(Me.btnQDocs, "btnQDocs")
        Me.btnQDocs.Name = "btnQDocs"
        Me.btnQDocs.UseVisualStyleBackColor = True
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.FileName = "OpenFileDialog1"
        '
        'frmDocsGen
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.Name = "frmDocsGen"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBox3.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents txtSource As System.Windows.Forms.TextBox
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents lblDocFormat As System.Windows.Forms.Label
    Friend WithEvents btnDocs As System.Windows.Forms.Button
    Friend WithEvents cmbDocFormat As System.Windows.Forms.ComboBox
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents btnQDocs As System.Windows.Forms.Button
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtOutputDir As System.Windows.Forms.TextBox
    Friend WithEvents btnSourceGen As System.Windows.Forms.Button
    Friend WithEvents btnBrowse As System.Windows.Forms.Button
    Friend WithEvents OpenFileDialog1 As System.Windows.Forms.OpenFileDialog
    Friend WithEvents chkFixLinks As System.Windows.Forms.CheckBox
    Friend WithEvents btnBrowseOutput As System.Windows.Forms.Button
End Class
