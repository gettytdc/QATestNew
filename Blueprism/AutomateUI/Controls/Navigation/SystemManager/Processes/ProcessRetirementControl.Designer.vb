<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ProcessRetirementControl
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ProcessRetirementControl))
        Dim TreeListViewItemCollectionComparer1 As AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer = New AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer()
        Dim TreeListViewItemCollectionComparer2 As AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer = New AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer()
        Me.lblActive = New System.Windows.Forms.Label()
        Me.lblRetired = New System.Windows.Forms.Label()
        Me.splitMain = New AutomateControls.GrippableSplitContainer()
        Me.tvActive = New AutomateUI.ProcessBackedMemberTreeListView()
        Me.tvRetired = New AutomateUI.ProcessBackedMemberTreeListView()
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitMain.Panel1.SuspendLayout()
        Me.splitMain.Panel2.SuspendLayout()
        Me.splitMain.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblActive
        '
        resources.ApplyResources(Me.lblActive, "lblActive")
        Me.lblActive.Name = "lblActive"
        '
        'lblRetired
        '
        resources.ApplyResources(Me.lblRetired, "lblRetired")
        Me.lblRetired.Name = "lblRetired"
        '
        'splitMain
        '
        resources.ApplyResources(Me.splitMain, "splitMain")
        Me.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.splitMain.GripVisible = False
        Me.splitMain.Name = "splitMain"
        '
        'splitMain.Panel1
        '
        Me.splitMain.Panel1.Controls.Add(Me.tvActive)
        Me.splitMain.Panel1.Controls.Add(Me.lblActive)
        '
        'splitMain.Panel2
        '
        Me.splitMain.Panel2.Controls.Add(Me.tvRetired)
        Me.splitMain.Panel2.Controls.Add(Me.lblRetired)
        Me.splitMain.TabStop = False
        '
        'tvActive
        '
        Me.tvActive.AllowDrop = True
        TreeListViewItemCollectionComparer1.Column = 0
        TreeListViewItemCollectionComparer1.SortOrder = System.Windows.Forms.SortOrder.Ascending
        Me.tvActive.Comparer = TreeListViewItemCollectionComparer1
        resources.ApplyResources(Me.tvActive, "tvActive")
        Me.tvActive.FocusedItem = Nothing
        Me.tvActive.HideSelection = False
        Me.tvActive.ManagePermissions = False
        Me.tvActive.MultiLevelSelect = True
        Me.tvActive.Name = "tvActive"
        Me.tvActive.ShowDescription = True
        Me.tvActive.ShowDocumentLiteralFlag = False
        Me.tvActive.ShowExposedWebServiceName = False
        Me.tvActive.ShowMemberHighlights = False
        Me.tvActive.UpdateTreeFromStore = True
        Me.tvActive.UseCompatibleStateImageBehavior = False
        Me.tvActive.UseLegacyNamespaceFlag = False
        '
        'tvRetired
        '
        Me.tvRetired.AllowDrop = True
        TreeListViewItemCollectionComparer2.Column = 0
        TreeListViewItemCollectionComparer2.SortOrder = System.Windows.Forms.SortOrder.Ascending
        Me.tvRetired.Comparer = TreeListViewItemCollectionComparer2
        resources.ApplyResources(Me.tvRetired, "tvRetired")
        Me.tvRetired.FocusedItem = Nothing
        Me.tvRetired.HideSelection = False
        Me.tvRetired.ManagePermissions = False
        Me.tvRetired.MultiLevelSelect = True
        Me.tvRetired.Name = "tvRetired"
        Me.tvRetired.PlusMinusLineColor = System.Drawing.Color.Transparent
        Me.tvRetired.ShowDescription = True
        Me.tvRetired.ShowDocumentLiteralFlag = False
        Me.tvRetired.ShowExposedWebServiceName = False
        Me.tvRetired.ShowFlat = True
        Me.tvRetired.ShowMemberHighlights = False
        Me.tvRetired.ShowPlusMinus = False
        Me.tvRetired.UpdateTreeFromStore = False
        Me.tvRetired.UseCompatibleStateImageBehavior = False
        Me.tvRetired.UseLegacyNamespaceFlag = False
        '
        'ProcessRetirementControl
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.splitMain)
        Me.Name = "ProcessRetirementControl"
        resources.ApplyResources(Me, "$this")
        Me.splitMain.Panel1.ResumeLayout(False)
        Me.splitMain.Panel2.ResumeLayout(False)
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitMain.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents tvActive As AutomateUI.ProcessBackedMemberTreeListView
    Private WithEvents tvRetired As AutomateUI.ProcessBackedMemberTreeListView
    Private WithEvents splitMain As AutomateControls.GrippableSplitContainer
    Private WithEvents lblActive As System.Windows.Forms.Label
    Private WithEvents lblRetired As System.Windows.Forms.Label

End Class
