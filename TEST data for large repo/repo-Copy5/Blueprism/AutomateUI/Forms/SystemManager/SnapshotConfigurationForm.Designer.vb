Imports AutomateUI.Controls.Widgets.SystemManager.WorkQueueAnalysis

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SnapshotConfigurationForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SnapshotConfigurationForm))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.btnClose = New System.Windows.Forms.PictureBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.chkIsEnabled = New System.Windows.Forms.CheckBox()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.BorderPanel = New AutomateUI.Controls.Widgets.SystemManager.WorkQueueAnalysis.NoneAutoScrollingFlowLayoutPanel()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtConfigName = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.cboInterval = New System.Windows.Forms.ComboBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.cboTimezone = New System.Windows.Forms.ComboBox()
        Me.chkSpecificTimes = New System.Windows.Forms.CheckBox()
        Me.pnlSpecificTimes = New System.Windows.Forms.Panel()
        Me.cboEndTime = New System.Windows.Forms.ComboBox()
        Me.cboStartTime = New System.Windows.Forms.ComboBox()
        Me.lblStart = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.chkSpecificDays = New System.Windows.Forms.CheckBox()
        Me.pnlSpecificDays = New System.Windows.Forms.Panel()
        Me.chkMonday = New System.Windows.Forms.CheckBox()
        Me.chkMonToFri = New System.Windows.Forms.CheckBox()
        Me.chkTuesday = New System.Windows.Forms.CheckBox()
        Me.chkSatToSun = New System.Windows.Forms.CheckBox()
        Me.chkThursday = New System.Windows.Forms.CheckBox()
        Me.chkWednesday = New System.Windows.Forms.CheckBox()
        Me.chkSunday = New System.Windows.Forms.CheckBox()
        Me.chkFriday = New System.Windows.Forms.CheckBox()
        Me.chkSaturday = New System.Windows.Forms.CheckBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.btnEnableAll = New AutomateControls.BulletedLinkLabel()
        Me.btnEnableSelected = New AutomateControls.BulletedLinkLabel()
        Me.dgvQueues = New System.Windows.Forms.DataGridView()
        CType(Me.btnClose,System.ComponentModel.ISupportInitialize).BeginInit
        Me.TableLayoutPanel1.SuspendLayout
        Me.Panel2.SuspendLayout
        Me.Panel1.SuspendLayout
        Me.BorderPanel.SuspendLayout
        Me.pnlSpecificTimes.SuspendLayout
        Me.pnlSpecificDays.SuspendLayout
        CType(Me.dgvQueues,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'btnClose
        '
        resources.ApplyResources(Me.btnClose, "btnClose")
        Me.btnClose.Image = Global.AutomateUI.ActivationWizardResources.Close_32x32
        Me.btnClose.Name = "btnClose"
        Me.btnClose.TabStop = false
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.Panel2, 0, 2)
        Me.TableLayoutPanel1.Controls.Add(Me.Panel1, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.BorderPanel, 0, 1)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'Panel2
        '
        Me.Panel2.BackColor = System.Drawing.SystemColors.Window
        Me.Panel2.Controls.Add(Me.chkIsEnabled)
        Me.Panel2.Controls.Add(Me.btnOK)
        Me.Panel2.Controls.Add(Me.btnCancel)
        resources.ApplyResources(Me.Panel2, "Panel2")
        Me.Panel2.Name = "Panel2"
        '
        'chkIsEnabled
        '
        resources.ApplyResources(Me.chkIsEnabled, "chkIsEnabled")
        Me.chkIsEnabled.Name = "chkIsEnabled"
        Me.chkIsEnabled.UseVisualStyleBackColor = true
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        Me.btnOK.UseVisualStyleBackColor = false
        '
        'btnCancel
        '
        Me.btnCancel.ForeColor = System.Drawing.Color.Black
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = false
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.SystemColors.Window
        Me.Panel1.Controls.Add(Me.Label3)
        Me.Panel1.Controls.Add(Me.Label1)
        Me.Panel1.Controls.Add(Me.btnClose)
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Name = "Panel1"
        '
        'BorderPanel
        '
        resources.ApplyResources(Me.BorderPanel, "BorderPanel")
        Me.BorderPanel.BackColor = System.Drawing.SystemColors.Window
        Me.BorderPanel.Controls.Add(Me.Label2)
        Me.BorderPanel.Controls.Add(Me.txtConfigName)
        Me.BorderPanel.Controls.Add(Me.Label4)
        Me.BorderPanel.Controls.Add(Me.cboInterval)
        Me.BorderPanel.Controls.Add(Me.Label5)
        Me.BorderPanel.Controls.Add(Me.cboTimezone)
        Me.BorderPanel.Controls.Add(Me.chkSpecificTimes)
        Me.BorderPanel.Controls.Add(Me.pnlSpecificTimes)
        Me.BorderPanel.Controls.Add(Me.chkSpecificDays)
        Me.BorderPanel.Controls.Add(Me.pnlSpecificDays)
        Me.BorderPanel.Controls.Add(Me.Label6)
        Me.BorderPanel.Controls.Add(Me.btnEnableAll)
        Me.BorderPanel.Controls.Add(Me.btnEnableSelected)
        Me.BorderPanel.Controls.Add(Me.dgvQueues)
        Me.BorderPanel.Name = "BorderPanel"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.BorderPanel.SetFlowBreak(Me.Label2, true)
        Me.Label2.Name = "Label2"
        '
        'txtConfigName
        '
        Me.BorderPanel.SetFlowBreak(Me.txtConfigName, true)
        resources.ApplyResources(Me.txtConfigName, "txtConfigName")
        Me.txtConfigName.Name = "txtConfigName"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.BorderPanel.SetFlowBreak(Me.Label4, true)
        Me.Label4.Name = "Label4"
        '
        'cboInterval
        '
        Me.cboInterval.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.BorderPanel.SetFlowBreak(Me.cboInterval, true)
        Me.cboInterval.FormattingEnabled = true
        resources.ApplyResources(Me.cboInterval, "cboInterval")
        Me.cboInterval.Name = "cboInterval"
        '
        'Label5
        '
        resources.ApplyResources(Me.Label5, "Label5")
        Me.BorderPanel.SetFlowBreak(Me.Label5, true)
        Me.Label5.Name = "Label5"
        '
        'cboTimezone
        '
        Me.cboTimezone.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.BorderPanel.SetFlowBreak(Me.cboTimezone, true)
        Me.cboTimezone.FormattingEnabled = true
        resources.ApplyResources(Me.cboTimezone, "cboTimezone")
        Me.cboTimezone.Name = "cboTimezone"
        '
        'chkSpecificTimes
        '
        resources.ApplyResources(Me.chkSpecificTimes, "chkSpecificTimes")
        Me.BorderPanel.SetFlowBreak(Me.chkSpecificTimes, true)
        Me.chkSpecificTimes.Name = "chkSpecificTimes"
        Me.chkSpecificTimes.UseVisualStyleBackColor = true
        '
        'pnlSpecificTimes
        '
        Me.pnlSpecificTimes.BackColor = System.Drawing.Color.FromArgb(CType(CType(208,Byte),Integer), CType(CType(237,Byte),Integer), CType(CType(255,Byte),Integer))
        Me.pnlSpecificTimes.Controls.Add(Me.cboEndTime)
        Me.pnlSpecificTimes.Controls.Add(Me.cboStartTime)
        Me.pnlSpecificTimes.Controls.Add(Me.lblStart)
        Me.pnlSpecificTimes.Controls.Add(Me.Label7)
        resources.ApplyResources(Me.pnlSpecificTimes, "pnlSpecificTimes")
        Me.pnlSpecificTimes.Name = "pnlSpecificTimes"
        '
        'cboEndTime
        '
        Me.cboEndTime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboEndTime.FormattingEnabled = true
        resources.ApplyResources(Me.cboEndTime, "cboEndTime")
        Me.cboEndTime.Name = "cboEndTime"
        '
        'cboStartTime
        '
        Me.cboStartTime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboStartTime.FormattingEnabled = true
        resources.ApplyResources(Me.cboStartTime, "cboStartTime")
        Me.cboStartTime.Name = "cboStartTime"
        '
        'lblStart
        '
        resources.ApplyResources(Me.lblStart, "lblStart")
        Me.lblStart.Name = "lblStart"
        '
        'Label7
        '
        resources.ApplyResources(Me.Label7, "Label7")
        Me.Label7.Name = "Label7"
        '
        'chkSpecificDays
        '
        resources.ApplyResources(Me.chkSpecificDays, "chkSpecificDays")
        Me.BorderPanel.SetFlowBreak(Me.chkSpecificDays, true)
        Me.chkSpecificDays.Name = "chkSpecificDays"
        Me.chkSpecificDays.UseVisualStyleBackColor = true
        '
        'pnlSpecificDays
        '
        Me.pnlSpecificDays.BackColor = System.Drawing.Color.FromArgb(CType(CType(208,Byte),Integer), CType(CType(237,Byte),Integer), CType(CType(255,Byte),Integer))
        Me.pnlSpecificDays.Controls.Add(Me.chkMonday)
        Me.pnlSpecificDays.Controls.Add(Me.chkMonToFri)
        Me.pnlSpecificDays.Controls.Add(Me.chkTuesday)
        Me.pnlSpecificDays.Controls.Add(Me.chkSatToSun)
        Me.pnlSpecificDays.Controls.Add(Me.chkThursday)
        Me.pnlSpecificDays.Controls.Add(Me.chkWednesday)
        Me.pnlSpecificDays.Controls.Add(Me.chkSunday)
        Me.pnlSpecificDays.Controls.Add(Me.chkFriday)
        Me.pnlSpecificDays.Controls.Add(Me.chkSaturday)
        resources.ApplyResources(Me.pnlSpecificDays, "pnlSpecificDays")
        Me.pnlSpecificDays.Name = "pnlSpecificDays"
        '
        'chkMonday
        '
        resources.ApplyResources(Me.chkMonday, "chkMonday")
        Me.chkMonday.Name = "chkMonday"
        Me.chkMonday.UseVisualStyleBackColor = true
        '
        'chkMonToFri
        '
        resources.ApplyResources(Me.chkMonToFri, "chkMonToFri")
        Me.chkMonToFri.Name = "chkMonToFri"
        Me.chkMonToFri.UseVisualStyleBackColor = true
        '
        'chkTuesday
        '
        resources.ApplyResources(Me.chkTuesday, "chkTuesday")
        Me.chkTuesday.Name = "chkTuesday"
        Me.chkTuesday.UseVisualStyleBackColor = true
        '
        'chkSatToSun
        '
        resources.ApplyResources(Me.chkSatToSun, "chkSatToSun")
        Me.chkSatToSun.Name = "chkSatToSun"
        Me.chkSatToSun.UseVisualStyleBackColor = true
        '
        'chkThursday
        '
        resources.ApplyResources(Me.chkThursday, "chkThursday")
        Me.chkThursday.Name = "chkThursday"
        Me.chkThursday.UseVisualStyleBackColor = true
        '
        'chkWednesday
        '
        resources.ApplyResources(Me.chkWednesday, "chkWednesday")
        Me.chkWednesday.Name = "chkWednesday"
        Me.chkWednesday.UseVisualStyleBackColor = true
        '
        'chkSunday
        '
        resources.ApplyResources(Me.chkSunday, "chkSunday")
        Me.chkSunday.Name = "chkSunday"
        Me.chkSunday.UseVisualStyleBackColor = true
        '
        'chkFriday
        '
        resources.ApplyResources(Me.chkFriday, "chkFriday")
        Me.chkFriday.Name = "chkFriday"
        Me.chkFriday.UseVisualStyleBackColor = true
        '
        'chkSaturday
        '
        resources.ApplyResources(Me.chkSaturday, "chkSaturday")
        Me.chkSaturday.Name = "chkSaturday"
        Me.chkSaturday.UseVisualStyleBackColor = true
        '
        'Label6
        '
        resources.ApplyResources(Me.Label6, "Label6")
        Me.BorderPanel.SetFlowBreak(Me.Label6, true)
        Me.Label6.Name = "Label6"
        '
        'btnEnableAll
        '
        resources.ApplyResources(Me.btnEnableAll, "btnEnableAll")
        Me.btnEnableAll.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64,Byte),Integer), CType(CType(64,Byte),Integer), CType(CType(64,Byte),Integer))
        Me.btnEnableAll.Name = "btnEnableAll"
        Me.btnEnableAll.TabStop = true
        Me.btnEnableAll.UseCompatibleTextRendering = true
        '
        'btnEnableSelected
        '
        resources.ApplyResources(Me.btnEnableSelected, "btnEnableSelected")
        Me.btnEnableSelected.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64,Byte),Integer), CType(CType(64,Byte),Integer), CType(CType(64,Byte),Integer))
        Me.btnEnableSelected.Name = "btnEnableSelected"
        Me.btnEnableSelected.TabStop = true
        Me.btnEnableSelected.UseCompatibleTextRendering = true
        '
        'dgvQueues
        '
        Me.dgvQueues.AllowUserToAddRows = false
        Me.dgvQueues.AllowUserToDeleteRows = false
        Me.dgvQueues.AllowUserToResizeRows = false
        resources.ApplyResources(Me.dgvQueues, "dgvQueues")
        Me.dgvQueues.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvQueues.BackgroundColor = System.Drawing.Color.FromArgb(CType(CType(208,Byte),Integer), CType(CType(237,Byte),Integer), CType(CType(255,Byte),Integer))
        Me.dgvQueues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvQueues.GridColor = System.Drawing.SystemColors.ControlLight
        Me.dgvQueues.Name = "dgvQueues"
        Me.dgvQueues.RowHeadersVisible = false
        Me.dgvQueues.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomLeft
        Me.dgvQueues.RowsDefaultCellStyle = DataGridViewCellStyle1
        Me.dgvQueues.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvQueues.ShowEditingIcon = false
        '
        'SnapshotConfigurationForm
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.CausesValidation = false
        resources.ApplyResources(Me, "$this")
        Me.ControlBox = false
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "SnapshotConfigurationForm"
        Me.ShowInTaskbar = false
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show
        CType(Me.btnClose,System.ComponentModel.ISupportInitialize).EndInit
        Me.TableLayoutPanel1.ResumeLayout(false)
        Me.Panel2.ResumeLayout(false)
        Me.Panel2.PerformLayout
        Me.Panel1.ResumeLayout(false)
        Me.Panel1.PerformLayout
        Me.BorderPanel.ResumeLayout(false)
        Me.BorderPanel.PerformLayout
        Me.pnlSpecificTimes.ResumeLayout(false)
        Me.pnlSpecificTimes.PerformLayout
        Me.pnlSpecificDays.ResumeLayout(false)
        Me.pnlSpecificDays.PerformLayout
        CType(Me.dgvQueues,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)

End Sub
    Friend WithEvents Label1 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents btnClose As PictureBox
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents Panel2 As Panel
    Friend WithEvents chkIsEnabled As CheckBox
    Friend WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents Panel1 As Panel
    Friend WithEvents BorderPanel As NoneAutoScrollingFlowLayoutPanel
    Friend WithEvents Label2 As Label
    Friend WithEvents txtConfigName As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents Label4 As Label
    Friend WithEvents cboInterval As ComboBox
    Friend WithEvents Label5 As Label
    Friend WithEvents cboTimezone As ComboBox
    Friend WithEvents chkSpecificTimes As CheckBox
    Friend WithEvents pnlSpecificTimes As Panel
    Friend WithEvents cboEndTime As ComboBox
    Friend WithEvents cboStartTime As ComboBox
    Friend WithEvents lblStart As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents chkSpecificDays As CheckBox
    Friend WithEvents pnlSpecificDays As Panel
    Friend WithEvents chkMonday As CheckBox
    Friend WithEvents chkMonToFri As CheckBox
    Friend WithEvents chkTuesday As CheckBox
    Friend WithEvents chkSatToSun As CheckBox
    Friend WithEvents chkThursday As CheckBox
    Friend WithEvents chkWednesday As CheckBox
    Friend WithEvents chkSunday As CheckBox
    Friend WithEvents chkFriday As CheckBox
    Friend WithEvents chkSaturday As CheckBox
    Friend WithEvents Label6 As Label
    Friend WithEvents btnEnableAll As AutomateControls.BulletedLinkLabel
    Friend WithEvents btnEnableSelected As AutomateControls.BulletedLinkLabel
    Friend WithEvents dgvQueues As DataGridView
End Class
