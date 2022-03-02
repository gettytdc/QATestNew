Imports AutomateControls.UIState.UIElements
Imports AutomateUI.Classes
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.BPCoreLib.Collections

''' Project  : Automate
''' Class    : ctlTask
''' 
''' <summary>
''' Allows users to view an modify Tasks.
''' </summary>
Public Class ctlTask
    Implements IChild
    Implements IScheduleModifier

#Region " Class-scope definitions: Inner classes, events, enums, constants "

    ''' <summary>
    ''' Event raised whenever the name of the task changes.
    ''' </summary>
    Public Event NameChanged(ByVal args As TaskNameChangedEventArgs)

    ''' <summary>
    ''' Class to store name change event details.
    ''' </summary>
    Public Class TaskNameChangedEventArgs : Inherits EventArgs

        Public mNewName As String
        Public mOldName As String
        Public mSourceTask As ScheduledTask

        ''' <summary>
        ''' The new name of the task
        ''' </summary>
        Public ReadOnly Property NewName() As String
            Get
                Return mNewName
            End Get
        End Property
        ''' <summary>
        ''' The old name of the task
        ''' </summary>
        Public ReadOnly Property OldName() As String
            Get
                Return mOldName
            End Get
        End Property
        ''' <summary>
        ''' The task whose name has changed
        ''' </summary>
        Public ReadOnly Property SourceTask() As ScheduledTask
            Get
                Return mSourceTask
            End Get
        End Property

        ''' <summary>
        ''' Creates a new task name change event args object
        ''' </summary>
        ''' <param name="source">The task whose name has changed</param>
        ''' <param name="oldname">The old name of the task</param>
        ''' <param name="newname">The new name of the task</param>
        Public Sub New(ByVal source As ScheduledTask, ByVal oldname As String, ByVal newname As String)
            mSourceTask = source
            mOldName = oldname
            mNewName = newname
        End Sub
    End Class

#End Region

#Region " Member vars "

    ' The task that is being displayed / modified by this control
    Private mTask As ScheduledTask

    ' Flag indicating read-only state of this control
    Private mReadonly As Boolean

    ' The form which ultimately owns this control - initialised in this control or
    ' the current top level control's ParentChanged handler
    Private WithEvents mOwnerForm As Form

    Private ReadOnly mUserMessage As IUserMessage

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new, blank, Task control
    ''' </summary>
    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        Dim incrSize = If(Label1.Width > txtName.Left, Label1.Width - txtName.Left, 0)
        incrSize = If(Label2.Width > txtDescription.Left And Label2.Width - txtDescription.Left > incrSize, Label2.Width - txtDescription.Left, 0)
        If incrSize > 0 Then
            incrSize += Label1.Margin.Right
            txtName.Left = txtName.Left + incrSize
            txtName.Width = txtName.Width - incrSize
            txtDescription.Left = txtDescription.Left + incrSize
            txtDescription.Width = txtDescription.Width - incrSize
        End If

        mUserMessage = New UserMessageWrapper()

        ' Add any initialization after the InitializeComponent() call.
        Me.ReadOnly = Not User.Current.HasPermission("Edit Schedule")
    End Sub

#End Region

#Region " IScheduleModifier implementation "

    ''' <summary>
    ''' Event fired when the schedule data has changed.
    ''' </summary>
    ''' <param name="sender">The schedule whose data has changed as a result of a
    '''  change on this class.</param>
    Public Event ScheduleDataChange(ByVal sender As SessionRunnerSchedule) _
     Implements IScheduleModifier.ScheduleDataChange

    ''' <summary>
    ''' Raises a change event for the schedule data, if this control is currently
    ''' assigned to a ctlSchedule control (which has a schedule assigned to it).
    ''' </summary>
    Private Sub RaiseScheduleChangeEvent()
        Dim sched As SessionRunnerSchedule = Nothing
        If mTask IsNot Nothing Then sched = mTask.Owner
        If sched IsNot Nothing Then
            RaiseEvent ScheduleDataChange(sched)
        End If
    End Sub

#End Region

#Region " IChild implementation "

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
            listResources.ParentAppForm = mParent
        End Set
    End Property

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the read-only state of this control
    ''' </summary>
    Public Property [ReadOnly]() As Boolean
        Get
            Return mReadonly
        End Get
        Set(ByVal value As Boolean)
            mReadonly = value 'This member variable is checked in event handlers

            txtName.ReadOnly = value
            txtDescription.ReadOnly = value
            PostCompletionDelayNumericInput.ReadOnly = value

            clsUserInterfaceUtils.ShowReadOnlyControl(value, comboOnCompletion, txtOnCompletion)
            clsUserInterfaceUtils.ShowReadOnlyControl(value, comboOnException, txtOnException)

            cbFailFast.Enabled = Not value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the task being modelled by this class
    ''' </summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Property Task() As ScheduledTask
        Get
            Return mTask
        End Get
        Set(ByVal value As ScheduledTask)
            SuspendLayout()
            Try
                Dim savedReadOnlyState As Boolean = Me.ReadOnly
                Me.ReadOnly = False
                mTask = Nothing
                comboOnCompletion.Items.Clear()
                comboOnException.Items.Clear()
                comboOnCompletion.Items.Add(My.Resources.ctlTask_Task_Stop)
                comboOnException.Items.Add(My.Resources.ctlTask_Abort)

                If value IsNot Nothing Then
                    Dim sched As SessionRunnerSchedule = value.Owner
                    For Each task As ScheduledTask In sched
                        If Not Object.ReferenceEquals(task, value) Then
                            comboOnCompletion.Items.Add(task)
                            comboOnException.Items.Add(task)
                        End If
                    Next
                    txtName.Text = value.Name
                    txtDescription.Text = value.Description
                    PostCompletionDelayNumericInput.Value = Convert.ToDecimal(value.DelayAfterEnd)

                    comboOnCompletion.SelectedItem = value.OnSuccess
                    If comboOnCompletion.SelectedIndex = -1 Then _
                     comboOnCompletion.SelectedIndex = 0

                    comboOnException.SelectedItem = value.OnFailure
                    If comboOnException.SelectedIndex = -1 Then _
                     comboOnException.SelectedIndex = 0

                    cbFailFast.Checked = value.FailFastOnError

                    listSessions.Items.Clear()
                    For Each sess As ScheduledSession In value.Sessions
                        Dim item = New ListViewItem()
                        item.Text = If(sess.CanCurrentUserSeeProcess AndAlso sess.CanCurrentUserSeeResource,
                            String.Empty, My.Resources.AutomateUI_Controls.General_WarningSymbol)
                        item.SubItems.AddRange(
                            {
                                gSv.GetProcessNameByID(sess.ProcessId),
                                sess.ResourceName
                            })
                        item.SubItems.Add(sess.ResourceName)
                        item.Tag = sess
                        item.ToolTipText = If(sess.CanCurrentUserSeeProcess AndAlso sess.CanCurrentUserSeeResource,
                            String.Empty, My.Resources.AutomateUI_Controls.ctlTask_Task_ProcessNotVisibleToUser)
                        listSessions.Items.Add(item)
                    Next
                End If

                listProcesses.Filter =
                    ProcessGroupMember.PublishedAndNotRetiredFilter
                listResources.RefreshView()
                mTask = value
                Me.ReadOnly = savedReadOnlyState
            Finally
                ResumeLayout()
            End Try
        End Set
    End Property

#End Region

#Region " Member Methods "

    ''' <summary>
    ''' Adds a session to the sessions listview and adds the session to the task
    ''' </summary>
    ''' <param name="processID">The process to add to the session</param>
    ''' <param name="resourceName">The resource to add to the session</param>
    Private Sub AddSession(ByVal processID As Guid, ByVal resourceName As String)
        Dim processName As String = gSv.GetProcessNameByID(processID)

        Dim item = New ListViewItem()
        item.SubItems.AddRange({processName, resourceName})
        listSessions.Items.Add(item)

        Const CanCurrentUserSeeProcess As Boolean = True
        Const CanCurrentUserSeeResource As Boolean = True

        Dim s As New ScheduledSession(0, processID, resourceName, Guid.Empty,
                                      CanCurrentUserSeeProcess, CanCurrentUserSeeResource, Nothing)
        item.Tag = s
        mTask.AddSession(s)
    End Sub

#End Region

#Region " Overrides "

    ''' <summary>
    ''' Handles the parent changing on this control by setting the owner form, and
    ''' registering an interest in the top level control's parent, if there is no
    ''' form holding this control at the moment.
    ''' </summary>
    Protected Overrides Sub OnParentChanged(ByVal e As EventArgs)
        MyBase.OnParentChanged(e)
        mOwnerForm = FindForm()

        ' If not, find the top ancestor control and add this method as ParentChanged handler
        If mOwnerForm Is Nothing Then
            Dim tlc As Control = TopLevelControl
            If tlc IsNot Nothing Then _
             AddHandler tlc.ParentChanged, AddressOf HandleAncestorParentChanged
        End If

    End Sub

    ''' <summary>
    ''' Handles the (previous) top level control's parent changing, by trying to get
    ''' the owning form of this control. If there is none, yet, it re-establishes an
    ''' interest in the (current) top level control's parent changing.
    ''' </summary>
    Private Sub HandleAncestorParentChanged(ByVal sender As Object, ByVal e As EventArgs)

        ' Cast and remove this handler first
        Dim ctl As Control = DirectCast(sender, Control)
        RemoveHandler ctl.ParentChanged, AddressOf HandleAncestorParentChanged

        ' Get the form, if it's there
        mOwnerForm = FindForm()

        ' If not, find the top ancestor control and add this method as ParentChanged handler
        If mOwnerForm Is Nothing Then
            Dim tlc As Control = TopLevelControl
            If tlc IsNot Nothing Then _
             AddHandler tlc.ParentChanged, AddressOf HandleAncestorParentChanged
        End If

    End Sub

    ''' <summary>
    ''' Suspends the layout of this control while the form is being resized.
    ''' </summary>
    Private Sub HandleFormResizeStart(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mOwnerForm.ResizeBegin
        SuspendLayout()
    End Sub

    ''' <summary>
    ''' Resumes the layout of this control after the form is being resized.
    ''' </summary>
    Private Sub HandleFormResizeEnd(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mOwnerForm.ResizeEnd
        ResumeLayout()
    End Sub

#End Region

#Region " UI Event Handlers "

    ''' <summary>
    '''  Updates the task name when the textbox is edited.
    ''' </summary>
    Private Sub HandleNameTextChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtName.TextChanged
        If mTask Is Nothing Then Return
        RaiseEvent NameChanged(New TaskNameChangedEventArgs(mTask, mTask.Name, txtName.Text))
        mTask.Name = txtName.Text
        RaiseScheduleChangeEvent()
    End Sub

    ''' <summary>
    ''' Updates the description when the textbox is edited
    ''' </summary>
    Private Sub HandleDescriptionTextChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtDescription.TextChanged
        If mTask Is Nothing Then Return
        mTask.Description = txtDescription.Text
        RaiseScheduleChangeEvent()
    End Sub

    ''' <summary>
    ''' Updates the onsuccess of the task when the combobox is changed
    ''' </summary>
    Private Sub HandleOnCompleteChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles comboOnCompletion.SelectedIndexChanged

        If mTask Is Nothing Then Return
        mTask.OnSuccess = TryCast(comboOnCompletion.SelectedItem, ScheduledTask)
        RaiseScheduleChangeEvent()
    End Sub

    ''' <summary>
    ''' Updates the onfailure of the task when the combobox is changed
    ''' </summary>
    Private Sub HandleOnFailChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles comboOnException.SelectedIndexChanged
        If mTask Is Nothing Then Return
        mTask.OnFailure = TryCast(comboOnException.SelectedItem, ScheduledTask)
        RaiseScheduleChangeEvent()
    End Sub

    ''' <summary>
    ''' Updates the sessions listview when a session selection is made
    ''' </summary>
    Private Sub HandleSessionsCreated( _
     ByVal sender As Object, ByVal e As GroupMemberDropEventArgs) _
     Handles listProcesses.GroupMemberDropped, listResources.GroupMemberDropped

        ' Ignore empty contents (TODO: can this happen? It probably shouldn't)
        If e.Contents.Count = 0 Then Return

        ' Ignore if we're currently in readonly mode
        If mReadonly Then Return

        ' Ignore if we're trying to drop process onto process or resource onto resource
        If e.Target.GetType().Equals(e.Contents.First().GetType()) Then Return

        ' We're either dropping multiple processes onto a single resource, or
        ' multiple resources onto a single process... determine which and
        ' assign the collections accordingly
        Dim processes As ICollection(Of IGroupMember)
        Dim resources As ICollection(Of IGroupMember)
        If TypeOf e.Target Is ProcessGroupMember Then
            Debug.Assert(TypeOf e.Contents.First() Is ResourceGroupMember)
            processes = GetSingleton.ICollection(e.Target)
            resources = e.Contents

        Else
            Debug.Assert(TypeOf e.Target Is ResourceGroupMember)
            Debug.Assert(TypeOf e.Contents.First() Is ProcessGroupMember)
            resources = GetSingleton.ICollection(e.Target)
            processes = e.Contents

        End If

        For Each proc As IGroupMember In processes
            For Each res As IGroupMember In resources
                AddSession(proc.IdAsGuid, res.Name)
            Next
        Next
        RaiseScheduleChangeEvent()

    End Sub

    ''' <summary>
    ''' Deletes sessions from the listview when the context menu action is clicked
    ''' </summary>
    Private Sub HandleDeleteSessionClicked(ByVal sender As Object, ByVal e As EventArgs) _
     Handles DeleteToolStripMenuItem.Click

        If listSessions.SelectedItems.
            Cast(Of ListViewItem).
            Select(Function(x) x.Tag).
            OfType(Of ScheduledSession).
            Any(Function(x) Not (x.CanCurrentUserSeeProcess AndAlso x.CanCurrentUserSeeResource)) Then

            If Not mUserMessage.ShowYesNo(My.Resources.AutomateUI_Controls.ctlTask_HandleDeleteSessionClicked_WarningMessage) Then
                Return
            End If

        End If

        For Each item As ListViewItem In listSessions.SelectedItems
            mTask.RemoveSession(TryCast(item.Tag, ScheduledSession))
            item.Remove()
        Next
        RaiseScheduleChangeEvent()
    End Sub

    ''' <summary>
    ''' Shows the configuration form for startup parameters.
    ''' </summary>
    Private Sub HandleStartParamsClick(
     ByVal sender As Object, ByVal e As EventArgs) Handles StartupParametersToolStripMenuItem.Click

        Dim canCreate As Boolean = User.Current.HasPermission(Permission.Scheduler.CreateSchedule)
        Dim canEdit As Boolean = User.Current.HasPermission(Permission.Scheduler.EditSchedule)

        Using f As New frmStartParams()
            f.Sessions = mTask.Sessions
            f.btnStart.Visible = False
            f.SetEnvironmentColours(mParent)
            f.ReadOnly = Not (canEdit OrElse canCreate)
            f.ShowInTaskbar = False
            f.ShowDialog()

            ' Assume it's changed, unless it was read only
            If Not f.ReadOnly And
                f.DialogResult = DialogResult.OK Then
                RaiseScheduleChangeEvent()
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Handles the 'Fail Fast' ie. fail all sessions and the task if any session
    ''' fails checkbox.
    ''' </summary>
    Private Sub HandleFailFastChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles cbFailFast.CheckedChanged
        If Not Me.ReadOnly AndAlso mTask IsNot Nothing AndAlso mTask.FailFastOnError <> cbFailFast.Checked Then
            mTask.FailFastOnError = cbFailFast.Checked
            RaiseScheduleChangeEvent()
        End If
    End Sub

    Private Sub PostCompletionDelayNumericInput_ValueChanged(sender As Object, e As EventArgs) Handles PostCompletionDelayNumericInput.ValueChanged
        If Not Me.ReadOnly AndAlso mTask IsNot Nothing Then
            Dim valueAsInt = Convert.ToInt32(PostCompletionDelayNumericInput.Value)

            mTask.DelayAfterEnd = valueAsInt
            RaiseScheduleChangeEvent()
        End If
    End Sub

#End Region

End Class
