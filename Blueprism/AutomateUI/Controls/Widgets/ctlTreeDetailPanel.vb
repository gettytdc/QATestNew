
Imports AutomateControls
Imports BluePrism.Images

''' <summary>
''' Panel to hold a treeview against a corresponding detail panel.
''' </summary>
Public Class ctlTreeDetailPanel

    ''' <summary>
    ''' Creates a new empty tree detail panel
    ''' </summary>
    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        Me.ImageList = ImageLists.Components_16x16
    End Sub

    Public Property ImageList() As ImageList
        Get
            Return treeMain.ImageList
        End Get
        Set(ByVal value As ImageList)
            treeMain.ImageList = value
        End Set
    End Property

    Protected Overridable ReadOnly Property MainTree() As TreeView
        Get
            Return treeMain
        End Get
    End Property

    Protected Overridable Property CurrentDetailPanel() As Control
        Get
            Dim ctl As Control = Nothing
            If splitPane.Panel2.Controls.Count > 0 Then
                ctl = splitPane.Panel2.Controls(0)
            End If
            Return ctl
        End Get
        Set(ByVal value As Control)

            ' If this is what's already there, then leave it there...
            If value Is CurrentDetailPanel Then Return

            WinUtil.TrySuspendDrawing(splitPane)
            Try
                With splitPane.Panel2.Controls
                    .Clear()
                    If value IsNot Nothing Then
                        value.Dock = DockStyle.Fill
                        .Add(value)
                    End If
                End With

            Finally
                WinUtil.TryResumeDrawing(splitPane)

            End Try

        End Set
    End Property

    Protected Overridable Function FindAllElements(Of T)(ByVal coll As ICollection(Of T)) As ICollection(Of T)
        Return FindAllElements(treeMain.Nodes, coll)
    End Function

    Protected Overridable Function FindAllElements(Of T)( _
     ByVal nodes As TreeNodeCollection, ByVal coll As ICollection(Of T)) _
     As ICollection(Of T)
        If coll Is Nothing Then coll = New List(Of T)
        For Each n As TreeNode In nodes
            If TypeOf n.Tag Is T Then coll.Add(DirectCast(n.Tag, T))
            If n.Nodes.Count > 0 Then FindAllElements(n.Nodes, coll)
        Next
        Return coll
    End Function

    Protected Overridable Function AddNode(ByVal name As String, ByVal imageKey As String) As TreeNode
        Return AddNode(treeMain.Nodes, name, imageKey)
    End Function

    Protected Overridable Function AddNode( _
     ByVal nodes As TreeNodeCollection, ByVal name As String, ByVal imageKey As String) As TreeNode
        Return nodes.Add(name, name, imageKey, imageKey)
    End Function

    Protected Overridable Function FindNode(ByVal tag As Object) As TreeNode
        For Each n As TreeNode In treeMain.Nodes
            Dim found As TreeNode = FindNode(n, tag)
            If found IsNot Nothing Then Return found
        Next
        Return Nothing
    End Function

    Protected Overridable Function FindNode(ByVal n As TreeNode, ByVal tag As Object) As TreeNode
        If n.Tag Is tag Then Return n
        For Each innerNode As TreeNode In n.Nodes
            Dim found As TreeNode = FindNode(innerNode, tag)
            If found IsNot Nothing Then Return found
        Next
        Return Nothing
    End Function

    Protected Overridable Function SelectNode(ByVal tag As Object) As TreeNode
        Dim n As TreeNode = FindNode(tag)
        If n IsNot Nothing Then treeMain.SelectedNode = n
        Return n
    End Function

    Protected Overridable Function SelectNode(ByVal root As TreeNode, ByVal tag As Object) As TreeNode
        Dim n As TreeNode = FindNode(root, tag)
        If n IsNot Nothing Then treeMain.SelectedNode = n
        Return n
    End Function

    Protected Overridable Sub ShowDetailForNode(ByVal n As TreeNode)
        ' Can't use an abstract method / class due to the way the designer works.
        Throw New NotImplementedException( _
         "Subclasses of ctlTreeDetailPanel must implement ShowDetailForNode(TreeNode)")
    End Sub

    Private Sub HandleSelect(ByVal sender As Object, ByVal e As TreeViewEventArgs) _
     Handles treeMain.AfterSelect
        ShowDetailForNode(e.Node)
    End Sub


End Class



