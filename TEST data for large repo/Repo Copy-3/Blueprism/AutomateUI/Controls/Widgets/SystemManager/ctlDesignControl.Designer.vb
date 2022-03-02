<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlDesignControl
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlDesignControl))
        Me.llImport = New AutomateControls.BulletedLinkLabel()
        Me.llExport = New AutomateControls.BulletedLinkLabel()
        Me.panMain = New System.Windows.Forms.TableLayoutPanel()
        Me.lblDesignControl = New System.Windows.Forms.Label()
        Me.lblSeverity = New System.Windows.Forms.Label()
        Me.lblCategory = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.cbValidateOnOpen = New System.Windows.Forms.CheckBox()
        Me.cbValidateOnReset = New System.Windows.Forms.CheckBox()
        Me.cbValidateOnSave = New System.Windows.Forms.CheckBox()
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.FlowLayoutPanel2 = New System.Windows.Forms.FlowLayoutPanel()
        Me.panMain.SuspendLayout()
        Me.FlowLayoutPanel1.SuspendLayout()
        Me.FlowLayoutPanel2.SuspendLayout()
        Me.SuspendLayout()
        '
        'llImport
        '
        resources.ApplyResources(Me.llImport, "llImport")
        Me.llImport.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llImport.Name = "llImport"
        Me.llImport.TabStop = True
        '
        'llExport
        '
        resources.ApplyResources(Me.llExport, "llExport")
        Me.llExport.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llExport.Name = "llExport"
        Me.llExport.TabStop = True
        '
        'panMain
        '
        resources.ApplyResources(Me.panMain, "panMain")
        Me.panMain.Controls.Add(Me.lblDesignControl, 2, 0)
        Me.panMain.Controls.Add(Me.lblSeverity, 1, 0)
        Me.panMain.Controls.Add(Me.lblCategory, 0, 0)
        Me.panMain.Name = "panMain"
        '
        'lblDesignControl
        '
        resources.ApplyResources(Me.lblDesignControl, "lblDesignControl")
        Me.lblDesignControl.Name = "lblDesignControl"
        '
        'lblSeverity
        '
        resources.ApplyResources(Me.lblSeverity, "lblSeverity")
        Me.lblSeverity.Name = "lblSeverity"
        '
        'lblCategory
        '
        resources.ApplyResources(Me.lblCategory, "lblCategory")
        Me.lblCategory.Name = "lblCategory"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'cbValidateOnOpen
        '
        resources.ApplyResources(Me.cbValidateOnOpen, "cbValidateOnOpen")
        Me.cbValidateOnOpen.Name = "cbValidateOnOpen"
        Me.cbValidateOnOpen.Tag = ""
        Me.cbValidateOnOpen.UseVisualStyleBackColor = True
        '
        'cbValidateOnReset
        '
        resources.ApplyResources(Me.cbValidateOnReset, "cbValidateOnReset")
        Me.cbValidateOnReset.Name = "cbValidateOnReset"
        Me.cbValidateOnReset.Tag = ""
        Me.cbValidateOnReset.UseVisualStyleBackColor = True
        '
        'cbValidateOnSave
        '
        resources.ApplyResources(Me.cbValidateOnSave, "cbValidateOnSave")
        Me.cbValidateOnSave.Name = "cbValidateOnSave"
        Me.cbValidateOnSave.Tag = ""
        Me.cbValidateOnSave.UseVisualStyleBackColor = True
        '
        'FlowLayoutPanel1
        '
        resources.ApplyResources(Me.FlowLayoutPanel1, "FlowLayoutPanel1")
        Me.FlowLayoutPanel1.Controls.Add(Me.Label1)
        Me.FlowLayoutPanel1.Controls.Add(Me.cbValidateOnSave)
        Me.FlowLayoutPanel1.Controls.Add(Me.cbValidateOnReset)
        Me.FlowLayoutPanel1.Controls.Add(Me.cbValidateOnOpen)
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        '
        'FlowLayoutPanel2
        '
        resources.ApplyResources(Me.FlowLayoutPanel2, "FlowLayoutPanel2")
        Me.FlowLayoutPanel2.Controls.Add(Me.llImport)
        Me.FlowLayoutPanel2.Controls.Add(Me.llExport)
        Me.FlowLayoutPanel2.Name = "FlowLayoutPanel2"
        '
        'ctlDesignControl
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.FlowLayoutPanel2)
        Me.Controls.Add(Me.FlowLayoutPanel1)
        Me.Controls.Add(Me.panMain)
        Me.Name = "ctlDesignControl"
        resources.ApplyResources(Me, "$this")
        Me.panMain.ResumeLayout(False)
        Me.FlowLayoutPanel1.ResumeLayout(False)
        Me.FlowLayoutPanel1.PerformLayout()
        Me.FlowLayoutPanel2.ResumeLayout(False)
        Me.FlowLayoutPanel2.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents lblCategory As System.Windows.Forms.Label
    Private WithEvents lblSeverity As System.Windows.Forms.Label
    Private WithEvents lblDesignControl As System.Windows.Forms.Label
    Private WithEvents llImport As AutomateControls.BulletedLinkLabel
    Private WithEvents llExport As AutomateControls.BulletedLinkLabel
    Private WithEvents panMain As System.Windows.Forms.TableLayoutPanel
    Private WithEvents Label1 As System.Windows.Forms.Label
    Private WithEvents cbValidateOnOpen As System.Windows.Forms.CheckBox
    Private WithEvents cbValidateOnReset As System.Windows.Forms.CheckBox
    Private WithEvents cbValidateOnSave As System.Windows.Forms.CheckBox
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
    Friend WithEvents FlowLayoutPanel2 As FlowLayoutPanel
End Class
