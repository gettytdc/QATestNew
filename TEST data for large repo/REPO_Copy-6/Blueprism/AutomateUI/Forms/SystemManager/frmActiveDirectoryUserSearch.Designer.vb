<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmActiveDirectoryUserSearch
    Inherits AutomateControls.Forms.AutomateForm

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmActiveDirectoryUserSearch))
        Me.tBar = New AutomateControls.TitleBar()
        Me.ctlSingleUserActiveDirectorySearch = New AutomateUI.CtlSingleUserActiveDirectorySearch()
        Me.SuspendLayout()
        '
        'tBar
        '
        Me.tBar.Dock = System.Windows.Forms.DockStyle.Top
        Me.tBar.Font = New System.Drawing.Font("Segoe UI", 12!)
        Me.tBar.Location = New System.Drawing.Point(0, 0)
        Me.tBar.Name = "tBar"
        Me.tBar.Size = New System.Drawing.Size(718, 70)
        Me.tBar.TabIndex = 1
        Me.tBar.TabStop = False
        Me.tBar.Title = "Search for an Active Directory account to map"
        Me.tBar.WrapTitle = False
        '
        'ctlSingleUserActiveDirectorySearch
        '
        Me.ctlSingleUserActiveDirectorySearch.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom)  _
            Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.ctlSingleUserActiveDirectorySearch.Location = New System.Drawing.Point(0, 72)
        Me.ctlSingleUserActiveDirectorySearch.MinimumSize = New System.Drawing.Size(400, 350)
        Me.ctlSingleUserActiveDirectorySearch.Name = "ctlSingleUserActiveDirectorySearch"
        Me.ctlSingleUserActiveDirectorySearch.Size = New System.Drawing.Size(724, 429)
        Me.ctlSingleUserActiveDirectorySearch.TabIndex = 2
        '
        'frmActiveDirectoryUserSearch
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.ClientSize = New System.Drawing.Size(718, 511)
        Me.Controls.Add(Me.ctlSingleUserActiveDirectorySearch)
        Me.Controls.Add(Me.tBar)
        Me.HelpButton = True
        Me.Icon = CType(resources.GetObject("$this.Icon"),System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(734, 550)
        Me.Name = "frmActiveDirectoryUserSearch"
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Active Directory search"
        Me.ResumeLayout(False)

End Sub

    Private WithEvents tBar As AutomateControls.TitleBar
    Friend WithEvents ctlSingleUserActiveDirectorySearch As ctlSingleUserActiveDirectorySearch
End Class
