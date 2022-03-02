<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlNamedCalendar
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlNamedCalendar))
        Me.mWorkingWeekGroup = New System.Windows.Forms.GroupBox()
        Me.mWorkingWeekLabel = New System.Windows.Forms.Label()
        Me.mSundayCheckbox = New System.Windows.Forms.CheckBox()
        Me.mSaturdayCheckbox = New System.Windows.Forms.CheckBox()
        Me.mFridayCheckbox = New System.Windows.Forms.CheckBox()
        Me.mThursdayCheckbox = New System.Windows.Forms.CheckBox()
        Me.mWednesdayCheckbox = New System.Windows.Forms.CheckBox()
        Me.mTuesdayCheckbox = New System.Windows.Forms.CheckBox()
        Me.mMondayCheckbox = New System.Windows.Forms.CheckBox()
        Me.mPublicHolidayCombo = New System.Windows.Forms.ComboBox()
        Me.mPublicHolidayListBox = New System.Windows.Forms.CheckedListBox()
        Me.mPublicHolidayGroup = New System.Windows.Forms.GroupBox()
        Me.IncludeDatesCheckbox = New System.Windows.Forms.CheckBox()
        Me.lblIncluding = New System.Windows.Forms.Label()
        Me.mPublicHolidayLabel = New System.Windows.Forms.Label()
        Me.mOtherHolidaysGroup = New System.Windows.Forms.GroupBox()
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.mOtherHolidaysAddLink = New System.Windows.Forms.LinkLabel()
        Me.mOtherHolidaysRemoveLink = New System.Windows.Forms.LinkLabel()
        Me.mCultureDatePicker = New CustomControls.DatePicker()
        Me.mOtherHolidayDatePicker = New System.Windows.Forms.DateTimePicker()
        Me.mOtherHolidaysList = New System.Windows.Forms.ListBox()
        Me.mOtherHolidaysLabel = New System.Windows.Forms.Label()
        Me.mCalendarLabel = New System.Windows.Forms.Label()
        Me.mSaveCalendarButton = New AutomateControls.Buttons.StandardStyledButton()
        Me.mDeleteCalendarButton = New AutomateControls.Buttons.StandardStyledButton()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.mNewCalendarButton = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnReferences = New AutomateControls.Buttons.StandardStyledButton()
        Me.mCalendarCombo = New AutomateControls.SelectionPreviewComboBox()
        Me.mWorkingWeekGroup.SuspendLayout()
        Me.mPublicHolidayGroup.SuspendLayout()
        Me.mOtherHolidaysGroup.SuspendLayout()
        Me.FlowLayoutPanel1.SuspendLayout()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'mWorkingWeekGroup
        '
        Me.mWorkingWeekGroup.Controls.Add(Me.mWorkingWeekLabel)
        Me.mWorkingWeekGroup.Controls.Add(Me.mSundayCheckbox)
        Me.mWorkingWeekGroup.Controls.Add(Me.mSaturdayCheckbox)
        Me.mWorkingWeekGroup.Controls.Add(Me.mFridayCheckbox)
        Me.mWorkingWeekGroup.Controls.Add(Me.mThursdayCheckbox)
        Me.mWorkingWeekGroup.Controls.Add(Me.mWednesdayCheckbox)
        Me.mWorkingWeekGroup.Controls.Add(Me.mTuesdayCheckbox)
        Me.mWorkingWeekGroup.Controls.Add(Me.mMondayCheckbox)
        resources.ApplyResources(Me.mWorkingWeekGroup, "mWorkingWeekGroup")
        Me.mWorkingWeekGroup.Name = "mWorkingWeekGroup"
        Me.mWorkingWeekGroup.TabStop = False
        '
        'mWorkingWeekLabel
        '
        resources.ApplyResources(Me.mWorkingWeekLabel, "mWorkingWeekLabel")
        Me.mWorkingWeekLabel.Name = "mWorkingWeekLabel"
        '
        'mSundayCheckbox
        '
        resources.ApplyResources(Me.mSundayCheckbox, "mSundayCheckbox")
        Me.mSundayCheckbox.Name = "mSundayCheckbox"
        Me.mSundayCheckbox.UseVisualStyleBackColor = True
        '
        'mSaturdayCheckbox
        '
        resources.ApplyResources(Me.mSaturdayCheckbox, "mSaturdayCheckbox")
        Me.mSaturdayCheckbox.Name = "mSaturdayCheckbox"
        Me.mSaturdayCheckbox.UseVisualStyleBackColor = True
        '
        'mFridayCheckbox
        '
        resources.ApplyResources(Me.mFridayCheckbox, "mFridayCheckbox")
        Me.mFridayCheckbox.Checked = True
        Me.mFridayCheckbox.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mFridayCheckbox.Name = "mFridayCheckbox"
        Me.mFridayCheckbox.UseVisualStyleBackColor = True
        '
        'mThursdayCheckbox
        '
        resources.ApplyResources(Me.mThursdayCheckbox, "mThursdayCheckbox")
        Me.mThursdayCheckbox.Checked = True
        Me.mThursdayCheckbox.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mThursdayCheckbox.Name = "mThursdayCheckbox"
        Me.mThursdayCheckbox.UseVisualStyleBackColor = True
        '
        'mWednesdayCheckbox
        '
        resources.ApplyResources(Me.mWednesdayCheckbox, "mWednesdayCheckbox")
        Me.mWednesdayCheckbox.Checked = True
        Me.mWednesdayCheckbox.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mWednesdayCheckbox.Name = "mWednesdayCheckbox"
        Me.mWednesdayCheckbox.UseVisualStyleBackColor = True
        '
        'mTuesdayCheckbox
        '
        resources.ApplyResources(Me.mTuesdayCheckbox, "mTuesdayCheckbox")
        Me.mTuesdayCheckbox.Checked = True
        Me.mTuesdayCheckbox.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mTuesdayCheckbox.Name = "mTuesdayCheckbox"
        Me.mTuesdayCheckbox.UseVisualStyleBackColor = True
        '
        'mMondayCheckbox
        '
        resources.ApplyResources(Me.mMondayCheckbox, "mMondayCheckbox")
        Me.mMondayCheckbox.Checked = True
        Me.mMondayCheckbox.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mMondayCheckbox.Name = "mMondayCheckbox"
        Me.mMondayCheckbox.UseVisualStyleBackColor = True
        '
        'mPublicHolidayCombo
        '
        resources.ApplyResources(Me.mPublicHolidayCombo, "mPublicHolidayCombo")
        Me.mPublicHolidayCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.mPublicHolidayCombo.FormattingEnabled = True
        Me.mPublicHolidayCombo.Name = "mPublicHolidayCombo"
        '
        'mPublicHolidayListBox
        '
        resources.ApplyResources(Me.mPublicHolidayListBox, "mPublicHolidayListBox")
        Me.mPublicHolidayListBox.CheckOnClick = True
        Me.mPublicHolidayListBox.FormattingEnabled = True
        Me.mPublicHolidayListBox.Name = "mPublicHolidayListBox"
        '
        'mPublicHolidayGroup
        '
        Me.mPublicHolidayGroup.Controls.Add(Me.IncludeDatesCheckbox)
        Me.mPublicHolidayGroup.Controls.Add(Me.lblIncluding)
        Me.mPublicHolidayGroup.Controls.Add(Me.mPublicHolidayLabel)
        Me.mPublicHolidayGroup.Controls.Add(Me.mPublicHolidayCombo)
        Me.mPublicHolidayGroup.Controls.Add(Me.mPublicHolidayListBox)
        resources.ApplyResources(Me.mPublicHolidayGroup, "mPublicHolidayGroup")
        Me.mPublicHolidayGroup.Name = "mPublicHolidayGroup"
        Me.mPublicHolidayGroup.TabStop = False
        '
        'IncludeDatesCheckbox
        '
        resources.ApplyResources(Me.IncludeDatesCheckbox, "IncludeDatesCheckbox")
        Me.IncludeDatesCheckbox.Name = "IncludeDatesCheckbox"
        Me.IncludeDatesCheckbox.UseVisualStyleBackColor = True
        '
        'lblIncluding
        '
        resources.ApplyResources(Me.lblIncluding, "lblIncluding")
        Me.lblIncluding.Name = "lblIncluding"
        '
        'mPublicHolidayLabel
        '
        resources.ApplyResources(Me.mPublicHolidayLabel, "mPublicHolidayLabel")
        Me.mPublicHolidayLabel.Name = "mPublicHolidayLabel"
        '
        'mOtherHolidaysGroup
        '
        Me.mOtherHolidaysGroup.Controls.Add(Me.FlowLayoutPanel1)
        Me.mOtherHolidaysGroup.Controls.Add(Me.mCultureDatePicker)
        Me.mOtherHolidaysGroup.Controls.Add(Me.mOtherHolidayDatePicker)
        Me.mOtherHolidaysGroup.Controls.Add(Me.mOtherHolidaysList)
        Me.mOtherHolidaysGroup.Controls.Add(Me.mOtherHolidaysLabel)
        resources.ApplyResources(Me.mOtherHolidaysGroup, "mOtherHolidaysGroup")
        Me.mOtherHolidaysGroup.Name = "mOtherHolidaysGroup"
        Me.mOtherHolidaysGroup.TabStop = False
        '
        'FlowLayoutPanel1
        '
        resources.ApplyResources(Me.FlowLayoutPanel1, "FlowLayoutPanel1")
        Me.FlowLayoutPanel1.Controls.Add(Me.mOtherHolidaysAddLink)
        Me.FlowLayoutPanel1.Controls.Add(Me.mOtherHolidaysRemoveLink)
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        '
        'mOtherHolidaysAddLink
        '
        resources.ApplyResources(Me.mOtherHolidaysAddLink, "mOtherHolidaysAddLink")
        Me.mOtherHolidaysAddLink.Name = "mOtherHolidaysAddLink"
        Me.mOtherHolidaysAddLink.TabStop = True
        '
        'mOtherHolidaysRemoveLink
        '
        resources.ApplyResources(Me.mOtherHolidaysRemoveLink, "mOtherHolidaysRemoveLink")
        Me.mOtherHolidaysRemoveLink.Name = "mOtherHolidaysRemoveLink"
        Me.mOtherHolidaysRemoveLink.TabStop = True
        '
        'mCultureDatePicker
        '
        resources.ApplyResources(Me.mCultureDatePicker, "mCultureDatePicker")
        Me.mCultureDatePicker.Name = "mCultureDatePicker"
        '
        'mOtherHolidayDatePicker
        '
        resources.ApplyResources(Me.mOtherHolidayDatePicker, "mOtherHolidayDatePicker")
        Me.mOtherHolidayDatePicker.Name = "mOtherHolidayDatePicker"
        '
        'mOtherHolidaysList
        '
        resources.ApplyResources(Me.mOtherHolidaysList, "mOtherHolidaysList")
        Me.mOtherHolidaysList.FormattingEnabled = True
        Me.mOtherHolidaysList.Name = "mOtherHolidaysList"
        Me.mOtherHolidaysList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        '
        'mOtherHolidaysLabel
        '
        resources.ApplyResources(Me.mOtherHolidaysLabel, "mOtherHolidaysLabel")
        Me.mOtherHolidaysLabel.Name = "mOtherHolidaysLabel"
        '
        'mCalendarLabel
        '
        resources.ApplyResources(Me.mCalendarLabel, "mCalendarLabel")
        Me.mCalendarLabel.Name = "mCalendarLabel"
        '
        'mSaveCalendarButton
        '
        resources.ApplyResources(Me.mSaveCalendarButton, "mSaveCalendarButton")
        Me.mSaveCalendarButton.Image = Global.AutomateUI.My.Resources.ToolImages.Save_16x16
        Me.mSaveCalendarButton.Name = "mSaveCalendarButton"
        Me.mSaveCalendarButton.UseVisualStyleBackColor = True
        '
        'mDeleteCalendarButton
        '
        resources.ApplyResources(Me.mDeleteCalendarButton, "mDeleteCalendarButton")
        Me.mDeleteCalendarButton.Image = Global.AutomateUI.My.Resources.ToolImages.Delete_Red_16x16
        Me.mDeleteCalendarButton.Name = "mDeleteCalendarButton"
        Me.mDeleteCalendarButton.UseVisualStyleBackColor = True
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.mWorkingWeekGroup, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.mPublicHolidayGroup, 1, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.mOtherHolidaysGroup, 2, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'mNewCalendarButton
        '
        resources.ApplyResources(Me.mNewCalendarButton, "mNewCalendarButton")
        Me.mNewCalendarButton.Image = Global.AutomateUI.My.Resources.ToolImages.New_16x16
        Me.mNewCalendarButton.Name = "mNewCalendarButton"
        Me.mNewCalendarButton.UseVisualStyleBackColor = True
        '
        'btnReferences
        '
        resources.ApplyResources(Me.btnReferences, "btnReferences")
        Me.btnReferences.Image = Global.AutomateUI.My.Resources.ToolImages.Site_Map2_16x16
        Me.btnReferences.Name = "btnReferences"
        Me.btnReferences.UseVisualStyleBackColor = True
        '
        'mCalendarCombo
        '
        resources.ApplyResources(Me.mCalendarCombo, "mCalendarCombo")
        Me.mCalendarCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.mCalendarCombo.FormattingEnabled = True
        Me.mCalendarCombo.Name = "mCalendarCombo"
        '
        'ctlNamedCalendar
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.btnReferences)
        Me.Controls.Add(Me.mNewCalendarButton)
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Controls.Add(Me.mDeleteCalendarButton)
        Me.Controls.Add(Me.mSaveCalendarButton)
        Me.Controls.Add(Me.mCalendarLabel)
        Me.Controls.Add(Me.mCalendarCombo)
        resources.ApplyResources(Me, "$this")
        Me.Name = "ctlNamedCalendar"
        Me.mWorkingWeekGroup.ResumeLayout(False)
        Me.mWorkingWeekGroup.PerformLayout()
        Me.mPublicHolidayGroup.ResumeLayout(False)
        Me.mPublicHolidayGroup.PerformLayout()
        Me.mOtherHolidaysGroup.ResumeLayout(False)
        Me.mOtherHolidaysGroup.PerformLayout()
        Me.FlowLayoutPanel1.ResumeLayout(False)
        Me.FlowLayoutPanel1.PerformLayout()
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents mWorkingWeekGroup As System.Windows.Forms.GroupBox
    Private WithEvents mWorkingWeekLabel As System.Windows.Forms.Label
    Private WithEvents mPublicHolidayCombo As System.Windows.Forms.ComboBox
    Private WithEvents mPublicHolidayListBox As System.Windows.Forms.CheckedListBox
    Private WithEvents mPublicHolidayGroup As System.Windows.Forms.GroupBox
    Private WithEvents mPublicHolidayLabel As System.Windows.Forms.Label
    Private WithEvents mSundayCheckbox As System.Windows.Forms.CheckBox
    Private WithEvents mSaturdayCheckbox As System.Windows.Forms.CheckBox
    Private WithEvents mFridayCheckbox As System.Windows.Forms.CheckBox
    Private WithEvents mThursdayCheckbox As System.Windows.Forms.CheckBox
    Private WithEvents mWednesdayCheckbox As System.Windows.Forms.CheckBox
    Private WithEvents mTuesdayCheckbox As System.Windows.Forms.CheckBox
    Private WithEvents mMondayCheckbox As System.Windows.Forms.CheckBox
    Private WithEvents mOtherHolidaysGroup As System.Windows.Forms.GroupBox
    Private WithEvents mOtherHolidaysLabel As System.Windows.Forms.Label
    Private WithEvents mOtherHolidaysList As System.Windows.Forms.ListBox
    Private WithEvents mOtherHolidaysRemoveLink As System.Windows.Forms.LinkLabel
    Private WithEvents mOtherHolidaysAddLink As System.Windows.Forms.LinkLabel
    Private WithEvents mOtherHolidayDatePicker As System.Windows.Forms.DateTimePicker
    Private WithEvents mCalendarLabel As System.Windows.Forms.Label
    Private WithEvents mCalendarCombo As AutomateControls.SelectionPreviewComboBox
    Private WithEvents mSaveCalendarButton As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents mDeleteCalendarButton As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Private WithEvents mNewCalendarButton As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents lblIncluding As System.Windows.Forms.Label
    Friend WithEvents btnReferences As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents mCultureDatePicker As CustomControls.DatePicker
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
    Friend WithEvents IncludeDatesCheckbox As CheckBox
End Class
