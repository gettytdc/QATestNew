<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmStagePropertiesException
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesException))
        Me.objExceptionDetailExpressionEdit = New AutomateUI.ctlExpressionEdit()
        Me.lblExceptionType = New System.Windows.Forms.Label()
        Me.lblExceptionDetail = New System.Windows.Forms.Label()
        Me.cmbExceptionType = New System.Windows.Forms.ComboBox()
        Me.chkSameException = New System.Windows.Forms.CheckBox()
        Me.CtlDataTypeTips1 = New AutomateUI.ctlDataTypeTips()
        Me.chkSaveScreenCapture = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout
        '
        'mTitleBar
        '
        resources.ApplyResources(Me.mTitleBar, "mTitleBar")
        '
        'objExceptionDetailExpressionEdit
        '
        resources.ApplyResources(Me.objExceptionDetailExpressionEdit, "objExceptionDetailExpressionEdit")
        Me.objExceptionDetailExpressionEdit.Border = true
        Me.objExceptionDetailExpressionEdit.HighlightingEnabled = true
        Me.objExceptionDetailExpressionEdit.IsDecision = false
        Me.objExceptionDetailExpressionEdit.Name = "objExceptionDetailExpressionEdit"
        Me.objExceptionDetailExpressionEdit.PasswordChar = ChrW(0)
        Me.objExceptionDetailExpressionEdit.Stage = Nothing
        '
        'lblExceptionType
        '
        resources.ApplyResources(Me.lblExceptionType, "lblExceptionType")
        Me.lblExceptionType.Name = "lblExceptionType"
        '
        'lblExceptionDetail
        '
        resources.ApplyResources(Me.lblExceptionDetail, "lblExceptionDetail")
        Me.lblExceptionDetail.Name = "lblExceptionDetail"
        '
        'cmbExceptionType
        '
        resources.ApplyResources(Me.cmbExceptionType, "cmbExceptionType")
        Me.cmbExceptionType.FormattingEnabled = true
        Me.cmbExceptionType.Name = "cmbExceptionType"
        '
        'chkSameException
        '
        resources.ApplyResources(Me.chkSameException, "chkSameException")
        Me.chkSameException.Name = "chkSameException"
        Me.chkSameException.UseVisualStyleBackColor = true
        '
        'CtlDataTypeTips1
        '
        resources.ApplyResources(Me.CtlDataTypeTips1, "CtlDataTypeTips1")
        Me.CtlDataTypeTips1.BackColor = System.Drawing.Color.White
        Me.CtlDataTypeTips1.Name = "CtlDataTypeTips1"
        Me.CtlDataTypeTips1.TabStop = false
        '
        'chkSaveScreenCapture
        '
        resources.ApplyResources(Me.chkSaveScreenCapture, "chkSaveScreenCapture")
        Me.chkSaveScreenCapture.Name = "chkSaveScreenCapture"
        Me.chkSaveScreenCapture.UseVisualStyleBackColor = true
        '
        'frmStagePropertiesException
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.chkSaveScreenCapture)
        Me.Controls.Add(Me.CtlDataTypeTips1)
        Me.Controls.Add(Me.chkSameException)
        Me.Controls.Add(Me.cmbExceptionType)
        Me.Controls.Add(Me.lblExceptionDetail)
        Me.Controls.Add(Me.objExceptionDetailExpressionEdit)
        Me.Controls.Add(Me.lblExceptionType)
        Me.Name = "frmStagePropertiesException"
        Me.Controls.SetChildIndex(Me.lblExceptionType, 0)
        Me.Controls.SetChildIndex(Me.objExceptionDetailExpressionEdit, 0)
        Me.Controls.SetChildIndex(Me.mTitleBar, 0)
        Me.Controls.SetChildIndex(Me.txtName, 0)
        Me.Controls.SetChildIndex(Me.txtDescription, 0)
        Me.Controls.SetChildIndex(Me.lblExceptionDetail, 0)
        Me.Controls.SetChildIndex(Me.cmbExceptionType, 0)
        Me.Controls.SetChildIndex(Me.chkSameException, 0)
        Me.Controls.SetChildIndex(Me.CtlDataTypeTips1, 0)
        Me.Controls.SetChildIndex(Me.chkSaveScreenCapture, 0)
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Friend WithEvents objExceptionDetailExpressionEdit As AutomateUI.ctlExpressionEdit
    Friend WithEvents lblExceptionType As System.Windows.Forms.Label
    Friend WithEvents lblExceptionDetail As System.Windows.Forms.Label
    Friend WithEvents cmbExceptionType As System.Windows.Forms.ComboBox
    Friend WithEvents chkSameException As System.Windows.Forms.CheckBox
    Friend WithEvents CtlDataTypeTips1 As AutomateUI.ctlDataTypeTips
    Friend WithEvents chkSaveScreenCapture As System.Windows.Forms.CheckBox

End Class