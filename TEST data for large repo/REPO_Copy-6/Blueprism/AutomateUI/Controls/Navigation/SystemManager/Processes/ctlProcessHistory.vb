Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports AutomateControls
Imports BluePrism.AutomateAppCore.Groups

Public Class ctlProcessHistory : Implements IMode, IPermission, IStubbornChild, IRefreshable

#Region " Member Variables "

    ' The mode set in this control
    Private mMode As ProcessType

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the process type 'mode' for this control
    ''' </summary>
    Public Property Mode() As ProcessType Implements IMode.Mode
        Get
            Return mMode
        End Get
        Set(ByVal value As ProcessType)
            If value <> mMode Then
                mMode = value
                mProcessHistoryList.Mode = value
                ChangeLabelText()
                UpdateProcessList()
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets the permissions required to use this control. This is dependent on the
    ''' mode of the control - if the mode is not set, this will throw a
    ''' <see cref="BluePrism.Server.Domain.Models.NoSuchPermissionException"/>
    ''' </summary>
    Public ReadOnly Property RequiredPermissions As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(mMode.GetPermissionString("History"))
        End Get
    End Property

    Public Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
   
#End Region

#Region " Methods "

    ''' <summary>
    ''' Changes the action label text to match the mode context of the view.
    ''' </summary>
    Private Sub ChangeLabelText()
        llCompare.Text = If(Mode = ProcessType.BusinessObject,
                                        My.Resources.frmProcess_CompareSelectedBusinessObject,
                                         My.Resources.frmProcess_CompareSelectedProcess)

        llHistory.Text = If(Mode = ProcessType.BusinessObject,
                                         My.Resources.frmProcess_ViewSelectedBusinessObject,
                                         My.Resources.frmProcess_ViewSelectedProcess)
    End Sub

    ''' <summary>
    ''' Populates the process combobox with available processes
    ''' </summary>
    Private Sub UpdateProcessList()
        Cursor = Cursors.WaitCursor

        Try
            cmbProcessList.TreeType = If(Mode = ProcessType.BusinessObject,
                                         GroupTreeType.Objects,
                                         GroupTreeType.Processes)
            cmbProcessList.Filter = ProcessBackedGroupMember.NotRetired
        Catch ex As Exception
            UserMessage.Err(ex,
             My.Resources.ctlProcessHistory_AnErrorOccurredWhileLoadingTheProcessesObjects0, ex.Message)
        End Try

        If cmbProcessList.Items.Count = 0 Then
            Dim noItemsText = If(Mode = ProcessType.BusinessObject, My.Resources.ctlProcessHistory_NoObjectsToView, My.Resources.ctlProcessHistory_NoProcessesToView)
            cmbProcessList.Items.Add(New ComboBoxItem(noItemsText))
            mProcessHistoryList.Enabled = False
            llHistory.Enabled = False
            llCompare.Enabled = False
        Else
            mProcessHistoryList.Enabled = True
            llHistory.Enabled = True
            llCompare.Enabled = True
        End If
        mProcessHistoryList.ProcessId = Guid.Empty
        mProcessHistoryList.ClearItems()

        Cursor = Cursors.Default
    End Sub

    ''' <summary>
    ''' Handles the 'Compare' link being clicked
    ''' </summary>
    Private Sub HandleCompareLinkClicked(
     sender As Object, e As LinkLabelLinkClickedEventArgs) Handles llCompare.LinkClicked
        mProcessHistoryList.CompareSelectedHistoryEntries()
    End Sub

    ''' <summary>
    ''' Handles the 'View' link being clicked
    ''' </summary>
    Private Sub HandleViewLinkClicked(
     sender As Object, e As LinkLabelLinkClickedEventArgs) Handles llHistory.LinkClicked
        mProcessHistoryList.ViewSelectedHistoryEntry()
    End Sub

    ''' <summary>
    ''' Handles the process selection changing in the process combo box
    ''' </summary>
    Private Sub HandleSelectedProcessChanged(
     sender As Object, e As EventArgs) Handles cmbProcessList.SelectedIndexChanged
        ' Get the selected item - if none is selected abort now.
        Dim proc = TryCast(cmbProcessList.SelectedMember, ProcessBackedGroupMember)
        If proc IsNot Nothing Then mProcessHistoryList.ProcessId = proc.IdAsGuid

        Dim canView = gSv.GetEffectiveMemberPermissionsForProcess(proc.IdAsGuid).HasPermission(
                        User.Current, If(Mode = ProcessType.Process,
                            Permission.ProcessStudio.ImpliedViewProcess,
                            Permission.ObjectStudio.ImpliedViewBusinessObject))
        mProcessHistoryList.CanViewDefinition = canView
        llCompare.Enabled = canView
        llHistory.Enabled = canView
    End Sub

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        Return True
    End Function 
    Public Sub RefreshView() Implements IRefreshable.RefreshView
        cmbProcessList.RefreshFromStore()
        mProcessHistoryList.RefreshView()
    End Sub


#End Region

End Class
