Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Images

''' <summary>
''' Control to handle the bundling of the package - ie. the definition of the
''' components which should go together to form the package.
''' </summary>
Public Class ctlPackageBundler : Inherits ctlWizardStageControl

#Region " Properties "

    ''' <summary>
    ''' The process/object ID of the selected tree node.
    ''' </summary>
    Private mSelectedNodeId As String

    ''' <summary>
    ''' The process/object name of the selected tree node.
    ''' </summary>
    Private mSelectedNodeName As String

    ''' <summary>
    ''' A processes/object's hierarchy of dependencies.
    ''' </summary>
    Private mAllDependencies As New List(Of clsProcessDependency)

    ''' <summary>
    ''' Gets or sets the package components to be displayed as the package in this
    ''' control.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property PackageComponents() As ICollection(Of PackageComponent)
        Get
            Return mOutputTree.Components
        End Get
        Set(ByVal value As ICollection(Of PackageComponent))
            'Load all components from database into input tree
            mInputTree.LoadAll()

            'Move any components already selected for this package to the output tree
            mOutputTree.PutNodes(mInputTree.GetNodes(value))
            If mOutputTree.Nodes.Count > 0 Then mOutputTree.Nodes(0).EnsureVisible()
        End Set
    End Property

#End Region

#Region " Component Tree Event Handlers "

    ''' <summary>
    ''' Handles launching of component tree context menu
    ''' </summary>
    Private Sub HandleTreeMenu(sender As Object, e As MouseEventArgs) _
     Handles mInputTree.MouseUp, mOutputTree.MouseUp
        If e.Button <> MouseButtons.Right Then Return

        Dim tree = CType(sender, ctlComponentTree)
        If tree.SelectedNodes.Count <= 1 Then
            'lets make sure that the node being right clicked is the selected node
            Try
                tree.SelectedNode = tree.GetNodeAt(e.X, e.Y)
            Catch
                'nothing to catch, leave the selection as it was
            End Try
        End If

        Dim ctxMenu = New ContextMenuStrip With {
            .Tag = tree
        }

        'Add item for selecting dependencies, if we have one process/VBO selected
        Dim item As ToolStripItem
        item = ctxMenu.Items.Add(My.Resources.SelectDependencies, ToolImages.Site_Map2_16x16,
                                 AddressOf HandleSelectDependencies)
        If tree.SelectedNodes.Count = 1 AndAlso
         TypeOf CType(tree.SelectedNodes(0), TreeNode).Tag Is ProcessComponent Then
            item.Enabled = True
        Else
            item.Enabled = False
        End If
        'Add options to expand/collapse tree nodes
        ctxMenu.Items.Add(New ToolStripSeparator())
        ctxMenu.Items.Add(My.Resources.ExpandAll, ToolImages.Expand_All_16x16, AddressOf HandleExpandAll)
        ctxMenu.Items.Add(My.Resources.ctlResourceView_CollapseAll, ToolImages.Collapse_All_16x16, AddressOf HandleCollapseAll)
        ctxMenu.Show(tree, e.Location)
    End Sub

    ''' <summary>
    ''' Handles select dependencies context menu option
    ''' </summary>
    Private Sub HandleSelectDependencies(sender As Object, e As EventArgs)
        mAllDependencies.Clear()
        Dim tree = CType(CType(sender, ToolStripItem).Owner.Tag, ctlComponentTree)
        Dim comp = CType(CType(tree.SelectedNodes(0), TreeNode).Tag, PackageComponent)
        mSelectedNodeId = comp.IdAsGuid.ToString
        mSelectedNodeName = comp.Name
        Dim selectedComps As New clsSet(Of PackageComponent)
        Dim unSelectedComps As New clsSet(Of PackageComponent)

        Dim topLevelDependencies = gSv.GetExternalDependencies(comp.IdAsGuid).Dependencies
        For Each dependency In topLevelDependencies
            GetExternalDependencies(dependency)
        Next

        For Each dep As clsProcessDependency In mAllDependencies
            Dim fn As Func(Of PackageComponent, Boolean) = Nothing
            If TypeOf dep Is clsProcessCalendarDependency Then
                fn = Function(c) c.Type = PackageComponentType.Calendar AndAlso
                 c.Name = CType(dep, clsProcessCalendarDependency).RefCalendarName
            ElseIf TypeOf dep Is clsProcessCredentialsDependency Then
                fn = Function(c) c.Type = PackageComponentType.Credential AndAlso
                 c.Name = CType(dep, clsProcessCredentialsDependency).RefCredentialsName
            ElseIf TypeOf dep Is clsProcessFontDependency Then
                fn = Function(c) c.Type = PackageComponentType.Font AndAlso
                 c.Name = CType(dep, clsProcessFontDependency).RefFontName
            ElseIf TypeOf dep Is clsProcessEnvironmentVarDependency Then
                fn = Function(c) c.Type = PackageComponentType.EnvironmentVariable AndAlso
                 c.Name = CType(dep, clsProcessEnvironmentVarDependency).RefVariableName
            ElseIf TypeOf dep Is clsProcessWebServiceDependency Then
                fn = Function(c) c.Type = PackageComponentType.WebService AndAlso
                 c.Name = CType(dep, clsProcessWebServiceDependency).RefServiceName
            ElseIf TypeOf dep Is clsProcessWebApiDependency Then
                fn = Function(c) c.Type = PackageComponentType.WebApi AndAlso
                 c.Name = CType(dep, clsProcessWebApiDependency).RefApiName
            ElseIf TypeOf dep Is clsProcessQueueDependency Then
                fn = Function(c) c.Type = PackageComponentType.Queue AndAlso
                 c.Name = CType(dep, clsProcessQueueDependency).RefQueueName
            ElseIf TypeOf dep Is clsProcessIDDependency Then
                fn = Function(c) c.Type = PackageComponentType.Process AndAlso
                 c.IdAsGuid = CType(dep, clsProcessIDDependency).RefProcessID
            ElseIf TypeOf dep Is clsProcessNameDependency Then
                fn = Function(c) c.Type = PackageComponentType.BusinessObject AndAlso
                 c.Name = CType(dep, clsProcessNameDependency).RefProcessName
            ElseIf TypeOf dep Is clsProcessParentDependency Then
                fn = Function(c) c.Type = PackageComponentType.BusinessObject AndAlso
                 c.Name = CType(dep, clsProcessParentDependency).RefParentName
            End If

            'Add dependency to selected/unselected list as appropriate
            If fn IsNot Nothing Then
                Dim c As PackageComponent = mOutputTree.Components.SingleOrDefault(fn)
                If c IsNot Nothing Then
                    'If it is in the output tree then already selected
                    If Not selectedComps.Contains(c) Then selectedComps.Add(c)
                Else
                    'Otherwise it must be in the input tree (unselected)
                    c = mInputTree.Components.SingleOrDefault(fn)
                    If c IsNot Nothing AndAlso Not unSelectedComps.Contains(c) Then unSelectedComps.Add(c)
                End If
            End If
        Next

        'Pass dependency lists to selection UI where user can choose to include/exclude
        Dim f As New frmSelectDependencies(comp, selectedComps, unSelectedComps)
        f.SetEnvironmentColoursFromAncestor(ParentForm)
        f.ShowInTaskbar = False
        If f.ShowDialog() <> DialogResult.OK Then Return
        mOutputTree.PutNodes(mInputTree.GetNodes(f.SelectedComponents))
        mInputTree.PutNodes(mOutputTree.GetNodes(f.UnSelectedComponents))
        f.Dispose()
    End Sub

    ''' <summary>
    ''' Iterates over a hierarchy of process/object dependencies.
    ''' </summary>
    Private Sub GetExternalDependencies(dependency As clsProcessDependency)

        Dim dependencyReference = dependency.GetValues.Values(0).ToString()
        Dim dependencyIterated = mAllDependencies.Any(Function(x) x.GetValues.Values(0).ToString() = dependencyReference)
        If dependencyIterated Then Return

        mAllDependencies.Add(dependency)

        If IsSelectedNode(dependency.GetFriendlyName, dependencyReference) OrElse
           (dependency.TypeName <> "ProcessNameDependency" AndAlso dependency.TypeName <> "ProcessIDDependency") Then Return

        Dim processId = GetProcessId(dependency)
        For Each childDependency In gSv.GetExternalDependencies(processId).Dependencies
            GetExternalDependencies(childDependency)
        Next

    End Sub

    ''' <summary>
    ''' Determines whether the current dependency is the same as the selected tree node.
    ''' </summary>
    Private Function IsSelectedNode(dependencyType As String, dependencyReference As String) As Boolean

        If dependencyType = "Process" Then
            Return dependencyReference = mSelectedNodeId
        Else
            Return dependencyReference = mSelectedNodeName
        End If

    End Function

    Private Function GetProcessId(dependency As clsProcessDependency) As Guid

        If dependency.TypeName = "ProcessNameDependency" Then
            Dim processName = CType(dependency, clsProcessNameDependency).RefProcessName
            Return gSv.GetProcessIDByName(processName, True)
        Else
            Return CType(dependency, clsProcessIDDependency).RefProcessID
        End If

    End Function

    ''' <summary>
    ''' Handles Expand All context menu option
    ''' </summary>
    Private Sub HandleExpandAll(sender As Object, e As EventArgs)
        Dim tree = CType(CType(sender, ToolStripItem).Owner.Tag, ctlComponentTree)
        tree.ExpandAll()
    End Sub

    ''' <summary>
    ''' Handles Collapse All context menu option
    ''' </summary>
    Private Sub HandleCollapseAll(sender As Object, e As EventArgs)
        Dim tree = CType(CType(sender, ToolStripItem).Owner.Tag, ctlComponentTree)
        tree.CollapseAll()
    End Sub

#End Region

End Class
