Namespace Controls.Widgets.ReleaseManager

    ''' <summary>
    ''' Implemented by types derived from TreeView that manage removal of its own nodes
    ''' </summary>
    Public Interface IManageNodeRemoval
    
        ''' <summary>
        ''' Removes a node from the current TreeView
        ''' </summary>
        ''' <param name="node">The node to remove</param>
        Sub RemoveNode(node As TreeNode)

    End Interface
End NameSpace