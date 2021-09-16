<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlSchedule
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
        Dim Label2 As System.Windows.Forms.Label
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSchedule))
        Dim Label3 As System.Windows.Forms.Label
        Dim panScheduleBox As System.Windows.Forms.GroupBox
        Dim lblExpires As System.Windows.Forms.Label
        Dim Label5 As System.Windows.Forms.Label
        Dim Label33 As System.Windows.Forms.Label
        Me.pnlScheduleTiming = New System.Windows.Forms.Panel()
        Me.TimezoneWarningLabel = New System.Windows.Forms.Label()
        Me.chkUseDaylightSavings = New System.Windows.Forms.CheckBox()
        Me.useTimeZoneCheckbox = New System.Windows.Forms.CheckBox()
        Me.cultureDpStartDate = New CustomControls.DatePicker()
        Me.mRunRadioTable = New System.Windows.Forms.TableLayoutPanel()
        Me.dpStartDate = New System.Windows.Forms.DateTimePicker()
        Me.dpStartTime = New System.Windows.Forms.DateTimePicker()
        Me.panEnds = New System.Windows.Forms.Panel()
        Me.cultureDpEndDate = New CustomControls.DatePicker()
        Me.dpEndTime = New System.Windows.Forms.DateTimePicker()
        Me.dpEndDate = New System.Windows.Forms.DateTimePicker()
        Me.lblTimeZone = New System.Windows.Forms.Label()
        Me.cmbTimeZone = New System.Windows.Forms.ComboBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.mRepeatingSchedulePanel = New System.Windows.Forms.Panel()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.mInitialTaskCombo = New System.Windows.Forms.ComboBox()
        Me.radioRunOnce = New AutomateControls.StyledRadioButton()
        Me.radioRunHourly = New AutomateControls.StyledRadioButton()
        Me.radioRunDaily = New AutomateControls.StyledRadioButton()
        Me.radioRunWeekly = New AutomateControls.StyledRadioButton()
        Me.radioRunMonthly = New AutomateControls.StyledRadioButton()
        Me.radioRunYearly = New AutomateControls.StyledRadioButton()
        Me.radioEndsNever = New AutomateControls.StyledRadioButton()
        Me.txtEndTime = New AutomateControls.Textboxes.StyledTextBox()
        Me.radioEndsOn = New AutomateControls.StyledRadioButton()
        Me.txtEndDate = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtStartTime = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtStartDate = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtDescription = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtName = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtReadOnlyInitialTask = New AutomateControls.Textboxes.StyledTextBox()
        Label2 = New System.Windows.Forms.Label()
        Label3 = New System.Windows.Forms.Label()
        panScheduleBox = New System.Windows.Forms.GroupBox()
        lblExpires = New System.Windows.Forms.Label()
        Label5 = New System.Windows.Forms.Label()
        Label33 = New System.Windows.Forms.Label()
        panScheduleBox.SuspendLayout()
        Me.pnlScheduleTiming.SuspendLayout()
        Me.mRunRadioTable.SuspendLayout()
        Me.panEnds.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label2
        '
        resources.ApplyResources(Label2, "Label2")
        Label2.ForeColor = System.Drawing.Color.Black
        Label2.Name = "Label2"
        '
        'Label3
        '
        resources.ApplyResources(Label3, "Label3")
        Label3.ForeColor = System.Drawing.Color.Black
        Label3.Name = "Label3"
        '
        'panScheduleBox
        '
        resources.ApplyResources(panScheduleBox, "panScheduleBox")
        panScheduleBox.Controls.Add(Me.pnlScheduleTiming)
        panScheduleBox.Name = "panScheduleBox"
        panScheduleBox.TabStop = False
        '
        'pnlScheduleTiming
        '
        Me.pnlScheduleTiming.Controls.Add(Me.TimezoneWarningLabel)
        Me.pnlScheduleTiming.Controls.Add(Me.chkUseDaylightSavings)
        Me.pnlScheduleTiming.Controls.Add(Me.useTimeZoneCheckbox)
        Me.pnlScheduleTiming.Controls.Add(Me.cultureDpStartDate)
        Me.pnlScheduleTiming.Controls.Add(Me.mRunRadioTable)
        Me.pnlScheduleTiming.Controls.Add(Me.dpStartDate)
        Me.pnlScheduleTiming.Controls.Add(Label2)
        Me.pnlScheduleTiming.Controls.Add(Me.dpStartTime)
        Me.pnlScheduleTiming.Controls.Add(Me.panEnds)
        Me.pnlScheduleTiming.Controls.Add(Me.txtStartTime)
        Me.pnlScheduleTiming.Controls.Add(Me.txtStartDate)
        Me.pnlScheduleTiming.Controls.Add(Label5)
        Me.pnlScheduleTiming.Controls.Add(Label33)
        Me.pnlScheduleTiming.Controls.Add(Me.lblTimeZone)
        Me.pnlScheduleTiming.Controls.Add(Me.cmbTimeZone)
        resources.ApplyResources(Me.pnlScheduleTiming, "pnlScheduleTiming")
        Me.pnlScheduleTiming.Name = "pnlScheduleTiming"
        '
        'TimezoneWarningLabel
        '
        resources.ApplyResources(Me.TimezoneWarningLabel, "TimezoneWarningLabel")
        Me.TimezoneWarningLabel.ForeColor = System.Drawing.Color.OrangeRed
        Me.TimezoneWarningLabel.Name = "TimezoneWarningLabel"
        '
        'chkUseDaylightSavings
        '
        resources.ApplyResources(Me.chkUseDaylightSavings, "chkUseDaylightSavings")
        Me.chkUseDaylightSavings.Name = "chkUseDaylightSavings"
        Me.chkUseDaylightSavings.UseVisualStyleBackColor = True
        '
        'useTimeZoneCheckbox
        '
        resources.ApplyResources(Me.useTimeZoneCheckbox, "useTimeZoneCheckbox")
        Me.useTimeZoneCheckbox.Checked = True
        Me.useTimeZoneCheckbox.CheckState = System.Windows.Forms.CheckState.Checked
        Me.useTimeZoneCheckbox.Name = "useTimeZoneCheckbox"
        Me.useTimeZoneCheckbox.UseVisualStyleBackColor = True
        '
        'cultureDpStartDate
        '
        resources.ApplyResources(Me.cultureDpStartDate, "cultureDpStartDate")
        Me.cultureDpStartDate.Name = "cultureDpStartDate"
        Me.cultureDpStartDate.PickerDayFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        '
        'mRunRadioTable
        '
        resources.ApplyResources(Me.mRunRadioTable, "mRunRadioTable")
        Me.mRunRadioTable.Controls.Add(Me.radioRunOnce, 0, 0)
        Me.mRunRadioTable.Controls.Add(Me.radioRunHourly, 1, 0)
        Me.mRunRadioTable.Controls.Add(Me.radioRunDaily, 2, 0)
        Me.mRunRadioTable.Controls.Add(Me.radioRunWeekly, 0, 1)
        Me.mRunRadioTable.Controls.Add(Me.radioRunMonthly, 1, 1)
        Me.mRunRadioTable.Controls.Add(Me.radioRunYearly, 2, 1)
        Me.mRunRadioTable.Name = "mRunRadioTable"
        '
        'dpStartDate
        '
        resources.ApplyResources(Me.dpStartDate, "dpStartDate")
        Me.dpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dpStartDate.Name = "dpStartDate"
        '
        'dpStartTime
        '
        resources.ApplyResources(Me.dpStartTime, "dpStartTime")
        Me.dpStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dpStartTime.Name = "dpStartTime"
        Me.dpStartTime.ShowUpDown = True
        Me.dpStartTime.Value = New Date(2009, 12, 1, 0, 0, 0, 0)
        '
        'panEnds
        '
        Me.panEnds.Controls.Add(Me.cultureDpEndDate)
        Me.panEnds.Controls.Add(lblExpires)
        Me.panEnds.Controls.Add(Me.dpEndTime)
        Me.panEnds.Controls.Add(Me.radioEndsNever)
        Me.panEnds.Controls.Add(Me.txtEndTime)
        Me.panEnds.Controls.Add(Me.radioEndsOn)
        Me.panEnds.Controls.Add(Me.dpEndDate)
        Me.panEnds.Controls.Add(Me.txtEndDate)
        resources.ApplyResources(Me.panEnds, "panEnds")
        Me.panEnds.Name = "panEnds"
        '
        'cultureDpEndDate
        '
        resources.ApplyResources(Me.cultureDpEndDate, "cultureDpEndDate")
        Me.cultureDpEndDate.Name = "cultureDpEndDate"
        Me.cultureDpEndDate.PickerDayFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        '
        'lblExpires
        '
        resources.ApplyResources(lblExpires, "lblExpires")
        lblExpires.ForeColor = System.Drawing.Color.Black
        lblExpires.Name = "lblExpires"
        '
        'dpEndTime
        '
        resources.ApplyResources(Me.dpEndTime, "dpEndTime")
        Me.dpEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dpEndTime.Name = "dpEndTime"
        Me.dpEndTime.ShowUpDown = True
        Me.dpEndTime.Value = New Date(2009, 12, 1, 0, 0, 0, 0)
        '
        'dpEndDate
        '
        Me.dpEndDate.Checked = False
        resources.ApplyResources(Me.dpEndDate, "dpEndDate")
        Me.dpEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dpEndDate.Name = "dpEndDate"
        '
        'Label5
        '
        resources.ApplyResources(Label5, "Label5")
        Label5.ForeColor = System.Drawing.Color.Black
        Label5.Name = "Label5"
        '
        'Label33
        '
        resources.ApplyResources(Label33, "Label33")
        Label33.ForeColor = System.Drawing.Color.Black
        Label33.Name = "Label33"
        '
        'lblTimeZone
        '
        resources.ApplyResources(Me.lblTimeZone, "lblTimeZone")
        Me.lblTimeZone.Name = "lblTimeZone"
        '
        'cmbTimeZone
        '
        Me.cmbTimeZone.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        resources.ApplyResources(Me.cmbTimeZone, "cmbTimeZone")
        Me.cmbTimeZone.Name = "cmbTimeZone"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Name = "Label1"
        '
        'mRepeatingSchedulePanel
        '
        resources.ApplyResources(Me.mRepeatingSchedulePanel, "mRepeatingSchedulePanel")
        Me.mRepeatingSchedulePanel.Name = "mRepeatingSchedulePanel"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.ForeColor = System.Drawing.Color.Black
        Me.Label4.Name = "Label4"
        '
        'mInitialTaskCombo
        '
        resources.ApplyResources(Me.mInitialTaskCombo, "mInitialTaskCombo")
        Me.mInitialTaskCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.mInitialTaskCombo.FormattingEnabled = True
        Me.mInitialTaskCombo.Name = "mInitialTaskCombo"
        '
        'radioRunOnce
        '
        resources.ApplyResources(Me.radioRunOnce, "radioRunOnce")
        Me.radioRunOnce.ButtonHeight = 21
        Me.radioRunOnce.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.radioRunOnce.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.radioRunOnce.FocusDiameter = 16
        Me.radioRunOnce.FocusThickness = 3
        Me.radioRunOnce.FocusYLocation = 9
        Me.radioRunOnce.ForceFocus = True
        Me.radioRunOnce.ForeColor = System.Drawing.Color.Black
        Me.radioRunOnce.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.radioRunOnce.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.radioRunOnce.MouseLeaveColor = System.Drawing.Color.White
        Me.radioRunOnce.Name = "radioRunOnce"
        Me.radioRunOnce.RadioButtonDiameter = 12
        Me.radioRunOnce.RadioButtonThickness = 2
        Me.radioRunOnce.RadioYLocation = 7
        Me.radioRunOnce.StringYLocation = 1
        Me.radioRunOnce.TextColor = System.Drawing.Color.Black
        Me.radioRunOnce.UseVisualStyleBackColor = True
        '
        'radioRunHourly
        '
        resources.ApplyResources(Me.radioRunHourly, "radioRunHourly")
        Me.radioRunHourly.ButtonHeight = 21
        Me.radioRunHourly.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.radioRunHourly.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.radioRunHourly.FocusDiameter = 16
        Me.radioRunHourly.FocusThickness = 3
        Me.radioRunHourly.FocusYLocation = 9
        Me.radioRunHourly.ForceFocus = True
        Me.radioRunHourly.ForeColor = System.Drawing.Color.Black
        Me.radioRunHourly.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.radioRunHourly.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.radioRunHourly.MouseLeaveColor = System.Drawing.Color.White
        Me.radioRunHourly.Name = "radioRunHourly"
        Me.radioRunHourly.RadioButtonDiameter = 12
        Me.radioRunHourly.RadioButtonThickness = 2
        Me.radioRunHourly.RadioYLocation = 7
        Me.radioRunHourly.StringYLocation = 1
        Me.radioRunHourly.TextColor = System.Drawing.Color.Black
        Me.radioRunHourly.UseVisualStyleBackColor = True
        '
        'radioRunDaily
        '
        resources.ApplyResources(Me.radioRunDaily, "radioRunDaily")
        Me.radioRunDaily.ButtonHeight = 21
        Me.radioRunDaily.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.radioRunDaily.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.radioRunDaily.FocusDiameter = 16
        Me.radioRunDaily.FocusThickness = 3
        Me.radioRunDaily.FocusYLocation = 9
        Me.radioRunDaily.ForceFocus = True
        Me.radioRunDaily.ForeColor = System.Drawing.Color.Black
        Me.radioRunDaily.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.radioRunDaily.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.radioRunDaily.MouseLeaveColor = System.Drawing.Color.White
        Me.radioRunDaily.Name = "radioRunDaily"
        Me.radioRunDaily.RadioButtonDiameter = 12
        Me.radioRunDaily.RadioButtonThickness = 2
        Me.radioRunDaily.RadioYLocation = 7
        Me.radioRunDaily.StringYLocation = 1
        Me.radioRunDaily.TextColor = System.Drawing.Color.Black
        Me.radioRunDaily.UseVisualStyleBackColor = True
        '
        'radioRunWeekly
        '
        resources.ApplyResources(Me.radioRunWeekly, "radioRunWeekly")
        Me.radioRunWeekly.ButtonHeight = 21
        Me.radioRunWeekly.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.radioRunWeekly.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.radioRunWeekly.FocusDiameter = 16
        Me.radioRunWeekly.FocusThickness = 3
        Me.radioRunWeekly.FocusYLocation = 9
        Me.radioRunWeekly.ForceFocus = True
        Me.radioRunWeekly.ForeColor = System.Drawing.Color.Black
        Me.radioRunWeekly.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.radioRunWeekly.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.radioRunWeekly.MouseLeaveColor = System.Drawing.Color.White
        Me.radioRunWeekly.Name = "radioRunWeekly"
        Me.radioRunWeekly.RadioButtonDiameter = 12
        Me.radioRunWeekly.RadioButtonThickness = 2
        Me.radioRunWeekly.RadioYLocation = 7
        Me.radioRunWeekly.StringYLocation = 1
        Me.radioRunWeekly.TextColor = System.Drawing.Color.Black
        Me.radioRunWeekly.UseVisualStyleBackColor = True
        '
        'radioRunMonthly
        '
        resources.ApplyResources(Me.radioRunMonthly, "radioRunMonthly")
        Me.radioRunMonthly.ButtonHeight = 21
        Me.radioRunMonthly.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.radioRunMonthly.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.radioRunMonthly.FocusDiameter = 16
        Me.radioRunMonthly.FocusThickness = 3
        Me.radioRunMonthly.FocusYLocation = 9
        Me.radioRunMonthly.ForceFocus = True
        Me.radioRunMonthly.ForeColor = System.Drawing.Color.Black
        Me.radioRunMonthly.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.radioRunMonthly.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.radioRunMonthly.MouseLeaveColor = System.Drawing.Color.White
        Me.radioRunMonthly.Name = "radioRunMonthly"
        Me.radioRunMonthly.RadioButtonDiameter = 12
        Me.radioRunMonthly.RadioButtonThickness = 2
        Me.radioRunMonthly.RadioYLocation = 7
        Me.radioRunMonthly.StringYLocation = 1
        Me.radioRunMonthly.TextColor = System.Drawing.Color.Black
        Me.radioRunMonthly.UseVisualStyleBackColor = True
        '
        'radioRunYearly
        '
        resources.ApplyResources(Me.radioRunYearly, "radioRunYearly")
        Me.radioRunYearly.ButtonHeight = 21
        Me.radioRunYearly.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.radioRunYearly.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.radioRunYearly.FocusDiameter = 16
        Me.radioRunYearly.FocusThickness = 3
        Me.radioRunYearly.FocusYLocation = 9
        Me.radioRunYearly.ForceFocus = True
        Me.radioRunYearly.ForeColor = System.Drawing.Color.Black
        Me.radioRunYearly.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.radioRunYearly.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.radioRunYearly.MouseLeaveColor = System.Drawing.Color.White
        Me.radioRunYearly.Name = "radioRunYearly"
        Me.radioRunYearly.RadioButtonDiameter = 12
        Me.radioRunYearly.RadioButtonThickness = 2
        Me.radioRunYearly.RadioYLocation = 7
        Me.radioRunYearly.StringYLocation = 1
        Me.radioRunYearly.TextColor = System.Drawing.Color.Black
        Me.radioRunYearly.UseVisualStyleBackColor = True
        '
        'radioEndsNever
        '
        resources.ApplyResources(Me.radioEndsNever, "radioEndsNever")
        Me.radioEndsNever.ButtonHeight = 21
        Me.radioEndsNever.Checked = True
        Me.radioEndsNever.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.radioEndsNever.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.radioEndsNever.FocusDiameter = 16
        Me.radioEndsNever.FocusThickness = 3
        Me.radioEndsNever.FocusYLocation = 9
        Me.radioEndsNever.ForceFocus = True
        Me.radioEndsNever.ForeColor = System.Drawing.Color.Black
        Me.radioEndsNever.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.radioEndsNever.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.radioEndsNever.MouseLeaveColor = System.Drawing.Color.White
        Me.radioEndsNever.Name = "radioEndsNever"
        Me.radioEndsNever.RadioButtonDiameter = 12
        Me.radioEndsNever.RadioButtonThickness = 2
        Me.radioEndsNever.RadioYLocation = 7
        Me.radioEndsNever.StringYLocation = 1
        Me.radioEndsNever.TabStop = True
        Me.radioEndsNever.TextColor = System.Drawing.Color.Black
        Me.radioEndsNever.UseVisualStyleBackColor = True
        '
        'txtEndTime
        '
        Me.txtEndTime.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.txtEndTime, "txtEndTime")
        Me.txtEndTime.Name = "txtEndTime"
        Me.txtEndTime.ReadOnly = True
        '
        'radioEndsOn
        '
        resources.ApplyResources(Me.radioEndsOn, "radioEndsOn")
        Me.radioEndsOn.ButtonHeight = 21
        Me.radioEndsOn.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.radioEndsOn.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.radioEndsOn.FocusDiameter = 16
        Me.radioEndsOn.FocusThickness = 3
        Me.radioEndsOn.FocusYLocation = 9
        Me.radioEndsOn.ForceFocus = True
        Me.radioEndsOn.ForeColor = System.Drawing.Color.Black
        Me.radioEndsOn.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.radioEndsOn.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.radioEndsOn.MouseLeaveColor = System.Drawing.Color.White
        Me.radioEndsOn.Name = "radioEndsOn"
        Me.radioEndsOn.RadioButtonDiameter = 12
        Me.radioEndsOn.RadioButtonThickness = 2
        Me.radioEndsOn.RadioYLocation = 7
        Me.radioEndsOn.StringYLocation = 1
        Me.radioEndsOn.TabStop = True
        Me.radioEndsOn.TextColor = System.Drawing.Color.Black
        Me.radioEndsOn.UseVisualStyleBackColor = True
        '
        'txtEndDate
        '
        Me.txtEndDate.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.txtEndDate, "txtEndDate")
        Me.txtEndDate.Name = "txtEndDate"
        Me.txtEndDate.ReadOnly = True
        '
        'txtStartTime
        '
        Me.txtStartTime.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.txtStartTime, "txtStartTime")
        Me.txtStartTime.Name = "txtStartTime"
        Me.txtStartTime.ReadOnly = True
        '
        'txtStartDate
        '
        Me.txtStartDate.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.txtStartDate, "txtStartDate")
        Me.txtStartDate.Name = "txtStartDate"
        Me.txtStartDate.ReadOnly = True
        '
        'txtDescription
        '
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        Me.txtDescription.BorderColor = System.Drawing.Color.Empty
        Me.txtDescription.Name = "txtDescription"
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        Me.txtName.BorderColor = System.Drawing.Color.Empty
        Me.txtName.Name = "txtName"
        '
        'txtReadOnlyInitialTask
        '
        resources.ApplyResources(Me.txtReadOnlyInitialTask, "txtReadOnlyInitialTask")
        Me.txtReadOnlyInitialTask.BorderColor = System.Drawing.Color.Empty
        Me.txtReadOnlyInitialTask.Name = "txtReadOnlyInitialTask"
        Me.txtReadOnlyInitialTask.ReadOnly = True
        '
        'ctlSchedule
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.mInitialTaskCombo)
        Me.Controls.Add(Me.mRepeatingSchedulePanel)
        Me.Controls.Add(panScheduleBox)
        Me.Controls.Add(Me.txtDescription)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtName)
        Me.Controls.Add(Label3)
        Me.Controls.Add(Me.txtReadOnlyInitialTask)
        Me.DoubleBuffered = True
        Me.Name = "ctlSchedule"
        resources.ApplyResources(Me, "$this")
        panScheduleBox.ResumeLayout(False)
        Me.pnlScheduleTiming.ResumeLayout(False)
        Me.pnlScheduleTiming.PerformLayout()
        Me.mRunRadioTable.ResumeLayout(False)
        Me.mRunRadioTable.PerformLayout()
        Me.panEnds.ResumeLayout(False)
        Me.panEnds.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents pnlScheduleTiming As System.Windows.Forms.Panel
    Private WithEvents mRunRadioTable As System.Windows.Forms.TableLayoutPanel
    Private WithEvents radioRunOnce As AutomateControls.StyledRadioButton
    Private WithEvents radioRunMonthly As AutomateControls.StyledRadioButton
    Private WithEvents radioRunDaily As AutomateControls.StyledRadioButton
    Private WithEvents radioRunYearly As AutomateControls.StyledRadioButton
    Private WithEvents radioRunWeekly As AutomateControls.StyledRadioButton
    Private WithEvents radioRunHourly As AutomateControls.StyledRadioButton
    Private WithEvents dpEndDate As System.Windows.Forms.DateTimePicker
    Private WithEvents radioEndsOn As AutomateControls.StyledRadioButton
    Private WithEvents radioEndsNever As AutomateControls.StyledRadioButton
    Private WithEvents txtName As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents Label1 As System.Windows.Forms.Label
    Private WithEvents txtDescription As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents dpStartDate As System.Windows.Forms.DateTimePicker
    Private WithEvents dpStartTime As System.Windows.Forms.DateTimePicker
    Private WithEvents mRepeatingSchedulePanel As System.Windows.Forms.Panel
    Private WithEvents Label4 As System.Windows.Forms.Label
    Private WithEvents mInitialTaskCombo As System.Windows.Forms.ComboBox
    Private WithEvents dpEndTime As System.Windows.Forms.DateTimePicker
    Private WithEvents txtReadOnlyInitialTask As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents txtEndTime As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents txtEndDate As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents txtStartTime As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents txtStartDate As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents panEnds As System.Windows.Forms.Panel
    Friend WithEvents cultureDpStartDate As CustomControls.DatePicker
    Friend WithEvents cultureDpEndDate As CustomControls.DatePicker
    Friend WithEvents cmbTimeZone As ComboBox
    Friend lblTimeZone As Label
    Friend WithEvents useTimeZoneCheckbox As CheckBox
    Friend WithEvents chkUseDaylightSavings As CheckBox
    Friend WithEvents TimezoneWarningLabel As Label
End Class
