<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlFileChooser

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlFileChooser))
        Me.lblPrompt = New System.Windows.Forms.Label()
        Me.btnBrowse = New AutomateControls.Buttons.StandardStyledButton()
        Me.pnlChooseFileLocation = New System.Windows.Forms.Panel()
        Me.txtFilePath = New AutomateControls.ActivatingTextBox()
        Me.pnlChooseFileLocation.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblPrompt
        '
        resources.ApplyResources(Me.lblPrompt, "lblPrompt")
        Me.lblPrompt.Name = "lblPrompt"
        '
        'btnBrowse
        '
        resources.ApplyResources(Me.btnBrowse, "btnBrowse")
        Me.btnBrowse.Name = "btnBrowse"
        '
        'pnlChooseFileLocation
        '
        Me.pnlChooseFileLocation.Controls.Add(Me.btnBrowse)
        Me.pnlChooseFileLocation.Controls.Add(Me.lblPrompt)
        Me.pnlChooseFileLocation.Controls.Add(Me.txtFilePath)
        resources.ApplyResources(Me.pnlChooseFileLocation, "pnlChooseFileLocation")
        Me.pnlChooseFileLocation.Name = "pnlChooseFileLocation"
        '
        'txtFilePath
        '
        resources.ApplyResources(Me.txtFilePath, "txtFilePath")
        Me.txtFilePath.Name = "txtFilePath"
        '
        'ctlFileChooser
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.pnlChooseFileLocation)
        Me.Name = "ctlFileChooser"
        Me.pnlChooseFileLocation.ResumeLayout(False)
        Me.pnlChooseFileLocation.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btnBrowse As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents pnlChooseFileLocation As System.Windows.Forms.Panel
    Friend WithEvents txtFilePath As AutomateControls.ActivatingTextBox
    Private WithEvents lblPrompt As System.Windows.Forms.Label

End Class
