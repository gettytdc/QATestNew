<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlRunMonthly
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
        Dim mRunMonthlyGroupPanel As System.Windows.Forms.GroupBox
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlRunMonthly))
        Dim Label24 As System.Windows.Forms.Label
        Dim Label27 As System.Windows.Forms.Label
        Me.panMissingAction = New System.Windows.Forms.Panel()
        Me.lblMissingDateActionPrefix = New System.Windows.Forms.Label()
        Me.comboMissingDateAction = New System.Windows.Forms.ComboBox()
        Me.txtMissingDateAction = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblNthOfMonth = New System.Windows.Forms.Label()
        Me.comboDayOfWeek = New System.Windows.Forms.ComboBox()
        Me.comboCalendar = New System.Windows.Forms.ComboBox()
        Me.comboNthWorkingDay = New System.Windows.Forms.ComboBox()
        Me.comboNthDayOfWeek = New System.Windows.Forms.ComboBox()
        Me.radioNthWorkingDay = New AutomateControls.StyledRadioButton()
        Me.radioNthWeekdayOfMonth = New AutomateControls.StyledRadioButton()
        Me.radioNthOfMonth = New AutomateControls.StyledRadioButton()
        Me.updnPeriod = New AutomateControls.StyledNumericUpDown()
        Me.mMonthLabel = New System.Windows.Forms.Label()
        Me.txtDayOfWeek = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtNthDayOfWeek = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtNthWorkingDay = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtCalendar = New AutomateControls.Textboxes.StyledTextBox()
        mRunMonthlyGroupPanel = New System.Windows.Forms.GroupBox()
        Label24 = New System.Windows.Forms.Label()
        Label27 = New System.Windows.Forms.Label()
        mRunMonthlyGroupPanel.SuspendLayout()
        Me.panMissingAction.SuspendLayout()
        CType(Me.updnPeriod, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'mRunMonthlyGroupPanel
        '
        mRunMonthlyGroupPanel.Controls.Add(Me.panMissingAction)
        mRunMonthlyGroupPanel.Controls.Add(Me.lblNthOfMonth)
        mRunMonthlyGroupPanel.Controls.Add(Me.comboDayOfWeek)
        mRunMonthlyGroupPanel.Controls.Add(Me.comboCalendar)
        mRunMonthlyGroupPanel.Controls.Add(Me.comboNthWorkingDay)
        mRunMonthlyGroupPanel.Controls.Add(Me.comboNthDayOfWeek)
        mRunMonthlyGroupPanel.Controls.Add(Me.radioNthWorkingDay)
        mRunMonthlyGroupPanel.Controls.Add(Me.radioNthWeekdayOfMonth)
        mRunMonthlyGroupPanel.Controls.Add(Me.radioNthOfMonth)
        mRunMonthlyGroupPanel.Controls.Add(Me.updnPeriod)
        mRunMonthlyGroupPanel.Controls.Add(Label24)
        mRunMonthlyGroupPanel.Controls.Add(Me.mMonthLabel)
        mRunMonthlyGroupPanel.Controls.Add(Label27)
        mRunMonthlyGroupPanel.Controls.Add(Me.txtDayOfWeek)
        mRunMonthlyGroupPanel.Controls.Add(Me.txtNthDayOfWeek)
        mRunMonthlyGroupPanel.Controls.Add(Me.txtNthWorkingDay)
        mRunMonthlyGroupPanel.Controls.Add(Me.txtCalendar)
        resources.ApplyResources(mRunMonthlyGroupPanel, "mRunMonthlyGroupPanel")
        mRunMonthlyGroupPanel.Name = "mRunMonthlyGroupPanel"
        mRunMonthlyGroupPanel.TabStop = False
        '
        'panMissingAction
        '
        Me.panMissingAction.Controls.Add(Me.lblMissingDateActionPrefix)
        Me.panMissingAction.Controls.Add(Me.comboMissingDateAction)
        Me.panMissingAction.Controls.Add(Me.txtMissingDateAction)
        resources.ApplyResources(Me.panMissingAction, "panMissingAction")
        Me.panMissingAction.Name = "panMissingAction"
        '
        'lblMissingDateActionPrefix
        '
        resources.ApplyResources(Me.lblMissingDateActionPrefix, "lblMissingDateActionPrefix")
        Me.lblMissingDateActionPrefix.Name = "lblMissingDateActionPrefix"
        '
        'comboMissingDateAction
        '
        resources.ApplyResources(Me.comboMissingDateAction, "comboMissingDateAction")
        Me.comboMissingDateAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboMissingDateAction.FormattingEnabled = True
        Me.comboMissingDateAction.Items.AddRange(New Object() {resources.GetString("comboMissingDateAction.Items"), resources.GetString("comboMissingDateAction.Items1")})
        Me.comboMissingDateAction.Name = "comboMissingDateAction"
        '
        'txtMissingDateAction
        '
        resources.ApplyResources(Me.txtMissingDateAction, "txtMissingDateAction")
        Me.txtMissingDateAction.Name = "txtMissingDateAction"
        Me.txtMissingDateAction.ReadOnly = True
        '
        'lblNthOfMonth
        '
        resources.ApplyResources(Me.lblNthOfMonth, "lblNthOfMonth")
        Me.lblNthOfMonth.Name = "lblNthOfMonth"
        '
        'comboDayOfWeek
        '
        resources.ApplyResources(Me.comboDayOfWeek, "comboDayOfWeek")
        Me.comboDayOfWeek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboDayOfWeek.FormattingEnabled = True
        Me.comboDayOfWeek.Items.AddRange(New Object() {resources.GetString("comboDayOfWeek.Items"), resources.GetString("comboDayOfWeek.Items1"), resources.GetString("comboDayOfWeek.Items2"), resources.GetString("comboDayOfWeek.Items3"), resources.GetString("comboDayOfWeek.Items4"), resources.GetString("comboDayOfWeek.Items5"), resources.GetString("comboDayOfWeek.Items6")})
        Me.comboDayOfWeek.Name = "comboDayOfWeek"
        '
        'comboCalendar
        '
        resources.ApplyResources(Me.comboCalendar, "comboCalendar")
        Me.comboCalendar.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboCalendar.FormattingEnabled = True
        Me.comboCalendar.Name = "comboCalendar"
        '
        'comboNthWorkingDay
        '
        Me.comboNthWorkingDay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboNthWorkingDay.FormattingEnabled = True
        Me.comboNthWorkingDay.Items.AddRange(New Object() {resources.GetString("comboNthWorkingDay.Items"), resources.GetString("comboNthWorkingDay.Items1")})
        resources.ApplyResources(Me.comboNthWorkingDay, "comboNthWorkingDay")
        Me.comboNthWorkingDay.Name = "comboNthWorkingDay"
        '
        'comboNthDayOfWeek
        '
        Me.comboNthDayOfWeek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboNthDayOfWeek.FormattingEnabled = True
        Me.comboNthDayOfWeek.Items.AddRange(New Object() {resources.GetString("comboNthDayOfWeek.Items"), resources.GetString("comboNthDayOfWeek.Items1"), resources.GetString("comboNthDayOfWeek.Items2"), resources.GetString("comboNthDayOfWeek.Items3"), resources.GetString("comboNthDayOfWeek.Items4")})
        resources.ApplyResources(Me.comboNthDayOfWeek, "comboNthDayOfWeek")
        Me.comboNthDayOfWeek.Name = "comboNthDayOfWeek"
        '
        'radioNthWorkingDay
        '
        resources.ApplyResources(Me.radioNthWorkingDay, "radioNthWorkingDay")
        Me.radioNthWorkingDay.Name = "radioNthWorkingDay"
        Me.radioNthWorkingDay.TabStop = True
        Me.radioNthWorkingDay.UseVisualStyleBackColor = True
        '
        'radioNthWeekdayOfMonth
        '
        resources.ApplyResources(Me.radioNthWeekdayOfMonth, "radioNthWeekdayOfMonth")
        Me.radioNthWeekdayOfMonth.Name = "radioNthWeekdayOfMonth"
        Me.radioNthWeekdayOfMonth.TabStop = True
        Me.radioNthWeekdayOfMonth.UseVisualStyleBackColor = True
        '
        'radioNthOfMonth
        '
        resources.ApplyResources(Me.radioNthOfMonth, "radioNthOfMonth")
        Me.radioNthOfMonth.Name = "radioNthOfMonth"
        Me.radioNthOfMonth.TabStop = True
        Me.radioNthOfMonth.UseVisualStyleBackColor = True
        '
        'updnPeriod
        '
        resources.ApplyResources(Me.updnPeriod, "updnPeriod")
        Me.updnPeriod.Maximum = New Decimal(New Integer() {999, 0, 0, 0})
        Me.updnPeriod.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.updnPeriod.Name = "updnPeriod"
        Me.updnPeriod.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'Label24
        '
        resources.ApplyResources(Label24, "Label24")
        Label24.Name = "Label24"
        '
        'mMonthLabel
        '
        resources.ApplyResources(Me.mMonthLabel, "mMonthLabel")
        Me.mMonthLabel.Name = "mMonthLabel"
        '
        'Label27
        '
        resources.ApplyResources(Label27, "Label27")
        Label27.Name = "Label27"
        '
        'txtDayOfWeek
        '
        resources.ApplyResources(Me.txtDayOfWeek, "txtDayOfWeek")
        Me.txtDayOfWeek.Name = "txtDayOfWeek"
        Me.txtDayOfWeek.ReadOnly = True
        '
        'txtNthDayOfWeek
        '
        resources.ApplyResources(Me.txtNthDayOfWeek, "txtNthDayOfWeek")
        Me.txtNthDayOfWeek.Name = "txtNthDayOfWeek"
        Me.txtNthDayOfWeek.ReadOnly = True
        '
        'txtNthWorkingDay
        '
        resources.ApplyResources(Me.txtNthWorkingDay, "txtNthWorkingDay")
        Me.txtNthWorkingDay.Name = "txtNthWorkingDay"
        Me.txtNthWorkingDay.ReadOnly = True
        '
        'txtCalendar
        '
        resources.ApplyResources(Me.txtCalendar, "txtCalendar")
        Me.txtCalendar.Name = "txtCalendar"
        Me.txtCalendar.ReadOnly = True
        '
        'ctlRunMonthly
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(mRunMonthlyGroupPanel)
        Me.DoubleBuffered = True
        resources.ApplyResources(Me, "$this")
        Me.Name = "ctlRunMonthly"
        mRunMonthlyGroupPanel.ResumeLayout(False)
        mRunMonthlyGroupPanel.PerformLayout()
        Me.panMissingAction.ResumeLayout(False)
        Me.panMissingAction.PerformLayout()
        CType(Me.updnPeriod, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents comboCalendar As System.Windows.Forms.ComboBox
    Private WithEvents radioNthOfMonth As AutomateControls.StyledRadioButton
    Private WithEvents updnPeriod As AutomateControls.StyledNumericUpDown
    Private WithEvents radioNthWeekdayOfMonth As AutomateControls.StyledRadioButton
    Private WithEvents comboNthDayOfWeek As System.Windows.Forms.ComboBox
    Private WithEvents mMonthLabel As System.Windows.Forms.Label
    Private WithEvents comboDayOfWeek As System.Windows.Forms.ComboBox
    Private WithEvents comboNthWorkingDay As System.Windows.Forms.ComboBox
    Private WithEvents radioNthWorkingDay As AutomateControls.StyledRadioButton
    Friend WithEvents lblNthOfMonth As System.Windows.Forms.Label
    Friend WithEvents comboMissingDateAction As System.Windows.Forms.ComboBox
    Friend WithEvents lblMissingDateActionPrefix As System.Windows.Forms.Label
    Friend WithEvents txtMissingDateAction As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents txtDayOfWeek As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents txtNthDayOfWeek As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents txtNthWorkingDay As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents txtCalendar As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents panMissingAction As System.Windows.Forms.Panel

End Class
