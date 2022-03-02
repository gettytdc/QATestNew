<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlWebServices
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlWebServices))
        Me.objWebServicesView = New AutomateUI.ctlWebServicesView()
        Me.llAdd = New AutomateControls.BulletedLinkLabel()
        Me.llDelete = New AutomateControls.BulletedLinkLabel()
        Me.llUpdate = New AutomateControls.BulletedLinkLabel()
        Me.llFindReferences = New AutomateControls.BulletedLinkLabel()
        Me.SuspendLayout()
        '
        'objWebServicesView
        '
        resources.ApplyResources(Me.objWebServicesView, "objWebServicesView")
        Me.objWebServicesView.BackColor = System.Drawing.Color.White
        Me.objWebServicesView.Name = "objWebServicesView"
        '
        'llAdd
        '
        resources.ApplyResources(Me.llAdd, "llAdd")
        Me.llAdd.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llAdd.Name = "llAdd"
        Me.llAdd.TabStop = True
        Me.llAdd.UseCompatibleTextRendering = True
        '
        'llDelete
        '
        resources.ApplyResources(Me.llDelete, "llDelete")
        Me.llDelete.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llDelete.Name = "llDelete"
        Me.llDelete.TabStop = True
        Me.llDelete.UseCompatibleTextRendering = True
        '
        'llUpdate
        '
        resources.ApplyResources(Me.llUpdate, "llUpdate")
        Me.llUpdate.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llUpdate.Name = "llUpdate"
        Me.llUpdate.TabStop = True
        Me.llUpdate.UseCompatibleTextRendering = True
        '
        'llFindReferences
        '
        resources.ApplyResources(Me.llFindReferences, "llFindReferences")
        Me.llFindReferences.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llFindReferences.Name = "llFindReferences"
        Me.llFindReferences.TabStop = True
        Me.llFindReferences.UseCompatibleTextRendering = True
        '
        'ctlWebServices
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.llFindReferences)
        Me.Controls.Add(Me.objWebServicesView)
        Me.Controls.Add(Me.llAdd)
        Me.Controls.Add(Me.llDelete)
        Me.Controls.Add(Me.llUpdate)
        Me.Name = "ctlWebServices"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents objWebServicesView As AutomateUI.ctlWebServicesView
    Friend WithEvents llAdd As AutomateControls.BulletedLinkLabel
    Friend WithEvents llDelete As AutomateControls.BulletedLinkLabel
    Friend WithEvents llUpdate As AutomateControls.BulletedLinkLabel
    Friend WithEvents llFindReferences As AutomateControls.BulletedLinkLabel

End Class
