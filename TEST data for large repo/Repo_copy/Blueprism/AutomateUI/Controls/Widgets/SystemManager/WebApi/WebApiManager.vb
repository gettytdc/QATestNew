Imports System.Net.Http
Imports AutomateControls
Imports AutomateControls.Trees
Imports AutomateUI.Controls.Widgets.SystemManager.WebApi
Imports AutomateUI.Controls.Widgets.SystemManager.WebApi.Authentication
Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

''' <summary>
''' UI control to manage a Web API, this panel hosts all the sub-panels which look
''' after the other parts of the Web API.
''' </summary>
Friend NotInheritable Class WebApiManager : Implements IHelp

    ''' <summary>
    ''' Internal flag used to indicate what can be done to this control's tree view, 
    ''' given its current state
    ''' </summary>
    <Flags>
    Private Enum TreeViewState
        None = 0
        CanAddAction = 1
        CanDeleteAction = 2
    End Enum

    ''' <summary>
    ''' Internal delegate used to handle changes to this control's tree view state
    ''' </summary>
    Private Delegate Sub TreeViewStateChangedEventHandler(
    ByVal sender As Object, ByVal e As TreeViewStateChangedEventArgs)

    ''' <summary>
    ''' Provides data for the event fired when the tree view state changes
    ''' </summary>
    Private Class TreeViewStateChangedEventArgs
        ''' <summary>
        ''' The state that the tree view has changed to
        ''' </summary>
        Public ReadOnly NewState As TreeViewState

        ''' <summary>
        ''' Create a new instance of the event args data for the event fired when 
        ''' the tree view state changes
        ''' </summary>
        ''' <param name="state">The new state of the tree view</param>
        Public Sub New(state As TreeViewState)
            NewState = state
        End Sub

    End Class

#Region " Events "

    ''' <summary>
    ''' Event raised when the Web API name is in the process of being changed due
    ''' to a user modification
    ''' </summary>
    Friend Event WebApiNameChanging As NameChangingEventHandler

    ''' <summary>
    ''' Event raised when the Web API name has been changed due to user input
    ''' </summary>
    Friend Event WebApiNameChanged As NameChangedEventHandler

    ''' <summary>
    ''' Event raised when the state of the tree view changes
    ''' </summary>
    Private Event TreeViewStateChanged As TreeViewStateChangedEventHandler

#End Region

#Region " Member Variables "

    ' The root node in the filterable tree, present after a service has been set
    Private mRootNode As TreeNode

    ' The tree which is displaying the configuration nodes for the web api
    Private WithEvents mTree As FilterableTreeView

    ' The action summary panel, when it is active as the detail panel
    Private WithEvents mActionSummaryPanel As WebApiActionSummary

    ' The WebApi detail panel, when it is active as the detail panel
    Private WithEvents mWebApiDetailPanel As WebApiDetailPanel

    ' The Action panel, when it is active as the detail panel
    Private WithEvents mActionPanel As WebApiActionPanel

    ' The Action request panel, when it is active as the detail panel
    Private WithEvents mActionRequestPanel As WebApiActionRequestPanel

    ' The Authentication panel, when it is active as the detail panel
    Private WithEvents mAuthenticationPanel As WebApiAuthenticationPanel

    Private WithEvents mSelectedRequiresValidationPanel As IRequiresValidation

    ' The cache used primarily for fonts within the tree
    Private mCache As GDICache

    ' The current state of the tree view
    Private mTreeViewState As TreeViewState

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new, empty, WebApiManager control.
    ''' </summary>
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mCache = New GDICache()
        mTree = tvServices.Tree
        mTree.Font = mCache.GetFont(mTree.Font, FontStyle.Bold)

        ' Basic filterer for now; we may want to flesh this out when web apis are
        ' a bit more fleshed out
        tvServices.Filterer =
        Function(t, n) n.Text.Contains(t, StringComparison.CurrentCultureIgnoreCase)

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the ID of the service currently held in this manager control or
    ''' <see cref="Guid.Empty"/> if no service is currently set.
    ''' </summary>
    Friend ReadOnly Property ServiceId As Guid
        Get
            Return If(Service?.Id, Guid.Empty)
        End Get
    End Property

    ''' <summary>
    ''' Gets and sets the WebApi service associated with this manager control.
    ''' By setting the service to null, that indicates that the manager should
    ''' create a new service. If the control has already been and effectively shown,
    ''' it will create one immediately; otherwise it will wait until the control's
    ''' handle is created to do so.
    ''' </summary>
    <Browsable(False),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property Service As WebApiDetails
        Get
            Return DirectCast(mRootNode?.Tag, WebApiDetails)
        End Get
        Set(value As WebApiDetails)
            mTree.BeginUpdate()
            Try
                mTree.Nodes.Clear()
                mRootNode = Nothing
                DetailPanel = Nothing

                ' If null was passed we either want to do nothing (if the manager
                ' control has not yet been 'Load'ed) or generate a new service (if
                ' it has).
                If value Is Nothing AndAlso Not IsHandleCreated Then Return
                If value Is Nothing Then value = CreateNewService()

                mRootNode = mTree.Nodes.Add(value.Name)
                mRootNode.Tag = value

                AddNode(mRootNode.Nodes, WebApi_Resources.CommonHeadersNode,
                value.CommonHeaders)
                AddNode(mRootNode.Nodes, WebApi_Resources.CommonParametersNode,
                value.CommonParameters)

                If Licensing.License.CanUse(LicenseUse.Credentials) Then
                    AddNode(mRootNode.Nodes, WebApi_Resources.CommonAuthenticationsNode,
                 value.CommonAuthentication)
                End If

                AddNode(mRootNode.Nodes, WebApi_Resources.CommonCode,
                value.CommonCode)

                Dim actions As New Dictionary(Of String, WebApiActionDetails)(
                StringComparer.OrdinalIgnoreCase)
                Dim actionsNode =
                AddNode(mRootNode.Nodes, WebApi_Resources.ActionsNode, actions)

                For Each act In value.Actions
                    AddActionNode(actionsNode.Nodes, act)
                Next

                mTree.ExpandAll()
                mTree.SelectedNode = mRootNode

            Finally
                mTree.EndUpdate()

            End Try
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the detail panel within this WebApiManager control
    ''' </summary>
    <Browsable(False),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Private Property DetailPanel As Control
        Get
            Return splitDetail.Panel2.Controls.Cast(Of Control).FirstOrDefault()
        End Get
        Set(value As Control)
            With splitDetail.Panel2.Controls
                .Clear()
                If value IsNot Nothing Then
                    value.Dock = DockStyle.Fill
                    .Add(value)
                End If
            End With
        End Set
    End Property

    ''' <summary>
    ''' Gets the actions associated with the Web API currently being managed by this
    ''' control.
    ''' </summary>
    <Browsable(False),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Private ReadOnly Property Actions As IDictionary(Of String, WebApiActionDetails)
        Get
            Return DirectCast(
            ActionsNode?.Tag, IDictionary(Of String, WebApiActionDetails))
        End Get
    End Property

    ''' <summary>
    ''' Gets the 'Actions' node in the tree hosted by this manager, ie. the node
    ''' whose child nodes represent the actions in the web api being managed by this
    ''' control.
    ''' </summary>
    <Browsable(False),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Private ReadOnly Property ActionsNode As TreeNode
        Get
            ' This is at most 4 nodes in, so should be fairly fast
            Return mTree.FindNode(
            Function(n) TypeOf n.Tag Is IDictionary(Of String, WebApiActionDetails))
        End Get
    End Property


#End Region

#Region " Methods "

    ''' <summary>
    ''' Raises the <see cref="WebApiNameChanging"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Private Sub OnWebApiNameChanging(e As NameChangingEventArgs)
        RaiseEvent WebApiNameChanging(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="WebApiNameChanged"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Private Sub OnWebApiNameChanged(e As NameChangedEventArgs)
        RaiseEvent WebApiNameChanged(Me, e)
    End Sub

    ''' <summary>
    ''' Handles the loading of the control.
    ''' This ensures that a service is present to be managed by this control,
    ''' creating a new one if none has been set.
    ''' </summary>
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        If Service Is Nothing Then Service = CreateNewService()
    End Sub

    ''' <summary>
    ''' Creates a new service with a unique name, some default values and an initial
    ''' action.
    ''' </summary>
    ''' <returns>A new service with a unique name</returns>
    Private Function CreateNewService() As WebApiDetails
        ' Get a new API name using the name changing event
        Dim newApiName = BPUtil.FindUnique(
            WebApi_Resources.NewWebApiName_Template, True,
            Function(n)
                Dim ncea As New NameChangingEventArgs(Nothing, n)
                OnWebApiNameChanging(ncea)
                Return ncea.Cancel
            End Function
        )
        ' If we've settled on a name, ensure that interested parties are aware of it
        OnWebApiNameChanged(New NameChangedEventArgs(Nothing, newApiName))

        Return WebApiDetails.CreateNewInstanceWithAction(newApiName)
    End Function

    ''' <summary>
    ''' Adds and returns a node to the given node collection, ensuring that it is
    ''' set with the correct font.
    ''' </summary>
    ''' <param name="nodes">The node collection to add the new node to</param>
    ''' <param name="text">The text of the node to add</param>
    ''' <returns>The new node after being created and added to
    ''' <paramref name="nodes"/></returns>
    Private Function AddNode(nodes As TreeNodeCollection, text As String) As TreeNode
        Return AddNode(nodes, text, Nothing)
    End Function

    ''' <summary>
    ''' Adds and returns a node to the given node collection, ensuring that it is
    ''' set with the correct font and tagged with the specified object.
    ''' </summary>
    ''' <param name="nodes">The node collection to add the new node to</param>
    ''' <param name="text">The text of the node to add</param>
    ''' <param name="tag">The object to set into the tag of the new node</param>
    ''' <returns>The new node after being created and added to
    ''' <paramref name="nodes"/></returns>
    Private Function AddNode(
    nodes As TreeNodeCollection, text As String, tag As Object) As TreeNode
        With nodes.Add(text)
            .NodeFont = mCache.GetFont(mTree.Font, FontStyle.Regular)
            .Tag = tag
            Return .It()
        End With
    End Function

    ''' <summary>
    ''' Adds a node to represent a new action and any child nodes necessary to be
    ''' able to configure the new action fully. Note that this does no validation to
    ''' ensure that the given action can safely be added to the underlying Web API
    ''' and, indeed, it does not add it to the underlying Web API, though it does
    ''' add it to the <see cref="Actions"/> collection held in this control.
    ''' </summary>
    ''' <param name="newAction">The new action to add a node for</param>
    ''' <returns>The new node representing the given action</returns>
    Private Function AddActionNode(newAction As WebApiActionDetails) As TreeNode
        Return AddActionNode(ActionsNode.Nodes, newAction)
    End Function

    ''' <summary>
    ''' Adds a node to represent a new action and any child nodes necessary to be
    ''' able to configure the new action fully. Note that this does no validation to
    ''' ensure that the given action can safely be added to the underlying Web API
    ''' and, indeed, it does not add it to the underlying Web API, though it does
    ''' add it to the <see cref="Actions"/> collection held in this control.
    ''' </summary>
    ''' <param name="nodes">The node collection to which the node representing the
    ''' action should be added</param>
    ''' <param name="newAction">The new action to add a node for</param>
    ''' <returns>The new node representing the given action</returns>
    Private Function AddActionNode(
    nodes As TreeNodeCollection, newAction As WebApiActionDetails) As TreeNode
        Dim actions = Me.Actions
        actions(newAction.Name) = newAction
        Dim newNode = AddNode(nodes, newAction.Name, newAction)
        AddNode(newNode.Nodes, WebApi_Resources.ParametersNode, newAction.Parameters)

        Dim requestNode = AddNode(newNode.Nodes, WebApi_Resources.RequestNode, newAction.Request)
        AddNode(requestNode.Nodes, WebApi_Resources.HeadersNode, newAction.Request.Headers)

        Dim responseNode = AddNode(newNode.Nodes, WebApi_Resources.ResponseNode, newAction.Response)
        Return newNode
    End Function

    ''' <summary>
    ''' Handles the name of the Web API changing on the detail control, ensuring that
    ''' a valid, unique, name is chosen by passing the event on through the
    ''' <see cref="WebApiNameChanging"/> event to a listener with the context to be
    ''' able to make such checks.
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="e">The args detailing the event</param>
    Private Sub HandleWebApiNameChanging(sender As Object, e As NameChangingEventArgs) _
    Handles mWebApiDetailPanel.NameChanging
        OnWebApiNameChanging(e)
    End Sub

    ''' <summary>
    ''' Handles the Web API name being changed on the Web API detail panel
    ''' </summary>
    Private Sub HandleWebApiNameChanged(sender As Object, e As NameChangedEventArgs) _
    Handles mWebApiDetailPanel.NameChanged
        Service.Name = e.NewName
        mRootNode.Text = e.NewName
        OnWebApiNameChanged(e)
    End Sub

    ''' <summary>
    ''' Handles the name of an action changing on the detail control, ensuring that
    ''' a valid, unique, name is chosen.
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="e">The args detailing the event</param>
    Private Sub HandleActionNameChanging(sender As Object, e As NameChangingEventArgs) _
    Handles mActionPanel.NameChanging
        If String.IsNullOrEmpty(e.NewName.Trim()) Then
            e.CancelReason = String.Format(WebApi_Resources.ErrorActionNameCannotBeEmpty)
            e.Cancel = True
            Return
        End If

        If Actions.Any(Function(a) Not HasSameActionName(a.Key, e.OldName) _
                                AndAlso HasSameActionName(a.Key, e.NewName)) Then
            e.CancelReason = String.Format(WebApi_Resources.ActionNameExistsMessage_Template, e.NewName)
            e.Cancel = True
        End If
    End Sub

    Private Function HasSameActionName(name As String, otherName As String) As Boolean
        Return String.Equals(CodeCompiler.GetIdentifier(name.Trim()),
                             CodeCompiler.GetIdentifier(otherName.Trim()), StringComparison.CurrentCultureIgnoreCase)
    End Function

    ''' <summary>
    ''' Handles an action name being changed in the action detail panel
    ''' </summary>
    Private Sub HandleActionNameChanged(sender As Object, e As NameChangedEventArgs) _
    Handles mActionPanel.NameChanged
        Dim action = mActionPanel.Action
        Actions.Remove(e.OldName)
        Actions.Add(e.NewName, action)
        mTree.SelectedNode.Text = e.NewName
    End Sub

    ''' <summary>
    ''' Handles an action being activated (typically double-clicked) in the action
    ''' summary panel.
    ''' </summary>
    Private Sub HandleActionActivated(sender As Object, e As EventArgs) _
    Handles mActionSummaryPanel.ActionActivated
        Dim act = mActionSummaryPanel?.SelectedAction
        If act Is Nothing Then Return
        For Each n As TreeNode In mTree.SelectedNode.Nodes
            If n.Text = act.Name Then mTree.SelectedNode = n
        Next
    End Sub

    ''' <summary>
    ''' Handles the authentication type changing on the authentication panel.
    ''' </summary>
    Private Sub HandleAuthenticationTypeChanged(sender As Object, e As AuthenticationChangedEventArgs) _
    Handles mAuthenticationPanel.AuthenticationTypeChanged
        Dim newType = e.Authentication.Type
        lblGuidance.Text = mAuthenticationPanel.GuidanceText
    End Sub

    ''' <summary>
    ''' Sets the detail panel to the specified type, and then populates the panel
    ''' using a specified callback.
    ''' </summary>
    ''' <typeparam name="TPanel">The type of panel that needs its detail set. If this
    ''' corresponds to the currently selected <see cref="DetailPanel"/>, then that is
    ''' what is used, otherwise a new instance of <typeparamref name="TPanel"/> is
    ''' created to set as the detail panel and to populate.</typeparam>
    ''' <param name="setDataOn">Delegate used to set the data on the panel.</param>
    ''' <returns>The panel of the specified type; either the </returns>
    Private Function SetDetail(Of TPanel As {Control, New})(setDataOn As Action(Of TPanel)) As TPanel
        If TypeOf DetailPanel IsNot TPanel Then DetailPanel = New TPanel()
        Dim pan As TPanel = DirectCast(DetailPanel, TPanel)
        setDataOn(pan)
        lblGuidance.Text = TryCast(pan, IGuidanceProvider)?.GuidanceText
        Return pan
    End Function


    Private Sub HandleNodeSelection(sender As Object, e As TreeViewCancelEventArgs) _
    Handles mTree.BeforeSelect

        Try
            ValidateSelectedPanel()
        Catch ex As Exception
            e.Cancel = True
            UserMessage.Err(ex.Message)
        End Try
    End Sub

    Private Sub ValidateSelectedPanel()
        If mSelectedRequiresValidationPanel IsNot Nothing Then _
            mSelectedRequiresValidationPanel.Validate()
    End Sub

    ''' <summary>
    ''' Handles a node being selected, ensuring that the appropriate detail screen
    ''' is populated and displayed.
    ''' </summary>
    Private Sub HandleNodeSelected(sender As Object, e As TreeViewEventArgs) _
    Handles mTree.AfterSelect

        UpdateTreeViewState()

        Dim n = e.Node

        Debug.Assert(n IsNot Nothing)
        If n Is Nothing Then DetailPanel = Nothing : Return

        Select Case True
            Case TypeOf n.Tag Is WebApiDetails
                Dim service = DirectCast(n.Tag, WebApiDetails)
                mWebApiDetailPanel = SetDetail(Of WebApiDetailPanel)(
                Sub(p) p.Service = service)

            Case TypeOf n.Tag Is IDictionary(Of String, WebApiActionDetails)
                mActionSummaryPanel = SetDetail(Of WebApiActionSummary)(
                Sub(p) p.Actions = Actions.Values)

            Case TypeOf n.Tag Is WebApiActionDetails
                Dim action = DirectCast(n.Tag, WebApiActionDetails)
                mActionPanel = SetDetail(Of WebApiActionPanel)(
                Sub(p) p.Action = action)

            Case TypeOf n.Tag Is WebApiActionRequestDetails
                Dim request = DirectCast(n.Tag, WebApiActionRequestDetails)
                mActionRequestPanel = SetDetail(Of WebApiActionRequestPanel)(
                Sub(p) p.Request = request)

            Case TypeOf n.Tag Is WebApiActionResponseDetails
                Dim response = DirectCast(n.Tag, WebApiActionResponseDetails)
                mSelectedRequiresValidationPanel = SetDetail(Of WebApiActionResponsePanel)(
                Sub(p) p.Response = response)

            Case TypeOf n.Tag Is WebApiCollection(Of ActionParameter)
                Dim params = DirectCast(n.Tag, WebApiCollection(Of ActionParameter))
                mSelectedRequiresValidationPanel = SetDetail(Of WebApiParametersPanel)(
                Sub(p) p.Parameters = params)

            Case TypeOf n.Tag Is WebApiCollection(Of HttpHeader)
                Dim headers = DirectCast(n.Tag, WebApiCollection(Of HttpHeader))
                SetDetail(Of HttpHeaderPanel)(Sub(p) p.Headers = headers)

            Case TypeOf n.Tag Is AuthenticationWrapper
                Dim authentication = DirectCast(n.Tag, AuthenticationWrapper)
                mAuthenticationPanel = SetDetail(Of WebApiAuthenticationPanel)(
                Sub(p) p.Authentication = authentication)

            Case TypeOf n.Tag Is CodePropertiesDetails
                Dim commonCodeProperties = DirectCast(n.Tag, CodePropertiesDetails)
                SetDetail(Of WebApiCommonCodePanel)(
                Sub(p) p.CommonCode = commonCodeProperties)

            Case Else
                DetailPanel = Nothing
        End Select

    End Sub

    ''' <summary>
    ''' Disposes of this manager controls
    ''' </summary>
    ''' <param name="disposing">True to indicate explicit disposal of the control;
    ''' False to indicate disposal on object finalisation.</param>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing Then
                If components IsNot Nothing Then components.Dispose()
                If mCache IsNot Nothing Then mCache.Dispose() : mCache = Nothing
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    ''' <summary>
    ''' Handles a filter being applied in the tree and updates the tree view's state
    ''' appropriately
    ''' </summary>
    Private Sub HandleFilterApplied(sender As Object, e As EventArgs) _
    Handles mTree.FilterApplied

        Try
            ValidateSelectedPanel()
        Catch ex As Exception
            mTree.ClearFilter()
            UserMessage.Err(ex.Message)
        End Try

        UpdateTreeViewState()
    End Sub

    ''' <summary>
    ''' Handles the filter being cleared and updates the tree view's state
    ''' appropriately
    ''' </summary>
    Private Sub HandleFilterCleared(sender As Object, e As EventArgs) _
    Handles mTree.FilterCleared
        UpdateTreeViewState()
    End Sub

    ''' <summary>
    ''' Handles a node being right clicked on the tree view, and ensuring that this
    ''' node is selected so that any context menu actions that are called occur on
    ''' the node the menu was opened for.
    ''' </summary>
    Private Sub HandleNodeRightClick(
    sender As Object, e As TreeNodeRightClickEventArgs) Handles mTree.RightClickNode
        If e.Node IsNot Nothing Then mTree.SelectedNode = e.Node
    End Sub

    ''' <summary>
    ''' Handles a new action being requested from the context menu or from clicking
    ''' the new action button.
    ''' </summary>
    Private Sub HandleNewAction(sender As Object, e As EventArgs) _
    Handles menuAddAction.Click, btnAddAction.Click
        Dim newAction As New WebApiActionDetails(Service) With {
        .Name = BPUtil.FindUnique(
            WebApi_Resources.NewActionName_Template, True,
            Function(n) Actions.ContainsKey(n)),
        .Enabled = True
    }

        newAction.Request.Method = HttpMethod.Get
        newAction.Request.UrlPath = String.Empty

        Service.Actions.Add(newAction)
        Dim newNode = AddActionNode(newAction)
        newNode.ExpandAll()
        mTree.SelectedNode = newNode

    End Sub

    ''' <summary>
    ''' Handles an action being deleted using the context menu or the delete action
    ''' button.
    ''' </summary>
    Private Sub HandleDeleteAction(sender As Object, e As EventArgs) _
    Handles menuDeleteAction.Click, btnDeleteAction.Click
        Dim n = mTree.SelectedNode
        If n Is Nothing Then Return

        ' If it has no children then it's not the action node
        If n.Nodes.Count = 0 Then n = n.Parent
        Debug.Assert(n IsNot Nothing AndAlso TypeOf n.Tag Is WebApiActionDetails)

        If n Is Nothing Then Return

        Dim act = TryCast(n.Tag, WebApiActionDetails)
        If act Is Nothing Then Return

        If UserMessage.YesNo(WebApi_Resources.DeleteActionPrompt_Template, act.Name) <>
        MsgBoxResult.Yes Then Return

        Actions.Remove(act.Name)
        n.Remove()
        Service.Actions.Remove(act)

    End Sub

    ''' <summary>
    ''' Method that updates the tree view state
    ''' </summary>
    Private Sub UpdateTreeViewState()
        mTreeViewState = GetTreeViewState()
        OnTreeViewStateChanged()
    End Sub

    ''' <summary>
    ''' Gets the current state of the tree view based on which node is selected and
    ''' whether a filter has been applied
    ''' </summary>
    ''' <returns>The current state of this control's tree view</returns>
    Private Function GetTreeViewState() As TreeViewState
        Dim state = TreeViewState.None

        If mTree.IsFiltered Then
            Return state
        Else
            state = state.SetFlags(TreeViewState.CanAddAction)
        End If

        If TypeOf mTree.SelectedNode?.Tag Is WebApiActionDetails OrElse
        TypeOf mTree.SelectedNode?.Parent?.Tag Is WebApiActionDetails Then _
            state = state.SetFlags(TreeViewState.CanDeleteAction)

        Return state

    End Function

    ''' <summary>
    ''' Method that raises the <see cref="TreeViewStateChanged"/> event
    ''' </summary>
    Private Sub OnTreeViewStateChanged()
        RaiseEvent TreeViewStateChanged(Me, New TreeViewStateChangedEventArgs(mTreeViewState))
    End Sub

    ''' <summary>
    ''' Handles the state changing of this control's tree view and enabling/disabling
    ''' buttons and context menu items accordingly
    ''' </summary>
    Private Sub HandlesTreeViewStateChanged(sender As Object, e As TreeViewStateChangedEventArgs) _
    Handles Me.TreeViewStateChanged

        Dim canAddAction = e.NewState.HasFlag(TreeViewState.CanAddAction)
        Dim canDeleteAction = e.NewState.HasFlag(TreeViewState.CanDeleteAction)

        btnAddAction.Enabled = canAddAction
        menuAddAction.Enabled = canAddAction

        btnDeleteAction.Enabled = canDeleteAction
        menuDeleteAction.Enabled = canDeleteAction

        ctxActions.Enabled = canDeleteAction OrElse canAddAction
    End Sub


    ''' <summary>
    ''' Ensures that the web API service set in this manager is valid.
    ''' </summary>
    ''' <remarks>This is quite a naive, slightly inefficient, implementation in that
    ''' it only tries to convert the value in this manager to a <see cref="WebApi"/>
    ''' instance to see if there are any errors. I'm sure there are better ways.
    ''' </remarks>
    ''' <exception cref="Exception">If any errors occur indicating a problem with the
    ''' data in this manager.</exception>
    Friend Sub EnsureServiceValid()

        ValidateSelectedPanel()

        Dim container = TryCast(DetailPanel, ContainerControl)

        If container IsNot Nothing AndAlso Not container.Validate() Then _
        Throw New InvalidStateException(WebApi_Resources.ErrorValidatingWebApi)

        ' If there are further errors, this should force them
        Dim api = CType(Service, WebApi)

    End Sub

    ''' <summary>
    ''' Handles the resizing of the detail panel, ensuring that the maximum size of
    ''' the guidance label is set (which forces the rendering to wrap text at the
    ''' 'maximum width' boundary).
    ''' </summary>
    Private Sub HandleDetailResize(sender As Object, e As EventArgs) _
    Handles splitDetail.Resize
        lblGuidance.MaximumSize = New Size(splitDetail.Panel1.Width, 0)
    End Sub

    ''' <summary>
    ''' Handles the resizing of the guidance label, ensuring that the detail splitter
    ''' is updated to match the content of the label
    ''' </summary>
    Private Sub HandleLabelResize(sender As Object, e As EventArgs) _
    Handles lblGuidance.Resize
        splitDetail.SplitterDistance =
        lblGuidance.Margin.Vertical + lblGuidance.Height

    End Sub

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "Web API/HTML/web-api.htm"
    End Function

#End Region

End Class
