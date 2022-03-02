Imports AutomateControls.Wizard
Imports DataPipelineOutputConfigUISettings = BluePrism.DataPipeline.UI.DataPipelineOutputConfigUISettings

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlChooseDataType
    Inherits WizardPanel

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlChooseDataType))
        Me.lblConfigName = New System.Windows.Forms.Label()
        Me.chkSessionLogs = New System.Windows.Forms.CheckBox()
        Me.chkPublishedDashboards = New System.Windows.Forms.CheckBox()
        Me.lblSelectedConfigName = New System.Windows.Forms.Label()
        Me.lblChooseDataToSend = New System.Windows.Forms.Label()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.chkCustomObjectData = New System.Windows.Forms.CheckBox()
        Me.chkWqaSnapshotData = New System.Windows.Forms.CheckBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblConfigName
        '
        resources.ApplyResources(Me.lblConfigName, "lblConfigName")
        Me.lblConfigName.Name = "lblConfigName"
        '
        'chkSessionLogs
        '
        resources.ApplyResources(Me.chkSessionLogs, "chkSessionLogs")
        Me.chkSessionLogs.Name = "chkSessionLogs"
        Me.chkSessionLogs.UseVisualStyleBackColor = True
        '
        'chkPublishedDashboards
        '
        resources.ApplyResources(Me.chkPublishedDashboards, "chkPublishedDashboards")
        Me.chkPublishedDashboards.Name = "chkPublishedDashboards"
        Me.chkPublishedDashboards.UseVisualStyleBackColor = True
        '
        'lblSelectedConfigName
        '
        resources.ApplyResources(Me.lblSelectedConfigName, "lblSelectedConfigName")
        Me.lblSelectedConfigName.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.lblSelectedConfigName.Name = "lblSelectedConfigName"
        '
        'lblChooseDataToSend
        '
        resources.ApplyResources(Me.lblChooseDataToSend, "lblChooseDataToSend")
        Me.lblChooseDataToSend.Name = "lblChooseDataToSend"
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(CType(CType(208, Byte), Integer), CType(CType(238, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.TableLayoutPanel1.Controls.Add(Me.chkCustomObjectData, 0, 4)
        Me.TableLayoutPanel1.Controls.Add(Me.chkPublishedDashboards, 0, 2)
        Me.TableLayoutPanel1.Controls.Add(Me.chkSessionLogs, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.lblChooseDataToSend, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.chkWqaSnapshotData, 0, 3)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'chkCustomObjectData
        '
        resources.ApplyResources(Me.chkCustomObjectData, "chkCustomObjectData")
        Me.chkCustomObjectData.Name = "chkCustomObjectData"
        Me.chkCustomObjectData.UseVisualStyleBackColor = True
        '
        'chkWqaSnapshotData
        '
        resources.ApplyResources(Me.chkWqaSnapshotData, "chkWqaSnapshotData")
        Me.chkWqaSnapshotData.Name = "chkWqaSnapshotData"
        Me.chkWqaSnapshotData.UseVisualStyleBackColor = True
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'ctlChooseDataType
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Controls.Add(Me.lblSelectedConfigName)
        Me.Controls.Add(Me.lblConfigName)
        resources.ApplyResources(Me, "$this")
        Me.Name = "ctlChooseDataType"
        Me.NavigatePrevious = True
        Me.Title = Global.AutomateUI.My.Resources.Resources.ctlChooseDataType_title
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblConfigName As Label
    Friend WithEvents chkSessionLogs As CheckBox
    Friend WithEvents chkPublishedDashboards As CheckBox
    Friend WithEvents lblSelectedConfigName As Label
    Friend WithEvents lblChooseDataToSend As Label
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents Label1 As Label
    Friend WithEvents chkCustomObjectData As CheckBox
    Friend WithEvents chkWqaSnapshotData As CheckBox
End Class
