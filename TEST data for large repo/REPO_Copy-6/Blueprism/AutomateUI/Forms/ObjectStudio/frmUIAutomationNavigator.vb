Imports BluePrism.AMI
Imports AutomateControls
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AMI.clsAMI
Imports BluePrism.UIAutomation
Imports BluePrism.BPCoreLib
Imports BluePrism.ApplicationManager
Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.ApplicationManager.UIAutomation
Imports AutomateControls.Trees
Imports BluePrism.Server.Domain.Models

''' <summary>
''' A form used to navigate over the UIAutomation tree for a process.
''' </summary>
Public Class frmUIAutomationNavigator
    Inherits frmForm

#Region " Events "

    ''' <summary>
    ''' Event fired when an element has been chosen in this form
    ''' </summary>
    Public Event ElementChosen As ElementChosenEventHandler

#End Region

#Region " Member Variables "

    ' The factory for creating various ui automation types
    Private ReadOnly mFactory As IAutomationFactory =
        AutomationTypeProvider.GetType(Of IAutomationFactory)

    ' The helper object to provide utility methods for UIA elements
    Private ReadOnly mHelper As IAutomationHelper =
        AutomationTypeProvider.GetType(Of IAutomationHelper)

    ' A helper which combines Automation utilities with BP Identifiers
    Private ReadOnly mIdHelper As IUIAutomationIdentifierHelper =
        New UIAutomationIdentifierHelper(mFactory, mHelper)

    ' The AMI instance that we are connected to
    Private WithEvents mAMI As clsAMI

    ' A highlighter window used to highlight the current element in the model tree
    Private mHighlighter As New HighlighterWindow()

    ' The treeview held by the treeview and filter control
    Private WithEvents tvElements As FilterableTreeView

    ' The width of the collapsed panel at the point it was collapsed
    Private mSavedPanelSize As Integer

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new UIAutomation navigator using the given AMI instance.
    ''' </summary>
    ''' <param name="ami">The AMI instance to use to access the target application.
    ''' </param>
    Public Sub New(ByVal ami As clsAMI)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mAMI = ami

        ' Set the filtering method to use in the filtering tree
        filTreeElements.Filterer = AddressOf IsNodeMatch

        ' Ensure we can capture the events in the tree
        tvElements = filTreeElements.Tree

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The currently selected element or null if nothing is selected.
    ''' </summary>
    Private Property SelectedAutomationElement As IAutomationElementFacade
        Get
            Return DirectCast(pgElement.SelectedObject, IAutomationElementFacade)
        End Get
        Set(value As IAutomationElementFacade)
            pgElement.SelectedObject = value
            HighlightElement(value)
        End Set
    End Property

    ''' <summary>
    ''' Gets the currently selected element from this navigator, or null if no
    ''' element is currently selected.
    ''' </summary>
    ''' <remarks>The element returned from this property will have a MatchIndex
    ''' identifier set with a value of "1"</remarks>
    Private ReadOnly Property SelectedElement As clsElement
        Get
            Return GetEquivalentElement(SelectedAutomationElement, True)
        End Get
    End Property

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Handles the loading of this form by setting the default view of the navigator
    ''' </summary>
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        'Set to be control view by default as this will load quicker than raw view
        radControl.Checked = True
    End Sub

    ''' <summary>
    ''' Handles the Refresh button being pressed
    ''' </summary>
    Private Sub HandleRefreshClick(sender As Object, e As EventArgs) _
     Handles btnRefresh.Click
        UpdateView()
    End Sub

    ''' <summary>
    ''' Handles a tree node being selected in the automation model treeview
    ''' </summary>
    Private Sub HandleTreeViewAfterSelect(sender As Object, e As TreeViewEventArgs) _
     Handles tvElements.AfterSelect
        SelectedAutomationElement = TryCast(e.Node.Tag, IAutomationElementFacade)
    End Sub

    ''' <summary>
    ''' Handles the OK button being pressed
    ''' </summary>
    Private Sub HandleOkClick(sender As Object, e As EventArgs) Handles btnOK.Click

        Dim el = SelectedAutomationElement

        If el Is Nothing Then
            UserMessage.Err(
                My.Resources.NoElementWasSelectedPleaseChooseAnElementOrClickCancel)
            Return
        End If

        Dim chosenEl As clsElement = Nothing

        If el IsNot Nothing Then
            Dim sb As New StringBuilder(1240)
            mIdHelper.AppendIdentifiers(el.Element, sb)
            chosenEl = mAMI.GetUIAutomationElement(sb.ToString())
        End If

        If chosenEl IsNot Nothing Then OnElementChosen(
            New ElementChosenEventArgs(GetSingleton.ICollection(chosenEl)))

        DialogResult = DialogResult.OK
        Close()
    End Sub

    ''' <summary>
    ''' Handles the Cancel button being clicked
    ''' </summary>
    Private Sub HandleCancelClick(sender As Object, e As EventArgs) _
     Handles btnCancel.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    ''' <summary>
    ''' Handles the form being closed by ensuring that the highlighter window is
    ''' hidden.
    ''' </summary>
    Protected Overrides Sub OnClosed(ByVal e As EventArgs)
        MyBase.OnClosed(e)
        ' Hide the highlighter window if it's there. Don't dispose just yet, however,
        ' the form might be reopened. It is disposed of in the Dispose() method.
        If mHighlighter IsNot Nothing Then mHighlighter.Visible = False
    End Sub

    ''' <summary>
    ''' Handles any of the 'View' radio buttons being selected.
    ''' </summary>
    Private Sub HandleUIAutomationViewChanged(sender As Object, e As EventArgs) _
     Handles radContent.CheckedChanged, radRaw.CheckedChanged, radControl.CheckedChanged
        If CType(sender, RadioButton).Checked Then UpdateView()
    End Sub

    ''' <summary>
    ''' Handles the collapse (/expand) toggle button being clicked
    ''' </summary>
    Private Sub HandleCollapse(sender As Object, e As EventArgs) _
     Handles btnCollapse.Click

        ' Don't perform any layout while we're messing around in here - it would
        ' judder all over the place if we let it.
        splitAppTree.SuspendLayout()

        Try
            ' If it's collapsed already, re-expand it to its former size; ensure
            ' that the current size of panel1 is maintained
            If splitAppTree.Panel2Collapsed Then
                ' Save the panel1 width
                Dim panel1Width As Integer = splitAppTree.Panel1.Width

                ' Increase the size of this form by the size of panel2 and the
                ' width of the splitter
                Width += mSavedPanelSize + splitAppTree.SplitterWidth
                ' Restore panel 2
                splitAppTree.Panel2Collapsed = False
                ' Restore the size of panel 1 to what it was before the change
                splitAppTree.SplitterDistance = panel1Width

                ' Toggle the direction of the arrow on the collapse toggle button
                btnCollapse.Direction = Direction.Left
                ttTip.SetToolTip(btnCollapse, My.Resources.frmUIAutomationNavigator_HideAttributes)

            Else ' It's not collapse, so we collapse it, saving its width first

                ' Store the width of the collapsing panel
                mSavedPanelSize = splitAppTree.Panel2.Width
                ' Reduce that width from the size of this form
                Width -= mSavedPanelSize + splitAppTree.SplitterWidth
                ' Collapse the panel
                splitAppTree.Panel2Collapsed = True

                ' Toggle the direction of the arrow on the collapse toggle button
                btnCollapse.Direction = Direction.Right
                ttTip.SetToolTip(btnCollapse, My.Resources.frmUIAutomationNavigator_ShowAttributes)

            End If

        Finally
            splitAppTree.ResumeLayout()

        End Try

    End Sub

    ''' <summary>
    ''' Raises the <see cref="ElementChosen"/> event
    ''' </summary>
    ''' <param name="e">The args to pass in the event</param>
    Protected Overridable Sub OnElementChosen(
     ByVal e As ElementChosenEventArgs)
        RaiseEvent ElementChosen(Me, e)
    End Sub

    Private Sub HandleAppClosed(applicationTypeInfo As clsApplicationTypeInfo,
                                applicationStatus As ApplicationStatus) Handles mAMI.ApplicationStatusChanged

        If IsDisposed OrElse Not IsHandleCreated Then Return

        If applicationStatus = clsAMI.ApplicationStatus.NotLaunched Then
            Invoke(AddressOf NotifyAppClosedAndClose)
        End If

    End Sub

    Private Sub NotifyAppClosedAndClose()
        UserMessage.Err(My.Resources.frmUIAutomationNavigator_AttachedApplicationClosed)
        Close()
    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Gets a <see cref="clsElement"/> equivalent of an Automation element.
    ''' </summary>
    ''' <param name="el">The automation element for which the equivalent element
    ''' instance is required</param>
    ''' <param name="includeMatchIndex">True to include a MatchIndex identifier with
    ''' a value of 1; False to just include the automation element-specific
    ''' identifiers in the returned element</param>
    ''' <returns>A <see cref="clsElement"/> containing the values provided by the
    ''' automation element, or null if <paramref name="el"/> was null.</returns>
    Private Function GetEquivalentElement(
     el As IAutomationElementFacade, includeMatchIndex As Boolean) As clsElement
        If el Is Nothing Then Return Nothing

        Dim sb As New StringBuilder(1240)
        If includeMatchIndex Then sb.Append("MatchIndex=1")
        mIdHelper.AppendIdentifiers(el.Element, sb)
        Return mAMI.GetUIAutomationElement(sb.ToString())
    End Function

    ''' <summary>
    ''' Tests to see if the given node matches the search term, testing its text
    ''' and then all of its identifier values to see if there is a value with the
    ''' given term within it (case insensitive)
    ''' </summary>
    ''' <param name="term">The search term to test with</param>
    ''' <param name="node">The node representing the element to be tested</param>
    ''' <returns>True if the given node is match for the search term; False
    ''' otherwise.</returns>
    Private Function IsNodeMatch(ByVal term As String, ByVal node As TreeNode) _
     As Boolean

        ' First test - the node text itself
        If BPUtil.IsMatch(node.Text, term, True, False) Then Return True

        ' Now we go through the properties and test them

        ' First get the element (skipping the MatchIndex identifier at this stage)
        Dim elem =
            GetEquivalentElement(TryCast(node.Tag, IAutomationElementFacade), False)
        If elem Is Nothing Then Return False

        ' Now test each of the properties in turn.
        For Each id As clsIdentifierInfo In elem.Identifiers.Values
            If BPUtil.IsMatch(id.Value, term, True, False) Then Return True
        Next

        Return False

    End Function

    ''' <summary>
    ''' Updates the treeview with the UIAutomation model
    ''' </summary>
    Private Sub UpdateView()
        ' Save the path of the current node, if one is selected so we can reselect
        ' it after the tree is refreshed
        Dim lastSelectedNodePath = tvElements.SelectedNode?.FullPath

        SelectedAutomationElement = Nothing
        tvElements.BeginUpdate()
        Try
            tvElements.Nodes.Clear()

            Dim root =
             mIdHelper.FindProcessWindowElements(mAMI.TargetPID).FirstOrDefault()

            If root Is Nothing Then Throw New NoSuchWindowException(
             My.Resources.NoWindowFoundForProcessWithPID0, mAMI.TargetPID)

            Dim parentNode = New TreeNode($"{root.CurrentName} ({root.CurrentLocalizedControlType})")
            parentNode.Tag = root
            WalkElements(root, parentNode, 0)
            filTreeElements.Tree.Nodes.Add(parentNode)
            filTreeElements.ReapplyFilter()
            tvElements.ExpandAll()

            Dim selNode As TreeNode = Nothing
            If lastSelectedNodePath IsNot Nothing Then _
            selNode = tvElements.FindNodeByPath(lastSelectedNodePath)

            selNode = If(selNode, tvElements.Nodes.Cast(Of TreeNode).FirstOrDefault())

            tvElements.SelectedNode = selNode
            tvElements.TopNode = selNode
        Catch ex As Exception
            UserMessage.Err(ex, ex.Message)
        Finally
            tvElements.EndUpdate()

        End Try

    End Sub

    ''' <summary>
    ''' Walks the UI Automation tree and adds the control type of each enabled
    ''' control element it finds to a TreeView.
    ''' </summary>
    ''' <param name="rootElement">The root of the search on this iteration.</param>
    ''' <param name="treeNode">The node in the TreeView for this iteration.</param>
    ''' <remarks>
    ''' This is a recursive function that maps out the structure of the subtree
    ''' beginning at the UI Automation element passed in as rootElement on the first
    ''' call. This could be, for example, an application window.
    ''' </remarks>
    Private Sub WalkElements(rootElement As IAutomationElement, treeNode As TreeNode, currentDepth As Integer)
        Dim walker As IAutomationTreeWalker = GetTreeWalker()
        Dim elem As IAutomationElement = walker.GetFirstChild(rootElement)

        While elem IsNot Nothing AndAlso currentDepth < 100
            currentDepth = currentDepth + 1
            Dim node As TreeNode = treeNode.Nodes.Add(String.Format(My.Resources.frmUIAutomationNavigator_WalkElements_01,
             elem.GetCurrentPropertyValue(PropertyType.Name),
             elem.GetCurrentPropertyValue(PropertyType.LocalizedControlType)))
            node.Tag = elem.Current
            WalkElements(elem, node, currentDepth)
            elem = walker.GetNextSibling(elem)
        End While
    End Sub

    ''' <summary>
    ''' Gets the treewalker corresponding to the current selection.
    ''' </summary>
    ''' <returns>An initialised tree walker which matches the type of walker
    ''' configured in the UI</returns>
    ''' <exception cref="InvalidStateException">If no tree walker type is selected
    ''' in this form</exception>
    Private Function GetTreeWalker() As IAutomationTreeWalker
        Select Case True
            Case radRaw.Checked : Return mFactory.CreateRawTreeWalker()
            Case radControl.Checked : Return mFactory.CreateControlTreeWalker()
            Case radContent.Checked : Return mFactory.CreateContentTreeWalker()
            Case Else
                Throw New InvalidStateException(My.Resources.NoViewSelected)
        End Select
    End Function

    ''' <summary>
    ''' Highlights the given element using its current bounding rectangle
    ''' </summary>
    ''' <param name="el">The element to highlight</param>
    Private Sub HighlightElement(el As IAutomationElementFacade)
        Dim nullableRect = el?.BoundingRectangle
        If Not nullableRect.HasValue OrElse nullableRect.Value = Nothing Then
            mHighlighter.Visible = False

        Else
            Dim bounds As Rectangle = nullableRect.Value
            mHighlighter.Visible = True
            mHighlighter.HighlightScreenRect = bounds

        End If
    End Sub

    ''' <summary>
    ''' Disposes of the automation navigator form
    ''' </summary>
    ''' <param name="disposing">True when called explicitly from an
    ''' <see cref="IDisposable.Dispose"/> call; False when called implicitly from a
    ''' finalizer.</param>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        MyBase.Dispose(disposing)
        If disposing Then
            filTreeElements = Nothing
            tvElements = Nothing
            If components IsNot Nothing Then components.Dispose()
            If mHighlighter IsNot Nothing Then mHighlighter.Dispose()
        End If
    End Sub

#End Region

End Class
