<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlDataItemTreeView
    Inherits UserControl

    'UserControl overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents trvStages As AutomateControls.Trees.FlickerFreeTreeView
    Friend WithEvents chkPage As System.Windows.Forms.CheckBox
    Friend WithEvents chkDataType As System.Windows.Forms.CheckBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents pnlHeader As AutomateUI.clsPanel
    Friend WithEvents trvStagesBySheet As AutomateControls.Trees.FlickerFreeTreeView
    Friend WithEvents trvStagesByDataType As AutomateControls.Trees.FlickerFreeTreeView
    Friend WithEvents trvStagesBySheetAndDataType As AutomateControls.Trees.FlickerFreeTreeView
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlDataItemTreeView))
        Me.trvStages = New AutomateControls.Trees.FlickerFreeTreeView
        Me.trvStagesBySheet = New AutomateControls.Trees.FlickerFreeTreeView
        Me.trvStagesByDataType = New AutomateControls.Trees.FlickerFreeTreeView
        Me.trvStagesBySheetAndDataType = New AutomateControls.Trees.FlickerFreeTreeView
        Me.pnlHeader = New AutomateUI.clsPanel()
        Me.chkViewAllItems = New System.Windows.Forms.CheckBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.chkPage = New System.Windows.Forms.CheckBox()
        Me.chkDataType = New System.Windows.Forms.CheckBox()
        Me.pnlHeader.SuspendLayout()
        Me.SuspendLayout()
        '
        'trvStages
        '
        resources.ApplyResources(Me.trvStages, "trvStages")
        Me.trvStages.ItemHeight = 20
        Me.trvStages.Name = "trvStages"
        '
        'trvStagesBySheet
        '
        resources.ApplyResources(Me.trvStagesBySheet, "trvStagesBySheet")
        Me.trvStagesBySheet.ItemHeight = 20
        Me.trvStagesBySheet.Name = "trvStagesBySheet"
        '
        'trvStagesByDataType
        '
        resources.ApplyResources(Me.trvStagesByDataType, "trvStagesByDataType")
        Me.trvStagesByDataType.ItemHeight = 20
        Me.trvStagesByDataType.Name = "trvStagesByDataType"
        '
        'trvStagesBySheetAndDataType
        '
        resources.ApplyResources(Me.trvStagesBySheetAndDataType, "trvStagesBySheetAndDataType")
        Me.trvStagesBySheetAndDataType.ItemHeight = 20
        Me.trvStagesBySheetAndDataType.Name = "trvStagesBySheetAndDataType"
        '
        'pnlHeader
        '
        resources.ApplyResources(Me.pnlHeader, "pnlHeader")
        Me.pnlHeader.BorderColor = System.Drawing.SystemColors.InactiveCaption
        Me.pnlHeader.BorderStyle = AutomateUI.clsPanel.BorderMode.[On]
        Me.pnlHeader.BorderWidth = 1
        Me.pnlHeader.Controls.Add(Me.chkViewAllItems)
        Me.pnlHeader.Controls.Add(Me.Label1)
        Me.pnlHeader.Controls.Add(Me.chkPage)
        Me.pnlHeader.Controls.Add(Me.chkDataType)
        Me.pnlHeader.Name = "pnlHeader"
        '
        'chkViewAllItems
        '
        resources.ApplyResources(Me.chkViewAllItems, "chkViewAllItems")
        Me.chkViewAllItems.Name = "chkViewAllItems"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'chkPage
        '
        resources.ApplyResources(Me.chkPage, "chkPage")
        Me.chkPage.Name = "chkPage"
        '
        'chkDataType
        '
        resources.ApplyResources(Me.chkDataType, "chkDataType")
        Me.chkDataType.Name = "chkDataType"
        '
        'ctlDataItemTreeView
        '
        Me.Controls.Add(Me.trvStages)
        Me.Controls.Add(Me.pnlHeader)
        Me.Controls.Add(Me.trvStagesBySheetAndDataType)
        Me.Controls.Add(Me.trvStagesByDataType)
        Me.Controls.Add(Me.trvStagesBySheet)
        resources.ApplyResources(Me, "$this")
        Me.Name = "ctlDataItemTreeView"
        Me.pnlHeader.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents chkViewAllItems As System.Windows.Forms.CheckBox
End Class
