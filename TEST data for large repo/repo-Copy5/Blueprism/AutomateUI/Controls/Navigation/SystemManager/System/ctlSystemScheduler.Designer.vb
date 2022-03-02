<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlSystemScheduler
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSystemScheduler))
        Me.gpSchedulerConfig = New System.Windows.Forms.GroupBox()
        Me.FullWidthFlowLayoutPanel2 = New AutomateControls.FullWidthFlowLayoutPanel()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.cbActivateScheduler = New System.Windows.Forms.CheckBox()
        Me.flowScheduleMinutes = New System.Windows.Forms.FlowLayoutPanel()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.numSchedulerCheckMinutes = new AutomateControls.StyledNumericUpDown()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.gpSchedulerResilience = New System.Windows.Forms.GroupBox()
        Me.flowScheduleRetryTimes = New System.Windows.Forms.FlowLayoutPanel()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.numSchedulerRetryTimes = New AutomateControls.StyledNumericUpDown()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.flowScheduleRetryPeriod = New System.Windows.Forms.FlowLayoutPanel()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.numSchedulerRetryPeriod = New AutomateControls.StyledNumericUpDown()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.btnApply = New AutomateControls.Buttons.StandardStyledButton()
        Me.gpSchedulerConfig.SuspendLayout()
        Me.FullWidthFlowLayoutPanel2.SuspendLayout()
        Me.flowScheduleMinutes.SuspendLayout()
        CType(Me.numSchedulerCheckMinutes, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.gpSchedulerResilience.SuspendLayout()
        Me.flowScheduleRetryTimes.SuspendLayout()
        CType(Me.numSchedulerRetryTimes, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.flowScheduleRetryPeriod.SuspendLayout()
        CType(Me.numSchedulerRetryPeriod, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'gpSchedulerConfig
        '
        resources.ApplyResources(Me.gpSchedulerConfig, "gpSchedulerConfig")
        Me.gpSchedulerConfig.Controls.Add(Me.FullWidthFlowLayoutPanel2)
        Me.gpSchedulerConfig.Name = "gpSchedulerConfig"
        Me.gpSchedulerConfig.TabStop = False
        '
        'FullWidthFlowLayoutPanel2
        '
        resources.ApplyResources(Me.FullWidthFlowLayoutPanel2, "FullWidthFlowLayoutPanel2")
        Me.FullWidthFlowLayoutPanel2.Controls.Add(Me.Label3)
        Me.FullWidthFlowLayoutPanel2.Controls.Add(Me.cbActivateScheduler)
        Me.FullWidthFlowLayoutPanel2.Controls.Add(Me.flowScheduleMinutes)
        Me.FullWidthFlowLayoutPanel2.Name = "FullWidthFlowLayoutPanel2"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'cbActivateScheduler
        '
        resources.ApplyResources(Me.cbActivateScheduler, "cbActivateScheduler")
        Me.cbActivateScheduler.Name = "cbActivateScheduler"
        Me.cbActivateScheduler.UseVisualStyleBackColor = True
        '
        'flowScheduleMinutes
        '
        resources.ApplyResources(Me.flowScheduleMinutes, "flowScheduleMinutes")
        Me.flowScheduleMinutes.Controls.Add(Me.Label1)
        Me.flowScheduleMinutes.Controls.Add(Me.numSchedulerCheckMinutes)
        Me.flowScheduleMinutes.Controls.Add(Me.Label2)
        Me.flowScheduleMinutes.Name = "flowScheduleMinutes"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'numSchedulerCheckMinutes
        '
        resources.ApplyResources(Me.numSchedulerCheckMinutes, "numSchedulerCheckMinutes")
        Me.numSchedulerCheckMinutes.Maximum = New Decimal(New Integer() {3600, 0, 0, 0})
        Me.numSchedulerCheckMinutes.Name = "numSchedulerCheckMinutes"
        Me.numSchedulerCheckMinutes.Value = New Decimal(New Integer() {30, 0, 0, 0})
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'gpSchedulerResilience
        '
        resources.ApplyResources(Me.gpSchedulerResilience, "gpSchedulerResilience")
        Me.gpSchedulerResilience.Controls.Add(Me.flowScheduleRetryTimes)
        Me.gpSchedulerResilience.Controls.Add(Me.flowScheduleRetryPeriod)
        Me.gpSchedulerResilience.Name = "gpSchedulerResilience"
        Me.gpSchedulerResilience.TabStop = False
        '
        'flowScheduleRetryTimes
        '
        Me.flowScheduleRetryTimes.Controls.Add(Me.Label6)
        Me.flowScheduleRetryTimes.Controls.Add(Me.numSchedulerRetryTimes)
        Me.flowScheduleRetryTimes.Controls.Add(Me.Label7)
        resources.ApplyResources(Me.flowScheduleRetryTimes, "flowScheduleRetryTimes")
        Me.flowScheduleRetryTimes.Name = "flowScheduleRetryTimes"
        '
        'Label6
        '
        resources.ApplyResources(Me.Label6, "Label6")
        Me.Label6.Name = "Label6"
        '
        'numSchedulerRetryTimes
        '
        resources.ApplyResources(Me.numSchedulerRetryTimes, "numSchedulerRetryTimes")
        Me.numSchedulerRetryTimes.Name = "numSchedulerRetryTimes"
        Me.numSchedulerRetryTimes.Value = New Decimal(New Integer() {5, 0, 0, 0})
        '
        'Label7
        '
        resources.ApplyResources(Me.Label7, "Label7")
        Me.Label7.Name = "Label7"
        '
        'flowScheduleRetryPeriod
        '
        Me.flowScheduleRetryPeriod.Controls.Add(Me.Label4)
        Me.flowScheduleRetryPeriod.Controls.Add(Me.numSchedulerRetryPeriod)
        Me.flowScheduleRetryPeriod.Controls.Add(Me.Label5)
        resources.ApplyResources(Me.flowScheduleRetryPeriod, "flowScheduleRetryPeriod")
        Me.flowScheduleRetryPeriod.Name = "flowScheduleRetryPeriod"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'numSchedulerRetryPeriod
        '
        resources.ApplyResources(Me.numSchedulerRetryPeriod, "numSchedulerRetryPeriod")
        Me.numSchedulerRetryPeriod.Maximum = New Decimal(New Integer() {300, 0, 0, 0})
        Me.numSchedulerRetryPeriod.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.numSchedulerRetryPeriod.Name = "numSchedulerRetryPeriod"
        Me.numSchedulerRetryPeriod.Value = New Decimal(New Integer() {5, 0, 0, 0})
        '
        'Label5
        '
        resources.ApplyResources(Me.Label5, "Label5")
        Me.Label5.Name = "Label5"
        '
        'btnApply
        '
        resources.ApplyResources(Me.btnApply, "btnApply")
        Me.btnApply.Name = "btnApply"
        Me.btnApply.UseVisualStyleBackColor = True
        '
        'ctlSystemScheduler
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.btnApply)
        Me.Controls.Add(Me.gpSchedulerConfig)
        Me.Controls.Add(Me.gpSchedulerResilience)
        Me.Name = "ctlSystemScheduler"
        resources.ApplyResources(Me, "$this")
        Me.gpSchedulerConfig.ResumeLayout(False)
        Me.gpSchedulerConfig.PerformLayout()
        Me.FullWidthFlowLayoutPanel2.ResumeLayout(False)
        Me.FullWidthFlowLayoutPanel2.PerformLayout()
        Me.flowScheduleMinutes.ResumeLayout(False)
        Me.flowScheduleMinutes.PerformLayout()
        CType(Me.numSchedulerCheckMinutes, System.ComponentModel.ISupportInitialize).EndInit()
        Me.gpSchedulerResilience.ResumeLayout(False)
        Me.flowScheduleRetryTimes.ResumeLayout(False)
        Me.flowScheduleRetryTimes.PerformLayout()
        CType(Me.numSchedulerRetryTimes, System.ComponentModel.ISupportInitialize).EndInit()
        Me.flowScheduleRetryPeriod.ResumeLayout(False)
        Me.flowScheduleRetryPeriod.PerformLayout()
        CType(Me.numSchedulerRetryPeriod, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents gpSchedulerConfig As System.Windows.Forms.GroupBox
    Friend WithEvents FullWidthFlowLayoutPanel2 As AutomateControls.FullWidthFlowLayoutPanel
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Private WithEvents cbActivateScheduler As System.Windows.Forms.CheckBox
    Private WithEvents flowScheduleMinutes As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents numSchedulerCheckMinutes As AutomateControls.StyledNumericUpDown
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Private WithEvents gpSchedulerResilience As System.Windows.Forms.GroupBox
    Private WithEvents flowScheduleRetryTimes As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents numSchedulerRetryTimes As AutomateControls.StyledNumericUpDown
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Private WithEvents flowScheduleRetryPeriod As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents numSchedulerRetryPeriod As AutomateControls.StyledNumericUpDown
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents btnApply As AutomateControls.Buttons.StandardStyledButton
End Class
