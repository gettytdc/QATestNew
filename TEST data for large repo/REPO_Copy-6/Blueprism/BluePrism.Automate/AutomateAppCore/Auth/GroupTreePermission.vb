Imports System.Runtime.Serialization

Namespace Auth

    ''' <summary>
    ''' A permission applied to a Group Tree member
    ''' </summary>
    <DataContract([Namespace]:="bp"), Serializable>
    Public Class GroupTreePermission

        Public Sub New(perm As Permission, permType As GroupPermissionLevel)
            Me.Perm = perm
            PermissionType = permType
        End Sub

        <DataMember>
        Public Property Perm As Permission

        <DataMember>
        Public Property PermissionType As GroupPermissionLevel

    End Class

End Namespace
