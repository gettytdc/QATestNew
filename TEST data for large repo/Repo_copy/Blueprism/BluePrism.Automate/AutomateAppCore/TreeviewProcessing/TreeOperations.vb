Imports System.Runtime.CompilerServices
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.Core.Utility

Namespace TreeviewProcessing

    Public Module TreeOperations

        <Extension()>
        Public Function FindById(root As IGroup, id As Guid) As IGroupMember

            'First pass, just look for the id of the element we are interested in.
            Dim item = root.FirstOrDefault(Function(r) r.IdAsGuid() = id)

            If item IsNot Nothing Then
                Return item
            End If

            Dim innerGroups = root.Where(Function(x) TypeOf x Is IGroup).Cast(Of IGroup)()

            For Each innerGroup In innerGroups
                Dim result = FindById(innerGroup, id)
                If result IsNot Nothing Then
                    Return result
                End If
            Next

            Return Nothing
        End Function

        <Extension()>
        Public Function FlattenGroups(e As IGroup) As IEnumerable(Of IGroup)
            Return e.Where(Function(t) t.IsGroup).Cast(Of IGroup).Traverse(Function(t) t.Where(Function(q) q.IsGroup).Cast(Of IGroup).AsEnumerable())
        End Function

        <Extension()>
        Public Function ReturnName(root As IGroup, resourceStatus As ResourceStatus) As IEnumerable(Of String)

            Dim strings = root.Where(Function(y) TypeOf y Is ResourceGroupMember).
                    Cast(Of ResourceGroupMember).
                    Where(Function(r) r.Status = resourceStatus).
                    Select(Function(t) t.Name).ToList()

            For Each innerGroup In root.Where(Function(x) TypeOf x Is IGroup).Cast(Of IGroup)()
                strings.AddRange(innerGroup.ReturnName(resourceStatus))
            Next
            Return strings
        End Function
    End Module
End Namespace
