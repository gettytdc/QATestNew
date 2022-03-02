<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlRunWeekly
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
        Dim mRunWeeklyGroupPanel As System.Windows.Forms.GroupBox
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlRunWeekly))
        Dim Label24 As System.Windows.Forms.Label
        Dim Label27 As System.Windows.Forms.Label
        Me.comboNthWorkingDay = New System.Windows.Forms.ComboBox()
        Me.comboCalendar = New System.Windows.Forms.ComboBox()
        Me.radioOnWorkingDay = New AutomateControls.StyledRadioButton()
        Me.radioOnWeekday = New AutomateControls.StyledRadioButton()
        Me.updnPeriod = New AutomateControls.StyledNumericUpDown()
        Me.mWeekLabel = New System.Windows.Forms.Label()
        Me.txtNthWorkingDay = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtCalendar = New AutomateControls.Textboxes.StyledTextBox()
        mRunWeeklyGroupPanel = New System.Windows.Forms.GroupBox()
        Label24 = New System.Windows.Forms.Label()
        Label27 = New System.Windows.Forms.Label()
        mRunWeeklyGroupPanel.SuspendLayout()
        CType(Me.updnPeriod, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'mRunWeeklyGroupPanel
        '
        mRunWeeklyGroupPanel.Controls.Add(Me.comboNthWorkingDay)
        mRunWeeklyGroupPanel.Controls.Add(Label24)
        mRunWeeklyGroupPanel.Controls.Add(Me.comboCalendar)
        mRunWeeklyGroupPanel.Controls.Add(Me.radioOnWorkingDay)
        mRunWeeklyGroupPanel.Controls.Add(Me.radioOnWeekday)
        mRunWeeklyGroupPanel.Controls.Add(Me.updnPeriod)
        mRunWeeklyGroupPanel.Controls.Add(Me.mWeekLabel)
        mRunWeeklyGroupPanel.Controls.Add(Label27)
        mRunWeeklyGroupPanel.Controls.Add(Me.txtNthWorkingDay)
        mRunWeeklyGroupPanel.Controls.Add(Me.txtCalendar)
        resources.ApplyResources(mRunWeeklyGroupPanel, "mRunWeeklyGroupPanel")
        mRunWeeklyGroupPanel.Name = "mRunWeeklyGroupPanel"
        mRunWeeklyGroupPanel.TabStop = False
        '
        'comboNthWorkingDay
        '
        Me.comboNthWorkingDay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboNthWorkingDay.FormattingEnabled = True
        Me.comboNthWorkingDay.Items.AddRange(New Object() {resources.GetString("comboNthWorkingDay.Items"), resources.GetString("comboNthWorkingDay.Items1")})
        resources.ApplyResources(Me.comboNthWorkingDay, "comboNthWorkingDay")
        Me.comboNthWorkingDay.Name = "comboNthWorkingDay"
        '
        'Label24
        '
        resources.ApplyResources(Label24, "Label24")
        Label24.Name = "Label24"
        '
        'comboCalendar
        '
        resources.ApplyResources(Me.comboCalendar, "comboCalendar")
        Me.comboCalendar.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboCalendar.FormattingEnabled = True
        Me.comboCalendar.Items.AddRange(New Object() {resources.GetString("comboCalendar.Items"), resources.GetString("comboCalendar.Items1"), resources.GetString("comboCalendar.Items2")})
        Me.comboCalendar.Name = "comboCalendar"
        '
        'radioOnWorkingDay
        '
        resources.ApplyResources(Me.radioOnWorkingDay, "radioOnWorkingDay")
        Me.radioOnWorkingDay.Name = "radioOnWorkingDay"
        Me.radioOnWorkingDay.TabStop = True
        Me.radioOnWorkingDay.UseVisualStyleBackColor = True
        '
        'radioOnWeekday
        '
        resources.ApplyResources(Me.radioOnWeekday, "radioOnWeekday")
        Me.radioOnWeekday.Name = "radioOnWeekday"
        Me.radioOnWeekday.TabStop = True
        Me.radioOnWeekday.UseVisualStyleBackColor = True
        '
        'updnPeriod
        '
        resources.ApplyResources(Me.updnPeriod, "updnPeriod")
        Me.updnPeriod.Maximum = New Decimal(New Integer() {999, 0, 0, 0})
        Me.updnPeriod.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.updnPeriod.Name = "updnPeriod"
        Me.updnPeriod.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'mWeekLabel
        '
        resources.ApplyResources(Me.mWeekLabel, "mWeekLabel")
        Me.mWeekLabel.Name = "mWeekLabel"
        '
        'Label27
        '
        resources.ApplyResources(Label27, "Label27")
        Label27.Name = "Label27"
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
        'ctlRunWeekly
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(mRunWeeklyGroupPanel)
        Me.DoubleBuffered = True
        resources.ApplyResources(Me, "$this")
        Me.Name = "ctlRunWeekly"
        mRunWeeklyGroupPanel.ResumeLayout(False)
        mRunWeeklyGroupPanel.PerformLayout()
        CType(Me.updnPeriod, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents comboCalendar As System.Windows.Forms.ComboBox
    Private WithEvents radioOnWorkingDay As AutomateControls.StyledRadioButton
    Private WithEvents radioOnWeekday As AutomateControls.StyledRadioButton
    Private WithEvents updnPeriod As AutomateControls.StyledNumericUpDown
    Private WithEvents mWeekLabel As System.Windows.Forms.Label
    Private WithEvents comboNthWorkingDay As System.Windows.Forms.ComboBox
    Friend WithEvents txtNthWorkingDay As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents txtCalendar As AutomateControls.Textboxes.StyledTextBox

End Class
