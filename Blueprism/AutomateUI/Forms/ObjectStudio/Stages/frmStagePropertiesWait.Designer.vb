<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Friend Class frmStagePropertiesWait
    Inherits AutomateUI.frmProperties

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesWait))
        Me.objApplicationExplorer = New AutomateUI.ctlApplicationExplorer()
        Me.objDataItemTreeView = New AutomateUI.ctlDataItemTreeView()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.ctlTimeout = New AutomateUI.ctlExpressionEdit()
        Me.lblApplicationExplorer = New System.Windows.Forms.Label()
        Me.lblDataExplorer = New System.Windows.Forms.Label()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.pnlLeft = New System.Windows.Forms.Panel()
        Me.pnlMiddle = New System.Windows.Forms.Panel()
        Me.ctlListPair = New AutomateUI.ctlActionAndArgumentsListPair()
        Me.pnlRight = New System.Windows.Forms.Panel()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.pnlLeft.SuspendLayout()
        Me.pnlMiddle.SuspendLayout()
        Me.pnlRight.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        '
        'txtDescription
        '
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        '
        'objApplicationExplorer
        '
        resources.ApplyResources(Me.objApplicationExplorer, "objApplicationExplorer")
        Me.objApplicationExplorer.Name = "objApplicationExplorer"
        Me.objApplicationExplorer.ReadOnly = True
        '
        'objDataItemTreeView
        '
        resources.ApplyResources(Me.objDataItemTreeView, "objDataItemTreeView")
        Me.objDataItemTreeView.CheckBoxes = False
        Me.objDataItemTreeView.IgnoreScope = False
        Me.objDataItemTreeView.Name = "objDataItemTreeView"
        Me.objDataItemTreeView.Stage = Nothing
        Me.objDataItemTreeView.StatisticsMode = False
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'ctlTimeout
        '
        Me.ctlTimeout.AllowDrop = True
        resources.ApplyResources(Me.ctlTimeout, "ctlTimeout")
        Me.ctlTimeout.Border = True
        Me.ctlTimeout.HighlightingEnabled = True
        Me.ctlTimeout.IsDecision = False
        Me.ctlTimeout.Name = "ctlTimeout"
        Me.ctlTimeout.PasswordChar = ChrW(0)
        Me.ctlTimeout.Stage = Nothing
        '
        'lblApplicationExplorer
        '
        resources.ApplyResources(Me.lblApplicationExplorer, "lblApplicationExplorer")
        Me.lblApplicationExplorer.Name = "lblApplicationExplorer"
        '
        'lblDataExplorer
        '
        resources.ApplyResources(Me.lblDataExplorer, "lblDataExplorer")
        Me.lblDataExplorer.Name = "lblDataExplorer"
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.pnlLeft, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.pnlMiddle, 1, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.pnlRight, 2, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'pnlLeft
        '
        Me.pnlLeft.Controls.Add(Me.objApplicationExplorer)
        Me.pnlLeft.Controls.Add(Me.lblApplicationExplorer)
        resources.ApplyResources(Me.pnlLeft, "pnlLeft")
        Me.pnlLeft.Name = "pnlLeft"
        '
        'pnlMiddle
        '
        Me.pnlMiddle.Controls.Add(Me.ctlListPair)
        Me.pnlMiddle.Controls.Add(Me.Label1)
        Me.pnlMiddle.Controls.Add(Me.ctlTimeout)
        resources.ApplyResources(Me.pnlMiddle, "pnlMiddle")
        Me.pnlMiddle.Name = "pnlMiddle"
        '
        'ctlListPair
        '
        resources.ApplyResources(Me.ctlListPair, "ctlListPair")
        Me.ctlListPair.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.ctlListPair.Name = "ctlListPair"
        '
        'pnlRight
        '
        Me.pnlRight.Controls.Add(Me.objDataItemTreeView)
        Me.pnlRight.Controls.Add(Me.lblDataExplorer)
        resources.ApplyResources(Me.pnlRight, "pnlRight")
        Me.pnlRight.Name = "pnlRight"
        '
        'frmStagePropertiesWait
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Name = "frmStagePropertiesWait"
        Me.Controls.SetChildIndex(Me.mTitleBar, 0)
        Me.Controls.SetChildIndex(Me.txtName, 0)
        Me.Controls.SetChildIndex(Me.txtDescription, 0)
        Me.Controls.SetChildIndex(Me.TableLayoutPanel1, 0)
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.pnlLeft.ResumeLayout(False)
        Me.pnlLeft.PerformLayout()
        Me.pnlMiddle.ResumeLayout(False)
        Me.pnlMiddle.PerformLayout()
        Me.pnlRight.ResumeLayout(False)
        Me.pnlRight.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents objApplicationExplorer As AutomateUI.ctlApplicationExplorer
    Friend WithEvents objDataItemTreeView As AutomateUI.ctlDataItemTreeView
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents ctlTimeout As AutomateUI.ctlExpressionEdit
    Friend WithEvents lblApplicationExplorer As System.Windows.Forms.Label
    Friend WithEvents lblDataExplorer As System.Windows.Forms.Label
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents pnlLeft As System.Windows.Forms.Panel
    Friend WithEvents pnlMiddle As System.Windows.Forms.Panel
    Friend WithEvents pnlRight As System.Windows.Forms.Panel
    Friend WithEvents ctlListPair As AutomateUI.ctlActionAndArgumentsListPair

End Class
