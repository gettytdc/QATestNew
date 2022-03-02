Partial Class ctlResourceView
#Region " Windows Form Designer generated code "

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents lvResourceList2 As AutomateControls.FlickerFreeListView
    Friend WithEvents lOffline As System.Windows.Forms.Label
    Friend WithEvents ttResStatus As System.Windows.Forms.ToolTip
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlResourceView))
        Dim TreeListViewItemCollectionComparer1 As AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer = New AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer()
        Me.ttResStatus = New System.Windows.Forms.ToolTip(Me.components)
        Me.lOffline = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'lOffline
        '
        resources.ApplyResources(Me.lOffline, "lOffline")
        Me.lOffline.BackColor = System.Drawing.Color.Transparent
        Me.lOffline.Name = "lOffline"
        '
        'ctlResourceView
        '
        TreeListViewItemCollectionComparer1.Column = 0
        TreeListViewItemCollectionComparer1.SortOrder = System.Windows.Forms.SortOrder.Ascending
        Me.Comparer = TreeListViewItemCollectionComparer1
        Me.Controls.Add(Me.lOffline)
        Me.ShowEmptyGroups = False
        resources.ApplyResources(Me, "$this")
        Me.TreeType = BluePrism.AutomateAppCore.Groups.GroupTreeType.Resources
        Me.UseCompatibleStateImageBehavior = False
        Me.ResumeLayout(False)

    End Sub

#End Region
End Class
