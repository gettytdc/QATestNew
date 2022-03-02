<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlProcessHistory
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlProcessHistory))
        Me.llHistory = New AutomateControls.BulletedLinkLabel()
        Me.llCompare = New AutomateControls.BulletedLinkLabel()
        Me.cmbProcessList = New AutomateUI.GroupMemberComboBox()
        Me.mProcessHistoryList = New AutomateUI.ProcessHistoryListView()
        Me.SuspendLayout()
        '
        'llHistory
        '
        resources.ApplyResources(Me.llHistory, "llHistory")
        Me.llHistory.BackColor = System.Drawing.Color.Transparent
        Me.llHistory.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llHistory.Name = "llHistory"
        Me.llHistory.TabStop = True
        '
        'llCompare
        '
        resources.ApplyResources(Me.llCompare, "llCompare")
        Me.llCompare.BackColor = System.Drawing.Color.Transparent
        Me.llCompare.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llCompare.Name = "llCompare"
        Me.llCompare.TabStop = True
        '
        'cmbProcessList
        '
        resources.ApplyResources(Me.cmbProcessList, "cmbProcessList")
        Me.cmbProcessList.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.cmbProcessList.Checkable = False
        Me.cmbProcessList.DropDownBackColor = System.Drawing.SystemColors.ControlLightLight
        Me.cmbProcessList.DropDownWidth = 280
        Me.cmbProcessList.Name = "cmbProcessList"
        Me.cmbProcessList.SelectedItemsXML = "<GroupComboBox><SelectedItems /></GroupComboBox>"
        Me.cmbProcessList.TreeType = BluePrism.AutomateAppCore.Groups.GroupTreeType.Processes
        '
        'mProcessHistoryList
        '
        resources.ApplyResources(Me.mProcessHistoryList, "mProcessHistoryList")
        Me.mProcessHistoryList.CanViewDefinition = True
        Me.mProcessHistoryList.FillColumn = 3
        Me.mProcessHistoryList.Mode = Nothing
        Me.mProcessHistoryList.Name = "mProcessHistoryList"
        '
        'ctlProcessHistory
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.mProcessHistoryList)
        Me.Controls.Add(Me.llHistory)
        Me.Controls.Add(Me.llCompare)
        Me.Controls.Add(Me.cmbProcessList)
        Me.Name = "ctlProcessHistory"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents llHistory As AutomateControls.BulletedLinkLabel
    Friend WithEvents llCompare As AutomateControls.BulletedLinkLabel
    Friend WithEvents cmbProcessList As AutomateUI.GroupMemberComboBox
    Private WithEvents mProcessHistoryList As AutomateUI.ProcessHistoryListView

End Class
