<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmElementParams
    Inherits frmWizard

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmElementParams))
        Me.ctlParamsList = New AutomateUI.ctlListView()
        Me.CtlDataItemTreeView1 = New AutomateUI.ctlDataItemTreeView()
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.SuspendLayout()
        '
        'objBluebar
        '
        Me.objBluebar.Title = "Provide dynamic data from your process to identify the current application elemen" &
    "t"
        '
        'ctlParamsList
        '
        Me.ctlParamsList.AllowDrop = True
        Me.ctlParamsList.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.ctlParamsList.CurrentEditableRow = Nothing
        resources.ApplyResources(Me.ctlParamsList, "ctlParamsList")
        Me.ctlParamsList.FillColumn = Nothing
        Me.ctlParamsList.FrameBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.ctlParamsList.HighlightedForeColor = System.Drawing.SystemColors.HighlightText
        Me.ctlParamsList.HighlightedRowBackColour = System.Drawing.Color.FromArgb(CType(CType(182, Byte), Integer), CType(CType(202, Byte), Integer), CType(CType(234, Byte), Integer))
        Me.ctlParamsList.HighlightedRowOutline = System.Drawing.SystemColors.Highlight
        Me.ctlParamsList.LastColumnAutoSize = False
        Me.ctlParamsList.MinimumColumnWidth = 200
        Me.ctlParamsList.Name = "ctlParamsList"
        Me.ctlParamsList.Readonly = False
        Me.ctlParamsList.RowHeight = 26
        Me.ctlParamsList.Rows.Capacity = 0
        Me.ctlParamsList.Sortable = False
        '
        'CtlDataItemTreeView1
        '
        Me.CtlDataItemTreeView1.CheckBoxes = False
        resources.ApplyResources(Me.CtlDataItemTreeView1, "CtlDataItemTreeView1")
        Me.CtlDataItemTreeView1.IgnoreScope = False
        Me.CtlDataItemTreeView1.Name = "CtlDataItemTreeView1"
        Me.CtlDataItemTreeView1.Stage = Nothing
        Me.CtlDataItemTreeView1.StatisticsMode = False
        '
        'SplitContainer1
        '
        resources.ApplyResources(Me.SplitContainer1, "SplitContainer1")
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.ctlParamsList)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.CtlDataItemTreeView1)
        '
        'frmElementParams
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.SplitContainer1)
        Me.Name = "frmElementParams"
        Me.Title = "Provide dynamic data from your process to identify the current application elemen" &
    "t"
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.Controls.SetChildIndex(Me.SplitContainer1, 0)
        Me.Controls.SetChildIndex(Me.btnBack, 0)
        Me.Controls.SetChildIndex(Me.btnNext, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlParamsList As AutomateUI.ctlListView
    Friend WithEvents CtlDataItemTreeView1 As AutomateUI.ctlDataItemTreeView
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
End Class
