<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlParameterList
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlParameterList))
        Me.pnlBottomButtons = New System.Windows.Forms.Panel()
        Me.btnRemoveParam = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnAddParam = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnParamMoveDown = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnParamMoveUp = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.pnlListView = New System.Windows.Forms.Panel()
        Me.objParamsList = New AutomateUI.ctlListView()
        Me.pnlBottomButtons.SuspendLayout()
        Me.pnlListView.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlBottomButtons
        '
        Me.pnlBottomButtons.Controls.Add(Me.btnRemoveParam)
        Me.pnlBottomButtons.Controls.Add(Me.btnAddParam)
        Me.pnlBottomButtons.Controls.Add(Me.btnParamMoveDown)
        Me.pnlBottomButtons.Controls.Add(Me.btnParamMoveUp)
        resources.ApplyResources(Me.pnlBottomButtons, "pnlBottomButtons")
        Me.pnlBottomButtons.Name = "pnlBottomButtons"
        '
        'btnRemoveParam
        '
        resources.ApplyResources(Me.btnRemoveParam, "btnRemoveParam")
        Me.btnRemoveParam.Name = "btnRemoveParam"
        Me.btnRemoveParam.UseVisualStyleBackColor = False
        '
        'btnAddParam
        '
        resources.ApplyResources(Me.btnAddParam, "btnAddParam")
        Me.btnAddParam.Name = "btnAddParam"
        Me.btnAddParam.UseVisualStyleBackColor = False
        '
        'btnParamMoveDown
        '
        resources.ApplyResources(Me.btnParamMoveDown, "btnParamMoveDown")
        Me.btnParamMoveDown.Name = "btnParamMoveDown"
        Me.btnParamMoveDown.UseVisualStyleBackColor = False
        '
        'btnParamMoveUp
        '
        resources.ApplyResources(Me.btnParamMoveUp, "btnParamMoveUp")
        Me.btnParamMoveUp.Name = "btnParamMoveUp"
        Me.btnParamMoveUp.UseVisualStyleBackColor = False
        '
        'pnlListView
        '
        Me.pnlListView.Controls.Add(Me.objParamsList)
        resources.ApplyResources(Me.pnlListView, "pnlListView")
        Me.pnlListView.Name = "pnlListView"
        '
        'objParamsList
        '
        Me.objParamsList.AllowDrop = True
        Me.objParamsList.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.objParamsList.CurrentEditableRow = Nothing
        resources.ApplyResources(Me.objParamsList, "objParamsList")
        Me.objParamsList.FillColumn = Nothing
        Me.objParamsList.FrameBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.objParamsList.HighlightedForeColor = System.Drawing.SystemColors.HighlightText
        Me.objParamsList.HighlightedRowBackColour = System.Drawing.Color.FromArgb(CType(CType(182, Byte), Integer), CType(CType(202, Byte), Integer), CType(CType(234, Byte), Integer))
        Me.objParamsList.HighlightedRowOutline = System.Drawing.Color.Red
        Me.objParamsList.LastColumnAutoSize = True
        Me.objParamsList.MinimumColumnWidth = 200
        Me.objParamsList.Name = "objParamsList"
        Me.objParamsList.Readonly = False
        Me.objParamsList.RowHeight = 26
        Me.objParamsList.Rows.Capacity = 0
        Me.objParamsList.Sortable = False
        '
        'ctlParameterList
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.pnlListView)
        Me.Controls.Add(Me.pnlBottomButtons)
        Me.Name = "ctlParameterList"
        resources.ApplyResources(Me, "$this")
        Me.pnlBottomButtons.ResumeLayout(False)
        Me.pnlListView.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents pnlBottomButtons As Panel
    Friend WithEvents btnRemoveParam As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnAddParam As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents pnlListView As Panel
    Friend WithEvents objParamsList As ctlListView
    Friend WithEvents btnParamMoveDown As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnParamMoveUp As AutomateControls.Buttons.StandardStyledButton

End Class
