<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlActionAndArgumentsListPair
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlActionAndArgumentsListPair))
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.panPause = New System.Windows.Forms.Panel()
        Me.lblInterval = New System.Windows.Forms.Label()
        Me.btnRemove = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnAdd = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnMoveDown = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnMoveUp = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.TabControl2 = New System.Windows.Forms.TabControl()
        Me.TabPage3 = New System.Windows.Forms.TabPage()
        Me.OutputTab = New System.Windows.Forms.TabPage()
        Me.ctlActions = New AutomateUI.ctlListView()
        Me.exprPause = New AutomateUI.ctlExpressionEdit()
        Me.btnInfo = New AutomateUI.ctlRolloverButton()
        Me.ctlArguments = New AutomateUI.ctlListView()
        Me.ctlListViewOutputs = New AutomateUI.ctlListView()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.TabControl1.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.panPause.SuspendLayout()
        Me.TabControl2.SuspendLayout()
        Me.TabPage3.SuspendLayout()
        Me.OutputTab.SuspendLayout()
        Me.SuspendLayout()
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.Panel2, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.TabControl2, 0, 1)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.TabControl1)
        Me.Panel2.Controls.Add(Me.btnRemove)
        Me.Panel2.Controls.Add(Me.btnAdd)
        Me.Panel2.Controls.Add(Me.btnInfo)
        Me.Panel2.Controls.Add(Me.btnMoveDown)
        Me.Panel2.Controls.Add(Me.btnMoveUp)
        resources.ApplyResources(Me.Panel2, "Panel2")
        Me.Panel2.Name = "Panel2"
        '
        'TabControl1
        '
        resources.ApplyResources(Me.TabControl1, "TabControl1")
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.ctlActions)
        Me.TabPage1.Controls.Add(Me.panPause)
        resources.ApplyResources(Me.TabPage1, "TabPage1")
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'panPause
        '
        Me.panPause.Controls.Add(Me.lblInterval)
        Me.panPause.Controls.Add(Me.exprPause)
        resources.ApplyResources(Me.panPause, "panPause")
        Me.panPause.Name = "panPause"
        '
        'lblInterval
        '
        resources.ApplyResources(Me.lblInterval, "lblInterval")
        Me.lblInterval.Name = "lblInterval"
        '
        'btnRemove
        '
        resources.ApplyResources(Me.btnRemove, "btnRemove")
        Me.btnRemove.Name = "btnRemove"
        Me.btnRemove.UseVisualStyleBackColor = False
        '
        'btnAdd
        '
        resources.ApplyResources(Me.btnAdd, "btnAdd")
        Me.btnAdd.Name = "btnAdd"
        Me.btnAdd.UseVisualStyleBackColor = False
        '
        'btnMoveDown
        '
        resources.ApplyResources(Me.btnMoveDown, "btnMoveDown")
        Me.btnMoveDown.Name = "btnMoveDown"
        Me.btnMoveDown.UseVisualStyleBackColor = False
        '
        'btnMoveUp
        '
        resources.ApplyResources(Me.btnMoveUp, "btnMoveUp")
        Me.btnMoveUp.Name = "btnMoveUp"
        Me.btnMoveUp.UseVisualStyleBackColor = False
        '
        'TabControl2
        '
        Me.TabControl2.Controls.Add(Me.TabPage3)
        Me.TabControl2.Controls.Add(Me.OutputTab)
        resources.ApplyResources(Me.TabControl2, "TabControl2")
        Me.TabControl2.Name = "TabControl2"
        Me.TabControl2.SelectedIndex = 0
        '
        'TabPage3
        '
        Me.TabPage3.Controls.Add(Me.ctlArguments)
        Me.TabPage3.ForeColor = System.Drawing.SystemColors.ControlText
        resources.ApplyResources(Me.TabPage3, "TabPage3")
        Me.TabPage3.Name = "TabPage3"
        Me.TabPage3.UseVisualStyleBackColor = True
        '
        'OutputTab
        '
        Me.OutputTab.Controls.Add(Me.ctlListViewOutputs)
        resources.ApplyResources(Me.OutputTab, "OutputTab")
        Me.OutputTab.Name = "OutputTab"
        Me.OutputTab.UseVisualStyleBackColor = True
        '
        'ctlActions
        '
        Me.ctlActions.AllowDrop = True
        Me.ctlActions.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.ctlActions.CurrentEditableRow = Nothing
        resources.ApplyResources(Me.ctlActions, "ctlActions")
        Me.ctlActions.FillColumn = Nothing
        Me.ctlActions.FrameBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.ctlActions.HighlightedForeColor = System.Drawing.SystemColors.HighlightText
        Me.ctlActions.HighlightedRowBackColour = System.Drawing.Color.FromArgb(CType(CType(182, Byte), Integer), CType(CType(202, Byte), Integer), CType(CType(234, Byte), Integer))
        Me.ctlActions.HighlightedRowOutline = System.Drawing.Color.Red
        Me.ctlActions.LastColumnAutoSize = False
        Me.ctlActions.MinimumColumnWidth = 200
        Me.ctlActions.Name = "ctlActions"
        Me.ctlActions.Readonly = False
        Me.ctlActions.RowHeight = 26
        Me.ctlActions.Rows.Capacity = 0
        Me.ctlActions.Sortable = False
        '
        'exprPause
        '
        resources.ApplyResources(Me.exprPause, "exprPause")
        Me.exprPause.Border = True
        Me.exprPause.HighlightingEnabled = True
        Me.exprPause.IsDecision = False
        Me.exprPause.Name = "exprPause"
        Me.exprPause.PasswordChar = ChrW(0)
        Me.exprPause.Stage = Nothing
        '
        'btnInfo
        '
        resources.ApplyResources(Me.btnInfo, "btnInfo")
        Me.btnInfo.DefaultImage = Global.AutomateUI.My.Resources.ToolImages.Information_16x16
        Me.btnInfo.DisabledImage = Global.AutomateUI.My.Resources.ToolImages.Information_16x16_Disabled
        Me.btnInfo.Name = "btnInfo"
        Me.btnInfo.RolloverImage = Global.AutomateUI.My.Resources.ToolImages.Information_16x16_Hot
        Me.btnInfo.TooltipText = ""
        Me.btnInfo.TooltipTitle = ""
        '
        'ctlArguments
        '
        Me.ctlArguments.AllowDrop = True
        Me.ctlArguments.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.ctlArguments.CurrentEditableRow = Nothing
        resources.ApplyResources(Me.ctlArguments, "ctlArguments")
        Me.ctlArguments.FillColumn = Nothing
        Me.ctlArguments.FrameBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.ctlArguments.HighlightedForeColor = System.Drawing.SystemColors.HighlightText
        Me.ctlArguments.HighlightedRowBackColour = System.Drawing.Color.FromArgb(CType(CType(182, Byte), Integer), CType(CType(202, Byte), Integer), CType(CType(234, Byte), Integer))
        Me.ctlArguments.HighlightedRowOutline = System.Drawing.SystemColors.Highlight
        Me.ctlArguments.LastColumnAutoSize = True
        Me.ctlArguments.MinimumColumnWidth = 200
        Me.ctlArguments.Name = "ctlArguments"
        Me.ctlArguments.Readonly = False
        Me.ctlArguments.RowHeight = 26
        Me.ctlArguments.Rows.Capacity = 0
        Me.ctlArguments.Sortable = False
        '
        'ctlListViewOutputs
        '
        Me.ctlListViewOutputs.AllowDrop = True
        Me.ctlListViewOutputs.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.ctlListViewOutputs.CurrentEditableRow = Nothing
        resources.ApplyResources(Me.ctlListViewOutputs, "ctlListViewOutputs")
        Me.ctlListViewOutputs.FillColumn = Nothing
        Me.ctlListViewOutputs.FrameBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.ctlListViewOutputs.HighlightedForeColor = System.Drawing.SystemColors.HighlightText
        Me.ctlListViewOutputs.HighlightedRowBackColour = System.Drawing.Color.FromArgb(CType(CType(182, Byte), Integer), CType(CType(202, Byte), Integer), CType(CType(234, Byte), Integer))
        Me.ctlListViewOutputs.HighlightedRowOutline = System.Drawing.SystemColors.Highlight
        Me.ctlListViewOutputs.LastColumnAutoSize = True
        Me.ctlListViewOutputs.MinimumColumnWidth = 200
        Me.ctlListViewOutputs.Name = "ctlListViewOutputs"
        Me.ctlListViewOutputs.Readonly = False
        Me.ctlListViewOutputs.RowHeight = 26
        Me.ctlListViewOutputs.Rows.Capacity = 0
        Me.ctlListViewOutputs.Sortable = False
        '
        'ctlActionAndArgumentsListPair
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Name = "ctlActionAndArgumentsListPair"
        resources.ApplyResources(Me, "$this")
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.Panel2.ResumeLayout(False)
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.panPause.ResumeLayout(False)
        Me.panPause.PerformLayout()
        Me.TabControl2.ResumeLayout(False)
        Me.TabPage3.ResumeLayout(False)
        Me.OutputTab.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
    Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents ctlActions As AutomateUI.ctlListView
    Friend WithEvents btnRemove As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnAdd As AutomateControls.Buttons.StandardStyledButton
    Protected WithEvents btnInfo As AutomateUI.ctlRolloverButton
    Friend WithEvents btnMoveDown As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnMoveUp As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents TabControl2 As System.Windows.Forms.TabControl
    Friend WithEvents TabPage3 As System.Windows.Forms.TabPage
    Friend WithEvents ctlArguments As AutomateUI.ctlListView
    Private WithEvents lblInterval As Label
    Private WithEvents panPause As Panel
    Private WithEvents exprPause As ctlExpressionEdit
    Friend WithEvents OutputTab As TabPage
    Friend WithEvents ctlListViewOutputs As ctlListView
End Class
