Imports BluePrism.AutomateAppCore.Groups
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports BluePrism.Core.Utility

Public Module TreeviewExtensionMethods
    <Extension()>
    Public Function ReturnResourceName(root As IGroup, resourceStatus As ResourceStatus) As IEnumerable(Of String)

        Dim strings = root.Where(Function(y) TypeOf y Is ResourceGroupMember).
                           Cast(Of ResourceGroupMember).
                           Where(Function(r) r.Status = resourceStatus).
                           Select(Function(t) t.Name).ToList()

        For Each innerGroup In root.Where(Function(x) TypeOf x Is IGroup).Cast(Of IGroup)()
            strings.AddRange(innerGroup.ReturnResourceName(resourceStatus))
        Next
        Return strings
    End Function
End Module
