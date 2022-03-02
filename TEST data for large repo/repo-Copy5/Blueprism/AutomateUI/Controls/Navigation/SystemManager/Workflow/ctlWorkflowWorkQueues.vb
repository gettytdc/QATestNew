Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateAppCore.clsServer
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib
Imports LocaleTools
Imports AutomateControls.Forms
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Control to configure work queues in System Manager
''' </summary>
Public Class ctlWorkflowWorkQueues : Implements IHelp, IStubbornChild, IPermission, IRefreshable

#Region " Member Variables "

    ' The retrieved group tree
    Private mGroupTree As IGroupTree

    ' The not parent
    Private mParent As frmApplication

    ' The current selected queue
    Private mCurrentQueue As clsWorkQueue

    Private mLastSelectedDefaultScheme As Integer

    Private mFIPSCompliantOptionsAvailable As Boolean

    Private mLoading As Boolean

    Private mSavePromptTriggered As Boolean

#End Region

#Region " Constructor "

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add a filter to only show active published processes
        cmbProcess.Filter = ProcessGroupMember.PublishedAndNotRetiredFilter
        cmbResourceGroup.GroupFilter = ResourceGroupMember.ControllableResourceGroup

        ' Add any initialization after the InitializeComponent() call.
        ' Use the defined constants, so that if they change, the labels are correct
        lblKeyNameHint.Text = String.Format(
         My.Resources.ctlWorkflowWorkQueues_TakenFromAProcessStudioCollectionFieldMax0Chars,
         clsWorkQueue.MaxLengths.KeyField)

        lblQueueNameHint.Text = String.Format(
         My.Resources.ctlWorkflowWorkQueues_MaximumOf0Characters, clsWorkQueue.MaxLengths.Name)

        txtQueueName.MaxLength = clsWorkQueue.MaxLengths.Name
        txtKeyName.MaxLength = clsWorkQueue.MaxLengths.KeyField
        ' Default the queue status to not running, which is what is set in the designer
        SelectedQueueRunningStatus = False
        tvQueueGroups.EnableDoubleClick = False
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the running status of the selected queue within the context of
    ''' the UI only (ie. it gets its value from the UI elements and sets the UI
    ''' elements - it makes no attempt to update the model behind the UI).
    ''' </summary>
    Private Property SelectedQueueRunningStatus() As Boolean
        Get
            If tvQueueGroups.SelectedMember Is Nothing Then Return False
            Return (txtQueueStatus.Tag IsNot Nothing AndAlso CBool(txtQueueStatus.Tag))
        End Get
        Set(ByVal value As Boolean)
            txtQueueStatus.Tag = value
            txtQueueStatus.Text = If(value, My.Resources.ctlWorkflowWorkQueues_Running, My.Resources.ctlWorkflowWorkQueues_Paused)
            lnkToggleQueueStatus.Text = If(value, My.Resources.ctlWorkflowWorkQueues_PauseQueue, My.Resources.ctlWorkflowWorkQueues_ResumeQueue)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the selected queue's encryption id, adding it to the list if
    ''' it is not already there (highlighted to indicate that the server is unaware
    ''' of the encryption key with that name).
    ''' Null indicates 'not encrypted' (assuming a queue is selected). On setting
    ''' with null, the 'encrypted' checkbox is unchecked and the encryption name
    ''' combo box is disabled.
    ''' </summary>
    ''' <remarks>This has no effect if there is no queue selected.</remarks>
    Private Property SelectedQueueEncryptionID() As Integer
        Get
            If tvQueueGroups.SelectedMember Is Nothing Then Return Nothing
            If Not cbEncrypted.Checked Then Return Nothing
            Dim item As ComboBoxItem = DirectCast(cmbQueueKey.SelectedItem, ComboBoxItem)
            If item Is Nothing Then Return Nothing Else Return CInt(item.Tag)
        End Get
        Set(ByVal value As Integer)
            If tvQueueGroups.SelectedMember Is Nothing Then Return

            ' Check / Uncheck the encryption checkbox as appropriate
            cbEncrypted.Checked = (value > 0)

            ' If there's nothing more to do, do nothing more.
            If value = 0 Then Return

            ' Otherwise, find the item and select it, or create a new item and
            ' select it, as appropriate.
            Dim item As ComboBoxItem =
             cmbQueueKey.FindComboBoxItemByTag(value)

            ' If it's not there, it implies that it's been removed from the
            ' server / not configured in this server. Highlight it in red
            If item Is Nothing Then
                item = New ComboBoxItem(My.Resources.ctlWorkflowWorkQueues_Missing, Color.Red)
                cmbQueueKey.Items.Add(item)
            End If
            cmbQueueKey.SelectedItem = item
            mLastSelectedDefaultScheme = value
        End Set
    End Property

    ''' <summary>
    ''' The Application form which is ultimately hosting this control
    ''' </summary>
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property
    ''' <summary>
    ''' The permissions required to view this control
    ''' </summary>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("Workflow - Work Queue Configuration")
        End Get
    End Property

#End Region

#Region " Event Handler Methods "

    ''' <summary>
    ''' Instruct treeview control to refresh
    ''' </summary>
    Private Sub RefreshViewHandle() Handles tvQueueGroups.RefreshView
        tvQueueGroups.ClearGroupCache()
        tvQueueGroups.UpdateView(True)
        cmbResourceGroup.RefreshFromStore()
    End Sub

    ''' <summary>
    ''' Handles the control being created, populating the controls from the database
    ''' </summary>
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        mLoading = True

        ' Don't init if we're running in the designer
        If DesignMode Then Return

        'Hide encryption option if NHS license (no access to credentials)
        If Not License.CanUse(LicenseUse.Credentials) Then
            cbEncrypted.Visible = False
            lblEncryptedUsing.Visible = False
            cmbQueueKey.Visible = False
        Else
            PopulateEncryptionSchemes()

            ' Default the encryption choice to disable (since the checkbox is unchecked)
            lblEncryptedUsing.Enabled = False
            cmbQueueKey.Enabled = False

            ' And disable the encryption checkbox if there are no encrypter names.
            If cmbQueueKey.Items.Count = 0 Then cbEncrypted.Enabled = False
        End If

        PopulateQueueList()
        CreateDefaultQueueIfNotExists()

        mLoading = False
    End Sub

    Private Sub CreateDefaultQueueIfNotExists()
        If Not mGroupTree.Root.Any() Then CreateNewQueue()
    End Sub

    Private Sub PopulateEncryptionSchemes()
        If Not License.CanUse(LicenseUse.Credentials) Then Return

        Dim oldIndex = cmbQueueKey.SelectedIndex

        cmbQueueKey.Items.Clear()
        mFIPSCompliantOptionsAvailable = False

        ' Load the schemes combo box with new data
        For Each encScheme As clsEncryptionScheme In gSv.GetEncryptionSchemesExcludingKey()

            If Not encScheme.IsAvailable Then Continue For

            Dim displayedText = If(encScheme.Name = clsEncryptionScheme.DefaultEncryptionSchemeName,
                encScheme.Name,
                LTools.Get(encScheme.Name, "misc", Options.Instance.CurrentLocale, "crypto"))

            Dim comboItem As ComboBoxItem
            If encScheme.FIPSCompliant Then
                comboItem = New ComboBoxItem(displayedText, encScheme.ID)
                mFIPSCompliantOptionsAvailable = True
            Else
                displayedText += My.Resources.NotFIPSCompliant
                comboItem = New ComboBoxItem(displayedText, encScheme.ID) With
                {
                    .Selectable = False,
                    .Enabled = False
                }
            End If
            cmbQueueKey.Items.Add(comboItem)
        Next

        If oldIndex <> -1 Then cmbQueueKey.SelectedIndex = oldIndex
    End Sub

    ''' <summary>
    ''' Checks if this control can be left in its current state or not
    ''' </summary>
    ''' <returns>True if the control can safely be left; False otherwise.</returns>
    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        If Not mSavePromptTriggered Then
            If User.LoggedIn Then
                Dim nonCompliantSchemesSelected = False
                Dim schemes = gSv.GetEncryptionSchemesExcludingKey()
                Dim queues = gSv.WorkQueueGetAllQueues()
                For Each queue As clsWorkQueue In queues
                    If queue.IsEncrypted Then
                        Dim schemeName = schemes.Where(Function(x) x.ID = queue.EncryptionKeyID).First.Name
                        If Not gSv.CheckSchemeForFIPSCompliance(schemeName) Then
                            nonCompliantSchemesSelected = True
                        End If
                    End If
                Next
                If nonCompliantSchemesSelected Then
                    Dim popup = New PopupForm(My.Resources.FIPSPolicyEnabled, My.Resources.WorkQueues_QueueUsesNonFIPSEncryptSch, My.Resources.btnOk)
                    AddHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
                    popup.ShowDialog()
                End If
            End If

            ' Get the currently displayed queue locally
            Dim q As clsWorkQueue = mCurrentQueue

            ' If we don't have a 'current' queue to test, there's no point in continuing
            If q Is Nothing Then Return True

            ' Get the currently selected queue group member. If it's not a queue group
            ' member then we're not interested.
            Dim selected = TryCast(tvQueueGroups.SelectedMember, QueueGroupMember)
            If selected Is Nothing Then Return True

            ' If it doesn't match the queue we think we're dealing with, there's not a
            ' lot we can do about it
            If q.Ident <> selected.IdAsInteger Then Return True

            Dim changes As New List(Of String)

            ' Check if anything has changed (note that we don't need to check the running
            ' status since that is applied on the database immediately)
            If q.Name <> txtQueueName.Text Then changes.Add(My.Resources.ctlWorkflowWorkQueues_Name)
            If q.KeyField <> txtKeyName.Text Then changes.Add(My.Resources.ctlWorkflowWorkQueues_KeyField)
            If q.MaxAttempts <> CInt(numMaxAttempts.Value) _
             Then changes.Add(My.Resources.ctlWorkflowWorkQueues_MaxAttempts)
            If q.EncryptionKeyID <> SelectedQueueEncryptionID _
             Then changes.Add(My.Resources.ctlWorkflowWorkQueues_EncryptionKey)
            If q.IsActive <> cbActiveQueue.Checked _
             Then changes.Add(My.Resources.ctlWorkflowWorkQueues_WhetherQueueIsActive)
            If q.ProcessId <> cmbProcess.SelectedIdAsGuid _
             Then changes.Add(My.Resources.ctlWorkflowWorkQueues_AssignedProcess)
            If q.ResourceGroupId <> cmbResourceGroup.SelectedIdAsGuid _
             Then changes.Add(My.Resources.ctlWorkflowWorkQueues_AssignedResourceGroup)
            If q.SessionExceptionRetry <> cbRetryQueueException.Checked _
             Then changes.Add(My.Resources.ctlWorkflowWorkQueues_RetryTerminatedItems)

            ' Nothing changed? Carry on
            If changes.Count = 0 Then Return True

            ' Put together a message indicating the changes we need
            Dim sb As New StringBuilder(100)
            sb.Append(String.Format(My.Resources.ctlWorkflowWorkQueues_YouHaveNotYetSavedYourChangesToTheQueue0, txtQueueName.Text)).
                Append(vbCrLf)
            sb.Append(My.Resources.ctlWorkflowWorkQueues_TheFollowingPropertiesHaveBeenChanged)
            For i As Integer = 0 To changes.Count - 1
                If i > 0 Then sb.Append(My.Resources.ctlWorkflowWorkQueues_JoinStringsComma)
                sb.Append(changes(i))
            Next
            sb.Append(vbCrLf)
            sb.Append(My.Resources.ctlWorkflowWorkQueues_WouldYouLikeToSaveYourChangesBeforeLeaving)

            ' See what the user wants to do:
            ' Yes   : Save Changes; yes, this can leave
            ' No    : Don't save changes; yes, this can leave
            ' Cancel: Don't save changes; no, this can't leave
            Dim form = New YesNoCancelPopupForm(My.Resources.ctlWorkflowWorkQueues_UnsavedChanges, sb.ToString(), String.Empty)

            Select Case form.ShowDialog()
                Case DialogResult.Cancel
                    Return False
                Case DialogResult.Yes
                    AttemptToSaveChanges()
                Case DialogResult.No
                    mSavePromptTriggered = True
                    Return True
            End Select
        End If
        Return True
    End Function

    ''' <summary>
    ''' Handles the currently selected group member being deselected. This checks if
    ''' the queue has changed and, if so, if the user wants to save changes
    ''' </summary>
    Private Sub HandleGroupMemberDeselecting(sender As Object, e As CancelEventArgs) _
     Handles tvQueueGroups.MemberDeselecting

        Dim hiddenProcessId = Guid.Empty
        Dim hiddenResourceGroupId = Guid.Empty

        If mCurrentQueue IsNot Nothing Then
            If mCurrentQueue.IsAssignedProcessHidden Then
                hiddenProcessId = mCurrentQueue.ProcessId
            End If

            If mCurrentQueue.IsResourceGroupHidden Then
                hiddenResourceGroupId = mCurrentQueue.ResourceGroupId
            End If
        End If

        e.Cancel = Not CanLeave()

        If Not e.Cancel Then
            RemoveItemFromProcessComboBox(hiddenProcessId)
            RemoveItemFromResourceGroupComboBox(hiddenResourceGroupId)
        End If
    End Sub

    ''' <summary>
    ''' Removes the combobox item from the process combobox which matches the process id
    ''' </summary>
    ''' <param name="processid"></param>
    Private Sub RemoveItemFromProcessComboBox(processid As Guid)
        If processid = Guid.Empty Then Return
        Dim restrictedProcessIndex = -1
        For Each item In cmbProcess.Items
            Dim comboBoxItem = CType(item, ComboBoxItem)
            If comboBoxItem.Tag.GetType() Is GetType(ProcessGroupMember) AndAlso CType(comboBoxItem.Tag, ProcessGroupMember).IdAsGuid() = processid Then
                restrictedProcessIndex = cmbProcess.Items.IndexOf(item)
            End If
        Next
        cmbProcess.Items.RemoveAt(restrictedProcessIndex)
    End Sub

    ''' <summary>
    ''' Removes the combobox item from the resource group combobox which matches the resource group id
    ''' </summary>
    ''' <param name="groupid"></param>
    Private Sub RemoveItemFromResourceGroupComboBox(groupid As Guid)
        If groupid = Guid.Empty Then Return
        Dim restrictedResourceGroupIndex = -1
        For Each item In cmbResourceGroup.Items
            Dim comboBoxItem = CType(item, ComboBoxItem)
            If CType(comboBoxItem.Tag, IGroup).IdAsGuid() = groupid Then
                restrictedResourceGroupIndex = cmbResourceGroup.Items.IndexOf(item)
            End If
        Next
        cmbResourceGroup.Items.RemoveAt(restrictedResourceGroupIndex)
    End Sub

    Private Sub AttemptToSaveChanges()
        Dim errorMessage As String = Nothing

        errorMessage = ValidateAssignedProcess()

        If errorMessage Is Nothing Then
            SaveCurrentQueue()
        Else
            UserMessage.Err(errorMessage)
        End If
    End Sub

    '' <summary>
    '' Handles the selection changing in the list of queues.
    '' </summary>
    Private Sub tvQueueGroups_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) _
      Handles tvQueueGroups.ItemSelected, tvQueueGroups.GroupSelected

        Try
            mLoading = True
            Dim selected = TryCast(tvQueueGroups.SelectedMember, QueueGroupMember)
            Dim enabled As Boolean = selected IsNot Nothing
            SetQueueDetailsEnabled(enabled)
            btnDeleteQueue.Enabled = enabled
            ' Clear the current queue for now - it is reinstated later if a queue
            ' is successfully loaded from the database

            mCurrentQueue = Nothing
            If Not enabled Then Return

            Dim q As clsWorkQueue = gSv.WorkQueueGetQueue(selected.IdAsInteger)
            If q IsNot Nothing Then
                txtQueueName.Text = q.Name
                txtKeyName.Text = q.KeyField
                numMaxAttempts.Value = q.MaxAttempts
                SelectedQueueRunningStatus = q.IsRunning
                SelectedQueueEncryptionID = q.EncryptionKeyID
                cbRetryQueueException.Checked = q.SessionExceptionRetry
                If Not q.IsEncrypted AndAlso cmbQueueKey.Items.Count > 0 Then _
                 cmbQueueKey.SelectedIndex = 0
                cbActiveQueue.Checked = q.IsActive


                ' If queue has a process / resource group assigned that this user can't see, then add it to the combobox
                If q.IsAssignedProcessHidden Then
                    Dim comboBoxItem = New ComboBoxItem(String.Format(My.Resources.restricted_warn, q.ProcessName)) With {.Checkable = True, .Checked = True}
                    comboBoxItem.Tag = New ProcessGroupMember() With {.Id = q.ProcessId, .Name = q.ProcessName}
                    cmbProcess.Items.Add(comboBoxItem)
                End If
                cmbProcess.SelectedIdAsGuid = q.ProcessId

                If q.IsResourceGroupHidden Then
                    Dim comboBoxItem = New ComboBoxItem(String.Format(My.Resources.restricted_warn, q.ResourceGroupName)) With {.Checkable = True, .Checked = True}
                    comboBoxItem.Tag = New Group() With {.Id = q.ResourceGroupId, .Name = q.ResourceGroupName}
                    cmbResourceGroup.Items.Add(comboBoxItem)
                End If
                cmbResourceGroup.SelectedIdAsGuid = q.ResourceGroupId
                If gpActiveQueues.Enabled Then
                    For Each item In cmbResourceGroup.Items
                        Dim comboBoxItem = CType(item, ComboBoxItem)
                        If comboBoxItem.Text.Equals("Default") Then
                            comboBoxItem.Text = My.Resources.GroupMemberComboBox_AddEntry_Default
                        End If
                    Next
                End If
            Else
                If tvQueueGroups.IsFiltered Then
                    tvQueueGroups.ClearTreeFilter()
                Else
                    UserMessage.Show(
                 String.Format(My.Resources.ctlWorkflowWorkQueues_UnexpectedErrorCouldNotIdentifyQueueWithName0, selected.Name))
                End If
            End If

            mCurrentQueue = q
            IsQueueSnapshotConfigured()

        Catch ex As Exception
            UserMessage.Show(My.Resources.ctlWorkflowWorkQueues_FailedToRetrieveDetailsOfQueue, ex)
        Finally
            mLoading = False
        End Try
    End Sub

    ''' <summary>
    ''' Handles the 'Resume Queue' link being clicked.
    ''' </summary>
    Private Sub lnkToggleQueueStatus_LinkClicked(
     ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) _
     Handles lnkToggleQueueStatus.LinkClicked
        Try
            Dim item As QueueGroupMember = TryCast(tvQueueGroups.SelectedMember, QueueGroupMember)
            If item IsNot Nothing Then
                item.Running = gSv.ToggleQueueRunningStatus(item.QueueGuid)
                SelectedQueueRunningStatus = item.Running
            End If
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkflowWorkQueues_ErrorWhileTogglingQueueStatus0, ex.Message), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Handles the 'Apply' button being clicked.
    ''' </summary>
    Private Sub btnApply_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnApply.Click
        AttemptToSaveChanges()
    End Sub

    ''' <summary>
    ''' Handles the 'Delete Queue' button being clicked.
    ''' </summary>
    Private Sub btnDeleteQueue_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnDeleteQueue.Click
        Try
            Dim m As IGroupMember = tvQueueGroups.SelectedMember
            Dim originalOwner = m.Owner
            If m Is Nothing Then _
             UserMessage.Show(My.Resources.ctlWorkflowWorkQueues_YouMustSelectAQueueToDelete) : Return

            If m.MemberType = GroupMemberType.Queue Then
                If (Not mCurrentQueue.IsSnapshotConfigured AndAlso UserMessage.YesNo(My.Resources.ctlWorkflowWorkQueues_WarningDeleteQueue) = MsgBoxResult.Yes) OrElse
                   (mCurrentQueue.IsSnapshotConfigured AndAlso
                    UserMessage.YesNo(My.Resources.ctlWorkflowWorkQueues_WarningDeletingSnapshotData) = MsgBoxResult.Yes) Then
                    Try
                        gSv.DeleteWorkQueue(m.Name, mCurrentQueue.IsSnapshotConfigured)
                        m.Delete()
                    Catch ex As Exception
                        UserMessage.Err(ex, My.Resources.ctlWorkflowWorkQueues_FailedToDeleteQueue0, ex.Message)
                    End Try
                End If
            Else
                If m.IsGroup Then DirectCast(m, IGroup).Delete()
            End If

            tvQueueGroups.FlushGroupFromCache(originalOwner)
            tvQueueGroups.UpdateView()

        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.ctlWorkflowWorkQueues_FailedToDeleteQueue0, ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Handles the 'New Queue' button being clicked.
    ''' </summary>
    Private Sub btnNewQueue_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnNewQueue.Click
        Try
            CreateNewQueue()
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkflowWorkQueues_UnexpectedError0, ex.Message), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Handles the 'Encrypted' checkbox's state changing
    ''' </summary>
    Private Sub cbEncrypted_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles cbEncrypted.CheckedChanged
        Dim enc As Boolean = cbEncrypted.Checked
        lblEncryptedUsing.Enabled = enc
        cmbQueueKey.Enabled = enc
        CheckEncryptWorkQueue()
    End Sub

    ''' <summary>
    ''' Handles the 'Active Queue' checkbox's state changing
    ''' </summary>
    Private Sub HandleActiveCheckedChanged(sender As Object, e As EventArgs) _
     Handles cbActiveQueue.CheckedChanged
        gpActiveQueues.Enabled = cbActiveQueue.Checked
    End Sub

    ''' <summary>
    ''' Handles the 'Find References' linklabel being clicked
    ''' </summary>
    Private Sub lnkReferences_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) _
     Handles lnkReferences.LinkClicked
        mParent.FindReferences(New clsProcessQueueDependency(txtQueueName.Text))
    End Sub

    Private Sub CmbQueueKey_LostFocus() Handles cmbQueueKey.LostFocus
        If cmbQueueKey.SelectedItem?.ToString.Contains(My.Resources.NotFIPSCompliant) Then
            Dim comboBoxItem As ComboBoxItem = cmbQueueKey.FindComboBoxItemByTag(mLastSelectedDefaultScheme)
            If comboBoxItem Is Nothing Then
                Dim found = cmbQueueKey.Items.Cast(Of ComboBoxItem).FirstOrDefault(Function(x) Not x.Text.Contains(My.Resources.NotFIPSCompliant))
                If found IsNot Nothing Then
                    cmbQueueKey.SelectedItem = found
                End If
            Else
                cmbQueueKey.SelectedItem = comboBoxItem
            End If
        End If
    End Sub

    Private Sub CmbQueueKey_SelectedIndexChanged() Handles cmbQueueKey.SelectedIndexChanged
        If cmbQueueKey.SelectedItem IsNot Nothing AndAlso
         cmbQueueKey.SelectedItem.ToString.Contains(My.Resources.NotFIPSCompliant) Then _
            mLastSelectedDefaultScheme = cmbQueueKey.SelectedIndex
    End Sub

    Private Sub CheckEncryptWorkQueue()
        If mLoading OrElse Not cbEncrypted.Checked Then Return
        If Not mFIPSCompliantOptionsAvailable Then
            cbEncrypted.Checked = False
            Dim popup = New PopupForm(My.Resources.FIPSPolicyEnabled, My.Resources.ctlWorkflowWorkQueue_NoAvailableFIPSSchemes, My.Resources.btnOk)
            AddHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
            popup.ShowDialog()
        End If
    End Sub

    Private Sub HandleOnBtnOKClick(sender As Object, e As EventArgs)
        Dim popup = CType(sender, PopupForm)
        RemoveHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
        popup.Close()
    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Populates the list of queues with the latest data from the database.
    ''' </summary>
    Private Sub PopulateQueueList()
        Try

            mGroupTree = GetGroupStore().GetTree(GroupTreeType.Queues, Nothing, Nothing, True, False, False)
            tvQueueGroups.Clear()

            tvQueueGroups.AddTree(mGroupTree, True)

        Catch ex As Exception
            UserMessage.Show(My.Resources.ctlWorkflowWorkQueues_CouldNotPopulateQueueList, ex)
        End Try

    End Sub

    ''' <summary>
    ''' Gets the name of the control help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "sysman-queues.html"
    End Function

    ''' <summary>
    ''' Enables/disables the right hand view, showing the queue details
    ''' </summary>
    Private Sub SetQueueDetailsEnabled(ByVal en As Boolean)
        panQueueDetail.Enabled = en

        If Not en Then
            txtQueueName.Clear()
            txtKeyName.Clear()
            numMaxAttempts.Value = numMaxAttempts.Minimum
            txtQueueStatus.Clear()
        End If
    End Sub

    Private Function ValidateAssignedProcess() As String
        Dim selectedProcess As ComboBoxItem
        Dim message As String = Nothing

        If cbEncrypted.Checked AndAlso cmbQueueKey.SelectedItem?.ToString.Contains(My.Resources.NotFIPSCompliant) Then
            message = My.Resources.ctlWorkflowWorkQueues_PleaseSelectAFIPSCompliantEncryptionScheme
        End If

        If cbActiveQueue.Checked Then
            selectedProcess = CType(cmbProcess.SelectedItem, ComboBoxItem)

            If Not selectedProcess?.Selectable Then
                message = My.Resources.ctlWorkflowWorkQueues_InvalidAssignedProcessSelectedYouMustSelectAProcessNotAGroup
            End If
        End If

        Return message
    End Function

    ''' <summary>
    ''' Get the currently displayed queue locally and checks if snapshot configured
    ''' </summary>
    ''' <returns></returns>
    Private Function IsQueueSnapshotConfigured() As Boolean
        Dim q As clsWorkQueue = mCurrentQueue
        If q IsNot Nothing Then
            txtQueueName.Text = q.Name
            If q.IsSnapshotConfigured Then
                flpConfigQueueInfo.Visible = True
            Else
                flpConfigQueueInfo.Visible = False
            End If
        End If
    End Function

    ''' <summary>
    ''' Saves the currently selected queue to the database, outputting any errors
    ''' in doing so to the user
    ''' </summary>
    Private Function SaveCurrentQueue() As Boolean
        Dim mem = TryCast(tvQueueGroups.SelectedMember, QueueGroupMember)
        If mem Is Nothing Then Return UserMessage.Err(
            My.Resources.ctlWorkflowWorkQueues_FailedToResolveIDOfCurrentlySelectedItem)

        Try

            If (mCurrentQueue.IsAssignedProcessHidden AndAlso cmbProcess.SelectedIdAsGuid <> mCurrentQueue.ProcessId) OrElse
            (mCurrentQueue.IsResourceGroupHidden AndAlso cmbResourceGroup.SelectedIdAsGuid <> mCurrentQueue.ResourceGroupId) Then

                If UserMessage.OkCancel(My.Resources.queue_overwrite_hidden) = MsgBoxResult.Cancel Then
                    Return False
                End If
            End If

            Dim wq As New clsWorkQueue() With {
                .Id = mem.QueueGuid,
                .Name = txtQueueName.Text,
                .KeyField = txtKeyName.Text,
                .MaxAttempts = CInt(numMaxAttempts.Value),
                .EncryptionKeyID = SelectedQueueEncryptionID,
                .GroupStore = GetGroupStore(),
                .SessionExceptionRetry = cbRetryQueueException.Checked
            }
            If cbActiveQueue.Checked Then
                If cmbProcess.SelectedMember Is Nothing Then
                    Return UserMessage.Err(
                        My.Resources.ctlWorkflowWorkQueues_YouMustSelectTheProcessThatAnActiveQueueWillExecuteToWorkItself)
                End If
                wq.ProcessId = cmbProcess.SelectedMember.IdAsGuid

                If cmbResourceGroup.SelectedGroup Is Nothing Then
                    Return UserMessage.Err(
                        My.Resources.ctlWorkflowWorkQueues_YouMustSelectTheResourceGroupThatAnActiveQueueWillUseToRunItsSessions)
                End If

                wq.ResourceGroupId = cmbResourceGroup.SelectedGroup.IdAsGuid


                ' If queue has a process the user could not view assigned, and it is no longer selected, remove the restricted process name from the combobox.
                Dim isRestrictedProcessSelected = mCurrentQueue.IsAssignedProcessHidden AndAlso cmbProcess.SelectedIdAsGuid = mCurrentQueue.ProcessId
                If Not isRestrictedProcessSelected AndAlso mCurrentQueue.IsAssignedProcessHidden Then
                    RemoveItemFromProcessComboBox(mCurrentQueue.ProcessId)
                End If

                ' If queue has a resource group the user could not view assigned, and it is no longer selected, remove the resource group from the combobox.
                Dim isRestrictedResourceGroupSelected = mCurrentQueue.IsResourceGroupHidden AndAlso cmbResourceGroup.SelectedIdAsGuid = mCurrentQueue.ResourceGroupId
                If Not isRestrictedResourceGroupSelected AndAlso mCurrentQueue.IsResourceGroupHidden Then
                    RemoveItemFromResourceGroupComboBox(mCurrentQueue.ResourceGroupId)
                End If
            End If

            wq = gSv.UpdateWorkQueue(wq)

            ' Now update the group member so that it reflects the changes made to
            ' the queue data itself
            mem.Name = wq.Name
            mem.Running = wq.IsRunning
            mem.EncryptKeyID = wq.EncryptionKeyID
            mem.IsActive = wq.IsActive

            ' Update the current queue and tell the treeview to update itself
            mCurrentQueue = wq
            tvQueueGroups.FlushGroupFromCache(mem.Owner)
            tvQueueGroups.UpdateView()
            Return True

        Catch qnee As QueueNotEmptyException
            UserMessage.Err(qnee,
             My.Resources.ctlWorkflowWorkQueues_FailedToUpdateTheQueueTheEncryptionConfigurationCannotBeChangedIfTheQueueIsNotE)

            ' Reset the queue's encrypter name to its original value in the UI
            SelectedQueueEncryptionID = mem.EncryptKeyID

            ' Leave everything else the same so the user can resubmit the other
            ' changes (if there were any)

        Catch ex As Exception
            Return UserMessage.Err(ex,
                My.Resources.ctlWorkflowWorkQueues_FailedToUpdateQueueDetails0, ex.Message)

        End Try

        ' Only an error can get us here
        Return False

    End Function

    ''' <summary>
    ''' Creates a new queue, and refreshes the UI.
    ''' </summary>
    Private Sub CreateNewQueue()
        Try
            Dim selected As IGroup = tvQueueGroups.SelectedGroup
            If selected Is Nothing Then
                selected = mGroupTree.Root
            End If

            'Get a set of existing queue names
            Dim names As New clsSet(Of String)
            For Each q As clsWorkQueue In gSv.WorkQueueGetAllQueues()
                names.Add(q.Name)
            Next

            Dim name As String = Nothing
            For i As Integer = 1 To Integer.MaxValue
                Dim qName = String.Format(My.Resources.ctlWorkflowWorkQueues_Queue0, i.ToString())
                If Not names.Contains(qName) Then
                    name = qName
                    Exit For
                End If
            Next

            ' Really shouldn't happen in any sane world...
            If name Is Nothing Then Throw New OverflowException(
             My.Resources.ctlWorkflowWorkQueues_ApplicationRanOutOfValidNamesForTheNewQueue)

            ' Round trip the work queue object to ensure that the ID and Identity are set
            ' correctly in the object we are handling (they are set by the server)
            Dim wq As clsWorkQueue = gSv.CreateWorkQueue(New clsWorkQueue(Nothing, name, My.Resources.ctlWorkflowWorkQueues_Field1, 1, True, Nothing))
            wq.GroupStore = GetGroupStore()

            Dim mem As New QueueGroupMember(wq)
            selected.Add(mem)
            tvQueueGroups.FlushGroupFromCache(selected)
            tvQueueGroups.UpdateView()
            tvQueueGroups.SelectedMember = mem

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkflowWorkQueues_FailedToCreateQueue0, ex.Message), ex)
        End Try
    End Sub

    Public Sub RefreshView() Implements IRefreshable.RefreshView
        If CanLeave() Then
            RefreshViewHandle()
            cmbProcess.RefreshFromStore()
            PopulateEncryptionSchemes()
            tvQueueGroups_SelectedIndexChanged(Me, EventArgs.Empty)
        End If
    End Sub
#End Region

End Class
