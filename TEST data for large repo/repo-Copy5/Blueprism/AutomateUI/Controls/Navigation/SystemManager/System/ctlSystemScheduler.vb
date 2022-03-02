Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib

Public Class ctlSystemScheduler
    Implements IStubbornChild
    Implements IPermission

    ''' <summary>
    ''' Flag to indicate when the control is loading / populating itself, so
    ''' that event listeners listening to control state changes can act
    ''' accordingly.
    ''' </summary>
    Private mLoading As Boolean

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        ' Set the maximums for the up-down controls from the config class
        numSchedulerCheckMinutes.Maximum = SchedulerConfig.MaximumCheckSeconds \ 60
        numSchedulerRetryPeriod.Maximum = SchedulerConfig.MaximumRetryPeriod
        numSchedulerRetryTimes.Maximum = SchedulerConfig.MaximumRetryTimes

        Me.Enabled = Licensing.License.CanUse(LicenseUse.Scheduler)
    End Sub

    ''' <summary>
    ''' Populates the scheduler UI with the data from the current system prefs and
    ''' ensures the controls are in their relevant state (ie. mostly disabled if
    ''' the scheduler is inactive on this connection).
    ''' </summary>
    Private Sub PopulateScheduler()
        cbActivateScheduler.Checked = SchedulerConfig.Active
        ' The prefs holds the check value as seconds - conversion required.
        numSchedulerCheckMinutes.Value = SchedulerConfig.CheckSeconds \ 60
        numSchedulerRetryPeriod.Value = SchedulerConfig.RetryPeriod
        numSchedulerRetryTimes.Value = SchedulerConfig.RetryTimes
        UpdateSchedulerControlState()
    End Sub

    Private Sub ctlSystemScheduler_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        mLoading = True
        Try
            PopulateScheduler()
        Finally
            mLoading = False
        End Try
    End Sub

    ''' <summary>
    ''' Handles the scheduler being activated / deactivated. This updates the state
    ''' of the scheduler controls and updates the appropriate system pref.
    ''' </summary>
    Private Sub cbActivateScheduler_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles cbActivateScheduler.CheckedChanged
        ' Ensure the other controls are enabled / disabled as appropriate.
        UpdateSchedulerControlState()
        ' Only update the pref if we're not setting the checkbox as a result of
        ' loading the form.
        If Not mLoading Then ChangesMade()
    End Sub

    ''' <summary>
    ''' Enables or disables the scheduler controls as appropriate according to the
    ''' current state of the 'activate scheduler' checkbox.
    ''' </summary>
    Private Sub UpdateSchedulerControlState()
        Dim enable As Boolean = cbActivateScheduler.Checked
        flowScheduleMinutes.Enabled = enable
        flowScheduleRetryPeriod.Enabled = enable
        flowScheduleRetryTimes.Enabled = enable
    End Sub

    ''' <summary>
    ''' Handles the scheduler textboxes being validated.
    ''' This ensures that the appropriate system pref is updated.
    ''' </summary>
    Private Sub HandlesTextboxChanges(ByVal sender As Object, ByVal e As EventArgs) _
     Handles numSchedulerCheckMinutes.ValueChanged, numSchedulerRetryPeriod.ValueChanged, numSchedulerRetryTimes.ValueChanged
        If Not mLoading Then ChangesMade()
    End Sub

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public ReadOnly Property RequiredPermissions() As System.Collections.Generic.ICollection(Of BluePrism.AutomateAppCore.Auth.Permission) Implements BluePrism.AutomateAppCore.Auth.IPermission.RequiredPermissions
        Get
            Return Permission.ByName(Permission.SystemManager.System.Scheduler)
        End Get
    End Property

    Private Sub ChangesMade()
        If Not btnApply.Enabled Then btnApply.Enabled = True
    End Sub

    Private Sub HandleClickApplyButton(sender As Object, e As EventArgs) Handles btnApply.Click
        SaveChanges()
    End Sub

    Private Function SaveChanges() As Boolean
        Try
            gSv.SetSchedulerConfig(cbActivateScheduler.Checked,
                                   CInt(numSchedulerCheckMinutes.Value) * 60,
                                   CInt(numSchedulerRetryTimes.Value),
                                   CInt(numSchedulerRetryPeriod.Value))
            btnApply.Enabled = False
            Return True
        Catch ex As Exception
            UserMessage.Err(ex.Message)
            Return False
        End Try
    End Function

    Private Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        ' Check if there are any unsaved changes
        If Not btnApply.Enabled Then Return True

        Dim warning As String =
         My.Resources.ctlSystemScheduler_ThereAreUnsavedChangesToTheSchedulerConfigurationDoYouWantToSaveTheChanges

        ' And see what they want to do
        Dim response = UserMessage.YesNoCancel(warning, True)
        Select Case response
            Case MsgBoxResult.No
                btnApply.Enabled = False
                Return True
            Case MsgBoxResult.Yes : Return SaveChanges()
            Case Else : Return False
        End Select

    End Function

End Class
