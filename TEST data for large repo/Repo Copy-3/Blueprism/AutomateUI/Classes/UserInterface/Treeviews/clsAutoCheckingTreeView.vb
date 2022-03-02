Imports AutomateControls.Trees

''' Project  : Automate
''' Class    : clsAutoCheckingTreeView
''' 
''' <summary>
''' Slight extension to the treeview class. Makes nodes (un)tick the checkboxes
''' of their children when they are (un)ticked, and (un)ticks the parent node
''' when a node and all siblings are (un)ticked.
''' </summary>
Public Class clsAutoCheckingTreeView
    Inherits TreeView

#Region " Members "

    ' The tool tip object. The tag is a flag indicating if it is enabled or not
    Private mToolTip As ToolTip

#End Region

#Region " Constructor "

    Public Sub New()
        MyBase.CheckBoxes = True

        mToolTip = New ToolTip()
        With mToolTip
            .AutoPopDelay = 3000
            .InitialDelay = 500
            .ReshowDelay = 500
            .ShowAlways = True
            .Tag = False ' Default to 'disabled'
        End With

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Checks if any leaves are checked in this treeview
    ''' </summary>
    Public ReadOnly Property IsAnyLeafChecked() As Boolean
        Get
            For Each n As TreeNode In New TreeNodeEnumerable(Me)
                If n.Checked AndAlso n.Nodes.Count = 0 Then Return True
            Next
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether a tooltip should be shown when a node is hovered on.
    ''' </summary>
    ''' <value></value>
    Public Property UseToolTips() As Boolean
        Get
            Return CBool(mToolTip.Tag)
        End Get
        Set(ByVal value As Boolean)
            mToolTip.Tag = value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Disposes of this treeview
    ''' </summary>
    ''' <param name="disposing">True to indicate that this Dispose() has been called
    ''' explicitly; False if being called by the garbage collector via a destructor 
    ''' </param>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        MyBase.Dispose(disposing)
        If Not IsDisposed AndAlso disposing Then mToolTip.Dispose()
    End Sub

    ''' <summary>
    ''' Sets the checked() property of the node to the specified value, and
    ''' propagates this value to the node's children.
    ''' </summary>
    ''' <param name="node">The node to be changed.</param>
    ''' <param name="checked">The new value to be placed into the Checked() property.
    ''' </param>
    Public Sub RecursivelySetNodesChecked( _
     ByVal node As TreeNode, ByVal checked As Boolean)
        BeginUpdate()
        If Not node Is Nothing Then
            node.Checked = checked
            For Each n As TreeNode In node.Nodes
                RecursivelySetNodesChecked(n, checked)
            Next
        End If
        EndUpdate()
    End Sub

    ''' <summary>
    ''' Sets the appropriate check status in the children of that supplied.
    ''' </summary>
    ''' <param name="node">The node whose children are to be updated.</param>
    ''' <param name="checked">The new check status</param>
    Private Sub CheckChildren(ByVal node As TreeNode, ByVal checked As Boolean)
        For Each child As TreeNode In node.Nodes
            If child.Checked <> checked Then
                'The child checked status needs changing
                child.Checked = checked
                'Recurse to the next level
                If child.Nodes.Count > 0 Then CheckChildren(child, checked)
            End If
        Next
    End Sub

    ''' <summary>
    ''' Sets the appropriate check status in the parent node of that supplied.
    ''' Does nothing if the supplied node has no parent.
    ''' </summary>
    ''' <param name="node">The node whose parent is to be updated.</param>
    Private Sub CheckParent(ByVal node As TreeNode)
        Dim bAllNodesTheSame As Boolean = True
        Dim parent As TreeNode = node.Parent
        If parent Is Nothing Then Exit Sub

        For Each n As TreeNode In parent.Nodes
            If Not n.Checked = node.Checked Then
                bAllNodesTheSame = False
                Exit For
            End If
        Next

        If Not bAllNodesTheSame Then
            parent.Checked = False
        Else
            parent.Checked = node.Checked
        End If

        CheckParent(parent)

    End Sub

    Protected Overrides Sub WndProc(ByRef m As Message)

        'Change double click to single click
        If m.Msg = &H203 Then _
            m.Msg = &H201

        MyBase.WndProc(m)
    End Sub

    Public Sub ClearSelection()
        For Each node As TreeNode In Nodes
            UncheckNodeAndChildren(node)
        Next
    End Sub

    Private Sub UncheckNodeAndChildren(node As TreeNode)
        node.Checked = False

        For Each child As TreeNode In node.Nodes
            UncheckNodeAndChildren(child)
        Next
    End Sub
#End Region

#Region " Event Code "

    Protected Overrides Sub OnAfterCheck(ByVal e As TreeViewEventArgs)
        'Don't act on events not raised by user actions
        If e.Action <> TreeViewAction.Unknown Then
            If e.Node.Nodes.Count > 0 Then CheckChildren(e.Node, e.Node.Checked)
            CheckParent(e.Node)
        End If
        MyBase.OnAfterCheck(e)

    End Sub

    Protected Overrides Sub OnAfterSelect(ByVal e As TreeViewEventArgs)
        If UseToolTips Then mToolTip.SetToolTip(Me, e.Node.ToolTipText)
        MyBase.OnAfterSelect(e)
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        Me.ResumeLayout(False)

    End Sub

#End Region

End Class
