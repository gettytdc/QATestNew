Imports AutomateControls.Wizard

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlChooseWebExposeProcess
    Inherits WizardPanel

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlChooseWebExposeProcess))
        Dim TreeListViewItemCollectionComparer1 As AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer = New AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer()
        Me.ctlProcesses = New AutomateUI.ProcessBackedMemberTreeListView()
        Me.SuspendLayout()
        '
        'ctlProcesses
        '
        resources.ApplyResources(Me.ctlProcesses, "ctlProcesses")
        Me.ctlProcesses.BackColor = System.Drawing.Color.White
        Me.ctlProcesses.CausesValidation = False
        TreeListViewItemCollectionComparer1.Column = 0
        TreeListViewItemCollectionComparer1.SortOrder = System.Windows.Forms.SortOrder.Ascending
        Me.ctlProcesses.Comparer = TreeListViewItemCollectionComparer1
        Me.ctlProcesses.FocusedItem = Nothing
        Me.ctlProcesses.MultiLevelSelect = True
        Me.ctlProcesses.MultiSelect = False
        Me.ctlProcesses.Name = "ctlProcesses"
        Me.ctlProcesses.ShowExposedWebServiceName = False
        Me.ctlProcesses.ShowDocumentLiteralFlag = False
        Me.ctlProcesses.UseLegacyNamespaceFlag = False
        Me.ctlProcesses.UseCompatibleStateImageBehavior = False
        '
        'ctlChooseWebExposeProcess
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.ctlProcesses)
        Me.Name = "ctlChooseWebExposeProcess"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlProcesses As ProcessBackedMemberTreeListView

End Class
