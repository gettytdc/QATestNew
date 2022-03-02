<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmStagePropertiesReadWriteBase
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesReadWriteBase))
        Me.objDataItemTreeView = New AutomateUI.ctlDataItemTreeView()
        Me.CtlApplicationExplorer1 = New AutomateUI.ctlApplicationExplorer()
        Me.lblDataExplorer = New System.Windows.Forms.Label()
        Me.lblDataFlow = New System.Windows.Forms.Label()
        Me.lblApplicationExplorer = New System.Windows.Forms.Label()
        Me.pnlLeft = New System.Windows.Forms.Panel()
        Me.pnlMiddle = New System.Windows.Forms.Panel()
        Me.ctlListPair = New AutomateUI.ctlActionAndArgumentsListPair()
        Me.pnlRight = New System.Windows.Forms.Panel()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.pnlLeft.SuspendLayout()
        Me.pnlMiddle.SuspendLayout()
        Me.pnlRight.SuspendLayout()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.SuspendLayout()
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
        'CtlApplicationExplorer1
        '
        resources.ApplyResources(Me.CtlApplicationExplorer1, "CtlApplicationExplorer1")
        Me.CtlApplicationExplorer1.Name = "CtlApplicationExplorer1"
        Me.CtlApplicationExplorer1.ReadOnly = True
        '
        'lblDataExplorer
        '
        resources.ApplyResources(Me.lblDataExplorer, "lblDataExplorer")
        Me.lblDataExplorer.Name = "lblDataExplorer"
        '
        'lblDataFlow
        '
        resources.ApplyResources(Me.lblDataFlow, "lblDataFlow")
        Me.lblDataFlow.Name = "lblDataFlow"
        '
        'lblApplicationExplorer
        '
        resources.ApplyResources(Me.lblApplicationExplorer, "lblApplicationExplorer")
        Me.lblApplicationExplorer.Name = "lblApplicationExplorer"
        '
        'pnlLeft
        '
        Me.pnlLeft.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.pnlLeft.Controls.Add(Me.lblApplicationExplorer)
        Me.pnlLeft.Controls.Add(Me.CtlApplicationExplorer1)
        resources.ApplyResources(Me.pnlLeft, "pnlLeft")
        Me.pnlLeft.Name = "pnlLeft"
        '
        'pnlMiddle
        '
        Me.pnlMiddle.BackColor = System.Drawing.SystemColors.Control
        Me.pnlMiddle.Controls.Add(Me.ctlListPair)
        Me.pnlMiddle.Controls.Add(Me.lblDataFlow)
        resources.ApplyResources(Me.pnlMiddle, "pnlMiddle")
        Me.pnlMiddle.Name = "pnlMiddle"
        '
        'ctlListPair
        '
        Me.ctlListPair.BackColor = System.Drawing.SystemColors.ControlLightLight
        resources.ApplyResources(Me.ctlListPair, "ctlListPair")
        Me.ctlListPair.Name = "ctlListPair"
        '
        'pnlRight
        '
        Me.pnlRight.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.pnlRight.Controls.Add(Me.objDataItemTreeView)
        Me.pnlRight.Controls.Add(Me.lblDataExplorer)
        resources.ApplyResources(Me.pnlRight, "pnlRight")
        Me.pnlRight.Name = "pnlRight"
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.pnlLeft, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.pnlMiddle, 1, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.pnlRight, 2, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'frmStagePropertiesReadWriteBase
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Name = "frmStagePropertiesReadWriteBase"
        Me.Controls.SetChildIndex(Me.TableLayoutPanel1, 0)
        Me.Controls.SetChildIndex(Me.mTitleBar, 0)
        Me.Controls.SetChildIndex(Me.txtName, 0)
        Me.Controls.SetChildIndex(Me.txtDescription, 0)
        Me.pnlLeft.ResumeLayout(False)
        Me.pnlLeft.PerformLayout()
        Me.pnlMiddle.ResumeLayout(False)
        Me.pnlMiddle.PerformLayout()
        Me.pnlRight.ResumeLayout(False)
        Me.pnlRight.PerformLayout()
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents objDataItemTreeView As AutomateUI.ctlDataItemTreeView
    Friend WithEvents CtlApplicationExplorer1 As AutomateUI.ctlApplicationExplorer
    Friend WithEvents lblDataExplorer As System.Windows.Forms.Label
    Friend WithEvents lblDataFlow As System.Windows.Forms.Label
    Friend WithEvents lblApplicationExplorer As System.Windows.Forms.Label
    Friend WithEvents pnlLeft As System.Windows.Forms.Panel
    Friend WithEvents pnlMiddle As System.Windows.Forms.Panel
    Friend WithEvents pnlRight As System.Windows.Forms.Panel
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents ctlListPair As ctlActionAndArgumentsListPair

End Class

