
Imports System.Xml

Imports AutomateControls

Imports BluePrism.AMI
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateProcessCore

Imports TreeViewCancelEditEventArgs = AutomateUI.clsReorderableTreeview.TreeViewCancelEditEventArgs
Imports TreeViewEditEventArgs = AutomateUI.clsReorderableTreeview.TreeViewEditEventArgs
Imports BluePrism.Core.Xml
Imports BluePrism.Server.Domain.Models

''' Project  : Automate
''' Class    : ctlApplicationExplorer
''' 
''' <summary>
''' A control that allows the application definition to be viewed and edited
''' in a treeview. The property Readonly determines if user edits are allowed
''' in the tree.
''' 
''' Edits made to the tree are updated live in the application definition,
''' so you should clone your definition if you want to wait before commiting
''' changes made.
''' </summary>
Public Class ctlApplicationExplorer

#Region " Class Scope Definitions "

    ''' <summary>
    ''' Event fired when a node representing an application element has been
    ''' selected in this application explorer
    ''' </summary>
    ''' <param name="sender">The application explorer instance on which a node has
    ''' been selected.</param>
    ''' <param name="e">The args detailing the event.</param>
    Public Event MemberSelected(ByVal sender As Object, ByVal e As ApplicationMemberEventArgs)

    ''' <summary>
    ''' Raised whenever a new node is added.
    ''' </summary>
    ''' <param name="member">The newly added application member.</param>
    Public Event MemberAdded(ByVal member As clsApplicationMember)

    ''' <summary>
    ''' Raised whenever a node is deleted.
    ''' </summary>
    ''' <param name="member">The deleted member.</param>
    Public Event MemberDeleted(ByVal member As clsApplicationMember)

    ''' <summary>
    ''' Raised whenever references to the selected element are requested.
    ''' </summary>
    ''' <param name="el">The element to find references to.</param>
    Public Event FindReferences(el As clsApplicationElement)

    ''' <summary>
    ''' The prefix to use for new DDE Elements.
    ''' </summary>
    Private Const DDEPrefix As String = "DDE Element"

    ' The ID of the last element that was selected in *any* application explorer control
    Private Shared mLastSelectedElement As Guid

    ''' <summary>
    ''' Class providing a delegate for searching the tree for members with a matching
    ''' ID. Used in GetNode(Guid)
    ''' </summary>
    Private Class IdMatcher
        ' The ID to match
        Private mId As Guid
        ' Creates a new IdMatcher for the given ID
        Public Sub New(ByVal id As Guid)
            mId = id
        End Sub
        ' Checks if the given member has the same ID as this object
        Public Function IsMatch(ByVal member As clsApplicationMember) As Boolean
            Return member IsNot Nothing AndAlso member.ID = mId
        End Function
    End Class

    ''' <summary>
    ''' Treenode which represents an application member.
    ''' This provides a lot of the mechanisms for handling the relationships
    ''' between parent and child application members.
    ''' </summary>
    Private Class ApplicationMemberNode : Inherits TreeNode

        ' The member that this node represents.
        Private mMember As clsApplicationMember

        ' The owner of this node
        Private mOwner As ctlApplicationExplorer

        ''' <summary>
        ''' Constructor here only for the Clone() method to work correctly
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Creates a new ApplicationMemberNode within the specified application
        ''' explorer, and representing the given application member.
        ''' </summary>
        ''' <param name="owner">The owning application explorer control</param>
        ''' <param name="member">The member that this node should represent.</param>
        Public Sub New(ByVal owner As ctlApplicationExplorer, ByVal member As clsApplicationMember)
            MyBase.New(member.Name)
            mOwner = owner
            mMember = member
            Tag = member

            UpdateAppearance()

            For Each child As clsApplicationMember In member.ChildMembers
                Nodes.Add(New ApplicationMemberNode(owner, child))
            Next

        End Sub

        ''' <summary>
        ''' Handles the cloning of this node, ensuring that the owner and
        ''' member are transferred to the clone (note that these are shallow
        ''' cloned - ie. both this node and the source node refer to the same
        ''' objects in memory)
        ''' </summary>
        ''' <returns>The cloned application member node</returns>
        Public Overrides Function Clone() As Object
            Dim copy As ApplicationMemberNode = DirectCast(MyBase.Clone(), ApplicationMemberNode)
            copy.mOwner = mOwner
            copy.mMember = mMember
            copy.NodeFont = MemberFont
            If IsExpanded() Then copy.Expand()
            Return copy
        End Function

        ''' <summary>
        ''' Updates the font, colour and tooltip for this node, based on the backing
        ''' application member data.
        ''' </summary>
        Public Sub UpdateAppearance()
            ' Set the font and colour of this node
            NodeFont = MemberFont
            ForeColor = MemberColor

            ' Add a description tooltip
            If Element IsNot Nothing Then ToolTipText = Element.Description

        End Sub

        ''' <summary>
        ''' Updates the parent member using the data set in this node - ie. it sets
        ''' the backing data to the value currently set in the node.
        ''' </summary>
        Public Sub UpdateParentMember()
            Dim regMem As clsApplicationRegion =
             TryCast(mMember, clsApplicationRegion)

            Dim parentNode As ApplicationMemberNode = Parent
            If parentNode IsNot Nothing Then
                ' The act of inserting the member in the parent member (from the
                ' parent node will remove it from its existing parent member
                parentNode.Member.InsertMember(Index, mMember)

            Else
                ' We have no parent node - in effect, this member no long has a
                ' parent. Make it so.
                mMember.RemoveFromParent()
            End If
        End Sub

        ''' <summary>
        ''' Removes the member that this node represents from its region container.
        ''' This only works if the member is an application region, and its container
        ''' could be found in the same application model that this member is within
        ''' </summary>
        Public Sub RemoveFromContainer()
            Dim reg As clsApplicationRegion = TryCast(mMember, clsApplicationRegion)
            If reg Is Nothing Then Return

            ' Get the container that we want to remove this region from
            Dim cont As clsRegionContainer = reg.Container
            If cont Is Nothing Then Return

            cont.Regions.Remove(reg)

        End Sub

        ''' <summary>
        ''' Replaces the current member in this node with a copy, updating the member
        ''' relationships with those found in this treenode. This should be called
        ''' if a treenode is copied to ensure that the member in the node correctly
        ''' matches up to the structure in the tree.
        ''' </summary>
        ''' <returns>The newly copied member</returns>
        Public Function ReplaceMemberWithCopy() As clsApplicationMember
            If mMember Is Nothing Then Return Nothing ' Nothing to do...

            ' Ensure that we have the correct owner - ie. that of the target
            ' treeview, not that of the source
            Dim tgtOwner As ctlApplicationExplorer =
             UIUtil.GetAncestor(Of ctlApplicationExplorer)(Me.TreeView)
            Dim sameOwner As Boolean = (tgtOwner Is mOwner)
            mOwner = tgtOwner

            'When copying to and from the same application model we create new IDs
            If sameOwner Then
                ReplaceMember(mMember.Copy(), False)
                Return mMember
            End If

            ' We need to search the root of the target model for conflicts -
            ' since this node's current member still reflects the source model,
            ' we must check against the target treeviews root member.
            Dim targetRoot As clsApplicationMember =
             DirectCast(Me.TreeView.Nodes(0), ApplicationMemberNode).Member

            Dim conflicts As ICollection(Of clsApplicationMember) = targetRoot.FindConflicts(mMember)
            Dim idfilter As New Predicate(Of clsApplicationMember)(AddressOf conflicts.Contains)
            If conflicts.Count > 0 Then
                Select Case ShowConflictMessage(conflicts)
                    Case DialogResult.Yes
                        For Each conflict As clsApplicationMember In conflicts
                            conflict.ID = Guid.NewGuid()
                        Next
                        ReplaceMember(DirectCast(mMember.Clone, clsApplicationMember), False)
                        Return mMember
                    Case DialogResult.No
                        ReplaceMember(mMember.ConstrainedClone(idfilter), False)
                        Return mMember
                    Case Else
                        mOwner.DeleteMember(mMember)
                        Return Nothing
                End Select
            End If
            ReplaceMember(mMember.ConstrainedClone(idfilter), False)
            Return mMember
        End Function


        Public Function ShowConflictMessage(ByVal conflicts As ICollection(Of clsApplicationMember)) As DialogResult
            Dim message As String =
            My.Resources.ApplicationMemberNode_TheFollowingElementsAlreadyExistInTheApplicationModel & vbCrLf & vbCrLf
            Dim lines As Integer = 0
            For Each conflict As clsApplicationMember In conflicts
                Dim conflictName As String = conflict.Name
                Dim pastedMember As clsApplicationMember = mMember.FindMember(conflict.ID)
                Dim pastedName As String = String.Empty
                If pastedMember IsNot Nothing Then
                    pastedName = pastedMember.Name
                End If

                message &= vbTab & pastedName
                If conflictName <> pastedName Then
                    message &= String.Format(My.Resources.ApplicationMemberNode_ConflictName0, conflictName)
                End If
                message &= vbCrLf
                lines += 1
                If lines >= 5 Then
                    message &= vbTab & String.Format(My.Resources.ApplicationMemberNode_And0Others, conflicts.Count - 5) & vbCrLf
                    Exit For
                End If
            Next

            message &= vbCrLf & My.Resources.ApplicationMemberNode_DoYouWantToUpdateYourObjectToReferenceTheseNewElementsInstead

            Return MessageBox.Show(message, My.Resources.ApplicationMemberNode_ConflictingElementsFound, MessageBoxButtons.YesNoCancel)
        End Function


        ''' <summary>
        ''' Recursively updates the member assignments in this node and all its
        ''' child nodes to the given member (and all its child members).
        ''' Also updates the parent of the given member to the current parent of
        ''' this node - effectively, this replaces the current member with the new
        ''' member given.
        ''' </summary>
        ''' <param name="newMember">The application member to set in this node,
        ''' and whose descendent members should be set within this node's
        ''' descendents.</param>
        ''' <remarks>Note that if there is a disparity in structure between this
        ''' node's descendents and the application member's descendents, exceptions
        ''' may occur.</remarks>
        Public Sub ReplaceMember(ByVal newMember As clsApplicationMember)
            ' We usually want to update the old member for the sake of completion
            ReplaceMember(newMember, True)
        End Sub

        ''' <summary>
        ''' Recursively updates the member assignments in this node and all its
        ''' child nodes to the given member (and all its child members).
        ''' Also updates the parent of the given member to the current parent of
        ''' this node - effectively, this replaces the current member with the new
        ''' member given.
        ''' </summary>
        ''' <param name="newMember">The application member to set in this node,
        ''' and whose descendent members should be set within this node's
        ''' descendents.</param>
        ''' <param name="updateOldMember">True to update the parent of the old member
        ''' from this node; False to leave the old member alone.</param>
        ''' <remarks>Note that if there is a disparity in structure between this
        ''' node's descendents and the application member's descendents, exceptions
        ''' may occur.</remarks>
        Private Sub ReplaceMember(
         ByVal newMember As clsApplicationMember, ByVal updateOldMember As Boolean)
            ' If we had a previous member, ensure that it is removed from the
            ' parent member or we'll end up saving dupes (unless directed otherwise)
            If updateOldMember _
             AndAlso mMember IsNot Nothing AndAlso mMember IsNot newMember Then
                mMember.RemoveFromParent()
            End If
            InnerReplaceMember(newMember)
            UpdateParentMember()
            UpdateAppearance()
        End Sub

        ''' <summary>
        ''' Inner implementation of the ReplaceMember method which walks the tree
        ''' ensuring that all descendent nodes are replaced too.
        ''' </summary>
        ''' <param name="newMember">The new member to replace in this node, and whose
        ''' descendents should replace the members in the descendent nodes.</param>
        Private Sub InnerReplaceMember(ByVal newMember As clsApplicationMember)
            mMember = newMember
            Tag = newMember

            ' We now need to update our child nodes with the member children
            If mMember IsNot Nothing Then
                Dim childrenEnum As IEnumerator(Of clsApplicationMember) =
                 mMember.ChildMembers.GetEnumerator()

                For Each n As ApplicationMemberNode In Me.Nodes
                    If Not childrenEnum.MoveNext Then Throw New BluePrismException(
                     My.Resources.ApplicationMemberNode_TreeMismatchFoundWhenUpdatingTreeMembersForNode0, Name)
                    n.InnerReplaceMember(childrenEnum.Current)
                Next
            End If

        End Sub

        ''' <summary>
        ''' Toggles the diagnostics setting for this node. 
        ''' </summary>
        ''' <seealso cref="DiagnosticsEnabled"/>
        Public Sub ToggleDiagnostics()
            DiagnosticsEnabled = Not DiagnosticsEnabled
        End Sub

        ''' <summary>
        ''' Gets or sets whether diagnostics are enabled for this node (and, by
        ''' inference, for the application element represented by this node).
        ''' This has no effect (and will always return false) if this node does
        ''' not represent an application element.
        ''' </summary>
        Public Property DiagnosticsEnabled() As Boolean
            Get
                Return Element IsNot Nothing AndAlso Checked
            End Get
            Set(ByVal value As Boolean)
                Dim el As clsApplicationElement = Element
                If el IsNot Nothing Then
                    'GB: we use n.checked to conveniently hold the state of the flag
                    ' however oddly this does not affect the appearance
                    'PW: If you want to see the checkbox item beside the
                    ' node then you have to set the "CheckBoxes" property
                    ' on the parent treeview to True

                    Checked = value
                    el.Diagnose = value
                    UpdateAppearance()
                End If
            End Set
        End Property

        ''' <summary>
        ''' The parent node of this node, as an ApplicationMemberNode
        ''' </summary>
        Public Overloads ReadOnly Property Parent() As ApplicationMemberNode
            Get
                Return TryCast(MyBase.Parent, ApplicationMemberNode)
            End Get
        End Property

        ''' <summary>
        ''' The application member that this node represents.
        ''' </summary>
        Public ReadOnly Property Member() As clsApplicationMember
            Get
                Return mMember
            End Get
        End Property

        ''' <summary>
        ''' The application element that this node represents, or null if it does not
        ''' represent an application element.
        ''' </summary>
        Public ReadOnly Property Element() As clsApplicationElement
            Get
                Return TryCast(mMember, clsApplicationElement)
            End Get
        End Property

        ''' <summary>
        ''' The element group that this node represents, or null if it does not
        ''' represent an element group.
        ''' </summary>
        Public ReadOnly Property ElementGroup() As clsApplicationElementGroup
            Get
                Return TryCast(mMember, clsApplicationElementGroup)
            End Get
        End Property

        ''' <summary>
        ''' The font to use for this node.
        ''' </summary>
        Public ReadOnly Property MemberFont() As Font
            Get
                If mMember.ChildMembers.Count = 0 Then Return mOwner.RegularNodeFont
                Return mOwner.tvAppModel.Font
            End Get
        End Property

        ''' <summary>
        ''' The color to use for this node.
        ''' </summary>
        Public ReadOnly Property MemberColor() As Color
            Get
                If ElementGroup IsNot Nothing Then Return Color.DarkGray

                Dim el As clsApplicationElement = Element
                If el IsNot Nothing AndAlso el.Diagnose() Then Return Color.Gold
                Return mOwner.tvAppModel.ForeColor
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Class providing a delegate with which the application member on a treenode
    ''' can be replaced. This would be far better done with an anonymous method
    ''' </summary>
    Private Class MemberReplacer
        Private mMember As clsApplicationMember
        Public Sub New(ByVal member As clsApplicationMember)
            mMember = member
        End Sub
        Public Sub ReplaceMemberAction(ByVal node As TreeNode)
            Dim n As ApplicationMemberNode = TryCast(node, ApplicationMemberNode)
            If n Is Nothing Then Throw New InvalidOperationException(
             My.Resources.MemberReplacer_CannotReplaceTheAppMemberOnThisTreenode)
            n.ReplaceMember(mMember)
        End Sub
    End Class

#End Region

#Region " Member Variables "

    ' The regular font to use for nodes with no children
    Private mRegularFont As Font

    ' The application definition being modelled in this explorer control
    Private mAppDefn As clsApplicationDefinition

    ' The current element mask node which can be pasted onto other elements.
    Private mElementMask As ApplicationMemberNode

    ' Flag indicating if this application explorer is currently in a filter operation
    Private mIsFiltering As Boolean

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new application explorer control
    ''' </summary>
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlApplicationExplorer))
        txtFilter.GuidanceText = resources.GetString("txtFilter.Text")

        ' Add any initialization after the InitializeComponent() call.
        UpdateButtons()
        tvAppModel.CollapseAllImplementation = AddressOf CollapseAll

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the selected member id.
    ''' </summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedMemberId() As Guid
        Get
            Dim mem As clsApplicationMember = SelectedMember
            If mem IsNot Nothing Then Return mem.ID Else Return Guid.Empty
        End Get
        Set(ByVal value As Guid)
            Dim n As ApplicationMemberNode = GetNode(value)
            If n IsNot Nothing Then SelectedNode = n
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the selected application element
    ''' </summary>
    ''' <value>The application element to select. If the given value is null, the
    ''' selected node is set to null; if the value does not appear in the app model
    ''' tree, setting it has no effect. Otherwise, the node representing the given
    ''' application element is selected.</value>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedElement() As clsApplicationElement
        Get
            Return GetApplicationElement(tvAppModel.SelectedNode)
        End Get
        Set(ByVal value As clsApplicationElement)
            ' Just delegate to the SelectedMember property since an
            ' ApplicationElement is an ApplicationMember by inheritance anyway
            SelectedMember = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the selected application member
    ''' </summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedMember() As clsApplicationMember
        Get
            Return GetApplicationMember(tvAppModel.SelectedNode)
        End Get
        Set(ByVal value As clsApplicationMember)
            Dim n As ApplicationMemberNode = GetNode(value)
            If n IsNot Nothing Then SelectedNode = n
        End Set
    End Property

    ''' <summary>
    ''' The selected node in the tree as an application member node.
    ''' </summary>
    Private Property SelectedNode() As ApplicationMemberNode
        Get
            Return DirectCast(tvAppModel.SelectedNode, ApplicationMemberNode)
        End Get
        Set(ByVal value As ApplicationMemberNode)
            tvAppModel.SelectedNode = value
            If value IsNot Nothing Then value.EnsureVisible()
        End Set
    End Property

    ''' <summary>
    ''' Determines whether the user should be able to edit the tree.
    ''' 
    ''' When set to false, three buttons to "Add node", "Add child node"
    ''' and "Manage tree" will be provided.
    ''' 
    ''' False by default.
    ''' </summary>
    Public Property [ReadOnly]() As Boolean
        Get
            Return Not tvAppModel.AllowReordering
        End Get
        Set(ByVal value As Boolean)
            tvAppModel.AllowReordering = Not value
            panButtons.Visible = Not value
        End Set
    End Property

    ''' <summary>
    ''' The Regular font to use for nodes in the treeview.
    ''' </summary>
    Private ReadOnly Property RegularNodeFont() As Font
        Get
            If mRegularFont Is Nothing Then _
             mRegularFont = New Font(tvAppModel.Font, tvAppModel.Font.Style And Not FontStyle.Bold)

            Return mRegularFont
        End Get
    End Property


    ''' <summary>
    ''' Gets a view of all the nodes in this application explorer in a flat
    ''' collection
    ''' </summary>
    Private ReadOnly Property AllNodes() As ICollection(Of TreeNode)
        Get
            Return tvAppModel.FindNodes(AddressOf Yes)
        End Get
    End Property

    ''' <summary>
    ''' Gets a disconnected collection of the names of all the application members in
    ''' this application explorer.
    ''' </summary>
    Private ReadOnly Property AllNames() As ICollection(Of String)
        Get
            Dim names As New clsSet(Of String)
            For Each n As TreeNode In AllNodes
                names.Add(n.Text)
            Next
            Return names
        End Get
    End Property

#End Region

#Region " Internal Events "

    ''' <summary>
    ''' Raises the <see cref="MemberSelected"/> event.
    ''' </summary>
    ''' <param name="e">The args detailing the event.</param>
    Protected Overridable Sub OnMemberSelected(ByVal e As ApplicationMemberEventArgs)
        RaiseEvent MemberSelected(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="MemberAdded"/> event.
    ''' </summary>
    ''' <param name="n">The node which has been added</param>
    Protected Overridable Sub OnMemberAdded(ByVal n As TreeNode)
        RaiseEvent MemberAdded(DirectCast(n, ApplicationMemberNode).Member)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="MemberDeleted"/> event.
    ''' </summary>
    ''' <param name="n">The node which has been deleted</param>
    Protected Overridable Sub OnMemberDeleted(ByVal n As TreeNode)
        RaiseEvent MemberDeleted(DirectCast(n, ApplicationMemberNode).Member)
    End Sub

#End Region

#Region " Methods "

    ''' <summary>
    ''' Updates the name of the selected application member, ensuring that the source
    ''' node is updated if a filter is in place.
    ''' </summary>
    ''' <param name="name">The name to update the currently selected node to</param>
    Public Sub UpdateSelectedName(ByVal name As String)
        tvAppModel.UpdateText(tvAppModel.SelectedNode, name)
    End Sub

    ''' <summary>
    ''' Updates the name from the given application member.
    ''' </summary>
    ''' <param name="member">The member whose node needs updating to its new name.
    ''' </param>
    Public Sub UpdateName(ByVal member As clsApplicationMember)
        tvAppModel.UpdateText(GetNode(member), member.Name)
    End Sub

    ''' <summary>
    ''' Updates the enabled property of the buttons in the user interface.
    ''' </summary>
    Private Sub UpdateButtons()
        Dim n As TreeNode = tvAppModel.SelectedNode
        Select Case True
            Case n Is Nothing
                'no node selected, so unsafe to click buttons
                btnAddChild.Enabled = False
                btnAddElement.Enabled = False

            Case tvAppModel.Nodes(0) Is n
                'selected node is the parent node, no sibling elements should be added
                btnAddChild.Enabled = True
                btnAddElement.Enabled = False

            Case Else
                btnAddElement.Enabled = True
                btnAddChild.Enabled = True
        End Select
    End Sub

    ''' <summary>
    ''' Checks if the given application element represents the last selected element,
    ''' checking its ID against the saved last selected element.
    ''' </summary>
    ''' <param name="el">The element to check</param>
    ''' <returns>True if the given element is not null and has the same ID as the
    ''' last selected element.</returns>
    Private Function IsLastSelectedElement(ByVal el As clsApplicationElement) As Boolean
        Return el IsNot Nothing AndAlso el.ID = mLastSelectedElement
    End Function

    ''' <summary>
    ''' Loads the supplied application definition, if it is not null. Clears
    ''' any existing definition that may be being displayed.
    ''' </summary>
    ''' <param name="defn">The definition to load.</param>
    Public Sub LoadApplicationDefinition(ByVal defn As clsApplicationDefinition)
        LoadApplicationDefinition(defn, defn.RootApplicationElement)
    End Sub

    ''' <summary>
    ''' Loads the supplied application definition, if it is not null. Clears
    ''' any existing definition that may be being displayed. 
    ''' </summary>
    ''' <param name="defn">The definition to load.</param>
    ''' <param name="rootEl">The root element of the definition.</param>
    Public Sub LoadApplicationDefinition(
     ByVal defn As clsApplicationDefinition, ByVal rootEl As clsApplicationElement)
        mAppDefn = defn

        tvAppModel.BeginUpdate()
        tvAppModel.Nodes.Clear()

        Dim n As TreeNode = CreateNode(rootEl)
        tvAppModel.Nodes.Add(n)
        tvAppModel.ExpandAll()

        Dim selected As TreeNode = n
        If mLastSelectedElement <> Nothing Then
            Dim lastSelected As TreeNode =
             tvAppModel.FindNodeByTag(Of clsApplicationElement)(AddressOf IsLastSelectedElement)
            If lastSelected IsNot Nothing Then selected = lastSelected
        End If
        tvAppModel.SelectedNode = selected
        selected.EnsureVisible()

        tvAppModel.EndUpdate()
    End Sub

    ''' <summary>
    ''' Replaces the specified member within this app explorer with a new member
    ''' </summary>
    ''' <param name="member">The member which should be replaced.</param>
    ''' <param name="withMember">The member to replace it with.</param>
    Public Sub ReplaceMember(
     ByVal member As clsApplicationMember, ByVal withMember As clsApplicationMember)
        Dim n As ApplicationMemberNode = GetNode(member)
        If n Is Nothing Then Throw New NoSuchElementException(My.Resources.ctlApplicationExplorer_NoNodeFoundForTheGivenAppMember)
        tvAppModel.UpdateNode(n, AddressOf New MemberReplacer(withMember).ReplaceMemberAction)
    End Sub

    ''' <summary>
    ''' Deletes the given member from this app explorer.
    ''' </summary>
    ''' <param name="member">The member to delete.</param>
    Public Sub DeleteMember(ByVal member As clsApplicationMember)
        Dim n As ApplicationMemberNode = GetNode(member)
        If n Is Nothing Then Throw New NoSuchElementException(My.Resources.ctlApplicationExplorer_NoNodeFoundForTheGivenAppMember)
        tvAppModel.DeleteNode(n)
    End Sub

    ''' <summary>
    ''' Generates a string which is unique among the text appearing in the nodes 
    ''' of the treeview.
    ''' </summary>
    ''' <returns>A unique name for an element in the current tree.</returns>
    Private Function GenerateUniqueElementName() As String
        Dim names As ICollection(Of String) = AllNames
        Dim stem As String = My.Resources.Element
        Dim index As Integer = 1
        Do
            Dim name As String = stem & index
            If Not names.Contains(name) Then Return name
            index += 1
        Loop While index < Integer.MaxValue

        ' We've run out of all available integers, nothing for it - break out the guid
        Return My.Resources.Element & Guid.NewGuid().ToString()
    End Function

    ''' <summary>
    ''' Creates a treenode corresponding to the supplied application member
    ''' (recurses on member's children).
    ''' 
    ''' The member is stored in the node's tag for later retrieval.
    ''' </summary>
    ''' <param name="member">The member to create a treenode for.</param>
    ''' <returns>Returns a treenode corresponding to the supplied member.</returns>
    Private Function CreateNode(ByVal member As clsApplicationMember) As ApplicationMemberNode
        If member Is Nothing Then Return Nothing
        Return New ApplicationMemberNode(Me, member)
    End Function

    ''' <summary>
    ''' Creates a new treenode with a unique name.
    ''' </summary>
    ''' <param name="parent">The node to which the newly created
    ''' node should be added. Also used to deal with parent/child
    ''' relationship between associated members.</param>
    ''' <returns>The newly created node.</returns>
    Private Function CreateNewElementNode(ByVal parent As ApplicationMemberNode) As ApplicationMemberNode
        Dim gp As New clsApplicationElementGroup(GenerateUniqueElementName())
        parent.Member.AddMember(gp)
        Dim n As New ApplicationMemberNode(Me, gp)
        parent.Nodes.Add(n)
        n.EnsureVisible()
        Return n
    End Function

    ''' <summary>
    ''' Adds nodes representing the given collection of application members to the
    ''' node representing the given parent application member.
    ''' Note that this does not alter the model - ie. the children members are not
    ''' added to the parent member - this is expected to be done outseide of this
    ''' application explorer class - this method deals only with the treenodes which
    ''' represent the application members.
    ''' </summary>
    ''' <param name="parent">The parent application member to which the child nodes
    ''' need to be added.</param>
    ''' <param name="children">The collection of child application members for which
    ''' representative nodes are required</param>
    Public Sub AddChildren(
     ByVal parent As clsApplicationMember, ByVal children As ICollection(Of clsApplicationMember))

        Dim n As ApplicationMemberNode = GetNode(parent)
        Dim parentNodes As TreeNodeCollection = n.Nodes
        For Each child As clsApplicationMember In children
            parentNodes.Add(CreateNode(child))
        Next
        n.Expand()

    End Sub

    ''' <summary>
    ''' Gets the application member associated with the given treenode.
    ''' </summary>
    ''' <param name="n">The node for which the associated application member is
    ''' required.</param>
    ''' <returns>The application element associated with the given treenode or null
    ''' if the data associated with the node was not an application member or if
    ''' the treenode itself was null.</returns>
    Private Function GetApplicationMember(ByVal n As TreeNode) As clsApplicationMember
        If n Is Nothing Then Return Nothing
        Return DirectCast(n, ApplicationMemberNode).Member
    End Function

    ''' <summary>
    ''' Gets the application element associated with the given treenode.
    ''' </summary>
    ''' <param name="n">The node for which the associated application element is
    ''' required.</param>
    ''' <returns>The application element associated with the given treenode or null
    ''' if the data associated with the node was not an application element or if
    ''' the treenode itself was null.</returns>
    Private Function GetApplicationElement(ByVal n As TreeNode) As clsApplicationElement
        If n Is Nothing Then Return Nothing
        Return DirectCast(n, ApplicationMemberNode).Element
    End Function

    ''' <summary>
    ''' Gets the treenode representing the given application member.
    ''' </summary>
    ''' <param name="el">The application member to search for</param>
    ''' <returns>The node representing the given application member or null if there
    ''' is no such node in this application explorer</returns>
    Private Function GetNode(ByVal el As clsApplicationMember) As ApplicationMemberNode
        ' Little shortcut for handling if this is the currently selected member
        Dim n As ApplicationMemberNode = SelectedNode
        If n IsNot Nothing AndAlso n.Member Is el Then Return n
        Return DirectCast(tvAppModel.FindNodeWithTag(el), ApplicationMemberNode)
    End Function

    ''' <summary>
    ''' Gets the treenode representing the application member with the specified ID.
    ''' </summary>
    ''' <param name="id">The ID of the member for which the corresponding treenode
    ''' is required.</param>
    ''' <returns>The node representing the application member with the given ID or
    ''' null if no such node was found.</returns>
    Private Function GetNode(ByVal id As Guid) As ApplicationMemberNode
        ' Little shortcut for handling if this is the currently selected member
        Dim n As ApplicationMemberNode = SelectedNode
        If n IsNot Nothing AndAlso n.Member IsNot Nothing AndAlso n.Member.ID = id Then Return n
        Return DirectCast(
         tvAppModel.FindNodeByTag(Of clsApplicationMember)(AddressOf New IdMatcher(id).IsMatch),
         ApplicationMemberNode)
    End Function

    ''' <summary>
    ''' Predicate which returns true for any tree node
    ''' </summary>
    ''' <param name="node">The node to not care a fig about</param>
    ''' <returns>True</returns>
    Private Function Yes(ByVal node As TreeNode) As Boolean
        Return True
    End Function

    ''' <summary>
    ''' Performs the collapsing of all nodes in the treeview
    ''' </summary>
    Public Sub CollapseAll()
        'All nodes except the root node should be collapsed
        tvAppModel.BeginUpdate()
        If tvAppModel.Nodes.Count > 0 Then
            For Each n As TreeNode In tvAppModel.Nodes(0).Nodes
                n.Collapse(False)
            Next
        End If
        tvAppModel.EndUpdate()
    End Sub

#End Region

#Region " Button Event Handlers "

    ''' <summary>
    ''' Handles the Add Element button being pressed.
    ''' </summary>
    Private Sub HandleAddElement(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddElement.Click
        ' Get the selected node to use
        Dim selected As ApplicationMemberNode = SelectedNode

        ' If none is selected, use the first non-root node
        If selected Is Nothing Then _
         selected = TryCast(tvAppModel.Nodes(0).Nodes(0), ApplicationMemberNode)

        ' If there's still nothing there, just cancel
        If selected Is Nothing Then Return

        Dim n As TreeNode = CreateNewElementNode(selected.Parent)
        tvAppModel.SelectedNode = n
        OnMemberAdded(n)
    End Sub

    ''' <summary>
    ''' Handles the Add Child button being pressed.
    ''' </summary>
    Private Sub HandleAddChild(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddChild.Click
        Dim parent As ApplicationMemberNode = SelectedNode
        If parent Is Nothing Then UserMessage.Show(
          My.Resources.ctlApplicationExplorer_ToAddAChildYouMustFirstSelectAnApplicationElementToBeItsParent) : Return

        Dim n As TreeNode = CreateNewElementNode(parent)
        parent.UpdateAppearance()
        tvAppModel.SelectedNode = n
        OnMemberAdded(n)
    End Sub

#End Region

#Region " Filter Event Handlers "

    ''' <summary>
    ''' Filter class for searching the application model.
    ''' It would be a lot easier and nicer done inside an anonymous method, but
    ''' unfortunately VB doesn't get them until the future.
    ''' </summary>
    Private Class AppModelFilter
        ' The text to filter on
        Private mText As String

        ' The owner of this filter
        Private mRoot As clsApplicationMember

        ''' <summary>
        '''  Creates a new filter with the specified text.
        ''' </summary>
        ''' <param name="text">The text to filter on</param>
        Public Sub New(ByVal root As clsApplicationMember, ByVal text As String)
            mText = text
            mRoot = root
        End Sub
        ''' <summary>
        ''' Checks if the held text matches the given application member.
        ''' </summary>
        ''' <param name="member">The member to match the text on.</param>
        ''' <returns>True if the text supplied matches the given member, false
        ''' otherwise.</returns>
        Public Function Matches(ByVal member As clsApplicationMember) As Boolean
            Return member Is mRoot OrElse member.Matches(mText, True, False)
        End Function
    End Class

    ''' <summary>
    ''' Handles the filter being applied from the filter text box.
    ''' </summary>
    Private Sub HandleApplyFilter(ByVal sender As Object, ByVal e As FilterEventArgs) _
     Handles txtFilter.FilterTextChanged
        mIsFiltering = True
        Try
            Dim rootEl As clsApplicationMember = Nothing
            Try
                rootEl = GetApplicationMember(tvAppModel.Nodes(0))
            Catch
                ' Really shouldn't ever occur - there should always be a rootEl
                ' But just in case, there's no need for a crash here.
            End Try
            tvAppModel.FilterByTag(Of clsApplicationMember)(
             AddressOf New AppModelFilter(rootEl, e.FilterText).Matches)
        Finally
            mIsFiltering = False
        End Try
        panButtons.Enabled = False
    End Sub

    ''' <summary>
    ''' Handles the filter being cleared in the text box.
    ''' </summary>
    Private Sub HandleClearFilter(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtFilter.FilterCleared
        mIsFiltering = True
        Try
            tvAppModel.ClearFilter()
        Finally
            mIsFiltering = False
        End Try
        panButtons.Enabled = True
    End Sub

#End Region

#Region " Tree Structure Event Handlers "

    ''' <summary>
    ''' Handles a node being deleted, ensuring that the backing data is updated
    ''' with the new structure
    ''' </summary>
    Private Sub HandleNodeDeleted(ByVal e As TreeViewEditEventArgs) _
     Handles tvAppModel.NodeDeleted
        ' Ensure that the backing data is updated.
        With DirectCast(e.Node, ApplicationMemberNode)
            .RemoveFromContainer()
            .UpdateParentMember()
        End With

        ' Ensure that the parent's appearance changes as necessary
        DirectCast(e.SourceParentNode, ApplicationMemberNode).UpdateAppearance()

        ' Fire a NodeDeleted event.
        OnMemberDeleted(e.Node)
    End Sub

    ''' <summary>
    ''' Handles a node being copied, ensuring that the data model is updated with
    ''' the new structure.
    ''' </summary>
    Private Sub HandleNodeCopied(ByVal e As TreeViewEditEventArgs) Handles tvAppModel.NodeCopied
        ' Update the member in the node, ensuring that all child and parent members
        ' are similarly updated.
        DirectCast(e.Node, ApplicationMemberNode).ReplaceMemberWithCopy()

    End Sub

    ''' <summary>
    ''' Handles a node being moved, ensuring that the data model is updated with the
    ''' new structure
    ''' </summary>
    Private Sub HandleNodeMoved(ByVal e As TreeViewEditEventArgs) Handles tvAppModel.NodeMoved
        ' Ensure that the backing data is updated with the new relationship
        DirectCast(e.Node, ApplicationMemberNode).UpdateParentMember()
    End Sub

    ''' <summary>
    ''' Handles a node being deleted, ensuring that the root node is not deleted.
    ''' </summary>
    Private Sub HandleBeforeNodeDeleted(ByVal e As TreeViewCancelEditEventArgs) _
     Handles tvAppModel.NodeDeleting
        If e.Node.Parent Is Nothing Then
            'Must not allow deletion of root node
            e.Cancel = True
            e.DenialReason =
             My.Resources.ctlApplicationExplorer_TheApplicationItselfCannotBeDeletedOnlyItsChildElementsCanBeDeleted
        End If
    End Sub

    ''' <summary>
    ''' Handles a node being copied or moved, ensuring that nodes are not
    ''' transitioned into the root node.
    ''' </summary>
    Private Sub HandleBeforeNodeTransition(ByVal e As TreeViewCancelEditEventArgs) _
     Handles tvAppModel.BeforeNodeCopied, tvAppModel.BeforeNodeMoved
        'we don't allow people to add things alongside the root application element node
        e.Cancel = (e.TargetNodeCollection Is tvAppModel.Nodes)
        e.DenialReason =
         My.Resources.ctlApplicationExplorer_AllApplicationElementsMustBeChildrenOfTheApplicationItselfWhichResidesAtTheRoot
    End Sub

    ''' <summary>
    ''' Handles a node in the app model being selected
    ''' </summary>
    Private Sub HandleAfterSelect(ByVal sender As Object, ByVal e As TreeViewEventArgs) _
     Handles tvAppModel.AfterSelect
        Dim n As ApplicationMemberNode = DirectCast(e.Node, ApplicationMemberNode)
        'we should not allow sibling nodes to be added when root node is selected, etc
        UpdateButtons()
        Dim member As clsApplicationMember = n.Member
        If member IsNot Nothing Then
            OnMemberSelected(New ApplicationMemberEventArgs(member, mIsFiltering))
            mLastSelectedElement = member.ID
        End If
    End Sub

#End Region

#Region " Context Menu Handlers "

    ''' <summary>
    ''' Event handler for the BeforeShow context menu event of the treeview.
    ''' We use this to sneak in our extra menu item for diagnostics.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TreeView1_BeforeShowContextMenu(ByVal sender As Object, ByVal e As System.EventArgs) Handles tvAppModel.BeforeShowContextMenu
        Dim objContext As ContextMenuStrip = tvAppModel.ContextMenuStrip
        Dim objSelectedNode As TreeNode = tvAppModel.SelectedNode



        '"Insert New Element" options
        Dim InsertionParent As New ToolStripMenuItem(My.Resources.ctlApplicationExplorer_InsertNew, Nothing)
        objContext.Items.Insert(0, InsertionParent)
        Dim objDDEItem As ToolStripMenuItem = TryCast(InsertionParent.DropDownItems.Add(My.Resources.ctlApplicationExplorer_DDEElement, Nothing, AddressOf Me.InsertDDEElement), ToolStripMenuItem)
        objDDEItem.Enabled = (objSelectedNode IsNot Nothing) AndAlso Not [ReadOnly]     'only enabled when a node is selected
        objContext.Items.Insert(1, New ToolStripSeparator)

        'Add the copy and paste mask context menu titems
        Dim CopyMask As New ToolStripMenuItem(My.Resources.ctlApplicationExplorer_CopyElementMask, Nothing, AddressOf Me.CopyElementMask)
        objContext.Items.Insert(2, CopyMask)
        Dim PasteMask As New ToolStripMenuItem(My.Resources.ctlApplicationExplorer_PasteElementMask, Nothing, AddressOf Me.PasteElementMask)
        PasteMask.Enabled = (mElementMask IsNot Nothing)
        objContext.Items.Insert(3, PasteMask)
        objContext.Items.Insert(4, New ToolStripSeparator)

        'Insert snapshot option
        Dim objElement As clsApplicationElement = TryCast(objSelectedNode.Tag, clsApplicationElement)
        If Not objElement Is Nothing Then
            objContext.Items.Add(New ToolStripSeparator)

            Dim objItem As ToolStripMenuItem = TryCast(objContext.Items.Add(My.Resources.ctlApplicationExplorer_CreateSnapshotOnError, Nothing, AddressOf Me.ToggleDiagnostics), ToolStripMenuItem)
            objItem.Enabled = (mAppDefn.DiagnosticActions.Count <> 0)
            objItem.Checked = objElement.Diagnose

            'Insert Find references option
            objContext.Items.Add(New ToolStripSeparator)
            objItem = TryCast(objContext.Items.Add(My.Resources.ctlApplicationExplorer_FindReferences, Nothing, AddressOf Me.RequestFindReferences), ToolStripMenuItem)
            objItem.Enabled = Not [ReadOnly]
        End If

    End Sub


    ''' <summary>
    ''' Handles the event of the copy element mask operation
    ''' </summary>
    Private Sub CopyElementMask(ByVal sender As Object, ByVal e As EventArgs)
        If SelectedNode IsNot Nothing Then mElementMask = SelectedNode
    End Sub

    ''' <summary>
    ''' Handles the event for the paste element mask operation
    ''' </summary>
    Private Sub PasteElementMask(ByVal sender As Object, ByVal e As EventArgs)

        Dim currEl As clsApplicationElement = SelectedElement

        ' Not an element selected? Nothing to do
        If currEl Is Nothing Then Return

        Dim pastedEl As clsApplicationElement = mElementMask.Element

        ' Again, if what is pasted is not an element, little we can do
        If pastedEl Is Nothing Then Return

        Dim modified As Boolean = False
        For Each pastedAttr As clsApplicationAttribute In pastedEl.Attributes

            ' Get the attribute with the same name from the current element.
            Dim currAttr As clsApplicationAttribute = currEl.GetAttribute(pastedAttr.Name)
            ' If the target element doesn't have that attribute, skip it
            If currAttr Is Nothing Then Continue For

            ' Otherwise, update it accordingly
            currAttr.InUse = pastedAttr.InUse
            currAttr.Dynamic = pastedAttr.Dynamic
            currAttr.ComparisonType = pastedAttr.ComparisonType
            If Not pastedAttr.Dynamic Then
                Select Case pastedAttr.ComparisonType
                    Case clsAMI.ComparisonTypes.Equal, clsAMI.ComparisonTypes.NotEqual
                    Case Else
                        currAttr.Value = pastedAttr.Value.Clone()
                End Select
            End If
            modified = True

        Next

        ' Force a reload of the node's details if they have been changed
        If modified Then OnMemberSelected(New ApplicationMemberEventArgs(currEl, False))

    End Sub

    ''' <summary>
    ''' Event handler for the enable diagnostics context menu item.
    ''' </summary>
    Private Sub ToggleDiagnostics(ByVal sender As Object, ByVal e As EventArgs)
        SelectedNode.ToggleDiagnostics()
    End Sub

    ''' <summary>
    ''' Event handler for the Find References context menu item.
    ''' </summary>
    Private Sub RequestFindReferences(sender As Object, e As EventArgs)
        RaiseEvent FindReferences(SelectedElement())
    End Sub

#End Region

#Region " Overriding Methods "

    ''' <summary>
    ''' Disposes of this control, ensuring that any disposable resources are
    ''' disposed of too.
    ''' </summary>
    ''' <param name="explicit">True to indicate explicit disposing, False to indicate
    ''' disposing as a result of finalizers being run</param>
    Protected Overrides Sub Dispose(ByVal explicit As Boolean)
        Try
            If explicit Then
                If components IsNot Nothing Then components.Dispose() : components = Nothing
                If mRegularFont IsNot Nothing Then mRegularFont.Dispose() : mRegularFont = Nothing
            End If
        Finally
            MyBase.Dispose(explicit)
        End Try
    End Sub

#End Region

#Region " DDE "

    ''' <summary>
    ''' Inserts a DDE Element
    ''' </summary>
    Private Sub InsertDDEElement(ByVal sender As Object, ByVal e As EventArgs)
        Try
            'Choose a name for the new node
            Dim ddeNames As ICollection(Of String) = GetDDENames()
            Dim suffix As Integer = 0
            Dim name As String
            Do
                suffix += 1
                name = DDEPrefix & " " & suffix
            Loop While ddeNames.Contains(name)

            'Create and insert the new node
            Dim n As ApplicationMemberNode = SelectedNode

            ' No selected node - not a lot we can do
            If n Is Nothing Then Return

            ' Can't add to the root node - nothing to do
            If n.Parent Is Nothing Then
                UserMessage.Show(My.Resources.ctlApplicationExplorer_NewNodesCannotBeInsertedAtTheRootLevel)
                Return
            End If

            Const templateXml As String =
             "<element name='9'>" &
             " <type>DDEElement</type>" &
             "  <basetype>DDEElement</basetype>" &
             "  <id>b4f4f3b5-100d-44cb-a4de-f397d2486d05</id>" &
             "  <datatype>text</datatype>" &
             "  <diagnose>False</diagnose>" &
             "  <attributes>" &
             "     <attribute name='DDEServerName' dynamic='False' datatype='' inuse='True'>" &
             "       <ProcessValue>" &
             "         <datatype>text</datatype>" &
             "          <value></value>" &
             "         </ProcessValue>" &
             "      </attribute>" &
             "       <attribute name='DDETopicName' dynamic='False' datatype='' inuse='True'>" &
             "        <ProcessValue>" &
             "          <datatype>text</datatype>" &
             "          <value></value>" &
             "        </ProcessValue>" &
             "      </attribute>" &
             "      <attribute name='DDEItemName' dynamic='False' datatype='' inuse='True'>" &
             "        <ProcessValue>" &
             "          <datatype>text</datatype>" &
             "         <value></value>" &
             "        </ProcessValue>" &
             "      </attribute>" &
             "  </attributes>" &
             "</element>"

            'Create new application element based on template
            Dim xmldoc As New ReadableXmlDocument(templateXml)

            Dim xmlEl As XmlElement = DirectCast(xmldoc.FirstChild, XmlElement)
            Dim el As clsApplicationElement = DirectCast(
             clsApplicationMember.CreateFromXML(xmlEl), clsApplicationElement)


            el.ID = Guid.NewGuid
            el.Name = name

            'Create new node in tree
            Dim newNode As ApplicationMemberNode = CreateNode(el)

            'Insert the new element into its parent element
            n.Parent.Nodes.Insert(n.Index + 1, newNode)

            n.Parent.Member.AddMember(el)

            SelectedNode = newNode

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlApplicationExplorer_UnexpectedError0, ex.Message), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Gets the names of any elements prefixed with the DDE prefixed, assumed to
    ''' be DDE elements.
    ''' </summary>
    ''' <returns>A collection of names of DDE elements.</returns>
    Private Function GetDDENames() As ICollection(Of String)
        Dim ddeNames As New clsSet(Of String)
        For Each name As String In AllNames
            If name.StartsWith(DDEPrefix) Then ddeNames.Add(name)
        Next
        Return ddeNames
    End Function

#End Region

End Class
