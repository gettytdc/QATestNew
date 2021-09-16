Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
''' Project  : Automate
''' Class    : frmStagePropertiesCalculation
''' 
''' <summary>
''' A calculation properties form.
''' </summary>
Friend Class frmCalculationDecisionBase
    Inherits frmProperties
    Implements IDataItemTreeRefresher

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()
    End Sub

    'Form overrides dispose to clean up the component list.
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
    Friend WithEvents mBuilder As ctlProcessExpressionBuilder   

    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmCalculationDecisionBase))
        Me.mBuilder = New AutomateUI.ctlProcessExpressionBuilder()
        Me.SuspendLayout()
        '
        'mBuilder
        '
        resources.ApplyResources(Me.mBuilder, "mBuilder")
        Me.mBuilder.ExpressionText = ""
        Me.mBuilder.Name = "mBuilder"
        Me.mBuilder.StoreInText = ""
        Me.mBuilder.StoreInVisible = True
        '
        'frmCalculationDecisionBase
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.mBuilder)
        Me.Name = "frmCalculationDecisionBase"
        Me.Controls.SetChildIndex(Me.mBuilder, 0)
        Me.Controls.SetChildIndex(Me.mTitleBar, 0)
        Me.Controls.SetChildIndex(Me.txtName, 0)
        Me.Controls.SetChildIndex(Me.txtDescription, 0)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region
    Public ReadOnly Property Stage As clsProcessStage Implements IDataItemTreeRefresher.Stage
        Get
            Return ProcessStage
        End Get
    End Property

    Public Sub Repopulate(displayStage As clsDataStage) Implements IDataItemTreeRefresher.Repopulate
        mBuilder.treeDataItems.Repopulate(displayStage)
    End Sub

    Public Sub Remove(stage As clsDataStage) Implements IDataItemTreeRefresher.Remove
        mBuilder.treeDataItems.RemoveDataItemTreeNode(stage)
    End Sub
End Class