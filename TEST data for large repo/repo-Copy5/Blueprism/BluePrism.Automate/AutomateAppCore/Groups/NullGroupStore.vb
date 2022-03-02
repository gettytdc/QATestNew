

Imports BluePrism.AutomateProcessCore

Namespace Groups

    ''' <summary>
    ''' Group store implementation which retrieves nothing, updates nothing and just
    ''' exists to allow an in-place object where a store would otherwise be, meaning
    ''' that we don't need unnecessary null checks throughout the code.
    ''' </summary>
    Public Class NullGroupStore : Implements IGroupStore

        ''' <summary>
        ''' The single instance necessary of a null group store
        ''' </summary>
        Public Shared ReadOnly Instance As New NullGroupStore()

        Public Property Server As IServer Implements IGroupStore.Server

        ''' <summary>
        ''' Returns a new tree of the given type. Note that this tree will have no
        ''' entries in it at all (and, incidentally, no filtering) beyond the root
        ''' group
        ''' </summary>
        Public Function GetTree(tp As GroupTreeType, filter As Predicate(Of IGroupMember),
                         groupFilter As Predicate(Of IGroup), fullReload As Boolean,
                         poolsAsGroups As Boolean, getRetired As Boolean) As IGroupTree _
         Implements IGroupStore.GetTree
            Return New GroupTree(tp)
        End Function

        ''' <summary>
        ''' No effect
        ''' </summary>
        Sub Update(mem As IGroup) Implements IGroupStore.Update
        End Sub

        ''' <summary>
        ''' No effect
        ''' </summary>
        Sub Dispose() Implements IDisposable.Dispose
        End Sub

        ''' <summary>
        ''' No effect
        ''' </summary>
        Sub AddTo(gp As IGroup, mem As IGroupMember) Implements IGroupStore.AddTo
        End Sub

        ''' <summary>
        ''' No effect
        ''' </summary>
        Sub RemoveFrom(gp As IGroup, mem As IGroupMember) Implements IGroupStore.RemoveFrom
        End Sub

        ''' <summary>
        ''' No effect
        ''' </summary>
        Sub MoveTo(
                 fromGp As IGroup, toGp As IGroup, ByRef mem As IGroupMember, isCopy As Boolean) _
                 Implements IGroupStore.MoveTo
        End Sub

        ''' <summary>
        ''' No effect
        ''' </summary>
        Sub AddTo(gp As IGroup, mem As IEnumerable(Of IGroupMember)) Implements IGroupStore.AddTo
        End Sub

        ''' <summary>
        ''' No effect
        ''' </summary>
        Sub RemoveFrom(gp As IGroup, mem As IEnumerable(Of IGroupMember)) Implements IGroupStore.RemoveFrom
        End Sub

        ''' <summary>
        ''' No effect
        ''' </summary>
        Sub DeleteGroup(group As IGroup) Implements IGroupStore.DeleteGroup
        End Sub

        ''' <summary>
        ''' No effect
        ''' </summary>
        Public Sub Refresh(tree As IGroupTree) Implements IGroupStore.Refresh
        End Sub

        ''' <summary>
        ''' No effect
        ''' </summary>
        Public Function GetPermissionsState(gp As IGroup) As PermissionState Implements IGroupStore.GetPermissionsState
            Return PermissionState.Unknown
        End Function
        ''' <summary>
        ''' Return empty object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        Public Function GetGroup(id As Guid) As IGroup Implements IGroupStore.GetGroup 
            Return New Group
        End Function
    End Class
End Namespace
