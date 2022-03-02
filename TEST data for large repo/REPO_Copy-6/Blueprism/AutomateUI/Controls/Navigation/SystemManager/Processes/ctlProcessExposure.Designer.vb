<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlProcessExposure
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlProcessExposure))
        Dim TreeListViewItemCollectionComparer1 As AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer = New AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.llExpose = New AutomateControls.BulletedLinkLabel()
        Me.llConceal = New AutomateControls.BulletedLinkLabel()
        Me.ctlExposedProcesses = New AutomateUI.ProcessBackedMemberTreeListView()
        Me.SuspendLayout()
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.Label2.Name = "Label2"
        '
        'llExpose
        '
        resources.ApplyResources(Me.llExpose, "llExpose")
        Me.llExpose.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llExpose.Name = "llExpose"
        Me.llExpose.TabStop = True
        '
        'llConceal
        '
        resources.ApplyResources(Me.llConceal, "llConceal")
        Me.llConceal.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llConceal.Name = "llConceal"
        Me.llConceal.TabStop = True
        '
        'ctlExposedProcesses
        '
        resources.ApplyResources(Me.ctlExposedProcesses, "ctlExposedProcesses")
        Me.ctlExposedProcesses.BackColor = System.Drawing.Color.White
        Me.ctlExposedProcesses.CausesValidation = False
        TreeListViewItemCollectionComparer1.Column = 0
        TreeListViewItemCollectionComparer1.SortOrder = System.Windows.Forms.SortOrder.Ascending
        Me.ctlExposedProcesses.Comparer = TreeListViewItemCollectionComparer1
        Me.ctlExposedProcesses.FocusedItem = Nothing
        Me.ctlExposedProcesses.ManagePermissions = False
        Me.ctlExposedProcesses.MultiLevelSelect = True
        Me.ctlExposedProcesses.MultiSelect = False
        Me.ctlExposedProcesses.Name = "ctlExposedProcesses"
        Me.ctlExposedProcesses.ShowDescription = True
        Me.ctlExposedProcesses.ShowDocumentLiteralFlag = True
        Me.ctlExposedProcesses.ShowEmptyGroups = False
        Me.ctlExposedProcesses.ShowExposedWebServiceName = True
        Me.ctlExposedProcesses.UpdateTreeFromStore = true
        Me.ctlExposedProcesses.UseCompatibleStateImageBehavior = False
        Me.ctlExposedProcesses.UseLegacyNamespaceFlag = True
        '
        'ctlProcessExposure
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.ctlExposedProcesses)
        Me.Controls.Add(Me.llExpose)
        Me.Controls.Add(Me.llConceal)
        Me.Name = "ctlProcessExposure"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents ctlExposedProcesses As ProcessBackedMemberTreeListView
    Friend WithEvents llExpose As AutomateControls.BulletedLinkLabel
    Friend WithEvents llConceal As AutomateControls.BulletedLinkLabel

End Class
