<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Friend Class frmStagePropertiesNavigate
    Inherits frmProperties

    'Form overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesNavigate))
        Me.ctlApplicationExplorer = New AutomateUI.ctlApplicationExplorer()
        Me.lblApplicationExplorer = New System.Windows.Forms.Label()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.ctlListPair = New AutomateUI.ctlActionAndArgumentsListPair()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.objDataItemTreeView = New AutomateUI.ctlDataItemTreeView()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
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
        'mTitleBar
        '
        resources.ApplyResources(Me.mTitleBar, "mTitleBar")
        '
        'ctlApplicationExplorer
        '
        resources.ApplyResources(Me.ctlApplicationExplorer, "ctlApplicationExplorer")
        Me.ctlApplicationExplorer.Name = "ctlApplicationExplorer"
        Me.ctlApplicationExplorer.ReadOnly = True
        '
        'lblApplicationExplorer
        '
        resources.ApplyResources(Me.lblApplicationExplorer, "lblApplicationExplorer")
        Me.lblApplicationExplorer.Name = "lblApplicationExplorer"
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.Panel1, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.ctlListPair, 1, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.Panel2, 2, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.lblApplicationExplorer)
        Me.Panel1.Controls.Add(Me.ctlApplicationExplorer)
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Name = "Panel1"
        '
        'ctlListPair
        '
        resources.ApplyResources(Me.ctlListPair, "ctlListPair")
        Me.ctlListPair.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.ctlListPair.Name = "ctlListPair"
        Me.ctlListPair.PauseAfterStepVisible = True
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.Label2)
        Me.Panel2.Controls.Add(Me.objDataItemTreeView)
        resources.ApplyResources(Me.Panel2, "Panel2")
        Me.Panel2.Name = "Panel2"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
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
        'frmStagePropertiesNavigate
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Name = "frmStagePropertiesNavigate"
        Me.Controls.SetChildIndex(Me.TableLayoutPanel1, 0)
        Me.Controls.SetChildIndex(Me.mTitleBar, 0)
        Me.Controls.SetChildIndex(Me.txtName, 0)
        Me.Controls.SetChildIndex(Me.txtDescription, 0)
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel1.PerformLayout()
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ctlApplicationExplorer As AutomateUI.ctlApplicationExplorer
    Friend WithEvents lblApplicationExplorer As System.Windows.Forms.Label
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents ctlListPair As ctlActionAndArgumentsListPair
    Friend WithEvents objDataItemTreeView As ctlDataItemTreeView
    Friend WithEvents Label2 As Label
    Friend WithEvents Panel2 As Panel
End Class
