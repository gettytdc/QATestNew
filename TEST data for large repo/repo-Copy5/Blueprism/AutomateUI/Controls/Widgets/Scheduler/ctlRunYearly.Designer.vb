<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlRunYearly
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlRunYearly))
        Dim Label27 As System.Windows.Forms.Label
        Me.updnPeriod = New AutomateControls.StyledNumericUpDown()
        Me.mYearLabel = New System.Windows.Forms.Label()
        mRunMonthlyGroupPanel = New System.Windows.Forms.GroupBox()
        Label27 = New System.Windows.Forms.Label()
        mRunMonthlyGroupPanel.SuspendLayout()
        CType(Me.updnPeriod, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'mRunMonthlyGroupPanel
        '
        mRunMonthlyGroupPanel.Controls.Add(Me.updnPeriod)
        mRunMonthlyGroupPanel.Controls.Add(Me.mYearLabel)
        mRunMonthlyGroupPanel.Controls.Add(Label27)
        resources.ApplyResources(mRunMonthlyGroupPanel, "mRunMonthlyGroupPanel")
        mRunMonthlyGroupPanel.Name = "mRunMonthlyGroupPanel"
        mRunMonthlyGroupPanel.TabStop = False
        '
        'updnPeriod
        '
        resources.ApplyResources(Me.updnPeriod, "updnPeriod")
        Me.updnPeriod.Maximum = New Decimal(New Integer() {999, 0, 0, 0})
        Me.updnPeriod.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.updnPeriod.Name = "updnPeriod"
        Me.updnPeriod.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'mYearLabel
        '
        resources.ApplyResources(Me.mYearLabel, "mYearLabel")
        Me.mYearLabel.Name = "mYearLabel"
        '
        'Label27
        '
        resources.ApplyResources(Label27, "Label27")
        Label27.Name = "Label27"
        '
        'ctlRunYearly
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(mRunMonthlyGroupPanel)
        Me.DoubleBuffered = True
        resources.ApplyResources(Me, "$this")
        Me.Name = "ctlRunYearly"
        mRunMonthlyGroupPanel.ResumeLayout(False)
        mRunMonthlyGroupPanel.PerformLayout()
        CType(Me.updnPeriod, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents updnPeriod As AutomateControls.StyledNumericUpDown
    Private WithEvents mYearLabel As System.Windows.Forms.Label

End Class
