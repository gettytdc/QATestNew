<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlExceptionTypes
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlExceptionTypes))
        Me.llScanProcesses = New AutomateControls.BulletedLinkLabel()
        Me.llDeleteExceptionType = New AutomateControls.BulletedLinkLabel()
        Me.lvExceptionTypes = New AutomateControls.FlickerFreeListView()
        Me.ColumnHeader13 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader14 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.SuspendLayout()
        '
        'llScanProcesses
        '
        resources.ApplyResources(Me.llScanProcesses, "llScanProcesses")
        Me.llScanProcesses.BackColor = System.Drawing.Color.Transparent
        Me.llScanProcesses.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llScanProcesses.Name = "llScanProcesses"
        Me.llScanProcesses.TabStop = True
        '
        'llDeleteExceptionType
        '
        resources.ApplyResources(Me.llDeleteExceptionType, "llDeleteExceptionType")
        Me.llDeleteExceptionType.BackColor = System.Drawing.Color.Transparent
        Me.llDeleteExceptionType.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llDeleteExceptionType.Name = "llDeleteExceptionType"
        Me.llDeleteExceptionType.TabStop = True
        '
        'lvExceptionTypes
        '
        resources.ApplyResources(Me.lvExceptionTypes, "lvExceptionTypes")
        Me.lvExceptionTypes.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader13, Me.ColumnHeader14})
        Me.lvExceptionTypes.FullRowSelect = True
        Me.lvExceptionTypes.HideSelection = False
        Me.lvExceptionTypes.Name = "lvExceptionTypes"
        Me.lvExceptionTypes.UseCompatibleStateImageBehavior = False
        Me.lvExceptionTypes.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader13
        '
        resources.ApplyResources(Me.ColumnHeader13, "ColumnHeader13")
        '
        'ColumnHeader14
        '
        resources.ApplyResources(Me.ColumnHeader14, "ColumnHeader14")
        '
        'ctlExceptionTypes
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.llScanProcesses)
        Me.Controls.Add(Me.llDeleteExceptionType)
        Me.Controls.Add(Me.lvExceptionTypes)
        Me.Name = "ctlExceptionTypes"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents llScanProcesses As AutomateControls.BulletedLinkLabel
    Friend WithEvents llDeleteExceptionType As AutomateControls.BulletedLinkLabel
    Friend WithEvents lvExceptionTypes As AutomateControls.FlickerFreeListView
    Friend WithEvents ColumnHeader13 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader14 As System.Windows.Forms.ColumnHeader

End Class
