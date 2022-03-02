<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlProcessChooser
    Inherits AutomateUI.ctlWizardStageControl

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
        Dim TreeListViewItemCollectionComparer1 As AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer = New AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlProcessChooser))
        Me.tvGroups = New AutomateUI.ProcessBackedMemberTreeListView()
        Me.SuspendLayout()
        '
        'tvGroups
        '
        TreeListViewItemCollectionComparer1.Column = 0
        TreeListViewItemCollectionComparer1.SortOrder = System.Windows.Forms.SortOrder.Ascending
        Me.tvGroups.Comparer = TreeListViewItemCollectionComparer1
        resources.ApplyResources(Me.tvGroups, "tvGroups")
        Me.tvGroups.FocusedItem = Nothing
        Me.tvGroups.MultiLevelSelect = True
        Me.tvGroups.Name = "tvGroups"
        Me.tvGroups.ShowExposedWebServiceName = False
        Me.tvGroups.ShowDocumentLiteralFlag = False
        Me.tvGroups.UseLegacyNamespaceFlag = False
        Me.tvGroups.UseCompatibleStateImageBehavior = False
        '
        'ctlProcessChooser
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.tvGroups)
        Me.Name = "ctlProcessChooser"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents tvGroups As AutomateUI.ProcessBackedMemberTreeListView

End Class
