Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.Images
Imports System.Reflection

''' <summary>
''' Group tree list view designed for processes/VBOs. This configures the subitems
''' to display the 'Description' of a process backed group member
''' </summary>
Public Class ProcessBackedMemberTreeListView : Inherits GroupTreeListView

    Private mShowDescription As Boolean = True
    Private mShowWSName As Boolean = False
    Private mShowDocumentLiteralFlag As Boolean = False
    Private mShowLegacyNamespaceFlag As Boolean = False
    Private mManagePermissions As Boolean = False
    Private mnuPermissions As ToolStripItem

    ''' <summary>
    ''' Creates a new tree list view for process-backed group members
    ''' </summary>
    Public Sub New()
        SetColumns()
        SetContextMenu()
    End Sub

    Protected Overrides Sub OnCreateControl()
        If Not DesignMode Then UpdateView(UpdateTreeFromStore)
    End Sub


    ''' <summary>
    ''' Setup columns depending on which ones are required.
    ''' </summary>
    Private Sub SetColumns()
        If SubitemDefinitions IsNot Nothing Then SubitemDefinitions = Nothing

        SubitemDefinitions = New Dictionary(Of String, PropertyInfo)
        If mShowWSName Then _
            SubitemDefinitions.Add(New KeyValuePair(Of String, PropertyInfo)(
                My.Resources.ProcessBackedMemberTreeListView_SetColumns_WebServiceName, GetType(ProcessBackedGroupMember).GetProperty("WebServiceName")))

        If mShowDocumentLiteralFlag Then _
            SubitemDefinitions.Add(New KeyValuePair(Of String, PropertyInfo)(
                My.Resources.ProcessBackedMemberTreeListView_SetColumns_BindingStyle, GetType(ProcessBackedGroupMember).GetProperty("WebServiceDocLitFormatText")))

        If mShowLegacyNamespaceFlag Then _
            SubitemDefinitions.Add(New KeyValuePair(Of String, PropertyInfo)(
                My.Resources.ProcessBackedMemberTreeListView_LegacyNamespacing, GetType(ProcessBackedGroupMember).GetProperty("WebServiceLegacyNamespaceFormatText")))

        If mShowDescription Then _
            SubitemDefinitions.Add(New KeyValuePair(Of String, PropertyInfo)(
                My.Resources.ProcessBackedMemberTreeListView_SetColumns_Description, GetType(ProcessBackedGroupMember).GetProperty("Description")))
    End Sub

    ''' <summary>
    ''' Setup the default context menu depending on which options are required.
    ''' </summary>
    Private Sub SetContextMenu()
        'Setup the default context menu
        Dim ctxMenu As New ContextMenuStrip()
        If mManagePermissions Then
            mnuPermissions = New ToolStripMenuItem(My.Resources.ProcessBackedMemberTreeListView_AccessRights, ComponentImages.Folder_Lock_16x16, Sub() HandleManageGroupPermissions())
            ctxMenu.Items.Add(mnuPermissions)
            ctxMenu.Items.Add(New ToolStripSeparator())
        End If
        ctxMenu.Items.Add(New ToolStripMenuItem(My.Resources.ProcessBackedMemberTreeListView_ExpandAll, ToolImages.Expand_All_16x16, Sub() ExpandAll(), "mnuExpand"))
        ctxMenu.Items.Add(New ToolStripMenuItem(My.Resources.ProcessBackedMemberTreeListView_CollapseAll, ToolImages.Collapse_All_16x16, Sub() CollapseAll(), "mnuCollapse"))
        AddHandler ctxMenu.Opening, AddressOf HandleMenuOpening
        ContextMenuStrip = ctxMenu
    End Sub

    ''' <summary>
    ''' The GUID ID of the first selected member in this tree listview, or
    ''' <see cref="Guid.Empty"/> if no member is selected.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property ProcessId As Guid
        Get
            Dim m = FirstSelectedItem
            If m Is Nothing Then Return Nothing
            Return m.IdAsGuid
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether or not the Manage Permissions context menu option is
    ''' enabled for this control.
    ''' </summary>
    <Browsable(True),
    Description("Indicates whether or not permissions can be managed for the groups displayed in the tree.")>
    Public Property ManagePermissions As Boolean
        Get
            Return mManagePermissions
        End Get
        Set(value As Boolean)
            mManagePermissions = value
            SetContextMenu()
        End Set
    End Property

    ''' <summary>
    ''' Indicates whether or not the object/process web service name is displayed.
    ''' </summary>
    <Browsable(True),
    Description("Indicates whether or not the object/process web service name is displayed.")>
    Public Property ShowExposedWebServiceName As Boolean
        Get
            Return mShowWSName
        End Get
        Set(value As Boolean)
            mShowWSName = value
            SetColumns()
        End Set
    End Property

    <Browsable(True),
    Description("")>
    Public Property ShowDescription As Boolean
        Get
            Return mShowDescription
        End Get
        Set(value As Boolean)
            mShowDescription = value
            SetColumns()
        End Set
    End Property

    <Browsable(True),
    Description("Indicates whether or not the force Document/literal flag should be shown.")>
    Public Property ShowDocumentLiteralFlag As Boolean
        Get
            Return mShowDocumentLiteralFlag
        End Get
        Set(value As Boolean)
            mShowDocumentLiteralFlag = value
            SetColumns()
        End Set
    End Property

    <Browsable(True),
    Description("Indicates whether or not the Use Legacy RPC XML Namespace should be shown.")>
    Public Property UseLegacyNamespaceFlag As Boolean
        Get
            Return mShowLegacyNamespaceFlag
        End Get
        Set(value As Boolean)
            mShowLegacyNamespaceFlag = value
            SetColumns()
        End Set
    End Property

    <Browsable(True),
    Description("Indicates whether or not the tree should update from store.")>
    Public Property UpdateTreeFromStore As Boolean = False

    ''' <summary>
    ''' Return a list of the names of the selected processes. 
    ''' If a group is selected, all resources in that group (including sub-groups) are returned.
    ''' </summary>
    ''' <returns>A distinct list of selected processes</returns>
    Public Function GetSelected() As List(Of String)

        Dim processes As New List(Of String)

        For Each member In MyBase.GetSelectedMembers(Of IGroupMember)

            ' if selected item is a Group, then get all the names of the resources in the group, including sub-groups.
            If member.GetType() = GetType(ProcessGroupMember) Then
                Dim proc = CType(member, ProcessGroupMember)
                processes.Add(proc.Name)
            ElseIf member.GetType() = GetType(FilteringGroup) Then
                Dim grp = CType(member, FilteringGroup)
                processes.AddRange(
                    grp.FlattenedContents(Of BluePrism.BPCoreLib.Collections.clsSortedSet(Of IGroupMember))(False).
                    ToList().
                    Select(Function(y) y.Name))
            End If
        Next

        ' The same resource could be a member of multiple groups, and therefore may have been selected more than once.
        ' Remove any duplicates
        Return processes.Distinct().ToList()

    End Function

    ''' <summary>
    ''' Handle context menu opening and enable/disable as required.
    ''' </summary>
    Private Sub HandleMenuOpening(sender As Object, e As CancelEventArgs)
        For Each m In Me.ContextMenuStrip.Items
            If TypeOf (m) Is ToolStripSeparator Then Continue For
            Dim item = TryCast(m, ToolStripItem)
            If item IsNot Nothing Then
                If item Is mnuPermissions Then
                    item.Enabled = GetSelectedMembers(Of IGroup).Count = 1 AndAlso GetSelectedMembers(Of IGroupMember).Count = 1
                Else
                    item.Enabled = Me.ItemsCount > 0 AndAlso
                        Me.ShowGroups AndAlso Not Me.ShowFlat
                End If
            End If
        Next
    End Sub

    ''' <summary>
    ''' Handle the Access Rights context menu option
    ''' </summary>
    Public Sub HandleManageGroupPermissions()

        Using f As New frmGroupPermissions(GetSelectedMembers(Of IGroup)(0), False)
            f.SetEnvironmentColoursFromAncestor(Me)
            f.StartPosition = FormStartPosition.CenterParent
            f.ShowInTaskbar = False
            f.ShowDialog()
        End Using
    End Sub

End Class
