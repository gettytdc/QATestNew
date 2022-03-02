<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlSecurityCredentials
    Inherits System.Windows.Forms.UserControl

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSecurityCredentials))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.llCredEdit = New AutomateControls.BulletedLinkLabel()
        Me.llCredDelete = New AutomateControls.BulletedLinkLabel()
        Me.llCredAdd = New AutomateControls.BulletedLinkLabel()
        Me.lstCredentials = New AutomateControls.FlickerFreeListView()
        Me.ColumnHeader5 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader6 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader7 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader8 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.llFindReferences = New AutomateControls.BulletedLinkLabel()
        Me.SuspendLayout()
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.Label1.Name = "Label1"
        '
        'llCredEdit
        '
        resources.ApplyResources(Me.llCredEdit, "llCredEdit")
        Me.llCredEdit.LinkColor = System.Drawing.SystemColors.ControlText
        Me.llCredEdit.Name = "llCredEdit"
        Me.llCredEdit.TabStop = True
        Me.llCredEdit.UseCompatibleTextRendering = True
        '
        'llCredDelete
        '
        resources.ApplyResources(Me.llCredDelete, "llCredDelete")
        Me.llCredDelete.LinkColor = System.Drawing.SystemColors.ControlText
        Me.llCredDelete.Name = "llCredDelete"
        Me.llCredDelete.TabStop = True
        Me.llCredDelete.UseCompatibleTextRendering = True
        '
        'llCredAdd
        '
        resources.ApplyResources(Me.llCredAdd, "llCredAdd")
        Me.llCredAdd.LinkColor = System.Drawing.SystemColors.ControlText
        Me.llCredAdd.Name = "llCredAdd"
        Me.llCredAdd.TabStop = True
        Me.llCredAdd.UseCompatibleTextRendering = True
        '
        'lstCredentials
        '
        resources.ApplyResources(Me.lstCredentials, "lstCredentials")
        Me.lstCredentials.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader5, Me.ColumnHeader6, Me.ColumnHeader7, Me.ColumnHeader8, Me.ColumnHeader1})
        Me.lstCredentials.FullRowSelect = True
        Me.lstCredentials.HideSelection = False
        Me.lstCredentials.Name = "lstCredentials"
        Me.lstCredentials.UseCompatibleStateImageBehavior = False
        Me.lstCredentials.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader5
        '
        resources.ApplyResources(Me.ColumnHeader5, "ColumnHeader5")
        '
        'ColumnHeader6
        '
        resources.ApplyResources(Me.ColumnHeader6, "ColumnHeader6")
        '
        'ColumnHeader7
        '
        resources.ApplyResources(Me.ColumnHeader7, "ColumnHeader7")
        '
        'ColumnHeader8
        '
        resources.ApplyResources(Me.ColumnHeader8, "ColumnHeader8")
        '
        'ColumnHeader1
        '
        resources.ApplyResources(Me.ColumnHeader1, "ColumnHeader1")
        '
        'llFindReferences
        '
        resources.ApplyResources(Me.llFindReferences, "llFindReferences")
        Me.llFindReferences.LinkColor = System.Drawing.SystemColors.ControlText
        Me.llFindReferences.Name = "llFindReferences"
        Me.llFindReferences.TabStop = True
        Me.llFindReferences.UseCompatibleTextRendering = True
        '
        'ctlSecurityCredentials
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.llFindReferences)
        Me.Controls.Add(Me.llCredEdit)
        Me.Controls.Add(Me.llCredDelete)
        Me.Controls.Add(Me.llCredAdd)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.lstCredentials)
        Me.Name = "ctlSecurityCredentials"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents llCredEdit As AutomateControls.BulletedLinkLabel
    Private WithEvents llCredDelete As AutomateControls.BulletedLinkLabel
    Private WithEvents llCredAdd As AutomateControls.BulletedLinkLabel
    Private WithEvents Label1 As System.Windows.Forms.Label
    Private WithEvents lstCredentials As AutomateControls.FlickerFreeListView
    Private WithEvents ColumnHeader5 As System.Windows.Forms.ColumnHeader
    Private WithEvents ColumnHeader6 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader7 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader8 As System.Windows.Forms.ColumnHeader
    Private WithEvents llFindReferences As AutomateControls.BulletedLinkLabel
    Friend WithEvents ColumnHeader1 As ColumnHeader
End Class
