#If Nada Then
Imports System.Windows.Forms

Imports BluePrism.Scheduling
Imports BluePrism.Scheduling.Calendar
Imports BluePrism.AutomateAppCore

Imports ProcessAttributes = BluePrism.AutomateProcessCore.clsProcess.ProcessAttributes
Imports GroupInfo = BluePrism.AutomateAppCore.clsServer.GroupInfo
Imports ProcessInfo = BluePrism.AutomateAppCore.clsServer.ProcessInfo

Public Class ctlComponentTree_OLD : Inherits TreeView

    <Flags()> _
    Public Enum ShowIf
        None = 0
        Snapshot = 1 << 0
        BusinessProcess = 1 << 1
        BusinessObject = 1 << 2
        ApplicationModel = 1 << 3
        WorkQueue = 1 << 4
        Schedule = 1 << 5
        Calendar = 1 << 6
        All = &HFFFFFFFF

        'Snapshot _
        ' Or ProcessGroup _
        ' Or BusinessProcess _
        ' Or BusinessObject _
        ' Or ApplicationModel _
        ' Or WorkQueue _
        ' Or Schedule _
        ' Or Calendar

    End Enum

    Private Const DefaultShowComponents As ShowIf = ShowIf.BusinessProcess Or ShowIf.BusinessObject

    Private mShow As ShowIf
    Private mAutoLoad As Boolean
    Private mGroupFont As Font

    Public Sub New()
        Me.New(DefaultShowComponents)
    End Sub

    Public Sub New(ByVal show As ShowIf)
        HideSelection = False ' Why on earth anyone would want otherwise I don't even

        ' Add all the images, even if we're not using them (they may be introduced later)
        MyBase.ImageList = clsImageLists.SmallComponents

        Me.ShowComponents = show
        Me.AllowDrop = True
        mGroupFont = New Font(MyBase.Font, FontStyle.Bold)

    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            mGroupFont.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    Public Property ShowComponents() As ShowIf
        Get
            Return mShow
        End Get
        Set(ByVal value As ShowIf)
            If value = mShow Then Return
            mShow = value
            BeginUpdate()
            Try
                Nodes.Clear()

                If (value And ShowIf.Snapshot) <> 0 Then BuildSnapshots()
                If (value And ShowIf.BusinessProcess) <> 0 Then BuildProcesses()
                If (value And ShowIf.BusinessObject) <> 0 Then BuildObjects()
                If (value And ShowIf.ApplicationModel) <> 0 Then BuildModels()
                If (value And ShowIf.WorkQueue) <> 0 Then BuildQueues()
                If (value And ShowIf.Schedule) <> 0 Then BuildSchedules()
                If (value And ShowIf.Calendar) <> 0 Then BuildCalendars()

            Catch nre As NullReferenceException
                ' There seems to be no way to stop things occurring in DesignMode, since
                ' DesignMode itself isn't set until the constructor is completed.

            Finally
                EndUpdate()

            End Try
        End Set
    End Property

    Protected Overrides Sub OnItemDrag(ByVal e As ItemDragEventArgs)
        Dim n As TreeNode = DirectCast(e.Item, TreeNode)
        If n.Parent IsNot Nothing Then ' Don't drag the root nodes - everything else is fair game
            DoDragDrop(n, DragDropEffects.Move)
        End If
        MyBase.OnItemDrag(e)
    End Sub

    Protected Overrides Sub OnDragEnter(ByVal e As DragEventArgs)

        Dim allowMove As Boolean = False
        If e.Data.GetDataPresent(GetType(TreeNode)) Then
            Dim n As TreeNode = DirectCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
            allowMove = n.TreeView IsNot Me AndAlso TypeOf n.TreeView Is ctlComponentTree_OLD
        End If

        If allowMove Then e.Effect = DragDropEffects.Move Else e.Effect = DragDropEffects.None
        MyBase.OnDragEnter(e)

    End Sub

    Protected Overrides Sub OnDragDrop(ByVal e As DragEventArgs)
        If e.Data.GetDataPresent(GetType(TreeNode)) Then
            Dim n As TreeNode = DirectCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
            ' If we're dropping on ourselves, do nothing....
            If n.TreeView IsNot Me Then
                Dim cd As PackageComponent = TryCast(n.Tag, PackageComponent)
                If cd IsNot Nothing Then
                    AddComponent(cd)
                    DirectCast(n.TreeView, ctlComponentTree_OLD).DroppedNodeAccepted(Me, n)
                End If
            End If
        End If
        MyBase.OnDragDrop(e)
    End Sub

    Private Sub RemoveComponentNode(ByVal n As TreeNode)
        If n.TreeView IsNot Me Then Return
        Dim parentNode As TreeNode = n.Parent
        n.Remove()
        ' If the parent node exists (it really should) and is a root node, and it has no
        ' more children, remove it.
        If parentNode IsNot Nothing _
         AndAlso parentNode.Parent Is Nothing _
         AndAlso parentNode.Nodes.Count = 0 Then
            parentNode.Remove()
        End If
    End Sub

    Private Sub DroppedNodeAccepted(ByVal dest As ctlComponentTree_OLD, ByVal n As TreeNode)
        RemoveComponentNode(n)
    End Sub

    Private Shared sRootNodeLabels As IDictionary(Of String, String) = GenerateRootNodeLabels()
    Private Shared Function GenerateRootNodeLabels() As IDictionary(Of String, String)
        Dim map As New Dictionary(Of String, String)
        map("process") = "Business Processes"
        map("object") = "Business Objects"
        map("model") = "Application Model"
        map("queue") = "Work Queues"
        map("schedule") = "Schedules"
        map("calendar") = "Calendars"
        Return map
    End Function

    Private Function GetRootNode(ByVal datum As PackageComponent) As TreeNode
        Return GetRootNode(datum, True)
    End Function

    Private Function GetRootNode(ByVal datum As PackageComponent, ByVal createIfAbsent As Boolean) As TreeNode

        Dim imageKey As String
        Dim gd As SingleTypeComponentGroup = TryCast(datum, SingleTypeComponentGroup)
        If gd IsNot Nothing Then imageKey = gd.MembersTypeKey Else imageKey = datum.TypeKey

        Dim n As TreeNode = Nodes(imageKey)
        If n IsNot Nothing OrElse Not createIfAbsent Then Return n

        ' imageKey is quite popular.
        n = Nodes.Add(imageKey, sRootNodeLabels(imageKey), imageKey, imageKey)
        n.Expand()
        Return n

    End Function

    Public Sub AddComponent(ByVal ParamArray data() As PackageComponent)
        AddComponents(data)
    End Sub

    Public Sub AddComponents(ByVal data As ICollection(Of PackageComponent))
        BeginUpdate()
        Try
            For Each datum As PackageComponent In data
                Dim r As TreeNode = GetRootNode(datum)
                AddNode(r.Nodes, datum)
                r.Expand()
            Next
        Finally
            EndUpdate()
        End Try
    End Sub

    Public Sub RemoveComponent(ByVal ParamArray data() As PackageComponent)

        BeginUpdate()
        Try
            For Each datum As PackageComponent In data
                Dim root As TreeNode = GetRootNode(datum, False)
                If root Is Nothing Then Continue For
                For Each n As TreeNode In root.Nodes.Find(datum.Name, True)
                    RemoveComponentNode(n)
                Next
            Next
        Finally
            EndUpdate()
        End Try
    End Sub

    Public Property Components() As ICollection(Of PackageComponent)
        Get
            Dim list As New List(Of PackageComponent)
            For Each root As TreeNode In Nodes
                For Each n As TreeNode In root.Nodes
                    Dim cd As PackageComponent = TryCast(n.Tag, PackageComponent)
                    Dim gd As ComponentGroup = TryCast(n.Tag, ComponentGroup)
                    If cd IsNot Nothing Then
                        list.Add(cd)
                        If gd IsNot Nothing Then
                            For Each md As PackageComponent In gd.Members
                                list.Add(md)
                            Next
                        End If
                    End If
                Next
            Next
            Return list
        End Get
        Set(ByVal value As ICollection(Of PackageComponent))
            AddComponents(value)
        End Set
    End Property

    Private Sub Transfer(ByVal inputs As ICollection(Of ProcessInfo), ByVal outputs As ICollection(Of PackageComponent))
        For Each pd As ProcessComponent In Convert(inputs)
            outputs.Add(pd)
        Next
    End Sub

    Private Function Convert(ByVal inputs As ICollection(Of ProcessInfo)) As ICollection(Of ProcessComponent)
        Dim outputs As New List(Of ProcessComponent)
        For Each pi As ProcessInfo In inputs
            Dim pd As ProcessComponent
            If pi.IsBusinessObject Then pd = New ObjectData() Else pd = New ProcessComponent()
            pd.Name = pi.Name
            pd.Id = pi.ID
            outputs.Add(pd)
        Next
        Return outputs
    End Function

    Private Function Convert(ByVal inputs As Dictionary(Of Guid, GroupInfo)) As IDictionary(Of Guid, PackageComponent)
        Dim map As New Dictionary(Of Guid, PackageComponent)
        For Each gi As GroupInfo In inputs.Values
            Dim gd As New ProcessGroupData()
            gd.ID = gi.ID
            gd.Name = gi.Name
            Transfer(gi.Members.Values, gd.Members)
            map(gd.ID) = gd
        Next
        Return map
    End Function

    Private Function Convert(ByVal inputs As IList(Of clsWorkQueue)) As ICollection(Of QueueData)
        Dim out As New List(Of QueueData)
        For Each q As clsWorkQueue In inputs
            Dim qd As New QueueData()
            qd.Name = q.Name
            qd.ID = q.Id
            out.Add(qd)
        Next
        Return out
    End Function

    Private Function AddNode(ByVal name As String, ByVal imageKey As String) As TreeNode
        Return AddNode(Me.Nodes, name, imageKey)
    End Function

    ''' <summary>
    ''' Adds the given component data to the given tree node collection.
    ''' If the given data is a group, then its members are added beneath that, recursively.
    ''' </summary>
    ''' <param name="nodes">The node collection to add the data to</param>
    ''' <param name="cd">The data to add</param>
    ''' <returns>The tree node which was created by adding the given data to the nodes.</returns>
    Private Function AddNode(ByVal nodes As TreeNodeCollection, ByVal cd As PackageComponent) As TreeNode
        Dim n As TreeNode = AddNode(nodes, cd.Name, cd.ImageKey)
        n.Tag = cd
        Dim gd As PackageComponent = TryCast(cd, PackageComponent)
        If gd IsNot Nothing Then
            n.NodeFont = mGroupFont
            For Each memberData As PackageComponent In gd.Members
                AddNode(n.Nodes, memberData)
            Next
        End If
        Return n
    End Function

    Private Function AddNode( _
     ByVal nodes As TreeNodeCollection, ByVal name As String, ByVal imageKey As String) As TreeNode
        Return nodes.Add(name, name, imageKey, imageKey)
    End Function


#Region "BuildThings()"

    Private Sub BuildSnapshots()
        If DesignMode Then Return

        For Each s As Snapshot In Snapshot.GenerateDummies()
            Dim sd As New SnapshotData()
            sd.Name = s.Name
            sd.ID = Guid.NewGuid()
            AddComponent(sd)
        Next
    End Sub

    Private Sub BuildProcesses()

        If DesignMode Then Return

        Dim groups As Dictionary(Of Guid, GroupInfo) = Nothing
        If gSv.ProcessGroupsGetGroups(groups, True, ProcessAttributes.None, ProcessAttributes.Retired, Nothing) Then

            For Each group As PackageComponent In Convert(groups).Values

                AddComponent(group)

                'Dim groupNode As TreeNode = AddNode(root.Nodes, group)
                'groupNode.NodeFont = mGroupFont

                'For Each proc As ProcessComponent In group.Members
                '   AddNode(groupNode.Nodes, proc)
                'Next

            Next

        End If

    End Sub

    Private Sub BuildObjects()
        If DesignMode Then Return

        Dim group As New ObjectGroupData()
        group.Name = "<Ungrouped Objects>"
        'Dim groupNode As TreeNode = AddNode(root.Nodes, group)
        'groupNode.NodeFont = mGroupFont
        For Each obj As ObjectData In Convert( _
         gSv.GetProcessInfos(ProcessAttributes.None, ProcessAttributes.Retired, True))
            group.Members.Add(obj)
        Next
        AddComponent(group)
        ' root.Expand()

    End Sub

    Private Sub BuildModels()
        If DesignMode Then Return

        Dim group As New ModelGroupData()
        group.Name = "<Ungrouped Models>"
        ' Dim groupNode As TreeNode = AddNode(root.Nodes, group)
        ' groupNode.NodeFont = mGroupFont

        For Each obj As ObjectData In Convert( _
         gSv.GetProcessInfos(ProcessAttributes.None, ProcessAttributes.Retired, True))

            Dim model As New ModelData()
            model.ID = Guid.NewGuid()
            model.Name = String.Format("Model from '{0}'", obj.Name)
            group.Members.Add(model)
        Next

        AddComponent(group)
        ' root.Expand()

    End Sub

    Private Sub BuildQueues()
        If DesignMode Then Return

        Dim group As New QueueGroupData()
        group.Name = "<Ungrouped Queues>"
        'Dim groupNode As TreeNode = AddNode(root.Nodes, group)
        'groupNode.NodeFont = mGroupFont

        For Each q As QueueData In Convert(gSv.WorkQueueGetQueues(False))
            group.Members.Add(q)
        Next
        AddComponent(group)

    End Sub

    Private mScheduleStore As IScheduleStore
    Private ReadOnly Property ScheduleStore() As IScheduleStore
        Get
            If mScheduleStore Is Nothing Then mScheduleStore = _
             New clsDatabaseBackedScheduleStore(New InertScheduler())
            Return mScheduleStore
        End Get
    End Property

    Private Sub BuildSchedules()
        If DesignMode Then Return

        For Each sched As ISchedule In ScheduleStore.GetActiveSchedules()
            Dim sd As New ScheduleData()
            sd.Name = sched.Name
            sd.ID = Guid.NewGuid() ' No GUIDs in schedules
            AddComponent(sd)
        Next
    End Sub

    Private Sub BuildCalendars()
        If DesignMode Then Return

        For Each cal As ScheduleCalendar In ScheduleStore.GetAllCalendars()
            Dim cd As New CalendarData()
            cd.Name = cal.Name
            cd.ID = Guid.NewGuid()
            AddComponent(cd)
        Next
    End Sub

#End Region

#Region " Getting Selection "

    Public Property SelectedComponent() As PackageComponent
        Get
            Dim n As TreeNode = SelectedNode
            If n IsNot Nothing Then Return TryCast(n.Tag, PackageComponent)
            Return Nothing
        End Get
        Set(ByVal value As PackageComponent)
            If value Is Nothing Then
                SelectedNode = Nothing
            Else
                Dim found() As TreeNode = Nodes.Find(value.Name, True)
                If found.Length > 0 Then SelectedNode = found(0)
            End If
        End Set
    End Property

#End Region

#Region " Event Handling "

#Region " ComponentSelected and ComponentActivated events "

    Public Event ComponentSelected(ByVal sender As Object, ByVal e As ComponentEventArgs)
    Public Event ComponentActivated(ByVal sender As Object, ByVal e As ComponentEventArgs)

    Protected Overridable Sub OnComponentSelected(ByVal e As ComponentEventArgs)
        RaiseEvent ComponentSelected(Me, e)
    End Sub

    Protected Overridable Sub OnComponentActivated(ByVal e As ComponentEventArgs)
        RaiseEvent ComponentActivated(Me, e)
    End Sub

#End Region

#Region " Translating low level events into component events "

    ''' <summary>
    ''' Select Node => Select component.
    ''' Attempt to get the component that's been selected. If the node represents
    ''' a PackageComponent, raise a ComponentSelected event.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Overrides Sub OnAfterSelect(ByVal e As TreeViewEventArgs)
        Dim comp As PackageComponent = TryCast(e.Node.Tag, PackageComponent)
        If comp IsNot Nothing Then OnComponentSelected(New ComponentEventArgs(comp))
        MyBase.OnAfterSelect(e)
    End Sub

    ''' <summary>
    ''' Handle the keypress to check for enter key being pressed.
    ''' Toggle the node if it has any children, and treat 'Enter' pressed on a
    ''' component as 'activating' that component - ie. raise the ComponentActivated
    ''' event.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Overrides Sub OnKeyPress(ByVal e As KeyPressEventArgs)

        ' Open / close the tree node.
        Dim n As TreeNode = SelectedNode
        If n IsNot Nothing AndAlso n.Nodes.Count > 0 Then n.Toggle()

        ' If enter pressed and the selected node represents a PackageComponent, activate it
        If e.KeyChar = vbLf(0) Then
            Dim comp As PackageComponent = SelectedComponent
            If comp IsNot Nothing Then
                OnComponentActivated(New ComponentEventArgs(comp))
            End If
        End If

        MyBase.OnKeyPress(e)
    End Sub

    ''' <summary>
    ''' Handle double-clicking on a node - if the node represents a PackageComponent,
    ''' translate it into a ComponentActivated event.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Overrides Sub OnNodeMouseDoubleClick(ByVal e As TreeNodeMouseClickEventArgs)
        Dim comp As PackageComponent = TryCast(e.Node.Tag, PackageComponent)
        If comp IsNot Nothing Then OnComponentActivated(New ComponentEventArgs(comp))
        MyBase.OnNodeMouseDoubleClick(e)
    End Sub

#End Region

#End Region


End Class

#End If