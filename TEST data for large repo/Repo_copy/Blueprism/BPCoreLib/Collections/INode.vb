Namespace Collections

    Public Interface INode(Of T)

        ReadOnly Property Children() As ICollection(Of INode(Of T))

        Function Add(ByVal value As T) As INode(Of T)

        Function Remove(ByVal value As T) As INode(Of T)

        Function Clone() As INode(Of T)

        Function Find(ByVal value As T) As INode(Of T)

        Sub [Each](ByVal d As [Delegate], ByVal breadthFirst As Boolean)

    End Interface

End Namespace