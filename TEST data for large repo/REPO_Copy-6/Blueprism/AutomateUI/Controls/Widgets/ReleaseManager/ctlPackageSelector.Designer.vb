<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlPackageSelector

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlPackageSelector))
        Me.lvPackages = New System.Windows.Forms.ListView()
        Me.colName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colDescription = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.rbCreateExisting = New AutomateControls.StyledRadioButton()
        Me.rbCreateAdhoc = New AutomateControls.StyledRadioButton()
        Me.SuspendLayout()
        '
        'lvPackages
        '
        resources.ApplyResources(Me.lvPackages, "lvPackages")
        Me.lvPackages.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colName, Me.colDescription})
        Me.lvPackages.FullRowSelect = True
        Me.lvPackages.MultiSelect = False
        Me.lvPackages.Name = "lvPackages"
        Me.lvPackages.UseCompatibleStateImageBehavior = False
        Me.lvPackages.View = System.Windows.Forms.View.Details
        '
        'colName
        '
        resources.ApplyResources(Me.colName, "colName")
        '
        'colDescription
        '
        resources.ApplyResources(Me.colDescription, "colDescription")
        '
        'rbCreateExisting
        '
        resources.ApplyResources(Me.rbCreateExisting, "rbCreateExisting")
        Me.rbCreateExisting.Name = "rbCreateExisting"
        Me.rbCreateExisting.TabStop = True
        Me.rbCreateExisting.UseVisualStyleBackColor = True
        '
        'rbCreateAdhoc
        '
        resources.ApplyResources(Me.rbCreateAdhoc, "rbCreateAdhoc")
        Me.rbCreateAdhoc.Name = "rbCreateAdhoc"
        Me.rbCreateAdhoc.TabStop = True
        Me.rbCreateAdhoc.UseVisualStyleBackColor = True
        '
        'ctlPackageSelector
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.rbCreateAdhoc)
        Me.Controls.Add(Me.rbCreateExisting)
        Me.Controls.Add(Me.lvPackages)
        Me.Name = "ctlPackageSelector"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lvPackages As System.Windows.Forms.ListView
    Private WithEvents colName As System.Windows.Forms.ColumnHeader
    Private WithEvents colDescription As System.Windows.Forms.ColumnHeader
    Friend WithEvents rbCreateExisting As AutomateControls.StyledRadioButton
    Friend WithEvents rbCreateAdhoc As AutomateControls.StyledRadioButton

End Class
