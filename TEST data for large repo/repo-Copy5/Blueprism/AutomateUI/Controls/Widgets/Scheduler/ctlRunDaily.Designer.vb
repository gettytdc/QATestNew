<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlRunDaily
    Inherits System.Windows.Forms.UserControl

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
    End Sub

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
        Dim mRunDailyGroupPanel As System.Windows.Forms.GroupBox
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlRunDaily))
        Dim Label27 As System.Windows.Forms.Label
        Me.cbOnWorkingDaysOnly = New System.Windows.Forms.CheckBox()
        Me.comboCalendar = New System.Windows.Forms.ComboBox()
        Me.updnPeriod = New AutomateControls.StyledNumericUpDown()
        Me.mDayLabel = New System.Windows.Forms.Label()
        Me.txtCalendar = New AutomateControls.Textboxes.StyledTextBox()
        mRunDailyGroupPanel = New System.Windows.Forms.GroupBox()
        Label27 = New System.Windows.Forms.Label()
        mRunDailyGroupPanel.SuspendLayout()
        CType(Me.updnPeriod, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'mRunDailyGroupPanel
        '
        mRunDailyGroupPanel.Controls.Add(Me.cbOnWorkingDaysOnly)
        mRunDailyGroupPanel.Controls.Add(Me.comboCalendar)
        mRunDailyGroupPanel.Controls.Add(Me.updnPeriod)
        mRunDailyGroupPanel.Controls.Add(Me.mDayLabel)
        mRunDailyGroupPanel.Controls.Add(Label27)
        mRunDailyGroupPanel.Controls.Add(Me.txtCalendar)
        resources.ApplyResources(mRunDailyGroupPanel, "mRunDailyGroupPanel")
        mRunDailyGroupPanel.Name = "mRunDailyGroupPanel"
        mRunDailyGroupPanel.TabStop = False
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
        'updnPeriod
        '
        resources.ApplyResources(Me.updnPeriod, "updnPeriod")
        Me.updnPeriod.Maximum = New Decimal(New Integer() {999, 0, 0, 0})
        Me.updnPeriod.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.updnPeriod.Name = "updnPeriod"
        Me.updnPeriod.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'mDayLabel
        '
        resources.ApplyResources(Me.mDayLabel, "mDayLabel")
        Me.mDayLabel.Name = "mDayLabel"
        '
        'Label27
        '
        resources.ApplyResources(Label27, "Label27")
        Label27.Name = "Label27"
        '
        'txtCalendar
        '
        resources.ApplyResources(Me.txtCalendar, "txtCalendar")
        Me.txtCalendar.Name = "txtCalendar"
        Me.txtCalendar.ReadOnly = True
        '
        'ctlRunDaily
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(mRunDailyGroupPanel)
        Me.DoubleBuffered = True
        resources.ApplyResources(Me, "$this")
        Me.Name = "ctlRunDaily"
        mRunDailyGroupPanel.ResumeLayout(False)
        mRunDailyGroupPanel.PerformLayout()
        CType(Me.updnPeriod, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents updnPeriod As AutomateControls.StyledNumericUpDown
    Private WithEvents mDayLabel As System.Windows.Forms.Label
    Private WithEvents comboCalendar As System.Windows.Forms.ComboBox
    Friend WithEvents cbOnWorkingDaysOnly As System.Windows.Forms.CheckBox
    Friend WithEvents txtCalendar As AutomateControls.Textboxes.StyledTextBox

End Class
