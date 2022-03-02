<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlRunHourly
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
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
        Dim mRunMonthlyGroupPanel As System.Windows.Forms.GroupBox
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlRunHourly))
        Me.HourlyGroupPanel = New System.Windows.Forms.FlowLayoutPanel()
        Me.DateRange_StartLabel = New System.Windows.Forms.Label()
        Me.dpEndTime = New System.Windows.Forms.DateTimePicker()
        Me.DateRange_MiddleLabel = New System.Windows.Forms.Label()
        Me.dpStartTime = New System.Windows.Forms.DateTimePicker()
        Me.DateRange_EndLabel = New System.Windows.Forms.Label()
        Me.txtStartTime = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtEndTime = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.updnMinutes = New AutomateControls.StyledNumericUpDown()
        Me.radioEveryNMinutes = New AutomateControls.StyledRadioButton()
        Me.radioEveryNHours = New AutomateControls.StyledRadioButton()
        Me.cbOnWorkingDaysOnly = New System.Windows.Forms.CheckBox()
        Me.comboCalendar = New System.Windows.Forms.ComboBox()
        Me.txtCalendar = New AutomateControls.Textboxes.StyledTextBox()
        Me.updnHours = New AutomateControls.StyledNumericUpDown()
        Me.mYearLabel = New System.Windows.Forms.Label()
        mRunMonthlyGroupPanel = New System.Windows.Forms.GroupBox()
        mRunMonthlyGroupPanel.SuspendLayout()
        Me.HourlyGroupPanel.SuspendLayout()
        CType(Me.updnMinutes, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.updnHours, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'mRunMonthlyGroupPanel
        '
        mRunMonthlyGroupPanel.Controls.Add(Me.HourlyGroupPanel)
        mRunMonthlyGroupPanel.Controls.Add(Me.Label3)
        mRunMonthlyGroupPanel.Controls.Add(Me.updnMinutes)
        mRunMonthlyGroupPanel.Controls.Add(Me.radioEveryNMinutes)
        mRunMonthlyGroupPanel.Controls.Add(Me.radioEveryNHours)
        mRunMonthlyGroupPanel.Controls.Add(Me.cbOnWorkingDaysOnly)
        mRunMonthlyGroupPanel.Controls.Add(Me.comboCalendar)
        mRunMonthlyGroupPanel.Controls.Add(Me.txtCalendar)
        mRunMonthlyGroupPanel.Controls.Add(Me.updnHours)
        mRunMonthlyGroupPanel.Controls.Add(Me.mYearLabel)
        resources.ApplyResources(mRunMonthlyGroupPanel, "mRunMonthlyGroupPanel")
        mRunMonthlyGroupPanel.Name = "mRunMonthlyGroupPanel"
        mRunMonthlyGroupPanel.TabStop = False
        '
        'HourlyGroupPanel
        '
        Me.HourlyGroupPanel.Controls.Add(Me.DateRange_StartLabel)
        Me.HourlyGroupPanel.Controls.Add(Me.dpStartTime)
        Me.HourlyGroupPanel.Controls.Add(Me.DateRange_MiddleLabel)
        Me.HourlyGroupPanel.Controls.Add(Me.dpEndTime)
        Me.HourlyGroupPanel.Controls.Add(Me.DateRange_EndLabel)
        Me.HourlyGroupPanel.Controls.Add(Me.txtStartTime)
        Me.HourlyGroupPanel.Controls.Add(Me.txtEndTime)
        resources.ApplyResources(Me.HourlyGroupPanel, "HourlyGroupPanel")
        Me.HourlyGroupPanel.Name = "HourlyGroupPanel"
        '
        'DateRange_StartLabel
        '
        resources.ApplyResources(Me.DateRange_StartLabel, "DateRange_StartLabel")
        Me.DateRange_StartLabel.Name = "DateRange_StartLabel"
        '
        'dpEndTime
        '
        resources.ApplyResources(Me.dpEndTime, "dpEndTime")
        Me.dpEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dpEndTime.Name = "dpEndTime"
        Me.dpEndTime.ShowUpDown = True
        '
        'DateRange_MiddleLabel
        '
        resources.ApplyResources(Me.DateRange_MiddleLabel, "DateRange_MiddleLabel")
        Me.DateRange_MiddleLabel.Name = "DateRange_MiddleLabel"
        '
        'dpStartTime
        '
        resources.ApplyResources(Me.dpStartTime, "dpStartTime")
        Me.dpStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dpStartTime.Name = "dpStartTime"
        Me.dpStartTime.ShowUpDown = True
        '
        'DateRange_EndLabel
        '
        resources.ApplyResources(Me.DateRange_EndLabel, "DateRange_EndLabel")
        Me.DateRange_EndLabel.Name = "DateRange_EndLabel"
        '
        'txtStartTime
        '
        resources.ApplyResources(Me.txtStartTime, "txtStartTime")
        Me.txtStartTime.Name = "txtStartTime"
        Me.txtStartTime.ReadOnly = True
        '
        'txtEndTime
        '
        resources.ApplyResources(Me.txtEndTime, "txtEndTime")
        Me.txtEndTime.Name = "txtEndTime"
        Me.txtEndTime.ReadOnly = True
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'updnMinutes
        '
        resources.ApplyResources(Me.updnMinutes, "updnMinutes")
        Me.updnMinutes.Maximum = New Decimal(New Integer() {999, 0, 0, 0})
        Me.updnMinutes.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.updnMinutes.Name = "updnMinutes"
        Me.updnMinutes.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'radioEveryNMinutes
        '
        resources.ApplyResources(Me.radioEveryNMinutes, "radioEveryNMinutes")
        Me.radioEveryNMinutes.Name = "radioEveryNMinutes"
        Me.radioEveryNMinutes.TabStop = True
        Me.radioEveryNMinutes.UseVisualStyleBackColor = True
        '
        'radioEveryNHours
        '
        resources.ApplyResources(Me.radioEveryNHours, "radioEveryNHours")
        Me.radioEveryNHours.Name = "radioEveryNHours"
        Me.radioEveryNHours.TabStop = True
        Me.radioEveryNHours.UseVisualStyleBackColor = True
        '
        'cbOnWorkingDaysOnly
        '
        resources.ApplyResources(Me.cbOnWorkingDaysOnly, "cbOnWorkingDaysOnly")
        Me.cbOnWorkingDaysOnly.Name = "cbOnWorkingDaysOnly"
        Me.cbOnWorkingDaysOnly.UseVisualStyleBackColor = True
        '
        'comboCalendar
        '
        resources.ApplyResources(Me.comboCalendar, "comboCalendar")
        Me.comboCalendar.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboCalendar.FormattingEnabled = True
        Me.comboCalendar.Items.AddRange(New Object() {resources.GetString("comboCalendar.Items"), resources.GetString("comboCalendar.Items1"), resources.GetString("comboCalendar.Items2")})
        Me.comboCalendar.Name = "comboCalendar"
        '
        'txtCalendar
        '
        resources.ApplyResources(Me.txtCalendar, "txtCalendar")
        Me.txtCalendar.Name = "txtCalendar"
        Me.txtCalendar.ReadOnly = True
        '
        'updnHours
        '
        resources.ApplyResources(Me.updnHours, "updnHours")
        Me.updnHours.Maximum = New Decimal(New Integer() {999, 0, 0, 0})
        Me.updnHours.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.updnHours.Name = "updnHours"
        Me.updnHours.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'mYearLabel
        '
        resources.ApplyResources(Me.mYearLabel, "mYearLabel")
        Me.mYearLabel.Name = "mYearLabel"
        '
        'ctlRunHourly
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(mRunMonthlyGroupPanel)
        Me.DoubleBuffered = True
        resources.ApplyResources(Me, "$this")
        Me.Name = "ctlRunHourly"
        mRunMonthlyGroupPanel.ResumeLayout(False)
        mRunMonthlyGroupPanel.PerformLayout()
        Me.HourlyGroupPanel.ResumeLayout(False)
        Me.HourlyGroupPanel.PerformLayout()
        CType(Me.updnMinutes, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.updnHours, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents updnHours As AutomateControls.StyledNumericUpDown
    Private WithEvents mYearLabel As System.Windows.Forms.Label
    Friend WithEvents cbOnWorkingDaysOnly As System.Windows.Forms.CheckBox
    Private WithEvents comboCalendar As System.Windows.Forms.ComboBox
    Friend WithEvents txtCalendar As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents radioEveryNHours As AutomateControls.StyledRadioButton
    Private WithEvents Label3 As System.Windows.Forms.Label
    Private WithEvents updnMinutes As AutomateControls.StyledNumericUpDown
    Private WithEvents radioEveryNMinutes As AutomateControls.StyledRadioButton
    Friend WithEvents HourlyGroupPanel As FlowLayoutPanel
    Friend WithEvents DateRange_StartLabel As Label
    Friend WithEvents dpEndTime As DateTimePicker
    Friend WithEvents DateRange_MiddleLabel As Label
    Friend WithEvents dpStartTime As DateTimePicker
    Friend WithEvents txtStartTime As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents txtEndTime As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents DateRange_EndLabel As Label
End Class
