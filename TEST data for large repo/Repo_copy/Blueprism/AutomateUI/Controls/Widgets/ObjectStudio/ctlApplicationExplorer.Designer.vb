<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlApplicationExplorer
    Inherits System.Windows.Forms.UserControl

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlApplicationExplorer))
        Me.btnAddChild = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnAddElement = New AutomateControls.Buttons.StandardStyledButton()
        Me.panButtons = New System.Windows.Forms.FlowLayoutPanel()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.txtFilter = New AutomateControls.FilterTextBox()
        Me.lblAdd = New System.Windows.Forms.Label()
        Me.tvAppModel = New AutomateUI.clsReorderableTreeview()
        Me.panButtons.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnAddChild
        '
        resources.ApplyResources(Me.btnAddChild, "btnAddChild")
        Me.btnAddChild.Name = "btnAddChild"
        '
        'btnAddElement
        '
        resources.ApplyResources(Me.btnAddElement, "btnAddElement")
        Me.btnAddElement.Name = "btnAddElement"
        '
        'panButtons
        '
        Me.panButtons.Controls.Add(Me.lblAdd)
        Me.panButtons.Controls.Add(Me.btnAddElement)
        Me.panButtons.Controls.Add(Me.btnAddChild)
        resources.ApplyResources(Me.panButtons, "panButtons")
        Me.panButtons.Name = "panButtons"
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.txtFilter)
        Me.Panel1.Controls.Add(Me.tvAppModel)
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Name = "Panel1"
        '
        'txtFilter
        '
        Me.txtFilter.AlwaysShowHandOnFarHover = True
        Me.txtFilter.AlwaysShowHandOnNearHover = True
        resources.ApplyResources(Me.txtFilter, "txtFilter")
        Me.txtFilter.GuidanceText = "Filter the tree..."
        Me.txtFilter.Name = "txtFilter"
        '
        'lblAdd
        '
        resources.ApplyResources(Me.lblAdd, "lblAdd")
        Me.lblAdd.Name = "lblAdd"
        '
        'tvAppModel
        '
        Me.tvAppModel.AllowDrop = True
        Me.tvAppModel.AllowReordering = True
        resources.ApplyResources(Me.tvAppModel, "tvAppModel")
        Me.tvAppModel.DropTargetColour = System.Drawing.Color.Olive
        Me.tvAppModel.HideSelection = False
        Me.tvAppModel.Name = "tvAppModel"
        Me.tvAppModel.ShowNodeToolTips = True
        '
        'ctlApplicationExplorer
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.panButtons)
        Me.Name = "ctlApplicationExplorer"
        resources.ApplyResources(Me, "$this")
        Me.panButtons.ResumeLayout(False)
        Me.panButtons.PerformLayout()
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents tvAppModel As AutomateUI.clsReorderableTreeview
    Private WithEvents btnAddChild As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents btnAddElement As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents Panel1 As System.Windows.Forms.Panel
    Private WithEvents panButtons As System.Windows.Forms.FlowLayoutPanel
    Private WithEvents txtFilter As AutomateControls.FilterTextBox
    Friend WithEvents lblAdd As Label
End Class
