Imports AutomateControls.Trees
Imports AutomateUI.Controls.Widgets.ReleaseManager

Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Groups

Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections

Imports BluePrism.AutomateProcessCore

Imports BluePrism.Scheduling.Calendar

Imports BluePrism.Images
Imports Component = BluePrism.Images.ImageLists.Keys.Component
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.Core.Utility
Imports LocaleTools

''' <summary>
''' A class representing a collection of package components in tree form, with each
''' of the component types separated into their own category nodes.
''' </summary>
Public Class ctlComponentTree : Inherits TreeView
    Implements IManageNodeRemoval

#Region " Class scope declarations "

    ''' <summary>
    ''' Returns the collection of nodes selected within this tree
    ''' </summary>
    Public ReadOnly Property SelectedNodes As ArrayList
        Get
            Return nodeCollection
        End Get
    End Property
    Private nodeCollection As New ArrayList()
    'Variables for multi-selection of nodes
    Private lastNode, firstNode As TreeNode

    ''' <summary>
    ''' The format of the data being passed around in a drag/drop operation
    ''' </summary>
    Private Const DragDataFormat As String = "ICollection(Of TreeNode)"

    'Indicates whether or not this tree is the input tree (i.e. where components
    'are selected from)
    Private inputTree As Boolean = False

    ' Collection of root nodes grouped by type.  Processes, Objects.
    Private ReadOnly mRootNodesByComponentType As Dictionary(Of String, TreeNode) = New Dictionary(Of String, TreeNode)

    ' Collection of child nodes to the parent tree nodes.
    'Private ReadOnly mNodesByComponent As Dictionary(Of PackageComponent, TreeNode) = New Dictionary(Of PackageComponent, TreeNode)
    Private ReadOnly mNodesByComponent As Dictionary(Of PackageComponent, HashSet(Of TreeNode)) = New Dictionary(Of PackageComponent, HashSet(Of TreeNode))

    ''' <summary>
    ''' Sorting class for the component tree
    ''' </summary>
    Private Class ComponentTreeSorter : Implements IComparer
        Public Function Compare(x As Object, y As Object) As Integer Implements IComparer.Compare
            Dim xItem As TreeNode = DirectCast(x, TreeNode)
            Dim yItem As TreeNode = DirectCast(y, TreeNode)

            If xItem.Parent Is Nothing AndAlso yItem.Parent Is Nothing Then
                'Order component types as they are defined in PackageComponentType
                Dim typeKeys As List(Of String) = PackageComponentType.AllTypes.Keys.ToList()
                If typeKeys.IndexOf(CStr(xItem.Tag)) < typeKeys.IndexOf(CStr(yItem.Tag)) Then
                    Return -1
                End If
                Return 1
            Else
                'Make sure groups appear before other component types
                Dim xComp As PackageComponent = TryCast(xItem.Tag, PackageComponent)
                Dim yComp As PackageComponent = TryCast(yItem.Tag, PackageComponent)
                If xComp IsNot Nothing AndAlso yComp IsNot Nothing AndAlso xComp.GetType() <> yComp.GetType() Then
                    If TypeOf xComp Is GroupComponent Then
                        Return -1
                    End If
                    Return 1
                End If
            End If
            'Sort nodes of the same type alphabetically
            Return String.Compare(xItem.Text, yItem.Text)
        End Function
    End Class

    ''' <summary>
    ''' Class for holding dependency information (as child nodes of the package
    ''' component that they belong to).
    ''' </summary>
    Private Class DependentItem
        Public Property Component As PackageComponent

        Public Sub New(comp As PackageComponent)
            Component = comp
        End Sub
    End Class

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new, empty, component tree
    ''' </summary>
    Public Sub New()
        Me.ImageList = ImageLists.Components_16x16
        Me.AllowDrop = True
        Me.TreeViewNodeSorter = New ComponentTreeSorter()
        Me.ShowNodeToolTips = True
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The components being rendered in this tree.
    ''' </summary>
    <Browsable(False)> _
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public ReadOnly Property Components() As ICollection(Of PackageComponent)
        Get
            Return ScrapeComponents()
        End Get
    End Property

    ''' <summary>
    ''' Gets the group store to be used to retrieve tree/group information
    ''' </summary>
    Private ReadOnly Property GroupStore As IGroupStore
        Get
            Return GetGroupStore()
        End Get
    End Property

    ''' <summary>
    ''' Re-introduces the Text property, hidden in listviews for some reason.
    ''' This allows control consumers to set the window text of the corresponding
    ''' Win32 control.
    ''' </summary>
    <Browsable(True)> _
    Public Overrides Property Text() As String
        Get
            Return MyBase.Text
        End Get
        Set(value As String)
            MyBase.Text = value
        End Set
    End Property

#End Region

#Region " Component Loading "

    ''' <summary>
    ''' Loads all the data from the database for the specified types, which
    ''' correspond to the keys defined in the 
    ''' <see cref="PackageComponentType.Keys" /> class.
    ''' </summary>
    Public Sub LoadAll()
        inputTree = True
        Dim packageComponents As New clsSet(Of PackageComponent)
        For Each type As PackageComponentType In PackageComponentType.AllTypes.Values
            RetrieveComponents(type, packageComponents)
        Next
        AddComponents(packageComponents)
    End Sub

    ''' <summary>
    ''' Builds the tree from the database, using the given type.
    ''' </summary>
    ''' <param name="type">The type of component to be built</param>
    Private Sub RetrieveComponents( _
     ByVal type As PackageComponentType, ByVal into As IBPSet(Of PackageComponent))
        Try
            Select Case type

                Case PackageComponentType.Process
                    Convert(GroupStore.GetTree(
                        GroupTreeType.Processes, GroupMember.NotRetired, Nothing, False, False, False), type, into)

                Case PackageComponentType.BusinessObject
                    Convert(GroupStore.GetTree(
                        GroupTreeType.Objects, GroupMember.NotRetired, Nothing, False, False, False), type, into)

                Case PackageComponentType.Queue
                    Convert(GroupStore.GetTree(GroupTreeType.Queues, Nothing, Nothing, False, False, False), type, into)

                Case PackageComponentType.Tile
                    Convert(GroupStore.GetTree(GroupTreeType.Tiles, Nothing, Nothing, False, False, False), type, into)

                Case PackageComponentType.Credential
                    Convert(gSv.GetAllCredentialsInfo(), into)

                Case PackageComponentType.Calendar
                    Dim store As DatabaseBackedScheduleStore = _
                     DatabaseBackedScheduleStore.InertStore
                    Convert(store.GetAllCalendars(), into)

                Case PackageComponentType.Schedule
                    Dim store As DatabaseBackedScheduleStore = _
                     DatabaseBackedScheduleStore.InertStore
                    Convert(store.GetActiveSessionRunnerSchedules(), into)

                Case PackageComponentType.ScheduleList ' TODO: Er, implement this. It somehow got missed

                Case PackageComponentType.WebService
                    Convert(gSv.GetWebServiceDefinitions(), into)

                Case PackageComponentType.WebApi
                    Convert(gSv.GetWebApis(), into)

                Case PackageComponentType.EnvironmentVariable
                    Convert(gSv.GetEnvironmentVariables(), into)

                Case PackageComponentType.Font
                    ConvertFonts(gSv.GetFonts(), into)

                Case PackageComponentType.Dashboard
                    Convert(gSv.GetDashboardList(), into)
            End Select

        Catch ex As Exception
            UserMessage.Show(String.Format(
             My.Resources.ctlComponentTree_FailedToBuildType0InComponentTree1, type, ex.Message), ex)

        End Try
    End Sub

#Region " Convert Methods (native => components) "

    ''' <summary>
    ''' Converts the contents of the given tree into package components (according
    ''' to the member type for the group).
    ''' </summary>
    ''' <param name="gt">The tree whose contents is to be converted</param>
    ''' <param name="type">The member type for the group</param>
    ''' <param name="into">The collection of package components into which the
    ''' converted group members are to be added</param>
    Private Sub Convert(gt As IGroupTree, type As PackageComponentType,
                        into As ICollection(Of PackageComponent))
        Convert(gt.Root, type, into)
    End Sub

    ''' <summary>
    ''' Converts the contents of the given group into package components (according
    ''' to the member type for the group).
    ''' </summary>
    ''' <param name="inputs">The group to convert</param>
    ''' <param name="type">The member type for the group</param>
    ''' <param name="into">The collection of package components into which the
    ''' converted group members are to be added</param>
    Private Sub Convert(inputs As IGroup, type As PackageComponentType,
                        into As ICollection(Of PackageComponent))
        For Each mem As IGroupMember In inputs
            Select Case mem.MemberType
                Case GroupMemberType.Group
                    Dim gp As IGroup = DirectCast(mem, IGroup)
                    If gp.ContainsInSubtree(Function(m) Not m.IsGroup) Then
                        Dim comp As New GroupComponent(
                         Nothing, gp.IdAsGuid, String.Join("/", gp.Breadcrumbs()), type, gp.IsDefault)
                        Convert(gp, type, comp)
                        into.Add(comp)
                    End If
                Case GroupMemberType.Process
                    into.Add(New ProcessComponent(Nothing, mem.IdAsGuid, mem.Name))
                Case GroupMemberType.Object
                    into.Add(New VBOComponent(Nothing, mem.IdAsGuid, mem.Name))
                Case GroupMemberType.Queue
                    into.Add(New WorkQueueComponent(Nothing, mem.IdAsInteger, mem.Name))
                Case GroupMemberType.Tile
                    into.Add(New TileComponent(Nothing, mem.IdAsGuid, mem.Name))
            End Select
        Next
    End Sub

    ''' <summary>
    ''' Converts the given collection of dashboard objects into package components
    ''' </summary>
    ''' <param name="inputs">The collection of dashboards to convert</param>
    ''' <param name="into">The collection of package components into which the
    ''' converted dashboards are to be added.</param>
    Public Sub Convert(inputs As ICollection(Of Dashboard), into As ICollection(Of PackageComponent))
        For Each d As Dashboard In inputs
            If d.Type = DashboardTypes.Global Or d.Type = DashboardTypes.Published Then
                into.Add(New DashboardComponent(Nothing, d.ID, d.Name))
            End If
        Next
    End Sub

    ''' <summary>
    ''' Converts the given collection of credential objects into package components
    ''' </summary>
    ''' <param name="inputs">The collection of <see cref="clsCredential"/> objects to
    ''' convert</param>
    ''' <param name="into">The collection of package components into which the
    ''' converted <see cref="CredentialComponent"/>s are to be added.</param>
    Private Sub Convert(
     ByVal inputs As ICollection(Of clsCredential), ByVal into As ICollection(Of PackageComponent))
        For Each c As clsCredential In inputs
            into.Add(New CredentialComponent(Nothing, c))
        Next
    End Sub

    ''' <summary>
    ''' Converts the given collection of schedule objects into package components
    ''' </summary>
    ''' <param name="scheds">The collection of schedule objects to convert.</param>
    ''' <param name="into">A corresponding collection of
    ''' <see cref="PackageComponent"/> objects into which the converted components
    ''' should be added</param>
    Private Sub Convert(ByVal scheds As ICollection(Of SessionRunnerSchedule),
     ByVal into As ICollection(Of PackageComponent))
        For Each sched As SessionRunnerSchedule In scheds
            ' Add a schedule with no owner and the default store
            into.Add(New ScheduleComponent(Nothing, Nothing, sched))
        Next
    End Sub

    ''' <summary>
    ''' Converts the given collection of calendar objects into package components
    ''' </summary>
    ''' <param name="cals">The collection of calendar objects to convert.</param>
    ''' <param name="into">A corresponding collection of
    ''' <see cref="PackageComponent"/> objects into which the converted components
    ''' should be added</param>
    Private Sub Convert(ByVal cals As ICollection(Of ScheduleCalendar),
     ByVal into As ICollection(Of PackageComponent))
        For Each cal As ScheduleCalendar In cals
            into.Add(New CalendarComponent(Nothing, Nothing, cal))
        Next
    End Sub

    ''' <summary>
    ''' Converts the given collection of environment variable objects into package
    ''' components
    ''' </summary>
    ''' <param name="inputs">The collection of <see cref="clsEnvironmentVariable"/>
    ''' objects to convert</param>
    ''' <param name="into">The collection into which the converted
    ''' <see cref="EnvironmentVariableComponent"/>s should go.</param>
    Private Sub Convert(ByVal inputs As ICollection(Of clsEnvironmentVariable),
     ByVal into As ICollection(Of PackageComponent))
        For Each ev As clsEnvironmentVariable In inputs
            into.Add(New EnvironmentVariableComponent(Nothing, ev))
        Next
    End Sub

    ''' <summary>
    ''' Converts the given collection of web service details objects into package
    ''' components
    ''' </summary>
    ''' <param name="inputs">The collection of <see cref="clsWebServiceDetails"/>
    ''' objects to convert</param>
    ''' <param name="into">The collection into which the converted
    ''' <see cref="WebServiceComponent"/>s should be added</param>
    Private Sub Convert(ByVal inputs As ICollection(Of clsWebServiceDetails),
     ByVal into As ICollection(Of PackageComponent))
        For Each ws As clsWebServiceDetails In inputs
            into.Add(New WebServiceComponent(Nothing, ws))
        Next
    End Sub


    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Converts the given collection of web api objects into package
    ''' components
    ''' </summary>
    ''' <param name="inputs">The collection of <see cref="WebApi"/>
    ''' objects to convert</param>
    ''' <param name="into">The collection into which the converted
    ''' <see cref="WebApiComponent"/>s should be added</param>
    ''' -----------------------------------------------------------------------------
    Private Sub Convert(ByVal inputs As ICollection(Of WebApi),
                        ByVal into As ICollection(Of PackageComponent))
        For Each api As WebApi In inputs
            into.Add(New WebApiComponent(Nothing, api))
        Next
    End Sub

    ''' <summary>
    ''' Converts the data from the given data table into package components
    ''' </summary>
    ''' <param name="tab">The table containing the font data - this is expected to
    ''' have a column named "name" which holds the font name</param>
    ''' <param name="into">The collection components into which the converted
    ''' <see cref="FontComponent"/>s should be added.</param>
    Private Sub ConvertFonts(
     ByVal tab As DataTable, ByVal into As ICollection(Of PackageComponent))
        Using reader As IDataReader = tab.CreateDataReader()
            Dim prov As New ReaderDataProvider(reader)
            While reader.Read()
                into.Add(New FontComponent(Nothing, prov))
            End While
        End Using
    End Sub

#End Region

#End Region

#Region " Add Component/s "

    ''' <summary>
    ''' Adds the given components to this tree.
    ''' </summary>
    ''' <param name="packageComponents">The components to add to this tree</param>
    Public Sub AddComponents(packageComponents As ICollection(Of PackageComponent))
        BeginUpdate()
        Try
            ' Add the component to the appropriate root node
            For Each component As PackageComponent In packageComponents
                Dim rootNodeType = GetRootNode(component)
                AddComponentTo(rootNodeType, component)
            Next
        Finally
            EndUpdate()
        End Try
    End Sub

    ''' <summary>
    ''' Adds the given component to this tree.
    ''' </summary>
    ''' <param name="packageComponents">The components to add to this component tree</param>
    Private Sub AddComponent(ParamArray packageComponents() As PackageComponent)
        AddComponents(packageComponents)
    End Sub

    ''' <summary>
    ''' Adds the given component to the specified parent item
    ''' </summary>
    ''' <param name="parent">The parent node to add to. Note that if the component
    ''' has a group which is set to <see
    ''' cref="ComponentGroup.ShowMembersInComponentTree"/> then a node representing
    ''' the group will be inserted as a child of the parent node, and the parent of
    ''' the specified component.
    ''' </param>
    ''' <param name="comp">The component to add</param>
    Private Sub AddComponentTo(parent As TreeNode, comp As PackageComponent)
        AddComponentsTo(parent, New PackageComponent() {comp})
    End Sub

    ''' <summary>
    ''' Adds the given components to the given parent node
    ''' </summary>
    ''' <param name="parent">The parent node to add to. Note that if any of the
    ''' components are a group which is set to <see
    ''' cref="ComponentGroup.ShowMembersInComponentTree"/> then nodes representing
    ''' the group's contents will be appended as children of the node created to
    ''' represent that group.
    ''' </param>
    ''' <param name="components">The collection of components to add.</param>
    Private Sub AddComponentsTo(parent As TreeNode, components As ICollection(Of PackageComponent))

        If components Is Nothing OrElse components.Count = 0 Then Return

        For Each comp As PackageComponent In components
            Dim groupComponent As GroupComponent = TryCast(comp, GroupComponent)
            Dim item As TreeNode = FindWithinParent(parent, comp)

            If item Is Nothing Then

                ' Add the item
                item = New TreeNode()
                Dim typeLabel = PackageComponentType.GetLocalizedFriendlyName(comp.Type, capitalize:=True)
                If groupComponent Is Nothing Then
                    item.Text = If(comp.TypeKey = "tile", LTools.GetC(comp.Name, "Tile"), comp.Name)
                    item.ImageKey = comp.TypeKey
                    item.SelectedImageKey = comp.TypeKey
                    item.ToolTipText = typeLabel
                    AddDependentItems(item, comp.GetDependents())
                Else
                    item.Text = PackageComponentType.GetLocalizedFriendlyName(GroupDisplayName(comp.Name), capitalize:=True)
                    item.ImageKey = Component.ClosedGroup
                    item.SelectedImageKey = Component.ClosedGroup
                    item.ToolTipText = typeLabel
                End If
                item.Tag = comp
                parent.Nodes.Add(item)
                Dim pg As GroupComponent = TryCast(parent.Tag, GroupComponent)
                If pg IsNot Nothing Then pg.Add(comp)
                If Not inputTree Then parent.Expand()

                IndexNodeByComponent(comp, item)

            End If

            ' If it's a group, and we should be displaying its members, do so.
            If groupComponent IsNot Nothing AndAlso groupComponent.ShowMembersInComponentTree Then
                AddComponentsTo(item, groupComponent.Members)
            End If


        Next
    End Sub

    Private Sub IndexNodeByComponent(component As PackageComponent, node As TreeNode)

        Dim items = mNodesByComponent.GetOrAdd(component, Function() New HashSet(Of TreeNode))
        items.Add(node)

    End Sub

    ''' <summary>
    ''' Removes node from dictionary used to quickly look up node by component
    ''' </summary>
    ''' <param name="node"></param>
    Private Sub RemoveNodeByComponent(node As TreeNode)

        Dim component = TryCast(node.Tag, PackageComponent)
        If component IsNot Nothing Then
            Dim componentNodes As HashSet(Of TreeNode) = Nothing
            If mNodesByComponent.TryGetValue(component, componentNodes) Then
                componentNodes.Remove(node)
                If componentNodes.Count = 0 Then
                    mNodesByComponent.Remove(component)
                End If
            End If
        End If

    End Sub

    ''' <summary>
    ''' Returns the display name for the passed group (i.e. the part after the last
    ''' separator).
    ''' </summary>
    Private Function GroupDisplayName(name As String) As String
        If Not name.Contains("/") _
            Then Return name _
            Else Return name.Substring(name.LastIndexOf("/") + 1)
    End Function

    ''' <summary>
    ''' Add any dependent items as child nodes of the package component
    ''' </summary>
    Private Sub AddDependentItems(parent As TreeNode, comps As ICollection(Of PackageComponent))
        For Each comp As PackageComponent In comps
            Dim item As TreeNode = New TreeNode()
            item.Text = comp.Name
            item.ImageKey = comp.TypeKey
            item.SelectedImageKey = comp.TypeKey
            item.Tag = New DependentItem(comp)
            item.NodeFont = New Font(Me.Font, FontStyle.Italic)
            item.ToolTipText = String.Format(My.Resources.ctlComponentTree_0AutomaticallyIncluded, PackageComponentType.GetLocalizedFriendlyName(comp.Type))
            parent.Nodes.Add(item)
        Next
    End Sub

#End Region

#Region " Get Components "

    ''' <summary>
    ''' Scrapes the collection of package components currently held in this tree.
    ''' </summary>
    ''' <returns>The collection of components represented by this tree</returns>
    Private Function ScrapeComponents() As ICollection(Of PackageComponent)
        Dim comps As New clsSortedSet(Of PackageComponent)
        For Each item As TreeNode In Me.Nodes
            comps.Union(GetAllComponents(item.Nodes))
        Next
        Return comps
    End Function

    ''' <summary>
    ''' Gets all the components from the given item collection and its descendents
    ''' </summary>
    ''' <param name="items">The item collection from which the components should be
    ''' retrieved.</param>
    ''' <returns>The collection of package components from the given item collection
    ''' and any of its contents and descendents</returns>
    Private Function GetAllComponents(items As TreeNodeCollection) _
     As ICollection(Of PackageComponent)
        Dim comps As New clsSet(Of PackageComponent)
        For Each item As TreeNode In items
            Dim comp As PackageComponent = TryCast(item.Tag, PackageComponent)
            If comp Is Nothing Then Continue For
            Dim gp As ComponentGroup = TryCast(comp, ComponentGroup)
            ' We don't want to add a component for empty member-showing groups.
            ' Basically, this is for the "<Ungrouped Processes>" type group.
            ' We still want to add its members, however.
            If (gp Is Nothing OrElse Not gp.ShowMembersInComponentTree OrElse gp.HasId) _
             AndAlso Not TypeOf comp Is GroupComponent Then
                comps.Add(comp)
            End If
            If item.Nodes.Count > 0 Then comps.Union(GetAllComponents(item.Nodes))
        Next
        Return comps
    End Function

#End Region

#Region " Drag and Drop "

    ''' <summary>
    ''' Handles an item (or multiple items) being dragged from this tree
    ''' </summary>
    Protected Overrides Sub OnItemDrag(e As ItemDragEventArgs)
        MyBase.OnItemDrag(e)
        If Not Me.SelectedNodes.Contains(e.Item) Then
            Me.SelectedNode = CType(e.Item, TreeNode)
        End If
        Dim items As New List(Of TreeNode)
        Try
            BeginUpdate()
            For Each n As TreeNode In Me.SelectedNodes
                Dim comp As PackageComponent = TryCast(n.Tag, PackageComponent)
                If comp IsNot Nothing Then
                    If TypeOf comp Is GroupComponent Then
                        GetComponentNodes(n, items)
                    Else
                        items.AddRange(FindWithinTree(comp))
                    End If
                Else
                    Dim dep As DependentItem = TryCast(n.Tag, DependentItem)
                    If dep IsNot Nothing Then
                        items.AddRange(FindWithinTree(dep.Component))
                    Else
                        items.Add(n)
                    End If
                End If
            Next

            DoDragDrop(New DataObject(DragDataFormat, items), DragDropEffects.Move)
        Finally
            EndUpdate()
        End Try
    End Sub

    ''' <summary>
    ''' Get all component nodes (recursively) belonging to the passed group node.
    ''' </summary>
    ''' <param name="gpNode">The group node</param>
    ''' <param name="nodeList">The list of nodes to add to</param>
    Private Sub GetComponentNodes(gpNode As TreeNode, nodeList As List(Of TreeNode))
        For Each n As TreeNode In gpNode.Nodes
            Dim comp As PackageComponent = TryCast(n.Tag, PackageComponent)
            If comp IsNot Nothing Then
                If TypeOf comp Is GroupComponent Then
                    GetComponentNodes(n, nodeList)
                Else
                    nodeList.AddRange(FindWithinTree(comp))
                End If
            End If
        Next
    End Sub

    ''' <summary>
    ''' Handles items being dragged into this tree.
    ''' </summary>
    Protected Overrides Sub OnDragEnter(e As DragEventArgs)
        MyBase.OnDragEnter(e)
        If e.Data.GetDataPresent(DragDataFormat) Then
            ' Check the first item - if it is from this tree then don't
            ' allow the drag. If it is from another tree then it is
            ' allowed to continue. If the collection is null or empty then
            ' don't allow the drag.
            Dim items As ICollection(Of TreeNode) =
             DirectCast(e.Data.GetData(DragDataFormat), ICollection(Of TreeNode))
            If items?.TakeWhile(Function(item) item.TreeView IsNot Me).Any() Then
                e.Effect = DragDropEffects.Move
                Return
            End If
        End If

        e.Effect = DragDropEffects.None
    End Sub

    ''' <summary>
    ''' Handles items being dropped onto this tree.
    ''' </summary>
    Protected Overrides Sub OnDragDrop(e As DragEventArgs)
        MyBase.OnDragDrop(e)
        BeginUpdate()
        Try
            Dim items As ICollection(Of TreeNode) =
             DirectCast(e.Data.GetData(DragDataFormat), ICollection(Of TreeNode))
            If items.Count > 0 AndAlso items(0).TreeView IsNot Me Then
                PutNodes(items)
            End If
        Finally
            EndUpdate()
        End Try
    End Sub

    ''' <summary>
    ''' Handles a package component being dropped onto this tree.
    ''' </summary>
    ''' <param name="comp">The component to be dropped on this tree.</param>
    ''' <param name="groupCollection">The group of components to retrieve a Tree Node from.</param>
    ''' <returns>True if the package component was successfully added to this tree,
    ''' False otherwise.</returns>
    Private Function DoDrop(comp As PackageComponent, groupCollection As ICollection(Of GroupComponent)) As Boolean

        If comp Is Nothing Then Return False

        ' If it's in a group, we need to ensure it gets assigned to the corresponding
        ' group in this tree and that it displays correctly
        If groupCollection IsNot Nothing AndAlso groupCollection.Count > 0 Then
            Dim groupTreeItem As TreeNode = Nothing
            Dim lastGroupItem As TreeNode = Nothing

            For i As Integer = groupCollection.Count - 1 To 0 Step -1
                ' Find the group in this tree.
                groupTreeItem = FindWithinTree(groupCollection(i)).FirstOrDefault

                If groupTreeItem Is Nothing Then
                    Dim newGroup As ComponentGroup = DirectCast(groupCollection(i).CloneDisconnected(), ComponentGroup)

                    If lastGroupItem Is Nothing Then
                        AddComponent(newGroup)
                    Else
                        DirectCast(lastGroupItem.Tag, ComponentGroup).Add(newGroup)
                        AddComponentTo(lastGroupItem, newGroup)
                    End If

                    groupTreeItem = FindWithinTree(newGroup).FirstOrDefault
                End If

                lastGroupItem = groupTreeItem
            Next

            DirectCast(groupTreeItem.Tag, ComponentGroup).Add(comp)
            AddComponentTo(groupTreeItem, comp)
        Else
            AddComponent(comp)
        End If
        Return True

    End Function

#End Region

#Region " Find / Node Access Methods "

    ''' <summary>
    ''' Gets the root node for components of the given type.  If not found then a new
    ''' TreeNode root is created.
    ''' </summary>
    ''' <param name="component">The component for which the root node is required.</param>
    ''' <returns>The treelistviewitem which represents the root node for components
    ''' of the given type, creating such an item first if it does not already exist.
    ''' </returns>
    Private Function GetRootNode(component As PackageComponent) As TreeNode
        ' If it's a group in which we're showing the members, and it's a single
        ' type of component - the root for this type is the type of component it
        ' contains, rather than the group type itself - eg. 'processes' not 'process groups'
        Dim typeKey As String = Nothing
        Dim group = TryCast(component, SingleTypeComponentGroup)
        If group IsNot Nothing AndAlso group.ShowMembersInComponentTree Then
            typeKey = group.MembersTypeKey
        Else
            typeKey = component.TypeKey
        End If

        Dim rootNode = Nodes.Cast(Of TreeNode).
            FirstOrDefault(Function(node) Equals(node.Tag, typeKey))
        If rootNode Is Nothing Then
            ' If it wasn't there, we create it and return that
            rootNode = New TreeNode(PackageComponentType.GetLocalizedFriendlyName(PackageComponentType.AllTypes(typeKey).Plural, True, True))
            rootNode.ImageKey = typeKey
            rootNode.SelectedImageKey = typeKey
            rootNode.Tag = typeKey
            Nodes.Add(rootNode)
        End If
        Return rootNode
    End Function

    ''' <summary>
    ''' Finds the first node based on the given component at or within the specified 
    ''' node
    ''' </summary>
    ''' <param name="parentNode">The parent node within which to search</param>
    ''' <param name="component">The component to search for</param>
    ''' <returns>The node or null if no such item was found</returns>
    Private Function FindWithinParent(parentNode As TreeNode, component As PackageComponent) As TreeNode

        Return FindWithinTree(component).
            FirstOrDefault(Function(node) node.GetAncestorsAndSelf().Contains(parentNode))

    End Function

    ''' <summary>
    ''' Finds all nodes based on the passed component anywhere in the tree
    ''' </summary>
    ''' <param name="comp">The component to look for</param>
    ''' <returns>The collection of nodes containing the component</returns>
    Private Function FindWithinTree(comp As PackageComponent) As IEnumerable(Of TreeNode)

        Dim result As IEnumerable(Of TreeNode) = mNodesByComponent.GetOrDefault(comp)
        Return If(result, Enumerable.Empty(Of TreeNode)())

    End Function

#End Region

#Region " Tree event handlers "

    'Handles expand of group node
    Protected Overrides Sub OnAfterExpand(e As TreeViewEventArgs)
        MyBase.OnAfterExpand(e)
        Dim g As GroupComponent = TryCast(e.Node.Tag, GroupComponent)
        If g IsNot Nothing Then
            e.Node.ImageKey = Component.OpenGroup
            e.Node.SelectedImageKey = Component.OpenGroup
        End If
    End Sub

    'Handles collapse of group node
    Protected Overrides Sub OnAfterCollapse(e As TreeViewEventArgs)
        MyBase.OnAfterCollapse(e)
        Dim g As GroupComponent = TryCast(e.Node.Tag, GroupComponent)
        If g IsNot Nothing Then
            e.Node.ImageKey = Component.ClosedGroup
            e.Node.SelectedImageKey = Component.ClosedGroup
        End If
    End Sub

    'Handles before selection of node
    Protected Overrides Sub OnBeforeSelect(e As TreeViewCancelEventArgs)
        MyBase.OnBeforeSelect(e)

        Dim ctrlPressed As Boolean = (ModifierKeys = Keys.Control)
        Dim shiftPressed As Boolean = (ModifierKeys = Keys.Shift)

        If ctrlPressed AndAlso nodeCollection.Contains(e.Node) Then
            'De-select node if already selected and CTRL pressed
            e.Cancel = True
            nodeCollection.Remove(e.Node)
            UnHighlightNode(e.Node)
            Return
        End If

        lastNode = e.Node
        If Not shiftPressed Then firstNode = e.Node
    End Sub

    'Handles after selection of node
    Protected Overrides Sub OnAfterSelect(e As TreeViewEventArgs)
        MyBase.OnAfterSelect(e)

        Dim ctrlPressed As Boolean = (ModifierKeys = Keys.Control)
        Dim shiftPressed As Boolean = (ModifierKeys = Keys.Shift)

        If ctrlPressed Then
            If nodeCollection.Contains(e.Node) Then
                nodeCollection.Remove(e.Node)
                UnHighlightNode(e.Node)
            Else
                nodeCollection.Add(e.Node)
                HighlightNode(e.Node)
            End If
        ElseIf shiftPressed Then
            Dim myQueue As Queue = New Queue()

            Dim upperNode As TreeNode = firstNode
            Dim bottomNode As TreeNode = e.Node

            'Begin and end nodes are parents
            Dim bParent As Boolean = e.Node.Parent Is firstNode 'is firstNode parent (direct or not) of e.Node
            If Not bParent Then
                bParent = upperNode.Parent Is bottomNode
                If bParent Then '// swap nodes
                    Dim t As TreeNode = upperNode
                    upperNode = bottomNode
                    bottomNode = t
                End If
            End If
            If bParent Then
                Dim n As TreeNode = bottomNode
                While n IsNot upperNode.Parent
                    If Not nodeCollection.Contains(n) Then
                        myQueue.Enqueue(n)
                    End If
                    n = n.Parent
                End While
            Else
                'Neither the begin or end nodes are descendent one another
                If ((upperNode.Parent Is Nothing AndAlso bottomNode.Parent Is Nothing) OrElse _
                 (upperNode.Parent IsNot Nothing AndAlso upperNode.Parent.Nodes.Contains(bottomNode))) Then

                    Dim nIndexUpper As Integer = upperNode.Index
                    Dim nIndexBottom As Integer = bottomNode.Index
                    If nIndexBottom < nIndexUpper Then
                        Dim t As TreeNode = upperNode
                        upperNode = bottomNode
                        bottomNode = t
                        nIndexUpper = upperNode.Index
                        nIndexBottom = bottomNode.Index
                    End If

                    Dim n As TreeNode = upperNode
                    While nIndexUpper <= nIndexBottom
                        If (Not nodeCollection.Contains(n)) Then
                            myQueue.Enqueue(n)
                        End If
                        n = n.NextNode
                        nIndexUpper += 1
                    End While
                Else
                    If Not nodeCollection.Contains(upperNode) Then myQueue.Enqueue(upperNode)
                    If Not nodeCollection.Contains(bottomNode) Then myQueue.Enqueue(bottomNode)
                End If
            End If

            nodeCollection.AddRange(myQueue)
            HighlightNodes(myQueue)
            firstNode = e.Node
        Else
            'Simple click: clear any selected nodes and add this one
            If nodeCollection IsNot Nothing AndAlso nodeCollection.Count > 0 Then
                UnHighlightNodes(nodeCollection)
                nodeCollection.Clear()
            End If
            nodeCollection.Add(e.Node)
            HighlightNode(e.Node)
        End If

    End Sub

    'Highlight passed nodes
    Private Sub HighlightNodes(nodes As ICollection)
        For Each n As TreeNode In nodes
            HighlightNode(n)
        Next
    End Sub

    'Highlight passed node
    Private Sub HighlightNode(n As TreeNode)
        n.BackColor = SystemColors.Highlight
        n.ForeColor = SystemColors.HighlightText
    End Sub

    'Un-highlight passed nodes
    Private Sub UnHighlightNodes(nodes As ICollection)
        For Each n As TreeNode In nodes
            UnHighlightNode(n)
        Next
    End Sub

    'Un-highlight passed node
    Private Sub UnHighlightNode(n As TreeNode)
        n.BackColor = Me.BackColor
        n.ForeColor = Me.ForeColor
    End Sub

#End Region

#Region " Node Handling "

    ''' <summary>
    ''' Returns all nodes containing components in the passed collection
    ''' </summary>
    ''' <param name="comps">The collection of components to look for</param>
    ''' <returns>The collection of containing nodes</returns>
    Public Function GetNodes(comps As ICollection(Of PackageComponent)) As ICollection(Of TreeNode)
        Dim items As New List(Of TreeNode)
        For Each comp As PackageComponent In comps
            items.AddRange(FindWithinTree(comp))
        Next
        Return items
    End Function

    ''' <summary>
    ''' Adds the passed nodes (including ancestry where it doesn't already exist) to
    ''' this tree.
    ''' </summary>
    ''' <param name="nodes">The nodes to add</param>
    Public Sub PutNodes(nodes As ICollection(Of TreeNode))
        For Each item As TreeNode In nodes
            Dim comp As PackageComponent = TryCast(item.Tag, PackageComponent)
            If item.Parent IsNot Nothing Then
                Dim ancestry As New List(Of GroupComponent)
                AddParents(item, ancestry)
                If DoDrop(comp, ancestry) Then RemoveNodeAndEmptyParents(item)
            Else
                For i As Integer = item.Nodes.Count - 1 To 0 Step -1
                    comp = TryCast(item.Nodes(i).Tag, PackageComponent)
                    If DoDrop(comp, Nothing) Then RemoveNodeAndEmptyParents(item.Nodes(i))
                Next
                RemoveNodeAndEmptyParents(item)
            End If
        Next
    End Sub

    ''' <summary>
    ''' Adds ancestry information (groups) to this node
    ''' </summary>
    ''' <param name="n">The node</param>
    ''' <param name="into">The group collection to add to</param>
    Private Sub AddParents(n As TreeNode, into As ICollection(Of GroupComponent))
        If n.Parent Is Nothing Then Return
        Dim gp As GroupComponent = TryCast(n.Parent.Tag, GroupComponent)
        If gp IsNot Nothing Then
            into.Add(gp)
            AddParents(n.Parent, into)
        End If
    End Sub

    ''' <summary>
    ''' Remove a node from this tree, recursively removing parents made empty
    ''' following removal.
    ''' </summary>
    ''' <param name="node">The node to remove</param>
    Private Sub RemoveNodeAndEmptyParents(node As TreeNode)

        Dim parentNode = node.Parent

        RemoveNodeViaTreeViewOrDefault(node)

        If parentNode IsNot Nothing AndAlso parentNode.Nodes.Count = 0 Then
            RemoveNodeAndEmptyParents(parentNode)
        End If

    End Sub

    ''' <summary>
    ''' Removes a node, using the node's TreeView to manage removal if it implements
    ''' <see cref="IManageNodeRemoval"/> or the node's Remove method if not.
    ''' </summary>
    ''' <param name="node">The node to remove</param>
    Private Sub RemoveNodeViaTreeViewOrDefault(node As TreeNode)

        Dim treeView = TryCast(node.TreeView, IManageNodeRemoval)
        If treeView IsNot Nothing Then
            treeView.RemoveNode(node)
        Else
            node.Remove()
        End If

    End Sub

#End Region

    ''' <summary>
    ''' RemoveNode implementation used to update backing dictionary used to index nodes
    ''' by the component
    ''' </summary>
    ''' <param name="node"></param>
    Public Sub RemoveNode(node As TreeNode) Implements IManageNodeRemoval.RemoveNode

        For Each subtreeNode As TreeNode In node.GetDescendantsAndSelf()
            RemoveNodeByComponent(subtreeNode)
        Next
        node.Remove()
    End Sub

End Class
