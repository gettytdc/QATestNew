
Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' Control to maintain a trail of 'breadcrumbs', ie labels and
''' corresponding tag objects which form a stack.
''' </summary>
Public Class ctlBreadcrumbTrail

#Region " Breadcrumb Class "

    ''' <summary>
    ''' Class in which the breadcrumbs are wrapped.
    ''' A breadcrumb represents a single entry in the trail - it consists of
    ''' a label and a tag, and can be represented by an arbitrary control -
    ''' usually a link label (for active / clickable crumbs) or a label (for
    ''' inactive / non-clickable ones).
    ''' </summary>
    Private Class Breadcrumb

        ' The parent crumb - ie the one prior to this in the stack.
        Private mParent As Breadcrumb

        ' The label for this crumb
        Private mLabel As String

        ' The tag object in this crumb
        Private mTag As Object

        ' The control used to display this crumb
        Private mControl As Control

        ''' <summary>
        ''' Creates a new breadcrumb, descendant from the given crumb, with
        ''' the given label and tag data.
        ''' </summary>
        ''' <param name="parent">The crumb prior to this one in the stack -
        ''' ie. its parent. Null for the root crumb.</param>
        ''' <param name="label">The label for this crumb</param>
        ''' <param name="tag">The data tag held against this crumb.</param>
        Public Sub New(ByVal parent As Breadcrumb, ByVal label As String, ByVal tag As Object)
            If label Is Nothing OrElse label.Trim().Length = 0 Then
                Throw New ArgumentException(
                 My.Resources.Breadcrumb_CannotCreateABreadcrumbWithAnEmptyLabel, label)
            End If
            mParent = parent
            mLabel = label
            mTag = tag
        End Sub

        ''' <summary>
        ''' The parent of this crumb, or null if it has no parent
        ''' </summary>
        Public ReadOnly Property Parent() As Breadcrumb
            Get
                Return mParent
            End Get
        End Property

        ''' <summary>
        ''' The label used to describe this crumb. Changing this will also
        ''' change the label on the control registered with the crumb if one
        ''' is registered.
        ''' </summary>
        Public Property Label() As String
            Get
                Return mLabel
            End Get
            Set(ByVal value As String)
                mLabel = value
                If mControl IsNot Nothing Then mControl.Text = value
            End Set
        End Property

        ''' <summary>
        ''' The data tag associated with this crumb
        ''' </summary>
        Public ReadOnly Property Tag() As Object
            Get
                Return mTag
            End Get
        End Property

        ''' <summary>
        ''' The control registered with this crumb.
        ''' </summary>
        Public Property Control() As Control
            Get
                Return mControl
            End Get
            Set(ByVal value As Control)
                mControl = value
            End Set
        End Property

        ''' <summary>
        ''' Gets a string representation of this crumb - this is just the 
        ''' currently set label
        ''' </summary>
        ''' <returns>A representation of this crumb in a string.</returns>
        Public Overrides Function ToString() As String
            Return mLabel
        End Function
    End Class

#End Region

#Region " Events "

    ''' <summary>
    ''' Event fired when a link label containing a breadcrumb is clicked.
    ''' </summary>
    ''' <param name="sender">The control responsible for the event, ie. this
    ''' BreadcrumbTrail control</param>
    ''' <param name="label">The label that was clicked</param>
    ''' <param name="tag">The tag associated with the crumb clicked</param>
    Public Event BreadcrumbClicked(
     ByVal sender As Object, ByVal label As String, ByVal tag As Object)

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty breadcrumb trail control
    ''' </summary>
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Controls are examples in the flow panel are examples for design mode only.
        ' ... and, of course, the DesignMode property isn't yet set in this object.
        If LicenseManager.UsageMode = LicenseUsageMode.Runtime Then
            flowPanel.Controls.Clear()
        End If

        ' Add any initialization after the InitializeComponent() call.

    End Sub

#End Region

#Region " Member Variables "

    ''' <summary>
    ''' Marker object to indicate 'no tag' when searching the stack for a
    ''' breadcrumb.
    ''' </summary>
    Private ReadOnly NoTagMarker As New Object()

    ''' <summary>
    ''' The stack in which the breadcrumbs are held.
    ''' </summary>
    Private mStack As New Stack(Of Breadcrumb)

#End Region

#Region " Properties "

    ''' <summary>
    ''' Property indicating if the breadcrumbs are wrapped or not.
    ''' </summary>
    Public Property Wrap() As Boolean
        Get
            Return flowPanel.WrapContents
        End Get
        Set(ByVal value As Boolean)
            flowPanel.WrapContents = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the number of crumbs in this breadcrumb trail.
    ''' </summary>
    Public ReadOnly Property Count() As Integer
        Get
            Return mStack.Count
        End Get
    End Property

    ''' <summary>
    ''' The label for the root breadcrumb in this trail - ie. the first
    ''' breadcrumb in the trail. Returns null if there are no crumbs in the
    ''' trail.
    ''' </summary>
    ''' <exception cref="IndexOutOfRangeException">If an attempt is made to set
    ''' the root label when the breadcrumb trail is empty.</exception>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property RootLabel() As String
        Get
            If mStack.Count = 0 Then Return Nothing
            Return Peek(mStack.Count - 1).Label
        End Get
        Set(ByVal value As String)
            If mStack.Count = 0 Then Throw New IndexOutOfRangeException(My.Resources.ctlBreadcrumbTrail_NoRootLabelToChange)
            Peek(mStack.Count - 1).Label = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the current top breadcrumb in the trail - ie. the latest entry
    ''' in the breadcrumb trail. Null if the trail is empty.
    ''' </summary>
    Private ReadOnly Property TopCrumb() As Breadcrumb
        Get
            If mStack.Count = 0 Then Return Nothing
            Return mStack.Peek
        End Get
    End Property

    ''' <summary>
    ''' Gets the current top label in the trail - ie. the latest entry in the
    ''' breadcrumb trail. Null if the trail is empty.
    ''' </summary>
    Public ReadOnly Property TopLabel() As String
        Get
            Dim bc As Breadcrumb = TopCrumb
            If bc Is Nothing Then Return Nothing Else Return bc.Label
        End Get
    End Property

    ''' <summary>
    ''' Gets the current tag object in the trail - ie. the latest entry in the
    ''' breadcrumb trail. Null if the trail is empty.
    ''' </summary>
    Public ReadOnly Property TopTag() As Object
        Get
            Dim bc As Breadcrumb = TopCrumb
            If bc Is Nothing Then Return Nothing Else Return bc.Tag
        End Get
    End Property

    ''' <summary>
    ''' The breadcrumbs in this trail in reverse order - ie. the last crumb
    ''' added to the trail will be the first element in the dictionary.
    ''' </summary>
    Public ReadOnly Property Crumbs() As IDictionary(Of String, Object)
        Get
            Dim map As New clsOrderedDictionary(Of String, Object)
            For Each crumb As Breadcrumb In mStack
                map(crumb.Label) = crumb.Tag
            Next
            Return map
        End Get
    End Property

#End Region

#Region " Breadcrumb Handling Methods "

    ''' <summary>
    ''' Push an entry onto the breadcrumb trail with the given label and no
    ''' tag data.
    ''' </summary>
    ''' <param name="label">The label to display for the breadcrumb trail entry
    ''' </param>
    Public Sub Push(ByVal label As String)
        Push(label, Nothing)
    End Sub

    ''' <summary>
    ''' Push an entry onto the trail with the given label and tag data.
    ''' </summary>
    ''' <param name="label">The label to display for the breadcrumb trail entry
    ''' </param>
    ''' <param name="tag">The tag data to associated with the breadcrumb trail
    ''' entry.</param>
    ''' <exception cref="ArgumentException">If the given label is null, empty
    ''' or only consists of whitespace.</exception>
    Public Sub Push(ByVal label As String, ByVal tag As Object)
        If String.IsNullOrWhiteSpace(label) Then
            Throw New ArgumentException(
             My.Resources.ctlBreadcrumbTrail_CannotAddAnEmptyBreadcrumbToABreadcrumbTrail, NameOf(label))
        End If

        ' Make the current top one a linklabel rather than a label
        Dim currTop As Breadcrumb = Me.TopCrumb
        If currTop IsNot Nothing Then
            Dim ctl As Control = GetControl(currTop)
            If TypeOf ctl Is Label Then
                flowPanel.Controls.Remove(ctl)
                flowPanel.Controls.Add(CreateLinkLabel(currTop))
            End If
        End If
        Dim crumb As Breadcrumb = New Breadcrumb(currTop, label, tag)
        mStack.Push(crumb)
        flowPanel.Controls.Add(CreateTextLabel(crumb))
    End Sub

    ''' <summary>
    ''' Pops the top entry from the breadcrumb trail and returns the label
    ''' </summary>
    ''' <returns>The label associated with the popped trail entry. This will
    ''' return null if the trail is currently empty.</returns>
    Public Function Pop() As KeyValuePair(Of String, Object)
        Return Pop(1).FirstOrDefault()
    End Function

    ''' <summary>
    ''' Pops the given count of entries from the top of the breadcrumb trail
    ''' and returns their labels. If the given number is 0 or less, this will
    ''' have no effect. If the given number is larger than the current size of
    ''' the trail, the trail will be cleared and all labels returned.
    ''' </summary>
    ''' <param name="count">The maximum number of entries to pop off the trail.
    ''' </param>
    ''' <returns>The list of labels to tags which were popped off, in stack 
    ''' order - ie. the last one added will be the first entry in the
    ''' dictionary</returns>
    Public Function Pop(ByVal count As Integer) As List(Of KeyValuePair(Of String, Object))

        Dim list As New List(Of KeyValuePair(Of String, Object))


        ' Nothing there or not really popping? Return empty
        If mStack.Count = 0 OrElse count <= 0 Then Return list

        Dim crumbs As New List(Of Breadcrumb)
        ' Keep going until we run out of count or run out of controls
        Dim currTop As Breadcrumb
        While count > 0
            currTop = mStack.Pop()
            flowPanel.Controls.Remove(GetControl(currTop))
            list.Add(New KeyValuePair(Of String, Object)(currTop.Label, currTop.Tag))
            count -= 1
        End While

        ' If we still have data, make the top crumb a label rather than a link
        If mStack.Count > 0 Then
            currTop = mStack.Peek()
            flowPanel.Controls.Remove(GetControl(currTop))
            flowPanel.Controls.Add(CreateTextLabel(currTop))
        End If

        Return list

    End Function

    ''' <summary>
    ''' Pops all the top crumbs off the stack until the first element with the
    ''' given label and the same tag object is found.
    ''' This will have no effect if the given label and tag combination could
    ''' not be found in the breadcrumb trail.
    ''' </summary>
    ''' <param name="label">The label to search for.</param>
    ''' <param name="tag">The tag to search for - note that it will search
    ''' for the same reference, not just equality.</param>
    ''' <returns>The collection of strings that have been popped off the stack
    ''' </returns>
    Public Function PopTo(ByVal label As String, ByVal tag As Object) As List(Of KeyValuePair(Of String, Object))
        Return Pop(FindInStack(label, tag))
    End Function

    ''' <summary>
    ''' Gets the crumb found <i>index</i> levels from the top of the stack.
    ''' </summary>
    ''' <param name="index">The number of elements from the top of the stack
    ''' to descend, such that <i>0</i> indicates the current top level of the
    ''' stack and <i>Count - 1</i> represents the root of the stack.</param>
    ''' <returns></returns>
    Private Function Peek(ByVal index As Integer) As Breadcrumb
        Dim curr As Integer = 0
        For Each crumb As Breadcrumb In mStack
            If curr = index Then Return crumb
            curr += 1
        Next
        Throw New IndexOutOfRangeException(String.Format(
         My.Resources.ctlBreadcrumbTrail_TriedToPeekToIndex0Only1CrumbsInTheStack, index, mStack.Count))
    End Function

#End Region

#Region " Internal Utility Methods "

    ''' <summary>
    ''' Finds the first instance of the given label from the top of the stack.
    ''' Returns an integer indicating how many elements down the stack were
    ''' traversed to reach an element with the given label, where 0 indicates
    ''' that the label was at the top of the stack, and -1 indicates that the
    ''' label wasn't found on the stack.
    ''' </summary>
    ''' <param name="label">The label to search for.</param>
    ''' <param name="tag">The tag to search for - if this is
    ''' <see cref="NoTagMarker"/> then the tags will not be checked. Note that
    ''' passing in null / Nothing, will search for a crumb with the given label
    ''' <em>and</em> a tag of null.</param>
    ''' <returns>The number of elements traversed from the top of the stack
    ''' before the given label was found. -1 if the given label was not found
    ''' on the stack at all.</returns>
    Private Function FindInStack(ByVal label As String, ByVal tag As Object) As Integer
        ' The stack enumerates from the top of the stack, though you'd be hard
        ' pressed to find that in the documentation...
        Dim i As Integer = 0
        For Each crumb As Breadcrumb In mStack
            If crumb.Label = label AndAlso (tag Is NoTagMarker OrElse tag Is crumb.Tag) Then
                Return i
            End If
            i += 1
        Next
        Return -1
    End Function

    ''' <summary>
    ''' Gets the control corresponding to the given breadcrumb.
    ''' </summary>
    ''' <param name="crumb">The crumb for which the control is required.
    ''' </param>
    ''' <returns>The control relating to the given crumb - either a 
    ''' <see cref="Label"/> or a <see cref="LinkLabel"/> object. </returns>
    Private Function GetControl(ByVal crumb As Breadcrumb) As Control
        ' It *should* be registered in the control
        If crumb.Control IsNot Nothing Then Return crumb.Control
        ' If not, just sanity check that it's not been added the 'old' way
        For Each ctl As Control In flowPanel.Controls
            If ctl.Text = crumb.Label Then Return ctl
        Next
        ' Nothing there? Return nothing.
        Return Nothing
    End Function

    ''' <summary>
    ''' Creates a link label for the given crumb and registers it.
    ''' </summary>
    ''' <param name="crumb">The breadcrumb for which a link label is required.
    ''' </param>
    ''' <returns>The newly generated link label, initialised and registered
    ''' with the given breadcrumb.</returns>
    Private Function CreateLinkLabel(ByVal crumb As Breadcrumb) As LinkLabel
        Dim ll As New LinkLabel()
        ll.Text = crumb.Label
        ll.Tag = crumb
        ll.AutoSize = True
        AddHandler ll.Click, AddressOf HandleLinkClicked
        crumb.Control = ll
        Return ll
    End Function

    ''' <summary>
    ''' Creates a text label for the given crumb and registers it.
    ''' </summary>
    ''' <param name="crumb">The breadcrumb for which a label is required.
    ''' </param>
    ''' <returns>The newly generated label, initialised and registered with the
    ''' given breadcrumb.</returns>
    Private Function CreateTextLabel(ByVal crumb As Breadcrumb) As Label
        Dim l As New Label()
        l.Text = crumb.Label
        l.Tag = crumb
        l.AutoSize = True
        crumb.Control = l
        Return l
    End Function

#End Region

#Region " Internal Event Handlers "

    ''' <summary>
    ''' Handles a link label being clicked. This just passes on the event in
    ''' the form of a <see cref="BreadcrumbClicked"/> clicked event.
    ''' </summary>
    Private Sub HandleLinkClicked(ByVal sender As Object, ByVal e As EventArgs)
        Dim ll As LinkLabel = TryCast(sender, LinkLabel)
        If ll Is Nothing Then Return
        RaiseEvent BreadcrumbClicked(Me, ll.Text, DirectCast(ll.Tag, Breadcrumb).Tag)
    End Sub

#End Region

End Class
