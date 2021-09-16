Imports BluePrism.Images

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlRowsPerPage
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlRowsPerPage))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtCurrentPage = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnFirstPage = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnLastPage = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnPrevPage = New AutomateControls.Buttons.StandardStyledButton()
        Me.txtTotalPages = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnNextPage = New AutomateControls.Buttons.StandardStyledButton()
        Me.cmbRowsPerPage = New System.Windows.Forms.ComboBox()
        Me.lblRowsPerPage = New System.Windows.Forms.Label()
        Me.llTotalRows = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'txtCurrentPage
        '
        Me.txtCurrentPage.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.txtCurrentPage, "txtCurrentPage")
        Me.txtCurrentPage.Name = "txtCurrentPage"
        '
        'btnFirstPage
        '
        resources.ApplyResources(Me.btnFirstPage, "btnFirstPage")
        Me.btnFirstPage.Name = "btnFirstPage"
        '
        'btnLastPage
        '
        resources.ApplyResources(Me.btnLastPage, "btnLastPage")
        Me.btnLastPage.Name = "btnLastPage"
        '
        'btnPrevPage
        '
        resources.ApplyResources(Me.btnPrevPage, "btnPrevPage")
        Me.btnPrevPage.Name = "btnPrevPage"
        '
        'txtTotalPages
        '
        Me.txtTotalPages.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.txtTotalPages, "txtTotalPages")
        Me.txtTotalPages.Name = "txtTotalPages"
        Me.txtTotalPages.ReadOnly = True
        '
        'btnNextPage
        '
        resources.ApplyResources(Me.btnNextPage, "btnNextPage")
        Me.btnNextPage.Name = "btnNextPage"
        '
        'cmbRowsPerPage
        '
        resources.ApplyResources(Me.cmbRowsPerPage, "cmbRowsPerPage")
        Me.cmbRowsPerPage.FormattingEnabled = True
        Me.cmbRowsPerPage.Name = "cmbRowsPerPage"
        '
        'lblRowsPerPage
        '
        resources.ApplyResources(Me.lblRowsPerPage, "lblRowsPerPage")
        Me.lblRowsPerPage.Name = "lblRowsPerPage"
        '
        'llTotalRows
        '
        resources.ApplyResources(Me.llTotalRows, "llTotalRows")
        Me.llTotalRows.Name = "llTotalRows"
        '
        'ctlRowsPerPage
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.llTotalRows)
        Me.Controls.Add(Me.cmbRowsPerPage)
        Me.Controls.Add(Me.lblRowsPerPage)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtCurrentPage)
        Me.Controls.Add(Me.btnFirstPage)
        Me.Controls.Add(Me.btnLastPage)
        Me.Controls.Add(Me.btnPrevPage)
        Me.Controls.Add(Me.txtTotalPages)
        Me.Controls.Add(Me.btnNextPage)
        Me.Name = "ctlRowsPerPage"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtCurrentPage As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents btnFirstPage As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnLastPage As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnPrevPage As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents txtTotalPages As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents btnNextPage As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents cmbRowsPerPage As System.Windows.Forms.ComboBox
    Friend WithEvents lblRowsPerPage As System.Windows.Forms.Label
    Friend WithEvents llTotalRows As System.Windows.Forms.Label

End Class
