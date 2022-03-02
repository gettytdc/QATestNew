Imports AutomateControls
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

''' <summary>
''' The control used to handle retirement for processes
''' </summary>
Public Class ProcessRetirementControl
    Implements IChild, IMode, IPermission, IHelp, IStubbornChild, IRefreshable

    ' The frmApplication which ultimately holds this control
    Private mAppForm As frmApplication

    ''' <summary>
    ''' Creates a new empty process retirement control
    ''' </summary>
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        tvActive.Filter = GroupMember.NotRetired
        tvRetired.Filter = GroupMember.Retired

    End Sub

    ''' <summary>
    ''' Sets the frmApplication in this control
    ''' </summary>
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mAppForm
        End Get
        Set(value As frmApplication)
            mAppForm = value
        End Set
    End Property

    ''' <summary>
    ''' Triggers post loading of primary tree.  Refresh the second trees data so inline with the first.
    ''' </summary>
    Private Sub TreeLoaded() Handles tvActive.TreeLoaded
        BeginInvoke(Sub()
                        tvRetired.RefreshLocalStore()
                    End Sub)
    End Sub



    ''' <summary>
    ''' Processes the dropping of some group members onto a target, either causing a
    ''' retirement or an unretirement of a process.
    ''' </summary>
    ''' <param name="contents">The group members which are being dragged</param>
    ''' <param name="target">The target onto which the members are being dropped.
    ''' </param>
    ''' <param name="retiring">True if the action should cause the retiring of the
    ''' contents; False if it should cause the 'unretiring' of the contents.</param>
    Private Sub ProcessDroppedContents(
     contents As ICollection(Of IGroupMember),
     target As IGroupMember,
     retiring As Boolean)

        ' If we're retiring the contents and any of them are retired, *or* we're
        ' unretiring the contents and any are not retired, exit now.
        If contents.Any(Function(mem) Not (retiring Xor mem.IsRetired)) Then Return

        ' Get the group to add it to
        Dim targetMem As IGroupMember = target
        Dim gp As IGroup = TryCast(targetMem, IGroup)
        If gp Is Nothing Then gp = targetMem.Owner
        If gp Is Nothing Then Return

        ' If trying to place into root node when the tree has a default group - disallow this.
        If Not retiring AndAlso gp.IsRoot AndAlso gp.Tree.HasDefaultGroup Then
            Return
        End If

        ' Go through all the dragged resources and retire / unretire them

        Try
            For Each mem As ProcessBackedGroupMember In contents
                If retiring Then mem.Retire() Else mem.UnRetire(gp)
            Next
        Catch ex As BluePrismException
            UserMessage.OK(ex.Message)
        End Try



    End Sub

    ''' <summary>
    ''' Handles a group member being dropped on the active treeview
    ''' </summary>
    Private Sub HandleGroupMemberDropOnActive(
     sender As Object, e As GroupMemberDropEventArgs) _
     Handles tvActive.GroupMemberDropped
        Try
            ProcessDroppedContents(e.Contents, e.Target, False)
        Catch ex As Exception
            UserMessage.Err(ex,
             My.Resources.ProcessRetirementControl_AnErrorOccurredWhileTryingToUnretireTheSelectedItems0,
             ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Handles a group member being dropped on the retired list
    ''' </summary>
    Private Sub HandleGroupMemberDropOnRetired(
     sender As Object, e As GroupMemberDropEventArgs) _
     Handles tvRetired.GroupMemberDropped
        Try
            ProcessDroppedContents(e.Contents, e.Target, True)
        Catch ex As Exception
            UserMessage.Err(ex,
             My.Resources.ProcessRetirementControl_AnErrorOccurredWhileTryingToRetireTheSelectedItems0,
             ex.Message)
        End Try
    End Sub


    ''' <summary>
    ''' Gets or sets the process type mode that this control operates on.
    ''' </summary>
    Public Property Mode As ProcessType Implements IMode.Mode
        Get
            Return If(tvActive.TreeType = GroupTreeType.Processes,
                      ProcessType.Process,
                      ProcessType.BusinessObject)
        End Get
        Set(value As ProcessType)


            Dim tp = If(value = ProcessType.Process,
                        GroupTreeType.Processes,
                        GroupTreeType.Objects)

            tvActive.GetRetired = True
            tvRetired.GetRetired = True
            tvActive.TreeType = tp
            tvRetired.TreeType = tp

            lblActive.Text = String.Format(My.Resources.ProcessRetirementControl_Active0, TreeDefinitionAttribute.GetLocalizedFriendlyName(tp.GetTreeDefinition().PluralName))
            lblRetired.Text = String.Format(My.Resources.ProcessRetirementControl_Retired0, TreeDefinitionAttribute.GetLocalizedFriendlyName(tp.GetTreeDefinition().PluralName))
        End Set
    End Property

    ''' <summary>
    ''' The required permissions for this control
    ''' </summary>
    Public ReadOnly Property RequiredPermissions As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(Mode.GetPermissionString("Management"))
        End Get
    End Property

    ''' <summary>
    ''' Gets the help page which corresponds to this control
    ''' </summary>
    ''' <returns>The name of the page which provides help for this control, given its
    ''' current <see cref="Mode"/></returns>
    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        If Mode = ProcessType.BusinessObject _
            Then Return "helpSystemManagerBusinessObjects.htm" _
            Else Return "HelpSystemManagerProcesses.htm"
    End Function

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        Return True
    End Function

    Public Sub RefreshView() Implements IRefreshable.RefreshView
        tvActive.UpdateView(True)
        tvRetired.RefreshLocalStore()
    End Sub

    Private Sub ActiveProcesses_DoubleClick(sender As Object, e As GroupMemberEventArgs) Handles tvActive.GroupItemActivated
        If Not TypeOf e.Target Is ProcessBackedGroupMember Then
            Return
        End If
        Try
            ProcessDroppedContents({e.Target}, tvRetired.Tree.Root, True)
            tvRetired.UpdateView(False)
            tvActive.UpdateView(False)
        Catch ex As Exception
            UserMessage.Err(ex,
             My.Resources.ProcessRetirementControl_AnErrorOccurredWhileTryingToRetireTheSelectedItems0,
             ex.Message)
        End Try
    End Sub
End Class
